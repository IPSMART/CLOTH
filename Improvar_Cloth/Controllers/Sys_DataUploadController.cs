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
using System.IO;
using OfficeOpenXml;
//using NDbfReader;
namespace Improvar.Controllers
{
    public class Sys_DataUploadController : Controller
    {
        string CS = null; string sql = ""; string dbsql = ""; string[] dbsql1;
        string dberrmsg = "";
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        Salesfunc Salesfunc = new Salesfunc(); DataTable dt = new DataTable();
        private ImprovarDB DB, DBF;
        public Sys_DataUploadController()
        {
            DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
        }
        // GET: Sys_DataUpload
        public ActionResult Sys_DataUpload()
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                ViewBag.formname = "Data Upload";
                ReportViewinHtml VE = new ReportViewinHtml();
                List<DropDown_list1> drplist1 = new List<DropDown_list1>();
                drplist1.Add(new DropDown_list1() { value = "OP", text = "Opening Stock" });
                VE.DropDown_list1 = drplist1;
                return View(VE);
            }
        }
        [HttpPost]
        public ActionResult Sys_DataUpload(ReportViewinHtml VE, FormCollection FC, String Command)
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Request.Files.Count == 0) return Content("No File Selected");
            HttpPostedFileBase file = Request.Files["UploadedFile"];
            if (System.IO.Path.GetExtension(file.FileName) != ".xlsx") return Content(".xlsx file need to choose");
            string resp = ReadOpstockExcel(VE, file.InputStream);
            return Content(resp);
        }
        public string ReadOpstockExcel(ReportViewinHtml VE, Stream stream)
        {
            string msg = "";
            try
            {
                // Excel Columns: DOCDT	PBLNO	SLNM	SLCD	ITGRPNM	FABITGRPNM	FABITNM	ITNM	PRODGRPCD	STYLENO	UOMCD	BARNO	NOS	QNTY	RATE	AMT	GOCD	LRNO	LRDT	BALENO	COMMONUNIQBAR	PAGENO	PAGESLNO	WPRATE	RPRATE	MAKERATE	SHADE	COLRCD	SIZECD	PDESIGN	PCSCTG	HSNCODE	GSTPER	GSTABOVEPER
                DataTable dbfdt = new DataTable();
                dbfdt.Columns.Add("EXCELROWNUM", typeof(int));
                dbfdt.Columns.Add("DOCDT", typeof(string));
                dbfdt.Columns.Add("PBLNO", typeof(string));
                dbfdt.Columns.Add("SLNM", typeof(string));
                dbfdt.Columns.Add("SLCD", typeof(string));
                dbfdt.Columns.Add("ITGRPNM", typeof(string));
                dbfdt.Columns.Add("FABITGRPNM", typeof(string));
                dbfdt.Columns.Add("FABITNM", typeof(string));
                dbfdt.Columns.Add("ITNM", typeof(string));
                dbfdt.Columns.Add("PRODGRPCD", typeof(string));
                dbfdt.Columns.Add("STYLENO", typeof(string));
                dbfdt.Columns.Add("UOMCD", typeof(string));
                dbfdt.Columns.Add("BARNO", typeof(string));
                dbfdt.Columns.Add("NOS", typeof(string));
                dbfdt.Columns.Add("QNTY", typeof(string));
                dbfdt.Columns.Add("RATE", typeof(string));
                dbfdt.Columns.Add("AMT", typeof(string));
                dbfdt.Columns.Add("GOCD", typeof(string));
                dbfdt.Columns.Add("LRNO", typeof(string));
                dbfdt.Columns.Add("LRDT", typeof(string));
                dbfdt.Columns.Add("BALENO", typeof(string));
                dbfdt.Columns.Add("COMMONUNIQBAR", typeof(string));
                dbfdt.Columns.Add("PAGENO", typeof(string));
                dbfdt.Columns.Add("PAGESLNO", typeof(string));
                dbfdt.Columns.Add("WPRATE", typeof(string));
                dbfdt.Columns.Add("RPRATE", typeof(string));
                dbfdt.Columns.Add("MAKERATE", typeof(string));
                dbfdt.Columns.Add("SHADE", typeof(string));
                dbfdt.Columns.Add("COLRCD", typeof(string));
                dbfdt.Columns.Add("SIZECD", typeof(string));
                dbfdt.Columns.Add("PDESIGN", typeof(string));
                dbfdt.Columns.Add("PCSCTG", typeof(string));
                dbfdt.Columns.Add("HSNCODE", typeof(string));
                dbfdt.Columns.Add("GSTPER", typeof(string));
                dbfdt.Columns.Add("GSTABOVEPER", typeof(string));

                using (var package = new ExcelPackage(stream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    int rowNum = 2;
                    for (rowNum = 2; rowNum <= noOfRow; rowNum++)
                    {
                        if (workSheet.Cells[rowNum, 1].Value.retStr() != "")
                        {
                            DataRow dr = dbfdt.NewRow();
                            dr["EXCELROWNUM"] = rowNum;
                            var wsRow = workSheet.Cells[rowNum, 1, rowNum, noOfCol];
                            for (int colnum = 1; colnum <= noOfCol; colnum++)
                            {
                                string colname = workSheet.Cells[1, colnum].Value.retStr();
                                string colValue = workSheet.Cells[rowNum, colnum].Value.retStr();
                                try
                                {
                                    if (colname == "DOCDT" || colname == "LRDT") { colValue = colValue.retDateStr(); }
                                    dr[colname] = colValue;
                                }
                                catch (ArgumentException ex)
                                {
                                    return "Wrong ColumnName:" + colname + " Error:" + ex.Message;
                                }
                            }
                            dbfdt.Rows.Add(dr);
                        }
                    }
                }
                TransactionSaleEntry TMPVE = new TransactionSaleEntry();
                T_SALEController TSCntlr = new T_SALEController();
                T_TXN TTXN = new T_TXN();
                T_TXNTRANS TXNTRANS = new T_TXNTRANS();
                T_TXNOTH TTXNOTH = new T_TXNOTH();
                TMPVE.DefaultAction = "A";
                TMPVE.MENU_PARA = "OP";
                var outerDT = dbfdt.AsEnumerable()
               .GroupBy(g => new { DOCDT = g["DOCDT"], PBLNO = g["PBLNO"], SLNM = g["SLNM"], SLCD = g["SLCD"], LRNO = g["LRNO"], LRDT = g["LRDT"], GOCD = g["GOCD"] })
               .Select(g =>
               {
                   var row = dbfdt.NewRow();
                   row["DOCDT"] = g.Key.DOCDT;
                   row["PBLNO"] = g.Key.PBLNO;
                   row["SLNM"] = g.Key.SLNM;
                   row["SLCD"] = g.Key.SLCD;
                   row["LRNO"] = g.Key.LRNO;
                   row["LRDT"] = g.Key.LRDT;
                   row["GOCD"] = g.Key.GOCD;
                   return row;
               }).CopyToDataTable();

                TTXN.EMD_NO = 0;
                TTXN.DOCCD = DB.M_DOCTYPE.Where(d => d.DOCTYPE == "FOSTK").FirstOrDefault()?.DOCCD;
                if(string.IsNullOrEmpty(TTXN.DOCCD)) return "Please add Document code. ";
                TTXN.CLCD = CommVar.ClientCode(UNQSNO);
                sql = "select * from " + CommVar.CurSchema(UNQSNO) + ".m_group ";
                dt = masterHelp.SQLquery(sql);
                if (dt.Rows.Count == 0)
                {
                    return "Please create a group Master";
                }
                int excelrow = 1;
                foreach (DataRow oudr in outerDT.Rows)
                {
                    ++excelrow; msg = " Excelrow:" + excelrow;
                    short txnslno = 0;
                    List<TBATCHDTL> TBATCHDTLlist = new List<Models.TBATCHDTL>();
                    List<TTXNDTL> TTXNDTLlist = new List<Models.TTXNDTL>();
                    List<TTXNAMT> TTXNAMTlist = new List<Models.TTXNAMT>();
                    TTXN.GOCD = oudr["GOCD"].ToString();
                    TTXN.DOCTAG = "OP";
                    TTXN.PREFNO = oudr["PBLNO"].ToString();
                    TTXN.TCSPER = 0.075;
                    string Ddate = DateTime.ParseExact(oudr["DOCDT"].retDateStr(), "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
                    TTXN.DOCDT = Convert.ToDateTime(Ddate);
                    TTXN.PREFDT = TTXN.DOCDT;
                    TTXN.SLCD = oudr["SLCD"].ToString();
                    double igstper = 0;
                    double cgstper = 0;
                    double sgstper = 0;
                    double gstper = 0;
                    TTXNOTH.TAXGRPCD = getTAXGRPCD(TTXN.SLCD);
                    if (TTXNOTH.TAXGRPCD == "")
                    {
                        return "Please add  SLCD:(" + TTXN.SLCD + ") from [Tax code link up With Party]." + msg;
                    }
                    TTXNOTH.PRCCD = "CP";
                    if (oudr["LRDT"].ToString() != "")
                    {
                        string LR_DATE = DateTime.ParseExact(oudr["LRDT"].retDateStr(), "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
                        TXNTRANS.LRNO = oudr["LRNO"].ToString();
                        TXNTRANS.LRDT = Convert.ToDateTime(LR_DATE);
                    }
                    var tepb = TTXN.PREFNO == "" ? " " : TTXN.PREFNO;
                    sql = "";
                    sql = "select a.autono,b.docno,a.SLCD,a.blamt,a.ROAMT  from  " + CommVar.CurSchema(UNQSNO) + ".t_txn a, " + CommVar.CurSchema(UNQSNO) + ".t_cntrl_hdr b ";
                    sql += " where   a.autono=b.autono and b.docdt=to_date('" + Ddate + "','dd/mm/yyyy') and a.slcd='" + TTXN.SLCD + "' and nvl(a.PREFNO,' ')='" + tepb + "' ";//  and a.LRNO='" + TTXN.LRNO + "' ";
                    var dt = masterHelp.SQLquery(sql);
                    if (dt.Rows.Count > 0) continue;
                    string PURGLCD = "";
                    DataTable innerDt = dbfdt.Select("docdt = '" + Ddate + "' and slcd = '" + TTXN.SLCD + "' and PBLNO = '" + TTXN.PREFNO + "'").CopyToDataTable();

                    double txable = 0, gstamt = 0; short batchslno = 0;
                    foreach (DataRow inrdr in innerDt.Rows)
                    {
                        excelrow = inrdr["EXCELROWNUM"].retInt();
                        msg = " Excelrow:" + excelrow;
                        //detail tab start
                        TTXNDTL TTXNDTL = new TTXNDTL();
                        string style = inrdr["STYLENO"].ToString();
                        string grpnm = inrdr["ITGRPNM"].ToString();
                        string HSNCODE = inrdr["HSNCODE"].ToString();
                        string BARGENTYPE = inrdr["COMMONUNIQBAR"].ToString();
                        TTXNDTL.UOM = inrdr["UOMCD"].ToString();
                        TTXNDTL.BARNO = inrdr["BARNO"].ToString();
                        string FABITNM = inrdr["FABITNM"].ToString(); string fabitcd = "";
                        if (FABITNM != "")
                        {
                            ItemDet FABITDet = Salesfunc.CreateItem(FABITNM, TTXNDTL.UOM, grpnm, HSNCODE, "","", "C", BARGENTYPE);
                            fabitcd = FABITDet.ITCD;
                        }
                        ItemDet ItemDet = Salesfunc.CreateItem(style, TTXNDTL.UOM, grpnm, HSNCODE, fabitcd, TTXNDTL.BARNO, "F", BARGENTYPE);
                        TTXNDTL.ITCD = ItemDet.ITCD; PURGLCD = ItemDet.PURGLCD;
                   
                        TTXNDTL.ITNM = style;
                        TTXNDTL.MTRLJOBCD = "FS";
                        TTXNDTL.STKDRCR = "D";
                        TTXNDTL.STKTYPE = "F";
                        TTXNDTL.HSNCODE = HSNCODE;
                        TTXNDTL.BALENO = inrdr["BALENO"].ToString();
                        TTXNDTL.PAGENO = inrdr["PAGENO"].retInt();
                        TTXNDTL.PAGESLNO = inrdr["PAGESLNO"].retInt();
                        TTXNDTL.GOCD = TTXN.GOCD == "" ? "TR" : TTXN.GOCD;
                        TTXNDTL.NOS = inrdr["NOS"].retDbl();
                        TTXNDTL.QNTY = inrdr["QNTY"].retDbl();
                        TTXNDTL.RATE = inrdr["RATE"].retDbl();
                        TTXNDTL.WPRATE = inrdr["WPRATE"].retDbl();
                        TTXNDTL.RPRATE = inrdr["RPRATE"].retDbl();
                        TTXNDTL.AMT = inrdr["amt"].retDbl();
                        if (TTXNDTL.AMT == 0) TTXNDTL.AMT = TTXNDTL.QNTY * TTXNDTL.RATE;
                        gstper = inrdr["GSTPER"].retDbl();
                        if (TTXNOTH.TAXGRPCD == "I001")
                        {
                            igstper = gstper;
                        }
                        else
                        {
                            cgstper = gstper / 2;
                            sgstper = cgstper;
                        }
                        TTXNDTL.GLCD = PURGLCD;
                        TTXNDTL.TXBLVAL = TTXNDTL.AMT.retDbl(); txable += TTXNDTL.TXBLVAL.retDbl();
                        if (TTXNDTL.TXBLVAL == 0)
                        {
                            return "TXBLVAL/RATE/QNTY should not Zeor(0) at " + msg;
                        }
                        TTXNDTL.IGSTPER = igstper;
                        TTXNDTL.CGSTPER = cgstper;
                        TTXNDTL.SGSTPER = sgstper;
                        TTXNDTL.GSTPER = gstper;
                        if (igstper != 0 && TTXNDTL.AMT.retDbl() != 0)
                        {
                            TTXNDTL.IGSTAMT = TTXNDTL.AMT.retDbl() * igstper / 100; gstamt += TTXNDTL.IGSTAMT.retDbl();
                        }
                        else if (cgstper != 0 && TTXNDTL.AMT.retDbl() != 0)
                        {
                            TTXNDTL.CGSTAMT = TTXNDTL.AMT.retDbl() * cgstper / 100; ; gstamt += TTXNDTL.CGSTAMT.retDbl();
                            TTXNDTL.SGSTAMT = TTXNDTL.AMT.retDbl() * sgstper / 100; ; gstamt += TTXNDTL.SGSTAMT.retDbl();
                        }
                        TTXNDTL.NETAMT = (TTXNDTL.AMT.retDbl() + TTXNDTL.IGSTAMT.retDbl() + TTXNDTL.CGSTAMT.retDbl() + TTXNDTL.SGSTAMT.retDbl()).toRound(2);
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
                        TBATCHDTL.SLNO = ++batchslno;
                        TBATCHDTL.BARNO = TTXNDTL.BARNO;
                        TBATCHDTL.ITCD = TTXNDTL.ITCD;
                        TBATCHDTL.MTRLJOBCD = TTXNDTL.MTRLJOBCD;
                        TBATCHDTL.BARGENTYPE = BARGENTYPE;
                        TBATCHDTL.PARTCD = TTXNDTL.PARTCD;
                        TBATCHDTL.HSNCODE = TTXNDTL.HSNCODE;
                        TBATCHDTL.STKDRCR = TTXNDTL.STKDRCR;
                        TBATCHDTL.NOS = TTXNDTL.NOS;
                        TBATCHDTL.QNTY = TTXNDTL.QNTY;
                        TBATCHDTL.BLQNTY = TTXNDTL.BLQNTY;
                        TBATCHDTL.FLAGMTR = TTXNDTL.FLAGMTR;
                        TBATCHDTL.ITREM = TTXNDTL.ITREM;
                        TBATCHDTL.UOM = TTXNDTL.UOM;
                        TBATCHDTL.RATE = TTXNDTL.RATE;
                        TBATCHDTL.WPRATE = TTXNDTL.WPRATE;
                        TBATCHDTL.RPRATE = TTXNDTL.RPRATE;
                        TBATCHDTL.DISCRATE = TTXNDTL.DISCRATE;
                        TBATCHDTL.DISCTYPE = TTXNDTL.DISCTYPE;
                        TBATCHDTL.SCMDISCRATE = TTXNDTL.SCMDISCRATE;
                        TBATCHDTL.SCMDISCTYPE = TTXNDTL.SCMDISCTYPE;
                        TBATCHDTL.TDDISCRATE = TTXNDTL.TDDISCRATE;
                        TBATCHDTL.TDDISCTYPE = TTXNDTL.TDDISCTYPE;
                        TBATCHDTL.SHADE = inrdr["SHADE"].retStr();
                        TBATCHDTL.BALEYR = TTXNDTL.BALENO.retStr() == "" ? "" : TTXNDTL.BALEYR;
                        TBATCHDTL.BALENO = TTXNDTL.BALENO;
                        TBATCHDTL.LISTPRICE = TTXNDTL.LISTPRICE;
                        TBATCHDTL.LISTDISCPER = TTXNDTL.LISTDISCPER;
                        TBATCHDTL.STKTYPE = TTXNDTL.STKTYPE;
                        TBATCHDTL.PCSTYPE = inrdr["PCSCTG"].retStr();
                        TBATCHDTLlist.Add(TBATCHDTL);
                    }// inner loop of TTXNDTL

                    TTXN.BLAMT = (txable + gstamt).toRound(2);
                    //TTXN.TDSCODE = "X";
                    //TTXN.ROYN = "Y";               
                    TTXN.ROAMT = (0.0).toRound(2);
                    TMPVE.T_TXN = TTXN;
                    TMPVE.T_TXNTRANS = TXNTRANS;
                    TMPVE.T_TXNOTH = TTXNOTH;
                    TMPVE.TTXNDTL = TTXNDTLlist;
                    TMPVE.TBATCHDTL = TBATCHDTLlist;
                    TMPVE.TTXNAMT = TTXNAMTlist;
                    TMPVE.T_VCH_GST = new T_VCH_GST();
                    string tslCont = (string)TSCntlr.SAVE(TMPVE, "OpStock");
                    tslCont = tslCont.retStr().Split('~')[0];
                    if (tslCont.Length > 0 && tslCont.Substring(0, 1) == "1")
                    {

                    }// msg += "Success " + tslCont.Substring(1);
                    else return tslCont + " at " + msg;
                }//outer

            }//try
            catch (Exception ex)
            {
                Cn.SaveException(ex, msg);
                return ex.Message;
            }
            return "Excel Uploaded Successfully !!";
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
        private string getTAXGRPCD(string slcd)
        {
            sql = "select taxgrpcd from " + CommVar.CurSchema(UNQSNO) + ".m_subleg_sddtl where slcd='" + slcd + "'";
            var dt = masterHelp.SQLquery(sql);
            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0]["taxgrpcd"].ToString();
            }
            return "";
        }
    }
}
