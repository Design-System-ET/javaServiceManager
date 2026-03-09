using System.Drawing;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;


namespace JarServiceManager
{
    public static class Style
    {
        // Colores generales
        public static readonly Color BackgroundMain = ColorTranslator.FromHtml("#F4F6F8");
        public static readonly Color PanelBackground = ColorTranslator.FromHtml("#FFFFFF");
        public static readonly Color BorderColor = ColorTranslator.FromHtml("#E5E7EB");

        // Colores botones
        public static readonly Color ButtonPrimary = ColorTranslator.FromHtml("#018ee6"); 
        public static readonly Color ButtonPrimaryHover = ColorTranslator.FromHtml("#014897");
        public static readonly Color ButtonDisabled = ColorTranslator.FromHtml("#93C5FD");
        public static readonly Color ButtonText = Color.White;

        // Estados servicios
        public static readonly Color RunningBackground = ColorTranslator.FromHtml("#DCFCE7");
        public static readonly Color RunningText = ColorTranslator.FromHtml("#166534");

        public static readonly Color StoppedBackground = ColorTranslator.FromHtml("#FEE2E2");
        public static readonly Color StoppedText = ColorTranslator.FromHtml("#991B1B");

        public static readonly Color WarningBackground = ColorTranslator.FromHtml("#FEF3C7");
        public static readonly Color WarningText = ColorTranslator.FromHtml("#78350F");

        // Texto
        public static readonly Color TextPrimary = ColorTranslator.FromHtml("#111827");
        public static readonly Color TextSecondary = ColorTranslator.FromHtml("#6B7280");

        // Bordes y redondeo
        public static readonly int BorderRadius = 6;



        // Métodos de aplicación de estilo
        public static void ApplyButtonRadius(Button button, int radius)
        {
            int width = button.Width;
            int height = button.Height;

            GraphicsPath path = new GraphicsPath();
            path.StartFigure();

            // esquina superior izquierda
            path.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
            // borde superior
            path.AddLine(radius, 0, width - radius, 0);
            // esquina superior derecha
            path.AddArc(width - radius * 2, 0, radius * 2, radius * 2, 270, 90);
            // borde derecho
            path.AddLine(width, radius, width, height - radius);
            // esquina inferior derecha
            path.AddArc(width - radius * 2, height - radius * 2, radius * 2, radius * 2, 0, 90);
            // borde inferior
            path.AddLine(width - radius, height, radius, height);
            // esquina inferior izquierda
            path.AddArc(0, height - radius * 2, radius * 2, radius * 2, 90, 90);
            // borde izquierdo
            path.AddLine(0, height - radius, 0, radius);

            path.CloseFigure();

            button.Region = new Region(path);
        }

        public static void ApplyFormStyle(Form form)
        {
            form.BackColor = BackgroundMain;
            form.ForeColor = TextPrimary;
        }

        public static void ApplyGroupBoxStyle(GroupBox groupBox)
        {
            groupBox.BackColor = PanelBackground;
            //panel.BorderStyle = BorderStyle.FixedSingle;
            
        }

        public static void ApplyButtonPrimary(Button button)
        {
            button.BackColor = ButtonPrimary;
            button.ForeColor = ButtonText;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ButtonPrimaryHover;
        }

        public static void ApplyButtonSecondary(Button button)
        {
            button.BackColor = PanelBackground;
            button.ForeColor = TextPrimary;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = BorderColor;
            button.FlatAppearance.BorderSize = 1;
        }

        public static void SetPictureBoxOpacity(PictureBox pictureBox, float opacity)
        {
            if (pictureBox.Image == null) return;

            Bitmap bmp = new Bitmap(pictureBox.Image.Width, pictureBox.Image.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);

                ColorMatrix matrix = new ColorMatrix
                {
                    Matrix33 = opacity
                };

                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                g.DrawImage(pictureBox.Image, new Rectangle(0, 0, bmp.Width, bmp.Height),
                            0, 0, pictureBox.Image.Width, pictureBox.Image.Height,
                            GraphicsUnit.Pixel, attributes);
            }

            pictureBox.Image = bmp;
        }


        public static void ApplyServiceCellStyle(DataGridViewCell cell, string status)
        {
            switch (status.ToLower())
            {
                case "running":
                    cell.Style.BackColor = RunningBackground;
                    cell.Style.ForeColor = RunningText;
                    break;
                case "stopped":
                    cell.Style.BackColor = StoppedBackground;
                    cell.Style.ForeColor = StoppedText;
                    break;
                case "warning":
                    cell.Style.BackColor = WarningBackground;
                    cell.Style.ForeColor = WarningText;
                    break;
                default:
                    cell.Style.BackColor = PanelBackground;
                    cell.Style.ForeColor = TextPrimary;
                    break;
            }
        }
    }
}