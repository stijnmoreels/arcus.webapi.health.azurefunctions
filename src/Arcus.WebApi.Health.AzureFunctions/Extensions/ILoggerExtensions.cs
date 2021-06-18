//Copyright(c) .NET Foundation and Contributors

using System;
using Arcus.WebApi.Health.AzureFunctions.Logging;
using GuardNet;
using Microsoft.Extensions.Diagnostics.HealthChecks;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Represents extensions on the <see cref="ILogger"/> that writes pre-defined messages regarding health checks.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class ILoggerExtensions
    {
        private static readonly Action<ILogger, Exception> HealthCheckProcessingBegin = 
            LoggerMessage.Define(
                LogLevel.Debug,
                EventIds.HealthCheckProcessingBegin,
                "Running health checks");

        private static readonly Action<ILogger, double, HealthStatus, Exception> HealthCheckProcessingEnd = 
            LoggerMessage.Define<double, HealthStatus>(
                LogLevel.Debug,
                EventIds.HealthCheckProcessingEnd,
                "Health check processing completed after {ElapsedMilliseconds}ms with combined status {HealthStatus}");

        private static readonly Action<ILogger, string, Exception> HealthCheckBegin = 
            LoggerMessage.Define<string>(
                LogLevel.Debug,
                EventIds.HealthCheckBegin,
                "Running health check {HealthCheckName}");

        // These are separate so they can have different log levels
        private static readonly string HealthCheckEndText = 
            "Health check {HealthCheckName} completed after {ElapsedMilliseconds}ms with status {HealthStatus} and '{HealthCheckDescription}'";

        private static readonly Action<ILogger, string, double, HealthStatus, string, Exception> HealthCheckEndHealthy = 
            LoggerMessage.Define<string, double, HealthStatus, string>(
                LogLevel.Debug,
                EventIds.HealthCheckEnd,
                HealthCheckEndText);

        private static readonly Action<ILogger, string, double, HealthStatus, string, Exception> HealthCheckEndDegraded = 
            LoggerMessage.Define<string, double, HealthStatus, string>(
                LogLevel.Warning,
                EventIds.HealthCheckEnd,
                HealthCheckEndText);

        private static readonly Action<ILogger, string, double, HealthStatus, string, Exception> HealthCheckEndUnhealthy = 
            LoggerMessage.Define<string, double, HealthStatus, string>(
                LogLevel.Error,
                EventIds.HealthCheckEnd,
                HealthCheckEndText);

        private static readonly Action<ILogger, string, double, HealthStatus, string, Exception> HealthCheckEndFailed = 
            LoggerMessage.Define<string, double, HealthStatus, string>(
                LogLevel.Error,
                EventIds.HealthCheckEnd,
                HealthCheckEndText);

        private static readonly Action<ILogger, string, double, Exception> HealthCheckError = 
            LoggerMessage.Define<string, double>(
                LogLevel.Error,
                EventIds.HealthCheckError,
                "Health check {HealthCheckName} threw an unhandled exception after {ElapsedMilliseconds}ms");

        /// <summary>
        /// Writes a pre-defined log message to describe the begin of the health check process.
        /// </summary>
        /// <param name="logger">The logger to write the log message to.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        public static void LogHealthCheckProcessingBegin(this ILogger logger)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the message describing the begin of the health check process");
            HealthCheckProcessingBegin(logger, null);
        }

        /// <summary>
        /// Writes a pre-defined log message to describe the end of the health check process.
        /// </summary>
        /// <param name="logger">The logger to write the log message to.</param>
        /// <param name="status">The end-result health check status of the health check process.</param>
        /// <param name="duration">The total duration it took to run the health check process.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the <paramref name="status"/> is outside the bounds of the enumeration
        ///     or the <paramref name="duration"/> is a negative time range.
        /// </exception>
        public static void LogHealthCheckProcessingEnd(this ILogger logger, HealthStatus status, TimeSpan duration)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the message describing the end of the health check process");
            Guard.For(() => !Enum.IsDefined(typeof(HealthStatus), status), 
                new ArgumentOutOfRangeException(nameof(status), status, "Requires a health check status that is within the bounds of the enumeration"));
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a total duration that represents a positive time range");
            
            HealthCheckProcessingEnd(logger, duration.TotalMilliseconds, status, null);
        }

        /// <summary>
        /// Writes a pre-defined log message to describe the begin of a single health check.
        /// </summary>
        /// <param name="logger">The logger to write the log message to.</param>
        /// <param name="registration">The health check registration that describes what should be verified during the health check.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="registration"/> is <c>null</c>.</exception>
        public static void LogHealthCheckBegin(this ILogger logger, HealthCheckRegistration registration)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the message describing the begin of a single health check");
            Guard.NotNull(registration, nameof(registration), "Requires a health check registration that describes what should be verified during the health check");
            
            HealthCheckBegin(logger, registration.Name, null);
        }

        /// <summary>
        /// Writes a pre-defined log message to describe the end of a single health check.
        /// </summary>
        /// <param name="logger">The logger to write the log message to.</param>
        /// <param name="registration">The health check registration that describes what was verified during the health check.</param>
        /// <param name="entry">The resulting health entry report that describes the health of the single ended health check.</param>
        /// <param name="duration">The total duration it took to verify this single health check.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="registration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="entry"/> is the <c>default</c> value.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the <paramref name="duration"/> is a negative time range
        ///     or the <paramref name="entry"/> has an unknown <see cref="HealthStatus"/> value for the <see cref="HealthReportEntry.Status"/>.
        /// </exception>
        public static void LogHealthCheckEnd(this ILogger logger, HealthCheckRegistration registration, HealthReportEntry entry, TimeSpan duration)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the message describing the end of a single health check");
            Guard.NotNull(registration, nameof(registration), "Requires a health check registration that describes what was verified during the health check");
            Guard.For(() => entry.Equals(default(HealthReportEntry)),
                new ArgumentException("Requires a non-default health report entry that describes the health of the single ended health check", nameof(entry)));
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time range for the total duration it took to verify this single health check");
            
            switch (entry.Status)
            {
                case HealthStatus.Healthy:
                    HealthCheckEndHealthy(logger, registration.Name, duration.TotalMilliseconds, entry.Status, entry.Description, null);
                    break;

                case HealthStatus.Degraded:
                    HealthCheckEndDegraded(logger, registration.Name, duration.TotalMilliseconds, entry.Status, entry.Description, null);
                    break;

                case HealthStatus.Unhealthy:
                    HealthCheckEndUnhealthy(logger, registration.Name, duration.TotalMilliseconds, entry.Status, entry.Description, null);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), entry.Status, 
                        "Requires a health status in the resulting health report entry which has a known value within the bounds of the enumeration");
            }
        }

        /// <summary>
        /// Writes a pre-defined log message to describe an occurred error during a single health check.
        /// </summary>
        /// <param name="logger">The logger to write the log message to.</param>
        /// <param name="registration">The health check registration that describes what should have been verified during the health check.</param>
        /// <param name="exception">The occurred exception during the health check.</param>
        /// <param name="duration">The total duration it took until the <paramref name="exception"/> happened.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="logger"/>, the <paramref name="registration"/>, or the <paramref name="exception"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="duration"/> is a negative time range.</exception>
        public static void LogHealthCheckError(this ILogger logger, HealthCheckRegistration registration, Exception exception, TimeSpan duration)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the message describing the faulted health check");
            Guard.NotNull(registration, nameof(registration), "Requires a health check registration that describes what should have been verified during the health check");
            Guard.NotNull(exception, nameof(exception), "Requires an exception that occurred during the health check");
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time range for the total duration it took until the exception happened");
            
            HealthCheckError(logger, registration.Name, duration.TotalMilliseconds, exception);
        }

        /// <summary>
        /// Writes a pre-defined log message to describe an occurred error during a single health check.
        /// </summary>
        /// <param name="logger">The logger to write the log message to.</param>
        /// <param name="registration">The health check registration that describes what should have been verified during the health check.</param>
        /// <param name="entry">The resulting health entry report that describes why the health of the single health check failed.</param>
        /// <param name="duration">The total duration it took until the health check failed.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="registration"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="entry"/> is the <c>default</c> value.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when the <paramref name="duration"/> is a negative duration
        ///     or the <paramref name="entry"/> has an unknown <see cref="HealthStatus"/> value for the <see cref="HealthReportEntry.Status"/>.
        /// </exception>
        public static void LogHealthCheckEndFailed(this ILogger logger, HealthCheckRegistration registration, HealthReportEntry entry, TimeSpan duration)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the message describing the faulted health check");
            Guard.NotNull(registration, nameof(registration), "Requires a health check registration that describes what should have been verified during the health check");
            Guard.For(() => entry.Equals(default(HealthReportEntry)),
                new ArgumentException("Requires a non-default health report entry that describes why the health of the health check failed"));
            Guard.NotLessThan(duration, TimeSpan.Zero, nameof(duration), "Requires a positive time range for the total duration it took until the health check failed");
            Guard.For(() => !Enum.IsDefined(typeof(HealthStatus), entry.Status),
                new ArgumentOutOfRangeException(nameof(entry), entry.Status,
                    "Requires a health check status for the health check report entry that is within the bounds of the enumeration"));
            
            HealthCheckEndFailed(logger, registration.Name, duration.TotalMilliseconds, entry.Status, entry.Description, null);
        }

        /// <summary>
        /// Writes a pre-defined log message that describes the resulting health report entry's data.
        /// </summary>
        /// <remarks>
        ///     This log message will only be shown when the <paramref name="logger"/> has the log level <see cref="LogLevel.Debug"/> enabled.
        /// </remarks>
        /// <param name="logger">The logger instance to write the log message to.</param>
        /// <param name="registration">The health check registration that describes what should be verified during the health check.</param>
        /// <param name="entry">The resulting health report entry that contains additional data that will be logged.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or the <paramref name="registration"/> is <c>null</c>.</exception>
        public static void LogHealthCheckData(this ILogger logger, HealthCheckRegistration registration, HealthReportEntry entry)
        {
            Guard.NotNull(logger, nameof(logger), "Requires an logger instance to write the message describing the additional health report entry data");
            Guard.NotNull(registration, nameof(registration), "Requires a health check registration that describes what should be verified during the health check");
            Guard.For(() => entry.Equals(default(HealthReportEntry)),
                new ArgumentException("Requires a non-default health report entry that holds possible additional data for the health check"));
            
            if (entry.Data.Count > 0 && logger.IsEnabled(LogLevel.Debug))
            {
                logger.Log(
                    logLevel: LogLevel.Debug,
                    eventId: EventIds.HealthCheckData,
                    state: new HealthCheckDataLogValue(registration.Name, entry.Data),
                    exception: null,
                    formatter: (state, ex) => state.ToString());
            }
        }
    }
}