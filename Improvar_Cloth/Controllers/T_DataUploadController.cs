using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Globalization;
//using NDbfReader;
namespace Improvar.Controllers
{
    public class T_DataUploadController : Controller
    {
        string CS = null; string sql = "";
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        Salesfunc Salesfunc = new Salesfunc();
        // GET: T_DataUpload
        public ActionResult T_DataUpload()
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                ViewBag.formname = "Data Upload";
                return View();
            }
            return View();
        }
        [HttpPost]
        public ActionResult T_DataUpload(DataUploadVM VE, FormCollection FC, String Command)
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            //ReadRemondDBF();
            return null;
        }
        public string ReadRaymondPurchaseDBF()
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

                string Path = "C:\\IPSMART\\Temp";
                if (!System.IO.Directory.Exists(Path)) { System.IO.Directory.CreateDirectory(Path); }
                Path = "C:\\IPSMART\\Temp\\Raymond.dbf";
                if (System.IO.File.Exists(Path)) { System.IO.File.Delete(Path); }
                GC.Collect();
                Request.Files["FileUpload"].SaveAs(Path);

                System.Data.Odbc.OdbcConnection obdcconn = new System.Data.Odbc.OdbcConnection();
                obdcconn.ConnectionString = "Driver={Microsoft dBase Driver (*.dbf)};SourceType=DBF;SourceDB=" + Path + ";Exclusive=No; NULL=NO;DELETED=NO;BACKGROUNDFETCH=NO;";
                obdcconn.Open();
                System.Data.Odbc.OdbcCommand oCmd = obdcconn.CreateCommand();
                oCmd.CommandText = "SELECT * FROM " + Path;
                DataTable dbfdt = new DataTable();
                dbfdt.Load(oCmd.ExecuteReader());
                obdcconn.Close();
                if (System.IO.File.Exists(Path)) { System.IO.File.Delete(Path); }

                TransactionSaleEntry TMPVE = new TransactionSaleEntry();
                T_SALEController TSCntlr = new T_SALEController();
                T_TXN TTXN = new T_TXN();
                string auto_no = ""; string Month = "";

                TTXN.EMD_NO = 0;
                TTXN.DOCCD = DB.M_DOCTYPE.Where(d => d.DOCTYPE == "PB").FirstOrDefault()?.DOCCD;


                foreach (DataRow dr in dbfdt.Rows)
                {
                    string CUSTOMERNO = dr["CUSTOMERNO"].ToString();
                    sql = "select slcd from " + CommVar.CurSchema(UNQSNO) + ".m_subleg_com where sapcode='" + CUSTOMERNO + "'";
                    var dt = masterHelp.SQLquery(sql);
                    TTXN.SLCD = dt.Rows[0]["slcd"].ToString();
                    string Ddate = DateTime.ParseExact(dr["INVDATE"].ToString(), "MM/dd/yyyy", CultureInfo.InvariantCulture).ToString("dd/mm/yyyy");
                    TTXN.DOCDT = Convert.ToDateTime(Ddate);
                    TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
                    //TTXN.DOCNO = Cn.MaxDocNumber(TTXN.DOCCD, Ddate);
                    string DOCPATTERN = Cn.DocPattern(Convert.ToInt32(TTXN.DOCNO), TTXN.DOCCD, CommVar.CurSchema(UNQSNO).ToString(), CommVar.FinSchema(UNQSNO), Ddate);
                    auto_no = Cn.Autonumber_Transaction(CommVar.Compcd(UNQSNO), CommVar.Loccd(UNQSNO), TTXN.DOCNO, TTXN.DOCCD, Ddate);
                    TTXN.AUTONO = auto_no.Split(Convert.ToChar(Cn.GCS()))[0].ToString();
                    Month = auto_no.Split(Convert.ToChar(Cn.GCS()))[1].ToString();
                    TTXN.GOCD = "TR";
                    TTXN.DOCTAG ="PB";
                }





            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
            return null;
        }
    }
}