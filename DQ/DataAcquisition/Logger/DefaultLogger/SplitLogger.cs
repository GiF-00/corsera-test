using System;

namespace DataAcquisition.Logger.DefaultLogger
{
    /// <summary>
    /// Class <c>SplitLogger</c> implements the <c>ILogger</c> interface to split incoming
    /// log messages amongst two or more implementations of <c>ILogger</c> interface.
    /// </summary>
    public class SplitLogger : ILogger
    {
        private readonly ILogger firstLogger;
        private readonly ILogger secondLogger;
        private readonly ILogger[]? loggers;

        /// <summary>
        /// Initializes the class by providing two or more implementations
        /// of <c>ILogger</c> interface.
        /// </summary>
        /// <param name="firstLogger">Required first instance of <c>ILogger</c> interface implementation.</param>
        /// <param name="secondLogger">Required second instance of <c>ILogger</c> interface implementation.</param>
        /// <param name="loggers">Any number of instances of <c>ILogger</c> interface implementations.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SplitLogger(ILogger firstLogger, ILogger secondLogger, params ILogger[] loggers)
        {
            this.firstLogger = firstLogger ?? throw new ArgumentNullException(nameof(firstLogger));
            this.secondLogger = secondLogger ?? throw new ArgumentNullException(nameof(secondLogger));
            this.loggers = loggers;
        }

        public void Dispose()
        {
            if (firstLogger is IDisposable firstDisposable) { firstDisposable.Dispose(); }
            if (secondLogger is IDisposable secondDisposable) { secondDisposable.Dispose(); }

            if (loggers != null && loggers.Length > 0)
            {
                foreach (var logger in loggers)
                {
                    if (logger != null && logger is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }

        public void Info(params string[] messages)
        {
            firstLogger?.Info(messages);
            secondLogger?.Info(messages);

            if (loggers != null && loggers.Length > 0)
            {
                foreach (var logger in loggers)
                {
                    logger?.Info(messages);
                }
            }
        }

        public void Warning(params string[] messages)
        {
            firstLogger?.Warning(messages);
            secondLogger?.Warning(messages);

            if (loggers != null && loggers.Length > 0)
            {
                foreach (var logger in loggers)
                {
                    logger?.Warning(messages);
                }
            }
        }

        public void Error(params string[] messages)
        {
            firstLogger.Error(messages);
            secondLogger.Error(messages);

            if (loggers != null && loggers.Length > 0)
            {
                foreach (var logger in loggers)
                {
                    logger?.Error(messages);
                }
            }
        }
    }
}
