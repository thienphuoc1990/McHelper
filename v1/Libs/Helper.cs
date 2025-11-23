using AutoVPT.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace AutoVPT.Libs
{
    static class Helper
    {
        public static List<Thread> threadList = new List<Thread>();
        public static Dictionary<string, CancellationTokenSource> cancellationTokens = new Dictionary<string, CancellationTokenSource>();
        public static Dictionary<string, Character> runningCharacters = new Dictionary<string, Character>();
        private static object _tokenLock = new object();
        private static object _charLock = new object();
        private static object _threadLock = new object();

        public static CancellationToken GetCancellationToken(string key)
        {
            lock (_tokenLock)
            {
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
            lock (_charLock)
            {
                foreach (var character in runningCharacters.Values)
                {
                    character.Running = 0;
                    saveSettingsToXML(character);
                }
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
                        // Try to interrupt the thread first (for sleeping threads)
                        thread.Interrupt();

                        // Give it a moment to wake up and check Running flag
                        Thread.Sleep(100);

                        // If still alive, abort it
                        if (thread.IsAlive)
                        {
#pragma warning disable CS0618 // Thread.Abort is obsolete but necessary here
                            thread.Abort();
#pragma warning restore CS0618
                        }
                    }
                }
                catch
                {
                    // Ignore errors during thread abort
                }
            }

            // Clear the thread list
            lock (_threadLock)
            {
                threadList.Clear();
            }
        }

        public static void writeStatus(TextBox textBox, string id, string statusText)
        {
            try
            {
                textBox.BeginInvoke(new Action(() => textBox.AppendText(id + ": " + statusText + Environment.NewLine)));
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

        public static void saveSettingsToXML(Character character)
        {
            StreamWriter myWriter = null;
            try
            {
                var dbPath = Path.Combine(Application.StartupPath, "database");
                Directory.CreateDirectory(dbPath);

                XmlSerializer mySerializer = new XmlSerializer(typeof(Character));
                myWriter = new StreamWriter(Path.Combine(dbPath, character.ID + ".xml"));
                mySerializer.Serialize(myWriter, character);
            }
            catch (Exception ex)
            {
                Logger.LogError(character.ID, "saveSettingsToXML", ex);
                throw; // Re-throw to maintain existing behavior
            }
            finally
            {
                myWriter?.Close();
            }
        }

        public static Character loadSettingsFromXML(string id)
        {
            FileStream myFileStream = null;
            try
            {
                Character character = new Character();
                XmlSerializer mySerializer = new XmlSerializer(typeof(Character));
                var filePath = Path.Combine(Application.StartupPath, "database", id + ".xml");

                if (!File.Exists(filePath))
                {
                    Logger.LogWarning(id, "loadSettingsFromXML", $"Character file not found: {filePath}");
                    return character;
                }

                myFileStream = new FileStream(filePath, FileMode.Open);
                character = (Character)mySerializer.Deserialize(myFileStream);

                return character;
            }
            catch (Exception ex)
            {
                Logger.LogError(id, "loadSettingsFromXML", ex);
                return new Character(); // Return empty character on error
            }
            finally
            {
                myFileStream?.Close();
            }
        }
    }
}
