using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brainfuck
{
    public partial class Help : Form
    {
        public Help()
        {
            InitializeComponent();
            Icon = Icon.FromHandle(Properties.Resources.bf.GetHicon());
        }

        private void link_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.hevanet.com/cristofd/brainfuck/");
        }
    }
}
