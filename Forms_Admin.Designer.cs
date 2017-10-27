namespace Traking_Forms
{
    partial class Forms_Admin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Btn_Despriorizar = new System.Windows.Forms.Button();
            this.DGV_Datos = new System.Windows.Forms.DataGridView();
            this.gb_Priorizacion = new System.Windows.Forms.GroupBox();
            this.cb_Departamento = new System.Windows.Forms.ComboBox();
            this.button3 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lbl_Departament = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.Lst_Estados = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.lbl_Mensaje = new System.Windows.Forms.Label();
            this.cb_Municipio = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.DGV_Datos)).BeginInit();
            this.gb_Priorizacion.SuspendLayout();
            this.SuspendLayout();
            // 
            // Btn_Despriorizar
            // 
            this.Btn_Despriorizar.Location = new System.Drawing.Point(4, 268);
            this.Btn_Despriorizar.Name = "Btn_Despriorizar";
            this.Btn_Despriorizar.Size = new System.Drawing.Size(155, 23);
            this.Btn_Despriorizar.TabIndex = 12;
            this.Btn_Despriorizar.Text = "Despriorizar";
            this.Btn_Despriorizar.UseVisualStyleBackColor = true;
            this.Btn_Despriorizar.Click += new System.EventHandler(this.Btn_Despriorizar_Click);
            // 
            // DGV_Datos
            // 
            this.DGV_Datos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV_Datos.Location = new System.Drawing.Point(162, 61);
            this.DGV_Datos.Name = "DGV_Datos";
            this.DGV_Datos.Size = new System.Drawing.Size(600, 448);
            this.DGV_Datos.TabIndex = 7;
            // 
            // gb_Priorizacion
            // 
            this.gb_Priorizacion.Controls.Add(this.cb_Municipio);
            this.gb_Priorizacion.Controls.Add(this.cb_Departamento);
            this.gb_Priorizacion.Controls.Add(this.button3);
            this.gb_Priorizacion.Controls.Add(this.label2);
            this.gb_Priorizacion.Controls.Add(this.lbl_Departament);
            this.gb_Priorizacion.Enabled = false;
            this.gb_Priorizacion.Location = new System.Drawing.Point(160, 1);
            this.gb_Priorizacion.Name = "gb_Priorizacion";
            this.gb_Priorizacion.Size = new System.Drawing.Size(602, 54);
            this.gb_Priorizacion.TabIndex = 11;
            this.gb_Priorizacion.TabStop = false;
            this.gb_Priorizacion.Text = "Estados Campos";
            // 
            // cb_Departamento
            // 
            this.cb_Departamento.FormattingEnabled = true;
            this.cb_Departamento.Location = new System.Drawing.Point(114, 21);
            this.cb_Departamento.Name = "cb_Departamento";
            this.cb_Departamento.Size = new System.Drawing.Size(121, 21);
            this.cb_Departamento.TabIndex = 6;
            this.cb_Departamento.SelectedValueChanged += new System.EventHandler(this.cb_Departamento_SelectedValueChanged);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(489, 18);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(107, 25);
            this.button3.TabIndex = 6;
            this.button3.Text = "Do";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(246, 25);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(69, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "No Municipio";
            // 
            // lbl_Departament
            // 
            this.lbl_Departament.AutoSize = true;
            this.lbl_Departament.Location = new System.Drawing.Point(17, 25);
            this.lbl_Departament.Name = "lbl_Departament";
            this.lbl_Departament.Size = new System.Drawing.Size(91, 13);
            this.lbl_Departament.TabIndex = 2;
            this.lbl_Departament.Text = "No Departamento";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(2, 239);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(155, 23);
            this.button2.TabIndex = 10;
            this.button2.Text = "Priorizar";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Lst_Estados
            // 
            this.Lst_Estados.FormattingEnabled = true;
            this.Lst_Estados.Location = new System.Drawing.Point(2, 5);
            this.Lst_Estados.Name = "Lst_Estados";
            this.Lst_Estados.Size = new System.Drawing.Size(154, 199);
            this.Lst_Estados.TabIndex = 9;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(2, 210);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(155, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Refrescar";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lbl_Mensaje
            // 
            this.lbl_Mensaje.AutoSize = true;
            this.lbl_Mensaje.ForeColor = System.Drawing.Color.Red;
            this.lbl_Mensaje.Location = new System.Drawing.Point(1, 512);
            this.lbl_Mensaje.Name = "lbl_Mensaje";
            this.lbl_Mensaje.Size = new System.Drawing.Size(11, 13);
            this.lbl_Mensaje.TabIndex = 13;
            this.lbl_Mensaje.Text = "*";
            this.lbl_Mensaje.Visible = false;
            // 
            // cb_Municipio
            // 
            this.cb_Municipio.FormattingEnabled = true;
            this.cb_Municipio.Location = new System.Drawing.Point(322, 21);
            this.cb_Municipio.Name = "cb_Municipio";
            this.cb_Municipio.Size = new System.Drawing.Size(121, 21);
            this.cb_Municipio.TabIndex = 7;
            // 
            // Forms_Admin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(762, 534);
            this.Controls.Add(this.lbl_Mensaje);
            this.Controls.Add(this.Btn_Despriorizar);
            this.Controls.Add(this.DGV_Datos);
            this.Controls.Add(this.gb_Priorizacion);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.Lst_Estados);
            this.Controls.Add(this.button1);
            this.Name = "Forms_Admin";
            this.Text = "Forms_Admin";
            this.Load += new System.EventHandler(this.Forms_Admin_Load);
            ((System.ComponentModel.ISupportInitialize)(this.DGV_Datos)).EndInit();
            this.gb_Priorizacion.ResumeLayout(false);
            this.gb_Priorizacion.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Btn_Despriorizar;
        private System.Windows.Forms.DataGridView DGV_Datos;
        private System.Windows.Forms.GroupBox gb_Priorizacion;
        private System.Windows.Forms.ComboBox cb_Departamento;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lbl_Departament;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ListBox Lst_Estados;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lbl_Mensaje;
        private System.Windows.Forms.ComboBox cb_Municipio;
    }
}