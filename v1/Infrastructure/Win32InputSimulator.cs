using AutoVPT.Interfaces;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VPT_Login.Libs;

namespace AutoVPT.Infrastructure
{
    /// <summary>
    /// Win32 API-based input simulator implementation.
    /// Wraps ClickHelper functionality to provide async interface.
    /// </summary>
    public class Win32InputSimulator : IInputSimulator
    {
        private readonly IntPtr _windowHandle;

        public Win32InputSimulator(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
        }

        public async Task ClickAsync(Point location, int delayAfterMs = 100)
        {
            await Task.Run(() =>
            {
                ClickHelper.Click(_windowHandle, 1, location.X, location.Y);
                if (delayAfterMs > 0)
                    Thread.Sleep(delayAfterMs);
            });
        }

        public async Task DoubleClickAsync(Point location, int delayAfterMs = 100)
        {
            await Task.Run(() =>
            {
                ClickHelper.Click(_windowHandle, 2, location.X, location.Y);
                if (delayAfterMs > 0)
                    Thread.Sleep(delayAfterMs);
            });
        }

        public async Task RightClickAsync(Point location, int delayAfterMs = 100)
        {
            await Task.Run(() =>
            {
                ClickHelper.ClickRight(_windowHandle, 1, location.X, location.Y);
                if (delayAfterMs > 0)
                    Thread.Sleep(delayAfterMs);
            });
        }

        public async Task SendTextAsync(string text, int delayAfterMs = 100)
        {
            await Task.Run(() =>
            {
                ClickHelper.SendTextToWindow(_windowHandle, text);
                if (delayAfterMs > 0)
                    Thread.Sleep(delayAfterMs);
            });
        }

        public async Task SendKeyAsync(Keys key, int delayAfterMs = 100)
        {
            await Task.Run(() =>
            {
                ClickHelper.ControlSendKey(_windowHandle, key);
                if (delayAfterMs > 0)
                    Thread.Sleep(delayAfterMs);
            });
        }

        public async Task SendKeysAsync(Keys[] keys, int delayBetweenMs = 50)
        {
            await Task.Run(() =>
            {
                foreach (var key in keys)
                {
                    ClickHelper.ControlSendKey(_windowHandle, key);
                    if (delayBetweenMs > 0)
                        Thread.Sleep(delayBetweenMs);
                }
            });
        }

        public async Task MoveMouseAsync(Point location)
        {
            await Task.Run(() =>
            {
                ClickHelper.MoveCursorToWindow(_windowHandle, location.X, location.Y);
            });
        }

        public async Task DragAsync(Point from, Point to, int durationMs = 500)
        {
            await Task.Run(() =>
            {
                // Move to start position
                ClickHelper.MoveCursorToWindow(_windowHandle, from.X, from.Y);
                Thread.Sleep(50);

                // Calculate steps for smooth drag
                int steps = Math.Max(1, durationMs / 20);
                double dx = (to.X - from.X) / (double)steps;
                double dy = (to.Y - from.Y) / (double)steps;

                // Simulate mouse down
                // Note: This is simplified - may need enhancement for actual drag operation
                ClickHelper.Click(_windowHandle, 1, from.X, from.Y);

                // Move in steps
                for (int i = 1; i <= steps; i++)
                {
                    int x = from.X + (int)(dx * i);
                    int y = from.Y + (int)(dy * i);
                    ClickHelper.MoveCursorToWindow(_windowHandle, x, y);
                    Thread.Sleep(20);
                }

                // Mouse up at end position
                Thread.Sleep(50);
            });
        }
    }
}
