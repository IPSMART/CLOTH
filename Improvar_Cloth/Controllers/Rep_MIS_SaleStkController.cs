using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;

namespace Improvar.Controllers
{
    public class Rep_MIS_SaleStkController : Controller
    {
        // GET: Rep_MIS_SaleStk
        public static string[,] headerArray;
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_MIS_SaleStk
        public ActionResult Rep_MIS_SaleStk()
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
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    ViewBag.formname = " MIS Sales Stock";
                    //VE.DropDown_list = (from i in DB.M_GROUP where (selgrpcd.Contains(i.ITGRPCD)) select new DropDown_list() { value = i.ITGRPCD, text = i.ITGRPNM }).OrderBy(s => s.text).ToList();
                    //VE.DropDown_list1 = (from i in DB.M_ITEMPLIST
                    //                     join j in DB.T_BATCHMST on i.ITMPRCCD equals j.ITMPRCCD
                    //                     where (selgrpcd.Contains(i.ITGRPCD))
                    //                     select new DropDown_list1()
                    //                     { value = i.ITMPRCCD, text = i.PRCDESC }).Distinct().OrderBy(s => s.text).ToList();
                    //VE.DropDown_list_text = (from i in items select new DropDown_list_text() { value = i.value, text1 = i.text1 + " [" + i.text3 + "]" }).OrderBy(s => s.text1).ToList();
                    VE.DropDown_list_ITGRP = DropDownHelp.GetItgrpcdforSelection();
                    VE.Itgrpnm = MasterHelp.ComboFill("itgrpcd", VE.DropDown_list_ITGRP, 0, 1);
                    VE.DropDown_list2 = (from i in DBF.M_GODOWN
                                         select new DropDown_list2()
                                         {
                                             value = i.GOCD,
                                             text = i.GONM
                                         }).Distinct().OrderBy(s => s.text).ToList();

                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);

                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection();
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);
                    
                    VE.DefaultView = true;
                    VE.FDT = CommVar.FinStartDate(UNQSNO);
                    VE.TDT = CommVar.CurrDate(UNQSNO);
                    VE.Checkbox2 = true;
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
        public ActionResult Rep_MIS_SaleStk(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                Cn.getQueryString(VE);
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string fdt = VE.FDT.retDateStr();
                string tdt = VE.TDT.retDateStr();

                string itgrpcd = "";
                string rateqntybag = "B";
                //if (FC["RATEQNTYBAG"].ToString() == "BAGS") rateqntybag = "B";
                //else rateqntybag = "Q";

                string query = ""; string query1 = "";

                string mfdt = "01" + fdt.Substring(2, 8);
                string yfdt = "01/01" + fdt.Substring(5, 5);

                Int32 i = 0;
                Int32 maxR = 0;
                string chkval, chkval1 = "";
                string selitcd = "", plist = "", selgocd = "", selgonm = "", LOCCD = "", party = "";

                if (FC.AllKeys.Contains("Godown"))
                {
                    selgocd = FC["Godown"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("slcdvalue"))
                {
                    party = FC["slcdvalue"].ToString().retSqlformat();
                }
                
                if (FC.AllKeys.Contains("itcdvalue"))  //'ITEM'
                {
                    selitcd = CommFunc.retSqlformat(FC["itcdvalue"].ToString());
                }
                if (FC.AllKeys.Contains("itgrpcdvalue")) itgrpcd = CommFunc.retSqlformat(FC["itgrpcdvalue"].retStr());
                string mtrljobcd = "'FS'";
                string prccd = "WP";

                bool showbatch = true;
                var MSYSCNFG = Salesfunc.M_SYSCNFG(tdt.retDateStr());
                string sql = "";


                sql += Environment.NewLine + "select a.doccd,a.doctag,a.docnm, a.docdt,a.stkdrcr,a.itcd,s.styleno, ";
                sql += Environment.NewLine + "a.qnty,b.prccd, b.effdt, b.rate wprate, q.rate rprate from ";

                sql += Environment.NewLine + " (select  b.doccd, b.doctag, c.docdt, a.stkdrcr, a.itcd, ";
                sql += Environment.NewLine + " sum(case a.stkdrcr when 'D' then a.qnty else a.qnty * -1 end) qnty,d.docnm ";
                sql += Environment.NewLine + " from "+ scm + ".t_txndtl a, " + scm + ".t_txn b, " + scm + ".t_cntrl_hdr c, " + scm + ".m_doctype d, ";
                sql += Environment.NewLine + "" + scmf + ".m_subleg i ";
                sql += Environment.NewLine + "where a.autono = b.autono(+) and a.autono = c.autono(+) and c.doccd = d.doccd(+) and b.slcd = i.slcd(+) and ";

                sql += Environment.NewLine + "nvl(c.cancel, 'N') = 'N' and c.compcd = '"+COM+"' and c.loccd='" + LOC + "'";
               // sql += Environment.NewLine + "and c.docdt >= to_date('"+fdt+"', 'dd/mm/yyyy') ";
                sql += Environment.NewLine + " and c.docdt <= to_date('"+tdt+"', 'dd/mm/yyyy') ";
                sql += Environment.NewLine + " and a.mtrljobcd in ("+ mtrljobcd + ") ";
                sql += Environment.NewLine + " group by b.doccd, b.doctag, c.docdt, a.stkdrcr, a.itcd,d.docnm) a, ";

                for (int x = 0; x <= 1; x++)
                {
                    string sqlals = "";
                    switch (x)
                    {
                        case 0:
                            sqlals = "b"; break;
                        case 1:
                            prccd = "RP"; sqlals = "q"; break;

                    }
                    sql += Environment.NewLine + "(select a.barno, a.itcd, a.colrcd, a.sizecd, a.prccd, a.effdt, a.rate from " + Environment.NewLine;
                    sql += Environment.NewLine + "(select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate from " + Environment.NewLine;
                    sql += Environment.NewLine+ "(select a.barno, a.prccd, a.effdt, " + Environment.NewLine;
                    sql += Environment.NewLine+ "row_number() over (partition by a.barno, a.prccd order by a.effdt desc) as rn " + Environment.NewLine;
                    sql += Environment.NewLine+ "from " + scm + ".t_batchmst_price a where nvl(a.rate,0) <> 0 and a.effdt <= to_date('" + tdt + "','dd/mm/yyyy') ) " + Environment.NewLine;
                    sql += Environment.NewLine+ "a, " + scm + ".t_batchmst_price b, " + scm + ".t_batchmst c " + Environment.NewLine;
                    sql += Environment.NewLine+ "where a.barno=b.barno(+) and a.prccd=b.prccd(+) and a.effdt=b.effdt(+) and a.rn=1 and a.barno=c.barno(+) " + Environment.NewLine;
                    sql += Environment.NewLine+ ") a where prccd='" + prccd + "' " + Environment.NewLine;
                    sql += Environment.NewLine+ ") " + sqlals;
                    if (x != 2) sql += ", ";
                }
                sql += Environment.NewLine + "" + scm + ".m_sitem s ";
                sql += Environment.NewLine+"where a.itcd = b.itcd(+) and a.itcd = q.itcd(+)and a.itcd = s.itcd(+) ";
                if (selitcd != "") sql += Environment.NewLine + "and a.itcd in("+ selitcd + ") ";
                 sql += "order by styleno, docdt ";
                DataTable tbl = MasterHelp.SQLquery(sql);
                DataTable Docdesc = new DataTable("doccd");

                if (tbl.Rows.Count == 0)
                {
                    return RedirectToAction("NoRecords", "RPTViewer");
                }
                DataTable IR = new DataTable("mstrep");
                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                string days_aging = VE.TEXTBOX5; // Days aging value
                string days1 = "0", days2 = "0", days3 = "0";
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
                long duedays1, duedays2, duedays3, duedays4, duedays5 = 0;
                long ageingperiod = 0;
                duedays1 = Convert.ToInt64(days1); duedays2 = Convert.ToInt64(days2); duedays3 = Convert.ToInt64(days3);
                ageingperiod = Convert.ToInt64(days_aging);

                long due1fDys = 0, due1tDys = 0, due2fDys = 0, due2tDys = 0, due3fDys = 0, due3tDys = 0, due4fDys = 0, due4tDys = 0;
                if (ageingperiod != 0)
                {
                    ageingperiod = ageingperiod + 1;

                    if (ageingperiod >= 1) due1fDys = 0; due1tDys = duedays1;
                    if (ageingperiod >= 2) due2fDys = due1tDys + 1; due2tDys = duedays2;
                    if (ageingperiod >= 3) due3fDys = due2tDys + 1; due3tDys = duedays3;
                    if (ageingperiod >= 4) due4fDys = due3tDys + 1; due4tDys = 0;

                    //define last ageing column
                    if (ageingperiod == 2) due2tDys = 99999;
                    if (ageingperiod == 3) due3tDys = 99999;
                    if (ageingperiod == 4) due4tDys = 99999;
                }
                double gdue1Amt = 0, gdue2Amt = 0, gdue3Amt = 0, gdue4Amt = 0;
                double gdue1Qty = 0, gdue2Qty = 0, gdue3Qty = 0, gdue4Qty = 0;
                double bdue1Amt = 0, bdue2Amt = 0, bdue3Amt = 0, bdue4Amt = 0;
                double bdue1Qty = 0, bdue2Qty = 0, bdue3Qty = 0, bdue4Qty = 0;
                double due1Amt = 0, due2Amt = 0, due3Amt = 0, due4Amt = 0;
                double due1Qty = 0, due2Qty = 0, due3Qty = 0, due4Qty = 0;
                string qdsp = "";
                //if (rateqntybag == "B") qdsp = "n,12";
                //qdsp = "n,12,4";
                qdsp = "n,12,2";
                string[] DOCCOLS = new string[] { "doctag", "doccd", "docnm" };
                Docdesc = tbl.DefaultView.ToTable(true, DOCCOLS);
                Docdesc = Docdesc.Select("doctag not in('OP','PI')").CopyToDataTable();
                Docdesc.Columns.Add("docqty", typeof(double));
                HC.RepStart(IR, 3);
                //HC.RepStart(IR, 2);
                HC.GetPrintHeader(IR, "styleno", "string", "c,16", "Article");
                HC.GetPrintHeader(IR, "purrate", "double", "n,10,2", "Pur Rate");
                HC.GetPrintHeader(IR, "openingqnty", "double", "n,10,2", "Opening");
                foreach (DataRow dr in Docdesc.Rows)
                {
                    //if((dr["doctag"].ToString() + dr["doccd"].ToString()!="OPSFOST")  && (dr["doctag"].ToString() + dr["doccd"].ToString() != "PISPROF"))
                    //{
                        HC.GetPrintHeader(IR, dr["doctag"].ToString() + dr["doccd"].ToString(), "double", "n,14,3", dr["docnm"].ToString());
                    //}
                   
                }
                HC.GetPrintHeader(IR, "stockqnty", "double", "n,10,2", "Stock");
                if (ageingperiod >= 1) HC.GetPrintHeader(IR, "stk1qty", "double", "n,14,3", "<= " + due1tDys.ToString() + ";Qty");
                if (ageingperiod >= 2) HC.GetPrintHeader(IR, "stk2qty", "double", "n,14,3", due2fDys.ToString() + " to " + due2tDys.ToString() + ";Qty");
                if (ageingperiod >= 3) HC.GetPrintHeader(IR, "stk3qty", "double", "n,14,3", due3fDys.ToString() + " to " + due3tDys.ToString() + ";Qty");
                if (ageingperiod >= 4) HC.GetPrintHeader(IR, "stk4qty", "double", "n,14,3", "> " + due3tDys.ToString() + ";Qty");
                HC.GetPrintHeader(IR, "indt", "string", "c,10", "Last In Date");
                HC.GetPrintHeader(IR, "outdt", "string", "c,10", "Last Out Date");
                HC.GetPrintHeader(IR, "days", "double", "n,10", "Days");
                double iop = 0, idr = 0, icr = 0, icls = 0, idramt = 0, icramt = 0;
                double top = 0, tdr = 0, tcr = 0, tcls = 0, tdramt = 0, tcramt = 0, tblqty = 0;
                double dbqty = 0, dbamt = 0;

                top = 0; tdr = 0; tcr = 0; tcls = 0;
                string DOCTAG = "";
                Int32 rNo = 0;

                // Report begins
                i = 0; maxR = tbl.Rows.Count - 1;
                gdue1Amt = 0; gdue2Amt = 0; gdue3Amt = 0; gdue4Amt = 0;
                gdue1Qty = 0; gdue2Qty = 0; gdue3Qty = 0; gdue4Qty = 0;
                while (i <= maxR)
                {
                    chkval = tbl.Rows[i]["itcd"].ToString();
                    iop = 0; idr = 0; icr = 0; icls = 0; idramt = 0; icramt = 0;

                    double days = 0;
                    TimeSpan TSdys;
                    if (tbl.Rows[i]["docdt"] == DBNull.Value) days = 0;
                    else
                    {
                        TSdys = Convert.ToDateTime(tdt) - Convert.ToDateTime(tbl.Rows[i]["docdt"]);
                        days = TSdys.Days;
                    }
                    due1Amt = 0; due2Amt = 0; due3Amt = 0; due4Amt = 0;
                    due1Qty = 0; due2Qty = 0; due3Qty = 0; due4Qty = 0;
                    string lstInDt = "", lstOutDt = "";
                    while (tbl.Rows[i]["itcd"].ToString() == chkval)
                    {
                        double bqnty = 0, bnos = 0, bval = 0, bamt = 0;
                        while (tbl.Rows[i]["itcd"].ToString() == chkval && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(fdt))
                        {
                            //if (rateqntybag == "B") dbqty = Convert.ToDouble(tbl.Rows[i]["nos"]);
                            dbqty = Convert.ToDouble(tbl.Rows[i]["qnty"]);

                            if (tbl.Rows[i]["stkdrcr"].ToString() == "D")
                            {
                               // bnos = bnos + tbl.Rows[i]["nos"].retDbl();
                                bqnty = bqnty + tbl.Rows[i]["qnty"].retDbl();
                                iop = iop + dbqty;
                            }
                            else if (tbl.Rows[i]["stkdrcr"].ToString() == "C")
                            {
                               // bnos = bnos - tbl.Rows[i]["nos"].retDbl();
                                bqnty = bqnty - tbl.Rows[i]["qnty"].retDbl();
                                iop = iop - dbqty;
                            }
                            i = i + 1;
                            if (i > maxR) break;
                        }
                        if (iop != 0)
                        {
                           // IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                           // IR.Rows[rNo]["docdt"] = fdt;
                           // IR.Rows[rNo]["slnm"] = "Opening";
                          //  IR.Rows[rNo]["balqnty"] = iop;
                            if (iop < 0) icr = iop * -1;
                            else idr = iop;
                            icls = iop;
                        }
                        if (i > maxR)
                        {
                            break;
                        }
                        double opqnty = 0; double opvalue = 0; double othersqnty = 0; double othersvalue = 0; double pbqnty = 0; double pbvalue = 0; double sbqnty = 0; double sbcmqnty = 0; double sbsskqnty = 0; double sbsrcmqnty = 0; double convqnty = 0; double sbvalue = 0; double srqnty = 0; double srvalue = 0; double prqnty = 0; double prvalue = 0; double tiqnty = 0; double tivalue = 0; double toqnty = 0; double tovalue = 0;
                        lstInDt = ""; lstOutDt="";
                        while (tbl.Rows[i]["itcd"].ToString() == chkval && Convert.ToDateTime(tbl.Rows[i]["docdt"]) <= Convert.ToDateTime(tdt))
                        {
                            //if (rateqntybag == "B") dbqty = Convert.ToDouble(tbl.Rows[i]["nos"]);
                            DOCTAG = tbl.Rows[i]["doctag"].ToString()+ tbl.Rows[i]["doccd"].ToString();

                            lstInDt = (from DataRow dr in tbl.Rows where dr["itcd"].retStr() == chkval && dr["doctag"].retStr() == "PB" select dr["docdt"].retDateStr()).Max();
                            lstOutDt = (from DataRow dr in tbl.Rows where dr["itcd"].retStr() == chkval && dr["doctag"].retStr() == "SB" select dr["docdt"].retDateStr()).Max();

                            foreach (DataRow amtdr in Docdesc.Rows)
                            {
                                if (DOCTAG == amtdr["doctag"].retStr()+ amtdr["doccd"].retStr())
                                {
                                   
                                    if (DOCTAG == "PBSSPBL")
                                    {
                                        pbqnty += tbl.Rows[i]["qnty"].retDbl();
                                        amtdr["docqty"] = pbqnty;
                                    }
                                    else if (DOCTAG == "PRSSPRM")
                                    {
                                        prqnty += tbl.Rows[i]["qnty"].retDbl() * -1;
                                        amtdr["docqty"] = prqnty;
                                    }
                                   
                                    else if (DOCTAG == "SBSSD" || DOCTAG == "SBSSBCM" || DOCTAG == "SBSSSK"  || DOCTAG == "SBSSRCM" || DOCTAG == "SRSRET")
                                    {
                                        

                                        if (DOCTAG == "SBSSD")
                                        {
                                            if (tbl.Rows[i]["stkdrcr"].retStr() == "C")
                                            {
                                                sbqnty += tbl.Rows[i]["qnty"].retDbl() * -1;
                                                amtdr["docqty"] = sbqnty;

                                            }
                                            else
                                            {
                                                sbqnty += tbl.Rows[i]["qnty"].retDbl();
                                                amtdr["docqty"] = sbqnty;

                                               
                                            }
                                            

                                        }
                                        else if(DOCTAG == "SBSSBCM")
                                        {
                                            if (tbl.Rows[i]["stkdrcr"].retStr() == "C")
                                            {
                                                sbcmqnty += tbl.Rows[i]["qnty"].retDbl() * -1;
                                                amtdr["docqty"] = sbcmqnty;

                                            }
                                            else
                                            {
                                                sbcmqnty += tbl.Rows[i]["qnty"].retDbl();
                                                amtdr["docqty"] = sbcmqnty;


                                            }
                                        }
                                        else if(DOCTAG == "SBSSSK")
                                        {
                                            if (tbl.Rows[i]["stkdrcr"].retStr() == "C")
                                            {
                                                sbsskqnty += tbl.Rows[i]["qnty"].retDbl() * -1;
                                                amtdr["docqty"] = sbsskqnty;

                                            }
                                            else
                                            {
                                                sbsskqnty += tbl.Rows[i]["qnty"].retDbl();
                                                amtdr["docqty"] = sbsskqnty;


                                            }
                                        }
                                        else if (DOCTAG == "SBSSRCM")
                                        {
                                            if (tbl.Rows[i]["stkdrcr"].retStr() == "C")
                                            {
                                                sbsrcmqnty += tbl.Rows[i]["qnty"].retDbl() * -1;
                                                amtdr["docqty"] = sbsrcmqnty;

                                            }
                                            else
                                            {
                                                sbsrcmqnty += tbl.Rows[i]["qnty"].retDbl();
                                                amtdr["docqty"] = sbsrcmqnty;


                                            }
                                        }
                                        else
                                        {
                                            srqnty += tbl.Rows[i]["qnty"].retDbl();
                                            amtdr["docqty"] = srqnty;
                                        }
                                    }
                                    else if (DOCTAG == "CNSSTCN")
                                    {
                                         if (tbl.Rows[i]["stkdrcr"].retStr() == "C")
                                            {
                                                convqnty += tbl.Rows[i]["qnty"].retDbl() * -1;
                                                amtdr["docqty"] = convqnty;

                                            }
                                            else
                                            {
                                            convqnty += tbl.Rows[i]["qnty"].retDbl();
                                                amtdr["docqty"] = convqnty;
                                            }
                                    }
                                    //else if (DOCTAG == "OPSFOST")
                                    //{ }
                                    else
                                    {
                                        othersqnty += tbl.Rows[i]["qnty"].retDbl();
                                        amtdr["docqty"] = othersqnty;
                                       
                                    }
                                   
                                   
                                }
                            }
                          
                        
                            icls = idr - icr;
                           // IR.Rows[rNo]["balqnty"] = icls;
                          //  tblqty = tblqty + icls;
                            i = i + 1;
                            if (i > maxR) break;
                        }
                        tblqty=(iop + pbqnty + tiqnty + srqnty + othersqnty) - (prqnty + toqnty + sbqnty+ sbcmqnty+ sbsskqnty+ sbsrcmqnty + convqnty);
                        if (ageingperiod > 0)
                        {
                            if (days <= due1tDys && due1tDys != 0) { due1Qty = due1Qty + tblqty; }
                            else if (days <= due2tDys && due2tDys != 0) { due2Qty = due2Qty + tblqty;}
                            else if (days <= due3tDys && due3tDys != 0) { due3Qty = due3Qty + tblqty;}
                            else { due4Qty = due4Qty + tblqty;}
                        }

                        if (i > maxR) break;
                    }
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["openingqnty"] = iop;
                    IR.Rows[rNo]["styleno"] = tbl.Rows[i - 1]["styleno"].ToString();
                    IR.Rows[rNo]["purrate"] = tbl.Rows[i - 1]["wprate"].retDbl();
                    IR.Rows[rNo]["indt"] = lstInDt;
                    IR.Rows[rNo]["outdt"] = lstOutDt;
                    foreach (DataRow amtdr in Docdesc.Rows)
                    {
                        IR.Rows[rNo][amtdr["doctag"].ToString()+ amtdr["doccd"].ToString()] = amtdr["docqty"].retDbl();
                       // tqty += amtdr["goqty"].retDbl();

                    }
                    IR.Rows[rNo]["stockqnty"] = tblqty;
                    if (ageingperiod > 0)
                    {
                        if (due1Qty != 0) { IR.Rows[rNo]["stk1qty"] = due1Qty; }
                        if (due2Qty != 0) { IR.Rows[rNo]["stk2qty"] = due2Qty;}
                        if (due3Qty != 0) { IR.Rows[rNo]["stk3qty"] = due3Qty;}
                        if (due4Qty != 0) { IR.Rows[rNo]["stk4qty"] = due4Qty;}
                    }
                    //IR.Rows[rNo]["balqnty"] = icls;
                    //IR.Rows[rNo]["balqnty"] = tblqty;
                    // IR.Rows[rNo]["balqnty"] = idr - icr;

                    //top = top + iop;
                    //tdr = tdr + idr; tdramt = tdramt + idramt;
                    //tcr = tcr + icr; tcramt = tcramt + icramt;
                    //tcls = tcls + icls;
                }

                // Create Blank line
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["dammy"] = " ";
                IR.Rows[rNo]["flag"] = " height:14px; ";

                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["dammy"] = "";
                IR.Rows[rNo]["styleno"] = "Grand Totals";
                IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                IR.Rows[rNo]["openingqnty"] = IR.AsEnumerable().Where(a => a.Field<double?>("openingqnty").retDbl() !=0).Sum(b => b.Field<double?>("openingqnty")==null?0: b.Field<double?>("openingqnty"));
                IR.Rows[rNo]["stockqnty"] = IR.AsEnumerable().Where(a => a.Field<double?>("stockqnty").retDbl() != 0).Sum(b => b.Field<double?>("stockqnty") == null ? 0 : b.Field<double?>("stockqnty"));
                if (ageingperiod > 0)
                {
                    IR.Rows[rNo]["stk1qty"] = IR.AsEnumerable().Where(a => a.Field<double?>("stk1qty").retDbl() != 0).Sum(b => b.Field<double?>("stk1qty") == null ? 0 : b.Field<double?>("stk1qty"));
                    IR.Rows[rNo]["stk2qty"] = IR.AsEnumerable().Where(a => a.Field<double?>("stk2qty").retDbl() != 0).Sum(b => b.Field<double?>("stk2qty") == null ? 0 : b.Field<double?>("stk2qty"));
                    IR.Rows[rNo]["stk3qty"] = IR.AsEnumerable().Where(a => a.Field<double?>("stk3qty").retDbl() != 0).Sum(b => b.Field<double?>("stk3qty") == null ? 0 : b.Field<double?>("stk3qty"));
                    IR.Rows[rNo]["stk4qty"] = IR.AsEnumerable().Where(a => a.Field<double?>("stk4qty").retDbl() != 0).Sum(b => b.Field<double?>("stk4qty") == null ? 0 : b.Field<double?>("stk4qty"));
                    
                }

                for (int k = 6; k <= IR.Columns.Count - 4; k++)
                {
                    IR.Rows[rNo][IR.Columns[k].ColumnName] = IR.AsEnumerable().Sum(g => g.Field<double?>(IR.Columns[k].ColumnName).retDbl());
                }
                //  IR.Rows[rNo]["qntyin"] = tdr;

                //  IR.Rows[rNo]["qntyout"] = tcr;

                // IR.Rows[rNo]["balqnty"] = tdr - tcr;

                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                string jobnm = DB.M_JOBMST.Find(VE.MENU_PARA)?.JOBNM;
                string pghdr1 = "";
                string repname = "Rep_MIS_SaleStk";
                pghdr1 = "MIS Sales Stock from " + fdt + " to " + tdt;
                if (selgocd != "")
                {
                    selgonm = "Godown: " + string.Join(",", (from a in DBF.M_GODOWN where (selgocd.Contains(a.GOCD)) select a.GONM).ToList()).retSqlformat();
                }
                if (FC.AllKeys.Contains("mtrljobcdvalue"))
                {
                    if (selgonm != "") selgonm += "</br>";
                    selgonm += "Material Job: " + CommFunc.retSqlformat(FC["mtrljobcdtext"].ToString()).Replace("*", ",").Replace("'", "");
                }
                PV = HC.ShowReport(IR, repname, pghdr1, selgonm, true, true, "L", false);

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