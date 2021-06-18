//Copyright(c) .NET Foundation and Contributors

using Microsoft.Extensions.Logging;

namespace Arcus.WebApi.Health.AzureFunctions.Logging
{
    /// <summary>
    /// Represents all the used event ID's during the health checks.
    /// </summary>
    public static class EventIds
    {
        /// <summary>
        /// Gets the event ID to describe the beginning of the health checks processing.
        /// </summary>
        public static readonly EventId HealthCheckProcessingBegin = new EventId(100, "HealthCheckProcessingBegin");
        
        /// <summary>
        /// Gets the event ID to describe the ending of the health checks processing.
        /// </summary>
        public static readonly EventId HealthCheckProcessingEnd = new EventId(101, "HealthCheckProcessingEnd");

        /// <summary>
        /// Gets the event ID to describe the beginning of a single health check.
        /// </summary>
        public static readonly EventId HealthCheckBegin = new EventId(102, "HealthCheckBegin");
        
        /// <summary>
        /// Gets the event ID to describe the end of a single health check.
        /// </summary>
        public static readonly EventId HealthCheckEnd = new EventId(103, "HealthCheckEnd");
        
        /// <summary>
        /// Gets the event ID to describe an occurred error during a single health check.
        /// </summary>
        public static readonly EventId HealthCheckError = new EventId(104, "HealthCheckError");
        
        /// <summary>
        /// Gets the event ID to describe the health check data of a single health check.
        /// </summary>
        public static readonly EventId HealthCheckData = new EventId(105, "HealthCheckData");
    }
}