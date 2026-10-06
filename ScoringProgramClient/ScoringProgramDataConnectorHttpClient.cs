using BridgeSystems.Bridgemate.DataConnector.ScoringProgramClient;
using BridgeSystems.Bridgemate.DataConnector.ScoringProgramClient.DataConnector;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BridgeSystems.Bridgemate.DataConnector.ScoringProgramClient
{
    /// <summary>
    /// Exchanges information with the dataconnector using http.
    /// </summary>
    public class ScoringProgramDataConnectorHttpClient : ScoringProgramDataConnectorClientCommandManager, IScoringProgramClient
    {
        /// <summary>
        /// The url for the retired cloud hosted webservice without the https:// prefix.
        /// </summary>
        [Obsolete("The cloud hosted data connector has been retired. Use the BaseAddress property to target a data connector host.")]
        public const string ProductionUrlWithouProtocol = "bridgematedataconnector-a6bndyc3gwgmhydq.germanywestcentral-01.azurewebsites.net";

        /// <summary>
        /// The full url for the retired cloud hosted webservice.
        /// </summary>
        [Obsolete("The cloud hosted data connector has been retired. Use the BaseAddress property to target a data connector host.")]
        public const string ProductionUrl = "https://" + ProductionUrlWithouProtocol;

        /// <summary>
        /// The url for the data connector on the local computer without the http:// prefix.
        /// </summary>
        public const string LocalHostUrlWithoutProtocol = "localhost:5079";

        /// <summary>
        /// The full url for the data connector on the local computer on the default port (5079). This is the fallback when
        /// <see cref="BaseAddress"/> has not been set and the data connector service has not published its port
        /// (<see cref="PublishedLocalHttpPort"/>).
        /// </summary>
        public const string LocalHostUrl = "http://" + LocalHostUrlWithoutProtocol;

        /// <summary>
        /// The registry key under HKEY_CURRENT_USER where the data connector service publishes the http port it actually
        /// bound (value <see cref="PublishedHttpPortValueName"/>, DWORD), its binding (<see cref="PublishedHttpBindingValueName"/>:
        /// "lan", "local" or "off") and its process id (<see cref="PublishedProcessIdValueName"/>, DWORD). Each Windows user
        /// runs their own data connector instance, hence the per-user hive.
        /// </summary>
        public const string DataConnectorPublicationRegistryKey = @"Software\Bridge Systems BV\BridgemateDataConnector";

        /// <summary>The registry value holding the http port the data connector service bound.</summary>
        public const string PublishedHttpPortValueName = "HttpPort";

        /// <summary>The registry value holding the binding of the data connector service: "lan", "local" or "off".</summary>
        public const string PublishedHttpBindingValueName = "HttpBinding";

        /// <summary>
        /// The registry value holding the process id of the data connector instance that published the port and binding,
        /// so that values left behind by an instance that no longer runs can be recognised. Absent with data connector
        /// versions that predate it.
        /// </summary>
        public const string PublishedProcessIdValueName = "HttpProcessId";

        /// <summary>
        /// The http port the data connector service of the current Windows user has published, or null when it has not
        /// published one (service never ran, or a version that predates port publication) or when the instance that
        /// published it no longer runs.
        /// </summary>
        public static int? PublishedLocalHttpPort
        {
            get
            {
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(DataConnectorPublicationRegistryKey))
                    {
                        if (key != null && IsPublisherRunning(key) &&
                            key.GetValue(PublishedHttpPortValueName) is int port && port > 0 && port <= 65535)
                            return port;
                    }
                }
                catch
                {
                    //Non-Windows platform or no registry access: fall back to the default port.
                }
                return null;
            }
        }

        //The published values outlive the instance that wrote them: a data connector that was killed (for instance by
        //the data connector of an old installation, which has the same process name) leaves them behind. Values without
        //a process id come from a data connector that predates its publication; they are taken at face value.
        private static bool IsPublisherRunning(RegistryKey key)
        {
            if (!(key.GetValue(PublishedProcessIdValueName) is int processId))
                return true;
            try
            {
                using (var process = System.Diagnostics.Process.GetProcessById(processId))
                {
                    //Guards against the process id having been reused by an unrelated process.
                    return string.Equals(process.ProcessName, BridgemateDataConnectorManager.DataConnectorProcessName,
                                         StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                //No process with this id: the publishing instance has exited.
                return false;
            }
        }

        /// <summary>
        /// The url of the data connector on the local computer: the published port when available
        /// (<see cref="PublishedLocalHttpPort"/>), otherwise the default port (<see cref="LocalHostUrl"/>).
        /// </summary>
        public static string LocalHostDiscoveredUrl
        {
            get
            {
                var publishedPort = PublishedLocalHttpPort;
                return publishedPort.HasValue ? $"http://localhost:{publishedPort.Value}" : LocalHostUrl;
            }
        }

        /// <summary>
        /// If set to true will make the client communicate with the local host hosted webservice.
        /// </summary>
        [Obsolete("The cloud hosted data connector has been retired and the local host url is the default. Use the BaseAddress property to target a different data connector host.")]
        public static bool UseLocalHost { get; set; }

        /// <summary>
        /// Constructs the full url for the webservice based on whether it is hosted in local host or in the cloud.
        /// </summary>
        [Obsolete("The cloud hosted data connector has been retired. Use the UrlRoot property, which honours BaseAddress and defaults to LocalHostUrl.")]
        public static string ApiUrlRoot => UseLocalHost ? LocalHostUrl : ProductionUrl;

        /// <summary>
        /// Constructs the url for the webservice without the http:// prefix based on whether it is hosted in local host or in the cloud.
        /// </summary>
        [Obsolete("The cloud hosted data connector has been retired. Use the UrlRoot property, which honours BaseAddress and defaults to LocalHostUrl.")]
        public static string ApiUrlRootWihtoutProtocol => UseLocalHost ? LocalHostUrlWithoutProtocol : ProductionUrlWithouProtocol;

        private string _baseAddress;

        /// <summary>
        /// The base address of the data connector host to communicate with, e.g. "http://192.168.1.50:5079".
        /// Must be an absolute http or https url. A trailing slash is removed.
        /// When not set the client targets the data connector on the local computer (<see cref="LocalHostUrl"/>).
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the value is not an absolute http or https url.</exception>
        public string BaseAddress
        {
            get => _baseAddress;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _baseAddress = null;
                    return;
                }
                if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                {
                    throw new ArgumentException($"'{value}' is not an absolute http or https url.", nameof(BaseAddress));
                }
                _baseAddress = value.TrimEnd('/');
            }
        }

        /// <summary>
        /// The url root used for all communication: <see cref="BaseAddress"/> when set, otherwise the data connector on the
        /// local computer (<see cref="LocalHostDiscoveredUrl"/>: the port the service published for this Windows user,
        /// or 5079 when none is published).
        /// </summary>
        public string UrlRoot => !string.IsNullOrWhiteSpace(_baseAddress) ? _baseAddress : LocalHostDiscoveredUrl;

        /// <summary>
        /// When set to true and the data connector host is the local computer, <see cref="Connect()"/> and <see cref="ConnectAsync()"/>
        /// will start the local data connector service if it is not running before pinging it.
        /// Has no effect when <see cref="BaseAddress"/> points to a different computer: a remote process cannot be started from here.
        /// </summary>
        public bool EnsureLocalDataConnectorIsRunning { get; set; }

        /// <summary>
        /// The url to call when the scoring program communicates with the data connector.
        /// </summary>
        public const string ApiCall = "dc-scoringprogram";

        /// <summary>
        /// Part of the expected response from the webservice when sending a Get httprequest to it.
        /// </summary>
        public const string ApiPingResponse = "Bridgemate dataconnector service version";

        /// <summary>
        /// The debug logger
        /// </summary>
        protected static readonly ILogger DebugLogger = DataConnectorLogging.LoggerFactory.CreateLogger(nameof(DebugLogger));

        /// <summary>
        /// The error logger
        /// </summary>
        protected static readonly ILogger ErrorLogger = DataConnectorLogging.LoggerFactory.CreateLogger(nameof(ErrorLogger));
        private static readonly ILogger Logger = DataConnectorLogging.LoggerFactory.CreateLogger(nameof(ScoringProgramDataConnectorHttpClient));

        private static ScoringProgramDataConnectorHttpClient _instance;

        //A single HttpClient for all requests: creating one per request exhausts sockets under load.
        //The data connector lives on localhost or the LAN: a corporate proxy without a localhost bypass
        //hijacks the request and answers 503, so the system proxy must never be used.
        private static readonly HttpClient SharedHttpClient = new HttpClient(new HttpClientHandler { UseProxy = false });

        /// <summary>
        /// Returns the singleton instance of the client with its ClubdId and LicenceKey properties set to the values of the parameters.
        /// The client targets the data connector on the local computer unless <see cref="BaseAddress"/> is set.
        /// </summary>
        /// <param name="clubId">The id of the club that is using the client</param>
        /// <param name="licenceKey">The licence key for the club using the client</param>
        /// <returns></returns>
        public static ScoringProgramDataConnectorHttpClient Instance(string clubId, string licenceKey)
        {
            if (_instance == null)
                _instance = new ScoringProgramDataConnectorHttpClient(clubId, licenceKey);
            _instance.Credentials = (clubId, licenceKey);
            return _instance;
        }

        /// <summary>
        /// Returns the singleton instance of the client with its ClubdId, LicenceKey and BaseAddress properties set to the values of the parameters.
        /// </summary>
        /// <param name="clubId">The id of the club that is using the client</param>
        /// <param name="licenceKey">The licence key for the club using the client</param>
        /// <param name="baseAddress">The base address of the data connector host, e.g. "http://192.168.1.50:5079"</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="baseAddress"/> is not an absolute http or https url.</exception>
        public static ScoringProgramDataConnectorHttpClient Instance(string clubId, string licenceKey, string baseAddress)
        {
            var instance = Instance(clubId, licenceKey);
            instance.BaseAddress = baseAddress;
            return instance;
        }

        private ScoringProgramDataConnectorHttpClient(string clubdId, string licenceKey)
        {
            Credentials = (clubdId, licenceKey);
        }

        /// <summary>
        /// Checks if the webservice on the static <see cref="ApiUrlRoot"/> url can be reached.
        /// </summary>
        [Obsolete("Use the IsServiceAliveAsync instance method, which honours BaseAddress.")]
        public static Task<string> IsServiceAlive()
        {
#pragma warning disable CS0618 //Both members are obsolete for the same reason.
            return PingService(ApiUrlRoot);
#pragma warning restore CS0618
        }

        /// <summary>
        /// Checks if the data connector host on <see cref="UrlRoot"/> can be reached by sending a Get request to it.
        /// The returned message contains <see cref="ApiPingResponse"/> when it can.
        /// </summary>
        public Task<string> IsServiceAliveAsync()
        {
            return PingService(UrlRoot);
        }

        private static async Task<string> PingService(string url)
        {
            var responseMessage = "";
            try
            {
                HttpResponseMessage response = await SharedHttpClient.GetAsync(url).ConfigureAwait(false);
                response.EnsureSuccessStatusCode(); // Throw if not 200–299

                responseMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex, ex.Message);
                responseMessage = ex.Message;
            }
            return responseMessage;
        }
/// <summary>
/// The information needed to get access to the http channel for the data communicator.
/// </summary>
public (string clubId, string licenceKey) Credentials { get; set; }


        private bool disposedValue;

        /// <summary>
        /// Always returns true.
        /// </summary>
        public override bool IsActive => true;

        /// <summary>
        /// Checks if the endpoint can be reached by sending a ping request to it.
        /// </summary>
        /// <returns></returns>
        public ScoringProgramResponse Connect()
        {
            return ConnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Checks if the endpoint can be reached by sending a ping request to it.
        /// When <see cref="EnsureLocalDataConnectorIsRunning"/> is true and the endpoint is on the local computer,
        /// starts the local data connector service first if it is not running.
        /// </summary>
        /// <returns></returns>
        public async Task<ScoringProgramResponse> ConnectAsync()
        {
            var startedLocally = false;
            if (EnsureLocalDataConnectorIsRunning && TargetsLocalComputer)
            {
                //When the base address explicitly names a localhost port, the started service is told to bind it.
                //Without a base address the service chooses its own port and publishes it, which UrlRoot picks up.
                int? explicitPort = null;
                if (!string.IsNullOrWhiteSpace(_baseAddress) && Uri.TryCreate(_baseAddress, UriKind.Absolute, out var baseUri))
                    explicitPort = baseUri.Port;
                startedLocally = BridgemateDataConnectorManager.EnsureDataConnectorServiceIsRunning(forceRestart: false, explicitPort);
            }
            var responseMessage = await IsServiceAliveAsync().ConfigureAwait(false);
            var success = responseMessage.Contains(ApiPingResponse);
            if (!success && startedLocally)
            {
                //A freshly started service needs a moment to migrate its database, bind its port and publish it.
                //UrlRoot is re-evaluated on every attempt, so a newly published port is picked up.
                for (var attempt = 0; attempt < 20 && !success; attempt++)
                {
                    await Task.Delay(500).ConfigureAwait(false);
                    responseMessage = await IsServiceAliveAsync().ConfigureAwait(false);
                    success = responseMessage.Contains(ApiPingResponse);
                }
            }
            return new ScoringProgramResponse
            {
                RequestCommand = ScoringProgramDataConnectorCommands.Connect ,
                DataType = success ? DataConnectorResponseData.OK : DataConnectorResponseData.Error,
                ErrorType = success ? ErrorType.None : ErrorType.NoConnection,
                SerializedData = JsonSerializer.Serialize(responseMessage)
            };
        }

        /// <summary>
        /// True when <see cref="UrlRoot"/> points to the local computer.
        /// </summary>
        private bool TargetsLocalComputer =>
            Uri.TryCreate(UrlRoot, UriKind.Absolute, out var uri) && uri.IsLoopback;

        /// <summary>
        /// Not implemented for http.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public override (DataConnectorResponseData result, string message, ErrorType errorType) Connect(string connectionName)
        {
            return (DataConnectorResponseData.None, "Not implemented", ErrorType.NotImplemented);
        }


        /// <summary>
        /// Not implemented for http.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        public override async Task<(DataConnectorResponseData result, string message, ErrorType errorType)> ConnectAsync(string connectionName)
        {
            await Task.CompletedTask;
            return (DataConnectorResponseData.None, "Not implemented", ErrorType.NotImplemented);
        }

        /// <summary>
        /// Not implemented for http.
        /// </summary>
        /// <returns></returns>
        public async Task<ScoringProgramResponse> DisconnectAsync()
        {
            await Task.CompletedTask;
            return new ScoringProgramResponse
            {
                RequestCommand = ScoringProgramDataConnectorCommands.Disconnect,
                SessionGuid = "",
                DataType = DataConnectorResponseData.OK,
                ErrorType = ErrorType.NotImplemented,
                SerializedData = JsonSerializer.Serialize("Not implemented for Http client.")
            };
        }

        /// <summary>
        /// Not implemented for http.
        /// </summary>
        /// <returns></returns>
        public ScoringProgramResponse Disconnect()
        {
            return new ScoringProgramResponse
            {
                RequestCommand = ScoringProgramDataConnectorCommands.Disconnect,
                SessionGuid = "",
                DataType = DataConnectorResponseData.OK,
                ErrorType = ErrorType.NotImplemented,
                SerializedData = JsonSerializer.Serialize("Not implemented for Http client.")
            };
        }

        /// <summary>
        /// The code that handles the actual sending of requests and reading their reponses.
        /// </summary>
        /// <param name="sessionGuid">Specifies which session the request targets (if any)</param>
        /// <param name="command">The command to the middlleman</param>
        /// <param name="serializedData">The data to send to the Data Connector as json data. (If any)</param>
        /// <returns></returns>
        protected override async Task<ScoringProgramResponse> SendDataAsync(string sessionGuid,
            ScoringProgramDataConnectorCommands command,
            string serializedData, 
            string caller = "")
        {
            //Construct the request to the Data Connector.
            var request = new ScoringProgramRequest
            {
                Command = command,
                SessionGuid = sessionGuid,
                SerializedData = serializedData,
                ClubId = Credentials.clubId,
                LicenceKey = Credentials.licenceKey
            };

            //One request at a time: wait for any in-flight request to finish. The wait does not block the
            //calling thread; only after the timeout the request is rejected with Busy.
            if (!await SendGate.WaitAsync(AsyncSendGateTimeoutMs).ConfigureAwait(false))
            {
                return CreateBusyResponse(command, sessionGuid);
            }
            try
            {
                IsSending = true;

                //Serialize
                var requestSerialized = JsonSerializer.Serialize(request);
                {
                    var retryCounter = 5;
                    while (retryCounter > 0)
                    {
                        //Send the request to the Data Connector and await the response.
                        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{UrlRoot}/{ApiCall}");
                        var content = new StringContent(requestSerialized, Encoding.UTF8, "application/json");
                        requestMessage.Content = content;
                        HttpResponseMessage httpResponse = null;
                        try
                        {
                            httpResponse = await SharedHttpClient.SendAsync(requestMessage).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, ex.Message);
                            retryCounter--;
                            await Task.Delay(1000 - retryCounter * 200).ConfigureAwait(false);
                        }
                        if (httpResponse != null)
                        {
                            if (httpResponse.IsSuccessStatusCode)
                            {
                                var json = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                                retryCounter = 0;
                                var clientResponse = JsonSerializer.Deserialize<ScoringProgramResponse>(json);
                                return clientResponse ??
                                             new ScoringProgramResponse
                                             {
                                                 RequestCommand = command,
                                                 DataType = DataConnectorResponseData.Error,
                                                 SerializedData = JsonSerializer.Serialize("Empty response")
                                             };

                            }
                            else
                            {
                                retryCounter--;
                                var errorMessage = httpResponse.ReasonPhrase;
                                Logger.LogError(errorMessage);
                                await Task.Delay(1000 - retryCounter * 200).ConfigureAwait(false);
                            }
                        }
                    }
                    return new ScoringProgramResponse
                    {
                        RequestCommand = command,
                        DataType = DataConnectorResponseData.Error,
                        ErrorType = ErrorType.NoConnection
                    };
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogError(ex, ex.Message);
                ErrorLogger.LogError(ex, ex.Message);
                return
                new ScoringProgramResponse
                {
                    RequestCommand = command,
                    DataType = DataConnectorResponseData.Error,
                    SerializedData = JsonSerializer.Serialize(ex.Message)
                };
            }

            finally
            {
                //Always signal that the client is free for the next items to send.
                //Otherwise after an exception further communication will be blocked.
                IsSending = false;
                SendGate.Release();
            }
        }

        /// <summary>
        /// The code that handles the actual sending of requests and reading their reponses.
        /// </summary>
        /// <param name="sessionGuid">Specifies which session the request targets (if any)</param>
        /// <param name="command">The command to the middlleman</param>
        /// <param name="serializedData">The data to send to the Data Connector as json data. (If any)</param>
        /// <returns></returns>
        protected override ScoringProgramResponse SendData(string sessionGuid,
            ScoringProgramDataConnectorCommands command,
            string serializedData, 
            string caller="")
        {
            //Construct the request to the Data Connector.
            var request = new ScoringProgramRequest
            {
                Command = command,
                SessionGuid = sessionGuid,
                SerializedData = serializedData,
                ClubId = Credentials.clubId,
                LicenceKey = Credentials.licenceKey
            };



            //One request at a time: wait briefly for any in-flight request to finish. The wait blocks the
            //calling thread (possibly the UI thread), hence the short synchronous timeout before rejecting with Busy.
            if (!SendGate.Wait(SyncSendGateTimeoutMs))
            {
                return CreateBusyResponse(command, sessionGuid);
            }
            try
            {
                IsSending = true;

                //Serialize
                var requestSerialized = JsonSerializer.Serialize(request);
                {
                    var retryCounter = 5;
                    while (retryCounter > 0)
                    {
                        //Send the request to the Data Connector and await the response.
                        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{UrlRoot}/{ApiCall}");
                        var content = new StringContent(requestSerialized, Encoding.UTF8, "application/json");
                        requestMessage.Content = content;
                        HttpResponseMessage httpResponse = null;
                        try
                        {
                            httpResponse = SharedHttpClient.SendAsync(requestMessage).ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, ex.Message);
                            retryCounter--;
                            Thread.Sleep(1000 - retryCounter * 200);
                        }
                        if (httpResponse != null)
                        {
                            if (httpResponse.IsSuccessStatusCode)
                            {
                                var json = httpResponse.Content.ReadAsStringAsync().Result;
                                retryCounter = 0;
                                var clientResponse = JsonSerializer.Deserialize<ScoringProgramResponse>(json);
                                return clientResponse ??
                                             new ScoringProgramResponse
                                             {
                                                 RequestCommand = command,
                                                 DataType = DataConnectorResponseData.Error,
                                                 SerializedData = JsonSerializer.Serialize("Empty response")
                                             };

                            }
                            else
                            {
                                retryCounter--;
                                var errorMessage = httpResponse.ReasonPhrase;
                                Logger.LogError(errorMessage);
                                Thread.Sleep(1000 - retryCounter * 200);
                            }
                        }
                    }
                    return new ScoringProgramResponse
                    {
                        RequestCommand = command,
                        DataType = DataConnectorResponseData.Error,
                        ErrorType = ErrorType.NoConnection
                    };
                }
            }
            catch (Exception ex)
            {
                DebugLogger.LogError(ex, ex.Message);
                ErrorLogger.LogError(ex, ex.Message);
                return
                new ScoringProgramResponse
                {
                    RequestCommand = command,
                    DataType = DataConnectorResponseData.Error,
                    SerializedData = JsonSerializer.Serialize(ex.Message)
                };
            }

            finally
            {
                //Always signal that the client is free for the next items to send.
                //Otherwise after an exception further communication will be blocked.
                IsSending = false;
                SendGate.Release();
            }
        }




        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ScoringProgramHttpClient()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}
