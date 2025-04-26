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

                    //VE.DropDown_list_BATCHNO = (from a in DB.T_JOBORD_DTL select new DropDown_list_BATCHNO() { text = a.BATCHNO, value = a.BATCHNO }).Distinct().ToList();
                    //VE.Batchno = MasterHelp.ComboFill("batchno", VE.DropDown_list_BATCHNO, 0, 1);

                    VE.DropDown_list3 = (from a in DB.M_COLOR select new DropDown_list3() { text = a.COLRNM, value = a.COLRCD }).ToList();//sizes
                    VE.TEXTBOX10 = MasterHelp.ComboFill("itcolor", VE.DropDown_list3, 0, 1);

                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);

                    //VE.MTRLSUPPLBY_TYPE = MasterHelp.MTRLSUPPLBY_TYPE();
                    //VE.TEXTBOX4 = MasterHelp.ComboFill("MTRLSUPPLBY", VE.MTRLSUPPLBY_TYPE, 0, 1);

                    VE.DropDown_list_DOCCD = DropDownHelp.GetDocCdforSelection("'SJBOR'");
                    VE.Docnm = MasterHelp.ComboFill("doccd", VE.DropDown_list_DOCCD, 0, 1);

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
                string slcd = "", agslcd = "", slmslcd = "", brandcd = "", fdt, tdt = "", colrcd = "", batchno = "", item = "", mtrlsupplby = "", seldoccd = "";
                fdt = VE.FDT;
                tdt = VE.TDT;
                string ReportType = VE.TEXTBOX3;
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
                if (ReportType == "Details")
                {
                    if (FC.AllKeys.Contains("itcolorvalue"))
                    {
                        colrcd = FC["itcolorvalue"].ToString().retSqlformat();
                    }

                    if (FC.AllKeys.Contains("batchnovalue"))
                    {
                        batchno = FC["batchnovalue"].ToString().retSqlformat();
                    }
                    if (FC.AllKeys.Contains("itcdvalue"))
                    {
                        item = FC["itcdvalue"].ToString().retSqlformat();
                    }
                }
                if (FC.AllKeys.Contains("MTRLSUPPLBYvalue"))
                {
                    mtrlsupplby = FC["MTRLSUPPLBYvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("doccdvalue")) seldoccd = CommFunc.retSqlformat(FC["doccdvalue"].ToString());
                bool mergeloca = VE.Checkbox3;
                string txnupto = "";
                bool showasperpslip = false;
                if (VE.TEXTBOX2 == "PSLIP") showasperpslip = true;
                bool showcolor = false, ShowBatchNumber = false, ShowSLCol = false;
                if (ReportType == "Details")
                {
                    showcolor = VE.Checkbox4;
                    ShowBatchNumber = VE.Checkbox5;
                    ShowSLCol = VE.Checkbox6;
                }
                string sort = "agslnm, agslcd ";
                if (ReportType == "Details") sort += ",slcd,slnm";
                sort += ",docdt, docno,  itnm, itcd,rate";
                if (showcolor == true) sort += ",colrcd";
                if (ShowBatchNumber == true) sort += ",batchno";
                if (ReportType == "Details") sort += ",dia";
                if (ShowBatchNumber == true) sort += ",ll";
                if (ReportType == "Supper Summary") sort = "agslnm, agslcd,slnm,slcd,MTRLSUPPLBY,jobnm";

                DataTable tbl = Salesfunc.GetPendOrder(slcd,tdt);
                tbl.DefaultView.Sort = sort;
                tbl = tbl.DefaultView.ToTable();
                if (tbl.Rows.Count == 0)
                {
                    return Content("No Record Found");
                }
                string sql = "";

                sql = "select a.autono,b.itcd,d.jobordautono,sum(a.qnty) qnty " + Environment.NewLine;
                sql += System.Environment.NewLine + "from " + scm + ".t_batchdtl a," + scm + ".t_batchmst b," + scm + ".t_cntrl_hdr c," + scm + ".T_PROGMAST d," + scm + ".m_doctype e ";
                sql += System.Environment.NewLine + "where a.batchautono=b.batchautono(+) and a.batchslno=b.batchslno(+) and a.autono=c.autono(+)and b.progautono=d.autono(+) and b.progslno=d.slno(+) and c.doccd=e.doccd(+) and e.doctype in ('YRLOTR') ";
                sql += "group by a.autono,b.itcd,d.jobordautono " + Environment.NewLine;
                DataTable tbl_PendProg = MasterHelp.SQLquery(sql);

                DataTable tbl1 = new DataTable();
                sql = "select c.docno,c.docdt,c.autono,b.ordautono,b.itcd,sum(b.qnty)qnty,sum(round(nvl(b.amt,0)+(nvl(b.igstamt,0)+nvl(b.cgstamt,0)+nvl(b.sgstamt,0)),2))netamt from " + scm + ".t_txn a, " + scm + ".t_txndtl b, " + scm + ".t_cntrl_hdr c, " + scm + ".m_doctype d " + Environment.NewLine;
                sql += "where a.autono = b.autono and b.autono = c.autono and c.doccd = d.doccd and c.compcd = '" + COM + "' and c.loccd = '" + LOC + "' " + Environment.NewLine;
                sql += "and nvl(c.cancel, 'N')= 'N' and d.doctype in ('SBFAB','SOTH','SRET','PROF','JBFAB','RPFAB','SBEXP','JRET') " + Environment.NewLine;
                sql += "group by c.docno,c.docdt,c.autono,b.ordautono,b.itcd " + Environment.NewLine;
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
                    HC.GetPrintHeader(IR, "docno", "string", "c,15", "DocNo.");
                    if (ReportType == "Summary")
                    {
                        HC.GetPrintHeader(IR, "slcd", "string", "c,8", "Party Cd");
                        HC.GetPrintHeader(IR, "slnm", "string", "c,45", "Party Name");
                        HC.GetPrintHeader(IR, "district", "string", "c,20", "Area");
                    }
                    HC.GetPrintHeader(IR, "prefno", "string", "c,20", "Party Ref");
                    HC.GetPrintHeader(IR, "MTRLSUPPLBY", "string", "c,20", "Order Type");
                    HC.GetPrintHeader(IR, "jobnm", "string", "c,20", "Nature Of Order");
                    if (ReportType == "Details")
                    {
                        HC.GetPrintHeader(IR, "itnm", "string", "c,15", "Item Name");
                        if (showcolor == true) HC.GetPrintHeader(IR, "colrnm", "string", "c,50", "Color");
                        if (ShowBatchNumber == true) HC.GetPrintHeader(IR, "BATCHNO", "string", "c,45", "Batch Number");
                        HC.GetPrintHeader(IR, "dia", "string", "c,25", "Dia");
                        HC.GetPrintHeader(IR, "GSM", "string", "c,25", "GSM");
                        if (ShowSLCol == true) HC.GetPrintHeader(IR, "ll", "string", "c,25", "SL");
                    }
                    if (VE.Checkbox1 == true && VE.Checkbox7 == false)
                    {
                        HC.GetPrintHeader(IR, "qnty", "double", "n,12,3:##,##,##,##0.000", "Balance Order Qnty");
                    }
                    else
                    {
                        HC.GetPrintHeader(IR, "qnty", "double", "n,12,3:##,##,##,##0.000", "Order Qnty");
                    }
                    if (ReportType == "Details" && VE.Checkbox8 == true)
                    {
                        HC.GetPrintHeader(IR, "ktqnty", "double", "n,12,3:##,##,##,##0.000", "Knitted Qnty");
                        HC.GetPrintHeader(IR, "balktqnty", "double", "n,12,3:##,##,##,##0.000", "Balance to Knitted");
                    }
                    HC.GetPrintHeader(IR, "rate", "double", "n,17,2:####,##,##,##0.00", "Rate");
                    HC.GetPrintHeader(IR, "amt", "double", "n,17,2:####,##,##,##0.00", "Order Value");
                    if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                    {
                        HC.GetPrintHeader(IR, "cancqnty", "double", "n,12,3:##,##,##,##0.000", "Cancel Qnty");

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
                                        //if (count == 0)
                                        //{
                                            IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].ToString().Remove(10);

                                            IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].ToString() + (tbl.Rows[i]["cancel"].retStr() == "Y" ? " (Cancel)" : "");
                                            //IR.Rows[rNo]["slcd"] = tbl.Rows[i]["slcd"].ToString();
                                            //IR.Rows[rNo]["district"] = tbl.Rows[i]["district"].ToString();
                                            IR.Rows[rNo]["prefno"] = tbl.Rows[i]["prefno"].ToString();

                                            //IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"].ToString();
                                            //IR.Rows[rNo]["MTRLSUPPLBY"] = Salesfunc.GetMTRLSUPPLBY(tbl.Rows[i]["MTRLSUPPLBY"].retStr());
                                            IR.Rows[rNo]["jobnm"] = tbl.Rows[i]["jobnm"].ToString();
                                        //}

                                        IR.Rows[rNo]["itnm"] = tbl.Rows[i]["itnm"].ToString() + " [" + tbl.Rows[i]["itcd"].ToString() + "]";
                                        //if (showcolor == true) IR.Rows[rNo]["colrnm"] = tbl.Rows[i]["colrnm"];
                                        //if (ShowBatchNumber == true) IR.Rows[rNo]["BATCHNO"] = tbl.Rows[i]["BATCHNO"];
                                        //IR.Rows[rNo]["dia"] = tbl.Rows[i]["dia"];
                                        //IR.Rows[rNo]["gsm"] = tbl.Rows[i]["gsm"];
                                        //IR.Rows[rNo]["rate"] = tbl.Rows[i]["rate"];

                                        //if (ShowSLCol == true) IR.Rows[rNo]["ll"] = tbl.Rows[i]["ll"];

                                        double cnt = 0;
                                        while (tbl.Rows[i]["agslcd"].ToString() == stragslcd && tbl.Rows[i]["slcd"].ToString() == party && tbl.Rows[i]["autono"].ToString() == chk1 && tbl.Rows[i]["itcd"].ToString() == ichk)
                                        {


                                            double ctqnty = 0, ctamt = 0, ctcancqnty = 0;
                                            string gsm = "";
                                            string chk = tbl.Rows[i]["dia"].retStr()
                                                      + (showcolor == true ? tbl.Rows[i]["colrcd"].retStr() : tbl.Rows[i]["itcd"].retStr())
                                                      + (ShowBatchNumber == true ? tbl.Rows[i]["batchno"].retStr() : tbl.Rows[i]["itcd"].retStr())
                                                  + (ShowSLCol == true ? tbl.Rows[i]["ll"].retStr() : tbl.Rows[i]["itcd"].retStr()) + tbl.Rows[i]["rate"].retDbl()+ tbl.Rows[i]["gsm"].retStr();

                                            count++;
                                            if (cnt != 0)
                                            {
                                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                            }
                                            if (showcolor == true) IR.Rows[rNo]["colrnm"] = tbl.Rows[i]["colrnm"];
                                            if (ShowBatchNumber == true) IR.Rows[rNo]["BATCHNO"] = tbl.Rows[i]["BATCHNO"];
                                            IR.Rows[rNo]["dia"] = tbl.Rows[i]["dia"];
                                            IR.Rows[rNo]["gsm"] = tbl.Rows[i]["gsm"];
                                            IR.Rows[rNo]["rate"] = tbl.Rows[i]["rate"];
                                            if (ShowSLCol == true) IR.Rows[rNo]["ll"] = tbl.Rows[i]["ll"];

                                            while (tbl.Rows[i]["agslcd"].ToString() == stragslcd && tbl.Rows[i]["slcd"].ToString() == party && tbl.Rows[i]["autono"].ToString() == chk1 && tbl.Rows[i]["itcd"].ToString() == ichk &&
                                                    tbl.Rows[i]["dia"].retStr()
                                                    + (showcolor == true ? tbl.Rows[i]["colrcd"].retStr() : tbl.Rows[i]["itcd"].retStr())
                                                    + (ShowBatchNumber == true ? tbl.Rows[i]["batchno"].retStr() : tbl.Rows[i]["itcd"].retStr())
                                                + (ShowSLCol == true ? tbl.Rows[i]["ll"].retStr() : tbl.Rows[i]["itcd"].retStr()) + tbl.Rows[i]["rate"].retDbl() + tbl.Rows[i]["gsm"].retStr() == chk)
                                            {
                                                ctqnty = ctqnty + tbl.Rows[i][qtyfld].retDbl();
                                                ctamt = ctamt + Math.Round((tbl.Rows[i][qtyfld].retDbl() * tbl.Rows[i]["rate"].retDbl()), 0);
                                                //gsm += tbl.Rows[i]["gsm"].retDbl();
                                                ctcancqnty += tbl.Rows[i]["cancordqnty"].retDbl();

                                                i++;
                                                if (i > maxR) break;
                                            }
                                            //i = i - 1;

                                            IR.Rows[rNo]["qnty"] = ctqnty;
                                            if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                                            {
                                                IR.Rows[rNo]["cancqnty"] = ctcancqnty;
                                            }
                                            IR.Rows[rNo]["amt"] = ctamt;
                                            //IR.Rows[rNo]["gsm"] = gsm;



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
                                        double cancqnty = (from DataRow a in tbl.Rows where a["autono"].retStr() == chk1 && a["itcd"].retStr() == ichk select a["cancordqnty"].retDbl()).Sum().retDbl();
                                        balqnty -= cancqnty;
                                        double ktqnty = 0;
                                        if (VE.Checkbox8 == true)
                                        {
                                            ktqnty = (from DataRow dr in tbl_PendProg.Rows where dr["jobordautono"].retStr() == chk1 && dr["itcd"].retStr() == ichk select dr["qnty"].retDbl()).Sum();
                                        }
                                        if (VE.Checkbox7 == true)
                                        {
                                            string autono = tbl.Rows[i - 1]["AUTONO"].ToString();
                                            string itcd = tbl.Rows[i - 1]["ITCD"].ToString();

                                            string sqlcon = "ordautono = '" + autono + "'";
                                            if (ReportType == "Details") sqlcon += "and ITCD = '" + itcd + "'";

                                            var tempbill = tbl1.Select(sqlcon, "autono,docdt");
                                            if (tempbill != null && tempbill.Count() > 0)
                                            {
                                                DataTable selectedTable = tempbill.CopyToDataTable();
                                                int j = 0;
                                                double maxJ = selectedTable.Rows.Count - 1;
                                                int cnt1 = 0;
                                                while (j <= maxJ)
                                                {
                                                    double tbillqnty = 0;
                                                    string fld1 = ReportType == "Details" ? "itcd" : "autono";
                                                    string fldval1 = selectedTable.Rows[j]["autono"].ToString() + selectedTable.Rows[j][fld1].ToString();
                                                    while (selectedTable.Rows[j]["autono"].ToString() + selectedTable.Rows[j][fld1].ToString() == fldval1)
                                                    {
                                                        tbillqnty += selectedTable.Rows[j]["QNTY"].retDbl();
                                                        tbqnty += selectedTable.Rows[j]["QNTY"].retDbl();
                                                        tabqnty += selectedTable.Rows[j]["QNTY"].retDbl();
                                                        tpbqnty += selectedTable.Rows[j]["QNTY"].retDbl();
                                                        j++;
                                                        if (j > maxJ) break;
                                                    }
                                                    if (cnt1 != 0)
                                                    {
                                                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                                    }
                                                    IR.Rows[rNo]["billdocno"] = selectedTable.Rows[j - 1]["DOCNO"];
                                                    IR.Rows[rNo]["billdocdt"] = selectedTable.Rows[j - 1]["DOCDT"].retDateStr();
                                                    IR.Rows[rNo]["billqnty"] = tbillqnty;
                                                    balqnty -= tbillqnty;
                                                    if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                                                    {
                                                        IR.Rows[rNo]["balordqnty"] = balqnty;
                                                    }
                                                    if (VE.Checkbox8 == true)
                                                    {
                                                        IR.Rows[rNo]["ktqnty"] = ktqnty;
                                                        IR.Rows[rNo]["balktqnty"] = balqnty - ktqnty;
                                                        tpktqnty += ktqnty;
                                                        tpbalktqnty += (balqnty - ktqnty);

                                                        taktqnty += ktqnty;
                                                        tabalktqnty += (balqnty - ktqnty);

                                                        tktqnty += ktqnty;
                                                        tbalktqnty += (balqnty - ktqnty);
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
                                                if (VE.Checkbox8 == true)
                                                {
                                                    IR.Rows[rNo]["ktqnty"] = ktqnty;
                                                    IR.Rows[rNo]["balktqnty"] = balqnty - ktqnty;
                                                    tpktqnty += ktqnty;
                                                    tpbalktqnty += (balqnty - ktqnty);

                                                    taktqnty += ktqnty;
                                                    tabalktqnty += (balqnty - ktqnty);

                                                    tktqnty += ktqnty;
                                                    tbalktqnty += (balqnty - ktqnty);
                                                }

                                            }

                                        }
                                        else
                                        {
                                            if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                                            {
                                                IR.Rows[rNo]["balordqnty"] = balqnty;

                                            }
                                            if (VE.Checkbox8 == true)
                                            {
                                                IR.Rows[rNo]["ktqnty"] = ktqnty;
                                                IR.Rows[rNo]["balktqnty"] = balqnty - ktqnty;
                                                tpktqnty += ktqnty;
                                                tpbalktqnty += (balqnty - ktqnty);

                                                taktqnty += ktqnty;
                                                tabalktqnty += (balqnty - ktqnty);
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
                                if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                                {
                                    IR.Rows[rNo]["cancqnty"] = tpcancqnty;
                                }
                                IR.Rows[rNo]["amt"] = tpamt;
                                if (VE.Checkbox7 == true)
                                {
                                    IR.Rows[rNo]["billqnty"] = tpbqnty;
                                }
                                if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                                {
                                    IR.Rows[rNo]["balordqnty"] = tpbalqnty;
                                }
                                if (VE.Checkbox8 == true)
                                {
                                    IR.Rows[rNo]["ktqnty"] = tpktqnty;
                                    IR.Rows[rNo]["balktqnty"] = tpbalktqnty;
                                }
                                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;~6";
                            }
                            else
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].ToString().Remove(10);

                                IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].ToString() + (tbl.Rows[i]["cancel"].retStr() == "Y" ? " (Cancel)" : "");
                                IR.Rows[rNo]["slcd"] = tbl.Rows[i]["slcd"].ToString();
                                IR.Rows[rNo]["district"] = tbl.Rows[i]["district"].ToString();
                                IR.Rows[rNo]["prefno"] = tbl.Rows[i]["prefno"].ToString();

                                IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"].ToString();
                                //IR.Rows[rNo]["MTRLSUPPLBY"] = Salesfunc.GetMTRLSUPPLBY(tbl.Rows[i]["MTRLSUPPLBY"].retStr());
                                IR.Rows[rNo]["rate"] = tbl.Rows[i]["rate"].retDbl();
                                IR.Rows[rNo]["jobnm"] = tbl.Rows[i]["jobnm"].ToString();

                                string chk1 = tbl.Rows[i]["autono"].ToString();
                                double cqnty = 0, camt = 0, cancqnty = 0;
                                while (tbl.Rows[i]["agslcd"].ToString() == stragslcd && tbl.Rows[i]["autono"].ToString() == chk1)
                                {
                                    string ichk = tbl.Rows[i]["itcd"].ToString() + tbl.Rows[i]["rate"].retDbl();
                                    while (tbl.Rows[i]["agslcd"].ToString() == stragslcd && tbl.Rows[i]["autono"].ToString() == chk1 && tbl.Rows[i]["itcd"].ToString() + tbl.Rows[i]["rate"].retDbl() == ichk)
                                    {
                                        cqnty = cqnty + tbl.Rows[i][qtyfld].retDbl();
                                        camt = camt + Math.Round((tbl.Rows[i][qtyfld].retDbl() * tbl.Rows[i]["rate"].retDbl()), 0);
                                        cancqnty += tbl.Rows[i]["cancordqnty"].retDbl();

                                        i++;
                                        if (i > maxR) break;
                                    }
                                    i = i - 1;
                                    i++;
                                    if (i > maxR) break;
                                }
                                IR.Rows[rNo]["qnty"] = cqnty;
                                if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                                {
                                    IR.Rows[rNo]["cancqnty"] = cancqnty;
                                }

                                IR.Rows[rNo]["amt"] = camt;
                                double balqnty = cqnty - cancqnty;

                                if (VE.Checkbox7 == true)
                                {
                                    string autono = tbl.Rows[i - 1]["AUTONO"].ToString();

                                    string sqlcon = "ordautono = '" + autono + "'";

                                    var tempbill = tbl1.Select(sqlcon, "autono,docdt");
                                    if (tempbill != null && tempbill.Count() > 0)
                                    {
                                        DataTable selectedTable = tempbill.CopyToDataTable();
                                        int j = 0;
                                        double maxJ = selectedTable.Rows.Count - 1;
                                        int cnt = 0;
                                        while (j <= maxJ)
                                        {
                                            double tbillqnty = 0;
                                            string fldval1 = selectedTable.Rows[j]["autono"].ToString();
                                            while (selectedTable.Rows[j]["autono"].ToString() == fldval1)
                                            {
                                                tbillqnty += selectedTable.Rows[j]["QNTY"].retDbl();
                                                tbqnty += selectedTable.Rows[j]["QNTY"].retDbl();
                                                tabqnty += selectedTable.Rows[j]["QNTY"].retDbl();
                                                j++;
                                                if (j > maxJ) break;
                                            }
                                            if (cnt != 0)
                                            {
                                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                            }
                                            IR.Rows[rNo]["billdocno"] = selectedTable.Rows[j - 1]["DOCNO"];
                                            IR.Rows[rNo]["billdocdt"] = selectedTable.Rows[j - 1]["DOCDT"].retDateStr();
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
                        if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                        {
                            IR.Rows[rNo]["cancqnty"] = tacancqnty;
                        }
                        IR.Rows[rNo]["amt"] = taamt;
                        if (VE.Checkbox7 == true)
                        {
                            IR.Rows[rNo]["billqnty"] = tabqnty;
                        }
                        if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                        {
                            IR.Rows[rNo]["balordqnty"] = tabalqnty;
                        }
                        if (ReportType == "Details" && VE.Checkbox8 == true)
                        {
                            IR.Rows[rNo]["ktqnty"] = taktqnty;
                            IR.Rows[rNo]["balktqnty"] = tabalktqnty;
                        }
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;~6";

                        if (i > maxR) break;
                    }

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["Dammy"] = "Grand Totals";
                    IR.Rows[rNo]["qnty"] = tqnty;
                    if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                    {
                        IR.Rows[rNo]["cancqnty"] = tcancqnty;
                    }
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
                    HC.GetPrintHeader(IR, "district", "string", "c,20", "Area");
                    HC.GetPrintHeader(IR, "MTRLSUPPLBY", "string", "c,20", "Order Type");
                    HC.GetPrintHeader(IR, "jobnm", "string", "c,20", "Nature Of Order");
                    //if (VE.Checkbox7 == true)
                    //{
                    if (VE.Checkbox1 == true && VE.Checkbox7 == false)
                    {
                        HC.GetPrintHeader(IR, "qnty", "double", "n,12,3", "Balance Order;Quantity");

                    }
                    else
                    {
                        HC.GetPrintHeader(IR, "qnty", "double", "n,12,3:##,##,##,##0.000", "Order;Qnty");
                        //}
                        //else
                        //{
                        //    HC.GetPrintHeader(IR, "qnty", "double", "n,12,3:##,##,##,##0.000", "Total;Qnty");
                        //}
                        HC.GetPrintHeader(IR, "cancqnty", "double", "n,12,3:##,##,##,##0.000", "Cancel;Qnty");
                        //HC.GetPrintHeader(IR, "amt", "double", "n,17,2:####,##,##,##0.00", "Orde;Value");
                        if (VE.Checkbox7 == true)
                        {
                            HC.GetPrintHeader(IR, "billqnty", "double", "n,12,3", "Bill;Quantity");
                            //HC.GetPrintHeader(IR, "billamt", "double", "n,17,2:####,##,##,##0.00", "Bill;Value");
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
                            string fldval = tbl.Rows[i]["slcd"].ToString() + tbl.Rows[i]["MTRLSUPPLBY"].ToString() + tbl.Rows[i]["jobnm"].ToString();
                            while (tbl.Rows[i]["slcd"].ToString() + tbl.Rows[i]["MTRLSUPPLBY"].ToString() + tbl.Rows[i]["jobnm"].ToString() == fldval)
                            {
                                string autono = tbl.Rows[i]["autono"].ToString();
                                if (VE.Checkbox7 == true)
                                {
                                    billqnty += (from DataRow a in tbl1.Rows where a["ordautono"].retStr() == autono select a["QNTY"].retDbl()).Sum();
                                    billamt += (from DataRow a in tbl1.Rows where a["ordautono"].retStr() == autono select a["netamt"].retDbl()).Sum();

                                }
                                while (tbl.Rows[i]["slcd"].ToString() + tbl.Rows[i]["MTRLSUPPLBY"].ToString() + tbl.Rows[i]["jobnm"].ToString() == fldval && tbl.Rows[i]["autono"].ToString() == autono)
                                {
                                    ordqnty += tbl.Rows[i][qtyfld].retDbl();
                                    ordval += Math.Round((tbl.Rows[i][qtyfld].retDbl() * tbl.Rows[i]["rate"].retDbl()), 2);
                                    tqnty += tbl.Rows[i][qtyfld].retDbl();
                                    tamt += Math.Round((tbl.Rows[i][qtyfld].retDbl() * tbl.Rows[i]["rate"].retDbl()), 2);
                                    taqnty += tbl.Rows[i][qtyfld].retDbl();
                                    taamt += Math.Round((tbl.Rows[i][qtyfld].retDbl() * tbl.Rows[i]["rate"].retDbl()), 2);
                                    cancordqnty += tbl.Rows[i]["cancordqnty"].retDbl();
                                    tacancqnty += tbl.Rows[i]["cancordqnty"].retDbl();
                                    tcancqnty += tbl.Rows[i]["cancordqnty"].retDbl();

                                    i++;
                                    if (i > maxR) break;
                                }
                                if (i > maxR) break;
                            }
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["slcd"] = tbl.Rows[i - 1]["slcd"].ToString();
                            IR.Rows[rNo]["district"] = tbl.Rows[i - 1]["district"].ToString();
                            IR.Rows[rNo]["slnm"] = tbl.Rows[i - 1]["slnm"].ToString();
                            //IR.Rows[rNo]["MTRLSUPPLBY"] = Salesfunc.GetMTRLSUPPLBY(tbl.Rows[i - 1]["MTRLSUPPLBY"].retStr());
                            IR.Rows[rNo]["jobnm"] = tbl.Rows[i - 1]["jobnm"].ToString();
                            IR.Rows[rNo]["qnty"] = ordqnty;
                            if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                            {
                                IR.Rows[rNo]["cancqnty"] = cancordqnty;
                            }
                            //IR.Rows[rNo]["amt"] = ordval;
                            if (VE.Checkbox7 == true)
                            {
                                IR.Rows[rNo]["billqnty"] = billqnty;
                                //IR.Rows[rNo]["billamt"] = billamt;                               
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
                        if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                        {
                            IR.Rows[rNo]["cancqnty"] = tacancqnty;
                        }
                        //IR.Rows[rNo]["amt"] = taamt;
                        if (VE.Checkbox7 == true)
                        {
                            IR.Rows[rNo]["billqnty"] = tabqnty;
                            //IR.Rows[rNo]["billamt"] = tabamt;
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
                    if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                    {
                        IR.Rows[rNo]["cancqnty"] = tcancqnty;
                    }
                    //IR.Rows[rNo]["amt"] = tamt;
                    if (VE.Checkbox7 == true)
                    {
                        IR.Rows[rNo]["billqnty"] = tbqnty;
                        //IR.Rows[rNo]["billamt"] = tbamt;
                    }
                    if (!(VE.Checkbox1 == true && VE.Checkbox7 == false))
                    {
                        IR.Rows[rNo]["balordqnty"] = tbalqnty;
                    }
                    if (ReportType == "Details" && VE.Checkbox8 == true)
                    {
                        IR.Rows[rNo]["ktqnty"] = tktqnty;
                        IR.Rows[rNo]["balktqnty"] = tbalktqnty;
                    }
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;~2";
                }
                string pghdr1 = "", repname = CommFunc.retRepname("Rep_JobOrdReg");
                if (VE.Checkbox1 == true && showasperpslip == true) pghdr1 = "Pending Order (Packingslip) Register " + ReportType + " from " + fdt + " to " + tdt + " (" + txnupto + ")";
                else if (VE.Checkbox1 == true && showasperpslip == false) pghdr1 = "Pending Order (Bill) Register " + ReportType + " from " + fdt + " to " + tdt + " (" + txnupto + ")";
                else pghdr1 = "Order Register " + ReportType + " from " + fdt + " to " + tdt;

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