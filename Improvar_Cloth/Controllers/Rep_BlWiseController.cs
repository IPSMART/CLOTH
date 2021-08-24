using System;
using System.Linq;
using System.Data;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;

namespace Improvar.Controllers
{
    public class Rep_BlWiseController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        DropDownHelp DropDownHelp = new DropDownHelp();
        MasterHelpFa masterhelpfa = new MasterHelpFa();
        MasterHelp MasterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        DataTable tbl = new DataTable("os");
        DataTable txn = new DataTable();
        long ageingperiod = 0;
        string TD = "";
        string format = "";
        bool showOrder = false;
        bool bltype = false;
        string rtdebslcd = "";
        string Para2 = "";
        // GET: Rep_BlWise
        public ActionResult Rep_BlWise()
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                ReportViewinHtml VE = new ReportViewinHtml(); Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                string ModuleCode = CommVar.ModuleCode();
                string[] dt = CommVar.FinPeriod(UNQSNO).Split('-');

                VE.FDT = CommVar.FinStartDate(UNQSNO);
                VE.TDT = CommVar.CurrDate(UNQSNO);

                string Para1 = VE.MENU_PARA.Split(',')[0].retStr();
                string FScm1 = CommVar.FinSchema(UNQSNO), SScm1 = CommVar.SaleSchema(UNQSNO), Userid = CommVar.UserID(), com = CommVar.Compcd(UNQSNO), loc = CommVar.Loccd(UNQSNO);
                string query = "";
                if (VE.MENU_PARA.Split(',').Count() > 1) Para2 = VE.MENU_PARA.Split(',')[1].retStr();

                ViewBag.formname = "Bill Wise Outstanding (Bills " + (VE.MENU_PARA.Split(',')[0].retStr() == "DR" ? "Receivable" : "Payable") + ")" + (Para2 != "" ? " [Retail Party]" : "");

                query += " select distinct c.class1cd,d.class1nm from " + SScm1 + ".m_usr_acs_doccd a, " + SScm1 + ".m_groupdoccd b,";
                query += " " + SScm1 + ".m_group c, " + FScm1 + ".m_class1 d";
                query += " where a.user_id = '" + Userid + "' and a.compcd = '" + com + "' and a.loccd = '" + loc + "' and a.doccd = b.doccd and b.itgrpcd = c.itgrpcd";
                query += " and c.class1cd = d.class1cd";
                query += " union";
                query += " select a.class1cd,b.class1nm";
                query += " from " + SScm1 + ".m_group a, " + FScm1 + ".m_class1 b where a.class1cd = b.class1cd and 0 in ";
                query += " (select count(b.itgrpcd) from " + SScm1 + ".m_usr_acs_doccd a, " + SScm1 + ".m_groupdoccd b where a.user_id = '" + Userid + "' and a.compcd = '" + com + "' and a.loccd = '" + loc + "' and a.doccd = b.doccd )";

                string linkcd = "D";
                if (Para1 == "CR") linkcd = "C";

                string sql = "";

                sql += "select distinct a.glcd, a.slcd ";
                sql += "from " + FScm1 + ".t_vch_bl a, " + FScm1 + ".m_genleg b ";
                sql += "where a.glcd = b.glcd(+) and b.linkcd = '" + linkcd + "' and a.rtdebcd is not null ";
                DataTable tbl = MasterHelp.SQLquery(sql);
                if (tbl.Rows.Count > 0) rtdebslcd = tbl.Rows[0]["slcd"].retStr();
                VE.TEXTBOX8 = rtdebslcd;

                VE.DropDown_list_GLCD = DropDownHelp.DropDown_list_GLCD(linkcd, "Y");
                VE.Glnm = MasterHelp.ComboFill("glcd", VE.DropDown_list_GLCD, 0, 1);

                if (Para2 == "")
                {
                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("");
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);
                }
                else
                {
                    VE.DropDown_list_RTCD = DropDownHelp.GetRtDebCdforSelection();
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_RTCD, 0, 1);
                }

                VE.DropDown_list_AGSLCD = DropDownHelp.GetAgSlcdforSelection();
                VE.DropDown_list_SLMSLCD = DropDownHelp.GetSlmSlcdforSelection();

                VE.DropDown_list_SubLegGrp = DropDownHelp.GetSubLegGrpforSelection();
                VE.SubLeg_Grp = MasterHelp.ComboFill("slcdgrpcd", VE.DropDown_list_SubLegGrp, 0, 1);

                var T_VCH_BL_REMLst = (from i in DBF.T_VCH_BL_REM
                                       select new Database_Combo1()
                                       { FIELD_VALUE = i.CTG }).Distinct().OrderBy(s => s.FIELD_VALUE).ToList();
                VE.TEXTBOX7 = MasterHelp.ComboFill("ctg", T_VCH_BL_REMLst, 0, 0);

                if (ModuleCode == "SALESCHEM")
                {
                    DataTable clsdata = MasterHelp.SQLquery(query);
                    VE.DropDown_list_Class1 = (from DataRow dr in clsdata.Rows
                                               select new DropDown_list_Class1()
                                               {
                                                   text = dr["class1nm"].ToString(),
                                                   value = dr["class1cd"].ToString()
                                               }).OrderBy(s => s.text).ToList();
                }
                else
                {
                    VE.DropDown_list_Class1 = DropDownHelp.DropDown_list_Class1();
                }
                VE.CLASS1CD = MasterHelp.ComboFill("class1cd", VE.DropDown_list_Class1, 0, 1);

                sql = "select distinct nvl(vchtype,' ') vchtype from " + FScm1 + ".t_vch_bl ";
                var dt1 = masterhelpfa.SQLquery(sql);

                VE.DropDown_list3 = (from DataRow dr in dt1.Rows
                           select new DropDown_list3()
                           {
                               value = dr["vchtype"].ToString(),
                               text = dr["vchtype"].ToString(),

                           }).ToList();

                VE.Checkbox2 = false;
                VE.Checkbox3 = true;
                VE.Checkbox3 = false;
                VE.Checkbox4 = false;
                VE.Checkbox5 = false;
                VE.Checkbox6 = false;
                VE.TEXTBOX5 = "0";
                VE.DefaultView = true;
                VE.ExitMode = 1;
                VE.DefaultDay = 0;
                if (CommVar.Compcd(UNQSNO) == "SNFP")
                {
                    VE.Checkbox14 = true; //Skip clear bills
                }

                return View(VE);
            }
        }
        [HttpPost]
        public ActionResult Rep_BlWise(ReportViewinHtml VE, FormCollection FC, string submitbutton)
        {
            Cn.getQueryString(VE);
            string ModuleCode = CommVar.ModuleCode(); // Session["ModuleCode"].ToString();
            string scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO);
            bool ShowPymtHoldRem = false;
            if (VE.Checkbox16 == true) ShowPymtHoldRem = true;
            string sql = "", sqlc = "";
            try
            {
                if (VE.MENU_PARA.Split(',').Count() > 1) Para2 = VE.MENU_PARA.Split(',')[1].retStr();

                string selglcd = "", selclass1cd = "", linkcd = "", selagslcd = "", selstate = "", seldistrict = "", selflag = "", selPymtHoldCtg = "", selvchtype = "";
                TD = VE.TDT;
                string acsel1 = "";
                if (FC.AllKeys.Contains("glcdvalue"))
                {
                    selglcd = FC["glcdvalue"].ToString().retSqlformat();
                    acsel1 = " and a.glcd in (" + acsel1 + ") ";
                }
                string selslcd = "", unselslcd = "", selrtdebcd = "", unselrtdebcd = "";
                if (Para2 == "")
                {
                    if (FC.AllKeys.Contains("slcdvalue")) selslcd = CommFunc.retSqlformat(FC["slcdvalue"].ToString());
                    if (FC.AllKeys.Contains("slcdunselvalue")) unselslcd = CommFunc.retSqlformat(FC["slcdunselvalue"].ToString());
                }
                else
                {
                    selslcd = "'" + VE.TEXTBOX8.retStr() + "'";
                    if (FC.AllKeys.Contains("slcdvalue")) selrtdebcd = CommFunc.retSqlformat(FC["slcdvalue"].ToString());
                    if (FC.AllKeys.Contains("slcdunselvalue")) unselrtdebcd = CommFunc.retSqlformat(FC["slcdunselvalue"].ToString());
                }
                string selslcdgrpcd = "";
                if (FC.AllKeys.Contains("slcdgrpcdvalue")) selslcdgrpcd = CommFunc.retSqlformat(FC["slcdgrpcdvalue"].ToString());

                if (selglcd == "") return Content("You have to select One General Ledger...");
                if (FC.AllKeys.Contains("class1cdvalue")) selclass1cd = FC["class1cdvalue"].ToString().retSqlformat();
                if (FC.AllKeys.Contains("AGENT")) selagslcd = FC["AGENT"].ToString().retSqlformat();
                if (FC.AllKeys.Contains("ctgvalue")) selPymtHoldCtg = FC["ctgvalue"].ToString().retSqlformat();
                if (FC.AllKeys.Contains("VCHTYPE")) selvchtype = FC["VCHTYPE"].ToString().retSqlformat();

                if (VE.TEXTBOX6 != null) selflag = "'" + VE.TEXTBOX6 + "'";
                if (ModuleCode == "SALESCHEM")
                {
                    if (selclass1cd == "") return Content("Please Choose <B STYLE='COLOR:BLUE;'> CLASS 1 </B>");
                }
                sql = "";
                if (selslcdgrpcd != "")
                {
                    sql = "select distinct b.class1cd from " + scmf + ".m_subleg_grp a, " + scmf + ".m_subleg_grpclass1 b ";
                    sql += "where a.grpcd=b.grpcd(+) and a.slcdgrpcd in (" + selslcdgrpcd + ") ";
                    DataTable tbl = MasterHelp.SQLquery(sql);
                    string grpclass1 = string.Join(",", (from DataRow dr in tbl.Rows select dr["class1cd"].ToString()).Distinct());
                    selclass1cd = grpclass1.retSqlformat();
                }
                string optRepOpt = FC["OPTIONS"]; //Due Bill/NIL
                string due_caldt = FC["ddco"];//Due days calculate on  Due/doc=B/Lr 
                format = FC["format"];//format
                bool crbaladjauto = VE.Checkbox7;

                sql = "select distinct bltype from " + scmf + ".t_vch_bl where trim(bltype) is not null";
                DataTable tblbltype = MasterHelp.SQLquery(sql);
                if (tblbltype.Rows.Count > 0) bltype = true;

                string bill_dtf = "", bill_dtt = "";
                if (VE.Checkbox1 == true)
                {
                    bill_dtf = VE.TEXTBOX3; //Bill Date From
                    bill_dtt = VE.TEXTBOX4; //Bill date to
                    if (bill_dtf == null) bill_dtf = "";
                    if (bill_dtt == null)
                    {
                        return Content("Please Fill [Bill Date From] and [Bill Date To]");
                    }
                }
                else
                {
                    bill_dtf = ""; //Bill Date From
                    bill_dtt = ""; //bill date to
                }
                string bldtfrom = ""; string bldtto = "";
                ageingperiod = 0;
                bldtfrom = bill_dtf; bldtto = bill_dtt;

                string OsBill = "Y";
                if (optRepOpt == "NIL") OsBill = "N"; else if (optRepOpt == "ALL") OsBill = "A";
                if (VE.Checkbox14 == true) OsBill = "Y";
                string showagentpara = "";
                if (VE.Checkbox4 == true) showagentpara = "A"; else if (VE.Checkbox17 == true) showagentpara = "S";

                tbl = masterhelpfa.GenOSTbl(selglcd, selslcd, bldtto, TD, "", bldtfrom, selclass1cd, linkcd, OsBill, selagslcd, selstate, seldistrict, "", selflag, crbaladjauto, VE.Checkbox8, unselslcd, selslcdgrpcd, showagentpara, "", "", selrtdebcd, ShowPymtHoldRem, selPymtHoldCtg, selvchtype);

                string osmsg = masterhelpfa.CheckOSTbl(tbl);
                if (osmsg != "OK") return Content(osmsg);

                if (tbl.Rows.Count == 0) return Content("No Record Found");

                if (FC["Sorting"].retStr() == "DOC")
                {
                    tbl.DefaultView.Sort = "glcd,slnm,slcd,docdt,bldt1,docno";
                    tbl = tbl.DefaultView.ToTable();
                }
                else
                {
                    //tbl.DefaultView.Sort = "glcd,slnm,slcd,bldt1,docdt,docno";
                    tbl.DefaultView.Sort = "glcd,slnm,slcd,bldt1,blno,docdt,docno";
                    tbl = tbl.DefaultView.ToTable();
                }
                if (Para2 != "")
                {
                    tbl.DefaultView.Sort = "glcd,slnm,slcd,rtdebnm,rtdebcd,docdt,docno";
                    tbl = tbl.DefaultView.ToTable();
                }
                if (optRepOpt == "NIL" || optRepOpt == "ALL")
                {
                    sqlc = ""; sql = "";
                    string curdocautono = "null", txnupto = TD;
                    sqlc = "";
                    //sqlc += "select a.autono||a.r_slno vautoslno, a.i_autono||a.i_slno autoslno, b.docdt, nvl(e.blno,b.doccd||b.doconlyno) doccdno, b.docno, b.doccd, e.bltype, e.vchtype, a.pymtrem, ";
                    sqlc += "select a.autono, a.autono||a.r_slno vautoslno, a.i_autono||a.i_slno autoslno, b.docdt, nvl(e.blno,b.docno) doccdno, b.docno, b.doccd, e.bltype, e.vchtype, a.pymtrem, ";
                    sqlc += "sum(decode(a.r_autono||a.r_slno," + curdocautono + ",nvl(a.adj_amt,0),0) * decode(c.drcr,d.linkcd,1,-1)) cur_adj, ";
                    sqlc += "sum(decode(a.r_autono||a.r_slno," + curdocautono + ",0,nvl(a.adj_amt,0)) * decode(c.drcr,d.linkcd,1,-1)) adj_amt ";
                    sqlc += "from " + scmf + ".t_vch_bl_adj a, " + scmf + ".t_cntrl_hdr b, " + scmf + ".t_vch_bl c, " + scmf + ".m_genleg d, " + scmf + ".t_vch_bl e ";
                    sqlc += "where a.i_autono||a.i_slno=c.autono||c.slno and c.glcd=d.glcd and a.r_autono||a.r_slno=e.autono||e.slno and ";
                    if (txnupto != "") sqlc += "b.docdt <= to_date('" + txnupto + "','dd/mm/yyyy') and ";
                    if (selglcd != "") sqlc += "c.glcd in (" + selglcd + ") and ";
                    if (selslcd != "") sqlc += "c.slcd in (" + selslcd + ") and ";
                    sqlc += "a.autono=b.autono and nvl(b.cancel,'N')='N' and b.compcd='" + COM + "' ";
                    sqlc += "group by a.autono, a.autono||a.r_slno, a.i_autono||a.i_slno, b.docdt, nvl(e.blno,b.docno), b.docno, b.doccd, e.bltype, e.vchtype, a.pymtrem ";

                    sql = "";

                    sql += "select a.autono, d.docnm, a.vautoslno, a.autoslno, a.docdt, a.doccdno, a.docno, a.doccd, a.bltype, a.vchtype, a.pymtrem, a.cur_adj, a.adj_amt from  ( ";
                    sql += sqlc;
                    sql += "union all ";
                    sql += sqlc.Replace("i_", "r_").Replace(",1,-1", ",-1,1");

                    sql += ") a, " + scmf + ".t_vch_bl b, " + scmf + ".t_cntrl_hdr c, " + scmf + ".m_doctype d where a.autono=c.autono and c.doccd=d.doccd and a.autoslno=b.autono||b.slno ";
                    if (selslcd != "") sql += "and b.slcd in (" + selslcd + ") ";
                    sql += "order by autoslno, docdt, docno";
                    txn = MasterHelp.SQLquery(sql);

                    return RepShowNill(VE, FC, submitbutton);
                }
                else if (optRepOpt == "FORM2")
                {
                    return Rep_Normal(VE, FC, "FORM2", submitbutton);
                }
                else
                {
                    return Rep_Normal(VE, FC, "FORM1", submitbutton);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public ActionResult Rep_Normal(ReportViewinHtml VE, FormCollection FC, string reptypesel, string submitbutton)
        {
            try
            {
                bool ordrefprint = false, blamtprint = false, itamtprint = false, showAgent = false, showcrbalsep = false, ShowPymtHoldRem = false;
                string crbalhead = "Cr.";
                if (VE.Checkbox2 == true) ordrefprint = true;
                if (VE.Checkbox5 == true) showcrbalsep = true;
                if (VE.Checkbox4 == true) showAgent = true;
                if (VE.Checkbox16 == true) ShowPymtHoldRem = true;
                string due_caldt = FC["ddco"];//Due days calculate on   
                string ShowDuedaysfrom = FC["ddshowas"]; // D=due date B=Doc Dt
                string days_aging = VE.TEXTBOX5; // Days aging value
                string days1 = "0", days2 = "0", days3 = "0", days4 = "0", days5 = "0";
                if (days_aging.retStr() != "0" && days_aging != null) showcrbalsep = true;
                if (showcrbalsep == true && VE.MENU_PARA.Split(',')[0].retStr() == "CR") crbalhead = "Dr.";
                bool recdamtcol = false;
                if (VE.Checkbox10 == true) recdamtcol = true;
                string selslcdgrpcd = "";

                if (FC.AllKeys.Contains("slcdgrpcdvalue")) selslcdgrpcd = CommFunc.retSqlformat(FC["slcdgrpcdvalue"].ToString());
                if (selslcdgrpcd.retStr() != "") showAgent = true;
                if (reptypesel == "FORM2")
                {
                    blamtprint = true;
                    if (VE.MENU_PARA.Split(',')[0].retStr() == "DR") itamtprint = true;
                }
                itamtprint = VE.Checkbox6;
                if (days_aging == null)
                {
                }
                else if (days_aging == "1")
                {
                    days1 = FC["days1"];
                }
                else if (days_aging == "2")
                {
                    days1 = FC["days1"];
                    days2 = FC["days2"];
                }
                else if (days_aging == "3")
                {
                    days1 = FC["days1"];
                    days2 = FC["days2"];
                    days3 = FC["days3"];
                }
                else if (days_aging == "4")
                {
                    days1 = FC["days1"];
                    days2 = FC["days2"];
                    days3 = FC["days3"];
                    days4 = FC["days4"];
                }
                else if (days_aging == "5")
                {
                    days1 = FC["days1"];
                    days2 = FC["days2"];
                    days3 = FC["days3"];
                    days4 = FC["days4"];
                    days5 = FC["days5"];
                }

                if (showAgent == true)
                {
                    if (FC["Sorting"].retStr() == "DOC")
                    {
                        tbl.DefaultView.Sort = "agslnm,agslcd,glcd,slnm,slcd,docdt,bldt1,docno";
                        tbl = tbl.DefaultView.ToTable();
                    }
                    else
                    {
                        tbl.DefaultView.Sort = "agslnm,agslcd,glcd,slnm,slcd,bldt1,docdt,docno";
                        tbl = tbl.DefaultView.ToTable();
                    }
                }
                if (Para2 != "")
                {
                    tbl.DefaultView.Sort = "agslnm,agslcd,glcd,slnm,slcd,rtdebnm,rtdebcd,docdt,docno";
                    tbl = tbl.DefaultView.ToTable();
                }
                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                DataTable IR = new DataTable("os");

                long duedays1, duedays2, duedays3, duedays4, duedays5 = 0;
                duedays1 = Convert.ToInt64(days1); duedays2 = Convert.ToInt64(days2); duedays3 = Convert.ToInt64(days3); duedays4 = Convert.ToInt64(days4); duedays5 = Convert.ToInt64(days5);
                ageingperiod = Convert.ToInt64(days_aging);

                long due1fDys, due1tDys, due2fDys, due2tDys, due3fDys, due3tDys, due4fDys, due4tDys, due5fDys, due5tDys, due6fDys, due6tDys = 0;
                double due1Amt, due2Amt, due3Amt, due4Amt = 0, due5Amt, due6Amt = 0;
                double pdue1Amt, pdue2Amt, pdue3Amt, pdue4Amt, pdue5Amt, pdue6Amt = 0;
                double tdue1Amt, tdue2Amt, tdue3Amt, tdue4Amt, tdue5Amt, tdue6Amt = 0;
                double adue1Amt, adue2Amt, adue3Amt, adue4Amt, adue5Amt, adue6Amt = 0;
                due1fDys = 0; due1tDys = 0; due2fDys = 0; due2tDys = 0; due3fDys = 0; due3tDys = 0; due4fDys = 0; due4tDys = 0; due5fDys = 0; due5tDys = 0; due6fDys = 0; due6tDys = 0;
                tdue1Amt = 0; tdue2Amt = 0; tdue3Amt = 0; tdue4Amt = 0; tdue5Amt = 0; tdue6Amt = 0;
                pdue1Amt = 0; pdue2Amt = 0; pdue3Amt = 0; pdue4Amt = 0; pdue5Amt = 0; pdue6Amt = 0;
                adue1Amt = 0; adue2Amt = 0; adue3Amt = 0; adue4Amt = 0; adue5Amt = 0; adue6Amt = 0;

                if (ageingperiod != 0)
                {
                    ageingperiod = ageingperiod + 1;

                    if (ageingperiod >= 1) due1fDys = 0; due1tDys = duedays1;
                    if (ageingperiod >= 2) due2fDys = due1tDys + 1; due2tDys = duedays2;
                    if (ageingperiod >= 3) due3fDys = due2tDys + 1; due3tDys = duedays3;
                    if (ageingperiod >= 4) due4fDys = due3tDys + 1; due4tDys = duedays4;
                    if (ageingperiod >= 5) due5fDys = due4tDys + 1; due5tDys = duedays5;
                    if (ageingperiod >= 6) due6fDys = due5tDys + 1; due6tDys = 0;

                    //define last ageing column
                    if (ageingperiod == 2) due2tDys = 99999;
                    if (ageingperiod == 3) due3tDys = 99999;
                    if (ageingperiod == 4) due4tDys = 99999;
                    if (ageingperiod == 5) due5tDys = 99999;
                    if (ageingperiod == 6) due6tDys = 99999;
                }

                string dtlsumm = format;
                if (format == "B") dtlsumm = "D"; else dtlsumm = "S";
                string ordno = "";
                HC.RepStart(IR, 3);
                if (dtlsumm == "D")
                {
                    if (VE.Checkbox15 == true) HC.GetPrintHeader(IR, "agshortnm", "string", "c,10", "Agent Name");
                    if (VE.MENU_PARA.Split(',')[0].retStr() == "CR")
                    {
                        HC.GetPrintHeader(IR, "docdt", "string", "c,10", "Doc Date");
                        HC.GetPrintHeader(IR, "docno", "string", "c,15", "Doc No.");
                    }
                    HC.GetPrintHeader(IR, "bldt", "string", "c,10", "Bill Date");
                    HC.GetPrintHeader(IR, "blno", "string", "c,15", "Bill No.");
                    if (bltype == true) HC.GetPrintHeader(IR, "bltype", "string", "c,5", "Bill Type");
                    if (reptypesel == "FORM1") HC.GetPrintHeader(IR, "cdays", "double", "n,4,0", "Cr.Days");
                    string duedaysdsp = "";
                    if (ShowDuedaysfrom == "D") duedaysdsp = "Over ";
                    HC.GetPrintHeader(IR, "days", "double", "n,4,0", duedaysdsp + "Due Days");
                    if (itamtprint == true) HC.GetPrintHeader(IR, "itamt", "double", "n,17,2:####,##,##,##0.00", "Item Value");
                    if (blamtprint == true) HC.GetPrintHeader(IR, "blamt", "double", "n,17,2:####,##,##,##0.00", "Bill Amount");
                    if (blamtprint == true && recdamtcol == true) HC.GetPrintHeader(IR, "recdamt", "double", "n,17,2:####,##,##,##0.00", "Recd Amount");
                }
                else
                {
                    HC.GetPrintHeader(IR, "slno", "string", "c,4", "Sl#");
                    HC.GetPrintHeader(IR, "blno", "string", "c,8", "Party Cd");
                    HC.GetPrintHeader(IR, "slnm", "string", "c,35", "Party Name");
                    HC.GetPrintHeader(IR, "slcity", "string", "c,17", "City");
                    HC.GetPrintHeader(IR, "days", "double", "n,4,0", "Av.Days");
                }
                if (reptypesel == "FORM1") HC.GetPrintHeader(IR, "amt", "double", "n,17,2:####,##,##,##0.00", "Balance;Amount");
                if (reptypesel == "FORM1") HC.GetPrintHeader(IR, "dueamt", "double", "n,17,2:####,##,##,##0.00", "Over due;Amount");
                HC.GetPrintHeader(IR, "balamt", "double", "n,17,2:####,##,##,##0.00", "Total O/S Amt");
                if (reptypesel == "FORM2" && showcrbalsep == true)
                {
                    HC.GetPrintHeader(IR, "cramt", "double", "n,17,2:####,##,##,##0.00", ";" + crbalhead + " Amount");
                    if (dtlsumm == "S") HC.GetPrintHeader(IR, "netos", "double", "n,17,2:####,##,##,##0.00", "; Net O/s Amount");
                }

                if (dtlsumm == "D") HC.GetPrintHeader(IR, "class1cd", "string", "c,10", "Class1;Code");
                if (dtlsumm == "D" && ordrefprint == true)
                {
                    HC.GetPrintHeader(IR, "ordno", "string", "c,20", "Order;No.");
                    HC.GetPrintHeader(IR, "orddt", "string", "c,10", "Order;Date");
                }
                if (ageingperiod >= 1) HC.GetPrintHeader(IR, "due1amt", "double", "n,14,2", "<= " + due1tDys.ToString());
                if (ageingperiod >= 2) HC.GetPrintHeader(IR, "due2amt", "double", "n,14,2", due2fDys.ToString() + " to " + due2tDys.ToString());
                if (ageingperiod >= 3) HC.GetPrintHeader(IR, "due3amt", "double", "n,14,2", due3fDys.ToString() + " to " + due3tDys.ToString());
                if (ageingperiod >= 4) HC.GetPrintHeader(IR, "due4amt", "double", "n,14,2", due4fDys.ToString() + " to " + due4tDys.ToString());
                if (ageingperiod >= 5) HC.GetPrintHeader(IR, "due5amt", "double", "n,14,2", due5fDys.ToString() + " to " + due5tDys.ToString());
                if (ageingperiod >= 6) HC.GetPrintHeader(IR, "due6amt", "double", "n,14,2", due6fDys.ToString() + " to " + due6tDys.ToString());
                if (showOrder == true && dtlsumm == "D")
                {
                    HC.GetPrintHeader(IR, "ordno", "string", "c,30", "Order No.");
                    if (dtlsumm == "D") HC.GetPrintHeader(IR, "orddt", "string", "c,10", "Order;Date");
                }
                if (VE.Checkbox9 == true && dtlsumm == "D") HC.GetPrintHeader(IR, "lrno", "string", "c,15", "LR;No.");
                if ((VE.Checkbox3 == true || VE.Checkbox9 == true) && dtlsumm == "D") HC.GetPrintHeader(IR, "lrdt", "string", "c,10", "LR;Date");
                if (VE.Checkbox9 == true && dtlsumm == "D") HC.GetPrintHeader(IR, "transnm", "string", "c,35", "Transporter");
                if (VE.Checkbox12 == true && dtlsumm == "D") HC.GetPrintHeader(IR, "blrem", "string", "c,35", "Remarks");

                double amt2 = 0, balamt2 = 0, cramt2 = 0;
                double namt2 = 0, namt3 = 0;
                double amt3 = 0, balamt3 = 0, cramt3 = 0;
                double days = 0, cdays = 0;
                double mult = 1;
                double aamt2 = 0, abalamt2 = 0, anamt2 = 0, acramt2 = 0;

                string glcd = tbl.Rows[0]["glcd"].ToString(); string glnm = tbl.Rows[0]["glnm"].ToString();

                string glcd1 = "", slcd1 = "", slnm1 = "", slcity1 = "", slphno1 = "", agslcd1 = "", agslnm1 = "", agslcity1 = "";
                Int32 slno1 = 0, aslno1 = 0;
                Int32 maxR = 0, i = 0, rNo = 0;

                i = 0; maxR = tbl.Rows.Count - 1;
                string agheaddsp = "Agent - ";
                if (selslcdgrpcd.retStr() != "") agheaddsp = "Group - ";
                while (i <= maxR)
                {
                    agslcd1 = tbl.Rows[i]["agslcd"].ToString();
                    agslnm1 = tbl.Rows[i]["agslnm"].ToString();
                    agslcity1 = tbl.Rows[i]["agslcity"].ToString();
                    aamt2 = 0; abalamt2 = 0; anamt2 = 0; acramt2 = 0;
                    adue1Amt = 0; adue2Amt = 0; adue3Amt = 0; adue4Amt = 0; adue5Amt = 0; adue6Amt = 0;
                    if (showAgent == true)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        string agdsp = "";
                        agdsp = "<span style='font-weight:100;font-size:11px;'>" + agheaddsp + agslcd1 + "  " + " </span>" + tbl.Rows[i]["agslnm"];
                        if (agslcity1 != "") agdsp += "<span style='font-weight:100;font-size:13px;'>" + " [" + agslcity1 + "]  " + " </span>";
                        if (tbl.Rows[i]["agphno"].ToString() != "") agdsp += " Ph. " + " </span>" + tbl.Rows[i]["agphno"];

                        IR.Rows[rNo]["Dammy"] = agdsp;
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                    }
                    else
                    {
                        agslcd1 = (Para2 == "" ? tbl.Rows[i]["slcd"].ToString() : tbl.Rows[i]["rtdebcd"].ToString());
                    }
                    while (showAgent == true ? tbl.Rows[i]["agslcd"].ToString() == agslcd1 : (Para2 == "" ? tbl.Rows[i]["slcd"].ToString() : tbl.Rows[i]["rtdebcd"].ToString()) == agslcd1)
                    {
                        glcd1 = tbl.Rows[i]["glcd"].ToString();
                        slcd1 = (Para2 == "" ? tbl.Rows[i]["slcd"].ToString() : tbl.Rows[i]["rtdebcd"].ToString());
                        slnm1 = (Para2 == "" ? tbl.Rows[i]["slnm"].ToString() : tbl.Rows[i]["rtdebnm"].ToString());
                        slphno1 = (Para2 == "" ? tbl.Rows[i]["phno"].retStr() : tbl.Rows[i]["rtdebmobile"].retStr());
                        slcity1 = (Para2 == "" ? tbl.Rows[i]["slcity"].ToString() : tbl.Rows[i]["retdebarea"].ToString());

                        if (dtlsumm == "D")
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            string sldsp = "";
                            sldsp = "<span style='font-weight:100;font-size:9px;'>" + " " + slcd1 + "  " + " </span>" + slnm1;
                            sldsp += "<span style='font-weight:100;font-size:11px;'>" + " [" + slcity1 + "]  " + " </span>";
                            if (slphno1 != "") sldsp += IR.Rows[rNo]["Dammy"] + " Ph. " + " </span>" + slphno1;
                            IR.Rows[rNo]["Dammy"] = sldsp;
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                        }

                        amt2 = 0; balamt2 = 0; namt2 = 0; cramt2 = 0;
                        pdue1Amt = 0; pdue2Amt = 0; pdue3Amt = 0; pdue4Amt = 0; pdue5Amt = 0; pdue6Amt = 0; ordno = "";
                        if (showOrder == true && dtlsumm != "D")
                        {
                            var ORD_DATA = (from DataRow DR in tbl.Rows
                                            where (DR["slcd"].ToString() == slcd1 && DR["glcd"].ToString() == glcd1 && DR["ordno"].ToString() != "")
                                            group DR by new { ORDNO = DR["ordno"].ToString() } into x
                                            select new { ORDNO = x.Key.ORDNO }
                                            ).Distinct().ToList();
                            if (ORD_DATA != null)
                            {
                                ordno = string.Join(",", ORD_DATA.Select(p => p.ORDNO.ToString()));
                            }
                        }

                        while ((showAgent == true ? tbl.Rows[i]["agslcd"].ToString() == agslcd1 : (Para2 == "" ? tbl.Rows[i]["slcd"].ToString() : tbl.Rows[i]["rtdebcd"].ToString()) == agslcd1) && tbl.Rows[i]["glcd"].ToString() == glcd1 && (Para2 == "" ? tbl.Rows[i]["slcd"].ToString() : tbl.Rows[i]["rtdebcd"].ToString()) == slcd1)
                        {
                            if (dtlsumm == "D") IR.Rows.Add(""); rNo = IR.Rows.Count - 1;

                            TimeSpan TSdys, TCdys;
                            //calculate credit days
                            cdays = tbl.Rows[i]["crdays"].retDbl();
                            if (cdays == 0)
                            {
                                if (tbl.Rows[i]["duedt"].ToString() == "") cdays = 0;
                                else
                                {
                                    if (tbl.Rows[i]["bldt"].ToString() != null && tbl.Rows[i]["bldt"].ToString() != "") TCdys = Convert.ToDateTime(tbl.Rows[i]["duedt"]) - Convert.ToDateTime(tbl.Rows[i]["bldt"].ToString().Substring(0, 10));
                                    else TCdys = Convert.ToDateTime(tbl.Rows[i]["duedt"]) - Convert.ToDateTime(tbl.Rows[i]["docdt"].ToString().Substring(0, 10));
                                    cdays = TCdys.Days;
                                }
                            }
                            //
                            double checkdays = cdays;
                            days = 0;
                            string checkdt = "";
                            if (due_caldt == "D" || due_caldt == "L")
                            {
                                if (due_caldt == "D") checkdt = tbl.Rows[i]["duedt"].retStr(); else checkdt = tbl.Rows[i]["lrdt"].retStr();
                                if (checkdt == "") checkdt = tbl.Rows[i]["docdt"].retStr();
                                TSdys = Convert.ToDateTime(TD) - Convert.ToDateTime(checkdt);
                                days = cdays + TSdys.Days;
                            }
                            else
                            {
                                TSdys = Convert.ToDateTime(TD) - Convert.ToDateTime(tbl.Rows[i]["docdt"]);
                                days = TSdys.Days;
                            }
                            if (ShowDuedaysfrom == "D") { days = days - cdays; checkdays = 0; }
                            if (days < 0) days = 0;

                            due1Amt = 0; due2Amt = 0; due3Amt = 0; due4Amt = 0; due5Amt = 0; due6Amt = 0;
                            if (ageingperiod > 0)
                            {
                                if (showcrbalsep == true && Convert.ToDouble(tbl.Rows[i]["bal_amt"]) < 0)
                                {

                                }
                                else
                                {
                                    if (days <= due1tDys && due1tDys != 0) due1Amt = due1Amt + Convert.ToDouble(tbl.Rows[i]["bal_amt"]);
                                    else if (days <= due2tDys && due2tDys != 0) due2Amt = due2Amt + Convert.ToDouble(tbl.Rows[i]["bal_amt"]);
                                    else if (days <= due3tDys && due3tDys != 0) due3Amt = due3Amt + Convert.ToDouble(tbl.Rows[i]["bal_amt"]);
                                    else if (days <= due4tDys && due4tDys != 0) due4Amt = due4Amt + Convert.ToDouble(tbl.Rows[i]["bal_amt"]);
                                    else if (days <= due5tDys && due5tDys != 0) due5Amt = due5Amt + Convert.ToDouble(tbl.Rows[i]["bal_amt"]);
                                    else due6Amt = due6Amt + Convert.ToDouble(tbl.Rows[i]["bal_amt"]);
                                }
                            }

                            double balamt1 = 0, mamt = 0, oamt = 0, cramt = 0;
                            balamt1 = 0; mamt = 0; oamt = 0;
                            if (tbl.Rows[i]["drcr"].ToString() == "D") mult = 1; else mult = -1;

                            if (showcrbalsep == true && Convert.ToDouble(tbl.Rows[i]["bal_amt"]) < 0) cramt = Math.Abs(Convert.ToDouble(tbl.Rows[i]["bal_amt"])); else balamt1 = Convert.ToDouble(tbl.Rows[i]["bal_amt"]);

                            if (dtlsumm == "D")
                            {
                                if (VE.MENU_PARA.Split(',')[0].retStr() == "CR")
                                {
                                    IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].ToString().Substring(0, 10);
                                    //IR.Rows[rNo]["docno"] = tbl.Rows[i]["doccd"].ToString() + tbl.Rows[i]["docno"].ToString();
                                    IR.Rows[rNo]["docno"] = tbl.Rows[i]["tchdocno"].ToString();
                                }
                                if (VE.Checkbox15 == true) IR.Rows[rNo]["agshortnm"] = tbl.Rows[i]["agshortnm"].retStr() + (tbl.Rows[i]["sagshortnm"].retStr() == "" ? "" : " - " + tbl.Rows[i]["sagshortnm"].retStr());
                                if (bltype == true) IR.Rows[rNo]["bltype"] = tbl.Rows[i]["bltype"].retStr();
                                if (tbl.Rows[i]["blno"].ToString() == "") IR.Rows[rNo]["blno"] = tbl.Rows[i]["doccd"].ToString() + tbl.Rows[i]["docno"].ToString();
                                else IR.Rows[rNo]["blno"] = tbl.Rows[i]["blno"].ToString();
                                if (tbl.Rows[i]["bldt"].ToString() == "") IR.Rows[rNo]["bldt"] = tbl.Rows[i]["docdt"].ToString().Substring(0, 10);
                                else IR.Rows[rNo]["bldt"] = tbl.Rows[i]["bldt"].ToString();
                                if (reptypesel == "FORM1") IR.Rows[rNo]["cdays"] = cdays;

                                if (reptypesel == "FORM1")
                                {
                                    if (days > checkdays) IR.Rows[rNo]["dueamt"] = Convert.ToDouble(tbl.Rows[i]["bal_amt"]);
                                    else IR.Rows[rNo]["amt"] = Convert.ToDouble(tbl.Rows[i]["bal_amt"]);
                                }
                                double prnblamt = tbl.Rows[i]["blamt"].retDbl();
                                if (tbl.Rows[i]["drcr"].retStr() != tbl.Rows[i]["linkcd"].retStr()) prnblamt = prnblamt * -1;
                                if (blamtprint == true && recdamtcol == true) IR.Rows[rNo]["recdamt"] = prnblamt - tbl.Rows[i]["bal_amt"].retDbl();
                                if (itamtprint == true) IR.Rows[rNo]["itamt"] = Convert.ToDouble(tbl.Rows[i]["itamt"] == DBNull.Value ? 0 : tbl.Rows[i]["itamt"]);
                                if (blamtprint == true) IR.Rows[rNo]["blamt"] = prnblamt; // Convert.ToDouble(tbl.Rows[i]["blamt"] == DBNull.Value ? 0 : tbl.Rows[i]["blamt"]);
                                if (ordrefprint == true)
                                {
                                    IR.Rows[rNo]["ordno"] = tbl.Rows[i]["ordno"];
                                    IR.Rows[rNo]["orddt"] = tbl.Rows[i]["orddt"];
                                }
                                if (VE.Checkbox9 == true && dtlsumm == "D") IR.Rows[rNo]["lrno"] = tbl.Rows[i]["lrno"];
                                if ((VE.Checkbox3 == true || VE.Checkbox9 == true) && dtlsumm == "D") IR.Rows[rNo]["lrdt"] = tbl.Rows[i]["lrdt"];
                                if (VE.Checkbox9 == true && dtlsumm == "D") IR.Rows[rNo]["transnm"] = tbl.Rows[i]["transnm"];
                                if (VE.Checkbox12 == true && dtlsumm == "D") IR.Rows[rNo]["blrem"] = tbl.Rows[i]["billrem"];

                                if (balamt1 != 0) IR.Rows[rNo]["balamt"] = Convert.ToDouble(tbl.Rows[i]["bal_amt"]);
                                if (reptypesel == "FORM2" && showcrbalsep == true && cramt != 0) IR.Rows[rNo]["cramt"] = cramt;
                                IR.Rows[rNo]["days"] = days;
                                IR.Rows[rNo]["class1cd"] = tbl.Rows[i]["class1cd"];
                                if (due1tDys != 0) IR.Rows[rNo]["due1amt"] = due1Amt;
                                if (due2tDys != 0) IR.Rows[rNo]["due2amt"] = due2Amt;
                                if (due3tDys != 0) IR.Rows[rNo]["due3amt"] = due3Amt;
                                if (due4tDys != 0) IR.Rows[rNo]["due4amt"] = due4Amt;
                                if (due5tDys != 0) IR.Rows[rNo]["due5amt"] = due5Amt;
                                if (due6tDys != 0) IR.Rows[rNo]["due6amt"] = due6Amt;
                                if (showOrder == true)
                                {
                                    IR.Rows[rNo]["ordno"] = tbl.Rows[i]["ordno"];
                                    IR.Rows[rNo]["orddt"] = tbl.Rows[i]["orddt"].ToString().retDateStr();
                                }
                            }
                            if (reptypesel == "FORM1")
                            {
                                if (days > checkdays) mamt = Convert.ToDouble(tbl.Rows[i]["bal_amt"]);
                                else oamt = Convert.ToDouble(tbl.Rows[i]["bal_amt"]);
                            }
                            balamt2 = balamt2 + balamt1;
                            amt2 = amt2 + mamt;
                            namt2 = namt2 + oamt;
                            cramt2 = cramt2 + cramt;
                            pdue1Amt = pdue1Amt + due1Amt;
                            pdue2Amt = pdue2Amt + due2Amt;
                            pdue3Amt = pdue3Amt + due3Amt;
                            pdue4Amt = pdue4Amt + due4Amt;
                            pdue5Amt = pdue5Amt + due5Amt;
                            pdue6Amt = pdue6Amt + due6Amt;

                            i++;
                            if (i > maxR) break;
                        }

                        abalamt2 = abalamt2 + balamt2;
                        aamt2 = aamt2 + amt2;
                        anamt2 = anamt2 + namt2;
                        acramt2 = acramt2 + cramt2;
                        adue1Amt = adue1Amt + pdue1Amt;
                        adue2Amt = adue2Amt + pdue2Amt;
                        adue3Amt = adue3Amt + pdue3Amt;
                        adue4Amt = adue4Amt + pdue4Amt;
                        adue5Amt = adue5Amt + pdue5Amt;
                        adue6Amt = adue6Amt + pdue6Amt;

                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        if (dtlsumm == "D")
                        {
                            IR.Rows[rNo]["blno"] = "Total";
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 1px solid;;border-top: 1px solid;";
                        }
                        else
                        {
                            slno1++;
                            IR.Rows[rNo]["slno"] = slno1.ToString();
                            IR.Rows[rNo]["blno"] = slcd1;
                            IR.Rows[rNo]["slnm"] = slnm1;
                            IR.Rows[rNo]["slcity"] = slcity1;
                        }
                        if (reptypesel == "FORM1")
                        {
                            IR.Rows[rNo]["dueamt"] = amt2;
                            IR.Rows[rNo]["amt"] = namt2;
                        }
                        if (reptypesel == "FORM2" && showcrbalsep == true)
                        {
                            IR.Rows[rNo]["cramt"] = cramt2;
                            if (dtlsumm == "D") IR.Rows[rNo]["class1cd"] = (balamt2 - cramt2).ToINRFormat();
                            if (dtlsumm == "S") IR.Rows[rNo]["netos"] = balamt2 - cramt2;
                        }
                        IR.Rows[rNo]["balamt"] = balamt2;
                        if (due1tDys != 0) IR.Rows[rNo]["due1amt"] = pdue1Amt;
                        if (due2tDys != 0) IR.Rows[rNo]["due2amt"] = pdue2Amt;
                        if (due3tDys != 0) IR.Rows[rNo]["due3amt"] = pdue3Amt;
                        if (due4tDys != 0) IR.Rows[rNo]["due4amt"] = pdue4Amt;
                        if (due5tDys != 0) IR.Rows[rNo]["due5amt"] = pdue5Amt;
                        if (due6tDys != 0) IR.Rows[rNo]["due6amt"] = pdue6Amt;
                        if (showOrder == true && dtlsumm != "D") IR.Rows[rNo]["ordno"] = ordno;
                        if (dtlsumm == "D")
                        {
                            // Create Blank line
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["dammy"] = " ";
                            IR.Rows[rNo]["flag"] = " height:12px; ";
                        }
                        if (i > maxR) break;
                    }
                    //Agent Totals
                    if (showAgent == true)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["blno"] = agheaddsp + "Total";
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 2px solid;;border-top: 2px solid;";
                        if (dtlsumm == "S")
                        {
                            aslno1++;
                            IR.Rows[rNo]["slno"] = aslno1.ToString();
                            IR.Rows[rNo]["blno"] = agslcd1;
                            IR.Rows[rNo]["slnm"] = "Total of " + agslnm1;
                            IR.Rows[rNo]["slcity"] = agslcity1;
                        }
                        if (reptypesel == "FORM1")
                        {
                            IR.Rows[rNo]["dueamt"] = aamt2;
                            IR.Rows[rNo]["amt"] = anamt2;
                        }
                        if (reptypesel == "FORM2" && showcrbalsep == true)
                        {
                            IR.Rows[rNo]["cramt"] = acramt2;
                            if (dtlsumm == "D") IR.Rows[rNo]["class1cd"] = (abalamt2 - acramt2).ToINRFormat();
                            if (dtlsumm == "S") IR.Rows[rNo]["netos"] = abalamt2 - acramt2;
                        }
                        IR.Rows[rNo]["balamt"] = abalamt2;
                        if (due1tDys != 0) IR.Rows[rNo]["due1amt"] = adue1Amt;
                        if (due2tDys != 0) IR.Rows[rNo]["due2amt"] = adue2Amt;
                        if (due3tDys != 0) IR.Rows[rNo]["due3amt"] = adue3Amt;
                        if (due4tDys != 0) IR.Rows[rNo]["due4amt"] = adue4Amt;
                        if (due5tDys != 0) IR.Rows[rNo]["due5amt"] = adue5Amt;
                        if (due6tDys != 0) IR.Rows[rNo]["due6amt"] = adue6Amt;
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["dammy"] = " ";
                        IR.Rows[rNo]["flag"] = " height:14px; ";
                    }
                    balamt3 = balamt3 + abalamt2;
                    amt3 = amt3 + aamt2;
                    namt3 = namt3 + anamt2;
                    cramt3 = cramt3 + acramt2;
                    tdue1Amt = tdue1Amt + adue1Amt;
                    tdue2Amt = tdue2Amt + adue2Amt;
                    tdue3Amt = tdue3Amt + adue3Amt;
                    tdue4Amt = tdue4Amt + adue4Amt;
                    tdue5Amt = tdue5Amt + adue5Amt;
                    tdue6Amt = tdue6Amt + adue6Amt;
                    if (i > maxR) break;
                }
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                if (dtlsumm == "D") IR.Rows[rNo]["blno"] = "Grand Totals"; else IR.Rows[rNo]["slnm"] = "Grand Totals";
                if (reptypesel == "FORM1")
                {
                    IR.Rows[rNo]["amt"] = namt3;
                    IR.Rows[rNo]["dueamt"] = amt3;
                }
                if (reptypesel == "FORM2" && showcrbalsep == true)
                {
                    IR.Rows[rNo]["cramt"] = cramt3;
                    if (dtlsumm == "D") IR.Rows[rNo]["class1cd"] = (balamt3 - cramt3).ToINRFormat();
                    if (dtlsumm == "S") IR.Rows[rNo]["netos"] = balamt3 - cramt3;
                }
                IR.Rows[rNo]["balamt"] = balamt3;
                if (due1tDys != 0) IR.Rows[rNo]["due1amt"] = tdue1Amt;
                if (due2tDys != 0) IR.Rows[rNo]["due2amt"] = tdue2Amt;
                if (due3tDys != 0) IR.Rows[rNo]["due3amt"] = tdue3Amt;
                if (due4tDys != 0) IR.Rows[rNo]["due4amt"] = tdue4Amt;
                if (due5tDys != 0) IR.Rows[rNo]["due5amt"] = tdue5Amt;
                if (due6tDys != 0) IR.Rows[rNo]["due6amt"] = tdue6Amt;
                //IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 1px solid;";

                string pghdr1 = "";
                string repname = "Bill_Outstanding" + System.DateTime.Now;
                pghdr1 = "Bill Wise Outstanding of " + glnm + " (" + glcd + ") " + (Para2 == "" ? "" : "[Retail Party]") + "as on " + TD;
                if (submitbutton == "Download Excel")
                {
                    IR.Columns.Remove("dammy");
                    IR.Columns.Remove("flag");
                    IR.Columns.Remove("celldesign");
                    IR = IR.Rows.Cast<DataRow>().Where(row => !row.ItemArray.All(field => field is DBNull || string.IsNullOrWhiteSpace(field as string))).CopyToDataTable();

                    DataTable[] exdt = new DataTable[1];
                    exdt[0] = IR;
                    string[] sheetname = new string[1];
                    sheetname[0] = "Sheet1";
                    MasterHelp.ExcelfromDataTables(exdt, sheetname, "Bill_Outstanding".retRepname(), false, pghdr1);
                    return Content("Downloaded");
                }
                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);

                TempData[repname] = PV;
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public ActionResult RepShowNill(ReportViewinHtml VE, FormCollection FC, string submitbutton)
        {
            try
            {
                string pghdr1 = "";
                string repname = "Bill_Outstanding" + System.DateTime.Now;
                string ShowDuedaysfrom = FC["ddshowas"]; // D=due date B=Doc Dt
                bool itamtprint = VE.Checkbox6;
                bool PymtDaysprint = VE.Checkbox13; //added from sn fabric not want
                string due_caldt = FC["ddco"];//Due days calculate on   
                bool showagent = (VE.Checkbox4 == true || VE.Checkbox17 == true ? true : false);
                string agdsp = "Agent";
                if (VE.Checkbox17 == true) agdsp = "Sub Agent";
                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                DataTable IR = new DataTable("os");

                if (showagent == true)
                {
                    if (FC["Sorting"].retStr() == "DOC")
                    {
                        tbl.DefaultView.Sort = (VE.Checkbox17 == true ? "sagslnm,sagslcd" : "agslnm,agslcd") + ",glcd,slnm,slcd,docdt,bldt1,docno";
                        tbl = tbl.DefaultView.ToTable();
                    }
                    else
                    {
                        tbl.DefaultView.Sort = (VE.Checkbox17 == true ? "sagslnm,sagslcd" : "agslnm,agslcd") + ",glcd,slnm,slcd,bldt1,docdt,docno";
                        tbl = tbl.DefaultView.ToTable();
                    }
                }
                if (Para2 != "")
                {
                    tbl.DefaultView.Sort = "agslnm,agslcd,glcd,slnm,slcd,rtdebnm,rtdebcd,docdt,docno";
                    tbl = tbl.DefaultView.ToTable();
                }

                string glcd = tbl.Rows[0]["glcd"].ToString(); string glnm = tbl.Rows[0]["glnm"].ToString();
                string dtlsumm = format;
                if (format == "B") dtlsumm = "D"; else dtlsumm = "S";

                HC.RepStart(IR, 3);
                if (dtlsumm == "D")
                {
                    if (VE.Checkbox15 == true) HC.GetPrintHeader(IR, "agshortnm", "string", "c,10", "Agent Name");
                    if (VE.MENU_PARA.Split(',')[0].retStr() == "CR")
                    {
                        HC.GetPrintHeader(IR, "docdt", "string", "c,10", "Doc;Date");
                        HC.GetPrintHeader(IR, "docno", "string", "c,15", "Doc;No.");
                    }
                    HC.GetPrintHeader(IR, "bldt", "string", "c,10", "Bill;Date");
                    HC.GetPrintHeader(IR, "blno", "string", "c,17", "Bill;No.");
                    if (bltype == true) HC.GetPrintHeader(IR, "bltype", "string", "c,5", "Bill Type");
                    if (itamtprint == true) HC.GetPrintHeader(IR, "itamt", "double", "n,16,2", "Item Value");
                    HC.GetPrintHeader(IR, "blamt", "double", "n,16,2", "Op/Bill Amt");
                    if (PymtDaysprint == true) HC.GetPrintHeader(IR, "cdays", "double", "n,4,0", ";Days");
                    HC.GetPrintHeader(IR, "adjno", "string", "c,15", "Adj;RefNo.");
                    HC.GetPrintHeader(IR, "adjdt", "string", "c,10", "Adj;Date");
                    HC.GetPrintHeader(IR, "adjamt", "double", "n,16,2", "Adj Amt");
                    HC.GetPrintHeader(IR, "dueamt", "double", "n,16,2", "Due Amt");
                    if (VE.Checkbox12 == true && dtlsumm == "D") HC.GetPrintHeader(IR, "blrem", "string", "c,35", "Remarks");
                    if (PymtDaysprint == true) HC.GetPrintHeader(IR, "pdays", "double", "n,4,0", "Payment;Days");
                    HC.GetPrintHeader(IR, "prem", "string", "c,15", "Payment;Remarks");
                    if (dtlsumm == "D" && VE.Checkbox2 == true)
                    {
                        HC.GetPrintHeader(IR, "ordno", "string", "c,20", "Order;No.");
                        HC.GetPrintHeader(IR, "orddt", "string", "c,10", "Order;Date");
                    }
                    if (VE.Checkbox9 == true && dtlsumm == "D") HC.GetPrintHeader(IR, "lrno", "string", "c,15", "LR;No.");
                    if ((VE.Checkbox3 == true || VE.Checkbox9 == true) && dtlsumm == "D") HC.GetPrintHeader(IR, "lrdt", "string", "c,10", "LR;Date");
                    if (VE.Checkbox9 == true && dtlsumm == "D") HC.GetPrintHeader(IR, "transnm", "string", "c,35", "Transporter");
                }
                else
                {
                    HC.GetPrintHeader(IR, "slno", "string", "c,4", "Sl#");
                    HC.GetPrintHeader(IR, "blno", "string", "c,8", "Party Cd");
                    HC.GetPrintHeader(IR, "slnm", "string", "c,35", "Party Name");
                    HC.GetPrintHeader(IR, "dueamt", "double", "n,16,2", "Due Amt");
                    HC.GetPrintHeader(IR, "pdays", "double", "n,4,0", "Av.Pymt;Days");
                }
                if (showOrder == true) HC.GetPrintHeader(IR, "ordno", "string", "c,30", "Order No.");

                double days = 0, cdays = 0, odays = 0;
                double mult = 1;
                string glcd1 = "", slcd1 = "", slnm1 = "", slcity1 = "", slphno1 = "";
                Int32 slno1 = 0;
                Int32 maxR = 0, i = 0, rNo = 0;

                i = 0; maxR = tbl.Rows.Count - 1;
                string agslcdfld1 = "agslcd", agslcdfld2 = "agslnm";
                if (VE.Checkbox17 == true) { agslcdfld1 = "sagslcd"; agslcdfld2 = "sagslnm"; }
                double gdueamt = 0, adueamt = 0;
                while (i <= maxR)
                {
                    glcd1 = tbl.Rows[i]["glcd"].ToString();
                    string chkagslcd = tbl.Rows[i][agslcdfld1].retStr();
                    if (showagent == true)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        string sldsp = agdsp + " - ";
                        sldsp += tbl.Rows[i][agslcdfld2].retStr();
                        //sldsp += "<span style='font-weight:100;font-size:11px;'>" + " [" + slcity1 + "]  " + " </span>";
                        sldsp += "<span style='font-weight:100;font-size:9px;'>" + " " + tbl.Rows[i][agslcdfld1].retStr() + "  " + " </span>";
                        IR.Rows[rNo]["Dammy"] = sldsp;
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                    }
                    adueamt = 0;
                    while (tbl.Rows[i]["glcd"].ToString() == glcd1 && tbl.Rows[i][agslcdfld1].ToString() == chkagslcd)
                    {
                        slcd1 = (Para2 == "" ? tbl.Rows[i]["slcd"].ToString() : tbl.Rows[i]["rtdebcd"].ToString());
                        slnm1 = (Para2 == "" ? tbl.Rows[i]["slnm"].ToString() : tbl.Rows[i]["rtdebnm"].ToString());
                        slphno1 = (Para2 == "" ? tbl.Rows[i]["phno"].retStr() : tbl.Rows[i]["rtdebmobile"].retStr());
                        slcity1 = (Para2 == "" ? tbl.Rows[i]["slcity"].ToString() : tbl.Rows[i]["retdebarea"].ToString());
                        if (dtlsumm == "D")
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;

                            string sldsp = "";
                            sldsp = slnm1;
                            sldsp += "<span style='font-weight:100;font-size:11px;'>" + " [" + slcity1 + "]  " + " </span>";
                            sldsp += "<span style='font-weight:100;font-size:9px;'>" + " " + slcd1 + "  " + " </span>";
                            if (slphno1 != "") sldsp += IR.Rows[rNo]["Dammy"] + " Ph. " + " </span>" + slphno1;
                            IR.Rows[rNo]["Dammy"] = sldsp;
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                        }
                        double tdays = 0, tdueamt = 0, tbills = 0;
                        slno1++;
                        while (tbl.Rows[i]["glcd"].ToString() == glcd1 && (Para2 == "" ? tbl.Rows[i]["slcd"].ToString() : tbl.Rows[i]["rtdebcd"].ToString()) == slcd1)
                        {
                            TimeSpan TSdys, TCdys;
                            //calculate credit days
                            cdays = Convert.ToDouble(tbl.Rows[i]["crdays"]);
                            if (cdays == 0)
                            {
                                if (tbl.Rows[i]["duedt"].ToString() == "") cdays = 0;
                                else
                                {
                                    if (tbl.Rows[i]["bldt"].ToString() != null && tbl.Rows[i]["bldt"].ToString() != "") TCdys = Convert.ToDateTime(tbl.Rows[i]["duedt"]) - Convert.ToDateTime(tbl.Rows[i]["bldt"].ToString().Substring(0, 10));
                                    else TCdys = Convert.ToDateTime(tbl.Rows[i]["duedt"]) - Convert.ToDateTime(tbl.Rows[i]["docdt"].ToString().Substring(0, 10));
                                    cdays = TCdys.Days;
                                }
                            }
                            //
                            double checkdays = cdays;
                            days = 0;
                            string checkdt = "";
                            if (due_caldt == "D" || due_caldt == "L")
                            {
                                if (due_caldt == "D") checkdt = tbl.Rows[i]["duedt"].retStr(); else checkdt = tbl.Rows[i]["lrdt"].retStr();
                                if (checkdt == "") checkdt = tbl.Rows[i]["docdt"].retStr();
                                TSdys = Convert.ToDateTime(TD) - Convert.ToDateTime(checkdt);
                                days = cdays + TSdys.Days;
                            }
                            else
                            {
                                TSdys = Convert.ToDateTime(TD) - Convert.ToDateTime(tbl.Rows[i]["docdt"]);
                                days = TSdys.Days;
                            }
                            if (ShowDuedaysfrom == "D") { days = days - cdays; checkdays = 0; }
                            if (days < 0) days = 0;
                            string iautoslno = tbl.Rows[i]["autoslno"].ToString();
                            var rsTxn = (from DataRow dr in txn.Rows
                                         select new
                                         {
                                             vautoslno = dr["vautoslno"],
                                             autoslno = dr["autoslno"],
                                             adjdt = dr["docdt"],
                                             adjdocno = dr["docno"],
                                             adjno = dr["doccdno"],
                                             adjamt = dr["adj_amt"],
                                             vchtype = dr["vchtype"],
                                             pymtrem = dr["pymtrem"],
                                             docnm = dr["docnm"],
                                         }).Where(a => a.autoslno.ToString() == iautoslno).ToList();
                            bool recshow = true;
                            if (rsTxn != null)
                            {
                                if (rsTxn.Count == 1 && rsTxn[0].vautoslno.ToString() == iautoslno && Math.Abs(rsTxn[0].adjamt.ToString().retDbl()) == Math.Abs(tbl.Rows[i]["amt"].ToString().retDbl())) recshow = (VE.Checkbox11 == true ? true : false);
                            }
                            if (recshow == true)
                            {
                                if (dtlsumm == "D")
                                {
                                    if (ShowDuedaysfrom == "D") checkdays = 0;
                                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                    if (VE.MENU_PARA.Split(',')[0].retStr() == "CR")
                                    {
                                        IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].ToString().Substring(0, 10);
                                        //IR.Rows[rNo]["docno"] = tbl.Rows[i]["doccd"].ToString() + tbl.Rows[i]["docno"].ToString();
                                        IR.Rows[rNo]["docno"] = tbl.Rows[i]["tchdocno"].ToString();
                                    }
                                    if (VE.Checkbox15 == true) IR.Rows[rNo]["agshortnm"] = tbl.Rows[i]["agshortnm"].retStr() + (tbl.Rows[i]["sagshortnm"].retStr() == "" ? "" : " - " + tbl.Rows[i]["sagshortnm"].retStr());
                                    if (tbl.Rows[i]["blno"].ToString() == "") IR.Rows[rNo]["blno"] = tbl.Rows[i]["doccd"].ToString() + tbl.Rows[i]["docno"].ToString();
                                    else IR.Rows[rNo]["blno"] = tbl.Rows[i]["blno"].ToString();
                                    if (tbl.Rows[i]["bldt"].ToString() == "") IR.Rows[rNo]["bldt"] = tbl.Rows[i]["docdt"].ToString().Substring(0, 10);
                                    else IR.Rows[rNo]["bldt"] = tbl.Rows[i]["bldt"].ToString();
                                    if (bltype == true) IR.Rows[rNo]["bltype"] = tbl.Rows[i]["bltype"].retStr();
                                    if (PymtDaysprint == true) IR.Rows[rNo]["cdays"] = days; //cdays
                                    IR.Rows[rNo]["blamt"] = tbl.Rows[i]["amt"].ToString().retDbl();

                                    if (VE.Checkbox12 == true && dtlsumm == "D") IR.Rows[rNo]["blrem"] = tbl.Rows[i]["billrem"];
                                    if (itamtprint == true) IR.Rows[rNo]["itamt"] = tbl.Rows[i]["itamt"].ToString().retDbl();
                                    if (VE.Checkbox2 == true && dtlsumm == "D") IR.Rows[rNo]["ordno"] = tbl.Rows[i]["ordno"];
                                    if (VE.Checkbox2 == true && dtlsumm == "D") IR.Rows[rNo]["orddt"] = tbl.Rows[i]["orddt"];
                                    if (VE.Checkbox9 == true && dtlsumm == "D") IR.Rows[rNo]["lrno"] = tbl.Rows[i]["lrno"];
                                    if ((VE.Checkbox3 == true || VE.Checkbox9 == true) && dtlsumm == "D") IR.Rows[rNo]["lrdt"] = tbl.Rows[i]["lrdt"];
                                    if (VE.Checkbox9 == true && dtlsumm == "D") IR.Rows[rNo]["transnm"] = tbl.Rows[i]["transnm"];
                                }
                                days = 0;
                                double dueamt = tbl.Rows[i]["amt"].ToString().retDbl();
                                #region //Check in txn Datatable
                                if (rsTxn != null)
                                {
                                    bool newrow = false;
                                    foreach (var txdr in rsTxn)
                                    {
                                        days = 0;
                                        if (ShowDuedaysfrom == "D" && tbl.Rows[i]["duedt"].ToString() == "") days = 0;
                                        else if (txdr.adjdt.ToString() == "") days = 0;
                                        else
                                        {
                                            if (ShowDuedaysfrom == "D") TSdys = Convert.ToDateTime(txdr.adjdt.ToString()) - Convert.ToDateTime(tbl.Rows[i]["duedt"]);
                                            else TSdys = Convert.ToDateTime(txdr.adjdt.ToString()) - Convert.ToDateTime(tbl.Rows[i]["docdt"]);
                                            days = TSdys.Days;
                                            if (days < 0) days = 0;
                                        }

                                        if (dtlsumm == "D")
                                        {
                                            if (newrow == true)
                                            {
                                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                            }
                                            IR.Rows[rNo]["adjdt"] = txdr.adjdt.ToString().retDateStr();
                                            IR.Rows[rNo]["adjno"] = (txdr.autoslno == txdr.vautoslno || txdr.adjdocno.retStr() == txdr.adjno.retStr() ? txdr.adjdocno : txdr.adjno.retStr() + "/" + txdr.adjdocno.retStr());
                                            IR.Rows[rNo]["adjamt"] = txdr.adjamt.ToString();
                                            if (PymtDaysprint == true) IR.Rows[rNo]["pdays"] = days;
                                            string prem = "";
                                            switch (txdr.vchtype.ToString())
                                            {
                                                case "ADV":
                                                    prem = "Adv"; break;
                                                case "DSC":
                                                    prem = "Disc"; break;
                                                case "TDS":
                                                    prem = "TDS"; break;
                                                default:
                                                    prem = ""; break;
                                            }
                                            IR.Rows[rNo]["prem"] = prem + txdr.docnm.retStr() + (txdr.pymtrem.retStr() != "" ? " # " : "") + txdr.pymtrem.retStr();
                                        }
                                        if (tbl.Rows[i]["amt"].ToString().retDbl() < 0) dueamt = dueamt + txdr.adjamt.ToString().retDbl();
                                        else dueamt = dueamt - txdr.adjamt.ToString().retDbl();
                                        newrow = true;
                                    }
                                }
                                #endregion
                                if (dtlsumm == "D") IR.Rows[rNo]["dueamt"] = dueamt;
                                tdays = tdays + days;
                                tbills = tbills + 1;
                                tdueamt = tdueamt + dueamt;
                            }
                            i++;
                            if (i > maxR) break;
                        }
                        double avdays = 0;
                        if (tdays > 0) avdays = tdays / tbills;
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        if (dtlsumm == "D")
                        {
                            IR.Rows[rNo]["blno"] = "Totals";
                            //IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 1px solid;";
                        }
                        else
                        {
                            IR.Rows[rNo]["slno"] = slno1;
                            IR.Rows[rNo]["blno"] = tbl.Rows[i - 1]["slcd"].ToString();
                            IR.Rows[rNo]["slnm"] = tbl.Rows[i - 1]["slnm"].ToString();
                        }
                        IR.Rows[rNo]["dueamt"] = tdueamt;
                        if (PymtDaysprint == true) IR.Rows[rNo]["pdays"] = avdays;
                        adueamt = adueamt + tdueamt;
                        gdueamt = gdueamt + tdueamt;
                        if (i > maxR) break;
                    }
                    if (showagent == true)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["blno"] = agdsp + " ( " + tbl.Rows[i - 1][agslcdfld2].retStr() + " Totals";
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 1px solid;";
                        IR.Rows[rNo]["dueamt"] = adueamt;
                    }
                    if (i > maxR) break;
                }
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["blno"] = "Grand Totals";
                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 1px solid;";
                IR.Rows[rNo]["dueamt"] = gdueamt;

                pghdr1 = "Bill Wise Outstanding (Pay Days) of " + glnm + " (" + glcd + ") " + (Para2 == "" ? "" : "[Retail Party] ") + "as on " + TD;
                if (submitbutton == "Download Excel")
                {
                    IR.Columns.Remove("dammy");
                    IR.Columns.Remove("flag");
                    IR.Columns.Remove("celldesign");
                    IR = IR.Rows.Cast<DataRow>().Where(row => !row.ItemArray.All(field => field is DBNull || string.IsNullOrWhiteSpace(field as string))).CopyToDataTable();
                    DataTable[] exdt = new DataTable[1];
                    exdt[0] = IR;
                    string[] sheetname = new string[1];
                    sheetname[0] = "Sheet1";
                    MasterHelp.ExcelfromDataTables(exdt, sheetname, "Bill_Outstanding".retRepname(), false, pghdr1);
                    return Content("Downloaded");
                }
                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);

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