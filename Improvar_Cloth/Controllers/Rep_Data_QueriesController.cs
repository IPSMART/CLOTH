using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Web;
using Newtonsoft.Json.Converters;

namespace Improvar.Controllers
{
    public class Rep_Data_QueriesController : Controller
    {
        // GET: Rep_Data_Queries
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc salesfunc = new Salesfunc();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult Rep_Data_Queries()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Data Queries";
                    RepDataQryUpdt VE = new RepDataQryUpdt();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    List<DropDown_list1> CHNGSTYL = new List<DropDown_list1>();
                    CHNGSTYL.Add(new DropDown_list1 { value = "select", text = "--Select--" });
                    CHNGSTYL.Add(new DropDown_list1 { value = "Change Customer_View", text = "Customer View" });

                    VE.DropDown_list1 = CHNGSTYL;
                    VE.NEWPREFDT = Cn.getCurrentDate(VE.mindate);
                    VE.TEXTBOX1 = Session["LASTUPDATE"].retStr();
                    VE.DefaultView = true;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetWorkNo(string Code, string val, string tag = "")
        {
            try
            {
                TransactionSaleEntry VE = new TransactionSaleEntry();
                Cn.getQueryString(VE);
                string itcd = val.retStr() == "" ? "" : Code.Split(Convert.ToChar(Cn.GCS()))[1].retStr();
                tag = Code.Split(Convert.ToChar(Cn.GCS()))[2].retStr();
                string scm = CommVar.CurSchema(UNQSNO);
                string Workno = val;
                if (tag == "Change Item")
                {
                    string sql = " select distinct a.workno,decode(nvl(c.commonid, 'N'), 'Y', 'Yes', 'No')commonid ,nvl(nvl(e.itcd, d.itcd), f.itcd)itcd,nvl(nvl(g.itnm, h.itnm), i.itnm)itnm ";
                    sql += Environment.NewLine + " from( ";
                    sql += Environment.NewLine + "select a.nworkno workno, autono ";
                    sql += Environment.NewLine + "from " + scm + ".t_repair_iss a ";
                    sql += Environment.NewLine + " union ";
                    sql += Environment.NewLine + "select a.workno, autono ";
                    sql += Environment.NewLine + " from " + scm + ".t_kcstone_hdr a ";
                    sql += Environment.NewLine + "union ";
                    sql += Environment.NewLine + "select a.workno, autono ";
                    sql += Environment.NewLine + "from " + scm + ".t_finitem_hdr a )a , ";
                    sql += Environment.NewLine + "" + scm + ".t_cntrl_hdr b, " + scm + ".m_item c, " + scm + ".T_FINITEM_MAIN d, " + scm + ".t_finitem e, " + scm + ".t_kcstone_hdr f, " + scm + ".m_item g, ";
                    sql += Environment.NewLine + "" + scm + ".m_item h, " + scm + ".m_item i ";
                    sql += Environment.NewLine + "where a.autono = b.autono(+) and a.workno = c.COMMONWORKNO(+) and nvl(b.cancel, 'N')= 'N' ";
                    sql += Environment.NewLine + "and a.workno = d.workno(+) ";
                    sql += Environment.NewLine + "and a.workno = e.workno(+) ";
                    sql += Environment.NewLine + "and a.workno = f.workno(+) and d.itcd = g.itcd(+)and e.itcd = h.itcd(+)and f.itcd = i.itcd(+) ";
                    if (Workno != "") sql += Environment.NewLine + "and a.workno='" + Workno + "' ";
                    if (itcd != "") sql += Environment.NewLine + "and nvl(nvl(e.itcd, d.itcd), f.itcd)='" + itcd + "' ";
                    sql += Environment.NewLine + "order by a.workno ";
                    DataTable dt = MasterHelp.SQLquery(sql);

                    if (Workno.retStr() == "" || dt.Rows.Count > 1)
                    {
                        System.Text.StringBuilder SB = new System.Text.StringBuilder();
                        for (int i = 0; i <= dt.Rows.Count - 1; i++)
                        {
                            SB.Append("<tr><td>" + dt.Rows[i]["WORKNO"] + "</td> "
                                + "<td>" + dt.Rows[i]["ITNM"] + "</td><td>" + dt.Rows[i]["ITCD"] + "</td></tr>");
                        }
                        var hdr = "Work Order No." + Cn.GCS() + "ITEM NAME" + Cn.GCS() + " ITCD";
                        return PartialView("_Help2", MasterHelp.Generate_help(hdr, SB.ToString()));
                    }
                    else
                    {
                        if (dt.Rows.Count > 0)
                        {
                            var str = MasterHelp.ToReturnFieldValues("", dt);
                            return Content(str);

                        }
                        else
                        {
                            return Content("Invalid Work Order No. ! Please Select / Enter a Valid Work Order No. !!");
                        }
                    }
                }
                else
                {
                    string str = "";
                    //var str = MasterHelp.WORKNO_help(val);
                    if (str.IndexOf("='helpmnu'") >= 0)
                    {
                        return PartialView("_Help2", str);
                    }
                    else
                    {
                        return Content(str);
                    }
                }

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetFromWorkNo(string Code, string val)
        {
            try
            {
                //string tdt = Code.Split(Convert.ToChar(Cn.GCS()))[0];
                //string taxgrpcd = Code.Split(Convert.ToChar(Cn.GCS()))[1];
                //string workdt = Code.Split(Convert.ToChar(Cn.GCS()))[2];
                //string itcd = Code.Split(Convert.ToChar(Cn.GCS()))[3].retStr();
                string Workno = val;
                //var str = MasterHelp.WORKNO_help(val);
                string str = "";
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetItemDetails(RepDataQryUpdt VE, string val)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                Cn.getQueryString(VE);
                //var str = MasterHelp.ITCD_help(val);
                string str = "";
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {

                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetSublegData(RepDataQryUpdt VE, string NAME ="", string AREA ="", string ADDRESS="", string AGNM="", string CITY = "", string STATE = "", string PIN = "", string REGMOB = "", string REGMAIL = "", string GSTNO = "")
        {
            try
            {
                Cn.getQueryString(VE);
                string scm = CommVar.CurSchema(UNQSNO); string scmf = CommVar.FinSchema(UNQSNO);
                string tdt = System.DateTime.Now.Date.retDateStr();
                //DataTable DT = salesfunc.GetFinishStoneDataDetails(Worknos.retSqlformat(), "", tdt);
                string sql = "";
                sql = "select a.slcd,a.slnm,a.locality,a.add1 || ',' || a.add2 || ',' || a.add3 || ',' || a.add4 || ',' || a.add5 || ',' || a.add6 || ',' || a.add7 AS Address, " +Environment.NewLine;
                sql += "a.state,a.district,a.pin,a.regemailid,a.regmobile,a.gstno,b.agslcd, c.slnm agslnm from " + Environment.NewLine;
                sql += "" + scmf + ".m_subleg a, " + scm + ".m_subleg_com b, " + scmf + ".m_subleg c " + Environment.NewLine;
                sql += "where a.slcd = b.slcd(+) and b.agslcd = c.slcd(+) " + Environment.NewLine;
                if (NAME.retStr() != "") sql += "and a.slnm like '%" + NAME.ToUpper() + "%'" + Environment.NewLine;
                if (AREA.retStr() != "") sql += "and a.locality like '%" + AREA.ToUpper() + "%'" + Environment.NewLine;
                if (ADDRESS.retStr() != "") sql += "and address like '%" + ADDRESS.ToUpper() + "%'" + Environment.NewLine;
                if (AGNM.retStr() != "") sql += "and c.slnm like '%" + AGNM.ToUpper() + "%'" + Environment.NewLine;
                if (CITY.retStr() != "") sql += "and a.district like '%" + CITY.ToUpper() + "%'" + Environment.NewLine;
                if (STATE.retStr() != "") sql += "and a.state like '%" + STATE.ToUpper() + "%'" + Environment.NewLine;
                if (PIN.retStr() != "") sql += "and a.pin like '%" + PIN + "%'" + Environment.NewLine;
                if (REGMOB.retStr() != "") sql += "and a.regmobile like '%" + REGMOB + "%'" + Environment.NewLine;
                if (REGMAIL.retStr() != "") sql += "and a.regemailid like '%" + REGMAIL.ToUpper() + "%'" + Environment.NewLine;
                if (GSTNO.retStr() != "") sql += "and a.gstno like '%" + GSTNO.ToUpper() + "%'" + Environment.NewLine;
                DataTable DT = MasterHelp.SQLquery(sql);
                
                

                    if (DT != null && DT.Rows.Count > 0)
                    {                        
                        VE.CUSTOMER_DTL = (from DataRow dr in DT.Rows
                                           select new CUSTOMER_DTL
                                           {
                                               NM = dr["slnm"].retStr(),
                                               ADDRESS = dr["address"].retStr(),
                                               AREA = dr["locality"].retStr(),
                                               CITY = dr["district"].retStr(),
                                               PIN = dr["pin"].retDbl(),
                                               REGMOB = dr["regmobile"].retDbl(),
                                               REGMAIL = dr["regemailid"].retStr(),
                                               GSTNO = dr["gstno"].retStr(),
                                               AGNM = dr["agslnm"].retStr(),
                                               STATE = dr["state"].retStr(),
                                           }).ToList();


                        for (int i = 0; i <= VE.CUSTOMER_DTL.Count - 1; i++)
                        {
                            VE.CUSTOMER_DTL[i].SLNO = (i + 1).retDbl();
                        }
                    }
                
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_Rep_Data_Qry_Updt_GRID", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        [HttpPost]
        public ActionResult Rep_Data_Queries(FormCollection FC, RepDataQryUpdt VE, string BtnNm)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            try
            {
                string LOC = CommVar.Loccd(UNQSNO);
                string COM = CommVar.Compcd(UNQSNO);
                string scm1 = CommVar.CurSchema(UNQSNO);

                if (VE.CUSTOMER_DTL == null)
                {
                    return Content("FILL THE GRID FIRST!!");
                }

                var dt = VE.CUSTOMER_DTL;
                if (dt.Count == 0)
                {
                    return Content("NO RECORD!!");
                }
                DataTable Template1 = new DataTable();
                Template1.Columns.Add("SL NO.", typeof(double), "");
                Template1.Columns.Add("Name", typeof(string), "");
                Template1.Columns.Add("Address", typeof(string), "");
                Template1.Columns.Add("Area", typeof(string), "");
                Template1.Columns.Add("City", typeof(string), "");
                Template1.Columns.Add("Pincode", typeof(double), "");
                Template1.Columns.Add("Reg.Email", typeof(string), "");
                Template1.Columns.Add("Reg.Mobile", typeof(double), "");
                Template1.Columns.Add("GST No", typeof(string), "");
                Template1.Columns.Add("Agent Name", typeof(string), "");


                foreach (var row in dt)
                {
                    DataRow fin1 = Template1.NewRow();
                    fin1["SL NO."] = row.SLNO.retDbl();
                    fin1["Name"] = row.NM.retStr();
                    fin1["Address"] = row.ADDRESS.retStr();
                    fin1["Area"] = row.AREA.retStr();
                    fin1["City"] = row.CITY.retStr();
                    fin1["Pincode"] = row.PIN.retDbl();
                    fin1["Reg.Email"] = row.REGMAIL.retStr();
                    fin1["Reg.Mobile"] = row.REGMOB.retDbl();
                    fin1["GST No"] = row.GSTNO.retStr();
                    fin1["Agent Name"] = row.AGNM.retStr();
                    Template1.Rows.Add(fin1);
                }               

                ExcelPackage workbook = new ExcelPackage();
                ExcelWorksheet worksheet = workbook.Workbook.Worksheets.Add("Sheet1");
                worksheet.Cells["A1"].LoadFromDataTable(Template1, true);
                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=" + "Customer Wise Details" + ".xlsx");
                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    workbook.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
                workbook.Dispose();


                return Content("Excel Generated Successfully...");

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
    }
}