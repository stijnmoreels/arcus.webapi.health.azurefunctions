using System;
using Arcus.WebApi.Health.AzureFunctions.Tests.Unit.Fixture;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using Xunit;

namespace Arcus.WebApi.Health.AzureFunctions.Tests.Unit.Extensions
{
    // ReSharper disable once InconsistentNaming
    public class IFunctionsHostBuilderExtensionsTests
    {
        [Fact]
        public void AddHealthChecks_WithDefaultService_Succeeds()
        {
            // Arrange
            var services = new ServiceCollection();
            var builderStub = new Mock<IFunctionsHostBuilder>();
            builderStub.Setup(build => build.Services).Returns(services);
            IFunctionsHostBuilder builder = builderStub.Object;
            
            // Act
            builder.AddHealthChecks();
            
            // Assert
            IServiceProvider provider = builder.Services.BuildServiceProvider();
            var service = provider.GetRequiredService<HealthCheckService>();
            Assert.IsType<DefaultHealthCheckService>(service);
        }

        [Fact]
        public void AddHealthChecks_WithCustomService_Succeeds()
        {
            // Arrange
            var services = new ServiceCollection();
            var builderStub = new Mock<IFunctionsHostBuilder>();
            builderStub.Setup(build => build.Services).Returns(services);
            IFunctionsHostBuilder builder = builderStub.Object;
            
            // Act
            builder.AddHealthChecks<TestHealthCheckService>();
            
            // Assert
            IServiceProvider provider = builder.Services.BuildServiceProvider();
            var service = provider.GetRequiredService<HealthCheckService>();
            Assert.IsType<TestHealthCheckService>(service);
        }

        [Fact]
        public void AddHealthChecks_WithCustomServiceViaImplementationFactory_Succeeds()
        {
            // Arrange
            var services = new ServiceCollection();
            var builderStub = new Mock<IFunctionsHostBuilder>();
            builderStub.Setup(build => build.Services).Returns(services);
            IFunctionsHostBuilder builder = builderStub.Object;
            
            // Act
            builder.AddHealthChecks(serviceProvider =>
            {
                var factory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
                return new TestHealthCheckService(factory);
            });
            
            // Assert
            IServiceProvider provider = builder.Services.BuildServiceProvider();
            var service = provider.GetRequiredService<HealthCheckService>();
            Assert.IsType<TestHealthCheckService>(service);
        }
        
        [Fact]
        public void AddHealthChecks_WithoutImplementationFactory_Fails()
        {
            // Arrange
            var services = new ServiceCollection();
            var builderStub = new Mock<IFunctionsHostBuilder>();
            builderStub.Setup(build => build.Services).Returns(services);
            IFunctionsHostBuilder builder = builderStub.Object;

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => builder.AddHealthChecks<DefaultHealthCheckService>(implementationFactory: null));
        }
    }
}
