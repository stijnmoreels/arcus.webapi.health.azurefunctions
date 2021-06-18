using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Arcus.WebApi.Health.AzureFunctions.Tests.Unit.Fixture;
using Bogus;
using GuardNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static Arcus.WebApi.Health.AzureFunctions.Tests.Unit.Fixture.HealthCheckGenerator;
using static Arcus.WebApi.Health.AzureFunctions.Tests.Unit.Fixture.HealthCheckRegistrationGenerator;

namespace Arcus.WebApi.Health.AzureFunctions.Tests.Unit
{
    public class DefaultHealthCheckServiceTests
    {
        private static readonly Faker BogusGenerator = new Faker();
        
        [Theory]
        [InlineData(HealthStatus.Healthy)]
        [InlineData(HealthStatus.Degraded)]
        [InlineData(HealthStatus.Unhealthy)]
        public async Task CheckHealth_OverallHealthStatus_ResultsInSameHealthStatus(HealthStatus healthStatus)
        {
            // Arrange
            IServiceScopeFactory factory = CreateServiceScopeFactory();
            var options = new HealthCheckServiceOptions();
            
            HealthCheckRegistration[] registrations = GenerateHealthCheckRegistrations(healthStatus);
            Assert.All(registrations, registration => options.Registrations.Add(registration));

            var service = 
                new DefaultHealthCheckService(factory, Options.Create(options), NullLogger<DefaultHealthCheckService>.Instance);

            // Act
            HealthReport report = await service.CheckHealthAsync();
            
            // Assert
            Assert.Equal(healthStatus, report.Status);
            Assert.Equal(registrations.Length, report.Entries.Count);
        }
        
        [Theory]
        [InlineData(HealthStatus.Healthy)]
        [InlineData(HealthStatus.Degraded)]
        [InlineData(HealthStatus.Unhealthy)]
        public async Task CheckHealth_OverallHealthStatusWithNegativePredicate_ResultsInHealthyEmptyReport(HealthStatus healthStatus)
        {
            // Arrange
            IServiceScopeFactory factory = CreateServiceScopeFactory();
            var options = new HealthCheckServiceOptions();
            
            HealthCheckRegistration[] registrations = GenerateHealthCheckRegistrations(healthStatus);
            Assert.All(registrations, registration => options.Registrations.Add(registration));

            var service = 
                new DefaultHealthCheckService(factory, Options.Create(options), NullLogger<DefaultHealthCheckService>.Instance);

            // Act
            HealthReport report = await service.CheckHealthAsync(registration => false);
            
            // Assert
            Assert.Equal(HealthStatus.Healthy, report.Status);
            Assert.Empty(report.Entries);
        }

        [Fact]
        public async Task CheckHealth_SingleUnhealthyHealthStatus_CompromisesOverallHealthStatus()
        {
            // Arrange
            IServiceScopeFactory factory = CreateServiceScopeFactory();
            var options = new HealthCheckServiceOptions();
            
            HealthCheckRegistration[] healthyRegistrations = GenerateHealthCheckRegistrations(HealthStatus.Healthy);
            HealthCheckRegistration unhealthyRegistration = GenerateHealthCheckRegistration(HealthStatus.Unhealthy);
            Assert.All(healthyRegistrations, reg => options.Registrations.Add(reg));
            options.Registrations.Add(unhealthyRegistration);
            
            var service = 
                new DefaultHealthCheckService(factory, Options.Create(options), NullLogger<DefaultHealthCheckService>.Instance);
            
            // Act
            HealthReport report = await service.CheckHealthAsync();

            // Assert
            Assert.Equal(HealthStatus.Unhealthy, report.Status);
            Assert.Equal(healthyRegistrations.Length + 1, report.Entries.Count);
        }

        [Fact]
        public async Task CheckHealth_TracksTheHealthChecks_IntoTotalDuration()
        {
            // Arrange
            IServiceScopeFactory factory = CreateServiceScopeFactory();
            var options = new HealthCheckServiceOptions();

            TimeSpan healthyDuration = TimeSpan.FromSeconds(1), 
                     unhealthyDuration = TimeSpan.FromSeconds(2);
            IHealthCheck healthyCheck = GenerateHealthCheck(HealthStatus.Healthy, healthyDuration),
                         unhealthyCheck = GenerateHealthCheck(HealthStatus.Unhealthy, unhealthyDuration);
            HealthCheckRegistration healthyRegistration = CreateHealthCheckRegistration(healthyCheck),
                                    unhealthyRegistration = CreateHealthCheckRegistration(unhealthyCheck);
            
            options.Registrations.Add(healthyRegistration);
            options.Registrations.Add(unhealthyRegistration);
            
            var service =
                new DefaultHealthCheckService(factory, Options.Create(options), NullLogger<DefaultHealthCheckService>.Instance);
            
            // Act
            HealthReport report = await service.CheckHealthAsync();
            
            // Assert
            Assert.Equal(HealthStatus.Unhealthy, report.Status);
            Assert.Equal(2, report.Entries.Count);
            TimeSpan expectedDuration = healthyDuration + unhealthyDuration;
            Assert.True(expectedDuration < report.TotalDuration);
            Assert.True(expectedDuration + TimeSpan.FromSeconds(1) > report.TotalDuration);
        }

        [Fact]
        public async Task CheckHealth_HealthCheckThrowsException_ResultsInUnhealthyHealthStatus()
        {
            // Arrange
            IServiceScopeFactory factory = CreateServiceScopeFactory();
            var options = new HealthCheckServiceOptions();

            IHealthCheck thrownHealthCheck = GenerateThrownHealthCheck();
            HealthCheckRegistration registration = CreateHealthCheckRegistration(thrownHealthCheck);
            options.Registrations.Add(registration);
            
            var service = 
                new DefaultHealthCheckService(factory, Options.Create(options), NullLogger<DefaultHealthCheckService>.Instance);
            
            // Act
            HealthReport report = await service.CheckHealthAsync();
            
            // Assert
            Assert.Equal(HealthStatus.Unhealthy, report.Status);
            Assert.Single(report.Entries);
        }

        [Fact]
        public async Task CheckHealth_HealthCheckCancelled_DoesntResultInUnhealthy()
        {
            // Arrange
            IServiceScopeFactory factory = CreateServiceScopeFactory();
            var options = new HealthCheckServiceOptions();

            IHealthCheck thrownHealthCheck = CreateThrownHealthCheck(new OperationCanceledException());
            HealthCheckRegistration registration = CreateHealthCheckRegistration(thrownHealthCheck);
            options.Registrations.Add(registration);
            
            var service =
                new DefaultHealthCheckService(factory, Options.Create(options), NullLogger<DefaultHealthCheckService>.Instance);
            
            // Act / Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => service.CheckHealthAsync());
        }

        [Fact]
        public void CreateService_WithDuplicateHealthCheckRegistrationName_Fails()
        {
            // Arrange
            IServiceScopeFactory factory = CreateServiceScopeFactory();
            var options = new HealthCheckServiceOptions();
            HealthCheckRegistration[] healthyRegistrations = GenerateHealthCheckRegistrations(HealthStatus.Healthy);
            
            string[] tags = BogusGenerator.Lorem.Words();
            IHealthCheck healthCheck = GenerateHealthCheck(HealthStatus.Healthy);
            string duplicateName = healthyRegistrations[0].Name;
            var registration = new HealthCheckRegistration(duplicateName, healthCheck, HealthStatus.Unhealthy, tags);
            
            Assert.All(healthyRegistrations, reg => options.Registrations.Add(reg));
            options.Registrations.Add(registration);
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => new DefaultHealthCheckService(factory, Options.Create(options), NullLogger<DefaultHealthCheckService>.Instance));
        }

        [Fact]
        public void CreateService_WithoutServiceScopeFactory_Fails()
        {
            // Arrange
            var options = new HealthCheckServiceOptions();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => new DefaultHealthCheckService(scopeFactory: null, options: Options.Create(options), logger: NullLogger<DefaultHealthCheckService>.Instance));
        }

        [Fact]
        public void CreateService_WithoutHealthOptions_Fails()
        {
            // Arrange
            IServiceScopeFactory factory = CreateServiceScopeFactory();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => new DefaultHealthCheckService(factory, options: null, logger: NullLogger<DefaultHealthCheckService>.Instance));
        }

        [Fact]
        public void CreateServiceWithoutLogger_WithoutScopeFactory_Fails()
        {
            // Arrange
            var options = new HealthCheckServiceOptions();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() =>
                new DefaultHealthCheckService(scopeFactory: null, options: Options.Create(options)));
        }

        [Fact]
        public void CreateServiceWithoutLogger_WithoutHealthOptions_Fails()
        {
            // Arrange
            IServiceScopeFactory factory = CreateServiceScopeFactory();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => new DefaultHealthCheckService(factory, options: null));
        }

        [Fact]
        public void CreateServiceWithoutHealthOptionsAndLogger_WithoutScopeFactory_Fails()
        {
            Assert.ThrowsAny<ArgumentException>(() => new DefaultHealthCheckService(scopeFactory: null));
        }

        [Fact]
        public void CreateService_WithNullOptionsRegistration_Fails()
        {
            // Arrange
            IServiceScopeFactory factory = CreateServiceScopeFactory();
            var options = new HealthCheckServiceOptions();
            options.Registrations.Add(null);
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => new DefaultHealthCheckService(factory, Options.Create(options), NullLogger<DefaultHealthCheckService>.Instance));
        }
        
        [Fact]
        public void CreateServiceWithoutLogger_WithNullOptionsRegistration_Fails()
        {
            // Arrange
            IServiceScopeFactory factory = CreateServiceScopeFactory();
            var options = new HealthCheckServiceOptions();
            options.Registrations.Add(null);
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => new DefaultHealthCheckService(factory, Options.Create(options)));
        }

        [Fact]
        public void CreateService_WithNullOptionsValue_Fails()
        {
            // Arrange
            IServiceScopeFactory factory = CreateServiceScopeFactory();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => new DefaultHealthCheckService(
                factory,
                Options.Create<HealthCheckServiceOptions>(options: null),
                NullLogger<DefaultHealthCheckService>.Instance));
        }

        [Fact]
        public void CreateServiceWithoutLogger_WithNullOptionsValue_Fails()
        {
            // Arrange
            IServiceScopeFactory factory = CreateServiceScopeFactory();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() =>
                new DefaultHealthCheckService(factory, Options.Create<HealthCheckServiceOptions>(options: null)));
        }

        [Fact]
        public void CreateService_WithNullRegistrationFactory_Fails()
        {
            // Arrange
            IServiceScopeFactory factory = CreateServiceScopeFactory();
            var options = new HealthCheckServiceOptions();

            HealthCheckRegistration registration = GenerateHealthCheckRegistration();
            RemoveHealthCheckFactory(registration);
            options.Registrations.Add(registration);
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => new DefaultHealthCheckService(
                factory,
                Options.Create(options),
                NullLogger<DefaultHealthCheckService>.Instance));
        }

        [Fact]
        public void CreateServiceWithoutLogger_WithNullRegistrationFactory_Fails()
        {
            // Arrange
            IServiceScopeFactory factory = CreateServiceScopeFactory();
            var options = new HealthCheckServiceOptions();

            HealthCheckRegistration registration = GenerateHealthCheckRegistration();
            RemoveHealthCheckFactory(registration);
            options.Registrations.Add(registration);
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => new DefaultHealthCheckService(
                factory,
                Options.Create(options)));
        }

        private static void RemoveHealthCheckFactory(HealthCheckRegistration registration)
        {
            Type type = registration.GetType();
            FieldInfo factoryField = type.GetField("_factory", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(factoryField);
            
            factoryField.SetValue(registration, null);
        }

        private static IServiceScopeFactory CreateServiceScopeFactory()
        {
            var factory = new Mock<IServiceScopeFactory>();
            factory.Setup(fac => fac.CreateScope())
                   .Returns(new ServiceCollection().BuildServiceProvider().CreateScope());
            
            return factory.Object;
        }

        private static HealthCheckRegistration[] GenerateHealthCheckRegistrations(HealthStatus healthStatus)
        {
            IList<HealthCheckRegistration> registrations = 
                BogusGenerator.Make(10, () => GenerateHealthCheckRegistration(healthStatus));

            return registrations.ToArray();
        }

        private static IHealthCheck GenerateThrownHealthCheck()
        {
            Exception exception = BogusGenerator.System.Exception();
            IHealthCheck healthCheck = CreateThrownHealthCheck(exception);

            return healthCheck;
        }

        private static IHealthCheck CreateThrownHealthCheck(Exception exception)
        {
            var healthCheck = new Mock<IHealthCheck>();
            healthCheck.Setup(check => check.CheckHealthAsync(It.IsAny<HealthCheckContext>(), CancellationToken.None))
                       .ThrowsAsync(exception);

            return healthCheck.Object;
        }
    }
}
