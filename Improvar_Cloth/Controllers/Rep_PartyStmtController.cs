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
    public class Rep_PartyStmtController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string fdt = ""; string tdt = ""; bool showpacksize = false, showrate = false;
        string modulecode = CommVar.ModuleCode();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_PartyStmt
        public ActionResult Rep_PartyStmt()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Party or Item wise Analysis";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    //string selgrp = MasterHelp.GetUserITGrpCd().Replace("','", ",");
                    //string[] selgrpcd = selgrp.Split(','); 
                    string com = CommVar.Compcd(UNQSNO);
                    //var items = (from i in DB.M_SITEM select new DropDown_list_text() { value = i.ITCD, text1 = i.ITNM, text3 = i.PACKSIZE.Value }).ToList();
                    var items = (from i in DB.M_SITEM select new DropDown_list_text() { value = i.ITCD, text1 = i.ITNM }).ToList();


                    //VE.DropDown_list_ITGRP = DropDownHelp.GetItgrpcdforSelection(selgrp);
                    VE.DropDown_list_ITGRP = (from i in DB.M_GROUP select new DropDown_list_ITGRP() { value = i.ITGRPCD, text = i.ITGRPNM }).OrderBy(s => s.text).ToList();
                    VE.Itgrpnm = MasterHelp.ComboFill("itgrpcd", VE.DropDown_list_ITGRP, 0, 1);

                    //VE.DropDown_list1 = (from i in DB.M_ITEMPLIST
                    //                     join j in DB.T_BATCHMST on i.ITMPRCCD equals j.ITMPRCCD
                    //                     where (selgrpcd.Contains(i.ITGRPCD))
                    //                     select new DropDown_list1()
                    //                     { value = i.ITMPRCCD, text = i.PRCDESC }).Distinct().OrderBy(s => s.text).ToList();
                    //VE.DropDown_list2 = (from i in DBF.M_SUBLEG_GRPHDR
                    //                     select new DropDown_list2()
                    //                     { value = i.GRPCD, text = i.GRPNM }).Distinct().OrderBy(s => s.text).ToList();
                    VE.DropDown_list3 = new List<DropDown_list3>() {
                    new DropDown_list3{text="Item Group",value="ITGRPCD" },
                    new DropDown_list3{text="Item",value="ITCD" },
                    new DropDown_list3{text="Month",value="MONTH" }
                        };
                    //VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection(selgrp);
                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);

                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("D,C");
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);

                    //VE.DropDown_list_BRGRP = DropDownHelp.GetBrgrpcdforSelection(selgrp);
                    //VE.Brgrpnm = MasterHelp.ComboFill("brgrpcd", VE.DropDown_list_BRGRP, 0, 1);
                    VE.DropDown_list_SubLegGrp = DropDownHelp.GetSubLegGrpforSelection();
                    VE.SubLeg_Grp = MasterHelp.ComboFill("slcdgrpcd", VE.DropDown_list_SubLegGrp, 0, 1);
                    VE.FDT = CommVar.FinStartDate(UNQSNO); VE.TDT = CommVar.CurrDate(UNQSNO);
                    VE.DropDown_list = (from i in DBF.M_LOCA
                                         where i.COMPCD == com
                                         select new DropDown_list() { value = i.LOCCD, text = i.LOCNM }).Distinct().OrderBy(s => s.text).ToList();// location
                    VE.TEXTBOX4 = MasterHelp.ComboFill("loccd", VE.DropDown_list, 0, 1);
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
        public ActionResult Rep_PartyStmt(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                fdt = VE.FDT.retDateStr(); tdt = VE.TDT.retDateStr();

                string RateQntyBAg = "Q";
                //if (FC["RATEQNTYBAG"].ToString() == "BAGS") RateQntyBAg = "B";
                //else RateQntyBAg = "Q";
                string txntag = ""; string txnrettag = "", selloccd = "" ;
                string reptype = FC["Reptype"].ToString();
                string repon = FC["PartyItem"].ToString();
                //string grpcd = VE.TEXTBOX2;
                string groupingwith = VE.TEXTBOX3;
                bool WithoutParty = VE.Checkbox3;
                string selslcdgrpcd = "";
                if (FC.AllKeys.Contains("slcdgrpcdvalue")) selslcdgrpcd = CommFunc.retSqlformat(FC["slcdgrpcdvalue"].ToString());
                if (FC.AllKeys.Contains("loccdvalue")) selloccd = FC["loccdvalue"].retSqlformat();
                //if (grpcd.retStr() != "")
                //{
                // string   sql1 = "select a.itgrpcd, a.class1cd from " + scm1 + ".m_group a where a.class1cd in ( ";
                //    sql1 += "select b.class1cd ";
                //    sql1 += "from " + scmf + ".m_subleg_grphdr a, " + scmf + ".m_subleg_grpclass1 b ";
                //    sql1 += "where a.grpcd=b.grpcd(+) and a.grpcd='" + grpcd + "') ";
                //    DataTable tmp = MasterHelp.SQLquery(sql1);
                //    selitgrpcd = string.Join(",", (from DataRow dr in tmp.Rows select dr["itgrpcd"].ToString()).Distinct()).retSqlformat();
                //}
                switch (FC["SalPur"].ToString())
                {
                    case "S":
                        txntag = "'SB'";
                        if (VE.Checkbox1 == true) txnrettag = ",'SR'";
                        break;
                    case "P":
                        txntag = "'PB'";
                        if (VE.Checkbox1 == true) txnrettag = ",'PR'";
                        break;
                    default: txntag = ""; break;
                }
                txntag = txntag + txnrettag;

                string selitcd = "", unselitcd = "", plist = "", selslcd = "", unselslcd = "", selitgrpcd = "", selbrgrpcd = "";
                if (FC.AllKeys.Contains("slcdgrpcdvalue")) selslcdgrpcd = CommFunc.retSqlformat(FC["slcdgrpcdvalue"].ToString());
                if (FC.AllKeys.Contains("itgrpcdvalue")) selitgrpcd = CommFunc.retSqlformat(FC["itgrpcdvalue"].ToString());
                //if (FC.AllKeys.Contains("brgrpcdvalue")) selbrgrpcd = CommFunc.retSqlformat(FC["brgrpcdvalue"].ToString());

                if (FC.AllKeys.Contains("plist")) plist = CommFunc.retSqlformat(FC["plist"].ToString());

                if (FC.AllKeys.Contains("itcdvalue")) selitcd = CommFunc.retSqlformat(FC["itcdvalue"].ToString());
                if (FC.AllKeys.Contains("itcdunselvalue")) unselitcd = CommFunc.retSqlformat(FC["itcdunselvalue"].ToString());

                if (FC.AllKeys.Contains("slcdvalue")) selslcd = CommFunc.retSqlformat(FC["slcdvalue"].ToString());
                if (FC.AllKeys.Contains("slcdunselvalue")) unselslcd = CommFunc.retSqlformat(FC["slcdunselvalue"].ToString());

                string taxfld = "";
                if (VE.Checkbox4 == true) taxfld = " + nvl(a.igstamt,0)+nvl(a.cgstamt,0)+nvl(a.sgstamt,0) ";
                if (VE.Checkbox5 == true) showrate = true;
                string sql = "";
                int maxQ = 0;
                if (CommVar.LastYearSchema(UNQSNO) != "") maxQ = 1;
                for (int q = 0; q <= maxQ; q++)
                {
                    if (q == 1 && CommVar.LastYearSchema(UNQSNO) != "")
                    {
                        sql += "union all ";
                        scm1 = CommVar.LastYearSchema(UNQSNO);
                    }
                    sql += "select a.autono, a.doctag, a.doccd, a.docdt, to_char(a.docdt, 'MON-YYYY') docmonth, a.itgrpcd, ";
                    if (FC["SalPur"].ToString() == "S") sql += "a.docno, ";
                    else sql += "a.prefno docno, ";
                    if (repon == "P")
                    {
                        sql += "a.slcd cd, a.slnm nm, a.shortnm snm,a.conslcd,a.conslnm conslnm, ";
                        sql += "b.itcd ocd, b.itnm onm, b.prodgrpcd osnm, ";
                    }
                    else
                    {
                        sql += "a.slcd ocd, a.slnm onm, a.shortnm osnm,a.conslcd,a.conslnm conslnm, ";
                        sql += "b.itcd cd, b.itnm nm, b.prodgrpcd snm, ";
                    }
                    sql += "b.stkdrcr,b.styleno, b.uomnm, b.decimals, nvl(b.nos,0) nos, nvl(b.qnty,0) qnty, nvl(b.basamt,0) basamt, b.othramt, ";
                    sql += "b.batchno from ";
                    sql += "( select a.autono, a.doctag, b.doccd, b.docno, b.docdt, e.itgrpcd, e.class1cd, ";
                    sql += "a.slcd||nvl(e.class1cd,' ') slcdclass1cd, a.prefno, a.prefdt, ";
                    sql += "a.slcd, c.slnm,a.conslcd,f.slnm conslnm,c.shortnm ";
                    sql += "from " + scm1 + ".t_txn a, " + scm1 + ".t_cntrl_hdr b, " + scmf + ".m_subleg c, " + scm1 + ".m_group e," + scmf + ".m_subleg f " ;
                    sql += "where a.autono=b.autono(+) and a.slcd=c.slcd(+) and a.conslcd=f.slcd(+) and ";
                        //a.itgrpcd=e.itgrpcd(+) and ";
                    sql += "b.compcd='" + COM + "' and ";
                    if (selloccd == "") sql = sql + "b.loccd='" + LOC + "' and ";
                    else sql = sql + "b.loccd in (" + selloccd + ") and ";
                    sql +=" nvl(b.cancel,'N') = 'N' and ";
                    sql += "b.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and b.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and  ";
                    if (selitgrpcd.retStr() != "") sql += "e.itgrpcd in (" + selitgrpcd + ") and ";
                    sql += "a.doctag in ( " + txntag + ") ";
                    sql += ") a, ( ";
                    sql += "select a.autono, a.slno, a.itcd,b.prodgrpcd, b.itgrpcd, b.itnm, a.prccd, a.stkdrcr,b.styleno, c.uomnm, c.decimals, ";
                    sql += "sum(nvl(d.nos,a.nos)) nos, sum(nvl(d.qnty,a.qnty)) qnty, ";
                    sql += "listagg(e.batchno,',') within group (order by d.autono, d.slno) batchno, ";
                    //sql += "a.basamt-nvl(a.stddiscamt,0)-nvl(a.discamt,0) basamt, nvl(a.othramt,0) othramt ";
                    //sql += "sum(decode(nvl(d.rslno,0),1,a.basamt-nvl(a.stddiscamt,0)-nvl(a.discamt,0),0)) basamt, ";
                    //sql += "sum(decode(nvl(d.rslno,0),1,nvl(a.othramt,0),0)) othramt ";
                    sql += "sum((case nvl(d.txnslno,0) when 0 then a.amt-nvl(a.scmdiscamt,0)-nvl(a.tddiscamt,0)-nvl(a.discamt,0)" + taxfld + " when 1 then a.amt-nvl(a.scmdiscamt,0)-nvl(a.tddiscamt,0)-nvl(a.discamt,0)" + taxfld + " end)) basamt, ";
                    sql += "sum((case nvl(d.txnslno,0) when 0 then nvl(a.othramt,0) when 1 then nvl(a.othramt,0) end )) othramt ";
                    sql += "from " + scm1 + ".t_txndtl a, " + scm1 + ".m_sitem b, " + scmf + ".m_uom c, ";
                    sql += scm1 + ".t_batchdtl d, " + scm1 + ".t_batchmst e  ";
                    sql += "where a.itcd=b.itcd(+) and b.uomcd=c.uomcd(+) and a.autono=d.autono(+) and a.slno=d.txnslno(+) and  ";
                    sql += "d.autono=e.autono(+) ";
                    sql += "group by a.autono, a.slno, a.itcd, b.prodgrpcd,b.itgrpcd, b.itnm, a.prccd, a.stkdrcr,b.styleno, c.uomnm, c.decimals ";
                    //sql += "a.basamt-nvl(a.stddiscamt,0)-nvl(a.discamt,0) , nvl(a.othramt,0) ";
                    sql += ") b, ";

                    sql += "(select a.grpcd, a.parentcd, c.slcdgrpnm||'  '||b.slcdgrpnm parentnm, ";
                    sql += "a.grpcdfull, a.slcdgrpnm, a.class1cd, a.slcd, a.slcdclass1cd from ";

                    sql += "(select a.grpcd, a.slcdgrpcd parentcd, a.parentcd rootcd, a.grpcdfull, a.slcdgrpnm, b.class1cd, a.slcd, ";
                    sql += "a.slcd||nvl(b.class1cd,' ') slcdclass1cd ";
                    sql += "from " + scmf + ".m_subleg_grp a, " + scmf + ".m_subleg_grpclass1 b ";
                    sql += "where a.grpcd=b.grpcd(+) and a.slcd is not null ) a, ";

                    sql += "(select distinct a.slcdgrpcd, a.slcdgrpnm, a.grpcdfull ";
                    sql += "from " + scmf + ".m_subleg_grp a where a.slcd is null) b, ";

                    sql += "(select distinct a.slcdgrpcd, a.slcdgrpnm, a.grpcdfull ";
                    sql += "from " + scmf + ".m_subleg_grp a where a.slcd is null) c ";

                    sql += "where a.rootcd = c.slcdgrpcd(+) and a.parentcd=b.slcdgrpcd(+) ) s ";

                    sql += "where a.autono=b.autono(+) and a.slcdclass1cd = s.slcdclass1cd(+) ";
                    if (selitcd.retStr() != "") sql += "and b.itcd in (" + selitcd + ") ";
                    if (unselitcd.retStr() != "") sql += "and b.itcd not in (" + unselitcd + ") ";
                    if (selitcd.retStr() != "") sql += "and b.itcd in (" + selitcd + ") ";
                    if (selslcd.retStr() != "") sql += "and a.slcd in (" + selslcd + ") ";
                    if (unselslcd.retStr() != "") sql += "and a.slcd not in (" + unselslcd + ") ";
                    if (plist.retStr() != "") sql += "and b.prccd in (" + plist + ") ";
                    if (selslcdgrpcd.retStr() != "") sql += "and (s.parentcd in (" + selslcdgrpcd + ") ) ";
                }
                if (reptype == "D") sql += " order by nm,cd,docdt,docno,docno,autono ";
                else if (reptype == "G" && groupingwith == "MONTH") sql += " order by docdt,nm,cd ";
                else if ((reptype == "S"|| reptype == "SS") && VE.Checkbox6 == true) sql += " order by nm,cd,docdt,onm,ocd,docno,autono ";
                else sql += " order by nm,cd,onm,ocd,docdt,docno,autono ";

                DataTable tbl = MasterHelp.SQLquery(sql);
                if (tbl.Rows.Count == 0) return Content("no records..");

                //if (Convert.ToDouble(tbl.Rows[0]["packsize"]) == 0) showpacksize = false; else showpacksize = true;

                if (FC["Reptype".ToString()] == "D")
                {
                    return ReportDtl(tbl, reptype, repon, RateQntyBAg, "", VE.Checkbox2, VE.Checkbox6);
                }
                else if (reptype == "G")
                {
                    //if (grpcd == null) { return Content("Please select Group for analysis !!"); }
                    if (repon != "P") { return Content("Please Checked Report on Party wise analysis !!"); }
                    return DownloadExcel(tbl, "", groupingwith, WithoutParty, null);
                }
                else
                {
                    return ReportSumm(tbl, reptype, repon, RateQntyBAg, "", VE.Checkbox6);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public ActionResult ReportDtl(DataTable tbl, string reptype, string repon, string rateqntybag, string pghdr, bool batchprint, bool monthtotal)
        {
            try
            {
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
                string qdsp = "";
                if (rateqntybag == "B") qdsp = "n,12";
                else qdsp = "n,12,4";

                string dsp1 = "";
                //if (repon == "I") dsp1 = "Party"; else dsp1 = "Item";
                if (repon == "I") dsp1 = "Item"; else dsp1 = "Party";
                HC.RepStart(IR, 3);
              
             
                HC.GetPrintHeader(IR, "cd", "string", "c,10", "Code");
                HC.GetPrintHeader(IR, "nm", "string", "c,35", dsp1 + " Name");
                HC.GetPrintHeader(IR, "conslnm", "string", "c,35", "Consignee Name");
                HC.GetPrintHeader(IR, "docdt", "string", "c,10", "Document Date");
                HC.GetPrintHeader(IR, "docno", "string", "c,10", "Document No");
                HC.GetPrintHeader(IR, "snm", "string", "c,15", "prodgrpcd");
                //HC.GetPrintHeader(IR, "docno", "string", "c,16", "Doc No");

                //if (showpacksize == true) HC.GetPrintHeader(IR, "packsize", "double", "n,10,6", "P/Size");
                HC.GetPrintHeader(IR, "uom", "string", "c,4", "uom");
                HC.GetPrintHeader(IR, "qnty", "double", qdsp, "Qnty");
                if (showrate == true) HC.GetPrintHeader(IR, "rate", "double", qdsp, "Rate");
                HC.GetPrintHeader(IR, "amt", "double", "n,12,2", "Value");
                if (batchprint == true) HC.GetPrintHeader(IR, "batchno", "string", "c,35", "Batch Nos");

                double qty, amt = 0;
                double tqty, tamt = 0;
                double dbqty = 0;

                tqty = 0; tamt = 0;

                Int32 rNo = 0;

                // Report begins
                i = 0; maxR = tbl.Rows.Count - 1;

                while (i <= maxR)
                {
                    chkval = tbl.Rows[i]["cd"].ToString();
                    qty = 0; amt = 0;

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["cd"] = tbl.Rows[i]["cd"].ToString();
                    IR.Rows[rNo]["nm"] = tbl.Rows[i]["nm"].ToString();
                    IR.Rows[rNo]["snm"] = tbl.Rows[i]["snm"].ToString();
                    IR.Rows[rNo]["celldesign"] = "cd=font-weight:bold;font-size:13px;^nm=font-weight:bold;font-size:13px;^snm=font-weight:bold;font-size:13px; ";
                    while (tbl.Rows[i]["cd"].ToString() == chkval)
                    {
                        //chkval1 = tbl.Rows[i]["ocd"].ToString();
                        double monthamt = 0, monthqnty = 0;
                        chkval2 = tbl.Rows[i]["docmonth"].ToString();
                        //double bqnty, bamt, namt, mult = 0;
                        //bqnty = 0; bamt = 0; namt = 0; mult = 0;
                        while (tbl.Rows[i]["cd"].ToString() == chkval && tbl.Rows[i]["docmonth"].ToString() == chkval2)//monthly condition
                        {
                            chkval1 = tbl.Rows[i]["ocd"].ToString();
                            double bqnty, bamt, namt, mult = 0;
                            bqnty = 0; bamt = 0; namt = 0; mult = 0;
                            while (tbl.Rows[i]["cd"].ToString() == chkval && tbl.Rows[i]["ocd"].ToString() == chkval1 && tbl.Rows[i]["docmonth"].ToString() == chkval2)
                            {
                                switch (tbl.Rows[i]["doctag"].ToString())
                                {
                                    case "SB":
                                    case "PB":
                                        mult = 1;
                                        break;
                                    case "SR":
                                    case "PR":
                                        mult = -1;
                                        break;
                                }
                                if (rateqntybag == "B") dbqty = Convert.ToDouble(tbl.Rows[i]["nos"]);
                                else dbqty = Convert.ToDouble(tbl.Rows[i]["qnty"]) * mult;
                                namt = Convert.ToDouble(tbl.Rows[i]["basamt"]) * mult;

                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["cd"] = tbl.Rows[i]["ocd"].ToString();
                                IR.Rows[rNo]["nm"] = tbl.Rows[i]["onm"].ToString();
                                //IR.Rows[rNo]["conslnm"] = tbl.Rows[i]["conslnm"].ToString()+"["+ tbl.Rows[i]["conslcd"].ToString()+"]";
                                if (tbl.Rows[i]["conslcd"].ToString() != "")
                                {
                                    IR.Rows[rNo]["conslnm"] = tbl.Rows[i]["conslnm"].ToString() + "[" + tbl.Rows[i]["conslcd"].ToString() + "]";
                                }
                                IR.Rows[rNo]["docdt"] = Convert.ToString(tbl.Rows[i]["docdt"]).Substring(0, 10);
                                IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].ToString();
                                IR.Rows[rNo]["snm"] = tbl.Rows[i]["osnm"].ToString();
                                //if (showpacksize == true) IR.Rows[rNo]["packsize"] = Convert.ToDouble(tbl.Rows[i]["packsize"]);
                                IR.Rows[rNo]["uom"] = tbl.Rows[i]["uomnm"].ToString();
                                IR.Rows[rNo]["qnty"] = dbqty;
                                IR.Rows[rNo]["amt"] = namt;
                                if (showrate == true) IR.Rows[rNo]["rate"] = (dbqty == 0 ? 0 : namt / dbqty).toRound(4);
                                if (batchprint == true) IR.Rows[rNo]["batchno"] = tbl.Rows[i]["batchno"].ToString();

                                bqnty = bqnty + dbqty;
                                bamt = bamt + namt;
                                i = i + 1;
                                if (i > maxR) break;
                            }
                            if (bqnty + bamt != 0)
                            {
                                //IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                //IR.Rows[rNo]["cd"] = tbl.Rows[i - 1]["ocd"].ToString();
                                //IR.Rows[rNo]["nm"] = tbl.Rows[i - 1]["onm"].ToString();
                                //IR.Rows[rNo]["snm"] = tbl.Rows[i - 1]["osnm"].ToString();
                                //IR.Rows[rNo]["packsize"] = Convert.ToDouble(tbl.Rows[i - 1]["packsize"]);
                                //IR.Rows[rNo]["uom"] = tbl.Rows[i - 1]["uomnm"].ToString();
                                //IR.Rows[rNo]["qnty"] = bqnty;
                                //IR.Rows[rNo]["amt"] = bamt;
                                qty = qty + bqnty;
                                amt = amt + bamt;
                            }
                            //monthlytotal calculation
                            monthqnty += bqnty;
                            monthamt += bamt;
                            if (i > maxR) break;
                        }
                        //monthly total
                        if (monthtotal == true)
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["dammy"] = "";
                            IR.Rows[rNo]["nm"] = "Month (" + chkval2 + ") Totals";
                            IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                            IR.Rows[rNo]["qnty"] = monthqnty;
                            IR.Rows[rNo]["amt"] = monthamt;
                        }
                        //end
                        if (i > maxR) break;
                    }
                        if (qty + amt != 0)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["dammy"] = "";
                        if (repon == "I") IR.Rows[rNo]["nm"] = "Item Totals"; else IR.Rows[rNo]["nm"] = "Party Totals";
                        IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                        IR.Rows[rNo]["qnty"] = qty;
                        IR.Rows[rNo]["amt"] = amt;
                    }
                    tqty = tqty + qty;
                    tamt = tamt + amt;
                }
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["dammy"] = "";
                IR.Rows[rNo]["nm"] = "Grand Totals";
                IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                IR.Rows[rNo]["qnty"] = tqty;
                IR.Rows[rNo]["amt"] = tamt;

                // Create Blank line
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["dammy"] = " ";
                IR.Rows[rNo]["flag"] = " height:14px; ";

                string pghdr1 = dsp1 + " wise detail Statement from " + fdt + " to " + tdt;
                string repname = "PartyStmt";
                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "L", false);

                TempData[repname] = PV;
                TempData[repname + "xxx"] = IR;
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult ReportSumm(DataTable tbl, string reptype, string repon, string rateqntybag, string pghdr, bool monthtotal)
        {
            try
            {
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
                string qdsp = "";
                if (rateqntybag == "B") qdsp = "n,12";
                else qdsp = "n,12,4";

                string dsp1 = ""; string dsp2 = "";
                //if (repon == "I") dsp1 = "Party"; else dsp1 = "Item";
                if (repon == "I") dsp1 = "Item"; else dsp1 = "Party";
                //if (repon == "I") dsp2 = "Area"; else dsp2 = "ProdCd";
                dsp2 = "prodgrpcd";
                HC.RepStart(IR, 3);
             
                HC.GetPrintHeader(IR, "cd", "string", "c,10", "Code");
                HC.GetPrintHeader(IR, "nm", "string", "c,35", dsp1 + " Name");
                HC.GetPrintHeader(IR, "conslnm", "string", "c,35", "Consignee Name");
                //if (reptype != "SS") { HC.GetPrintHeader(IR, "docno", "string", "c,10", "Document No."); }
                //HC.GetPrintHeader(IR, "docno", "string", "c,10", "Document No.");
                HC.GetPrintHeader(IR, "snm", "string", "c,15", dsp2);
                if (reptype != "SS") if (monthtotal == true) HC.GetPrintHeader(IR, "monthnm", "string", "c,10", "Month");
                //if (showpacksize == true) HC.GetPrintHeader(IR, "packsize", "double", "n,10,6", "Packsize");
                HC.GetPrintHeader(IR, "uom", "string", "c,4", "uom");
                HC.GetPrintHeader(IR, "qnty", "double", qdsp, "Qnty");
                if (showrate == true) HC.GetPrintHeader(IR, "rate", "double", qdsp, "Av.Rate");
                HC.GetPrintHeader(IR, "amt", "double", "n,12,2", "Value");

                double qty, amt = 0;
                double tqty, tamt = 0;
                double dbqty = 0;
                string conslcd = "";
                tqty = 0; tamt = 0;

                Int32 rNo = 0;

                // Report begins
                i = 0; maxR = tbl.Rows.Count - 1;

                while (i <= maxR)
                {
                    chkval = tbl.Rows[i]["cd"].ToString();
                    qty = 0; amt = 0;

                    if (reptype == "S")
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["cd"] = tbl.Rows[i]["cd"].ToString();
                        IR.Rows[rNo]["nm"] = tbl.Rows[i]["nm"].ToString();
                        IR.Rows[rNo]["snm"] = tbl.Rows[i]["snm"].ToString();
                        IR.Rows[rNo]["celldesign"] = "cd=font-weight:bold;font-size:13px;^nm=font-weight:bold;font-size:13px;^snm=font-weight:bold;font-size:13px; ";
                    }
                    while (tbl.Rows[i]["cd"].ToString() == chkval)
                    {
                        chkval1 = tbl.Rows[i]["ocd"].ToString();
                        //double bqnty, bamt = 0;
                        //bqnty = 0; bamt = 0;
                        double monthamt = 0, monthqnty = 0;
                        if (monthtotal == true) { chkval2 = tbl.Rows[i]["docmonth"].ToString(); } else { chkval2 = tbl.Rows[i]["ocd"].ToString(); }
                        string checkstr1 = tbl.Rows[i]["cd"].ToString() + tbl.Rows[i]["ocd"].ToString();
                        if (monthtotal == true) checkstr1 = tbl.Rows[i]["cd"].ToString() + tbl.Rows[i]["docmonth"].ToString();
                        while (checkstr1 == chkval + chkval2)//monthly condition
                        {
                            if (monthtotal == true) chkval1 = tbl.Rows[i]["ocd"].ToString();
                            double bqnty, bamt = 0;
                            bqnty = 0; bamt = 0;
                            string checkstr2 = tbl.Rows[i]["cd"].ToString() + tbl.Rows[i]["ocd"].ToString();
                            if (monthtotal == true) checkstr2 = tbl.Rows[i]["cd"].ToString() + tbl.Rows[i]["ocd"].ToString() + tbl.Rows[i]["docmonth"].ToString();
                            if (monthtotal == true) { chkval2 = tbl.Rows[i]["docmonth"].ToString(); } else { chkval2 = ""; }
                            while (tbl.Rows[i]["cd"].ToString() == chkval && tbl.Rows[i]["ocd"].ToString() == chkval1 && checkstr2 == chkval + chkval1 + chkval2)
                            {
                                if (rateqntybag == "B") dbqty = Convert.ToDouble(tbl.Rows[i]["nos"]);
                                else dbqty = Convert.ToDouble(tbl.Rows[i]["qnty"]);

                                switch (tbl.Rows[i]["doctag"].ToString())
                                {
                                    case "SB":
                                    case "PB":
                                        bqnty = bqnty + dbqty;
                                        bamt = bamt + Convert.ToDouble(tbl.Rows[i]["basamt"]);
                                        break;
                                    case "SR":
                                    case "PR":
                                        bqnty = bqnty - dbqty;
                                        bamt = bamt - Convert.ToDouble(tbl.Rows[i]["basamt"]);
                                        break;
                                }
                                i = i + 1;
                                if (i > maxR) break;
                                checkstr2 = tbl.Rows[i]["cd"].ToString() + tbl.Rows[i]["ocd"].ToString();
                                if (monthtotal == true) checkstr2 = tbl.Rows[i]["cd"].ToString() + tbl.Rows[i]["ocd"].ToString() + tbl.Rows[i]["docmonth"].ToString();
                            }
                            if (bqnty + bamt != 0 && reptype == "S")
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;

                                IR.Rows[rNo]["cd"] = tbl.Rows[i - 1]["ocd"].ToString();
                                IR.Rows[rNo]["nm"] = tbl.Rows[i - 1]["onm"].ToString();
                                if (tbl.Rows[i - 1]["conslcd"].ToString() != "")
                                {
                                    IR.Rows[rNo]["conslnm"] = tbl.Rows[i - 1]["conslnm"].ToString() + "[" + tbl.Rows[i - 1]["conslcd"].ToString() + "]";
                                }
                                //IR.Rows[rNo]["conslnm"] = tbl.Rows[i - 1]["conslnm"].ToString();
                                //if (reptype != "SS") { IR.Rows[rNo]["docno"] = tbl.Rows[i - 1]["docno"].ToString(); }
                                //IR.Rows[rNo]["docno"] = tbl.Rows[i - 1]["docno"].ToString();
                                IR.Rows[rNo]["snm"] = tbl.Rows[i - 1]["osnm"].ToString();
                                //if (showpacksize == true) IR.Rows[rNo]["packsize"] = Convert.ToDouble(tbl.Rows[i - 1]["packsize"]);
                                IR.Rows[rNo]["uom"] = tbl.Rows[i - 1]["uomnm"].ToString();
                                IR.Rows[rNo]["qnty"] = bqnty;
                                IR.Rows[rNo]["amt"] = bamt;
                                if (showrate == true) IR.Rows[rNo]["rate"] = Convert.ToDouble(bqnty == 0 ? 0 : (bamt / bqnty));
                                if (reptype != "SS")if (monthtotal == true) IR.Rows[rNo]["monthnm"] = tbl.Rows[i - 1]["docmonth"].ToString();
                            }
                            qty = qty + bqnty;
                            amt = amt + bamt;
                            //monthlytotal calculation
                            monthqnty += bqnty;
                            monthamt += bamt;
                            if (i > maxR) break;
                            checkstr1 = tbl.Rows[i]["cd"].ToString() + tbl.Rows[i]["ocd"].ToString();
                            if (monthtotal == true) checkstr1 = tbl.Rows[i]["cd"].ToString() + tbl.Rows[i]["docmonth"].ToString();
                        }
                        //monthly total
                        if (monthtotal == true)
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["dammy"] = "";
                            IR.Rows[rNo]["nm"] = "Month (" + chkval2 + ") Totals";
                            IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                            IR.Rows[rNo]["qnty"] = monthqnty;
                            IR.Rows[rNo]["amt"] = monthamt;
                        }
                        //end
                        if (i > maxR) break;
                    }
                        if (qty + amt != 0)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        if (reptype == "S")
                        {
                            IR.Rows[rNo]["dammy"] = "";
                            if (repon == "I") IR.Rows[rNo]["nm"] = "Item Totals"; else IR.Rows[rNo]["nm"] = "Party Totals";
                            IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                        }
                        else
                        {
                            IR.Rows[rNo]["cd"] = tbl.Rows[i - 1]["cd"].ToString();
                            IR.Rows[rNo]["nm"] = tbl.Rows[i - 1]["nm"].ToString();
                            IR.Rows[rNo]["snm"] = tbl.Rows[i - 1]["snm"].ToString();
                        }
                        IR.Rows[rNo]["qnty"] = qty;
                        IR.Rows[rNo]["amt"] = amt;
                    }
                    tqty = tqty + qty;
                    tamt = tamt + amt;
                }
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["dammy"] = "";
                IR.Rows[rNo]["nm"] = "Grand Totals";
                IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                IR.Rows[rNo]["qnty"] = tqty;
                IR.Rows[rNo]["amt"] = tamt;

                // Create Blank line
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["dammy"] = " ";
                IR.Rows[rNo]["flag"] = " height:14px; ";
                string pghdr1 = "";
                if (reptype=="SS") {  pghdr1 = dsp1 + " wise Super summary from " + fdt + " to " + tdt; }
                else { pghdr1 = dsp1 + " wise summary from " + fdt + " to " + tdt; }
                string rephdr = "PartyStmt";
                PV = HC.ShowReport(IR, rephdr, pghdr1, "", true, true, "L", false);

                TempData[rephdr] = PV;
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = rephdr });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult DownloadExcel(DataTable tbl, string GRPCD, string groupingwith, bool WithoutParty, string pghdr)
        {
            try
            {
                /* Steps of Processing Excel:
                1: Retrive group with SLCD and without slcd
                2.design header depands on the group (iggrpcd/grpcd)
                3.Print row by row Parties 
                4.Increament of the group total with heirarchy
                5.Then print heirarchycal total
                6.delete zero 0 value rows
                7.Print UNtagged Slcd.
                */
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                //string GRPNM = DBF.M_SUBLEG_GRPHDR.Where(s => s.GRPCD == GRPCD).First()?.GRPNM;
                string GRPNM = "";
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                string Sql = "";
                Sql += " select slcdgrpcd,slcdgrpnm,grpcdfull,nvl(parentcd,'0') parentcd,0 rowindex from " + CommVar.FinSchema(UNQSNO) + ".m_subleg_grp";
                Sql += " where GRPCD='" + GRPCD + "' and slcd is null";
                Sql += " order by grpcdfull,grpslno";
                DataTable tblgrouphierarchy = MasterHelp.SQLquery(Sql);
                Sql = "";
                Sql += " select grpcd,slcdgrpcd,slcdgrpnm,grpcdfull,nvl(parentcd,'0') parentcd,user_id,grpslno,slcdgrpnmdesc,a.slcd, ";
                Sql += "  (b.add1||' '||b.ADD2||' '||  b.ADD3||' '||b.ADD6) as sladd,b.REGMOBILE as slmob  ";
                Sql += " from " + CommVar.FinSchema(UNQSNO) + ".m_subleg_grp a, " + CommVar.FinSchema(UNQSNO) + ".m_subleg b ";
                Sql += " where a.slcd=b.slcd(+) and GRPCD='" + GRPCD + "' ";
                Sql += " order by grpcdfull,grpslno";
                DataTable tblmain = MasterHelp.SQLquery(Sql);
                if (tblmain.Rows.Count == 0)
                {
                    return Content("No group Found. Please Create Sub ledger grouping from finance module.");
                }
                //create a datatable for excelno
                DataRow[] rows = tbl.Select("DAMSTOCK ='F'");
                foreach (DataRow row in rows)
                {   //REMOVE FREE ITEM FROM TABLE FOR SANATAN COMPANY
                    tbl.Rows.Remove(row);
                }
                DataTable itgrpcdtbl = new DataView(tbl).ToTable(true, "itgrpcd");
                //DataTable brgrpcdtbl = new DataView(tbl).ToTable(true, "brgrpcd");
                string[] itTBLCOLS = new string[] { "OCD", "ONM", "OSNM" };
                DataTable itcdtbl = new DataView(tbl).ToTable(true, itTBLCOLS);
                DataTable monthtbl = new DataView(tbl).ToTable(true, "docmonth");
                /// end
                ExcelPackage ExcelPkg = new ExcelPackage();
                ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("Sheet1");
                wsSheet1.View.FreezePanes(3, 8);
                string grpabbr = "";
                switch (groupingwith.retStr())
                {
                    case "": grpabbr = "G"; break;
                    case "ITGRPCD": grpabbr = "Item Group Name"; break;
                    //case "BRGRPCD": grpabbr = "Broad Group Name"; break;
                    case "ITCD": grpabbr = "Item Name"; break;
                    case "MONTH": grpabbr = "Month Name"; break;
                    default: break;
                }
                wsSheet1.Cells[1, 1].Value = grpabbr;
                wsSheet1.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                wsSheet1.Cells[2, 1].Value = GRPNM + " analysis as on " + fdt + " to " + tdt;
                wsSheet1.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                int hdrcount = 7;//left 7 row fixed
                List<string> GroupHeaderlist = new List<string>();
                //no group
                if (groupingwith == null)
                {
                    wsSheet1.Cells[1, ++hdrcount].Value = "Total";
                    wsSheet1.Cells[1, hdrcount, 1, hdrcount + 1].Merge = true;
                    wsSheet1.Cells[1, hdrcount].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                    wsSheet1.Cells[2, hdrcount].Value = "Qnty";
                    tblgrouphierarchy.Columns.Add(hdrcount + "_" + "GRPCD", typeof(Double));
                    GroupHeaderlist.Add(hdrcount + "_" + "GRPCD");
                    wsSheet1.Cells[2, ++hdrcount].Value = "Amt";
                    tblgrouphierarchy.Columns.Add("GRPCD", typeof(Double));
                    wsSheet1.Column(hdrcount).Style.Numberformat.Format = "0.00";
                    GroupHeaderlist.Add("GRPCD");

                }
                else if (groupingwith == "ITGRPCD")
                {
                    foreach (DataRow hrdr in itgrpcdtbl.Rows)
                    {
                        string itgrpcd = hrdr["itgrpcd"].ToString();
                        M_GROUP mgrop = DB.M_GROUP.Where(k => k.ITGRPCD == itgrpcd).First();
                        wsSheet1.Cells[1, ++hdrcount].Value = mgrop.ITGRPNM;
                        wsSheet1.Cells[1, hdrcount, 1, hdrcount + 1].Merge = true;
                        wsSheet1.Cells[1, hdrcount].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                        wsSheet1.Cells[2, hdrcount].Value = "Qnty";
                        tblgrouphierarchy.Columns.Add(hdrcount + "_" + itgrpcd, typeof(Double));
                        GroupHeaderlist.Add(hdrcount + "_" + itgrpcd);
                        wsSheet1.Cells[2, ++hdrcount].Value = "Amt";
                        tblgrouphierarchy.Columns.Add(itgrpcd, typeof(Double));
                        wsSheet1.Column(hdrcount).Style.Numberformat.Format = "0.00";
                        GroupHeaderlist.Add(itgrpcd);
                    }
                    //ADD TOTAL FIELD
                    wsSheet1.Cells[1, ++hdrcount].Value = "Total";
                    wsSheet1.Cells[2, hdrcount].Value = "Qnty";
                    tblgrouphierarchy.Columns.Add("Qnty", typeof(Double));
                    wsSheet1.Cells[2, ++hdrcount].Value = "Amt";
                    tblgrouphierarchy.Columns.Add("BASAMT", typeof(Double));
                    wsSheet1.Column(hdrcount).Style.Numberformat.Format = "0.00";
                    //itgrp 
                }
             
                else if (groupingwith == "ITCD")
                {   //ITCD
                    foreach (DataRow hrdr in itcdtbl.Rows)
                    {
                        string itcd = hrdr["OCD"].ToString();//OCD STANDS FOR ITCD
                        string itnm = hrdr["ONM"].ToString();
                        string itshortnm = hrdr["OSNM"].ToString();
                        //M_SITEM sitem = DB.M_SITEM.Where(k => k.ITCD == itcd).First();
                        wsSheet1.Cells[1, ++hdrcount].Value = itshortnm.retStr() == "" ? itnm : itshortnm;
                        wsSheet1.Cells[1, hdrcount, 1, hdrcount + 1].Merge = true;
                        wsSheet1.Cells[1, hdrcount].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                        wsSheet1.Cells[2, hdrcount].Value = "Qnty";
                        tblgrouphierarchy.Columns.Add(hdrcount + "_" + itcd, typeof(Double));
                        GroupHeaderlist.Add(hdrcount + "_" + itcd);
                        wsSheet1.Cells[2, ++hdrcount].Value = "Amt";
                        tblgrouphierarchy.Columns.Add(itcd, typeof(Double));
                        GroupHeaderlist.Add(itcd);
                        wsSheet1.Column(hdrcount).Style.Numberformat.Format = "0.00";
                    }
                    //ADD TOTAL FIELD
                    wsSheet1.Cells[1, ++hdrcount].Value = "Total";
                    wsSheet1.Cells[2, hdrcount].Value = "Qnty";
                    tblgrouphierarchy.Columns.Add("Qnty", typeof(Double));
                    wsSheet1.Cells[2, ++hdrcount].Value = "Amt";
                    tblgrouphierarchy.Columns.Add("BASAMT", typeof(Double));
                    wsSheet1.Column(hdrcount).Style.Numberformat.Format = "0.00";
                }
                else if (groupingwith == "MONTH")
                {   //MONTH
                    foreach (DataRow hrdr in monthtbl.Rows)
                    {
                        string month = hrdr["docmonth"].ToString();
                        wsSheet1.Cells[1, ++hdrcount].Value = month;
                        wsSheet1.Cells[1, hdrcount, 1, hdrcount + 1].Merge = true;
                        wsSheet1.Cells[1, hdrcount].Style.HorizontalAlignment = ExcelHorizontalAlignment.CenterContinuous;
                        wsSheet1.Cells[2, hdrcount].Value = "Qnty";
                        tblgrouphierarchy.Columns.Add(hdrcount + "_" + month, typeof(Double));
                        GroupHeaderlist.Add(hdrcount + "_" + month);
                        wsSheet1.Cells[2, ++hdrcount].Value = "Amt";
                        tblgrouphierarchy.Columns.Add(month, typeof(Double));
                        GroupHeaderlist.Add(month);
                        wsSheet1.Column(hdrcount).Style.Numberformat.Format = "0.00";
                    }
                    //ADD TOTAL FIELD
                    wsSheet1.Cells[1, ++hdrcount].Value = "Total";
                    wsSheet1.Cells[2, hdrcount].Value = "Qnty";
                    tblgrouphierarchy.Columns.Add("Qnty", typeof(Double));
                    wsSheet1.Cells[2, ++hdrcount].Value = "Amt";
                    tblgrouphierarchy.Columns.Add("BASAMT", typeof(Double));
                    wsSheet1.Column(hdrcount).Style.Numberformat.Format = "0.00";
                }
                //end heirarchy list
                wsSheet1.Column(1).Style.Font.Bold = true;
                wsSheet1.Row(1).Style.Font.Bold = true;
                wsSheet1.Row(1).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                wsSheet1.Row(1).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SteelBlue);
                wsSheet1.Row(2).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                wsSheet1.Row(2).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SteelBlue);
                for (int q = 1; q <= 4; q++)
                {
                    wsSheet1.Column(q).Width = 2.82;
                }
                wsSheet1.Column(5).Width = 35;
                int groupRowIndex = 0;
                int rowindex = 2;
                int columnindex = 1;
                for (int maindrindex = 0; maindrindex < tblmain.Rows.Count; maindrindex++)
                {
                    string slcdgrpcd = tblmain.Rows[maindrindex]["slcdgrpcd"].ToString().Trim();
                    string grpcdfull = tblmain.Rows[maindrindex]["grpcdfull"].ToString().Trim();
                    string mname = tblmain.Rows[maindrindex]["slcdgrpnm"].ToString().Trim();
                    string sladd = tblmain.Rows[maindrindex]["sladd"].ToString().Trim();
                    string slmob = tblmain.Rows[maindrindex]["slmob"].ToString().Trim();
                    string slcd = tblmain.Rows[maindrindex]["slcd"].ToString();
                    List<string> arr = new List<string>();
                    for (int i = 0; i < grpcdfull.Length; i += 6)
                    {
                        if ((i + 6) < grpcdfull.Length)
                            arr.Add(grpcdfull.Substring(i, 6));
                        else
                            arr.Add(grpcdfull.Substring(i));
                    }
                    if (slcd == "")
                    {
                        Sql = " SELECT nvl(SUM(SLCDGRPCD),0) totchild FROM(";
                        Sql += " SELECT COUNT(SLCDGRPCD) SLCDGRPCD  from " + CommVar.FinSchema(UNQSNO) + ".m_subleg_grp where PARENTCD = '" + slcdgrpcd + "'";
                        Sql += " UNION ALL";
                        Sql += " SELECT COUNT(SLCDGRPCD) SLCDGRPCD  from " + CommVar.FinSchema(UNQSNO) + ".m_subleg_grp where (SELECT COUNT(SLCDGRPCD)  from " + CommVar.FinSchema(UNQSNO) + ".m_subleg_grp where SLCDGRPCD = '" + slcdgrpcd + "')> 1  )";
                        DataTable temptbl = MasterHelp.SQLquery(Sql);
                        if (Convert.ToInt16(temptbl.Rows[0]["totchild"].ToString()) > 0)
                        {
                            wsSheet1.Cells[++rowindex, arr.Count].Value = mname;
                            groupRowIndex = rowindex;
                            //add row index
                            DataRow row = tblgrouphierarchy.Select("slcdgrpcd='" + slcdgrpcd + "'").First();
                            int hrcyRowindex = tblgrouphierarchy.Rows.IndexOf(row);
                            var hrcyColumnindex = tblgrouphierarchy.Columns["rowindex"].Ordinal;
                            tblgrouphierarchy.Rows[hrcyRowindex][hrcyColumnindex] = rowindex;
                            wsSheet1.Row(rowindex).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            if (arr.Count == 1) { wsSheet1.Row(rowindex).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSkyBlue); }
                            else if (arr.Count == 2) { wsSheet1.Row(rowindex).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SkyBlue); }
                            else if (arr.Count == 3) { wsSheet1.Row(rowindex).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue); }
                            else if (arr.Count == 4) { wsSheet1.Row(rowindex).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.AliceBlue); }

                        }
                    }
                    else
                    {
                        #region Party qnty print and add to group total
                        var drarr = tbl.Select("cd='" + slcd + "'");
                        if (drarr.Length > 0)
                        {
                            wsSheet1.Cells[++rowindex, 5].Value = mname;//Set party name in the excel
                            wsSheet1.Cells[rowindex, 6].Value = sladd;//Set party name in the excel
                            wsSheet1.Cells[rowindex, 7].Value = slmob;//Set party name in the excel
                            if (groupingwith == null)
                            {
                                var grptbl = tbl.AsEnumerable()
                                    .Where(w => w.Field<string>("cd") == slcd)
                                    .GroupBy(g => g.Field<string>("cd"))
                                    .Select(g =>
                                    {
                                        var row = tbl.NewRow();
                                        row["cd"] = g.Key;
                                        row["QNTY"] = g.Sum(r => r.Field<decimal>("QNTY"));
                                        row["BASAMT"] = g.Sum(r => r.Field<decimal>("BASAMT"));
                                        return row;
                                    }).CopyToDataTable();
                                foreach (DataRow drgrp in grptbl.Rows)
                                {
                                    var grpcd = "GRPCD";
                                    var hrcyColumnindex = tblgrouphierarchy.Columns[grpcd].Ordinal;
                                    //start summation of group total 
                                    for (int i = 0; i < grpcdfull.Length; i += 6)
                                    {
                                        if ((i + 6) < grpcdfull.Length)
                                        {
                                            string slcdgrpcdtmp = grpcdfull.Substring(i, 6);
                                            DataRow row = tblgrouphierarchy.Select("slcdgrpcd='" + slcdgrpcdtmp + "'").FirstOrDefault();
                                            if (row != null)
                                            {
                                                int hrcyRowindex = tblgrouphierarchy.Rows.IndexOf(row);
                                                tblgrouphierarchy.Rows[hrcyRowindex][hrcyColumnindex - 1] = tblgrouphierarchy.Rows[hrcyRowindex][hrcyColumnindex - 1].retDbl() + Convert.ToDouble(drgrp["QNTY"]);
                                                tblgrouphierarchy.Rows[hrcyRowindex][hrcyColumnindex] = tblgrouphierarchy.Rows[hrcyRowindex][hrcyColumnindex].retDbl() + Convert.ToDouble(drgrp["BASAMT"]);
                                            }
                                        }
                                    } // end summation of group total 
                                    //print column
                                    var ColumnName = tblgrouphierarchy.Columns[hrcyColumnindex - 1].ColumnName;
                                    if (ColumnName.Split('_').Length > 1)
                                    {
                                        columnindex = ColumnName.Split('_')[0].retInt();
                                    }
                                    wsSheet1.Cells[rowindex, columnindex].Value = drgrp["QNTY"];
                                    wsSheet1.Cells[rowindex, columnindex + 1].Value = drgrp["BASAMT"];
                                    if (WithoutParty)
                                    {
                                        wsSheet1.Row(rowindex).Hidden = true;
                                    }
                                }
                            }
                            else if (groupingwith == "ITGRPCD")
                            {
                                var itgrptbl = tbl.AsEnumerable()
                                 .Where(w => w.Field<string>("cd") == slcd)
                                 .GroupBy(g => new { cd = g["cd"], itgrpcd = g["itgrpcd"] })
                                 .Select(g =>
                                       {
                                           var row = tbl.NewRow();
                                           row["cd"] = g.Key.cd;
                                           row["itgrpcd"] = g.Key.itgrpcd;
                                           row["QNTY"] = g.Sum(r => r.Field<decimal>("QNTY"));
                                           row["BASAMT"] = g.Sum(r => r.Field<decimal>("BASAMT"));
                                           return row;
                                       }).CopyToDataTable();

                                foreach (DataRow dritgrp in itgrptbl.Rows)
                                {
                                    var grpcd = dritgrp["itgrpcd"].ToString();
                                    var hrcyColumnindex = tblgrouphierarchy.Columns[grpcd].Ordinal;
                                    for (int i = 0; i < grpcdfull.Length; i += 6)
                                    {
                                        if ((i + 6) < grpcdfull.Length)
                                        {
                                            string slcdgrpcdtmp = grpcdfull.Substring(i, 6);
                                            DataRow row = tblgrouphierarchy.Select("slcdgrpcd='" + slcdgrpcdtmp + "'").FirstOrDefault();
                                            if (row != null)
                                            {
                                                int hrcyRowindex = tblgrouphierarchy.Rows.IndexOf(row);
                                                tblgrouphierarchy.Rows[hrcyRowindex][hrcyColumnindex - 1] = tblgrouphierarchy.Rows[hrcyRowindex][hrcyColumnindex - 1].retDbl() + Convert.ToDouble(dritgrp["QNTY"]);
                                                tblgrouphierarchy.Rows[hrcyRowindex][hrcyColumnindex] = tblgrouphierarchy.Rows[hrcyRowindex][hrcyColumnindex].retDbl() + Convert.ToDouble(dritgrp["BASAMT"]);
                                                tblgrouphierarchy.Rows[hrcyRowindex]["QNTY"] = tblgrouphierarchy.Rows[hrcyRowindex]["QNTY"].retDbl() + Convert.ToDouble(dritgrp["QNTY"]);
                                                tblgrouphierarchy.Rows[hrcyRowindex]["BASAMT"] = tblgrouphierarchy.Rows[hrcyRowindex]["BASAMT"].retDbl() + Convert.ToDouble(dritgrp["BASAMT"]);
                                            }
                                        }
                                    }
                                    //print column
                                    var ColumnName = tblgrouphierarchy.Columns[hrcyColumnindex - 1].ColumnName;
                                    if (ColumnName.Split('_').Length > 1)
                                    {
                                        columnindex = ColumnName.Split('_')[0].retInt();
                                    }
                                    wsSheet1.Cells[rowindex, columnindex].Value = dritgrp["QNTY"];
                                    wsSheet1.Cells[rowindex, columnindex + 1].Value = dritgrp["BASAMT"];
                                    if (WithoutParty)
                                    {
                                        wsSheet1.Row(rowindex).Hidden = true;
                                    }
                                }
                            }
                         
                            else if (groupingwith == "ITCD")
                            {
                                var ittbl = tbl.AsEnumerable()
                              .Where(w => w.Field<string>("cd") == slcd)
                              .GroupBy(g => new { cd = g["cd"], ITCD = g["OCD"] })//OCD STANDS FOR ITCD
                              .Select(g =>
                              {
                                  var row = tbl.NewRow();
                                  row["cd"] = g.Key.cd;
                                  row["OCD"] = g.Key.ITCD;
                                  row["QNTY"] = g.Sum(r => r.Field<decimal>("QNTY"));
                                  row["BASAMT"] = g.Sum(r => r.Field<decimal>("BASAMT"));
                                  return row;
                              }).CopyToDataTable();
                                foreach (DataRow drbrgrp in ittbl.Rows)
                                {
                                    var ITCD = drbrgrp["OCD"].ToString();
                                    var hrcyColumnindex = tblgrouphierarchy.Columns[ITCD].Ordinal;
                                    //start summation of group total 
                                    for (int i = 0; i < grpcdfull.Length; i += 6)
                                    {
                                        if ((i + 6) < grpcdfull.Length)
                                        {
                                            string slcdgrpcdtmp = grpcdfull.Substring(i, 6);
                                            DataRow row = tblgrouphierarchy.Select("slcdgrpcd='" + slcdgrpcdtmp + "'").FirstOrDefault();
                                            if (row != null)
                                            {
                                                int hrcyRowindex = tblgrouphierarchy.Rows.IndexOf(row);
                                                tblgrouphierarchy.Rows[hrcyRowindex][hrcyColumnindex - 1] = tblgrouphierarchy.Rows[hrcyRowindex][hrcyColumnindex - 1].retDbl() + Convert.ToDouble(drbrgrp["QNTY"]);
                                                tblgrouphierarchy.Rows[hrcyRowindex][hrcyColumnindex] = tblgrouphierarchy.Rows[hrcyRowindex][hrcyColumnindex].retDbl() + Convert.ToDouble(drbrgrp["BASAMT"]);
                                                tblgrouphierarchy.Rows[hrcyRowindex]["QNTY"] = tblgrouphierarchy.Rows[hrcyRowindex]["QNTY"].retDbl() + Convert.ToDouble(drbrgrp["QNTY"]);
                                                tblgrouphierarchy.Rows[hrcyRowindex]["BASAMT"] = tblgrouphierarchy.Rows[hrcyRowindex]["BASAMT"].retDbl() + Convert.ToDouble(drbrgrp["BASAMT"]);

                                            }
                                        }
                                    } // end summation of group total 
                                    //print column
                                    var ColumnName = tblgrouphierarchy.Columns[hrcyColumnindex - 1].ColumnName;
                                    if (ColumnName.Split('_').Length > 1)
                                    {
                                        columnindex = ColumnName.Split('_')[0].retInt();
                                    }
                                    wsSheet1.Cells[rowindex, columnindex].Value = drbrgrp["QNTY"];
                                    wsSheet1.Cells[rowindex, columnindex + 1].Value = drbrgrp["BASAMT"];
                                    if (WithoutParty)
                                    {
                                        wsSheet1.Row(rowindex).Hidden = true;
                                    }
                                }
                            }
                            else if (groupingwith == "MONTH")
                            {
                                var ittbl = tbl.AsEnumerable()
                               .Where(w => w.Field<string>("cd") == slcd)
                               .GroupBy(g => new { cd = g["cd"], MONTH = g["DOCMONTH"] })
                               .Select(g =>
                               {
                                   var row = tbl.NewRow();
                                   row["cd"] = g.Key.cd;
                                   row["DOCMONTH"] = g.Key.MONTH;
                                   row["QNTY"] = g.Sum(r => r.Field<decimal>("QNTY"));
                                   row["BASAMT"] = g.Sum(r => r.Field<decimal>("BASAMT"));
                                   return row;
                               }).CopyToDataTable();
                                foreach (DataRow drmongrp in ittbl.Rows)
                                {
                                    var MONTH = drmongrp["docmonth"].ToString();
                                    var hrcyColumnindex = tblgrouphierarchy.Columns[MONTH].Ordinal;
                                    //start summation of group total 
                                    for (int i = 0; i < grpcdfull.Length; i += 6)
                                    {
                                        if ((i + 6) < grpcdfull.Length)
                                        {
                                            string slcdgrpcdtmp = grpcdfull.Substring(i, 6);
                                            DataRow row = tblgrouphierarchy.Select("slcdgrpcd='" + slcdgrpcdtmp + "'").FirstOrDefault();
                                            if (row != null)
                                            {
                                                int hrcyRowindex = tblgrouphierarchy.Rows.IndexOf(row);
                                                tblgrouphierarchy.Rows[hrcyRowindex][hrcyColumnindex - 1] = tblgrouphierarchy.Rows[hrcyRowindex][hrcyColumnindex - 1].retDbl() + Convert.ToDouble(drmongrp["QNTY"]);
                                                tblgrouphierarchy.Rows[hrcyRowindex][hrcyColumnindex] = tblgrouphierarchy.Rows[hrcyRowindex][hrcyColumnindex].retDbl() + Convert.ToDouble(drmongrp["BASAMT"]);
                                                tblgrouphierarchy.Rows[hrcyRowindex]["QNTY"] = tblgrouphierarchy.Rows[hrcyRowindex]["QNTY"].retDbl() + Convert.ToDouble(drmongrp["QNTY"]);
                                                tblgrouphierarchy.Rows[hrcyRowindex]["BASAMT"] = tblgrouphierarchy.Rows[hrcyRowindex]["BASAMT"].retDbl() + Convert.ToDouble(drmongrp["BASAMT"]);
                                            }
                                        }
                                    } // end summation of group total 
                                    //print column
                                    var ColumnName = tblgrouphierarchy.Columns[hrcyColumnindex - 1].ColumnName;
                                    if (ColumnName.Split('_').Length > 1)
                                    {
                                        columnindex = ColumnName.Split('_')[0].retInt();
                                    }
                                    wsSheet1.Cells[rowindex, columnindex].Value = drmongrp["QNTY"];
                                    wsSheet1.Cells[rowindex, columnindex + 1].Value = drmongrp["BASAMT"];
                                    if (WithoutParty)
                                    {
                                        wsSheet1.Row(rowindex).Hidden = true;
                                    }
                                }
                            }
                            //ADD TOTAL FIELD
                        }
                        #endregion
                    }
                }
                #region print group total
                foreach (DataRow drgrphrcy in tblgrouphierarchy.Rows)
                {
                    int excelColIndex = 1;
                    int excelRowindex = drgrphrcy["rowindex"].retInt();
                    if (excelRowindex > 0)
                    {
                        for (int i = 5; i < drgrphrcy.ItemArray.Length; i++)
                        {

                            var ColumnName = drgrphrcy.Table.Columns[i].ColumnName;
                            if (ColumnName.Split('_').Length > 1)
                            {
                                excelColIndex = ColumnName.Split('_')[0].retInt();
                            }
                            else
                            {
                                ++excelColIndex;
                            }
                            wsSheet1.Cells[excelRowindex, excelColIndex].Value = drgrphrcy.ItemArray[i].retDbl();
                            wsSheet1.Row(excelRowindex).Style.Font.Bold = true;
                        }
                        wsSheet1.Cells[excelRowindex, excelColIndex].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        wsSheet1.Cells[excelRowindex, excelColIndex].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DeepSkyBlue);
                        wsSheet1.Cells[excelRowindex, excelColIndex - 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        wsSheet1.Cells[excelRowindex, excelColIndex - 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DeepSkyBlue);
                    }
                }
                // Grand total
                wsSheet1.Cells[++rowindex, 1].Value = "Grand total";
                wsSheet1.Row(rowindex).Style.Font.Size = 12;
                wsSheet1.Row(rowindex).Style.Font.Bold = true;
                wsSheet1.Row(rowindex).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                wsSheet1.Row(rowindex).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DeepSkyBlue);
                tblgrouphierarchy = tblgrouphierarchy.Select("parentcd='0'").CopyToDataTable();
                double totqnty = 0, totamt = 0;
                foreach (string strgh in GroupHeaderlist)
                {
                    if (strgh.Split('_').Length > 1)
                    {
                        columnindex = strgh.Split('_')[0].retInt();
                        var grpval = strgh.Split('_')[1].retStr();
                        var sumQnty = tblgrouphierarchy.AsEnumerable().Sum(x => x.Field<double?>(strgh)).ToString();
                        var sumAmt = tblgrouphierarchy.AsEnumerable().Sum(x => x.Field<double?>(grpval)).ToString();
                        wsSheet1.Cells[rowindex, columnindex].Value = sumQnty.retDbl();
                        wsSheet1.Cells[rowindex, ++columnindex].Value = sumAmt.retDbl();
                        totqnty += sumQnty.retDbl(); totamt += sumAmt.retDbl();
                    }
                }
                if (groupingwith != null)
                {
                    wsSheet1.Cells[rowindex, ++columnindex].Value = totqnty;
                    wsSheet1.Cells[rowindex, ++columnindex].Value = totamt;
                }
                //end print group total
                //Delete zero value row from excel
                var noOfRow = wsSheet1.Dimension.End.Row;
                var noOfCol = wsSheet1.Dimension.End.Column;
                for (int rowIterator = 3; rowIterator <= noOfRow; rowIterator++)
                {
                    var ty1 = wsSheet1.Cells[rowIterator, 1].Value;
                    var tyw1 = wsSheet1.Cells[rowIterator, 2].Value;
                    var tey1 = wsSheet1.Cells[rowIterator, 3].Value;
                    string partynm = wsSheet1.Cells[rowIterator, 5].Value.retStr();
                    var amt = wsSheet1.Cells[rowIterator, noOfCol].Value.retDbl();
                    var QNTY = wsSheet1.Cells[rowIterator, noOfCol - 1].Value.retDbl();
                    if (amt == 0 && QNTY == 0 && string.IsNullOrEmpty(partynm))
                    {
                        wsSheet1.DeleteRow(rowIterator); rowIterator--; noOfRow--;
                    }
                }
                #endregion
                #region Print Untagged SLCD
                DataTable pendingtagtbl = new DataTable();
                var idsNotInB = tbl.AsEnumerable().Select(r => r.Field<string>("cd"))
                                    .Except(tblmain.AsEnumerable().Select(r => r.Field<string>("slcd")));
                var pendingtag = (from row in tbl.AsEnumerable()
                                  join id in idsNotInB
                                  on row.Field<string>("cd") equals id
                                  select row);
                if (pendingtag.Any())
                {
                    pendingtagtbl = pendingtag.CopyToDataTable();
                    ++rowindex;
                    wsSheet1.Cells[++rowindex, 1].Value = "Untagged Parties";
                    wsSheet1.Row(rowindex).Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    wsSheet1.Row(rowindex).Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    string lastslcd = "";
                    if (groupingwith == null)
                    {
                        var grptbl = pendingtagtbl.AsEnumerable()
                            .GroupBy(g => new { cd = g["cd"], nm = g["nm"] })
                            .Select(g =>
                            {
                                var row = pendingtagtbl.NewRow();
                                row["cd"] = g.Key.cd;
                                row["nm"] = g.Key.nm;
                                row["QNTY"] = g.Sum(r => r.Field<decimal>("QNTY"));
                                row["BASAMT"] = g.Sum(r => r.Field<decimal>("BASAMT"));
                                return row;
                            }).CopyToDataTable();

                        foreach (DataRow dritgrp in grptbl.Rows)
                        {
                            if (lastslcd != dritgrp["cd"].ToString())
                            {
                                lastslcd = dritgrp["cd"].ToString();
                                wsSheet1.Cells[++rowindex, 5].Value = dritgrp["nm"].ToString();
                            }
                            foreach (string strgh in GroupHeaderlist)
                            {
                                if (strgh.Split('_').Length > 1)
                                {
                                    columnindex = strgh.Split('_')[0].retInt();
                                    wsSheet1.Cells[rowindex, columnindex].Value = dritgrp["QNTY"];
                                    wsSheet1.Cells[rowindex, columnindex + 1].Value = dritgrp["BASAMT"];
                                }
                            }
                        }
                    }
                    else if (groupingwith == "ITGRPCD")
                    {
                        var itgrptbl = pendingtagtbl.AsEnumerable()
                         .GroupBy(g => new { cd = g["cd"], nm = g["nm"], itgrpcd = g["itgrpcd"] })
                         .Select(g =>
                         {
                             var row = pendingtagtbl.NewRow();
                             row["cd"] = g.Key.cd;
                             row["nm"] = g.Key.nm;
                             row["itgrpcd"] = g.Key.itgrpcd;
                             row["QNTY"] = g.Sum(r => r.Field<decimal>("QNTY"));
                             row["BASAMT"] = g.Sum(r => r.Field<decimal>("BASAMT"));
                             return row;
                         }).CopyToDataTable();

                        foreach (DataRow dritgrp in itgrptbl.Rows)
                        {
                            if (lastslcd != dritgrp["cd"].ToString())
                            {
                                lastslcd = dritgrp["cd"].ToString();
                                wsSheet1.Cells[++rowindex, 5].Value = dritgrp["nm"].ToString();
                            }
                            foreach (string strgh in GroupHeaderlist)
                            {
                                if (strgh.Split('_').Length > 1)
                                {
                                    if (strgh.Split('_')[1].retStr() == dritgrp["itgrpcd"].ToString())
                                    {
                                        columnindex = strgh.Split('_')[0].retInt();
                                        wsSheet1.Cells[rowindex, columnindex].Value = dritgrp["QNTY"];
                                        wsSheet1.Cells[rowindex, columnindex + 1].Value = dritgrp["BASAMT"];
                                    }
                                }
                            }
                        }
                    }
                  
                    else if (groupingwith == "ITCD")
                    {
                        var brgrptbl = pendingtagtbl.AsEnumerable()
                      .GroupBy(g => new { cd = g["cd"], nm = g["nm"], ITCD = g["OCD"] }) //OCD STANDS FOR ITCD
                      .Select(g =>
                      {
                          var row = pendingtagtbl.NewRow();
                          row["cd"] = g.Key.cd;
                          row["nm"] = g.Key.nm;
                          row["OCD"] = g.Key.ITCD;
                          row["QNTY"] = g.Sum(r => r.Field<decimal>("QNTY"));
                          row["BASAMT"] = g.Sum(r => r.Field<decimal>("BASAMT"));
                          return row;
                      }).CopyToDataTable();

                        foreach (DataRow dritgrp in brgrptbl.Rows)
                        {
                            if (lastslcd != dritgrp["cd"].ToString())
                            {
                                lastslcd = dritgrp["cd"].ToString();
                                wsSheet1.Cells[++rowindex, 5].Value = dritgrp["nm"].ToString();
                            }
                            foreach (string strgh in GroupHeaderlist)
                            {

                                if (strgh.Split('_').Length > 1)
                                {
                                    if (strgh.Split('_')[1].retStr() == dritgrp["OCD"].ToString())////OCD STANDS FOR ITCD
                                    {
                                        columnindex = strgh.Split('_')[0].retInt();
                                        wsSheet1.Cells[rowindex, columnindex].Value = dritgrp["QNTY"];
                                        wsSheet1.Cells[rowindex, columnindex + 1].Value = dritgrp["BASAMT"];
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion print untagged
                //for download//
                Response.Clear();
                Response.ClearContent();
                Response.Buffer = true;
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("Content-Disposition", "attachment; filename=Rep_PartyStmt" + System.DateTime.Now.ToString("ddHHmmss") + ".xlsx");
                Response.BinaryWrite(ExcelPkg.GetAsByteArray());
                Response.Flush();
                Response.Close();
                Response.End();
                return Content("Export Successfully.");
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }

    }
}
