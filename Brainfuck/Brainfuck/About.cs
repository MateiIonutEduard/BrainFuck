using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Brainfuck
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            Icon = Icon.FromHandle(Properties.Resources.bf.GetHicon());
        }
    }
}
