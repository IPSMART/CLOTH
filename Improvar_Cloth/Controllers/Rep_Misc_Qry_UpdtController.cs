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
                    CHNGSTYL.Add(new DropDown_list1 { value = "Change Pageno", text = "Change Pageno in bale" });
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
                string gocd = "", itcd = "";
                if (code != "")
                {
                    var data = code.Split(Convert.ToChar(Cn.GCS()));
                    gocd = data[0].retStr() == "" ? "" : data[0].retSqlformat();
                    itcd = data[1].retStr() == "" ? "" : data[1].retSqlformat();
                }

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
                var str = MasterHelp.BaleNo_help(val, tdt, gocd, itcd);
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
        //public ActionResult GetBarCodeDetails(string val, string Code)
        //{
        //    try
        //    {
        //        TransactionSalePosEntry VE = new TransactionSalePosEntry();
        //        //sequence MTRLJOBCD/PARTCD/DOCDT/TAXGRPCD/GOCD/PRCCD/ALLMTRLJOBCD
        //        Cn.getQueryString(VE);
        //        var data = Code.Split(Convert.ToChar(Cn.GCS()));
        //        string barnoOrStyle = val.retStr();
        //        string MTRLJOBCD = data[0].retSqlformat();
        //        string PARTCD = data[1].retStr();
        //        string DOCDT = CommVar.CurrDate(UNQSNO); /*data[2].retStr()*/
        //        string TAXGRPCD = data[3].retStr();
        //        string GOCD = data[4].retStr() == "" ? "" : data[4].retStr().retSqlformat();   /*data[2].retStr() == "" ? "" : data[4].retStr().retSqlformat();*/
        //        string PRCCD = data[5].retStr();
        //        if (MTRLJOBCD == "" || barnoOrStyle == "") { MTRLJOBCD = data[6].retStr(); }
        //        string BARNO = data[7].retStr() == "" || val.retStr() == "" ? "" : data[7].retStr().retSqlformat();

        //        string str = MasterHelp.T_TXN_BARNO_help(barnoOrStyle, "PB", DOCDT, TAXGRPCD, GOCD, PRCCD, MTRLJOBCD, "", false, PARTCD, BARNO);
        //        if (str.IndexOf("='helpmnu'") >= 0)
        //        {
        //            return PartialView("_Help2", str);
        //        }
        //        else
        //        {
        //            if (str.IndexOf(Cn.GCS()) == -1) return Content(str);
        //            return Content(str);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Cn.SaveException(ex, "");
        //        return Content(ex.Message + ex.InnerException);
        //    }
        //}
        public ActionResult GetItemDetails(string val)
        {
            try
            {
                var str = MasterHelp.ITCD_help(val,"");
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
                if (VE.TEXTBOX1 == "Change Style")
                {
                    sql = "update " + schnm + ". T_TXNDTL set  ITCD= '" + VE.ITCD2 + "' "
                    + " where AUTONO='" + VE.BLAUTONO1 + "' and  ITCD= '" + VE.ITCD1 + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                }
                else
                {
                    sql = "update " + schnm + ". T_TXNDTL set PAGENO='" + VE.NEWPAGENO + "',PAGESLNO='" + VE.NEWPAGESLNO + "' "
                   + " where AUTONO='" + VE.BLAUTONO2 + "' and PAGENO='" + VE.OLDPAGENO.retStr() + "' and PAGESLNO='" + VE.OLDPAGESLNO.retStr() + "'   ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                }
                ModelState.Clear();
                OraTrans.Commit();
                OraTrans.Dispose();
                ContentFlg = "1";
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