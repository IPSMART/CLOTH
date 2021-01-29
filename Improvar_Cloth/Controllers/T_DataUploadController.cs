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
                return View();
            }
            return View();
        }
        [HttpPost]
        public ActionResult T_DataUpload(DataUploadVM VE, FormCollection FC, String Command)
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            ReadRaymondPurchaseDBF();
            return null;
        }
        public string ReadRaymondPurchaseDBF()
        {
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
                T_TXNEWB TTXNEWB = new T_TXNEWB();
                string auto_no = ""; string Month = "";

              //var er=  dbfdt



                TTXN.EMD_NO = 0;
                TTXN.DOCCD = DB.M_DOCTYPE.Where(d => d.DOCTYPE == "PB").FirstOrDefault()?.DOCCD;
                TTXN.CLCD = CommVar.ClientCode(UNQSNO);
                List<TTXNDTL> TTXNDTLlist = new List<Models.TTXNDTL>();
                short slno = 0;
                foreach (DataRow dr in dbfdt.Rows)
                {
                    string CUSTOMERNO = dr["CUSTOMERNO"].ToString();
                    TTXN.SLCD = getSLCD(CUSTOMERNO);
                    string Ddate = DateTime.ParseExact(dr["INVDATE"].ToString(), "MM/dd/yyyy", CultureInfo.InvariantCulture).ToString("dd/mm/yyyy");
                    TTXN.DOCDT = Convert.ToDateTime(Ddate);
                    TTXN.GOCD = "TR";
                    TTXN.DOCTAG = "PB";
                    TTXN.PREFNO = dr["INV_NO"].ToString();
                    TTXN.PREFDT = TTXN.DOCDT;
                    TTXN.BLAMT = dr["NET_AMT"].retDbl();


                    //-------------------------EWB--------------------------//
                    string TRANSLCD = "";
                    if (dr["CARR_NO"].ToString() != "")
                    {
                        TRANSLCD = getSLCD(dr["CARR_NO"].ToString());
                    }              
                    TTXNEWB.AUTONO = TTXN.AUTONO;
                    TTXNEWB.EMD_NO = TTXN.EMD_NO;
                    TTXNEWB.CLCD = TTXN.CLCD;
                    TTXNEWB.DTAG = TTXN.DTAG;
                    TTXNEWB.TRANSLCD = getSLCD(dr["CARR_NO"].ToString());
                    //TTXNEWB.EWAYBILLNO = VE.T_TXNTRANS.EWAYBILLNO;
                    TTXNEWB.LRNO = dr["LR_NO"].ToString();
                    string LR_DATE = DateTime.ParseExact(dr["LR_DATE"].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture).ToString("dd/mm/yyyy");
                    TTXNEWB.LRDT = Convert.ToDateTime(LR_DATE);
                    //TTXNEWB.LORRYNO = VE.T_TXNTRANS.LORRYNO;
                    //TTXNEWB.TRANSMODE = VE.T_TXNTRANS.TRANSMODE;
                    //TTXNEWB.VECHLTYPE = VE.T_TXNTRANS.VECHLTYPE;
                    //TTXNEWB.GOCD = VE.T_TXN.GOCD;
                    //----------------------------------------------------------//

                    //-------------------------Transport--------------------------//
                    TXNTRANS.AUTONO = TTXN.AUTONO;
                    TXNTRANS.EMD_NO = TTXN.EMD_NO;
                    TXNTRANS.CLCD = TTXN.CLCD;
                    TXNTRANS.DTAG = TTXN.DTAG;
                    TXNTRANS.TRANSLCD = TRANSLCD;
                    //TXNTRANS.TRANSMODE = VE.T_TXNTRANS.TRANSMODE;
                    //TXNTRANS.CRSLCD = VE.T_TXNTRANS.CRSLCD;
                    //TXNTRANS.EWAYBILLNO = VE.T_TXNTRANS.EWAYBILLNO;
                    TXNTRANS.LRNO = dr["LR_NO"].ToString();
                    TXNTRANS.LRDT = Convert.ToDateTime(LR_DATE);
                    //TXNTRANS.LORRYNO = VE.T_TXNTRANS.LORRYNO;
                    //TXNTRANS.GRWT = VE.T_TXNTRANS.GRWT;
                    //TXNTRANS.TRWT = VE.T_TXNTRANS.TRWT;
                    //TXNTRANS.NTWT = VE.T_TXNTRANS.NTWT;
                    //TXNTRANS.DESTN = VE.T_TXNTRANS.DESTN;
                    //TXNTRANS.RECVPERSON = VE.T_TXNTRANS.RECVPERSON;
                    //TXNTRANS.VECHLTYPE = VE.T_TXNTRANS.VECHLTYPE;
                    //TXNTRANS.GATEENTNO = VE.T_TXNTRANS.GATEENTNO;
                    //----------------------------------------------------------//




                    T_TXNDTL TTXNDTL = new T_TXNDTL();
                    TTXNDTL.CLCD = TTXN.CLCD;
                    TTXNDTL.EMD_NO = TTXN.EMD_NO;
                    TTXNDTL.AUTONO = TTXN.AUTONO;
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



                }










            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
            return null;
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
        public class ItemDet
        {
            public string ITCD { get; set; }
            public string PURGLCD { get; set; }
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
