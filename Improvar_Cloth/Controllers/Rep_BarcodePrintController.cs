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
{//
    public class Rep_BarcodePrintController : Controller
    {
        Connection Cn = new Connection(); MasterHelp masterHelp = new MasterHelp();
        Salesfunc salesfunc = new Salesfunc(); DataTable DTNEW = new DataTable();
        EmailControl EmailControl = new EmailControl();
        T_TXN TXN; T_TXNTRANS TXNTRN; T_TXNOTH TXNOTH; T_CNTRL_HDR TCH; T_CNTRL_HDR_REM SLR; T_TXN_LINKNO TTXNLINKNO;
        SMS SMS = new SMS();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_BarcodePrint
        public ActionResult Rep_BarcodePrint(string autono,string docdt, string barno)
        {
            try
            {if (docdt.retDateStr() == "") docdt = System.DateTime.Now.retDateStr();
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Barcode Printing";
                    RepBarcodePrint VE = new RepBarcodePrint(); ;
                    //if (TempData["printparameter"] == null)
                    //{
                    //    VE = new RepBarcodePrint();
                    //}
                    //else
                    //{
                    //    VE = (RepBarcodePrint)TempData["printparameter"];
                    //}
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    var Schnm = CommVar.CurSchema(UNQSNO);
                    ////GenerateBarcode();
                    ////barcodeTest();
                    VE.DropDown_list1 = (from i in DB.M_REPFORMAT
                                         select new DropDown_list1()
                                         { value = i.REPTYPE, text = i.REPTYPE }).Distinct().OrderBy(s => s.text).ToList();
                    //var sql = "select distinct '123456789' BARNO,a.SLNO,b.itnm FABITNM ,b.STYLENO,c.ITGRPNM  from " + Schnm + ".t_txndtl a," + Schnm + ".M_SITEM b," + Schnm + ".M_GROUP c where a.itcd=b.itcd(+) and b.itgrpcd=c.itgrpcd(+)";

                    //DataTable ttxndtl = masterHelp.SQLquery(sql);
                    DataTable ttxndtl = retBarPrn(docdt, autono, barno);
                    VE.BarcodePrint = (from DataRow dr in ttxndtl.Rows
                                       select new BarcodePrint()
                                       {
                                           TAXSLNO = dr["txnslno"].retStr(),
                                           BARNO = dr["BARNO"].retStr(),
                                           ITGRPNM = dr["ITGRPNM"].retStr(),
                                           FABITNM = dr["FABITNM"].retStr(),
                                           STYLENO = dr["itnm"].retStr(),
                                           NOS = dr["barnos"].retStr(),
                                           WPRATE= dr["wprate"].retStr(),
                                           CPRATE = dr["cprate"].retStr(),
                                           RPRATE = dr["rprate"].retStr(),
                                           DESIGN = dr["design"].retStr(),
                                           PDESIGN = dr["pdesign"].retStr(),
                                           COLRNM = dr["colrnm"].retStr(),
                                           SIZENM = dr["sizenm"].retStr(),
                                           ITGRPSHORTNM= dr["shortnm"].retStr(),
                                           GRPNM = dr["grpnm"].retStr(),
                                           ITREM = dr["itrem"].retStr(),
                                           
                                           //WPPRICE = dr["wpprice"].retStr(),
                                           //WPPRICECODE = dr["wppricecode"].retStr(),
                                           //RPPRICE = dr["rpprice"].retStr(),
                                           //RPPRICECODE = dr["rppricecode"].retStr(),
                                           //COST = dr["cost"].retStr(),
                                           //COSTCODE = dr["costcode"].retStr(),
                                           DOCNO = dr["docno"].retStr(),
                                           DOCDT = dr["docdt"].retStr(),
                                           PREFNO = dr["blno"].retStr(),
                                           PREFDT = dr["docdt"].retStr(),
                                           //DOCDTCODE = dr["docdtcode"].retStr()

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

        public DataTable retBarPrn(string docdt, string autono = "", string barno = "", string wppricecd = "WP", string rppricecd = "RP")
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            string sql = "";
            bool tblmst = false;
            if (barno != "") tblmst = true;
            sql = "";

            sql += "select a.autono, x.barno, x.txnslno, x.qnty, x.barnos, b.uomcd, nvl(b.itnm,e.itnm) itnm, b.itgrpcd, f.grpnm, f.itgrpnm,f.shortnm ,j.sizenm , ";
            sql += "a.pdesign, nvl(a.ourdesign,b.styleno) design, ";
            sql += "nvl(m.rate,x.rate) cprate, nvl(n.rate,0) wprate, nvl(o.rate,0) rprate, ";
            sql += "a.itrem, a.partcd, h.partnm, a.sizecd, a.colrcd, nvl(a.shade,g.colrnm) colrnm, ";
            sql += "nvl(c.prefno,d.docno) blno, d.docdt,d.docno,d.doconlyno, c.slcd, nvl(i.shortnm,i.slnm) slnm, ";
            sql += "a.fabitcd, e.itnm fabitnm from ";

            sql += "( select a.autono, to_number(" + (tblmst == true ? "0" : "a.txnslno") + ") txnslno, nvl(b.fabitcd,c.fabitcd) fabitcd, a.barno, ";
            sql += "a.qnty, a.rate, decode(nvl(a.nos,0),0,a.qnty,a.nos) barnos ";
            sql += "from " + scm + (tblmst == false ? ".t_batchdtl" : ".t_batchmst") + " a, " + scm + ".t_batchmst b, " + scm + ".m_sitem c, " + scm + ".t_cntrl_hdr d ";
            sql += "where a.autono=d.autono(+) and a.barno=b.barno(+) and b.itcd=c.itcd(+) and ";
            if (autono.retStr() != "") sql += "a.autono in ('" + autono + "') and ";
            if (barno.retStr() != "") sql += "a.barno in ('" + barno + "') and ";
            sql += "d.compcd='" + COM + "' and d.loccd='" + LOC + "' and nvl(d.cancel,'N')='N' ) x, ";

            for (int x = 0; x <= 2; x++)
            {
                string prccd = "", sqlals = "";
                switch (x)
                {
                    case 0:
                        prccd = "CP"; sqlals = "m"; break;
                    case 1:
                        prccd = wppricecd; sqlals = "n"; break;
                    case 2:
                        prccd = rppricecd; sqlals = "o"; break;
                }
                sql += "(select a.barno, a.itcd, a.colrcd, a.sizecd, a.prccd, a.effdt, a.rate from ";
                sql += "(select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate from ";
                sql += "(select a.barno, a.prccd, a.effdt, ";
                sql += "row_number() over (partition by a.barno, a.prccd order by a.effdt desc) as rn ";
                sql += "from " + scm + ".m_itemplistdtl a where nvl(a.rate,0) <> 0 and a.effdt <= to_date('" + docdt + "','dd/mm/yyyy') ";
                sql += ") a, " + scm + ".m_itemplistdtl b, " + scm + ".m_sitem_barcode c ";
                sql += "where a.barno=b.barno(+) and a.prccd=b.prccd(+) and a.effdt=b.effdt(+) and a.barno=c.barno(+) and a.rn=1 and a.prccd='" + prccd + "' ";
                sql += "union ";
                sql += "select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate from ";
                sql += "(select a.barno, a.prccd, a.effdt, ";
                sql += "row_number() over (partition by a.barno, a.prccd order by a.effdt desc) as rn ";
                sql += "from " + scm + ".t_batchmst_price a where nvl(a.rate,0) <> 0 and a.effdt <= to_date('" + docdt + "','dd/mm/yyyy') ) ";
                sql += "a, " + scm + ".t_batchmst_price b, " + scm + ".t_batchmst c,  " + scm + ".m_sitem_barcode d ";
                sql += "where a.barno=b.barno(+) and a.prccd=b.prccd(+) and a.effdt=b.effdt(+) and a.rn=1 and a.prccd='" + prccd + "' and ";
                sql += "a.barno=c.barno(+) and a.barno=d.barno(+) and d.barno is null ";
                sql += ") a where prccd='" + prccd + "') " + sqlals + ", ";
            }

            sql += "" + scm + ".t_batchmst a, " + scm + ".m_sitem b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, ";
            sql += "" + scm + ".m_sitem e, " + scm + ".m_group f, " + scm + ".m_color g, " + scm + ".m_parts h, ";
            sql += "" + scmf + ".m_subleg i ," + scm + ".m_size j ";
            sql += "where x.autono=c.autono(+) and x.autono=d.autono(+) and x.barno=a.barno(+) and ";
            sql += "a.itcd=b.itcd(+) and a.fabitcd=e.itcd(+) and b.itgrpcd=f.itgrpcd(+) and ";
            sql += "x.barno=m.barno(+) and x.barno=n.barno(+) and x.barno=o.barno(+) and ";
            sql += "a.colrcd=g.colrcd(+) and a.partcd=h.partcd(+) and c.slcd=i.slcd(+) and a.sizecd=j.sizecd(+) ";
            DataTable tbl = masterHelp.SQLquery(sql);

            return tbl;

        }


        [HttpPost]
        public ActionResult Rep_BarcodePrint(RepBarcodePrint VE)
        {
            DataTable IR = new DataTable("DataTable1");
            IR.Columns.Add("brcodeImage", typeof(byte[]));
            IR.Columns.Add("barno", typeof(string));
            IR.Columns.Add("compinit", typeof(string));
            IR.Columns.Add("itgrpnm", typeof(string));
            IR.Columns.Add("itgrpshortnm", typeof(string));
            IR.Columns.Add("itnm", typeof(string));
            IR.Columns.Add("design", typeof(string));

            IR.Columns.Add("pdesign", typeof(string));

            IR.Columns.Add("mtr", typeof(string));

            IR.Columns.Add("colrnm", typeof(string));

            IR.Columns.Add("sizenm", typeof(string));

            IR.Columns.Add("txslno", typeof(string));
            IR.Columns.Add("wprate", typeof(string));
            IR.Columns.Add("wprate_paisa", typeof(string));

            IR.Columns.Add("wprate_code", typeof(string));
            IR.Columns.Add("cprate", typeof(string));

            IR.Columns.Add("cprate_paisa", typeof(string));

            IR.Columns.Add("cprate_code", typeof(string));
            IR.Columns.Add("rprate", typeof(string));

            IR.Columns.Add("rprate_paisa", typeof(string));

            IR.Columns.Add("rprate_code", typeof(string));

            IR.Columns.Add("cost", typeof(string));

            IR.Columns.Add("costcode", typeof(string));

            IR.Columns.Add("docno", typeof(string));

            IR.Columns.Add("docdt", typeof(string));

            IR.Columns.Add("blno", typeof(string));

            IR.Columns.Add("prefdt", typeof(string));

            IR.Columns.Add("docdtcode", typeof(string));
            IR.Columns.Add("grpnm", typeof(string));
            IR.Columns.Add("itrem", typeof(string));

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
                        dr["grpnm"] = VE.BarcodePrint[i].GRPNM.retStr();
                        dr["itgrpnm"] = VE.BarcodePrint[i].ITGRPNM.retStr();
                        dr["itgrpshortnm"] = VE.BarcodePrint[i].ITGRPSHORTNM.retStr();
                        dr["itnm"] = VE.BarcodePrint[i].STYLENO.retStr();
                        dr["design"] = VE.BarcodePrint[i].DESIGN.retStr();
                        dr["pdesign"] = VE.BarcodePrint[i].PDESIGN.retStr();
                        dr["mtr"] = VE.BarcodePrint[i].MTR.retStr();
                        dr["colrnm"] = VE.BarcodePrint[i].COLRNM.retStr();
                        dr["sizenm"] = VE.BarcodePrint[i].SIZENM.retStr();
                        dr["txslno"] = VE.BarcodePrint[i].TAXSLNO.retStr();
                        dr["wprate"] = VE.BarcodePrint[i].WPRATE.retStr();
                        dr["wprate_paisa"] = VE.BarcodePrint[i].WPRATE.retStr().Replace(".","");
                        dr["cprate"] = VE.BarcodePrint[i].CPRATE.retStr();
                        dr["cprate_paisa"] = VE.BarcodePrint[i].CPRATE.retStr().Replace(".", "");
                        dr["wprate_code"] = "";
                        dr["rprate"] = VE.BarcodePrint[i].RPRATE.retStr();
                        dr["rprate_paisa"] = VE.BarcodePrint[i].RPRATE.retStr().Replace(".", "");
                        dr["rprate_code"] = "";
                        dr["cost"] = "";
                        dr["costcode"] = "";
                        dr["docno"] = VE.BarcodePrint[i].DOCNO.retStr();
                        dr["docdt"] = VE.BarcodePrint[i].DOCDT.retDateStr().Replace("/","");
                        dr["blno"] = VE.BarcodePrint[i].PREFNO.retStr();
                        dr["prefdt"] = VE.BarcodePrint[i].DOCDT.retDateStr().Replace("/", "");
                        dr["docdtcode"] = "";
                        dr["blno"] = VE.BarcodePrint[i].PREFNO.retStr();
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