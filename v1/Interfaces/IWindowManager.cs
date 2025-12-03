using System;
using System.Drawing;
using System.Threading.Tasks;

namespace AutoVPT.Interfaces
{
    /// <summary>
    /// Abstraction for window management operations.
    /// Handles finding and controlling Flash Player windows.
    /// </summary>
    public interface IWindowManager
    {
        /// <summary>
        /// Find window handle by character ID (window title)
        /// </summary>
        /// <param name="characterId">Character ID used as window title</param>
        /// <returns>Window handle, or IntPtr.Zero if not found</returns>
        IntPtr FindWindow(string characterId);

        /// <summary>
        /// Find window handle by process ID
        /// </summary>
        IntPtr FindWindowByProcessId(int processId);

        /// <summary>
        /// Bring window to foreground
        /// </summary>
        /// <param name="handle">Window handle</param>
        /// <returns>True if successful</returns>
        bool BringToFront(IntPtr handle);

        /// <summary>
        /// Get window bounds (position and size)
        /// </summary>
        Rectangle GetWindowBounds(IntPtr handle);

        /// <summary>
        /// Check if window is visible
        /// </summary>
        bool IsWindowVisible(IntPtr handle);

        /// <summary>
        /// Set window title
        /// </summary>
        bool SetWindowTitle(IntPtr handle, string title);

        /// <summary>
        /// Close window
        /// </summary>
        bool CloseWindow(IntPtr handle);

        /// <summary>
        /// Check if window exists and is valid
        /// </summary>
        bool IsWindowValid(IntPtr handle);

        /// <summary>
        /// Get process ID from window handle
        /// </summary>
        int GetProcessId(IntPtr handle);
    }
}
