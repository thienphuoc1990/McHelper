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

        public static void writeStatus(TextBox textBox, string id, string statusText)
        {
            try
            {
                textBox.BeginInvoke(new Action(() => textBox.AppendText(id + ": " + statusText + Environment.NewLine)));
            }
            catch
            {
                
            }
            
        }

        public static void showAlert(string id, string message)
        {
            MessageBox.Show(id + ": " + message);
        }

        public static void saveSettingsToXML(Character character)
        {
            XmlSerializer mySerializer = new XmlSerializer(typeof(Character));
            StreamWriter myWriter = new StreamWriter(Application.StartupPath + "\\database\\" + character.ID + ".xml");
            mySerializer.Serialize(myWriter, character);
            myWriter.Close();
        }

        public static Character loadSettingsFromXML(string id)
        {
            Character character = new Character();
            XmlSerializer mySerializer = new XmlSerializer(typeof(Character));
            FileStream myFileStream = new FileStream(Application.StartupPath + "\\database\\" + id + ".xml", FileMode.Open);

            character = (Character)mySerializer.Deserialize(myFileStream);
            myFileStream.Close();

            return character;
        }
    }
}
