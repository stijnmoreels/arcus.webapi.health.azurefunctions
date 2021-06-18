using System;
using System.Collections.Generic;
using Arcus.WebApi.Health.AzureFunctions.Logging;
using Xunit;

namespace Arcus.WebApi.Health.AzureFunctions.Tests.Unit.Logging
{
    public class HealthCheckLogValueTests
    {
        [Fact]
        public void Create_WithValidArguments_FormatsData()
        {
            // Arrange
            string name = $"name-{Guid.NewGuid()}",
                   key = $"key-{Guid.NewGuid()}",
                   value = $"value-{Guid.NewGuid()}";
            var data = new Dictionary<string, object>()
            {
                [key] = value
            };

            var logValue = new HealthCheckDataLogValue(name, data);

            // Act
            string formatted = logValue.ToString();
            
            // Assert
            Assert.Contains(name, formatted);
            Assert.Contains(key, formatted);
            Assert.Contains(value, formatted);
        }
        
        [Theory]
        [ClassData(typeof(Blanks))]
        public void Create_WithoutHealthCheckRegistrationName_Fails(string name)
        {
            // Arrange
            var data = new Dictionary<string, object>();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => new HealthCheckDataLogValue(name, data));
        }

        [Fact]
        public void Create_WithoutData_Fails()
        {
            // Arrange
            var name = $"name-{Guid.NewGuid()}";
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() => new HealthCheckDataLogValue(name, healthCheckData: null));
        }
    }
}
