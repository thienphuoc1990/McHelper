using AutoVPT.Objects;
using AutoVPT.DependencyInjection;
using AutoVPT.Interfaces;
using AutoVPT.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoVPT.Libs
{
    static class Helper
    {
        /*
         * LOCK HIERARCHY - Thread Safety Guidelines
         * ==========================================
         * To prevent deadlocks, locks must ALWAYS be acquired in this order:
         *
         * 1. _stopLock     - Global stop flag (highest priority)
         * 2. _charLock     - Character registry operations
         * 3. _tokenLock    - Cancellation token operations
         * 4. _threadLock   - Thread list operations (lowest priority)
         *
         * IMPORTANT RULES:
         * - NEVER acquire locks in reverse order
         * - NEVER hold multiple locks unless absolutely necessary
         * - Always release locks in reverse order of acquisition
         * - Keep critical sections as short as possible
         *
         * Example of safe nested locking:
         *   lock(_stopLock) {
         *       lock(_charLock) {
         *           // Safe: acquired in correct order
         *       }
         *   }
         *
         * Example of UNSAFE nested locking:
         *   lock(_threadLock) {
         *       lock(_stopLock) {  // DEADLOCK RISK! Wrong order
         *       }
         *   }
         */

        public static List<Thread> threadList = new List<Thread>();
        public static Dictionary<string, CancellationTokenSource> cancellationTokens = new Dictionary<string, CancellationTokenSource>();
        public static Dictionary<string, Character> runningCharacters = new Dictionary<string, Character>();
        private static bool _isStoppingAll = false;
        private static object _tokenLock = new object();
        private static object _charLock = new object();
        internal static object _threadLock = new object(); // Made internal for Form1 access to fix race condition
        private static object _stopLock = new object();

        public static CancellationToken GetCancellationToken(string key)
        {
            lock (_tokenLock)
            {
                // If we're stopping all, create a cancelled token
                if (_isStoppingAll)
                {
                    var cancelledSource = new CancellationTokenSource();
                    cancelledSource.Cancel();
                    return cancelledSource.Token;
                }

                if (!cancellationTokens.ContainsKey(key))
                {
                    cancellationTokens[key] = new CancellationTokenSource();
                }
                return cancellationTokens[key].Token;
            }
        }

        public static void CancelToken(string key)
        {
            lock (_tokenLock)
            {
                if (cancellationTokens.ContainsKey(key))
                {
                    cancellationTokens[key].Cancel();
                    cancellationTokens[key].Dispose();
                    cancellationTokens.Remove(key);
                }
            }
        }

        public static void RemoveToken(string key)
        {
            lock (_tokenLock)
            {
                if (cancellationTokens.ContainsKey(key))
                {
                    cancellationTokens[key].Dispose();
                    cancellationTokens.Remove(key);
                }
            }
        }

        public static void RegisterRunningCharacter(Character character)
        {
            lock (_charLock)
            {
                if (character != null && !string.IsNullOrEmpty(character.ID))
                {
                    runningCharacters[character.ID] = character;
                }
            }
        }

        public static void UnregisterRunningCharacter(string characterId)
        {
            lock (_charLock)
            {
                if (runningCharacters.ContainsKey(characterId))
                {
                    runningCharacters.Remove(characterId);
                }
            }
        }

        public static void StopAllRunningCharacters()
        {
            // Set the stopping flag first to prevent new features from starting
            lock (_stopLock)
            {
                _isStoppingAll = true;
            }

            lock (_charLock)
            {
                foreach (var character in runningCharacters.Values)
                {
                    character.Running = 0;
                    saveSettingsToXML(character);
                }
            }
        }

        public static void ResetStopAllFlag()
        {
            lock (_stopLock)
            {
                _isStoppingAll = false;
            }
        }

        public static bool IsStoppingAll()
        {
            lock (_stopLock)
            {
                return _isStoppingAll;
            }
        }

        public static void AbortAllThreads()
        {
            // Make a copy of the thread list to avoid modification during iteration
            Thread[] threads;
            lock (_threadLock)
            {
                threads = threadList.ToArray();
            }

            foreach (var thread in threads)
            {
                try
                {
                    if (thread != null && thread.IsAlive)
                    {
                        // Only interrupt the thread (for sleeping threads)
                        // DO NOT use Thread.Abort() as it can crash the application
                        thread.Interrupt();
                    }
                }
                catch
                {
                    // Ignore errors during thread interrupt
                }
            }

            // Wait a short time for threads to respond to interrupts
            Thread.Sleep(500);

            // Clear the thread list (threads should stop gracefully via Running flag)
            lock (_threadLock)
            {
                threadList.Clear();
            }
        }

        public static void writeStatus(TextBox textBox, string id, string statusText)
        {
            try
            {
                // Check if character filter is active
                bool shouldDisplay = true;
                try
                {
                    var logger = ServiceContainer.GetService<ILogger>();
                    if (logger is CompositeLogger compositeLogger)
                    {
                        var uiLogger = compositeLogger.GetLoggers()
                            .OfType<UiLogger>()
                            .FirstOrDefault();

                        if (uiLogger != null)
                        {
                            var filterCharacterId = uiLogger.GetCharacterFilter();
                            if (filterCharacterId != null)
                            {
                                // Filter is active - only show if ID matches
                                shouldDisplay = string.Equals(id, filterCharacterId, StringComparison.OrdinalIgnoreCase);
                            }
                        }
                    }
                }
                catch
                {
                    // If we can't access the filter, default to showing the message
                    shouldDisplay = true;
                }

                if (shouldDisplay)
                {
                    textBox.BeginInvoke(new Action(() => textBox.AppendText(id + ": " + statusText + Environment.NewLine)));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(id, "writeStatus", ex);
            }
        }

        public static void showAlert(string id, string message)
        {
            MessageBox.Show(id + ": " + message);
        }

        /// <summary>
        /// Save character settings to database
        /// Note: Method name kept as 'saveSettingsToXML' for backward compatibility
        /// Now uses SQLite instead of XML files
        /// </summary>
        public static void saveSettingsToXML(Character character)
        {
            try
            {
                AutoVPT.Database.DatabaseHelper.SaveCharacter(character);
            }
            catch (Exception ex)
            {
                Logger.LogError(character.ID, "saveSettingsToXML", ex);
                throw;
            }
        }

        /// <summary>
        /// Load character settings from database
        /// Note: Method name kept as 'loadSettingsFromXML' for backward compatibility
        /// Now uses SQLite instead of XML files
        /// </summary>
        public static Character loadSettingsFromXML(string id)
        {
            try
            {
                return AutoVPT.Database.DatabaseHelper.LoadCharacter(id);
            }
            catch (Exception ex)
            {
                Logger.LogError(id, "loadSettingsFromXML", ex);
                return new Character { ID = id };
            }
        }

        /// <summary>
        /// Load character directly from XML file (used for migration)
        /// </summary>
        public static Character LoadFromXmlFile(string id)
        {
            System.IO.FileStream myFileStream = null;
            try
            {
                Character character = new Character();
                System.Xml.Serialization.XmlSerializer mySerializer = new System.Xml.Serialization.XmlSerializer(typeof(Character));
                var filePath = System.IO.Path.Combine(Application.StartupPath, "database", id + ".xml");

                if (!System.IO.File.Exists(filePath))
                {
                    Logger.LogWarning(id, "LoadFromXmlFile", $"Character file not found: {filePath}");
                    return character;
                }

                myFileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open);
                character = (Character)mySerializer.Deserialize(myFileStream);

                return character;
            }
            catch (Exception ex)
            {
                Logger.LogError(id, "LoadFromXmlFile", ex);
                return new Character(); // Return empty character on error
            }
            finally
            {
                myFileStream?.Close();
            }
        }
    }
}
