﻿using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using System.Data;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using System.Web;
using OfficeOpenXml;

namespace Improvar.Controllers
{
    public class RPTViewerController : Controller
    {
        string CS = null;
        MasterHelpFa masterhelpfa = new MasterHelpFa();
        MasterHelp masterhelp = new MasterHelp();
        Connection Cn = new Connection();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        string path_Save = @"C:\\Ipsmart\\Temp";

        // GET: RPTViewer
        public ActionResult PrintViewer()
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                MyGlobal.MyGlobalString = "";
                PrintViewer PV1 = (Improvar.Models.PrintViewer)TempData["KeepPV"];
                TempData["Keep"] = PV1;
                return View(PV1);
            }
        }
        public ActionResult NoRecords()
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                string Errormessage = "";
                if (Request.QueryString.AllKeys.Contains("errmsg"))
                {
                    Errormessage = Request.QueryString["errmsg"];
                }
                else
                {
                    Errormessage = "No Record Round ";
                }
                ViewBag.errormessage = Errormessage;
                ViewBag.errorsuggestion = "Please Go Back and Choose Different Query";

                return View();
            }
        }
        public ActionResult ResponsivePrintViewer()
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                string ReportName = "";
                if (Request.QueryString.AllKeys.Contains("ReportName"))
                {
                    ReportName = Request.QueryString["ReportName"];
                }
                PrintViewer PV1 = (Improvar.Models.PrintViewer)System.Web.HttpContext.Current.Session[ReportName];
                ViewBag.Title = ReportName;
                return View(PV1);
            }
        }
        public ActionResult GetExcel(string RepNM = "")
        {
            string ReportName = "";
            if (RepNM == "")
            {
                var PreviousUrl = Request.UrlReferrer.AbsoluteUri;
                var uri = new Uri(PreviousUrl);//Create Virtually Query String
                var queryString = HttpUtility.ParseQueryString(uri.Query);
                if (queryString.AllKeys.Contains("ReportName"))
                {
                    ReportName = queryString.Get("ReportName").ToString();
                }
            }
            else
            {
                ReportName = RepNM;
            }
            PrintViewer PV = (Improvar.Models.PrintViewer)System.Web.HttpContext.Current.Session[ReportName];
            string HTML = PV.SetReportContaint[0].GetHtml;
            HTML = HTML.Replace("border:1px", "border:0.1pt");
            HTML = HTML.Replace("border: 1px", "border:0.1pt");
            HTML = HTML.Replace("border-top: 1px", "border-top: 0.1pt");
            HTML = HTML.Replace("border-bottom: 1px", "border-bottom: 0.1pt");
            HTML = HTML.Replace("border-left: 1px", "border-left: 0.1pt");
            HTML = HTML.Replace("border-right: 1px", "border-right: 0.1pt");
            HTML = HTML.Replace("border-top: 2px", "border-top: 0.1pt");
            HTML = HTML.Replace("border-bottom: 2px", "border-bottom: 0.1pt");
            HTML = HTML.Replace("border-left: 2px", "border-left: 0.1pt");
            HTML = HTML.Replace("border-right: 2px", "border-right: 0.1pt");
            HTML = HTML.Replace("<table", "<table border='1'");
            Response.ClearContent();
            Response.Buffer = true;
            //Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            //Response.AddHeader("content-disposition", "attachment;filename= " + ReportName + ".xlsx");
            Response.AddHeader("content-disposition", "attachment; filename=" + ReportName + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.Charset = "";
            Response.Output.Write(HTML);
            Response.Flush();
            Response.End();


            return PartialView();
        }
        public ActionResult GetPDF(string RepNM = "", string Action = "")
        {

            string ReportName = "";
            if (RepNM == "")
            {
                var PreviousUrl = Request.UrlReferrer.AbsoluteUri;
                var uri = new Uri(PreviousUrl);//Create Virtually Query String
                var queryString = HttpUtility.ParseQueryString(uri.Query);
                if (queryString.AllKeys.Contains("ReportName"))
                {
                    ReportName = queryString.Get("ReportName").ToString();
                }
            }
            else
            {
                ReportName = RepNM;
            }
            PrintViewer PV = (Improvar.Models.PrintViewer)System.Web.HttpContext.Current.Session[ReportName];
            string HTML = PV.SetReportContaint[0].GetHtml;
            //HTML = HTML.Replace("border:1px", "border:0.1pt");
            //HTML = HTML.Replace("border: 1px", "border:0.1pt");
            //HTML = HTML.Replace("border-top: 1px", "border-top: 0.1pt");
            //HTML = HTML.Replace("border-bottom: 1px", "border-bottom: 0.1pt");
            //HTML = HTML.Replace("border-left: 1px", "border-left: 0.1pt");
            //HTML = HTML.Replace("border-right: 1px", "border-right: 0.1pt");
            //HTML = HTML.Replace("border-top: 2px", "border-top: 0.1pt");
            //HTML = HTML.Replace("border-bottom: 2px", "border-bottom: 0.1pt");
            //HTML = HTML.Replace("border-left: 2px", "border-left: 0.1pt");
            //HTML = HTML.Replace("border-right: 2px", "border-right: 0.1pt");
            //HTML = HTML.Replace("border: hidden", "border: hidden");
            //HTML = HTML.Replace("<table", "<table border='1'");

            StyleSheet st = new StyleSheet();
            st.LoadStyle("body", "fontsize", "8");
            //HTML = HTML.Replace("font-size:10px;", "font-size:7px;");
            //HTML = HTML.Replace("font-size:13px;", "font-size:7px;");
            //HTML = HTML.Replace("font-size:14px;", "font-size:9pt;");
            //HTML = HTML.Replace("font-size:17px;", "font-size:9pt;");
            //HTML = HTML.Replace("font-size:9pt;", "font-size:7px;");
            HTML = System.Text.RegularExpressions.Regex.Replace(HTML, "<script.*?</script>", "", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            //HTML = HTML.Replace("px", "");

            byte[] pdfBytes = (new NReco.PdfGenerator.HtmlToPdfConverter()).GeneratePdf(HTML);

            //StringReader sr = new StringReader(HTML);
            //Document pdfDoc = new Document();

            //HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
            //MemoryStream memoryStream = new MemoryStream();
            //PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
            //pdfDoc.Open();
            //htmlparser.Parse(sr);
            //pdfDoc.Close();
            //byte[] bytes = memoryStream.ToArray();
            //memoryStream.Close();
            if (Action == "Save")
            {
                string filePath = path_Save + "\\" + ReportName + ".pdf";
                System.IO.File.WriteAllBytes(filePath, pdfBytes);
            }
            else
            {
                System.Web.HttpContext.Current.Response.ClearContent();
                System.Web.HttpContext.Current.Response.Buffer = true;
                System.Web.HttpContext.Current.Response.AddHeader("content-disposition", "attachment; filename=" + ReportName + ".pdf");
                System.Web.HttpContext.Current.Response.ContentType = "application/pdf";
                System.Web.HttpContext.Current.Response.Charset = "";
                System.Web.HttpContext.Current.Response.OutputStream.Write(pdfBytes, 0, pdfBytes.Length);
                System.Web.HttpContext.Current.Response.Flush();
                System.Web.HttpContext.Current.Response.End();
            }
            return PartialView();
        }

        //public ActionResult GetPDF()
        //{

        //    string ReportName = "";
        //    var PreviousUrl = Request.UrlReferrer.AbsoluteUri;
        //    var uri = new Uri(PreviousUrl);//Create Virtually Query String
        //    var queryString = HttpUtility.ParseQueryString(uri.Query);
        //    if (queryString.AllKeys.Contains("ReportName"))
        //    {
        //        ReportName = queryString.Get("ReportName").ToString();
        //    }
        //    PrintViewer PV = (Improvar.Models.PrintViewer)TempData[ReportName];
        //    TempData.Keep();
        //    string HTML = PV.SetReportContaint[0].GetHtml;

        //    StyleSheet st = new StyleSheet();
        //    st.LoadStyle("body", "fontsize", "8");
        //    //HTML = HTML.Replace("font-size:10px;", "font-size:7px;");
        //    //HTML = HTML.Replace("font-size:13px;", "font-size:7px;");
        //    //HTML = HTML.Replace("font-size:14px;", "font-size:9pt;");
        //    //HTML = HTML.Replace("font-size:17px;", "font-size:9pt;");
        //    //HTML = HTML.Replace("font-size:9pt;", "font-size:7px;");
        //    HTML = System.Text.RegularExpressions.Regex.Replace(HTML, "<script.*?</script>", "", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        //    //HTML = HTML.Replace("px", "");

        //    StringReader sr = new StringReader(HTML);
        //    Document pdfDoc = new Document(PageSize.A2, 10f, 10f, 10f, 0f);

        //    HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
        //    MemoryStream memoryStream = new MemoryStream();
        //    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
        //    pdfDoc.Open();
        //    htmlparser.Parse(sr);
        //    pdfDoc.Close();
        //    byte[] bytes = memoryStream.ToArray();
        //    memoryStream.Close();
        //    Response.ClearContent();
        //    Response.Buffer = true;
        //    Response.AddHeader("content-disposition", "attachment; filename=" + MyGlobal.ReportName + ".pdf");
        //    Response.ContentType = "application/pdf";
        //    Response.Charset = "";
        //    Response.OutputStream.Write(bytes, 0, bytes.Length);
        //    Response.Flush();
        //    Response.End();
        //    return PartialView();
        //}
        public ActionResult GetFreez(string freezIndex)
        {
            string ReportName = ""; string extr_col = "";
            var PreviousUrl = Request.UrlReferrer.AbsoluteUri;
            var uri = new Uri(PreviousUrl);//Create Virtually Query String
            var queryString = HttpUtility.ParseQueryString(uri.Query);
            if (queryString.AllKeys.Contains("ReportName"))
            {
                ReportName = queryString.Get("ReportName").ToString();
            }
            HtmlConverter HC = new HtmlConverter();
            string[] rowColindex = freezIndex.Split('_');
            int Colindex = Convert.ToInt32(rowColindex[2]);
            PrintViewer PV1 = (Improvar.Models.PrintViewer)System.Web.HttpContext.Current.Session[ReportName];
            DataTable IR = (DataTable)PV1.IR;
            string html = PV1.SetReportContaint[0].GetHtml;
            if (IR.Columns.Contains("doclink"))
            {
                extr_col = "doclink";
            }
            PrintViewer PV = HC.ShowReport3(IR, ReportName, "", "", true, true, "L", false, Convert.ToInt32(rowColindex[1]), Colindex - 3, PV1, extr_col);
            string HTML1 = PV.SetReportContaint[0].GetHtml;
            PV.SetReportContaint[0].GetHtml = html;
            return Content(HTML1 + "******~~~****" + PV.FreezInnerWidth);
        }
        public ActionResult GetStandardExcel(string RepNM = "", string Action = "")
        {
            try
            {
                string ReportName = "";
                if (RepNM == "")
                {
                    var PreviousUrl = Request.UrlReferrer.AbsoluteUri;
                    var uri = new Uri(PreviousUrl);//Create Virtually Query String
                    var queryString = HttpUtility.ParseQueryString(uri.Query);
                    if (queryString.AllKeys.Contains("ReportName"))
                    {
                        ReportName = queryString.Get("ReportName").ToString();
                    }
                }
                else
                {
                    ReportName = RepNM;
                }
                bool isdammy = false;
                if (System.Web.HttpContext.Current.Session[ReportName] != null)
                {
                    PrintViewer PV = (Improvar.Models.PrintViewer)System.Web.HttpContext.Current.Session[ReportName];
                    DataTable newdt = new DataView(PV.IR).ToTable();
                    var extr_col = "";
                    if (newdt.Columns[0].ColumnName == "dammy")
                    {
                        var dammmyval = newdt.AsEnumerable().Where(a => a.Field<string>("dammy") != "").Select(a => a.Field<string>("dammy")).Distinct().FirstOrDefault();
                        if (dammmyval != null && dammmyval != " ")
                        {
                            foreach (DataRow dr in newdt.Rows)
                            {//<span style='font-weight:100;font-size:9px;'> DA00129   </span>ANKIT JAIN.
                                string dammy = dr["dammy"].retStr();
                                if (dammy != "")
                                {
                                    while (true)
                                    {
                                        int eind = dammy.IndexOf("'>");
                                        if (eind != -1)
                                        {
                                            int sind = dammy.IndexOf("<span");
                                            eind = Math.Abs(sind - eind);
                                            var spanstr = dammy.Substring(sind, eind + 2);
                                            dammy = dammy.Replace(spanstr, "");
                                            dammy = dammy.Replace("</span>", "");
                                            dr["dammy"] = dammy;
                                        }
                                        if (!dammy.Contains("span"))
                                        {
                                            break;
                                        }
                                    }
                                }

                            }
                            //rename column dammy=>Group
                            newdt.Columns[0].ColumnName = "Group";
                            isdammy = true;
                        }
                        else
                        {
                            newdt.Columns.Remove("dammy");
                        }
                        newdt.Columns.Remove("flag");
                        newdt.Columns.Remove("celldesign");
                        if (newdt.Columns.Contains("doclink"))
                        {
                            newdt.Columns.Remove("doclink");
                            extr_col = "doclink";
                        }

                    }
                    //remove a blank row from datatable
                    //newdt = newdt.Rows.Cast<DataRow>().Where(row => !row.ItemArray.All(field => field is DBNull || string.IsNullOrWhiteSpace(field as string))).CopyToDataTable();
                    string RemoveBlankRowExl = System.Web.HttpContext.Current.Session[ReportName + "_RemoveBlankRowExl"].retStr();
                    if (RemoveBlankRowExl != "N")
                    {
                        newdt = newdt.Rows.Cast<DataRow>().Where(row => !row.ItemArray.All(field => field is DBNull || string.IsNullOrEmpty(field as string ?? field.ToString()))).CopyToDataTable();
                    }
                    ExcelPackage workbook = new ExcelPackage();
                    ExcelWorksheet worksheet = workbook.Workbook.Worksheets.Add("Sheet1");
                    worksheet.Cells["A1"].Value = PV.Vname + ", " + PV.Title;
                    worksheet.Cells["A1"].Style.Font.Bold = true;
                    string para = "";
                    if (string.IsNullOrEmpty(PV.Para2)) { para = PV.Para1; } else { para = PV.Para1 + " [" + PV.Para2 + "]"; }
                    worksheet.Cells["A2"].Value = para;
                    worksheet.Cells["A2"].Style.Font.Bold = true;
                    worksheet.Cells["A3"].LoadFromDataTable(newdt, true);
                    worksheet.Row(3).Style.Font.Bold = true;
                    if (isdammy)
                    {
                        worksheet.Column(1).Width = 2.82;
                    }
                    int rowslength = PV.HeaderArray.GetLength(0);
                    int collength = PV.HeaderArray.GetLength(1);
                    string hdrvalue = "";
                    int i1 = 0; int i2 = 0; int i3 = 0;
                    if (extr_col == "") { i1 = 3; i2 = 1; i3 = 2; } else { i1 = 4; i2 = 2; i3 = 3; }
                    for (int i = i1; i < collength; i++)
                    {
                        hdrvalue = "";
                        for (int j = 1; j < rowslength; j++)
                        {
                            hdrvalue = hdrvalue + PV.HeaderArray[j, i] + " ";

                        }
                        if (isdammy)
                        {
                            worksheet.Cells[3, i - i2].Value = hdrvalue;
                        }
                        else
                        {
                            worksheet.Cells[3, i - i3].Value = hdrvalue;
                        }
                    }
                    worksheet.View.FreezePanes(4, 1);

                    if (Action == "Save")
                    {
                        Byte[] fileBytes = workbook.GetAsByteArray();
                        string filePath = path_Save + "\\" + ReportName + ".xlsx";
                        System.IO.File.WriteAllBytes(filePath, fileBytes);
                    }
                    else
                    {
                        Response.Clear();
                        Response.Buffer = true;
                        Response.Charset = "";
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AddHeader("content-disposition", "attachment;filename=" + ReportName + ".xlsx");
                        using (MemoryStream MyMemoryStream = new MemoryStream())
                        {
                            workbook.SaveAs(MyMemoryStream);
                            MyMemoryStream.WriteTo(Response.OutputStream);
                            Response.Flush();
                            Response.End();
                        }
                        workbook.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
            return Content("");
        }
        public ActionResult CheckDocument(string autono, string extr_col = "")
        {
            try
            {
                if (autono == null) autono = "";
                if (autono != "") if (autono.IndexOf("'") < 0) autono = "'" + autono + "'";
                string scmS = CommVar.SaleSchema(UNQSNO);
                string scmF = CommVar.FinSchema(UNQSNO);
                string scmI = CommVar.InvSchema(UNQSNO);
                string scmP = CommVar.PaySchema(UNQSNO);
                string sql = "", str = "", PopupDetails = "", RecordCnt = "", scm = "", docsrc = "", docdesc = "", docno = "";
                if (scmS != "")
                {
                    sql += "select count(*)cnt,x.autono,y.modcd,y.docno,y.docdt " + Environment.NewLine;
                    sql += "from " + scmS + ".t_cntrl_hdr_doc x," + scmS + ".t_cntrl_hdr y " + Environment.NewLine;
                    sql += "where x.autono = y.autono(+) and x.autono in (" + autono + ")" + Environment.NewLine;
                    sql += "group by x.autono,y.modcd,y.docno,y.docdt " + Environment.NewLine;
                }

                if (scmF != "")
                {
                    if (sql != "") sql += "union all " + Environment.NewLine;
                    sql += "select count(*)cnt,x.autono,y.modcd,y.docno,y.docdt " + Environment.NewLine;
                    sql += "from " + scmF + ".t_cntrl_hdr_doc x," + scmF + ".t_cntrl_hdr y " + Environment.NewLine;
                    sql += "where x.autono = y.autono(+) and x.autono in (" + autono + ")" + Environment.NewLine;
                    sql += "group by x.autono,y.modcd,y.docno,y.docdt " + Environment.NewLine;
                }
                if (scmI != "")
                {
                    if (sql != "") sql += "union all " + Environment.NewLine;
                    sql += "select count(*)cnt,x.autono,y.modcd,y.docno,y.docdt " + Environment.NewLine;
                    sql += "from " + scmI + ".t_cntrl_hdr_doc x," + scmI + ".t_cntrl_hdr y " + Environment.NewLine;
                    sql += "where x.autono = y.autono(+) and x.autono in (" + autono + ")" + Environment.NewLine;
                    sql += "group by x.autono,y.modcd,y.docno,y.docdt " + Environment.NewLine;
                }
                if (scmP != "")
                {
                    if (sql != "") sql += "union all " + Environment.NewLine;
                    sql += "select count(*)cnt,x.autono,y.modcd,y.docno,y.docdt " + Environment.NewLine;
                    sql += "from " + scmP + ".t_cntrl_hdr_doc x," + scmP + ".t_cntrl_hdr y " + Environment.NewLine;
                    sql += "where x.autono = y.autono(+) and x.autono in (" + autono + ")" + Environment.NewLine;
                    sql += "group by x.autono,y.modcd,y.docno,y.docdt " + Environment.NewLine;
                }
                DataTable dt = masterhelp.SQLquery(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    RecordCnt = dt.Rows[0]["cnt"].retStr();
                    switch (dt.Rows[0]["modcd"].retStr())
                    {
                        case "F":
                            scm = CommVar.FinSchema(UNQSNO); break;
                        case "I":
                            scm = CommVar.InvSchema(UNQSNO); break;
                        case "S":
                            scm = CommVar.SaleSchema(UNQSNO); break;
                        case "P":
                            scm = CommVar.PaySchema(UNQSNO); break;
                    }
                    docno = dt.Rows[0]["docno"].retStr();
                }
                if (RecordCnt.retDbl() > 1)
                {
                    DataTable IR = new DataTable("mstrep1");
                    Models.PrintViewer PV = new Models.PrintViewer();
                    HtmlConverter HC = new HtmlConverter();
                    string pghdr1 = "";
                    HC.RepStart(IR, 2, extr_col);
                    HC.GetPrintHeader(IR, "docdt", "string", "d,10:dd/mm/yy", "Doc. Date");
                    HC.GetPrintHeader(IR, "docno", "string", "c,45", "Doc. No");
                    HC.GetPrintHeader(IR, "DOC_CTG", "string", "C,45", "Doc. Type");
                    HC.GetPrintHeader(IR, "DOC_DESC", "string", "C,45", "Doc. Desc");
                    HC.GetPrintHeader(IR, "image_1", "string", "c,10", "Image");

                    List<UploadDOC> UploadDOC = new List<Models.UploadDOC>();
                    UploadDOC = Cn.GetUploadImageTransaction(scm, autono);
                    for (int i = 0; i <= UploadDOC.Count - 1; i++)
                    {
                        DataRow dr = IR.NewRow();
                        dr["DOC_CTG"] = UploadDOC[i].docID;
                        dr["DOC_DESC"] = UploadDOC[i].DOC_DESC;
                        dr["image_1"] = UploadDOC[i].DOC_FILE;
                        //dr["Link"] = "";
                        dr["docdt"] = dt.Rows[0]["docdt"].retStr().retDateStr();
                        dr["docno"] = dt.Rows[0]["docno"].retStr();
                        IR.Rows.Add(dr);
                    }

                    string repname = "Popup Register".retRepname();
                    PV = HC.ShowReportPopup(IR, repname, "", "", true, true, "P", false, "", "docpopup_overlay", "docno", "doc_ctg");

                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    SB.Append(PV.SetReportContaint[0].GetHtml);
                    PV.SetReportContaint[0].GetHtml = SB.ToString();
                    SB.Append("##^^");
                    SB.Append(" ");
                    SB.Append("##^^");
                    SB.Append("RPTViewer%20" + repname);
                    SB.Append("##^^");
                    SB.Append("RPTViewer%20" + repname);
                    //return PartialView("_PopUp4", SB.ToString());
                    ModelState.Clear();
                    PopupDetails = RenderRazorViewToString(ControllerContext, "_PopUp4", SB.ToString());//BINDING GRID
                }
                if (RecordCnt.retDbl() == 1)
                {
                    List<UploadDOC> UploadDOC = new List<Models.UploadDOC>();
                    UploadDOC = Cn.GetUploadImageTransaction(scm, autono);
                    docsrc = UploadDOC[0].DOC_FILE;
                    docdesc = UploadDOC[0].docID;
                }
                str = "";
                str += "^RECORDCOUNT=^" + RecordCnt + Cn.GCS();
                str += "^DOCDETAILS=^" + PopupDetails + Cn.GCS();
                str += "^DOCSRC=^" + docsrc + Cn.GCS();
                str += "^DOCDESC=^" + docdesc + Cn.GCS();
                str += "^DOCNO=^" + docno + Cn.GCS();
                return Content(str);

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public static string RenderRazorViewToString(ControllerContext controllerContext, string viewName, object model)
        {
            controllerContext.Controller.ViewData.Model = model;
            using (var stringWriter = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                var viewContext = new ViewContext(controllerContext, viewResult.View, controllerContext.Controller.ViewData, controllerContext.Controller.TempData, stringWriter);
                viewResult.View.Render(viewContext, stringWriter);
                viewResult.ViewEngine.ReleaseView(controllerContext, viewResult.View);
                return stringWriter.GetStringBuilder().ToString();
            }
        }
    }

    public class HtmlConverter
    {
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        PrintViewer PV = new PrintViewer();
        Connection Cn = new Connection();
        MasterHelp masterhelp = new MasterHelp();
        public static string[,] headerArray;
        public string SpanColumnContaint { get; set; }
        public string SpanColumnLength { get; set; }
        public string SpanColumnSetIndex { get; set; }
        //public PrintViewer GetResponsiveHTML(DataTable Table, PrintViewer pv, string[,] headerArray1)
        //{
        //    List<ReportContaint> listemail = new List<ReportContaint>();
        //    System.Text.StringBuilder SB = new System.Text.StringBuilder();

        //    int aryA = pv.HeaderArray.GetLength(0);
        //    int aryB = pv.HeaderArray.GetLength(1);
        //    string[] columnNames = Table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
        //    string[] columnTypes = Table.Columns.Cast<DataColumn>().Select(x => x.DataType.ToString()).ToArray();
        //    string[] span = SpanColumnContaint == null ? null : SpanColumnContaint.Split(',');
        //    string[] spanLangth = SpanColumnLength == null ? null : SpanColumnLength.Split(',');
        //    string[] setIndex = SpanColumnSetIndex == null ? null : SpanColumnSetIndex.Split(',');
        //    try
        //    {
        //        string assign_table = "<table class='grid_table'>";
        //        SB.Append(assign_table);
        //        //Start Hook Codding To Print Header in Every Page When Print
        //        string Header = "<thead>";
        //        string Vname = pv.Vname;
        //        string Title = pv.Title;
        //        string Para1 = pv.Para1;
        //        string Para2 = pv.Para2;
        //        int col_span = pv.ColspanForPDF;

        //        Header = Header + "<tr style='height:24px;'> <th colspan='" + col_span + "' style='border: hidden;font-size:17px;font-weight:300; '>" + Vname + "</th> </tr> ";
        //        Header = Header + "<tr style='height:24px;'> <th colspan='" + col_span + "' style='border: hidden;font-size:17px;font-weight:300;'>" + Title + "</th> </tr> ";

        //        if (Para1 != "") { Header = Header + "<tr style='height:24px;'> <th colspan='" + col_span + "' style='border: hidden;font-size: 15px;font-weight:100;'>" + Para1 + "</th> </tr> "; }
        //        if (Para2 != "") { Header = Header + "<tr style='height:24px;'> <th colspan='" + col_span + "' style='border: hidden;font-size: 15px;font-weight:100;'>" + Para2 + "</th> </tr> "; }
        //        //Header = Header + "<tr> <th colspan='" + col_span + "' style='font-size: 15px;font-weight:100;'></th> </tr> ";

        //        SB.Append(Header);
        //        int totalCol = aryB - 4;
        //        string chk = "";
        //        bool img;
        //        int maxA = aryA - 1; int maxB = aryB - 1;
        //        int a = 1; int b = 0;
        //        if (span != null)
        //        {
        //            Header = "<tr style='color:white;font-size:12px;font-style: normal;font-weight:bold;height:25px'>";
        //            for (int i = 0; i <= totalCol; i++)
        //            {
        //                int flag = 0;
        //                for (int x = 0; x <= setIndex.Length - 1; x++)
        //                {
        //                    if (i.ToString() == setIndex[x].ToString())
        //                    {
        //                        Header = Header + "<th class='' colspan='" + spanLangth[x] + "' style='background-color:#808080;text-align:center;border:1px solid white;' valign='middle'>" + span[x] + "</th>";
        //                        flag = 1;
        //                        i = i + Convert.ToInt32(spanLangth[x]);
        //                    }
        //                }
        //                if (flag == 0)
        //                {
        //                    Header = Header + "<th align='center' class='' style='background-color:#808080;border:1px solid white;'> " + "" + "</th>";
        //                }
        //                else if (flag == 1)
        //                {
        //                    if (i <= totalCol)
        //                    {
        //                        Header = Header + "<th align='center' class='' style='background-color:#808080;border:1px solid white;'>" + "" + "</th>";
        //                    }
        //                }
        //            }
        //            Header = Header + "</tr>";
        //            SB.Append(Header);
        //        }
        //        while (a <= maxA)
        //        {
        //            Header = "";
        //            Header = Header + "<tr style='font-size:12px;font-style: normal;height:16px'>";
        //            b = 0;
        //            while (b <= maxB)
        //            {
        //                chk = Table.Columns[b].ColumnName.ToString().ToLower();

        //                if (chk != "flag" && chk != "dammy" && chk != "celldesign")
        //                {
        //                    var sd1 = Table.Rows[0][b].GetType();
        //                    var sd2 = columnTypes[b].ToString().Replace("System.", "");
        //                    int colwidth = getCollen(pv.HeaderArray[0, b].ToString());
        //                    int colmult;
        //                    if (sd2 == "String")
        //                    { colmult = 7; }
        //                    else { colmult = 6; }
        //                    colwidth = colwidth * colmult;
        //                    string widthpx = "";
        //                    if (colwidth != 0) widthpx = "width:" + Convert.ToString(colwidth) + "px;";
        //                    if (chk == "")
        //                    { Header = Header + "<th  style='border:1px solid #cac7c7;" + widthpx + "' class='grid_th' > </th>"; }
        //                    else
        //                    if (sd2 == "Double" || sd2 == "Long" || sd2 == "Int")
        //                    { Header = Header + "<th  style='text-align:right;border:1px solid #cac7c7;background-color:dodgerblue;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() + "" + "</th>"; }
        //                    //   { Header = Header + "<th  style='text-align:right;border:1px outset;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() ?? "" + "</th>"; }
        //                    else
        //                    { Header = Header + "<th  style='border:1px solid #cac7c7;background-color:dodgerblue;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() + "" + "</th>"; }
        //                    // { Header = Header + "<th  STYLE='border:1px outset;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() ?? "" + "</th>"; }
        //                }
        //                b = b + 1;
        //            }
        //            if (Header != "")
        //            {
        //                Header = Header + "</tr>";
        //                SB.Append(Header);
        //            }
        //            a = a + 1;
        //        }
        //        Header = "</thead>";
        //        SB.Append(Header);

        //        string Body = "<tbody style='border:1px solid;' >";
        //        SB.Append(Body);
        //        for (Int32 i = 0; i < Table.Rows.Count; i++)
        //        {
        //            string InnerRow = "<tr style='font-size:9pt;'>";
        //            if (pv.AlternetiveRowColor)
        //            {
        //                if (i % 2 == 0) InnerRow = "<tr style='font-size:9pt;background-color:#e2effa;'>";
        //            }
        //            for (int x = 0; x <= Table.Columns.Count - 1; x++)
        //            {
        //                string cellform = getCellDesign(columnNames[x].ToString(), Table.Rows[i]["celldesign"].ToString());
        //                string[] chkn1 = cellform.ToString().Split('~');
        //                string cellstyle = "";
        //                cellform = chkn1[0].ToString();
        //                if (chkn1.Count() > 1) cellstyle = chkn1[1].ToString();
        //                chk = Table.Columns[x].ColumnName.ToString().ToLower();

        //                if (chk.IndexOf("image_") == -1) img = false;
        //                else img = true;

        //                if (chk != "flag" && chk != "celldesign")
        //                {
        //                    if (chk == "dammy")
        //                    {
        //                        if (Table.Rows[i][x].ToString() != "")
        //                        {
        //                            string[] dammyCOLSPAN = Table.Rows[i]["Flag"].ToString().Split('~');
        //                            if (dammyCOLSPAN.Length > 1)
        //                            {
        //                                InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "'  colspan='" + dammyCOLSPAN[1] + "' style='" + dammyCOLSPAN[0] + "'>" + Table.Rows[i][x].ToString() + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + " </td>";
        //                                x += 4;
        //                            }
        //                            else
        //                            {
        //                                InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "' colspan='" + col_span + "' style='" + Table.Rows[i]["Flag"].ToString() + "'>" + Table.Rows[i][x].ToString() + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + "</td>";
        //                                break;
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (Table.Rows[i][x].ToString() != "")
        //                        {
        //                            var sd = Table.Rows[i][x].GetType();
        //                            if (img == true)
        //                            {
        //                                InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "'" + " style='border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'> <img src = '" + Table.Rows[i][x].ToString() + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;' />" + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + "</td>";
        //                            }
        //                            else if (sd == typeof(string)) InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "'" + " style='text-align:left;border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + "</td>";
        //                            else
        //                            {
        //                                var objvalue = Table.Rows[i][x];
        //                                if (cellstyle == "") cellstyle = pv.HeaderArray[0, x].ToString();
        //                                string dspform = getformat(cellstyle);
        //                                string precisionvalue = "";
        //                                precisionvalue = Cn.Indian_Number_format(objvalue.ToString(), dspform);//Convert.ToDouble(objvalue).ToString(dspform);
        //                                InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "'" + " style='text-align:right;border: 1px outset;padding-right: 1px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + precisionvalue + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + "</td>";
        //                            }
        //                        }
        //                        else InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "'" + " style='text-align:left;border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + "</td>";
        //                    }
        //                }
        //            }
        //            InnerRow = InnerRow + "</tr>";
        //            SB.Append(InnerRow);
        //        }
        //        SB.Append("</tbody>");
        //        SB.Append("</table>");
        //        ReportContaint Rc1 = new ReportContaint();
        //        Rc1.GetHtml = SB.ToString();
        //        listemail.Add(Rc1);
        //        PV.SetReportContaint = listemail;
        //        return PV;
        //    }
        //    catch (Exception e)
        //    {
        //        var er = e;
        //        return PV;
        //    }
        //}
        public PrintViewer GetResponsiveHTML(DataTable Table, PrintViewer pv, string[,] headerArray1)
        {
            List<ReportContaint> listemail = new List<ReportContaint>();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();

            int aryA = pv.HeaderArray.GetLength(0);
            int aryB = pv.HeaderArray.GetLength(1);
            string[] columnNames = Table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
            string[] columnTypes = Table.Columns.Cast<DataColumn>().Select(x => x.DataType.ToString()).ToArray();
            string[] span = SpanColumnContaint == null ? null : SpanColumnContaint.Split(',');
            string[] spanLangth = SpanColumnLength == null ? null : SpanColumnLength.Split(',');
            string[] setIndex = SpanColumnSetIndex == null ? null : SpanColumnSetIndex.Split(',');
            try
            {
                string assign_table = "<table class='grid_table'>";
                SB.Append(assign_table);
                //Start Hook Codding To Print Header in Every Page When Print
                string Header = "<thead>";
                string Vname = pv.Vname;
                string Title = pv.Title;
                string Para1 = pv.Para1;
                string Para2 = pv.Para2;
                int col_span = pv.ColspanForPDF;

                Header = Header + "<tr style='height:24px;'> <th colspan='" + col_span + "' style='border: hidden;font-size:17px;font-weight:300; '>" + Vname + "</th> </tr> ";
                Header = Header + "<tr style='height:24px;'> <th colspan='" + col_span + "' style='border: hidden;font-size:17px;font-weight:300;'>" + Title + "</th> </tr> ";

                if (Para1 != "") { Header = Header + "<tr style='height:24px;'> <th colspan='" + col_span + "' style='border: hidden;font-size: 15px;font-weight:100;'>" + Para1 + "</th> </tr> "; }
                if (Para2 != "") { Header = Header + "<tr style='height:24px;'> <th colspan='" + col_span + "' style='border: hidden;font-size: 15px;font-weight:100;'>" + Para2 + "</th> </tr> "; }
                //Header = Header + "<tr> <th colspan='" + col_span + "' style='font-size: 15px;font-weight:100;'></th> </tr> ";

                SB.Append(Header);
                int totalCol = aryB - 4;
                string chk = "";
                bool img;
                int maxA = aryA - 1; int maxB = aryB - 1;
                int a = 1; int b = 0;
                if (span != null)
                {
                    Header = "<tr style='color:white;font-size:12px;font-style: normal;font-weight:bold;height:25px'>";
                    for (int i = 0; i <= totalCol; i++)
                    {
                        int flag = 0;
                        for (int x = 0; x <= setIndex.Length - 1; x++)
                        {
                            if (i.ToString() == setIndex[x].ToString())
                            {
                                Header = Header + "<th class='' colspan='" + spanLangth[x] + "' style='background-color:#808080;text-align:center;border:1px solid white;' valign='middle'>" + span[x] + "</th>";
                                flag = 1;
                                i = i + Convert.ToInt32(spanLangth[x]);
                            }
                        }
                        if (flag == 0)
                        {
                            Header = Header + "<th align='center' class='' style='background-color:#808080;border:1px solid white;'> " + "" + "</th>";
                        }
                        else if (flag == 1)
                        {
                            if (i <= totalCol)
                            {
                                Header = Header + "<th align='center' class='' style='background-color:#808080;border:1px solid white;'>" + "" + "</th>";
                            }
                        }
                    }
                    Header = Header + "</tr>";
                    SB.Append(Header);
                }
                while (a <= maxA)
                {
                    Header = "";
                    Header = Header + "<tr style='font-size:12px;font-style: normal;height:16px'>";
                    b = 0;
                    while (b <= maxB)
                    {
                        chk = Table.Columns[b].ColumnName.ToString().ToLower();

                        if (chk != "flag" && chk != "dammy" && chk != "celldesign")
                        {
                            var sd1 = Table.Rows[0][b].GetType();
                            var sd2 = columnTypes[b].ToString().Replace("System.", "");
                            int colwidth = getCollen(pv.HeaderArray[0, b].ToString());
                            int colmult;
                            if (sd2 == "String")
                            { colmult = 7; }
                            else { colmult = 6; }
                            colwidth = colwidth * colmult;
                            string widthpx = "";
                            if (colwidth != 0) widthpx = "width:" + Convert.ToString(colwidth) + "px;";
                            if (chk == "")
                            { Header = Header + "<th  style='border:1px solid #cac7c7;" + widthpx + "' class='grid_th' > </th>"; }
                            else
                            if (sd2 == "Double" || sd2 == "Long" || sd2 == "Int")
                            { Header = Header + "<th  style='text-align:right;border:1px solid #cac7c7;background-color:dodgerblue;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() + "" + "</th>"; }
                            //   { Header = Header + "<th  style='text-align:right;border:1px outset;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() ?? "" + "</th>"; }
                            else
                            { Header = Header + "<th  style='border:1px solid #cac7c7;background-color:dodgerblue;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() + "" + "</th>"; }
                            // { Header = Header + "<th  STYLE='border:1px outset;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() ?? "" + "</th>"; }
                        }
                        b = b + 1;
                    }
                    if (Header != "")
                    {
                        Header = Header + "</tr>";
                        SB.Append(Header);
                    }
                    a = a + 1;
                }
                Header = "</thead>";
                SB.Append(Header);

                string Body = "<tbody style='border:1px solid;' >";
                SB.Append(Body);
                for (Int32 i = 0; i < Table.Rows.Count; i++)
                {
                    string InnerRow = "<tr style='font-size:9pt;'>";
                    if (pv.AlternetiveRowColor)
                    {
                        if (i % 2 == 0) InnerRow = "<tr style='font-size:9pt;background-color:#e2effa;'>";
                    }
                    for (int x = 0; x <= Table.Columns.Count - 1; x++)
                    {
                        string cellform = getCellDesign(columnNames[x].ToString(), Table.Rows[i]["celldesign"].ToString());
                        string[] chkn1 = cellform.ToString().Split('~');
                        string cellstyle = "";
                        cellform = chkn1[0].ToString();
                        if (chkn1.Count() > 1) cellstyle = chkn1[1].ToString();
                        chk = Table.Columns[x].ColumnName.ToString().ToLower();

                        if (chk.IndexOf("image_") == -1) img = false;
                        else img = true;

                        if (chk != "flag" && chk != "celldesign")
                        {
                            if (chk == "dammy")
                            {
                                if (Table.Rows[i][x].ToString() != "")
                                {
                                    string[] dammyCOLSPAN = Table.Rows[i]["Flag"].ToString().Split('~');
                                    if (dammyCOLSPAN.Length > 1)
                                    {
                                        InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "'  colspan='" + dammyCOLSPAN[1] + "' style='" + dammyCOLSPAN[0] + "'>" + Table.Rows[i][x].ToString() + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + " </td>";
                                        //x += 4;
                                        x += (dammyCOLSPAN[1].retInt() + 2);
                                    }
                                    else
                                    {
                                        InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "' colspan='" + col_span + "' style='" + Table.Rows[i]["Flag"].ToString() + "'>" + Table.Rows[i][x].ToString() + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + "</td>";
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (Table.Rows[i][x].ToString() != "")
                                {
                                    var sd = Table.Rows[i][x].GetType();
                                    if (img == true)
                                    {
                                        InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "'" + " style='border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'> <img src = '" + Table.Rows[i][x].ToString() + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;' />" + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + "</td>";
                                    }
                                    else if (sd == typeof(string)) InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "'" + " style='text-align:left;border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + "</td>";
                                    else
                                    {
                                        var objvalue = Table.Rows[i][x];
                                        if (cellstyle == "") cellstyle = pv.HeaderArray[0, x].ToString();
                                        string dspform = getformat(cellstyle);
                                        string precisionvalue = "";
                                        precisionvalue = Cn.Indian_Number_format(objvalue.ToString(), dspform);//Convert.ToDouble(objvalue).ToString(dspform);
                                        InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "'" + " style='text-align:right;border: 1px outset;padding-right: 1px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + precisionvalue + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + "</td>";
                                    }
                                }
                                else InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "'" + " style='text-align:left;border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + "</td>";
                            }
                        }
                    }
                    InnerRow = InnerRow + "</tr>";
                    SB.Append(InnerRow);
                }
                SB.Append("</tbody>");
                SB.Append("</table>");
                ReportContaint Rc1 = new ReportContaint();
                Rc1.GetHtml = SB.ToString();
                listemail.Add(Rc1);
                PV.SetReportContaint = listemail;
                return PV;
            }
            catch (Exception e)
            {
                var er = e;
                return PV;
            }
        }

        public PrintViewer ShowReport(DataTable IR, string reportname, string header1 = "", string header2 = "", Boolean showfooter = true, Boolean showhdronevrypage = true,
            string orientation = "P", Boolean alternativerowcolor = true, string extr_col = "", string RemoveBlankRowExl = "Y")
        {
            PV.RepetedHeader = showhdronevrypage;
            if (orientation == "P")
            { PV.Portrait = true; }
            else { PV.Portrait = false; }
            PV.IR = IR;
            if (extr_col != "")
            { PV.ColspanForPDF = PV.HeaderArray.GetLength(1) - 4; }
            else { PV.ColspanForPDF = PV.HeaderArray.GetLength(1) - 3; }
            //PV.Title = CommVar.LocName(UNQSNO);
            //PV.Vname = CommVar.CompName(UNQSNO);
            string compdet = GetComptbl();
            PV.Title = compdet.retCompValue("locnm");
            PV.Vname = compdet.retCompValue("compnm");

            PV.Para1 = header1; PV.Para2 = header2;
            PV.AlternetiveRowColor = alternativerowcolor;
            PV = GetResponsiveHTML(IR, PV, headerArray);
            PV.ReportName = reportname;
            if (showfooter == true) PV.StaticFooter = DateTime.Now.ToString("dd/MM/yyyy") + " " + DateTime.Now.ToString("hh:mm:ss tt");
            System.Web.HttpContext.Current.Session[reportname] = PV;
            System.Web.HttpContext.Current.Session[reportname + "_RemoveBlankRowExl"] = RemoveBlankRowExl;

            return PV;
        }

        public int getCollen(string header)
        {
            int decival = 0;
            string[] chkn = header.Split(':');
            string[] chkn1 = chkn[0].ToString().Split(',');
            if (chkn1.Count() < 1) { decival = 0; }
            else { decival = Convert.ToInt16(chkn1[1].ToString()); }
            return decival;
        }

        public int getdouble(string header)
        {
            int decival = 0;
            string[] chkn = header.Split(':');
            string[] chkn1 = chkn[0].ToString().Split(',');
            if (chkn1.Count() <= 2) { decival = 0; }
            else { decival = Convert.ToInt16(chkn1[2].ToString()); }
            return decival;
        }

        public string getCellDesign(string colsearch, string header)
        {
            string dispform = "";
            if (header != "")
            {
                int colpos = header.IndexOf(colsearch + "=");
                if (colpos >= 0)
                {
                    string[] chkn = header.Substring(colpos + colsearch.Length + 1).Split('^');
                    dispform = chkn[0].ToString();
                }
            }
            return dispform;
        }

        public string getformat(string header)
        {
            string dispform = "";
            string[] chkn = header.Split(':');
            if (chkn.Count() <= 1)
            {
                string[] chkn1 = chkn[0].ToString().Split(',');
                int xc = Convert.ToInt16(chkn1[1].ToString());
                int xd = 0;
                if (chkn1.Count() <= 2) { xd = 0; }
                else { xd = Convert.ToInt16(chkn1[2].ToString()); }
                int y = 0;
                xc = xc - 1 - xd;
                dispform = Convert.ToString("").PadLeft(xc, '#') + "0";
                if (xd > 0)
                {
                    dispform = dispform + "." + Convert.ToString("").PadLeft(xd, '0');
                }
            }
            else { dispform = chkn[1].ToString(); }
            return dispform;
        }

        public void GetPrintHeader(DataTable IR, string zfldnm, string zfldtype, string fldstyle = "", string zfldcaption = "")
        {
            zfldtype = zfldtype.ToString().ToLower();
            if (zfldtype == "double")
            { IR.Columns.Add(zfldnm, typeof(double)); }
            else if (zfldtype == "int")
            { IR.Columns.Add(zfldnm, typeof(int)); }
            else if (zfldtype == "long")
            { IR.Columns.Add(zfldnm, typeof(long)); }
            else if (zfldcaption == "date")
            { IR.Columns.Add(zfldnm, typeof(DateTime)); }
            else
            { IR.Columns.Add(zfldnm, typeof(string)); }
            int aryA = PV.HeaderArray.GetLength(0);
            int aryB = PV.HeaderArray.GetLength(1) + 1;//row no
            int colno = IR.Columns.Count;

            string[,] hdArray = new string[aryA, colno];

            for (int x = 0; x < aryA; x++)
            {
                for (int y = 0; y < aryB - 1; y++)
                {
                    hdArray[x, y] = PV.HeaderArray[x, y];
                }
            }
            colno -= 1;

            hdArray[0, colno] = fldstyle;

            string[] fldtmp = zfldcaption.ToString().Split(';');
            if (fldtmp.Count() != 0)
            {
                for (int q = 0; q <= fldtmp.Count() - 1; q++)
                {
                    hdArray[q + 1, colno] = fldtmp[q];
                }
            }
            PV.HeaderArray = hdArray;
            PV.TotReportWidth += (colno * 6);
        }

        public void RepStart(DataTable IR, int noheadrow = 2, string extr_col = "")
        {
            int aryA = noheadrow; int aryB = 3;
            string[,] hdArray = new string[aryA, aryB];
            PV.HeaderArray = hdArray;
            IR.Columns.Add("dammy", typeof(string));
            IR.Columns.Add("flag", typeof(string));
            IR.Columns.Add("celldesign", typeof(string));
            if (extr_col.Count() > 0)
            {
                var extr_col_add = extr_col.Split(',');
                for (int i = 0; i <= extr_col_add.Count() - 1; i++)
                { IR.Columns.Add(extr_col_add[i], typeof(string)); }
            }


        }
        public PrintViewer ShowReport2(DataTable IR, string reportname, string header1 = "", string header2 = "", Boolean showfooter = true, Boolean showhdronevrypage = true,
            string orientation = "P", Boolean alternativerowcolor = true, string RowIDColumnNM = "", bool expand = false, string extr_col = "")
        {
            PV.RepetedHeader = showhdronevrypage;
            if (orientation == "P")
            { PV.Portrait = true; }
            else { PV.Portrait = false; }

            if (extr_col != "")
            { PV.ColspanForPDF = PV.HeaderArray.GetLength(1) - 4; }
            else { PV.ColspanForPDF = PV.HeaderArray.GetLength(1) - 3; }
            PV.AlternetiveRowColor = true;
            //PV.Title = CommVar.LocName(UNQSNO);
            //PV.Vname = CommVar.CompName(UNQSNO);
            string compdet = GetComptbl();
            PV.Title = compdet.retCompValue("locnm");
            PV.Vname = compdet.retCompValue("compnm");
            PV.Para1 = header1; PV.Para2 = header2;

            PV.AlternetiveRowColor = alternativerowcolor;
            PV = GetResponsiveHTML2(IR, PV, headerArray, RowIDColumnNM, expand);

            PV.ReportName = reportname;
            if (showfooter == true) PV.StaticFooter = DateTime.Now.ToString("dd/MM/yyyy") + " " + DateTime.Now.ToString("hh:mm:ss tt");
            return PV;
        }
        public PrintViewer ShowReport1(DataTable IR, string reportname, string header1 = "", string header2 = "", Boolean showfooter = true, Boolean showhdronevrypage = true,
            string orientation = "P", Boolean alternativerowcolor = true, string RowIDColumnNM = "", bool expand = false, string extr_col = "")
        {
            PV.RepetedHeader = showhdronevrypage;
            if (orientation == "P")
            { PV.Portrait = true; }
            else { PV.Portrait = false; }

            if (extr_col != "")
            { PV.ColspanForPDF = PV.HeaderArray.GetLength(1) - 4; }
            else { PV.ColspanForPDF = PV.HeaderArray.GetLength(1) - 3; }
            PV.AlternetiveRowColor = true;
            //PV.Title = CommVar.LocName(UNQSNO);
            //PV.Vname = CommVar.CompName(UNQSNO);
            string compdet = GetComptbl();
            PV.Title = compdet.retCompValue("locnm");
            PV.Vname = compdet.retCompValue("compnm");
            PV.Para1 = header1; PV.Para2 = header2;

            PV.AlternetiveRowColor = alternativerowcolor;
            PV = GetResponsiveHTML1(IR, PV, headerArray, RowIDColumnNM, expand);

            PV.ReportName = reportname;
            if (showfooter == true) PV.StaticFooter = DateTime.Now.ToString("dd/MM/yyyy") + " " + DateTime.Now.ToString("hh:mm:ss tt");
            return PV;
        }

        public PrintViewer ShowReport1(DataTable IR, string reportname, string header1 = "", string header2 = "", Boolean showfooter = true, Boolean showhdronevrypage = true,
           string orientation = "P", Boolean alternativerowcolor = true, int height = 0, string link = "", string pagename = "", string extr_col = "")
        {
            PV.RepetedHeader = showhdronevrypage;
            if (orientation == "P")
            { PV.Portrait = true; }
            else { PV.Portrait = false; }

            if (extr_col != "")
            { PV.ColspanForPDF = PV.HeaderArray.GetLength(1) - 4; }
            else { PV.ColspanForPDF = PV.HeaderArray.GetLength(1) - 3; }

            //PV.Title = CommVar.LocName(UNQSNO);
            //PV.Vname = CommVar.CompName(UNQSNO);
            string compdet = GetComptbl();
            PV.Title = compdet.retCompValue("locnm");
            PV.Vname = compdet.retCompValue("compnm");
            PV.Para1 = header1; PV.Para2 = header2;

            PV.AlternetiveRowColor = alternativerowcolor;
            PV = GetResponsiveHTML1(IR, PV, headerArray, true, height, 0, link, pagename);

            PV.ReportName = reportname;
            if (showfooter == true) PV.StaticFooter = DateTime.Now.ToString("dd/MM/yyyy") + " " + DateTime.Now.ToString("hh:mm:ss tt");
            return PV;
        }
        public PrintViewer GetResponsiveHTML1(DataTable Table, PrintViewer pv, string[,] headerArray1, string RowIDColumnNM, bool expand)
        {
            List<ReportContaint> listemail = new List<ReportContaint>();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            pv.AlternetiveRowColor = true;
            int aryA = pv.HeaderArray.GetLength(0);
            int aryB = pv.HeaderArray.GetLength(1);
            string[] columnNames = Table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
            string[] columnTypes = Table.Columns.Cast<DataColumn>().Select(x => x.DataType.ToString()).ToArray();

            try
            {
                string assign_table = "<table id='zooming' class='grid_table' width='100%'>";
                SB.Append(assign_table);
                //Start Hook Codding To Print Header in Every Page When Print
                string Header = "<thead>";
                string Vname = pv.Vname;
                string Title = pv.Title;
                string Para1 = pv.Para1;
                string Para2 = pv.Para2;
                int col_span = pv.ColspanForPDF;

                //Header = Header + "<tr> <th colspan='" + col_span + "' style='border: hidden;font-size: 17px;font-weight:300; '>" + Vname + "</th> </tr> ";
                //Header = Header + "<tr> <th colspan='" + col_span + "' style='border: hidden;font-size: 17px;font-weight:300;'>" + Title + "</th> </tr> ";

                if (Para1 != "") { Header = Header + "<tr> <th colspan='" + col_span + "' style='border: hidden;font-size: 15px;font-weight:100;'>" + Para1 + "</th> </tr> "; }
                if (Para2 != "") { Header = Header + "<tr> <th colspan='" + col_span + "' style='border: hidden;font-size: 15px;font-weight:100;'>" + Para2 + "</th> </tr> "; }
                //Header = Header + "<tr> <th colspan='" + col_span + "' style='font-size: 15px;font-weight:100;'></th> </tr> ";

                SB.Append(Header);

                string chk = "";
                bool img;
                int maxA = aryA - 1; int maxB = aryB - 1;
                int a = 1; int b = 0;
                while (a <= maxA)
                {
                    Header = "";
                    Header = Header + "<tr style='background-color:#e8e5e5;font-size:12px;font-style: normal;'>";
                    b = 0;
                    while (b <= maxB)
                    {
                        chk = Table.Columns[b].ColumnName.ToString().ToLower();

                        if (chk != "flag" && chk != "dammy" && chk != "celldesign" && chk != "footer" && chk != "doclink")
                        {
                            var sd1 = Table.Rows[0][b].GetType();
                            var sd2 = columnTypes[b].ToString().Replace("System.", "");
                            int colwidth = getCollen(pv.HeaderArray[0, b].ToString());
                            int colmult;
                            if (sd2 == "String")
                            { colmult = 7; }
                            else { colmult = 6; }
                            colwidth = colwidth * colmult;
                            string widthpx = "";
                            if (colwidth != 0) widthpx = "width:" + Convert.ToString(colwidth) + "px;";
                            if (chk == "")
                            {
                                Header = Header + "<th style='border:1px outset;" + widthpx + "' class='grid_th' > </th>";
                            }
                            else
                            if (sd2 == "Double" || sd2 == "Long" || sd2 == "Int")
                            { Header = Header + "<th  style='text-align:right;border:1px outset;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() + "" + "</th>"; }
                            //   { Header = Header + "<th  style='text-align:right;border:1px outset;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() ?? "" + "</th>"; }
                            else
                            { Header = Header + "<th  style='border:1px outset;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() + "" + "</th>"; }
                            // { Header = Header + "<th  STYLE='border:1px outset;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() ?? "" + "</th>"; }
                        }
                        b = b + 1;
                    }
                    if (Header != "")
                    {
                        Header = Header + "</tr>";
                        SB.Append(Header);
                    }
                    a = a + 1;
                }
                Header = "</thead>";
                SB.Append(Header);
                string footer = "";
                string Body = "<tbody style='border:1px solid;' >";
                SB.Append(Body);
                for (Int32 i = 0; i < Table.Rows.Count; i++)
                {
                    bool isfooter = false;
                    if (Table.Rows[i]["footer"].ToString() == "3")
                    {
                        isfooter = true;
                        footer = footer + "<tr>";
                    }
                    string show = "";
                    if (expand == false)
                    {
                        if (Table.Rows[i]["dammy"].ToString().Length <= 0)
                        {
                            if (i == Table.Rows.Count - 1)
                            {
                                show = "";
                            }
                            else
                            {
                                show = "display:none;";
                            }
                        }
                    }
                    string link = Table.Rows[i]["Link"] == null ? "" : Table.Rows[i]["Link"].ToString();
                    string InnerRow = "<tr group='" + Table.Rows[i][RowIDColumnNM] + "' style='font-size:9pt;" + show + (link == "" ? "" : "cursor:pointer;") + "' " + link + ">";
                    if (pv.AlternetiveRowColor)
                    {
                        if (i % 2 == 0)
                        {
                            InnerRow = "<tr group='" + Table.Rows[i][RowIDColumnNM] + "' style='font-size:9pt;background-color:#e2effa;" + show + (link == "" ? "" : "cursor:pointer;") + "' " + link + ">";
                        }
                        else
                        {
                            InnerRow = "<tr group='" + Table.Rows[i][RowIDColumnNM] + "' style='font-size:9pt;" + show + (link == "" ? "" : "cursor:pointer;") + "' " + link + ">";
                        }
                    }
                    for (int x = 0; x <= Table.Columns.Count - 1; x++)
                    {
                        string cellform = getCellDesign(columnNames[x].ToString(), Table.Rows[i]["celldesign"].ToString());
                        string[] chkn1 = cellform.ToString().Split('~');
                        string cellstyle = "";
                        cellform = chkn1[0].ToString();
                        if (chkn1.Count() > 1) cellstyle = chkn1[1].ToString();
                        chk = Table.Columns[x].ColumnName.ToString().ToLower();

                        if (chk.IndexOf("image_") == -1) img = false;
                        else img = true;

                        if (chk != "flag" && chk != "celldesign" && chk != RowIDColumnNM && chk != "link" && chk != "doclink" && chk != "footer")
                        {
                            if (chk == "dammy")
                            {
                                if (Table.Rows[i][x].ToString() != "")
                                {
                                    string[] dammyCOLSPAN = Table.Rows[i]["Flag"].ToString().Split('~');
                                    if (dammyCOLSPAN.Length > 1)
                                    {
                                        if (isfooter)
                                        {
                                            footer = footer + "<td  colspan='" + dammyCOLSPAN[1] + "' style='" + dammyCOLSPAN[0] + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                            x += (2 + Convert.ToInt32(dammyCOLSPAN[1]));
                                        }
                                        else
                                        {
                                            InnerRow = InnerRow + "<td  colspan='" + dammyCOLSPAN[1] + "' style='" + dammyCOLSPAN[0] + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                            x += (2 + Convert.ToInt32(dammyCOLSPAN[1]));
                                        }
                                    }
                                    else
                                    {
                                        if (isfooter)
                                        {
                                            footer = footer + "<td  colspan='" + col_span + "' style='" + Table.Rows[i]["Flag"].ToString() + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                            break;
                                        }
                                        else
                                        {
                                            InnerRow = InnerRow + "<td  colspan='" + col_span + "' style='" + Table.Rows[i]["Flag"].ToString() + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (Table.Rows[i][x].ToString() != "")
                                {
                                    var sd = Table.Rows[i][x].GetType();
                                    if (img == true)
                                    {
                                        if (isfooter)
                                        {
                                            footer = footer + "<td " + " style='border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'> <img src = '" + Table.Rows[i][x].ToString() + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;' /></td>";
                                        }
                                        else
                                        {
                                            InnerRow = InnerRow + "<td " + " style='border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'> <img src = '" + Table.Rows[i][x].ToString() + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;' /></td>";
                                        }
                                    }
                                    else if (sd == typeof(string))
                                    {
                                        if (isfooter)
                                        {
                                            footer = footer + "<td " + " style='text-align:left;border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                        }
                                        else
                                        {
                                            InnerRow = InnerRow + "<td " + " style='text-align:left;border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                        }
                                    }
                                    else
                                    {
                                        var objvalue = Table.Rows[i][x];
                                        if (cellstyle == "") cellstyle = pv.HeaderArray[0, x].ToString();
                                        string dspform = getformat(cellstyle);
                                        string precisionvalue = "";
                                        precisionvalue = Cn.Indian_Number_format(objvalue.ToString(), dspform);//Convert.ToDouble(objvalue).ToString(dspform);
                                        if (isfooter)
                                        {
                                            footer = footer + "<td " + " style='text-align:right;border: 1px outset;padding-right: 1px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + precisionvalue + "</td>";
                                        }
                                        else
                                        {
                                            InnerRow = InnerRow + "<td " + " style='text-align:right;border: 1px outset;padding-right: 1px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + precisionvalue + "</td>";
                                        }
                                    }
                                }
                                else
                                {
                                    if (isfooter)
                                    {
                                        footer = footer + "<td " + " style='text-align:left;border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                    }
                                    else
                                    {
                                        InnerRow = InnerRow + "<td " + " style='text-align:left;border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                    }
                                }
                            }
                        }
                    }
                    if (isfooter)
                    {
                        footer = footer + "</tr>";
                    }
                    else
                    {
                        InnerRow = InnerRow + "</tr>";
                        SB.Append(InnerRow);
                    }
                }
                SB.Append("</tbody>");
                if (footer.Length > 0)
                {
                    footer = "<tfoot>" + footer + "</tfoot>";
                    SB.Append(footer);
                }

                SB.Append("</table>");
                Table.Columns.Remove(RowIDColumnNM);
                ReportContaint Rc1 = new ReportContaint();
                Rc1.GetHtml = SB.ToString();
                listemail.Add(Rc1);
                PV.SetReportContaint = listemail;
                return PV;
            }
            catch (Exception e)
            {
                var er = e;
                return PV;
            }
        }
        public PrintViewer GetResponsiveHTML2(DataTable Table, PrintViewer pv, string[,] headerArray1, string RowIDColumnNM, bool expand)
        {
            List<ReportContaint> listemail = new List<ReportContaint>();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            pv.AlternetiveRowColor = true;
            int aryA = pv.HeaderArray.GetLength(0);
            int aryB = pv.HeaderArray.GetLength(1);
            string[] columnNames = Table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
            string[] columnTypes = Table.Columns.Cast<DataColumn>().Select(x => x.DataType.ToString()).ToArray();

            try
            {
                string assign_table = "<table id='zooming' class='resizable table-striped' cellpadding='0px' cellspacing='0px'  width='100%'>";
                SB.Append(assign_table);
                //Start Hook Codding To Print Header in Every Page When Print
                string Header = "<thead>";
                string Vname = pv.Vname;
                string Title = pv.Title;
                string Para1 = pv.Para1;
                string Para2 = pv.Para2;
                int col_span = pv.ColspanForPDF;

                //Header = Header + "<tr> <th colspan='" + col_span + "' style='border: hidden;font-size: 17px;font-weight:300; '>" + Vname + "</th> </tr> ";
                //Header = Header + "<tr> <th colspan='" + col_span + "' style='border: hidden;font-size: 17px;font-weight:300;'>" + Title + "</th> </tr> ";

                if (Para1 != "") { Header = Header + "<tr> <th colspan='" + col_span + "' style='border: hidden;font-size: 15px;font-weight:100;'>" + Para1 + "</th> </tr> "; }
                if (Para2 != "") { Header = Header + "<tr> <th colspan='" + col_span + "' style='border: hidden;font-size: 15px;font-weight:100;'>" + Para2 + "</th> </tr> "; }
                //Header = Header + "<tr> <th colspan='" + col_span + "' style='font-size: 15px;font-weight:100;'></th> </tr> ";

                SB.Append(Header);

                string chk = "";
                bool img;
                int maxA = aryA - 2; int maxB = aryB - 1;
                int a = 1; int b = 0;
                while (a <= maxA)
                {
                    Header = "";
                    Header = Header + "<tr class='sticky-header' style='background-color:#e8e5e5;font-size:12px;font-style:normal;height:30px;'>";
                    b = 0;
                    while (b <= maxB)
                    {
                        chk = Table.Columns[b].ColumnName.ToString().ToLower();

                        if (chk != "flag" && chk != "dammy" && chk != "celldesign" && chk != "footer" && chk != "doclink")
                        {
                            var sd1 = Table.Rows[0][b].GetType();
                            var sd2 = columnTypes[b].ToString().Replace("System.", "");
                            int colwidth = getCollen(pv.HeaderArray[0, b].ToString());
                            int colmult;
                            if (sd2 == "String")
                            { colmult = 7; }
                            else { colmult = 6; }
                            colwidth = colwidth * colmult;
                            string widthpx = "";
                            if (colwidth != 0) widthpx = "width:" + Convert.ToString(colwidth) + "px;";
                            if (chk == "")
                            {
                                Header = Header + "<th style='" + widthpx + "' class='' > </th>";
                            }
                            else
                            if (sd2 == "Double" || sd2 == "Long" || sd2 == "Int")
                            { Header = Header + "<th  style='text-align:right;padding-right:5px;" + widthpx + "' class='' > " + pv.HeaderArray[a, b]?.ToString() + "" + "</th>"; }
                            //   { Header = Header + "<th  style='text-align:right;border:1px outset;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() ?? "" + "</th>"; }
                            else
                            { Header = Header + "<th  style='" + widthpx + "' class='' > " + pv.HeaderArray[a, b]?.ToString() + "" + "</th>"; }
                            // { Header = Header + "<th  STYLE='border:1px outset;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() ?? "" + "</th>"; }
                        }
                        b = b + 1;
                    }
                    if (Header != "")
                    {
                        Header = Header + "</tr>";
                        SB.Append(Header);
                    }
                    a = a + 1;
                }
                Header = "</thead>";
                SB.Append(Header);
                string footer = "";
                string Body = "<tbody>";
                SB.Append(Body);
                for (Int32 i = 0; i < Table.Rows.Count; i++)
                {
                    bool isfooter = false;
                    if (Table.Rows[i]["footer"].ToString() == "3")
                    {
                        isfooter = true;
                        footer = footer + "<tr class='sticky-footer' style='height:30px;'>";
                    }
                    string show = "";
                    if (expand == false)
                    {
                        if (Table.Rows[i]["dammy"].ToString().Length <= 0)
                        {
                            if (i == Table.Rows.Count - 1)
                            {
                                show = "";
                            }
                            else
                            {
                                show = "display:none;";
                            }
                        }
                    }
                    string link = Table.Rows[i]["Link"] == null ? "" : Table.Rows[i]["Link"].ToString();
                    string InnerRow = "<tr group='" + Table.Rows[i][RowIDColumnNM] + "' style='font-size:9pt;" + show + (link == "" ? "" : "cursor:pointer;") + "' " + link + ">";
                    if (pv.AlternetiveRowColor)
                    {
                        if (i % 2 == 0)
                        {
                            InnerRow = "<tr group='" + Table.Rows[i][RowIDColumnNM] + "' style='font-size:9pt;background-color:#e2effa;" + show + (link == "" ? "" : "cursor:pointer;") + "' " + link + ">";
                        }
                        else
                        {
                            InnerRow = "<tr group='" + Table.Rows[i][RowIDColumnNM] + "' style='font-size:9pt;" + show + (link == "" ? "" : "cursor:pointer;") + "' " + link + ">";
                        }
                    }
                    for (int x = 0; x <= Table.Columns.Count - 1; x++)
                    {
                        string cellform = getCellDesign(columnNames[x].ToString(), Table.Rows[i]["celldesign"].ToString());
                        string[] chkn1 = cellform.ToString().Split('~');
                        string cellstyle = "";
                        cellform = chkn1[0].ToString();
                        if (chkn1.Count() > 1) cellstyle = chkn1[1].ToString();
                        chk = Table.Columns[x].ColumnName.ToString().ToLower();

                        if (chk.IndexOf("image_") == -1) img = false;
                        else img = true;

                        if (chk != "flag" && chk != "celldesign" && chk != RowIDColumnNM && chk != "link" && chk != "doclink" && chk != "footer")
                        {
                            if (chk == "dammy")
                            {
                                if (Table.Rows[i][x].ToString() != "")
                                {
                                    string[] dammyCOLSPAN = Table.Rows[i]["Flag"].ToString().Split('~');
                                    if (dammyCOLSPAN.Length > 1)
                                    {
                                        if (isfooter)
                                        {
                                            footer = footer + "<td  colspan='" + dammyCOLSPAN[1] + "' style='" + dammyCOLSPAN[0] + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                            x += (2 + Convert.ToInt32(dammyCOLSPAN[1]));
                                        }
                                        else
                                        {
                                            InnerRow = InnerRow + "<td  colspan='" + dammyCOLSPAN[1] + "' style='" + dammyCOLSPAN[0] + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                            x += (2 + Convert.ToInt32(dammyCOLSPAN[1]));
                                        }
                                    }
                                    else
                                    {
                                        if (isfooter)
                                        {
                                            footer = footer + "<td  colspan='" + col_span + "' style='" + Table.Rows[i]["Flag"].ToString() + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                            break;
                                        }
                                        else
                                        {
                                            InnerRow = InnerRow + "<td  colspan='" + col_span + "' style='" + Table.Rows[i]["Flag"].ToString() + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (Table.Rows[i][x].ToString() != "")
                                {
                                    var sd = Table.Rows[i][x].GetType();

                                    if (img == true)
                                    {
                                        if (isfooter)
                                        {
                                            footer = footer + "<td " + " style='padding-right:2px;padding-left:2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'> <img src = '" + Table.Rows[i][x].ToString() + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;' /></td>";
                                        }
                                        else
                                        {
                                            InnerRow = InnerRow + "<td " + " style='border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'> <img src = '" + Table.Rows[i][x].ToString() + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;' /></td>";
                                        }
                                    }
                                    else if (sd == typeof(string))
                                    {
                                        if (isfooter)
                                        {
                                            footer = footer + "<td " + " style='text-align:left;padding-right:2px;padding-left:2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                        }
                                        else
                                        {
                                            InnerRow = InnerRow + "<td " + " style='text-align:left;border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                        }
                                    }
                                    else
                                    {
                                        var objvalue = Table.Rows[i][x];
                                        if (cellstyle == "") cellstyle = pv.HeaderArray[0, x].ToString();
                                        string dspform = getformat(cellstyle);
                                        string precisionvalue = "";
                                        precisionvalue = Cn.Indian_Number_format(objvalue.ToString(), dspform);//Convert.ToDouble(objvalue).ToString(dspform);
                                        if (isfooter)
                                        {
                                            footer = footer + "<td " + " style='text-align:right;padding-right:1px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + precisionvalue + "</td>";
                                        }
                                        else
                                        {
                                            InnerRow = InnerRow + "<td " + " style='text-align:right;border: 1px outset;padding-right: 1px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + precisionvalue + "</td>";
                                        }
                                    }
                                }
                                else
                                {
                                    if (isfooter)
                                    {
                                        footer = footer + "<td " + " style='text-align:left;padding-right:2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                    }
                                    else
                                    {
                                        InnerRow = InnerRow + "<td " + " style='text-align:left;border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                    }
                                }
                            }
                        }
                    }
                    if (isfooter)
                    {
                        footer = footer + "</tr>";
                    }
                    else
                    {
                        InnerRow = InnerRow + "</tr>";
                        SB.Append(InnerRow);
                    }
                }
                SB.Append("</tbody>");
                if (footer.Length > 0)
                {
                    footer = "<tfoot>" + footer + "</tfoot>";
                    SB.Append(footer);
                }

                SB.Append("</table>");
                Table.Columns.Remove(RowIDColumnNM);
                ReportContaint Rc1 = new ReportContaint();
                Rc1.GetHtml = SB.ToString();
                listemail.Add(Rc1);
                PV.SetReportContaint = listemail;
                return PV;
            }
            catch (Exception e)
            {
                var er = e;
                return PV;
            }
        }
        public PrintViewer GetResponsiveHTML1(DataTable Table, PrintViewer pv, string[,] headerArray1, bool bodyscroll, int height, int width, string link, string pagename)
        {
            List<ReportContaint> listemail = new List<ReportContaint>();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            pv.AlternetiveRowColor = true;
            int aryA = pv.HeaderArray.GetLength(0);
            int aryB = pv.HeaderArray.GetLength(1);
            string[] columnNames = Table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
            string[] columnTypes = Table.Columns.Cast<DataColumn>().Select(x => x.DataType.ToString()).ToArray();

            try
            {
                string assign_table = "<table class='grid_table'  width='100%'>";
                SB.Append(assign_table);
                //Start Hook Codding To Print Header in Every Page When Print
                string Header = "<thead>";
                string Vname = pv.Vname;
                string Title = pv.Title;
                string Para1 = pv.Para1;
                string Para2 = pv.Para2;
                int col_span = pv.ColspanForPDF;

                //Header = Header + "<tr> <th colspan='" + col_span + "' style='border: hidden;font-size: 17px;font-weight:300; '>" + Vname + "</th> </tr> ";
                //Header = Header + "<tr> <th colspan='" + col_span + "' style='border: hidden;font-size: 17px;font-weight:300;'>" + Title + "</th> </tr> ";

                if (Para1 != "") { Header = Header + "<tr> <th colspan='" + (col_span - 1) + "' style='border: hidden;font-size: 15px;font-weight:100;'>" + Para1 + "</th><th align='right' style='border: hidden;text-align:right;'><img src='../Image/close.png' title='Close' width='16px' height='16px' onclick=pageclose('" + pagename + "');> </img></th> </tr>"; }
                if (Para2 != "") { Header = Header + "<tr> <th colspan='" + col_span + "' style='border: hidden;font-size: 15px;font-weight:100;'>" + Para2 + "</th> </tr> "; }
                // Header = Header + "<tr> <th colspan='" + col_span + "' style='font-size: 15px;font-weight:100;'></th> </tr> ";

                SB.Append(Header);

                string chk = "";
                bool img;
                int maxA = aryA - 1; int maxB = aryB - 1;
                int a = 1; int b = 0;
                while (a <= maxA)
                {
                    Header = "";
                    Header = Header + "<tr style='background-color:#e8e5e5;font-size:12px;font-style: normal;'>";
                    b = 0;
                    while (b <= maxB)
                    {
                        chk = Table.Columns[b].ColumnName.ToString().ToLower();

                        if (chk != "flag" && chk != "dammy" && chk != "celldesign" && chk != link && chk != "doclink")
                        {
                            var sd1 = Table.Rows[0][b].GetType();
                            var sd2 = columnTypes[b].ToString().Replace("System.", "");
                            int colwidth = getCollen(pv.HeaderArray[0, b].ToString());
                            int colmult;
                            if (sd2 == "String")
                            { colmult = 7; }
                            else { colmult = 6; }
                            colwidth = colwidth * colmult;
                            string widthpx = "";
                            if (colwidth != 0) widthpx = "width:" + Convert.ToString(colwidth) + "px;";
                            if (chk == "")
                            { Header = Header + "<th  style='border:1px outset;" + widthpx + "' class='grid_th' > </th>"; }
                            else
                            if (sd2 == "Double" || sd2 == "Long" || sd2 == "Int")
                            { Header = Header + "<th  style='text-align:right;border:1px outset;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() + "" + "</th>"; }
                            //   { Header = Header + "<th  style='text-align:right;border:1px outset;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() ?? "" + "</th>"; }
                            else
                            { Header = Header + "<th  style='border:1px outset;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() + "" + "</th>"; }
                            // { Header = Header + "<th  STYLE='border:1px outset;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() ?? "" + "</th>"; }
                        }
                        b = b + 1;
                    }
                    if (Header != "")
                    {
                        Header = Header + "</tr>";
                        SB.Append(Header);
                    }
                    a = a + 1;
                }
                Header = "</thead>";
                SB.Append(Header);

                string Body = "<tbody style='border:1px solid;' >";
                SB.Append(Body);
                for (Int32 i = 0; i < Table.Rows.Count; i++)
                {
                    string linkcd = Table.Rows[i][link] == null ? "" : Table.Rows[i][link].ToString();
                    string InnerRow = "<tr style='font-size:9pt;" + (linkcd == "" ? "" : "cursor:pointer;") + "' " + linkcd + ">";
                    if (pv.AlternetiveRowColor)
                    {
                        if (i % 2 == 0)
                        {
                            InnerRow = "<tr style='font-size:9pt;background-color:#e2effa;" + (linkcd == "" ? "" : "cursor:pointer;") + "' " + linkcd + ">";
                        }
                        else
                        {
                            InnerRow = "<tr style='font-size:9pt;" + (linkcd == "" ? "" : "cursor:pointer;") + "' " + linkcd + ">";
                        }
                    }
                    for (int x = 0; x <= Table.Columns.Count - 1; x++)
                    {
                        string cellform = getCellDesign(columnNames[x].ToString(), Table.Rows[i]["celldesign"].ToString());
                        string[] chkn1 = cellform.ToString().Split('~');
                        string cellstyle = "";
                        cellform = chkn1[0].ToString();
                        if (chkn1.Count() > 1) cellstyle = chkn1[1].ToString();
                        chk = Table.Columns[x].ColumnName.ToString().ToLower();

                        if (chk.IndexOf("image_") == -1) img = false;
                        else img = true;

                        if (chk != "flag" && chk != "celldesign" && chk != link && chk != "doclink")
                        {
                            if (chk == "dammy")
                            {
                                if (Table.Rows[i][x].ToString() != "")
                                {
                                    string[] dammyCOLSPAN = Table.Rows[i]["Flag"].ToString().Split('~');
                                    if (dammyCOLSPAN.Length > 1)
                                    {
                                        InnerRow = InnerRow + "<td  colspan='" + dammyCOLSPAN[1] + "' style='" + dammyCOLSPAN[0] + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                        x += 4;
                                    }
                                    else
                                    {
                                        InnerRow = InnerRow + "<td  colspan='" + col_span + "' style='" + Table.Rows[i]["Flag"].ToString() + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                var sd1 = Table.Rows[0][x].GetType();
                                var sd2 = columnTypes[x].ToString().Replace("System.", "");
                                int colwidth = getCollen(pv.HeaderArray[0, x].ToString());
                                int colmult;
                                if (sd2 == "String")
                                { colmult = 7; }
                                else { colmult = 6; }
                                colwidth = colwidth * colmult;
                                string widthpx = "";
                                if (colwidth != 0) widthpx = "width:" + Convert.ToString(colwidth) + "px;";
                                if (Table.Rows[i][x].ToString() != "")
                                {
                                    var sd = Table.Rows[i][x].GetType();

                                    if (img == true)
                                    {
                                        InnerRow = InnerRow + "<td " + " style='border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'> <img src = '" + Table.Rows[i][x].ToString() + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;' /></td>";
                                    }
                                    else if (sd == typeof(string)) InnerRow = InnerRow + "<td " + " style='" + widthpx + "text-align:left;border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                    else
                                    {
                                        var objvalue = Table.Rows[i][x];
                                        if (cellstyle == "") cellstyle = pv.HeaderArray[0, x].ToString();
                                        string dspform = getformat(cellstyle);
                                        string precisionvalue = "";
                                        precisionvalue = Cn.Indian_Number_format(objvalue.ToString(), dspform);//Convert.ToDouble(objvalue).ToString(dspform);
                                        InnerRow = InnerRow + "<td " + " style='" + widthpx + "text-align:right;border: 1px outset;padding-right: 1px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + precisionvalue + "</td>";
                                    }
                                }
                                else InnerRow = InnerRow + "<td " + " style='" + widthpx + "text-align:left;border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "</td>";
                            }
                        }
                    }
                    InnerRow = InnerRow + "</tr>";
                    SB.Append(InnerRow);
                }
                SB.Append("</tbody>");
                SB.Append("</table>");
                ReportContaint Rc1 = new ReportContaint();
                Rc1.GetHtml = SB.ToString();
                listemail.Add(Rc1);
                PV.SetReportContaint = listemail;
                return PV;
            }
            catch (Exception e)
            {
                var er = e;
                return PV;
            }
        }
        public PrintViewer ShowReport3(DataTable IR, string reportname, string header1 = "", string header2 = "", Boolean showfooter = true, Boolean showhdronevrypage = true,
          string orientation = "P", Boolean alternativerowcolor = true, int RowIndex = 0, int colIndex = 0, PrintViewer PV2 = null, string extr_col = "")
        {
            PV = PV2;
            PV.RepetedHeader = showhdronevrypage;
            if (orientation == "P")
            { PV.Portrait = true; }
            else { PV.Portrait = false; }
            if (extr_col != "")
            { PV.ColspanForPDF = PV.HeaderArray.GetLength(1) - 4; }
            else { PV.ColspanForPDF = PV.HeaderArray.GetLength(1) - 3; }

            PV.AlternetiveRowColor = true;
            // PV.Title = CommVar.LocName(UNQSNO);
            //  PV.Vname = CommVar.CompName(UNQSNO); 

            PV.Para1 = header1; PV.Para2 = header2;

            PV.AlternetiveRowColor = alternativerowcolor;
            PV = GetResponsiveHTML3(IR, PV, headerArray, RowIndex, colIndex);

            PV.ReportName = reportname;
            if (showfooter == true) PV.StaticFooter = DateTime.Now.ToString("dd/MM/yyyy") + " " + DateTime.Now.ToString("hh:mm:ss tt");
            return PV;
        }
        public PrintViewer GetResponsiveHTML3(DataTable Table, PrintViewer pv, string[,] headerArray1, int RowIndex, int colIndex)
        {
            List<ReportContaint> listemail = new List<ReportContaint>();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            int TotalWidth = 0;
            int backup = 0;
            int aryA = pv.HeaderArray.GetLength(0);
            int aryB = pv.HeaderArray.GetLength(1);
            string[] columnNames = Table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
            string[] columnTypes = Table.Columns.Cast<DataColumn>().Select(x => x.DataType.ToString()).ToArray();
            System.Collections.Hashtable HST = new System.Collections.Hashtable();
            colIndex -= 1;
            RowIndex -= 1;
            try
            {
                string assign_table = "<table id='freezing' class='resizable table-striped' style='sys'>";
                SB.Append(assign_table);
                //Start Hook Codding To Print Header in Every Page When Print
                string Header = "<thead>";
                string Vname = pv.Vname;
                string Title = pv.Title;
                string Para1 = pv.Para1;
                string Para2 = pv.Para2;
                int col_span = pv.ColspanForPDF;

                SB.Append(Header);
                string chk = "";
                bool img;
                int maxA = aryA - 1; int maxB = aryB - 1;
                int a = 1; int b = 0;
                int colcount = 0;
                int top = 0;
                while (a <= maxA)
                {
                    Header = "";
                    Header = Header + "<tr class='sticky-header' id='sticky-header" + a + "' style='background-color:#e8e5e5;font-size:12px;font-style: normal;height:16px'>";
                    b = 0;
                    colcount = 0;
                    backup = 0;
                    while (b <= maxB)
                    {
                        chk = Table.Columns[b].ColumnName.ToString().ToLower();

                        if (chk != "flag" && chk != "dammy" && chk != "celldesign" && chk != "doclink")
                        {
                            var sd1 = Table.Rows[0][b].GetType();
                            var sd2 = columnTypes[b].ToString().Replace("System.", "");
                            int colwidth = getCollen(pv.HeaderArray[0, b].ToString());
                            int colmult;
                            if (sd2 == "String")
                            { colmult = 7; }
                            else { colmult = 6; }
                            colwidth = colwidth * colmult;
                            string widthpx = "";
                            if (colwidth != 0)
                            {
                                if (HST[b] == null)
                                {
                                    HST.Add(b, (colwidth));
                                }
                                else
                                {
                                    HST[b] = (colwidth);
                                }
                                widthpx = "width:" + Convert.ToString(colwidth) + "px;top:" + top + "px;";
                                if (a == 1)
                                {
                                    TotalWidth = TotalWidth + colwidth;
                                }
                            }
                            string classnm = "";
                            if (colcount <= colIndex)
                            {
                                classnm = "class='sticky-cell'";
                                widthpx = widthpx + "left:" + backup + "px;";
                            }
                            if (chk == "")
                            { Header = Header + "<th " + classnm + " style='" + widthpx + "'> </th>"; }
                            else
                            if (sd2 == "Double" || sd2 == "Long" || sd2 == "Int")
                            { Header = Header + "<th " + classnm + " style ='text-align:right;" + widthpx + "'> " + pv.HeaderArray[a, b]?.ToString() + "" + "</th>"; }
                            //   { Header = Header + "<th  style='text-align:right;border:1px outset;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() ?? "" + "</th>"; }
                            else
                            { Header = Header + "<th " + classnm + " style ='" + widthpx + "' > " + pv.HeaderArray[a, b]?.ToString() + "" + "</th>"; }
                            // { Header = Header + "<th  STYLE='border:1px outset;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() ?? "" + "</th>"; }
                            colcount += 1;
                            if (classnm.Length > 0)
                            {
                                backup = backup + (Convert.ToInt32(HST[b]) - 4);
                            }
                        }
                        b = b + 1;
                    }
                    if (Header != "")
                    {
                        Header = Header + "</tr>";
                        SB.Append(Header);
                    }
                    a = a + 1;
                    top += 17;
                }

                for (Int32 i = 0; i <= RowIndex; i++)
                {
                    colcount = 0;
                    backup = 0;
                    string InnerRow = "<tr style='font-size:9pt;'class='sticky-header'>";
                    for (int x = 0; x <= Table.Columns.Count - 1; x++)
                    {
                        string cellform = getCellDesign(columnNames[x].ToString(), Table.Rows[i]["celldesign"].ToString());
                        string[] chkn1 = cellform.ToString().Split('~');
                        string cellstyle = "";
                        cellform = chkn1[0].ToString();
                        if (chkn1.Count() > 1) cellstyle = chkn1[1].ToString();
                        chk = Table.Columns[x].ColumnName.ToString().ToLower();

                        if (chk.IndexOf("image_") == -1) img = false;
                        else img = true;

                        if (chk != "flag" && chk != "celldesign" && chk != "doclink")
                        {
                            if (chk == "dammy")
                            {
                                if (Table.Rows[i][x].ToString() != "")
                                {
                                    string classnm = "";
                                    string left = "";
                                    if (colcount <= colIndex)
                                    {
                                        classnm = "class='sticky-cell'";
                                        left = "";
                                    }
                                    string[] dammyCOLSPAN = Table.Rows[i]["Flag"].ToString().Split('~');
                                    if (dammyCOLSPAN.Length > 1)
                                    {
                                        InnerRow = InnerRow + "<th " + classnm + " id='col1_" + i + "_" + x + "'  colspan='" + dammyCOLSPAN[1] + "' style='" + left + dammyCOLSPAN[0] + "'>" + Table.Rows[i][x].ToString() + " </th>";
                                        x += 4;
                                        colcount += 1;
                                    }
                                    else
                                    {
                                        InnerRow = InnerRow + "<th " + classnm + " id='col1_" + i + "_" + x + "' colspan='" + col_span + "' style='" + left + Table.Rows[i]["Flag"].ToString() + "'>" + Table.Rows[i][x].ToString() + "</th>";
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                string classnm = "";
                                string left = "";
                                if (colcount <= colIndex)
                                {
                                    classnm = "class='sticky-cell'";
                                    left = "left:" + backup + "px;";
                                }
                                if (Table.Rows[i][x].ToString() != "")
                                {
                                    var sd = Table.Rows[i][x].GetType();

                                    if (img == true)
                                    {
                                        if (classnm.Length > 0)
                                        {
                                            InnerRow = InnerRow + "<th " + classnm + " id = 'col1_" + i + "_" + x + "'" + " style='" + left + "top:" + top + "px;padding-right:2px;padding-left: 2px;background-color:#f5f4d8;color:black; " + Table.Rows[i]["Flag"].ToString() + cellform + "'> <img src = '" + Table.Rows[i][x].ToString() + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;' />" + "</th>";
                                        }
                                        else
                                        {
                                            InnerRow = InnerRow + "<th " + classnm + " id = 'col1_" + i + "_" + x + "'" + " style='top:" + top + "px;padding-right: 2px;padding-left: 2px;background-color:#f5f4d8;color:black; " + Table.Rows[i]["Flag"].ToString() + cellform + "'> <img src = '" + Table.Rows[i][x].ToString() + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;' />" + "</th>";
                                        }
                                    }
                                    else if (sd == typeof(string))
                                    {
                                        if (classnm.Length > 0)
                                        {
                                            InnerRow = InnerRow + "<th " + classnm + " id = 'col_" + i + "_" + x + "'" + " style='" + left + "text-align:left;padding-right:2px;background-color:#f5f4d8;color:black;padding-left:2px;top:" + top + "px;" + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "</th>";
                                        }
                                        else
                                        {
                                            InnerRow = InnerRow + "<th " + classnm + " id = 'col_" + i + "_" + x + "'" + " style='text-align:left;padding-right: 2px;background-color:#f5f4d8;color:black;padding-left: 2px;top:" + top + "px;" + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "</th>";
                                        }
                                    }
                                    else
                                    {
                                        var objvalue = Table.Rows[i][x];
                                        if (cellstyle == "") cellstyle = pv.HeaderArray[0, x].ToString();
                                        string dspform = getformat(cellstyle);
                                        string precisionvalue = "";
                                        precisionvalue = Cn.Indian_Number_format(objvalue.ToString(), dspform);//Convert.ToDouble(objvalue).ToString(dspform);
                                        if (classnm.Length > 0)
                                        {
                                            InnerRow = InnerRow + "<th " + classnm + " id = 'col1_" + i + "_" + x + "'" + " style='" + left + "text-align:right;background-color:#f5f4d8;color:black;padding-right:1px;top:" + top + "px;" + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + precisionvalue + "</th>";
                                        }
                                        else
                                        {
                                            InnerRow = InnerRow + "<th " + classnm + " id = 'col1_" + i + "_" + x + "'" + " style='text-align:right;padding-right:1px;top:" + top + "px;background-color:#f5f4d8;color:black;" + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + precisionvalue + "</th>";
                                        }
                                    }
                                }
                                else
                                {
                                    if (classnm.Length > 0)
                                    {
                                        InnerRow = InnerRow + "<th " + classnm + " id = 'col1_" + i + "_" + x + "'" + " style='" + left + "text-align:left;padding-right:2px;background-color:#f5f4d8;color:black;padding-left:2px;top:" + top + "px;" + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "</th>";
                                    }
                                    else
                                    {
                                        InnerRow = InnerRow + "<th " + classnm + " id = 'col1_" + i + "_" + x + "'" + " style='text-align:left;padding-right:2px;padding-left:2px;background-color:#f5f4d8;color:black;top:" + top + "px;" + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "</th>";
                                    }
                                }
                                colcount += 1;
                                if (classnm.Length > 0)
                                {
                                    backup = backup + (Convert.ToInt32(HST[x]) - 4);
                                }
                            }
                        }
                    }
                    InnerRow = InnerRow + "</tr>";
                    SB.Append(InnerRow);
                    top += 15;
                }


                Header = "</thead>";
                SB.Append(Header);
                string gg = SB.ToString();
                string Body = "<tbody>";
                SB.Append(Body);
                int startrow = RowIndex + 1;
                for (Int32 i = startrow; i < Table.Rows.Count; i++)
                {
                    colcount = 0;
                    backup = 0;
                    string InnerRow = "<tr style='font-size:9pt;'>";
                    if (pv.AlternetiveRowColor)
                    {
                        if (i % 2 == 0) InnerRow = "<tr style='font-size:9pt;'>";
                    }
                    for (int x = 0; x <= Table.Columns.Count - 1; x++)
                    {
                        string cellform = getCellDesign(columnNames[x].ToString(), Table.Rows[i]["celldesign"].ToString());
                        string[] chkn1 = cellform.ToString().Split('~');
                        string cellstyle = "";
                        cellform = chkn1[0].ToString();
                        if (chkn1.Count() > 1) cellstyle = chkn1[1].ToString();
                        chk = Table.Columns[x].ColumnName.ToString().ToLower();

                        if (chk.IndexOf("image_") == -1) img = false;
                        else img = true;

                        if (chk != "flag" && chk != "celldesign" && chk != "doclink")
                        {
                            if (chk == "dammy")
                            {
                                if (Table.Rows[i][x].ToString() != "")
                                {
                                    string classnm = "";
                                    string left = "";
                                    if (colcount <= colIndex)
                                    {
                                        classnm = "class='sticky-cell'";
                                        left = "background-color:#f5f4d8;";
                                    }
                                    string[] dammyCOLSPAN = Table.Rows[i]["Flag"].ToString().Split('~');
                                    if (dammyCOLSPAN.Length > 1)
                                    {
                                        InnerRow = InnerRow + "<td " + " id='col1_" + i + "_" + x + "'  colspan='" + dammyCOLSPAN[1] + "' style='" + left + dammyCOLSPAN[0] + "'>" + Table.Rows[i][x].ToString() + " </td>";
                                        x += 4;
                                        colcount += 1;
                                    }
                                    else
                                    {
                                        InnerRow = InnerRow + "<td " + " id='col1_" + i + "_" + x + "' colspan='" + col_span + "' style='" + left + Table.Rows[i]["Flag"].ToString() + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                string classnm = "";
                                string left = "";
                                if (colcount <= colIndex)
                                {
                                    classnm = "class='sticky-cell'";
                                    left = "left:" + backup + "px;background-color:#f5f4d8;";
                                }
                                if (Table.Rows[i][x].ToString() != "")
                                {
                                    var sd = Table.Rows[i][x].GetType();

                                    if (img == true)
                                    {
                                        if (classnm.Length > 0)
                                        {
                                            InnerRow = InnerRow + "<td " + classnm + " id = 'col1_" + i + "_" + x + "'" + " style='" + left + "border-top:1px outset;padding-right:2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'> <img src = '" + Table.Rows[i][x].ToString() + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;' />" + "</td>";
                                        }
                                        else
                                        {
                                            InnerRow = InnerRow + "<td " + classnm + " id = 'col1_" + i + "_" + x + "'" + " style='border-left:1px outset;border-top:1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'> <img src = '" + Table.Rows[i][x].ToString() + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;' />" + "</td>";
                                        }
                                    }
                                    else if (sd == typeof(string))
                                    {
                                        if (classnm.Length > 0)
                                        {
                                            InnerRow = InnerRow + "<td " + classnm + " id = 'col_" + i + "_" + x + "'" + " style='" + left + "text-align:left;padding-right:2px;padding-left:2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                        }
                                        else
                                        {
                                            InnerRow = InnerRow + "<td " + classnm + " id = 'col_" + i + "_" + x + "'" + " style='text-align:left;border-left: 1px outset;border-top:1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                        }
                                    }
                                    else
                                    {
                                        var objvalue = Table.Rows[i][x];
                                        if (cellstyle == "") cellstyle = pv.HeaderArray[0, x].ToString();
                                        string dspform = getformat(cellstyle);
                                        string precisionvalue = "";
                                        precisionvalue = Cn.Indian_Number_format(objvalue.ToString(), dspform);//Convert.ToDouble(objvalue).ToString(dspform);
                                        if (classnm.Length > 0)
                                        {
                                            InnerRow = InnerRow + "<td " + classnm + " id = 'col1_" + i + "_" + x + "'" + " style='" + left + "text-align:right;padding-right:1px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + precisionvalue + "</td>";
                                        }
                                        else
                                        {
                                            InnerRow = InnerRow + "<td " + classnm + " id = 'col1_" + i + "_" + x + "'" + " style='text-align:right;border:1px outset;padding-right:1px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + precisionvalue + "</td>";
                                        }
                                    }
                                }
                                else
                                {
                                    if (classnm.Length > 0)
                                    {
                                        InnerRow = InnerRow + "<td " + classnm + " id = 'col1_" + i + "_" + x + "'" + " style='" + left + "text-align:left;padding-right:2px;padding-left:2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                    }
                                    else
                                    {
                                        InnerRow = InnerRow + "<td " + classnm + " id = 'col1_" + i + "_" + x + "'" + " style='text-align:left;border-left:1px outset;border-top:1px outset;padding-right:2px;padding-left:2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "</td>";
                                    }
                                }
                                colcount += 1;
                                if (classnm.Length > 0)
                                {
                                    backup = backup + (Convert.ToInt32(HST[x]) - 4);
                                }
                            }
                        }
                    }
                    InnerRow = InnerRow + "</tr>";
                    SB.Append(InnerRow);
                }
                SB.Append("</tbody>");
                SB.Append("</table>");
                ReportContaint Rc1 = new ReportContaint();
                Rc1.GetHtml = SB.ToString();
                Rc1.GetHtml = Rc1.GetHtml.ToString().Replace("style='sys'", "style='width:" + TotalWidth + "px'");
                listemail.Add(Rc1);
                PV.SetReportContaint = listemail;
                pv.FreezInnerWidth = (TotalWidth + 27).ToString();
                return PV;
            }
            catch (Exception e)
            {
                var er = e;
                return PV;
            }
        }

        public PrintViewer ShowReportWithImage(DataTable IR, string reportname, string header1 = "", string header2 = "", Boolean showfooter = true, Boolean showhdronevrypage = true,
        string orientation = "P", Boolean alternativerowcolor = true, string Link_Var = "", string extr_col = "")
        {
            PV.RepetedHeader = showhdronevrypage;
            if (orientation == "P")
            { PV.Portrait = true; }
            else { PV.Portrait = false; }
            PV.IR = IR;
            PV.ColspanForPDF = PV.HeaderArray.GetLength(1) - 4;

            //PV.Title = CommVar.LocName(UNQSNO);
            //PV.Vname = CommVar.CompName(UNQSNO);
            string compdet = GetComptbl();
            PV.Title = compdet.retCompValue("locnm");
            PV.Vname = compdet.retCompValue("compnm");
            PV.Para1 = header1; PV.Para2 = header2;

            PV.AlternetiveRowColor = alternativerowcolor;
            PV = GetResponsiveHTMLWithImage(IR, PV, headerArray, Link_Var, extr_col);

            PV.ReportName = reportname;
            if (showfooter == true) PV.StaticFooter = DateTime.Now.ToString("dd/MM/yyyy") + " " + DateTime.Now.ToString("hh:mm:ss tt");
            System.Web.HttpContext.Current.Session[reportname] = PV;
            return PV;
        }

        public PrintViewer GetResponsiveHTMLWithImage(DataTable Table, PrintViewer pv, string[,] headerArray1, string Link_Var, string extr_col = "")
        {
            List<ReportContaint> listemail = new List<ReportContaint>();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();

            int aryA = pv.HeaderArray.GetLength(0);
            int aryB = pv.HeaderArray.GetLength(1);
            string[] columnNames = Table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
            string[] columnTypes = Table.Columns.Cast<DataColumn>().Select(x => x.DataType.ToString()).ToArray();
            string[] span = SpanColumnContaint == null ? null : SpanColumnContaint.Split(',');
            string[] spanLangth = SpanColumnLength == null ? null : SpanColumnLength.Split(',');
            string[] setIndex = SpanColumnSetIndex == null ? null : SpanColumnSetIndex.Split(',');
            try
            {
                string assign_table = "<table class='grid_table'>";
                SB.Append(assign_table);
                //Start Hook Codding To Print Header in Every Page When Print
                string Header = "<thead>";
                string Vname = pv.Vname;
                string Title = pv.Title;
                string Para1 = pv.Para1;
                string Para2 = pv.Para2;
                int col_span = pv.ColspanForPDF;

                Header = Header + "<tr style='height:24px;'> <th colspan='" + col_span + "' style='border: hidden;font-size:17px;font-weight:300; '>" + Vname + "</th> </tr> ";
                Header = Header + "<tr style='height:24px;'> <th colspan='" + col_span + "' style='border: hidden;font-size:17px;font-weight:300;'>" + Title + "</th> </tr> ";

                if (Para1 != "") { Header = Header + "<tr style='height:24px;'> <th colspan='" + col_span + "' style='border: hidden;font-size: 15px;font-weight:100;'>" + Para1 + "   " + " </th></tr> "; }
                if (Para2 != "") { Header = Header + "<tr style='height:24px;'> <th colspan='" + col_span + "' style='border: hidden;font-size: 15px;font-weight:100;'>" + Para2 + "</th> </tr> "; }

                SB.Append(Header);
                int totalCol = aryB - 4;
                string chk = "";
                bool img;
                int maxA = aryA - 1; //int maxB = aryB - 1;
                int maxB = aryB - 1;
                int a = 1; int b = 0;
                if (span != null)
                {
                    Header = "<tr style='color:white;font-size:12px;font-style: normal;font-weight:bold;height:25px'>";
                    for (int i = 0; i <= totalCol; i++)
                    {
                        int flag = 0;
                        for (int x = 0; x <= setIndex.Length - 1; x++)
                        {
                            if (i.ToString() == setIndex[x].ToString())
                            {
                                Header = Header + "<th class='' colspan='" + spanLangth[x] + "' style='background-color:#808080;text-align:center;border:1px solid white;' valign='middle'>" + span[x] + "</th>";
                                flag = 1;
                                i = i + Convert.ToInt32(spanLangth[x]);
                            }
                        }
                        if (flag == 0)
                        {
                            Header = Header + "<th align='center' class='' style='background-color:#808080;border:1px solid white;'> " + "" + "</th>";
                        }
                        else if (flag == 1)
                        {
                            if (i <= totalCol)
                            {
                                Header = Header + "<th align='center' class='' style='background-color:#808080;border:1px solid white;'>" + "" + "</th>";
                            }
                        }
                    }
                    Header = Header + "</tr>";
                    SB.Append(Header);
                }
                while (a <= maxA)
                {
                    Header = "";
                    Header = Header + "<tr style='font-size:12px;font-style: normal;height:16px'>";
                    b = 0;
                    while (b <= maxB)
                    {
                        chk = Table.Columns[b].ColumnName.ToString().ToLower();

                        if (chk != "flag" && chk != "dammy" && chk != "celldesign" && chk != "doclink")
                        {
                            var sd1 = Table.Rows[0][b].GetType();
                            var sd2 = columnTypes[b].ToString().Replace("System.", "");
                            int colwidth = getCollen(pv.HeaderArray[0, b].ToString());
                            int colmult;
                            if (sd2 == "String")
                            { colmult = 7; }
                            else { colmult = 6; }
                            colwidth = colwidth * colmult;
                            string widthpx = "";
                            if (colwidth != 0) widthpx = "width:" + Convert.ToString(colwidth) + "px;";

                            if (chk == "")
                            {
                                Header = Header + "<th  style='border:1px solid #cac7c7;" + widthpx + "' class='grid_th' > </th>";
                            }
                            else {
                                if (sd2 == "Double" || sd2 == "Long" || sd2 == "Int")
                                {
                                    Header = Header + "<th  style='text-align:right;border:1px solid #cac7c7;background-color:dodgerblue;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() + "" + "</th>";
                                }
                                else
                                {
                                    Header = Header + "<th  style='border:1px solid #cac7c7;background-color:dodgerblue;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() + "" + "</th>";
                                }
                            }
                        }

                        b = b + 1;
                    }
                    if (Header != "")
                    {
                        Header = Header + "</tr>";
                        SB.Append(Header);
                    }
                    a = a + 1;
                }
                Header = "</thead>";
                SB.Append(Header);

                string Body = "<tbody style='border:1px solid;' >";
                SB.Append(Body);
                for (Int32 i = 0; i < Table.Rows.Count; i++)
                {
                    string linkcd = Table.Rows[i]["doclink"] == null ? "" : Table.Rows[i]["doclink"].ToString();

                    string InnerRow = "";
                    if (pv.AlternetiveRowColor)
                    {
                        if (i % 2 == 0) InnerRow = "<tr style='font-size:9pt;background-color:#e2effa;" + (linkcd == "" ? "" : "cursor:pointer;") + "' " + linkcd + ">";
                    }
                    for (int x = 0; x <= Table.Columns.Count - 1; x++)
                    {
                        string cellform = getCellDesign(columnNames[x].ToString(), Table.Rows[i]["celldesign"].ToString());
                        string[] chkn1 = cellform.ToString().Split('~');
                        string cellstyle = "";
                        cellform = chkn1[0].ToString();
                        if (chkn1.Count() > 1) cellstyle = chkn1[1].ToString();
                        chk = Table.Columns[x].ColumnName.ToString().ToLower();

                        if (chk == "doclink")
                        { }
                        var LinkVar = Link_Var.Split(',');

                        if (chk.IndexOf("image_") == -1) img = false;
                        else img = true;
                        if (chk != "flag" && chk != "celldesign" && chk != "doclink")
                        {
                            if (chk == "dammy")
                            {
                                if (Table.Rows[i][x].ToString() != "")
                                {
                                    string[] dammyCOLSPAN = Table.Rows[i]["Flag"].ToString().Split('~');
                                    if (dammyCOLSPAN.Length > 1)
                                    {
                                        InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "'  colspan='" + dammyCOLSPAN[1] + "' style='" + dammyCOLSPAN[0] + "'>" + Table.Rows[i][x].ToString() + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + "  </td>";
                                        x += 4;
                                    }
                                    else
                                    {
                                        InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "' colspan='" + col_span + "' style='" + Table.Rows[i]["Flag"].ToString() + "'>" + Table.Rows[i][x].ToString() + " <script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + " </td>";
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (Table.Rows[i][x].ToString() != "")
                                {
                                    var sd = Table.Rows[i][x].GetType();
                                    if (img == true)
                                    {
                                        InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "'" + " style='border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'> <img src = '" + Table.Rows[i][x].ToString() + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;' />" + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + "</td>";
                                    }
                                    else if (sd == typeof(string) && LinkVar.Contains(chk) && Table.Rows[i]["doclink"].ToString() != "")
                                    {
                                        string linkpara = Table.Rows[i]["doclink"].retStr().Replace("'", "\'");
                                        InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "'" + " style='text-align:left;border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'> ";//onclick=ShowPopup();
                                        InnerRow = InnerRow + " <a href='#' onclick=\"CheckDocument(" + linkpara + ",'" + extr_col + "');return false;\">" + Table.Rows[i][x].ToString() + " </a>  <script>Rmenu('col_" + i + "_" + x + "','',1);</script> </td>";
                                    }
                                    else if (sd == typeof(string) && chk != "doclink") InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "'" + " style='text-align:left;border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + " <script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + " </td>";//onclick=ShowPopup();

                                    else
                                    {
                                        var objvalue = Table.Rows[i][x];
                                        if (cellstyle == "") cellstyle = pv.HeaderArray[0, x].ToString();
                                        string dspform = getformat(cellstyle);
                                        string precisionvalue = "";
                                        precisionvalue = Cn.Indian_Number_format(objvalue.ToString(), dspform);//Convert.ToDouble(objvalue).ToString(dspform);
                                        InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "'" + " style='text-align:right;border: 1px outset;padding-right: 1px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + precisionvalue + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + "</td>";
                                    }
                                }
                                else
                                {
                                    InnerRow = InnerRow + "<td id='col_" + i + "_" + x + "'" + " style='text-align:left;border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + "</td>"; //<a href='#' target='_blank'>Document Details
                                }
                            }
                        }

                    }
                    InnerRow = InnerRow + "</tr>";
                    SB.Append(InnerRow);
                }
                SB.Append("</tbody>");
                SB.Append("</table>");
                ReportContaint Rc1 = new ReportContaint();
                Rc1.GetHtml = SB.ToString();
                listemail.Add(Rc1);
                PV.SetReportContaint = listemail;
                return PV;
            }
            catch (Exception e)
            {
                var er = e;
                return PV;
            }
        }

        public PrintViewer ShowReportPopup(DataTable IR, string reportname, string header1 = "", string header2 = "", Boolean showfooter = true, Boolean showhdronevrypage = true,
      string orientation = "P", Boolean alternativerowcolor = true, string header3 = "", string pagename = "", string imgprev_para1 = "", string imgprev_para2 = "")
        {
            PV.RepetedHeader = showhdronevrypage;
            if (orientation == "P")
            { PV.Portrait = true; }
            else { PV.Portrait = false; }

            PV.ColspanForPDF = PV.HeaderArray.GetLength(1);

            //PV.Title = CommVar.LocName(UNQSNO);
            //PV.Vname = CommVar.CompName(UNQSNO);
            string compdet = GetComptbl();
            PV.Title = compdet.retCompValue("locnm");
            PV.Vname = compdet.retCompValue("compnm");

            PV.Para1 = header1; PV.Para2 = header2;
            PV.Para3 = header3;

            PV.AlternetiveRowColor = alternativerowcolor;
            PV = GetResponsiveHTMLPopup(IR, PV, headerArray, pagename, imgprev_para1, imgprev_para2);

            PV.ReportName = reportname;
            if (showfooter == true) PV.StaticFooter = DateTime.Now.ToString("dd/MM/yyyy") + " " + DateTime.Now.ToString("hh:mm:ss tt");
            System.Web.HttpContext.Current.Session[reportname] = PV;
            return PV;
        }
        public PrintViewer GetResponsiveHTMLPopup(DataTable Table, PrintViewer pv, string[,] headerArray1, string pagename = "", string imgprev_para1 = "", string imgprev_para2 = "")
        {
            List<ReportContaint> listemail = new List<ReportContaint>();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();

            int aryA = pv.HeaderArray.GetLength(0);
            int aryB = pv.HeaderArray.GetLength(1);
            string[] columnNames = Table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
            string[] columnTypes = Table.Columns.Cast<DataColumn>().Select(x => x.DataType.ToString()).ToArray();
            string[] span = SpanColumnContaint == null ? null : SpanColumnContaint.Split(',');
            string[] spanLangth = SpanColumnLength == null ? null : SpanColumnLength.Split(',');
            string[] setIndex = SpanColumnSetIndex == null ? null : SpanColumnSetIndex.Split(',');
            try
            {
                string assign_table = "<table class='grid_table'>";
                SB.Append(assign_table);
                //Start Hook Codding To Print Header in Every Page When Print
                string Header = "<thead>";
                string Vname = pv.Vname;
                string Title = pv.Title;
                string Para1 = pv.Para1;
                string Para2 = pv.Para2;
                string Para3 = pv.Para3;
                int col_span = pv.ColspanForPDF;

                Header = Header + "<tr style='height:24px;'> <th colspan='" + col_span + "' style='border: hidden;font-size:17px;font-weight:300; '>" + Vname + "</th><th align='right' style='border: hidden;text-align:right;cursor:pointer;'><img src='../Image/close.png' title='Close' width='16px' height='16px' onclick=PopupPageclose('" + pagename + "');> </img></th> </tr> ";
                Header = Header + "<tr style='height:24px;'> <th colspan='" + col_span + "' style='border: hidden;font-size:17px;font-weight:300;'>" + Title + "</th> </tr> ";

                if (Para1 != "") { Header = Header + "<tr style='height:24px;'> <th colspan='" + col_span + "' style='border: hidden;font-size: 15px;font-weight:100;'>" + Para1 + "   " + " </th></tr> "; }
                if (Para2 != "") { Header = Header + "<tr style='height:24px;'> <th colspan='" + col_span + "' style='border: hidden;font-size: 15px;font-weight:100;'>" + Para2 + "</th> </tr> "; }

                SB.Append(Header);
                int totalCol = aryB - 4;
                string chk = ""; string linkbtn = "";
                bool img; bool link;
                int maxA = aryA - 1; int maxB = aryB - 1;
                int a = 1; int b = 0;
                if (span != null)
                {
                    Header = "<tr style='color:white;font-size:12px;font-style: normal;font-weight:bold;height:25px'>";
                    for (int i = 0; i <= totalCol; i++)
                    {
                        int flag = 0;
                        for (int x = 0; x <= setIndex.Length - 1; x++)
                        {
                            if (i.ToString() == setIndex[x].ToString())
                            {
                                Header = Header + "<th class='' colspan='" + spanLangth[x] + "' style='background-color:#808080;text-align:center;border:1px solid white;' valign='middle'>" + span[x] + "</th>";
                                flag = 1;
                                i = i + Convert.ToInt32(spanLangth[x]);
                            }
                        }
                        if (flag == 0)
                        {
                            Header = Header + "<th align='center' class='' style='background-color:#808080;border:1px solid white;'> " + "" + "</th>";
                        }
                        else if (flag == 1)
                        {
                            if (i <= totalCol)
                            {
                                Header = Header + "<th align='center' class='' style='background-color:#808080;border:1px solid white;'>" + "" + "</th>";
                            }
                        }
                    }
                    Header = Header + "</tr>";
                    SB.Append(Header);
                }
                while (a <= maxA)
                {
                    Header = "";
                    Header = Header + "<tr style='font-size:12px;font-style: normal;height:16px'>";
                    b = 0;
                    while (b <= maxB)
                    {
                        chk = Table.Columns[b].ColumnName.ToString().ToLower();

                        if (chk != "flag" && chk != "dammy" && chk != "celldesign" && chk != "doclink")
                        {
                            var sd1 = Table.Rows[0][b].GetType();
                            var sd2 = columnTypes[b].ToString().Replace("System.", "");
                            int colwidth = getCollen(pv.HeaderArray[0, b].ToString());
                            int colmult;
                            if (sd2 == "String")
                            { colmult = 7; }
                            else { colmult = 6; }
                            colwidth = colwidth * colmult;
                            string widthpx = "";
                            if (colwidth != 0) widthpx = "width:" + Convert.ToString(colwidth) + "px;";
                            if (chk == "")
                            { Header = Header + "<th  style='border:1px solid #cac7c7;" + widthpx + "' class='grid_th' > </th>"; }
                            else
                            if (sd2 == "Double" || sd2 == "Long" || sd2 == "Int")
                            { Header = Header + "<th  style='text-align:right;border:1px solid #cac7c7;background-color:dodgerblue;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() + "" + "</th>"; }
                            else
                            { Header = Header + "<th  style='border:1px solid #cac7c7;background-color:dodgerblue;" + widthpx + "' class='grid_th' > " + pv.HeaderArray[a, b]?.ToString() + "" + "</th>"; }
                        }
                        b = b + 1;
                    }
                    if (Header != "")
                    {
                        Header = Header + "</tr>";
                        SB.Append(Header);
                    }
                    a = a + 1;
                }
                Header = "</thead>";
                SB.Append(Header);

                // string Body = "<tbody style='border:1px solid;' >";
                string Body = "<tbody style='border:1px;' >";
                SB.Append(Body);
                for (Int32 i = 0; i < Table.Rows.Count; i++)
                {
                    string InnerRow = "<tr style='font-size:9pt;'>";
                    if (pv.AlternetiveRowColor)
                    {
                        if (i % 2 == 0) InnerRow = "<tr style='font-size:9pt;background-color:#e2effa;'>";
                    }
                    string str_docno = "", str_docdesc = "";
                    for (int x = 0; x <= Table.Columns.Count - 1; x++)
                    {
                        string cellform = getCellDesign(columnNames[x].ToString(), Table.Rows[i]["celldesign"].ToString());
                        string[] chkn1 = cellform.ToString().Split('~');
                        string cellstyle = "";
                        cellform = chkn1[0].ToString();
                        if (chkn1.Count() > 1) cellstyle = chkn1[1].ToString();
                        chk = Table.Columns[x].ColumnName.ToString().ToLower();

                        if (chk.IndexOf("image_") == -1) img = false;
                        else img = true;

                        if (chk != "flag" && chk != "celldesign" && chk != "doclink")
                        {
                            if (chk == "dammy")
                            {
                                if (Table.Rows[i][x].ToString() != "")
                                {
                                    string[] dammyCOLSPAN = Table.Rows[i]["Flag"].ToString().Split('~');
                                    if (dammyCOLSPAN.Length > 1)
                                    {
                                        InnerRow = InnerRow + "<td id='Pcol_" + i + "_" + x + "'  colspan='" + dammyCOLSPAN[1] + "' style='" + dammyCOLSPAN[0] + "'>" + Table.Rows[i][x].ToString() + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + "  </td>";
                                        x += 4;
                                    }
                                    else
                                    {
                                        InnerRow = InnerRow + "<td id='Pcol_" + i + "_" + x + "' colspan='" + col_span + "' style='" + Table.Rows[i]["Flag"].ToString() + "'>" + Table.Rows[i][x].ToString() + " <script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + " </td>";
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (Table.Rows[i][x].ToString() != "")
                                {
                                    var sd = Table.Rows[i][x].GetType();
                                    // string str = "";
                                    if (chk == imgprev_para1)
                                    { str_docno += Table.Rows[i][x].ToString(); }
                                    if (chk == imgprev_para2)
                                    { str_docdesc += Table.Rows[i][x].ToString(); }

                                    if (img == true)
                                    {
                                        if (Table.Rows[i][x].ToString().Contains("pdf") == true && Table.Rows[i][x].ToString().Contains("data:image") == false)
                                        {
                                            InnerRow = InnerRow + "<td id='Pcol_" + i + "_" + x + "'" + " style='border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'> <img src = '" + Table.Rows[i][x].ToString() + "' id='img_" + i + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;display:none;'onclick=imgpreview('img_" + i + "','" + str_docno + "','" + str_docdesc + "','',''); />";
                                            InnerRow = InnerRow + " <img src = '../Image/pdf_48_icon.png' id='img1_" + i + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;cursor:pointer;'onclick = imgpreview('img_" + i + "','" + str_docno + "','" + str_docdesc + "','',''); />";
                                            InnerRow = InnerRow + "<input id='col_" + i + "_" + x + "' type=hidden value='" + Table.Rows[i][x].ToString() + "'/>" + " <script> Rmenu('col_" + i + "_" + x + "', '', 1);</script>" + "</td> ";
                                        }
                                        else if (Table.Rows[i][x].ToString().Contains("text") == true && Table.Rows[i][x].ToString().Contains("data:image") == false)
                                        {
                                            InnerRow = InnerRow + "<td id='Pcol_" + i + "_" + x + "'" + " style='border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'> <img src = '" + Table.Rows[i][x].ToString() + "' id='img_" + i + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;display:none;'onclick=imgpreview('img_" + i + "','" + str_docno + "','" + str_docdesc + "','',''); />";
                                            InnerRow = InnerRow + " <img src = '../Image/text_48_icon.png' id='img1_" + i + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;cursor:pointer;'onclick = imgpreview('img_" + i + "','" + str_docno + "','" + str_docdesc + "','',''); />";
                                            InnerRow = InnerRow + "<input id='col_" + i + "_" + x + "' type=hidden value='" + Table.Rows[i][x].ToString() + "'/>" + " <script> Rmenu('col_" + i + "_" + x + "', '', 1);</script>" + "</td> ";

                                        }
                                        else if (Table.Rows[i][x].ToString().Contains("openxmlformats") == true && Table.Rows[i][x].ToString().Contains("data:image") == false)
                                        {
                                            InnerRow = InnerRow + "<td id='Pcol_" + i + "_" + x + "'" + " style='border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'> <img src = '" + Table.Rows[i][x].ToString() + "' id='img_" + i + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;display:none;'onclick=imgpreview('img_" + i + "','" + str_docno + "','" + str_docdesc + "','',''); />";
                                            InnerRow = InnerRow + " <img src = '../Image/excel_48_icon.png' id='img1_" + i + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;cursor:pointer;'onclick = imgpreview('img_" + i + "','" + str_docno + "','" + str_docdesc + "','',''); />";
                                            InnerRow = InnerRow + "<input id='col_" + i + "_" + x + "' type=hidden value='" + Table.Rows[i][x].ToString() + "'/>" + " <script> Rmenu('col_" + i + "_" + x + "', '', 1);</script>" + "</td> ";

                                        }
                                        else
                                        {
                                            InnerRow = InnerRow + "<td id='Pcol_" + i + "_" + x + "'" + " style='border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px;cursor:pointer; " + Table.Rows[i]["Flag"].ToString() + cellform + "'> <img src = '" + Table.Rows[i][x].ToString() + "' id='img_" + i + "' width ='40px' height ='35px' style ='max-height:100%; max-width:100%; vertical-align:middle;'onclick=imgpreview('img_" + i + "','" + str_docno + "','" + str_docdesc + "','',''); />";
                                        }
                                    }
                                    else if (sd == typeof(string)) InnerRow = InnerRow + "<td id='Pcol_" + i + "_" + x + "'" + " style='text-align:left;border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + "</td>";//onclick=ShowPopup();

                                    else
                                    {
                                        var objvalue = Table.Rows[i][x];
                                        if (cellstyle == "") cellstyle = pv.HeaderArray[0, x].ToString();
                                        string dspform = getformat(cellstyle);
                                        string precisionvalue = "";
                                        precisionvalue = Cn.Indian_Number_format(objvalue.ToString(), dspform);//Convert.ToDouble(objvalue).ToString(dspform);
                                        InnerRow = InnerRow + "<td id='Pcol_" + i + "_" + x + "'" + " style='text-align:right;border: 1px outset;padding-right: 1px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + precisionvalue + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + "</td>";
                                    }
                                }
                                else InnerRow = InnerRow + "<td id='Pcol_" + i + "_" + x + "'" + " style='text-align:left;border-left: 1px outset;border-top: 1px outset;padding-right: 2px;padding-left: 2px; " + Table.Rows[i]["Flag"].ToString() + cellform + "'>" + Table.Rows[i][x].ToString() + "<script>Rmenu('col_" + i + "_" + x + "','',1);</script>" + "</td>"; //<a href='#' target='_blank'>Document Details


                            }
                        }
                    }
                    InnerRow = InnerRow + "</tr>";
                    SB.Append(InnerRow);
                }
                SB.Append("</tbody>");
                SB.Append("</table>");
                ReportContaint Rc1 = new ReportContaint();
                Rc1.GetHtml = SB.ToString();
                listemail.Add(Rc1);
                PV.SetReportContaint = listemail;
                return PV;
            }
            catch (Exception e)
            {
                var er = e;
                return PV;
            }
        }
        public string GetComptbl()
        {
            string sql = "", scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            sql = "";
            sql += "select b.compnm, b.add1, b.add2, b.add3, b.add4, b.add5, b.add6, b.state, b.country, b.statecd, b.panno, a.tanno, b.cinno, b.propname, ";
            sql += "nvl(a.regdoffsame,'Y') regdoffsame, a.addtype, a.linkloccd, ";
            sql += "a.add1 ladd1, a.add2 ladd2, a.add3 ladd3, a.add4 ladd4, a.add5 ladd5, a.add6 ladd6, ";
            sql += "decode(nvl(a.phno1std,0),0,'',to_char(a.phno1std)||'-')||to_char(a.phno1) phno1, ";
            sql += "decode(nvl(a.phno2std,0),0,'',to_char(a.phno2std)||'-')||to_char(a.phno2) phno2, ";
            sql += "decode(nvl(a.phno3std,0),0,'',to_char(a.phno3std)||'-')||to_char(a.phno3) phno3, ";
            sql += "a.statno_1, a.statno_2, a.statno_3, ";
            sql += "a.state lstate, a.statecd lstatecd, a.country lcountry, a.gstno, a.regemailid, a.regmobile,a.locnm ";
            sql += "from " + scmf + ".m_loca a, " + scmf + ".m_comp b ";
            sql += "where a.compcd='" + COM + "' and a.loccd='" + LOC + "' and a.compcd=b.compcd(+) ";
            DataTable tbl = masterhelp.SQLquery(sql);

            string str = masterhelp.ToReturnFieldValues("", tbl);
            return str;
        }

    }
}