using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoVPT.Objects
{
    class Map
    {
        public string name;
        public int mapIndex;
        public int x;
        public int y;
        public List<string> posPaths;

        public Map(string _name, int _mapIndex, int _x, int _y)
        {
            name = _name;
            mapIndex = _mapIndex;
            x = _x;
            y = _y;
        }

        public void setPosPaths(List<string> _posPaths)
        {
            posPaths = _posPaths;
        }
    }
}
