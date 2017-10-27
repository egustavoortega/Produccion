namespace Traking_Forms
{
    partial class Report
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
            this.GB2 = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.MTB_FFinal = new System.Windows.Forms.MaskedTextBox();
            this.MTB_Finicial = new System.Windows.Forms.MaskedTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.DGV_Datos = new System.Windows.Forms.DataGridView();
            this.GB2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGV_Datos)).BeginInit();
            this.SuspendLayout();
            // 
            // GB2
            // 
            this.GB2.Controls.Add(this.DGV_Datos);
            this.GB2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GB2.Location = new System.Drawing.Point(93, 0);
            this.GB2.Name = "GB2";
            this.GB2.Size = new System.Drawing.Size(727, 299);
            this.GB2.TabIndex = 12;
            this.GB2.TabStop = false;
            this.GB2.Visible = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.MTB_FFinal);
            this.groupBox1.Controls.Add(this.MTB_Finicial);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(93, 299);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 268);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 11;
            this.button2.Text = "Salir";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // MTB_FFinal
            // 
            this.MTB_FFinal.Location = new System.Drawing.Point(8, 383);
            this.MTB_FFinal.Mask = "00/00/0000 00:00";
            this.MTB_FFinal.Name = "MTB_FFinal";
            this.MTB_FFinal.Size = new System.Drawing.Size(127, 20);
            this.MTB_FFinal.TabIndex = 10;
            this.MTB_FFinal.ValidatingType = typeof(System.DateTime);
            this.MTB_FFinal.Visible = false;
            // 
            // MTB_Finicial
            // 
            this.MTB_Finicial.Location = new System.Drawing.Point(11, 330);
            this.MTB_Finicial.Mask = "00/00/0000 00:00";
            this.MTB_Finicial.Name = "MTB_Finicial";
            this.MTB_Finicial.Size = new System.Drawing.Size(127, 20);
            this.MTB_Finicial.TabIndex = 9;
            this.MTB_Finicial.ValidatingType = typeof(System.DateTime);
            this.MTB_Finicial.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 367);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Fecha Final  (D/M/Y H:M)";
            this.label2.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 314);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Fecha Inicial (D/M/Y H:M)";
            this.label1.Visible = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(10, 19);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(72, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Estadistica";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // DGV_Datos
            // 
            this.DGV_Datos.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader;
            this.DGV_Datos.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllHeaders;
            this.DGV_Datos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DGV_Datos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DGV_Datos.Location = new System.Drawing.Point(3, 16);
            this.DGV_Datos.Name = "DGV_Datos";
            this.DGV_Datos.Size = new System.Drawing.Size(721, 280);
            this.DGV_Datos.TabIndex = 0;
            // 
            // Report
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(820, 299);
            this.Controls.Add(this.GB2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Report";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Report";
            this.Load += new System.EventHandler(this.Report_Load);
            this.GB2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DGV_Datos)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox GB2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.MaskedTextBox MTB_FFinal;
        private System.Windows.Forms.MaskedTextBox MTB_Finicial;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView DGV_Datos;
    }
}