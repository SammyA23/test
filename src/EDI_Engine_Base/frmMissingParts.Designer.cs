namespace EDI
{
    partial class frmMissingParts
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.PartNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PO = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Plant = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Client = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Source = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Created = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.CocherTout_Button = new System.Windows.Forms.Button();
            this.CocherAucun_Button = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(15, 571);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(704, 36);
            this.button1.TabIndex = 0;
            this.button1.Text = "Ok!";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(15, 529);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(704, 36);
            this.button2.TabIndex = 3;
            this.button2.Text = "Sauvegarder la liste";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "csv";
            this.saveFileDialog1.FileName = "Materiaux";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PartNumber,
            this.PO,
            this.Plant,
            this.Client,
            this.Source,
            this.Created});
            this.dataGridView1.Location = new System.Drawing.Point(15, 27);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.Size = new System.Drawing.Size(704, 496);
            this.dataGridView1.TabIndex = 1;
            // 
            // PartNumber
            // 
            this.PartNumber.HeaderText = "Pièces";
            this.PartNumber.Name = "PartNumber";
            this.PartNumber.ReadOnly = true;
            // 
            // PO
            // 
            this.PO.HeaderText = "PO";
            this.PO.Name = "PO";
            // 
            // Plant
            // 
            this.Plant.HeaderText = "Plant";
            this.Plant.Name = "Plant";
            this.Plant.ReadOnly = true;
            // 
            // Client
            // 
            this.Client.HeaderText = "JB Client";
            this.Client.Name = "Client";
            this.Client.ReadOnly = true;
            // 
            // Source
            // 
            this.Source.HeaderText = "Source Manquante";
            this.Source.Name = "Source";
            this.Source.ReadOnly = true;
            this.Source.Width = 200;
            // 
            // Created
            // 
            this.Created.HeaderText = "Crée";
            this.Created.Name = "Created";
            // 
            // CocherTout_Button
            // 
            this.CocherTout_Button.Location = new System.Drawing.Point(617, 1);
            this.CocherTout_Button.Name = "CocherTout_Button";
            this.CocherTout_Button.Size = new System.Drawing.Size(41, 23);
            this.CocherTout_Button.TabIndex = 4;
            this.CocherTout_Button.Text = "Tout";
            this.CocherTout_Button.UseVisualStyleBackColor = true;
            this.CocherTout_Button.Click += new System.EventHandler(this.CocherTout_Button_Click);
            // 
            // CocherAucun_Button
            // 
            this.CocherAucun_Button.Location = new System.Drawing.Point(664, 1);
            this.CocherAucun_Button.Name = "CocherAucun_Button";
            this.CocherAucun_Button.Size = new System.Drawing.Size(52, 23);
            this.CocherAucun_Button.TabIndex = 5;
            this.CocherAucun_Button.Text = "Aucun";
            this.CocherAucun_Button.UseVisualStyleBackColor = true;
            this.CocherAucun_Button.Click += new System.EventHandler(this.CocherAucun_Button_Click);
            // 
            // frmMissingParts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(732, 618);
            this.Controls.Add(this.CocherAucun_Button);
            this.Controls.Add(this.CocherTout_Button);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmMissingParts";
            this.Text = "Matériaux et/ou Sales Order manquants dans JobBOSS";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn PartNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn PO;
        private System.Windows.Forms.DataGridViewTextBoxColumn Plant;
        private System.Windows.Forms.DataGridViewTextBoxColumn Client;
        private System.Windows.Forms.DataGridViewTextBoxColumn Source;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Created;
        private System.Windows.Forms.Button CocherTout_Button;
        private System.Windows.Forms.Button CocherAucun_Button;
    }
}
