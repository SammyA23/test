using System.Windows.Forms;

namespace EDIAPP
{
  public partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) =>
            System.Diagnostics.Process.Start("mailto:support@mfgtech.ca");
    }
}
