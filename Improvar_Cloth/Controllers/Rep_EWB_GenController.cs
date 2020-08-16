using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using Excel = Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Web;
using Oracle.ManagedDataAccess.Client;
using System.Web.UI.WebControls;
using OfficeOpenXml;
using System.Globalization;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class Rep_EWB_GenController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_EWB_Gen
        public ActionResult Rep_EWB_Gen()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "E WAY BILL GENERATION";
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    EWayBillReport VE = new EWayBillReport();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    if (VE.DOC_CODE == "") VE.DOC_CODE = "SRET,PRET,SBILL,PBILL";
                    VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                    string[] dt = CommVar.FinPeriod(UNQSNO).Split('-');
                    VE.DATEFROM = (DateTime.Now.ToString().Remove(10));
                    VE.DATETO = (DateTime.Now.ToString().Remove(10));
                    if (CommVar.Loccd(UNQSNO) == "KOLK") VE.Checkbox1 = true; else VE.Checkbox1 = false;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                EWayBillReport VE = new EWayBillReport();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }

        public ActionResult ShowList(EWayBillReport VE, FormCollection FC)
        {
            try
            {
                string dbnm = CommVar.CurSchema(UNQSNO).ToString();
                string fdbnm = CommVar.FinSchema(UNQSNO);
                string comp = CommVar.Compcd(UNQSNO);
                string loc = CommVar.Loccd(UNQSNO);

                string doccd = "";
                if (FC.AllKeys.Contains("DOCCD"))
                {
                    string[] DOCCD = FC["DOCCD"].ToString().Split(',');
                    if (DOCCD.Count() != 0)
                    {
                        for (int m = 0; m <= DOCCD.Count() - 1; m++)
                        {
                            doccd = doccd + "'" + DOCCD[m] + "',";
                        }
                        doccd = doccd.Substring(0, doccd.Length - 1);
                    }
                }

                DataTable tbl;
                string query = "";

                query += "select a.autono, b.doccd, b.docno, b.docdt,d.slcd, nvl(d.fullname,d.slnm) slnm, d.district, nvl(g.distance,0) distance, a.blamt, e.slnm trslnm, c.lorryno, c.lrno, c.lrdt ";
                query += "from " + dbnm + ".t_txn a, " + dbnm + ".t_cntrl_hdr b, " + dbnm + ".t_txntrans c, " + fdbnm + ".m_subleg d, ";
                query += "" + fdbnm + ".m_subleg e, " + fdbnm + ".m_loca f, " + fdbnm + ".m_subleg_locoth g , " + dbnm + ".m_doctype h ";
                //query += "where a.autono=b.autono and a.autono=c.autono(+) and nvl(a.conslcd,a.slcd)=d.slcd(+) and nvl(c.translcd,c.crslcd)=e.slcd(+) and ";
                query += "where a.autono=b.autono and a.autono=c.autono(+) and a.slcd=d.slcd(+) and nvl(c.translcd,c.crslcd)=e.slcd(+) and ";
                query += "nvl(b.cancel,'N')='N' and b.compcd='" + comp + "' and b.loccd='" + loc + "' and b.compcd||b.loccd=f.compcd||f.loccd and trim(c.ewaybillno) is null and  ";
                query += "(b.loccd=g.loccd or g.loccd is null) and (b.compcd=g.compcd or g.compcd is null) and d.slcd=g.slcd(+) and ";
                if (doccd != "") query += "b.doccd in (" + doccd + ") and ";
                query += "b.docdt >= to_date('" + VE.DATEFROM + "','dd/mm/yyyy') and b.docdt <= to_date('" + VE.DATETO + "','dd/mm/yyyy') ";
                query += " and a.doccd=h.doccd and h.dOCTYPE in('SRET','PRET','SBILL','PBILL') ";
                query += "order by b.docdt, c.lrno, b.docno ";
                tbl = masterHelp.SQLquery(query);
                VE.EWAYBILL = (from DataRow dr in tbl.Rows
                               select new EWAYBILL()
                               {
                                   AUTONO = dr["AUTONO"].ToString(),
                                   DOCNO = dr["DOCNO"].ToString(),
                                   DOCDT = dr["DOCDT"].ToString().Remove(10),
                                   SLNM = dr["SLNM"].ToString(),
                                   SLCD = dr["SLCD"].ToString(),
                                   DISTRICT = dr["DISTRICT"].ToString(),
                                   DISTANCE = dr["DISTANCE"].ToString(),
                                   TRSLNM = dr["TRSLNM"].ToString(),
                                   BLAMT = Convert.ToDouble(dr["BLAMT"].ToString()),
                                   LRDT = dr["LRDT"].ToString(),
                                   LORRYNO = dr["LORRYNO"].ToString(),
                                   LRNO = dr["LRNO"].ToString()
                               }).ToList();
                for (int i = 0; i < VE.EWAYBILL.Count; i++)
                {
                    if (VE.EWAYBILL[i].LRDT != "") VE.EWAYBILL[i].LRDT = VE.EWAYBILL[i].LRDT.Remove(10);
                    VE.EWAYBILL[i].SLNO = Convert.ToInt16(i + 1);
                }
                return PartialView("_Rep_EWB_Gen", VE);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult Rep_EWB_Gen(FormCollection FC, EWayBillReport VE, string Command)
        {
            string despfrom = "L";
            if (Command == "UPLOAD EXCEL")
            {
                #region Upload Excel
                string LOC = CommVar.Loccd(UNQSNO);
                string COM = CommVar.Compcd(UNQSNO);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

                if (VE.EWAYBILL == null)
                {
                    return Content("<h1> No Data Found in the page. Please click proceed to get Data and then you will view grid..</h1>");
                }

                CS = Cn.GetConnectionString();
                Cn.con = new OracleConnection(CS);
                if (Cn.con.State == ConnectionState.Closed)
                {
                    Cn.con.Open();
                }
                var transaction = Cn.con.BeginTransaction();

                try
                {
                    //delete attn from server

                    if (Request != null)
                    {
                        string Excel_Header = "SlNo" + "|" + "Supply Type" + "|" + "Doc No" + "|" + "Doc Date" + "|" + "Other Party Gstin" + "|" + "Supply State" + "|";
                        Excel_Header += "Vehicle No" + "|" + "No of Items" + "|" + "EWB No" + "|" + "EWD Date" + "|" + "Valid Till Date"; // + "|" + "Errors";
                        string[] Header = Excel_Header.Split('|');
                        string fileExtension = System.IO.Path.GetExtension(Request.Files["UploadedFile"].FileName);
                        if (fileExtension != ".xlsx")
                        {
                            return Content("File Extention must be [.XLSX] ");
                        }
                        HttpPostedFileBase file = Request.Files["UploadedFile"];
                        if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                        {
                            string fileName = file.FileName;
                            string fileContentType = file.ContentType;
                            byte[] fileBytes = new byte[file.ContentLength];
                            var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                            using (var package = new ExcelPackage(file.InputStream))
                            {
                                var currentSheet = package.Workbook.Worksheets;
                                var workSheet = currentSheet.First();
                                var noOfCol = workSheet.Dimension.End.Column;
                                var noOfRow = workSheet.Dimension.End.Row;
                                //if (Header.Count() != noOfCol)
                                //{
                                //    string message = "Invalid Excel Format ! Number Of Columns of Excel Sheet is not Maintained !!";
                                //    return Content("<h1 style='color:red;'>Error Log :</h1><br/><h3 style='color:darkred;'>" + message + " </h3><br/> Header Must be =>" + Excel_Header);
                                //}
                                for (int i = 0; i < Header.Length; i++)
                                {
                                    if (workSheet.Cells[1, i + 1].Value.ToString().ToUpper() != Header[i].ToUpper().ToString())
                                    {
                                        string message = "Invalid Excel Format ! Columns Name of Excel Sheet is not Maintained !! [" + Header[i].ToUpper().ToString() + "] Not Match";
                                        return Content("<h1 style='color:red;'>Error Log :</h1><br/><h3 style='color:darkred;'>" + message + " </h3><br/> Header Must be =>" + Excel_Header);
                                    }
                                }
                                for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                                {
                                    var err = workSheet.Cells[rowIterator, 3].Value.ToString();
                                    var auto_eway = (from f in VE.EWAYBILL
                                                     where f.DOCNO == workSheet.Cells[rowIterator, 3].Value.ToString()
                                                     select new
                                                     {
                                                         autono = f.AUTONO,
                                                     });
                                    if (auto_eway != null)
                                    {
                                        string sql = "";
                                        var schm = CommVar.CurSchema(UNQSNO).ToString();
                                        sql = "update " + schm + ".t_txntrans set ewaybillno='" + workSheet.Cells[rowIterator, 9].Value.ToString() + "' ";
                                        sql += "where autono='" + auto_eway.First().autono + "' and trim(ewaybillno) is null ";
                                        Cn.com = new OracleCommand(sql, Cn.con);
                                        Cn.com.ExecuteNonQuery();
                                    }
                                }
                                transaction.Commit();
                                Cn.con.Close();
                                return Content("Imported Successfully");
                            }
                        }
                    }
                    return View("T_Daily_Attnd_Excel");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Content(ex.Message + ex.InnerException + "  ");
                }

                #endregion
            }
            else
            {
                #region get data

                string msg = "";
                try
                {

                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    string dbnm = CommVar.CurSchema(UNQSNO).ToString();
                    string fdbnm = CommVar.FinSchema(UNQSNO);
                    string comp = CommVar.Compcd(UNQSNO);
                    string loc = CommVar.Loccd(UNQSNO);
                    string aauto = "";
                    bool flag = false;
                    for (int i = 0; i < VE.EWAYBILL.Count; i++)
                    {
                        if (VE.EWAYBILL[i].Checked == true)
                        {
                            flag = true;
                            aauto += ",'" + VE.EWAYBILL[i].AUTONO + "' ";

                            //var chk = FC["EWAYBILL[" + i + "].DISTANCE"].ToString().Split(',');
                            //if (chk.Length == 2)
                            //{
                            //if (chk[0].ToString() != chk[1].ToString())
                            //{
                            using (var TRANS = DBF.Database.BeginTransaction())
                            {
                                M_SUBLEG_LOCOTH msuboth = new M_SUBLEG_LOCOTH();

                                msuboth.CLCD = CommVar.ClientCode(UNQSNO);
                                msuboth.SLCD = VE.EWAYBILL[i].SLCD;
                                msuboth.COMPCD = CommVar.Compcd(UNQSNO);
                                msuboth.LOCCD = CommVar.Loccd(UNQSNO);
                                msuboth.DISTANCE = Convert.ToInt32(VE.EWAYBILL[i].DISTANCE);
                                DBF.M_SUBLEG_LOCOTH.RemoveRange(DBF.M_SUBLEG_LOCOTH.Where(x => x.SLCD == msuboth.SLCD && x.LOCCD == msuboth.LOCCD && x.COMPCD == msuboth.COMPCD));
                                DBF.M_SUBLEG_LOCOTH.Add(msuboth);
                                DBF.SaveChanges();
                                TRANS.Commit();
                            }
                            //}
                            //}
                        }
                    }
                    if (!flag) return Content("No row selected.Please select row.");
                    if (aauto != "") aauto = aauto.Substring(1);
                    DataTable rsComp;
                    DataTable tbl;
                    string query = "";

                    query = "select a.locnm slnm, a.gstno, a.add1||' '||a.add2 add1, a.add3||' '||a.add4 add2, a.district, a.pin, b.compnm,a.statecd, upper(c.statenm) statenm ";
                    query += "from " + fdbnm + ".m_loca a, " + fdbnm + ".m_comp b, improvar.ms_state c ";
                    query += "where a.compcd=b.compcd and a.statecd=c.statecd(+) and ";
                    query += "a.compcd='" + comp + "' and a.loccd='" + loc + "' ";
                    rsComp = masterHelp.SQLquery(query);

                    query = "";
                    query += "select a.autono, b.doccd, a.blno, a.bldt, translate(nvl(d.fullname,d.slnm),'+[#./()]^',' ') slnm, d.gstno, ";
                    query += "decode(d.othaddpin,null,d.add1||' '||d.add2, d.othadd1||' '||d.othadd2) add1, decode(d.othaddpin,null,d.add3||' '||d.add4,d.othadd3||' '||d.othadd4) add2, d.district, nvl(d.othaddpin,d.pin) pin, ";
                    query += "d.statecd, upper(k.statenm) statenm, ";
                    query += "translate(nvl(p.fullname,p.slnm),'+[#./()]^',' ') cslnm, p.gstno cgstno, ";
                    query += "decode(p.othaddpin,null,p.add1||' '||p.add2, p.othadd1||' '||p.othadd2) cadd1, decode(p.othaddpin, null, p.add3||' '||p.add4,p.othadd3||' '||p.othadd4) cadd2, p.district cdistrict, nvl(p.othaddpin,p.pin) cpin, ";
                    query += "p.statecd cstatecd, upper(q.statenm) cstatenm, ";
                    query += "e.slnm trslnm, e.gstno trgst, replace(translate(c.lorryno,'/-',' '),' ','') lorryno, c.lrno, c.lrdt, a.igstper, a.cgstper, a.sgstper, a.cessper, ";
                    query += "translate(a.itnm,'+[#/()]^',' ') itnm, a.slno, a.hsncode,l.guomcd, l.guomnm, nvl(m.distance,0) distance, ";
                    query += "o.goadd1||' '||o.goadd2 goadd1, o.goadd3 goadd2, o.district godistrict, o.pin gopin, ";
                    query += "sum(decode(nvl(j.gst_qntyconv,0),0,1,j.gst_qntyconv)*a.qnty) qnty, sum(a.amt) amt, sum(a.blamt) blamt, ";
                    query += "sum(a.igstamt) igstamt, sum(a.cgstamt) cgstamt, sum(a.sgstamt) sgstamt, sum(a.cessamt) cessamt ,sum(a.othramt) othramt ";
                    query += "from " + fdbnm + ".t_vch_gst a, " + dbnm + ".t_cntrl_hdr b, " + dbnm + ".t_txntrans c, " + fdbnm + ".m_subleg d, ";
                    query += "" + fdbnm + ".m_subleg e, " + fdbnm + ".m_loca f, " + fdbnm + ".m_uom j, improvar.ms_state k, improvar.ms_gstuom l, ";
                    query += fdbnm + ".m_subleg_locoth m, " + dbnm + ".t_txn n, " + dbnm + ".m_godown o, " + fdbnm + ".m_subleg p, improvar.ms_state q ";
                    query += "where a.autono=b.autono and a.autono=c.autono(+) and a.pcode=d.slcd(+) and nvl(c.translcd,c.crslcd)=e.slcd(+) and ";
                    query += "d.statecd=k.statecd(+) and a.uom=j.uomcd(+) and a.autono=n.autono(+) and n.gocd=o.gocd(+) and nvl(a.conslcd,a.pcode)=p.slcd(+) and p.statecd=q.statecd(+) and ";
                    query += "nvl(j.gst_uomcd,j.uomcd)=l.guomcd(+) and (b.loccd=m.loccd or m.loccd is null) and (b.compcd=m.compcd or m.compcd is null) and d.slcd=m.slcd(+) and ";
                    query += "nvl(b.cancel,'N')='N' and b.compcd='" + comp + "' and b.loccd='" + loc + "' and b.compcd||b.loccd=f.compcd||f.loccd and trim(c.ewaybillno) is null and ";
                    if (aauto != "") query += "a.autono in(" + aauto + ") and ";
                    query += "b.docdt >= to_date('" + VE.DATEFROM + "','dd/mm/yyyy') and b.docdt <= to_date('" + VE.DATETO + "','dd/mm/yyyy') ";
                    query += "group by a.autono, b.doccd, a.blno, a.bldt, translate(nvl(d.fullname,d.slnm),'+[#./()]^',' '), d.gstno, ";
                    query += "decode(d.othaddpin,null,d.add1||' '||d.add2,d.othadd1||' '||d.othadd2), decode(d.othaddpin,null,d.add3||' '||d.add4,d.othadd3||' '||d.othadd4), d.district, nvl(d.othaddpin,d.pin), ";
                    query += "d.statecd, upper(k.statenm), ";
                    query += "e.slnm, e.gstno, replace(translate(c.lorryno,'/-',' '),' ',''), c.lrno, c.lrdt, a.igstper, a.cgstper, a.sgstper, a.cessper, ";
                    query += "translate(nvl(p.fullname,p.slnm),'+[#./()]^',' '), p.gstno, ";
                    query += "decode(p.othaddpin,null,p.add1||' '||p.add2, p.othadd1||' '||p.othadd2), decode(p.othaddpin,null,p.add3||' '||p.add4,p.othadd3||' '||p.othadd4), p.district, nvl(p.othaddpin,p.pin), ";
                    query += "p.statecd, upper(q.statenm), ";
                    query += "translate(a.itnm,'+[#/()]^',' '), a.slno, a.hsncode,l.guomcd, l.guomnm, nvl(m.distance,0), ";
                    query += "o.goadd1||' '||o.goadd2, o.goadd3, o.district, o.pin ";
                    query += "order by blno, bldt, autono, a.slno ";
                    tbl = masterHelp.SQLquery(query);
                    if (tbl == null)
                    {
                        return Content("No record Found");
                    }
                    #endregion
                    #region ewaybill preparation starts
                    string path = ""; string path_Save = "";
                    path = Server.MapPath("~/Templates\\Excel\\EWB_Bulk_Data.xlsm");//EWB_Bulk_1_0_1118
                                                                                    // FileInfo workBook = new FileInfo(path);
                    path_Save = @"C:\improvar\EWB_Bulk_Data.xlsm";
                    try
                    {
                        msg += "Try Excel open =>";
                        List<Prepare_JSON> PJSON = new List<Prepare_JSON>();
                        Microsoft.Office.Interop.Excel.Application exlApplcn = null;
                        msg += " excel opemed .Try book open =>";
                        Microsoft.Office.Interop.Excel.Workbook workbook = null;
                        msg += " excel opemed .Try sheet open =><br/>";
                        Excel.Worksheet Wsheet = null;
                        if (Command == "GENERATE EXCEL")
                        {
                            exlApplcn = new Microsoft.Office.Interop.Excel.Application();
                            workbook = exlApplcn.Workbooks.Open(path, 0, true);
                            //    Microsoft.Office.Interop.Excel.Workbook workbook = exlApplcn.Workbooks.Open(path,
                            //0,true, Type.Missing, Type.Missing, Type.Missing,
                            //Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                            //true, Type.Missing, Type.Missing, true);
                            msg += "Excel opened =>";
                            Wsheet = (exlApplcn.Sheets["eWayBill"]);
                            Wsheet.EnableCalculation = false;
                        }

                        int i = 0;
                        var rno = 4;
                        string TtlTax = "";
                        string invamt = "";
                        string crntblno = "";
                        while (i < tbl.Rows.Count)
                        {
                            Prepare_JSON prejson = new Prepare_JSON();
                            TtlTax = tbl.Rows[i]["sgstper"].ToString() + '+' + tbl.Rows[i]["cgstper"].ToString() + '+' + tbl.Rows[i]["igstper"].ToString() + '+' + tbl.Rows[i]["cessper"].ToString() + "+0";
                            //Wsheet.Cells[rno, 1].Value = tbl.Rows[i]["autono"];
                            prejson.SLNO = Convert.ToInt16(i + 1);
                            prejson.Supply_Type = "O"; //a (Outward)
                            prejson.SubSupply_Type = "1"; //b(Supply)
                            prejson.Doctype = "INV"; //c (Tax Invoice)
                            prejson.blno = tbl.Rows[i]["blno"].ToString();//d
                            prejson.bldt = Convert.ToDateTime(tbl.Rows[i]["bldt"]);//e
                            prejson.Transaction_Type = "1";//f
                            if (Command == "GENERATE EXCEL")
                            {
                                Wsheet.Cells[rno, 1].Value = "Outward"; //a
                                Wsheet.Cells[rno, 2].Value = "Supply"; //b 
                                Wsheet.Cells[rno, 3].Value = "Tax Invoice"; //c
                                Wsheet.Cells[rno, 4].Value = tbl.Rows[i]["blno"];//d
                                string bldt = Convert.ToDateTime(tbl.Rows[i]["bldt"]).ToString("MM/dd/yyyy");//
                                Wsheet.Cells[rno, 5].Value = bldt;//e
                                Wsheet.Cells[rno, 6].Value = "Regular";//f
                                Wsheet.Cells[rno, 7].Value = rsComp.Rows[0]["compnm"];//g
                                Wsheet.Cells[rno, 8].Value = rsComp.Rows[0]["gstno"];//h

                            }
                            prejson.compnm = rsComp.Rows[0]["compnm"].ToString();//g
                            prejson.frmgstno = rsComp.Rows[0]["gstno"].ToString();//h
                            if (VE.Checkbox1 == true) // (loc == "KOLK" && comp == "CHEM")
                            {
                                prejson.frmadd1 = tbl.Rows[0]["goadd1"].ToString();//i
                                prejson.frmadd2 = tbl.Rows[0]["goadd2"].ToString();//j
                                prejson.frmdistrict = tbl.Rows[0]["godistrict"].ToString();//k
                                prejson.frmpin = tbl.Rows[0]["gopin"].ToString();//l
                                if (Command == "GENERATE EXCEL")
                                {
                                    Wsheet.Cells[rno, 9].Value = tbl.Rows[i]["goadd1"];//i
                                    Wsheet.Cells[rno, 10].Value = tbl.Rows[i]["goadd2"];//j
                                    Wsheet.Cells[rno, 11].Value = tbl.Rows[i]["godistrict"];//k
                                    Wsheet.Cells[rno, 12].Value = tbl.Rows[i]["gopin"];//l
                                }
                            }
                            else
                            {
                                prejson.frmadd1 = rsComp.Rows[0]["add1"].ToString();//i
                                prejson.frmadd2 = rsComp.Rows[0]["add2"].ToString();//j
                                prejson.frmdistrict = rsComp.Rows[0]["district"].ToString();//k
                                prejson.frmpin = rsComp.Rows[0]["pin"].ToString();//l
                                if (Command == "GENERATE EXCEL")
                                {
                                    Wsheet.Cells[rno, 9].Value = rsComp.Rows[0]["add1"];//i
                                    Wsheet.Cells[rno, 10].Value = rsComp.Rows[0]["add2"];//j
                                    Wsheet.Cells[rno, 11].Value = rsComp.Rows[0]["district"];//k
                                    Wsheet.Cells[rno, 12].Value = rsComp.Rows[0]["pin"];//l
                                }
                            }
                            if (Command == "GENERATE EXCEL")
                            {
                                Wsheet.Cells[rno, 13].Value = rsComp.Rows[0]["statenm"];//m
                                Wsheet.Cells[rno, 14].Value = rsComp.Rows[0]["statenm"];//n
                                Wsheet.Cells[rno, 15].Value = tbl.Rows[i]["slnm"];//o
                                Wsheet.Cells[rno, 16].Value = tbl.Rows[i]["gstno"];//p
                                Wsheet.Cells[rno, 17].Value = tbl.Rows[i]["cadd1"];//q
                                Wsheet.Cells[rno, 18].Value = tbl.Rows[i]["cadd2"];//r
                                Wsheet.Cells[rno, 19].Value = tbl.Rows[i]["district"];//s
                                Wsheet.Cells[rno, 20].Value = tbl.Rows[i]["cpin"];//t
                                Wsheet.Cells[rno, 21].Value = tbl.Rows[i]["statenm"];//u
                                Wsheet.Cells[rno, 22].Value = tbl.Rows[i]["cstatenm"];//v
                                Wsheet.Cells[rno, 23].Value = tbl.Rows[i]["itnm"];//w
                                Wsheet.Cells[rno, 24].Value = tbl.Rows[i]["itnm"];//x
                                Wsheet.Cells[rno, 25].Value = tbl.Rows[i]["hsncode"];//y
                                Wsheet.Cells[rno, 26].Value = tbl.Rows[i]["guomnm"];//z
                                Wsheet.Cells[rno, 27].Value = tbl.Rows[i]["qnty"];//aa
                                Wsheet.Cells[rno, 28].Value = tbl.Rows[i]["amt"];//ab
                                Wsheet.Cells[rno, 29].Value = TtlTax;//ac 
                                Wsheet.Cells[rno, 30].Value = tbl.Rows[i]["cgstamt"];//ad 
                                Wsheet.Cells[rno, 31].Value = tbl.Rows[i]["sgstamt"];//ae
                                Wsheet.Cells[rno, 32].Value = tbl.Rows[i]["igstamt"];//af
                                Wsheet.Cells[rno, 33].Value = tbl.Rows[i]["cessamt"];//ag    
                                Wsheet.Cells[rno, 34].Value = "0";//ah cess non advol
                                Wsheet.Cells[rno, 35].Value = tbl.Rows[i]["othramt"];//ai others
                            }
                            prejson.frmstatecd = rsComp.Rows[0]["statecd"].ToString();//m
                            prejson.disptchfrmstatecd = rsComp.Rows[0]["statecd"].ToString();//n
                            prejson.slnm = tbl.Rows[i]["slnm"].ToString();//o
                            prejson.togstno = tbl.Rows[i]["gstno"].ToString(); //p
                            prejson.toadd1 = tbl.Rows[i]["cadd1"].ToString();//q
                            prejson.toadd2 = tbl.Rows[i]["cadd2"].ToString();//r
                            prejson.todistrict = tbl.Rows[i]["district"].ToString();//s
                            prejson.shiptopin = tbl.Rows[i]["cpin"].ToString();//t
                            prejson.billtostcd = tbl.Rows[i]["statecd"].ToString();//u
                            prejson.shiptostcd = tbl.Rows[i]["cstatecd"].ToString();//v
                            prejson.itnm = tbl.Rows[i]["itnm"].ToString();//w
                            prejson.itdscp = tbl.Rows[i]["itnm"].ToString();//x
                            prejson.hsncode = tbl.Rows[i]["hsncode"].ToString();//y
                            prejson.guomcd = tbl.Rows[i]["guomcd"].ToString();//z
                            prejson.qnty = double.Parse(tbl.Rows[i]["qnty"].ToString());//aa
                            prejson.amt = double.Parse(tbl.Rows[i]["amt"].ToString());//ab
                            prejson.TtlTax = TtlTax;//ac
                            prejson.sgstper = double.Parse(tbl.Rows[i]["sgstper"].ToString());//json
                            prejson.cgstper = double.Parse(tbl.Rows[i]["cgstper"].ToString());//json
                            prejson.igstper = double.Parse(tbl.Rows[i]["igstper"].ToString());//json
                            prejson.cessper = double.Parse(tbl.Rows[i]["cessper"].ToString());//json
                            prejson.cgstamt = double.Parse(tbl.Rows[i]["cgstamt"].ToString());//ad
                            prejson.sgstamt = double.Parse(tbl.Rows[i]["sgstamt"].ToString());//ae
                            prejson.igstamt = double.Parse(tbl.Rows[i]["igstamt"].ToString());//af
                            prejson.cessamt = double.Parse(tbl.Rows[i]["cessamt"].ToString());//ag                           
                            prejson.cess_non_advol = 0;//ah cess non advol
                            prejson.othramt = double.Parse(tbl.Rows[i]["othramt"].ToString());//ah others
                            prejson.transMode = "1";//ah                       
                            if ((crntblno == "") || (crntblno != tbl.Rows[i]["blno"].ToString()))
                            {
                                crntblno = tbl.Rows[i]["blno"].ToString();
                                invamt = tbl.Rows[i]["blamt"].ToString();
                            }
                            prejson.invamt = double.Parse(invamt);//aj total invoice value
                            if (Command == "GENERATE EXCEL")
                            {
                                Wsheet.Cells[rno, 36].Value = invamt;//aj total invoice value
                                Wsheet.Cells[rno, 37].Value = "Road";//ak
                                Wsheet.Cells[rno, 38].Value = tbl.Rows[i]["distance"];//al
                                Wsheet.Cells[rno, 39].Value = tbl.Rows[i]["trslnm"];//am
                                Wsheet.Cells[rno, 40].Value = tbl.Rows[i]["trgst"];//an
                                Wsheet.Cells[rno, 41].Value = tbl.Rows[i]["lrno"];//ao
                                Wsheet.Cells[rno, 43].Value = tbl.Rows[i]["lorryno"];//aq
                                Wsheet.Cells[rno, 44].Value = "Regular";//ar
                            }
                            prejson.distance = Convert.ToInt32(tbl.Rows[i]["distance"].ToString());//ai
                            prejson.trslnm = tbl.Rows[i]["trslnm"].ToString();//am
                            prejson.trgst = tbl.Rows[i]["trgst"].ToString();//an
                            prejson.lrno = tbl.Rows[i]["lrno"].ToString();//ao 
                            if (tbl.Rows[i]["lrdt"].ToString() == "" && tbl.Rows[i]["lrno"].ToString() != "")
                            {
                                if (Command == "GENERATE EXCEL")
                                {
                                    Wsheet.Cells[rno, 42].Value = tbl.Rows[i]["lrdt"]; //ap
                                }
                                return Content("LR DATE NOT FOUND AT DOCNO=" + prejson.blno + ",ITEM NAME=" + prejson.itnm + "<BR/> <H3>Please put 'lr date' at the invoice</H3>");
                            }
                            else
                            {
                                prejson.lrdt = tbl.Rows[i]["lrdt"] == DBNull.Value ? "" : tbl.Rows[i]["lrdt"].ToString().Substring(0, 10);//ap
                                string lrdt = tbl.Rows[i]["lrdt"] == DBNull.Value ? "" : Convert.ToDateTime(tbl.Rows[i]["lrdt"]).ToString("MM/dd/yyyy");
                                if (Command == "GENERATE EXCEL")
                                {
                                    Wsheet.Cells[rno, 42].Value = lrdt;//ap
                                }
                            }

                            prejson.Vehicle_No = tbl.Rows[i]["lorryno"].ToString();//ap
                            prejson.Vehile_Type = "R";//aq

                            PJSON.Add(prejson);
                            i = i + 1;
                            rno = rno + 1;
                        }
                        #endregion
                        #region EXCEL DOWNLOAD
                        //Opens an Excel Template.
                        if (Command == "GENERATE EXCEL")
                        {

                            msg += " come calculTION =>";
                            Wsheet.EnableCalculation = true;
                            exlApplcn.CalculateBeforeSave = true;
                            msg += " DONE CALCULATION =>";
                            if (System.IO.File.Exists(path_Save))
                            {
                                System.IO.File.Delete(path_Save);
                            }
                            msg += " Start Save as =>";
                            workbook.SaveAs(path_Save, Excel.XlFileFormat.xlOpenXMLWorkbookMacroEnabled, Missing.Value,
                            Missing.Value, false, false, Excel.XlSaveAsAccessMode.xlNoChange,
                            Excel.XlSaveConflictResolution.xlUserResolution, true,
                            Missing.Value, Missing.Value, Missing.Value); ;
                            msg += " DONE saved =>";
                            workbook.Close();
                            exlApplcn.Quit();
                            msg += " close interop ";
                            var buffer = System.IO.File.ReadAllBytes(path_Save);
                            Response.Clear();
                            Response.ClearContent();
                            Response.Buffer = true;
                            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            Response.AddHeader("Content-Disposition", "attachment; filename=" + "EWB_" + comp + VE.DATETO.Substring(3, 2) + VE.DATETO.Substring(0, 2) + ".xlsm");
                            Response.BinaryWrite(buffer);
                            msg += " DONE All ";
                            Response.Flush();
                            Response.Close();
                            Response.End();
                            return Content("Excel exported sucessfully");
                        }
                        else
                        {
                            Prepare_JSONvalidation PVE = new Prepare_JSONvalidation();
                            PVE.Prepare_JSON = PJSON;
                            TempData["PrepareJSON"] = PVE;
                            return RedirectToAction("Prepare_JSON", "Prepare_JSON", new { US = CommFunc.Encrypt_URL(UNQSNO) });
                        }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Cn.SaveException(ex, "");
                        throw ex;
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    Cn.SaveException(ex, "");
                    return Content(msg + ex.ToString());
                }
            }
        }
    }
}
