using MfgConnection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EDIAPP
{
    public partial class FormConfigs : Form
    {
        private EdiDbInterface ediDbInterface;
        private string executionTypeSelected;
        private bool mustInsertSettings;
        private object[][] allUserSettings;
        private int currentlyDisplayedSetting;

        public FormConfigs(ref EdiDbInterface ediDbInterface, in object[] settings)
        {
            InitializeComponent();

            this.ediDbInterface = ediDbInterface;
            textBoxUserName.Text = ediDbInterface.userName;
            textBoxWorkstation.Text = ediDbInterface.machineName;

            var conn = new jbConnection();
            comboBoxJbDbName.Items.AddRange(conn.showAvailableDBs());

            if (!(settings == null || settings.Length == 0))
            {
                mustInsertSettings = false;

                comboBoxEngine.SelectedItem = settings[(int)EdiDbInterface.Settings.engine].ToString();
                comboBoxJbDbName.SelectedItem = settings[(int)EdiDbInterface.Settings.jbDbName].ToString();
                textBoxJbDbServer.Text = settings[(int)EdiDbInterface.Settings.jbDbServer].ToString();
                checkBoxSoCreation.Checked = (bool)settings[(int)EdiDbInterface.Settings.soCreation];
                comboBoxSoCreationStatus.SelectedItem = settings[(int)EdiDbInterface.Settings.soCreationStatus].ToString();

                var execution = settings[(int)EdiDbInterface.Settings.executionType].ToString();
                if (execution == "simple") { radioButtonExecutionSimple.Checked = true; }
                else if (execution == "double") { radioButtonExecutionDouble.Checked = true; }
                else if (execution == "complex") { radioButtonExecutionComplex.Checked = true; }

                listBoxTemplates.Items.AddRange(settings[(int)EdiDbInterface.Settings.template].ToString().Split(';'));
                listBoxClients.Items.AddRange(settings[(int)EdiDbInterface.Settings.clients].ToString().Split(','));
                listBoxFiletypes.Items.AddRange(settings[(int)EdiDbInterface.Settings.filetype].ToString().Split(','));
                listBoxExtras.Items.AddRange(settings[(int)EdiDbInterface.Settings.extras].ToString().Split(','));

                checkBoxWriteSchedulineToNote.Checked = (bool)settings[(int)EdiDbInterface.Settings.writeScheduleLineToNote];

                checkBoxConfActive.Checked = (bool)settings[(int)EdiDbInterface.Settings.active];
                textBoxConfigName.Text = settings[(int)EdiDbInterface.Settings.title].ToString();
                labelConfId.Text = settings[(int)EdiDbInterface.Settings.id].ToString();
                checkBoxUseShippedSoDetailLines.Checked = (bool)settings[(int)EdiDbInterface.Settings.useShippedLines];
            }
            else
            {
                mustInsertSettings = true;
                textBoxJbDbServer.Text = conn.GetJbSettingsServer();
                labelConfId.Text = "";
            }

            buttonSave.Enabled = false;

            allUserSettings = ediDbInterface.AcquireAllConfigurationsForUserAndStation();
        }

        private void buttonTemplateNew_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var filePath = openFileDialog1.FileName;
                listBoxTemplates.Items.Add(filePath);
            }
        }

        private void buttonTemplateModify_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var filePath = openFileDialog1.FileName;
                listBoxTemplates.Items[listBoxTemplates.SelectedIndex] = filePath;
            }
        }

        private void buttonTemplateDelete_Click(object sender, EventArgs e)
        {
            if (listBoxTemplates.SelectedIndex != -1)
            {
                listBoxTemplates.Items.Remove(listBoxTemplates.SelectedItem);
                EnableButtonSave();
            }
        }

        private void buttonClientNew_Click(object sender, EventArgs e)
        {
            listBoxClients.ClearSelected();
            textBoxClientInput.Text = "";
            textBoxClientInput.Visible = true;
            textBoxClientInput.Focus();
        }

        private void buttonClientModify_Click(object sender, EventArgs e)
        {
            if (listBoxClients.SelectedIndex != -1)
            {
                textBoxClientInput.Text = listBoxClients.SelectedItem.ToString();
                textBoxClientInput.Visible = true;
                textBoxClientInput.Focus();
            }
        }

        private void buttonClientDelete_Click(object sender, EventArgs e)
        {
            if (listBoxClients.SelectedIndex != -1)
            {
                listBoxClients.Items.RemoveAt(listBoxClients.SelectedIndex);
                EnableButtonSave();
            }
        }

        private void buttonFiletypesNew_Click(object sender, EventArgs e)
        {
            listBoxFiletypes.ClearSelected();
            textBoxFiletypesInput.Text = "";
            textBoxFiletypesInput.Visible = true;
            textBoxFiletypesInput.Focus();
        }

        private void buttonFiletypesModify_Click(object sender, EventArgs e)
        {
            if (listBoxFiletypes.SelectedIndex != -1)
            {
                textBoxFiletypesInput.Text = "";
                textBoxFiletypesInput.Visible = true;
                textBoxFiletypesInput.Focus();
            }
        }

        private void buttonFiletypesDelete_Click(object sender, EventArgs e)
        {
            if (listBoxFiletypes.SelectedIndex != -1)
            {
                listBoxFiletypes.Items.RemoveAt(listBoxFiletypes.SelectedIndex);
                EnableButtonSave();
            }
        }

        private void buttonExtrasNew_Click(object sender, EventArgs e)
        {
            listBoxExtras.ClearSelected();
            textBoxExtrasInput.Text = "";
            textBoxExtrasInput.Visible = true;
            textBoxExtrasInput.Focus();
        }

        private void buttonExtrasModify_Click(object sender, EventArgs e)
        {
            if (listBoxExtras.SelectedIndex != -1)
            {
                textBoxExtrasInput.Text = "";
                textBoxExtrasInput.Visible = true;
                textBoxExtrasInput.Focus();
            }
        }

        private void buttonExtrasDelete_Click(object sender, EventArgs e)
        {
            if (listBoxExtras.SelectedIndex != -1)
            {
                listBoxExtras.Items.RemoveAt(listBoxExtras.SelectedIndex);
                EnableButtonSave();
            }
        }

        private bool DoWeHaveFullSettings()
        {
            if (comboBoxEngine.SelectedIndex == -1 || comboBoxJbDbName.SelectedIndex == -1
              || String.IsNullOrWhiteSpace(textBoxJbDbServer.Text) || comboBoxSoCreationStatus.SelectedIndex == -1
              || (!radioButtonExecutionComplex.Checked && !radioButtonExecutionDouble.Checked && !radioButtonExecutionSimple.Checked)
              || listBoxTemplates.Items.Count == 0 || listBoxClients.Items.Count == 0
              || listBoxFiletypes.Items.Count == 0) { return false; }

            return true;
        }
        private void EnableButtonSave()
        {
            if (DoWeHaveFullSettings()) { buttonSave.Enabled = true; }
            else { buttonSave.Enabled = false; }
        }

        private void textBoxClientNew_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !String.IsNullOrWhiteSpace(textBoxClientInput.Text))
            {
                if (listBoxClients.SelectedIndex == -1)
                {
                    listBoxClients.Items.Add(textBoxClientInput.Text);
                }
                else
                {
                    listBoxClients.Items[listBoxClients.SelectedIndex] = textBoxClientInput.Text;
                }
                textBoxClientInput.Visible = false;
                listBoxClients.ClearSelected();
                buttonClientNew.Focus();
                EnableButtonSave();
            }
        }

        private void textBoxFiletypesInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !String.IsNullOrWhiteSpace(textBoxFiletypesInput.Text))
            {
                if (listBoxFiletypes.SelectedIndex == -1)
                {
                    listBoxFiletypes.Items.Add(textBoxFiletypesInput.Text);
                }
                else
                {
                    listBoxFiletypes.Items[listBoxFiletypes.SelectedIndex] = textBoxFiletypesInput.Text;
                }
                textBoxFiletypesInput.Visible = false;
                listBoxFiletypes.ClearSelected();
                buttonFiletypesNew.Focus();
                EnableButtonSave();
            }
        }

        private void clearAllControls()
        {
            textBoxConfigName.Text = "";
            checkBoxConfActive.Checked = false;
            comboBoxEngine.SelectedIndex = 0;
            comboBoxJbDbName.SelectedIndex = 0;
            checkBoxSoCreation.Checked = false;
            comboBoxSoCreationStatus.SelectedIndex = 0;
            radioButtonExecutionSimple.Checked = true;
            listBoxTemplates.Items.Clear();
            listBoxClients.Items.Clear();
            listBoxExtras.Items.Clear();
            listBoxFiletypes.Items.Clear();
            checkBoxWriteSchedulineToNote.Checked = false;
            labelConfId.Text = "";
            checkBoxUseShippedSoDetailLines.Checked = false;
        }

        private void comboBoxEngine_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableButtonSave();
        }

        private void comboBoxJbDbName_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableButtonSave();
        }

        private void comboBoxSoCreationStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableButtonSave();
        }

        private void textBoxJbDbServer_TextChanged(object sender, EventArgs e)
        {
            EnableButtonSave();
        }

        private void checkBoxSoCreation_CheckStateChanged(object sender, EventArgs e)
        {
            EnableButtonSave();
        }

        private void radioButtonExecutionSimple_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonExecutionSimple.Checked) { executionTypeSelected = "simple"; }
            EnableButtonSave();
        }

        private void radioButtonExecutionDouble_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonExecutionDouble.Checked) { executionTypeSelected = "double"; }
            EnableButtonSave();
        }

        private void radioButtonExecutionComplex_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonExecutionComplex.Checked) { executionTypeSelected = "complex"; }
            EnableButtonSave();
        }

        private void checkBoxWriteSchedulineToNote_CheckStateChanged(object sender, EventArgs e)
        {
            EnableButtonSave();
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (DoWeHaveFullSettings())
            {
                var settings = new List<string>();
                //public enum Settings { id, userName, workstation, engine, soCreation,
                //soCreationStatus, executionType, template, clients, filetype, extras,
                //writeScheduleLineToNote, jbDbName, jbDbServer };
                settings.Add(labelConfId.Text);
                settings.Add(textBoxUserName.Text);
                settings.Add(textBoxWorkstation.Text);
                settings.Add(comboBoxEngine.SelectedItem.ToString());
                settings.Add(checkBoxSoCreation.Checked.ToString());
                settings.Add(comboBoxSoCreationStatus.SelectedItem.ToString());
                settings.Add(executionTypeSelected);
                settings.Add(ListBoxItemsToSettingsString(in listBoxTemplates, ";"));
                settings.Add(ListBoxItemsToSettingsString(in listBoxClients, ","));
                settings.Add(ListBoxItemsToSettingsString(in listBoxFiletypes, ","));
                settings.Add(ListBoxItemsToSettingsString(in listBoxExtras, ","));
                settings.Add(checkBoxWriteSchedulineToNote.Checked.ToString());
                settings.Add(comboBoxJbDbName.SelectedItem.ToString());
                settings.Add(textBoxJbDbServer.Text);
                settings.Add(checkBoxConfActive.Checked?1.ToString():0.ToString());
                settings.Add(textBoxConfigName.Text);
                settings.Add(checkBoxUseShippedSoDetailLines.Checked ? 1.ToString() : 0.ToString());


                if (mustInsertSettings) { ediDbInterface.WriteBasicSettings(in settings); }
                else { ediDbInterface.UpdateBasicSettings(in settings); }
                buttonSave.Enabled = false;
                mustInsertSettings = false;
                allUserSettings = ediDbInterface.AcquireAllConfigurationsForUserAndStation();
            }
        }

        private void textBoxExtrasInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !String.IsNullOrWhiteSpace(textBoxExtrasInput.Text))
            {
                if (listBoxExtras.SelectedIndex == -1)
                {
                    listBoxExtras.Items.Add(textBoxExtrasInput.Text);
                }
                else
                {
                    listBoxExtras.Items[listBoxExtras.SelectedIndex] = textBoxExtrasInput.Text;
                }
                textBoxExtrasInput.Visible = false;
                listBoxExtras.ClearSelected();
                buttonExtrasNew.Focus();
                EnableButtonSave();
            }
        }

        private void textBoxExtrasInput_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private string ListBoxItemsToSettingsString(in ListBox lBox, string delimeter)
        {
            string[] list = lBox.Items.OfType<string>().ToArray();
            var settingString = String.Join(delimeter, list);

            return settingString;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (buttonSave.Enabled)
            {
                buttonSave_Click(sender, e);
            }
        }

        private void textBoxConfigName_Enter(object sender, EventArgs e)
        {
            if (textBoxConfigName.Text == "Nom de la configuration")
            {
                textBoxConfigName.Text = "";
                textBoxConfigName.ForeColor = Color.Black;
            }
        }

        private void textBoxConfigName_Leave(object sender, EventArgs e)
        {
            if (textBoxConfigName.Text == "")
            {
                textBoxConfigName.Text = "Nom de la configuration";
                textBoxConfigName.ForeColor = Color.Silver;
            }
        }

        private void buttonConfAddNew_Click(object sender, EventArgs e)
        {
            clearAllControls();
            mustInsertSettings = true;

        }

        private void buttonConfDelete_Click(object sender, EventArgs e)
        {
            ediDbInterface.DeleteSettingWithId(labelConfId.Text);
            allUserSettings = ediDbInterface.AcquireAllConfigurationsForUserAndStation();
        }

        private void checkBoxConfActive_CheckedChanged(object sender, EventArgs e)
        {
            EnableButtonSave();
        }

        private void textBoxConfigName_TextChanged(object sender, EventArgs e)
        {
            EnableButtonSave();

            if (textBoxConfigName.Text == "")
            {
                textBoxConfigName.Text = "Nom de la configuration";
                textBoxConfigName.ForeColor = Color.Silver;
            }
            else
            {
                textBoxConfigName.ForeColor = Color.Black;
            }
        }

        private void buttonConfNavNext_Click(object sender, EventArgs e)
        {
            int positionOfCurrentConf = 0;
            int positionOfConfToDisplay;

            for(int i = 0; i < allUserSettings.Length; i++)
            {
                if(labelConfId.Text == allUserSettings[i][(int)EdiDbInterface.Settings.id].ToString())
                {
                    positionOfCurrentConf = i;
                }
            }

            if(positionOfCurrentConf >= allUserSettings.Length - 1)
            {
                positionOfConfToDisplay = 0;
            }
            else
            {
                positionOfConfToDisplay = positionOfCurrentConf + 1;
            }

            DisplayConfAtPosition(positionOfConfToDisplay);
        }

        private void buttonConfNavPrev_Click(object sender, EventArgs e)
        {
            int positionOfCurrentConf = 0;
            int positionOfConfToDisplay;

            for (int i = 0; i < allUserSettings.Length; i++)
            {
                if (labelConfId.Text == allUserSettings[i][(int)EdiDbInterface.Settings.id].ToString())
                {
                    positionOfCurrentConf = i;
                }
            }

            if (positionOfCurrentConf <= 0)
            {
                positionOfConfToDisplay = allUserSettings.Length - 1;
            }
            else
            {
                positionOfConfToDisplay = positionOfCurrentConf - 1;
            }

            DisplayConfAtPosition(positionOfConfToDisplay);
        }

        private void DisplayConfAtPosition(in int pos)
        {
            mustInsertSettings = false;

            clearAllControls();

            comboBoxEngine.SelectedItem = allUserSettings[pos][(int)EdiDbInterface.Settings.engine].ToString();
            comboBoxJbDbName.SelectedItem = allUserSettings[pos][(int)EdiDbInterface.Settings.jbDbName].ToString();
            textBoxJbDbServer.Text = allUserSettings[pos][(int)EdiDbInterface.Settings.jbDbServer].ToString();
            checkBoxSoCreation.Checked = (bool)allUserSettings[pos][(int)EdiDbInterface.Settings.soCreation];
            comboBoxSoCreationStatus.SelectedItem = allUserSettings[pos][(int)EdiDbInterface.Settings.soCreationStatus].ToString();

            var execution = allUserSettings[pos][(int)EdiDbInterface.Settings.executionType].ToString();
            if (execution == "simple") { radioButtonExecutionSimple.Checked = true; }
            else if (execution == "double") { radioButtonExecutionDouble.Checked = true; }
            else if (execution == "complex") { radioButtonExecutionComplex.Checked = true; }

            listBoxTemplates.Items.AddRange(allUserSettings[pos][(int)EdiDbInterface.Settings.template].ToString().Split(';'));
            listBoxClients.Items.AddRange(allUserSettings[pos][(int)EdiDbInterface.Settings.clients].ToString().Split(','));
            listBoxFiletypes.Items.AddRange(allUserSettings[pos][(int)EdiDbInterface.Settings.filetype].ToString().Split(','));
            listBoxExtras.Items.AddRange(allUserSettings[pos][(int)EdiDbInterface.Settings.extras].ToString().Split(','));

            checkBoxWriteSchedulineToNote.Checked = (bool)allUserSettings[pos][(int)EdiDbInterface.Settings.writeScheduleLineToNote];

            checkBoxConfActive.Checked = (bool)allUserSettings[pos][(int)EdiDbInterface.Settings.active];
            textBoxConfigName.Text = allUserSettings[pos][(int)EdiDbInterface.Settings.title].ToString();
            labelConfId.Text = allUserSettings[pos][(int)EdiDbInterface.Settings.id].ToString();

            checkBoxUseShippedSoDetailLines.Checked = (bool)allUserSettings[pos][(int)EdiDbInterface.Settings.useShippedLines];

            buttonSave.Enabled = false;
        }

        private void checkBoxUseShippedSoDetailLines_CheckStateChanged(object sender, EventArgs e)
        {
            EnableButtonSave();
        }
    }
}
