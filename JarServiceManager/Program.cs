using System;
using System.Threading;
using System.Windows.Forms;

namespace JarServiceManager
{
    static class Program
    {
        private const string MutexName = "JarServiceManagerSingletonMutex";
        private const string EventName = "JarServiceManagerBringToFrontEvent";

        [STAThread]
        static void Main(string[] args)
        {
            bool createdNew;
            using (Mutex mutex = new Mutex(true, MutexName, out createdNew))
            {
                if (!createdNew)
                {
                    // Ya hay una instancia: enviamos señal para restaurarla
                    try
                    {
                        EventWaitHandle.OpenExisting(EventName).Set();
                    }
                    catch { }
                    return; // salir de la nueva instancia
                }

                // Creamos EventWaitHandle para recibir señales de nuevas instancias
                EventWaitHandle eventHandle = new EventWaitHandle(false, EventResetMode.AutoReset, EventName);


                //soluciona problema de elementos ofuscados
                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var mainForm = new Form1();

                // Si se pasó un .jar como argumento desde el menú contextual
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
                    }
                });

                Application.Run(mainForm);
            }
        }
    }
}