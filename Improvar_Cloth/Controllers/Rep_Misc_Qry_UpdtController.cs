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
                    VE.DropDown_list1 = CHNGSTYL;
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
                var str = MasterHelp.BaleNo_help(val, tdt, gocd, itcd, skipstyleno, skippageno, pageno,false);
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
                else
                {
                    autono = VE.BLAUTONO2;
                }
                var dt = DB.T_BALE.Where(a => a.BLAUTONO == autono).Select(a => a.AUTONO).Distinct().ToArray();
                if (dt.Count() > 0)
                {
                    autono = autono.retSqlformat() + "," + dt.retSqlfromStrarray();
                }
                else
                {
                    autono = autono.retSqlformat();
                }
                if (VE.TEXTBOX1 == "Change Style")
                {
                    sql = "update " + schnm + ". T_TXNDTL set  ITCD= '" + VE.ITCD2 + "' "
                    + " where AUTONO in(" + autono + ") and  ITCD= '" + VE.ITCD1 + "' and BALENO='" + VE.BALENO1 + "' and BALEYR='" + VE.BALEYR1 + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    sql = "update " + schnm + ".T_BATCHMST set  ITCD= '" + VE.ITCD2 + "',BARNO= '" + VE.NEWBARNO + "' "
                  + " where AUTONO='" + VE.BLAUTONO1 + "' and  ITCD= '" + VE.ITCD1 + "'  and BARNO='" + VE.OLDBARNO + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    sql = "update " + schnm + ".T_BATCHDTL set BARNO= '" + VE.NEWBARNO + "' "
                + " where AUTONO in (" + autono + ")  and BARNO='" + VE.OLDBARNO + "' and BALENO='" + VE.BALENO1 + "' and BALEYR='" + VE.BALEYR1 + "' ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

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
                else
                {
                    sql = "update " + schnm + ". T_TXNDTL set PAGENO='" + VE.NEWPAGENO + "',PAGESLNO='" + VE.NEWPAGESLNO + "' "
                   + " where AUTONO='" + VE.BLAUTONO2 + "' and PAGENO='" + VE.OLDPAGENO.retStr() + "' and PAGESLNO='" + VE.OLDPAGESLNO.retStr() + "'  and BALENO='" + VE.BALENO2 + "' and BALEYR='" + VE.BALEYR2 + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                }
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