using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.ViewModels;
using System.Data;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using Improvar.Models;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class Rep_EnvelopeController : Controller
    {
        public static string[,] headerArray;
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        MasterHelpFa MasterHelpFa = new MasterHelpFa();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_Envelope
        public ActionResult Rep_Envelope()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Envelope/Correspondence Printing";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    if (TempData["printparameter"] != null)
                    {
                        VE = (ReportViewinHtml)TempData["printparameter"];
                    }

                    DataTable repformat = MasterHelpFa.getRepFormat("ENVL");
                    DataTable repformat1 = MasterHelpFa.getRepFormat("CORRES");
                    repformat1.Merge(repformat1);
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
                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("ALL");
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);
                    VE.Checkbox1 = true;
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
        public ActionResult Rep_Envelope(FormCollection FC, ReportViewinHtml VE, string submitbutton)
        {
            try
            {
                string slcd = "", sql = "";
                string scmf = CommVar.FinSchema(UNQSNO), LOC =CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO);

                if (FC.AllKeys.Contains("slcdvalue"))
                {
                    slcd = FC["slcdvalue"].ToString().retSqlformat();
                }

                sql = "select a.compcd, a.compnm, b.add1, b.add2,  b.add3, b.add4, b.add5, b.add6, b.add7, ";
                sql += "b.phno1std, b.phno1, b.regmobile, b.regemailid ";
                sql += "from " + scmf + ".m_comp a, " + scmf + ".m_loca b ";
                sql += "where a.compcd = b.compcd and b.loccd = '" + (VE.Checkbox1 == true?"KOLK":CommVar.Loccd(UNQSNO)) + "' and ";
                sql += "a.compcd = '" +CommVar.Compcd(UNQSNO) + "' ";
                DataTable tblComp = MasterHelp.SQLquery(sql);

                sql = "select a.slcd, nvl(a.fullname,a.slnm) slnm, a.add1, a.add2, a.add3, a.add4, ";
                sql += "a.add5, a.add6, a.add7, a.regmobile, a.regemailid ";
                sql += "from " + scmf + ".m_subleg a ";
                sql += "where a.slcd in (" + slcd + ") ";
                sql += "order by slnm";
                DataTable tbl = MasterHelp.SQLquery(sql);
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

                Int32 i = 0, rNo=0, maxR = tbl.Rows.Count - 1;
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
                    IR.Rows[rNo]["slphno"] = tbl.Rows[i]["regmobile"] == DBNull.Value?"":"Ph. " + tbl.Rows[i]["regmobile"];
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

                string rptfile = submitbutton=="Envelope"?"ENVELOPE":"CORRES", printemail = "";
                if (VE.TEXTBOX1 != null) rptfile = VE.TEXTBOX1;
                string rptname = "~/Report/" + rptfile + ".rpt";
                string complogo = Salesfunc.retCompLogo();

                ReportDocument reportdocument = new ReportDocument();
                reportdocument.Load(Server.MapPath(rptname));

                reportdocument.SetDataSource(IR);
                reportdocument.SetParameterValue("complogo", complogo);

                if (printemail == "Excel")
                {
                    string path_Save = @"C:\improvar\" + rptname + ".xls";
                    string exlfile = rptname + ".xls";
                    if (System.IO.File.Exists(path_Save))
                    {
                        System.IO.File.Delete(path_Save);
                    }
                    reportdocument.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.Excel, path_Save);
                    byte[] buffer = System.IO.File.ReadAllBytes(path_Save);
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + exlfile);
                    Response.BinaryWrite(buffer);
                    reportdocument.Dispose();
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
                    reportdocument.Dispose();
                    return new FileStreamResult(stream, "application/pdf");
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
    }
}
