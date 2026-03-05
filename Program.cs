using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;

namespace HashChecker
{
    public class SetupForm : Form
    {
        public SetupForm()
        {
            this.Text = "Context Menu Hash Checker - Setup";
            this.Size = new Size(420, 240);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            // --- TIKLANABİLİR HEADER ALANI ---
            EventHandler openUrl = (s, e) => Process.Start(new ProcessStartInfo("https://www.osmanonurkoc.com") { UseShellExecute = true });

            try
            {
                Icon appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                this.Icon = appIcon;

                PictureBox pbIcon = new PictureBox()
                {
                    Image = appIcon.ToBitmap(),
                    Location = new Point(20, 15),
                    Size = new Size(48, 48),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Cursor = Cursors.Hand
                };
                pbIcon.Click += openUrl;
                this.Controls.Add(pbIcon);
            }
            catch { }

            Label lblTitle = new Label()
            {
                Text = "Context Menu Hash Checker",
                Location = new Point(75, 20),
                AutoSize = true,
                Font = new Font(this.Font, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            lblTitle.Click += openUrl;

            Label lblSubtitle = new Label()
            {
                Text = "@osmanonurkoc",
                Location = new Point(77, 40),
                AutoSize = true,
                ForeColor = Color.Gray,
                Cursor = Cursors.Hand
            };
            lblSubtitle.Click += openUrl;

            // --- İÇERİK ALANI ---
            Label lblDesc = new Label()
            {
                Text = "This tool adds hash verification options to the right-click menu.\nYou can use the buttons below for installation.",
                Location = new Point(20, 85),
                AutoSize = true
            };

            Button btnInstall = new Button()
            {
                Text = "Install to Context Menu",
                Location = new Point(20, 140),
                Width = 175,
                Height = 35
            };
            btnInstall.Click += BtnInstall_Click;

            Button btnUninstall = new Button()
            {
                Text = "Remove from Context Menu",
                Location = new Point(205, 140),
                Width = 175,
                Height = 35
            };
            btnUninstall.Click += BtnUninstall_Click;

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblSubtitle);
            this.Controls.Add(lblDesc);
            this.Controls.Add(btnInstall);
            this.Controls.Add(btnUninstall);
        }

        private void BtnInstall_Click(object sender, EventArgs e)
        {
            try
            {
                string exePath = Application.ExecutablePath;
                string basePath = @"Software\Classes\*\shell\Checksum";

                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(basePath))
                {
                    key.SetValue("MUIVerb", "Checksum");
                    key.SetValue("SubCommands", "");
                    key.SetValue("Icon", $"\"{exePath}\",0");
                }

                string[] algos = { "MD5", "SHA1", "SHA256", "SHA384", "SHA512" };
                for (int i = 0; i < algos.Length; i++)
                {
                    string algo = algos[i];
                    using (RegistryKey subKey = Registry.CurrentUser.CreateSubKey($@"{basePath}\shell\0{i + 1}{algo}"))
                    {
                        subKey.SetValue("MUIVerb", algo);
                        using (RegistryKey cmdKey = subKey.CreateSubKey("command"))
                        {
                            cmdKey.SetValue("", $"\"{exePath}\" \"%1\" {algo.ToLower()}");
                        }
                    }
                }
                MessageBox.Show("Successfully added to the context menu!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUninstall_Click(object sender, EventArgs e)
        {
            try
            {
                Registry.CurrentUser.DeleteSubKeyTree(@"Software\Classes\*\shell\Checksum", false);
                MessageBox.Show("Successfully removed from the context menu.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class HashForm : Form
    {
        private TextBox txtCalculated;
        private TextBox txtCompare;
        private Label lblStatus;
        private Button btnCopy;
        private ProgressBar progressBar;
        private string calculatedHash = "";
        private string targetFile = "";
        private string algorithm = "";

        public HashForm(string file, string algo)
        {
            targetFile = file;
            algorithm = algo.ToUpper();

            this.Text = $"{algorithm} Checksum Verifier";
            this.Size = new Size(500, 310);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Load += HashForm_Load;

            try { this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            Label lblCalc = new Label() { Text = $"Calculated {algorithm}:", Location = new Point(15, 15), AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };

            txtCalculated = new TextBox() { Text = "Calculating...", Location = new Point(15, 40), Width = 370, ReadOnly = true };

            btnCopy = new Button() { Text = "Copy", Location = new Point(395, 38), Width = 75, Enabled = false };
            btnCopy.Click += (s, e) => { if(!string.IsNullOrEmpty(calculatedHash)) Clipboard.SetText(calculatedHash); };

            progressBar = new ProgressBar() { Location = new Point(15, 75), Width = 455, Height = 20, Style = ProgressBarStyle.Marquee };

            Label lblComp = new Label() { Text = "Paste Target Hash Here:", Location = new Point(15, 110), AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };

            txtCompare = new TextBox() { Location = new Point(15, 135), Width = 455 };
            txtCompare.TextChanged += TxtCompare_TextChanged;

            lblStatus = new Label() { Text = "Waiting for hash calculation...", Location = new Point(15, 175), Width = 455, TextAlign = ContentAlignment.MiddleCenter };

            Button btnOk = new Button() { Text = "OK", Location = new Point(205, 215), Width = 80 };
            btnOk.Click += (s, e) => { this.Close(); };

            this.Controls.Add(lblCalc);
            this.Controls.Add(txtCalculated);
            this.Controls.Add(btnCopy);
            this.Controls.Add(progressBar);
            this.Controls.Add(lblComp);
            this.Controls.Add(txtCompare);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnOk);
        }

        private async void HashForm_Load(object sender, EventArgs e)
        {
            try
            {
                calculatedHash = await Task.Run(() => CalculateHashOptimized(targetFile, algorithm));

                txtCalculated.Text = calculatedHash;
                btnCopy.Enabled = true;

                progressBar.Style = ProgressBarStyle.Blocks;
                progressBar.Value = 100;

                if (string.IsNullOrEmpty(txtCompare.Text))
                {
                    lblStatus.Text = "Waiting for comparison...";
                }
                else
                {
                    TxtCompare_TextChanged(null, null);
                }
            }
            catch (Exception ex)
            {
                txtCalculated.Text = "Error: " + ex.Message;
                progressBar.Style = ProgressBarStyle.Blocks;
                lblStatus.Text = "Calculation failed!";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private void TxtCompare_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(calculatedHash)) return;

            string target = txtCompare.Text.Trim().ToLower();
            string calc = calculatedHash.ToLower();

            if (string.IsNullOrEmpty(target))
            {
                lblStatus.Text = "Waiting for comparison...";
                lblStatus.ForeColor = Color.Black;
            }
            else if (target == calc)
            {
                lblStatus.Text = "✓ Hash Matches!";
                lblStatus.ForeColor = Color.Green;
            }
            else
            {
                lblStatus.Text = "✗ Hash Mismatch!";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private string CalculateHashOptimized(string filename, string algo)
        {
            HashAlgorithm hashAlgo = null;
            switch (algo)
            {
                case "MD5": hashAlgo = MD5.Create(); break;
                case "SHA1": hashAlgo = SHA1.Create(); break;
                case "SHA256": hashAlgo = SHA256.Create(); break;
                case "SHA384": hashAlgo = SHA384.Create(); break;
                case "SHA512": hashAlgo = SHA512.Create(); break;
                default: throw new Exception("Unsupported Algorithm");
            }

            int bufferSize = 1024 * 1024;
            using (FileStream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan))
            {
                byte[] hash = hashAlgo.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }

    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length == 0)
            {
                Application.Run(new SetupForm());
            }
            else if (args.Length >= 2)
            {
                Application.Run(new HashForm(args[0], args[1]));
            }
            else
            {
                MessageBox.Show("Missing parameters!\nDouble-click the program for normal use.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
