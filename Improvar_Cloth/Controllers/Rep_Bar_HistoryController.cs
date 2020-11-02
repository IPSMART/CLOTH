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
        public ActionResult GetBarCodeDetails(string val, string Code)
        {
            try
            {
                //sequence MTRLJOBCD/PARTCD/DOCDT/TAXGRPCD/GOCD/PRCCD/ALLMTRLJOBCD
                RepBarHistory VE = new RepBarHistory();
                Cn.getQueryString(VE); string scm = CommVar.CurSchema(UNQSNO); string scmf = CommVar.FinSchema(UNQSNO);
                var data = Code.Split(Convert.ToChar(Cn.GCS()));
                string barnoOrStyle = val.retStr();
                //string MTRLJOBCD = data[0].retSqlformat();
                //string PARTCD = data[1].retStr();
                string DOCDT = System.DateTime.Today.ToString().retDateStr();   /*data[2].retStr()*/
                //string TAXGRPCD = data[3].retStr();
                //string GOCD = data[2].retStr() == "" ? "" : data[4].retStr().retSqlformat();
                //string PRCCD = data[5].retStr();
                //if (MTRLJOBCD == "" || barnoOrStyle == "") { MTRLJOBCD = data[6].retStr(); }
                string str = masterHelp.T_TXN_BARNO_help(barnoOrStyle, VE.MENU_PARA, DOCDT, "", "", "", "");
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    if (str.IndexOf(Cn.GCS()) == -1) return Content(str);
                    string sql1 = "select distinct a.SLNO,a.AUTONO,a.BARNO,b.DOCNO,b.DOCDT,b.PREFNO,c.DOCNM,b.SLCD,d.SLNM,d.DISTRICT,a.STKDRCR,a.QNTY, ";
                    sql1 += "a.NOS,a.RATE,a.DISCTYPE,a.GOCD,e.GONM,f.LOCCD,g.LOCNM,nvl(e.GONM,g.LOCNM)LOCANM from " + scm + ".t_batchdtl a," + scm + ".t_txn b," + scm + ".m_doctype c, ";
                    sql1 += "" + scmf + ".m_subleg d," + scm + ".m_godown e," + scm + ".t_cntrl_hdr f ," + scmf + ".m_loca g   where a.AUTONO=b.AUTONO(+) and b.DOCCD=c.DOCCD(+)  ";
                    sql1 += "and b.SLCD=d.SLCD(+) and a.GOCD=e.GOCD(+) and a.AUTONO=f.AUTONO(+) and f.LOCCD=g.LOCCD(+) and a.BARNO='" + barnoOrStyle + "' order by b.DOCDT,b.DOCNO";

                    string sql2 = "select a.EFDTF,a.PRCCD,a.RATE from " + scm + ".m_itemplistdtl a where a.BARNO='" + barnoOrStyle + "'";
                    DataTable tbatchdtl = masterHelp.SQLquery(sql1);
                    DataTable itempricedtl = masterHelp.SQLquery(sql2);
                    VE.BARCODEHISTORY = (from DataRow dr in tbatchdtl.Rows
                                         select new BARCODEHISTORY
                                         {
                                             SLNO = dr["SLNO"].retShort(),
                                             AUTONO = dr["AUTONO"].retStr(),
                                             DOCDT = dr["DOCDT"].retDateStr(),
                                             DOCNO = dr["DOCNO"].retStr(),
                                             PREFNO = dr["PREFNO"].retStr(),
                                             SLNM = dr["SLCD"].retStr() == "" ? "" : dr["SLNM"].retStr() + "[" + dr["SLCD"].retStr() + "]" + "[" + dr["DISTRICT"].retStr() + "]",
                                             LOCNM = dr["LOCANM"].retStr(),
                                             NOS = dr["NOS"].retDbl(),
                                             RATE = dr["RATE"].retDbl(),
                                             STKDRCR = dr["STKDRCR"].retStr(),
                                             QNTY = dr["QNTY"].retDbl(),
                                             DOCNM = dr["DOCNM"].retStr()
                                         }).ToList();
                    double TINQTY = 0, TOUTQTY = 0, TNOS = 0;
                    for (int p = 0; p <= VE.BARCODEHISTORY.Count - 1; p++)
                    {
                        var INQNTY = (from i in VE.BARCODEHISTORY
                                      where i.STKDRCR == "D"
                                      select i.QNTY).FirstOrDefault();
                        var OUTQNTY = (from i in VE.BARCODEHISTORY
                                       where i.STKDRCR == "C"
                                       select i.QNTY).FirstOrDefault();
                        VE.BARCODEHISTORY[p].INQNTY = INQNTY.retDbl();
                        VE.BARCODEHISTORY[p].OUTQNTY = OUTQNTY.retDbl();
                        TINQTY = TINQTY + INQNTY.retDbl();
                        TOUTQTY = TOUTQTY + OUTQNTY.retDbl();
                        TNOS = TNOS + VE.BARCODEHISTORY[p].NOS.retDbl();
                    }
                    VE.T_INQNTY = TINQTY; VE.T_OUTQNTY = TOUTQTY;VE.T_NOS = TNOS;
                    VE.TOTALIN = TINQTY;VE.TOTALOUT = TOUTQTY;VE.TOTINOUT = (TINQTY - TOUTQTY).retDbl();
                    sql = "select * from " + CommVar.CurSchema(UNQSNO) + ".M_BATCH_IMG_HDR WHERE barno in(" + barnoOrStyle + ")";
                    DataTable dt = masterHelp.SQLquery(sql);
                    VE.UploadBarImages = (from DataRow dr in dt.Rows
                                          select new UploadDOC
                                          {
                                              docID = Path.GetFileNameWithoutExtension(dr["DOC_FLNAME"].retStr()),
                                              DOC_DESC = dr["DOC_DESC"].retStr(),
                                              DOC_FILE = "/UploadDocuments/" + dr["DOC_FLNAME"].retStr(),
                                              DOC_FILE_NAME = dr["DOC_FLNAME"].retStr(),
                                          }).ToList();
                    foreach (var v in VE.UploadBarImages)
                    {
                        string FROMpath = CommVar.SaveFolderPath() + "/ItemImages/" + v.DOC_FILE_NAME;
                        FROMpath = Path.Combine(FROMpath, "");
                        string TOPATH = System.Web.Hosting.HostingEnvironment.MapPath(v.DOC_FILE);
                        Cn.CopyImage(FROMpath, TOPATH);
                        VE.BarImages += Cn.GCS() + v.DOC_FILE_NAME + "~" + v.DOC_DESC;
                    }
                    VE.DefaultView = true;
                    var _barcodehistory = RenderRazorViewToString(ControllerContext, "_REP_BAR_HISTORY", VE);
                    return Content(str + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + _barcodehistory);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
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