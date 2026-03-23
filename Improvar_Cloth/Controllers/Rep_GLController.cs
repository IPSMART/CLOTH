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

                VE.DropDown_list_GLCD = DropDownHelp.DropDown_list_GLCD("D,C", slcdmust);
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
                    //acsel1 = " and B.GLCD in (" + acsel1 + ") ";
                }
                string selslcdgrpcd = "";
                if (FC.AllKeys.Contains("slcdgrpcdvalue")) selslcdgrpcd = CommFunc.retSqlformat(FC["slcdgrpcdvalue"].ToString());
                string slsel1 = "", slunsel1 = ""; string slcd = "";
                if (FC.AllKeys.Contains("slcdvalue"))
                {
                    slsel1 = FC["slcdvalue"].ToString().retSqlformat();
                    //if (slsel1 != "") slsel1 = " and d.SLCD in (" + slsel1 + ") ";
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

                sql1 = "";
                for (int fi = 0; fi <= rsFinYr.Rows.Count - 1; fi++)
                {
                    string FinSchm = rsFinYr.Rows[fi]["schema_name"].retStr();
                    if (fi > 0) sql1 += " union all ";

                    sql1 += "select b.compcd,h.compnm,b.loccd,g.locnm,a.autono, a.agslcd, e.slnm agslnm, c.glnm, a.glcd, d.slnm,RTRIM(d.district|| '-' ||d.slarea,'-') slcity, a.slcd, a.docdt, a.docno,  " + Environment.NewLine;
                    sql1 += "y.trcd, x.t_rem, x.chqno, x.chqdt, y.pay_by, y.bank_code,y.paid_to,x.rtgsno, f.glnm bankglnm, f.shortnm, a.amt,a.dramt,a.cramt,a.drcr, " + Environment.NewLine;
                    sql1 += "d.add1,d.add2,d.add3,d.add4,d.add5,d.add6,d.phno1std sltel1,d.phno1 sltel2,d.panno slpan,d.gstno slgstno, d.state slstate, d.statecd slstatecd, " + Environment.NewLine;
                    sql1 += "i.slnm dslnm, i.district dslcity,x.bank_name,j.glnm dglnm,j.shortnm dshortnm,'" + rsFinYr.Rows[fi]["from_date"].retStr() + "' from_date,'" + rsFinYr.Rows[fi]["to_date"].retStr() + "' to_date, " + Environment.NewLine;
                    sql1 += "x.inttds,x.bank_dt,x.qnty " + Environment.NewLine;
                    sql1 += "from ( " + Environment.NewLine;

                    sql1 += "select a.autono, a.glcd || a.slcd glslcd, a.agslcd, a.glcd, a.slcd, a.docdt, a.docno, " + Environment.NewLine;
                    sql1 += "sum(case a.drcr when 'D' then a.amt else a.amt * -1 end) amt, " + Environment.NewLine;
                    sql1 += "sum(case a.drcr when 'D' then a.amt else 0 end) dramt, " + Environment.NewLine;
                    sql1 += "sum(case a.drcr when 'C' then a.amt else 0 end) cramt,a.drcr  " + Environment.NewLine;
                    sql1 += "from( " + Environment.NewLine;

                    sql1 += "select a.autono, a.slno, a.glcd, a.slcd, a.agslcd, b.docno, b.docdt, a.blno, a.bldt, a.drcr, a.amt " + Environment.NewLine;
                    sql1 += "from " + FScm1 + ".t_vch_bl a, " + FScm1 + ".t_cntrl_hdr b " + Environment.NewLine;
                    sql1 += "where a.autono = b.autono(+)  " + Environment.NewLine;
                    if (VE.Checkbox9 == true && COMLOC == "") sql1 += " and b.loccd = '" + loc + "' " + Environment.NewLine;
                    if (COMLOC != "") sql1 += " and b.compcd||b.loccd in (" + COMLOC + ") " + Environment.NewLine; else sql1 += " and b.compcd='" + com + "' " + Environment.NewLine;
                    sql1 += "and nvl(b.cancel, 'N') = 'N'  " + Environment.NewLine;
                    if (acsel1.retStr() != "") sql1 += "and a.glcd in (" + acsel1 + ") " + Environment.NewLine;
                    if (slsel1.retStr() != "") sql1 += "and a.slcd in (" + slsel1 + ") " + Environment.NewLine;
                    sql1 += "union all " + Environment.NewLine;
                    sql1 += "select a.autono, a.slno, c.glcd, c.slcd, c.agslcd, b.docno, b.docdt, c.blno, c.bldt, decode(c.drcr, 'D', 'C', 'D') drcr, a.adj_amt amt " + Environment.NewLine;
                    sql1 += "from " + FScm1 + ".t_vch_bl_adj a, " + FScm1 + ".t_cntrl_hdr b, " + FScm1 + ".t_vch_bl c " + Environment.NewLine;
                    sql1 += "where a.autono = b.autono(+) and a.i_autono || a.i_slno = c.autono || c.slno  " + Environment.NewLine;
                    if (VE.Checkbox9 == true && COMLOC == "") sql1 += " and b.loccd = '" + loc + "' " + Environment.NewLine;
                    if (COMLOC != "") sql1 += " and b.compcd||b.loccd in (" + COMLOC + ") " + Environment.NewLine; else sql1 += " and b.compcd='" + com + "' " + Environment.NewLine;
                    sql1 += "and nvl(b.cancel, 'N') = 'N'  " + Environment.NewLine;
                    if (acsel1.retStr() != "") sql1 += "and c.glcd in (" + acsel1 + ") " + Environment.NewLine;
                    if (slsel1.retStr() != "") sql1 += "and c.slcd in (" + slsel1 + ") " + Environment.NewLine;
                    sql1 += "union all " + Environment.NewLine;
                    sql1 += "select a.autono, a.slno, c.glcd, c.slcd, c.agslcd, b.docno, b.docdt, c.blno, c.bldt, decode(c.drcr, 'C', 'D', 'C') drcr, a.adj_amt amt " + Environment.NewLine;
                    sql1 += "from " + FScm1 + ".t_vch_bl_adj a, " + FScm1 + ".t_cntrl_hdr b, " + FScm1 + ".t_vch_bl c " + Environment.NewLine;
                    sql1 += "where a.autono = b.autono(+) and a.r_autono || a.r_slno = c.autono || c.slno " + Environment.NewLine;
                    if (VE.Checkbox9 == true && COMLOC == "") sql1 += " and b.loccd = '" + loc + "' " + Environment.NewLine;
                    if (COMLOC != "") sql1 += " and b.compcd||b.loccd in (" + COMLOC + ") " + Environment.NewLine; else sql1 += " and b.compcd='" + com + "' " + Environment.NewLine;
                    sql1 += "and nvl(b.cancel, 'N') = 'N'  " + Environment.NewLine;
                    if (acsel1.retStr() != "") sql1 += "and c.glcd in (" + acsel1 + ") " + Environment.NewLine;
                    if (slsel1.retStr() != "") sql1 += "and c.slcd in (" + slsel1 + ") " + Environment.NewLine;
                    sql1 += ") a group by autono, a.glcd || a.slcd, agslcd, glcd, slcd, docdt, docno,drcr ) a, " + Environment.NewLine;
                    sql1 += " " + Environment.NewLine;
                    sql1 += "(select a.autono, a.glcd || a.slcd glslcd, max(a.t_rem) t_rem,  " + Environment.NewLine;
                    sql1 += "max(a.chqno) chqno, max(a.chqdt) chqdt,max(r_slcd)r_slcd,max(rtgsno)rtgsno,max(bank_name)bank_name,max(r_glcd)r_glcd,max(inttds)inttds,max(bank_dt)bank_dt,sum(qty)qnty " + Environment.NewLine;
                    sql1 += "from " + FScm1 + ".t_vch_det a " + Environment.NewLine;
                    sql1 += "group by a.autono, a.glcd || a.slcd ) x, " + Environment.NewLine;
                    sql1 += " " + Environment.NewLine;
                    sql1 += "" + FScm1 + ".t_cntrl_hdr b, " + FScm1 + ".t_vch_hdr y, " + Environment.NewLine;
                    sql1 += "" + FScm1 + ".m_genleg c, " + FScm1 + ".m_subleg d, " + FScm1 + ".m_subleg e, " + FScm1 + ".m_genleg f, " + FScm1 + ".m_loca g, " + FScm1 + ".m_comp h, " + Environment.NewLine;
                    sql1 += "" + FScm1 + ".m_subleg i," + FScm1 + ".m_genleg j " + Environment.NewLine;
                    sql1 += "where a.autono = b.autono(+) and a.autono = x.autono(+) and " + Environment.NewLine;
                    sql1 += "a.glslcd = x.glslcd(+) and a.autono = y.autono(+) and y.bank_code = f.glcd(+) and " + Environment.NewLine;
                    sql1 += "a.glcd = c.glcd(+) and a.slcd = d.slcd(+) and a.agslcd = e.slcd(+) and b.compcd=g.compcd(+) and b.loccd=g.loccd(+) and b.compcd=h.compcd(+)  " + Environment.NewLine;
                    sql1 += "and x.r_slcd=i.slcd(+) and x.r_glcd=j.glcd(+) " + Environment.NewLine;
                    if (VE.Checkbox9 == true && COMLOC == "") sql1 += " and b.loccd = '" + loc + "' " + Environment.NewLine;
                    if (COMLOC != "") sql1 += " and b.compcd||b.loccd in (" + COMLOC + ") " + Environment.NewLine; else sql1 += " and b.compcd='" + com + "' " + Environment.NewLine;
                    sql1 += "and nvl(b.cancel, 'N')= 'N' and " + Environment.NewLine;
                    sql1 += "b.docdt <= to_date('" + TD + "', 'dd/mm/yyyy') " + Environment.NewLine;
                    sql1 += "order by h.compnm,h.compcd,g.locnm,g.loccd,agslnm, agslcd, glnm, glcd, slnm, slcd, docdt, docno " + Environment.NewLine;
                }
                DataTable tbl = MasterHelp.SQLquery(sql1);

                //DataTable TVCH = new DataTable();
                //Cn.com = new OracleCommand(sql1, Cn.con);
                //Cn.da.SelectCommand = Cn.com;
                //Cn.da.Fill(TVCH);

                //DataTable TOP = new DataTable();
                //Cn.com = new OracleCommand(sql2, Cn.con);
                //Cn.da.SelectCommand = Cn.com;
                //Cn.da.Fill(TOP);

                DataTable TCLASS = new DataTable();
                //Cn.com = new OracleCommand(sql3, Cn.con);
                //Cn.da.SelectCommand = Cn.com;
                //Cn.da.Fill(TCLASS);

                //DataTable THDR = new DataTable();
                //Cn.com = new OracleCommand(sql4, Cn.con);
                //Cn.da.SelectCommand = Cn.com;
                //Cn.da.Fill(THDR);

                //DataTable TBL = new DataTable();
                //Cn.com = new OracleCommand(sql5, Cn.con);
                //Cn.da.SelectCommand = Cn.com;
                //Cn.da.Fill(TBL);

                Cn.con.Close();

                //DataTable TSHRTXNDTL = MasterHelp.SQLquery(sql6);

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                Int32 rNo = 0;
                string balnat = "", dcfld = "";
                int currentmonth = 0, nextmonth = 0, currentyr = 0, nextyr = 0;
                DataTable IR = new DataTable("");

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



                if (tbl.Rows.Count == 0)
                {
                    return Content("No Records Found");
                }
                string dateform = "dd/MM/yyyy";
                string ShowLeginHeader = "";

                Int32 i = 0;
                Int32 maxR = 0;

                rNo = 0;
                double tdramt = 0, tcramt = 0, trunbal = 0;
                double totaldramt = 0, totalcramt = 0, totalrunbal = 0;

                // Report begins
                i = 0; maxR = tbl.Rows.Count - 1;
                while (i <= maxR)
                {
                    string comploccd = tbl.Rows[i]["compcd"].retStr() + tbl.Rows[i]["loccd"].retStr();
                    if (COMLOC != "")
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " </span>" + tbl.Rows[i]["compnm"].ToString() + (VE.Checkbox9 == true ? ", " + tbl.Rows[i]["locnm"].ToString() : "");
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                    }
                    while (tbl.Rows[i]["compcd"].retStr() + tbl.Rows[i]["loccd"].retStr() == comploccd)
                    {
                        string agslcd = tbl.Rows[i]["agslcd"].retStr();

                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["Dammy"] = "Agent-" + "<span style='font-weight:100;font-size:9px;'>" + " " + tbl.Rows[i]["agslcd"].ToString() + "  " + " </span>" + tbl.Rows[i]["agslnm"].ToString();
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                        double a_runbal = 0;
                        double a_dramt = 0, a_cramt = 0;

                        while (tbl.Rows[i]["compcd"].retStr() + tbl.Rows[i]["loccd"].retStr() == comploccd && tbl.Rows[i]["agslcd"].retStr() == agslcd)
                        {
                            string slglcd = tbl.Rows[i]["slcd"].retStr() + tbl.Rows[i]["glcd"].retStr();

                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["Dammy"] = "Party-<span style='font-weight:100;font-size:9px;'>" + " " + tbl.Rows[i]["slcd"].ToString() + "  " + " </span>" + tbl.Rows[i]["slnm"].ToString() + " [" + tbl.Rows[i]["slcity"].ToString() + "]";
                            IR.Rows[rNo]["Dammy"] = IR.Rows[rNo]["Dammy"] + "<span style='font-weight:100;font-size:9px;'>" + " [" + tbl.Rows[i]["glcd"].ToString() + "] " + " </span>" + tbl.Rows[i]["glnm"].ToString();
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";

                            double op = 0;
                            while (tbl.Rows[i]["compcd"].retStr() + tbl.Rows[i]["loccd"].retStr() == comploccd && tbl.Rows[i]["agslcd"].retStr() == agslcd && tbl.Rows[i]["slcd"].retStr() + tbl.Rows[i]["glcd"].retStr() == slglcd && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(FD))
                            {
                                op += tbl.Rows[i]["amt"].retDbl();
                                i = i + 1;
                                if (i > maxR) break;
                            }
                            if (op != 0)
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["docdt"] = FD.retDateStr(dateform);
                                IR.Rows[rNo]["trem"] = "Opening Balance";
                                if (op > 0)
                                {
                                    IR.Rows[rNo]["dramt"] = Math.Abs(op);
                                    a_dramt += op;
                                    tdramt += op;
                                    totaldramt += op;
                                }
                                else
                                {
                                    IR.Rows[rNo]["cramt"] = Math.Abs(op);
                                    a_cramt += op;
                                    tcramt += op;
                                    totalcramt += op;
                                }
                                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px";
                                IR.Rows[rNo]["runbal"] = Math.Abs(op).retDbl();
                                IR.Rows[rNo]["drcr"] = op > 0 ? "Dr." : "Cr.";

                                a_runbal += op;
                                trunbal += op;
                                totalrunbal += op;

                            }
                            double p_dramt = 0, p_cramt = 0, p_runbal = op;
                            if (op > 0)
                            {
                                p_dramt += op;
                            }
                            else
                            {
                                p_cramt += op;
                            }
                            while (tbl.Rows[i]["compcd"].retStr() + tbl.Rows[i]["loccd"].retStr() == comploccd && tbl.Rows[i]["agslcd"].retStr() == agslcd && tbl.Rows[i]["slcd"].retStr() + tbl.Rows[i]["glcd"].retStr() == slglcd)
                            {
                                #region remarks
                                string nar1 = "", rplchar = "<br />";
                                if (sublegshow == true && (VE.MENU_PARA != "CASH" && VE.MENU_PARA != "BANK"))
                                {
                                    if (tbl.Rows[i]["slnm"].ToString() != "") nar1 = tbl.Rows[i]["slnm"].ToString();
                                    if (tbl.Rows[i]["slnm"].ToString() == "" && tbl.Rows[i]["dslnm"].ToString() != "") nar1 = tbl.Rows[i]["dslnm"].ToString();
                                    if (VE.Checkbox10 == true && tbl.Rows[i]["slnm"].ToString() != "")
                                    {
                                        string sladd = "";
                                        string cfld = "";
                                        for (int f = 1; f <= 6; f++)
                                        {
                                            cfld = "add" + Convert.ToString(f).ToString();
                                            if (tbl.Rows[i][cfld].ToString() != "")
                                            {
                                                sladd += tbl.Rows[i][cfld].ToString() + " ";
                                            }
                                        }
                                        sladd += tbl.Rows[i]["slstate"].ToString() + " [ Code - " + tbl.Rows[i]["slstatecd"].ToString() + " ]";
                                        if (tbl.Rows[i]["slpan"].ToString() != "")
                                        {
                                            sladd += rplchar + "PAN # " + tbl.Rows[i]["slpan"].ToString();
                                        }
                                        nar1 += rplchar + sladd;
                                    }
                                }
                                if (printtype == "Y") nar1 += (nar1 == "" ? "" : rplchar) + CommVar.retTrCD(tbl.Rows[i]["trcd"].ToString());
                                if ((tbl.Rows[i]["paid_to"].ToString() != "" && tbl.Rows[i]["paid_to"].ToString() != tbl.Rows[i]["slcd"].retStr())) // && (VE.MENU_PARA == "CASH" || VE.MENU_PARA == "BANK" || VE.MENU_PARA == "SUBLEG"))
                                {
                                    nar1 += "Pay / receipt : " + tbl.Rows[i]["paid_to"].ToString();
                                }
                                if (tbl.Rows[i]["t_rem"].ToString() != "")
                                {
                                    if (printtype == "Y" && tbl.Rows[i]["trcd"].ToString() == "JV") nar1 += (nar1 == "" ? "" : rplchar) + tbl.Rows[i]["t_rem"].ToString();
                                    else if (printtype == "N") nar1 += (nar1 == "" ? "" : rplchar) + tbl.Rows[i]["t_rem"].ToString();
                                }
                                if (chq1 == "True")
                                {
                                    if (tbl.Rows[i]["chqno"].ToString() != "" || tbl.Rows[i]["chqdt"].ToString() != "" || tbl.Rows[i]["bank_name"].ToString() != "")
                                    {
                                        nar1 += (nar1 == "" ? "" : rplchar);
                                        if (tbl.Rows[i]["chqno"].ToString() != "" && reptype == "C")
                                        {
                                            nar1 += "Chq No. " + tbl.Rows[i]["chqno"] + " ";
                                        }
                                        if (tbl.Rows[i]["chqdt"].ToString() != "")
                                        {
                                            nar1 += "Chq Dt. " + tbl.Rows[i]["chqdt"].ToString().Substring(0, 10) + " ";
                                        }
                                        if (tbl.Rows[i]["bank_name"].ToString() != "")
                                        {
                                            nar1 += "Bank- " + tbl.Rows[i]["bank_name"].ToString() + " ";
                                        }
                                        if (tbl.Rows[i]["rtgsno"].ToString() != "")
                                        {
                                            nar1 += "RTGS- " + tbl.Rows[i]["rtgsno"].ToString() + " ";
                                        }
                                    }
                                }
                                if (bill1 == "True")
                                {
                                    string sel2 = "autono = '" + tbl.Rows[i]["autono"] + "' and glcd = '" + tbl.Rows[i]["glcd"].retStr() + "' and slcd = '" + tbl.Rows[i]["slcd"].retStr() + "' and agslcd = '" + tbl.Rows[i]["agslcd"].retStr() + "' and drcr = '" + tbl.Rows[i]["drcr"].retStr() + "'";
                                    var rm4 = tbl.Select(sel2);
                                    if (rm4.Count() != 0)
                                    {
                                        nar1 += (nar1 == "" ? "" : rplchar);

                                        for (int z = 0; z <= rm4.Count() - 1; z++)
                                        {
                                            nar1 += "BLNO. " + rm4[z]["blno"] + " Dt. " + rm4[z]["bldt"];
                                        }
                                    }
                                }

                                #endregion

                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].retDateStr();
                                IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].retStr();
                                IR.Rows[rNo]["trem"] = nar1;
                                IR.Rows[rNo]["chqno"] = tbl.Rows[i]["chqno"].retStr();
                                IR.Rows[rNo]["type"] = (tbl.Rows[i]["dshortnm"].retStr() == "" ? CommVar.retTrCD(tbl.Rows[i]["trcd"].retStr()) : tbl.Rows[i]["dshortnm"].retStr());

                                if (VE.Checkbox12 == true)
                                {
                                    IR.Rows[rNo]["txntype"] = masterhelpfa.retIntTds(tbl.Rows[i]["inttds"].retStr());
                                }
                                IR.Rows[rNo]["bank_dt"] = tbl.Rows[i]["bank_dt"].retDateStr();
                                if (VE.Checkbox11 == true)
                                {
                                    IR.Rows[rNo]["qnty"] = tbl.Rows[i]["qnty"].retDbl();
                                }
                                IR.Rows[rNo]["dramt"] = tbl.Rows[i]["dramt"].retDbl();
                                IR.Rows[rNo]["cramt"] = tbl.Rows[i]["cramt"].retDbl();


                                a_runbal += tbl.Rows[i]["amt"].retDbl();
                                p_runbal += tbl.Rows[i]["amt"].retDbl();
                                trunbal += tbl.Rows[i]["amt"].retDbl();
                                totalrunbal += tbl.Rows[i]["amt"].retDbl();
                                if (RunnBal1 == "Y")
                                {
                                    if (total == "G")
                                    {
                                        IR.Rows[rNo]["runbal"] = Math.Abs(p_runbal).retDbl();
                                        IR.Rows[rNo]["drcr"] = p_runbal > 0 ? "Dr." : "Cr.";
                                    }
                                    else
                                    {
                                        IR.Rows[rNo]["runbal"] = Math.Abs(totalrunbal).retDbl();
                                        IR.Rows[rNo]["drcr"] = totalrunbal > 0 ? "Dr." : "Cr.";
                                    }
                                }

                                p_dramt += tbl.Rows[i]["dramt"].retDbl();
                                p_cramt += tbl.Rows[i]["cramt"].retDbl();
                                a_dramt += tbl.Rows[i]["dramt"].retDbl();
                                a_cramt += tbl.Rows[i]["cramt"].retDbl();
                                tdramt += tbl.Rows[i]["dramt"].retDbl();
                                tcramt += tbl.Rows[i]["cramt"].retDbl();

                                totaldramt += tbl.Rows[i]["dramt"].retDbl();
                                totalcramt += tbl.Rows[i]["cramt"].retDbl();

                                #region Print Totals
                                bool flag = false;
                                if (reptype == "R" && total == "M" && i != tbl.Rows.Count - 1)
                                {
                                    currentmonth = Convert.ToDateTime(tbl.Rows[i]["docdt"].retStr()).Month;
                                    nextmonth = Convert.ToDateTime(tbl.Rows[i + 1]["docdt"].retStr()).Month;
                                }
                                if (reptype == "R" && total == "Y" && i != tbl.Rows.Count - 1)
                                {
                                    currentyr = Convert.ToDateTime(tbl.Rows[i]["to_date"].retStr()).Year;
                                    nextyr = Convert.ToDateTime(tbl.Rows[i + 1]["to_date"].retStr()).Year;
                                }
                                if (reptype == "R" && total == "D" && ((i) == tbl.Rows.Count - 1 || tbl.Rows[i]["docdt"].retStr() != tbl.Rows[i + 1]["docdt"].retStr())) flag = true;
                                if (reptype == "R" && total == "M" && ((i) == tbl.Rows.Count - 1 || currentmonth != nextmonth)) flag = true;
                                if (reptype == "R" && total == "Y" && ((i) == tbl.Rows.Count - 1 || currentyr != nextyr)) flag = true;
                                if (reptype == "R" && total == "G" && ((i) == tbl.Rows.Count - 1)) flag = false;
                                if (flag == true)
                                {
                                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                    IR.Rows[rNo]["trem"] = "Totals" + (total == "M" ? " (Month Wise)" : total == "D" ? " (Day Wise)" : total == "Y" ? " (Year Wise)" : "");
                                    IR.Rows[rNo]["dramt"] = totaldramt;
                                    IR.Rows[rNo]["cramt"] = totalcramt;
                                    if (reptype == "R") IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                                    if (RunnBal1 == "Y")
                                    {
                                        if (reptype == "R")
                                        {
                                            IR.Rows[rNo]["runbal"] = Math.Abs(totalrunbal).ToINRFormat();
                                            IR.Rows[rNo]["drcr"] = totalrunbal > 0 ? "Dr." : "Cr.";
                                        }
                                        else if (reptype == "E" || reptype == "V")
                                        {
                                            IR.Rows[rNo]["runbal"] = Math.Abs(totalrunbal).retDbl();
                                            IR.Rows[rNo]["drcr"] = totalrunbal > 0 ? "Dr." : "Cr.";
                                        }
                                        else
                                        {
                                            IR.Rows[rNo]["runbal"] = Math.Abs(totalrunbal).ToINRFormat() + " " + (totalrunbal > 0 ? "Dr." : "Cr.");
                                        }
                                        if (reptype == "R") IR.Rows[rNo]["celldesign"] = "runbal=text-align:right^;";
                                    }
                                    DateTime dt = Convert.ToDateTime(tbl.Rows[i]["docdt"].retStr());
                                    DateTime firstDayOfMonth = new DateTime(dt.Year, dt.Month, (total == "D" ? dt.Day : 1));
                                    if (total == "G") firstDayOfMonth = Convert.ToDateTime(VE.TDT);
                                    if (total == "Y")
                                    {
                                        if (Convert.ToDateTime(TD) <= Convert.ToDateTime(tbl.Rows[i]["to_date"].retStr()))
                                        {
                                            firstDayOfMonth = Convert.ToDateTime(TD);
                                        }
                                        else
                                        {
                                            firstDayOfMonth = Convert.ToDateTime(tbl.Rows[i]["to_date"].retStr());
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
                                            IR.Rows[rNo]["dramt"] = Math.Abs(totalrunbal).ToINRFormat();
                                        }
                                        else
                                        {
                                            IR.Rows[rNo]["cramt"] = Math.Abs(totalrunbal).ToINRFormat();
                                        }
                                        if (reptype == "R") IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                                    }
                                    //}
                                    totaldramt = 0; totalcramt = 0; totalrunbal = 0;
                                }
                                #endregion

                                i++;
                                if (i > maxR) break;
                            }
                            if (total == "G")
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["dammy"] = "";
                                IR.Rows[rNo]["trem"] = "Totals (" + tbl.Rows[i - 1]["slnm"] + " - " + tbl.Rows[i - 1]["glnm"] + ")";
                                IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                                IR.Rows[rNo]["dramt"] = p_dramt;
                                IR.Rows[rNo]["cramt"] = p_cramt;
                                IR.Rows[rNo]["runbal"] = Math.Abs(p_runbal).retDbl();
                                IR.Rows[rNo]["drcr"] = p_runbal > 0 ? "Dr." : "Cr.";
                            }

                            if (i > maxR) break;
                        }
                        if (total == "G")
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["dammy"] = "";
                            IR.Rows[rNo]["trem"] = "Totals (" + tbl.Rows[i - 1]["agslnm"] + ")";
                            IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                            IR.Rows[rNo]["dramt"] = a_dramt;
                            IR.Rows[rNo]["cramt"] = a_cramt;
                            IR.Rows[rNo]["runbal"] = Math.Abs(a_runbal).retDbl();
                            IR.Rows[rNo]["drcr"] = a_runbal > 0 ? "Dr." : "Cr.";
                        }
                        if (i > maxR) break;
                    }
                    if (i > maxR) break;
                }
                if (total == "G")
                {
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["dammy"] = "";
                    IR.Rows[rNo]["trem"] = "Grand Totals";
                    IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                    IR.Rows[rNo]["dramt"] = tdramt;
                    IR.Rows[rNo]["cramt"] = tcramt;
                    IR.Rows[rNo]["runbal"] = Math.Abs(trunbal).retDbl();
                    IR.Rows[rNo]["drcr"] = trunbal > 0 ? "Dr." : "Cr.";
                }
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