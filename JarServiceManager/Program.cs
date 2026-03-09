using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace JarServiceManager
{
    static class Program
    {
        private const string MutexName = "JarServiceManagerSingletonMutex";
        private const string EventName = "JarServiceManagerBringToFrontEvent";
        private static readonly string ArgsFile = Path.Combine(Path.GetTempPath(), "JarServiceManagerArgs.txt");

        [STAThread]
        static void Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // --- Control de instancia única ---
            bool createdNew;
            using (Mutex mutex = new Mutex(true, MutexName, out createdNew))
            {
                if (!createdNew)
                {
                    // Ya hay otra instancia: guardar args si existen y traer al frente
                    if (args.Length > 1 && args[0].Equals("install", StringComparison.OrdinalIgnoreCase))
                        File.WriteAllText(ArgsFile, args[1]);

                    try { EventWaitHandle.OpenExisting(EventName).Set(); } catch { }

                    return; // salir de la nueva instancia
                }

                // --- Mostrar Splash solo si es la primera instancia ---
                using (SplashForm splash = new SplashForm())
                {
                    splash.ShowDialog(); // bloquea 3 segundos y luego se cierra
                }

                // Crear EventWaitHandle para nuevas instancias
                EventWaitHandle eventHandle = new EventWaitHandle(false, EventResetMode.AutoReset, EventName);

                var mainForm = new Form1();

                // Procesar argumento de instalación si se pasó
                if (args.Length > 1 && args[0].Equals("install", StringComparison.OrdinalIgnoreCase)
                    && File.Exists(args[1])
                    && Path.GetExtension(args[1]).Equals(".jar", StringComparison.OrdinalIgnoreCase))
                {
                    mainForm.SetJarPathAndInstall(args[1]);
                }

                // Hilo para manejar nuevas instancias y args
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    while (true)
                    {
                        eventHandle.WaitOne();

                        if (mainForm.InvokeRequired)
                            mainForm.Invoke(new Action(mainForm.ShowFromTray));
                        else
                            mainForm.ShowFromTray();

                        if (File.Exists(ArgsFile))
                        {
                            string jarPath = File.ReadAllText(ArgsFile);
                            if (File.Exists(jarPath) && Path.GetExtension(jarPath).Equals(".jar", StringComparison.OrdinalIgnoreCase))
                                mainForm.Invoke(() => mainForm.SetJarPathAndInstall(jarPath));

                            File.Delete(ArgsFile);
                        }
                    }
                });

                // Ejecutar loop principal con Form1
                Application.Run(mainForm);
            }
        }
    }
}