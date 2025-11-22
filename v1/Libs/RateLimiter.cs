using System;
using System.Threading;

namespace AutoVPT.Libs
{
    /// <summary>
    /// Rate limiter to prevent overwhelming the system with too many concurrent operations
    /// </summary>
    public class RateLimiter : IDisposable
    {
        private SemaphoreSlim _semaphore;
        private int _maxConcurrent;
        private bool _disposed = false;

        public RateLimiter(int maxConcurrent = 3)
        {
            _maxConcurrent = maxConcurrent;
            _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
        }

        /// <summary>
        /// Execute an action with rate limiting
        /// </summary>
        public void Execute(Action action, int timeoutMs = 30000)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(RateLimiter));
            }

            try
            {
                // Wait for a slot to become available
                if (_semaphore.Wait(timeoutMs))
                {
                    try
                    {
                        action();
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
                else
                {
                    Logger.LogWarning("System", "RateLimiter.Execute", $"Timeout after {timeoutMs}ms waiting for rate limiter slot");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("System", "RateLimiter.Execute", ex);
                throw;
            }
        }

        /// <summary>
        /// Get the number of available slots
        /// </summary>
        public int AvailableSlots
        {
            get { return _semaphore.CurrentCount; }
        }

        /// <summary>
        /// Wait for all operations to complete
        /// </summary>
        public bool WaitForAll(int timeoutMs = 30000)
        {
            DateTime start = DateTime.Now;
            while (_semaphore.CurrentCount < _maxConcurrent)
            {
                if ((DateTime.Now - start).TotalMilliseconds > timeoutMs)
                {
                    return false;
                }
                Thread.Sleep(100);
            }
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Wait for all operations to complete
                    WaitForAll(10000);
                    _semaphore?.Dispose();
                }
                _disposed = true;
            }
        }

        ~RateLimiter()
        {
            Dispose(false);
        }
    }
}
