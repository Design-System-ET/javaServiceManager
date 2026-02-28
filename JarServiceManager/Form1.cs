using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JarServiceManager
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer timerRefresh = new System.Windows.Forms.Timer();

        public Form1()
        {
            InitializeComponent();

            timerRefresh.Interval = 5000;
            timerRefresh.Tick += (s, e) => CargarServicios();
            timerRefresh.Start();

            CargarServicios();
        }

        private void CargarServicios()
        {
            dgvServicios.Rows.Clear();
            string filtro = txtFiltro.Text?.ToUpper() ?? "";

            var servicios = System.ServiceProcess.ServiceController.GetServices()
                .Where(s => s.ServiceName.StartsWith("JSM-JAR"))
                .Where(s => s.ServiceName.ToUpper().Contains(filtro));

            foreach (var s in servicios)
            {
                string statusText = GetServiceStatus(s.ServiceName);
                string uptime = GetServiceUptime(s.ServiceName);

                int row = dgvServicios.Rows.Add(s.ServiceName, statusText, uptime);
                dgvServicios.Rows[row].DefaultCellStyle.BackColor =
                    statusText == "Running" ? System.Drawing.Color.LightGreen : System.Drawing.Color.LightCoral;
            }
        }

        private string GetServiceStatus(string serviceName)
        {
            try
            {
                string nssmPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nssm", "nssm.exe");
                var psi = new ProcessStartInfo
                {
                    FileName = nssmPath,
                    Arguments = $"status \"{serviceName}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proc = Process.Start(psi);
                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit();

                if (output.Contains("SERVICE_RUNNING")) return "Running";
                return "Stopped";
            }
            catch
            {
                return "Stopped";
            }
        }

        private string GetServiceUptime(string serviceName)
        {
            try
            {
                string jarName = serviceName.Substring(8) + ".jar"; // corregido
                var procesos = Process.GetProcessesByName("java")
                                .Where(p =>
                                {
                                    try
                                    {
                                        string cmd = GetCommandLine(p);
                                        return cmd.Contains(jarName, StringComparison.OrdinalIgnoreCase);
                                    }
                                    catch { return false; }
                                }).ToList();

                if (!procesos.Any()) return "-";

                var proc = procesos.First();
                TimeSpan up = DateTime.Now - proc.StartTime;
                return $"{(int)up.TotalHours}h {up.Minutes}m";
            }
            catch
            {
                return "-";
            }
        }

        private string GetCommandLine(Process process)
        {
            try
            {
                using var searcher = new System.Management.ManagementObjectSearcher(
                    $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}");
                var moc = searcher.Get().Cast<System.Management.ManagementBaseObject>().FirstOrDefault();
                return moc?["CommandLine"]?.ToString() ?? "";
            }
            catch
            {
                return "";
            }
        }

        private void txtFiltro_TextChanged(object sender, EventArgs e) => CargarServicios();

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Java JAR files (*.jar)|*.jar";
            ofd.Title = "Selecciona un archivo JAR";

            if (ofd.ShowDialog() == DialogResult.OK)
                txtJarPath.Text = ofd.FileName;
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            string jarPath = txtJarPath.Text.Trim();
            if (!File.Exists(jarPath))
            {
                MessageBox.Show("Archivo .jar no encontrado.");
                return;
            }

            string serviceName = "JSM-JAR-" + Path.GetFileNameWithoutExtension(jarPath);
            string nssmPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nssm", "nssm.exe");

            string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                        "JarServiceManager", "Logs", serviceName);
            Directory.CreateDirectory(logDir);

            ProcessStart(nssmPath, $"install \"{serviceName}\" \"java.exe\" \"-jar \\\"{jarPath}\\\"\"");
            ProcessStart(nssmPath, $"set \"{serviceName}\" AppStdout \"{Path.Combine(logDir, "stdout.log")}\"");
            ProcessStart(nssmPath, $"set \"{serviceName}\" AppStderr \"{Path.Combine(logDir, "stderr.log")}\"");
            ProcessStart(nssmPath, $"set \"{serviceName}\" AppStdoutCreationDisposition 4");
            ProcessStart(nssmPath, $"set \"{serviceName}\" AppRestartDelay 5000");
            ProcessStart(nssmPath, $"set \"{serviceName}\" Start SERVICE_AUTO_START");

            ProcessStart(nssmPath, $"start \"{serviceName}\"");

            CargarServicios();
            MessageBox.Show($"Servicio {serviceName} instalado e iniciado automáticamente con logs y auto-restart.");

            txtJarPath.ResetText();


        }

        private void btnUninstall_Click(object sender, EventArgs e)
        {
            DialogResult resultado = MessageBox.Show("¿Seguro quiere eliminar el Servicio?", "Confirmar", MessageBoxButtons.YesNo);
            if (resultado == DialogResult.Yes)
            {
                if (dgvServicios.CurrentRow == null) return;

                string serviceName = dgvServicios.CurrentRow.Cells[0].Value.ToString();
                string nssmPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nssm", "nssm.exe");

                ProcessStart(nssmPath, $"stop \"{serviceName}\"");
                ProcessStart(nssmPath, $"remove \"{serviceName}\" confirm");

                CargarServicios();
                MessageBox.Show($"Servicio {serviceName} detenido y desinstalado.");
            } 
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (dgvServicios.CurrentRow == null) return;

            string serviceName = dgvServicios.CurrentRow.Cells[0].Value.ToString();
            string nssmPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nssm", "nssm.exe");

            ProcessStart(nssmPath, $"start \"{serviceName}\"");
            CargarServicios();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (dgvServicios.CurrentRow == null) return;

            string serviceName = dgvServicios.CurrentRow.Cells[0].Value.ToString();
            string nssmPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nssm", "nssm.exe");

            ProcessStart(nssmPath, $"stop \"{serviceName}\"");
            CargarServicios();
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            if (dgvServicios.CurrentRow == null) return;

            string serviceName = dgvServicios.CurrentRow.Cells[0].Value.ToString();
            string nssmPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nssm", "nssm.exe");

            ProcessStart(nssmPath, $"restart \"{serviceName}\"");
            CargarServicios();
        }

        private void btnViewLog_Click(object sender, EventArgs e)
        {
            if (dgvServicios.CurrentRow == null) return;

            string serviceName = dgvServicios.CurrentRow.Cells[0].Value.ToString();
            string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                         "JarServiceManager", "Logs", serviceName);

            if (Directory.Exists(logDir))
                Process.Start("explorer.exe", logDir);
            else
                MessageBox.Show("No hay logs disponibles para este servicio.");
        }

        private void btnRefresh_Click(object sender, EventArgs e) => CargarServicios();

        private void ProcessStart(string fileName, string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = true,
                Verb = "runas"
            };
            var proc = Process.Start(psi);
            proc.WaitForExit();
        }

        private void newJARToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Java JAR files (*.jar)|*.jar";
            ofd.Title = "Selecciona un archivo JAR";

            if (ofd.ShowDialog() == DialogResult.OK)
                txtJarPath.Text = ofd.FileName;
        }


        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            txtFiltro.ResetText();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DialogResult resultado = MessageBox.Show("¿Seguro que querés cerrar?", "Confirmar", MessageBoxButtons.YesNo);
            if (resultado == DialogResult.Yes)
            {
                this.Close(); // Cancela el cierre
            }
        }

        private void minimizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}