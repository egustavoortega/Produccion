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
    public partial class Report : Form
    {
        SqlConnection CadenaConexion;
        private GroupBox groupBox1;
        private ListBox lb_Lista;
        private Button button2;
        private Button button1;
        private Button button3;
        private TextBox txt_IdReporte;
        Timer MyTimer = new Timer();
        string NombreTXT = "";
		DataTable reportes;

        public Report(SqlConnection Conexion)
        {
            InitializeComponent();
            CadenaConexion = Conexion;

            NombreTXT = "";
            Dictionary<string, object> parametros = new Dictionary<string, object>();
            parametros.Add("@Name", "RutaReporte");
            parametros.Add("@opcion", "1");
            DataTable dt = ExecuteSP("[dbo].[Get_Traking_Set]", parametros);
            NombreTXT = dt.Rows[0][0].ToString();
            if (!NombreTXT.StartsWith("\\"))
            {
                NombreTXT = NombreTXT + "\\";
            }
        }

        private void Report_Load(object sender, EventArgs e)
        {
            try
            {
                NombreTXT = "";
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@Name", "RutaReporte");
                parametros.Add("@opcion", "1");
                DataTable dt = ExecuteSP("[dbo].[Get_Traking_Set]", parametros);
                NombreTXT = dt.Rows[0][0].ToString();
                if (!NombreTXT.StartsWith("\\"))
                {
                    NombreTXT = NombreTXT + "\\";
                }
				
				#region CargueReporte
				Dictionary<string, object> Reporte = new Dictionary<string, object>();
				DataTable dt_Reporte = ExecuteSP("[dbo].[Get_Reporte]", Reporte);
				reportes = dt_Reporte;

				if (lb_Lista.Items.Count == 0)
				{
					if (dt_Reporte.Rows.Count > 0)
					{
						foreach (DataRow Datos in dt_Reporte.Rows)
						{
							lb_Lista.Items.Add(Datos.ItemArray[1].ToString());
						}

					}
				}
				

				#endregion


			}
			catch(Exception )
            {
            }
        }

       

   

        private void GenerarReporte(int opcion, string Nombre , string Ext)
        {
            try
            {

                #region Reporte 1

                Report_Load(null, null);
                NombreTXT = NombreTXT + Nombre +"_"+ DateTime.Now.ToString("yyyyMMddhhmmss") + Ext;
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@opcion", opcion);
                DataTable dt = ExecuteSP("[dbo].[Rpt_Estadistica]", parametros);
                if (dt.Rows.Count > 0)
                {
                    
                    EscribirArchivoTxt(NombreTXT, dt);
                    MessageBox.Show("Proceso Terminado");

                }
                else
                {
                    MessageBox.Show("Los valores de busqueda no arrojaron resultado.");
                }
                #endregion
            }
            catch (Exception ex)
            {

                #region Reporte 1
                Dictionary<string, object> parametros = new Dictionary<string, object>();
                parametros.Add("@opcion", opcion);
                DataTable dt = ExecuteSP2("[dbo].[Rpt_Estadistica]", parametros);
                if (dt.Rows.Count > 0)
                {
                    EscribirArchivoTxt(NombreTXT, dt);
                    MessageBox.Show("Proceso Terminado");

                }
                else
                {
                    MessageBox.Show("Los valores de busqueda no arrojaron resultado.");
                }
                #endregion

            }
        }

        private bool EscribirArchivoTxt(string NombreTXT, DataTable Resultado)
        {
            try
            {
                foreach (DataRow D in Resultado.Rows)
                {
                    

                    string Linea = D.ItemArray[0].ToString();

                    string Nombre = NombreTXT;

                    if (File.Exists(Nombre))
                    {
                        StreamWriter objWr = File.AppendText(Nombre);
                        objWr.WriteLine(Linea);
                        objWr.Flush();
                        objWr.Close();
                    }
                    else
                    {
                        StreamWriter objWr = File.CreateText(Nombre);
                        objWr.WriteLine(Linea);
                        objWr.Flush();
                        objWr.Close();
                    }


                }
                return false;
            }
            catch (Exception xe)
            {
                return false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MyTimer.Dispose();
            this.Close();
        }

        private void InitializeComponent()
        {
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.txt_IdReporte = new System.Windows.Forms.TextBox();
			this.button3 = new System.Windows.Forms.Button();
			this.lb_Lista = new System.Windows.Forms.ListBox();
			this.button2 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.txt_IdReporte);
			this.groupBox1.Controls.Add(this.button3);
			this.groupBox1.Controls.Add(this.lb_Lista);
			this.groupBox1.Controls.Add(this.button2);
			this.groupBox1.Controls.Add(this.button1);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
			this.groupBox1.Location = new System.Drawing.Point(0, 0);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(346, 510);
			this.groupBox1.TabIndex = 13;
			this.groupBox1.TabStop = false;
			// 
			// txt_IdReporte
			// 
			this.txt_IdReporte.Location = new System.Drawing.Point(304, 364);
			this.txt_IdReporte.Name = "txt_IdReporte";
			this.txt_IdReporte.Size = new System.Drawing.Size(36, 20);
			this.txt_IdReporte.TabIndex = 15;
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(12, 419);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(328, 23);
			this.button3.TabIndex = 14;
			this.button3.Text = "Generar Todos";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Visible = false;
			// 
			// lb_Lista
			// 
			this.lb_Lista.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.lb_Lista.FormattingEnabled = true;
			this.lb_Lista.Location = new System.Drawing.Point(6, 16);
			this.lb_Lista.Name = "lb_Lista";
			this.lb_Lista.ScrollAlwaysVisible = true;
			this.lb_Lista.Size = new System.Drawing.Size(334, 312);
			this.lb_Lista.TabIndex = 12;
			this.lb_Lista.SelectedIndexChanged += new System.EventHandler(this.lb_Lista_SelectedIndexChanged);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(12, 477);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(328, 23);
			this.button2.TabIndex = 11;
			this.button2.Text = "Salir";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click_1);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(11, 448);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(329, 23);
			this.button1.TabIndex = 4;
			this.button1.Text = "Generar Reporte";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click_1);
			// 
			// Report
			// 
			this.ClientSize = new System.Drawing.Size(348, 510);
			this.Controls.Add(this.groupBox1);
			this.Name = "Report";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Reportes";
			this.Load += new System.EventHandler(this.Report_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (lb_Lista.SelectedItem.ToString() != "")
                {
					int opcion = 90;
					string ext = "";
					foreach (DataRow d in reportes.Rows)
					{
						if (d.ItemArray[1].ToString()== lb_Lista.SelectedItem.ToString())
						{
							opcion = int.Parse(d.ItemArray[0].ToString());
							ext = d.ItemArray[2].ToString();
						}
					}

					if (opcion != 90)
					{
						GenerarReporte(opcion, lb_Lista.SelectedItem.ToString(), ext);
					}
                }

            }
            catch (Exception xe)
            {
            }

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private DataTable ExecuteSP(string storeProcedure, Dictionary<string, object> parametro, bool isParametro = false)
        {
            try
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
            }catch(Exception x)
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

        private void lb_Lista_SelectedIndexChanged(object sender, EventArgs e)
        {
            txt_IdReporte.Text = lb_Lista.SelectedIndex.ToString();
        }



    }
}
