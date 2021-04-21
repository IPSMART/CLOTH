using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Globalization;
using Oracle.ManagedDataAccess.Client;
using OfficeOpenXml;
//using NDbfReader;
namespace Improvar.Controllers
{
    public class T_PBillUploadController : Controller
    {
        string tmpPath = "C:\\IPSMART\\Temp";
        string sql = ""; string dbsql = ""; string[] dbsql1;
        string dberrmsg = "";
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        Salesfunc Salesfunc = new Salesfunc();
        private ImprovarDB DB, DBF;
        public T_PBillUploadController()
        {
            DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
        }
        // GET: T_PBillUpload
        public ActionResult T_PBillUpload()
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                ViewBag.formname = "Data Upload";
                DataUploadVM VE = new DataUploadVM();
                if (!System.IO.Directory.Exists(tmpPath)) { System.IO.Directory.CreateDirectory(tmpPath); }
                return View(VE);
            }
        }

        public ActionResult GetSubLedgerDetails(string val, string Code)
        {
            try
            {
                var agent = Code.Split(Convert.ToChar(Cn.GCS()));
                if (agent.Count() > 1)
                {
                    if (agent[1] == "")
                    {
                        return Content("Please Select Agent !!");
                    }
                    else
                    {
                        Code = agent[0];
                    }
                }
                var str = masterHelp.SLCD_help(val, Code);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        [HttpPost]
        public ActionResult ExtractPurchase()
        {
            if (CommVar.ClientCode(UNQSNO) == "BNBH")
            {
                return ExtractRaymondPurchaseDBF();
            }
            else if (CommVar.ClientCode(UNQSNO) == "LALF")
            {
                return ExtractAdittyaBirlaPTxls();
            }
            return Content(CommVar.ClientCode(UNQSNO));
        }

        public ActionResult ExtractRaymondPurchaseDBF()
        {
            try
            {
                DataUploadVM VE = new DataUploadVM();
                if (Request.Files.Count > 0)
                {
                    if (!System.IO.Directory.Exists(tmpPath)) { System.IO.Directory.CreateDirectory(tmpPath); }
                    tmpPath = "C:\\IPSMART\\Temp\\Raymond.dbf";
                    if (System.IO.File.Exists(tmpPath)) { System.IO.File.Delete(tmpPath); }
                    GC.Collect();
                    HttpFileCollectionBase files = Request.Files;
                    HttpPostedFileBase file = files[0];
                    string fname;
                    if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                    {
                        string[] testfiles = file.FileName.Split(new char[] { '\\' });
                        fname = testfiles[testfiles.Length - 1];
                    }
                    else
                    {
                        fname = file.FileName;
                    }
                    fname = System.IO.Path.Combine(tmpPath, "");
                    file.SaveAs(fname);

                    System.Data.Odbc.OdbcConnection obdcconn = new System.Data.Odbc.OdbcConnection();
                    obdcconn.ConnectionString = "Driver={Microsoft dBase Driver (*.dbf)};SourceType=DBF;SourceDB=" + tmpPath + ";Exclusive=No; NULL=NO;DELETED=NO;BACKGROUNDFETCH=NO;";
                    obdcconn.Open();
                    System.Data.Odbc.OdbcCommand oCmd = obdcconn.CreateCommand();
                    oCmd.CommandText = "SELECT * FROM " + tmpPath;
                    DataTable dbfdt = new DataTable();
                    dbfdt.Load(oCmd.ExecuteReader());
                    obdcconn.Close();
                    if (System.IO.File.Exists(tmpPath)) { System.IO.File.Delete(tmpPath); }
                    var outerDT = dbfdt.AsEnumerable()
                   .GroupBy(g => new { CUSTOMERNO = g["CUSTOMERNO"], INV_NO = g["INV_NO"], INVDATE = g["INVDATE"], LR_NO = g["LR_NO"], LR_DATE = g["LR_DATE"], CARR_NO = g["CARR_NO"], CARR_NAME = g["CARR_NAME"] })
                   .Select(g =>
                   {
                       var row = dbfdt.NewRow();
                       row["CUSTOMERNO"] = g.Key.CUSTOMERNO;
                       row["INV_NO"] = g.Key.INV_NO;
                       row["INVDATE"] = g.Key.INVDATE;
                       row["LR_NO"] = g.Key.LR_NO;
                       row["LR_DATE"] = g.Key.LR_DATE;
                       row["CARR_NO"] = g.Key.CARR_NO;
                       row["CARR_NAME"] = g.Key.CARR_NAME;
                       row["FREIGHT"] = g.Sum(r => r.Field<double>("FREIGHT"));
                       row["INSURANCE"] = g.Sum(r => r.Field<double>("INSURANCE"));
                       row["INV_VALUE"] = g.Sum(r => r.Field<double>("INV_VALUE"));
                       row["NET_AMT"] = g.Sum(r => r.Field<double>("NET_AMT"));
                       row["TAX_AMT"] = g.Sum(r => r.Field<double>("TAX_AMT"));
                       row["INTEGR_TAX"] = g.Average(r => r.Field<double>("INTEGR_TAX"));
                       row["INTEGR_AMT"] = g.Sum(r => r.Field<double>("INTEGR_AMT"));
                       row["CENT_TAX"] = g.Average(r => r.Field<double>("CENT_TAX"));
                       row["CENT_AMT"] = g.Sum(r => r.Field<double>("CENT_AMT"));
                       row["STATE_TAX"] = g.Average(r => r.Field<double>("STATE_TAX"));
                       row["STATE_AMT"] = g.Sum(r => r.Field<double>("STATE_AMT"));
                       return row;
                   }).CopyToDataTable();

                    List<DUpGrid> DUGridlist = new List<DUpGrid>();
                    int slno = 0;
                    foreach (DataRow oudr in outerDT.Rows)
                    {
                        DUpGrid dupgrid = new DUpGrid();
                        dupgrid.Slno = ++slno;
                        dupgrid.BLNO = oudr["INV_NO"].retStr();
                        dupgrid.BLDT = oudr["INVDATE"].retDateStr();
                        dupgrid.Checked = true;
                        DUGridlist.Add(dupgrid);
                    }
                    VE.DUpGrid = DUGridlist;
                }
                VE.DefaultView = true;
                ModelState.Clear();
                return PartialView("_T_PBillUpload_Bill", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Json("Error occurred. Error details: " + ex.Message);
            }
        }

        public ActionResult ExtractAdittyaBirlaPTxls()
        {
            try
            {
                DataUploadVM VE = new DataUploadVM();
                if (Request.Files.Count > 0)
                {
                    DataTable dbfdt = new DataTable();
                    dbfdt.Columns.Add("EXCELROWNUM", typeof(int));
                    dbfdt.Columns.Add("BLNO", typeof(string));
                    dbfdt.Columns.Add("BLDT", typeof(string));

                    HttpFileCollectionBase files = Request.Files;
                    HttpPostedFileBase file = files[0];
                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var currentSheet = package.Workbook.Worksheets;
                        var workSheet = currentSheet.First();
                        var noOfCol = workSheet.Dimension.End.Column;
                        var noOfRow = workSheet.Dimension.End.Row;
                        int rowNum = 2;
                        for (rowNum = 2; rowNum <= noOfRow; rowNum++)
                        {
                            DataRow dr = dbfdt.NewRow();
                            dr["EXCELROWNUM"] = rowNum;
                            string BLNO = workSheet.Cells[rowNum, 7].Value.retStr();
                            string BLDT = workSheet.Cells[rowNum, 10].Value.retDateStr();
                            dr["BLNO"] = BLNO;
                            dr["BLDT"] = BLDT;
                            dbfdt.Rows.Add(dr);
                        }
                    }

                    var outerDT = dbfdt.AsEnumerable()
                   .GroupBy(g => new { BLNO = g["BLNO"], BLDT = g["BLDT"] })
                   .Select(g =>
                   {
                       var row = dbfdt.NewRow();
                       row["BLNO"] = g.Key.BLNO;
                       row["BLDT"] = g.Key.BLDT;
                       return row;
                   }).CopyToDataTable();

                    List<DUpGrid> DUGridlist = new List<DUpGrid>();
                    int slno = 0;
                    foreach (DataRow oudr in outerDT.Rows)
                    {
                        DUpGrid dupgrid = new DUpGrid();
                        dupgrid.Slno = ++slno;
                        dupgrid.BLNO = oudr["BLNO"].retStr();
                        dupgrid.BLDT = oudr["BLDT"].retDateStr();
                        dupgrid.Checked = true;
                        DUGridlist.Add(dupgrid);
                    }
                    VE.DUpGrid = DUGridlist;
                }
                VE.DefaultView = true;
                ModelState.Clear();
                return PartialView("_T_PBillUpload_Bill", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Json("Error occurred. Error details: " + ex.Message);
            }
        }




        [HttpPost]
        public ActionResult T_PBillUpload(DataUploadVM VE, FormCollection FC, String Command)
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Request.Files.Count == 0) return Content("No File Selected");
            if (VE.DUpGrid.Where(m => m.Checked == true).FirstOrDefault() == null)
            {
                return Content("Please Select a Purchase bill For upload");
            }
            sql = "select * from " + CommVar.CurSchema(UNQSNO) + ".m_group ";
            var dt = masterHelp.SQLquery(sql);
            if (dt.Rows.Count == 0)
            {
                return Content("Please create a group Master");
            }
            if (CommVar.Compcd(UNQSNO) == "BNBH")
            {
                VE = ReadRaymondPurchaseDBF(VE);
            }
            else if (CommVar.Compcd(UNQSNO) == "LALF")
            {
                VE = ReadAdityaBirlaPTfile(VE);
            }
            return View(VE);
        }

        public DataUploadVM ReadRaymondPurchaseDBF(DataUploadVM VE)
        {
            List<DUpGrid> DUGridlist = new List<DUpGrid>();
            try
            {
                //Enable 32 bit application from IIS. to run dbf and ins  32 bit cryastal report 32 bit.
                string Path = "C:\\IPSMART\\Temp";
                if (!System.IO.Directory.Exists(Path)) { System.IO.Directory.CreateDirectory(Path); }
                Path = "C:\\IPSMART\\Temp\\Raymond.dbf";
                if (System.IO.File.Exists(Path)) { System.IO.File.Delete(Path); }
                GC.Collect();
                Request.Files["filePurchase"].SaveAs(Path);

                System.Data.Odbc.OdbcConnection obdcconn = new System.Data.Odbc.OdbcConnection();
                obdcconn.ConnectionString = "Driver={Microsoft dBase Driver (*.dbf)};SourceType=DBF;SourceDB=" + Path + ";Exclusive=No; NULL=NO;DELETED=NO;BACKGROUNDFETCH=NO;";
                obdcconn.Open();
                System.Data.Odbc.OdbcCommand oCmd = obdcconn.CreateCommand();
                oCmd.CommandText = "SELECT * FROM " + Path;
                DataTable dbfdt = new DataTable();
                dbfdt.Load(oCmd.ExecuteReader());
                obdcconn.Close();
                if (System.IO.File.Exists(Path)) { System.IO.File.Delete(Path); }

                TransactionSaleEntry TMPVE = new TransactionSaleEntry();
                T_SALEController TSCntlr = new T_SALEController();
                T_TXN TTXN = new T_TXN();
                T_TXNTRANS TXNTRANS = new T_TXNTRANS();
                T_TXNOTH TTXNOTH = new T_TXNOTH();
                TMPVE.DefaultAction = "A";
                TMPVE.MENU_PARA = "PB";
                var outerDT = dbfdt.AsEnumerable()
               .GroupBy(g => new { CUSTOMERNO = g["CUSTOMERNO"], GSTINPLANT = g["GSTINPLANT"], INV_NO = g["INV_NO"], INVDATE = g["INVDATE"], LR_NO = g["LR_NO"], LR_DATE = g["LR_DATE"], CARR_NO = g["CARR_NO"], CARR_NAME = g["CARR_NAME"] })
               .Select(g =>
               {
                   var row = dbfdt.NewRow();
                   row["CUSTOMERNO"] = g.Key.CUSTOMERNO;
                   row["GSTINPLANT"] = g.Key.GSTINPLANT;
                   row["INV_NO"] = g.Key.INV_NO;
                   row["INVDATE"] = g.Key.INVDATE;
                   row["LR_NO"] = g.Key.LR_NO;
                   row["LR_DATE"] = g.Key.LR_DATE;
                   row["CARR_NO"] = g.Key.CARR_NO;
                   row["CARR_NAME"] = g.Key.CARR_NAME;
                   row["FREIGHT"] = g.Sum(r => r.Field<double>("FREIGHT"));
                   row["INSURANCE"] = g.Sum(r => r.Field<double>("INSURANCE"));
                   row["INV_VALUE"] = g.Sum(r => r.Field<double>("INV_VALUE"));
                   row["NET_AMT"] = g.Sum(r => r.Field<double>("NET_AMT"));
                   row["TAX_AMT"] = g.Sum(r => r.Field<double>("TAX_AMT"));
                   row["INTEGR_TAX"] = g.Average(r => r.Field<double>("INTEGR_TAX"));
                   row["INTEGR_AMT"] = g.Sum(r => r.Field<double>("INTEGR_AMT"));
                   row["CENT_TAX"] = g.Average(r => r.Field<double>("CENT_TAX"));
                   row["CENT_AMT"] = g.Sum(r => r.Field<double>("CENT_AMT"));
                   row["STATE_TAX"] = g.Average(r => r.Field<double>("STATE_TAX"));
                   row["STATE_AMT"] = g.Sum(r => r.Field<double>("STATE_AMT"));
                   return row;
               }).CopyToDataTable();

                TTXN.EMD_NO = 0;
                TTXN.DOCCD = DB.M_DOCTYPE.Where(d => d.DOCTYPE == "SPBL").FirstOrDefault()?.DOCCD;
                TTXN.CLCD = CommVar.ClientCode(UNQSNO);
                // freai j, pf=i,ins k
                string AMTCD_FREIGHT = DB.M_AMTTYPE.Where(m => m.CALCCODE == "J").FirstOrDefault()?.AMTCD;
                string AMTCD_INSURANCE = DB.M_AMTTYPE.Where(m => m.CALCCODE == "K").FirstOrDefault()?.AMTCD;
                string AMTCD_PackFordng = DB.M_AMTTYPE.Where(m => m.CALCCODE == "I").FirstOrDefault()?.AMTCD;
                //
                foreach (DataRow oudr in outerDT.Rows)
                {
                    short txnslno = 0;
                    List<TBATCHDTL> TBATCHDTLlist = new List<Models.TBATCHDTL>();
                    List<TTXNDTL> TTXNDTLlist = new List<Models.TTXNDTL>();
                    List<TTXNAMT> TTXNAMTlist = new List<Models.TTXNAMT>();
                    DUpGrid dupgrid = new DUpGrid();
                    TTXN.GOCD = "TR";
                    TTXN.DOCTAG = "PB";
                    TTXN.PREFNO = oudr["INV_NO"].ToString();

                    if (VE.DUpGrid.Where(m => m.Checked == true & m.BLNO == TTXN.PREFNO).FirstOrDefault() == null)
                    {//Selected row will upload only
                        continue;
                    }
                    TTXN.TCSPER = 0;// 0.075;
                    dupgrid.BLNO = TTXN.PREFNO;
                    string Ddate = DateTime.ParseExact(oudr["INVDATE"].retDateStr(), "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
                    TTXN.DOCDT = Convert.ToDateTime(Ddate);
                    dupgrid.BLDT = Ddate;
                    TTXN.PREFDT = TTXN.DOCDT;
                    dupgrid.BLNO = TTXN.PREFNO;
                    string CUSTOMERNO = oudr["CUSTOMERNO"].ToString();
                    string GSTINPLANT = oudr["GSTINPLANT"].ToString();
                    TTXN.SLCD = getSLCD(CUSTOMERNO, GSTINPLANT); dupgrid.CUSTOMERNO = CUSTOMERNO;
                    if (TTXN.SLCD == "")
                    {
                        dupgrid.MESSAGE = "Please add Customer No:(" + CUSTOMERNO + ") and GSTNO=" + GSTINPLANT + " in the SAPCODE from [Tax code link up With Party].";
                        DUGridlist.Add(dupgrid);
                        break;
                    }
                    double igstper = oudr["INTEGR_TAX"].retDbl();
                    if (igstper == 0) { TTXNOTH.TAXGRPCD = "C001"; } else { TTXNOTH.TAXGRPCD = "I001"; }
                    TTXNOTH.PRCCD = "CP";
                    double cgstper = oudr["CENT_AMT"].retDbl();
                    double gstper = igstper == 0 ? (cgstper * 2) : igstper;
                    double blINV_VALUE = oudr["INV_VALUE"].retDbl();
                    double bltaxable = oudr["TAX_AMT"].retDbl();
                    double calculatedTax = Math.Round(((bltaxable * gstper) / 100), 2);
                    double calcultednet = (bltaxable + calculatedTax);//.toRound(2);
                    var roffamt = (blINV_VALUE - calcultednet).toRound(2);
                    double blTAX_AMT = oudr["TAX_AMT"].retDbl();
                    double tcsamt = 0;// (blINV_VALUE * TTXN.TCSPER.retDbl() / 100).toRound(2);
                    TTXN.BLAMT = blINV_VALUE + tcsamt;
                    //TTXN.TDSCODE = "X";
                    //TTXN.ROYN = "Y";
                    TMPVE.RoundOff = true;
                    TTXN.TCSON = calcultednet;
                    TTXN.TCSAMT = tcsamt; dupgrid.TCSAMT = tcsamt.ToString();
                    sql = "";
                    sql = "select a.autono,b.docno,a.SLCD,a.blamt,a.tcsamt,a.ROAMT  from  " + CommVar.CurSchema(UNQSNO) + ".t_txn a, " + CommVar.CurSchema(UNQSNO) + ".t_cntrl_hdr b ";
                    sql += " where   a.autono=b.autono and a.PREFNO='" + TTXN.PREFNO + "' and a.slcd='" + TTXN.SLCD + "' ";
                    var dt = masterHelp.SQLquery(sql);
                    if (dt.Rows.Count > 0)
                    {
                        dupgrid.MESSAGE = "Allready Added at docno:" + dt.Rows[0]["docno"].ToString();
                        dupgrid.BLNO = TTXN.PREFNO;
                        dupgrid.TCSAMT = dt.Rows[0]["tcsamt"].ToString();
                        dupgrid.BLAMT = dt.Rows[0]["blamt"].ToString();
                        dupgrid.ROAMT = dt.Rows[0]["ROAMT"].ToString();
                        DUGridlist.Add(dupgrid);
                        continue;
                    }
                    else
                    {
                        dupgrid.TCSAMT = TTXN.TCSAMT.retStr();
                        dupgrid.BLAMT = TTXN.BLAMT.retStr();
                    }

                    //-------------------------Transport--------------------------//

                    if (oudr["CARR_NO"].ToString() != "")
                    {
                        TXNTRANS.TRANSLCD = getSLCD(oudr["CARR_NO"].ToString(), "");
                        if (TXNTRANS.TRANSLCD == "")
                        {
                            dupgrid.MESSAGE = "Please add  CARR_NO:(" + oudr["CARR_NO"].ToString() + ")/ Transporter,CARR_NAME:(" + oudr["CARR_NAME"].ToString() + ") in the SAPCODE from [Tax code link up With Party].";
                            DUGridlist.Add(dupgrid); break;
                        }
                    }

                    string LR_DATE = DateTime.ParseExact(oudr["LR_DATE"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
                    TXNTRANS.LRNO = oudr["LR_NO"].ToString();
                    TXNTRANS.LRDT = Convert.ToDateTime(LR_DATE);
                    //----------------------------------------------------------//
                    string PURGLCD = "";

                    DataTable innerDt = dbfdt.Select("INV_NO='" + TTXN.PREFNO + "'").CopyToDataTable();
                    double txable = 0, gstamt = 0; short batchslno = 0;
                    foreach (DataRow inrdr in innerDt.Rows)
                    {
                        double amttabigstamt = 0; double amttabcgstamt = 0;
                        //Amount tab start
                        if (inrdr["FREIGHT"].retDbl() != 0)
                        {
                            if (igstper > 0)
                            {
                                amttabigstamt += (inrdr["FREIGHT"].retDbl() * igstper / 100).toRound(2);
                            }
                            else
                            {
                                amttabcgstamt += (inrdr["FREIGHT"].retDbl() * cgstper / 100).toRound(2);
                            }
                        }
                        if (inrdr["INSURANCE"].retDbl() != 0)
                        {
                            if (igstper > 0)
                            {
                                amttabigstamt += (inrdr["INSURANCE"].retDbl() * igstper / 100).toRound(2);
                            }
                            else
                            {
                                amttabcgstamt += (inrdr["INSURANCE"].retDbl() * cgstper / 100).toRound(2);
                            }
                        }
                        //detail tab start
                        TTXNDTL TTXNDTL = new TTXNDTL();
                        string style = inrdr["MAT_GRP"].ToString() + inrdr["MATERIAL"].ToString().Split('-')[0];
                        string grpnm = inrdr["MAT_DESCRI"].ToString();
                        string HSNCODE = inrdr["HSN_CODE"].ToString();
                        ItemDet ItemDet = Salesfunc.CreateItem(style, TTXNDTL.UOM, grpnm, HSNCODE, "", "", "F", "C", "");
                        TTXNDTL.ITCD = ItemDet.ITCD; PURGLCD = ItemDet.PURGLCD;
                        TTXNDTL.ITSTYLE = style;
                        TTXNDTL.MTRLJOBCD = "FS";

                        TTXNDTL.STKDRCR = "D";
                        TTXNDTL.STKTYPE = "F";
                        TTXNDTL.HSNCODE = HSNCODE;
                        //TTXNDTL.ITREM = VE.TTXNDTL[i].ITREM;
                        //TTXNDTL.BATCHNO = inrdr["BATCH"].ToString();
                        TTXNDTL.BALENO = inrdr["BALENO"].ToString();
                        TTXNDTL.GOCD = "TR";
                        TTXNDTL.UOM = "MTR";
                        TTXNDTL.QNTY = inrdr["GROSS_QTY"].retDbl(); // NET_QTY
                        TTXNDTL.NOS = 1;
                        TTXNDTL.RATE = inrdr["RATE"].retDbl();
                        TTXNDTL.AMT = inrdr["GROSS_AMT"].retDbl();
                        TTXNDTL.FLAGMTR = inrdr["W_FLG_Q"].retDbl();
                        string grade = inrdr["GRADATION"].ToString();
                        string foc = inrdr["FOC"].ToString();
                        string pCSTYPE = PCSTYPE(grade, foc);
                        double W_FLG_Q = Math.Abs(inrdr["W_FLG_Q"].retDbl());
                        double R_FLG_Q = Math.Abs(inrdr["R_FLG_Q"].retDbl());
                        double discamt = Math.Abs(inrdr["QLTY_DISC"].retDbl());
                        double discamt1 = Math.Abs(inrdr["MKTG_DISC"].retDbl());
                        double Flagamt = (W_FLG_Q + R_FLG_Q) * TTXNDTL.RATE.retDbl();
                        TTXNDTL.TOTDISCAMT = Flagamt;
                        TTXNDTL.DISCTYPE = "F";
                        TTXNDTL.DISCRATE = discamt;
                        TTXNDTL.DISCAMT = discamt;
                        TTXNDTL.SCMDISCTYPE = "F";
                        TTXNDTL.SCMDISCRATE = discamt1;
                        TTXNDTL.SCMDISCAMT = discamt1;
                        TTXNDTL.GLCD = PURGLCD;
                        TTXNDTL.TXBLVAL = inrdr["NET_AMT"].retDbl(); txable += TTXNDTL.TXBLVAL.retDbl();

                        TTXNDTL.IGSTPER = inrdr["INTEGR_TAX"].retDbl();
                        TTXNDTL.CGSTPER = inrdr["CENT_TAX"].retDbl();
                        TTXNDTL.SGSTPER = inrdr["STATE_TAX"].retDbl();
                        TTXNDTL.GSTPER = TTXNDTL.IGSTPER.retDbl() + TTXNDTL.CGSTPER.retDbl() + TTXNDTL.SGSTPER.retDbl();

                        TTXNDTL.IGSTAMT = inrdr["INTEGR_AMT"].retDbl() - amttabigstamt; gstamt += TTXNDTL.IGSTAMT.retDbl();
                        TTXNDTL.CGSTAMT = inrdr["CENT_AMT"].retDbl() - amttabcgstamt; gstamt += TTXNDTL.CGSTAMT.retDbl();
                        TTXNDTL.SGSTAMT = inrdr["STATE_AMT"].retDbl() - amttabcgstamt; gstamt += TTXNDTL.SGSTAMT.retDbl();
                        //double NET_AMT = ((TTXNDTL.TXBLVAL * (100 + gstper)) / 100).retDbl();
                        double NET_AMT = TTXNDTL.TXBLVAL.retDbl() + TTXNDTL.CGSTAMT.retDbl() + TTXNDTL.SGSTAMT.retDbl() + TTXNDTL.IGSTAMT.retDbl();
                        TTXNDTL.NETAMT = NET_AMT.toRound(2);
                        TTXNDTL tmpTTXNDTL = TTXNDTLlist.Where(r => r.BALENO == TTXNDTL.BALENO && r.HSNCODE == TTXNDTL.HSNCODE && r.ITCD == TTXNDTL.ITCD && r.STKTYPE == TTXNDTL.STKTYPE && r.RATE == TTXNDTL.RATE && r.FLAGMTR == TTXNDTL.FLAGMTR && r.DISCRATE == TTXNDTL.DISCRATE && r.SCMDISCRATE == TTXNDTL.SCMDISCRATE).FirstOrDefault();
                        if (tmpTTXNDTL != null)
                        {
                            foreach (var tmpdtl in TTXNDTLlist.Where(r => r.BALENO == TTXNDTL.BALENO && r.HSNCODE == TTXNDTL.HSNCODE && r.ITCD == TTXNDTL.ITCD && r.STKTYPE == TTXNDTL.STKTYPE && r.RATE == TTXNDTL.RATE && r.FLAGMTR == TTXNDTL.FLAGMTR && r.DISCRATE == TTXNDTL.DISCRATE && r.SCMDISCRATE == TTXNDTL.SCMDISCRATE))
                            {
                                tmpdtl.NOS += TTXNDTL.NOS;
                                tmpdtl.QNTY += TTXNDTL.QNTY;
                                tmpdtl.AMT += TTXNDTL.AMT;
                                tmpdtl.TOTDISCAMT += TTXNDTL.TOTDISCAMT;
                                tmpdtl.DISCAMT += TTXNDTL.DISCAMT;
                                tmpdtl.SCMDISCAMT += TTXNDTL.SCMDISCAMT;
                                tmpdtl.TXBLVAL += TTXNDTL.TXBLVAL;
                                tmpdtl.IGSTAMT += TTXNDTL.IGSTAMT;
                                tmpdtl.CGSTAMT += TTXNDTL.CGSTAMT;
                                tmpdtl.SGSTAMT += TTXNDTL.SGSTAMT;
                                tmpdtl.NETAMT += TTXNDTL.NETAMT;

                                TTXNDTL.SLNO = tmpdtl.SLNO;
                            }
                        }
                        else
                        {
                            TTXNDTL.SLNO = ++txnslno;
                            TTXNDTLlist.Add(TTXNDTL);
                        }

                        TBATCHDTL TBATCHDTL = new TBATCHDTL();
                        TBATCHDTL.TXNSLNO = TTXNDTL.SLNO;
                        TBATCHDTL.SLNO = ++batchslno;  //COUNTER.retShort();
                        //TBATCHDTL.GOCD = VE.T_TXN.GOCD;
                        //TBATCHDTL.BARNO = barno;
                        TBATCHDTL.ITCD = TTXNDTL.ITCD;
                        TBATCHDTL.MTRLJOBCD = TTXNDTL.MTRLJOBCD;
                        TBATCHDTL.PARTCD = TTXNDTL.PARTCD;
                        TBATCHDTL.HSNCODE = TTXNDTL.HSNCODE;
                        TBATCHDTL.STKDRCR = TTXNDTL.STKDRCR;
                        TBATCHDTL.NOS = TTXNDTL.NOS;
                        TBATCHDTL.QNTY = TTXNDTL.QNTY;
                        TBATCHDTL.BLQNTY = TTXNDTL.BLQNTY;
                        TBATCHDTL.FLAGMTR = TTXNDTL.FLAGMTR;
                        TBATCHDTL.ITREM = TTXNDTL.ITREM;
                        TBATCHDTL.UOM = "MTR";
                        TBATCHDTL.RATE = TTXNDTL.RATE;
                        TBATCHDTL.DISCRATE = TTXNDTL.DISCRATE;
                        TBATCHDTL.DISCTYPE = TTXNDTL.DISCTYPE;
                        TBATCHDTL.SCMDISCRATE = TTXNDTL.SCMDISCRATE;
                        TBATCHDTL.SCMDISCTYPE = TTXNDTL.SCMDISCTYPE;
                        TBATCHDTL.TDDISCRATE = TTXNDTL.TDDISCRATE;
                        TBATCHDTL.TDDISCTYPE = TTXNDTL.TDDISCTYPE;
                        //TBATCHDTL.DIA = TTXNDTL.DIA;
                        //TBATCHDTL.CUTLENGTH = TTXNDTL.CUTLENGTH;
                        //TBATCHDTL.LOCABIN = TTXNDTL.LOCABIN;
                        //TBATCHDTL.SHADE = TTXNDTL.SHADE;
                        //TBATCHDTL.MILLNM = TTXNDTL.MILLNM;
                        TBATCHDTL.BATCHNO = inrdr["BATCH"].ToString();
                        TBATCHDTL.BALEYR = TTXNDTL.BALENO.retStr() == "" ? "" : TTXNDTL.BALEYR;
                        TBATCHDTL.BALENO = TTXNDTL.BALENO;
                        //if (VE.MENU_PARA == "SBPCK")
                        //{
                        //    TBATCHDTL.ORDAUTONO = TTXNDTL.ORDAUTONO;
                        //    TBATCHDTL.ORDSLNO = TTXNDTL.ORDSLNO;
                        //}
                        TBATCHDTL.LISTPRICE = TTXNDTL.LISTPRICE;
                        TBATCHDTL.LISTDISCPER = TTXNDTL.LISTDISCPER;
                        //TBATCHDTL.CUTLENGTH = TTXNDTL.CUTLENGTH;
                        TBATCHDTL.STKTYPE = TTXNDTL.STKTYPE;

                        //if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "OP") && VE.M_SYSCNFG.MNTNPCSTYPE == "Y")
                        //{
                        //    TBATCHDTL.PCSTYPE = TTXNDTL.PCSTYPE;
                        //}
                        TBATCHDTLlist.Add(TBATCHDTL);
                    }// inner loop of TTXNDTL
                     //Amount tab start
                    if (oudr["FREIGHT"].retDbl() != 0)
                    {
                        TTXNAMT TTXNAMT = new TTXNAMT();
                        TTXNAMT.SLNO = 1;
                        TTXNAMT.GLCD = PURGLCD;
                        TTXNAMT.AMTCD = AMTCD_FREIGHT;
                        TTXNAMT.AMTDESC = "FREIGHT";
                        TTXNAMT.AMTRATE = oudr["FREIGHT"].retDbl();
                        TTXNAMT.HSNCODE = "";
                        TTXNAMT.AMT = TTXNAMT.AMTRATE; txable += TTXNAMT.AMT.retDbl();
                        if (igstper > 0)
                        {
                            TTXNAMT.IGSTPER = igstper;
                            TTXNAMT.IGSTAMT = (oudr["FREIGHT"].retDbl() * igstper / 100).toRound(2); gstamt += TTXNAMT.IGSTAMT.retDbl();
                        }
                        else
                        {
                            TTXNAMT.CGSTPER = cgstper;
                            TTXNAMT.CGSTAMT = (oudr["FREIGHT"].retDbl() * cgstper / 100).toRound(2); gstamt += TTXNAMT.CGSTAMT.retDbl();
                            TTXNAMT.SGSTPER = cgstper;
                            TTXNAMT.SGSTAMT = (oudr["FREIGHT"].retDbl() * cgstper / 100).toRound(2); gstamt += TTXNAMT.SGSTAMT.retDbl();
                        }
                        TTXNAMTlist.Add(TTXNAMT);
                    }
                    if (oudr["INSURANCE"].retDbl() != 0)
                    {
                        TTXNAMT TTXNAMT = new TTXNAMT();
                        TTXNAMT.SLNO = 2;
                        TTXNAMT.GLCD = PURGLCD;
                        TTXNAMT.AMTCD = AMTCD_INSURANCE;
                        TTXNAMT.AMTDESC = "INSURANCE";
                        TTXNAMT.AMTRATE = oudr["INSURANCE"].retDbl();
                        TTXNAMT.HSNCODE = "";
                        TTXNAMT.AMT = TTXNAMT.AMTRATE; txable += TTXNAMT.AMT.retDbl();
                        if (igstper > 0)
                        {
                            TTXNAMT.IGSTPER = igstper;
                            TTXNAMT.IGSTAMT = (oudr["INSURANCE"].retDbl() * igstper / 100).toRound(2); gstamt += TTXNAMT.IGSTAMT.retDbl();
                        }
                        else
                        {
                            TTXNAMT.CGSTPER = cgstper;
                            TTXNAMT.CGSTAMT = (oudr["INSURANCE"].retDbl() * cgstper / 100).toRound(2); gstamt += TTXNAMT.CGSTAMT.retDbl();
                            TTXNAMT.SGSTPER = cgstper;
                            TTXNAMT.SGSTAMT = (oudr["INSURANCE"].retDbl() * cgstper / 100).toRound(2); gstamt += TTXNAMT.SGSTAMT.retDbl();
                        }
                        TTXNAMTlist.Add(TTXNAMT);
                    }
                    //           //Amount tab end
                    TTXN.ROAMT = (TTXN.BLAMT.retDbl() - (txable + gstamt + tcsamt)).toRound(2);
                    dupgrid.ROAMT = TTXN.ROAMT.retStr();
                    TMPVE.T_TXN = TTXN;
                    TMPVE.T_TXNTRANS = TXNTRANS;
                    TMPVE.T_TXNOTH = TTXNOTH;
                    TMPVE.TTXNDTL = TTXNDTLlist;
                    TMPVE.TBATCHDTL = TBATCHDTLlist;
                    TMPVE.TTXNAMT = TTXNAMTlist;
                    TMPVE.T_VCH_GST = new T_VCH_GST();
                    string tslCont = (string)TSCntlr.SAVE(TMPVE, "PosPurchase");
                    tslCont = tslCont.retStr().Split('~')[0];
                    if (tslCont.Length > 0 && tslCont.Substring(0, 1) == "1") dupgrid.MESSAGE = "Success " + tslCont.Substring(1);
                    else dupgrid.MESSAGE = tslCont;
                    DUGridlist.Add(dupgrid);
                }//outer
                VE.STATUS = "Data Uploaded Successfully";

            }//try
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                VE.STATUS = ex.Message + ex.StackTrace;
            }
            VE.DUpGrid = DUGridlist;
            return VE;
        }
        public string PCSTYPE(string grade, string foc)
        {
            string pcstype = "";
            switch (grade)
            {
                case "G":
                    grade = "GOOD"; break;
                case "B":
                    grade = "BCD"; break;
                case "C":
                    grade = "CCD"; break;
                case "A":
                    grade = "ACD"; break;
                default:
                    grade = ""; break;
            }
            switch (foc)
            {
                case "1":
                    foc = "NORMAL"; break;
                case "2":
                    foc = "ODD"; break;
                case "3":
                    foc = "SHORT"; break;
                case "5":
                case "6":
                    foc = "CUTS"; break;
                default:
                    foc = ""; break;
            }
            if (grade == "G" && foc == "1")
            {
                pcstype = "FRESH";
            }
            else
            {
                pcstype = grade + " " + foc;
            }
            return pcstype;
        }
        private string getSLCD(string sapcode, string gstno)
        {
            sql = "select a.slcd from " + CommVar.CurSchema(UNQSNO) + ".m_subleg_com a," + CommVar.FinSchema(UNQSNO) + ".m_subleg b  where a.slcd=b.slcd and a.sapcode='" + sapcode + "'";
            if (gstno != "") sql += " and b.gstno='" + gstno + "'";
            var dt = masterHelp.SQLquery(sql);
            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0]["slcd"].ToString();
            }
            return "";
        }

        public DataUploadVM ReadAdityaBirlaPTfile(DataUploadVM VE)
        {
            List<DUpGrid> DUGridlist = new List<DUpGrid>();
            try
            {
                DataTable dbfdt = new DataTable();
                dbfdt.Columns.Add("EXCELROWNUM", typeof(int));
                dbfdt.Columns.Add("GRP_SAPCD", typeof(string));
                dbfdt.Columns.Add("ITGRPNM", typeof(string));
                dbfdt.Columns.Add("STYLE", typeof(string));
                dbfdt.Columns.Add("SIZENM", typeof(string));
                dbfdt.Columns.Add("BLNO", typeof(string));
                dbfdt.Columns.Add("BARNO", typeof(string));
                dbfdt.Columns.Add("HSN", typeof(string));
                dbfdt.Columns.Add("BLDT", typeof(string));
                dbfdt.Columns.Add("QNTY", typeof(double));
                dbfdt.Columns.Add("TAXPER", typeof(double));
                dbfdt.Columns.Add("MRP", typeof(double));
                dbfdt.Columns.Add("TXBL", typeof(double));
                dbfdt.Columns.Add("TAXAMT", typeof(double));
                dbfdt.Columns.Add("NETVALUE", typeof(double));

                HttpFileCollectionBase files = Request.Files;
                HttpPostedFileBase file = files[0];
                using (var package = new ExcelPackage(file.InputStream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    int rowNum = 2;
                    for (rowNum = 2; rowNum <= noOfRow; rowNum++)
                    {
                        DataRow dr = dbfdt.NewRow();
                        dr["EXCELROWNUM"] = rowNum;
                        string GRP_SAPCD = workSheet.Cells[rowNum, 3].Value.retStr();
                        string ITGRPNM = workSheet.Cells[rowNum, 4].Value.retStr();
                        string STYLE = workSheet.Cells[rowNum, 5].Value.retStr();
                        string SIZENM = workSheet.Cells[rowNum, 6].Value.retStr();
                        string BLNO = workSheet.Cells[rowNum, 7].Value.retStr();
                        string BARNO = workSheet.Cells[rowNum, 8].Value.retStr();
                        string HSN = workSheet.Cells[rowNum, 9].Value.retStr();
                        string BLDT = workSheet.Cells[rowNum, 10].Value.retDateStr();
                        double QNTY = workSheet.Cells[rowNum, 11].Value.retDbl();
                        double TAXPER = workSheet.Cells[rowNum, 121].Value.retDbl();
                        double MRP = workSheet.Cells[rowNum, 13].Value.retDbl();
                        double TXBL = workSheet.Cells[rowNum, 14].Value.retDbl();
                        double TAXAMT = workSheet.Cells[rowNum, 15].Value.retDbl();
                        double NETVALUE = workSheet.Cells[rowNum, 16].Value.retDbl();
                        dr["GRP_SAPCD"] = GRP_SAPCD;
                        dr["ITGRPNM"] = ITGRPNM;
                        dr["STYLE"] = STYLE;
                        dr["SIZENM"] = SIZENM;
                        dr["BLNO"] = BLNO;
                        dr["BARNO"] = BARNO;
                        dr["HSN"] = HSN;
                        dr["BLDT"] = BLDT;
                        dr["QNTY"] = QNTY;
                        dr["TAXPER"] = TAXPER;
                        dr["MRP"] = MRP;
                        dr["TXBL"] = TXBL;
                        dr["TAXAMT"] = TAXAMT;
                        dr["NETVALUE"] = NETVALUE;
                        dbfdt.Rows.Add(dr);

                    }
                }
                TransactionSaleEntry TMPVE = new TransactionSaleEntry();
                T_SALEController TSCntlr = new T_SALEController();
                T_TXN TTXN = new T_TXN();
                T_TXNTRANS TXNTRANS = new T_TXNTRANS();
                T_TXNOTH TTXNOTH = new T_TXNOTH();
                TMPVE.DefaultAction = "A";
                TMPVE.MENU_PARA = "PB";
                var outerDT = dbfdt.AsEnumerable()
                  .GroupBy(g => new { BLNO = g["BLNO"], BLDT = g["BLDT"] })
                  .Select(g =>
                    {
                        var row = dbfdt.NewRow();
                        row["BLNO"] = g.Key.BLNO;
                        row["BLDT"] = g.Key.BLDT;
                        //row["GRP_SAPCD"] = g.OrderBy(r => r["GRP_SAPCD"].ToString()).First();
                        //row["ITGRPNM"] = g.OrderBy(r => r["ITGRPNM"].ToString()).First();
                        //row["STYLE"] = g.OrderBy(r => r["STYLE"]).First();
                        //row["SIZENM"] = g.OrderBy(r => r["SIZENM"]).First();
                        //row["BARNO"] = g.OrderBy(r => r["BARNO"]).First();
                        //row["HSN"] = g.OrderBy(r => r["HSN"]).First();
                        row["QNTY"] = g.Sum(r => r.Field<double>("QNTY"));
                        //row["TAXPER"] = g.Average(r => r.Field<double>("TAXPER"));
                        //row["MRP"] = g.Sum(r => r.Field<double>("MRP"));
                        row["TXBL"] = g.Sum(r => r.Field<double>("TXBL"));
                        row["TAXAMT"] = g.Sum(r => r.Field<double>("TAXAMT"));
                        row["NETVALUE"] = g.Sum(r => r.Field<double>("NETVALUE"));
                        return row;
                    }).CopyToDataTable();


                TTXN.EMD_NO = 0;
                TTXN.DOCCD = DB.M_DOCTYPE.Where(d => d.DOCTYPE == "SPBL").FirstOrDefault()?.DOCCD;
                TTXN.CLCD = CommVar.ClientCode(UNQSNO);
                TTXN.SLCD = VE.SLCD;
                var stcode = VE.GSTNO.retStr().Substring(2); bool igstappl = false;
                if (stcode != "19") igstappl = true;
                // freai j, pf=i,ins k
                //string AMTCD_FREIGHT = DB.M_AMTTYPE.Where(m => m.CALCCODE == "J").FirstOrDefault()?.AMTCD;
                //string AMTCD_INSURANCE = DB.M_AMTTYPE.Where(m => m.CALCCODE == "K").FirstOrDefault()?.AMTCD;
                //string AMTCD_PackFordng = DB.M_AMTTYPE.Where(m => m.CALCCODE == "I").FirstOrDefault()?.AMTCD;
                //
                foreach (DataRow oudr in outerDT.Rows)
                {
                    short txnslno = 0;
                    List<TBATCHDTL> TBATCHDTLlist = new List<Models.TBATCHDTL>();
                    List<TTXNDTL> TTXNDTLlist = new List<Models.TTXNDTL>();
                    List<TTXNAMT> TTXNAMTlist = new List<Models.TTXNAMT>();
                    DUpGrid dupgrid = new DUpGrid();
                    TTXN.GOCD = "SHOP";
                    TTXN.DOCTAG = "PB";
                    TTXN.PREFNO = oudr["BLNO"].ToString();

                    if (VE.DUpGrid.Where(m => m.Checked == true & m.BLNO == TTXN.PREFNO).FirstOrDefault() == null)
                    {//Selected row will upload only
                        continue;
                    }
                    TTXN.TCSPER = 0;// 0.075;
                    dupgrid.BLNO = TTXN.PREFNO;
                    string Ddate = DateTime.ParseExact(oudr["BLDT"].retDateStr(), "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
                    TTXN.DOCDT = Convert.ToDateTime(Ddate);
                    dupgrid.BLDT = Ddate;
                    TTXN.PREFDT = TTXN.DOCDT;
                    dupgrid.BLNO = TTXN.PREFNO;
                    dupgrid.CUSTOMERNO = VE.GSTNO;


                    TTXNOTH.PRCCD = "CP";
                    double blINV_VALUE = oudr["NETVALUE"].retDbl();
                    //double bltaxable = oudr["TXBL"].retDbl();
                    //double calculatedTax = Math.Round(((bltaxable * gstper) / 100), 2);
                    //double calcultednet = (bltaxable + calculatedTax);//.toRound(2);
                    //var roffamt = (blINV_VALUE - calcultednet).toRound(2);
                    //double blTAX_AMT = oudr["TAX_AMT"].retDbl();
                    double tcsamt = 0;// (blINV_VALUE * TTXN.TCSPER.retDbl() / 100).toRound(2);
                    TTXN.BLAMT = blINV_VALUE + tcsamt;
                    //TTXN.TDSCODE = "X";
                    //TTXN.ROYN = "Y";
                    TMPVE.RoundOff = true;
                    //TTXN.TCSON = calcultednet;
                    TTXN.TCSAMT = tcsamt; dupgrid.TCSAMT = tcsamt.ToString();
                    sql = "";
                    sql = "select a.autono,b.docno,a.SLCD,a.blamt,a.tcsamt,a.ROAMT  from  " + CommVar.CurSchema(UNQSNO) + ".t_txn a, " + CommVar.CurSchema(UNQSNO) + ".t_cntrl_hdr b ";
                    sql += " where   a.autono=b.autono and a.PREFNO='" + TTXN.PREFNO + "' and a.slcd='" + TTXN.SLCD + "' ";
                    var dt = masterHelp.SQLquery(sql);
                    if (dt.Rows.Count > 0)
                    {
                        dupgrid.MESSAGE = "Allready Added at docno:" + dt.Rows[0]["docno"].ToString();
                        dupgrid.BLNO = TTXN.PREFNO;
                        dupgrid.TCSAMT = dt.Rows[0]["tcsamt"].ToString();
                        dupgrid.BLAMT = dt.Rows[0]["blamt"].ToString();
                        dupgrid.ROAMT = dt.Rows[0]["ROAMT"].ToString();
                        DUGridlist.Add(dupgrid);
                        continue;
                    }
                    else
                    {
                        dupgrid.TCSAMT = TTXN.TCSAMT.retStr();
                        dupgrid.BLAMT = TTXN.BLAMT.retStr();
                    }

                    TXNTRANS.LRNO = null;
                    TXNTRANS.LRDT = null;
                    //----------------------------------------------------------//

                    string PURGLCD = "";
                    DataTable innerDt = dbfdt.Select("BLNO='" + TTXN.PREFNO + "'").CopyToDataTable();
                    double txable = 0, gstamt = 0; short batchslno = 0;
                    foreach (DataRow inrdr in innerDt.Rows)
                    {
                        //detail tab start
                        TTXNDTL TTXNDTL = new TTXNDTL();
                        string style = inrdr["STYLE"].ToString();
                        string grpnm = inrdr["ITGRPNM"].ToString();
                        string HSNCODE = inrdr["HSN"].ToString();
                        ItemDet ItemDet = Salesfunc.CreateItem(style, TTXNDTL.UOM, grpnm, HSNCODE, "", "", "F", "C", "");
                        TTXNDTL.ITCD = ItemDet.ITCD;
                        PURGLCD = ItemDet.PURGLCD;
                        TTXNDTL.ITSTYLE = style;
                        TTXNDTL.MTRLJOBCD = "FS";
                        TTXNDTL.STKDRCR = "D";
                        TTXNDTL.STKTYPE = "F";
                        TTXNDTL.HSNCODE = HSNCODE;
                        //TTXNDTL.ITREM = VE.TTXNDTL[i].ITREM;
                        //TTXNDTL.BATCHNO = inrdr["BATCH"].ToString();
                        //TTXNDTL.BALENO = inrdr["BALENO"].ToString();
                        TTXNDTL.GOCD = "SHOP";
                        TTXNDTL.UOM = "MTR";
                        TTXNDTL.QNTY = inrdr["QNTY"].retDbl(); // NET_QTY
                        TTXNDTL.NOS = 1;
                        TTXNDTL.RATE = inrdr["TXBL"].retDbl().toRound(2);
                        TTXNDTL.AMT = inrdr["NETVALUE"].retDbl();
                        //TTXNDTL.FLAGMTR = null;
                        //string grade = inrdr["GRADATION"].ToString();
                        //string foc = inrdr["FOC"].ToString();
                        //string pCSTYPE = PCSTYPE(grade, foc);
                        //double W_FLG_Q = Math.Abs(inrdr["W_FLG_Q"].retDbl());
                        //double R_FLG_Q = Math.Abs(inrdr["R_FLG_Q"].retDbl());
                        //double discamt = Math.Abs(inrdr["QLTY_DISC"].retDbl());
                        //double discamt1 = Math.Abs(inrdr["MKTG_DISC"].retDbl());
                        //double Flagamt = (W_FLG_Q + R_FLG_Q) * TTXNDTL.RATE.retDbl();
                        //TTXNDTL.TOTDISCAMT = Flagamt;
                        //TTXNDTL.DISCTYPE = "F";
                        //TTXNDTL.DISCRATE = discamt;
                        //TTXNDTL.DISCAMT = discamt;
                        //TTXNDTL.SCMDISCTYPE = "F";
                        //TTXNDTL.SCMDISCRATE = discamt1;
                        //TTXNDTL.SCMDISCAMT = discamt1;
                        TTXNDTL.GLCD = PURGLCD;
                        TTXNDTL.TXBLVAL = TTXNDTL.RATE;
                        txable += TTXNDTL.TXBLVAL.retDbl();
                        double gstper = inrdr["TAXPER"].retDbl();
                        double igstper = 0;
                        double cgstper = 0;
                        double sgstper = 0;
                        double itmgstamt = inrdr["TAXAMT"].retDbl().toRound(2);
                        if (igstappl)
                        {
                            TTXNOTH.TAXGRPCD = "C001";
                            igstper = gstper;
                            TTXNDTL.IGSTPER = igstper;
                            TTXNDTL.IGSTAMT = itmgstamt.retDbl();
                        }
                        else {
                            TTXNOTH.TAXGRPCD = "I001";
                            cgstper = gstper / 2;
                            sgstper = gstper / 2;
                            TTXNDTL.CGSTPER = cgstper;
                            TTXNDTL.SGSTPER = sgstper;
                            TTXNDTL.CGSTAMT = (itmgstamt / 2).toRound(2);// inrdr["CENT_AMT"].retDbl();   // gstamt += TTXNDTL.CGSTAMT.retDbl();
                            TTXNDTL.SGSTAMT = TTXNDTL.CGSTAMT.retDbl(); // gstamt += TTXNDTL.SGSTAMT.retDbl();
                        }

                        TTXNDTL.GSTPER = TTXNDTL.IGSTPER.retDbl() + TTXNDTL.CGSTPER.retDbl() + TTXNDTL.SGSTPER.retDbl();

                        gstamt += itmgstamt.retDbl();
                        //double NET_AMT = ((TTXNDTL.TXBLVAL * (100 + gstper)) / 100).retDbl();
                        double NET_AMT = TTXNDTL.TXBLVAL.retDbl() + TTXNDTL.CGSTAMT.retDbl() + TTXNDTL.SGSTAMT.retDbl() + TTXNDTL.IGSTAMT.retDbl();
                        TTXNDTL.NETAMT = NET_AMT.toRound(2);
                        TTXNDTL tmpTTXNDTL = TTXNDTLlist.Where(r => r.BALENO == TTXNDTL.BALENO && r.HSNCODE == TTXNDTL.HSNCODE && r.ITCD == TTXNDTL.ITCD && r.STKTYPE == TTXNDTL.STKTYPE && r.RATE == TTXNDTL.RATE && r.FLAGMTR == TTXNDTL.FLAGMTR && r.DISCRATE == TTXNDTL.DISCRATE && r.SCMDISCRATE == TTXNDTL.SCMDISCRATE).FirstOrDefault();
                        if (tmpTTXNDTL != null)
                        {
                            foreach (var tmpdtl in TTXNDTLlist.Where(r => r.BALENO == TTXNDTL.BALENO && r.HSNCODE == TTXNDTL.HSNCODE && r.ITCD == TTXNDTL.ITCD && r.STKTYPE == TTXNDTL.STKTYPE && r.RATE == TTXNDTL.RATE && r.FLAGMTR == TTXNDTL.FLAGMTR && r.DISCRATE == TTXNDTL.DISCRATE && r.SCMDISCRATE == TTXNDTL.SCMDISCRATE))
                            {
                                tmpdtl.NOS += TTXNDTL.NOS;
                                tmpdtl.QNTY += TTXNDTL.QNTY;
                                tmpdtl.AMT += TTXNDTL.AMT;
                                tmpdtl.TOTDISCAMT += TTXNDTL.TOTDISCAMT;
                                tmpdtl.DISCAMT += TTXNDTL.DISCAMT;
                                tmpdtl.SCMDISCAMT += TTXNDTL.SCMDISCAMT;
                                tmpdtl.TXBLVAL += TTXNDTL.TXBLVAL;
                                tmpdtl.IGSTAMT += TTXNDTL.IGSTAMT;
                                tmpdtl.CGSTAMT += TTXNDTL.CGSTAMT;
                                tmpdtl.SGSTAMT += TTXNDTL.SGSTAMT;
                                tmpdtl.NETAMT += TTXNDTL.NETAMT;

                                TTXNDTL.SLNO = tmpdtl.SLNO;
                            }
                        }
                        else
                        {
                            TTXNDTL.SLNO = ++txnslno;
                            TTXNDTLlist.Add(TTXNDTL);
                        }

                        TBATCHDTL TBATCHDTL = new TBATCHDTL();
                        TBATCHDTL.TXNSLNO = TTXNDTL.SLNO;
                        TBATCHDTL.SLNO = ++batchslno;  //COUNTER.retShort();
                        //TBATCHDTL.GOCD = VE.T_TXN.GOCD;
                        //TBATCHDTL.BARNO = barno;
                        TBATCHDTL.ITCD = TTXNDTL.ITCD;
                        TBATCHDTL.MTRLJOBCD = TTXNDTL.MTRLJOBCD;
                        TBATCHDTL.PARTCD = TTXNDTL.PARTCD;
                        TBATCHDTL.HSNCODE = TTXNDTL.HSNCODE;
                        TBATCHDTL.STKDRCR = TTXNDTL.STKDRCR;
                        TBATCHDTL.NOS = TTXNDTL.NOS;
                        TBATCHDTL.QNTY = TTXNDTL.QNTY;
                        TBATCHDTL.BLQNTY = TTXNDTL.BLQNTY;
                        TBATCHDTL.FLAGMTR = TTXNDTL.FLAGMTR;
                        TBATCHDTL.ITREM = TTXNDTL.ITREM;
                        TBATCHDTL.UOM = "MTR";
                        TBATCHDTL.RATE = TTXNDTL.RATE;
                        TBATCHDTL.DISCRATE = TTXNDTL.DISCRATE;
                        TBATCHDTL.DISCTYPE = TTXNDTL.DISCTYPE;
                        TBATCHDTL.SCMDISCRATE = TTXNDTL.SCMDISCRATE;
                        TBATCHDTL.SCMDISCTYPE = TTXNDTL.SCMDISCTYPE;
                        TBATCHDTL.TDDISCRATE = TTXNDTL.TDDISCRATE;
                        TBATCHDTL.TDDISCTYPE = TTXNDTL.TDDISCTYPE;
                        //TBATCHDTL.DIA = TTXNDTL.DIA;
                        //TBATCHDTL.CUTLENGTH = TTXNDTL.CUTLENGTH;
                        //TBATCHDTL.LOCABIN = TTXNDTL.LOCABIN;
                        //TBATCHDTL.SHADE = TTXNDTL.SHADE;
                        //TBATCHDTL.MILLNM = TTXNDTL.MILLNM;
                        //TBATCHDTL.BATCHNO = inrdr["BATCH"].ToString();
                        TBATCHDTL.BALEYR = TTXNDTL.BALENO.retStr() == "" ? "" : TTXNDTL.BALEYR;
                        TBATCHDTL.BALENO = TTXNDTL.BALENO;
                        //if (VE.MENU_PARA == "SBPCK")
                        //{
                        //    TBATCHDTL.ORDAUTONO = TTXNDTL.ORDAUTONO;
                        //    TBATCHDTL.ORDSLNO = TTXNDTL.ORDSLNO;
                        //}
                        TBATCHDTL.LISTPRICE = TTXNDTL.LISTPRICE;
                        TBATCHDTL.LISTDISCPER = TTXNDTL.LISTDISCPER;
                        //TBATCHDTL.CUTLENGTH = TTXNDTL.CUTLENGTH;
                        TBATCHDTL.STKTYPE = TTXNDTL.STKTYPE;

                        //if ((VE.MENU_PARA == "PB" || VE.MENU_PARA == "PR" || VE.MENU_PARA == "OP") && VE.M_SYSCNFG.MNTNPCSTYPE == "Y")
                        //{
                        //    TBATCHDTL.PCSTYPE = TTXNDTL.PCSTYPE;
                        //}
                        TBATCHDTLlist.Add(TBATCHDTL);
                    }// inner loop of TTXNDTL
                     //Amount tab start
                     //if (oudr["FREIGHT"].retDbl() != 0)
                     //{
                     //    TTXNAMT TTXNAMT = new TTXNAMT();
                     //    TTXNAMT.SLNO = 1;
                     //    TTXNAMT.GLCD = PURGLCD;
                     //    TTXNAMT.AMTCD = AMTCD_FREIGHT;
                     //    TTXNAMT.AMTDESC = "FREIGHT";
                     //    TTXNAMT.AMTRATE = oudr["FREIGHT"].retDbl();
                     //    TTXNAMT.HSNCODE = "";
                     //    TTXNAMT.AMT = TTXNAMT.AMTRATE; txable += TTXNAMT.AMT.retDbl();
                     //    if (igstper > 0)
                     //    {
                     //        TTXNAMT.IGSTPER = igstper;
                     //        TTXNAMT.IGSTAMT = (oudr["FREIGHT"].retDbl() * igstper / 100).toRound(2); gstamt += TTXNAMT.IGSTAMT.retDbl();
                     //    }
                     //    else
                     //    {
                     //        TTXNAMT.CGSTPER = cgstper;
                     //        TTXNAMT.CGSTAMT = (oudr["FREIGHT"].retDbl() * cgstper / 100).toRound(2); gstamt += TTXNAMT.CGSTAMT.retDbl();
                     //        TTXNAMT.SGSTPER = cgstper;
                     //        TTXNAMT.SGSTAMT = (oudr["FREIGHT"].retDbl() * cgstper / 100).toRound(2); gstamt += TTXNAMT.SGSTAMT.retDbl();
                     //    }
                     //    TTXNAMTlist.Add(TTXNAMT);
                     //}
                     //if (oudr["INSURANCE"].retDbl() != 0)
                     //{
                     //    TTXNAMT TTXNAMT = new TTXNAMT();
                     //    TTXNAMT.SLNO = 2;
                     //    TTXNAMT.GLCD = PURGLCD;
                     //    TTXNAMT.AMTCD = AMTCD_INSURANCE;
                     //    TTXNAMT.AMTDESC = "INSURANCE";
                     //    TTXNAMT.AMTRATE = oudr["INSURANCE"].retDbl();
                     //    TTXNAMT.HSNCODE = "";
                     //    TTXNAMT.AMT = TTXNAMT.AMTRATE; txable += TTXNAMT.AMT.retDbl();
                     //    if (igstper > 0)
                     //    {
                     //        TTXNAMT.IGSTPER = igstper;
                     //        TTXNAMT.IGSTAMT = (oudr["INSURANCE"].retDbl() * igstper / 100).toRound(2); gstamt += TTXNAMT.IGSTAMT.retDbl();
                     //    }
                     //    else
                     //    {
                     //        TTXNAMT.CGSTPER = cgstper;
                     //        TTXNAMT.CGSTAMT = (oudr["INSURANCE"].retDbl() * cgstper / 100).toRound(2); gstamt += TTXNAMT.CGSTAMT.retDbl();
                     //        TTXNAMT.SGSTPER = cgstper;
                     //        TTXNAMT.SGSTAMT = (oudr["INSURANCE"].retDbl() * cgstper / 100).toRound(2); gstamt += TTXNAMT.SGSTAMT.retDbl();
                     //    }
                     //    TTXNAMTlist.Add(TTXNAMT);
                     //}
                     //           //Amount tab end
                    TTXN.ROAMT = (TTXN.BLAMT.retDbl() - (txable + gstamt + tcsamt)).toRound(2);
                    dupgrid.ROAMT = TTXN.ROAMT.retStr();
                    TMPVE.T_TXN = TTXN;
                    TMPVE.T_TXNTRANS = TXNTRANS;
                    TMPVE.T_TXNOTH = TTXNOTH;
                    TMPVE.TTXNDTL = TTXNDTLlist;
                    TMPVE.TBATCHDTL = TBATCHDTLlist;
                    TMPVE.TTXNAMT = TTXNAMTlist;
                    TMPVE.T_VCH_GST = new T_VCH_GST();
                    string tslCont = (string)TSCntlr.SAVE(TMPVE, "PosPurchase");
                    tslCont = tslCont.retStr().Split('~')[0];
                    if (tslCont.Length > 0 && tslCont.Substring(0, 1) == "1") dupgrid.MESSAGE = "Success " + tslCont.Substring(1);
                    else dupgrid.MESSAGE = tslCont;
                    DUGridlist.Add(dupgrid);
                }//outer
                VE.STATUS = "Data Uploaded Successfully";

            }//try
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                VE.STATUS = ex.Message + ex.StackTrace;
            }
            VE.DUpGrid = DUGridlist;
            return VE;
        }






    }
}
