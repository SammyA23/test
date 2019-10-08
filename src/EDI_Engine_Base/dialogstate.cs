using System.Windows.Forms;

namespace EDI
{
    class DialogState
    {
        public DialogResult result;
        public frmMissingParts dialog;

        public void ThreadProcShowDialog()
        {
            this.result = dialog.ShowDialog();
        }
    }
}
