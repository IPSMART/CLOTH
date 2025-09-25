using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using System.Collections.Generic;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Text.RegularExpressions;

namespace Improvar.Controllers
{
    public class PhyStockPrintController : Controller
    {
        // GET: PhyStockPrint
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        MasterHelpFa MasterHelpFa = new MasterHelpFa();
        DropDownHelp DropDownHelp = new DropDownHelp();

        string path_Save = "C:\\Ipsmart\\Temp";

        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult PhyStockPrint()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ReportViewinHtml VE;
                    if (TempData["printparameter"] == null)
                    {
                        VE = new ReportViewinHtml();
                    }
                    else
                    {
                        VE = (ReportViewinHtml)TempData["printparameter"];
                    }
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ViewBag.formname = "Physical Stock Printing";
                    ViewBag.Title = "Physical Stock Printing";

                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    string reptype = "PHSTK";
                    DataTable repformat = Salesfunc.getRepFormat(reptype);

                    if (repformat != null)
                    {
                        VE.DropDown_list1 = (from DataRow dr in repformat.Rows
                                             select new DropDown_list1()
                                             {
                                                 text = dr["text"].ToString(),
                                                 value = dr["value"].ToString()
                                             }).ToList();
                    }
                    else
                    {
                        List<DropDown_list1> drplst = new List<DropDown_list1>();
                        VE.DropDown_list1 = drplst;
                    }

                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = masterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);

                    VE.DropDown_list_MTRLJOBCDList = DropDownHelp.GetMtrljobforSelection();
                    VE.Mtrljobnm = masterHelp.ComboFill("mtrljobcd", VE.DropDown_list_MTRLJOBCDList, 0, 1);

                    VE.DefaultView = true;
                    VE.ExitMode = 1;
                    VE.DefaultDay = 0;
                    return View(VE);
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
                var str = masterHelp.GOCD_help(val);
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

        public ActionResult PhyStockDetailsPrint(ReportViewinHtml VE, FormCollection FC, string Command)
        {
            try
            {
                //Cn.getQueryString(VE);

                string fdate = "", tdate = "";
                if (VE.FDT != null)
                {
                    fdate = Convert.ToString(Convert.ToDateTime(FC["FDT"].ToString())).Substring(0, 10);
                }
                if (VE.TDT != null)
                {
                    tdate = Convert.ToString(Convert.ToDateTime(FC["TDT"].ToString())).Substring(0, 10);
                }
                string fdocno = FC["FDOCNO"].ToString();
                string tdocno = FC["TDOCNO"].ToString();
                string doccd = FC["doccd"].ToString();
                string godown = VE.TEXTBOX1;
                string ReportType = VE.TEXTBOX3;
                string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
                string yr_cd = CommVar.YearCode(UNQSNO);
                string prccd = "WP";

                DataTable tbl = new DataTable();
                string chk = "a.barno||a.cutlength||a.rate";
                if (ReportType == "Super Summary")
                {
                    chk = "a.autono";
                }
                string sql = "select " + chk + " chkfld, a.docno,a.docdt,a.autoslno,a.autono,c.itcd,a.slno,a.barno,a.stktype,a.mtrljobcd,a.partcd,a.nos,a.qnty,a.itrem,a.rate,a.cutlength,a.locabin,a.shade,a.baleyr, " + Environment.NewLine;
                sql += "a.baleno,c.styleno||' '||c.itnm itstyle,c.styleno,d.gonm,d.prcnm,a.trem,a.usr_id,x1.rate wprate,x2.rate rprate,x3.rate cprate,x4.rate jobprate,x5.rate jobsrate,c.itnm,f.itcd fabitcd,f.itnm fabitnm,g.itgrpcd,g.itgrpnm " + Environment.NewLine;

                sql += "from  ( " + Environment.NewLine;
                sql += "select d.docno,d.docdt,a.autono||a.slno autoslno,a.autono,a.slno,a.barno,a.stktype,a.mtrljobcd,a.partcd,a.nos,a.qnty,a.itrem,a.rate,a.cutlength,a.locabin,a.shade,a.baleyr,a.baleno,b.gocd,b.prccd,b.trem,d.usr_id " + Environment.NewLine;
                sql += "from " + scm + ".T_PHYSTK a, " + scm + ".T_PHYSTK_hdr b, " + scm + ".t_cntrl_hdr d " + Environment.NewLine;
                sql += "where a.autono = b.autono(+) and a.autono = d.autono(+) and " + Environment.NewLine;
                sql += "d.compcd = '" + COM + "' and nvl(d.cancel, 'N') = 'N' and " + Environment.NewLine;
                sql += "d.loccd='" + LOC + "' and d.yr_cd = '" + yr_cd + "'  " + Environment.NewLine;
                if (doccd.retStr() != "") sql += "and d.doccd ='" + doccd + "' " + Environment.NewLine;
                if (fdate.retStr() != "") sql += "and d.docdt >= to_date('" + fdate + "', 'dd/mm/yyyy') " + Environment.NewLine;
                if (tdate.retStr() != "") sql += "and d.docdt <= to_date('" + tdate + "', 'dd/mm/yyyy') " + Environment.NewLine;
                if (fdocno != "") sql += "and d.doconlyno >= '" + fdocno + "' " + Environment.NewLine;
                if (tdocno != "") sql += "and d.doconlyno <= '" + tdocno + "'   " + Environment.NewLine;
                sql += ") a, " + Environment.NewLine;

                for (int x = 0; x <= 4; x++)
                {
                    string sqlals = "";
                    switch (x)
                    {
                        case 0:
                            prccd = "WP"; sqlals = "x1"; break;
                        case 1:
                            prccd = "RP"; sqlals = "x2"; break;
                        case 2:
                            prccd = "CP"; sqlals = "x3"; break;
                        case 3:
                            prccd = "JOBP"; sqlals = "x4"; break;
                        case 4:
                            prccd = "JOBS"; sqlals = "x5"; break;

                    }
                    sql += "(select a.barno, a.itcd, a.colrcd, a.sizecd, a.prccd, a.effdt, a.rate from " + Environment.NewLine;
                    sql += "(select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate from " + Environment.NewLine;
                    sql += "(select a.barno, a.prccd, a.effdt, " + Environment.NewLine;
                    sql += "row_number() over (partition by a.barno, a.prccd order by a.effdt desc) as rn " + Environment.NewLine;
                    sql += "from " + scm + ".t_batchmst_price a where nvl(a.rate,0) <> 0 and a.effdt <= to_date('" + tdate + "','dd/mm/yyyy') ) " + Environment.NewLine;
                    sql += "a, " + scm + ".t_batchmst_price b, " + scm + ".t_batchmst c " + Environment.NewLine;
                    sql += "where a.barno=b.barno(+) and a.prccd=b.prccd(+) and a.effdt=b.effdt(+) and a.rn=1 and a.barno=c.barno(+) " + Environment.NewLine;
                    sql += ") a where prccd='" + prccd + "' " + Environment.NewLine;
                    sql += ") " + sqlals;
                    sql += ", ";
                }

                sql += "" + scm + ".t_batchmst b, " + scm + ".m_sitem c, " + scmf + ".m_godown d," + scmf + ".m_uom e, " + scmf + ".M_PRCLST d, " + scm + ".m_sitem f, " + scm + ".m_group g " + Environment.NewLine;
                sql += "where a.barno=b.barno and b.itcd=c.itcd(+) and a.gocd=d.gocd(+) and c.uomcd=e.uomcd(+) and a.prccd=d.prccd(+)  and a.barno=x1.barno(+) and a.barno=x2.barno(+) and a.barno=x3.barno(+) and a.barno=x4.barno(+) and a.barno=x5.barno(+) and c.fabitcd=f.itcd(+) and c.itgrpcd=g.itgrpcd(+) " + Environment.NewLine;
                if (ReportType == "Date Wise Summary")
                {
                    sql += "order by a.docdt,a.barno,c.styleno||' '||c.itnm,a.cutlength,a.rate ";
                }
                else
                {
                    sql += "order by a.autono,a.slno ";
                }
                tbl = masterHelp.SQLquery(sql);
                #region Show Report

                if (tbl.Rows.Count == 0)
                {
                    return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "No Records Found !!" });
                }
                if (tbl.Rows.Count != 0)
                {
                    DataTable IR = new DataTable("mstrep");

                    Models.PrintViewer PV = new Models.PrintViewer();
                    HtmlConverter HC = new HtmlConverter();

                    HC.RepStart(IR, 2);
                    if (ReportType == "Details" || ReportType == "Date Wise Summary")
                    {
                        if (ReportType == "Details")
                        {
                            HC.GetPrintHeader(IR, "docno", "string", "c,12", "Doc No.");
                            HC.GetPrintHeader(IR, "docdt", "string", "c,12", "Date");
                            HC.GetPrintHeader(IR, "gonm", "string", "c,12", "Godown");
                            HC.GetPrintHeader(IR, "prcnm", "string", "c,12", "Price");
                        }
                        HC.GetPrintHeader(IR, "barno", "string", "c,25", "Bar No.");
                        HC.GetPrintHeader(IR, "itstyle", "string", "c,25", "Style No.");
                        if (ReportType == "Date Wise Summary")
                        {
                            HC.GetPrintHeader(IR, "itnm", "string", "c,25", "Item Name");
                            HC.GetPrintHeader(IR, "itgrpnm", "string", "c,25", "Item Group");
                            HC.GetPrintHeader(IR, "fabitnm", "string", "c,25", "Facbric Item");
                        }
                        HC.GetPrintHeader(IR, "cutlength", "double", "c,6,2", "Length");
                        HC.GetPrintHeader(IR, "nos", "double", "c,15", "Nos");
                        HC.GetPrintHeader(IR, "qnty", "double", "c,15,3", "Qnty");
                        HC.GetPrintHeader(IR, "rate", "double", "c,14,2", "Rate");
                        if (ReportType == "Date Wise Summary")
                        {
                            HC.GetPrintHeader(IR, "cprate", "double", "c,14,2", "CP Rate");
                            HC.GetPrintHeader(IR, "jobprate", "double", "c,14,2", "JOB (KARIGAR) Rate");
                        }
                        if (ReportType == "Details")
                        {
                            HC.GetPrintHeader(IR, "stktype", "string", "c,12", "Stock Type");
                            HC.GetPrintHeader(IR, "itrem", "string", "c,15", "Item Remarks");
                            HC.GetPrintHeader(IR, "trem", "string", "c,25", "Remarks");
                        }
                        if (ReportType == "Date Wise Summary")
                        {
                            HC.GetPrintHeader(IR, "trem", "string", "c,25", "Remarks");
                            HC.GetPrintHeader(IR, "usr_id", "string", "c,15", "Created By");

                        }
                    }
                    else
                    {
                        HC.GetPrintHeader(IR, "docno", "string", "c,12", "Doc No.");
                        HC.GetPrintHeader(IR, "docdt", "string", "c,12", "Date");
                        HC.GetPrintHeader(IR, "qnty", "double", "c,15,3", "Qnty");
                        HC.GetPrintHeader(IR, "trem", "string", "c,15", "Remarks");
                        HC.GetPrintHeader(IR, "usr_id", "string", "c,15", "Created By");
                    }
                    Int32 rNo = 0; Int32 i = 0; Int32 maxR = 0;
                    i = 0; maxR = tbl.Rows.Count - 1;
                    string fld = "autoslno";
                    if (ReportType == "Date Wise Summary")
                    {
                        fld = "docdt";
                    }
                    else if (ReportType == "Super Summary")
                    {
                        fld = "autono";
                    }
                    double tnos = 0, tqnty = 0;
                    while (i <= maxR)
                    {
                        string fldval = tbl.Rows[i][fld].retStr();
                        if (ReportType == "Date Wise Summary")
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["Dammy"] = "<span style='font-weight:bold;'>" + "DOC NO :  " + tbl.Rows[i]["docno"].retStr() + " Date :  " + tbl.Rows[i]["docdt"].retDateStr();
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                        }
                        double tgrpnos = 0, tgrpqnty = 0;

                        while (tbl.Rows[i][fld].retStr() == fldval)
                        {
                            string chkfld = tbl.Rows[i]["chkfld"].retStr();
                            double nos = 0, qnty = 0;
                            while (tbl.Rows[i][fld].retStr() == fldval && tbl.Rows[i]["chkfld"].retStr() == chkfld)
                            {
                                tnos += tbl.Rows[i]["nos"].retDbl();
                                tqnty += tbl.Rows[i]["qnty"].retDbl();
                                tgrpnos += tbl.Rows[i]["nos"].retDbl();
                                tgrpqnty += tbl.Rows[i]["qnty"].retDbl();
                                if (ReportType == "Details")
                                {
                                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                    IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].retStr();
                                    IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].retDateStr();
                                    IR.Rows[rNo]["gonm"] = tbl.Rows[i]["gonm"].retStr();
                                    IR.Rows[rNo]["prcnm"] = tbl.Rows[i]["prcnm"].retStr();
                                    IR.Rows[rNo]["barno"] = tbl.Rows[i]["barno"].retStr();
                                    IR.Rows[rNo]["itstyle"] = tbl.Rows[i]["itstyle"].retStr();
                                    if (ReportType == "Date Wise Summary")
                                    {
                                        IR.Rows[rNo]["itnm"] = tbl.Rows[i]["itnm"].retStr();
                                        IR.Rows[rNo]["fabitnm"] = tbl.Rows[i]["fabitnm"].retStr() + " [" + tbl.Rows[i]["fabitcd"].retStr() + "]";
                                        IR.Rows[rNo]["itgrpnm"] = tbl.Rows[i]["itgrpnm"].retStr();
                                    }
                                    IR.Rows[rNo]["cutlength"] = tbl.Rows[i]["cutlength"].retDbl();
                                    IR.Rows[rNo]["nos"] = tbl.Rows[i]["nos"].retDbl();
                                    IR.Rows[rNo]["qnty"] = tbl.Rows[i]["qnty"].retDbl();
                                    IR.Rows[rNo]["rate"] = tbl.Rows[i]["rate"].retDbl();
                                    if (ReportType == "Date Wise Summary")
                                    {
                                        IR.Rows[rNo]["cprate"] = tbl.Rows[i]["cprate"].retDbl();
                                        IR.Rows[rNo]["jobprate"] = tbl.Rows[i]["jobprate"].retDbl();
                                    }
                                    IR.Rows[rNo]["stktype"] = tbl.Rows[i]["stktype"].retStr();
                                    IR.Rows[rNo]["itrem"] = tbl.Rows[i]["itrem"].retStr();
                                    IR.Rows[rNo]["trem"] = tbl.Rows[i]["trem"].retStr();
                                }
                                else
                                {
                                    nos += tbl.Rows[i]["nos"].retDbl();
                                    qnty += tbl.Rows[i]["qnty"].retDbl();
                                }
                                i = i + 1;
                                if (i > maxR) break;
                            }
                            if (ReportType == "Date Wise Summary")
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["barno"] = tbl.Rows[i - 1]["barno"].retStr();
                                IR.Rows[rNo]["itstyle"] = tbl.Rows[i - 1]["itstyle"].retStr();
                                IR.Rows[rNo]["itnm"] = tbl.Rows[i - 1]["itnm"].retStr();
                                IR.Rows[rNo]["fabitnm"] = tbl.Rows[i - 1]["fabitnm"].retStr() + " [" + tbl.Rows[i - 1]["fabitcd"].retStr() + "]";
                                IR.Rows[rNo]["itgrpnm"] = tbl.Rows[i - 1]["itgrpnm"].retStr();
                                IR.Rows[rNo]["cutlength"] = tbl.Rows[i - 1]["cutlength"].retDbl();
                                IR.Rows[rNo]["nos"] = nos;
                                IR.Rows[rNo]["qnty"] = qnty;
                                IR.Rows[rNo]["rate"] = tbl.Rows[i - 1]["rate"].retDbl();
                                if (ReportType == "Date Wise Summary")
                                {
                                    IR.Rows[rNo]["cprate"] = tbl.Rows[i - 1]["cprate"].retDbl();
                                    IR.Rows[rNo]["jobprate"] = tbl.Rows[i - 1]["jobprate"].retDbl();
                                }
                                IR.Rows[rNo]["trem"] = tbl.Rows[i - 1]["trem"].retStr();
                                IR.Rows[rNo]["usr_id"] = tbl.Rows[i - 1]["usr_id"].retStr();

                            }
                            if (ReportType == "Super Summary")
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["docno"] = tbl.Rows[i - 1]["docno"].retStr();
                                IR.Rows[rNo]["docdt"] = tbl.Rows[i - 1]["docdt"].retDateStr();
                                IR.Rows[rNo]["qnty"] = qnty;
                                IR.Rows[rNo]["trem"] = tbl.Rows[i - 1]["trem"].retStr();
                                IR.Rows[rNo]["usr_id"] = tbl.Rows[i - 1]["usr_id"].retStr();


                            }
                            if (i > maxR) break;

                        }
                        if (ReportType == "Date Wise Summary")
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["dammy"] = "";
                            IR.Rows[rNo]["itstyle"] = "Totals (" + fldval.retDateStr() + ")";
                            IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                            IR.Rows[rNo]["nos"] = tgrpnos;
                            IR.Rows[rNo]["qnty"] = tgrpqnty;
                        }

                        if (i > maxR) break;
                    }
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["dammy"] = "";
                    if (ReportType == "Super Summary")
                    {
                        IR.Rows[rNo]["docno"] = "Totals";
                    }
                    else
                    {
                        IR.Rows[rNo]["itstyle"] = "Totals";
                    }
                    IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                    if (ReportType != "Super Summary")
                    {
                        IR.Rows[rNo]["nos"] = tnos;
                    }
                    IR.Rows[rNo]["qnty"] = tqnty;

                    string pghdr1 = "";

                    pghdr1 = "Physical Stock " + ReportType + " as on from " + fdate + " to " + tdate;

                    PV = HC.ShowReport(IR, "PhyStockPrint", pghdr1, "", true, true, "P", false);

                    TempData["PhyStockPrint"] = PV;
                    return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = "PhyStockPrint" });
                }
                #endregion





            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
            return null;
        }

        public ActionResult StockDiffPrint(ReportViewinHtml VE, FormCollection FC, string Command)
        {
            try
            {
                DataTable tbl = GetStockDiffTbl(VE, FC, Command);
                if (tbl.Rows.Count == 0)
                {
                    return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "No Records Found !!" });
                }

                DataTable IR = new DataTable("mstrep");

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                HC.RepStart(IR, 2);
                HC.GetPrintHeader(IR, "itcd", "string", "c,15", "Item Code");
                HC.GetPrintHeader(IR, "itstyle", "string", "c,25", "Style No.");
                HC.GetPrintHeader(IR, "uomcd", "string", "c,10", "Uom");
                HC.GetPrintHeader(IR, "pqnty", "double", "c,15,3", "Physical Stock");
                HC.GetPrintHeader(IR, "aqnty", "double", "c,15,3", "Actual Stock");
                HC.GetPrintHeader(IR, "balqnty", "double", "c,15,3", "Difference");

                Int32 rNo = 0; Int32 i = 0; Int32 maxR = 0;
                i = 0; maxR = tbl.Rows.Count - 1;

                double taqnty = 0, tpqnty = 0, tbalqnty = 0;
                while (i <= maxR)
                {
                    string itcd = tbl.Rows[i]["itcd"].retStr();

                    double aqnty = 0, pqnty = 0, balqnty = 0;
                    while (tbl.Rows[i]["itcd"].retStr() == itcd)
                    {
                        aqnty += tbl.Rows[i]["aqnty"].retDbl();
                        pqnty += tbl.Rows[i]["pqnty"].retDbl();
                        balqnty += tbl.Rows[i]["balqnty"].retDbl();
                        i++;
                        if (i > maxR) break;
                    }
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["itcd"] = tbl.Rows[i - 1]["itcd"].retStr();
                    IR.Rows[rNo]["itstyle"] = tbl.Rows[i - 1]["itstyle"].retStr();
                    IR.Rows[rNo]["uomcd"] = tbl.Rows[i - 1]["uomcd"].retStr();
                    IR.Rows[rNo]["pqnty"] = pqnty;
                    IR.Rows[rNo]["aqnty"] = aqnty;
                    IR.Rows[rNo]["balqnty"] = balqnty;

                    taqnty += aqnty;
                    tpqnty += pqnty;
                    tbalqnty += balqnty;

                    if (i > maxR) break;
                }
                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["dammy"] = "";

                IR.Rows[rNo]["itcd"] = "Grand Totals";
                IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                IR.Rows[rNo]["aqnty"] = taqnty;
                IR.Rows[rNo]["pqnty"] = tpqnty;
                IR.Rows[rNo]["balqnty"] = tbalqnty;

                string pghdr1 = "";

                pghdr1 = "Physical stock v/s Actual stock Details as on " + VE.TDT;

                PV = HC.ShowReport(IR, "PhyStockPrint", pghdr1, "", true, true, "P", false);

                TempData["PhyStockPrint"] = PV;
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = "PhyStockPrint" });






            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
            return null;
        }
        public DataTable GetStockDiffTbl(ReportViewinHtml VE, FormCollection FC, string Command)
        {
            try
            {
                //Cn.getQueryString(VE);

                string fdate = "", tdate = "";
                if (VE.FDT != null)
                {
                    fdate = Convert.ToString(Convert.ToDateTime(FC["FDT"].ToString())).Substring(0, 10);
                }
                if (VE.TDT != null)
                {
                    tdate = Convert.ToString(Convert.ToDateTime(FC["TDT"].ToString())).Substring(0, 10);
                }
                string fdocno = FC["FDOCNO"].ToString();
                string tdocno = FC["TDOCNO"].ToString();
                string doccd = FC["doccd"].ToString();
                string godown = VE.TEXTBOX1.retSqlformat();
                string ReportType = VE.TEXTBOX3;
                string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
                string yr_cd = CommVar.YearCode(UNQSNO);
                string prccd = "WP";
                string selitcd = "";
                if (FC.AllKeys.Contains("itcdvalue")) selitcd = CommFunc.retSqlformat(FC["itcdvalue"].ToString());

                string mtrljobcd = "'FS'";
                if (FC.AllKeys.Contains("mtrljobcdvalue")) mtrljobcd = CommFunc.retSqlformat(FC["mtrljobcdvalue"].ToString());

                DataTable tbl_Phystk = Salesfunc.GetStock(tdate, godown, "", selitcd, mtrljobcd, "", "", "", "CP", "C001", "", "", true, false, "", "", false, false, true, "", false, "", "", false, false, true, false, fdate);
                DataTable tbl_Ackstk = Salesfunc.GetStock(tdate, godown, "", selitcd, mtrljobcd, "", "", "", "CP", "C001", "", "", true, false, "", "", false, false, true, "", false, "", "", false, false, false, false, "", false);

                var Item = tbl_Ackstk.AsEnumerable().Union(tbl_Phystk.AsEnumerable()).OrderBy(d => d.Field<string>("itstyle")).Select(A => new
                {
                    itcd = A["itcd"].ToString(),
                    itstyle = A["itstyle"].ToString(),
                    uomcd = A["uomcd"].ToString(),
                    barno = A["barno"].ToString(),
                    stktype = A["stktype"].ToString(),
                    mtrljobcd = A["mtrljobcd"].ToString(),
                }).Distinct().OrderBy(a => a.itstyle).ToList();

                DataTable tbl = new DataTable();
                if (Item.Count() > 0)
                {

                    var tempPhystk = (from DataRow DR in tbl_Phystk.Rows
                                      group DR by DR["itcd"].retStr() into X
                                      select new
                                      {
                                          itcd = X.Key.retStr(),
                                          balqnty = X.Sum(Z => Z["balqnty"].retDbl())
                                      }).ToList();

                    var tempAckstk = (from DataRow DR in tbl_Ackstk.Rows
                                      group DR by DR["itcd"].retStr() into X
                                      select new
                                      {
                                          itcd = X.Key.retStr(),
                                          balqnty = X.Sum(Z => Z["balqnty"].retDbl())
                                      }).ToList();


                    var tempdata = (from a in Item
                                    join b in tempPhystk on a.itcd equals b.itcd into physJoin
                                    from b in physJoin.DefaultIfEmpty()
                                    join c in tempAckstk on a.itcd equals c.itcd into ackJoin
                                    from c in ackJoin.DefaultIfEmpty()
                                    select new
                                    {
                                        itcd = a.itcd,
                                        itstyle = a.itstyle,
                                        uomcd = a.uomcd,
                                        barno = a.barno,
                                        stktype = a.stktype,
                                        mtrljobcd = a.mtrljobcd,
                                        pqnty = b != null ? b.balqnty : 0,
                                        aqnty = c != null ? c.balqnty : 0
                                    })
                  .GroupBy(x => new { x.itcd, x.itstyle, x.uomcd, x.barno, x.stktype, x.mtrljobcd })
                  .Select(g => new
                  {
                      itcd = g.Key.itcd,
                      itstyle = g.Key.itstyle,
                      uomcd = g.Key.uomcd,
                      barno = g.Key.barno,
                      stktype = g.Key.stktype,
                      mtrljobcd = g.Key.mtrljobcd,
                      pqnty = g.Sum(x => x.pqnty),
                      aqnty = g.Sum(x => x.aqnty),
                      balqnty = (g.Sum(x => x.pqnty).toRound(3) - g.Sum(x => x.aqnty).toRound(3)).toRound(3)
                  })
                  .ToList();

                    if (tempdata.Count > 0)
                    {
                        if (VE.Checkbox1 == true)
                        {
                            tempdata = tempdata.Where(a => a.balqnty != 0).ToList();
                        }
                    }

                    if (tempdata.Count > 0)
                    {
                        tbl = ListToDatatable.LINQResultToDataTable(tempdata);
                    }
                }
                return tbl;

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return null;
            }
        }
        public ActionResult DownloadPhyStockTemplate(ReportViewinHtml VE, FormCollection FC)
        {
            try
            {
                VE.Checkbox1 = true;
                DataTable tbl = GetStockDiffTbl(VE, FC, "");
                if (tbl.Rows.Count == 0)
                {
                    return Content("No Records Found");
                }

                string nm = "Template_Physical stock".retRepname();

                string Excel_Header = "Item Code" + "|" + "Style No" + "|" + "Bar No." + "|" + "Quantity" + "|" + "Stock Type" + "|" + "Material Job";

                ExcelPackage ExcelPkg = new ExcelPackage();
                ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("Sheet1");

                using (ExcelRange Rng = wsSheet1.Cells["A1:F1"])
                {
                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    string[] Header = Excel_Header.Split('|');
                    for (int a = 0; a < Header.Length; a++)
                    {
                        wsSheet1.Cells[1, a + 1].Value = Header[a];
                    }
                }
                Int32 i = 0; Int32 maxR = 0;
                i = 0; maxR = tbl.Rows.Count - 1;

                int rowno = 2, colno = 1;
                while (i <= maxR)
                {
                    colno = 1;

                    wsSheet1.Cells[rowno, colno].Value = tbl.Rows[i]["itcd"].retStr(); colno++;
                    wsSheet1.Cells[rowno, colno].Value = tbl.Rows[i]["itstyle"].retStr(); colno++;
                    wsSheet1.Cells[rowno, colno].Value = tbl.Rows[i]["barno"].retStr(); colno++;
                    wsSheet1.Cells[rowno, colno].Value = tbl.Rows[i]["balqnty"].retDbl(); colno++;
                    wsSheet1.Cells[rowno, colno].Value = tbl.Rows[i]["stktype"].retStr(); colno++;
                    wsSheet1.Cells[rowno, colno].Value = tbl.Rows[i]["mtrljobcd"].retStr(); colno++;
                    rowno++;
                    i++;
                    if (i > maxR) break;
                }
                wsSheet1.Cells[1, 1, 1, 6].AutoFitColumns();

                Response.Clear();
                Response.ClearContent();
                Response.Buffer = true;
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("Content-Disposition", "attachment; filename=Template_" + nm + ".xlsx");
                Response.BinaryWrite(ExcelPkg.GetAsByteArray());
                Response.Flush();
                Response.Close();
                Response.End();
                return Content("Download Sucessfull");


            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
    }
}


