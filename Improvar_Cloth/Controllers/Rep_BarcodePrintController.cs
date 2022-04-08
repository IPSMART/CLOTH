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
using System.Text;

namespace Improvar.Controllers
{//
    public class Rep_BarcodePrintController : Controller
    {
        Connection Cn = new Connection(); MasterHelp masterHelp = new MasterHelp();
        Salesfunc salesfunc = new Salesfunc(); DataTable DTNEW = new DataTable();
        EmailControl EmailControl = new EmailControl();
        SMS SMS = new SMS();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_BarcodePrint
        public ActionResult Rep_BarcodePrint(string autono, string docdt, string barno, string callfrm = "")
        {
            try
            {
                if (docdt.retDateStr() == "") docdt = System.DateTime.Now.retDateStr();
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Barcode Printing";
                    RepBarcodePrint VE = new RepBarcodePrint();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    var Schnm = CommVar.CurSchema(UNQSNO);
                    string reptype = "BARPRN";
                    DataTable repformat = salesfunc.getRepFormat(reptype);

                    if (repformat != null)
                    {
                        VE.DropDown_list1 = (from DataRow dr in repformat.Rows
                                             select new DropDown_list1()
                                             {
                                                 text = dr["text"].ToString(),
                                                 value = dr["value"].ToString()
                                             }).ToList();
                    }
                    else
                    {
                        List<DropDown_list1> drplst = new List<DropDown_list1>();
                        VE.DropDown_list1 = drplst;
                    }
                    var tembarno = ""; DataTable innerDt = new DataTable();
                    if (callfrm == "T_StockAdj")
                    { tembarno = barno; barno = null; }
                    //DataTable ttxndtl = retBarPrn(docdt, autono, barno);
                    DataTable ttxndtl = retBarPrn(docdt, autono, barno, "WP", "RP", callfrm);
                    if (callfrm == "T_StockAdj")
                    { innerDt = ttxndtl.Select("barno in(" + tembarno + ")").CopyToDataTable(); }
                    else {
                        if (ttxndtl.Rows.Count!=0) innerDt = ttxndtl.Select().CopyToDataTable();
                    }
                    if (innerDt.Rows.Count == 0) return Content("No Records..");
                    VE.BarcodePrint = (from DataRow dr in innerDt.Rows
                                       select new BarcodePrint()
                                       {
                                           TAXSLNO = dr["txnslno"].retStr(),
                                           BARNO = dr["BARNO"].retStr(),
                                           ITGRPNM = dr["ITGRPNM"].retStr(),
                                           FABITNM = dr["FABITNM"].retStr(),
                                           STYLENO = dr["itnm"].retStr(),
                                           ITSTYLE = dr["styleno"].retStr(),
                                           NOS = (dr["barnos"].retInt() == 1 || dr["barnos"].retInt() == 0) ? (dr["uomcd"].retStr() == "MTR" ? "1" : dr["qnty"].retStr()) : dr["barnos"].retStr(),//if barno==0==1 then( uom==mtr then nos=1 otherwise nos=qnty)
                                           WPRATE = dr["wprate"].retDbl(),
                                           CPRATE = dr["cprate"].retDbl(),
                                           RPRATE = dr["rprate"].retDbl(),
                                           DESIGN = dr["design"].retStr(),
                                           PDESIGN = dr["pdesign"].retStr(),
                                           COLRNM = dr["colrnm"].retStr(),
                                           SIZENM = dr["sizenm"].retStr(),
                                           ITGRPSHORTNM = dr["shortnm"].retStr(),
                                           GRPNM = dr["grpnm"].retStr(),
                                           ITREM = dr["itrem"].retStr(),
                                           PARTNM = dr["partnm"].retStr(),
                                           SIZECD = dr["sizecd"].retStr(),
                                           DOCNO = dr["docno"].retStr(),
                                           DOCDT = dr["docdt"].retStr(),
                                           PREFNO = dr["blno"].retStr(),
                                           PREFDT = dr["docdt"].retStr(),
                                           UOMCD = dr["uomcd"].retStr(),
                                           QNTY = dr["qnty"].retStr(),
                                           DOCPRFX = dr["docprfx"].retStr(),
                                           DOCONLYNO = dr["doconlyno"].retStr(),
                                           Checked = dr["barnos"].retDbl() == 0 ? false : true,
                                           //}).Distinct().OrderBy(s => s.TAXSLNO).ToList();
                                       }).ToList();
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

        public DataTable retBarPrn(string docdt, string autono = "", string barno = "", string wppricecd = "WP", string rppricecd = "RP", string callfrm = "")
        {
            string UNQSNO = CommVar.getQueryStringUNQSNO();
            string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
            string sql = "";
            bool tblmst = false;
            if (barno.retStr() != "" && barno.IndexOf("'") < 0) barno = "'" + barno + "'";
            if (barno.retStr() != "") tblmst = true;
            bool tphystk = false;
            if (callfrm.retStr() == "PHYSTK") tphystk = true;
            sql = "";

            sql += "select a.autono, x.barno, a.txnslno, a.qnty, a.barnos, b.uomcd, nvl(b.itnm,e.itnm) itnm, b.itgrpcd, f.grpnm, f.itgrpnm,f.shortnm ,j.sizenm , " + Environment.NewLine;
            sql += "x.pdesign, nvl(x.ourdesign,b.styleno) design, " + Environment.NewLine;
            sql += "nvl(m.cprate,x.rate) cprate, nvl(m.wprate,0) wprate, nvl(m.rprate,0) rprate, " + Environment.NewLine;
            sql += "x.itrem, x.partcd, h.partnm, x.sizecd, x.colrcd, nvl(x.shade,g.colrnm) colrnm, " + Environment.NewLine;
            sql += "nvl(c.prefno,d.docno) blno, d.docdt,d.docno,d.doconlyno, c.slcd, nvl(i.shortnm,i.slnm) slnm, k.docprfx, " + Environment.NewLine;
            sql += "a.fabitcd, e.itnm fabitnm,b.styleno from " + Environment.NewLine;

            sql += "( select a.autono, to_number(" + (tphystk == true ? "a.slno" : tblmst == true ? "0" : "a.txnslno") + ") txnslno, nvl(b.fabitcd,c.fabitcd) fabitcd, a.barno, " + Environment.NewLine;
            //sql += "a.qnty, a.rate, decode(nvl(a.nos,0),0,a.qnty,a.nos) barnos ";
            sql += "a.qnty, a.rate, decode(nvl(a.nos,0),0,1,a.nos) barnos " + Environment.NewLine;
            sql += "from " + scm + (tphystk == true ? ".t_phystk" : tblmst == false ? ".t_batchdtl" : ".t_batchmst") + " a, " + scm + ".t_batchmst b, " + scm + ".m_sitem c, " + scm + ".t_cntrl_hdr d " + Environment.NewLine;
            sql += "where a.autono=d.autono(+) and a.barno=b.barno(+) and b.itcd=c.itcd(+) and " + Environment.NewLine;
            if (autono.retStr() != "") sql += "a.autono in ('" + autono + "') and " + Environment.NewLine;
            //if (barno.retStr() != "") sql += "a.barno in ('" + barno + "') and " + Environment.NewLine;
            if (barno.retStr() != "") sql += "a.barno in (" + barno + ") and " + Environment.NewLine;
            sql += "d.compcd='" + COM + "' and d.loccd='" + LOC + "' and nvl(d.cancel,'N')='N' ) a, " + Environment.NewLine;
            sql += "(select a.barno, nvl(m.rate, 0) cprate, nvl(n.rate, 0) wprate, nvl(o.rate, 0) rprate from " + Environment.NewLine;
            sql += "" + scm + ".t_batchmst a, " + Environment.NewLine;
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
                        prccd = rppricecd; sqlals = "o "; break;
                }
                //sql += "(select a.barno, a.itcd, a.colrcd, a.sizecd, a.prccd, a.effdt, a.rate from ";
                sql += "(select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate from " + Environment.NewLine;
                sql += "(select a.barno, a.prccd, a.effdt, " + Environment.NewLine;
                sql += "row_number() over (partition by a.barno, a.prccd order by a.effdt desc) as rn " + Environment.NewLine;
                sql += "from " + scm + ".T_BATCHMST_PRICE a where nvl(a.rate,0) <> 0 and a.effdt <= to_date('" + docdt + "','dd/mm/yyyy') " + Environment.NewLine;
                sql += ") a, " + scm + ".T_BATCHMST_PRICE b, " + scm + ".T_BATCHmst c " + Environment.NewLine;
                sql += "where a.barno=b.barno(+) and a.prccd=b.prccd(+) and a.effdt=b.effdt(+) and a.barno=c.barno(+) and a.rn=1 and a.prccd='" + prccd + "' " + Environment.NewLine;
                //sql += "union ";
                //sql += "select a.barno, c.itcd, c.colrcd, c.sizecd, a.prccd, a.effdt, b.rate from ";
                //sql += "(select a.barno, a.prccd, a.effdt, ";
                //sql += "row_number() over (partition by a.barno, a.prccd order by a.effdt desc) as rn ";
                //sql += "from " + scm + ".t_batchmst_price a where nvl(a.rate,0) <> 0 and a.effdt <= to_date('" + docdt + "','dd/mm/yyyy') ) ";
                //sql += "a, " + scm + ".t_batchmst_price b, " + scm + ".t_batchmst c,  " + scm + ".T_BATCHmst d ";
                //sql += "where a.barno=b.barno(+) and a.prccd=b.prccd(+) and a.effdt=b.effdt(+) and a.rn=1 and a.prccd='" + prccd + "' and ";
                //sql += "a.barno=c.barno(+) and a.barno=d.barno(+) and d.barno is null ";
                //sql += ") a where prccd='" + prccd + "' ";
                sql += ") " + sqlals;
                if (x != 2) sql += ", ";
            }
            sql += "where a.barno = m.barno(+) and a.barno = n.barno(+) and a.barno = o.barno(+) ) m, " + Environment.NewLine;
            sql += "" + scm + ".t_batchmst x, " + scm + ".m_sitem b, " + scm + ".t_txn c, " + scm + ".t_cntrl_hdr d, " + Environment.NewLine;
            sql += "" + scm + ".m_sitem e, " + scm + ".m_group f, " + scm + ".m_color g, " + scm + ".m_parts h, " + Environment.NewLine;
            sql += "" + scmf + ".m_subleg i ," + scm + ".m_size j, " + scm + ".m_doctype k " + Environment.NewLine;
            //sql += "where x.autono=c.autono(+) and x.autono=d.autono(+) and x.barno=a.barno(+) and " + Environment.NewLine;
            if (tphystk == true)
            {
                sql += "where a.barno=x.barno(+) and x.autono=c.autono(+) and c.autono=d.autono(+)  and " + Environment.NewLine;
            }
            else
            {
                sql += "where a.autono=c.autono(+) and a.autono=d.autono(+) and a.barno=x.barno(+) and " + Environment.NewLine;
            }
            sql += "x.itcd=b.itcd(+) and x.fabitcd=e.itcd(+) and b.itgrpcd=f.itgrpcd(+) and d.doccd=k.doccd(+) and " + Environment.NewLine;
            sql += "a.barno=m.barno(+) and " + Environment.NewLine;
            sql += "x.colrcd=g.colrcd(+) and x.partcd=h.partcd(+) and c.slcd=i.slcd(+) and x.sizecd=j.sizecd(+) " + Environment.NewLine;
            sql += " order by a.txnslno" + Environment.NewLine;
            DataTable tbl = masterHelp.SQLquery(sql);

            return tbl;

        }


        [HttpPost]
        public ActionResult Rep_BarcodePrint(RepBarcodePrint VE, string btnSubmit)
        {
            try
            {
                string sql = "select PRICEINCODE from  " + CommVar.CurSchema(UNQSNO) + ".m_syscnfg where rownum=1 order by effdt desc";
                var dtsyscnfg = masterHelp.SQLquery(sql); string PRICEINCODE = "";
                if (dtsyscnfg != null && dtsyscnfg.Rows.Count > 0)
                {
                    PRICEINCODE = dtsyscnfg.Rows[0]["PRICEINCODE"].retStr();
                }
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

                IR.Columns.Add("txnslno", typeof(string));
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
                IR.Columns.Add("docprfx", typeof(string));

                IR.Columns.Add("docdt", typeof(string));

                IR.Columns.Add("blno", typeof(string));

                IR.Columns.Add("prefdt", typeof(string));

                IR.Columns.Add("docdt_code", typeof(string));
                IR.Columns.Add("grpnm", typeof(string));
                IR.Columns.Add("itrem", typeof(string));
                IR.Columns.Add("partnm", typeof(string));
                IR.Columns.Add("sizecd", typeof(string));
                IR.Columns.Add("recdt_code", typeof(string));
                IR.Columns.Add("compnm", typeof(string));
                IR.Columns.Add("compcd", typeof(string));
                IR.Columns.Add("uom", typeof(string));
                IR.Columns.Add("qnty", typeof(string));
                IR.Columns.Add("fulldocdt", typeof(string));
                IR.Columns.Add("doconlyno", typeof(string));

                string FileName = "";
                var ischecked = VE.BarcodePrint.Where(c => c.Checked == true).ToList();
                if (ischecked.Count == 0) return Content("<h1>Please select/checked a row in the grid. <h1>");
                VE.BarcodePrint = VE.BarcodePrint.Where(a => a.Checked == true).ToList();
                for (int i = 0; i < VE.BarcodePrint.Count; i++)
                {
                    if (VE.BarcodePrint[i].Checked == true)
                    {
                        string barno = VE.BarcodePrint[i].BARNO.retStr();
                        byte[] brcodeImage = (byte[])Cn.GenerateBarcode(barno, "byte", false);
                        FileName = VE.BarcodePrint[i].DOCNO.retStr();
                        for (int j = 0; j < VE.BarcodePrint[i].NOS.retDbl(); j++)
                        {
                            DataRow dr = IR.NewRow();
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
                            dr["txnslno"] = "(" + VE.BarcodePrint[i].TAXSLNO.retStr() + ")";

                            var wpp = VE.BarcodePrint[i].WPRATE.retDbl();
                            dr["wprate"] = wpp.retInt().retStr();
                            dr["wprate_paisa"] = wpp.ToString("0.00");
                            var cpp = VE.BarcodePrint[i].CPRATE.retDbl();
                            dr["cprate"] = cpp.retInt().retStr();
                            dr["cprate_paisa"] = cpp.ToString("0.00");
                            var rpp = VE.BarcodePrint[i].RPRATE.retDbl();
                            dr["rprate"] = rpp.retInt().retStr();
                            dr["rprate_paisa"] = rpp.ToString("0.00");

                            dr["cprate_code"] = RateEncode(VE.BarcodePrint[i].CPRATE.retDbl().retInt(), PRICEINCODE);
                            dr["wprate_code"] = RateEncode(VE.BarcodePrint[i].WPRATE.retDbl().retInt(), PRICEINCODE);
                            dr["rprate_code"] = RateEncode(VE.BarcodePrint[i].RPRATE.retDbl().retInt(), PRICEINCODE);
                            var recdtrate = VE.BarcodePrint[i].DOCDT.retDateStr().Replace("/", "");
                            dr["recdt_code"] = RateEncode(recdtrate.retInt(), PRICEINCODE); ;
                            dr["cost"] = VE.BarcodePrint[i].CPRATE.retDbl().retStr();
                            dr["costcode"] = RateEncode(VE.BarcodePrint[i].CPRATE.retDbl().retInt(), PRICEINCODE);
                            dr["docno"] = VE.BarcodePrint[i].DOCNO.retStr();
                            dr["doconlyno"] = VE.BarcodePrint[i].DOCONLYNO.retStr();
                            dr["docprfx"] = VE.BarcodePrint[i].DOCPRFX.retStr();
                            dr["docdt"] = VE.BarcodePrint[i].DOCDT.retDateStr().Replace("/", "");
                            dr["blno"] = VE.BarcodePrint[i].PREFNO.retStr();
                            dr["prefdt"] = VE.BarcodePrint[i].DOCDT.retDateStr().Replace("/", "");
                            dr["docdt_code"] = VE.BarcodePrint[i].DOCDT.retDateStr().Replace("/", "");
                            dr["blno"] = VE.BarcodePrint[i].PREFNO.retStr();
                            dr["itrem"] = VE.BarcodePrint[i].ITREM.retStr();
                            dr["partnm"] = VE.BarcodePrint[i].PARTNM.retStr();
                            dr["sizecd"] = VE.BarcodePrint[i].SIZECD.retStr();
                            dr["compnm"] = CommVar.CompName(UNQSNO);
                            dr["compcd"] = CommVar.Compcd(UNQSNO);
                            dr["qnty"] = VE.BarcodePrint[i].QNTY.retStr();
                            dr["uom"] = VE.BarcodePrint[i].UOMCD.retStr();
                            dr["fulldocdt"] = VE.BarcodePrint[i].DOCDT.retDateStr();
                            IR.Rows.Add(dr);
                        }
                    }
                }
                string rptfile = "PrintBarcode.rpt";
                if (VE.Reptype != null) rptfile = VE.Reptype;
                string rptname = "~/Report/" + rptfile;

                ReportDocument reportdocument = new ReportDocument();
                reportdocument.Load(Server.MapPath(rptname));
                DSPrintBarcode DSP = new DSPrintBarcode();
                DSP.Merge(IR);
                reportdocument.SetDataSource(DSP);
                Response.Buffer = false;
                Response.Clear();
                Response.ClearContent();
                Response.ClearHeaders();
                Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
                if (btnSubmit == "Download Barcode")
                {
                    Response.ContentType = "application/pdf";
                    Response.Charset = string.Empty;
                    Response.Cache.SetCacheability(System.Web.HttpCacheability.Public);
                    Response.AddHeader("Content-Disposition", string.Format("attachment;filename=" + FileName + ".pdf", "Collection"));
                    byte[] bytes;
                    using (var ms = new MemoryStream()) { stream.CopyTo(ms); bytes = ms.ToArray(); }
                    Response.OutputStream.Write(bytes, 0, bytes.Length);
                    Response.OutputStream.Flush();
                    Response.OutputStream.Close();
                    Response.End();
                    return Content("Done");
                }
                else
                {
                    return new FileStreamResult(stream, "application/pdf");
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.StackTrace);
            }
            return Content("Done");
        }
        public string RateEncode(int rate, string PRICEINCODE)
        {
            PRICEINCODE = PRICEINCODE.ToUpper();
            string str = ""; string rptchar = "";
            string[,] arr = new string[11, 2];
            //G A N A S H R //11th charecter can repeat eg 1122=ARNR
            //0 1 2 3 4 5 H
            if (PRICEINCODE != "")
            {
                int i = 0;
                foreach (char c in PRICEINCODE)
                {
                    arr[i, 0] = c.ToString();
                    arr[i, 1] = i.ToString(); i++;
                }
                var strate = rate.ToString(); string lastchar = "";
                if (PRICEINCODE.Length == 11) rptchar = arr[10, 0];
                foreach (char c in strate)
                {
                    for (int k = 0; k < arr.GetLength(0) - rptchar.Length; k++)
                    {
                        if (c.ToString() == arr[k, 1])
                        {
                            if (lastchar == arr[k, 0] && rptchar != "")
                            {
                                str += rptchar;
                                lastchar = "";
                            }
                            else
                            {
                                str += arr[k, 0];
                                lastchar = arr[k, 0];
                            }
                        }
                    }
                }
                return str;
            }
            else
            {
                return rate.ToString();
            }
        }

        public ActionResult PreviewBarcodeTxtFmt(string text)
        {
            ViewBag.BarText = "hook\nhook\nhook\nhook\nhook\nhook\nhook\n jkkkjhh<br> mithun";
            //return View();
            return Content("Hook\n mithun", "text/plain", Encoding.UTF8);
        }


    }
}