namespace EDIAPP
{
    partial class FormConfigs
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
            this.textBoxUserName = new System.Windows.Forms.TextBox();
            this.textBoxWorkstation = new System.Windows.Forms.TextBox();
            this.comboBoxEngine = new System.Windows.Forms.ComboBox();
            this.checkBoxSoCreation = new System.Windows.Forms.CheckBox();
            this.comboBoxSoCreationStatus = new System.Windows.Forms.ComboBox();
            this.listBoxTemplates = new System.Windows.Forms.ListBox();
            this.buttonTemplateNew = new System.Windows.Forms.Button();
            this.buttonTemplateModify = new System.Windows.Forms.Button();
            this.buttonTemplateDelete = new System.Windows.Forms.Button();
            this.buttonClientDelete = new System.Windows.Forms.Button();
            this.buttonClientModify = new System.Windows.Forms.Button();
            this.buttonClientNew = new System.Windows.Forms.Button();
            this.listBoxClients = new System.Windows.Forms.ListBox();
            this.buttonFiletypesDelete = new System.Windows.Forms.Button();
            this.buttonFiletypesModify = new System.Windows.Forms.Button();
            this.buttonFiletypesNew = new System.Windows.Forms.Button();
            this.listBoxFiletypes = new System.Windows.Forms.ListBox();
            this.buttonExtrasDelete = new System.Windows.Forms.Button();
            this.buttonExtrasModify = new System.Windows.Forms.Button();
            this.buttonExtrasNew = new System.Windows.Forms.Button();
            this.listBoxExtras = new System.Windows.Forms.ListBox();
            this.checkBoxWriteSchedulineToNote = new System.Windows.Forms.CheckBox();
            this.labelUsername = new System.Windows.Forms.Label();
            this.labelWorkstation = new System.Windows.Forms.Label();
            this.labelEngine = new System.Windows.Forms.Label();
            this.labelSoCreationStatus = new System.Windows.Forms.Label();
            this.labelExecutionType = new System.Windows.Forms.Label();
            this.groupBoxExecutionType = new System.Windows.Forms.GroupBox();
            this.radioButtonExecutionComplex = new System.Windows.Forms.RadioButton();
            this.radioButtonExecutionDouble = new System.Windows.Forms.RadioButton();
            this.radioButtonExecutionSimple = new System.Windows.Forms.RadioButton();
            this.labelTemplates = new System.Windows.Forms.Label();
            this.labelClients = new System.Windows.Forms.Label();
            this.labelFiletypes = new System.Windows.Forms.Label();
            this.labelExtras = new System.Windows.Forms.Label();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.textBoxClientInput = new System.Windows.Forms.TextBox();
            this.textBoxFiletypesInput = new System.Windows.Forms.TextBox();
            this.comboBoxJbDbName = new System.Windows.Forms.ComboBox();
            this.textBoxJbDbServer = new System.Windows.Forms.TextBox();
            this.labelJbDbName = new System.Windows.Forms.Label();
            this.labelJbDbServer = new System.Windows.Forms.Label();
            this.textBoxExtrasInput = new System.Windows.Forms.TextBox();
            this.buttonConfNavPrev = new System.Windows.Forms.Button();
            this.buttonConfNavNext = new System.Windows.Forms.Button();
            this.textBoxConfigName = new System.Windows.Forms.TextBox();
            this.buttonConfAddNew = new System.Windows.Forms.Button();
            this.buttonConfDelete = new System.Windows.Forms.Button();
            this.checkBoxConfActive = new System.Windows.Forms.CheckBox();
            this.labelConfId = new System.Windows.Forms.Label();
            this.checkBoxUseShippedSoDetailLines = new System.Windows.Forms.CheckBox();
            this.groupBoxExecutionType.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxUserName
            // 
            this.textBoxUserName.Location = new System.Drawing.Point(164, 85);
            this.textBoxUserName.Name = "textBoxUserName";
            this.textBoxUserName.ReadOnly = true;
            this.textBoxUserName.Size = new System.Drawing.Size(146, 20);
            this.textBoxUserName.TabIndex = 7;
            this.textBoxUserName.TabStop = false;
            // 
            // textBoxWorkstation
            // 
            this.textBoxWorkstation.Location = new System.Drawing.Point(417, 85);
            this.textBoxWorkstation.Name = "textBoxWorkstation";
            this.textBoxWorkstation.ReadOnly = true;
            this.textBoxWorkstation.Size = new System.Drawing.Size(163, 20);
            this.textBoxWorkstation.TabIndex = 1;
            this.textBoxWorkstation.TabStop = false;
            // 
            // comboBoxEngine
            // 
            this.comboBoxEngine.FormattingEnabled = true;
            this.comboBoxEngine.Items.AddRange(new object[] {
            "Générique",
            "Pratt",
            "Bombardier"});
            this.comboBoxEngine.Location = new System.Drawing.Point(164, 114);
            this.comboBoxEngine.Name = "comboBoxEngine";
            this.comboBoxEngine.Size = new System.Drawing.Size(121, 21);
            this.comboBoxEngine.TabIndex = 2;
            this.comboBoxEngine.SelectedIndexChanged += new System.EventHandler(this.comboBoxEngine_SelectedIndexChanged);
            // 
            // checkBoxSoCreation
            // 
            this.checkBoxSoCreation.AutoSize = true;
            this.checkBoxSoCreation.Location = new System.Drawing.Point(50, 180);
            this.checkBoxSoCreation.Name = "checkBoxSoCreation";
            this.checkBoxSoCreation.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBoxSoCreation.Size = new System.Drawing.Size(183, 17);
            this.checkBoxSoCreation.TabIndex = 5;
            this.checkBoxSoCreation.Text = "Création des Bon de Commandes";
            this.checkBoxSoCreation.UseVisualStyleBackColor = true;
            this.checkBoxSoCreation.CheckStateChanged += new System.EventHandler(this.checkBoxSoCreation_CheckStateChanged);
            // 
            // comboBoxSoCreationStatus
            // 
            this.comboBoxSoCreationStatus.FormattingEnabled = true;
            this.comboBoxSoCreationStatus.Items.AddRange(new object[] {
            "Open",
            "Hold",
            "Closed"});
            this.comboBoxSoCreationStatus.Location = new System.Drawing.Point(427, 178);
            this.comboBoxSoCreationStatus.Name = "comboBoxSoCreationStatus";
            this.comboBoxSoCreationStatus.Size = new System.Drawing.Size(121, 21);
            this.comboBoxSoCreationStatus.TabIndex = 6;
            this.comboBoxSoCreationStatus.SelectedIndexChanged += new System.EventHandler(this.comboBoxSoCreationStatus_SelectedIndexChanged);
            // 
            // listBoxTemplates
            // 
            this.listBoxTemplates.FormattingEnabled = true;
            this.listBoxTemplates.Location = new System.Drawing.Point(165, 277);
            this.listBoxTemplates.Name = "listBoxTemplates";
            this.listBoxTemplates.Size = new System.Drawing.Size(443, 95);
            this.listBoxTemplates.TabIndex = 9;
            // 
            // buttonTemplateNew
            // 
            this.buttonTemplateNew.Location = new System.Drawing.Point(614, 277);
            this.buttonTemplateNew.Name = "buttonTemplateNew";
            this.buttonTemplateNew.Size = new System.Drawing.Size(75, 23);
            this.buttonTemplateNew.TabIndex = 8;
            this.buttonTemplateNew.Text = "Nouveau";
            this.buttonTemplateNew.UseVisualStyleBackColor = true;
            this.buttonTemplateNew.Click += new System.EventHandler(this.buttonTemplateNew_Click);
            // 
            // buttonTemplateModify
            // 
            this.buttonTemplateModify.Location = new System.Drawing.Point(614, 313);
            this.buttonTemplateModify.Name = "buttonTemplateModify";
            this.buttonTemplateModify.Size = new System.Drawing.Size(75, 23);
            this.buttonTemplateModify.TabIndex = 10;
            this.buttonTemplateModify.Text = "Modifier";
            this.buttonTemplateModify.UseVisualStyleBackColor = true;
            this.buttonTemplateModify.Click += new System.EventHandler(this.buttonTemplateModify_Click);
            // 
            // buttonTemplateDelete
            // 
            this.buttonTemplateDelete.Location = new System.Drawing.Point(614, 349);
            this.buttonTemplateDelete.Name = "buttonTemplateDelete";
            this.buttonTemplateDelete.Size = new System.Drawing.Size(75, 23);
            this.buttonTemplateDelete.TabIndex = 11;
            this.buttonTemplateDelete.Text = "Effacer";
            this.buttonTemplateDelete.UseVisualStyleBackColor = true;
            this.buttonTemplateDelete.Click += new System.EventHandler(this.buttonTemplateDelete_Click);
            // 
            // buttonClientDelete
            // 
            this.buttonClientDelete.Location = new System.Drawing.Point(317, 465);
            this.buttonClientDelete.Name = "buttonClientDelete";
            this.buttonClientDelete.Size = new System.Drawing.Size(75, 23);
            this.buttonClientDelete.TabIndex = 15;
            this.buttonClientDelete.Text = "Effacer";
            this.buttonClientDelete.UseVisualStyleBackColor = true;
            this.buttonClientDelete.Click += new System.EventHandler(this.buttonClientDelete_Click);
            // 
            // buttonClientModify
            // 
            this.buttonClientModify.Location = new System.Drawing.Point(317, 429);
            this.buttonClientModify.Name = "buttonClientModify";
            this.buttonClientModify.Size = new System.Drawing.Size(75, 23);
            this.buttonClientModify.TabIndex = 14;
            this.buttonClientModify.Text = "Modifier";
            this.buttonClientModify.UseVisualStyleBackColor = true;
            this.buttonClientModify.Click += new System.EventHandler(this.buttonClientModify_Click);
            // 
            // buttonClientNew
            // 
            this.buttonClientNew.Location = new System.Drawing.Point(317, 393);
            this.buttonClientNew.Name = "buttonClientNew";
            this.buttonClientNew.Size = new System.Drawing.Size(75, 23);
            this.buttonClientNew.TabIndex = 12;
            this.buttonClientNew.Text = "Nouveau";
            this.buttonClientNew.UseVisualStyleBackColor = true;
            this.buttonClientNew.Click += new System.EventHandler(this.buttonClientNew_Click);
            // 
            // listBoxClients
            // 
            this.listBoxClients.FormattingEnabled = true;
            this.listBoxClients.Location = new System.Drawing.Point(165, 393);
            this.listBoxClients.Name = "listBoxClients";
            this.listBoxClients.Size = new System.Drawing.Size(146, 95);
            this.listBoxClients.TabIndex = 13;
            // 
            // buttonFiletypesDelete
            // 
            this.buttonFiletypesDelete.Location = new System.Drawing.Point(316, 578);
            this.buttonFiletypesDelete.Name = "buttonFiletypesDelete";
            this.buttonFiletypesDelete.Size = new System.Drawing.Size(75, 23);
            this.buttonFiletypesDelete.TabIndex = 19;
            this.buttonFiletypesDelete.Text = "Effacer";
            this.buttonFiletypesDelete.UseVisualStyleBackColor = true;
            this.buttonFiletypesDelete.Click += new System.EventHandler(this.buttonFiletypesDelete_Click);
            // 
            // buttonFiletypesModify
            // 
            this.buttonFiletypesModify.Location = new System.Drawing.Point(316, 542);
            this.buttonFiletypesModify.Name = "buttonFiletypesModify";
            this.buttonFiletypesModify.Size = new System.Drawing.Size(75, 23);
            this.buttonFiletypesModify.TabIndex = 18;
            this.buttonFiletypesModify.Text = "Modifier";
            this.buttonFiletypesModify.UseVisualStyleBackColor = true;
            this.buttonFiletypesModify.Click += new System.EventHandler(this.buttonFiletypesModify_Click);
            // 
            // buttonFiletypesNew
            // 
            this.buttonFiletypesNew.Location = new System.Drawing.Point(316, 506);
            this.buttonFiletypesNew.Name = "buttonFiletypesNew";
            this.buttonFiletypesNew.Size = new System.Drawing.Size(75, 23);
            this.buttonFiletypesNew.TabIndex = 16;
            this.buttonFiletypesNew.Text = "Nouveau";
            this.buttonFiletypesNew.UseVisualStyleBackColor = true;
            this.buttonFiletypesNew.Click += new System.EventHandler(this.buttonFiletypesNew_Click);
            // 
            // listBoxFiletypes
            // 
            this.listBoxFiletypes.FormattingEnabled = true;
            this.listBoxFiletypes.Location = new System.Drawing.Point(164, 506);
            this.listBoxFiletypes.Name = "listBoxFiletypes";
            this.listBoxFiletypes.Size = new System.Drawing.Size(146, 95);
            this.listBoxFiletypes.TabIndex = 17;
            // 
            // buttonExtrasDelete
            // 
            this.buttonExtrasDelete.Location = new System.Drawing.Point(614, 578);
            this.buttonExtrasDelete.Name = "buttonExtrasDelete";
            this.buttonExtrasDelete.Size = new System.Drawing.Size(75, 23);
            this.buttonExtrasDelete.TabIndex = 23;
            this.buttonExtrasDelete.Text = "Effacer";
            this.buttonExtrasDelete.UseVisualStyleBackColor = true;
            this.buttonExtrasDelete.Click += new System.EventHandler(this.buttonExtrasDelete_Click);
            // 
            // buttonExtrasModify
            // 
            this.buttonExtrasModify.Location = new System.Drawing.Point(614, 542);
            this.buttonExtrasModify.Name = "buttonExtrasModify";
            this.buttonExtrasModify.Size = new System.Drawing.Size(75, 23);
            this.buttonExtrasModify.TabIndex = 22;
            this.buttonExtrasModify.Text = "Modifier";
            this.buttonExtrasModify.UseVisualStyleBackColor = true;
            this.buttonExtrasModify.Click += new System.EventHandler(this.buttonExtrasModify_Click);
            // 
            // buttonExtrasNew
            // 
            this.buttonExtrasNew.Location = new System.Drawing.Point(614, 506);
            this.buttonExtrasNew.Name = "buttonExtrasNew";
            this.buttonExtrasNew.Size = new System.Drawing.Size(75, 23);
            this.buttonExtrasNew.TabIndex = 20;
            this.buttonExtrasNew.Text = "Nouveau";
            this.buttonExtrasNew.UseVisualStyleBackColor = true;
            this.buttonExtrasNew.Click += new System.EventHandler(this.buttonExtrasNew_Click);
            // 
            // listBoxExtras
            // 
            this.listBoxExtras.FormattingEnabled = true;
            this.listBoxExtras.Location = new System.Drawing.Point(462, 506);
            this.listBoxExtras.Name = "listBoxExtras";
            this.listBoxExtras.Size = new System.Drawing.Size(146, 95);
            this.listBoxExtras.TabIndex = 21;
            // 
            // checkBoxWriteSchedulineToNote
            // 
            this.checkBoxWriteSchedulineToNote.AutoSize = true;
            this.checkBoxWriteSchedulineToNote.Location = new System.Drawing.Point(50, 615);
            this.checkBoxWriteSchedulineToNote.Name = "checkBoxWriteSchedulineToNote";
            this.checkBoxWriteSchedulineToNote.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBoxWriteSchedulineToNote.Size = new System.Drawing.Size(129, 17);
            this.checkBoxWriteSchedulineToNote.TabIndex = 24;
            this.checkBoxWriteSchedulineToNote.Text = "ScheduleLine -> Note";
            this.checkBoxWriteSchedulineToNote.UseVisualStyleBackColor = true;
            this.checkBoxWriteSchedulineToNote.CheckStateChanged += new System.EventHandler(this.checkBoxWriteSchedulineToNote_CheckStateChanged);
            // 
            // labelUsername
            // 
            this.labelUsername.AutoSize = true;
            this.labelUsername.Location = new System.Drawing.Point(50, 89);
            this.labelUsername.Name = "labelUsername";
            this.labelUsername.Size = new System.Drawing.Size(72, 13);
            this.labelUsername.TabIndex = 29;
            this.labelUsername.Text = "Nom d\'usager";
            // 
            // labelWorkstation
            // 
            this.labelWorkstation.AutoSize = true;
            this.labelWorkstation.Location = new System.Drawing.Point(316, 89);
            this.labelWorkstation.Name = "labelWorkstation";
            this.labelWorkstation.Size = new System.Drawing.Size(78, 13);
            this.labelWorkstation.TabIndex = 30;
            this.labelWorkstation.Text = "Nom de station";
            // 
            // labelEngine
            // 
            this.labelEngine.AutoSize = true;
            this.labelEngine.Location = new System.Drawing.Point(50, 118);
            this.labelEngine.Name = "labelEngine";
            this.labelEngine.Size = new System.Drawing.Size(34, 13);
            this.labelEngine.TabIndex = 31;
            this.labelEngine.Text = "Engin";
            // 
            // labelSoCreationStatus
            // 
            this.labelSoCreationStatus.AutoSize = true;
            this.labelSoCreationStatus.Location = new System.Drawing.Point(242, 182);
            this.labelSoCreationStatus.Name = "labelSoCreationStatus";
            this.labelSoCreationStatus.Size = new System.Drawing.Size(179, 13);
            this.labelSoCreationStatus.TabIndex = 33;
            this.labelSoCreationStatus.Text = "Status de création des Bon d\'Achats";
            // 
            // labelExecutionType
            // 
            this.labelExecutionType.AutoSize = true;
            this.labelExecutionType.Location = new System.Drawing.Point(50, 209);
            this.labelExecutionType.Name = "labelExecutionType";
            this.labelExecutionType.Size = new System.Drawing.Size(88, 13);
            this.labelExecutionType.TabIndex = 34;
            this.labelExecutionType.Text = "Type d\'exécution";
            // 
            // groupBoxExecutionType
            // 
            this.groupBoxExecutionType.Controls.Add(this.radioButtonExecutionComplex);
            this.groupBoxExecutionType.Controls.Add(this.radioButtonExecutionDouble);
            this.groupBoxExecutionType.Controls.Add(this.radioButtonExecutionSimple);
            this.groupBoxExecutionType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBoxExecutionType.Location = new System.Drawing.Point(167, 209);
            this.groupBoxExecutionType.Name = "groupBoxExecutionType";
            this.groupBoxExecutionType.Size = new System.Drawing.Size(315, 54);
            this.groupBoxExecutionType.TabIndex = 7;
            this.groupBoxExecutionType.TabStop = false;
            // 
            // radioButtonExecutionComplex
            // 
            this.radioButtonExecutionComplex.AutoSize = true;
            this.radioButtonExecutionComplex.Location = new System.Drawing.Point(221, 20);
            this.radioButtonExecutionComplex.Name = "radioButtonExecutionComplex";
            this.radioButtonExecutionComplex.Size = new System.Drawing.Size(71, 17);
            this.radioButtonExecutionComplex.TabIndex = 10;
            this.radioButtonExecutionComplex.TabStop = true;
            this.radioButtonExecutionComplex.Text = "Complexe";
            this.radioButtonExecutionComplex.UseVisualStyleBackColor = true;
            this.radioButtonExecutionComplex.CheckedChanged += new System.EventHandler(this.radioButtonExecutionComplex_CheckedChanged);
            // 
            // radioButtonExecutionDouble
            // 
            this.radioButtonExecutionDouble.AutoSize = true;
            this.radioButtonExecutionDouble.Location = new System.Drawing.Point(136, 20);
            this.radioButtonExecutionDouble.Name = "radioButtonExecutionDouble";
            this.radioButtonExecutionDouble.Size = new System.Drawing.Size(59, 17);
            this.radioButtonExecutionDouble.TabIndex = 9;
            this.radioButtonExecutionDouble.TabStop = true;
            this.radioButtonExecutionDouble.Text = "Double";
            this.radioButtonExecutionDouble.UseVisualStyleBackColor = true;
            this.radioButtonExecutionDouble.CheckedChanged += new System.EventHandler(this.radioButtonExecutionDouble_CheckedChanged);
            // 
            // radioButtonExecutionSimple
            // 
            this.radioButtonExecutionSimple.AutoSize = true;
            this.radioButtonExecutionSimple.Location = new System.Drawing.Point(23, 20);
            this.radioButtonExecutionSimple.Name = "radioButtonExecutionSimple";
            this.radioButtonExecutionSimple.Size = new System.Drawing.Size(95, 17);
            this.radioButtonExecutionSimple.TabIndex = 0;
            this.radioButtonExecutionSimple.TabStop = true;
            this.radioButtonExecutionSimple.Text = "Simple/Unique";
            this.radioButtonExecutionSimple.UseVisualStyleBackColor = true;
            this.radioButtonExecutionSimple.CheckedChanged += new System.EventHandler(this.radioButtonExecutionSimple_CheckedChanged);
            // 
            // labelTemplates
            // 
            this.labelTemplates.AutoSize = true;
            this.labelTemplates.Location = new System.Drawing.Point(50, 277);
            this.labelTemplates.Name = "labelTemplates";
            this.labelTemplates.Size = new System.Drawing.Size(52, 13);
            this.labelTemplates.TabIndex = 36;
            this.labelTemplates.Text = "Gabarit(s)";
            // 
            // labelClients
            // 
            this.labelClients.AutoSize = true;
            this.labelClients.Location = new System.Drawing.Point(50, 393);
            this.labelClients.Name = "labelClients";
            this.labelClients.Size = new System.Drawing.Size(44, 13);
            this.labelClients.TabIndex = 37;
            this.labelClients.Text = "Client(s)";
            // 
            // labelFiletypes
            // 
            this.labelFiletypes.AutoSize = true;
            this.labelFiletypes.Location = new System.Drawing.Point(50, 506);
            this.labelFiletypes.Name = "labelFiletypes";
            this.labelFiletypes.Size = new System.Drawing.Size(82, 13);
            this.labelFiletypes.TabIndex = 38;
            this.labelFiletypes.Text = "Types de fichier";
            // 
            // labelExtras
            // 
            this.labelExtras.AutoSize = true;
            this.labelExtras.Location = new System.Drawing.Point(417, 506);
            this.labelExtras.Name = "labelExtras";
            this.labelExtras.Size = new System.Drawing.Size(36, 13);
            this.labelExtras.TabIndex = 39;
            this.labelExtras.Text = "Extras";
            // 
            // buttonSave
            // 
            this.buttonSave.Enabled = false;
            this.buttonSave.Location = new System.Drawing.Point(12, 640);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(703, 52);
            this.buttonSave.TabIndex = 25;
            this.buttonSave.Text = "Sauvegarder";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonOk
            // 
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(12, 704);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(264, 52);
            this.buttonOk.TabIndex = 26;
            this.buttonOk.Text = "Ok";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(451, 704);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(264, 52);
            this.buttonCancel.TabIndex = 27;
            this.buttonCancel.Text = "Canceler";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // textBoxClientInput
            // 
            this.textBoxClientInput.AcceptsReturn = true;
            this.textBoxClientInput.Location = new System.Drawing.Point(165, 468);
            this.textBoxClientInput.Name = "textBoxClientInput";
            this.textBoxClientInput.Size = new System.Drawing.Size(146, 20);
            this.textBoxClientInput.TabIndex = 43;
            this.textBoxClientInput.TabStop = false;
            this.textBoxClientInput.Visible = false;
            this.textBoxClientInput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxClientNew_KeyUp);
            // 
            // textBoxFiletypesInput
            // 
            this.textBoxFiletypesInput.AcceptsReturn = true;
            this.textBoxFiletypesInput.Location = new System.Drawing.Point(164, 581);
            this.textBoxFiletypesInput.Name = "textBoxFiletypesInput";
            this.textBoxFiletypesInput.Size = new System.Drawing.Size(146, 20);
            this.textBoxFiletypesInput.TabIndex = 44;
            this.textBoxFiletypesInput.TabStop = false;
            this.textBoxFiletypesInput.Visible = false;
            this.textBoxFiletypesInput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxFiletypesInput_KeyUp);
            // 
            // comboBoxJbDbName
            // 
            this.comboBoxJbDbName.FormattingEnabled = true;
            this.comboBoxJbDbName.Location = new System.Drawing.Point(164, 147);
            this.comboBoxJbDbName.Name = "comboBoxJbDbName";
            this.comboBoxJbDbName.Size = new System.Drawing.Size(121, 21);
            this.comboBoxJbDbName.TabIndex = 3;
            this.comboBoxJbDbName.SelectedIndexChanged += new System.EventHandler(this.comboBoxJbDbName_SelectedIndexChanged);
            // 
            // textBoxJbDbServer
            // 
            this.textBoxJbDbServer.Location = new System.Drawing.Point(417, 147);
            this.textBoxJbDbServer.Name = "textBoxJbDbServer";
            this.textBoxJbDbServer.Size = new System.Drawing.Size(272, 20);
            this.textBoxJbDbServer.TabIndex = 4;
            this.textBoxJbDbServer.TextChanged += new System.EventHandler(this.textBoxJbDbServer_TextChanged);
            // 
            // labelJbDbName
            // 
            this.labelJbDbName.AutoSize = true;
            this.labelJbDbName.Location = new System.Drawing.Point(50, 151);
            this.labelJbDbName.Name = "labelJbDbName";
            this.labelJbDbName.Size = new System.Drawing.Size(86, 13);
            this.labelJbDbName.TabIndex = 47;
            this.labelJbDbName.Text = "BD de JobBOSS";
            // 
            // labelJbDbServer
            // 
            this.labelJbDbServer.AutoSize = true;
            this.labelJbDbServer.Location = new System.Drawing.Point(316, 151);
            this.labelJbDbServer.Name = "labelJbDbServer";
            this.labelJbDbServer.Size = new System.Drawing.Size(62, 13);
            this.labelJbDbServer.TabIndex = 48;
            this.labelJbDbServer.Text = "Server SQL";
            // 
            // textBoxExtrasInput
            // 
            this.textBoxExtrasInput.AcceptsReturn = true;
            this.textBoxExtrasInput.Location = new System.Drawing.Point(462, 581);
            this.textBoxExtrasInput.Name = "textBoxExtrasInput";
            this.textBoxExtrasInput.Size = new System.Drawing.Size(146, 20);
            this.textBoxExtrasInput.TabIndex = 49;
            this.textBoxExtrasInput.TabStop = false;
            this.textBoxExtrasInput.Visible = false;
            this.textBoxExtrasInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxExtrasInput_KeyDown);
            this.textBoxExtrasInput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxExtrasInput_KeyUp);
            // 
            // buttonConfNavPrev
            // 
            this.buttonConfNavPrev.Location = new System.Drawing.Point(86, 21);
            this.buttonConfNavPrev.Name = "buttonConfNavPrev";
            this.buttonConfNavPrev.Size = new System.Drawing.Size(75, 23);
            this.buttonConfNavPrev.TabIndex = 29;
            this.buttonConfNavPrev.Text = "Précédent";
            this.buttonConfNavPrev.UseVisualStyleBackColor = true;
            this.buttonConfNavPrev.Click += new System.EventHandler(this.buttonConfNavPrev_Click);
            // 
            // buttonConfNavNext
            // 
            this.buttonConfNavNext.Location = new System.Drawing.Point(586, 20);
            this.buttonConfNavNext.Name = "buttonConfNavNext";
            this.buttonConfNavNext.Size = new System.Drawing.Size(75, 23);
            this.buttonConfNavNext.TabIndex = 30;
            this.buttonConfNavNext.Text = "Suivant";
            this.buttonConfNavNext.UseVisualStyleBackColor = true;
            this.buttonConfNavNext.Click += new System.EventHandler(this.buttonConfNavNext_Click);
            // 
            // textBoxConfigName
            // 
            this.textBoxConfigName.ForeColor = System.Drawing.SystemColors.InactiveCaption;
            this.textBoxConfigName.Location = new System.Drawing.Point(167, 22);
            this.textBoxConfigName.Name = "textBoxConfigName";
            this.textBoxConfigName.Size = new System.Drawing.Size(413, 20);
            this.textBoxConfigName.TabIndex = 0;
            this.textBoxConfigName.Text = "Nom de la configuration";
            this.textBoxConfigName.TextChanged += new System.EventHandler(this.textBoxConfigName_TextChanged);
            this.textBoxConfigName.Enter += new System.EventHandler(this.textBoxConfigName_Enter);
            this.textBoxConfigName.Leave += new System.EventHandler(this.textBoxConfigName_Leave);
            // 
            // buttonConfAddNew
            // 
            this.buttonConfAddNew.Location = new System.Drawing.Point(32, 21);
            this.buttonConfAddNew.Name = "buttonConfAddNew";
            this.buttonConfAddNew.Size = new System.Drawing.Size(52, 23);
            this.buttonConfAddNew.TabIndex = 28;
            this.buttonConfAddNew.Text = "Ajouter";
            this.buttonConfAddNew.UseVisualStyleBackColor = true;
            this.buttonConfAddNew.Click += new System.EventHandler(this.buttonConfAddNew_Click);
            // 
            // buttonConfDelete
            // 
            this.buttonConfDelete.Location = new System.Drawing.Point(663, 20);
            this.buttonConfDelete.Name = "buttonConfDelete";
            this.buttonConfDelete.Size = new System.Drawing.Size(52, 23);
            this.buttonConfDelete.TabIndex = 31;
            this.buttonConfDelete.Text = "Supp";
            this.buttonConfDelete.UseVisualStyleBackColor = true;
            this.buttonConfDelete.Click += new System.EventHandler(this.buttonConfDelete_Click);
            // 
            // checkBoxConfActive
            // 
            this.checkBoxConfActive.AutoSize = true;
            this.checkBoxConfActive.Location = new System.Drawing.Point(460, 48);
            this.checkBoxConfActive.Name = "checkBoxConfActive";
            this.checkBoxConfActive.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBoxConfActive.Size = new System.Drawing.Size(120, 17);
            this.checkBoxConfActive.TabIndex = 1;
            this.checkBoxConfActive.Text = "Configuration active";
            this.checkBoxConfActive.UseVisualStyleBackColor = true;
            this.checkBoxConfActive.CheckedChanged += new System.EventHandler(this.checkBoxConfActive_CheckedChanged);
            // 
            // labelConfId
            // 
            this.labelConfId.AutoSize = true;
            this.labelConfId.Location = new System.Drawing.Point(136, 56);
            this.labelConfId.Name = "labelConfId";
            this.labelConfId.Size = new System.Drawing.Size(35, 13);
            this.labelConfId.TabIndex = 56;
            this.labelConfId.Text = "label1";
            this.labelConfId.Visible = false;
            // 
            // checkBoxUseShippedSoDetailLines
            // 
            this.checkBoxUseShippedSoDetailLines.AutoSize = true;
            this.checkBoxUseShippedSoDetailLines.Location = new System.Drawing.Point(517, 230);
            this.checkBoxUseShippedSoDetailLines.Name = "checkBoxUseShippedSoDetailLines";
            this.checkBoxUseShippedSoDetailLines.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.checkBoxUseShippedSoDetailLines.Size = new System.Drawing.Size(133, 17);
            this.checkBoxUseShippedSoDetailLines.TabIndex = 57;
            this.checkBoxUseShippedSoDetailLines.Text = "Utiliser lignes \'Shipped\'";
            this.checkBoxUseShippedSoDetailLines.UseVisualStyleBackColor = true;
            this.checkBoxUseShippedSoDetailLines.CheckStateChanged += new System.EventHandler(this.checkBoxUseShippedSoDetailLines_CheckStateChanged);
            // 
            // FormConfigs
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(727, 768);
            this.Controls.Add(this.checkBoxUseShippedSoDetailLines);
            this.Controls.Add(this.labelConfId);
            this.Controls.Add(this.checkBoxConfActive);
            this.Controls.Add(this.buttonConfDelete);
            this.Controls.Add(this.buttonConfAddNew);
            this.Controls.Add(this.textBoxConfigName);
            this.Controls.Add(this.buttonConfNavNext);
            this.Controls.Add(this.buttonConfNavPrev);
            this.Controls.Add(this.textBoxExtrasInput);
            this.Controls.Add(this.labelJbDbServer);
            this.Controls.Add(this.labelJbDbName);
            this.Controls.Add(this.textBoxJbDbServer);
            this.Controls.Add(this.comboBoxJbDbName);
            this.Controls.Add(this.textBoxFiletypesInput);
            this.Controls.Add(this.textBoxClientInput);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.labelExtras);
            this.Controls.Add(this.labelFiletypes);
            this.Controls.Add(this.labelClients);
            this.Controls.Add(this.labelTemplates);
            this.Controls.Add(this.groupBoxExecutionType);
            this.Controls.Add(this.labelExecutionType);
            this.Controls.Add(this.labelSoCreationStatus);
            this.Controls.Add(this.labelEngine);
            this.Controls.Add(this.labelWorkstation);
            this.Controls.Add(this.labelUsername);
            this.Controls.Add(this.checkBoxWriteSchedulineToNote);
            this.Controls.Add(this.buttonExtrasDelete);
            this.Controls.Add(this.buttonExtrasModify);
            this.Controls.Add(this.buttonExtrasNew);
            this.Controls.Add(this.listBoxExtras);
            this.Controls.Add(this.buttonFiletypesDelete);
            this.Controls.Add(this.buttonFiletypesModify);
            this.Controls.Add(this.buttonFiletypesNew);
            this.Controls.Add(this.listBoxFiletypes);
            this.Controls.Add(this.buttonClientDelete);
            this.Controls.Add(this.buttonClientModify);
            this.Controls.Add(this.buttonClientNew);
            this.Controls.Add(this.listBoxClients);
            this.Controls.Add(this.buttonTemplateDelete);
            this.Controls.Add(this.buttonTemplateModify);
            this.Controls.Add(this.buttonTemplateNew);
            this.Controls.Add(this.listBoxTemplates);
            this.Controls.Add(this.comboBoxSoCreationStatus);
            this.Controls.Add(this.checkBoxSoCreation);
            this.Controls.Add(this.comboBoxEngine);
            this.Controls.Add(this.textBoxWorkstation);
            this.Controls.Add(this.textBoxUserName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FormConfigs";
            this.ShowIcon = false;
            this.Text = "Paramètres";
            this.groupBoxExecutionType.ResumeLayout(false);
            this.groupBoxExecutionType.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxUserName;
        private System.Windows.Forms.TextBox textBoxWorkstation;
        private System.Windows.Forms.ComboBox comboBoxEngine;
        private System.Windows.Forms.CheckBox checkBoxSoCreation;
        private System.Windows.Forms.ComboBox comboBoxSoCreationStatus;
        private System.Windows.Forms.ListBox listBoxTemplates;
        private System.Windows.Forms.Button buttonTemplateNew;
        private System.Windows.Forms.Button buttonTemplateModify;
        private System.Windows.Forms.Button buttonTemplateDelete;
        private System.Windows.Forms.Button buttonClientDelete;
        private System.Windows.Forms.Button buttonClientModify;
        private System.Windows.Forms.Button buttonClientNew;
        private System.Windows.Forms.ListBox listBoxClients;
        private System.Windows.Forms.Button buttonFiletypesDelete;
        private System.Windows.Forms.Button buttonFiletypesModify;
        private System.Windows.Forms.Button buttonFiletypesNew;
        private System.Windows.Forms.ListBox listBoxFiletypes;
        private System.Windows.Forms.Button buttonExtrasDelete;
        private System.Windows.Forms.Button buttonExtrasModify;
        private System.Windows.Forms.Button buttonExtrasNew;
        private System.Windows.Forms.ListBox listBoxExtras;
        private System.Windows.Forms.CheckBox checkBoxWriteSchedulineToNote;
        private System.Windows.Forms.Label labelUsername;
        private System.Windows.Forms.Label labelWorkstation;
        private System.Windows.Forms.Label labelEngine;
        private System.Windows.Forms.Label labelSoCreationStatus;
        private System.Windows.Forms.Label labelExecutionType;
        private System.Windows.Forms.GroupBox groupBoxExecutionType;
        private System.Windows.Forms.RadioButton radioButtonExecutionComplex;
        private System.Windows.Forms.RadioButton radioButtonExecutionDouble;
        private System.Windows.Forms.RadioButton radioButtonExecutionSimple;
        private System.Windows.Forms.Label labelTemplates;
        private System.Windows.Forms.Label labelClients;
        private System.Windows.Forms.Label labelFiletypes;
        private System.Windows.Forms.Label labelExtras;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox textBoxClientInput;
        private System.Windows.Forms.TextBox textBoxFiletypesInput;
        private System.Windows.Forms.ComboBox comboBoxJbDbName;
        private System.Windows.Forms.TextBox textBoxJbDbServer;
        private System.Windows.Forms.Label labelJbDbName;
        private System.Windows.Forms.Label labelJbDbServer;
        private System.Windows.Forms.TextBox textBoxExtrasInput;
        private System.Windows.Forms.Button buttonConfNavPrev;
        private System.Windows.Forms.Button buttonConfNavNext;
        private System.Windows.Forms.TextBox textBoxConfigName;
        private System.Windows.Forms.Button buttonConfAddNew;
        private System.Windows.Forms.Button buttonConfDelete;
        private System.Windows.Forms.CheckBox checkBoxConfActive;
        private System.Windows.Forms.Label labelConfId;
        private System.Windows.Forms.CheckBox checkBoxUseShippedSoDetailLines;
    }
}