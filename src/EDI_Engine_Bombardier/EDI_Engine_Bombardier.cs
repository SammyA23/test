using MfgConnection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace EDI
{
    public struct Transaction
    {
        public string line;
        public string qty;
        public string unit;
        public string unitPrice;
        public string partNumber;
        public string rev;
        public string dr;
        public string shipTo;
        public string customer;
        public string salesOrder;
        public string po;
        public string promisedDate;
        public string buyer;
        public string description;
        public string extendedDescription;
        public string freeFormMessage;
    }
    public sealed class EDI_Engine_Bombardier : EDI_Engine_Base
    {
        #region members
        private readonly DataTable _dtFilesAndInfos;
        private readonly Dictionary<string, string> customersDict;
        private FileStream extraFeaturesWriter;
        private DataTable extraFeaturesTable;
        #endregion

        #region ctors
        public EDI_Engine_Bombardier(string sDeliveryTemplatePath,
          string sDeliveryFilePath, string sDbName, string sDbServer,
          string[] customers, string sAppPath, int iDelDateFormat,
          string sSoCreation, string sSoCreationStatus)
        : base(sDbName, sDbServer, customers[0], sAppPath, sSoCreation, sSoCreationStatus)
        {
            customersDict = new Dictionary<string, string>();
            foreach (var cus in customers)
            {
                var splitted = cus.Split(' ');
                customersDict.Add(splitted[0], splitted[1]);
            }

            if (sDeliveryTemplatePath == "ExtraFeatures")
            {
                m_DeliveryFileReader = new MFG.Files.CSV.CCsvReader(sDeliveryFilePath);
            }
            else if (sDeliveryTemplatePath == "ExtraFeaturesWrite")
            {
                var index = sDeliveryFilePath.LastIndexOf('\\');
                var path = sDeliveryFilePath.Substring(0, index);
                var name = sDeliveryFilePath.Substring(index);
                extraFeaturesWriter = new FileStream(path + name, FileMode.OpenOrCreate,
                  FileAccess.Write);
            }
            else
            {
                m_DeliveryFileTemplate = new MFG.MFG_File_Template();
                m_DeliveryFileTemplate.SetFile(sDeliveryTemplatePath);
                m_DeliveryFileTemplate.Process();

                m_DeliveryFileReader = new MFG.Files.CSV.CCsvReader(sDeliveryFilePath, m_DeliveryFileTemplate);
            }
            M_APPPATH = sAppPath;
            m_iDeliveryDateFormat = iDelDateFormat;
        }

        public EDI_Engine_Bombardier(string[][] sDeliveryTemplates, string[] files, string sDbName, string sDbServer,
          string[] sCustId, string sAppPath, int iDelDateFormat, string sSoCreation, string sSoCreationStatus)
        : base(sDbName, sDbServer, sCustId[0], sAppPath, sSoCreation, sSoCreationStatus)
        {
            M_APPPATH = sAppPath;
            m_iDeliveryDateFormat = iDelDateFormat;

            _dtFilesAndInfos = new DataTable();
            _dtFilesAndInfos.Columns.Add("filepath", typeof(string));
            _dtFilesAndInfos.Columns.Add("creationtime", typeof(DateTime));
            _dtFilesAndInfos.Columns.Add("filetype", typeof(string));
            _dtFilesAndInfos.Columns.Add("part", typeof(string));
            _dtFilesAndInfos.Columns.Add("po", typeof(string));
            _dtFilesAndInfos.Columns.Add("plant", typeof(string));
            _dtFilesAndInfos.Columns.Add("jbclient", typeof(string));
            _dtFilesAndInfos.Columns.Add("templatepath", typeof(string));

            foreach (var file in files)
            {
                var row = _dtFilesAndInfos.NewRow();
                //filepath, creation time, filetype, part, po, plant/client
                row[0] = file;

                //use the filesystem.io namespace to get the creation time of the file
                var fileinfo = new FileInfo(file);
                row[1] = fileinfo.CreationTime;

                //open the file and get the first field of row 2 to get the filetype
                row[2] = MFG.Files.CSV.CCsvReader.GetFieldAtLocation(file, 2, 1);
                row[2] = row[2].ToString().Replace("\"", "");

                foreach (var type in sDeliveryTemplates)
                {
                    if (type[0] == row[2].ToString())
                    {
                        row[7] = type[1];
                        break;
                    }
                }

                //open the csv file with the template and fetch the part, po, and plant
                var currentFileTemplate = new MFG.MFG_File_Template();
                currentFileTemplate.SetFile(row[7].ToString());
                currentFileTemplate.Process();

                var currentFileReader = new MFG.Files.CSV.CCsvReader(file, currentFileTemplate);
                currentFileReader.Read();
                var aRow = currentFileReader.GetCurrentRow();
                var sPart = AdjustMaterial(aRow[0]);
                var sPo = (aRow[2].Length > 20) ? aRow[2].Substring(0, 20) : aRow[2];
                var sPlant = (aRow[1].Length > 6) ? aRow[1].Substring(aRow[1].Length - 6) : aRow[1];

                row[3] = sPart;
                row[4] = sPo;
                row[5] = sPlant;

                row[6] = aRow[7].Replace("BRP plant code - ", "");

                _dtFilesAndInfos.Rows.Add(row);
            }

            var dtTemp = _dtFilesAndInfos.Clone();
            var view = new DataView(_dtFilesAndInfos);
            var dtDisctinctPartPoCombinations = view.ToTable(true, "part", "po");
            foreach (DataRow combination in dtDisctinctPartPoCombinations.Rows)
            {
                //foreach (var rowToImport in _dtFilesAndInfos.Select($"part='{combination[0]}' and po='{combination[1]}'", "creationtime DESC")) <- C#6 == .Net4.6
                foreach (var rowToImport in _dtFilesAndInfos.Select(string.Format("part='{0}' and po='{1}'", combination[0], combination[1]), "creationtime DESC"))
                {
                    dtTemp.ImportRow(rowToImport);
                }
            }

            _dtFilesAndInfos = dtTemp;

            customersDict = new Dictionary<string, string>();
            foreach (var cus in sCustId)
            {
                var splitted = cus.Split(' ');
                customersDict.Add(splitted[0], splitted[1]);
            }

            FetchCustomersForFiles(ref customersDict);

            m_DeliveryFileReader = new MFG.Files.CSV.CCsvReader(_dtFilesAndInfos);
        }
        #endregion

        #region Priv methods

        private void Analyze850(string[] args)
        {
            var progressinit = new ProgressInitEngine(OnProgressInit_Event);
            progressinit(100, "850 - Lecture et analyse du fichier");

            var po = "";
            var releaseNumber = "";
            var buyer = "";
            var buyertel = "";
            var buyerfax = "";
            var terms = "";
            var orderDate = "";
            var promisedDate = "";
            var salesRep = "";

            var billToId = "";
            var street = "";
            var streetTwo = "";
            var city = "";
            var state = "";
            var zip = "";
            var country = "";

            var line = "";
            var qty = "";
            var unit = "";
            var unitPrice = "";
            var partNumber = "";
            var description = "";
            var extendedDescription = "";
            var rev = "";
            var dr = "";
            var shipTo = "";
            var fileTransactionCount = "";
            var freeFormMessage = "";

            var transactions = new List<Transaction>();
            while (m_DeliveryFileReader.Read())
            {
                var fileline = m_DeliveryFileReader.GetCurrentRow();

                if (fileline[0] == "100")
                {
                    po = fileline[5];
                    releaseNumber = fileline[6];
                }
                else if (fileline[0] == "120")
                {
                    buyer = fileline[2];
                    buyertel = fileline[4];
                    buyerfax = fileline[6];
                }
                else if (fileline[0] == "140")
                {
                    terms = fileline[5];

                    if (!string.IsNullOrWhiteSpace(terms) && terms.Length > 15)
                        terms = terms.Substring(0, 15);
                }
                else if (fileline[0] == "150")
                {
                    if (fileline[1] == "007") { orderDate = fileline[2]; }
                    else if (fileline[1] == "036") { promisedDate = fileline[2]; }
                }
                else if (fileline[0] == "162")
                {
                    freeFormMessage += fileline[1] + "\n";
                }
                else if (fileline[0] == "175" && fileline[1] == "SR")
                {
                    salesRep = fileline[2];
                }
                else if (fileline[0] == "200")
                {
                    line = fileline[1].TrimStart('0');
                    unit = fileline[3];
                    unitPrice = fileline[4];
                    partNumber = String.IsNullOrEmpty(fileline[6]) ? "FRAIS DE SET-UP" : fileline[6];
                    if (PoIs550(po) &&
                      (!partNumber.Equals("FRAIS DE SET-UP") || !partNumber.Equals("SUPPLIER_SETUP")))
                    {
                        qty = 0.ToString();
                    }
                    else
                    {
                        qty = fileline[2];
                    }
                }
                else if (fileline[0] == "210" && fileline[2] == "PT")
                {
                    rev = fileline[3];
                    dr = fileline[5];
                }
                else if (fileline[0] == "230" && fileline[4] == "D1")
                {
                    description = fileline[5];
                }
                else if (fileline[0] == "230" && fileline[4] == "D2")
                {
                    //"230","F","08","ZZ","D2","VOIR 705502133"
                    extendedDescription = fileline[5];
                }
                else if (fileline[0] == "240" && fileline[1] == "SL")
                {
                    shipTo = fileline[2];
                }
                else if (!PoIs550(po) && fileline[0] == "270")
                {
                    //"270","50","PC","010","20181010"," "
                    qty = fileline[1];
                    promisedDate = fileline[4];
                }
                else if (fileline[0] == "300")
                {
                    billToId = fileline[4];
                    street = fileline[7];
                    streetTwo = fileline[8];
                    city = fileline[9];
                    state = fileline[10];
                    zip = fileline[11];
                    country = fileline[12];

                    string sCustomer;
                    billToId = billToId.Substring(Math.Max(0, billToId.Length - 6)); ;
                    customersDict.TryGetValue(billToId, out sCustomer);
                    if (string.IsNullOrEmpty(sCustomer))
                    {
                        MessageBox.Show("Nous ne pouvons continuer car le plant " + billToId
                          + " n'existe pas dans le fichier info. Veuillez corriger la situation SVP.");
                        Environment.Exit(0);
                    }

                    Transaction trans = new Transaction();
                    trans.line = line;
                    trans.qty = qty;
                    trans.unit = unit;
                    trans.unitPrice = unitPrice;
                    trans.partNumber = partNumber;
                    trans.rev = rev;
                    trans.dr = dr;
                    trans.shipTo = string.IsNullOrWhiteSpace(shipTo) ? "MAIN" : shipTo;
                    trans.customer = sCustomer;
                    trans.buyer = buyer;
                    trans.promisedDate = promisedDate;
                    trans.description = description;
                    trans.extendedDescription = extendedDescription;
                    trans.freeFormMessage = freeFormMessage;
                    transactions.Add(trans);

                    line = "";
                    qty = "";
                    unit = "";
                    unitPrice = "";
                    partNumber = "";
                    rev = "";
                    dr = "";
                    description = "";
                    extendedDescription = "";
                    shipTo = "";
                    qty = "";
                }
                else if (fileline[0] == "400")
                {
                    fileTransactionCount = fileline[1];
                }
                else
                {
                    //We don't care about those rows/lines
                }
            }

            var progressreport = new ProgressReportEngine(OnProgressReport_Event);
            progressreport(50);

            if (fileTransactionCount == transactions.Count.ToString())
            {
                CreateEightFiftyTemporaryTable(out extraFeaturesTable);
                var connEdi = new System.Data.SqlClient.SqlConnection(args[0]);
                connEdi.Open();
                var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
                foreach (Transaction trans in transactions)
                {
                    var tempRowToInsert = extraFeaturesTable.NewRow();

                    if (DoesCustomerExistInJbDb(trans.customer) &&
                      DoesCustomerHaveAddressInJbDb(trans.customer, trans.shipTo))
                    {
                        var address = GetAddressForShipToId(trans.customer, trans.shipTo);

                        var insertSoDetailQuery = Create850And860NewSalesOrderDetailQuery(trans,
                          ref promisedDate, ref address);

                        if (trans.partNumber == "SUPPLIER_SETUP")
                        {
                            string sql = "select h.Sales_Order from SO_Header h join SO_Detail d on d.Sales_Order = h.Sales_Order where h.Status = 'Open' and d.Status = 'Open' and h.Customer_PO = '" + EscapeSQLString(trans.po) + "' and h.Customer = '" + EscapeSQLString(trans.customer) + "' and d.Material = 'SUPPLIER_SETUP' ";
                            var dt = conn.GetData(sql);
                            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                            {
                                tempRowToInsert["Sales_Order"] = dt.Rows[0][0];
                                tempRowToInsert["JB_Client"] = trans.customer;
                                tempRowToInsert["Customer_PO"] = po;
                                tempRowToInsert["Part"] = trans.partNumber;
                                tempRowToInsert["Promised_Date"] = promisedDate;
                                tempRowToInsert["Qty"] = trans.qty;
                                tempRowToInsert["Plant"] = trans.shipTo;
                                tempRowToInsert["ReleaseNumber"] = releaseNumber;
                                tempRowToInsert["SO_HeaderInsert"] = "";
                                tempRowToInsert["SO_DetailInsert"] = "";

                                sql = "select d.SO_Detail from SO_Header h join SO_Detail d on d.Sales_Order = h.Sales_Order where h.Status = 'Open' and d.Status = 'Open' and h.Customer_PO = '" + EscapeSQLString(trans.po) + "' and h.Customer = '" + EscapeSQLString(trans.customer) + "' and d.Material = 'SUPPLIER_SETUP' and case when isnumeric(d.SO_Line) = 1 then convert(varchar(max), cast(d.SO_Line as int)) else d.SO_Line end = case when isnumeric('@soline') = 1 then convert(varchar(max), cast('@soline' as int)) else '@soline' end ";
                                sql = sql.Replace("@soline", EscapeSQLString(trans.line));
                                dt = conn.GetData(sql);

                                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                                    tempRowToInsert["SO_DetailInsert"] = insertSoDetailQuery;

                                extraFeaturesTable.Rows.Add(tempRowToInsert);
                                continue;
                            }
                        }

                        var insertSoQuery = Create850And860NewSalesOrderQuery(
                          trans, ref po, ref terms, ref orderDate, ref promisedDate, ref address);

                        tempRowToInsert["Sales_Order"] = "Nouveau Sales_Order";
                        tempRowToInsert["JB_Client"] = trans.customer;
                        tempRowToInsert["Customer_PO"] = po;
                        tempRowToInsert["Part"] = trans.partNumber;
                        tempRowToInsert["Promised_Date"] = promisedDate;
                        tempRowToInsert["Qty"] = trans.qty;
                        tempRowToInsert["Plant"] = trans.shipTo;
                        tempRowToInsert["ReleaseNumber"] = releaseNumber;
                        tempRowToInsert["SO_HeaderInsert"] = insertSoQuery;
                        tempRowToInsert["SO_DetailInsert"] = insertSoDetailQuery;

                        extraFeaturesTable.Rows.Add(tempRowToInsert);
                    }
                    else
                    {
                        MessageBox.Show(String.Format("Le plant {0}, n'existe pas "
                          + "pour le client {1}. SVP corrigez la situation "
                          + "avant de relancer l'analyse.", trans.shipTo,
                          trans.customer));
                        return;
                    }
                }

                conn.Dispose();
            }
            else
            {
                throw new WrongNumberOfTransactionsException(
                  "Analyze 850 - Nous ne trouvons pas le même nombre de transactions que ce le "
                  + "fichier nous dit qu'il contiens. (Nous:" + fileTransactionCount
                  + " Fichier: " + transactions.Count.ToString() + ")");
            }

            progressreport = new ProgressReportEngine(OnProgressReport_Event);
            progressreport(50);

            var completeproc = new CompleteEngineProcessWithMessage(OnCompleteWithMessage_Event);
            completeproc("brp850AnalyzeDone");
        }

        private void Analyze860(string[] args)
        {
            var progressinit = new ProgressInitEngine(OnProgressInit_Event);
            progressinit(100, "860 - Lecture et analyse du fichier");

            var po = "";
            var releaseNumber = "";
            var buyer = "";
            var buyertel = "";
            var buyerfax = "";
            var terms = "";
            var orderDate = "";
            var promisedDate = "";
            var salesRep = "";

            var billToId = "";
            var street = "";
            var streetTwo = "";
            var city = "";
            var state = "";
            var zip = "";
            var country = "";

            var line = "";
            var qty = "";
            var unit = "";
            var unitPrice = "";
            var partNumber = "";
            var description = "";
            var extendedDescription = "";
            var rev = "";
            var dr = "";
            var shipTo = "MAIN";
            var fileTransactionCount = "";
            var freeFormMessage = "";

            string state200 = "";

            List<string> qty290 = new List<string>();
            List<string> date290 = new List<string>();
            List<string> line290 = new List<string>();

            int transactionCount = 0;

            bool hasItems = false;

            var transactions = new List<Transaction>();
            while (m_DeliveryFileReader.Read())
            {
                var fileline = m_DeliveryFileReader.GetCurrentRow();

                if (fileline[0] == "100")
                {
                    po = fileline[5];
                    releaseNumber = fileline[6];
                }
                else if (fileline[0] == "120")
                {
                    buyer = fileline[2];
                    buyertel = fileline[4];
                    buyerfax = fileline[6];
                }
                else if (fileline[0] == "140")
                {
                    terms = fileline[5];

                    if (!string.IsNullOrWhiteSpace(terms) && terms.Length > 15)
                        terms = terms.Substring(0, 15);
                }
                else if (PoIs550(po) && fileline[0] == "150")
                {
                    if (fileline[1] == "007") { orderDate = fileline[2]; }
                    else if (fileline[1] == "036") { promisedDate = fileline[2]; }
                }
                else if (fileline[0] == "162")
                {
                    freeFormMessage = fileline[1];
                }
                else if (fileline[0] == "170" && fileline[1] == "BT")
                {
                }
                else if (fileline[0] == "175" && fileline[1] == "SR")
                {
                    salesRep = fileline[2];
                }
                else if (fileline[0] == "200")
                {
                    line = fileline[1].TrimStart('0');
                    state200 = fileline[2];
                    unit = fileline[4];
                    unitPrice = fileline[5];
                    partNumber = String.IsNullOrEmpty(fileline[7]) ? "FRAIS DE SET-UP" : fileline[7];
                    if (PoIs550(po) &&
                      (!partNumber.Equals("FRAIS DE SET-UP") || !partNumber.Equals("SUPPLIER_SETUP")))
                    {
                        qty = 0.ToString();
                    }
                    else
                    {
                        qty = fileline[3];
                    }
                }
                else if (fileline[0] == "210" && fileline[2] == "PT")
                {
                    rev = fileline[3];
                    dr = fileline[5];
                }
                else if (fileline[0] == "240" && fileline[4] == "D1")
                {
                    description = fileline[5];
                }
                else if (fileline[0] == "240" && fileline[4] == "D2")
                {
                    extendedDescription = fileline[5];
                }
                else if (fileline[0] == "260" && fileline[1] == "SL")
                {
                    shipTo = fileline[2];
                }
                else if (!PoIs550(po) && fileline[0] == "290")
                {
                    promisedDate = fileline[4];
                    qty290.Add(fileline[1]);
                    date290.Add(fileline[4]);
                    line290.Add(fileline[3]);
                }
                else if (fileline[0] == "310")
                {
                    billToId = fileline[4];
                    street = fileline[7];
                    streetTwo = fileline[8];
                    city = fileline[9];
                    state = fileline[10];
                    zip = fileline[11];
                    country = fileline[12];

                    string sCustomer;
                    billToId = billToId.Substring(Math.Max(0, billToId.Length - 6)); ;
                    customersDict.TryGetValue(billToId, out sCustomer);
                    if (string.IsNullOrEmpty(sCustomer))
                    {
                        MessageBox.Show("Nous ne pouvons continuer car le plant " + billToId
                          + " n'existe pas dans le fichier info. Veuillez corriger la situation SVP.");
                        Environment.Exit(0);
                    }

                    if (!PoIs550(po))
                    {
                        string promisedDateNotInString = "";
                        string poSo = this.GetSalesOrderForPartAndPoAndLine(partNumber, po, sCustomer, line);
                        for (int i = 0; i < qty290.Count; i++)
                        {
                            Transaction trans = new Transaction
                            {
                                line = line,
                                qty = qty290[i],
                                unit = unit,
                                unitPrice = unitPrice,
                                partNumber = partNumber,
                                rev = rev,
                                dr = dr,
                                shipTo = shipTo,
                                customer = sCustomer,
                                buyer = buyer,
                                po = po,
                                salesOrder = (partNumber == "SUPPLIER_SETUP" ? "" : "440|") + poSo,
                                description = description,
                                extendedDescription = extendedDescription,
                                freeFormMessage = freeFormMessage,
                                promisedDate = date290[i]
                            };
                            transactions.Add(trans);

                            if (!string.IsNullOrWhiteSpace(promisedDateNotInString))
                                promisedDateNotInString = promisedDateNotInString + ",";

                            promisedDateNotInString = promisedDateNotInString + "'" + date290[i] + "'";
                        }
                        transactionCount++;
                        hasItems = true;

                        if (!string.IsNullOrWhiteSpace(poSo) && !string.IsNullOrWhiteSpace(promisedDateNotInString))
                        {
                            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
                            var sodToExclude = conn.GetData("select Order_Qty, convert(varchar(10), Promised_Date, 120) as Promised_Date from so_detail where sales_order = '" + EscapeSQLString(poSo) + "' and replace(convert(varchar(10), Promised_Date, 120), '-', '') not in (" + promisedDateNotInString + ") and status in ('open', 'hold')");

                            if (sodToExclude != null && sodToExclude.Rows != null && sodToExclude.Rows.Count > 0)
                            {
                                for (int i = 0; i < sodToExclude.Rows.Count; i++)
                                {
                                    Transaction trans = new Transaction
                                    {
                                        line = line,
                                        qty = sodToExclude.Rows[i]["Order_Qty"].ToString(),
                                        unit = unit,
                                        unitPrice = unitPrice,
                                        partNumber = partNumber,
                                        rev = rev,
                                        dr = dr,
                                        shipTo = shipTo,
                                        customer = sCustomer,
                                        buyer = buyer,
                                        po = po,
                                        salesOrder = "440Delete|" + poSo,
                                        description = description,
                                        extendedDescription = extendedDescription,
                                        freeFormMessage = freeFormMessage,
                                        promisedDate = sodToExclude.Rows[i]["Promised_Date"].ToString().Replace("-", "").Substring(0, 8)
                                    };
                                    transactions.Add(trans);
                                }
                            }

                            conn.Dispose();
                        }
                    }
                    else
                    {
                        Transaction trans = new Transaction
                        {
                            line = line,
                            qty = state200 == "DI" ? "0" : qty,
                            unit = unit,
                            unitPrice = unitPrice,
                            partNumber = partNumber,
                            rev = rev,
                            dr = dr,
                            shipTo = shipTo,
                            customer = sCustomer,
                            buyer = buyer,
                            po = po,
                            salesOrder = this.GetSalesOrderForPartAndPoAndLine(partNumber, po, sCustomer, line),
                            description = description,
                            extendedDescription = extendedDescription,
                            freeFormMessage = freeFormMessage,
                            promisedDate = promisedDate
                        };
                        transactions.Add(trans);
                        transactionCount++;
                        hasItems = true;
                    }

                    line = "";
                    qty = "";
                    unit = "";
                    unitPrice = "";
                    partNumber = "";
                    rev = "";
                    dr = "";
                    description = "";
                    extendedDescription = "";
                    shipTo = "MAIN";
                    qty290 = new List<string>();
                    date290 = new List<string>();
                    line290 = new List<string>();
                }
                else if (fileline[0] == "400")
                {
                    fileTransactionCount = fileline[1];
                }
                else
                {
                    //We don't care about those rows/lines
                }
            }

            if (!hasItems && !string.IsNullOrWhiteSpace(po))
            {
                var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
                var dt = conn.GetData("select Customer, Sales_Order from SO_Header where Customer_PO = '" + EscapeSQLString(po) + "'");

                if (dt != null && dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Transaction trans = new Transaction
                        {
                            line = line,
                            qty = qty,
                            unit = unit,
                            unitPrice = unitPrice,
                            partNumber = partNumber,
                            rev = rev,
                            dr = dr,
                            shipTo = shipTo,
                            customer = dt.Rows[i][0].ToString() ?? "",
                            buyer = buyer,
                            po = po,
                            salesOrder = dt.Rows[i][1].ToString() ?? "",
                            description = description,
                            extendedDescription = extendedDescription,
                            freeFormMessage = freeFormMessage,
                        };
                        transactions.Add(trans);
                    }
                }
                else
                {
                    Transaction trans = new Transaction
                    {
                        line = line,
                        qty = qty,
                        unit = unit,
                        unitPrice = unitPrice,
                        partNumber = partNumber,
                        rev = rev,
                        dr = dr,
                        shipTo = shipTo,
                        customer = "",
                        buyer = buyer,
                        po = po,
                        salesOrder = "",
                        description = description,
                        extendedDescription = extendedDescription,
                        freeFormMessage = freeFormMessage,
                    };
                    transactions.Add(trans);
                }

                conn.Dispose();
            }

            var progressreport = new ProgressReportEngine(OnProgressReport_Event);
            progressreport(50);

            if ((fileTransactionCount == "0" && !hasItems) || (hasItems && fileTransactionCount == transactionCount.ToString()))
            {
                CreateEightSixtyTemporaryTable(out extraFeaturesTable);
                var connEdi = new System.Data.SqlClient.SqlConnection(args[0]);
                connEdi.Open();
                var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
                foreach (Transaction trans in transactions)
                {
                    var tempRowToInsert = extraFeaturesTable.NewRow();

                    if (!hasItems)
                    {
                        var updateSoQuery = "UPDATE SO_Header SET Terms='@terms'";

                        if (!String.IsNullOrWhiteSpace(promisedDate))
                            updateSoQuery = updateSoQuery + ", Promised_Date='" + EscapeSQLString(promisedDate.Substring(0, 4) + "-" + promisedDate.Substring(4, 2) + "-" + promisedDate.Substring(6)) + "'";

                        if (!String.IsNullOrWhiteSpace(orderDate))
                            updateSoQuery = updateSoQuery + ", Order_Date = '" + EscapeSQLString(orderDate.Substring(0, 4) + "-" + orderDate.Substring(4, 2) + "-" + orderDate.Substring(6)) + "' ";
                        updateSoQuery = updateSoQuery + ", last_updated = getdate() WHERE Customer_PO = '@po'";

                        updateSoQuery = updateSoQuery.Replace("@po", EscapeSQLString(po));
                        updateSoQuery = updateSoQuery.Replace("@terms", EscapeSQLString(terms));

                        tempRowToInsert["Sales_Order"] = trans.salesOrder;
                        tempRowToInsert["SO_Detail"] = "";
                        tempRowToInsert["JB_Client"] = trans.customer;
                        tempRowToInsert["Customer_PO"] = po;
                        tempRowToInsert["Part"] = trans.partNumber;
                        tempRowToInsert["Promised_Date"] = promisedDate;
                        tempRowToInsert["Qty"] = trans.qty;
                        tempRowToInsert["Plant"] = trans.shipTo;
                        tempRowToInsert["ReleaseNumber"] = releaseNumber;
                        tempRowToInsert["SO_HeaderInsert"] = updateSoQuery;
                        tempRowToInsert["SO_DetailInsert"] = "";

                        extraFeaturesTable.Rows.Add(tempRowToInsert);
                    }
                    else if (DoesCustomerExistInJbDb(trans.customer) &&
                      DoesCustomerHaveAddressInJbDb(trans.customer, trans.shipTo))
                    {
                        var updateSoQuery = "";
                        var updateSoDetailQuery = "";
                        var soDetail = "";
                        var address = GetAddressForShipToId(trans.customer, trans.shipTo);
                        string so = trans.salesOrder;

                        if (trans.salesOrder.StartsWith("440Delete|"))
                        {
                            so = trans.salesOrder.Replace("440Delete|", "");

                            updateSoQuery = "";

                            soDetail = GetSingleSoDetailForSalesOrder(so, trans.promisedDate);

                            updateSoDetailQuery = "";

                            if (!string.IsNullOrWhiteSpace(soDetail) && IsNumber(soDetail))
                            {
                                updateSoDetailQuery = "if exists(select 1 from SO_Detail where SO_Detail = " + EscapeSQLString(soDetail) + " and status <> 'Shipped') begin delete from Packlist_Detail where SO_Detail = " + EscapeSQLString(soDetail) + "; DELETE FROM Delivery WHERE Delivery.SO_Detail = " + EscapeSQLString(soDetail) + ";" + "DELETE FROM SO_Detail WHERE SO_Detail.SO_Detail = " + EscapeSQLString(soDetail) + "; end";
                            }

                            tempRowToInsert["Sales_Order"] = "SOD à supprimer - " + so;
                            tempRowToInsert["SO_Detail"] = soDetail;
                            tempRowToInsert["JB_Client"] = trans.customer;
                            tempRowToInsert["Customer_PO"] = po;
                            tempRowToInsert["Part"] = trans.partNumber;
                            tempRowToInsert["Promised_Date"] = trans.promisedDate;
                            tempRowToInsert["Qty"] = trans.qty;
                            tempRowToInsert["Plant"] = trans.shipTo;
                            tempRowToInsert["ReleaseNumber"] = releaseNumber;
                            tempRowToInsert["SO_HeaderInsert"] = updateSoQuery;
                            tempRowToInsert["SO_DetailInsert"] = updateSoDetailQuery;
                        }
                        else if (trans.salesOrder.StartsWith("440|"))
                        {
                            string promisedDate2 = trans.promisedDate;
                            so = trans.salesOrder.Replace("440|", "");


                            if (!so.Equals("Nouveau Sales_Order"))
                            {
                                updateSoQuery = Create850And860UpdateSalesOrderQuery(trans, ref terms, ref promisedDate2, ref address, trans.salesOrder);

                                soDetail = GetSingleSoDetailForSalesOrder(so, trans.promisedDate);
                                if (string.IsNullOrWhiteSpace(soDetail))
                                {
                                    soDetail = "Nouveau SOD";
                                    updateSoDetailQuery = Create850And860NewSalesOrderDetailQuery(trans, ref promisedDate2, ref address);

                                    updateSoDetailQuery = updateSoDetailQuery.Replace("@so", EscapeSQLString(so));
                                }
                                else
                                {
                                    updateSoDetailQuery = Create850And860UpdateSalesOrderDetailQuery(trans,
                                      ref soDetail, ref promisedDate2, ref address);
                                }
                            }
                            else
                            {
                                soDetail = "Nouveau SOD";

                                if (transactions.IndexOf(trans) == 0)
                                {
                                    updateSoQuery = Create850And860NewSalesOrderQuery(trans, ref po, ref terms,
                                      ref orderDate, ref promisedDate2, ref address);
                                    updateSoDetailQuery = Create850And860NewSalesOrderDetailQuery(trans,
                                      ref promisedDate2, ref address);
                                }
                                else
                                {
                                    updateSoQuery = Create850And860UpdateSalesOrderQuery(trans, ref terms, ref promisedDate2, ref address, "@so");
                                    updateSoDetailQuery = Create850And860NewSalesOrderDetailQuery(trans, ref promisedDate2, ref address);
                                }
                            }

                            tempRowToInsert["Sales_Order"] = so;
                            tempRowToInsert["SO_Detail"] = soDetail;
                            tempRowToInsert["JB_Client"] = trans.customer;
                            tempRowToInsert["Customer_PO"] = po;
                            tempRowToInsert["Part"] = trans.partNumber;
                            tempRowToInsert["Promised_Date"] = trans.promisedDate;
                            tempRowToInsert["Qty"] = trans.qty;
                            tempRowToInsert["Plant"] = trans.shipTo;
                            tempRowToInsert["ReleaseNumber"] = releaseNumber;
                            tempRowToInsert["SO_HeaderInsert"] = updateSoQuery;
                            tempRowToInsert["SO_DetailInsert"] = updateSoDetailQuery;
                        }
                        else
                        {
                            string promisedDate2 = trans.promisedDate;
                            if (trans.salesOrder.Equals("Sales_Order Closed"))
                            {
                                //Empty query, on ne fait rien
                            }
                            else if (trans.qty == "0")
                            {
                                updateSoQuery = "";

                                soDetail = GetSingleSoDetailForSalesOrder(so, trans.promisedDate);

                                updateSoDetailQuery = "";

                                if (!string.IsNullOrWhiteSpace(soDetail) && IsNumber(soDetail))
                                {
                                    updateSoDetailQuery = "update SO_Detail SET Status = 'Closed', Last_Updated = getdate() where SO_Detail = " + soDetail + ";";
                                    so = "SOD à supprimer - " + so;
                                }
                                else
                                {
                                    updateSoDetailQuery = "INSERT INTO MFG_EDI.dbo.brpModifiedSalesOrders (modifiedSO, tempStatus, releaseNumber) select d.Sales_Order, '860', " + EscapeSQLString(releaseNumber) + " from SO_Header h join SO_Detail d on d.Sales_Order = h.Sales_Order where h.Customer_PO = '" + EscapeSQLString(trans.po) + "' and d.Status IN('Hold', 'Open') and DATEDIFF(day, '" + EscapeSQLString(trans.promisedDate.Insert(4, "-").Insert(7, "-")) + "', d.Promised_Date) in (-2, -1, 0, 1,2,3,4,5,6) and d.Material = '" + trans.partNumber + "'; update d SET Status = 'Closed', Last_Updated = getdate() from SO_Header h join SO_Detail d on d.Sales_Order = h.Sales_Order where h.Customer_PO = '" + EscapeSQLString(trans.po) + "' and d.Status IN('Hold', 'Open') and DATEDIFF(day, '" + EscapeSQLString(trans.promisedDate.Insert(4, "-").Insert(7, "-")) + "', d.Promised_Date) in (-2, -1, 0, 1,2,3,4,5,6) and d.Material = '" + trans.partNumber + "';";
                                    so = "SOD à supprimer";
                                }
                            }
                            else if (!trans.salesOrder.Equals("Nouveau Sales_Order"))
                            {
                                if (trans.partNumber != "SUPPLIER_SETUP")
                                {
                                    updateSoQuery = Create850And860UpdateSalesOrderQuery(trans, ref terms, ref promisedDate2, ref address, trans.salesOrder);

                                    soDetail = GetSingleSoDetailForSalesOrder(trans.salesOrder);

                                    updateSoDetailQuery = Create850And860UpdateSalesOrderDetailQuery(trans, ref soDetail, ref promisedDate2, ref address);
                                }
                                else
                                {
                                    soDetail = GetSingleSoDetailForSalesOrderNotShipped(trans.salesOrder, trans.promisedDate, trans.line);

                                    if (!string.IsNullOrWhiteSpace(soDetail))
                                        updateSoDetailQuery = Create850And860UpdateSalesOrderDetailQuery(trans, ref soDetail, ref promisedDate2, ref address);
                                    else
                                        updateSoDetailQuery = Create850And860NewSalesOrderDetailQuery(trans, ref promisedDate2, ref address);
                                }
                            }
                            else
                            {
                                try
                                {
                                    updateSoQuery = Create850And860NewSalesOrderQuery(trans, ref po, ref terms,
                                      ref orderDate, ref promisedDate, ref address);
                                    updateSoDetailQuery = Create850And860NewSalesOrderDetailQuery(trans,
                                      ref promisedDate2, ref address);
                                }
                                catch (MissingContactFieldException e)
                                {
                                    throw;
                                }
                            }

                            tempRowToInsert["Sales_Order"] = so;
                            tempRowToInsert["SO_Detail"] = soDetail;
                            tempRowToInsert["JB_Client"] = trans.customer;
                            tempRowToInsert["Customer_PO"] = po;
                            tempRowToInsert["Part"] = trans.partNumber;
                            tempRowToInsert["Promised_Date"] = promisedDate2;
                            tempRowToInsert["Qty"] = trans.qty;
                            tempRowToInsert["Plant"] = trans.shipTo;
                            tempRowToInsert["ReleaseNumber"] = releaseNumber;
                            tempRowToInsert["SO_HeaderInsert"] = updateSoQuery;
                            tempRowToInsert["SO_DetailInsert"] = updateSoDetailQuery;
                        }

                        extraFeaturesTable.Rows.Add(tempRowToInsert);
                    }
                    else
                    {
                        MessageBox.Show(String.Format("Le plant {0}, n'existe pas "
                          + "pour le client {1}. SVP corrigez la situation "
                          + "avant de relancer l'analyse.", trans.shipTo,
                          trans.customer));
                        return;
                    }
                }

                conn.Dispose();
            }
            else
            {
                throw new WrongNumberOfTransactionsException(
                  "Analyze 860 - Nous ne trouvons pas le même nombre de transactions que ce le "
                  + "fichier nous dit qu'il contiens. (Nous:" + fileTransactionCount
                  + " Fichier: " + transactions.Count.ToString() + ")");
            }

            progressreport = new ProgressReportEngine(OnProgressReport_Event);
            progressreport(50);

            var completeproc = new CompleteEngineProcessWithMessage(OnCompleteWithMessage_Event);
            completeproc("brp860AnalyzeDone");
        }

        private string Create850And860NewSalesOrderQuery(Transaction trans,
          ref string po, ref string terms, ref string orderDate, ref string promisedDate,
          ref string address)
        {
            var insertSoQuery = "INSERT INTO SO_Header (Sales_Order, Customer, Ship_To, "
            + "Terms, Order_Date, Promised_Date, Customer_PO, Status, Contact, "
            + "Note_Text) VALUES ('@so','@customer', @shipTo, '@terms', "
            + "'@orderDate', '@promisedDate', "
            + "'@customerPo', '@status', @contact, '@noteText')";

            insertSoQuery = insertSoQuery.Replace("@customerPo", EscapeSQLString(po));
            insertSoQuery = insertSoQuery.Replace("@customer", EscapeSQLString(trans.customer));

            insertSoQuery = insertSoQuery.Replace("@shipTo", EscapeSQLString(address));

            insertSoQuery = insertSoQuery.Replace("@terms", EscapeSQLString(terms));

            var contact = GetContactForCustomerAndContactName(trans.customer, trans.buyer, trans.shipTo);
            bool contactIsNumeric = int.TryParse(contact, out int contactId);
            if (contactIsNumeric)
            {
                insertSoQuery = insertSoQuery.Replace("@contact", EscapeSQLString(contact));
            }
            else
            {
                var msg = "Il n'y as pas d'acheteur nommé " + trans.buyer
                + " pour le client " + trans.customer + ". SVP corriger la situation.";
                throw new MissingContactFieldException(msg);
            }

            if (String.IsNullOrWhiteSpace(orderDate))
            {
                insertSoQuery = insertSoQuery.Replace("@orderDate", EscapeSQLString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            else
            {
                insertSoQuery = insertSoQuery.Replace("@orderDate", EscapeSQLString(orderDate.Substring(0, 4) + "-" + orderDate.Substring(4, 2) + "-" + orderDate.Substring(6)));
            }

            if (String.IsNullOrWhiteSpace(promisedDate))
            {
                insertSoQuery = insertSoQuery.Replace("@promisedDate", EscapeSQLString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            else
            {
                insertSoQuery = insertSoQuery.Replace("@promisedDate", EscapeSQLString(promisedDate.Substring(0, 4) + "-" + promisedDate.Substring(4, 2) + "-" + promisedDate.Substring(6)));
            }

            insertSoQuery = insertSoQuery.Replace("@status", EscapeSQLString(M_SO_CREATION_STATUS));
            insertSoQuery = insertSoQuery.Replace("@noteText", EscapeSQLString(trans.freeFormMessage));

            return insertSoQuery;
        }

        private string Create850And860NewSalesOrderDetailQuery(Transaction trans,
          ref string promisedDate, ref string address)
        {
            var insertSoDetailQuery = "INSERT INTO SO_Detail (Sales_Order, SO_Line, Material, "
            + "Ship_To, Status, Make_Buy, Unit_Price, Price_UofM, Total_Price, "
            + "Order_Qty, Stock_UofM, Drop_Ship, Discount_Pct, "
            + "Deferred_Qty, Prepaid_Amt, Unit_Cost, Backorder_Qty, "
            + "Picked_Qty, Shipped_Qty, Returned_Qty, Certs_Required, Taxable, "
            + "Commissionable, Commission_Pct, Last_Updated, ObjectID, Promised_Date, "
            + "Note_Text, Ext_Description, Description) VALUES ("
            + "'@so',  '@line', '@partNumber', '@shipTo', '@status', 'M' , "
            + "@unitPrice, 'ea', @totalPrice, @orderQty, "
            + "'ea', 0, 0, @defferedQty, 0, 0, 0, 0, 0, 0, 0, 0, 0, "
            + "0, '@lastUpdated', '@guid', '@promisedDate', '@noteText', '@extDesc', '@description')";

            insertSoDetailQuery = insertSoDetailQuery.Replace("@line", EscapeSQLString(trans.line));
            insertSoDetailQuery = insertSoDetailQuery.Replace("@partNumber", EscapeSQLString(trans.partNumber));
            insertSoDetailQuery = insertSoDetailQuery.Replace("@shipTo", EscapeSQLString(address));
            insertSoDetailQuery = insertSoDetailQuery.Replace("@status", EscapeSQLString(M_SO_CREATION_STATUS));
            insertSoDetailQuery = insertSoDetailQuery.Replace("@unitPrice", EscapeSQLString(trans.unitPrice));
            insertSoDetailQuery = insertSoDetailQuery.Replace("@totalPrice", EscapeSQLString((Convert.ToDouble(trans.qty) * Convert.ToDouble(trans.unitPrice)).ToString()));
            insertSoDetailQuery = insertSoDetailQuery.Replace("@orderQty", EscapeSQLString(trans.qty));
            insertSoDetailQuery = insertSoDetailQuery.Replace("@defferedQty", EscapeSQLString(trans.qty));
            insertSoDetailQuery = insertSoDetailQuery.Replace("@lastUpdated", EscapeSQLString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            insertSoDetailQuery = insertSoDetailQuery.Replace("@guid", EscapeSQLString(System.Guid.NewGuid().ToString()));
            if (String.IsNullOrWhiteSpace(promisedDate))
            {
                insertSoDetailQuery = insertSoDetailQuery.Replace("@promisedDate", EscapeSQLString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            else
            {
                insertSoDetailQuery = insertSoDetailQuery.Replace("@promisedDate", EscapeSQLString(promisedDate.Substring(0, 4) + "-" + promisedDate.Substring(4, 2) + "-" + promisedDate.Substring(6)));
            }

            insertSoDetailQuery = insertSoDetailQuery.Replace("@extDesc", EscapeSQLString(trans.extendedDescription));
            insertSoDetailQuery = insertSoDetailQuery.Replace("@noteText", EscapeSQLString("Rev: " + trans.rev + "\nDessin: " + trans.dr));
            insertSoDetailQuery = insertSoDetailQuery.Replace("@description", EscapeSQLString(trans.description.Length >= 30 ? trans.description.Substring(0, 30) : trans.description));

            return insertSoDetailQuery;
        }

        private string Create850And860UpdateSalesOrderQuery(Transaction trans,
          ref string terms, ref string promisedDate, ref string address, string so)
        {
            var updateSoQuery = "UPDATE SO_Header SET Ship_To='@shipTo', "
            + "Terms='@terms', Promised_Date='@promisedDate', "
            + "Status='@status', Note_Text=@noteText "
            + "WHERE Sales_Order = '@salesOrder'";

            updateSoQuery = updateSoQuery.Replace("@salesOrder", EscapeSQLString(so.Replace("440|", "")));
            updateSoQuery = updateSoQuery.Replace("@shipTo", EscapeSQLString(address));
            updateSoQuery = updateSoQuery.Replace("@terms", EscapeSQLString(terms));
            if (String.IsNullOrWhiteSpace(promisedDate))
            {
                updateSoQuery = updateSoQuery.Replace("@promisedDate", EscapeSQLString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            else
            {
                updateSoQuery = updateSoQuery.Replace("@promisedDate", EscapeSQLString(promisedDate.Substring(0, 4) + "-" + promisedDate.Substring(4, 2) + "-" + promisedDate.Substring(6)));
            }
            updateSoQuery = updateSoQuery.Replace("@status", EscapeSQLString(M_SO_CREATION_STATUS));
            if (!String.IsNullOrWhiteSpace(trans.freeFormMessage))
            {
                updateSoQuery = updateSoQuery.Replace("@noteText", "CONCAT(Note_Text, CHAR(13)+CHAR(10)+CHAR(13)+CHAR(10) + '" + EscapeSQLString(trans.freeFormMessage) + "'");
            }
            else
            {
                updateSoQuery = updateSoQuery.Replace("@noteText", EscapeSQLString("Note_Text"));

            }

            return updateSoQuery;
        }

        private string Create850And860UpdateSalesOrderDetailQuery(Transaction trans,
          ref string soDetail, ref string promisedDate, ref string address)
        {

            var updateSoDetailQuery = "UPDATE SO_Detail SET SO_Line=@line, "
            + "Ship_To='@shipTo', Unit_Price=@unitPrice, Total_Price=@totalPrice, "
            + "Order_Qty=@orderQty, Deferred_Qty=@orderQty, Last_Updated='@lastUpdated', "
            + "Promised_Date='@promisedDate', Note_Text='@noteText', "
            + "Ext_Description='@extDesc', Description='@description' "
            + "WHERE SO_Detail = '@soDetail'";
            updateSoDetailQuery = updateSoDetailQuery.Replace("@line", EscapeSQLString(trans.line));
            updateSoDetailQuery = updateSoDetailQuery.Replace("@shipTo", EscapeSQLString(address));

            updateSoDetailQuery = updateSoDetailQuery.Replace("@status", EscapeSQLString(M_SO_CREATION_STATUS));

            updateSoDetailQuery = updateSoDetailQuery.Replace("@unitPrice", EscapeSQLString(trans.unitPrice));
            updateSoDetailQuery = updateSoDetailQuery.Replace("@totalPrice", EscapeSQLString((Convert.ToDouble(trans.qty) * Convert.ToDouble(trans.unitPrice)).ToString()));
            updateSoDetailQuery = updateSoDetailQuery.Replace("@orderQty", EscapeSQLString(trans.qty));
            updateSoDetailQuery = updateSoDetailQuery.Replace("@lastUpdated", EscapeSQLString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));

            // TODO @mond: What do we do when we have no promised date????????
            if (String.IsNullOrWhiteSpace(promisedDate))
            {
                updateSoDetailQuery = updateSoDetailQuery.Replace("@promisedDate", EscapeSQLString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            else
            {
                updateSoDetailQuery = updateSoDetailQuery.Replace("@promisedDate", EscapeSQLString(promisedDate.Substring(0, 4) + "-" + promisedDate.Substring(4, 2) + "-" + promisedDate.Substring(6)));
            }

            updateSoDetailQuery = updateSoDetailQuery.Replace("@soDetail", EscapeSQLString(soDetail));

            updateSoDetailQuery = updateSoDetailQuery.Replace("@extDesc", EscapeSQLString(trans.extendedDescription));

            updateSoDetailQuery = updateSoDetailQuery.Replace("@description", EscapeSQLString(trans.description.Length >= 30 ? trans.description.Substring(0, 30) : trans.description));

            updateSoDetailQuery = updateSoDetailQuery.Replace("@noteText", EscapeSQLString("Rev: " + trans.rev + "\nDessin: " + trans.dr));

            return updateSoDetailQuery;
        }

        private void CreateEightFiftyTemporaryTable(out DataTable m_DataTable)
        {
            m_DataTable = new DataTable();
            string sColName;
            string sColType;
            string sColLength;

            sColName = "Sales_Order";
            sColType = "System.String";
            sColLength = "50";
            AddColumnToTable(ref m_DataTable, sColName, "Bon d'achat", sColType,
             Convert.ToInt32(sColLength), true);

            sColName = "JB_Client";
            AddColumnToTable(ref m_DataTable, sColName, "Client", sColType,
             Convert.ToInt32(sColLength), true);

            sColName = "Customer_PO";
            AddColumnToTable(ref m_DataTable, sColName, "PO", sColType,
             Convert.ToInt32(sColLength), true);

            sColName = "Part";
            AddColumnToTable(ref m_DataTable, sColName, "Matérial", sColType,
             Convert.ToInt32(sColLength), true);

            sColName = "Promised_Date";
            AddColumnToTable(ref m_DataTable, sColName, "Date Promise", sColType,
             Convert.ToInt32(sColLength), true);

            sColName = "Qty";
            AddColumnToTable(ref m_DataTable, sColName, "Quantité", sColType,
             Convert.ToInt32(sColLength), true);

            sColName = "Plant";
            AddColumnToTable(ref m_DataTable, sColName, "Plant de livraison", sColType,
             Convert.ToInt32(sColLength), true);

            sColName = "ReleaseNumber";
            AddColumnToTable(ref m_DataTable, sColName, "ReleaseNumber", sColType,
             Convert.ToInt32(sColLength), true);

            sColName = "SO_HeaderInsert";
            sColLength = "4000";
            AddColumnToTable(ref m_DataTable, sColName, "SOH Insert Query", sColType,
             Convert.ToInt32(sColLength), true);

            sColName = "SO_DetailInsert";
            AddColumnToTable(ref m_DataTable, sColName, "SOD Insert Query", sColType,
             Convert.ToInt32(sColLength), true);
        }

        private void CreateEightSixtyTemporaryTable(out DataTable m_DataTable)
        {
            CreateEightFiftyTemporaryTable(out m_DataTable);

            string sColName;
            string sColType;
            string sColLength;

            sColName = "SO_Detail";
            sColType = "System.String";
            sColLength = "50";
            AddColumnToTable(ref m_DataTable, sColName, "SO Detail", sColType,
             Convert.ToInt32(sColLength), true);
        }

        private void CreateTheDataTable(out DataTable m_DataTable)
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
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = this.ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "Material", sColType, Convert.ToInt32(sColLength), true);


            //1 Sales_Order
            theRow = so_detail_schema.Select("COLUMN_NAME = 'Sales_Order'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = this.ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "Sales Order", this.ProperTypeString(sColType), Convert.ToInt32(sColLength), true);


            //2 PO
            theRow = so_header_schema.Select("COLUMN_NAME = 'Customer_PO'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = this.ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
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
            AddColumnToTable(ref m_DataTable, sColName, "# de ligne du fichier", this.ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //15 SO Detail Line
            sColName = "JBLineNumber";
            sColType = "text";
            sColLength = "6";
            theRow = so_detail_schema.Select("COLUMN_NAME = 'SO_Line'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = this.ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "# de ligne du SO_Detail", sColType, Convert.ToInt32(sColLength), true);

            //16 SO Detail
            sColName = "JBSODetail";
            sColType = "int";
            sColLength = "6";
            theRow = so_detail_schema.Select("COLUMN_NAME = 'SO_Detail'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = this.ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "SO_Detail", sColType,
              Convert.ToInt32(this.GiveCorrectLength(sColType, sColLength)), true);

            //17 SO Detail Status
            sColName = "SO_Detail_Status";
            sColType = "text";
            sColLength = "10";
            theRow = so_detail_schema.Select("COLUMN_NAME = 'Status'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = this.ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "SO Detail Status",
              sColType, Convert.ToInt32(sColLength), true);

            //18 Unit Price
            sColName = "UnitPrice";
            sColType = "double";
            sColLength = "6";
            theRow = so_detail_schema.Select("COLUMN_NAME = 'Unit_Price'");
            sColName = theRow[0]["COLUMN_NAME"].ToString();
            sColType = this.ProperTypeString(theRow[0]["DATA_TYPE"].ToString());
            sColLength = theRow[0]["CHARACTER_MAXIMUM_LENGTH"].ToString();
            AddColumnToTable(ref m_DataTable, sColName, "Prix Unitaire", sColType,
              Convert.ToInt32(this.GiveCorrectLength(sColType, sColLength)), true);

            //19 FileOrigin
            sColName = "FileOrigin";
            sColType = "text";
            sColLength = "4";
            AddColumnToTable(ref m_DataTable, sColName, "Delivery || Forecast",
              this.ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //20 QtyCumulated
            sColName = "Qty Cum.";
            sColType = "int";
            AddColumnToTable(ref m_DataTable, sColName, "Qté cum reçue",
              this.ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

            //21 Source
            sColName = "Source";
            sColType = "text";
            sColLength = "6";
            AddColumnToTable(ref m_DataTable, sColName, "Source",
              this.ProperTypeString(sColType), Convert.ToInt32(sColLength), true);

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

            conn.Dispose();
        }

        private bool DoesCustomerHaveAddressInJbDb(string customer, string address)
        {
            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
            string query = "SELECT Ship_To_ID FROM Address WHERE Customer LIKE '@customer'";
            query = query.Replace("@customer", EscapeSQLString(customer));

            var dt = conn.GetData(query);
            conn.Dispose();

            foreach (DataRow row in dt.Rows)
            {
                if (row["Ship_To_ID"].ToString() == address)
                {
                    return true;
                }
            }

            return false;
        }

        private bool DoesCustomerExistInJbDb(string customer)
        {
            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
            string query = "SELECT Customer FROM Customer WHERE Customer LIKE '@customer'";
            query = query.Replace("@customer", EscapeSQLString(customer));

            var dt = conn.GetData(query);
            conn.Dispose();

            if (dt.Rows.Count >= 1)
            {
                if (dt.Rows.Count > 1)
                {
                    MessageBox.Show(String.Format("Nous avons plus d'un client "
                      + "avec ce id ({0}). Nous utiliserons le premier trouver",
                      customer));
                }
                return true;
            }

            return false;
        }

        public override void ExecuteExtraFeature(string featureName, string[] args = null)
        {
            if (featureName == "850") { Analyze850(args); }
            else if (featureName == "850Exec") { Exec850(args); RemoveEmptyPackListDetails(); }
            else if (featureName == "855") { Exec855(args); }
            else if (featureName == "860") { Analyze860(args); }
            else if (featureName == "860Exec") { Exec860(args); RemoveEmptyPackListDetails(); }
            else if (featureName == "865") { Exec865(args); }
        }

        public void Exec850(string[] args)
        {
            var progressinit = new ProgressInitEngine(OnProgressInit_Event);
            var progressreport = new ProgressReportEngine(OnProgressReport_Event);
            progressinit(extraFeaturesTable.Rows.Count, "850 - Création des Bon de commandes");

            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
            var connEdi = new System.Data.SqlClient.SqlConnection(args[0]);
            connEdi.Open();

            foreach (DataRow row in extraFeaturesTable.Rows)
            {
                var insertSoQuery = row["SO_HeaderInsert"].ToString();
                var insertSoDetailQuery = row["SO_DetailInsert"].ToString();

                if (!string.IsNullOrWhiteSpace(insertSoQuery) && !string.IsNullOrWhiteSpace(insertSoDetailQuery))
                {
                    int soNumber = AcquireNextSalesOrderNumberAndIncrementAutoNumber();

                    insertSoQuery = insertSoQuery.Replace("@so", soNumber.ToString());
                    insertSoDetailQuery = insertSoDetailQuery.Replace("@so", soNumber.ToString());

                    conn.SetData(insertSoQuery);
                    conn.SetData(insertSoDetailQuery);

                    string sod = GetSingleSoDetailForSalesOrder(soNumber.ToString());

                    InsertOrUpdateDeliveryForSoDetail(sod,
                          row["Promised_Date"].ToString(), row["Qty"].ToString(),
                          !PoIs550(row["Customer_PO"].ToString()) ? "PPAP" : "");

                    var insertBrpCreatedSoQuery = "INSERT INTO brpCreatedSalesOrders "
                    + "(createdSO, tempStatus, releaseNumber) VALUES "
                    + "(@createdSO, '850', @releaseNumber)";
                    var command = new System.Data.SqlClient.SqlCommand(
                      insertBrpCreatedSoQuery, connEdi);
                    command.Parameters.AddWithValue("@createdSO", EscapeSQLString(soNumber.ToString()));
                    command.Parameters.AddWithValue("@releaseNumber", EscapeSQLString(row["ReleaseNumber"].ToString()));
                    command.ExecuteNonQuery();
                }

                progressreport(1);
            }
            conn.Dispose();
            var completeproc = new CompleteEngineProcessWithMessage(OnCompleteWithMessage_Event);
            completeproc("brp850ExecDone");
        }

        public void Exec855(string[] args)
        {
            Queue writeQueue = new Queue();

            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);

            var getAddressQuery = "SELECT Name, Line1, Line2, City, State, Zip, Country "
            + "FROM Address WHERE Address = 1";

            var tableAddress = conn.GetData(getAddressQuery);

            if (tableAddress.Rows.Count < 1) { return; }

            var ourName = tableAddress.Rows[0]["Name"].ToString();
            var ourStreetOne = tableAddress.Rows[0]["Line1"].ToString();
            var ourStreetTwo = tableAddress.Rows[0]["Line2"].ToString();
            var ourCity = tableAddress.Rows[0]["City"].ToString();
            var ourState = tableAddress.Rows[0]["State"].ToString();
            var ourZip = tableAddress.Rows[0]["Zip"].ToString();
            var ourCountry = tableAddress.Rows[0]["Country"].ToString();

            var connEdi = new System.Data.SqlClient.SqlConnection(args[0]);
            connEdi.Open();

            var getEightFiftySalesOrdersQuery = "SELECT createdSO As SO, releaseNumber, id FROM brpCreatedSalesOrders WHERE tempStatus LIKE '850'";
            var command = new System.Data.SqlClient.SqlCommand(getEightFiftySalesOrdersQuery, connEdi);
            var readerForEightFiftySalesOrders = command.ExecuteReader();

            try
            {
                if (readerForEightFiftySalesOrders.HasRows)
                {
                    var headerHasBeenCreated = false;
                    var eightFiftyIds = "";

                    var todayDate = DateTime.Now.Year.ToString()
                    + DateTime.Now.ToString("MM")
                    + DateTime.Now.ToString("dd");

                    var tableEightFiftySalesOrders = new System.Data.DataTable();
                    tableEightFiftySalesOrders.Load(readerForEightFiftySalesOrders);

                    foreach (DataRow currentRowFromEightFifty
                      in tableEightFiftySalesOrders.Rows)
                    {
                        var jbSalesOrderHeaderQuery = "SELECT Sales_Order, Customer, "
                        + "Ship_To, Terms, Order_Date, Promised_Date, Customer_PO, "
                        + "Status FROM SO_Header WHERE case when isnumeric(Sales_Order) = 1 then convert(varchar(max), cast(Sales_Order as int)) else Sales_Order end = "
                        + "case when isnumeric('@soNumber') = 1 then convert(varchar(max), cast('@soNumber' as int)) else '@soNumber' end";

                        var jbSalesOrderDetailQuery = "SELECT TOP 1 "
                        + "Material AS PartNumber, SO_Line "
                        + "FROM SO_Detail WHERE case when isnumeric(Sales_Order) = 1 then convert(varchar(max), cast(Sales_Order as int)) else Sales_Order end = "
                        + "case when isnumeric('@soNumber') = 1 then convert(varchar(max), cast('@soNumber' as int)) else '@soNumber' end";

                        jbSalesOrderHeaderQuery = jbSalesOrderHeaderQuery.Replace("@soNumber", EscapeSQLString(currentRowFromEightFifty["SO"].ToString()));
                        jbSalesOrderDetailQuery = jbSalesOrderDetailQuery.Replace("@soNumber", EscapeSQLString(currentRowFromEightFifty["SO"].ToString()));

                        var dtJbSoInfo = conn.GetData(jbSalesOrderHeaderQuery);
                        if (dtJbSoInfo.Rows.Count < 1) { continue; }

                        var dtJbSoDetailInfo = conn.GetData(jbSalesOrderDetailQuery);
                        if (dtJbSoDetailInfo.Rows.Count < 1) { continue; }

                        var rowJbSoInfo = dtJbSoInfo.Rows[0];

                        if (!headerHasBeenCreated)
                        {
                            var line000 = "000|ZZ|NUTECH|ZZ|"
                            + "254127301|P|NUTECH|254127301";

                            var line100 = "100|855|0001|00|"
                            + "AK|@PoNumber|@date|@850releaseNumber";

                            line100 = line100.Replace("@PoNumber", rowJbSoInfo["Customer_PO"].ToString());
                            line100 = line100.Replace("@date", todayDate);
                            line100 = line100.Replace("@850releaseNumber", currentRowFromEightFifty["releaseNumber"].ToString());

                            //TODO (@mond): We should probably add a whole
                            //supplier section of fields to include all of the
                            //static fields for the generation. Think of
                            //something like the config dialog for
                            //wiredTransfers application and the bank accounts
                            //dialog.

                            var line110 = "110|SU|@name|92|@numeroFournisseurBrp||"
                            + "@addressStreet1|@addressStreet2|@addressCity|@addressState|"
                            + "@addressZip|@addressCountry";

                            line110 = line110.Replace("@numeroFournisseurBrp", "0000104947");
                            line110 = line110.Replace("@name", ourName);
                            line110 = line110.Replace("@addressStreet1", ourStreetOne);
                            line110 = line110.Replace("@addressStreet2", ourStreetTwo);
                            line110 = line110.Replace("@addressCity", ourCity);
                            line110 = line110.Replace("@addressState", ourState);
                            line110 = line110.Replace("@addressZip", ourZip);
                            line110 = line110.Replace("@addressCountry", ourCountry);

                            writeQueue.Enqueue(line000);
                            writeQueue.Enqueue(line100);
                            writeQueue.Enqueue(line110);

                            headerHasBeenCreated = true;
                        }

                        var line200 = "200|@soLine|BP|@partNumber";
                        line200 = line200.Replace("@partNumber",
                          dtJbSoDetailInfo.Rows[0]["PartNumber"].ToString());
                        line200 = line200.Replace("@soLine",
                          dtJbSoDetailInfo.Rows[0]["SO_Line"].ToString());

                        var line210 = "210|IA";

                        writeQueue.Enqueue(line200);
                        writeQueue.Enqueue(line210);

                        if (!String.IsNullOrWhiteSpace(eightFiftyIds))
                        {
                            eightFiftyIds += ",";
                        }
                        eightFiftyIds += currentRowFromEightFifty["id"].ToString();
                    }

                    var line300 = "300|@count";
                    line300 = line300.Replace("@count",
                      tableEightFiftySalesOrders.Rows.Count.ToString());

                    writeQueue.Enqueue(line300);
                    WriteQueueToFile(writeQueue);

                    var changeStatusOfWrittenEightFiftySalesOrdersQuery =
                    String.Format("UPDATE brpCreatedSalesOrders SET tempStatus = '855' WHERE id in ({0})", EscapeSQLString(eightFiftyIds));

                    var updateCommand = new System.Data.SqlClient.SqlCommand(
                      changeStatusOfWrittenEightFiftySalesOrdersQuery, connEdi);

                    updateCommand.ExecuteNonQuery();
                }
                else
                {
                    var message = "Exec 855 - Il n'y a pas de Sales_Order venant d'un "
                    + "850 en attente d'approbation.";
                    throw new Exception(message);
                }
            }
            finally
            {
                conn.Dispose();
            }
        }

        public void Exec860(string[] args)
        {
            var progressinit = new ProgressInitEngine(OnProgressInit_Event);
            var progressreport = new ProgressReportEngine(OnProgressReport_Event);
            progressinit(extraFeaturesTable.Rows.Count, "860 - Mise à jour des Bon de commandes");

            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);

            var connEdi = new System.Data.SqlClient.SqlConnection(args[0]);
            connEdi.Open();

            string lastSo = string.Empty;
            foreach (DataRow row in extraFeaturesTable.Rows)
            {
                var updateSoQuery = row["SO_HeaderInsert"].ToString();
                var updateSoDetailQuery = row["SO_DetailInsert"].ToString();

                if (row["Sales_Order"].Equals("Nouveau Sales_Order") && updateSoQuery.StartsWith("INSERT"))
                {
                    row["Sales_Order"] = AcquireNextSalesOrderNumberAndIncrementAutoNumber().ToString();
                    updateSoQuery = updateSoQuery.Replace("@so", EscapeSQLString(row["Sales_Order"].ToString()));
                    updateSoDetailQuery = updateSoDetailQuery.Replace("@so", EscapeSQLString(row["Sales_Order"].ToString()));
                }

                if (IsNumber(row["Sales_Order"].ToString()))
                    lastSo = row["Sales_Order"].ToString();

                if (!IsNumber(row["Sales_Order"].ToString()) && IsNumber(lastSo))
                    row["Sales_Order"] = lastSo;

                if (!string.IsNullOrWhiteSpace(updateSoQuery))
                    conn.SetData(updateSoQuery.Replace("@so", EscapeSQLString(lastSo)));

                if (!string.IsNullOrWhiteSpace(updateSoDetailQuery))
                {
                    conn.SetData(updateSoDetailQuery.Replace("@so", EscapeSQLString(lastSo)));

                    if (updateSoDetailQuery.StartsWith("INSERT"))
                    {
                        row["SO_Detail"] = GetSingleSoDetailForSalesOrder(row["Sales_Order"].ToString());
                    }
                    if (!updateSoDetailQuery.StartsWith("DELETE "))
                    {
                        InsertOrUpdateDeliveryForSoDetail(row["SO_Detail"].ToString(),
                          row["Promised_Date"].ToString(), row["Qty"].ToString(),
                          !PoIs550(row["Customer_PO"].ToString()) ? "PPAP" : "");
                    }
                }

                if (IsNumber(row["Sales_Order"]?.ToString().Replace("SOD à supprimer - ", "")))
                {
                    var insertBrpModifiedSoQuery = "INSERT INTO brpModifiedSalesOrders "
                        + "(modifiedSO, tempStatus, releaseNumber) VALUES "
                        + "(@modifiedSO, '860', @releaseNumber)";
                    var command = new System.Data.SqlClient.SqlCommand(
                      insertBrpModifiedSoQuery, connEdi);
                    command.Parameters.AddWithValue("@modifiedSO", EscapeSQLString(row["Sales_Order"]?.ToString().Replace("SOD à supprimer - ", "")));
                    command.Parameters.AddWithValue("@releaseNumber", EscapeSQLString(row["ReleaseNumber"].ToString()));
                    command.ExecuteNonQuery();
                }

                progressreport(1);
            }

            conn.Dispose();

            var completeproc = new CompleteEngineProcessWithMessage(OnCompleteWithMessage_Event);
            completeproc("brp860ExecDone");
        }

        public void Exec865(string[] args)
        {
            Queue writeQueue = new Queue();

            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);

            var getAddressQuery = "SELECT Name, Line1, Line2, City, State, Zip, Country "
            + "FROM Address WHERE Address = 1";

            var tableAddress = conn.GetData(getAddressQuery);

            if (tableAddress.Rows.Count < 1) { return; }

            var ourName = tableAddress.Rows[0]["Name"].ToString();
            var ourStreetOne = tableAddress.Rows[0]["Line1"].ToString();
            var ourStreetTwo = tableAddress.Rows[0]["Line2"].ToString();
            var ourCity = tableAddress.Rows[0]["City"].ToString();
            var ourState = tableAddress.Rows[0]["State"].ToString();
            var ourZip = tableAddress.Rows[0]["Zip"].ToString();
            var ourCountry = tableAddress.Rows[0]["Country"].ToString();

            var connEdi = new System.Data.SqlClient.SqlConnection(args[0]);
            connEdi.Open();

            var getEightSixtySalesOrdersQuery = "SELECT modifiedSO As SO, releaseNumber, id FROM brpModifiedSalesOrders WHERE tempStatus LIKE '860'";
            var command = new System.Data.SqlClient.SqlCommand(getEightSixtySalesOrdersQuery, connEdi);
            var readerForEightSixtySalesOrders = command.ExecuteReader();

            try
            {
                if (readerForEightSixtySalesOrders.HasRows)
                {
                    var headerHasBeenCreated = false;
                    var eightSixtyIds = "";

                    var todayDate = DateTime.Now.Year.ToString()
                    + DateTime.Now.ToString("MM")
                    + DateTime.Now.ToString("dd");

                    var tableEightSixtySalesOrders = new System.Data.DataTable();
                    tableEightSixtySalesOrders.Load(readerForEightSixtySalesOrders);

                    foreach (DataRow currentRow
                      in tableEightSixtySalesOrders.Rows)
                    {
                        var jbSalesOrderHeaderQuery = "SELECT Sales_Order, Customer, "
                        + "Ship_To, Terms, Order_Date, Promised_Date, Customer_PO, "
                        + "Status FROM SO_Header WHERE case when isnumeric(Sales_Order) = 1 then convert(varchar(max), cast(Sales_Order as int)) else Sales_Order end = "
                        + "case when isnumeric('@soNumber') = 1 then convert(varchar(max), cast('@soNumber' as int)) else '@soNumber' end";

                        var jbSalesOrderDetailQuery = "SELECT TOP 1 "
                        + "Material AS PartNumber, SO_Line "
                        + "FROM SO_Detail WHERE case when isnumeric(Sales_Order) = 1 then convert(varchar(max), cast(Sales_Order as int)) else Sales_Order end = "
                        + "case when isnumeric('@soNumber') = 1 then convert(varchar(max), cast('@soNumber' as int)) else '@soNumber' end";

                        jbSalesOrderHeaderQuery = jbSalesOrderHeaderQuery.Replace("@soNumber", EscapeSQLString(currentRow["SO"].ToString()));
                        jbSalesOrderDetailQuery = jbSalesOrderDetailQuery.Replace("@soNumber", EscapeSQLString(currentRow["SO"].ToString()));

                        var dtJbSoInfo = conn.GetData(jbSalesOrderHeaderQuery);
                        if (dtJbSoInfo.Rows.Count < 1) { continue; }

                        var dtJbSoDetailInfo = conn.GetData(jbSalesOrderDetailQuery);
                        if (dtJbSoDetailInfo.Rows.Count < 1) { continue; }

                        var rowJbSoInfo = dtJbSoInfo.Rows[0];

                        if (!headerHasBeenCreated)
                        {
                            var line000 = "000|ZZ|NUTECH|ZZ|"
                            + "254127301|P|NUTECH|254127301";

                            var line100 = "100|865|0001|00|"
                            + "AK|@PoNumber|@860releaseNumber|@date";

                            line100 = line100.Replace("@PoNumber", rowJbSoInfo["Customer_PO"].ToString());
                            line100 = line100.Replace("@date", todayDate);
                            line100 = line100.Replace("@860releaseNumber", currentRow["releaseNumber"].ToString());

                            //TODO (@mond): We should probably add a whole
                            //supplier section of fields to include all of the
                            //static fields for the generation. Think of
                            //something like the config dialog for
                            //wiredTransfers application and the bank accounts
                            //dialog.

                            var line110 = "110|SU|@name|92|@numeroFournisseurBrp||"
                            + "@addressStreet1|@addressStreet2|@addressCity|@addressState|"
                            + "@addressZip|@addressCountry";

                            line110 = line110.Replace("@numeroFournisseurBrp", "0000104947");
                            line110 = line110.Replace("@name", ourName);
                            line110 = line110.Replace("@addressStreet1", ourStreetOne);
                            line110 = line110.Replace("@addressStreet2", ourStreetTwo);
                            line110 = line110.Replace("@addressCity", ourCity);
                            line110 = line110.Replace("@addressState", ourState);
                            line110 = line110.Replace("@addressZip", ourZip);
                            line110 = line110.Replace("@addressCountry", ourCountry);

                            writeQueue.Enqueue(line000);
                            writeQueue.Enqueue(line100);
                            writeQueue.Enqueue(line110);

                            headerHasBeenCreated = true;
                        }

                        // TODO @mond: 3rd field (response type code) is hardcoded to CA
                        // CA means a modified line/item. Other values exist and we might
                        // need to implement mechanisms to distinguish the proper type and
                        // give this value programmatically.
                        var line200 = "200|@soLine|CA|BP|@partNumber";
                        line200 = line200.Replace("@partNumber", dtJbSoDetailInfo.Rows[0]["PartNumber"].ToString());
                        line200 = line200.Replace("@soLine", dtJbSoDetailInfo.Rows[0]["SO_Line"].ToString());

                        var line210 = "210|IA";

                        writeQueue.Enqueue(line200);
                        writeQueue.Enqueue(line210);

                        if (!String.IsNullOrWhiteSpace(eightSixtyIds))
                        {
                            eightSixtyIds += ",";
                        }
                        eightSixtyIds += currentRow["id"].ToString();
                    }

                    var line300 = "300|@count";
                    line300 = line300.Replace("@count",
                      tableEightSixtySalesOrders.Rows.Count.ToString());

                    writeQueue.Enqueue(line300);
                    WriteQueueToFile(writeQueue);

                    var changeStatusOfWrittenEightSixtySalesOrdersQuery = "UPDATE brpModifiedSalesOrders SET tempStatus = '865' where tempStatus = '860'";

                    var updateCommand = new System.Data.SqlClient.SqlCommand(
                      changeStatusOfWrittenEightSixtySalesOrdersQuery, connEdi);

                    updateCommand.ExecuteNonQuery();
                }
                else
                {
                    var message = "Exec 865 - Il n'y a pas de Sales_Order venant d'un "
                    + "860 en attente d'approbation.";
                    throw new Exception(message);
                }
            }
            finally
            {
                conn.Dispose();
            }
        }

        private void InsertNewFileRowsWithoutMatchingJBSalesOrder(
          ref DataRow[] poRows, ref DataTable mDataTable, ref int currentRowIndex)
        {
            for (var i = 0; i < poRows.Length; i++)
            {
                System.Data.DataRow newRow = poRows[i];

                newRow[3] = "0";
                newRow[8] = mDataTable.Rows[currentRowIndex - 1][9];

                mDataTable.Rows.Add(newRow.ItemArray);
                PerformCalculationsForRow(ref mDataTable, currentRowIndex);

                currentRowIndex++;
            }
        }

        private void InsertOrUpdateDeliveryForSoDetail(string sod, string date,
          string qty, string shippingInstructions = "")
        {
            if (string.IsNullOrWhiteSpace(sod)) return;

            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);

            var sQuery = "if not exists(select 1 from Delivery where SO_Detail = " + EscapeSQLString(sod) + ") \n begin \n ";

            sQuery = sQuery + "INSERT INTO Delivery (SO_Detail, Requested_Date, Promised_Date, "
            + "Promised_Quantity, Remaining_Quantity, ObjectID";

            if (!String.IsNullOrWhiteSpace(shippingInstructions))
            {
                sQuery += ", Comment";
            }

            sQuery += ") VALUES (" + EscapeSQLString(sod) + ", '" + EscapeSQLString(date) + "', '" + EscapeSQLString(date) + "', " + EscapeSQLString(qty)
            + " , " + EscapeSQLString(qty) + ", '" + EscapeSQLString(Guid.NewGuid().ToString()) + "'";

            if (!String.IsNullOrWhiteSpace(shippingInstructions))
            {
                sQuery += ", '" + EscapeSQLString(shippingInstructions) + "'";
            }

            sQuery += ") \n end \n else \n begin \n ";

            sQuery = sQuery + "UPDATE Delivery SET Requested_Date='" + EscapeSQLString(date) + "', Promised_Date='" + EscapeSQLString(date)
            + "', Promised_Quantity=" + EscapeSQLString(qty) + ", Remaining_Quantity=" + EscapeSQLString(qty);

            if (!String.IsNullOrWhiteSpace(shippingInstructions))
            {
                sQuery += ", Comment='" + EscapeSQLString(shippingInstructions) + "'";
            }

            sQuery += " WHERE " + "SO_Detail = " + EscapeSQLString(sod) + " \n end";

            conn.SetData(sQuery);
        }

        private void FetchCustomersForFiles(ref Dictionary<string, string> customersDict)
        {
            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
            const string sPartialQuery = "SELECT AddressKey, Address, Customer, "
            + "Ship_To_ID FROM Address WHERE ";

            foreach (DataRow row in _dtFilesAndInfos.Rows)
            {
                string sCustomer;
                customersDict.TryGetValue(row["jbclient"].ToString(), out sCustomer);
                if (string.IsNullOrEmpty(sCustomer))
                {
                    MessageBox.Show("Nous ne pouvons continuer car le plant " + row["plant"]
                      + " n'existe pas dans le fichier info. Veuillez corriger la situation SVP.");
                    Environment.Exit(0);
                }

                var sFullQuery = sPartialQuery + " Customer LIKE '" + EscapeSQLString(sCustomer)
                + "' AND Address.Ship_To_ID = '" + EscapeSQLString(row["plant"]?.ToString()) + "'";
                var dtCustomer = conn.GetData(sFullQuery);

                if (dtCustomer != null && dtCustomer.Rows.Count == 1)
                {
                    //This is the expected behavior
                    row["jbclient"] = sCustomer;
                }
                else if (dtCustomer == null || dtCustomer.Rows.Count <= 0)
                {
                    //We need to react to this as this means there are no ShipTo with this Id.
                    var textforbox = "Le plant(" + row["plant"]
                    + ") n'existe pas dans le client " + sCustomer
                    + ". Avez-vous corriger la situation dans JobBOSS?";

                    var dialogResult = MessageBox.Show(textforbox,
                      "Plant non existant pour le client " + sCustomer, MessageBoxButtons.YesNo);

                    if (dialogResult == DialogResult.No)
                    {
                        MessageBox.Show("Nous ne pouvons continuer car le plant " + row["plant"]
                          + " n'existe pas pour le client " + sCustomer
                          + ". Veuillez corriger la situation SVP.");
                        Environment.Exit(0);
                    }
                }
                // dtCustomer.Rows.Count > 1 is not possible as JobBOSS enforces unique ShipTo id's

                //Eliminate possible jbclient corruption
                dtCustomer.Dispose();
            }
            conn.Dispose();
        }

        private void FillDataTable(ref DataTable mDataTable)
        {
            PreemptiveVerification(ref mDataTable);
            RemoveMaterialsToBeIgnored(ref mDataTable, ref m_MaterialsToBeIgnored);

            var progressinit = new ProgressInitEngine(OnProgressInit_Event);
            progressinit(mDataTable.Rows.Count, "Remplissage avec les données de JobBOSS");

            //Start by making us a temp table from the table submitted to us.
            //We will be adding rows amongst the existing ones, but we also need to move linearly
            //throughout the table.
            var tempTable = mDataTable;
            CreateTheDataTable(out mDataTable);

            bool has830 = false;
            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
            for (int i = tempTable.Rows.Count - 1; i >= 0; i--)
            {
                if (tempTable.Rows[i][19].ToString() == "830")
                {
                    if (tempTable.Rows[i][4].ToString() != "0")
                    {
                        string sql = "select 1 from SO_Detail d join SO_Header h on h.Sales_Order = d.Sales_Order where d.SO_Line <> '830'  and d.Material = '@material' and h.Customer_PO = '@po' and DATEDIFF(day, '@date', d.Promised_Date) in (1,2,3,4,5,6) and d.Status in ('Open', 'Hold')";
                        sql = sql.Replace("@material", EscapeSQLString(tempTable.Rows[i]["Material"].ToString()));
                        sql = sql.Replace("@po", EscapeSQLString(tempTable.Rows[i]["Customer_PO"].ToString()));
                        sql = sql.Replace("@date", EscapeSQLString(((DateTime)tempTable.Rows[i]["Date_New"]).ToString("yyyy-MM-dd")));
                        var data = conn.GetData(sql);
                        if (data != null && data.Rows != null && data.Rows.Count > 0)
                        {
                            tempTable.Rows.RemoveAt(i);
                            continue;
                        }
                    }

                    has830 = true;
                }

                if (tempTable.Rows[i][4].ToString() == "0") continue;
                if (!string.IsNullOrWhiteSpace(tempTable.Rows[i]["SO_Detail"].ToString())) continue;

                for (int i2 = 0; i2 < tempTable.Rows.Count; i2++)
                {
                    if (i == i2) continue;

                    if (tempTable.Rows[i]["Material"].ToString() == tempTable.Rows[i2]["Material"].ToString() &&
                       tempTable.Rows[i]["Sales_Order"].ToString() == tempTable.Rows[i2]["Sales_Order"].ToString() &&
                       tempTable.Rows[i]["Customer_PO"].ToString() == tempTable.Rows[i2]["Customer_PO"].ToString() &&
                       tempTable.Rows[i]["Promised_Date"].ToString() == tempTable.Rows[i2]["Promised_Date"].ToString() &&
                       tempTable.Rows[i]["Date_New"].ToString() == tempTable.Rows[i2]["Date_New"].ToString())
                    {
                        if (tempTable.Rows[i][19].ToString() == "830" && tempTable.Rows[i2][19].ToString() == "862")
                        {
                            tempTable.Rows.RemoveAt(i);
                        }
                        else
                        {
                            tempTable.Rows.RemoveAt(i2);
                        }
                        break;
                    }
                }
            }

            conn.Dispose();

            var currentRowIndex = 0;
            while (currentRowIndex < tempTable.Rows.Count)
            {
                currentRowIndex = MergeJbWithCurrentPartAndPo(ref tempTable, ref mDataTable, ref currentRowIndex, has830);
            }
        }

        private DateTime GetRelativeMondayForDate(DateTime date)
        {
            //find the proper monday relative to lastDateOfEightSixtyTwoLine
            int mondayOffset;
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Tuesday:
                    mondayOffset = -1;
                    break;
                case DayOfWeek.Wednesday:
                    mondayOffset = -2;
                    break;
                case DayOfWeek.Thursday:
                    mondayOffset = -3;
                    break;
                case DayOfWeek.Friday:
                    mondayOffset = -4;
                    break;
                case DayOfWeek.Saturday:
                    mondayOffset = -5;
                    break;
                case DayOfWeek.Sunday:
                    mondayOffset = -6;
                    break;
                case DayOfWeek.Monday:
                default:
                    mondayOffset = 0;
                    break;
            }

            return date.AddDays(mondayOffset);
        }

        private string GetSingleSoDetailForSalesOrder(in string so)
        {
            var conn = new jbConnection(
              this.M_DBNAME, this.M_DBSERVER);

            var query = "SELECT top 1 SO_Detail.SO_Detail FROM SO_Detail "
            + "WHERE SO_Detail.Status IN('Hold', 'Open', 'Shipped') AND "
            + "case when isnumeric(SO_Detail.Sales_Order) = 1 then convert(varchar(max), cast(SO_Detail.Sales_Order as int)) else SO_Detail.Sales_Order end = "
            + "case when isnumeric('@so') = 1 then convert(varchar(max), cast('@so' as int)) else '@so' end order by 1 desc";

            query = query.Replace("@so", EscapeSQLString(so));

            var dt = conn.GetData(query);
            conn.Dispose();

            if (dt.Rows.Count >= 1)
            {
                return dt.Rows[0]["SO_Detail"].ToString();
            }

            return "";
        }

        private string GetSingleSoDetailForSalesOrderNotShipped(in string so, string promisedDate)
        {
            var conn = new jbConnection(
              this.M_DBNAME, this.M_DBSERVER);

            var query = "SELECT SO_Detail.SO_Detail FROM SO_Detail "
            + "WHERE SO_Detail.Status IN('Hold', 'Open') AND "
            + "case when isnumeric(SO_Detail.Sales_Order) = 1 then convert(varchar(max), cast(SO_Detail.Sales_Order as int)) else SO_Detail.Sales_Order end = "
            + "case when isnumeric('@so') = 1 then convert(varchar(max), cast('@so' as int)) else '@so' end and replace(convert(varchar(10), Promised_Date, 120), '-', '') = @promisedDate";

            query = query.Replace("@so", EscapeSQLString(so));
            query = query.Replace("@promisedDate", EscapeSQLString(promisedDate?.Replace("-", "")));

            var dt = conn.GetData(query);
            conn.Dispose();

            if (dt.Rows.Count >= 1)
            {
                return dt.Rows[0]["SO_Detail"].ToString();
            }

            return "";
        }

        private string GetSingleSoDetailForSalesOrderNotShipped(in string so, string promisedDate, string line)
        {
            var conn = new jbConnection(
              this.M_DBNAME, this.M_DBSERVER);

            var query = "SELECT SO_Detail.SO_Detail FROM SO_Detail "
            + "WHERE SO_Detail.Status IN('Hold', 'Open') AND "
            + "case when isnumeric(SO_Detail.Sales_Order) = 1 then convert(varchar(max), cast(SO_Detail.Sales_Order as int)) else SO_Detail.Sales_Order end = "
            + "case when isnumeric('@so') = 1 then convert(varchar(max), cast('@so' as int)) else '@so' end and replace(convert(varchar(10), Promised_Date, 120), '-', '') = @promisedDate "
            + "and case when isnumeric(SO_Detail.SO_Line) = 1 then convert(varchar(max), cast(SO_Detail.SO_Line as int)) else SO_Detail.SO_Line end = case when isnumeric('@soline') = 1 then convert(varchar(max), cast('@soline' as int)) else '@soline' end";

            query = query.Replace("@soline", EscapeSQLString(line));
            query = query.Replace("@so", EscapeSQLString(so));
            query = query.Replace("@promisedDate", EscapeSQLString(promisedDate?.Replace("-", "")));

            var dt = conn.GetData(query);
            conn.Dispose();

            if (dt.Rows.Count >= 1)
            {
                return dt.Rows[0]["SO_Detail"].ToString();
            }

            return "";
        }

        private string GetSingleSoDetailForSalesOrder(in string so, string promisedDate)
        {
            var conn = new jbConnection(
              this.M_DBNAME, this.M_DBSERVER);

            var query = "SELECT SO_Detail.SO_Detail FROM SO_Detail "
            + "WHERE SO_Detail.Status IN('Hold', 'Open', 'Shipped') AND "
            + "case when isnumeric(SO_Detail.Sales_Order) = 1 then convert(varchar(max), cast(SO_Detail.Sales_Order as int)) else SO_Detail.Sales_Order end = "
            + "case when isnumeric('@so') = 1 then convert(varchar(max), cast('@so' as int)) else '@so' end and replace(convert(varchar(10), Promised_Date, 120), '-', '') = @promisedDate";

            query = query.Replace("@so", EscapeSQLString(so));
            query = query.Replace("@promisedDate", EscapeSQLString(promisedDate));

            var dt = conn.GetData(query);
            conn.Dispose();

            if (dt.Rows.Count >= 1)
            {
                return dt.Rows[0]["SO_Detail"].ToString();
            }

            return "";
        }

        private string GetSingleSoDetailForSalesOrderCanBe830(in string so, string promisedDate)
        {
            var conn = new jbConnection(
              this.M_DBNAME, this.M_DBSERVER);

            var query = "SELECT SO_Detail.SO_Detail FROM SO_Detail "
            + "WHERE SO_Detail.Status IN('Hold', 'Open', 'Shipped') AND "
            + "case when isnumeric(SO_Detail.Sales_Order) = 1 then convert(varchar(max), cast(SO_Detail.Sales_Order as int)) else SO_Detail.Sales_Order end = "
            + "case when isnumeric('@so') = 1 then convert(varchar(max), cast('@so' as int)) else '@so' end and (replace(convert(varchar(10), Promised_Date, 120), '-', '') = @promisedDate or (SO_Line = '830' and DATEDIFF(day, Promised_Date, '@promisedDate2') in (1,2,3,4,5,6)))";

            query = query.Replace("@so", EscapeSQLString(so));
            query = query.Replace("@promisedDate2", EscapeSQLString(promisedDate.Insert(4, "-").Insert(7, "-")));
            query = query.Replace("@promisedDate", EscapeSQLString(promisedDate));

            var dt = conn.GetData(query);
            conn.Dispose();

            if (dt.Rows.Count >= 1)
            {
                return dt.Rows[0]["SO_Detail"].ToString();
            }

            return "";
        }

        private void InsertCsvRows(DataTable mDataTable, MFG.Files.CSV.CCsvReader ediCsvFileReader, int iDateFormat)
        {
            ediCsvFileReader.MoveToFirstRow();

            try
            {
                while (ediCsvFileReader.Read())
                {
                    var currentRow = ediCsvFileReader.GetCurrentRow();
                    var dataRow = mDataTable.NewRow();

                    //part number
                    dataRow[0] = AdjustMaterial(currentRow[0]);

                    //customer po
                    dataRow[2] = currentRow[2].Length > 20 ? currentRow[2].Substring(0, 20) : currentRow[2];

                    if (dataRow[2].ToString().Contains("FORECAST"))
                    {
                        dataRow[2] = dataRow[2].ToString().Trim(' ');
                    }

                    //quantity new
                    dataRow[4] = currentRow[4];

                    //date new
                    var sExcelDate = FormatExcelDate(currentRow[5], iDateFormat);
                    var excelDate = new DateTime(sExcelDate[2], sExcelDate[1], sExcelDate[0]);
                    dataRow[9] = excelDate.ToString("d");

                    //delivery plant
                    dataRow[13] = currentRow[1].Length > 6 ? currentRow[1].Substring(currentRow[1].Length - 6) : currentRow[1];

                    //detail line
                    dataRow[14] = currentRow[9] == "830" ? "830" : currentRow[3];

                    dataRow[19] = currentRow[9];

                    dataRow[20] = IsNumber(currentRow[6]) ? currentRow[6] : 0.0f.ToString();

                    if (currentRow.Length >= 8)
                    {
                        dataRow[22] = currentRow[8];
                    }

                    //if (Convert.ToDouble(dataRow[4]) > 0)
                    //{
                    mDataTable.Rows.Add(dataRow);
                    //}
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                MessageBox.Show("ArgumentOutOfRange");
                Trace.Write("Argument out of range : " + e);
                throw;
            }
            catch (Exception e)
            {
                MessageBox.Show("Autre chose...\n" + e.Message);
                Trace.Write("New unexpected error : " + e);
                throw;
            }
        }

        private bool PoIs550(in string po)
        {
            if (po.StartsWith("550"))
            {
                return true;
            }

            return false;
        }

        private DataRow[] Prep830And862Rows(DataRow[] poRows)
        {
            var eightThirtyRows = poRows.AsEnumerable().Where(r => r.Field<string>("FileOrigin") == "830");
            var eightThirtyRowsArray = eightThirtyRows.ToArray();

            var eightSixtyTwoRows = poRows.AsEnumerable().Where(r => r.Field<string>("FileOrigin") == "862");
            var eightSixtyTwoRowsArray = eightSixtyTwoRows.ToArray();

            //now sort both arrays by incremental dates
            if (eightThirtyRowsArray.Length > 0)
            {
                SortArrayByDate(ref eightThirtyRowsArray);
            }

            if (eightSixtyTwoRowsArray.Length > 0)
            {
                SortArrayByDate(ref eightSixtyTwoRowsArray);
            }


            if (eightSixtyTwoRowsArray.Length == 0 && eightThirtyRowsArray.Length > 0)
            {
                return eightThirtyRowsArray;
            }
            else if (eightSixtyTwoRowsArray.Length > 0 && eightThirtyRowsArray.Length == 0)
            {
                return eightSixtyTwoRowsArray;
            }
            else
            {
                //take the date of the last row of the 862 array
                var lastDateOfEightSixtyTwoLine = Convert.ToDateTime(eightSixtyTwoRowsArray[eightSixtyTwoRowsArray.Length - 1][9]);
                var dtRelativeMonday = GetRelativeMondayForDate(lastDateOfEightSixtyTwoLine);

                //adjuste the eightThirtyRowsArrray accordingly by removing all lines with dates prior to
                //dtRelateMonday and subtract quantities for dates prior to lastDateOfEightSixtyTwoLine
                //TODOPLB: removed
                RemoveDatesFromArrayPriorTo(ref eightThirtyRowsArray, dtRelativeMonday);

                if (eightThirtyRowsArray.Length == 0)
                {
                    return eightSixtyTwoRowsArray;
                }

                //if we still have two lines in the 830 array for the week of dtRelativeMonday we must abort and
                //get rid of all 862 and 830 lines. We simply do not handle this situation. BRP promised only one
                //line in the 830 per week.
                if (eightThirtyRowsArray.Length > 1 &&
                  (DateTime)eightThirtyRowsArray[0]["Date_New"] >= dtRelativeMonday &&
                  (DateTime)eightThirtyRowsArray[0]["Date_New"] <= dtRelativeMonday.AddDays(5) &&
                  (DateTime)eightThirtyRowsArray[1]["Date_New"] >= dtRelativeMonday &&
                  (DateTime)eightThirtyRowsArray[1]["Date_New"] <= dtRelativeMonday.AddDays(5))
                {
                    var except = new ConstraintException("TooMany830Lines");
                    except.Data.Add("PoLines", eightSixtyTwoRowsArray.Length + eightThirtyRowsArray.Length);
                    except.Data.Add("WeekOf", dtRelativeMonday);
                    throw except;
                }

                //if ((DateTime)eightThirtyRowsArray[0]["Date_New"] >= dtRelativeMonday &&
                //  (DateTime)eightThirtyRowsArray[0]["Date_New"] <= dtRelativeMonday.AddDays(5))
                //{
                //foreach (var row in eightSixtyTwoRowsArray)
                //{
                //    if ((DateTime)row["Date_New"] >= dtRelativeMonday && (DateTime)row["Date_New"] <= dtRelativeMonday.AddDays(5))
                //    {
                //        eightThirtyRowsArray[0]["NewQty"] = (double)eightThirtyRowsArray[0]["NewQty"] - (double)row["NewQty"];
                //    }

                //if ((double)eightThirtyRowsArray[0]["NewQty"] < 0.0d)
                //{
                //    RemoveAt(ref eightThirtyRowsArray, 0);
                //    break;
                //}
                //}
                //}

                //find how many rows are remaining in the 830 array
                //create a new array the size of the 862array + remaining rows of the 830 array
                var actualRows = new DataRow[eightSixtyTwoRowsArray.Length + eightThirtyRowsArray.Length];

                //copy all 862 rows
                eightSixtyTwoRowsArray.CopyTo(actualRows, 0);
                //insert remaining 830 rows
                eightThirtyRowsArray.CopyTo(actualRows, eightSixtyTwoRowsArray.Length);

                return actualRows;
            }
        }

        private void RemoveAllRefusedLines(ref DataTable gridTable)
        {
            var rowsToRemove = gridTable.Select("Refus = true");

            foreach (var row in rowsToRemove)
            {
                row.Delete();
            }
        }

        private void RemoveAt<T>(ref T[] source, int index)
        {
            var dest = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            source = dest;
        }

        private void RemoveDatesFromArrayPriorTo(ref DataRow[] arrayToRemoveFrom,
          DateTime triggerDate)
        {
            var dt = arrayToRemoveFrom.CopyToDataTable();
            dt.DefaultView.RowFilter = "Date_New >= #" + triggerDate.ToString("MM/dd/yyyy") + "#";

            var trimmedArray = new DataRow[dt.DefaultView.Count];
            dt.DefaultView.ToTable().Rows.CopyTo(trimmedArray, 0);

            arrayToRemoveFrom = trimmedArray;
        }

        private void SortArrayByDate(ref DataRow[] arrayToSort)
        {
            var dtSorted = arrayToSort.CopyToDataTable();
            dtSorted.DefaultView.Sort = "Date_New ASC";

            dtSorted.AcceptChanges();

            var sortedArray = new DataRow[dtSorted.Rows.Count];
            dtSorted.DefaultView.ToTable().Rows.CopyTo(sortedArray, 0);
            arrayToSort = sortedArray;
        }
        #endregion

        #region Prot methods
        new void AddSODetailLine(DataRow dr, string sSONumber, string sLine)
        {
            string sDifferentialQty;
            string sDifferentialDate;

            var sMaterial = dr[0].ToString();
            var sPlant = dr[13].ToString();

            if (dr[22].ToString() != "")
            {
                M_CUSTOMER_ID = dr[22].ToString();
            }

            if (sPlant == "0")
            {
                MessageBox.Show("'Plant 0'...EDI essaie de crée une ligne alors qu'il devrait ne rien faire.");
            }

            sDifferentialQty = (dr[3] == DBNull.Value ? dr[4] : dr[3]).ToString();

            sDifferentialDate = dr[10] == DBNull.Value ? "0" : dr[10].ToString();

            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
            var address = conn.GetData("SELECT Address FROM Address WHERE Customer LIKE '"
             + M_CUSTOMER_ID + "' AND Ship_To_ID LIKE '" + EscapeSQLString(dr[13]?.ToString()) + "'");

            try
            {
                if (address.Rows.Count < 1)
                {
                    throw new MissingAddressFieldException("Le plant de livraison " + dr[13] +
                     " n'existe pas dans votre base de données.(Bomb) - Mat: " + dr[0] + " | SO: " + dr[1]);
                }
            }
            catch (MissingAddressFieldException e)
            {
                throw e;
            }


            var addressID = (address.Rows[0][0]).ToString();

            var dtInvCosting = conn.GetData("SELECT Inv_Cost_Method FROM Preferences");
            var sInvCostingMethod = dtInvCosting.Rows[0][0].ToString();
            string sQuery = "SELECT Material, Selling_Price, Price_UofM, "
                + "Stocked_UofM, Cost_UofM, Sales_Code, Rev FROM Material WHERE Material LIKE '" +
                EscapeSQLString(sMaterial) + "'";

            var dtMaterialInfo = conn.GetData(sQuery);

            var sUnitPrice = dtMaterialInfo.Rows[0][1].ToString();
            var sPriceUofM = dtMaterialInfo.Rows[0][2].ToString();
            var sOrderQty = dr[4].ToString();
            var sStockUofM = dtMaterialInfo.Rows[0][3].ToString();
            var sCostUofM = dtMaterialInfo.Rows[0][4].ToString();
            var oSalesCode = dtMaterialInfo.Rows[0][5];

            var oRev = (M_CUSTOMER_ID == "PRINOTH") ? dr[15].ToString() : dtMaterialInfo.Rows[0][6];

            var arPromisedDate = FormatJBDate(dr[9].ToString());
            var sPromisedDate = arPromisedDate[1] + "/" + arPromisedDate[0] + "/" + arPromisedDate[2];

            var sTotalPrice = (Convert.ToDouble(sUnitPrice) * Convert.ToDouble(sOrderQty)).ToString();

            var sStatus = M_SO_CREATION_STATUS;


            //create the detail line
            sQuery = "INSERT INTO SO_Detail (Sales_Order, SO_Line, Ship_To, Status, Make_Buy, Unit_Price, "
            + "Price_UofM, Total_Price, Order_Qty, Stock_UofM, Deferred_Qty, Promised_Date, Material, Cost_UofM ,"
            + "ObjectID, Unit_Cost, Shipped_Qty, Returned_Qty";

            if (oSalesCode != DBNull.Value)
            {
                sQuery += ", Sales_Code";
            }

            if (oRev != DBNull.Value)
            {
                sQuery += ", Rev";
            }

            sQuery += ") VALUES ('" + EscapeSQLString(sSONumber) + "', '" + EscapeSQLString(sLine) + "', " + EscapeSQLString(addressID) + ", '" + EscapeSQLString(sStatus)
            + "', 'M', " + EscapeSQLString(sUnitPrice) + ", '" + EscapeSQLString(sPriceUofM) + "', " + EscapeSQLString(sTotalPrice) + ", "
            + EscapeSQLString(sOrderQty) + ", '" + EscapeSQLString(sStockUofM) + "'" + ", " + EscapeSQLString(sOrderQty) + ", '" + EscapeSQLString(sPromisedDate)
            + "', '" + EscapeSQLString(sMaterial) + "', '" + EscapeSQLString(sCostUofM) + "', '" + EscapeSQLString(Guid.NewGuid().ToString()) + "', 0, 0, 0";

            if (oSalesCode != DBNull.Value)
            {
                sQuery += ", '" + EscapeSQLString(oSalesCode?.ToString()) + "'";
            }

            if (oRev != DBNull.Value)
            {
                sQuery += ", '" + EscapeSQLString(oRev?.ToString()) + "'";
            }

            sQuery += ")";

            conn.SetData(sQuery);

            var dtLatestSoDetail = conn.GetData("SELECT MAX(SO_Detail) FROM SO_Detail");

            sQuery = "INSERT INTO Delivery (SO_Detail, Requested_Date, Promised_Date, Promised_Quantity, "
            + "Remaining_Quantity, ObjectID) VALUES (" + EscapeSQLString(dtLatestSoDetail.Rows[0][0]?.ToString()) + ", '"
            + EscapeSQLString(sPromisedDate) + "', '" + EscapeSQLString(sPromisedDate) + "', " + EscapeSQLString(sOrderQty) + " , " + EscapeSQLString(sOrderQty)
            + ", '" + EscapeSQLString(Guid.NewGuid().ToString()) + "')";

            conn.SetData(sQuery);

            UpdateSOTotalPrice(sSONumber);
            AddToHistory(new string[] { sDifferentialQty, sDifferentialDate, sUnitPrice, sOrderQty });

            conn.Dispose();
        }

        private new int[] FormatExcelDate(string excelDate, int iDateFormat)
        {
            var returnArray = new int[3];
            var sDay = "";
            var sMonth = "";
            var sYear = "";

            try
            {
                int iDayPosition;
                int iMonthPosition;

                switch (iDateFormat)
                {
                    case 0:
                        iDayPosition = 0;
                        iMonthPosition = 1;
                        break;
                    case 1:
                        iDayPosition = 1;
                        iMonthPosition = 0;
                        break;
                    case 2:
                        iDayPosition = 2;
                        iMonthPosition = 1;
                        break;
                    default:
                        iDayPosition = 0;
                        iMonthPosition = 1;
                        break;
                }

                if (excelDate != "Stat Date" && excelDate.Length > 6)
                {
                    var iCounter = 0;
                    while (iCounter < 3)
                    {
                        if (iCounter == iDayPosition)
                        {
                            var index = excelDate.IndexOf('/');
                            if (index >= 0)
                            {
                                sDay = excelDate.Substring(0, index);
                                excelDate = excelDate.Remove(0, index + 1);
                            }
                            else
                            {
                                sDay = excelDate;
                            }
                        }
                        else if (iCounter == iMonthPosition)
                        {
                            sMonth = excelDate.Substring(0, 2);
                            excelDate = excelDate.Remove(0, 2);
                        }
                        else
                        {
                            if (excelDate.Length < 5)
                            {
                                sYear = excelDate;
                            }
                            else
                            {
                                sYear = excelDate.Substring(0, 4);
                                excelDate = excelDate.Remove(0, sYear.Length);
                            }

                            if (sYear.Length < 3)
                            {
                                sYear = "20" + sYear;
                            }
                        }

                        iCounter++;
                    }

                    returnArray[0] = Convert.ToInt32(sDay);
                    returnArray[1] = Convert.ToInt32(sMonth);
                    returnArray[2] = Convert.ToInt32(sYear);
                }

                return returnArray;
            }
            catch (Exception e)
            {
                MessageBox.Show("Dans la fonction de date\n" + e);
            }
            var dtToday = DateTime.Now;
            returnArray[0] = dtToday.Day;
            returnArray[1] = dtToday.Month;
            returnArray[2] = dtToday.Year;

            return returnArray;
        }

        private bool IsNumber(string s)
        {
            return s.Length > 0 && s.All(c => Char.IsDigit(c));
        }

        private int MergeJbWithCurrentPartAndPo(ref DataTable tempTable, ref DataTable mDataTable, ref int currentRowIndex, bool has830)
        {
            var sClient = tempTable.Rows[currentRowIndex][22].ToString();
            var sCurrentPo = tempTable.Rows[currentRowIndex][2].ToString();
            var sCurrentPart = tempTable.Rows[currentRowIndex][0].ToString();

            var sTempSelect = "Customer_PO = '" + EscapeSQLString(sCurrentPo) + "' AND Material = '" + EscapeSQLString(sCurrentPart) + "'";

            if (sClient != "" || sClient != null)
            {
                M_CUSTOMER_ID = sClient;
                sTempSelect += " AND Client = '" + EscapeSQLString(M_CUSTOMER_ID) + "'";
            }

            var poRows = tempTable.Select(sTempSelect);
            var iPoLines = poRows.Length;

            //This is where we must split the 830 and 862 rows, remerge them in the proper order and remove the
            //overlapping rows
            try
            {
                poRows = Prep830And862Rows(poRows);
            }
            catch (ConstraintException e)
            {
                if (e.Message == "TooMany830Lines")
                {
                    var message = "Il y a plusieurs lignes 830 pour la pièce " + sCurrentPart + " et le PO " +
                    sCurrentPo + " pour la semaine du " + e.Data["WeekOf"];
                    MessageBox.Show(message);
                    Trace.WriteLine(message);

                    return currentRowIndex + iPoLines;
                }

                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            var sQuery = "SELECT Distinct(SO_Detail.Sales_Order) "
            + "FROM SO_Detail LEFT JOIN SO_Header ON SO_Detail.Sales_Order = SO_Header.Sales_Order "
            + "WHERE Material LIKE '" + EscapeSQLString(sCurrentPart) + "' AND SO_Header.Status IN ('Open', 'Hold', 'Backorder') AND "
            + "SO_Header.Customer LIKE '" + EscapeSQLString(M_CUSTOMER_ID) + "' AND SO_Header.Customer_PO = '" + EscapeSQLString(sCurrentPo) + "' ";

            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
            var dtJbSo = conn.GetData(sQuery);

            if (dtJbSo == null || dtJbSo.Rows.Count == 0) //No sales order in JB, fill placeholder data to set creation for write operation
            {
                InsertNewFileRowsWithoutMatchingJBSalesOrder(ref poRows, ref mDataTable, ref currentRowIndex);

                conn.Dispose();

                return currentRowIndex;
            }
            else
            {
                if (dtJbSo.Rows.Count > 1)
                {
                    MessageBox.Show("Vous avez plus d'un Sales Order pour la pièce " + sCurrentPart + " pour votre "
                      + "client EDI. Veuillez vous assurer que nous n'en avons juste un svp.");

                    System.Threading.Thread.CurrentThread.Abort();
                }//end if
                else if (dtJbSo.Rows.Count == 1)
                {
                    var sJbSo = dtJbSo.Rows[0][0].ToString();

                    sQuery = "SELECT SO_Detail.Sales_Order, SO_Detail.SO_Line, SO_Detail.Order_Qty, "
                    + "SO_Detail.Promised_Date, Address.Ship_To_ID, SO_Detail.Unit_Price, "
                    + "SO_Detail.SO_Detail, SO_Detail.Status, Customer_Part.Sell_Price AS CustomerPartPrice "
                    + "FROM SO_Detail LEFT JOIN Address ON SO_Detail.Ship_To = Address.Address "
                    + "LEFT JOIN SO_Header ON SO_Detail.Sales_Order = SO_Header.Sales_Order "
                    + "LEFT JOIN Customer_Part ON SO_Header.Customer = Customer_Part.Customer AND SO_Detail.Material = Customer_Part.Material "
                    + "WHERE case when isnumeric(SO_Detail.Sales_Order) = 1 then convert(varchar(max), cast(SO_Detail.Sales_Order as int)) else SO_Detail.Sales_Order end = "
                    + "case when isnumeric('" + EscapeSQLString(sJbSo) + "') = 1 then convert(varchar(max), cast('" + EscapeSQLString(sJbSo) + "' as int)) else '" + EscapeSQLString(sJbSo) + "' end  AND (SO_Detail.Material LIKE '" + EscapeSQLString(sCurrentPart) + "') "
                    + "AND (SO_Detail.Status IN ('Open', 'Hold', 'Backorder', 'Shipped') OR (SO_Detail.Status LIKE 'Shipped' AND SO_Detail.Promised_Date >= getdate()))"
                    + (has830 ? "" : " and SO_Detail.SO_Line <> '830' ")
                    + "ORDER BY SO_Detail.Promised_Date ASC";

                    var jbTable = conn.GetData(sQuery);
                    var alreadyMerged = new DataColumn("AlreadyMerged", Type.GetType("System.Boolean"));
                    alreadyMerged.ReadOnly = false;
                    alreadyMerged.DefaultValue = false;
                    jbTable.Columns.Add(alreadyMerged);

                    for (var x = 0; x < poRows.Length; x++)
                    {
                        var currentLinePromisedDate = ((DateTime)poRows[x][9]);

                        if (currentLinePromisedDate > DateTime.Today)
                        {
                            Console.WriteLine("The current line's promised date is the future");
                        }


                        //We first need to find if one of the jb rows matches the current line
                        var matchedRow = jbTable.Select("Promised_Date = #" + ((DateTime)poRows[x][9]).ToString("M/dd/yyyy") + "# AND Order_Qty = " + poRows[x][4] + " AND AlreadyMerged <> True");
                        //DataRow[] matchedRow = jbTable.Select("Promised_Date = #" + ((DateTime)POrows[x][9]).ToString("M/dd/yyyy") + "#");

                        //We, then, need to acknowledge prior and unprocessed jb rows
                        var priorUnmatchedRows = jbTable.Select("Promised_Date < #" + ((DateTime)poRows[x][9]).ToString("M/dd/yyyy") + "# AND AlreadyMerged <> True");
                        if (priorUnmatchedRows.Length > 0)
                        {
                            foreach (var row in priorUnmatchedRows)
                            {
                                if (row["Status"].ToString() == "Shipped") continue;
                                var newRow = mDataTable.NewRow();

                                newRow[0] = sCurrentPart;
                                newRow[1] = sJbSo;
                                newRow[2] = sCurrentPo;
                                newRow[3] = row["Order_Qty"];
                                newRow[4] = "0";
                                newRow[8] = row["Promised_Date"];
                                newRow[9] = new DateTime(1, 1, 1);
                                newRow[15] = row["SO_Line"];
                                newRow[16] = row["SO_Detail"];
                                newRow[17] = row["Status"];

                                newRow[18] = row["CustomerPartPrice"] != DBNull.Value ? row["CustomerPartPrice"] : row["Unit_Price"];

                                newRow[21] = "JB";
                                newRow[22] = M_CUSTOMER_ID;

                                mDataTable.Rows.Add(newRow);
                                PerformCalculationsForRow(ref newRow);
                                //currentRowIndex++;

                                row["AlreadyMerged"] = true;
                            }
                        }

                        //Then we process the matched row from step 1
                        if (matchedRow.Length > 0)
                        {
                            {
                                var newRow = mDataTable.NewRow();

                                newRow[0] = sCurrentPart;
                                newRow[1] = sJbSo;
                                newRow[2] = sCurrentPo;
                                newRow[3] = matchedRow[0]["Order_Qty"];
                                newRow[4] = poRows[x][4].ToString();
                                newRow[8] = matchedRow[0]["Promised_Date"];
                                newRow[9] = poRows[x][9];
                                newRow[13] = poRows[x][13];
                                newRow[14] = poRows[x][14];
                                newRow[15] = matchedRow[0]["SO_Line"];
                                newRow[16] = matchedRow[0]["SO_Detail"];
                                newRow[17] = matchedRow[0]["Status"];

                                newRow[18] = matchedRow[0]["CustomerPartPrice"] != DBNull.Value ? matchedRow[0]["CustomerPartPrice"] : matchedRow[0]["Unit_Price"];

                                newRow[19] = poRows[x][19];
                                newRow[20] = poRows[x][20];
                                newRow[21] = "JB+CSV";
                                newRow[22] = M_CUSTOMER_ID;

                                mDataTable.Rows.Add(newRow);
                                PerformCalculationsForRow(ref newRow);
                            }

                            if (matchedRow[0]["Status"].ToString() == "Backorder")
                            {
                                var newRow = mDataTable.NewRow();

                                newRow[0] = sCurrentPart;
                                newRow[1] = sJbSo;
                                newRow[2] = sCurrentPo;
                                newRow[3] = matchedRow[0]["Order_Qty"];
                                newRow[4] = poRows[x][4].ToString();
                                newRow[8] = matchedRow[0]["Promised_Date"];
                                newRow[9] = poRows[x][9];
                                newRow[13] = poRows[x][13];
                                newRow[14] = poRows[x][14];
                                newRow[15] = matchedRow[0]["SO_Line"];

                                newRow[18] = matchedRow[0]["CustomerPartPrice"] != DBNull.Value ? matchedRow[0]["CustomerPartPrice"] : matchedRow[0]["Unit_Price"];

                                newRow[19] = poRows[x][19];
                                newRow[20] = poRows[x][20];

                                newRow[21] = "JB";
                                newRow[22] = M_CUSTOMER_ID;

                                mDataTable.Rows.Add(newRow);
                                PerformCalculationsForRow(ref newRow);
                            }

                            //currentRowIndex++;

                            matchedRow[0]["AlreadyMerged"] = true;
                        }
                        else //otherwise add our current line
                        {
                            var newRow = mDataTable.NewRow();

                            newRow[0] = sCurrentPart;
                            newRow[1] = sJbSo;
                            newRow[2] = sCurrentPo;
                            newRow[3] = "0";
                            newRow[4] = poRows[x][4].ToString();
                            newRow[8] = new DateTime(1, 1, 1);
                            newRow[9] = poRows[x][9];
                            newRow[13] = poRows[x][13];
                            newRow[14] = poRows[x][14];
                            newRow[19] = poRows[x][19];
                            newRow[20] = poRows[x][20];
                            newRow[21] = "CSV";
                            newRow[22] = M_CUSTOMER_ID;

                            mDataTable.Rows.Add(newRow);
                            PerformCalculationsForRow(ref newRow);
                            //currentRowIndex++;
                        }
                    }

                    //Must check for jb lines still unmatched and add those to flag them for re-use or removal
                    var unmatchedRow = jbTable.Select("AlreadyMerged <> True");
                    foreach (var row in unmatchedRow)
                    {
                        if (row["Status"].ToString() == "Shipped") continue;
                        var newRow = mDataTable.NewRow();

                        newRow[0] = sCurrentPart;
                        newRow[1] = sJbSo;
                        newRow[2] = sCurrentPo;
                        newRow[3] = row["Order_Qty"];
                        newRow[4] = "0";
                        newRow[8] = row["Promised_Date"];
                        newRow[9] = new DateTime(1, 1, 1);
                        newRow[15] = row["SO_Line"];
                        newRow[16] = row["SO_Detail"];
                        newRow[17] = row["Status"];

                        newRow[18] = row["CustomerPartPrice"] != DBNull.Value ? row["CustomerPartPrice"] : row["Unit_Price"];

                        newRow[21] = "JB";
                        newRow[22] = M_CUSTOMER_ID;

                        mDataTable.Rows.Add(newRow);
                        PerformCalculationsForRow(ref newRow);

                        row["AlreadyMerged"] = true;
                    }
                }
            }

            conn.Dispose();

            return currentRowIndex + iPoLines;
        }

        protected override void PerformCalculationsForRow(ref DataTable mDataTable, int iCurrentRow)
        {
            //Qty Diff
            var oQty = mDataTable.Rows[iCurrentRow][3];
            var iJbQty = Convert.ToInt32(oQty);
            oQty = mDataTable.Rows[iCurrentRow][4];
            var iFileQty = Convert.ToInt32(oQty);
            mDataTable.Rows[iCurrentRow][5] = iFileQty - iJbQty; //Qty diff

            //Date Diff
            var sTempDate = FormatJBDate(mDataTable.Rows[iCurrentRow][9].ToString());
            var cvsDate = new DateTime((int)sTempDate[2], (int)sTempDate[1], (int)sTempDate[0]);

            sTempDate = FormatJBDate(mDataTable.Rows[iCurrentRow][8].ToString());
            var jbDate = new DateTime((int)sTempDate[2], (int)sTempDate[1], (int)sTempDate[0]);

            mDataTable.Rows[iCurrentRow][10] = cvsDate.Subtract(jbDate).TotalDays;
        }

        private void PerformCalculationsForRow(ref DataRow dr)
        {
            //Qty Diff
            var oQty = dr[3];
            var iJbQty = Convert.ToInt32(oQty);
            oQty = dr[4];
            var iFileQty = Convert.ToInt32(oQty);
            dr[5] = iFileQty - iJbQty; //Qty diff

            //Date Diff
            var sTempDate = FormatJBDate(dr[9].ToString());
            var cvsDate = new DateTime((int)sTempDate[2], (int)sTempDate[1], (int)sTempDate[0]);

            sTempDate = FormatJBDate(dr[8].ToString());
            var jbDate = new DateTime((int)sTempDate[2], (int)sTempDate[1], (int)sTempDate[0]);

            dr[10] = cvsDate.Subtract(jbDate).TotalDays;
        }

        private void UpdateRow(ref DataRow row)
        {
            var sQuery = "SELECT SO_Detail FROM Delivery WHERE SO_Detail = " + EscapeSQLString(row[16]?.ToString());

            var bDeliveryUpdate = false;
            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
            var existingDeliveryTable = conn.GetData(sQuery);
            if (existingDeliveryTable.Rows.Count > 0)
            {
                bDeliveryUpdate = true;
            }

            var dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            var timePattern = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
            var newPromisedDate = DateTime.ParseExact(row[9].ToString(), dateFormat + " " + timePattern, CultureInfo.InvariantCulture);
            var sDate = newPromisedDate.ToString("yyyy-MM-dd");
            var sQty = row[4].ToString();
            var sLine = row[14].ToString();

            var sSelect = "SELECT SO_Detail.Unit_Price, Customer_Part.Sell_Price AS CustomerPartPrice FROM SO_Detail "
            + "LEFT JOIN SO_Header ON SO_Detail.Sales_Order = SO_Header.Sales_Order "
            + "LEFT JOIN Customer_Part ON SO_Header.Customer = Customer_Part.Customer AND SO_Detail.Material = Customer_Part.Material "
            + "WHERE SO_Detail.SO_Detail = " + EscapeSQLString(row[16]?.ToString());

            var dTUnitPrice = conn.GetData(sSelect);

            var oPrice = dTUnitPrice.Rows[0]["CustomerPartPrice"] != DBNull.Value ? dTUnitPrice.Rows[0]["CustomerPartPrice"] : dTUnitPrice.Rows[0]["Unit_Price"];
            var dUnitPrice = Convert.ToDouble(oPrice);
            var iQty = Convert.ToDouble(sQty);

            var sNewPrice = (iQty * dUnitPrice).ToString(CultureInfo.InvariantCulture);

            sSelect = "SELECT Sales_Code FROM Material WHERE Material LIKE '" + EscapeSQLString(row[0]?.ToString()) + "'";
            var dTSalesCode = conn.GetData(sSelect);
            var oSalesCode = dTSalesCode.Rows[0][0];

            conn.Dispose();
            UpdateRowWithValues(sDate, sQty, sLine, dUnitPrice.ToString(CultureInfo.InvariantCulture), sNewPrice, oSalesCode, row[16].ToString(), bDeliveryUpdate);
        }

        //private void UpdateRowUsing(ref DataRow jbRow, ref DataRow csvRow)
        //{
        //    var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
        //    var sQuery = "SELECT SO_Detail FROM Delivery WHERE SO_Detail = " + EscapeSQLString(jbRow[16]?.ToString());

        //    var bDeliveryUpdate = false;
        //    var existingDeliveryTable = conn.GetData(sQuery);
        //    if (existingDeliveryTable.Rows.Count > 0)
        //    {
        //        bDeliveryUpdate = true;
        //    }

        //    var dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
        //    var timePattern = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
        //    var newPromisedDate = DateTime.ParseExact(csvRow[9].ToString(), dateFormat + " " + timePattern, CultureInfo.InvariantCulture);
        //    var sDate = newPromisedDate.ToString("yyyy-MM-dd");
        //    var sQty = csvRow[4].ToString();
        //    var sLine = csvRow[14].ToString();

        //    var sSelect = "SELECT SO_Detail.Unit_Price, Customer_Part.Sell_Price AS CustomerPartPrice FROM SO_Detail "
        //    + "LEFT JOIN SO_Header ON SO_Detail.Sales_Order = SO_Header.Sales_Order "
        //    + "LEFT JOIN Customer_Part ON SO_Header.Customer = Customer_Part.Customer AND SO_Detail.Material = Customer_Part.Material "
        //    + "WHERE SO_Detail.SO_Detail = " + EscapeSQLString(jbRow[16]?.ToString());

        //    var dTUnitPrice = conn.GetData(sSelect);

        //    var oPrice = dTUnitPrice.Rows[0]["CustomerPartPrice"] != DBNull.Value ? dTUnitPrice.Rows[0]["CustomerPartPrice"] : dTUnitPrice.Rows[0]["Unit_Price"];
        //    var dUnitPrice = Convert.ToDouble(oPrice);
        //    var iQty = Convert.ToDouble(sQty);

        //    var sNewPrice = (iQty * dUnitPrice).ToString(CultureInfo.InvariantCulture);

        //    sSelect = "SELECT Sales_Code FROM Material WHERE Material LIKE '" + EscapeSQLString(jbRow[0]?.ToString()) + "'";
        //    var dTSalesCode = conn.GetData(sSelect);
        //    var oSalesCode = dTSalesCode.Rows[0][0];

        //    conn.Dispose();
        //    UpdateRowWithValues(sDate, sQty, sLine, dUnitPrice.ToString(CultureInfo.InvariantCulture), sNewPrice, oSalesCode, jbRow[16].ToString(), bDeliveryUpdate);
        //}

        private void UpdateRowWithValues(string date, string qty, string line, string unitPrice,
          string newPrice, object salesCode, string soDetail, bool bDeliveryUpdate)
        {
            var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
            var sUpdate = "UPDATE SO_Detail SET ";

            var sUpdateDelivery = "UPDATE Delivery SET ";

            sUpdate += "Promised_Date = '" + EscapeSQLString(date) + "', Last_Updated = getdate()";
            sUpdateDelivery += "Promised_Date = '" + EscapeSQLString(date) + "' , Requested_Date = '" + EscapeSQLString(date) + "' ";

            sUpdate += ", Order_Qty = " + EscapeSQLString(qty) + ", Deferred_Qty = " + EscapeSQLString(qty);
            sUpdateDelivery += ", Promised_Quantity = " + EscapeSQLString(qty) + ", Remaining_Quantity = " + EscapeSQLString(qty) + " ";

            sUpdate += ", Unit_Price = " + EscapeSQLString(unitPrice);
            sUpdate += ", Total_Price = " + EscapeSQLString(newPrice);
            sUpdate += ", SO_Line = '" + EscapeSQLString(line) + "'";

            if (salesCode != DBNull.Value)
            {
                sUpdate += ", Sales_Code = '" + EscapeSQLString(salesCode?.ToString()) + "' ";
            }

            sUpdate += ", Status = '" + EscapeSQLString(M_SO_CREATION_STATUS) + "'";

            sUpdate += " WHERE SO_Detail = " + EscapeSQLString(soDetail);
            sUpdateDelivery += "WHERE SO_Detail = " + EscapeSQLString(soDetail) + "; ";

            conn.SetData(sUpdate);

            if (!bDeliveryUpdate)
            {
                sUpdateDelivery = "INSERT INTO Delivery (SO_Detail, Requested_Date, Promised_Date,"
                + "Promised_Quantity, Remaining_Quantity, ObjectID) VALUES";

                sUpdateDelivery += " (" + EscapeSQLString(soDetail) + ", '" + EscapeSQLString(date) + "' , '" + EscapeSQLString(date) + "' ," + EscapeSQLString(qty)
                + ", " + EscapeSQLString(qty) + ", '" + EscapeSQLString(Guid.NewGuid().ToString()) + "')";
            }

            conn.SetData(sUpdateDelivery);
            conn.Dispose();
        }
        #endregion

        #region Pub methods
        public override DataTable GetExtraFeatureTable()
        {
            return extraFeaturesTable;
        }

        public override DataTable GetMergedTable() { return m_MergedTable; }

        public override void Read()
        {
            try
            {
                var progressinit = new ProgressInitEngine(OnProgressInit_Event);
                progressinit(100, "Création/Remplissage de la table de comparaison");

                CreateTheDataTable(out m_MergedTable);
                InsertCsvRows(m_MergedTable, m_DeliveryFileReader, m_iDeliveryDateFormat);

                var progressreport = new ProgressReportEngine(OnProgressReport_Event);
                progressreport(25);

                FillDataTable(ref m_MergedTable);

                if (m_MergedTable != null && m_MergedTable.Rows.Count > 0)
                {
                    m_MergedTable.Columns.Add("Id");

                    var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
                    for (int i = m_MergedTable.Rows.Count - 1; i >= 0; i--)
                    {
                        m_MergedTable.Rows[i]["Id"] = i;
                        if (!string.IsNullOrWhiteSpace(m_MergedTable.Rows[i][22].ToString()) && string.IsNullOrWhiteSpace(m_MergedTable.Rows[i][13].ToString()) && int.TryParse(m_MergedTable.Rows[i][1].ToString(), out int so))
                        {
                            string sql = "select a.Ship_To_ID from SO_Header h join Address a on a.Address = h.Ship_To where h.Sales_Order = '" + EscapeSQLString(m_MergedTable.Rows[i][1].ToString()) + "' and h.Customer = '" + EscapeSQLString(m_MergedTable.Rows[i][22].ToString()) + "';";
                            var dt = conn.GetData(sql);

                            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                                m_MergedTable.Rows[i][13] = dt.Rows[0][0].ToString();
                        }
                        //else
                        //{
                        //    m_MergedTable.Rows.RemoveAt(i);
                        //}

                        //if (m_MergedTable.Rows[i][1].ToString() != "69489")
                        //{
                        //    m_MergedTable.Rows.RemoveAt(i);
                        //}

                        //if (m_MergedTable.Rows[i]["Qty_Diff"].ToString() == "0" && m_MergedTable.Rows[i]["Date_Diff"].ToString() == "0")
                        //{
                        //    m_MergedTable.Rows.RemoveAt(i);
                        //}
                    }
                    conn.Dispose();
                }

                var completeproc = new CompleteEngineProcess(OnComplete_Event);
                completeproc();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //public override void Write()
        //{
        //    var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);

        //    var iCurrentLine = 0;

        //    while (iCurrentLine < m_MergedTable.Rows.Count)
        //    {
        //        var sClient = "";

        //        var sCurrentPart = m_MergedTable.Rows[iCurrentLine][0].ToString();
        //        var sSo = m_MergedTable.Rows[iCurrentLine][1].ToString();
        //        var sCurrentPo = m_MergedTable.Rows[iCurrentLine][2].ToString();

        //        if (m_MergedTable.Rows[iCurrentLine][22].ToString().Length > 0)
        //        {
        //            sClient = m_MergedTable.Rows[iCurrentLine][22].ToString();
        //        }

        //        var sTempSelect = "Customer_PO = '" + EscapeSQLString(sCurrentPo) + "' AND Material = '" + EscapeSQLString(sCurrentPart) + "'";
        //        if (sClient != "") { sTempSelect += " AND Client = '" + EscapeSQLString(sClient) + "'"; }
        //        var poRows = m_MergedTable.Select(sTempSelect);

        //        sTempSelect = "Customer_PO = '" + EscapeSQLString(sCurrentPo) + "' AND Material = '" + EscapeSQLString(sCurrentPart) + "' AND Source = 'JB' AND SO_Detail_Status <> 'Shipped'";
        //        if (sClient != "") { sTempSelect += " AND Client = '" + EscapeSQLString(sClient) + "'"; }
        //        var jbNotShipped = m_MergedTable.Select(sTempSelect);

        //        sTempSelect = "Customer_PO = '" + EscapeSQLString(sCurrentPo) + "' AND Material = '" + EscapeSQLString(sCurrentPart) + "' AND Source = 'CSV'";
        //        if (sClient != "") { sTempSelect += " AND Client = '" + EscapeSQLString(sClient) + "'"; }
        //        var csvOnlyRows = m_MergedTable.Select(sTempSelect);

        //        sTempSelect = "Customer_PO = '" + EscapeSQLString(sCurrentPo) + "' AND Material = '" + EscapeSQLString(sCurrentPart) + "' AND Source NOT IN ('JB', 'CSV')";
        //        if (sClient != "") { sTempSelect += " AND Client = '" + EscapeSQLString(sClient) + "'"; }
        //        var jbAndCsvRows = m_MergedTable.Select(sTempSelect);

        //        if (sSo != "")
        //        {
        //            for (var x = 0; x < jbAndCsvRows.Length; x++)
        //            {
        //                if (jbAndCsvRows[x]["SO_Detail_Status"].ToString() != "Shipped")
        //                {
        //                    UpdateRow(ref jbAndCsvRows[x]);
        //                }
        //            }

        //            var iSmallestCount = (jbNotShipped.Length < csvOnlyRows.Length) ? jbNotShipped.Length : csvOnlyRows.Length;

        //            for (var x = 0; x < iSmallestCount; x++)
        //            {
        //                UpdateRowUsing(ref jbNotShipped[x], ref csvOnlyRows[x]);
        //            }

        //            if (jbNotShipped.Length != csvOnlyRows.Length)
        //            {
        //                if (csvOnlyRows.Length > jbNotShipped.Length)     //We need to insert new rows
        //                {
        //                    for (var x = jbNotShipped.Length; x < csvOnlyRows.Length; x++)
        //                    {
        //                        AddSODetailLine(csvOnlyRows[x], sSo, csvOnlyRows[x][14].ToString());
        //                    }
        //                }
        //                else
        //                {
        //                    for (var x = csvOnlyRows.Length; x < jbNotShipped.Length; x++)
        //                    {
        //                        conn.SetData("DELETE FROM Delivery WHERE Delivery.SO_Detail = " + EscapeSQLString(jbNotShipped[x][16]?.ToString()));
        //                        conn.SetData("DELETE FROM SO_Detail WHERE SO_Detail.SO_Detail = " + EscapeSQLString(jbNotShipped[x][16]?.ToString()));
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            var newSo = CreateNewSalesOrder(m_MergedTable.Rows[iCurrentLine]);

        //            for (var i = 1; i < poRows.Length; i++)
        //            {
        //                AddSODetailLine(m_MergedTable.Rows[iCurrentLine + i], newSo, m_MergedTable.Rows[iCurrentLine + i][14].ToString());
        //            }
        //        }

        //        iCurrentLine += poRows.Length;
        //    }

        //    conn.Dispose();
        //    var completeproc = new CompleteEngineProcess(OnComplete_Event);
        //    completeproc();
        //}

        public override void Write(object obj)
        {
            try
            {
                //var dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
                //var timePattern = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
                var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
                var datagrid = (DataGridView)obj;
                var gridTable = new DataTable();

                foreach (DataGridViewColumn col in datagrid.Columns)
                {
                    gridTable.Columns.Add(col.HeaderText);
                }

                for (int i = 0; i < datagrid.Rows.Count; i++)
                {
                    int index = i;
                    for (int i2 = 0; i2 < datagrid.Rows.Count; i2++)
                    {
                        if (datagrid.Rows[i2].Cells["Id"].Value.ToString() == i.ToString())
                        {
                            index = i2;
                            break;
                        }
                    }

                    DataGridViewRow dgvrow = datagrid.Rows[index];

                    var row = gridTable.NewRow();
                    foreach (DataGridViewCell cell in dgvrow.Cells)
                    {
                        row[cell.ColumnIndex] = cell.Value;
                    }

                    gridTable.Rows.Add(row);
                }

                RemoveAllRefusedLines(ref gridTable);

                while (gridTable.Rows.Count > 0)
                {
                    var sClient = "";

                    var sCurrentPart = gridTable.Rows[0][0].ToString();
                    var sSo = gridTable.Rows[0][1].ToString();
                    var sCurrentPo = gridTable.Rows[0][2].ToString();

                    if (gridTable.Rows[0][22].ToString().Length > 0)
                    {
                        sClient = gridTable.Rows[0][22].ToString();
                        this.M_CUSTOMER_ID = sClient;
                    }

                    var sTempSelect = "PO = '" + EscapeSQLString(sCurrentPo) + "' AND Material = '" + EscapeSQLString(sCurrentPart) + "'";
                    if (sClient != "") { sTempSelect += " AND Client = '" + EscapeSQLString(sClient) + "'"; }
                    var poRows = gridTable.Select(sTempSelect);

                    if (sSo != "")
                    {
                        for (var i = 0; i < poRows.Length; i++)
                        {
                            if (poRows[i]["SO Detail Status"].ToString() == "Shipped") continue;

                            string sodetail = poRows[i][16]?.ToString();

                            if (poRows[i]["SO Detail Status"].ToString() == "Backorder")
                            {
                                //Close existing SO Detail
                                conn.SetData("UPDATE SO_Detail set Status = 'Closed', Last_Updated = getdate() WHERE SO_Detail.SO_Detail = " + EscapeSQLString(sodetail));
                            }
                            else
                            {
                                if (poRows[i]["Qty New"].ToString() == "0")
                                {
                                    if (!string.IsNullOrWhiteSpace(sodetail) && IsNumber(sodetail))
                                    {
                                        conn.SetData("DELETE FROM Packlist_Detail WHERE SO_Detail = " + EscapeSQLString(sodetail));
                                        conn.SetData("DELETE FROM Delivery WHERE Delivery.SO_Detail = " + EscapeSQLString(sodetail));
                                        conn.SetData("DELETE FROM SO_Detail WHERE SO_Detail.SO_Detail = " + EscapeSQLString(sodetail));
                                    }
                                }
                                else
                                {
                                    //var newPromisedDate = DateTime.ParseExact(gridTable.Rows[i][9].ToString(), dateFormat + " " + timePattern, CultureInfo.InvariantCulture);
                                    //var sDate = newPromisedDate.ToString("yyyy-MM-dd");
                                    //sodetail = GetSingleSoDetailForSalesOrderNotShipped(sSo, sDate);
                                    if (string.IsNullOrWhiteSpace(sodetail))
                                    {
                                        //Create SO Detail
                                        AddSODetailLine(gridTable.Rows[i], sSo, gridTable.Rows[i][14].ToString());
                                    }
                                    else //Update existing SO + SO detail
                                    {
                                        //poRows[i][16] = sodetail;
                                        UpdateRow(ref poRows[i]);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var newSo = CreateNewSalesOrder(gridTable.Rows[0]);

                        for (var i = 1; i < poRows.Length; i++)
                        {
                            //var newPromisedDate = DateTime.ParseExact(gridTable.Rows[0 + i][9].ToString(), dateFormat + " " + timePattern, CultureInfo.InvariantCulture);
                            //var sDate = newPromisedDate.ToString("yyyy-MM-dd");
                            //string sodetail = GetSingleSoDetailForSalesOrderNotShipped(sSo, sDate);
                            //if (string.IsNullOrWhiteSpace(sodetail))
                            //{
                            //Create SO Detail
                            AddSODetailLine(gridTable.Rows[0 + i], newSo, gridTable.Rows[0 + i][14].ToString());
                            //}
                            //else //Update existing SO + SO detail
                            //{
                            //    poRows[i][1] = newSo;
                            //    poRows[i][16] = sodetail;
                            //    UpdateRow(ref poRows[i]);
                            //}
                        }
                    }

                    //Remove all the rows we processed from gridTable
                    foreach (var row in poRows)
                    {
                        gridTable.Rows.Remove(row);
                    }
                }

                conn.Dispose();

                RemoveEmptyPackListDetails();

                var completeproc = new CompleteEngineProcess(OnComplete_Event);
                completeproc();
            }
            catch (Exception ex)
            {
                var completeproc = new CompleteEngineProcessWithMessage(OnCompleteWithMessage_Event);
                completeproc(ex.Message);
            }
        }

        private void WriteQueueToFile(IEnumerable writeQueue)
        {
            StreamWriter fw = new StreamWriter(extraFeaturesWriter);

            foreach (object obj in writeQueue)
            {
                fw.WriteLine(obj.ToString());
                fw.Flush();
            }

            extraFeaturesWriter.Close();
        }
        #endregion
    }
}
