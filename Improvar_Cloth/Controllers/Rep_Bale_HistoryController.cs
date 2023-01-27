
using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using OfficeOpenXml;
using System.Collections.Generic;
using OfficeOpenXml.Style;

namespace Improvar.Controllers
{
    public class Rep_Bale_HistoryController : Controller
    {
        // GET: Rep_Bale_History
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string fdt = ""; string tdt = ""; bool showpacksize = false, showrate = false;
        string modulecode = CommVar.ModuleCode();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult Rep_Bale_History()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Bale History";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    string com = CommVar.Compcd(UNQSNO); string gcs = Cn.GCS();
                    string qry = "select distinct baleno ||'/'|| baleyr BaleNoBaleYr,baleno || baleyr BaleNoBaleYrcd from " + CommVar.CurSchema(UNQSNO) + ".t_txndtl where  baleno is not null and baleyr is not null  ";
                    DataTable tbl = MasterHelp.SQLquery(qry);
                    //VE.DropDown_list1 = (from DataRow dr in tbl.Rows select new DropDown_list1() { value = dr["BaleNoBaleYrcd"].retStr(), text = dr["BaleNoBaleYr"].retStr() }).OrderBy(s => s.text).ToList();
                    VE.DropDown_list1 = (from DataRow dr in tbl.Rows select new DropDown_list1() { value = dr["BaleNoBaleYrcd"].retStr() }).OrderBy(s => s.text).ToList();
                    VE.TEXTBOX1 = MasterHelp.ComboFill("BaleNoBaleYrcd", VE.DropDown_list1, "".retInt(), 1);
                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);
                    VE.DropDown_list_ITGRP = DropDownHelp.GetItgrpcdforSelection();
                    VE.Itgrpnm = MasterHelp.ComboFill("itgrpcd", VE.DropDown_list_ITGRP, 0, 1);
                    VE.TDT = CommVar.CurrDate(UNQSNO);
                    List<DropDown_list1> drplst = new List<DropDown_list1>();
                    DropDown_list1 dropobj1 = new DropDown_list1();
                    dropobj1.value = "BH";
                    dropobj1.text = "Bale History";
                    drplst.Add(dropobj1);

                    DropDown_list1 dropobj2 = new DropDown_list1();
                    dropobj2.value = "BM";
                    dropobj2.text = "Bale Movement";
                    drplst.Add(dropobj2);
                    VE.DropDown_list1 = drplst;
                    VE.Checkbox1 = false;
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
        public ActionResult Rep_Bale_History(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                fdt = VE.FDT.retDateStr(); tdt = VE.TDT.retDateStr();

                string txntag = ""; string txnrettag = "", rep_type = "";
                string selitcd = "", unselitcd = "", selitgrpcd = "", selbalenoyr = "", unselbalenoyr = "";

                if (FC.AllKeys.Contains("BaleNoBaleYrcdvalue")) selbalenoyr = FC["BaleNoBaleYrcdvalue"].retSqlformat();
                if (FC.AllKeys.Contains("BaleNoBaleYrcdunselvalue")) unselbalenoyr = FC["BaleNoBaleYrcdunselvalue"].retSqlformat();
                if (FC.AllKeys.Contains("itcdvalue")) selitcd = FC["itcdvalue"].retSqlformat();
                if (FC.AllKeys.Contains("itcdunselvalue")) unselitcd = FC["itcdunselvalue"].retSqlformat();
                if (FC.AllKeys.Contains("itgrpcdvalue")) selitgrpcd = FC["itgrpcdvalue"].retSqlformat();
                rep_type = VE.TEXTBOX2;
                txntag = txntag + txnrettag;
                bool RepeatAllRow = VE.Checkbox1;

                string sql = "";
                if (rep_type == "BH")
                {
                    //sql += "select a.autono, a.baleopen, a.blautono, a.txnslno, a.gocd, a.stkdrcr, a.baleno, a.baleyr,a.baleno || a.baleyr BaleNoBaleYrcd, a.itcd, b.shade, a.nos, a.qnty, c.usr_entdt, ";
                    //sql += "c.docno, c.docdt, b.prefno, a.slcd, h.slnm, g.gonm, f.styleno, f.itnm, f.itgrpcd, f.uomcd, c.doccd, e.docnm, e.doctype, ";
                    sql += "select distinct a.autono, a.baleopen, a.blautono, nvl(a.txnslno,k.txnslno)txnslno, nvl(a.gocd,k.gocd)gocd, a.stkdrcr, a.baleno, a.baleyr,a.baleno || a.baleyr BaleNoBaleYrcd, nvl(a.itcd,k.itcd)itcd, b.shade, nvl(a.nos,k.nos)nos, nvl(a.qnty,k.qnty)qnty, c.usr_entdt, ";
                    sql += "c.docno || (case when nvl(c.cancel,'N')='Y' then ' (Cancelled)' else '' end)docno, c.docdt, b.prefno, a.slcd, h.slnm, nvl(g.gonm,k.gonm)gonm, nvl(f.styleno,k.styleno)styleno, nvl(f.itnm,k.itnm)itnm, f.itgrpcd, f.uomcd, c.doccd, e.docnm, e.doctype, ";
                    sql += "b.pageno, b.pageslno, b.lrno from ";

                    sql += "( select '' baleopen, e.autono, e.blautono, a.txnslno, e.blautono||a.txnslno autoslno, a.gocd, b.stkdrcr, b.baleno, b.baleyr, b.itcd, f.mutslcd slcd, ";
                    sql += "sum(a.nos) nos, sum(a.qnty) qnty ";
                    sql += "from " + scm + ".t_bilty e, " + scm + ".t_batchdtl a, " + scm + ".t_txndtl b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".t_bilty_hdr f, " + scm + ".m_doctype g ";
                    sql += "where e.blautono = a.autono(+) and e.baleno = a.baleno(+) and e.baleyr = a.baleyr(+) and ";
                    sql += "a.autono = b.autono(+) and a.txnslno = b.slno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and ";
                    if (fdt.retStr() != "") sql += "d.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and ";
                    sql += "d.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and d.doccd=g.doccd(+) and ";
                    //sql += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and a.baleno is not null and e.autono = f.autono(+) "; // g.doctype not in ('KHSR') ";
                    sql += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and a.baleno is not null and e.autono = f.autono(+) "; // g.doctype not in ('KHSR') ";
                    sql += "group by '',  e.autono, e.blautono, a.txnslno, e.blautono||a.txnslno, a.gocd, b.stkdrcr, b.baleno, b.baleyr, b.itcd, f.mutslcd ";
                    sql += "union all ";
                    sql += "select e.baleopen, e.autono, e.blautono, a.txnslno, e.blautono||e.blslno autoslno, a.gocd, b.stkdrcr, e.baleno, e.baleyr, b.itcd, nvl(c.slcd,f.mutslcd) slcd, ";
                    sql += "sum(a.nos) nos, sum(a.qnty) qnty ";
                    sql += "from " + scm + ".t_bale e, " + scm + ".t_batchdtl a, " + scm + ".t_txndtl b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".t_bale_hdr f, " + scm + ".m_doctype g ";
                    sql += "where e.autono = a.autono(+) and e.slno=a.txnslno(+) and e.baleno = a.baleno(+) and e.baleyr = a.baleyr(+) and ";
                    sql += "not (g.doctype in ('KHSR') and a.gocd='TR') and ";
                    if (fdt.retStr() != "") sql += "d.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and ";
                    sql += "d.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and d.doccd=g.doccd(+) and ";
                    //sql += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and e.baleno is not null and e.autono = f.autono(+) and ";
                    sql += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and e.baleno is not null and e.autono = f.autono(+) and ";
                    //sql += "a.autono = b.autono(+) and a.txnslno = b.slno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) ";
                    sql += "a.autono = b.autono(+) and a.txnslno = b.slno(+) and a.autono = c.autono(+) and e.autono = d.autono(+) ";
                    sql += "group by e.baleopen, e.autono, e.blautono, a.txnslno, e.blautono||e.blslno, a.gocd, b.stkdrcr, e.baleno, e.baleyr, b.itcd, nvl(c.slcd,f.mutslcd) ";
                    sql += "union all ";
                    sql += "select e.baleopen, e.autono, e.blautono, a.txnslno, e.blautono||e.blslno autoslno, nvl(h.gocd,a.gocd) gocd, b.stkdrcr, e.baleno, e.baleyr, b.itcd, nvl(c.slcd,f.mutslcd) slcd, ";
                    sql += "sum(a.nos) nos, sum(a.qnty) qnty ";
                    sql += "from " + scm + ".t_bale e, " + scm + ".t_batchdtl a, " + scm + ".t_txndtl b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + scm + ".t_bale_hdr f, " + scm + ".m_doctype g, " + scm + ".t_batchdtl h ";
                    sql += "where e.autono = a.autono(+) and e.slno=a.txnslno(+) and e.baleno = a.baleno(+) and e.baleyr = a.baleyr(+) and ";
                    sql += "nvl(e.baleopen,'N')='Y' and e.autono=h.autono(+) and e.slno-1000 = h.slno(+) and ";
                    if (fdt.retStr() != "") sql += "d.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and ";
                    sql += "d.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and d.doccd=g.doccd(+) and ";
                    //sql += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and nvl(d.cancel, 'N') = 'N' and e.baleno is not null and e.autono = f.autono(+) and ";
                    sql += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' and e.baleno is not null and e.autono = f.autono(+) and ";
                    sql += "a.autono = b.autono(+) and a.txnslno = b.slno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) ";
                    sql += "group by e.baleopen, e.autono, e.blautono, a.txnslno, e.blautono||e.blslno, nvl(h.gocd,a.gocd), b.stkdrcr, e.baleno, e.baleyr, b.itcd, nvl(c.slcd,f.mutslcd) ) a, ";

                    sql += "(select a.autono, a.slno, b.slcd, a.autono||a.slno autoslno, b.prefno, c.lrno, a.pageno, a.pageslno, ";
                    sql += "listagg(d.shade,',') within group (order by d.autono, d.txnslno) as shade ";
                    sql += " from " + scm + ".t_txndtl a, " + scm + ".t_txn b, " + scm + ".t_txntrans c, " + scm + ".t_batchdtl d ";
                    sql += "where d.autono=a.autono(+) and d.txnslno=a.slno(+) and d.autono = b.autono(+) and d.autono = c.autono(+) and b.doctag in ('OP','PB') ";
                    sql += "group by a.autono, a.slno, b.slcd, a.autono||a.slno, b.prefno, c.lrno, a.pageno, a.pageslno ";
                    sql += ") b, ";

                    //receive from mutia item detail
                    sql += "(select a.autono||a.txnslno autoslno,a.txnslno,a.baleno,a.baleyr,sum(a.nos) nos, sum(a.qnty) qnty,b.itcd,a.gocd,f.gonm, e.styleno, e.itnm ";
                    sql += " from " + scm + ".t_batchdtl a, " + scm + ".t_txndtl b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d," + scm + ".m_sitem e," + scmf + ".m_godown f," + scm + ".m_doctype g ";
                    sql += "where a.autono = b.autono(+) and a.txnslno = b.slno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and b.itcd = e.itcd(+) and a.gocd = f.gocd(+) and d.doccd=g.doccd(+) ";
                    sql += "group by a.autono||a.txnslno,a.txnslno,a.baleno,a.baleyr,b.itcd,a.gocd,f.gonm, e.styleno, e.itnm ";
                    sql += ") k, ";

                    sql += "" + scm + ".t_cntrl_hdr c, " + scm + ".t_txn d, " + scm + ".m_doctype e, ";
                    sql += "" + scm + ".m_sitem f, " + scmf + ".m_godown g, " + scmf + ".m_subleg h, " + scm + ".t_txn i, " + scm + ".t_txntrans j ";
                    sql += "where a.autoslno = b.autoslno(+) and a.autono = c.autono(+) and a.autono = d.autono(+) and ";
                    sql += "a.blautono=i.autono(+) and a.blautono=j.autono(+) and ";
                    sql += "c.doccd = e.doccd(+) and a.itcd = f.itcd(+) and a.gocd = g.gocd(+) and a.slcd = h.slcd(+) ";
                    sql += "and a.autoslno=k.autoslno(+) and a.baleno = k.baleno(+) and a.baleyr = k.baleyr(+) ";
                    if (selbalenoyr.retStr() != "") sql += "and a.baleno||a.baleyr in (" + selbalenoyr + ") ";
                    if (unselbalenoyr.retStr() != "") sql += "and a.baleno||a.baleyr not in (" + unselbalenoyr + ") ";
                    if (selitcd.retStr() != "") sql += "and a.itcd in(" + selitcd + ") ";
                    if (unselitcd.retStr() != "") sql += "and a.itcd not in (" + unselitcd + ") ";
                    if (selitgrpcd.retStr() != "") sql += "and f.itgrpcd in(" + selitgrpcd + ") ";
                    sql += "order by baleyr, baleno, styleno, itcd, usr_entdt ";

                    DataTable tbl = MasterHelp.SQLquery(sql);
                    if (tbl.Rows.Count == 0) return Content("no records..");
                    DataView dv = new DataView(tbl);
                    dv.Sort = "baleno,styleno,usr_entdt";
                    tbl = dv.ToTable();

                    //return ReportBaleHistory(tbl, RepeatAllRow, VE.Checkbox2, VE.Checkbox6);
                    Int32 i = 0;
                    Int32 maxR = 0;
                    string chkval, chkval1 = "", chkval2 = "";
                    if (tbl.Rows.Count == 0)
                    {
                        return RedirectToAction("NoRecords", "RPTViewer");
                    }

                    DataTable IR = new DataTable("mstrep");

                    Models.PrintViewer PV = new Models.PrintViewer();
                    HtmlConverter HC = new HtmlConverter();

                    HC.RepStart(IR, 3);
                    HC.GetPrintHeader(IR, "baleno", "string", "c,12", "Bale No");
                    HC.GetPrintHeader(IR, "pblno", "string", "c,20", "P/Blno");
                    HC.GetPrintHeader(IR, "lrno", "string", "c,14", "LR No");
                    HC.GetPrintHeader(IR, "styleno", "string", "c,25", "Style No");
                    HC.GetPrintHeader(IR, "docnm", "string", "c,15", "Activity In");
                    HC.GetPrintHeader(IR, "docdt", "string", "c,11", "Doc Date");
                    HC.GetPrintHeader(IR, "docno", "string", "c,16", "Doc No");
                    HC.GetPrintHeader(IR, "slno", "string", "c,8", "Sl.");
                    HC.GetPrintHeader(IR, "gonm", "string", "c,16", "Godown");
                    HC.GetPrintHeader(IR, "slnm", "string", "c,22", "Particulars");
                    HC.GetPrintHeader(IR, "nos", "double", "n,7", "Nos");
                    HC.GetPrintHeader(IR, "qnty", "double", "n,16,2", "Qnty");

                    double qty, amt = 0;
                    double tqty = 0, tnos = 0;
                    double gtqty = 0, gtnos = 0;

                    Int32 rNo = 0;
                    string baleno = "";
                    // Report begins
                    i = 0; maxR = tbl.Rows.Count - 1;
                    int count = 0;
                    while (i <= maxR)
                    {
                        chkval = tbl.Rows[i]["BaleNoBaleYrcd"].ToString();
                        baleno = tbl.Rows[i]["baleno"].ToString();
                        qty = 0; amt = 0;
                        bool balefirst = true;
                        while (tbl.Rows[i]["BaleNoBaleYrcd"].ToString() == chkval)
                        {
                            bool itemfirst = true;
                            chkval2 = tbl.Rows[i]["itcd"].ToString();
                            while (tbl.Rows[i]["BaleNoBaleYrcd"].ToString() == chkval && tbl.Rows[i]["itcd"].ToString() == chkval2)
                            {
                                tnos = tnos + tbl.Rows[i]["qnty"].retDbl();
                                tqty = tqty + tbl.Rows[i]["qnty"].retDbl();

                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                if (RepeatAllRow == true || balefirst == true) IR.Rows[rNo]["baleno"] = tbl.Rows[i]["baleno"].retStr();
                                if (RepeatAllRow == true || itemfirst == true) IR.Rows[rNo]["styleno"] = tbl.Rows[i]["styleno"].ToString();
                                if (RepeatAllRow == true || balefirst == true) IR.Rows[rNo]["pblno"] = tbl.Rows[i]["prefno"].ToString();
                                if (RepeatAllRow == true || balefirst == true) IR.Rows[rNo]["lrno"] = tbl.Rows[i]["lrno"].ToString();
                                IR.Rows[rNo]["docnm"] = tbl.Rows[i]["docnm"].ToString();
                                IR.Rows[rNo]["docdt"] = Convert.ToString(tbl.Rows[i]["docdt"]).Substring(0, 10);
                                IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].ToString();
                                IR.Rows[rNo]["slno"] = tbl.Rows[i]["txnslno"].ToString();
                                IR.Rows[rNo]["gonm"] = tbl.Rows[i]["gonm"].ToString();
                                IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"].ToString() + (tbl.Rows[i]["baleopen"].retStr() == "Y" ? " (Open)" : "");
                                IR.Rows[rNo]["nos"] = tbl.Rows[i]["nos"].retDbl();
                                IR.Rows[rNo]["qnty"] = tbl.Rows[i]["qnty"].retDbl();
                                itemfirst = false; balefirst = false;
                                i = i + 1;
                                if (i > maxR) break;
                            }
                            count++;
                            if (i > maxR) break;
                        }
                        if (RepeatAllRow == false)
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["dammy"] = "";
                            IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                        }

                        gtqty = gtqty + tnos;
                        gtnos = gtnos + tqty;
                    }
                    // Create Blank line
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["dammy"] = " ";
                    IR.Rows[rNo]["flag"] = " height:14px; ";

                    string pghdr1 = " Bale History " + (fdt != "" ? " from " + fdt + " to " : "as on ") + tdt;
                    string repname = "Bale Report";
                    PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "L", false);

                    TempData[repname] = PV;
                    TempData[repname + "xxx"] = IR;
                    return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
                }
                else
                {
                    sql += "select a.gocd, c.gonm, a.doccd, b.docnm, a.dttag, ";
                    sql += "sum((case a.drcr when 'D' then 1 when 'C' then - 1 end)) qty from ";
                    sql += " (select distinct b.doccd, a.drcr, a.gocd, a.baleno, a.baleyr, ";//c.itcd,d.itgrpcd,
                    sql += " (case when b.docdt < to_date('" + fdt + "', 'dd/mm/yyyy') then 'OP' else 'CR' end) dttag ";
                    sql += "  from " + scm + ".t_bale a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_txndtl c," + scm + ".m_sitem d ";
                    sql += "where a.autono = b.autono(+) and a.autono = c.autono and c.itcd = d.itcd and a.gocd is not null and ";
                    sql += "b.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') and ";
                    sql += "b.compcd = '" + COM + "' and nvl(b.cancel, 'N')= 'N' ) a, ";
                    sql += "" + scm + ".m_doctype b, " + scmf + ".m_godown c ";
                    sql += "where a.doccd = b.doccd(+) and a.gocd = c.gocd(+) ";
                    if (selbalenoyr.retStr() != "") sql += "and a.baleno||a.baleyr in (" + selbalenoyr + ") ";
                    if (unselbalenoyr.retStr() != "") sql += "and a.baleno||a.baleyr not in (" + unselbalenoyr + ") ";
                    if (selitcd.retStr() != "") sql += "and a.itcd in(" + selitcd + ") ";
                    if (unselitcd.retStr() != "") sql += "and a.itcd not in (" + unselitcd + ") ";
                    if (selitgrpcd.retStr() != "") sql += "and a.itgrpcd in(" + selitgrpcd + ") ";
                    sql += "group by a.gocd, c.gocd, c.gonm, a.doccd, b.docnm, a.dttag ";
                    sql += "order by gonm, gocd, docnm, doccd ";
                    DataTable tbl = MasterHelp.SQLquery(sql);
                    if (tbl.Rows.Count == 0) return Content("no records..");
                    //return ReportBaleMovement(tbl);

                    Models.PrintViewer PV = new Models.PrintViewer();
                    HtmlConverter HC = new HtmlConverter();
                    DataTable IR = new DataTable("");
                    Int32 rNo = 0, maxR = 0, i = 0;
                    DataTable Doctbl = tbl.DefaultView.ToTable(true, "doccd", "docnm");
                    #region GodownWise
                    DataTable amtDT = new DataTable("goDT");
                    string[] amtTBLCOLS = new string[] { "gocd", "gonm" };
                    amtDT = tbl.DefaultView.ToTable(true, amtTBLCOLS);
                    amtDT.Columns.Add("goqty", typeof(double));

                    HC.RepStart(IR, 3);
                    HC.GetPrintHeader(IR, "docnm", "string", "C,14", "Document Name");
                    //HC.GetPrintHeader(IR, "closing", "double", "n,14,2", "Closing");
                    foreach (DataRow dr in amtDT.Rows)
                    {
                        HC.GetPrintHeader(IR, dr["gocd"].ToString(), "double", "n,12,2", dr["gonm"].ToString());
                    }
                    HC.GetPrintHeader(IR, "totcolqnty", "double", "n,12,2", "Total Qty");
                    maxR = tbl.Rows.Count - 1;
                    i = 0;

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    //IR.Rows[rNo]["docnm"] = "Openig";
                    IR.Rows[rNo]["docnm"] = "<span style='font-weight:100;font-size:9px;'>" + " Openig  " + " </span>";
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                    foreach (DataRow dr in Doctbl.Rows)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["docnm"] = dr["docnm"].ToString();
                    }
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["docnm"] = "Closing";
                    IR.Rows[rNo]["docnm"] = "<span style='font-weight:100;font-size:9px;'>" + " Closing  " + " </span>";
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";

                    while (i <= maxR)
                    {
                        string docnm = tbl.Rows[i]["docnm"].ToString();
                        double qty = tbl.Rows[i]["qty"].retDbl();
                        string gocd = tbl.Rows[i]["gocd"].ToString();
                        string doctag = tbl.Rows[i]["dttag"].ToString();
                        if (doctag == "OP")
                        {
                            rNo = 0;
                        }
                        else
                        {
                            DataRow dr = IR.Select("docnm='" + docnm + "'").FirstOrDefault();
                            if (dr != null)
                            {
                                rNo = IR.Rows.IndexOf(dr);
                            }
                        }
                        double OLDqnty = IR.Rows[rNo][gocd].retDbl();
                        IR.Rows[rNo][gocd] = OLDqnty + qty;

                        double totcolqnty = IR.Rows[rNo]["totcolqnty"].retDbl();
                        IR.Rows[rNo]["totcolqnty"] = totcolqnty + qty;

                        double Closing = IR.Rows[IR.Rows.Count - 1][gocd].retDbl();
                        IR.Rows[IR.Rows.Count - 1][gocd] = Closing + qty;

                        double colsingtotcolqnty = IR.Rows[IR.Rows.Count - 1]["totcolqnty"].retDbl();
                        IR.Rows[IR.Rows.Count - 1]["totcolqnty"] = colsingtotcolqnty + qty;

                        i++;
                        if (i > maxR) break;
                    }
                    #endregion
                    // Create Blank line
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["dammy"] = " ";
                    IR.Rows[rNo]["flag"] = " height:14px; ";

                    string pghdr1 = " Bale History Movement " + (fdt != "" ? " from " + fdt + " to " : "as on ") + tdt;
                    string repname = "Bale Report";


                    PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "L", false);

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
        }

    }
}