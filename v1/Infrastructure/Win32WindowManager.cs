using AutoVPT.Interfaces;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using VPT_Login.Libs;

namespace AutoVPT.Infrastructure
{
    /// <summary>
    /// Win32 API-based window manager implementation.
    /// Provides window finding and management capabilities.
    /// </summary>
    public class Win32WindowManager : IWindowManager
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out ClickHelper.RECT lpRect);

        [DllImport("user32.dll", EntryPoint = "IsWindowVisible")]
        private static extern bool IsWindowVisibleWin32(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Ansi)]
        private static extern bool SetWindowText(IntPtr hWnd, string strNewWindowName);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private const int SW_RESTORE = 9;
        private const uint WM_CLOSE = 0x0010;

        public IntPtr FindWindow(string characterId)
        {
            return ClickHelper.GetHwndByTitle(characterId);
        }

        public IntPtr FindWindowByProcessId(int processId)
        {
            IntPtr foundHandle = IntPtr.Zero;

            Process process = null;
            try
            {
                process = Process.GetProcessById(processId);
                foundHandle = process.MainWindowHandle;
            }
            catch
            {
                return IntPtr.Zero;
            }

            return foundHandle;
        }

        public bool BringToFront(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                return false;

            ShowWindow(handle, SW_RESTORE);
            return SetForegroundWindow(handle);
        }

        public Rectangle GetWindowBounds(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                return Rectangle.Empty;

            if (GetWindowRect(handle, out ClickHelper.RECT rect))
            {
                return new Rectangle(
                    rect.Left,
                    rect.Top,
                    rect.Right - rect.Left,
                    rect.Bottom - rect.Top
                );
            }

            return Rectangle.Empty;
        }

        public bool IsWindowVisible(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                return false;

            return IsWindowVisibleWin32(handle);
        }

        public bool SetWindowTitle(IntPtr handle, string title)
        {
            if (handle == IntPtr.Zero)
                return false;

            return SetWindowText(handle, title);
        }

        public bool CloseWindow(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                return false;

            return PostMessage(handle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        public bool IsWindowValid(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                return false;

            // Check if window still exists by trying to get its rect
            return GetWindowRect(handle, out ClickHelper.RECT _);
        }

        public int GetProcessId(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                return 0;

            GetWindowThreadProcessId(handle, out uint processId);
            return (int)processId;
        }
    }
}
