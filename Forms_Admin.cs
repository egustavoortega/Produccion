using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Traking_Forms
{
    public partial class Forms_Admin : Form
    {
        string _Ehlocal = "";
        SqlConnection CadenaConexion;
        public Forms_Admin(SqlConnection Conexion)
        {
            InitializeComponent();
            CadenaConexion = Conexion;
            _Ehlocal = @"C:\Users\Public\ReadSoft\FORMS";
        }

        #region Metodos Basicos
        private DataTable ExecuteSP(string storeProcedure, Dictionary<string, object> parametro, bool isParametro = false)
        {
            SqlConnection conexion = CadenaConexion;

            conexion.Open();
            SqlCommand comando = new SqlCommand(storeProcedure, conexion);
            foreach (KeyValuePair<string, object> aux in parametro)
                comando.Parameters.Add(new SqlParameter(aux.Key, aux.Value));
            comando.CommandType = CommandType.StoredProcedure;
            comando.CommandTimeout = 0;
            SqlDataReader objRead = comando.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(objRead);
            //conexion.Close();
            //conexion.Dispose();
            return dt;
        }

        private DataTable ExecuteSP2(string storeProcedure, Dictionary<string, object> parametro, bool isParametro = false)
        {
            SqlConnection conexion = CadenaConexion;

            SqlCommand comando = new SqlCommand(storeProcedure, conexion);
            foreach (KeyValuePair<string, object> aux in parametro)
                comando.Parameters.Add(new SqlParameter(aux.Key, aux.Value));
            comando.CommandType = CommandType.StoredProcedure;
            comando.CommandTimeout = 0;
            SqlDataReader objRead = comando.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(objRead);

            return dt;
        }

        private SqlConnection GetConexion()
        {
            SqlConnection conexion;
            conexion = new SqlConnection(ConfigurationManager.ConnectionStrings["ConexionData"].ConnectionString);
            return conexion;
        }


        #endregion

        private void button2_Click(object sender, EventArgs e)
        {
            gb_Priorizacion.Enabled = true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            cb_Municipio.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Forms_Admin_Load(null, null);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Dictionary<string, object> parametros = new Dictionary<string, object>();
            parametros.Add("@opcion", "2");
            parametros.Add("@departamento", cb_Departamento.SelectedItem);
            parametros.Add("@municipio", cb_Municipio.SelectedItem);
            DataTable dt = ExecuteSP2("[dbo].[Traking_Doc]", parametros);
            DGV_Datos.DataSource = dt;

            gb_Priorizacion.Enabled = false;
            Forms_Admin_Load(null, null);
            MessageBox.Show("Priorizacion realizada");

        }

        private void Btn_Despriorizar_Click(object sender, EventArgs e)
        {
            cb_Departamento.Visible = false;
            Dictionary<string, object> parametros = new Dictionary<string, object>();
            parametros.Add("@opcion", "3");
            DataTable dt = ExecuteSP2("[dbo].[Traking_Doc]", parametros);
            Forms_Admin_Load(null, null);
            MessageBox.Show("Desriorizacion realizada");
        }

        private void Forms_Admin_Load(object sender, EventArgs e)
        {
            try
            {
                Lst_Estados.Items.Clear();
                lbl_Mensaje.Visible = false;
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@opcion", "1");
                DataTable dt = ExecuteSP("[dbo].[Traking_Doc]", parametros);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow Dato in dt.Rows)
                    {
                        Lst_Estados.Items.Add(Dato.ItemArray[0].ToString() + " (" + Dato.ItemArray[1].ToString() + ")");
                    }


                }
                else
                {
                    lbl_Mensaje.Visible = true;
                    lbl_Mensaje.Text = "Actualmente no se encuentra Formularios ";
                }


                Dictionary<string, object> parametros2 = new Dictionary<string, object>();
                parametros2.Add("@opcion", "1");
                DataTable dt2 = ExecuteSP2("[dbo].[Traking_Priorizacion]", parametros2);

                if (dt2.Rows.Count > 0)
                {
                    foreach (DataRow Datos in dt2.Rows)
                    {
                        cb_Departamento.Items.Add(Datos.ItemArray[0]);

                    }
                }
                cb_Departamento.Refresh();
            }
            catch (Exception ex)
            {
                Lst_Estados.Items.Clear();
                lbl_Mensaje.Visible = false;
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@opcion", "1");
                DataTable dt = ExecuteSP2("[dbo].[Traking_Doc]", parametros);

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow Dato in dt.Rows)
                    {
                        Lst_Estados.Items.Add(Dato.ItemArray[0].ToString() + " (" + Dato.ItemArray[1].ToString() + ")");
                    }

                }
                else
                {
                    lbl_Mensaje.Visible = true;
                    lbl_Mensaje.Text = "Actualmente no se encuentra Formularios ";
                }

                Dictionary<string, object> parametros2 = new Dictionary<string, object>();
                parametros2.Add("@opcion", "1");
                DataTable dt2 = ExecuteSP2("[dbo].[Traking_Priorizacion]", parametros2);

                if (dt2.Rows.Count > 0)
                {
                    foreach (DataRow Datos in dt2.Rows)
                    {
                        cb_Departamento.Items.Add(Datos.ItemArray[0]);

                    }
                }
                cb_Departamento.Refresh();

            }
        }

        private void cb_Departamento_SelectedValueChanged(object sender, EventArgs e)
        {
            cb_Municipio.Items.Clear();


            Dictionary<string, object> parametros2 = new Dictionary<string, object>();
            parametros2.Add("@opcion", "2");
            parametros2.Add("@Depto", cb_Departamento.SelectedItem.ToString());
            DataTable dt2 = ExecuteSP2("[dbo].[Traking_Priorizacion]", parametros2);

            if (dt2.Rows.Count > 0)
            {
                foreach (DataRow Datos in dt2.Rows)
                {
                    cb_Municipio.Items.Add(Datos.ItemArray[0]);

                }
            }
            cb_Municipio.Visible = true;
            cb_Municipio.Refresh();


        }

        

    }
}
