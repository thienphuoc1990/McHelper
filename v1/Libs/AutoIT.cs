using AutoItX3Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoVPT.Libs
{
    class AutoIT
    {
        AutoItX3 auto = new AutoItX3();

        public void clickRight(string title, int numclick, int X, int Y)
        {
            auto.ControlClick(title, "", "", "right", numclick, X, Y);
        }

        public void click(string title, int numclick, int X, int Y)
        {
            auto.ControlClick(title, "", "", "left", numclick, X, Y);
        }

        public void move(string title, int X, int Y)
        {
            auto.ControlMove(title, "", "", X, Y);
        }

        public void winclose(string title)
        {
            auto.WinClose(title);
        }

        public void winwait(string title)
        {
            auto.WinWait(title);
        }

        public void controlsend(string title, string all)
        {
            auto.ControlSend(title, "", "", all);

        }

        public int winexist(string title)
        {
            return auto.WinExists(title);
        }

        public void controlfocus(string title)
        {
            //auto.WinSetOnTop(title, "", 1);
            auto.ControlFocus(title, "", "");
        }

        public void optsendkey(int opt)
        {
            auto.Opt("SendKeyDelay", opt);
        }

        public string clipget()
        {
            var clipget = auto.ClipGet();
            return clipget;
        }

        public void clipput(string copy)
        {
            auto.ClipPut(copy);
        }
    }
}
