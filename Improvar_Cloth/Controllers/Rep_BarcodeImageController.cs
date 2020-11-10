using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace Improvar.Controllers
{
    public class Rep_BarcodeImageController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        //BarCode CnBarCode = new BarCode();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        string sql = "";
        string LOC = CommVar.Loccd(CommVar.getQueryStringUNQSNO()), COM = CommVar.Compcd(CommVar.getQueryStringUNQSNO()), scm1 = CommVar.CurSchema(CommVar.getQueryStringUNQSNO()), scmf = CommVar.FinSchema(CommVar.getQueryStringUNQSNO());

        // GET: Rep_BarcodeImage
        public ActionResult Rep_BarcodeImage(string reptype = "")
        {
            Improvar.ViewModels.RepBarcodeImage VE = new Improvar.ViewModels.RepBarcodeImage();
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "BarCode Image";
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
                    {
                        VE.COLPERPAGE = 4;
                        VE.DefaultView = true;
                        VE.ExitMode = 1;
                        VE.DefaultDay = 0;
                        return View(VE);
                    }
                }
            }
            catch (Exception ex)
            {
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        [HttpPost]
        public ActionResult Rep_BarcodeImage(RepBarcodeImage VE)
        {
            try
            {
                int COLPERPAGE = VE.COLPERPAGE.retInt() == 0 ? 1 : VE.COLPERPAGE.retInt();
                DataTable DtBarImage = (DataTable)Session["DtRepBarcodeImage"];//BARNO,DOC_FLNAME,INE1,LINE2   

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    var pgSize = new iTextSharp.text.Rectangle(CommFunc.MMtoPointFloat(210), CommFunc.MMtoPointFloat(297));
                    var doc = new iTextSharp.text.Document(pgSize, 5.5f, 10f, 0.1f, 0.1f);
                    PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                    doc.Open();
                    Font font2 = new Font(Font.FontFamily.TIMES_ROMAN, 9f);
                    PdfPTable maintable = new PdfPTable(COLPERPAGE);//NO OF COLUMN
                    maintable.WidthPercentage = 100;
                    //table.TotalWidth = 300f;
                    //table.LockedWidth = true;
                    maintable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    doc.NewPage();
                    PdfPCell cell = new PdfPCell(new Phrase(CommVar.CompName(UNQSNO)));
                    cell.Border = Rectangle.NO_BORDER;
                    cell.Colspan = COLPERPAGE;
                    maintable.AddCell(cell);

                    cell = new PdfPCell(new Phrase(CommVar.LocName(UNQSNO)));
                    cell.Border = Rectangle.NO_BORDER;
                    cell.Colspan = COLPERPAGE;
                    maintable.AddCell(cell);

                    cell = new PdfPCell(new Phrase("Sales data"));
                    cell.Border = Rectangle.NO_BORDER;
                    cell.Colspan = COLPERPAGE;
                    maintable.AddCell(cell);

                    for (int i = 0; i < DtBarImage.Rows.Count; i++)
                    {
                        PdfPTable table = new PdfPTable(1);
                        ///////////
                        var path = CommVar.SaveFolderPath() + "/ItemImages/" + DtBarImage.Rows[i]["doc_flname"].ToString();
                        path = Path.Combine(path, "");
                        byte[] imgdata = System.IO.File.ReadAllBytes(path);
                        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imgdata);
                        //////design//////////
                        cell = new PdfPCell();
                        cell.AddElement(jpg);
                        cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                        cell.HorizontalAlignment = 0;
                        table.AddCell(cell);

                        cell = new PdfPCell();
                        cell.AddElement(new Phrase(DtBarImage.Rows[i]["LINE1"].ToString(), font2));
                        cell.UseAscender = true;
                        cell.PaddingTop = 0f;
                        cell.Border = Rectangle.NO_BORDER;
                        table.AddCell(cell);

                        cell = new PdfPCell();
                        cell.AddElement(new Phrase(DtBarImage.Rows[i]["LINE2"].ToString(), font2));
                        cell.UseAscender = true;
                        cell.PaddingTop = 3f;
                        cell.Border = Rectangle.NO_BORDER;
                        table.AddCell(cell);
                        maintable.AddCell(table);
                    }
                    doc.Add(maintable);
                    doc.Close();
                    //AddPageNumber;
                    byte[] result = ms.ToArray();
                    Font blackFont = FontFactory.GetFont("Arial", 12, Font.NORMAL, BaseColor.BLACK);
                    using (MemoryStream stream = new MemoryStream())
                    {
                        PdfReader reader = new PdfReader(result);
                        using (PdfStamper stamper = new PdfStamper(reader, stream))
                        {
                            int pages = reader.NumberOfPages;
                            for (int i = 1; i <= pages; i++)
                            {
                                ColumnText.ShowTextAligned(stamper.GetUnderContent(i), Element.ALIGN_RIGHT, new Phrase(i.ToString() + " of " + pages, blackFont), 568f, 15f, 0);
                            }
                        }
                        result = stream.ToArray();
                    }
                    Response.Buffer = false;
                    Response.ClearContent();
                    Response.ClearHeaders();
                    Stream outstream = new MemoryStream(result);
                    outstream.Seek(0, SeekOrigin.Begin);
                    GC.Collect();
                    return new FileStreamResult(outstream, "application/pdf");
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