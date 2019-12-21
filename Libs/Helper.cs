using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoVPT.Libs
{
    static class Helper
    {
        public static List<Thread> threadList = new List<Thread>();

        public static void writeStatus(TextBox textBox, string statusText)
        {
            try
            {
                textBox.BeginInvoke(new Action(() => textBox.AppendText(statusText + Environment.NewLine)));
            }
            catch
            {
                
            }
            
        }
    }
}
