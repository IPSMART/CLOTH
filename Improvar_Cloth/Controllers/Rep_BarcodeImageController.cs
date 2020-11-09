using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using CrystalDecisions.CrystalReports.Engine;
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
            barcodeTest(VE);
            VE.DefaultView = true;
            return View(VE);
        }
        public void barcodeTest(RepBarcodeImage VE)
        {
            int COLPERPAGE = VE.COLPERPAGE.retInt() == 0 ? 1 : VE.COLPERPAGE.retInt();

       
            DataTable DtBarImage = (DataTable)Session["DtRepBarcodeImage"];
            var sql = "select * from " + scm1 + ".m_batch_img_hdr where barno='1700001'";

            sql = "(select a.barno, count(*) barimagecount, ";
            sql += "listagg(a.doc_flname||'~'||a.doc_desc,chr(179)) ";
            sql += "within group (order by a.barno) as barimage from ";
            //sql += "listagg(a.imgbarno||chr(181)||a.imgslno||chr(181)||a.doc_flname||chr(181)||a.doc_extn||chr(181)||substr(a.doc_desc,50),chr(179)) ";
            sql += "(select a.barno, a.imgbarno, a.imgslno, b.doc_flname, b.doc_extn, b.doc_desc from ";
            sql += "(select a.barno, a.barno imgbarno, a.slno imgslno ";
            sql += "from " + scm1 + ".m_batch_img_hdr a ";
            sql += "union ";
            sql += "select a.barno, b.barno imgbarno, b.slno imgslno ";
            sql += "from " + scm1 + ".m_batch_img_hdr_link a, " + scm1 + ".m_batch_img_hdr b ";
            sql += "where a.mainbarno=b.barno(+) ) a, ";
            sql += "" + scm1 + ".m_batch_img_hdr b ";
            sql += "where a.imgbarno=b.barno(+) and a.imgslno=b.slno(+) ";
            sql += "union ";
            sql += "select a.barno, a.imgbarno, a.imgslno, b.doc_flname, b.doc_extn, b.doc_desc from ";
            sql += "(select a.barno, a.barno imgbarno, a.slno imgslno ";
            sql += "from " + scm1 + ".t_batch_img_hdr a ";
            sql += "union ";
            sql += "select a.barno, b.barno imgbarno, b.slno imgslno ";
            sql += "from " + scm1 + ".t_batch_img_hdr_link a, " + scm1 + ".t_batch_img_hdr b ";
            sql += "where a.mainbarno=b.barno(+) ) a, ";
            sql += "" + scm1 + ".t_batch_img_hdr b ";
            sql += "where a.imgbarno=b.barno(+) and a.imgslno=b.slno(+) ) a ";
            sql += "group by a.barno ) y, ";


            var dt = masterHelp.SQLquery(sql);
            var path = CommVar.SaveFolderPath() + "/ItemImages/" + dt.Rows[0]["doc_flname"].ToString();
            path = Path.Combine(path, "");
            byte[] imgdata = System.IO.File.ReadAllBytes(path);

            float Pwidth = CommFunc.MMtoPointFloat(210);
            var pgSize = new iTextSharp.text.Rectangle(CommFunc.MMtoPointFloat(210), CommFunc.MMtoPointFloat(297));
            var doc = new iTextSharp.text.Document(pgSize, 5.5f, 10f, 0.1f, 0.1f);
            try
            {
                string pdfFilePath = System.Web.Hosting.HostingEnvironment.MapPath("/UploadDocuments");
                MemoryStream PDFData = new MemoryStream();
                //PdfWriter writer = PdfWriter.GetInstance(doc, PDFData);
                PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(pdfFilePath + "/Default.pdf", FileMode.Create));
                doc.Open();
                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imgdata);
                Font font2 = new Font(Font.FontFamily.TIMES_ROMAN, 9f   );
                PdfPTable maintable = new PdfPTable(COLPERPAGE);//NO OF COLUMN
                maintable.WidthPercentage = 100;
                //table.TotalWidth = 300f;
                //table.LockedWidth = true;
                maintable.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                PdfPTable table = new PdfPTable(1);
                PdfPCell cell = new PdfPCell(new Phrase("Header spanning 3 columns"));
                doc.NewPage();
                for (int i = 0; i < COLPERPAGE + 8; i++)
                {
                    table = new PdfPTable(1);

                    cell = new PdfPCell();
                    cell.AddElement(jpg);
                    cell.Border = iTextSharp.text.Rectangle.NO_BORDER;
                    cell.HorizontalAlignment = 0;
                    table.AddCell(cell);

                    cell = new PdfPCell();
                    cell.AddElement(new Phrase("Color :Red ", font2));
                    cell.UseAscender = true;
                    cell.PaddingTop = 0f;
                    cell.Border = Rectangle.NO_BORDER;
                    table.AddCell(cell);

                    cell = new PdfPCell();
                    cell.AddElement(new Phrase( "Size:Small", font2));
                    cell.UseAscender = true;
                    cell.PaddingTop = 3f;
                    cell.Border = Rectangle.NO_BORDER;
                    table.AddCell(cell);
                    maintable.AddCell(table);
                }
                doc.Add(maintable);
            }
            catch (Exception ex)
            { }
            finally
            {
                doc.Close();
            }
        }
    }
}