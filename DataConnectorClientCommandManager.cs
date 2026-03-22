using System;
using System.Threading.Tasks;

namespace BridgeSystems.Bridgemate.DataConnector.ScoringProgramClient
{

    /// <summary>
    /// Defines shared code for client classes for the dataconnector. Clients for the scoringprogram and BCS both inherit from this class.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public abstract class DataConnectorClientCommandManager<TCommand> where TCommand : Enum
    {
        /// <summary>
        /// Then application name
        /// </summary>
        public const string ApplicationName = "BCS.Net";
        /// <summary>
        /// The default time to wait for a connection attempt to succeed.
        /// </summary>
        public static int DefaultTimeOutInMilliSeconds = 5000;
        /// <summary>
        /// Structured logger for DataConnector actions.
        /// </summary>
        public DataConnectorLogCreator<TCommand> DataConnectorClientLogger = new DataConnectorLogCreator<TCommand>(jsonDataLogLevel: DataConnectorLogLevel.Debug, nameof(DataConnectorClientLogger));

        /// <summary>
        /// Returns true if the
        /// </summary>
        public abstract bool IsActive { get; }

        private int? _timeOutInMilliSeconds;
        /// <summary>
        /// The timeout for establishing a connection with the Data Connector.
        /// If not set the default value DefaultTimeOutInMilliSeconds will be used.
        /// </summary>
        public int TimeOutInMilliSeconds
        {
            get => _timeOutInMilliSeconds ?? DefaultTimeOutInMilliSeconds;
            set => _timeOutInMilliSeconds = value;
        }

        /// <summary>
        /// The source for the logging: Client or Server, BCS or ScoringProgram.
        /// </summary>
        public abstract DataConnectorLoggingSource LoggingSource { get; }

        /// <summary>
        /// Disposes the class.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the class.
        /// </summary>
        /// <param name="disposing"></param>
        protected abstract void Dispose(bool disposing);    

        /// <summary>
        /// Connects to the dataconnector synchronously.
        /// </summary>
        /// <param name="connectionName">data needed to locate the dataconnector</param>
        /// <returns></returns>
        public abstract (DataConnectorResponseData result, string message, ErrorType errorType) Connect(string connectionName);

        /// <summary>
        /// Connects to the dataconnector asynchronously.
        /// </summary>
        /// <param name="connectionName">data needed to locate the dataconnector</param>
        /// <returns></returns>
        public abstract Task<(DataConnectorResponseData result, string message, ErrorType errorType)> ConnectAsync(string connectionName);

        /// <summary>
        /// Logs an error
        /// </summary>
        /// <param name="ex"></param>
        protected abstract void LogError(Exception ex);

        /// <summary>
        /// Logs the entry to a method.
        /// </summary>
        /// <param name="entry"></param>
        protected abstract void LogMethodEntry(string entry);


        /// <summary>
        /// Logs the entry to a method with its parameters (if any),
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        public void LogMethodEntry(string methodName, params (string parameterName, object parameterValue)[] parameters)
        {
            var logEntry = DataConnectorClientLogger.CreateMethodEntryLog(methodName, LoggingSource, parameters);
            LogMethodEntry(logEntry);
        }
    }
}