using System;

namespace DataAcquisition.Logger
{
    /// <summary>
    /// Provides selection of minimum level of log messages to reach output by
    /// any implementation of <c cref="ILogger">ILogger</c>.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Identifies <c>Information</c>-level log messages.
        /// </summary>
        Information,
        /// <summary>
        /// Identifies <c>Warning</c>-level log messages.
        /// </summary>
        Warning,
        /// <summary>
        /// Identifies <c>Error</c>-level log messages.
        /// </summary>
        Error
    }

    /// <summary>
    /// The interface <c>ILogger</c> provides means to output log messages
    /// by different parts of the application.
    /// </summary>
    public interface ILogger : IDisposable
    {
        /// <summary>
        /// Outputs <c>Information</c>-level (<c cref="LogLevel.Information">LogLevel.Information</c>) log message.
        /// </summary>
        /// <param name="messages">The messages text.</param>
        void Info(params string[] messages);
        /// <summary>
        /// Outputs <c>Warning</c>-level (<c cref="LogLevel.Warning">LogLevel.Warning</c>) log message.
        /// </summary>
        /// <param name="messages">The messages text.</param>
        void Warning(params string[] messages);
        /// <summary>
        /// Outputs <c>Error</c>-level (<c cref="LogLevel.Error">LogLevel.Error</c>) log message.
        /// </summary>
        /// <param name="messages">The messages text.</param>
        void Error(params string[] messages);
    }
}
