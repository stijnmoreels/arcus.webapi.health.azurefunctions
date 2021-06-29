using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Arcus.WebApi.Health.AzureFunctions.Tests.Runtime
{
    public class HealthFunction
    {
        private readonly HealthCheckService _healthService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthFunction" /> class.
        /// </summary>
        public HealthFunction(HealthCheckService healthService)
        {
            _healthService = healthService;
        }
        
        [FunctionName("health")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP health function processed a request");

            HealthReport report = await _healthService.CheckHealthAsync();
            if (report?.Status == HealthStatus.Healthy)
            {
                return new OkObjectResult(report);
            }

            return new ObjectResult(report) { StatusCode = StatusCodes.Status503ServiceUnavailable };
        }
    }
}
