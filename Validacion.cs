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
    public partial class Validacion : Form
    {
        public Validacion()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            lb_Mensaje.Text = "";
            lb_Mensaje.Visible = false;

            if (txt_Usuario.Text == "Admin")
            {
                if (txt_password.Text == "123456")
                {
                        ClassData.Validacion = true;
                        this.Close();
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
                button2_Click(null,null);
            }
        }
    }
}
