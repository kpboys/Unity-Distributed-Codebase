using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prim8VR_Installer.Helpers
{
    public class ExecuteCommandHelper
    {
        Process commandProcess;
        TaskCompletionSource<bool> commandCompleted;
        string commandResultText = "CMD_SUCCESS=";
        private Action<string> outputCallback;

        public string lastInput;
        public string lastOutput;
        public ExecuteCommandHelper(Action<string> outputCallback)
        {
            this.outputCallback = outputCallback;
        }
        public void Initialize()
        {
            commandProcess = new Process();
            commandProcess.StartInfo.FileName = "powershell.exe";
            commandProcess.StartInfo.RedirectStandardInput = true;
            commandProcess.StartInfo.RedirectStandardOutput = true;
            commandProcess.StartInfo.RedirectStandardError = true;
            commandProcess.StartInfo.UseShellExecute = false;
            commandProcess.Start();

            //Set up output watcher
            commandProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (e.Data.Contains(commandResultText))
                    {
                        if (e.Data.Contains(commandResultText + "True"))
                        {
                            commandCompleted.TrySetResult(true);
                        }
                        else if (e.Data.Contains(commandResultText + "False"))
                        {
                            commandCompleted.TrySetResult(false);
                        }
                    }
                    else if (e.Data.StartsWith("PS") == false)
                    {
                        outputCallback.Invoke(e.Data);
                        lastOutput = e.Data;
                        Program.WriteInDebug(e.Data);
                    }
                }
            };
            commandProcess.BeginOutputReadLine();

            //Set up error output
            commandProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Program.WriteInDebug(e.Data);
                }
            };
            commandProcess.BeginErrorReadLine();
        }
        public bool DoCommand(string command)
        {
            Program.WriteInDebug("Doing command...");
            //Set up a fresh task completion source;
            commandCompleted = new TaskCompletionSource<bool>();

            commandProcess.StandardInput.WriteLine(command);
            lastInput = command;
            commandProcess.StandardInput.WriteLine($"echo {commandResultText}$?");

            commandCompleted.Task.Wait();
            return commandCompleted.Task.Result;
        }
        public void Stop()
        {
            commandProcess.StandardInput.Close();
            commandProcess.Close();
            lastInput = "";
        }
    }
}
