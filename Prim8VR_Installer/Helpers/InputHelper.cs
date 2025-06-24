using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prim8VR_Installer.Helpers
{
    public static class InputHelper
    {
        class InputEvent
        {
            public int ID { get; set; }
            public Action<ConsoleKeyInfo> Action { get; set; }
        }
        static List<InputEvent> listeners = new List<InputEvent>();
        static bool running;
        static int idCounter = 1;
        static readonly int maxID = 1001;
        static bool hideCharacters = false;

        public static void Start()
        {
            Thread t = new Thread(() =>
            {
                running = true;
                while (running)
                {
                    ConsoleKeyInfo key = Console.ReadKey(hideCharacters);
                    if(listeners.Count > 0)
                    {
                        lock (listeners)
                        {
                            for (int i = 0; i < listeners.Count; i++)
                            {
                                listeners[i].Action.Invoke(key);
                            }
                        }
                    }
                }
            });
            t.IsBackground = true;
            t.Start();
        }
        private static int TickID()
        {
            idCounter++;
            if (idCounter >= maxID)
                idCounter = 1;
            return idCounter;
        }
        public static int AddListener(Action<ConsoleKeyInfo> callback)
        {
            lock (listeners)
            {
                int id = TickID();
                //Look to see if ID is unique
                bool foundOpenId = false;
                while (foundOpenId == false)
                {
                    foundOpenId = true;
                    for (int i = 0; i < listeners.Count; i++)
                    {
                        if (listeners[i].ID == id)
                        {
                            foundOpenId = false;
                            id = TickID();
                            break;
                        }
                    }
                }

                listeners.Add(new InputEvent()
                {
                    ID = id,
                    Action = callback
                });
                return id;
            }
        }
        public static int AddListener(ConsoleKey key, Action callback)
        {
            Action<ConsoleKeyInfo> newCB = (x) =>
            {
                if (x.Key == key)
                    callback.Invoke();
            };
            return AddListener(newCB);
        }
        public static int AddListener(ConsoleKey key, ConsoleModifiers modifier, Action callback)
        {
            Action<ConsoleKeyInfo> newCB = (x) =>
            {
                if (x.Key == key && x.Modifiers.HasFlag(modifier))
                    callback.Invoke();
            };
            return AddListener(newCB);
        }
        //public static int AddCompoundListener(Action callback, params ConsoleKey[] keys)
        //{
        //    Queue<ConsoleKey> keyHistory = new Queue<ConsoleKey>();
        //    int length = keys.Length;
        //    Action<ConsoleKeyInfo> compoundCB = (x) =>
        //    {
        //        keyHistory.Enqueue(x.Key);
        //        if (keyHistory.Count > length)
        //            keyHistory.Dequeue();
        //        if (keyHistory.ToArray().SequenceEqual(keys))
        //            callback.Invoke();
        //    };
        //    return AddListener(compoundCB);
        //}
        public static void RemoveListener(int id)
        {
            lock (listeners)
            {
                for (int i = 0; i < listeners.Count; i++)
                {
                    if (listeners[i].ID == id)
                    {
                        listeners.RemoveAt(i);
                        return;
                    }
                }
            }
        }
        public static ConsoleKeyInfo ReadKey(bool hide)
        {
            bool prevVal = hideCharacters;
            hideCharacters = hide;
            ConsoleKeyInfo result = new ConsoleKeyInfo();
            bool gotInput = false;
            int id = AddListener((x) =>
            { 
                gotInput = true;
                result = x;
            });
            while (gotInput == false)
            {
                Thread.Sleep(30);
            }
            RemoveListener(id);
            hideCharacters = prevVal;
            return result;
        }
        public static string ReadLine()
        {
            string result = "";
            bool gotInput = false;
            int id = AddListener((x) =>
            {
                if (x.Key == ConsoleKey.Enter)
                    gotInput = true;
                else
                    result += x.KeyChar;
            });
            while (gotInput == false)
            {
                Thread.Sleep(30);
            }
            RemoveListener(id);
            return result + "\n";
        }
    }
}
