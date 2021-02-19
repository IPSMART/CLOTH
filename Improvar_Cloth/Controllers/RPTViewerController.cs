using System;
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
        public ActionResult GetExcel()
        {
            string ReportName = "";
            var PreviousUrl = Request.UrlReferrer.AbsoluteUri;
            var uri = new Uri(PreviousUrl);//Create Virtually Query String
            var queryString = HttpUtility.ParseQueryString(uri.Query);
            if (queryString.AllKeys.Contains("ReportName"))
            {
                ReportName = queryString.Get("ReportName").ToString();
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
        public ActionResult GetPDF()
        {

            string ReportName = "";
            var PreviousUrl = Request.UrlReferrer.AbsoluteUri;
            var uri = new Uri(PreviousUrl);//Create Virtually Query String
            var queryString = HttpUtility.ParseQueryString(uri.Query);
            if (queryString.AllKeys.Contains("ReportName"))
            {
                ReportName = queryString.Get("ReportName").ToString();
            }
            PrintViewer PV = (Improvar.Models.PrintViewer)TempData[ReportName];
            TempData.Keep();
            string HTML = PV.SetReportContaint[0].GetHtml;

            StyleSheet st = new StyleSheet();
            st.LoadStyle("body", "fontsize", "8");
            //HTML = HTML.Replace("font-size:10px;", "font-size:7px;");
            //HTML = HTML.Replace("font-size:13px;", "font-size:7px;");
            //HTML = HTML.Replace("font-size:14px;", "font-size:9pt;");
            //HTML = HTML.Replace("font-size:17px;", "font-size:9pt;");
            //HTML = HTML.Replace("font-size:9pt;", "font-size:7px;");
            HTML = System.Text.RegularExpressions.Regex.Replace(HTML, "<script.*?</script>", "", System.Text.RegularExpressions.RegexOptions.Singleline | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            //HTML = HTML.Replace("px", "");

            StringReader sr = new StringReader(HTML);
            Document pdfDoc = new Document(PageSize.A2, 10f, 10f, 10f, 0f);

            HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
            MemoryStream memoryStream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
            pdfDoc.Open();
            htmlparser.Parse(sr);
            pdfDoc.Close();
            byte[] bytes = memoryStream.ToArray();
            memoryStream.Close();
            Response.ClearContent();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + MyGlobal.ReportName + ".pdf");
            Response.ContentType = "application/pdf";
            Response.Charset = "";
            Response.OutputStream.Write(bytes, 0, bytes.Length);
            Response.Flush();
            Response.End();
            return PartialView();
        }
        public ActionResult GetFreez(string freezIndex)
        {
            string ReportName = "";
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
            PrintViewer PV = HC.ShowReport3(IR, ReportName, "", "", true, true, "L", false, Convert.ToInt32(rowColindex[1]), Colindex - 3, PV1);
            string HTML1 = PV.SetReportContaint[0].GetHtml;
            PV.SetReportContaint[0].GetHtml = html;
            return Content(HTML1 + "******~~~****" + PV.FreezInnerWidth);
        }
        public ActionResult GetStandardExcel()
        {
            try
            {
                string ReportName = "";
                var PreviousUrl = Request.UrlReferrer.AbsoluteUri;
                var uri = new Uri(PreviousUrl);//Create Virtually Query String
                var queryString = HttpUtility.ParseQueryString(uri.Query);
                if (queryString.AllKeys.Contains("ReportName"))
                {
                    ReportName = queryString.Get("ReportName").ToString();
                }
                bool isdammy = false;
                if (Session[ReportName] != null)
                {
                    PrintViewer PV = (Improvar.Models.PrintViewer)System.Web.HttpContext.Current.Session[ReportName];
                    DataTable newdt = new DataView(PV.IR).ToTable();
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
                                    int eind = dammy.IndexOf("'>");
                                    if (eind != -1)
                                    {
                                        var spanstr = dammy.Substring(0, eind + 2);
                                        dammy = dammy.Replace(spanstr, "");
                                        dammy = dammy.Replace("</span>", "");
                                        dr["dammy"] = dammy;
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
                    }
                    //remove a blank row from datatable
                    newdt = newdt.Rows.Cast<DataRow>().Where(row => !row.ItemArray.All(field => field is DBNull || string.IsNullOrWhiteSpace(field as string))).CopyToDataTable();
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

                    for (int i = 3; i < collength; i++)
                    {
                        hdrvalue = "";
                        for (int j = 1; j < rowslength; j++)
                        {
                            hdrvalue = hdrvalue + PV.HeaderArray[j, i] + " ";

                        }
                        if (isdammy)
                        {
                            worksheet.Cells[3, i - 1].Value = hdrvalue;
                        }
                        else
                        {
                            worksheet.Cells[3, i - 2].Value = hdrvalue;
                        }
                    }
                    worksheet.View.FreezePanes(4, 1);
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
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
            return Content("");
        }
    }

    public class HtmlConverter
    {
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        PrintViewer PV = new PrintViewer();
        Connection Cn = new Connection();
        public static string[,] headerArray;
        public string SpanColumnContaint { get; set; }
        public string SpanColumnLength { get; set; }
        public string SpanColumnSetIndex { get; set; }
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
                                        x += 4;
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
            string orientation = "P", Boolean alternativerowcolor = true)
        {
            PV.RepetedHeader = showhdronevrypage;
            if (orientation == "P")
            { PV.Portrait = true; }
            else { PV.Portrait = false; }
            PV.IR = IR;
            PV.ColspanForPDF = PV.HeaderArray.GetLength(1) - 3;
            PV.Title = CommVar.LocName(UNQSNO);
            PV.Vname = CommVar.CompName(UNQSNO);
            PV.Para1 = header1; PV.Para2 = header2;
            PV.AlternetiveRowColor = alternativerowcolor;
            PV = GetResponsiveHTML(IR, PV, headerArray);
            PV.ReportName = reportname;
            if (showfooter == true) PV.StaticFooter = DateTime.Now.ToString("dd/MM/yyyy") + " " + DateTime.Now.ToString("hh:mm:ss tt");
            System.Web.HttpContext.Current.Session[reportname] = PV;
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

        public void RepStart(DataTable IR, int noheadrow = 2)
        {
            int aryA = noheadrow; int aryB = 3;
            string[,] hdArray = new string[aryA, aryB];
            PV.HeaderArray = hdArray;
            IR.Columns.Add("dammy", typeof(string));
            IR.Columns.Add("flag", typeof(string));
            IR.Columns.Add("celldesign", typeof(string));
        }
        public PrintViewer ShowReport2(DataTable IR, string reportname, string header1 = "", string header2 = "", Boolean showfooter = true, Boolean showhdronevrypage = true,
            string orientation = "P", Boolean alternativerowcolor = true, string RowIDColumnNM = "", bool expand = false)
        {
            PV.RepetedHeader = showhdronevrypage;
            if (orientation == "P")
            { PV.Portrait = true; }
            else { PV.Portrait = false; }

            PV.ColspanForPDF = PV.HeaderArray.GetLength(1) - 3;
            PV.AlternetiveRowColor = true;
            PV.Title = CommVar.LocName(UNQSNO);
            PV.Vname = CommVar.CompName(UNQSNO);

            PV.Para1 = header1; PV.Para2 = header2;

            PV.AlternetiveRowColor = alternativerowcolor;
            PV = GetResponsiveHTML2(IR, PV, headerArray, RowIDColumnNM, expand);

            PV.ReportName = reportname;
            if (showfooter == true) PV.StaticFooter = DateTime.Now.ToString("dd/MM/yyyy") + " " + DateTime.Now.ToString("hh:mm:ss tt");
            return PV;
        }
        public PrintViewer ShowReport1(DataTable IR, string reportname, string header1 = "", string header2 = "", Boolean showfooter = true, Boolean showhdronevrypage = true,
            string orientation = "P", Boolean alternativerowcolor = true, string RowIDColumnNM = "", bool expand = false)
        {
            PV.RepetedHeader = showhdronevrypage;
            if (orientation == "P")
            { PV.Portrait = true; }
            else { PV.Portrait = false; }

            PV.ColspanForPDF = PV.HeaderArray.GetLength(1) - 3;
            PV.AlternetiveRowColor = true;
            PV.Title = CommVar.LocName(UNQSNO);
            PV.Vname = CommVar.CompName(UNQSNO);

            PV.Para1 = header1; PV.Para2 = header2;

            PV.AlternetiveRowColor = alternativerowcolor;
            PV = GetResponsiveHTML1(IR, PV, headerArray, RowIDColumnNM, expand);

            PV.ReportName = reportname;
            if (showfooter == true) PV.StaticFooter = DateTime.Now.ToString("dd/MM/yyyy") + " " + DateTime.Now.ToString("hh:mm:ss tt");
            return PV;
        }

        public PrintViewer ShowReport1(DataTable IR, string reportname, string header1 = "", string header2 = "", Boolean showfooter = true, Boolean showhdronevrypage = true,
           string orientation = "P", Boolean alternativerowcolor = true, int height = 0, string link = "", string pagename = "")
        {
            PV.RepetedHeader = showhdronevrypage;
            if (orientation == "P")
            { PV.Portrait = true; }
            else { PV.Portrait = false; }

            PV.ColspanForPDF = PV.HeaderArray.GetLength(1) - 3;

            PV.Title = CommVar.LocName(UNQSNO);
            PV.Vname = CommVar.CompName(UNQSNO);

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

                        if (chk != "flag" && chk != "dammy" && chk != "celldesign" && chk != "footer")
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

                        if (chk != "flag" && chk != "celldesign" && chk != RowIDColumnNM && chk != "link" && chk != "footer")
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

                        if (chk != "flag" && chk != "dammy" && chk != "celldesign" && chk != "footer")
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

                        if (chk != "flag" && chk != "celldesign" && chk != RowIDColumnNM && chk != "link" && chk != "footer")
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

                        if (chk != "flag" && chk != "dammy" && chk != "celldesign" && chk != link)
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

                        if (chk != "flag" && chk != "celldesign" && chk != link)
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
          string orientation = "P", Boolean alternativerowcolor = true, int RowIndex = 0, int colIndex = 0, PrintViewer PV2 = null)
        {
            PV = PV2;
            PV.RepetedHeader = showhdronevrypage;
            if (orientation == "P")
            { PV.Portrait = true; }
            else { PV.Portrait = false; }

            PV.ColspanForPDF = PV.HeaderArray.GetLength(1) - 3;
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

                        if (chk != "flag" && chk != "celldesign")
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

                        if (chk != "flag" && chk != "celldesign")
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

    }
}