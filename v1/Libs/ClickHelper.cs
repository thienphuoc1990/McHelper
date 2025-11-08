using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace VPT_Login.Libs
{
    public static class ClickHelper
    {
        const int WM_CHAR = 0x0102;
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;
        const int WM_RBUTTONDOWN = 0x0204;
        const int WM_RBUTTONUP = 0x0205;
        const int WM_CLOSE = 0x0010;
        const int SW_RESTORE = 9;
        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);

        public static void Click(IntPtr hwnd, int numClick, int x, int y)
        {
            if (hwnd == IntPtr.Zero) return;

            for (int i = 0; i < numClick; i++)
            {
                SendClick(hwnd, x, y, rightClick: false);
            }
        }

        public static void ClickRight(IntPtr hwnd, int numClick, int x, int y)
        {
            if (hwnd == IntPtr.Zero) return;

            for (int i = 0; i < numClick; i++)
            {
                SendClick(hwnd, x, y, rightClick: true);
            }
        }

        private static void SendClick(IntPtr hwnd, int x, int y, bool rightClick)
        {
            int lParam = (y << 16) | (x & 0xFFFF);
            uint down = (uint)(rightClick ? WM_RBUTTONDOWN : WM_LBUTTONDOWN);
            uint up = (uint)(rightClick ? WM_RBUTTONUP : WM_LBUTTONUP);

            PostMessage(hwnd, down, (IntPtr)1, (IntPtr)lParam);
            PostMessage(hwnd, up, IntPtr.Zero, (IntPtr)lParam);
        }

        public static void MoveCursorToWindow(IntPtr hwnd, int x, int y)
        {
            if (hwnd == IntPtr.Zero) return;

            if (GetWindowRect(hwnd, out RECT rect))
            {
                int globalX = rect.Left + x;
                int globalY = rect.Top + y;
                SetCursorPos(globalX, globalY);
            }
        }

        public static void CloseWindow(IntPtr hwnd)
        {
            if (hwnd != IntPtr.Zero)
            {
                PostMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
        }

        public static void SendTextToWindow(IntPtr hwnd, string text)
        {
            if (hwnd == IntPtr.Zero || string.IsNullOrEmpty(text)) return;

            foreach (char c in text)
            {
                PostMessage(hwnd, WM_CHAR, (IntPtr)c, IntPtr.Zero);
            }
        }

        public static void FocusWindow(IntPtr hwnd)
        {
            if (hwnd != IntPtr.Zero)
            {
                ShowWindow(hwnd, SW_RESTORE);
                SetForegroundWindow(hwnd);
            }
        }

        public static void SetSendKeyDelay(int milliseconds)
        {
            Thread.Sleep(milliseconds); // bạn có thể dùng khi gửi từng phím một
        }

        public static string GetClipboardText()
        {
            try { return Clipboard.GetText(); }
            catch { return string.Empty; }
        }

        public static void SetClipboardText(string text)
        {
            try { Clipboard.SetText(text); }
            catch { }
        }

        public static IntPtr GetHwndByTitle(string title)
        {
            return FindWindow(null, title);
        }

        public static void WaitForWindow(string title, int timeoutMs = 5000)
        {
            DateTime start = DateTime.Now;
            while ((DateTime.Now - start).TotalMilliseconds < timeoutMs)
            {
                IntPtr hwnd = FindWindow(null, title);
                if (hwnd != IntPtr.Zero) break;
                Thread.Sleep(100);
            }
        }

        public static bool WindowExists(string title)
        {
            return FindWindow(null, title) != IntPtr.Zero;
        }

        public static IntPtr FindWindowHandle(string className, string windowName)
        {
            IntPtr zero = IntPtr.Zero;
            return FindWindow(className, windowName);
        }

        public static void ControlSendKey(IntPtr hwnd, Keys key)
        {
            if (hwnd == IntPtr.Zero) return;

            int vkCode = (int)key;
            int scanCode = (int)MapVirtualKey((uint)vkCode, 0);
            int lParamDown = (0x00000001 | (scanCode << 16));
            int lParamUp = unchecked((int)(0xC0000001 | ((long)scanCode << 16)));

            PostMessage(hwnd, WM_KEYDOWN, (IntPtr)vkCode, (IntPtr)lParamDown);
            PostMessage(hwnd, WM_KEYUP, (IntPtr)vkCode, (IntPtr)lParamUp);
        }

    }
}
