using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoVPT.Interfaces
{
    /// <summary>
    /// Abstraction for input simulation (mouse and keyboard).
    /// Decouples business logic from KAutoHelper.dll implementation.
    /// </summary>
    public interface IInputSimulator
    {
        /// <summary>
        /// Perform a single mouse click at the specified location
        /// </summary>
        /// <param name="location">Screen coordinates to click</param>
        /// <param name="delayAfterMs">Delay after click in milliseconds</param>
        Task ClickAsync(Point location, int delayAfterMs = 100);

        /// <summary>
        /// Perform a double click at the specified location
        /// </summary>
        Task DoubleClickAsync(Point location, int delayAfterMs = 100);

        /// <summary>
        /// Perform a right click at the specified location
        /// </summary>
        Task RightClickAsync(Point location, int delayAfterMs = 100);

        /// <summary>
        /// Send text input to the active window
        /// </summary>
        Task SendTextAsync(string text, int delayAfterMs = 100);

        /// <summary>
        /// Send a single key press
        /// </summary>
        Task SendKeyAsync(Keys key, int delayAfterMs = 100);

        /// <summary>
        /// Send multiple key presses in sequence
        /// </summary>
        Task SendKeysAsync(Keys[] keys, int delayBetweenMs = 50);

        /// <summary>
        /// Move mouse to specified location
        /// </summary>
        Task MoveMouseAsync(Point location);

        /// <summary>
        /// Drag mouse from one location to another
        /// </summary>
        Task DragAsync(Point from, Point to, int durationMs = 500);
    }
}
