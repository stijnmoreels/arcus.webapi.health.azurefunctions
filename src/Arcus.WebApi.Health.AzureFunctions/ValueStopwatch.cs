//Copyright(c) .NET Foundation and Contributors

using System;
using System.Diagnostics;

namespace Arcus.WebApi.Health.AzureFunctions
{
    /// <summary>
    /// Represents a stopwatch model using timestamp values.
    /// </summary>
    internal readonly struct ValueStopwatch
    {
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double) Stopwatch.Frequency;
        
        private readonly long _value;

        private ValueStopwatch(long timestamp)
        {
            _value = timestamp;
        }

        /// <summary>
        /// Returns true if this instance is running or false otherwise.
        /// </summary>
        public bool IsRunning => _value > 0;

        /// <summary>
        /// Returns the elapsed time.
        /// </summary>
        public TimeSpan Elapsed => TimeSpan.FromTicks(ElapsedTicks);

        /// <summary>
        /// Returns the elapsed ticks.
        /// </summary>
        public long ElapsedTicks
        {
            get
            {
                // A positive timestamp value indicates the start time of a running stopwatch,
                // a negative value indicates the negative total duration of a stopped stopwatch.
                long timestamp = _value;

                long delta;
                if (IsRunning)
                {
                    // The stopwatch is still running.
                    long start = timestamp;
                    long end = Stopwatch.GetTimestamp();
                    delta = end - start;
                }
                else
                {
                    // The stopwatch has been stopped.
                    delta = -timestamp;
                }

                return (long)(delta * TimestampToTicks);
            }
        }

        /// <summary>
        /// Starts a new instance.
        /// </summary>
        /// <returns>A new, running stopwatch.</returns>
        public static ValueStopwatch StartNew()
        {
            long timestamp = Stopwatch.GetTimestamp();
            return new ValueStopwatch(timestamp);
        }
    }
}