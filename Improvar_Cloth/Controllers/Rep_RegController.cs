using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;

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
                    ViewBag.formname = "Registers";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    //string selgrp = MasterHelp.GetUserITGrpCd().Replace("','", ",");
                    //string[] selgrpcd = selgrp.Split(',');
                    string com = CommVar.Compcd(UNQSNO);
                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("");
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);

                    //VE.DropDown_list = (from i in DB.M_GROUP select new DropDown_list() { value = i.ITGRPCD, text = i.ITGRPNM }).OrderBy(s => s.text).ToList();
                    //VE.Itgrpnm = MasterHelp.ComboFill("ITGRPCD", VE.DropDown_list, 0, 1);
                    //=========For Report Type===========//
                    List<DropDown_list1> RT = new List<DropDown_list1>();
                    DropDown_list1 RT1 = new DropDown_list1();
                    RT1.value = "Sales";
                    RT1.text = "Sales";
                    RT.Add(RT1);
                    DropDown_list1 RT2 = new DropDown_list1();
                    RT2.value = "Sales Return";
                    RT2.text = "Sales Return";
                    RT.Add(RT2);
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

                    DropDown_list1 RT6 = new DropDown_list1();
                    RT6.value = "Stock Transfer";
                    RT6.text = "Stock Transfer";
                    RT.Add(RT6);
                    DropDown_list1 RT7 = new DropDown_list1();
                    RT7.value = "Stock Conversion";
                    RT7.text = "Stock Conversion";
                    RT.Add(RT7);
                    DropDown_list1 RT8 = new DropDown_list1();
                    RT8.value = "Stock Adjustment";
                    RT8.text = "Stock Adjustment";
                    RT.Add(RT8);
                    RT.Add(new DropDown_list1 { value = "SDWOQ", text = "Sales Debit Note (W/O Qnty)" });
                    RT.Add(new DropDown_list1 { value = "SCWOQ", text = "Sales Credit Note (W/O Qnty)" });
                    RT.Add(new DropDown_list1 { value = "PDWOQ", text = "Purchase Debit Note (W/O Qnty)" });
                    RT.Add(new DropDown_list1 { value = "PCWOQ", text = "Purchase Credit Note (W/O Qnty)" });
                    VE.DropDown_list1 = RT;
                    //=========End Report Type===========//

                    //=========For Report Type===========//
                    List<DropDown_list> ALLRT = new List<DropDown_list>();
                    ALLRT.Add(new DropDown_list { value = "", text = "--SELECT--" });
                    ALLRT.Add(new DropDown_list { value = "All Sales", text = "All Sales" });
                    ALLRT.Add(new DropDown_list { value = "All Purchase", text = "All Purchase" });
                    VE.DropDown_list = ALLRT;
                    //=========End Report Type===========//

                    VE.DropDown_list3 = (from i in DBF.M_LOCA
                                         where i.COMPCD == com
                                         select new DropDown_list3() { value = i.LOCCD, text = i.LOCNM }).Distinct().OrderBy(s => s.text).ToList();// location
                    VE.TEXTBOX5 = MasterHelp.ComboFill("loccd", VE.DropDown_list3, 0, 1);
                    //VE.DropDown_list2 = MasterHelp.INSURANCE().ConvertAll(x => new DropDown_list2 { text = x.Text, value = x.Value });
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
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);

                string fdt = VE.FDT.retDateStr(), tdt = VE.TDT.retDateStr();

                string GODOWN = VE.TEXTBOX3.retStr();
                string INSBY = VE.TEXTBOX6.retStr();

                bool itmdtl = false, batchdtl = false, itemrem = false;
                if (VE.Checkbox1 == true) itmdtl = true;
                if (VE.Checkbox2 == true) batchdtl = true;
                if (VE.Checkbox6 == true) itemrem = true;

                string itgrpcd = "";

                string reptype = FC["reptype"].ToString();
                string selslcd = "", unselslcd = "", selloccd = "";
                if (FC.AllKeys.Contains("slcdvalue")) selslcd = CommFunc.retSqlformat(FC["slcdvalue"].ToString());
                if (FC.AllKeys.Contains("slcdunselvalue")) unselslcd = CommFunc.retSqlformat(FC["slcdunselvalue"].ToString());
                if (FC.AllKeys.Contains("ITGRPCDvalue")) itgrpcd = CommFunc.retSqlformat(FC["ITGRPCDvalue"].ToString());
                if (FC.AllKeys.Contains("loccdvalue")) selloccd = FC["loccdvalue"].retSqlformat();

                string txntag = ""; string regdsp = "";
                txntag = "SALES";
                if (VE.TEXTBOX7.retStr() != "")
                {
                    regdsp = VE.TEXTBOX7.ToString() + " Register";
                }
                else
                {
                    regdsp = VE.TEXTBOX1.ToString() + " Register";
                }

                string repsorton = FC["RepSortOn"].ToString();
                bool plistprint = VE.Checkbox4;
                bool con_transprint = VE.Checkbox3;
                //bool saprefnoprint = VE.Checkbox5;
                bool orddetprint = VE.Checkbox7;
                bool SeparateAchead = VE.Checkbox8;
                //if (VE.TEXTBOX7.retStr() != "")
                //{
                //    switch (VE.TEXTBOX7)
                //    {
                //        case "All Sales":
                //            txntag = "'SB','SR','SD','SC'"; break;
                //        case "All Purchase":
                //            txntag = "'PB','PR','PD','PC'"; break;
                //        default: txntag = ""; break;
                //    }
                //}
                //else
                //{
                //    switch (VE.TEXTBOX1)
                //    {
                //        case "Sales":
                //            txntag = "'SB'"; break;
                //        case "Purchase":
                //            txntag = "'PB'"; break;
                //        case "Sales Return":
                //            txntag = "'SR'"; break;
                //        case "Purchase Return":
                //            txntag = "'PR'"; break;
                //        case "Opening Stock":
                //            txntag = "'OP'"; break;
                //        case "Proforma":
                //            txntag = "'PI'"; break;
                //        case "SDWOQ":
                //            txntag = "'SD'"; break;
                //        case "SCWOQ":
                //            txntag = "'SC'"; break;
                //        case "PDWOQ":
                //            txntag = "'PD'"; break;
                //        case "PCWOQ":
                //            txntag = "'PC'"; break;
                //        default: txntag = ""; break;
                //    }
                //}


                string query = "";
                query = "";
                //query += "select a.autono, a.doccd, a.docno, a.cancel, a.docdt, a.itgrpcd, a.pblno, a.pbldt, ";
                //query += "a.slcd, c.slnm, c.gstno, c.district, nvl(a.roamt,0) roamt, nvl(a.tcsamt,0) tcsamt, a.blamt, a.prcdesc, ";
                //query += "b.slno, b.itcd, b.prodcd, b.itnm, b.itrem, b.hsncode, b.uomcd, b.uomnm, b.decimals, b.packsize, b.nos, b.qnty, ";
                //query += "b.rate, b.basamt, b.stddiscamt, b.discamt, g.conslcd, d.slnm cslnm, d.gstno cgstno, h.sapblno, ";
                //query += "d.district cdistrict, e.slnm trslnm,f.lrno,f.lrdt, g.ordrefno, to_char(nvl(g.ordrefdt,''),'dd/mm/yyyy') ordrefdt, ";
                //query += "b.igstper, b.igstamt, b.cgstper, b.cgstamt, b.sgstper, b.sgstamt, b.cessper, b.cessamt, b.batchno,b.blqnty,b.bluomcd from ";
                //query += "( select a.autono, b.doccd, b.docno, b.cancel, b.docdt, a.itgrpcd, a.pblno, a.pbldt,  ";
                //query += "a.slcd, a.roamt, a.tcsamt, a.blamt, d.prcdesc ";
                //query += "from " + scm1 + ".t_txn a, " + scm1 + ".t_cntrl_hdr b, " + scmf + ".m_subleg c, " + scm1 + ".m_itemplist d ";
                //query += "where a.autono=b.autono(+) and a.slcd=c.slcd(+) and a.itmprccd=d.itmprccd(+) and  ";
                //query += "b.compcd='" + COM + "' and ";
                //if (selloccd == "") query += "b.loccd='" + LOC + "' and "; else query += "b.loccd in (" + selloccd + ") and ";
                //if (VE.FDOCNO.retStr() != "") query += "b.doconlyno >= '" + VE.FDOCNO + "' and ";
                //if (VE.TDOCNO.retStr() != "") query += "b.doconlyno <= '" + VE.TDOCNO + "' and ";
                //if (GODOWN != "") query += " a.GOCD in('" + GODOWN + "') and ";
                //if (repsorton == "bldt")
                //{
                //    if (fdt != "") query += "a.pbldt >= to_date('" + fdt + "','dd/mm/yyyy') and ";
                //    if (tdt != "") query += "a.pbldt <= to_date('" + tdt + "','dd/mm/yyyy') and  ";
                //}
                //else
                //{
                //    if (fdt != "") query += "b.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and ";
                //    if (tdt != "") query += "b.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and  ";
                //}
                ////query += "a.itgrpcd in (" + itgrpcd + ") and a.doctag in ( " + txntag + ")  ";
                //query += "a.itgrpcd in (" + itgrpcd + ")  ";
                //query += ") a, ( ";
                //query += "select a.autono, a.slno, a.itcd, a.itrem, b.prodcd, b.itnm, b.hsnsaccd hsncode, b.uomcd, c.uomnm, c.decimals, b.packsize, a.nos, a.qnty, a.rate, a.basamt,  ";
                //query += "listagg(e.batchno,',') within group (order by d.autono, d.slno) batchno,  ";
                //query += "a.stddiscamt, a.discamt, a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.cessper, a.cessamt,a.blqnty,a.bluomcd  ";
                //query += "from " + scm1 + ".t_txndtl a, " + scm1 + ".m_sitem b, " + scmf + ".m_uom c, ";
                //query += scm1 + ".t_batchdtl d, " + scm1 + ".t_batchmst e, " + scm1 + ".m_itemplist f  ";
                //query += "where a.itcd=b.itcd(+) and b.uomcd=c.uomcd(+) and a.autono=d.autono(+) and a.slno=d.slno(+) and  ";
                //query += "d.batchautono=e.batchautono(+) and e.itmprccd=f.itmprccd(+) ";
                //query += "group by a.autono, a.slno, a.itcd, a.itrem, b.prodcd, b.itnm, b.hsnsaccd, b.uomcd, c.uomnm, c.decimals, b.packsize, a.nos, a.qnty, a.rate, a.basamt,  ";
                //query += "a.stddiscamt, a.discamt, a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.cessper, a.cessamt,a.blqnty,a.bluomcd  ";
                //query += "union ";
                //query += "select a.autono, a.slno+1000 slno, a.amtcd itcd, '' itrem, '' prodcd, b.amtnm||a.amtdesc itnm, a.hsncode, 'OTH' uomcd, 'OTH' uomnm, 0 decimals, 0 packsize, 0 nos, 0 qnty, 0 rate, a.amt basamt,  ";
                //query += "'' batchno,  ";
                //query += "0 stddiscamt, 0 discamt, a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.cessper, a.cessamt,0 blqnty,'OTH' bluomcd  ";
                //query += "from " + scm1 + ".t_txnamt a, " + scm1 + ".m_amttype b  ";
                //query += "where a.amtcd=b.amtcd(+) ";
                //query += ") b, " + scmf + ".m_subleg c, " + scmf + ".m_subleg d, " + scmf + ".m_subleg e, " + scm1 + ".t_txntrans f, " + scm1 + ".t_txn g, " + scm1 + ".t_txnoth h ";
                //query += "where a.autono=b.autono(+) and a.slcd=c.slcd(+) and g.conslcd=d.slcd(+) and a.autono=f.autono(+) and ";
                //query += "f.translcd=e.slcd(+) and a.autono=f.autono(+) and a.autono=g.autono(+) and a.autono=h.autono(+) ";
                //if (selslcd != "") query += " and a.slcd in (" + selslcd + ") ";
                //if (unselslcd != "") query += " and a.slcd not in (" + unselslcd + ") ";
                ////if (INSBY != "") query += " and h.insby='" + INSBY + "' ";
                //query += "order by ";
                //if (repsorton == "partywise") query += "slcd,pbldt,pblno,docdt,docno";
                //else if (repsorton == "bldt") query += "pbldt,pblno";
                //else query += "docdt";
                //if (itmdtl == true || reptype == "Product")
                //{
                //    query += " , itgrpcd, docno, slno ";
                //}
                //else
                //{
                //    query += " , itgrpcd, docno, igstper, cgstper, sgstper ";
                //}

                string query1 = "";
                query1 += " select a.autono, a.doccd, a.docno, a.cancel, a.docdt, ";
                //query1 += "--a.itgrpcd, ";
                query1 += "  a.prefno, a.prefdt, a.slcd, c.slnm, c.gstno, c.district, nvl(a.roamt, 0) roamt, ";
                query1 += " nvl(a.tcsamt, 0) tcsamt, a.blamt, ";
                //query1 += "--a.prcdesc, ";
                query1 += "   b.slno, b.itcd, ";
                //--b.prodgrpcd,  ";
                query1 += "   b.itnm,b.itstyle, b.itrem, b.hsncode, b.uomcd, b.uomnm, b.decimals, b.nos, ";
                query1 += " b.qnty, b.rate, b.amt, b.tddiscamt, b.discamt, g.conslcd, d.slnm cslnm, d.gstno cgstno, d.district cdistrict, ";
                query1 += " e.slnm trslnm, f.lrno,f.lrdt, '' ordrefno, to_char(nvl('', ''), 'dd/mm/yyyy') ordrefdt, b.igstper, b.igstamt, b.cgstper, ";
                query1 += " b.cgstamt, b.sgstper, b.sgstamt, b.cessper, b.cessamt, b.batchno,b.blqnty from ( ";
                query1 += " select a.autono, b.doccd, b.docno, b.cancel, ";
                query1 += "b.docdt, ";
                //query1 += "--a.itgrpcd, ";
                query1 += "a.prefno, a.prefdt, a.slcd, a.roamt, a.tcsamt, a.blamt ";
                //query1 += "--, d.prcdesc ";
                query1 += "from " + scm1 + ".t_txn a, " + scm1 + ".t_cntrl_hdr b, ";
                query1 += " " + scmf + ".m_subleg c ";
                //--, " + scm1 + ".m_itemplistdtl d ";
                query1 += "  where a.autono = b.autono and a.slcd = c.slcd(+) ";
                //query1 += " -- and a.prccd = d.prccd(+) ";
                query1 += "  and  b.compcd = '" + COM + "' ";
                if (selloccd == "") query1 += " and b.loccd = '" + LOC + "'  "; else query1 += " and b.loccd in (" + selloccd + ")  ";
                if (GODOWN.retStr() != "") query1 += "and  a.GOCD in('" + GODOWN + "')  ";
                if (repsorton == "bldt")
                {
                    if (fdt != "") query1 += "and a.prefdt >= to_date('" + fdt + "','dd/mm/yyyy')  ";
                    if (tdt != "") query1 += "and a.prefdt <= to_date('" + tdt + "','dd/mm/yyyy')   ";
                }
                else
                {
                    if (fdt != "") query1 += "and b.docdt >= to_date('" + fdt + "','dd/mm/yyyy')   ";
                    if (tdt != "") query1 += "and b.docdt <= to_date('" + tdt + "','dd/mm/yyyy')   ";
                }

                //query1 += "--and  a.itgrpcd in (" + itgrpcd + ") ";

                query1 += " ) a, ( ";
                query1 += " select distinct a.autono, a.slno, a.itcd, a.itrem, ";
                //--i.prodgrpcd,  ";
                query1 += " b.itnm,b.styleno||' '||b.itnm itstyle, b.hsncode hsncode, b.uomcd, c.uomnm, c.decimals, ";
                query1 += "  a.nos, a.qnty, a.rate, a.amt,  listagg(e.batchno, ',') within group (order by d.autono, d.slno) batchno,  a.tddiscamt, a.discamt,  ";
                query1 += " a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.cessper, a.cessamt,a.blqnty from " + scm1 + ".t_txndtl a, ";
                query1 += "" + scm1 + ".m_sitem b, " + scmf + ".m_uom c, " + scm1 + ".t_batchdtl d, " + scm1 + ".t_batchmst e ";
                //query1 += " --," + scm1 + ".m_group i ";
                query1 += "   where a.itcd = b.itcd  and b.uomcd = c.uomcd and a.autono = e.autono(+) and e.autono = d.autono(+) ";
                query1 += " group by ";
                query1 += " a.autono, a.slno, a.itcd, a.itrem, ";
                //--i.prodgrpcd, ";
                query1 += "  b.itnm, b.hsncode, b.uomcd, c.uomnm, c.decimals, a.nos, a.qnty, a.rate, a.amt,  ";
                query1 += " a.tddiscamt, a.discamt, a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.cessper, a.cessamt,a.blqnty,b.styleno||' '||b.itnm ";
                //query1 += " --,a.bluomcd ";
                query1 += " union select a.autono, a.slno + 1000 slno, a.amtcd itcd, '' itrem ,''itstyle ";
                //--'' prodgrpcd ";
                query1 += " , b.amtnm itnm, a.hsncode,  ";
                query1 += " 'OTH' uomcd, 'OTH' uomnm, 0 decimals, 0 nos, 0 qnty, 0 rate, a.amt,  '' batchno,  0 tddiscamt, 0 discamt, a.igstper, a.igstamt, ";
                query1 += " a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.cessper, a.cessamt,0 blqnty ";
                query1 += " from " + scm1 + ".t_txnamt a, " + scm1 + ".m_amttype b ";
                query1 += " where a.amtcd = b.amtcd ";
                query1 += " ) b, " + scmf + ".m_subleg c, " + scmf + ".m_subleg d, " + scmf + ".m_subleg e, " + scm1 + ".t_txntrans f, ";
                query1 += "" + scm1 + ".t_txn g, " + scm1 + ".t_txnoth h ";
                query1 += "where a.autono = b.autono(+) and a.slcd = c.slcd and g.conslcd = d.slcd(+) and a.autono = f.autono(+) ";
                query1 += "and f.translcd = e.slcd(+) and a.autono = f.autono(+) and a.autono = g.autono(+) and a.autono = h.autono(+) ";
                if (selslcd != "") query1 += " and a.slcd in (" + selslcd + ") ";
                if (unselslcd != "") query1 += " and a.slcd not in (" + unselslcd + ") ";
                query1 += "order by ";
                if (repsorton == "partywise") query1 += "slcd,prefdt,prefno,docdt,docno";
                else if (repsorton == "bldt") query1 += "prefdt,prefno";
                else query1 += "docdt";
                if (itmdtl == true || reptype == "Product")
                {
                    query1 += " , docno, slno ";
                }
                else
                {
                    query1 += " , docno, igstper, cgstper, sgstper ";
                }


                DataTable tbl = MasterHelp.SQLquery(query1);
                if (tbl.Rows.Count == 0)
                {
                    return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "Records not found !!" });
                }
                var cnt_blqnty = (from DataRow dr in tbl.Rows where dr["blqnty"].retDbl() != 0 select dr["blqnty"].retDbl()).ToList();
                //var cnt_bluom = (from DataRow dr in tbl.Rows where dr["bluomcd"].retStr() != "" select dr["bluomcd"].retStr()).ToList();
                DataTable rsStkPrcDesc;
                string query_new = "";
                query_new += "select distinct a.autono, c.prcdesc stkprcdesc, nvl(e.docrem,'') docrem ";
                query_new += "from " + scm1 + ".t_batchdtl a, " + scm1 + ".t_batchmst b, " + scm1 + ".m_itemplist c, " + scm1 + ".t_cntrl_hdr d, " + scm1 + ".T_CNTRL_HDR_REM e ";
                query_new += "where a.batchautono=b.batchautono(+) and b.itmprccd=c.itmprccd(+) and a.autono=d.autono(+) and a.autono=e.autono(+) and ";
                query_new += "d.compcd='" + COM + "' ";
                if (selloccd == "") query = query + " and d.loccd='" + LOC + "'  "; else query = query + " and d.loccd in (" + selloccd + ")  ";
                query_new += " and a.autono=e.autono(+) and (e.slno=1 or e.slno is null) ";
                if (fdt != "") query_new += "and d.docdt >= to_date('" + fdt + "','dd/mm/yyyy') ";
                if (tdt != "") query_new += "and d.docdt <= to_date('" + tdt + "','dd/mm/yyyy') ";

                rsStkPrcDesc = MasterHelp.SQLquery(query_new);

                DataTable IR = new DataTable("mstrep");
                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                string pghdr1 = "";

                double tnos = 0; double tqnty = 0; double tbasamt = 0; double tdisc1 = 0; double tdisc2 = 0; double ttaxable = 0, tbqnty = 0;
                double tigstamt = 0; double tcgstamt = 0; double tsgstamt = 0; double troamt = 0; double ttcsamt = 0; double tblamt = 0;
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
                if (((VE.TEXTBOX1 == "Purchase" || VE.TEXTBOX1 == "Purchase Return") && (VE.TEXTBOX7.retStr() == "")) || (VE.TEXTBOX7 == "All Purchase")) showpbill = true;
                #region Normal Report
                if (reptype == "Normal")
                {
                    HC.RepStart(IR, 3);
                    HC.GetPrintHeader(IR, "doccd", "string", "c,5", "Doc;Code");
                    HC.GetPrintHeader(IR, "docdt", "string", "d,10:dd/mm/yy", ";Doc Date");
                    HC.GetPrintHeader(IR, "docno", "string", "c,18", ";Doc No");
                    if (showpbill == true) HC.GetPrintHeader(IR, "prefdt", "string", "d,10:dd/mm/yy", "Party;Bill Date");
                    if (showpbill == true) HC.GetPrintHeader(IR, "prefno", "string", "c,16", "Party;Bill No");
                    if (itmdtl == true)
                    {
                        HC.GetPrintHeader(IR, "slnm", "string", "c,35", "Party Name;Item Name");
                        if (itemrem == true) HC.GetPrintHeader(IR, "itrem", "string", "c,20", ";Item Remarks");
                        HC.GetPrintHeader(IR, "gstno", "string", "c,15", "GST No.;Prod Code");
                        HC.GetPrintHeader(IR, "hsncode", "string", "c,8", "HSN/SAC");
                        HC.GetPrintHeader(IR, "uomcd", "string", "c,4", "Uom");
                        HC.GetPrintHeader(IR, "nos", "double", "n,5", "Cases");
                        HC.GetPrintHeader(IR, "qnty", "double", "n,12,6", "Qnty");
                        HC.GetPrintHeader(IR, "rate", "double", "n,10,2", "Rate");
                        //if (cnt_bluom.Count() > 0)
                        //{
                        //    HC.GetPrintHeader(IR, "bluomcd", "string", "c,4", "BL Uom");
                        //}
                        if (cnt_blqnty.Count() > 0)
                        {
                            HC.GetPrintHeader(IR, "blqnty", "double", "n,12,3", "BL Qnty");
                        }
                    }
                    else
                    {
                        HC.GetPrintHeader(IR, "slnm", "string", "c,35", "Party Name");
                        HC.GetPrintHeader(IR, "gstno", "string", "c,15", "GST No.");
                        HC.GetPrintHeader(IR, "nos", "double", "n,5", "Cases");
                        HC.GetPrintHeader(IR, "qnty", "double", "n,12,5", "Qnty");
                    }
                    HC.GetPrintHeader(IR, "amt", "double", "n,12,2", "Basic;Amount");
                    if (SeparateAchead)
                    {
                        foreach (DataRow dr in amtDT.Rows)
                        {
                            HC.GetPrintHeader(IR, dr["itcd"].ToString(), "string", "c,10", dr["itnm"].ToString());
                        }
                    }
                    HC.GetPrintHeader(IR, "disc1", "double", "n,10,2", "Disc1;Amount");
                    HC.GetPrintHeader(IR, "disc2", "double", "n,10,2", "Disc2;Amount");
                    HC.GetPrintHeader(IR, "taxableval", "double", "n,12,2", "Taxable;Value");
                    HC.GetPrintHeader(IR, "igstper", "double", "n,5,3", "IGST;%");
                    HC.GetPrintHeader(IR, "igstamt", "double", "n,10,2", "IGST;Amt");
                    HC.GetPrintHeader(IR, "cgstper", "double", "n,5,3", "CGST;%");
                    HC.GetPrintHeader(IR, "cgstamt", "double", "n,10,2", "CGST;Amt");
                    HC.GetPrintHeader(IR, "sgstper", "double", "n,5,3", "SGST;%");
                    HC.GetPrintHeader(IR, "sgstamt", "double", "n,10,2", "SGST;Amt");
                    HC.GetPrintHeader(IR, "tcsamt", "double", "n,10,2", "TCS;Amt");
                    HC.GetPrintHeader(IR, "roamt", "double", "n,6,2", "R/Off;Amt");
                    HC.GetPrintHeader(IR, "blamt", "double", "n,12,2", ";Bill Value");
                    if (plistprint == true) HC.GetPrintHeader(IR, "prcdesc", "string", "c,10", "Price;List");
                    if (con_transprint == true) HC.GetPrintHeader(IR, "cslnm", "string", "c,40", "Consignee;Name");
                    if (con_transprint == true) HC.GetPrintHeader(IR, "trslnm", "string", "c,40", "Transporter;Name");
                    if (con_transprint == true) HC.GetPrintHeader(IR, "lrno", "string", "c,15", "Lr. ;No");
                    if (con_transprint == true) HC.GetPrintHeader(IR, "lrdt", "string", "d,10:dd/mm/yy", "Lr. ;Date");
                    if (orddetprint == true) HC.GetPrintHeader(IR, "ordno", "string", "c,50", "Order; No");
                    if (orddetprint == true) HC.GetPrintHeader(IR, "orddt", "string", "d,10:dd/mm/yy", "Order ;Date");
                    if (batchdtl == true && itmdtl == true) HC.GetPrintHeader(IR, "batchno", "string", "c,20", ";Batch nos");
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
                                if (tbl.Rows[i]["cancel"].ToString() != "Y")
                                {
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
                                        bbasamt = bbasamt + tbl.Rows[i]["amt"].retDbl();
                                    }
                                    bdisc1 = bdisc1 + tbl.Rows[i]["tddiscamt"].retDbl();
                                    bdisc2 = bdisc2 + tbl.Rows[i]["discamt"].retDbl();
                                    btaxable = btaxable + tbl.Rows[i]["amt"].retDbl() - tbl.Rows[i]["tddiscamt"].retDbl() - tbl.Rows[i]["discamt"].retDbl();
                                    bigstamt = bigstamt + tbl.Rows[i]["igstamt"].retDbl();
                                    bcgstamt = bcgstamt + tbl.Rows[i]["cgstamt"].retDbl();
                                    bsgstamt = bsgstamt + tbl.Rows[i]["sgstamt"].retDbl();
                                    blqnty = blqnty + tbl.Rows[i]["blqnty"].retDbl();
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

                                dr["doccd"] = tbl.Rows[i]["doccd"].ToString();
                                dr["docdt"] = tbl.Rows[i]["docdt"] == DBNull.Value ? "" : tbl.Rows[i]["docdt"].ToString().Substring(0, 10).ToString();
                                dr["docno"] = tbl.Rows[i]["docno"].ToString() + cancrem;
                                dr["slnm"] = tbl.Rows[i]["slnm"].ToString();
                                dr["gstno"] = tbl.Rows[i]["gstno"].ToString();
                                if (showpbill == true) dr["prefno"] = tbl.Rows[i]["prefno"].ToString();
                                //if (plistprint == true) dr["prcdesc"] = tbl.Rows[i]["prcdesc"].ToString();
                                if (VE.Checkbox5 == true) dr["saprem"] = (tbl.Rows[i]["sapblno"].ToString() == "" ? "" : "BL# " + tbl.Rows[i]["sapblno"].ToString());
                                if (showpbill == true) dr["prefdt"] = tbl.Rows[i]["prefdt"] == DBNull.Value ? "" : tbl.Rows[i]["prefdt"].ToString().Substring(0, 10).ToString();
                                if (con_transprint == true)
                                {
                                    dr["cslnm"] = tbl.Rows[i]["cslnm"].ToString();
                                    dr["trslnm"] = tbl.Rows[i]["trslnm"].ToString();
                                    dr["lrno"] = tbl.Rows[i]["lrno"].ToString();
                                    dr["lrdt"] = tbl.Rows[i]["lrdt"].retDateStr();
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
                                    dr["blamt"] = tbl.Rows[i]["blamt"].retDbl();
                                    ttcsamt = ttcsamt + tbl.Rows[i]["tcsamt"].retDbl();
                                    troamt = troamt + tbl.Rows[i]["roamt"].retDbl();
                                    tblamt = tblamt + tbl.Rows[i]["blamt"].retDbl();
                                }

                                bldtl = false;
                            }
                            if (tbl.Rows[i]["cancel"].ToString() != "Y")
                            {
                                dr["nos"] = bnos;
                                dr["qnty"] = bqnty;
                                if (SeparateAchead == true)
                                {
                                    foreach (DataRow amtdr in amtDT.Rows)
                                    {
                                        dr[amtdr["itcd"].ToString()] = amtdr["AmtItcdRate"].retDbl();
                                    }
                                }
                                dr["amt"] = bbasamt;
                                dr["disc1"] = bdisc1;
                                dr["disc2"] = bdisc2;
                                dr["taxableval"] = btaxable;
                                dr["igstper"] = bigstper;
                                dr["cgstper"] = bcgstper;
                                dr["sgstper"] = bsgstper;
                                dr["igstamt"] = bigstamt;
                                dr["cgstamt"] = bcgstamt;
                                dr["sgstamt"] = bsgstamt;

                                tnos = tnos + bnos;
                                tqnty = tqnty + bqnty;
                                tbasamt = tbasamt + bbasamt;
                                tdisc1 = tdisc1 + bdisc1;
                                tdisc2 = tdisc2 + bdisc2;
                                ttaxable = ttaxable + btaxable;
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
                            i = istore;
                            int ino = 0;
                            while (auto1 == tbl.Rows[i]["autono"].ToString())
                            {
                                ino = ino + 1;
                                DataRow dr = IR.NewRow();
                                //dr["gstno"] = tbl.Rows[i]["prodcd"].ToString();
                                dr["slnm"] = tbl.Rows[i]["itstyle"].ToString();
                                if (itemrem == true && itmdtl == true) dr["itrem"] = tbl.Rows[i]["itrem"].ToString();
                                dr["hsncode"] = tbl.Rows[i]["hsncode"].ToString();
                                dr["uomcd"] = tbl.Rows[i]["uomcd"].ToString();
                                //if (cnt_bluom.Count() > 0)
                                //{
                                //    dr["bluomcd"] = tbl.Rows[i]["bluomcd"].ToString();
                                //}
                                if (cnt_blqnty.Count() > 0)
                                {
                                    dr["blqnty"] = tbl.Rows[i]["blqnty"];
                                }
                                if (tbl.Rows[i]["cancel"].ToString() != "Y")
                                {
                                    dr["nos"] = tbl.Rows[i]["nos"];
                                    dr["qnty"] = tbl.Rows[i]["qnty"];
                                    dr["rate"] = tbl.Rows[i]["rate"];
                                    dr["amt"] = tbl.Rows[i]["amt"] == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["amt"]);
                                    dr["disc1"] = tbl.Rows[i]["tddiscamt"];
                                    dr["disc2"] = tbl.Rows[i]["discamt"];
                                    dsc1 = tbl.Rows[i]["tddiscamt"] == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["tddiscamt"]);
                                    dsc2 = tbl.Rows[i]["discamt"] == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["discamt"]);
                                    dr["taxableval"] = Convert.ToDouble(tbl.Rows[i]["amt"]) - dsc1 - dsc2;
                                    dr["igstper"] = tbl.Rows[i]["igstper"];
                                    dr["igstamt"] = tbl.Rows[i]["igstamt"];
                                    dr["cgstper"] = tbl.Rows[i]["cgstper"];
                                    dr["cgstamt"] = tbl.Rows[i]["cgstamt"];
                                    dr["sgstper"] = tbl.Rows[i]["sgstper"];
                                    dr["sgstamt"] = tbl.Rows[i]["sgstamt"];
                                }
                                if (batchdtl == true && itmdtl == true) dr["batchno"] = tbl.Rows[i]["batchno"].ToString();
                                if (dr["celldesign"].ToString() != "") dr["celldesign"] = dr["celldesign"] + "^";
                                dr["celldesign"] = dr["celldesign"] + "qnty=~n,12," + tbl.Rows[i]["decimals"].retInt();
                                i += 1;

                                IR.Rows.Add(dr);
                                if (i > maxR) break;
                            }
                        }
                    }
                    DataRow dr3 = IR.NewRow();
                    dr3["dammy"] = "";
                    dr3["slnm"] = "Grand Totals";
                    dr3["nos"] = tnos;
                    dr3["qnty"] = tqnty;
                    dr3["amt"] = tbasamt;
                    if (SeparateAchead == true)
                    {
                        foreach (DataRow amtdr in amtDT.Rows)
                        {
                            dr3[amtdr["itcd"].ToString()] = amtdr["AmtItcdtotAmt"].retDbl();
                        }
                    }
                    dr3["disc1"] = tdisc1;
                    dr3["disc2"] = tdisc2;
                    dr3["taxableval"] = ttaxable;
                    dr3["igstamt"] = tigstamt;
                    dr3["cgstamt"] = tcgstamt;
                    dr3["sgstamt"] = tsgstamt;
                    dr3["tcsamt"] = ttcsamt;
                    dr3["roamt"] = troamt;
                    dr3["blamt"] = tblamt;
                    if (itmdtl == true && tbqnty != 0)
                    {
                        dr3["blqnty"] = tbqnty;
                    }
                    dr3["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 3px solid;";
                    IR.Rows.Add(dr3);
                    if (i <= maxR)
                    {
                        DataRow dr21 = IR.NewRow();
                        dr21["dammy"] = " ";
                        dr21["flag"] = " height:14px; ";
                        IR.Rows.Add(dr21);
                    }
                    if (txntag == "'SB'")
                    {
                        //IR.Columns.Remove("pblno");
                        //IR.Columns.Remove("pbldt");
                    }
                }
                #endregion
                //#region Report Product wise
                //else if (reptype == "Product")//Product Report for Godown
                //{
                //    HC.RepStart(IR, 2);
                //    HC.GetPrintHeader(IR, "docdt", "string", "d,10:dd/mm/yy", "Doc Date");
                //    HC.GetPrintHeader(IR, "docno", "string", "c,16", "Doc No");
                //    if (txntag == "'PB'")
                //    {
                //        HC.GetPrintHeader(IR, "pbldt", "string", "d,10:dd/mm/yy", "Ref Date");
                //        HC.GetPrintHeader(IR, "pblno", "string", "c,16", "Ref No");
                //    }
                //    HC.GetPrintHeader(IR, "slnm", "string", "c,35", "Party Name");
                //    HC.GetPrintHeader(IR, "itnm", "string", "c,35", "Item Name");
                //    HC.GetPrintHeader(IR, "packsize", "double", "n,7,6", "Pack");
                //    HC.GetPrintHeader(IR, "qnty", "double", "n,12,6", "Qnty");
                //    HC.GetPrintHeader(IR, "taxableval", "double", "n,10,2", "Taxable Amt");
                //    HC.GetPrintHeader(IR, "igstamt", "double", "n,10,2", "IGST Amt");
                //    HC.GetPrintHeader(IR, "cgstamt", "double", "n,10,2", "CGST Amt");
                //    HC.GetPrintHeader(IR, "sgstamt", "double", "n,10,2", "SGST Amt");
                //    HC.GetPrintHeader(IR, "netamt", "double", "n,10,2", "Net Amt");
                //    if (batchdtl == true) HC.GetPrintHeader(IR, "batchno", "string", "c,20", "Batch nos");
                //    HC.GetPrintHeader(IR, "prcdesc", "string", "c,20", "Price List");
                //    HC.GetPrintHeader(IR, "stkprcdesc", "string", "c,20", "Stk Price List");
                //    if (con_transprint == true) HC.GetPrintHeader(IR, "cslnm", "string", "c,40", "Consignee Name");
                //    if (con_transprint == true) HC.GetPrintHeader(IR, "trslnm", "string", "c,40", "Transporter Name");
                //    if (con_transprint == true) HC.GetPrintHeader(IR, "lrno", "string", "c,15", "Lr. No");
                //    if (con_transprint == true) HC.GetPrintHeader(IR, "lrdt", "string", "d,10:dd/mm/yy", "Lr. Date");
                //    if (orddetprint == true) HC.GetPrintHeader(IR, "ordno", "string", "c,50", "Order No");
                //    if (orddetprint == true) HC.GetPrintHeader(IR, "orddt", "string", "d,10:dd/mm/yy", "Order Date");
                //    HC.GetPrintHeader(IR, "docrem", "string", "c,20", "Rem");

                //    double btaxable = 0; double bnetamt = 0; double tnetamt = 0;
                //    while (i <= maxR)
                //    {
                //        auto1 = tbl.Rows[i]["autono"].ToString();
                //        istore = i;
                //        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                //        itmdtl = false;
                //        while (auto1 == tbl.Rows[i]["autono"].ToString())
                //        {
                //            var v = tbl.Rows[i]["slno"].ToString();
                //            if (Convert.ToDouble(tbl.Rows[i]["slno"]) < 10000)
                //            {
                //                if (itmdtl == true)
                //                {
                //                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                //                }
                //                itmdtl = true;
                //                string cancrem = "";
                //                if (tbl.Rows[i]["cancel"].ToString() == "Y") cancrem = "  (CANCELLED)";

                //                IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"] + cancrem;
                //                IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"] == DBNull.Value ? "" : tbl.Rows[i]["docdt"].ToString().Substring(0, 10).ToString();
                //                IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"];
                //                IR.Rows[rNo]["itnm"] = tbl.Rows[i]["itnm"];
                //                IR.Rows[rNo]["packsize"] = Convert.ToDouble(tbl.Rows[i]["packsize"]);
                //                if (tbl.Rows[i]["cancel"].ToString() != "Y")
                //                {
                //                    btaxable = Convert.ToDouble(tbl.Rows[i]["basamt"]) - Convert.ToDouble(tbl.Rows[i]["stddiscamt"]) - Convert.ToDouble(tbl.Rows[i]["discamt"]);
                //                    bnetamt = btaxable + Convert.ToDouble(tbl.Rows[i]["igstamt"]) + Convert.ToDouble(tbl.Rows[i]["cgstamt"]) + Convert.ToDouble(tbl.Rows[i]["sgstamt"]);
                //                    IR.Rows[rNo]["qnty"] = Convert.ToDouble(tbl.Rows[i]["qnty"]);
                //                    IR.Rows[rNo]["taxableval"] = btaxable;
                //                    IR.Rows[rNo]["igstamt"] = Convert.ToDouble(tbl.Rows[i]["igstamt"]);
                //                    IR.Rows[rNo]["cgstamt"] = Convert.ToDouble(tbl.Rows[i]["cgstamt"]);
                //                    IR.Rows[rNo]["sgstamt"] = Convert.ToDouble(tbl.Rows[i]["sgstamt"]);
                //                    IR.Rows[rNo]["netamt"] = bnetamt;
                //                }
                //                IR.Rows[rNo]["prcdesc"] = tbl.Rows[i]["prcdesc"].ToString();//hook
                //                if (txntag == "'PB'")
                //                {
                //                    IR.Rows[rNo]["pblno"] = tbl.Rows[i]["pblno"].ToString();//hook
                //                    IR.Rows[rNo]["pbldt"] = tbl.Rows[i]["pbldt"] == DBNull.Value ? "" : tbl.Rows[i]["pbldt"].ToString().Substring(0, 10).ToString();//hook
                //                }

                //                if (rsStkPrcDesc.Rows.Count > 0)
                //                {
                //                    var DATA = (from DataRow DR in rsStkPrcDesc.Rows where DR["autono"].ToString() == auto1 select new { STKPRCDEC = DR["stkprcdesc"].ToString(), DOCREM = DR["docrem"].ToString(), }).ToList();

                //                    if (DATA.Count > 0)
                //                    {
                //                        IR.Rows[rNo]["stkprcdesc"] = DATA[0].STKPRCDEC;
                //                        IR.Rows[rNo]["DOCREM"] = DATA[0].DOCREM;
                //                    }
                //                }
                //                if (batchdtl == true) IR.Rows[rNo]["batchno"] = tbl.Rows[i]["batchno"];
                //                if (con_transprint == true)
                //                {
                //                    IR.Rows[rNo]["cslnm"] = tbl.Rows[i]["cslnm"].ToString();
                //                    IR.Rows[rNo]["trslnm"] = tbl.Rows[i]["trslnm"].ToString();
                //                    IR.Rows[rNo]["lrno"] = tbl.Rows[i]["lrno"].ToString();
                //                    IR.Rows[rNo]["lrdt"] = tbl.Rows[i]["lrdt"].retDateStr();
                //                }
                //                if (orddetprint == true)
                //                {
                //                    IR.Rows[rNo]["ordno"] = tbl.Rows[i]["ordrefno"].ToString();
                //                    IR.Rows[rNo]["orddt"] = tbl.Rows[i]["ordrefdt"].ToString();
                //                }
                //                if (IR.Rows[rNo]["celldesign"].ToString() != "") IR.Rows[rNo]["celldesign"] = IR.Rows[rNo]["celldesign"] + "^";
                //                IR.Rows[rNo]["celldesign"] = IR.Rows[rNo]["celldesign"] + "qnty=~n,12," + tbl.Rows[i]["decimals"].retInt();

                //                if (tbl.Rows[i]["cancel"].ToString() != "Y")
                //                {
                //                    tqnty = tqnty + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                //                    ttaxable = ttaxable + btaxable;
                //                    tigstamt = tigstamt + Convert.ToDouble(tbl.Rows[i]["igstamt"]);
                //                    tcgstamt = tcgstamt + Convert.ToDouble(tbl.Rows[i]["cgstamt"]);
                //                    tsgstamt = tsgstamt + Convert.ToDouble(tbl.Rows[i]["sgstamt"]);
                //                    tnetamt = tnetamt + bnetamt;
                //                }
                //            }
                //            i = i + 1;
                //            if (i > maxR) break;
                //        }
                //        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                //        IR.Rows[rNo]["slnm"] = " ";
                //        IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;"; // "font-weight:bold;font-size:13px;border-top: 1px solid;";
                //    }
                //    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                //    IR.Rows[rNo]["itnm"] = "Totals";
                //    IR.Rows[rNo]["qnty"] = tqnty;
                //    IR.Rows[rNo]["taxableval"] = ttaxable;
                //    IR.Rows[rNo]["igstamt"] = tigstamt;
                //    IR.Rows[rNo]["cgstamt"] = tcgstamt;
                //    IR.Rows[rNo]["sgstamt"] = tsgstamt;
                //    IR.Rows[rNo]["netamt"] = tnetamt;
                //    IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 3px solid;";
                //}
                //#endregion
                //#region Annexure List
                //else if (reptype == "Annex")
                //{
                //    HC.RepStart(IR, 3);
                //    HC.GetPrintHeader(IR, "docdt", "string", "d,10:dd/mm/yy", ";Doc Date");
                //    HC.GetPrintHeader(IR, "docno", "string", "c,16", ";Doc No");
                //    HC.GetPrintHeader(IR, "slnm", "string", "c,35", "Party Name");
                //    HC.GetPrintHeader(IR, "district", "string", "c,20", "City");
                //    HC.GetPrintHeader(IR, "gstno", "string", "c,35", "GST #");
                //    HC.GetPrintHeader(IR, "blamt", "double", "n,17,2:####,##,##,##0.00", "Bill Amt");
                //    while (i <= maxR)
                //    {
                //        auto1 = tbl.Rows[i]["autono"].ToString();
                //        istore = i;
                //        bldtl = true;
                //        while (auto1 == tbl.Rows[i]["autono"].ToString())
                //        {
                //            while (auto1 == tbl.Rows[i]["autono"].ToString())
                //            {
                //                i = i + 1;
                //                if (i > maxR) break;
                //            }
                //            i = i - 1;
                //            DataRow dr = IR.NewRow();
                //            if (bldtl == true)
                //            {
                //                string cancrem = "";
                //                if (tbl.Rows[i]["cancel"].ToString() == "Y") cancrem = "  (CANCELLED)";

                //                dr["docdt"] = tbl.Rows[i]["docdt"] == DBNull.Value ? "" : tbl.Rows[i]["docdt"].ToString().Substring(0, 10).ToString();
                //                dr["docno"] = tbl.Rows[i]["docno"].ToString() + cancrem;
                //                dr["slnm"] = tbl.Rows[i]["slnm"].ToString();
                //                dr["district"] = tbl.Rows[i]["district"].ToString();
                //                dr["gstno"] = tbl.Rows[i]["gstno"].ToString();
                //                if (tbl.Rows[i]["cancel"].ToString() != "Y")
                //                {
                //                    dr["blamt"] = Convert.ToDouble(tbl.Rows[i]["blamt"]);
                //                    tblamt = tblamt + Convert.ToDouble(tbl.Rows[i]["blamt"]);
                //                }
                //                bldtl = false;
                //            }
                //            IR.Rows.Add(dr);
                //            i = i + 1;
                //            if (i > maxR) break;
                //        }
                //    }
                //    DataRow dr3 = IR.NewRow();
                //    dr3["dammy"] = "";
                //    dr3["slnm"] = "Grand Totals";
                //    dr3["blamt"] = tblamt;
                //    dr3["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 3px solid;";
                //    IR.Rows.Add(dr3);
                //    if (i <= maxR)
                //    {
                //        DataRow dr21 = IR.NewRow();
                //        dr21["dammy"] = " ";
                //        dr21["flag"] = " height:14px; ";
                //        IR.Rows.Add(dr21);
                //    }
                //}
                //#endregion
                pghdr1 = regdsp + " from " + fdt + " to " + tdt;
                string repname = regdsp + " Register";
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
        }
        public ActionResult Rep_Reg_Stock(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);

                string fdt = VE.FDT.retDateStr(), tdt = VE.TDT.retDateStr();
                string reptype = VE.TEXTBOX1;
                string GODOWN = VE.TEXTBOX3.retStr();

                string itgrpcd = "";
                if (FC.AllKeys.Contains("ITGRPCDvalue")) itgrpcd = CommFunc.retSqlformat(FC["ITGRPCDvalue"].ToString());


                string txntag = "SALES";

                switch (reptype)
                {
                    case "Stock Transfer":
                        txntag = "'SO'"; break;
                    case "Stock Conversion":
                        txntag = "'SC'"; break;
                    case "Stock Adjustment":
                        txntag = "'SA'"; break;
                    default: txntag = ""; break;
                }


                string query = "";
                query = "";
                query += "select distinct a.autono, c.docno, c.docdt, c.doccd, a.itgrpcd, g.itgrpnm, b.slno,decode(b.stkdrcr,'D','Add','Less')stkdrcr, b.itcd, e.itnm, f.uomnm,f.decimals, ";
                query += "b.qnty, b.rate, b.basamt, c.loccd || a.gocd locagocd, c.loccd || ', ' || j.gonm locagonm, ";
                query += "d.tloccd || d.tgocd tlocagocd, d.tloccd || ', ' || l.gonm tlocagonm,m.docrem ";
                query += "from " + CommVar.CurSchema(UNQSNO) + ".t_txn a, " + CommVar.CurSchema(UNQSNO) + ".t_txndtl b, " + CommVar.CurSchema(UNQSNO) + ".t_cntrl_hdr c, " + CommVar.CurSchema(UNQSNO) + ".t_stktrnf d, ";
                query += "" + CommVar.CurSchema(UNQSNO) + ".m_sitem e, " + CommVar.FinSchema(UNQSNO) + ".m_uom f, " + CommVar.CurSchema(UNQSNO) + ".m_group g, ";
                query += "" + CommVar.FinSchema(UNQSNO) + ".m_loca i, " + CommVar.FinSchema(UNQSNO) + ".m_godown j, " + CommVar.FinSchema(UNQSNO) + ".m_loca k, " + CommVar.CurSchema(UNQSNO) + ".m_godown l, " + CommVar.CurSchema(UNQSNO) + ".t_txnoth m ";
                query += "where a.autono = b.autono and a.autono = c.autono and a.autono = d.autono(+) and ";
                query += "b.itcd = e.itcd(+) and e.uomcd = f.uomcd(+) and a.itgrpcd = g.itgrpcd(+) and ";
                query += "c.loccd = i.loccd(+) and a.gocd = j.gocd(+) and d.tloccd = k.loccd(+) and d.tgocd = l.gocd(+) and a.autono = m.autono(+) ";
                query += "and a.itgrpcd in (" + itgrpcd + ") and a.doctag in ( " + txntag + ")  ";
                query += "and c.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and c.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy')  ";
                if (VE.FDOCNO.retStr() != "") query += " and a.docno >= '" + VE.FDOCNO + "'  ";
                if (VE.TDOCNO.retStr() != "") query += " and a.docno <= '" + VE.TDOCNO + "'  ";
                if (GODOWN != "") query += " and (a.gocd like '%" + GODOWN + "%' or  d.tgocd like '%" + GODOWN + "%')  ";

                query += "and nvl(c.cancel, 'N')= 'N' and c.compcd = '" + CommVar.Compcd(UNQSNO) + "' ";
                if (reptype != "Stock Transfer") query += "and c.loccd = '" + CommVar.Loccd(UNQSNO) + "'  ";
                if (reptype == "Stock Transfer") { query += "order by a.itgrpcd, g.itgrpnm,locagonm,docdt, docno, slno "; }
                else { query += "order by a.itgrpcd, g.itgrpnm,docdt, docno, slno "; }



                DataTable tbl = MasterHelp.SQLquery(query);
                if (tbl.Rows.Count == 0)
                {
                    return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "Records not found !!" });
                }

                DataTable IR = new DataTable("mstrep");
                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                Int32 i = 0, maxR = tbl.Rows.Count - 1;



                HC.RepStart(IR, 2);
                HC.GetPrintHeader(IR, "docdt", "string", "d,10:dd/mm/yy", "Date");
                HC.GetPrintHeader(IR, "docno", "string", "c,16", "Doc No");
                if (reptype == "Stock Transfer")
                {
                    HC.GetPrintHeader(IR, "tlocagonm", "string", "c,35", "To Location & To Godown");
                }
                else
                {
                    HC.GetPrintHeader(IR, "locagonm", "string", "c,35", "Location & Godown");
                    HC.GetPrintHeader(IR, "addless", "string", "c,6", "Add/Less");
                }
                HC.GetPrintHeader(IR, "itcd", "string", "c,16", "Item Code");
                HC.GetPrintHeader(IR, "itnm", "string", "c,35", "Item Name");
                HC.GetPrintHeader(IR, "qnty", "double", "n,12,6", "Qnty");
                HC.GetPrintHeader(IR, "uomnm", "string", "c,4", "Uom");

                if (reptype == "Stock Transfer")
                {
                    HC.GetPrintHeader(IR, "rate", "double", "n,10,2", "Rate");
                    HC.GetPrintHeader(IR, "amt", "double", "n,17,2:####,##,##,##0.00", "Value");
                }
                else
                {
                    HC.GetPrintHeader(IR, "docrem", "string", "c,35", "Remarks");
                }
                if (reptype == "Stock Transfer")
                {
                    #region Stock Transfer
                    while (i <= maxR)
                    {
                        string itgrp_cd = tbl.Rows[i]["itgrpcd"].ToString();

                        DataRow dr = IR.NewRow();
                        dr["Dammy"] = "Group : " + tbl.Rows[i]["itgrpnm"].ToString() + " [" + itgrp_cd + "] ";
                        dr["flag"] = "font-weight:bold;font-size:13px;background-color:#e8dd8e;border:1px solid gray; ";
                        IR.Rows.Add(dr);
                        while (itgrp_cd == tbl.Rows[i]["itgrpcd"].ToString())
                        {
                            string locagonm = tbl.Rows[i]["locagonm"].ToString();
                            DataRow dr1 = IR.NewRow();
                            dr1["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + "From Location & From Godown :  </span>" + tbl.Rows[i]["locagonm"].ToString();
                            dr1["flag"] = "font-weight:bold;font-size:13px;background-color:#cce6ff;border:1.2px solid black;";
                            IR.Rows.Add(dr1);

                            while (itgrp_cd == tbl.Rows[i]["itgrpcd"].ToString() && locagonm == tbl.Rows[i]["locagonm"].ToString())
                            {
                                DataRow dr2 = IR.NewRow();
                                dr2["docdt"] = tbl.Rows[i]["docdt"].retStr() == "" ? "" : tbl.Rows[i]["docdt"].retStr().Remove(10);
                                dr2["docno"] = tbl.Rows[i]["docno"].retStr();
                                dr2["tlocagonm"] = tbl.Rows[i]["tlocagonm"].retStr();
                                dr2["itcd"] = tbl.Rows[i]["itcd"].retStr();
                                dr2["itnm"] = tbl.Rows[i]["itnm"].retStr();
                                dr2["qnty"] = tbl.Rows[i]["qnty"].retDbl();
                                dr2["uomnm"] = tbl.Rows[i]["uomnm"].retStr();
                                dr2["rate"] = tbl.Rows[i]["rate"].retDbl();
                                dr2["amt"] = tbl.Rows[i]["amt"].retDbl();
                                dr2["celldesign"] = dr["celldesign"] + "qnty=~n,12," + tbl.Rows[i]["decimals"].retInt();
                                IR.Rows.Add(dr2);
                                i = i + 1;
                                if (i > maxR) break;
                            }
                            if (i > maxR) break;
                        }
                    }
                    #endregion Transfer
                }
                else
                {
                    #region Stock Conversion/Adjustment
                    while (i <= maxR)
                    {
                        string itgrp_cd = tbl.Rows[i]["itgrpcd"].ToString();

                        DataRow dr = IR.NewRow();
                        dr["Dammy"] = "Group : " + tbl.Rows[i]["itgrpnm"].ToString() + " [" + itgrp_cd + "] ";
                        dr["flag"] = "font-weight:bold;font-size:13px;background-color:#e8dd8e;border:1px solid gray; ";
                        IR.Rows.Add(dr);
                        while (itgrp_cd == tbl.Rows[i]["itgrpcd"].ToString())
                        {
                            DataRow dr1 = IR.NewRow();
                            dr1["docdt"] = tbl.Rows[i]["docdt"].retStr() == "" ? "" : tbl.Rows[i]["docdt"].retStr().Remove(10);
                            dr1["docno"] = tbl.Rows[i]["docno"].ToString();
                            dr1["locagonm"] = tbl.Rows[i]["locagonm"].ToString();
                            dr1["addless"] = tbl.Rows[i]["stkdrcr"].ToString();
                            dr1["itcd"] = tbl.Rows[i]["itcd"].ToString();
                            dr1["itnm"] = tbl.Rows[i]["itnm"].ToString();
                            dr1["qnty"] = tbl.Rows[i]["qnty"].ToString();
                            dr1["uomnm"] = tbl.Rows[i]["uomnm"].ToString();
                            dr1["docrem"] = tbl.Rows[i]["docrem"].ToString();
                            dr1["celldesign"] = dr["celldesign"] + "qnty=~n,12," + tbl.Rows[i]["decimals"].retInt();
                            IR.Rows.Add(dr1);
                            i = i + 1;
                            if (i > maxR) break;
                        }
                    }
                    #endregion Stock Conversion/Adjustment
                }

                string regdsp = "";
                regdsp = reptype + " Register";
                string pghdr1 = regdsp + " from " + fdt + " to " + tdt;

                PV = HC.ShowReport(IR, regdsp, pghdr1, "", true, true, "P", false);

                TempData[regdsp] = PV;
                TempData[regdsp + "xxx"] = IR;
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = regdsp });
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