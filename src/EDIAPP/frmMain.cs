using MfgConnection;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using EDI;

namespace EDIAPP
{
    public delegate void ReadThreadEnd(ref DataGridView dgv, ref DataTable dt);
    public delegate void ReadThreadInit(int a, string b);
    public delegate void ReadThreadReport(int a);


    public partial class frmMain : Form
    {
        private delegate void PassABindingSource(in BindingSource bindingSource);
        private delegate void PassAString(in string message);
        private delegate void ControlPropertyModifyCallback();

        private string M_CUSTOMER_ID;
        private string M_DBNAME;
        private string M_DBSERVER;
        private string M_SO_CREATION;
        private string M_SO_CREATION_STATUS;
        private string M_SO_CREATION_STATUS2;
        private string M_EXECUTION;
        private string M_ENGINE;
        private string M_WRITE_SCHEDULE_LINE_TO_NOTE;

        //FORECAST, DELIVERY, MERGED RELATED
        private EDI.EDI_Engine_Base engine_for_app;
        private string m_sDeliveryFilePath;
        private string m_sForecastFilePath;
        private string m_sTemplateFilePathDelivery;
        private string m_sTemplateFilePathForecast;
        private MFG.MFG_File_Template m_DeliveryFileTemplate;
        private MFG.MFG_File_Template m_ForecastFileTemplate;
        private MFG.Files.CSV.CCsvReader m_DeliveryFileReader;
        private MFG.Files.XLS.XlsReader m_DeliveryXlsFileReader;
        private MFG.Files.CSV.CCsvReader m_ForecastFileReader;
        private DataTable m_MergedTable;

        private Image m_BookmarkIcon;

        private string[] m_sCustomers;
        private string[] m_sFileTypes;
        private string[] m_sTemplates;
        private string[] filesForRead;

        private bool m_IsReady;
        private bool m_bCombineFiles;
        private bool bOneTemplateExecution;

        private object[] m_QuantityRefusals;
        private object[] m_DateRefusals;

        private bool useShippedLines;


        private DataGridViewCellStyle m_OddRowDefaultStyle;
        private DataGridViewCellStyle m_EvenRowDefaultStyle;
        private DataGridViewCellStyle m_refusalStyle;
        private DataGridViewCellStyle m_bookmarkedStyle;

        private int m_BookmarkedLine;

        private string[] engineExtraFeatures;
        private string engine850file;
        private string engine860file;

        private System.Timers.Timer timerForEightFiftyStandbys;
        private System.Timers.Timer timerForEightSixtyStandbys;

        public frmMain()
        {
            InitializeComponent();

            SetCulture();

            Init();

            NbRowsLabel.Text = "0 lignes";
        }

        private void SetCulture()
        {
            var culture = CultureInfo.CreateSpecificCulture("en-CA");
            culture.NumberFormat.CurrencyDecimalSeparator = ".";
            culture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        private void AddToHistory(string[] arrValues)
        {
            var sDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/EDI-History/";
            var sFileNameStart = "Rapport-" + DateTime.Now.ToShortDateString().Replace("/", "-");
            var sFileName = "";

            if (!Directory.Exists(sDirPath))
            {
                Directory.CreateDirectory(sDirPath);
            }

            sFileName = "History.csv";
            var dumpWriter = new MFG.Files.CSV.CCsvWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EDI-History\\", sFileName);

            /*
            Date of the Write

            Pull Qty,
            Push Qty,
            Diff Qty,

            Pull Qty Value,
            Push Qty Vallue,
            Diff Qty Value,

            Pull Days,
            Push Days,
            Diff Days,

            Pull Days Value,
            Push Days Value,
            Diff Days Value
            */

            var arrValuesToWrite = new string[13] { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
            var dtToday = DateTime.Now;
            arrValuesToWrite[0] = dtToday.Year + "/" + dtToday.Month + "/" + dtToday.Day;

            if (Convert.ToInt32(arrValues[0]) > 0) //This is a quantity pull
            {
                arrValuesToWrite[1] = arrValues[0];
                arrValuesToWrite[3] = arrValues[0];

                arrValuesToWrite[4] = (Convert.ToInt32(arrValues[0]) * Convert.ToDouble(arrValues[2])).ToString();
                arrValuesToWrite[6] = (Convert.ToInt32(arrValues[0]) * Convert.ToDouble(arrValues[2])).ToString();
            }
            else if (Convert.ToInt32(arrValues[0]) < 0) //This is a quantity push
            {
                arrValuesToWrite[2] = (Convert.ToInt32(arrValues[0]) * -1).ToString();
                arrValuesToWrite[3] = arrValues[0];

                arrValuesToWrite[5] = (Convert.ToInt32(arrValues[0]) * -1 * Convert.ToDouble(arrValues[2])).ToString();
                arrValuesToWrite[6] = (Convert.ToInt32(arrValues[0]) * Convert.ToDouble(arrValues[2])).ToString();
            }

            if (Convert.ToInt32(arrValues[1]) < 0) //This is a date pull
            {
                arrValuesToWrite[7] = (Convert.ToInt32(arrValues[1]) * -1).ToString(); ;
                arrValuesToWrite[9] = (Convert.ToInt32(arrValues[1]) * -1).ToString(); ;

                arrValuesToWrite[10] = (Convert.ToInt32(arrValues[3]) * Convert.ToDouble(arrValues[2])).ToString();
                arrValuesToWrite[12] = (Convert.ToInt32(arrValues[3]) * Convert.ToDouble(arrValues[2])).ToString(); ;
            }
            else if (Convert.ToInt32(arrValues[1]) > 0) //This is a date push
            {
                arrValuesToWrite[8] = arrValues[1];
                arrValuesToWrite[9] = arrValues[1];

                arrValuesToWrite[11] = (Convert.ToInt32(arrValues[3]) * Convert.ToDouble(arrValues[2])).ToString();
                arrValuesToWrite[12] = (Convert.ToInt32(arrValues[3]) * -1 * Convert.ToDouble(arrValues[2])).ToString();
            }

            dumpWriter.AddToFile(arrValuesToWrite);
        }

        private void ApplyGridColoring()
        {
            foreach (DataGridViewRow dgvr in dataGridViewMain.Rows)
            {
                if (dgvr.Index == m_BookmarkedLine)
                {
                    dgvr.DefaultCellStyle = m_bookmarkedStyle;
                }
                else
                {
                    if (dgvr.Index % 2 == 0)
                    {
                        //Even
                        dgvr.DefaultCellStyle = m_EvenRowDefaultStyle;
                    }
                    else
                    {
                        //Odd
                        dgvr.DefaultCellStyle = m_OddRowDefaultStyle;
                    }
                }


                if (dgvr.Cells[5].Value != DBNull.Value)
                {
                    if (Convert.ToInt32(dgvr.Cells[5].Value) > 0)
                    {
                        dgvr.Cells[5].Style.ForeColor = Color.Red;
                    }
                    else if (Convert.ToInt32(dgvr.Cells[5].Value) < 0)
                    {
                        dgvr.Cells[5].Style.ForeColor = Color.Green;
                    }
                }

                if (dgvr.Cells[7].Value != DBNull.Value)
                {
                    if (Convert.ToInt32(dgvr.Cells[7].Value) > 0)
                    {
                        dgvr.Cells[7].Style.ForeColor = Color.Green;
                    }
                    else if (Convert.ToInt32(dgvr.Cells[7].Value) < 0)
                    {
                        dgvr.Cells[7].Style.ForeColor = Color.Red;
                    }
                }

                if (dgvr.Cells[10].Value != DBNull.Value)
                {
                    if (Convert.ToInt32(dgvr.Cells[10].Value) > 0)
                    {
                        dgvr.Cells[10].Style.ForeColor = Color.Green;
                    }
                    else if (Convert.ToInt32(dgvr.Cells[10].Value) < 0)
                    {
                        dgvr.Cells[10].Style.ForeColor = Color.Red;
                    }
                }

                if (dgvr.Cells[12].Value != DBNull.Value)
                {
                    if (Convert.ToInt32(dgvr.Cells[12].Value) > 0)
                    {
                        dgvr.Cells[12].Style.ForeColor = Color.Green;
                    }
                    else if (Convert.ToInt32(dgvr.Cells[12].Value) < 0)
                    {
                        dgvr.Cells[12].Style.ForeColor = Color.Red;
                    }
                }
            }
        }

        private void ApplySettings(in object[] settings)
        {
            M_ENGINE = settings[(int)EdiDbInterface.Settings.engine].ToString().ToUpper();
            M_DBNAME = settings[(int)EdiDbInterface.Settings.jbDbName].ToString().ToUpper();
            M_DBSERVER = settings[(int)EdiDbInterface.Settings.jbDbServer].ToString().ToUpper();
            M_SO_CREATION = settings[(int)EdiDbInterface.Settings.soCreation].ToString();
            M_SO_CREATION_STATUS = settings[(int)EdiDbInterface.Settings.soCreationStatus].ToString();
            M_SO_CREATION_STATUS2 = settings[(int)EdiDbInterface.Settings.soCreationStatus2].ToString();
            M_EXECUTION = settings[(int)EdiDbInterface.Settings.executionType].ToString();
            M_WRITE_SCHEDULE_LINE_TO_NOTE = settings[(int)EdiDbInterface.Settings.writeScheduleLineToNote].ToString();

            m_sCustomers = settings[(int)EdiDbInterface.Settings.clients].ToString().Split(',');
            m_sFileTypes = settings[(int)EdiDbInterface.Settings.filetype].ToString().Split(',');
            m_sTemplates = settings[(int)EdiDbInterface.Settings.template].ToString().Split(';');
            engineExtraFeatures = settings[(int)EdiDbInterface.Settings.extras].ToString().Split(',');

            useShippedLines = (bool)settings[(int)EdiDbInterface.Settings.useShippedLines];

            m_sTemplateFilePathDelivery = m_sTemplates[0];

            if (M_EXECUTION == "simple")
            {
                m_sTemplateFilePathForecast = m_sTemplateFilePathDelivery;
                bOneTemplateExecution = true;
            }
            else if (M_EXECUTION == "double")
            {
                m_sTemplateFilePathForecast = m_sTemplates[1];
                bOneTemplateExecution = false;
            }
            else //complex
            {
                m_sTemplateFilePathForecast = m_sTemplates[1];
                bOneTemplateExecution = false;
            }
        }

        private bool AreBothFilesThere()
        {
            if (
              (this.m_sDeliveryFilePath != null &&
               (this.m_sForecastFilePath != null || this.bOneTemplateExecution)
               ) ||
              (this.filesForRead != null && this.m_bCombineFiles == true)
              )
            {
                return true;
            }

            return false;
        }

        private void Completed850Exec(in string message)
        {
            MessageBox.Show("850 - Création terminée");
            EnableAllTimers();
        }

        private void Completed855(in string message)
        {
            MessageBox.Show("855 - Rapport Terminer");
        }

        private void Completed860Exec(in string message)
        {
            MessageBox.Show("860 - Modification terminée");
            EnableAllTimers();
        }

        private void Completed865(in string message)
        {
            MessageBox.Show("865 - Rapport Terminer");
        }

        private void CreateDataGridCellStyles()
        {
            m_refusalStyle = new DataGridViewCellStyle();
            m_refusalStyle.BackColor = Color.Red;
            m_refusalStyle.ForeColor = Color.Black;

            m_EvenRowDefaultStyle = new DataGridViewCellStyle();

            m_OddRowDefaultStyle = new DataGridViewCellStyle();
            m_OddRowDefaultStyle.BackColor = Color.LightGray;

            m_bookmarkedStyle = new DataGridViewCellStyle();
            m_bookmarkedStyle.BackColor = Color.Cyan;
            m_bookmarkedStyle.ForeColor = Color.Black;
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                Point cell = ((DataGridView)sender).CurrentCellAddress;

                if (cell.X == 6 || cell.X == 11)
                {
                    if (dataGridViewMain.CurrentCell.RowIndex == m_BookmarkedLine)
                    {
                        dataGridViewMain.CurrentCell.Style = m_bookmarkedStyle;
                    }
                    else
                    {
                        if (dataGridViewMain.CurrentCell.RowIndex % 2 == 0)
                        {
                            //Even
                            dataGridViewMain.CurrentCell.Style = m_EvenRowDefaultStyle;
                        }
                        else
                        {
                            //Odd
                            dataGridViewMain.CurrentCell.Style = m_OddRowDefaultStyle;
                        }
                    }

                    if (M_ENGINE == "BOMBARDIER" && cell.X == 6)
                    {
                        Boolean isChecked = Convert.ToBoolean(
                          ((DataGridView)sender).Rows[cell.Y].Cells[cell.X].Value);

                        if (isChecked)
                            dataGridViewMain.CurrentCell.Style = m_refusalStyle;
                    }
                    else if (dataGridViewMain.CurrentCell.Value != null)
                    {
                        dataGridViewMain.CurrentCell.Style = m_refusalStyle;
                    }
                }
            }
            finally { }
        }

        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            Point cell = ((DataGridView)sender).CurrentCellAddress;

            if (((DataGridView)sender).Rows[cell.Y].Cells[cell.X].GetType() == typeof(DataGridViewCheckBoxCell))
            {
                ((DataGridView)sender).CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dataGridView1_Sorted(object sender, EventArgs e)
        {
            ApplyGridColoring();
        }

        private void DisableAllTimers()
        {
            DisableEightFiftyStandbyTimer();
            DisableEightSixtyStandbyTimer();
        }

        private void DisableEightFiftyStandbyTimer()
        {
            timerForEightFiftyStandbys.Enabled = false;
        }

        private void DisableEightFiftyFiveButton()
        {
            buttonExec855.Enabled = false;
        }

        private void DisableEightSixtyFiveButton()
        {
            buttonExec865.Enabled = false;
        }

        private void DisableEightSixtyStandbyTimer()
        {
            timerForEightSixtyStandbys.Enabled = false;

        }

        private void DisplayEightFiftyAnalyzeResults(in BindingSource bindingSource)
        {
            dataGridView_ExtraFeatures.DataSource = bindingSource;
            dataGridView_ExtraFeatures.Columns["ReleaseNumber"].Visible = false;
            dataGridView_ExtraFeatures.Columns["SO_HeaderInsert"].Visible = false;
            dataGridView_ExtraFeatures.Columns["SO_DetailInsert"].Visible = false;

            dataGridViewMain.Visible = false;
            buttonExec850.Enabled = true;
        }

        private void DisplayEightSixtyAnalyzeResults(in BindingSource bindingSource)
        {
            dataGridView_ExtraFeatures.DataSource = bindingSource;
            dataGridView_ExtraFeatures.Columns["ReleaseNumber"].Visible = false;
            dataGridView_ExtraFeatures.Columns["SO_HeaderInsert"].Visible = false;
            dataGridView_ExtraFeatures.Columns["SO_DetailInsert"].Visible = false;

            dataGridViewMain.Visible = false;
            buttonExec860.Enabled = true;
        }

        private void DisableExtraFeatures()
        {
            buttonOpen850File.Enabled = false;
            buttonOpen850File.Visible = false;
            buttonExec850.Enabled = false;
            buttonExec850.Visible = false;
            buttonExec855.Enabled = false;
            buttonExec855.Visible = false;
            buttonOpen860File.Enabled = false;
            buttonOpen860File.Visible = false;
            buttonExec860.Enabled = false;
            buttonExec860.Visible = false;
            buttonExec865.Enabled = false;
            buttonExec865.Visible = false;
        }

        private void DumpDataToTempFile()
        {
            var sDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/EDI-History/";
            var sFileNameStart = "Rapport-" + DateTime.Now.ToShortDateString().Replace("/", "-");
            var sFileName = "";
            if (!Directory.Exists(sDirPath))
            {
                Directory.CreateDirectory(sDirPath);
            }

            var bFoundFilename = false;
            var sFileNumber = "0001";

            while (!bFoundFilename)
            {
                var file = new FileInfo(sDirPath + sFileNameStart + "-" + sFileNumber + ".csv");

                if (file.Exists)
                {
                    sFileNumber = (Convert.ToUInt32(sFileNumber) + 1).ToString().PadLeft(4, '0');
                }
                else
                {
                    bFoundFilename = true;
                    sFileName = sFileNameStart + "-" + sFileNumber + ".csv";
                }

            }

            var dumpWriter = new MFG.Files.CSV.CCsvWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EDI-History\\", sFileName);
            string[] aCurrentRow;

            if (dataGridViewMain.Rows.Count > 0)
            {
                aCurrentRow = new string[dataGridViewMain.Rows[0].Cells.Count];
                foreach (DataGridViewColumn dgvc in this.dataGridViewMain.Columns)
                {
                    aCurrentRow[dgvc.Index] = dgvc.Name;
                }
                dumpWriter.AddToFile(aCurrentRow);

                foreach (DataGridViewRow dgvr in this.dataGridViewMain.Rows)
                {
                    for (int i = 0; i < dgvr.Cells.Count; i++)
                    {
                        if (dgvr.Cells[i].Value != null)
                        {
                            aCurrentRow[i] = dgvr.Cells[i].Value.ToString();
                        }
                        else
                        {
                            aCurrentRow[i] = "";
                        }
                    }
                    dumpWriter.AddToFile(aCurrentRow);
                }
            }
        }

        private void EnableEightFiftyFiveButton()
        {
            buttonExec855.Enabled = true;
        }

        private void EnableEightSixtyFiveButton()
        {
            buttonExec865.Enabled = true;
        }

        private void EnableAllTimers()
        {
            EnableEightFiftyStandbyTimer();
            EnableEightSixtyStandbyTimer();
        }

        private void EnableEightFiftyStandbyTimer()
        {
            timerForEightFiftyStandbys.Enabled = true;
        }

        private void EnableEightSixtyStandbyTimer()
        {
            timerForEightSixtyStandbys.Enabled = true;
        }

        private void EnableExtraFeatures(in string[] extras)
        {
            foreach (string item in extras)
            {
                if (item == "850")
                {
                    buttonOpen850File.Enabled = true;
                    buttonOpen850File.Visible = true;
                    buttonExec850.Enabled = false;
                    buttonExec850.Visible = true;
                }
                else if (item == "855")
                {
                    buttonExec855.Enabled = false;
                    buttonExec855.Visible = true;
                }
                else if (item == "860")
                {
                    buttonOpen860File.Enabled = true;
                    buttonOpen860File.Visible = true;
                    buttonExec860.Enabled = false;
                    buttonExec860.Visible = true;
                }
                else if (item == "865")
                {
                    buttonExec865.Enabled = false;
                    buttonExec865.Visible = true;
                }
            }
        }

        private void ExitApplication()
        {
            if (System.Windows.Forms.Application.MessageLoop)
            {
                // WinForms app
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                // Console app
                System.Environment.Exit(1);
            }
        }

        private void FillDataGridView(ref DataGridView dgv, ref DataTable dTable)
        {
            if (dgv.Rows.Count > 0)
            {
                dgv.Rows.Clear();
            }

            if (dTable.Rows.Count > 0)
            {
                foreach (DataRow dr in dTable.Rows)
                {
                    DataRow ddr = dTable.NewRow();
                    for (int i = 0; i < dTable.Columns.Count; i++)
                    {
                        if (i == 8 || i == 9)
                        {
                            if (dr[i] != DBNull.Value)
                            {
                                ddr[i] = ((DateTime)(dr[i])).ToString("d");
                            }
                        }
                        else
                        {
                            ddr[i] = dr[i];
                        }
                    }
                    dgv.Rows.Add(ddr.ItemArray);
                }

                dgv.Columns[0].ReadOnly = true;
                dgv.Columns[1].ReadOnly = true;
                dgv.Columns[2].ReadOnly = true;
                dgv.Columns[3].ReadOnly = true;
                dgv.Columns[4].ReadOnly = true;
                dgv.Columns[5].ReadOnly = true;
                if (this.M_ENGINE == "BOMBARDIER")
                {
                    dgv.Columns.RemoveAt(6);
                    var col = new DataGridViewCheckBoxColumn();
                    col.HeaderText = "Refus";
                    col.Name = "RefusalToggle";
                    dgv.Columns.Insert(6, col);
                }
                dgv.Columns[7].ReadOnly = true;
                dgv.Columns[8].ReadOnly = true;
                dgv.Columns[9].ReadOnly = true;
                dgv.Columns[10].ReadOnly = true;
                dgv.Columns[12].ReadOnly = true;
                dgv.Columns[13].ReadOnly = true;
                dgv.Columns[14].ReadOnly = true;
                dgv.Columns[15].ReadOnly = true;
                dgv.Columns[16].ReadOnly = true;
                dgv.Columns[17].ReadOnly = true;
                dgv.Columns[18].ReadOnly = true;
                dgv.Columns[19].ReadOnly = true;
                dgv.Columns[20].ReadOnly = true;
                dgv.Columns[21].ReadOnly = true;
                dgv.Columns[22].ReadOnly = true;
            }
        }

        private int[] FormatJBDate(string jbDate)
        {
            var returnArray = new int[3];

            string sDay;
            string sMonth;
            string sYear;

            var index = jbDate.IndexOf('/');
            sMonth = jbDate.Substring(0, index);
            jbDate = jbDate.Remove(0, index + 1);

            index = jbDate.IndexOf('/');
            sDay = jbDate.Substring(0, index);
            jbDate = jbDate.Remove(0, index + 1);

            if (jbDate.Length < 5)
            {
                sYear = jbDate;
            }
            else
            {
                sYear = jbDate.Substring(0, 4);
            }


            if (sYear.Length < 3)
            {
                sYear = "20" + sYear;
            }

            returnArray[0] = Convert.ToInt32(sDay);
            returnArray[1] = Convert.ToInt32(sMonth);
            returnArray[2] = Convert.ToInt32(sYear);

            return returnArray;
        }

        private void frmMain_SizeChanged(object sender, EventArgs e)
        {
            this.toolStrip1.Width = this.Width;
            this.dataGridViewMain.Height = this.Height - 142;
            this.dataGridViewMain.Width = this.Width - 16;
            this.progressBar1.Width = this.Width;
        }

        private void frmMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3 && toolStripButton1.Enabled)
            {
                toolStripButton1_Click(toolStripButton1, new EventArgs());
            }
            else if (e.KeyCode == Keys.F4 && toolStripButton2.Enabled)
            {
                toolStripButton1_Click(toolStripButton2, new EventArgs());
            }
            else if (e.KeyCode == Keys.F5 && this.toolStripButton_Read.Enabled)
            {
                toolStripButton_Read_Click(toolStripButton_Read, new EventArgs());
            }
            else if (e.Control && e.KeyCode == Keys.P) // Ctrl+P Settings/Preferences
            {
                préférencesToolStripMenuItem_Click(préférencesToolStripMenuItem, new EventArgs());
                e.SuppressKeyPress = true;
            }
        }

        string GiveCorrectLength(string colType, string currentLength)
        {
            if (colType == "datetime")
            {
                return null;
            }

            return "";
        }

        private void Init()
        {
            EdiDbInterface dbSettingsInterface = null;
            object[] settings = null;
            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                Version version = assembly.GetName().Version;
                this.Text = "Application EDI " + version.ToString();
                // TODO (@mond): Remove unnecessary init of variables since
                // they are all done in each engine settings.
                m_sDeliveryFilePath = null;
                m_sForecastFilePath = null;
                m_sTemplateFilePathDelivery = null;
                m_sTemplateFilePathForecast = null;
                m_sTemplates = null;
                m_BookmarkedLine = -1;
                m_IsReady = false;
                // m_bCombineFiles = false;
                m_DeliveryFileTemplate = null;
                m_DeliveryFileReader = null;
                m_ForecastFileReader = null;
                lblThreadOperation.Text = "";
                lblThreadMaxValue.Text = "";
                lblThreadCurrentValue.Text = "";

                toolStrip_cboBox_Customer.Items.Clear();

                object[] arrDateFormats = { "Jour/Mois/Année", "Mois/Jour/Année", "Année/Mois/Jour" };
                cBoxDeliveryDateFormat.Items.AddRange(arrDateFormats);
                cBoxForecastDateFormat.Items.AddRange(arrDateFormats);

                cBoxDeliveryDateFormat.SelectedIndex = 2;
                cBoxForecastDateFormat.SelectedIndex = 2;

                openFileDialog_CsvFile.InitialDirectory = Application.ExecutablePath;

                M_WRITE_SCHEDULE_LINE_TO_NOTE = "Off";

                dbSettingsInterface = new EdiDbInterface();
                settings = dbSettingsInterface.ReadBasicSettingsFromEdiMfgTables();
                ApplySettings(in settings);
                m_DateRefusals = dbSettingsInterface.ReadRefusalsForDateReasons();
                m_QuantityRefusals = dbSettingsInterface.ReadRefusalsForQuantityReasons();

                //TODO (@mond): Should these be moved to the ApplySettings method?
                if (this.M_ENGINE == "BOMBARDIER")
                {
                    m_bCombineFiles = true;
                    lblDeliveryDateFormat.Text = "Format de date du fichier";
                    cBoxForecastDateFormat.Visible = false;
                    cBoxForecastDateFormat.Enabled = false;
                    lblForecastDateFormat.Enabled = true;
                    lblForecastDateFormat.Text = "Type du fichier";
                    cbo_FileType.Items.AddRange(this.m_sFileTypes);
                    cbo_FileType.Visible = false;
                    cbo_FileType.Enabled = false;
                    toolStrip_cboBox_Customer.Enabled = false;
                    toolStrip_cboBox_Customer.Visible = false;
                    toolStripButton2.Enabled = false;
                    //toolStripButton1.Text = "Delivery";
                }
                else if (M_ENGINE == "GENERIC" || M_ENGINE == "GÉNÉRIQUE")
                {
                    m_bCombineFiles = false;
                    lblDeliveryDateFormat.Text = "Format de date pour le fichier Delivery";
                    cBoxForecastDateFormat.Visible = false;
                    cBoxForecastDateFormat.Enabled = false;
                    lblForecastDateFormat.Enabled = false;
                    toolStripButton2.Enabled = false;
                    cbo_FileType.Visible = false;
                }
                else // PRATT
                {
                    m_bCombineFiles = false;
                    cbo_FileType.Visible = false;

                    if (bOneTemplateExecution /*== false*/)
                    {
                        lblForecastDateFormat.Enabled = false;
                        toolStripButton2.Enabled = false;
                        cBoxForecastDateFormat.Visible = false;
                        cBoxForecastDateFormat.Enabled = false;

                        toolStripButton1.Text = "CSV";
                        toolStripButton1.ToolTipText = "Ouvrir le fichier CSV a utiliser";

                        lblDeliveryDateFormat.Text = "Format de date pour le fichier";
                    }
                    else
                    {
                        lblForecastDateFormat.Enabled = true;
                        toolStripButton2.Enabled = true;
                        cBoxForecastDateFormat.Visible = true;
                        cBoxForecastDateFormat.Enabled = true;

                        toolStripButton1.Text = "Delivery";
                        toolStripButton1.ToolTipText = "Ouvrir le fichier Delivery a utiliser";
                        lblDeliveryDateFormat.Text = "Format de date pour le fichier";
                    }
                }

                if (!IsNullOrEmpty(in engineExtraFeatures) && !String.IsNullOrEmpty(engineExtraFeatures[0].ToString()))
                {
                    EnableExtraFeatures(in engineExtraFeatures);
                }
                else
                {
                    DisableExtraFeatures();
                }

                this.RefuseNewDate.Items.Add("");
                this.RefuseNewQty.Items.Add("");
                this.RefuseNewDate.Items.AddRange(this.m_DateRefusals);
                this.RefuseNewQty.Items.AddRange(this.m_QuantityRefusals);

                this.toolStrip_cboBox_Customer.Items.AddRange(this.m_sCustomers);

                if (this.toolStrip_cboBox_Customer.Items.Count < 2)
                {
                    this.toolStrip_cboBox_Customer.SelectedIndex = 0;
                    this.M_CUSTOMER_ID = this.toolStrip_cboBox_Customer.Text;
                }

                this.CreateDataGridCellStyles();
            }
            catch (DatabaseNotFoundException e)
            {
                MessageBox.Show("La base de données de MFG n'existe pas.\n" + e.Message);
                ExitApplication();

            }
            catch (EDIAPP.TooManyConfigsFoundException e)
            {
                MessageBox.Show(e.Message);
                //TODO (@mond): Make the user clean up the configs....or ask support?
            }
            catch (EDIAPP.NoConfigsFoundException e)
            {
                MessageBox.Show(e.Message);
                //TODO (@mond): Force the user to enter some configs
                //var configDialog = new FormConfigs(dbSettingsInterface.userName, dbSettingsInterface.machineName, in settings);
                var configDialog = new FormConfigs(ref dbSettingsInterface, in settings);
                configDialog.ShowDialog();

                Init();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            timerForEightFiftyStandbys = new System.Timers.Timer(5000);
            timerForEightFiftyStandbys.Elapsed += LookForEightFiftySalesOrdersOnStandbyEvent;
            timerForEightFiftyStandbys.AutoReset = true;
            timerForEightFiftyStandbys.Enabled = true;

            timerForEightSixtyStandbys = new System.Timers.Timer(5000);
            timerForEightSixtyStandbys.Elapsed += LookForEightSixtySalesOrdersOnStandbyEvent;
            timerForEightSixtyStandbys.AutoReset = true;
            timerForEightSixtyStandbys.Enabled = true;
        }

        private bool IsNullOrEmpty(in string[] array)
        {
            return (array == null || array.Length == 0);
        }

        private void Launch850Analysis()
        {
            DisableAllTimers();
            if (M_ENGINE == "BOMBARDIER")
            {
                engine_for_app = new EDI.EDI_Engine_Bombardier("ExtraFeatures",
                  engine850file, M_DBNAME, M_DBSERVER, m_sCustomers,
                  Application.ExecutablePath, cBoxDeliveryDateFormat.SelectedIndex,
                  M_SO_CREATION, M_SO_CREATION_STATUS2);
            }
            else
            {
                MessageBox.Show("Présentement l'engin actuel ne supporte pas "
                  + "les opération de types 850.");
                return;
            }

            engine_for_app.OnCompleteWithMessage += ThreadChildCompleteWithMessage;
            engine_for_app.OnProgressInit += ThreadChildProgressInit;
            engine_for_app.OnProgressReport += ThreadChildProgressReport;

            var ediDbInterface = new EdiDbInterface();
            var args = new string[1] { ediDbInterface.connectionStringToMfgDb };

            var t = new Thread(() => RealStart(ref engine_for_app, "850", args));
            t.Start();
        }

        private void Launch860Analysis()
        {
            DisableAllTimers();
            if (M_ENGINE == "BOMBARDIER")
            {
                engine_for_app = new EDI.EDI_Engine_Bombardier("ExtraFeatures",
                  engine860file, M_DBNAME, M_DBSERVER, m_sCustomers,
                  Application.ExecutablePath, cBoxDeliveryDateFormat.SelectedIndex,
                  M_SO_CREATION, M_SO_CREATION_STATUS2);
            }
            else
            {
                MessageBox.Show("Présentement l'engin actuel ne supporte pas "
                  + "les opération de types 860.");
                return;
            }

            engine_for_app.OnCompleteWithMessage += ThreadChildCompleteWithMessage;
            engine_for_app.OnProgressInit += ThreadChildProgressInit;
            engine_for_app.OnProgressReport += ThreadChildProgressReport;

            var ediDbInterface = new EdiDbInterface();
            var args = new string[1] { ediDbInterface.connectionStringToMfgDb };

            var t = new Thread(() => RealStart(ref engine_for_app, "860", args));
            t.Start();
        }

        private void OpenDeliveryFile()
        {
            string myStream;

            // TODO Find out and implement what needs to be done about or in relation to the filesForRead array.
            if (m_bCombineFiles == true)
            {

                string sSelectedFolder = string.Empty;
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    sSelectedFolder = @"D:\code\MfgTech\EDI\examples\2021-01-07";
                    this.filesForRead = System.IO.Directory.GetFiles(sSelectedFolder, "*.csv");
                }
                else
                {
                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        sSelectedFolder = this.folderBrowserDialog.SelectedPath;
                        this.filesForRead = System.IO.Directory.GetFiles(sSelectedFolder, "*.csv");
                    }
                }

            }
            else if (this.openFileDialog_CsvFile.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = this.openFileDialog_CsvFile.FileName) != null)
                {
                    this.m_sDeliveryFilePath = myStream;
                }

                if (this.bOneTemplateExecution)
                {
                    this.m_sForecastFilePath = this.m_sDeliveryFilePath;
                }
            }

            this.VerifyReadiness();

            if (m_IsReady)
                toolStripButton_Read_Click(null, EventArgs.Empty);
        }

        private void OpenForecastFile()
        {
            string myStream;

            if (openFileDialog_CsvFile.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = openFileDialog_CsvFile.FileName) != null)
                {
                    m_sForecastFilePath = myStream.ToString();
                    VerifyReadiness();
                }
            }
        }

        private void Process()
        {
            engine_for_app.OnCompleteWithMessage += ThreadChildCompleteWithMessage;
            engine_for_app.OnComplete += ThreadChildWriteComplete;
            engine_for_app.OnProgressInit += ThreadChildProgressInit;
            engine_for_app.OnProgressReport += ThreadChildProgressReport;

            if (M_ENGINE == "BOMBARDIER")
            {
                var aThread = new Thread(new ParameterizedThreadStart(engine_for_app.Write));
                aThread.Start(this.dataGridViewMain);
            }
            else
            {
                var aThread = new Thread(new ThreadStart(engine_for_app.Write));
                aThread.Start();
            }
        }

        private void ProcessThreadCompleted()
        {
            progressBar2.Value = 0;
            lblThreadMaxValue.Text = "";
            lblThreadCurrentValue.Text = "";
            progressBar1.Visible = false;
            lblThreadOperation.Visible = false;
            toolStripButton_Process.Enabled = true;
        }

        private void ProcessThreadProgressInit(int iMaxValue, string sOperation)
        {
            progressBar2.Value = 0;
            progressBar2.Maximum = iMaxValue;
            lblThreadOperation.Text = sOperation;
            lblThreadMaxValue.Text = "/" + progressBar2.Maximum;
            lblThreadCurrentValue.Text = progressBar2.Value.ToString();
        }

        private void ProcessThreadProgressReport(int iIncrementBy)
        {
            progressBar2.Increment(iIncrementBy);
            lblThreadCurrentValue.Text = progressBar2.Value.ToString();
        }

        private void ReadThreadCompleted(ref DataGridView dgv, ref DataTable dTable)
        {
            FillDataGridView(ref dgv, ref dTable);
            ApplyGridColoring();
            DumpDataToTempFile();

            toolStripButton4.Enabled = true;
            toolStripButton5.Enabled = true;
            toolStripButton_Process.Enabled = true;

            progressBar1.Visible = false;
            lblThreadOperation.Text = "";
            lblThreadMaxValue.Text = "";
            lblThreadCurrentValue.Text = "";
            progressBar2.Value = 0;
            toolStripButton_Read.Enabled = true;

            NbRowsLabel.Text = dataGridViewMain.Rows.Count.ToString() + " lignes";
        }

        private void ReadThreadProgressInit(int iMaxValue, string sOperation)
        {
            progressBar2.Value = 0;
            progressBar2.Maximum = iMaxValue;
            lblThreadOperation.Text = sOperation;
            lblThreadMaxValue.Text = "/" + progressBar2.Maximum;
            lblThreadCurrentValue.Text = progressBar2.Value.ToString();
        }

        private void ReadThreadProgressReport(int iIncrementBy)
        {
            progressBar2.Increment(iIncrementBy);
            lblThreadCurrentValue.Text = progressBar2.Value.ToString();
        }

        public void ThreadChildComplete()
        {
            m_MergedTable = engine_for_app.GetMergedTable();

            if (InvokeRequired)
            {
                this.Invoke(new ReadThreadEnd(ReadThreadCompleted), new object[] { this.dataGridViewMain, this.m_MergedTable });
            }
            else
            {
                ReadThreadCompleted(ref dataGridViewMain, ref m_MergedTable);
            }
        }

        public void ThreadChildCompleteWithMessage(string message)
        {
            if (message == "brp850AnalyzeDone")
            {
                var bindingSource = new BindingSource();
                bindingSource.DataSource = engine_for_app.GetExtraFeatureTable();
                if (InvokeRequired)
                {
                    Invoke(new PassABindingSource(DisplayEightFiftyAnalyzeResults),
                      new object[] { bindingSource });

                }
                else
                {
                    DisplayEightFiftyAnalyzeResults(in bindingSource);
                }
            }
            else if (message == "brp850ExecDone")
            {
                if (InvokeRequired)
                {
                    Invoke(new PassAString(Completed850Exec),
                      new object[] { message });
                }
                else { Completed850Exec(message); }
            }
            else if (message == "brp855Done")
            {
                if (InvokeRequired)
                {
                    Invoke(new PassAString(Completed855),
                      new object[] { message });
                }
                else { Completed855(message); }
            }
            else if (message == "brp860AnalyzeDone")
            {
                var bindingSource = new BindingSource();
                bindingSource.DataSource = engine_for_app.GetExtraFeatureTable();
                if (InvokeRequired)
                {
                    Invoke(new PassABindingSource(DisplayEightSixtyAnalyzeResults),
                      new object[] { bindingSource });

                }
                else
                {
                    DisplayEightSixtyAnalyzeResults(in bindingSource);
                }
            }
            else if (message == "brp860ExecDone")
            {
                if (InvokeRequired)
                {
                    Invoke(new PassAString(Completed860Exec),
                      new object[] { message });
                }
                else { Completed860Exec(message); }
            }
            else if (message == "brp865Done")
            {
                if (InvokeRequired)
                {
                    Invoke(new PassAString(Completed865),
                      new object[] { message });
                }
                else { Completed865(message); }
            }
            else
            {
                if (InvokeRequired)
                {
                    ControlPropertyModifyCallback d = new ControlPropertyModifyCallback(ThreadChildWriteCompleteThread2);
                    Invoke(d);
                }
                else
                {
                    ThreadChildWriteCompleteThread2();
                }

                MessageBox.Show(message);
            }

            EnableAllTimers();
        }


        public void ThreadChildProgressInit(int iMaxValue, string sOperation)
        {
            if (InvokeRequired)
            {
                Invoke(new ReadThreadInit(ReadThreadProgressInit), new object[] { iMaxValue, sOperation });
            }
            else
            {
                ReadThreadProgressInit(iMaxValue, sOperation);
            }
        }

        public void ThreadChildProgressReport(int iIncrementBy)
        {
            if (this.InvokeRequired)
            {
                Invoke(new ReadThreadReport(ReadThreadProgressReport),
                  new object[] { iIncrementBy });
            }
            else
            {
                this.ReadThreadProgressReport(iIncrementBy);
            }
        }

        public void ThreadChildWriteComplete()
        {
            if (InvokeRequired)
            {
                ControlPropertyModifyCallback d = new ControlPropertyModifyCallback(ThreadChildWriteCompleteThread);
                Invoke(d);
            }
            else
            {
                ThreadChildWriteCompleteThread();
            }
        }

        private void ThreadChildWriteCompleteThread()
        {
            if (dataGridViewMain.Rows.Count > 0)
            {
                dataGridViewMain.Rows.Clear();
            }

            NbRowsLabel.Text = dataGridViewMain.Rows.Count.ToString() + " lignes";

            MessageBox.Show("Écriture terminée");
        }

        private void ThreadChildWriteCompleteThread2()
        {
            if (dataGridViewMain.Rows.Count > 0)
            {
                dataGridViewMain.Rows.Clear();
            }

            ProcessThreadCompleted();

            NbRowsLabel.Text = dataGridViewMain.Rows.Count.ToString() + " lignes";
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenDeliveryFile();
        }

        private void toolStripButton_Read_Click(object sender, EventArgs e)
        {
            if (M_ENGINE == "PRATT")
            {
                engine_for_app = (EDI_Engine_Base)(new EDI_Engine_Pratt(m_sTemplateFilePathForecast,
                    m_sForecastFilePath, m_sTemplateFilePathDelivery, m_sDeliveryFilePath, M_DBNAME,
                    M_DBSERVER, M_CUSTOMER_ID, Application.ExecutablePath,
                    cBoxDeliveryDateFormat.SelectedIndex, cBoxForecastDateFormat.SelectedIndex,
                    M_SO_CREATION, M_SO_CREATION_STATUS, M_WRITE_SCHEDULE_LINE_TO_NOTE, useShippedLines));
            }
            else if (M_ENGINE == "BOMBARDIER")
            {
                if (m_bCombineFiles == true)
                {
                    var typesAndTemplates = new string[m_sTemplates.Length][];
                    for (int index = 0; index < m_sTemplates.Length; index++)
                    {
                        typesAndTemplates[index] = new string[2];
                        typesAndTemplates[index][0] = m_sFileTypes[index];
                        typesAndTemplates[index][1] = m_sTemplates[index];
                    }
                    engine_for_app = (EDI_Engine_Base)(new EDI_Engine_Bombardier(typesAndTemplates,
                        filesForRead, M_DBNAME, M_DBSERVER, m_sCustomers, Application.ExecutablePath,
                        cBoxDeliveryDateFormat.SelectedIndex, M_SO_CREATION, M_SO_CREATION_STATUS));
                }
                else
                {
                    MessageBox.Show("Edi_Engine_Bombardier n'a plus d'utilisation à un seul fichier.");
                    return;
                }
            }
            else if (M_ENGINE == "GENERIC" || M_ENGINE == "GÉNÉRIQUE")
            {
                engine_for_app = (EDI_Engine_Base)(new EDI_Engine_Generic(m_sTemplateFilePathDelivery,
                    m_sDeliveryFilePath, M_DBNAME, M_DBSERVER, M_CUSTOMER_ID, Application.ExecutablePath,
                    cBoxDeliveryDateFormat.SelectedIndex, M_SO_CREATION, M_SO_CREATION_STATUS));
            }
            else
            {
                //Messagebox.Show("Vérifiez le fichier de configuration. L'engin sélectionner n'est pas valide.");
            }

            engine_for_app.OnComplete += ThreadChildComplete;
            engine_for_app.OnProgressInit += ThreadChildProgressInit;
            engine_for_app.OnProgressReport += ThreadChildProgressReport;

            var aThread = new Thread(new ThreadStart(engine_for_app.Read));
            aThread.Start();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            OpenForecastFile();
        }

        private void toolStripButton_PrepareMaterialPlanning_Click(object sender, EventArgs e)
        {
            DumpDataToTempFile();
        }

        private void toolStripBookmarkLine_Click(object sender, EventArgs e)
        {
            if (m_BookmarkedLine > 0)
            {
                dataGridViewMain.Rows[m_BookmarkedLine].DefaultCellStyle = dataGridViewMain.DefaultCellStyle;
            }

            m_BookmarkedLine = dataGridViewMain.CurrentCell.RowIndex;

            dataGridViewMain.CurrentRow.DefaultCellStyle = m_bookmarkedStyle;
            ApplyGridColoring();
        }

        private void toolStripBookmarkedLineGoTo_Click(object sender, EventArgs e)
        {
            dataGridViewMain.CurrentCell = dataGridViewMain[0, m_BookmarkedLine];
        }

        private void toolStripButton_Process_Click(object sender, EventArgs e)
        {
            this.progressBar1.Visible = true;

            Thread processingThread = new Thread(
              new ThreadStart(Process));

            processingThread.Name = "WriteThread";

            processingThread.Start();

            this.toolStripButton_Process.Enabled = false;
        }

        private void toolStrip_cboBox_Customer_SelectedIndexChanged(object sender, EventArgs e)
        {
            M_CUSTOMER_ID = toolStrip_cboBox_Customer.Text;
        }

        void TryReady()
        {
            m_DeliveryFileTemplate = new MFG.MFG_File_Template();
            m_DeliveryFileTemplate.SetFile(m_sTemplateFilePathDelivery);
            m_DeliveryFileTemplate.Process();

            if (!bOneTemplateExecution)
            {
                m_ForecastFileTemplate = new MFG.MFG_File_Template();
                m_ForecastFileTemplate.SetFile(m_sTemplateFilePathForecast);
                m_ForecastFileTemplate.Process();
                //m_ForecastFileReader = new MFG.Files.CSV.CCsvReader(m_sForecastFilePath, m_ForecastFileTemplate);
            }
            else
            {
                //What to do when only one file?
            }

            m_IsReady = true;
            toolStripButton_Read.Enabled = true;
            toolStripButton_PrepareMaterialPlanning.Enabled = true;
        }

        void UpdateSOTotalPrice(string sSO)
        {
            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
            var sQuery = "SELECT SUM(Total_Price) FROM SO_Detail WHERE Sales_Order LIKE '" + sSO + "'";

            DataTable dt = conn.GetData(sQuery);

            sQuery = "UPDATE SO_Header SET Total_Price = " + dt.Rows[0][0].ToString() + " WHERE Sales_Order LIKE '" + sSO + "'";
            conn.SetData(sQuery);
        }

        void VerifyReadiness()
        {
            if (AreBothFilesThere())
            {
                TryReady();
            }

        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var theAbout = new frmAbout();
            theAbout.ShowDialog();
        }

        private void quitterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ouvrirDeliveryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenDeliveryFile();
        }

        private void ouvrirForecastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenForecastFile();
        }

        private void préférencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EdiDbInterface dbSettingsInterface = null;
            object[] settings = null;

            dbSettingsInterface = new EdiDbInterface();
            settings = dbSettingsInterface.ReadBasicSettingsFromEdiMfgTables();

            var configDialog = new FormConfigs(ref dbSettingsInterface, in settings);
            configDialog.ShowDialog();

            Init();
        }

        private void buttonOpen850File_Click(object sender, EventArgs e)
        {
            string myStream;
            if (this.openFileDialog_CsvFile.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = this.openFileDialog_CsvFile.FileName) != null)
                {
                    this.engine850file = myStream;
                }

                Launch850Analysis();
            }
        }

        private void buttonOpen860File_Click(object sender, EventArgs e)
        {
            string myStream;
            if (this.openFileDialog_CsvFile.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = this.openFileDialog_CsvFile.FileName) != null)
                {
                    this.engine860file = myStream;
                }

                Launch860Analysis();
            }
        }

        private void buttonExec850_Click(object sender, EventArgs e)
        {
            buttonExec850.Enabled = false;
            dataGridViewMain.Visible = true;
            dataGridView_ExtraFeatures.DataSource = null;
            DisableAllTimers();

            var ediDbInterface = new EdiDbInterface();
            var args = new string[1] { ediDbInterface.connectionStringToMfgDb };

            var t = new Thread(() => RealStart(ref engine_for_app, "850Exec", args));
            t.Start();
        }

        private void buttonExec860_Click(object sender, EventArgs e)
        {
            buttonExec860.Enabled = false;
            dataGridViewMain.Visible = true;
            dataGridView_ExtraFeatures.DataSource = null;
            DisableAllTimers();

            var ediDbInterface = new EdiDbInterface();
            var args = new string[1] { ediDbInterface.connectionStringToMfgDb };

            var t = new Thread(() => RealStart(ref engine_for_app, "860Exec", args));
            t.Start();
        }

        private void buttonExec855_Click(object sender, EventArgs e)
        {
            if (M_ENGINE != "BOMBARDIER")
            {
                MessageBox.Show("Présentement l'engin actuel ne supporte pas les opération de types 855.");
                return;
            }

            buttonExec855.Enabled = false;
            DisableAllTimers();

            EDI_Engine_Base engine;
            string fileName;
            var saveCsvDialog = new System.Windows.Forms.SaveFileDialog();
            //saveCsvDialog.InitialDirectory = "%USERPROFILE%";
            saveCsvDialog.FileName = this.openFileDialog_CsvFile.FileName.Replace("850_", "855_");
            saveCsvDialog.DefaultExt = "csv";
            saveCsvDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            saveCsvDialog.FilterIndex = 1;
            saveCsvDialog.CheckPathExists = true;
            if (saveCsvDialog.ShowDialog() == DialogResult.OK)
            {
                if ((fileName = saveCsvDialog.FileName) != null)
                {
                    engine = new EDI.EDI_Engine_Bombardier(
                      "ExtraFeaturesWrite", fileName, M_DBNAME, M_DBSERVER,
                      m_sCustomers, Application.ExecutablePath,
                      cBoxDeliveryDateFormat.SelectedIndex, M_SO_CREATION,
                      M_SO_CREATION_STATUS2);
                }
                else { return; }
            }
            else
            {
                MessageBox.Show("Vous n'avez pas sélectionneé de fichier pour sauvegarder. Nous ne pouvons procéder.");
                EnableAllTimers();
                return;
            }

            var ediDbInterface = new EdiDbInterface();
            var args = new string[2] { ediDbInterface.connectionStringToMfgDb,
        fileName };

            var t = new Thread(() => RealStart(ref engine, "855", args));
            t.Start();
        }

        private void buttonExec865_Click(object sender, EventArgs e)
        {
            if (M_ENGINE != "BOMBARDIER")
            {
                MessageBox.Show("Présentement l'engin actuel ne supporte pas les opération de types 865.");
                return;
            }

            buttonExec865.Enabled = false;
            DisableAllTimers();

            EDI_Engine_Base engine;
            string fileName;
            var saveCsvDialog = new System.Windows.Forms.SaveFileDialog();
            //saveCsvDialog.InitialDirectory = "%USERPROFILE%";
            saveCsvDialog.FileName = this.openFileDialog_CsvFile.FileName.Replace("860_", "865_");
            saveCsvDialog.DefaultExt = "csv";
            saveCsvDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            saveCsvDialog.FilterIndex = 1;
            saveCsvDialog.CheckPathExists = true;
            if (saveCsvDialog.ShowDialog() == DialogResult.OK)
            {
                if ((fileName = saveCsvDialog.FileName) != null)
                {
                    engine = new EDI.EDI_Engine_Bombardier(
                      "ExtraFeaturesWrite", fileName, M_DBNAME, M_DBSERVER,
                      m_sCustomers, Application.ExecutablePath,
                      cBoxDeliveryDateFormat.SelectedIndex, M_SO_CREATION,
                      M_SO_CREATION_STATUS2);
                }
                else { return; }
            }
            else
            {
                MessageBox.Show("Vous n'avez pas sélectionner de fichier pour sauvegarder. Nous ne pouvons procéder.");
                EnableAllTimers();
                return;
            }

            var ediDbInterface = new EdiDbInterface();
            var args = new string[2] { ediDbInterface.connectionStringToMfgDb,
          fileName };

            var t = new Thread(() => RealStart(ref engine, "865", args));
            t.Start();
        }

        private void LookForEightFiftySalesOrdersOnStandby()
        {
            var ediDbInterface = new EdiDbInterface();
            var connEdi = new System.Data.SqlClient.SqlConnection(
              ediDbInterface.connectionStringToMfgDb);
            connEdi.Open();

            var getEightFiftySalesOrdersQuery = "SELECT createdSO As SO, "
            + "releaseNumber, id FROM brpCreatedSalesOrders WHERE tempStatus LIKE '850'";
            var command = new System.Data.SqlClient.SqlCommand(getEightFiftySalesOrdersQuery, connEdi);
            var readerForEightFiftySalesOrders = command.ExecuteReader();
            try
            {
                if (readerForEightFiftySalesOrders.HasRows)
                {
                    ControlPropertyModifyCallback d = new ControlPropertyModifyCallback(
                      EnableEightFiftyFiveButton);
                    this.Invoke(d);
                }
                else
                {
                    ControlPropertyModifyCallback d = new ControlPropertyModifyCallback(
                      DisableEightFiftyFiveButton);
                    this.Invoke(d);
                }
            }
            finally
            {
            }
        }

        private void LookForEightFiftySalesOrdersOnStandbyEvent(object sender, EventArgs e)
        {
            try
            {
                LookForEightFiftySalesOrdersOnStandby();
            }
            catch { }
        }

        private void LookForEightSixtySalesOrdersOnStandby()
        {
            var ediDbInterface = new EdiDbInterface();
            var connEdi = new System.Data.SqlClient.SqlConnection(
              ediDbInterface.connectionStringToMfgDb);
            connEdi.Open();

            var getEightSixtySalesOrdersQuery = "SELECT modifiedSO As SO, "
            + "releaseNumber, id FROM brpModifiedSalesOrders WHERE tempStatus LIKE '860'";
            var command = new System.Data.SqlClient.SqlCommand(getEightSixtySalesOrdersQuery, connEdi);
            var readerForEightSixtySalesOrders = command.ExecuteReader();
            try
            {
                if (readerForEightSixtySalesOrders.HasRows)
                {
                    ControlPropertyModifyCallback d = new ControlPropertyModifyCallback(
                      EnableEightSixtyFiveButton);
                    this.Invoke(d);
                }
                else
                {
                    ControlPropertyModifyCallback d = new ControlPropertyModifyCallback(
                      DisableEightSixtyFiveButton);
                    this.Invoke(d);
                }
            }
            finally
            {
            }


        }

        private void LookForEightSixtySalesOrdersOnStandbyEvent(object sender, EventArgs e)
        {
            try
            {
                LookForEightSixtySalesOrdersOnStandby();
            }
            catch { }
        }

        private static void RealStart(ref EDI_Engine_Base engine, string str, string[] args)
        {
            try
            {
                engine.ExecuteExtraFeature(str, args);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
