using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using CrystalDecisions.CrystalReports.Engine;
using System.Data;
using System.IO;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class Rep_Bar_HistoryController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        //BarCode CnBarCode = new BarCode();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        string sql = "";
        string LOC = CommVar.Loccd(CommVar.getQueryStringUNQSNO()), COM = CommVar.Compcd(CommVar.getQueryStringUNQSNO()), scm1 = CommVar.CurSchema(CommVar.getQueryStringUNQSNO()), scmf = CommVar.FinSchema(CommVar.getQueryStringUNQSNO());

        // GET: Rep_Bar_History
        public ActionResult Rep_Bar_History(string reptype = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    RepBarHistory VE;
                    if (TempData["printparameter"] == null)
                    {
                        VE = new RepBarHistory();
                    }
                    else
                    {
                        VE = (RepBarHistory)TempData["printparameter"];
                    }
                    ViewBag.formname = VE.CaptionName;
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    //ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(),  CommVar.CurSchema(UNQSNO));
                    using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
                    {
                        DataTable repformat = Salesfunc.getRepFormat(VE.RepType, VE.DOCCD);
                        //if (repformat != null)
                        //{
                        //    VE.DropDown_list1 = (from DataRow dr in repformat.Rows
                        //                         select new DropDown_list1()
                        //                         {
                        //                             text = dr["text"].ToString(),
                        //                             value = dr["value"].ToString()
                        //                         }).ToList();
                        //}
                        //else
                        //{
                        //    List<DropDown_list1> drplst = new List<DropDown_list1>();
                        //    VE.DropDown_list1 = drplst;
                        //}

                        VE.DOCNM = (from j in DB.M_DOCTYPE where j.DOCCD == VE.DOCCD select j.DOCNM).SingleOrDefault();
                        //VE = (Rep_Bar_History)Cn.EntryCommonLoading(VE, VE.PermissionID);
                        VE.DefaultView = true;
                        VE.ExitMode = 1;
                        VE.DefaultDay = 0;
                        return View(VE);
                    }
                }
            }
            catch (Exception ex)
            {
                RepBarHistory VE = new RepBarHistory();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }

        }
        public ActionResult GetDOC_Code(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.DOCCD_help(val, "", ""));
                }
                else
                {
                    string str = masterHelp.DOCCD_help(val, "", "");
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetDOC_Number(string val, string Code)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.DOCNO_help(val, Code));
                }
                else
                {
                    string str = masterHelp.DOCNO_help(val, Code);
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