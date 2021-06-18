using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Xunit;
using static Arcus.WebApi.Health.AzureFunctions.Tests.Unit.Fixture.HealthCheckRegistrationGenerator;

namespace Arcus.WebApi.Health.AzureFunctions.Tests.Unit
{
    public class HealthCheckBuilderTests
    {
        [Fact]
        public void Create_WithoutServices_Fails()
        {
            Assert.ThrowsAny<ArgumentException>(() => new HealthChecksBuilder(services: null));
        }

        [Fact]
        public void Add_WithoutHealthCheckRegistration_Fails()
        {
            // Arrange
            var services = new ServiceCollection();
            var builder = new HealthChecksBuilder(services);

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => builder.Add(registration: null));
        }

        [Fact]
        public void Add_WithHealthCheckRegistration_Succeeds()
        {
            // Arrange
            HealthCheckRegistration registration = GenerateHealthCheckRegistration();
            var services = new ServiceCollection();
            var builder = new HealthChecksBuilder(services);
            
            // Act
            builder.Add(registration);
            
            // Assert
            IServiceProvider provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<HealthCheckServiceOptions>>();
            Assert.Contains(registration, options.Value.Registrations);
        }
    }
}
