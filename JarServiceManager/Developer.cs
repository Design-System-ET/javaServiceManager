using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace JarServiceManager
{
    public partial class Developer : Form
    {
        public Developer()
        {
            InitializeComponent();

            //aplicando stilos
            Style.ApplyButtonPrimary(button1);
            Style.ApplyButtonRadius(button1, 1);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
