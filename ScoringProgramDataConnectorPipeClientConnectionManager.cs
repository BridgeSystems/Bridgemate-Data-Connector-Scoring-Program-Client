using Microsoft.Extensions.Logging;
using System;

namespace BridgeSystems.Bridgemate.DataConnector.ScoringProgramClient
{
    /// <summary>
    /// Handles connections to the dataconnector for scoring programs.
    /// </summary>
    public class ScoringProgramDataConnectorPipeClientConnectionManager : DataConnectorPipeClientConnectionManager<ScoringProgramDataConnectorCommands>
    {
        /// <summary>
        /// Returns the logging source.
        /// </summary>
        public override DataConnectorLoggingSource LoggingSource => DataConnectorLoggingSource.ScoringProgramClient;

        /// <summary>
        /// The logger.
        /// </summary>
        public static readonly ILogger ScoringProgramClientLogger = DataConnectorLogging.LoggerFactory.CreateLogger(nameof(ScoringProgramClientLogger));

        /// <summary>
        /// Logs an exception.
        /// </summary>
        /// <param name="ex"></param>
        protected override void LogError(Exception ex)
        {
            ScoringProgramClientLogger.LogError(ex, ex.Message);
        }

        /// <summary>
        /// Logs entry of a method.
        /// </summary>
        /// <param name="entry"></param>
        protected override void LogMethodEntry(string entry)
        {
            ScoringProgramClientLogger.LogDebug(entry);
        }
    }
}