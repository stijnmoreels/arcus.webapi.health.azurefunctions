//Copyright(c) .NET Foundation and Contributors

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GuardNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Arcus.WebApi.Health.AzureFunctions
{
    /// <summary>
    /// Represents an default <see cref="HealthCheckService"/> implementation that only checks the registered <see cref="IHealthCheck"/>'s for their statuses.
    /// </summary>
    public class DefaultHealthCheckService : HealthCheckService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHealthCheckService"/> class.
        /// </summary>
        /// <param name="scopeFactory">The factory to resolve -and or create- <see cref="IHealthCheck"/> instances for each <see cref="HealthCheckRegistration"/>.</param>
        /// <param name="options">The internally-added options to specify the <see cref="HealthCheckRegistration"/>s.</param>
        /// <param name="logger">The logger instance to write diagnostic trace messages during the health checks process.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="scopeFactory"/>, the <paramref name="options"/> is <c>null</c>
        ///     or the <paramref name="options"/> doesn't contain a value for <see cref="HealthCheckServiceOptions.Registrations"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="options"/> contains <c>null</c> elements for the <see cref="HealthCheckServiceOptions.Registrations"/>
        ///     or <c>null</c> values for the <see cref="HealthCheckRegistration.Factory"/> property
        ///     or duplicate <see cref="HealthCheckRegistration.Name"/>s for the <see cref="HealthCheckServiceOptions.Registrations"/>
        /// </exception>
        public DefaultHealthCheckService(
            IServiceScopeFactory scopeFactory,
            IOptions<HealthCheckServiceOptions> options,
            ILogger<DefaultHealthCheckService> logger)
        {
            Guard.NotNull(scopeFactory, nameof(scopeFactory), "Requires an scope factory instance to resolve -and or create- health check instances for each health check registration");
            Guard.NotNull(options, nameof(options), "Requires a set of options to define all the health check registrations to be checked");
            Guard.NotNull(options.Value, nameof(options), "Requires a value for the set of options to define all the health check registrations to be checked");
            Guard.NotNull(options.Value.Registrations, nameof(options), "Requires a set of health check registrations in the set of options");
            Guard.For(() => options.Value.Registrations.Any(reg => reg is null), 
                new ArgumentException("Requires a health check registration instance for each element in the health check options", nameof(options)));
            Guard.For(() => options.Value.Registrations.Any(reg => reg.Factory is null),
                new ArgumentException("Requires a factory function to create health check instances for each health check registration in the health check options", nameof(options)));
            
            string[] duplicateNames = 
                options.Value.Registrations
                        .GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key)
                        .ToArray();

            Guard.For(() => duplicateNames.Length > 0, new ArgumentException(
                $"Requires unique names for the health check registrations in the options, but got duplicate name(s): {string.Join(", ", duplicateNames)}", nameof(options)));
            
            ScopeFactory = scopeFactory;
            Options = options;
            Logger = (ILogger) logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHealthCheckService" /> class.
        /// </summary>
        /// <param name="scopeFactory">The factory to resolve -and or create- <see cref="IHealthCheck"/> instances for each <see cref="HealthCheckRegistration"/>.</param>
        /// <param name="options">The internally-added options to specify the <see cref="HealthCheckRegistration"/>s.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="scopeFactory"/>, the <paramref name="options"/> is <c>null</c>
        ///     or the <paramref name="options"/> doesn't contain a value for <see cref="HealthCheckServiceOptions.Registrations"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the <paramref name="options"/> contains <c>null</c> elements for the <see cref="HealthCheckServiceOptions.Registrations"/>
        ///     or <c>null</c> values for the <see cref="HealthCheckRegistration.Factory"/> property
        ///     or duplicate <see cref="HealthCheckRegistration.Name"/>s for the <see cref="HealthCheckServiceOptions.Registrations"/>
        /// </exception>
        public DefaultHealthCheckService(
            IServiceScopeFactory scopeFactory,
            IOptions<HealthCheckServiceOptions> options)
            : this(scopeFactory, options, NullLogger<DefaultHealthCheckService>.Instance)
        {
            Guard.NotNull(scopeFactory, nameof(scopeFactory), "Requires an scope factory instance to resolve -and or create- health check instances for each health check registration");
            Guard.NotNull(options, nameof(options), "Requires a set of options to define all the health check registrations to be checked");
            Guard.NotNull(options.Value, nameof(options), "Requires a value for the set of options to define all the health check registrations to be checked");
            Guard.NotNull(options.Value.Registrations, nameof(options), "Requires a set of health check registrations in the set of options");
            Guard.For(() => options.Value.Registrations.Any(reg => reg is null), 
                new ArgumentException("Requires a health check registration instance for each element in the health check options", nameof(options)));
            Guard.For(() => options.Value.Registrations.Any(reg => reg.Factory is null),
                new ArgumentException("Requires a factory function to create health check instances for each health check registration in the health check options", nameof(options)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHealthCheckService" /> class.
        /// </summary>
        /// <param name="scopeFactory">The factory to resolve -and or create- <see cref="IHealthCheck"/> instances for each <see cref="HealthCheckRegistration"/>.</param>
        /// <param name="logger">The logger instance to write diagnostic trace messages during the health checks process.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="scopeFactory"/> is <c>null</c>.</exception>
        public DefaultHealthCheckService(
            IServiceScopeFactory scopeFactory,
            ILogger<DefaultHealthCheckService> logger)
            : this(scopeFactory, Microsoft.Extensions.Options.Options.Create(new HealthCheckServiceOptions()), logger)
        {
            Guard.NotNull(scopeFactory, nameof(scopeFactory), "Requires an scope factory instance to resolve -and or create- health check instances for each health check registration");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultHealthCheckService" /> class.
        /// </summary>
        /// <param name="scopeFactory">The factory to resolve -and or create- <see cref="IHealthCheck"/> instances for each <see cref="HealthCheckRegistration"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="scopeFactory"/> is <c>null</c>.</exception>
        public DefaultHealthCheckService(IServiceScopeFactory scopeFactory)
            : this(scopeFactory, Microsoft.Extensions.Options.Options.Create(new HealthCheckServiceOptions()), NullLogger<DefaultHealthCheckService>.Instance)
        {
            Guard.NotNull(scopeFactory, nameof(scopeFactory), "Requires an scope factory instance to resolve -and or create- health check instances for each health check registration");
        }

        /// <summary>
        /// Gets the factory to resolve -and or create- <see cref="IHealthCheck"/> instances for each <see cref="HealthCheckRegistration"/>.
        /// </summary>
        protected IServiceScopeFactory ScopeFactory { get; }
        
        /// <summary>
        /// Gets the internally-added options to specify the <see cref="HealthCheckRegistration"/>s.
        /// </summary>
        protected IOptions<HealthCheckServiceOptions> Options { get; }
        
        /// <summary>
        /// Gets the logger instance to write diagnostic trace messages during the health checks process.
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Runs the provided health checks and returns the aggregated status
        /// </summary>
        /// <param name="predicate">
        ///     A predicate that can be used to include health checks based on user-defined criteria.
        /// </param>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> which can be used to cancel the health checks.</param>
        /// <returns>
        ///     A <see cref="T:System.Threading.Tasks.Task`1" /> which will complete when all the health checks have been run,
        ///     yielding a <see cref="T:Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport" /> containing the results.
        /// </returns>
        public override async Task<HealthReport> CheckHealthAsync(
            Func<HealthCheckRegistration, bool> predicate,
            CancellationToken cancellationToken = default)
        {
            ICollection<HealthCheckRegistration> registrations = Options.Value.Registrations;
            using (IServiceScope scope = ScopeFactory.CreateScope())
            {
                var context = new HealthCheckContext();
                var entries = new Dictionary<string, HealthReportEntry>(StringComparer.OrdinalIgnoreCase);

                var totalTime = ValueStopwatch.StartNew();
                Logger.LogHealthCheckProcessingBegin();

                foreach (HealthCheckRegistration registration in registrations)
                {
                    if (predicate != null && !predicate(registration))
                    {
                        continue;
                    }

                    cancellationToken.ThrowIfCancellationRequested();
                    context.Registration = registration;
                    
                    HealthReportEntry entry = await CheckHealthReportEntryAsync(registration, scope, context, cancellationToken);
                    entries[registration.Name] = entry;
                }

                TimeSpan totalElapsedTime = totalTime.Elapsed;
                var report = new HealthReport(entries, totalElapsedTime);
                Logger.LogHealthCheckProcessingEnd(report.Status, totalElapsedTime);
                
                return report;
            }
        }

        /// <summary>
        /// Runs the provided health check <paramref name="healthRegistration"/>.
        /// </summary>
        /// <param name="healthRegistration">The health check registration that holds the <see cref="IHealthCheck"/> that needs to be run.</param>
        /// <param name="serviceScope">
        ///     The service scope to create -and or resolve- the next <see cref="IHealthCheck"/> from the <see cref="HealthCheckRegistration.Factory"/>.
        /// </param>
        /// <param name="healthContext">The context that gets accumulated during each <see cref="IHealthCheck"/> run.</param>
        /// <param name="cancellationToken">The cancellation token that can be used to cancel the health check.</param>
        /// <returns>
        ///     The <see cref="HealthReportEntry"/> that describes the end-result status
        ///     of the <see cref="IHealthCheck"/> that was found in the given <paramref name="healthRegistration"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when the <paramref name="healthRegistration"/>, the <paramref name="serviceScope"/>, or the <paramref name="healthContext"/> is <c>null</c>.
        /// </exception>
        protected virtual async Task<HealthReportEntry> CheckHealthReportEntryAsync(
            HealthCheckRegistration healthRegistration,
            IServiceScope serviceScope,
            HealthCheckContext healthContext,
            CancellationToken cancellationToken)
        {
            Guard.NotNull(healthRegistration, nameof(healthRegistration), "Requires a health check registration to run the health check");
            Guard.NotNull(serviceScope, nameof(serviceScope), "Requires a service scope to resolve -and or create- an health check instance");
            Guard.NotNull(healthContext, nameof(healthContext), "Requires a health check context to accumulate during each health check run");
            
            IHealthCheck healthCheck = healthRegistration.Factory(serviceScope.ServiceProvider);
            var stopwatch = ValueStopwatch.StartNew();
            
            Logger.LogHealthCheckBegin(healthRegistration);
            
            if (healthCheck is null)
            {
                TimeSpan duration = stopwatch.Elapsed;
                var entry = new HealthReportEntry(
                    HealthStatus.Unhealthy, "No health check instance was returned by the health registration factory, got 'null'", duration, exception: null, data: null);
                
                Logger.LogHealthCheckEndFailed(healthRegistration, entry, duration);
                return entry;
            }

            try
            {
                HealthCheckResult result = await healthCheck.CheckHealthAsync(healthContext, cancellationToken);
                TimeSpan duration = stopwatch.Elapsed;

                var entry = new HealthReportEntry(result.Status, result.Description, duration, result.Exception, result.Data);
                Logger.LogHealthCheckEnd(healthRegistration, entry, duration);
                Logger.LogHealthCheckData(healthRegistration, entry);

                return entry;
            }
            catch (Exception exception) when (!(exception is OperationCanceledException))
            {
                TimeSpan duration = stopwatch.Elapsed;
                var entry = new HealthReportEntry(HealthStatus.Unhealthy, exception.Message, duration, exception, data: null);

                Logger.LogHealthCheckError(healthRegistration, exception, duration);
                return entry;
            }
        }
    }
}
