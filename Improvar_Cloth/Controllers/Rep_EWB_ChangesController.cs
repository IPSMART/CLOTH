using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using CrystalDecisions.CrystalReports.Engine;
using System.Data;
using System.IO;
using System.Collections.Generic;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Oracle.ManagedDataAccess.Client;

namespace Improvar.Controllers
{
    public class Rep_EWB_ChangesController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        //BarCode CnBarCode = new BarCode();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        string sql = "";
        string LOC = CommVar.Loccd(CommVar.getQueryStringUNQSNO()), COM = CommVar.Compcd(CommVar.getQueryStringUNQSNO()), scm1 = CommVar.CurSchema(CommVar.getQueryStringUNQSNO()), scmf = CommVar.FinSchema(CommVar.getQueryStringUNQSNO());
        // GET: Rep_EWB_Changes
        public ActionResult Rep_EWB_Changes(string reptype = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    RepEWBChanges VE;
                    if (TempData["printparameter"] == null)
                    {
                        VE = new RepEWBChanges();
                    }
                    else
                    {
                        VE = (RepEWBChanges)TempData["printparameter"];
                    }
                    ViewBag.formname = "Eway Bill Changes";
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
                    {
                        VE.Database_Combo1 = (from i in DBF.T_TXNEWB select new Database_Combo1() { FIELD_VALUE = i.REASONCD }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                        VE.Database_Combo2 = (from i in DBF.T_TXNEWB select new Database_Combo2() { FIELD_VALUE = i.REASONREM }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                        
                        VE.DefaultView = true;
                        VE.ExitMode = 1;
                        VE.DefaultDay = 0;
                        return View(VE);
                    }
                }
            }
            catch (Exception ex)
            {
                RepEWBChanges VE = new RepEWBChanges();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }

        }
       
        public static string RenderRazorViewToString(ControllerContext controllerContext, string viewName, object model)
        {
            controllerContext.Controller.ViewData.Model = model;
            using (var stringWriter = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                var viewContext = new ViewContext(controllerContext, viewResult.View, controllerContext.Controller.ViewData, controllerContext.Controller.TempData, stringWriter);
                viewResult.View.Render(viewContext, stringWriter);
                viewResult.ViewEngine.ReleaseView(controllerContext, viewResult.View);
                return stringWriter.GetStringBuilder().ToString();
            }
        }
      
        public ActionResult Update(RepEWBChanges VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            string sql = "";
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                OraCmd.Transaction = OraTrans;
                try
                {
                    string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                    string ContentFlg = "";
                    var schnm = CommVar.CurSchema(UNQSNO);
                    var schnmF = CommVar.FinSchema(UNQSNO);
                    var CLCD = CommVar.ClientCode(UNQSNO);
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));

                    var LRDT = VE.LRDT1.retDateStr();
                    var LRNO = VE.LRNO1.retStr();
                    var EWAYBILLNO = VE.EWAYBILLNO.retStr();
                    var TRANSLCD = VE.TRSLCD1.retStr();
                    var LORRYNO = VE.LORRYNO1.retStr();
                    var REASONCD = VE.REASONCD.retStr()==""?"00": VE.REASONCD.retStr();
                    var REASONREM = VE.REASONREM.retStr();
                    var AGSLCD = VE.AGSLCD1.retStr();

                    var CHNG_BY = CommVar.UserID();
                    var CHNG_TIME = "to_date('" + System.DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");

                    sql = "update " + schnm + ".T_TXNOTH set AGSLCD ='" + AGSLCD + "' "
                               + " where AUTONO='" + VE.AUTONO + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    sql = "update " + schnm + ".t_txntrans set LRDT =to_date('" + LRDT + "','dd/mm/yyyy'),LRNO ='" + LRNO + "',TRANSLCD ='" + TRANSLCD + "',LORRYNO ='" + LORRYNO + "' "
                                 + " where AUTONO='" + VE.AUTONO + "'  ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    sql = "update " + schnmF + ".t_txnewb set LRDT =to_date('" + LRDT + "','dd/mm/yyyy'),LRNO ='" + LRNO + "',TRANSLCD ='" + TRANSLCD + "',LORRYNO ='" + LORRYNO + "',REASONCD ='" + REASONCD + "',REASONREM ='" + REASONREM + "',CHNG_BY ='" + CHNG_BY + "',CHNG_TIME =to_date('" + System.DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
                    sql += "','dd/mm/yyyy hh24:mi:ss')";
                    sql += " where AUTONO='" + VE.AUTONO + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    sql = "update " + schnmF + ".t_vch_bl set AGSLCD ='" + AGSLCD + "' ";
                    sql += " where AUTONO='" + VE.AUTONO + "'  ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                    

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
        public ActionResult GetBLNODetails(string val, string Code)
        {
            try
            { 
                var autono = Code.Split('~')[0];
                
                string docdt = System.DateTime.Today.AddDays(-15).retDateStr();
                var str = masterHelp.BLNO_help(val, autono, docdt);
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
        public ActionResult GetPartyDetails(RepEWBChanges VE, string val, string TAG, string DOC_DT)
        {
            try
            {
                Cn.getQueryString(VE);
                string LINK_CD = "";
                if (TAG != null && TAG != "")
                {
                    LINK_CD = TAG;
                }
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.SubLeg_help(val, LINK_CD));
                }
                else
                {
                    string str = masterHelp.SubLeg_help(val, LINK_CD);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }


    }
}