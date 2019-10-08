using MfgConnection;
using System;
using System.Data;
using System.Windows.Forms;

namespace EDI
{
    public sealed class EDI_Engine_Generic : EDI_Engine_Base
    {
        public EDI_Engine_Generic(string sDeliveryTemplatePath, string sDeliveryFilePath,
                                  string sDbName, string sDbType, string sCustId,
                                  string sAppPath, int iDelDateFormat, string sSOCreation,
                                  string sSOCreationStatus) : base(sDbName, sDbType, sCustId, sAppPath, sSOCreation, sSOCreationStatus)
        {
            this.m_DeliveryFileTemplate = new MFG.MFG_File_Template();
            this.m_DeliveryFileTemplate.SetFile(sDeliveryTemplatePath);
            this.m_DeliveryFileTemplate.Process();

            this.m_DeliveryXlsFileReader = new MFG.Files.XLS.XlsReader(sDeliveryFilePath,
                                                                        this.m_DeliveryFileTemplate);

            this.M_APPPATH = sAppPath;
            this.m_iDeliveryDateFormat = iDelDateFormat;
        }


        public void CreateTheDataTable(out DataTable m_DataTable)
        {
            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
            System.Data.DataTable so_header_schema = conn.getSchema("SO_Header");
            System.Data.DataTable so_detail_schema = conn.getSchema("SO_Detail");


            //Create an empty table of the format required for this application
            m_DataTable = new DataTable();
            string sColName;
            string sColType;
            string sColLength;

            //0 Material
            var theRow = so_detail_schema.Select("COLUMN_NAME = 'Material'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();//so_detail_structure[31][0];
            sColType = this.ProperTypeString(theRow[0]["DATA_TYPE"].ToString());//so_detail_structure[31][1];
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();//so_detail_structure[31][2];
            AddColumnToTable(ref m_DataTable, sColName, "Material", sColType, Convert.ToInt32(sColLength), true);

            //1 Sales_Order
            theRow = so_detail_schema.Select("COLUMN_NAME = 'Sales_Order'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();//so_detail_structure[1][0];
            sColType = this.ProperTypeString(theRow[0]["DATA_TYPE"].ToString());//so_detail_structure[1][1];
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();//so_detail_structure[1][2];
            AddColumnToTable(ref m_DataTable, sColName, "Sales Order", this.ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //2 PO
            theRow = so_header_schema.Select("COLUMN_NAME = 'Customer_PO'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();//so_header_structure[13][0];
            sColType = this.ProperTypeString(theRow[0]["DATA_TYPE"].ToString());//so_header_structure[13][1]; // <- "text"
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();//so_header_structure[13][2];
            AddColumnToTable(ref m_DataTable, sColName, "PO", sColType, Convert.ToInt32(sColLength), true);

            //3 Qty_Old
            theRow = so_detail_schema.Select("COLUMN_NAME = 'Order_Qty'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();//so_detail_structure[17][0];
            sColType = this.ProperTypeString(theRow[0]["DATA_TYPE"].ToString());//so_detail_structure[17][1];
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();//so_detail_structure[17][2];
            AddColumnToTable(ref m_DataTable, sColName, "Qty Old", sColType,
                                  Convert.ToInt32(this.GiveCorrectLength(sColType, sColLength)), true);


            //4 Qty_New ---.Comes from Excel file and not jbDB
            sColName = "NewQty";
            AddColumnToTable(ref m_DataTable, sColName, "Qty New", sColType,
                                  Convert.ToInt32(this.GiveCorrectLength(sColType, sColLength)), true);

            //5 Qty_Diff
            sColName = "Qty_Diff";
            AddColumnToTable(ref m_DataTable, sColName, "Qty Diff", sColType,
                                  Convert.ToInt32(this.GiveCorrectLength(sColType, sColLength)), true);

            //6 RefuseNewQty
            sColName = "RefuseNewQty";
            sColType = "varchar";
            sColLength = "50";
            AddColumnToTable(ref m_DataTable, sColName, "Refuse New Qty",
                                  this.ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //7 Qty_Diff_Value
            sColName = "Qty_Diff_Value";
            sColType = "double";
            AddColumnToTable(ref m_DataTable, sColName, "Qty Diff Value",
                this.ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //8 Date_Old
            theRow = so_detail_schema.Select("COLUMN_NAME = 'Promised_Date'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();//so_detail_structure[30][0];
            sColType = this.ProperTypeString(theRow[0]["DATA_TYPE"].ToString());//so_detail_structure[30][1];
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();//so_detail_structure[30][2];
            AddColumnToTable(ref m_DataTable, sColName, "Date Old", sColType,
                                  Convert.ToInt32(this.GiveCorrectLength(sColType, sColLength)), true);

            //9 Date_New
            sColName = "Date_New";
            AddColumnToTable(ref m_DataTable, sColName, "Date New", sColType,
                                  Convert.ToInt32(this.GiveCorrectLength(sColType, sColLength)), true);

            //10 Date_Diff
            sColName = "Date_Diff";
            sColType = this.ProperTypeString("int");
            AddColumnToTable(ref m_DataTable, sColName, "Date Diff", sColType,
                                  Convert.ToInt32(this.GiveCorrectLength(sColType, sColLength)), true);

            //11 RefuseNewDate
            sColName = "RefuseNewDate";
            sColType = "text";
            sColLength = "50";
            AddColumnToTable(ref m_DataTable, sColName, "Refuse New Date",
                this.ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //12 Date_Diff_Value
            sColName = "Date_Diff_Value";
            sColType = "double";
            AddColumnToTable(ref m_DataTable, sColName, "Date Diff Value",
               this.ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //13 DeliveryNewLocation
            sColName = "DeliveryNewLocation";
            sColType = "text";
            sColLength = "6";
            AddColumnToTable(ref m_DataTable, sColName, "Plant de livraison", this.ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //14 Excel File Detail Line
            sColName = "NewLineNumber";
            sColType = "text";
            sColLength = "6";
            AddColumnToTable(ref m_DataTable, sColName, "# de ligne du fichier",
                ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //15 SO Detail Line
            sColName = "JBLineNumber";
            sColType = "text";
            sColLength = "6";
            theRow = so_detail_schema.Select("COLUMN_NAME = 'SO_Line'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = this.ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "# de ligne du SO_Detail", this.ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //16 SO Detail
            sColName = "JBSODetail";
            sColType = "int";
            sColLength = "6";
            theRow = so_detail_schema.Select("COLUMN_NAME = 'SO_Detail'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = this.ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "SO_Detail", this.ProperTypeString(sColType),
                  Convert.ToInt32(this.GiveCorrectLength(sColType, sColLength)), true);

            //17 SO Detail Status
            sColName = "SO_Detail_Status";
            sColType = "text";
            sColLength = "10";
            theRow = so_detail_schema.Select("COLUMN_NAME = 'Status'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = this.ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "SO Detail Status", this.ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //18 Unit Price
            sColName = "UnitPrice";
            sColType = "double";
            sColLength = "6";
            theRow = so_detail_schema.Select("COLUMN_NAME = 'Unit_Price'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = this.ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "Prix Unitaire", this.ProperTypeString(sColType),
                  Convert.ToInt32(this.GiveCorrectLength(sColType, sColLength)), true);

            //19 FileOrigin
            sColName = "FileOrigin";
            sColType = "text";
            sColLength = "4";
            AddColumnToTable(ref m_DataTable, sColName, "Delivery || Forecast", this.ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //20 QtyCumulated
            sColName = "Qty Cum.";
            sColType = "int";
            AddColumnToTable(ref m_DataTable, sColName, "Qté cum reçue",
                this.ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //21 Source
            sColName = "Source";
            sColType = "text";
            sColLength = "6";
            AddColumnToTable(ref m_DataTable, sColName, "Source", this.ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //22 Source
            sColName = "Client";
            sColType = "text";
            sColLength = "10";
            AddColumnToTable(ref m_DataTable, sColName, "Client", this.ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //23 Source
            sColName = "DeliveryComment";
            sColType = "text";
            sColLength = "2147483647";
            AddColumnToTable(ref m_DataTable, sColName, "Commentaire de livraison", this.ProperTypeString(sColType), Convert.ToInt32(sColLength), true);
        }

        public void FillDataTable(ref DataTable m_DataTable)
        {
            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);

            this.PreemptiveVerification(ref this.m_MergedTable);
            this.RemoveMaterialsToBeIgnored(ref this.m_MergedTable,
                                            ref this.m_MaterialsToBeIgnored);

            int iCurrentLine = 0;

            OnProgressInitOutside(m_DataTable.Rows.Count, "Remplissage avec les données de JobBOSS"); // TODO: When converting to C# change OnProgressInitOutside to OnProgressInit and get rid of the method in EDI_Engine_Base

            string sCurrentPO;
            string sCurrentPart;
            string sQuery;

            while (iCurrentLine < m_DataTable.Rows.Count)
            {
                sCurrentPart = m_DataTable.Rows[iCurrentLine][0].ToString();
                sCurrentPO = m_DataTable.Rows[iCurrentLine][2].ToString();

                int iPOLines = 1;

                while ((iCurrentLine + iPOLines) < m_DataTable.Rows.Count)
                {
                    if ((m_DataTable.Rows[iCurrentLine + iPOLines][0].ToString() == sCurrentPart) && (m_DataTable.Rows[iCurrentLine + iPOLines][2].ToString() == sCurrentPO))
                    {
                        iPOLines++;
                    }
                    else
                    {
                        break;
                    }
                }

                sQuery = "SELECT Distinct(SO_Detail.Sales_Order) "
                    + "FROM SO_Detail LEFT JOIN SO_Header ON SO_Detail.Sales_Order = SO_Header.Sales_Order "
                    + "WHERE Material LIKE '" + sCurrentPart + "' AND SO_Header.Status IN ('Open', 'Hold') AND "
                    + "SO_Header.Customer LIKE '" + this.M_CUSTOMER_ID + "' AND SO_Header.Customer_PO = '" + sCurrentPO + "' ";

                var jbSO = conn.GetData(sQuery);

                if (jbSO != null)
                {
                    if (jbSO.Rows.Count > 1)
                    {
                        MessageBox.Show("Vous avez plus d'un Sales Order pour la pièce " + sCurrentPart + " pour votre "
                                        + "client EDI. Veuillez vous assurer que nous n'en avons juste un svp.");

                        System.Threading.Thread.CurrentThread.Abort();
                    }//end if
                    else if (jbSO.Rows.Count == 1)
                    {
                        sQuery = "SELECT SO_Detail.Sales_Order, SO_Detail.SO_Line, SO_Detail.Order_Qty, "
                            + "SO_Detail.Promised_Date, Address.Ship_To_ID, SO_Detail.Unit_Price, SO_Detail.SO_Detail, SO_Detail.Status "
                            + "FROM SO_Detail LEFT JOIN Address ON SO_Detail.Ship_To = Address.Address "
                            + "WHERE SO_Detail.Sales_Order = '" + jbSO.Rows[0][0] + "' AND (SO_Detail.Material LIKE '" + sCurrentPart + "')  AND SO_Detail.Status NOT IN ('Shipped','Closed')"
                            + "ORDER BY SO_Detail.SO_Detail";

                        var jbTable = conn.GetData(sQuery);

                        if (jbTable.Rows.Count > 0)
                        {
                            int iLinesToMatch;

                            if (jbTable.Rows.Count > iPOLines)
                            {
                                iLinesToMatch = iPOLines;
                            }
                            else
                            {
                                iLinesToMatch = jbTable.Rows.Count;
                            }

                            for (int x = 0; x < iLinesToMatch; x++)
                            {
                                m_DataTable.Rows[iCurrentLine][1] = jbTable.Rows[x][0].ToString();
                                m_DataTable.Rows[iCurrentLine][3] = jbTable.Rows[x][2].ToString();
                                m_DataTable.Rows[iCurrentLine][8] = jbTable.Rows[x][3].ToString();

                                this.PerformCalculationsForRow(ref m_DataTable, iCurrentLine);

                                iCurrentLine++;
                            }

                            if (iPOLines > jbTable.Rows.Count)
                            {
                                int iRemainingPOLines = iPOLines - jbTable.Rows.Count;

                                for (int x = 0; x < iRemainingPOLines; x++)
                                {
                                    m_DataTable.Rows[iCurrentLine][1] = jbTable.Rows[0][0].ToString();
                                    m_DataTable.Rows[iCurrentLine][3] = m_DataTable.Rows[iCurrentLine][4];
                                    m_DataTable.Rows[iCurrentLine][8] = m_DataTable.Rows[iCurrentLine][9];

                                    this.PerformCalculationsForRow(ref m_DataTable, iCurrentLine);

                                    iCurrentLine++;
                                }
                            }
                        }
                        else
                        { //All the lines of this Sales Order are closed or shipped. We must therefore add all of the file lines to new Sales Order detail lines.
                            for (int x = 0; x < iPOLines; x++)
                            {
                                m_DataTable.Rows[iCurrentLine][1] = jbSO.Rows[0][0].ToString();
                                m_DataTable.Rows[iCurrentLine][3] = 0;
                                m_DataTable.Rows[iCurrentLine][8] = m_DataTable.Rows[iCurrentLine][9];

                                this.PerformCalculationsForRow(ref m_DataTable, iCurrentLine);

                                iCurrentLine++;
                            }

                        }
                        iCurrentLine--;
                    }
                }
                else
                {
                    for (int i = 0; i < iPOLines; i++)
                    {
                        m_DataTable.Rows[iCurrentLine + i][3] = "0";
                        m_DataTable.Rows[iCurrentLine + i][8] = m_DataTable.Rows[iCurrentLine][9];

                        this.PerformCalculationsForRow(ref m_DataTable, iCurrentLine + i);
                    }
                }

                iCurrentLine++;
            }
        }

        public override DataTable GetMergedTable()
        {
            return this.m_MergedTable;
        }

        private void GroupUpDataTable()
        {
            var view = new System.Data.DataView(this.m_MergedTable);

            view.Sort = "Material, Customer_PO";

            var dtTemp = view.ToTable();

            this.m_MergedTable.Clear();
            foreach (System.Data.DataRow row in dtTemp.Rows)
            {
                this.m_MergedTable.ImportRow(row);
            }
        }

        public void InsertCsvRows(ref DataTable m_DataTable,
                                  ref MFG.Files.XLS.XlsReader m_EdiXlsFileReader,
                                  int DateFormat, string sFileOrigin)
        {
            m_EdiXlsFileReader.MoveToFirstRow();

            try
            {
                while (m_EdiXlsFileReader.Read())
                {
                    var currentRow = m_EdiXlsFileReader.GetCurrentRow();
                    var iRowLength = currentRow.Length;

                    string sTempPart = this.AdjustMaterial(currentRow[1]);
                    string sPart;
                    string sRev;
                    string sPO;

                    if (sTempPart.Contains(" REV "))
                    {
                        int revpos = sTempPart.IndexOf(" REV ");
                        sPart = sTempPart.Substring(0, revpos);
                        sRev = sTempPart.Substring(revpos + 5, (sTempPart.Length - (revpos + 5)));
                    }
                    else
                    {
                        sPart = sTempPart;
                        sRev = "";
                    }

                    if (currentRow[0].ToString().Length > 20)
                    {
                        sPO = currentRow[0].ToString().Substring(0, 20);
                    }
                    else
                    {
                        sPO = currentRow[0];
                    }

                    var dataRow = m_DataTable.NewRow();

                    //part number
                    dataRow[0] = sPart;
                    //Using the SO_Line field to store the part's revision in this engine.
                    dataRow[15] = sRev;

                    //customer po
                    dataRow[2] = sPO;

                    if (dataRow[2].ToString().Contains("FORECAST"))
                    {
                        dataRow[2] = dataRow[2].ToString().Trim(' ');
                    }

                    //quantity new
                    dataRow[4] = currentRow[4];

                    //date new
                    DateTime excelDate = DateTime.FromOADate(Convert.ToDouble(currentRow[3]));
                    dataRow[9] = excelDate.ToString("d");

                    dataRow[19] = sFileOrigin;

                    //location - plugging a value here since the file doesn't hold one
                    dataRow[13] = "main";
                    dataRow[14] = currentRow[2];//line# "0";

                    dataRow[21] = "XLS";
                    dataRow[22] = this.M_CUSTOMER_ID;
                    dataRow[23] = currentRow[5];                                //delivery comment

                    if (System.Convert.ToDouble(dataRow[4]) > 0)
                    {
                        m_DataTable.Rows.Add(dataRow);
                    }
                }
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                MessageBox.Show("ArgumentOutOfRange");
                throw e;
            }
            catch (Exception e)
            {
                MessageBox.Show("Autre chose...\n" + e.Message);
                throw e;
            }
        }

        protected override void PerformCalculationsForRow(ref DataTable m_DataTable, int iCurrentRow)
        {
            //Qty Diff
            object oQty = m_DataTable.Rows[iCurrentRow][3];
            int iJBQty = Convert.ToInt32(oQty);
            oQty = m_DataTable.Rows[iCurrentRow][4];
            int iFileQty = Convert.ToInt32(oQty);
            m_DataTable.Rows[iCurrentRow][5] = iFileQty - iJBQty; //Qty diff


            //Date Diff
            int[] sTempDate = this.FormatJBDate(m_DataTable.Rows[iCurrentRow][9].ToString());
            var cvsDate = new DateTime((int)sTempDate[2],
                           (int)sTempDate[1], (int)sTempDate[0]);

            sTempDate = this.FormatJBDate(m_DataTable.Rows[iCurrentRow][8].ToString());
            var jbDate = new DateTime((int)sTempDate[2], (int)sTempDate[1], (int)sTempDate[0]);

            m_DataTable.Rows[iCurrentRow][10] = cvsDate.Subtract(jbDate).TotalDays;
        }

        public override void Read()
        {
            var progressinit = new ProgressInitEngine(OnProgressInit_Event);
            progressinit(100, "Création/Remplissage de la table de comparaison");

            CreateTheDataTable(out m_MergedTable);
            InsertCsvRows(ref m_MergedTable, ref m_DeliveryXlsFileReader,
			  this.m_iDeliveryDateFormat, "D");

            GroupUpDataTable();

            var progressreport = new ProgressReportEngine(OnProgressReport_Event);
            progressreport(25);

            this.FillDataTable(ref m_MergedTable);

            var completeproc = new CompleteEngineProcess(OnComplete_Event);
            completeproc();
        }

        public override void Write()
        {
            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);

            string sCurrentPO;
            string sCurrentPart;
            string sQuery;
            string sSO;

            int iCurrentLine = 0;

            while (iCurrentLine < this.m_MergedTable.Rows.Count)
            {
                sCurrentPart = this.m_MergedTable.Rows[iCurrentLine][0].ToString();
                sSO = this.m_MergedTable.Rows[iCurrentLine][1].ToString();
                sCurrentPO = this.m_MergedTable.Rows[iCurrentLine][2].ToString();

                int iPOLines = 1;

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

                if (sSO != "")
                {
                    sQuery = "SELECT SO_Detail.Sales_Order, SO_Detail.SO_Line, SO_Detail.Order_Qty, "
                        + "SO_Detail.Promised_Date, Address.Ship_To_ID, SO_Detail.Unit_Price, SO_Detail.SO_Detail, SO_Detail.Status "
                        + "FROM SO_Detail LEFT JOIN Address ON SO_Detail.Ship_To = Address.Address "
                        + "WHERE SO_Detail.Sales_Order = '" + sSO + "' AND (SO_Detail.Material LIKE '" + sCurrentPart + "') AND SO_Detail.Status NOT IN ('Shipped','Closed')"
                        + "ORDER BY SO_Detail.SO_Detail";

                    var jbTable = conn.GetData(sQuery);

                    for (int i = 0; i < iPOLines; i++)
                    {
                        if (i < jbTable.Rows.Count)
                        {
                            string sUpdate = "UPDATE SO_Detail SET ";

                            string sUpdateDelivery;
                            bool bDeliveryUpdate = false;
                            sQuery = "SELECT SO_Detail FROM Delivery WHERE SO_Detail = " + jbTable.Rows[i][6].ToString();
                            var existingDeliveryTable = conn.GetData(sQuery);
                            if (existingDeliveryTable.Rows.Count > 0)
                            {
                                bDeliveryUpdate = true;
                            }

                            sUpdateDelivery = "UPDATE Delivery SET ";

                            string sDate = this.m_MergedTable.Rows[iCurrentLine + i][9].ToString();
                            string sQty = this.m_MergedTable.Rows[iCurrentLine + i][4].ToString();
                            string sLine = this.m_MergedTable.Rows[iCurrentLine + i][14].ToString();
                            string sDeliveryComment = this.m_MergedTable.Rows[iCurrentLine + i][23].ToString();

                            sUpdate += "Promised_Date = '" + sDate + "'";
                            sUpdateDelivery += "Promised_Date = '" + sDate + "' , Requested_Date = '" + sDate + "' ";


                            sUpdate += ", Order_Qty = " + sQty + ", Deferred_Qty = " + sQty;
                            sUpdateDelivery += ", Promised_Quantity = " + sQty + ", Remaining_Quantity = " + sQty + ", Comment = '" + sDeliveryComment + "' ";


                            string sSelect = "SELECT Unit_Price FROM SO_Detail WHERE SO_Detail.SO_Detail = " + jbTable.Rows[i][6].ToString();
                            var dTUnitPrice = conn.GetData(sSelect);

                            object oPrice = dTUnitPrice.Rows[0][0];
                            Double dUnitPrice = System.Convert.ToDouble(oPrice);
                            Double iQty = System.Convert.ToDouble(sQty);

                            string sNewPrice = (iQty * dUnitPrice).ToString();

                            sUpdate += ", Total_Price = " + sNewPrice;

                            sUpdate += ", SO_Line = " + sLine;

                            sSelect = "SELECT Sales_Code FROM Material WHERE Material LIKE '" + sCurrentPart + "'";
                            var dTSalesCode = conn.GetData(sSelect);
                            object oSalesCode = dTSalesCode.Rows[0][0];

                            if (oSalesCode != System.DBNull.Value)
                            {
                                sUpdate += ", Sales_Code = '" + oSalesCode.ToString() + "' ";
                            }


                            sUpdate += " WHERE SO_Detail = " + jbTable.Rows[i][6].ToString();
                            sUpdateDelivery += "WHERE SO_Detail = " + jbTable.Rows[i][6].ToString();

                            conn.SetData(sUpdate);

                            if (!bDeliveryUpdate)
                            {
                                sUpdateDelivery = "INSERT INTO Delivery (SO_Detail, Requested_Date, Promised_Date, Promised_Quantity, Remaining_Quantity, ObjectID) VALUES";

                                sUpdateDelivery += " (" + jbTable.Rows[i][6].ToString() + ", '" + sDate + "' , '" + sDate + "' ," + sQty + ", " + sQty +
                                    ", '" + System.Guid.NewGuid() + "')";
                            }

                            conn.SetData(sUpdateDelivery);
                        }
                        else
                        {
                            //Add an SO_Detail line to the SO
                            this.AddSODetailLine(this.m_MergedTable.Rows[iCurrentLine + i], sSO, this.m_MergedTable.Rows[iCurrentLine + i][14].ToString());
                        }
                    }

                    while (jbTable.Rows.Count > iPOLines)
                    {
                        conn.SetData("DELETE FROM Delivery WHERE Delivery.SO_Detail = " + jbTable.Rows[jbTable.Rows.Count - 1][6].ToString());
                        conn.SetData("DELETE FROM SO_Detail WHERE SO_Detail.SO_Detail = " + jbTable.Rows[jbTable.Rows.Count - 1][6].ToString());
                        jbTable.Rows.Remove(jbTable.Rows[jbTable.Rows.Count - 1]);
                    }

                    iCurrentLine += iPOLines;
                }
                else
                {
                    sQuery = "SELECT Distinct(SO_Detail.Sales_Order) "
                        + "FROM SO_Detail LEFT JOIN SO_Header ON SO_Detail.Sales_Order = SO_Header.Sales_Order "
                        + "WHERE Material LIKE '" + sCurrentPart + "' AND SO_Header.Status LIKE 'Closed' AND "
                        + "SO_Header.Customer LIKE '" + this.M_CUSTOMER_ID + "' AND SO_Header.Customer_PO = '" + sCurrentPO + "' ";

                    var jbClosedSOs = conn.GetData(sQuery);

                    if (jbClosedSOs.Rows.Count == 0)
                    {
                        //create SO
                        //and insert all the lines
                        string newSO = this.CreateNewSalesOrder(this.m_MergedTable.Rows[iCurrentLine]);

                        for (int i = 1; i < iPOLines; i++)
                        {
                            this.AddSODetailLine(this.m_MergedTable.Rows[iCurrentLine + i], newSO, this.m_MergedTable.Rows[iCurrentLine + i][14].ToString());

                        }
                    }

                    iCurrentLine += iPOLines;
                }
            }

            var completeproc = new CompleteEngineProcess(OnComplete_Event);
            completeproc();
        }
    }
}
