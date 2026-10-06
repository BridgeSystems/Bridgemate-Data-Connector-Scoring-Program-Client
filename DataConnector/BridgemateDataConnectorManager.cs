using System;
using System.Collections.Generic;
using SIO = System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;

namespace BridgeSystems.Bridgemate.DataConnector.ScoringProgramClient.DataConnector
{

    public static class BridgemateDataConnectorManager
    {
        private static ILogger Logger = DataConnectorLogging.LoggerFactory.CreateLogger(nameof(BridgemateDataConnectorManager));

        /// <summary>
        /// The process name of the Bridgemate DataConnectorService (the name of its executable without extension).
        /// </summary>
        public const string DataConnectorProcessName = "BridgeSystems.Bridgemate.DataConnectorService";

        /// <summary>
        /// The name of the executable of the Bridgemate DataConnectorService
        /// </summary>
        public const string FullDataConnectorName = DataConnectorProcessName + ".exe";

        /// <summary>
        /// Starts the Bridgemate Data Connector of the installed BCS when it is not running in the current Windows session.
        /// Every Windows user runs their own data connector instance, so only processes in the caller's session count.
        /// A running data connector of a different installation (for instance an old one started at logon) is replaced:
        /// it may not serve http at all. The started data connector stops it.
        /// </summary>
        /// <param name="forceRestart">If "true" restart even if it is running.</param>
        /// <param name="httpPort">When given, the started service is told to bind this http port (--httpport).
        /// When null the service chooses its port itself and publishes it in the registry.</param>
        /// <returns></returns>
        public static bool EnsureDataConnectorServiceIsRunning(bool forceRestart, int? httpPort = null)
        {
            try
            {
                var BcsExePath =(string) Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Bridge Systems BV\BCS.Net\InfoForExternalProgram")
                    .GetValue("ExePath");
                var dataconnectorExePath = Path.Combine(Path.GetDirectoryName(BcsExePath), "BDC", FullDataConnectorName);
                if (forceRestart)
                {
                    Logger.LogInformation("Starting the data connector {Path}: forced restart.", dataconnectorExePath);
                    return Restart(dataconnectorExePath);
                }

                var runningPaths = GetRunningPathsInCurrentSession();
                if (runningPaths.Count == 0)
                {
                    Logger.LogInformation("Starting the data connector {Path}: no instance is running in this session.", dataconnectorExePath);
                    return Restart(dataconnectorExePath);
                }
                //An instance whose path cannot be read (null) is assumed to be the right one: restarting the data
                //connector disturbs BCS, so that is only done on evidence.
                var foreignPath = runningPaths.FirstOrDefault(path => path != null && !IsSameFile(path, dataconnectorExePath));
                if (foreignPath != null)
                {
                    Logger.LogWarning("Starting the data connector {Path}: the running instance is a different installation ({ForeignPath}).",
                                      dataconnectorExePath, foreignPath);
                    return Restart(dataconnectorExePath);
                }
                return true;
            }
            catch
            {
                return false;
            }

            //No "close" argument is needed: the data connector stops a predecessor in the same session at start-up
            //(and -c would mean --console, popping up a console window).
            bool Restart(string path)
            {
                var portArgument = httpPort.HasValue ? $" --httpport {httpPort.Value}" : "";
                return StartProcess(path, $"-i{FullDataConnectorName}{portArgument}");
            }
        }

        //The executable paths of the data connector processes in the current Windows session; null for a process whose
        //path cannot be read.
        private static List<string> GetRunningPathsInCurrentSession()
        {
            var currentSessionId = Process.GetCurrentProcess().SessionId;
            var paths = new List<string>();
            foreach (var process in Process.GetProcessesByName(DataConnectorProcessName))
            {
                using (process)
                {
                    try
                    {
                        if (process.SessionId != currentSessionId)
                            continue;
                    }
                    catch
                    {
                        //Another user's process may refuse the query; it is not ours then.
                        continue;
                    }
                    paths.Add(GetExecutablePath(process.Id));
                }
            }
            return paths;
        }

        private static bool IsSameFile(string path, string otherPath)
        {
            try
            {
                return string.Equals(Path.GetFullPath(path), Path.GetFullPath(otherPath), StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        //Process.MainModule cannot read a 64-bit process from a 32-bit one (and many scoring programs are 32-bit);
        //QueryFullProcessImageName with limited query rights can.
        private static string GetExecutablePath(int processId)
        {
            var handle = OpenProcess(ProcessQueryLimitedInformation, false, processId);
            if (handle == IntPtr.Zero)
                return null;
            try
            {
                var buffer = new StringBuilder(1024);
                var size = buffer.Capacity;
                return QueryFullProcessImageName(handle, 0, buffer, ref size) ? buffer.ToString(0, size) : null;
            }
            finally
            {
                CloseHandle(handle);
            }
        }

        private const uint ProcessQueryLimitedInformation = 0x1000;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint desiredAccess, bool inheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool QueryFullProcessImageName(IntPtr process, int flags, StringBuilder exeName, ref int size);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);

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
