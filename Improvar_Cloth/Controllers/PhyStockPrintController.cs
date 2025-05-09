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

        [HttpPost]
        public ActionResult PhyStockPrint(ReportViewinHtml VE, FormCollection FC, string Command)
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
                                        IR.Rows[rNo]["fabitnm"] = tbl.Rows[i]["fabitnm"].retStr()+" ["+ tbl.Rows[i]["fabitcd"].retStr()+"]";
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
                                IR.Rows[rNo]["itnm"] = tbl.Rows[i-1]["itnm"].retStr();
                                IR.Rows[rNo]["fabitnm"] = tbl.Rows[i-1]["fabitnm"].retStr() + " [" + tbl.Rows[i-1]["fabitcd"].retStr() + "]";
                                IR.Rows[rNo]["itgrpnm"] = tbl.Rows[i-1]["itgrpnm"].retStr();
                                IR.Rows[rNo]["cutlength"] = tbl.Rows[i - 1]["cutlength"].retDbl();
                                IR.Rows[rNo]["nos"] = nos;
                                IR.Rows[rNo]["qnty"] = qnty;
                                IR.Rows[rNo]["rate"] = tbl.Rows[i - 1]["rate"].retDbl();
                                if (ReportType == "Date Wise Summary")
                                {
                                    IR.Rows[rNo]["cprate"] = tbl.Rows[i - 1]["cprate"].retDbl();
                                    IR.Rows[rNo]["jobprate"] = tbl.Rows[i-1]["jobprate"].retDbl();
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

    }
}


