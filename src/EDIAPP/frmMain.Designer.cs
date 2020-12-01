namespace EDIAPP
{
    partial class frmMain
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.dataGridViewMain = new System.Windows.Forms.DataGridView();
            this.Material = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Sales_Order = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PO = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Qty_Old = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Qty_New = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Qty_Diff = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RefuseNewQty = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Qty_Diff_Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Date_Old = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Date_New = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Date_Diff = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RefuseNewDate = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.Date_Diff_Value = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NewDeliveryLocation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NewLineNumber = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SoDetailLine = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SO_Detail = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SO_Detail_Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Unit_Price = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FileOrigin = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.QtyCum = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Source = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Client = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DeliveryComment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip_frmMain = new System.Windows.Forms.MenuStrip();
            this.fichierToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ouvrirDeliveryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ouvrirForecastToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.préférencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.BookmarkSelectedLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.GoToBookmarkedLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.AboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_Read = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_Process = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_PrepareMaterialPlanning = new System.Windows.Forms.ToolStripButton();
            this.toolStrip_cboBox_Customer = new System.Windows.Forms.ToolStripComboBox();
            this.openFileDialog_CsvFile = new System.Windows.Forms.OpenFileDialog();
            this.lblDeliveryDateFormat = new System.Windows.Forms.Label();
            this.lblForecastDateFormat = new System.Windows.Forms.Label();
            this.lblThreadOperation = new System.Windows.Forms.Label();
            this.lblThreadMaxValue = new System.Windows.Forms.Label();
            this.lblThreadCurrentValue = new System.Windows.Forms.Label();
            this.cBoxDeliveryDateFormat = new System.Windows.Forms.ComboBox();
            this.cBoxForecastDateFormat = new System.Windows.Forms.ComboBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.cbo_FileType = new System.Windows.Forms.ComboBox();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.buttonOpen850File = new System.Windows.Forms.Button();
            this.buttonExec850 = new System.Windows.Forms.Button();
            this.buttonExec855 = new System.Windows.Forms.Button();
            this.buttonOpen860File = new System.Windows.Forms.Button();
            this.buttonExec860 = new System.Windows.Forms.Button();
            this.buttonExec865 = new System.Windows.Forms.Button();
            this.saveFileDialogCSV = new System.Windows.Forms.SaveFileDialog();
            this.dataGridView_ExtraFeatures = new System.Windows.Forms.DataGridView();
            this.NbRowsLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMain)).BeginInit();
            this.menuStrip_frmMain.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_ExtraFeatures)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewMain
            // 
            this.dataGridViewMain.AllowUserToAddRows = false;
            this.dataGridViewMain.AllowUserToDeleteRows = false;
            this.dataGridViewMain.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewMain.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridViewMain.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewMain.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Material,
            this.Sales_Order,
            this.PO,
            this.Qty_Old,
            this.Qty_New,
            this.Qty_Diff,
            this.RefuseNewQty,
            this.Qty_Diff_Value,
            this.Date_Old,
            this.Date_New,
            this.Date_Diff,
            this.RefuseNewDate,
            this.Date_Diff_Value,
            this.NewDeliveryLocation,
            this.NewLineNumber,
            this.SoDetailLine,
            this.SO_Detail,
            this.SO_Detail_Status,
            this.Unit_Price,
            this.FileOrigin,
            this.QtyCum,
            this.Source,
            this.Client,
            this.DeliveryComment,
            this.Id});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridViewMain.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridViewMain.Location = new System.Drawing.Point(0, 112);
            this.dataGridViewMain.MultiSelect = false;
            this.dataGridViewMain.Name = "dataGridViewMain";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridViewMain.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dataGridViewMain.RowHeadersWidth = 76;
            this.dataGridViewMain.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridViewMain.ShowEditingIcon = false;
            this.dataGridViewMain.Size = new System.Drawing.Size(899, 464);
            this.dataGridViewMain.TabIndex = 4;
            this.dataGridViewMain.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellValueChanged);
            this.dataGridViewMain.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGridView1_CurrentCellDirtyStateChanged);
            this.dataGridViewMain.Sorted += new System.EventHandler(this.dataGridView1_Sorted);
            // 
            // Material
            // 
            this.Material.HeaderText = "Material";
            this.Material.Name = "Material";
            // 
            // Sales_Order
            // 
            this.Sales_Order.HeaderText = "Sales Order";
            this.Sales_Order.Name = "Sales_Order";
            // 
            // PO
            // 
            this.PO.HeaderText = "PO";
            this.PO.Name = "PO";
            // 
            // Qty_Old
            // 
            this.Qty_Old.HeaderText = "Qty Old";
            this.Qty_Old.Name = "Qty_Old";
            // 
            // Qty_New
            // 
            this.Qty_New.HeaderText = "Qty New";
            this.Qty_New.Name = "Qty_New";
            // 
            // Qty_Diff
            // 
            this.Qty_Diff.HeaderText = "Qty Diff";
            this.Qty_Diff.Name = "Qty_Diff";
            // 
            // RefuseNewQty
            // 
            this.RefuseNewQty.HeaderText = "Refuse New Qty";
            this.RefuseNewQty.Name = "RefuseNewQty";
            this.RefuseNewQty.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.RefuseNewQty.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.RefuseNewQty.Width = 200;
            // 
            // Qty_Diff_Value
            // 
            this.Qty_Diff_Value.HeaderText = "Qty Diff Value";
            this.Qty_Diff_Value.Name = "Qty_Diff_Value";
            // 
            // Date_Old
            // 
            this.Date_Old.HeaderText = "Date Old";
            this.Date_Old.Name = "Date_Old";
            // 
            // Date_New
            // 
            this.Date_New.HeaderText = "Date New";
            this.Date_New.Name = "Date_New";
            // 
            // Date_Diff
            // 
            this.Date_Diff.HeaderText = "Date Diff";
            this.Date_Diff.Name = "Date_Diff";
            // 
            // RefuseNewDate
            // 
            this.RefuseNewDate.HeaderText = "Refuse New Date";
            this.RefuseNewDate.Name = "RefuseNewDate";
            this.RefuseNewDate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.RefuseNewDate.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.RefuseNewDate.Width = 200;
            // 
            // Date_Diff_Value
            // 
            this.Date_Diff_Value.HeaderText = "Date Diff Value";
            this.Date_Diff_Value.Name = "Date_Diff_Value";
            this.Date_Diff_Value.Width = 110;
            // 
            // NewDeliveryLocation
            // 
            this.NewDeliveryLocation.HeaderText = "Plant de livraison";
            this.NewDeliveryLocation.Name = "NewDeliveryLocation";
            // 
            // NewLineNumber
            // 
            this.NewLineNumber.HeaderText = "# de ligne du fichier";
            this.NewLineNumber.Name = "NewLineNumber";
            // 
            // SoDetailLine
            // 
            this.SoDetailLine.HeaderText = "# de ligne SO";
            this.SoDetailLine.Name = "SoDetailLine";
            // 
            // SO_Detail
            // 
            this.SO_Detail.HeaderText = "SO Detail";
            this.SO_Detail.Name = "SO_Detail";
            // 
            // SO_Detail_Status
            // 
            this.SO_Detail_Status.HeaderText = "SO Detail Status";
            this.SO_Detail_Status.Name = "SO_Detail_Status";
            this.SO_Detail_Status.ReadOnly = true;
            // 
            // Unit_Price
            // 
            this.Unit_Price.HeaderText = "Prix Unitaire";
            this.Unit_Price.Name = "Unit_Price";
            // 
            // FileOrigin
            // 
            this.FileOrigin.HeaderText = "Delivery || Forecast";
            this.FileOrigin.Name = "FileOrigin";
            // 
            // QtyCum
            // 
            this.QtyCum.HeaderText = "Qté cum reçue";
            this.QtyCum.Name = "QtyCum";
            this.QtyCum.ReadOnly = true;
            // 
            // Source
            // 
            this.Source.HeaderText = "Source";
            this.Source.Name = "Source";
            this.Source.ReadOnly = true;
            this.Source.Visible = false;
            // 
            // Client
            // 
            this.Client.HeaderText = "Client";
            this.Client.Name = "Client";
            this.Client.ReadOnly = true;
            // 
            // DeliveryComment
            // 
            this.DeliveryComment.HeaderText = "Commentaire de livraison";
            this.DeliveryComment.Name = "DeliveryComment";
            // 
            // Id
            // 
            this.Id.HeaderText = "Id";
            this.Id.Name = "Id";
            this.Id.ReadOnly = true;
            // 
            // menuStrip_frmMain
            // 
            this.menuStrip_frmMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fichierToolStripMenuItem,
            this.editToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip_frmMain.Location = new System.Drawing.Point(0, 0);
            this.menuStrip_frmMain.Name = "menuStrip_frmMain";
            this.menuStrip_frmMain.Size = new System.Drawing.Size(978, 24);
            this.menuStrip_frmMain.TabIndex = 5;
            this.menuStrip_frmMain.Text = "menuStrip1";
            // 
            // fichierToolStripMenuItem
            // 
            this.fichierToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ouvrirDeliveryToolStripMenuItem,
            this.ouvrirForecastToolStripMenuItem,
            this.préférencesToolStripMenuItem,
            this.quitterToolStripMenuItem});
            this.fichierToolStripMenuItem.Name = "fichierToolStripMenuItem";
            this.fichierToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.fichierToolStripMenuItem.Text = "Fichier";
            // 
            // ouvrirDeliveryToolStripMenuItem
            // 
            this.ouvrirDeliveryToolStripMenuItem.Name = "ouvrirDeliveryToolStripMenuItem";
            this.ouvrirDeliveryToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.ouvrirDeliveryToolStripMenuItem.Text = "Ouvrir dossier 830-862";
            this.ouvrirDeliveryToolStripMenuItem.Click += new System.EventHandler(this.ouvrirDeliveryToolStripMenuItem_Click);
            // 
            // ouvrirForecastToolStripMenuItem
            // 
            this.ouvrirForecastToolStripMenuItem.Name = "ouvrirForecastToolStripMenuItem";
            this.ouvrirForecastToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.ouvrirForecastToolStripMenuItem.Text = "Ouvrir forecast";
            this.ouvrirForecastToolStripMenuItem.Visible = false;
            this.ouvrirForecastToolStripMenuItem.Click += new System.EventHandler(this.ouvrirForecastToolStripMenuItem_Click);
            // 
            // préférencesToolStripMenuItem
            // 
            this.préférencesToolStripMenuItem.Name = "préférencesToolStripMenuItem";
            this.préférencesToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.préférencesToolStripMenuItem.Text = "Préférences (Ctrl+P)";
            this.préférencesToolStripMenuItem.Click += new System.EventHandler(this.préférencesToolStripMenuItem_Click);
            // 
            // quitterToolStripMenuItem
            // 
            this.quitterToolStripMenuItem.Name = "quitterToolStripMenuItem";
            this.quitterToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.quitterToolStripMenuItem.Text = "Quitter";
            this.quitterToolStripMenuItem.Click += new System.EventHandler(this.quitterToolStripMenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.BookmarkSelectedLineToolStripMenuItem,
            this.GoToBookmarkedLineToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // BookmarkSelectedLineToolStripMenuItem
            // 
            this.BookmarkSelectedLineToolStripMenuItem.Name = "BookmarkSelectedLineToolStripMenuItem";
            this.BookmarkSelectedLineToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.BookmarkSelectedLineToolStripMenuItem.Text = "Placer signet";
            // 
            // GoToBookmarkedLineToolStripMenuItem
            // 
            this.GoToBookmarkedLineToolStripMenuItem.Name = "GoToBookmarkedLineToolStripMenuItem";
            this.GoToBookmarkedLineToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
            this.GoToBookmarkedLineToolStripMenuItem.Text = "Aller au signet";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.AboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.helpToolStripMenuItem.Text = "Aide";
            // 
            // AboutToolStripMenuItem
            // 
            this.AboutToolStripMenuItem.Name = "AboutToolStripMenuItem";
            this.AboutToolStripMenuItem.Size = new System.Drawing.Size(122, 22);
            this.AboutToolStripMenuItem.Text = "À propos";
            this.AboutToolStripMenuItem.Click += new System.EventHandler(this.AboutToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripSeparator1,
            this.toolStripButton2,
            this.toolStripSeparator2,
            this.toolStripButton_Read,
            this.toolStripButton_Process,
            this.toolStripButton4,
            this.toolStripSeparator3,
            this.toolStripButton5,
            this.toolStripButton_PrepareMaterialPlanning,
            this.toolStrip_cboBox_Customer});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(978, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(99, 22);
            this.toolStripButton1.Text = "Dossier 830 - 862";
            this.toolStripButton1.ToolTipText = "(F3) Ouvrir un fichier \'Delivery\'";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            this.toolStripSeparator1.Visible = false;
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(55, 22);
            this.toolStripButton2.Text = "Forecast";
            this.toolStripButton2.ToolTipText = "(F4) Ouvrir un fichier de \'Forecast\'";
            this.toolStripButton2.Visible = false;
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            this.toolStripSeparator2.Visible = false;
            // 
            // toolStripButton_Read
            // 
            this.toolStripButton_Read.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_Read.Enabled = false;
            this.toolStripButton_Read.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_Read.Image")));
            this.toolStripButton_Read.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Read.Name = "toolStripButton_Read";
            this.toolStripButton_Read.Size = new System.Drawing.Size(50, 22);
            this.toolStripButton_Read.Text = "Lecture";
            this.toolStripButton_Read.ToolTipText = "(F5) Lecture";
            this.toolStripButton_Read.Click += new System.EventHandler(this.toolStripButton_Read_Click);
            // 
            // toolStripButton_Process
            // 
            this.toolStripButton_Process.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_Process.Enabled = false;
            this.toolStripButton_Process.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_Process.Image")));
            this.toolStripButton_Process.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Process.Name = "toolStripButton_Process";
            this.toolStripButton_Process.Size = new System.Drawing.Size(63, 22);
            this.toolStripButton_Process.Text = "Exécution";
            this.toolStripButton_Process.Click += new System.EventHandler(this.toolStripButton_Process_Click);
            // 
            // toolStripButton4
            // 
            this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton4.Enabled = false;
            this.toolStripButton4.Image = global::EDIAPP.Properties.Resources.bookmark1;
            this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton4.Text = "Marquer la ligne";
            this.toolStripButton4.Click += new System.EventHandler(this.toolStripBookmarkLine_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton5
            // 
            this.toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton5.Enabled = false;
            this.toolStripButton5.Image = global::EDIAPP.Properties.Resources.bookmarkGoTo1;
            this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton5.Name = "toolStripButton5";
            this.toolStripButton5.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton5.Text = "Aller à la ligne";
            this.toolStripButton5.Click += new System.EventHandler(this.toolStripBookmarkedLineGoTo_Click);
            // 
            // toolStripButton_PrepareMaterialPlanning
            // 
            this.toolStripButton_PrepareMaterialPlanning.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_PrepareMaterialPlanning.Enabled = false;
            this.toolStripButton_PrepareMaterialPlanning.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_PrepareMaterialPlanning.Image")));
            this.toolStripButton_PrepareMaterialPlanning.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_PrepareMaterialPlanning.Name = "toolStripButton_PrepareMaterialPlanning";
            this.toolStripButton_PrepareMaterialPlanning.Size = new System.Drawing.Size(91, 22);
            this.toolStripButton_PrepareMaterialPlanning.Text = "Prép.  Rapports";
            this.toolStripButton_PrepareMaterialPlanning.Click += new System.EventHandler(this.toolStripButton_PrepareMaterialPlanning_Click);
            // 
            // toolStrip_cboBox_Customer
            // 
            this.toolStrip_cboBox_Customer.Name = "toolStrip_cboBox_Customer";
            this.toolStrip_cboBox_Customer.Size = new System.Drawing.Size(121, 25);
            this.toolStrip_cboBox_Customer.Text = "Client:";
            this.toolStrip_cboBox_Customer.SelectedIndexChanged += new System.EventHandler(this.toolStrip_cboBox_Customer_SelectedIndexChanged);
            // 
            // openFileDialog_CsvFile
            // 
            this.openFileDialog_CsvFile.FileName = "openFileDialog1";
            // 
            // lblDeliveryDateFormat
            // 
            this.lblDeliveryDateFormat.AutoSize = true;
            this.lblDeliveryDateFormat.Location = new System.Drawing.Point(12, 55);
            this.lblDeliveryDateFormat.Name = "lblDeliveryDateFormat";
            this.lblDeliveryDateFormat.Size = new System.Drawing.Size(188, 13);
            this.lblDeliveryDateFormat.TabIndex = 8;
            this.lblDeliveryDateFormat.Text = "Format de date pour le fichier Delivery:";
            // 
            // lblForecastDateFormat
            // 
            this.lblForecastDateFormat.AutoSize = true;
            this.lblForecastDateFormat.Location = new System.Drawing.Point(12, 79);
            this.lblForecastDateFormat.Name = "lblForecastDateFormat";
            this.lblForecastDateFormat.Size = new System.Drawing.Size(191, 13);
            this.lblForecastDateFormat.TabIndex = 9;
            this.lblForecastDateFormat.Text = "Format de date pour le fichier Forecast:";
            // 
            // lblThreadOperation
            // 
            this.lblThreadOperation.AutoSize = true;
            this.lblThreadOperation.Location = new System.Drawing.Point(622, 55);
            this.lblThreadOperation.Name = "lblThreadOperation";
            this.lblThreadOperation.Size = new System.Drawing.Size(97, 13);
            this.lblThreadOperation.TabIndex = 15;
            this.lblThreadOperation.Text = "lblThreadOperation";
            // 
            // lblThreadMaxValue
            // 
            this.lblThreadMaxValue.AutoSize = true;
            this.lblThreadMaxValue.Location = new System.Drawing.Point(868, 84);
            this.lblThreadMaxValue.Name = "lblThreadMaxValue";
            this.lblThreadMaxValue.Size = new System.Drawing.Size(35, 13);
            this.lblThreadMaxValue.TabIndex = 16;
            this.lblThreadMaxValue.Text = "label4";
            // 
            // lblThreadCurrentValue
            // 
            this.lblThreadCurrentValue.AutoSize = true;
            this.lblThreadCurrentValue.Location = new System.Drawing.Point(868, 71);
            this.lblThreadCurrentValue.Name = "lblThreadCurrentValue";
            this.lblThreadCurrentValue.Size = new System.Drawing.Size(35, 13);
            this.lblThreadCurrentValue.TabIndex = 17;
            this.lblThreadCurrentValue.Text = "label5";
            // 
            // cBoxDeliveryDateFormat
            // 
            this.cBoxDeliveryDateFormat.FormattingEnabled = true;
            this.cBoxDeliveryDateFormat.Location = new System.Drawing.Point(218, 52);
            this.cBoxDeliveryDateFormat.Name = "cBoxDeliveryDateFormat";
            this.cBoxDeliveryDateFormat.Size = new System.Drawing.Size(98, 21);
            this.cBoxDeliveryDateFormat.TabIndex = 10;
            // 
            // cBoxForecastDateFormat
            // 
            this.cBoxForecastDateFormat.FormattingEnabled = true;
            this.cBoxForecastDateFormat.Location = new System.Drawing.Point(218, 76);
            this.cBoxForecastDateFormat.Name = "cBoxForecastDateFormat";
            this.cBoxForecastDateFormat.Size = new System.Drawing.Size(98, 21);
            this.cBoxForecastDateFormat.TabIndex = 11;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(0, 230);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(899, 45);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 12;
            this.progressBar1.Visible = false;
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(622, 74);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(236, 23);
            this.progressBar2.TabIndex = 14;
            // 
            // cbo_FileType
            // 
            this.cbo_FileType.FormattingEnabled = true;
            this.cbo_FileType.Location = new System.Drawing.Point(218, 74);
            this.cbo_FileType.Name = "cbo_FileType";
            this.cbo_FileType.Size = new System.Drawing.Size(98, 21);
            this.cbo_FileType.TabIndex = 18;
            // 
            // folderBrowserDialog
            // 
            this.folderBrowserDialog.ShowNewFolderButton = false;
            // 
            // buttonOpen850File
            // 
            this.buttonOpen850File.Enabled = false;
            this.buttonOpen850File.Location = new System.Drawing.Point(344, 56);
            this.buttonOpen850File.Name = "buttonOpen850File";
            this.buttonOpen850File.Size = new System.Drawing.Size(75, 23);
            this.buttonOpen850File.TabIndex = 19;
            this.buttonOpen850File.Text = "850 fichier";
            this.buttonOpen850File.UseVisualStyleBackColor = true;
            this.buttonOpen850File.Visible = false;
            this.buttonOpen850File.Click += new System.EventHandler(this.buttonOpen850File_Click);
            // 
            // buttonExec850
            // 
            this.buttonExec850.Enabled = false;
            this.buttonExec850.Location = new System.Drawing.Point(433, 56);
            this.buttonExec850.Name = "buttonExec850";
            this.buttonExec850.Size = new System.Drawing.Size(75, 23);
            this.buttonExec850.TabIndex = 20;
            this.buttonExec850.Text = "Exéc. 850";
            this.buttonExec850.UseVisualStyleBackColor = true;
            this.buttonExec850.Visible = false;
            this.buttonExec850.Click += new System.EventHandler(this.buttonExec850_Click);
            // 
            // buttonExec855
            // 
            this.buttonExec855.Enabled = false;
            this.buttonExec855.Location = new System.Drawing.Point(521, 56);
            this.buttonExec855.Name = "buttonExec855";
            this.buttonExec855.Size = new System.Drawing.Size(75, 23);
            this.buttonExec855.TabIndex = 21;
            this.buttonExec855.Text = "855";
            this.buttonExec855.UseVisualStyleBackColor = true;
            this.buttonExec855.Visible = false;
            this.buttonExec855.Click += new System.EventHandler(this.buttonExec855_Click);
            // 
            // buttonOpen860File
            // 
            this.buttonOpen860File.Enabled = false;
            this.buttonOpen860File.Location = new System.Drawing.Point(344, 83);
            this.buttonOpen860File.Name = "buttonOpen860File";
            this.buttonOpen860File.Size = new System.Drawing.Size(75, 23);
            this.buttonOpen860File.TabIndex = 22;
            this.buttonOpen860File.Text = "860 fichier";
            this.buttonOpen860File.UseVisualStyleBackColor = true;
            this.buttonOpen860File.Visible = false;
            this.buttonOpen860File.Click += new System.EventHandler(this.buttonOpen860File_Click);
            // 
            // buttonExec860
            // 
            this.buttonExec860.Enabled = false;
            this.buttonExec860.Location = new System.Drawing.Point(433, 83);
            this.buttonExec860.Name = "buttonExec860";
            this.buttonExec860.Size = new System.Drawing.Size(75, 23);
            this.buttonExec860.TabIndex = 23;
            this.buttonExec860.Text = "Exéc. 860";
            this.buttonExec860.UseVisualStyleBackColor = true;
            this.buttonExec860.Visible = false;
            this.buttonExec860.Click += new System.EventHandler(this.buttonExec860_Click);
            // 
            // buttonExec865
            // 
            this.buttonExec865.Enabled = false;
            this.buttonExec865.Location = new System.Drawing.Point(521, 83);
            this.buttonExec865.Name = "buttonExec865";
            this.buttonExec865.Size = new System.Drawing.Size(75, 23);
            this.buttonExec865.TabIndex = 24;
            this.buttonExec865.Text = "865";
            this.buttonExec865.UseVisualStyleBackColor = true;
            this.buttonExec865.Visible = false;
            this.buttonExec865.Click += new System.EventHandler(this.buttonExec865_Click);
            // 
            // saveFileDialogCSV
            // 
            this.saveFileDialogCSV.DefaultExt = "csv";
            this.saveFileDialogCSV.Filter = "\"CSV files|*.csv\"";
            this.saveFileDialogCSV.InitialDirectory = "%USERPROFILE%";
            this.saveFileDialogCSV.ValidateNames = false;
            // 
            // dataGridView_ExtraFeatures
            // 
            this.dataGridView_ExtraFeatures.AllowUserToAddRows = false;
            this.dataGridView_ExtraFeatures.AllowUserToDeleteRows = false;
            this.dataGridView_ExtraFeatures.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_ExtraFeatures.Location = new System.Drawing.Point(0, 112);
            this.dataGridView_ExtraFeatures.Name = "dataGridView_ExtraFeatures";
            this.dataGridView_ExtraFeatures.ReadOnly = true;
            this.dataGridView_ExtraFeatures.Size = new System.Drawing.Size(899, 441);
            this.dataGridView_ExtraFeatures.TabIndex = 25;
            // 
            // NbRowsLabel
            // 
            this.NbRowsLabel.AutoSize = true;
            this.NbRowsLabel.Location = new System.Drawing.Point(931, 83);
            this.NbRowsLabel.Name = "NbRowsLabel";
            this.NbRowsLabel.Size = new System.Drawing.Size(48, 13);
            this.NbRowsLabel.TabIndex = 26;
            this.NbRowsLabel.Text = "NbRows";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(978, 577);
            this.Controls.Add(this.NbRowsLabel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonExec865);
            this.Controls.Add(this.buttonExec860);
            this.Controls.Add(this.buttonOpen860File);
            this.Controls.Add(this.buttonExec855);
            this.Controls.Add(this.buttonExec850);
            this.Controls.Add(this.buttonOpen850File);
            this.Controls.Add(this.cbo_FileType);
            this.Controls.Add(this.progressBar2);
            this.Controls.Add(this.cBoxForecastDateFormat);
            this.Controls.Add(this.cBoxDeliveryDateFormat);
            this.Controls.Add(this.lblThreadCurrentValue);
            this.Controls.Add(this.lblThreadMaxValue);
            this.Controls.Add(this.lblThreadOperation);
            this.Controls.Add(this.lblForecastDateFormat);
            this.Controls.Add(this.lblDeliveryDateFormat);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.dataGridViewMain);
            this.Controls.Add(this.menuStrip_frmMain);
            this.Controls.Add(this.dataGridView_ExtraFeatures);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip_frmMain;
            this.Name = "frmMain";
            this.Text = "Application EDI";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.SizeChanged += new System.EventHandler(this.frmMain_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.frmMain_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewMain)).EndInit();
            this.menuStrip_frmMain.ResumeLayout(false);
            this.menuStrip_frmMain.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_ExtraFeatures)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewMain;
        private System.Windows.Forms.MenuStrip menuStrip_frmMain;
        private System.Windows.Forms.ToolStripMenuItem fichierToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem BookmarkSelectedLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem GoToBookmarkedLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem AboutToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_Read;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton_Process;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripButton toolStripButton_PrepareMaterialPlanning;
        private System.Windows.Forms.ToolStripComboBox toolStrip_cboBox_Customer;
        private System.Windows.Forms.OpenFileDialog openFileDialog_CsvFile;
        private System.Windows.Forms.Label lblDeliveryDateFormat;
        private System.Windows.Forms.Label lblForecastDateFormat;
        private System.Windows.Forms.Label lblThreadOperation;
        private System.Windows.Forms.Label lblThreadMaxValue;
        private System.Windows.Forms.Label lblThreadCurrentValue;
        private System.Windows.Forms.ComboBox cBoxDeliveryDateFormat;
        private System.Windows.Forms.ComboBox cBoxForecastDateFormat;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.ComboBox cbo_FileType;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.ToolStripMenuItem quitterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ouvrirDeliveryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ouvrirForecastToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem préférencesToolStripMenuItem;
        private System.Windows.Forms.Button buttonOpen850File;
        private System.Windows.Forms.Button buttonExec850;
        private System.Windows.Forms.Button buttonExec855;
        private System.Windows.Forms.Button buttonOpen860File;
        private System.Windows.Forms.Button buttonExec860;
        private System.Windows.Forms.Button buttonExec865;
        private System.Windows.Forms.DataGridView dataGridView_ExtraFeatures;
        private System.Windows.Forms.SaveFileDialog saveFileDialogCSV;
        private System.Windows.Forms.Label NbRowsLabel;
        private System.Windows.Forms.DataGridViewTextBoxColumn Material;
        private System.Windows.Forms.DataGridViewTextBoxColumn Sales_Order;
        private System.Windows.Forms.DataGridViewTextBoxColumn PO;
        private System.Windows.Forms.DataGridViewTextBoxColumn Qty_Old;
        private System.Windows.Forms.DataGridViewTextBoxColumn Qty_New;
        private System.Windows.Forms.DataGridViewTextBoxColumn Qty_Diff;
        private System.Windows.Forms.DataGridViewComboBoxColumn RefuseNewQty;
        private System.Windows.Forms.DataGridViewTextBoxColumn Qty_Diff_Value;
        private System.Windows.Forms.DataGridViewTextBoxColumn Date_Old;
        private System.Windows.Forms.DataGridViewTextBoxColumn Date_New;
        private System.Windows.Forms.DataGridViewTextBoxColumn Date_Diff;
        private System.Windows.Forms.DataGridViewComboBoxColumn RefuseNewDate;
        private System.Windows.Forms.DataGridViewTextBoxColumn Date_Diff_Value;
        private System.Windows.Forms.DataGridViewTextBoxColumn NewDeliveryLocation;
        private System.Windows.Forms.DataGridViewTextBoxColumn NewLineNumber;
        private System.Windows.Forms.DataGridViewTextBoxColumn SoDetailLine;
        private System.Windows.Forms.DataGridViewTextBoxColumn SO_Detail;
        private System.Windows.Forms.DataGridViewTextBoxColumn SO_Detail_Status;
        private System.Windows.Forms.DataGridViewTextBoxColumn Unit_Price;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileOrigin;
        private System.Windows.Forms.DataGridViewTextBoxColumn QtyCum;
        private System.Windows.Forms.DataGridViewTextBoxColumn Source;
        private System.Windows.Forms.DataGridViewTextBoxColumn Client;
        private System.Windows.Forms.DataGridViewTextBoxColumn DeliveryComment;
        private System.Windows.Forms.DataGridViewTextBoxColumn Id;
    }
}

