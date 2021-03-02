using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Reflection;
using System.Web;
using Oracle.ManagedDataAccess.Client;
using System.Web.UI.WebControls;
using OfficeOpenXml;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Improvar.Controllers
{
    public class Rep_EWB_GenController : Controller
    {
        string CS = null; string doctype = "SRET,PRET,SBILL,STRFO,SOTH,SBILD,TRWB";
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        AdaequareGSP adaequareGSP = new AdaequareGSP();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_EWB_Gen
        public ActionResult Rep_EWB_Gen()
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                ViewBag.formname = "E WAY BILL GENERATION";
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                EWayBillReport VE = new EWayBillReport();
                Cn.getQueryString(VE);
                if (VE.DOC_CODE == "") VE.DOC_CODE = doctype;
                VE.DocumentType = Cn.DOCTYPE1(VE.DOC_CODE);
                string[] dt = CommVar.FinPeriod(UNQSNO).Split('-');
                VE.DATEFROM = (DateTime.Now.ToString().Remove(10));
                VE.DATETO = (DateTime.Now.ToString().Remove(10));
                if (CommVar.ClientCode(UNQSNO) == "CHEM")
                {
                    VE.Checkbox1 = true;
                    VE.Checkbox2 = true;
                }
                else
                {
                    VE.Checkbox1 = false;
                    VE.Checkbox2 = true;
                }
                return View(VE);
            }
        }
        public ActionResult ShowList(EWayBillReport VE, FormCollection FC)
        {
            try
            {
                string dbnm = CommVar.CurSchema(UNQSNO);
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
                string trntable = "", crslcd = "";
                if (Module.MODCD == "F") { trntable = "T_TXNewb"; crslcd = "translcd"; } else { trntable = "t_txntrans"; crslcd = "crslcd"; }
                string query = "";
                query += " select distinct a.autono, a.doccd, a.docno, a.docdt,a.slcd, a.slnm, a.district, a.distance,sum(a.blamt) blamt, a.trslnm, a.lorryno, a.lrno, a.lrdt,a.irnno from ";
                query += "(select a.autono, b.doccd, b.docno, b.docdt, d.slcd, d.slnm, d.district, nvl(g.distance, 0) distance, a.blamt, e.slnm trslnm, c.lorryno, c.lrno, c.lrdt, i.irnno ";
                query += "from " + fdbnm + ".t_vch_gst a,  " + dbnm + ".t_cntrl_hdr b,  " + dbnm + "." + trntable + " c, " + fdbnm + ".m_subleg d, " + fdbnm + ".m_subleg e, " + fdbnm + ".m_loca f, ";
                query += "" + fdbnm + ".m_subleg_locoth g,  " + dbnm + ".m_doctype h, " + fdbnm + ".T_TXNeinv i ";
                query += "where a.autono = b.autono and a.autono = c.autono(+) and nvl(a.conslcd, a.pcode) = d.slcd(+) and nvl(c.translcd, c." + crslcd + ") = e.slcd(+) and nvl(b.cancel, 'N') = 'N' and b.compcd ='" + comp + "' ";
                query += "and b.loccd = '" + loc + "' and b.compcd || b.loccd = f.compcd || f.loccd and trim(c.ewaybillno) is null and(b.loccd = g.loccd or g.loccd is null) and(b.compcd = g.compcd or g.compcd is null) ";
                if (doccd != "") query += " and b.doccd in (" + doccd + ")  ";
                query += "and d.slcd = g.slcd(+) and a.autono = i.AUTONO(+) and b.docdt >= to_date('" + VE.DATEFROM + "', 'dd/mm/yyyy') and b.docdt <= to_date('" + VE.DATETO + "', 'dd/mm/yyyy') ";
                query += "and a.doccd = h.doccd and h.dOCTYPE in(" + doctype.retSqlformat() + ")  ";
                query += ") a group by a.autono, a.doccd, a.docno, a.docdt,a.slcd, a.slnm, a.district, a.distance, a.trslnm, a.lorryno, a.lrno, a.lrdt,a.irnno ";
                query += "order by docdt, lrno, docno ";
                tbl = masterHelp.SQLquery(query);
                VE.EWAYBILL = (from DataRow dr in tbl.Rows
                               select new EWAYBILL()
                               {
                                   AUTONO = dr["AUTONO"].ToString(),
                                   DOCNO = dr["DOCNO"].ToString(),
                                   DOCDT = dr["DOCDT"].ToString().retDateStr(),
                                   SLNM = dr["SLNM"].ToString(),
                                   SLCD = dr["SLCD"].ToString(),
                                   DISTRICT = dr["DISTRICT"].ToString(),
                                   DISTANCE = dr["DISTANCE"].ToString(),
                                   TRSLNM = dr["TRSLNM"].ToString(),
                                   BLAMT = dr["BLAMT"].ToString().retDbl(),
                                   LRDT = dr["LRDT"].ToString(),
                                   LORRYNO = dr["LORRYNO"].ToString(),
                                   LRNO = dr["LRNO"].ToString(),
                                   LORRYNOEXIST = (dr["LORRYNO"] == DBNull.Value ? false : true),
                                   IRNNO = dr["IRNNO"].ToString(),
                               }).ToList();
                for (int i = 0; i < VE.EWAYBILL.Count; i++)
                {
                    VE.EWAYBILL[i].LRDT = VE.EWAYBILL[i].LRDT.retDateStr();
                    VE.EWAYBILL[i].SLNO = Convert.ToInt16(i + 1);
                }
                return PartialView("_Rep_EWB_Gen", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
       [HttpPost]
        public ActionResult Rep_EWB_Gen(FormCollection FC, EWayBillReport VE, string Command)
        {
            string msg = "";
            try
            {
                if (Command == "UPLOAD EXCEL")
                {
                    #region Upload Excel
                    string LOC = CommVar.Loccd(UNQSNO);
                    string COM = CommVar.Compcd(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
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

                    if (Request != null)
                    {
                        string fileExtension = System.IO.Path.GetExtension(Request.Files["UploadedFile"].FileName);
                        HttpPostedFileBase file = Request.Files["UploadedFile"];
                        if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                        {
                            using (var WB = new ExcelPackage(file.InputStream))
                            {
                                var currentSheet = WB.Workbook.Worksheets;
                                var workSheet = currentSheet.First();
                                var noOfCol = workSheet.Dimension.End.Column;
                                var noOfRow = workSheet.Dimension.End.Row;
                                for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                                {
                                    var err = workSheet.Cells[rowIterator, 3].Value.ToString();
                                    var auto_eway = (from f in VE.EWAYBILL
                                                     where f.DOCNO == workSheet.Cells[rowIterator, 3].Value.ToString()
                                                     select new
                                                     {
                                                         autono = f.AUTONO,
                                                     }).FirstOrDefault();
                                    if (auto_eway != null)
                                    {
                                        string sql = "";
                                        var schm = CommVar.CurSchema(UNQSNO);
                                        sql = "update " + schm + ".t_txntrans set ewaybillno='" + workSheet.Cells[rowIterator, 9].Value.ToString() + "' ";
                                        sql += "where autono='" + auto_eway.autono + "' and trim(ewaybillno) is null ";
                                        Cn.com = new OracleCommand(sql, Cn.con);
                                        Cn.com.ExecuteNonQuery();
                                        
                                        sql = "Update " + CommVar.FinSchema(UNQSNO) + ".T_TXNewb set EWAYBILLNO='" + workSheet.Cells[rowIterator, 9].Value.ToString() + "' " 
                                            + " where autono='" + auto_eway.autono + "'";
                                        Cn.com = new OracleCommand(sql, Cn.con);
                                        Cn.com.ExecuteNonQuery();
                                    }
                                }
                                WB.Dispose();
                            }
                            transaction.Commit();
                            Cn.con.Close();
                            return Content("Imported Successfully");
                        }
                    }
                    return Content("No Request Found. ");

                    #endregion
                }
                else if (Command == "View JSON Data")
                {
                    if (VE.EWAYBILL == null)
                    {
                        return Content("No Waybill Data Found");
                    }

                    for (int j = 0; j < VE.EWAYBILL.Count; j++)
                    {
                        if (VE.EWAYBILL[j].Checked == true)
                        {
                            msg = "ok";
                        }
                    }
                    if (msg != "ok")
                    {
                        return Content("No row Selected.");
                    }
                    var PJSON = PrepareJSONList(VE);

                    foreach (var v in PJSON)
                    {
                        if (v.message != "")
                        {
                            return Content(v.blno + " / " + v.message);
                        }
                    }
                    Prepare_JSONvalidation PVE = new Prepare_JSONvalidation();
                    PVE.Prepare_JSON = PJSON;
                    TempData["PrepareJSON"] = PVE;
                    return RedirectToAction("Prepare_JSON", "Prepare_JSON");


                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(msg + ex.ToString());
            }
            return null;
        }
        public ActionResult GenerateEWB(EWayBillReport VE)
        {
            string msg = "";
            try
            {
                if (VE.EWAYBILL == null)
                {
                    return Content("No Waybill Data Found. Click Proceed to get data.");
                }
                if (VE.EWAYBILL.Where(e => e.Checked == true).FirstOrDefault() == null)
                {
                    return Content("No row Selected. Please select a row");
                }
                VE.EWAYBILL = VE.EWAYBILL.Where(e => e.Checked == true).ToList();
                var PJSON = PrepareJSONList(VE);
                foreach (var v in PJSON)
                {
                    if (v.message != "")
                    {
                        return Content(v.blno + " / " + v.message);
                    }
                }
                //var no_bill = PJSON.Select(e => e.AUTONO).Distinct().ToArray();
                foreach (var slctrow in VE.EWAYBILL)
                {
                    string jsonstr = "";
                    var ewbBill = PJSON.Where(a => a.AUTONO == slctrow.AUTONO).ToList();
                    if (slctrow.IRNNO == null)
                    {
                        AdqrGenEWAYBILL EWBbill = new AdqrGenEWAYBILL();
                        List<AdqrEWBItemList> itemList = new List<AdqrEWBItemList>();
                        double totalTaxableAmt = 0;
                        double totaligstAmt = 0;
                        double totalcgstAmt = 0;
                        double totalsgstAmt = 0;
                        double totalcessAmt = 0;

                        for (int i = 0; i < ewbBill.Count; i++)
                        {
                            EWBbill.supplyType = ewbBill[i].Supply_Type;
                            EWBbill.subSupplyType = ewbBill[i].SubSupply_Type;
                            EWBbill.subSupplyDesc = "";
                            EWBbill.docType = ewbBill[i].Doctype;
                            EWBbill.docNo = ewbBill[i].blno;
                            EWBbill.docDate = ewbBill[i].bldt.ToShortDateString();
                            EWBbill.fromGstin = ewbBill[i].frmgstno;
                            EWBbill.fromTrdName = ewbBill[i].compnm;
                            EWBbill.fromAddr1 = ewbBill[i].frmadd1;
                            EWBbill.fromAddr2 = ewbBill[i].frmadd2;
                            EWBbill.fromPlace = ewbBill[i].frmdistrict;
                            //if (VE.SellerDtls.Addr2.Trim() == "") { VE.SellerDtls.Addr2 = "   "; }
                            //if (VE.SellerDtls.Ph.IndexOf(" ") != -1 || VE.SellerDtls.Ph.IndexOf("/") != -1 || VE.SellerDtls.Ph.IndexOf("-") != -1 || VE.SellerDtls.Ph.Length < 6 || VE.SellerDtls.Ph.Length > 12)
                            //{ msg += "Seller Phone Number(" + VE.SellerDtls.Ph + ") minimum length of 6 and a maximum length of 12,Should not contain any special char."; }
                            if (ewbBill[i].frmpin.ToString().retDcml().retInt() < 100000 || ewbBill[i].frmpin.ToString().retDcml().retInt() > 999999) { msg += "from Pincode should be 6 digit; "; continue; }
                            var erer = ewbBill[i].frmpin.ToString().retDcml().retInt();
                            EWBbill.fromPincode = Convert.ToInt32(ewbBill[i].frmpin);
                            EWBbill.fromStateCode = Convert.ToInt32(ewbBill[i].frmstatecd);
                            EWBbill.actFromStateCode = Convert.ToInt32(ewbBill[i].frmstatecd);
                            EWBbill.toGstin = ewbBill[i].togstno;
                            EWBbill.toTrdName = ewbBill[i].slnm;
                            EWBbill.toAddr1 = ewbBill[i].toadd1;
                            EWBbill.toAddr2 = ewbBill[i].toadd2;
                            EWBbill.toPlace = ewbBill[i].todistrict;
                            EWBbill.toPincode = ewbBill[i].shiptopin.retInt();//
                            EWBbill.toStateCode = Convert.ToInt32(ewbBill[i].billtostcd);
                            EWBbill.actToStateCode = Convert.ToInt32(ewbBill[i].shiptostcd);

                            EWBbill.transactionType = ewbBill[i].Transaction_Type.retInt();
                            EWBbill.dispatchFromGSTIN = EWBbill.fromGstin;
                            EWBbill.dispatchFromTradeName = EWBbill.fromTrdName;
                            EWBbill.shipToGSTIN = EWBbill.toGstin;
                            EWBbill.shipToTradeName = EWBbill.toTrdName;
                            EWBbill.otherValue = ewbBill[i].othramt;
                            totalTaxableAmt += ewbBill[i].amt;
                            EWBbill.totalValue = Math.Round(totalTaxableAmt, 2);
                            totalcgstAmt += ewbBill[i].cgstamt;
                            EWBbill.cgstValue = Math.Round(totalcgstAmt, 2);
                            totalsgstAmt += ewbBill[i].sgstamt;
                            EWBbill.sgstValue = Math.Round(totalsgstAmt, 2);
                            totaligstAmt += ewbBill[i].igstamt;
                            EWBbill.igstValue = Math.Round(totaligstAmt, 2);
                            totalcessAmt += ewbBill[i].cessamt;
                            EWBbill.cessValue = Math.Round(totalcessAmt, 2);
                            EWBbill.cessNonAdvolValue = ewbBill[i].cess_non_advol;
                            EWBbill.totInvValue = ewbBill[i].invamt;
                            EWBbill.transDistance = ewbBill[i].distance.retStr();
                            EWBbill.transporterName = ewbBill[i].trslnm == "" ? null : ewbBill[i].trslnm;
                            EWBbill.transporterId = ewbBill[i].trgst == "" ? null : ewbBill[i].trgst;
                            if (string.IsNullOrEmpty(ewbBill[i].lrno))
                            {// Transport mode is mandatory as Vehicle Number/Transport Document Number is given
                                EWBbill.transDocNo = null;
                                EWBbill.transDocDate = null;
                            }
                            else
                            {
                                EWBbill.transDocNo = ewbBill[i].lrno;
                                EWBbill.transDocDate = ewbBill[i].lrdt;
                            }
                            if (string.IsNullOrEmpty(ewbBill[i].Vehicle_No))
                            {
                                EWBbill.vehicleNo = null;
                                EWBbill.vehicleType = null;
                            }
                            else
                            {
                                EWBbill.vehicleNo = ewbBill[i].Vehicle_No;
                                EWBbill.vehicleType = ewbBill[i].Vehile_Type;
                            }
                            if (string.IsNullOrEmpty(ewbBill[i].lrno) && string.IsNullOrEmpty(ewbBill[i].Vehicle_No))
                            {
                                EWBbill.transMode = null;
                            }
                            else
                            {
                                EWBbill.transMode = ewbBill[i].transMode;
                            }
                            AdqrEWBItemList itemlst = new AdqrEWBItemList();
                            itemlst.productName = ewbBill[i].itnm;
                            itemlst.productDesc = ewbBill[i].itdscp;
                            itemlst.hsnCode = ewbBill[i].hsncode.retDcml().retInt();
                            itemlst.quantity = ewbBill[i].qnty;
                            itemlst.qtyUnit = ewbBill[i].guomcd;
                            itemlst.taxableAmount = ewbBill[i].amt;
                            itemlst.sgstRate = ewbBill[i].sgstper;
                            itemlst.cgstRate = ewbBill[i].cgstper;
                            itemlst.igstRate = ewbBill[i].igstper;
                            itemlst.cessRate = ewbBill[i].cessper;
                            itemlst.cessNonAdvol = 0;
                            itemList.Add(itemlst);
                        }
                        EWBbill.itemList = itemList;
                        jsonstr = JsonConvert.SerializeObject(EWBbill);
                        if (msg == "")
                        {
                            AdqrRespGENEWAYBILL adqrRespGENEWAYBILL = adaequareGSP.AdqrGenEwayBill(jsonstr);
                            if (adqrRespGENEWAYBILL != null && adqrRespGENEWAYBILL.success == true)
                            {
                                string sql = "";//TO_DATE('" + fdt + "', 'DD/MM/YYYY')
                                sql = "Update " + CommVar.SaleSchema(UNQSNO) + ".T_TXNTRANS set EWAYBILLNO='" + adqrRespGENEWAYBILL.result.ewayBillNo + "' where autono='" + slctrow.AUTONO + "'";
                                masterHelp.SQLNonQuery(sql);
                                sql = "Update " + CommVar.FinSchema(UNQSNO) + ".T_TXNewb set EWAYBILLNO='" + adqrRespGENEWAYBILL.result.ewayBillNo
                                    + "',EWAYBILLDT=TO_DATE('" + adqrRespGENEWAYBILL.result.ewayBillDate + "','dd/mm/yyyy hh:mi:ss AM'),EWAYBILLVALID= TO_DATE('" + adqrRespGENEWAYBILL.result.validUpto + "','dd/mm/yyyy hh:mi:ss AM')"
                                    + " where autono='" + slctrow.AUTONO + "'";
                                masterHelp.SQLNonQuery(sql);
                                slctrow.message = "" + adqrRespGENEWAYBILL.message;
                                slctrow.EWBNO = "" + adqrRespGENEWAYBILL.result.ewayBillNo;
                            }
                            else
                            {
                                slctrow.message = "" + adqrRespGENEWAYBILL.message;
                            }
                        }
                        else
                        {
                            slctrow.message = "" + msg; msg = "";
                        }
                    }//if irnno null
                    else
                    {
                        AdaequareIRNEWB adaequareIRNEWB = new AdaequareIRNEWB();
                        adaequareIRNEWB.Irn = slctrow.IRNNO;
                        adaequareIRNEWB.TransId = ewbBill[0].trgst == "" ? null : ewbBill[0].trgst;
                        adaequareIRNEWB.Distance = ewbBill[0].distance.retInt();
                        adaequareIRNEWB.TransName = ewbBill[0].trslnm == "" ? null : ewbBill[0].trslnm;
                        if (string.IsNullOrEmpty(ewbBill[0].lrno))
                        {// Transport mode is mandatory as Vehicle Number/Transport Document Number is given
                            adaequareIRNEWB.TrnDocNo = null;
                            adaequareIRNEWB.TrnDocDt = null;
                        }
                        else
                        {
                            adaequareIRNEWB.TrnDocNo = ewbBill[0].lrno;
                            adaequareIRNEWB.TrnDocDt = ewbBill[0].lrdt;
                        }
                        if (string.IsNullOrEmpty(ewbBill[0].Vehicle_No))
                        {
                            adaequareIRNEWB.VehNo = null;
                            adaequareIRNEWB.VehType = null;
                        }
                        else
                        {
                            adaequareIRNEWB.VehNo = ewbBill[0].Vehicle_No;
                            adaequareIRNEWB.VehType = ewbBill[0].Vehile_Type;
                        }
                        if (string.IsNullOrEmpty(ewbBill[0].lrno) && string.IsNullOrEmpty(ewbBill[0].Vehicle_No))
                        {
                            adaequareIRNEWB.TransMode = null;
                        }
                        else
                        {
                            adaequareIRNEWB.TransMode = ewbBill[0].transMode;
                        }
                        msg = "";
                        jsonstr = JsonConvert.SerializeObject(adaequareIRNEWB);
                        AdqrRespIRNEWB adqrRespGENEWAYBILL = adaequareGSP.AdqrGenIRNtoEWB(jsonstr);
                        if (adqrRespGENEWAYBILL.success == false && adqrRespGENEWAYBILL.message == "4002 : EwayBill is already generated for this IRN")
                        {
                            adqrRespGENEWAYBILL = adaequareGSP.AdqrGetEWBbyIRN(adaequareIRNEWB.Irn);
                        }
                        if (adqrRespGENEWAYBILL != null && adqrRespGENEWAYBILL.success == true)
                        {
                            string sql = "";//TO_DATE('" + fdt + "', 'DD/MM/YYYY')
                            sql = "Update " + CommVar.SaleSchema(UNQSNO) + ".T_TXNTRANS set EWAYBILLNO='" + adqrRespGENEWAYBILL.result.EwbNo + "' where autono='" + slctrow.AUTONO + "'";
                            masterHelp.SQLNonQuery(sql);
                            sql = "Update " + CommVar.FinSchema(UNQSNO) + ".T_TXNewb set EWAYBILLNO='" + adqrRespGENEWAYBILL.result.EwbNo
                                + "',EWAYBILLDT=TO_DATE('" + adqrRespGENEWAYBILL.result.EwbDt + "','yyyy-mm-dd hh24:mi:ss'),EWAYBILLVALID= TO_DATE('" + adqrRespGENEWAYBILL.result.EwbDt + "','yyyy-mm-dd hh24:mi:ss')"
                                + " where autono='" + slctrow.AUTONO + "'";
                            masterHelp.SQLNonQuery(sql);
                            slctrow.message = "" + adqrRespGENEWAYBILL.message;
                            slctrow.EWBNO = "" + adqrRespGENEWAYBILL.result.EwbNo;
                        }
                        else
                        {
                            slctrow.message = "" + adqrRespGENEWAYBILL.message;
                        }
                    }//else irnno not null

                }//loop ve.ewb main loop
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
            ModelState.Clear();
            return PartialView("_Rep_EWB_Gen", VE);
        }
        public List<Prepare_JSON> PrepareJSONList(EWayBillReport VE)
        {
            List<Prepare_JSON> PJSON = new List<Prepare_JSON>();
            try
            {
                #region get data
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                string dbnm = CommVar.CurSchema(UNQSNO);
                string fdbnm = CommVar.FinSchema(UNQSNO);
                string comp = CommVar.Compcd(UNQSNO);
                string loc = CommVar.Loccd(UNQSNO);
                string aauto = "";
                bool flag = false;
                for (int j = 0; j < VE.EWAYBILL.Count; j++)
                {
                    if (VE.EWAYBILL[j].Checked == true)
                    {
                        if (VE.EWAYBILL[j].LORRYNOEXIST == false && VE.EWAYBILL[j].LORRYNO.retStr() != "")
                        {
                            string sql = "";
                            sql = "update " + CommVar.SaleSchema(UNQSNO) + ".t_txntrans set lorryno='" + VE.EWAYBILL[j].LORRYNO + "' ";
                            sql += "where autono='" + VE.EWAYBILL[j].AUTONO + "' ";
                            masterHelp.SQLNonQuery(sql);
                        }
                        flag = true;
                        aauto += ",'" + VE.EWAYBILL[j].AUTONO + "' ";
                        using (var TRANS = DBF.Database.BeginTransaction())
                        {
                            M_SUBLEG_LOCOTH msuboth = new M_SUBLEG_LOCOTH();

                            msuboth.CLCD = CommVar.ClientCode(UNQSNO);
                            msuboth.SLCD = VE.EWAYBILL[j].SLCD;
                            msuboth.COMPCD = CommVar.Compcd(UNQSNO);
                            msuboth.LOCCD = CommVar.Loccd(UNQSNO);
                            msuboth.DISTANCE = VE.EWAYBILL[j].DISTANCE.retInt();
                            DBF.M_SUBLEG_LOCOTH.RemoveRange(DBF.M_SUBLEG_LOCOTH.Where(x => x.SLCD == msuboth.SLCD && x.LOCCD == msuboth.LOCCD && x.COMPCD == msuboth.COMPCD));
                            DBF.M_SUBLEG_LOCOTH.Add(msuboth);
                            DBF.SaveChanges();
                            TRANS.Commit();
                        }
                    }
                }
                if (!flag)
                {
                    PJSON.ForEach(e => e.message = "No row selected.Please select row.");
                    return PJSON;
                }
                if (aauto != "") aauto = aauto.Substring(1);
                DataTable rsComp;
                DataTable tbl;
                string query = "";

                query = "select a.locnm slnm, a.gstno, a.add1||' '||a.add2 add1, a.add3||' '||a.add4 add2, a.district, a.pin, b.compnm,a.statecd, upper(c.statenm) statenm ";
                query += "from " + fdbnm + ".m_loca a, " + fdbnm + ".m_comp b, improvar.ms_state c ";
                query += "where a.compcd=b.compcd and a.statecd=c.statecd(+) and ";
                query += "a.compcd='" + comp + "' and a.loccd='" + loc + "' ";
                rsComp = masterHelp.SQLquery(query);
                string trntable = "", crslcd = "";
                if (Module.MODCD == "F") { trntable = "T_TXNewb"; crslcd = "translcd"; } else { trntable = "t_txntrans"; crslcd = "crslcd"; }
                query = "";
                query += "select a.autono, b.doccd, a.blno, a.bldt, translate(nvl(d.fullname,d.slnm),'+[#./()]^',' ') slnm, d.gstno,";
                query += "decode(d.othaddpin,null,d.add1||' '||d.add2, d.othadd1||' '||d.othadd2) add1, decode(d.othaddpin,null,d.add3||' '||d.add4,d.othadd3||' '||d.othadd4) add2, d.district, nvl(d.othaddpin,d.pin) pin, ";
                query += "d.statecd, upper(k.statenm) statenm, translate(nvl(p.fullname,p.slnm),'+[#./()]^',' ') bslnm, p.gstno bgstno, p.add1||' '||p.add2 badd1, ";
                query += "p.add3||' '||p.add4 badd2, p.district bdistrict, p.pin bpin, p.statecd bstatecd, upper(q.statenm) bstatenm, ";
                query += "e.slnm trslnm, e.gstno trgst, e.cenno trcen, replace(translate(c.lorryno,'/-',' '),' ','') lorryno, c.lrno, c.lrdt, a.igstper, a.cgstper, a.sgstper, a.cessper, ";
                if (VE.Checkbox2 == true) query += "translate(a.itnm,'+[#/()]^',' ') itnm, a.slno, "; else query += "'' itnm, 1 slno, ";
                query += "a.hsncode, l.guomcd, l.guomnm, nvl(m.distance,0) distance, ";
                query += "o.goadd1||' '||o.goadd2 goadd1, o.goadd3 goadd2, o.district godistrict, o.pin gopin, ";
                query += "sum(decode(nvl(j.gst_qntyconv,0),0,1,j.gst_qntyconv)*a.qnty) qnty, sum(a.amt) amt, ";
                query += "nvl((select sum(blamt) blamt from " + fdbnm + ".t_vch_gst where autono=a.autono and nvl(blamt,0) <> 0),0) blamt, ";
                query += "nvl((select sum(tcsamt) tcsamt from " + fdbnm + ".t_vch_gst where autono=a.autono and nvl(tcsamt,0) <> 0),0) tcsamt, ";
                query += "sum(a.igstamt) igstamt, sum(a.cgstamt) cgstamt, sum(a.sgstamt) sgstamt, sum(a.cessamt) cessamt ,sum(a.othramt) othramt ";
                query += "from " + fdbnm + ".t_vch_gst a, " + dbnm + ".t_cntrl_hdr b, " + dbnm + "."+ trntable + " c, " + fdbnm + ".m_subleg d, ";
                query += "" + fdbnm + ".m_subleg e, " + fdbnm + ".m_loca f, " + fdbnm + ".m_uom j, improvar.ms_state k, improvar.ms_gstuom l, ";
                query += fdbnm + ".m_subleg_locoth m, " + fdbnm + ".T_TXNewb n, " + fdbnm + ".m_godown o, " + fdbnm + ".m_subleg p, " + "improvar.ms_state q ";
                query += "where a.autono=b.autono and a.autono=c.autono(+) and nvl(a.conslcd, a.pcode)=d.slcd(+) and nvl(c.translcd,c."+ crslcd +   ")=e.slcd(+) and ";
                query += "d.statecd=k.statecd(+) and a.uom=j.uomcd(+) and a.autono=n.autono(+) and n.gocd=o.gocd(+) and a.pcode=p.slcd(+) and p.statecd=q.statecd(+) and ";
                query += "nvl(j.gst_uomcd,j.uomcd)=l.guomcd(+) and (b.loccd=m.loccd or m.loccd is null) and (b.compcd=m.compcd or m.compcd is null) and d.slcd=m.slcd(+) and ";
                query += "nvl(b.cancel,'N')='N' and b.compcd='" + comp + "' and b.loccd='" + loc + "' and b.compcd||b.loccd=f.compcd||f.loccd and trim(c.ewaybillno) is null and ";
                if (aauto != "") query += "a.autono in(" + aauto + ") and ";
                query += "b.docdt >= to_date('" + VE.DATEFROM + "','dd/mm/yyyy') and b.docdt <= to_date('" + VE.DATETO + "','dd/mm/yyyy') ";
                query += "group by a.autono, b.doccd, a.blno, a.bldt, translate(nvl(d.fullname,d.slnm),'+[#./()]^',' '), d.gstno, d.district,d.statecd, upper(k.statenm), ";
                query += "decode(d.othaddpin,null,d.add1||' '||d.add2, d.othadd1||' '||d.othadd2) , decode(d.othaddpin,null,d.add3||' '||d.add4,d.othadd3||' '||d.othadd4) , nvl(d.othaddpin,d.pin) , ";
                query += "translate(nvl(p.fullname,p.slnm),'+[#./()]^',' '), p.gstno, p.add1||' '||p.add2, ";
                query += "p.add3||' '||p.add4, p.district, p.pin, p.statecd, upper(q.statenm), ";
                query += "e.slnm, e.gstno, e.cenno, replace(translate(c.lorryno,'/-',' '),' ',''), c.lrno, c.lrdt, a.igstper, a.cgstper, a.sgstper, a.cessper, ";
                if (VE.Checkbox2 == true) query += "translate(a.itnm,'+[#/()]^',' '), a.slno, "; else query += "'', 1, ";
                query += "a.hsncode,l.guomcd, l.guomnm, nvl(m.distance,0), ";
                query += "o.goadd1||' '||o.goadd2, o.goadd3, o.district, o.pin ";
                query += "order by blno, bldt, autono, slno ";
                tbl = masterHelp.SQLquery(query);
                if (tbl == null)
                {
                    PJSON.ForEach(e => e.message = "No record Found.");
                    return PJSON;
                }
                #endregion
                #region ewaybill preparation starts     
                int i = 0;
                var rno = 4;
                string TtlTax = "";
                while (i < tbl.Rows.Count)
                {
                    bool bltoshipto = false;
                    if (tbl.Rows[i]["bgstno"].ToString() != tbl.Rows[i]["gstno"].ToString()) bltoshipto = true;
                    Prepare_JSON prejson = new Prepare_JSON();
                    TtlTax = tbl.Rows[i]["sgstper"].ToString() + '+' + tbl.Rows[i]["cgstper"].ToString() + '+' + tbl.Rows[i]["igstper"].ToString() + '+' + tbl.Rows[i]["cessper"].ToString() + "+0";
                    //Wsheet.Cells[rno, 1].Value = tbl.Rows[i]["autono"];
                    prejson.SLNO = Convert.ToInt16(i + 1);
                    prejson.AUTONO = tbl.Rows[i]["AUTONO"].ToString();
                    prejson.Supply_Type = "O"; //a (Outward)
                    prejson.SubSupply_Type = "1"; //b(Supply)
                    prejson.Doctype = "INV"; //c (Tax Invoice)
                    prejson.blno = tbl.Rows[i]["blno"].ToString();//d
                    prejson.bldt = Convert.ToDateTime(tbl.Rows[i]["bldt"]);//e
                    prejson.Transaction_Type = (bltoshipto == true ? "2" : "1");//f
                    prejson.compnm = rsComp.Rows[0]["compnm"].ToString();//g
                    prejson.frmgstno = rsComp.Rows[0]["gstno"].ToString();//h
                    if (VE.Checkbox1 == true && tbl.Rows[i]["goadd1"].ToString().Trim() != "") // (loc == "KOLK" && comp == "CHEM")
                    {
                        prejson.frmadd1 = tbl.Rows[i]["goadd1"].ToString();//i
                        prejson.frmadd2 = tbl.Rows[i]["goadd2"].ToString();//j
                        prejson.frmdistrict = tbl.Rows[i]["godistrict"].ToString();//k
                        prejson.frmpin = tbl.Rows[i]["gopin"].ToString();//l
                    }
                    else
                    {
                        prejson.frmadd1 = rsComp.Rows[0]["add1"].ToString();//i
                        prejson.frmadd2 = rsComp.Rows[0]["add2"].ToString();//j
                        prejson.frmdistrict = rsComp.Rows[0]["district"].ToString();//k
                        prejson.frmpin = rsComp.Rows[0]["pin"].ToString();//l
                    }
                    prejson.frmstatecd = rsComp.Rows[0]["statecd"].ToString();//m
                    prejson.disptchfrmstatecd = rsComp.Rows[0]["statecd"].ToString();//n
                    prejson.slnm = (bltoshipto == true ? tbl.Rows[i]["bslnm"].ToString() : tbl.Rows[i]["slnm"].ToString());//o
                    prejson.togstno = tbl.Rows[i]["bgstno"].ToString(); //p
                    prejson.toadd1 = (bltoshipto == true ? tbl.Rows[i]["badd1"].ToString() : tbl.Rows[i]["add1"].ToString());
                    prejson.toadd2 = (bltoshipto == true ? tbl.Rows[i]["badd2"].ToString() : tbl.Rows[i]["add2"].ToString());
                    prejson.todistrict = (bltoshipto == true ? tbl.Rows[i]["bdistrict"].ToString() : tbl.Rows[i]["district"].ToString());//s
                    prejson.shiptopin = tbl.Rows[i]["pin"].ToString();//t
                    prejson.billtostcd = tbl.Rows[i]["bstatecd"].ToString();//u
                    prejson.shiptostcd = tbl.Rows[i]["statecd"].ToString();//v
                    prejson.itnm = tbl.Rows[i]["itnm"].ToString();//w
                    prejson.itdscp = tbl.Rows[i]["itnm"].ToString();//x
                    prejson.hsncode = tbl.Rows[i]["hsncode"].ToString();//y
                    prejson.guomcd = tbl.Rows[i]["guomcd"].ToString();//z
                    prejson.qnty = tbl.Rows[i]["qnty"].retDbl();//aa
                    prejson.amt = tbl.Rows[i]["amt"].retDbl();//ab
                    prejson.TtlTax = TtlTax;//ac
                    prejson.sgstper = tbl.Rows[i]["sgstper"].retDbl();//json
                    prejson.cgstper = tbl.Rows[i]["cgstper"].retDbl();//json
                    prejson.igstper = tbl.Rows[i]["igstper"].retDbl();//json
                    prejson.cessper = tbl.Rows[i]["cessper"].retDbl();//json
                    prejson.cgstamt = tbl.Rows[i]["cgstamt"].retDbl();//ad
                    prejson.sgstamt = tbl.Rows[i]["sgstamt"].retDbl();//ae
                    prejson.igstamt = tbl.Rows[i]["igstamt"].retDbl();//af
                    prejson.cessamt = tbl.Rows[i]["cessamt"].retDbl();//ag                           
                    prejson.cess_non_advol = 0;//ah cess non advol
                    prejson.othramt = tbl.Rows[i]["othramt"].retDbl();//ah others
                    prejson.transMode = "1";//ah
                    prejson.invamt = tbl.Rows[i]["blamt"].retDbl();
                    prejson.distance = tbl.Rows[i]["distance"].retInt();
                    prejson.trslnm = tbl.Rows[i]["trslnm"].ToString();//am
                    if (tbl.Rows[i]["trcen"].retStr() != "")
                    {
                        prejson.trgst = tbl.Rows[i]["trcen"].ToString();//an
                    }
                    else
                    {
                        prejson.trgst = tbl.Rows[i]["trgst"].ToString();//an
                    }
                    prejson.lrno = tbl.Rows[i]["lrno"].ToString();//ao 
                    if (tbl.Rows[i]["lrdt"].ToString() == "" && tbl.Rows[i]["lrno"].ToString() != "")
                    {
                        if (tbl == null)
                        {
                            PJSON.ForEach(e => e.message = "LR DATE NOT FOUND AT DOCNO=" + prejson.blno + ",ITEM NAME=" + prejson.itnm + "<BR/> <H3>Please put 'lr date' at the invoice</H3>");
                            return PJSON;
                        }
                    }
                    else
                    {
                        prejson.lrdt = tbl.Rows[i]["lrdt"] == DBNull.Value ? "" : tbl.Rows[i]["lrdt"].ToString().Substring(0, 10);//ap
                    }
                    prejson.Vehicle_No = tbl.Rows[i]["lorryno"].ToString();//ap
                    prejson.Vehile_Type = "R";//aq

                    PJSON.Add(prejson);
                    i = i + 1;
                    rno = rno + 1;
                }//while
                PJSON.ForEach(e => e.message = "");
                #endregion
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
            return PJSON;
        }

    }
}
