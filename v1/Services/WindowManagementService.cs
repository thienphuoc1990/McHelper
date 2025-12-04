using AutoVPT.Libs;
using AutoVPT.Objects;
using KAutoHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using VPT_Login.Libs;

namespace AutoVPT.Services
{
    /// <summary>
    /// Service for managing game windows - opening, closing, and tracking processes.
    /// Extracted from GeneralFunctions to provide single-responsibility window management.
    /// </summary>
    public class WindowManagementService : IWindowManagementService
    {
        private static readonly Dictionary<string, Process> _gameProcesses = new Dictionary<string, Process>();
        private static readonly object _processLock = new object();

        [DllImport("user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Ansi)]
        private static extern bool SetWindowText(IntPtr hWnd, string strNewWindowName);

        /// <summary>
        /// Check if a game window is open for the given character.
        /// </summary>
        /// <param name="characterId">Character ID (window name)</param>
        /// <returns>True if window is open, false otherwise</returns>
        public bool IsWindowOpen(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
                return false;

            IntPtr hWnd = AutoControl.FindWindowHandle(null, characterId);
            return hWnd != IntPtr.Zero;
        }

        /// <summary>
        /// Get the window handle for a character.
        /// </summary>
        /// <param name="characterId">Character ID (window name)</param>
        /// <returns>Window handle, or IntPtr.Zero if not found</returns>
        public IntPtr GetWindowHandle(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
                return IntPtr.Zero;

            return AutoControl.FindWindowHandle(null, characterId);
        }

        /// <summary>
        /// Open a new game window for the character.
        /// </summary>
        /// <param name="character">Character settings</param>
        /// <returns>Window handle of the opened window</returns>
        public IntPtr OpenWindow(Character character)
        {
            if (character == null)
                throw new ArgumentNullException(nameof(character));

            if (string.IsNullOrEmpty(character.ID))
                throw new ArgumentException("Character ID cannot be empty", nameof(character));

            IntPtr defaultHWnd = IntPtr.Zero;
            string defaultWindowName = "Adobe Flash Player 10";

            try
            {
                // Check if process already exists
                lock (_processLock)
                {
                    if (_gameProcesses.TryGetValue(character.ID, out var existingProcess))
                    {
                        if (!existingProcess.HasExited)
                        {
                            // Process already running, return existing handle
                            return GetWindowHandle(character.ID);
                        }
                        else
                        {
                            // Process exited, clean up
                            existingProcess.Dispose();
                            _gameProcesses.Remove(character.ID);
                        }
                    }
                }

                // Start new process
                var process = Process.Start("flash.exe", character.Link);

                // Track the process
                lock (_processLock)
                {
                    _gameProcesses[character.ID] = process;
                }

                // Wait for window to appear and rename it
                int maxAttempts = 100;
                int attempts = 0;
                do
                {
                    defaultHWnd = AutoControl.FindWindowHandle(null, defaultWindowName);

                    if (defaultHWnd != IntPtr.Zero)
                    {
                        SetWindowText(defaultHWnd, character.ID);
                        break;
                    }
                    else
                    {
                        Thread.Sleep(100);
                        attempts++;
                    }
                } while (attempts < maxAttempts);

                return GetWindowHandle(character.ID);
            }
            catch (Exception ex)
            {
                Logger.LogError(character.ID, "WindowManagementService.OpenWindow", ex);
                throw;
            }
        }

        /// <summary>
        /// Close and cleanup game window/process for a character.
        /// </summary>
        /// <param name="characterId">Character ID</param>
        /// <param name="forceKill">Force kill if graceful close fails</param>
        public void CloseWindow(string characterId, bool forceKill = true)
        {
            if (string.IsNullOrEmpty(characterId))
                return;

            try
            {
                lock (_processLock)
                {
                    if (_gameProcesses.TryGetValue(characterId, out var process))
                    {
                        try
                        {
                            if (!process.HasExited)
                            {
                                // Try graceful close first
                                process.CloseMainWindow();

                                // Wait up to 5 seconds for graceful close
                                if (!process.WaitForExit(5000) && forceKill)
                                {
                                    // Force kill if not closed
                                    process.Kill();
                                    process.WaitForExit(1000);
                                }
                            }
                        }
                        finally
                        {
                            process.Dispose();
                        }
                        _gameProcesses.Remove(characterId);
                    }
                }

                // Also try to close by window handle in case process tracking failed
                IntPtr hWnd = GetWindowHandle(characterId);
                if (hWnd != IntPtr.Zero)
                {
                    ClickHelper.CloseWindow(hWnd);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(characterId, "WindowManagementService.CloseWindow", ex);
            }
        }

        /// <summary>
        /// Set the name/title of a window.
        /// </summary>
        /// <param name="hWnd">Window handle</param>
        /// <param name="name">New window name</param>
        public void SetWindowName(IntPtr hWnd, string name)
        {
            if (hWnd == IntPtr.Zero || string.IsNullOrEmpty(name))
                return;

            SetWindowText(hWnd, name);
        }

        /// <summary>
        /// Check if a process is being tracked for a character.
        /// </summary>
        /// <param name="characterId">Character ID</param>
        /// <returns>True if process is tracked and running</returns>
        public bool IsProcessRunning(string characterId)
        {
            lock (_processLock)
            {
                if (_gameProcesses.TryGetValue(characterId, out var process))
                {
                    return !process.HasExited;
                }
                return false;
            }
        }

        /// <summary>
        /// Cleanup all tracked processes (for application shutdown).
        /// </summary>
        public static void CleanupAllProcesses()
        {
            lock (_processLock)
            {
                foreach (var kvp in _gameProcesses)
                {
                    try
                    {
                        if (!kvp.Value.HasExited)
                        {
                            kvp.Value.CloseMainWindow();
                            if (!kvp.Value.WaitForExit(2000))
                            {
                                kvp.Value.Kill();
                            }
                        }
                        kvp.Value.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(kvp.Key, "CleanupAllProcesses", ex);
                    }
                }
                _gameProcesses.Clear();
            }
        }
    }

    /// <summary>
    /// Interface for window management operations.
    /// </summary>
    public interface IWindowManagementService
    {
        bool IsWindowOpen(string characterId);
        IntPtr GetWindowHandle(string characterId);
        IntPtr OpenWindow(Character character);
        void CloseWindow(string characterId, bool forceKill = true);
        void SetWindowName(IntPtr hWnd, string name);
        bool IsProcessRunning(string characterId);
    }
}

