using System;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace Brainfuck
{
    public partial class Brainfuck : Form
    {
        private int ID = 1;
        private List<string> tbn;
        private List<string> tfp;

        private string input;
        private BackgroundWorker bw;

        private string code;
        private BuildState bs;
        private string path = "";
        private string msg = "";

        public Brainfuck()
        {
            InitializeComponent();
            Icon = Icon.FromHandle(Properties.Resources.bf.GetHicon());
            code = "";

            bs = BuildState.Disabled;
            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;

            bw.DoWork += CodeRunner;
            bw.RunWorkerCompleted += BuildComplete;
            tbn = new List<string>();
            tfp = new List<string>();
        }

        private string GetPath()
        {
            try
            {
                var data = File.ReadAllText("setup.json");
                var jObject = JObject.Parse(data);
                if (jObject != null) return jObject["path"].ToString();
            } catch(Exception e)
            {
                MessageBox.Show(e.Message);
                Environment.Exit(-1);
            }

            return null;
        }

        private void BuildComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            // So, the compiler has finalized the job, it is interprets
            // the program code or build it into executable, after that it is running in terminal.
            if(bs == BuildState.Interpret)
            {
                textBox1.AppendText(Environment.NewLine);
                textBox1.AppendText("Build succeed.");
            }
            else if(bs == BuildState.BuildRun)
            {
                textBox1.AppendText(Environment.NewLine);
                textBox1.AppendText(msg);
            }
        }

        private void CodeRunner(object sender, DoWorkEventArgs e)
        {
            // It run the program's code or it is built it and run in terminal.
            if (bs == BuildState.Interpret)
            {
                unsafe
                {
                    byte[] input = Encoding.UTF8.GetBytes(this.input);
                    BufferStream bs = new BufferStream(input);
                    bs.ExecuteCode(bw, code);
                    string output = Encoding.UTF8.GetString(bs.GetOutput());
                    textBox1.AppendText("Output: " + output);
                    bs.ResetCompiler();
                }
            }
            else if (bs == BuildState.BuildRun)
            {
                string path = GetPath();
                msg = "Build succeed.";

                if (path == string.Empty)
                {
                    MessageBox.Show("You have not set up the compiler path!", "Brainfuck", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    msg = "Build unsuccessful!";
                    bw.CancelAsync();
                }

                string script = this.path.Replace(".bf", ".d");

                string obj = script.Replace(".d", ".obj");
                string app = script.Replace(".d", ".exe");

                string newCode = BufferStream.TranslateCode(code);
                File.WriteAllText(script, newCode);

                ProcessStartInfo ps = new ProcessStartInfo()
                {
                    FileName = path,
                    Arguments = string.Format("-O {0} -of={1}", script, app),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                textBox1.AppendText(string.Format("Build... {0}", app));

                using (Process proc = Process.Start(ps))
                    proc.WaitForExit();

                string output = "";
                textBox1.AppendText(Environment.NewLine);

                ps = new ProcessStartInfo()
                {
                    FileName = app,
                    Arguments = input,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                };

                using (var proc = Process.Start(ps))
                {
                    output = proc.StandardOutput.ReadToEnd();
                    textBox1.AppendText("Output: " + output);
                }

                File.Delete(script);
                File.Delete(obj);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            string[] files = Environment.GetCommandLineArgs();
            if (files.Length == 1 || files.Length > 2) return;

            TabPage tab = new TabPage(NameOf(files[1]));
            TextBox box = new TextBox();
            box.ScrollBars = ScrollBars.Both;
            box.Font = new Font("Microsoft Sans Serif", 11);
            box.Multiline = true;
            box.Size = new Size(419, 179);
            box.Text = File.ReadAllText(files[1]);
            tab.Controls.Add(box);
            tabs.TabPages.Add(tab);
            tabs.SelectedTab = tab;
            tfp.Add(files[1]);
            tbn.Add(tab.Text);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = string.Format("File{0}.bf", ID++);
            TabPage tab = new TabPage(name);
            TextBox box = new TextBox();
            box.KeyUp += AutoComplete;
            box.ScrollBars = ScrollBars.Both;
            box.Font = new Font("Microsoft Sans Serif", 11);
            box.Multiline = true;
            box.Size = new Size(419, 179);
            tab.Controls.Add(box);
            tabs.TabPages.Add(tab);
            tabs.SelectedTab = tab;
        }

        private void AutoComplete(object sender, KeyEventArgs e)
        {
            if(e.KeyValue == 219)
            {
                TextBox box = sender as TextBox;
                string[] lines = box.Lines;
                int i = lines.Length - 1;
                int j = lines[i].LastIndexOf('[');
                lines[i] += "]";
                box.Lines = lines;
                box.SelectionStart = lines.Length * i + j + 1;
                box.SelectionLength = 0;
            }
        }

        private string NameOf(string path)
        {
            string[] all = path.Split('\\');
            return all[all.Length - 1];
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Title = "Open";
            opf.DefaultExt = "*.bf";
            opf.Filter = "Brainfuck files(*.bf)|*.bf";

            if(opf.ShowDialog() == DialogResult.OK)
            {
                TabPage tab = new TabPage(NameOf(opf.FileName));
                TextBox box = new TextBox();
                box.ScrollBars = ScrollBars.Both;
                box.Font = new Font("Microsoft Sans Serif", 11);
                box.Multiline = true;
                box.Size = new Size(419, 179);
                box.Text = File.ReadAllText(opf.FileName);
                tab.Controls.Add(box);
                tabs.TabPages.Add(tab);
                tabs.SelectedTab = tab;
                tfp.Add(opf.FileName);
                tbn.Add(tab.Text);
                path = opf.FileName;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabs.TabCount < 2) return;
            TabPage tab = tabs.SelectedTab;
            string content = ((TextBox)tabs.SelectedTab.Controls[0]).Text;

            if (!tbn.Contains(tab.Text))
            {
                // Because It does not exists in the list.
                SaveFileDialog svf = new SaveFileDialog();
                svf.Title = "Save as";
                svf.DefaultExt = "*.bf";
                svf.Filter = "Brainfuck files(*.bf)|*.bf";
                svf.FileName = tab.Text;

                if (svf.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(svf.FileName, content);
                    tfp.Add(svf.FileName);
                    tab.Text = NameOf(svf.FileName);
                    path = svf.FileName;
                    tbn.Add(tab.Text);
                }
            }
            else
            {
                // Using binary search find the index.
                int ink = tbn.BinarySearch(tab.Text);
                // Update the source code of file.
                File.WriteAllText(tfp[ink], content);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabs.TabCount < 2) return;
            SaveFileDialog svf = new SaveFileDialog();
            svf.Title = "Save as";
            svf.DefaultExt = "*.bf";
            svf.Filter = "Brainfuck files(*.bf)|*.bf";

            if(svf.ShowDialog() == DialogResult.OK)
            {
                string content = ((TextBox)tabs.SelectedTab.Controls[0]).Text;
                File.WriteAllText(svf.FileName, content);
                TabPage tab = tabs.SelectedTab;
                tab.Text = NameOf(svf.FileName);
                path = svf.FileName;
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabs.SelectedTab.Text != "Start Page")
                tabs.TabPages.Remove(tabs.SelectedTab);
        }

        private int Compile(string code)
        {
            List<int> bl = new List<int>();
            List<int> el = new List<int>();

            for (int k = 0; k < code.Length; k++)
            {
                if (code[k] == '[')
                    bl.Add(k);
                else
                if (code[k] == ']')
                {
                    el.Add(k);

                    if (bl.Count != 0)
                        bl.Remove(bl[bl.Count - 1]);
                    else return k;
                }
            }

            if (bl.Count != 0)
                return bl[0];

            return 0;
        }

        private void buildToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabs.TabCount > 1)
            {
                code = ((TextBox)tabs.SelectedTab.Controls[0]).Text;
                int error = Compile(code);
                textBox1.Clear();

                textBox1.AppendText(string.Format("Compiling {0}...", tabs.SelectedTab.Text));
                textBox1.AppendText(Environment.NewLine);

                if (error != 0)
                {
                    char bracket = code[error];
                    if (bracket == '[') bracket = ']';
                    else bracket = '[';
                    textBox1.AppendText(string.Format("Error({0}): Bracket {1} are expected.", error, bracket));
                    textBox1.AppendText(Environment.NewLine);
                }
                else
                {
                    textBox1.AppendText("The program was compiled successfully.");
                    textBox1.AppendText(Environment.NewLine);
                    textBox1.AppendText("Input: ");
                    textBox1.ReadOnly = false;
                    bs = BuildState.Interpret;
                }
            }
            else MessageBox.Show("Please select brainfuck program first.", "Brainfuck", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help help = new Help();
            help.ShowDialog();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private void buildToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // Build and export the executable.
            if(tabs.TabCount > 1)
            {
                code = ((TextBox)tabs.SelectedTab.Controls[0]).Text;
                int error = Compile(code);
                textBox1.Clear();

                textBox1.AppendText(string.Format("Compiling {0}...", tabs.SelectedTab.Text));
                textBox1.AppendText(Environment.NewLine);

                if (error != 0)
                {
                    char bracket = code[error];
                    if (bracket == '[') bracket = ']';
                    else bracket = '[';
                    textBox1.AppendText(string.Format("Error({0}): Bracket {1} are expected.", error, bracket));
                    textBox1.AppendText(Environment.NewLine);
                }
                else
                {
                    textBox1.AppendText("The program was compiled successfully.");
                    textBox1.AppendText(Environment.NewLine);
                    textBox1.AppendText("Input: ");
                    textBox1.ReadOnly = false;
                    bs = BuildState.BuildRun;
                }
            }
            else MessageBox.Show("Selects brainfuck program to build and run it.", "Brainfuck", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void stopToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // It is stoping the running of the program.
            if (!bw.CancellationPending)
            {
                bw.CancelAsync();
                bs = BuildState.Disabled;
            }
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Here I will set up the dlang compiler directory needed to build executable program.
            // Open window to set up the path.
            Setup setup = new Setup();
            setup.ShowDialog();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (bs == BuildState.Interpret)
            {
                if (e.KeyCode == Keys.Return)
                {
                    int index = textBox1.Lines.Length - 1;
                    string str = textBox1.Lines[index];
                    input = str.Split(' ')[1];
                    textBox1.AppendText(Environment.NewLine);
                    textBox1.AppendText("Running...");
                    textBox1.AppendText(Environment.NewLine);
                    textBox1.ReadOnly = true;
                    bw.RunWorkerAsync();
                }
            }
            else if(bs == BuildState.BuildRun)
            {
                if (e.KeyCode == Keys.Return)
                {
                    int index = textBox1.Lines.Length - 1;
                    string str = textBox1.Lines[index];
                    input = str.Split(' ')[1];
                    textBox1.AppendText(Environment.NewLine);
                    textBox1.AppendText("Running...");
                    textBox1.AppendText(Environment.NewLine);
                    textBox1.ReadOnly = true;
                    bw.RunWorkerAsync();
                }
            }
        }
    }
}
