using System;
using System.Reflection;
using System.Windows.Forms;

namespace JarServiceManager
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            this.Load += About_Load; // Suscribimos el evento Load
        }

        private void About_Load(object sender, EventArgs e)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            label8.Text = $"Versión: {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
}
