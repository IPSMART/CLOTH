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
                sql += "a.sizecd, a.rate, a.scmdiscamt, a.discamt, a.qnty,A.DELVDT,a.ITREM,a.PDESIGN,c.itgrpcd, d.itgrpnm,c.fabitcd, ";
                sql += "e.itnm fabitnm,a.colrcd,a.partcd,f.colrnm,g.sizenm,h.partnm,a.rate from ";
                sql += scm + ".T_SORDDTL a, " + scm + ".T_CNTRL_HDR b, ";
                sql += scm + ".m_sitem c, " + scm + ".m_group d, " + scm + ".m_sitem e, " + scm + ".m_color f, " + scm + ".m_size g, " + scm + ".m_parts h, " + scm + ".T_SORD i  ";
                sql += "where a.autono = b.autono and a.autono = i.autono and  a.itcd = c.itcd(+) and c.itgrpcd=d.itgrpcd and c.fabitcd=e.itcd(+) ";
                sql += "and a.colrcd=f.colrcd(+) and a.sizecd=g.sizecd(+) and a.partcd=h.partcd(+)  ";
                sql += "nvl(b.cancel,'N')='N' and b.compcd='" + COM + "' and b.loccd='" + LOC + "' and ";
                if (slcd != null) sql += "a.slcd in (" + slcd + ") and ";
                if (fdt != null) sql += "b.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and ";
                if (tdt != null) sql += "b.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and ";
                if (fdocno != null) sql += "b.doconlyno >= '" + fdocno + "' and ";
                if (fdocno != null) sql += "b.doconlyno <= '" + tdocno + "' and ";
                sql += "d.doccd = '" + doccd + "' ) a, ";
                sql += "order by styleno ";




                //sql += "select a.autono, a.docno, a.docdt, y.slcd, y.agslcd, y.trslcd, y.crslcd, y.slmslcd, y.prccd, y.prceffdt, ";
                //sql += "y.discrtcd, y.discrteffdt, y.docth, z.scmnm, k.prcnm, y.splnote, l.slnm cournm, ";
                //sql += "nvl(e.fullname,e.slnm) slnm, e.district, e.gstno, e.panno, f.slnm agslnm, g.slnm trslnm, h.slnm slmslnm, i.slnm crslnm, ";
                //sql += "e.regemailid, e.add1, e.add2, e.add3, e.add4, e.add5, e.add6, e.add7, d.usr_id, to_char(d.usr_entdt,'dd/mm/yyyy') usr_entdt, ";
                //sql += "y.docth1, y.docth2, y.docth3, y.paytrmcd, j.paytrmnm, y.delvins, y.duedays, y.cod, y.prefno, y.prefdt from ";

                //sql += "(select a.autono, d.docno, d.docdt ";
                //sql += "from " + scm + ".t_sord a, " + scm + ".t_sord_scheme b, " + scm + ".t_cntrl_hdr d ";
                //sql += "where a.autono=b.autono(+) and a.autono=d.autono and ";
                //sql += "nvl(d.cancel,'N')='N' and d.compcd='" + COM + "' and d.loccd='" + LOC + "' and ";
                //if (slcd != null) sql += "a.slcd in (" + slcd + ") and ";
                //if (fdt != null) sql += "d.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and ";
                //if (tdt != null) sql += "d.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and ";
                //if (fdocno != null) sql += "d.doconlyno >= '" + fdocno + "' and ";
                //if (fdocno != null) sql += "d.doconlyno <= '" + tdocno + "' and ";
                //sql += "d.doccd = '" + doccd + "' ) a, ";

                //sql += "( select a.autono, listagg(e.scmnm,',') within group (order by b.autono, b.scmcd) scmnm ";
                //sql += "from " + scm + ".t_sord a, " + scm + ".t_sord_scheme b, " + scm + ".t_cntrl_hdr d, " + scm + ".m_scheme_hdr e ";
                //sql += "where a.autono=b.autono and b.scmcd=e.scmcd(+) and ";
                //sql += "nvl(d.cancel,'N')='N' and d.compcd='" + COM + "' and d.loccd='" + LOC + "' and ";
                //if (slcd != null) sql += "a.slcd in (" + slcd + ") and ";
                //if (fdt != null) sql += "d.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and ";
                //if (tdt != null) sql += "d.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and ";
                //if (fdocno != null) sql += "d.doconlyno >= '" + fdocno + "' and ";
                //if (fdocno != null) sql += "d.doconlyno <= '" + tdocno + "' and ";
                //sql += "a.autono=d.autono ";
                //sql += "group by a.autono ) z, ";

                //sql += scm + ".t_sord y, " + scm + ".t_cntrl_hdr d, ";
                //sql += scmf + ".m_subleg e, " + scmf + ".m_subleg f, " + scmf + ".m_subleg g, " + scmf + ".m_subleg h, " + scmf + ".m_subleg i, ";
                //sql += "" + scm + ".m_paytrms j, " + scmf + ".m_prclst k, " + scmf + ".m_subleg l ";
                //sql += "where a.autono=z.autono(+) and a.autono=d.autono and a.autono=y.autono(+) and y.paytrmcd=j.paytrmcd(+) and ";
                //sql += "y.slcd=e.slcd(+) and y.agslcd=f.slcd(+) and y.trslcd=g.slcd(+) and y.slmslcd=h.slcd(+) and y.crslcd=i.slcd(+) and ";
                //sql += "y.prccd=k.prccd(+) and y.crslcd=l.slcd(+) ";

                rstbl = Master_Help.SQLquery(sql);

                string AUTO_NO = string.Join(",", (from DataRow dr in rstbl.Rows select "'" + dr["autono"].ToString() + "'").Distinct());
                bool blOnlyBal = VE.Checkbox2;

                DataTable rsPendOrd = Salesfunc.GetPendOrder(slcd, "", AUTO_NO, "", "", "SB", "", "", true, "", "", "", "");
                rsPendOrd.Columns.Add("STOCKQNTY", typeof(decimal));
                if (rstbl.Rows.Count == 0)
                {
                    return RedirectToAction("NorsPendOrds", "RPTViewer", new { errmsg = "No Records Found !!" });
                }
                //sql = "";
                //sql += "select  nvl(discrate,0) discrate,  nvl(scmdiscrate,0) scmdiscrate,  nvl( tddiscrate,0) tddiscrate ";
                //sql += " from " + scm + ".t_sorddtl where autono = "+ AUTO_NO + " group by discrate,scmdiscrate,tddiscrate ";
                //DataTable DTdiscsorddtl = Master_Help.SQLquery(sql);

                #region 'Crystal Format
                if (VE.Checkbox4 == false)
                {
                    DataTable IR = new DataTable();

                    IR.Columns.Add("docno", typeof(string), "");
                    IR.Columns.Add("docdt", typeof(string), "");
                    IR.Columns.Add("slnm", typeof(string), "");
                    IR.Columns.Add("slcd", typeof(string), "");
                    IR.Columns.Add("trslnm", typeof(string), "");
                    IR.Columns.Add("trslcd", typeof(string), "");
                    IR.Columns.Add("cournm", typeof(string), "");
                    IR.Columns.Add("destn", typeof(string), "");
                    IR.Columns.Add("agslnm", typeof(string), "");
                    IR.Columns.Add("agslcd", typeof(string), "");
                    IR.Columns.Add("slmslnm", typeof(string), "");
                    IR.Columns.Add("slmslcd", typeof(string), "");
                    IR.Columns.Add("prcnm", typeof(string), "");
                    IR.Columns.Add("rem", typeof(string), "");
                    IR.Columns.Add("splnote", typeof(string), "");
                    IR.Columns.Add("gstno", typeof(string), "");
                    IR.Columns.Add("docth1", typeof(string), "");
                    IR.Columns.Add("docth2", typeof(string), "");
                    IR.Columns.Add("docth3", typeof(string), "");
                    IR.Columns.Add("scmnm", typeof(string), "");
                    IR.Columns.Add("totbox", typeof(string), "");
                    IR.Columns.Add("toset", typeof(string), "");
                    IR.Columns.Add("ordamt", typeof(double), "");
                    IR.Columns.Add("delvtypedsc", typeof(string), "");
                    //extra
                    IR.Columns.Add("rateprint", typeof(string), "");
                    IR.Columns.Add("crslcd", typeof(string), "");
                    IR.Columns.Add("prccd", typeof(string), "");
                    IR.Columns.Add("prceffdt", typeof(string), "");
                    IR.Columns.Add("discrtcd", typeof(string), "");
                    IR.Columns.Add("discrteffdt", typeof(string), "");
                    IR.Columns.Add("district", typeof(string), "");
                    IR.Columns.Add("crslnm", typeof(string), "");
                    IR.Columns.Add("regemailid", typeof(string), "");
                    IR.Columns.Add("add1", typeof(string), "");
                    IR.Columns.Add("add2", typeof(string), "");
                    IR.Columns.Add("add3", typeof(string), "");
                    IR.Columns.Add("add4", typeof(string), "");
                    IR.Columns.Add("add5", typeof(string), "");
                    IR.Columns.Add("add6", typeof(string), "");
                    IR.Columns.Add("add7", typeof(string), "");
                    IR.Columns.Add("usr_id", typeof(string), "");
                    IR.Columns.Add("usr_entdt", typeof(string), "");
                    IR.Columns.Add("paytrmcd", typeof(string), "");
                    IR.Columns.Add("paytrmnm", typeof(string), "");
                    IR.Columns.Add("duedays", typeof(string), "");
                    //details
                    IR.Columns.Add("slno", typeof(double), "");
                    IR.Columns.Add("styleno", typeof(string), "");
                    IR.Columns.Add("itnm", typeof(string), "");
                    IR.Columns.Add("stktype", typeof(string), "");
                    IR.Columns.Add("pcstyle", typeof(string), "");
                    IR.Columns.Add("sizes", typeof(string), "");
                    IR.Columns.Add("boxpcs", typeof(string), "");
                    IR.Columns.Add("tbox", typeof(double), "");
                    IR.Columns.Add("tpcs", typeof(double), "");
                    IR.Columns.Add("rate", typeof(double), "");
                    IR.Columns.Add("obldt1", typeof(string), "");
                    IR.Columns.Add("oblno1", typeof(string), "");
                    IR.Columns.Add("oblamt1", typeof(string), "");
                    IR.Columns.Add("osamt1", typeof(string), "");
                    IR.Columns.Add("obldt2", typeof(string), "");
                    IR.Columns.Add("oblno2", typeof(string), "");
                    IR.Columns.Add("oblamt2", typeof(string), "");
                    IR.Columns.Add("osamt2", typeof(string), "");
                    IR.Columns.Add("totos", typeof(string), "");
                    IR.Columns.Add("prefno", typeof(string), "");
                    IR.Columns.Add("prefdt", typeof(string), "");

                    Int32 maxR = rstbl.Rows.Count - 1;
                    Int32 maxC = 0;
                    Int32 i = 0; double partytotos = 0, totbox = 0, totset = 0, approxvalue = 0;
                    while (i <= maxR)
                    {
                        int j = 0;
                        string autono = rstbl.Rows[i]["autono"].ToString();
                        string docdt = rstbl.Rows[i]["docdt"].ToString().retDateStr();
                        sql = "";

                        sql += "select distinct max(b.docdt) docdt ";
                        sql += "from " + scm + ".t_pslipdtl a, " + scm + ".t_cntrl_hdr b ";
                        sql += "where a.ordautono='" + autono + "' and a.ordautono = b.autono(+) ";
                        DataTable tblorddt = Master_Help.SQLquery(sql);
                        string prndt = docdt;
                        if (blOnlyBal == true && tblorddt.Rows.Count > 0) prndt = tblorddt.Rows[0]["docdt"].ToString().retDateStr();


                        var rsDtl = rsPendOrd.Select("autono='" + autono + "'");

                        maxC = rsDtl.Length - 1;
                        int slno = 0;
                        double tset = 0;
                        while (j <= maxC)
                        {
                            string check1 = rsDtl[j]["itcd"].ToString() + rsDtl[j]["stktype"].ToString() + rsDtl[j]["freestk"] + Convert.ToDouble(rsDtl[j]["rate"].ToString());
                            string pcstyle = "", sizes = "", boxes = "";
                            double tbox = 0, tpcs = 0, rate = 0, ordqnty = 0, chkpcs = 0;

                            rate = Convert.ToDouble(rsDtl[j]["rate"]);
                            ordqnty = Convert.ToDouble(rsDtl[j]["ordqnty"]);
                            //approxvalue += Math.Round(rate * ordqnty, 2);
                            pcstyle = rsDtl[j]["pcsperbox"].ToString() + "/" + rsDtl[j]["pcsperset"].ToString() + "/" + rsDtl[j]["colrperset"].ToString();

                            while (rsDtl[j]["itcd"].ToString() + rsDtl[j]["stktype"].ToString() + rsDtl[j]["freestk"] + Convert.ToDouble(rsDtl[j]["rate"].ToString()) == check1)
                            {
                                approxvalue += Math.Round(rsDtl[j]["rate"].retDbl() * rsDtl[j]["ordqnty"].retDbl(), 2);//new
                                string fld = "balqnty";
                                if (blOnlyBal == true) fld = "balaspslip";
                                double dbboxes = Salesfunc.ConvPcstoBox(Convert.ToDouble(rsDtl[j][fld]), Convert.ToDouble(rsDtl[j]["pcsperbox"]));
                                if (boxes != "") boxes += "+";
                                if (sizes != "") sizes += ",";
                                sizes += rsDtl[j]["sizenm"] + "=" + dbboxes.ToString();

                                boxes += dbboxes.ToString();
                                tpcs = tpcs + Convert.ToDouble(rsDtl[j][fld]);
                                if (rsDtl[j]["stktype"].ToString() == "F") chkpcs = chkpcs + Convert.ToDouble(rsDtl[j][fld]);
                                j++;
                                if (j > maxC) break;
                            }
                            tbox = Salesfunc.ConvPcstoBox(chkpcs, Convert.ToDouble(rsDtl[j - 1]["pcsperbox"]));
                            //tset = tset + Salesfunc.ConvPcstoSet(chkpcs, Convert.ToDouble(rsDtl[j - 1]["pcsperset"]));
                            tset = Salesfunc.ConvPcstoSet(chkpcs, Convert.ToDouble(rsDtl[j - 1]["pcsperset"]));//new
                            totbox += tbox;
                            totset += tset;//new
                            DataRow Row1 = IR.NewRow();
                            Row1["docno"] = rstbl.Rows[i]["docno"].ToString();
                            Row1["docdt"] = prndt; // rstbl.Rows[i]["docdt"].ToString().Remove(10);
                            Row1["slnm"] = rstbl.Rows[i]["slnm"].ToString();
                            Row1["slcd"] = rstbl.Rows[i]["slcd"].ToString();
                            Row1["trslnm"] = rstbl.Rows[i]["trslnm"].ToString();
                            Row1["trslcd"] = rstbl.Rows[i]["trslcd"].ToString();
                            Row1["destn"] = rstbl.Rows[i]["district"].ToString();
                            Row1["agslnm"] = rstbl.Rows[i]["agslnm"].ToString();
                            Row1["agslcd"] = rstbl.Rows[i]["agslcd"].ToString();
                            Row1["slmslnm"] = rstbl.Rows[i]["slmslnm"].ToString();
                            Row1["slmslcd"] = rstbl.Rows[i]["slmslcd"].ToString();
                            Row1["cournm"] = rstbl.Rows[i]["cournm"].ToString();
                            Row1["delvtypedsc"] = Salesfunc.retDelvTypeDesc(rstbl.Rows[i]["cod"].ToString());
                            Row1["prcnm"] = rstbl.Rows[i]["prcnm"].ToString();
                            Row1["prceffdt"] = rstbl.Rows[i]["prceffdt"].ToString().retDateStr();
                            Row1["gstno"] = rstbl.Rows[i]["gstno"].ToString();
                            //Row1["docth1"] = Salesfunc.retDocTh(rstbl.Rows[i]["docth"].ToString()) + " " + rstbl.Rows[i]["docth1"].ToString();
                            Row1["docth2"] = rstbl.Rows[i]["docth2"].ToString();
                            Row1["docth3"] = rstbl.Rows[i]["docth3"].ToString();
                            Row1["scmnm"] = rstbl.Rows[i]["scmnm"].ToString();
                            Row1["totbox"] = totbox.ToString();
                            //Row1["toset"] = tset.ToString();
                            Row1["toset"] = totset.ToString();//new
                            //  Row1["ordamt"] = Convert.ToDouble(rstbl.Rows[i]["delvins"]);//
                            Row1["ordamt"] = approxvalue;
                            //extra
                            Row1["crslcd"] = rstbl.Rows[i]["crslcd"].ToString();
                            Row1["prccd"] = rstbl.Rows[i]["prccd"].ToString();
                            Row1["discrtcd"] = rstbl.Rows[i]["discrtcd"].ToString();
                            Row1["discrteffdt"] = rstbl.Rows[i]["discrteffdt"].ToString();
                            Row1["district"] = rstbl.Rows[i]["district"].ToString();
                            Row1["crslnm"] = rstbl.Rows[i]["crslnm"].ToString();
                            Row1["regemailid"] = rstbl.Rows[i]["regemailid"].ToString();
                            Row1["add1"] = rstbl.Rows[i]["add1"].ToString();
                            Row1["add2"] = rstbl.Rows[i]["add2"].ToString();
                            Row1["add3"] = rstbl.Rows[i]["add3"].ToString();
                            Row1["add4"] = rstbl.Rows[i]["add4"].ToString();
                            Row1["add5"] = rstbl.Rows[i]["add5"].ToString();
                            Row1["add6"] = rstbl.Rows[i]["add6"].ToString();
                            Row1["add7"] = rstbl.Rows[i]["add7"].ToString();
                            Row1["usr_id"] = rstbl.Rows[i]["usr_id"].ToString();
                            Row1["usr_entdt"] = rstbl.Rows[i]["usr_entdt"].ToString();
                            Row1["paytrmcd"] = rstbl.Rows[i]["paytrmcd"].ToString();
                            Row1["paytrmnm"] = rstbl.Rows[i]["paytrmnm"].ToString();
                            Row1["rem"] = "";
                            Row1["splnote"] = rstbl.Rows[i]["splnote"].ToString();
                            Row1["duedays"] = rstbl.Rows[i]["duedays"].ToString();
                            //details table
                            slno++;
                            Row1["slno"] = slno;
                            Row1["styleno"] = rsDtl[j - 1]["styleno"];
                            Row1["itnm"] = rsDtl[j - 1]["itnm"].ToString();
                            Row1["stktype"] = rsDtl[j - 1]["stktype"].ToString(); // Salesfunc.retStkTypeDesc(rsDtl[j - 1]["stktype"].ToString());
                            Row1["pcstyle"] = pcstyle;
                            Row1["sizes"] = sizes;
                            Row1["boxpcs"] = boxes;
                            Row1["tbox"] = tbox;
                            Row1["tpcs"] = tpcs;
                            Row1["rate"] = rate;
                            Row1["prefno"] = rstbl.Rows[i]["prefno"].ToString();
                            Row1["prefdt"] = rstbl.Rows[i]["prefdt"].ToString();
                            //if (VE.Checkbox3 == true) Row1["rateprint"] = "Y"; else Row1["rateprint"] = "N";

                            IR.Rows.Add(Row1);
                        }
                        i++;
                    }
                    //Company Location 
                    string[] compaddress; string stremail = "";
                    compaddress = Salesfunc.retCompAddress("").Split(Convert.ToChar(Cn.GCS()));
                    stremail = compaddress[6];
                    ReportDocument reportdocument = new ReportDocument();
                    string complogo = Salesfunc.retCompLogo();
                    EmailControl EmailControl = new EmailControl();
                    string blhead = "PARTY ORDER";
                    if (blOnlyBal == true) blhead = "BALANCE ORDER";

                    reportdocument.Load(Server.MapPath("~/Report/Rep_Ord.rpt"));
                    reportdocument.SetDataSource(IR);
                    reportdocument.SetParameterValue("partytotos", partytotos.ToString());
                    reportdocument.SetParameterValue("billheading", blhead);
                    //   reportdocument.SetParameterValue("complogo", complogo);
                    reportdocument.SetParameterValue("compnm", compaddress[0]);
                    reportdocument.SetParameterValue("compadd", compaddress[1]);
                    reportdocument.SetParameterValue("compstat", compaddress[2]);
                    reportdocument.SetParameterValue("locaadd", compaddress[3]);
                    reportdocument.SetParameterValue("locastat", compaddress[4]);
                    //reportdocument.SetParameterValue("prodlogo", "");
                    //reportdocument.SetParameterValue("legalname", compaddress[5]);                
                    Response.Buffer = false;
                    Response.ClearContent();
                    Response.ClearHeaders();
                    Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                    stream.Seek(0, SeekOrigin.Begin);
                    reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
                    return new FileStreamResult(stream, "application/pdf");
                }
                #endregion
                #region Old Format in Excel
                else
                {
                    rsPendOrd.DefaultView.Sort = "itgrpnm, itgrpcd, styleno, itcd, print_seq, sizecdgrp, sizenm";
                    rsPendOrd = rsPendOrd.DefaultView.ToTable();
                    string glcd = Salesfunc.retSDGlcd(rstbl.Rows[0]["slcd"].ToString());
                    #region checking no activity items
                    string mtrljobcd = "'FS','CT','PR','ST','JW','IR','WA','EM'";
                    string jobcd = "'ST','IR'";
                    string selitcd = "";
                    selitcd = "'" + string.Join("','", (from DataRow dr in rsPendOrd.AsEnumerable() select dr.Field<string>("itcd")).Distinct()) + "'";
                    tdt = rsPendOrd.Rows[0]["docdt"].ToString().retDateStr();
                    string crdt = System.DateTime.Now.retDateStr();
                    DataTable tblstk, tblwip;

                    tblstk = Salesfunc.GetStock(crdt, "", selitcd, "'FS'", "", "", "", "", "", "'F'", "", "", true, true);

                    double expectedAvailableBoxes = 0;
                    foreach (DataRow row in rsPendOrd.Rows)
                    {
                        DataRow rowsToUpdate = tblstk.AsEnumerable()
                            .Where(r => r.Field<string>("ITCD") == row.Field<string>("ITCD") && r.Field<string>("SIZECD") == row.Field<string>("SIZECD")).FirstOrDefault();
                        if (rowsToUpdate != null)
                        {
                            decimal? ORDQNTY = row.Field<decimal>("ORDQNTY");
                            decimal? stkqnty = rowsToUpdate.Field<decimal>("QNTY");
                            row.SetField("STOCKQNTY", stkqnty); double pndqnty = (stkqnty.retDcml() - ORDQNTY.retDcml()).retDbl();
                            if (pndqnty > 0)
                            {

                            }
                            else
                            {
                                pndqnty = 0;
                            }
                            rowsToUpdate.SetField("QNTY", pndqnty);
                        }
                    }
                    TempData["FINSTOCK_FOR_CONTINIOUSPRINTING"] = tblstk;
                    #endregion
                    string fld = "balqnty";
                    if (blOnlyBal == true) fld = "balaspslip";

                    Int32 maxR = rstbl.Rows.Count - 1;
                    Int32 maxC = 0, rNo = 0;
                    Int32 i = 0; double partytotos = 0, totbox = 0, approxvalue = 0;
                    while (i <= maxR)
                    {
                        int j = 0;
                        string autono = rstbl.Rows[i]["autono"].ToString();
                        string docdt = rstbl.Rows[i]["docdt"].ToString().retDateStr();
                        string docno = rstbl.Rows[i]["docno"].ToString();
                        string trslnm = rstbl.Rows[i]["trslnm"] == DBNull.Value ? rstbl.Rows[i]["cournm"].ToString() : rstbl.Rows[i]["trslnm"].ToString();
                        sql = "";
                        sql += "select distinct max(b.docdt) docdt ";
                        sql += "from " + scm + ".t_pslipdtl a, " + scm + ".t_cntrl_hdr b ";
                        sql += "where a.ordautono='" + autono + "' and a.ordautono = b.autono(+) ";
                        DataTable tblorddt = Master_Help.SQLquery(sql);
                        string prndt = docdt;
                        if (blOnlyBal == true && tblorddt.Rows.Count > 0) prndt = tblorddt.Rows[0]["docdt"].ToString().retDateStr();

                        var rsDtl = rsPendOrd.Select("autono='" + autono + "'");
                        string blhead = "PARTY ORDER";
                        if (blOnlyBal == true) blhead = "BALANCE ORDER";
                        ExcelPackage workbook = new ExcelPackage();
                        ExcelWorksheet worksheet = workbook.Workbook.Worksheets.Add("Sheet1");
                        int excelRow = 1; int excelColumn = 1;
                        worksheet.PrinterSettings.TopMargin = 1M / 2.54M;
                        worksheet.PrinterSettings.BottomMargin = 0M / 2.54M;
                        worksheet.PrinterSettings.LeftMargin = 0.75M / 2.54M;
                        worksheet.PrinterSettings.RightMargin = 0M / 2.54M;
                        worksheet.Cells[1, 1, 1, 9].Style.Font.Bold = true;
                        worksheet.Column(1).Width = 14;
                        worksheet.Column(2).Width = 6;
                        worksheet.Column(3).Width = 6;
                        worksheet.Column(4).Width = 6;
                        worksheet.Column(5).Width = 6;
                        worksheet.Column(6).Width = 6;
                        worksheet.Column(7).Width = 6;
                        worksheet.Column(8).Width = 6;
                        worksheet.Column(9).Width = 6;
                        worksheet.Column(10).Width = 6;
                        worksheet.Column(11).Width = 6;
                        worksheet.Column(12).Width = 6;
                        //string exlhd = "";
                        //exlhd += "<table><thead><tr><th style='width:130px'></th><th style='width:50px'></th><th style='width:50px'></th><th style='width:50px;'></th><th style='width:50px'></th><th style='width:50px'></th><th style='width:50px'></th><th style='width:50px'></th><th style='width:50px'></th><th style='width:50px'></th><th style='width:50px'></th><th style='width:50px'></th><th style='width:50px'></th><th style='width:50px'></th><th style='width:50px'></th></tr></thead><tbody>";
                        //exlhd += "<tr style='font-weight: bold'><td>" + "&nbsp;" + "</td><td colspan=8 style='font-size:15px; text-decoration:underline; text-align: center;'>" + blhead + "</td><td></td></tr>";
                        worksheet.Cells[excelRow, 4].Value = CommVar.CompName(UNQSNO); excelRow++;

                        worksheet.Cells[excelRow, 4].Value = blhead;
                        worksheet.Cells[excelRow, excelColumn].Style.Font.Bold = true;
                        worksheet.Cells[excelRow, 4].Style.Font.Bold = true;
                        //exlhd += "<tr style='font-weight: bold'><td></td><td colspan=3 style='border:0.1pt solid;padding-left: 5px;'>" + docno + "</td><td colspan=4 style='border:0.1pt solid;text-align: center;'>" + rstbl.Rows[i]["prefno"].ToString() + "</td><td style='font-weight:bold;'>Date</td><td colspan=2 style='font-weight:bold;'>" + docdt + "</td></tr>";
                        worksheet.Cells[++excelRow, 1].Value = "Order No"; worksheet.Cells[excelRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[excelRow, 2].Value = docno + (rstbl.Rows[i]["prefno"].ToString() == "" ? "" : "," + rstbl.Rows[i]["prefno"].ToString());
                        worksheet.Cells[excelRow, 2].Style.Font.Bold = true;
                        worksheet.Cells[excelRow, 9].Value = "Date"; worksheet.Cells[excelRow, 9].Style.Font.Bold = true;
                        worksheet.Cells[excelRow, 10].Value = docdt;
                        //exlhd += "<tr style='font-weight: bold'><td>Party Name</td><td colspan=9 style='font-size:15px' >" +  + "</td><td></td></tr>";
                        worksheet.Cells[++excelRow, 1].Value = "Party Name"; worksheet.Cells[excelRow, 1].Style.Font.Bold = true;
                        //worksheet.Cells[3, 2].Value = rstbl.Rows[i]["slnm"];
                        worksheet.Cells[excelRow, 2].Value = rstbl.Rows[i]["slnm"];
                        worksheet.Cells[excelRow, 2].Style.Font.Bold = true;
                        //exlhd += "<tr><td >Transporter</td><td colspan=6 style='font-size:16px;font-weight: bold'>" + trslnm + "</td><td></td><td style='font-weight:bold;'>GST</td><td colspan=3>" + rstbl.Rows[i]["gstno"].ToString() + "</td></tr>";
                        worksheet.Cells[++excelRow, 1].Value = "Transporter"; worksheet.Cells[excelRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[excelRow, 2].Value = trslnm;
                        worksheet.Cells[excelRow, 2].Style.Font.Bold = true;
                        worksheet.Cells[excelRow, 9].Value = "GST"; worksheet.Cells[excelRow, 9].Style.Font.Bold = true;
                        worksheet.Cells[excelRow, 10].Value = rstbl.Rows[i]["gstno"].ToString();

                        //exlhd += "<tr><td >Destination</td><td colspan=4 style='font-size:15px;font-weight: bold'>" + rstbl.Rows[i]["district"] + "</td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td><td></td></tr>";
                        worksheet.Cells[++excelRow, 1].Value = "Destination"; worksheet.Cells[excelRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[excelRow, 2].Value = rstbl.Rows[i]["district"];
                        worksheet.Cells[excelRow, 2].Style.Font.Bold = true;


                        //exlhd += "<tr><td >Agent Name</td><td colspan=6 style='font-size:15px;font-weight: bold'>" + rstbl.Rows[i]["agslnm"] + "</td><td></td><td style='font-weight: bold;'>PAN</td><td colspan=3>" + rstbl.Rows[i]["panno"] + "</td></tr>";
                        worksheet.Cells[++excelRow, 1].Value = "Agent Name"; worksheet.Cells[excelRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[excelRow, 2].Value = rstbl.Rows[i]["agslnm"];
                        worksheet.Cells[excelRow, 9].Value = "PAN"; worksheet.Cells[excelRow, 9].Style.Font.Bold = true;
                        worksheet.Cells[excelRow, 10].Value = rstbl.Rows[i]["panno"];

                        //exlhd += "<tr><td >Rate Name</td><td colspan=3 style='font-size:16px;font-weight: bold'>" + rstbl.Rows[i]["prcnm"] + "</td><td></td><td></td><td></td><td></td><td colspan=2 style='font-weight: bold;'>Doc Thru</td><td>" + Salesfunc.retDocTh(rstbl.Rows[i]["docth"].ToString()) + "</td></tr>";
                        worksheet.Cells[++excelRow, 1].Value = "Rate Name"; worksheet.Cells[excelRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[excelRow, 2].Value = rstbl.Rows[i]["prcnm"];
                        worksheet.Cells[excelRow, 9].Value = "Doc Thru";
                        worksheet.Cells[excelRow, 9].Style.Font.Bold = true;
                        worksheet.Cells[excelRow, 11].Value = rstbl.Rows[i]["docth"].ToString();

                        //exlhd += "<tr><td >Case No.</td><td colspan=2 style='font-size:15px;font-weight: bold'></td><td></td><td></td><td></td><td></td><td></td><td colspan=3 style='font-weight: bold;'>Despatch Date</td><td></td></tr>";
                        //exlhd += "<tr><td >Special Note</td><td colspan=5 style='font-size:15px;font-weight: bold'>" + rstbl.Rows[i]["splnote"] + "</td><td></td><td></td><td colspan=3 style='font-weight: bold;'>Total Cases</td><td></td></tr></table>";
                        worksheet.Cells[++excelRow, 1].Value = "Special Note"; worksheet.Cells[excelRow, 1].Style.Font.Bold = true;
                        worksheet.Cells[excelRow, 2].Value = rstbl.Rows[i]["splnote"]; //worksheet.Cells[8, 8].Value = "Total Cases"; worksheet.Cells[8, 9].Value = rstbl.Rows[i]["docth"].ToString();

                        //string excelstr = "";
                        maxC = rsDtl.Length - 1;
                        int slno = 0;
                        double tbox = 0, tpcs = 0, tset = 0;
                        selitcd = "'" + string.Join("','", (from DataRow dr in rsDtl.AsEnumerable() select dr.Field<string>("itcd")).Distinct()) + "'";
                        // Checking for size grouping
                        DataTable rssize = Salesfunc.retSizeGrpData(selitcd);
                        excelColumn = 1;
                        while (j <= maxC)
                        {
                            string itgrpcd = rsDtl[j]["itgrpcd"].ToString();
                            if (j == 145)
                            {

                            }
                            var rssizehead = (from DataRow dr in rssize.Rows
                                              select new
                                              {
                                                  itgrpcd = dr["itgrpcd"],
                                                  sizecdgrp = dr["sizecdgrp"],
                                                  print_seq = dr["print_seq"]
                                              }).Where(x => x.itgrpcd.ToString() == itgrpcd).Distinct().OrderBy(x => x.print_seq.ToString()).ToList();

                            DataTable tblsizegrp = ListToDatatable.LINQResultToDataTable(rssizehead);

                            DataTable IR = new DataTable();
                            IR.Columns.Add("itgrpcd", typeof(string), "");
                            IR.Columns.Add("itgrpnm", typeof(string), "");
                            IR.Columns.Add("itcd", typeof(string), "");
                            IR.Columns.Add("stktype", typeof(string), "");
                            IR.Columns.Add("freestk", typeof(string), "");
                            IR.Columns.Add("styleno", typeof(string), "");
                            IR.Columns.Add("sizecd", typeof(string), "");
                            IR.Columns.Add("rate", typeof(double), "");
                            IR.Columns.Add("sizecdgrp", typeof(string), "");
                            IR.Columns.Add("pcsperbox", typeof(double), "");
                            IR.Columns.Add("pcsperset", typeof(double), "");
                            IR.Columns.Add("mixsize", typeof(string), "");
                            IR.Columns.Add("AVLBLSTOCK", typeof(string), "");
                            IR.Columns.Add("tqnty", typeof(double), "");
                            for (int z = 0; z <= tblsizegrp.Rows.Count - 1; z++)
                            {
                                string sznm = Salesfunc.retsizemaxmin(tblsizegrp.Rows[z]["sizecdgrp"].ToString());
                                IR.Columns.Add(sznm, typeof(double), "");
                                IR.Columns.Add(sznm + "_AVLBLSTOCK", typeof(string));
                            }
                            while (rsDtl[j]["itgrpcd"].ToString() == itgrpcd)
                            {
                                string check1 = rsDtl[j]["itcd"].ToString() + rsDtl[j]["stktype"].ToString() + rsDtl[j]["freestk"] + Convert.ToDouble(rsDtl[j]["rate"].ToString());
                                double ibox = 0, ipcs = 0, rate = 0, ordqnty = 0, stkqnty = 0;
                                string itcd = rsDtl[j]["itcd"].ToString();
                                string stktype = rsDtl[j]["stktype"].ToString();
                                string freestk = rsDtl[j]["freestk"].ToString();
                                string AVLBLSTOCK = "Y";
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["itcd"] = itcd;
                                IR.Rows[rNo]["styleno"] = rsDtl[j]["styleno"];
                                IR.Rows[rNo]["stktype"] = rsDtl[j]["stktype"];
                                IR.Rows[rNo]["freestk"] = rsDtl[j]["freestk"];
                                //IR.Rows[rNo]["rate"] = 0; // Convert.ToDouble(rsDtl[j]["rate"]);
                                var tyyi = rsDtl.CopyToDataTable();
                                while (rsDtl[j]["itcd"].ToString() == itcd && rsDtl[j]["stktype"].ToString() == stktype && rsDtl[j]["freestk"].ToString() == freestk)
                                {
                                    string sizecdgrp = rsDtl[j]["sizecdgrp"].ToString(), sizes = "", boxes = "";
                                    ordqnty = 0; stkqnty = 0;
                                    while (rsDtl[j]["itcd"].ToString() == itcd && rsDtl[j]["stktype"].ToString() == stktype && rsDtl[j]["freestk"].ToString() == freestk && rsDtl[j]["sizecdgrp"].ToString() == sizecdgrp)
                                    {
                                        ordqnty = ordqnty + Convert.ToDouble(rsDtl[j][fld]);
                                        stkqnty += rsDtl[j]["STOCKQNTY"].retDbl();
                                        approxvalue += Math.Round(rate * Convert.ToDouble(rsDtl[j][fld]), 2);
                                        j++;
                                        if (j > maxC) break;
                                    }
                                    double box = Salesfunc.ConvPcstoBox(ordqnty, Convert.ToDouble(rsDtl[j - 1]["pcsperbox"]));
                                    double stkqntyinbox = Salesfunc.ConvPcstoBox(ordqnty, Convert.ToDouble(rsDtl[j - 1]["pcsperbox"]));
                                    if (minimumStockCoalculatedOn > stkqntyinbox) { AVLBLSTOCK = "N"; }
                                    else {
                                        AVLBLSTOCK = "Y";
                                        expectedAvailableBoxes += box;
                                    }

                                    string szfld = Salesfunc.retsizemaxmin(sizecdgrp);
                                    if (szfld == "")
                                    {
                                        szfld = "";
                                    }
                                    else
                                    {
                                        IR.Rows[rNo][szfld] = box;
                                        IR.Rows[rNo][szfld + "_AVLBLSTOCK"] = AVLBLSTOCK;
                                    }
                                    ibox = ibox + box;
                                    ipcs = ipcs + ordqnty;
                                    if (j > maxC) break;
                                }
                                IR.Rows[rNo]["tqnty"] = ibox;
                                IR.Rows[rNo]["AVLBLSTOCK"] = AVLBLSTOCK;
                                tbox = tbox + ibox;
                                tpcs = tpcs + ipcs;
                                if (j > maxC) break;
                            }
                            Int32 s = 0, maxS = 0;
                            maxS = IR.Rows.Count - 1;
                            //excelstr += "<table>";
                            //excelstr += "<thead><tr>";

                            //excelstr += "<th style='border:0.5pt solid;width:130px'>" + "Style No" + "</th>";
                            excelColumn = 1; ++excelRow;
                            worksheet.Cells[++excelRow, excelColumn].Value = "Style No";
                            worksheet.Cells[excelRow, excelColumn].Style.Font.Bold = true;
                            GenerateBorder(worksheet, excelRow, excelColumn);
                            for (int es = 13; es <= IR.Columns.Count - 1; es++)
                            {
                                string colnm = IR.Columns[es].ColumnName;
                                if (colnm != "tqnty") ++es;
                                //excelstr += "<th style='border:0.5pt solid;width:50px'>" + colnm + "</th>";
                                worksheet.Cells[excelRow, ++excelColumn].Value = colnm;
                                worksheet.Cells[excelRow, excelColumn].Style.Font.Bold = true;
                                GenerateBorder(worksheet, excelRow, excelColumn);
                            }
                            //excelstr += "</tr></thead><tbody>";
                            while (s <= maxS)
                            {
                                excelColumn = 1;
                                //excelstr += "<tr>";
                                //string itmcolr = "";
                                //excelstr += "<td style='border:0.1pt solid;'>" + IR.Rows[s]["styleno"].ToString() + "</td>";
                                worksheet.Cells[++excelRow, excelColumn].Value = IR.Rows[s]["styleno"].ToString();
                                GenerateBorder(worksheet, excelRow, excelColumn);
                                for (int es = 13; es < IR.Columns.Count - 1; es++)
                                {
                                    if (IR.Columns[es].ColumnName == "tqnty")
                                    {
                                        //excelstr += "<td style='border:0.1pt solid;'>" + IR.Rows[s][es] + "</td>";
                                        worksheet.Cells[excelRow, ++excelColumn].Value = IR.Rows[s][es];
                                    }
                                    else
                                    {
                                        es++;
                                        worksheet.Cells[excelRow, ++excelColumn].Value = IR.Rows[s][es - 1];
                                        if (IR.Rows[s][es].ToString() == "N")
                                        {
                                            worksheet.Cells[excelRow, excelColumn].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                            worksheet.Cells[excelRow, excelColumn].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSalmon);
                                            //itmcolr = " background-color: lightsalmon;";
                                        }
                                        else
                                        {
                                            //itmcolr = "";
                                        }
                                        //excelstr += "<td style='border:0.1pt solid;" + itmcolr + "'>" + IR.Rows[s][es - 1] + "</td>";
                                    }
                                    GenerateBorder(worksheet, excelRow, excelColumn);
                                }
                                //excelstr += "</tr>";
                                s++;
                            }
                        }
                        ++excelRow; excelColumn = 1;
                        worksheet.Cells[++excelRow, excelColumn].Value = "Total Boxes";
                        worksheet.Cells[excelRow, excelColumn].Style.Font.Bold = true;
                        worksheet.Cells[excelRow, 2].Value = tbox.ToString();

                        worksheet.Cells[++excelRow, excelColumn].Value = "Total Expected available Stock";
                        worksheet.Cells[excelRow, excelColumn].Style.Font.Bold = true;
                        worksheet.Cells[excelRow, 5].Value = expectedAvailableBoxes.ToString();

                        //lect nvl(discrate,0) discrate,  nvl(scmdiscrate, 0) scmdiscrate,  nvl(tddiscrate, 0) tddiscrate ";

                        //if (DTdiscsorddtl.Rows[0]["scmdiscrate"].retDbl() != 0)
                        //{
                        //    worksheet.Cells[++excelRow, excelColumn].Value = "Scheme Discount Rate";
                        //    worksheet.Cells[excelRow, excelColumn].Style.Font.Bold = true;
                        //    worksheet.Cells[excelRow, 3].Value = DTdiscsorddtl.Rows[0]["scmdiscrate"].retDbl();
                        //}
                        //if (DTdiscsorddtl.Rows[0]["tddiscrate"].retDbl() != 0)
                        //{
                        //    worksheet.Cells[++excelRow, excelColumn].Value = "Trade Discount Rate";
                        //    worksheet.Cells[excelRow, excelColumn].Style.Font.Bold = true;
                        //    worksheet.Cells[excelRow, 3].Value = DTdiscsorddtl.Rows[0]["tddiscrate"].retDbl();
                        //}
                        //if (DTdiscsorddtl.Rows[0]["discrate"].retDbl() != 0)
                        //{
                        //    worksheet.Cells[++excelRow, excelColumn].Value = "STD Discount Rate";
                        //    worksheet.Cells[excelRow, excelColumn].Style.Font.Bold = true;
                        //    worksheet.Cells[excelRow, 3].Value = DTdiscsorddtl.Rows[0]["discrate"].retDbl();
                        //}


                        worksheet.Cells[++excelRow, excelColumn].Value = "Prepared by";
                        worksheet.Cells[excelRow, excelColumn].Style.Font.Bold = true;
                        //excelstr += "</tr>";
                        //excelstr += "<tr>";
                        //excelstr += CommFunc.retHtmlCell(rstbl.Rows[i]["usr_id"].ToString(), "C", true, 16);
                        worksheet.Cells[excelRow, 2].Value = rstbl.Rows[i]["usr_id"].ToString();
                        //excelstr += "</tr>";
                        excelColumn = 0;
                        //excelstr += "<tr>" + CommFunc.retHtmlCell("") + "</tr>";
                        if (VE.Checkbox1 == true)
                        {
                            ++excelRow;
                            //excelstr += "<tr>";++excelRow
                            //excelstr += CommFunc.retHtmlCell("Bill Date", "C", true, 15);
                            worksheet.Cells[++excelRow, ++excelColumn].Value = "Bill Date";
                            worksheet.Cells[excelRow, excelColumn].Style.Font.Bold = true;
                            //excelstr += CommFunc.retHtmlCell("Bill No", "C", true, 15, 0, "", 2);
                            worksheet.Cells[excelRow, ++excelColumn].Value = "Bill No";
                            worksheet.Cells[excelRow, excelColumn].Style.Font.Bold = true;
                            ++excelColumn;
                            //excelstr += CommFunc.retHtmlCell("O/s Amt", "C", true, 15, 0, "", 2);
                            ++excelColumn;
                            worksheet.Cells[excelRow, ++excelColumn].Value = "O/s Amt";
                            worksheet.Cells[excelRow, excelColumn].Style.Font.Bold = true;
                            ++excelColumn;
                            //excelstr += CommFunc.retHtmlCell("", "C", true, 15);
                            //excelstr += CommFunc.retHtmlCell("Bill Date", "C", true, 15, 0, "", 2);
                            worksheet.Cells[excelRow, ++excelColumn].Value = "Bill Date";
                            worksheet.Cells[excelRow, excelColumn].Style.Font.Bold = true;
                            ++excelColumn;
                            //excelstr += CommFunc.retHtmlCell("Bill No", "C", true, 15, 0, "", 2);
                            worksheet.Cells[excelRow, ++excelColumn].Value = "Bill No";
                            worksheet.Cells[excelRow, excelColumn].Style.Font.Bold = true;
                            //excelstr += CommFunc.retHtmlCell("O/s Amt", "C", true, 15, 0, "", 2);
                            ++excelColumn;
                            ++excelColumn;
                            excelColumn = 0;

                        }
                        Response.ClearContent();
                        Response.Clear();
                        Response.Buffer = true;
                        Response.Charset = "";
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AddHeader("content-disposition", "attachment;filename=" + docno.retRepname() + ".xlsx");
                        using (MemoryStream MyMemoryStream = new MemoryStream())
                        {
                            workbook.SaveAs(MyMemoryStream);
                            MyMemoryStream.WriteTo(Response.OutputStream);
                            Response.Flush();
                            Response.End();
                        }
                        workbook.Dispose();
                        i++;
                    }
                    Response.End();
                }
                return Content("Download sucessfully");
                #endregion
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