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


                DataTable IR = new DataTable();
                //IR.Columns.Add("docno", typeof(string), "");
                //IR.Columns.Add("docdt", typeof(string), "");
                //IR.Columns.Add("slcd", typeof(string), "");
                //IR.Columns.Add("SLNM", typeof(string), "");
                //IR.Columns.Add("address", typeof(string), "");
                IR.Columns.Add("slno", typeof(int), "");
                IR.Columns.Add("itgrpcd", typeof(string), "");
                IR.Columns.Add("itgrpnm", typeof(string), "");
                IR.Columns.Add("ITCD", typeof(string), "");
                IR.Columns.Add("ITNM", typeof(string), "");
                IR.Columns.Add("STYLENO", typeof(string), "");
                IR.Columns.Add("ITREM", typeof(string), "");
                IR.Columns.Add("fabitcd", typeof(string), "");
                IR.Columns.Add("PDESIGN", typeof(string), "");
                IR.Columns.Add("STKDRCR", typeof(string), "");
                IR.Columns.Add("STKTYPE", typeof(string), "");
                IR.Columns.Add("FREESTK", typeof(string), "");
                IR.Columns.Add("PCSPERSET", typeof(string), "");
                IR.Columns.Add("sizecd", typeof(string), "");
                IR.Columns.Add("UOMCD", typeof(string), "");
                IR.Columns.Add("qnty", typeof(double), "");
                IR.Columns.Add("rate", typeof(double), "");
                IR.Columns.Add("scmdiscamt", typeof(double), "");
                IR.Columns.Add("discamt", typeof(double), "");
                IR.Columns.Add("DELVDT", typeof(string), "");


                for (int m = 0; m <= rstbl.Rows.Count - 1; m++)
                {
                    DataRow fin1 = IR.NewRow();
                    //fin1["docno"] = rstbl.Rows[m]["docno"].ToString();
                    //fin1["docdt"] = rstbl.Rows[m]["docdt"].ToString().Remove(10);
                    //fin1["slcd"] = rstbl.Rows[m]["slcd"].ToString();
                    //fin1["SLNM"] = rstbl.Rows[m]["SLNM"].ToString();
                    //fin1["address"] = rstbl.Rows[m]["address"].ToString();
                    fin1["slno"] = rstbl.Rows[m]["slno"].retInt();
                    fin1["itgrpcd"] = rstbl.Rows[m]["itgrpcd"].ToString();
                    fin1["itgrpnm"] = rstbl.Rows[m]["itgrpnm"].ToString();
                    fin1["ITCD"] = rstbl.Rows[m]["ITCD"].ToString();
                    fin1["ITNM"] = rstbl.Rows[m]["ITNM"].ToString();
                    fin1["STYLENO"] = rstbl.Rows[m]["STYLENO"].ToString();
                    fin1["fabitcd"] = rstbl.Rows[m]["fabitcd"].ToString();
                    fin1["PDESIGN"] = rstbl.Rows[m]["PDESIGN"].ToString();
                    fin1["STKDRCR"] = rstbl.Rows[m]["STKDRCR"].ToString();
                    fin1["STKTYPE"] = rstbl.Rows[m]["STKTYPE"].ToString();
                    fin1["FREESTK"] = rstbl.Rows[m]["FREESTK"].ToString();
                    fin1["PCSPERSET"] = rstbl.Rows[m]["PCSPERSET"].ToString();
                    fin1["sizecd"] = rstbl.Rows[m]["sizecd"].ToString();
                    fin1["UOMCD"] = rstbl.Rows[m]["UOMCD"].ToString();
                    fin1["qnty"] = rstbl.Rows[m]["qnty"].ToString();
                    fin1["rate"] = rstbl.Rows[m]["rate"].ToString();
                    fin1["scmdiscamt"] = rstbl.Rows[m]["scmdiscamt"].ToString();
                    fin1["discamt"] = rstbl.Rows[m]["discamt"].ToString();
                    fin1["DELVDT"] = rstbl.Rows[m]["DELVDT"].ToString();
                    fin1["ITREM"] = rstbl.Rows[m]["ITREM"].ToString();
                    IR.Rows.Add(fin1);
                }
                string compaddress;
                compaddress = Salesfunc.retCompAddress();

                ExcelPackage workbook = new ExcelPackage();
                ExcelWorksheet worksheet = workbook.Workbook.Worksheets.Add("Sheet1");
                int row = 1;
                worksheet.Cells["A" + row].Value = compaddress.retCompValue("compnm");
                worksheet.Cells["A" + row].Style.Font.Bold = true;
                row++;
                if (compaddress.retCompValue("compadd").retStr() != "")
                {
                    worksheet.Cells["A" + row++].Value = compaddress.retCompValue("compadd");
                }
                if (compaddress.retCompValue("compcommu").retStr() != "")
                {
                    worksheet.Cells["A" + row++].Value = compaddress.retCompValue("compcommu");
                }
                if (compaddress.retCompValue("compstat").retStr() != "")
                {
                    worksheet.Cells["A" + row++].Value = compaddress.retCompValue("compstat");
                }
                if (compaddress.retCompValue("corpadd").retStr() != "")
                {
                    worksheet.Cells["A" + row++].Value = compaddress.retCompValue("corpadd");
                }
                if (compaddress.retCompValue("corpcommu").retStr() != "")
                {
                    worksheet.Cells["A" + row++].Value = compaddress.retCompValue("corpcommu");
                }
                if (compaddress.retCompValue("locaadd").retStr() != "")
                {
                    worksheet.Cells["A" + row++].Value = compaddress.retCompValue("locaadd");
                }
                if (compaddress.retCompValue("locacommu").retStr() != "")
                {
                    worksheet.Cells["A" + row++].Value = compaddress.retCompValue("locacommu");
                }
                if (compaddress.retCompValue("locastat").retStr() != "")
                {
                    worksheet.Cells["A" + row++].Value = compaddress.retCompValue("locastat");
                }
                row++;
                worksheet.Cells["A" + row++].Value = "To : " + rstbl.Rows[0]["slnm"] + "[" + rstbl.Rows[0]["slcd"] + "]";
                worksheet.Cells["A" + row++].Value = rstbl.Rows[0]["address"];

                row++;
                worksheet.Cells["A" + row++].Value = "Order No: " + rstbl.Rows[0]["docno"];
                worksheet.Cells["A" + row++].Value = "Order Date: " + rstbl.Rows[0]["docdt"].retStr().Remove(10);

                row++;
                worksheet.Cells["A" + row++].LoadFromDataTable(IR, true);
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