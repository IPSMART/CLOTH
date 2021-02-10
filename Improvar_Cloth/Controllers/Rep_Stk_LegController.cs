using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;

namespace Improvar.Controllers
{
    public class Rep_Stk_LegController : Controller
    {
        public static string[,] headerArray;
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_Stk_Leg
        public ActionResult Rep_Stk_Leg()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Stock Ledger";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    //VE.DropDown_list = (from i in DB.M_GROUP where (selgrpcd.Contains(i.ITGRPCD)) select new DropDown_list() { value = i.ITGRPCD, text = i.ITGRPNM }).OrderBy(s => s.text).ToList();
                    //VE.DropDown_list1 = (from i in DB.M_ITEMPLIST
                    //                     join j in DB.T_BATCHMST on i.ITMPRCCD equals j.ITMPRCCD
                    //                     where (selgrpcd.Contains(i.ITGRPCD))
                    //                     select new DropDown_list1()
                    //                     { value = i.ITMPRCCD, text = i.PRCDESC }).Distinct().OrderBy(s => s.text).ToList();
                    //VE.DropDown_list_text = (from i in items select new DropDown_list_text() { value = i.value, text1 = i.text1 + " [" + i.text3 + "]" }).OrderBy(s => s.text1).ToList();
                    VE.DropDown_list_ITGRP = DropDownHelp.GetItgrpcdforSelection();
                    VE.Itgrpnm = MasterHelp.ComboFill("itgrpcd", VE.DropDown_list_ITGRP, 0, 1);
                    VE.DropDown_list2 = (from i in DBF.M_GODOWN
                                         select new DropDown_list2()
                                         {
                                             value = i.GOCD,
                                             text = i.GONM
                                         }).Distinct().OrderBy(s => s.text).ToList();

                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);

                
                    var locnmlist = (from a in DBF.M_LOCA
                                     select new DropDown_list2()
                                     {
                                         value = a.LOCCD,
                                         text = a.LOCNM
                                     }).Distinct().ToList();
                    VE.Locnm = MasterHelp.ComboFill("loccd", locnmlist, 0, 1);
                    VE.DefaultView = true;
                    VE.FDT = CommVar.FinStartDate(UNQSNO);
                    VE.TDT = CommVar.CurrDate(UNQSNO);
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult LoadItemCode(FormCollection FC, ReportViewinHtml VE, string val)
        {
            try
            {

                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = "Stk_Leg" });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }

        [HttpPost]
        public ActionResult Rep_Stk_Leg(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string fdt = VE.FDT.retDateStr();
                string tdt = VE.TDT.retDateStr();

                string itgrpcd = "";
                string rateqntybag = "B";
                //if (FC["RATEQNTYBAG"].ToString() == "BAGS") rateqntybag = "B";
                //else rateqntybag = "Q";

                string query = ""; string query1 = "";

                string mfdt = "01" + fdt.Substring(2, 8);
                string yfdt = "01/01" + fdt.Substring(5, 5);

                Int32 i = 0;
                Int32 maxR = 0;
                string chkval, chkval1 = "";
                string selitcd = "", plist = "", selgocd = "", LOCCD = "";

                if (FC.AllKeys.Contains("Godown"))
                {
                    selgocd = FC["Godown"].ToString().retSqlformat();
                }

                if (FC.AllKeys.Contains("plist"))
                {
                    plist = CommFunc.retSqlformat(FC["plist"].ToString());
                }

                if (FC.AllKeys.Contains("itcdvalue"))  //'ITEM'
                {
                    selitcd = CommFunc.retSqlformat(FC["itcdvalue"].ToString());
                }
                if (FC.AllKeys.Contains("loccdvalue")) LOCCD = CommFunc.retSqlformat(FC["loccdvalue"].ToString());
                if (FC.AllKeys.Contains("itgrpcdvalue")) itgrpcd = CommFunc.retSqlformat(FC["itgrpcdvalue"].retSqlformat());
                bool showbatch = true;

                string sql = "";
                sql += "select a.autono, a.slno, a.autoslno, a.stkdrcr, a.docno, a.docdt, a.prefno, a.prefdt, a.slnm, a.gstno,a.itcd itcd1 , a.itcd||nvl(c.styleno,' ') itcd, c.styleno, ";
                sql += "c.itnm,c.styleno||' '||c.itnm itstyle,c.uomcd, d.uomnm, a.nos, a.qnty, nvl(a.netamt,0) netamt, b.batchnos from ";
                sql += "(select a.autono, a.slno, a.autono||a.slno autoslno, a.stkdrcr, c.docno, c.docdt, b.prefno, b.prefdt, i.slnm, i.gstno, a.itcd, ";
                sql += " sum(nvl(a.txblval,0)+nvl(a.othramt,0)) netamt, ";
                sql += "sum(nvl(a.nos,0)) nos, sum(nvl(a.qnty,0)) qnty ";
                sql += "from " + scm1 + ".t_txndtl a, " + scm1 + ".t_txn b, " + scm1 + ".t_cntrl_hdr c, ";
                sql += scmf + ".m_subleg i ";
                sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and b.slcd=i.slcd(+) and ";
                sql += "a.stkdrcr in ('D','C') and  nvl(c.cancel,'N') = 'N' and c.compcd='" + COM + "'  and ";
                if (selitcd.retStr() != "") sql += "a.itcd in (" + selitcd + ") and ";
                if (LOCCD != "") { sql += " c.loccd in(" + LOCCD + ") and "; } else { sql += " c.loccd='" + LOC + "' and "; }
                if (plist.retStr() != "")
                {
                    //sql += "k.itmprccd in (" + plist + ") and ";
                }
                if (selgocd.retStr() != "") sql += "b.gocd in (" + selgocd + ") and ";
                sql += "c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') ";
                sql += "group by a.autono, a.slno, a.autono||a.slno, a.stkdrcr, c.docno, c.docdt, b.prefno, b.prefdt, i.slnm, i.gstno, a.itcd ) a, ";

                sql += "( select a.autono, a.slno, a.autono||a.slno autoslno, listagg(b.batchno,',') within group (order by a.autono,a.slno) batchnos ";
                sql += "from " + scm1 + ".t_batchdtl a, " + scm1 + ".t_batchmst b where a.autono=b.autono(+) group by a.autono, a.slno, a.autono||a.slno ) b, ";

                sql += scm1 + ".m_sitem c, " + scmf + ".m_uom d ";
                sql += "where a.autoslno=b.autoslno(+) and a.itcd=c.itcd(+) and c.uomcd=d.uomcd(+) ";
                if (itgrpcd.retStr() != "") sql += "and c.itgrpcd in(" + itgrpcd + ") ";

                sql += "order by itnm, itcd, docdt, stkdrcr desc, autono ";
                DataTable tbl = MasterHelp.SQLquery(sql);

                //string autono = string.Join(",", (from DataRow dr in tbl.Rows where (dr => dr.Field<string>("batchnos").Equals("A"));

                //
                if (tbl.Rows.Count == 0)
                {
                    return RedirectToAction("NoRecords", "RPTViewer");
                }

                DataTable IR = new DataTable("mstrep");

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                string qdsp = "";
                //if (rateqntybag == "B") qdsp = "n,12";
                 qdsp = "n,12,4";

                HC.RepStart(IR, 3);
                HC.GetPrintHeader(IR, "docdt", "string", "d,10:dd/mm/yyyy", "Doc Date");
                HC.GetPrintHeader(IR, "docno", "string", "c,16", "Doc No");
                HC.GetPrintHeader(IR, "prefno", "string", "c,16", "P Blno");
                HC.GetPrintHeader(IR, "slnm", "string", "c,40", "Particulars");
                HC.GetPrintHeader(IR, "qntyin", "double", qdsp, "Qnty (In)");
                HC.GetPrintHeader(IR, "amtin", "double", "n,12,2", "Amt (In)");
                HC.GetPrintHeader(IR, "qntyout", "double", qdsp, "Qnty (Out)");
                HC.GetPrintHeader(IR, "amtout", "double", "n,12,2", "Amt (Out)");
                HC.GetPrintHeader(IR, "balqnty", "double", qdsp, "Bal Qnty");
                //if (showbatch == true) HC.GetPrintHeader(IR, "batchno", "string", "c,30", "Batchnos");

                double iop = 0, idr = 0, icr = 0, icls = 0, idramt = 0, icramt = 0;
                double top = 0, tdr = 0, tcr = 0, tcls = 0, tdramt = 0, tcramt = 0;
                double dbqty = 0, dbamt = 0;

                top = 0; tdr = 0; tcr = 0; tcls = 0;

                Int32 rNo = 0;

                // Report begins
                i = 0; maxR = tbl.Rows.Count - 1;

                while (i <= maxR)
                {
                    chkval = tbl.Rows[i]["itcd"].ToString();
                    iop = 0; idr = 0; icr = 0; icls = 0; idramt = 0; icramt = 0;

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + tbl.Rows[i]["itcd1"].ToString() + "  " + " </span>" + tbl.Rows[i]["itstyle"].ToString() ;
                    IR.Rows[rNo]["Dammy"] = IR.Rows[rNo]["Dammy"] + " </span>" + " [" + tbl.Rows[i]["uomcd"] + "]";
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                    while (tbl.Rows[i]["itcd"].ToString() == chkval)
                    {
                        double bqnty = 0, bnos = 0, bval = 0, bamt = 0;
                        while (tbl.Rows[i]["itcd"].ToString() == chkval && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(fdt))
                        {
                            //if (rateqntybag == "B") dbqty = Convert.ToDouble(tbl.Rows[i]["nos"]);
                             dbqty = Convert.ToDouble(tbl.Rows[i]["qnty"]);

                            if (tbl.Rows[i]["stkdrcr"].ToString() == "D")
                            {
                                bnos = bnos + Convert.ToDouble(tbl.Rows[i]["nos"]);
                                bqnty = bqnty + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                                iop = iop + dbqty;
                            }
                            else if (tbl.Rows[i]["stkdrcr"].ToString() == "C")
                            {
                                bnos = bnos - Convert.ToDouble(tbl.Rows[i]["nos"]);
                                bqnty = bqnty - Convert.ToDouble(tbl.Rows[i]["qnty"]);
                                iop = iop - dbqty;
                            }
                            i = i + 1;
                            if (i > maxR) break;
                        }
                        if (iop != 0)
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["docdt"] = fdt;
                            IR.Rows[rNo]["slnm"] = "Opening";
                            IR.Rows[rNo]["balqnty"] = iop;
                            if (iop < 0) icr = iop * -1;
                            else idr = iop;
                            icls = iop;
                        }
                        if (i > maxR)
                        {
                            break;
                        }
                        while (tbl.Rows[i]["itcd"].ToString() == chkval && Convert.ToDateTime(tbl.Rows[i]["docdt"]) <= Convert.ToDateTime(tdt))
                        {
                            //if (rateqntybag == "B") dbqty = Convert.ToDouble(tbl.Rows[i]["nos"]);
                             dbqty = Convert.ToDouble(tbl.Rows[i]["qnty"]);
                            dbamt = Convert.ToDouble(tbl.Rows[i]["netamt"]);

                            //string pdocno = tbl.Rows[i]["pblno"].ToString();
                            //if (pdocno == "") pdocno = tbl.Rows[i]["docno"].ToString();
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].ToString().Substring(0, 10);
                            IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].ToString();
                            IR.Rows[rNo]["prefno"] = tbl.Rows[i]["prefno"].ToString();
                            IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"] + " [" + tbl.Rows[i]["gstno"] + "]";
                            //if (showbatch == true) IR.Rows[rNo]["batchno"] = tbl.Rows[i]["batchnos"];
                            if (tbl.Rows[i]["stkdrcr"].ToString() == "D")
                            {
                                IR.Rows[rNo]["qntyin"] = dbqty; IR.Rows[rNo]["amtin"] = dbamt;
                                idr = idr + dbqty; idramt = idramt + dbamt;
                            }
                            else
                            {
                                IR.Rows[rNo]["qntyout"] = dbqty; IR.Rows[rNo]["amtout"] = dbamt;
                                icr = icr + dbqty; icramt = icramt + dbamt;
                            }
                            icls = idr - icr;
                            IR.Rows[rNo]["balqnty"] = icls;
                            i = i + 1;
                            if (i > maxR) break;
                        }
                        if (i > maxR) break;
                    }

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["dammy"] = "";
                    IR.Rows[rNo]["slnm"] = "Totals (" + tbl.Rows[i - 1]["styleno"] + tbl.Rows[i - 1]["itnm"] +")";
                    IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                    IR.Rows[rNo]["qntyin"] = idr; IR.Rows[rNo]["amtin"] = idramt;
                    IR.Rows[rNo]["qntyout"] = icr; IR.Rows[rNo]["amtout"] = icramt;
                    IR.Rows[rNo]["balqnty"] = icls;

                    top = top + iop;
                    tdr = tdr + idr; tdramt = tdramt + idramt;
                    tcr = tcr + icr; tdramt = tcramt + icramt;
                    tcls = tcls + icls;
                }

                // Create Blank line
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["dammy"] = " ";
                IR.Rows[rNo]["flag"] = " height:14px; ";

                string pghdr1 = "";
                string repname = "Stk_Leg";
                pghdr1 = "Stock Ledger from " + fdt + " to " + tdt;
                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "L", false);

                TempData[repname] = PV;
                TempData[repname + "xxx"] = IR;
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }

        public ActionResult PrintReport()
        {
            try
            {
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
    }
}
