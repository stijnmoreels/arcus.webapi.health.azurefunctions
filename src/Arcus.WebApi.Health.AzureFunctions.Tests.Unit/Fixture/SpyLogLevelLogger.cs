using System;
using Microsoft.Extensions.Logging;

namespace Arcus.WebApi.Health.AzureFunctions.Tests.Unit.Fixture
{
    /// <summary>
    /// Represents an <see cref="ILogger"/> implementation that spies on a written log messages based on a specified <see cref="LogLevel"/>.
    /// </summary>
   public class SpyLogLevelLogger : ILogger
    {
        private readonly LogLevel _logLevel;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SpyLogLevelLogger" /> class.
        /// </summary>
        /// <param name="logLevel">The level at which the logger should write it's messages.</param>
        public SpyLogLevelLogger(LogLevel logLevel)
        {
            _logLevel = logLevel;
        }
        
        /// <summary>
        /// Gets the flag indicating whether or not the logger was able to write the log message according to the configured <see cref="LogLevel"/>.
        /// </summary>
        public bool IsLogMessageWritten { get; private set; }

        /// <summary>
        /// Writes a log entry.
        /// </summary>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">Id of the event.</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a <see cref="T:System.String" /> message of the <paramref name="state" /> and <paramref name="exception" />.</param>
        /// <typeparam name="TState">The type of the object to be written.</typeparam>
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            IsLogMessageWritten = true;
        }

        /// <summary>
        /// Checks if the given <paramref name="logLevel" /> is enabled.
        /// </summary>
        /// <param name="logLevel">level to be checked.</param>
        /// <returns><c>true</c> if enabled.</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return _logLevel == logLevel;
        }

        /// <summary>
        /// Begins a logical operation scope.
        /// </summary>
        /// <param name="state">The identifier for the scope.</param>
        /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
        /// <returns>An <see cref="T:System.IDisposable" /> that ends the logical operation scope on dispose.</returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }
    }
}
