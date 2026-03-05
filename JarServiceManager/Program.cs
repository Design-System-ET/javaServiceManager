using System;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace JarServiceManager
{
    static class Program
    {
        // Mutex para asegurarse de que solo haya una instancia de JSM corriendo
        static Mutex mutex = new Mutex(true, "JSM_SINGLE_INSTANCE");

        // Permite traer la ventana existente al frente
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [STAThread]
        static void Main(string[] args)  // <-- recibir argumentos
        {
            // Revisar si ya hay una instancia corriendo
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                // Buscar la ventana principal existente y traerla al frente
                foreach (var p in Process.GetProcessesByName("JarServiceManager"))
                {
                    if (p.MainWindowHandle != IntPtr.Zero)
                    {
                        SetForegroundWindow(p.MainWindowHandle);
                        break;
                    }
                }
                return; // salir de la nueva instancia
            }

            // Esto soluciona los elementos ofuscados y la DPI alta
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

            // Ejecutar la aplicación
            Application.Run(mainForm);
        }
    }
}