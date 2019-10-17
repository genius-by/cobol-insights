using Kbg.NppPluginNET.PluginInfrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kbg.NppPluginNET
{
    public partial class FrmAddVerticalLine : Form
    {
        public int Position = 0;
        public Colour Colour = new Colour(153, 189, 255);
        public FrmAddVerticalLine()
        {
            InitializeComponent();
            button2.BackColor = Color.FromArgb(Colour.Red, Colour.Green, Colour.Blue);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Position = int.Parse(textBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = Color.FromArgb(Colour.Red, Colour.Green, Colour.Blue);
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                button2.BackColor = colorDialog1.Color;
                Colour = new Colour(colorDialog1.Color.R, colorDialog1.Color.G, colorDialog1.Color.B);
            }
        }
    }
}
