using System;
using System.Linq;
using Oracle.ManagedDataAccess.Client;
using System.IO;
using System.Data;
using CrystalDecisions.CrystalReports.Engine;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;

namespace Improvar.Controllers
{
    public class Rep_GLController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        DropDownHelp DropDownHelp = new DropDownHelp();
        MasterHelp MasterHelp = new MasterHelp();
        MasterHelpFa masterhelpfa = new MasterHelpFa();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_GL
        public ActionResult Rep_GL()
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                string Para1 = "";
                ReportViewinHtml VE = new ReportViewinHtml(); Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                string slcdmust = "N";
                if (VE.MENU_PARA == "BANK")
                {
                    ViewBag.formname = "Bank Book Printing";
                    Para1 = "B";
                }
                else if (VE.MENU_PARA == "CASH")
                {
                    ViewBag.formname = "Cash Book Printing";
                    Para1 = "S";
                }
                else if (VE.MENU_PARA == "SUBLEG")
                {
                    ViewBag.formname = "Sub Ledger Printing";
                    Para1 = "D,C,O"; slcdmust = "N,Y";
                    Para1 = "D,C,O"; slcdmust = "N,Y";
                }
                else
                {
                    ViewBag.formname = "General Ledger Printing";
                    Para1 = "D,C,O"; slcdmust = "N,Y";
                }

                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

                VE.FDT = CommVar.FinStartDate(UNQSNO);
                VE.TDT = CommVar.CurrDate(UNQSNO);

                VE.DropDown_list_GLCD = DropDownHelp.DropDown_list_GLCD(Para1, slcdmust);
                VE.Glnm = MasterHelp.ComboFill("glcd", VE.DropDown_list_GLCD, 0, 1);

                VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("");
                VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);

                //VE.DropDown_list_Class1 = DropDownHelp.GetClass1cdforSelection("");
                //VE.CLASS1CD = MasterHelp.ComboFill("class1cd", VE.DropDown_list_Class1, 0, 1);

                //VE.DropDown_list_Class2 = DropDownHelp.GetClass2cdforSelection("");
                //VE.CLASS2CD = MasterHelp.ComboFill("class2cd", VE.DropDown_list_Class2, 0, 1);

                VE.DropDown_list_SubLegGrp = DropDownHelp.GetSubLegGrpforSelection();
                VE.SubLeg_Grp = MasterHelp.ComboFill("slcdgrpcd", VE.DropDown_list_SubLegGrp, 0, 1);

                VE.DropDown_list_SLPartyGrp = DropDownHelp.GetSLPartyGrpforSelection();
                VE.SLPartyGrp = MasterHelp.ComboFill("slpartygrp", VE.DropDown_list_SLPartyGrp, 0, 1);

                VE.DropDown_list_GLTBGrp = DropDownHelp.GetGLTBGrpforSelection();
                VE.GLTBGrp = MasterHelp.ComboFill("gltbgrpcd", VE.DropDown_list_GLTBGrp, 0, 1);

                VE.DropDown_list_CompanyLocationName = DropDownHelp.GetCompanyLocationName(VE.MenuID, Convert.ToDouble(VE.MenuIndex));
                VE.CompanyLocationName = MasterHelp.ComboFill("comploccd", VE.DropDown_list_CompanyLocationName, 0, 1);

                if (VE.MENU_PARA == "SUBLEG") VE.Checkbox1 = false; else VE.Checkbox1 = true; // Show Sub Ledger details
                VE.Checkbox2 = CommVar.ClientCode(UNQSNO) == "MKJ" ? false : true;// Running Balance
                VE.Checkbox3 = true; // Dr/Cr Ledger head
                VE.Checkbox4 = false; // Class details
                VE.Checkbox5 = true; //Cheque Details
                VE.Checkbox6 = false; //Bill Details
                VE.Checkbox7 = false; //Add Opening in Totals
                VE.Checkbox8 = false; // Class 1 wise ledger
                VE.Checkbox9 = CommVar.ClientCode(UNQSNO) == "MKJ" ? true : false; // Search in Only Current Locations
                VE.DefaultView = true;
                VE.ExitMode = 1;
                VE.DefaultDay = 0;
                VE.Checkbox8 = false; //Transaction Type
                return View(VE);
            }
        }
        [HttpPost]
        public ActionResult Rep_GL(ReportViewinHtml VE, FormCollection FC, string Command)
        {
            try
            {
                Cn.getQueryString(VE);

                string FD = VE.FDT;
                string TD = VE.TDT;
                string Para1 = "";
                string Para2 = "";
                string srt2 = "glcd,";
                string fld1 = "b.glcd,'' slcd,";
                string fld2 = "a.glcd,'' slcd,";
                string fld3 = "glcd,'' slcd,";
                string fld4 = "b.glnm, '' slnm ";
                string fld5 = "'' add1,'' add2,'' add3,'' add4,'' add5,'' add6,'' sltel1,'' sltel2,'' slpan,'' slgstno, '' slstate, '' slstatecd ";
                string grp1 = "a.glcd ";
                string grp2 = "b.glcd ";
                string grp3 = "glcd ";
                string reptype = "C";
                string printtype = VE.Checkbox8 == true ? "Y" : "N";

                string COMLOC = "";
                if (FC.AllKeys.Contains("comploccdvalue")) COMLOC = FC["comploccdvalue"].ToString().retSqlformat();
                string total = VE.TEXTBOX1;
                switch (Command)
                {
                    case "Crystal":
                        reptype = "C"; break;
                    case "Standard":
                        reptype = "R"; break;
                    case "Excel":
                        reptype = "E"; break;
                    case "CSV":
                        reptype = "V"; break;
                    case "EXPEXCL":
                        reptype = "G"; break;
                }

                if (VE.MENU_PARA == "BANK")
                {
                    Para1 = " and c.linkcd = 'B' ";
                    Para2 = "Bank Book From " + FD + " upto " + TD;
                }
                if (VE.MENU_PARA == "CASH")
                {
                    Para1 = " and c.linkcd = 'S' ";
                    Para2 = "Cash Book From " + FD + " upto " + TD;
                }
                if (VE.MENU_PARA == "JOURNAL")
                {
                    Para1 = " and c.linkcd not in ('B','S') and nvl(c.slcdmust,'N') in ('N','Y') ";
                    Para2 = "Ledger Book From " + FD + " upto " + TD;
                }
                if (VE.MENU_PARA == "SUBLEG")
                {
                    Para1 = " and c.slcdmust = 'Y' ";
                    Para1 = " and nvl(c.slcdmust,'N') in ('N','Y') ";
                    Para2 = "Sub Ledger Book From " + FD + " upto " + TD;
                    srt2 = "glcd,slnm,slcd,";
                    fld1 = "b.glcd,b.slcd,";
                    fld2 = "a.glcd,a.slcd,";
                    grp1 = "a.glcd,a.slcd ";
                    grp2 = "b.glcd,b.slcd ";
                    //fld4 = "b.glnm,c.slnm,c.district slcity ";
                    fld4 = "b.glnm,c.slnm,RTRIM(c.district|| '-' ||c.slarea,'-') slcity ";
                    fld3 = "glcd,slcd,";
                    grp3 = "glcd,slcd ";
                    fld5 = "c.add1,c.add2,c.add3,c.add4,c.add5,c.add6,c.phno1std sltel1,c.phno1 sltel2,c.panno slpan,c.gstno slgstno, c.state slstate, c.statecd slstatecd";
                }

                string RunnBal1 = VE.Checkbox2.ToString();
                if (RunnBal1 == "False") RunnBal1 = "N"; else RunnBal1 = "Y";
                if (reptype == "E" || reptype == "V") RunnBal1 = "Y";

                string drcrhead = VE.Checkbox3.ToString();
                string class1 = VE.Checkbox4.ToString();
                string chq1 = VE.Checkbox5.ToString();
                string bill1 = VE.Checkbox6.ToString();
                bool ShowOpinTotal = VE.Checkbox7;
                bool sublegshow = VE.Checkbox1;
                bool PrintShareDetails = VE.Checkbox14;
                if (drcrhead == "False")
                {
                    drcrhead = "N";
                }
                else
                {
                    drcrhead = "Y";
                }

                string selGlTbGrp = "";
                if (FC.AllKeys.Contains("gltbgrpcdvalue")) selGlTbGrp = FC["gltbgrpcdvalue"].ToString().retSqlformat();
                string selSlPartyGrp = "";
                if (FC.AllKeys.Contains("slpartygrpvalue")) selSlPartyGrp = FC["slpartygrpvalue"].ToString().retSqlformat();

                string acsel1 = "";
                if (FC.AllKeys.Contains("glcdvalue"))
                {
                    acsel1 = FC["glcdvalue"].ToString().retSqlformat();
                    acsel1 = " and B.GLCD in (" + acsel1 + ") ";
                }
                string selslcdgrpcd = "";
                if (FC.AllKeys.Contains("slcdgrpcdvalue")) selslcdgrpcd = CommFunc.retSqlformat(FC["slcdgrpcdvalue"].ToString());
                string slsel1 = "", slunsel1 = ""; string slcd = "";
                if (FC.AllKeys.Contains("slcdvalue"))
                {
                    slsel1 = FC["slcdvalue"].ToString().retSqlformat();
                    if (slsel1 != "") slsel1 = " and d.SLCD in (" + slsel1 + ") ";
                    slcd = FC["slcdvalue"].ToString();
                }
                if (FC.AllKeys.Contains("slcdunselvalue"))
                {
                    slunsel1 = FC["slcdunselvalue"].ToString().retSqlformat();
                    if (slunsel1 != "") slsel1 += " and d.SLCD not in (" + slunsel1 + ") ";
                }
                string selclass1cd = "";
                if (FC.AllKeys.Contains("class1cdvalue")) selclass1cd = FC["class1cdvalue"].ToString().retSqlformat();
                string selclass2cd = "";
                if (FC.AllKeys.Contains("class2cdvalue")) selclass2cd = FC["class2cdvalue"].ToString().retSqlformat();

                string FScm1 = CommVar.FinSchema(UNQSNO);
                CS = Cn.GetConnectionString();
                Cn.con = new OracleConnection(CS);
                if ((Cn.ds.Tables["fill_RECORD"] == null) == false)
                {
                    Cn.ds.Tables["fill_RECORD"].Clear();
                }
                if (Cn.con.State == ConnectionState.Closed)
                {
                    Cn.con.Open();
                }
                string sql1 = "";
                string sql2 = "";
                //  string sql3 = "";
                string sql4 = "";
                string sql = "";
                string com = CommVar.Compcd(UNQSNO), loc = CommVar.Loccd(UNQSNO);
                bool checkclass = false;
                if (selclass1cd != "" || selclass2cd != "") checkclass = true;

                string repname = CommFunc.retRepname("Leg_Book");
                Para2 += (checkclass == true ? " [Filtered]" : "");

                string finfromyr, fintoyr;
                finfromyr = FD.Substring(3, 2);
                if (finfromyr.retDbl() <= 3) finfromyr = "01/04/" + (FD.Substring(6, 4).retInt() - 1).retStr(); else finfromyr = "01/04/" + (FD.Substring(6, 4)).retStr();
                fintoyr = TD.Substring(3, 2);
                if (fintoyr.retDbl() <= 3) fintoyr = "01/04/" + (TD.Substring(6, 4).retInt() - 1).retStr(); else fintoyr = "01/04/" + (TD.Substring(6, 4)).retStr();
                sql = "select distinct a.schema_name, min(a.from_date) from_date,max(a.upto_date) to_date from fin_company a ";
                sql += "where a.client_code='" + CommVar.ClientCode(UNQSNO) + "' and a.from_date >= to_date('" + finfromyr + "','dd/mm/yyyy') and a.from_date <= to_date('" + fintoyr + "','dd/mm/yyyy') ";
                sql += "group by a.schema_name order by schema_name ";
                DataTable rsFinYr = masterhelpfa.SQLquery(sql);

                #region Exp.Ledger GST Excel
                if (reptype == "G")
                {
                    sql = "";
                    sql += "select a.expglcd glcd, c.glnm, b.docno, b.docdt, a.slcd, d.slnm, nvl(d.slarea,d.district) slarea, d.gstno, a.blno, a.bldt, ";
                    sql += "a.drcr, a.amt, a.gstamt, (case a.drcr when 'D' then a.amt else 0 end) dramt, (case a.drcr when 'C' then a.amt else 0 end) cramt from ";
                    sql += "( select a.autono, a.expglcd, a.pcode slcd, a.blno, to_char(a.bldt,'dd/mm/yyyy') bldt, a.drcr, ";
                    sql += "sum(a.amt) amt, sum(a.igstamt+a.sgstamt+a.cgstamt) gstamt ";
                    sql += "from " + FScm1 + ".t_vch_gst a, " + FScm1 + ".t_vch_hdr b ";
                    sql += "where a.autono=b.autono(+) ";
                    sql += "group by a.autono, a.expglcd, a.pcode, a.blno, to_char(a.bldt,'dd/mm/yyyy'), a.drcr ";
                    sql += "union all ";
                    sql += "select a.autono, a.glcd expglcd, a.slcd, '' blno, '' bldt, a.drcr, ";
                    sql += "a.amt, 0 gstamt ";
                    sql += "from " + FScm1 + ".t_vch_det a, " + FScm1 + ".t_vch_hdr b ";
                    sql += "where a.autono=b.autono(+) and a.autono not in (select autono from " + FScm1 + ".t_vch_gst) ) a, ";
                    sql += "" + FScm1 + ".t_cntrl_hdr b, " + FScm1 + ".m_genleg c, " + FScm1 + ".m_subleg d ";
                    sql += "where a.autono=b.autono(+) and a.expglcd=c.glcd(+) and a.slcd=d.slcd(+) and ";
                    sql += "b.compcd='" + com + "' and nvl(b.cancel,'N')='N' and ";
                    if (selGlTbGrp != "") sql += "c.gltbgrpcd in (" + selGlTbGrp + ") and ";
                    if (FC.AllKeys.Contains("glcdvalue")) sql += "a.expglcd in (" + FC["glcdvalue"].ToString().retSqlformat() + ") and ";
                    if (FC.AllKeys.Contains("slcdvalue")) sql += "a.slcd in (" + FC["slcdvalue"].ToString().retSqlformat() + ") and ";
                    if (FC.AllKeys.Contains("slcdunselvalue")) sql += "a.slcd not in (" + FC["slcdunselvalue"].ToString().retSqlformat() + ") and ";
                    sql += "b.docdt >= to_date('" + FD + "','dd/mm/yyyy') and b.docdt <= to_date('" + TD + "','dd/mm/yyyy') ";
                    if (VE.SkipClosingVoucher == true) sql += "and nvl(a.INTTDS,' ') not in ('C') ";
                    sql += "order by glnm, docdt, docno ";
                    DataTable tbl = MasterHelp.SQLquery(sql);

                    DataTable[] exdt = new DataTable[1];
                    exdt[0] = tbl;
                    string[] sheetname = new string[1];
                    sheetname[0] = "Sheet1";
                    MasterHelp.ExcelfromDataTables(exdt, sheetname, repname, false, Para2, false);
                    return Content("Downloaded");
                }
                #endregion

                string FinSchm = FScm1;
                sql1 = "";
                for (int fi = 0; fi <= rsFinYr.Rows.Count - 1; fi++)
                {
                    FinSchm = rsFinYr.Rows[fi]["schema_name"].retStr();
                    if (fi > 0) sql1 += " union all ";
                    sql1 += "select a.compcd, a.compnm, a.loccd, a.locnm, a.autono, a.trcd, a.doccd, " + (VE.Checkbox13 == true ? "nvl(b.blno,a.docno)" : "a.docno") + " docno, a.docdt, a.slno, ";
                    sql1 += "a.vl_dt, a.pay_by, a.paid_to," + fld1.Replace("b.", "a.") + "a.r_glcd, a.r_slcd, a.t_rem, ";
                    sql1 += "a.amt, a.qnty, a.subloccd, a.chqno, a.chqdt, a.bank_name, a.bank_dt, a.rtgsno, a.postdt, a.inttds, ";
                    sql1 += "a.drcr, a.dglnm, a.dshortnm, a.dslnm, a.dslcity, a.oslno, a.slnm, a.slcity, ";
                    sql1 += "a.add1, a.add2, a.add3, a.add4, a.add5, a.add6, a.sltel1, a.sltel2, a.slpan, a.slgstno, a.slstate, a.slstatecd,'" + rsFinYr.Rows[fi]["from_date"].retStr() + "' from_date,'" + rsFinYr.Rows[fi]["to_date"].retStr() + "' to_date from ( " + Environment.NewLine;

                    sql1 += "select g.compcd, y.compnm, " + (VE.Checkbox9 == true || COMLOC != "" ? "x.locnm, g.loccd," : "'ALL' locnm, 'ALL' loccd,");
                    sql1 += "a.autono, a.trcd, g.doccd, g.docno, g.docdt, b.slno,a.vl_dt,a.pay_by,a.paid_to," + fld1 + "b.r_glcd,b.r_slcd,b.t_rem, ";
                    sql1 += (checkclass == false ? "nvl(b.amt,0)" : "nvl(i.amt,b.amt)") + " amt, b.qty qnty, ";
                    sql1 += "b.subloccd, b.chqno,to_char(b.chqdt,'dd/mm/yyyy') chqdt,b.bank_name, b.bank_dt, b.rtgsno, b.postdt, b.inttds, ";
                    sql1 += "b.drcr,e.glnm dglnm,e.shortnm dshortnm, f.slnm dslnm, f.district dslcity, decode(nvl(b.oslno,0),0,b.slno,nvl(b.oslno,0)) oslno, h.slnm, h.district slcity ";
                    sql1 += ",h.add1,h.add2,h.add3,h.add4,h.add5,h.add6,h.phno1std sltel1,h.phno1 sltel2,h.panno slpan,h.gstno slgstno, h.state slstate, h.statecd slstatecd ";
                    sql1 += "from " + FinSchm + ".t_vch_hdr a," + FinSchm + ".t_vch_det b," + FScm1 + ".m_genleg c," + FScm1 + ".m_subleg d,";
                    sql1 += FScm1 + ".m_genleg e," + FScm1 + ".m_subleg f," + FinSchm + ".t_cntrl_hdr g, " + FScm1 + ".m_subleg h, " + FScm1 + ".m_loca x, " + FScm1 + ".m_comp y ";
                    if (checkclass == true) sql1 += ", " + FinSchm + ".t_vch_class i ";
                    sql1 += "where a.autono = b.autono and b.glcd = c.glcd and b.slcd = d.slcd(+) and b.r_glcd = e.glcd(+) and b.slcd=h.slcd(+) and ";
                    sql1 += "g.compcd||g.loccd=x.compcd||x.loccd and g.compcd=y.compcd and ";
                    sql1 += "b.r_slcd = f.slcd(+) and a.autono = g.autono and a.docdt >= to_date('" + FD + "','dd/mm/yyyy') and ";
                    if (selGlTbGrp != "") sql += "c.gltbgrpcd in (" + selGlTbGrp + ") and ";
                    if (checkclass == true) sql1 += "b.autono=i.autono(+) and b.slno=i.dslno(+) and ";
                    if (selclass1cd != "") sql1 += "i.class1cd in (" + selclass1cd + ") and ";
                    if (selclass2cd != "") sql1 += "i.class2cd in (" + selclass2cd + ") and ";
                    sql1 += "a.docdt <= to_date('" + TD + "','dd/mm/yyyy') and nvl(g.cancel,'N') = 'N' " + acsel1 + slsel1;
                    if (VE.Checkbox9 == true && COMLOC == "") sql1 += " and nvl(b.subloccd,g.loccd) = '" + loc + "' ";
                    if (COMLOC != "") sql1 += " and g.compcd||g.loccd in (" + COMLOC + ") "; else sql1 += " and g.compcd='" + com + "' ";
                    if (fi > 0) sql1 += "and a.trcd not in ('OP') ";
                    if (VE.SkipClosingVoucher == true) sql1 += "and nvl(b.INTTDS,' ') not in ('C') ";
                    sql1 += " and a.trcd not in (" + CommVar.SkipTrCd() + ") " + Para1 + ") a, " + Environment.NewLine;

                    sql1 += "( select distinct a.autono, a.blno from " + FinSchm + ".t_vch_bl a ) b ";
                    sql1 += "where a.autono=b.autono(+) " + Environment.NewLine;
                }
                //sql1 += " order by compnm," + srt2 + "docdt,chqno,doccd,docno ";
                sql1 += " order by compnm," + srt2 + "docdt,doccd,docno,chqno ";

                sql2 = "";
                for (int fi = 0; fi <= rsFinYr.Rows.Count - 1; fi++)
                {
                    FinSchm = rsFinYr.Rows[fi]["schema_name"].retStr();
                    if (fi > 0) sql2 += " union all ";
                    sql2 += "select c.compcd, y.compnm, " + (VE.Checkbox9 == true || COMLOC != "" ? "c.loccd, x.locnm," : "'ALL' loccd, 'ALL' locnm, ");
                    sql2 += fld1 + "sum(decode(drcr,'D',b.amt,b.amt*-1)) amt from ";
                    sql2 += FinSchm + ".t_vch_hdr a," + FinSchm + ".t_vch_det b," + FinSchm + ".t_cntrl_hdr c," + FScm1 + ".m_genleg d, " + FScm1 + ".m_loca x, " + FScm1 + ".m_comp y ";
                    if (checkclass == true) sql2 += ", " + FinSchm + ".t_vch_class i ";
                    sql2 += " where a.autono = b.autono and a.autono =c.autono and a.trcd not in(" + CommVar.SkipTrCd() + ") and b.glcd = d.glcd " + Para1.Replace("c.", "d.");
                    sql2 += acsel1 + slsel1.Replace("d.", "b.");
                    sql2 += "  and nvl(c.cancel,'N') = 'N' and ";
                    sql2 += "c.compcd||c.loccd=x.compcd||x.loccd and c.compcd=y.compcd and ";
                    if (selGlTbGrp != "") sql += "d.gltbgrpcd in (" + selGlTbGrp + ") and ";
                    if (checkclass == true) sql2 += "b.autono=i.autono(+) and b.slno=i.dslno(+) and ";
                    if (selclass1cd != "") sql2 += "i.class1cd in (" + selclass1cd + ") and ";
                    if (selclass2cd != "") sql2 += "i.class2cd in (" + selclass2cd + ") and ";
                    if (VE.Checkbox9 == true && COMLOC == "") sql2 += "nvl(b.subloccd,c.loccd) = '" + loc + "' and ";
                    if (fi > 0) sql2 += "a.trcd not in ('OP') and ";
                    sql2 += "a.docdt < to_date('" + FD + "','dd/mm/yyyy') and ";
                    if (COMLOC != "") sql2 += " c.compcd||c.loccd in (" + COMLOC + ") "; else sql2 += " c.compcd='" + com + "' ";
                    if (VE.SkipClosingVoucher == true) sql2 += "and nvl(b.INTTDS,' ') not in ('C') ";
                    sql2 += " group by c.compcd, y.compnm, " + (VE.Checkbox9 == true || COMLOC != "" ? "c.loccd, x.locnm," : "'ALL','ALL',") + grp2;
                }
                sql2 = "select compcd, compnm, loccd, locnm, " + fld3 + "sum(amt) amt from (" + sql2 + ") group by compcd, compnm, loccd, locnm, " + grp3 + " order by " + grp3;

                //sql3 = "";
                //for (int fi = 0; fi <= rsFinYr.Rows.Count - 1; fi++)
                //{
                //    FinSchm = rsFinYr.Rows[fi]["schema_name"].retStr();
                //    if (fi > 0) sql3 += " union all ";
                //    sql3 += "select a.autono,a.doccd,b.docno,b.docdt,a.dslno,a.slno,a.class1cd,a.class2cd,a.refcd,a.amt, ";
                //    sql3 += "c.class1nm,d.class2nm,e.slnm refnm ";
                //    sql3 += " from " + FinSchm + ".t_vch_class a," + FinSchm + ".t_cntrl_hdr b,";
                //    sql3 += FScm1 + ".m_class1 c," + FScm1 + ".m_class2 d," + FScm1 + ".m_subleg e ";
                //    sql3 += "where a.autono = b.autono and a.class1cd = c.class1cd(+) and a.class2cd = d.class2cd(+) and ";
                //    sql3 += "a.refcd = e.slcd(+) and a.docdt >= to_date('" + FD + "','dd/mm/yyyy') and ";
                //    sql3 = sql3 + "a.docdt <= to_date('" + TD + "','dd/mm/yyyy') and ";
                //    if (COMLOC != "") sql3 += " b.compcd||b.loccd in (" + COMLOC + ") and "; else sql3 += " b.compcd='" + com + "' and ";
                //    sql3 += "nvl(b.cancel,'N') = 'N' ";
                //}
                //sql3 = sql3 + "order by autono ";

                sql4 = "";
                for (int fi = 0; fi <= rsFinYr.Rows.Count - 1; fi++)
                {
                    FinSchm = rsFinYr.Rows[fi]["schema_name"].retStr();
                    if (fi > 0) sql4 += " union ";
                    //sql4 += "select distinct d.compcd, y.compnm, " + (VE.Checkbox9 == true ? "d.loccd, x.locnm," : "'ALL' loccd, 'ALL' locnm,") + fld2 + fld4 + " ";
                    sql4 += "select distinct d.compcd, y.compnm, " + (VE.Checkbox9 == true || COMLOC != "" ? "d.loccd, x.locnm," : "'ALL' loccd, 'ALL' locnm,") + fld2 + fld4 + " ";
                    sql4 += "," + fld5 + " ";
                    sql4 += "from " + FinSchm + ".t_vch_det a," + FScm1 + ".m_genleg b,";
                    sql4 += FScm1 + ".m_subleg c," + FinSchm + ".t_cntrl_hdr d," + FinSchm + ".t_VCH_HDR e, " + FScm1 + ".m_loca x, " + FScm1 + ".m_comp y ";
                    if (checkclass == true) sql4 += ", " + FinSchm + ".t_vch_class i ";
                    sql4 += " where a.glcd = b.glcd(+) and a.slcd = c.slcd(+) and a.autono = d.autono and ";
                    sql4 += "d.compcd||d.loccd=x.compcd||x.loccd and d.compcd=y.compcd and ";
                    if (COMLOC != "") sql4 += " d.compcd||d.loccd in (" + COMLOC + ") "; else sql4 += " d.compcd='" + com + "' ";
                    sql4 += " and a.autono = e.autono  and nvl(d.cancel,'N') = 'N' and ";
                    if (selSlPartyGrp != "") sql4 += "nvl(c.partygrp,' ') in (" + selSlPartyGrp + ") and ";
                    if (selGlTbGrp != "") sql4 += "b.gltbgrpcd in (" + selGlTbGrp + ") and ";
                    //if (VE.Checkbox9 == true) sql4 += "nvl(a.subloccd,d.loccd) = '" + loc + "' and ";
                    if (VE.Checkbox9 == true && COMLOC == "") sql4 += "nvl(a.subloccd,d.loccd) = '" + loc + "' and ";
                    if (checkclass == true) sql4 += "a.autono=i.autono(+) and a.slno=i.dslno(+) and ";
                    if (selclass1cd != "") sql4 += "i.class1cd in (" + selclass1cd + ") and ";
                    if (selclass2cd != "") sql4 += "i.class2cd in (" + selclass2cd + ") and ";
                    sql4 += "e.trcd not in (" + CommVar.SkipTrCd() + ") and a.docdt <= to_date('" + TD + "','dd/mm/yyyy') " + Para1.Replace("c.", "b.") + acsel1 + slsel1.Replace("d.", "c.") + Para1.Replace("c.", "b.");
                    if (VE.SkipClosingVoucher == true) sql4 += "and nvl(a.INTTDS,' ') not in ('C') ";
                }
                //sql4 += " order by glnm," + srt2.Substring(0, srt2.Length - 1) + ",compcd,locnm";
                sql4 += " order by compcd,locnm,glnm," + srt2.Substring(0, srt2.Length - 1) + "";

                string sql5 = "", sql6 = "";
                for (int fi = 0; fi <= rsFinYr.Rows.Count - 1; fi++)
                {
                    FinSchm = rsFinYr.Rows[fi]["schema_name"].retStr();
                    if (fi > 0)
                    {
                        sql5 += " union all ";
                        sql6 += " union all ";
                    }
                    sql5 += "select a.autono,a.i_autono,a.r_slno,a.adj_amt,c.blno,to_char(c.bldt,'dd/mm/yyyy') bldt ";
                    sql5 += "from " + FinSchm + ".t_vch_bl_adj a," + FinSchm + ".t_cntrl_hdr b," + FinSchm + ".t_vch_bl c, ";
                    sql5 += FinSchm + ".t_vch_hdr d ";
                    sql5 += "where a.autono = b.autono and a.i_autono = c.autono and ";
                    if (COMLOC != "") sql5 += " b.compcd||b.loccd in (" + COMLOC + ") and "; else sql5 += " b.compcd='" + com + "' and ";
                    sql5 += "a.autono = d.autono and a.i_slno = c.slno and d.trcd not in (" + CommVar.SkipTrCd() + ")  and nvl(b.cancel,'N') = 'N' and ";
                    sql5 += "b.docdt >= to_date('" + rsFinYr.Rows[fi]["from_date"].retDateStr() + "','dd/mm/yyyy') and ";
                    sql5 += "d.docdt >= to_Date('" + FD + "','dd/mm/yyyy') and d.docdt <= to_date('" + TD + "','dd/mm/yyyy') ";


                    sql6 += "select a.AUTONO,a.slno,a.drcr,a.amt,a.netamt,a.brokamt,a.sttamt,a.qty,a.tradedt,a.shrcd,a.rate,a.invest,a.txrem, ";
                    sql6 += "a.SLCD,b.slnm,c.ctgcd,f.CTGNM,c.shortnm,c.shrnm,a.shrdpcd,d.shrdpnm ";
                    sql6 += " from " + FinSchm + ".T_SHRTXNDTL a," + FScm1 + ".M_SUBLEG b ," + FinSchm + " .M_SHRMST c," + FinSchm + " .M_SHRDP d," + FinSchm + " .t_cntrl_hdr e," + FinSchm + " .M_SHRCTG f ";
                    sql6 += " where a.SLCD=b.SLCD(+) and a.shrcd=c.shrcd(+) and a.shrdpcd=d.shrdpcd(+) and a.autono=e.autono(+) and c.ctgcd=f.ctgcd(+) ";
                    sql6 += "and e.docdt >= to_Date('" + FD + "','dd/mm/yyyy') and e.docdt <= to_date('" + TD + "','dd/mm/yyyy') ";
                    if (COMLOC != "") sql6 += "and e.compcd||e.loccd in (" + COMLOC + ") "; else sql6 += "and e.compcd='" + com + "'  ";

                }

                DataTable TVCH = new DataTable();
                Cn.com = new OracleCommand(sql1, Cn.con);
                Cn.da.SelectCommand = Cn.com;
                Cn.da.Fill(TVCH);

                DataTable TOP = new DataTable();
                Cn.com = new OracleCommand(sql2, Cn.con);
                Cn.da.SelectCommand = Cn.com;
                Cn.da.Fill(TOP);

                DataTable TCLASS = new DataTable();
                //Cn.com = new OracleCommand(sql3, Cn.con);
                //Cn.da.SelectCommand = Cn.com;
                //Cn.da.Fill(TCLASS);

                DataTable THDR = new DataTable();
                Cn.com = new OracleCommand(sql4, Cn.con);
                Cn.da.SelectCommand = Cn.com;
                Cn.da.Fill(THDR);

                DataTable TBL = new DataTable();
                Cn.com = new OracleCommand(sql5, Cn.con);
                Cn.da.SelectCommand = Cn.com;
                Cn.da.Fill(TBL);

                Cn.con.Close();

                DataTable TSHRTXNDTL = MasterHelp.SQLquery(sql6);

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                Int32 rNo = 0;
                string balnat = "", dcfld = "";
                int currentmonth = 0, nextmonth = 0, currentyr = 0, nextyr = 0;
                DataTable IR = new DataTable("");
                if (reptype == "R")
                {
                    HC.RepStart(IR);
                    HC.GetPrintHeader(IR, "docdt", "string", "c,10", "Date");
                    HC.GetPrintHeader(IR, "docno", "string", "c,17", "Doc No");
                    HC.GetPrintHeader(IR, "trem", "string", "c,38", "Remarks");
                    HC.GetPrintHeader(IR, "chqno", "string", "c,8", "Chq.No");
                    HC.GetPrintHeader(IR, "type", "string", "c,10", "Type");
                    if (VE.Checkbox12 == true) HC.GetPrintHeader(IR, "txntype", "string", "c,6", "Txn.Ty");
                    HC.GetPrintHeader(IR, "bank_dt", "string", "c,8", "Clr.Dt");
                    if (VE.Checkbox11 == true) HC.GetPrintHeader(IR, "qnty", "double", "n,16,4:#####,##,##0.000", "Qnty");
                    HC.GetPrintHeader(IR, "dramt", "double", "n,19,2:###,##,##,##,##0.00", "Dr. Amt");
                    HC.GetPrintHeader(IR, "cramt", "double", "n,19,2:###,##,##,##,##0.00", "Cr. Amt");
                    if (RunnBal1 == "Y")
                    {
                        HC.GetPrintHeader(IR, "runbal", "double", "n,20,2:####,##,##,##,##0.00", "Bal Amt");
                        HC.GetPrintHeader(IR, "drcr", "string", "C,5", "D/C");
                    }
                }
                else
                {
                    if (COMLOC != "")
                    {
                        IR.Columns.Add("compcd", typeof(string));
                        IR.Columns.Add("compnm", typeof(string));
                        IR.Columns.Add("loccd", typeof(string));
                        IR.Columns.Add("locnm", typeof(string));
                    }
                    IR.Columns.Add("glcd", typeof(string));
                    IR.Columns.Add("glnm", typeof(string));
                    IR.Columns.Add("slcd", typeof(string));
                    IR.Columns.Add("slnm", typeof(string));
                    IR.Columns.Add("doccd", typeof(string));
                    IR.Columns.Add("docdt", typeof(DateTime));
                    IR.Columns.Add("docno", typeof(string));
                    IR.Columns.Add("trem", typeof(string));
                    IR.Columns.Add("chqno", typeof(string));
                    IR.Columns.Add("type", typeof(string));
                    if (VE.Checkbox12 == true) IR.Columns.Add("txntype", typeof(string));
                    IR.Columns.Add("bank_dt", typeof(string));
                    if (VE.Checkbox11 == true) IR.Columns.Add("qnty", typeof(double));
                    IR.Columns.Add("dramt", typeof(double));
                    IR.Columns.Add("cramt", typeof(double));
                    if (reptype == "E" || reptype == "V")
                    {
                        IR.Columns.Add("runbal", typeof(double));
                    }
                    else
                    {
                        IR.Columns.Add("runbal", typeof(string));
                    }
                    IR.Columns.Add("drcr", typeof(string));
                    if (VE.Checkbox10 == true)
                    {
                        IR.Columns.Add("sladd1", typeof(string));
                        IR.Columns.Add("sladd2", typeof(string));
                    }
                }

                DataTable FREC = new DataTable();
                FREC.Columns.Add("m_voudt", typeof(string));
                FREC.Columns.Add("M_VOUNO", typeof(string));
                FREC.Columns.Add("PCD1A", typeof(string));
                FREC.Columns.Add("M_DRAMT", typeof(double));
                FREC.Columns.Add("M_CRAMT", typeof(double));
                FREC.Columns.Add("M_BALANCE", typeof(double));
                FREC.Columns.Add("Bal_type", typeof(string));
                FREC.Columns.Add("M_PCD", typeof(string));
                FREC.Columns.Add("M_NARRATION", typeof(string));
                FREC.Columns.Add("M_PCDNAME", typeof(string));
                FREC.Columns.Add("P_DRAMT", typeof(double));
                FREC.Columns.Add("P_CRAMT", typeof(double));
                FREC.Columns.Add("UL", typeof(string));
                FREC.Columns.Add("LL", typeof(string));
                FREC.Columns.Add("MNTH", typeof(string));
                FREC.Columns.Add("M_NARRATION1", typeof(string));
                FREC.Columns.Add("M_NARRATION2", typeof(string));
                FREC.Columns.Add("B_TAG", typeof(string));
                FREC.Columns.Add("TOBAL", typeof(double));
                FREC.Columns.Add("opdrcr", typeof(string));
                FREC.Columns.Add("TDR", typeof(double));
                FREC.Columns.Add("TCR", typeof(double));
                FREC.Columns.Add("TCBAL", typeof(double));
                FREC.Columns.Add("cldrcr", typeof(string));
                FREC.Columns.Add("M_ACCHEAD", typeof(string));
                FREC.Columns.Add("CHQNO", typeof(string));
                FREC.Columns.Add("m_pcdadd1", typeof(string));
                FREC.Columns.Add("m_pcdadd2", typeof(string));
                if (THDR.Rows.Count == 0)
                {
                    return Content("No Records Found");
                }
                string dateform = "dd/MM/yyyy";
                string ShowLeginHeader = "";
                for (int i = 0; i <= THDR.Rows.Count - 1; i++)
                {
                    string sel1 = "";
                    if (THDR.Rows[i]["slcd"].ToString() == "" && VE.MENU_PARA != "SUBLEG")
                    {
                        sel1 = "glcd = '" + THDR.Rows[i]["glcd"].ToString() + "' and compcd='" + THDR.Rows[i]["compcd"] + "' and loccd='" + THDR.Rows[i]["loccd"] + "' ";
                    }
                    else
                    {
                        sel1 = "glcd = '" + THDR.Rows[i]["glcd"].ToString() + "' and compcd='" + THDR.Rows[i]["compcd"] + "' and loccd='" + THDR.Rows[i]["loccd"] + "' and " + (THDR.Rows[i]["slcd"].retStr() == "" ? "slcd is null" : "slcd='" + THDR.Rows[i]["slcd"].retStr() + "'");
                    }
                    var rm1 = TVCH.Select(sel1);
                    var rm2 = TOP.Select(sel1);
                    double op1 = 0;
                    double tdramt = 0, tcramt = 0, topamt = 0;
                    bool dataexist = false;

                    if (rm2.Count() != 0) op1 = rm2[0]["amt"].retDbl();
                    balnat = ""; dcfld = "dr";
                    //topamt = op1;
                    if (op1 > 0)
                    {
                        dcfld = "dr"; balnat = "Dr.";
                        if (ShowOpinTotal == true) tdramt = tdramt + op1;
                    }
                    if (op1 < 0)
                    {
                        dcfld = "cr"; balnat = "Cr.";
                        if (ShowOpinTotal == true) tcramt = tcramt + Math.Abs(op1);
                    }

                    #region Print Heading
                    if ((op1 != 0 || rm1.Count() > 0) && reptype == "R")
                    {
                        dataexist = true;
                        if (COMLOC != "")
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " </span>" + THDR.Rows[i]["compnm"].ToString() + (VE.Checkbox9 == true ? ", " + THDR.Rows[i]["locnm"].ToString() : "");
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                        }
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        if (VE.MENU_PARA == "SUBLEG")
                        {
                            IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + THDR.Rows[i]["slcd"].ToString() + "  " + " </span>" + THDR.Rows[i]["slnm"].ToString() + " [" + THDR.Rows[i]["slcity"].ToString() + "]";
                            IR.Rows[rNo]["Dammy"] = IR.Rows[rNo]["Dammy"] + "<span style='font-weight:100;font-size:9px;'>" + " [" + THDR.Rows[i]["glcd"].ToString() + "] " + " </span>" + THDR.Rows[i]["glnm"].ToString();
                            ShowLeginHeader = THDR.Rows[i]["slnm"].retStr() + " [" + THDR.Rows[i]["slcity"].ToString() + "]";
                        }
                        else
                        {
                            IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " [" + THDR.Rows[i]["glcd"].ToString() + "]  " + " </span>" + THDR.Rows[i]["glnm"].ToString();
                        }
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                        if (VE.Checkbox10 == true && VE.MENU_PARA == "SUBLEG")
                        {
                            string sladd = "";
                            string cfld = "";
                            for (int f = 1; f <= 6; f++)
                            {
                                cfld = "add" + Convert.ToString(f).ToString();
                                if (THDR.Rows[i][cfld].ToString() != "")
                                {
                                    sladd += THDR.Rows[i][cfld].ToString() + " ";
                                }
                            }
                            sladd += THDR.Rows[i]["slstate"].ToString() + " [ Code - " + THDR.Rows[i]["slstatecd"].ToString() + " ]";
                            if (THDR.Rows[i]["slpan"].ToString() != "")
                            {
                                sladd += "</br>" + "PAN # " + THDR.Rows[i]["slpan"].ToString();
                            }
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + sladd + "  " + " </span>";
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                        }
                    }
                    #endregion

                    #region Print Opening Balance
                    if (reptype == "C")
                    {
                        DataRow dtop = FREC.NewRow();
                        dtop["m_acchead"] = "Opening Balance";
                        dtop["m_" + dcfld + "amt"] = Math.Abs(op1);
                        dtop["bal_type"] = balnat;
                        if (VE.MENU_PARA == "SUBLEG")
                        {
                            dtop["m_pcd"] = THDR.Rows[i]["slcd"].ToString() + " [" + THDR.Rows[i]["glcd"].ToString() + "] ";
                            if (COMLOC != "")
                            {
                                dtop["m_pcdname"] = THDR.Rows[i]["compnm"].ToString() + (VE.Checkbox9 == true ? ", " + THDR.Rows[i]["locnm"].ToString() : "") + " " + ((char)13); // "^^";
                            }
                            dtop["m_pcdname"] = dtop["m_pcdname"].ToString() + THDR.Rows[i]["slnm"] + " (" + THDR.Rows[i]["slcity"] + ")  " + " [" + THDR.Rows[i]["glnm"] + "]";
                            if (VE.Checkbox10 == true)
                            {
                                string cfld = "", sladd = "";
                                for (int f = 1; f <= 6; f++)
                                {
                                    cfld = "add" + Convert.ToString(f).ToString();
                                    if (THDR.Rows[i][cfld].ToString() != "")
                                    {
                                        sladd += THDR.Rows[i][cfld].ToString() + " ";
                                    }
                                }
                                sladd += THDR.Rows[i]["slstate"].ToString() + " [ Code - " + THDR.Rows[i]["slstatecd"].ToString() + " ]";
                                dtop["m_pcdadd1"] = sladd;
                                if (THDR.Rows[i]["slpan"].ToString() != "")
                                {
                                    dtop["m_pcdadd2"] = "PAN # " + THDR.Rows[i]["slpan"].ToString();
                                }
                            }
                        }
                        else
                        {
                            if (COMLOC != "")
                            {
                                dtop["m_pcdname"] = THDR.Rows[i]["compnm"].ToString() + (VE.Checkbox9 == true ? ", " + THDR.Rows[i]["locnm"].ToString() : "") + " " + ((char)13); // "^^";
                            }
                            dtop["m_pcd"] = THDR.Rows[i]["glcd"];
                            dtop["m_pcdname"] = dtop["m_pcdname"].ToString() + THDR.Rows[i]["glnm"];
                        }
                        dtop["pcd1a"] = "";
                        dtop["b_tag"] = "B";
                        FREC.Rows.Add(dtop);
                    }
                    else
                    {
                        if (op1 != 0)
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            if (reptype != "R")
                            {
                                if (COMLOC != "")
                                {
                                    IR.Rows[rNo]["compcd"] = THDR.Rows[i]["compcd"].ToString();
                                    IR.Rows[rNo]["compnm"] = THDR.Rows[i]["compnm"].ToString();
                                    IR.Rows[rNo]["loccd"] = THDR.Rows[i]["loccd"].ToString();
                                    IR.Rows[rNo]["locnm"] = THDR.Rows[i]["locnm"].ToString();
                                }
                                IR.Rows[rNo]["glcd"] = THDR.Rows[i]["glcd"].ToString();
                                IR.Rows[rNo]["glnm"] = THDR.Rows[i]["glnm"].ToString();
                                IR.Rows[rNo]["slcd"] = THDR.Rows[i]["slcd"].ToString();
                                IR.Rows[rNo]["slnm"] = THDR.Rows[i]["slnm"].ToString();
                                if (VE.Checkbox10 == true)
                                {
                                    string cfld = "", sladd = "";
                                    for (int f = 1; f <= 6; f++)
                                    {
                                        cfld = "add" + Convert.ToString(f).ToString();
                                        if (THDR.Rows[i][cfld].ToString() != "")
                                        {
                                            sladd += THDR.Rows[i][cfld].ToString() + " ";
                                        }
                                    }
                                    sladd += THDR.Rows[i]["slstate"].ToString() + " [ Code - " + THDR.Rows[i]["slstatecd"].ToString() + " ]";
                                    IR.Rows[rNo]["sladd1"] = sladd;
                                    if (THDR.Rows[i]["slpan"].ToString() != "")
                                    {
                                        IR.Rows[rNo]["sladd2"] = "PAN # " + THDR.Rows[i]["slpan"].ToString();
                                    }
                                }
                            }
                            if (reptype == "R") IR.Rows[rNo]["docdt"] = FD.retDateStr(dateform); else IR.Rows[rNo]["docdt"] = Convert.ToDateTime(FD.retDateStr(dateform));
                            IR.Rows[rNo]["trem"] = "Opening Balance";
                            IR.Rows[rNo][dcfld + "amt"] = Math.Abs(op1);
                            if (reptype == "R") IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px";
                            if (RunnBal1 == "Y")
                            {
                                if (reptype == "R")
                                {
                                    IR.Rows[rNo]["runbal"] = Math.Abs(op1).ToINRFormat();
                                    IR.Rows[rNo]["drcr"] = balnat;
                                }
                                else if (reptype == "E" || reptype == "V")
                                {
                                    IR.Rows[rNo]["runbal"] = Math.Abs(op1).retDbl();
                                    IR.Rows[rNo]["drcr"] = balnat;
                                }
                                else
                                {
                                    IR.Rows[rNo]["runbal"] = Math.Abs(op1).ToINRFormat() + " " + balnat;
                                }
                                if (reptype == "R") IR.Rows[rNo]["celldesign"] = "runbal=text-align:right^;";
                            }
                        }
                    }
                    string rplchar = "<br />";
                    if (reptype == "C") rplchar = "" + ((char)13);
                    #endregion

                    #region Current Data
                    if (rm1.Count() > 0)
                    {
                        for (int j = 0; j <= rm1.Count() - 1; j++)
                        {
                            string nar1 = "";
                            if (sublegshow == true && (VE.MENU_PARA != "CASH" && VE.MENU_PARA != "BANK"))
                            {
                                if (rm1[j]["slnm"].ToString() != "") nar1 = rm1[j]["slnm"].ToString();
                                if (rm1[j]["slnm"].ToString() == "" && rm1[j]["dslnm"].ToString() != "") nar1 = rm1[j]["dslnm"].ToString();
                                if (VE.Checkbox10 == true && rm1[j]["slnm"].ToString() != "")
                                {
                                    string sladd = "";
                                    string cfld = "";
                                    for (int f = 1; f <= 6; f++)
                                    {
                                        cfld = "add" + Convert.ToString(f).ToString();
                                        if (rm1[j][cfld].ToString() != "")
                                        {
                                            sladd += rm1[j][cfld].ToString() + " ";
                                        }
                                    }
                                    sladd += rm1[j]["slstate"].ToString() + " [ Code - " + rm1[j]["slstatecd"].ToString() + " ]";
                                    if (rm1[j]["slpan"].ToString() != "")
                                    {
                                        sladd += rplchar + "PAN # " + rm1[j]["slpan"].ToString();
                                    }
                                    nar1 += rplchar + sladd;
                                }
                            }
                            if (printtype == "Y") nar1 += (nar1 == "" ? "" : rplchar) + CommVar.retTrCD(rm1[j]["trcd"].ToString());
                            if ((rm1[j]["paid_to"].ToString() != "" && rm1[j]["paid_to"].ToString() != rm1[j]["slcd"].retStr())) // && (VE.MENU_PARA == "CASH" || VE.MENU_PARA == "BANK" || VE.MENU_PARA == "SUBLEG"))
                            {
                                nar1 += "Pay / receipt : " + rm1[j]["paid_to"].ToString();
                            }
                            if (rm1[j]["t_rem"].ToString() != "")
                            {
                                if (printtype == "Y" && rm1[j]["trcd"].ToString() == "JV") nar1 += (nar1 == "" ? "" : rplchar) + rm1[j]["t_rem"].ToString();
                                else if (printtype == "N") nar1 += (nar1 == "" ? "" : rplchar) + rm1[j]["t_rem"].ToString();
                            }
                            if (class1 == "True")
                            {
                                string sel2 = "autono = '" + rm1[j]["autono"] + "' and dslno = " + Convert.ToInt32(rm1[j]["OSLNO"]);
                                var rm4 = TCLASS.Select(sel2);
                                if (rm4.Count() != 0)
                                {
                                    for (int z = 0; z <= rm4.Count() - 1; z++)
                                    {
                                        if (rm4[z]["class1cd"] != null && rm4[z]["class1cd"].ToString() != "")
                                        {
                                            if (nar1 != "") { nar1 = nar1 + rplchar; }
                                            nar1 += "Class1 :" + rm4[z]["class1cd"] + ":" + rm4[z]["class1nm"] + "    " + Convert.ToDouble(rm4[z]["amt"]).ToString("#######0.00");
                                        }
                                        if (rm4[z]["class2cd"] != null && rm4[z]["class2cd"].ToString() != "")
                                        {
                                            nar1 += (nar1 == "" ? "" : rplchar);
                                            nar1 += "Class2 :" + rm4[z]["class2cd"] + ":" + rm4[z]["class2nm"] + "    " + Convert.ToDouble(rm4[z]["amt"]).ToString("#######0.00");
                                        }
                                        if (rm4[z]["refcd"] != null && rm4[z]["refcd"].ToString() != "")
                                        {
                                            nar1 += (nar1 == "" ? "" : rplchar);
                                            nar1 += "Ref :" + rm4[z]["refcd"] + ":" + rm4[z]["refnm"] + "    " + Convert.ToDouble(rm4[z]["amt"]).ToString("#######0.00");
                                        }
                                    }
                                }
                            }
                            if (chq1 == "True")
                            {
                                if (rm1[j]["chqno"].ToString() != "" || rm1[j]["chqdt"].ToString() != "" || rm1[j]["bank_name"].ToString() != "")
                                {
                                    nar1 += (nar1 == "" ? "" : rplchar);
                                    if (rm1[j]["chqno"].ToString() != "" && reptype == "C")
                                    {
                                        nar1 += "Chq No. " + rm1[j]["chqno"] + " ";
                                    }
                                    if (rm1[j]["chqdt"].ToString() != "")
                                    {
                                        nar1 += "Chq Dt. " + rm1[j]["chqdt"].ToString().Substring(0, 10) + " ";
                                    }
                                    if (rm1[j]["bank_name"].ToString() != "")
                                    {
                                        nar1 += "Bank- " + rm1[j]["Bank_name"].ToString() + " ";
                                    }
                                    if (rm1[j]["rtgsno"].ToString() != "")
                                    {
                                        nar1 += "RTGS- " + rm1[j]["rtgsno"].ToString() + " ";
                                    }
                                }
                            }
                            if (bill1 == "True")
                            {
                                string sel2 = "autono = '" + rm1[j]["autono"] + "' and r_slno = " + Convert.ToInt32(rm1[j]["OSLNO"]);
                                var rm4 = TBL.Select(sel2);
                                if (rm4.Count() != 0)
                                {
                                    nar1 += (nar1 == "" ? "" : rplchar);

                                    for (int z = 0; z <= rm4.Count() - 1; z++)
                                    {
                                        nar1 += "BLNO. " + rm4[z]["blno"] + " Dt. " + rm4[z]["bldt"];
                                    }
                                }
                            }

                            if (rm1[j]["drcr"].ToString() == "D")
                            {
                                dcfld = "dr";
                                op1 = op1 + Convert.ToDouble(rm1[j]["amt"]);
                                tdramt = tdramt + Convert.ToDouble(rm1[j]["amt"]);
                            }
                            else
                            {
                                dcfld = "cr";
                                op1 = op1 - Convert.ToDouble(rm1[j]["amt"]);
                                tcramt = tcramt + Convert.ToDouble(rm1[j]["amt"]);
                            }
                            if (op1 > 0)
                            {
                                balnat = "Dr.";
                            }
                            if (op1 < 0)
                            {
                                balnat = "Cr.";
                            }

                            if (reptype == "C")
                            {
                                DataRow dtrow = FREC.NewRow();
                                if (VE.MENU_PARA == "SUBLEG")
                                {
                                    dtrow["m_pcd"] = rm1[j]["slcd"] + " [" + rm1[j]["glcd"] + "] ";
                                    if (COMLOC != "")
                                    {
                                        dtrow["m_pcdname"] = THDR.Rows[i]["compnm"].ToString() + (VE.Checkbox9 == true ? ", " + THDR.Rows[i]["locnm"].ToString() : "") + "\r\n"; // "^^";
                                    }
                                    dtrow["m_pcdname"] = dtrow["m_pcdname"].ToString() + THDR.Rows[i]["slnm"] + " (" + THDR.Rows[i]["slcity"] + ")  " + " [" + THDR.Rows[i]["glnm"] + "]";
                                    if (VE.Checkbox10 == true)
                                    {
                                        string cfld = "", sladd = "";
                                        for (int f = 1; f <= 6; f++)
                                        {
                                            cfld = "add" + Convert.ToString(f).ToString();
                                            if (THDR.Rows[i][cfld].ToString() != "")
                                            {
                                                sladd += THDR.Rows[i][cfld].ToString() + " ";
                                            }
                                        }
                                        sladd += THDR.Rows[i]["slstate"].ToString() + " [ Code - " + THDR.Rows[i]["slstatecd"].ToString() + " ]";
                                        dtrow["m_pcdadd1"] = sladd;
                                        if (THDR.Rows[i]["slpan"].ToString() != "")
                                        {
                                            dtrow["m_pcdadd2"] = "PAN # " + THDR.Rows[i]["slpan"].ToString();
                                        }
                                    }
                                }
                                else
                                {
                                    dtrow["m_pcd"] = rm1[j]["glcd"];
                                    if (COMLOC != "")
                                    {
                                        dtrow["m_pcdname"] = THDR.Rows[i]["compnm"].ToString() + (VE.Checkbox9 == true ? ", " + THDR.Rows[i]["locnm"].ToString() : "") + " " + ((char)13); // "^^";
                                    }
                                    dtrow["m_pcdname"] = dtrow["m_pcdname"].ToString() + THDR.Rows[i]["glnm"];
                                }
                                dtrow["m_vouno"] = rm1[j]["docno"].ToString();
                                dtrow["m_voudt"] = rm1[j]["docdt"].retDateStr();
                                dtrow["chqno"] = rm1[j]["chqno"].ToString();
                                if (drcrhead == "Y")
                                {
                                    dtrow["m_narration"] = nar1;
                                    dtrow["M_ACCHEAD"] = rm1[j]["dglnm"].ToString();
                                    if (rm1[j]["dslnm"].ToString() != "")
                                    {
                                        dtrow["M_ACCHEAD"] = dtrow["M_ACCHEAD"].ToString() + rplchar + rm1[j]["dslnm"].ToString();
                                    }
                                    dtrow["pcd1a"] = rm1[j]["r_glcd"].ToString() + " " + rm1[j]["r_slcd"].ToString();
                                    dtrow["b_tag"] = "B";
                                }
                                else
                                {
                                    dtrow["M_ACCHEAD"] = nar1;
                                    dtrow["pcd1a"] = rm1[j]["t_rem"];
                                    dtrow["b_tag"] = "";
                                }
                                dtrow["m_" + dcfld + "amt"] = rm1[j]["amt"];
                                dtrow["m_balance"] = Math.Abs(op1);
                                dtrow["bal_type"] = balnat;
                                FREC.Rows.Add(dtrow);
                            }
                            else
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                if (reptype != "R")
                                {
                                    if (COMLOC != "")
                                    {
                                        IR.Rows[rNo]["compcd"] = THDR.Rows[i]["compcd"].ToString();
                                        IR.Rows[rNo]["compnm"] = THDR.Rows[i]["compnm"].ToString();
                                        IR.Rows[rNo]["loccd"] = THDR.Rows[i]["loccd"].ToString();
                                        IR.Rows[rNo]["locnm"] = THDR.Rows[i]["locnm"].ToString();
                                    }
                                    IR.Rows[rNo]["glcd"] = THDR.Rows[i]["glcd"].ToString();
                                    IR.Rows[rNo]["glnm"] = THDR.Rows[i]["glnm"].ToString();
                                    IR.Rows[rNo]["slcd"] = THDR.Rows[i]["slcd"].ToString();
                                    IR.Rows[rNo]["slnm"] = THDR.Rows[i]["slnm"].ToString();
                                    if (VE.Checkbox10 == true)
                                    {
                                        string cfld = "", sladd = "";
                                        for (int f = 1; f <= 6; f++)
                                        {
                                            cfld = "add" + Convert.ToString(f).ToString();
                                            if (THDR.Rows[i][cfld].ToString() != "")
                                            {
                                                sladd += THDR.Rows[i][cfld].ToString() + " ";
                                            }
                                        }
                                        sladd += THDR.Rows[i]["slstate"].ToString() + " [ Code - " + THDR.Rows[i]["slstatecd"].ToString() + " ]";
                                        IR.Rows[rNo]["sladd1"] = sladd;
                                        if (THDR.Rows[i]["slpan"].ToString() != "")
                                        {
                                            IR.Rows[rNo]["sladd2"] = "PAN # " + THDR.Rows[i]["slpan"].ToString();
                                        }
                                    }
                                    IR.Rows[rNo]["doccd"] = rm1[j]["doccd"].ToString();
                                    nar1 = nar1.ToString().Replace(rplchar, "");
                                }
                                if (reptype == "R") IR.Rows[rNo]["docdt"] = rm1[j]["docdt"].retDateStr(dateform); else IR.Rows[rNo]["docdt"] = Convert.ToDateTime(rm1[j]["docdt"].ToString().retDateStr("yyyy", dateform));
                                IR.Rows[rNo]["chqno"] = rm1[j]["chqno"].ToString();
                                IR.Rows[rNo]["type"] = (rm1[j]["dshortnm"].retStr() == "" ? CommVar.retTrCD(rm1[j]["trcd"].retStr()) : rm1[j]["dshortnm"].retStr());
                                if (VE.Checkbox12 == true) IR.Rows[rNo]["txntype"] = masterhelpfa.retIntTds(rm1[j]["inttds"].retStr());
                                if (VE.Checkbox11 == true && rm1[j]["qnty"].retDbl() != 0) IR.Rows[rNo]["qnty"] = rm1[j]["qnty"].retDbl();
                                IR.Rows[rNo]["bank_dt"] = rm1[j]["bank_dt"].ToString().retDateStr("yyyy", dateform);
                                IR.Rows[rNo]["docno"] = rm1[j]["docno"].ToString();
                                if (VE.MENU_PARA == "CASH" || VE.MENU_PARA == "BANK")
                                {
                                    string rplglnm = rm1[j]["dglnm"].ToString();
                                    if (rm1[j]["dslnm"].ToString() != "") rplglnm += (rplglnm == "" ? "" : rplchar) + rm1[j]["dslnm"].ToString();
                                    if (nar1 != "") nar1 = rplglnm + (rplglnm == "" ? "" : rplchar) + nar1; else nar1 = rplglnm;
                                }
                                string nar2 = "";
                                if (PrintShareDetails == true)
                                {
                                    string autono = rm1[j]["autono"].ToString();
                                    var tempshrdet = TSHRTXNDTL.Select("autono='" + rm1[j]["autono"].ToString() + "'");
                                    if (tempshrdet.Count() > 0)
                                    {
                                        double sttamt = 0;
                                        for (int a = 0; a <= tempshrdet.Count() - 1; a++)
                                        {
                                            sttamt += tempshrdet[a]["sttamt"].retDbl();
                                            nar2 += rplchar;
                                            nar2 += tempshrdet[a]["shrnm"].ToString() + "    ";
                                            if (tempshrdet[a]["CTGNM"].retStr().Length > 2)
                                            {
                                                nar2 += tempshrdet[a]["CTGNM"].retStr().Substring(0, 2) + "    ";
                                            }
                                            else
                                            {
                                                nar2 += tempshrdet[a]["CTGNM"].retStr() + "    ";
                                            }
                                            if (tempshrdet[a]["DRCR"].retStr() == "D")
                                            {
                                                nar2 += "+" + "    ";
                                            }
                                            else
                                            {
                                                nar2 += "-" + "    ";
                                            }
                                            nar2 += tempshrdet[a]["qty"].ToString() + "    ";
                                            nar2 += Cn.Indian_Number_format(tempshrdet[a]["rate"].retDbl().retStr(), "0.0000") + "    ";
                                            nar2 += tempshrdet[a]["amt"].retDbl().ToINRFormat();
                                        }
                                        if (sttamt.retDbl() != 0)
                                        {
                                            nar2 += rplchar;
                                            nar2 += "STT : ";
                                            nar2 += sttamt.ToINRFormat();
                                        }

                                    }
                                }
                                IR.Rows[rNo]["trem"] = nar1 + nar2;
                                IR.Rows[rNo][dcfld + "amt"] = rm1[j]["amt"];
                                if (RunnBal1 == "Y")
                                {
                                    if (reptype == "R")
                                    {
                                        IR.Rows[rNo]["runbal"] = Math.Abs(op1).ToINRFormat();
                                        IR.Rows[rNo]["drcr"] = balnat;
                                    }
                                    else if (reptype == "E" || reptype == "V")
                                    {
                                        IR.Rows[rNo]["runbal"] = Math.Abs(op1).retDbl();
                                        IR.Rows[rNo]["drcr"] = balnat;
                                    }
                                    else
                                    {
                                        IR.Rows[rNo]["runbal"] = Math.Abs(op1).ToINRFormat() + " " + balnat;
                                    }
                                    if (reptype == "R") IR.Rows[rNo]["celldesign"] = "runbal=text-align:right^;";
                                }
                                #region Print Totals
                                bool flag = false;
                                if (reptype == "R" && total == "M" && j != rm1.Count() - 1)
                                {
                                    currentmonth = Convert.ToDateTime(rm1[j]["docdt"].retStr()).Month;
                                    nextmonth = Convert.ToDateTime(rm1[j + 1]["docdt"].retStr()).Month;
                                }
                                if (reptype == "R" && total == "Y" && j != rm1.Count() - 1)
                                {
                                    currentyr = Convert.ToDateTime(rm1[j]["to_date"].retStr()).Year;
                                    nextyr = Convert.ToDateTime(rm1[j + 1]["to_date"].retStr()).Year;
                                }
                                if (reptype == "R" && total == "D" && ((j) == rm1.Count() - 1 || rm1[j]["docdt"].retStr() != rm1[j + 1]["docdt"].retStr())) flag = true;
                                if (reptype == "R" && total == "M" && ((j) == rm1.Count() - 1 || currentmonth != nextmonth)) flag = true;
                                if (reptype == "R" && total == "Y" && ((j) == rm1.Count() - 1 || currentyr != nextyr)) flag = true;
                                if (reptype == "R" && total == "G" && ((j) == rm1.Count() - 1)) flag = true;
                                if (flag == true)
                                {
                                    if (ShowOpinTotal == true)
                                    {
                                        if (topamt > 0)
                                        {
                                            tdramt = tdramt + topamt;
                                        }
                                        if (topamt < 0)
                                        {
                                            tcramt = tcramt + Math.Abs(topamt);
                                        }
                                        topamt = op1;
                                    }
                                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                    IR.Rows[rNo]["trem"] = "Totals" + (total == "M" ? " (Month Wise)" : total == "D" ? " (Day Wise)" : total == "Y" ? " (Year Wise)" : "");
                                    IR.Rows[rNo]["dramt"] = tdramt;
                                    IR.Rows[rNo]["cramt"] = tcramt;
                                    if (reptype == "R") IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                                    if (RunnBal1 == "Y")
                                    {
                                        if (reptype == "R")
                                        {
                                            IR.Rows[rNo]["runbal"] = Math.Abs(op1).ToINRFormat();
                                            IR.Rows[rNo]["drcr"] = balnat;
                                        }
                                        else if (reptype == "E" || reptype == "V")
                                        {
                                            IR.Rows[rNo]["runbal"] = Math.Abs(op1).retDbl();
                                            IR.Rows[rNo]["drcr"] = balnat;
                                        }
                                        else
                                        {
                                            IR.Rows[rNo]["runbal"] = Math.Abs(op1).ToINRFormat() + " " + balnat;
                                        }
                                        if (reptype == "R") IR.Rows[rNo]["celldesign"] = "runbal=text-align:right^;";
                                    }
                                    else
                                    {
                                        //IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                        //if (reptype == "R") IR.Rows[rNo]["docdt"] = TD.retDateStr(dateform); else IR.Rows[rNo]["docdt"] = Convert.ToDateTime(TD.retDateStr(dateform));
                                        //IR.Rows[rNo]["trem"] = "Closing Balance";
                                        //if (op1 < 0) IR.Rows[rNo]["cramt"] = Math.Abs(op1); else IR.Rows[rNo]["dramt"] = Math.Abs(op1);
                                        //IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                                    }
                                    //if (reptype == "R" && total == "M" && j != rm1.Count() - 1)
                                    //{
                                    DateTime dt = Convert.ToDateTime(rm1[j]["docdt"].retStr());
                                    DateTime firstDayOfMonth = new DateTime(dt.Year, dt.Month, (total == "D" ? dt.Day : 1));
                                    if (total == "G") firstDayOfMonth = Convert.ToDateTime(VE.TDT);
                                    if (total == "Y")
                                    {
                                        if (Convert.ToDateTime(TD) <= Convert.ToDateTime(rm1[j]["to_date"].retStr()))
                                        {
                                            firstDayOfMonth = Convert.ToDateTime(TD);
                                        }
                                        else
                                        {
                                            firstDayOfMonth = Convert.ToDateTime(rm1[j]["to_date"].retStr());
                                        }
                                    }
                                    if (RunnBal1 != "Y")
                                    {
                                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                        IR.Rows[rNo]["trem"] = "Balance c/f";
                                        if (total == "M")
                                        {
                                            IR.Rows[rNo]["docdt"] = (firstDayOfMonth.AddMonths(1)).retDateStr();
                                        }
                                        else if (total == "D")
                                        {
                                            IR.Rows[rNo]["docdt"] = (firstDayOfMonth.AddDays(1)).retDateStr();
                                        }
                                        else if (total == "Y")
                                        {
                                            IR.Rows[rNo]["docdt"] = (firstDayOfMonth.AddDays(1)).retDateStr();
                                        }
                                        else
                                        {
                                            IR.Rows[rNo]["docdt"] = (firstDayOfMonth.AddDays(1)).retDateStr();
                                        }
                                        if (balnat == "Dr.")
                                        {
                                            IR.Rows[rNo]["dramt"] = Math.Abs(op1).ToINRFormat();
                                        }
                                        else
                                        {
                                            IR.Rows[rNo]["cramt"] = Math.Abs(op1).ToINRFormat();
                                        }
                                        if (reptype == "R") IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                                    }
                                    //}
                                    tdramt = 0; tcramt = 0;
                                    #endregion
                                }
                            }
                        }
                    }
                    else
                    {
                        //if (reptype == "R" && total == "M" && (tdramt + tcramt) != 0)
                        //{
                        //    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        //    IR.Rows[rNo]["trem"] = "Totals";
                        //    IR.Rows[rNo]["dramt"] = tdramt;
                        //    IR.Rows[rNo]["cramt"] = tcramt;
                        //    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                        //    if (RunnBal1 == "Y")
                        //    {
                        //        IR.Rows[rNo]["runbal"] = Math.Abs(op1).ToINRFormat();
                        //        IR.Rows[rNo]["drcr"] = balnat;
                        //    }
                        //    IR.Rows[rNo]["celldesign"] = "runbal=text-align:right^;";
                        //}
                    }
                    #endregion

                    #region Print Closing Balance
                    //if (dataexist == true && reptype == "R")
                    if (dataexist == true && reptype == "R" && rm1.Count() == 0 && op1 != 0)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        if (reptype != "R")
                        {
                            if (COMLOC != "")
                            {
                                IR.Rows[rNo]["compcd"] = THDR.Rows[i]["compcd"].ToString();
                                IR.Rows[rNo]["compnm"] = THDR.Rows[i]["compnm"].ToString();
                                IR.Rows[rNo]["loccd"] = THDR.Rows[i]["loccd"].ToString();
                                IR.Rows[rNo]["locnm"] = THDR.Rows[i]["locnm"].ToString();
                            }
                            IR.Rows[rNo]["glcd"] = THDR.Rows[i]["glcd"].ToString();
                            IR.Rows[rNo]["glnm"] = THDR.Rows[i]["glnm"].ToString();
                            IR.Rows[rNo]["slcd"] = THDR.Rows[i]["slcd"].ToString();
                            IR.Rows[rNo]["slnm"] = THDR.Rows[i]["slnm"].ToString();
                            if (VE.Checkbox10 == true)
                            {
                                string cfld = "", sladd = "";
                                for (int f = 1; f <= 6; f++)
                                {
                                    cfld = "add" + Convert.ToString(f).ToString();
                                    if (THDR.Rows[i][cfld].ToString() != "")
                                    {
                                        sladd += THDR.Rows[i][cfld].ToString() + " ";
                                    }
                                }
                                sladd += THDR.Rows[i]["slstate"].ToString() + " [ Code - " + THDR.Rows[i]["slstatecd"].ToString() + " ]";
                                IR.Rows[rNo]["sladd1"] = sladd;
                                if (THDR.Rows[i]["slpan"].ToString() != "")
                                {
                                    IR.Rows[rNo]["sladd2"] = "PAN # " + THDR.Rows[i]["slpan"].ToString();
                                }
                            }
                        }
                        if (ShowOpinTotal == true)
                        {
                            if (op1 > 0)
                            {
                                tdramt = tdramt + topamt;
                            }
                            if (op1 < 0)
                            {
                                tcramt = tcramt + Math.Abs(topamt);
                            }
                        }
                        IR.Rows[rNo]["trem"] = "Totals";
                        IR.Rows[rNo]["dramt"] = tdramt;
                        IR.Rows[rNo]["cramt"] = tcramt;
                        if (reptype == "R") IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                        if (RunnBal1 == "Y")
                        {
                            if (reptype == "R")
                            {
                                IR.Rows[rNo]["runbal"] = Math.Abs(op1).ToINRFormat();
                                IR.Rows[rNo]["drcr"] = balnat;
                            }
                            else if (reptype == "E" || reptype == "V")
                            {
                                IR.Rows[rNo]["runbal"] = Math.Abs(op1).retDbl();
                                IR.Rows[rNo]["drcr"] = balnat;
                            }
                            else
                            {
                                IR.Rows[rNo]["runbal"] = Math.Abs(op1).ToINRFormat() + " " + balnat;
                            }
                            if (reptype == "R") IR.Rows[rNo]["celldesign"] = "runbal=text-align:right^;";
                        }
                        if (RunnBal1 != "Y")
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            if (reptype == "R") IR.Rows[rNo]["docdt"] = TD.retDateStr(dateform); else IR.Rows[rNo]["docdt"] = Convert.ToDateTime(TD.retDateStr(dateform));
                            IR.Rows[rNo]["trem"] = "Balance c/f";
                            if (op1 < 0) IR.Rows[rNo]["cramt"] = Math.Abs(op1); else IR.Rows[rNo]["dramt"] = Math.Abs(op1);
                            if (reptype == "R") IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                        }
                    }
                    #endregion
                }
                if (reptype == "C")
                {
                    string compaddress = MasterHelp.retCompAddress();
                    DataTable comptbl = MasterHelp.retComptbl();
                    string COMPPAN = (comptbl.Rows[0]["panno"].retStr() == "" || VE.Checkbox10 == false) ? "" : "PAN # " + comptbl.Rows[0]["panno"].ToString() + " ";
                    string COMPADD = (compaddress.retCompValue("compadd").retStr() == "" || VE.Checkbox10 == false) ? "" : compaddress.retCompValue("compadd");
                    ReportDocument reportdocument = new ReportDocument();
                    reportdocument.Load(Server.MapPath("~/Report/Rep_Ledger.rpt"));
                    reportdocument.SetDataSource(FREC);
                    //reportdocument.SetParameterValue("head", CommVar.CompName(UNQSNO));
                    reportdocument.SetParameterValue("head", compaddress.retCompValue("compnm"));
                    reportdocument.SetParameterValue("formerlynm", compaddress.retCompValue("formerlynm"));
                    reportdocument.SetParameterValue("head1", CommVar.LocName(UNQSNO).ToString());
                    reportdocument.SetParameterValue("compadd", COMPADD);
                    reportdocument.SetParameterValue("comppan", COMPPAN);
                    reportdocument.SetParameterValue("head2", Para2);
                    reportdocument.SetParameterValue("RunnBal", RunnBal1);
                    reportdocument.SetParameterValue("DrCrHead", drcrhead);
                    reportdocument.SetParameterValue("PrintPageNo", "Y");
                    reportdocument.SetParameterValue("PageBreak", "N");
                    reportdocument.SetParameterValue("PrintCode", "Y");
                    reportdocument.SetParameterValue("RepGroup", "No Group");
                    reportdocument.SetParameterValue("NrrOpt", "Y");
                    reportdocument.SetParameterValue("PrintDate", System.DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + " User " + Session["UR_ID"]);
                    Response.Buffer = false;
                    Response.ClearContent();
                    Response.ClearHeaders();
                    Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                    reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
                    stream.Seek(0, SeekOrigin.Begin);
                    return new FileStreamResult(stream, "application/pdf");
                }
                else if (reptype == "R")
                {
                    string PARAEXT = "";
                    if (VE.MENU_PARA == "SUBLEG" && slcd != "" && slcd.Split(',').Length == 1)
                    {
                        PARAEXT = ShowLeginHeader;
                    }
                    string pghdr1 = Para2;
                    pghdr1 = Para2;
                    if (VE.Checkbox10 == true)
                    {
                        string compaddress = MasterHelp.retCompAddress();
                        DataTable comptbl = MasterHelp.retComptbl();
                        string COMPPAN = (comptbl.Rows[0]["panno"].retStr() == "" || VE.Checkbox10 == false) ? "" : "</br>" + "PAN # " + comptbl.Rows[0]["panno"].ToString() + " ";
                        string COMPADD = (compaddress.retCompValue("compadd").retStr() == "" || VE.Checkbox10 == false) ? "" : compaddress.retCompValue("compadd");
                        Para2 = COMPADD + COMPPAN + "</br>" + Para2;

                    }
                    PV = HC.ShowReport(IR, repname, Para2, PARAEXT, true, true, "P", false);

                    TempData[repname] = PV;
                    return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
                }
                else
                {
                    if (reptype == "E")
                    {
                        if (VE.Checkbox10 == true)
                        {
                            string compaddress = MasterHelp.retCompAddress();
                            DataTable comptbl = MasterHelp.retComptbl();
                            string COMPPAN = (comptbl.Rows[0]["panno"].retStr() == "" || VE.Checkbox10 == false) ? "" : "PAN # " + comptbl.Rows[0]["panno"].ToString() + " ";
                            string COMPADD = (compaddress.retCompValue("compadd").retStr() == "" || VE.Checkbox10 == false) ? "" : compaddress.retCompValue("compadd");
                            Para2 += "</br>" + COMPADD + COMPPAN;
                        }
                        DataTable[] exdt = new DataTable[1];
                        exdt[0] = IR;
                        string[] sheetname = new string[1];
                        sheetname[0] = "Sheet1";
                        //MasterHelp.ExcelfromDataTables(exdt, sheetname, repname, false, Para2, true, true);
                        return Content("Downloaded");
                    }
                    else
                    {
                        string strfilename = MasterHelp.DataTbltoCSV(repname, "", IR, Cn.GCS());
                        return Content(strfilename + " file generated in download folder...");
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public ActionResult ShowReport()
        {
            return RedirectToAction("ResponsivePrintViewer", "RPTViewer");
        }
        public ActionResult PrintReport()
        {
            return RedirectToAction("PrintViewer", "RPTViewer");
        }
    }
}