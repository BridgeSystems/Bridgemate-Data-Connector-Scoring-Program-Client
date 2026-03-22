using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BridgeSystems.Bridgemate.DataConnector.ScoringProgramClient
{
    /// <summary>
    /// Central logging configuration for the DataConnector library.
    /// Set the <see cref="LoggerFactory"/> property to enable logging with the framework of your choice (NLog, Serilog, etc.).
    /// If not set, logging is silently disabled.
    /// </summary>
    public static class DataConnectorLogging
    {
        /// <summary>
        /// The logger factory used to create loggers throughout the library.
        /// Defaults to <see cref="NullLoggerFactory.Instance"/> (no-op).
        /// Set this to your own <see cref="ILoggerFactory"/> to enable logging.
        /// </summary>
        public static ILoggerFactory LoggerFactory { get; set; } = NullLoggerFactory.Instance;
    }
}
