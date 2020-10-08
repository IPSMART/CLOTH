using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using Improvar.DataSets;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class Rep_BarcodePrintController : Controller
    {
        Connection Cn = new Connection(); MasterHelp masterHelp = new MasterHelp();
        Salesfunc salesfunc = new Salesfunc(); DataTable DTNEW = new DataTable();
        EmailControl EmailControl = new EmailControl();
        T_TXN TXN; T_TXNTRANS TXNTRN; T_TXNOTH TXNOTH; T_CNTRL_HDR TCH; T_CNTRL_HDR_REM SLR; T_TXN_LINKNO TTXNLINKNO;
        SMS SMS = new SMS();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_BarcodePrint
        public ActionResult Rep_BarcodePrint()
        {
            //GenerateBarcode();
            //barcodeTest();
            RepBarcodePrint VE = new RepBarcodePrint();
            Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            VE.DropDown_list1 = (from i in DB.M_REPFORMAT
                                 select new DropDown_list1()
                                 { value = i.CTGCD, text = i.CTGNM }).Distinct().OrderBy(s => s.text).ToList();
            VE.DefaultView = true;
            return View(VE);
        }

        //public void GenerateBarcode()
        //{
        //    var rt = Cn.GenerateBarcode("00100000078pp0001", "image");
        //    iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(rt);

        //    string sql = "SELECT * FROM " + CommVar.CurSchema(UNQSNO) + ".M_BarcodePrintFormat";
        //    DataTable DT = masterHelp.SQLquery(sql);
        //    int width = DT.Rows[0]["width"].retInt();
        //    int height = DT.Rows[0]["height"].retInt();
        //    int marginetop = DT.Rows[0]["marginetop"].retInt();
        //    int marginebottom = DT.Rows[0]["marginebottom"].retInt();
        //    int margineleft = DT.Rows[0]["margineleft"].retInt();
        //    int margineright = DT.Rows[0]["margineright"].retInt();
        //    Font.FontFamily FF = Font.FontFamily.TIMES_ROMAN;
        //    int STICKERPERPAGE = DT.Rows[0]["STICKERPERPAGE"].retInt() == 0 ? 1 : DT.Rows[0]["STICKERPERPAGE"].retInt();


        //    sql = "SELECT * FROM " + CommVar.CurSchema(UNQSNO) + ".M_BarcodePrintFormatdtl ORDER BY SLNO";
        //    DataTable formatdt = masterHelp.SQLquery(sql);
        //    //var pgSize = new iTextSharp.text.Rectangle(width, 300, 400f);
        //    var pgSize = new iTextSharp.text.Rectangle(width, height);
        //    //var doc = new iTextSharp.text.Document(pgSize, 1, 10f, 0.1f, 0.1f);
        //    var doc = new iTextSharp.text.Document(pgSize, margineleft, margineright, marginetop, marginebottom);
        //    try
        //    {
        //        string pdfFilePath = System.Web.Hosting.HostingEnvironment.MapPath("/UploadDocuments");
        //        MemoryStream PDFData = new MemoryStream();
        //        //PdfWriter writer = PdfWriter.GetInstance(doc, PDFData);
        //        PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(pdfFilePath + "/Default024.pdf", FileMode.Create));
        //        doc.Open();
        //        doc.NewPage();
        //        PdfPTable maintable = new PdfPTable(STICKERPERPAGE);
        //        maintable.WidthPercentage = 100;
        //        maintable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;


        //        PdfPTable table = new PdfPTable(1);
        //        table.DefaultCell.Padding = 0;
        //        PdfPCell cell = new PdfPCell();
        //        Font font = new Font(Font.FontFamily.TIMES_ROMAN, 7, 2);
        //        for (int row = 0; row < formatdt.Rows.Count; row++)
        //        {
        //            string linevalue = formatdt.Rows[row]["FMTTEXT"].retStr();
        //            int paddingtop = formatdt.Rows[row]["paddingtop"].retInt();
        //            int paddingleft = formatdt.Rows[row]["PADDINGLEFT"].retInt();
        //            int fontsize = formatdt.Rows[row]["fontsize"].retInt();
        //            int fontstyle = formatdt.Rows[row]["fontstyle"].retInt();
        //            if (linevalue == "<BARIMAGE>")
        //            {
        //                cell = new PdfPCell();
        //                jpg.ScaleAbsolute(159f, 159f);
        //                cell.AddElement(jpg);
        //                cell.Border = Rectangle.NO_BORDER;
        //                cell.HorizontalAlignment = 0;
        //                table.AddCell(cell);
        //            }
        //            else if (linevalue == "<SZNM>")
        //            {
        //                cell = new PdfPCell();
        //                cell.AddElement(new Phrase("SZNM :2323wsfsdfsdfs ", font));
        //                cell.UseAscender = true;
        //                cell.PaddingBottom = 0;
        //                cell.Bottom = 0;
        //                cell.Padding = 0f;
        //                cell.Border = Rectangle.NO_BORDER;
        //                table.AddCell(cell);
        //            }
        //            else if (linevalue == "<ITNM>")
        //            {
        //                font = new Font(Font.FontFamily.TIMES_ROMAN, fontsize, fontstyle);
        //                cell = new PdfPCell();
        //                cell.AddElement(new Phrase("ITNM :dsdsdvsdfsd dsd ", font));
        //                cell.UseAscender = true;
        //                cell.PaddingTop = paddingtop;
        //                cell.PaddingLeft = paddingleft;
        //                cell.Border = Rectangle.NO_BORDER;
        //                table.AddCell(cell);
        //            }
        //        }
        //        //Font font1 = new Font(Font.FontFamily.TIMES_ROMAN, 7, 2);
        //        table.DefaultCell.Border = 0;
        //        //Bind our custom event to the default cell
        //        //table.DefaultCell.CellEvent = new CellSpacingEvent(2);
        //        //We're not changing actual layout so we're going to cheat and padd the cells a little


        //        //table.TotalWidth = 250f;
        //        table.TotalWidth = width / 2;
        //        table.LockedWidth = true;



        //        cell = new PdfPCell();
        //        cell.AddElement(new Phrase("Color :Red ", font));
        //        cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
        //        cell.Top = 0;
        //        cell.Bottom = 0;
        //        //cell.Right (0);// (0f);
        //        //cell.setPaddingLeft(20f);
        //        table.AddCell(cell);

        //        cell = new PdfPCell();
        //        cell.AddElement(new Phrase("Color :Ressd ", font));
        //        cell.Border = iTextSharp.text.Rectangle.BOX;
        //        cell.UseAscender = true;
        //        cell.PaddingBottom = 0;
        //        cell.Bottom = 0;
        //        cell.Padding = 0f;
        //        table.AddCell(cell);

        //        cell = new PdfPCell();
        //        cell.AddElement(new Phrase("Size :Red ", font));
        //        cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
        //        cell.Padding = 0f;
        //        cell.Top = -50f;
        //        cell.PaddingTop = 0; //cell.Table.PaddingTop = 0;
        //        cell.SetLeading(table.DefaultCell.Leading, -10f);
        //        table.AddCell(cell);

        //        maintable.AddCell(table);
        //        //=================================

        //        table = new PdfPTable(1);
        //        table.DefaultCell.Border = 0;
        //        //Bind our custom event to the default cell
        //        //table.DefaultCell.CellEvent = new CellSpacingEvent(2);
        //        //We're not changing actual layout so we're going to cheat and padd the cells a little
        //        table.DefaultCell.Padding = 0;

        //        //table.TotalWidth = 250f;
        //        table.TotalWidth = width / 2;
        //        table.LockedWidth = true;

        //        cell = new PdfPCell();
        //        cell.AddElement(jpg);
        //        cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
        //        cell.HorizontalAlignment = 0;
        //        table.AddCell(cell);

        //        cell = new PdfPCell();
        //        cell.AddElement(new Phrase("Color :Red ", font));
        //        cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
        //        cell.Top = 0;
        //        cell.Bottom = 0;
        //        //cell.Right (0);// (0f);
        //        //cell.setPaddingLeft(20f);
        //        table.AddCell(cell);

        //        cell = new PdfPCell();
        //        cell.AddElement(new Phrase("Color :Ressd ", font));
        //        cell.Border = iTextSharp.text.Rectangle.BOX;
        //        cell.UseAscender = true;
        //        cell.PaddingBottom = 0;
        //        cell.Bottom = 0;
        //        cell.Padding = 0f;
        //        table.AddCell(cell);

        //        cell = new PdfPCell();
        //        cell.AddElement(new Phrase("Size :Red ", font));
        //        cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
        //        cell.Padding = 0f;
        //        cell.Top = -50f;
        //        cell.PaddingTop = 0; //cell.Table.PaddingTop = 0;
        //        cell.SetLeading(table.DefaultCell.Leading, -10f);
        //        table.AddCell(cell);

        //        maintable.AddCell(table);
        //        doc.Add(maintable);
        //    }
        //    catch (Exception ex)
        //    { }
        //    finally
        //    {
        //        doc.Close();
        //    }
        //}
        //public void barcodeTest()
        //{

        //    var pgSize = new iTextSharp.text.Rectangle(300f, 400f);
        //    var doc = new iTextSharp.text.Document(pgSize, 5.5f, 10f, 0.1f, 0.1f);
        //    try
        //    {
        //        string pdfFilePath = System.Web.Hosting.HostingEnvironment.MapPath("/UploadDocuments");
        //        MemoryStream PDFData = new MemoryStream();
        //        //PdfWriter writer = PdfWriter.GetInstance(doc, PDFData);
        //        PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(pdfFilePath + "/Default.pdf", FileMode.Create));
        //        doc.Open();

        //        doc.NewPage();
        //        //Paragraph paragraph = new Paragraph("Getting Started ITextSharp.");

        //        string imageURL = Server.MapPath(".") + "/image2.jpg";
        //        var rt = Cn.GenerateBarcode("001000000pp780001", "image");
        //        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(rt);
        //        Paragraph paragraph1 = new Paragraph("Color:Greem");
        //        Paragraph paragraph2 = new Paragraph("CHANDERI");
        //        Paragraph paragraph3 = new Paragraph("AEMB SAREE");


        //        //Resize image depend upon your need

        //        jpg.ScaleToFit(140f, 120f);
        //        //Give space before image

        //        jpg.SpacingBefore = 10f;
        //        //Give some space after the image

        //        jpg.SpacingAfter = 1f;
        //        jpg.Alignment = Element.ALIGN_LEFT;

        //        doc.Add(jpg);
        //        doc.Add(paragraph1);
        //        doc.Add(paragraph2);
        //        doc.Add(paragraph3);
        //        //=================
        //        //    doc.NewPage();
        //        Font font2 = new Font(Font.FontFamily.TIMES_ROMAN, 9f);


        //        PdfPTable table = new PdfPTable(3);

        //        //    float[] widths = new float[] { 1f, 1f, 1f };

        //        //    table.TotalWidth = 300f;
        //        //    table.LockedWidth = true;
        //        //    table.SetWidths(widths);

        //        PdfPCell cell = new PdfPCell(new Phrase("Header spanning 3 columns"));

        //        //    cell.Colspan = 3;

        //        //    cell.HorizontalAlignment = 0;

        //        //    table.AddCell(cell);

        //        //    table.AddCell("Col 1 Row 1");
        //        //    table.AddCell("Col 2 Row 1");

        //        //    table.AddCell("Col 3 Row 1");

        //        //    table.AddCell("Col 1 Row 2");

        //        //    table.AddCell("Col 2 Row 2");

        //        //    table.AddCell("Col 3 Row 2");
        //        //    doc.Add(table);


        //        //    doc.Add(new Paragraph("Hello World on a new page!"));
        //        //=================
        //        doc.NewPage();

        //        table = new PdfPTable(2);
        //        table.TotalWidth = 250f;
        //        //table.LockedWidth = true;

        //        cell = new PdfPCell();
        //        cell.AddElement(jpg);
        //        cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
        //        cell.HorizontalAlignment = 0;
        //        table.AddCell(cell);

        //        cell = new PdfPCell();
        //        cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
        //        cell.HorizontalAlignment = 0;
        //        table.AddCell(cell);

        //        cell = new PdfPCell();
        //        cell.AddElement(new Phrase("Color :Red ", font2));
        //        cell.AddElement(new Phrase("Size:Small", font2));
        //        cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
        //        table.AddCell(cell);

        //        cell = new PdfPCell();
        //        cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
        //        cell.HorizontalAlignment = 0;
        //        table.AddCell(cell);
        //        doc.Add(table);

        //    }
        //    catch (Exception ex)
        //    { }
        //    finally
        //    {
        //        doc.Close();
        //    }
        //}
        [HttpPost]
        public ActionResult Rep_BarcodePrint(RepBarcodePrint VE)
        {

            string rptfile = "PrintBarcode";
            string rptname = "~/Report/" + rptfile + ".rpt";

            DataTable IR = new DataTable("DataTable1");
            IR.Columns.Add("brcodeImage", typeof(byte[]));
            IR.Columns.Add("itnm", typeof(string));
            var byteimg = Cn.GenerateBarcode("1234515"+System.DateTime.Now.ToString("ddmmyyyyss"), "byte");
            byte[] imgb = (byte[])byteimg;

            var byteimg1 = Cn.GenerateBarcode("123456789012", "byte");
            byte[] imgb1 = (byte[])byteimg1;

            IR.Rows.Add(imgb, "hook");
            byte[] imgb12 = Cn.GenerateBarcode("1235" + System.DateTime.Now.ToString("ddmmyyyyss"), "byte");

            IR.Rows.Add(imgb12, "hook");
            IR.Rows.Add(imgb1, "hook");

            //var byteimg1 = Cn.GenerateBarcode("12345678901234P", "byte");
           
            //ReportDocument crystalReport = new ReportDocument();
            //crystalReport.Load(Server.MapPath("~/EmployeesReport.rpt"));
            DSPrintBarcode dsEmployees=new DSPrintBarcode();
            dsEmployees.Merge(IR);
            //crystalReport.SetDataSource(dsEmployees);
            //CrystalReportViewer1.ReportSource = crystalReport;


            ReportDocument reportdocument = new ReportDocument();
            reportdocument.Load(Server.MapPath(rptname));

            reportdocument.SetDataSource(dsEmployees);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);
            reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
            return new FileStreamResult(stream, "application/pdf");


            return View();
        }
    }
}