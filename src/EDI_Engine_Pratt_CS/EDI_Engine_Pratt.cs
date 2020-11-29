using MfgConnection;
using System;
using System.Data;
using System.IO;

namespace EDI
{
    public sealed class EDI_Engine_Pratt : EDI_Engine_Base
    {
        #region Members
        private MFG.MFG_File_Template m_ForecastFileTemplate;
        private MFG.Files.CSV.CCsvReader m_ForecastFileReader;
        private int m_iForecastDateFormat;
        private string[][] m_MaterialsShipped;
        private string[][] m_MaterialsScrapped;
        private bool is_single_exec;
        private DataTable m_ForecastTable;
        private string M_WRITE_SCHEDULE_LINE_TO_NOTE;
        #endregion

        #region Constructors
        public EDI_Engine_Pratt(string sForecastTemplatePath, string sForecastFilePath,
            string sDeliveryTemplatePath, string sDeliveryFilePath,
            string sDbName, string sDbType, string sCustId,
            string sAppPath, int iDelDateFormat, int iForDateFormat,
            string sSOCreation, string sSOCreationStatus,
            string sWriteSchedLineToNote, bool useShippedLines = false) :
        base(sDbName, sDbType, sCustId, sAppPath, sSOCreation, sSOCreationStatus)
        {
            is_single_exec = sDeliveryFilePath == sForecastFilePath ? true : false;

            m_DeliveryFileTemplate = new MFG.MFG_File_Template();
            m_DeliveryFileTemplate.SetFile(sDeliveryTemplatePath);
            m_DeliveryFileTemplate.Process();
            m_DeliveryFileReader = new MFG.Files.CSV.CCsvReader(sDeliveryFilePath, m_DeliveryFileTemplate);

            m_iDeliveryDateFormat = iDelDateFormat;

            if (!is_single_exec)
            {
                m_ForecastFileTemplate = new MFG.MFG_File_Template();
                m_ForecastFileTemplate.SetFile(sForecastTemplatePath);
                m_ForecastFileTemplate.Process();
                m_ForecastFileReader = new MFG.Files.CSV.CCsvReader(sForecastFilePath, m_ForecastFileTemplate);

                m_iForecastDateFormat = iForDateFormat;
            }

            M_APPPATH = sAppPath;

            M_WRITE_SCHEDULE_LINE_TO_NOTE = sWriteSchedLineToNote;

            this.useShippedLines = useShippedLines;
        }
        #endregion
        #region Methods
        private object[][] CombineArrays(ref object[][] arrDel, ref object[][] arrFor)
        {
            arrDel = SortArrayByDate(arrDel, 9);
            arrFor = SortArrayByDate(arrFor, 9);

            if (arrDel.Length > 0)
            {
                var dtLatestDelivery = System.Convert.ToDateTime(arrDel[(arrDel.Length - 1)][9]);
                arrFor = RemoveRowsEarlierTo(arrFor, dtLatestDelivery);
            }

            var iTotalRows = arrDel.Length + arrFor.Length;
            object[][] arrayCombined = new object[iTotalRows][];

            for (var i = 0; i < iTotalRows; i++)
            {
                if (i < arrDel.Length)
                {
                    arrayCombined[i] = arrDel[i];
                }
                else
                {
                    arrayCombined[i] = arrFor[i - arrDel.Length];
                }
            }

            return arrayCombined;
        }

        public void CreateTheDataTable(ref DataTable m_DataTable)
        {
            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
            System.Data.DataTable so_header_schema = conn.getSchema("SO_Header");
            System.Data.DataTable so_detail_schema = conn.getSchema("SO_Detail");

            //Create an empty table of the format required for application
            m_DataTable = new DataTable();
            string sColName;
            string sColType;
            string sColLength;

            //0 Material
            var theRow = so_detail_schema.Select("COLUMN_NAME = 'Material'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = "100"; //theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "Material", sColType,
                Convert.ToInt32(sColLength), true);

            //1 Sales_Order
            theRow = so_detail_schema.Select("COLUMN_NAME = 'Sales_Order'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "Sales Order", ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //2 PO
            theRow = so_header_schema.Select("COLUMN_NAME = 'Customer_PO'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "PO", sColType, Convert.ToInt32(sColLength), true);

            //3 Qty_Old
            theRow = so_detail_schema.Select("COLUMN_NAME = 'Order_Qty'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "Qty Old", sColType,
                Convert.ToInt32(GiveCorrectLength(sColType, sColLength)), true);

            //4 Qty_New ---.Comes from Excel file and not jbDB
            sColName = "NewQty";
            AddColumnToTable(ref m_DataTable, sColName, "Qty New", sColType,
                Convert.ToInt32(GiveCorrectLength(sColType, sColLength)), true);

            //5 Qty_Diff
            sColName = "Qty_Diff";
            AddColumnToTable(ref m_DataTable, sColName, "Qty Diff", sColType,
                Convert.ToInt32(GiveCorrectLength(sColType, sColLength)), true);

            //6 RefuseNewQty
            sColName = "RefuseNewQty";
            sColType = "varchar";
            sColLength = "50";
            AddColumnToTable(ref m_DataTable, sColName, "Refuse New Qty",
                ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //7 Qty_Diff_Value
            sColName = "Qty_Diff_Value";
            sColType = "double";
            AddColumnToTable(ref m_DataTable, sColName, "Qty Diff Value",
                ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //8 Date_Old
            theRow = so_detail_schema.Select("COLUMN_NAME = 'Promised_Date'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "Date Old", sColType,
                Convert.ToInt32(GiveCorrectLength(sColType, sColLength)), true);

            //9 Date_New
            sColName = "Date_New";
            AddColumnToTable(ref m_DataTable, sColName, "Date New", sColType,
                Convert.ToInt32(GiveCorrectLength(sColType, sColLength)), true);

            //10 Date_Diff
            sColName = "Date_Diff";
            sColType = ProperTypeString("int");
            AddColumnToTable(ref m_DataTable, sColName, "Date Diff", sColType,
                Convert.ToInt32(GiveCorrectLength(sColType, sColLength)), true);

            //11 RefuseNewDate
            sColName = "RefuseNewDate";
            sColType = "text";
            sColLength = "50";
            AddColumnToTable(ref m_DataTable, sColName, "Refuse New Date",
                ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //12 Date_Diff_Value
            sColName = "Date_Diff_Value";
            sColType = "double";
            AddColumnToTable(ref m_DataTable, sColName, "Date Diff Value",
                ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //13 DeliveryNewLocation
            sColName = "DeliveryNewLocation";
            sColType = "text";
            sColLength = "6";
            AddColumnToTable(ref m_DataTable, sColName, "Plant de livraison",
                ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //14 Excel File Detail Line
            sColName = "NewLineNumber";
            sColType = "text";
            sColLength = "6";
            AddColumnToTable(ref m_DataTable, sColName, "# de ligne du fichier",
                ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //15 SO Detail Line
            theRow = so_detail_schema.Select("COLUMN_NAME = 'SO_Line'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "# de ligne du SO_Detail",
                ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //16 SO Detail
            theRow = so_detail_schema.Select("COLUMN_NAME = 'SO_Detail'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = "6"; //theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "SO_Detail",
                ProperTypeString(sColType),
                Convert.ToInt32(sColLength), true);

            //17 SO Detail Status
            theRow = so_detail_schema.Select("COLUMN_NAME = 'Status'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "SO Detail Status",
                ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //18 Unit Price
            theRow = so_detail_schema.Select("COLUMN_NAME = 'Unit_Price'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "Prix Unitaire", sColType,
                Convert.ToInt32(this.GiveCorrectLength(sColType, sColLength)), true);

            //19 FileOrigin
            sColName = "FileOrigin";
            sColType = "text";
            sColLength = "4";
            AddColumnToTable(ref m_DataTable, sColName, "Delivery || Forecast",
                ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //20 QtyCumulated
            sColName = "Qty Cum.";
            sColType = "int";
            AddColumnToTable(ref m_DataTable, sColName, "Qté cum reçue",
                ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //21 Source
            sColName = "Source";
            sColType = "text";
            sColLength = "6";
            AddColumnToTable(ref m_DataTable, sColName, "Source", ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //22 Source
            sColName = "Client";
            sColType = "text";
            sColLength = "10";
            AddColumnToTable(ref m_DataTable, sColName, "Client", ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //23 Source
            sColName = "DeliveryComment";
            sColType = "text";
            sColLength = "2147483647";
            AddColumnToTable(ref m_DataTable, sColName, "Commentaire de livraison", ProperTypeString(sColType), Convert.ToInt32(sColLength), true);
        }


        private void FillMergedDataTable(ref DataTable m_DataTable)
        {
            var iTry = 0;
            try
            {
                var conn = new jbConnection(M_DBNAME, M_DBSERVER);

                PreemptiveVerification(ref m_DataTable);
                RemoveMaterialsToBeIgnored(ref m_DataTable, ref m_MaterialsToBeIgnored);

                var iLinesCount = m_DataTable.Rows.Count;
                var iCurrentLine = 0;

                var progressinit = new ProgressInitEngine(OnProgressInit_Event);
                progressinit(iLinesCount, "Remplissage avec les données de JobBOSS");

                while (iCurrentLine < iLinesCount)
                {
                    iTry = iCurrentLine;
                    var sCurrentMaterial = m_DataTable.Rows[iCurrentLine][0].ToString();
                    var sCurrentPO = m_DataTable.Rows[iCurrentLine][2].ToString();
                    var iMaterialStartLine = iCurrentLine;
                    var bSameMat = true;
                    var bSamePO = true;
                    var iNumberOfLineForCurrentMaterial = 1;

                    while (bSameMat && bSamePO && iCurrentLine + iNumberOfLineForCurrentMaterial < iLinesCount)
                    {
                        var sTempMaterial = m_DataTable.Rows[iCurrentLine + iNumberOfLineForCurrentMaterial][0].ToString();
                        var sTempPO = m_DataTable.Rows[iCurrentLine + iNumberOfLineForCurrentMaterial][2].ToString();

                        if (sCurrentMaterial == sTempMaterial && sCurrentPO == sTempPO)
                        {
                            iNumberOfLineForCurrentMaterial++;
                        }
                        else
                        {
                            bSameMat = false;
                        }
                    }

                    string sSelectJBLines = "SELECT Distinct(SO_Detail.Sales_Order) "
                        + "FROM SO_Detail LEFT JOIN SO_Header ON SO_Detail.Sales_Order = SO_Header.Sales_Order "
                        + "WHERE Material LIKE '" + EscapeSQLString(sCurrentMaterial) + "' AND SO_Header.Status IN ('Open', 'Hold') AND "
                        + "SO_Header.Customer LIKE '" + EscapeSQLString(M_CUSTOMER_ID) + "' AND SO_Header.Customer_PO = '" + EscapeSQLString(sCurrentPO) + "' ";

                    var jbSO = conn.GetData(sSelectJBLines);

                    if (jbSO.Rows.Count > 1)
                    {
                        System.Windows.Forms.MessageBox.Show("Vous avez plus d'un Sales Order pour la pièce " + sCurrentMaterial + " pour votre "
                            + "client EDI. Veuillez vous assurer que nous n'en avons juste un svp.");

                        System.Threading.Thread.CurrentThread.Abort();
                    }

                    if (jbSO.Rows.Count == 1)
                    {
                        sSelectJBLines = "SELECT SO_Detail.Sales_Order, SO_Detail.SO_Line, SO_Detail.Order_Qty, "
                        + "SO_Detail.Promised_Date, Address.Ship_To_ID, SO_Detail.Unit_Price, SO_Detail.SO_Detail, "
                        + "SO_Detail.Status FROM SO_Detail LEFT JOIN Address ON SO_Detail.Ship_To = Address.Address "
                        + "WHERE SO_Detail.Sales_Order = '" + EscapeSQLString(jbSO.Rows[0][0]?.ToString()) + "'";

                        if (useShippedLines)
                        {
                            sSelectJBLines += " AND (SO_Detail.Status NOT IN ('Closed'))";
                        }
                        else
                        {
                            sSelectJBLines += " AND (SO_Detail.Status NOT IN ('Shipped', 'Closed'))";
                        }

                        sSelectJBLines += " AND (SO_Detail.Material LIKE '" + EscapeSQLString(sCurrentMaterial) + "')"
                        + "ORDER BY SO_Detail.SO_Detail";
                    }


                    var jbLines = conn.GetData(sSelectJBLines);
                    var iJbLinesCount = jbLines.Rows.Count;
                    var iCurrentJBLine = 0;

                    while (iCurrentJBLine < iNumberOfLineForCurrentMaterial)
                    {
                        if (iCurrentJBLine < iJbLinesCount)
                        {
                            m_DataTable.Rows[iCurrentLine + iCurrentJBLine][1] = jbLines.Rows[iCurrentJBLine][0];
                            m_DataTable.Rows[iCurrentLine + iCurrentJBLine][3] = jbLines.Rows[iCurrentJBLine][2];
                            m_DataTable.Rows[iCurrentLine + iCurrentJBLine][8] = jbLines.Rows[iCurrentJBLine][3];
                            m_DataTable.Rows[iCurrentLine + iCurrentJBLine][15] = jbLines.Rows[iCurrentJBLine][1];
                            m_DataTable.Rows[iCurrentLine + iCurrentJBLine][16] = jbLines.Rows[iCurrentJBLine][6];
                            m_DataTable.Rows[iCurrentLine + iCurrentJBLine][17] = jbLines.Rows[iCurrentJBLine][7];
                            m_DataTable.Rows[iCurrentLine + iCurrentJBLine][18] = jbLines.Rows[iCurrentJBLine][5];

                            PerformCalculationsForRow(ref m_DataTable, iCurrentLine + iCurrentJBLine);
                        }
                        else
                        {
                            m_DataTable.Rows[iCurrentLine + iCurrentJBLine][1] = "N/A";
                            m_DataTable.Rows[iCurrentLine + iCurrentJBLine][3] = "0";
                            m_DataTable.Rows[iCurrentLine + iCurrentJBLine][8] = m_DataTable.Rows[iCurrentLine + iCurrentJBLine][9];
                            m_DataTable.Rows[iCurrentLine + iCurrentJBLine][15] = "0";
                            m_DataTable.Rows[iCurrentLine + iCurrentJBLine][16] = "0";

                            string s = "SELECT Material, Selling_Price FROM Material WHERE Material LIKE '" + EscapeSQLString(sCurrentMaterial) + "'";

                            var dtMaterialInfo = conn.GetData(s);
                            m_DataTable.Rows[iCurrentLine + iCurrentJBLine][18] = dtMaterialInfo.Rows[0][1].ToString();
                            PerformCalculationsForRow(ref m_DataTable, iCurrentLine + iCurrentJBLine);
                        }

                        iCurrentJBLine++;
                    }

                    while (iCurrentJBLine < iJbLinesCount) //there is more jb lines than cvs....process the extra ones
                    {
                        var dataRow = m_DataTable.NewRow();

                        var sJBDate = FormatJBDate(jbLines.Rows[iCurrentJBLine][3].ToString());
                        var jbDate = new DateTime((int)sJBDate[2], (int)sJBDate[1], (int)sJBDate[0]);


                        dataRow[0] = sCurrentMaterial;
                        dataRow[1] = jbLines.Rows[iCurrentJBLine][0]; //Sales Order
                        dataRow[2] = sCurrentPO;
                        dataRow[3] = jbLines.Rows[iCurrentJBLine][2]; //Qty
                        dataRow[4] = "0";
                        dataRow[5] = "0";
                        dataRow[6] = DBNull.Value;
                        dataRow[7] = "0";
                        dataRow[8] = jbDate.ToString("d"); //Date
                        dataRow[9] = dataRow[8];
                        dataRow[10] = "0";
                        dataRow[11] = DBNull.Value;
                        dataRow[12] = "0";
                        dataRow[13] = "0";
                        dataRow[14] = "0";
                        dataRow[15] = jbLines.Rows[iCurrentJBLine][1];
                        dataRow[16] = jbLines.Rows[iCurrentJBLine][6];
                        dataRow[18] = jbLines.Rows[iCurrentJBLine][5];
                        dataRow[19] = "N/A";
                        dataRow[20] = "0";
                        dataRow[21] = "JB";
                        dataRow[22] = M_CUSTOMER_ID;

                        m_DataTable.Rows.Add(dataRow);
                        PerformCalculationsForRow(ref m_DataTable, m_DataTable.Rows.Count - 1);

                        iCurrentJBLine++;
                    }


                    var progressreport = new ProgressReportEngine(OnProgressReport_Event);
                    progressreport(iNumberOfLineForCurrentMaterial);
                    iCurrentLine = iCurrentLine + iNumberOfLineForCurrentMaterial;
                }
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                m_DataTable.Rows.Clear();
                System.Windows.Forms.MessageBox.Show("Le format de date sélectionner est inadéquat.\nVeuillez sélectionner le bon format de date.");
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Ligne actuelle:\n" + iTry + "\nMessage Recherché? :\n" + e.Message);
                System.Threading.Thread.CurrentThread.Abort();
            }
        }


        private void GatherShippedParts(ref MFG.Files.CSV.CCsvReader m_EdiCsvFileReader, int DateFormat)
        {
            m_EdiCsvFileReader.MoveToFirstRow();

            string[] currentRow;
            int iRowLength;
            var iShippedCounter = 0;
            var iScrappedCounter = 0;
            var lastRow = new string[1];

            try
            {
                var bFirstExecution = true;
                while (m_EdiCsvFileReader.Read())
                {
                    currentRow = m_EdiCsvFileReader.GetCurrentRow();
                    iRowLength = currentRow.Length;

                    if (!bFirstExecution && lastRow[iRowLength - 1].Length >= 3)
                    {
                        if (lastRow[iRowLength - 1].Substring(0, 3) == "LV*")
                        {
                            iShippedCounter++;
                        }
                        else if (lastRow[iRowLength - 1].Substring(0, 3) == "SC*" && lastRow[0] != currentRow[0])
                        {
                            iScrappedCounter++;
                        }
                    }

                    if (bFirstExecution)
                    {
                        bFirstExecution = false;
                    }

                    lastRow = currentRow;
                }

                m_EdiCsvFileReader.MoveToFirstRow();
                this.m_MaterialsShipped = new string[iShippedCounter][];
                this.m_MaterialsScrapped = new string[iScrappedCounter][];
                iShippedCounter = 0;
                iScrappedCounter = 0;
                bFirstExecution = true;
                lastRow = new string[1];

                while (m_EdiCsvFileReader.Read())
                {
                    currentRow = m_EdiCsvFileReader.GetCurrentRow();
                    iRowLength = currentRow.Length;

                    if (!bFirstExecution && lastRow[iRowLength - 1].Length >= 3)
                    {
                        if (lastRow[iRowLength - 1].Substring(0, 3) == "LV*")
                        {
                            string sPart = this.AdjustMaterial(lastRow[0]);
                            string sPO;

                            if (lastRow[2].ToString().Length > 20)
                            {
                                sPO = lastRow[2].ToString().Substring(0, 20);
                            }
                            else
                            {
                                sPO = lastRow[2];
                            }

                            var sExcelDate = this.FormatExcelDate(lastRow[5], DateFormat);
                            var excelDate = new DateTime((int)sExcelDate[2], (int)sExcelDate[1], (int)sExcelDate[0]);


                            this.m_MaterialsShipped[iShippedCounter] = new string[4];
                            this.m_MaterialsShipped[iShippedCounter][0] = sPart;
                            this.m_MaterialsShipped[iShippedCounter][1] = sPO;
                            this.m_MaterialsShipped[iShippedCounter][2] = excelDate.ToString("yyyy'/'MM'/'dd");
                            this.m_MaterialsShipped[iShippedCounter][3] = "N";
                            iShippedCounter++;
                        }
                        else if (lastRow[iRowLength - 1].Substring(0, 3) == "SC*" && lastRow[0] != currentRow[0])
                        {
                            string sPart = this.AdjustMaterial(lastRow[0]);
                            string sPO;

                            if (lastRow[2].ToString().Length > 20)
                            {
                                sPO = lastRow[2].ToString().Substring(0, 20);
                            }
                            else
                            {
                                sPO = lastRow[2];
                            }

                            var sExcelDate = this.FormatExcelDate(lastRow[5], DateFormat);
                            var excelDate = new DateTime((int)sExcelDate[2], (int)sExcelDate[1], (int)sExcelDate[0]);

                            this.m_MaterialsScrapped[iScrappedCounter] = new string[4];
                            this.m_MaterialsScrapped[iScrappedCounter][0] = sPart;
                            this.m_MaterialsScrapped[iScrappedCounter][1] = sPO;
                            this.m_MaterialsScrapped[iScrappedCounter][2] = excelDate.ToString("d");
                            this.m_MaterialsScrapped[iScrappedCounter][3] = "N";
                            iScrappedCounter++;
                        }
                    }

                    if (bFirstExecution)
                    {
                        bFirstExecution = false;
                    }

                    lastRow = currentRow;
                }
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                System.Windows.Forms.MessageBox.Show("ArgumentOutOfRange");

                throw e;
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("ArgumentOutOfRange");
                throw e;
            }
        }
        public override DataTable GetMergedTable()
        {
            return this.m_MergedTable;
        }

        private object[][] GetRows(ref DataTable dtTemp, ref string sMat, ref string sPO)
        {
            var iMatchCounter = 0;

            for (var i = 0; i < dtTemp.Rows.Count; i++)
            {
                if (dtTemp.Rows[i][0].ToString() == sMat && dtTemp.Rows[i][2].ToString() == sPO)
                {
                    iMatchCounter++;
                }
            }

            var arrayRows = new object[iMatchCounter][];
            var iInsertPosition = 0;

            if (iMatchCounter > 0)
            {
                for (int i = 0; i < dtTemp.Rows.Count; i++)
                {
                    if (dtTemp.Rows[i][0].ToString() == sMat && dtTemp.Rows[i][2].ToString() == sPO)
                    {
                        arrayRows[iInsertPosition] = new object[23];

                        arrayRows[iInsertPosition][0] = dtTemp.Rows[i][0];
                        arrayRows[iInsertPosition][1] = dtTemp.Rows[i][1];
                        arrayRows[iInsertPosition][2] = dtTemp.Rows[i][2];
                        arrayRows[iInsertPosition][3] = dtTemp.Rows[i][3];
                        arrayRows[iInsertPosition][4] = dtTemp.Rows[i][4];
                        arrayRows[iInsertPosition][5] = dtTemp.Rows[i][5];
                        arrayRows[iInsertPosition][6] = dtTemp.Rows[i][6];
                        arrayRows[iInsertPosition][7] = dtTemp.Rows[i][7];
                        arrayRows[iInsertPosition][8] = dtTemp.Rows[i][8];
                        arrayRows[iInsertPosition][9] = dtTemp.Rows[i][9];
                        arrayRows[iInsertPosition][10] = dtTemp.Rows[i][10];
                        arrayRows[iInsertPosition][11] = dtTemp.Rows[i][11];
                        arrayRows[iInsertPosition][12] = dtTemp.Rows[i][12];
                        arrayRows[iInsertPosition][13] = dtTemp.Rows[i][13];

                        if (!String.IsNullOrWhiteSpace(dtTemp.Rows[i][14].ToString()))
                        {
                            arrayRows[iInsertPosition][14] = dtTemp.Rows[i][14];
                        }
                        else
                        {
                            arrayRows[iInsertPosition][14] = "00010";
                        }

                        arrayRows[iInsertPosition][15] = dtTemp.Rows[i][15];
                        arrayRows[iInsertPosition][16] = dtTemp.Rows[i][16];
                        arrayRows[iInsertPosition][17] = dtTemp.Rows[i][17];
                        arrayRows[iInsertPosition][18] = dtTemp.Rows[i][18];
                        arrayRows[iInsertPosition][19] = dtTemp.Rows[i][19];
                        arrayRows[iInsertPosition][20] = dtTemp.Rows[i][20];
                        arrayRows[iInsertPosition][21] = dtTemp.Rows[i][21];
                        arrayRows[iInsertPosition][22] = dtTemp.Rows[i][22];

                        iInsertPosition++;
                    }
                }
            }
            else
            {
                arrayRows = new object[0][];
            }

            return arrayRows;
        }

        private bool HasBeenScrapped(string sPart, string sPO, DateTime dateFromFile, int iDateFormat)
        {
            var iCurrent = 0;

            while (iCurrent < this.m_MaterialsScrapped.Length)
            {
                var aDateScrapped = this.FormatExcelDate(this.m_MaterialsScrapped[iCurrent][2], 1);
                var dateSCrapped = new DateTime((int)aDateScrapped[2], (int)aDateScrapped[1], (int)aDateScrapped[0]);

                if ((this.m_MaterialsScrapped[iCurrent][3] == "N") && (sPart == this.m_MaterialsScrapped[iCurrent][0] && sPO == this.m_MaterialsScrapped[iCurrent][1] && dateFromFile <= dateSCrapped))
                {
                    this.m_MaterialsScrapped[iCurrent][3] = "Y";
                    return true;
                }

                iCurrent++;
            }

            return false;
        }
        private bool HasBeenShipped(string sPart, string sPO, DateTime dateFromFile, int iDateFormat)
        {
            var iCurrent = 0;

            while (iCurrent < this.m_MaterialsShipped.Length)
            {
                var aDateShipped = this.FormatExcelDate(this.m_MaterialsShipped[iCurrent][2], iDateFormat);
                var dateShipped = new DateTime((int)aDateShipped[2], (int)aDateShipped[1], (int)aDateShipped[0]);

                if ((this.m_MaterialsShipped[iCurrent][3] == "N") && (sPart == this.m_MaterialsShipped[iCurrent][0] && sPO == this.m_MaterialsShipped[iCurrent][1] && dateFromFile <= dateShipped))
                {
                    this.m_MaterialsShipped[iCurrent][3] = "Y";
                    return true;
                }

                iCurrent++;
            }

            return false;
        }
        public void InsertCsvRows(ref DataTable m_DataTable, ref MFG.Files.CSV.CCsvReader m_EdiCsvFileReader, int DateFormat, string sFileOrigin)
        {
            m_EdiCsvFileReader.MoveToFirstRow();

            try
            {
                while (m_EdiCsvFileReader.Read())
                {
                    var currentRow = m_EdiCsvFileReader.GetCurrentRow();
                    var iRowLength = currentRow.Length;
                    var bShipped = false;
                    var bScrapped = false;

                    if (currentRow[iRowLength - 1].Length >= 3)
                    {
                        if (currentRow[iRowLength - 1].Substring(0, 3) == "LV*")
                        {
                            bShipped = true;
                        }
                        else if (currentRow[iRowLength - 1].Substring(0, 3) == "LV*")
                        {
                            bScrapped = true;
                        }
                    }

                    var sPart = this.AdjustMaterial(currentRow[0]);
                    string sPO;

                    if (currentRow[2].ToString().Length > 20)
                    {
                        sPO = currentRow[2].ToString().Substring(0, 20);
                    }
                    else
                    {
                        sPO = currentRow[2];
                    }

                    var sExcelDate = this.FormatExcelDate(currentRow[5], DateFormat);
                    var excelDate = new DateTime((int)sExcelDate[2], (int)sExcelDate[1], (int)sExcelDate[0]);

                    if ((sFileOrigin == "D" && !(bShipped || bScrapped)) || (sFileOrigin == "F" && !(this.HasBeenShipped(sPart, sPO, excelDate, DateFormat) || this.HasBeenScrapped(sPart, sPO, excelDate, DateFormat))))
                    {
                        var dataRow = m_DataTable.NewRow();

                        //part number
                        dataRow[0] = this.AdjustMaterial(currentRow[0]);

                        //customer po
                        if (currentRow[2].ToString().Length > 20)
                        {
                            dataRow[2] = currentRow[2].ToString().Substring(0, 20);
                        }//end if
                        else
                        {
                            dataRow[2] = currentRow[2];
                        }//end if

                        if (dataRow[2].ToString().Contains("FORECAST"))
                        {
                            dataRow[2] = dataRow[2].ToString().Trim(' ');
                        }

                        //quantity new
                        dataRow[4] = currentRow[4];

                        //date new
                        sExcelDate = this.FormatExcelDate(currentRow[5], DateFormat);
                        excelDate = new DateTime((int)sExcelDate[2], (int)sExcelDate[1], (int)sExcelDate[0]);
                        dataRow[9] = excelDate.ToString("d");

                        //delivery plant
                        dataRow[13] = currentRow[1];
                        //detail line
                        dataRow[14] = currentRow[3];

                        dataRow[19] = sFileOrigin;

                        dataRow[21] = "CSV";
                        dataRow[22] = this.M_CUSTOMER_ID;

                        if (this.M_WRITE_SCHEDULE_LINE_TO_NOTE.ToLower() == "true")
                        {
                            //we need to write the value of schedule_line into a column
                            //therefore we need something like:
                            //dataRow[x] = currentRow[y];
                            //where
                            //x = whichever column # we add to the createdatatable method
                            //y = whichever column is the one container the value we want in the reader
                            //at the moment none of those columns have been added to either containers....
                        }


                        m_DataTable.Rows.Add(dataRow);
                    }
                }
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                System.Windows.Forms.MessageBox.Show("ArgumentOutOfRange");
                throw e;
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Autre chose...\n" + e.Message);
                throw e;
            }
        }

        private void MergeDataFiles(ref DataTable mergedDT)
        {
            var dtDelivery = this.m_DeliveryTable;
            var dtForecast = this.m_ForecastTable;

            var progressinit = new ProgressInitEngine(OnProgressInit_Event);
            progressinit(this.m_DeliveryTable.Rows.Count + this.m_ForecastTable.Rows.Count, "Jummelage de Delivery et Forecast");
            var progressreport = new ProgressReportEngine(OnProgressReport_Event);

            string sCurrentMat;
            string sCurrentPO;

            object[][] arrayDeliveryRows;
            object[][] arrayForecastRows;
            object[][] arrayMergedRows;

            while (dtDelivery.Rows.Count > 0 || dtForecast.Rows.Count > 0)
            {
                if (dtDelivery.Rows.Count > 0)
                {
                    sCurrentMat = dtDelivery.Rows[0][0].ToString();
                    sCurrentPO = dtDelivery.Rows[0][2].ToString();
                }
                else
                {
                    sCurrentMat = dtForecast.Rows[0][0].ToString();
                    sCurrentPO = dtForecast.Rows[0][2].ToString();
                }


                arrayDeliveryRows = this.GetRows(ref dtDelivery, ref sCurrentMat, ref sCurrentPO);
                this.RemoveRows(ref dtDelivery, ref sCurrentMat, ref sCurrentPO);

                arrayForecastRows = this.GetRows(ref dtForecast, ref sCurrentMat, ref sCurrentPO);
                this.RemoveRows(ref dtForecast, ref sCurrentMat, ref sCurrentPO);

                arrayMergedRows = this.CombineArrays(ref arrayDeliveryRows, ref arrayForecastRows);

                for (var i = 0; i < arrayMergedRows.Length; i++)
                {
                    mergedDT.Rows.Add(arrayMergedRows[i]);
                }

                progressreport(1);
            }
        }

        private void MergeSingleDataFile(ref DataTable mergedDT)
        {
            var dtDelivery = this.m_DeliveryTable;

            var progressinit = new ProgressInitEngine(OnProgressInit_Event);
            progressinit(this.m_DeliveryTable.Rows.Count, "Groupage Forecast");
            var progressreport = new ProgressReportEngine(OnProgressReport_Event);

            string sCurrentMat;
            string sCurrentPO;

            object[][] arrayForecastRows;
            object[][] arrayMergedRows;

            while (dtDelivery.Rows.Count > 0)
            {
                sCurrentMat = dtDelivery.Rows[0][0].ToString();
                sCurrentPO = dtDelivery.Rows[0][2].ToString();

                arrayForecastRows = this.GetRows(ref dtDelivery, ref sCurrentMat, ref sCurrentPO);
                this.RemoveRows(ref dtDelivery, ref sCurrentMat, ref sCurrentPO);

                arrayMergedRows = arrayForecastRows;

                for (var i = 0; i < arrayMergedRows.Length; i++)
                {
                    mergedDT.Rows.Add(arrayMergedRows[i]);
                }

                progressreport(1);
            }
        }
        protected override void PerformCalculationsForRow(ref DataTable m_DataTable, int iCurrentRow)
        {
            //Qty Diff
            object oQty = m_DataTable.Rows[iCurrentRow][3];
            var iJBQty = Convert.ToInt32(oQty);
            oQty = m_DataTable.Rows[iCurrentRow][4];
            var iFileQty = Convert.ToInt32(oQty);
            m_DataTable.Rows[iCurrentRow][5] = iFileQty - iJBQty; //Qty diff


            //Qty Diff Value
            double unitPrice;

            unitPrice = Convert.ToDouble(m_DataTable.Rows[iCurrentRow][18]);
            oQty = m_DataTable.Rows[iCurrentRow][5];
            var dDiffQty = Convert.ToDouble(oQty);
            m_DataTable.Rows[iCurrentRow][7] = dDiffQty * unitPrice; //Qty diff value


            //Date Diff
            var sTempDate = this.FormatJBDate(m_DataTable.Rows[iCurrentRow][9].ToString());
            var cvsDate = new DateTime((int)sTempDate[2], (int)sTempDate[1], (int)sTempDate[0]);

            sTempDate = this.FormatJBDate(m_DataTable.Rows[iCurrentRow][8].ToString());
            var jbDate = new DateTime((int)sTempDate[2], (int)sTempDate[1], (int)sTempDate[0]);

            m_DataTable.Rows[iCurrentRow][10] = cvsDate.Subtract(jbDate).TotalDays;

            //Date Diff Value
            dDiffQty = Convert.ToDouble(m_DataTable.Rows[iCurrentRow][3]);
            if (Convert.ToInt32(m_DataTable.Rows[iCurrentRow][10]) > 0)
            {
                m_DataTable.Rows[iCurrentRow][12] = 0 - (dDiffQty * unitPrice); //date diff value
            }
            else if (Convert.ToInt32(m_DataTable.Rows[iCurrentRow][10]) < 0)
            {
                m_DataTable.Rows[iCurrentRow][12] = dDiffQty * unitPrice; //date diff value
            }
            else
            {
                m_DataTable.Rows[iCurrentRow][12] = 0; //date diff value
            }
        }
        public override void Read()
        {
            var progressinit = new ProgressInitEngine(OnProgressInit_Event);
            progressinit(100, "Création/Remplissage des tables Delivery et Forecast");

            if (is_single_exec)
            {
                GatherShippedParts(ref m_DeliveryFileReader, m_iDeliveryDateFormat);
            }
            else
            {
                GatherShippedParts(ref m_ForecastFileReader, m_iForecastDateFormat);
            }

            var progressreport = new ProgressReportEngine(OnProgressReport_Event);
            progressreport(25);

            CreateTheDataTable(ref m_DeliveryTable);
            InsertCsvRows(ref m_DeliveryTable, ref m_DeliveryFileReader, m_iDeliveryDateFormat, "D");

            progressreport(25);

            if (!is_single_exec)
            {
                CreateTheDataTable(ref m_ForecastTable);
                InsertCsvRows(ref m_ForecastTable, ref m_ForecastFileReader, m_iForecastDateFormat, "F");
            }

            progressreport(50);

            CreateTheDataTable(ref m_MergedTable);

            //merge tables if we use delivery and forecast files otherwise just use the delivery table
            if (!is_single_exec)
            {
                MergeDataFiles(ref m_MergedTable);
            }
            else
            {
                MergeSingleDataFile(ref m_MergedTable);
            }

            //fillmergedtable here
            FillMergedDataTable(ref m_MergedTable);

            var completeproc = new CompleteEngineProcess(OnComplete_Event);
            completeproc();
        }
        private object[][] RemoveRowAtIndex(object[][] arrayToFilter, int iIndexToRemove)
        {
            var arrTemp = new object[arrayToFilter.Length - 1][];
            var iInsertPosition = 0;

            for (var i = 0; i < arrayToFilter.Length; i++)
            {
                if (i != iIndexToRemove)
                {
                    arrTemp[iInsertPosition] = arrayToFilter[i];
                    iInsertPosition++;
                }
            }

            return arrTemp;
        }
        private object[][] RemoveRowsEarlierTo(object[][] arrayToFilter, DateTime dtFilterDate)
        {
            var bContainsRowsToRemove = true;
            var iIndexToRemove = 0;
            var bFoundARowToRemove = false;

            while (bContainsRowsToRemove)
            {
                for (var i = 0; i < arrayToFilter.Length; i++)
                {
                    if (dtFilterDate >= System.Convert.ToDateTime(arrayToFilter[i][9]))
                    {
                        iIndexToRemove = i;
                        bFoundARowToRemove = true;
                    }
                }

                if (bFoundARowToRemove)
                {
                    arrayToFilter = this.RemoveRowAtIndex(arrayToFilter, iIndexToRemove);
                    bFoundARowToRemove = false;
                }
                else
                {
                    bContainsRowsToRemove = false;
                }
            }

            return arrayToFilter;
        }
        private object[][] SortArrayByDate(object[][] arrayToSort, int iDateColumnToSortBy)
        {
            if (arrayToSort.Length > 0)
            {
                var arraySorted = new object[arrayToSort.Length][];
                var earliest = new object[arrayToSort[0].Length];
                var sorted = new int[arrayToSort.Length];

                var earliestIndex = 0;
                var iSorted = 0;
                var bFirst = true;

                for (int i = 0; i < sorted.Length; i++)
                {
                    sorted[i] = arrayToSort.Length;
                }

                while (iSorted < arrayToSort.Length)
                {
                    bFirst = true;

                    for (int i = 0; i < arrayToSort.Length; i++)
                    {
                        if (System.Array.IndexOf(sorted, i) < 0)
                        {
                            if (bFirst)
                            {
                                earliestIndex = i;
                                earliest = arrayToSort[earliestIndex];
                                bFirst = false;
                            }
                            else if (System.Convert.ToDateTime(earliest[iDateColumnToSortBy]) >= System.Convert.ToDateTime(arrayToSort[i][iDateColumnToSortBy]))
                            {
                                earliestIndex = i;
                                earliest = arrayToSort[earliestIndex];
                            }
                        }
                    }

                    arraySorted[iSorted] = earliest;
                    sorted[iSorted] = earliestIndex;

                    iSorted++;
                }

                return arraySorted;
            }
            else
            {
                return arrayToSort;
            }
        }
        public override void Write()
        {
            var conn = new jbConnection(M_DBNAME, M_DBSERVER);

            string sCurrentPO;
            string sCurrentPart;
            string sQuery;
            string sSO;

            var iCurrentLine = 0;

            /* ********* addition of code to perform creation or not of SOs ********* */
            var bIsFirstUncreate = true;

            string sDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/EDI-History/";
            string sFileNameStart = "Refus-" + System.DateTime.Now.ToShortDateString().Replace("/", "-");

            string sFileName = "";

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

            var sUncreatedName = "";
            if ((this.M_SO_CREATION == "ON" && this.M_SO_CREATION_STATUS == "Hold") || this.M_SO_CREATION != "ON")
            {
                var sUncreatedStart = "SO_NonCreer-" + System.DateTime.Now.ToShortDateString().Replace("/", "-");
                bFoundFilename = false;
                sFileNumber = "0001";

                while (!bFoundFilename)
                {
                    var file = new FileInfo(sDirPath + sUncreatedStart + "-" + sFileNumber + ".csv");

                    if (file.Exists)
                    {
                        sFileNumber = (Convert.ToUInt32(sFileNumber) + 1).ToString().PadLeft(4, '0');
                    }
                    else
                    {
                        bFoundFilename = true;
                        sUncreatedName = sUncreatedStart + "-" + sFileNumber + ".csv";
                    }
                }
            }

            var refusalWriter = new MFG.Files.CSV.CCsvWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EDI-History\\", sFileName);
            var uncreatedWriter = new MFG.Files.CSV.CCsvWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EDI-History\\", sUncreatedName);
            /* ***END*** addition of code to perform creation or not of SOs ********* */
            while (iCurrentLine < this.m_MergedTable.Rows.Count)
            {
                if (this.m_MergedTable.Rows[iCurrentLine][4].ToString() == "0")
                {
                    conn.SetData("DELETE FROM Delivery WHERE Delivery.SO_Detail = " + EscapeSQLString(this.m_MergedTable.Rows[iCurrentLine][16].ToString()));
                    conn.SetData("DELETE FROM SO_Detail WHERE SO_Detail.SO_Detail = " + EscapeSQLString(this.m_MergedTable.Rows[iCurrentLine][16].ToString()));
                    this.m_MergedTable.Rows[iCurrentLine].Delete();
                }
                else
                {
                    iCurrentLine++;
                }
            }
            iCurrentLine = 0;
            while (iCurrentLine < this.m_MergedTable.Rows.Count)
            {
                sCurrentPart = this.m_MergedTable.Rows[iCurrentLine][0].ToString();
                sSO = this.m_MergedTable.Rows[iCurrentLine][1].ToString();
                sCurrentPO = this.m_MergedTable.Rows[iCurrentLine][2].ToString();

                var iPOLines = 1;

                while ((iCurrentLine + iPOLines) < this.m_MergedTable.Rows.Count)
                {
                    if ((this.m_MergedTable.Rows[iCurrentLine + iPOLines][0].ToString() == sCurrentPart) && (this.m_MergedTable.Rows[iCurrentLine + iPOLines][2].ToString() == sCurrentPO))
                    {
                        iPOLines++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (sSO != "" && sSO != "N/A")
                {
                    sQuery = "SELECT SO_Detail.Sales_Order, SO_Detail.SO_Line, SO_Detail.Order_Qty, "
                    + "SO_Detail.Promised_Date, Address.Ship_To_ID, SO_Detail.Unit_Price, "
                    + "SO_Detail.SO_Detail, SO_Detail.Status "
                    + "FROM SO_Detail LEFT JOIN Address ON SO_Detail.Ship_To = Address.Address "
                    + "WHERE SO_Detail.Sales_Order = '" + EscapeSQLString(sSO) + "' AND (SO_Detail.Material LIKE '"
                    + sCurrentPart + "') ";

                    if (useShippedLines)
                    {
                        sQuery += " AND SO_Detail.Status NOT IN('Closed')";
                    }
                    else
                    {
                        sQuery += " AND SO_Detail.Status NOT IN('Shipped', 'Closed')";
                    }

                    sQuery += " ORDER BY SO_Detail.SO_Detail";

                    var jbTable = conn.GetData(sQuery);

                    string taxCode = GetTaxCodeFromSO(sSO);

                    for (var i = 0; i < iPOLines; i++)
                    {
                        if (i < jbTable.Rows.Count)
                        {
                            if (this.m_MergedTable.Rows[iCurrentLine + i][4].ToString() != "0")
                            {
                                var sUpdate = "UPDATE SO_Detail SET ";

                                string sUpdateDelivery;
                                var bDeliveryUpdate = false;
                                sQuery = "SELECT SO_Detail FROM Delivery WHERE SO_Detail = " + EscapeSQLString(jbTable.Rows[i][6].ToString());
                                var existingDeliveryTable = conn.GetData(sQuery);
                                if (existingDeliveryTable.Rows.Count > 0)
                                {
                                    bDeliveryUpdate = true;
                                }

                                sUpdateDelivery = "UPDATE Delivery SET ";

                                var sDate = this.m_MergedTable.Rows[iCurrentLine + i][9].ToString();
                                var sQty = this.m_MergedTable.Rows[iCurrentLine + i][4].ToString();
                                var sLine = this.m_MergedTable.Rows[iCurrentLine + i][14].ToString();

                                if (sLine == "" && sCurrentPO == "FORECAST")
                                {
                                    sLine = "00010";
                                }

                                sUpdate += "Promised_Date = '" + EscapeSQLString(sDate) + "'";
                                sUpdateDelivery += "Promised_Date = '" + EscapeSQLString(sDate) + "' , Requested_Date = '" + EscapeSQLString(sDate) + "' ";


                                sUpdate += ", Order_Qty = " + EscapeSQLString(sQty) + ", Deferred_Qty = " + EscapeSQLString(sQty);
                                sUpdateDelivery += ", Promised_Quantity = " + EscapeSQLString(sQty) + ", Remaining_Quantity = " + EscapeSQLString(sQty) + " ";

                                var sSelect = "SELECT Unit_Price FROM SO_Detail WHERE SO_Detail.SO_Detail = " + EscapeSQLString(jbTable.Rows[i][6].ToString());
                                var dTUnitPrice = conn.GetData(sSelect);

                                object oPrice = dTUnitPrice.Rows[0][0];
                                var dUnitPrice = Convert.ToDouble(oPrice);
                                var iQty = Convert.ToDouble(sQty);

                                string sNewPrice = (iQty * dUnitPrice).ToString();

                                sUpdate += ", Total_Price = " + sNewPrice;

                                sUpdate += ", SO_Line = " + sLine;

                                sSelect = "SELECT Sales_Code FROM Material WHERE Material LIKE '" + EscapeSQLString(sCurrentPart) + "'";
                                var dTSalesCode = conn.GetData(sSelect);
                                object oSalesCode = dTSalesCode.Rows[0][0];

                                if (oSalesCode != System.DBNull.Value)
                                {
                                    sUpdate += ", Sales_Code = '" + EscapeSQLString(oSalesCode.ToString()) + "' ";
                                }

                                if (!String.IsNullOrWhiteSpace(taxCode))
                                {
                                    sUpdate += ", Tax_Code = '" + EscapeSQLString(taxCode) + "' ";
                                }

                                sUpdate += " WHERE SO_Detail = " + EscapeSQLString(jbTable.Rows[i][6].ToString());
                                sUpdateDelivery += "WHERE SO_Detail = " + EscapeSQLString(jbTable.Rows[i][6].ToString());

                                conn.SetData(sUpdate);

                                if (!bDeliveryUpdate)
                                {
                                    sUpdateDelivery = "INSERT INTO Delivery (SO_Detail, Requested_Date, Promised_Date, Promised_Quantity, Remaining_Quantity, ObjectID) VALUES";

                                    sUpdateDelivery += " (" + EscapeSQLString(jbTable.Rows[i][6].ToString()) + ", '" + EscapeSQLString(sDate) + "' , '" + EscapeSQLString(sDate) + "' ," + EscapeSQLString(sQty) + ", " + EscapeSQLString(sQty) +
                                    ", '" + EscapeSQLString(System.Guid.NewGuid().ToString()) + "')";
                                }

                                if (jbTable.Rows[i][7].ToString() != "Shipped")
                                {
                                    conn.SetData(sUpdateDelivery);
                                }
                            }//If newQty end
                            else //Get rid of extra JB lines
                            {
                                conn.SetData("DELETE FROM Delivery WHERE Delivery.SO_Detail = " + EscapeSQLString(jbTable.Rows[jbTable.Rows.Count - 1][6].ToString()));
                                conn.SetData("DELETE FROM SO_Detail WHERE SO_Detail.SO_Detail = " + EscapeSQLString(jbTable.Rows[jbTable.Rows.Count - 1][6].ToString()));
                                jbTable.Rows.Remove(jbTable.Rows[jbTable.Rows.Count - 1]);
                            }
                        }
                        else if (this.m_MergedTable.Rows[iCurrentLine + i][4] != System.DBNull.Value && this.m_MergedTable.Rows[iCurrentLine + i][4].ToString() != "0")
                        {
                            if (m_MergedTable.Rows[iCurrentLine + i][4] != System.DBNull.Value
                                && m_MergedTable.Rows[iCurrentLine + i][4].ToString() != "0"
                                && System.Convert.ToDouble(
                                    m_MergedTable.Rows[iCurrentLine + i][4].ToString()) > 0.00)
                            {
                                AddSODetailLineWithTaxCode(m_MergedTable.Rows[iCurrentLine + i],
                                    sSO, m_MergedTable.Rows[iCurrentLine + i][14].ToString());
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("Not supposed to happen..."
                                + "Current PO loop counter is greater than JBTable count and "
                                + "there is nothing to insert!");
                        }
                    }

                    iCurrentLine += iPOLines;
                }
                else
                {
                    if (m_MergedTable.Rows[iCurrentLine][4].ToString() != "0")
                    {
                        sQuery = "SELECT Distinct(SO_Detail.Sales_Order) "
                        + "FROM SO_Detail LEFT JOIN SO_Header ON SO_Detail.Sales_Order = SO_Header.Sales_Order "
                        + "WHERE Material LIKE '" + EscapeSQLString(sCurrentPart) + "' AND SO_Header.Status LIKE 'Closed' AND "
                        + "SO_Header.Customer LIKE '" + EscapeSQLString(M_CUSTOMER_ID) + "' AND SO_Header.Customer_PO = '" + EscapeSQLString(sCurrentPO) + "' ";

                        var jbClosedSOs = conn.GetData(sQuery);

                        if (jbClosedSOs.Rows.Count == 0)
                        {
                            string newSO;
                            //create SO and insert all the lines
                            if (M_SO_CREATION == "ON")
                            {
                                newSO = CreateNewSalesOrder(m_MergedTable.Rows[iCurrentLine]);

                                for (var i = 1; i < iPOLines; i++)
                                {
                                    AddSODetailLineWithTaxCode(m_MergedTable.Rows[iCurrentLine + i],
                                        newSO, m_MergedTable.Rows[iCurrentLine + i][14].ToString());
                                }
                            }

                            if ((this.M_SO_CREATION == "ON" && this.M_SO_CREATION_STATUS == "Hold") || this.M_SO_CREATION != "ON")
                            {
                                var aUncreated = new string[this.m_MergedTable.Columns.Count];
                                for (var j = 0; j < this.m_MergedTable.Columns.Count; j++)
                                {
                                    aUncreated[j] = this.m_MergedTable.Rows[iCurrentLine][j].ToString();
                                }
                                uncreatedWriter.AddToFile(aUncreated);
                            }
                        }
                    }

                    iCurrentLine += iPOLines;
                }
            }

            var completeproc = new CompleteEngineProcess(OnComplete_Event);
            completeproc();
        }

        private void WriteDataTableOut(ref DataTable table)
        {
            var counter = 0;
            var now = DateTime.Now;
            var path = "%USERPROFILE%\\Documents\\EDITABLE-" + now.ToString("yyyyMMdd-hhmmss") + "-rows.csv";
            var sw = File.CreateText(Environment.ExpandEnvironmentVariables(path));
            {
                System.Data.DataRow row;
                for (var x = 0; x < table.Rows.Count; x++)
                {
                    row = table.Rows[x];

                    if (row[0].ToString() == "")
                    {
                        Console.WriteLine("We got an empty line");
                    }
                    else
                    {
                        sw.WriteLine(String.Join(",", row.ItemArray));
                        counter++;
                    }
                }
            }
            sw.Close();
        }
        #endregion
    }
}
