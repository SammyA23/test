using System.Data;
using System.Windows.Forms;

namespace EDI
{
    public partial class frmMissingParts : Form
    {
        public frmMissingParts(DataTable dtMats)
        {
            InitializeComponent();

            this.AddMatsToGrid(dtMats);
        }
        private void AddMatsToGrid(DataTable mats)
        {
            foreach(DataRow dr in mats.Rows)
            {
                int n = this.dataGridView1.Rows.Add();
                this.dataGridView1.Rows[n].Cells[0].Value = dr[0];
                this.dataGridView1.Rows[n].Cells[1].Value = dr[1];
                this.dataGridView1.Rows[n].Cells[2].Value = dr[2];
                this.dataGridView1.Rows[n].Cells[3].Value = dr[3];
                this.dataGridView1.Rows[n].Cells[4].Value = dr[4];
                this.dataGridView1.Rows[n].Cells[5].Value = false;
            }
        }

        public string[][] MaterialsToIgnore()
	{
	    int iRowsToIgnoreCount = 0;
	    foreach(DataGridViewRow dr in this.dataGridView1.Rows)
	    {
		if (dr.Cells[5].Value.ToString() == "False")
		{
		    iRowsToIgnoreCount++;
		}
	    }

	    int iInsertPosition = 0;
	    var arrReturn = new string[iRowsToIgnoreCount][];

	    foreach(DataGridViewRow dr in this.dataGridView1.Rows)
	    {
		if (dr.Cells[5].Value.ToString() == "False")
		{
		    arrReturn[iInsertPosition] = new string[5];
		    arrReturn[iInsertPosition][0] = dr.Cells[0].Value.ToString();
		    arrReturn[iInsertPosition][1] = dr.Cells[1].Value.ToString();
		    arrReturn[iInsertPosition][2] = dr.Cells[2].Value.ToString();
		    arrReturn[iInsertPosition][3] = dr.Cells[3].Value.ToString();
		    arrReturn[iInsertPosition][4] = dr.Cells[4].Value.ToString();
		    iInsertPosition++;

		}
	    }

	    return arrReturn;
	}

        private void button2_Click(object sender, System.EventArgs e)
        {
            string myStream;

            if ( this.saveFileDialog1.ShowDialog() == DialogResult.OK )
            {
                if ( (myStream = this.saveFileDialog1.FileName) != null )
                {
                    //save list
                    string sPath;
                    string sFilename;
                    int iSeparator = myStream.LastIndexOf('\\');
                    sFilename = myStream.Substring(iSeparator+1);
                    sPath = myStream.Substring(0, iSeparator);
                    MFG.Files.CSV.CCsvWriter writer = new MFG.Files.CSV.CCsvWriter(sPath, sFilename);

                    foreach(DataGridViewRow dgvr in this.dataGridView1.Rows)
                    {
                        var aLine = new string[2];
                        aLine[0] = dgvr.Cells[0].Value.ToString();
                        aLine[1] = dgvr.Cells[1].Value.ToString();
                        writer.AddToFile(aLine);
                    }
                }
            }
        }

        private void CocherTout_Button_Click(object sender, System.EventArgs e)
        {
            if (this.dataGridView1.Rows != null && this.dataGridView1.Rows.Count > 0)
            {
                foreach (DataGridViewRow dgvr in this.dataGridView1.Rows)
                {
                    dgvr.Cells[5].Value = true;
                }
            }
        }

        private void CocherAucun_Button_Click(object sender, System.EventArgs e)
        {
            if (this.dataGridView1.Rows != null && this.dataGridView1.Rows.Count > 0)
            {
                foreach (DataGridViewRow dgvr in this.dataGridView1.Rows)
                {
                    dgvr.Cells[5].Value = false;
                }
            }
        }
    }
}
