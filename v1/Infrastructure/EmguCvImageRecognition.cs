using AutoVPT.Interfaces;
using Emgu.CV;
using KAutoHelper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Infrastructure
{
    /// <summary>
    /// Emgu.CV-based implementation of image recognition interface.
    /// Wraps existing ImageScanOpenCV and CaptureHelper functionality.
    /// </summary>
    public class EmguCvImageRecognition : IImageRecognition, IDisposable
    {
        private readonly IntPtr _windowHandle;
        private readonly Dictionary<string, Bitmap> _imageCache;
        private readonly object _cacheLock = new object();
        private bool _disposed = false;

        public EmguCvImageRecognition(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            _imageCache = new Dictionary<string, Bitmap>();
        }

        public async Task<Point?> FindImageAsync(string imagePath, Rectangle? searchArea = null, double threshold = 0.8)
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(imagePath))
                {
                    return null;
                }

                using (var screen = CaptureScreen(searchArea))
                {
                    if (screen == null)
                        return null;

                    var template = GetCachedImage(imagePath);
                    if (template == null)
                        return null;

                    var result = ImageScanOpenCV.FindOutPoint((Bitmap)screen, template, threshold);
                    return result;
                }
            });
        }

        public async Task<List<Point>> FindAllImagesAsync(string imagePath, Rectangle? searchArea = null, double threshold = 0.8)
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(imagePath))
                {
                    return new List<Point>();
                }

                using (var screen = CaptureScreen(searchArea))
                {
                    if (screen == null)
                        return new List<Point>();

                    var template = GetCachedImage(imagePath);
                    if (template == null)
                        return new List<Point>();

                    var results = ImageScanOpenCV.FindOutPoints((Bitmap)screen, template, threshold);
                    return results ?? new List<Point>();
                }
            });
        }

        public async Task<bool> WaitForImageAsync(string imagePath, TimeSpan timeout, CancellationToken ct = default)
        {
            var endTime = DateTime.Now.Add(timeout);

            while (DateTime.Now < endTime && !ct.IsCancellationRequested)
            {
                var result = await FindImageAsync(imagePath, null, 0.8);
                if (result.HasValue)
                    return true;

                await Task.Delay(100, ct);
            }

            return false;
        }

        public async Task<Bitmap> CaptureScreenAsync(Rectangle area)
        {
            return await Task.Run(() =>
            {
                var fullScreen = CaptureHelper.CaptureWindow(_windowHandle) as Bitmap;
                if (fullScreen == null)
                    return null;

                if (area.IsEmpty)
                    return fullScreen;

                // Crop to specified area
                try
                {
                    var cropped = fullScreen.Clone(area, fullScreen.PixelFormat);
                    fullScreen.Dispose();
                    return cropped;
                }
                catch
                {
                    return fullScreen;
                }
            });
        }

        public async Task<bool> ImageExistsAsync(string imagePath, Rectangle? searchArea = null, double threshold = 0.8)
        {
            var result = await FindImageAsync(imagePath, searchArea, threshold);
            return result.HasValue;
        }

        public void ClearCache()
        {
            lock (_cacheLock)
            {
                foreach (var image in _imageCache.Values)
                {
                    image?.Dispose();
                }
                _imageCache.Clear();
            }
        }

        private Bitmap GetCachedImage(string imagePath)
        {
            lock (_cacheLock)
            {
                if (!_imageCache.ContainsKey(imagePath))
                {
                    try
                    {
                        _imageCache[imagePath] = ImageScanOpenCV.GetImage(imagePath);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
                return _imageCache[imagePath];
            }
        }

        private Bitmap CaptureScreen(Rectangle? area)
        {
            var fullScreen = CaptureHelper.CaptureWindow(_windowHandle) as Bitmap;
            if (fullScreen == null)
                return null;

            if (!area.HasValue || area.Value.IsEmpty)
                return fullScreen;

            // Crop to specified area
            try
            {
                var cropped = fullScreen.Clone(area.Value, fullScreen.PixelFormat);
                fullScreen.Dispose();
                return cropped;
            }
            catch
            {
                return fullScreen;
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
                    ClearCache();
                }
                _disposed = true;
            }
        }

        ~EmguCvImageRecognition()
        {
            Dispose(false);
        }
    }
}
