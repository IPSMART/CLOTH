using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Globalization;
using Oracle.ManagedDataAccess.Client;
using OfficeOpenXml;
using System.IO;
//using NDbfReader;
namespace Improvar.Controllers
{
    public class Sys_PriceList_UploadController : Controller
    {
        string CS = null; string sql = ""; string dbsql = ""; string[] dbsql1;
        string dberrmsg = "";
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        Salesfunc Salesfunc = new Salesfunc();
        private ImprovarDB DB, DBF;
        public Sys_PriceList_UploadController()
        {
            DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
        }
        // GET: Sys_PriceList_Upload
        public ActionResult Sys_PriceList_Upload()
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                ViewBag.formname = "Data Upload";
                ReportViewinHtml VE = new ReportViewinHtml();
                return View(VE);
            }
        }
        [HttpPost]
        public ActionResult Sys_PriceList_Upload(ReportViewinHtml VE, FormCollection FC, String Command)
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            if (Request.Files.Count == 0) return Content("No File Selected");
            HttpPostedFileBase file = Request.Files["UploadedFile"];
            if (System.IO.Path.GetExtension(file.FileName) != ".xlsx") return Content(".xlsx file need to choose");
            string resp = ReadRaymondPricelist(VE, file.InputStream);
            return Content(resp);
        }
        public string ReadRaymondPricelist(ReportViewinHtml VE, Stream stream)
        {
            string msg = "";
            try
            {
                using (var package = new ExcelPackage(stream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    int row = 2;
                    for (row = 2; row <= noOfRow; row++)
                    {
                        string grpnm = workSheet.Cells[row, 2].Value.ToString();
                        string style = workSheet.Cells[row, 3].Value.ToString() + workSheet.Cells[row, 4].Value.ToString();
                        string HSNCODE = workSheet.Cells[row, 5].Value.ToString();
                        ItemDet ItemDet = Salesfunc.CreateItem(style, "MTR", grpnm, HSNCODE, "","", "F", "C");
                        sql = "SELECT * FROM " + CommVar.CurSchema(UNQSNO) + ".M_ITEMPLISTDTL where barno='" + ItemDet.BARNO + "' and EFFDT=to_date('" + VE.TDT + "','dd/mm/yyyy') ";
                        var dt = masterHelp.SQLquery(sql);
                        if (dt.Rows.Count > 0) continue;
                        double CP = workSheet.Cells[row, 5].Value.retDbl();
                        double WP = workSheet.Cells[row, 5].Value.retDbl();
                        double RP = workSheet.Cells[row, 5].Value.retDbl();
                        msg = Salesfunc.CreatePricelist(ItemDet.BARNO, VE.TDT, CP, WP, RP);
                        if (msg != "ok")
                        {
                            return "Failed because of row:" + row + ",  " + msg;
                        }
                        msg = row.ToString();
                        row++;
                    }
                }
                return "Uploaded Successfully ! ";
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, msg);
            }
            return "Failed because of row:" + msg;
        }
  
    }
}
