using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using CrystalDecisions.CrystalReports.Engine;
using System.Data;
using System.IO;
using System.Collections.Generic;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Newtonsoft.Json;

namespace Improvar.Controllers
{
    public class Gen_EinvIRNController : Controller
    {
        string sql = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        string LOC = CommVar.Loccd(CommVar.getQueryStringUNQSNO()), COM = CommVar.Compcd(CommVar.getQueryStringUNQSNO()), scm1 = CommVar.CurSchema(CommVar.getQueryStringUNQSNO()), scmf = CommVar.FinSchema(CommVar.getQueryStringUNQSNO());
        // GET: Gen_EinvIRN
        public ActionResult Gen_EinvIRN(string reptype = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    AdaequareGSP adaequareGSP = new AdaequareGSP();
                    GenEinvIRN VE = new GenEinvIRN();
                    sql = "select EINVAPPL from " + CommVar.FinSchema(UNQSNO) + ".m_comp where compcd='" + CommVar.Compcd(UNQSNO) + "'";
                    var dt = masterHelp.SQLquery(sql);
                    if (dt.Rows[0]["EINVAPPL"].retStr() != "Y")
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                        ViewBag.ErrorMessage = "[EINVAPPL] ENABLED FROM COMPANY MASTER.";
                        return View(VE);
                    }
                    if (adaequareGSP.AppType == "STAGING" && Session["UR_ID"].ToString() != "IPSTEAM")
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                        ViewBag.ErrorMessage = "[STAGING] MODE ENABLED.    PLEASE LOGIN WITH USERID:IPSTEAM.   Contact Admin.";
                        return View(VE);
                    }
                    VE.AppType = adaequareGSP.AppType;
                    ViewBag.formname = "Generate E Invoice";
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    VE.FROMDT = CommVar.CurrDate(UNQSNO);
                    VE.TODT = CommVar.CurrDate(UNQSNO);
                    VE.DefaultView = true;
                    VE.ExitMode = 1;
                    VE.DefaultDay = 0;
                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                GenEinvIRN VE = new GenEinvIRN();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }

        [HttpPost]
        public ActionResult Gen_EinvIRN(GenEinvIRN VE, string submitbutton)
        {
            try
            {
                string fdt = VE.FROMDT, tdt = VE.TODT;
                ExcelPackage ExcelPkg = new ExcelPackage();
                ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("Sheet1");
                if (submitbutton == "Download IRN Excel")
                {
                    if (CommVar.Compcd(UNQSNO) == "CDET")
                    {
                        var autonos = string.Join(",", (VE.GenEinvIRNGrid.Where(e => e.IRNNO != null).Select(e => e.AUTONO))).retSqlformat();
                        var dt = GetIRNData(fdt, tdt, autonos);
                        if (dt.Rows.Count == 0) return Content("No IRN Found !");
                        DataTable[] exdt = new DataTable[1] { dt };
                        string[] sheetname = new string[1] { "Sheet1" };
                        masterHelp.ExcelfromDataTables(exdt, sheetname, "IRN".retRepname(), false, "");
                    }
                    else
                    {

                        string Excel_Header = "SLNO" + "|" + "DOCUMENT NO." + "|" + "DOCUMENT DATE " + "|" + "SLNM" + "|" + "BILL AMOUNT" + "|" + "IRN NO." + "|" + "EWB" + "|" + "MESSAGE";
                        using (ExcelRange Rng = wsSheet1.Cells["A1:H1"])
                        {
                            Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                            Rng.Style.Font.Size = 14; Rng.Style.Font.Bold = true;
                            wsSheet1.Cells["A1:A1"].Value = CommVar.CompName(UNQSNO);
                        }
                        using (ExcelRange Rng = wsSheet1.Cells["A2:H2"])
                        {
                            Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                            Rng.Style.Font.Size = 12; Rng.Style.Font.Bold = true;
                            wsSheet1.Cells["A2:A2"].Value = CommVar.LocName(UNQSNO);
                        }
                        using (ExcelRange Rng = wsSheet1.Cells["A3:H3"])
                        {
                            Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                            Rng.Style.Font.Size = 11; Rng.Style.Font.Bold = true;
                            wsSheet1.Cells["A3:A3"].Value = "Generate E Invoice as on " + fdt + " to " + tdt;
                        }
                        using (ExcelRange Rng = wsSheet1.Cells["A4:H4"])
                        {
                            Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            Rng.Style.Font.Bold = true;
                            Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SkyBlue);
                            string[] Header = Excel_Header.Split('|');
                            for (int j = 0; j < Header.Length; j++)
                            {
                                wsSheet1.Cells[4, j + 1].Value = Header[j];
                                // wsSheet1.SelectedRange[4, j + 1].Style.WrapText = true;

                            }
                        }
                        string filename = "irn".retRepname();
                        int er1 = 5;
                        if (VE.GenEinvIRNGrid != null)
                        {
                            for (int i = 0; i <= VE.GenEinvIRNGrid.Count - 1; i++)
                            {
                                if (VE.GenEinvIRNGrid[i].IRNNO != null)
                                {
                                    wsSheet1.Cells[er1, 1].Value = VE.GenEinvIRNGrid[i].SLNO;
                                    wsSheet1.Cells[er1, 2].Value = VE.GenEinvIRNGrid[i].BLNO;
                                    wsSheet1.Cells[er1, 3].Value = VE.GenEinvIRNGrid[i].BLDT;
                                    wsSheet1.Cells[er1, 4].Value = VE.GenEinvIRNGrid[i].SLNM;
                                    wsSheet1.Cells[er1, 5].Value = VE.GenEinvIRNGrid[i].BLAMT;
                                    wsSheet1.Cells[er1, 6].Value = VE.GenEinvIRNGrid[i].IRNNO;
                                    wsSheet1.Cells[er1, 7].Value = VE.GenEinvIRNGrid[i].EWB;
                                    wsSheet1.Cells[er1, 8].Value = VE.GenEinvIRNGrid[i].MESSAGE;
                                    er1++;
                                }
                            }
                        }
                        Response.Clear();
                        Response.ClearContent();
                        Response.Buffer = true;
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AddHeader("Content-Disposition", "attachment; filename=" + filename + ".xlsx");
                        Response.BinaryWrite(ExcelPkg.GetAsByteArray());
                        Response.Flush();
                        Response.Close();
                        Response.End();
                        return Content("<div style='margin-top: 21%; */'><center style='color:green'><h1>Download Successfully...</h1></center></div>");
                    }
                }
                else if (submitbutton == "IRN Report")
                {
                    var dt = GetIRNData(fdt, tdt, "");
                    if (dt.Rows.Count == 0) return Content("No IRN Found !");
                    DataTable[] exdt = new DataTable[1] { dt };
                    string[] sheetname = new string[1] { "Sheet1" };
                    if (CommVar.Compcd(UNQSNO) == "CDET")
                    {
                        masterHelp.ExcelfromDataTables(exdt, sheetname, "IRN".retRepname(), false, "");
                    }
                    else
                    {
                        masterHelp.ExcelfromDataTables(exdt, sheetname, "IRN".retRepname(), false, "IRN Details as on " + fdt + " to " + tdt);
                    }
                }
                else
                {
                    string text = GetIRNdetails(VE, true);
                    string filename = CommFunc.retRepname(Session["GSTNO"].retStr()) + ".txt";
                    Response.Clear();
                    Response.ClearHeaders();
                    Response.AppendHeader("Content-Length", text.Length.ToString());
                    Response.ContentType = "text/plain";
                    Response.AppendHeader("Content-Disposition", "attachment;filename=" + filename);
                    Response.Write(text);
                    Response.End();
                    return Content("Export Successfully.");
                }

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException + "  ");
            }
            return Content("Export Successfully.");
        }

        private DataTable GetIRNData(string fdt, string tdt, string autonos)
        {
            sql = "";
            if (CommVar.Compcd(UNQSNO) == "CDET")
            {
                sql = " select distinct ROW_NUMBER() OVER (ORDER BY blno) \"S.No.\",'Outward' \"Supply Type\", f.sapblno \"Doc.No.\",  to_char(b.docdt,'dd.mm.yyyy')      \"Doc.Date.\", ";
                sql += "  c.gstno \"Other Party GSTIN\", c.state  \"Supply State\" , e.lorryno \"Vehicle No\", count(*)  \"No. of items\", e.ewaybillno \"Ewaybill \",    ";
                sql += "  to_char(a.ackdt,'fmMM/DD/yyyy:hh24:mi:ss') \"Bill Generated Date\" , a.irnno \"IRN number\", b.blno \"Our Docno\" ";
                sql += " from  " + CommVar.FinSchema(UNQSNO) + ".t_txneinv a,  " + CommVar.FinSchema(UNQSNO) + ".t_vch_gst b,  " + CommVar.FinSchema(UNQSNO) + ".m_subleg c, ";
                sql += "  " + CommVar.FinSchema(UNQSNO) + ".t_cntrl_hdr d,  " + CommVar.FinSchema(UNQSNO) + ".t_txnewb e,  " + CommVar.SaleSchema(UNQSNO) + ".t_txnoth f ";
                sql += " where a.autono = b.autono(+) and b.pcode = c.slcd(+) and a.autono = d.autono(+) and ";
                sql += " d.compcd = '" + CommVar.Compcd(UNQSNO) + "' and ";
                sql += " a.autono = e.autono(+) and a.autono = f.autono(+) ";
                if (autonos != "") sql += " and b.autono in(" + autonos + ") ";
                sql += " AND  b.docdt >= TO_DATE('" + fdt + "', 'DD/MM/YYYY') AND b.docdt <= TO_DATE('" + tdt + "', 'DD/MM/YYYY')   ";
                sql += " group by f.sapblno, b.docdt, c.gstno, c.state, e.lorryno, e.ewaybillno,a.ackdt, a.irnno, b.blno ";
                sql += " order by  to_char(b.docdt,'dd.mm.yyyy')  , b.blno";

            }
            else
            {
                sql = " select distinct b.docdt,b.blno, c.slnm,sum(B.BLAMT)BLAMT, e.ewaybillno, a.ackdt, a.irnno ";
                sql += " from " + CommVar.FinSchema(UNQSNO) + ".t_txneinv a, " + CommVar.FinSchema(UNQSNO) + ".t_vch_gst b, " + CommVar.FinSchema(UNQSNO) + ".m_subleg c, ";
                sql += " " + CommVar.FinSchema(UNQSNO) + ".t_cntrl_hdr d, " + CommVar.FinSchema(UNQSNO) + ".t_txnewb e ";
                sql += " where a.autono = b.autono(+) and b.pcode = c.slcd(+) and a.autono = d.autono(+) and ";
                sql += " b.docdt >= TO_DATE('" + fdt + "', 'DD/MM/YYYY') AND b.docdt <= TO_DATE('" + tdt + "', 'DD/MM/YYYY')   ";
                sql += " d.compcd = '" + CommVar.Compcd(UNQSNO) + "' and a.autono = e.autono(+) and d.loccd = '" + CommVar.Loccd(UNQSNO) + "' ";
                sql += " group by b.docdt, c.slnm, e.lorryno, e.ewaybillno,a.ackdt, a.irnno, b.blno ";
                sql += " order by b.docdt, b.blno ";

            }
            var dt = masterHelp.SQLquery(sql);
            return dt;
        }

        public ActionResult ShowList(GenEinvIRN VE, FormCollection FC)
        {
            try
            {
                string tdt = VE.TODT;
                string fdt = VE.FROMDT;

                sql = "";
                string scmf = CommVar.FinSchema(UNQSNO);
                sql = "select distinct a.autono,c.doctype,d.docno,d.docdt,b.SLCD,b.SLNM,sum(a.BLAMT ) BLAMT from " + scmf + ".t_vch_gst a,"
                + scmf + ".m_subleg b," + scmf + ".m_doctype c," + scmf + ".t_cntrl_hdr d "
                    + " where a.pcode=b.slcd and  a.doccd=c.doccd and  a.autono=d.autono and ";
                sql += " A.docdt >= TO_DATE('" + fdt + "', 'DD/MM/YYYY') AND A.docdt <= TO_DATE('" + tdt + "', 'DD/MM/YYYY') AND  ";
                sql += " b.regntype in ('R') and a.salpur='S' and nvl(a.exemptedtype,' ') <> 'Z' and a.expcd is null ";
                sql += " and a.autono not in (select autono from " + scmf + ".t_txneinv) ";
                sql += " and d.compcd='" + CommVar.Compcd(UNQSNO) + "' and d.loccd='" + CommVar.Loccd(UNQSNO) + "'  and b.gstno is not null ";
                sql += " group by a.autono,c.doctype,d.docno,d.docdt,b.SLCD,b.SLNM ";
                sql += " order by docdt,autono ";
                DataTable txn = masterHelp.SQLquery(sql);
                VE.GenEinvIRNGrid = (from DataRow dr in txn.Rows
                                     select new GenEinvIRNGrid
                                     {
                                         AUTONO = dr["AUTONO"].retStr(),
                                         //DOCTYPE = dr["doctype"].retStr(),
                                         BLNO = dr["docno"].retStr(),
                                         BLDT = dr["docdt"].retDateStr(),
                                         SLNM = dr["SLCD"].retStr() == "" ? "" : dr["SLNM"].retStr() + "[" + dr["SLCD"].retStr() + "]",
                                         BLAMT = dr["BLAMT"].retDbl()
                                     }).OrderBy(a => a.BLDT).Distinct().ToList();
                int slno = 1;
                for (int p = 0; p <= VE.GenEinvIRNGrid.Count - 1; p++)
                {
                    VE.GenEinvIRNGrid[p].SLNO = (slno + p).retInt();
                }
                ModelState.Clear();
                return PartialView("_Gen_EinvIRN", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }

        public ActionResult GenerateEinvIRN(GenEinvIRN VE)
        {
            try
            {
                VE = GetIRNdetails(VE, false);
                VE.DefaultView = true;
                ModelState.Clear();
                return PartialView("_Gen_EinvIRN", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException + "  ");
            }
        }

        private dynamic GetIRNdetails(GenEinvIRN VE, bool CallForValidation)
        {
            try
            {
                AdaequareGSP adaequareGSP = new AdaequareGSP();
                for (int gridindex = 0; gridindex < VE.GenEinvIRNGrid.Count; gridindex++)
                {
                    string autono = VE.GenEinvIRNGrid[gridindex].AUTONO;
                    if (VE.GenEinvIRNGrid[gridindex].Checked == true)
                    {
                        string dbnm = CommVar.SaleSchema(UNQSNO);
                        string fdbnm = CommVar.FinSchema(UNQSNO);
                        string comp = CommVar.Compcd(UNQSNO);
                        string loc = CommVar.Loccd(UNQSNO);
                        var sql = "";
                        DataTable rsComp;
                        sql = "select a.locnm, a.gstno, a.add1||' '||a.add2 add1, a.add3||' '||a.add4 add2, a.district, a.pin,"
                            + " b.compnm,a.statecd,nvl(propname,b.compnm) LegalNm,upper(c.statenm) statenm,a.regemailid,nvl(a.phno1std||a.phno1,a.regmobile) regph ";
                        sql += "from " + fdbnm + ".m_loca a, " + fdbnm + ".m_comp b, ms_state c ";
                        sql += "where a.compcd=b.compcd and a.statecd=c.statecd(+) and ";
                        sql += "a.compcd='" + comp + "' and a.loccd='" + loc + "' ";
                        rsComp = masterHelp.SQLquery(sql);
                        SellerDtls SellerDtls = new SellerDtls()
                        {
                            Gstin = rsComp.Rows[0]["gstno"].retStr(),
                            LglNm = rsComp.Rows[0]["LegalNm"].retStr(),
                            TrdNm = rsComp.Rows[0]["LegalNm"].retStr(),
                            Addr1 = rsComp.Rows[0]["add1"].retStr(),
                            Addr2 = rsComp.Rows[0]["add2"].retStr(),
                            Loc = rsComp.Rows[0]["district"].retStr(),
                            Pin = rsComp.Rows[0]["pin"].ToString().retDcml().retInt(),
                            Stcd = rsComp.Rows[0]["statecd"].retStr(),
                            Ph = rsComp.Rows[0]["regph"].retStr(),
                            Em = rsComp.Rows[0]["regemailid"].retStr(),
                        };

                        sql = "";
                        sql += "select a.autono, a.slno, b.doccd, a.blno, a.bldt, a.pos,a.gstslnm posslnm,a.gstno posgstno,a.gstslpin pospin,a.gstsldist posdist ,a.gstsladd1 posadd1,a.gstsladd2 posadd2,";
                        sql += "a.SHIPDOCNO,a.SHIPDOCDT,a.PORTCD, a.basamt , a.discamt,a.GOOD_SERV,dncnsalpur, ";
                        sql += " translate(nvl(d.fullname,d.slnm),'+[#./()]^',' ') slnm, d.gstno,p.PROPNAME LegalNm, d.add1||' '||d.add2 add1, d.add3||' '||d.add4 add2, d.district, d.pin,d.statecd, upper(k.statenm) statenm, ";
                        sql += "translate(nvl(p.fullname,p.slnm),'+[#./()]^',' ') bslnm, p.gstno bgstno,p.PROPNAME bLegalNm, p.add1||' '||p.add2 badd1, ";
                        sql += "p.add3||' '||p.add4 badd2, p.district bdistrict, p.pin bpin, p.statecd bstatecd, upper(q.statenm) bstatenm, p.regemailid bregemailid,nvl(p.phno1std||p.phno1,p.regmobile) bregph, ";
                        sql += "e.slnm trslnm, e.gstno trgst, replace(translate(c.lorryno,'/-',' '),' ','') lorryno, c.lrno, c.lrdt,a.rate, a.igstper, a.cgstper, a.sgstper, a.cessper, ";
                        sql += "translate(a.itnm,'+[#/()]^',' ') itnm, ";
                        sql += "a.hsncode, l.guomcd, l.guomnm, ";
                        sql += "o.gonm,o.goadd1||' '||o.goadd2 goadd1, o.goadd3 goadd2, o.district godistrict, o.pin gopin, ";
                        sql += "decode(nvl(j.gst_qntyconv,0),0,1,j.gst_qntyconv)*a.qnty qnty, a.amt txablamt, ";
                        sql += "nvl((select distinct distance from " + fdbnm + ".m_subleg_locoth where slcd=a.pcode and compcd||loccd=b.compcd||b.loccd and nvl(distance,0) <> 0),0) distance, ";
                        sql += "nvl((select sum(blamt) blamt from " + fdbnm + ".t_vch_gst where autono=a.autono and nvl(blamt,0) <> 0),0) blamt, ";
                        sql += "nvl((select sum(roamt) roamt from " + fdbnm + ".t_vch_gst where autono=a.autono and nvl(roamt,0) <> 0),0) roamt,  ";
                        sql += "nvl((select sum(tcsamt) tcsamt from " + fdbnm + ".t_vch_gst where autono=a.autono and nvl(tcsamt,0) <> 0),0) tcsamt, ";
                        sql += "a.igstamt, a.cgstamt, a.sgstamt, a.cessamt , a.othramt ";
                        sql += "from " + fdbnm + ".t_vch_gst a, " + fdbnm + ".t_cntrl_hdr b, " + fdbnm + ".t_txnewb c, " + fdbnm + ".m_subleg d, ";
                        sql += "" + fdbnm + ".m_subleg e, " + fdbnm + ".m_loca f, " + fdbnm + ".m_uom j, improvar.ms_state k, improvar.ms_gstuom l, ";
                        sql += fdbnm + ".m_godown o, " + fdbnm + ".m_subleg p, " + "improvar.ms_state q ";
                        sql += "where a.autono=b.autono and a.autono=c.autono(+) and nvl(a.conslcd, a.pcode)=d.slcd(+) and c.translcd=e.slcd(+) and ";
                        sql += "d.statecd=k.statecd(+) and a.uom=j.uomcd(+) and c.gocd=o.gocd(+) and a.pcode=p.slcd(+) and p.statecd=q.statecd(+) and ";
                        sql += "nvl(j.gst_uomcd,j.uomcd)=l.guomcd(+) and ";
                        sql += "nvl(b.cancel,'N')='N' and b.compcd='" + comp + "' and b.loccd='" + loc + "' and b.compcd||b.loccd=f.compcd||f.loccd  and a.autono = '" + autono + "'";
                        sql += "order by blno, bldt, autono, slno ";
                        DataTable dt = masterHelp.SQLquery(sql);
                        if (dt.Rows.Count == 0) { VE.Message = "No Data; " + sql; return VE; }
                        AdaequareIRN adaequareIRN = new AdaequareIRN();
                        TranDtls tranDtls = new TranDtls();
                        DocDtls docDtls = new DocDtls();
                        BuyerDtls buyerDtls = new BuyerDtls();
                        DispDtls dispDtls = new DispDtls();
                        ShipDtls shipDtls = new ShipDtls();
                        List<ItemList> itemLists = new List<ItemList>();
                        ValDtls valDtls = new ValDtls();
                        PayDtls payDtls = new PayDtls();
                        ExpDtls expDtls = new ExpDtls();
                        EwbDtls ewbDtls = new EwbDtls();
                        double igstamt = 0, sgstamt = 0, cgstamt = 0, cessamt = 0, roamt = 0, blamt = 0, tottxablamt = 0;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            docDtls = new DocDtls();
                            if (dt.Rows[i]["dncnsalpur"].retStr() == "SD") { docDtls.Typ = "DBN"; }
                            else if (dt.Rows[i]["dncnsalpur"].retStr() == "SC") { docDtls.Typ = "CRN"; }
                            else { docDtls.Typ = "INV"; }
                            docDtls.No = dt.Rows[i]["blno"].retStr();
                            docDtls.Dt = dt.Rows[i]["bldt"].retDateStr();
                            //BuyerDtls
                            //BuyerDtls
                            if (dt.Rows[i]["posgstno"].retStr() != "")
                            {
                                buyerDtls.Gstin = dt.Rows[i]["posgstno"].retStr();
                                buyerDtls.LglNm = dt.Rows[i]["posslnm"].retStr();
                                buyerDtls.TrdNm = dt.Rows[i]["posslnm"].retStr();
                                buyerDtls.Pos = dt.Rows[i]["pos"].retStr();
                                buyerDtls.Addr1 = dt.Rows[i]["posadd1"].retStr();
                                buyerDtls.Addr2 = dt.Rows[i]["posadd2"].retStr();
                                buyerDtls.Loc = dt.Rows[i]["posdist"].retStr();
                                buyerDtls.Pin = dt.Rows[i]["pospin"].retDcml().retInt();
                                buyerDtls.Stcd = dt.Rows[i]["bstatecd"].retStr();
                                buyerDtls.Ph = dt.Rows[i]["bregemailid"].retStr();
                                buyerDtls.Em = dt.Rows[i]["bregph"].retStr();
                            }
                            else {
                                buyerDtls.Gstin = dt.Rows[i]["bgstno"].retStr();
                                buyerDtls.LglNm = dt.Rows[i]["bLegalNm"].retStr() == "" ? dt.Rows[i]["bslnm"].retStr() : dt.Rows[i]["bLegalNm"].retStr();
                                buyerDtls.TrdNm = dt.Rows[i]["bslnm"].retStr();
                                buyerDtls.Pos = dt.Rows[i]["bstatecd"].retStr();
                                buyerDtls.Addr1 = dt.Rows[i]["badd1"].retStr();
                                buyerDtls.Addr2 = dt.Rows[i]["badd2"].retStr();
                                buyerDtls.Loc = dt.Rows[i]["bdistrict"].retStr();
                                buyerDtls.Pin = dt.Rows[i]["bpin"].ToString().retDcml().retInt();
                                buyerDtls.Stcd = dt.Rows[i]["bstatecd"].retStr();
                                buyerDtls.Ph = dt.Rows[i]["bregemailid"].retStr();
                                buyerDtls.Em = dt.Rows[i]["bregph"].retStr();
                            }

                            // dispatch                          
                            if (dt.Rows[0]["gonm"].ToString().Trim() != "" && dt.Rows[i]["goadd1"].ToString().Trim() != "") // (loc == "KOLK" && comp == "CHEM")
                            {
                                dispDtls.Nm = dt.Rows[i]["gonm"].retStr();
                                dispDtls.Addr1 = dt.Rows[i]["goadd1"].retStr();
                                dispDtls.Addr2 = dt.Rows[i]["goadd2"].retStr();
                                dispDtls.Loc = dt.Rows[i]["godistrict"].retStr();
                                dispDtls.Pin = dt.Rows[i]["gopin"].ToString().retDcml().retInt();
                                dispDtls.Stcd = rsComp.Rows[0]["statecd"].retStr();
                            }
                            else
                            {
                                dispDtls.Nm = rsComp.Rows[0]["compnm"].retStr();
                                dispDtls.Addr1 = rsComp.Rows[0]["add1"].retStr();
                                dispDtls.Addr2 = rsComp.Rows[0]["add2"].retStr();
                                dispDtls.Loc = rsComp.Rows[0]["district"].retStr();
                                dispDtls.Pin = rsComp.Rows[0]["pin"].ToString().retDcml().retInt();
                                dispDtls.Stcd = rsComp.Rows[0]["statecd"].retStr();
                            }

                            //consign   ==paRTY              
                            shipDtls.Gstin = dt.Rows[i]["gstno"].retStr();
                            shipDtls.LglNm = dt.Rows[i]["LegalNm"].retStr() == "" ? dt.Rows[i]["slnm"].retStr() : dt.Rows[i]["LegalNm"].retStr();// dt.Rows[i][""].retStr();
                            shipDtls.TrdNm = dt.Rows[i]["slnm"].retStr();
                            shipDtls.Addr1 = dt.Rows[i]["add1"].retStr();
                            shipDtls.Addr2 = dt.Rows[i]["add2"].retStr();
                            shipDtls.Loc = dt.Rows[i]["district"].retStr();
                            shipDtls.Pin = dt.Rows[i]["pin"].ToString().retDcml().retInt();
                            shipDtls.Stcd = dt.Rows[i]["statecd"].retStr();

                            ItemList itemlst = new ItemList();
                            itemlst.SlNo = dt.Rows[i]["slno"].retStr();
                            itemlst.PrdDesc = dt.Rows[i]["itnm"].retStr();
                            itemlst.IsServc = dt.Rows[i]["GOOD_SERV"].retStr() == "S" ? "Y" : "N";
                            itemlst.HsnCd = dt.Rows[i]["hsncode"].retStr();
                            itemlst.Barcde = null;
                            itemlst.Qty = dt.Rows[i]["qnty"].ToString().retDbl().toRound(3);
                            itemlst.FreeQty = 0;
                            itemlst.Unit = dt.Rows[i]["guomcd"].retStr();
                            itemlst.UnitPrice = dt.Rows[i]["rate"].retDbl().toRound(3);
                            itemlst.TotAmt = dt.Rows[i]["basamt"].retDbl();
                            itemlst.Discount = dt.Rows[i]["discamt"].retDbl();
                            itemlst.PreTaxVal = dt.Rows[i]["txablamt"].retDbl();
                            itemlst.AssAmt = dt.Rows[i]["txablamt"].retDbl();
                            tottxablamt += dt.Rows[i]["txablamt"].retDbl();
                            itemlst.GstRt = dt.Rows[i]["cgstper"].retDbl() + dt.Rows[i]["sgstper"].retDbl() + dt.Rows[i]["igstper"].retDbl();
                            itemlst.IgstAmt = dt.Rows[i]["igstamt"].retDbl();
                            igstamt += dt.Rows[i]["igstamt"].retDbl();
                            //double igstamt = 0, sgstamt = 0, cgstamt = 0,  = 0;
                            itemlst.CgstAmt = dt.Rows[i]["cgstamt"].retDbl();
                            cgstamt += dt.Rows[i]["cgstamt"].retDbl();
                            itemlst.SgstAmt = dt.Rows[i]["sgstamt"].retDbl();
                            sgstamt += dt.Rows[i]["sgstamt"].retDbl();
                            itemlst.CesRt = dt.Rows[i]["cessper"].retDbl();
                            itemlst.CesAmt = dt.Rows[i]["cessamt"].retDbl();
                            cessamt += dt.Rows[i]["cessamt"].retDbl();
                            itemlst.CesNonAdvlAmt = 0;
                            itemlst.StateCesRt = 0;
                            itemlst.StateCesNonAdvlAmt = 0;
                            itemlst.OthChrg = 0;
                            itemlst.TotItemVal = (dt.Rows[i]["txablamt"].retDbl() + dt.Rows[i]["cgstamt"].retDbl() + dt.Rows[i]["sgstamt"].retDbl() + dt.Rows[i]["igstamt"].retDbl()).toRound(2);
                            itemlst.OrdLineRef = null;
                            itemlst.OrgCntry = null;
                            itemlst.PrdSlNo = dt.Rows[i]["slno"].retStr();
                            itemLists.Add(itemlst);
                            //
                            roamt = dt.Rows[i]["roamt"].retDbl();
                            blamt = dt.Rows[i]["blamt"].retDbl();
                            //expDtls
                            expDtls.ShipBNo = dt.Rows[i]["SHIPDOCNO"].retStr();
                            expDtls.ShipBDt = dt.Rows[i]["SHIPDOCDT"].retDateStr();
                            expDtls.Port = dt.Rows[i]["PORTCD"].retStr();
                            //EwbDtls
                            ewbDtls.Transid = dt.Rows[i]["trgst"].retStr() == "" ? null : dt.Rows[i]["trgst"].retStr();
                            ewbDtls.Transname = dt.Rows[i]["trslnm"].retStr() == "" ? null : dt.Rows[i]["trslnm"].retStr();
                            ewbDtls.Distance = dt.Rows[i]["distance"].retInt();// 
                            //ewbDtls.Transdocno = dt.Rows[i]["lrno"].retStr() == "" ? null : dt.Rows[i]["lrno"].retStr();// 
                            //ewbDtls.TransdocDt = dt.Rows[i]["lrdt"].retStr() == "" ? null : dt.Rows[i]["lrdt"].retDateStr();// ;
                            //ewbDtls.Vehno = (dt.Rows[i]["lorryno"].retStr() == "" && dt.Rows[i]["lorryno"].retStr().Length < 4) ? null : dt.Rows[i]["lorryno"].retStr();// ;
                            //ewbDtls.Vehtype = "R";//aq
                            //ewbDtls.TransMode = "1";//aq

                            if (string.IsNullOrEmpty(dt.Rows[i]["lrno"].retStr()))
                            {// Transport mode is mandatory as Vehicle Number/Transport Document Number is given
                                ewbDtls.Transdocno = null;
                                ewbDtls.TransdocDt = null;
                            }
                            else
                            {
                                ewbDtls.Transdocno = dt.Rows[i]["lrno"].retStr();
                                ewbDtls.TransdocDt = dt.Rows[i]["lrdt"].retDateStr();
                            }
                            if (string.IsNullOrEmpty(dt.Rows[i]["lorryno"].retStr()) || dt.Rows[i]["lorryno"].retStr().Length < 4)
                            {
                                ewbDtls.Vehno = null;
                                ewbDtls.Vehtype = null;
                            }
                            else
                            {
                                ewbDtls.Vehno = dt.Rows[i]["lorryno"].retStr();
                                ewbDtls.Vehtype = "R";
                            }
                            if (string.IsNullOrEmpty(ewbDtls.Transdocno) && string.IsNullOrEmpty(ewbDtls.Vehno))
                            {
                                ewbDtls.TransMode = null;
                            }
                            else
                            {
                                ewbDtls.TransMode = "1";
                            }
                        }
                        //  ValDtls
                        valDtls.AssVal = tottxablamt.toRound(2);
                        valDtls.CgstVal = cgstamt.toRound(2);
                        valDtls.SgstVal = sgstamt.toRound(2);
                        valDtls.IgstVal = igstamt.toRound(2);
                        valDtls.CesVal = cessamt.toRound(2);
                        valDtls.StCesVal = 0;
                        valDtls.Discount = 0;
                        valDtls.OthChrg = (dt.Rows[0]["othramt"].retDbl() + dt.Rows[0]["tcsamt"].retDbl()).toRound(2);
                        valDtls.RndOffAmt = roamt.toRound(2);
                        valDtls.TotInvVal = blamt.toRound(2);
                        valDtls.TotInvValFc = 0;

                        adaequareIRN.TranDtls = tranDtls;
                        adaequareIRN.DocDtls = docDtls;
                        adaequareIRN.SellerDtls = SellerDtls;
                        adaequareIRN.BuyerDtls = buyerDtls;
                        if (SellerDtls.Pin != dispDtls.Pin)
                        {
                            adaequareIRN.DispDtls = dispDtls;
                        }
                        if (buyerDtls.Gstin != shipDtls.Gstin || buyerDtls.Pin != shipDtls.Pin)
                        {
                            adaequareIRN.ShipDtls = shipDtls;
                        }
                        adaequareIRN.ItemList = itemLists;
                        adaequareIRN.ValDtls = valDtls;
                        if (VE.GenEinvIRNGrid[gridindex].WAYBILLChecked == true)// && ewbDtls.Transid != ""
                        {
                            if (ewbDtls.Distance == 0)
                            {
                                VE.GenEinvIRNGrid[gridindex].MESSAGE = "Distance need to Add in Sub Ledger"; continue;
                            }
                            else if (ewbDtls.Transid == null && ewbDtls.Transdocno == null && ewbDtls.Vehno == null)
                            {
                                VE.GenEinvIRNGrid[gridindex].MESSAGE = "Transid or Transdocno or Vehno  add into your bill for generate EWB"; continue;
                            }
                            else
                            {
                                adaequareIRN.EwbDtls = ewbDtls;
                            }
                        }
                        string ValidateIRNdetailsmsg = adaequareGSP.ValidateIRNdetails(adaequareIRN);
                        if (ValidateIRNdetailsmsg != "ok") { VE.GenEinvIRNGrid[gridindex].MESSAGE = ValidateIRNdetailsmsg; continue; }
                        string jsonstr = JsonConvert.SerializeObject(adaequareIRN);
                        if (CallForValidation == false)
                        {
                            AdqrRespGenIRN adqrRespGenIRN = adaequareGSP.AdqrGenIRN(jsonstr);
                            string msg = adqrRespGenIRN.message;
                            if (adqrRespGenIRN != null && adqrRespGenIRN.result != null)
                            {
                                sql = "insert into " + CommVar.FinSchema(UNQSNO) + ".T_TXNEINV(CLCD,AUTONO,ACKNO,ACKDT,IRNNO,SIGNQRCODE)"
                                    + " values('" + CommVar.ClientCode(UNQSNO) + "','" + autono + "','" + adqrRespGenIRN.result.AckNo + "',to_date('" + adqrRespGenIRN.result.AckDt + "','yyyy-mm-dd hh24:mi:ss'),'"
                                    + adqrRespGenIRN.result.Irn + "','" + adqrRespGenIRN.result.SignedQRCode.retStr() + "' )  ";
                                masterHelp.SQLNonQuery(sql);
                                SaveSignedInvoice(adqrRespGenIRN.result.SignedInvoice, autono);
                                if (adqrRespGenIRN.result.SignedQRCode.retStr() != "")
                                {
                                    string Savepath = "C:/IPSMART/IRNQrcode/" + adqrRespGenIRN.result.Irn + ".png";
                                    Cn.GenerateQRcode(adqrRespGenIRN.result.SignedQRCode, "", Savepath);
                                }
                                if (adqrRespGenIRN.result.EwbNo.retStr() != "")
                                {
                                    if (Module.MODCD == "S" || Module.MODCD == "I")
                                    {
                                        sql = "update " + CommVar.SaleSchema(UNQSNO) + ".t_txntrans set ewaybillno='" + adqrRespGenIRN.result.EwbNo.retStr() + "' where autono='" + autono + "' ";
                                        masterHelp.SQLNonQuery(sql);
                                    }
                                    sql = "update " + CommVar.FinSchema(UNQSNO) + ".T_TXNEWB set EWAYBILLNO='" + adqrRespGenIRN.result.EwbNo.retStr()
                                        + "',  EWAYBILLDT=to_date('" + adqrRespGenIRN.result.EwbDt.retStr() + "','yyyy-mm-dd hh24:mi:ss'),  EWAYBILLVALID=to_date('"
                                        + adqrRespGenIRN.result.EwbValidTill.retStr() + "','yyyy-mm-dd hh24:mi:ss') where autono='" + autono + "' ";
                                    masterHelp.SQLNonQuery(sql);
                                    VE.GenEinvIRNGrid[gridindex].EWB = adqrRespGenIRN.result.EwbNo.ToString();
                                }
                                if (adqrRespGenIRN.info != null && adqrRespGenIRN.info.Count > 0 && adqrRespGenIRN.info[0].Desc != null)
                                {
                                    foreach (var inf in adqrRespGenIRN.info)
                                    {
                                        msg += " ; " + inf.InfCd + "=>";
                                        foreach (var desc in inf.Desc)
                                        {
                                            msg += " " + desc.ErrorCode + ":" + desc.ErrorMessage;
                                        }
                                    }
                                }
                                VE.GenEinvIRNGrid[gridindex].IRNNO = adqrRespGenIRN.result.Irn.ToString();
                                VE.GenEinvIRNGrid[gridindex].Checked = false;
                            }
                            VE.GenEinvIRNGrid[gridindex].MESSAGE = msg;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                VE.Message = ex.Message;
            }
            return VE;
        }
        public JsonResult ExtractQR(GenEinvIRN VE)
        {
            try
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                var irn = VE.Irn;
                var SIGNQRCODE = VE.QRTEXT;
                if (SIGNQRCODE == null)
                {
                    sql = "select * from " + CommVar.FinSchema(UNQSNO) + ".t_txneinv where irnno='" + irn + "'";
                    var dt = masterHelp.SQLquery(sql);
                    if (dt.Rows.Count > 0)
                    {
                        SIGNQRCODE = dt.Rows[0]["SIGNQRCODE"].ToString();
                    }
                }
                if (SIGNQRCODE == null) { dic.Add("message", "IRN nor found."); goto ret; }
                AdaequareGSP adaequareGSP = new AdaequareGSP();
                //string jsonstr = "{"data\":\"";jsonstr += VE.QRTEXT;jsonstr += "\"}";
                string jsonstr = JsonConvert.SerializeObject(SIGNQRCODE);
                var AdqrRespExtractInvoic = adaequareGSP.AdqrExtractQR(jsonstr);

                if (AdqrRespExtractInvoic.success == true && AdqrRespExtractInvoic.result != null)
                {
                    dic.Add("message", "ok");
                    dic.Add("BuyerGstin", AdqrRespExtractInvoic.result.BuyerGstin);
                    dic.Add("DocDt", AdqrRespExtractInvoic.result.DocDt);
                    dic.Add("DocNo", AdqrRespExtractInvoic.result.DocNo);
                    dic.Add("DocTyp", AdqrRespExtractInvoic.result.DocTyp);
                    dic.Add("Irn", AdqrRespExtractInvoic.result.Irn);
                    dic.Add("IrnDt", AdqrRespExtractInvoic.result.IrnDt);
                    dic.Add("ItemCnt", AdqrRespExtractInvoic.result.ItemCnt.retStr());
                    dic.Add("MainHsnCode", AdqrRespExtractInvoic.result.MainHsnCode);
                    dic.Add("SellerGstin", AdqrRespExtractInvoic.result.SellerGstin);
                    dic.Add("TotInvVal", AdqrRespExtractInvoic.result.TotInvVal.retStr());
                }
                else
                {
                    dic.Add("message", AdqrRespExtractInvoic.message);
                }
                ret:
                ModelState.Clear();
                return Json(dic);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Json(ex.Message + ex.InnerException, JsonRequestBehavior.AllowGet);
            }
        }

        private void SaveSignedInvoice(string SignedInvoice, string autono)
        {
            long length = SignedInvoice.Length;
            long count = length / 4000;
            int index = 0;
            for (int i = 0; i <= count; i++)
            {
                string file = "";
                if (length - index > 4000)
                {
                    file = SignedInvoice.Substring(index, 4000);
                }
                else
                {
                    file = SignedInvoice.Substring(index, SignedInvoice.Length - index);
                }
                sql = " INSERT INTO " + CommVar.FinSchema(UNQSNO) + ".T_TXNEINV_SIGN (EMD_NO, CLCD, AUTONO, SLNO, SIGNINV) VALUES(0,'" + CommVar.ClientCode(UNQSNO) + "','" + autono + "'," + (i + 1) + ",'" + file + "')";
                masterHelp.SQLNonQuery(sql);
                index += 4000;
            }
        }
    }
}