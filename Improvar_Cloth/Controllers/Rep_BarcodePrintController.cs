using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using Improvar.DataSets;
using System.Collections.Generic;
using System.Linq;

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
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Barcode Printing";
                    RepBarcodePrint VE;
                    if (TempData["printparameter"] == null)
                    {
                        VE = new RepBarcodePrint();
                    }
                    else
                    {
                        VE = (RepBarcodePrint)TempData["printparameter"];
                    }
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    var Schnm = CommVar.CurSchema(UNQSNO);
                    ////GenerateBarcode();
                    ////barcodeTest();
                    VE.DropDown_list1 = (from i in DB.M_REPFORMAT
                                         select new DropDown_list1()
                                         { value = i.REPTYPE, text = i.REPTYPE }).Distinct().OrderBy(s => s.text).ToList();
                    var sql = "select distinct '123456789' BARNO,a.SLNO,b.itnm FABITNM ,b.STYLENO,c.ITGRPNM  from " + Schnm + ".t_txndtl a," + Schnm + ".M_SITEM b," + Schnm + ".M_GROUP c where a.itcd=b.itcd(+) and b.itgrpcd=c.itgrpcd(+)";
                    DataTable ttxndtl = masterHelp.SQLquery(sql);
                    VE.BarcodePrint = (from DataRow dr in ttxndtl.Rows
                                       select new BarcodePrint()
                                       {
                                           TAXSLNO = dr["SLNO"].retStr(),
                                           BARNO = dr["BARNO"].retStr(),
                                           ITGRPNM = dr["ITGRPNM"].retStr(),
                                           FABITNM = dr["FABITNM"].retStr(),
                                           STYLENO = dr["STYLENO"].retStr()
                                       }).Distinct().OrderBy(s => s.TAXSLNO).ToList();
                    VE.DefaultView = true;
                    VE.ExitMode = 1;
                    VE.DefaultDay = 0;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }



        [HttpPost]
        public ActionResult Rep_BarcodePrint(RepBarcodePrint VE)
        {
            DataTable IR = new DataTable("DataTable1");
            IR.Columns.Add("brcodeImage", typeof(byte[]));
            IR.Columns.Add("barno", typeof(string));
            IR.Columns.Add("compinit", typeof(string));
            IR.Columns.Add("itgrpnm", typeof(string));
            IR.Columns.Add("itnm", typeof(string));
            IR.Columns.Add("design", typeof(string));

            IR.Columns.Add("pdesign", typeof(string));

            IR.Columns.Add("mtr", typeof(string));

            IR.Columns.Add("colrnm", typeof(string));

            IR.Columns.Add("sizenm", typeof(string));

            IR.Columns.Add("txslno", typeof(string));

            IR.Columns.Add("wpprice", typeof(string));

            IR.Columns.Add("wppricecode", typeof(string));

            IR.Columns.Add("rpprice", typeof(string));

            IR.Columns.Add("rppricecode", typeof(string));

            IR.Columns.Add("cost", typeof(string));

            IR.Columns.Add("costcode", typeof(string));

            IR.Columns.Add("docno", typeof(string));

            IR.Columns.Add("docdt", typeof(string));

            IR.Columns.Add("prefno", typeof(string));

            IR.Columns.Add("prefdt", typeof(string));

            IR.Columns.Add("docdtcode", typeof(string));

            for (int i = 0; i < VE.BarcodePrint.Count; i++)
            {
                if (VE.BarcodePrint[i].Checked == true)
                {
                    for (int j = 0; j < VE.BarcodePrint[i].NOS.retInt(); j++)
                    {
                        DataRow dr = IR.NewRow();
                        string barno = VE.BarcodePrint[i].BARNO.retStr();
                        byte[] brcodeImage = (byte[])Cn.GenerateBarcode(barno, "byte");
                        dr["brcodeImage"] = brcodeImage;
                        dr["barno"] = barno;
                        dr["compinit"] = "";
                        dr["itgrpnm"] = "";
                        dr["itnm"] = "";
                        dr["design"] = "";
                        dr["pdesign"] = "";
                        dr["mtr"] = "";
                        dr["colrnm"] = "";
                        dr["sizenm"] = "";
                        dr["txslno"] = "";
                        dr["wpprice"] = "";
                        dr["rpprice"] = "";
                        dr["rppricecode"] = "";
                        dr["cost"] = "";
                        dr["costcode"] = "";
                        dr["docno"] = "";
                        dr["docdt"] = "";
                        dr["prefno"] = "";
                        dr["prefdt"] = "";
                        dr["docdtcode"] = "";
                        IR.Rows.Add(dr);
                    }
                }
            }
            string rptfile = "PrintBarcode";
            string rptname = "~/Report/" + rptfile + ".rpt";

            ReportDocument reportdocument = new ReportDocument();
            reportdocument.Load(Server.MapPath(rptname));
            DSPrintBarcode DSP = new DSPrintBarcode();
            DSP.Merge(IR);
            reportdocument.SetDataSource(DSP);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);
            reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
            return new FileStreamResult(stream, "application/pdf");
        }
    }
}