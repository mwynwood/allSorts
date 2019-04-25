using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AllSorts
{
    public partial class frmSplash : Form
    {
        public int ticks = 0;

        public frmSplash()
        {
            InitializeComponent();

            this.Text = Application.ProductName + " (" + Application.ProductVersion + ")";
           
            progressBar1.Maximum = 10;
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.MarqueeAnimationSpeed = 50;
            progressBar1.UseWaitCursor = true;

            timer1.Interval = 100;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ticks = ticks + 1;
            progressBar1.Value = ticks;

            if (progressBar1.Value == progressBar1.Maximum)
            {
                timer1.Stop();
                this.TopMost = true;
                frmMain f = new frmMain();
                this.Hide();
                f.Show();
            }
        }

    }
}