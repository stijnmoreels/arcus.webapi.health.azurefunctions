using System;
using Arcus.WebApi.Health.AzureFunctions;
using GuardNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

// ReSharper disable once CheckNamespace
namespace Microsoft.Azure.Functions.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides extension methods for registering <see cref="HealthCheckService"/> in an <see cref="IServiceCollection"/>.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public static class IFunctionsHostBuilderExtensions
    {
        /// <summary>
        /// Adds the health checks services to the application.
        /// </summary>
        /// <remarks>
        ///     This operation is idempotent - multiple invocations will still only result in a single
        ///     <see cref="HealthCheckService"/> instance in the <see cref="IServiceCollection"/>. It can be invoked
        ///     multiple times in order to get access to the <see cref="IHealthChecksBuilder"/> in multiple places.
        /// </remarks>
        /// <param name="builder">The <see cref="IFunctionsHostBuilder"/> to add the <see cref="HealthCheckService"/> to the application.</param>
        /// <returns>
        ///     An instance of <see cref="IHealthChecksBuilder"/> from which health checks can be registered.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="builder"/> is <c>null</c>.</exception>
        public static IHealthChecksBuilder AddHealthChecks(this IFunctionsHostBuilder builder)
        {
            Guard.NotNull(builder, nameof(builder), "Requires a functions host builder to add the health checks to");

            IHealthChecksBuilder healthChecksBuilder = AddHealthChecks<DefaultHealthCheckService>(builder);
            return healthChecksBuilder;
        }

        /// <summary>
        /// Adds the health checks services to the application, using an custom <typeparamref name="THealthCheckService"/> implementation.
        /// </summary>
        /// <typeparam name="THealthCheckService">The custom implementation to handle the health checks process.</typeparam>
        /// <param name="builder">The <see cref="IFunctionsHostBuilder"/> to add the <typeparamref name="THealthCheckService"/> to the application.</param>
        /// <returns>
        ///     An instance of <see cref="IHealthChecksBuilder"/> from which health checks can be registered.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="builder"/> is <c>null</c>.</exception>
        public static IHealthChecksBuilder AddHealthChecks<THealthCheckService>(this IFunctionsHostBuilder builder)
            where THealthCheckService : DefaultHealthCheckService
        {
            Guard.NotNull(builder, nameof(builder), "Requires a functions host builder to add the health checks to");

            builder.Services.TryAddSingleton<HealthCheckService, THealthCheckService>();
            return new HealthChecksBuilder(builder.Services);
        }

        /// <summary>
        /// Adds the health checks services to the application, using an custom <typeparamref name="THealthCheckService"/> implementation.
        /// </summary>
        /// <typeparam name="THealthCheckService">The custom implementation to handle the health checks process.</typeparam>
        /// <param name="builder">The <see cref="IFunctionsHostBuilder"/> to add the <typeparamref name="THealthCheckService"/> to the application.</param>
        /// <param name="implementationFactory">The factory function to create an custom <typeparamref name="THealthCheckService"/> implementation.</param>
        /// <returns>
        ///     An instance of <see cref="IHealthChecksBuilder"/> from which health checks can be registered.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="builder"/> or the <paramref name="implementationFactory"/> is <c>null</c>.</exception>
        public static IHealthChecksBuilder AddHealthChecks<THealthCheckService>(
            this IFunctionsHostBuilder builder,
            Func<IServiceProvider, THealthCheckService> implementationFactory)
            where THealthCheckService : DefaultHealthCheckService
        {
            Guard.NotNull(builder, nameof(builder), "Requires a functions host builder to add the health checks to");
            Guard.NotNull(implementationFactory, nameof(implementationFactory), "Requires a factory function to create an custom health check service implementation");
            
            builder.Services.TryAddSingleton<HealthCheckService>(implementationFactory);
            return new HealthChecksBuilder(builder.Services);
        }
    }
}

