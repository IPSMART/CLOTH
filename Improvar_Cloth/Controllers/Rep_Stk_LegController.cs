using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;

namespace Improvar.Controllers
{
    public class Rep_Stk_LegController : Controller
    {
        public static string[,] headerArray;
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
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    string jobnm = DB.M_JOBMST.Find(VE.MENU_PARA)?.JOBNM;
                    ViewBag.formname = jobnm + " Stock Ledger";
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


                    var locnmlist = (from a in DBF.M_LOCA
                                     select new DropDown_list2()
                                     {
                                         value = a.LOCCD,
                                         text = a.LOCNM
                                     }).Distinct().ToList();
                    VE.Locnm = MasterHelp.ComboFill("loccd", locnmlist, 0, 1);

                    string SCHEMA = CommVar.CurSchema(UNQSNO);
                    string SQL = "select a.MTRLJOBCD,a.MTRLJOBNM  ";
                    SQL += "from " + SCHEMA + ".M_MTRLJOBMST a, " + SCHEMA + ".m_cntrl_hdr b ";
                    SQL += "where a.m_autono = b.m_autono(+) and nvl(b.inactive_tag, 'N')= 'N' ";
                    SQL += "order by a.MTRLJOBNM ";
                    var data = MasterHelp.SQLquery(SQL);
                    if (data != null)
                    {
                        VE.DropDown_list3 = (from DataRow DR in data.Rows
                                             select new DropDown_list3()
                                             {
                                                 value = DR["MTRLJOBCD"].ToString(),
                                                 text = DR["MTRLJOBNM"].ToString()
                                             }).ToList();
                    }
                    VE.TEXTBOX2 = MasterHelp.ComboFill("mtrljobcd", VE.DropDown_list3, 0, 1);

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
        public ActionResult LoadItemCode(FormCollection FC, ReportViewinHtml VE, string val)
        {
            try
            {

                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = "Stk_Leg" });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }

        [HttpPost]
        public ActionResult Rep_Stk_Leg(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                Cn.getQueryString(VE);
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
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

                if (FC.AllKeys.Contains("plist"))
                {
                    plist = CommFunc.retSqlformat(FC["plist"].ToString());
                }

                if (FC.AllKeys.Contains("itcdvalue"))  //'ITEM'
                {
                    selitcd = CommFunc.retSqlformat(FC["itcdvalue"].ToString());
                }
                if (FC.AllKeys.Contains("loccdvalue")) LOCCD = CommFunc.retSqlformat(FC["loccdvalue"].ToString());
                if (FC.AllKeys.Contains("itgrpcdvalue")) itgrpcd = CommFunc.retSqlformat(FC["itgrpcdvalue"].retStr());
                string mtrljobcd = "'FS'";
                if (FC.AllKeys.Contains("mtrljobcdvalue")) mtrljobcd = CommFunc.retSqlformat(FC["mtrljobcdvalue"].ToString());

                bool showbatch = true;
                var MSYSCNFG = Salesfunc.M_SYSCNFG(tdt.retDateStr());
                string sql = "";
                sql += "select a.autono, a.slno, a.autoslno, a.stkdrcr, a.docno, a.docdt,a.docnm, a.prefno, a.prefdt, a.slnm, a.gstno,a.district,a.itcd itcd1 , a.itcd||nvl(c.styleno,' ') itcd, c.styleno, " + Environment.NewLine;
                sql += "c.itnm,c.styleno||' '||c.itnm itstyle,c.uomcd, d.uomnm, a.rate,a.pageslno, a.nos, a.qnty, nvl(a.netamt,0) netamt,a.txblval, b.batchnos,e.tgonm,f.fgonm,g.baleno from " + Environment.NewLine;
                sql += "(select a.autono, a.slno, a.autono||a.slno autoslno, a.stkdrcr, c.docno, c.docdt,d.docnm, b.prefno, b.prefdt, i.slnm, i.gstno,i.district, a.itcd, a.rate,a.pageslno, " + Environment.NewLine;
                sql += "sum(nvl(a.txblval,0)+nvl(a.othramt,0)) netamt,a.txblval, " + Environment.NewLine;
                sql += "sum(nvl(a.nos,0)) nos, sum(nvl(a.qnty,0)) qnty " + Environment.NewLine;
                sql += "from " + scm1 + ".t_txndtl a, " + scm1 + ".t_txn b, " + scm1 + ".t_cntrl_hdr c," + scm1 + ".m_doctype d, " + Environment.NewLine;
                sql += scmf + ".m_subleg i, " + scm1 + ".t_bale j " + Environment.NewLine;
                sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and c.doccd=d.doccd(+) and b.slcd=i.slcd(+) and " + Environment.NewLine;
                //if (MSYSCNFG.STKINCLPINV == "Y")
                //{
                //    sql += "a.stkdrcr in ('D','C','0') ";
                //}
                //else
                //{
                    sql += "a.stkdrcr in ('D','C') ";
                //}

                sql += "and  nvl(c.cancel,'N') = 'N' and c.compcd='" + COM + "' " + Environment.NewLine;
                sql += "and a.autono = j.autono(+) and a.slno=j.slno(+) " + Environment.NewLine;
                if (VE.Checkbox2 == false) sql += "and j.autono is null " + Environment.NewLine;
                if (selitcd.retStr() != "") sql += "and a.itcd in (" + selitcd + ")  " + Environment.NewLine;
                if (LOCCD != "") { sql += "and c.loccd in(" + LOCCD + ")  "; } else { sql += "and c.loccd='" + LOC + "' "; }
                if (plist.retStr() != "")
                {
                    //sql += "and k.itmprccd in (" + plist + ")  ";
                }
                if (selgocd.retStr() != "") sql += "and a.gocd in (" + selgocd + ") " + Environment.NewLine;
                if (party.retStr() != "") sql += "and b.slcd in (" + party + ") " + Environment.NewLine;
                //if (fdt != "") sql += "and c.docdt >= to_date('" + fdt + "','dd/mm/yyyy')   ";
                if (tdt != "") sql += "and c.docdt <= to_date('" + tdt + "','dd/mm/yyyy') " + Environment.NewLine;
                if (mtrljobcd.retStr() != "") sql += "and a.mtrljobcd in (" + mtrljobcd + ") " + Environment.NewLine;
                sql += "group by a.autono, a.slno, a.autono||a.slno, a.stkdrcr, c.docno, c.docdt,d.docnm, b.prefno, b.prefdt, i.slnm, i.gstno,i.district, a.itcd, a.rate, a.txblval,a.pageslno ) a, " + Environment.NewLine;

                sql += "( select a.autono, a.slno, a.autono||a.slno autoslno, listagg(b.batchno,',') within group (order by a.autono,a.slno) batchnos " + Environment.NewLine;
                sql += "from " + scm1 + ".t_batchdtl a, " + scm1 + ".t_batchmst b where a.autono=b.autono(+) group by a.autono, a.slno, a.autono||a.slno ) b, " + Environment.NewLine;

                sql += "(select listagg(gonm, ',') within group (order by gonm) tgonm,autono,slno " + Environment.NewLine;
                sql += "from (select distinct a.gocd, a.autono, c.gonm,a.slno " + Environment.NewLine;
                sql += "from " + scm1 + ".t_txndtl a, " + scm1 + ".t_txn b, " + scmf + ".m_godown c where a.autono = b.autono(+) and a.gocd = c.gocd(+) and a.stkdrcr = 'D' " + Environment.NewLine;
                sql += "group by a.gocd, a.autono, c.gonm,a.slno) " + Environment.NewLine;
                sql += "group by autono,slno) e, " + Environment.NewLine;

                sql += "(select listagg(gonm, ',') within group (order by gonm) fgonm,autono,slno " + Environment.NewLine;
                sql += "from (select distinct a.gocd, b.autono, c.gonm,a.slno " + Environment.NewLine;
                sql += "from " + scm1 + ".t_txndtl a, " + scm1 + ".t_txn b, " + scmf + ".m_godown c where a.autono = b.autono(+) and a.gocd = c.gocd(+) " + Environment.NewLine;
                //if (MSYSCNFG.STKINCLPINV == "Y")
                //{
                //    sql += " and a.stkdrcr in('C','0')" + Environment.NewLine;

                //}
                //else
                //{
                    sql += " and a.stkdrcr = 'C' " + Environment.NewLine;
                //}
                sql += " and a.stkdrcr = 'C' " + Environment.NewLine;
                sql += "group by a.gocd, b.autono, c.gonm,a.slno) " + Environment.NewLine;
                sql += "group by autono,slno) f, " + Environment.NewLine;

                sql += "(select listagg(baleno, ',') within group (order by baleno) baleno,autono " + Environment.NewLine;
                sql += "from (select distinct a.baleno, b.autono " + Environment.NewLine;
                sql += "from " + scm1 + ".t_txndtl a, " + scm1 + ".t_txn b where a.autono = b.autono(+) " + Environment.NewLine;
                sql += "group by a.baleno, b.autono) " + Environment.NewLine;
                sql += "group by autono) g, " + Environment.NewLine;


                sql += scm1 + ".m_sitem c, " + scmf + ".m_uom d " + Environment.NewLine;
                sql += "where a.autoslno=b.autoslno(+) and a.itcd=c.itcd(+) and c.uomcd=d.uomcd(+) and a.autono=e.autono(+) and a.slno=e.slno(+) and a.autono=f.autono(+) and a.slno=f.slno(+) and a.autono=g.autono(+) " + Environment.NewLine;
                if (itgrpcd.retStr() != "") sql += "and c.itgrpcd in(" + itgrpcd + ") " + Environment.NewLine;

                sql += "order by itnm, itcd, docdt, stkdrcr desc, autono ";
                DataTable tbl = MasterHelp.SQLquery(sql);

                //string autono = string.Join(",", (from DataRow dr in tbl.Rows where (dr => dr.Field<string>("batchnos").Equals("A"));

                //
                if (tbl.Rows.Count == 0)
                {
                    return RedirectToAction("NoRecords", "RPTViewer");
                }

                DataTable IR = new DataTable("mstrep");

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                string qdsp = "";
                //if (rateqntybag == "B") qdsp = "n,12";
                //qdsp = "n,12,4";
                qdsp = "n,12,2";

                //HC.RepStart(IR, 3);
                HC.RepStart(IR, 2);
                HC.GetPrintHeader(IR, "docdt", "string", "d,10:dd/mm/yyyy", "Doc Date");
                HC.GetPrintHeader(IR, "docno", "string", "c,16", "Doc No");
                HC.GetPrintHeader(IR, "prefno", "string", "c,16", "P Blno");
                HC.GetPrintHeader(IR, "slnm", "string", "c,40", "Particulars");
                HC.GetPrintHeader(IR, "rate", "double", "n,10,2", "Rate");
                HC.GetPrintHeader(IR, "pageslno", "string", "c,10", "page slno");
                HC.GetPrintHeader(IR, "qntyin", "double", qdsp, "Qnty (In)");
                if (VE.Checkbox1 == true)
                {
                    HC.GetPrintHeader(IR, "amtin", "double", "n,12,2", "Amt (In)");
                }
                HC.GetPrintHeader(IR, "qntyout", "double", qdsp, "Qnty (Out)");
                if (VE.Checkbox1 == true)
                {
                    HC.GetPrintHeader(IR, "amtout", "double", "n,12,2", "Amt (Out)");
                }
                HC.GetPrintHeader(IR, "balqnty", "double", qdsp, "Bal Qnty");
                //if (showbatch == true) HC.GetPrintHeader(IR, "batchno", "string", "c,30", "Batchnos");

                double iop = 0, idr = 0, icr = 0, icls = 0, idramt = 0, icramt = 0;
                double top = 0, tdr = 0, tcr = 0, tcls = 0, tdramt = 0, tcramt = 0, tblqty = 0;
                double dbqty = 0, dbamt = 0;

                top = 0; tdr = 0; tcr = 0; tcls = 0;

                Int32 rNo = 0;

                // Report begins
                i = 0; maxR = tbl.Rows.Count - 1;

                while (i <= maxR)
                {
                    chkval = tbl.Rows[i]["itcd"].ToString();
                    iop = 0; idr = 0; icr = 0; icls = 0; idramt = 0; icramt = 0;

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + tbl.Rows[i]["itcd1"].ToString() + "  " + " </span>" + tbl.Rows[i]["itstyle"].ToString();
                    IR.Rows[rNo]["Dammy"] = IR.Rows[rNo]["Dammy"] + " </span>" + " [" + tbl.Rows[i]["uomcd"] + "]";
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                    while (tbl.Rows[i]["itcd"].ToString() == chkval)
                    {
                        double bqnty = 0, bnos = 0, bval = 0, bamt = 0;
                        while (tbl.Rows[i]["itcd"].ToString() == chkval && Convert.ToDateTime(tbl.Rows[i]["docdt"]) < Convert.ToDateTime(fdt))
                        {
                            //if (rateqntybag == "B") dbqty = Convert.ToDouble(tbl.Rows[i]["nos"]);
                            dbqty = Convert.ToDouble(tbl.Rows[i]["qnty"]);

                            if (tbl.Rows[i]["stkdrcr"].ToString() == "D")
                            {
                                bnos = bnos + tbl.Rows[i]["nos"].retDbl();
                                bqnty = bqnty + tbl.Rows[i]["qnty"].retDbl();
                                iop = iop + dbqty;
                            }
                            else if (tbl.Rows[i]["stkdrcr"].ToString() == "C")
                            {
                                bnos = bnos - tbl.Rows[i]["nos"].retDbl();
                                bqnty = bqnty - tbl.Rows[i]["qnty"].retDbl();
                                iop = iop - dbqty;
                            }
                            i = i + 1;
                            if (i > maxR) break;
                        }
                        if (iop != 0)
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["docdt"] = fdt;
                            IR.Rows[rNo]["slnm"] = "Opening";
                            IR.Rows[rNo]["balqnty"] = iop;
                            if (iop < 0) icr = iop * -1;
                            else idr = iop;
                            icls = iop;
                        }
                        if (i > maxR)
                        {
                            break;
                        }
                        while (tbl.Rows[i]["itcd"].ToString() == chkval && Convert.ToDateTime(tbl.Rows[i]["docdt"]) <= Convert.ToDateTime(tdt))
                        {
                            //if (rateqntybag == "B") dbqty = Convert.ToDouble(tbl.Rows[i]["nos"]);
                            dbqty = tbl.Rows[i]["qnty"].retDbl();
                            dbamt = tbl.Rows[i]["txblval"].retDbl();

                            //string pdocno = tbl.Rows[i]["pblno"].ToString();
                            //if (pdocno == "") pdocno = tbl.Rows[i]["docno"].ToString();
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].ToString().Substring(0, 10);
                            IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].ToString();
                            IR.Rows[rNo]["prefno"] = tbl.Rows[i]["prefno"].ToString();
                            string gonm = tbl.Rows[i]["stkdrcr"].ToString() == "D" ? ("To Godown : " + tbl.Rows[i]["tgonm"].ToString()) : ("From Godown : " + tbl.Rows[i]["fgonm"].ToString());
                            IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"].retStr() == "" ? tbl.Rows[i]["docnm"] + " [" + gonm + "]" : tbl.Rows[i]["slnm"] + " [" + tbl.Rows[i]["gstno"] + " " + tbl.Rows[i]["district"] + "]";
                            //if (showbatch == true) IR.Rows[rNo]["batchno"] = tbl.Rows[i]["batchnos"];
                            IR.Rows[rNo]["rate"] = tbl.Rows[i]["rate"].retDbl();
                            IR.Rows[rNo]["pageslno"] = tbl.Rows[i]["pageslno"].ToString();
                            if (tbl.Rows[i]["stkdrcr"].ToString() == "D")
                            {
                                IR.Rows[rNo]["qntyin"] = dbqty;
                                if (VE.Checkbox1 == true)
                                {
                                    IR.Rows[rNo]["amtin"] = dbamt;
                                }
                                idr = idr + dbqty; idramt = idramt + dbamt;
                            }
                            else
                            {
                                IR.Rows[rNo]["qntyout"] = dbqty;
                                if (VE.Checkbox1 == true)
                                {
                                    IR.Rows[rNo]["amtout"] = dbamt;
                                }
                                icr = icr + dbqty; icramt = icramt + dbamt;
                            }
                            icls = idr - icr;
                            IR.Rows[rNo]["balqnty"] = icls;
                            tblqty = tblqty + icls;
                            i = i + 1;
                            if (i > maxR) break;
                        }

                        if (i > maxR) break;
                    }

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["dammy"] = "";
                    IR.Rows[rNo]["slnm"] = "Totals (" + tbl.Rows[i - 1]["styleno"] + tbl.Rows[i - 1]["itnm"] + ")";
                    IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                    IR.Rows[rNo]["qntyin"] = idr;
                    if (VE.Checkbox1 == true)
                    {
                        IR.Rows[rNo]["amtin"] = idramt;
                    }
                    IR.Rows[rNo]["qntyout"] = icr;
                    if (VE.Checkbox1 == true)
                    {
                        IR.Rows[rNo]["amtout"] = icramt;
                    }
                    //IR.Rows[rNo]["balqnty"] = icls;
                    //IR.Rows[rNo]["balqnty"] = tblqty;
                    IR.Rows[rNo]["balqnty"] = idr - icr;

                    top = top + iop;
                    tdr = tdr + idr; tdramt = tdramt + idramt;
                    tcr = tcr + icr; tcramt = tcramt + icramt;
                    tcls = tcls + icls;
                }

                // Create Blank line
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["dammy"] = " ";
                IR.Rows[rNo]["flag"] = " height:14px; ";

                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["dammy"] = "";
                IR.Rows[rNo]["slnm"] = "Grand Totals";
                IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                IR.Rows[rNo]["qntyin"] = tdr;
                if (VE.Checkbox1 == true)
                {
                    IR.Rows[rNo]["amtin"] = tdramt;
                }
                IR.Rows[rNo]["qntyout"] = tcr;
                if (VE.Checkbox1 == true)
                {
                    IR.Rows[rNo]["amtout"] = tcramt;
                }
                IR.Rows[rNo]["balqnty"] = tdr - tcr;

                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                string jobnm = DB.M_JOBMST.Find(VE.MENU_PARA)?.JOBNM;
                string pghdr1 = "";
                string repname = jobnm + "_Stk_Leg";
                pghdr1 = jobnm + " Stock Ledger from " + fdt + " to " + tdt;
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
