using System;
using System.Linq;
using System.Data;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;

namespace Improvar.Controllers
{
    public class Rep_Stk_LegController : Controller
    {
        string CS = null;
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
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ViewBag.formname = "Stock Ledger";
                    if(VE.MENU_PARA=="DY") ViewBag.formname = "Dyer Register";
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    VE.FDT = CommVar.FinStartDate(UNQSNO);
                    VE.TDT = CommVar.CurrDate(UNQSNO);
                    VE.TEXTBOX2 = "P"; //Pcs, Box
                    VE.TEXTBOX1 = "S"; //Super summary, Item, Group
                    if (VE.MENU_PARA != null) VE.TEXTBOX3 = VE.MENU_PARA; else VE.TEXTBOX3 = "FS"; //Finish Stock
                    VE.TEXTBOX6 = DB.M_MTRLJOBMST.Find(VE.TEXTBOX3)?.MTRLJOBNM;
                    string comcd = CommVar.Compcd(UNQSNO);
                    string location = CommVar.Loccd(UNQSNO);

                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);

                    VE.DropDown_list_ITGRP = DropDownHelp.GetItgrpcdforSelection();
                    VE.Itgrpnm = MasterHelp.ComboFill("itgrpcd", VE.DropDown_list_ITGRP, 0, 1);

                    VE.DropDown_list_BRAND = DropDownHelp.GetBrandcddforSelection();
                    VE.Brandnm = MasterHelp.ComboFill("brandcd", VE.DropDown_list_BRAND, 0, 1);

                    VE.DropDown_list4 = (from a in DB.M_SIZE select new DropDown_list4() { text = a.SIZENM, value = a.SIZECD }).ToList();//sizes
                    VE.TEXTBOX7 = MasterHelp.ComboFill("itsize", VE.DropDown_list4, 0, 1);

                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("D,C,J");
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);

                    VE.DropDown_list_StkType = Salesfunc.GetforStkTypeSelection();
                    VE.TEXTBOX8 = MasterHelp.ComboFill("stktype", VE.DropDown_list_StkType, 0, 1);

                    VE.DropDown_list_DOCCD = DropDownHelp.GetDocCdforSelection();
                    VE.Docnm = MasterHelp.ComboFill("doccd", VE.DropDown_list_DOCCD, 0, 1);

                    VE.DropDown_list_AGSLCD = DropDownHelp.GetAgSlcdforSelection();
                    VE.Agslnm = MasterHelp.ComboFill("agslcd", VE.DropDown_list_AGSLCD, 0, 1);

                    VE.DefaultView = true;
                    VE.ExitMode = 1;
                    VE.DefaultDay = 0;
                    VE.Checkbox2 = true; //Stock Adj. Considered
                    VE.Checkbox3 = false; //Merge Locations
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                ReportViewinHtml VE = new ReportViewinHtml();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        //public ActionResult GetGodownDetails(string val)
        //{
        //    try
        //    {
        //        if (val == null)
        //        {
        //            return PartialView("_Help2", MasterHelp.GOCD_help(val));
        //        }
        //        else
        //        {
        //            string str = MasterHelp.GOCD_help(val);
        //            return Content(str);
        //        }
        //    }
        //    catch (Exception Ex)
        //    {
        //        Cn.SaveException(Ex, "");
        //        return Content(Ex.Message + Ex.InnerException);
        //    }
        //}
        public ActionResult GetGodownDetails(string val)
        {
            try
            {
                var str = MasterHelp.GOCD_help(val);
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
        public ActionResult GetMtrlJobDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", MasterHelp.MTRLJOBCD_help(val));
                }
                else
                {
                    string str = MasterHelp.MTRLJOBCD_help(val);
                    return Content(str);
                }
            }
            catch (Exception Ex)
            {
                Cn.SaveException(Ex, "");
                return Content(Ex.Message + Ex.InnerException);
            }
        }
        [HttpPost]
        public ActionResult Rep_Stk_Leg(ReportViewinHtml VE, FormCollection FC)
        {
            string ModuleCode = Module.Module_Code;
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string brandcd = "", itgrpcd = "", itcd = "", sizecd = "", gocd = "", stktype = "", mtrljobcd = "", fdt, tdt = "", slcd = "", agslcd = "";
                string seldoccd = "", seldoctype = "";
                string unselitcd = "", unselslcd = "";
                //string stkcalcon = VE.TEXTBOX2; // P=Pcs, B=Box
                string reptype = VE.TEXTBOX1; //Summary, I=Item, G=Group;
                string pghdr2 = "";
                fdt = VE.FDT;
                tdt = VE.TDT;
                if (VE.TEXTBOX4 != null) gocd = "'" + VE.TEXTBOX4 + "'";

                if (FC.AllKeys.Contains("brandcdvalue"))
                {
                    brandcd = FC["brandcdvalue"].ToString().retSqlformat();
                    pghdr2 += (pghdr2.retStr() == "" ? "" : "<br/>") + "Brand :" + FC["brandcdtext"].ToString();
                }
                if (FC.AllKeys.Contains("itgrpcdvalue"))
                {
                    itgrpcd = FC["itgrpcdvalue"].ToString().retSqlformat();
                    pghdr2 += (pghdr2.retStr() == "" ? "" : "<br/>") + "Item Group :" + FC["itgrpcdtext"].ToString();
                }
                if (FC.AllKeys.Contains("itcdvalue"))
                {
                    itcd = FC["itcdvalue"].ToString().retSqlformat();
                    pghdr2 += (pghdr2.retStr() == "" ? "" : "<br/>") + "Articles :" + FC["itcdtext"].ToString();
                }
                //if (FC.AllKeys.Contains("ITSIZES")) sizecd = FC["ITSIZES"].ToString().retSqlformat();
                if (FC.AllKeys.Contains("itsizevalue"))
                {
                    sizecd = FC["itsizevalue"].ToString().retSqlformat();
                    pghdr2 += (pghdr2.retStr() == "" ? "" : "<br/>") + "Item Sizes :" + FC["itsizetext"].ToString();
                }
                //if (FC.AllKeys.Contains("STKTYPE")) stktype = FC["STKTYPE"].ToString().retSqlformat();
                if (FC.AllKeys.Contains("stktypevalue"))
                {
                    stktype = FC["stktypevalue"].ToString().retSqlformat();
                    pghdr2 += (pghdr2.retStr() == "" ? "" : "<br/>") + "Stock type :" + FC["stktypetext"].ToString();
                }
                if (FC.AllKeys.Contains("agslcdvalue"))
                {
                    agslcd = FC["agslcdvalue"].ToString().retSqlformat();
                    pghdr2 += (pghdr2.retStr() == "" ? "" : "<br/>") + "Agent :" + FC["agslcdtext"].ToString();
                }
                if (FC.AllKeys.Contains("slcdvalue"))
                {
                    slcd = FC["slcdvalue"].ToString().retSqlformat();
                    pghdr2 += (pghdr2.retStr() == "" ? "" : "<br/>") + "Party :" + FC["slcdtext"].ToString();
                }
                if (FC.AllKeys.Contains("doccdvalue"))
                {
                    seldoccd = CommFunc.retSqlformat(FC["doccdvalue"].ToString());
                    pghdr2 += (pghdr2.retStr() == "" ? "" : "<br/>") + "Document Type :" + FC["doccdtext"].ToString();
                }

                if (FC.AllKeys.Contains("itcdunselvalue")) unselitcd = FC["itcdunselvalue"].ToString().retSqlformat();
                if (FC.AllKeys.Contains("slcdunselvalue")) unselslcd = FC["slcdunselvalue"].ToString().retSqlformat();

                mtrljobcd = "'FS'";
                if (VE.TEXTBOX3 != null) mtrljobcd = "'" + VE.TEXTBOX3 + "'";
                string ffdt = Convert.ToDateTime(fdt.retDateStr()).AddDays(-1).ToString().retDateStr();

                string sql = "";

                sql = "select doccd from " + scm + ".m_doctype where doctype in ('STCNV') ";
                DataTable rstmp = MasterHelp.SQLquery(sql);
                string skipdoccd = "";
                if (VE.Checkbox2 == false)
                {
                    for (int s = 0; s <= rstmp.Rows.Count - 1; s++)
                    {
                        if (skipdoccd != "") skipdoccd += ",";
                        skipdoccd += "'" + rstmp.Rows[s]["doccd"].ToString() + "'";
                    }
                }

                bool showsizes = false;
                if ((VE.TEXTBOX1 == "I" || VE.TEXTBOX1 == "G") && (VE.Checkbox1 == true))
                {
                    showsizes = true;
                }

                sql = "";
                sql += "select a.autono||a.cancel autono, a.cancel, b.slcd, s.slnm, c.docno, b.prefno, a.docdt, a.doctag, a.itcd,d.uomcd,a.partcd, a.mtrljobcd, a.stktype, a.itcolsize, ";
                if (showsizes == true) sql += "a.sizecd, ";
                sql += "d.itnm, d.styleno, nvl(d.pcsperset,0) pcsperset, ";
                sql += "d.itgrpcd, e.itgrpnm, ";
                sql += "d.brandcd, f.brandnm, a.stkdrcr, a.qnty,i.slnm agslnm from ( ";
                sql += "select a.autono, a.cancel, a.docdt, a.doccd, a.doctag, a.itcd, a.partcd, a.mtrljobcd, a.stktype, a.itcolsize," + (showsizes == true ? "a.sizecd, " : "") + " a.stkdrcr, sum(a.qnty) qnty from ( ";

                sql += "select (case when c.docdt < to_date('" + fdt + "','dd/mm/yyyy') then 'opng' else a.autono end) autono, nvl(c.cancel,'N') cancel, ";
                sql += "(case when c.docdt < to_date('" + fdt + "','dd/mm/yyyy') then to_date('" + ffdt + "','dd/mm/yyyy')  else c.docdt end) docdt, ";
                sql += "c.doccd, b.doctag, nvl(d.linkitcd,a.itcd) itcd, a.partcd, a.mtrljobcd, nvl(a.stktype,'F') stktype, ";
                sql += "nvl(a.stktype,'F')||a.itcd" + (showsizes == true ? "||a.sizecd" : "") + " itcolsize, " + (showsizes == true ? "a.sizecd, " : "") + "a.stkdrcr, ";
                //sql += "(case when a.mtrljobcd in ('YP','YD','GT','FT','TF','WA','PF','WS') then nvl(a.qnty,0) else nvl(nvl(e.qnty,a.qnty),0) end)*decode(nvl(e.skipstk,'N'),'Y',0,1) qnty ";
                sql += "nvl(a.qnty,0) qnty ";
                sql += "from " + scm + ".t_txndtl a, " + scm + ".t_txn b, " + scm + ".t_cntrl_hdr c, " + scm + ".m_sitem d, " + scm + ".t_batchdtl e, " + scm + ".t_batchmst f ";
                sql += "where a.autono=b.autono and a.autono=c.autono and a.stkdrcr in ('D','C') and ";
                sql += "a.autono=e.autono(+) and a.slno=e.slno(+) and e.autono=f.autono(+) and e.slno=f.slno(+) and ";
                //sql += "(case when a.mtrljobcd in ('YP','YD','GT','FT','TF') then a.mtrljobcd else nvl(f.jobcd,a.mtrljobcd) end) in (" + mtrljobcd + ") and ";
                sql += "a.mtrljobcd in (" + mtrljobcd + ") and ";
                if (gocd != "") sql += "b.gocd in (" + gocd + ") and ";
                if (stktype != "") sql += "nvl(a.stktype,'F') in (" + stktype + ") and ";
                if (sizecd != "") sql += "a.sizecd in (" + sizecd + ") and ";
                sql += "c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and a.itcd=d.itcd and ";
                if (VE.Checkbox3 == false) sql += "c.loccd='" + LOC + "' and ";
                sql += "c.compcd='" + COM + "' ";

                sql += "union all ";

                sql += "select (case when to_date(to_char(c.canc_usr_entdt,'dd/mm/yyyy'),'dd/mm/yyyy') < to_date('" + fdt + "','dd/mm/yyyy') then 'opng' else a.autono end) autono, 'R' cancel, ";
                sql += "(case when to_date(to_char(c.canc_usr_entdt,'dd/mm/yyyy'),'dd/mm/yyyy') < to_date('" + fdt + "','dd/mm/yyyy') then to_date('" + ffdt + "','dd/mm/yyyy')  else to_date(to_char(c.canc_usr_entdt,'dd/mm/yyyy'),'dd/mm/yyyy') end) docdt, ";
                sql += "c.doccd, b.doctag, nvl(d.linkitcd,a.itcd) itcd, a.partcd, a.mtrljobcd, nvl(a.stktype,'F') stktype, ";
                sql += "nvl(a.stktype,'F')||a.itcd" + (showsizes == true ? "||a.sizecd" : "") + " itcolsize, " + (showsizes == true ? "a.sizecd, " : "");
                //if (reptype != "T") sql += "decode(a.stkdrcr,'D','C','D') stkdrcr, (case when a.mtrljobcd in ('YP','YD','GT','FT','TF','WA','PF') then nvl(a.qnty,0) else nvl(nvl(e.qnty,a.qnty),0) end) qnty ";
                //else sql += "a.stkdrcr, (case when a.mtrljobcd in ('YP','YD','GT','FT','TF','WA','PF','WS') then nvl(a.qnty,0) else nvl(nvl(e.qnty,a.qnty),0) end)*-1*decode(nvl(e.skipstk,'N'),'Y',0,1) qnty ";
                if (reptype != "T") sql += "decode(a.stkdrcr,'D','C','D') stkdrcr, (case when a.mtrljobcd in ('YP','YD','GT','FT','TF','PF') then nvl(a.qnty,0) else nvl(nvl(e.qnty,a.qnty),0) end) qnty ";
                else sql += "a.stkdrcr, (case when a.mtrljobcd in ('YP','YD','GT','FT','TF') then nvl(a.qnty,0) else nvl(nvl(e.qnty,a.qnty),0) end) qnty ";
                sql += "from " + scm + ".t_txndtl a, " + scm + ".t_txn b, " + scm + ".t_cntrl_hdr c, " + scm + ".m_sitem d, " + scm + ".t_batchdtl e, " + scm + ".t_batchmst f ";
                sql += "where a.autono=b.autono and a.autono=c.autono and a.stkdrcr in ('D','C') and ";
                sql += "a.autono=e.autono(+) and a.slno=e.slno(+) and e.autono=f.autono(+) and e.slno=f.slno(+) and ";
                //sql += "(case when a.mtrljobcd in ('YP','YD','GT','FT','TF','WA','PF','WS') then a.mtrljobcd else nvl(f.jobcd,a.mtrljobcd) end) in (" + mtrljobcd + ") and ";
                sql += "a.mtrljobcd in (" + mtrljobcd + ") and ";
                if (gocd != "") sql += "b.gocd in (" + gocd + ") and ";
                if (stktype != "") sql += "nvl(a.stktype,'F') in (" + stktype + ") and ";
                if (sizecd != "") sql += "a.sizecd in (" + sizecd + ") and ";
                sql += "to_date(to_char(c.canc_usr_entdt,'dd/mm/yyyy'),'dd/mm/yyyy') <= to_date('" + tdt + "','dd/mm/yyyy') and a.itcd=d.itcd and ";
                if (VE.Checkbox3 == false) sql += "c.loccd='" + LOC + "' and ";
                sql += "c.compcd='" + COM + "' and nvl(c.cancel,'N')='Y' ";

                sql += ") a group by a.autono, a.cancel, a.docdt, a.doccd, a.doctag, a.itcd, a.partcd, a.mtrljobcd, a.stktype, a.itcolsize, " + (showsizes == true ? "a.sizecd," : "") + " a.stkdrcr  ) a, ";

                sql += scm + ".t_txn b, " + scm + ".t_cntrl_hdr c, " + scmf + ".m_subleg s, ";
                sql += scm + ".m_sitem d, " + scm + ".m_group e, " + scm + ".m_brand f, " + scm + ".m_doctype g, " + scm + ".t_txnoth h, " + scmf + ".m_subleg i ";
                sql += "where a.itcd=d.itcd(+) and d.itgrpcd=e.itgrpcd(+) and d.brandcd=f.brandcd(+) and c.doccd=g.doccd(+) and ";
                if (itcd != "") sql += "a.itcd in (" + itcd + ") and ";
                if (unselitcd != "") sql += "a.itcd not in (" + unselitcd + ") and ";
                if (brandcd != "") sql += "d.brandcd in (" + brandcd + ") and ";
                if (itgrpcd != "") sql += "d.itgrpcd in (" + itgrpcd + ") and ";
                if (skipdoccd != "") sql += "a.doccd not in (" + skipdoccd + ") and ";
                if (slcd != "") sql += "b.slcd in (" + slcd + ") and ";
                if (unselslcd != "") sql += "b.slcd not in (" + unselslcd + ") and ";
                if (seldoccd != "") sql += "c.doccd in (" + seldoccd + ") and ";
                if (seldoctype != "") sql += "g.doctype in (" + seldoctype + ") and ";
                sql += "a.autono=b.autono(+) and a.autono=c.autono(+) and b.slcd=s.slcd(+) and b.autono=h.autono(+) and h.agslcd=i.slcd(+) ";
                if (agslcd != "") sql += "and h.agslcd in (" + agslcd + ") ";
                sql += "order by ";
                if (reptype == "S") sql += "docdt, docno, autono, styleno, itcd, " + (showsizes == true ? "sizecd," : "") + "autono ";
                else if (reptype == "G") sql += "itgrpnm, itgrpcd, docdt, docno, autono ";
                else sql += "itgrpnm, itgrpcd, styleno, itcd,docdt, docno, autono " + (showsizes == true ? ",sizecd" : "");

                DataTable tbl = MasterHelp.SQLquery(sql);
                if (tbl.Rows.Count == 0)
                {
                    return Content("No Record Found");
                }

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                mtrljobcd = VE.TEXTBOX3;
                string qtyhd = "", qtydsp = "", qtydsp1 = "n,17,2:##,##,##,##0.00";
                //if (mtrljobcd == "YP" || mtrljobcd == "FT" || mtrljobcd == "GT" || mtrljobcd == "YD" || mtrljobcd == "WA" || mtrljobcd == "PF" || mtrljobcd == "TF") stkcalcon = "Q";

                //if (stkcalcon == "P" || stkcalcon == "Z") { qtyhd = "Pcs"; qtydsp = "n,17,2:####,##,##,##0"; }
                //else if (stkcalcon == "B") { qtyhd = "Box"; qtydsp = "n,17,2:##,##,##,##0.00"; }
                //else if (stkcalcon == "S") { qtyhd = "Set"; qtydsp = "n,17,2:##,##,##,##0.00"; }
                //else { qtyhd = "Un"; qtydsp = "n,17,2:####,##,##0.000"; }
                //if (stkcalcon == "Q") qtydsp = "n,17,2:####,##,##0.000";

                DataTable IR = new DataTable("mstrep");

                string stritgrpcd = "", stritcd="", chk1 = "", chk2 = "";
                Int32 maxR = 0, i = 0, rNo = 0;

                double cop = 0, cop1 = 0, cdr = 0, ccr = 0, cdr1 = 0, ccr1 = 0, cpcs = 0, cbox = 0, cqty = 0, cret = 0, cret1 = 0, cbal = 0, cbal1 = 0;
                double chkpcsdr = 0, chkpcscr = 0, chkpcsret = 0, lpcsdr = 0, lpcscr = 0, lpcsret = 0;
                string autono = "", itmdsp = "", ichk = "", lastdt = "", chk3 = "";
                double chkpcs = 0; cpcs = 0;
                double top = 0, tdr = 0, tcr = 0, tret = 0, tbal = 0;
                double iop = 0, idr = 0, icr = 0, iret = 0, ibal = 0;
                double gop = 0, gdr = 0, gcr = 0, gret = 0, gbal = 0;

                double top1 = 0, tdr1 = 0, tcr1 = 0, tret1 = 0, tbal1 = 0;
                double iop1 = 0, idr1 = 0, icr1 = 0, iret1 = 0, ibal1 = 0;
                double gop1 = 0, gdr1 = 0, gcr1 = 0, gret1 = 0, gbal1 = 0;

                if (reptype != "T")
                {
                    #region Stock Ledger
                    i = 0; maxR = tbl.Rows.Count - 1;
                    HC.RepStart(IR, 3);
                    HC.GetPrintHeader(IR, "docdt", "string", "c,10", "DocDate");
                    HC.GetPrintHeader(IR, "docno", "string", "c,15", "DocNo.");
                    HC.GetPrintHeader(IR, "prefno", "string", "c,15", "Party Ref#");
                    //HC.GetPrintHeader(IR, "slcd", "string", "c,8", "Party Cd");
                    HC.GetPrintHeader(IR, "slnm", "string", "c,45", "Party Name");
                    //HC.GetPrintHeader(IR, "agslnm", "string", "c,45", "Agent Name");
                    //if (reptype != "I") HC.GetPrintHeader(IR, "itnm", "string", "c,35", "Item");
                    if (showsizes == true)
                    {
                        //HC.GetPrintHeader(IR, "pcsperbox", "double", "n,5", "P/Box");
                        //HC.GetPrintHeader(IR, "sizedsp", "string", "c,50", "Sizes;(Box)");
                    }
                    HC.GetPrintHeader(IR, "inqnty", "double", qtydsp1, "In " + qtyhd);
                    HC.GetPrintHeader(IR, "outqnty", "double", qtydsp1, "Out " + qtyhd);
                    if (slcd == "") HC.GetPrintHeader(IR, "balqnty", "double", qtydsp1, "Bal " + qtyhd);

                    while (i <= maxR)
                    {
                        stritgrpcd = tbl.Rows[i]["itgrpcd"].ToString();
                        stritcd = tbl.Rows[i]["itcd"].ToString();

                        if (reptype != "S")
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["Dammy"] = "[ " + stritgrpcd + "  " + "] " + tbl.Rows[i]["itgrpnm"];
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                        }
                        //else if(reptype=="S")
                        //{
                        //    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        //    IR.Rows[rNo]["dammy"] = "[ " + tbl.Rows[i]["itcd"].retStr() + "  " + " ] " + tbl.Rows[i]["styleno"].ToString() + " , " + tbl.Rows[i]["itnm"]+"[" + tbl.Rows[i]["uomcd"].ToString() + "]";
                        
                        //    IR.Rows[rNo]["flag"] = "itnm=font-weight:bold;font-size:13px; ";
                        //}
                        gdr = 0; gcr = 0; gbal = 0;
                        while (tbl.Rows[i]["itcd"].ToString() == stritcd)
                        {
                            chk1 = "itgrpcd"; chk2 = tbl.Rows[i]["itgrpcd"].ToString();
                            if (reptype == "I"|| reptype == "S")
                            {
                                chk1 = "itcd"; chk2 = tbl.Rows[i][chk1].ToString();

                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["Dammy"] = "[ " + chk2 + "  " + " ] " + tbl.Rows[i]["styleno"].ToString() + " , " + tbl.Rows[i]["itnm"] + "[" + tbl.Rows[i]["uomcd"].ToString() + "]";
                                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                            }

                            idr = 0; icr = 0; ibal = 0;
                            while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2)
                            {
                                cop = 0; cdr = 0; ccr = 0; cpcs = 0; cbox = 0; cqty = 0; cbal = 0;
                                while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(fdt))
                                {
                                    ichk = tbl.Rows[i]["itcd"].ToString();
                                    chkpcs = 0; cpcs = 0;
                                    while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(fdt) && tbl.Rows[i]["itcd"].ToString() == ichk)
                                    {
                                        autono = tbl.Rows[i]["autono"].ToString();
                                        string partcd = tbl.Rows[i]["partcd"].ToString();
                                        while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(fdt) && tbl.Rows[i]["itcd"].ToString() == ichk && tbl.Rows[i]["autono"].ToString() == autono)
                                        {
                                            if (tbl.Rows[i]["partcd"].ToString() == partcd)
                                            {
                                                int mult = 0;
                                                if (tbl.Rows[i]["stkdrcr"].ToString() == "D") mult = 1; else mult = -1;
                                                if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcs = chkpcs + (Convert.ToDouble(tbl.Rows[i]["qnty"]) * mult);
                                                cpcs = cpcs + (Convert.ToDouble(tbl.Rows[i]["qnty"]) * mult);
                                            }
                                            i++;
                                            if (i > maxR) break;
                                        }
                                        if (i > maxR) break;
                                    }
                                    //if (stkcalcon == "B") cqty = Salesfunc.ConvPcstoBox(chkpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
                                    // if (stkcalcon == "S") cqty = Salesfunc.ConvPcstoSet(chkpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
                                    //else cqty = cpcs;
                                    cqty = cpcs;
                                    cop = cop + cqty;
                                    if (i > maxR) break;
                                }
                                if (cop != 0)
                                {
                                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                    IR.Rows[rNo]["docdt"] = fdt;
                                    IR.Rows[rNo]["slnm"] = "Opening Stock";
                                    if (slcd == "") IR.Rows[rNo]["balqnty"] = cop;
                                }
                                cbal = cop;
                                bool itotadd = true;
                                string sizes = "";

                                if (i <= maxR)
                                {
                                    lastdt = tbl.Rows[i]["docdt"].ToString();
                                    while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && Convert.ToDateTime(tbl.Rows[i]["docdt"]) <= Convert.ToDateTime(tdt))
                                    {
                                        sizes = "";
                                        itotadd = false;
                                        autono = tbl.Rows[i]["autono"].ToString(); itmdsp = "";
                                        while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && tbl.Rows[i]["autono"].ToString() == autono)
                                        {

                                            ichk = tbl.Rows[i]["itcd"].ToString();
                                            chkpcsdr = 0; chkpcscr = 0; lpcsdr = 0; lpcscr = 0;
                                            while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && tbl.Rows[i]["autono"].ToString() == autono && tbl.Rows[i]["itcd"].ToString() == ichk)
                                            {
                                                autono = tbl.Rows[i]["autono"].ToString();
                                                string partcd = tbl.Rows[i]["partcd"].ToString();

                                                while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && tbl.Rows[i]["autono"].ToString() == autono && tbl.Rows[i]["itcd"].ToString() == ichk && tbl.Rows[i]["autono"].ToString() == autono)
                                                {
                                                    if (showsizes == true)
                                                    {
                                                        chk3 = tbl.Rows[i]["itcolsize"].ToString();
                                                        string size = tbl.Rows[i]["sizecd"].ToString();
                                                        double pcs = 0;
                                                        while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && tbl.Rows[i]["autono"].ToString() == autono && tbl.Rows[i]["itcd"].ToString() == ichk && tbl.Rows[i]["autono"].ToString() == autono && tbl.Rows[i]["itcolsize"].ToString() == chk3)
                                                        {
                                                            if (tbl.Rows[i]["partcd"].ToString() == partcd)
                                                            {
                                                                int mult = 0;
                                                                if (tbl.Rows[i]["stkdrcr"].ToString() == "D")
                                                                {
                                                                    if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcsdr = chkpcsdr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                                                                    lpcsdr = lpcsdr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                                                                }
                                                                else
                                                                {
                                                                    if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcscr = chkpcscr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                                                                    lpcscr = lpcscr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                                                                    pcs += tbl.Rows[i]["qnty"].retDbl();
                                                                }
                                                            }
                                                            i++;
                                                            if (i > maxR) break;
                                                        }
                                                        //double dbboxes = Salesfunc.ConvPcstoBox(pcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
                                                        if (sizes != "") sizes += ", ";
                                                        //sizes += Salesfunc.retsizemaxmin(size) + "=" + dbboxes.ToString();
                                                    }
                                                    else
                                                    {
                                                        if (tbl.Rows[i]["partcd"].ToString() == partcd)
                                                        {
                                                            int mult = 0;
                                                            if (tbl.Rows[i]["stkdrcr"].ToString() == "D")
                                                            {
                                                                if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcsdr = chkpcsdr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                                                                lpcsdr = lpcsdr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                                                            }
                                                            else
                                                            {
                                                                if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcscr = chkpcscr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                                                                lpcscr = lpcscr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                                                            }
                                                        }
                                                        i++;
                                                    }

                                                    if (i > maxR) break;
                                                }
                                                if (i > maxR) break;
                                            }
                                           
                                            
                                                lpcsdr = Salesfunc.ConvPcstoSet(chkpcsdr, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
                                                lpcscr = Salesfunc.ConvPcstoSet(chkpcscr, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
                                          
                                            cqty = lpcsdr + lpcscr;
                                            cdr = cdr + lpcsdr; ccr = ccr + lpcscr;
                                            itmdsp += tbl.Rows[i - 1]["styleno"].ToString() + " " + tbl.Rows[i - 1]["partcd"].ToString() + "=" + cqty.ToString() + ",";

                                            if (i > maxR) break;
                                        }
                                        string cancdsc = "";

                                        switch (tbl.Rows[i - 1]["cancel"].ToString())
                                        {
                                            case "Y":
                                                cancdsc = " (Cancel)"; break;
                                            case "R":
                                                cancdsc = " (Reverse)"; break;
                                        }
                                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                        IR.Rows[rNo]["docdt"] = tbl.Rows[i - 1]["docdt"].ToString().retDateStr();
                                        IR.Rows[rNo]["docno"] = tbl.Rows[i - 1]["docno"] + cancdsc;
                                        IR.Rows[rNo]["prefno"] = tbl.Rows[i - 1]["prefno"];
                                        IR.Rows[rNo]["slnm"] = tbl.Rows[i - 1]["slnm"];
                                        //IR.Rows[rNo]["agslnm"] = tbl.Rows[i - 1]["agslnm"];
                                        //if (reptype != "I") IR.Rows[rNo]["itnm"] = itmdsp;
                                        if (showsizes == true)
                                        {
                                            //IR.Rows[rNo]["sizedsp"] = sizes;
                                            //IR.Rows[rNo]["pcsperbox"] = Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]);
                                        }
                                        if (cdr != 0) IR.Rows[rNo]["inqnty"] = cdr;
                                        if (ccr != 0) IR.Rows[rNo]["outqnty"] = ccr;
                                        cbal = cbal + cdr - ccr;
                                        if (i <= maxR)
                                        {
                                            if (tbl.Rows[i]["docdt"].ToString() != lastdt) // || i < maxR)
                                            {
                                                if (slcd == "") IR.Rows[rNo]["balqnty"] = cbal; // cop+cdr-ccr;
                                                lastdt = tbl.Rows[i]["docdt"].ToString();
                                            }
                                        }
                                        idr = idr + cdr;
                                        icr = icr + ccr;
                                        ibal = cbal;
                                        cdr = 0; ccr = 0;
                                        if (i > maxR) break;
                                    }
                                    if (itotadd == true) ibal = ibal + cbal;
                                    if (i > maxR) break;
                                }
                                else
                                {
                                    ibal = ibal + cbal;
                                }
                                if (i > maxR) break;
                            }
                            gdr = gdr + idr;
                            gcr = gcr + icr;
                            gbal = gbal + ibal;
                            if (reptype == "I")
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["slnm"] = "Total of [" + tbl.Rows[i - 1]["itnm"].ToString() + " ]";
                                IR.Rows[rNo]["inqnty"] = idr;
                                IR.Rows[rNo]["outqnty"] = icr;
                                if (slcd == "") IR.Rows[rNo]["balqnty"] = ibal;
                                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                            }
                            if (i > maxR) break;
                        }
                        //total of Item Group
                        tdr = tdr + gdr;
                        tcr = tcr + gcr;
                        tbal = tbal + gbal;
                        if (reptype == "G")
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["slnm"] = "Total of [" + tbl.Rows[i - 1]["itgrpnm"].ToString() + " ]";
                            IR.Rows[rNo]["inqnty"] = gdr;
                            IR.Rows[rNo]["outqnty"] = gcr;
                            if (slcd == "") IR.Rows[rNo]["balqnty"] = gbal;
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                        }
                        if (i > maxR) break;
                    }

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["slnm"] = "Grand Totals";
                    IR.Rows[rNo]["inqnty"] = tdr;
                    IR.Rows[rNo]["outqnty"] = tcr;
                    if (slcd == "") IR.Rows[rNo]["balqnty"] = tbal;
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                    #endregion
                }
                else
                {
                    #region Stock reprot as Op/Closing
                    i = 0; maxR = tbl.Rows.Count - 1;
                    HC.RepStart(IR, 3);
                    HC.GetPrintHeader(IR, "itcd", "string", "c,10", "Item Code");
                    HC.GetPrintHeader(IR, "itnm", "string", "c,20", "Article");
                    HC.GetPrintHeader(IR, "opqnty", "double", qtydsp1, "Op." + qtyhd);
                    //if (stkcalcon == "Z") HC.GetPrintHeader(IR, "opqnty1", "double", qtydsp1, "Op. Box");
                    //HC.GetPrintHeader(IR, "inqnty", "double", qtydsp, "In " + qtyhd);
                    //if (stkcalcon == "Z") HC.GetPrintHeader(IR, "inqnty1", "double", qtydsp1, "In. Box");
                    //HC.GetPrintHeader(IR, "outqnty", "double", qtydsp, "Out " + qtyhd);
                    //if (stkcalcon == "Z") HC.GetPrintHeader(IR, "outqnty1", "double", qtydsp1, "Out. Box");
                    //HC.GetPrintHeader(IR, "retqnty", "double", qtydsp, "Ret " + qtyhd);
                    //if (stkcalcon == "Z") HC.GetPrintHeader(IR, "retqnty1", "double", qtydsp1, "Ret. Box");
                    //HC.GetPrintHeader(IR, "balqnty", "double", qtydsp, "Bal " + qtyhd);
                    //if (stkcalcon == "Z") HC.GetPrintHeader(IR, "balqnty1", "double", qtydsp1, "Bal. Box");

                    while (i <= maxR)
                    {
                        stritgrpcd = tbl.Rows[i]["itgrpcd"].ToString();

                        if (reptype != "S")
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["Dammy"] = "[ " + stritgrpcd + "  " + " ]" + tbl.Rows[i]["itgrpnm"];
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                        }
                        gop = 0; gdr = 0; gcr = 0; gret = 0; gbal = 0;
                        gop1 = 0; gdr1 = 0; gcr1 = 0; gret1 = 0; gbal1 = 0;
                        while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd)
                        {
                            chk1 = "itgrpcd"; chk2 = tbl.Rows[i]["itgrpcd"].ToString();
                            if (reptype == "I" || reptype == "T")
                            {
                                chk1 = "itcd"; chk2 = tbl.Rows[i][chk1].ToString();
                            }

                            iop = 0; idr = 0; icr = 0; iret = 0; ibal = 0;
                            iop1 = 0; idr1 = 0; icr1 = 0; iret1 = 0; ibal1 = 0;
                            while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2)
                            {
                                cop = 0; cdr = 0; ccr = 0; cret = 0; cpcs = 0; cbox = 0; cqty = 0; cbal = 0;
                                cop1 = 0; cdr1 = 0; ccr1 = 0; cret1 = 0; cbal = 0;
                                while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(fdt))
                                {
                                    ichk = tbl.Rows[i]["itcd"].ToString();
                                    chkpcs = 0; cpcs = 0;
                                    while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(fdt) && tbl.Rows[i]["itcd"].ToString() == ichk)
                                    {
                                        autono = tbl.Rows[i]["autono"].ToString();
                                        string partcd = tbl.Rows[i]["partcd"].ToString();
                                        while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(fdt) && tbl.Rows[i]["itcd"].ToString() == ichk && tbl.Rows[i]["autono"].ToString() == autono)
                                        {
                                            int mult = 0;
                                            if (tbl.Rows[i]["partcd"].ToString() == partcd)
                                            {
                                                if (tbl.Rows[i]["stkdrcr"].ToString() == "D") mult = 1; else mult = -1;
                                                if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcs = chkpcs + (Convert.ToDouble(tbl.Rows[i]["qnty"]) * mult);
                                                cpcs = cpcs + (Convert.ToDouble(tbl.Rows[i]["qnty"]) * mult);
                                            }
                                            i++;
                                            if (i > maxR) break;
                                        }
                                        if (i > maxR) break;
                                    }
                                    //if (stkcalcon == "B") cqty = Salesfunc.ConvPcstoBox(chkpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
                                     //if (stkcalcon == "S") cqty = Salesfunc.ConvPcstoSet(chkpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));

                                    //else cqty = cpcs;
                                     cqty = cpcs;
                                    cop = cop + cqty;
                                    //cop1 = cop1 + Salesfunc.ConvPcstoBox(chkpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
                                    if (i > maxR) break;
                                }
                                cbal = cop; cbal1 = cop1;
                                iop = iop + cop;
                                iop1 = iop1 + cop1;
                                ibal = cbal; ibal1 = cbal1;
                                if (i <= maxR)
                                {
                                    lastdt = tbl.Rows[i]["docdt"].ToString();
                                    while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && Convert.ToDateTime(tbl.Rows[i]["docdt"]) <= Convert.ToDateTime(tdt))
                                    {
                                        autono = tbl.Rows[i]["autono"].ToString(); itmdsp = "";
                                        while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && tbl.Rows[i]["autono"].ToString() == autono)
                                        {
                                            ichk = tbl.Rows[i]["itcd"].ToString();
                                            chkpcsdr = 0; chkpcscr = 0; lpcsdr = 0; lpcscr = 0; lpcsret = 0;
                                            string partcd = tbl.Rows[i]["partcd"].ToString();
                                            while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && tbl.Rows[i]["autono"].ToString() == autono && tbl.Rows[i]["itcd"].ToString() == ichk)
                                            {
                                                int mult = 1;
                                                if (tbl.Rows[i]["partcd"].ToString() == partcd)
                                                {
                                                    if (tbl.Rows[i]["doctag"].ToString() == "SR" || tbl.Rows[i]["doctag"].ToString() == "JU")
                                                    {
                                                        if (tbl.Rows[i]["stkdrcr"].ToString() == "C") mult = -1;
                                                        if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcsret = chkpcsret + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                                                        lpcsret = lpcsret + (Convert.ToDouble(tbl.Rows[i]["qnty"]) * mult);
                                                    }
                                                    else if (tbl.Rows[i]["stkdrcr"].ToString() == "D")
                                                    {
                                                        if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcsdr = chkpcsdr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                                                        lpcsdr = lpcsdr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                                                    }
                                                    else
                                                    {
                                                        if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcscr = chkpcscr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                                                        lpcscr = lpcscr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                                                    }
                                                }
                                                i++;
                                                if (i > maxR) break;
                                            }
                                            //if (stkcalcon == "B")
                                            //{
                                            //    lpcsdr = Salesfunc.ConvPcstoBox(chkpcsdr, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
                                            //    lpcscr = Salesfunc.ConvPcstoBox(chkpcscr, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
                                            //    lpcsret = Salesfunc.ConvPcstoBox(chkpcsret, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
                                            //}
                                            //else if (stkcalcon == "S")
                                            //{
                                            //    lpcsdr = Salesfunc.ConvPcstoSet(chkpcsdr, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
                                            //    lpcscr = Salesfunc.ConvPcstoSet(chkpcscr, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
                                            //    lpcsret = Salesfunc.ConvPcstoSet(chkpcsret, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
                                            //}
                                            cqty = lpcsdr + lpcscr;

                                            cdr = cdr + lpcsdr; ccr = ccr + lpcscr; cret = lpcsret;
                                            //cdr1 = cdr1 + Salesfunc.ConvPcstoBox(chkpcsdr, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
                                            //ccr1 = ccr1 + Salesfunc.ConvPcstoBox(chkpcscr, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
                                            cret1 = cret1 + Salesfunc.ConvPcstoSet(chkpcsret, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
                                            if (i > maxR) break;
                                        }
                                        cbal = cbal + cdr - ccr + cret;
                                        cbal1 = cbal1 + cdr1 - ccr1 + cret1;
                                        if (tbl.Rows[i - 1]["docdt"].ToString() != lastdt || i == maxR)
                                        {
                                            lastdt = tbl.Rows[i - 1]["docdt"].ToString();
                                        }
                                        idr = idr + cdr;
                                        icr = icr + ccr;
                                        iret = iret + cret;
                                        ibal = cbal;

                                        idr1 = idr1 + cdr1;
                                        iret1 = iret1 + cret1;
                                        icr1 = icr1 + ccr1;
                                        ibal1 = cbal1;

                                        cdr = 0; ccr = 0; cret = 0;
                                        cdr1 = 0; ccr1 = 0; cret1 = 0;
                                        if (i > maxR) break;
                                    }
                                    if (i > maxR) break;
                                }
                                if (i > maxR) break;
                            }
                            gop = gop + iop; gdr = gdr + idr; gcr = gcr + icr; gret = gret + iret; gbal = gbal + ibal;
                            gop1 = gop1 + iop1; gdr1 = gdr1 + idr1; gcr1 = gcr1 + icr1; gret1 = gret1 + iret1; gbal1 = gbal1 + ibal1;
                            if (reptype == "I" || reptype == "T")
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["itcd"] = tbl.Rows[i - 1]["itcd"].ToString();
                                if (tbl.Rows[i - 1]["styleno"].ToString() == "") IR.Rows[rNo]["itnm"] = tbl.Rows[i - 1]["itnm"].ToString();
                                else IR.Rows[rNo]["itnm"] = tbl.Rows[i - 1]["styleno"].ToString();
                                IR.Rows[rNo]["opqnty"] = iop;
                                IR.Rows[rNo]["inqnty"] = idr;
                                IR.Rows[rNo]["outqnty"] = icr;
                                IR.Rows[rNo]["retqnty"] = iret;
                                IR.Rows[rNo]["balqnty"] = ibal;
                                //if (stkcalcon == "Z")
                                //{
                                //    IR.Rows[rNo]["opqnty1"] = iop1;
                                //    IR.Rows[rNo]["inqnty1"] = idr1;
                                //    IR.Rows[rNo]["outqnty1"] = icr1;
                                //    IR.Rows[rNo]["retqnty1"] = iret1;
                                //    IR.Rows[rNo]["balqnty1"] = ibal1;
                                //}
                            }
                            if (i > maxR) break;
                        }
                        //total of Item Group
                        top = top + gop; tdr = tdr + gdr; tcr = tcr + gcr; tret = tret + gret; tbal = tbal + gbal;
                        top1 = top1 + gop1; tdr1 = tdr1 + gdr1; tcr1 = tcr1 + gcr1; tret1 = tret1 + gret1; tbal1 = tbal1 + gbal1;
                        if (reptype == "G" || reptype == "T")
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["itnm"] = "Total of [" + tbl.Rows[i - 1]["itgrpnm"].ToString() + " ]";
                            IR.Rows[rNo]["opqnty"] = gop;
                            IR.Rows[rNo]["inqnty"] = gdr;
                            IR.Rows[rNo]["outqnty"] = gcr;
                            IR.Rows[rNo]["retqnty"] = gret;
                            IR.Rows[rNo]["balqnty"] = gbal;
                            //if (stkcalcon == "Z")
                            //{
                            //    IR.Rows[rNo]["opqnty1"] = gop1;
                            //    IR.Rows[rNo]["inqnty1"] = gdr1;
                            //    IR.Rows[rNo]["outqnty1"] = gcr1;
                            //    IR.Rows[rNo]["retqnty1"] = gret1;
                            //    IR.Rows[rNo]["balqnty1"] = gbal1;
                            //}
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                        }
                        if (i > maxR) break;
                    }

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["itnm"] = "Grand Totals";
                    IR.Rows[rNo]["opqnty"] = top;
                    IR.Rows[rNo]["inqnty"] = tdr;
                    IR.Rows[rNo]["outqnty"] = tcr;
                    IR.Rows[rNo]["retqnty"] = tret;
                    IR.Rows[rNo]["balqnty"] = tbal;
                    //if (stkcalcon == "Z")
                    //{
                    //    IR.Rows[rNo]["opqnty1"] = top1;
                    //    IR.Rows[rNo]["inqnty1"] = tdr1;
                    //    IR.Rows[rNo]["outqnty1"] = tcr1;
                    //    IR.Rows[rNo]["retqnty1"] = tret1;
                    //    IR.Rows[rNo]["balqnty1"] = tbal1;
                    //}
                    if (slcd == "") IR.Rows[rNo]["balqnty"] = tbal;
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                    #endregion
                }

                string pghdr1 = "", repname = CommFunc.retRepname("Rep_Stk_Leg");
                pghdr1 = "Stock Ledger " + (VE.Checkbox3 == true ? " (Combined) " : "");
                if (mtrljobcd != "FS") pghdr1 += " [" + VE.TEXTBOX6 + "] ";
                pghdr1 += "from " + fdt + " to " + tdt;
                //if (pghdr2.retStr() != "")
                //{
                //    pghdr2 = "<html>" + pghdr2 + "</html>";
                //}
                PV = HC.ShowReport(IR, repname, pghdr1, pghdr2, true, true, "P", false);
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        //public ActionResult Rep_Stk_Leg(ReportViewinHtml VE, FormCollection FC)
        //{
        //    string ModuleCode = Module.Module_Code;
        //    try
        //    {
        //        string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
        //        string brandcd = "", itgrpcd = "", itcd = "", sizecd = "", gocd = "", stktype = "", mtrljobcd = "", fdt, tdt = "", slcd = "";
        //        string seldoccd = "", seldoctype = "";
        //        string unselitcd = "", unselslcd = "";
        //        string stkcalcon = VE.TEXTBOX2; // P=Pcs, B=Box
        //        string reptype = VE.TEXTBOX1; //Summary, I=Item, G=Group;

        //        fdt = VE.FDT;
        //        tdt = VE.TDT;
        //        if (VE.TEXTBOX4 != null) gocd = "'" + VE.TEXTBOX4 + "'";

        //        if (FC.AllKeys.Contains("brandcdvalue")) brandcd = FC["brandcdvalue"].ToString().retSqlformat();
        //        if (FC.AllKeys.Contains("itgrpcdvalue")) itgrpcd = FC["itgrpcdvalue"].ToString().retSqlformat();
        //        if (FC.AllKeys.Contains("itcdvalue")) itcd = FC["itcdvalue"].ToString().retSqlformat();
        //        if (FC.AllKeys.Contains("itcdunselvalue")) unselitcd = FC["itcdunselvalue"].ToString().retSqlformat();
        //        if (FC.AllKeys.Contains("slcdvalue")) slcd = FC["slcdvalue"].ToString().retSqlformat();
        //        if (FC.AllKeys.Contains("slcdunselvalue")) unselslcd = FC["slcdunselvalue"].ToString().retSqlformat();
        //        if (FC.AllKeys.Contains("doccdvalue")) seldoccd = CommFunc.retSqlformat(FC["doccdvalue"].ToString());

        //        if (FC.AllKeys.Contains("ITSIZES")) sizecd = FC["ITSIZES"].ToString().retSqlformat();
        //        if (FC.AllKeys.Contains("STKTYPE")) stktype = FC["STKTYPE"].ToString().retSqlformat();

        //        mtrljobcd = "'FS'";
        //        if (VE.TEXTBOX3 != null) mtrljobcd = "'" + VE.TEXTBOX3 + "'";
        //        string ffdt = Convert.ToDateTime(fdt.retDateStr()).AddDays(-1).ToString().retDateStr();

        //        string sql = "";

        //        sql = "select doccd from " + scm + ".m_doctype where doctype in ('STCNV') ";
        //        DataTable rstmp = MasterHelp.SQLquery(sql);
        //        string skipdoccd = "";
        //        if (VE.Checkbox2 == false)
        //        {
        //            for (int s = 0; s <= rstmp.Rows.Count - 1; s++)
        //            {
        //                if (skipdoccd != "") skipdoccd += ",";
        //                skipdoccd += "'" + rstmp.Rows[s]["doccd"].ToString() + "'";
        //            }
        //        }
        //        sql = "";
        //        sql += "select a.autono||a.cancel autono, a.cancel, b.slcd, s.slnm, c.docno, b.prefno, a.docdt, a.doctag, a.itcd, a.partcd, a.mtrljobcd, a.stktype, a.itcolsize, ";
        //        sql += "d.itnm, d.styleno, nvl(d.pcsperbox,0) pcsperbox, nvl(d.pcsperset,0) pcsperset, ";
        //        if (reptype == "S" || mtrljobcd != "FS") sql += "'' itgrpcd, '' itgrpnm, "; else sql += "d.itgrpcd, e.itgrpnm, ";
        //        sql += "e.brandcd, f.brandnm, a.stkdrcr, a.qnty from ( ";
        //        sql += "select a.autono, a.cancel, a.docdt, a.doccd, a.doctag, a.itcd, a.partcd, a.mtrljobcd, a.stktype, a.itcolsize, a.stkdrcr, sum(a.qnty) qnty from ( ";

        //        sql += "select (case when c.docdt < to_date('" + fdt + "','dd/mm/yyyy') then 'opng' else a.autono end) autono, nvl(c.cancel,'N') cancel, ";
        //        sql += "(case when c.docdt < to_date('" + fdt + "','dd/mm/yyyy') then to_date('" + ffdt + "','dd/mm/yyyy')  else c.docdt end) docdt, ";
        //        sql += "c.doccd, b.doctag, nvl(d.linkitcd,a.itcd) itcd, a.partcd, a.mtrljobcd, nvl(a.stktype,'F') stktype, ";
        //        sql += "nvl(a.stktype,'F')||a.itcd itcolsize, a.stkdrcr, ";
        //        //sql += "(case when a.mtrljobcd in ('YP','YD','GT','FT','TF','WA','PF','WS') then nvl(a.qnty,0) else nvl(nvl(e.qnty,a.qnty),0) end)*decode(nvl(e.skipstk,'N'),'Y',0,1) qnty ";
        //        sql += "nvl(a.qnty,0)*decode(nvl(e.skipstk,'N'),'Y',0,1) qnty ";
        //        sql += "from " + scm + ".t_txndtl a, " + scm + ".t_txn b, " + scm + ".t_cntrl_hdr c, " + scm + ".m_sitem d, " + scm + ".t_batchdtl e, " + scm + ".t_batchmst f ";
        //        sql += "where a.autono=b.autono and a.autono=c.autono and a.stkdrcr in ('D','C') and ";
        //        sql += "a.autono=e.autono(+) and a.slno=e.slno(+) and e.batchautono=f.autono(+) and e.batchslno=f.slno(+) and ";
        //        //sql += "(case when a.mtrljobcd in ('YP','YD','GT','FT','TF') then a.mtrljobcd else nvl(f.jobcd,a.mtrljobcd) end) in (" + mtrljobcd + ") and ";
        //        sql += "a.mtrljobcd in (" + mtrljobcd + ") and ";
        //        if (gocd != "") sql += "b.gocd in (" + gocd + ") and ";
        //        if (stktype != "") sql += "nvl(a.stktype,'F') in (" + stktype + ") and ";
        //        if (sizecd != "") sql += "a.sizecd in (" + sizecd + ") and ";
        //        sql += "c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and a.itcd=d.itcd and ";
        //        if (VE.Checkbox3 == false) sql += "c.loccd='" + LOC + "' and ";
        //        sql += "c.compcd='" + COM + "' ";

        //        sql += "union all ";

        //        sql += "select (case when to_date(to_char(c.canc_usr_entdt,'dd/mm/yyyy'),'dd/mm/yyyy') < to_date('" + fdt + "','dd/mm/yyyy') then 'opng' else a.autono end) autono, 'R' cancel, ";
        //        sql += "(case when to_date(to_char(c.canc_usr_entdt,'dd/mm/yyyy'),'dd/mm/yyyy') < to_date('" + fdt + "','dd/mm/yyyy') then to_date('" + ffdt + "','dd/mm/yyyy')  else to_date(to_char(c.canc_usr_entdt,'dd/mm/yyyy'),'dd/mm/yyyy') end) docdt, ";
        //        sql += "c.doccd, b.doctag, nvl(d.linkitcd,a.itcd) itcd, a.partcd, a.mtrljobcd, nvl(a.stktype,'F') stktype, ";
        //        sql += "nvl(a.stktype,'F')||a.itcd itcolsize, ";
        //        //if (reptype != "T") sql += "decode(a.stkdrcr,'D','C','D') stkdrcr, (case when a.mtrljobcd in ('YP','YD','GT','FT','TF','WA','PF') then nvl(a.qnty,0) else nvl(nvl(e.qnty,a.qnty),0) end) qnty ";
        //        //else sql += "a.stkdrcr, (case when a.mtrljobcd in ('YP','YD','GT','FT','TF','WA','PF','WS') then nvl(a.qnty,0) else nvl(nvl(e.qnty,a.qnty),0) end)*-1*decode(nvl(e.skipstk,'N'),'Y',0,1) qnty ";
        //        if (reptype != "T") sql += "decode(a.stkdrcr,'D','C','D') stkdrcr, (case when a.mtrljobcd in ('YP','YD','GT','FT','TF','PF') then nvl(a.qnty,0) else nvl(nvl(e.qnty,a.qnty),0) end) qnty ";
        //        else sql += "a.stkdrcr, (case when a.mtrljobcd in ('YP','YD','GT','FT','TF') then nvl(a.qnty,0) else nvl(nvl(e.qnty,a.qnty),0) end)*-1*decode(nvl(e.skipstk,'N'),'Y',0,1) qnty ";
        //        sql += "from " + scm + ".t_txndtl a, " + scm + ".t_txn b, " + scm + ".t_cntrl_hdr c, " + scm + ".m_sitem d, " + scm + ".t_batchdtl e, " + scm + ".t_batchmst f ";
        //        sql += "where a.autono=b.autono and a.autono=c.autono and a.stkdrcr in ('D','C') and ";
        //        sql += "a.autono=e.autono(+) and a.slno=e.slno(+) and e.batchautono=f.autono(+) and e.batchslno=f.slno(+) and ";
        //        //sql += "(case when a.mtrljobcd in ('YP','YD','GT','FT','TF','WA','PF','WS') then a.mtrljobcd else nvl(f.jobcd,a.mtrljobcd) end) in (" + mtrljobcd + ") and ";
        //        sql += "a.mtrljobcd in (" + mtrljobcd + ") and ";
        //        if (gocd != "") sql += "b.gocd in (" + gocd + ") and ";
        //        if (stktype != "") sql += "nvl(a.stktype,'F') in (" + stktype + ") and ";
        //        if (sizecd != "") sql += "a.sizecd in (" + sizecd + ") and ";
        //        sql += "to_date(to_char(c.canc_usr_entdt,'dd/mm/yyyy'),'dd/mm/yyyy') <= to_date('" + tdt + "','dd/mm/yyyy') and a.itcd=d.itcd and ";
        //        if (VE.Checkbox3 == false) sql += "c.loccd='" + LOC + "' and ";
        //        sql += "c.compcd='" + COM + "' and nvl(c.cancel,'N')='Y' ";

        //        sql += ") a group by a.autono, a.cancel, a.docdt, a.doccd, a.doctag, a.itcd, a.partcd, a.mtrljobcd, a.stktype, a.itcolsize, a.stkdrcr  ) a, ";

        //        sql += scm + ".t_txn b, " + scm + ".t_cntrl_hdr c, " + scmf + ".m_subleg s, ";
        //        sql += scm + ".m_sitem d, " + scm + ".m_group e, " + scm + ".m_brand f, " + scm + ".m_doctype g ";
        //        sql += "where a.itcd=d.itcd(+) and d.itgrpcd=e.itgrpcd(+) and e.brandcd=f.brandcd(+) and c.doccd=g.doccd(+) and ";
        //        if (itcd != "") sql += "a.itcd in (" + itcd + ") and ";
        //        if (unselitcd != "") sql += "a.itcd not in (" + unselitcd + ") and ";
        //        if (brandcd != "") sql += "e.brandcd in (" + brandcd + ") and ";
        //        if (itgrpcd != "") sql += "d.itgrpcd in (" + itgrpcd + ") and ";
        //        if (skipdoccd != "") sql += "a.doccd not in (" + skipdoccd + ") and ";
        //        if (slcd != "") sql += "b.slcd in (" + slcd + ") and ";
        //        if (unselslcd != "") sql += "b.slcd not in (" + unselslcd + ") and ";
        //        if (seldoccd != "") sql += "c.doccd in (" + seldoccd + ") and ";
        //        if (seldoctype != "") sql += "g.doctype in (" + seldoctype + ") and ";
        //        sql += "a.autono=b.autono(+) and a.autono=c.autono(+) and b.slcd=s.slcd(+) ";
        //        sql += "order by ";
        //        if (reptype == "S") sql += "docdt, docno, autono, styleno, itcd, autono ";
        //        else if (reptype == "G") sql += "itgrpnm, itgrpcd, docdt, docno, autono ";
        //        else sql += "itgrpnm, itgrpcd, styleno, itcd, docdt, docno, autono ";

        //        DataTable tbl = MasterHelp.SQLquery(sql);
        //        if (tbl.Rows.Count == 0)
        //        {
        //            return Content("No Record Found");
        //        }

        //        Models.PrintViewer PV = new Models.PrintViewer();
        //        HtmlConverter HC = new HtmlConverter();

        //        mtrljobcd = VE.TEXTBOX3;
        //        string qtyhd = "", qtydsp = "", qtydsp1 = "n,17,2:##,##,##,##0.00";
        //        if (mtrljobcd == "YP" || mtrljobcd == "FT" || mtrljobcd == "GT" || mtrljobcd == "YD" || mtrljobcd == "WA" || mtrljobcd == "PF" || mtrljobcd == "TF") stkcalcon = "Q";

        //        if (stkcalcon == "P" || stkcalcon == "Z") { qtyhd = "Pcs"; qtydsp = "n,17,2:####,##,##,##0"; }
        //        else if (stkcalcon == "B") { qtyhd = "Box"; qtydsp = "n,17,2:##,##,##,##0.00"; }
        //        else if (stkcalcon == "S") { qtyhd = "Set"; qtydsp = "n,17,2:##,##,##,##0.00"; }
        //        else { qtyhd = "Un"; qtydsp = "n,17,2:####,##,##0.000"; }
        //        if (stkcalcon == "Q") qtydsp = "n,17,2:####,##,##0.000";

        //        DataTable IR = new DataTable("mstrep");

        //        string stritgrpcd = "", chk1 = "", chk2 = "";
        //        Int32 maxR = 0, i = 0, rNo = 0;

        //        double cop = 0, cop1 = 0, cdr = 0, ccr = 0, cdr1 = 0, ccr1 = 0, cpcs = 0, cbox = 0, cqty = 0, cret = 0, cret1 = 0, cbal = 0, cbal1 = 0;
        //        double chkpcsdr = 0, chkpcscr = 0, chkpcsret = 0, lpcsdr = 0, lpcscr = 0, lpcsret = 0;
        //        string autono = "", itmdsp = "", ichk = "", lastdt = "";
        //        double chkpcs = 0; cpcs = 0;
        //        double top = 0, tdr = 0, tcr = 0, tret = 0, tbal = 0;
        //        double iop = 0, idr = 0, icr = 0, iret = 0, ibal = 0;
        //        double gop = 0, gdr = 0, gcr = 0, gret = 0, gbal = 0;

        //        double top1 = 0, tdr1 = 0, tcr1 = 0, tret1 = 0, tbal1 = 0;
        //        double iop1 = 0, idr1 = 0, icr1 = 0, iret1 = 0, ibal1 = 0;
        //        double gop1 = 0, gdr1 = 0, gcr1 = 0, gret1 = 0, gbal1 = 0;

        //        if (reptype != "T")
        //        {
        //            #region Stock Ledger
        //            i = 0; maxR = tbl.Rows.Count - 1;
        //            HC.RepStart(IR, 3);
        //            HC.GetPrintHeader(IR, "docdt", "string", "c,10", "DocDate");
        //            HC.GetPrintHeader(IR, "docno", "string", "c,15", "DocNo.");
        //            HC.GetPrintHeader(IR, "prefno", "string", "c,15", "Party Ref#");
        //            //HC.GetPrintHeader(IR, "slcd", "string", "c,8", "Party Cd");
        //            HC.GetPrintHeader(IR, "slnm", "string", "c,45", "Party Name");
        //            if (reptype != "I") HC.GetPrintHeader(IR, "itnm", "string", "c,35", "Item");
        //            HC.GetPrintHeader(IR, "inqnty", "double", qtydsp, "In " + qtyhd);
        //            HC.GetPrintHeader(IR, "outqnty", "double", qtydsp, "Out " + qtyhd);
        //            if (slcd == "") HC.GetPrintHeader(IR, "balqnty", "double", qtydsp, "Bal " + qtyhd);

        //            while (i <= maxR)
        //            {
        //                stritgrpcd = tbl.Rows[i]["itgrpcd"].ToString();

        //                if (reptype != "S")
        //                {
        //                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                    IR.Rows[rNo]["Dammy"] = "[ " + stritgrpcd + "  " + "] " + tbl.Rows[i]["itgrpnm"];
        //                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
        //                }
        //                gdr = 0; gcr = 0; gbal = 0;
        //                while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd)
        //                {
        //                    chk1 = "itgrpcd"; chk2 = tbl.Rows[i]["itgrpcd"].ToString();
        //                    if (reptype == "I")
        //                    {
        //                        chk1 = "itcd"; chk2 = tbl.Rows[i][chk1].ToString();
        //                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                        IR.Rows[rNo]["Dammy"] = "[ " + chk2 + "  " + " ] " + tbl.Rows[i]["styleno"].ToString() + " , " + tbl.Rows[i]["itnm"];
        //                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
        //                    }

        //                    idr = 0; icr = 0; ibal = 0;
        //                    while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2)
        //                    {
        //                        cop = 0; cdr = 0; ccr = 0; cpcs = 0; cbox = 0; cqty = 0; cbal = 0;
        //                        while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(fdt))
        //                        {
        //                            ichk = tbl.Rows[i]["itcd"].ToString();
        //                            chkpcs = 0; cpcs = 0;
        //                            while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(fdt) && tbl.Rows[i]["itcd"].ToString() == ichk)
        //                            {
        //                                autono = tbl.Rows[i]["autono"].ToString();
        //                                string partcd = tbl.Rows[i]["partcd"].ToString();
        //                                while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(fdt) && tbl.Rows[i]["itcd"].ToString() == ichk && tbl.Rows[i]["autono"].ToString() == autono)
        //                                {
        //                                    if (tbl.Rows[i]["partcd"].ToString() == partcd)
        //                                    {
        //                                        int mult = 0;
        //                                        if (tbl.Rows[i]["stkdrcr"].ToString() == "D") mult = 1; else mult = -1;
        //                                        if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcs = chkpcs + (Convert.ToDouble(tbl.Rows[i]["qnty"]) * mult);
        //                                        cpcs = cpcs + (Convert.ToDouble(tbl.Rows[i]["qnty"]) * mult);
        //                                    }
        //                                    i++;
        //                                    if (i > maxR) break;
        //                                }
        //                                if (i > maxR) break;
        //                            }
        //                            if (stkcalcon == "B") cqty = Salesfunc.ConvPcstoBox(chkpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
        //                            else if (stkcalcon == "S") cqty = Salesfunc.ConvPcstoSet(chkpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
        //                            else cqty = cpcs;
        //                            cop = cop + cqty;
        //                            if (i > maxR) break;
        //                        }
        //                        if (cop != 0)
        //                        {
        //                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                            IR.Rows[rNo]["docdt"] = fdt;
        //                            IR.Rows[rNo]["slnm"] = "Opening Stock";
        //                            if (slcd == "") IR.Rows[rNo]["balqnty"] = cop;
        //                        }
        //                        cbal = cop;
        //                        if (i <= maxR)
        //                        {
        //                            lastdt = tbl.Rows[i]["docdt"].ToString();
        //                            while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && Convert.ToDateTime(tbl.Rows[i]["docdt"]) <= Convert.ToDateTime(tdt))
        //                            {
        //                                autono = tbl.Rows[i]["autono"].ToString(); itmdsp = "";
        //                                while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && tbl.Rows[i]["autono"].ToString() == autono)
        //                                {
        //                                    ichk = tbl.Rows[i]["itcd"].ToString();
        //                                    chkpcsdr = 0; chkpcscr = 0; lpcsdr = 0; lpcscr = 0;
        //                                    while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && tbl.Rows[i]["autono"].ToString() == autono && tbl.Rows[i]["itcd"].ToString() == ichk)
        //                                    {
        //                                        autono = tbl.Rows[i]["autono"].ToString();
        //                                        string partcd = tbl.Rows[i]["partcd"].ToString();
        //                                        while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && tbl.Rows[i]["autono"].ToString() == autono && tbl.Rows[i]["itcd"].ToString() == ichk && tbl.Rows[i]["autono"].ToString() == autono)
        //                                        {
        //                                            if (tbl.Rows[i]["partcd"].ToString() == partcd)
        //                                            {
        //                                                int mult = 0;
        //                                                if (tbl.Rows[i]["stkdrcr"].ToString() == "D")
        //                                                {
        //                                                    if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcsdr = chkpcsdr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
        //                                                    lpcsdr = lpcsdr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
        //                                                }
        //                                                else
        //                                                {
        //                                                    if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcscr = chkpcscr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
        //                                                    lpcscr = lpcscr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
        //                                                }
        //                                            }
        //                                            i++;
        //                                            if (i > maxR) break;
        //                                        }
        //                                        if (i > maxR) break;
        //                                    }
        //                                    if (stkcalcon == "B")
        //                                    {
        //                                        lpcsdr = Salesfunc.ConvPcstoBox(chkpcsdr, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
        //                                        lpcscr = Salesfunc.ConvPcstoBox(chkpcscr, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
        //                                    }
        //                                    else if (stkcalcon == "S")
        //                                    {
        //                                        lpcsdr = Salesfunc.ConvPcstoSet(chkpcsdr, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
        //                                        lpcscr = Salesfunc.ConvPcstoSet(chkpcscr, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
        //                                    }
        //                                    cqty = lpcsdr + lpcscr;
        //                                    cdr = cdr + lpcsdr; ccr = ccr + lpcscr;
        //                                    itmdsp += tbl.Rows[i - 1]["styleno"].ToString() + " " + tbl.Rows[i - 1]["partcd"].ToString() + "=" + cqty.ToString() + ",";

        //                                    if (i > maxR) break;
        //                                }
        //                                string cancdsc = "";

        //                                switch (tbl.Rows[i - 1]["cancel"].ToString())
        //                                {
        //                                    case "Y":
        //                                        cancdsc = " (Cancel)"; break;
        //                                    case "R":
        //                                        cancdsc = " (Reverse)"; break;
        //                                }
        //                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                                IR.Rows[rNo]["docdt"] = tbl.Rows[i - 1]["docdt"].ToString().retDateStr();
        //                                IR.Rows[rNo]["docno"] = tbl.Rows[i - 1]["docno"] + cancdsc;
        //                                IR.Rows[rNo]["prefno"] = tbl.Rows[i - 1]["prefno"];
        //                                IR.Rows[rNo]["slnm"] = tbl.Rows[i - 1]["slnm"];
        //                                if (reptype != "I") IR.Rows[rNo]["itnm"] = itmdsp;
        //                                if (cdr != 0) IR.Rows[rNo]["inqnty"] = cdr;
        //                                if (ccr != 0) IR.Rows[rNo]["outqnty"] = ccr;
        //                                cbal = cbal + cdr - ccr;
        //                                if (i <= maxR)
        //                                {
        //                                    if (tbl.Rows[i]["docdt"].ToString() != lastdt) // || i < maxR)
        //                                    {
        //                                        if (slcd == "") IR.Rows[rNo]["balqnty"] = cbal; // cop+cdr-ccr;
        //                                        lastdt = tbl.Rows[i]["docdt"].ToString();
        //                                    }
        //                                }
        //                                idr = idr + cdr;
        //                                icr = icr + ccr;
        //                                ibal = cbal;
        //                                cdr = 0; ccr = 0;
        //                                if (i > maxR) break;
        //                            }
        //                            if (i > maxR) break;
        //                        }
        //                        else
        //                        {
        //                            ibal = ibal + cbal;
        //                        }
        //                        if (i > maxR) break;
        //                    }
        //                    gdr = gdr + idr;
        //                    gcr = gcr + icr;
        //                    gbal = gbal + ibal;
        //                    if (reptype == "I")
        //                    {
        //                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                        IR.Rows[rNo]["slnm"] = "Total of [" + tbl.Rows[i - 1]["itnm"].ToString() + " ]";
        //                        IR.Rows[rNo]["inqnty"] = idr;
        //                        IR.Rows[rNo]["outqnty"] = icr;
        //                        if (slcd == "") IR.Rows[rNo]["balqnty"] = ibal;
        //                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
        //                    }
        //                    if (i > maxR) break;
        //                }
        //                //total of Item Group
        //                tdr = tdr + gdr;
        //                tcr = tcr + gcr;
        //                tbal = tbal + gbal;
        //                if (reptype == "G")
        //                {
        //                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                    IR.Rows[rNo]["slnm"] = "Total of [" + tbl.Rows[i - 1]["itgrpnm"].ToString() + " ]";
        //                    IR.Rows[rNo]["inqnty"] = gdr;
        //                    IR.Rows[rNo]["outqnty"] = gcr;
        //                    if (slcd == "") IR.Rows[rNo]["balqnty"] = gbal;
        //                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
        //                }
        //                if (i > maxR) break;
        //            }

        //            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //            IR.Rows[rNo]["slnm"] = "Grand Totals";
        //            IR.Rows[rNo]["inqnty"] = tdr;
        //            IR.Rows[rNo]["outqnty"] = tcr;
        //            if (slcd == "") IR.Rows[rNo]["balqnty"] = tbal;
        //            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
        //            #endregion
        //        }
        //        else
        //        {
        //            #region Stock reprot as Op/Closing
        //            i = 0; maxR = tbl.Rows.Count - 1;
        //            HC.RepStart(IR, 3);
        //            HC.GetPrintHeader(IR, "itcd", "string", "c,10", "Item Code");
        //            HC.GetPrintHeader(IR, "itnm", "string", "c,20", "Article");
        //            HC.GetPrintHeader(IR, "opqnty", "double", qtydsp, "Op." + qtyhd);
        //            if (stkcalcon == "Z") HC.GetPrintHeader(IR, "opqnty1", "double", qtydsp1, "Op. Box");
        //            HC.GetPrintHeader(IR, "inqnty", "double", qtydsp, "In " + qtyhd);
        //            if (stkcalcon == "Z") HC.GetPrintHeader(IR, "inqnty1", "double", qtydsp1, "In. Box");
        //            HC.GetPrintHeader(IR, "outqnty", "double", qtydsp, "Out " + qtyhd);
        //            if (stkcalcon == "Z") HC.GetPrintHeader(IR, "outqnty1", "double", qtydsp1, "Out. Box");
        //            HC.GetPrintHeader(IR, "retqnty", "double", qtydsp, "Ret " + qtyhd);
        //            if (stkcalcon == "Z") HC.GetPrintHeader(IR, "retqnty1", "double", qtydsp1, "Ret. Box");
        //            HC.GetPrintHeader(IR, "balqnty", "double", qtydsp, "Bal " + qtyhd);
        //            if (stkcalcon == "Z") HC.GetPrintHeader(IR, "balqnty1", "double", qtydsp1, "Bal. Box");

        //            while (i <= maxR)
        //            {
        //                stritgrpcd = tbl.Rows[i]["itgrpcd"].ToString();

        //                if (reptype != "S")
        //                {
        //                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                    IR.Rows[rNo]["Dammy"] = "[ " + stritgrpcd + "  " + " ]" + tbl.Rows[i]["itgrpnm"];
        //                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
        //                }
        //                gop = 0; gdr = 0; gcr = 0; gret = 0; gbal = 0;
        //                gop1 = 0; gdr1 = 0; gcr1 = 0; gret1 = 0; gbal1 = 0;
        //                while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd)
        //                {
        //                    chk1 = "itgrpcd"; chk2 = tbl.Rows[i]["itgrpcd"].ToString();
        //                    if (reptype == "I" || reptype == "T")
        //                    {
        //                        chk1 = "itcd"; chk2 = tbl.Rows[i][chk1].ToString();
        //                    }

        //                    iop = 0; idr = 0; icr = 0; iret = 0; ibal = 0;
        //                    iop1 = 0; idr1 = 0; icr1 = 0; iret1 = 0; ibal1 = 0;
        //                    while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2)
        //                    {
        //                        cop = 0; cdr = 0; ccr = 0; cret = 0; cpcs = 0; cbox = 0; cqty = 0; cbal = 0;
        //                        cop1 = 0; cdr1 = 0; ccr1 = 0; cret1 = 0; cbal = 0;
        //                        while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(fdt))
        //                        {
        //                            ichk = tbl.Rows[i]["itcd"].ToString();
        //                            chkpcs = 0; cpcs = 0;
        //                            while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(fdt) && tbl.Rows[i]["itcd"].ToString() == ichk)
        //                            {
        //                                autono = tbl.Rows[i]["autono"].ToString();
        //                                string partcd = tbl.Rows[i]["partcd"].ToString();
        //                                while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(fdt) && tbl.Rows[i]["itcd"].ToString() == ichk && tbl.Rows[i]["autono"].ToString() == autono)
        //                                {
        //                                    int mult = 0;
        //                                    if (tbl.Rows[i]["partcd"].ToString() == partcd)
        //                                    {
        //                                        if (tbl.Rows[i]["stkdrcr"].ToString() == "D") mult = 1; else mult = -1;
        //                                        if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcs = chkpcs + (Convert.ToDouble(tbl.Rows[i]["qnty"]) * mult);
        //                                        cpcs = cpcs + (Convert.ToDouble(tbl.Rows[i]["qnty"]) * mult);
        //                                    }
        //                                    i++;
        //                                    if (i > maxR) break;
        //                                }
        //                                if (i > maxR) break;
        //                            }
        //                            if (stkcalcon == "B") cqty = Salesfunc.ConvPcstoBox(chkpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
        //                            else if (stkcalcon == "S") cqty = Salesfunc.ConvPcstoSet(chkpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
        //                            else cqty = cpcs;
        //                            cop = cop + cqty;
        //                            cop1 = cop1 + Salesfunc.ConvPcstoBox(chkpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
        //                            if (i > maxR) break;
        //                        }
        //                        cbal = cop; cbal1 = cop1;
        //                        iop = iop + cop;
        //                        iop1 = iop1 + cop1;
        //                        ibal = cbal; ibal1 = cbal1;
        //                        if (i <= maxR)
        //                        {
        //                            lastdt = tbl.Rows[i]["docdt"].ToString();
        //                            while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && Convert.ToDateTime(tbl.Rows[i]["docdt"]) <= Convert.ToDateTime(tdt))
        //                            {
        //                                autono = tbl.Rows[i]["autono"].ToString(); itmdsp = "";
        //                                while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && tbl.Rows[i]["autono"].ToString() == autono)
        //                                {
        //                                    ichk = tbl.Rows[i]["itcd"].ToString();
        //                                    chkpcsdr = 0; chkpcscr = 0; lpcsdr = 0; lpcscr = 0; lpcsret = 0;
        //                                    string partcd = tbl.Rows[i]["partcd"].ToString();
        //                                    while (tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd && tbl.Rows[i][chk1].ToString() == chk2 && tbl.Rows[i]["autono"].ToString() == autono && tbl.Rows[i]["itcd"].ToString() == ichk)
        //                                    {
        //                                        int mult = 1;
        //                                        if (tbl.Rows[i]["partcd"].ToString() == partcd)
        //                                        {
        //                                            if (tbl.Rows[i]["doctag"].ToString() == "SR" || tbl.Rows[i]["doctag"].ToString() == "JU")
        //                                            {
        //                                                if (tbl.Rows[i]["stkdrcr"].ToString() == "C") mult = -1;
        //                                                if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcsret = chkpcsret + Convert.ToDouble(tbl.Rows[i]["qnty"]);
        //                                                lpcsret = lpcsret + (Convert.ToDouble(tbl.Rows[i]["qnty"]) * mult);
        //                                            }
        //                                            else if (tbl.Rows[i]["stkdrcr"].ToString() == "D")
        //                                            {
        //                                                if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcsdr = chkpcsdr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
        //                                                lpcsdr = lpcsdr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
        //                                            }
        //                                            else
        //                                            {
        //                                                if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcscr = chkpcscr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
        //                                                lpcscr = lpcscr + Convert.ToDouble(tbl.Rows[i]["qnty"]);
        //                                            }
        //                                        }
        //                                        i++;
        //                                        if (i > maxR) break;
        //                                    }
        //                                    if (stkcalcon == "B")
        //                                    {
        //                                        lpcsdr = Salesfunc.ConvPcstoBox(chkpcsdr, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
        //                                        lpcscr = Salesfunc.ConvPcstoBox(chkpcscr, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
        //                                        lpcsret = Salesfunc.ConvPcstoBox(chkpcsret, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
        //                                    }
        //                                    else if (stkcalcon == "S")
        //                                    {
        //                                        lpcsdr = Salesfunc.ConvPcstoSet(chkpcsdr, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
        //                                        lpcscr = Salesfunc.ConvPcstoSet(chkpcscr, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
        //                                        lpcsret = Salesfunc.ConvPcstoSet(chkpcsret, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
        //                                    }
        //                                    cqty = lpcsdr + lpcscr;

        //                                    cdr = cdr + lpcsdr; ccr = ccr + lpcscr; cret = lpcsret;
        //                                    cdr1 = cdr1 + Salesfunc.ConvPcstoBox(chkpcsdr, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
        //                                    ccr1 = ccr1 + Salesfunc.ConvPcstoBox(chkpcscr, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
        //                                    cret1 = cret1 + Salesfunc.ConvPcstoSet(chkpcsret, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
        //                                    if (i > maxR) break;
        //                                }
        //                                cbal = cbal + cdr - ccr + cret;
        //                                cbal1 = cbal1 + cdr1 - ccr1 + cret1;
        //                                if (tbl.Rows[i - 1]["docdt"].ToString() != lastdt || i == maxR)
        //                                {
        //                                    lastdt = tbl.Rows[i - 1]["docdt"].ToString();
        //                                }
        //                                idr = idr + cdr;
        //                                icr = icr + ccr;
        //                                iret = iret + cret;
        //                                ibal = cbal;

        //                                idr1 = idr1 + cdr1;
        //                                iret1 = iret1 + cret1;
        //                                icr1 = icr1 + ccr1;
        //                                ibal1 = cbal1;

        //                                cdr = 0; ccr = 0; cret = 0;
        //                                cdr1 = 0; ccr1 = 0; cret1 = 0;
        //                                if (i > maxR) break;
        //                            }
        //                            if (i > maxR) break;
        //                        }
        //                        if (i > maxR) break;
        //                    }
        //                    gop = gop + iop; gdr = gdr + idr; gcr = gcr + icr; gret = gret + iret; gbal = gbal + ibal;
        //                    gop1 = gop1 + iop1; gdr1 = gdr1 + idr1; gcr1 = gcr1 + icr1; gret1 = gret1 + iret1; gbal1 = gbal1 + ibal1;
        //                    if (reptype == "I" || reptype == "T")
        //                    {
        //                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                        IR.Rows[rNo]["itcd"] = tbl.Rows[i - 1]["itcd"].ToString();
        //                        if (tbl.Rows[i - 1]["styleno"].ToString() == "") IR.Rows[rNo]["itnm"] = tbl.Rows[i - 1]["itnm"].ToString();
        //                        else IR.Rows[rNo]["itnm"] = tbl.Rows[i - 1]["styleno"].ToString();
        //                        IR.Rows[rNo]["opqnty"] = iop;
        //                        IR.Rows[rNo]["inqnty"] = idr;
        //                        IR.Rows[rNo]["outqnty"] = icr;
        //                        IR.Rows[rNo]["retqnty"] = iret;
        //                        IR.Rows[rNo]["balqnty"] = ibal;
        //                        if (stkcalcon == "Z")
        //                        {
        //                            IR.Rows[rNo]["opqnty1"] = iop1;
        //                            IR.Rows[rNo]["inqnty1"] = idr1;
        //                            IR.Rows[rNo]["outqnty1"] = icr1;
        //                            IR.Rows[rNo]["retqnty1"] = iret1;
        //                            IR.Rows[rNo]["balqnty1"] = ibal1;
        //                        }
        //                    }
        //                    if (i > maxR) break;
        //                }
        //                //total of Item Group
        //                top = top + gop; tdr = tdr + gdr; tcr = tcr + gcr; tret = tret + gret; tbal = tbal + gbal;
        //                top1 = top1 + gop1; tdr1 = tdr1 + gdr1; tcr1 = tcr1 + gcr1; tret1 = tret1 + gret1; tbal1 = tbal1 + gbal1;
        //                if (reptype == "G" || reptype == "T")
        //                {
        //                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                    IR.Rows[rNo]["itnm"] = "Total of [" + tbl.Rows[i - 1]["itgrpnm"].ToString() + " ]";
        //                    IR.Rows[rNo]["opqnty"] = gop;
        //                    IR.Rows[rNo]["inqnty"] = gdr;
        //                    IR.Rows[rNo]["outqnty"] = gcr;
        //                    IR.Rows[rNo]["retqnty"] = gret;
        //                    IR.Rows[rNo]["balqnty"] = gbal;
        //                    if (stkcalcon == "Z")
        //                    {
        //                        IR.Rows[rNo]["opqnty1"] = gop1;
        //                        IR.Rows[rNo]["inqnty1"] = gdr1;
        //                        IR.Rows[rNo]["outqnty1"] = gcr1;
        //                        IR.Rows[rNo]["retqnty1"] = gret1;
        //                        IR.Rows[rNo]["balqnty1"] = gbal1;
        //                    }
        //                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
        //                }
        //                if (i > maxR) break;
        //            }

        //            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //            IR.Rows[rNo]["itnm"] = "Grand Totals";
        //            IR.Rows[rNo]["opqnty"] = top;
        //            IR.Rows[rNo]["inqnty"] = tdr;
        //            IR.Rows[rNo]["outqnty"] = tcr;
        //            IR.Rows[rNo]["retqnty"] = tret;
        //            IR.Rows[rNo]["balqnty"] = tbal;
        //            if (stkcalcon == "Z")
        //            {
        //                IR.Rows[rNo]["opqnty1"] = top1;
        //                IR.Rows[rNo]["inqnty1"] = tdr1;
        //                IR.Rows[rNo]["outqnty1"] = tcr1;
        //                IR.Rows[rNo]["retqnty1"] = tret1;
        //                IR.Rows[rNo]["balqnty1"] = tbal1;
        //            }
        //            if (slcd == "") IR.Rows[rNo]["balqnty"] = tbal;
        //            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
        //            #endregion
        //        }

        //        string pghdr1 = "", repname = CommFunc.retRepname("Rep_Stk_Leg");
        //        pghdr1 = "Stock Ledger " + (VE.Checkbox3 == true ? " (Combined) " : "");
        //        if (mtrljobcd != "FS") pghdr1 += " [" + VE.TEXTBOX6 + "] ";
        //        pghdr1 += "from " + fdt + " to " + tdt;

        //        PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);
        //        return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
        //    }
        //    catch (Exception ex)
        //    {
        //        Cn.SaveException(ex, "");
        //        return Content(ex.Message);
        //    }
        //}
    }
}