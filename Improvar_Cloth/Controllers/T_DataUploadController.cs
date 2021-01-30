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
//using NDbfReader;
namespace Improvar.Controllers
{
    public class T_DataUploadController : Controller
    {
        string CS = null; string sql = ""; string dbsql = ""; string[] dbsql1;
        string dberrmsg = "";
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        Salesfunc Salesfunc = new Salesfunc();
        private ImprovarDB DB, DBF;
        public T_DataUploadController()
        {
            DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
        }
        // GET: T_DataUpload
        public ActionResult T_DataUpload()
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                ViewBag.formname = "Data Upload";
                DataUploadVM VE = new DataUploadVM();
                return View(VE);
            }
        }
        [HttpPost]
        public ActionResult T_DataUpload(DataUploadVM VE, FormCollection FC, String Command)
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Request.Files.Count == 0) return Content("No File Selected");
            VE = ReadRaymondPurchaseDBF(VE);
            return View(VE);
        }
        public DataUploadVM ReadRaymondPurchaseDBF(DataUploadVM VE)
        {
            List<DUpGrid> DUGridlist = new List<DUpGrid>();
            try
            {
                string Path = "C:\\IPSMART\\Temp";
                if (!System.IO.Directory.Exists(Path)) { System.IO.Directory.CreateDirectory(Path); }
                Path = "C:\\IPSMART\\Temp\\Raymond.dbf";
                if (System.IO.File.Exists(Path)) { System.IO.File.Delete(Path); }
                GC.Collect();
                Request.Files["FileUpload"].SaveAs(Path);

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
                T_TXN_LINKNO TTXNLINKNO = new T_TXN_LINKNO();
                var tys = dbfdt.Rows[0]["FREIGHT"].GetType();
                var tsy = dbfdt.Rows[0]["INSURANCE"].GetType();
                var tssy = dbfdt.Rows[0]["INTEGR_TAX"].GetType();
                var ty = dbfdt.Rows[0]["CUSTOMERNO"].GetType();
                var tysxs = dbfdt.Rows[0]["STATE_AMT"].GetType();
                var tsay = dbfdt.Rows[0]["STATE_TAX"].GetType();
                var tssay = dbfdt.Rows[0]["STATE_AMT"].GetType();
                var outerDT = dbfdt.AsEnumerable()
               .GroupBy(g => new { CUSTOMERNO = g["CUSTOMERNO"], INV_NO = g["INV_NO"], INVDATE = g["INVDATE"], LR_NO = g["LR_NO"], LR_DATE = g["LR_DATE"], CARR_NO = g["CARR_NO"] })
               .Select(g =>
               {
                   var row = dbfdt.NewRow();
                   row["CUSTOMERNO"] = g.Key.CUSTOMERNO;
                   row["INV_NO"] = g.Key.INV_NO;
                   row["INVDATE"] = g.Key.INVDATE;
                   row["LR_NO"] = g.Key.LR_NO;
                   row["LR_DATE"] = g.Key.LR_DATE;
                   row["CARR_NO"] = g.Key.CARR_NO;
                   row["FREIGHT"] = g.Sum(r => r.Field<double>("FREIGHT"));
                   row["INSURANCE"] = g.Sum(r => r.Field<double>("INSURANCE"));
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
                TTXN.DOCCD = DB.M_DOCTYPE.Where(d => d.DOCTYPE == "PB").FirstOrDefault()?.DOCCD;
                TTXN.CLCD = CommVar.ClientCode(UNQSNO);
                List<TTXNDTL> TTXNDTLlist = new List<Models.TTXNDTL>();
                List<TTXNAMT> TTXNAMTlist = new List<Models.TTXNAMT>();
                short slno = 0;
                foreach (DataRow oudr in outerDT.Rows)
                {
                    DUpGrid dupgrid = new DUpGrid();
                    TTXN.GOCD = "TR";
                    TTXN.DOCTAG = "PB";
                    TTXN.PREFNO = oudr["INV_NO"].ToString();
                    dupgrid.BLNO = TTXN.PREFNO;
                    string Ddate = DateTime.ParseExact(oudr["INVDATE"].retDateStr(), "dd/MM/yyyy", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
                    TTXN.DOCDT = Convert.ToDateTime(Ddate);
                    dupgrid.BLDT = Ddate;
                    TTXN.PREFDT = TTXN.DOCDT;
                    dupgrid.BLNO = TTXN.PREFNO;
                    string CUSTOMERNO = oudr["CUSTOMERNO"].ToString();
                    TTXN.SLCD = getSLCD(CUSTOMERNO); dupgrid.SLCD = CUSTOMERNO;
                    if (TTXN.SLCD == "")
                    {
                        dupgrid.MESSAGE = "Please add Customer No:(" + CUSTOMERNO + ") in the SAPCODE from [Tax code link up With Party].";
                        DUGridlist.Add(dupgrid);
                        break;
                    }
                    TTXN.BLAMT = oudr["NET_AMT"].retDbl();


                    TMPVE.T_TXN = TTXN;//adding to Viewmodel

                    TTXNAMT TTXNAMT = new TTXNAMT();
                    double igstper = oudr["INTEGR_TAX"].retDbl();
                    double cgstper = oudr["CENT_AMT"].retDbl();
                    if (oudr["FREIGHT"].retDbl() != 0)
                    {
                        TTXNAMT.SLNO = 1;
                        TTXNAMT.AMTCD = "";
                        TTXNAMT.AMTDESC = "";
                        TTXNAMT.AMTRATE = oudr["FREIGHT"].retDbl();
                        TTXNAMT.HSNCODE = "";
                        TTXNAMT.AMT = TTXNAMT.AMTRATE;
                        if (igstper > 0)
                        {
                            TTXNAMT.IGSTAMT = oudr["FREIGHT"].retDbl() * igstper / 100;
                        }
                        else
                        {
                            TTXNAMT.CGSTPER = cgstper;
                            TTXNAMT.CGSTAMT = oudr["FREIGHT"].retDbl() * cgstper / 100;
                            TTXNAMT.SGSTPER = cgstper;
                            TTXNAMT.SGSTAMT = oudr["FREIGHT"].retDbl() * cgstper / 100;
                        }
                        TTXNAMTlist.Add(TTXNAMT);
                    }
                    if (oudr["INSURANCE"].retDbl() != 0)
                    {
                        TTXNAMT.SLNO = 2;
                        TTXNAMT.AMTCD = "0002";
                        TTXNAMT.AMTDESC = "";
                        TTXNAMT.AMTRATE = oudr["INSURANCE"].retDbl();
                        TTXNAMT.HSNCODE = "";
                        TTXNAMT.AMT = TTXNAMT.AMTRATE;
                        if (igstper > 0)
                        {
                            TTXNAMT.IGSTAMT = oudr["INSURANCE"].retDbl() * igstper / 100;
                        }
                        else
                        {
                            TTXNAMT.CGSTPER = cgstper;
                            TTXNAMT.CGSTAMT = oudr["INSURANCE"].retDbl() * cgstper / 100;
                            TTXNAMT.SGSTPER = cgstper;
                            TTXNAMT.SGSTAMT = oudr["INSURANCE"].retDbl() * cgstper / 100;
                        }
                        TTXNAMTlist.Add(TTXNAMT);
                    }
                    TMPVE.TTXNAMT = TTXNAMTlist;//adding to Viewmodel

                    //-------------------------Transport--------------------------//

                    if (oudr["CARR_NO"].ToString() != "")
                    {
                        TXNTRANS.TRANSLCD = getSLCD(oudr["CARR_NO"].ToString());
                        if (TXNTRANS.TRANSLCD == "")
                        {
                            dupgrid.MESSAGE = "Please add  CARR_NO:(" + oudr["CARR_NO"].ToString() + ")/Transporter in the SAPCODE from [Tax code link up With Party].";
                            DUGridlist.Add(dupgrid); break;
                        }
                    }

                    string LR_DATE = DateTime.ParseExact(oudr["LR_DATE"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture).ToString("dd/MM/yyyy");
                    TXNTRANS.LRNO = oudr["LR_NO"].ToString();
                    TXNTRANS.LRDT = Convert.ToDateTime(LR_DATE);
                    //----------------------------------------------------------//


                    foreach (DataRow dr in dbfdt.Rows)
                    {

                        TTXNDTL TTXNDTL = new TTXNDTL();
                        TTXNDTL.SLNO = ++slno;
                        TTXNDTL.MTRLJOBCD = "FS";
                        string style = dr["MAT_GRP"].ToString() + dr["MAT_GRP"].ToString().Split('-')[0];
                        string grpnm = dr["MAT_DESCRI"].ToString();
                        string HSNCODE = dr["HSN_CODE"].ToString();
                        ItemDet ItemDet = CreateItem(style, "MTR", grpnm, HSNCODE);
                        TTXNDTL.ITCD = ItemDet.ITCD;
                        TTXNDTL.STKDRCR = "D";
                        TTXNDTL.STKTYPE = "F";
                        TTXNDTL.HSNCODE = HSNCODE;
                        //TTXNDTL.ITREM = VE.TTXNDTL[i].ITREM;
                        TTXNDTL.BATCHNO = dr["BATCH"].ToString();
                        TTXNDTL.BALENO = dr["BALENO"].ToString();
                        TTXNDTL.GOCD = "TR";
                        TTXNDTL.QNTY = dr["NET_QTY"].retDbl();
                        TTXNDTL.NOS = 1;
                        TTXNDTL.RATE = dr["RATE"].retDbl();
                        TTXNDTL.AMT = dr["GROSS_AMT"].retDbl();
                        TTXNDTL.FLAGMTR = dr["W_FLG_Q"].retDbl();
                        string grade = dr["GRADATION"].ToString();
                        string foc = dr["FOC"].ToString();
                        string pCSTYPE = PCSTYPE(grade, foc);
                        double W_FLG_Q = Math.Abs(dr["W_FLG_Q"].retDbl());
                        double R_FLG_Q = Math.Abs(dr["R_FLG_Q"].retDbl());
                        double discamt = Math.Abs(dr["QLTY_DISC"].retDbl());
                        double discamt1 = Math.Abs(dr["MKTG_DISC"].retDbl());
                        double Flagamt = (W_FLG_Q + R_FLG_Q) * TTXNDTL.RATE.retDbl();
                        TTXNDTL.TOTDISCAMT = Flagamt;
                        TTXNDTL.DISCTYPE = "F";
                        TTXNDTL.DISCRATE = discamt;
                        TTXNDTL.DISCAMT = discamt;
                        TTXNDTL.SCMDISCTYPE = "F";
                        TTXNDTL.SCMDISCRATE = discamt1;
                        TTXNDTL.SCMDISCAMT = discamt1;
                        TTXNDTL.GLCD = ItemDet.PURGLCD;
                        double NET_AMT = dr["NET_AMT"].retDbl();
                        TTXNDTL.TXBLVAL = dr["TAX_AMT"].retDbl();
                        TTXNDTL.IGSTPER = dr["INTEGR_TAX"].retDbl();
                        TTXNDTL.CGSTPER = dr["CENT_TAX"].retDbl();
                        TTXNDTL.SGSTPER = dr["STATE_TAX"].retDbl();
                        TTXNDTL.IGSTAMT = dr["INTEGR_AMT"].retDbl();
                        TTXNDTL.CGSTAMT = dr["CENT_AMT"].retDbl();
                        TTXNDTL.SGSTAMT = dr["STATE_AMT"].retDbl();
                        TTXNDTL.NETAMT = NET_AMT;
                        TTXNDTLlist.Add(TTXNDTL);
                    }
                    var tssaddy = TSCntlr.SAVE(TMPVE);
                    dupgrid.MESSAGE = "Success";
                    DUGridlist.Add(dupgrid);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                VE.STATUS = ex.Message + ex.StackTrace;
            }
            VE.DUpGrid = DUGridlist;
            return VE;
        }

        public ItemDet CreateItem(string style, string UOM, string grpnm, string HSNCODE)
        {
            string DefaultAction = "A"; ItemDet ItemDet = new ItemDet();
            M_SITEM MSITEM = new M_SITEM(); M_GROUP MGROUP = new M_GROUP();
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            try
            {
                OraCon.Open();
                MSITEM.CLCD = CommVar.ClientCode(UNQSNO);
                var STYLEdt = (from g in DB.M_SITEM
                               join h in DB.M_GROUP on g.ITGRPCD equals h.ITGRPCD
                               where g.STYLENO == style
                               select new
                               {
                                   ITCD = g.ITCD,
                                   PURGLCD = h.PURGLCD,
                               }).FirstOrDefault();
                if (STYLEdt != null)
                {
                    ItemDet.ITCD = STYLEdt.ITCD;
                    ItemDet.PURGLCD = STYLEdt.PURGLCD;
                    return ItemDet;
                }
                MSITEM.EMD_NO = 0;
                MSITEM.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());
                MGROUP = CreateGroup(grpnm);
                string sql = "select max(itcd)itcd from " + CommVar.CurSchema(UNQSNO) + ".m_sitem where itcd like('" + MGROUP.ITGRPTYPE + MGROUP.GRPBARCODE + "%') ";
                var tbl = masterHelp.SQLquery(sql);
                if (tbl.Rows[0]["itcd"].ToString() == "")
                {
                    MSITEM.ITCD = MGROUP.ITGRPTYPE + MGROUP.GRPBARCODE + "00001";
                }
                else
                {
                    string s = tbl.Rows[0]["itcd"].ToString();
                    string digits = new string(s.Where(char.IsDigit).ToArray());
                    string letters = new string(s.Where(char.IsLetter).ToArray());
                    int number;
                    if (!int.TryParse(digits, out number))                   //int.Parse would do the job since only digits are selected
                    {
                        Console.WriteLine("Something weired happened");
                    }
                    MSITEM.ITCD = letters + (++number).ToString("D7");
                }
                MSITEM.ITGRPCD = MGROUP.ITGRPCD;
                MSITEM.ITNM = "";
                MSITEM.STYLENO = style.Trim();
                MSITEM.UOMCD = UOM;
                MSITEM.HSNCODE = HSNCODE;
                MSITEM.NEGSTOCK = "Y";
                var MPRODGRP = DB.M_PRODGRP.FirstOrDefault();
                MSITEM.PRODGRPCD = MPRODGRP?.PRODGRPCD;


                M_SITEM_BARCODE MSITEMBARCODE1 = new M_SITEM_BARCODE();
                MSITEMBARCODE1.EMD_NO = MSITEM.EMD_NO;
                MSITEMBARCODE1.CLCD = MSITEM.CLCD;
                MSITEMBARCODE1.ITCD = MSITEM.ITCD;
                MSITEMBARCODE1.BARNO = Salesfunc.GenerateBARNO(MSITEM.ITCD, "", "");

                DB.M_SITEM_BARCODE.Add(MSITEMBARCODE1);


                OracleCommand OraCmd = OraCon.CreateCommand();
                using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(false, "M_GROUP", MGROUP.M_AUTONO, DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                    dbsql = masterHelp.RetModeltoSql(MCH, "A", CommVar.FinSchema(UNQSNO));
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelp.RetModeltoSql(MSITEM, "A", CommVar.FinSchema(UNQSNO));
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    OraTrans.Rollback();
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
            OraCon.Dispose();
            ItemDet.ITCD = MSITEM.ITCD;
            ItemDet.PURGLCD = MGROUP.PURGLCD;
            return ItemDet;
        }
        public M_GROUP CreateGroup(string grpnm)
        {
            M_GROUP MGROUP = new M_GROUP();
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            try
            {
                string DefaultAction = "A";
                var tMGROU = DB.M_GROUP.Where(m => m.GRPNM == grpnm).FirstOrDefault();
                if (tMGROU != null)
                {
                    return tMGROU;
                }
                MGROUP.CLCD = CommVar.ClientCode(UNQSNO);
                MGROUP.EMD_NO = 0;
                MGROUP.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());

                string txtst = grpnm.Substring(0, 1).Trim().ToUpper();
                string sql = " select max(SUBSTR(ITGRPCD, 2)) ITGRPCD FROM " + CommVar.CurSchema(UNQSNO) + ".M_GROUP";
                string sql1 = " select max(GRPBARCODE) GRPBARCODE FROM " + CommVar.CurSchema(UNQSNO) + ".M_GROUP";
                var tbl = masterHelp.SQLquery(sql);
                if (tbl.Rows[0]["ITGRPCD"].ToString() != "")
                {
                    MGROUP.ITGRPCD = txtst + ((tbl.Rows[0]["ITGRPCD"]).retInt() + 1).ToString("D3");
                }
                else
                {
                    MGROUP.ITGRPCD = txtst + (10).ToString("D3");
                }
                var tb1l = masterHelp.SQLquery(sql1);
                if (tb1l.Rows[0]["GRPBARCODE"].ToString() != "")
                {
                    MGROUP.GRPBARCODE = ((tb1l.Rows[0]["GRPBARCODE"]).retInt() + 1).ToString("D2");
                }
                else
                {
                    MGROUP.GRPBARCODE = (10).ToString("D2");
                }
                OraCon.Open();
                OracleCommand OraCmd = OraCon.CreateCommand();
                using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(false, "M_GROUP", MGROUP.M_AUTONO, DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                    dbsql = masterHelp.RetModeltoSql(MCH, "A", CommVar.FinSchema(UNQSNO));
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    dbsql = masterHelp.RetModeltoSql(MGROUP, "A", CommVar.FinSchema(UNQSNO));
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    OraTrans.Rollback();
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
            OraCon.Dispose();
            return MGROUP;
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

        private string getSLCD(string sapcode)
        {
            sql = "select slcd from " + CommVar.CurSchema(UNQSNO) + ".m_subleg_com where sapcode='" + sapcode + "'";
            var dt = masterHelp.SQLquery(sql);
            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0]["slcd"].ToString();
            }
            return "";
        }
    }
}
