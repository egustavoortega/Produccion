using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Traking_Forms
{
    public partial class VisorFormulario : Form
    {
        Image ImgOriginal;
        string  CadenaRuta;

        public VisorFormulario(String Ruta)
        {
            InitializeComponent();
            CadenaRuta = Ruta;
        }

        private void VisorFormulario_Load(object sender, EventArgs e)
        {
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            panel1.AutoScroll = true;
            this.pb_Visor.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;  
            pb_Visor.Image = Image.FromFile(CadenaRuta);
            ImgOriginal = pb_Visor.Image;
            ClassData.Zoom = -70;
            if (ClassData.Zoom >= -80)
            {
                ClassData.Zoom = ClassData.Zoom - 10;
                pb_Visor.Image = Zoom(ImgOriginal, new Size(ClassData.Zoom, ClassData.Zoom));
            }
            else
            {
                MessageBox.Show("La imagen no se puede ampliar mas");
            }

        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

 
        private Image Zoom(Image img, Size size)
        {
            try
            {

                Bitmap bmp = new Bitmap(img, img.Width + (img.Width * size.Width / 100), img.Height + (img.Height * size.Height / 100));
                Graphics g = Graphics.FromImage(bmp);
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                return bmp;

            }
            catch (Exception ex)
            {
                return null;
            }
        }


        private void panel3_Click(object sender, EventArgs e)
        {
            if (ClassData.Zoom >= -80)
            {
                ClassData.Zoom = ClassData.Zoom - 10;
                pb_Visor.Image = Zoom(ImgOriginal, new Size(ClassData.Zoom, ClassData.Zoom));
            }
            else
            {
                MessageBox.Show("La imagen no se puede ampliar mas");
            }
        }

        private void panel2_Click(object sender, EventArgs e)
        {
            if (ClassData.Zoom <= 10)
            {

                ClassData.Zoom = ClassData.Zoom + 10;
                pb_Visor.Image = Zoom(ImgOriginal, new Size(ClassData.Zoom, ClassData.Zoom));
            }
            else
            {
                MessageBox.Show("La imagen no se puede ampliar mas");
            }
        }
    }
}
