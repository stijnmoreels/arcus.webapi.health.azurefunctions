using Bogus;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Arcus.WebApi.Health.AzureFunctions.Tests.Unit.Fixture
{
    /// <summary>
    /// Represents a model to generate <see cref="HealthCheckRegistration"/> instances.
    /// </summary>
    public static class HealthCheckRegistrationGenerator
    {
        private static readonly Faker BogusGenerator = new Faker();

        /// <summary>
        /// Generates an <see cref="HealthCheckRegistration"/> instance with a generated <see cref="HealthStatus"/> end-result status.
        /// </summary>
        public static HealthCheckRegistration GenerateHealthCheckRegistration()
        {
            var status = BogusGenerator.Random.Enum<HealthStatus>();
            HealthCheckRegistration registration = GenerateHealthCheckRegistration(status);

            return registration;
        }
        
        /// <summary>
        /// Generates an <see cref="HealthCheckRegistration"/> instance with a given <paramref name="healthStatus"/> end-result status.
        /// </summary>
        /// <param name="healthStatus">The end-result health status of the health check.</param>
        public static HealthCheckRegistration GenerateHealthCheckRegistration(HealthStatus healthStatus)
        {
            IHealthCheck healthCheck = HealthCheckGenerator.GenerateHealthCheck(healthStatus);
            HealthCheckRegistration registration = CreateHealthCheckRegistration(healthCheck);

            return registration;
        }
        
        /// <summary>
        /// Creates an <see cref="HealthCheckRegistration"/> instance from a given <paramref name="healthCheck"/>.
        /// </summary>
        /// <param name="healthCheck">The health check to run.</param>
        public static HealthCheckRegistration CreateHealthCheckRegistration(IHealthCheck healthCheck)
        { 
            string name = BogusGenerator.Internet.DomainName();
            string[] tags = BogusGenerator.Lorem.Words();

            return new HealthCheckRegistration(name, healthCheck, HealthStatus.Unhealthy, tags);
        }
    }
}
