using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoVPT.Objects
{
    public class Monster
    {
        public string imagePath;
        public int x;
        public int y;

        public Monster(string _imagePath, int _x, int _y)
        {
            imagePath = _imagePath;
            x = _x;
            y = _y;
        }
    }
}
