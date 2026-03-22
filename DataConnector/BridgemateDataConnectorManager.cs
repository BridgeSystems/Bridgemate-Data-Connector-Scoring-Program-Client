using System;
using System.Collections.Generic;
using SIO = System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace BridgeSystems.Bridgemate.DataConnector.ScoringProgramClient.DataConnector
{

    public static class BridgemateDataConnectorManager
    {
        private static ILogger Logger = DataConnectorLogging.LoggerFactory.CreateLogger(nameof(BridgemateDataConnectorManager));

        /// <summary>
        /// The name of the executable of the Bridgemate DataConnectorService
        /// </summary>
        public const string FullDataConnectorName = "BridgeSystems.Bridgemate.DataConnectorService.exe";

        /// <summary>
        /// Restarts the Bridgemate Data Connector if it is not running.
        /// </summary>
        /// <param name="forceRestart">If "true" restart even if it is running.</param>
        /// <returns></returns>
        public static bool EnsureDataConnectorServiceIsRunning(bool forceRestart)
        {
            try
            {
                var BcsExePath =(string) Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Bridge Systems BV\BCS.Net\InfoForExternalProgram")
                    .GetValue("ExePath");
                var dataconnectorExePath = Path.Combine(Path.GetDirectoryName(BcsExePath), "BDC", FullDataConnectorName);
                if (forceRestart)
                    return Restart(dataconnectorExePath, force: true);
                else
                {
                    Process process = GetProcess(Path.GetFileNameWithoutExtension(dataconnectorExePath));
                    if (process != null) return true;

                    return Restart(dataconnectorExePath);
                }
            }
            catch
            {
                return false;
            }

            bool Restart(string path, bool force = false)
            {
                return StartProcess(path, $"-i{FullDataConnectorName} {(force ? "-c" : "")}");
            }
        }

        public static bool StartProcess(string path, string parameters = "", string workingDirectory = null)
        {
            var process = new Process();
            try
            {
                process.StartInfo.FileName = path;
                process.StartInfo.WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(path);
                process.StartInfo.Arguments = parameters;
                process.Start();
                //process.WaitForInputIdle();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                return false;
            }
        }

        public static Process GetProcess(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0) return null;
            return processes[0];
        }
    }

}
