//Copyright(c) .NET Foundation and Contributors

using System;
using GuardNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Arcus.WebApi.Health.AzureFunctions
{
    /// <summary>
    /// Represents the builder to add health checks to the system.
    /// </summary>
    public class HealthChecksBuilder : IHealthChecksBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HealthChecksBuilder"/> class.
        /// </summary>
        /// <param name="services">The services collection to add the health checks to.</param>
        public HealthChecksBuilder(IServiceCollection services)
        {
            Guard.NotNull(services, nameof(services), "Requires a collection of dependency services to add the health checks");
            Services = services;
        }

        /// <summary>
        /// Gets the current service collection where the health checks will be added.
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Adds an <see cref="HealthCheckRegistration"/> instance to the <see cref="Services"/> so the health check gets run upon checking the health.
        /// </summary>
        /// <param name="registration">The health check registration, containing the health check to run.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="registration"/> is <c>null</c>.</exception>
        public IHealthChecksBuilder Add(HealthCheckRegistration registration)
        {
            Guard.NotNull(registration, nameof(registration), "Requires a health check registration instance containing a health check to add it to the services collection");

            Services.Configure<HealthCheckServiceOptions>(options =>
            {
                options.Registrations.Add(registration);
            });

            return this;
        }
    }
}
