using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using OfficeOpenXml;
using OfficeOpenXml.Table;

namespace Improvar.Controllers
{
    public class Rep_RegController : Controller
    {
        public static string[,] headerArray;
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        DropDownHelp DropDownHelp = new DropDownHelp();
        // GET: Rep_Reg
        public ActionResult Rep_Reg()
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
                    ViewBag.formname = "Registers";
                    switch (VE.MENU_PARA)
                    {
                        case "SB":
                            ViewBag.formname = "Sales Registers"; break;
                        case "PB":
                            ViewBag.formname = "Purchase Registers"; break;
                        case "CM":
                            ViewBag.formname = "Cash Memo Registers"; break;
                        default: ViewBag.formname = ""; break;
                    }
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    string com = CommVar.Compcd(UNQSNO);
                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("");
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);
                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("A");
                    VE.Agslnm = MasterHelp.ComboFill("agslcd", VE.DropDown_list_SLCD, 0, 1);
                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("A");
                    VE.SubAgent = MasterHelp.ComboFill("sagslcd", VE.DropDown_list_SLCD, 0, 1);
                    var bltypelst = DropDownHelp.DropDownBLTYPE();
                    VE.BlType = MasterHelp.ComboFill("bltype", bltypelst, 0, 1);
                    VE.FDT = CommVar.CurrDate(UNQSNO);     //CommVar.FinStartDate(UNQSNO);
                    VE.TDT = CommVar.CurrDate(UNQSNO);
                    //=========For Report Type===========//
                    List<DropDown_list1> RT = new List<DropDown_list1>();
                    if (VE.MENU_PARA == "SB")
                    {

                        DropDown_list1 RT1 = new DropDown_list1();
                        RT1.value = "Sales";
                        RT1.text = "Sales";
                        RT.Add(RT1);
                        DropDown_list1 RT2 = new DropDown_list1();
                        RT2.value = "Sales Return";
                        RT2.text = "Sales Return";
                        RT.Add(RT2);
                        RT.Add(new DropDown_list1 { value = "SDWOQ", text = "Sales Debit Note (W/O Qnty)" });
                        RT.Add(new DropDown_list1 { value = "SCWOQ", text = "Sales Credit Note (W/O Qnty)" });
                        RT.Add(new DropDown_list1 { value = "Proforma", text = "Proforma Invoice" });
                        RT.Add(new DropDown_list1 { value = "Sales Cash Memo", text = "Sales Cash Memo" });
                        RT.Add(new DropDown_list1 { value = "Cash Memo Credit Note", text = "Cash Memo Credit Note" });
                        RT.Add(new DropDown_list1 { value = "Cash Sales", text = "Cash Sales" });
                        RT.Add(new DropDown_list1 { value = "Job Bill raised to Party", text = "Job Bill raised to Party" });

                        VE.DropDown_list1 = RT;
                    }
                    else if (VE.MENU_PARA == "CM")
                    {
                        RT.Add(new DropDown_list1 { value = "Sales Cash Memo", text = "Sales Cash Memo" });
                        RT.Add(new DropDown_list1 { value = "Cash Memo Credit Note", text = "Cash Memo Credit Note" });
                        VE.DropDown_list1 = RT;
                    }
                    else
                    {
                        DropDown_list1 RT3 = new DropDown_list1();
                        RT3.value = "Purchase";
                        RT3.text = "Purchase";
                        RT.Add(RT3);
                        DropDown_list1 RT4 = new DropDown_list1();
                        RT4.value = "Purchase Return";
                        RT4.text = "Purchase Return";
                        RT.Add(RT4);
                        DropDown_list1 RT5 = new DropDown_list1();
                        RT5.value = "Opening Stock";
                        RT5.text = "Opening Stock";
                        RT.Add(RT5);
                        RT.Add(new DropDown_list1 { value = "PDWOQ", text = "Purchase Debit Note (W/O Qnty)" });
                        RT.Add(new DropDown_list1 { value = "PCWOQ", text = "Purchase Credit Note (W/O Qnty)" });
                        VE.DropDown_list1 = RT;
                    }
                    //=========End Report Type===========//


                    VE.DropDown_list3 = (from i in DBF.M_LOCA
                                         where i.COMPCD == com
                                         select new DropDown_list3() { value = i.LOCCD, text = i.LOCNM }).Distinct().OrderBy(s => s.text).ToList();// location
                    VE.TEXTBOX5 = MasterHelp.ComboFill("loccd", VE.DropDown_list3, 0, 1);
                    //VE.DropDown_list2 = MasterHelp.INSURANCE().ConvertAll(x => new DropDown_list2 { text = x.Text, value = x.Value });

                    INI inifile = new INI();
                    string[] SectionName = inifile.GetSectionNames(Server.MapPath("~/Ipsmart.ini"));
                    VE.DropDown_list2 = (from i in SectionName
                                         where i.Contains("Rep_Reg_")
                                         select new DropDown_list2() { value = i, text = i }).Distinct().OrderBy(s => s.text).ToList();
                    INI Handel_Ini = new INI();
                    if (VE.DropDown_list2 != null && VE.DropDown_list2.Count() != 0)
                    {
                        foreach (var v in VE.DropDown_list2)
                        {
                            string ActiveSection = Handel_Ini.IniReadValue(v.value, "ACTIVE", Server.MapPath("~/Ipsmart.ini"));
                            if (ActiveSection != "")
                            {
                                VE.TEXTBOX6 = v.value;
                                break;
                            }
                        }
                    }
                    ShowAllColumn(VE);
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
        public ActionResult GetDOC_Number(string val, string Code)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", MasterHelp.DOCNO_help(val, Code));
                }
                else
                {
                    string str = MasterHelp.DOCNO_help(val, Code);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
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
        [HttpPost]
        public ActionResult Rep_Reg(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                //ReportViewinHtml VE = new ReportViewinHtml();
                Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string dtlsumm = "";
                string fdt = VE.FDT.retDateStr(), tdt = VE.TDT.retDateStr();

                string GODOWN = VE.TEXTBOX3.retStr();
                bool itmdtl = false, batchdtl = false, itemrem = false;
                dtlsumm = FC["DtlSumm"].ToString();
                if (dtlsumm == "ID") itmdtl = true;   //if (VE.Checkbox1 == true) itmdtl = true;
                if (VE.Checkbox2 == true) batchdtl = true;
                if (VE.Checkbox6 == true) itemrem = true;

                string itgrpcd = "";

                //string reptype = FC["reptype"].ToString();
                string selslcd = "", unselslcd = "", selloccd = "", selagslcd = "", bltype = "",selSagslcd="";
                if (FC.AllKeys.Contains("slcdvalue")) selslcd = CommFunc.retSqlformat(FC["slcdvalue"].ToString());
                if (FC.AllKeys.Contains("slcdunselvalue")) unselslcd = CommFunc.retSqlformat(FC["slcdunselvalue"].ToString());
                if (FC.AllKeys.Contains("ITGRPCDvalue")) itgrpcd = CommFunc.retSqlformat(FC["ITGRPCDvalue"].ToString());
                if (FC.AllKeys.Contains("loccdvalue")) selloccd = FC["loccdvalue"].retSqlformat();
                if (FC.AllKeys.Contains("agslcdvalue")) selagslcd = CommFunc.retSqlformat(FC["agslcdvalue"].ToString());
                if (FC.AllKeys.Contains("sagslcdvalue")) selSagslcd = CommFunc.retSqlformat(FC["sagslcdvalue"].ToString());
                if (FC.AllKeys.Contains("bltypevalue")) bltype = CommFunc.retSqlformat(FC["bltypevalue"].ToString());
                string txntag = "", doctype = ""; string regdsp = "";
                txntag = "SALES";
                if (VE.TEXTBOX7.retStr() != "")
                {
                    regdsp = VE.TEXTBOX7.ToString() + " Register";
                }
                else
                {
                    regdsp = VE.TEXTBOX1.ToString() + " Register";
                }
                var MSYSCNFG = DB.M_SYSCNFG.OrderByDescending(t => t.EFFDT).FirstOrDefault();

                string repsorton = FC["RepSortOn"].ToString();
                bool plistprint = VE.Checkbox4;
                bool transprint = VE.Checkbox3;
                bool con_print = VE.Checkbox9;
                //bool saprefnoprint = VE.Checkbox5;
                bool orddetprint = VE.Checkbox7;
                bool SeparateAchead = VE.Checkbox8;
                switch (VE.TEXTBOX1)
                {
                    case "Sales":
                        txntag = "'SB'";
                        doctype = "'SBILD','SPSLP'"; break;
                    case "Purchase":
                        txntag = "'PB'";
                        doctype = "'SPBL'"; break;
                    case "Sales Return":
                        txntag = "'SR'";
                        doctype = "'SRET'"; break;
                    case "Sales Cash Memo":
                        txntag = "'SB'";
                        doctype = "'SBCM'"; break;
                    case "Cash Sales":
                        txntag = "'SB'";
                        doctype = "'SBPOS'"; break;
                    case "Cash Memo Credit Note":
                        txntag = "'SB'";
                        doctype = "'SBCMR'"; break;
                    case "Purchase Return":
                        txntag = "'PR'";
                        doctype = "'SPRM'"; break;
                    case "Opening Stock":
                        txntag = "'OP'"; break;
                    case "Proforma":
                        txntag = "'PI'";
                        doctype = "'PROF'"; break;
                    case "SDWOQ":
                        txntag = "'SD'"; break;
                    case "SCWOQ":
                        txntag = "'SC'"; break;
                    case "PDWOQ":
                        txntag = "'PD'"; break;
                    case "PCWOQ":
                        txntag = "'PC'"; break;
                    case "Job Bill raised to Party":
                        txntag = "'JB'"; break;
                    default: txntag = ""; break;

                }
                // }

                string sql = "";
                sql += " select a.autono, a.doccd, a.docno,a.doctag, a.cancel,to_char(a.docdt,'DD/MM/YYYY')docdt,h.agslcd, " + Environment.NewLine;
                sql += "  a.prefno, nvl(to_char(a.prefdt,'dd/mm/yyyy'),'')prefdt, a.slcd, c.slnm,c.slarea,l.slnm agslnm,m.slnm sagslnm,i.nm,i.mobile,c.gstno, c.district, nvl(a.roamt, 0) roamt, " + Environment.NewLine;
                sql += " nvl(a.tcsamt, 0) tcsamt, a.blamt, " + Environment.NewLine;
                sql += "   b.slno,b.stkdrcr, b.itcd, " + Environment.NewLine;
                //query1 += "   b.itnm,b.itstyle, b.itrem, b.hsncode, b.uomcd, b.uomnm, b.decimals, b.nos, ";
                sql += "   b.itnm,b.itstyle, b.itrem, b.hsncode,nvl(b.bluomcd,b.uomcd)uomcd, nvl(b.bluomnm,b.uomnm)uomnm, nvl(nullif(b.bldecimals,0),b.decimals) decimals, b.nos, " + Environment.NewLine;
                sql += " nvl(nullif(b.blqnty,0),b.qnty)qnty, b.rate, b.amt,b.scmdiscamt, b.tddiscamt, b.discamt,b.TXBLVAL, g.conslcd, d.slnm cslnm, d.gstno cgstno, d.district cdistrict, " + Environment.NewLine;
                //query1 += " b.qnty, b.rate, b.amt,b.scmdiscamt, b.tddiscamt, b.discamt,b.TXBLVAL, g.conslcd, d.slnm cslnm, d.gstno cgstno, d.district cdistrict, ";
                sql += " e.slnm trslnm, f.lrno,nvl(to_char(f.lrdt,'dd/mm/yyyy'),'')lrdt,f.GRWT,f.TRWT,f.NTWT, '' ordrefno, to_char(nvl('', ''), 'dd/mm/yyyy') ordrefdt, b.igstper, b.igstamt, b.cgstper, " + Environment.NewLine;
                sql += " b.cgstamt,b.sgstamt, b.cessper, b.cessamt,b.blqnty,b.NETAMT,b.sgstper,b.igstper+b.cgstper+b.sgstper gstper,b.igstamt + b.cgstamt + b.sgstamt gstamt,k.ackno,nvl(to_char(k.ackdt,'dd/mm/yyyy'),'')ackdt,b.pageno,b.PAGESLNO,b.baleno,h.docrem,h.bltype  " + Environment.NewLine;

                sql += " from ( " + Environment.NewLine;
                sql += " select a.autono,a.doctag, b.doccd, b.docno, b.cancel, " + Environment.NewLine;
                sql += "b.docdt, " + Environment.NewLine;
                sql += "a.prefno, a.prefdt, a.slcd, a.roamt, a.tcsamt, a.blamt  " + Environment.NewLine;

                sql += "from " + scm1 + ".t_txn a, " + scm1 + ".t_cntrl_hdr b, " + Environment.NewLine;
                sql += " " + scmf + ".m_subleg c " + Environment.NewLine;
                sql += "  where a.autono = b.autono and a.slcd = c.slcd(+) " + Environment.NewLine;
                sql += "  and  b.compcd = '" + COM + "' " + Environment.NewLine;
                if (selloccd == "") sql += " and b.loccd = '" + LOC + "'  " + Environment.NewLine; else sql += " and b.loccd in (" + selloccd + ")  " + Environment.NewLine;
                if (GODOWN.retStr() != "") sql += "and  a.GOCD in('" + GODOWN + "')  " + Environment.NewLine;
                if (repsorton == "bldt")
                {
                    if (fdt != "") sql += "and a.prefdt >= to_date('" + fdt + "','dd/mm/yyyy')  " + Environment.NewLine;
                    if (tdt != "") sql += "and a.prefdt <= to_date('" + tdt + "','dd/mm/yyyy')   " + Environment.NewLine;
                }
                else
                {
                    if (fdt != "") sql += "and b.docdt >= to_date('" + fdt + "','dd/mm/yyyy')   " + Environment.NewLine;
                    if (tdt != "") sql += "and b.docdt <= to_date('" + tdt + "','dd/mm/yyyy')   " + Environment.NewLine;
                }
                sql += "and a.doctag in (" + txntag + ") " + Environment.NewLine;
                sql += " ) a,  " + Environment.NewLine;

                sql += "(select distinct a.autono,a.stkdrcr, a.slno, a.itcd, a.itrem, " + Environment.NewLine;
                sql += " b.itnm,b.styleno||' '||b.itnm itstyle, b.hsncode hsncode, b.uomcd, c.uomnm, c.decimals, " + Environment.NewLine;
                sql += "  a.nos, a.qnty, a.rate, a.amt,a.scmdiscamt,a.tddiscamt,a.discamt,a.TXBLVAL,a.NETAMT,   " + Environment.NewLine;
                sql += " a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.cessper, a.cessamt,a.blqnty,a.bluomcd,f.uomnm bluomnm,f.decimals bldecimals,a.pageno,a.pageslno,a.baleno  from " + scm1 + ".t_txndtl a, " + Environment.NewLine;
                sql += "" + scm1 + ".m_sitem b, " + scmf + ".m_uom c, " + scm1 + ".t_batchdtl d, " + scm1 + ".t_batchmst e, " + scmf + ".m_uom f " + Environment.NewLine;
                sql += " where a.itcd = b.itcd  and b.uomcd = c.uomcd and a.autono = d.autono(+) and a.slno=d.txnslno and d.barno = e.barno(+) and  a.bluomcd= f.uomcd(+) " + Environment.NewLine;
                sql += " group by " + Environment.NewLine;
                sql += " a.autono,a.stkdrcr, a.slno, a.itcd, a.itrem, " + Environment.NewLine;
                sql += "  b.itnm, b.hsncode, b.uomcd, c.uomnm, c.decimals, a.nos, a.qnty, a.rate, a.amt,a.scmdiscamt,  " + Environment.NewLine;
                sql += " a.tddiscamt, a.discamt,a.TXBLVAL,a.NETAMT, a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.cessper, a.cessamt,a.blqnty,a.bluomcd,f.uomnm,f.decimals,b.styleno||' '||b.itnm,a.pageno,a.PAGESLNO,a.baleno " + Environment.NewLine;
                sql += " union " + Environment.NewLine;
                sql += "select a.autono,";
                if (txntag == "SB" || txntag == "PR") { sql += "'C'"; } else { sql += "'D'"; }
                sql += " stkdrcr, a.slno + 1000 slno, a.amtcd itcd, '' itrem , b.amtnm itnm,b.amtnm itstyle ,a.hsncode,  " + Environment.NewLine;
                sql += " 'OTH' uomcd, 'OTH' uomnm, 0 decimals, 0 nos, 0 qnty, a.amtrate rate, a.amt,0 scmdiscamt, 0 tddiscamt, 0 discamt,a.amt TXBLVAL,0 NETAMT, a.igstper, a.igstamt, " + Environment.NewLine;
                sql += " a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.cessper, a.cessamt,0 blqnty,'' bluomcd,''bluomnm,0 bldecimals,0 pageno,0 PAGESLNO,''baleno " + Environment.NewLine;
                sql += " from " + scm1 + ".t_txnamt a, " + scm1 + ".m_amttype b " + Environment.NewLine;
                sql += " where a.amtcd = b.amtcd " + Environment.NewLine;
                sql += " ) b, " + scmf + ".m_subleg c, " + scmf + ".m_subleg d, " + scmf + ".m_subleg e, " + scm1 + ".t_txntrans f, " + Environment.NewLine;
                sql += "" + scm1 + ".t_txn g, " + scm1 + ".t_txnoth h ," + scm1 + ".t_txnmemo i ," + scmf + ".m_doctype j," + scmf + ".t_txneinv k," + scmf + ".m_subleg l, " + Environment.NewLine;
                sql += "" + scmf + ".m_subleg m " + Environment.NewLine;
                sql += "where a.autono = b.autono(+) and a.slcd = c.slcd and g.conslcd = d.slcd(+) and a.autono = f.autono(+) and h.agslcd = l.slcd(+)  and h.sagslcd = m.slcd(+) " + Environment.NewLine;
                sql += "and f.translcd = e.slcd(+) and a.autono = f.autono(+) and a.autono = g.autono(+) and a.autono = h.autono(+) and  g.autono = i.autono(+) and a.doccd = j.doccd(+) and a.autono = k.autono(+)  " + Environment.NewLine;
                if (selslcd != "") sql += " and a.slcd in (" + selslcd + ") " + Environment.NewLine;
                if (unselslcd != "") sql += " and a.slcd not in (" + unselslcd + ") " + Environment.NewLine;
                if (selagslcd != "") sql += " and h.agslcd in (" + selagslcd + ") " + Environment.NewLine;
                if (selSagslcd != "") sql += " and h.sagslcd in (" + selSagslcd + ") " + Environment.NewLine;
                if (bltype != "") sql += " and h.bltype in (" + bltype + ") " + Environment.NewLine;
                if (doctype != "") sql += " and j.doctype in(" + doctype + ") " + Environment.NewLine;
                sql += "order by ";
                if (repsorton == "partywise") { sql += "slcd,a.prefdt,prefno,a.docdt,docno" + Environment.NewLine; }
                else if (repsorton == "bldt") { sql += "a.prefdt,prefno" + Environment.NewLine; }
                else { sql += "a.docdt"; }
                if (itmdtl == true)
                {
                    sql += " , docno, slno ";
                }
                else
                {
                    sql += " , docno, igstper, cgstper, sgstper ";
                }

                DataTable tbl = MasterHelp.SQLquery(sql);
                if (tbl.Rows.Count == 0)
                {
                    return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "Records not found !!" });
                }
                string query2 = " select b.amt payamt,b.autono from " + scm1 + ".t_txnpymt b ";
                var paymentDt = MasterHelp.SQLquery(query2);

                if (dtlsumm == "E")
                {
                    string colnm = ShowAllColumn(VE, "ExcelHeader");
                    if (colnm.retStr() != "")
                    {
                        DataView dv = new DataView(tbl);
                        string[] COL = colnm.Split(',').ToArray();
                        tbl = dv.ToTable(true, COL);
                    }
                    DataTable[] exdt = new DataTable[1];
                    exdt[0] = tbl;
                    string[] sheetname = new string[1];
                    sheetname[0] = "Sheet1";
                    RegExcelfromDataTables(exdt, sheetname, regdsp + " Register".retRepname(), false, regdsp + " Register");
                    return null;
                }
                else {
                    var cnt_blqnty = (from DataRow dr in tbl.Rows where dr["blqnty"].retDbl() != 0 select dr["blqnty"].retDbl()).ToList();
                    //  DataTable rsStkPrcDesc;
                    //string query_new = "";
                    //query_new += "select distinct a.autono, c.prcdesc stkprcdesc, nvl(e.docrem,'') docrem ";
                    //query_new += "from " + scm1 + ".t_batchdtl a, " + scm1 + ".t_batchmst b, " + scm1 + ".m_itemplist c, " + scm1 + ".t_cntrl_hdr d, " + scm1 + ".T_CNTRL_HDR_REM e ";
                    //query_new += "where a.batchautono=b.batchautono(+) and b.itmprccd=c.itmprccd(+) and a.autono=d.autono(+) and a.autono=e.autono(+) and ";
                    //query_new += "d.compcd='" + COM + "' ";
                    //if (selloccd == "") query_new = query_new + " and d.loccd='" + LOC + "'  "; else query_new = query_new + " and d.loccd in (" + selloccd + ")  ";
                    //query_new += " and a.autono=e.autono(+) and (e.slno=1 or e.slno is null) ";
                    //if (fdt != "") query_new += "and d.docdt >= to_date('" + fdt + "','dd/mm/yyyy') ";
                    //if (tdt != "") query_new += "and d.docdt <= to_date('" + tdt + "','dd/mm/yyyy') ";

                    //   rsStkPrcDesc = MasterHelp.SQLquery(query_new);

                    DataTable IR = new DataTable("mstrep");
                    Models.PrintViewer PV = new Models.PrintViewer();
                    HtmlConverter HC = new HtmlConverter();
                    string pghdr1 = "";

                    double tnos = 0; double tqnty = 0; double tbasamt = 0; double tdisc1 = 0; double tdisc2 = 0; double ttaxable = 0; double tgstamt = 0, tbqnty = 0;
                    double tigstamt = 0; double tcgstamt = 0; double tsgstamt = 0; double troamt = 0; double ttcsamt = 0; double tblamt = 0; double tpayamt = 0;
                    string auto1 = "";
                    double dsc1 = 0; double dsc2 = 0;
                    bool bldtl = true, showpbill = false;
                    //use for amount separation
                    DataTable amtDT = new DataTable("amtDT");
                    DataRow[] amtrows = tbl.Select("slno>1000");
                    if (amtrows.Count() > 0)
                    {
                        amtDT = amtrows.CopyToDataTable();
                    }
                    string[] amtTBLCOLS = new string[] { "itcd", "itnm" };
                    if (amtDT.Rows.Count > 0) amtDT = amtDT.DefaultView.ToTable(true, amtTBLCOLS);
                    amtDT.Columns.Add("AmtItcdRate", typeof(double));
                    amtDT.Columns.Add("AmtItcdtotAmt", typeof(double));

                    Int32 i = 0, istore = 0, rNo = 0, maxR = tbl.Rows.Count - 1;
                    if (((VE.TEXTBOX1 == "Purchase" || VE.TEXTBOX1 == "Purchase Return" || VE.TEXTBOX1 == "Purchase Return" || VE.TEXTBOX1 == "Opening Stock" || VE.TEXTBOX1 == "PDWOQ" || VE.TEXTBOX7 == "PCWOQ"))) showpbill = true;
                    #region Normal Report               
                    HC.RepStart(IR, 3);
                    if (dtlsumm != "C")
                    {
                        HC.GetPrintHeader(IR, "doccd", "string", "c,5", "Doc;Code");
                        HC.GetPrintHeader(IR, "docdt", "string", "d,10:dd/mm/yy", ";Doc Date");
                        HC.GetPrintHeader(IR, "docno", "string", "c,18", ";Doc No");
                    }
                    else
                    {
                        HC.GetPrintHeader(IR, "docno", "string", "c,18", ";Doc No");
                        HC.GetPrintHeader(IR, "docdt", "string", "d,10:dd/mm/yy", ";Doc Date");
                    }
                    if (showpbill == true) HC.GetPrintHeader(IR, "prefdt", "string", "d,10:dd/mm/yy", "Party;Bill Date");
                    if (showpbill == true) HC.GetPrintHeader(IR, "prefno", "string", "c,16", "Party;Bill No");
                    if (itmdtl == true)
                    {
                        //HC.GetPrintHeader(IR, "slnm", "string", "c,35", "Party Name;Item Name");
                        HC.GetPrintHeader(IR, "slnm", "string", "c,40", "Party;Name");
                        HC.GetPrintHeader(IR, "slarea", "string", "c,10", "Area");
                        HC.GetPrintHeader(IR, "agslnm", "string", "c,40", "Agent;Name");
                        HC.GetPrintHeader(IR, "sagslnm", "string", "c,40", "Sub Agent;Name");
                        if (dtlsumm != "C" && VE.Checkbox1 == true) HC.GetPrintHeader(IR, "bltype", "string", "c,20", "Bill;Type");
                        HC.GetPrintHeader(IR, "itnm", "string", "c,35", "Item;Name");
                        if (VE.TEXTBOX1 == "Sales Cash Memo") HC.GetPrintHeader(IR, "mobile", "string", "c,12", "Mobile Number");
                        if (itemrem == true) HC.GetPrintHeader(IR, "itrem", "string", "c,20", ";Item Remarks");
                        HC.GetPrintHeader(IR, "gstno", "string", "c,15", "GST No.;Prod Code");
                        HC.GetPrintHeader(IR, "hsncode", "string", "c,8", "HSN/SAC");
                        HC.GetPrintHeader(IR, "uomcd", "string", "c,4", "Uom");
                        HC.GetPrintHeader(IR, "nos", "double", "n,5", "Nos");
                        HC.GetPrintHeader(IR, "qnty", "double", "n,12,3", "Qnty");
                        HC.GetPrintHeader(IR, "rate", "double", "n,10,2", "Rate");

                        if (cnt_blqnty.Count() > 0)
                        {
                            HC.GetPrintHeader(IR, "blqnty", "double", "n,12,3", "BL Qnty");
                        }
                    }
                    else
                    {
                        if (dtlsumm == "C") HC.GetPrintHeader(IR, "localcentral", "string", "c,5", "Local/;Central");
                        HC.GetPrintHeader(IR, "slnm", "string", "c,40", "Party; Name");
                        if (dtlsumm != "C") HC.GetPrintHeader(IR, "slarea", "string", "c,10", "Area");
                        if (dtlsumm != "C") HC.GetPrintHeader(IR, "agslnm", "string", "c,40", "Agent; Name");
                        if (dtlsumm != "C") HC.GetPrintHeader(IR, "sagslnm", "string", "c,40", "Sub Agent; Name");
                        if (dtlsumm == "D" && VE.TEXTBOX1 == "Proforma") HC.GetPrintHeader(IR, "docremoth", "string", "c,35", "Doc. Remarks");
                        if (dtlsumm != "C" && VE.Checkbox1 == true) HC.GetPrintHeader(IR, "bltype", "string", "c,20", "Bill;Type");
                        if (VE.TEXTBOX1 == "Sales Cash Memo") HC.GetPrintHeader(IR, "mobile", "string", "c,12", "Mobile Number");
                        if (dtlsumm != "C") HC.GetPrintHeader(IR, "gstno", "string", "c,15", "GST No.");
                        if (dtlsumm != "C") HC.GetPrintHeader(IR, "nos", "double", "n,5", "Nos");
                        if (dtlsumm != "C") HC.GetPrintHeader(IR, "qnty", "double", "n,12,3", "Qnty");
                    }
                    if (dtlsumm != "C") HC.GetPrintHeader(IR, "amt", "double", "n,12,2", "Basic;Amount");
                    if (SeparateAchead)
                    {
                        foreach (DataRow dr in amtDT.Rows)
                        {
                            HC.GetPrintHeader(IR, dr["itcd"].ToString(), "string", "c,10", dr["itnm"].ToString());
                        }
                    }
                    if (dtlsumm != "C") HC.GetPrintHeader(IR, "disc1", "double", "n,10,2", "Disc1;Amount");
                    if (dtlsumm != "C") HC.GetPrintHeader(IR, "disc2", "double", "n,10,2", "Disc2;Amount");
                    HC.GetPrintHeader(IR, "taxableval", "double", "n,12,2", "Taxable;Value");
                    if (dtlsumm == "C") HC.GetPrintHeader(IR, "gstper", "double", "n,5,2", ";GST %");
                    if (dtlsumm == "C") HC.GetPrintHeader(IR, "gstamt", "double", "n,15,2:#####,##,##0.00", "GST;Amount");
                    if (dtlsumm != "C") HC.GetPrintHeader(IR, "igstper", "double", "n,5,3", "IGST;%");
                    if (dtlsumm != "C") HC.GetPrintHeader(IR, "igstamt", "double", "n,10,2", "IGST;Amt");
                    if (dtlsumm != "C") HC.GetPrintHeader(IR, "cgstper", "double", "n,5,3", "CGST;%");
                    if (dtlsumm != "C") HC.GetPrintHeader(IR, "cgstamt", "double", "n,10,2", "CGST;Amt");
                    if (dtlsumm != "C") HC.GetPrintHeader(IR, "sgstper", "double", "n,5,3", "SGST;%");
                    if (dtlsumm != "C") HC.GetPrintHeader(IR, "sgstamt", "double", "n,10,2", "SGST;Amt");
                    HC.GetPrintHeader(IR, "tcsamt", "double", "n,10,2", "TCS;Amt");
                    HC.GetPrintHeader(IR, "roamt", "double", "n,6,2", "R/Off;Amt");
                    if (VE.TEXTBOX1 == "Sales Cash Memo") HC.GetPrintHeader(IR, "payamt", "double", "n,12,2", ";Payment");

                    HC.GetPrintHeader(IR, "blamt", "double", "n,12,2", ";Bill Value");
                    if (itmdtl == true && MSYSCNFG.MNTNBALE == "Y") HC.GetPrintHeader(IR, "baleno", "string", "c,15", "Bale. ;No");
                    if (itmdtl == true) HC.GetPrintHeader(IR, "pagenoslno", "string", "c,15", "Page No. /;Page Slno.");
                    HC.GetPrintHeader(IR, "ackno", "string", "c,15", "ACK. ;No");
                    HC.GetPrintHeader(IR, "ackdt", "string", "d,10:dd/mm/yy", "ACK. ;Date");
                    if (plistprint == true) HC.GetPrintHeader(IR, "prcdesc", "string", "c,10", "Price;List");
                    if (con_print == true) HC.GetPrintHeader(IR, "cslnm", "string", "c,40", "Consignee;Name");
                    if (transprint == true) HC.GetPrintHeader(IR, "trslnm", "string", "c,40", "Transporter;Name");
                    if (transprint == true) HC.GetPrintHeader(IR, "lrno", "string", "c,15", "Lr. ;No");
                    if (transprint == true) HC.GetPrintHeader(IR, "lrdt", "string", "d,10:dd/mm/yy", "Lr. ;Date");
                    if (transprint == true) HC.GetPrintHeader(IR, "GRWT", "double", "n,12,2", "Gross;Weight");
                    if (transprint == true) HC.GetPrintHeader(IR, "NTWT", "double", "n,12,2", "Net;Weight");
                    if (transprint == true) HC.GetPrintHeader(IR, "TRWT", "double", "n,12,2", "Tare;Weight");
                    if (orddetprint == true) HC.GetPrintHeader(IR, "ordno", "string", "c,50", "Order; No");
                    if (orddetprint == true) HC.GetPrintHeader(IR, "orddt", "string", "d,10:dd/mm/yy", "Order ;Date");
                    if (VE.Checkbox5 == true) HC.GetPrintHeader(IR, "saprem", "string", "c,20", "SAP;Details");
                    while (i <= maxR)
                    {
                        auto1 = tbl.Rows[i]["autono"].ToString();
                        istore = i;
                        bldtl = true;

                        while (auto1 == tbl.Rows[i]["autono"].ToString())
                        {
                            string itcd = tbl.Rows[i]["itcd"].ToString();
                            double bnos = 0; double bqnty = 0; double bbasamt = 0; double bdisc1 = 0; double bdisc2 = 0; double btaxable = 0, blqnty = 0;
                            double bigstamt = 0; double bcgstamt = 0; double bsgstamt = 0;
                            double bigstper = 0; double bcgstper = 0; double bsgstper = 0;
                            foreach (DataRow amtdr in amtDT.Rows) amtdr["AmtItcdRate"] = 0;


                            bigstper = tbl.Rows[i]["igstper"].retDbl();
                            bcgstper = tbl.Rows[i]["cgstper"].retDbl();
                            bsgstper = tbl.Rows[i]["sgstper"].retDbl();

                            while (auto1 == tbl.Rows[i]["autono"].ToString())
                            {
                                double mult = 1;
                                if (tbl.Rows[i]["cancel"].ToString() != "Y")
                                {
                                    if (tbl.Rows[i]["doctag"].retStr() == "SB" && tbl.Rows[i]["stkdrcr"].retStr() == "D")
                                    { mult = -1; }
                                    else if (tbl.Rows[i]["doctag"].retStr() == "PB" && tbl.Rows[i]["stkdrcr"].retStr() == "C")
                                    { mult = -1; }
                                    bnos = bnos + tbl.Rows[i]["nos"].retDbl();
                                    bqnty = bqnty + tbl.Rows[i]["qnty"].retDbl();
                                    if (SeparateAchead == true && tbl.Rows[i]["slno"].retInt() > 1000)
                                    {
                                        foreach (DataRow amtdr in amtDT.Rows)
                                        {
                                            if (itcd == amtdr["itcd"].retStr())
                                            {
                                                amtdr["AmtItcdRate"] = amtdr["AmtItcdRate"].retDbl() + tbl.Rows[i]["amt"].retDbl();
                                                amtdr["AmtItcdtotAmt"] = amtdr["AmtItcdtotAmt"].retDbl() + tbl.Rows[i]["amt"].retDbl();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        bbasamt = bbasamt + tbl.Rows[i]["amt"].retDbl() * mult;
                                    }

                                    bdisc1 = bdisc1 + tbl.Rows[i]["scmdiscamt"].retDbl() * mult;
                                    bdisc2 = bdisc2 + tbl.Rows[i]["tddiscamt"].retDbl() + tbl.Rows[i]["discamt"].retDbl() * mult;
                                    btaxable = btaxable + tbl.Rows[i]["TXBLVAL"].retDbl() * mult;
                                    bigstamt = bigstamt + tbl.Rows[i]["igstamt"].retDbl() * mult;
                                    bcgstamt = bcgstamt + tbl.Rows[i]["cgstamt"].retDbl() * mult;
                                    bsgstamt = bsgstamt + tbl.Rows[i]["sgstamt"].retDbl() * mult;
                                    blqnty = blqnty + tbl.Rows[i]["blqnty"].retDbl() * mult;
                                }
                                i = i + 1;
                                if (i > maxR) break;
                                if (tbl.Rows[i]["igstper"].retDbl() + tbl.Rows[i]["cgstper"].retDbl() + tbl.Rows[i]["sgstper"].retDbl() != bigstper + bcgstper + bsgstper) break;
                            }
                            i = i - 1;
                            DataRow dr = IR.NewRow();
                            if (bldtl == true)
                            {
                                string cancrem = "";
                                if (tbl.Rows[i]["cancel"].ToString() == "Y") cancrem = "  (CANCELLED)";
                                if (dtlsumm != "C")
                                {
                                    dr["doccd"] = tbl.Rows[i]["doccd"].ToString();
                                    dr["docdt"] = tbl.Rows[i]["docdt"] == DBNull.Value ? "" : tbl.Rows[i]["docdt"].ToString().Substring(0, 10).ToString();
                                    dr["docno"] = tbl.Rows[i]["docno"].ToString() + cancrem;
                                }
                                else {
                                    dr["docno"] = tbl.Rows[i]["docno"].ToString() + cancrem;
                                    dr["docdt"] = tbl.Rows[i]["docdt"] == DBNull.Value ? "" : tbl.Rows[i]["docdt"].ToString().Substring(0, 10).ToString();
                                }
                                string locacent = "Local";
                                if (bigstamt != 0) locacent = "Central";
                                if (dtlsumm == "C") dr["localcentral"] = locacent;
                                if (VE.TEXTBOX1 != "Sales Cash Memo")
                                {
                                    dr["slnm"] = tbl.Rows[i]["slnm"].ToString();
                                    if (dtlsumm != "C") dr["slarea"] = tbl.Rows[i]["slarea"].ToString();
                                    if (dtlsumm != "C") dr["agslnm"] = tbl.Rows[i]["agslnm"].ToString();
                                    if (dtlsumm != "C") dr["sagslnm"] = tbl.Rows[i]["sagslnm"].ToString();
                                    if (dtlsumm != "C" && VE.Checkbox1 == true) dr["bltype"] = tbl.Rows[i]["bltype"].retStr();
                                }
                                else
                                {
                                    dr["slnm"] = tbl.Rows[i]["nm"].ToString();
                                    dr["mobile"] = tbl.Rows[i]["mobile"].ToString();
                                }
                                if (dtlsumm == "D" && VE.TEXTBOX1 == "Proforma") dr["docremoth"] = tbl.Rows[i]["docrem"].ToString();
                                if (dtlsumm != "C") dr["gstno"] = tbl.Rows[i]["gstno"].ToString();
                                if (showpbill == true) dr["prefno"] = tbl.Rows[i]["prefno"].ToString();
                                if (VE.Checkbox5 == true) dr["saprem"] = (tbl.Rows[i]["sapblno"].ToString() == "" ? "" : "BL# " + tbl.Rows[i]["sapblno"].ToString());
                                if (showpbill == true) dr["prefdt"] = tbl.Rows[i]["prefdt"] == DBNull.Value ? "" : tbl.Rows[i]["prefdt"].ToString().Substring(0, 10).ToString();
                                if (con_print == true)
                                {
                                    dr["cslnm"] = tbl.Rows[i]["cslnm"].ToString();
                                }
                                if (transprint == true)
                                {
                                    dr["trslnm"] = tbl.Rows[i]["trslnm"].ToString();
                                    dr["lrno"] = tbl.Rows[i]["lrno"].retStr();
                                    dr["lrdt"] = tbl.Rows[i]["lrdt"].retDateStr();
                                    dr["GRWT"] = tbl.Rows[i]["GRWT"].retDbl();
                                    dr["NTWT"] = tbl.Rows[i]["NTWT"].retDbl();
                                    dr["TRWT"] = tbl.Rows[i]["TRWT"].retDbl();
                                }
                                if (orddetprint == true)
                                {
                                    dr["ordno"] = tbl.Rows[i]["ordrefno"].ToString();
                                    dr["orddt"] = tbl.Rows[i]["ordrefdt"].ToString();
                                }
                                if (tbl.Rows[i]["cancel"].ToString() != "Y")
                                {
                                    dr["tcsamt"] = tbl.Rows[i]["tcsamt"].retDbl();
                                    dr["roamt"] = tbl.Rows[i]["roamt"].retDbl();
                                    var a = (from DataRow dr1 in paymentDt.Rows
                                             where auto1 == dr1["autono"].retStr()
                                             select new { payamt = dr1["payamt"].retDbl() }).ToList();
                                    if (VE.TEXTBOX1 == "Sales Cash Memo") dr["payamt"] = a.Sum(b => b.payamt).retDbl();
                                    dr["blamt"] = tbl.Rows[i]["blamt"].retDbl();
                                    dr["ackno"] = tbl.Rows[i]["ackno"].retStr();
                                    dr["ackdt"] = tbl.Rows[i]["ackdt"].retDateStr();
                                    ttcsamt = ttcsamt + tbl.Rows[i]["tcsamt"].retDbl();
                                    troamt = troamt + tbl.Rows[i]["roamt"].retDbl();
                                    tblamt = tblamt + tbl.Rows[i]["blamt"].retDbl();
                                    if (VE.TEXTBOX1 == "Sales Cash Memo") tpayamt = tpayamt + a.Sum(b => b.payamt).retDbl();
                                    //tpayamt = tpayamt + tbl.Rows[i]["payamt"].retDbl();
                                }

                                bldtl = false;
                            }
                            if (tbl.Rows[i]["cancel"].ToString() != "Y")
                            {
                                if (dtlsumm != "C") dr["nos"] = bnos;
                                if (dtlsumm != "C") dr["qnty"] = bqnty;
                                if (SeparateAchead == true)
                                {
                                    foreach (DataRow amtdr in amtDT.Rows)
                                    {
                                        dr[amtdr["itcd"].ToString()] = amtdr["AmtItcdRate"].retDbl();
                                    }
                                }
                                if (dtlsumm != "C") dr["amt"] = bbasamt;
                                if (dtlsumm != "C") dr["disc1"] = bdisc1;
                                if (dtlsumm != "C") dr["disc2"] = bdisc2;
                                dr["taxableval"] = btaxable;
                                if (dtlsumm == "C") dr["gstper"] = tbl.Rows[i]["gstper"].retDbl();
                                if (dtlsumm == "C") dr["gstamt"] = bigstamt + bcgstamt + bsgstamt;    /* tbl.Rows[i]["gstamt"].retDbl();*/
                                if (dtlsumm != "C") dr["igstper"] = bigstper;
                                if (dtlsumm != "C") dr["cgstper"] = bcgstper;
                                if (dtlsumm != "C") dr["sgstper"] = bsgstper;
                                if (dtlsumm != "C") dr["igstamt"] = bigstamt;
                                if (dtlsumm != "C") dr["cgstamt"] = bcgstamt;
                                if (dtlsumm != "C") dr["sgstamt"] = bsgstamt;

                                tnos = tnos + bnos;
                                tqnty = tqnty + bqnty;
                                tbasamt = tbasamt + bbasamt;
                                tdisc1 = tdisc1 + bdisc1;
                                tdisc2 = tdisc2 + bdisc2;
                                ttaxable = ttaxable + btaxable;
                                if (dtlsumm == "C") tgstamt = tgstamt + dr["gstamt"].retDbl();
                                tigstamt = tigstamt + bigstamt;
                                tcgstamt = tcgstamt + bcgstamt;
                                tsgstamt = tsgstamt + bsgstamt;
                                tbqnty = tbqnty + blqnty;
                            }

                            if (itmdtl == true) dr["Flag"] = "font-weight:bold";
                            IR.Rows.Add(dr);
                            i = i + 1;
                            if (i > maxR) break;
                        }

                        if (itmdtl == true)
                        {
                            string slash = "";
                            i = istore;
                            int ino = 0;
                            while (auto1 == tbl.Rows[i]["autono"].ToString())
                            {
                                ino = ino + 1;
                                DataRow dr = IR.NewRow();
                                //dr["slnm"] = tbl.Rows[i]["itstyle"].ToString();
                                dr["itnm"] = tbl.Rows[i]["itstyle"].ToString();
                                if (dtlsumm != "C" && VE.Checkbox1 == true) dr["bltype"] = tbl.Rows[i]["bltype"].retStr();
                                if (VE.TEXTBOX1 == "Sales Cash Memo") dr["mobile"] = tbl.Rows[i]["mobile"].ToString();
                                if (itemrem == true && itmdtl == true) dr["itrem"] = tbl.Rows[i]["itrem"].ToString();
                                dr["hsncode"] = tbl.Rows[i]["hsncode"].ToString();
                                dr["uomcd"] = tbl.Rows[i]["uomcd"].ToString();
                                if (cnt_blqnty.Count() > 0)
                                {
                                    dr["blqnty"] = tbl.Rows[i]["blqnty"].retDbl();
                                }
                                if (tbl.Rows[i]["cancel"].ToString() != "Y")
                                {
                                    dr["nos"] = tbl.Rows[i]["nos"].retDbl();
                                    dr["qnty"] = tbl.Rows[i]["qnty"] == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["qnty"]);
                                    dr["rate"] = tbl.Rows[i]["rate"].retDbl();
                                    dr["amt"] = tbl.Rows[i]["amt"] == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["amt"]);
                                    dr["disc1"] = tbl.Rows[i]["scmdiscamt"].retDbl();
                                    dr["disc2"] = tbl.Rows[i]["discamt"].retDbl();
                                    dsc1 = tbl.Rows[i]["tddiscamt"] == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["tddiscamt"]);
                                    dsc2 = tbl.Rows[i]["discamt"] == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["discamt"]);
                                    dr["taxableval"] = tbl.Rows[i]["TXBLVAL"].retDbl();      /*Convert.ToDouble(tbl.Rows[i]["amt"]) - dsc1 - dsc2;*/
                                    dr["igstper"] = tbl.Rows[i]["igstper"].retDbl();
                                    dr["igstamt"] = tbl.Rows[i]["igstamt"].retDbl();
                                    dr["cgstper"] = tbl.Rows[i]["cgstper"].retDbl();
                                    dr["cgstamt"] = tbl.Rows[i]["cgstamt"].retDbl();
                                    dr["sgstper"] = tbl.Rows[i]["sgstper"].retDbl();
                                    dr["sgstamt"] = tbl.Rows[i]["sgstamt"].retDbl();
                                    //dr["blamt"] = tbl.Rows[i]["NETAMT"].retDbl();
                                    if (MSYSCNFG.MNTNBALE == "Y") dr["baleno"] = tbl.Rows[i]["baleno"].retStr();
                                    dr["pagenoslno"] = (tbl.Rows[i]["pageno"].retInt() == 0 && tbl.Rows[i]["pageslno"].retInt() == 0) ? "" : tbl.Rows[i]["pageno"].retInt() + "/" + tbl.Rows[i]["pageslno"].retInt();
                                    dr["ackno"] = tbl.Rows[i]["ackno"].retStr();
                                    dr["ackdt"] = tbl.Rows[i]["ackdt"].retDateStr();

                                }
                                if (dr["celldesign"].ToString() != "") dr["celldesign"] = dr["celldesign"] + "^";
                                dr["celldesign"] = dr["celldesign"] + "qnty=~n,12," + tbl.Rows[i]["decimals"].retInt();
                                i += 1;

                                IR.Rows.Add(dr);
                                if (i > maxR) break;
                            }
                        }
                    }


                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["dammy"] = "";
                    IR.Rows[rNo]["slnm"] = "Grand Totals";
                    if (itmdtl == true) { IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;"; }
                    else { IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 3px solid;"; }
                    if (itmdtl == false && dtlsumm != "C") IR.Rows[rNo]["nos"] = tnos;
                    if (itmdtl == false && dtlsumm != "C") IR.Rows[rNo]["qnty"] = tqnty;
                    if (dtlsumm != "C") IR.Rows[rNo]["amt"] = tbasamt;
                    if (dtlsumm != "C") IR.Rows[rNo]["disc1"] = tdisc1;
                    if (dtlsumm != "C") IR.Rows[rNo]["disc2"] = tdisc2;
                    IR.Rows[rNo]["taxableval"] = ttaxable;
                    if (dtlsumm == "C") IR.Rows[rNo]["gstamt"] = tgstamt;
                    if (dtlsumm != "C") IR.Rows[rNo]["igstamt"] = tigstamt;
                    if (dtlsumm != "C") IR.Rows[rNo]["cgstamt"] = tcgstamt;
                    if (dtlsumm != "C") IR.Rows[rNo]["sgstamt"] = tsgstamt;
                    IR.Rows[rNo]["tcsamt"] = ttcsamt;
                    IR.Rows[rNo]["roamt"] = troamt;
                    IR.Rows[rNo]["blamt"] = tblamt;
                    if (VE.TEXTBOX1 == "Sales Cash Memo") IR.Rows[rNo]["payamt"] = tpayamt;
                    if (itmdtl == true && tbqnty != 0)
                    {
                        IR.Rows[rNo]["blqnty"] = tbqnty;
                    }
                    if (itmdtl == true)
                    {
                        var grptbl = IR.AsEnumerable().Where(g => g.Field<string>("uomcd").retStr() != "")
                                        .GroupBy(g => g.Field<string>("uomcd"))
                                        .Select(g =>
                                        {
                                            var row = IR.NewRow();
                                            row["uomcd"] = g.Key;
                                            row["qnty"] = g.Sum(r => r.Field<double>("qnty").retDbl());
                                            row["nos"] = g.Sum(r => r.Field<double>("nos").retDbl());
                                            return row;
                                        }).CopyToDataTable();
                        // int j = 0;
                        for (int j = 0; j <= grptbl.Rows.Count - 1; j++)
                        {
                            if (grptbl.Rows[j]["qnty"].retDbl() != 0)
                            {
                                if (j == 0) { }
                                else { IR.Rows.Add(""); rNo = IR.Rows.Count - 1; }
                                IR.Rows[rNo]["uomcd"] = grptbl.Rows[j]["uomcd"];
                                IR.Rows[rNo]["qnty"] = grptbl.Rows[j]["qnty"];
                                IR.Rows[rNo]["nos"] = grptbl.Rows[j]["nos"];

                            }
                        }
                    }
                    if (itmdtl == true) { IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;"; }

                    // Create Blank line
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["dammy"] = " ";
                    IR.Rows[rNo]["flag"] = " height:14px; ";

                    #endregion
                    pghdr1 = regdsp + " from " + fdt + " to " + tdt;
                    string repname = regdsp + " Register";
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
        public dynamic ShowAllColumn(ReportViewinHtml VE, string TAG = "")
        {
            INI Handel_Ini = new INI();
            string columnnm = "";
            string SectionNm = VE.TEXTBOX6.retStr();
            var SectionName = Handel_Ini.GetSectionNames(Server.MapPath("~/Ipsmart.ini")).ToArray().Where(a => a.Contains("Rep_Reg_")).ToList();
            if (TAG == "ExcelHeader" && SectionName != null && SectionName.Count() != 0)
            {
                foreach (var v in SectionName)
                {
                    string ActiveSection = Handel_Ini.IniReadValue(v, "ACTIVE", Server.MapPath("~/Ipsmart.ini"));
                    if (ActiveSection != "")
                    {
                        SectionNm = v;
                        break;
                    }
                }
            }
            List<ColumnName> ColumnNamelist = new List<ColumnName>();
            ColumnName ColumnNameObj = new ColumnName();

            string BRS = Handel_Ini.IniReadValue(SectionNm, "AUTONO", Server.MapPath("~/Ipsmart.ini"));
            if (BRS == "" && TAG != "ExcelHeader")
            {
                #region if any column name not saved
                ColumnNameObj.AUTONO = true;
                ColumnNameObj.DOCCD = true;
                ColumnNameObj.DOCNO = true;
                ColumnNameObj.CANCEL = true;
                ColumnNameObj.DOCDT = true;
                ColumnNameObj.AGSLCD = true;
                ColumnNameObj.PREFNO = true;
                ColumnNameObj.PREFDT = true;
                ColumnNameObj.SLCD = true;
                ColumnNameObj.SLNM = true;
                ColumnNameObj.SLAREA = true;
                ColumnNameObj.AGSLNM = true;
                ColumnNameObj.NM = true;
                ColumnNameObj.MOBILE = true;
                ColumnNameObj.GSTNO = true;
                ColumnNameObj.DISTRICT = true;
                ColumnNameObj.ROAMT = true;
                ColumnNameObj.TCSAMT = true;
                ColumnNameObj.BLAMT = true;
                ColumnNameObj.SLNO = true;
                ColumnNameObj.ITCD = true;
                ColumnNameObj.ITNM = true;
                ColumnNameObj.ITSTYLE = true;
                ColumnNameObj.ITREM = true;
                ColumnNameObj.HSNCODE = true;
                ColumnNameObj.UOMCD = true;
                ColumnNameObj.UOMNM = true;
                ColumnNameObj.DECIMALS = true;
                ColumnNameObj.NOS = true;
                ColumnNameObj.QNTY = true;
                ColumnNameObj.RATE = true;
                ColumnNameObj.AMT = true;
                ColumnNameObj.SCMDISCAMT = true;
                ColumnNameObj.TDDISCAMT = true;
                ColumnNameObj.DISCAMT = true;
                ColumnNameObj.TXBLVAL = true;
                ColumnNameObj.CONSLCD = true;
                ColumnNameObj.CSLNM = true;
                ColumnNameObj.CGSTNO = true;
                ColumnNameObj.CDISTRICT = true;
                ColumnNameObj.TRSLNM = true;
                ColumnNameObj.LRNO = true;
                ColumnNameObj.LRDT = true;
                ColumnNameObj.GRWT = true;
                ColumnNameObj.TRWT = true;
                ColumnNameObj.NTWT = true;
                ColumnNameObj.ORDREFNO = true;
                ColumnNameObj.ORDREFDT = true;
                ColumnNameObj.IGSTPER = true;
                ColumnNameObj.IGSTAMT = true;
                ColumnNameObj.CGSTPER = true;
                ColumnNameObj.CGSTAMT = true;
                ColumnNameObj.SGSTAMT = true;
                ColumnNameObj.CESSPER = true;
                ColumnNameObj.CESSAMT = true;
                ColumnNameObj.BLQNTY = true;
                ColumnNameObj.NETAMT = true;
                ColumnNameObj.SGSTPER = true;
                ColumnNameObj.GSTPER = true;
                ColumnNameObj.GSTAMT = true;
                ColumnNameObj.ACKNO = true;
                ColumnNameObj.ACKDT = true;
                ColumnNameObj.PAGENO = true;
                ColumnNameObj.PAGESLNO = true;
                ColumnNameObj.BALENO = true;
                ColumnNameObj.DOCREM = true;
                ColumnNameObj.BLTYPE = true;

                INI Handel_ini = new INI();
                Handel_ini.IniWriteValue(SectionNm, "AUTONO", ColumnNameObj.AUTONO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "DOCCD", ColumnNameObj.DOCCD.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "DOCNO", ColumnNameObj.DOCNO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "CANCEL", ColumnNameObj.CANCEL.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "DOCDT", ColumnNameObj.DOCDT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "AGSLCD", ColumnNameObj.AGSLCD.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "PREFNO", ColumnNameObj.PREFNO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "PREFDT", ColumnNameObj.PREFDT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "SLCD", ColumnNameObj.SLCD.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "SLNM", ColumnNameObj.SLNM.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "SLAREA", ColumnNameObj.SLAREA.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "AGSLNM", ColumnNameObj.AGSLNM.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "NM", ColumnNameObj.NM.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "MOBILE", ColumnNameObj.MOBILE.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "GSTNO", ColumnNameObj.GSTNO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "DISTRICT", ColumnNameObj.DISTRICT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "ROAMT", ColumnNameObj.ROAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "TCSAMT", ColumnNameObj.TCSAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "BLAMT", ColumnNameObj.BLAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "SLNO", ColumnNameObj.SLNO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "ITCD", ColumnNameObj.ITCD.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "ITNM", ColumnNameObj.ITNM.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "ITSTYLE", ColumnNameObj.ITSTYLE.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "ITREM", ColumnNameObj.ITREM.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "HSNCODE", ColumnNameObj.HSNCODE.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "UOMCD", ColumnNameObj.UOMCD.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "UOMNM", ColumnNameObj.UOMNM.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "DECIMALS", ColumnNameObj.DECIMALS.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "NOS", ColumnNameObj.NOS.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "QNTY", ColumnNameObj.QNTY.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "RATE", ColumnNameObj.RATE.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "AMT", ColumnNameObj.AMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "SCMDISCAMT", ColumnNameObj.SCMDISCAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "TDDISCAMT", ColumnNameObj.TDDISCAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "DISCAMT", ColumnNameObj.DISCAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "TXBLVAL", ColumnNameObj.TXBLVAL.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "CONSLCD", ColumnNameObj.CONSLCD.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "CSLNM", ColumnNameObj.CSLNM.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "CGSTNO", ColumnNameObj.CGSTNO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "CDISTRICT", ColumnNameObj.CDISTRICT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "TRSLNM", ColumnNameObj.TRSLNM.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "LRNO", ColumnNameObj.LRNO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "LRDT", ColumnNameObj.LRDT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "GRWT", ColumnNameObj.GRWT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "TRWT", ColumnNameObj.TRWT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "NTWT", ColumnNameObj.NTWT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "ORDREFNO", ColumnNameObj.ORDREFNO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "ORDREFDT", ColumnNameObj.ORDREFDT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "IGSTPER", ColumnNameObj.IGSTPER.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "IGSTAMT", ColumnNameObj.IGSTAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "CGSTPER", ColumnNameObj.CGSTPER.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "CGSTAMT", ColumnNameObj.CGSTAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "SGSTAMT", ColumnNameObj.SGSTAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "CESSPER", ColumnNameObj.CESSPER.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "CESSAMT", ColumnNameObj.CESSAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "BLQNTY", ColumnNameObj.BLQNTY.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "NETAMT", ColumnNameObj.NETAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "SGSTPER", ColumnNameObj.SGSTPER.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "GSTPER", ColumnNameObj.GSTPER.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "GSTAMT", ColumnNameObj.GSTAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "ACKNO", ColumnNameObj.ACKNO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "ACKDT", ColumnNameObj.ACKDT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "PAGENO", ColumnNameObj.PAGENO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "PAGESLNO", ColumnNameObj.PAGESLNO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "BALENO", ColumnNameObj.BALENO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "DOCREM", ColumnNameObj.DOCREM.ToString(), Server.MapPath("~/Ipsmart.ini"));
                Handel_ini.IniWriteValue(SectionNm, "BLTYPE", ColumnNameObj.BLTYPE.ToString(), Server.MapPath("~/Ipsmart.ini"));
                ColumnNamelist.Add(ColumnNameObj);
                VE.ColumnName = ColumnNamelist;
                #endregion
            }
            else
            {
                #region 
                bool AUTONO = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "AUTONO", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.AUTONO = AUTONO;
                columnnm += AUTONO == true ? "AUTONO," : "";

                bool DOCCD = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "DOCCD", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.DOCCD = DOCCD;
                columnnm += DOCCD == true ? "DOCCD," : "";

                bool DOCNO = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "DOCNO", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.DOCNO = DOCNO;
                columnnm += DOCNO == true ? "DOCNO," : "";

                bool CANCEL = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "CANCEL", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.CANCEL = CANCEL;
                columnnm += CANCEL == true ? "CANCEL," : "";

                bool DOCDT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "DOCDT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.DOCDT = DOCDT;
                columnnm += DOCDT == true ? "DOCDT," : "";

                bool AGSLCD = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "AGSLCD", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.AGSLCD = AGSLCD;
                columnnm += AGSLCD == true ? "AGSLCD," : "";

                bool PREFNO = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "PREFNO", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.PREFNO = PREFNO;
                columnnm += PREFNO == true ? "PREFNO," : "";

                bool PREFDT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "PREFDT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.PREFDT = PREFDT;
                columnnm += PREFDT == true ? "PREFDT," : "";

                bool SLCD = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "SLCD", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.SLCD = SLCD;
                columnnm += SLCD == true ? "SLCD," : "";

                bool SLNM = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "SLNM", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.SLNM = SLNM;
                columnnm += SLNM == true ? "SLNM," : "";

                bool SLAREA = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "SLAREA", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.SLAREA = SLAREA;
                columnnm += SLAREA == true ? "SLAREA," : "";

                bool AGSLNM = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "AGSLNM", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.AGSLNM = AGSLNM;
                columnnm += AGSLNM == true ? "AGSLNM," : "";

                bool NM = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "NM", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.NM = NM;
                columnnm += NM == true ? "NM," : "";

                bool MOBILE = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "MOBILE", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.MOBILE = MOBILE;
                columnnm += MOBILE == true ? "MOBILE," : "";

                bool GSTNO = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "GSTNO", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.GSTNO = GSTNO;
                columnnm += GSTNO == true ? "GSTNO," : "";

                bool DISTRICT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "DISTRICT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.DISTRICT = DISTRICT;
                columnnm += DISTRICT == true ? "DISTRICT," : "";

                bool ROAMT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "ROAMT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.ROAMT = ROAMT;
                columnnm += ROAMT == true ? "ROAMT," : "";

                bool TCSAMT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "TCSAMT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.TCSAMT = TCSAMT;
                columnnm += TCSAMT == true ? "TCSAMT," : "";

                bool BLAMT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "BLAMT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.BLAMT = BLAMT;
                columnnm += BLAMT == true ? "BLAMT," : "";

                bool SLNO = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "SLNO", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.SLNO = SLNO;
                columnnm += SLNO == true ? "SLNO," : "";

                bool ITCD = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "ITCD", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.ITCD = ITCD;
                columnnm += ITCD == true ? "ITCD," : "";

                bool ITNM = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "ITNM", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.ITNM = ITNM;
                columnnm += ITNM == true ? "ITNM," : "";

                bool ITSTYLE = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "ITSTYLE", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.ITSTYLE = ITSTYLE;
                columnnm += ITSTYLE == true ? "ITSTYLE," : "";

                bool ITREM = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "ITREM", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.ITREM = ITREM;
                columnnm += ITREM == true ? "ITREM," : "";

                bool HSNCODE = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "HSNCODE", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.HSNCODE = HSNCODE;
                columnnm += HSNCODE == true ? "HSNCODE," : "";

                bool UOMCD = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "UOMCD", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.UOMCD = UOMCD;
                columnnm += UOMCD == true ? "UOMCD," : "";

                bool UOMNM = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "UOMNM", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.UOMNM = UOMNM;
                columnnm += UOMNM == true ? "UOMNM," : "";

                bool DECIMALS = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "DECIMALS", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.DECIMALS = DECIMALS;
                columnnm += DECIMALS == true ? "DECIMALS," : "";

                bool NOS = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "NOS", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.NOS = NOS;
                columnnm += NOS == true ? "NOS," : "";

                bool QNTY = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "QNTY", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.QNTY = QNTY;
                columnnm += QNTY == true ? "QNTY," : "";

                bool RATE = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "RATE", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.RATE = RATE;
                columnnm += RATE == true ? "RATE," : "";

                bool AMT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "AMT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.AMT = AMT;
                columnnm += AMT == true ? "AMT," : "";

                bool SCMDISCAMT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "SCMDISCAMT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.SCMDISCAMT = SCMDISCAMT;
                columnnm += SCMDISCAMT == true ? "SCMDISCAMT," : "";

                bool TDDISCAMT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "TDDISCAMT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.TDDISCAMT = TDDISCAMT;
                columnnm += TDDISCAMT == true ? "TDDISCAMT," : "";

                bool DISCAMT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "DISCAMT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.DISCAMT = DISCAMT;
                columnnm += DISCAMT == true ? "DISCAMT," : "";

                bool TXBLVAL = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "TXBLVAL", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.TXBLVAL = TXBLVAL;
                columnnm += TXBLVAL == true ? "TXBLVAL," : "";

                bool CONSLCD = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "CONSLCD", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.CONSLCD = CONSLCD;
                columnnm += CONSLCD == true ? "CONSLCD," : "";

                bool CSLNM = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "CSLNM", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.CSLNM = CSLNM;
                columnnm += CSLNM == true ? "CSLNM," : "";

                bool CGSTNO = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "CGSTNO", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.CGSTNO = CGSTNO;
                columnnm += CGSTNO == true ? "CGSTNO," : "";

                bool CDISTRICT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "CDISTRICT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.CDISTRICT = CDISTRICT;
                columnnm += CDISTRICT == true ? "CDISTRICT," : "";

                bool TRSLNM = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "TRSLNM", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.TRSLNM = TRSLNM;
                columnnm += TRSLNM == true ? "TRSLNM," : "";

                bool LRNO = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "LRNO", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.LRNO = LRNO;
                columnnm += LRNO == true ? "LRNO," : "";

                bool LRDT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "LRDT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.LRDT = LRDT;
                columnnm += LRDT == true ? "LRDT," : "";

                bool GRWT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "GRWT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.GRWT = GRWT;
                columnnm += GRWT == true ? "GRWT," : "";

                bool TRWT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "TRWT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.TRWT = TRWT;
                columnnm += TRWT == true ? "TRWT," : "";

                bool NTWT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "NTWT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.NTWT = NTWT;
                columnnm += NTWT == true ? "NTWT," : "";

                bool ORDREFNO = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "ORDREFNO", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.ORDREFNO = ORDREFNO;
                columnnm += ORDREFNO == true ? "ORDREFNO," : "";

                bool ORDREFDT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "ORDREFDT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.ORDREFDT = ORDREFDT;
                columnnm += ORDREFDT == true ? "ORDREFDT," : "";

                bool IGSTPER = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "IGSTPER", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.IGSTPER = IGSTPER;
                columnnm += IGSTPER == true ? "IGSTPER," : "";

                bool IGSTAMT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "IGSTAMT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.IGSTAMT = IGSTAMT;
                columnnm += IGSTAMT == true ? "IGSTAMT," : "";

                bool CGSTPER = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "CGSTPER", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.CGSTPER = CGSTPER;
                columnnm += CGSTPER == true ? "CGSTPER," : "";

                bool CGSTAMT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "CGSTAMT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.CGSTAMT = CGSTAMT;
                columnnm += CGSTAMT == true ? "CGSTAMT," : "";

                bool SGSTAMT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "SGSTAMT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.SGSTAMT = SGSTAMT;
                columnnm += SGSTAMT == true ? "SGSTAMT," : "";

                bool CESSPER = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "CESSPER", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.CESSPER = CESSPER;
                columnnm += CESSPER == true ? "CESSPER," : "";

                bool CESSAMT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "CESSAMT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.CESSAMT = CESSAMT;
                columnnm += CESSAMT == true ? "CESSAMT," : "";

                bool BLQNTY = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "BLQNTY", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.BLQNTY = BLQNTY;
                columnnm += BLQNTY == true ? "BLQNTY," : "";

                bool NETAMT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "NETAMT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.NETAMT = NETAMT;
                columnnm += NETAMT == true ? "NETAMT," : "";

                bool SGSTPER = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "SGSTPER", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.SGSTPER = SGSTPER;
                columnnm += SGSTPER == true ? "SGSTPER," : "";

                bool GSTPER = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "GSTPER", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.GSTPER = GSTPER;
                columnnm += GSTPER == true ? "GSTPER," : "";

                bool GSTAMT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "GSTAMT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.GSTAMT = GSTAMT;
                columnnm += GSTAMT == true ? "GSTAMT," : "";

                bool ACKNO = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "ACKNO", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.ACKNO = ACKNO;
                columnnm += ACKNO == true ? "ACKNO," : "";

                bool ACKDT = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "ACKDT", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.ACKDT = ACKDT;
                columnnm += ACKDT == true ? "ACKDT," : "";

                bool PAGENO = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "PAGENO", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.PAGENO = PAGENO;
                columnnm += PAGENO == true ? "PAGENO," : "";

                bool PAGESLNO = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "PAGESLNO", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.PAGESLNO = PAGESLNO;
                columnnm += PAGESLNO == true ? "PAGESLNO," : "";

                bool BALENO = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "BALENO", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.BALENO = BALENO;
                columnnm += BALENO == true ? "BALENO," : "";

                bool DOCREM = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "DOCREM", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.DOCREM = DOCREM;
                columnnm += DOCREM == true ? "DOCREM," : "";

                bool BLTYPE = Convert.ToBoolean(Handel_Ini.IniReadValue(SectionNm, "BLTYPE", Server.MapPath("~/Ipsmart.ini")));
                ColumnNameObj.BLTYPE = BLTYPE;
                columnnm += BLTYPE == true ? "BLTYPE," : "";
                #endregion
                ColumnNamelist.Add(ColumnNameObj);
                VE.ColumnName = ColumnNamelist;
            }
            if (TAG == "ExcelHeader")
            {
                if (columnnm.retStr() != "")
                {
                    columnnm = columnnm.TrimEnd(',');
                }
                return columnnm;
            }
            else if (TAG == "ColumnName")
            {
                string[] SectionName1 = Handel_Ini.GetSectionNames(Server.MapPath("~/Ipsmart.ini"));
                VE.DropDown_list2 = (from i in SectionName1
                                     where i.Contains("Rep_Reg_")
                                     select new DropDown_list2() { value = i, text = i }).Distinct().OrderBy(s => s.text).ToList();
                ModelState.Clear();
                return PartialView("_Rep_Reg_Column", VE);
            }
            else return null;
        }
        public ActionResult FilterColumn(ReportViewinHtml VE, FormCollection FC)
        {//call from View Page

            List<ColumnName> ColumnNamelist = new List<ColumnName>();
            ColumnName colnm = new ColumnName();
            colnm.AUTONO = VE.ColumnName[0].AUTONO;
            colnm.DOCCD = VE.ColumnName[0].DOCCD;
            colnm.DOCNO = VE.ColumnName[0].DOCNO;
            colnm.CANCEL = VE.ColumnName[0].CANCEL;
            colnm.DOCDT = VE.ColumnName[0].DOCDT;
            colnm.AGSLCD = VE.ColumnName[0].AGSLCD;
            colnm.PREFNO = VE.ColumnName[0].PREFNO;
            colnm.PREFDT = VE.ColumnName[0].PREFDT;
            colnm.SLCD = VE.ColumnName[0].SLCD;
            colnm.SLNM = VE.ColumnName[0].SLNM;
            colnm.SLAREA = VE.ColumnName[0].SLAREA;
            colnm.AGSLNM = VE.ColumnName[0].AGSLNM;
            colnm.NM = VE.ColumnName[0].NM;
            colnm.MOBILE = VE.ColumnName[0].MOBILE;
            colnm.GSTNO = VE.ColumnName[0].GSTNO;
            colnm.DISTRICT = VE.ColumnName[0].DISTRICT;
            colnm.ROAMT = VE.ColumnName[0].ROAMT;
            colnm.TCSAMT = VE.ColumnName[0].TCSAMT;
            colnm.BLAMT = VE.ColumnName[0].BLAMT;
            colnm.SLNO = VE.ColumnName[0].SLNO;
            colnm.ITCD = VE.ColumnName[0].ITCD;
            colnm.ITNM = VE.ColumnName[0].ITNM;
            colnm.ITSTYLE = VE.ColumnName[0].ITSTYLE;
            colnm.ITREM = VE.ColumnName[0].ITREM;
            colnm.HSNCODE = VE.ColumnName[0].HSNCODE;
            colnm.UOMCD = VE.ColumnName[0].UOMCD;
            colnm.UOMNM = VE.ColumnName[0].UOMNM;
            colnm.DECIMALS = VE.ColumnName[0].DECIMALS;
            colnm.NOS = VE.ColumnName[0].NOS;
            colnm.QNTY = VE.ColumnName[0].QNTY;
            colnm.RATE = VE.ColumnName[0].RATE;
            colnm.AMT = VE.ColumnName[0].AMT;
            colnm.SCMDISCAMT = VE.ColumnName[0].SCMDISCAMT;
            colnm.TDDISCAMT = VE.ColumnName[0].TDDISCAMT;
            colnm.DISCAMT = VE.ColumnName[0].DISCAMT;
            colnm.TXBLVAL = VE.ColumnName[0].TXBLVAL;
            colnm.CONSLCD = VE.ColumnName[0].CONSLCD;
            colnm.CSLNM = VE.ColumnName[0].CSLNM;
            colnm.CGSTNO = VE.ColumnName[0].CGSTNO;
            colnm.CDISTRICT = VE.ColumnName[0].CDISTRICT;
            colnm.TRSLNM = VE.ColumnName[0].TRSLNM;
            colnm.LRNO = VE.ColumnName[0].LRNO;
            colnm.LRDT = VE.ColumnName[0].LRDT;
            colnm.GRWT = VE.ColumnName[0].GRWT;
            colnm.TRWT = VE.ColumnName[0].TRWT;
            colnm.NTWT = VE.ColumnName[0].NTWT;
            colnm.ORDREFNO = VE.ColumnName[0].ORDREFNO;
            colnm.ORDREFDT = VE.ColumnName[0].ORDREFDT;
            colnm.IGSTPER = VE.ColumnName[0].IGSTPER;
            colnm.IGSTAMT = VE.ColumnName[0].IGSTAMT;
            colnm.CGSTPER = VE.ColumnName[0].CGSTPER;
            colnm.CGSTAMT = VE.ColumnName[0].CGSTAMT;
            colnm.SGSTAMT = VE.ColumnName[0].SGSTAMT;
            colnm.CESSPER = VE.ColumnName[0].CESSPER;
            colnm.CESSAMT = VE.ColumnName[0].CESSAMT;
            colnm.BLQNTY = VE.ColumnName[0].BLQNTY;
            colnm.NETAMT = VE.ColumnName[0].NETAMT;
            colnm.SGSTPER = VE.ColumnName[0].SGSTPER;
            colnm.GSTPER = VE.ColumnName[0].GSTPER;
            colnm.GSTAMT = VE.ColumnName[0].GSTAMT;
            colnm.ACKNO = VE.ColumnName[0].ACKNO;
            colnm.ACKDT = VE.ColumnName[0].ACKDT;
            colnm.PAGENO = VE.ColumnName[0].PAGENO;
            colnm.PAGESLNO = VE.ColumnName[0].PAGESLNO;
            colnm.BALENO = VE.ColumnName[0].BALENO;
            colnm.DOCREM = VE.ColumnName[0].DOCREM;
            colnm.BLTYPE = VE.ColumnName[0].BLTYPE;

            ColumnNamelist.Add(colnm);
            VE.ColumnName = ColumnNamelist;
            VE.DefaultView = true;
            INI Handel_Ini = new INI();
            string[] SectionName1 = Handel_Ini.GetSectionNames(Server.MapPath("~/Ipsmart.ini"));
            VE.DropDown_list2 = (from i in SectionName1
                                 where i.Contains("Rep_Reg_")
                                 select new DropDown_list2() { value = i, text = i }).Distinct().OrderBy(s => s.text).ToList();
            return PartialView("_Rep_Reg_Column", VE);
        }
        public ActionResult SaveSetting(ReportViewinHtml VE, FormCollection FC, string BtnId)
        {//call from View Page
            try
            {
                string SectionNm = "";
                INI Handel_ini = new INI();
                var SectionName = Handel_ini.GetSectionNames(Server.MapPath("~/Ipsmart.ini")).ToArray().Where(a => a.Contains("Rep_Reg_")).ToList();
                if (BtnId == "S")
                {
                    if (VE.TEXTBOX7.retStr() != "")
                    {
                        SectionNm = "Rep_Reg_" + VE.TEXTBOX7;
                    }
                    else
                    {
                        double maxfrmt = 1;
                        if (SectionName != null && SectionName.Count() != 0)
                        {
                            maxfrmt = SectionName.Where(a => a.Contains("Rep_Reg_Fmt")).Select(b => b.Substring(11, b.Length - 11)).Max().retDbl();
                            maxfrmt++;
                        }
                        SectionNm = "Rep_Reg_Fmt" + maxfrmt;
                    }
                }
                else
                {
                    SectionNm = VE.TEXTBOX6;
                }

                if (BtnId == "S" || BtnId == "U")
                {
                    if (SectionName != null && SectionName.Count() != 0)
                    {
                        foreach (var v in SectionName)
                        {
                            string ActiveSection = Handel_ini.IniReadValue(v, "ACTIVE", Server.MapPath("~/Ipsmart.ini"));
                            if (ActiveSection != "")
                            {
                                Handel_ini.DeleteKey(v, "ACTIVE", Server.MapPath("~/Ipsmart.ini"));
                                break;
                            }
                        }
                    }
                    List<ColumnName> ColumnNamelist = new List<ColumnName>();
                    ColumnName ColumnNameObj = new ColumnName();
                    ColumnNameObj.AUTONO = VE.ColumnName[0].AUTONO;
                    ColumnNameObj.DOCCD = VE.ColumnName[0].DOCCD;
                    ColumnNameObj.DOCNO = VE.ColumnName[0].DOCNO;
                    ColumnNameObj.CANCEL = VE.ColumnName[0].CANCEL;
                    ColumnNameObj.DOCDT = VE.ColumnName[0].DOCDT;
                    ColumnNameObj.AGSLCD = VE.ColumnName[0].AGSLCD;
                    ColumnNameObj.PREFNO = VE.ColumnName[0].PREFNO;
                    ColumnNameObj.PREFDT = VE.ColumnName[0].PREFDT;
                    ColumnNameObj.SLCD = VE.ColumnName[0].SLCD;
                    ColumnNameObj.SLNM = VE.ColumnName[0].SLNM;
                    ColumnNameObj.SLAREA = VE.ColumnName[0].SLAREA;
                    ColumnNameObj.AGSLNM = VE.ColumnName[0].AGSLNM;
                    ColumnNameObj.NM = VE.ColumnName[0].NM;
                    ColumnNameObj.MOBILE = VE.ColumnName[0].MOBILE;
                    ColumnNameObj.GSTNO = VE.ColumnName[0].GSTNO;
                    ColumnNameObj.DISTRICT = VE.ColumnName[0].DISTRICT;
                    ColumnNameObj.ROAMT = VE.ColumnName[0].ROAMT;
                    ColumnNameObj.TCSAMT = VE.ColumnName[0].TCSAMT;
                    ColumnNameObj.BLAMT = VE.ColumnName[0].BLAMT;
                    ColumnNameObj.SLNO = VE.ColumnName[0].SLNO;
                    ColumnNameObj.ITCD = VE.ColumnName[0].ITCD;
                    ColumnNameObj.ITNM = VE.ColumnName[0].ITNM;
                    ColumnNameObj.ITSTYLE = VE.ColumnName[0].ITSTYLE;
                    ColumnNameObj.ITREM = VE.ColumnName[0].ITREM;
                    ColumnNameObj.HSNCODE = VE.ColumnName[0].HSNCODE;
                    ColumnNameObj.UOMCD = VE.ColumnName[0].UOMCD;
                    ColumnNameObj.UOMNM = VE.ColumnName[0].UOMNM;
                    ColumnNameObj.DECIMALS = VE.ColumnName[0].DECIMALS;
                    ColumnNameObj.NOS = VE.ColumnName[0].NOS;
                    ColumnNameObj.QNTY = VE.ColumnName[0].QNTY;
                    ColumnNameObj.RATE = VE.ColumnName[0].RATE;
                    ColumnNameObj.AMT = VE.ColumnName[0].AMT;
                    ColumnNameObj.SCMDISCAMT = VE.ColumnName[0].SCMDISCAMT;
                    ColumnNameObj.TDDISCAMT = VE.ColumnName[0].TDDISCAMT;
                    ColumnNameObj.DISCAMT = VE.ColumnName[0].DISCAMT;
                    ColumnNameObj.TXBLVAL = VE.ColumnName[0].TXBLVAL;
                    ColumnNameObj.CONSLCD = VE.ColumnName[0].CONSLCD;
                    ColumnNameObj.CSLNM = VE.ColumnName[0].CSLNM;
                    ColumnNameObj.CGSTNO = VE.ColumnName[0].CGSTNO;
                    ColumnNameObj.CDISTRICT = VE.ColumnName[0].CDISTRICT;
                    ColumnNameObj.TRSLNM = VE.ColumnName[0].TRSLNM;
                    ColumnNameObj.LRNO = VE.ColumnName[0].LRNO;
                    ColumnNameObj.LRDT = VE.ColumnName[0].LRDT;
                    ColumnNameObj.GRWT = VE.ColumnName[0].GRWT;
                    ColumnNameObj.TRWT = VE.ColumnName[0].TRWT;
                    ColumnNameObj.NTWT = VE.ColumnName[0].NTWT;
                    ColumnNameObj.ORDREFNO = VE.ColumnName[0].ORDREFNO;
                    ColumnNameObj.ORDREFDT = VE.ColumnName[0].ORDREFDT;
                    ColumnNameObj.IGSTPER = VE.ColumnName[0].IGSTPER;
                    ColumnNameObj.IGSTAMT = VE.ColumnName[0].IGSTAMT;
                    ColumnNameObj.CGSTPER = VE.ColumnName[0].CGSTPER;
                    ColumnNameObj.CGSTAMT = VE.ColumnName[0].CGSTAMT;
                    ColumnNameObj.SGSTAMT = VE.ColumnName[0].SGSTAMT;
                    ColumnNameObj.CESSPER = VE.ColumnName[0].CESSPER;
                    ColumnNameObj.CESSAMT = VE.ColumnName[0].CESSAMT;
                    ColumnNameObj.BLQNTY = VE.ColumnName[0].BLQNTY;
                    ColumnNameObj.NETAMT = VE.ColumnName[0].NETAMT;
                    ColumnNameObj.SGSTPER = VE.ColumnName[0].SGSTPER;
                    ColumnNameObj.GSTPER = VE.ColumnName[0].GSTPER;
                    ColumnNameObj.GSTAMT = VE.ColumnName[0].GSTAMT;
                    ColumnNameObj.ACKNO = VE.ColumnName[0].ACKNO;
                    ColumnNameObj.ACKDT = VE.ColumnName[0].ACKDT;
                    ColumnNameObj.PAGENO = VE.ColumnName[0].PAGENO;
                    ColumnNameObj.PAGESLNO = VE.ColumnName[0].PAGESLNO;
                    ColumnNameObj.BALENO = VE.ColumnName[0].BALENO;
                    ColumnNameObj.DOCREM = VE.ColumnName[0].DOCREM;
                    ColumnNameObj.BLTYPE = VE.ColumnName[0].BLTYPE;


                    Handel_ini.IniWriteValue(SectionNm, "AUTONO", ColumnNameObj.AUTONO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "DOCCD", ColumnNameObj.DOCCD.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "DOCNO", ColumnNameObj.DOCNO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "CANCEL", ColumnNameObj.CANCEL.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "DOCDT", ColumnNameObj.DOCDT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "AGSLCD", ColumnNameObj.AGSLCD.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "PREFNO", ColumnNameObj.PREFNO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "PREFDT", ColumnNameObj.PREFDT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "SLCD", ColumnNameObj.SLCD.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "SLNM", ColumnNameObj.SLNM.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "SLAREA", ColumnNameObj.SLAREA.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "AGSLNM", ColumnNameObj.AGSLNM.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "NM", ColumnNameObj.NM.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "MOBILE", ColumnNameObj.MOBILE.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "GSTNO", ColumnNameObj.GSTNO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "DISTRICT", ColumnNameObj.DISTRICT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "ROAMT", ColumnNameObj.ROAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "TCSAMT", ColumnNameObj.TCSAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "BLAMT", ColumnNameObj.BLAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "SLNO", ColumnNameObj.SLNO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "ITCD", ColumnNameObj.ITCD.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "ITNM", ColumnNameObj.ITNM.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "ITSTYLE", ColumnNameObj.ITSTYLE.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "ITREM", ColumnNameObj.ITREM.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "HSNCODE", ColumnNameObj.HSNCODE.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "UOMCD", ColumnNameObj.UOMCD.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "UOMNM", ColumnNameObj.UOMNM.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "DECIMALS", ColumnNameObj.DECIMALS.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "NOS", ColumnNameObj.NOS.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "QNTY", ColumnNameObj.QNTY.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "RATE", ColumnNameObj.RATE.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "AMT", ColumnNameObj.AMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "SCMDISCAMT", ColumnNameObj.SCMDISCAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "TDDISCAMT", ColumnNameObj.TDDISCAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "DISCAMT", ColumnNameObj.DISCAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "TXBLVAL", ColumnNameObj.TXBLVAL.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "CONSLCD", ColumnNameObj.CONSLCD.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "CSLNM", ColumnNameObj.CSLNM.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "CGSTNO", ColumnNameObj.CGSTNO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "CDISTRICT", ColumnNameObj.CDISTRICT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "TRSLNM", ColumnNameObj.TRSLNM.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "LRNO", ColumnNameObj.LRNO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "LRDT", ColumnNameObj.LRDT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "GRWT", ColumnNameObj.GRWT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "TRWT", ColumnNameObj.TRWT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "NTWT", ColumnNameObj.NTWT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "ORDREFNO", ColumnNameObj.ORDREFNO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "ORDREFDT", ColumnNameObj.ORDREFDT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "IGSTPER", ColumnNameObj.IGSTPER.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "IGSTAMT", ColumnNameObj.IGSTAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "CGSTPER", ColumnNameObj.CGSTPER.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "CGSTAMT", ColumnNameObj.CGSTAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "SGSTAMT", ColumnNameObj.SGSTAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "CESSPER", ColumnNameObj.CESSPER.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "CESSAMT", ColumnNameObj.CESSAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "BLQNTY", ColumnNameObj.BLQNTY.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "NETAMT", ColumnNameObj.NETAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "SGSTPER", ColumnNameObj.SGSTPER.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "GSTPER", ColumnNameObj.GSTPER.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "GSTAMT", ColumnNameObj.GSTAMT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "ACKNO", ColumnNameObj.ACKNO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "ACKDT", ColumnNameObj.ACKDT.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "PAGENO", ColumnNameObj.PAGENO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "PAGESLNO", ColumnNameObj.PAGESLNO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "BALENO", ColumnNameObj.BALENO.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "DOCREM", ColumnNameObj.DOCREM.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "BLTYPE", ColumnNameObj.BLTYPE.ToString(), Server.MapPath("~/Ipsmart.ini"));
                    Handel_ini.IniWriteValue(SectionNm, "ACTIVE", "True", Server.MapPath("~/Ipsmart.ini"));

                    ColumnNamelist.Add(ColumnNameObj);
                    VE.ColumnName = ColumnNamelist;
                    VE.DefaultView = true;
                }
                else
                {
                    Handel_ini.DeleteSection(SectionNm, Server.MapPath("~/Ipsmart.ini"));
                }
                return Content("");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public void RegExcelfromDataTables(DataTable[] dt, string[] sheetname, string filename, bool isRowHighlight, string Caption)
        {
            try
            {
                string[] roundoffupto2dec = { "ROAMT", "TCSAMT", "BLAMT", "RATE", "AMT", "SCMDISCAMT", "TDDISCAMT", "DISCAMT", "TXBLVAL", "GRWT", "TRWT", "NTWT", "IGSTPER", "IGSTAMT", "CGSTPER", "CGSTAMT", "SGSTAMT", "CESSPER", "CESSAMT", "NETAMT", "SGSTPER", "GSTPER", "GSTAMT" };
                string[] roundoffupto3dec = { "QNTY", "BLQNTY" };

                using (ExcelPackage pck = new ExcelPackage())
                {
                    for (int i = 0; i < dt.Length; i++)
                    {
                        ExcelWorksheet ws = pck.Workbook.Worksheets.Add(sheetname[i]);
                        int row = 0;
                        if (Caption != "")
                        {
                            ws.Cells[++row, 1].Value = CommVar.CompName(UNQSNO);
                            ws.Cells[++row, 1].Value = CommVar.LocName(UNQSNO);
                            ws.Cells[++row, 1].Value = Caption;
                        }
                        if (isRowHighlight)
                        {
                            ws.Cells[++row, 1].LoadFromDataTable(dt[i], true, TableStyles.Medium15); //You can Use TableStyles property of your desire.    ,
                        }
                        else
                        {
                            using (ExcelRange Rng = ws.Cells[row + 1, 1, row + 1, dt[i].Columns.Count])
                            {
                                Rng.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                Rng.Style.Font.Bold = true;
                                Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SkyBlue);
                            }
                            ws.Cells[++row, 1].LoadFromDataTable(dt[i], true);
                        }
                        int strtRow = row + 1;
                        row = row + dt[i].Rows.Count;
                        using (ExcelRange Rng = ws.Cells[++row, 1, row, dt[i].Columns.Count])
                        {
                            Rng.Style.Font.Bold = true;
                        }
                        ws.Cells[row, 1].Value = "Sub Total";
                        int column = 0;
                        foreach (DataColumn dc in dt[i].Columns)
                        {
                            ++column;
                            if (dc.DataType == typeof(double) || dc.DataType == typeof(decimal) || dc.DataType == typeof(int))
                            {
                                ws.Cells[row, column, row, column].Formula = "=sum(" + ws.Cells[strtRow, column].Address + ":" + ws.Cells[row - 1, column].Address + ")";
                            }
                            if ((dc.DataType == typeof(double) || dc.DataType == typeof(decimal)) && roundoffupto2dec.Contains(dc.ColumnName))
                            {
                                ws.Column(column).Style.Numberformat.Format = "0.00";
                            }
                            else if ((dc.DataType == typeof(double) || dc.DataType == typeof(decimal)) && roundoffupto3dec.Contains(dc.ColumnName))
                            {
                                ws.Column(column).Style.Numberformat.Format = "0.000";
                            }
                        }
                        ws.Cells.AutoFitColumns();
                    }

                    //Read the Excel file in a byte array    
                    Byte[] fileBytes = pck.GetAsByteArray();
                    Response.ClearContent();
                    Response.AddHeader("content-disposition", "attachment;filename=" + filename.retRepname() + ".xlsx");
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.BinaryWrite(fileBytes);
                    Response.End();
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
        }
    }
}