using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Traking_Forms
{
    public partial class MensajeCorregir : Form
    {
        public MensajeCorregir()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            lb_Mensaje.Text = "";
            lb_Mensaje.Visible = false;

            if (txt_Usuario.Text == "Admin")
            {
                if (txt_password.Text == "123456")
                {
                    if (txt_Valor.Text == "")
                    {
                        lb_Mensaje.Visible = true;
                        lb_Mensaje.Text = "";
                    }
                    else
                    {
                        ClassData.VlrAprobado = txt_Valor.Text;
                        this.Close();
                    }
                }
                else
                {
                    lb_Mensaje.Visible = true;
                    lb_Mensaje.Text = "La contraseña no es valida";
                }
            }
            else
            {
                lb_Mensaje.Visible = true;
                lb_Mensaje.Text = "El Usuario no es valido";
            }
            
        }

        private void button2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == (int)Keys.Enter)
            {
                button2_Click(null, null);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MensajeCorregir_Load(object sender, EventArgs e)
        {
            
        }
    }
}
