using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace JarServiceManager
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer timerRefresh = new System.Windows.Forms.Timer();

        //icono para barra de tareas
        NotifyIcon trayIcon;

        // direcciones de logs
        private string rutaStdout = @"C:\logs\stdout.log";
        private string rutaStderr = @"C:\logs\stderr.log";

        public Form1()
        {
            InitializeComponent();

            //cuando la ventana cambie de tamano se oculta
            this.Resize += Form1_Resize;

            //constructor del icono de notificacion
            trayIcon = new NotifyIcon();
            trayIcon.Icon = this.Icon;
            trayIcon.Text = "Jar Service Manager";
            trayIcon.Visible = true;

            ContextMenuStrip menu = new ContextMenuStrip();

            menu.Items.Add("Abrir", null, (s, e) =>
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            });

            menu.Items.Add("Salir", null, (s, e) =>
            {
                trayIcon.Visible = false;
                Application.Exit();
            });

            trayIcon.ContextMenuStrip = menu;



            //aplico estilos
            Style.ApplyFormStyle(this);
            Style.ApplyGroupBoxStyle(groupBox1);
            Style.ApplyGroupBoxStyle(groupBox2);
            Style.ApplyGroupBoxStyle(groupBox3);


            Style.ApplyButtonPrimary(btnBrowse);
            Style.ApplyButtonRadius(btnBrowse, 1);

            Style.ApplyButtonPrimary(btnInstall);
            Style.ApplyButtonRadius(btnInstall, 1);

            Style.ApplyButtonPrimary(btnStart);
            Style.ApplyButtonRadius(btnStart, 1);


            Style.ApplyButtonSecondary(btnStop);
            Style.ApplyButtonSecondary(btnRefresh);
            Style.ApplyButtonSecondary(btnRestart);
            Style.ApplyButtonSecondary(btnUninstall);
            Style.ApplyButtonSecondary(btnViewLog);

            Style.SetPictureBoxOpacity(pictureBox1, 0.5f);


            // Obtener la ruta de la carpeta Documentos
            string documentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Ruta de la carpeta que queremos crear
            string rutaJars = Path.Combine(documentos, "JARs - Jar Server Manager");

            // Crear la carpeta si no existe
            if (!Directory.Exists(rutaJars))
            {
                Directory.CreateDirectory(rutaJars);
            }


            timerRefresh.Interval = 5000;
            timerRefresh.Tick += (s, e) => CargarServicios();
            timerRefresh.Start();

            CargarServicios();
        }

        // Mostrar la ventana aunque esté minimizada a bandeja
        public void ShowFromTray()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Activate();
        }


        //oculta la ventana al minimizar
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                ShowInTaskbar = false;
            }
        }


        //recibir parametros de menu contextual
        public void SetJarPathAndInstall(string jarPath)
        {
            txtJarPath.Text = jarPath;
            btnInstall_Click(this, EventArgs.Empty);
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
            // Ejemplo: tomamos el nombre del servicio seleccionado en un DataGridView
            string serviceName = dgvServicios.CurrentRow.Cells["dataGridViewTextBoxColumn1"].Value.ToString();

            try
            {
                string jarName = serviceName.Substring(8) + ".jar";

                var proceso = Process.GetProcessesByName("java")
                    .FirstOrDefault(p =>
                    {
                        try
                        {
                            using var searcher = new System.Management.ManagementObjectSearcher(
                                $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {p.Id}");
                            var moc = searcher.Get().Cast<System.Management.ManagementBaseObject>().FirstOrDefault();
                            return moc?["CommandLine"]?.ToString().Contains(jarName, StringComparison.OrdinalIgnoreCase) ?? false;
                        }
                        catch { return false; }
                    });

                if (proceso == null)
                {
                    MessageBox.Show("El servicio no se está ejecutando.", "Estado del servicio", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using var cpuCounter = new PerformanceCounter("Process", "% Processor Time", proceso.ProcessName, true);
                using var memCounter = new PerformanceCounter("Process", "Working Set - Private", proceso.ProcessName, true);

                float cpu = cpuCounter.NextValue() / Environment.ProcessorCount;
                float mem = memCounter.NextValue() / 1024 / 1024; // MB

                TimeSpan uptime = DateTime.Now - proceso.StartTime;

                string msg = $"Estado: Running\nCPU: {cpu:F1}%\nMemoria: {mem:F1} MB\nTiempo de ejecución: {uptime:hh\\:mm\\:ss}";

                MessageBox.Show(msg, $"Estado del servicio - {serviceName}", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener estado del servicio:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            txtFiltro.ResetText();
        }


        private void minimizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dgvServicios.CurrentRow == null) return;
            string serviceName = dgvServicios.CurrentRow.Cells[0].Value.ToString();
            FormGraph fg = new FormGraph(serviceName);
            fg.Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About frm = new About();
            frm.ShowDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult resultado = MessageBox.Show("¿Seguro que querés cerrar?", "Confirmar", MessageBoxButtons.YesNo);
            if (resultado == DialogResult.Yes)
            {
                this.Close(); // Cancela el cierre
            }
        }

        private void technicalSupportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string numero = "59891388175"; // sin + ni espacios
            string url = $"https://wa.me/{numero}";

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private void developerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Developer dev = new Developer();
            dev.ShowDialog();
        }

        private void stderrToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string serviceName = dgvServicios.CurrentRow.Cells[0].Value.ToString();
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                          "JarServiceManager", "Logs", serviceName, "stderr.log");
            AbrirLogCMD(logPath);
        }

        private void stdoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string serviceName = dgvServicios.CurrentRow.Cells[0].Value.ToString();
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                                          "JarServiceManager", "Logs", serviceName, "stdout.log");
            AbrirLogCMD(logPath);
        }


        //muestra logs por cmd con tail -f
        private void AbrirLogCMD(string logPath)
        {
            if (!File.Exists(logPath))
            {
                MessageBox.Show("No existe el log: " + logPath);
                return;
            }

            // PowerShell tail -f
            string comandoPowerShell = $"powershell -Command \"Get-Content -Path '{logPath}' -Wait -Tail 50\"";

            // Abrir cmd con fondo blanco y texto negro
            string argumentos = $"/K color F0 & {comandoPowerShell}";

            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = argumentos,
                UseShellExecute = true
            };

            System.Diagnostics.Process.Start(psi);
        }

        private void openFolderJARToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string documentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string rutaJars = Path.Combine(documentos, "JARs - Jar Server Manager");

            // Asegurarse de que la carpeta exista
            if (!Directory.Exists(rutaJars))
                Directory.CreateDirectory(rutaJars);

            // Abrir la carpeta
            System.Diagnostics.Process.Start("explorer.exe", rutaJars);
        }
    }
}