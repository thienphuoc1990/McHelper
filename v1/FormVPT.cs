using AutoVPT.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoVPT
{
    public partial class FormVPT : Form
    {
        private Character mCharacter;

        public FormVPT()
        {
            InitializeComponent();
        }

        public void setCharacter(Character _character)
        {
            mCharacter = _character;
        }

        public void runVPT()
        {
            axShockwaveFlash1.Stop();
            axShockwaveFlash1.Movie = mCharacter.Link;
            axShockwaveFlash1.Play();
        }

        private void FormVPT_Load(object sender, EventArgs e)
        {
            runVPT();
        }
    }
}
