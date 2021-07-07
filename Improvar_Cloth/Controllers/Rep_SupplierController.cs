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
        string slcd = "", itgrpcd = "", loccd = "";
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
                reptype = VE.TEXTBOX1.retStr();
                DataTable tbl = new DataTable();
                DataTable LOCDT = new DataTable("loccd");

                if (reptype != "PurchasebillwiseStock" || reptype != "AdityaBirlaSale")
                {
                    string sql = "select e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign, a.doctag, a.qnty, a.txblval, a.othramt, f.itgrpcd, h.itgrpnm, f.itnm, ";
                    sql += "nvl(e.pdesign, f.styleno) styleno, e.othrate, nvl(b.rate, 0) oprate, nvl(cp.rate, 0) clrate, ";
                    sql += "nvl(rp.rate, 0) rprate, ";
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
                    sql += "where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) cp, ";
                    sql += " (select barno, effdt, prccd, rate from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
                    sql += " from " + scm + ".t_batchmst_price a where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'RP' ) where rn = 1) rp, ";
                    sql += " ";
                    sql += "" + scm + ".t_batchmst e, " + scm + ".m_sitem f, " + scm + ".m_sitem g, " + scm + ".m_group h, " + scmf + ".m_uom i, ";
                    sql += "" + scmf + ".m_subleg j, " + scmf + ".m_loca k ";
                    sql += "where a.barno = e.barno(+) and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and ";
                    sql += "a.barno = b.barno(+) and a.barno = cp.barno(+)and a.barno = rp.barno(+)  and e.slcd = j.slcd(+) and a.compcd || a.loccd = k.compcd || k.loccd and ";
                    sql += "f.itgrpcd = h.itgrpcd(+) and f.uomcd = i.uomcd(+) ";
                    if (slcd.retStr() != "") sql += "and e.slcd in (" + slcd + ") ";
                    if (itgrpcd.retStr() != "") sql += "and f.itgrpcd in (" + itgrpcd + ") ";
                    if (loccd.retStr() != "") sql += "and a.loccd in (" + loccd + ") ";
                    sql += "order by slcd,slnm,itgrpnm, itgrpcd, fabitnm, fabitcd, itnm, itcd, styleno, barno ";
                    tbl = MasterHelp.SQLquery(sql);
                    if (tbl.Rows.Count == 0) return Content("no records..");

                    string[] LOCTBLCOLS = new string[] { "loccd", "locnm" };
                    LOCDT = tbl.DefaultView.ToTable(true, LOCTBLCOLS);
                }


                string filename = "";
                if (reptype == "AdityaBirlaStock" || reptype == "PurchasebillwiseStock" || reptype == "AdityaBirlaSale")
                {
                    ExcelPackage ExcelPkg = new ExcelPackage();
                    ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("sheet1");
                    if (reptype == "AdityaBirlaStock")
                    {
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
                        filename = "AdittyaBirlaStock".retRepname();
                        int exlrowno = 2; var rslno = 0;
                        for (int i = 0; i < tbl.Rows.Count; i++)
                        {
                            double opqnty = 0;
                            if (tbl.Rows[i]["doctag"].retStr() == "OP" || tbl.Rows[i]["doctag"].retStr() == "PB" || tbl.Rows[i]["doctag"].retStr() == "TI" || tbl.Rows[i]["doctag"].retStr() == "SR")
                            {
                                opqnty += tbl.Rows[i]["qnty"].retDbl();
                            }
                            else if (tbl.Rows[i]["doctag"].retStr() == "SB" || tbl.Rows[i]["doctag"].retStr() == "PR" || tbl.Rows[i]["doctag"].retStr() == "TO")
                            {
                                opqnty -= tbl.Rows[i]["qnty"].retDbl();
                            }
                            else
                            {
                                opqnty += tbl.Rows[i]["qnty"].retDbl();
                            }
                            wsSheet1.Cells[exlrowno, 1].Value = tbl.Rows[i]["barno"].retStr();
                            wsSheet1.Cells[exlrowno, 2].Value = tbl.Rows[i]["styleno"].retStr();
                            wsSheet1.Cells[exlrowno, 3].Value = tbl.Rows[i]["itgrpnm"].retStr();
                            wsSheet1.Cells[exlrowno, 4].Value = tbl.Rows[i]["uomnm"].retStr();
                            wsSheet1.Cells[exlrowno, 5].Value = opqnty.retDbl();
                            wsSheet1.Cells[exlrowno, 6].Value = tbl.Rows[i]["rprate"].retDbl();

                            exlrowno++;

                        }
                    }
                    else
                    {
                        DataTable Purtbl = new DataTable();
                        if (reptype == "AdityaBirlaSale") { return GetSalesfromSupplier(scm, scmf, fdt, tdt, LOC, COM, slcd, itgrpcd, loccd); }
                        else { Purtbl = retPurchaseWiseStock(scm, scmf, fdt, tdt, LOC, COM, slcd, itgrpcd, loccd); filename = "PurchasebillwiseStock".retRepname(); }

                        if (Purtbl.Rows.Count == 0) return Content("no records..");
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
                    }
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
                }
                else
                {
                    Int32 rNo = 0, maxR = 0, maxB = 0, i = 0;
                    maxR = tbl.Rows.Count - 1;
                    Int32 islno = 0;

                    Models.PrintViewer PV = new Models.PrintViewer();
                    HtmlConverter HC = new HtmlConverter();
                    DataTable IR = new DataTable("");

                    HC.RepStart(IR, 2);
                    if (reptype == "N") HC.GetPrintHeader(IR, "slno", "long", "n,4", "Slno");
                    if (reptype == "S") HC.GetPrintHeader(IR, "slnm", "string", "c,40", "Party");
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
                    itgrpcd = "";
                    int cnt = 0;
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

                        while (tbl.Rows[i]["slcd"].retStr() == slcd)
                        {
                            itgrpcd = tbl.Rows[i]["itgrpcd"].retStr();
                            if (reptype == "N")
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["Dammy"] = tbl.Rows[i]["itgrpnm"].ToString();
                                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                            }
                            islno = 0;
                            while (tbl.Rows[i]["slcd"].retStr() == slcd && tbl.Rows[i]["itgrpcd"].retStr() == itgrpcd)
                            {
                                string itcd = tbl.Rows[i]["itcd"].retStr();
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                islno++;
                                double opqnty = 0, opvalue = 0, othersqnty = 0, othersvalue = 0;
                                while (tbl.Rows[i]["slcd"].retStr() == slcd && tbl.Rows[i]["itgrpcd"].retStr() == itgrpcd && tbl.Rows[i]["itcd"].retStr() == itcd)
                                {
                                    string key = tbl.Rows[i]["barno"].retStr() + tbl.Rows[i]["pdesign"].retStr() + tbl.Rows[i]["doctag"].retStr() + tbl.Rows[i]["loccd"].retStr();
                                    while (tbl.Rows[i]["slcd"].retStr() == slcd && tbl.Rows[i]["itgrpcd"].retStr() == itgrpcd && tbl.Rows[i]["itcd"].retStr() == itcd && (tbl.Rows[i]["barno"].retStr() + tbl.Rows[i]["pdesign"].retStr() + tbl.Rows[i]["doctag"].retStr() + tbl.Rows[i]["loccd"].retStr()) == key)
                                    {
                                        if (reptype == "N") IR.Rows[rNo]["slno"] = islno;
                                        IR.Rows[rNo]["styleno"] = tbl.Rows[i]["styleno"].ToString();
                                        IR.Rows[rNo]["pdesign"] = tbl.Rows[i]["pdesign"].ToString();
                                        IR.Rows[rNo]["uomnm"] = tbl.Rows[i]["uomnm"].ToString();
                                        IR.Rows[rNo]["itgrpcd"] = tbl.Rows[i]["itgrpcd"].retStr();
                                        IR.Rows[rNo]["slcd"] = tbl.Rows[i]["slcd"].retStr();
                                        if (reptype == "S") IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"].retStr();
                                        if (reptype == "S") IR.Rows[rNo]["itgrpnm"] = tbl.Rows[i]["itgrpnm"].retStr();
                                        if (tbl.Rows[i]["doctag"].retStr() == "OP")
                                        {
                                            IR.Rows[rNo]["openingqnty"] = IR.Rows[rNo]["openingqnty"].retDbl() + tbl.Rows[i]["qnty"].retDbl();
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["openingamt"] = IR.Rows[rNo]["openingamt"].retDbl() + tbl.Rows[i]["txblval"].retDbl();
                                            opqnty += tbl.Rows[i]["qnty"].retDbl();
                                            opvalue += tbl.Rows[i]["txblval"].retDbl();
                                        }
                                        else if (tbl.Rows[i]["doctag"].retStr() == "PB")
                                        {
                                            IR.Rows[rNo]["purchaseqnty"] = IR.Rows[rNo]["purchaseqnty"].retDbl() + tbl.Rows[i]["qnty"].retDbl();
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["purchaseamt"] = IR.Rows[rNo]["purchaseamt"].retDbl() + tbl.Rows[i]["txblval"].retDbl();
                                            opqnty += tbl.Rows[i]["qnty"].retDbl();
                                            opvalue += tbl.Rows[i]["txblval"].retDbl();
                                        }
                                        else if (tbl.Rows[i]["doctag"].retStr() == "PR")
                                        {
                                            IR.Rows[rNo]["purretqnty"] = IR.Rows[rNo]["purretqnty"].retDbl() + (tbl.Rows[i]["qnty"].retDbl() * -1);
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["purretamt"] = IR.Rows[rNo]["purretamt"].retDbl() + (tbl.Rows[i]["txblval"].retDbl() * -1);
                                            opqnty += tbl.Rows[i]["qnty"].retDbl();
                                            opvalue += tbl.Rows[i]["txblval"].retDbl();
                                        }
                                        else if (tbl.Rows[i]["doctag"].retStr() == "TI")
                                        {
                                            IR.Rows[rNo]["transferinqnty"] = IR.Rows[rNo]["transferinqnty"].retDbl() + tbl.Rows[i]["qnty"].retDbl();
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["transferinamt"] = IR.Rows[rNo]["transferinamt"].retDbl() + tbl.Rows[i]["txblval"].retDbl();
                                            opqnty += tbl.Rows[i]["qnty"].retDbl();
                                            opvalue += tbl.Rows[i]["txblval"].retDbl();
                                        }
                                        else if (tbl.Rows[i]["doctag"].retStr() == "TO")
                                        {
                                            IR.Rows[rNo]["transferoutqnty"] = IR.Rows[rNo]["transferoutqnty"].retDbl() + (tbl.Rows[i]["qnty"].retDbl() * -1);
                                            if (MENU_PARA != "Q") IR.Rows[rNo]["transferoutamt"] = IR.Rows[rNo]["transferoutamt"].retDbl() + (tbl.Rows[i]["txblval"].retDbl() * -1);
                                            opqnty += tbl.Rows[i]["qnty"].retDbl();
                                            opvalue += tbl.Rows[i]["txblval"].retDbl();
                                        }
                                        else if (tbl.Rows[i]["doctag"].retStr() == "SB" || tbl.Rows[i]["doctag"].retStr() == "SR")
                                        {
                                            if (VE.Checkbox1 == true)
                                            {
                                                string colnm = tbl.Rows[i]["loccd"].retStr() + "salesqnty";
                                                IR.Rows[rNo][colnm] = IR.Rows[rNo][colnm].retDbl() + (tbl.Rows[i]["doctag"].retStr() == "SB" ? (tbl.Rows[i]["qnty"].retDbl() * -1) : tbl.Rows[i]["qnty"].retDbl());//modify by mithun
                                                if (MENU_PARA != "Q") IR.Rows[rNo][colnm] = IR.Rows[rNo][colnm].retDbl() + (tbl.Rows[i]["txblval"].retDbl() * -1);

                                            }
                                            else
                                            {
                                                IR.Rows[rNo]["salesqnty"] = IR.Rows[rNo]["salesqnty"].retDbl() + (tbl.Rows[i]["doctag"].retStr() == "SB" ? (tbl.Rows[i]["qnty"].retDbl() * -1) : tbl.Rows[i]["qnty"].retDbl());//modify by mithun
                                                if (MENU_PARA != "Q") IR.Rows[rNo]["salesamt"] = IR.Rows[rNo]["salesamt"].retDbl() + (tbl.Rows[i]["txblval"].retDbl() * -1);

                                            }

                                            if (tbl.Rows[i]["doctag"].retStr() == "SB")
                                            {
                                                opqnty += tbl.Rows[i]["qnty"].retDbl();
                                                opvalue += tbl.Rows[i]["txblval"].retDbl();
                                            }
                                            else
                                            {
                                                opqnty += tbl.Rows[i]["qnty"].retDbl();
                                                opvalue += tbl.Rows[i]["txblval"].retDbl();
                                            }
                                        }
                                        else
                                        {
                                            othersqnty += tbl.Rows[i]["qnty"].retDbl();
                                            othersvalue += tbl.Rows[i]["txblval"].retDbl();

                                            opqnty += tbl.Rows[i]["qnty"].retDbl();
                                            opvalue += tbl.Rows[i]["txblval"].retDbl();
                                        }
                                        IR.Rows[rNo]["othersqnty"] = othersqnty;
                                        if (MENU_PARA != "Q") IR.Rows[rNo]["othersamt"] = othersvalue;

                                        IR.Rows[rNo]["stockqnty"] = opqnty;
                                        if (MENU_PARA != "Q") IR.Rows[rNo]["stockamt"] = opvalue;
                                        i++;
                                        if (i > maxB) break;
                                    }
                                    if (i > maxB) break;
                                }
                                if (i > maxB) break;
                            }
                            if (reptype == "N")
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["styleno"] = "Total of " + tbl.Rows[i - 1]["itgrpnm"].ToString();
                                IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;";

                                itgrpcd = tbl.Rows[i - 1]["itgrpcd"].ToString();
                                slcd = tbl.Rows[i - 1]["slcd"].ToString();
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
                                        int strart = reptype == "N" ? 7 : 8;
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
                            if (i > maxB) break;
                        }
                        if (reptype == "N")
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["styleno"] = "Total of " + tbl.Rows[i - 1]["slnm"].ToString();
                            IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;";

                            slcd = tbl.Rows[i - 1]["slcd"].ToString();
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
                                    int strart = reptype == "N" ? 7 : 8;
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
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
            return null;
        }



        public DataTable retPurchaseWiseStock(string scm, string scmf, string fdt, string tdt, string LOC, string COM, string slcd = "", string itgrpcd = "", string loccd = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string sql = "";
            sql += " select e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign, a.doctag,a.docdt,a.docno, f.itgrpcd, h.itgrpnm, f.itnm, nvl(e.pdesign, f.styleno) styleno, ";
            sql += " e.othrate, nvl(b.rate, 0) oprate, nvl(cp.rate, 0) clrate, nvl(rp.rate, 0) rprate, f.uomcd, i.uomnm, i.decimals, g.itnm fabitnm, h.sapcode,a.prefno,a.prefdt,a.shade,a.sizecd, ";
            sql += " sum(a.nos) nos,sum(a.qnty)qnty, sum(a.txblval)txblval, sum(a.othramt)othramt,sum(a.discamt)discamt ";
            sql += " from  ( ";
            sql += "select d.compcd, d.loccd, a.barno, 'OP' doctag, d.docdt, d.docno, c.prefno, c.prefdt, a.shade, sum(a.nos)nos, ";
            sql += "sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, sum(a.txblval) txblval, ";
            sql += "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt, nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0) discamt, b.sizecd ";

            sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e, " + scm + ".t_txndtl f ";
            sql += "where a.barno = b.barno(+) and c.autono = f.autono ";
            sql += "and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' ";
            sql += "and e.doctype not in ('KHSR') and a.stkdrcr in ('D', 'C') and d.docdt < to_date('" + fdt + "', 'dd/mm/yyyy') ";
            sql += "group by d.compcd, d.loccd, a.barno, 'OP',d.docdt,d.docno,c.prefno,c.prefdt,a.shade,nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0),b.sizecd ";

            sql += "union all select d.compcd, d.loccd, a.barno, c.doctag,d.docdt,d.docno,c.prefno,c.prefdt,a.shade,sum(a.nos)nos, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, ";
            sql += "sum(a.txblval) txblval, ";
            sql += "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt,nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0) discamt ,b.sizecd ";
            sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e, " + scm + ".t_txndtl f ";
            sql += "where a.barno = b.barno(+)  and c.autono = f.autono ";
            sql += "and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and d.compcd = '" + COM + "' ";
            sql += "and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N')= 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D','C') and d.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') ";
            sql += "and d.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') ";
            sql += "group by d.compcd, d.loccd, a.barno, c.doctag,d.docdt,d.docno,c.prefno,c.prefdt,a.shade,nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0),b.sizecd ";
            sql += " ) a, ";

            sql += "(select barno, effdt, prccd, rate ";
            sql += "from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn from " + scm + ".t_batchmst_price a ";
            sql += "where a.effdt <= to_date('" + fdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) b,  (select barno, effdt, prccd, rate ";
            sql += "from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += "from " + scm + ".t_batchmst_price a where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) cp, ";
            sql += "(select barno, effdt, prccd, rate from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += "from " + scm + ".t_batchmst_price a where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'RP' ) where rn = 1) rp, ";
            sql += "" + scm + ".t_batchmst e, " + scm + ".m_sitem f, " + scm + ".m_sitem g, " + scm + ".m_group h, " + scmf + ".m_uom i, " + scmf + ".m_subleg j, " + scmf + ".m_loca k ";
            sql += ", " + scm + ".m_size l ";
            sql += "where a.barno = e.barno(+) and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and a.barno = b.barno(+) ";
            sql += "and a.barno = cp.barno(+)and a.barno = rp.barno(+)  and e.slcd = j.slcd(+) and a.compcd || a.loccd = k.compcd || k.loccd and f.itgrpcd = h.itgrpcd(+) ";
            sql += "and f.uomcd = i.uomcd(+) and a.sizecd = l.sizecd(+) ";
            sql += "and a.barno in ( ";
            sql += "select barno from  ( ";
            sql += "select  a.barno, sum(a.qnty)qnty ";
            sql += "from( ";
            sql += "select d.compcd, d.loccd, a.barno, d.docdt, d.docno, c.prefno, c.prefdt, a.shade, ";
            sql += "sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval, ";
            sql += "sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt, nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0) discamt, b.sizecd ";


            sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e, " + scm + ".t_txndtl f ";
            sql += "where a.barno = b.barno(+) and c.autono = f.autono ";
            sql += "and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' ";
            sql += "and e.doctype not in ('KHSR') and a.stkdrcr in ('D', 'C') and d.docdt < to_date('" + fdt + "', 'dd/mm/yyyy') ";
            sql += "group by d.compcd, d.loccd, a.barno, 'OP', d.docdt, d.docno, c.prefno, c.prefdt, a.shade, nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0), b.sizecd ";

            sql += "union all select d.compcd, d.loccd, a.barno, d.docdt, d.docno, c.prefno, c.prefdt, a.shade, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, ";
            sql += "sum(case a.stkdrcr when 'D' then nvl(a.txblval, 0) else nvl(a.txblval, 0) * -1 end) txblval, ";
            sql += "  sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt, nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0) discamt, b.sizecd ";
            sql += "from " + scm + ".t_batchdtl a, " + scm + ".t_batchmst b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".m_doctype e, " + scm + ".t_txndtl f ";
            sql += "where a.barno = b.barno(+)  and c.autono = f.autono ";

            sql += "and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and d.compcd = '" + COM + "' ";
            sql += "and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D', 'C') and d.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') ";
            sql += " and d.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') group by d.compcd, d.loccd, a.barno,d.docdt,d.docno,c.prefno,c.prefdt,a.shade,nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0),b.sizecd ";
            sql += " ) a, ";
            sql += "(select barno, effdt, prccd, rate ";
            sql += "from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn from " + scm + ".t_batchmst_price a ";
            sql += "where a.effdt <= to_date('" + fdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) b,  (select barno, effdt, prccd, rate ";
            sql += "from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += "from " + scm + ".t_batchmst_price a where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) cp, ";
            sql += "(select barno, effdt, prccd, rate from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn ";
            sql += "from " + scm + ".t_batchmst_price a where a.effdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and a.prccd = 'RP' ) where rn = 1) rp, ";
            sql += "" + scm + ".t_batchmst e, " + scm + ".m_sitem f, " + scm + ".m_sitem g, " + scm + ".m_group h, " + scmf + ".m_uom i, " + scmf + ".m_subleg j, " + scmf + ".m_loca k ";
            sql += ", " + scm + ".m_size l ";
            sql += "where a.barno = e.barno(+) and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and a.barno = b.barno(+) ";
            sql += "and a.barno = cp.barno(+)and a.barno = rp.barno(+)  and e.slcd = j.slcd(+) and a.compcd || a.loccd = k.compcd || k.loccd and f.itgrpcd = h.itgrpcd(+) ";
            sql += "and f.uomcd = i.uomcd(+) and a.sizecd = l.sizecd(+) ";
            sql += "group by ";
            sql += "e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign,a.docdt,a.docno,f.itgrpcd, h.itgrpnm, f.itnm, nvl(e.pdesign, f.styleno), ";
            sql += " e.othrate, nvl(b.rate, 0) , nvl(cp.rate, 0) , nvl(rp.rate, 0) , f.uomcd, i.uomnm, i.decimals, g.itnm ,h.sapcode,a.prefno,a.prefdt,a.shade,a.sizecd ";
            sql += ") summ ";
            sql += "where summ.qnty > 0 ";
            sql += ")  ";
            if (slcd.retStr() != "") sql += "and e.slcd in (" + slcd + ") ";
            if (itgrpcd.retStr() != "") sql += "and f.itgrpcd in (" + itgrpcd + ") ";
            if (loccd.retStr() != "") sql += "and a.loccd in (" + loccd + ") ";
            sql += "group by ";
            sql += " e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign, a.doctag,a.docdt,a.docno,f.itgrpcd, h.itgrpnm, f.itnm, nvl(e.pdesign, f.styleno), ";
            sql += " e.othrate, nvl(b.rate, 0) , nvl(cp.rate, 0) , nvl(rp.rate, 0) , f.uomcd, i.uomnm, i.decimals, g.itnm ,h.sapcode,a.prefno,a.prefdt,a.shade,a.sizecd ";
            sql += "order by barno ";
            DataTable tbl = MasterHelp.SQLquery(sql);
            return tbl;
            //                        select e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign, a.doctag,a.docdt,a.docno, f.itgrpcd, h.itgrpnm, f.itnm, nvl(e.pdesign, f.styleno) styleno,
            // e.othrate, nvl(b.rate, 0) oprate, nvl(cp.rate, 0) clrate, nvl(rp.rate, 0) rprate, f.uomcd, i.uomnm, i.decimals, g.itnm fabitnm, h.sapcode,a.prefno,a.prefdt,a.shade,a.sizecd,
            //  sum(a.nos) nos,sum(a.qnty)qnty, sum(a.txblval)txblval, sum(a.othramt)othramt,sum(a.discamt)discamt
            //  from  (
            //select d.compcd, d.loccd, a.barno, 'OP' doctag, d.docdt, d.docno, c.prefno, c.prefdt, a.shade, sum(a.nos)nos,
            // sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, sum(a.txblval) txblval,
            // sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt, nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0) discamt, b.sizecd


            //from SD_LALF2021.t_batchdtl a, SD_LALF2021.t_batchmst b, SD_LALF2021.t_txn c, SD_LALF2021.t_cntrl_hdr d, SD_LALF2021.m_doctype e, SD_LALF2021.t_txndtl f
            //where a.barno = b.barno(+) and c.autono = f.autono
            //and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and d.compcd = 'LALF' and d.loccd = 'KOLK' and nvl(d.cancel, 'N') = 'N'
            //and e.doctype not in ('KHSR') and a.stkdrcr in ('D', 'C') and d.docdt < to_date('01/04/2021', 'dd/mm/yyyy')
            //group by d.compcd, d.loccd, a.barno, 'OP',d.docdt,d.docno,c.prefno,c.prefdt,a.shade,nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0),b.sizecd

            //      union all select d.compcd, d.loccd, a.barno, c.doctag,d.docdt,d.docno,c.prefno,c.prefdt,a.shade,sum(a.nos)nos, sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty, 
            // sum(a.txblval) txblval,
            // sum(case a.stkdrcr when 'D' then nvl(a.othramt, 0) else nvl(a.othramt, 0) * -1 end) othramt,nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0) discamt ,b.sizecd
            //     from SD_LALF2021.t_batchdtl a, SD_LALF2021.t_batchmst b, SD_LALF2021.t_txn c, SD_LALF2021.t_cntrl_hdr d, SD_LALF2021.m_doctype e, SD_LALF2021.t_txndtl f
            //where a.barno = b.barno(+)  and c.autono = f.autono
            // and a.autono = c.autono(+) and a.autono = d.autono(+) and d.doccd = e.doccd(+) and d.compcd = 'LALF'
            //and d.loccd = 'KOLK' and nvl(d.cancel, 'N')= 'N' and e.doctype not in ('KHSR') and a.stkdrcr in ('D','C') and d.docdt >= to_date('01/04/2021', 'dd/mm/yyyy')
            // and d.docdt <= to_date('26/04/2021', 'dd/mm/yyyy')
            // group by d.compcd, d.loccd, a.barno, c.doctag,d.docdt,d.docno,c.prefno,c.prefdt,a.shade,nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0),b.sizecd
            // ) a,

            //   (select barno, effdt, prccd, rate
            //  from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn from SD_LALF2021.t_batchmst_price a
            //where a.effdt <= to_date('01/04/2021', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) b,  (select barno, effdt, prccd, rate
            //from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn
            //from SD_LALF2021.t_batchmst_price a where a.effdt <= to_date('26/04/2021', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) cp,
            //  (select barno, effdt, prccd, rate from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn
            //from SD_LALF2021.t_batchmst_price a where a.effdt <= to_date('26/04/2021', 'dd/mm/yyyy') and a.prccd = 'RP' ) where rn = 1) rp,
            //  SD_LALF2021.t_batchmst e, SD_LALF2021.m_sitem f, SD_LALF2021.m_sitem g, SD_LALF2021.m_group h, FIN_LALF2021.m_uom i, FIN_LALF2021.m_subleg j, FIN_LALF2021.m_loca k
            //  , SD_LALF2021.m_size l
            //where a.barno = e.barno(+) and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and a.barno = b.barno(+)
            //and a.barno = cp.barno(+)and a.barno = rp.barno(+)  and e.slcd = j.slcd(+) and a.compcd || a.loccd = k.compcd || k.loccd and f.itgrpcd = h.itgrpcd(+)
            // and f.uomcd = i.uomcd(+) and a.sizecd = l.sizecd(+)
            //and a.barno in (
            //select barno from  (
            //select  a.barno, sum(a.qnty)qnty
            //from(
            //select d.compcd, d.loccd, a.barno, d.docdt, d.docno, c.prefno, c.prefdt, a.shade,
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
            // and d.docdt <= to_date('26/04/2021', 'dd/mm/yyyy') group by d.compcd, d.loccd, a.barno,d.docdt,d.docno,c.prefno,c.prefdt,a.shade,nvl(TDDISCAMT, 0) + nvl(SCMDISCAMT, 0) + nvl(DISCAMT, 0),b.sizecd
            // ) a,

            //   (select barno, effdt, prccd, rate
            //  from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn from SD_LALF2021.t_batchmst_price a
            //where a.effdt <= to_date('01/04/2021', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) b,  (select barno, effdt, prccd, rate
            //from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn
            //from SD_LALF2021.t_batchmst_price a where a.effdt <= to_date('26/04/2021', 'dd/mm/yyyy') and a.prccd = 'CP' ) where rn = 1) cp,
            //  (select barno, effdt, prccd, rate from (select a.barno, a.effdt, a.prccd, a.rate, row_number() over(partition by a.barno, a.prccd order by a.effdt desc) as rn
            //from SD_LALF2021.t_batchmst_price a where a.effdt <= to_date('26/04/2021', 'dd/mm/yyyy') and a.prccd = 'RP' ) where rn = 1) rp,
            //  SD_LALF2021.t_batchmst e, SD_LALF2021.m_sitem f, SD_LALF2021.m_sitem g, SD_LALF2021.m_group h, FIN_LALF2021.m_uom i, FIN_LALF2021.m_subleg j, FIN_LALF2021.m_loca k
            //  , SD_LALF2021.m_size l
            //where a.barno = e.barno(+) and e.itcd = f.itcd(+) and e.fabitcd = g.fabitcd(+) and a.barno = b.barno(+)
            //and a.barno = cp.barno(+)and a.barno = rp.barno(+)  and e.slcd = j.slcd(+) and a.compcd || a.loccd = k.compcd || k.loccd and f.itgrpcd = h.itgrpcd(+)
            // and f.uomcd = i.uomcd(+) and a.sizecd = l.sizecd(+)


            // group by
            //   e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign,a.docdt,a.docno,f.itgrpcd, h.itgrpnm, f.itnm, nvl(e.pdesign, f.styleno),
            // e.othrate, nvl(b.rate, 0) , nvl(cp.rate, 0) , nvl(rp.rate, 0) , f.uomcd, i.uomnm, i.decimals, g.itnm ,h.sapcode,a.prefno,a.prefdt,a.shade,a.sizecd
            //) summ
            //where summ.qnty > 0
            //) 

            //  group by
            //   e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign, a.doctag,a.docdt,a.docno,f.itgrpcd, h.itgrpnm, f.itnm, nvl(e.pdesign, f.styleno),
            // e.othrate, nvl(b.rate, 0) , nvl(cp.rate, 0) , nvl(rp.rate, 0) , f.uomcd, i.uomnm, i.decimals, g.itnm ,h.sapcode,a.prefno,a.prefdt,a.shade,a.sizecd
            //order by barno
        }

        public ActionResult GetSalesfromSupplier(string scm, string scmf, string fdt, string tdt, string LOC, string COM, string slcd = "", string itgrpcd = "", string loccd = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string sql = "";
            sql += " select e.slcd,j.slnm, a.loccd, k.locnm, a.barno, e.itcd, e.fabitcd,e.pdesign, a.doctag,a.docdt,a.docno, f.itgrpcd, h.itgrpnm, f.itnm, nvl(e.pdesign, f.styleno) styleno, f.BRANDCD,";
            sql += Environment.NewLine + " e.othrate, nvl(b.rate, 0) oprate, nvl(cp.rate, 0) clrate, nvl(rp.rate, 0) rprate, f.uomcd, i.uomnm, i.decimals, g.itnm fabitnm, h.sapcode,a.prefno,a.prefdt,a.shade,a.sizecd, ";
            sql += Environment.NewLine + " sum(a.nos) nos,sum(a.qnty)qnty,sum(d.amt) basamt, sum(a.txblval)txblval, sum(a.othramt)othramt,sum(nvl(d.TDDISCAMT, 0) + nvl(d.SCMDISCAMT, 0) + nvl(d.DISCAMT, 0)) discamt, sum(d.igstamt)igstamt, sum(d.cgstamt)cgstamt, sum(d.sgstamt)sgstamt, sum(d.cessamt)cessamt ";
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
            sql += Environment.NewLine + " e.othrate, nvl(b.rate, 0) , nvl(cp.rate, 0) , nvl(rp.rate, 0) , f.uomcd, i.uomnm, i.decimals, g.itnm ,h.sapcode,a.prefno,a.prefdt,a.shade,a.sizecd ,(nvl(d.TDDISCAMT, 0) + nvl(d.SCMDISCAMT, 0) + nvl(d.DISCAMT, 0))";
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
                wsSheet1.Cells[exlrowno, 15].Value = tbl.Rows[i]["basamt"].retDbl()+ tbl.Rows[i]["igstamt"].retDbl() + tbl.Rows[i]["cgstamt"].retDbl() + tbl.Rows[i]["sgstamt"].retDbl() + tbl.Rows[i]["cessamt"].retDbl();
                wsSheet1.Cells[exlrowno, 16].Value = tbl.Rows[i]["discamt"].retDbl();
                wsSheet1.Cells[exlrowno, 17].Value = (tbl.Rows[i]["basamt"].retDbl() + tbl.Rows[i]["othramt"].retDbl() - tbl.Rows[i]["discamt"].retDbl()).toRound();
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



    }
}