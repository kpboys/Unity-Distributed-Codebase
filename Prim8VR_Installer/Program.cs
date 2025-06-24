using Newtonsoft.Json;
using Prim8VR_Installer.Behaviors;
using Prim8VR_Installer.Behaviors.DevOnly;
using Prim8VR_Installer.Helpers;
using Prim8VR_Installer.JsonObjects;
using Prim8VR_Installer.Menus;
using Prim8VR_Installer.Menus.MenuTypes;
using SubWindowSystem;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Prim8VR_Installer
{
    public enum DebugMessageFilter
    {
        Verbose = 0,
        Debug = 1,
        Info = 2,
        Error = 3,
        Silent = 4
    }
    internal class Program
    {
        public static SubWindow debugWindow;
        public static SubWindowWithListSelection startMenu;
        private static SubWindow errorPopup;
        private static LoadingBar loadBar;
        //Debug stuff
        private static bool debugRunning;
        private static bool debugSenderActive;
        private static Queue<DebugMessage> debugMessageQueue = new Queue<DebugMessage>();
        private static DebugMessageFilter currentMessageFilter = DebugMessageFilter.Verbose; //Change if you want
        private static readonly string debugModeArgument = "DebugMode";
        private static StreamWriter sendStream;
        private static TcpClient senderClient;
        static void Main(string[] args)
        {
            if(args.Length != 0)
            {
                Console.WriteLine(args[0]);
                if (args[0] == debugModeArgument)
                {
                    DebugWindowFlow();
                }
            }
            else
            {
                StandardFlow();
            }
            Console.ReadKey(true);
        }
        private static void StandardFlow()
        {
            Console.WriteLine("Standard flow going");

            //Setup popups
            errorPopup = new SubWindow(new System.Drawing.Rectangle(30, 10, 36, 5), '=', 1, false);
            loadBar = new LoadingBar(new System.Drawing.Rectangle(30, 18, 10, 1), '=', 0, false)
                .SetupBarDesign('#', '.');

            SetupDebugSender();

            //Start Inpurt helper
            InputHelper.Start();
            InputHelper.AddListener(ConsoleKey.S, ConsoleModifiers.Alt, () => SetupDebugSender());

            //Get General Config
            GeneralConfig config = JsonConvert.DeserializeObject<GeneralConfig>(File.ReadAllText("config/general_config.json"));
            if (config == null)
            {
                Program.WriteInDebug("Couldn't find General Config!");
                return;
            }
            //Setup MenuHandler
            MenuHandler menuHandler = new MenuHandler();

            //Welcome menu
            WelcomeMenu welcomeMenu = new WelcomeMenu(new BasicMenuLayout());
            menuHandler.AddStateMenuBinding(MenuHandler.MenuStates.Welcome, welcomeMenu);

            //Main menu setup
            ListSelectionMenuLayout mainMenuLayout = new ListSelectionMenuLayout();
            mainMenuLayout.AddOption("Select and install releases",
                () => { MenuHandler.Instance.ChangeState(MenuHandler.MenuStates.CheckDevice); });
            mainMenuLayout.AddOption("Add Project link",
                () => { MenuHandler.Instance.ChangeState(MenuHandler.MenuStates.AddProject); });
            mainMenuLayout.AddOption("Add Bundle link",
                () => { MenuHandler.Instance.ChangeState(MenuHandler.MenuStates.AddBundle); });
            mainMenuLayout.AddSpacer();
            mainMenuLayout.AddOption("Exit", 
                () => { MenuHandler.Instance.Stop(); });
            MainMenu mainMenu = new MainMenu(mainMenuLayout);
            menuHandler.AddStateMenuBinding(MenuHandler.MenuStates.MainMenu, mainMenu);

            //Set up device check step
            GetDevice deviceGetter = new GetDevice(config);
            BasicMenuLayout deviceMenuLayout = new BasicMenuLayout();
            CheckDeviceMenu deviceMenu = new CheckDeviceMenu(deviceMenuLayout, deviceGetter);
            menuHandler.AddStateMenuBinding(MenuHandler.MenuStates.CheckDevice, deviceMenu);

            //Set up build selection
            //DevOnly BuildGatherers
            FileBasedBuildGatherer fileGatherer = new FileBasedBuildGatherer();
            fileGatherer.ManuallyAddBuilds(
                "C:\\Users\\peter\\Desktop\\PB Stuff\\Bachelor\\Builds\\VeryDifferent_Test_1.0.apk",
                "C:\\Users\\peter\\Desktop\\PB Stuff\\Bachelor\\Builds\\HubApp_Draft_0.2.apk",
                "C:\\Users\\peter\\Desktop\\PB Stuff\\Bachelor\\Builds\\LaunchableApp_Draft_0.1.apk"
                );
            //Build handler
            BuildHandler buildHandler = new BuildHandler(config, fileGatherer);
            buildHandler.Initialize();
            //Build Menu
            BuildSelectionMenu buildMenu = new BuildSelectionMenu(buildHandler);
            menuHandler.AddStateMenuBinding(MenuHandler.MenuStates.BuildSelection, buildMenu);

            //AddProject menu
            AddProjectMenu addProjMenu = new AddProjectMenu(new BasicMenuLayout());
            menuHandler.AddStateMenuBinding(MenuHandler.MenuStates.AddProject, addProjMenu);

            //Add Bundle menu
            AddBundleMenu addBundleMenu = new AddBundleMenu(new BasicMenuLayout());
            menuHandler.AddStateMenuBinding(MenuHandler.MenuStates.AddBundle, addBundleMenu);


            menuHandler.MenuLoop();
        }
        public static void WriteInDebug(string text)
        {
            WriteInDebugWindow(text); //Forwarding call for now

            //if (debugWindow == null) return;
            //lock (debugWindow)
            //{
            //    debugWindow.WriteLine(text);
            //}
        }
        public static void WriteInDebugWindow(string text, DebugMessageFilter filter = DebugMessageFilter.Verbose) //add optional priority parameter 
        {
            if(debugRunning == false) return;

            DebugMessage message = new DebugMessage() { FilterType = filter, Message = text };
            lock (debugMessageQueue)
            {
                debugMessageQueue.Enqueue(message);
            }
        }
        public static void PopupMessage(string errorText, bool clearAfterKeyPress = true)
        {
            if(errorPopup == null) return;

            errorPopup.ResetLog();
            errorPopup.WriteLine(errorText, false);

            if(clearAfterKeyPress)
                errorPopup.WriteLine("Press any key to continue");

            errorPopup.RefreshWholeWindow();

            if(clearAfterKeyPress)
            {
                InputHelper.ReadKey(true);
                errorPopup.ClearWholeWindow();
            }
        }
        public static void ClearPopupWindow()
        {
            if (errorPopup == null) return;
            errorPopup.ClearWholeWindow();
        }
        public static void ShowLoading(float percentage)
        {
            if(loadBar == null) return;
            loadBar.RefreshWholeWindow();
            loadBar.SetPercentage(percentage);
        }
        public static void ClearLoadBar()
        {
            if (loadBar == null) return;
            loadBar.ClearWholeWindow();
        }
        private static void SetupDebugSender()
        {
            if(debugRunning)
            {
                bool isDisconnected = senderClient.Client.Poll(1000, SelectMode.SelectRead) && senderClient.Client.Available == 0;
                if (isDisconnected == false)
                {
                    return;
                }
                else
                {
                    debugRunning = false;
                }
            }

            Process debugProc = new Process();
            debugProc.StartInfo.FileName = "cmd.exe";
            debugProc.StartInfo.Arguments = "/c start \"DebugWindow\" \"Prim8VR_Installer.exe\" " + debugModeArgument;
            debugProc.StartInfo.UseShellExecute = false;
            debugProc.Start();

            string serverIP = "127.0.0.1";
            senderClient = new TcpClient(serverIP, 6364);
            sendStream = new StreamWriter(senderClient.GetStream());
            Thread sender = new Thread(() =>
            {
                //Wait while there is an active sender
                while (debugSenderActive)
                {
                    Thread.Sleep(30);
                }

                //Set debugRunning to false to tell a sender to close
                debugRunning = true;
                while (debugRunning)
                {
                    debugSenderActive = true;
                    Thread.Sleep(30); //Slightly faster than 30 FPS
                    while(debugMessageQueue.Count > 0)
                    {
                        try
                        {
                            DebugMessage message = debugMessageQueue.Dequeue();
                            string jsonData = JsonConvert.SerializeObject(message);
                            sendStream.WriteLine(jsonData);
                            sendStream.Flush();
                        }
                        catch (Exception ex)
                        {
                            debugRunning = false;
                        }
                    }
                }
                //Console.WriteLine("Client closed");
                sendStream.Close();
                senderClient.Close();
                debugSenderActive = false;
            });

            sender.IsBackground = true;
            sender.Start();
        }
        private static void DebugWindowFlow()
        {
            debugRunning = true;
            TcpListener listener = new TcpListener(IPAddress.Any, 6364);
            listener.Start();
            Console.WriteLine("Debug server started");

            TcpClient client = listener.AcceptTcpClient();
            StreamReader stream = new StreamReader(client.GetStream());

            while (debugRunning)
            {
                try
                {
                    string receivedData = stream.ReadLine();
                    DebugMessage? message = JsonConvert.DeserializeObject<DebugMessage>(receivedData);
                    //Check if it should be filtered off
                    if((int)message.FilterType <= (int)currentMessageFilter)
                    {
                        //Format: <TIME> | <FILTER CHAR> | <MESSAGE...>
                        string print = $"{DateTime.Now.ToString()} | {message.FilterType.ToString()[0]} |: {message.Message}";
                        Console.WriteLine(print);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Connection closed: " + ex.Message);
                    debugRunning = false;
                    break;
                }
            }
            stream.Close();
            client.Close();
            //Console.ReadKey(true);
        }
        private class DebugMessage
        {
            public DebugMessageFilter FilterType { get; set; }
            public string Message { get; set; }
        }
    }
}
