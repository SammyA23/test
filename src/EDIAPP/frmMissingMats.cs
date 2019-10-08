using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MFG;

namespace EDIAPP
{
    public partial class frmMissingMats : Form
    {
        public frmMissingMats(DataTable dtMats)
        {
            InitializeComponent();

            AddMatsToGrid(dtMats);
        }

        private void AddMatsToGrid(DataTable mats)
        {
            foreach (DataRow dr in mats.Rows)
            {
                var rowToAdd = new DataGridViewRow();
                rowToAdd.CreateCells(this.dataGridView1);

                rowToAdd.Cells[0].Value = dr[0];
                rowToAdd.Cells[1].Value = dr[1];
                rowToAdd.Cells[2].Value = dr[2];
                rowToAdd.Cells[3].Value = dr[3];
                rowToAdd.Cells[4].Value = dr[4];
                rowToAdd.Cells[5].Value = false;

                this.dataGridView1.Rows.Add(rowToAdd);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string myStream;

            if (this.saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = this.saveFileDialog1.FileName) != null)
                {
                    var iSeparator = myStream.LastIndexOf('\\');
                    var sFilename = myStream.Substring(iSeparator + 1);
                    var sPath = myStream.Substring(0, iSeparator);

                    var writer = new MFG.Files.CSV.CCsvWriter(sPath, sFilename);

                    foreach (DataGridViewRow dgvr in this.dataGridView1.Rows)
                    {
                        var aLine = new string[2];
                        aLine[0] = dgvr.Cells[0].Value.ToString();
                        aLine[1] = dgvr.Cells[1].Value.ToString();
                        writer.AddToFile(aLine);
                    }
                }
            }
        }

        string[][] MaterialsToIgnore()
        {
            int iRowsToIgnoreCount = 0;
            foreach (DataGridViewRow dr in this.dataGridView1.Rows)
            {
                if (dr.Cells[2].Value.ToString() == "False")
                {
                    iRowsToIgnoreCount++;
                }
            }

            int iInsertPosition = 0;
            var arrReturn = new string[iRowsToIgnoreCount][];


            foreach (DataGridViewRow dr in this.dataGridView1.Rows)
            {
                if (dr.Cells[2].Value.ToString() == "False")
                {
                    arrReturn[iInsertPosition] = new string[2];
                    arrReturn[iInsertPosition][0] = dr.Cells[0].Value.ToString();
                    arrReturn[iInsertPosition][1] = dr.Cells[1].Value.ToString();
                    iInsertPosition++;
                }
            }

            return arrReturn;
        }
    }
}
