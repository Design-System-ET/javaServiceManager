using System;
using System.Drawing;
using System.Windows.Forms;

namespace JarServiceManager
{
    public partial class SplashForm : Form
    {
        private System.Windows.Forms.Timer timer;
        private Image splashImage;

        public SplashForm()
        {
            // Cargar tu PNG desde recursos
            splashImage = Properties.Resources.pajaro_2; // debe tener transparencia

            // Configuración del Form
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Width = splashImage.Width;
            this.Height = splashImage.Height;
            this.TopMost = true;
            this.ShowInTaskbar = false;

            // Timer para cerrar automáticamente
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 5000; // 5 segundos
            timer.Tick += Timer_Tick;
            timer.Start();

            // Hacer que el fondo sea transparente
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Black;
            this.TransparencyKey = Color.Black;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            this.Close();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawImage(splashImage, 0, 0, splashImage.Width, splashImage.Height);
        }
    }
}