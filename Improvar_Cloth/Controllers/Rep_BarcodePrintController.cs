using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Microsoft.Ajax.Utilities;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;
using System.IO;
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
            barcodeTest();


            return View();
        }
        public void barcodeTest()
        {

            var pgSize = new iTextSharp.text.Rectangle(300f, 400f);
            var doc = new iTextSharp.text.Document(pgSize, 5.5f, 10f, 0.1f, 0.1f);
            try
            {
                string pdfFilePath = System.Web.Hosting.HostingEnvironment.MapPath("/UploadDocuments");
                MemoryStream PDFData = new MemoryStream();
                //PdfWriter writer = PdfWriter.GetInstance(doc, PDFData);
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(pdfFilePath + "/Default.pdf", FileMode.Create));
                doc.Open();

                doc.NewPage();
                //Paragraph paragraph = new Paragraph("Getting Started ITextSharp.");

                string imageURL = Server.MapPath(".") + "/image2.jpg";
                var rt = Cn.GenerateBarcode("001000000780001", "image");
                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(rt);
                Paragraph paragraph1 = new Paragraph("Color:Greem");
                Paragraph paragraph2 = new Paragraph("CHANDERI");
                Paragraph paragraph3 = new Paragraph("AEMB SAREE");


                //Resize image depend upon your need

                jpg.ScaleToFit(140f, 120f);
                //Give space before image

                jpg.SpacingBefore = 10f;
                //Give some space after the image

                jpg.SpacingAfter = 1f;
                jpg.Alignment = Element.ALIGN_LEFT;

                doc.Add(jpg);
                doc.Add(paragraph1);
                doc.Add(paragraph2);
                doc.Add(paragraph3);
                //=================
                //    doc.NewPage();
                Font font2 = new Font(Font.FontFamily.TIMES_ROMAN, 9f);


                PdfPTable table = new PdfPTable(3);

                //    float[] widths = new float[] { 1f, 1f, 1f };

                //    table.TotalWidth = 300f;
                //    table.LockedWidth = true;
                //    table.SetWidths(widths);

                PdfPCell cell = new PdfPCell(new Phrase("Header spanning 3 columns"));

                //    cell.Colspan = 3;

                //    cell.HorizontalAlignment = 0;

                //    table.AddCell(cell);

                //    table.AddCell("Col 1 Row 1");
                //    table.AddCell("Col 2 Row 1");

                //    table.AddCell("Col 3 Row 1");

                //    table.AddCell("Col 1 Row 2");

                //    table.AddCell("Col 2 Row 2");

                //    table.AddCell("Col 3 Row 2");
                //    doc.Add(table);


                //    doc.Add(new Paragraph("Hello World on a new page!"));
                //=================
                doc.NewPage();

                table = new PdfPTable(2);
                table.TotalWidth = 250f;
                //table.LockedWidth = true;

                cell = new PdfPCell();
                cell.AddElement(jpg);
                cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                cell.HorizontalAlignment = 0;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                cell.HorizontalAlignment = 0;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell.AddElement(new Phrase("Color :Red ", font2));
                cell.AddElement(new Phrase("Size:Small", font2));
                cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                table.AddCell(cell);

                cell = new PdfPCell();
                cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                cell.HorizontalAlignment = 0;
                table.AddCell(cell);
                doc.Add(table);

            }
            catch (Exception ex)
            { }
            finally
            {
                doc.Close();
            }
        }
        [HttpPost]
        public ActionResult Rep_BarcodePrint(RepBarcodePrint VE)
        {
            barcodeTest();


            return View();
        }
    }
}