using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class Rep_SupplierController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        DropDownHelp DropDownHelp = new DropDownHelp();
        // GET: Rep_Supplier
        public ActionResult Rep_Supplier()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ViewBag.formname = "Rep_Supplier";
                    switch (VE.MENU_PARA)
                    {
                        case "Q":
                            ViewBag.formname = "Supplier Wise Report"; break;
                        case "A":
                            ViewBag.formname = "Supplier Wise Report w/Value"; break;
                        default: ViewBag.formname = ""; break;
                    }
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    string com = CommVar.Compcd(UNQSNO);

                    VE.FDT = CommVar.FinStartDate(UNQSNO);
                    VE.TDT = CommVar.CurrDate(UNQSNO);

                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("");
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);

                    VE.DropDown_list_ITGRP = DropDownHelp.GetItgrpcdforSelection();
                    VE.Itgrpnm = MasterHelp.ComboFill("itgrpcd", VE.DropDown_list_ITGRP, 0, 1);

                    VE.DropDown_list3 = (from i in DBF.M_LOCA
                                         where i.COMPCD == com
                                         select new DropDown_list3() { value = i.LOCCD, text = i.LOCNM }).Distinct().OrderBy(s => s.text).ToList();// location
                    VE.TEXTBOX5 = MasterHelp.ComboFill("loccd", VE.DropDown_list3, 0, 1);

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
        [HttpPost]
        public ActionResult Rep_Supplier(FormCollection FC, ReportViewinHtml VE)
        {
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
            string fdt = VE.FDT.retStr();
            string tdt = VE.TDT.retStr();
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO);

            string sql = "select e.slcd, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd, a.doctag, a.qnty, a.txblval, a.othramt, f.itgrpcd, h.itgrpnm, f.itnm, ";
            sql += "nvl(e.pdesign, f.styleno) styleno, e.othrate, nvl(b.rate, 0) oprate, nvl(c.rate, 0) clrate, ";
            sql += "f.uomcd, i.uomnm, i.decimals, g.itnm fabitnm  from ";
            sql += " ";
            sql += "(select d.compcd, d.loccd, a.barno, 'OP' doctag, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, ";
            sql += "sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval, ";
            sql += "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt ";
            sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e ";
            sql += "where a.barno = b.barno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and ";
            sql += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D', 'C') and ";
            sql += "d.docdt < to_date('" + fdt + "', 'dd/mm/yyyy') ";
            sql += "group by d.compcd, d.loccd, a.barno, 'OP' ";
            sql += "union all ";
            sql += "select d.compcd, d.loccd, a.barno, c.doctag, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, ";
            sql += "sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval,  ";
            sql += "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt ";
            sql += "    from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e ";
            sql += "where a.barno = b.barno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and ";
            sql += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N')= 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D','C') and ";
            sql += "d.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and d.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') ";
            sql += "group by d.compcd, d.loccd, a.barno, c.doctag ) a, ";
            sql += " ";
            sql += "(select barno, effdt, prccd, rate from ( ";
            sql += "select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += "from " + scm + ".t_batchmst_price a ";
            sql += "where a.effdt <= to_date('" + fdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) b, ";
            sql += " ";
            sql += "(select barno, effdt, prccd, rate from ( ";
            sql += "select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += "from " + scm + ".t_batchmst_price a ";
            sql += "where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) c, ";
            sql += " ";
            sql += "" + scm + ".t_batchmst e, " + scm + ".m_sitem f, " + scm + ".m_sitem g, " + scm + ".m_group h, " + scmf + ".m_uom i, ";
            sql += "" + scmf + ".m_subleg j, " + scmf + ".m_loca k ";
            sql += "where a.barno = e.barno(+) and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and ";
            sql += "a.barno = b.barno(+) and a.barno = c.barno(+) and e.slcd = j.slcd(+) and a.compcd || a.loccd = k.compcd || k.loccd and ";
            sql += "f.itgrpcd = h.itgrpcd(+) and f.uomcd = i.uomcd(+) ";
            sql += "order by itgrpnm, itgrpcd, fabitnm, fabitcd, itnm, itcd, styleno, barno ";


            DataTable tbl = MasterHelp.SQLquery(sql);
            if (tbl.Rows.Count == 0) return Content("no records..");


            Int32 rNo = 0, maxR = 0, maxB = 0, i = 0;
            maxR = tbl.Rows.Count - 1;
            Int32 islno = 0;

            Models.PrintViewer PV = new Models.PrintViewer();
            HtmlConverter HC = new HtmlConverter();
            DataTable IR = new DataTable("");

            HC.RepStart(IR, 3);
            HC.GetPrintHeader(IR, "slno", "long", "n,4", "Slno");
            HC.GetPrintHeader(IR, "styleno", "string", "c,40", "Styleno");
            HC.GetPrintHeader(IR, "pdesign", "string", "c,40", "Party Design");
            HC.GetPrintHeader(IR, "uomnm", "string", "c,5", "uom");
            HC.GetPrintHeader(IR, "opening", "double", "n,10,2", "Opening");
            HC.GetPrintHeader(IR, "purchase", "double", "n,10,2", "Purchase");
            HC.GetPrintHeader(IR, "purret", "double", "n,10,2", "Pur.Ret");
            HC.GetPrintHeader(IR, "transferin", "double", "n,10,2", "Transfer In");
            HC.GetPrintHeader(IR, "transferout", "double", "n,10,2", "Transfer Out");
            HC.GetPrintHeader(IR, "sales", "double", "n,10,2", "Sales");
            HC.GetPrintHeader(IR, "others", "double", "n,10,2", "Others");
            HC.GetPrintHeader(IR, "stock", "double", "n,10,2", "Stock");
            IR.Columns.Add("itgrpcd", typeof(string), "");
            IR.Columns.Add("slcd", typeof(string), "");

            maxB = tbl.Rows.Count - 1;
            i = 0;
            string itgrpcd = "";
            while (i <= maxB)
            {
                string slcd = tbl.Rows[i]["slcd"].retStr();

                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + slcd + "  " + " </span>" + tbl.Rows[i]["slnm"].ToString();
                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";

                while (tbl.Rows[i]["slcd"].retStr() == slcd)
                {
                    itgrpcd = tbl.Rows[i]["itgrpcd"].retStr();

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + itgrpcd + "  " + " </span>" + tbl.Rows[i]["itgrpnm"].ToString();
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";

                    while (tbl.Rows[i]["slcd"].retStr() == slcd && tbl.Rows[i]["itgrpcd"].retStr() == itgrpcd)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        islno++;
                        IR.Rows[rNo]["slno"] = islno;
                        IR.Rows[rNo]["styleno"] = tbl.Rows[i]["styleno"].ToString();
                        IR.Rows[rNo]["pdesign"] = tbl.Rows[i]["pdesign"].ToString();
                        IR.Rows[rNo]["uomnm"] = tbl.Rows[i]["uomnm"].ToString();
                        IR.Rows[rNo]["itgrpcd"] = tbl.Rows[i]["itgrpcd"].retDbl();
                        IR.Rows[rNo]["slcd"] = tbl.Rows[i]["slcd"].retDbl();

                        IR.Rows[rNo]["opening"] = tbl.Rows[i]["opening"].retDbl();
                        IR.Rows[rNo]["purchase"] = tbl.Rows[i]["purchase"].retDbl();
                        IR.Rows[rNo]["purret"] = tbl.Rows[i]["purret"].retDbl();
                        IR.Rows[rNo]["transferin"] = tbl.Rows[i]["transferin"].retDbl();
                        IR.Rows[rNo]["transferout"] = tbl.Rows[i]["transferout"].retDbl();
                        IR.Rows[rNo]["sales"] = tbl.Rows[i]["sales"].retDbl();
                        IR.Rows[rNo]["others"] = tbl.Rows[i]["others"].retDbl();
                        IR.Rows[rNo]["stock"] = tbl.Rows[i]["stock"].retDbl();


                        i++;
                        if (i > maxB) break;
                    }
                    if (i > maxB) break;
                }
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["itnm"] = "Total of " + tbl.Rows[i - 1]["itgrpnm"].ToString();
                IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;";

                itgrpcd = tbl.Rows[i - 1]["itgrpcd"].ToString();
                var unitwisegrptotal = IR.AsEnumerable().Where(g => g.Field<string>("uomnm").retStr() != "" && g.Field<string>("itgrpcd").retStr() == itgrpcd)
                            .GroupBy(g => g.Field<string>("uomnm"))
                            .Select(g =>
                            {
                                var row = IR.NewRow();
                                row["uomnm"] = g.Key;
                                row["opqty"] = g.Sum(r => r.Field<double?>("opqty") == null ? 0 : r.Field<double>("opqty"));
                                row["opval"] = g.Sum(r => r.Field<double?>("opval") == null ? 0 : r.Field<double>("opval"));
                                row["netpur"] = g.Sum(r => r.Field<double?>("netpur").retDbl());
                                row["purval"] = g.Sum(r => r.Field<double?>("purval").retDbl());
                                row["karqty"] = g.Sum(r => r.Field<double?>("karqty").retDbl());
                                row["karval"] = g.Sum(r => r.Field<double?>("karval").retDbl());
                                row["approval"] = g.Sum(r => r.Field<double?>("approval").retDbl());
                                row["netstktrans"] = g.Sum(r => r.Field<double?>("netstktrans").retDbl());
                                row["netadj"] = g.Sum(r => r.Field<double?>("netadj").retDbl());
                                row["netsale"] = g.Sum(r => r.Field<double?>("netsale").retDbl());
                                row["salevalue"] = g.Sum(r => r.Field<double?>("salevalue").retDbl());
                                row["balqty"] = g.Sum(r => r.Field<double?>("balqty").retDbl());
                                row["balval"] = g.Sum(r => r.Field<double?>("balval").retDbl());
                                return row;
                            }).CopyToDataTable();
                int cnt = 0;
                for (int k = 0; k <= unitwisegrptotal.Rows.Count - 1; k++)
                {
                    if (unitwisegrptotal.Rows[k]["opqty"].retDbl() != 0 || unitwisegrptotal.Rows[k]["netpur"].retDbl() != 0 || unitwisegrptotal.Rows[k]["karqty"].retDbl() != 0 || unitwisegrptotal.Rows[k]["approval"].retDbl() != 0 || unitwisegrptotal.Rows[k]["netstktrans"].retDbl() != 0 || unitwisegrptotal.Rows[k]["netadj"].retDbl() != 0 || unitwisegrptotal.Rows[k]["netsale"].retDbl() != 0 || unitwisegrptotal.Rows[k]["balqty"].retDbl() != 0)
                    {
                        cnt++;
                        if (k == 0) { }
                        else { IR.Rows.Add(""); rNo = IR.Rows.Count - 1; }
                        IR.Rows[rNo]["uomnm"] = unitwisegrptotal.Rows[k]["uomnm"];
                        IR.Rows[rNo]["opqty"] = unitwisegrptotal.Rows[k]["opqty"];
                        IR.Rows[rNo]["opval"] = unitwisegrptotal.Rows[k]["opval"];
                        IR.Rows[rNo]["netpur"] = unitwisegrptotal.Rows[k]["netpur"];
                        IR.Rows[rNo]["purval"] = unitwisegrptotal.Rows[k]["purval"];
                        IR.Rows[rNo]["karqty"] = unitwisegrptotal.Rows[k]["karqty"];
                        IR.Rows[rNo]["karval"] = unitwisegrptotal.Rows[k]["karval"];
                        IR.Rows[rNo]["approval"] = unitwisegrptotal.Rows[k]["approval"];
                        IR.Rows[rNo]["netstktrans"] = unitwisegrptotal.Rows[k]["netstktrans"];
                        IR.Rows[rNo]["netadj"] = unitwisegrptotal.Rows[k]["netadj"];
                        IR.Rows[rNo]["netsale"] = unitwisegrptotal.Rows[k]["netsale"];
                        IR.Rows[rNo]["salevalue"] = unitwisegrptotal.Rows[k]["salevalue"];
                        IR.Rows[rNo]["balqty"] = unitwisegrptotal.Rows[k]["balqty"];
                        IR.Rows[rNo]["balval"] = unitwisegrptotal.Rows[k]["balval"];

                    }
                }
                if (cnt > 1)
                {
                    IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;";
                }
                else
                {
                    IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;border-top: 3px solid;";
                }
                if (i > maxB) break;
            }
            // Create Blank line
            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
            IR.Rows[rNo]["dammy"] = " ";
            IR.Rows[rNo]["flag"] = " height:8px; ";

            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
            IR.Rows[rNo]["itnm"] = "Grand Total";
            IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;";

            var grptbl = IR.AsEnumerable().Where(g => g.Field<string>("itgrpcd").retStr() != "")
                            .GroupBy(g => g.Field<string>("uomnm"))
                            .Select(g =>
                            {
                                var row = IR.NewRow();
                                row["uomnm"] = g.Key;
                                row["opqty"] = g.Sum(r => r.Field<double?>("opqty") == null ? 0 : r.Field<double>("opqty"));
                                row["opval"] = g.Sum(r => r.Field<double?>("opval") == null ? 0 : r.Field<double>("opval"));
                                row["netpur"] = g.Sum(r => r.Field<double?>("netpur").retDbl());
                                row["purval"] = g.Sum(r => r.Field<double?>("purval").retDbl());
                                row["karqty"] = g.Sum(r => r.Field<double?>("karqty").retDbl());
                                row["karval"] = g.Sum(r => r.Field<double?>("karval").retDbl());
                                row["approval"] = g.Sum(r => r.Field<double?>("approval").retDbl());
                                row["netstktrans"] = g.Sum(r => r.Field<double?>("netstktrans").retDbl());
                                row["netadj"] = g.Sum(r => r.Field<double?>("netadj").retDbl());
                                row["netsale"] = g.Sum(r => r.Field<double?>("netsale").retDbl());
                                row["salevalue"] = g.Sum(r => r.Field<double?>("salevalue").retDbl());
                                row["balqty"] = g.Sum(r => r.Field<double?>("balqty").retDbl());
                                row["balval"] = g.Sum(r => r.Field<double?>("balval").retDbl());
                                return row;
                            }).CopyToDataTable();
            int cnt1 = 0;
            for (int k = 0; k <= grptbl.Rows.Count - 1; k++)
            {
                if (grptbl.Rows[k]["opqty"].retDbl() != 0 || grptbl.Rows[k]["netpur"].retDbl() != 0 || grptbl.Rows[k]["karqty"].retDbl() != 0 || grptbl.Rows[k]["approval"].retDbl() != 0 || grptbl.Rows[k]["netstktrans"].retDbl() != 0 || grptbl.Rows[k]["netadj"].retDbl() != 0 || grptbl.Rows[k]["netsale"].retDbl() != 0 || grptbl.Rows[k]["balqty"].retDbl() != 0)
                {
                    cnt1++;
                    if (k != 0)
                    {
                        IR.Rows.Add("");
                        rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;";
                    }

                    IR.Rows[rNo]["itgrpcd"] = "grandtotal";
                    IR.Rows[rNo]["uomnm"] = grptbl.Rows[k]["uomnm"];
                    IR.Rows[rNo]["opqty"] = grptbl.Rows[k]["opqty"];
                    IR.Rows[rNo]["opval"] = grptbl.Rows[k]["opval"];
                    IR.Rows[rNo]["netpur"] = grptbl.Rows[k]["netpur"];
                    IR.Rows[rNo]["purval"] = grptbl.Rows[k]["purval"];
                    IR.Rows[rNo]["karqty"] = grptbl.Rows[k]["karqty"];
                    IR.Rows[rNo]["karval"] = grptbl.Rows[k]["karval"];
                    IR.Rows[rNo]["approval"] = grptbl.Rows[k]["approval"];
                    IR.Rows[rNo]["netstktrans"] = grptbl.Rows[k]["netstktrans"];
                    IR.Rows[rNo]["netadj"] = grptbl.Rows[k]["netadj"];
                    IR.Rows[rNo]["netsale"] = grptbl.Rows[k]["netsale"];
                    IR.Rows[rNo]["salevalue"] = grptbl.Rows[k]["salevalue"];
                    IR.Rows[rNo]["balqty"] = grptbl.Rows[k]["balqty"];
                    IR.Rows[rNo]["balval"] = grptbl.Rows[k]["balval"];

                }
            }


            if (cnt1 > 1)
            {
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["itnm"] = "Total Value";
                IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;";
                IR.Rows[rNo]["opval"] = IR.AsEnumerable().Where(a => a.Field<string>("itgrpcd") == "grandtotal").Sum(b => b.Field<double>("opval"));
                IR.Rows[rNo]["purval"] = IR.AsEnumerable().Where(a => a.Field<string>("itgrpcd") == "grandtotal").Sum(b => b.Field<double>("purval"));
                IR.Rows[rNo]["karval"] = IR.AsEnumerable().Where(a => a.Field<string>("itgrpcd") == "grandtotal").Sum(b => b.Field<double>("karval"));
                IR.Rows[rNo]["salevalue"] = IR.AsEnumerable().Where(a => a.Field<string>("itgrpcd") == "grandtotal").Sum(b => b.Field<double>("salevalue"));
                IR.Rows[rNo]["balval"] = IR.AsEnumerable().Where(a => a.Field<string>("itgrpcd") == "grandtotal").Sum(b => b.Field<double>("balval"));
            }
            IR.Columns.Remove("itgrpcd");
            string pghdr1 = "";
            string repname = "Stock_Val" + System.DateTime.Now;

            pghdr1 = "Stock Valuation as on " + fdt;
            PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);

            TempData[repname] = PV;
            TempData[repname + "xxx"] = IR;
            return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
        }
    }
}