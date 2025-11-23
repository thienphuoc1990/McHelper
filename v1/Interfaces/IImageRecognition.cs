using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Interfaces
{
    /// <summary>
    /// Abstraction for image recognition operations.
    /// Decouples business logic from Emgu.CV implementation.
    /// </summary>
    public interface IImageRecognition
    {
        /// <summary>
        /// Find a single image on screen
        /// </summary>
        /// <param name="imagePath">Path to template image</param>
        /// <param name="searchArea">Optional area to search within (null = entire window)</param>
        /// <param name="threshold">Similarity threshold (0.0 to 1.0)</param>
        /// <returns>Location of found image, or null if not found</returns>
        Task<Point?> FindImageAsync(string imagePath, Rectangle? searchArea = null, double threshold = 0.8);

        /// <summary>
        /// Find all occurrences of an image on screen
        /// </summary>
        Task<List<Point>> FindAllImagesAsync(string imagePath, Rectangle? searchArea = null, double threshold = 0.8);

        /// <summary>
        /// Wait for an image to appear on screen
        /// </summary>
        /// <param name="imagePath">Path to template image</param>
        /// <param name="timeout">Maximum time to wait</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if image found within timeout, false otherwise</returns>
        Task<bool> WaitForImageAsync(string imagePath, TimeSpan timeout, CancellationToken ct = default);

        /// <summary>
        /// Capture screenshot of specified area
        /// </summary>
        Task<Bitmap> CaptureScreenAsync(Rectangle area);

        /// <summary>
        /// Check if image exists without waiting
        /// </summary>
        Task<bool> ImageExistsAsync(string imagePath, Rectangle? searchArea = null, double threshold = 0.8);

        /// <summary>
        /// Clear any cached images to free memory
        /// </summary>
        void ClearCache();
    }
}
