using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using System.Threading;

namespace Obis_New
{
    public partial class Picture_Show : DevExpress.XtraEditors.XtraForm
    {
        public string Image_Path;
        Thread thDraw;
        delegate void myDrawRectangel();
        myDrawRectangel mydraw;
        private Point ptBegin = new Point();
        private bool blIsDrawRectangle = false;
        //const int WM_NCLBUTTONDOWN = 0x00A1;//固定标题栏
        //const int HTCAPTION = 2;
        //protected override void WndProc(ref Message m)
        //{
        //    if (m.Msg == WM_NCLBUTTONDOWN && m.WParam.ToInt32() == HTCAPTION)
        //        return;
        //    base.WndProc(ref m);
        //}

        public Picture_Show()
        {
            InitializeComponent();
        }

        public void Picture_ShowInit()
        {
            pictureEdit1.Image = Image.FromFile(Image_Path);
        }

        private void ShowDrawRectangle()
        {
            Rectangle rec = new Rectangle(ptBegin.X * pictureEdit1.Image.Size.Width / 1352, ptBegin.Y * pictureEdit1.Image.Size.Height / 732,
                                           250 * pictureEdit1.Image.Size.Width / 1352, 250 * pictureEdit1.Image.Size.Height / 732);
            Graphics g = pictureBox2.CreateGraphics();
            g.DrawImage(pictureEdit1.Image, pictureBox2.ClientRectangle, rec, GraphicsUnit.Pixel);
            g.Flush();
        }

        private void pictureEdit1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox2.Visible = true;
            blIsDrawRectangle = true;
        }

        private void pictureEdit1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.X - 125 <= 0)
            {
                ptBegin.X = 0;
            }
            else if (pictureEdit1.Size.Width - e.X <=125)
            {
                ptBegin.X = pictureEdit1.Size.Width - 250;
            }
            else
            {
                ptBegin.X = e.X - 125;
            }

            if (e.Y - 125 <= 0)
            {
                ptBegin.Y = 0;
            }
            else if (pictureEdit1.Size.Height - e.Y <= 125)
            {
                ptBegin.Y = pictureEdit1.Size.Height - 250;
            }
            else
            {
                ptBegin.Y = e.Y - 125;
            }
            pictureEdit1.Refresh();
        }

        private void pictureEdit1_MouseLeave(object sender, EventArgs e)
        {
            pictureBox2.Visible = false;
            blIsDrawRectangle = false;
            pictureEdit1.Refresh();
        }

        private void pictureEdit1_Paint(object sender, PaintEventArgs e)
        {
            if (blIsDrawRectangle)
            {
                e.Graphics.DrawRectangle(new Pen(Brushes.Black, 1), ptBegin.X, ptBegin.Y,250, 250);
            }
        }

        private void Picture_Show_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (thDraw != null)
            {
                thDraw.Abort();
            }
            pictureEdit1.Image.Dispose();
        }
        private void Run()
        {
            while (true)
            {
                if (pictureEdit1.Image != null)
                {
                    this.BeginInvoke(mydraw);
                }
                Thread.Sleep(50);
            }
        }
        private void Picture_Show_Load(object sender, EventArgs e)
        {
            mydraw = new myDrawRectangel(ShowDrawRectangle);
            thDraw = new Thread(Run);
            thDraw.Start();
        }
    }
}