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
        Salesfunc Salesfunc = new Salesfunc();
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
                string slcd = "",linkcd="", sql = "";
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
                if (reptype == "All")
                {
                    query = query + "Select distinct a.SLCD,a.SLNM,a.FULLNAME,a.add1,a.add2,a.add3,a.add4,a.add5,a.add6,a.add7,a.PANNO,a.GSTNO,a.ADHAARNO,a.REGMOBILE,a.REGEMAILID from " + dbname + ".m_subleg a," + dbname + ".m_subleg_link b where a.slcd=b.slcd(+) ";
                    if (linkcd != "") query += " and b.linkcd in(" + linkcd + ") ";
                    query += " order by a.slnm ";
                     tbl = MasterHelp.SQLquery(query);
                    if (tbl.Rows.Count != 0)
                    {
                        DataTable IR = new DataTable("mstrep");
                        Models.PrintViewer PV = new Models.PrintViewer();
                        HtmlConverter HC = new HtmlConverter();
                        HC.RepStart(IR, 2);
                        HC.GetPrintHeader(IR, "SLCD", "string", "c,12", "Ledger Code");
                        HC.GetPrintHeader(IR, "SLNM", "string", "c,28", "Ledger Name");
                        HC.GetPrintHeader(IR, "FULLNAME", "string", "c,26", "Full Name");
                        HC.GetPrintHeader(IR, "ADDRESS", "string", "c,50", "Address");
                        HC.GetPrintHeader(IR, "PANNO", "string", "c,14", "PANNO");
                        HC.GetPrintHeader(IR, "GSTNO", "string", "c,17", "GSTNO");
                        HC.GetPrintHeader(IR, "ADHAARNO", "string", "c,13", "ADHAARNO");
                        HC.GetPrintHeader(IR, "REGMOBILE", "string", "c,13", "Mobile");
                        HC.GetPrintHeader(IR, "REGEMAILID", "string", "c,27", "Email");

                        Int32 i = 0; Int32 maxR = 0;
                        i = 0; maxR = tbl.Rows.Count - 1;
                        while (i <= maxR)
                        {
                            IR.Rows.Add("");
                            IR.Rows[i]["SLCD"] = tbl.Rows[i]["SLCD"];
                            IR.Rows[i]["SLNM"] = tbl.Rows[i]["SLNM"];
                            IR.Rows[i]["FULLNAME"] = tbl.Rows[i]["FULLNAME"];
                            IR.Rows[i]["ADDRESS"] = tbl.Rows[i]["add1"].ToString() + "," + tbl.Rows[i]["add2"].ToString() + "," + tbl.Rows[i]["add3"].ToString() + "," +
                                 tbl.Rows[i]["add4"].ToString() + "," + tbl.Rows[i]["add5"].ToString() + "," + tbl.Rows[i]["add6"].ToString() + "," + tbl.Rows[i]["add7"].ToString();
                            IR.Rows[i]["PANNO"] = tbl.Rows[i]["PANNO"];
                            IR.Rows[i]["GSTNO"] = tbl.Rows[i]["GSTNO"];
                            IR.Rows[i]["ADHAARNO"] = tbl.Rows[i]["ADHAARNO"];
                            IR.Rows[i]["REGMOBILE"] = tbl.Rows[i]["REGMOBILE"];
                            IR.Rows[i]["REGEMAILID"] = tbl.Rows[i]["REGEMAILID"];
                            i = i + 1;
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

                    sql = "select distinct a.slcd, nvl(a.fullname,a.slnm) slnm, a.add1, a.add2, a.add3, a.add4, ";
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
                    if(linkcd!="") sql += " and b.linkcd in (" + linkcd + ") ";
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
                        i++;
                    }

                    string rptfile = reptype == "Envelope" ? "ENVELOPE" : "CORRES";
                    string rptname = "~/Report/" + rptfile + ".rpt";
                    string complogo = Salesfunc.retCompLogo();

                    ReportDocument reportdocument = new ReportDocument();
                    reportdocument.Load(Server.MapPath(rptname));

                    reportdocument.SetDataSource(IR);
                    if (reptype =="Excel")
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
                        Response.AddHeader("Content-Disposition", "attachment; filename=SublegExcel.xls" );
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
