using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;

namespace Arcus.WebApi.Health.AzureFunctions.Tests.Unit.Fixture
{
    /// <summary>
    /// Represents a model to generate <see cref="IHealthCheck"/> implementations.
    /// </summary>
    public static class HealthCheckGenerator
    {
        private static readonly Faker BogusGenerator = new Faker();

        /// <summary>
        /// Generates an <see cref="IHealthCheck"/> implementation with a given <paramref name="healthStatus"/>.
        /// </summary>
        /// <param name="healthStatus">The result health status of the the health check.</param>
        /// <param name="duration">The duration it took to run the health check.</param>
        public static IHealthCheck GenerateHealthCheck(HealthStatus healthStatus, TimeSpan duration = default)
        {
            string description = BogusGenerator.Lorem.Sentence();
                
            Exception exception = null;
            if (healthStatus is HealthStatus.Unhealthy)
            {
                exception = BogusGenerator.System.Exception();
            }

            IDictionary<string, object> data = 
                BogusGenerator.Lorem.Words()
                              .Distinct(StringComparer.OrdinalIgnoreCase)
                              .ToDictionary(word => word, word => (object) BogusGenerator.Random.Int());
                
            var dataReadOnly = new ReadOnlyDictionary<string, object>(data);
                
            var healthCheck = new Mock<IHealthCheck>();
            healthCheck.Setup(check => check.CheckHealthAsync(It.IsAny<HealthCheckContext>(), CancellationToken.None))
                       .Returns(async () =>
                       {
                           await Task.Delay(duration);
                           return new HealthCheckResult(healthStatus, description, exception, dataReadOnly);
                       });

            return healthCheck.Object;
        }
    }
}
