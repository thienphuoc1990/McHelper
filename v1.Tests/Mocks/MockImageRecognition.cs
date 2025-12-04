using AutoVPT.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of IImageRecognition for testing.
    /// Allows configuring expected image search results.
    /// </summary>
    public class MockImageRecognition : IImageRecognition
    {
        private readonly Dictionary<string, bool> _findResults = new Dictionary<string, bool>();
        private readonly Dictionary<string, Point> _findPointResults = new Dictionary<string, Point>();
        private readonly List<string> _findCalls = new List<string>();

        /// <summary>
        /// Gets all image search calls made
        /// </summary>
        public IReadOnlyList<string> FindCalls => _findCalls;

        /// <summary>
        /// Configure an image to be found (returns true)
        /// </summary>
        public void SetImageFound(string imagePath, bool found = true)
        {
            _findResults[imagePath] = found;
        }

        /// <summary>
        /// Configure an image to be found at a specific point
        /// </summary>
        public void SetImageFoundAt(string imagePath, Point location)
        {
            _findResults[imagePath] = true;
            _findPointResults[imagePath] = location;
        }

        /// <summary>
        /// Configure multiple images as found
        /// </summary>
        public void SetImagesFound(params string[] imagePaths)
        {
            foreach (var path in imagePaths)
            {
                _findResults[path] = true;
            }
        }

        /// <summary>
        /// Clear all configured results and recorded calls
        /// </summary>
        public void Reset()
        {
            _findResults.Clear();
            _findPointResults.Clear();
            _findCalls.Clear();
        }

        // IImageRecognition implementation

        public Task<Point?> FindImageAsync(string imagePath, Rectangle? searchArea = null, double threshold = 0.8)
        {
            _findCalls.Add(imagePath);
            
            if (_findPointResults.ContainsKey(imagePath))
            {
                return Task.FromResult<Point?>(_findPointResults[imagePath]);
            }
            
            if (_findResults.ContainsKey(imagePath) && _findResults[imagePath])
            {
                return Task.FromResult<Point?>(new Point(100, 100));
            }
            
            return Task.FromResult<Point?>(null);
        }

        public Task<List<Point>> FindAllImagesAsync(string imagePath, Rectangle? searchArea = null, double threshold = 0.8)
        {
            _findCalls.Add(imagePath);
            
            var results = new List<Point>();
            if (_findResults.ContainsKey(imagePath) && _findResults[imagePath])
            {
                if (_findPointResults.ContainsKey(imagePath))
                {
                    results.Add(_findPointResults[imagePath]);
                }
                else
                {
                    results.Add(new Point(100, 100));
                }
            }
            
            return Task.FromResult(results);
        }

        public Task<bool> WaitForImageAsync(string imagePath, TimeSpan timeout, CancellationToken ct = default)
        {
            _findCalls.Add(imagePath);
            var found = _findResults.ContainsKey(imagePath) && _findResults[imagePath];
            return Task.FromResult(found);
        }

        public Task<Bitmap> CaptureScreenAsync(Rectangle area)
        {
            // Return a small dummy bitmap for testing
            var bitmap = new Bitmap(area.Width > 0 ? area.Width : 100, area.Height > 0 ? area.Height : 100);
            return Task.FromResult(bitmap);
        }

        public Task<bool> ImageExistsAsync(string imagePath, Rectangle? searchArea = null, double threshold = 0.8)
        {
            _findCalls.Add(imagePath);
            var found = _findResults.ContainsKey(imagePath) && _findResults[imagePath];
            return Task.FromResult(found);
        }

        public void ClearCache()
        {
            // No-op for mock
        }
    }
}
