using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class SaleBill_PrintController : Controller
    {
        // GET: SaleBill_Print
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        MasterHelpFa MasterHelpFa = new MasterHelpFa();

        string UNQSNO = CommVar.getQueryStringUNQSNO();

        public ActionResult SaleBill_Print()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Document Printing";
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

                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    //string reptype = "SALEBILL";
                    string reptype = "SALEBILL";
                    if (VE.MENU_PARA == "SBCM" || VE.MENU_PARA == "SBCMR") reptype = "CASHMEMO";
                    if (VE.maxdate == "CHALLAN") reptype = "CHALLAN";
                    DataTable repformat = Salesfunc.getRepFormat(reptype);

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
        public ActionResult GetSubLedgerDetails(string val, string code)
        {
            try
            {
                var str = masterHelp.SLCD_help(val, code);
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


        [HttpPost]
        public ActionResult SaleBill_Print(ReportViewinHtml VE, FormCollection FC, string submitbutton)
        {
            try
            {
                if (VE.MENU_PARA != "SBCM" && VE.MENU_PARA != "SBCMR" && VE.Checkbox8 == true)
                {
                    if (VE.TDT.retStr() == "")
                    {
                        return Content("Please Enter/Select To Date");
                    }
                    return ReportBarcodeImagePrint(VE, FC, VE.TDT.retStr(), VE.BARNO.retStr());
                }
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

                var printemail = submitbutton.ToString();
                string fdate = "", tdate = "";
                if (VE.FDT != null)
                {
                    fdate = Convert.ToString(Convert.ToDateTime(FC["FDT"].ToString())).Substring(0, 10);
                }
                if (VE.TDT != null)
                {
                    tdate = Convert.ToString(Convert.ToDateTime(FC["TDT"].ToString())).Substring(0, 10);
                }
                string fdocno = FC["FDOCNO"].ToString();
                string tdocno = FC["TDOCNO"].ToString();
                string doccd = FC["doccd"].ToString();
                string docnm = DB.M_DOCTYPE.Find(doccd).DOCNM;
                string slcd = VE.TEXTBOX3;

                string[] copyno = new string[6];
                if (VE.Checkbox1 == true) copyno[0] = "Y"; else copyno[0] = "N";
                if (VE.Checkbox2 == true) copyno[1] = "Y"; else copyno[1] = "N";
                if (VE.Checkbox3 == true) copyno[2] = "Y"; else copyno[2] = "N";
                if (VE.Checkbox4 == true) copyno[3] = "Y"; else copyno[3] = "N";
                if (VE.Checkbox5 == true) copyno[4] = "Y"; else copyno[4] = "N";
                if (VE.Checkbox6 == true) copyno[5] = "Y"; else copyno[5] = "N";

                if (copyno[0] == "N" && copyno[1] == "N" && copyno[2] == "N" && copyno[3] == "N" && copyno[4] == "N" && copyno[5] == "N")
                {
                    copyno[0] = "Y";
                }

                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), Scm1 = CommVar.CurSchema(UNQSNO), Scmf = CommVar.FinSchema(UNQSNO), scmI = CommVar.InvSchema(UNQSNO);
                string yr_cd = CommVar.YearCode(UNQSNO);
                //string str1 = "";
                //DataTable rsTmp;
                //string doctype = "";
                //str1 = "select doctype from " + Scm1 + ".m_doctype where doccd='" + VE.DOCCD + "'";
                //rsTmp = masterHelp.SQLquery(str1);
                //doctype = rsTmp.Rows[0]["doctype"].ToString();

                string prnemailid = "";
                //if (VE.TEXTBOX5 != null) prnemailid = "'" + VE.TEXTBOX5 + "' regemailid"; else prnemailid = "a.regemailid";
                if (VE.TEXTBOX5 != null) prnemailid = "'" + VE.TEXTBOX5 + "' regemailid"; else prnemailid = "e.regemailid";

                string sql = "", sqlc = "";

                #region SMS Send
                if (printemail == "SMS")
                {
                    string SmsRetVal = "";
                    SmsRetVal = SaleSMSSend("", doccd, slcd, fdate, tdate, fdocno, tdocno);
                    string[] msgretval = SmsRetVal.Split('=');
                    return Content(SmsRetVal);
                }
                #endregion
                //DataTable IR = new DataTable();
                string rptname = "";
                int maxR = 0; string blhead = "", gocd = "", grpemailid = "";


                if (VE.MENU_PARA == "SBCM" || VE.MENU_PARA == "SBCMR")
                {
                    return ReportCashMemoPrint(VE, fdate, tdate, fdocno, tdocno, COM, LOC, yr_cd, slcd, doccd, prnemailid, maxR, blhead, gocd, grpemailid, Scm1, Scmf, scmI, copyno, rptname, printemail, docnm);
                }
                else
                {
                    return ReportSaleBillPrint(VE, fdate, tdate, fdocno, tdocno, COM, LOC, yr_cd, slcd, doccd, prnemailid, maxR, blhead, gocd, grpemailid, Scm1, Scmf, scmI, copyno, rptname, printemail, docnm);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }

        //sql = "";

        //sqlc = "";
        //sqlc += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and c.yr_cd='" + yr_cd + "' and ";
        //if (fdocno != "") sqlc += "c.doconlyno >= '" + fdocno + "' and c.doconlyno <= '" + tdocno + "' and ";
        //if (fdate != "") sqlc += "c.docdt >= to_date('" + fdate + "','dd/mm/yyyy') and ";
        //if (tdate != "") sqlc += "c.docdt <= to_date('" + tdate + "','dd/mm/yyyy') and ";
        //if (slcd != null) sqlc += "b.slcd='" + slcd + "' and ";
        //sqlc += "c.doccd = '" + doccd + "' and ";

        ////sql += "select a.autono, b.itgrpcd, b.doctag, h.doccd, h.docno, h.docdt, b.areacd, b.invisstime, b.duedays,d.ADVRECDREM,d.ADVRECDAMT, h.canc_rem, h.cancel, ";
        ////sql += "a.itmprccd, o.mrp, l.prcdesc itmprcdesc, l.effdt prceffdt, ";
        ////sql += "b.gocd, k.gonm, k.goadd1, k.goadd2, k.goadd3, k.gophno, k.goemail, k.fssailicno, ";
        ////sql += "b.weekno, j.batchdlprint, j.grpemailid, h.usr_id, h.usr_entdt, h.vchrno, ";

        ////sql += "nvl(e.pslcd,e.slcd) oslcd, j.debglcd, ";
        ////sql += "b.slcd, z.sapcd partycd, nvl(e.fullname,e.slnm) slnm, " + prnemailid + ", e.add1 sladd1, e.add2 sladd2, e.add3 sladd3, e.add4 sladd4, e.add5 sladd5, e.add6 sladd6, e.add7  ";
        ////sql += "sladd7, e.gstno, e.panno, trim(e.regmobile||decode(e.regmobile,null,'',',')||e.slphno||decode(e.phno1,null,'',','||e.phno1)) phno, e.state, e.country, e.statecd,e.actnameof slactnameof,";
        ////sql += "nvl(b.conslcd,b.slcd) cslcd, '' cpartycd, nvl(f.fullname,f.slnm) cslnm, f.add1 csladd1, f.add2 csladd2, f.add3 csladd3, f.add4 csladd4, f.add5 csladd5, ";
        ////sql += "f.add6 csladd6, f.add7 csladd7, nvl(f.gstno,f.gstno) cgstno, nvl(f.panno,f.panno) cpanno,f.actnameof cslactnameof, ";
        ////sql += "trim(f.regmobile||decode(f.regmobile,null,'',',')||f.slphno||decode(f.phno1,null,'',','||f.phno1)) cphno, ";
        ////sql += "f.state cstate, f.statecd cstatecd, ";

        ////sql += "b.porefno, b.porefdt, c.translcd trslcd, g.slnm trslnm, g.gstno trgst, g.add1 trsladd1, g.add2 trsladd2, g.add3 trsladd3, g.add4 trsladd4, g.phno1 trslphno, ";
        ////sql += "c.lrno, c.lrdt, c.lorryno, c.ewaybillno, c.grwt, c.ntwt, ";
        ////sql += "a.slno, a.itcd, a.itnm, a.itrem, a.damstock, a.batchdtl, a.packsize, a.prodcd, a.nosinbag, a.hsncode, a.nos, a.qnty, nvl(i.decimals,0) qdecimal, i.uomnm,  ";
        ////sql += "a.rate, a.rateqntybag, a.basamt, d.sapopdno, d.sapblno, d.sapshipno, d.docrem, d.docth, d.nopkgs, d.agslcd, m.slnm agslnm, ";
        ////sql += "a.tddisctype, a.tddiscrate, nvl(a.tddiscamt,0)tddiscamt, a.disctype, a.discrate, a.discamt, a.agdocno, a.agdocdt, nvl(d.bltophead,a.bltophead) bltophead, a.makenm, ";
        ////sql += "nvl(a.igstper,0)igstper, nvl(a.igstamt,0)igstamt, nvl(a.cgstper,0)cgstper, nvl(a.cgstamt,0)cgstamt, nvl(a.sgstper,0)sgstper, nvl(a.sgstamt,0)sgstamt, nvl(a.dutyper,0)dutyper, nvl(a.dutyamt,0)dutyamt, nvl(a.cessper,0)cessper, nvl(a.cessamt,0)cessamt, nvl(a.igstper+a.cgstper+a.sgstper,0) gstper,  ";
        ////sql += "nvl(b.roamt,0)roamt, nvl(b.blamt,0) blamt, nvl(b.tcsper,0) tcsper, nvl(b.tcsamt,0) tcsamt, d.insby, d.payterms,d.precarr,d.precarrrecpt,d.shipmarkno,d.portload,d.portdesc,d.finaldest,d.bankinter,d.insudesc, ";
        ////sql += "d.othnm, nvl(d.othadd1,f.othadd1) othadd1, decode(d.othadd1,null,f.othadd2,d.othadd2) othadd2, decode(d.othadd1,null,f.othadd3,d.othadd3) othadd3, decode(d.othadd1,null,f.othadd4,d.othadd4) othadd4, ";
        ////sql += " nvl(a.mtrlcd,n.mtrlcd) mtrlcd, nvl(a.poslno,n.poslno) poslno, nvl(d.plsupply,nvl(f.state,e.state)) plsupply, nvl(d.destn,e.district) destn, y.bas_rate, y.pv_per, decode(nvl(y.pv_per,0),0,'N','Y') pv_tag,b.curr_cd from ( ";

        ////sql += "select a.autono, a.autono||a.slno autoslno, a.ordautoslno, nvl(a.itmprccd,b.itmprccd) itmprccd, a.slno, a.itcd, d.itnm, ";
        ////sql += "nvl(a.bluomcd,d.uomcd) uomcd, a.damstock, nvl(a.hsncode,nvl(d.hsncode,e.hsncode)) hsncode, a.itrem, d.prodcd, d.packsize, d.nosinbag, ";
        ////sql += "a.nos, nvl(a.blqnty,a.qnty) qnty, a.rate, nvl(e.rateqntybag,f.rateqntybag) rateqntybag, a.basamt, a.tddisctype, a.tddiscrate, a.tddiscamt, a.disctype, a.discrate, a.discamt, ";
        ////sql += "a.agdocno, to_char(a.agdocdt, 'dd/mm/yyyy') agdocdt, b.slcd||b.itgrpcd||c.compcd||c.loccd slitgrpcd, e.bltophead, g.makenm, ";
        ////sql += "listagg(o.batchno || ' (' || n.qnty || ')', ', ') within group (order by n.autono, n.slno, n.batchautono) batchdtl, ";
        ////sql += "a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.dutyper, a.dutyamt, a.cessper, a.cessamt,a.mtrlcd, a.poslno  ";
        ////sql += "from " + Scm1 + ".t_txndtl a, " + Scm1 + ".t_txn b, " + Scm1 + ".t_cntrl_hdr c, " + Scm1 + ".m_sitem d, " + Scm1 + ".m_brgrp e, " + Scm1 + ".m_group f, " + Scm1 + ".m_make g, ";
        ////sql += Scm1 + ".t_batchdtl  n, " + Scm1 + ".t_batchmst o ";
        ////sql += "where a.autono=b.autono and a.autono=c.autono and a.itcd=d.itcd and a.autono=n.autono(+) and a.slno=n.slno(+) and n.batchautono=o.batchautono(+) and ";
        ////sql += sqlc;
        ////sql += "d.brgrpcd=e.brgrpcd(+) and b.itgrpcd=f.itgrpcd(+) and a.makecd=g.makecd(+) ";
        ////sql += "group by a.autono, a.autono||a.slno, a.ordautoslno, nvl(a.itmprccd,b.itmprccd), a.slno, a.itcd, d.itnm, ";
        ////sql += "nvl(a.bluomcd,d.uomcd), a.damstock, nvl(a.hsncode,nvl(d.hsncode,e.hsncode)), a.itrem, d.prodcd, d.packsize, d.nosinbag, ";
        ////sql += "a.nos, nvl(a.blqnty,a.qnty), a.rate, nvl(e.rateqntybag,f.rateqntybag), a.basamt, a.tddisctype, a.tddiscrate, a.tddiscamt, a.disctype, a.discrate, a.discamt, ";
        ////sql += "a.agdocno, to_char(a.agdocdt, 'dd/mm/yyyy'), b.slcd||b.itgrpcd||c.compcd||c.loccd, e.bltophead, g.makenm, ";
        ////sql += "a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.dutyper, a.dutyamt, a.cessper, a.cessamt,a. mtrlcd, a.poslno  ";
        ////if (CommVar.InvSchema(UNQSNO) != "")
        ////{
        ////    sql += "union all ";

        ////    sql += "select a.autono, a.autono||a.slno autoslno, '' ordautoslno, '' itmprccd, a.slno+500 slno, a.itcd, d.itdescn itnm, ";
        ////    sql += "d.uom uomcd, '' damstock, d.hsncd hsncode, a.itrem, '' prodcd, 0 packsize, 0 nosinbag, ";
        ////    sql += "0 nos, a.qnty, nvl(a.rate,0) rate, '' rateqntybag, 0 basamt, '' tddisctype, 0 tddiscrate, 0 tddiscamt, '' disctype, 0 discrate, 0 discamt, ";
        ////    sql += "'' agdocno, to_char('', 'dd/mm/yyyy') agdocdt, b.slcd||b.itgrpcd||c.compcd||c.loccd slitgrpcd, '' bltophead, '' makenm, ";
        ////    sql += "'' batchdtl, ";
        ////    sql += "nvl(a.igstper,0) igstper, nvl(a.igstamt,0) igstamt, nvl(a.cgstper,0) cgstper, nvl(a.cgstamt,0) cgstamt, nvl(a.sgstper,0) sgstper, nvl(a.sgstamt,0) sgstamt, 0 dutyper, 0 dutyamt, nvl(a.cessper,0) cessper, nvl(a.cessamt,0) cessamt,'' mtrlcd, '' poslno  ";
        ////    sql += "from " + Scm1 + ".t_txnproddtl a, " + Scm1 + ".t_txn b, " + Scm1 + ".t_cntrl_hdr c, " + scmI + ".m_item d ";
        ////    sql += "where a.autono=b.autono and a.autono=c.autono and ";
        ////    sql += sqlc;
        ////    sql += "a.itcd=d.itcd ";
        ////}
        ////sql += "union all ";

        ////sql += "select a.autono, a.autono autoslno, '' ordautoslno, b.itmprccd, nvl(ascii(d.calccode),0)+1000 slno, '' itcd, d.amtnm||' '||a.amtdesc itnm, ";
        ////sql += "'' uomcd, '' damstock, a.hsncode hsncode,  '' itrem, '' prodcd, 0 packsize, 0 nosinbag, ";
        ////sql += "0 nos, 0 qnty, 0 rate, '' rateqntybag, a.amt basamt, '' tddisctype, 0 tddiscrate, 0 tddiscamt, '' disctype, 0 discrate, 0 discamt, ";
        ////sql += "'' agdocno, '' agdocdt, b.slcd||b.itgrpcd||c.compcd||c.loccd slitgrpcd, '' bltophead, '' makenm, '' batchdtl, ";
        ////sql += "a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.dutyper, a.dutyamt, a.cessper, a.cessamt ,'' mtrlcd, '' poslno ";
        ////sql += "from " + Scm1 + ".t_txnamt a, " + Scm1 + ".t_txn b, " + Scm1 + ".t_cntrl_hdr c, " + Scm1 + ".m_amttype d  ";
        ////sql += "where a.autono=b.autono and a.autono=c.autono and ";
        ////sql += sqlc;
        ////sql += "a.amtcd=d.amtcd(+) ";
        ////sql += ") a, ";

        ////sql += "(select a.sapcd, a.slcd||a.itgrpcd||a.compcd||a.loccd slitgrpcd ";
        ////sql += "from " + Scm1 + ".m_subleg_sddtl_itgrp a ) z, ";

        ////sql += "(select a.autono||a.slno autoslno, a.bas_rate, a.pv_per ";
        ////sql += "from " + Scm1 + ".t_txndtl a ) y, ";

        ////sql += Scm1 + ".t_txn b, " + Scm1 + ".t_txntrans c, " + Scm1 + ".t_txnoth d, " + Scmf + ".m_subleg e, " + Scmf + ".m_subleg f, " + Scmf + ".m_subleg g, " + Scm1 + ".t_cntrl_hdr h, ";
        ////sql += Scmf + ".m_uom i, " + Scm1 + ".m_group j, " + Scm1 + ".m_godown k, " + Scm1 + ".m_itemplist l, " + Scmf + ".m_subleg m, " + Scm1 + ".t_sorddtl n, " + Scm1 + ".m_itemplistdtl o ";
        ////sql += "where a.autono=b.autono and a.autono=c.autono(+) and a.autono=d.autono(+) and b.slcd=e.slcd and nvl(b.conslcd,b.slcd)=f.slcd(+) and c.translcd=g.slcd(+) and a.autono=h.autono and ";
        ////sql += "b.itgrpcd=j.itgrpcd(+) and a.uomcd=i.uomcd(+) and b.gocd=k.gocd(+) and a.itmprccd=l.itmprccd(+) and a.itmprccd=o.itmprccd(+) and a.itcd=o.itcd(+) and ";
        ////sql += "a.slitgrpcd=z.slitgrpcd(+) and d.agslcd=m.slcd(+) and a.ordautoslno=n.autoslno(+) and a.autoslno=y.autoslno(+) and ";
        ////sql += "a.autono not in ( ";
        ////sql += "select a.autono ";
        ////sql += "from " + Scm1 + ".t_cntrl_doc_pass a, " + Scm1 + ".t_cntrl_hdr b, " + Scm1 + ".t_cntrl_auth c ";
        ////sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and c.autono is null and ";
        ////sql += "b.doccd='" + doccd + "' ) ";
        ////sql += "order by docno,autono,slno";
        //sql += " select a.autono, b.doctag, h.doccd, h.docno, h.docdt, b.duedays, h.canc_rem, h.cancel,0 invisstime,'N' batchdlprint,  ";
        //sql += " b.gocd, k.gonm, k.goadd1, k.goadd2, k.goadd3, k.gophno, k.goemail, h.usr_id, h.usr_entdt, h.vchrno, nvl(e.pslcd, e.slcd) oslcd, b.slcd, ";
        //sql += " nvl(e.fullname, e.slnm) slnm, " + prnemailid + ", e.add1 sladd1, e.add2 sladd2, e.add3 sladd3, e.add4 sladd4, e.add5 sladd5, e.add6 sladd6, e.add7 sladd7,  ";
        //sql += " e.gstno, e.panno, trim(e.regmobile || decode(e.regmobile, null, '', ',') || e.slphno || decode(e.phno1, null, '', ',' || e.phno1)) phno, e.state, e.country, e.statecd, e.actnameof slactnameof,  ";
        //sql += " nvl(b.conslcd, b.slcd) cslcd, '' cpartycd, nvl(f.fullname, f.slnm) cslnm, f.add1 csladd1, f.add2 csladd2, f.add3 csladd3, f.add4 csladd4, f.add5 csladd5, ";
        //sql += " f.add6 csladd6, f.add7 csladd7, nvl(f.gstno, f.gstno) cgstno, nvl(f.panno, f.panno) cpanno,f.actnameof cslactnameof, ";
        //sql += " trim(f.regmobile || decode(f.regmobile, null, '', ',') || f.slphno || decode(f.phno1, null, '', ',' || f.phno1)) cphno, f.state cstate, f.statecd cstatecd,  ";
        //sql += " c.translcd trslcd, g.slnm trslnm, g.gstno trgst, g.add1 trsladd1, g.add2 trsladd2, g.add3 trsladd3, g.add4 trsladd4, g.phno1 trslphno, c.lrno,  ";
        //sql += " c.lrdt, c.lorryno, c.ewaybillno, c.grwt, c.ntwt, a.slno, a.itcd, a.styleno, a.itnm, a.itrem, a.batchdtl, a.hsncode,  ";
        //sql += " a.nos, a.qnty, nvl(i.decimals, 0) qdecimal, i.uomnm, a.rate, a.amt, d.docrem, d.docth, d.casenos, d.noofcases,  ";
        //sql += " d.agslcd, m.slnm agslnm, a.agdocno, a.agdocdt, j.itgrpnm, j.shortnm,  ";
        //sql += " nvl(a.igstper, 0)igstper, nvl(a.igstamt, 0)igstamt, nvl(a.cgstper, 0)cgstper, nvl(a.cgstamt, 0)cgstamt,  ";
        //sql += " nvl(a.sgstper, 0)sgstper, nvl(a.sgstamt, 0)sgstamt, nvl(a.dutyper, 0)dutyper, nvl(a.dutyamt, 0)dutyamt, nvl(a.cessper, 0)cessper, nvl(a.cessamt, 0)cessamt,  ";
        //sql += " nvl(a.igstper + a.cgstper + a.sgstper, 0) gstper, nvl(b.roamt, 0)roamt, nvl(b.blamt, 0) blamt, nvl(b.tcsper, 0) tcsper, nvl(b.tcsamt, 0) tcsamt, d.insby,  ";
        //sql += " d.othnm, nvl(d.othadd1, f.othadd1) othadd1, d.porefno, d.porefdt, d.despby, d.dealby, d.packby, d.selby,  ";
        //sql += " decode(d.othadd1, null, f.othadd2, d.othadd2) othadd2, decode(d.othadd1, null, f.othadd3, d.othadd3) othadd3, decode(d.othadd1, null, f.othadd4, d.othadd4) othadd4,  ";
        //sql += " z.disctype, z.discrate, z.discamt, z.scmdisctype, z.scmdiscrate, z.scmdiscamt, z.tddisctype, z.tddiscrate, z.tddiscamt,  ";
        //sql += " b.curr_cd from  ";

        //sql += " (select a.autono, a.autono || a.slno autoslno, a.slno, a.itcd, d.itnm, nvl(o.pdesign, d.styleno) styleno, d.uomcd, nvl(a.hsncode, nvl(d.hsncode, f.hsncode)) hsncode,  ";
        //sql += " a.itrem, a.baleno, a.nos, nvl(a.blqnty, a.qnty) qnty, a.flagmtr, a.rate, a.amt, a.agdocno, to_char(a.agdocdt, 'dd/mm/yyyy') agdocdt,  ";
        //sql += " listagg(o.barno || ' (' || n.qnty || ')', ', ') within group(order by n.autono, n.slno) batchdtl,  ";
        //sql += " a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.dutyper, a.dutyamt, a.cessper, a.cessamt  ";
        //sql += " from " + Scm1 + ".t_txndtl a, " + Scm1 + ".t_txn b, " + Scm1 + ".t_cntrl_hdr c, " + Scm1 + ".m_sitem d, " + Scm1 + ".m_group f, " + Scm1 + ".t_batchdtl  n, " + Scm1 + ".t_batchmst o  ";
        //sql += " where a.autono = b.autono and a.autono = c.autono and a.itcd = d.itcd and a.autono = n.autono(+) and a.slno = n.txnslno(+) and n.barno = o.barno(+) and  ";
        //sql += " c.compcd = '" + COM + "' and c.loccd = '" + LOC + "' and c.yr_cd = '" + yr_cd + "' and  ";
        //if (fdocno != "") sql += " c.doconlyno >= '" + fdocno + "' and c.doconlyno <= '" + tdocno + "' and  ";
        //if (fdate != "") sql += " c.docdt >= to_date('" + fdate + "', 'dd/mm/yyyy') and  ";
        //if (tdate != "") sql += " c.docdt <= to_date('" + tdate + "', 'dd/mm/yyyy') and  ";
        //sql += " c.doccd = '" + doccd + "' and d.itgrpcd = f.itgrpcd(+)  ";
        //sql += " group by a.autono, a.autono || a.slno, a.slno, a.itcd, d.itnm, nvl(o.pdesign, d.styleno), d.uomcd, nvl(a.hsncode, nvl(d.hsncode, f.hsncode)),  ";
        //sql += " a.itrem, a.baleno, a.nos, nvl(a.blqnty, a.qnty), a.flagmtr, a.rate, a.amt, a.agdocno, to_char(a.agdocdt, 'dd/mm/yyyy'),  ";
        //sql += " a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.dutyper, a.dutyamt, a.cessper, a.cessamt  ";
        //sql += " union all  ";

        //sql += " select a.autono, a.autono autoslno, nvl(ascii(d.calccode), 0) + 1000 slno, '' itcd, d.amtnm || ' ' || a.amtdesc itnm, '' styleno, '' uomcd, a.hsncode hsncode,  ";
        //sql += " '' itrem, '' baleno, 0 nos, 0 qnty, 0 flagmtr, 0 rate, a.amt, '' agdocno, '' agdocdt, '' batchdtl,  ";
        //sql += " a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.dutyper, a.dutyamt, a.cessper, a.cessamt  ";
        //sql += " from " + Scm1 + ".t_txnamt a, " + Scm1 + ".t_txn b, " + Scm1 + ".t_cntrl_hdr c, " + Scm1 + ".m_amttype d  ";
        //sql += " where a.autono = b.autono and a.autono = c.autono and c.compcd = '" + COM + "' and c.loccd = '" + LOC + "' and c.yr_cd = '" + yr_cd + "' and  ";
        //if (fdocno != "") sql += " c.doconlyno >= '" + fdocno + "' and c.doconlyno <= '" + tdocno + "' and ";
        //if (fdate != "") sql += " c.docdt >= to_date('" + fdate + "', 'dd/mm/yyyy') and  ";
        //if (tdate != "") sql += " c.docdt <= to_date('" + tdate + "', 'dd/mm/yyyy') and  ";
        //sql += "c.doccd = '" + doccd + "'  ";
        //sql += "and a.amtcd = d.amtcd(+)  ";
        //sql += " ) a,  ";

        //sql += " " + Scm1 + ".t_txndtl z, " + Scm1 + ".t_txn b, " + Scm1 + ".t_txntrans c, " + Scm1 + ".t_txnoth d, " + Scmf + ".m_subleg e, " + Scmf + ".m_subleg f, " + Scmf + ".m_subleg g,  ";
        //sql += " " + Scm1 + ".t_cntrl_hdr h, " + Scmf + ".m_uom i, " + Scm1 + ".m_group j, " + Scm1 + ".m_godown k, " + Scm1 + ".m_sitem l, " + Scmf + ".m_subleg m  ";
        //sql += " where a.autono = z.autono(+) and a.slno = z.slno(+) and a.autono = b.autono and a.autono = c.autono(+) and a.autono = d.autono(+) and  ";
        //sql += " b.slcd = e.slcd and nvl(b.conslcd, b.slcd) = f.slcd(+) and c.translcd = g.slcd(+) and a.autono = h.autono and a.itcd = l.itcd(+) and l.itgrpcd = j.itgrpcd(+) and a.uomcd = i.uomcd(+) and  ";
        //sql += " b.gocd = k.gocd(+) and d.agslcd = m.slcd(+) and  ";
        //if (slcd != null) sql += " b.slcd ='" + slcd + "' and ";
        //sql += " a.autono not in (select a.autono from " + Scm1 + ".t_cntrl_doc_pass a, " + Scm1 + ".t_cntrl_hdr b, " + Scm1 + ".t_cntrl_auth c  ";
        //sql += " where a.autono = b.autono(+) and a.autono = c.autono(+) and c.autono is null and b.doccd = '" + doccd + "' )   ";
        //sql += " order by docno,autono,slno  ";
        //DataTable tbl = new DataTable();
        //tbl = masterHelp.SQLquery(sql);
        //if (tbl.Rows.Count == 0) return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "Records not found !!" });

        //DataTable rsStkPrcDesc;
        //sql = "";
        //sql += "select distinct a.autono, a.autono||a.itcd autoitcd, e.prcdesc stkprcdesc ";
        //sql += "from " + Scm1 + ".t_batchdtl a, " + Scm1 + ".t_batchmst d, " + Scm1 + ".t_txn b, " + Scm1 + ".m_itemplist e, " + Scm1 + ".t_cntrl_hdr c ";
        //sql += "where a.batchautono=d.batchautono(+) and d.itmprccd=e.itmprccd(+) and a.autono=b.autono(+) and ";
        //sql += sqlc;
        //sql += "a.autono=c.autono ";
        //rsStkPrcDesc = masterHelp.SQLquery(sql);

        //string blterms = "", inspoldesc = "", dealsin = "";
        //Int16 bankslno = 0;
        ////sql = "select blterms, inspoldesc, dealsin, nvl(bankslno,0) bankslno from " + Scm1 + ".m_mgroup_spl where compcd='" + CommVar.Compcd(UNQSNO) + "' and itgrpcd='" + tbl.Rows[0]["itgrpcd"].ToString() + "'";
        ////DataTable rsMgroupSpl = masterHelp.SQLquery(sql);
        ////if (rsMgroupSpl.Rows.Count > 0)
        ////{
        ////    if (rsMgroupSpl.Rows[0]["blterms"].ToString() != "") blterms = rsMgroupSpl.Rows[0]["blterms"].ToString();
        ////    inspoldesc = rsMgroupSpl.Rows[0]["inspoldesc"].ToString();
        ////    dealsin = rsMgroupSpl.Rows[0]["dealsin"].ToString();
        ////    bankslno = Convert.ToInt16(rsMgroupSpl.Rows[0]["bankslno"]);
        ////}

        //#region  Datatabe IR generate
        ////DataTable IR = new DataTable();
        //IR.Columns.Add("autono", typeof(string), "");
        //IR.Columns.Add("copymode", typeof(string), "");
        //IR.Columns.Add("docno", typeof(string), "");
        //IR.Columns.Add("docdt", typeof(string), "");
        //IR.Columns.Add("insby", typeof(string), "");
        //IR.Columns.Add("invisstime", typeof(double), "");
        //IR.Columns.Add("duedays", typeof(double), "");
        //IR.Columns.Add("country", typeof(string), "");
        //IR.Columns.Add("itgrpnm", typeof(string), "");
        //IR.Columns.Add("shortnm", typeof(string), "");
        //IR.Columns.Add("stkprcdesc", typeof(string), "");
        //IR.Columns.Add("gocd", typeof(string), "");
        //IR.Columns.Add("gonm", typeof(string), "");
        //IR.Columns.Add("goadd1", typeof(string), "");
        //IR.Columns.Add("goadd2", typeof(string), "");
        //IR.Columns.Add("goadd3", typeof(string), "");
        //IR.Columns.Add("gophno", typeof(string), "");
        //IR.Columns.Add("goemail", typeof(string), "");
        //IR.Columns.Add("slcd", typeof(string), "");
        //IR.Columns.Add("partycd", typeof(string), "");
        //IR.Columns.Add("slnm", typeof(string), "");
        //IR.Columns.Add("sladd1", typeof(string), "");
        //IR.Columns.Add("sladd2", typeof(string), "");
        //IR.Columns.Add("sladd3", typeof(string), "");
        //IR.Columns.Add("sladd4", typeof(string), "");
        //IR.Columns.Add("sladd5", typeof(string), "");
        //IR.Columns.Add("sladd6", typeof(string), "");
        //IR.Columns.Add("sladd7", typeof(string), "");
        //IR.Columns.Add("sladd8", typeof(string), "");
        //IR.Columns.Add("sladd9", typeof(string), "");
        //IR.Columns.Add("sladd10", typeof(string), "");
        //IR.Columns.Add("othadd1", typeof(string), "");
        //IR.Columns.Add("othadd2", typeof(string), "");
        //IR.Columns.Add("othadd3", typeof(string), "");
        //IR.Columns.Add("othadd4", typeof(string), "");
        //IR.Columns.Add("disctype", typeof(string), "");
        //IR.Columns.Add("discrate", typeof(double), "");
        //IR.Columns.Add("cslcd", typeof(string), "");
        //IR.Columns.Add("cpartycd", typeof(string), "");
        //IR.Columns.Add("cslnm", typeof(string), "");
        //IR.Columns.Add("csladd1", typeof(string), "");
        //IR.Columns.Add("csladd2", typeof(string), "");
        //IR.Columns.Add("csladd3", typeof(string), "");
        //IR.Columns.Add("csladd4", typeof(string), "");
        //IR.Columns.Add("csladd5", typeof(string), "");
        //IR.Columns.Add("csladd6", typeof(string), "");
        //IR.Columns.Add("csladd7", typeof(string), "");
        //IR.Columns.Add("csladd8", typeof(string), "");
        //IR.Columns.Add("csladd9", typeof(string), "");
        //IR.Columns.Add("csladd10", typeof(string), "");
        //IR.Columns.Add("porefno", typeof(string), "");
        //IR.Columns.Add("porefdt", typeof(string), "");
        //IR.Columns.Add("trslcd", typeof(string), "");
        //IR.Columns.Add("trslnm", typeof(string), "");
        //IR.Columns.Add("trgst", typeof(string), "");
        //IR.Columns.Add("lrno", typeof(string), "");
        //IR.Columns.Add("lrdt", typeof(string), "");
        //IR.Columns.Add("lorryno", typeof(string), "");
        //IR.Columns.Add("ewaybillno", typeof(string), "");
        //IR.Columns.Add("grwt", typeof(double), "");
        //IR.Columns.Add("ntwt", typeof(double), "");
        //IR.Columns.Add("caltype", typeof(double), "");
        //IR.Columns.Add("slno", typeof(double), "");
        //IR.Columns.Add("itcd", typeof(string), "");
        //IR.Columns.Add("styleno", typeof(string), "");
        //IR.Columns.Add("itnm", typeof(string), "");
        //IR.Columns.Add("itrem", typeof(string), "");
        //IR.Columns.Add("itdesc", typeof(string), "");
        //IR.Columns.Add("batchdtl", typeof(string), "");
        //IR.Columns.Add("gstno", typeof(double), "");
        //IR.Columns.Add("hsncode", typeof(string), "");
        //IR.Columns.Add("nos", typeof(double), "");
        //IR.Columns.Add("casenos", typeof(double), "");
        //IR.Columns.Add("noofcases", typeof(double), "");
        //IR.Columns.Add("qnty", typeof(double), "");
        //IR.Columns.Add("uomnm", typeof(string), "");
        //IR.Columns.Add("qdecimal", typeof(double), "");
        //IR.Columns.Add("rate", typeof(double), "");
        //IR.Columns.Add("rateuomnm", typeof(string), "");
        //IR.Columns.Add("amt", typeof(double), "");
        //IR.Columns.Add("stddisc", typeof(string), "");
        //IR.Columns.Add("tddiscamt", typeof(double), "");
        //IR.Columns.Add("disc", typeof(string), "");
        //IR.Columns.Add("discamt", typeof(double), "");
        //IR.Columns.Add("scmdisctype", typeof(string), "");
        //IR.Columns.Add("scmdiscrate", typeof(double), "");
        //IR.Columns.Add("scmdiscamt", typeof(double), "");
        //IR.Columns.Add("tddisctype", typeof(string), "");
        //IR.Columns.Add("tddiscrate", typeof(double), "");
        //IR.Columns.Add("txblval", typeof(string), "");
        //IR.Columns.Add("cgstdsp", typeof(string), "");
        //IR.Columns.Add("cgstper", typeof(double), "");
        //IR.Columns.Add("cgstamt", typeof(double), "");
        //IR.Columns.Add("sgstdsp", typeof(string), "");
        //IR.Columns.Add("sgstper", typeof(double), "");
        //IR.Columns.Add("sgstamt", typeof(double), "");
        //IR.Columns.Add("cessper", typeof(double), "");
        //IR.Columns.Add("cessamt", typeof(double), "");
        //IR.Columns.Add("gstper", typeof(double), "");
        //IR.Columns.Add("discper", typeof(double), "");
        //IR.Columns.Add("revchrg", typeof(string), "");
        //IR.Columns.Add("rupinword", typeof(string), "");
        //IR.Columns.Add("netamt", typeof(double), "");
        //IR.Columns.Add("roamt", typeof(double), "");
        //IR.Columns.Add("blamt", typeof(string), "");
        //IR.Columns.Add("tcsper", typeof(double), "");
        //IR.Columns.Add("tcsamt", typeof(double), "");
        //IR.Columns.Add("blremarks", typeof(string), "");
        //IR.Columns.Add("agdocno", typeof(string), "");
        //IR.Columns.Add("agdocdt", typeof(string), "");
        //IR.Columns.Add("usr_id", typeof(string), "");
        //IR.Columns.Add("usr_entdt", typeof(string), "");
        //IR.Columns.Add("vchrno", typeof(string), "");
        //IR.Columns.Add("cancel", typeof(string), "");
        //IR.Columns.Add("canc_rem", typeof(string), "");
        //IR.Columns.Add("titdiscamt", typeof(double), "");
        //IR.Columns.Add("oslcd", typeof(string), "");
        //IR.Columns.Add("agslcd", typeof(string), "");
        //IR.Columns.Add("docth", typeof(string), "");
        //IR.Columns.Add("transgst", typeof(string), "");
        //IR.Columns.Add("agentnm", typeof(string), "");
        //IR.Columns.Add("trsladd1", typeof(string), "");
        //IR.Columns.Add("trsladd2", typeof(string), "");
        //IR.Columns.Add("trsladd3", typeof(string), "");
        //IR.Columns.Add("trsladd4", typeof(string), "");
        //IR.Columns.Add("payterms", typeof(string), "");
        //IR.Columns.Add("bankactno", typeof(string), "");
        //IR.Columns.Add("bankname", typeof(string), "");
        //IR.Columns.Add("bankbranch", typeof(string), "");
        //IR.Columns.Add("bankifsc", typeof(string), "");
        //IR.Columns.Add("bankadd", typeof(string), "");
        //IR.Columns.Add("bankrtgs", typeof(string), "");
        //IR.Columns.Add("duedt", typeof(string), "");
        //IR.Columns.Add("igstper", typeof(double), "");
        //IR.Columns.Add("igstamt", typeof(double), "");
        //IR.Columns.Add("dutyper", typeof(double), "");
        //IR.Columns.Add("dutyamt", typeof(double), "");
        //IR.Columns.Add("dtldsc", typeof(string), "");
        //IR.Columns.Add("dtlamt", typeof(string), "");
        //IR.Columns.Add("despby", typeof(string), "");
        //IR.Columns.Add("dealby", typeof(string), "");
        //IR.Columns.Add("packby", typeof(string), "");
        //IR.Columns.Add("selby", typeof(string), "");
        //IR.Columns.Add("dealsin", typeof(string), "");
        //IR.Columns.Add("blterms", typeof(string), "");
        //IR.Columns.Add("hsn_cd", typeof(string), "");
        //IR.Columns.Add("hsn_hddsp1", typeof(string), "");
        //IR.Columns.Add("hsn_hddsp2", typeof(string), "");
        //IR.Columns.Add("hsn_txblval", typeof(string), "");
        //IR.Columns.Add("hsn_gstper1", typeof(string), "");
        //IR.Columns.Add("hsn_gstamt1", typeof(string), "");
        //IR.Columns.Add("hsn_gstper2", typeof(string), "");
        //IR.Columns.Add("hsn_gstamt2", typeof(string), "");
        //IR.Columns.Add("hsn_gstper3", typeof(string), "");
        //IR.Columns.Add("hsn_gstamt3", typeof(string), "");
        //IR.Columns.Add("hsn_cessamt", typeof(string), "");
        //IR.Columns.Add("hsn_gstamt", typeof(string), "");
        //IR.Columns.Add("hsn_qnty", typeof(string), "");
        //IR.Columns.Add("hsn_tqnty", typeof(string), "");
        //IR.Columns.Add("hsn_ttxblval", typeof(string), "");
        //IR.Columns.Add("hsn_tgstamt1", typeof(string), "");
        //IR.Columns.Add("hsn_tgstamt2", typeof(string), "");
        //IR.Columns.Add("hsn_tgstamt3", typeof(string), "");
        //IR.Columns.Add("totalosamt", typeof(string), "");
        //IR.Columns.Add("rateqntybag", typeof(string), "");
        //IR.Columns.Add("regemailid", typeof(string), "");
        //IR.Columns.Add("menu_para", typeof(string), "");
        //IR.Columns.Add("upiimg", typeof(string), "");
        //IR.Columns.Add("upidesc", typeof(string), "");
        //IR.Columns.Add("curr_cd", typeof(string), "");
        //IR.Columns.Add("shipmarkno", typeof(string), "");
        //IR.Columns.Add("blitdesc", typeof(string), "");


        //IR.Columns.Add("agstdocno", typeof(string), "");
        //IR.Columns.Add("agstdocdt", typeof(string), "");
        //IR.Columns.Add("makenm", typeof(string), "");
        //IR.Columns.Add("areacd", typeof(string), "");
        //IR.Columns.Add("itmprccd", typeof(string), "");
        //IR.Columns.Add("itmprcdesc", typeof(string), "");
        //IR.Columns.Add("prceffdt", typeof(string), "");
        //IR.Columns.Add("weekno", typeof(double), "");
        //IR.Columns.Add("ordrefno", typeof(string), "");
        //IR.Columns.Add("ordrefdt", typeof(string), "");
        //IR.Columns.Add("packsize", typeof(double), "");
        //IR.Columns.Add("hsnsaccd", typeof(string), "");
        //IR.Columns.Add("prodcd", typeof(string), "");
        //IR.Columns.Add("basamt", typeof(double), "");
        //IR.Columns.Add("stddiscamt", typeof(double), "");
        //IR.Columns.Add("taxableval", typeof(string), "");
        //IR.Columns.Add("user_id", typeof(string), "");
        //IR.Columns.Add("bltophead", typeof(string), "");
        //IR.Columns.Add("nopkgs", typeof(string), "");
        //IR.Columns.Add("mrp", typeof(double), "");
        //IR.Columns.Add("poslno", typeof(string), "");
        //IR.Columns.Add("plsupply", typeof(string), "");
        //IR.Columns.Add("destn", typeof(string), "");
        //IR.Columns.Add("mtrlcd", typeof(string), "");
        //IR.Columns.Add("bas_rate", typeof(string), "");
        //IR.Columns.Add("pv_per", typeof(string), "");
        //IR.Columns.Add("insudesc", typeof(string), "");
        //IR.Columns.Add("pvtag", typeof(string), "");
        //IR.Columns.Add("precarr", typeof(string), "");
        //IR.Columns.Add("precarrrecpt", typeof(string), "");
        //IR.Columns.Add("portload", typeof(string), "");
        //IR.Columns.Add("portdesc", typeof(string), "");
        //IR.Columns.Add("finaldest", typeof(string), "");
        //IR.Columns.Add("bankinter", typeof(string), "");
        //#endregion

        //string bankname = "", bankactno = "", bankbranch = "", bankifsc = "", bankadd = "", bankrtgs = "";
        //sql = "";
        //sql += "select a.bankname, a.bankactno, a.ifsccode, a.address, a.branch ";
        //sql += "from " + Scmf + ".m_loca_ifsc a ";
        //sql += "where a.compcd = '" + COM + "' and a.loccd = '" + LOC + "' and ";
        //if (bankslno == 0) sql += "a.defltbank = 'T' ";
        //else sql += "a.slno=" + bankslno.ToString() + " ";
        //DataTable rsbank = masterHelp.SQLquery(sql);
        //if (rsbank.Rows.Count > 0)
        //{
        //    bankrtgs += "You can Make RTGS/NEFT to " + rsbank.Rows[0]["bankname"] + " ( IFSC-" + rsbank.Rows[0]["ifsccode"] + " ) A/c No-" + rsbank.Rows[0]["bankactno"];
        //    if (rsbank.Rows[0]["address"].ToString() != "") bankrtgs += " Address - " + rsbank.Rows[0]["address"];
        //    bankrtgs += ".";

        //    bankname = rsbank.Rows[0]["bankname"].ToString();
        //    bankactno = rsbank.Rows[0]["bankactno"].ToString();
        //    bankifsc = rsbank.Rows[0]["ifsccode"].ToString();
        //    bankbranch = rsbank.Rows[0]["branch"].ToString();
        //    bankadd = rsbank.Rows[0]["address"].ToString();
        //}

        //maxR = tbl.Rows.Count - 1;
        //Int32 i = 0; int istore = 0; int lslno = 0; int ilast = 0;
        //string auto1 = ""; string copymode = ""; string blrem = ""; string itdsc = "";
        ///*string blhead = "";*/ string fssailicno = ""; /*string grpemailid = "";*/ string goadd = "" /*gocd = ""*/;
        //string rupinwords = "";
        //int uomdecimal = 3; int uommaxdecimal = 0;

        //switch (tbl.Rows[0]["doctag"].ToString())
        //{
        //    case "SB":
        //        blhead = "TAX INVOICE"; break;
        //    case "SD":
        //        blhead = "DEBIT NOTE"; break;
        //    case "SC":
        //        blhead = "CREDIT NOTE"; break;
        //    case "SR":
        //        blhead = "CREDIT NOTE"; break;
        //    case "PR":
        //        blhead = "PURCHASE RETURN NOTE"; break;
        //    case "PD":
        //        blhead = "DEBIT NOTE [Purchase]"; break;
        //    case "PC":
        //        blhead = "CREDIT NOTE [Purchase]"; break;
        //    case "PI":
        //        blhead = "PROFORMA INVOICE"; break;
        //    case "CI":
        //        blhead = "TAX INVOICE"; break;
        //    case "SO":
        //        blhead = "STOCK TRANSFER"; break;
        //    case "PB":
        //        blhead = "PURCHASE INVOICE"; break;
        //    default: blhead = ""; break;
        //}

        //Int16 maxCopy = 5;

        //while (i <= maxR)
        //{

        //    //grpemailid = tbl.Rows[i]["grpemailid"].ToString();
        //    gocd = tbl.Rows[i]["gocd"].ToString();
        //    goadd = tbl.Rows[i]["goadd1"].ToString() + " " + tbl.Rows[i]["goadd2"].ToString() + " " + tbl.Rows[i]["goadd3"].ToString();
        //    if (tbl.Rows[i]["gophno"].ToString() != "") goadd = goadd + " Phone : " + tbl.Rows[i]["gophno"].ToString();
        //    istore = i;
        //    string docno = "";
        //    for (int ic = 0; ic <= maxCopy; ic++)
        //    {
        //        i = istore;
        //        lslno = 0;
        //        auto1 = tbl.Rows[i]["autono"].ToString();
        //        double dbasamt = 0; double ddisc1 = 0; double ddisc2 = 0; double dtxblval = 0;
        //        double dcgstamt = 0; double dsgstamt = 0; double dnetamt = 0; double dnos = 0; double dqnty = 0;
        //        bool doctotprint = false; bool totalreadyprint = false; bool delvchrg = false;

        //        string dtldsc = "", dtlamt = "";
        //        double tqnty = 0, tnos = 0, tamt = 0, tgst = 0, blamt = 0, totalosamt = 0;
        //        string hsnqnty = "", hsntaxblval = "", hsngstper1 = "", hsngstper2 = "", hsngstper3 = "", hsngstamt1 = "", hsngstamt2 = "", hsngstamt3 = "", hsncode = "";
        //        double gstper1 = 0, gstamt1 = 0, total_qnty = 0, total_taxval = 0, total_gstamt1 = 0, total_gstamt2 = 0, total_gstamt3 = 0;
        //        bool flagi = false, flagc = false, flags = false;

        //        if (copyno[ic].ToString() != "N")
        //        {
        //            //rupinwords = Cn.AmountInWords((tbl.Rows[i]["blamt"].retDbl() - tbl.Rows[i]["advrecdamt"].retDbl()).ToString(), tbl.Rows[i]["curr_cd"].ToString());
        //            //rupinwords = Cn.AmountInWords((tbl.Rows[i]["blamt"].retDbl() - tbl.Rows[i]["advrecdamt"].retDbl()).ToString());
        //            string oslcd = "", oglcd = "", odocdt = "", oclass1cd = "";

        //            if (doctype == "SBILL" && VE.Checkbox7 == true)
        //            {
        //                oslcd = tbl.Rows[i]["oslcd"].ToString();
        //                oglcd = tbl.Rows[i]["debglcd"].ToString();
        //                odocdt = Convert.ToDateTime(tbl.Rows[i]["docdt"].ToString()).AddDays(-1).ToString().retDateStr();
        //                totalosamt = Convert.ToDouble(MasterHelpFa.slcdbal(oslcd, oglcd, odocdt, oclass1cd));
        //                oslcd.retStr();

        //                sql = "";
        //                sql += "select sum(nvl(a.blamt,0)) blamt from ( ";
        //                sql += "select nvl(b.pslcd,a.slcd) oslcd, sum(nvl(a.blamt,0)) blamt ";
        //                sql += "from " + Scm1 + ".t_txn a, " + Scmf + ".m_subleg b, " + Scm1 + ".t_cntrl_hdr c, " + Scm1 + ".m_doctype d ";
        //                sql += "where a.autono=c.autono and c.doccd=d.doccd and a.slcd=b.slcd and nvl(c.cancel,'N')='N' and c.compcd='" + COM + "' and c.yr_cd='" + yr_cd + "' and ";
        //                sql += "c.docdt=to_date('" + tbl.Rows[i]["docdt"].ToString().retDateStr() + "','dd/mm/yyyy') and ";
        //                sql += "d.doctype='SBILL' and c.vchrno <= " + Convert.ToDouble(tbl.Rows[i]["vchrno"]) + " and c.doccd='" + doccd + "' ";
        //                sql += "group by nvl(b.pslcd,a.slcd) ) a where oslcd='" + oslcd + "'";
        //                rsTmp = MasterHelpFa.SQLquery(sql);
        //                if (rsTmp.Rows.Count > 0) totalosamt = totalosamt + Convert.ToDouble(rsTmp.Rows[0]["blamt"] == DBNull.Value ? 0 : rsTmp.Rows[0]["blamt"]);
        //            }

        //            Type A_T = tbl.Rows[0]["amt"].GetType(); Type Q_T = tbl.Rows[0]["qnty"].GetType(); Type N_S = tbl.Rows[0]["nos"].GetType(); Type I_T = tbl.Rows[0]["igstamt"].GetType();
        //            Type C_T = tbl.Rows[0]["cgstamt"].GetType(); Type S_T = tbl.Rows[0]["sgstamt"].GetType();

        //            var GST_DATA = (from DataRow DR in tbl.Rows
        //                            where DR["autono"].ToString() == auto1
        //                            group DR by new { IGST = DR["igstper"].ToString(), CGST = DR["cgstper"].ToString(), SGST = DR["sgstper"].ToString() } into X
        //                            select new
        //                            {
        //                                IGSTPER = X.Key.IGST,
        //                                CGSTPER = X.Key.CGST,
        //                                SGSTPER = X.Key.SGST,
        //                                TAMT = A_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("amt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("amt"))),
        //                                TQNTY = Q_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("qnty")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("qnty"))),
        //                                TNOS = N_S.Name == "Double" ? X.Sum(Z => Z.Field<double>("nos")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("nos"))),
        //                                IGSTAMT = I_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("igstamt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("igstamt"))),
        //                                CGSTAMT = C_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("cgstamt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("cgstamt"))),
        //                                SGSTAMT = S_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("sgstamt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("sgstamt"))),
        //                                TOTALPER = Convert.ToDouble(X.Key.IGST) + Convert.ToDouble(X.Key.CGST) + Convert.ToDouble(X.Key.SGST)
        //                            }).OrderBy(A => A.TOTALPER).ToList();

        //            if (GST_DATA != null && GST_DATA.Count > 0)
        //            {
        //                foreach (var k in GST_DATA)
        //                {
        //                    if (k.IGSTAMT != 0) { dtldsc += "(+) IGST @ " + Cn.Indian_Number_format(k.IGSTPER, "0.00") + " %~"; dtlamt += Convert.ToDouble(k.IGSTAMT).ToINRFormat() + "~"; }
        //                    if (k.CGSTAMT != 0) { dtldsc += "(+) CGST @ " + Cn.Indian_Number_format(k.CGSTPER, "0.00") + " %~"; dtlamt += Convert.ToDouble(k.CGSTAMT).ToINRFormat() + "~"; }
        //                    if (k.SGSTAMT != 0) { dtldsc += "(+) SGST @ " + Cn.Indian_Number_format(k.SGSTPER, "0.00") + " %~"; dtlamt += Convert.ToDouble(k.SGSTAMT).ToINRFormat() + "~"; }
        //                    tqnty = tqnty + Convert.ToDouble(k.TQNTY);
        //                    tnos = tnos + Convert.ToDouble(k.TNOS);
        //                    tamt = tamt + Convert.ToDouble(k.TAMT);
        //                    tgst = tgst + Convert.ToDouble(k.IGSTAMT) + Convert.ToDouble(k.CGSTAMT) + Convert.ToDouble(k.SGSTAMT);
        //                }
        //            }
        //            //if (tbl.Rows[0]["ADVRECDAMT"].retDbl() != 0)
        //            //{
        //            //    dtldsc += "(-) " + tbl.Rows[0]["ADVRECDREM"].retStr() + "~";
        //            //    dtlamt += tbl.Rows[0]["ADVRECDAMT"].retDbl().ToINRFormat() + "~";
        //            //}

        //            var HSN_DATA = (from a in DBF.T_VCH_GST
        //                            where a.AUTONO == auto1
        //                            group a by new { HSNCODE = a.HSNCODE, IGSTPER = a.IGSTPER, CGSTPER = a.CGSTPER, SGSTPER = a.SGSTPER } into x
        //                            select new
        //                            {

        //                                HSNCODE = x.Key.HSNCODE,
        //                                IGSTPER = x.Key.IGSTPER,
        //                                CGSTPER = x.Key.CGSTPER,
        //                                SGSTPER = x.Key.SGSTPER,
        //                                TIGSTAMT = x.Sum(s => s.IGSTAMT),
        //                                TCGSTAMT = x.Sum(s => s.CGSTAMT),
        //                                TSGSTAMT = x.Sum(s => s.SGSTAMT),
        //                                TAMT = x.Sum(s => s.AMT),
        //                                TQNTY = x.Sum(s => s.QNTY)
        //                                //DECIMAL = (from z in DBF.M_UOM
        //                                //           where z.UOMCD == (from y in DBF.T_VCH_GST where y.AUTONO == auto1 select y.UOM).FirstOrDefault()
        //                                //           select z.DECIMALS).FirstOrDefault()
        //                                //DECIMALS = (from c in DBF.M_UOM where c.UOMCD ==  select c.DECIMALS)
        //                            }).ToList();

        //            if (HSN_DATA != null && HSN_DATA.Count > 0)
        //            {
        //                foreach (var k in HSN_DATA)
        //                {
        //                    var uom = (from a in DBF.T_VCH_GST
        //                               where a.AUTONO == auto1 && a.IGSTPER == k.IGSTPER && a.CGSTPER == k.CGSTPER
        //                      && a.SGSTPER == k.SGSTPER && a.HSNCODE == k.HSNCODE
        //                               select a.UOM).FirstOrDefault();
        //                    double DECIMAL = 0; string umnm = "";
        //                    var uomdata = DBF.M_UOM.Find(uom);
        //                    DECIMAL = Convert.ToDouble(uomdata.DECIMALS);
        //                    umnm = uomdata.UOMNM;
        //                    if (k.TIGSTAMT > 0) flagi = true;
        //                    if (k.TCGSTAMT > 0) flagc = true;

        //                    gstper1 = Convert.ToDouble(k.CGSTPER) + Convert.ToDouble(k.IGSTPER);
        //                    gstamt1 = Convert.ToDouble(k.TCGSTAMT) + Convert.ToDouble(k.TIGSTAMT);

        //                    if (k.HSNCODE != "") { hsncode += k.HSNCODE + "~"; }
        //                    if (k.TQNTY != 0) { hsnqnty += Convert.ToDouble(k.TQNTY).ToString("n" + DECIMAL.ToString()) + " " + umnm + "~"; }
        //                    if (k.TCGSTAMT + k.TIGSTAMT != 0)
        //                    {
        //                        if (k.IGSTPER != 0) hsngstper1 += Cn.Indian_Number_format(k.IGSTPER.ToString(), "0.00") + " %~";
        //                        if (k.TIGSTAMT != 0) hsngstamt1 += Convert.ToDouble(k.TIGSTAMT).ToINRFormat() + "~";
        //                    }
        //                    else
        //                    {
        //                        hsngstper1 += "~";
        //                        hsngstamt1 += "~";
        //                    }
        //                    if (k.TCGSTAMT + k.TCGSTAMT != 0)
        //                    {
        //                        if (k.CGSTPER != 0) hsngstper2 += Cn.Indian_Number_format(k.CGSTPER.ToString(), "0.00") + " %~";
        //                        if (k.TCGSTAMT != 0) hsngstamt2 += Convert.ToDouble(k.TCGSTAMT).ToINRFormat() + "~";
        //                    }
        //                    else
        //                    {
        //                        hsngstper2 += "~";
        //                        hsngstamt2 += "~";
        //                    }
        //                    if (k.TSGSTAMT != 0)
        //                    {
        //                        flags = true;
        //                        if (k.SGSTPER != 0) hsngstper3 += Cn.Indian_Number_format(k.SGSTPER.ToString(), "0.00") + " %~";
        //                        if (k.TSGSTAMT != 0) hsngstamt3 += Convert.ToDouble(k.TSGSTAMT).ToINRFormat() + "~";
        //                    }
        //                    else
        //                    {
        //                        hsngstper3 += "~";
        //                        hsngstamt3 += "~";
        //                    }
        //                    if (k.TAMT != 0) { hsntaxblval += Convert.ToDouble(k.TAMT).ToINRFormat() + "~"; } else { hsntaxblval += "~"; }

        //                    total_qnty = total_qnty + Convert.ToDouble(k.TQNTY);
        //                    total_taxval = total_taxval + Convert.ToDouble(k.TAMT);
        //                    total_gstamt1 = total_gstamt1 + Convert.ToDouble(k.TIGSTAMT);
        //                    total_gstamt2 = total_gstamt2 + Convert.ToDouble(k.TCGSTAMT);
        //                    total_gstamt3 = total_gstamt3 + Convert.ToDouble(k.TSGSTAMT);
        //                }
        //            }
        //        }

        //        while (tbl.Rows[i]["autono"].ToString() == auto1)
        //        {
        //            var dchrg = (from DataRow dr in tbl.Rows
        //                         where dr["itcd"].ToString() == "" && dr["autono"].ToString() == auto1
        //                         select new
        //                         {
        //                             itrem = dr["itrem"]
        //                         }).ToList();
        //            docno = tbl.Rows[i]["docno"].ToString();
        //            if (copyno[ic].ToString() == "N")
        //            {
        //                i = i + 1;
        //                break;
        //            }
        //            switch (ic)
        //            {
        //                case 0:
        //                    copymode = "ORIGINAL FOR RECIPIENT"; break;
        //                case 1:
        //                    copymode = "DUPLICATE FOR TRANSPORTER"; break;
        //                case 2:
        //                    copymode = "TRIPLICATE FOR SUPPLIER"; break;
        //                case 3:
        //                    copymode = "EXTRA COPY"; break;
        //                case 4:
        //                    copymode = "EXTRA COPY"; break;
        //                case 5:
        //                    copymode = "EXTRA COPY"; break;
        //                default: copymode = ""; break;
        //            }

        //            DataRow dr1 = IR.NewRow();
        //            docstart:
        //            double duedays = 0;
        //            string payterms = "";
        //            duedays = tbl.Rows[i]["duedays"] == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["duedays"]);
        //            //payterms = tbl.Rows[i]["payterms"].ToString();
        //            if (payterms == "")
        //            {
        //                if (duedays == 0) payterms = ""; else payterms = duedays.ToString() + " days.";
        //            }

        //            //dr1["menu_para"] = VE.MENU_PARA;
        //            //dr1["pvtag"] = VE.Checkbox7 == true ? "Y" : "N";
        //            dr1["menu_para"] = VE.MENU_PARA;
        //            //dr1["pvtag"] = tbl.Rows[i]["pv_tag"].ToString();
        //            dr1["autono"] = auto1 + ic.ToString();
        //            dr1["usr_id"] = tbl.Rows[i]["usr_id"].ToString();
        //            dr1["cancel"] = tbl.Rows[i]["cancel"].ToString();
        //            dr1["canc_rem"] = tbl.Rows[i]["canc_rem"].ToString();
        //            dr1["copymode"] = copymode;
        //            dr1["docno"] = tbl.Rows[i]["docno"].ToString();
        //            dr1["docdt"] = tbl.Rows[i]["docdt"] == DBNull.Value ? "" : tbl.Rows[i]["docdt"].ToString().Substring(0, 10).ToString();
        //            dr1["upiimg"] = "";
        //            dr1["upidesc"] = "";
        //            //dr1["areacd"] = tbl.Rows[i]["areacd"].ToString();
        //            dr1["invisstime"] = tbl.Rows[i]["invisstime"].retDbl();
        //            dr1["duedays"] = duedays;
        //            //dr1["itmprccd"] = tbl.Rows[i]["itmprccd"].ToString();
        //            //dr1["itmprcdesc"] = tbl.Rows[i]["itmprcdesc"].ToString();
        //            //dr1["prceffdt"] = tbl.Rows[i]["prceffdt"] == DBNull.Value ? "" : tbl.Rows[i]["prceffdt"].ToString().Substring(0, 10).ToString();
        //            dr1["duedt"] = Convert.ToDateTime(tbl.Rows[i]["docdt"].ToString()).AddDays(duedays).ToString().retDateStr();
        //            dr1["packby"] = tbl.Rows[i]["packby"].retStr();
        //            dr1["selby"] = tbl.Rows[i]["selby"].retStr();
        //            dr1["dealby"] = tbl.Rows[i]["dealby"].retStr();
        //            dr1["payterms"] = payterms;
        //            //if (rsStkPrcDesc.Rows.Count > 0 && tbl.Rows[i]["itgrpcd"].ToString() == "G001" && doctotprint == false)
        //            //{
        //            //    var DATA = (from DataRow DR in rsStkPrcDesc.Rows where DR["autoitcd"].ToString() == auto1 + tbl.Rows[i]["itcd"].ToString() select DR["stkprcdesc"].ToString()).ToList();
        //            //    if (DATA.Count > 0) dr1["stkprcdesc"] = DATA[0];
        //            //}
        //            dr1["gocd"] = tbl.Rows[i]["gocd"].ToString();
        //            dr1["gonm"] = tbl.Rows[i]["gonm"].ToString();
        //            dr1["goadd1"] = tbl.Rows[i]["goadd1"].ToString();
        //            //dr1["weekno"] = tbl.Rows[i]["weekno"] == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["weekno"]);

        //            dr1["slcd"] = tbl.Rows[i]["slcd"].ToString();
        //            //if (tbl.Rows[i]["partycd"].ToString() != "") dr1["partycd"] = "SAP - " + tbl.Rows[i]["partycd"].ToString();
        //            dr1["slnm"] = tbl.Rows[i]["slnm"].ToString();
        //            dr1["regemailid"] = tbl.Rows[i]["regemailid"].ToString();
        //            string cfld = "", rfld = ""; int rf = 0;
        //            for (int f = 1; f <= 6; f++)
        //            {
        //                cfld = "sladd" + Convert.ToString(f).ToString();
        //                if (tbl.Rows[i][cfld].ToString() != "")
        //                {
        //                    rf = rf + 1;
        //                    rfld = "sladd" + Convert.ToString(rf);
        //                    dr1[rfld] = tbl.Rows[i][cfld].ToString();
        //                }
        //            }
        //            rf = rf + 1;
        //            rfld = "sladd" + Convert.ToString(rf);
        //            dr1[rfld] = tbl.Rows[i]["state"].ToString() + " [ Code - " + tbl.Rows[i]["statecd"].ToString() + " ]";
        //            if (tbl.Rows[i]["gstno"].ToString() != "")
        //            {
        //                rf = rf + 1;
        //                rfld = "sladd" + Convert.ToString(rf);
        //                dr1[rfld] = "GST # " + tbl.Rows[i]["gstno"].ToString();
        //            }
        //            if (tbl.Rows[i]["panno"].ToString() != "")
        //            {
        //                rf = rf + 1;
        //                rfld = "sladd" + Convert.ToString(rf);
        //                dr1[rfld] = "PAN # " + tbl.Rows[i]["panno"].ToString();
        //            }
        //            if (tbl.Rows[i]["phno"].ToString() != "")
        //            {
        //                rf = rf + 1;
        //                rfld = "sladd" + Convert.ToString(rf);
        //                dr1[rfld] = "Ph. # " + tbl.Rows[i]["phno"].ToString();
        //            }
        //            if (tbl.Rows[i]["slactnameof"].ToString() != "")
        //            {
        //                rf = rf + 1;
        //                rfld = "sladd" + Convert.ToString(rf);
        //                dr1[rfld] = tbl.Rows[i]["slactnameof"].ToString();
        //            }
        //            // Consignee
        //            cfld = ""; rfld = ""; rf = 0;
        //            bool conslcdprn = true;
        //            if (tbl.Rows[i]["cslcd"].ToString() == tbl.Rows[i]["slcd"].ToString() && tbl.Rows[i]["othadd1"].ToString() != "") conslcdprn = false;

        //            if (conslcdprn == true)
        //            {
        //                dr1["cslcd"] = tbl.Rows[i]["cslcd"].ToString();
        //                dr1["cpartycd"] = tbl.Rows[i]["cpartycd"].ToString();
        //                dr1["cslnm"] = tbl.Rows[i]["cslnm"].ToString();
        //                for (int f = 1; f <= 6; f++)
        //                {
        //                    cfld = "csladd" + Convert.ToString(f).ToString();
        //                    if (tbl.Rows[i][cfld].ToString() != "")
        //                    {
        //                        rf = rf + 1;
        //                        rfld = "csladd" + Convert.ToString(rf);
        //                        dr1[rfld] = tbl.Rows[i][cfld].ToString();
        //                    }
        //                }
        //                rf = rf + 1;
        //                rfld = "csladd" + Convert.ToString(rf);
        //                dr1[rfld] = tbl.Rows[i]["cstate"].ToString() + " [ Code - " + tbl.Rows[i]["cstatecd"].ToString() + " ]";
        //                if (tbl.Rows[i]["cgstno"].ToString() != "")
        //                {
        //                    rf = rf + 1;
        //                    rfld = "csladd" + Convert.ToString(rf);
        //                    dr1[rfld] = "GST # " + tbl.Rows[i]["cgstno"].ToString();
        //                }
        //                if (tbl.Rows[i]["cpanno"].ToString() != "")
        //                {
        //                    rf = rf + 1;
        //                    rfld = "csladd" + Convert.ToString(rf);
        //                    dr1[rfld] = "PAN # " + tbl.Rows[i]["cpanno"].ToString();
        //                }
        //                if (tbl.Rows[i]["cphno"].ToString() != "")
        //                {
        //                    rf = rf + 1;
        //                    rfld = "csladd" + Convert.ToString(rf);
        //                    dr1[rfld] = "Ph. # " + tbl.Rows[i]["cphno"].ToString();
        //                }
        //                if (tbl.Rows[i]["cslactnameof"].ToString() != "")
        //                {
        //                    rf = rf + 1;
        //                    rfld = "csladd" + Convert.ToString(rf);
        //                    dr1[rfld] = tbl.Rows[i]["cslactnameof"].ToString();
        //                }
        //            }
        //            else if (tbl.Rows[i]["othadd1"].ToString() != "")
        //            {
        //                dr1["cslcd"] = ""; tbl.Rows[i]["slcd"].ToString();
        //                dr1["cpartycd"] = ""; tbl.Rows[i]["slcd"].ToString();
        //                dr1["cslnm"] = tbl.Rows[i]["othnm"] == DBNull.Value ? tbl.Rows[i]["slnm"].ToString() : tbl.Rows[i]["othnm"].ToString();
        //                for (int f = 1; f <= 3; f++)
        //                {
        //                    cfld = "othadd" + Convert.ToString(f).ToString();
        //                    if (tbl.Rows[i][cfld].ToString() != "")
        //                    {
        //                        rf = rf + 1;
        //                        rfld = "csladd" + Convert.ToString(rf);
        //                        dr1[rfld] = tbl.Rows[i][cfld].ToString();
        //                    }
        //                }
        //                rf = rf + 1;
        //                rfld = "csladd" + Convert.ToString(rf);
        //                dr1[rfld] = tbl.Rows[i]["cstate"].ToString() + " [ Code - " + tbl.Rows[i]["cstatecd"].ToString() + " ]";
        //                if (tbl.Rows[i]["cgstno"].ToString() != "")
        //                {
        //                    rf = rf + 1;
        //                    rfld = "csladd" + Convert.ToString(rf);
        //                    dr1[rfld] = "GST # " + tbl.Rows[i]["cgstno"].ToString();
        //                }
        //                if (tbl.Rows[i]["cpanno"].ToString() != "")
        //                {
        //                    rf = rf + 1;
        //                    rfld = "csladd" + Convert.ToString(rf);
        //                    dr1[rfld] = "PAN # " + tbl.Rows[i]["cpanno"].ToString();
        //                }
        //            }

        //            dr1["porefno"] = tbl.Rows[i]["porefno"].ToString();
        //            dr1["porefdt"] = tbl.Rows[i]["porefdt"] == DBNull.Value ? "" : tbl.Rows[i]["porefdt"].retDateStr();
        //            dr1["trslcd"] = tbl.Rows[i]["trslcd"].ToString();
        //            dr1["trslnm"] = tbl.Rows[i]["trslnm"].ToString();
        //            dr1["trsladd1"] = tbl.Rows[i]["trsladd1"].ToString();
        //            dr1["trsladd2"] = tbl.Rows[i]["trsladd2"].ToString();
        //            dr1["trsladd3"] = tbl.Rows[i]["trsladd3"].ToString();
        //            dr1["trsladd4"] = tbl.Rows[i]["trslphno"].ToString();
        //            dr1["trgst"] = tbl.Rows[i]["trgst"].ToString();
        //            dr1["lrno"] = tbl.Rows[i]["lrno"].ToString();
        //            dr1["lrdt"] = tbl.Rows[i]["lrdt"] == DBNull.Value ? "" : tbl.Rows[i]["lrdt"].retDateStr(); /*tbl.Rows[i]["lrdt"].ToString().Substring(0, 10).ToString();*/
        //            dr1["lorryno"] = tbl.Rows[i]["lorryno"].ToString();
        //            dr1["ewaybillno"] = tbl.Rows[i]["ewaybillno"].ToString();
        //            dr1["grwt"] = tbl.Rows[i]["grwt"] == DBNull.Value ? 0 : tbl.Rows[i]["grwt"].retDbl();
        //            dr1["ntwt"] = tbl.Rows[i]["ntwt"] == DBNull.Value ? 0 : tbl.Rows[i]["ntwt"].retDbl();
        //            dr1["agentnm"] = tbl.Rows[i]["agslnm"].ToString();

        //            dr1["revchrg"] = "N";
        //            dr1["roamt"] = tbl.Rows[i]["roamt"] == DBNull.Value ? 0 : tbl.Rows[i]["roamt"].retDbl();
        //            dr1["tcsper"] = tbl.Rows[i]["tcsper"].ToString().retDbl().ToINRFormat();
        //            dr1["tcsamt"] = tbl.Rows[i]["tcsamt"].ToString().retDbl().ToINRFormat(); // == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["tcsamt"]);
        //            dr1["blamt"] = tbl.Rows[i]["blamt"].ToString().retDbl().ToINRFormat(); // == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["blamt"]);

        //            //dr1["blamt"] = (tbl.Rows[i]["blamt"].retDbl() - tbl.Rows[i]["ADVRECDAMT"].retDbl()).ToINRFormat();

        //            dr1["rupinword"] = rupinwords;
        //            dr1["agdocno"] = tbl.Rows[i]["agdocno"].ToString();
        //            dr1["agdocdt"] = tbl.Rows[i]["agdocdt"] == DBNull.Value ? "" : tbl.Rows[i]["agdocdt"].ToString().Substring(0, 10).ToString();
        //            blrem = "";
        //            //if (tbl.Rows[i]["sapopdno"].ToString() != "") blrem = blrem + "ODP No. " + tbl.Rows[i]["sapopdno"].ToString() + "  ";
        //            //if (tbl.Rows[i]["sapblno"].ToString() != "") blrem = blrem + "SAP Bill # " + tbl.Rows[i]["sapblno"].ToString() + "  ";
        //            //if (tbl.Rows[i]["sapshipno"].ToString() != "") blrem = blrem + "SAP Shipment # " + tbl.Rows[i]["sapshipno"].ToString() + "  ";
        //            if (tbl.Rows[i]["docrem"].ToString() != "") blrem = blrem + tbl.Rows[i]["docrem"].ToString() + "  ";
        //            dr1["docth"] = tbl.Rows[i]["docth"];
        //            //dr1["nopkgs"] = tbl.Rows[i]["nopkgs"];
        //            dr1["blremarks"] = blrem;

        //            //dr1["precarr"] = tbl.Rows[i]["precarr"];
        //            //dr1["precarrrecpt"] = tbl.Rows[i]["precarrrecpt"];
        //            //dr1["shipmarkno"] = tbl.Rows[i]["shipmarkno"];
        //            //dr1["portload"] = tbl.Rows[i]["portload"];
        //            //dr1["portdesc"] = tbl.Rows[i]["portdesc"];
        //            //dr1["finaldest"] = tbl.Rows[i]["finaldest"];
        //            //dr1["bankinter"] = tbl.Rows[i]["bankinter"];

        //            //Bank Detals
        //            dr1["bankactno"] = bankactno;
        //            dr1["bankname"] = bankname;
        //            dr1["bankifsc"] = bankifsc;
        //            dr1["bankbranch"] = bankbranch;
        //            dr1["bankadd"] = bankadd;
        //            dr1["bankrtgs"] = bankrtgs;

        //            dr1["dtldsc"] = dtldsc;
        //            dr1["dtlamt"] = dtlamt;
        //            dr1["curr_cd"] = tbl.Rows[i]["curr_cd"].ToString();
        //            dr1["hsn_cd"] = hsncode;

        //            if (flagi == true)
        //            {
        //                dr1["hsn_hddsp1"] = "IGST";
        //            }
        //            else
        //            {
        //                if (flagc == true)
        //                {
        //                    dr1["hsn_hddsp1"] = "CGST";
        //                }
        //                else
        //                {
        //                    dr1["hsn_hddsp1"] = "";
        //                }
        //            }
        //            dr1["hsn_hddsp2"] = flags == true ? "SGST" : "";
        //            dr1["hsn_txblval"] = hsntaxblval;
        //            dr1["hsn_gstper1"] = hsngstper1;
        //            dr1["hsn_gstamt1"] = hsngstamt1;
        //            dr1["hsn_gstper2"] = hsngstper2;
        //            dr1["hsn_gstamt2"] = hsngstamt2;
        //            dr1["hsn_gstper3"] = hsngstper3;
        //            dr1["hsn_gstamt3"] = hsngstamt3;
        //            dr1["hsn_cessamt"] = "";
        //            dr1["hsn_gstamt"] = "";
        //            dr1["hsn_qnty"] = hsnqnty;
        //            dr1["hsn_tqnty"] = total_qnty;
        //            dr1["hsn_ttxblval"] = total_taxval.ToINRFormat();
        //            dr1["hsn_tgstamt1"] = total_gstamt1.ToINRFormat();
        //            dr1["hsn_tgstamt2"] = total_gstamt2.ToINRFormat();
        //            dr1["hsn_tgstamt3"] = total_gstamt3.ToINRFormat();
        //            if (totalosamt != 0) dr1["totalosamt"] = totalosamt.ToINRFormat();
        //            //dr1["destn"] = tbl.Rows[i]["destn"];
        //            //dr1["plsupply"] = tbl.Rows[i]["plsupply"];
        //            //dr1["poslno"] = tbl.Rows[i]["poslno"];
        //            //dr1["mtrlcd"] = tbl.Rows[i]["mtrlcd"];
        //            //dr1["bas_rate"] = Convert.ToDouble(tbl.Rows[i]["bas_rate"] == DBNull.Value ? 0 : tbl.Rows[i]["bas_rate"]).ToString("0.00");
        //            //dr1["pv_per"] = tbl.Rows[i]["pv_per"].ToString() == "" ? "" : tbl.Rows[i]["pv_per"].ToString() + " %";
        //            //if (tbl.Rows[i]["insby"].ToString().retStr() == "Y") dr1["insudesc"] = inspoldesc;
        //            dr1["dealsin"] = dealsin;
        //            dr1["blterms"] = blterms;

        //            if (doctotprint == false)
        //            {
        //                itdsc = "";
        //                if (tbl.Rows[i]["itcd"].ToString() != "")
        //                {
        //                    lslno = lslno + 1;
        //                    delvchrg = false;
        //                }
        //                else
        //                {
        //                    lslno = 0;
        //                    delvchrg = true;
        //                }
        //                if (tbl.Rows[i]["itrem"].ToString() != "") itdsc = tbl.Rows[i]["itrem"].ToString();
        //                //if (Convert.ToDouble(tbl.Rows[i]["nosinbag"]) != 0)
        //                //{
        //                //    double dbnopcks = Convert.ToDouble(tbl.Rows[i]["nosinbag"]) * Convert.ToDouble(tbl.Rows[i]["nos"]);
        //                //    itdsc = itdsc + "CLD: " + Convert.ToDouble(tbl.Rows[i]["nosinbag"]).ToString("0") + " NOPCKS: " + dbnopcks.ToString();
        //                //}
        //                if (itdsc == "" && CommVar.ClientCode(UNQSNO) == "RATN") itdsc = tbl.Rows[i]["itnm"].ToString();
        //                //if (tbl.Rows[i]["prodcd"].ToString() != "") itdsc = itdsc + "PCD: " + tbl.Rows[i]["prodcd"].ToString() + " ";
        //                if (tbl.Rows[i]["batchdlprint"].ToString() == "Y" && tbl.Rows[i]["batchdtl"].ToString() != "") itdsc += "Batch # " + tbl.Rows[i]["batchdtl"].ToString(); else dr1["batchdtl"] = "";
        //                if (tbl.Rows[i]["itcd"].ToString() != "") dr1["caltype"] = 1; else dr1["caltype"] = 0;
        //                dr1["agdocno"] = tbl.Rows[i]["agdocno"].ToString();
        //                dr1["agdocdt"] = tbl.Rows[i]["agdocdt"] == DBNull.Value ? "" : tbl.Rows[i]["agdocdt"].ToString().Substring(0, 10).ToString();
        //                dr1["slno"] = lslno;
        //                dr1["itcd"] = tbl.Rows[i]["itcd"].ToString();
        //                //dr1["prodcd"] = tbl.Rows[i]["prodcd"].ToString();
        //                dr1["itnm"] = tbl.Rows[i]["itnm"].ToString() + " " + tbl.Rows[i]["styleno"].ToString();
        //                //if (tbl.Rows[i]["damstock"].ToString() == "D")
        //                //{
        //                //    dr1["itnm"] = dr1["itnm"].ToString() + " [Damage]";
        //                //}
        //                dr1["itdesc"] = itdsc;
        //                //dr1["bltophead"] = tbl.Rows[i]["bltophead"].ToString();
        //                //dr1["makenm"] = tbl.Rows[i]["makenm"].ToString();
        //                //dr1["mrp"] = tbl.Rows[i]["mrp"];
        //                if (tbl.Rows[i]["batchdlprint"].ToString() == "Y" && tbl.Rows[i]["batchdtl"].ToString() != "") dr1["batchdtl"] = "Batch # " + tbl.Rows[i]["batchdtl"].ToString(); else dr1["batchdtl"] = "";
        //                dr1["nos"] = tbl.Rows[i]["nos"].ToString();
        //                dr1["hsncode"] = tbl.Rows[i]["hsncode"].ToString();
        //                //dr1["packsize"] = tbl.Rows[i]["packsize"] == DBNull.Value ? 0 : (tbl.Rows[i]["packsize"]).retDbl();
        //                dr1["nos"] = tbl.Rows[i]["nos"] == DBNull.Value ? 0 : (tbl.Rows[i]["nos"]).retDbl();
        //                dr1["qnty"] = tbl.Rows[i]["qnty"] == DBNull.Value ? 0 : (tbl.Rows[i]["qnty"]).retDbl();
        //                uomdecimal = tbl.Rows[i]["qdecimal"] == DBNull.Value ? 0 : Convert.ToInt16(tbl.Rows[i]["qdecimal"]);
        //                string dbqtyu = string.Format("{0:N6}", (dr1["qnty"]).retDbl());
        //                if (dbqtyu.Substring(dbqtyu.Length - 2, 2) == "00")
        //                {
        //                    if (uomdecimal > 4) uomdecimal = 4;
        //                }
        //                if (uomdecimal > uommaxdecimal) uommaxdecimal = uomdecimal;
        //                if (VE.DOCCD == "SOOS" && uomdecimal == 6) uomdecimal = 4;

        //                dr1["qdecimal"] = uomdecimal;
        //                dr1["uomnm"] = tbl.Rows[i]["uomnm"].ToString();
        //                dr1["rate"] = tbl.Rows[i]["rate"].retDbl().ToString("0.00");
        //                dr1["amt"] = tbl.Rows[i]["amt"] == DBNull.Value ? 0 : (tbl.Rows[i]["amt"]).retDbl();
        //                //dr1["poslno"] = tbl.Rows[i]["poslno"];
        //                //dr1["mtrlcd"] = tbl.Rows[i]["mtrlcd"];
        //                dr1["curr_cd"] = tbl.Rows[i]["curr_cd"].ToString();
        //                //dr1["bas_rate"] = (tbl.Rows[i]["bas_rate"] == DBNull.Value ? 0 : tbl.Rows[i]["bas_rate"]).retDbl().ToString("0.00");
        //                //dr1["pv_per"] = tbl.Rows[i]["pv_per"].ToString() == "" ? "" : tbl.Rows[i]["pv_per"].ToString() + " %"; if (tbl.Rows[i]["rateqntybag"].ToString() == "B") dr1["rateuomnm"] = "Case"; else dr1["rateuomnm"] = dr1["uomnm"];
        //                string strdsc = "";
        //                if (tbl.Rows[i]["tddiscamt"].retDbl() != 0)
        //                {
        //                    switch (tbl.Rows[i]["tddisctype"].ToString())
        //                    {
        //                        case "Q":

        //                            strdsc = ""; break;
        //                        case "N":

        //                            strdsc = ""; break;
        //                        case "F":
        //                            strdsc = "F"; break;
        //                        default:
        //                            dr1["discper"] = (tbl.Rows[i]["tddiscrate"]).retDbl();
        //                            strdsc = (tbl.Rows[i]["tddiscrate"]).retDbl().ToString("0.00") + "%"; break;
        //                    }
        //                }
        //                dr1["stddisc"] = strdsc;
        //                dr1["tddiscamt"] = (tbl.Rows[i]["tddiscamt"]).retDbl();
        //                if ((tbl.Rows[i]["discamt"]).retDbl() != 0)
        //                {
        //                    switch (tbl.Rows[i]["disctype"].ToString())
        //                    {
        //                        case "Q":
        //                            strdsc = "Q" + (tbl.Rows[i]["discrate"]).retDbl().ToString("0.00"); break;
        //                        case "B":
        //                            strdsc = "B" + (tbl.Rows[i]["discrate"]).retDbl().ToString("0.00"); break;
        //                        case "F":
        //                            strdsc = "F"; break;
        //                        case "P":
        //                            dr1["discper"] = (tbl.Rows[i]["discrate"]).retDbl();
        //                            strdsc = (tbl.Rows[i]["discrate"]).retDbl().ToString("0.00") + "%"; break;
        //                    }
        //                }
        //                dr1["disc"] = strdsc;
        //                dr1["titdiscamt"] = (tbl.Rows[i]["discamt"]).retDbl() + (tbl.Rows[i]["tddiscamt"]).retDbl();
        //                dr1["discamt"] = (tbl.Rows[i]["discamt"]).retDbl();
        //                dr1["txblval"] = ((tbl.Rows[i]["amt"]).retDbl() - (tbl.Rows[i]["tddiscamt"]).retDbl() - (tbl.Rows[i]["discamt"]).retDbl()).ToINRFormat();

        //                dr1["cgstdsp"] = flagi == true ? "IGST" : "CGST";
        //                dr1["sgstdsp"] = flagc == true ? "SGST" : "";
        //                dr1["cgstper"] = (tbl.Rows[i]["cgstper"]).retDbl() + (tbl.Rows[i]["igstper"]).retDbl();
        //                dr1["cgstamt"] = (tbl.Rows[i]["cgstamt"]).retDbl() + (tbl.Rows[i]["igstamt"]).retDbl();
        //                dr1["sgstper"] = (tbl.Rows[i]["sgstper"]).retDbl();
        //                dr1["sgstamt"] = (tbl.Rows[i]["sgstamt"]).retDbl();
        //                dr1["cessper"] = (tbl.Rows[i]["cessper"]).retDbl();
        //                dr1["cessamt"] = (tbl.Rows[i]["cessamt"]).retDbl();
        //                dr1["gstper"] = (tbl.Rows[i]["gstper"]).retDbl();
        //                dr1["netamt"] = (dr1["txblval"].ToString()).retDbl() + (dr1["cgstamt"].ToString()).retDbl() + (dr1["sgstamt"].ToString()).retDbl() + (dr1["cessamt"].ToString()).retDbl();
        //                //totals
        //                dnos = dnos + (dr1["nos"].ToString()).retDbl();
        //                dqnty = dqnty + (dr1["qnty"].ToString()).retDbl();
        //                dbasamt = dbasamt + (dr1["amt"].ToString()).retDbl();
        //                ddisc1 = ddisc1 + (dr1["tddiscamt"].ToString()).retDbl();
        //                ddisc2 = ddisc2 + (dr1["discamt"].ToString()).retDbl();
        //                dtxblval = dtxblval + (dr1["txblval"].ToString()).retDbl();
        //                //if (VE.TEXTBOX6 == "SaleBill_rec.rpt")
        //                //{
        //                //    if (tbl.Rows[0]["ADVRECDAMT"].retDbl() != 0)
        //                //    { dr1["blamt"] = (dtxblval - tbl.Rows[i]["ADVRECDAMT"].retDbl()).ToINRFormat(); }
        //                //}
        //                dcgstamt = dcgstamt + (dr1["cgstamt"].ToString()).retDbl();
        //                dsgstamt = dsgstamt + (dr1["sgstamt"].ToString()).retDbl();
        //                dnetamt = dnetamt + (dr1["netamt"].ToString()).retDbl();
        //            }
        //            IR.Rows.Add(dr1);

        //            if (totalreadyprint == false)
        //            {
        //                if (i == maxR) doctotprint = true;
        //                else if (tbl.Rows[i + 1]["autono"].ToString() != auto1) doctotprint = true;
        //                else if (tbl.Rows[i + 1]["itcd"].ToString() == "") doctotprint = true;
        //            }
        //            if (delvchrg == true)
        //            {
        //                doctotprint = true; totalreadyprint = false; delvchrg = false;
        //            }
        //            if (CommVar.ClientCode(UNQSNO) == "RATN")
        //            {
        //                if (dchrg != null && dchrg.Count() > 0)
        //                {
        //                    if (tbl.Rows[i]["itcd"].ToString() == "")
        //                    {
        //                        if (doctotprint == true && totalreadyprint == false)
        //                        {
        //                            dr1 = IR.NewRow();
        //                            dr1["menu_para"] = VE.MENU_PARA;
        //                            dr1["autono"] = auto1 + copymode;
        //                            dr1["copymode"] = copymode;
        //                            dr1["docno"] = tbl.Rows[i]["docno"].ToString();
        //                            if (CommVar.ClientCode(UNQSNO) == "RATN")
        //                            {
        //                                dr1["itdesc"] = "Total";
        //                            }
        //                            else
        //                            {
        //                                dr1["itnm"] = "Total";
        //                            }
        //                            dr1["nos"] = dnos;
        //                            dr1["qnty"] = dqnty;
        //                            if (VE.DOCCD == "SOOS" && uommaxdecimal == 6) uommaxdecimal = 4;
        //                            dr1["qdecimal"] = uommaxdecimal;
        //                            dr1["amt"] = dbasamt;
        //                            dr1["tddiscamt"] = ddisc1;
        //                            dr1["discamt"] = ddisc2;
        //                            dr1["txblval"] = dtxblval.ToINRFormat();
        //                            dr1["cgstamt"] = dcgstamt;
        //                            dr1["sgstamt"] = dsgstamt;
        //                            dr1["netamt"] = dnetamt;
        //                            dr1["titdiscamt"] = ddisc1 + ddisc2;
        //                            dr1["curr_cd"] = tbl.Rows[i]["curr_cd"].ToString();
        //                            totalreadyprint = true;
        //                            goto docstart;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (doctotprint == true && totalreadyprint == false)
        //                    {
        //                        dr1 = IR.NewRow();
        //                        dr1["menu_para"] = VE.MENU_PARA;
        //                        dr1["autono"] = auto1 + copymode;
        //                        dr1["copymode"] = copymode;
        //                        dr1["docno"] = tbl.Rows[i]["docno"].ToString();

        //                        if (CommVar.ClientCode(UNQSNO) == "RATN")
        //                        {
        //                            dr1["itdesc"] = "Total";
        //                        }
        //                        else
        //                        {
        //                            dr1["itnm"] = "Total";
        //                        }
        //                        dr1["nos"] = dnos;
        //                        dr1["qnty"] = dqnty;
        //                        if (VE.DOCCD == "SOOS" && uommaxdecimal == 6) uommaxdecimal = 4;
        //                        dr1["qdecimal"] = uommaxdecimal;
        //                        dr1["amt"] = dbasamt;
        //                        dr1["tddiscamt"] = ddisc1;
        //                        dr1["discamt"] = ddisc2;
        //                        dr1["txblval"] = dtxblval.ToINRFormat();
        //                        dr1["cgstamt"] = dcgstamt;
        //                        dr1["sgstamt"] = dsgstamt;
        //                        dr1["netamt"] = dnetamt;
        //                        dr1["titdiscamt"] = ddisc1 + ddisc2;
        //                        dr1["curr_cd"] = tbl.Rows[i]["curr_cd"].ToString();
        //                        totalreadyprint = true;
        //                        goto docstart;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (doctotprint == true && totalreadyprint == false)
        //                {
        //                    dr1 = IR.NewRow();
        //                    dr1["autono"] = auto1 + copymode;
        //                    dr1["copymode"] = copymode;
        //                    dr1["docno"] = tbl.Rows[i]["docno"].ToString();

        //                    dr1["itnm"] = "Total";
        //                    if (CommVar.ClientCode(UNQSNO) == "RATN") dr1["itdesc"] = "Total";
        //                    dr1["nos"] = dnos;
        //                    dr1["qnty"] = dqnty;
        //                    if (VE.DOCCD == "SOOS" && uommaxdecimal == 6) uommaxdecimal = 4;
        //                    dr1["qdecimal"] = uommaxdecimal;
        //                    dr1["amt"] = dbasamt;
        //                    dr1["tddiscamt"] = ddisc1;
        //                    dr1["discamt"] = ddisc2;
        //                    dr1["txblval"] = dtxblval.ToINRFormat();
        //                    dr1["cgstamt"] = dcgstamt;
        //                    dr1["sgstamt"] = dsgstamt;
        //                    dr1["netamt"] = dnetamt;
        //                    dr1["titdiscamt"] = ddisc1 + ddisc2;
        //                    dr1["curr_cd"] = tbl.Rows[i]["curr_cd"].ToString();
        //                    totalreadyprint = true;
        //                    goto docstart;
        //                }
        //            }
        //            doctotprint = false;
        //            i = i + 1;
        //            ilast = i;
        //            if (i > maxR) break;
        //        }
        //        i = ilast;
        //    }
        //}


        //string compaddress; string stremail = "";
        //compaddress = Salesfunc.retCompAddress(gocd, grpemailid);
        //stremail = compaddress.retCompValue("email");

        //string ccemail = grpemailid;
        //if (ccemail == "") ccemail = stremail;

        ////ReportDocument reportdocument = new ReportDocument();
        //string complogo = Salesfunc.retCompLogo();
        //EmailControl EmailControl = new EmailControl();

        //string complogosrc = complogo;
        //string compfixlogosrc = "c:\\improvar\\" + CommVar.Compcd(UNQSNO) + "fix.jpg";
        //string sendemailids = "";
        //string rptfile = "SaleBill.rpt";
        //if (VE.TEXTBOX6 != null) rptfile = VE.TEXTBOX6;
        //rptname = "~/Report/" + rptfile; // "SaleBill.rpt";
        //if (VE.maxdate == "CHALLAN") blhead = "CHALLAN";
        //  }
        public ActionResult ReportCashMemoPrint(ReportViewinHtml VE, string fdate, string tdate, string fdocno, string tdocno, string COM, string LOC, string yr_cd, string slcd, string doccd, string prnemailid, int maxR, string blhead, string gocd, string grpemailid, string Scm1, string Scmf, string scmI, string[] copyno, string rptname, string printemail, string docnm)
        {
            try
            {
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                string str1 = "";
                DataTable rsTmp;
                string doctype = "";
                str1 = "select doctype from " + Scm1 + ".m_doctype where doccd='" + VE.DOCCD + "'";
                rsTmp = masterHelp.SQLquery(str1);
                doctype = rsTmp.Rows[0]["doctype"].ToString();
                string sql = "";
                string sqlc = "";
                sqlc += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and c.yr_cd='" + yr_cd + "' and ";
                if (fdocno != "") sqlc += "c.doconlyno >= " + fdocno + " and c.doconlyno <= " + tdocno + " and ";
                if (fdate != "") sqlc += "c.docdt >= to_date('" + fdate + "','dd/mm/yyyy') and ";
                if (tdate != "") sqlc += "c.docdt <= to_date('" + tdate + "','dd/mm/yyyy') and ";
                if (slcd != null) sqlc += "b.slcd='" + slcd + "' and ";
                sqlc += "c.doccd = '" + doccd + "' and ";

                //sql += "select a.autono, b.itgrpcd, b.doctag, h.doccd, h.docno, h.docdt, b.areacd, b.invisstime, b.duedays,d.ADVRECDREM,d.ADVRECDAMT, h.canc_rem, h.cancel, ";
                //sql += "a.itmprccd, o.mrp, l.prcdesc itmprcdesc, l.effdt prceffdt, ";
                //sql += "b.gocd, k.gonm, k.goadd1, k.goadd2, k.goadd3, k.gophno, k.goemail, k.fssailicno, ";
                //sql += "b.weekno, j.batchdlprint, j.grpemailid, h.usr_id, h.usr_entdt, h.vchrno, ";

                //sql += "nvl(e.pslcd,e.slcd) oslcd, j.debglcd, ";
                //sql += "b.slcd, z.sapcd partycd, nvl(e.fullname,e.slnm) slnm, " + prnemailid + ", e.add1 sladd1, e.add2 sladd2, e.add3 sladd3, e.add4 sladd4, e.add5 sladd5, e.add6 sladd6, e.add7  ";
                //sql += "sladd7, e.gstno, e.panno, trim(e.regmobile||decode(e.regmobile,null,'',',')||e.slphno||decode(e.phno1,null,'',','||e.phno1)) phno, e.state, e.country, e.statecd,e.actnameof slactnameof,";
                //sql += "nvl(b.conslcd,b.slcd) cslcd, '' cpartycd, nvl(f.fullname,f.slnm) cslnm, f.add1 csladd1, f.add2 csladd2, f.add3 csladd3, f.add4 csladd4, f.add5 csladd5, ";
                //sql += "f.add6 csladd6, f.add7 csladd7, nvl(f.gstno,f.gstno) cgstno, nvl(f.panno,f.panno) cpanno,f.actnameof cslactnameof, ";
                //sql += "trim(f.regmobile||decode(f.regmobile,null,'',',')||f.slphno||decode(f.phno1,null,'',','||f.phno1)) cphno, ";
                //sql += "f.state cstate, f.statecd cstatecd, ";

                //sql += "b.porefno, b.porefdt, c.translcd trslcd, g.slnm trslnm, g.gstno trgst, g.add1 trsladd1, g.add2 trsladd2, g.add3 trsladd3, g.add4 trsladd4, g.phno1 trslphno, ";
                //sql += "c.lrno, c.lrdt, c.lorryno, c.ewaybillno, c.grwt, c.ntwt, ";
                //sql += "a.slno, a.itcd, a.itnm, a.itrem, a.damstock, a.batchdtl, a.packsize, a.prodcd, a.nosinbag, a.hsncode, a.nos, a.qnty, nvl(i.decimals,0) qdecimal, i.uomnm,  ";
                //sql += "a.rate, a.rateqntybag, a.basamt, d.sapopdno, d.sapblno, d.sapshipno, d.docrem, d.docth, d.nopkgs, d.agslcd, m.slnm agslnm, ";
                //sql += "a.tddisctype, a.tddiscrate, nvl(a.tddiscamt,0)tddiscamt, a.disctype, a.discrate, a.discamt, a.agdocno, a.agdocdt, nvl(d.bltophead,a.bltophead) bltophead, a.makenm, ";
                //sql += "nvl(a.igstper,0)igstper, nvl(a.igstamt,0)igstamt, nvl(a.cgstper,0)cgstper, nvl(a.cgstamt,0)cgstamt, nvl(a.sgstper,0)sgstper, nvl(a.sgstamt,0)sgstamt, nvl(a.dutyper,0)dutyper, nvl(a.dutyamt,0)dutyamt, nvl(a.cessper,0)cessper, nvl(a.cessamt,0)cessamt, nvl(a.igstper+a.cgstper+a.sgstper,0) gstper,  ";
                //sql += "nvl(b.roamt,0)roamt, nvl(b.blamt,0) blamt, nvl(b.tcsper,0) tcsper, nvl(b.tcsamt,0) tcsamt, d.insby, d.payterms,d.precarr,d.precarrrecpt,d.shipmarkno,d.portload,d.portdesc,d.finaldest,d.bankinter,d.insudesc, ";
                //sql += "d.othnm, nvl(d.othadd1,f.othadd1) othadd1, decode(d.othadd1,null,f.othadd2,d.othadd2) othadd2, decode(d.othadd1,null,f.othadd3,d.othadd3) othadd3, decode(d.othadd1,null,f.othadd4,d.othadd4) othadd4, ";
                //sql += " nvl(a.mtrlcd,n.mtrlcd) mtrlcd, nvl(a.poslno,n.poslno) poslno, nvl(d.plsupply,nvl(f.state,e.state)) plsupply, nvl(d.destn,e.district) destn, y.bas_rate, y.pv_per, decode(nvl(y.pv_per,0),0,'N','Y') pv_tag,b.curr_cd from ( ";

                //sql += "select a.autono, a.autono||a.slno autoslno, a.ordautoslno, nvl(a.itmprccd,b.itmprccd) itmprccd, a.slno, a.itcd, d.itnm, ";
                //sql += "nvl(a.bluomcd,d.uomcd) uomcd, a.damstock, nvl(a.hsncode,nvl(d.hsncode,e.hsncode)) hsncode, a.itrem, d.prodcd, d.packsize, d.nosinbag, ";
                //sql += "a.nos, nvl(a.blqnty,a.qnty) qnty, a.rate, nvl(e.rateqntybag,f.rateqntybag) rateqntybag, a.basamt, a.tddisctype, a.tddiscrate, a.tddiscamt, a.disctype, a.discrate, a.discamt, ";
                //sql += "a.agdocno, to_char(a.agdocdt, 'dd/mm/yyyy') agdocdt, b.slcd||b.itgrpcd||c.compcd||c.loccd slitgrpcd, e.bltophead, g.makenm, ";
                //sql += "listagg(o.batchno || ' (' || n.qnty || ')', ', ') within group (order by n.autono, n.slno, n.batchautono) batchdtl, ";
                //sql += "a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.dutyper, a.dutyamt, a.cessper, a.cessamt,a.mtrlcd, a.poslno  ";
                //sql += "from " + Scm1 + ".t_txndtl a, " + Scm1 + ".t_txn b, " + Scm1 + ".t_cntrl_hdr c, " + Scm1 + ".m_sitem d, " + Scm1 + ".m_brgrp e, " + Scm1 + ".m_group f, " + Scm1 + ".m_make g, ";
                //sql += Scm1 + ".t_batchdtl  n, " + Scm1 + ".t_batchmst o ";
                //sql += "where a.autono=b.autono and a.autono=c.autono and a.itcd=d.itcd and a.autono=n.autono(+) and a.slno=n.slno(+) and n.batchautono=o.batchautono(+) and ";
                //sql += sqlc;
                //sql += "d.brgrpcd=e.brgrpcd(+) and b.itgrpcd=f.itgrpcd(+) and a.makecd=g.makecd(+) ";
                //sql += "group by a.autono, a.autono||a.slno, a.ordautoslno, nvl(a.itmprccd,b.itmprccd), a.slno, a.itcd, d.itnm, ";
                //sql += "nvl(a.bluomcd,d.uomcd), a.damstock, nvl(a.hsncode,nvl(d.hsncode,e.hsncode)), a.itrem, d.prodcd, d.packsize, d.nosinbag, ";
                //sql += "a.nos, nvl(a.blqnty,a.qnty), a.rate, nvl(e.rateqntybag,f.rateqntybag), a.basamt, a.tddisctype, a.tddiscrate, a.tddiscamt, a.disctype, a.discrate, a.discamt, ";
                //sql += "a.agdocno, to_char(a.agdocdt, 'dd/mm/yyyy'), b.slcd||b.itgrpcd||c.compcd||c.loccd, e.bltophead, g.makenm, ";
                //sql += "a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.dutyper, a.dutyamt, a.cessper, a.cessamt,a. mtrlcd, a.poslno  ";
                //if (CommVar.InvSchema(UNQSNO) != "")
                //{
                //    sql += "union all ";

                //    sql += "select a.autono, a.autono||a.slno autoslno, '' ordautoslno, '' itmprccd, a.slno+500 slno, a.itcd, d.itdescn itnm, ";
                //    sql += "d.uom uomcd, '' damstock, d.hsncd hsncode, a.itrem, '' prodcd, 0 packsize, 0 nosinbag, ";
                //    sql += "0 nos, a.qnty, nvl(a.rate,0) rate, '' rateqntybag, 0 basamt, '' tddisctype, 0 tddiscrate, 0 tddiscamt, '' disctype, 0 discrate, 0 discamt, ";
                //    sql += "'' agdocno, to_char('', 'dd/mm/yyyy') agdocdt, b.slcd||b.itgrpcd||c.compcd||c.loccd slitgrpcd, '' bltophead, '' makenm, ";
                //    sql += "'' batchdtl, ";
                //    sql += "nvl(a.igstper,0) igstper, nvl(a.igstamt,0) igstamt, nvl(a.cgstper,0) cgstper, nvl(a.cgstamt,0) cgstamt, nvl(a.sgstper,0) sgstper, nvl(a.sgstamt,0) sgstamt, 0 dutyper, 0 dutyamt, nvl(a.cessper,0) cessper, nvl(a.cessamt,0) cessamt,'' mtrlcd, '' poslno  ";
                //    sql += "from " + Scm1 + ".t_txnproddtl a, " + Scm1 + ".t_txn b, " + Scm1 + ".t_cntrl_hdr c, " + scmI + ".m_item d ";
                //    sql += "where a.autono=b.autono and a.autono=c.autono and ";
                //    sql += sqlc;
                //    sql += "a.itcd=d.itcd ";
                //}
                //sql += "union all ";

                //sql += "select a.autono, a.autono autoslno, '' ordautoslno, b.itmprccd, nvl(ascii(d.calccode),0)+1000 slno, '' itcd, d.amtnm||' '||a.amtdesc itnm, ";
                //sql += "'' uomcd, '' damstock, a.hsncode hsncode,  '' itrem, '' prodcd, 0 packsize, 0 nosinbag, ";
                //sql += "0 nos, 0 qnty, 0 rate, '' rateqntybag, a.amt basamt, '' tddisctype, 0 tddiscrate, 0 tddiscamt, '' disctype, 0 discrate, 0 discamt, ";
                //sql += "'' agdocno, '' agdocdt, b.slcd||b.itgrpcd||c.compcd||c.loccd slitgrpcd, '' bltophead, '' makenm, '' batchdtl, ";
                //sql += "a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.dutyper, a.dutyamt, a.cessper, a.cessamt ,'' mtrlcd, '' poslno ";
                //sql += "from " + Scm1 + ".t_txnamt a, " + Scm1 + ".t_txn b, " + Scm1 + ".t_cntrl_hdr c, " + Scm1 + ".m_amttype d  ";
                //sql += "where a.autono=b.autono and a.autono=c.autono and ";
                //sql += sqlc;
                //sql += "a.amtcd=d.amtcd(+) ";
                //sql += ") a, ";

                //sql += "(select a.sapcd, a.slcd||a.itgrpcd||a.compcd||a.loccd slitgrpcd ";
                //sql += "from " + Scm1 + ".m_subleg_sddtl_itgrp a ) z, ";

                //sql += "(select a.autono||a.slno autoslno, a.bas_rate, a.pv_per ";
                //sql += "from " + Scm1 + ".t_txndtl a ) y, ";

                //sql += Scm1 + ".t_txn b, " + Scm1 + ".t_txntrans c, " + Scm1 + ".t_txnoth d, " + Scmf + ".m_subleg e, " + Scmf + ".m_subleg f, " + Scmf + ".m_subleg g, " + Scm1 + ".t_cntrl_hdr h, ";
                //sql += Scmf + ".m_uom i, " + Scm1 + ".m_group j, " + Scm1 + ".m_godown k, " + Scm1 + ".m_itemplist l, " + Scmf + ".m_subleg m, " + Scm1 + ".t_sorddtl n, " + Scm1 + ".m_itemplistdtl o ";
                //sql += "where a.autono=b.autono and a.autono=c.autono(+) and a.autono=d.autono(+) and b.slcd=e.slcd and nvl(b.conslcd,b.slcd)=f.slcd(+) and c.translcd=g.slcd(+) and a.autono=h.autono and ";
                //sql += "b.itgrpcd=j.itgrpcd(+) and a.uomcd=i.uomcd(+) and b.gocd=k.gocd(+) and a.itmprccd=l.itmprccd(+) and a.itmprccd=o.itmprccd(+) and a.itcd=o.itcd(+) and ";
                //sql += "a.slitgrpcd=z.slitgrpcd(+) and d.agslcd=m.slcd(+) and a.ordautoslno=n.autoslno(+) and a.autoslno=y.autoslno(+) and ";
                //sql += "a.autono not in ( ";
                //sql += "select a.autono ";
                //sql += "from " + Scm1 + ".t_cntrl_doc_pass a, " + Scm1 + ".t_cntrl_hdr b, " + Scm1 + ".t_cntrl_auth c ";
                //sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and c.autono is null and ";
                //sql += "b.doccd='" + doccd + "' ) ";
                //sql += "order by docno,autono,slno";
                sql += " select distinct a.autono, n.nm prtynm ,n.RTDEBCD,o.RTDEBNM,o.mobile rtdebmob,o.add1 rtdebadd1,o.add2 rtdebadd2,o.add3 rtdebadd3,o.add3 rtdebadd4,o.add3 rtdebadd5,o.add3 rtdebadd6,o.add3 rtdebadd7, ";
                sql += " o.city rtdebcity,o.statecd rtdebstcd,o.state rtdebstnm,o.email rtdebemail,o.pin rtdebpin,n.MOBILE prtymob,p.gstno prtygstno, ";
                sql += " p.GSTSLNM prtygstslnm,p.GSTSLADD1 prtyadd1,''prtyadd2,''prtyadd3,''prtyadd4,''prtyadd5,''prtyadd6,''prtyadd7,p.GSTSLDIST prtydist,p.GSTSLPIN prtypin,b.doctag, h.doccd, ";
                sql += "  h.docno, h.docdt, b.duedays, h.canc_rem, h.cancel,0 invisstime,'N' batchdlprint,  ";
                sql += " b.gocd, k.gonm, k.goadd1, k.goadd2, k.goadd3, k.gophno, k.goemail, h.usr_id, h.usr_entdt, h.vchrno, nvl(e.pslcd, e.slcd) oslcd, b.slcd, ";
                sql += " nvl(e.fullname, e.slnm) slnm, " + prnemailid + ", e.add1 sladd1, e.add2 sladd2, e.add3 sladd3, e.add4 sladd4, e.add5 sladd5, e.add6 sladd6, e.add7 sladd7,  ";
                sql += " e.gstno, e.panno, trim(e.regmobile || decode(e.regmobile, null, '', ',') || e.slphno || decode(e.phno1, null, '', ',' || e.phno1)) phno, e.state, e.country, e.statecd, e.actnameof slactnameof,  ";
                sql += " nvl(b.conslcd, b.slcd) cslcd, '' cpartycd, nvl(f.fullname, f.slnm) cslnm, f.add1 csladd1, f.add2 csladd2, f.add3 csladd3, f.add4 csladd4, f.add5 csladd5, ";
                sql += " f.add6 csladd6, f.add7 csladd7, nvl(f.gstno, f.gstno) cgstno, nvl(f.panno, f.panno) cpanno,f.actnameof cslactnameof, ";
                sql += " trim(f.regmobile || decode(f.regmobile, null, '', ',') || f.slphno || decode(f.phno1, null, '', ',' || f.phno1)) cphno, f.state cstate, f.statecd cstatecd,  ";
                sql += " c.translcd trslcd, g.slnm trslnm, g.gstno trgst, g.add1 trsladd1, g.add2 trsladd2, g.add3 trsladd3, g.add4 trsladd4, g.phno1 trslphno, c.lrno,  ";
                sql += " c.lrdt, c.lorryno, c.ewaybillno, c.grwt, c.ntwt, a.slno, a.itcd, a.styleno, a.itnm, a.itrem, a.batchdtl, a.hsncode,  ";
                sql += " a.nos, nvl(a.qnty,0)qnty, nvl(i.decimals, 0) qdecimal, i.uomnm, a.rate, nvl(a.amt,0)amt, d.docrem, d.docth, d.casenos, d.noofcases,  ";
                sql += " d.agslcd, m.slnm agslnm, a.agdocno, a.agdocdt, j.itgrpnm, j.shortnm,  ";
                sql += " nvl(a.igstper, 0)igstper, nvl(a.igstamt, 0)igstamt, nvl(a.cgstper, 0)cgstper, nvl(a.cgstamt, 0)cgstamt,  ";
                sql += " nvl(a.sgstper, 0)sgstper, nvl(a.sgstamt, 0)sgstamt, nvl(a.dutyper, 0)dutyper, nvl(a.dutyamt, 0)dutyamt, nvl(a.cessper, 0)cessper, nvl(a.cessamt, 0)cessamt,  ";
                sql += " nvl(a.igstper + a.cgstper + a.sgstper, 0) gstper, nvl(b.roamt, 0)roamt, nvl(b.blamt, 0) blamt, nvl(b.tcsper, 0) tcsper, nvl(b.tcsamt, 0) tcsamt, d.insby,  ";
                sql += " d.othnm, nvl(d.othadd1, f.othadd1) othadd1, d.porefno, d.porefdt, d.despby, d.dealby, d.packby, d.selby,  ";
                sql += " decode(d.othadd1, null, f.othadd2, d.othadd2) othadd2, decode(d.othadd1, null, f.othadd3, d.othadd3) othadd3, decode(d.othadd1, null, f.othadd4, d.othadd4) othadd4,  ";
                sql += " z.disctype, z.discrate, z.discamt, z.scmdisctype, z.scmdiscrate, z.scmdiscamt, z.tddisctype, z.tddiscrate, z.tddiscamt,  ";
                sql += " b.curr_cd,a.usr_id from  ";

                sql += " (select a.autono, a.autono || a.slno autoslno, a.slno, a.itcd, d.itnm, nvl(o.pdesign, d.styleno) styleno, d.uomcd, nvl(a.hsncode, nvl(d.hsncode, f.hsncode)) hsncode,  ";
                sql += " a.itrem, a.baleno, a.nos, nvl(a.blqnty, a.qnty) qnty, a.flagmtr, a.rate, a.amt, a.agdocno, to_char(a.agdocdt, 'dd/mm/yyyy') agdocdt,  ";
                sql += " listagg(o.barno || ' (' || n.qnty || ')', ', ') within group(order by n.autono, n.slno) batchdtl,  ";
                sql += " a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.dutyper, a.dutyamt, a.cessper, a.cessamt,c.usr_id  ";
                sql += " from " + Scm1 + ".t_txndtl a, " + Scm1 + ".t_txn b, " + Scm1 + ".t_cntrl_hdr c, " + Scm1 + ".m_sitem d, " + Scm1 + ".m_group f, " + Scm1 + ".t_batchdtl  n, " + Scm1 + ".t_batchmst o  ";
                sql += " where a.autono = b.autono and a.autono = c.autono and a.itcd = d.itcd and a.autono = n.autono(+) and a.slno = n.txnslno(+) and n.barno = o.barno(+)  and";
                sql += " c.compcd = '" + COM + "' and c.loccd = '" + LOC + "' and c.yr_cd = '" + yr_cd + "' and  ";
                if (fdocno != "") sql += " c.doconlyno >= " + fdocno + " and c.doconlyno <= " + tdocno + " and  ";
                if (fdate != "") sql += " c.docdt >= to_date('" + fdate + "', 'dd/mm/yyyy') and  ";
                if (tdate != "") sql += " c.docdt <= to_date('" + tdate + "', 'dd/mm/yyyy') and  ";
                sql += " c.doccd = '" + doccd + "' and d.itgrpcd = f.itgrpcd(+)  ";
                sql += " group by a.autono, a.autono || a.slno, a.slno, a.itcd, d.itnm, nvl(o.pdesign, d.styleno), d.uomcd, nvl(a.hsncode, nvl(d.hsncode, f.hsncode)),  ";
                sql += " a.itrem, a.baleno, a.nos, nvl(a.blqnty, a.qnty), a.flagmtr, a.rate,a.amt, a.agdocno, to_char(a.agdocdt, 'dd/mm/yyyy'),  ";
                sql += " a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.dutyper, a.dutyamt, a.cessper, a.cessamt,c.usr_id  ";
                sql += " union all  ";

                sql += " select a.autono, a.autono autoslno, nvl(ascii(d.calccode), 0) + 1000 slno, '' itcd, d.amtnm || ' ' || a.amtdesc itnm, '' styleno, '' uomcd, a.hsncode hsncode,  ";
                sql += " '' itrem, '' baleno, 0 nos, 0 qnty, 0 flagmtr, a.AMTRATE rate, a.amt, '' agroup, '' agdocdt, '' batchdtl,  ";
                sql += " a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.dutyper, a.dutyamt, a.cessper, a.cessamt,c.usr_id  ";
                sql += " from " + Scm1 + ".t_txnamt a, " + Scm1 + ".t_txn b, " + Scm1 + ".t_cntrl_hdr c, " + Scm1 + ".m_amttype d  ";
                sql += " where a.autono = b.autono and a.autono = c.autono  and c.compcd = '" + COM + "' and c.loccd = '" + LOC + "' and c.yr_cd = '" + yr_cd + "' and  ";
                if (fdocno != "") sql += " c.doconlyno >= " + fdocno + " and c.doconlyno <= " + tdocno + " and ";
                if (fdate != "") sql += " c.docdt >= to_date('" + fdate + "', 'dd/mm/yyyy') and  ";
                if (tdate != "") sql += " c.docdt <= to_date('" + tdate + "', 'dd/mm/yyyy') and  ";
                sql += "c.doccd = '" + doccd + "'  ";
                sql += "and a.amtcd = d.amtcd(+)  ";
                sql += " ) a,  ";

                sql += " " + Scm1 + ".t_txndtl z, " + Scm1 + ".t_txn b, " + Scm1 + ".t_txntrans c, " + Scm1 + ".t_txnoth d, " + Scmf + ".m_subleg e, " + Scmf + ".m_subleg f, " + Scmf + ".m_subleg g,  ";
                sql += " " + Scm1 + ".t_cntrl_hdr h, " + Scmf + ".m_uom i, " + Scm1 + ".m_group j, " + Scmf + ".m_godown k, " + Scm1 + ".m_sitem l, " + Scmf + ".m_subleg m, " + Scm1 + ".T_TXNMEMO n," + Scmf + ".M_RETDEB o ," + Scmf + ".T_VCH_GST p ";
                sql += " where a.autono = z.autono(+) and a.slno = z.slno(+) and a.autono = b.autono and a.autono = c.autono(+) and a.autono = d.autono(+) and  ";
                sql += " b.slcd = e.slcd and nvl(b.conslcd, b.slcd) = f.slcd(+) and c.translcd = g.slcd(+) and a.autono = h.autono and a.itcd = l.itcd(+) and l.itgrpcd = j.itgrpcd(+) and a.uomcd = i.uomcd(+) and  ";
                sql += " b.gocd = k.gocd(+) and d.agslcd = m.slcd(+)  and b.autono = n.autono and n.RTDEBCD=o.RTDEBCD and b.autono=p.autono and ";
                if (slcd != null) sql += " b.slcd ='" + slcd + "' and ";
                sql += " a.autono not in (select a.autono from " + Scm1 + ".t_cntrl_doc_pass a, " + Scm1 + ".t_cntrl_hdr b, " + Scm1 + ".t_cntrl_auth c  ";
                sql += " where a.autono = b.autono(+) and a.autono = c.autono(+)  and c.autono is null and b.doccd = '" + doccd + "' )   ";
                sql += " order by docno,autono,slno  ";

                DataTable tbl = new DataTable();
                tbl = masterHelp.SQLquery(sql);
                if (tbl.Rows.Count == 0) return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "Records not found !!" });

                DataTable rsStkPrcDesc;
                sql = "";
                sql += "select distinct a.autono, a.autono||a.itcd autoitcd, e.prcdesc stkprcdesc ";
                sql += "from " + Scm1 + ".t_batchdtl a, " + Scm1 + ".t_batchmst d, " + Scm1 + ".t_txn b, " + Scm1 + ".m_itemplist e, " + Scm1 + ".t_cntrl_hdr c ";
                sql += "where a.batchautono=d.batchautono(+) and d.itmprccd=e.itmprccd(+) and a.autono=b.autono(+) and ";
                sql += sqlc;
                sql += "a.autono=c.autono ";
                rsStkPrcDesc = masterHelp.SQLquery(sql);

                //string blterms = "", inspoldesc = "", dealsin = "";
                //Int16 bankslno = 0;
                //sql = "select blterms, inspoldesc, dealsin, nvl(bankslno,0) bankslno from " + Scm1 + ".m_mgroup_spl where compcd='" + CommVar.Compcd(UNQSNO) + "' and itgrpcd='" + tbl.Rows[0]["itgrpcd"].ToString() + "'";
                //DataTable rsMgroupSpl = masterHelp.SQLquery(sql);
                //if (rsMgroupSpl.Rows.Count > 0)
                //{
                //    if (rsMgroupSpl.Rows[0]["blterms"].ToString() != "") blterms = rsMgroupSpl.Rows[0]["blterms"].ToString();
                //    inspoldesc = rsMgroupSpl.Rows[0]["inspoldesc"].ToString();
                //    dealsin = rsMgroupSpl.Rows[0]["dealsin"].ToString();
                //    bankslno = Convert.ToInt16(rsMgroupSpl.Rows[0]["bankslno"]);
                //}
                string blterms = "", inspoldesc = "", dealsin = "";
                Int16 bankslno = 0;
                sql = "select blterms, inspoldesc, dealsin, nvl(bankslno,0) bankslno from " + Scm1 + ".m_mgroup_spl where compcd='" + CommVar.Compcd(UNQSNO) + "' ";
                DataTable rsMgroupSpl = masterHelp.SQLquery(sql);
                if (rsMgroupSpl.Rows.Count > 0)
                {
                    if (rsMgroupSpl.Rows[0]["blterms"].ToString() != "") blterms = rsMgroupSpl.Rows[0]["blterms"].ToString();
                    inspoldesc = rsMgroupSpl.Rows[0]["inspoldesc"].ToString();
                    dealsin = rsMgroupSpl.Rows[0]["dealsin"].ToString();
                    bankslno = Convert.ToInt16(rsMgroupSpl.Rows[0]["bankslno"]);
                }

                #region  Datatabe IR generate
                DataTable IR = new DataTable();
                IR.Columns.Add("autono", typeof(string), "");
                IR.Columns.Add("copymode", typeof(string), "");
                IR.Columns.Add("docno", typeof(string), "");
                IR.Columns.Add("docdt", typeof(string), "");
                IR.Columns.Add("insby", typeof(string), "");
                IR.Columns.Add("invisstime", typeof(double), "");
                IR.Columns.Add("duedays", typeof(double), "");
                IR.Columns.Add("country", typeof(string), "");
                IR.Columns.Add("itgrpnm", typeof(string), "");
                IR.Columns.Add("shortnm", typeof(string), "");
                IR.Columns.Add("stkprcdesc", typeof(string), "");
                IR.Columns.Add("gocd", typeof(string), "");
                IR.Columns.Add("gonm", typeof(string), "");
                IR.Columns.Add("goadd1", typeof(string), "");
                IR.Columns.Add("goadd2", typeof(string), "");
                IR.Columns.Add("goadd3", typeof(string), "");
                IR.Columns.Add("gophno", typeof(string), "");
                IR.Columns.Add("goemail", typeof(string), "");
                IR.Columns.Add("slcd", typeof(string), "");
                IR.Columns.Add("partycd", typeof(string), "");
                IR.Columns.Add("slnm", typeof(string), "");
                IR.Columns.Add("sladd1", typeof(string), "");
                IR.Columns.Add("sladd2", typeof(string), "");
                IR.Columns.Add("sladd3", typeof(string), "");
                IR.Columns.Add("sladd4", typeof(string), "");
                IR.Columns.Add("sladd5", typeof(string), "");
                IR.Columns.Add("sladd6", typeof(string), "");
                IR.Columns.Add("sladd7", typeof(string), "");
                IR.Columns.Add("sladd8", typeof(string), "");
                IR.Columns.Add("sladd9", typeof(string), "");
                IR.Columns.Add("sladd10", typeof(string), "");
                IR.Columns.Add("othadd1", typeof(string), "");
                IR.Columns.Add("othadd2", typeof(string), "");
                IR.Columns.Add("othadd3", typeof(string), "");
                IR.Columns.Add("othadd4", typeof(string), "");
                IR.Columns.Add("disctype", typeof(string), "");
                IR.Columns.Add("discrate", typeof(double), "");
                IR.Columns.Add("cslcd", typeof(string), "");
                IR.Columns.Add("cpartycd", typeof(string), "");
                IR.Columns.Add("cslnm", typeof(string), "");
                IR.Columns.Add("csladd1", typeof(string), "");
                IR.Columns.Add("csladd2", typeof(string), "");
                IR.Columns.Add("csladd3", typeof(string), "");
                IR.Columns.Add("csladd4", typeof(string), "");
                IR.Columns.Add("csladd5", typeof(string), "");
                IR.Columns.Add("csladd6", typeof(string), "");
                IR.Columns.Add("csladd7", typeof(string), "");
                IR.Columns.Add("csladd8", typeof(string), "");
                IR.Columns.Add("csladd9", typeof(string), "");
                IR.Columns.Add("csladd10", typeof(string), "");
                IR.Columns.Add("porefno", typeof(string), "");
                IR.Columns.Add("porefdt", typeof(string), "");
                IR.Columns.Add("trslcd", typeof(string), "");
                IR.Columns.Add("trslnm", typeof(string), "");
                IR.Columns.Add("trgst", typeof(string), "");
                IR.Columns.Add("lrno", typeof(string), "");
                IR.Columns.Add("lrdt", typeof(string), "");
                IR.Columns.Add("lorryno", typeof(string), "");
                IR.Columns.Add("ewaybillno", typeof(string), "");
                IR.Columns.Add("grwt", typeof(double), "");
                IR.Columns.Add("ntwt", typeof(double), "");
                IR.Columns.Add("caltype", typeof(double), "");
                IR.Columns.Add("slno", typeof(double), "");
                IR.Columns.Add("itcd", typeof(string), "");
                IR.Columns.Add("styleno", typeof(string), "");
                IR.Columns.Add("itnm", typeof(string), "");
                IR.Columns.Add("itrem", typeof(string), "");
                IR.Columns.Add("itdesc", typeof(string), "");
                IR.Columns.Add("batchdtl", typeof(string), "");
                IR.Columns.Add("gstno", typeof(double), "");
                IR.Columns.Add("hsncode", typeof(string), "");
                IR.Columns.Add("nos", typeof(double), "");
                IR.Columns.Add("casenos", typeof(double), "");
                IR.Columns.Add("noofcases", typeof(double), "");
                IR.Columns.Add("qnty", typeof(double), "");
                IR.Columns.Add("uomnm", typeof(string), "");
                IR.Columns.Add("qdecimal", typeof(double), "");
                IR.Columns.Add("rate", typeof(double), "");
                IR.Columns.Add("rateuomnm", typeof(string), "");
                IR.Columns.Add("amt", typeof(double), "");
                IR.Columns.Add("stddisc", typeof(string), "");
                IR.Columns.Add("tddiscamt", typeof(double), "");
                IR.Columns.Add("disc", typeof(string), "");
                IR.Columns.Add("discamt", typeof(double), "");
                IR.Columns.Add("scmdisctype", typeof(string), "");
                IR.Columns.Add("scmdiscrate", typeof(double), "");
                IR.Columns.Add("scmdiscamt", typeof(double), "");
                IR.Columns.Add("tddisctype", typeof(string), "");
                IR.Columns.Add("tddiscrate", typeof(double), "");
                IR.Columns.Add("txblval", typeof(string), "");
                IR.Columns.Add("cgstdsp", typeof(string), "");
                IR.Columns.Add("cgstper", typeof(double), "");
                IR.Columns.Add("cgstamt", typeof(double), "");
                IR.Columns.Add("sgstdsp", typeof(string), "");
                IR.Columns.Add("sgstper", typeof(double), "");
                IR.Columns.Add("sgstamt", typeof(double), "");
                IR.Columns.Add("cessper", typeof(double), "");
                IR.Columns.Add("cessamt", typeof(double), "");
                IR.Columns.Add("gstper", typeof(double), "");
                IR.Columns.Add("discper", typeof(double), "");
                IR.Columns.Add("revchrg", typeof(string), "");
                IR.Columns.Add("rupinword", typeof(string), "");
                IR.Columns.Add("netamt", typeof(double), "");
                IR.Columns.Add("roamt", typeof(double), "");
                IR.Columns.Add("blamt", typeof(string), "");
                IR.Columns.Add("tcsper", typeof(double), "");
                IR.Columns.Add("tcsamt", typeof(double), "");
                IR.Columns.Add("blremarks", typeof(string), "");
                IR.Columns.Add("agdocno", typeof(string), "");
                IR.Columns.Add("agdocdt", typeof(string), "");
                IR.Columns.Add("usr_id", typeof(string), "");
                IR.Columns.Add("usr_entdt", typeof(string), "");
                IR.Columns.Add("vchrno", typeof(string), "");
                IR.Columns.Add("cancel", typeof(string), "");
                IR.Columns.Add("canc_rem", typeof(string), "");
                IR.Columns.Add("titdiscamt", typeof(double), "");
                IR.Columns.Add("oslcd", typeof(string), "");
                IR.Columns.Add("agslcd", typeof(string), "");
                IR.Columns.Add("docth", typeof(string), "");
                IR.Columns.Add("transgst", typeof(string), "");
                IR.Columns.Add("agentnm", typeof(string), "");
                IR.Columns.Add("trsladd1", typeof(string), "");
                IR.Columns.Add("trsladd2", typeof(string), "");
                IR.Columns.Add("trsladd3", typeof(string), "");
                IR.Columns.Add("trsladd4", typeof(string), "");
                IR.Columns.Add("payterms", typeof(string), "");
                IR.Columns.Add("bankactno", typeof(string), "");
                IR.Columns.Add("bankname", typeof(string), "");
                IR.Columns.Add("bankbranch", typeof(string), "");
                IR.Columns.Add("bankifsc", typeof(string), "");
                IR.Columns.Add("bankadd", typeof(string), "");
                IR.Columns.Add("bankrtgs", typeof(string), "");
                IR.Columns.Add("duedt", typeof(string), "");
                IR.Columns.Add("igstper", typeof(double), "");
                IR.Columns.Add("igstamt", typeof(double), "");
                IR.Columns.Add("dutyper", typeof(double), "");
                IR.Columns.Add("dutyamt", typeof(double), "");
                IR.Columns.Add("dtldsc", typeof(string), "");
                IR.Columns.Add("dtlamt", typeof(string), "");
                IR.Columns.Add("netpaybledesc", typeof(string), "");
                IR.Columns.Add("netpayble", typeof(double), "");
                IR.Columns.Add("despby", typeof(string), "");
                IR.Columns.Add("dealby", typeof(string), "");
                IR.Columns.Add("packby", typeof(string), "");
                IR.Columns.Add("selby", typeof(string), "");
                IR.Columns.Add("dealsin", typeof(string), "");
                IR.Columns.Add("blterms", typeof(string), "");
                IR.Columns.Add("hsn_cd", typeof(string), "");
                IR.Columns.Add("hsn_hddsp1", typeof(string), "");
                IR.Columns.Add("hsn_hddsp2", typeof(string), "");
                IR.Columns.Add("hsn_txblval", typeof(string), "");
                IR.Columns.Add("hsn_gstper1", typeof(string), "");
                IR.Columns.Add("hsn_gstamt1", typeof(string), "");
                IR.Columns.Add("hsn_gstper2", typeof(string), "");
                IR.Columns.Add("hsn_gstamt2", typeof(string), "");
                IR.Columns.Add("hsn_gstper3", typeof(string), "");
                IR.Columns.Add("hsn_gstamt3", typeof(string), "");
                IR.Columns.Add("hsn_cessamt", typeof(string), "");
                IR.Columns.Add("hsn_gstamt", typeof(string), "");
                IR.Columns.Add("hsn_qnty", typeof(string), "");
                IR.Columns.Add("hsn_tqnty", typeof(string), "");
                IR.Columns.Add("hsn_ttxblval", typeof(string), "");
                IR.Columns.Add("hsn_tgstamt1", typeof(string), "");
                IR.Columns.Add("hsn_tgstamt2", typeof(string), "");
                IR.Columns.Add("hsn_tgstamt3", typeof(string), "");
                IR.Columns.Add("totalosamt", typeof(string), "");
                IR.Columns.Add("rateqntybag", typeof(string), "");
                IR.Columns.Add("regemailid", typeof(string), "");
                IR.Columns.Add("menu_para", typeof(string), "");
                IR.Columns.Add("upiimg", typeof(string), "");
                IR.Columns.Add("upidesc", typeof(string), "");
                IR.Columns.Add("curr_cd", typeof(string), "");
                IR.Columns.Add("shipmarkno", typeof(string), "");
                IR.Columns.Add("blitdesc", typeof(string), "");


                IR.Columns.Add("agstdocno", typeof(string), "");
                IR.Columns.Add("agstdocdt", typeof(string), "");
                IR.Columns.Add("makenm", typeof(string), "");
                IR.Columns.Add("areacd", typeof(string), "");
                IR.Columns.Add("itmprccd", typeof(string), "");
                IR.Columns.Add("itmprcdesc", typeof(string), "");
                IR.Columns.Add("prceffdt", typeof(string), "");
                IR.Columns.Add("weekno", typeof(double), "");
                IR.Columns.Add("ordrefno", typeof(string), "");
                IR.Columns.Add("ordrefdt", typeof(string), "");
                IR.Columns.Add("packsize", typeof(double), "");
                IR.Columns.Add("hsnsaccd", typeof(string), "");
                IR.Columns.Add("prodcd", typeof(string), "");
                IR.Columns.Add("basamt", typeof(double), "");
                IR.Columns.Add("stddiscamt", typeof(double), "");
                IR.Columns.Add("taxableval", typeof(string), "");
                IR.Columns.Add("user_id", typeof(string), "");
                IR.Columns.Add("bltophead", typeof(string), "");
                IR.Columns.Add("nopkgs", typeof(string), "");
                IR.Columns.Add("mrp", typeof(double), "");
                IR.Columns.Add("poslno", typeof(string), "");
                IR.Columns.Add("plsupply", typeof(string), "");
                IR.Columns.Add("destn", typeof(string), "");
                IR.Columns.Add("mtrlcd", typeof(string), "");
                IR.Columns.Add("bas_rate", typeof(string), "");
                IR.Columns.Add("pv_per", typeof(string), "");
                IR.Columns.Add("insudesc", typeof(string), "");
                IR.Columns.Add("pvtag", typeof(string), "");
                IR.Columns.Add("precarr", typeof(string), "");
                IR.Columns.Add("precarrrecpt", typeof(string), "");
                IR.Columns.Add("portload", typeof(string), "");
                IR.Columns.Add("portdesc", typeof(string), "");
                IR.Columns.Add("finaldest", typeof(string), "");
                IR.Columns.Add("bankinter", typeof(string), "");
                #endregion

                string bankname = "", bankactno = "", bankbranch = "", bankifsc = "", bankadd = "", bankrtgs = "";
                sql = "";
                sql += "select a.bankname, a.bankactno, a.ifsccode, a.address, a.branch ";
                sql += "from " + Scmf + ".m_loca_ifsc a ";
                sql += "where a.compcd = '" + COM + "' and a.loccd = '" + LOC + "' and ";
                if (bankslno == 0) sql += "a.defltbank = 'T' ";
                else sql += "a.slno=" + bankslno.ToString() + " ";
                DataTable rsbank = masterHelp.SQLquery(sql);
                if (rsbank.Rows.Count > 0)
                {
                    bankrtgs += "You can Make RTGS/NEFT to " + rsbank.Rows[0]["bankname"] + " ( IFSC-" + rsbank.Rows[0]["ifsccode"] + " ) A/c No-" + rsbank.Rows[0]["bankactno"];
                    if (rsbank.Rows[0]["address"].ToString() != "") bankrtgs += " Address - " + rsbank.Rows[0]["address"];
                    bankrtgs += ".";

                    bankname = rsbank.Rows[0]["bankname"].ToString();
                    bankactno = rsbank.Rows[0]["bankactno"].ToString();
                    bankifsc = rsbank.Rows[0]["ifsccode"].ToString();
                    bankbranch = rsbank.Rows[0]["branch"].ToString();
                    bankadd = rsbank.Rows[0]["address"].ToString();
                }

                maxR = tbl.Rows.Count - 1;
                Int32 i = 0; int istore = 0; int lslno = 0; int ilast = 0;
                string auto1 = ""; string copymode = ""; string blrem = ""; string itdsc = "";
                /*string blhead = "";*/
                string fssailicno = ""; /*string grpemailid = "";*/ string goadd = ""/*, gocd = ""*/;
                string rupinwords = "";
                int uomdecimal = 3; int uommaxdecimal = 0;

                switch (tbl.Rows[0]["doctag"].ToString())
                {
                    case "SB":
                        blhead = "TAX INVOICE"; break;
                    case "SD":
                        blhead = "DEBIT NOTE"; break;
                    case "SC":
                        blhead = "CREDIT NOTE"; break;
                    case "SR":
                        blhead = "CREDIT NOTE"; break;
                    case "PR":
                        blhead = "PURCHASE RETURN NOTE"; break;
                    case "PD":
                        blhead = "DEBIT NOTE [Purchase]"; break;
                    case "PC":
                        blhead = "CREDIT NOTE [Purchase]"; break;
                    case "PI":
                        blhead = "PROFORMA INVOICE"; break;
                    case "CI":
                        blhead = "TAX INVOICE"; break;
                    case "SO":
                        blhead = "STOCK TRANSFER"; break;
                    case "PB":
                        blhead = "PURCHASE INVOICE"; break;
                    default: blhead = ""; break;
                }

                Int16 maxCopy = 5;

                while (i <= maxR)
                {

                    //grpemailid = tbl.Rows[i]["grpemailid"].ToString();
                    gocd = tbl.Rows[i]["gocd"].ToString();
                    goadd = tbl.Rows[i]["goadd1"].ToString() + " " + tbl.Rows[i]["goadd2"].ToString() + " " + tbl.Rows[i]["goadd3"].ToString();
                    if (tbl.Rows[i]["gophno"].ToString() != "") goadd = goadd + " Phone : " + tbl.Rows[i]["gophno"].ToString();
                    istore = i;
                    string docno = "";
                    for (int ic = 0; ic <= maxCopy; ic++)
                    {
                        i = istore;
                        lslno = 0;
                        auto1 = tbl.Rows[i]["autono"].ToString();
                        double dbasamt = 0; double ddisc1 = 0; double ddisc2 = 0; double dtxblval = 0;
                        double dcgstamt = 0; double dsgstamt = 0; double dnetamt = 0; double dnos = 0; double dqnty = 0;
                        bool doctotprint = false; bool totalreadyprint = false; bool delvchrg = false;

                        string dtldsc = "", dtlamt = "", fdtldsc = "", fdtlamt = "";
                        double tqnty = 0, tnos = 0, tamt = 0, tpamt = 0, tgst = 0, blamt = 0, totalosamt = 0, tpaymt = 0;
                        string hsnqnty = "", hsntaxblval = "", hsngstper1 = "", hsngstper2 = "", hsngstper3 = "", hsngstamt1 = "", hsngstamt2 = "", hsngstamt3 = "", hsncode = "";
                        double gstper1 = 0, gstamt1 = 0, total_qnty = 0, total_taxval = 0, total_gstamt1 = 0, total_gstamt2 = 0, total_gstamt3 = 0, payamt = 0;
                        bool flagi = false, flagc = false, flags = false;

                        if (copyno[ic].ToString() != "N")
                        {
                            //rupinwords = Cn.AmountInWords((tbl.Rows[i]["blamt"].retDbl() - tbl.Rows[i]["advrecdamt"].retDbl()).ToString(), tbl.Rows[i]["curr_cd"].ToString());
                            //rupinwords = Cn.AmountInWords((tbl.Rows[i]["blamt"].retDbl() - tbl.Rows[i]["advrecdamt"].retDbl()).ToString());
                            string oslcd = "", oglcd = "", odocdt = "", oclass1cd = "";

                            if (doctype == "SBILL" && VE.Checkbox7 == true)
                            {
                                oslcd = tbl.Rows[i]["oslcd"].ToString();
                                oglcd = tbl.Rows[i]["debglcd"].ToString();
                                odocdt = Convert.ToDateTime(tbl.Rows[i]["docdt"].ToString()).AddDays(-1).ToString().retDateStr();
                                totalosamt = Convert.ToDouble(MasterHelpFa.slcdbal(oslcd, oglcd, odocdt, oclass1cd));
                                oslcd.retStr();

                                sql = "";
                                sql += "select sum(nvl(a.blamt,0)) blamt from ( ";
                                sql += "select nvl(b.pslcd,a.slcd) oslcd, sum(nvl(a.blamt,0)) blamt ";
                                sql += "from " + Scm1 + ".t_txn a, " + Scmf + ".m_subleg b, " + Scm1 + ".t_cntrl_hdr c, " + Scm1 + ".m_doctype d ";
                                sql += "where a.autono=c.autono and c.doccd=d.doccd and a.slcd=b.slcd and nvl(c.cancel,'N')='N' and c.compcd='" + COM + "' and c.yr_cd='" + yr_cd + "' and ";
                                sql += "c.docdt=to_date('" + tbl.Rows[i]["docdt"].ToString().retDateStr() + "','dd/mm/yyyy') and ";
                                sql += "d.doctype='SBILL' and c.vchrno <= " + Convert.ToDouble(tbl.Rows[i]["vchrno"]) + " and c.doccd='" + doccd + "' ";
                                sql += "group by nvl(b.pslcd,a.slcd) ) a where oslcd='" + oslcd + "'";
                                rsTmp = MasterHelpFa.SQLquery(sql);
                                if (rsTmp.Rows.Count > 0) totalosamt = totalosamt + Convert.ToDouble(rsTmp.Rows[0]["blamt"] == DBNull.Value ? 0 : rsTmp.Rows[0]["blamt"]);
                            }
                            string qry = "select sum(a.amt) payamt from " + Scm1 + ".T_TXNPYMT a where a.autono= '" + auto1 + "'  ";

                            DataTable ptmy = masterHelp.SQLquery(qry);
                            if (ptmy.Rows.Count > 0) payamt = ptmy.Rows[0]["payamt"].retDbl();
                            Type A_T = tbl.Rows[0]["amt"].GetType(); var P_A = payamt.retDbl();
                            Type Q_T = tbl.Rows[0]["qnty"].GetType(); Type N_S = tbl.Rows[0]["nos"].GetType(); Type I_T = tbl.Rows[0]["igstamt"].GetType();
                            Type C_T = tbl.Rows[0]["cgstamt"].GetType(); Type S_T = tbl.Rows[0]["sgstamt"].GetType();

                            var GST_DATA = (from DataRow DR in tbl.Rows
                                            where DR["autono"].retStr() == auto1
                                            group DR by new { IGST = DR["igstper"].retStr(), CGST = DR["cgstper"].retStr(), SGST = DR["sgstper"].retStr() } into X
                                            select new
                                            {
                                                IGSTPER = X.Key.IGST.retDbl(),
                                                CGSTPER = X.Key.CGST.retDbl(),
                                                SGSTPER = X.Key.SGST.retDbl(),
                                                //TPAMT = P_A.Name == "Double" ? X.Sum(Z => Z.Field<double>("payamt").retDbl()) : (X.Sum(Z => Z.Field<decimal>("payamt").retDbl())),
                                                TPAMT = P_A,
                                                TAMT = A_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("amt").retDbl()) : (X.Sum(Z => Z.Field<decimal>("amt").retDbl())),
                                                TQNTY = Q_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("qnty").retDbl()) : (X.Sum(Z => Z.Field<decimal>("qnty").retDbl())),
                                                TNOS = N_S.Name == "Double" ? X.Sum(Z => Z.Field<double>("nos").retDbl()) : (X.Sum(Z => Z.Field<decimal>("nos").retDbl())),
                                                IGSTAMT = I_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("igstamt").retDbl()) : (X.Sum(Z => Z.Field<decimal>("igstamt").retDbl())),
                                                CGSTAMT = C_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("cgstamt").retDbl()) : (X.Sum(Z => Z.Field<decimal>("cgstamt").retDbl())),
                                                SGSTAMT = S_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("sgstamt").retDbl()) : (X.Sum(Z => Z.Field<decimal>("sgstamt").retDbl())),
                                                TOTALPER = (X.Key.IGST.retDbl()) + (X.Key.CGST.retDbl()) + (X.Key.SGST.retDbl())
                                            }).OrderBy(A => A.TOTALPER).ToList();

                            if (GST_DATA != null && GST_DATA.Count > 0)
                            {
                                foreach (var k in GST_DATA)
                                {
                                    //if (k.IGSTAMT != 0) { dtldsc += "(+) IGST @ " + Cn.Indian_Number_format(k.IGSTPER, "0.00") + " %~"; dtlamt += Convert.ToDouble(k.IGSTAMT).ToINRFormat() + "~"; }
                                    //if (k.CGSTAMT != 0) { dtldsc += "(+) CGST @ " + Cn.Indian_Number_format(k.CGSTPER, "0.00") + " %~"; dtlamt += Convert.ToDouble(k.CGSTAMT).ToINRFormat() + "~"; }
                                    //if (k.SGSTAMT != 0) { dtldsc += "(+) SGST @ " + Cn.Indian_Number_format(k.SGSTPER, "0.00") + " %~"; dtlamt += Convert.ToDouble(k.SGSTAMT).ToINRFormat() + "~"; }
                                    tpaymt = k.TPAMT.retDbl();
                                    //if (k.TPAMT.retDbl() != 0) { dtldsc += "Less Cash Received" + "~"; dtlamt += Convert.ToDouble(k.TPAMT).ToINRFormat(); }
                                    tqnty = tqnty + Convert.ToDouble(k.TQNTY);
                                    tnos = tnos + Convert.ToDouble(k.TNOS);
                                    tamt = tamt + Convert.ToDouble(k.TAMT);
                                    tpamt = k.TPAMT.retDbl();
                                    //tgst = tgst + Convert.ToDouble(k.IGSTAMT) + Convert.ToDouble(k.CGSTAMT) + Convert.ToDouble(k.SGSTAMT);
                                }
                            }
                            if (tpaymt != 0) { dtldsc += "Less Cash Received" + "~"; dtlamt += Convert.ToDouble(tpaymt).ToINRFormat(); }
                            //if (tbl.Rows[0]["ADVRECDAMT"].retDbl() != 0)
                            //{
                            //    dtldsc += "(-) " + tbl.Rows[0]["ADVRECDREM"].retStr() + "~";
                            //    dtlamt += tbl.Rows[0]["ADVRECDAMT"].retDbl().ToINRFormat() + "~";
                            //}

                            var HSN_DATA = (from a in DBF.T_VCH_GST
                                            where a.AUTONO == auto1
                                            group a by new { HSNCODE = a.HSNCODE, IGSTPER = a.IGSTPER, CGSTPER = a.CGSTPER, SGSTPER = a.SGSTPER } into x
                                            select new
                                            {

                                                HSNCODE = x.Key.HSNCODE,
                                                IGSTPER = x.Key.IGSTPER,
                                                CGSTPER = x.Key.CGSTPER,
                                                SGSTPER = x.Key.SGSTPER,
                                                TIGSTAMT = x.Sum(s => s.IGSTAMT),
                                                TCGSTAMT = x.Sum(s => s.CGSTAMT),
                                                TSGSTAMT = x.Sum(s => s.SGSTAMT),
                                                TAMT = x.Sum(s => s.AMT),
                                                TQNTY = x.Sum(s => s.QNTY)
                                                //DECIMAL = (from z in DBF.M_UOM
                                                //           where z.UOMCD == (from y in DBF.T_VCH_GST where y.AUTONO == auto1 select y.UOM).FirstOrDefault()
                                                //           select z.DECIMALS).FirstOrDefault()
                                                //DECIMALS = (from c in DBF.M_UOM where c.UOMCD ==  select c.DECIMALS)
                                            }).ToList();

                            if (HSN_DATA != null && HSN_DATA.Count > 0)
                            {
                                foreach (var k in HSN_DATA)
                                {
                                    var uom = (from a in DBF.T_VCH_GST
                                               where a.AUTONO == auto1 && a.IGSTPER == k.IGSTPER && a.CGSTPER == k.CGSTPER
                                      && a.SGSTPER == k.SGSTPER && a.HSNCODE == k.HSNCODE
                                               select a.UOM).FirstOrDefault();
                                    double DECIMAL = 0; string umnm = "";
                                    var uomdata = DBF.M_UOM.Find(uom);
                                    if (uomdata != null)
                                    {
                                        DECIMAL = Convert.ToDouble(uomdata.DECIMALS);
                                        umnm = uomdata.UOMNM;
                                    }
                                    if (k.TIGSTAMT > 0) flagi = true;
                                    if (k.TCGSTAMT > 0) flagc = true;

                                    gstper1 = Convert.ToDouble(k.CGSTPER) + Convert.ToDouble(k.IGSTPER);
                                    gstamt1 = Convert.ToDouble(k.TCGSTAMT) + Convert.ToDouble(k.TIGSTAMT);

                                    if (k.HSNCODE != "") { hsncode += k.HSNCODE + "~"; }
                                    if (k.TQNTY != 0) { hsnqnty += Convert.ToDouble(k.TQNTY).ToString("n" + DECIMAL.ToString()) + " " + umnm + "~"; }
                                    if (k.TCGSTAMT + k.TIGSTAMT != 0)
                                    {
                                        if (k.IGSTPER != 0) hsngstper1 += Cn.Indian_Number_format(k.IGSTPER.ToString(), "0.00") + " %~";
                                        if (k.TIGSTAMT != 0) hsngstamt1 += Convert.ToDouble(k.TIGSTAMT).ToINRFormat() + "~";
                                    }
                                    else
                                    {
                                        hsngstper1 += "~";
                                        hsngstamt1 += "~";
                                    }
                                    if (k.TCGSTAMT + k.TCGSTAMT != 0)
                                    {
                                        if (k.CGSTPER != 0) hsngstper2 += Cn.Indian_Number_format(k.CGSTPER.ToString(), "0.00") + " %~";
                                        if (k.TCGSTAMT != 0) hsngstamt2 += Convert.ToDouble(k.TCGSTAMT).ToINRFormat() + "~";
                                    }
                                    else
                                    {
                                        hsngstper2 += "~";
                                        hsngstamt2 += "~";
                                    }
                                    if (k.TSGSTAMT != 0)
                                    {
                                        flags = true;
                                        if (k.SGSTPER != 0) hsngstper3 += Cn.Indian_Number_format(k.SGSTPER.ToString(), "0.00") + " %~";
                                        if (k.TSGSTAMT != 0) hsngstamt3 += Convert.ToDouble(k.TSGSTAMT).ToINRFormat() + "~";
                                    }
                                    else
                                    {
                                        hsngstper3 += "~";
                                        hsngstamt3 += "~";
                                    }
                                    if (k.TAMT != 0) { hsntaxblval += Convert.ToDouble(k.TAMT).ToINRFormat() + "~"; } else { hsntaxblval += "~"; }

                                    total_qnty = total_qnty + Convert.ToDouble(k.TQNTY);
                                    total_taxval = total_taxval + Convert.ToDouble(k.TAMT);
                                    total_gstamt1 = total_gstamt1 + Convert.ToDouble(k.TIGSTAMT);
                                    total_gstamt2 = total_gstamt2 + Convert.ToDouble(k.TCGSTAMT);
                                    total_gstamt3 = total_gstamt3 + Convert.ToDouble(k.TSGSTAMT);
                                }
                            }
                        }

                        while (tbl.Rows[i]["autono"].ToString() == auto1)
                        {
                            var dchrg = (from DataRow dr in tbl.Rows
                                         where dr["itcd"].ToString() == "" && dr["autono"].ToString() == auto1
                                         select new
                                         {
                                             itrem = dr["itrem"]
                                         }).ToList();
                            docno = tbl.Rows[i]["docno"].ToString();
                            if (copyno[ic].ToString() == "N")
                            {
                                i = i + 1;
                                break;
                            }
                            switch (ic)
                            {
                                case 0:
                                    copymode = "ORIGINAL FOR RECIPIENT"; break;
                                case 1:
                                    copymode = "DUPLICATE FOR TRANSPORTER"; break;
                                case 2:
                                    copymode = "TRIPLICATE FOR SUPPLIER"; break;
                                case 3:
                                    copymode = "EXTRA COPY"; break;
                                case 4:
                                    copymode = "EXTRA COPY"; break;
                                case 5:
                                    copymode = "EXTRA COPY"; break;
                                default: copymode = ""; break;
                            }

                            DataRow dr1 = IR.NewRow();
                        docstart:
                            double duedays = 0;
                            string payterms = "";
                            duedays = tbl.Rows[i]["duedays"] == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["duedays"]);
                            //payterms = tbl.Rows[i]["payterms"].ToString();
                            if (payterms == "")
                            {
                                if (duedays == 0) payterms = ""; else payterms = duedays.ToString() + " days.";
                            }

                            //dr1["menu_para"] = VE.MENU_PARA;
                            //dr1["pvtag"] = VE.Checkbox7 == true ? "Y" : "N";
                            dr1["menu_para"] = VE.MENU_PARA;
                            //dr1["pvtag"] = tbl.Rows[i]["pv_tag"].ToString();
                            dr1["autono"] = auto1 + ic.ToString();
                            dr1["usr_id"] = tbl.Rows[i]["usr_id"].ToString();
                            dr1["cancel"] = tbl.Rows[i]["cancel"].ToString();
                            dr1["canc_rem"] = tbl.Rows[i]["canc_rem"].ToString();
                            dr1["copymode"] = copymode;
                            dr1["docno"] = tbl.Rows[i]["docno"].ToString();
                            dr1["docdt"] = tbl.Rows[i]["docdt"] == DBNull.Value ? "" : tbl.Rows[i]["docdt"].ToString().Substring(0, 10).ToString();
                            dr1["upiimg"] = "";
                            dr1["upidesc"] = "";
                            //dr1["areacd"] = tbl.Rows[i]["areacd"].ToString();
                            dr1["invisstime"] = tbl.Rows[i]["invisstime"].retDbl();
                            dr1["duedays"] = duedays;
                            //dr1["itmprccd"] = tbl.Rows[i]["itmprccd"].ToString();
                            //dr1["itmprcdesc"] = tbl.Rows[i]["itmprcdesc"].ToString();
                            //dr1["prceffdt"] = tbl.Rows[i]["prceffdt"] == DBNull.Value ? "" : tbl.Rows[i]["prceffdt"].ToString().Substring(0, 10).ToString();
                            dr1["duedt"] = Convert.ToDateTime(tbl.Rows[i]["docdt"].ToString()).AddDays(duedays).ToString().retDateStr();
                            dr1["packby"] = tbl.Rows[i]["packby"].retStr();
                            dr1["selby"] = tbl.Rows[i]["selby"].retStr();
                            dr1["dealby"] = tbl.Rows[i]["dealby"].retStr();
                            dr1["payterms"] = payterms;
                            //if (rsStkPrcDesc.Rows.Count > 0 && tbl.Rows[i]["itgrpcd"].ToString() == "G001" && doctotprint == false)
                            //{
                            //    var DATA = (from DataRow DR in rsStkPrcDesc.Rows where DR["autoitcd"].ToString() == auto1 + tbl.Rows[i]["itcd"].ToString() select DR["stkprcdesc"].ToString()).ToList();
                            //    if (DATA.Count > 0) dr1["stkprcdesc"] = DATA[0];
                            //}
                            dr1["gocd"] = tbl.Rows[i]["gocd"].ToString();
                            dr1["gonm"] = tbl.Rows[i]["gonm"].ToString();
                            dr1["goadd1"] = tbl.Rows[i]["goadd1"].ToString();
                            //dr1["weekno"] = tbl.Rows[i]["weekno"] == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["weekno"]);
                            string cfld = "", rfld = ""; int rf = 0;
                            if (tbl.Rows[i]["prtynm"].retStr() != "")
                            {
                                dr1["slnm"] = tbl.Rows[i]["prtynm"].ToString();

                                for (int f = 1; f <= 6; f++)
                                {
                                    cfld = "prtyadd" + Convert.ToString(f).ToString();
                                    if (tbl.Rows[i][cfld].ToString() != "")
                                    {
                                        rf = rf + 1;
                                        rfld = "sladd" + Convert.ToString(rf);
                                        dr1[rfld] = tbl.Rows[i][cfld].ToString();
                                    }
                                }
                                rf = rf + 1;
                                rfld = "sladd" + Convert.ToString(rf);
                                dr1[rfld] = tbl.Rows[i]["state"].ToString() + " [ Code - " + tbl.Rows[i]["statecd"].ToString() + " ]";
                                if (tbl.Rows[i]["gstno"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "sladd" + Convert.ToString(rf);
                                    dr1[rfld] = "GST # " + tbl.Rows[i]["prtygstno"].ToString();
                                }
                                if (tbl.Rows[i]["panno"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "sladd" + Convert.ToString(rf);
                                    dr1[rfld] = "PAN # " + tbl.Rows[i]["panno"].ToString();
                                }
                                if (tbl.Rows[i]["prtymob"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "sladd" + Convert.ToString(rf);
                                    dr1[rfld] = "Ph. # " + tbl.Rows[i]["prtymob"].ToString();
                                }
                                if (tbl.Rows[i]["slactnameof"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "sladd" + Convert.ToString(rf);
                                    dr1[rfld] = tbl.Rows[i]["slactnameof"].ToString();
                                }
                            }
                            else
                            {
                                dr1["slcd"] = tbl.Rows[i]["RTDEBCD"].ToString();
                                dr1["slnm"] = tbl.Rows[i]["RTDEBNM"].ToString();
                                dr1["regemailid"] = tbl.Rows[i]["rtdebemail"].ToString();

                                for (int f = 1; f <= 6; f++)
                                {
                                    cfld = "rtdebadd" + Convert.ToString(f).ToString();
                                    if (tbl.Rows[i][cfld].ToString() != "")
                                    {
                                        rf = rf + 1;
                                        rfld = "sladd" + Convert.ToString(rf);
                                        dr1[rfld] = tbl.Rows[i][cfld].ToString();
                                    }
                                }
                                rf = rf + 1;
                                rfld = "sladd" + Convert.ToString(rf);
                                dr1[rfld] = tbl.Rows[i]["rtdebstnm"].ToString() + " [ Code - " + tbl.Rows[i]["rtdebstcd"].ToString() + " ]";
                                if (tbl.Rows[i]["gstno"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "sladd" + Convert.ToString(rf);
                                    dr1[rfld] = "GST # " + tbl.Rows[i]["prtygstno"].ToString();
                                }
                                if (tbl.Rows[i]["panno"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "sladd" + Convert.ToString(rf);
                                    dr1[rfld] = "PAN # " + tbl.Rows[i]["panno"].ToString();
                                }
                                if (tbl.Rows[i]["phno"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "sladd" + Convert.ToString(rf);
                                    dr1[rfld] = "Ph. # " + tbl.Rows[i]["rtdebmob"].ToString();
                                }
                                if (tbl.Rows[i]["slactnameof"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "sladd" + Convert.ToString(rf);
                                    dr1[rfld] = tbl.Rows[i]["slactnameof"].ToString();
                                }
                            }



                            //dr1["slcd"] = tbl.Rows[i]["slcd"].ToString();
                            //dr1["slnm"] = tbl.Rows[i]["slnm"].ToString();
                            //dr1["regemailid"] = tbl.Rows[i]["regemailid"].ToString();
                            ////  string cfld = "", rfld = ""; int rf = 0;
                            //for (int f = 1; f <= 6; f++)
                            //{
                            //    cfld = "sladd" + Convert.ToString(f).ToString();
                            //    if (tbl.Rows[i][cfld].ToString() != "")
                            //    {
                            //        rf = rf + 1;
                            //        rfld = "sladd" + Convert.ToString(rf);
                            //        dr1[rfld] = tbl.Rows[i][cfld].ToString();
                            //    }
                            //}
                            //rf = rf + 1;
                            //rfld = "sladd" + Convert.ToString(rf);
                            //dr1[rfld] = tbl.Rows[i]["state"].ToString() + " [ Code - " + tbl.Rows[i]["statecd"].ToString() + " ]";
                            //if (tbl.Rows[i]["gstno"].ToString() != "")
                            //{
                            //    rf = rf + 1;
                            //    rfld = "sladd" + Convert.ToString(rf);
                            //    dr1[rfld] = "GST # " + tbl.Rows[i]["gstno"].ToString();
                            //}
                            //if (tbl.Rows[i]["panno"].ToString() != "")
                            //{
                            //    rf = rf + 1;
                            //    rfld = "sladd" + Convert.ToString(rf);
                            //    dr1[rfld] = "PAN # " + tbl.Rows[i]["panno"].ToString();
                            //}
                            //if (tbl.Rows[i]["phno"].ToString() != "")
                            //{
                            //    rf = rf + 1;
                            //    rfld = "sladd" + Convert.ToString(rf);
                            //    dr1[rfld] = "Ph. # " + tbl.Rows[i]["phno"].ToString();
                            //}
                            //if (tbl.Rows[i]["slactnameof"].ToString() != "")
                            //{
                            //    rf = rf + 1;
                            //    rfld = "sladd" + Convert.ToString(rf);
                            //    dr1[rfld] = tbl.Rows[i]["slactnameof"].ToString();
                            //}

                            // Consignee
                            cfld = ""; rfld = ""; rf = 0;
                            bool conslcdprn = true;
                            if (tbl.Rows[i]["cslcd"].ToString() == tbl.Rows[i]["slcd"].ToString() && tbl.Rows[i]["othadd1"].ToString() != "") conslcdprn = false;

                            if (conslcdprn == true)
                            {
                                dr1["cslcd"] = tbl.Rows[i]["cslcd"].ToString();
                                dr1["cpartycd"] = tbl.Rows[i]["cpartycd"].ToString();
                                dr1["cslnm"] = tbl.Rows[i]["cslnm"].ToString();
                                for (int f = 1; f <= 6; f++)
                                {
                                    cfld = "csladd" + Convert.ToString(f).ToString();
                                    if (tbl.Rows[i][cfld].ToString() != "")
                                    {
                                        rf = rf + 1;
                                        rfld = "csladd" + Convert.ToString(rf);
                                        dr1[rfld] = tbl.Rows[i][cfld].ToString();
                                    }
                                }
                                rf = rf + 1;
                                rfld = "csladd" + Convert.ToString(rf);
                                dr1[rfld] = tbl.Rows[i]["cstate"].ToString() + " [ Code - " + tbl.Rows[i]["cstatecd"].ToString() + " ]";
                                if (tbl.Rows[i]["cgstno"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "csladd" + Convert.ToString(rf);
                                    dr1[rfld] = "GST # " + tbl.Rows[i]["cgstno"].ToString();
                                }
                                if (tbl.Rows[i]["cpanno"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "csladd" + Convert.ToString(rf);
                                    dr1[rfld] = "PAN # " + tbl.Rows[i]["cpanno"].ToString();
                                }
                                if (tbl.Rows[i]["cphno"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "csladd" + Convert.ToString(rf);
                                    dr1[rfld] = "Ph. # " + tbl.Rows[i]["cphno"].ToString();
                                }
                                if (tbl.Rows[i]["cslactnameof"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "csladd" + Convert.ToString(rf);
                                    dr1[rfld] = tbl.Rows[i]["cslactnameof"].ToString();
                                }
                            }
                            else if (tbl.Rows[i]["othadd1"].ToString() != "")
                            {
                                dr1["cslcd"] = ""; tbl.Rows[i]["slcd"].ToString();
                                dr1["cpartycd"] = ""; tbl.Rows[i]["slcd"].ToString();
                                dr1["cslnm"] = tbl.Rows[i]["othnm"] == DBNull.Value ? tbl.Rows[i]["slnm"].ToString() : tbl.Rows[i]["othnm"].ToString();
                                for (int f = 1; f <= 3; f++)
                                {
                                    cfld = "othadd" + Convert.ToString(f).ToString();
                                    if (tbl.Rows[i][cfld].ToString() != "")
                                    {
                                        rf = rf + 1;
                                        rfld = "csladd" + Convert.ToString(rf);
                                        dr1[rfld] = tbl.Rows[i][cfld].ToString();
                                    }
                                }
                                rf = rf + 1;
                                rfld = "csladd" + Convert.ToString(rf);
                                dr1[rfld] = tbl.Rows[i]["cstate"].ToString() + " [ Code - " + tbl.Rows[i]["cstatecd"].ToString() + " ]";
                                if (tbl.Rows[i]["cgstno"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "csladd" + Convert.ToString(rf);
                                    dr1[rfld] = "GST # " + tbl.Rows[i]["cgstno"].ToString();
                                }
                                if (tbl.Rows[i]["cpanno"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "csladd" + Convert.ToString(rf);
                                    dr1[rfld] = "PAN # " + tbl.Rows[i]["cpanno"].ToString();
                                }
                            }

                            dr1["porefno"] = tbl.Rows[i]["porefno"].ToString();
                            dr1["porefdt"] = tbl.Rows[i]["porefdt"] == DBNull.Value ? "" : tbl.Rows[i]["porefdt"].retDateStr();
                            dr1["trslcd"] = tbl.Rows[i]["trslcd"].ToString();
                            dr1["trslnm"] = tbl.Rows[i]["trslnm"].ToString();
                            dr1["trsladd1"] = tbl.Rows[i]["trsladd1"].ToString();
                            dr1["trsladd2"] = tbl.Rows[i]["trsladd2"].ToString();
                            dr1["trsladd3"] = tbl.Rows[i]["trsladd3"].ToString();
                            dr1["trsladd4"] = tbl.Rows[i]["trslphno"].ToString();
                            dr1["trgst"] = tbl.Rows[i]["trgst"].ToString();
                            dr1["lrno"] = tbl.Rows[i]["lrno"].ToString();
                            dr1["lrdt"] = tbl.Rows[i]["lrdt"] == DBNull.Value ? "" : tbl.Rows[i]["lrdt"].retDateStr(); /*tbl.Rows[i]["lrdt"].ToString().Substring(0, 10).ToString();*/
                            dr1["lorryno"] = tbl.Rows[i]["lorryno"].ToString();
                            dr1["ewaybillno"] = tbl.Rows[i]["ewaybillno"].ToString();
                            dr1["grwt"] = tbl.Rows[i]["grwt"] == DBNull.Value ? 0 : tbl.Rows[i]["grwt"].retDbl();
                            dr1["ntwt"] = tbl.Rows[i]["ntwt"] == DBNull.Value ? 0 : tbl.Rows[i]["ntwt"].retDbl();
                            dr1["agentnm"] = tbl.Rows[i]["agslnm"].ToString();

                            dr1["revchrg"] = "N";
                            dr1["roamt"] = tbl.Rows[i]["roamt"] == DBNull.Value ? 0 : tbl.Rows[i]["roamt"].retDbl();
                            dr1["tcsper"] = tbl.Rows[i]["tcsper"].ToString().retDbl().ToINRFormat();
                            dr1["tcsamt"] = tbl.Rows[i]["tcsamt"].ToString().retDbl().ToINRFormat(); // == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["tcsamt"]);
                            dr1["blamt"] = tbl.Rows[i]["blamt"].retDbl().ToINRFormat(); // == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["blamt"]);
                            var netpaybleamt = tbl.Rows[i]["blamt"].ToString().retDbl() - tpamt;
                            rupinwords = Cn.AmountInWords(tbl.Rows[i]["blamt"].retStr());
                            //dr1["blamt"] = (tbl.Rows[i]["blamt"].retDbl() - tbl.Rows[i]["ADVRECDAMT"].retDbl()).ToINRFormat();

                            dr1["rupinword"] = rupinwords;
                            dr1["agdocno"] = tbl.Rows[i]["agdocno"].ToString();
                            dr1["agdocdt"] = tbl.Rows[i]["agdocdt"] == DBNull.Value ? "" : tbl.Rows[i]["agdocdt"].ToString().Substring(0, 10).ToString();
                            blrem = "";
                            //if (tbl.Rows[i]["sapopdno"].ToString() != "") blrem = blrem + "ODP No. " + tbl.Rows[i]["sapopdno"].ToString() + "  ";
                            //if (tbl.Rows[i]["sapblno"].ToString() != "") blrem = blrem + "SAP Bill # " + tbl.Rows[i]["sapblno"].ToString() + "  ";
                            //if (tbl.Rows[i]["sapshipno"].ToString() != "") blrem = blrem + "SAP Shipment # " + tbl.Rows[i]["sapshipno"].ToString() + "  ";
                            if (tbl.Rows[i]["docrem"].ToString() != "") blrem = blrem + tbl.Rows[i]["docrem"].ToString() + "  ";
                            dr1["docth"] = tbl.Rows[i]["docth"];
                            //dr1["nopkgs"] = tbl.Rows[i]["nopkgs"];
                            dr1["blremarks"] = blrem;

                            //dr1["precarr"] = tbl.Rows[i]["precarr"];
                            //dr1["precarrrecpt"] = tbl.Rows[i]["precarrrecpt"];
                            //dr1["shipmarkno"] = tbl.Rows[i]["shipmarkno"];
                            //dr1["portload"] = tbl.Rows[i]["portload"];
                            //dr1["portdesc"] = tbl.Rows[i]["portdesc"];
                            //dr1["finaldest"] = tbl.Rows[i]["finaldest"];
                            //dr1["bankinter"] = tbl.Rows[i]["bankinter"];

                            //Bank Detals
                            dr1["bankactno"] = bankactno;
                            dr1["bankname"] = bankname;
                            dr1["bankifsc"] = bankifsc;
                            dr1["bankbranch"] = bankbranch;
                            dr1["bankadd"] = bankadd;
                            dr1["bankrtgs"] = bankrtgs;

                            //dr1["dtldsc"] = dtldsc;
                            //dr1["dtlamt"] = dtlamt;
                            dr1["dtldsc"] = dtldsc;
                            dr1["dtlamt"] = dtlamt;
                            if (netpaybleamt.retDbl() > 0)
                            { dr1["netpaybledesc"] = "Net Due Payble Amount"; dr1["netpayble"] = netpaybleamt.retDbl().ToINRFormat(); }
                            else { dr1["netpaybledesc"] = "Net Advance Received Amount"; dr1["netpayble"] = netpaybleamt.retDbl().ToINRFormat(); }

                            dr1["user_id"] = tbl.Rows[i]["usr_id"].ToString();
                            dr1["curr_cd"] = tbl.Rows[i]["curr_cd"].ToString();
                            dr1["hsn_cd"] = hsncode;

                            if (flagi == true)
                            {
                                dr1["hsn_hddsp1"] = "IGST";
                            }
                            else
                            {
                                if (flagc == true)
                                {
                                    dr1["hsn_hddsp1"] = "CGST";
                                }
                                else
                                {
                                    dr1["hsn_hddsp1"] = "";
                                }
                            }
                            dr1["hsn_hddsp2"] = flags == true ? "SGST" : "";
                            dr1["hsn_txblval"] = hsntaxblval;
                            dr1["hsn_gstper1"] = hsngstper1;
                            dr1["hsn_gstamt1"] = hsngstamt1;
                            dr1["hsn_gstper2"] = hsngstper2;
                            dr1["hsn_gstamt2"] = hsngstamt2;
                            dr1["hsn_gstper3"] = hsngstper3;
                            dr1["hsn_gstamt3"] = hsngstamt3;
                            dr1["hsn_cessamt"] = "";
                            dr1["hsn_gstamt"] = "";
                            dr1["hsn_qnty"] = hsnqnty;
                            dr1["hsn_tqnty"] = total_qnty;
                            dr1["hsn_ttxblval"] = total_taxval.ToINRFormat();
                            dr1["hsn_tgstamt1"] = total_gstamt1.ToINRFormat();
                            dr1["hsn_tgstamt2"] = total_gstamt2.ToINRFormat();
                            dr1["hsn_tgstamt3"] = total_gstamt3.ToINRFormat();
                            if (totalosamt != 0) dr1["totalosamt"] = totalosamt.ToINRFormat();
                            //dr1["destn"] = tbl.Rows[i]["destn"];
                            //dr1["plsupply"] = tbl.Rows[i]["plsupply"];
                            //dr1["poslno"] = tbl.Rows[i]["poslno"];
                            //dr1["mtrlcd"] = tbl.Rows[i]["mtrlcd"];
                            //dr1["bas_rate"] = Convert.ToDouble(tbl.Rows[i]["bas_rate"] == DBNull.Value ? 0 : tbl.Rows[i]["bas_rate"]).ToString("0.00");
                            //dr1["pv_per"] = tbl.Rows[i]["pv_per"].ToString() == "" ? "" : tbl.Rows[i]["pv_per"].ToString() + " %";
                            //if (tbl.Rows[i]["insby"].ToString().retStr() == "Y") dr1["insudesc"] = inspoldesc;
                            dr1["dealsin"] = dealsin;
                            dr1["blterms"] = blterms;

                            if (doctotprint == false)
                            {
                                itdsc = "";
                                if (tbl.Rows[i]["itcd"].ToString() != "")
                                {
                                    lslno = lslno + 1;
                                    delvchrg = false;
                                }
                                else
                                {
                                    lslno = 0;
                                    delvchrg = true;
                                }
                                if (tbl.Rows[i]["itrem"].ToString() != "") itdsc = tbl.Rows[i]["itrem"].ToString();
                                //if (Convert.ToDouble(tbl.Rows[i]["nosinbag"]) != 0)
                                //{
                                //    double dbnopcks = Convert.ToDouble(tbl.Rows[i]["nosinbag"]) * Convert.ToDouble(tbl.Rows[i]["nos"]);
                                //    itdsc = itdsc + "CLD: " + Convert.ToDouble(tbl.Rows[i]["nosinbag"]).ToString("0") + " NOPCKS: " + dbnopcks.ToString();
                                //}
                                if (itdsc == "" && CommVar.ClientCode(UNQSNO) == "RATN") itdsc = tbl.Rows[i]["itnm"].ToString();
                                //if (tbl.Rows[i]["prodcd"].ToString() != "") itdsc = itdsc + "PCD: " + tbl.Rows[i]["prodcd"].ToString() + " ";
                                if (tbl.Rows[i]["batchdlprint"].ToString() == "Y" && tbl.Rows[i]["batchdtl"].ToString() != "") itdsc += "Batch # " + tbl.Rows[i]["batchdtl"].ToString(); else dr1["batchdtl"] = "";
                                if (tbl.Rows[i]["itcd"].ToString() != "") dr1["caltype"] = 1; else dr1["caltype"] = 0;
                                dr1["agdocno"] = tbl.Rows[i]["agdocno"].ToString();
                                dr1["agdocdt"] = tbl.Rows[i]["agdocdt"] == DBNull.Value ? "" : tbl.Rows[i]["agdocdt"].ToString().Substring(0, 10).ToString();
                                dr1["slno"] = lslno;
                                dr1["itcd"] = tbl.Rows[i]["itcd"].ToString();
                                //dr1["prodcd"] = tbl.Rows[i]["prodcd"].ToString();
                                dr1["itnm"] = tbl.Rows[i]["itnm"].ToString() + " " + tbl.Rows[i]["styleno"].ToString();
                                //if (tbl.Rows[i]["damstock"].ToString() == "D")
                                //{
                                //    dr1["itnm"] = dr1["itnm"].ToString() + " [Damage]";
                                //}
                                dr1["itdesc"] = itdsc;
                                //dr1["bltophead"] = tbl.Rows[i]["bltophead"].ToString();
                                //dr1["makenm"] = tbl.Rows[i]["makenm"].ToString();
                                //dr1["mrp"] = tbl.Rows[i]["mrp"];
                                if (tbl.Rows[i]["batchdlprint"].ToString() == "Y" && tbl.Rows[i]["batchdtl"].ToString() != "") dr1["batchdtl"] = "Batch # " + tbl.Rows[i]["batchdtl"].ToString(); else dr1["batchdtl"] = "";
                                dr1["nos"] = tbl.Rows[i]["nos"].ToString();
                                dr1["hsncode"] = tbl.Rows[i]["hsncode"].ToString();
                                //dr1["packsize"] = tbl.Rows[i]["packsize"] == DBNull.Value ? 0 : (tbl.Rows[i]["packsize"]).retDbl();
                                dr1["nos"] = tbl.Rows[i]["nos"] == DBNull.Value ? 0 : (tbl.Rows[i]["nos"]).retDbl();
                                dr1["qnty"] = tbl.Rows[i]["qnty"] == DBNull.Value ? 0 : (tbl.Rows[i]["qnty"]).retDbl();
                                uomdecimal = tbl.Rows[i]["qdecimal"] == DBNull.Value ? 0 : Convert.ToInt16(tbl.Rows[i]["qdecimal"]);
                                string dbqtyu = string.Format("{0:N6}", (dr1["qnty"]).retDbl());
                                if (dbqtyu.Substring(dbqtyu.Length - 2, 2) == "00")
                                {
                                    if (uomdecimal > 4) uomdecimal = 4;
                                }
                                if (uomdecimal > uommaxdecimal) uommaxdecimal = uomdecimal;
                                if (VE.DOCCD == "SOOS" && uomdecimal == 6) uomdecimal = 4;

                                dr1["qdecimal"] = uomdecimal;
                                dr1["uomnm"] = tbl.Rows[i]["uomnm"].ToString();
                                dr1["rate"] = tbl.Rows[i]["rate"].retDbl().ToString("0.00");
                                dr1["amt"] = tbl.Rows[i]["amt"] == DBNull.Value ? 0 : (tbl.Rows[i]["amt"]).retDbl();
                                //dr1["poslno"] = tbl.Rows[i]["poslno"];
                                //dr1["mtrlcd"] = tbl.Rows[i]["mtrlcd"];
                                dr1["curr_cd"] = tbl.Rows[i]["curr_cd"].ToString();
                                //dr1["bas_rate"] = (tbl.Rows[i]["bas_rate"] == DBNull.Value ? 0 : tbl.Rows[i]["bas_rate"]).retDbl().ToString("0.00");
                                //dr1["pv_per"] = tbl.Rows[i]["pv_per"].ToString() == "" ? "" : tbl.Rows[i]["pv_per"].ToString() + " %"; if (tbl.Rows[i]["rateqntybag"].ToString() == "B") dr1["rateuomnm"] = "Case"; else dr1["rateuomnm"] = dr1["uomnm"];
                                string strdsc = "";
                                if (tbl.Rows[i]["tddiscamt"].retDbl() != 0)
                                {
                                    switch (tbl.Rows[i]["tddisctype"].ToString())
                                    {
                                        case "Q":

                                            strdsc = ""; break;
                                        case "N":

                                            strdsc = ""; break;
                                        case "F":
                                            strdsc = "F"; break;
                                        default:
                                            dr1["discper"] = (tbl.Rows[i]["tddiscrate"]).retDbl();
                                            strdsc = (tbl.Rows[i]["tddiscrate"]).retDbl().ToString("0.00") + "%"; break;
                                    }
                                }
                                dr1["stddisc"] = strdsc;
                                dr1["tddiscamt"] = (tbl.Rows[i]["tddiscamt"]).retDbl();
                                if ((tbl.Rows[i]["discamt"]).retDbl() != 0)
                                {
                                    switch (tbl.Rows[i]["disctype"].ToString())
                                    {
                                        case "Q":
                                            strdsc = "Q" + (tbl.Rows[i]["discrate"]).retDbl().ToString("0.00"); break;
                                        case "B":
                                            strdsc = "B" + (tbl.Rows[i]["discrate"]).retDbl().ToString("0.00"); break;
                                        case "F":
                                            strdsc = "F"; break;
                                        case "P":
                                            dr1["discper"] = (tbl.Rows[i]["discrate"]).retDbl();
                                            strdsc = (tbl.Rows[i]["discrate"]).retDbl().ToString("0.00") + "%"; break;
                                    }
                                }
                                dr1["disc"] = strdsc;
                                dr1["titdiscamt"] = (tbl.Rows[i]["discamt"]).retDbl() + (tbl.Rows[i]["tddiscamt"]).retDbl();
                                dr1["discamt"] = (tbl.Rows[i]["discamt"]).retDbl();
                                dr1["txblval"] = ((tbl.Rows[i]["amt"]).retDbl() - (tbl.Rows[i]["tddiscamt"]).retDbl() - (tbl.Rows[i]["discamt"]).retDbl()).ToINRFormat();

                                dr1["cgstdsp"] = flagi == true ? "IGST" : "CGST";
                                dr1["sgstdsp"] = flagc == true ? "SGST" : "";
                                dr1["cgstper"] = (tbl.Rows[i]["cgstper"]).retDbl() + (tbl.Rows[i]["igstper"]).retDbl();
                                dr1["cgstamt"] = (tbl.Rows[i]["cgstamt"]).retDbl() + (tbl.Rows[i]["igstamt"]).retDbl();
                                dr1["sgstper"] = (tbl.Rows[i]["sgstper"]).retDbl();
                                dr1["sgstamt"] = (tbl.Rows[i]["sgstamt"]).retDbl();
                                dr1["cessper"] = (tbl.Rows[i]["cessper"]).retDbl();
                                dr1["cessamt"] = (tbl.Rows[i]["cessamt"]).retDbl();
                                dr1["gstper"] = (tbl.Rows[i]["gstper"]).retDbl();
                                dr1["netamt"] = (dr1["txblval"].ToString()).retDbl() + (dr1["cgstamt"].ToString()).retDbl() + (dr1["sgstamt"].ToString()).retDbl() + (dr1["cessamt"].ToString()).retDbl();
                                //totals
                                dnos = dnos + (dr1["nos"].ToString()).retDbl();
                                dqnty = dqnty + (dr1["qnty"].ToString()).retDbl();
                                dbasamt = dbasamt + (dr1["amt"].ToString()).retDbl();
                                ddisc1 = ddisc1 + (dr1["tddiscamt"].ToString()).retDbl();
                                ddisc2 = ddisc2 + (dr1["discamt"].ToString()).retDbl();
                                dtxblval = dtxblval + (dr1["txblval"].ToString()).retDbl();
                                //if (VE.TEXTBOX6 == "SaleBill_rec.rpt")
                                //{
                                //    if (tbl.Rows[0]["ADVRECDAMT"].retDbl() != 0)
                                //    { dr1["blamt"] = (dtxblval - tbl.Rows[i]["ADVRECDAMT"].retDbl()).ToINRFormat(); }
                                //}
                                dcgstamt = dcgstamt + (dr1["cgstamt"].ToString()).retDbl();
                                dsgstamt = dsgstamt + (dr1["sgstamt"].ToString()).retDbl();
                                dnetamt = dnetamt + (dr1["netamt"].ToString()).retDbl();
                            }
                            IR.Rows.Add(dr1);

                            if (totalreadyprint == false)
                            {
                                if (i == maxR) doctotprint = true;
                                else if (tbl.Rows[i + 1]["autono"].ToString() != auto1) doctotprint = true;
                                else if (tbl.Rows[i]["itcd"].ToString() == "") doctotprint = true;
                            }
                            if (delvchrg == true)
                            {
                                doctotprint = true; totalreadyprint = false; delvchrg = false;
                            }
                            if (CommVar.ClientCode(UNQSNO) == "RATN")
                            {
                                if (dchrg != null && dchrg.Count() > 0)
                                {
                                    if (tbl.Rows[i]["itcd"].ToString() == "")
                                    {
                                        if (doctotprint == true && totalreadyprint == false)
                                        {
                                            dr1 = IR.NewRow();
                                            dr1["menu_para"] = VE.MENU_PARA;
                                            dr1["autono"] = auto1 + copymode;
                                            dr1["copymode"] = copymode;
                                            dr1["docno"] = tbl.Rows[i]["docno"].ToString();
                                            if (CommVar.ClientCode(UNQSNO) == "RATN")
                                            {
                                                dr1["itdesc"] = "Total";
                                            }
                                            else
                                            {
                                                dr1["itnm"] = "Total";
                                            }
                                            dr1["nos"] = dnos;
                                            dr1["qnty"] = dqnty;
                                            if (VE.DOCCD == "SOOS" && uommaxdecimal == 6) uommaxdecimal = 4;
                                            dr1["qdecimal"] = uommaxdecimal;
                                            dr1["amt"] = dbasamt;
                                            dr1["tddiscamt"] = ddisc1;
                                            dr1["discamt"] = ddisc2;
                                            dr1["txblval"] = dtxblval.ToINRFormat();
                                            dr1["cgstamt"] = dcgstamt;
                                            dr1["sgstamt"] = dsgstamt;
                                            dr1["netamt"] = dnetamt;
                                            dr1["titdiscamt"] = ddisc1 + ddisc2;
                                            dr1["curr_cd"] = tbl.Rows[i]["curr_cd"].ToString();
                                            totalreadyprint = true;
                                            goto docstart;
                                        }
                                    }
                                }
                                else
                                {
                                    if (doctotprint == true && totalreadyprint == false)
                                    {
                                        dr1 = IR.NewRow();
                                        dr1["menu_para"] = VE.MENU_PARA;
                                        dr1["autono"] = auto1 + copymode;
                                        dr1["copymode"] = copymode;
                                        dr1["docno"] = tbl.Rows[i]["docno"].ToString();

                                        if (CommVar.ClientCode(UNQSNO) == "RATN")
                                        {
                                            dr1["itdesc"] = "Total";
                                        }
                                        else
                                        {
                                            dr1["itnm"] = "Total";
                                        }
                                        dr1["nos"] = dnos;
                                        dr1["qnty"] = dqnty;
                                        if (VE.DOCCD == "SOOS" && uommaxdecimal == 6) uommaxdecimal = 4;
                                        dr1["qdecimal"] = uommaxdecimal;
                                        dr1["amt"] = dbasamt;
                                        dr1["tddiscamt"] = ddisc1;
                                        dr1["discamt"] = ddisc2;
                                        dr1["txblval"] = dtxblval.ToINRFormat();
                                        dr1["cgstamt"] = dcgstamt;
                                        dr1["sgstamt"] = dsgstamt;
                                        dr1["netamt"] = dnetamt;
                                        dr1["titdiscamt"] = ddisc1 + ddisc2;
                                        dr1["curr_cd"] = tbl.Rows[i]["curr_cd"].ToString();
                                        totalreadyprint = true;
                                        goto docstart;
                                    }
                                }
                            }
                            else
                            {
                                if (doctotprint == true && totalreadyprint == false)
                                {
                                    dr1 = IR.NewRow();
                                    dr1["autono"] = auto1 + copymode;
                                    dr1["copymode"] = copymode;
                                    dr1["docno"] = tbl.Rows[i]["docno"].ToString();

                                    dr1["itnm"] = "Total";
                                    if (CommVar.ClientCode(UNQSNO) == "RATN") dr1["itdesc"] = "Total";
                                    dr1["nos"] = dnos;
                                    dr1["qnty"] = dqnty;
                                    if (VE.DOCCD == "SOOS" && uommaxdecimal == 6) uommaxdecimal = 4;
                                    dr1["qdecimal"] = uommaxdecimal;
                                    dr1["amt"] = dbasamt;
                                    dr1["tddiscamt"] = ddisc1;
                                    dr1["discamt"] = ddisc2;
                                    dr1["txblval"] = dtxblval.ToINRFormat();
                                    dr1["cgstamt"] = dcgstamt;
                                    dr1["sgstamt"] = dsgstamt;
                                    dr1["netamt"] = dnetamt;
                                    dr1["titdiscamt"] = ddisc1 + ddisc2;
                                    dr1["curr_cd"] = tbl.Rows[i]["curr_cd"].ToString();
                                    totalreadyprint = true;
                                    goto docstart;
                                }
                            }
                            doctotprint = false;
                            i = i + 1;
                            ilast = i;
                            if (i > maxR) break;
                        }
                        i = ilast;
                    }
                }
                string compaddress; string stremail = "";
                compaddress = Salesfunc.retCompAddress(gocd, grpemailid);
                stremail = compaddress.retCompValue("email");

                string ccemail = grpemailid;
                if (ccemail == "") ccemail = stremail;

                //ReportDocument reportdocument = new ReportDocument();
                string complogo = Salesfunc.retCompLogo();
                EmailControl EmailControl = new EmailControl();

                string complogosrc = complogo;
                string compfixlogosrc = "c:\\improvar\\" + CommVar.Compcd(UNQSNO) + "fix.jpg";
                string sendemailids = "";
                string rptfile = "SaleBillHalf.rpt";
                if (VE.TEXTBOX6 != null) rptfile = VE.TEXTBOX6;
                rptname = "~/Report/" + rptfile; // "SaleBill.rpt";
                                                 /* if (VE.maxdate == "CHALLAN")*/
                blhead = "CASH MEMO";
                ReportDocument reportdocument = new ReportDocument();
                if (printemail == "Email")
                {
                    var rsemailid = (from DataRow dr in IR.Rows
                                     select new
                                     {
                                         email = dr["regemailid"],
                                         slcd = dr["slcd"]
                                     }).Distinct().ToList();

                    for (int z = 0; z < rsemailid.Count; z++)
                    {
                        if (rsemailid[z].email.ToString() != "")
                        {
                            var queryq = from row in IR.AsEnumerable()
                                         where row.Field<string>("regemailid") == rsemailid[z].email.ToString()
                                         select row;

                            var rsemailid1 = queryq.AsDataView().ToTable();

                            if (doctype == "CINV") reportdocument.Load(Server.MapPath("~/Report/CommSaleBill.rpt"));
                            else reportdocument.Load(Server.MapPath(rptname));

                            maxR = rsemailid1.Rows.Count - 1;
                            Int32 iz = 0;
                            string slnm = "", emlslcd = "", body = "", chkfld = "", chkfld1 = "", ccemailid = "";
                            emlslcd = rsemailid[z].slcd.ToString();
                            DataTable tblslcd = MasterHelpFa.retslcdCont(emlslcd, "S", true);
                            for (int sz = 0; sz <= tblslcd.Rows.Count - 1; sz++)
                            {
                                if (tblslcd.Rows[sz]["regemailid"].ToString() != rsemailid[z].email.ToString())
                                {
                                    ccemailid += ";" + tblslcd.Rows[sz]["regemailid"].ToString();
                                }
                            }
                            while (iz <= maxR)
                            {
                                slnm = rsemailid1.Rows[iz]["slnm"].ToString();
                                emlslcd = rsemailid1.Rows[iz]["slcd"].ToString();
                                body += "<tr>";
                                body += "<td>" + rsemailid1.Rows[iz]["docno"] + "</td>";
                                body += "<td>" + rsemailid1.Rows[iz]["docdt"] + "</td>";
                                body += "<td style='text-align:right'>" + Cn.Indian_Number_format((rsemailid1.Rows[iz]["blamt"]).retDbl().ToString(), "0.00") + "</td>";
                                body += "</tr>";

                                chkfld = rsemailid1.Rows[iz]["autono"].ToString().Substring(0, rsemailid1.Rows[iz]["autono"].ToString().Length - 1);

                                while (rsemailid1.Rows[iz]["autono"].ToString().Substring(0, rsemailid1.Rows[iz]["autono"].ToString().Length - 1) == chkfld)
                                {
                                    iz++;
                                    if (iz > maxR) break;
                                }
                            }
                            string uid = CommVar.UserID();
                            string MOBILE = DB1.USER_APPL.Find(uid).MOBILE;
                            string ldt = rsemailid1.Rows[rsemailid1.Rows.Count - 1]["docdt"].ToString();

                            reportdocument.SetDataSource(rsemailid1);
                            reportdocument.SetParameterValue("billheading", blhead);

                            reportdocument.SetParameterValue("complogo", masterHelp.retCompLogo());
                            reportdocument.SetParameterValue("complogo1", masterHelp.retCompLogo1());
                            reportdocument.SetParameterValue("compnm", compaddress.retCompValue("compnm"));
                            reportdocument.SetParameterValue("compadd", compaddress.retCompValue("compadd"));
                            reportdocument.SetParameterValue("compcommu", compaddress.retCompValue("compcommu"));
                            reportdocument.SetParameterValue("compstat", compaddress.retCompValue("compstat"));
                            reportdocument.SetParameterValue("locaadd", compaddress.retCompValue("locaadd"));
                            reportdocument.SetParameterValue("locacommu", compaddress.retCompValue("locacommu"));
                            reportdocument.SetParameterValue("locastat", compaddress.retCompValue("locastat"));
                            reportdocument.SetParameterValue("legalname", compaddress.retCompValue("legalname"));
                            reportdocument.SetParameterValue("corpadd", compaddress.retCompValue("corpadd"));
                            reportdocument.SetParameterValue("corpcommu", compaddress.retCompValue("corpcommu"));
                            Response.Buffer = false;
                            Response.ClearContent();
                            Response.ClearHeaders();
                            Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                            stream.Seek(0, SeekOrigin.Begin);
                            string path_Save = @"C:\improvar\" + doccd + "-" + emlslcd + "-" + ldt.Substring(6, 4) + ldt.Substring(3, 2) + ldt.Substring(0, 2) + ".pdf";
                            if (System.IO.File.Exists(path_Save))
                            {
                                System.IO.File.Delete(path_Save);
                            }
                            reportdocument.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, path_Save);
                            reportdocument.Close();
                            // email

                            //System.Net.Mail.Attachment attchmail = new System.Net.Mail.Attachment(path_Save);
                            List<System.Net.Mail.Attachment> attchmail = new List<System.Net.Mail.Attachment>();// System.Net.Mail.Attachment(path_Save);
                            attchmail.Add(new System.Net.Mail.Attachment(path_Save));

                            string[,] emlaryBody = new string[7, 2];
                            if (VE.TEXTBOX5 != null)
                            {
                                bool emailsent = EmailControl.SendHtmlFormattedEmail(VE.TEXTBOX5, "", "", emlaryBody, attchmail, grpemailid);
                                if (emailsent == true) sendemailids = sendemailids + VE.TEXTBOX5 + ";"; else sendemailids = " not able to send on " + VE.TEXTBOX5 + ";";
                            }
                            else
                            {
                                emlaryBody[0, 0] = "{slnm}"; emlaryBody[0, 1] = slnm;
                                emlaryBody[1, 0] = "{tbody}"; emlaryBody[1, 1] = body;
                                emlaryBody[2, 0] = "{username}"; emlaryBody[2, 1] = System.Web.HttpContext.Current.Session["UR_NAME"].ToString();
                                emlaryBody[3, 0] = "{compname}"; emlaryBody[3, 1] = compaddress.retCompValue("compnm");
                                emlaryBody[4, 0] = "{usermobno}"; emlaryBody[4, 1] = MOBILE;
                                emlaryBody[5, 0] = "{complogo}"; emlaryBody[5, 1] = complogosrc;
                                emlaryBody[6, 0] = "{compfixlogo}"; emlaryBody[6, 1] = compfixlogosrc;
                                bool emailsent = EmailControl.SendHtmlFormattedEmail(rsemailid[z].email.ToString() + ccemailid, "Sales Bill copy of " + docnm, "Salebill.htm", emlaryBody, attchmail, grpemailid);
                                if (emailsent == true) sendemailids = sendemailids + rsemailid[z].email.ToString() + ";"; else sendemailids = sendemailids + " not able to send on " + rsemailid[z].email.ToString();
                            }
                            System.IO.File.Delete(path_Save);
                            //eof email sending
                        }
                    }
                    reportdocument.Dispose(); GC.Collect();
                    string emailretmsg = "email : " + sendemailids + "<br /> CC email on " + grpemailid;
                    return Content(emailretmsg);
                }
                else
                {
                    if (doctype == "CINV") reportdocument.Load(Server.MapPath("~/Report/CommSaleBill.rpt"));
                    else reportdocument.Load(Server.MapPath(rptname));

                    reportdocument.SetDataSource(IR);
                    reportdocument.SetParameterValue("billheading", blhead);
                    reportdocument.SetParameterValue("complogo", masterHelp.retCompLogo());
                    reportdocument.SetParameterValue("complogo1", masterHelp.retCompLogo1());
                    reportdocument.SetParameterValue("compnm", compaddress.retCompValue("compnm"));
                    reportdocument.SetParameterValue("compadd", compaddress.retCompValue("compadd"));
                    reportdocument.SetParameterValue("compcommu", compaddress.retCompValue("compcommu"));
                    reportdocument.SetParameterValue("compstat", compaddress.retCompValue("compstat"));
                    reportdocument.SetParameterValue("locaadd", compaddress.retCompValue("locaadd"));
                    reportdocument.SetParameterValue("locacommu", compaddress.retCompValue("locacommu"));
                    reportdocument.SetParameterValue("locastat", compaddress.retCompValue("locastat"));
                    reportdocument.SetParameterValue("legalname", compaddress.retCompValue("legalname"));
                    reportdocument.SetParameterValue("corpadd", compaddress.retCompValue("corpadd"));
                    reportdocument.SetParameterValue("corpcommu", compaddress.retCompValue("corpcommu"));
                    reportdocument.SetParameterValue("reptype", VE.TEXTBOX7.retStr());

                    if (printemail == "Excel")
                    {
                        string path_Save = @"C:\improvar\" + doccd + VE.FDOCNO + ".xls";
                        string exlfile = doccd + VE.FDOCNO + ".xls";
                        if (System.IO.File.Exists(path_Save))
                        {
                            System.IO.File.Delete(path_Save);
                        }
                        reportdocument.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.Excel, path_Save);
                        byte[] buffer = System.IO.File.ReadAllBytes(path_Save);
                        Response.ClearContent();
                        Response.Buffer = true;
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AddHeader("Content-Disposition", "attachment; filename=" + exlfile);
                        Response.BinaryWrite(buffer);
                        reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
                        Response.Flush();
                        Response.End();
                        return Content("Excel exported sucessfully");
                    }
                    else
                    {
                        Response.Buffer = false;
                        Response.ClearContent();
                        Response.ClearHeaders();
                        Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
                        stream.Seek(0, SeekOrigin.Begin);
                        return new FileStreamResult(stream, "application/pdf");
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult ReportSaleBillPrint(ReportViewinHtml VE, string fdate, string tdate, string fdocno, string tdocno, string COM, string LOC, string yr_cd, string slcd, string doccd, string prnemailid, int maxR, string blhead, string gocd, string grpemailid, string Scm1, string Scmf, string scmI, string[] copyno, string rptname, string printemail, string docnm)
        {
            try
            {
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                string str1 = "";
                DataTable rsTmp;
                string doctype = "";
                str1 = "select doctype from " + Scm1 + ".m_doctype where doccd='" + VE.DOCCD + "'";
                rsTmp = masterHelp.SQLquery(str1);
                doctype = rsTmp.Rows[0]["doctype"].ToString();
                string sql = "";
                string sqlc = "";
                sqlc += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and c.yr_cd='" + yr_cd + "' and ";
                if (fdocno != "") sqlc += "c.doconlyno >= " + fdocno + " and c.doconlyno <= " + tdocno + " and ";
                if (fdate != "") sqlc += "c.docdt >= to_date('" + fdate + "','dd/mm/yyyy') and ";
                if (tdate != "") sqlc += "c.docdt <= to_date('" + tdate + "','dd/mm/yyyy') and ";
                if (slcd != null) sqlc += "b.slcd='" + slcd + "' and ";
                sqlc += "c.doccd = '" + doccd + "' and ";

                sql += " select a.autono, b.doctag, h.doccd, h.docno, h.docdt, b.duedays, h.canc_rem,0 invisstime,'N' batchdlprint,  ";
                sql += " b.gocd, k.gonm, k.goadd1, k.goadd2, k.goadd3, k.gophno, k.goemail, h.usr_id, h.usr_entdt, h.vchrno, nvl(e.pslcd, e.slcd) oslcd, b.slcd, ";
                sql += " nvl(e.fullname, e.slnm) slnm, " + prnemailid + ", e.add1 sladd1, e.add2 sladd2, e.add3 sladd3, e.add4 sladd4, e.add5 sladd5, e.add6 sladd6, e.add7 sladd7,  ";
                sql += " e.gstno, e.panno, trim(e.regmobile || decode(e.regmobile, null, '', ',') || e.slphno || decode(e.phno1, null, '', ',' || e.phno1)) phno, e.state, e.country, e.statecd, e.actnameof slactnameof,  ";
                sql += " nvl(b.conslcd, b.slcd) cslcd, '' cpartycd, nvl(f.fullname, f.slnm) cslnm, f.add1 csladd1, f.add2 csladd2, f.add3 csladd3, f.add4 csladd4, f.add5 csladd5, ";
                sql += " f.add6 csladd6, f.add7 csladd7, nvl(f.gstno, f.gstno) cgstno, nvl(f.panno, f.panno) cpanno,f.actnameof cslactnameof, ";
                sql += " trim(f.regmobile || decode(f.regmobile, null, '', ',') || f.slphno || decode(f.phno1, null, '', ',' || f.phno1)) cphno, f.state cstate, f.statecd cstatecd,  ";
                sql += " c.translcd trslcd, g.slnm trslnm, g.gstno trgst, g.add1 trsladd1, g.add2 trsladd2, g.add3 trsladd3, g.add4 trsladd4, g.phno1 trslphno, c.lrno,  ";
                sql += " c.lrdt, c.lorryno, c.ewaybillno, c.grwt, c.ntwt, a.slno, a.itcd, a.styleno, a.itnm, a.itrem, a.batchdtl, a.hsncode,  ";
                sql += " a.nos, a.qnty, nvl(i.decimals, 0) qdecimal, i.uomnm, a.rate, a.amt, d.docrem, d.docth, d.casenos, d.noofcases,  ";
                sql += " d.agslcd, m.slnm agslnm, a.agdocno, a.agdocdt, j.itgrpnm, j.shortnm,  ";
                sql += " nvl(a.igstper, 0)igstper, nvl(a.igstamt, 0)igstamt, nvl(a.cgstper, 0)cgstper, nvl(a.cgstamt, 0)cgstamt,  ";
                sql += " nvl(a.sgstper, 0)sgstper, nvl(a.sgstamt, 0)sgstamt, nvl(a.dutyper, 0)dutyper, nvl(a.dutyamt, 0)dutyamt, nvl(a.cessper, 0)cessper, nvl(a.cessamt, 0)cessamt,  ";
                sql += " nvl(a.igstper + a.cgstper + a.sgstper, 0) gstper, nvl(b.roamt, 0)roamt, nvl(b.blamt, 0) blamt, nvl(b.tcsper, 0) tcsper, nvl(b.tcsamt, 0) tcsamt, d.insby,  ";
                sql += " d.othnm, nvl(d.othadd1, f.othadd1) othadd1, d.porefno, d.porefdt, d.despby, d.dealby, d.packby, d.selby,  ";
                sql += " decode(d.othadd1, null, f.othadd2, d.othadd2) othadd2, decode(d.othadd1, null, f.othadd3, d.othadd3) othadd3, decode(d.othadd1, null, f.othadd4, d.othadd4) othadd4,  ";
                sql += " z.disctype, z.discrate, z.discamt, z.scmdisctype, z.scmdiscrate, z.scmdiscamt, z.tddisctype, z.tddiscrate, z.tddiscamt,z.totdiscamt,  ";
                sql += "(case when nvl(h.cancel,'N')='Y' then 'C' when r.autono is not null then 'A' ";
                sql += "when nvl(s.einvappl,'N')='Y' and p.irnno is null and e.gstno is not null and s.expcd is null and s.salpur='S' then 'I' end) cancel,p.irnno, ";
                sql += " b.curr_cd,a.listprice,a.listdiscper,p.ackno,to_char(p.ackdt,'dd-mm-yyyy hh24:mi:ss') ackdt from  ";

                sql += " (select a.autono, a.autono || a.slno autoslno, a.slno, a.itcd, d.itnm, o.pdesign, d.styleno, d.uomcd, nvl(a.hsncode, nvl(d.hsncode, f.hsncode)) hsncode,  ";
                //sql += " a.itrem, a.baleno, a.nos, nvl(a.blqnty, a.qnty) qnty, a.flagmtr, a.rate, a.amt, a.agdocno, to_char(a.agdocdt, 'dd/mm/yyyy') agdocdt,  ";
                sql += " a.itrem, a.baleno, a.nos, decode( nvl(a.blqnty, 0),0,a.qnty,nvl(a.blqnty, 0)) qnty, a.flagmtr, a.rate, a.amt, a.agdocno, to_char(a.agdocdt, 'dd/mm/yyyy') agdocdt,  ";
                sql += " listagg(o.barno || ' (' || n.qnty || ')', ', ') within group(order by n.autono, n.slno) batchdtl,  ";
                sql += " a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.dutyper, a.dutyamt, a.cessper, a.cessamt,a.listprice,a.listdiscper  ";
                sql += " from " + Scm1 + ".t_txndtl a, " + Scm1 + ".t_txn b, " + Scm1 + ".t_cntrl_hdr c, " + Scm1 + ".m_sitem d, " + Scm1 + ".m_group f, " + Scm1 + ".t_batchdtl  n, " + Scm1 + ".t_batchmst o  ";
                sql += " where a.autono = b.autono and a.autono = c.autono and a.itcd = d.itcd and a.autono = n.autono(+) and a.slno = n.txnslno(+) and n.barno = o.barno(+) and  ";
                sql += " c.compcd = '" + COM + "' and c.loccd = '" + LOC + "' and c.yr_cd = '" + yr_cd + "' and  ";
                if (fdocno != "") sql += " c.doconlyno >= " + fdocno + " and c.doconlyno <= " + tdocno + " and  ";
                if (fdate != "") sql += " c.docdt >= to_date('" + fdate + "', 'dd/mm/yyyy') and  ";
                if (tdate != "") sql += " c.docdt <= to_date('" + tdate + "', 'dd/mm/yyyy') and  ";
                sql += " c.doccd = '" + doccd + "' and d.itgrpcd = f.itgrpcd(+)  ";
                sql += " group by a.autono, a.autono || a.slno, a.slno, a.itcd, d.itnm,o.pdesign, d.styleno, d.uomcd, nvl(a.hsncode, nvl(d.hsncode, f.hsncode)),  ";
                //sql += " a.itrem, a.baleno, a.nos, nvl(a.blqnty, a.qnty), a.flagmtr, a.rate, a.amt, a.agdocno, to_char(a.agdocdt, 'dd/mm/yyyy'),  ";
                sql += " a.itrem, a.baleno, a.nos, decode( nvl(a.blqnty, 0),0,a.qnty,nvl(a.blqnty, 0)), a.flagmtr, a.rate, a.amt, a.agdocno, to_char(a.agdocdt, 'dd/mm/yyyy'),  ";
                sql += " a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.dutyper, a.dutyamt, a.cessper, a.cessamt,a.listprice,a.listdiscper  ";
                sql += " union all  ";

                sql += " select a.autono, a.autono autoslno, nvl(ascii(d.calccode), 0) + 1000 slno, '' itcd, d.amtnm || ' ' || a.amtdesc itnm,'' pdesign, '' styleno, '' uomcd, a.hsncode hsncode,  ";
                sql += " '' itrem, '' baleno, 0 nos, 0 qnty, 0 flagmtr, a.amtrate rate, a.amt, '' agdocno, '' agdocdt, '' batchdtl,  ";
                sql += " a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.dutyper, a.dutyamt, a.cessper, a.cessamt,0 listprice,0 listdiscper  ";
                sql += " from " + Scm1 + ".t_txnamt a, " + Scm1 + ".t_txn b, " + Scm1 + ".t_cntrl_hdr c, " + Scm1 + ".m_amttype d  ";
                sql += " where a.autono = b.autono and a.autono = c.autono and c.compcd = '" + COM + "' and c.loccd = '" + LOC + "' and c.yr_cd = '" + yr_cd + "' and  ";
                if (fdocno != "") sql += " c.doconlyno >= " + fdocno + " and c.doconlyno <= " + tdocno + " and ";
                if (fdate != "") sql += " c.docdt >= to_date('" + fdate + "', 'dd/mm/yyyy') and  ";
                if (tdate != "") sql += " c.docdt <= to_date('" + tdate + "', 'dd/mm/yyyy') and  ";
                sql += "c.doccd = '" + doccd + "'  ";
                sql += "and a.amtcd = d.amtcd(+)  ";
                sql += " ) a,  ";

                sql += "( select distinct a.autono ";
                sql += "from " + Scm1 + ".t_cntrl_doc_pass a, " + Scm1 + ".t_cntrl_hdr b, " + Scm1 + ".t_cntrl_auth c ";
                sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and c.autono is null and ";
                sql += "b.doccd='" + doccd + "' ) r, ";

                sql += "( select distinct b.autono, e.expcd, e.salpur, decode(nvl(d.einvappl,'N'),'Y',(case when c.docdt > d.einvappldt then 'Y' else 'N' end),d.einvappl) einvappl ";
                sql += "from " + Scm1 + ".t_txn b, " + Scm1 + ".t_cntrl_hdr c, " + Scmf + ".m_comp d, " + Scmf + ".t_vch_gst e ";
                sql += "where b.autono = c.autono(+) and b.autono=e.autono(+) and ";
                sql += sqlc;
                sql += "c.compcd = d.compcd(+) ) s, ";

                sql += " " + Scm1 + ".t_txndtl z, " + Scm1 + ".t_txn b, " + Scm1 + ".t_txntrans c, " + Scm1 + ".t_txnoth d, " + Scmf + ".m_subleg e, " + Scmf + ".m_subleg f, " + Scmf + ".m_subleg g,  ";
                sql += " " + Scm1 + ".t_cntrl_hdr h, " + Scmf + ".m_uom i, " + Scm1 + ".m_group j, " + Scmf + ".m_godown k, " + Scm1 + ".m_sitem l, " + Scmf + ".m_subleg m," + Scmf + ".t_txneinv p  ";
                sql += " where a.autono = z.autono(+) and a.slno = z.slno(+) and a.autono = b.autono and a.autono = c.autono(+) and a.autono = d.autono(+) and  ";
                sql += " b.slcd = e.slcd and nvl(b.conslcd, b.slcd) = f.slcd(+) and c.translcd = g.slcd(+) and a.autono = h.autono and a.itcd = l.itcd(+) and l.itgrpcd = j.itgrpcd(+) and a.uomcd = i.uomcd(+) and  ";
                sql += " b.gocd = k.gocd(+) and d.agslcd = m.slcd(+) and  ";
                sql += "a.autono=r.autono(+) and a.autono=s.autono(+) and a.autono=p.autono(+) and ";
                if (slcd != null) sql += " b.slcd ='" + slcd + "' and ";
                sql += " a.autono not in (select a.autono from " + Scm1 + ".t_cntrl_doc_pass a, " + Scm1 + ".t_cntrl_hdr b, " + Scm1 + ".t_cntrl_auth c  ";
                sql += " where a.autono = b.autono(+) and a.autono = c.autono(+) and c.autono is null and b.doccd = '" + doccd + "' )   ";
                sql += " order by docno,autono,slno  ";
                DataTable tbl = new DataTable();
                tbl = masterHelp.SQLquery(sql);
                if (tbl.Rows.Count == 0) return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "Records not found !!" });

                DataTable rsStkPrcDesc;
                sql = "";
                sql += "select distinct a.autono,a.txnslno,a.barno,a.shade,a.nos,a.qnty,a.flagmtr,a.disctype,a.discrate,a.scmdisctype,a.scmdiscrate, ";
                sql += "a.tddisctype,a.tddiscrate,a.itrem,a.baleno,a.cutlength,a.slno ";
                sql += "from " + Scm1 + ".t_batchdtl a, " + Scm1 + ".t_txn b, " + Scm1 + ".t_cntrl_hdr c ";
                sql += "where  ";
                sql += sqlc;
                sql += "a.autono=b.autono and a.autono=c.autono order by a.autono,a.slno ";
                rsStkPrcDesc = masterHelp.SQLquery(sql);

                string blterms = "", inspoldesc = "", dealsin = "";
                Int16 bankslno = 0;
                sql = "select blterms, inspoldesc, dealsin, nvl(bankslno,0) bankslno from " + Scm1 + ".m_mgroup_spl where compcd='" + CommVar.Compcd(UNQSNO) + "' ";
                DataTable rsMgroupSpl = masterHelp.SQLquery(sql);
                if (rsMgroupSpl.Rows.Count > 0)
                {
                    if (rsMgroupSpl.Rows[0]["blterms"].ToString() != "") blterms = rsMgroupSpl.Rows[0]["blterms"].ToString();
                    inspoldesc = rsMgroupSpl.Rows[0]["inspoldesc"].ToString();
                    dealsin = rsMgroupSpl.Rows[0]["dealsin"].ToString();
                    bankslno = Convert.ToInt16(rsMgroupSpl.Rows[0]["bankslno"]);
                }

                #region  Datatabe IR generate
                DataTable IR = new DataTable();
                IR.Columns.Add("autono", typeof(string), "");
                IR.Columns.Add("copymode", typeof(string), "");
                IR.Columns.Add("docno", typeof(string), "");
                IR.Columns.Add("docdt", typeof(string), "");
                IR.Columns.Add("insby", typeof(string), "");
                IR.Columns.Add("invisstime", typeof(double), "");
                IR.Columns.Add("duedays", typeof(double), "");
                IR.Columns.Add("country", typeof(string), "");
                IR.Columns.Add("itgrpnm", typeof(string), "");
                IR.Columns.Add("shortnm", typeof(string), "");
                IR.Columns.Add("stkprcdesc", typeof(string), "");
                IR.Columns.Add("gocd", typeof(string), "");
                IR.Columns.Add("gonm", typeof(string), "");
                IR.Columns.Add("goadd1", typeof(string), "");
                IR.Columns.Add("goadd2", typeof(string), "");
                IR.Columns.Add("goadd3", typeof(string), "");
                IR.Columns.Add("gophno", typeof(string), "");
                IR.Columns.Add("goemail", typeof(string), "");
                IR.Columns.Add("slcd", typeof(string), "");
                IR.Columns.Add("partycd", typeof(string), "");
                IR.Columns.Add("slnm", typeof(string), "");
                IR.Columns.Add("sladd1", typeof(string), "");
                IR.Columns.Add("sladd2", typeof(string), "");
                IR.Columns.Add("sladd3", typeof(string), "");
                IR.Columns.Add("sladd4", typeof(string), "");
                IR.Columns.Add("sladd5", typeof(string), "");
                IR.Columns.Add("sladd6", typeof(string), "");
                IR.Columns.Add("sladd7", typeof(string), "");
                IR.Columns.Add("sladd8", typeof(string), "");
                IR.Columns.Add("sladd9", typeof(string), "");
                IR.Columns.Add("sladd10", typeof(string), "");
                IR.Columns.Add("othadd1", typeof(string), "");
                IR.Columns.Add("othadd2", typeof(string), "");
                IR.Columns.Add("othadd3", typeof(string), "");
                IR.Columns.Add("othadd4", typeof(string), "");
                IR.Columns.Add("disctype", typeof(string), "");
                IR.Columns.Add("discrate", typeof(double), "");
                IR.Columns.Add("cslcd", typeof(string), "");
                IR.Columns.Add("cpartycd", typeof(string), "");
                IR.Columns.Add("cslnm", typeof(string), "");
                IR.Columns.Add("csladd1", typeof(string), "");
                IR.Columns.Add("csladd2", typeof(string), "");
                IR.Columns.Add("csladd3", typeof(string), "");
                IR.Columns.Add("csladd4", typeof(string), "");
                IR.Columns.Add("csladd5", typeof(string), "");
                IR.Columns.Add("csladd6", typeof(string), "");
                IR.Columns.Add("csladd7", typeof(string), "");
                IR.Columns.Add("csladd8", typeof(string), "");
                IR.Columns.Add("csladd9", typeof(string), "");
                IR.Columns.Add("csladd10", typeof(string), "");
                IR.Columns.Add("porefno", typeof(string), "");
                IR.Columns.Add("porefdt", typeof(string), "");
                IR.Columns.Add("trslcd", typeof(string), "");
                IR.Columns.Add("trslnm", typeof(string), "");
                IR.Columns.Add("trgst", typeof(string), "");
                IR.Columns.Add("lrno", typeof(string), "");
                IR.Columns.Add("lrdt", typeof(string), "");
                IR.Columns.Add("lorryno", typeof(string), "");
                IR.Columns.Add("ewaybillno", typeof(string), "");
                IR.Columns.Add("grwt", typeof(double), "");
                IR.Columns.Add("ntwt", typeof(double), "");
                IR.Columns.Add("caltype", typeof(double), "");
                IR.Columns.Add("slno", typeof(double), "");
                IR.Columns.Add("itcd", typeof(string), "");
                IR.Columns.Add("styleno", typeof(string), "");
                IR.Columns.Add("itnm", typeof(string), "");
                IR.Columns.Add("itrem", typeof(string), "");
                IR.Columns.Add("itdesc", typeof(string), "");
                IR.Columns.Add("batchdtl", typeof(string), "");
                IR.Columns.Add("gstno", typeof(double), "");
                IR.Columns.Add("hsncode", typeof(string), "");
                IR.Columns.Add("nos", typeof(double), "");
                IR.Columns.Add("casenos", typeof(double), "");
                IR.Columns.Add("noofcases", typeof(double), "");
                IR.Columns.Add("qnty", typeof(double), "");
                IR.Columns.Add("uomnm", typeof(string), "");
                IR.Columns.Add("qdecimal", typeof(double), "");
                IR.Columns.Add("rate", typeof(double), "");
                IR.Columns.Add("rateuomnm", typeof(string), "");
                IR.Columns.Add("amt", typeof(double), "");
                IR.Columns.Add("stddisc", typeof(string), "");
                IR.Columns.Add("tddiscamt", typeof(double), "");
                IR.Columns.Add("disc", typeof(string), "");
                IR.Columns.Add("discamt", typeof(double), "");
                IR.Columns.Add("scmdisctype", typeof(string), "");
                IR.Columns.Add("scmdiscrate", typeof(double), "");
                IR.Columns.Add("scmdiscamt", typeof(double), "");
                IR.Columns.Add("tddisctype", typeof(string), "");
                IR.Columns.Add("tddiscrate", typeof(double), "");
                IR.Columns.Add("txblval", typeof(string), "");
                IR.Columns.Add("cgstdsp", typeof(string), "");
                IR.Columns.Add("cgstper", typeof(double), "");
                IR.Columns.Add("cgstamt", typeof(double), "");
                IR.Columns.Add("sgstdsp", typeof(string), "");
                IR.Columns.Add("sgstper", typeof(double), "");
                IR.Columns.Add("sgstamt", typeof(double), "");
                IR.Columns.Add("cessper", typeof(double), "");
                IR.Columns.Add("cessamt", typeof(double), "");
                IR.Columns.Add("gstper", typeof(double), "");
                IR.Columns.Add("discper", typeof(double), "");
                IR.Columns.Add("revchrg", typeof(string), "");
                IR.Columns.Add("rupinword", typeof(string), "");
                IR.Columns.Add("netamt", typeof(double), "");
                IR.Columns.Add("roamt", typeof(double), "");
                IR.Columns.Add("blamt", typeof(string), "");
                IR.Columns.Add("tcsper", typeof(double), "");
                IR.Columns.Add("tcsamt", typeof(double), "");
                IR.Columns.Add("blremarks", typeof(string), "");
                IR.Columns.Add("agdocno", typeof(string), "");
                IR.Columns.Add("agdocdt", typeof(string), "");
                IR.Columns.Add("usr_id", typeof(string), "");
                IR.Columns.Add("usr_entdt", typeof(string), "");
                IR.Columns.Add("vchrno", typeof(string), "");
                IR.Columns.Add("cancel", typeof(string), "");
                IR.Columns.Add("canc_rem", typeof(string), "");
                IR.Columns.Add("titdiscamt", typeof(double), "");
                IR.Columns.Add("oslcd", typeof(string), "");
                IR.Columns.Add("agslcd", typeof(string), "");
                IR.Columns.Add("docth", typeof(string), "");
                IR.Columns.Add("transgst", typeof(string), "");
                IR.Columns.Add("agentnm", typeof(string), "");
                IR.Columns.Add("trsladd1", typeof(string), "");
                IR.Columns.Add("trsladd2", typeof(string), "");
                IR.Columns.Add("trsladd3", typeof(string), "");
                IR.Columns.Add("trsladd4", typeof(string), "");
                IR.Columns.Add("payterms", typeof(string), "");
                IR.Columns.Add("bankactno", typeof(string), "");
                IR.Columns.Add("bankname", typeof(string), "");
                IR.Columns.Add("bankbranch", typeof(string), "");
                IR.Columns.Add("bankifsc", typeof(string), "");
                IR.Columns.Add("bankadd", typeof(string), "");
                IR.Columns.Add("bankrtgs", typeof(string), "");
                IR.Columns.Add("duedt", typeof(string), "");
                IR.Columns.Add("igstper", typeof(double), "");
                IR.Columns.Add("igstamt", typeof(double), "");
                IR.Columns.Add("dutyper", typeof(double), "");
                IR.Columns.Add("dutyamt", typeof(double), "");
                IR.Columns.Add("dtldsc", typeof(string), "");
                IR.Columns.Add("dtlamt", typeof(string), "");
                IR.Columns.Add("despby", typeof(string), "");
                IR.Columns.Add("dealby", typeof(string), "");
                IR.Columns.Add("packby", typeof(string), "");
                IR.Columns.Add("selby", typeof(string), "");
                IR.Columns.Add("dealsin", typeof(string), "");
                IR.Columns.Add("blterms", typeof(string), "");
                IR.Columns.Add("hsn_cd", typeof(string), "");
                IR.Columns.Add("hsn_hddsp1", typeof(string), "");
                IR.Columns.Add("hsn_hddsp2", typeof(string), "");
                IR.Columns.Add("hsn_txblval", typeof(string), "");
                IR.Columns.Add("hsn_gstper1", typeof(string), "");
                IR.Columns.Add("hsn_gstamt1", typeof(string), "");
                IR.Columns.Add("hsn_gstper2", typeof(string), "");
                IR.Columns.Add("hsn_gstamt2", typeof(string), "");
                IR.Columns.Add("hsn_gstper3", typeof(string), "");
                IR.Columns.Add("hsn_gstamt3", typeof(string), "");
                IR.Columns.Add("hsn_cessamt", typeof(string), "");
                IR.Columns.Add("hsn_gstamt", typeof(string), "");
                IR.Columns.Add("hsn_qnty", typeof(string), "");
                IR.Columns.Add("hsn_tqnty", typeof(string), "");
                IR.Columns.Add("hsn_ttxblval", typeof(string), "");
                IR.Columns.Add("hsn_tgstamt1", typeof(string), "");
                IR.Columns.Add("hsn_tgstamt2", typeof(string), "");
                IR.Columns.Add("hsn_tgstamt3", typeof(string), "");
                IR.Columns.Add("totalosamt", typeof(string), "");
                IR.Columns.Add("rateqntybag", typeof(string), "");
                IR.Columns.Add("regemailid", typeof(string), "");
                IR.Columns.Add("menu_para", typeof(string), "");
                IR.Columns.Add("upiimg", typeof(string), "");
                IR.Columns.Add("upidesc", typeof(string), "");
                IR.Columns.Add("curr_cd", typeof(string), "");
                IR.Columns.Add("shipmarkno", typeof(string), "");
                IR.Columns.Add("blitdesc", typeof(string), "");


                IR.Columns.Add("agstdocno", typeof(string), "");
                IR.Columns.Add("agstdocdt", typeof(string), "");
                IR.Columns.Add("makenm", typeof(string), "");
                IR.Columns.Add("areacd", typeof(string), "");
                IR.Columns.Add("itmprccd", typeof(string), "");
                IR.Columns.Add("itmprcdesc", typeof(string), "");
                IR.Columns.Add("prceffdt", typeof(string), "");
                IR.Columns.Add("weekno", typeof(double), "");
                IR.Columns.Add("ordrefno", typeof(string), "");
                IR.Columns.Add("ordrefdt", typeof(string), "");
                IR.Columns.Add("packsize", typeof(double), "");
                IR.Columns.Add("hsnsaccd", typeof(string), "");
                IR.Columns.Add("prodcd", typeof(string), "");
                IR.Columns.Add("basamt", typeof(double), "");
                IR.Columns.Add("stddiscamt", typeof(double), "");
                IR.Columns.Add("taxableval", typeof(string), "");
                IR.Columns.Add("user_id", typeof(string), "");
                IR.Columns.Add("bltophead", typeof(string), "");
                IR.Columns.Add("nopkgs", typeof(string), "");
                IR.Columns.Add("mrp", typeof(double), "");
                IR.Columns.Add("poslno", typeof(string), "");
                IR.Columns.Add("plsupply", typeof(string), "");
                IR.Columns.Add("destn", typeof(string), "");
                IR.Columns.Add("mtrlcd", typeof(string), "");
                IR.Columns.Add("bas_rate", typeof(string), "");
                IR.Columns.Add("pv_per", typeof(string), "");
                IR.Columns.Add("insudesc", typeof(string), "");
                IR.Columns.Add("pvtag", typeof(string), "");
                IR.Columns.Add("precarr", typeof(string), "");
                IR.Columns.Add("precarrrecpt", typeof(string), "");
                IR.Columns.Add("portload", typeof(string), "");
                IR.Columns.Add("portdesc", typeof(string), "");
                IR.Columns.Add("finaldest", typeof(string), "");
                IR.Columns.Add("bankinter", typeof(string), "");
                IR.Columns.Add("pcsdesc", typeof(string), "");
                IR.Columns.Add("QRIMGPATH", typeof(string), "");
                IR.Columns.Add("IRNNO", typeof(string), "");
                IR.Columns.Add("listprice", typeof(double), "");
                IR.Columns.Add("listdiscper", typeof(double), "");
                IR.Columns.Add("upiimgpath", typeof(string), "");
                IR.Columns.Add("ackno", typeof(string), "");
                #endregion

                string bankname = "", bankactno = "", bankbranch = "", bankifsc = "", bankadd = "", bankrtgs = "";
                sql = "";
                sql += "select a.bankname, a.bankactno, a.ifsccode, a.address, a.branch ";
                sql += "from " + Scmf + ".m_loca_ifsc a ";
                sql += "where a.compcd = '" + COM + "' and a.loccd = '" + LOC + "' and ";
                if (bankslno == 0) sql += "a.defltbank = 'T' ";
                else sql += "a.slno=" + bankslno.ToString() + " ";
                DataTable rsbank = masterHelp.SQLquery(sql);
                if (rsbank.Rows.Count > 0)
                {
                    bankrtgs += "You can Make RTGS/NEFT to " + rsbank.Rows[0]["bankname"] + " ( IFSC-" + rsbank.Rows[0]["ifsccode"] + " ) A/c No-" + rsbank.Rows[0]["bankactno"];
                    if (rsbank.Rows[0]["address"].ToString() != "") bankrtgs += " Address - " + rsbank.Rows[0]["address"];
                    bankrtgs += ".";

                    bankname = rsbank.Rows[0]["bankname"].ToString();
                    bankactno = rsbank.Rows[0]["bankactno"].ToString();
                    bankifsc = rsbank.Rows[0]["ifsccode"].ToString();
                    bankbranch = rsbank.Rows[0]["branch"].ToString();
                    bankadd = rsbank.Rows[0]["address"].ToString();
                }

                maxR = tbl.Rows.Count - 1;
                Int32 i = 0; int istore = 0; int lslno = 0; int ilast = 0;
                string auto1 = ""; string copymode = ""; string blrem = ""; string itdsc = "";
                /*string blhead = "";*/
                string fssailicno = ""; /*string grpemailid = "";*/ string goadd = ""/*, gocd = ""*/;
                string rupinwords = "";
                int uomdecimal = 3; int uommaxdecimal = 0;

                switch (tbl.Rows[0]["doctag"].ToString())
                {
                    case "SB":
                        blhead = "TAX INVOICE"; break;
                    case "SD":
                        blhead = "DEBIT NOTE"; break;
                    case "SC":
                        blhead = "CREDIT NOTE"; break;
                    case "SR":
                        blhead = "CREDIT NOTE"; break;
                    case "PR":
                        blhead = "PURCHASE RETURN NOTE"; break;
                    case "PD":
                        blhead = "DEBIT NOTE [Purchase]"; break;
                    case "PC":
                        blhead = "CREDIT NOTE [Purchase]"; break;
                    case "PI":
                        blhead = "PROFORMA INVOICE"; break;
                    case "CI":
                        blhead = "TAX INVOICE"; break;
                    case "SO":
                        blhead = "STOCK TRANSFER"; break;
                    case "PB":
                        blhead = "PURCHASE INVOICE"; break;
                    default: blhead = ""; break;
                }

                Int16 maxCopy = 5;

                while (i <= maxR)
                {

                    //grpemailid = tbl.Rows[i]["grpemailid"].ToString();
                    gocd = tbl.Rows[i]["gocd"].ToString();
                    goadd = tbl.Rows[i]["goadd1"].ToString() + " " + tbl.Rows[i]["goadd2"].ToString() + " " + tbl.Rows[i]["goadd3"].ToString();
                    if (tbl.Rows[i]["gophno"].ToString() != "") goadd = goadd + " Phone : " + tbl.Rows[i]["gophno"].ToString();
                    istore = i;
                    string docno = "";
                    for (int ic = 0; ic <= maxCopy; ic++)
                    {
                        i = istore;
                        lslno = 0;
                        auto1 = tbl.Rows[i]["autono"].ToString();
                        double dbasamt = 0; double ddisc1 = 0; double ddisc2 = 0; double ddisc3 = 0; double dtxblval = 0;
                        double dcgstamt = 0; double dsgstamt = 0; double dnetamt = 0; double dnos = 0; double dqnty = 0;
                        bool doctotprint = false; bool totalreadyprint = false; bool delvchrg = false;

                        string dtldsc = "", dtlamt = "";
                        double tqnty = 0, tnos = 0, tamt = 0, tgst = 0, blamt = 0, totalosamt = 0;
                        string hsnqnty = "", hsntaxblval = "", hsngstper1 = "", hsngstper2 = "", hsngstper3 = "", hsngstamt1 = "", hsngstamt2 = "", hsngstamt3 = "", hsncode = "";
                        double gstper1 = 0, gstamt1 = 0, total_qnty = 0, total_taxval = 0, total_gstamt1 = 0, total_gstamt2 = 0, total_gstamt3 = 0;
                        bool flagi = false, flagc = false, flags = false;

                        if (copyno[ic].ToString() != "N")
                        {
                            //rupinwords = Cn.AmountInWords((tbl.Rows[i]["blamt"].retDbl() - tbl.Rows[i]["advrecdamt"].retDbl()).ToString(), tbl.Rows[i]["curr_cd"].ToString());
                            rupinwords = Cn.AmountInWords(tbl.Rows[i]["blamt"].retStr());
                            string oslcd = "", oglcd = "", odocdt = "", oclass1cd = "";

                            if (doctype == "SBILL" && VE.Checkbox7 == true)
                            {
                                oslcd = tbl.Rows[i]["oslcd"].ToString();
                                oglcd = tbl.Rows[i]["debglcd"].ToString();
                                odocdt = Convert.ToDateTime(tbl.Rows[i]["docdt"].ToString()).AddDays(-1).ToString().retDateStr();
                                totalosamt = Convert.ToDouble(MasterHelpFa.slcdbal(oslcd, oglcd, odocdt, oclass1cd));
                                oslcd.retStr();

                                sql = "";
                                sql += "select sum(nvl(a.blamt,0)) blamt from ( ";
                                sql += "select nvl(b.pslcd,a.slcd) oslcd, sum(nvl(a.blamt,0)) blamt ";
                                sql += "from " + Scm1 + ".t_txn a, " + Scmf + ".m_subleg b, " + Scm1 + ".t_cntrl_hdr c, " + Scm1 + ".m_doctype d ";
                                sql += "where a.autono=c.autono and c.doccd=d.doccd and a.slcd=b.slcd and nvl(c.cancel,'N')='N' and c.compcd='" + COM + "' and c.yr_cd='" + yr_cd + "' and ";
                                sql += "c.docdt=to_date('" + tbl.Rows[i]["docdt"].ToString().retDateStr() + "','dd/mm/yyyy') and ";
                                sql += "d.doctype='SBILL' and c.vchrno <= " + Convert.ToDouble(tbl.Rows[i]["vchrno"]) + " and c.doccd='" + doccd + "' ";
                                sql += "group by nvl(b.pslcd,a.slcd) ) a where oslcd='" + oslcd + "'";
                                rsTmp = MasterHelpFa.SQLquery(sql);
                                if (rsTmp.Rows.Count > 0) totalosamt = totalosamt + Convert.ToDouble(rsTmp.Rows[0]["blamt"] == DBNull.Value ? 0 : rsTmp.Rows[0]["blamt"]);
                            }

                            Type A_T = tbl.Rows[0]["amt"].GetType(); Type Q_T = tbl.Rows[0]["qnty"].GetType(); Type N_S = tbl.Rows[0]["nos"].GetType(); Type I_T = tbl.Rows[0]["igstamt"].GetType();
                            Type C_T = tbl.Rows[0]["cgstamt"].GetType(); Type S_T = tbl.Rows[0]["sgstamt"].GetType();

                            var GST_DATA = (from DataRow DR in tbl.Rows
                                            where DR["autono"].ToString() == auto1
                                            group DR by new { IGST = DR["igstper"].ToString(), CGST = DR["cgstper"].ToString(), SGST = DR["sgstper"].ToString() } into X
                                            select new
                                            {
                                                IGSTPER = X.Key.IGST,
                                                CGSTPER = X.Key.CGST,
                                                SGSTPER = X.Key.SGST,
                                                TAMT = (A_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("amt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("amt")))) - X.Sum(Z => Z.Field<double?>("totdiscamt")),
                                                TQNTY = Q_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("qnty")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("qnty"))),
                                                TNOS = N_S.Name == "Double" ? X.Sum(Z => Z.Field<double>("nos")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("nos"))),
                                                IGSTAMT = I_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("igstamt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("igstamt"))),
                                                CGSTAMT = C_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("cgstamt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("cgstamt"))),
                                                SGSTAMT = S_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("sgstamt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("sgstamt"))),
                                                TOTALPER = Convert.ToDouble(X.Key.IGST) + Convert.ToDouble(X.Key.CGST) + Convert.ToDouble(X.Key.SGST),
                                            }).OrderBy(A => A.TOTALPER).ToList();

                            if (GST_DATA != null && GST_DATA.Count > 0)
                            {
                                foreach (var k in GST_DATA)
                                {
                                    if (k.IGSTAMT != 0) { dtldsc += "(+) IGST on " + Cn.Indian_Number_format(k.TAMT.retStr(), "0.00") + " @ " + Cn.Indian_Number_format(k.IGSTPER, "0.00") + " %~"; dtlamt += Convert.ToDouble(k.IGSTAMT).ToINRFormat() + "~"; }
                                    if (k.CGSTAMT != 0) { dtldsc += "(+) CGST on " + Cn.Indian_Number_format(k.TAMT.retStr(), "0.00") + " @ " + Cn.Indian_Number_format(k.CGSTPER, "0.00") + " %~"; dtlamt += Convert.ToDouble(k.CGSTAMT).ToINRFormat() + "~"; }
                                    if (k.SGSTAMT != 0) { dtldsc += "(+) SGST on " + Cn.Indian_Number_format(k.TAMT.retStr(), "0.00") + " @ " + Cn.Indian_Number_format(k.SGSTPER, "0.00") + " %~"; dtlamt += Convert.ToDouble(k.SGSTAMT).ToINRFormat() + "~"; }
                                    tqnty = tqnty + Convert.ToDouble(k.TQNTY);
                                    tnos = tnos + Convert.ToDouble(k.TNOS);
                                    tamt = tamt + Convert.ToDouble(k.TAMT);
                                    tgst = tgst + Convert.ToDouble(k.IGSTAMT) + Convert.ToDouble(k.CGSTAMT) + Convert.ToDouble(k.SGSTAMT);
                                }
                            }
                            //if (tbl.Rows[0]["ADVRECDAMT"].retDbl() != 0)
                            //{
                            //    dtldsc += "(-) " + tbl.Rows[0]["ADVRECDREM"].retStr() + "~";
                            //    dtlamt += tbl.Rows[0]["ADVRECDAMT"].retDbl().ToINRFormat() + "~";
                            //}

                            var HSN_DATA = (from a in DBF.T_VCH_GST
                                            where a.AUTONO == auto1
                                            group a by new { HSNCODE = a.HSNCODE, IGSTPER = a.IGSTPER, CGSTPER = a.CGSTPER, SGSTPER = a.SGSTPER } into x
                                            select new
                                            {

                                                HSNCODE = x.Key.HSNCODE,
                                                IGSTPER = x.Key.IGSTPER,
                                                CGSTPER = x.Key.CGSTPER,
                                                SGSTPER = x.Key.SGSTPER,
                                                TIGSTAMT = x.Sum(s => s.IGSTAMT),
                                                TCGSTAMT = x.Sum(s => s.CGSTAMT),
                                                TSGSTAMT = x.Sum(s => s.SGSTAMT),
                                                TAMT = x.Sum(s => s.AMT),
                                                TQNTY = x.Sum(s => s.QNTY)
                                                //DECIMAL = (from z in DBF.M_UOM
                                                //           where z.UOMCD == (from y in DBF.T_VCH_GST where y.AUTONO == auto1 select y.UOM).FirstOrDefault()
                                                //           select z.DECIMALS).FirstOrDefault()
                                                //DECIMALS = (from c in DBF.M_UOM where c.UOMCD ==  select c.DECIMALS)
                                            }).ToList();

                            if (HSN_DATA != null && HSN_DATA.Count > 0)
                            {
                                foreach (var k in HSN_DATA)
                                {
                                    var uom = (from a in DBF.T_VCH_GST
                                               where a.AUTONO == auto1 && a.IGSTPER == k.IGSTPER && a.CGSTPER == k.CGSTPER
                                      && a.SGSTPER == k.SGSTPER && a.HSNCODE == k.HSNCODE
                                               select a.UOM).FirstOrDefault();
                                    double DECIMAL = 0; string umnm = "";
                                    var uomdata = DBF.M_UOM.Find(uom);
                                    DECIMAL = Convert.ToDouble(uomdata.DECIMALS);
                                    umnm = uomdata.UOMNM;
                                    if (k.TIGSTAMT > 0) flagi = true;
                                    if (k.TCGSTAMT > 0) flagc = true;

                                    gstper1 = Convert.ToDouble(k.CGSTPER) + Convert.ToDouble(k.IGSTPER);
                                    gstamt1 = Convert.ToDouble(k.TCGSTAMT) + Convert.ToDouble(k.TIGSTAMT);

                                    if (k.HSNCODE != "") { hsncode += k.HSNCODE + "~"; }
                                    if (k.TQNTY != 0) { hsnqnty += Convert.ToDouble(k.TQNTY).ToString("n" + DECIMAL.ToString()) + " " + umnm + "~"; }
                                    if (k.TCGSTAMT + k.TIGSTAMT != 0)
                                    {
                                        if (k.IGSTPER != 0) hsngstper1 += Cn.Indian_Number_format(k.IGSTPER.ToString(), "0.00") + " %~";
                                        if (k.TIGSTAMT != 0) hsngstamt1 += Convert.ToDouble(k.TIGSTAMT).ToINRFormat() + "~";
                                    }
                                    else
                                    {
                                        hsngstper1 += "~";
                                        hsngstamt1 += "~";
                                    }
                                    if (k.TCGSTAMT + k.TCGSTAMT != 0)
                                    {
                                        if (k.CGSTPER != 0) hsngstper2 += Cn.Indian_Number_format(k.CGSTPER.ToString(), "0.00") + " %~";
                                        if (k.TCGSTAMT != 0) hsngstamt2 += Convert.ToDouble(k.TCGSTAMT).ToINRFormat() + "~";
                                    }
                                    else
                                    {
                                        hsngstper2 += "~";
                                        hsngstamt2 += "~";
                                    }
                                    if (k.TSGSTAMT != 0)
                                    {
                                        flags = true;
                                        if (k.SGSTPER != 0) hsngstper3 += Cn.Indian_Number_format(k.SGSTPER.ToString(), "0.00") + " %~";
                                        if (k.TSGSTAMT != 0) hsngstamt3 += Convert.ToDouble(k.TSGSTAMT).ToINRFormat() + "~";
                                    }
                                    else
                                    {
                                        hsngstper3 += "~";
                                        hsngstamt3 += "~";
                                    }
                                    if (k.TAMT != 0) { hsntaxblval += Convert.ToDouble(k.TAMT).ToINRFormat() + "~"; } else { hsntaxblval += "~"; }

                                    total_qnty = total_qnty + Convert.ToDouble(k.TQNTY);
                                    total_taxval = total_taxval + Convert.ToDouble(k.TAMT);
                                    total_gstamt1 = total_gstamt1 + Convert.ToDouble(k.TIGSTAMT);
                                    total_gstamt2 = total_gstamt2 + Convert.ToDouble(k.TCGSTAMT);
                                    total_gstamt3 = total_gstamt3 + Convert.ToDouble(k.TSGSTAMT);
                                }
                            }
                        }

                        while (tbl.Rows[i]["autono"].ToString() == auto1)
                        {
                            var dchrg = (from DataRow dr in tbl.Rows
                                         where dr["itcd"].ToString() == "" && dr["autono"].ToString() == auto1
                                         select new
                                         {
                                             itrem = dr["itrem"]
                                         }).ToList();
                            docno = tbl.Rows[i]["docno"].ToString();
                            if (copyno[ic].ToString() == "N")
                            {
                                i = i + 1;
                                break;
                            }
                            switch (ic)
                            {
                                case 0:
                                    copymode = "ORIGINAL FOR RECIPIENT"; break;
                                case 1:
                                    copymode = "DUPLICATE FOR TRANSPORTER"; break;
                                case 2:
                                    copymode = "TRIPLICATE FOR SUPPLIER"; break;
                                case 3:
                                    copymode = "EXTRA COPY"; break;
                                case 4:
                                    copymode = "EXTRA COPY"; break;
                                case 5:
                                    copymode = "EXTRA COPY"; break;
                                default: copymode = ""; break;
                            }

                            DataRow dr1 = IR.NewRow();
                        docstart:
                            double duedays = 0;
                            string payterms = "";
                            duedays = tbl.Rows[i]["duedays"] == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["duedays"]);
                            //payterms = tbl.Rows[i]["payterms"].ToString();
                            if (payterms == "")
                            {
                                if (duedays == 0) payterms = ""; else payterms = duedays.ToString() + " days.";
                            }

                            //dr1["menu_para"] = VE.MENU_PARA;
                            //dr1["pvtag"] = VE.Checkbox7 == true ? "Y" : "N";
                            dr1["menu_para"] = VE.MENU_PARA;
                            //dr1["pvtag"] = tbl.Rows[i]["pv_tag"].ToString();
                            dr1["autono"] = auto1 + ic.ToString();
                            dr1["usr_id"] = tbl.Rows[i]["usr_id"].ToString();
                            dr1["cancel"] = tbl.Rows[i]["cancel"].ToString();
                            dr1["canc_rem"] = tbl.Rows[i]["canc_rem"].ToString();
                            dr1["copymode"] = copymode;
                            dr1["docno"] = tbl.Rows[i]["docno"].ToString();
                            dr1["docdt"] = tbl.Rows[i]["docdt"] == DBNull.Value ? "" : tbl.Rows[i]["docdt"].ToString().Substring(0, 10).ToString();
                            dr1["upiimg"] = "";
                            dr1["upidesc"] = "";
                            //dr1["areacd"] = tbl.Rows[i]["areacd"].ToString();
                            dr1["invisstime"] = tbl.Rows[i]["invisstime"].retDbl();
                            dr1["duedays"] = duedays;
                            //dr1["itmprccd"] = tbl.Rows[i]["itmprccd"].ToString();
                            //dr1["itmprcdesc"] = tbl.Rows[i]["itmprcdesc"].ToString();
                            //dr1["prceffdt"] = tbl.Rows[i]["prceffdt"] == DBNull.Value ? "" : tbl.Rows[i]["prceffdt"].ToString().Substring(0, 10).ToString();
                            dr1["duedt"] = Convert.ToDateTime(tbl.Rows[i]["docdt"].ToString()).AddDays(duedays).ToString().retDateStr();
                            dr1["packby"] = tbl.Rows[i]["packby"].retStr();
                            dr1["selby"] = tbl.Rows[i]["selby"].retStr();
                            dr1["dealby"] = tbl.Rows[i]["dealby"].retStr();
                            dr1["payterms"] = payterms;
                            //if (rsStkPrcDesc.Rows.Count > 0 && tbl.Rows[i]["itgrpcd"].ToString() == "G001" && doctotprint == false)
                            //{
                            //    var DATA = (from DataRow DR in rsStkPrcDesc.Rows where DR["autoitcd"].ToString() == auto1 + tbl.Rows[i]["itcd"].ToString() select DR["stkprcdesc"].ToString()).ToList();
                            //    if (DATA.Count > 0) dr1["stkprcdesc"] = DATA[0];
                            //}
                            dr1["gocd"] = tbl.Rows[i]["gocd"].ToString();
                            dr1["gonm"] = tbl.Rows[i]["gonm"].ToString();
                            dr1["goadd1"] = tbl.Rows[i]["goadd1"].ToString();
                            //dr1["weekno"] = tbl.Rows[i]["weekno"] == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["weekno"]);
                            dr1["irnno"] = tbl.Rows[i]["irnno"].retStr() == "" ? "" : "IRN : " + tbl.Rows[i]["irnno"].ToString();
                            dr1["QRIMGPATH"] = tbl.Rows[i]["IRNNO"].ToString() == "" ? "" : "C:\\IPSMART\\IRNQrcode\\" + tbl.Rows[i]["IRNNO"].ToString() + ".png";
                            dr1["ackno"] = tbl.Rows[i]["ackno"].retStr() == "" ? "" : "Ack # " + tbl.Rows[i]["ackno"].ToString() + "/" + tbl.Rows[i]["ackdt"].ToString();
                            dr1["slcd"] = tbl.Rows[i]["slcd"].ToString();
                            //if (tbl.Rows[i]["partycd"].ToString() != "") dr1["partycd"] = "SAP - " + tbl.Rows[i]["partycd"].ToString();
                            dr1["slnm"] = tbl.Rows[i]["slnm"].ToString();
                            dr1["regemailid"] = tbl.Rows[i]["regemailid"].ToString();


                            string cfld = "", rfld = ""; int rf = 0;
                            for (int f = 1; f <= 6; f++)
                            {
                                cfld = "sladd" + Convert.ToString(f).ToString();
                                if (tbl.Rows[i][cfld].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "sladd" + Convert.ToString(rf);
                                    dr1[rfld] = tbl.Rows[i][cfld].ToString();
                                }
                            }
                            rf = rf + 1;
                            rfld = "sladd" + Convert.ToString(rf);
                            dr1[rfld] = tbl.Rows[i]["state"].ToString() + " [ Code - " + tbl.Rows[i]["statecd"].ToString() + " ]";
                            if (tbl.Rows[i]["gstno"].ToString() != "")
                            {
                                rf = rf + 1;
                                rfld = "sladd" + Convert.ToString(rf);
                                dr1[rfld] = "GST # " + tbl.Rows[i]["gstno"].ToString();
                            }
                            if (tbl.Rows[i]["panno"].ToString() != "")
                            {
                                rf = rf + 1;
                                rfld = "sladd" + Convert.ToString(rf);
                                dr1[rfld] = "PAN # " + tbl.Rows[i]["panno"].ToString();
                            }
                            if (tbl.Rows[i]["phno"].ToString() != "")
                            {
                                rf = rf + 1;
                                rfld = "sladd" + Convert.ToString(rf);
                                dr1[rfld] = "Ph. # " + tbl.Rows[i]["phno"].ToString();
                            }
                            if (tbl.Rows[i]["slactnameof"].ToString() != "")
                            {
                                rf = rf + 1;
                                rfld = "sladd" + Convert.ToString(rf);
                                dr1[rfld] = tbl.Rows[i]["slactnameof"].ToString();
                            }
                            // Consignee
                            cfld = ""; rfld = ""; rf = 0;
                            bool conslcdprn = true;
                            if (tbl.Rows[i]["cslcd"].ToString() == tbl.Rows[i]["slcd"].ToString() && tbl.Rows[i]["othadd1"].ToString() != "") conslcdprn = false;

                            if (conslcdprn == true)
                            {
                                dr1["cslcd"] = tbl.Rows[i]["cslcd"].ToString();
                                dr1["cpartycd"] = tbl.Rows[i]["cpartycd"].ToString();
                                dr1["cslnm"] = tbl.Rows[i]["cslnm"].ToString();
                                for (int f = 1; f <= 6; f++)
                                {
                                    cfld = "csladd" + Convert.ToString(f).ToString();
                                    if (tbl.Rows[i][cfld].ToString() != "")
                                    {
                                        rf = rf + 1;
                                        rfld = "csladd" + Convert.ToString(rf);
                                        dr1[rfld] = tbl.Rows[i][cfld].ToString();
                                    }
                                }
                                rf = rf + 1;
                                rfld = "csladd" + Convert.ToString(rf);
                                dr1[rfld] = tbl.Rows[i]["cstate"].ToString() + " [ Code - " + tbl.Rows[i]["cstatecd"].ToString() + " ]";
                                if (tbl.Rows[i]["cgstno"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "csladd" + Convert.ToString(rf);
                                    dr1[rfld] = "GST # " + tbl.Rows[i]["cgstno"].ToString();
                                }
                                if (tbl.Rows[i]["cpanno"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "csladd" + Convert.ToString(rf);
                                    dr1[rfld] = "PAN # " + tbl.Rows[i]["cpanno"].ToString();
                                }
                                if (tbl.Rows[i]["cphno"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "csladd" + Convert.ToString(rf);
                                    dr1[rfld] = "Ph. # " + tbl.Rows[i]["cphno"].ToString();
                                }
                                if (tbl.Rows[i]["cslactnameof"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "csladd" + Convert.ToString(rf);
                                    dr1[rfld] = tbl.Rows[i]["cslactnameof"].ToString();
                                }
                            }
                            else if (tbl.Rows[i]["othadd1"].ToString() != "")
                            {
                                //dr1["cslcd"] = "";
                                dr1["cslcd"] = tbl.Rows[i]["slcd"].ToString();
                                tbl.Rows[i]["slcd"].ToString();
                                dr1["cpartycd"] = ""; tbl.Rows[i]["slcd"].ToString();
                                dr1["cslnm"] = tbl.Rows[i]["othnm"] == DBNull.Value ? tbl.Rows[i]["slnm"].ToString() : tbl.Rows[i]["othnm"].ToString();
                                for (int f = 1; f <= 3; f++)
                                {
                                    cfld = "othadd" + Convert.ToString(f).ToString();
                                    if (tbl.Rows[i][cfld].ToString() != "")
                                    {
                                        rf = rf + 1;
                                        rfld = "csladd" + Convert.ToString(rf);
                                        dr1[rfld] = tbl.Rows[i][cfld].ToString();
                                    }
                                }
                                rf = rf + 1;
                                rfld = "csladd" + Convert.ToString(rf);
                                dr1[rfld] = tbl.Rows[i]["cstate"].ToString() + " [ Code - " + tbl.Rows[i]["cstatecd"].ToString() + " ]";
                                if (tbl.Rows[i]["cgstno"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "csladd" + Convert.ToString(rf);
                                    dr1[rfld] = "GST # " + tbl.Rows[i]["cgstno"].ToString();
                                }
                                if (tbl.Rows[i]["cpanno"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "csladd" + Convert.ToString(rf);
                                    dr1[rfld] = "PAN # " + tbl.Rows[i]["cpanno"].ToString();
                                }
                            }

                            dr1["porefno"] = tbl.Rows[i]["porefno"].ToString();
                            dr1["porefdt"] = tbl.Rows[i]["porefdt"] == DBNull.Value ? "" : tbl.Rows[i]["porefdt"].retDateStr();
                            dr1["trslcd"] = tbl.Rows[i]["trslcd"].ToString();
                            dr1["trslnm"] = tbl.Rows[i]["trslnm"].ToString();
                            dr1["trsladd1"] = tbl.Rows[i]["trsladd1"].ToString();
                            dr1["trsladd2"] = tbl.Rows[i]["trsladd2"].ToString();
                            dr1["trsladd3"] = tbl.Rows[i]["trsladd3"].ToString();
                            dr1["trsladd4"] = tbl.Rows[i]["trslphno"].ToString();
                            dr1["trgst"] = tbl.Rows[i]["trgst"].ToString();
                            dr1["lrno"] = tbl.Rows[i]["lrno"].ToString();
                            dr1["lrdt"] = tbl.Rows[i]["lrdt"] == DBNull.Value ? "" : tbl.Rows[i]["lrdt"].retDateStr(); /*tbl.Rows[i]["lrdt"].ToString().Substring(0, 10).ToString();*/
                            dr1["lorryno"] = tbl.Rows[i]["lorryno"].ToString();
                            dr1["ewaybillno"] = tbl.Rows[i]["ewaybillno"].ToString();
                            dr1["grwt"] = tbl.Rows[i]["grwt"] == DBNull.Value ? 0 : tbl.Rows[i]["grwt"].retDbl();
                            dr1["ntwt"] = tbl.Rows[i]["ntwt"] == DBNull.Value ? 0 : tbl.Rows[i]["ntwt"].retDbl();
                            dr1["agentnm"] = tbl.Rows[i]["agslnm"].ToString();

                            dr1["revchrg"] = "N";
                            dr1["roamt"] = tbl.Rows[i]["roamt"] == DBNull.Value ? 0 : tbl.Rows[i]["roamt"].retDbl();
                            dr1["tcsper"] = tbl.Rows[i]["tcsper"].ToString().retDbl().ToINRFormat();
                            dr1["tcsamt"] = tbl.Rows[i]["tcsamt"].ToString().retDbl().ToINRFormat(); // == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["tcsamt"]);
                            dr1["blamt"] = tbl.Rows[i]["blamt"].ToString().retDbl().ToINRFormat(); // == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["blamt"]);

                            //dr1["blamt"] = (tbl.Rows[i]["blamt"].retDbl() - tbl.Rows[i]["ADVRECDAMT"].retDbl()).ToINRFormat();

                            dr1["rupinword"] = rupinwords;
                            dr1["agdocno"] = tbl.Rows[i]["agdocno"].ToString();
                            dr1["agdocdt"] = tbl.Rows[i]["agdocdt"] == DBNull.Value ? "" : tbl.Rows[i]["agdocdt"].ToString().Substring(0, 10).ToString();
                            blrem = "";
                            //if (tbl.Rows[i]["sapopdno"].ToString() != "") blrem = blrem + "ODP No. " + tbl.Rows[i]["sapopdno"].ToString() + "  ";
                            //if (tbl.Rows[i]["sapblno"].ToString() != "") blrem = blrem + "SAP Bill # " + tbl.Rows[i]["sapblno"].ToString() + "  ";
                            //if (tbl.Rows[i]["sapshipno"].ToString() != "") blrem = blrem + "SAP Shipment # " + tbl.Rows[i]["sapshipno"].ToString() + "  ";
                            if (tbl.Rows[i]["docrem"].ToString() != "") blrem = blrem + tbl.Rows[i]["docrem"].ToString() + "  ";
                            dr1["docth"] = tbl.Rows[i]["docth"];
                            //dr1["nopkgs"] = tbl.Rows[i]["nopkgs"];
                            dr1["blremarks"] = blrem;

                            //dr1["precarr"] = tbl.Rows[i]["precarr"];
                            //dr1["precarrrecpt"] = tbl.Rows[i]["precarrrecpt"];
                            //dr1["shipmarkno"] = tbl.Rows[i]["shipmarkno"];
                            //dr1["portload"] = tbl.Rows[i]["portload"];
                            //dr1["portdesc"] = tbl.Rows[i]["portdesc"];
                            //dr1["finaldest"] = tbl.Rows[i]["finaldest"];
                            //dr1["bankinter"] = tbl.Rows[i]["bankinter"];

                            //Bank Detals
                            dr1["bankactno"] = bankactno;
                            dr1["bankname"] = bankname;
                            dr1["bankifsc"] = bankifsc;
                            dr1["bankbranch"] = bankbranch;
                            dr1["bankadd"] = bankadd;
                            dr1["bankrtgs"] = bankrtgs;

                            dr1["dtldsc"] = dtldsc;
                            dr1["dtlamt"] = dtlamt;
                            dr1["curr_cd"] = tbl.Rows[i]["curr_cd"].ToString();
                            dr1["hsn_cd"] = hsncode;

                            if (flagi == true)
                            {
                                dr1["hsn_hddsp1"] = "IGST";
                            }
                            else
                            {
                                if (flagc == true)
                                {
                                    dr1["hsn_hddsp1"] = "CGST";
                                }
                                else
                                {
                                    dr1["hsn_hddsp1"] = "";
                                }
                            }
                            dr1["hsn_hddsp2"] = flags == true ? "SGST" : "";
                            dr1["hsn_txblval"] = hsntaxblval;
                            dr1["hsn_gstper1"] = hsngstper1;
                            dr1["hsn_gstamt1"] = hsngstamt1;
                            dr1["hsn_gstper2"] = hsngstper2;
                            dr1["hsn_gstamt2"] = hsngstamt2;
                            dr1["hsn_gstper3"] = hsngstper3;
                            dr1["hsn_gstamt3"] = hsngstamt3;
                            dr1["hsn_cessamt"] = "";
                            dr1["hsn_gstamt"] = "";
                            dr1["hsn_qnty"] = hsnqnty;
                            dr1["hsn_tqnty"] = total_qnty;
                            dr1["hsn_ttxblval"] = total_taxval.ToINRFormat();
                            dr1["hsn_tgstamt1"] = total_gstamt1.ToINRFormat();
                            dr1["hsn_tgstamt2"] = total_gstamt2.ToINRFormat();
                            dr1["hsn_tgstamt3"] = total_gstamt3.ToINRFormat();
                            if (totalosamt != 0) dr1["totalosamt"] = totalosamt.ToINRFormat();
                            //dr1["destn"] = tbl.Rows[i]["destn"];
                            //dr1["plsupply"] = tbl.Rows[i]["plsupply"];
                            //dr1["poslno"] = tbl.Rows[i]["poslno"];
                            //dr1["mtrlcd"] = tbl.Rows[i]["mtrlcd"];
                            //dr1["bas_rate"] = Convert.ToDouble(tbl.Rows[i]["bas_rate"] == DBNull.Value ? 0 : tbl.Rows[i]["bas_rate"]).ToString("0.00");
                            //dr1["pv_per"] = tbl.Rows[i]["pv_per"].ToString() == "" ? "" : tbl.Rows[i]["pv_per"].ToString() + " %";
                            //if (tbl.Rows[i]["insby"].ToString().retStr() == "Y") dr1["insudesc"] = inspoldesc;
                            dr1["dealsin"] = dealsin;
                            dr1["blterms"] = blterms;

                            if (doctotprint == false)
                            {
                                itdsc = "";
                                if (tbl.Rows[i]["itcd"].ToString() != "")
                                {
                                    lslno = lslno + 1;
                                    delvchrg = false;
                                }
                                else
                                {
                                    lslno = 0;
                                    delvchrg = true;
                                }
                                if (tbl.Rows[i]["itrem"].ToString() != "") itdsc = tbl.Rows[i]["itrem"].ToString();
                                //if (Convert.ToDouble(tbl.Rows[i]["nosinbag"]) != 0)
                                //{
                                //    double dbnopcks = Convert.ToDouble(tbl.Rows[i]["nosinbag"]) * Convert.ToDouble(tbl.Rows[i]["nos"]);
                                //    itdsc = itdsc + "CLD: " + Convert.ToDouble(tbl.Rows[i]["nosinbag"]).ToString("0") + " NOPCKS: " + dbnopcks.ToString();
                                //}
                                if (itdsc == "" && CommVar.ClientCode(UNQSNO) == "RATN") itdsc = tbl.Rows[i]["itnm"].ToString();
                                //if (tbl.Rows[i]["prodcd"].ToString() != "") itdsc = itdsc + "PCD: " + tbl.Rows[i]["prodcd"].ToString() + " ";
                                if (tbl.Rows[i]["batchdlprint"].ToString() == "Y" && tbl.Rows[i]["batchdtl"].ToString() != "") itdsc += "Batch # " + tbl.Rows[i]["batchdtl"].ToString(); else dr1["batchdtl"] = "";
                                if (tbl.Rows[i]["itcd"].ToString() != "") dr1["caltype"] = 1; else dr1["caltype"] = 0;
                                dr1["agdocno"] = tbl.Rows[i]["agdocno"].ToString();
                                dr1["agdocdt"] = tbl.Rows[i]["agdocdt"] == DBNull.Value ? "" : tbl.Rows[i]["agdocdt"].ToString().Substring(0, 10).ToString();
                                dr1["slno"] = lslno;
                                dr1["itcd"] = tbl.Rows[i]["itcd"].ToString();
                                //dr1["prodcd"] = tbl.Rows[i]["prodcd"].ToString();
                                //dr1["itnm"] = tbl.Rows[i]["itnm"].ToString() + " " + tbl.Rows[i]["styleno"].ToString();
                                dr1["itnm"] = tbl.Rows[i]["itnm"].ToString();
                                dr1["styleno"] = tbl.Rows[i]["styleno"].ToString();

                                //if (tbl.Rows[i]["damstock"].ToString() == "D")
                                //{
                                //    dr1["itnm"] = dr1["itnm"].ToString() + " [Damage]";
                                //}
                                dr1["itdesc"] = itdsc;
                                //dr1["bltophead"] = tbl.Rows[i]["bltophead"].ToString();
                                //dr1["makenm"] = tbl.Rows[i]["makenm"].ToString();
                                //dr1["mrp"] = tbl.Rows[i]["mrp"];
                                if (tbl.Rows[i]["batchdlprint"].ToString() == "Y" && tbl.Rows[i]["batchdtl"].ToString() != "") dr1["batchdtl"] = "Batch # " + tbl.Rows[i]["batchdtl"].ToString(); else dr1["batchdtl"] = "";
                                dr1["nos"] = tbl.Rows[i]["nos"].ToString();
                                dr1["hsncode"] = tbl.Rows[i]["hsncode"].ToString();
                                //dr1["packsize"] = tbl.Rows[i]["packsize"] == DBNull.Value ? 0 : (tbl.Rows[i]["packsize"]).retDbl();
                                dr1["nos"] = tbl.Rows[i]["nos"] == DBNull.Value ? 0 : (tbl.Rows[i]["nos"]).retDbl();
                                dr1["qnty"] = tbl.Rows[i]["qnty"] == DBNull.Value ? 0 : (tbl.Rows[i]["qnty"]).retDbl();
                                uomdecimal = tbl.Rows[i]["qdecimal"] == DBNull.Value ? 0 : Convert.ToInt16(tbl.Rows[i]["qdecimal"]);
                                string dbqtyu = string.Format("{0:N6}", (dr1["qnty"]).retDbl());
                                if (dbqtyu.Substring(dbqtyu.Length - 2, 2) == "00")
                                {
                                    if (uomdecimal > 4) uomdecimal = 4;
                                }
                                if (uomdecimal > uommaxdecimal) uommaxdecimal = uomdecimal;
                                if (VE.DOCCD == "SOOS" && uomdecimal == 6) uomdecimal = 4;

                                dr1["qdecimal"] = uomdecimal;
                                dr1["uomnm"] = tbl.Rows[i]["uomnm"].ToString();
                                dr1["rate"] = tbl.Rows[i]["rate"].retDbl().ToString("0.00");
                                dr1["amt"] = tbl.Rows[i]["amt"] == DBNull.Value ? 0 : (tbl.Rows[i]["amt"]).retDbl();
                                dr1["listprice"] = tbl.Rows[i]["listprice"].retDbl().ToString("0.00");
                                dr1["listdiscper"] = tbl.Rows[i]["listdiscper"].retDbl().ToString("0.00");
                                #region pcsdescn
                                var batch_data = rsStkPrcDesc.Select("autono='" + auto1 + "' and txnslno = " + tbl.Rows[i]["slno"].ToString());
                                string pcsdesc = "";
                                for (int a = 0; a <= batch_data.Count() - 1; a++)
                                {
                                    pcsdesc += pcsdesc == "" ? "" : ",";
                                    pcsdesc += batch_data[a]["SHADE"].retStr() == "" ? "" : batch_data[a]["SHADE"].retStr() + "/";

                                    //if (batch_data[a]["nos"].retDbl() == 1)
                                    //{
                                    //    pcsdesc += batch_data[a]["cutlength"].retDbl() == 0 ? "" : batch_data[a]["cutlength"].retStr();
                                    //}
                                    //else {
                                    //    if (VE.Checkbox9 == true)
                                    //    {
                                    //        for (int v = 0; v < batch_data[a]["nos"].retDbl(); v++)
                                    //        {

                                    //            pcsdesc += batch_data[a]["cutlength"].retStr() + "+";
                                    //        }
                                    //    }
                                    //    else
                                    //    {
                                    //        pcsdesc += batch_data[a]["cutlength"].retStr() + (batch_data[a]["nos"].retDbl() > 0 ? "x" + batch_data[a]["nos"].retDbl() : "");
                                    //    }
                                    //}
                                    if (batch_data[a]["flagmtr"].retStr() != "")
                                    {
                                        double flagmtr = batch_data[a]["flagmtr"].retDbl() - Math.Truncate(batch_data[a]["flagmtr"].retDbl());
                                        if (flagmtr.retDbl() > 0)
                                        {
                                            pcsdesc += "(F" + flagmtr + ")";
                                        }
                                    }

                                    if (batch_data[a]["scmdiscrate"].retDbl() > 0)
                                    {
                                        pcsdesc += batch_data[a]["scmdiscrate"].retStr() + "% ";
                                    }
                                    if (batch_data[a]["tddiscrate"].retDbl() > 0)
                                    {
                                        pcsdesc += batch_data[a]["tddiscrate"].retStr() + "% ";
                                    }
                                    if (batch_data[a]["discrate"].retDbl() > 0)
                                    {
                                        pcsdesc += batch_data[a]["discrate"].retStr() + "% ";
                                    }
                                    if (batch_data[a]["itrem"].retStr() != "")
                                    {
                                        pcsdesc += "[" + batch_data[a]["itrem"].retStr() + "]";
                                    }
                                    if (batch_data[a]["baleno"].retStr() != "")
                                    {
                                        pcsdesc += "Bale No. " + batch_data[a]["baleno"].retStr();
                                    }
                                }
                                dr1["pcsdesc"] = pcsdesc;
                                #endregion
                                //dr1["poslno"] = tbl.Rows[i]["poslno"];
                                //dr1["mtrlcd"] = tbl.Rows[i]["mtrlcd"];
                                dr1["curr_cd"] = tbl.Rows[i]["curr_cd"].ToString();
                                //dr1["bas_rate"] = (tbl.Rows[i]["bas_rate"] == DBNull.Value ? 0 : tbl.Rows[i]["bas_rate"]).retDbl().ToString("0.00");
                                //dr1["pv_per"] = tbl.Rows[i]["pv_per"].ToString() == "" ? "" : tbl.Rows[i]["pv_per"].ToString() + " %";
                                //if (tbl.Rows[i]["rateqntybag"].ToString() == "B") dr1["rateuomnm"] = "Case"; else dr1["rateuomnm"] = dr1["uomnm"];
                                string strdsc = "";
                                if (tbl.Rows[i]["tddiscamt"].retDbl() != 0)
                                {
                                    switch (tbl.Rows[i]["tddisctype"].ToString())
                                    {
                                        case "Q":

                                            strdsc = ""; break;
                                        case "N":

                                            strdsc = ""; break;
                                        case "F":
                                            strdsc = "F"; break;
                                        default:
                                            dr1["discper"] = (tbl.Rows[i]["tddiscrate"]).retDbl();
                                            strdsc = (tbl.Rows[i]["tddiscrate"]).retDbl().ToString("0.00") + "%"; break;
                                    }
                                }
                                dr1["stddisc"] = strdsc;
                                dr1["tddiscamt"] = (tbl.Rows[i]["tddiscamt"]).retDbl();
                                if ((tbl.Rows[i]["discamt"]).retDbl() != 0)
                                {
                                    switch (tbl.Rows[i]["disctype"].ToString())
                                    {
                                        case "Q":
                                            strdsc = "Q" + (tbl.Rows[i]["discrate"]).retDbl().ToString("0.00"); break;
                                        case "B":
                                            strdsc = "B" + (tbl.Rows[i]["discrate"]).retDbl().ToString("0.00"); break;
                                        case "F":
                                            strdsc = "F"; break;
                                        case "P":
                                            dr1["discper"] = (tbl.Rows[i]["discrate"]).retDbl();
                                            strdsc = (tbl.Rows[i]["discrate"]).retDbl().ToString("0.00") + "%"; break;
                                    }
                                }
                                dr1["disc"] = strdsc;
                                dr1["titdiscamt"] = (tbl.Rows[i]["discamt"]).retDbl() + (tbl.Rows[i]["tddiscamt"]).retDbl() + (tbl.Rows[i]["scmdiscamt"]).retDbl();
                                dr1["discamt"] = (tbl.Rows[i]["discamt"]).retDbl();
                                dr1["txblval"] = ((tbl.Rows[i]["amt"]).retDbl() - (tbl.Rows[i]["tddiscamt"]).retDbl() - (tbl.Rows[i]["discamt"]).retDbl() - (tbl.Rows[i]["scmdiscamt"]).retDbl()).ToINRFormat();

                                dr1["cgstdsp"] = flagi == true ? "IGST" : "CGST";
                                dr1["sgstdsp"] = flagc == true ? "SGST" : "";
                                dr1["cgstper"] = (tbl.Rows[i]["cgstper"]).retDbl() + (tbl.Rows[i]["igstper"]).retDbl();
                                dr1["cgstamt"] = (tbl.Rows[i]["cgstamt"]).retDbl() + (tbl.Rows[i]["igstamt"]).retDbl();
                                dr1["sgstper"] = (tbl.Rows[i]["sgstper"]).retDbl();
                                dr1["sgstamt"] = (tbl.Rows[i]["sgstamt"]).retDbl();
                                dr1["cessper"] = (tbl.Rows[i]["cessper"]).retDbl();
                                dr1["cessamt"] = (tbl.Rows[i]["cessamt"]).retDbl();
                                dr1["gstper"] = (tbl.Rows[i]["gstper"]).retDbl();
                                dr1["netamt"] = (dr1["txblval"].ToString()).retDbl() + (dr1["cgstamt"].ToString()).retDbl() + (dr1["sgstamt"].ToString()).retDbl() + (dr1["cessamt"].ToString()).retDbl();
                                //totals
                                dnos = dnos + (dr1["nos"].ToString()).retDbl();
                                dqnty = dqnty + (dr1["qnty"].ToString()).retDbl();
                                dbasamt = dbasamt + (dr1["amt"].ToString()).retDbl();
                                ddisc1 = ddisc1 + (tbl.Rows[i]["scmdiscamt"]).retDbl();
                                ddisc2 = ddisc2 + (tbl.Rows[i]["tddiscamt"]).retDbl();
                                ddisc3 = ddisc3 + (tbl.Rows[i]["discamt"]).retDbl();
                                dtxblval = dtxblval + (dr1["txblval"].ToString()).retDbl();
                                //if (VE.TEXTBOX6 == "SaleBill_rec.rpt")
                                //{
                                //    if (tbl.Rows[0]["ADVRECDAMT"].retDbl() != 0)
                                //    { dr1["blamt"] = (dtxblval - tbl.Rows[i]["ADVRECDAMT"].retDbl()).ToINRFormat(); }
                                //}
                                dcgstamt = dcgstamt + (dr1["cgstamt"].ToString()).retDbl();
                                dsgstamt = dsgstamt + (dr1["sgstamt"].ToString()).retDbl();
                                dnetamt = dnetamt + (dr1["netamt"].ToString()).retDbl();
                            }
                            IR.Rows.Add(dr1);

                            if (totalreadyprint == false)
                            {
                                if (i == maxR) doctotprint = true;
                                else if (tbl.Rows[i + 1]["autono"].ToString() != auto1) doctotprint = true;
                                else if (tbl.Rows[i + 1]["itcd"].ToString() == "") doctotprint = true;
                            }
                            if (delvchrg == true)
                            {
                                doctotprint = true; totalreadyprint = false; delvchrg = false;
                            }
                            if (CommVar.ClientCode(UNQSNO) == "RATN")
                            {
                                if (dchrg != null && dchrg.Count() > 0)
                                {
                                    if (tbl.Rows[i]["itcd"].ToString() == "")
                                    {
                                        if (doctotprint == true && totalreadyprint == false)
                                        {
                                            dr1 = IR.NewRow();
                                            dr1["menu_para"] = VE.MENU_PARA;
                                            dr1["autono"] = auto1 + copymode;
                                            dr1["copymode"] = copymode;
                                            dr1["docno"] = tbl.Rows[i]["docno"].ToString();
                                            if (CommVar.ClientCode(UNQSNO) == "RATN")
                                            {
                                                dr1["itdesc"] = "Total";
                                            }
                                            else
                                            {
                                                dr1["itnm"] = "Total";
                                            }
                                            dr1["nos"] = dnos;
                                            dr1["qnty"] = dqnty;
                                            if (VE.DOCCD == "SOOS" && uommaxdecimal == 6) uommaxdecimal = 4;
                                            dr1["qdecimal"] = uommaxdecimal;
                                            dr1["amt"] = dbasamt;
                                            dr1["tddiscamt"] = ddisc1;
                                            dr1["discamt"] = ddisc2;
                                            dr1["txblval"] = dtxblval.ToINRFormat();
                                            dr1["cgstamt"] = dcgstamt;
                                            dr1["sgstamt"] = dsgstamt;
                                            dr1["netamt"] = dnetamt;
                                            dr1["titdiscamt"] = ddisc1 + ddisc2 + ddisc3;
                                            dr1["curr_cd"] = tbl.Rows[i]["curr_cd"].ToString();
                                            totalreadyprint = true;
                                            goto docstart;
                                        }
                                    }
                                }
                                else
                                {
                                    if (doctotprint == true && totalreadyprint == false)
                                    {
                                        dr1 = IR.NewRow();
                                        dr1["menu_para"] = VE.MENU_PARA;
                                        dr1["autono"] = auto1 + copymode;
                                        dr1["copymode"] = copymode;
                                        dr1["docno"] = tbl.Rows[i]["docno"].ToString();

                                        if (CommVar.ClientCode(UNQSNO) == "RATN")
                                        {
                                            dr1["itdesc"] = "Total";
                                        }
                                        else
                                        {
                                            dr1["itnm"] = "Total";
                                        }
                                        dr1["nos"] = dnos;
                                        dr1["qnty"] = dqnty;
                                        if (VE.DOCCD == "SOOS" && uommaxdecimal == 6) uommaxdecimal = 4;
                                        dr1["qdecimal"] = uommaxdecimal;
                                        dr1["amt"] = dbasamt;
                                        dr1["tddiscamt"] = ddisc1;
                                        dr1["discamt"] = ddisc2;
                                        dr1["txblval"] = dtxblval.ToINRFormat();
                                        dr1["cgstamt"] = dcgstamt;
                                        dr1["sgstamt"] = dsgstamt;
                                        dr1["netamt"] = dnetamt;
                                        dr1["titdiscamt"] = ddisc1 + ddisc2 + ddisc3;
                                        dr1["curr_cd"] = tbl.Rows[i]["curr_cd"].ToString();
                                        totalreadyprint = true;
                                        goto docstart;
                                    }
                                }
                            }
                            else
                            {
                                if (doctotprint == true && totalreadyprint == false)
                                {
                                    dr1 = IR.NewRow();
                                    dr1["autono"] = auto1 + copymode;
                                    dr1["copymode"] = copymode;
                                    dr1["docno"] = tbl.Rows[i]["docno"].ToString();

                                    dr1["itnm"] = "Total";
                                    if (CommVar.ClientCode(UNQSNO) == "RATN") dr1["itdesc"] = "Total";
                                    dr1["nos"] = dnos;
                                    dr1["qnty"] = dqnty;
                                    if (VE.DOCCD == "SOOS" && uommaxdecimal == 6) uommaxdecimal = 4;
                                    dr1["qdecimal"] = uommaxdecimal;
                                    dr1["amt"] = dbasamt;
                                    dr1["tddiscamt"] = ddisc1;
                                    dr1["discamt"] = ddisc2;
                                    dr1["txblval"] = dtxblval.ToINRFormat();
                                    dr1["cgstamt"] = dcgstamt;
                                    dr1["sgstamt"] = dsgstamt;
                                    dr1["netamt"] = dnetamt;
                                    dr1["titdiscamt"] = ddisc1 + ddisc2 + ddisc3;
                                    dr1["curr_cd"] = tbl.Rows[i]["curr_cd"].ToString();
                                    totalreadyprint = true;
                                    goto docstart;
                                }
                            }
                            doctotprint = false;
                            i = i + 1;
                            ilast = i;
                            if (i > maxR) break;
                        }
                        i = ilast;
                    }
                }
                string compaddress; string stremail = "";
                compaddress = Salesfunc.retCompAddress(gocd, grpemailid);
                stremail = compaddress.retCompValue("email");

                string ccemail = grpemailid;
                if (ccemail == "") ccemail = stremail;

                //ReportDocument reportdocument = new ReportDocument();
                string complogo = Salesfunc.retCompLogo();
                EmailControl EmailControl = new EmailControl();

                string complogosrc = complogo;
                string compfixlogosrc = "c:\\improvar\\" + CommVar.Compcd(UNQSNO) + "fix.jpg";
                string sendemailids = "";
                string rptfile = "SaleBill.rpt";
                if (VE.TEXTBOX6 != null) rptfile = VE.TEXTBOX6;
                rptname = "~/Report/" + rptfile; // "SaleBill.rpt";
                if (VE.maxdate == "CHALLAN") blhead = "CHALLAN";
                ReportDocument reportdocument = new ReportDocument();
                if (printemail == "Email")
                {
                    var rsemailid = (from DataRow dr in IR.Rows
                                     select new
                                     {
                                         email = dr["regemailid"],
                                         slcd = dr["slcd"]
                                     }).Distinct().ToList();

                    for (int z = 0; z < rsemailid.Count; z++)
                    {
                        if (rsemailid[z].email.ToString() != "")
                        {
                            var queryq = from row in IR.AsEnumerable()
                                         where row.Field<string>("regemailid") == rsemailid[z].email.ToString()
                                         select row;

                            var rsemailid1 = queryq.AsDataView().ToTable();

                            if (doctype == "CINV") reportdocument.Load(Server.MapPath("~/Report/CommSaleBill.rpt"));
                            else reportdocument.Load(Server.MapPath(rptname));

                            maxR = rsemailid1.Rows.Count - 1;
                            Int32 iz = 0;
                            string slnm = "", emlslcd = "", body = "", chkfld = "", chkfld1 = "", ccemailid = "";
                            emlslcd = rsemailid[z].slcd.ToString();
                            DataTable tblslcd = MasterHelpFa.retslcdCont(emlslcd, "S", true);
                            for (int sz = 0; sz <= tblslcd.Rows.Count - 1; sz++)
                            {
                                if (tblslcd.Rows[sz]["regemailid"].ToString() != rsemailid[z].email.ToString())
                                {
                                    ccemailid += ";" + tblslcd.Rows[sz]["regemailid"].ToString();
                                }
                            }
                            while (iz <= maxR)
                            {
                                slnm = rsemailid1.Rows[iz]["slnm"].ToString();
                                emlslcd = rsemailid1.Rows[iz]["slcd"].ToString();
                                body += "<tr>";
                                body += "<td>" + rsemailid1.Rows[iz]["docno"] + "</td>";
                                body += "<td>" + rsemailid1.Rows[iz]["docdt"] + "</td>";
                                body += "<td style='text-align:right'>" + Cn.Indian_Number_format((rsemailid1.Rows[iz]["blamt"]).retDbl().ToString(), "0.00") + "</td>";
                                body += "</tr>";

                                chkfld = rsemailid1.Rows[iz]["autono"].ToString().Substring(0, rsemailid1.Rows[iz]["autono"].ToString().Length - 1);

                                while (rsemailid1.Rows[iz]["autono"].ToString().Substring(0, rsemailid1.Rows[iz]["autono"].ToString().Length - 1) == chkfld)
                                {
                                    iz++;
                                    if (iz > maxR) break;
                                }
                            }
                            string uid = CommVar.UserID();
                            string MOBILE = DB1.USER_APPL.Find(uid).MOBILE;
                            string ldt = rsemailid1.Rows[rsemailid1.Rows.Count - 1]["docdt"].ToString();

                            reportdocument.SetDataSource(rsemailid1);
                            reportdocument.SetParameterValue("billheading", blhead);

                            reportdocument.SetParameterValue("complogo", masterHelp.retCompLogo());
                            reportdocument.SetParameterValue("complogo1", masterHelp.retCompLogo1());
                            reportdocument.SetParameterValue("compnm", compaddress.retCompValue("compnm"));
                            reportdocument.SetParameterValue("compadd", compaddress.retCompValue("compadd"));
                            reportdocument.SetParameterValue("compcommu", compaddress.retCompValue("compcommu"));
                            reportdocument.SetParameterValue("compstat", compaddress.retCompValue("compstat"));
                            reportdocument.SetParameterValue("locaadd", compaddress.retCompValue("locaadd"));
                            reportdocument.SetParameterValue("locacommu", compaddress.retCompValue("locacommu"));
                            reportdocument.SetParameterValue("locastat", compaddress.retCompValue("locastat"));
                            reportdocument.SetParameterValue("legalname", compaddress.retCompValue("legalname"));
                            reportdocument.SetParameterValue("corpadd", compaddress.retCompValue("corpadd"));
                            reportdocument.SetParameterValue("corpcommu", compaddress.retCompValue("corpcommu"));
                            reportdocument.SetParameterValue("formerlynm", compaddress.retCompValue("formerlynm"));
                            Response.Buffer = false;
                            Response.ClearContent();
                            Response.ClearHeaders();
                            Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                            stream.Seek(0, SeekOrigin.Begin);
                            string path_Save = @"C:\improvar\" + doccd + "-" + emlslcd + "-" + ldt.Substring(6, 4) + ldt.Substring(3, 2) + ldt.Substring(0, 2) + ".pdf";
                            if (System.IO.File.Exists(path_Save))
                            {
                                System.IO.File.Delete(path_Save);
                            }
                            reportdocument.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, path_Save);
                            reportdocument.Close();
                            // email

                            //System.Net.Mail.Attachment attchmail = new System.Net.Mail.Attachment(path_Save);
                            List<System.Net.Mail.Attachment> attchmail = new List<System.Net.Mail.Attachment>();// System.Net.Mail.Attachment(path_Save);
                            attchmail.Add(new System.Net.Mail.Attachment(path_Save));

                            string[,] emlaryBody = new string[7, 2];
                            if (VE.TEXTBOX5 != null)
                            {
                                bool emailsent = EmailControl.SendHtmlFormattedEmail(VE.TEXTBOX5, "", "", emlaryBody, attchmail, grpemailid);
                                if (emailsent == true) sendemailids = sendemailids + VE.TEXTBOX5 + ";"; else sendemailids = " not able to send on " + VE.TEXTBOX5 + ";";
                            }
                            else
                            {
                                emlaryBody[0, 0] = "{slnm}"; emlaryBody[0, 1] = slnm;
                                emlaryBody[1, 0] = "{tbody}"; emlaryBody[1, 1] = body;
                                emlaryBody[2, 0] = "{username}"; emlaryBody[2, 1] = System.Web.HttpContext.Current.Session["UR_NAME"].ToString();
                                emlaryBody[3, 0] = "{compname}"; emlaryBody[3, 1] = compaddress.retCompValue("compnm");
                                emlaryBody[4, 0] = "{usermobno}"; emlaryBody[4, 1] = MOBILE;
                                emlaryBody[5, 0] = "{complogo}"; emlaryBody[5, 1] = complogosrc;
                                emlaryBody[6, 0] = "{compfixlogo}"; emlaryBody[6, 1] = compfixlogosrc;
                                bool emailsent = EmailControl.SendHtmlFormattedEmail(rsemailid[z].email.ToString() + ccemailid, "Sales Bill copy of " + docnm, "Salebill.htm", emlaryBody, attchmail, grpemailid);
                                if (emailsent == true) sendemailids = sendemailids + rsemailid[z].email.ToString() + ";"; else sendemailids = sendemailids + " not able to send on " + rsemailid[z].email.ToString();
                            }
                            System.IO.File.Delete(path_Save);
                            //eof email sending
                        }
                    }
                    reportdocument.Dispose(); GC.Collect();
                    string emailretmsg = "email : " + sendemailids + "<br /> CC email on " + grpemailid;
                    return Content(emailretmsg);
                }
                else
                {
                    if (doctype == "CINV") reportdocument.Load(Server.MapPath("~/Report/CommSaleBill.rpt"));
                    else reportdocument.Load(Server.MapPath(rptname));

                    reportdocument.SetDataSource(IR);
                    reportdocument.SetParameterValue("billheading", blhead);
                    reportdocument.SetParameterValue("complogo", masterHelp.retCompLogo());
                    reportdocument.SetParameterValue("complogo1", masterHelp.retCompLogo1());
                    reportdocument.SetParameterValue("compnm", compaddress.retCompValue("compnm"));
                    reportdocument.SetParameterValue("compadd", compaddress.retCompValue("compadd"));
                    reportdocument.SetParameterValue("compcommu", compaddress.retCompValue("compcommu"));
                    reportdocument.SetParameterValue("compstat", compaddress.retCompValue("compstat"));
                    reportdocument.SetParameterValue("locaadd", compaddress.retCompValue("locaadd"));
                    reportdocument.SetParameterValue("locacommu", compaddress.retCompValue("locacommu"));
                    reportdocument.SetParameterValue("locastat", compaddress.retCompValue("locastat"));
                    reportdocument.SetParameterValue("legalname", compaddress.retCompValue("legalname"));
                    reportdocument.SetParameterValue("corpadd", compaddress.retCompValue("corpadd"));
                    reportdocument.SetParameterValue("corpcommu", compaddress.retCompValue("corpcommu"));
                    reportdocument.SetParameterValue("formerlynm", compaddress.retCompValue("formerlynm"));

                    if (printemail == "Excel")
                    {
                        string path_Save = @"C:\improvar\" + doccd + VE.FDOCNO + ".xls";
                        string exlfile = doccd + VE.FDOCNO + ".xls";
                        if (System.IO.File.Exists(path_Save))
                        {
                            System.IO.File.Delete(path_Save);
                        }
                        reportdocument.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.Excel, path_Save);
                        byte[] buffer = System.IO.File.ReadAllBytes(path_Save);
                        Response.ClearContent();
                        Response.Buffer = true;
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AddHeader("Content-Disposition", "attachment; filename=" + exlfile);
                        Response.BinaryWrite(buffer);
                        reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
                        Response.Flush();
                        Response.End();
                        return Content("Excel exported sucessfully");
                    }
                    else
                    {
                        Response.Buffer = false;
                        Response.ClearContent();
                        Response.ClearHeaders();
                        Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
                        stream.Seek(0, SeekOrigin.Begin);
                        return new FileStreamResult(stream, "application/pdf");
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        //string compaddress; string stremail = "";
        //compaddress = Salesfunc.retCompAddress(gocd, grpemailid);
        //stremail = compaddress.retCompValue("email");

        //string ccemail = grpemailid;
        //if (ccemail == "") ccemail = stremail;

        ////ReportDocument reportdocument = new ReportDocument();
        //string complogo = Salesfunc.retCompLogo();
        //EmailControl EmailControl = new EmailControl();

        //string complogosrc = complogo;
        //string compfixlogosrc = "c:\\improvar\\" + CommVar.Compcd(UNQSNO) + "fix.jpg";
        //string sendemailids = "";
        //string rptfile = "SaleBill.rpt";
        //if (VE.TEXTBOX6 != null) rptfile = VE.TEXTBOX6;
        //rptname = "~/Report/" + rptfile; // "SaleBill.rpt";
        //if (VE.maxdate == "CHALLAN") blhead = "CHALLAN";
        //ReportDocument reportdocument = new ReportDocument();
        //if (printemail == "Email")
        //{
        //    var rsemailid = (from DataRow dr in IR.Rows
        //                     select new
        //                     {
        //                         email = dr["regemailid"],
        //                         slcd = dr["slcd"]
        //                     }).Distinct().ToList();

        //    for (int z = 0; z < rsemailid.Count; z++)
        //    {
        //        if (rsemailid[z].email.ToString() != "")
        //        {
        //            var queryq = from row in IR.AsEnumerable()
        //                         where row.Field<string>("regemailid") == rsemailid[z].email.ToString()
        //                         select row;

        //            var rsemailid1 = queryq.AsDataView().ToTable();

        //            if (doctype == "CINV") reportdocument.Load(Server.MapPath("~/Report/CommSaleBill.rpt"));
        //            else reportdocument.Load(Server.MapPath(rptname));

        //            maxR = rsemailid1.Rows.Count - 1;
        //            Int32 iz = 0;
        //            string slnm = "", emlslcd = "", body = "", chkfld = "", chkfld1 = "", ccemailid = "";
        //            emlslcd = rsemailid[z].slcd.ToString();
        //            DataTable tblslcd = MasterHelpFa.retslcdCont(emlslcd, "S", true);
        //            for (int sz = 0; sz <= tblslcd.Rows.Count - 1; sz++)
        //            {
        //                if (tblslcd.Rows[sz]["regemailid"].ToString() != rsemailid[z].email.ToString())
        //                {
        //                    ccemailid += ";" + tblslcd.Rows[sz]["regemailid"].ToString();
        //                }
        //            }
        //            while (iz <= maxR)
        //            {
        //                slnm = rsemailid1.Rows[iz]["slnm"].ToString();
        //                emlslcd = rsemailid1.Rows[iz]["slcd"].ToString();
        //                body += "<tr>";
        //                body += "<td>" + rsemailid1.Rows[iz]["docno"] + "</td>";
        //                body += "<td>" + rsemailid1.Rows[iz]["docdt"] + "</td>";
        //                body += "<td style='text-align:right'>" + Cn.Indian_Number_format((rsemailid1.Rows[iz]["blamt"]).retDbl().ToString(), "0.00") + "</td>";
        //                body += "</tr>";

        //                chkfld = rsemailid1.Rows[iz]["autono"].ToString().Substring(0, rsemailid1.Rows[iz]["autono"].ToString().Length - 1);

        //                while (rsemailid1.Rows[iz]["autono"].ToString().Substring(0, rsemailid1.Rows[iz]["autono"].ToString().Length - 1) == chkfld)
        //                {
        //                    iz++;
        //                    if (iz > maxR) break;
        //                }
        //            }
        //            string uid = CommVar.UserID();
        //            string MOBILE = DB1.USER_APPL.Find(uid).MOBILE;
        //            string ldt = rsemailid1.Rows[rsemailid1.Rows.Count - 1]["docdt"].ToString();

        //            reportdocument.SetDataSource(rsemailid1);
        //            reportdocument.SetParameterValue("billheading", blhead);

        //            reportdocument.SetParameterValue("complogo", masterHelp.retCompLogo());
        //            reportdocument.SetParameterValue("complogo1", masterHelp.retCompLogo1());
        //            reportdocument.SetParameterValue("compnm", compaddress.retCompValue("compnm"));
        //            reportdocument.SetParameterValue("compadd", compaddress.retCompValue("compadd"));
        //            reportdocument.SetParameterValue("compcommu", compaddress.retCompValue("compcommu"));
        //            reportdocument.SetParameterValue("compstat", compaddress.retCompValue("compstat"));
        //            reportdocument.SetParameterValue("locaadd", compaddress.retCompValue("locaadd"));
        //            reportdocument.SetParameterValue("locacommu", compaddress.retCompValue("locacommu"));
        //            reportdocument.SetParameterValue("locastat", compaddress.retCompValue("locastat"));
        //            reportdocument.SetParameterValue("legalname", compaddress.retCompValue("legalname"));
        //            reportdocument.SetParameterValue("corpadd", compaddress.retCompValue("corpadd"));
        //            reportdocument.SetParameterValue("corpcommu", compaddress.retCompValue("corpcommu"));
        //            Response.Buffer = false;
        //            Response.ClearContent();
        //            Response.ClearHeaders();
        //            Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
        //            stream.Seek(0, SeekOrigin.Begin);
        //            string path_Save = @"C:\improvar\" + doccd + "-" + emlslcd + "-" + ldt.Substring(6, 4) + ldt.Substring(3, 2) + ldt.Substring(0, 2) + ".pdf";
        //            if (System.IO.File.Exists(path_Save))
        //            {
        //                System.IO.File.Delete(path_Save);
        //            }
        //            reportdocument.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, path_Save);
        //            reportdocument.Close();
        //             email

        //            System.Net.Mail.Attachment attchmail = new System.Net.Mail.Attachment(path_Save);
        //            List<System.Net.Mail.Attachment> attchmail = new List<System.Net.Mail.Attachment>();// System.Net.Mail.Attachment(path_Save);
        //            attchmail.Add(new System.Net.Mail.Attachment(path_Save));

        //            string[,] emlaryBody = new string[7, 2];
        //            if (VE.TEXTBOX5 != null)
        //            {
        //                bool emailsent = EmailControl.SendHtmlFormattedEmail(VE.TEXTBOX5, "", "", emlaryBody, attchmail, grpemailid);
        //                if (emailsent == true) sendemailids = sendemailids + VE.TEXTBOX5 + ";"; else sendemailids = " not able to send on " + VE.TEXTBOX5 + ";";
        //            }
        //            else
        //            {
        //                emlaryBody[0, 0] = "{slnm}"; emlaryBody[0, 1] = slnm;
        //                emlaryBody[1, 0] = "{tbody}"; emlaryBody[1, 1] = body;
        //                emlaryBody[2, 0] = "{username}"; emlaryBody[2, 1] = System.Web.HttpContext.Current.Session["UR_NAME"].ToString();
        //                emlaryBody[3, 0] = "{compname}"; emlaryBody[3, 1] = compaddress.retCompValue("compnm");
        //                emlaryBody[4, 0] = "{usermobno}"; emlaryBody[4, 1] = MOBILE;
        //                emlaryBody[5, 0] = "{complogo}"; emlaryBody[5, 1] = complogosrc;
        //                emlaryBody[6, 0] = "{compfixlogo}"; emlaryBody[6, 1] = compfixlogosrc;
        //                bool emailsent = EmailControl.SendHtmlFormattedEmail(rsemailid[z].email.ToString() + ccemailid, "Sales Bill copy of " + docnm, "Salebill.htm", emlaryBody, attchmail, grpemailid);
        //                if (emailsent == true) sendemailids = sendemailids + rsemailid[z].email.ToString() + ";"; else sendemailids = sendemailids + " not able to send on " + rsemailid[z].email.ToString();
        //            }
        //            System.IO.File.Delete(path_Save);
        //            eof email sending
        //        }
        //    }
        //    reportdocument.Dispose(); GC.Collect();
        //    string emailretmsg = "email : " + sendemailids + "<br /> CC email on " + grpemailid;
        //    return Content(emailretmsg);
        //}
        //else
        //{
        //    if (doctype == "CINV") reportdocument.Load(Server.MapPath("~/Report/CommSaleBill.rpt"));
        //    else reportdocument.Load(Server.MapPath(rptname));

        //    reportdocument.SetDataSource(IR);
        //    reportdocument.SetParameterValue("billheading", blhead);
        //    reportdocument.SetParameterValue("complogo", masterHelp.retCompLogo());
        //    reportdocument.SetParameterValue("complogo1", masterHelp.retCompLogo1());
        //    reportdocument.SetParameterValue("compnm", compaddress.retCompValue("compnm"));
        //    reportdocument.SetParameterValue("compadd", compaddress.retCompValue("compadd"));
        //    reportdocument.SetParameterValue("compcommu", compaddress.retCompValue("compcommu"));
        //    reportdocument.SetParameterValue("compstat", compaddress.retCompValue("compstat"));
        //    reportdocument.SetParameterValue("locaadd", compaddress.retCompValue("locaadd"));
        //    reportdocument.SetParameterValue("locacommu", compaddress.retCompValue("locacommu"));
        //    reportdocument.SetParameterValue("locastat", compaddress.retCompValue("locastat"));
        //    reportdocument.SetParameterValue("legalname", compaddress.retCompValue("legalname"));
        //    reportdocument.SetParameterValue("corpadd", compaddress.retCompValue("corpadd"));
        //    reportdocument.SetParameterValue("corpcommu", compaddress.retCompValue("corpcommu"));

        //    if (printemail == "Excel")
        //    {
        //        string path_Save = @"C:\improvar\" + doccd + VE.FDOCNO + ".xls";
        //        string exlfile = doccd + VE.FDOCNO + ".xls";
        //        if (System.IO.File.Exists(path_Save))
        //        {
        //            System.IO.File.Delete(path_Save);
        //        }
        //        reportdocument.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.Excel, path_Save);
        //        byte[] buffer = System.IO.File.ReadAllBytes(path_Save);
        //        Response.ClearContent();
        //        Response.Buffer = true;
        //        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        Response.AddHeader("Content-Disposition", "attachment; filename=" + exlfile);
        //        Response.BinaryWrite(buffer);
        //        reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
        //        Response.Flush();
        //        Response.End();
        //        return Content("Excel exported sucessfully");
        //    }
        //    else
        //    {
        //        Response.Buffer = false;
        //        Response.ClearContent();
        //        Response.ClearHeaders();
        //        Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
        //        reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
        //        stream.Seek(0, SeekOrigin.Begin);
        //        return new FileStreamResult(stream, "application/pdf");
        //    }
        //}


        //}
        //catch (Exception ex)
        //{
        //    Cn.SaveException(ex, "");
        //    return Content(ex.Message);
        //}
        public string SaleSMSSend(string autono = "", string doccd = "", string slcd = "", string fdocdt = "", string tdocdt = "", string fdocno = "", string tdocno = "")
        {
            SMS SMS = new SMS();
            string SmsRetVal = "";
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    SmsRetVal = SMS.sendSaleSMS("", doccd, slcd, fdocdt, tdocdt, fdocno, tdocno);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }
            return SmsRetVal;
        }
        public ActionResult ReportBarcodeImagePrint(ReportViewinHtml VE, FormCollection FC, string todt, string barno)
        {
            string scm1 = CommVar.CurSchema(UNQSNO).ToString();
            DataTable dt = new DataTable("Rep_BarcodeImage");
            dt.Columns.Add("BARNO", typeof(string));
            dt.Columns.Add("DOC_FLNAME", typeof(string));
            dt.Columns.Add("LINE1", typeof(string));
            dt.Columns.Add("LINE2", typeof(string));

            var dttt = Salesfunc.GetStock(todt, "", barno);
            for (int i = 0; i < dttt.Rows.Count; i++)
            {
                if (dttt.Rows[i]["barimage"].retStr() != "")
                {
                    var brimgs = dttt.Rows[i]["barimage"].retStr().Split((char)179);
                    foreach (var barimg in brimgs)
                    {
                        string barfilename = barimg.Split('~')[0].Trim();
                        string barimgdesc = barimg.Split('~')[1];
                        DataRow dr1 = dt.NewRow();
                        dr1["BARNO"] = dttt.Rows[i]["BARNO"].ToString();
                        dr1["DOC_FLNAME"] = barfilename;//dttt.Rows[i]["DOC_FLNAME"].ToString();
                        dr1["LINE1"] = barimgdesc;// dttt.Rows[i]["DOC_DESC"].ToString(); ;
                        dr1["LINE2"] = "1700001 SD";
                        dt.Rows.Add(dr1);
                    }
                }

            }
            Session["DtRepBarcodeImage"] = dt;
            return RedirectToAction("Rep_BarcodeImage", "Rep_BarcodeImage", new { US = Cn.Encrypt_URL(UNQSNO) });
        }
    }
}


