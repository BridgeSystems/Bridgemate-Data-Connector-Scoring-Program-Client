using BridgeSystems.Bridgemate.DataConnector.ScoringProgramClient;
using Microsoft.Extensions.Logging;
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
        /// The url for the webservice without the http:// prefix.
        /// </summary>
        public const string ProductionUrlWithouProtocol = "bridgematedataconnector-a6bndyc3gwgmhydq.germanywestcentral-01.azurewebsites.net";

        /// <summary>
        /// The full url for the webservice.
        /// </summary>
        public const string ProductionUrl = "https://" + ProductionUrlWithouProtocol;

        /// <summary>
        /// The url for the local host hosted webservice without the http:// prefix.
        /// Used for debugging only.
        /// </summary>
        public const string LocalHostUrlWithoutProtocol = "localhost:5079";

        /// <summary>
        /// The full url for the local host hosted webservice.
        /// Used for debugging only.
        /// </summary>
        public const string LocalHostUrl = "http://" + LocalHostUrlWithoutProtocol;

        /// <summary>
        /// If set to true will make the client communicate with the local host hosted webservice.
        /// Used for debugging only.
        /// </summary>
        public static bool UseLocalHost { get; set; }

        /// <summary>
        /// Constructs the full url for the webservice based on whether it is hosted in local host or in the cloud.
        /// </summary>
        public static string ApiUrlRoot => UseLocalHost ? LocalHostUrl : ProductionUrl;

        /// <summary>
        /// Constructs the url for the webservice without the http:// prefix based on whether it is hosted in local host or in the cloud.
        /// </summary>
        public static string ApiUrlRootWihtoutProtocol = UseLocalHost ? LocalHostUrlWithoutProtocol : ProductionUrlWithouProtocol;

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
        
        /// <summary>
        /// Returns the singleton instance of the client with its ClubdId and LicenceKey properties set to the values of the parameters.
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

        private ScoringProgramDataConnectorHttpClient(string clubdId, string licenceKey)
        {
            Credentials = (clubdId, licenceKey);
        }

        public static async Task<string> IsServiceAlive()
        {
            using (HttpClient client = new HttpClient())
            {
                var success = false;
                var responseMessage = "";
                try
                {
                    var url = ApiUrlRoot;
                    HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode(); // Throw if not 200–299

                    var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    success = responseBody.Contains(ApiPingResponse);
                    responseMessage = responseBody;
                }
                catch (HttpRequestException ex)
                {
                    Logger.LogError(ex, ex.Message);
                    success = false;
                    responseMessage = ex.Message;
                }
                return responseMessage;
            }
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
            var responseMessage = IsServiceAlive().ConfigureAwait(false).GetAwaiter().GetResult();
            var success = responseMessage.Contains(ApiPingResponse);
            return new ScoringProgramResponse
            {
                RequestCommand = ScoringProgramDataConnectorCommands.Connect,
                DataType = success ? DataConnectorResponseData.OK : DataConnectorResponseData.Error,
                ErrorType = success ? ErrorType.None : ErrorType.NoConnection,
                SerializedData = JsonSerializer.Serialize(responseMessage)
            };
        }

        /// <summary>
        /// Checks if the endpoint can be reached by sending a ping request to it.
        /// </summary>
        /// <returns></returns>
        public async Task<ScoringProgramResponse> ConnectAsync()
        {
            var responseMessage = await IsServiceAlive();
            var success = responseMessage.Contains(ApiPingResponse);
            return new ScoringProgramResponse
            {
                RequestCommand = ScoringProgramDataConnectorCommands.Connect ,
                DataType = success ? DataConnectorResponseData.OK : DataConnectorResponseData.Error,
                ErrorType = success ? ErrorType.None : ErrorType.NoConnection,
                SerializedData = JsonSerializer.Serialize(responseMessage)
            };
        }

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

            //Do not proceed if sending is already in progress (for an other request). There can be only on request be sent at the same time.
            if (IsSending)
            {
                return new ScoringProgramResponse
                {
                    RequestCommand = command,
                    SessionGuid = sessionGuid,
                    DataType = DataConnectorResponseData.Error,
                    ErrorType = ErrorType.Busy,
                    SerializedData = JsonSerializer.Serialize($"Client is busy, please retry later.")
                };
            }
            try
            {
                IsSending = true;

                //Serialize
                var requestSerialized = JsonSerializer.Serialize(request);
                using (var httpClient = new HttpClient())
                {
                    var retryCounter = 5;
                    while (retryCounter > 0)
                    {
                        //Send the request to the Data Connector and await the response.
                        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{ApiUrlRoot}/{ApiCall}");
                        var content = new StringContent(requestSerialized, Encoding.UTF8, "application/json");
                        requestMessage.Content = content;
                        HttpResponseMessage httpResponse = null;
                        try
                        {
                            httpResponse = await httpClient.SendAsync(requestMessage);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, ex.Message);
                            retryCounter--;
                            await Task.Delay(1000 - retryCounter * 200);
                        }
                        if (httpResponse != null)
                        {
                            if (httpResponse.IsSuccessStatusCode)
                            {
                                var json = await httpResponse.Content.ReadAsStringAsync();
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
                                await Task.Delay(1000 - retryCounter * 200);
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



            //Do not proceed if sending is already in progress (for an other request). There can be only on request be sent at the same time.
            if (IsSending)
            {
                return new ScoringProgramResponse
                {
                    RequestCommand = command,
                    SessionGuid = sessionGuid,
                    DataType = DataConnectorResponseData.Error,
                    ErrorType = ErrorType.Busy,
                    SerializedData = JsonSerializer.Serialize($"Client is busy, please retry later.")
                };
            }
            try
            {
                IsSending = true;

                //Serialize
                var requestSerialized = JsonSerializer.Serialize(request);
                using (var httpClient = new HttpClient())
                {
                    var retryCounter = 5;
                    while (retryCounter > 0)
                    {
                        //Send the request to the Data Connector and await the response.
                        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{ApiUrlRoot}/{ApiCall}");
                        var content = new StringContent(requestSerialized, Encoding.UTF8, "application/json");
                        requestMessage.Content = content;
                        HttpResponseMessage httpResponse = null;
                        try
                        {
                            httpResponse = httpClient.SendAsync(requestMessage).ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, ex.Message);
                            retryCounter--;
                            Thread.Sleep(10000 - retryCounter * 200);
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
                                Thread.Sleep(10000 - retryCounter * 200);
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
