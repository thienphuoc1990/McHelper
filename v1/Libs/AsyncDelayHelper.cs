using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Libs
{
    /// <summary>
    /// Static helper for cancellable delays.
    /// Use this to replace Thread.Sleep() in legacy code for responsive "Stop All" handling.
    /// 
    /// Migration guide:
    /// - Old: Thread.Sleep(1000);
    /// - New: AsyncDelayHelper.Delay(1000, characterId);
    /// 
    /// The delay will return early if:
    /// 1. Helper.IsStoppingAll() returns true
    /// 2. The character's cancellation token is triggered
    /// </summary>
    public static class AsyncDelayHelper
    {
        /// <summary>
        /// Delay with cancellation support. Returns early if stop is requested.
        /// </summary>
        /// <param name="milliseconds">Delay duration</param>
        /// <param name="characterId">Character ID for cancellation token lookup</param>
        /// <returns>True if delay completed normally, false if cancelled</returns>
        public static bool Delay(int milliseconds, string characterId)
        {
            // Check stop flag first
            if (Helper.IsStoppingAll())
                return false;

            // Get cancellation token for this character
            var token = Helper.GetCancellationToken(characterId);

            try
            {
                // Use Task.Delay for cancellable waiting
                Task.Delay(milliseconds, token).Wait(token);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
            {
                return false;
            }
        }

        /// <summary>
        /// Delay with global stop check only (no character-specific token).
        /// Use when character ID is not available.
        /// </summary>
        public static bool Delay(int milliseconds)
        {
            if (Helper.IsStoppingAll())
                return false;

            // Split into smaller chunks to check stop flag periodically
            const int checkInterval = 100; // Check every 100ms
            int remaining = milliseconds;

            while (remaining > 0)
            {
                int sleepTime = Math.Min(remaining, checkInterval);
                Thread.Sleep(sleepTime);
                remaining -= sleepTime;

                if (Helper.IsStoppingAll())
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Short delay (uses Constant.TimeShort)
        /// </summary>
        public static bool DelayShort(string characterId) => Delay(Constant.TimeShort, characterId);
        public static bool DelayShort() => Delay(Constant.TimeShort);

        /// <summary>
        /// Medium delay (uses Constant.TimeMedium)
        /// </summary>
        public static bool DelayMedium(string characterId) => Delay(Constant.TimeMedium, characterId);
        public static bool DelayMedium() => Delay(Constant.TimeMedium);

        /// <summary>
        /// Long delay (uses Constant.TimeLong)
        /// </summary>
        public static bool DelayLong(string characterId) => Delay(Constant.TimeLong, characterId);
        public static bool DelayLong() => Delay(Constant.TimeLong);

        /// <summary>
        /// Async version for use in async methods.
        /// </summary>
        public static async Task<bool> DelayAsync(int milliseconds, CancellationToken token)
        {
            if (Helper.IsStoppingAll())
                return false;

            try
            {
                await Task.Delay(milliseconds, token);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        /// <summary>
        /// Wait for a condition with timeout and cancellation support.
        /// </summary>
        /// <param name="condition">Function that returns true when condition is met</param>
        /// <param name="characterId">Character ID for cancellation token lookup</param>
        /// <param name="timeoutMs">Maximum time to wait</param>
        /// <param name="checkIntervalMs">Interval between condition checks</param>
        /// <returns>True if condition was met, false if timeout or cancelled</returns>
        public static bool WaitFor(Func<bool> condition, string characterId, int timeoutMs = 30000, int checkIntervalMs = 500)
        {
            var startTime = DateTime.Now;
            var timeout = TimeSpan.FromMilliseconds(timeoutMs);

            while (DateTime.Now - startTime < timeout)
            {
                // Check for stop
                if (Helper.IsStoppingAll())
                    return false;

                // Check condition
                if (condition())
                    return true;

                // Wait before next check
                if (!Delay(checkIntervalMs, characterId))
                    return false;
            }

            return false; // Timeout
        }

        /// <summary>
        /// Retry an operation with cancellation support.
        /// </summary>
        public static bool Retry(Func<bool> operation, string characterId, int maxRetries = 3, int delayMs = 1000)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                if (Helper.IsStoppingAll())
                    return false;

                if (operation())
                    return true;

                if (attempt < maxRetries)
                {
                    if (!Delay(delayMs, characterId))
                        return false;
                }
            }

            return false;
        }
    }
}

