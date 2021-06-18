//Copyright(c) .NET Foundation and Contributors

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GuardNet;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Arcus.WebApi.Health.AzureFunctions.Logging
{
    /// <summary>
    /// Represents a data model to collect all the information on a ran health check when logging.
    /// </summary>
    public class HealthCheckDataLogValue : IReadOnlyList<KeyValuePair<string, object>>
    {
        private readonly string _healthCheckRegistrationName;
        private readonly IList<KeyValuePair<string, object>> _items;
        private readonly Lazy<string> _formattedLogValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthCheckDataLogValue"/> class.
        /// </summary>
        /// <param name="healthCheckRegistrationName">The name of the <see cref="HealthCheckRegistration"/> that defines the ran health check.</param>
        /// <param name="healthCheckData">The data collected during the ran health check.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="healthCheckRegistrationName"/> is blank.</exception>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="healthCheckData"/> is <c>null</c>.</exception>
        public HealthCheckDataLogValue(string healthCheckRegistrationName, IReadOnlyDictionary<string, object> healthCheckData)
        {
            Guard.NotNullOrWhitespace(healthCheckRegistrationName, nameof(healthCheckRegistrationName), "Requires a non-blank name for the health check registration name");
            Guard.NotNull(healthCheckData, nameof(healthCheckData), "Requires a set of health check data to format into a logging format");
            
            _healthCheckRegistrationName = healthCheckRegistrationName;
            _items = healthCheckData.ToList();

            // We add the name as a kvp so that you can filter by health check name in the logs.
            // This is the same parameter name used in the other logs.
            _items.Add(new KeyValuePair<string, object>("HealthCheckName", healthCheckRegistrationName));

            _formattedLogValue = new Lazy<string>(FormatDataLogValue);
        }

        private string FormatDataLogValue()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Health check data for {_healthCheckRegistrationName}:");

            foreach (KeyValuePair<string, object> item in _items)
            {
                builder.Append("    ");
                builder.Append(item.Key);
                builder.Append(": ");

                builder.AppendLine(item.Value?.ToString());
            }

            return builder.ToString();
        }

        /// <summary>
        /// Gets the element at the specified index in the read-only list.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index in the read-only list.</returns>
        public KeyValuePair<string, object> this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new IndexOutOfRangeException(nameof(index));
                }

                return _items[index];
            }
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <returns>The number of elements in the collection.</returns>
        public int Count => _items.Count;

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return _formattedLogValue.Value;
        }
    }
}