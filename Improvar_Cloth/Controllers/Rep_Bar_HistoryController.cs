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
                    ViewBag.formname = "BarCode History";
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    //ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(),  CommVar.CurSchema(UNQSNO));
                    using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
                    {
                        DataTable repformat = Salesfunc.getRepFormat(VE.RepType, VE.DOCCD);

                        VE.DropDown_list_MTRLJOBCD = (from i in DB.M_MTRLJOBMST select new DropDown_list_MTRLJOBCD() { MTRLJOBCD = i.MTRLJOBCD, MTRLJOBNM = i.MTRLJOBNM }).OrderBy(s => s.MTRLJOBNM).ToList();
                        foreach (var v in VE.DropDown_list_MTRLJOBCD)
                        {
                            if (v.MTRLJOBCD == "FS")
                            {
                                v.Checked = true;
                            }
                        }
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
                string MTRLJOBCD = data[0].retSqlformat();
                string DOCDT = System.DateTime.Today.ToString().retDateStr();   /*data[2].retStr()*/
                //string TAXGRPCD = data[3].retStr();
                //string GOCD = data[2].retStr() == "" ? "" : data[4].retStr().retSqlformat();
                //string PRCCD = data[5].retStr();
                if (MTRLJOBCD == "" || barnoOrStyle == "") { MTRLJOBCD = data[1].retStr(); }
                string str = masterHelp.T_TXN_BARNO_help(barnoOrStyle, "ALL", DOCDT, "", "", "", MTRLJOBCD);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    if (str.IndexOf(Cn.GCS()) == -1)
                    { return Content(str = ""); }
                    else {
                        string sql1 = " select distinct a.SLNO,a.AUTONO,a.BARNO,b.DOCNO,b.DOCDT,b.PREFNO,c.DOCNM,b.SLCD,d.SLNM,d.DISTRICT, ";
                        sql1 += "a.STKDRCR,a.QNTY,a.NOS,a.RATE,a.DISCTYPE,A.DISCRATE, ";
                        sql1 += "a.GOCD,e.GONM,f.LOCCD,g.LOCNM,decode(f.loccd, '" + CommVar.Loccd(UNQSNO) + "', e.GONM, g.LOCNM) LOCANM ";
                        sql1 += "from " + scm + ".t_batchdtl a, " + scm + ".t_txn b, " + scm + ".m_doctype c, ";
                        sql1 += "" + scmf + ".m_subleg d, " + scm + ".m_godown e, " + scm + ".t_cntrl_hdr f, " + scmf + ".m_loca g ";
                        sql1 += "where a.AUTONO = b.AUTONO(+) and b.DOCCD = c.DOCCD(+) and b.SLCD = d.SLCD(+) and a.GOCD = e.GOCD(+) and ";
                        sql1 += "f.COMPCD = '" + CommVar.Compcd(UNQSNO) + "' and ";
                        sql1 += "a.AUTONO = f.AUTONO(+) and f.LOCCD = g.LOCCD(+) and A.STKDRCR in ('D','C') and a.BARNO = '" + barnoOrStyle + "' ";
                        sql1 += "order by b.DOCDT,b.DOCNO ";
                        string sql2 = " select a.barno, a.itcd, a.colrcd, a.sizecd, a.prccd, a.effdt, a.rate, b.prcnm from ";
                        sql2 += "(select a.barno, a.itcd, a.colrcd, a.sizecd, a.prccd, a.effdt, a.rate ";
                        sql2 += "from(select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate ";
                        sql2 += "from(select a.barno, a.prccd, a.effdt, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
                        sql2 += "from " + scm + ".m_itemplistdtl a where nvl(a.rate, 0) <> 0 ) a, ";
                        sql2 += "" + scm + ".m_itemplistdtl b, " + scm + ".m_sitem_barcode c where a.barno = b.barno(+) and a.prccd = b.prccd(+) and ";
                        sql2 += "a.effdt = b.effdt(+) and a.barno = c.barno(+) and a.rn = 1 ";
                        sql2 += "union all ";
                        sql2 += "select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate ";
                        sql2 += "from(select a.barno, a.prccd, a.effdt, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
                        sql2 += "from " + scm + ".t_batchmst_price a where nvl(a.rate, 0) <> 0 ) a, ";
                        sql2 += "" + scm + ".t_batchmst_price b, " + scm + ".t_batchmst c, " + scm + ".m_sitem_barcode d where a.barno = b.barno(+) and ";
                        sql2 += "a.prccd = b.prccd(+) and a.effdt = b.effdt(+) and a.barno = c.barno(+) and a.barno = d.barno(+) and d.barno is null) a ";
                        sql2 += ") a, ";
                        sql2 += "" + scmf + ".m_prclst b ";
                        sql2 += "where a.prccd = b.prccd(+) and a.barno = '" + barnoOrStyle + "' ";
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
                                                 DOCNM = dr["DOCNM"].retStr(),
                                                 DISCPER = dr["RATE"].retStr() + " " + dr["DISCTYPE"].retStr()
                                             }).OrderBy(a => a.SLNO).Distinct().ToList();
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
                        VE.T_INQNTY = TINQTY; VE.T_OUTQNTY = TOUTQTY; VE.T_NOS = TNOS;
                        //VE.TOTALIN = TINQTY;VE.TOTALOUT = TOUTQTY;VE.TOTINOUT = (TINQTY - TOUTQTY).retDbl();
                        string tinqty = TINQTY.retStr(); string toutqty = TOUTQTY.retStr();
                        str += "^TOTALIN=^" + tinqty + Cn.GCS();
                        str += "^TOTALOUT=^" + toutqty + Cn.GCS();
                        VE.BARCODEPRICE = (from DataRow dr in itempricedtl.Rows
                                           select new BARCODEPRICE
                                           {
                                               EFFDT = dr["effdt"].retDateStr(),
                                               PRCCD = dr["prccd"].retStr(),
                                               RATE = dr["rate"].retDbl(),
                                           }).ToList();
                        for (int p = 0; p <= VE.BARCODEPRICE.Count - 1; p++)
                        { VE.BARCODEPRICE[p].SLNO = (p + 1).retShort(); }
                        VE.DefaultView = true;
                        var _barcodeprice = RenderRazorViewToString(ControllerContext, "_REP_BAR_HISTORY_PRICE", VE);
                        var _barcodehistory = RenderRazorViewToString(ControllerContext, "_REP_BAR_HISTORY", VE);
                        return Content(str + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + _barcodehistory + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + _barcodeprice);
                    }
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
        [HttpPost]
        public ActionResult Rep_Bar_History(RepBarHistory VE)
        {
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), Scm1 = CommVar.CurSchema(UNQSNO);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            try
            {
                string Excel_Header = "AtnDate" + "|" + "DEPTDESCN" + "|" + "EMPCD" + "|" + "ENM" + "|" + "DESIGNM" + "|" + "Shift" + "|" + "InTime" + "|" + "OutTime" + "|";
                Excel_Header = Excel_Header + "LunchIn" + "|" + "LunchOut" + "|" + "WorkHrs" + "|" + "OTHrs" + "|" + "Attend" + "|" + "Absent" + "|";
                Excel_Header = Excel_Header + "Late" + "|" + "PaidHoliday" + "|" + "Woff" + "|" + "LeaveCode" + "|" + "Leave" + "|" + "Field" + "|" + "Remarks";

               
                    //ExcelPackage ExcelPkg = new ExcelPackage();
                    //ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("Sheet1");

                    //using (ExcelRange Rng = wsSheet1.Cells["A1:U1"])
                    //{
                    //    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //    Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    //    string[] Header = Excel_Header.Split('|');
                    //    for (int i = 0; i < Header.Length; i++)
                    //    {
                    //        wsSheet1.Cells[1, i + 1].Value = Header[i];
                    //    }
                    //}

                    //string E_DATE = System.DateTime.Now.ToString().Substring(0, 10);
                    //string seletype = "", Etyp1 = "";
                    //if (FC.AllKeys.Contains("EMPT")) seletype = FC["EMPT"].ToString().retSqlformat();

                    //string sql = "";
                    //sql = "select a.empcd, a.enm, a.leagacycd, nvl(a.doj,to_date('" + VE.FDT.retDateStr() + "','dd/mm/yyyy')) doj, b.deptcd, e.deptdescn, b.desigcd, d.designm ";
                    //sql += "from " + Scm1 + ".m_empmas a, " + Scm1 + ".m_empmas_det b," + Scm1 + ".m_cntrl_hdr c, ";
                    //sql += CommVar.FinSchema(UNQSNO) + ".m_designation d, " + Scm1 + ".m_dept e ";
                    //sql += "where a.empcd = b.empcd and a.m_autono = c.m_autono and ";
                    //if (VE.Col1.retStr() != "") sql += "a.doj >= to_date('" + VE.Col1.retDateStr() + "','dd/mm/yyyy') and ";
                    //sql += "b.edate = (select max(edate) from " + Scm1 + ".m_empmas_det ";
                    //sql += "where empcd = a.empcd and edate <= to_date('" + E_DATE + "', 'dd/mm/yyyy')) and ";
                    //if (seletype != "") sql += "a.etype in (" + seletype + ") and ";
                    //if (VE.FDT.retStr() != "") sql += "(a.dol is null or (a.dol between to_date('" + VE.FDT.retDateStr() + "','dd/mm/yyyy') and to_date('" + VE.TDT.retDateStr() + "','dd/mm/yyyy'))) and ";
                    //sql += "b.compcd = '" + COM + "' and b.loccd = '" + LOC + "' and b.desigcd=d.desigcd(+) and b.deptcd=e.deptcd(+) ";
                    //sql += "order by deptdescn, doj, enm ";
                    //DataTable dailyattn = Master_Help.SQLquery(sql);

                    //DateTime fdt = Convert.ToDateTime(VE.FDT);
                    //DateTime tdt = Convert.ToDateTime(VE.TDT);
                    //int exlrowno = 2;
                    //if (string.IsNullOrEmpty(VE.FDT.retStr()) && string.IsNullOrEmpty(VE.TDT.retStr()))
                    //{
                    //    for (int i = 0; i < dailyattn.Rows.Count; i++)
                    //    {
                    //        wsSheet1.Cells[i + 2, 2].Value = dailyattn.Rows[i]["deptdescn"];
                    //        wsSheet1.Cells[i + 2, 5].Value = dailyattn.Rows[i]["designm"];
                    //        wsSheet1.Cells[i + 2, 3].Value = dailyattn.Rows[i]["empcd"];
                    //        wsSheet1.Cells[i + 2, 4].Value = dailyattn.Rows[i]["enm"];
                    //    }
                    //}
                    //else
                    //{
                    //    for (DateTime dt = fdt; dt <= tdt;)
                    //    {
                    //        for (int i = 0; i < dailyattn.Rows.Count; i++)
                    //        {
                    //            if (Convert.ToDateTime(dailyattn.Rows[i]["doj"]) <= dt)
                    //            {
                    //                wsSheet1.Cells[exlrowno, 1].Value = dt.retDateStr();
                    //                wsSheet1.Cells[exlrowno, 3].Value = dailyattn.Rows[i]["empcd"];
                    //                wsSheet1.Cells[exlrowno, 4].Value = dailyattn.Rows[i]["enm"];
                    //                wsSheet1.Cells[exlrowno, 2].Value = dailyattn.Rows[i]["deptdescn"];
                    //                wsSheet1.Cells[exlrowno, 5].Value = dailyattn.Rows[i]["designm"];
                    //                exlrowno++;
                    //            }
                    //        }
                    //        wsSheet1.Row(exlrowno).Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    //        dt = dt.AddDays(1);
                    //    }
                    //}
                    //wsSheet1.View.FreezePanes(2, 6);
                    //wsSheet1.Cells[wsSheet1.Dimension.Address].AutoFilter = true;
                    ////for download//
                    //Response.Clear();
                    //Response.ClearContent();
                    //Response.Buffer = true;
                    //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    //Response.AddHeader("Content-Disposition", "attachment; filename=Daily Attn Employee List.xlsx");
                    //Response.BinaryWrite(ExcelPkg.GetAsByteArray());
                    //Response.Flush();
                    //Response.Close();
                    //Response.End();
                    //return Content("<div style='margin-top: 21%; */'><center style='color:green'><h1>Download Successfully...</h1></center></div>");
               
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException + "  ");
            }
            return null;

        }
    }
}