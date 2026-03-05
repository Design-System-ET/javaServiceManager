using System;
using System.IO;
using System.Windows.Forms;

namespace JarServiceManager
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)  // <-- recibir argumentos
        {
            //esto soluciona los elementos ofuscados
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm = new Form1();

            // Si se pasó un .jar como argumento
            if (args.Length > 1 && args[0].Equals("install", StringComparison.OrdinalIgnoreCase)
                && File.Exists(args[1])
                && Path.GetExtension(args[1]).Equals(".jar", StringComparison.OrdinalIgnoreCase))
            {
                mainForm.SetJarPathAndInstall(args[1]); // llama automáticamente a btnInstall_Click
            }

            Application.Run(mainForm);
        }
    }
}