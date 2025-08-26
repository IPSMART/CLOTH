using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;

namespace Improvar.Controllers
{
    public class Rep_Misc_Qry_UpdtController : Controller
    {
        // GET: Rep_Misc_Qry_Updt
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult Rep_Misc_Qry_Updt()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Misc Update Queries";
                    RepMiscQryUpdt VE = new RepMiscQryUpdt();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    List<DropDown_list1> CHNGSTYL = new List<DropDown_list1>();
                    CHNGSTYL.Add(new DropDown_list1 { value = "Change Style", text = "Change Style No in Bale" });
                    CHNGSTYL.Add(new DropDown_list1 { value = "Change Pageno", text = "Change Pageno in Bale" });
                    CHNGSTYL.Add(new DropDown_list1 { value = "Change BaleNo", text = "Change Bale No." });
                    CHNGSTYL.Add(new DropDown_list1 { value = "Change PREFNO && PREFDT", text = "Change PartyBill No. && PartyBill Dt." });
                    CHNGSTYL.Add(new DropDown_list1 { value = "Change Opening Rate", text = "Change Opening Rate" });
                    CHNGSTYL.Add(new DropDown_list1 { value = "Change LRNo && LRDate", text = "Change LRNo && LRDate" });
                    VE.DropDown_list1 = CHNGSTYL;
                    VE.NEWPREFDT = Cn.getCurrentDate(VE.mindate);
                    VE.TEXTBOX1 = Session["LASTUPDATE"].retStr();
                    VE.DefaultView = true;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetBaleNoDetails(string val, string code)
        {
            try
            {
                string gocd = "", itcd = "", pageno = "";
                bool skipstyleno = false, skippageno = false;
                if (code != "")
                {
                    var data = code.Split(Convert.ToChar(Cn.GCS()));
                    skipstyleno = data[0].retStr() == "Change Pageno" ? true : false;
                    skippageno = data[0].retStr() == "Change Style" ? true : false;
                    if (data[0].retStr() == "Change BaleNo")
                    {
                        skipstyleno = true;
                        skippageno = true;
                        gocd = data[1].retStr() == "" ? "" : data[1].retSqlformat();
                    }
                    else
                    {
                        if (data.Length > 1)
                        {
                            gocd = data[1].retStr() == "" ? "" : data[1].retSqlformat();
                            itcd = data[2].retStr() == "" ? "" : data[2].retSqlformat();
                        }
                        if (data.Length > 3)
                        {
                            pageno = data[3].retStr() == "" ? "" : data[3].retSqlformat();
                        }
                    }


                }
                if (val.retStr() == "") pageno = "";
                var tdt = CommVar.CurrDate(UNQSNO);
                if (val != "")
                {
                    string sql = "select distinct baleno||baleyr baleno from " + CommVar.CurSchema(UNQSNO) + ".T_BALE where BALENO like'%" + val + "%'  ";
                    DataTable dt = MasterHelp.SQLquery(sql);
                    if (dt.Rows.Count > 0)
                    {
                        val = (from DataRow dr in dt.Rows
                               select dr["baleno"].retStr()).ToArray().retSqlfromStrarray();
                    }
                    else
                    {
                        val = val.retSqlformat();
                    }
                }
                else
                {
                    val = val.retSqlformat();
                }
                var str = MasterHelp.BaleNo_help(val, tdt, gocd, itcd, skipstyleno, skippageno, pageno, false);
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
        public ActionResult GetBarCodeDetails(string val, string Code)
        {
            try
            {
                TransactionSalePosEntry VE = new TransactionSalePosEntry();
                //sequence MTRLJOBCD/PARTCD/DOCDT/TAXGRPCD/GOCD/PRCCD/ALLMTRLJOBCD
                Cn.getQueryString(VE);
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                string barnoOrStyle = val.retStr();
                string DOCDT = CommVar.CurrDate(UNQSNO); /*data[2].retStr()*/
                string BARNO = data[0].retStr() == "" || val.retStr() == "" ? "" : data[0].retStr().retSqlformat();

                string str = MasterHelp.T_TXN_BARNO_help(barnoOrStyle, "PB", DOCDT, "C001", "", "WP", "", "", false, "", BARNO);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    if (str.IndexOf(Cn.GCS()) == -1) return Content(str);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult CheckBaleno(string BALENO)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string COM_CD = CommVar.Compcd(UNQSNO);

            var query = (from c in DB.T_BALE
                         join d in DB.T_CNTRL_HDR on c.AUTONO equals d.AUTONO
                         where (c.BALENO == BALENO && d.COMPCD == COM_CD)
                         select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult GetDOCNO(string val, string code)
        {
            var str = MasterHelp.DOCNO_PUR_help(val);
            if (str.IndexOf("='helpmnu'") >= 0)
            {
                return PartialView("_Help2", str);
            }
            else
            {
                return Content(str);
            }
        }
        public ActionResult GetDOCNO2(string val, string Code)
        {
            try
            {
                if (val.retStr() == "")
                {
                    Code = "";
                }
                DataTable tbl = GetPurDet(val, Code);
                if (val.retStr() == "" || tbl.Rows.Count > 1)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + tbl.Rows[i]["DOCNO"] + "</td><td>" + tbl.Rows[i]["DOCDT"].retDateStr() + "</td><td>" + tbl.Rows[i]["SLCD"] + "</td><td>" + tbl.Rows[i]["SLNM"] + "</td><td>" + tbl.Rows[i]["autono"] + "</td><td>" + tbl.Rows[i]["prefno"] + "</td><td>" + tbl.Rows[i]["prefdt"] + "</td></tr>");
                    }
                    var hdr = "Doc No" + Cn.GCS() + "Doc Dt" + Cn.GCS() + "Party Code" + Cn.GCS() + "Party Name" + Cn.GCS() + "Autono" + Cn.GCS() + "PBill No" + Cn.GCS() + "PBill Date";
                    return PartialView("_Help2", MasterHelp.Generate_help(hdr, SB.ToString(), "4"));
                }
                else
                {
                    string str = "";
                    if (tbl.Rows.Count > 0)
                    {
                        str = MasterHelp.ToReturnFieldValues("", tbl);
                    }
                    else
                    {
                        str = "Invalid Document No. ! Please Select / Enter a Valid Document No. !!";
                    }
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message + " " + ex.InnerException);
            }
        }
        private DataTable GetPurDet(string docno, string autono)
        {
            string scm = CommVar.CurSchema(UNQSNO);
            string fcm = CommVar.FinSchema(UNQSNO);


            var COMPCD = CommVar.Compcd(UNQSNO);
            var LOCCD = CommVar.Loccd(UNQSNO);
            string valsrch = docno.ToUpper().Trim();
            string sql = "";
            sql += "select distinct b.DOCNO,to_char(b.DOCDT,'dd/mm/yyyy') DOCDT,a.SLCD,a.AUTONO ,c.SLNM,d.LRNO,to_char(d.LRDT,'dd/mm/yyyy') LRDT,a.PREFNO,to_char(a.PREFDT,'dd/mm/yyyy') PREFDT from " + scm + ".T_TXN a, " + scm + ".T_CNTRL_HDR b,  ";
            sql += fcm + ".M_SUBLEG c, " + scm + ".T_TXNTRANS d, " + scm + ".m_doctype e where a.AUTONO=b.AUTONO(+) and a.SLCD=c.SLCD(+) and a.AUTONO=d.AUTONO and b.doccd=e.doccd(+) and d.LRNO is not null  and b.compcd='" + COMPCD + "' and b.loccd='" + LOCCD + "' and e.doctype in ('SPBL') ";
            //sql += fcm + ".M_SUBLEG c, " + scm + ".T_TXNTRANS d, " + scm + ".m_doctype e where a.AUTONO=b.AUTONO(+) and a.SLCD=c.SLCD(+) and a.AUTONO=d.AUTONO and b.doccd=e.doccd(+) and d.LRNO is not null and d.LRDT is not null and b.compcd='" + COMPCD + "' and b.loccd='" + LOCCD + "' and e.doctype in ('SPBL') ";
            if (valsrch.retStr() != "") sql += " and upper(b.DOCNO) = '" + valsrch + "' ";
            if (autono.retStr() != "") sql += " and (b.autono) = '" + autono + "' ";
            sql += "  order by DOCNO,DOCDT ";
            DataTable tbl1 = MasterHelp.SQLquery(sql);

            string doctype = "'BLTG'";
            var dt1 = Salesfunc.GetBaleHistory("", "", "", doctype);
            if (dt1 != null && dt1.Rows.Count > 0)
            {
                string[] autonum = (from DataRow dr in dt1.Rows select dr["blautono"].retStr()).ToArray();
                var tt = (from DataRow a in tbl1.Rows where !autonum.Contains(a["autono"]) select a);
                if (tt.Count() > 0)
                {
                    tbl1 = tt.CopyToDataTable();
                }
                else
                {
                    tbl1 = tbl1.Clone();
                }
            }

            sql = "";
            sql += "select a.autono,b.docno,b.docdt,c.docnm,a.blautono ";
            sql += "from " + scm + ".T_BALE a,  " + scm + ".t_cntrl_hdr b,  " + scm + ".m_doctype c ";
            sql += "where a.autono = B.AUTONO and b.doccd = c.DOCCD and nvl(b.cancel,'N') = 'N'  and a.blautono <> a.autono  ";

            DataTable dt = MasterHelp.SQLquery(sql);
            if (dt.Rows.Count > 0)
            {
                string[] autonum = (from DataRow dr in dt.Rows select dr["blautono"].retStr()).ToArray();
                var tt = (from DataRow b in tbl1.Rows where !autonum.Contains(b["autono"]) select b);
                if (tt.Count() > 0)
                {
                    tbl1 = tt.CopyToDataTable();
                }
                else
                {
                    tbl1 = tbl1.Clone();
                }
            }
            return tbl1;
        }

        public ActionResult GetOPDOCNO(string val, string code)
        {
            try
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                string scm = CommVar.CurSchema(UNQSNO);
                string scmf = CommVar.FinSchema(UNQSNO);
                var COMPCD = CommVar.Compcd(UNQSNO);
                var LOCCD = CommVar.Loccd(UNQSNO);
                string valsrch = val.ToUpper().Trim();
                string autono = "";
                double slno = 0;
                if (val.retStr() != "")
                {
                    autono = code.Split(Convert.ToChar(Cn.GCS()))[0];
                    slno = code.Split(Convert.ToChar(Cn.GCS()))[1].retDbl();
                }
                string sql = "";
                sql += "select b.DOCNO,to_char(b.DOCDT,'dd/mm/yyyy') DOCDT,a.AUTONO,c.itcd,c.slno,d.styleno||' '||d.itnm itstyle,C.RATE,c.baleno   ";
                sql += "from " + scm + ".T_TXN a," + scm + ".T_CNTRL_HDR b," + scm + ".T_TXNDTL c," + scm + ".M_SITEM d  ";
                sql += " where a.AUTONO=b.AUTONO(+) and a.AUTONO=c.AUTONO(+) and c.itcd=d.itcd(+) and a.doctag in ('OP') and b.compcd='" + COMPCD + "' and b.loccd='" + LOCCD + "' ";
                if (valsrch.retStr() != "") sql += " and upper(b.DOCNO) = '" + valsrch + "' ";
                if (autono.retStr() != "") sql += " and a.autono = '" + autono + "' ";
                if (slno.retDbl() != 0) sql += " and c.slno = " + slno + " ";
                sql += "  order by DOCNO,DOCDT,slno ";
                DataTable tbl = MasterHelp.SQLquery(sql);
                if (val.retStr() == "")
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= tbl.Rows.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + tbl.Rows[i]["DOCNO"] + "</td><td>" + tbl.Rows[i]["DOCDT"].retDateStr() + "</td><td>" + tbl.Rows[i]["ITSTYLE"] + "</td><td>" + tbl.Rows[i]["baleno"] + "</td><td>" + tbl.Rows[i]["RATE"] + "</td><td>" + tbl.Rows[i]["SLNO"] + "</td><td>" + tbl.Rows[i]["autono"] + "</td></tr>");
                    }
                    var hdr = "Doc No" + Cn.GCS() + "Doc Dt" + Cn.GCS() + "Style" + Cn.GCS() + "Bale no." + Cn.GCS() + "Rate" + Cn.GCS() + "Slno" + Cn.GCS() + "Autono";
                    return PartialView("_Help2", MasterHelp.Generate_help(hdr, SB.ToString(), "6"));
                }
                else
                {
                    string str = "";
                    if (tbl.Rows.Count > 0)
                    {
                        str = MasterHelp.ToReturnFieldValues("", tbl);
                    }
                    else
                    {
                        str = "Invalid Document No. ! Please Select / Enter a Valid Document No. !!";
                    }
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message + " " + ex.InnerException);
            }

        }
        public ActionResult CheckBillNumber(RepMiscQryUpdt VE, string BILL_NO, string SUPPLIER, string AUTO_NO)
        {
            Cn.getQueryString(VE);
            if (VE.DefaultAction == "A") AUTO_NO = "";
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string COM_CD = CommVar.Compcd(UNQSNO);
            if (BILL_NO.retStr() == "") return Content("0");

            var query = (from c in DB.T_TXN
                         join d in DB.T_CNTRL_HDR on c.AUTONO equals d.AUTONO
                         where (c.PREFNO == BILL_NO && c.SLCD == SUPPLIER && c.AUTONO != AUTO_NO && d.COMPCD == COM_CD)
                         select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult Save(RepMiscQryUpdt VE)
        {
            Cn.getQueryString(VE);

            string sql = "";
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            OracleTransaction OraTrans;
            OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted);
            OraCmd.Transaction = OraTrans;

            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string ContentFlg = "";
                var schnm = CommVar.CurSchema(UNQSNO);
                var schnmf = CommVar.FinSchema(UNQSNO);
                var CLCD = CommVar.ClientCode(UNQSNO);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                string autono = "";
                if (VE.TEXTBOX1 == "Change Style")
                {
                    autono = VE.BLAUTONO1;
                }
                else if (VE.TEXTBOX1 == "Change BaleNo")
                {
                    autono = VE.BLAUTONO3;
                }
                else if (VE.TEXTBOX1 == "Change PREFNO && PREFDT")
                {
                    autono = VE.BLAUTONO4;
                }
                else
                {
                    autono = VE.BLAUTONO2;
                }
                if (VE.TEXTBOX1 == "Change PREFNO && PREFDT")
                {
                    //var dt = DB.T_TXN.Where(a => a.AUTONO == autono).Select(a => a.AUTONO).Distinct().ToArray();
                    //if (dt.Count() > 0)
                    //{
                    //    autono = autono.retSqlformat() + "," + dt.retSqlfromStrarray();
                    //}
                    //else
                    //{
                    autono = autono.retSqlformat();
                    //}
                }
                else
                {
                    var dt = DB.T_BALE.Where(a => a.BLAUTONO == autono).Select(a => a.AUTONO).Distinct().ToArray();
                    if (dt.Count() > 0)
                    {
                        autono = autono.retSqlformat() + "," + dt.retSqlfromStrarray();
                    }
                    else
                    {
                        autono = autono.retStr().retSqlformat();
                    }
                }
                if (VE.TEXTBOX1 == "Change PREFNO && PREFDT")
                {
                    if (!string.IsNullOrEmpty(VE.NEWPREFNO) && !string.IsNullOrEmpty(VE.NEWPREFDT.retDateStr()))
                    {
                        sql = "update " + schnm + ". T_TXN set  PREFNO= '" + VE.NEWPREFNO + "', PREFDT=TO_DATE('" + VE.NEWPREFDT.retDateStr() + "','dd/mm/yyyy')  "
                         + " where AUTONO in(" + autono + ") and  PREFNO= '" + VE.OLDPREFNO + "' and PREFDT=TO_DATE('" + VE.OLDPREFDT.retDateStr() + "','dd/mm/yyyy') ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnmf + ".T_VCH_BL set  BLNO= '" + VE.NEWPREFNO + "',BLDT= TO_DATE('" + VE.NEWPREFDT.retDateStr() + "','dd/mm/yyyy')  "
                        + " where AUTONO in(" + autono + ") and  BLNO= '" + VE.OLDPREFNO + "'  and BLDT=TO_DATE('" + VE.OLDPREFDT.retDateStr() + "','dd/mm/yyyy') ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnmf + ".T_VCH_GST set BLNO= '" + VE.NEWPREFNO + "',BLDT= TO_DATE('" + VE.NEWPREFDT.retDateStr() + "','dd/mm/yyyy')  "
                        + " where AUTONO in (" + autono + ")   and  BLNO= '" + VE.OLDPREFNO + "'  and BLDT=TO_DATE('" + VE.OLDPREFDT.retDateStr() + "','dd/mm/yyyy')";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                    }
                }

                else if (VE.TEXTBOX1 == "Change Style")
                {
                    if (!string.IsNullOrEmpty(VE.ITCD2))
                    {
                        sql = "update " + schnm + ". T_TXNDTL set  ITCD= '" + VE.ITCD2 + "' "
                        + " where AUTONO in(" + autono + ") and  ITCD= '" + VE.ITCD1 + "' and (BALENO='" + VE.BALENO1 + "' or BALENO is null) and BALEYR='" + VE.BALEYR1 + "'  ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ".T_BATCHMST set  ITCD= '" + VE.ITCD2 + "',BARNO= '" + VE.NEWBARNO + "' "
                        + " where AUTONO='" + VE.BLAUTONO1 + "' and  ITCD= '" + VE.ITCD1 + "'  and BARNO='" + VE.OLDBARNO + "'  ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        sql = "update " + schnm + ".T_BATCHDTL set BARNO= '" + VE.NEWBARNO + "' "
                        + " where AUTONO in (" + autono + ")  and BARNO='" + VE.OLDBARNO + "' and (BALENO='" + VE.BALENO1 + "' or BALENO is null) and BALEYR='" + VE.BALEYR1 + "' ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                    }
                }
                else if (VE.TEXTBOX1 == "Change BaleNo")
                {
                    sql = "update " + schnm + ". T_BALE set BALENO='" + VE.NEWBALENO + "' "
                   + " where BLAUTONO='" + VE.BLAUTONO3 + "' and BALENO='" + VE.OLDBALENO.retStr() + "' and BALEYR='" + VE.BALEYR3 + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    sql = "update " + schnm + ". T_BATCHDTL set BALENO='" + VE.NEWBALENO + "' "
                + " where AUTONO in (" + autono + ") and BALENO='" + VE.OLDBALENO.retStr() + "' and BALEYR='" + VE.BALEYR3 + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    sql = "update " + schnm + ". T_BILTY set BALENO='" + VE.NEWBALENO + "' "
                + " where BLAUTONO='" + VE.BLAUTONO3 + "' and BALENO='" + VE.OLDBALENO.retStr() + "' and BALEYR='" + VE.BALEYR3 + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    sql = "update " + schnm + ". T_PHYSTK set BALENO='" + VE.NEWBALENO + "' "
                + " where BALENO='" + VE.OLDBALENO.retStr() + "' and BALEYR='" + VE.BALEYR3 + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    sql = "update " + schnm + ". T_TXNDTL set BALENO='" + VE.NEWBALENO + "' "
                + " where AUTONO in (" + autono + ") and BALENO='" + VE.OLDBALENO.retStr() + "' and BALEYR='" + VE.BALEYR3 + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                }
                else if (VE.TEXTBOX1 == "Change Opening Rate")
                {
                    string barsql = "select distinct barno from " + schnm + ". T_BATCHDTL where AUTONO='" + VE.AUTONO + "' and TXNSLNO='"
                        + VE.SLNO + "' and RATE=" + VE.OLDRATE + " ";

                    sql = "update " + schnm + ". T_TXNDTL set RATE =" + VE.NEWRATE + ", AMT =ROUND(QNTY*" + VE.NEWRATE + ",2),TXBLVAL = ROUND(QNTY*" + VE.NEWRATE + ",2) "
                   + " where AUTONO='" + VE.AUTONO + "' and ITCD='" + VE.ITCD.retStr() + "' and SLNO='" + VE.SLNO + "' and RATE=" + VE.OLDRATE + " ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    sql = "update " + schnm + ". T_BATCHDTL set RATE =" + VE.NEWRATE + ",TXBLVAL = ROUND(QNTY*" + VE.NEWRATE + ",2) "
                   + " where AUTONO='" + VE.AUTONO + "' and TXNSLNO='" + VE.SLNO + "' and RATE=" + VE.OLDRATE + " ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    sql = "update " + schnm + ". T_BATCHMST set RATE =" + VE.NEWRATE + ", AMT =ROUND(QNTY*" + VE.NEWRATE + ",2) "
                    + " where AUTONO='" + VE.AUTONO + "'  and RATE=" + VE.OLDRATE + " and BARNO in (" + barsql + ") ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    sql = "update " + schnm + ". T_BATCHMST_PRICE set RATE =" + VE.NEWRATE + " "
                 + " where AUTONO='" + VE.AUTONO + "' and RATE=" + VE.OLDRATE + " and BARNO in (" + barsql + ") and PRCCD='CP' ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                }
                else
                {
                    sql = "update " + schnm + ". T_TXNDTL set PAGENO='" + VE.NEWPAGENO + "',PAGESLNO='" + VE.NEWPAGESLNO + "' "
                   + " where AUTONO='" + VE.BLAUTONO2 + "' and PAGENO='" + VE.OLDPAGENO.retStr() + "' and PAGESLNO='" + VE.OLDPAGESLNO.retStr() + "'  and BALENO='" + VE.BALENO2 + "' and BALEYR='" + VE.BALEYR2 + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                }


                if (VE.TEXTBOX1 == "Change LRNo && LRDate")
                {




                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    ImprovarDB DB2 = new ImprovarDB(Cn.GetConnectionString(), CommVar.SaleSchema(UNQSNO));

                    string AUTONO = VE.BLAUTONO7.retStr();
                    string LRNO = VE.NEWLRNO.retStr();
                    string LRDT = VE.NEWLRDT.retDateStr();



                    //update to t_txntrans//
                    var ttxantrans_data = DB2.T_TXNTRANS.Where(a => a.AUTONO == AUTONO).Select(b => b.AUTONO).ToList();
                    if (ttxantrans_data != null && ttxantrans_data.Count > 0)
                    {
                        sql = "update " + schnm + ". t_txntrans set LRNO ='" + LRNO + "', ";
                        sql += " LRDT =to_date('" + LRDT.retDateStr() + "', 'dd/mm/yyyy')";
                        sql += " where AUTONO='" + AUTONO + "'  ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                    }


                    //update to T_TXNEWB//
                    var txnweb_data = DB1.T_TXNEWB.Where(a => a.AUTONO == AUTONO).Select(b => b.AUTONO).ToList();
                    if (txnweb_data != null && txnweb_data.Count > 0)
                    {
                        sql = "update " + schnmf + ". T_TXNEWB set LRNO ='" + LRNO + "', ";
                        sql += " LRDT =to_date('" + LRDT.retDateStr() + "', 'dd/mm/yyyy')";
                        sql += " where AUTONO='" + AUTONO + "'  ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                    }



                    //update to T_VCH_BL//
                    var tvchbl_data = DB1.T_VCH_BL.Where(a => a.AUTONO == AUTONO).Select(b => b.AUTONO).ToList();
                    if (tvchbl_data != null && tvchbl_data.Count > 0)
                    {
                        sql = "update " + schnmf + ". T_VCH_BL set LRNO ='" + LRNO + "',LRDT =to_date('" + LRDT.retDateStr() + "', 'dd/mm/yyyy') ";
                        sql += " where AUTONO='" + AUTONO + "'  ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                    }


                    //update to T_BALE//
                    var tbale_data = DB2.T_BALE.Where(a => a.AUTONO == AUTONO).Select(b => b.AUTONO).ToList();
                    if (tbale_data != null && tbale_data.Count > 0)
                    {
                        sql = "update " + schnm + ". T_BALE set LRNO ='" + LRNO + "',LRDT =to_date('" + LRDT.retDateStr() + "', 'dd/mm/yyyy') ";
                        sql += " where AUTONO='" + AUTONO + "'  ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                    }

                }




                Session["LASTUPDATE"] = VE.TEXTBOX1;
                ModelState.Clear();
                OraTrans.Commit();
                OraTrans.Dispose();
                ContentFlg = "Update Successfully";
                return Content(ContentFlg);
            }
            catch (Exception ex)
            {
                OraTrans.Rollback();
                OraTrans.Dispose();
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
    }
}