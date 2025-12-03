using System;
using System.Collections.Generic;
using System.Drawing;

namespace AutoVPT.Infrastructure
{
    /// <summary>
    /// LRU (Least Recently Used) cache implementation for image caching.
    /// Automatically evicts least recently used items when capacity is reached.
    /// OPTIMIZATION 3: Memory-controlled caching with LRU eviction policy.
    /// </summary>
    public class LruCache<TKey, TValue> : IDisposable where TValue : class, IDisposable
    {
        private readonly int _capacity;
        private readonly Dictionary<TKey, LinkedListNode<LruItem>> _cache;
        private readonly LinkedList<LruItem> _lruList;
        private readonly object _lock = new object();
        private bool _disposed = false;

        private class LruItem
        {
            public TKey Key { get; set; }
            public TValue Value { get; set; }
        }

        public LruCache(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException("Capacity must be positive", nameof(capacity));

            _capacity = capacity;
            _cache = new Dictionary<TKey, LinkedListNode<LruItem>>(capacity);
            _lruList = new LinkedList<LruItem>();
        }

        /// <summary>
        /// Get or set value in cache
        /// </summary>
        public TValue Get(TKey key)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var node))
                {
                    // Move to front (most recently used)
                    _lruList.Remove(node);
                    _lruList.AddFirst(node);
                    return node.Value.Value;
                }
                return null;
            }
        }

        /// <summary>
        /// Add or update value in cache
        /// </summary>
        public void Set(TKey key, TValue value)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var existingNode))
                {
                    // Update existing item
                    existingNode.Value.Value?.Dispose();
                    existingNode.Value.Value = value;
                    _lruList.Remove(existingNode);
                    _lruList.AddFirst(existingNode);
                }
                else
                {
                    // Add new item
                    if (_cache.Count >= _capacity)
                    {
                        // Evict least recently used item
                        var lruNode = _lruList.Last;
                        if (lruNode != null)
                        {
                            lruNode.Value.Value?.Dispose();
                            _cache.Remove(lruNode.Value.Key);
                            _lruList.RemoveLast();
                        }
                    }

                    var newItem = new LruItem { Key = key, Value = value };
                    var newNode = _lruList.AddFirst(newItem);
                    _cache[key] = newNode;
                }
            }
        }

        /// <summary>
        /// Check if key exists in cache
        /// </summary>
        public bool ContainsKey(TKey key)
        {
            lock (_lock)
            {
                return _cache.ContainsKey(key);
            }
        }

        /// <summary>
        /// Clear all items from cache
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                foreach (var node in _cache.Values)
                {
                    node.Value.Value?.Dispose();
                }
                _cache.Clear();
                _lruList.Clear();
            }
        }

        /// <summary>
        /// Get current cache size
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _cache.Count;
                }
            }
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
                    Clear();
                }
                _disposed = true;
            }
        }

        ~LruCache()
        {
            Dispose(false);
        }
    }
}
