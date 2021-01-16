using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.ViewModels;
using System.Data;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Improvar.Controllers
{
    public class Rep_OrdPrintController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp Master_Help = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_OrdPrint
        public ActionResult Rep_OrdPrint()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Order Printing";
                    ReportViewinHtml VE;
                    if (TempData["printparameter"] == null)
                    {
                        VE = new ReportViewinHtml();
                    }
                    else
                    {
                        VE = (ReportViewinHtml)TempData["printparameter"];
                    }
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    if (VE.DOCCD == "")
                    {
                        VE.DocumentType = Cn.DOCTYPE1("SORD");
                    }
                    else
                    {
                        VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                    }
                    VE.TEXTBOX4 = "50";
                    VE.Checkbox1 = true;
                    VE.Checkbox3 = false;
                    VE.Checkbox4 = true;
                    VE.DefaultView = true;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                ReportViewinHtml VE = new ReportViewinHtml();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public ActionResult DOCNO_help(string Code, string val)
        {
            //if (val == null)
            //{
            //    return PartialView("_Help2", Master_Help.DOCNO_SORD_help(Code, val));
            //}
            //else
            //{
            //    string str = Master_Help.DOCNO_SORD_help(Code, val);
            return Content("");
            //}
        }

        public ActionResult GetBuyerDetails(string val)
        {
            if (val == null)
            {
                return PartialView("_Help2", Master_Help.SUBLEDGER(val, "D"));
            }
            else
            {
                string str = Master_Help.SUBLEDGER(val, "D");
                return Content(str);
            }
        }

        [HttpPost]
        public ActionResult Rep_OrdPrint(ReportViewinHtml VE, FormCollection FC)
        {
            string msg = ""; double minimumStockCoalculatedOn = 15;
            try
            {
                DataTable rstbl;
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string doccd = VE.DOCCD, slcd = VE.TEXTBOX1, fdt = VE.FDT, tdt = VE.TDT, fdocno = VE.FDOCNO, tdocno = VE.TDOCNO;
                if (slcd != null) slcd = "'" + slcd + "'";


                string sql = "";
                sql += "select a.SLNO,a.AUTONO, a.STKDRCR, a.STKTYPE, a.FREESTK, a.ITCD, c.ITNM, c.STYLENO, c.PCSPERSET, c.UOMCD, ";
                sql += "b.docno,b.docdt,i.slcd,H.SLNM,h.add1||' '||h.add2||' '||h.add3||' '||h.add4||' '||h.add5||' '||h.add6||' '||h.add7 address,";
                sql += "a.sizecd, a.rate, a.scmdiscamt, a.discamt, a.qnty,A.DELVDT,a.ITREM,a.PDESIGN,c.itgrpcd, d.itgrpnm,c.fabitcd, ";
                sql += "e.itnm fabitnm,a.colrcd,a.partcd,f.colrnm,g.sizenm,h.partnm,a.rate from ";
                sql += scm + ".T_SORDDTL a, " + scm + ".T_CNTRL_HDR b, ";
                sql += scm + ".m_sitem c, " + scm + ".m_group d, " + scm + ".m_sitem e, " + scm + ".m_color f, " + scm + ".m_size g, " + scm + ".m_parts h, "
                    + scm + ".T_SORD i," + scmf + ".m_subleg h  ";
                sql += "where a.autono = b.autono and a.autono = i.autono and  a.itcd = c.itcd(+) and c.itgrpcd=d.itgrpcd and c.fabitcd=e.itcd(+) ";
                sql += "and a.colrcd=f.colrcd(+) and a.sizecd=g.sizecd(+) and a.partcd=h.partcd(+) and i.slcd= h.slcd and ";
                sql += "nvl(b.cancel,'N')='N' and b.compcd='" + COM + "' and b.loccd='" + LOC + "' and ";
                if (slcd != null) sql += "a.slcd in (" + slcd + ") and ";
                if (fdt != null) sql += "b.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and ";
                if (tdt != null) sql += "b.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and ";
                if (fdocno != null) sql += "b.doconlyno >= '" + fdocno + "' and ";
                if (fdocno != null) sql += "b.doconlyno <= '" + tdocno + "' and ";
                sql += "b.doccd = '" + doccd + "'  ";
                sql += "order by styleno ";
                rstbl = Master_Help.SQLquery(sql);

                if (rstbl.Rows.Count == 0)
                {
                    return RedirectToAction("NorsPendOrds", "RPTViewer", new { errmsg = "No Records Found !!" });
                }


                //DataTable IR = new DataTable();
                //IR.Columns.Add("docno", typeof(string), "");
                //IR.Columns.Add("docdt", typeof(string), "");
                //IR.Columns.Add("slnm", typeof(string), "");
                //IR.Columns.Add("slcd", typeof(string), "");
                //IR.Columns.Add("trslnm", typeof(string), "");
                //IR.Columns.Add("trslcd", typeof(string), "");
                //IR.Columns.Add("cournm", typeof(string), "");
                //IR.Columns.Add("destn", typeof(string), "");
                //IR.Columns.Add("agslnm", typeof(string), "");
                //IR.Columns.Add("agslcd", typeof(string), "");
                //IR.Columns.Add("slmslnm", typeof(string), "");
                //IR.Columns.Add("slmslcd", typeof(string), "");
                //IR.Columns.Add("prcnm", typeof(string), "");
                //IR.Columns.Add("rem", typeof(string), "");
                //IR.Columns.Add("splnote", typeof(string), "");
                //IR.Columns.Add("gstno", typeof(string), "");
                //IR.Columns.Add("docth1", typeof(string), "");
                //IR.Columns.Add("docth2", typeof(string), "");
                //IR.Columns.Add("docth3", typeof(string), "");
                //IR.Columns.Add("scmnm", typeof(string), "");
                //IR.Columns.Add("totbox", typeof(string), "");
                //IR.Columns.Add("toset", typeof(string), "");
                //IR.Columns.Add("ordamt", typeof(double), "");
                //IR.Columns.Add("delvtypedsc", typeof(string), "");
                ////extra
                //IR.Columns.Add("rateprint", typeof(string), "");
                //IR.Columns.Add("crslcd", typeof(string), "");
                //IR.Columns.Add("prccd", typeof(string), "");
                //IR.Columns.Add("prceffdt", typeof(string), "");
                //IR.Columns.Add("discrtcd", typeof(string), "");
                //IR.Columns.Add("discrteffdt", typeof(string), "");
                //IR.Columns.Add("district", typeof(string), "");
                //IR.Columns.Add("crslnm", typeof(string), "");
                //IR.Columns.Add("regemailid", typeof(string), "");
                //IR.Columns.Add("add1", typeof(string), "");
                //IR.Columns.Add("add2", typeof(string), "");
                //IR.Columns.Add("add3", typeof(string), "");
                //IR.Columns.Add("add4", typeof(string), "");
                //IR.Columns.Add("add5", typeof(string), "");
                //IR.Columns.Add("add6", typeof(string), "");
                //IR.Columns.Add("add7", typeof(string), "");
                //IR.Columns.Add("usr_id", typeof(string), "");
                //IR.Columns.Add("usr_entdt", typeof(string), "");
                //IR.Columns.Add("paytrmcd", typeof(string), "");
                //IR.Columns.Add("paytrmnm", typeof(string), "");
                //IR.Columns.Add("duedays", typeof(string), "");
                ////details
                //IR.Columns.Add("slno", typeof(double), "");
                //IR.Columns.Add("styleno", typeof(string), "");
                //IR.Columns.Add("itnm", typeof(string), "");
                //IR.Columns.Add("stktype", typeof(string), "");
                //IR.Columns.Add("pcstyle", typeof(string), "");
                //IR.Columns.Add("sizes", typeof(string), "");
                //IR.Columns.Add("boxpcs", typeof(string), "");
                //IR.Columns.Add("tbox", typeof(double), "");
                //IR.Columns.Add("tpcs", typeof(double), "");
                //IR.Columns.Add("rate", typeof(double), "");
                //IR.Columns.Add("obldt1", typeof(string), "");
                //IR.Columns.Add("oblno1", typeof(string), "");
                //IR.Columns.Add("oblamt1", typeof(string), "");
                //IR.Columns.Add("osamt1", typeof(string), "");
                //IR.Columns.Add("obldt2", typeof(string), "");
                //IR.Columns.Add("oblno2", typeof(string), "");
                //IR.Columns.Add("oblamt2", typeof(string), "");
                //IR.Columns.Add("osamt2", typeof(string), "");
                //IR.Columns.Add("totos", typeof(string), "");
                //IR.Columns.Add("prefno", typeof(string), "");
                //IR.Columns.Add("prefdt", typeof(string), "");

                ExcelPackage workbook = new ExcelPackage();
                ExcelWorksheet worksheet = workbook.Workbook.Worksheets.Add("Sheet1");
                int excelRow = 1; int excelColumn = 1;

                foreach (DataRow dr in rstbl.Rows)
                {
                    worksheet.Cells[excelRow, ++excelColumn].Value = dr["ITNM"].ToString();
                }
                Response.ClearContent();
                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=Order" + VE.FDOCNO.Replace("/", "-") + ".xlsx");
                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
                workbook.Dispose();



                return Content("Download sucessfully");

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        private void GenerateBorder(ExcelWorksheet worksheet, int row, int column)
        {//make the borders of cell F6 thick
            worksheet.Cells[row, column].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[row, column].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[row, column].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[row, column].Style.Border.Left.Style = ExcelBorderStyle.Thin;
        }
    }
}