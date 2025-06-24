using Newtonsoft.Json;
using Prim8VR_Installer.Helpers;
using Prim8VR_Installer.JsonObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Prim8VR_Installer.Behaviors
{
    public struct DeviceInfo
    {
        public string ID;
        public string Device;
        public string Model;
        public string TransportID;
        public override string ToString()
        {
            return "ID: " + ID + " Device: " + Device + " Model: " + Model + " TransportID: " + TransportID;
        }
    }
    public class GetDevice
    {
        enum OutputListenerState
        {
            Ignore,
            ListDevices
        }
        GeneralConfig? config;
        //Process adbCheckProcess;
        bool running;
        //string commandResultText = "CMD_SUCCESS=";
        //string lastInput;
        //string lastOutput;
        OutputListenerState listenerState = OutputListenerState.Ignore;

        DeviceListOutputHandler devicesOutputHandler;
        private DeviceInfo targetDevice;

        //TaskCompletionSource<bool> commandCompleted;

        private ExecuteCommandHelper commandHelper;

        public DeviceInfo TargetDevice { get => targetDevice; }

        public GetDevice(GeneralConfig config)
        {
            this.config = config;
        }
        public bool DoCheck()
        {
            devicesOutputHandler = new DeviceListOutputHandler();

            commandHelper = new ExecuteCommandHelper((x) => ProcessCommandOutput(x));
            commandHelper.Initialize();

            //Flow starts here
            bool result = CommandFlow();
            if (result)
            {
                targetDevice = devicesOutputHandler.GetDevices.Last();
            }

            commandHelper.Stop();
            return result;
        }
        private bool CommandFlow()
        {
            //Go to ADB directory
            listenerState = OutputListenerState.Ignore;
            bool success = commandHelper.DoCommand("cd " + config.AdbPath);
            if (success == false)
            {
                Program.WriteInDebug("Failed to go to directory");
                return false;
            }

            //Get info about connected devices
            listenerState = OutputListenerState.ListDevices;
            success = commandHelper.DoCommand("./adb devices -l");
            if(success == false)
            {
                Program.WriteInDebug("Failed to run adb devices");
                return false;
            }

            if(devicesOutputHandler.GetDevices == null || devicesOutputHandler.GetDevices.Count == 0)
            {
                Program.WriteInDebug("Found no devices");
                return false;
            }
            Program.WriteInDebug("Devices: " + devicesOutputHandler.GetDevices.Last().ToString());
            return true;
        }
        private void ProcessCommandOutput(string output)
        {
            switch (listenerState)
            {
                case OutputListenerState.ListDevices:
                    devicesOutputHandler.InterpretText(output);
                    break;
                case OutputListenerState.Ignore:
                default:
                    break;
            }
        }
        //Might need a check for whether the device is authorized in here
        private class DeviceListOutputHandler
        {
            private List<DeviceInfo> devices; 
            private bool shouldSeeDeviceInfo;

            //Text to recognize
            private readonly string listDevicesText = "List of devices attached";

            public List<DeviceInfo> GetDevices { get => devices; }
            public DeviceListOutputHandler()
            {
                devices = new List<DeviceInfo>();
                shouldSeeDeviceInfo = false;
            }
            public void InterpretText(string text)
            {
                if(shouldSeeDeviceInfo == false)
                {
                    if (text.Contains(listDevicesText))
                    {
                        shouldSeeDeviceInfo = true;
                    }
                }
                else
                {
                    string[] parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    string device = "";
                    string model = "";
                    string transportID = "";

                    for (var i = 2; i < parts.Length; i++)
                    {
                        var halves = parts[i].Split(new Char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                        if (2 == halves.Length)
                        {
                            switch (halves[0])
                            {
                                case "transport_id":
                                    transportID = halves[1];
                                    break;
                                case "model":
                                    model = halves[1];
                                    break;
                                case "device":
                                    device = halves[1];
                                    break;
                            }
                        }
                    }
                    devices.Add(new DeviceInfo() { ID = parts[0], Device = device, Model = model, TransportID = transportID });
                }
            }
        }
    }
}
