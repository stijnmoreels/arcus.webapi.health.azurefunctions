using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Arcus.Testing.Logging;
using Arcus.WebApi.Health.AzureFunctions.Tests.Unit.Fixture;
using Bogus;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static Arcus.WebApi.Health.AzureFunctions.Tests.Unit.Fixture.HealthCheckRegistrationGenerator;

namespace Arcus.WebApi.Health.AzureFunctions.Tests.Unit.Extensions
{
    // ReSharper disable once InconsistentNaming
    public class ILoggerExtensionsTests
    {
        private static readonly Faker BogusGenerator = new Faker();

        [Fact]
        public void LogHealthCheckProcessingEnd_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new InMemoryLogger();
            var status = BogusGenerator.Random.Enum<HealthStatus>();
            TimeSpan duration = GeneratePositiveDuration();
            
            // Act
            logger.LogHealthCheckProcessingEnd(status, duration);
            
            // Assert
            string message = Assert.Single(logger.Messages);
            Assert.Contains(status.ToString(), message);
            Assert.Contains(duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture), message);
        }
        
        [Fact]
        public void LogHealthCheckProcessingEnd_WithUnknownHealthStatus_Fails()
        {
            // Arrange
            var status = (HealthStatus) BogusGenerator.Random.Int(min: 3);
            TimeSpan duration = GeneratePositiveDuration();
            var logger = new InMemoryLogger();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogHealthCheckProcessingEnd(status, duration));
        }
        
        [Fact]
        public void LogHealthCheckProcessingEnd_WithNegativeDuration_Fails()
        {
            // Arrange
            var status = BogusGenerator.Random.Enum<HealthStatus>();
            TimeSpan duration = GenerateNegativeDuration();
            var logger = new InMemoryLogger();

            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogHealthCheckProcessingEnd(status, duration));
        }

        [Fact]
        public void LogHealthCheckBegin_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new InMemoryLogger();
            var status = BogusGenerator.Random.Enum<HealthStatus>();
            HealthCheckRegistration registration = GenerateHealthCheckRegistration(status);

            // Act
            logger.LogHealthCheckBegin(registration);
            
            // Assert
            string message = Assert.Single(logger.Messages);
            Assert.Contains(registration.Name, message);
        }

        [Fact]
        public void LogHealthCheckBegin_WithoutHealthCheckRegistration_Fails()
        {
            // Arrange
            var logger = new InMemoryLogger();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogHealthCheckBegin(registration: null));
        }

        [Fact]
        public void LogHealthCheckEnd_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new InMemoryLogger();
            HealthReportEntry entry = GenerateHealthReportEntry();
            HealthCheckRegistration registration = GenerateHealthCheckRegistration(entry.Status);
            TimeSpan duration = GeneratePositiveDuration();
            
            // Act
            logger.LogHealthCheckEnd(registration, entry, duration);
            
            // Assert
            string message = Assert.Single(logger.Messages);
            Assert.Contains(registration.Name, message);
            Assert.Contains(duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture), message);
            Assert.Contains(entry.Status.ToString(), message);
            Assert.Contains(entry.Description, message);
        }

        [Fact]
        public void LogHealthCheckEnd_WithoutHealthCheckRegistration_Fails()
        {
            // Arrange
            var logger = new InMemoryLogger();
            HealthReportEntry entry = GenerateHealthReportEntry();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogHealthCheckEnd(registration: null, entry: entry, duration: entry.Duration));
        }

        [Fact]
        public void LogHealthCheckEnd_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new InMemoryLogger();
            HealthReportEntry entry = GenerateHealthReportEntry();
            HealthCheckRegistration registration = GenerateHealthCheckRegistration(entry.Status);
            TimeSpan duration = GenerateNegativeDuration();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogHealthCheckEnd(registration, entry, duration));
        }

        [Fact]
        public void LogHealthCheckEnd_WithUnknownHealthStatus_Fails()
        {
            // Arrange
            var logger = new InMemoryLogger();
            var status = (HealthStatus) BogusGenerator.Random.Int(min: 3);
            HealthReportEntry entry = GenerateHealthReportEntry(status);
            HealthCheckRegistration registration = GenerateHealthCheckRegistration(entry.Status);
            TimeSpan duration = GeneratePositiveDuration();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogHealthCheckEnd(registration, entry, duration));
        }

        [Fact]
        public void LogHealthCheckError_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new InMemoryLogger();
            var status = BogusGenerator.Random.Enum<HealthStatus>();
            HealthCheckRegistration registration = GenerateHealthCheckRegistration(status);
            Exception exception = BogusGenerator.System.Exception();
            TimeSpan duration = GeneratePositiveDuration();
            
            // Act
            logger.LogHealthCheckError(registration, exception, duration);
            
            // Assert
            string message = Assert.Single(logger.Messages);
            Assert.Contains(registration.Name, message);
            Assert.Contains(duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture), message);
            LogEntry logEntry = Assert.Single(logger.Entries);
            Assert.Equal(exception, logEntry.Exception);
        }

        [Fact]
        public void LogHealthCheckError_WithWithoutHealthCheckRegistration_Fails()
        {
            // Arrange
            var logger = new InMemoryLogger();
            Exception exception = BogusGenerator.System.Exception();
            TimeSpan duration = GeneratePositiveDuration();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(() =>
                logger.LogHealthCheckError(registration: null, exception: exception, duration: duration));
        }

        [Fact]
        public void LogHealthCheckError_WithoutException_Fails()
        {
            // Arrange
            var logger = new InMemoryLogger();
            var status = BogusGenerator.Random.Enum<HealthStatus>();
            HealthCheckRegistration registration = GenerateHealthCheckRegistration(status);
            TimeSpan duration = GeneratePositiveDuration();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogHealthCheckError(registration, exception: null, duration: duration));
        }

        [Fact]
        public void LogHealthCheckError_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new InMemoryLogger();
            var status = BogusGenerator.Random.Enum<HealthStatus>();
            HealthCheckRegistration registration = GenerateHealthCheckRegistration(status);
            Exception exception = BogusGenerator.System.Exception();
            TimeSpan duration = GenerateNegativeDuration();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogHealthCheckError(registration, exception, duration));
        }

        [Fact]
        public void LogHealthCheckEndFailed_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new InMemoryLogger();
            HealthReportEntry entry = GenerateHealthReportEntry();
            HealthCheckRegistration registration = GenerateHealthCheckRegistration(entry.Status);
            TimeSpan duration = GeneratePositiveDuration();
            
            // Act
            logger.LogHealthCheckEndFailed(registration, entry, duration);
            
            // Assert
            string message = Assert.Single(logger.Messages);
            Assert.Contains(registration.Name, message);
            Assert.Contains(duration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture), message);
            Assert.Contains(entry.Status.ToString(), message);
            Assert.Contains(entry.Description, message);
        }

        [Fact]
        public void LogHealthCheckEndFailed_WithoutRegistration_Fails()
        {
            // Arrange
            var logger = new InMemoryLogger();
            HealthReportEntry entry = GenerateHealthReportEntry();
            TimeSpan duration = GeneratePositiveDuration();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogHealthCheckEndFailed(registration: null, entry: entry, duration: duration));
        }

        [Fact]
        public void LogHealthCheckEndFailed_WithDefaultEntry_Fails()
        {
            // Arrange
            var logger = new InMemoryLogger();
            var status = BogusGenerator.Random.Enum<HealthStatus>();
            HealthCheckRegistration registration = GenerateHealthCheckRegistration(status);
            TimeSpan duration = GeneratePositiveDuration();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogHealthCheckEndFailed(registration, entry: default(HealthReportEntry), duration: duration));
        }

        [Fact]
        public void LogHealthCheckEndFailed_WithNegativeDuration_Fails()
        {
            // Arrange
            var logger = new InMemoryLogger();
            var status = BogusGenerator.Random.Enum<HealthStatus>();
            HealthCheckRegistration registration = GenerateHealthCheckRegistration(status);
            HealthReportEntry entry = GenerateHealthReportEntry();
            TimeSpan duration = GenerateNegativeDuration();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogHealthCheckEndFailed(registration, entry, duration));
        }

        [Fact]
        public void LogHealthCheckEndFailed_WithUnknownHealthStatus_Fails()
        {
            // Arrange
            var logger = new InMemoryLogger();
            var status = (HealthStatus) BogusGenerator.Random.Int(min: 3);
            HealthReportEntry entry = GenerateHealthReportEntry(status);
            HealthCheckRegistration registration = GenerateHealthCheckRegistration(status);
            TimeSpan duration = GeneratePositiveDuration();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogHealthCheckEndFailed(registration, entry, duration));
        }

        [Fact]
        public void LogHealthCheckData_WithValidArguments_Succeeds()
        {
            // Arrange
            var logger = new InMemoryLogger();
            HealthReportEntry entry = GenerateHealthReportEntry();
            HealthCheckRegistration registration = GenerateHealthCheckRegistration(entry.Status);

            // Act
            logger.LogHealthCheckData(registration, entry);
            
            // Assert
            string message = Assert.Single(logger.Messages);
            Assert.Contains(registration.Name, message);
            Assert.All(entry.Data.Keys, key => Assert.Contains(key, message));
            Assert.All(entry.Data.Values, value => Assert.Contains(value.ToString(), message));
        }

        [Fact]
        public void LogHealthCheckData_WithValidArgumentsWithoutDebugging_Succeeds()
        {
            // Arrange
            var level = BogusGenerator.Random.Enum<LogLevel>();
            var spyLogger = new SpyLogLevelLogger(level);

            HealthReportEntry entry = GenerateHealthReportEntry();
            HealthCheckRegistration registration = GenerateHealthCheckRegistration(entry.Status);
            
            // Act
            spyLogger.LogHealthCheckData(registration, entry);
            
            // Assert
            Assert.Equal(level == LogLevel.Debug, spyLogger.IsLogMessageWritten);
        }

        [Fact]
        public void LogHealthCheckData_WithoutHealthCheckRegistration_Fails()
        {
            // Arrange
            var logger = new InMemoryLogger();
            HealthReportEntry entry = GenerateHealthReportEntry();
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogHealthCheckData(registration: null, entry: entry));
        }

        [Fact]
        public void LogHealthCheckData_WithDefaultHealthCheckEntry_Fails()
        {
            // Arrange
            var logger = new InMemoryLogger();
            var status = BogusGenerator.Random.Enum<HealthStatus>();
            HealthCheckRegistration registration = GenerateHealthCheckRegistration(status);
            
            // Act / Assert
            Assert.ThrowsAny<ArgumentException>(
                () => logger.LogHealthCheckData(registration, default(HealthReportEntry)));
        }

        private static HealthReportEntry GenerateHealthReportEntry()
        {
            var status = BogusGenerator.Random.Enum<HealthStatus>();
            HealthReportEntry entry = GenerateHealthReportEntry(status);

            return entry;
        }

        private static HealthReportEntry GenerateHealthReportEntry(HealthStatus status)
        {
            string description = BogusGenerator.Lorem.Sentence();
            TimeSpan duration = GeneratePositiveDuration();

            Exception exception = null;
            if (status is HealthStatus.Unhealthy)
            {
                exception = BogusGenerator.System.Exception();
            }
            
            IDictionary<string, object> data = 
                BogusGenerator.Lorem.Words()
                              .Distinct(StringComparer.OrdinalIgnoreCase)
                              .ToDictionary(word => word, word => (object) BogusGenerator.Random.Int());
            var dataReadOnly = new ReadOnlyDictionary<string, object>(data);

            var entry = new HealthReportEntry(status, description, duration, exception, dataReadOnly);
            return entry;
        }

        private static TimeSpan GeneratePositiveDuration()
        {
            TimeSpan duration = BogusGenerator.Date.Timespan();
            if (duration > TimeSpan.Zero)
            {
                return duration;
            }

            TimeSpan positiveDuration = duration.Negate();
            return positiveDuration;
        }

        private static TimeSpan GenerateNegativeDuration()
        {
            TimeSpan duration = BogusGenerator.Date.Timespan();
            if (duration < TimeSpan.Zero)
            {
                return duration;
            }

            TimeSpan negativeDuration = duration.Negate();
            return negativeDuration;
        }
    }
}
