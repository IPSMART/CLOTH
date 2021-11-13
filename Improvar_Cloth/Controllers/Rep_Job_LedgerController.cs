using System;
using System.Linq;
using System.Data;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;

namespace Improvar.Controllers
{
    public class Rep_Job_LedgerController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string jobcd = "", jobnm = "";
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_Job_Ledger
        public ActionResult Rep_Job_Ledger()
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
                    // VE = (ReportViewinHtml)Cn.EntryCommonLoading(VE, VE.PermissionID);
                    jobcd = VE.MENU_PARA;
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    string jobnm = DB.M_JOBMST.Find(jobcd)?.JOBNM;
                    ViewBag.formname = jobnm + " Ledger (Item)";
                    VE.FDT = CommVar.FinStartDate(UNQSNO);
                    VE.TDT = CommVar.CurrDate(UNQSNO);
                    VE.Checkbox1 = false; //show summary
                    VE.Checkbox2 = true; //Show Party
                    VE.Checkbox3 = false; //Merge Locations
                    VE.TEXTBOX2 = "P"; //Calc on Box/Pcs/Sets;
                    if (VE.MENU_PARA == "IR") VE.Checkbox3 = true;
                    string comcd = CommVar.Compcd(UNQSNO);
                    string location = CommVar.Loccd(UNQSNO);

                    jobcd = VE.MENU_PARA;
                    jobnm = DB.M_JOBMST.Find(jobcd)?.JOBNM;

                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);

                    VE.DropDown_list_ITGRP = DropDownHelp.GetItgrpcdforSelection();
                    VE.Itgrpnm = MasterHelp.ComboFill("itgrpcd", VE.DropDown_list_ITGRP, 0, 1);

                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("J");
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);

                    VE.DropDown_list_LINECD = DropDownHelp.GetLinecdforSelection();
                    VE.Linenm = MasterHelp.ComboFill("linecd", VE.DropDown_list_LINECD, 0, 1);

                    VE.TEXTBOX4 = MasterHelp.ComboFill("recslcd", VE.DropDown_list_SLCD, 0, 1);

                    VE.DefaultView = true;
                    VE.ExitMode = 1;
                    VE.DefaultDay = 0;
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

        [HttpPost]
        public ActionResult Rep_Job_Ledger(ReportViewinHtml VE, FormCollection FC)
        {
            try
            {
                Cn.getQueryString(VE);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));

                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string slcd = "", linecd = "", itcd = "", itgrpcd = "", fdt, tdt = "";
                string unselslcd = "", recslcd = "";
                fdt = VE.FDT;
                tdt = VE.TDT;
                jobcd = VE.MENU_PARA;
                jobnm = DB.M_JOBMST.Find(jobcd).JOBNM;
                bool showitem = false, showsumm = VE.Checkbox1, showparty = VE.Checkbox2;
                if (FC.AllKeys.Contains("slcdvalue")) slcd = FC["slcdvalue"].ToString().retSqlformat();
                if (FC.AllKeys.Contains("slcdunselvalue")) unselslcd = FC["slcdunselvalue"].ToString().retSqlformat();
                if (FC.AllKeys.Contains("recslcdvalue")) recslcd = FC["recslcdvalue"].ToString().retSqlformat();

                if (FC.AllKeys.Contains("linecdvalue"))
                {
                    linecd = FC["linecdvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("itgrpcdvalue"))
                {
                    itgrpcd = FC["itgrpcdvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("itcdvalue"))
                {
                    itcd = FC["itcdvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("linecdvalue")) linecd = FC["linecdvalue"].ToString().retSqlformat();

                string sql = "";
                //sql += "select a.autono, a.slno, a.progautono, a.progslno, b.proguniqno, b.dia, b.batchno, b.jobcd, ";
                //sql += "e.docdt, nvl(d.prefno,e.docno) docno, d.prefdt, d.doctag, d.slcd, k.slnm, d.linecd, m.linenm, ";
                //sql += "nvl(c.mtrljobcd,c.mtrljobcd) mtrljobcd, f.styleno, f.itnm, nvl(f.pcsperbox,0) pcsperbox, ";
                //sql += "f.mixsize, nvl(f.pcsperset,0) pcsperset, ";
                //sql += "d.slcd||nvl(d.linecd,'') repslcd, k.slnm||decode(m.linenm,null,'',' ['||m.linenm||']') repslnm, ";
                //sql += "b.itcd||nvl(g.partcd,'') repitcd, nvl(f.styleno,f.itnm)||decode(g.partnm,null,'',' ['||g.partnm||']') repitnm, ";
                //sql += "g.partnm, h.print_seq, h.sizenm, l.uomnm, nvl(l.decimals,0) decimals, ";
                //sql += "nvl(o.prefno,r.docno) recdocno, r.docdt recdocdt, ";
                //sql += "o.slcd||nvl(o.linecd,'') recslcd, p.slnm||decode(q.linenm,null,'',' ['||q.linenm||']') recslnm, ";
                //if (showitem == true) sql += "f.itgrpcd, i.itgrpnm, i.brandcd, j.brandnm, ";
                //else sql += "'' itgrpcd, '' itgrpnm, '' brandcd, '' brandnm, ";
                //sql += "b.slcd issslcd, b.itcd, b.partcd, b.sizecd, nvl(c.stktype,b.stktype) stktype, a.stkdrcr, decode(a.stkdrcr,'D',1,-1) mult, ";
                //if (jobcd == "DY" || jobcd == "BL" || jobcd == "KT" || jobcd == "YD")
                //{
                //    //sql += "nvl(nvl(a.qnty,c.qnty),0)-(case when nvl(c.shortqnty,0) < 0 then 0 else nvl(c.shortqnty,0) end) qnty, ";
                //    sql += "nvl(nvl(a.qnty,c.qnty),0)-nvl(c.shortqnty,0) qnty, ";
                //}
                //else
                //{
                //    sql += "nvl(nvl(c.qnty,a.qnty),0) qnty, ";
                //}
                //sql += "nvl(c.shortqnty,0) shortqnty ";
                //sql += "from " + scm + ".t_progdtl a, " + scm + ".t_progmast b, " + scm + ".t_txndtl c, " + scm + ".t_txn d, " + scm + ".t_cntrl_hdr e, ";
                //sql += scm + ".m_sitem f, " + scm + ".m_parts g, " + scm + ".m_size h, " + scm + ".m_group i, " + scm + ".m_brand j, ";
                //sql += scmf + ".m_subleg k, " + scmf + ".m_uom l, " + scm + ".m_linemast m, " + scm + ".t_txn o, " + scmf + ".m_subleg p, " + scm + ".m_linemast q, " + scm + ".t_cntrl_hdr r ";
                //sql += "where a.progautono=b.autono(+) and a.progslno=b.slno(+) and a.autono=c.recprogautono(+) and a.slno=c.recprogslno(+) and ";
                //sql += "a.autono=d.autono(+) and a.autono=e.autono(+) and d.slcd=k.slcd(+) and f.uomcd=l.uomcd(+) and d.linecd=m.linecd(+) and ";
                //sql += "e.compcd = '" + COM + "' and nvl(e.cancel,'N')='N' and b.jobcd='" + jobcd + "' and ";
                //if (VE.Checkbox3 == false) sql += "e.loccd = '" + LOC + "' and ";
                //sql += "b.recautono=o.autono(+) and o.slcd=p.slcd(+) and o.linecd=q.linecd(+) and o.autono=r.autono(+) and ";
                //sql += "e.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and ";
                //if (slcd != "") sql += "d.slcd in (" + slcd + ") and ";
                //if (recslcd != "") sql += "o.slcd in (" + recslcd + ") and ";
                //if (linecd != "") sql += "d.linecd in (" + linecd + ") and ";
                //if (itgrpcd != "") sql += "f.itgrpcd in (" + itgrpcd + ") and ";
                //if (itcd != "") sql += "b.itcd in (" + itcd + ") and ";
                //sql += "b.itcd=f.itcd(+) and b.partcd=g.partcd(+) and b.sizecd=h.sizecd(+) and f.itgrpcd=i.itgrpcd(+) and i.brandcd=j.brandcd(+) ";

                //sql += "union all ";


                //sql += "order by ";
                //if (showparty == true) sql += "repslnm, repslcd, ";

                //sql += "docdt, docno, autono, styleno, itnm, itcd, partcd, brandnm, brandcd, itgrpnm, itgrpcd, itnm, itcd, print_seq, sizenm ";

                sql = "";
                //sql += "select a.autono, a.slno, a.progautono, a.progslno, b.proguniqno, b.dia, b.batchno, b.jobcd, ";
                sql += "select a.autono, a.slno, a.progautono, a.progslno, b.proguniqno, b.dia, ''batchno, b.jobcd, ";
                sql += "e.docdt, nvl(d.prefno,e.docno) docno, d.prefdt, a.doctag, a.slcd, k.slnm, d.linecd, d.linecd, m.linenm, ";
                //sql += "a.mtrljobcd, f.styleno, f.itnm, nvl(f.pcsperbox,0) pcsperbox, ";
                sql += "a.mtrljobcd, f.styleno, f.itnm, 0 pcsperbox, ";
                //sql += "f.mixsize, nvl(f.pcsperset,0) pcsperset, ";
                sql += "'' mixsize, nvl(f.pcsperset,0) pcsperset, ";
                sql += "a.slcd||nvl(d.linecd,'') repslcd, k.slnm||decode(m.linenm,null,'',' ['||m.linenm||']') repslnm, ";
                sql += "b.itcd||nvl(g.partcd,'') repitcd, nvl(f.styleno,f.itnm)||decode(g.partnm,null,'',' ['||g.partnm||']') repitnm, ";
                sql += "g.partnm, h.print_seq, h.sizenm, l.uomnm, nvl(l.decimals,0) decimals, ";
                sql += "nvl(o.prefno,r.docno) recdocno, r.docdt recdocdt, ";
                sql += "o.slcd||nvl(o.linecd,'') recslcd, p.slnm||decode(q.linenm,null,'',' ['||q.linenm||']') recslnm, ";
                if (showitem == true) sql += "f.itgrpcd, i.itgrpnm, i.brandcd, j.brandnm, ";
                else sql += "'' itgrpcd, '' itgrpnm, '' brandcd, '' brandnm, ";
                sql += "b.slcd issslcd, b.itcd, b.partcd, b.sizecd, a.stktype, a.stkdrcr, decode(a.stkdrcr,'D',1,-1) mult, ";
                sql += "a.mtrljobcd, a.stktype, ";
                sql += "a.qnty, a.shortqnty from ";

                sql += "(select a.autono, a.slno, a.autono||a.slno autoslno, a.progautono, a.progslno, a.progautono||a.progslno progautoslno, a.stkdrcr, d.slcd, d.doctag, ";
                sql += "c.mtrljobcd, nvl(c.stktype,e.stktype) stktype, ";
                if (jobcd == "DY" || jobcd == "BL" || jobcd == "KT" || jobcd == "YD")
                {
                    //sql += "nvl(nvl(a.qnty,c.qnty),0)-(case when nvl(c.shortqnty,0) < 0 then 0 else nvl(c.shortqnty,0) end) qnty, ";
                    sql += "nvl(nvl(a.qnty,c.qnty),0)-nvl(c.shortqnty,0) qnty, ";
                }
                else
                {
                    sql += "nvl(nvl(c.qnty,a.qnty),0) qnty, ";
                }
                sql += "nvl(c.shortqnty,0) shortqnty ";
                sql += "from " + scm + ".t_progdtl a, " + scm + ".t_txndtl c, " + scm + ".t_txn d, " + scm + ".t_progmast e ";
                //sql += "where a.autono=c.recprogautono(+) and a.slno=c.recprogslno(+) and a.autono=d.autono(+) and a.progautono=e.autono(+) and a.progslno=e.slno(+)"; 
                sql += "where a.autono=c.autono(+) and a.slno=c.slno(+) and a.autono=d.autono(+) and a.progautono=e.autono(+) and a.progslno=e.slno(+)";
                sql += "union all ";
                sql += "select a.autono, a.slno, a.autono||a.slno autoslno, a.progautono, a.progslno, a.progautono||a.progslno progautoslno, 'C' stkdrcr, b.slcd, 'SE' doctag, ";
                sql += "'' mtrljobcd, e.stktype, ";
                sql += "0 qnty, nvl(a.short_allow,0) shortqnty ";
                sql += "from " + scm + ".t_prog_close a, " + scm + ".t_prog_close_hdr b, " + scm + ".t_cntrl_hdr c, " + scm + ".t_progmast e ";
                sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and nvl(a.short_allow,0) <> 0 and a.progautono=e.autono(+) and a.progslno=e.slno(+) ";
                sql += ") a, ";

                sql += scm + ".t_progmast b, " + scm + ".t_txn d, " + scm + ".t_cntrl_hdr e, ";
                //sql += scm + ".m_sitem f, " + scm + ".m_parts g, " + scm + ".m_size h, " + scm + ".m_group i, " + scm + ".m_brand j, ";
                sql += scm + ".m_sitem f, " + scm + ".m_parts g, " + scm + ".m_size h, " + scm + ".m_group i, ";
                sql += scmf + ".m_subleg k, " + scmf + ".m_uom l, " + scm + ".m_linemast m, " + scm + ".t_txn o, " + scmf + ".m_subleg p, " + scm + ".m_linemast q, " + scm + ".t_cntrl_hdr r ";
                sql += "where a.progautono=b.autono(+) and a.progslno=b.slno(+) and ";
                sql += "a.autono=d.autono(+) and a.autono=e.autono(+) and a.slcd=k.slcd(+) and f.uomcd=l.uomcd(+) and d.linecd=m.linecd(+) and ";
                sql += "e.compcd = '" + COM + "' and nvl(e.cancel,'N')='N' and b.jobcd='" + jobcd + "' and ";
                if (VE.Checkbox3 == false) sql += "e.loccd = '" + LOC + "' and ";
                sql += "b.recautono=o.autono(+) and o.slcd=p.slcd(+) and o.linecd=q.linecd(+) and o.autono=r.autono(+) and ";
                sql += "e.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and ";
                if (slcd != "") sql += "a.slcd in (" + slcd + ") and ";
                if (recslcd != "") sql += "o.slcd in (" + recslcd + ") and ";
                if (linecd != "") sql += "d.linecd in (" + linecd + ") and ";
                if (itgrpcd != "") sql += "f.itgrpcd in (" + itgrpcd + ") and ";
                if (itcd != "") sql += "b.itcd in (" + itcd + ") and ";
                //sql += "b.itcd=f.itcd(+) and b.partcd=g.partcd(+) and b.sizecd=h.sizecd(+) and f.itgrpcd=i.itgrpcd(+) and i.brandcd=j.brandcd(+) ";
                sql += "b.itcd=f.itcd(+) and b.partcd=g.partcd(+) and b.sizecd=h.sizecd(+) and f.itgrpcd=i.itgrpcd(+) ";
                sql += "order by ";
                if (showparty == true) sql += "repslnm, repslcd, ";
                sql += "docdt, docno, autono, styleno, itnm, itcd, partcd, brandnm, brandcd, itgrpnm, itgrpcd, itnm, itcd, print_seq, sizenm ";
                DataTable tbl = MasterHelp.SQLquery(sql);
                if (tbl.Rows.Count == 0)
                {
                    return Content("No Record Found");
                }

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                string qtydsp = "n,13,2:##,##,##,##0";
                string stkcalcon = VE.TEXTBOX2, qty1hd = "Box";
                if (stkcalcon == "S") qty1hd = "Sets";
                bool groupondate = false, showothrunit = true, showraka = true;
                if (showitem == false && showparty == false) groupondate = true;
                if (stkcalcon == "P") showothrunit = false;
                if (jobcd == "CT" || jobcd == "DY" || jobcd == "BL" || jobcd == "KT" || jobcd == "FP" || jobcd == "YD") qtydsp = "n,13,2:##,##,##0.000";
                if (jobcd == "KT" || jobcd == "DY" || jobcd == "BL" || jobcd == "FP") showraka = false;

                DataTable IR = new DataTable("mstrep");
                HC.RepStart(IR, 3);
                if (showsumm == true)
                {
                    HC.GetPrintHeader(IR, "docdt", "string", "n,3", "Sl");
                    HC.GetPrintHeader(IR, "docno", "string", "c,10", "Code");
                    HC.GetPrintHeader(IR, "dsc", "string", "c,40", "Party Name");
                }
                else
                {
                    HC.GetPrintHeader(IR, "docdt", "string", "c,10", "Date");
                    HC.GetPrintHeader(IR, "docno", "string", "c,16", "Ref No");
                    HC.GetPrintHeader(IR, "dsc", "string", "c,40", "particulars");
                    if (VE.Checkbox3 == true) HC.GetPrintHeader(IR, "recdocno", "string", "c,16", "Recd Ref");
                    if (VE.Checkbox3 == true) HC.GetPrintHeader(IR, "recslnm", "string", "c,20", "Recd from");
                }
                if (showsumm == true) HC.GetPrintHeader(IR, "opqnty", "double", qtydsp, "Op.Qty");
                HC.GetPrintHeader(IR, "issqnty", "double", qtydsp, "Iss.Qty");
                if (showothrunit == true) HC.GetPrintHeader(IR, "issqnty1", "double", "n,13,2:##,##,##,##0", "Iss." + qty1hd);
                HC.GetPrintHeader(IR, "recqnty", "double", qtydsp, "Rec.Qty");
                if (showothrunit == true) HC.GetPrintHeader(IR, "recqnty1", "double", "n,13,2:##,##,##,##0", "Rec." + qty1hd);
                HC.GetPrintHeader(IR, "shrqnty", "double", qtydsp, "Shortage");
                HC.GetPrintHeader(IR, "excqnty", "double", qtydsp, "Excess");
                if (showraka == true)
                {
                    HC.GetPrintHeader(IR, "losqnty", "double", qtydsp, "Loose");
                    HC.GetPrintHeader(IR, "rakqnty", "double", qtydsp, "Raka");
                }
                HC.GetPrintHeader(IR, "retqnty", "double", qtydsp, "Ret.Qty");
                HC.GetPrintHeader(IR, "balqnty", "double", qtydsp, "Bal.Qty");

                double gopqty = 0, gissqty = 0, grecqty = 0, gretqty = 0, gshrqty = 0, gexcqty = 0, gbalqty = 0, glosqty = 0, grakqty = 0;
                double gissqty1 = 0, grecqty1 = 0, gretqty1 = 0, gshrqty1 = 0;

                string chk1fld = "", chk1val = "", chk1nm = "", chk2fld = "", chk2val = "", chk2nm = "";
                Int32 maxR = 0, i = 0, rNo = 0, slno = 0;
                i = 0; maxR = tbl.Rows.Count - 1;
                if (showparty == false)
                {
                    chk1fld = "docdt"; chk1nm = "docdt"; chk2fld = "docdt"; chk2nm = "docdt";
                }
                else
                {
                    chk1fld = "repslcd"; chk1nm = "repslnm"; chk2fld = "repslcd"; chk2nm = "repslnm";
                }
                Int32 noparty = 0;
                while (i <= maxR)
                {
                    chk1val = tbl.Rows[i][chk1fld].ToString();
                    if (showsumm == false && (showparty == true || showitem == true))
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["Dammy"] = "[" + chk1val + "] " + tbl.Rows[i][chk1nm];
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                    }

                    double popqty = 0, pissqty = 0, precqty = 0, pretqty = 0, pshrqty = 0, pexcqty = 0, pbalqty = 0, plosqty = 0, prakqty = 0;
                    double pissqty1 = 0, precqty1 = 0, pretqty1 = 0, pshrqty1 = 0;
                    while (tbl.Rows[i][chk1fld].ToString() == chk1val)
                    {
                        chk2val = tbl.Rows[i][chk2fld].ToString();
                        double op = 0, op1 = 0, cl = 0, cl1 = 0;
                        double sopqty = 0, sissqty = 0, srecqty = 0, sretqty = 0, sshrqty = 0, sexcqty = 0, sbalqty = 0, slosqty = 0, srakqty = 0;
                        double sissqty1 = 0, srecqty1 = 0, sretqty1 = 0, sshrqty1 = 0;
                        while (tbl.Rows[i][chk1fld].ToString() == chk1val && tbl.Rows[i][chk2fld].ToString() == chk2val)
                        {
                            while (tbl.Rows[i][chk1fld].ToString() == chk1val && tbl.Rows[i][chk2fld].ToString() == chk2val && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(fdt))
                            {
                                string chk1 = tbl.Rows[i]["itcd"].ToString();
                                double cpcs = 0, cbox = 0, chkqty = 0, chkpcs = 0, cqty = 0;
                                while (tbl.Rows[i][chk1fld].ToString() == chk1val && tbl.Rows[i][chk2fld].ToString() == chk2val && tbl.Rows[i]["itcd"].ToString() == chk1 && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(fdt))
                                {
                                    string partcd = tbl.Rows[i]["partcd"].ToString();
                                    string autono = tbl.Rows[i]["autono"].ToString();
                                    while (tbl.Rows[i][chk1fld].ToString() == chk1val && tbl.Rows[i][chk2fld].ToString() == chk2val && tbl.Rows[i]["itcd"].ToString() == chk1 && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(fdt) && tbl.Rows[i]["autono"].ToString() == autono)
                                    {
                                        if (tbl.Rows[i]["partcd"].ToString() == partcd)
                                        {
                                            chkqty = ((Convert.ToDouble(tbl.Rows[i]["qnty"]) + Convert.ToDouble(tbl.Rows[i]["shortqnty"])) * Convert.ToDouble(tbl.Rows[i]["mult"]));
                                            cpcs = cpcs + chkqty;
                                            if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcs = chkpcs + chkqty;
                                        }
                                        i++;
                                        if (i > maxR) break;
                                    }
                                    if (i > maxR) break;
                                }
                                if (stkcalcon == "B") cqty = Salesfunc.ConvPcstoBox(chkpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
                                else if (stkcalcon == "S") cqty = Salesfunc.ConvPcstoSet(chkpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
                                else cqty = cpcs;
                                op = op + cpcs;
                                op1 = op1 + cqty;
                                cl = op; cl1 = op1;
                                if (i > maxR) break;
                            }
                            if (showsumm == false && op + op1 != 0)
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["docdt"] = fdt;
                                IR.Rows[rNo]["dsc"] = "Opening Stock";
                                IR.Rows[rNo]["balqnty"] = op;
                            }
                            sopqty = sopqty + op;
                            if (i > maxR)
                            {
                                sbalqty = op; break;
                            }

                            //for the period
                            double tissqty = 0, trecqty = 0, tretqty = 0, tshrqty = 0, texcqty = 0, tlosqty = 0, trakqty = 0;
                            double tissqty1 = 0, trecqty1 = 0, tretqty1 = 0, tshrqty1 = 0;
                            while (tbl.Rows[i][chk1fld].ToString() == chk1val && tbl.Rows[i][chk2fld].ToString() == chk2val && Convert.ToDateTime(tbl.Rows[i]["docdt"]) <= Convert.ToDateTime(tdt))
                            {
                                string autono = tbl.Rows[i]["autono"].ToString();
                                string dsc = "";
                                double issqty = 0, recqty = 0, retqty = 0, shrqty = 0, excqty = 0, losqty = 0, rakqty = 0;
                                double issqty1 = 0, recqty1 = 0, retqty1 = 0, shrqty1 = 0;
                                while (tbl.Rows[i][chk1fld].ToString() == chk1val && tbl.Rows[i][chk2fld].ToString() == chk2val && tbl.Rows[i]["autono"].ToString() == autono)
                                {
                                    string chk1 = tbl.Rows[i]["itcd"].ToString();
                                    string partcd = tbl.Rows[i]["partcd"].ToString();
                                    double cpcs = 0, cbox = 0, chkqty = 0, chkpcs = 0, cqty = 0;
                                    if (tbl.Rows[i]["repitnm"].ToString() == "") dsc += tbl.Rows[i]["repitnm"].ToString() + ",";
                                    else dsc += tbl.Rows[i]["repitnm"].ToString() + ",";
                                    double shortqnty = 0;
                                    while (tbl.Rows[i][chk1fld].ToString() == chk1val && tbl.Rows[i][chk2fld].ToString() == chk2val && tbl.Rows[i]["autono"].ToString() == autono && tbl.Rows[i]["itcd"].ToString() == chk1)
                                    {
                                        if (tbl.Rows[i]["partcd"].ToString() == partcd)
                                        {
                                            chkqty = Convert.ToDouble(tbl.Rows[i]["qnty"]);
                                            shortqnty = shortqnty + Convert.ToDouble(tbl.Rows[i]["shortqnty"]);
                                            if (tbl.Rows[i]["stktype"].ToString() == "D" || tbl.Rows[i]["stktype"].ToString() == "R") rakqty = rakqty + chkqty;
                                            else if (tbl.Rows[i]["stktype"].ToString() == "L") losqty = losqty + chkqty;
                                            else cpcs = cpcs + chkqty;
                                            if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcs = chkpcs + chkqty;
                                        }
                                        i++;
                                        if (i > maxR) break;
                                    }
                                    if (shortqnty >= 0) shrqty = shrqty + shortqnty;
                                    if (shortqnty < 0) excqty = excqty + Math.Abs(shortqnty);
                                    if (stkcalcon == "B") cqty = Salesfunc.ConvPcstoBox(chkpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
                                    else if (stkcalcon == "S") cqty = Salesfunc.ConvPcstoSet(chkpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
                                    else cqty = cpcs;
                                    switch (tbl.Rows[i - 1]["doctag"].ToString())
                                    {
                                        case "JC":
                                            issqty = issqty + cpcs + losqty + rakqty; issqty1 = issqty1 + cqty + losqty + rakqty; losqty = 0; rakqty = 0; break;
                                        case "JR":
                                            recqty = recqty + cpcs; recqty1 = recqty1 + cqty; break;
                                        case "JU":
                                            retqty = retqty + cpcs + losqty + rakqty; retqty1 = retqty1 + cqty + losqty + rakqty; losqty = 0; rakqty = 0; break;
                                    }
                                    if (i > maxR) break;
                                }
                                if (tbl.Rows[i - 1]["doctag"].ToString() == "JC")
                                {
                                    issqty = issqty + losqty + rakqty; losqty = 0; rakqty = 0;
                                }
                                cl = cl + issqty - recqty - retqty - shrqty + excqty - losqty - rakqty;
                                tissqty = tissqty + issqty;
                                trecqty = trecqty + recqty;
                                tretqty = tretqty + retqty;
                                tshrqty = tshrqty + shrqty;
                                texcqty = texcqty + excqty;
                                tlosqty = tlosqty + losqty;
                                trakqty = trakqty + rakqty;
                                tissqty1 = tissqty1 + issqty1;
                                trecqty1 = trecqty1 + recqty1;
                                if (showsumm == false)
                                {
                                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                    IR.Rows[rNo]["docdt"] = tbl.Rows[i - 1]["docdt"].ToString().retDateStr();
                                    IR.Rows[rNo]["docno"] = tbl.Rows[i - 1]["docno"];
                                    IR.Rows[rNo]["dsc"] = dsc;
                                    if (VE.Checkbox3 == true) IR.Rows[rNo]["recdocno"] = tbl.Rows[i - 1]["recdocno"];
                                    if (VE.Checkbox3 == true) IR.Rows[rNo]["recslnm"] = tbl.Rows[i - 1]["recslnm"];
                                    IR.Rows[rNo]["issqnty"] = issqty;
                                    IR.Rows[rNo]["recqnty"] = recqty;
                                    IR.Rows[rNo]["retqnty"] = retqty;
                                    IR.Rows[rNo]["shrqnty"] = shrqty;
                                    IR.Rows[rNo]["excqnty"] = excqty;
                                    if (showraka == true)
                                    {
                                        if (showothrunit == true) IR.Rows[rNo]["issqnty1"] = issqty1;
                                        if (showothrunit == true) IR.Rows[rNo]["recqnty1"] = recqty1;
                                        IR.Rows[rNo]["losqnty"] = losqty;
                                        IR.Rows[rNo]["rakqnty"] = rakqty;
                                    }
                                    IR.Rows[rNo]["balqnty"] = cl;
                                }
                                if (i > maxR) break;
                            }
                            //
                            sissqty = sissqty + tissqty;
                            srecqty = srecqty + trecqty;
                            sretqty = sretqty + tretqty;
                            sshrqty = sshrqty + tshrqty;
                            sexcqty = sexcqty + texcqty;
                            slosqty = slosqty + tlosqty;
                            srakqty = srakqty + trakqty;
                            sbalqty = sbalqty + cl;
                            sissqty1 = sissqty1 + tissqty1;
                            srecqty1 = srecqty1 + trecqty1;
                            if (i > maxR) break;
                        }
                        popqty = popqty + sopqty;
                        pissqty = pissqty + sissqty;
                        precqty = precqty + srecqty;
                        pretqty = pretqty + sretqty;
                        pshrqty = pshrqty + sshrqty;
                        pexcqty = pexcqty + sexcqty;
                        pbalqty = pbalqty + sbalqty;
                        plosqty = plosqty + slosqty;
                        prakqty = prakqty + srakqty;
                        pissqty1 = pissqty1 + sissqty1;
                        precqty1 = precqty1 + srecqty1;
                        if (i > maxR) break;
                    }
                    gopqty = gopqty + popqty;
                    gissqty = gissqty + pissqty;
                    grecqty = grecqty + precqty;
                    gretqty = gretqty + pretqty;
                    gshrqty = gshrqty + pshrqty;
                    gexcqty = gexcqty + pexcqty;
                    gbalqty = gbalqty + pbalqty;
                    glosqty = glosqty + plosqty;
                    grakqty = grakqty + prakqty;
                    gissqty1 = gissqty1 + pissqty1;
                    grecqty1 = grecqty1 + precqty1;
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    if (showsumm == false)
                    {
                        if (showparty == true || showitem == true)
                        {
                            IR.Rows[rNo]["dsc"] = "Total of [" + tbl.Rows[i - 1][chk1nm] + " ]";
                        }
                        else
                        {
                            IR.Rows[rNo]["dsc"] = "Total";
                        }
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                    }
                    else
                    {
                        noparty++;
                        IR.Rows[rNo]["docdt"] = noparty;
                        if (groupondate == true)
                        {
                            IR.Rows[rNo]["docno"] = chk1val.retDateStr();
                            IR.Rows[rNo]["dsc"] = tbl.Rows[i - 1][chk1nm].ToString().retDateStr();
                        }
                        else
                        {
                            IR.Rows[rNo]["docno"] = chk1val;
                            IR.Rows[rNo]["dsc"] = tbl.Rows[i - 1][chk1nm];
                        }
                        IR.Rows[rNo]["opqnty"] = popqty;
                    }
                    IR.Rows[rNo]["issqnty"] = pissqty;
                    IR.Rows[rNo]["recqnty"] = precqty;
                    IR.Rows[rNo]["retqnty"] = pretqty;
                    IR.Rows[rNo]["shrqnty"] = pshrqty;
                    IR.Rows[rNo]["excqnty"] = pexcqty;
                    if (showraka == true)
                    {
                        IR.Rows[rNo]["losqnty"] = plosqty;
                        IR.Rows[rNo]["rakqnty"] = prakqty;
                        if (showothrunit == true) IR.Rows[rNo]["issqnty1"] = pissqty1;
                        if (showothrunit == true) IR.Rows[rNo]["recqnty1"] = precqty1;
                    }
                    IR.Rows[rNo]["balqnty"] = pbalqty;
                    if (i > maxR) break;
                }

                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["dsc"] = "Grand Totals";
                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                if (showsumm == true) IR.Rows[rNo]["opqnty"] = gopqty;
                IR.Rows[rNo]["issqnty"] = gissqty;
                IR.Rows[rNo]["recqnty"] = grecqty;
                IR.Rows[rNo]["retqnty"] = gretqty;
                IR.Rows[rNo]["shrqnty"] = gshrqty;
                IR.Rows[rNo]["excqnty"] = gexcqty;
                if (showraka == true)
                {
                    IR.Rows[rNo]["losqnty"] = glosqty;
                    IR.Rows[rNo]["rakqnty"] = grakqty;
                    if (showothrunit == true) IR.Rows[rNo]["issqnty1"] = gissqty1;
                    if (showothrunit == true) IR.Rows[rNo]["recqnty1"] = grecqty1;
                }
                IR.Rows[rNo]["balqnty"] = gbalqty;

                string pghdr1 = "", repname = CommFunc.retRepname("rep_partyleg");
                pghdr1 = "Party Ledger (" + jobnm + ") " + (VE.Checkbox3 == true ? " (Combine) " : "") + "from " + fdt + " to " + tdt;

                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
    }
}