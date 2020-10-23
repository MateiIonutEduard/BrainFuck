using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Configuration;
using System.Windows.Forms;
#pragma warning disable

namespace Brainfuck
{
    public partial class Setup : Form
    {
        private string path;
        public Setup()
        {
            InitializeComponent();
            Icon = Icon.FromHandle(Properties.Resources.bf.GetHicon());
            path = ConfigurationManager.AppSettings["dmd"];
            
            if (!string.IsNullOrEmpty(path) && File.Exists($"{path}\\dmd.exe"))
            {
                var folder = Directory.GetParent(path).ToString();
                label1.ForeColor = Color.ForestGreen;
                label1.Text = "Path was found: " + folder;
                textBox1.Text = folder;
            }
            else
            {
                label1.ForeColor = Color.Red;
                label1.Text = "You have not selected the compiler path!";
            }

            panel1.Focus();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel1.Focus();
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            // Find the path manually...
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = dialog.SelectedPath;
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["dmd"].Value = dialog.SelectedPath;

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            else Close();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                label1.ForeColor = Color.ForestGreen;
                label1.Text = "Path was found: " + textBox1.Text;
                Close();
            } else
            {
                var name = Environment.GetEnvironmentVariable("path").Split(';');

                var path = (from node in name.ToList()
                            where File.Exists($"{node}\\dmd.exe")
                            select node).FirstOrDefault();

                textBox1.Text = path;
                label1.Text = "Path was found: " + textBox1.Text;

                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["dmd"].Value = path;

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                Close();
            }
        }
    }
}
