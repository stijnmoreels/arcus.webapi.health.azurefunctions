using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Arcus.WebApi.Health.AzureFunctions.Tests.Unit.Fixture
{
    public class TestHealthCheckService : DefaultHealthCheckService
    {
        public TestHealthCheckService(
            IServiceScopeFactory scopeFactory, 
            IOptions<HealthCheckServiceOptions> options, 
            ILogger<DefaultHealthCheckService> logger) 
            : base(scopeFactory, options, logger)
        {
        }

        public TestHealthCheckService(
            IServiceScopeFactory scopeFactory, 
            IOptions<HealthCheckServiceOptions> options) 
            : base(scopeFactory, options)
        {
        }

        public TestHealthCheckService(
            IServiceScopeFactory scopeFactory, 
            ILogger<DefaultHealthCheckService> logger) 
            : base(scopeFactory, logger)
        {
        }

        public TestHealthCheckService(IServiceScopeFactory scopeFactory) : base(scopeFactory)
        {
        }
    }
}
