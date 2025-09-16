using System;
using System.Linq;
using System.Data;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;

namespace Improvar.Controllers
{
    public class Rep_OrdRegController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_OrdReg
        public ActionResult Rep_OrdReg()
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
                    ViewBag.formname = "Order Register";
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    VE.FDT = CommVar.FinStartDate(UNQSNO);
                    VE.TDT = CommVar.CurrDate(UNQSNO);
                    VE.TEXTBOX1 = CommVar.CurrDate(UNQSNO); // txnupto
                    VE.Checkbox1 = false; //show pending

                    string comcd = CommVar.Compcd(UNQSNO);
                    string location = CommVar.Loccd(UNQSNO);

                    VE.DropDown_list_BRAND = DropDownHelp.GetBrandcddforSelection();
                    VE.Brandnm = MasterHelp.ComboFill("brandcd", VE.DropDown_list_BRAND, 0, 1);

                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("D");
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);

                    VE.DropDown_list_AGSLCD = DropDownHelp.GetAgSlcdforSelection();
                    VE.Agslnm = MasterHelp.ComboFill("agslcd", VE.DropDown_list_AGSLCD, 0, 1);

                    VE.DropDown_list_SLMSLCD = DropDownHelp.GetSlmSlcdforSelection();
                    VE.Slmslnm = MasterHelp.ComboFill("slmslcd", VE.DropDown_list_SLMSLCD, 0, 1);


                    VE.DropDown_list3 = (from a in DB.M_COLOR select new DropDown_list3() { text = a.COLRNM, value = a.COLRCD }).ToList();//sizes
                    VE.TEXTBOX10 = MasterHelp.ComboFill("itcolor", VE.DropDown_list3, 0, 1);

                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);

                    VE.DropDown_list_DOCCD = DropDownHelp.GetDocCdforSelection("'SJBOR'");
                    VE.Docnm = MasterHelp.ComboFill("doccd", VE.DropDown_list_DOCCD, 0, 1);

                    VE.DropDown_list_LOCCD = DropDownHelp.DropDownLoccation();
                    VE.Locnm = MasterHelp.ComboFill("loccd", VE.DropDown_list_LOCCD, 1, 0);

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
        public ActionResult Rep_OrdReg(ReportViewinHtml VE, FormCollection FC)
        {
            string ModuleCode = Module.Module_Code;
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string slcd = "", agslcd = "", slmslcd = "", brandcd = "", fdt, tdt = "", colrcd = "", batchno = "", item = "", mtrlsupplby = "", seldoccd = "",loccd = "", pghdr2 = "";
                fdt = VE.FDT;
                tdt = VE.TDT;
                string ReportType = VE.TEXTBOX3, ShowReport = VE.TEXTBOX5;
                if (FC.AllKeys.Contains("agslcdvalue"))
                {
                    agslcd = FC["agslcdvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("slcdvalue"))
                {
                    slcd = FC["slcdvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("brandcdvalue"))
                {
                    brandcd = FC["brandcdvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("slmslcdvalue"))
                {
                    slmslcd = FC["slmslcdvalue"].ToString().retSqlformat();
                }
                //if (ReportType == "Details" || ReportType == "Summary")
                //{
                    if (FC.AllKeys.Contains("itcolorvalue"))
                    {
                        colrcd = FC["itcolorvalue"].ToString().retSqlformat();
                    }

                    if (FC.AllKeys.Contains("itcdvalue"))
                    {
                        item = FC["itcdvalue"].ToString().retSqlformat();
                    }
                //}
                if (FC.AllKeys.Contains("loccdvalue"))
                {
                    loccd = FC["loccdvalue"].ToString().retSqlformat();
                    pghdr2 += (pghdr2.retStr() == "" ? "" : "<br/>") + "Location :" + FC["loccdtext"].ToString();
                }

                if (FC.AllKeys.Contains("doccdvalue")) seldoccd = CommFunc.retSqlformat(FC["doccdvalue"].ToString());
                bool mergeloca = VE.Checkbox3;
                string txnupto = "";
                bool showasperpslip = false;
                if (VE.TEXTBOX2 == "PSLIP") showasperpslip = true;
                bool showcolor = false, ShowBatchNumber = false, ShowSLCol = false;
                if (ReportType == "Details" || ReportType == "Summary")
                {
                    showcolor = VE.Checkbox4;
                    ShowBatchNumber = VE.Checkbox5;
                    ShowSLCol = VE.Checkbox6;
                }
                string sort = "agslnm, agslcd ";
                if (ReportType == "Details") sort += ",slcd,slnm";
                if(ShowReport == "Dlvdt")
                {
                    sort += ",dlvdocdt,docno,STYLENO,itnm,itcd,rate";
                }
                if (ShowReport == "Prefdt")
                {
                    sort += ",prefdocdt,docno,STYLENO,itnm,itcd,rate";
                }
                sort += ",D_docdt,docno,STYLENO,itnm,itcd,rate";                              
                if (showcolor == true) sort += ",colrcd";
                if (ReportType == "Supper Summary") sort = "agslnm, agslcd,slnm,slcd";               

                DataTable tbl = Salesfunc.GetPendOrder(slcd, tdt, "", "", "", "", "SB", "", VE.Checkbox1, fdt, item, agslcd);
                tbl.DefaultView.Sort = sort;
                tbl = tbl.DefaultView.ToTable();
                if (tbl.Rows.Count == 0)
                {
                    return Content("No Record Found");
                }
                string sql = "";


                DataTable tbl1 = new DataTable();
                sql = "select a.pslipdocno,a.pslipdocdt,a.pslipautono,a.ordautono,a.ordslno,a.pslipqnty, " + Environment.NewLine;
                sql += "b.billdocno,b.billdocdt,b.billqnty,b.billautono,a.slno from " + Environment.NewLine;


                sql += "(select c.docno pslipdocno,c.docdt pslipdocdt,c.autono pslipautono,b.ordautono,b.ordslno,sum(b.qnty) pslipqnty,a.autono,b.slno,b.txnslno " + Environment.NewLine;
                sql += "from " + scm + ".t_txn a, " + scm + ".t_batchdtl b, " + scm + ".t_cntrl_hdr c, " + scm + ".m_doctype d " + Environment.NewLine;
                sql += "where a.autono = b.autono and b.autono = c.autono and c.doccd = d.doccd and " + Environment.NewLine;                
                sql += "c.compcd = '" + COM + "'  " + Environment.NewLine;
                if (loccd.retStr() != "")
                {
                    sql += "and c.loccd in (" + loccd + ") ";
                }
                sql += "and nvl(c.cancel, 'N')= 'N' and d.doctype in ('SPSLP') " + Environment.NewLine;
                sql += "group by c.docno,c.docdt,c.autono,b.ordautono,b.ordslno,a.autono,b.slno,b.txnslno)a, " + Environment.NewLine;

                sql += "(select f.docno billdocno,f.docdt billdocdt,f.autono billautono,sum(g.qnty) billqnty,g.autono,g.slno,g.txnslno,e.linkautono " + Environment.NewLine;
                sql += "from " + scm + ".t_txn_linkno e, " + scm + ".t_cntrl_hdr f, " + scm + ".t_batchdtl g " + Environment.NewLine;
                sql += "where e.linkautono=f.autono(+) and f.autono=g.autono(+) and f.compcd = '" + COM + "' and nvl(f.cancel, 'N')= 'N'  " + Environment.NewLine;
                if (loccd.retStr() != "")
                {
                    sql += "and f.loccd in (" + loccd + ") ";
                }
                sql += "group by f.docno ,f.docdt ,g.autono,g.slno,g.txnslno,e.linkautono,f.autono)b, " + Environment.NewLine;

                sql += "" + scm + ".t_cntrl_hdr c, " + scm + ".m_doctype d " + Environment.NewLine;

                sql += "where a.autono = b.linkautono(+) and a.slno = b.slno(+) and a.txnslno = b.txnslno(+) and a.autono = c.autono(+) and c.doccd = d.doccd(+) ";
                sql += "and c.compcd = '" + COM + "'  " + Environment.NewLine;
                if (loccd.retStr() != "")
                {
                    sql += "and c.loccd in (" + loccd + ") ";
                }
                sql += "and nvl(c.cancel, 'N')= 'N' and d.doctype in ('SPSLP') and b.billautono is not null " + Environment.NewLine;

                sql += "union all " + Environment.NewLine;
                sql += "select '' pslipdocno,c.docdt pslipdocdt,'' pslipautono,b.ordautono,b.ordslno,0 pslipqnty, " + Environment.NewLine;
                sql += "c.docno billdocno,c.docdt billdocdt,sum(b.qnty) billqnty,c.autono billautono,b.slno  " + Environment.NewLine;

                sql += "from " + scm + ".t_txn a, " + scm + ".t_batchdtl b, " + scm + ".t_cntrl_hdr c, " + scm + ".m_doctype d " + Environment.NewLine;
                sql += "where a.autono = b.autono and b.autono = c.autono and c.doccd = d.doccd and c.compcd = '" + COM + "'  " + Environment.NewLine;
                if (loccd.retStr() != "")
                {
                    sql += "and c.loccd in (" + loccd + ") ";
                }
                sql += "and nvl(c.cancel, 'N')= 'N' and d.doctype in ('SBEXP') " + Environment.NewLine;
                sql += "group by b.ordautono,b.ordslno,c.docno ,c.docdt ,c.autono ,b.slno " + Environment.NewLine;                
                tbl1 = MasterHelp.SQLquery(sql);

                Int32 maxR = 0, i = 0, rNo = 0;
                double tqnty = 0, tamt = 0, tbqnty = 0, tbamt = 0, tbalqnty = 0, tcancqnty = 0, tktqnty = 0, tbalktqnty = 0;
                string qtyfld = "ordqnty";
                if (VE.Checkbox1 == true) if (showasperpslip == true) qtyfld = "balaspslip"; else qtyfld = "balqnty";
                if (VE.Checkbox7 == true) qtyfld = "ordqnty";


                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                DataTable IR = new DataTable("mstrep");
                HC.RepStart(IR, 3);
                if (ReportType == "Details" || ReportType == "Summary")
                {
                    HC.GetPrintHeader(IR, "docdt", "string", "c,10", "DocDate");
                    if (ReportType != "Summary")
                    {
                        HC.GetPrintHeader(IR, "docno", "string", "c,15", "DocNo.");
                    }
                    if (ReportType == "Summary")
                    {
                        //HC.GetPrintHeader(IR, "slcd", "string", "c,8", "Party Cd");
                        HC.GetPrintHeader(IR, "slnm", "string", "c,45", "Party Name");
                        HC.GetPrintHeader(IR, "district", "string", "c,10", "Area");
                        HC.GetPrintHeader(IR, "docno", "string", "c,15", "DocNo.");
                    }
                    HC.GetPrintHeader(IR, "PREFDT", "string", "c,20", "Party Ref Date");
                    if (ReportType == "Summary")
                    {
                        HC.GetPrintHeader(IR, "DELVDT", "string", "c,20", "Delivary Date");
                    }
                    HC.GetPrintHeader(IR, "prefno", "string", "c,45", "Party Ref");
                    if (VE.Checkbox1 == true && VE.Checkbox7 == false)
                    {
                        HC.GetPrintHeader(IR, "pdesign", "string", "c,15", "Party Order Number");
                    }
                    HC.GetPrintHeader(IR, "itnm", "string", "c,15", "Item Name");
                    if (showcolor == true) HC.GetPrintHeader(IR, "colrnm", "string", "c,15", "Color");
                    //if (ReportType == "Details")
                    //{
                    //    if (showcolor == true) HC.GetPrintHeader(IR, "colrnm", "string", "c,50", "Color");
                    //}
                    if (VE.Checkbox1 == true && VE.Checkbox7 == false)
                    {
                        HC.GetPrintHeader(IR, "qnty", "double", "n,12,3:##,##,##,##0.000", "Balance Order Qnty");                        
                    }
                    else
                    {
                        HC.GetPrintHeader(IR, "qnty", "double", "n,12,3:##,##,##,##0.000", "Order Qnty");
                    }

                    HC.GetPrintHeader(IR, "rate", "double", "n,17,2:####,##,##,##0.00", "Rate");
                    HC.GetPrintHeader(IR, "amt", "double", "n,17,2:####,##,##,##0.00", "Order Value");                    
                    if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                    {
                        //HC.GetPrintHeader(IR, "cancqnty", "double", "n,12,3:##,##,##,##0.000", "Cancel Qnty");

                        if (VE.Checkbox7 == true)
                        {
                            HC.GetPrintHeader(IR, "billdocno", "string", "c,15", "Bill No");
                            HC.GetPrintHeader(IR, "billdocdt", "string", "c,10", "Bill Date");
                            HC.GetPrintHeader(IR, "billqnty", "double", "n,12,3", "Bill Quantity");
                        }
                        if (VE.Checkbox7 == true)
                        {
                            HC.GetPrintHeader(IR, "balordqnty", "double", "n,12,3", "Balance Order Quantity");
                        }
                        else
                        {
                            HC.GetPrintHeader(IR, "balordqnty", "double", "n,12,3:##,##,##,##0.000", "Total Qnty");                            
                        }
                        if (ReportType == "Details")
                        {
                            HC.GetPrintHeader(IR, "DELVDT", "string", "c,20", "Delivary Date");
                        }

                    }





                    i = 0; maxR = tbl.Rows.Count - 1;
                    string stragslcd = "";
                    while (i <= maxR)
                    {
                        stragslcd = tbl.Rows[i]["agslcd"].ToString();

                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["Dammy"] = "[ " + "Agent - " + stragslcd + "  " + " ]" + tbl.Rows[i]["agslnm"];
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";

                        double taqnty = 0, taamt = 0, tabqnty = 0, tabalqnty = 0, tacancqnty = 0, taktqnty = 0, tabalktqnty = 0;
                        while (tbl.Rows[i]["agslcd"].ToString() == stragslcd)
                        {
                            if (ReportType == "Details")
                            {
                                double tpqnty = 0, tpamt = 0, tpbqnty = 0, tpbalqnty = 0, tpcancqnty = 0, tpktqnty = 0, tpbalktqnty = 0;

                                string party = tbl.Rows[i]["slcd"].ToString();
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["Dammy"] = "[ " + "Party - " + party + "  " + " ]" + tbl.Rows[i]["slnm"];
                                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                                while (tbl.Rows[i]["agslcd"].ToString() == stragslcd && tbl.Rows[i]["slcd"].ToString() == party)
                                {
                                    string chk1 = tbl.Rows[i]["autono"].ToString();
                                    int count = 0;
                                    while (tbl.Rows[i]["agslcd"].ToString() == stragslcd && tbl.Rows[i]["slcd"].ToString() == party && tbl.Rows[i]["autono"].ToString() == chk1)
                                    {
                                        string ichk = tbl.Rows[i]["itcd"].ToString();

                                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;

                                        IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].retDateStr();
                                        IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].ToString() + (tbl.Rows[i]["cancel"].retStr() == "Y" ? " (Cancel)" : "");
                                        IR.Rows[rNo]["prefno"] = tbl.Rows[i]["prefno"].ToString();
                                        IR.Rows[rNo]["PREFDT"] = tbl.Rows[i]["PREFDT"].retDateStr();
                                        if (VE.Checkbox1 == true && VE.Checkbox7 == false)
                                        {
                                            IR.Rows[rNo]["pdesign"] = tbl.Rows[i]["pdesign"].ToString();
                                        }
                                        if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                                        {
                                            IR.Rows[rNo]["DELVDT"] = tbl.Rows[i]["DELVDT"].retDateStr();
                                        }
                                        IR.Rows[rNo]["itnm"] = tbl.Rows[i]["STYLENO"].ToString() + " "+tbl.Rows[i]["itnm"].ToString();

                                        double cnt = 0;
                                        while (tbl.Rows[i]["agslcd"].ToString() == stragslcd && tbl.Rows[i]["slcd"].ToString() == party && tbl.Rows[i]["autono"].ToString() == chk1 && tbl.Rows[i]["itcd"].ToString() == ichk)
                                        {


                                            double ctqnty = 0, ctamt = 0, ctcancqnty = 0;
                                            string chk = (showcolor == true ? tbl.Rows[i]["colrcd"].retStr() : tbl.Rows[i]["itcd"].retStr())
                                                      + (ShowBatchNumber == true ? tbl.Rows[i]["batchno"].retStr() : tbl.Rows[i]["itcd"].retStr()) +
                                                  tbl.Rows[i]["rate"].retDbl();

                                            count++;
                                            if (cnt != 0)
                                            {
                                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                            }
                                            if (showcolor == true) IR.Rows[rNo]["colrnm"] = tbl.Rows[i]["colrnm"];

                                            IR.Rows[rNo]["rate"] = tbl.Rows[i]["rate"];

                                            while (tbl.Rows[i]["agslcd"].ToString() == stragslcd && tbl.Rows[i]["slcd"].ToString() == party && tbl.Rows[i]["autono"].ToString() == chk1 && tbl.Rows[i]["itcd"].ToString() == ichk &&
                                                    (showcolor == true ? tbl.Rows[i]["colrcd"].retStr() : tbl.Rows[i]["itcd"].retStr())
                                                    + (ShowBatchNumber == true ? tbl.Rows[i]["batchno"].retStr() : tbl.Rows[i]["itcd"].retStr())
                                                + tbl.Rows[i]["rate"].retDbl() == chk)
                                            {
                                                ctqnty = ctqnty + tbl.Rows[i][qtyfld].retDbl();
                                                ctamt = ctamt + Math.Round((tbl.Rows[i][qtyfld].retDbl() * tbl.Rows[i]["rate"].retDbl()), 0);
                                                //ctcancqnty += tbl.Rows[i]["cancordqnty"].retDbl();

                                                i++;
                                                if (i > maxR) break;
                                            }

                                            IR.Rows[rNo]["qnty"] = ctqnty;
                                            //if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                                            //{
                                            //    IR.Rows[rNo]["cancqnty"] = ctcancqnty;
                                            //}
                                            IR.Rows[rNo]["amt"] = ctamt;

                                            tqnty += ctqnty;
                                            tamt += ctamt;
                                            taqnty += ctqnty;
                                            taamt += ctamt;
                                            tpqnty += ctqnty;
                                            tpamt += ctamt;

                                            tcancqnty += ctcancqnty;
                                            tacancqnty += ctcancqnty;
                                            tpcancqnty += ctcancqnty;
                                            cnt++;
                                            if (i > maxR) break;
                                        }
                                        double balqnty = (from DataRow a in tbl.Rows where a["autono"].retStr() == chk1 && a["itcd"].retStr() == ichk select a["ordqnty"].retDbl()).Sum().retDbl();
                                        double cancqnty = 0;// (from DataRow a in tbl.Rows where a["autono"].retStr() == chk1 && a["itcd"].retStr() == ichk select a["cancordqnty"].retDbl()).Sum().retDbl();
                                        balqnty -= cancqnty;

                                        if (VE.Checkbox7 == true)
                                        {
                                            string autono = tbl.Rows[i - 1]["AUTONO"].ToString();
                                            string itcd = tbl.Rows[i - 1]["ITCD"].ToString();
                                            string slno = tbl.Rows[i - 1]["slno"].ToString();

                                            string sqlcon = "ordautono = '" + autono + "'";
                                            if (ReportType == "Details") sqlcon += " and ordslno = " + slno + "";

                                            var tempbill = tbl1.Select(sqlcon, "billdocdt,billautono");
                                            if (tempbill != null && tempbill.Count() > 0)
                                            {
                                                DataTable selectedTable = tempbill.CopyToDataTable();
                                                int j = 0;
                                                double maxJ = selectedTable.Rows.Count - 1;
                                                int cnt1 = 0;
                                                while (j <= maxJ)
                                                {
                                                    double tbillqnty = 0;
                                                    string fld1 = ReportType == "Details" ? "slno" : "billautono";
                                                    string fldval1 = selectedTable.Rows[j]["billautono"].ToString() + selectedTable.Rows[j][fld1].ToString();
                                                    while (selectedTable.Rows[j]["billautono"].ToString() + selectedTable.Rows[j][fld1].ToString() == fldval1)
                                                    {
                                                        tbillqnty += selectedTable.Rows[j]["billQNTY"].retDbl();
                                                        tbqnty += selectedTable.Rows[j]["billQNTY"].retDbl();
                                                        tabqnty += selectedTable.Rows[j]["billQNTY"].retDbl();
                                                        tpbqnty += selectedTable.Rows[j]["billQNTY"].retDbl();
                                                        j++;
                                                        if (j > maxJ) break;
                                                    }
                                                    if (cnt1 != 0)
                                                    {
                                                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                                    }
                                                    IR.Rows[rNo]["billdocno"] = selectedTable.Rows[j - 1]["billDOCNO"];
                                                    IR.Rows[rNo]["billdocdt"] = selectedTable.Rows[j - 1]["billDOCDT"].retDateStr();
                                                    IR.Rows[rNo]["billqnty"] = tbillqnty;
                                                    balqnty -= tbillqnty;
                                                    if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                                                    {
                                                        IR.Rows[rNo]["balordqnty"] = balqnty;
                                                    }
                                                    cnt1++;
                                                    if (j > maxJ) break;

                                                }
                                            }
                                            else
                                            {
                                                if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                                                {
                                                    IR.Rows[rNo]["balordqnty"] = balqnty;
                                                }

                                            }

                                        }
                                        else
                                        {
                                            if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                                            {
                                                IR.Rows[rNo]["balordqnty"] = balqnty;

                                            }
                                        }
                                        tbalqnty += balqnty;
                                        tabalqnty += balqnty;
                                        tpbalqnty += balqnty;
                                        //i++;
                                        if (i > maxR) break;
                                    }
                                    if (i > maxR) break;
                                }

                                if (VE.Checkbox7 == true)
                                {
                                    IR.Rows[rNo]["flag"] = "border-bottom: 3px solid;";
                                }
                                //total of Party
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["Dammy"] = "Party Wise Total of [" + tbl.Rows[i - 1]["slnm"].ToString() + " ]";
                                IR.Rows[rNo]["qnty"] = tpqnty;
                                //if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                                //{
                                //    IR.Rows[rNo]["cancqnty"] = tpcancqnty;
                                //}
                                IR.Rows[rNo]["amt"] = tpamt;
                                if (VE.Checkbox7 == true)
                                {
                                    IR.Rows[rNo]["billqnty"] = tpbqnty;
                                }
                                if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                                {
                                    IR.Rows[rNo]["balordqnty"] = tpbalqnty;
                                }
                                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;~6";
                            }
                            else
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].retDateStr();

                                IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].ToString() + (tbl.Rows[i]["cancel"].retStr() == "Y" ? " (Cancel)" : "");
                                //IR.Rows[rNo]["slcd"] = tbl.Rows[i]["slcd"].ToString();
                                IR.Rows[rNo]["district"] = tbl.Rows[i]["district"].ToString();
                                IR.Rows[rNo]["prefno"] = tbl.Rows[i]["prefno"].ToString();
                                IR.Rows[rNo]["DELVDT"] = tbl.Rows[i]["DELVDT"].retDateStr();
                                IR.Rows[rNo]["PREFDT"] = tbl.Rows[i]["PREFDT"].retDateStr(); 
                                IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"].ToString();
                                IR.Rows[rNo]["rate"] = tbl.Rows[i]["rate"].retDbl();
                                IR.Rows[rNo]["itnm"] = tbl.Rows[i]["STYLENO"].ToString() + " " + tbl.Rows[i]["itnm"].ToString();
                                if (VE.Checkbox1 == true && VE.Checkbox7 == false)
                                {
                                    IR.Rows[rNo]["pdesign"] = tbl.Rows[i]["pdesign"].ToString();
                                }
                                if (showcolor == true) IR.Rows[rNo]["colrnm"] = tbl.Rows[i]["colrnm"];

                                string chk1 = tbl.Rows[i]["autono"].ToString();
                                double cqnty = 0, camt = 0, cancqnty = 0;
                                while (tbl.Rows[i]["agslcd"].ToString() == stragslcd && tbl.Rows[i]["autono"].ToString() == chk1)
                                {
                                    string ichk = tbl.Rows[i]["itcd"].ToString() + tbl.Rows[i]["rate"].retDbl();
                                    while (tbl.Rows[i]["agslcd"].ToString() == stragslcd && tbl.Rows[i]["autono"].ToString() == chk1 && tbl.Rows[i]["itcd"].ToString() + tbl.Rows[i]["rate"].retDbl() == ichk)
                                    {
                                        cqnty = cqnty + tbl.Rows[i][qtyfld].retDbl();
                                        camt = camt + Math.Round((tbl.Rows[i][qtyfld].retDbl() * tbl.Rows[i]["rate"].retDbl()), 0);
                                        //cancqnty += tbl.Rows[i]["cancordqnty"].retDbl();

                                        i++;
                                        if (i > maxR) break;
                                    }
                                    i = i - 1;
                                    i++;
                                    if (i > maxR) break;
                                }
                                IR.Rows[rNo]["qnty"] = cqnty;
                                //if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                                //{
                                //    IR.Rows[rNo]["cancqnty"] = cancqnty;
                                //}

                                IR.Rows[rNo]["amt"] = camt;
                                double balqnty = cqnty - cancqnty;

                                if (VE.Checkbox7 == true)
                                {
                                    string autono = tbl.Rows[i - 1]["AUTONO"].ToString();

                                    string sqlcon = "ordautono = '" + autono + "'";

                                    var tempbill = tbl1.Select(sqlcon, "billdocdt,billautono");
                                    if (tempbill != null && tempbill.Count() > 0)
                                    {
                                        DataTable selectedTable = tempbill.CopyToDataTable();
                                        int j = 0;
                                        double maxJ = selectedTable.Rows.Count - 1;
                                        int cnt = 0;
                                        while (j <= maxJ)
                                        {
                                            double tbillqnty = 0;
                                            string fldval1 = selectedTable.Rows[j]["billautono"].ToString();
                                            while (selectedTable.Rows[j]["billautono"].ToString() == fldval1)
                                            {
                                                tbillqnty += selectedTable.Rows[j]["billQNTY"].retDbl();
                                                tbqnty += selectedTable.Rows[j]["billQNTY"].retDbl();
                                                tabqnty += selectedTable.Rows[j]["billQNTY"].retDbl();
                                                j++;
                                                if (j > maxJ) break;
                                            }
                                            if (cnt != 0)
                                            {
                                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                            }
                                            IR.Rows[rNo]["billdocno"] = selectedTable.Rows[j - 1]["billDOCNO"];
                                            IR.Rows[rNo]["billdocdt"] = selectedTable.Rows[j - 1]["billDOCDT"].retDateStr();
                                            IR.Rows[rNo]["billqnty"] = tbillqnty;
                                            balqnty -= tbillqnty;
                                            if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                                            {
                                                IR.Rows[rNo]["balordqnty"] = balqnty;
                                            }
                                            cnt++;
                                            if (j > maxJ) break;

                                        }
                                    }
                                    else
                                    {
                                        if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                                        {
                                            IR.Rows[rNo]["balordqnty"] = balqnty;
                                        }
                                    }


                                    IR.Rows[rNo]["flag"] = "border-bottom: 3px solid;";
                                }
                                else
                                {
                                    if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                                    {
                                        IR.Rows[rNo]["balordqnty"] = balqnty;
                                    }
                                }
                                tbalqnty += balqnty;
                                tabalqnty += balqnty;

                                tqnty += cqnty;
                                tamt += camt;
                                taqnty += cqnty;
                                taamt += camt;

                                tcancqnty += cancqnty;
                                tacancqnty += cancqnty;

                            }


                            if (i > maxR) break;
                        }
                        //total of agent

                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["Dammy"] = "Agent Wise Total of [" + tbl.Rows[i - 1]["agslnm"].ToString() + " ]";
                        IR.Rows[rNo]["qnty"] = taqnty;
                        //if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                        //{
                        //    IR.Rows[rNo]["cancqnty"] = tacancqnty;
                        //}
                        IR.Rows[rNo]["amt"] = taamt;
                        if (VE.Checkbox7 == true)
                        {
                            IR.Rows[rNo]["billqnty"] = tabqnty;
                        }
                        if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                        {
                            IR.Rows[rNo]["balordqnty"] = tabalqnty;
                        }
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;~6";

                        if (i > maxR) break;
                    }

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["Dammy"] = "Grand Totals";
                    IR.Rows[rNo]["qnty"] = tqnty;
                    //if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                    //{
                    //    IR.Rows[rNo]["cancqnty"] = tcancqnty;
                    //}
                    IR.Rows[rNo]["amt"] = tamt;
                    if (VE.Checkbox7 == true)
                    {
                        IR.Rows[rNo]["billqnty"] = tbqnty;
                    }
                    if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                    {
                        IR.Rows[rNo]["balordqnty"] = tbalqnty;
                    }

                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;~2";
                }
                else
                {
                    HC.GetPrintHeader(IR, "slcd", "string", "c,8", "Party Cd");
                    HC.GetPrintHeader(IR, "slnm", "string", "c,45", "Party Name");
                    HC.GetPrintHeader(IR, "district", "string", "c,10", "Area");

                    if (VE.Checkbox1 == true && VE.Checkbox7 == false)
                    {
                        HC.GetPrintHeader(IR, "qnty", "double", "n,12,3", "Balance Order;Quantity");

                    }
                    else
                    {                        
                        HC.GetPrintHeader(IR, "qnty", "double", "n,12,3:##,##,##,##0.000", "Order;Qnty");
                        //HC.GetPrintHeader(IR, "cancqnty", "double", "n,12,3:##,##,##,##0.000", "Cancel;Qnty");
                        if (VE.Checkbox7 == true)
                        {
                            HC.GetPrintHeader(IR, "billqnty", "double", "n,12,3", "Bill;Quantity");
                        }
                        if (VE.Checkbox7 == true)
                        {
                            HC.GetPrintHeader(IR, "balordqnty", "double", "n,12,3", "Balance Order;Quantity");
                        }
                        else
                        {
                            HC.GetPrintHeader(IR, "balordqnty", "double", "n,12,3:##,##,##,##0.000", "Total;Qnty");
                        }
                    }


                    i = 0; maxR = tbl.Rows.Count - 1;
                    string stragslcd = "";
                    while (i <= maxR)
                    {
                        stragslcd = tbl.Rows[i]["agslcd"].ToString();

                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["Dammy"] = "[ " + "Agent - " + stragslcd + "  " + " ]" + tbl.Rows[i]["agslnm"];
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";

                        double taqnty = 0, taamt = 0, tabqnty = 0, tabamt = 0, tabalqnty = 0, tacancqnty = 0;
                        while (tbl.Rows[i]["agslcd"].ToString() == stragslcd)
                        {
                            double ordqnty = 0, billqnty = 0, billamt = 0, ordval = 0, cancordqnty = 0;
                            string fldval = tbl.Rows[i]["slcd"].ToString();
                            while (tbl.Rows[i]["slcd"].ToString() == fldval)
                            {
                                string autono = tbl.Rows[i]["autono"].ToString();
                                if (VE.Checkbox7 == true)
                                {
                                    billqnty += (from DataRow a in tbl1.Rows where a["ordautono"].retStr() == autono select a["billQNTY"].retDbl()).Sum();
                                    //billamt += (from DataRow a in tbl1.Rows where a["ordautono"].retStr() == autono select a["netamt"].retDbl()).Sum();

                                }
                                while (tbl.Rows[i]["slcd"].ToString() == fldval && tbl.Rows[i]["autono"].ToString() == autono)
                                {
                                    ordqnty += tbl.Rows[i][qtyfld].retDbl();
                                    ordval += Math.Round((tbl.Rows[i][qtyfld].retDbl() * tbl.Rows[i]["rate"].retDbl()), 2);
                                    tqnty += tbl.Rows[i][qtyfld].retDbl();
                                    tamt += Math.Round((tbl.Rows[i][qtyfld].retDbl() * tbl.Rows[i]["rate"].retDbl()), 2);
                                    taqnty += tbl.Rows[i][qtyfld].retDbl();
                                    taamt += Math.Round((tbl.Rows[i][qtyfld].retDbl() * tbl.Rows[i]["rate"].retDbl()), 2);
                                    //cancordqnty += tbl.Rows[i]["cancordqnty"].retDbl();
                                    //tacancqnty += tbl.Rows[i]["cancordqnty"].retDbl();
                                    //tcancqnty += tbl.Rows[i]["cancordqnty"].retDbl();

                                    i++;
                                    if (i > maxR) break;
                                }
                                if (i > maxR) break;
                            }
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["slcd"] = tbl.Rows[i - 1]["slcd"].ToString();
                            IR.Rows[rNo]["district"] = tbl.Rows[i - 1]["district"].ToString();
                            IR.Rows[rNo]["slnm"] = tbl.Rows[i - 1]["slnm"].ToString();
                            IR.Rows[rNo]["qnty"] = ordqnty;
                            //if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                            //{
                            //    IR.Rows[rNo]["cancqnty"] = cancordqnty;
                            //}
                            if (VE.Checkbox7 == true)
                            {
                                IR.Rows[rNo]["billqnty"] = billqnty;
                            }
                            if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                            {
                                IR.Rows[rNo]["balordqnty"] = (ordqnty - billqnty - cancordqnty);
                            }
                            tbqnty += billqnty;
                            tbamt += billamt;
                            tbalqnty += (ordqnty - billqnty - cancordqnty);
                            tabqnty += billqnty;
                            tabamt += billamt;
                            tabalqnty += (ordqnty - billqnty - cancordqnty);
                            if (i > maxR) break;
                        }
                        //total of agent

                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["Dammy"] = "Agent Wise Total of [" + tbl.Rows[i - 1]["agslnm"].ToString() + " ]";
                        IR.Rows[rNo]["qnty"] = taqnty;
                        //if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                        //{
                        //    IR.Rows[rNo]["cancqnty"] = tacancqnty;
                        //}
                        if (VE.Checkbox7 == true)
                        {
                            IR.Rows[rNo]["billqnty"] = tabqnty;
                        }
                        if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                        {
                            IR.Rows[rNo]["balordqnty"] = tabalqnty;
                        }
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;~4";

                        if (i > maxR) break;
                    }

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["Dammy"] = "Grand Totals";
                    IR.Rows[rNo]["qnty"] = tqnty;
                    //if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                    //{
                    //    IR.Rows[rNo]["cancqnty"] = tcancqnty;
                    //}
                    if (VE.Checkbox7 == true)
                    {
                        IR.Rows[rNo]["billqnty"] = tbqnty;
                    }
                    if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                    {
                        IR.Rows[rNo]["balordqnty"] = tbalqnty;
                    }
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;~2";
                }
                string pghdr1 = "", repname = CommFunc.retRepname("Rep_OrdReg");
                if (VE.Checkbox1 == true && showasperpslip == true) pghdr1 = "Pending Order (Packingslip) Register " + ReportType + " from " + fdt + " to " + tdt + " (" + txnupto + ")";
                else if (VE.Checkbox1 == true && showasperpslip == false) pghdr1 = "Pending Order (Bill) Register " + ReportType + " from " + fdt + " to " + tdt + " (" + txnupto + ")";
                else pghdr1 = "Order Register " + ReportType + " from " + fdt + " to " + tdt;

                PV = HC.ShowReport(IR, repname, pghdr1, pghdr2, true, true, "P", false);
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