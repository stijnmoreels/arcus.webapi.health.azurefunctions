using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Xunit;

namespace Arcus.WebApi.Health.AzureFunctions.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class HealthCheckAzureFunctionsTests
    {
        private readonly IConfiguration _config;

        private static readonly HttpClient HttpClient = new HttpClient();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="HealthCheckAzureFunctionsTests" /> class.
        /// </summary>
        public HealthCheckAzureFunctionsTests()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
        }

        [Fact]
        public async Task RuntimeWithHealthChecks_GetHealth_ReturnsHealthyReport()
        {
            // Arrange
            var httpPort = _config.GetValue<int>("Arcus:Infra:HttpPort");
            var route = $"http://localhost:{httpPort}/api/health";

            // Act
            using (HttpResponseMessage response = await HttpClient.GetAsync(route))
            {
                // Assert
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                string json = await response.Content.ReadAsStringAsync();
                var report = JsonConvert.DeserializeObject<HealthReport>(json);
                Assert.Equal(HealthStatus.Healthy, report.Status);
            }
        }
    }
}
