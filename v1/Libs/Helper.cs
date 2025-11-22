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
        private static object _tokenLock = new object();

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
