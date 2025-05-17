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

                        //VE.DropDown_list_MTRLJOBCD = (from i in DB.M_MTRLJOBMST select new DropDown_list_MTRLJOBCD() { MTRLJOBCD = i.MTRLJOBCD, MTRLJOBNM = i.MTRLJOBNM }).OrderBy(s => s.MTRLJOBNM).ToList();
                        VE.DropDown_list_MTRLJOBCD = masterHelp.MTRLJOBCD_List();
                        foreach (var v in VE.DropDown_list_MTRLJOBCD)
                        {
                            if (v.MTRLJOBCD == "FS")
                            {
                                v.Checked = true;
                            }
                        }
                        VE.SHOWMTRLJOBCD = VE.DropDown_list_MTRLJOBCD.Count() > 1 ? "Y" : "N";

                        ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                        VE.DropDown_list1 = (from a in DBF.M_PRCLST
                                             join i in DBF.M_CNTRL_HDR on a.M_AUTONO equals i.M_AUTONO
                                             where (i.INACTIVE_TAG == "N")
                                             select new DropDown_list1()
                                             {
                                                 text = a.PRCNM,
                                                 value = a.PRCCD,
                                             }).Distinct().OrderBy(A => A.text).ToList();
                        VE.listprccd = masterHelp.ComboFill("prccd", VE.DropDown_list1, 0, 1);

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
                string TAXGRPCD = data[3].retStr() == "" ? "C001" : data[3].retStr();
                string GOCD = data[4].retStr() == "" ? "" : data[4].retStr().retSqlformat();
                string PRCCD = data[5].retStr() == "" ? "WP" : data[5].retStr();
                if (MTRLJOBCD == "" || barnoOrStyle == "") { MTRLJOBCD = data[6].retStr(); }
                bool exactbarno = data[7].retStr() == "Bar" ? true : false;
                string bar_no = "";
                if (val.retStr() != "")
                {
                    bar_no = data[8].retStr() == "" ? "" : data[8].retStr().retSqlformat();
                }
                //string str = masterHelp.T_TXN_BARNO_help(barnoOrStyle, "ALL", DOCDT, TAXGRPCD, GOCD, PRCCD, MTRLJOBCD,"", exactbarno);
                string str = masterHelp.T_TXN_BARNO_help(barnoOrStyle, "ALL", DOCDT, TAXGRPCD, GOCD, PRCCD, MTRLJOBCD, "", exactbarno, "", bar_no);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    if (str.IndexOf(Cn.GCS()) == -1)
                    { return Content(str = ""); }
                    else {
                        string barno = str.retCompValue("BARNO").retStr();
                        string sql1 = " select distinct a.SLNO,a.AUTONO,a.BARNO,b.DOCNO,f.docno tchdocno ,b.DOCDT,b.PREFNO,c.DOCNM,b.SLCD,d.SLNM,d.DISTRICT, " + Environment.NewLine;
                        sql1 += "a.STKDRCR,a.QNTY,a.NOS,a.RATE,a.DISCTYPE,A.DISCRATE,nvl(f.cancel,'N')cancel,a.ITREM, k.COLRNM, j.itcd, l.styleno, j.COLRCD, k.colrcd,l.itnm,l.UOMCD, " + Environment.NewLine;
                        sql1 += "a.GOCD,e.GONM,f.LOCCD,g.LOCNM,decode(f.loccd, '" + CommVar.Loccd(UNQSNO) + "', e.GONM, g.LOCNM) LOCANM,f.doccd,h.rtdebcd,i.rtdebnm,a.SCMDISCTYPE,A.SCMDISCRATE " + Environment.NewLine;
                        sql1 += "from " + scm + ".t_batchdtl a, " + scm + ".t_txn b, " + scm + ".m_doctype c, ";
                        sql1 += "" + scmf + ".m_subleg d, " + scmf + ".m_godown e, " + scm + ".t_cntrl_hdr f, " + scmf + ".m_loca g," + scm + ".t_txnmemo h," + scmf + ".M_RETDEB i, " + scm + ".t_batchmst j, " + scm + ".M_COLOR k, " + scm + ".M_SITEM l " + Environment.NewLine;
                        sql1 += "where a.AUTONO = b.AUTONO(+) and b.DOCCD = c.DOCCD(+) and b.SLCD = d.SLCD(+) and a.GOCD = e.GOCD(+) and b.autono = h.autono(+) and h.rtdebcd = i.rtdebcd(+) and a.barno = j.barno(+) and j.COLRCD = k.colrcd(+) and j.itcd = l.itcd(+) and" + Environment.NewLine;
                        sql1 += "f.COMPCD = '" + CommVar.Compcd(UNQSNO) + "' and " + Environment.NewLine;
                        sql1 += "a.AUTONO = f.AUTONO(+) and f.LOCCD = g.LOCCD(+) and f.compcd = g.compcd(+) and (A.STKDRCR in ('D','C') or b.doctag in ('PI')) and upper(a.BARNO) = '" + barno.ToUpper() + "' " + Environment.NewLine;
                        sql1 += "order by b.DOCDT,b.DOCNO " + Environment.NewLine;

                        string sql2 = " select a.barno, a.itcd, a.colrcd, a.sizecd, a.prccd, a.effdt, a.rate, b.prcnm from ";
                        sql2 += "(select a.barno, a.itcd, a.colrcd, a.sizecd, a.prccd, a.effdt, a.rate ";
                        sql2 += "from(select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate ";
                        sql2 += "from(select a.barno, a.prccd, a.effdt, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
                        sql2 += "from " + scm + ".T_BATCHMST_PRICE a where nvl(a.rate, 0) <> 0 ) a, ";
                        sql2 += "" + scm + ".T_BATCHMST_PRICE b, " + scm + ".T_BATCHmst c where a.barno = b.barno(+) and a.prccd = b.prccd(+) and ";
                        sql2 += "a.effdt = b.effdt(+) and a.barno = c.barno(+) and a.rn = 1 ";
                        sql2 += "union all ";
                        sql2 += "select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate ";
                        sql2 += "from(select a.barno, a.prccd, a.effdt, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
                        sql2 += "from " + scm + ".t_batchmst_price a where nvl(a.rate, 0) <> 0 ) a, ";
                        sql2 += "" + scm + ".t_batchmst_price b, " + scm + ".t_batchmst c, " + scm + ".T_BATCHmst d where a.barno = b.barno(+) and ";
                        sql2 += "a.prccd = b.prccd(+) and a.effdt = b.effdt(+) and a.barno = c.barno(+) and a.barno = d.barno(+) and d.barno is null) a ";
                        sql2 += ") a, ";
                        sql2 += "" + scmf + ".m_prclst b ";
                        sql2 += "where a.prccd = b.prccd(+) and upper(a.barno) = '" + barno.ToUpper() + "' ";
                        DataTable tbatchdtl = masterHelp.SQLquery(sql1);
                        DataTable itempricedtl = masterHelp.SQLquery(sql2);
                        VE.BARCODEHISTORY = (from DataRow dr in tbatchdtl.Rows
                                             select new BARCODEHISTORY
                                             {
                                                 SLNO = dr["SLNO"].retShort(),
                                                 AUTONO = dr["AUTONO"].retStr(),
                                                 DOCDT = dr["DOCDT"].retDateStr(),
                                                 DOCNO = dr["cancel"].retStr() == "N" ? dr["tchdocno"].retStr() : dr["tchdocno"].retStr() + " (Record Cancelled)",
                                                 PREFNO = dr["PREFNO"].retStr(),
                                                 SLNM = dr["doccd"].retStr() == "SCM" ? dr["RTDEBCD"].retStr() == "" ? "" : dr["RTDEBNM"].retStr() + "[" + dr["RTDEBCD"].retStr() + "]" : dr["SLCD"].retStr() == "" ? "" : dr["SLNM"].retStr() + "[" + dr["SLCD"].retStr() + "]" + "[" + dr["DISTRICT"].retStr() + "]",
                                                 LOCNM = dr["LOCANM"].retStr(),
                                                 //NOS = dr["cancel"].retStr() == "N" ? dr["NOS"].retDbl() : 0,
                                                 NOS = (dr["cancel"].retStr() == "N" && (dr["STKDRCR"].retStr() == "D" || dr["STKDRCR"].retStr() == "C")) ? dr["NOS"].retDbl() : 0,
                                                 RATE = dr["cancel"].retStr() == "N" ? dr["RATE"].retDbl() : 0,
                                                 STKDRCR = dr["STKDRCR"].retStr(),
                                                 QNTY = dr["cancel"].retStr() == "N" ? dr["QNTY"].retDbl() : 0,
                                                 DOCNM = dr["DOCNM"].retStr(),
                                                 DISCPER = dr["cancel"].retStr() == "N" ? (dr["SCMDISCRATE"].retDbl() == 0 ? "" : dr["SCMDISCRATE"].retDbl() + " " + dr["SCMDISCTYPE"].retStr()) : 0.retStr(),
                                                 INQNTY = dr["cancel"].retStr() == "N" ? (dr["STKDRCR"].retStr() == "D" ? dr["QNTY"].retDbl() : "".retDbl()) : 0,
                                                 OUTQNTY = dr["cancel"].retStr() == "N" ? (dr["STKDRCR"].retStr() == "C" ? dr["QNTY"].retDbl() : "".retDbl()) : 0,
                                                 itremarks = dr["ITREM"].retStr(),
                                                 colornm = dr["COLRNM"].retStr(),
                                                 itstyleno = dr["styleno"].retStr() + " " + dr["itnm"].retStr(),
                                                 UOM = dr["UOMCD"].retStr()
                                             }).Distinct().ToList();
                        //}).OrderBy(a => a.SLNO).Distinct().ToList();

                        double TINQTY = 0, TOUTQTY = 0, TNOS = 0;
                        for (int p = 0; p <= VE.BARCODEHISTORY.Count - 1; p++)
                        {
                            //var INQNTY = (from i in VE.BARCODEHISTORY
                            //              where i.STKDRCR == "D"
                            //              select i.QNTY).FirstOrDefault();
                            //var OUTQNTY = (from i in VE.BARCODEHISTORY
                            //               where i.STKDRCR == "C"
                            //               select i.QNTY).FirstOrDefault();
                            //VE.BARCODEHISTORY[p].INQNTY = INQNTY.retDbl();
                            //VE.BARCODEHISTORY[p].OUTQNTY = OUTQNTY.retDbl();
                            TINQTY = TINQTY + VE.BARCODEHISTORY[p].INQNTY.retDbl();
                            TOUTQTY = TOUTQTY + VE.BARCODEHISTORY[p].OUTQNTY.retDbl();
                            TNOS = TNOS + VE.BARCODEHISTORY[p].NOS.retDbl();
                        }
                        VE.T_UomTotalIn = "In - " + string.Join(", ", (from x in VE.BARCODEHISTORY
                                                                       where x.UOM.retStr() != ""
                                                                       group x by new
                                                                       {
                                                                           x.UOM
                                                                       } into P
                                                                       select P.Key.UOM.retStr() + " : " + P.Sum(A => A.INQNTY.retDbl()).retDbl()).ToList());
                        VE.T_UomTotalOut = "Out - " + string.Join(", ", (from x in VE.BARCODEHISTORY
                                                                         where x.UOM.retStr() != ""
                                                                         group x by new
                                                                         {
                                                                             x.UOM
                                                                         } into P
                                                                         select P.Key.UOM.retStr() + " : " + P.Sum(A => A.OUTQNTY.retDbl()).retDbl()).ToList());
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
        [HttpPost]
        public ActionResult Rep_Bar_History(RepBarHistory VE, FormCollection FC, string submitbutton)
        {
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), Scmf = CommVar.FinSchema(UNQSNO);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            try
            {
                if (submitbutton == "Download Excel")
                {
                    #region BarCode Details
                    string Excel_Header = "SL No" + "|" + "Doc Date" + "|" + "Doc No" + "|" + "Party Ref" + "|" + "Doc Type" + "|" + "Party" + "|" + "Location" + "|" + "In" + "|";
                    Excel_Header = Excel_Header + "Out" + "|" + "Other Qnty" + "|" + "Rate" + "|" + "Disc %";


                    ExcelPackage ExcelPkg = new ExcelPackage();
                    ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("Sheet1");

                    using (ExcelRange Rng = wsSheet1.Cells["A1:L1"])
                    {
                        Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        string[] Header = Excel_Header.Split('|');
                        for (int i = 0; i < Header.Length; i++)
                        {
                            wsSheet1.Cells[1, i + 1].Value = Header[i];
                        }
                    }

                    string E_DATE = System.DateTime.Now.ToString().Substring(0, 10);
                    string seletype = "", Etyp1 = "";


                    string sql1 = " select distinct a.SLNO,a.AUTONO,a.BARNO,b.DOCNO,b.DOCDT,b.PREFNO,c.DOCNM,b.SLCD,d.SLNM,d.DISTRICT, ";
                    sql1 += "a.STKDRCR,a.QNTY,a.NOS,a.RATE,a.DISCTYPE,A.DISCRATE, ";
                    sql1 += "a.GOCD,e.GONM,f.LOCCD,g.LOCNM,decode(f.loccd, '" + CommVar.Loccd(UNQSNO) + "', e.GONM, g.LOCNM) LOCANM,nvl(f.cancel,'N')cancel ";
                    sql1 += "from " + scm + ".t_batchdtl a, " + scm + ".t_txn b, " + scm + ".m_doctype c, ";
                    sql1 += "" + scmf + ".m_subleg d, " + scmf + ".m_godown e, " + scm + ".t_cntrl_hdr f, " + scmf + ".m_loca g ";
                    sql1 += "where a.AUTONO = b.AUTONO(+) and b.DOCCD = c.DOCCD(+) and b.SLCD = d.SLCD(+) and a.GOCD = e.GOCD(+) and ";
                    sql1 += "f.COMPCD = '" + CommVar.Compcd(UNQSNO) + "' and ";
                    sql1 += "a.AUTONO = f.AUTONO(+) and f.compcd = g.compcd(+) and f.LOCCD = g.LOCCD(+) and A.STKDRCR in ('D','C') and a.BARNO = '" + VE.BAR_CODE + "' ";
                    sql1 += "order by b.DOCDT,b.DOCNO ";
                    DataTable barcdhistory = masterHelp.SQLquery(sql1);

                    int exlrowno = 2; double TINQTY = 0, TOUTQTY = 0, TNOS = 0, InQty = 0, OutQty = 0;
                    for (int i = 0; i < barcdhistory.Rows.Count; i++)
                    {
                        wsSheet1.Cells[i + 2, 1].Value = barcdhistory.Rows[i]["SLNO"].retShort();
                        wsSheet1.Cells[i + 2, 2].Value = barcdhistory.Rows[i]["DOCDT"].retDateStr();
                        wsSheet1.Cells[i + 2, 3].Value = barcdhistory.Rows[i]["cancel"].retStr() == "N" ? barcdhistory.Rows[i]["DOCNO"].retStr() : barcdhistory.Rows[i]["DOCNO"].retStr() + " (Record Cancelled)";
                        wsSheet1.Cells[i + 2, 4].Value = barcdhistory.Rows[i]["PREFNO"].retStr();
                        wsSheet1.Cells[i + 2, 5].Value = barcdhistory.Rows[i]["DOCNM"].retStr();
                        wsSheet1.Cells[i + 2, 6].Value = barcdhistory.Rows[i]["SLCD"].retStr() == "" ? "" : barcdhistory.Rows[i]["SLNM"].retStr() + "[" + barcdhistory.Rows[i]["SLCD"].retStr() + "]" + "[" + barcdhistory.Rows[i]["DISTRICT"].retStr() + "]";
                        wsSheet1.Cells[i + 2, 7].Value = barcdhistory.Rows[i]["LOCANM"].retStr();
                        //var inqty = (from DataRow dr in barcdhistory.Rows where dr["STKDRCR"].retStr() == "D" select new { QNTY = dr["QNTY"].retDbl() }).FirstOrDefault();
                        //var outqty = (from DataRow dr in barcdhistory.Rows where dr["STKDRCR"].retStr() == "C" select new { QNTY = dr["QNTY"].retDbl() }).FirstOrDefault();
                        InQty = barcdhistory.Rows[i]["cancel"].retStr() == "N" ? (barcdhistory.Rows[i]["STKDRCR"].retStr() == "D" ? barcdhistory.Rows[i]["QNTY"].retDbl() : 0) : 0;
                        OutQty = barcdhistory.Rows[i]["cancel"].retStr() == "N" ? (barcdhistory.Rows[i]["STKDRCR"].retStr() == "C" ? barcdhistory.Rows[i]["QNTY"].retDbl() : 0) : 0;
                        //if (inqty != null)
                        //{
                        //    InQty = inqty.QNTY.retDbl();
                        //}
                        //else { InQty = 0; }
                        wsSheet1.Cells[i + 2, 8].Value = InQty;
                        //if (outqty != null)
                        //{
                        //    OutQty = outqty.QNTY.retDbl();
                        //}
                        //else { OutQty = 0; }
                        wsSheet1.Cells[i + 2, 9].Value = OutQty;
                        wsSheet1.Cells[i + 2, 10].Value = barcdhistory.Rows[i]["cancel"].retStr() == "N" ? barcdhistory.Rows[i]["NOS"].retDbl() : 0;
                        wsSheet1.Cells[i + 2, 11].Value = barcdhistory.Rows[i]["cancel"].retStr() == "N" ? barcdhistory.Rows[i]["RATE"].retDbl() : 0;
                        wsSheet1.Cells[i + 2, 12].Value = barcdhistory.Rows[i]["cancel"].retStr() == "N" ? (barcdhistory.Rows[i]["DISCRATE"].retDbl() == 0 ? "" : barcdhistory.Rows[i]["DISCRATE"].retDbl() + " " + barcdhistory.Rows[i]["DISCTYPE"].retStr()) : 0.retStr();
                        TINQTY = TINQTY + InQty;
                        TOUTQTY = TOUTQTY + OutQty;
                        TNOS = TNOS + barcdhistory.Rows[i]["cancel"].retStr() == "N" ? barcdhistory.Rows[i]["NOS"].retDbl() : 0;
                        exlrowno++;
                    }
                    wsSheet1.Row(exlrowno).Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    wsSheet1.Row(exlrowno).Style.Font.Bold = true;
                    wsSheet1.Cells[exlrowno, 7].Value = "TOTAL";
                    wsSheet1.Cells[exlrowno, 8].Value = TINQTY;
                    wsSheet1.Cells[exlrowno, 9].Value = TOUTQTY;

                    wsSheet1.Row(++exlrowno).Style.Font.Bold = true;
                    wsSheet1.Cells[exlrowno, 7].Value = "Balance Qnty";
                    wsSheet1.Cells[exlrowno, 9].Value = (TINQTY - TOUTQTY);
                    //wsSheet1.Cells[wsSheet1.Dimension.Address].AutoFilter = true;
                    //for download//
                    Response.Clear();
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("Content-Disposition", "attachment; filename=BarCode History.xlsx");
                    Response.BinaryWrite(ExcelPkg.GetAsByteArray());
                    Response.Flush();
                    Response.Close();
                    Response.End();
                    #endregion
                }
                else if (submitbutton == "Download Price Details Excel")
                {
                    string selprccd = "";
                    if (FC.AllKeys.Contains("prccdvalue")) selprccd = CommFunc.retSqlformat(FC["prccdvalue"].ToString());
                    if (selprccd == "")
                    {
                        return Content("Select Price code");
                    }
                    string[] arrprccd = FC["prccdvalue"].ToString().Split(',');
                    #region Price Details
                    int col = 3;
                    string Excel_Header = "SL No" + "|" + "Bar Code" + "|" + "Design No./Item";
                    foreach (var v in arrprccd)
                    {
                        Excel_Header += "|" + v + " Rate";
                        col++;
                    }

                    ExcelPackage ExcelPkg = new ExcelPackage();
                    ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("Sheet1");

                    using (ExcelRange Rng = wsSheet1.Cells[1, 1, 1, col])
                    {
                        Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        string[] Header = Excel_Header.Split('|');
                        for (int i = 0; i < Header.Length; i++)
                        {
                            wsSheet1.Cells[1, i + 1].Value = Header[i];
                        }
                    }



                    string sql2 = " select a.barno, a.itcd, a.colrcd, a.sizecd, a.prccd, a.effdt, a.rate, b.prcnm, c.styleno||' '||c.itnm itstyle from ";
                    sql2 += "(select a.barno, a.itcd, a.colrcd, a.sizecd, a.prccd, a.effdt, a.rate ";
                    sql2 += "from(select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate ";
                    sql2 += "from(select a.barno, a.prccd, a.effdt, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
                    sql2 += "from " + scm + ".T_BATCHMST_PRICE a where nvl(a.rate, 0) <> 0 ) a, ";
                    sql2 += "" + scm + ".T_BATCHMST_PRICE b, " + scm + ".T_BATCHmst c where a.barno = b.barno(+) and a.prccd = b.prccd(+) and ";
                    sql2 += "a.effdt = b.effdt(+) and a.barno = c.barno(+) and a.rn = 1 ";
                    sql2 += "union all ";
                    sql2 += "select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate ";
                    sql2 += "from(select a.barno, a.prccd, a.effdt, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
                    sql2 += "from " + scm + ".t_batchmst_price a where nvl(a.rate, 0) <> 0 ) a, ";
                    sql2 += "" + scm + ".t_batchmst_price b, " + scm + ".t_batchmst c, " + scm + ".T_BATCHmst d where a.barno = b.barno(+) and ";
                    sql2 += "a.prccd = b.prccd(+) and a.effdt = b.effdt(+) and a.barno = c.barno(+) and a.barno = d.barno(+) and d.barno is null) a ";
                    sql2 += ") a, ";
                    sql2 += "" + scmf + ".m_prclst b," + scm + ".m_sitem c ";
                    sql2 += "where a.prccd = b.prccd(+) and a.itcd=c.itcd(+) and a.prccd in (" + selprccd + ")  ";
                    DataTable barcdhistory = masterHelp.SQLquery(sql2);
                    DataTable dt = new DataTable();
                    DataView dv = new DataView(barcdhistory);
                    string[] arr = { "barno", "itstyle" };
                    dt = dv.ToTable(true, arr);
                    int exlrowno = 2;
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        wsSheet1.Cells[i + 2, 1].Value = (i + 1);

                        wsSheet1.Cells[i + 2, 2].Value = dt.Rows[i]["barno"].retStr();
                        wsSheet1.Cells[i + 2, 3].Value = dt.Rows[i]["itstyle"].retStr();
                        string barno = dt.Rows[i]["barno"].retStr();
                        col = 4;
                        foreach (var v in arrprccd)
                        {
                            wsSheet1.Cells[i + 2, col].Value = (from DataRow dr in barcdhistory.Rows where dr["barno"].retStr() == barno && dr["prccd"].retStr() == v select dr["rate"].retDbl()).FirstOrDefault();
                            col++;
                        }


                        exlrowno++;
                    }

                    Response.Clear();
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("Content-Disposition", "attachment; filename=BarCode Rate Details.xlsx");
                    Response.BinaryWrite(ExcelPkg.GetAsByteArray());
                    Response.Flush();
                    Response.Close();
                    Response.End();
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException + "  ");
            }
            return null;
        }
        public ActionResult Update_Price(RepBarHistory VE)
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
                    var CLCD = CommVar.ClientCode(UNQSNO);
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    var T_BATCHMST_cnt = DB1.T_BATCHMST.Where(x => x.BARNO == VE.BAR_CODE).Select(a => a.AUTONO).Distinct().Count();

                    for (int i = 0; i <= VE.BARCODEPRICE.Count - 1; i++)
                    {
                        sql = "update " + schnm + ".T_BATCHMST_PRICE set RATE =" + VE.BARCODEPRICE[i].RATE + " "
                                 + " where BARNO='" + VE.BAR_CODE + "' and PRCCD='" + VE.BARCODEPRICE[i].PRCCD.retStr() + "' and EFFDT=to_date('" + VE.BARCODEPRICE[i].EFFDT.retStr() + "','dd/mm/yyyy')  ";
                        OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                        if (T_BATCHMST_cnt == 1)
                        {
                            if (VE.BARCODEPRICE[i].PRCCD.retStr() == "CP")
                            {
                                sql = "update " + schnm + ". T_BATCHMST set RATE='" + VE.BARCODEPRICE[i].RATE + "' "
                                    + " where BARNO='" + VE.BAR_CODE + "'   ";
                                OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                            }
                            else if (VE.BARCODEPRICE[i].PRCCD.retStr() == "WP")
                            {
                                sql = "update " + schnm + ". T_BATCHMST set WPRATE='" + VE.BARCODEPRICE[i].RATE + "' "
                                    + " where BARNO='" + VE.BAR_CODE + "'   ";
                                OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                            }
                            else if (VE.BARCODEPRICE[i].PRCCD.retStr() == "RP")
                            {
                                sql = "update " + schnm + ". T_BATCHMST set RPRATE='" + VE.BARCODEPRICE[i].RATE + "' "
                                    + " where BARNO='" + VE.BAR_CODE + "'   ";
                                OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();
                            }
                        }


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
        public ActionResult GetItemDetails(string val, string Code)
        {
            try
            {
                var str = masterHelp.ITCD_help(val, "");
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
        public ActionResult SaveNewBarno(RepBarHistory VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            string dbsql = "";
            string[] dbsql1;
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                OraCmd.Transaction = OraTrans;
                try
                {
                    var chk = DB.T_BATCHMST.Where(a => a.BARNO.ToUpper() == VE.NEWBARDATA.BARNO.ToUpper()).Select(a => a.BARNO).Distinct().ToList();
                    if (chk.Count > 0)
                    {
                        OraTrans.Rollback();
                        return Content("Barno already exist");
                    }
                    string ContentFlg = "";

                    T_BATCHMST TBATCHMST = new T_BATCHMST();
                    TBATCHMST.EMD_NO = 0;
                    TBATCHMST.CLCD = CommVar.ClientCode(UNQSNO);
                    TBATCHMST.BARNO = VE.NEWBARDATA.BARNO.retStr().ToUpper();
                    TBATCHMST.ITCD = VE.NEWBARDATA.ITCD.retStr();
                    TBATCHMST.RATE = VE.NEWBARDATA.CPRATE.retDbl();
                    TBATCHMST.WPRATE = VE.NEWBARDATA.WPRATE.retDbl();
                    TBATCHMST.RPRATE = VE.NEWBARDATA.RPRATE.retDbl();
                    TBATCHMST.COMMONUNIQBAR = "E";
                    TBATCHMST.PDESIGN = VE.NEWBARDATA.PDESIGN.retStr();

                    dbsql = masterHelp.RetModeltoSql(TBATCHMST, "A");
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    for (int i = 0; i <= 2; i++)
                    {
                        string PRCCD = i == 0 ? "CP" : i == 1 ? "WP" : "RP";
                        double RATE = i == 0 ? VE.NEWBARDATA.CPRATE.retDbl() : i == 1 ? VE.NEWBARDATA.WPRATE.retDbl() : VE.NEWBARDATA.RPRATE.retDbl();

                        T_BATCHMST_PRICE TBATCHMSTPRICE = new T_BATCHMST_PRICE();
                        TBATCHMSTPRICE.EMD_NO = TBATCHMST.EMD_NO;
                        TBATCHMSTPRICE.CLCD = TBATCHMST.CLCD;
                        TBATCHMSTPRICE.EFFDT = VE.NEWBARDATA.EFFDT != null ? Convert.ToDateTime(VE.NEWBARDATA.EFFDT) : System.DateTime.Now.Date;
                        TBATCHMSTPRICE.BARNO = TBATCHMST.BARNO.retStr();
                        TBATCHMSTPRICE.PRCCD = PRCCD;
                        TBATCHMSTPRICE.RATE = RATE;
                        dbsql = masterHelp.RetModeltoSql(TBATCHMSTPRICE, "A");
                        dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
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
        public ActionResult Update_PDesign(RepBarHistory VE)
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

                    sql = "update " + schnm + ". T_BATCHMST set PDESIGN='" + VE.PDESIGN + "' "
                                  + " where BARNO='" + VE.BAR_CODE + "'   ";
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
        public ActionResult SaveNewPrice(RepBarHistory VE)
        {
            Cn.getQueryString(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            string dbsql = "";
            string[] dbsql1;
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                OraCmd.Transaction = OraTrans;
                try
                {

                    string ContentFlg = "";

                    for (int i = 0; i <= 2; i++)
                    {
                        string PRCCD = i == 0 ? "CP" : i == 1 ? "WP" : "RP";
                        double RATE = i == 0 ? VE.NEWPRICEDATA.CPRATE.retDbl() : i == 1 ? VE.NEWPRICEDATA.WPRATE.retDbl() : VE.NEWPRICEDATA.RPRATE.retDbl();
                        if (RATE != 0)
                        {
                            var chk = DB.T_BATCHMST_PRICE.Where(a => a.BARNO.ToUpper() == VE.BAR_CODE.ToUpper() && a.EFFDT == VE.NEWPRICEDATA.EFFDT && a.PRCCD == PRCCD && a.RATE == RATE).Select(a => a.BARNO).Distinct().ToList();
                            if (chk.Count > 0)
                            {
                                OraTrans.Rollback();
                                return Content("Effective Date : " + VE.NEWPRICEDATA.EFFDT + " ,Price Code : " + PRCCD + " already exist");
                            }

                            T_BATCHMST_PRICE TBATCHMSTPRICE = new T_BATCHMST_PRICE();
                            TBATCHMSTPRICE.EMD_NO = 0;
                            TBATCHMSTPRICE.CLCD = CommVar.ClientCode(UNQSNO);
                            TBATCHMSTPRICE.EFFDT = VE.NEWPRICEDATA.EFFDT != null ? Convert.ToDateTime(VE.NEWPRICEDATA.EFFDT) : System.DateTime.Now.Date;
                            TBATCHMSTPRICE.BARNO = VE.BAR_CODE.retStr();
                            TBATCHMSTPRICE.PRCCD = PRCCD;
                            TBATCHMSTPRICE.RATE = RATE;
                            dbsql = masterHelp.RetModeltoSql(TBATCHMSTPRICE, "A");
                            dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                        }
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
}