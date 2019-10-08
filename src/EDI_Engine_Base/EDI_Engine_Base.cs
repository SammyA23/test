using MfgConnection;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace EDI
{
  public delegate void CompleteEngineProcess();
  public delegate void CompleteEngineProcessWithMessage(string message);
  public delegate void ProgressInitEngine(int iMaxValue, string sOperation);
  public delegate void ProgressReportEngine(int iIncrementBy);

  public class EDI_Engine_Base
  {
    public event CompleteEngineProcess OnComplete;
    public event CompleteEngineProcessWithMessage OnCompleteWithMessage;
    public event ProgressInitEngine OnProgressInit;
    public event ProgressReportEngine OnProgressReport;

    protected DataTable m_DeliveryTable;
    protected DataTable m_MergedTable;

    protected MFG.MFG_File_Template m_DeliveryFileTemplate;
    protected MFG.Files.CSV.CCsvReader m_DeliveryFileReader;
    protected MFG.Files.XLS.XlsReader m_DeliveryXlsFileReader;
    protected int m_iDeliveryDateFormat;

    protected string[][] m_MaterialsToBeIgnored;

    protected string M_CUSTOMER_ID;
    protected string M_DBNAME;
    protected string M_DBSERVER;
    protected string M_APPPATH;
    protected string M_SO_CREATION;
    protected string M_SO_CREATION_STATUS;
        protected bool useShippedLines;

    public EDI_Engine_Base(string sDbName, string sDbServer, string sCustId,
      string sAppPath, string sSOCreation, string sSOCreationStatus)
    {
      M_APPPATH = sAppPath;
      M_CUSTOMER_ID = sCustId;
      M_DBSERVER = sDbServer;
      M_DBNAME = sDbName;

      M_SO_CREATION = sSOCreation;
      M_SO_CREATION_STATUS = sSOCreationStatus;
    }

    protected int AcquireNextSalesOrderNumberAndIncrementAutoNumber()
    {
      var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
      var query = "SELECT Last_Nbr FROM Auto_Number WHERE Type LIKE 'SalesOrder'";
      DataTable dt = conn.GetData(query);

      int soNumber = Convert.ToInt32(dt.Rows[0][0]);
      soNumber++;

      var updateAutoNumberQuery = "UPDATE Auto_Number SET "
      + "Last_Nbr = " + soNumber.ToString() + " "
      + "WHERE Type LIKE 'SalesOrder'";

      conn.SetData(updateAutoNumberQuery);

      return soNumber;
    }

    protected void AddColumnToTable(ref DataTable dt, string columnName, string columnCaption,
      string columnType, int columnLength, bool canBeNull)
    {
      var tempCol = new DataColumn(columnName, Type.GetType(columnType));
      tempCol.AllowDBNull = canBeNull;
      tempCol.Caption = columnCaption;
      if (columnType == "System.String")
      {
        tempCol.MaxLength = Convert.ToInt32(columnLength);
      }


      dt.Columns.Add(tempCol);
    }

    protected void AddSODetailLine(DataRow dr, string sSONumber, string sLine)
    {
      var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
      string sDifferentialQty;
      string sDifferentialDate;

      string sMaterial = dr[0].ToString();
      string sPlant = dr[13].ToString();

      if (dr[22].ToString() != "")
      {
        this.M_CUSTOMER_ID = dr[22].ToString();
      }

      if (sPlant == "0")
      {
        MessageBox.Show("'Plant 0'...EDI essaie de crée une ligne alors qu'il devrait ne rien faire.");
      }

      if (dr[3] == DBNull.Value)
      {
        sDifferentialQty = dr[4].ToString();
      }
      else
      {
        sDifferentialQty = dr[3].ToString();
      }

      if (dr[10] == DBNull.Value)
      {
        sDifferentialDate = "0";
      }
      else
      {
        sDifferentialDate = dr[10].ToString();
      }

      var address = conn.GetData("SELECT Address FROM Address WHERE Customer LIKE '"
       + this.M_CUSTOMER_ID + "' AND Ship_To_ID LIKE '" + dr[13].ToString() + "'");

      try
      {
        if (address.Rows.Count < 1)
        {
          throw new MissingAddressFieldException("Le plant de livraison " + dr[13].ToString() +
           " n'existe pas dans votre base de données.");
        }
      }
      catch (MissingAddressFieldException e)
      {
        MessageBox.Show(e.Message);
        throw e;
      }

      var addressID = (address.Rows[0][0]).ToString();
      var dtInvCosting = conn.GetData("SELECT Inv_Cost_Method FROM Preferences");
      var sInvCostingMethod = dtInvCosting.Rows[0][0].ToString();
      var bHasStringWithSingleQuote = false;

      if (sMaterial.Contains("'"))
      {
        bHasStringWithSingleQuote = true;
      }

      string sQuery;

      if (bHasStringWithSingleQuote)
      {
        string sFixedMat = sMaterial.Replace("'", "''");
        sQuery = "SELECT Material, Selling_Price, Price_UofM, "
        + "Stocked_UofM, Cost_UofM, Sales_Code, Rev FROM Material WHERE Material LIKE '" + sFixedMat + "'";
      }
      else
      {
        sQuery = "SELECT Material, Selling_Price, Price_UofM, "
        + "Stocked_UofM, Cost_UofM, Sales_Code, Rev FROM Material WHERE Material LIKE '" + sMaterial + "'";
      }

      var dtMaterialInfo = conn.GetData(sQuery);

      string sUnitPrice = dtMaterialInfo.Rows[0][1].ToString();
      string sPriceUofM = dtMaterialInfo.Rows[0][2].ToString();
      string sOrderQty = dr[4].ToString();
      string sStockUofM = dtMaterialInfo.Rows[0][3].ToString();
      string sCostUofM = dtMaterialInfo.Rows[0][4].ToString();
      object oSalesCode = dtMaterialInfo.Rows[0][5];
      object oRev;

      if (this.M_CUSTOMER_ID == "PRINOTH")
      {
        oRev = dr[15].ToString();
      }
      else
      {
        oRev = dtMaterialInfo.Rows[0][6];
      }

      var arPromisedDate = this.FormatJBDate(dr[9].ToString());
      string sPromisedDate = arPromisedDate[1] + "/"
      + arPromisedDate[0] + "/" + arPromisedDate[2];

      string sTotalPrice = (Convert.ToDouble(sUnitPrice) * Convert.ToDouble(sOrderQty)).ToString();

      string sStatus = this.M_SO_CREATION_STATUS;


      //create the detail line
      if (bHasStringWithSingleQuote)
      {
        string sFixedMat = sMaterial.Replace("'", "''");
        sQuery = "INSERT INTO SO_Detail (Sales_Order, SO_Line, Ship_To, Status, Make_Buy"
        + ", Unit_Price, Price_UofM, Total_Price, Order_Qty, Stock_UofM"
        + ", Deferred_Qty, Promised_Date, Material, Cost_UofM, ObjectID , Unit_Cost"
        + ", Shipped_Qty, Returned_Qty";

        if (oSalesCode != DBNull.Value)
        {
          sQuery += ", Sales_Code";
        }

        if (oRev != DBNull.Value)
        {
          sQuery += ", Rev";
        }

        sQuery += ") VALUES ('" + sSONumber + "', '" + sLine + "', " + addressID
        + ", '" + sStatus + "', 'M', " + sUnitPrice + ", '" + sPriceUofM + "', "
        + sTotalPrice + ", " + sOrderQty + ", '" + sStockUofM + "'" + ", "
        + sOrderQty + ", '" + sPromisedDate + "', '" + sFixedMat + "', '"
        + sCostUofM + "', '" + Guid.NewGuid() + "', 0, 0, 0";

        if (oSalesCode != DBNull.Value)
        {
          sQuery += ", '" + oSalesCode.ToString() + "'";
        }

        if (oRev != DBNull.Value)
        {
          sQuery += ", '" + oRev.ToString() + "'";
        }

        sQuery += ")";
      }
      else
      {
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

        sQuery += ") VALUES ('" + sSONumber + "', '" + sLine + "', " + addressID + ", '" + sStatus + "', 'M', " + sUnitPrice
        + ", '" + sPriceUofM + "', " + sTotalPrice + ", " + sOrderQty + ", '" + sStockUofM + "'"
        + ", " + sOrderQty + ", '" + sPromisedDate + "', '" + sMaterial + "', '" + sCostUofM + "', '" + Guid.NewGuid() + "', 0, 0, 0";

        if (oSalesCode != DBNull.Value)
        {
          sQuery += ", '" + oSalesCode.ToString() + "'";
        }

        if (oRev != DBNull.Value)
        {
          sQuery += ", '" + oRev.ToString() + "'";
        }

        sQuery += ")";
      }


      conn.SetData(sQuery);

      var dtLatestSoDetail = conn.GetData("SELECT MAX(SO_Detail) FROM SO_Detail");

      sQuery = "INSERT INTO Delivery (SO_Detail, Requested_Date, Promised_Date, Promised_Quantity, Remaining_Quantity, ObjectID) "
      + "VALUES (" + dtLatestSoDetail.Rows[0][0].ToString() + ", '" + sPromisedDate + "', '" + sPromisedDate + "', "
      + sOrderQty + " , " + sOrderQty + ", '" + Guid.NewGuid() + "')";

      conn.SetData(sQuery);


      this.UpdateSOTotalPrice(sSONumber);
      this.AddToHistory(new string[] { sDifferentialQty, sDifferentialDate, sUnitPrice, sOrderQty });
    }

    protected void AddSODetailLineWithTaxCode(DataRow dr, string sSONumber, string sLine)
    {
      var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
      string sDifferentialQty;
      string sDifferentialDate;

      string taxCode = GetTaxCodeFromSO(sSONumber);

      string sMaterial = dr[0].ToString();
      string sPlant = dr[13].ToString();

      if (dr[22].ToString() != "")
      {
        this.M_CUSTOMER_ID = dr[22].ToString();
      }

      if (sPlant == "0")
      {
        MessageBox.Show("'Plant 0'...EDI essaie de crée une ligne alors qu'il devrait ne rien faire.");
      }

      if (dr[3] == DBNull.Value)
      {
        sDifferentialQty = dr[4].ToString();
      }
      else
      {
        sDifferentialQty = dr[3].ToString();
      }

      if (dr[10] == DBNull.Value)
      {
        sDifferentialDate = "0";
      }
      else
      {
        sDifferentialDate = dr[10].ToString();
      }

      var address = conn.GetData("SELECT Address FROM Address WHERE Customer LIKE '"
       + this.M_CUSTOMER_ID + "' AND Ship_To_ID LIKE '" + dr[13].ToString() + "'");

      try
      {
        if (address.Rows.Count < 1)
        {
          throw new MissingAddressFieldException("Le plant de livraison " + dr[13].ToString() +
           " n'existe pas dans votre base de données.");
        }
      }
      catch (MissingAddressFieldException e)
      {
        MessageBox.Show(e.Message);
        throw e;
      }

      var addressID = (address.Rows[0][0]).ToString();
      var dtInvCosting = conn.GetData("SELECT Inv_Cost_Method FROM Preferences");
      var sInvCostingMethod = dtInvCosting.Rows[0][0].ToString();
      var bHasStringWithSingleQuote = false;

      if (sMaterial.Contains("'"))
      {
        bHasStringWithSingleQuote = true;
      }

      string sQuery;

      if (bHasStringWithSingleQuote)
      {
        string sFixedMat = sMaterial.Replace("'", "''");
        sQuery = "SELECT Material, Selling_Price, Price_UofM, "
        + "Stocked_UofM, Cost_UofM, Sales_Code, Rev FROM Material WHERE Material LIKE '" + sFixedMat + "'";
      }
      else
      {
        sQuery = "SELECT Material, Selling_Price, Price_UofM, "
        + "Stocked_UofM, Cost_UofM, Sales_Code, Rev FROM Material WHERE Material LIKE '" + sMaterial + "'";
      }

      var dtMaterialInfo = conn.GetData(sQuery);

      string sUnitPrice = dtMaterialInfo.Rows[0][1].ToString();
      string sPriceUofM = dtMaterialInfo.Rows[0][2].ToString();
      string sOrderQty = dr[4].ToString();
      string sStockUofM = dtMaterialInfo.Rows[0][3].ToString();
      string sCostUofM = dtMaterialInfo.Rows[0][4].ToString();
      object oSalesCode = dtMaterialInfo.Rows[0][5];
      object oRev;

      if (this.M_CUSTOMER_ID == "PRINOTH")
      {
        oRev = dr[15].ToString();
      }
      else
      {
        oRev = dtMaterialInfo.Rows[0][6];
      }

      var arPromisedDate = this.FormatJBDate(dr[9].ToString());
      string sPromisedDate = arPromisedDate[1] + "/"
      + arPromisedDate[0] + "/" + arPromisedDate[2];

      string sTotalPrice = (Convert.ToDouble(sUnitPrice) * Convert.ToDouble(sOrderQty)).ToString();

      string sStatus = this.M_SO_CREATION_STATUS;


      //create the detail line
      if (bHasStringWithSingleQuote)
      {
        string sFixedMat = sMaterial.Replace("'", "''");
        sQuery = "INSERT INTO SO_Detail (Sales_Order, SO_Line, Ship_To, Status, Make_Buy"
        + ", Unit_Price, Price_UofM, Total_Price, Order_Qty, Stock_UofM"
        + ", Deferred_Qty, Promised_Date, Material, Cost_UofM, ObjectID , Unit_Cost"
        + ", Shipped_Qty, Returned_Qty";

        if (oSalesCode != DBNull.Value)
        {
          sQuery += ", Sales_Code";
        }

        if (oRev != DBNull.Value)
        {
          sQuery += ", Rev";
        }

        if(!String.IsNullOrWhiteSpace(taxCode))
        {
          sQuery += ", Tax_Code";
        }

        sQuery += ") VALUES ('" + sSONumber + "', '" + sLine + "', " + addressID
        + ", '" + sStatus + "', 'M', " + sUnitPrice + ", '" + sPriceUofM + "', "
        + sTotalPrice + ", " + sOrderQty + ", '" + sStockUofM + "'" + ", "
        + sOrderQty + ", '" + sPromisedDate + "', '" + sFixedMat + "', '"
        + sCostUofM + "', '" + Guid.NewGuid() + "', 0, 0, 0";

        if (oSalesCode != DBNull.Value)
        {
          sQuery += ", '" + oSalesCode.ToString() + "'";
        }

        if (oRev != DBNull.Value)
        {
          sQuery += ", '" + oRev.ToString() + "'";
        }

        if(!String.IsNullOrWhiteSpace(taxCode))
        {
          sQuery += ", '" + taxCode + "'";
        }

        sQuery += ")";
      }
      else
      {
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

        if(!String.IsNullOrWhiteSpace(taxCode))
        {
          sQuery += ", Tax_Code";
        }

        sQuery += ") VALUES ('" + sSONumber + "', '" + sLine + "', " + addressID + ", '" + sStatus + "', 'M', " + sUnitPrice
        + ", '" + sPriceUofM + "', " + sTotalPrice + ", " + sOrderQty + ", '" + sStockUofM + "'"
        + ", " + sOrderQty + ", '" + sPromisedDate + "', '" + sMaterial + "', '" + sCostUofM + "', '" + Guid.NewGuid() + "', 0, 0, 0";

        if (oSalesCode != DBNull.Value)
        {
          sQuery += ", '" + oSalesCode.ToString() + "'";
        }

        if (oRev != DBNull.Value)
        {
          sQuery += ", '" + oRev.ToString() + "'";
        }

        if(!String.IsNullOrWhiteSpace(taxCode))
        {
          sQuery += ", '" + taxCode + "'";
        }

        sQuery += ")";
      }


      conn.SetData(sQuery);

      var dtLatestSoDetail = conn.GetData("SELECT MAX(SO_Detail) FROM SO_Detail");

      sQuery = "INSERT INTO Delivery (SO_Detail, Requested_Date, Promised_Date, Promised_Quantity, Remaining_Quantity, ObjectID) "
      + "VALUES (" + dtLatestSoDetail.Rows[0][0].ToString() + ", '" + sPromisedDate + "', '" + sPromisedDate + "', "
      + sOrderQty + " , " + sOrderQty + ", '" + Guid.NewGuid() + "')";

      conn.SetData(sQuery);


      this.UpdateSOTotalPrice(sSONumber);
      this.AddToHistory(new string[] { sDifferentialQty, sDifferentialDate, sUnitPrice, sOrderQty });
    }

    protected void AddToHistory(string[] arrValues)
    {
      string sDirPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/EDI-History/";
      string sFileNameStart = "Rapport-" + DateTime.Now.ToShortDateString().Replace("/", "-");
      string sFileName = "";


      if (!Directory.Exists(sDirPath))
      {
        Directory.CreateDirectory(sDirPath);
      }

      sFileName = "History.csv";

      var dumpWriter = new MFG.Files.CSV.CCsvWriter(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EDI-History\\", sFileName);

      /*
        0 - Date of the Write



        1 - Pull Qty,
        2 - Push Qty,
        3 - Diff Qty,

        4 - Pull Qty Value,
        5 - Push Qty Vallue,
        6 - Diff Qty Value,



        7 - Pull Days,
        8 - Push Days,
        9 - Diff Days,

        10 - Pull Days Value,
        11 - Push Days Value,
        12 - Diff Days Value
      */

        var arrValuesToWrite = new string[13] { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };

        var dtToday = System.DateTime.Now;
        arrValuesToWrite[0] = dtToday.Year + "/" + dtToday.Month + "/" + dtToday.Day;

      if (System.Convert.ToInt32(arrValues[0]) > 0) //This is a quantity pull
      {
        arrValuesToWrite[1] = arrValues[0];
        arrValuesToWrite[3] = arrValues[0];

        arrValuesToWrite[4] = (System.Convert.ToInt32(arrValues[0]) * System.Convert.ToDouble(arrValues[2])).ToString();
        arrValuesToWrite[6] = (System.Convert.ToInt32(arrValues[0]) * System.Convert.ToDouble(arrValues[2])).ToString();
      }
      else if (System.Convert.ToInt32(arrValues[0]) < 0) //This is a quantity push
      {
        arrValuesToWrite[2] = (System.Convert.ToInt32(arrValues[0]) * -1).ToString();
        arrValuesToWrite[3] = arrValues[0];

        arrValuesToWrite[5] = (System.Convert.ToInt32(arrValues[0]) * -1 * System.Convert.ToDouble(arrValues[2])).ToString();
        arrValuesToWrite[6] = (System.Convert.ToInt32(arrValues[0]) * System.Convert.ToDouble(arrValues[2])).ToString();
      }

      if (System.Convert.ToInt32(arrValues[1]) < 0) //This is a date pull
      {
        arrValuesToWrite[7] = (System.Convert.ToInt32(arrValues[1]) * -1).ToString(); ;
        arrValuesToWrite[9] = (System.Convert.ToInt32(arrValues[1]) * -1).ToString(); ;

        arrValuesToWrite[10] = (System.Convert.ToInt32(arrValues[3]) * System.Convert.ToDouble(arrValues[2])).ToString();
        arrValuesToWrite[12] = (System.Convert.ToInt32(arrValues[3]) * System.Convert.ToDouble(arrValues[2])).ToString(); ;
      }
      else if (System.Convert.ToInt32(arrValues[1]) > 0) //This is a date push
      {
        arrValuesToWrite[8] = arrValues[1];
        arrValuesToWrite[9] = arrValues[1];

        arrValuesToWrite[11] = (System.Convert.ToInt32(arrValues[3]) * System.Convert.ToDouble(arrValues[2])).ToString();
        arrValuesToWrite[12] = (System.Convert.ToInt32(arrValues[3]) * -1 * System.Convert.ToDouble(arrValues[2])).ToString();
      }

      dumpWriter.AddToFile(arrValuesToWrite);
    }

    protected string AdjustMaterial(string sMat)
    {
      if (sMat[sMat.Length - 1] == ' ')
      {
        sMat = sMat.Remove(sMat.Length - 1);
      }

      return sMat.TrimStart('0');
    }

    protected string CreateNewSalesOrder(DataRow dr)
    {
      var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
      int iSONumber;

      var soNumber = conn.GetData(
        "SELECT Last_Nbr FROM Auto_Number WHERE Type LIKE 'SalesOrder'");
      iSONumber = Convert.ToInt32(soNumber.Rows[0][0].ToString());
      iSONumber++;

      soNumber.Dispose();
      conn.SetData("UPDATE Auto_Number SET Last_Nbr = " + (iSONumber).ToString() + " WHERE Type LIKE 'SalesOrder'");


      DataTable dtCustomerCurrency = conn.GetData("SELECT Currency_Def FROM Customer WHERE Customer LIKE '" + this.M_CUSTOMER_ID + "'");
      string sCustomerCurrency = dtCustomerCurrency.Rows[0][0].ToString();
      if (sCustomerCurrency == "")
      {
        dtCustomerCurrency = conn.GetData("SELECT System_Base_Currency FROM Preferences WHERE Preferences = 1");
        sCustomerCurrency = dtCustomerCurrency.Rows[0][0].ToString();
      }

      DateTime dtToday = DateTime.Now;
      string sToday = dtToday.Month + "/" + dtToday.Day + "/" + dtToday.Year;

      string PO = dr[2].ToString();

      string sStatus = this.M_SO_CREATION_STATUS;

      //create the sales order
      string sQuery;

      sQuery = "INSERT INTO SO_Header (Sales_Order, Customer, Customer_PO, Sales_Tax_Amt, Sales_Tax_Rate, Order_Date, Promised_Date, Status, "
      + "Total_Price, Trade_Currency, Currency_Conv_Rate)     VALUES ('" + iSONumber + "', '" + this.M_CUSTOMER_ID + "', '" + PO + "', 0.00, 0, '" + sToday + "', '" + sToday + "', '" + sStatus + "', 0, " + sCustomerCurrency + ", 1)";

      conn.SetData(sQuery);


      try
      {
        string sLineNumber = dr[14].ToString();

        if (dr[18].ToString() == "F")
        {
          sLineNumber = "FORE";
        }

        this.AddSODetailLine(dr, iSONumber.ToString(), sLineNumber);
      }
      catch (MissingAddressFieldException e)
      {
        throw e;
      }

      return System.Convert.ToString(iSONumber);
    }

    protected int[] FormatExcelDate(string excelDate, int DateFormat)
    {
      try
      {
        var returnArray = new int[3];

        var sDay = "";
        var sMonth = "";
        var sYear = "";
        var iDayPosition = 0;
        var iMonthPosition = 0;

        switch (DateFormat)
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
          int iCounter = 0;
          while (iCounter < 3)
          {
            int index;
            if (iCounter == iDayPosition)
            {
              index = excelDate.IndexOf('/');
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
              index = excelDate.IndexOf('/');
              if (index >= 0)
              {
                sMonth = excelDate.Substring(0, index);
                excelDate = excelDate.Remove(0, index + 1);
              }
              else
              {
                sMonth = excelDate;
              }
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
                excelDate = excelDate.Remove(0, sYear.Length + 1);
              }

              if (sYear.Length < 3)
              {
                sYear = "20" + sYear;
              }
            }

            iCounter++;
          }

          returnArray[0] = System.Convert.ToInt32(sDay);
          returnArray[1] = System.Convert.ToInt32(sMonth);
          returnArray[2] = System.Convert.ToInt32(sYear);
        }
        else
        {
          DateTime dtToday = DateTime.Now;
          returnArray[0] = dtToday.Day;
          returnArray[1] = dtToday.Month;
          returnArray[2] = dtToday.Year;
        }

        return returnArray;
      }
      catch (Exception e)
      {
        MessageBox.Show("Dans la fonction de date\n" + e.ToString());
      }

      return new int[] { 0, 1, 2 }; // TODO: Is this really necessary to keep the compiler from complaining abort return paths?
    }

    protected int[] FormatJBDate(string jbDate)
    {
      var val = DateTime.Parse(jbDate);
      var returnArray = new int[3];

      returnArray[0] = val.Day;
      returnArray[1] = val.Month;
      returnArray[2] = val.Year;

      return returnArray;
    }

    protected string GetAddressForShipToId(in string customer, in string shipTo)
    {
      var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
      var address = conn.GetData("SELECT Address FROM Address WHERE Customer LIKE '"
       + customer + "' AND Ship_To_ID LIKE '"
       + shipTo + "'");

      return address.Rows[0]["Address"].ToString();
    }

    protected string GetSalesOrderForPartAndPo(in string part, in string po,
      in string customer)
    {
      var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
      var query = "SELECT TOP 1 SOD.Sales_Order FROM SO_Detail AS SOD LEFT "
      + "JOIN SO_Header SOH ON SOD.Sales_Order = SOH.Sales_Order WHERE "
      + "SOD.Material LIKE '@part' AND SOH.Customer_PO LIKE '@po' AND "
      + "SOH.Customer LIKE '@customer' AND SOH.Status NOT LIKE 'Closed'";

      query = query.Replace("@po", po);
      query = query.Replace("@part", part);
      query = query.Replace("@customer", customer);

      var dt = conn.GetData(query);

      if(dt.Rows.Count > 0)
      {
        return dt.Rows[0]["Sales_Order"].ToString();
      }
      else
      {
        return "Nouveau Sales_Order";
      }
    }

    protected string GetSalesOrderForPartAndPoAndLine(in string part, in string po,
      in string customer, in string line)
    {
      var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
      var query = "SELECT TOP 1 SOD.Sales_Order FROM SO_Detail AS SOD LEFT "
      + "JOIN SO_Header SOH ON SOD.Sales_Order = SOH.Sales_Order WHERE "
      + "SOD.Material LIKE '@part' AND SOH.Customer_PO LIKE '@po' AND "
      + "SOH.Customer LIKE '@customer' AND SOH.Status NOT LIKE 'Closed'"
      + " AND SOD.SO_Line LIKE '@soline'";

      query = query.Replace("@po", po);
      query = query.Replace("@part", part);
      query = query.Replace("@customer", customer);
      query = query.Replace("@soline", line);

      var dt = conn.GetData(query);

      if(dt.Rows.Count > 0)
      {
        return dt.Rows[0]["Sales_Order"].ToString();
      }
      else
      {
        return "Nouveau Sales_Order";
      }
    }

    protected string GetTaxCodeFromSO(string so)
    {
      var conn = new jbConnection(M_DBNAME, M_DBSERVER);
      var query = "SELECT Tax_Code FROM SO_Header WHERE Sales_Order LIKE '" + so + "'";

      DataTable dt = conn.GetData(query);

      if(dt.Rows.Count < 1)
      {
        return "";
      }
      else
      {
        return dt.Rows[0][0].ToString();
      }
    }

    protected string GiveCorrectLength(string colType, string currentLength)
    {
      var typesToNull = new List<string>(new string[]{ "System.DateTime",
        "System.Double", "System.Int32", "datetime", "float", "double" });

      var answer = typesToNull.Find(x => x.Contains(colType));

      if (answer != null)
      {
        return null;
      }
      else
      {
        return currentLength;
      }
    }

    protected void PreemptiveVerification(ref DataTable dt)
    {
      var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);
      string sCurrentMat = "";
      string expression;

      DataTable matsAndPOs = new DataTable();
      matsAndPOs.Columns.Add("Part");
      matsAndPOs.Columns.Add("PO");
      matsAndPOs.Columns.Add("Plant");
      matsAndPOs.Columns.Add("Client");
      matsAndPOs.Columns.Add("Source Manquante");
      foreach (DataRow dr in dt.Rows)
      {
        sCurrentMat = dr[0].ToString();
        bool bHasStringWithSingleQuote = false;

        if (sCurrentMat.Contains("'"))
        {
          bHasStringWithSingleQuote = true;
        }

        if (bHasStringWithSingleQuote)
        {
          sCurrentMat = sCurrentMat.Replace("'", "''");
        }

        DataRow[] foundRows;
        expression = "Part LIKE '" + sCurrentMat + "' AND PO LIKE '" + dr[2].ToString() + "'";

        foundRows = matsAndPOs.Select(expression);

        if (foundRows.Length == 0)
        {
          matsAndPOs.Rows.Add(new string[] { sCurrentMat, dr[2].ToString(), dr[13].ToString(), dr[22].ToString() });
        }

      }

      var progressinit = new ProgressInitEngine(OnProgressInit_Event);
      progressinit(matsAndPOs.Rows.Count, "Vérification des pièces existantes dans JobBOSS.");

      string sMat;
      string sSelectJbMat;
      DataTable jbLines = new DataTable();
      DataTable requiredAction = matsAndPOs.Clone();

      foreach (DataRow row in matsAndPOs.Rows)
      {
        sMat = row[0].ToString();

        sSelectJbMat = "SELECT Material FROM Material WHERE Material LIKE '" + sMat + "'";

        jbLines = conn.GetData(sSelectJbMat);
        if (jbLines != null)
        {
          if (jbLines.Rows.Count < 1)
          {
            DataRow dr = row;
            dr[4] = "Pièce non existante";
            dr.AcceptChanges();

            requiredAction.ImportRow(dr);
          }
          else
          { //We have a material. This our queue for looking up if there is a Sales Order matching this PO, Part, JB Client combination
            DataTable jbSOs;
            string sQuery = "SELECT SO_Header.Sales_Order, SO_Header.Customer, SO_Header.Customer_PO, SOD.Material, SOD.Ship_To FROM SO_Header" +
            " LEFT JOIN (SELECT MIN(SO_Detail.SO_Detail) AS SOD, SO_Detail.Sales_Order, SO_Detail.Material, SO_Detail.Ship_To" +
            " FROM SO_Detail GROUP BY SO_Detail.Sales_Order, SO_Detail.Material, SO_Detail.Ship_To) AS SOD ON SOD.Sales_Order = SO_Header.Sales_Order" +
            " WHERE SOD.Material = '" + sMat + "' AND SO_Header.Customer_PO = '" + row[1] + "' AND SO_Header.Customer = '" + row[3] + "'" +
            " AND SO_Header.Status NOT IN ('Closed')";
            jbSOs = conn.GetData(sQuery);

            if (jbSOs.Rows.Count < 1)
            {
              DataRow dr = row;
              dr[4] = "Sales Order n'existe pas";
              dr.AcceptChanges();

              requiredAction.ImportRow(dr);
            }
          }

          requiredAction.AcceptChanges();
        }

        var progressreport = new ProgressReportEngine(OnProgressReport_Event);
        progressreport(1);

      }

      frmMissingParts missingMatsForm = new frmMissingParts(requiredAction);
      DialogResult ret = STAShowDialog(missingMatsForm);

      if (ret.ToString() == "OK")
      {
        MessageBox.Show("Prêt à continuer.");
        this.m_MaterialsToBeIgnored = missingMatsForm.MaterialsToIgnore();
      }
      else
      {
        MessageBox.Show("Nous devons arrêter l'exécution.");
        throw (new Exception());
      }
    }

    protected string ProperTypeString(string sType)
    {
      if (sType == "text" || sType == "varchar")
      {
        return "System.String";
      }
      else if (sType == "int")
      {
        return "System.Int32";
      }
      else if (sType == "double" || sType == "float")
      {
        return "System.Double";
      }
      else if (sType == "datetime")
      {
        return "System.DateTime";
      }
      else if (sType == "timepsan")
      {
        return "System.TimeSpan";
      }
      else
      {
        return "System.String";
      }
    }

    protected void RemoveMaterialsToBeIgnored(ref DataTable dt, ref string[][] arrayToIgnore)
    {
      for (int iArrayPosition = 0; iArrayPosition < arrayToIgnore.Length; iArrayPosition++)
      {
        DataRow[] rowsToRemove;
        if (arrayToIgnore[iArrayPosition][4].Contains("Sales"))
        {
          rowsToRemove = dt.Select("Material LIKE '"
            + arrayToIgnore[iArrayPosition][0] + "' AND "
            + "Customer_PO LIKE '" + arrayToIgnore[iArrayPosition][1] + "'");
        }
        else
        {
          rowsToRemove = dt.Select("Material LIKE '" + arrayToIgnore[iArrayPosition][0] + "'");
        }

        foreach (DataRow dr in rowsToRemove)
        {
          dr.Delete();
        }
      }
    }

    protected void RemoveRows(ref DataTable dtTemp, ref string sMat, ref string sPO)
    {
      int iCurrentRow = 0;

      while (iCurrentRow < dtTemp.Rows.Count)
      {
        string sCurrentLineMat = dtTemp.Rows[iCurrentRow][0].ToString();
        string sCurrentLinePO = dtTemp.Rows[iCurrentRow][2].ToString();

        if (sCurrentLineMat == sMat && sCurrentLinePO == sPO)
        {
          dtTemp.Rows[iCurrentRow].Delete();
        }
        else
        {
          iCurrentRow++;
        }
        dtTemp.AcceptChanges();
      }
    }

    protected DialogResult STAShowDialog(frmMissingParts dialog)
    {
      var state = new DialogState();
      state.dialog = dialog;
      var t = new System.Threading.Thread(new System.Threading.ThreadStart(state.ThreadProcShowDialog));
      t.SetApartmentState(System.Threading.ApartmentState.STA);
      t.Start();
      t.Join();
      return state.result;
    }

    protected void UpdateSOTotalPrice(string sSO)
    {
      var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);

      var sQuery = "UPDATE SO_Header SET Total_Price = (SELECT SUM(Total_Price) FROM SO_Detail WHERE Sales_Order LIKE '" + sSO + "') WHERE Sales_Order LIKE '" + sSO + "'";
      conn.SetData(sQuery);
    }

    protected virtual void PerformCalculationsForRow(ref DataTable m_DataTable, int iCurrentRow)
    {
    }

    protected void WriteToTextFile(string sTextFileName, string sStringToWrite)
    {
      StringBuilder sb = new StringBuilder();
      StreamWriter outfile = new StreamWriter("\\" + sTextFileName);

      sb.AppendLine(sStringToWrite);
      sb.AppendLine("= = = = = =");


      outfile.Write(sb);
      outfile.Close();
    }

    protected void OnProgressInit_Event(int count, string msg)
    {
      OnProgressInit(count, msg);
    }

    public void OnProgressInitOutside(int count, string msg)
    {
      OnProgressInit(count, msg);
    }

    protected void OnProgressReport_Event(int count)
    {
      OnProgressReport(count);
    }

    protected void OnComplete_Event()
    {
      OnComplete();
    }

    protected void OnCompleteWithMessage_Event(string message)
    {
      OnCompleteWithMessage(message);
    }

    private bool in_array(ref string value, ref List<string> target)
    {
      //TODO (@mond): Is this method in actual use, do we need to keep and more importantly should we implement it because this is clearly not implemented....
      return true;
    }

    public virtual void ExecuteExtraFeature(string featureName, string[] args = null) { }

    public virtual void Read() { }

    public string GetContactForCustomerAndContactName(
      in string customer, in string contactName)
    {
      var conn = new jbConnection(this.M_DBNAME, this.M_DBSERVER);

      var query = "SELECT Contact FROM Contact WHERE Customer LIKE '@cust' "
      + "AND Contact_Name LIKE '@contact'";

      query = query.Replace("@cust", customer);
      query = query.Replace("@contact", contactName);

      DataTable dt = conn.GetData(query);

      if( dt.Rows.Count > 0)
      {
        return dt.Rows[0][0].ToString();
      }

      return "NoContact";
    }

    public virtual DataTable GetExtraFeatureTable()
    {
      return null;
    }

    public virtual DataTable GetMergedTable()
    {
      return this.m_MergedTable;
    }

    public virtual void Write() { }

    public virtual void Write(object parentDataGrid)
    {
    }
  }
}
