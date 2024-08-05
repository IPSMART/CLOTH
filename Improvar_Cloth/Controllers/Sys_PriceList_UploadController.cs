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
using OfficeOpenXml.Style;
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
            if (Command == "Download Template")
            {
                string resp = DownloadTemplatePricelist(VE);
                return Content(resp);
            }
            else {
                if (Request.Files.Count == 0) return Content("No File Selected");
                HttpPostedFileBase file = Request.Files["UploadedFile"];
                if (System.IO.Path.GetExtension(file.FileName) != ".xlsx") return Content(".xlsx file need to choose");
                string resp = ReadRaymondPricelist(VE, file.InputStream);
                return Content(resp);
            }
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
                        string grpnm = workSheet.Cells[row, 1].Value.ToString();
                        string style = workSheet.Cells[row, 2].Value.ToString() + workSheet.Cells[row, 3].Value.ToString().Split('-')[0];
                        string HSNCODE = workSheet.Cells[row, 7].Value.ToString();
                        ItemDet ItemDet = Salesfunc.CreateItem(style, "MTR", grpnm, HSNCODE, "", "", "F", "C", "");
                        if (ItemDet.ITCD.retStr() == "")
                        {
                            if (ItemDet.ErrMsg.retStr() != "")
                            {
                                return ItemDet.ErrMsg + " row:" + row;
                            }
                            else
                            {
                                return "Please add style:(" + style + ") at Item Master Manually because master transfer done in next year of  row:" + row;
                            }
                        }
                        sql = "SELECT * FROM " + CommVar.CurSchema(UNQSNO) + ".T_BATCHMST_PRICE where barno ='" + ItemDet.BARNO + "' and EFFDT=to_date('" + VE.TDT + "','dd/mm/yyyy') ";
                        var dt = masterHelp.SQLquery(sql);
                        if (dt.Rows.Count > 0) continue;
                        double CP = workSheet.Cells[row, 4].Value.retDbl();
                        double WP = workSheet.Cells[row, 5].Value.retDbl();
                        double RP = workSheet.Cells[row, 6].Value.retDbl();
                        msg = Salesfunc.CreatePricelist(ItemDet.BARNO, VE.TDT, CP, WP, RP);
                        if (msg != "ok")
                        {
                            return "Failed because of row:" + row + ",  " + msg;
                        }
                        msg = row.ToString();
                        //row++;
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
        public string DownloadTemplatePricelist(ReportViewinHtml VE)
        {
            try
            {
                string Excel_Header = "Description" + "|" + "Material Group" + "|" + "Material" + "|" + "Ex factory price" + "|" + "Whosale Price" + "|" + "Retail Price"
                    + "|" + "HSN CODE" + "|" + "GST" + "|";

                ExcelPackage ExcelPkg = new ExcelPackage();
                ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("Sheet1");

                using (ExcelRange Rng = wsSheet1.Cells["A1:H1"])
                {
                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    string[] Header = Excel_Header.Split('|');
                    for (int i = 0; i < Header.Length; i++)
                    {
                        wsSheet1.Cells[1, i + 1].Value = Header[i];
                    }
                }
                wsSheet1.Cells[1, 1, 1, 8].AutoFitColumns();

                Response.Clear();
                Response.ClearContent();
                Response.Buffer = true;
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("Content-Disposition", "attachment; filename=Price list.xlsx");
                Response.BinaryWrite(ExcelPkg.GetAsByteArray());
                Response.Flush();
                Response.Close();
                Response.End();
                return "Download Sucessfull";
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return ex.Message + ex.InnerException + "  ";
            }
            return null;
        }

    }
}
