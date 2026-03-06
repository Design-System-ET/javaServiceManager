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
            bool createdNew;
            using (Mutex mutex = new Mutex(true, MutexName, out createdNew))
            {
                if (!createdNew)
                {
                    // Ya hay una instancia: guardamos argumentos si vienen y enviamos señal para restaurarla
                    if (args.Length > 1 && args[0].Equals("install", StringComparison.OrdinalIgnoreCase))
                    {
                        // Guardamos el path del .jar en un archivo temporal
                        File.WriteAllText(ArgsFile, args[1]);
                    }

                    // Señalamos a la instancia existente que se traiga al frente
                    try
                    {
                        EventWaitHandle.OpenExisting(EventName).Set();
                    }
                    catch { }

                    return; // salir de la nueva instancia
                }

                // Creamos EventWaitHandle para recibir señales de nuevas instancias
                EventWaitHandle eventHandle = new EventWaitHandle(false, EventResetMode.AutoReset, EventName);

                // Soluciona problema de elementos ofuscados
                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var mainForm = new Form1();

                // Si se pasó un .jar como argumento desde el menú contextual en esta primera instancia
                if (args.Length > 1 && args[0].Equals("install", StringComparison.OrdinalIgnoreCase)
                    && File.Exists(args[1])
                    && Path.GetExtension(args[1]).Equals(".jar", StringComparison.OrdinalIgnoreCase))
                {
                    mainForm.SetJarPathAndInstall(args[1]); // llama automáticamente a btnInstall_Click
                }

                // Hilo que espera señales de nuevas instancias
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    while (true)
                    {
                        eventHandle.WaitOne();

                        // Restaurar la ventana desde bandeja
                        if (mainForm.InvokeRequired)
                            mainForm.Invoke(new Action(mainForm.ShowFromTray));
                        else
                            mainForm.ShowFromTray();

                        // Revisar si hay un .jar enviado desde la segunda instancia
                        if (File.Exists(ArgsFile))
                        {
                            string jarPath = File.ReadAllText(ArgsFile);
                            if (File.Exists(jarPath) && Path.GetExtension(jarPath).Equals(".jar", StringComparison.OrdinalIgnoreCase))
                            {
                                // Ejecutar instalación en el hilo del formulario
                                mainForm.Invoke(() => mainForm.SetJarPathAndInstall(jarPath));
                            }

                            // Borrar archivo temporal para no procesarlo otra vez
                            File.Delete(ArgsFile);
                        }
                    }
                });

                Application.Run(mainForm);
            }
        }
    }
}