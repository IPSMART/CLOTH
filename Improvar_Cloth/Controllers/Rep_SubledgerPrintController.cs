using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class Rep_SubledgerPrintController : Controller
    {
        public static string[,] headerArray;
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();

        // GET: Rep_SubledgerPrint
        public ActionResult Rep_SubledgerPrint()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Subledger Printing";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE); ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    if (TempData["printparameter"] != null)
                    {
                        VE = (ReportViewinHtml)TempData["printparameter"];
                    }
                    //DataTable repformat = Salesfunc.getRepFormat("ENVL");
                    //DataTable repformat1 = Salesfunc.getRepFormat("CORRES");
                    //repformat.Merge(repformat1);
                    //if (repformat != null)
                    //{
                    //    VE.DropDown_list1 = (from DataRow dr in repformat.Rows
                    //                         select new DropDown_list1()
                    //                         {
                    //                             text = dr["text"].ToString(),
                    //                             value = dr["value"].ToString()
                    //                         }).ToList();
                    //}
                    //else
                    //{
                    //    List<DropDown_list1> drplst = new List<DropDown_list1>();
                    //    VE.DropDown_list1 = drplst;
                    //}
                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("ALL");
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);
                    VE.DropDown_list = (from i in DB1.MS_LINK select new DropDown_list() { value = i.LINKCD, text = i.LINKNM }).OrderBy(s => s.text).ToList();
                    VE.TEXTBOX3 = MasterHelp.ComboFill("linkcd", VE.DropDown_list, 0, 1);

                    VE.DropDown_list_SubLegGrp = DropDownHelp.GetSubLegGrpforSelection();
                    VE.SubLeg_Grp = MasterHelp.ComboFill("slcdgrpcd", VE.DropDown_list_SubLegGrp, 0, 1);

                    VE.DropDown_list_AGSLCD = DropDownHelp.GetAgSlcdforSelection();
                    VE.Agslnm = MasterHelp.ComboFill("agslcd", VE.DropDown_list_AGSLCD, 0, 1);

                    VE.Checkbox1 = true;
                    VE.DefaultView = true;
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
        public ActionResult GetSubLedgerDetails(string val, string code)
        {
            try
            {
                var str = MasterHelp.SLCD_help(val, code);
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
        public ActionResult Rep_SubledgerPrint(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string reptype = VE.TEXTBOX5;
                string dbname = CommVar.FinSchema(UNQSNO);
                string com = CommVar.Compcd(UNQSNO);
                string loc = CommVar.Loccd(UNQSNO);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                string slcd = "", sql = "", linkcd = "";
                string scmf = CommVar.FinSchema(UNQSNO), LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO);

                if (FC.AllKeys.Contains("slcdvalue"))
                {
                    slcd = FC["slcdvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("linkcdvalue"))
                {
                    linkcd = FC["linkcdvalue"].ToString().retSqlformat();
                }
                DataTable tbl;
                string query = "";
                string agslcdsql = "";
                switch (CommVar.ModuleCode())
                {
                    case "SALES":
                        agslcdsql = "( select a.slcd, max(a.agslcd) agslcd from " + CommVar.SaleSchema(UNQSNO) + ".m_subleg_com a where a.compcd='" + COM + "' group by a.slcd ) b, ";
                        break;
                    case "SALESCHEM": case "SALESREC": case "SALESSMPL": case "SALESELEC": case "SALESMKJ":
                        agslcdsql = "( select a.slcd, max(a.agslcd) agslcd from " + CommVar.SaleSchema(UNQSNO) + ".m_subleg_sddtl a where a.compcd='" + COM + "' group by a.slcd ) b, ";
                        break;
                    case "SALESSAREE": case "SALESCLOTH":
                        agslcdsql = "( select a.slcd, max(a.agslcd) agslcd from " + CommVar.SaleSchema(UNQSNO) + ".m_subleg_com a where a.compcd='" + COM + "' group by a.slcd ) b, ";
                        break;
                    case "SALESPOLY":
                        agslcdsql = "( select a.slcd, max(a.agslcd) agslcd from " + CommVar.SaleSchema(UNQSNO) + ".m_subleg_sddtl a where a.compcd='" + COM + "' group by a.slcd ) b, ";
                        break;
                    case "SALKNITFAB":
                        agslcdsql = "( select a.slcd, max(a.agslcd) agslcd from " + CommVar.SaleSchema(UNQSNO) + ".m_subleg_com a where a.compcd='" + COM + "' group by a.slcd ) b, ";
                        break;
                    default:
                        agslcdsql = "( select '' slcd, '' agslcd from dual ) b, ";
                        break;
                }
                if (reptype == "All")
                {
                    string selslcdgrpcd = "", agslcdvalue = "", fld = "slcd";
                    if (FC.AllKeys.Contains("slcdgrpcdvalue")) { selslcdgrpcd = CommFunc.retSqlformat(FC["slcdgrpcdvalue"].ToString()); fld = "parentcd"; }
                    if (FC.AllKeys.Contains("agslcdvalue")) agslcdvalue = CommFunc.retSqlformat(FC["agslcdvalue"].ToString());

                    query = "Select distinct a.SLCD,a.SLNM,a.FULLNAME,a.PARTYCD,a.add1,a.add2,a.add3,a.add4,a.add5,a.add6,a.add7,a.STATE,a.SLAREA,a.PANNO, b.agslcd, z.slnm agslnm, ";
                    query += "a.GSTNO,a.ADHAARNO,a.MSMENO,a.REGMOBILE,a.REGEMAILID,a.PARTYNM,nvl(a.AUTOREMINDEROFF,'N') AUTOREMINDEROFF,s.parentcd, s.parentnm,t.USR_ENTDT,a.TCSAPPL,a.TOT194Q from  ";

                    query += "(Select distinct a.m_autono,a.SLCD,a.SLNM,a.FULLNAME,a.PARTYCD,a.add1,a.add2,a.add3,a.add4,a.add5,a.add6,a.add7,a.STATE,a.SLAREA,a.PANNO,a.GSTNO,a.ADHAARNO,a.MSMENO,a.REGMOBILE,a.REGEMAILID,b.PARTYNM,nvl(a.AUTOREMINDEROFF,'N')AUTOREMINDEROFF,a.TCSAPPL,a.TOT194Q  ";
                    query += " from " + dbname + ".m_subleg a," + dbname + ".M_PARTYGRP b," + dbname + ".m_subleg_link c where a.PARTYCD=b.PARTYCD(+) and a.slcd=c.slcd(+)  ";
                    if (linkcd != "") query += " and c.linkcd in(" + linkcd + ") " + Environment.NewLine;
                    query += ") a , ";

                    query += agslcdsql + Environment.NewLine;

                    query += "(select a.grpcd, a.parentcd, c.slcdgrpnm||'  '||b.slcdgrpnm parentnm, " + Environment.NewLine;
                    query += "a.grpcdfull, a.slcdgrpnm, a.class1cd, a.slcd, a.slcdclass1cd from " + Environment.NewLine;

                    query += "(select a.grpcd, a.slcdgrpcd parentcd, a.parentcd rootcd, a.grpcdfull, a.slcdgrpnm, b.class1cd, a.slcd, " + Environment.NewLine;
                    query += "a.slcd||nvl(b.class1cd,' ') slcdclass1cd " + Environment.NewLine;
                    query += "from " + scmf + ".m_subleg_grp a, " + scmf + ".m_subleg_grpclass1 b " + Environment.NewLine;
                    query += "where a.grpcd=b.grpcd(+) and a.slcd is not null ) a, " + Environment.NewLine;

                    query += "(select distinct a.slcdgrpcd, a.slcdgrpnm, a.grpcdfull " + Environment.NewLine;
                    query += "from " + scmf + ".m_subleg_grp a where a.slcd is null) b, " + Environment.NewLine;

                    query += "(select distinct a.slcdgrpcd, a.slcdgrpnm, a.grpcdfull " + Environment.NewLine;
                    query += "from " + scmf + ".m_subleg_grp a where a.slcd is null) c " + Environment.NewLine;

                    query += "where a.rootcd = c.slcdgrpcd(+) and a.parentcd=b.slcdgrpcd(+) ) s," + scmf + ".m_cntrl_hdr t, " + scmf + ".m_subleg z " + Environment.NewLine;
                    query += " where a.slcd=s.slcd(+) and a.m_autono=t.m_autono(+) and a.slcd=b.slcd(+) and b.agslcd=z.slcd(+) ";
                    if (selslcdgrpcd.retStr() != "") query += "and (nvl(s.parentcd,' ') in (" + selslcdgrpcd + ") ) " + Environment.NewLine;
                    if (agslcdvalue != "") query += " and b.agslcd in (" + agslcdvalue + ") " + Environment.NewLine;
                    if (selslcdgrpcd.retStr() != "") query += " order by s.parentnm,a.SLNM "; else query += " order by a.SLNM ";
                    tbl = MasterHelp.SQLquery(query);
                    if (tbl.Rows.Count == 0)
                    {
                        return Content("No records found");
                    }
                    if (tbl.Rows.Count != 0)
                    {
                        DataTable IR = new DataTable("mstrep");
                        Models.PrintViewer PV = new Models.PrintViewer();
                        HtmlConverter HC = new HtmlConverter();
                        HC.RepStart(IR, 2);
                        HC.GetPrintHeader(IR, "SLCD", "string", "c,12", "Ledger Code");
                        HC.GetPrintHeader(IR, "SLNM", "string", "c,28", "Ledger Name");
                        HC.GetPrintHeader(IR, "FULLNAME", "string", "c,26", "Full Name");
                        HC.GetPrintHeader(IR, "PARTYCD", "string", "c,12", "Party Group Code");
                        HC.GetPrintHeader(IR, "PARTYNM", "string", "c,26", "Party Group Name");
                        HC.GetPrintHeader(IR, "STATE", "string", "c,26", "State");
                        HC.GetPrintHeader(IR, "SLAREA", "string", "c,50", "Area");
                        HC.GetPrintHeader(IR, "ADDRESS", "string", "c,50", "Address");
                        HC.GetPrintHeader(IR, "PANNO", "string", "c,14", "PANNO");
                        HC.GetPrintHeader(IR, "GSTNO", "string", "c,17", "GSTNO");
                        HC.GetPrintHeader(IR, "ADHAARNO", "string", "c,13", "ADHAARNO");
                        HC.GetPrintHeader(IR, "MSMENO", "string", "c,13", "MSME No");
                        HC.GetPrintHeader(IR, "REGMOBILE", "string", "c,13", "Mobile");
                        HC.GetPrintHeader(IR, "REGEMAILID", "string", "c,27", "Email");
                        if (VE.Checkbox2 == true) HC.GetPrintHeader(IR, "AUTOREMINDEROFF", "string", "c,10", "Auto Send Email/Sms of reminder disable");
                        HC.GetPrintHeader(IR, "USR_ENTDT", "string", "c,27", "Entry Date");
                        HC.GetPrintHeader(IR, "AGSLCD", "string", "c,10", "Agent Code");
                        HC.GetPrintHeader(IR, "AGSLNM", "string", "c,20", "Agent Name");
                        HC.GetPrintHeader(IR, "TCSAPPL", "string", "c,5", "TCS Applicable");
                        HC.GetPrintHeader(IR, "TOT194Q", "string", "c,5", "Turnover Limit Cross For 194Q");

                        Int32 i = 0; Int32 maxR = 0, rNo = 0;
                        i = 0; maxR = tbl.Rows.Count - 1;
                        while (i <= maxR)
                        {
                            string fldval = tbl.Rows[i][fld].retStr();
                            if (selslcdgrpcd != "")
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + fldval + "  " + " </span>" + tbl.Rows[i]["parentnm"].retStr();
                                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                            }
                            while (tbl.Rows[i][fld].retStr() == fldval)
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["SLCD"] = tbl.Rows[i]["SLCD"];
                                IR.Rows[rNo]["SLNM"] = tbl.Rows[i]["SLNM"];
                                IR.Rows[rNo]["FULLNAME"] = tbl.Rows[i]["FULLNAME"];
                                IR.Rows[rNo]["PARTYCD"] = tbl.Rows[i]["PARTYCD"];
                                IR.Rows[rNo]["PARTYNM"] = tbl.Rows[i]["PARTYNM"];
                                IR.Rows[rNo]["STATE"] = tbl.Rows[i]["STATE"];
                                IR.Rows[rNo]["SLAREA"] = tbl.Rows[i]["SLAREA"];
                                IR.Rows[rNo]["ADDRESS"] = tbl.Rows[i]["add1"].ToString() + "," + tbl.Rows[i]["add2"].ToString() + "," + tbl.Rows[i]["add3"].ToString() + "," +
                                     tbl.Rows[i]["add4"].ToString() + "," + tbl.Rows[i]["add5"].ToString() + "," + tbl.Rows[i]["add6"].ToString() + "," + tbl.Rows[i]["add7"].ToString();
                                IR.Rows[rNo]["PANNO"] = tbl.Rows[i]["PANNO"];
                                IR.Rows[rNo]["GSTNO"] = tbl.Rows[i]["GSTNO"];
                                IR.Rows[rNo]["ADHAARNO"] = tbl.Rows[i]["ADHAARNO"];
                                IR.Rows[rNo]["MSMENO"] = tbl.Rows[i]["MSMENO"];
                                IR.Rows[rNo]["REGMOBILE"] = tbl.Rows[i]["REGMOBILE"];
                                IR.Rows[rNo]["REGEMAILID"] = tbl.Rows[i]["REGEMAILID"];
                                if (VE.Checkbox2 == true) IR.Rows[rNo]["AUTOREMINDEROFF"] = tbl.Rows[i]["AUTOREMINDEROFF"].retStr() == "Y" ? "Yes" : "No";
                                IR.Rows[rNo]["USR_ENTDT"] = tbl.Rows[i]["USR_ENTDT"];
                                IR.Rows[rNo]["AGSLCD"] = tbl.Rows[i]["AGSLCD"];
                                IR.Rows[rNo]["AGSLNM"] = tbl.Rows[i]["AGSLNM"];
                                IR.Rows[rNo]["TCSAPPL"] = tbl.Rows[i]["TCSAPPL"].retStr() == "Y" ? "Yes" : "No";
                                IR.Rows[rNo]["TOT194Q"] = tbl.Rows[i]["TOT194Q"].retStr() == "Y" ? "Yes" : "No";
                                i = i + 1;
                                if (i > maxR) break;
                            }

                        }

                        string pghdr1 = "";
                        pghdr1 = "Sub Ledger Details";
                        string repname = pghdr1.retRepname();
                        PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);
                        return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
                    }
                }
                else
                {
                    sql = "select a.compcd, a.compnm, b.add1, b.add2,  b.add3, b.add4, b.add5, b.add6, b.add7, ";
                    sql += "b.phno1std, b.phno1, b.regmobile, b.regemailid ";
                    sql += "from " + scmf + ".m_comp a, " + scmf + ".m_loca b ";
                    sql += "where a.compcd = b.compcd and b.loccd = '" + (VE.Checkbox1 == true ? "KOLK" : CommVar.Loccd(UNQSNO)) + "' and ";
                    sql += "a.compcd = '" + CommVar.Compcd(UNQSNO) + "' ";
                    DataTable tblComp = MasterHelp.SQLquery(sql);

                    //sql = "select distinct a.slcd, nvl(a.fullname,a.slnm) slnm, a.add1, a.add2, a.add3, a.add4, ";
                    sql = "select distinct a.slcd, a.slnm, a.add1, a.add2, a.add3, a.add4, ";
                    sql += "a.add5, a.add6, a.add7, a.regmobile, a.regemailid ";
                    sql += "from " + scmf + ".m_subleg a," + scmf + ".m_subleg_link b where a.slcd=b.slcd(+) ";
                    if (slcd == "")
                    {
                        sql += " and a.slcd ='" + VE.TEXTBOX1 + "' ";
                    }
                    else
                    {
                        sql += " and a.slcd in (" + slcd + ") ";
                    }
                    if (linkcd != "") sql += " and b.linkcd in (" + linkcd + ") ";
                    sql += " order by slnm";
                    tbl = MasterHelp.SQLquery(sql);
                    if (tbl.Rows.Count == 0) return Content("No Records..");
                    if (tblComp.Rows.Count == 0) return Content("No Data Gathers for company Records..");

                    DataTable IR = new DataTable();
                    IR.Columns.Add("slcd", typeof(string));
                    IR.Columns.Add("othnm", typeof(string));
                    IR.Columns.Add("slnm", typeof(string));
                    IR.Columns.Add("sladd1", typeof(string));
                    IR.Columns.Add("sladd2", typeof(string));
                    IR.Columns.Add("sladd3", typeof(string));
                    IR.Columns.Add("sladd4", typeof(string));
                    IR.Columns.Add("sladd5", typeof(string));
                    IR.Columns.Add("sladd6", typeof(string));
                    IR.Columns.Add("sladd7", typeof(string));
                    IR.Columns.Add("slphno", typeof(string));
                    IR.Columns.Add("slemail", typeof(string));

                    IR.Columns.Add("compnm", typeof(string));
                    IR.Columns.Add("compadd1", typeof(string));
                    IR.Columns.Add("compadd2", typeof(string));
                    IR.Columns.Add("compadd3", typeof(string));
                    IR.Columns.Add("compadd4", typeof(string));
                    IR.Columns.Add("compadd5", typeof(string));
                    IR.Columns.Add("compadd6", typeof(string));
                    IR.Columns.Add("compadd7", typeof(string));
                    IR.Columns.Add("compphno", typeof(string));
                    IR.Columns.Add("compemail", typeof(string));
                    IR.Columns.Add("docno", typeof(string));
                    IR.Columns.Add("translnm", typeof(string));

                    Int32 i = 0, rNo = 0, maxR = tbl.Rows.Count - 1;
                    while (i <= maxR)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["slcd"] = tbl.Rows[i]["slcd"];
                        IR.Rows[rNo]["othnm"] = VE.TEXTBOX2;
                        IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"];
                        IR.Rows[rNo]["sladd1"] = tbl.Rows[i]["add1"];
                        IR.Rows[rNo]["sladd2"] = tbl.Rows[i]["add2"];
                        IR.Rows[rNo]["sladd3"] = tbl.Rows[i]["add3"];
                        IR.Rows[rNo]["sladd4"] = tbl.Rows[i]["add4"];
                        IR.Rows[rNo]["sladd5"] = tbl.Rows[i]["add5"];
                        IR.Rows[rNo]["sladd6"] = tbl.Rows[i]["add6"];
                        IR.Rows[rNo]["sladd7"] = tbl.Rows[i]["add7"];
                        IR.Rows[rNo]["slphno"] = tbl.Rows[i]["regmobile"] == DBNull.Value ? "" : "Ph. " + tbl.Rows[i]["regmobile"];
                        IR.Rows[rNo]["slemail"] = tbl.Rows[i]["regemailid"] == DBNull.Value ? "" : "Email : " + tbl.Rows[i]["regemailid"];
                        IR.Rows[rNo]["compnm"] = tblComp.Rows[0]["compnm"];
                        IR.Rows[rNo]["compadd1"] = tblComp.Rows[0]["add1"];
                        IR.Rows[rNo]["compadd2"] = tblComp.Rows[0]["add2"];
                        IR.Rows[rNo]["compadd3"] = tblComp.Rows[0]["add3"];
                        IR.Rows[rNo]["compadd4"] = tblComp.Rows[0]["add4"];
                        IR.Rows[rNo]["compadd5"] = tblComp.Rows[0]["add5"];
                        IR.Rows[rNo]["compadd6"] = tblComp.Rows[0]["add6"];
                        IR.Rows[rNo]["compadd7"] = tblComp.Rows[0]["add7"];
                        IR.Rows[rNo]["compphno"] = tblComp.Rows[0]["regmobile"] == DBNull.Value ? "" : "Ph. " + tblComp.Rows[0]["regmobile"];
                        IR.Rows[rNo]["compemail"] = tblComp.Rows[0]["regemailid"] == DBNull.Value ? "" : "Email : " + tblComp.Rows[0]["regemailid"];
                        if (VE.TEXTBOX7.retStr() != "") IR.Rows[rNo]["translnm"] = VE.TEXTBOX7.retStr();
                        IR.Rows[rNo]["docno"] = "";
                        i++;
                    }

                    //string rptfile = reptype == "Envelope" ? "ENVELOPE" : "CORRES";
                    string rptfile = (reptype == "Envelope" ? "ENVELOPE_" : "CORRES_") + CommVar.ClientCode(UNQSNO);
                    string path = Server.MapPath("~/Report/" + rptfile + ".rpt");
                    if (!System.IO.File.Exists(path))
                    {
                        rptfile = reptype == "Envelope" ? "ENVELOPE" : "CORRES";
                    }
                    string rptname = "~/Report/" + rptfile + ".rpt";
                    string complogo = MasterHelp.retCompLogo();

                    ReportDocument reportdocument = new ReportDocument();
                    reportdocument.Load(Server.MapPath(rptname));

                    reportdocument.SetDataSource(IR);
                    if (reptype == "Excel")
                    {
                        string path_Save = @"C:\Ipsmart\SublegExcel.xls";
                        if (System.IO.File.Exists(path_Save))
                        {
                            System.IO.File.Delete(path_Save);
                        }
                        reportdocument.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.Excel, path_Save);
                        byte[] buffer = System.IO.File.ReadAllBytes(path_Save);
                        Response.ClearContent();
                        Response.Buffer = true;
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AddHeader("Content-Disposition", "attachment; filename=SublegExcel.xls");
                        Response.BinaryWrite(buffer);
                        reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
                        Response.Flush();
                        Response.End();
                        return Content("Excel exported sucessfully");
                    }
                    else
                    {
                        Response.Buffer = false;
                        Response.ClearContent();
                        Response.ClearHeaders();
                        Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        stream.Seek(0, SeekOrigin.Begin);
                        reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
                        return new FileStreamResult(stream, "application/pdf");
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
    }
}
