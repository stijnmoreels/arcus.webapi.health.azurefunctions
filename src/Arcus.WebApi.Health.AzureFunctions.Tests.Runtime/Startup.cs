﻿using Arcus.WebApi.Health.AzureFunctions.Tests.Runtime;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Arcus.WebApi.Health.AzureFunctions.Tests.Runtime
{
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="builder">The instance to build the registered services inside the functions app.</param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.AddHealthChecks();
        }
    }
}
