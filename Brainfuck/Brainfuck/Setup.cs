using System;
using System.IO;
using System.Drawing;
using Newtonsoft.Json;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Brainfuck
{
    public partial class Setup : Form
    {
        private string path;
        public Setup()
        {
            InitializeComponent();
            Icon = Icon.FromHandle(Properties.Resources.bf.GetHicon());
            path = GetPath();

            if (path != string.Empty && File.Exists(path))
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

            if (dialog.ShowDialog() == DialogResult.OK)
                textBox1.Text = dialog.SelectedPath;
            else Close();
        }

        private string GetPath()
        {
            try
            {
                var data = File.ReadAllText("setup.json");
                var jObject = JObject.Parse(data);
                if (jObject != null) return jObject["path"].ToString();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Environment.Exit(-1);
            }

            return null;
        }

        private void SetPath(string folder)
        {
            try
            {
                var data = File.ReadAllText("setup.json");
                var jObject = JObject.Parse(data);
                if (jObject != null) jObject["path"] = path = Path.Combine(folder, "dmd.exe");
                string output = JsonConvert.SerializeObject(jObject);
                File.WriteAllText("setup.json", output);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                Environment.Exit(-1);
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            label1.ForeColor = Color.ForestGreen;
            label1.Text = "Path was found: " + textBox1.Text;
            SetPath(textBox1.Text);
            Close();
        }
    }
}
