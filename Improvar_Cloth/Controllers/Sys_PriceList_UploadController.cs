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
                        ItemDet ItemDet = Salesfunc.CreateItem(style, "MTR", grpnm, HSNCODE, "", "F", "C");
                        sql = "SELECT * FROM " + CommVar.CurSchema(UNQSNO) + ".M_ITEMPLISTDTL where barno='" + ItemDet.BARNO + "' and EFFDT=to_date('" + VE.TDT + "','dd/mm/yyyy') ";
                        var dt = masterHelp.SQLquery(sql);
                        if (dt.Rows.Count > 0) continue;
                        double CP = workSheet.Cells[row, 5].Value.retDbl();
                        double WP = workSheet.Cells[row, 5].Value.retDbl();
                        double RP = workSheet.Cells[row, 5].Value.retDbl();
                        msg = CreatePricelist(ItemDet.BARNO, VE.TDT, CP, WP, RP);
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
        private string CreatePricelist(string BARNO, string EFFDT, double CPRate, double WPRate, double RPRate)
        {
            try
            {
                M_ITEMPLISTDTL MIP = new M_ITEMPLISTDTL();
                MIP.EMD_NO = 0;
                MIP.CLCD = CommVar.ClientCode(UNQSNO);
                MIP.EFFDT = Convert.ToDateTime(EFFDT);
                MIP.BARNO = BARNO;
                OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
                OraCon.Open();
                OracleCommand OraCmd = OraCon.CreateCommand();
                using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    MIP.PRCCD = "CP";
                    MIP.RATE = CPRate;
                    var dbsql = masterHelp.RetModeltoSql(MIP, "A", CommVar.CurSchema(UNQSNO));
                    var dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    MIP.PRCCD = "WP";
                    MIP.RATE = WPRate;
                    dbsql = masterHelp.RetModeltoSql(MIP, "A", CommVar.CurSchema(UNQSNO));
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();

                    MIP.PRCCD = "RP";
                    MIP.RATE = RPRate;
                    dbsql = masterHelp.RetModeltoSql(MIP, "A", CommVar.CurSchema(UNQSNO));
                    dbsql1 = dbsql.Split('~'); OraCmd.CommandText = dbsql1[0]; OraCmd.ExecuteNonQuery();
                    OraTrans.Commit();
                }
                OraCon.Dispose();
                return "ok";
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, BARNO);
                return ex.Message;
            }
        }
    }
}
