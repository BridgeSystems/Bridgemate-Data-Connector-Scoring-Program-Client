using BridgeSystems.Bridgemate.DataConnector.ScoringProgramClient.DataConnector;
using BridgeSystems.Bridgemate.DataConnectorClasses.SharedDTO;
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using static BridgeSystems.Bridgemate.DataConnector.ScoringProgramClient.DataConnectorLogCreator<BridgeSystems.Bridgemate.DataConnector.ScoringProgramClient.ScoringProgramDataConnectorCommands>;

namespace BridgeSystems.Bridgemate.DataConnector.ScoringProgramClient
{
    /// <summary>
    /// Handles request from the scoring programs. This code is freely available for external programmers to adapt to their
    /// specific needs. This code, however, is guaranteed to work. Interop options are available for many programming languages 
    /// that can interface with .Net Standard 2.0 assemblies.
    /// <br/>
    /// Note that this class is written as a singleton: there should never be more than one instance communicating over the pipe. 
    /// The constructor is protected however, so by inheriting from this class a non singleton instance can be used.
    /// <br/>
    /// The class implements IDisposable through its base class, as the base class contains a stream writer and stream reader that must be disposed
    /// when the communication shuts down. Be sure to call Dispose on the class any time it is no longer in use, otherwise communication will stall.
    /// <br/>
    /// The code has no dependencies except on Microsoft.Extensions.Logging.Abstractions (and .NET Standard 2.0).
    /// To enable logging, set <see cref="DataConnectorLogging.LoggerFactory"/> to your own ILoggerFactory instance.
    /// <br/>
    /// All public functions have synchronous and asynchronous implementations (ending in "Async").
    /// <br/>
    /// To get started inspect the following functions:<br/>
    /// <see cref="Connect()">Connect</see> and <see cref="ConnectAsync()">ConnectAsync</see>: Connection to the Data Connector.<br/>
    /// <see cref="ScoringProgramDataConnectorClientCommandManager.Initialize(InitDTO)">Initialize</see> and <see cref="ScoringProgramDataConnectorClientCommandManager.InitializeAsync(InitDTO)">InitializeAsync</see>: Initialization of a new event. <br/>
    /// <see cref="ScoringProgramDataConnectorClientCommandManager.IssueManagementCommand(BCSManagementRequestDTO)">IssueManagementCommand</see> and <see cref="ScoringProgramDataConnectorClientCommandManager.IssueManagementCommandAsync(BCSManagementRequestDTO)">IssueManagementCommandAsync</see>: Query the Bridgemate Control Software.
    /// </summary>
    public class ScoringProgramDataConnectorPipeClient : ScoringProgramDataConnectorClientCommandManager, IScoringProgramClient
    {
        /// <summary>
        /// The name of the pipe that handles the bidirectional communication with the Data Connector.
        /// Each Windows account must have its own pipe, hence the username of the logged in user (as Windows knows it) is appended.
        /// Tip: you can find the usernames as Windows knows them by going to the C:\Users folder.
        /// </summary>
        public string PipeName = $"BridgeSystems.Bridgemate.DataConnectorService.ScoringProgram.{Environment.UserName}";

        /// <summary>
        /// The implementation of the singleton pattern. Atypically the constructor is protected rather than private.
        /// </summary>
        private static ScoringProgramDataConnectorPipeClient _instance;

        /// <summary>
        /// The single instance of the pipe client. Use this property to retrieve the client and use it in the external program.
        /// </summary>
        public static ScoringProgramDataConnectorPipeClient Instance()
        {
            
                if (_instance == null)
                    _instance = new ScoringProgramDataConnectorPipeClient();
                return _instance;
        }

        /// <summary>
        /// Initializes the class.
        /// </summary>
        protected ScoringProgramDataConnectorPipeClient()
        {
            _pipeClient = new ScoringProgramDataConnectorPipeClientConnectionManager();
        }
        // End of singleton implementation

        #region pipeClientwrapper

        private readonly ScoringProgramDataConnectorPipeClientConnectionManager _pipeClient;

        /// <summary>
        /// Can be used to see if there already is a connection to the Data Connector. If so, do not try to connect again.
        /// </summary>
        public override bool IsActive => _pipeClient.IsActive;

        private StreamWriter DataConnectorWriter => _pipeClient.DataConnectorWriter;
        private StreamReader DataConnectorReader => _pipeClient.DataConnectorReader;

        private void CloseWriter() => _pipeClient.CloseWriter();
        private void CloseReader() => _pipeClient.CloseReader();
        private void CloseConnection() => _pipeClient.CloseConnection();
        private NamedPipeClientStream DataConnectorStream => _pipeClient.DataConnectorStream;

        /// <summary>
        /// Connects to the specified named pipe synchronously.
        /// </summary>
        /// <param name="pipeName">The name of the pipe to connect to.</param>
        public override (DataConnectorResponseData result, string message, ErrorType errorType) Connect(string pipeName) => _pipeClient.Connect(pipeName);

        /// <summary>
        /// Connects to the specified named pipe synchronously.
        /// </summary>
        /// <param name="pipeName">The name of the pipe to connect to.</param>
        public async override Task<(DataConnectorResponseData result, string message, ErrorType errorType)>
            ConnectAsync(string pipeName) => await _pipeClient.ConnectAsync(pipeName);


        #endregion

        /// <summary>
        /// Connect to the Data Connector asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task<ScoringProgramResponse> ConnectAsync()
        {
            try
            {
                (DataConnectorResponseData result, string message, ErrorType errorType) result;
                var success = BridgemateDataConnectorManager.EnsureDataConnectorServiceIsRunning(forceRestart: false);
                if (!success)
                {
                    return new ScoringProgramResponse
                    {
                        RequestCommand = ScoringProgramDataConnectorCommands.Connect,
                        DataType = DataConnectorResponseData.Error,
                        ErrorType = ErrorType.NoConnection,
                        SerializedData = JsonSerializer.Serialize("The Bridgemate Data Connector is not running and could not be restarted.")
                    };
                }
                result = await ConnectAsync(PipeName);
                return new ScoringProgramResponse
                {
                    RequestCommand = ScoringProgramDataConnectorCommands.Connect,
                    DataType = result.result,
                    ErrorType = result.errorType,
                    SerializedData = JsonSerializer.Serialize(result.message)
                };
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new ScoringProgramResponse
                {
                    RequestCommand = ScoringProgramDataConnectorCommands.Connect,
                    DataType = DataConnectorResponseData.Error,
                    SerializedData = JsonSerializer.Serialize(ex.Message)
                };
            }
        }

        /// <summary>
        /// Connect to the Data Connector synchronously
        /// </summary>
        /// <returns></returns>
        public ScoringProgramResponse Connect()
        {
            (DataConnectorResponseData result, string message, ErrorType errorType) result;
            var success = BridgemateDataConnectorManager.EnsureDataConnectorServiceIsRunning(forceRestart: false);
            if (!success)
            {
                return new ScoringProgramResponse
                {
                    RequestCommand = ScoringProgramDataConnectorCommands.Connect,
                    DataType = DataConnectorResponseData.Error,
                    ErrorType = ErrorType.NoConnection,
                    SerializedData = JsonSerializer.Serialize("The Bridgemate Data Connector is not running and could not be restarted.")
                };
            }
            try
            {
                result = _pipeClient.Connect(PipeName);
                return new ScoringProgramResponse
                {
                    RequestCommand = ScoringProgramDataConnectorCommands.Connect,
                    DataType = result.result,
                    ErrorType = result.errorType,
                    SerializedData = JsonSerializer.Serialize(result.message)
                };
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new ScoringProgramResponse
                {
                    RequestCommand = ScoringProgramDataConnectorCommands.Connect,
                    DataType = DataConnectorResponseData.Error,
                    SerializedData = JsonSerializer.Serialize(ex.Message)
                };
            }
        }

        /// <summary>
        /// Disconnects from the Data Connector asynchronously, causing the Data Connector to close the pipe and reopen it.
        /// </summary>
        /// <returns></returns>
        public async Task<ScoringProgramResponse> DisconnectAsync()
        {
            LogMethodEntry(nameof(DisconnectAsync));

            //The pipe only supports one request being sent at the same time.
            if (IsSending)
            {
                return new ScoringProgramResponse
                {
                    RequestCommand = ScoringProgramDataConnectorCommands.Disconnect,
                    DataType = DataConnectorResponseData.Error,
                    ErrorType = ErrorType.Busy,
                    SerializedData = JsonSerializer.Serialize("Data is being sent from another command. Try again later.")
                };
            }
            try
            {
                if (DataConnectorWriter != null)
                {
                    var request = new ScoringProgramRequest { Command = ScoringProgramDataConnectorCommands.Disconnect };
                    var requestSerialized = JsonSerializer.Serialize(request);
                    await DataConnectorWriter.WriteLineAsync(requestSerialized);
                    CloseWriter();
                }
                if (DataConnectorReader != null)
                {
                    try
                    {
                        //Usually the code crashses on the line below because the connection has died. So that is expected behaviour.
                        //If an answer comes it will be an error message.
                        var response = await DataConnectorReader.ReadLineAsync();
                        if (response != null)
                        {
                            var responseDeserialized = JsonSerializer.Deserialize<ScoringProgramResponse>(response);
                        }
                    }
                    catch
                    {
                    }
                    CloseReader();
                }
                CloseConnection();
                return new ScoringProgramResponse
                {
                    RequestCommand = ScoringProgramDataConnectorCommands.Disconnect,
                    DataType = DataConnectorResponseData.OK,
                    SerializedData = "Data Connector stopped."
                };
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new ScoringProgramResponse
                {
                    RequestCommand = ScoringProgramDataConnectorCommands.Disconnect,
                    DataType = DataConnectorResponseData.Error,
                    SerializedData = ex.ToString()
                };
            }
            finally { CloseWriter(); }
        }

        /// <summary>
        /// Disconnects from the Data Connector synchronously, causing the Data Connector to close the pipe and reopen it.
        /// </summary>
        /// <returns></returns>
        public ScoringProgramResponse Disconnect()
        {
            LogMethodEntry(nameof(Disconnect));

            //Do not proceed if sending is already in progress (for an other request). There can be only on request be sent at the same time.
            if (IsSending)
            {
                return new ScoringProgramResponse
                {
                    RequestCommand = ScoringProgramDataConnectorCommands.Disconnect,
                    DataType = DataConnectorResponseData.Error,
                    ErrorType = ErrorType.Busy,
                    SerializedData = JsonSerializer.Serialize("Data is being sent from another command. Try again later.")
                };
            }
            try
            {
                if (DataConnectorWriter != null)
                {
                    var request = new ScoringProgramRequest { Command = ScoringProgramDataConnectorCommands.Disconnect };
                    var requestSerialized = JsonSerializer.Serialize(request);
                    DataConnectorWriter.WriteLine(requestSerialized);
                    CloseWriter();
                }
                if (DataConnectorReader != null)
                {
                    try
                    {
                        //Usually the code crashses on the line below because the connection has died. So that is expected behaviour.
                        //If an answer comes it will be an error message.
                        var response = DataConnectorReader.ReadLine();
                        if (response != null)
                        {
                            var responseDeserialized = JsonSerializer.Deserialize<ScoringProgramResponse>(response);
                        }
                    }
                    catch
                    {
                    }
                    CloseReader();
                }
                CloseConnection();
                return new ScoringProgramResponse
                {
                    RequestCommand = ScoringProgramDataConnectorCommands.Disconnect,
                    DataType = DataConnectorResponseData.OK,
                    SerializedData = "Data Connector stopped."
                };
            }
            catch (Exception ex)
            {
                LogError(ex);
                return new ScoringProgramResponse
                {
                    RequestCommand = ScoringProgramDataConnectorCommands.Disconnect,
                    DataType = DataConnectorResponseData.Error,
                    SerializedData = ex.ToString()
                };
            }
            finally { CloseWriter(); }
        }

        /// <summary>
        /// The code that handles the actual sending of requests and reading their reponses aynchronously.
        /// </summary>
        /// <param name="sessionGuid">Specifies which session the request targets (if any)</param>
        /// <param name="command">The command to the Data Connector</param>
        /// <param name="serializedData">The data to send to the Data Connector as json data. (If any)</param>
        /// <returns></returns>
        protected override async Task<ScoringProgramResponse> SendDataAsync(string sessionGuid,
            ScoringProgramDataConnectorCommands command, string serializedData, [CallerMemberName] string caller="")
        {
            var logRecord = new DataConnectorLogRecord(DataConnectorLogLevel.Debug, LoggingSource, command, serializedData, caller);
            DataConnectorClientLogger.LogRecord(logRecord);

            //Construct the request to the Data Connector.
            var request = new ScoringProgramRequest
            {
                Command = command,
                SessionGuid = sessionGuid,
                SerializedData = serializedData
            };

            //Serialize it.
            var requestSerialized = JsonSerializer.Serialize(request);

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

                //Do not continue if the connection has been broken. Call the Connect method again then resend.
                var errorResponse = CheckConnection(command);
                if (errorResponse != null)
                {
                    return errorResponse;
                }

                //Reconnect to the Data Connector if needed.
                if (!DataConnectorStream.IsConnected)
                {
                    await DataConnectorStream.ConnectAsync(5000);
                }

                //Send the request to the Data Connector. Mind: as it is written now this a blocking call.
                //However, in .Net an exception will be thrown if the connection has gone dead for whatever reason.
                //Mind: the Data ConnectorWriter is defined in the base class.
                await DataConnectorWriter.WriteLineAsync(requestSerialized);

                //Wait for the responses. This too is a blocking call. But in .Net a broken connection will throw an exception.
                //Mind: the Data ConnectorReader is defined in the base class.
                var response = await DataConnectorReader.ReadLineAsync();
                if (response != null)
                {
                    var responseDeserialized = JsonSerializer.Deserialize<ScoringProgramResponse>(response);
                    return responseDeserialized ??
                             new ScoringProgramResponse
                             {
                                 RequestCommand = command,
                                 DataType = DataConnectorResponseData.Error,
                                 SerializedData = JsonSerializer.Serialize("Empty response")
                             };
                }
                else
                {
                    return new ScoringProgramResponse
                    {
                        RequestCommand = command,
                        DataType = DataConnectorResponseData.Error,
                        SerializedData = JsonSerializer.Serialize("Empty response")
                    };
                }
            }
            catch (IOException ioex)
            {
                //CloseConnection is defined in the base class.
                CloseConnection();
                return
                new ScoringProgramResponse
                {
                    RequestCommand = command,
                    DataType = DataConnectorResponseData.Error,
                    SerializedData = JsonSerializer.Serialize($"Pipe broken: '{ioex}'")
                };
            }
            catch (Exception ex)
            {
                CloseConnection();
                LogError(ex);
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
        /// Checks if the connection has been established after a Connect command.
        /// </summary>
        /// <param name="command">The command that will be executed by the calling code it the connection is alive.
        ///                       Is passed only for logging purposes.</param>
        /// <returns>A Clientresponse when there is an error, null otherwise.</returns>
        /// <remarks>
        /// Alternatively the Ping command can be used to check the connection before sending a request.
        /// </remarks>
        private ScoringProgramResponse CheckConnection(ScoringProgramDataConnectorCommands command)
        {
            if (DataConnectorStream == null)
                return new ScoringProgramResponse
                {
                    RequestCommand = command,
                    DataType = DataConnectorResponseData.Error,
                    ErrorType = ErrorType.NoConnection,
                    SerializedData = JsonSerializer.Serialize($"{nameof(DataConnectorStream)}  is null.")
                };

            if (DataConnectorReader == null)
                return new ScoringProgramResponse
                {
                    RequestCommand = command,
                    DataType = DataConnectorResponseData.Error,
                    ErrorType = ErrorType.NoConnection,
                    SerializedData = JsonSerializer.Serialize($"{nameof(DataConnectorReader)}  is null.")
                };

            if (DataConnectorWriter == null)
                return new ScoringProgramResponse
                {
                    RequestCommand = command,
                    DataType = DataConnectorResponseData.Error,
                    ErrorType = ErrorType.NoConnection,
                    SerializedData = JsonSerializer.Serialize($"{nameof(DataConnectorWriter)}   is null.")
                };

            return null;
        }

        /// <summary>
        /// Disposes the class.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            _instance = null;
        }

        /// <summary>
        /// The code that handles the actual sending of requests and reading their reponses aynchronously.
        /// </summary>
        /// <param name="sessionGuid">Specifies which session the request targets (if any)</param>
        /// <param name="command">The command to the Data Connector</param>
        /// <param name="serializedData">The data to send to the Data Connector as json data. (If any)</param>
        /// <returns></returns>
        protected override ScoringProgramResponse SendData(string sessionGuid, ScoringProgramDataConnectorCommands command, string serializedData,
                                                           [CallerMemberName] string caller = "")
        {
            var logRecord = new DataConnectorLogRecord(DataConnectorLogLevel.Debug, LoggingSource, command, serializedData, caller);
            DataConnectorClientLogger.LogRecord(logRecord);

            //Construct the request to the Data Connector.
            var request = new ScoringProgramRequest
            {
                Command = command,
                SessionGuid = sessionGuid,
                SerializedData = serializedData
            };

            //Serialize it.
            var requestSerialized = JsonSerializer.Serialize(request);

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

                //Do not continue if the connection has been broken. Call the Connect method again then resend.
                var errorResponse = CheckConnection(command);
                if (errorResponse != null)
                {
                    return errorResponse;
                }

                //Reconnect to the Data Connector if needed.
                if (!DataConnectorStream.IsConnected)
                {
                    DataConnectorStream.Connect(5000);
                }

                //Send the request to the Data Connector. Mind: as it is written now this a blocking call.
                //However, in .Net an exception will be thrown if the connection has gone dead for whatever reason.
                //Mind: the Data ConnectorWriter is defined in the base class.
                DataConnectorWriter.WriteLine(requestSerialized);

                //Wait for the responses. This too is a blocking call. But in .Net a broken connection will throw an exception.
                //Mind: the Data ConnectorReader is defined in the base class.
                var response = DataConnectorReader.ReadLine();
                if (response != null)
                {
                    var responseDeserialized = JsonSerializer.Deserialize<ScoringProgramResponse>(response);
                    return responseDeserialized ??
                             new ScoringProgramResponse
                             {
                                 RequestCommand = command,
                                 DataType = DataConnectorResponseData.Error,
                                 SerializedData = JsonSerializer.Serialize("Empty response")
                             };
                }
                else
                {
                    return new ScoringProgramResponse
                    {
                        RequestCommand = command,
                        DataType = DataConnectorResponseData.Error,
                        SerializedData = JsonSerializer.Serialize("Empty response")
                    };
                }
            }
            catch (IOException ioex)
            {
                //CloseConnection is defined in the base class.
                CloseConnection();
                return
                new ScoringProgramResponse
                {
                    RequestCommand = command,
                    DataType = DataConnectorResponseData.Error,
                    ErrorType= ErrorType.NoConnection,
                    SerializedData = JsonSerializer.Serialize($"Pipe broken: '{ioex}'")
                };
            }
            catch (Exception ex)
            {
                CloseConnection();
                LogError(ex);
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

    }

}
