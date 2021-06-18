# Health checks in Azure Functions

The Arcus Web API health checks is a small library based on the [ASP.NET Core HealthChecks feature](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks). The traditional health checks registered in an ASP.NET Core API included the `HealthCheckPublisherHostedService` as a `IHostedService` which is not possible or desired to run in an Azure Function. 

However there are benefits to included a health check in an Azure Function to test the depencies of your service. This library will allow you to register health checks for your dependencies and create an HTTP endpoint that can be used to monitor the health of your application.

## How does it work?

1. Add the `Arcus.WebApi.Health.AzureFunctions` package to your project
2. Describe the health for your application with health checks (see [official ASP.NET Core documentation](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks) for more information)
    Example: `AspNetCore.HealthChecks.SqlServer`
3. Register the health checks in your `Startup.cs`:

```csharp
using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MyProject;

[assembly: FunctionsStartup(typeof(Startup))]
namespace MyProject
{
    public class Startup : FunctionsStartup
    {
        public override void ConfigureService(IFunctionsHostBuilder builder)
        {
            builder.AddHealthChecks()
                    .AddSqlServer("Server=localhost;Database=<your-db>;User Id=app;Password=<your-password");
        }
    }
}
```

4. Add an HTTP health check endpoint to your Azure Functions to expose the health of the applicatoin:

```csharp
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MyProject
{
    public class HttpFunc
    {
        private readonly HealthCheckService _healthCheckService;

        public HttpFunc(HealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }

        [FunctionName("health")]
        public async Task<IActionResult> Heartbeat(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/health")] HttpRequest req,
           ILogger log)
        {
            log.Log(LogLevel.Information, "Received heartbeat request");

            HealthReport report = await _healthCheckService.CheckHealthAsync();

            return new OkObjectResult(report);
        }
    }
}
```

5. Setup an external monitoring tool like Azure Monitor to create a ping test against this endpoint (See: https://docs.microsoft.com/en-us/azure/azure-monitor/app/monitor-web-app-availability for more information)
