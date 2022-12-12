using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using OfficeOpenXml;
using OfficeOpenXml.Style;
namespace Improvar.Controllers
{
    public class Rep_SupplierController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string scm = "", scmf = "";
        string fdt = "";
        string tdt = "";
        string LOC = "", COM = "";
        string MENU_PARA = "";
        string slcd = "", itgrpcd = "", loccd = "", autono = "";
        string reptype = "";

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

                    VE.DropDown_list_TXN = DropDownHelp.GetTxnforSelection("", "t_batchmst");
                    VE.DOCNO = MasterHelp.ComboFill("autono", VE.DropDown_list_TXN, 0, 1);

                    VE.DropDown_list3 = (from i in DBF.M_LOCA
                                         where i.COMPCD == com
                                         select new DropDown_list3() { value = i.LOCCD, text = i.LOCNM }).Distinct().OrderBy(s => s.text).ToList();// location
                    VE.TEXTBOX5 = MasterHelp.ComboFill("loccd", VE.DropDown_list3, 0, 1);
                    VE.Compnm = CommVar.Compcd(UNQSNO);
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
            try
            {
                scm = CommVar.CurSchema(UNQSNO); scmf = CommVar.FinSchema(UNQSNO);
                fdt = VE.FDT.retStr();
                tdt = VE.TDT.retStr();
                LOC = CommVar.Loccd(UNQSNO); COM = CommVar.Compcd(UNQSNO);
                MENU_PARA = VE.MENU_PARA.retStr();
                if (FC.AllKeys.Contains("slcdvalue")) slcd = CommFunc.retSqlformat(FC["slcdvalue"].ToString());
                if (FC.AllKeys.Contains("itgrpcdvalue")) itgrpcd = CommFunc.retSqlformat(FC["itgrpcdvalue"].ToString());
                if (FC.AllKeys.Contains("loccdvalue")) loccd = CommFunc.retSqlformat(FC["loccdvalue"].ToString());
                if (FC.AllKeys.Contains("autonovalue")) autono = CommFunc.retSqlformat(FC["autonovalue"].ToString());
                reptype = VE.TEXTBOX1.retStr();
                DataTable tbl = new DataTable();
                DataTable LOCDT = new DataTable("loccd");

                if (reptype == "AdityaBirlaStock") { return GetAdityaBirlaStock(scm, scmf, fdt, tdt, LOC, COM, slcd, itgrpcd, loccd); }
                else if (reptype == "AdityaBirlaSale") { return GetSalesfromSupplier(scm, scmf, fdt, tdt, LOC, COM, slcd, itgrpcd, loccd); }
                else if (reptype == "PurchasebillwiseStock") { return retPurchaseWiseStock(scm, scmf, fdt, tdt, LOC, COM, slcd, itgrpcd, loccd); }
                else if (reptype == "ClosingStockBarcode") { return ClosingStockBarcodeWise(scm, scmf, fdt, tdt, LOC, COM, slcd, itgrpcd, loccd); }
                //if (reptype != "PurchasebillwiseStock")
                //{
                string sql = "select a.autono,e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign, a.doctag, a.qnty, a.txblval, a.othramt, f.itgrpcd, h.itgrpnm, f.itnm, ";
                sql += Environment.NewLine + "nvl(e.pdesign, f.styleno) styleno, e.othrate, nvl(b.rate, 0) oprate, nvl(cp.rate, 0) clrate, ";
                sql += Environment.NewLine + "nvl(rp.rate, 0) rprate, ";
                sql += Environment.NewLine + "f.uomcd, i.uomnm, i.decimals, g.itnm fabitnm,l.docno,l.docdt,nvl(m.prefno,l.docno)blno,nvl(m.prefdt,l.docdt)bldt   from ";
                sql += Environment.NewLine + " ";
                sql += Environment.NewLine + "(select b.autono,d.compcd, d.loccd, a.barno, 'OP' doctag, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, ";
                sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval, ";
                sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt ";
                sql += Environment.NewLine + "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e ";
                sql += Environment.NewLine + "where a.barno = b.barno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and ";
                sql += Environment.NewLine + "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D', 'C') and ";
                sql += Environment.NewLine + "d.docdt < to_date('" + fdt + "', 'dd/mm/yyyy') ";
                sql += Environment.NewLine + "group by b.autono,d.compcd, d.loccd, a.barno, 'OP' ";
                sql += Environment.NewLine + "union all ";
                sql += Environment.NewLine + "select b.autono,d.compcd, d.loccd, a.barno, c.doctag, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, ";
                sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval,  ";
                sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt ";
                sql += Environment.NewLine + "    from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e ";
                sql += Environment.NewLine + "where a.barno = b.barno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and ";
                sql += Environment.NewLine + "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N')= 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D','C') and ";
                sql += Environment.NewLine + "d.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and d.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') ";
                sql += Environment.NewLine + "group by b.autono,d.compcd, d.loccd, a.barno, c.doctag ) a, ";
                sql += Environment.NewLine + " ";
                sql += Environment.NewLine + "(select barno, effdt, prccd, rate from ( ";
                sql += Environment.NewLine + "select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
                sql += Environment.NewLine + "from " + scm + ".t_batchmst_price a ";
                sql += Environment.NewLine + "where a.effdt <= to_date('" + fdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) b, ";
                sql += Environment.NewLine + " ";
                sql += Environment.NewLine + "(select barno, effdt, prccd, rate from ( ";
                sql += Environment.NewLine + "select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
                sql += Environment.NewLine + "from " + scm + ".t_batchmst_price a ";
                sql += Environment.NewLine + "where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) cp, ";
                sql += Environment.NewLine + " (select barno, effdt, prccd, rate from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
                sql += Environment.NewLine + " from " + scm + ".t_batchmst_price a where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'RP' ) where rn = 1) rp, ";
                sql += Environment.NewLine + " ";
                sql += Environment.NewLine + "" + scm + ".t_batchmst e, " + scm + ".m_sitem f, " + scm + ".m_sitem g, " + scm + ".m_group h, " + scmf + ".m_uom i, ";
                sql += Environment.NewLine + "" + scmf + ".m_subleg j, " + scmf + ".m_loca k," + scm + ".t_cntrl_hdr l," + scm + ".t_txn m ";
                sql += Environment.NewLine + "where a.barno = e.barno(+) and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and ";
                sql += Environment.NewLine + "a.barno = b.barno(+) and a.barno = cp.barno(+)and a.barno = rp.barno(+)  and e.slcd = j.slcd(+) and a.compcd || a.loccd = k.compcd || k.loccd and ";
                sql += Environment.NewLine + "f.itgrpcd = h.itgrpcd(+) and f.uomcd = i.uomcd(+) and a.autono=l.autono(+) and a.autono=m.autono(+) " + Environment.NewLine;
                if (slcd.retStr() != "") sql += "and e.slcd in (" + slcd + ") ";
                if (itgrpcd.retStr() != "") sql += "and f.itgrpcd in (" + itgrpcd + ") ";
                if (loccd.retStr() != "") sql += "and a.loccd in (" + loccd + ") ";
                if (autono.retStr() != "") sql += "and a.autono in (" + autono + ") ";
                if (reptype == "B")
                {
                    sql += Environment.NewLine + "order by slcd,slnm,blno,bldt";

                }
                else {

                    sql += Environment.NewLine + "order by slcd,slnm,itgrpnm, itgrpcd, fabitnm, fabitcd, itnm, itcd, styleno, barno ";
                }

                tbl = MasterHelp.SQLquery(sql);
                if (tbl.Rows.Count == 0) return Content("no records..");

                if (reptype == "B" || reptype == "D") { return GetBillWiseQnty(tbl, VE); }
                string[] LOCTBLCOLS = new string[] { "loccd", "locnm" };
                LOCDT = tbl.DefaultView.ToTable(true, LOCTBLCOLS);
                // }
                Int32 rNo = 0, maxR = 0, maxB = 0, i = 0;
                maxR = tbl.Rows.Count - 1;
                Int32 islno = 0;

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                DataTable IR = new DataTable("");

                HC.RepStart(IR, 2);
                if (reptype == "N") HC.GetPrintHeader(IR, "slno", "long", "n,4", "Slno");
                if (reptype == "S") HC.GetPrintHeader(IR, "slnm", "string", "c,40", "Party");
                if (reptype == "S") HC.GetPrintHeader(IR, "barno", "string", "c,30", "Bar No.");
                if (reptype == "N") HC.GetPrintHeader(IR, "blno", "string", "c,40", "Bill No.");
                if (reptype == "N") HC.GetPrintHeader(IR, "bldt", "string", "c,10", "Bill Date");
                if (reptype == "S") if (VE.Checkbox3 == true) HC.GetPrintHeader(IR, "blno", "string", "c,40", "Bill No.");
                if (reptype == "S") if (VE.Checkbox3 == true) HC.GetPrintHeader(IR, "bldt", "string", "c,10", "Bill Date");
                if (reptype == "S") HC.GetPrintHeader(IR, "itgrpnm", "string", "c,40", "Group");
                HC.GetPrintHeader(IR, "styleno", "string", "c,40", "Styleno");
                HC.GetPrintHeader(IR, "pdesign", "string", "c,20", "Party Design");
                HC.GetPrintHeader(IR, "uomnm", "string", "c,5", "uom");
                HC.GetPrintHeader(IR, "openingqnty", "double", "n,10,2", "Opening Qty");
                if (MENU_PARA != "Q") HC.GetPrintHeader(IR, "openingamt", "double", "n,10,2", "Opening Value");
                HC.GetPrintHeader(IR, "purchaseqnty", "double", "n,10,2", "Purchase Qty");
                if (MENU_PARA != "Q") HC.GetPrintHeader(IR, "purchaseamt", "double", "n,10,2", "Purchase Value");
                HC.GetPrintHeader(IR, "purretqnty", "double", "n,10,2", "Pur.Ret Qty");
                if (MENU_PARA != "Q") HC.GetPrintHeader(IR, "purretamt", "double", "n,10,2", "Pur.Ret Value");
                HC.GetPrintHeader(IR, "transferinqnty", "double", "n,10,2", "Transfer In Qty");
                if (MENU_PARA != "Q") HC.GetPrintHeader(IR, "transferinamt", "double", "n,10,2", "Transfer In Value");
                HC.GetPrintHeader(IR, "transferoutqnty", "double", "n,10,2", "Transfer Out Qty");
                if (MENU_PARA != "Q") HC.GetPrintHeader(IR, "transferoutamt", "double", "n,10,2", "Transfer Out Value");
                if (VE.Checkbox1 == true)
                {
                    foreach (DataRow dr in LOCDT.Rows)
                    {
                        HC.GetPrintHeader(IR, dr["loccd"].ToString() + "salesqnty", "double", "n,10,2", dr["locnm"].ToString() + " Sales Qty");
                        if (MENU_PARA != "Q") HC.GetPrintHeader(IR, dr["loccd"].ToString() + "salesamt", "double", "n,10,2", dr["locnm"].ToString() + " Sales Value");
                    }
                }
                else
                {
                    HC.GetPrintHeader(IR, "salesqnty", "double", "n,10,2", "Sales Qty");
                    if (MENU_PARA != "Q") HC.GetPrintHeader(IR, "salesamt", "double", "n,10,2", "Sales Value");
                }

                HC.GetPrintHeader(IR, "othersqnty", "double", "n,10,2", "Others Qty");
                if (MENU_PARA != "Q") HC.GetPrintHeader(IR, "othersamt", "double", "n,10,2", "Others Value");
                HC.GetPrintHeader(IR, "stockqnty", "double", "n,10,2", "Closing Stock Qty");
                if (MENU_PARA != "Q") HC.GetPrintHeader(IR, "stockamt", "double", "n,10,2", "Closing Stock Value");

                IR.Columns.Add("itgrpcd", typeof(string), "");
                IR.Columns.Add("slcd", typeof(string), "");

                maxB = tbl.Rows.Count - 1;
                i = 0;
                itgrpcd = ""; bool PrintSkip = false; int scount = 0;
                int cnt = 0; //int indx = 0;
                while (i <= maxB)
                {
                    if (i != 0 && reptype == "N")
                    {
                        // Create Blank line
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["dammy"] = " ";
                        IR.Rows[rNo]["flag"] = " height:8px; ";
                    }
                    slcd = tbl.Rows[i]["slcd"].retStr();
                    if (reptype == "N")
                    {

                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["Dammy"] = tbl.Rows[i]["slnm"].ToString();
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";


                    }
                    int indx = 0; PrintSkip = false; int cntslcd = 0;
                    while (tbl.Rows[i]["slcd"].retStr() == slcd)
                    {
                        int cntgrp = 0; scount = 0;
                        itgrpcd = tbl.Rows[i]["itgrpcd"].retStr();
                        if (reptype == "N")
                        {

                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["Dammy"] = tbl.Rows[i]["itgrpnm"].ToString();
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";


                        }
                        islno = 0;

                        indx = 0; PrintSkip = false;
                        while (tbl.Rows[i]["slcd"].retStr() == slcd && tbl.Rows[i]["itgrpcd"].retStr() == itgrpcd)
                        {
                            string itcd = tbl.Rows[i]["itcd"].retStr();
                            //IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            //islno++; 
                            if (VE.Checkbox2 == false) islno = 0;
                            indx = 0; PrintSkip = false;
                            double opqnty1 = 0, opvalue1 = 0, othersqnty1 = 0, othersvalue1 = 0, pbqnty1 = 0, pbvalue1 = 0, sbqnty1 = 0, sbvalue1 = 0, srqnty1 = 0, srvalue1 = 0, prqnty1 = 0, prvalue1 = 0, tiqnty1 = 0, tivalue1 = 0, toqnty1 = 0, tovalue1 = 0;
                            while (tbl.Rows[i]["slcd"].retStr() == slcd && tbl.Rows[i]["itgrpcd"].retStr() == itgrpcd && tbl.Rows[i]["itcd"].retStr() == itcd)
                            {
                                // scount = 0;
                                opqnty1 = 0; opvalue1 = 0; othersqnty1 = 0; othersvalue1 = 0; pbqnty1 = 0; pbvalue1 = 0; sbqnty1 = 0; sbvalue1 = 0; srqnty1 = 0; srvalue1 = 0; prqnty1 = 0; prvalue1 = 0; tiqnty1 = 0; tivalue1 = 0; toqnty1 = 0; tovalue1 = 0;
                                var blno = tbl.Rows[i]["blno"].retStr(); string colnm = ""; indx = 0; PrintSkip = false;
                                while (tbl.Rows[i]["slcd"].retStr() == slcd && tbl.Rows[i]["itgrpcd"].retStr() == itgrpcd && tbl.Rows[i]["itcd"].retStr() == itcd && tbl.Rows[i]["blno"].retStr() == blno)
                                {
                                    PrintSkip = false;
                                    indx = i; double opqnty = 0; double opvalue = 0; double othersqnty = 0; double othersvalue = 0; double pbqnty = 0; double pbvalue = 0; double sbqnty = 0; double sbvalue = 0; double srqnty = 0; double srvalue = 0; double prqnty = 0; double prvalue = 0; double tiqnty = 0; double tivalue = 0; double toqnty = 0; double tovalue = 0;
                                    colnm = ""; //islno = 0;
                                                //string key = tbl.Rows[i]["barno"].retStr() + tbl.Rows[i]["pdesign"].retStr() + tbl.Rows[i]["doctag"].retStr() + tbl.Rows[i]["loccd"].retStr();
                                    string key = tbl.Rows[i]["barno"].retStr() + tbl.Rows[i]["pdesign"].retStr() + tbl.Rows[i]["loccd"].retStr();
                                    while (tbl.Rows[i]["slcd"].retStr() == slcd && tbl.Rows[i]["itgrpcd"].retStr() == itgrpcd && tbl.Rows[i]["itcd"].retStr() == itcd && (tbl.Rows[i]["barno"].retStr() + tbl.Rows[i]["pdesign"].retStr() + tbl.Rows[i]["loccd"].retStr()) == key)
                                    {

                                        //IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                        //if (reptype == "N") IR.Rows[rNo]["slno"] = islno;
                                        //IR.Rows[rNo]["styleno"] = tbl.Rows[i]["styleno"].ToString();
                                        //IR.Rows[rNo]["pdesign"] = tbl.Rows[i]["pdesign"].ToString();
                                        //IR.Rows[rNo]["uomnm"] = tbl.Rows[i]["uomnm"].ToString();
                                        //IR.Rows[rNo]["itgrpcd"] = tbl.Rows[i]["itgrpcd"].retStr();
                                        //IR.Rows[rNo]["slcd"] = tbl.Rows[i]["slcd"].retStr();
                                        //if (reptype == "S") IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"].retStr();
                                        //if (reptype == "S") IR.Rows[rNo]["barno"] = tbl.Rows[i]["barno"].retStr();
                                        //if (reptype == "S") IR.Rows[rNo]["itgrpnm"] = tbl.Rows[i]["itgrpnm"].retStr();
                                        //if (reptype == "S") IR.Rows[rNo]["blno"] = tbl.Rows[i]["blno"].retStr();
                                        //if (reptype == "S") IR.Rows[rNo]["bldt"] = tbl.Rows[i]["bldt"].retDateStr();
                                        if (tbl.Rows[i]["doctag"].retStr() == "OP")
                                        {
                                            //IR.Rows[rNo]["openingqnty"] = IR.Rows[rNo]["openingqnty"].retDbl() + tbl.Rows[i]["qnty"].retDbl();
                                            //if (MENU_PARA != "Q") IR.Rows[rNo]["openingamt"] = IR.Rows[rNo]["openingamt"].retDbl() + tbl.Rows[i]["txblval"].retDbl();
                                            opqnty += tbl.Rows[i]["qnty"].retDbl();
                                            opvalue += tbl.Rows[i]["txblval"].retDbl();
                                        }
                                        else if (tbl.Rows[i]["doctag"].retStr() == "PB")
                                        {
                                            //IR.Rows[rNo]["purchaseqnty"] = IR.Rows[rNo]["purchaseqnty"].retDbl() + tbl.Rows[i]["qnty"].retDbl();
                                            //if (MENU_PARA != "Q") IR.Rows[rNo]["purchaseamt"] = IR.Rows[rNo]["purchaseamt"].retDbl() + tbl.Rows[i]["txblval"].retDbl();
                                            pbqnty += tbl.Rows[i]["qnty"].retDbl();
                                            pbvalue += tbl.Rows[i]["txblval"].retDbl();
                                        }
                                        else if (tbl.Rows[i]["doctag"].retStr() == "PR")
                                        {
                                            //IR.Rows[rNo]["purretqnty"] = IR.Rows[rNo]["purretqnty"].retDbl() + (tbl.Rows[i]["qnty"].retDbl() * -1);
                                            //if (MENU_PARA != "Q") IR.Rows[rNo]["purretamt"] = IR.Rows[rNo]["purretamt"].retDbl() + (tbl.Rows[i]["txblval"].retDbl() * -1);
                                            prqnty += tbl.Rows[i]["qnty"].retDbl() * -1;
                                            prvalue += tbl.Rows[i]["txblval"].retDbl() * -1;
                                        }
                                        else if (tbl.Rows[i]["doctag"].retStr() == "TI")
                                        {
                                            //IR.Rows[rNo]["transferinqnty"] = IR.Rows[rNo]["transferinqnty"].retDbl() + tbl.Rows[i]["qnty"].retDbl();
                                            //if (MENU_PARA != "Q") IR.Rows[rNo]["transferinamt"] = IR.Rows[rNo]["transferinamt"].retDbl() + tbl.Rows[i]["txblval"].retDbl();
                                            tiqnty += tbl.Rows[i]["qnty"].retDbl();
                                            tivalue += tbl.Rows[i]["txblval"].retDbl();
                                        }
                                        else if (tbl.Rows[i]["doctag"].retStr() == "TO")
                                        {
                                            //IR.Rows[rNo]["transferoutqnty"] = IR.Rows[rNo]["transferoutqnty"].retDbl() + (tbl.Rows[i]["qnty"].retDbl() * -1);
                                            //if (MENU_PARA != "Q") IR.Rows[rNo]["transferoutamt"] = IR.Rows[rNo]["transferoutamt"].retDbl() + (tbl.Rows[i]["txblval"].retDbl() * -1);
                                            toqnty += tbl.Rows[i]["qnty"].retDbl() * -1;
                                            tovalue += tbl.Rows[i]["txblval"].retDbl() * -1;
                                        }
                                        else if (tbl.Rows[i]["doctag"].retStr() == "SB" || tbl.Rows[i]["doctag"].retStr() == "SR")
                                        {
                                            if (VE.Checkbox1 == true)
                                            {
                                                colnm = tbl.Rows[i]["loccd"].retStr() + "salesqnty";
                                                //IR.Rows[rNo][colnm] = IR.Rows[rNo][colnm].retDbl() + (tbl.Rows[i]["doctag"].retStr() == "SB" ? (tbl.Rows[i]["qnty"].retDbl() * -1) : tbl.Rows[i]["qnty"].retDbl());//modify by mithun
                                                //if (MENU_PARA != "Q") IR.Rows[rNo][colnm] = IR.Rows[rNo][colnm].retDbl() + (tbl.Rows[i]["txblval"].retDbl() * -1);

                                            }
                                            else
                                            {
                                                //IR.Rows[rNo]["salesqnty"] = IR.Rows[rNo]["salesqnty"].retDbl() + (tbl.Rows[i]["doctag"].retStr() == "SB" ? (tbl.Rows[i]["qnty"].retDbl() * -1) : tbl.Rows[i]["qnty"].retDbl());//modify by mithun
                                                //if (MENU_PARA != "Q") IR.Rows[rNo]["salesamt"] = IR.Rows[rNo]["salesamt"].retDbl() + (tbl.Rows[i]["txblval"].retDbl() * -1);

                                            }

                                            if (tbl.Rows[i]["doctag"].retStr() == "SB")
                                            {
                                                sbqnty += tbl.Rows[i]["qnty"].retDbl() * -1;
                                                sbvalue += tbl.Rows[i]["txblval"].retDbl() * -1;

                                            }
                                            else
                                            {
                                                srqnty += tbl.Rows[i]["qnty"].retDbl();
                                                srvalue += tbl.Rows[i]["txblval"].retDbl();
                                            }
                                        }
                                        else
                                        {
                                            othersqnty += tbl.Rows[i]["qnty"].retDbl();
                                            othersvalue += tbl.Rows[i]["txblval"].retDbl();

                                            //opqnty += tbl.Rows[i]["qnty"].retDbl();
                                            //opvalue += tbl.Rows[i]["txblval"].retDbl();
                                        }
                                        //IR.Rows[rNo]["othersqnty"] = othersqnty;
                                        //if (MENU_PARA != "Q") IR.Rows[rNo]["othersamt"] = othersvalue;

                                        //IR.Rows[rNo]["stockqnty"] = opqnty+ pbqnty+ prqnty+ tiqnty + toqnty+ sbqnty+ srqnty+ othersqnty;
                                        //if (MENU_PARA != "Q") IR.Rows[rNo]["stockamt"] = opvalue+ pbvalue+ prvalue+ tivalue+ tovalue+ sbvalue+ srvalue+ othersvalue;
                                        i++;
                                        if (i > maxB) break;
                                    }
                                    opqnty1 += opqnty;
                                    opvalue1 += opvalue;
                                    pbqnty1 += pbqnty;
                                    pbvalue1 += pbvalue;
                                    prqnty1 += prqnty;
                                    prvalue1 += prvalue;
                                    tiqnty1 += tiqnty;
                                    tivalue1 += tivalue;
                                    toqnty1 += toqnty;
                                    tovalue1 += tovalue;
                                    sbqnty1 += sbqnty;
                                    sbvalue1 += sbvalue;
                                    srqnty1 += srqnty;
                                    srvalue1 += srvalue;
                                    othersqnty1 += othersqnty;
                                    othersvalue1 += othersvalue;

                                    if (reptype == "S")
                                    {
                                        if (VE.Checkbox2 == true && sbqnty.retDbl() == 0) PrintSkip = true;
                                        if (PrintSkip == false)
                                        {
                                            islno++;
                                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                            if (reptype == "N") IR.Rows[rNo]["slno"] = islno;
                                            if (VE.Checkbox3 == true) IR.Rows[rNo]["blno"] = tbl.Rows[i - 1]["blno"].retStr();
                                            if (VE.Checkbox3 == true) IR.Rows[rNo]["bldt"] = tbl.Rows[i - 1]["bldt"].retDateStr();

                                            IR.Rows[rNo]["styleno"] = tbl.Rows[i - 1]["styleno"].ToString();
                                            IR.Rows[rNo]["pdesign"] = tbl.Rows[i - 1]["pdesign"].ToString();
                                            IR.Rows[rNo]["uomnm"] = tbl.Rows[i - 1]["uomnm"].ToString();
                                            IR.Rows[rNo]["itgrpcd"] = tbl.Rows[i - 1]["itgrpcd"].retStr();
                                            IR.Rows[rNo]["slcd"] = tbl.Rows[i - 1]["slcd"].retStr();
                                            if (reptype == "S") IR.Rows[rNo]["slnm"] = tbl.Rows[i - 1]["slnm"].retStr();
                                            if (reptype == "S") IR.Rows[rNo]["barno"] = tbl.Rows[i - 1]["barno"].retStr();
                                            if (reptype == "S") IR.Rows[rNo]["itgrpnm"] = tbl.Rows[i - 1]["itgrpnm"].retStr();
                                            IR.Rows[rNo]["openingqnty"] = opqnty;
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["openingamt"] = opvalue;

                                            IR.Rows[rNo]["purchaseqnty"] = pbqnty;
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["purchaseamt"] = pbvalue;

                                            IR.Rows[rNo]["purretqnty"] = prqnty;
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["purretamt"] = prvalue;

                                            IR.Rows[rNo]["transferinqnty"] = tiqnty;
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["transferinamt"] = tivalue;

                                            IR.Rows[rNo]["transferoutqnty"] = toqnty;
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["transferoutamt"] = tovalue;

                                            if (VE.Checkbox1 == true)
                                            {
                                                //colnm = tbl.Rows[i-1]["loccd"].retStr() + "salesqnty";
                                                if (colnm.retStr() != "") IR.Rows[rNo][colnm] = sbqnty;

                                                if (colnm.retStr() != "") if (MENU_PARA != "Q") IR.Rows[rNo][colnm] = sbvalue;

                                            }
                                            else
                                            {
                                                IR.Rows[rNo]["salesqnty"] = sbqnty;
                                                if (MENU_PARA != "Q") IR.Rows[rNo]["salesamt"] = sbvalue;
                                            }



                                            IR.Rows[rNo]["othersqnty"] = othersqnty;
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["othersamt"] = othersvalue;

                                            IR.Rows[rNo]["stockqnty"] = (opqnty + pbqnty + tiqnty + srqnty + othersqnty) - (prqnty + toqnty + sbqnty);
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["stockamt"] = (opvalue + pbvalue + tivalue + srvalue + othersvalue) - (prvalue + tovalue + sbvalue);
                                        }
                                        else
                                        {
                                            if (scount == 0)
                                            {
                                                //if(IR.Rows.Count>0) IR.Rows.RemoveAt(IR.Rows.Count - 1); IR.AcceptChanges();

                                            }
                                        }
                                    }

                                    if (i > maxB) break;
                                }
                                if (reptype == "N")
                                {
                                    //i = indx;

                                    if (VE.Checkbox2 == true && sbqnty1.retDbl() == 0) PrintSkip = true;
                                    if (PrintSkip == false)
                                    {
                                        scount++; cntgrp++; cntslcd++;
                                        islno++;
                                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                        if (reptype == "N") IR.Rows[rNo]["slno"] = islno;
                                        IR.Rows[rNo]["styleno"] = tbl.Rows[indx]["styleno"].ToString();
                                        IR.Rows[rNo]["pdesign"] = tbl.Rows[indx]["pdesign"].ToString();
                                        IR.Rows[rNo]["uomnm"] = tbl.Rows[indx]["uomnm"].ToString();
                                        IR.Rows[rNo]["itgrpcd"] = tbl.Rows[indx]["itgrpcd"].retStr();
                                        IR.Rows[rNo]["slcd"] = tbl.Rows[indx]["slcd"].retStr();
                                        IR.Rows[rNo]["blno"] = tbl.Rows[indx]["blno"].retStr();
                                        IR.Rows[rNo]["bldt"] = tbl.Rows[indx]["bldt"].retDateStr();
                                        IR.Rows[rNo]["openingqnty"] = opqnty1;
                                        if (MENU_PARA != "Q") IR.Rows[rNo]["openingamt"] = opvalue1;

                                        IR.Rows[rNo]["purchaseqnty"] = pbqnty1;
                                        if (MENU_PARA != "Q") IR.Rows[rNo]["purchaseamt"] = pbvalue1;

                                        IR.Rows[rNo]["purretqnty"] = prqnty1;
                                        if (MENU_PARA != "Q") IR.Rows[rNo]["purretamt"] = prvalue1;

                                        IR.Rows[rNo]["transferinqnty"] = tiqnty1;
                                        if (MENU_PARA != "Q") IR.Rows[rNo]["transferinamt"] = tivalue1;

                                        IR.Rows[rNo]["transferoutqnty"] = toqnty1;
                                        if (MENU_PARA != "Q") IR.Rows[rNo]["transferoutamt"] = tovalue1;

                                        if (VE.Checkbox1 == true)
                                        {
                                            //colnm = tbl.Rows[i-1]["loccd"].retStr() + "salesqnty";
                                            if (colnm.retStr() != "") IR.Rows[rNo][colnm] = sbqnty1;

                                            if (colnm.retStr() != "") if (MENU_PARA != "Q") IR.Rows[rNo][colnm] = sbvalue1;

                                        }
                                        else
                                        {
                                            IR.Rows[rNo]["salesqnty"] = sbqnty1;
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["salesamt"] = sbvalue1;
                                        }



                                        IR.Rows[rNo]["othersqnty"] = othersqnty1;
                                        if (MENU_PARA != "Q") IR.Rows[rNo]["othersamt"] = othersvalue1;

                                        IR.Rows[rNo]["stockqnty"] = (opqnty1 + pbqnty1 + tiqnty1 + srqnty1 + othersqnty1) - (prqnty1 + toqnty1 + sbqnty1);

                                        if (MENU_PARA != "Q") IR.Rows[rNo]["stockamt"] = (opvalue1 + pbvalue1 + tivalue1 + srvalue1 + othersvalue1) - (prvalue1 + tovalue1 + sbvalue1);
                                    }
                                    else
                                    {
                                        if (scount == 0)
                                        {
                                            //if(IR.Rows.Count>0) IR.Rows.RemoveAt(IR.Rows.Count - 1); IR.AcceptChanges();

                                        }
                                    }

                                }
                                if (i > maxB) break;
                            }
                            if (scount == 0)
                            {
                                //if (IR.Rows.Count > 0) IR.Rows.RemoveAt(IR.Rows.Count - 1); IR.AcceptChanges();

                            }
                            if (i > maxB) break;

                            if (scount == 0)
                            {
                                //if (IR.Rows.Count > 0) IR.Rows.RemoveAt(IR.Rows.Count - 1); IR.AcceptChanges();

                            }
                        }
                        if (reptype == "N")
                        {
                            if (scount == 0 && cntgrp == 0)
                            {

                                IR.Rows.RemoveAt(IR.Rows.Count - 1); IR.AcceptChanges();
                            }

                            if (cntgrp != 0)
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["styleno"] = "Total of " + tbl.Rows[indx]["itgrpnm"].ToString();
                                IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;";

                                itgrpcd = tbl.Rows[indx]["itgrpcd"].ToString();
                                slcd = tbl.Rows[indx]["slcd"].ToString();
                                if (slcd == "OV00004")
                                {

                                }
                                cnt = 0;

                                var uomlist = IR.AsEnumerable().Where(g => g.Field<string>("uomnm").retStr() != "" && g.Field<string>("itgrpcd").retStr() == itgrpcd && g.Field<string>("slcd").retStr() == slcd).Select(a => a.Field<string>("uomnm")).Distinct().ToList();
                                if (uomlist != null && uomlist.Count > 0)
                                {
                                    for (int x = 0; x < uomlist.Count; x++)
                                    {
                                        if (cnt != 0)
                                        {
                                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                        }
                                        string uomnm = uomlist[x].retStr();
                                        IR.Rows[rNo]["uomnm"] = uomnm;
                                        int strart = reptype == "N" ? 9 : 10;
                                        for (int a = strart; a < IR.Columns.Count - 2; a++)
                                        {
                                            var unitwisegrptotal = IR.AsEnumerable().Where(g => g.Field<string>("uomnm").retStr() != "" && g.Field<string>("itgrpcd").retStr() == itgrpcd && g.Field<string>("slcd").retStr() == slcd && g.Field<string>("uomnm").retStr() == uomnm)
                                                   .GroupBy(g => g.Field<string>("uomnm"))
                                                   .Select(g =>
                                                   {
                                                       var row = IR.NewRow();
                                                       row["uomnm"] = g.Key;
                                                       row[IR.Columns[a].ColumnName] = g.Sum(r => r.Field<double?>(IR.Columns[a].ColumnName) == null ? 0 : r.Field<double>(IR.Columns[a].ColumnName));
                                                       return row;
                                                   }).CopyToDataTable();

                                            if (unitwisegrptotal != null && unitwisegrptotal.Rows.Count > 0)
                                            {
                                                IR.Rows[rNo][IR.Columns[a].ColumnName] = unitwisegrptotal.Rows[0][IR.Columns[a].ColumnName];
                                            }
                                        }
                                        cnt++;
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
                            }
                        }
                        if (i > maxB) break;
                    }
                    if (reptype == "N")
                    {
                        if (scount == 0 && cntslcd == 0)
                        {

                            IR.Rows.RemoveAt(IR.Rows.Count - 1); IR.AcceptChanges();
                        }
                        if (cntslcd != 0)
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["styleno"] = "Total of " + tbl.Rows[indx]["slnm"].ToString();
                            IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;";

                            slcd = tbl.Rows[indx]["slcd"].ToString();
                            if (slcd == "OV00004")
                            {
                            }


                            cnt = 0;
                            var uomlist1 = IR.AsEnumerable().Where(g => g.Field<string>("uomnm").retStr() != "" && g.Field<string>("slcd").retStr() == slcd).Select(a => a.Field<string>("uomnm")).Distinct().ToList();
                            if (uomlist1 != null && uomlist1.Count > 0)
                            {
                                for (int x = 0; x < uomlist1.Count; x++)
                                {
                                    if (cnt != 0)
                                    {
                                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                    }
                                    string uomnm = uomlist1[x].retStr();
                                    IR.Rows[rNo]["uomnm"] = uomnm;
                                    int strart = reptype == "N" ? 9 : 10;
                                    for (int a = strart; a < IR.Columns.Count - 2; a++)
                                    {
                                        var unitwisegrptotal = IR.AsEnumerable().Where(g => g.Field<string>("uomnm").retStr() != "" && g.Field<string>("slcd").retStr() == slcd && g.Field<string>("uomnm").retStr() == uomnm)
                                               .GroupBy(g => g.Field<string>("uomnm"))
                                               .Select(g =>
                                               {
                                                   var row = IR.NewRow();
                                                   row["uomnm"] = g.Key;
                                                   row[IR.Columns[a].ColumnName] = g.Sum(r => r.Field<double?>(IR.Columns[a].ColumnName) == null ? 0 : r.Field<double>(IR.Columns[a].ColumnName));
                                                   return row;
                                               }).CopyToDataTable();

                                        if (unitwisegrptotal != null && unitwisegrptotal.Rows.Count > 0)
                                        {
                                            IR.Rows[rNo][IR.Columns[a].ColumnName] = unitwisegrptotal.Rows[0][IR.Columns[a].ColumnName];
                                        }
                                    }
                                    cnt++;
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
                        }
                    }
                    if (i > maxB) break;
                }

                IR.Columns.Remove("itgrpcd");
                IR.Columns.Remove("slcd");
                string pghdr1 = "";
                string repname = ("Rep_Supplier").retRepname();

                pghdr1 = (MENU_PARA != "Q" ? "Supplier Wise Report with value from " : "Supplier wise Sales & Stock from ") + fdt + " to " + tdt;
                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);

                TempData[repname] = PV;
                TempData[repname + "xxx"] = IR;
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
            return null;
        }


        public ActionResult GetBillWiseQnty(DataTable tbl, ReportViewinHtml VE)
        {
            try
            {

                MENU_PARA = VE.MENU_PARA.retStr();

                reptype = VE.TEXTBOX1.retStr();

                DataTable LOCDT = new DataTable("loccd");

                string[] LOCTBLCOLS = new string[] { "loccd", "locnm" };
                LOCDT = tbl.DefaultView.ToTable(true, LOCTBLCOLS);
                // }
                Int32 rNo = 0, maxR = 0, maxB = 0, i = 0;
                maxR = tbl.Rows.Count - 1;
                Int32 islno = 0;

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                DataTable IR = new DataTable("");

                HC.RepStart(IR, 2);
                if (reptype == "D") HC.GetPrintHeader(IR, "slno", "long", "n,4", "Slno");
                if (reptype == "B") HC.GetPrintHeader(IR, "slnm", "string", "c,40", "Party");
                HC.GetPrintHeader(IR, "blno", "string", "c,40", "Bill No.");
                HC.GetPrintHeader(IR, "bldt", "string", "c,10", "Bill Date");
               // if (reptype == "B") HC.GetPrintHeader(IR, "itgrpnm", "string", "c,40", "Group");
                if (reptype == "D") HC.GetPrintHeader(IR, "barno", "string", "c,30", "Bar No.");
                if (reptype == "D") HC.GetPrintHeader(IR, "styleno", "string", "c,40", "Styleno");
                if (reptype == "D") HC.GetPrintHeader(IR, "pdesign", "string", "c,20", "Party Design");
                if (reptype == "D") HC.GetPrintHeader(IR, "uomnm", "string", "c,5", "uom");
                HC.GetPrintHeader(IR, "openingqnty", "double", "n,10,2", "Opening Qty");
                if (MENU_PARA != "Q") HC.GetPrintHeader(IR, "openingamt", "double", "n,10,2", "Opening Value");
                HC.GetPrintHeader(IR, "purchaseqnty", "double", "n,10,2", "Purchase Qty");
                if (MENU_PARA != "Q") HC.GetPrintHeader(IR, "purchaseamt", "double", "n,10,2", "Purchase Value");
                HC.GetPrintHeader(IR, "purretqnty", "double", "n,10,2", "Pur.Ret Qty");
                if (MENU_PARA != "Q") HC.GetPrintHeader(IR, "purretamt", "double", "n,10,2", "Pur.Ret Value");
                HC.GetPrintHeader(IR, "transferinqnty", "double", "n,10,2", "Transfer In Qty");
                if (MENU_PARA != "Q") HC.GetPrintHeader(IR, "transferinamt", "double", "n,10,2", "Transfer In Value");
                HC.GetPrintHeader(IR, "transferoutqnty", "double", "n,10,2", "Transfer Out Qty");
                if (MENU_PARA != "Q") HC.GetPrintHeader(IR, "transferoutamt", "double", "n,10,2", "Transfer Out Value");
                if (VE.Checkbox1 == true)
                {
                    foreach (DataRow dr in LOCDT.Rows)
                    {
                        HC.GetPrintHeader(IR, dr["loccd"].ToString() + "salesqnty", "double", "n,10,2", dr["locnm"].ToString() + " Sales Qty");
                        if (MENU_PARA != "Q") HC.GetPrintHeader(IR, dr["loccd"].ToString() + "salesamt", "double", "n,10,2", dr["locnm"].ToString() + " Sales Value");
                    }
                }
                else
                {
                    HC.GetPrintHeader(IR, "salesqnty", "double", "n,10,2", "Sales Qty");
                    if (MENU_PARA != "Q") HC.GetPrintHeader(IR, "salesamt", "double", "n,10,2", "Sales Value");
                }

                HC.GetPrintHeader(IR, "othersqnty", "double", "n,10,2", "Others Qty");
                if (MENU_PARA != "Q") HC.GetPrintHeader(IR, "othersamt", "double", "n,10,2", "Others Value");
                HC.GetPrintHeader(IR, "stockqnty", "double", "n,10,2", "Closing Stock Qty");
                if (MENU_PARA != "Q") HC.GetPrintHeader(IR, "stockamt", "double", "n,10,2", "Closing Stock Value");

                IR.Columns.Add("itgrpcd", typeof(string), "");
                IR.Columns.Add("slcd", typeof(string), "");

                maxB = tbl.Rows.Count - 1;
                i = 0;
                itgrpcd = ""; bool PrintSkip = false; int scount = 0;
                int cnt = 0; //int indx = 0;
                while (i <= maxB)
                {
                    if (i != 0 && reptype == "N")
                    {
                        // Create Blank line
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["dammy"] = " ";
                        IR.Rows[rNo]["flag"] = " height:8px; ";
                    }
                    slcd = tbl.Rows[i]["slcd"].retStr();
                    if (reptype == "N")
                    {

                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["Dammy"] = tbl.Rows[i]["slnm"].ToString();
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";


                    }
                    int indx = 0; PrintSkip = false; int cntslcd = 0;
                    while (tbl.Rows[i]["slcd"].retStr() == slcd)
                    {
                        int cntgrp = 0; scount = 0;
                        itgrpcd = tbl.Rows[i]["itgrpcd"].retStr();
                        if (reptype == "N")
                        {

                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["Dammy"] = tbl.Rows[i]["itgrpnm"].ToString();
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";


                        }
                        islno = 0;

                        indx = 0; PrintSkip = false; var blno = tbl.Rows[i]["blno"].retStr(); var bldt = tbl.Rows[i]["bldt"].retDateStr();
                        while (tbl.Rows[i]["slcd"].retStr() == slcd && tbl.Rows[i]["itgrpcd"].retStr() == itgrpcd)
                        {
                            string itcd = tbl.Rows[i]["itcd"].retStr();
                            //IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            //islno++; 
                            if (VE.Checkbox2 == false) islno = 0;
                            indx = 0; PrintSkip = false; blno = tbl.Rows[i]["blno"].retStr(); bldt = tbl.Rows[i]["bldt"].retDateStr();
                            double opqnty1 = 0, opvalue1 = 0, othersqnty1 = 0, othersvalue1 = 0, pbqnty1 = 0, pbvalue1 = 0, sbqnty1 = 0, sbvalue1 = 0, srqnty1 = 0, srvalue1 = 0, prqnty1 = 0, prvalue1 = 0, tiqnty1 = 0, tivalue1 = 0, toqnty1 = 0, tovalue1 = 0;
                            while (tbl.Rows[i]["slcd"].retStr() == slcd && tbl.Rows[i]["blno"].retStr() == blno && tbl.Rows[i]["bldt"].retDateStr() == bldt)
                            {
                                // scount = 0;
                                blno = tbl.Rows[i]["blno"].retStr(); bldt = tbl.Rows[i]["bldt"].retDateStr();
                                opqnty1 = 0; opvalue1 = 0; othersqnty1 = 0; othersvalue1 = 0; pbqnty1 = 0; pbvalue1 = 0; sbqnty1 = 0; sbvalue1 = 0; srqnty1 = 0; srvalue1 = 0; prqnty1 = 0; prvalue1 = 0; tiqnty1 = 0; tivalue1 = 0; toqnty1 = 0; tovalue1 = 0;
                                string colnm = ""; indx = 0; PrintSkip = false;
                                while (tbl.Rows[i]["slcd"].retStr() == slcd && tbl.Rows[i]["blno"].retStr() == blno && tbl.Rows[i]["bldt"].retDateStr() == bldt)
                                {
                                    PrintSkip = false;
                                    indx = i; double opqnty = 0; double opvalue = 0; double othersqnty = 0; double othersvalue = 0; double pbqnty = 0; double pbvalue = 0; double sbqnty = 0; double sbvalue = 0; double srqnty = 0; double srvalue = 0; double prqnty = 0; double prvalue = 0; double tiqnty = 0; double tivalue = 0; double toqnty = 0; double tovalue = 0;
                                    colnm = ""; //islno = 0;
                                                //string key = tbl.Rows[i]["barno"].retStr() + tbl.Rows[i]["pdesign"].retStr() + tbl.Rows[i]["doctag"].retStr() + tbl.Rows[i]["loccd"].retStr();
                                    string key = tbl.Rows[i]["blno"].retStr() + tbl.Rows[i]["bldt"].retDateStr() + tbl.Rows[i]["loccd"].retStr();
                                    while (tbl.Rows[i]["slcd"].retStr() == slcd && (tbl.Rows[i]["blno"].retStr() + tbl.Rows[i]["bldt"].retDateStr() + tbl.Rows[i]["loccd"].retStr()) == key)
                                    {

                                        //IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                        //if (reptype == "N") IR.Rows[rNo]["slno"] = islno;
                                        //IR.Rows[rNo]["styleno"] = tbl.Rows[i]["styleno"].ToString();
                                        //IR.Rows[rNo]["pdesign"] = tbl.Rows[i]["pdesign"].ToString();
                                        //IR.Rows[rNo]["uomnm"] = tbl.Rows[i]["uomnm"].ToString();
                                        //IR.Rows[rNo]["itgrpcd"] = tbl.Rows[i]["itgrpcd"].retStr();
                                        //IR.Rows[rNo]["slcd"] = tbl.Rows[i]["slcd"].retStr();
                                        //if (reptype == "S") IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"].retStr();
                                        //if (reptype == "S") IR.Rows[rNo]["barno"] = tbl.Rows[i]["barno"].retStr();
                                        //if (reptype == "S") IR.Rows[rNo]["itgrpnm"] = tbl.Rows[i]["itgrpnm"].retStr();
                                        //if (reptype == "S") IR.Rows[rNo]["blno"] = tbl.Rows[i]["blno"].retStr();
                                        //if (reptype == "S") IR.Rows[rNo]["bldt"] = tbl.Rows[i]["bldt"].retDateStr();
                                        if (tbl.Rows[i]["doctag"].retStr() == "OP")
                                        {
                                            //IR.Rows[rNo]["openingqnty"] = IR.Rows[rNo]["openingqnty"].retDbl() + tbl.Rows[i]["qnty"].retDbl();
                                            //if (MENU_PARA != "Q") IR.Rows[rNo]["openingamt"] = IR.Rows[rNo]["openingamt"].retDbl() + tbl.Rows[i]["txblval"].retDbl();
                                            opqnty += tbl.Rows[i]["qnty"].retDbl();
                                            opvalue += tbl.Rows[i]["txblval"].retDbl();
                                        }
                                        else if (tbl.Rows[i]["doctag"].retStr() == "PB")
                                        {
                                            //IR.Rows[rNo]["purchaseqnty"] = IR.Rows[rNo]["purchaseqnty"].retDbl() + tbl.Rows[i]["qnty"].retDbl();
                                            //if (MENU_PARA != "Q") IR.Rows[rNo]["purchaseamt"] = IR.Rows[rNo]["purchaseamt"].retDbl() + tbl.Rows[i]["txblval"].retDbl();
                                            pbqnty += tbl.Rows[i]["qnty"].retDbl();
                                            pbvalue += tbl.Rows[i]["txblval"].retDbl();
                                        }
                                        else if (tbl.Rows[i]["doctag"].retStr() == "PR")
                                        {
                                            //IR.Rows[rNo]["purretqnty"] = IR.Rows[rNo]["purretqnty"].retDbl() + (tbl.Rows[i]["qnty"].retDbl() * -1);
                                            //if (MENU_PARA != "Q") IR.Rows[rNo]["purretamt"] = IR.Rows[rNo]["purretamt"].retDbl() + (tbl.Rows[i]["txblval"].retDbl() * -1);
                                            prqnty += tbl.Rows[i]["qnty"].retDbl() * -1;
                                            prvalue += tbl.Rows[i]["txblval"].retDbl() * -1;
                                        }
                                        else if (tbl.Rows[i]["doctag"].retStr() == "TI")
                                        {
                                            //IR.Rows[rNo]["transferinqnty"] = IR.Rows[rNo]["transferinqnty"].retDbl() + tbl.Rows[i]["qnty"].retDbl();
                                            //if (MENU_PARA != "Q") IR.Rows[rNo]["transferinamt"] = IR.Rows[rNo]["transferinamt"].retDbl() + tbl.Rows[i]["txblval"].retDbl();
                                            tiqnty += tbl.Rows[i]["qnty"].retDbl();
                                            tivalue += tbl.Rows[i]["txblval"].retDbl();
                                        }
                                        else if (tbl.Rows[i]["doctag"].retStr() == "TO")
                                        {
                                            //IR.Rows[rNo]["transferoutqnty"] = IR.Rows[rNo]["transferoutqnty"].retDbl() + (tbl.Rows[i]["qnty"].retDbl() * -1);
                                            //if (MENU_PARA != "Q") IR.Rows[rNo]["transferoutamt"] = IR.Rows[rNo]["transferoutamt"].retDbl() + (tbl.Rows[i]["txblval"].retDbl() * -1);
                                            toqnty += tbl.Rows[i]["qnty"].retDbl() * -1;
                                            tovalue += tbl.Rows[i]["txblval"].retDbl() * -1;
                                        }
                                        else if (tbl.Rows[i]["doctag"].retStr() == "SB" || tbl.Rows[i]["doctag"].retStr() == "SR")
                                        {
                                            if (VE.Checkbox1 == true)
                                            {
                                                colnm = tbl.Rows[i]["loccd"].retStr() + "salesqnty";
                                                //IR.Rows[rNo][colnm] = IR.Rows[rNo][colnm].retDbl() + (tbl.Rows[i]["doctag"].retStr() == "SB" ? (tbl.Rows[i]["qnty"].retDbl() * -1) : tbl.Rows[i]["qnty"].retDbl());//modify by mithun
                                                //if (MENU_PARA != "Q") IR.Rows[rNo][colnm] = IR.Rows[rNo][colnm].retDbl() + (tbl.Rows[i]["txblval"].retDbl() * -1);

                                            }
                                            else
                                            {
                                                //IR.Rows[rNo]["salesqnty"] = IR.Rows[rNo]["salesqnty"].retDbl() + (tbl.Rows[i]["doctag"].retStr() == "SB" ? (tbl.Rows[i]["qnty"].retDbl() * -1) : tbl.Rows[i]["qnty"].retDbl());//modify by mithun
                                                //if (MENU_PARA != "Q") IR.Rows[rNo]["salesamt"] = IR.Rows[rNo]["salesamt"].retDbl() + (tbl.Rows[i]["txblval"].retDbl() * -1);

                                            }

                                            if (tbl.Rows[i]["doctag"].retStr() == "SB")
                                            {
                                                sbqnty += tbl.Rows[i]["qnty"].retDbl() * -1;
                                                sbvalue += tbl.Rows[i]["txblval"].retDbl() * -1;

                                            }
                                            else
                                            {
                                                srqnty += tbl.Rows[i]["qnty"].retDbl();
                                                srvalue += tbl.Rows[i]["txblval"].retDbl();
                                            }
                                        }
                                        else
                                        {
                                            othersqnty += tbl.Rows[i]["qnty"].retDbl();
                                            othersvalue += tbl.Rows[i]["txblval"].retDbl();

                                            //opqnty += tbl.Rows[i]["qnty"].retDbl();
                                            //opvalue += tbl.Rows[i]["txblval"].retDbl();
                                        }
                                        //IR.Rows[rNo]["othersqnty"] = othersqnty;
                                        //if (MENU_PARA != "Q") IR.Rows[rNo]["othersamt"] = othersvalue;

                                        //IR.Rows[rNo]["stockqnty"] = opqnty+ pbqnty+ prqnty+ tiqnty + toqnty+ sbqnty+ srqnty+ othersqnty;
                                        //if (MENU_PARA != "Q") IR.Rows[rNo]["stockamt"] = opvalue+ pbvalue+ prvalue+ tivalue+ tovalue+ sbvalue+ srvalue+ othersvalue;
                                        i++;
                                        if (i > maxB) break;
                                    }
                                    opqnty1 += opqnty;
                                    opvalue1 += opvalue;
                                    pbqnty1 += pbqnty;
                                    pbvalue1 += pbvalue;
                                    prqnty1 += prqnty;
                                    prvalue1 += prvalue;
                                    tiqnty1 += tiqnty;
                                    tivalue1 += tivalue;
                                    toqnty1 += toqnty;
                                    tovalue1 += tovalue;
                                    sbqnty1 += sbqnty;
                                    sbvalue1 += sbvalue;
                                    srqnty1 += srqnty;
                                    srvalue1 += srvalue;
                                    othersqnty1 += othersqnty;
                                    othersvalue1 += othersvalue;

                                    if (reptype == "B")
                                    {
                                        if (VE.Checkbox2 == true && sbqnty.retDbl() == 0) PrintSkip = true;
                                        if (PrintSkip == false)
                                        {
                                            islno++;
                                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                            if (reptype == "D") IR.Rows[rNo]["slno"] = islno;
                                            IR.Rows[rNo]["blno"] = tbl.Rows[i - 1]["blno"].retStr();
                                            IR.Rows[rNo]["bldt"] = tbl.Rows[i - 1]["bldt"].retDateStr();

                                            //IR.Rows[rNo]["styleno"] = tbl.Rows[i - 1]["styleno"].ToString();
                                            //IR.Rows[rNo]["pdesign"] = tbl.Rows[i - 1]["pdesign"].ToString();
                                            //IR.Rows[rNo]["uomnm"] = tbl.Rows[i - 1]["uomnm"].ToString();
                                            //IR.Rows[rNo]["itgrpcd"] = tbl.Rows[i - 1]["itgrpcd"].retStr();
                                            // IR.Rows[rNo]["slcd"] = tbl.Rows[i - 1]["slcd"].retStr();
                                            IR.Rows[rNo]["slnm"] = tbl.Rows[i - 1]["slnm"].retStr();

                                            //IR.Rows[rNo]["itgrpnm"] = tbl.Rows[i - 1]["itgrpnm"].retStr();
                                            IR.Rows[rNo]["openingqnty"] = opqnty;
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["openingamt"] = opvalue;

                                            IR.Rows[rNo]["purchaseqnty"] = pbqnty;
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["purchaseamt"] = pbvalue;

                                            IR.Rows[rNo]["purretqnty"] = prqnty;
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["purretamt"] = prvalue;

                                            IR.Rows[rNo]["transferinqnty"] = tiqnty;
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["transferinamt"] = tivalue;

                                            IR.Rows[rNo]["transferoutqnty"] = toqnty;
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["transferoutamt"] = tovalue;

                                            if (VE.Checkbox1 == true)
                                            {
                                                //colnm = tbl.Rows[i-1]["loccd"].retStr() + "salesqnty";
                                                if (colnm.retStr() != "") IR.Rows[rNo][colnm] = sbqnty;

                                                if (colnm.retStr() != "") if (MENU_PARA != "Q") IR.Rows[rNo][colnm] = sbvalue;

                                            }
                                            else
                                            {
                                                IR.Rows[rNo]["salesqnty"] = sbqnty;
                                                if (MENU_PARA != "Q") IR.Rows[rNo]["salesamt"] = sbvalue;
                                            }



                                            IR.Rows[rNo]["othersqnty"] = othersqnty;
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["othersamt"] = othersvalue;

                                            IR.Rows[rNo]["stockqnty"] = (opqnty + pbqnty + tiqnty + srqnty + othersqnty) - (prqnty + toqnty + sbqnty);
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["stockamt"] = (opvalue + pbvalue + tivalue + srvalue + othersvalue) - (prvalue + tovalue + sbvalue);
                                        }
                                        else
                                        {
                                            if (scount == 0)
                                            {
                                                //if(IR.Rows.Count>0) IR.Rows.RemoveAt(IR.Rows.Count - 1); IR.AcceptChanges();

                                            }
                                        }
                                    }

                                    if (i > maxB) break;
                                }
                                if (reptype == "D")
                                {
                                    //i = indx;

                                    if (VE.Checkbox2 == true && sbqnty1.retDbl() == 0) PrintSkip = true;
                                    if (PrintSkip == false)
                                    {
                                        scount++; cntgrp++; cntslcd++;
                                        islno++;
                                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                        if (reptype == "D") IR.Rows[rNo]["slno"] = islno;
                                        IR.Rows[rNo]["slcd"] = tbl.Rows[indx]["slcd"].retStr();
                                        IR.Rows[rNo]["barno"] = tbl.Rows[indx]["barno"].ToString();
                                        IR.Rows[rNo]["pdesign"] = tbl.Rows[indx]["pdesign"].ToString();
                                        IR.Rows[rNo]["uomnm"] = tbl.Rows[indx]["uomnm"].ToString();
                                        IR.Rows[rNo]["itgrpcd"] = tbl.Rows[indx]["itgrpcd"].retStr();
                                        IR.Rows[rNo]["slcd"] = tbl.Rows[indx]["slcd"].retStr();
                                        IR.Rows[rNo]["blno"] = tbl.Rows[indx]["blno"].retStr();
                                        IR.Rows[rNo]["bldt"] = tbl.Rows[indx]["bldt"].retDateStr();
                                        IR.Rows[rNo]["openingqnty"] = opqnty1;
                                        if (MENU_PARA != "Q") IR.Rows[rNo]["openingamt"] = opvalue1;

                                        IR.Rows[rNo]["purchaseqnty"] = pbqnty1;
                                        if (MENU_PARA != "Q") IR.Rows[rNo]["purchaseamt"] = pbvalue1;

                                        IR.Rows[rNo]["purretqnty"] = prqnty1;
                                        if (MENU_PARA != "Q") IR.Rows[rNo]["purretamt"] = prvalue1;

                                        IR.Rows[rNo]["transferinqnty"] = tiqnty1;
                                        if (MENU_PARA != "Q") IR.Rows[rNo]["transferinamt"] = tivalue1;

                                        IR.Rows[rNo]["transferoutqnty"] = toqnty1;
                                        if (MENU_PARA != "Q") IR.Rows[rNo]["transferoutamt"] = tovalue1;

                                        if (VE.Checkbox1 == true)
                                        {
                                            //colnm = tbl.Rows[i-1]["loccd"].retStr() + "salesqnty";
                                            if (colnm.retStr() != "") IR.Rows[rNo][colnm] = sbqnty1;

                                            if (colnm.retStr() != "") if (MENU_PARA != "Q") IR.Rows[rNo][colnm] = sbvalue1;

                                        }
                                        else
                                        {
                                            IR.Rows[rNo]["salesqnty"] = sbqnty1;
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["salesamt"] = sbvalue1;
                                        }



                                        IR.Rows[rNo]["othersqnty"] = othersqnty1;
                                        if (MENU_PARA != "Q") IR.Rows[rNo]["othersamt"] = othersvalue1;

                                        IR.Rows[rNo]["stockqnty"] = (opqnty1 + pbqnty1 + tiqnty1 + srqnty1 + othersqnty1) - (prqnty1 + toqnty1 + sbqnty1);

                                        if (MENU_PARA != "Q") IR.Rows[rNo]["stockamt"] = (opvalue1 + pbvalue1 + tivalue1 + srvalue1 + othersvalue1) - (prvalue1 + tovalue1 + sbvalue1);
                                    }
                                    else
                                    {
                                        if (scount == 0)
                                        {
                                            //if(IR.Rows.Count>0) IR.Rows.RemoveAt(IR.Rows.Count - 1); IR.AcceptChanges();

                                        }
                                    }

                                }
                                if (i > maxB) break;
                            }
                            if (scount == 0)
                            {
                                //if (IR.Rows.Count > 0) IR.Rows.RemoveAt(IR.Rows.Count - 1); IR.AcceptChanges();

                            }
                            if (i > maxB) break;

                            if (scount == 0)
                            {
                                //if (IR.Rows.Count > 0) IR.Rows.RemoveAt(IR.Rows.Count - 1); IR.AcceptChanges();

                            }
                        }
                        if (reptype == "D")
                        {
                            if (scount == 0 && cntgrp == 0)
                            {

                                IR.Rows.RemoveAt(IR.Rows.Count - 1); IR.AcceptChanges();
                            }

                            if (cntgrp != 0)
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["styleno"] = "Total of " + tbl.Rows[indx]["itgrpnm"].ToString();
                                IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;";

                                itgrpcd = tbl.Rows[indx]["itgrpcd"].ToString();
                                slcd = tbl.Rows[indx]["slcd"].ToString();
                                if (slcd == "OV00004")
                                {

                                }
                                cnt = 0;

                                var uomlist = IR.AsEnumerable().Where(g => g.Field<string>("uomnm").retStr() != "" && g.Field<string>("itgrpcd").retStr() == itgrpcd && g.Field<string>("slcd").retStr() == slcd).Select(a => a.Field<string>("uomnm")).Distinct().ToList();
                                if (uomlist != null && uomlist.Count > 0)
                                {
                                    for (int x = 0; x < uomlist.Count; x++)
                                    {
                                        if (cnt != 0)
                                        {
                                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                        }
                                        string uomnm = uomlist[x].retStr();
                                        IR.Rows[rNo]["uomnm"] = uomnm;
                                        int strart = reptype == "N" ? 9 : 10;
                                        for (int a = strart; a < IR.Columns.Count - 2; a++)
                                        {
                                            var unitwisegrptotal = IR.AsEnumerable().Where(g => g.Field<string>("uomnm").retStr() != "" && g.Field<string>("itgrpcd").retStr() == itgrpcd && g.Field<string>("slcd").retStr() == slcd && g.Field<string>("uomnm").retStr() == uomnm)
                                                   .GroupBy(g => g.Field<string>("uomnm"))
                                                   .Select(g =>
                                                   {
                                                       var row = IR.NewRow();
                                                       row["uomnm"] = g.Key;
                                                       row[IR.Columns[a].ColumnName] = g.Sum(r => r.Field<double?>(IR.Columns[a].ColumnName) == null ? 0 : r.Field<double>(IR.Columns[a].ColumnName));
                                                       return row;
                                                   }).CopyToDataTable();

                                            if (unitwisegrptotal != null && unitwisegrptotal.Rows.Count > 0)
                                            {
                                                IR.Rows[rNo][IR.Columns[a].ColumnName] = unitwisegrptotal.Rows[0][IR.Columns[a].ColumnName];
                                            }
                                        }
                                        cnt++;
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
                            }
                        }
                        if (i > maxB) break;
                    }
                    if (reptype == "D")
                    {
                        if (scount == 0 && cntslcd == 0)
                        {

                            IR.Rows.RemoveAt(IR.Rows.Count - 1); IR.AcceptChanges();
                        }
                        if (cntslcd != 0)
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["styleno"] = "Total of " + tbl.Rows[indx]["slnm"].ToString();
                            IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;";

                            slcd = tbl.Rows[indx]["slcd"].ToString();
                            if (slcd == "OV00004")
                            {
                            }


                            cnt = 0;
                            var uomlist1 = IR.AsEnumerable().Where(g => g.Field<string>("uomnm").retStr() != "" && g.Field<string>("slcd").retStr() == slcd).Select(a => a.Field<string>("uomnm")).Distinct().ToList();
                            if (uomlist1 != null && uomlist1.Count > 0)
                            {
                                for (int x = 0; x < uomlist1.Count; x++)
                                {
                                    if (cnt != 0)
                                    {
                                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                    }
                                    string uomnm = uomlist1[x].retStr();
                                    IR.Rows[rNo]["uomnm"] = uomnm;
                                    int strart = reptype == "N" ? 9 : 10;
                                    for (int a = strart; a < IR.Columns.Count - 2; a++)
                                    {
                                        var unitwisegrptotal = IR.AsEnumerable().Where(g => g.Field<string>("uomnm").retStr() != "" && g.Field<string>("slcd").retStr() == slcd && g.Field<string>("uomnm").retStr() == uomnm)
                                               .GroupBy(g => g.Field<string>("uomnm"))
                                               .Select(g =>
                                               {
                                                   var row = IR.NewRow();
                                                   row["uomnm"] = g.Key;
                                                   row[IR.Columns[a].ColumnName] = g.Sum(r => r.Field<double?>(IR.Columns[a].ColumnName) == null ? 0 : r.Field<double>(IR.Columns[a].ColumnName));
                                                   return row;
                                               }).CopyToDataTable();

                                        if (unitwisegrptotal != null && unitwisegrptotal.Rows.Count > 0)
                                        {
                                            IR.Rows[rNo][IR.Columns[a].ColumnName] = unitwisegrptotal.Rows[0][IR.Columns[a].ColumnName];
                                        }
                                    }
                                    cnt++;
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
                        }
                    }
                    if (i > maxB) break;
                }

                IR.Columns.Remove("itgrpcd");
                IR.Columns.Remove("slcd");
                string pghdr1 = "";
                string repname = ("Rep_Supplier").retRepname();

                pghdr1 = (MENU_PARA != "Q" ? "Supplier Wise Report with value from " : "Supplier wise Sales & Stock from ") + fdt + " to " + tdt;
                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);

                TempData[repname] = PV;
                TempData[repname + "xxx"] = IR;
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
            return null;
        }


        public ActionResult retPurchaseWiseStock(string scm, string scmf, string fdt, string tdt, string LOC, string COM, string slcd = "", string itgrpcd = "", string loccd = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string sql = "";
            sql += Environment.NewLine + " select e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign, a.doctag,a.docdt,a.docno, f.itgrpcd, h.itgrpnm, f.itnm, nvl(e.pdesign, f.styleno) styleno, ";
            sql += Environment.NewLine + " e.othrate, nvl(b.rate, 0) oprate, nvl(cp.rate, 0) clrate, nvl(rp.rate, 0) rprate, f.uomcd, i.uomnm, i.decimals, g.itnm fabitnm, h.sapcode,a.prefno,a.prefdt,a.shade,a.sizecd, ";
            sql += Environment.NewLine + " sum(a.nos) nos,sum(a.qnty)qnty, sum(a.txblval)txblval, sum(a.othramt)othramt,sum(a.discamt)discamt ";
            sql += Environment.NewLine + " from  ( ";
            sql += Environment.NewLine + "select d.compcd, d.loccd, a.barno, 'OP' doctag, d.docdt, d.docno, c.prefno, c.prefdt, a.shade, sum(a.nos)nos, ";
            sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, sum(a.txblval) txblval, ";
            sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt, nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0) discamt, b.sizecd ";

            sql += Environment.NewLine + "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e ,SD_SACHI2021.t_txndtl f ";
            sql += Environment.NewLine + "where a.barno = b.barno(+)  and  c.autono = f.barno(+)  ";
            sql += Environment.NewLine + "and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' ";
            sql += Environment.NewLine + "and e.doctype not in ('KHSR') and a.stkdrcr in ('D', 'C') and d.docdt < to_date('" + fdt + "', 'dd/mm/yyyy') ";
            sql += Environment.NewLine + "group by d.compcd, d.loccd, a.barno, 'OP',d.docdt,d.docno,c.prefno,c.prefdt,a.shade,nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0),b.sizecd ";

            sql += Environment.NewLine + "union all select d.compcd, d.loccd, a.barno, c.doctag,d.docdt,d.docno,c.prefno,c.prefdt,a.shade,sum(a.nos)nos, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, ";
            sql += Environment.NewLine + "sum(a.txblval) txblval, ";
            sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt,nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0) discamt ,b.sizecd ";
            sql += Environment.NewLine + "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e,SD_SACHI2021.t_txndtl f  ";
            sql += Environment.NewLine + "where a.barno = b.barno(+)  and  c.autono = f.barno(+)  ";
            sql += Environment.NewLine + "and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and d.compcd = '" + COM + "' ";
            sql += Environment.NewLine + "and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N')= 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D','C') and d.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') ";
            sql += Environment.NewLine + "and d.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') ";
            sql += Environment.NewLine + "group by d.compcd, d.loccd, a.barno, c.doctag,d.docdt,d.docno,c.prefno,c.prefdt,a.shade,nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0),b.sizecd ";
            sql += Environment.NewLine + " ) a, ";

            sql += Environment.NewLine + "(select barno, effdt, prccd, rate ";
            sql += Environment.NewLine + "from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn from " + scm + ".t_batchmst_price a ";
            sql += Environment.NewLine + "where a.effdt <= to_date('" + fdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) b,  (select barno, effdt, prccd, rate ";
            sql += Environment.NewLine + "from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += Environment.NewLine + "from " + scm + ".t_batchmst_price a where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) cp, ";
            sql += Environment.NewLine + "(select barno, effdt, prccd, rate from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += Environment.NewLine + "from " + scm + ".t_batchmst_price a where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'RP' ) where rn = 1) rp, ";
            sql += Environment.NewLine + "" + scm + ".t_batchmst e, " + scm + ".m_sitem f, " + scm + ".m_sitem g, " + scm + ".m_group h, " + scmf + ".m_uom i, " + scmf + ".m_subleg j, " + scmf + ".m_loca k ";
            sql += Environment.NewLine + ", " + scm + ".m_size l ";
            sql += Environment.NewLine + "where a.barno = e.barno(+) and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and a.barno = b.barno(+) ";
            sql += Environment.NewLine + "and a.barno = cp.barno(+)and a.barno = rp.barno(+)  and e.slcd = j.slcd(+) and a.compcd || a.loccd = k.compcd || k.loccd and f.itgrpcd = h.itgrpcd(+) ";
            sql += Environment.NewLine + "and f.uomcd = i.uomcd(+) and a.sizecd = l.sizecd(+) ";

            #region Check for stock this is not need
            sql += Environment.NewLine + "and a.barno in ( ";
            sql += Environment.NewLine + "select barno from  ( ";
            sql += Environment.NewLine + "select  a.barno, sum(a.qnty)qnty ";
            sql += Environment.NewLine + "from( ";
            sql += Environment.NewLine + "select d.compcd, d.loccd, a.barno, d.docdt, d.docno, c.prefno, c.prefdt, a.shade, ";
            sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval, ";
            sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt, b.sizecd ";

            sql += Environment.NewLine + "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e ";
            sql += Environment.NewLine + "where a.barno = b.barno(+) ";
            sql += Environment.NewLine + "and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' ";
            sql += Environment.NewLine + "and e.doctype not in ('KHSR') and a.stkdrcr in ('D', 'C') and d.docdt < to_date('" + fdt + "', 'dd/mm/yyyy') ";
            sql += Environment.NewLine + "group by d.compcd, d.loccd, a.barno, 'OP', d.docdt, d.docno, c.prefno, c.prefdt, a.shade, b.sizecd ";

            sql += Environment.NewLine + "union all select d.compcd, d.loccd, a.barno, d.docdt, d.docno, c.prefno, c.prefdt, a.shade, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, ";
            sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval, ";
            sql += Environment.NewLine + "  sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt, b.sizecd ";
            sql += Environment.NewLine + "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e ";
            sql += Environment.NewLine + "where a.barno = b.barno(+) ";

            sql += Environment.NewLine + "and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and d.compcd = '" + COM + "' ";
            sql += Environment.NewLine + "and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D', 'C') and d.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') ";
            sql += Environment.NewLine + " and d.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') group by d.compcd, d.loccd, a.barno,d.docdt,d.docno,c.prefno,c.prefdt,a.shade,b.sizecd ";
            sql += Environment.NewLine + " ) a, ";
            sql += Environment.NewLine + "(select barno, effdt, prccd, rate ";
            sql += Environment.NewLine + "from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn from " + scm + ".t_batchmst_price a ";
            sql += Environment.NewLine + "where a.effdt <= to_date('" + fdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) b,  (select barno, effdt, prccd, rate ";
            sql += Environment.NewLine + "from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += Environment.NewLine + "from " + scm + ".t_batchmst_price a where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) cp, ";
            sql += Environment.NewLine + "(select barno, effdt, prccd, rate from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += Environment.NewLine + "from " + scm + ".t_batchmst_price a where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'RP' ) where rn = 1) rp, ";
            sql += Environment.NewLine + "" + scm + ".t_batchmst e, " + scm + ".m_sitem f, " + scm + ".m_sitem g, " + scm + ".m_group h, " + scmf + ".m_uom i, " + scmf + ".m_subleg j, " + scmf + ".m_loca k ";
            sql += Environment.NewLine + ", " + scm + ".m_size l ";
            sql += Environment.NewLine + "where a.barno = e.barno(+) and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and a.barno = b.barno(+) ";
            sql += Environment.NewLine + "and a.barno = cp.barno(+)and a.barno = rp.barno(+)  and e.slcd = j.slcd(+) and a.compcd || a.loccd = k.compcd || k.loccd and f.itgrpcd = h.itgrpcd(+) ";
            sql += Environment.NewLine + "and f.uomcd = i.uomcd(+) and a.sizecd = l.sizecd(+) ";
            sql += Environment.NewLine + "group by ";
            sql += Environment.NewLine + "e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign,a.docdt,a.docno,f.itgrpcd, h.itgrpnm, f.itnm, nvl(e.pdesign, f.styleno), ";
            sql += Environment.NewLine + " e.othrate, nvl(b.rate, 0) , nvl(cp.rate, 0) , nvl(rp.rate, 0) , f.uomcd, i.uomnm, i.decimals, g.itnm ,h.sapcode,a.prefno,a.prefdt,a.shade,a.sizecd ";
            sql += Environment.NewLine + ") summ ";
            sql += Environment.NewLine + "where summ.qnty > 0 ";
            sql += Environment.NewLine + ")  ";
            #endregion end of stock check

            if (slcd.retStr() != "") sql += Environment.NewLine + "and e.slcd in (" + slcd + ") ";
            if (itgrpcd.retStr() != "") sql += "and f.itgrpcd in (" + itgrpcd + ") ";
            if (loccd.retStr() != "") sql += Environment.NewLine + "and a.loccd in (" + loccd + ") ";
            sql += Environment.NewLine + "group by ";
            sql += " e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign, a.doctag,a.docdt,a.docno,f.itgrpcd, h.itgrpnm, f.itnm, nvl(e.pdesign, f.styleno), ";
            sql += " e.othrate, nvl(b.rate, 0) , nvl(cp.rate, 0) , nvl(rp.rate, 0) , f.uomcd, i.uomnm, i.decimals, g.itnm ,h.sapcode,a.prefno,a.prefdt,a.shade,a.sizecd ";
            sql += Environment.NewLine + "order by barno ";
            DataTable Purtbl = MasterHelp.SQLquery(sql);
            if (Purtbl.Rows.Count == 0) return Content("no records..");
            ExcelPackage ExcelPkg = new ExcelPackage();
            ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("sheet1");
            string filename = "PurchasebillwiseStock".retRepname();
            string Excel_Header = "TYPE" + "|" + "Date" + "|" + "BARNO" + "|" + "CMNO" + "|" + "Matl Group" + "|" + "Dv" + "|" + "INVOICE" + "|" + "INVDATE" + "|" + "Material" + "|" + "SHADE" + "|" + "Grv" + "|" + "USP" + "|" + "QTY" + "|" + "MRP" + "|" + "Gross Value" + "|" + "Discount" + "|" + "Net Value";
            using (ExcelRange Rng = wsSheet1.Cells["A1:Q1"])
            {
                Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                Rng.Style.Font.Size = 14; Rng.Style.Font.Bold = true;
                wsSheet1.Cells["A1:A1"].Value = CommVar.CompName(UNQSNO);
            }
            using (ExcelRange Rng = wsSheet1.Cells["A2:Q2"])
            {
                Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                Rng.Style.Font.Size = 12; Rng.Style.Font.Bold = true;
                wsSheet1.Cells["A2:A2"].Value = CommVar.LocName(UNQSNO);
            }
            using (ExcelRange Rng = wsSheet1.Cells["A3:Q3"])
            {
                Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                Rng.Style.Font.Bold = true; Rng.Style.Font.Size = 9;
                Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SkyBlue);
                Rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                Rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                string[] Header = Excel_Header.Split('|');
                for (int j = 0; j < Header.Length; j++)
                {
                    wsSheet1.Cells[3, j + 1].Value = Header[j];
                }
            }

            int exlrowno = 4;
            for (int i = 0; i < Purtbl.Rows.Count; i++)
            {
                wsSheet1.Cells[exlrowno, 1].Value = "";
                wsSheet1.Cells[exlrowno, 2].Value = Purtbl.Rows[i]["docdt"].retDateStr("yyyy", "dd/MM/yyyy");//docdt
                wsSheet1.Cells[exlrowno, 3].Value = Purtbl.Rows[i]["barno"].retStr();
                wsSheet1.Cells[exlrowno, 4].Value = Purtbl.Rows[i]["docno"].retStr();//docno
                wsSheet1.Cells[exlrowno, 5].Value = Purtbl.Rows[i]["itgrpnm"].retStr();
                wsSheet1.Cells[exlrowno, 6].Value = Purtbl.Rows[i]["sapcode"].retStr();//sapcode
                wsSheet1.Cells[exlrowno, 7].Value = Purtbl.Rows[i]["prefno"].retStr();//prefno
                wsSheet1.Cells[exlrowno, 8].Value = Purtbl.Rows[i]["prefdt"].retDateStr("yyyy", "dd/MM/yyyy");//prefdt
                wsSheet1.Cells[exlrowno, 9].Value = Purtbl.Rows[i]["styleno"].retStr();
                wsSheet1.Cells[exlrowno, 10].Value = Purtbl.Rows[i]["shade"].retStr();//shade
                wsSheet1.Cells[exlrowno, 11].Value = Purtbl.Rows[i]["sizecd"].retStr();//sizecd
                wsSheet1.Cells[exlrowno, 12].Value = "";
                wsSheet1.Cells[exlrowno, 13].Value = Purtbl.Rows[i]["qnty"].retDbl();
                wsSheet1.Cells[exlrowno, 14].Value = Purtbl.Rows[i]["rprate"].retDbl();
                wsSheet1.Cells[exlrowno, 15].Value = Purtbl.Rows[i]["txblval"].retDbl();
                wsSheet1.Cells[exlrowno, 16].Value = Purtbl.Rows[i]["discamt"].retDbl();
                wsSheet1.Cells[exlrowno, 17].Value = Purtbl.Rows[i]["txblval"].retDbl() - (Purtbl.Rows[i]["discamt"].retDbl() + Purtbl.Rows[i]["othramt"].retDbl());
                exlrowno++;
            }
            // }
            //for download//
            Response.Clear();
            Response.ClearContent();
            Response.Buffer = true;
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + filename + ".xlsx");
            Response.BinaryWrite(ExcelPkg.GetAsByteArray());
            Response.Flush();
            Response.Close();
            Response.End();
            return Content("Downloded");
        }

        public ActionResult GetSalesfromSupplier(string scm, string scmf, string fdt, string tdt, string LOC, string COM, string slcd = "", string itgrpcd = "", string loccd = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string sql = "";
            sql += " select e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign, a.doctag,a.docdt,a.docno, f.itgrpcd, h.itgrpnm, f.itnm, nvl(e.pdesign, f.styleno) styleno, f.BRANDCD,";
            sql += Environment.NewLine + " e.othrate, nvl(b.rate, 0) oprate, nvl(cp.rate, 0) clrate, nvl(rp.rate, 0) rprate, f.uomcd, i.uomnm, i.decimals, g.itnm fabitnm, h.sapcode,a.prefno,a.prefdt,a.shade,a.sizecd, ";
            sql += Environment.NewLine + " sum(a.nos) nos,sum(a.qnty)qnty,sum(d.amt) basamt, sum(a.txblval)txblval,d.NETAMT,sum(a.othramt)othramt,sum(nvl(d.TDDISCAMT, 0) + nvl(d.SCMDISCAMT, 0) + nvl(d.DISCAMT, 0)) discamt,";
            sql += Environment.NewLine + " sum(NVL(CGSTPER,0)) CGSTPER,sum(NVL(SGSTPER,0)) SGSTPER,sum(NVL(IGSTPER,0)) IGSTPER , sum(d.cgstamt)cgstamt, sum(d.sgstamt)sgstamt, sum(d.cessamt)cessamt ";
            sql += Environment.NewLine + " from  ( ";
            //sql += "select d.compcd, d.loccd, a.barno, 'OP' doctag, d.docdt, d.docno, c.prefno, c.prefdt, a.shade, sum(a.nos)nos, ";
            //sql += "sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, sum(a.txblval) txblval, ";
            //sql += "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt, nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0) discamt, b.sizecd ";

            //sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e, " + scm + ".t_txndtl f ";
            //sql += "where a.barno = b.barno(+) and c.autono = f.autono ";
            //sql += "and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' ";
            //sql += "and e.doctype not in ('KHSR') and a.stkdrcr in ('D', 'C') and d.docdt < to_date('" + fdt + "', 'dd/mm/yyyy') ";
            //sql += "group by d.compcd, d.loccd, a.barno, 'OP',d.docdt,d.docno,c.prefno,c.prefdt,a.shade,nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0),b.sizecd ";

            //sql += "union all select d.compcd, d.loccd, a.barno, c.doctag,d.docdt,d.docno,c.prefno,c.prefdt,a.shade,sum(a.nos)nos, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, ";
            sql += Environment.NewLine + "select a.autono,a.txnslno,d.compcd, d.loccd, a.barno, c.doctag,d.docdt,d.docno,c.prefno,c.prefdt,a.shade,sum(a.nos)nos, sum(a.qnty) qnty, ";
            sql += Environment.NewLine + "sum(a.txblval) txblval, ";
            sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt,b.sizecd ";
            sql += Environment.NewLine + "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e ";
            sql += Environment.NewLine + "where a.barno = b.barno(+)  ";
            sql += Environment.NewLine + "and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and d.compcd = '" + COM + "' ";//ONLY SALES WILL COME
            sql += Environment.NewLine + "and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N')= 'N' and e.doctype not in ('KHSR')  and e.doctype in ('SB','SBDIR','SBCM')  and a.stkdrcr in ('D','C') and d.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') ";
            sql += Environment.NewLine + "and d.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') ";
            sql += Environment.NewLine + "group by  a.autono,a.txnslno,d.compcd, d.loccd, a.barno, c.doctag,d.docdt,d.docno,c.prefno,c.prefdt,a.shade,b.sizecd ";
            sql += Environment.NewLine + " ) a, ";

            sql += Environment.NewLine + "(select barno, effdt, prccd, rate ";
            sql += Environment.NewLine + "from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn from " + scm + ".t_batchmst_price a ";
            sql += Environment.NewLine + "where a.effdt <= to_date('" + fdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) b,  (select barno, effdt, prccd, rate ";
            sql += Environment.NewLine + "from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += Environment.NewLine + "from " + scm + ".t_batchmst_price a where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) cp, ";
            sql += Environment.NewLine + "(select barno, effdt, prccd, rate from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += Environment.NewLine + "from " + scm + ".t_batchmst_price a where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'RP' ) where rn = 1) rp, ";
            sql += Environment.NewLine + " " + scm + ".t_txndtl d," + scm + ".t_batchmst e, " + scm + ".m_sitem f, " + scm + ".m_sitem g, " + scm + ".m_group h, " + scmf + ".m_uom i, " + scmf + ".m_subleg j, " + scmf + ".m_loca k ";
            sql += Environment.NewLine + ", " + scm + ".m_size l";
            sql += Environment.NewLine + "where  a.autono||a.txnslno = d.autono||d.slno and a.barno = e.barno(+) and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and a.barno = b.barno(+) ";
            sql += Environment.NewLine + "and a.barno = cp.barno(+)and a.barno = rp.barno(+)  and e.slcd = j.slcd(+) and a.compcd || a.loccd = k.compcd || k.loccd and f.itgrpcd = h.itgrpcd(+) ";
            sql += Environment.NewLine + "and f.uomcd = i.uomcd(+) and a.sizecd = l.sizecd(+) ";
            sql += Environment.NewLine + "and a.barno in ( ";
            sql += Environment.NewLine + "select barno from  ( ";
            sql += Environment.NewLine + "select  a.barno, sum(a.qnty)qnty ";
            sql += Environment.NewLine + "from( ";
            sql += Environment.NewLine + "select d.compcd, d.loccd, a.barno, d.docdt, d.docno, c.prefno, c.prefdt, a.shade, ";
            sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval, ";
            sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt, nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0) discamt, b.sizecd ";

            sql += Environment.NewLine + "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e, " + scm + ".t_txndtl f ";
            sql += Environment.NewLine + "where a.barno = b.barno(+) and c.autono = f.autono ";
            sql += Environment.NewLine + "and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' ";
            sql += Environment.NewLine + "and e.doctype not in ('KHSR') and a.stkdrcr in ('D', 'C') and d.docdt < to_date('" + fdt + "', 'dd/mm/yyyy') ";
            sql += Environment.NewLine + "group by d.compcd, d.loccd, a.barno, 'OP', d.docdt, d.docno, c.prefno, c.prefdt, a.shade, nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0), b.sizecd ";

            sql += Environment.NewLine + "union all select d.compcd, d.loccd, a.barno, d.docdt, d.docno, c.prefno, c.prefdt, a.shade, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, ";
            sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval, ";
            sql += Environment.NewLine + "  sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt, nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0) discamt, b.sizecd ";
            sql += Environment.NewLine + "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e, " + scm + ".t_txndtl f ";
            sql += Environment.NewLine + "where a.barno = b.barno(+)  and c.autono = f.autono ";

            sql += Environment.NewLine + "and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and d.compcd = '" + COM + "' ";
            sql += Environment.NewLine + "and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D', 'C') and d.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') ";
            sql += Environment.NewLine + " and d.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') group by d.compcd, d.loccd, a.barno,d.docdt,d.docno,c.prefno,c.prefdt,a.shade,nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0),b.sizecd ";
            sql += Environment.NewLine + " ) a, ";
            sql += Environment.NewLine + "(select barno, effdt, prccd, rate ";
            sql += Environment.NewLine + "from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn from " + scm + ".t_batchmst_price a ";
            sql += Environment.NewLine + "where a.effdt <= to_date('" + fdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) b,  (select barno, effdt, prccd, rate ";
            sql += Environment.NewLine + "from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += Environment.NewLine + "from " + scm + ".t_batchmst_price a where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) cp, ";
            sql += Environment.NewLine + "(select barno, effdt, prccd, rate from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += Environment.NewLine + "from " + scm + ".t_batchmst_price a where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'RP' ) where rn = 1) rp, ";
            sql += Environment.NewLine + "" + scm + ".t_batchmst e, " + scm + ".m_sitem f, " + scm + ".m_sitem g, " + scm + ".m_group h, " + scmf + ".m_uom i, " + scmf + ".m_subleg j, " + scmf + ".m_loca k ";
            sql += Environment.NewLine + ", " + scm + ".m_size l ";
            sql += Environment.NewLine + "where a.barno = e.barno(+) and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and a.barno = b.barno(+) ";
            sql += Environment.NewLine + "and a.barno = cp.barno(+)and a.barno = rp.barno(+)  and e.slcd = j.slcd(+) and a.compcd || a.loccd = k.compcd || k.loccd and f.itgrpcd = h.itgrpcd(+) ";
            sql += Environment.NewLine + "and f.uomcd = i.uomcd(+) and a.sizecd = l.sizecd(+) ";
            if (slcd.retStr() != "") sql += " and e.slcd in (" + slcd + ") ";
            sql += Environment.NewLine + "group by ";
            sql += Environment.NewLine + "e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign,a.docdt,a.docno,f.itgrpcd, h.itgrpnm, f.itnm, nvl(e.pdesign, f.styleno), ";
            sql += Environment.NewLine + " e.othrate, nvl(b.rate, 0) , nvl(cp.rate, 0) , nvl(rp.rate, 0) , f.uomcd, i.uomnm, i.decimals, g.itnm ,h.sapcode,a.prefno,a.prefdt,a.shade,a.sizecd ";
            sql += Environment.NewLine + ") summ ";
            sql += Environment.NewLine + ")  ";
            if (itgrpcd.retStr() != "") sql += "and f.itgrpcd in (" + itgrpcd + ") ";
            if (loccd.retStr() != "") sql += "and a.loccd in (" + loccd + ") ";
            sql += Environment.NewLine + "group by ";
            sql += Environment.NewLine + " e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign, a.doctag,a.docdt,a.docno,f.itgrpcd, h.itgrpnm, f.itnm, nvl(e.pdesign, f.styleno),f.BRANDCD, ";
            sql += Environment.NewLine + " e.othrate, nvl(b.rate, 0) , nvl(cp.rate, 0) , nvl(rp.rate, 0) , f.uomcd, i.uomnm, i.decimals, g.itnm ,h.sapcode,a.prefno,a.prefdt,a.shade,a.sizecd ,d.NETAMT,(nvl(d.TDDISCAMT, 0) + nvl(d.SCMDISCAMT, 0) + nvl(d.DISCAMT, 0))";
            sql += Environment.NewLine + "order by docdt,barno ";
            DataTable tbl = MasterHelp.SQLquery(sql);
            if (tbl.Rows.Count == 0) return Content("no records..");

            ExcelPackage ExcelPkg = new ExcelPackage();
            ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("sheet1");
            string Excel_Header = "TYPE" + "|" + "Date" + "|" + "BARNO" + "|" + "CMNO" + "|" + "Matl Group" + "|" + "Dv" + "|" + "INVOICE" + "|" + "INVDATE" + "|" + "Material" + "|" + "SHADE" + "|" + "Grv" + "|" + "USP" + "|" + "QTY" + "|" + "MRP" + "|" + "Gross Value" + "|" + "Discount" + "|" + "Net Value";
            using (ExcelRange Rng = wsSheet1.Cells["A1:Q1"])
            {
                Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                Rng.Style.Font.Size = 14; Rng.Style.Font.Bold = true;
                wsSheet1.Cells["A1:A1"].Value = CommVar.CompName(UNQSNO);
            }
            using (ExcelRange Rng = wsSheet1.Cells["A2:Q2"])
            {
                Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                Rng.Style.Font.Size = 12; Rng.Style.Font.Bold = true;
                wsSheet1.Cells["A2:A2"].Value = CommVar.LocName(UNQSNO);
            }
            using (ExcelRange Rng = wsSheet1.Cells["A3:Q3"])
            {
                Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                Rng.Style.Font.Bold = true; Rng.Style.Font.Size = 9;
                Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SkyBlue);
                Rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                Rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                string[] Header = Excel_Header.Split('|');
                for (int j = 0; j < Header.Length; j++)
                {
                    wsSheet1.Cells[3, j + 1].Value = Header[j];
                }
            }

            int exlrowno = 4;
            for (int i = 0; i < tbl.Rows.Count; i++)
            {
                double gstper = tbl.Rows[i]["CGSTPER"].retDbl() + tbl.Rows[i]["SGSTPER"].retDbl() + tbl.Rows[i]["IGSTPER"].retDbl();
                double INCLDISCOUNTAMT = ((tbl.Rows[i]["discamt"].retDbl() * (gstper + 100)) / 100).retDbl().toRound(2);

                wsSheet1.Cells[exlrowno, 1].Value = "";
                wsSheet1.Cells[exlrowno, 2].Value = tbl.Rows[i]["docdt"].retDateStr("yyyy", "dd/MM/yyyy");//docdt
                wsSheet1.Cells[exlrowno, 3].Value = tbl.Rows[i]["barno"].retStr();
                wsSheet1.Cells[exlrowno, 4].Value = tbl.Rows[i]["docno"].retStr();//docno
                wsSheet1.Cells[exlrowno, 5].Value = tbl.Rows[i]["itgrpnm"].retStr();
                wsSheet1.Cells[exlrowno, 6].Value = tbl.Rows[i]["sapcode"].retStr();//sapcode
                wsSheet1.Cells[exlrowno, 7].Value = tbl.Rows[i]["docno"].retStr();//prefno
                wsSheet1.Cells[exlrowno, 8].Value = tbl.Rows[i]["docdt"].retDateStr("yyyy", "dd/MM/yyyy");//prefdt
                wsSheet1.Cells[exlrowno, 9].Value = tbl.Rows[i]["styleno"].retStr();
                wsSheet1.Cells[exlrowno, 10].Value = tbl.Rows[i]["shade"].retStr();//shade
                wsSheet1.Cells[exlrowno, 11].Value = tbl.Rows[i]["sizecd"].retStr();//sizecd
                wsSheet1.Cells[exlrowno, 12].Value = "";
                wsSheet1.Cells[exlrowno, 13].Value = tbl.Rows[i]["qnty"].retDbl();
                wsSheet1.Cells[exlrowno, 14].Value = tbl.Rows[i]["rprate"].retDbl();
                wsSheet1.Cells[exlrowno, 15].Value = (tbl.Rows[i]["NETAMT"].retDbl() + INCLDISCOUNTAMT).toRound();
                wsSheet1.Cells[exlrowno, 16].Value = INCLDISCOUNTAMT;
                wsSheet1.Cells[exlrowno, 17].Value = tbl.Rows[i]["NETAMT"].retDbl().toRound();
                exlrowno++;
            }
            //for download//
            Response.Clear();
            Response.ClearContent();
            Response.Buffer = true;
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string filename = "AdittyaBirlaSales".retRepname();
            Response.AddHeader("Content-Disposition", "attachment; filename=" + filename + ".xlsx");
            Response.BinaryWrite(ExcelPkg.GetAsByteArray());
            Response.Flush();
            Response.Close();
            Response.End();
            return Content("");

            //            select e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign, a.doctag,a.docdt,a.docno, f.itgrpcd, h.itgrpnm, f.itnm, nvl(e.pdesign, f.styleno) styleno, f.BRANDCD,
            // e.othrate, nvl(b.rate, 0) oprate, nvl(cp.rate, 0) clrate, nvl(rp.rate, 0) rprate, f.uomcd, i.uomnm, i.decimals, g.itnm fabitnm, h.sapcode,a.prefno,a.prefdt,a.shade,a.sizecd, 
            // sum(a.nos) nos,sum(a.qnty)qnty,sum(d.amt) basamt, sum(a.txblval)txblval, sum(a.othramt)othramt,sum(nvl(d.TDDISCAMT, 0) + nvl(d.SCMDISCAMT, 0) + nvl(d.DISCAMT, 0))discamt
            // from  (

            //select A.AUTONO, a.txnslno, d.compcd, d.loccd, a.barno, c.doctag, d.docdt, d.docno, c.prefno, c.prefdt, a.shade, sum(a.nos)nos, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty,
            //sum(a.txblval) txblval,
            //sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt, b.sizecd

            //from SD_LALF2021.t_batchdtl a, SD_LALF2021.t_batchmst b, SD_LALF2021.t_txn c, SD_LALF2021.t_cntrl_hdr d, SD_LALF2021.m_doctype e
            //where a.barno = b.barno
            //and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and d.compcd = 'LALF'

            //and d.loccd = 'KOLK' and nvl(d.cancel, 'N') = 'N' and e.doctype not in ('KHSR')  and e.doctype in ('SB', 'SBDIR', 'SBCM')  and a.stkdrcr in ('D','C') and d.docdt >= to_date('01/04/2021', 'dd/mm/yyyy')
            //and d.docdt <= to_date('07/04/2021', 'dd/mm/yyyy')  AND A.AUTONO = '2021LALFKOLKSSSBCM0000000049'
            //group by A.AUTONO,a.txnslno,d.compcd, d.loccd, a.barno, c.doctag,d.docdt,d.docno,c.prefno,c.prefdt,a.shade,b.sizecd
            //  ) a, 
            //(select barno, effdt, prccd, rate
            //from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn from SD_LALF2021.t_batchmst_price a
            //where a.effdt <= to_date('01/04/2021', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) b,  (select barno, effdt, prccd, rate
            //from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn
            //from SD_LALF2021.t_batchmst_price a where a.effdt <= to_date('07/04/2021', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) cp, 
            //(select barno, effdt, prccd, rate from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn
            //from SD_LALF2021.t_batchmst_price a where a.effdt <= to_date('07/04/2021', 'dd/mm/yyyy') and a.prccd = 'RP' ) where rn = 1) rp, 
            //SD_LALF2021.t_txndtl d, SD_LALF2021.t_batchmst e, SD_LALF2021.m_sitem f, SD_LALF2021.m_sitem g, SD_LALF2021.m_group h, FIN_LALF2021.m_uom i, FIN_LALF2021.m_subleg j, FIN_LALF2021.m_loca k
            //   , SD_LALF2021.m_size l
            //where a.autono || a.txnslno = d.autono || d.slno and a.barno = e.barno(+) and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and a.barno = b.barno(+)
            //and a.barno = cp.barno(+)and a.barno = rp.barno(+)  and e.slcd = j.slcd(+) and a.compcd || a.loccd = k.compcd || k.loccd and f.itgrpcd = h.itgrpcd(+)
            //and f.uomcd = i.uomcd(+) and a.sizecd = l.sizecd(+)
            //and a.barno in (
            //select barno from  (
            //select  a.barno, sum(a.qnty)qnty
            //from(
            //select  d.compcd, d.loccd, a.barno, d.docdt, d.docno, c.prefno, c.prefdt, a.shade,
            //sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval,
            //sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt, nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0) discamt, b.sizecd
            //from SD_LALF2021.t_batchdtl a, SD_LALF2021.t_batchmst b, SD_LALF2021.t_txn c, SD_LALF2021.t_cntrl_hdr d, SD_LALF2021.m_doctype e, SD_LALF2021.t_txndtl f
            //where a.barno = b.barno(+) and c.autono = f.autono
            //and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and d.compcd = 'LALF' and d.loccd = 'KOLK' and nvl(d.cancel, 'N') = 'N'
            //and e.doctype not in ('KHSR') and a.stkdrcr in ('D', 'C') and d.docdt < to_date('01/04/2021', 'dd/mm/yyyy')
            //group by d.compcd, d.loccd, a.barno, 'OP', d.docdt, d.docno, c.prefno, c.prefdt, a.shade, nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0), b.sizecd

            //union all select d.compcd, d.loccd, a.barno, d.docdt, d.docno, c.prefno, c.prefdt, a.shade, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty,
            //sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval,
            //  sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt, nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0) discamt, b.sizecd
            //from SD_LALF2021.t_batchdtl a, SD_LALF2021.t_batchmst b, SD_LALF2021.t_txn c, SD_LALF2021.t_cntrl_hdr d, SD_LALF2021.m_doctype e, SD_LALF2021.t_txndtl f
            //where a.barno = b.barno(+)  and c.autono = f.autono
            //and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and d.compcd = 'LALF'
            //and d.loccd = 'KOLK' and nvl(d.cancel, 'N') = 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D', 'C') and d.docdt >= to_date('01/04/2021', 'dd/mm/yyyy')
            // and d.docdt <= to_date('07/04/2021', 'dd/mm/yyyy')



            // group by d.compcd, d.loccd, a.barno,d.docdt,d.docno,c.prefno,c.prefdt,a.shade,nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0),b.sizecd


            // ) a, 
            //(select barno, effdt, prccd, rate
            //from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn from SD_LALF2021.t_batchmst_price a
            //where a.effdt <= to_date('01/04/2021', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) b,  (select barno, effdt, prccd, rate
            //from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn
            //from SD_LALF2021.t_batchmst_price a where a.effdt <= to_date('07/04/2021', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) cp, 
            //(select barno, effdt, prccd, rate from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn
            //from SD_LALF2021.t_batchmst_price a where a.effdt <= to_date('07/04/2021', 'dd/mm/yyyy') and a.prccd = 'RP' ) where rn = 1) rp, 
            //SD_LALF2021.t_batchmst e, SD_LALF2021.m_sitem f, SD_LALF2021.m_sitem g, SD_LALF2021.m_group h, FIN_LALF2021.m_uom i, FIN_LALF2021.m_subleg j, FIN_LALF2021.m_loca k
            //, SD_LALF2021.m_size l
            //where a.barno = e.barno(+) and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and a.barno = b.barno(+)
            //and a.barno = cp.barno(+)and a.barno = rp.barno(+)  and e.slcd = j.slcd(+) and a.compcd || a.loccd = k.compcd || k.loccd and f.itgrpcd = h.itgrpcd(+)
            //and f.uomcd = i.uomcd(+) and a.sizecd = l.sizecd(+)  and e.slcd in ('CA00005','CA00004','CA00003','CA00002','OA00006','OA00005') 
            //group by
            //e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign,a.docdt,a.docno,f.itgrpcd, h.itgrpnm, f.itnm, nvl(e.pdesign, f.styleno), 
            // e.othrate, nvl(b.rate, 0) , nvl(cp.rate, 0) , nvl(rp.rate, 0) , f.uomcd, i.uomnm, i.decimals, g.itnm ,h.sapcode,a.prefno,a.prefdt,a.shade,a.sizecd
            //) summ
            //)  
            //group by
            // e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign, a.doctag,a.docdt,a.docno,f.itgrpcd, h.itgrpnm, f.itnm, nvl(e.pdesign, f.styleno),f.BRANDCD, 
            // e.othrate, nvl(b.rate, 0) , nvl(cp.rate, 0) , nvl(rp.rate, 0) , f.uomcd, i.uomnm, i.decimals, g.itnm ,h.sapcode,a.prefno,a.prefdt,a.shade,a.sizecd
            // ,nvl(d.TDDISCAMT, 0) + nvl(d.SCMDISCAMT, 0) + nvl(d.DISCAMT, 0)
            //order by docdt,barno



        }

        public ActionResult GetAdityaBirlaStock(string scm, string scmf, string fdt, string tdt, string LOC, string COM, string slcd = "", string itgrpcd = "", string loccd = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string sql = "";
            sql += "select barno,styleno,itgrpnm,uomnm,doctag,rprate,sum(qnty) qnty from ( ";
            sql += Environment.NewLine + " select e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign, a.doctag, a.qnty, a.txblval, a.othramt, f.itgrpcd, h.itgrpnm, f.itnm, ";
            sql += Environment.NewLine + "nvl(e.pdesign, f.styleno) styleno, e.othrate, nvl(b.rate, 0) oprate, nvl(cp.rate, 0) clrate, ";
            sql += Environment.NewLine + "nvl(rp.rate, 0) rprate, ";
            sql += Environment.NewLine + "f.uomcd, i.uomnm, i.decimals, g.itnm fabitnm  from ";
            sql += Environment.NewLine + " ";
            sql += Environment.NewLine + "(select d.compcd, d.loccd, a.barno, 'OP' doctag, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, ";
            sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval, ";
            sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt ";
            sql += Environment.NewLine + "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e ";
            sql += Environment.NewLine + "where a.barno = b.barno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and ";
            sql += Environment.NewLine + "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D', 'C') and ";
            sql += Environment.NewLine + "d.docdt < to_date('" + fdt + "', 'dd/mm/yyyy') ";
            sql += Environment.NewLine + "group by d.compcd, d.loccd, a.barno, 'OP' ";
            sql += Environment.NewLine + "union all ";
            sql += Environment.NewLine + "select d.compcd, d.loccd, a.barno, c.doctag, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, ";
            sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval,  ";
            sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt ";
            sql += Environment.NewLine + "    from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e ";
            sql += Environment.NewLine + "where a.barno = b.barno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and ";
            sql += Environment.NewLine + "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N')= 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D','C') and ";
            sql += Environment.NewLine + "d.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and d.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') ";
            sql += Environment.NewLine + "group by d.compcd, d.loccd, a.barno, c.doctag ) a, ";
            sql += Environment.NewLine + " ";
            sql += Environment.NewLine + "(select barno, effdt, prccd, rate from ( ";
            sql += Environment.NewLine + "select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += Environment.NewLine + "from " + scm + ".t_batchmst_price a ";
            sql += Environment.NewLine + "where a.effdt <= to_date('" + fdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) b, ";
            sql += Environment.NewLine + " ";
            sql += Environment.NewLine + "(select barno, effdt, prccd, rate from ( ";
            sql += Environment.NewLine + "select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += Environment.NewLine + "from " + scm + ".t_batchmst_price a ";
            sql += Environment.NewLine + "where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) cp, ";
            sql += Environment.NewLine + " (select barno, effdt, prccd, rate from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += Environment.NewLine + " from " + scm + ".t_batchmst_price a where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'RP' ) where rn = 1) rp, ";
            sql += Environment.NewLine + " ";
            sql += Environment.NewLine + "" + scm + ".t_batchmst e, " + scm + ".m_sitem f, " + scm + ".m_sitem g, " + scm + ".m_group h, " + scmf + ".m_uom i, ";
            sql += Environment.NewLine + "" + scmf + ".m_subleg j, " + scmf + ".m_loca k ";
            //sql += Environment.NewLine + "where a.barno = e.barno(+) and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and ";
            sql += Environment.NewLine + "where a.barno = e.barno and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and ";
            sql += Environment.NewLine + "a.barno = b.barno(+) and a.barno = cp.barno(+)and a.barno = rp.barno(+)  and e.slcd = j.slcd(+) and a.compcd || a.loccd = k.compcd || k.loccd and ";
            sql += Environment.NewLine + "f.itgrpcd = h.itgrpcd(+) and f.uomcd = i.uomcd(+) " + Environment.NewLine;
            if (slcd.retStr() != "") sql += "and e.slcd in (" + slcd + ") ";
            if (itgrpcd.retStr() != "") sql += "and f.itgrpcd in (" + itgrpcd + ") ";
            if (loccd.retStr() != "") sql += "and a.loccd in (" + loccd + ") ";
            sql += Environment.NewLine + " ) a ";
            sql += Environment.NewLine + "group by barno, styleno, itgrpnm, uomnm, doctag, rprate ";
            sql += Environment.NewLine + "order by barno,styleno,itgrpnm,uomnm,doctag,rprate ";

            var tbl = MasterHelp.SQLquery(sql);
            if (tbl.Rows.Count == 0) return Content("no records..");


            string filename = "AdittyaBirlaStock".retRepname();
            ExcelPackage ExcelPkg = new ExcelPackage();
            ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("sheet1");

            string Excel_Header = "EanNumber" + "|" + "StyleCode" + "|" + "BrandName" + "|" + "UOM" + "|" + "StockQty" + "|" + "MRP";

            using (ExcelRange Rng = wsSheet1.Cells["A1:F1"])
            {
                Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                Rng.Style.Font.Bold = true; Rng.Style.Font.Size = 9;
                Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SkyBlue);
                Rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                Rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                string[] Header = Excel_Header.Split('|');
                for (int j = 0; j < Header.Length; j++)
                {
                    wsSheet1.Cells[1, j + 1].Value = Header[j];
                }
            }
            int exlrowno = 2; var rslno = 0;

            //DataView dv = new DataView(tbl);
            //string[] COL = new string[] { "barno", "styleno", "lrdt", "baleno", "prefno", "prefdt" };
            //tbl = dv.ToTable(true, COL);

            for (int i = 0; i < tbl.Rows.Count; i++)
            {
                double opqnty = 0;
                //if (tbl.Rows[i]["doctag"].retStr() == "OP" || tbl.Rows[i]["doctag"].retStr() == "PB" || tbl.Rows[i]["doctag"].retStr() == "TI" || tbl.Rows[i]["doctag"].retStr() == "SR")
                //{
                //    opqnty += tbl.Rows[i]["qnty"].retDbl();
                //}
                //else if (tbl.Rows[i]["doctag"].retStr() == "SB" || tbl.Rows[i]["doctag"].retStr() == "PR" || tbl.Rows[i]["doctag"].retStr() == "TO")
                //{
                //    opqnty -= tbl.Rows[i]["qnty"].retDbl();
                //}
                //else
                //{
                //    opqnty += tbl.Rows[i]["qnty"].retDbl();
                //}
                wsSheet1.Cells[exlrowno, 1].Value = tbl.Rows[i]["barno"].retStr();
                wsSheet1.Cells[exlrowno, 2].Value = tbl.Rows[i]["styleno"].retStr();
                wsSheet1.Cells[exlrowno, 3].Value = tbl.Rows[i]["itgrpnm"].retStr();
                wsSheet1.Cells[exlrowno, 4].Value = tbl.Rows[i]["uomnm"].retStr();
                wsSheet1.Cells[exlrowno, 5].Value = tbl.Rows[i]["qnty"].retDbl();// opqnty.retDbl();
                wsSheet1.Cells[exlrowno, 6].Value = tbl.Rows[i]["rprate"].retDbl();

                exlrowno++;

            }//for
            //for download//
            Response.Clear();
            Response.ClearContent();
            Response.Buffer = true;
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            // string filename = "AdittyaBirlaSales".retRepname();
            Response.AddHeader("Content-Disposition", "attachment; filename=" + filename + ".xlsx");
            Response.BinaryWrite(ExcelPkg.GetAsByteArray());
            Response.Flush();
            Response.Close();
            Response.End();
            return Content("");
        }
        public ActionResult ClosingStockBarcodeWise(string scm, string scmf, string fdt, string tdt, string LOC, string COM, string slcd = "", string itgrpcd = "", string loccd = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string sql = "";
            sql += "select barno,styleno,itgrpnm,uomnm,Cprate,sum(qnty) qnty from ( ";
            sql += Environment.NewLine + " select e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign, a.doctag, a.qnty, a.txblval, a.othramt, f.itgrpcd, h.itgrpnm, f.itnm, ";
            sql += Environment.NewLine + "nvl(e.pdesign, f.styleno) styleno, e.othrate, nvl(b.rate, 0) oprate, nvl(cp.rate, 0) clrate, ";
            sql += Environment.NewLine + "nvl(cp.rate, 0) cprate, ";
            sql += Environment.NewLine + "f.uomcd, i.uomnm, i.decimals, g.itnm fabitnm  from ";
            sql += Environment.NewLine + " ";
            sql += Environment.NewLine + "(select d.compcd, d.loccd, a.barno, 'OP' doctag, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, ";
            sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval, ";
            sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt ";
            sql += Environment.NewLine + "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e ";
            sql += Environment.NewLine + "where a.barno = b.barno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and ";
            sql += Environment.NewLine + "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D', 'C') and ";
            sql += Environment.NewLine + "d.docdt < to_date('" + fdt + "', 'dd/mm/yyyy') ";
            sql += Environment.NewLine + "group by d.compcd, d.loccd, a.barno, 'OP' ";
            sql += Environment.NewLine + "union all ";
            sql += Environment.NewLine + "select d.compcd, d.loccd, a.barno, c.doctag, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, ";
            sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval,  ";
            sql += Environment.NewLine + "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt ";
            sql += Environment.NewLine + "    from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e ";
            sql += Environment.NewLine + "where a.barno = b.barno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and ";
            sql += Environment.NewLine + "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N')= 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D','C') and ";
            sql += Environment.NewLine + "d.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and d.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') ";
            sql += Environment.NewLine + "group by d.compcd, d.loccd, a.barno, c.doctag ) a, ";
            sql += Environment.NewLine + " ";
            sql += Environment.NewLine + "(select barno, effdt, prccd, rate from ( ";
            sql += Environment.NewLine + "select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += Environment.NewLine + "from " + scm + ".t_batchmst_price a ";
            sql += Environment.NewLine + "where a.effdt <= to_date('" + fdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) b, ";
            sql += Environment.NewLine + " ";
            sql += Environment.NewLine + "(select barno, effdt, prccd, rate from ( ";
            sql += Environment.NewLine + "select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += Environment.NewLine + "from " + scm + ".t_batchmst_price a ";
            sql += Environment.NewLine + "where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) cp, ";
            sql += Environment.NewLine + " (select barno, effdt, prccd, rate from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += Environment.NewLine + " from " + scm + ".t_batchmst_price a where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'RP' ) where rn = 1) rp, ";
            sql += Environment.NewLine + " ";
            sql += Environment.NewLine + "" + scm + ".t_batchmst e, " + scm + ".m_sitem f, " + scm + ".m_sitem g, " + scm + ".m_group h, " + scmf + ".m_uom i, ";
            sql += Environment.NewLine + "" + scmf + ".m_subleg j, " + scmf + ".m_loca k ";
            //sql += Environment.NewLine + "where a.barno = e.barno(+) and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and ";
            sql += Environment.NewLine + "where a.barno = e.barno and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and ";
            sql += Environment.NewLine + "a.barno = b.barno(+) and a.barno = cp.barno(+)and a.barno = rp.barno(+)  and e.slcd = j.slcd(+) and a.compcd || a.loccd = k.compcd || k.loccd and ";
            sql += Environment.NewLine + "f.itgrpcd = h.itgrpcd(+) and f.uomcd = i.uomcd(+) " + Environment.NewLine;
            if (slcd.retStr() != "") sql += "and e.slcd in (" + slcd + ") ";
            if (itgrpcd.retStr() != "") sql += "and f.itgrpcd in (" + itgrpcd + ") ";
            if (loccd.retStr() != "") sql += "and a.loccd in (" + loccd + ") ";
            sql += Environment.NewLine + " ) a ";
            sql += Environment.NewLine + "group by barno, styleno, itgrpnm, uomnm, Cprate ";
            sql += Environment.NewLine + "order by barno,styleno,itgrpnm,uomnm,Cprate ";

            var tbl = MasterHelp.SQLquery(sql);
            if (tbl.Rows.Count == 0) return Content("no records..");

            string filename = "Closing Stock Barcode Wise".retRepname();
            ExcelPackage ExcelPkg = new ExcelPackage();
            ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("sheet1");

            string Excel_Header = "Barno" + "|" + "Style no" + "|" + "Brand Name" + "|" + "UOM" + "|" + "Stock Qty" + "|" + "Cost Price" + "|" + "Net Amount";

            using (ExcelRange Rng = wsSheet1.Cells["A1:G1"])
            {
                Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                Rng.Style.Font.Bold = true; Rng.Style.Font.Size = 9;
                Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SkyBlue);
                Rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                Rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                string[] Header = Excel_Header.Split('|');
                for (int j = 0; j < Header.Length; j++)
                {
                    wsSheet1.Cells[1, j + 1].Value = Header[j];
                }
            }
            int exlrowno = 2;

            for (int i = 0; i < tbl.Rows.Count; i++)
            {
                if (tbl.Rows[i]["qnty"].retDbl() != 0)
                {
                    wsSheet1.Cells[exlrowno, 1].Value = tbl.Rows[i]["barno"].retStr();
                    wsSheet1.Cells[exlrowno, 2].Value = tbl.Rows[i]["styleno"].retStr();
                    wsSheet1.Cells[exlrowno, 3].Value = tbl.Rows[i]["itgrpnm"].retStr();
                    wsSheet1.Cells[exlrowno, 4].Value = tbl.Rows[i]["uomnm"].retStr();
                    wsSheet1.Cells[exlrowno, 5].Value = tbl.Rows[i]["qnty"].retDbl();// opqnty.retDbl();
                    wsSheet1.Cells[exlrowno, 6].Value = tbl.Rows[i]["cprate"].retDbl();
                    wsSheet1.Cells[exlrowno, 7].Value = (tbl.Rows[i]["qnty"].retDbl() * tbl.Rows[i]["cprate"].retDbl()).toRound();
                    exlrowno++;
                }
            }//for
            //for download//
            Response.Clear();
            Response.ClearContent();
            Response.Buffer = true;
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            // string filename = "AdittyaBirlaSales".retRepname();
            Response.AddHeader("Content-Disposition", "attachment; filename=" + filename + ".xlsx");
            Response.BinaryWrite(ExcelPkg.GetAsByteArray());
            Response.Flush();
            Response.Close();
            Response.End();
            return Content("");
        }

    }
}