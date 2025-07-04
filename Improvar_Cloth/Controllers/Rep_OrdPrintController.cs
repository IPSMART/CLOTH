﻿using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.ViewModels;
using System.Data;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Improvar.Models;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class Rep_OrdPrintController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp Master_Help = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        MasterHelpFa Master_HelpFa = new MasterHelpFa();
        string path_Save = "C:\\Ipsmart\\Temp";

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
        public ActionResult Rep_OrdPrint(ReportViewinHtml VE, FormCollection FC, string submitbutton)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), Scm1 = CommVar.CurSchema(UNQSNO), Scmf = CommVar.FinSchema(UNQSNO), scmI = CommVar.InvSchema(UNQSNO);
                string yr_cd = CommVar.YearCode(UNQSNO);


                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                string str1 = "";
                DataTable rsTmp;
                string doctype = "", billno = "";
                string docnos = VE.TEXTBOX8.retStr();

                string fdate = "", tdate = "";
                if (VE.FDT != null)
                {
                    fdate = VE.FDT.retDateStr();   // Convert.ToString(Convert.ToDateTime(FC["FDT"].ToString())).Substring(0, 10);
                }
                if (VE.TDT != null)
                {
                    tdate = VE.TDT.retDateStr();   // Convert.ToString(Convert.ToDateTime(FC["TDT"].ToString())).Substring(0, 10);
                }
                string fdocno = VE.FDOCNO.retStr();// FC["FDOCNO"].ToString();
                string tdocno = VE.TDOCNO.retStr();//  FC["TDOCNO"].ToString();
                string doccd = VE.DOCCD.retStr();// FC["doccd"].ToString();
                string docnm = DB.M_DOCTYPE.Find(doccd).DOCNM;
                string slcd = "";
                if (FC.AllKeys.Contains("slcdvalue")) slcd = CommFunc.retSqlformat(FC["slcdvalue"].ToString());

                string prnemailid = "";
                if (VE.TEXTBOX5 != null) prnemailid = "'" + VE.TEXTBOX5 + "' regemailid"; else prnemailid = "e.regemailid";

                var printemail = submitbutton.ToString(); 
                int maxR = 0; string blhead = "", gocd = "", grpemailid = "";
                string rptname = "";


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

                string FileName = "";
                switch (VE.MENU_PARA)
                {
                    case "SORD":
                        FileName = "Sales_Order"; blhead = "Sales Order"; break;
                    case "PORD":
                        FileName = "Purchase_Order"; blhead = "Purchase Order"; break;
                    case "SORDC":
                        FileName = "Sales_Order_Retail"; blhead = "Sales Order Retail"; break;
                    default: FileName = "Doc_Print"; blhead = "Sales Order"; break;
                }

                str1 = "select doctype from " + Scm1 + ".m_doctype where doccd='" + VE.DOCCD + "'";
                rsTmp = Master_Help.SQLquery(str1);
                doctype = rsTmp.Rows[0]["doctype"].ToString();
                string sql = "";
                string sqlc = "";
                sqlc += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and c.yr_cd='" + yr_cd + "' and " + Environment.NewLine;
                if (docnos.retStr() == "")
                {
                    if (fdocno != "") sqlc += "c.doconlyno >= " + fdocno + " and c.doconlyno <= " + tdocno + " and " + Environment.NewLine;
                    if (fdate != "") sqlc += "c.docdt >= to_date('" + fdate + "','dd/mm/yyyy') and " + Environment.NewLine;
                    if (tdate != "") sqlc += "c.docdt <= to_date('" + tdate + "','dd/mm/yyyy') and " + Environment.NewLine;

                }
                else
                {
                    sqlc += "c.doconlyno in ( " + docnos + ") and " + Environment.NewLine;
                }

                if (slcd.retStr() != "") sqlc += "b.slcd in (" + slcd + ") and " + Environment.NewLine;
                sqlc += "c.doccd = '" + doccd + "' and " + Environment.NewLine;

                sql += " select a.autono, '' doctag, h.doccd, h.docno, h.docdt, b.duedays, h.canc_rem,to_char(h.usr_entdt, 'HH24:MI') invisstime,'N' batchdlprint,  " + Environment.NewLine;
                sql += " '' gocd,'' gonm, '' goadd1, '' goadd2, '' goadd3, '' gophno, '' goemail, h.usr_id, h.usr_entdt, h.vchrno, nvl(e.pslcd, e.slcd) oslcd, b.slcd, " + Environment.NewLine;
                sql += " nvl(x.nm,nvl(e.fullname, e.slnm)) slnm, " + prnemailid + ", e.add1 sladd1, e.add2 sladd2, e.add3 sladd3, e.add4 sladd4, e.add5 sladd5, e.add6 sladd6, e.add7 sladd7,  " + Environment.NewLine;
                sql += " e.gstno, e.panno, trim(e.regmobile || decode(e.regmobile, null, '', ',') || e.slphno || decode(e.phno1, null, '', ',' || e.phno1)) phno, e.state, e.country, e.statecd, e.actnameof slactnameof,e.subdistrict sldistrict,  " + Environment.NewLine;
                sql += " nvl(b.conslcd, b.slcd) cslcd, '' cpartycd, nvl(f.fullname, f.slnm) cslnm, f.add1 csladd1, f.add2 csladd2, f.add3 csladd3, f.add4 csladd4, f.add5 csladd5, " + Environment.NewLine;
                sql += " f.add6 csladd6, f.add7 csladd7, nvl(f.gstno, f.gstno) cgstno, nvl(f.panno, f.panno) cpanno,f.actnameof cslactnameof,f.subdistrict csldistrict, " + Environment.NewLine;
                sql += " trim(f.regmobile || decode(f.regmobile, null, '', ',') || f.slphno || decode(f.phno1, null, '', ',' || f.phno1)) cphno, f.state cstate, f.statecd cstatecd,  " + Environment.NewLine;
                sql += " c.translcd trslcd, g.slnm trslnm, g.gstno trgst, g.add1 trsladd1, g.add2 trsladd2, g.add3 trsladd3, g.add4 trsladd4, g.phno1 trslphno, c.lrno,  " + Environment.NewLine;
                sql += " c.lrdt, c.lorryno, c.ewaybillno, c.grwt, c.ntwt, a.slno, a.itcd, y.styleno, a.itnm, a.itrem, a.batchdtl, a.hsncode,  " + Environment.NewLine;
                sql += " a.nos, a.qnty, nvl(i.decimals, 0) qdecimal, i.uomnm, a.rate, a.amt, d.docrem, d.docth, d.casenos, d.noofcases,  " + Environment.NewLine;
                sql += " d.agslcd, m.slnm agslnm, a.agdocno, a.agdocdt, j.itgrpnm, j.shortnm,  " + Environment.NewLine;
                sql += " nvl(a.igstper, 0)igstper, nvl(a.igstamt, 0)igstamt, nvl(a.cgstper, 0)cgstper, nvl(a.cgstamt, 0)cgstamt,  " + Environment.NewLine;
                sql += " nvl(a.sgstper, 0)sgstper, nvl(a.sgstamt, 0)sgstamt, nvl(a.dutyper, 0)dutyper, nvl(a.dutyamt, 0)dutyamt, nvl(a.cessper, 0)cessper, nvl(a.cessamt, 0)cessamt,  " + Environment.NewLine;
                sql += " nvl(a.igstper + a.cgstper + a.sgstper, 0) gstper, 0 roamt, 0 blamt, 0 tcsper, 0 tcsamt, d.insby,  " + Environment.NewLine;
                sql += " d.othnm, nvl(d.othadd1, f.othadd1) othadd1, d.porefno, d.porefdt, d.despby, d.dealby, d.packby, d.selby,  " + Environment.NewLine;
                sql += " decode(d.othadd1, null, f.othadd2, d.othadd2) othadd2, decode(d.othadd1, null, f.othadd3, d.othadd3) othadd3, decode(d.othadd1, null, f.othadd4, d.othadd4) othadd4,f.OTHADDPIN,  " + Environment.NewLine;
                sql += " '' disctype, 0 discrate, z.discamt, '' scmdisctype, 0 scmdiscrate, z.scmdiscamt, '' tddisctype, 0 tddiscrate, 0 tddiscamt,(nvl(z.scmdiscamt,0)+nvl(z.discamt,0)) totdiscamt,  " + Environment.NewLine;
                sql += "(case when nvl(h.cancel,'N')='Y' then 'C' when r.autono is not null then 'A' " + Environment.NewLine;
                sql += "when nvl(s.einvappl,'N')='Y' and p.irnno is null and e.gstno is not null and s.expcd is null and s.salpur='S' then 'I' end) cancel,p.irnno, " + Environment.NewLine;
                sql += "(case when h.docdt >= to_date('01/07/2021','dd/mm/yyyy') and nvl(s.einvappl,'N')='Y' and e.gstno is null then 'Y' else 'N' end)B2C, ";
                sql += " '' curr_cd,a.listprice,a.listdiscper,p.ackno,to_char(p.ackdt,'dd-mm-yyyy hh24:mi:ss') ackdt,d.mutslcd,q.slnm mutslnm,a.flagmtr,d.payterms,d.bltype,y.pdesign,t.itcd fabitcd,t.itnm fabitnm,e.district plsupply,u.slnm sagslnm,v.courcd,w.slnm cournm,  " + Environment.NewLine;
                sql += "x.nm,x.addr addr1,x.city addr2,decode(x.mobile, null, '', 'Ph. # '||x.mobile)addr3,''addr4,''addr5,''addr6,''addr7,''addr8,''addr9,''addr10,''addr11,''addr12,x.mobile from " + Environment.NewLine;

                sql += " (select a.autono, '' addless,a.autono || a.slno autoslno, a.slno, a.itcd, d.itnm,''pdesign, d.styleno, d.uomcd, nvl(d.hsncode, f.hsncode) hsncode,  " + Environment.NewLine;
                sql += " a.itrem, ''baleno, 0 nos, a.qnty, 0 flagmtr, a.rate,ROUND( (nvl(a.qnty,0)*nvl(a.rate,0)),2)amt, ''agdocno, ''agdocdt,  " + Environment.NewLine;
                sql += " '' batchdtl,  " + Environment.NewLine;
                sql += " 0 igstper, 0 igstamt, 0 cgstper, 0 cgstamt, 0 sgstper, 0 sgstamt,0 dutyper, 0 dutyamt, 0 cessper, 0 cessamt,0 listprice,0 listdiscper  " + Environment.NewLine;
                sql += " from " + Scm1 + ".T_SORDDTL a, " + Scm1 + ".T_SORD b, " + Scm1 + ".t_cntrl_hdr c, " + Scm1 + ".m_sitem d, " + Scm1 + ".m_group f, " + Scm1 + ".t_batchdtl  n, " + Scm1 + ".t_batchmst o  " + Environment.NewLine;
                sql += " where a.autono = b.autono and a.autono = c.autono and a.itcd = d.itcd and a.autono = n.autono(+) and a.slno = n.txnslno(+) and n.barno = o.barno(+) and  " + Environment.NewLine;
                sql += " c.compcd = '" + COM + "' and c.loccd = '" + LOC + "' and c.yr_cd = '" + yr_cd + "' and  " + Environment.NewLine;
                if (docnos.retStr() == "")
                {
                    if (fdocno != "") sql += " c.doconlyno >= " + fdocno + " and c.doconlyno <= " + tdocno + " and  " + Environment.NewLine;
                    if (fdate != "") sql += " c.docdt >= to_date('" + fdate + "', 'dd/mm/yyyy') and  " + Environment.NewLine;
                    if (tdate != "") sql += " c.docdt <= to_date('" + tdate + "', 'dd/mm/yyyy') and  " + Environment.NewLine;
                }
                else
                {
                    sql += "c.doconlyno in ( " + docnos + ") and " + Environment.NewLine;
                }

                sql += " c.doccd = '" + doccd + "' and d.itgrpcd = f.itgrpcd(+)  " + Environment.NewLine;
                sql += " union all  ";

                sql += " select a.autono,d.addless, a.autono autoslno, nvl(ascii(d.calccode), 0) + 1000 slno, '' itcd, d.amtnm || ' ' || a.amtdesc itnm,'' pdesign, '' styleno, '' uomcd, a.hsncode hsncode,  " + Environment.NewLine;
                sql += " '' itrem, '' baleno, 0 nos, 0 qnty, 0 flagmtr, a.amtrate rate, decode(d.addless,'L',a.amt*-1,a.amt)amt, '' agdocno, '' agdocdt, '' batchdtl,  " + Environment.NewLine;
                sql += " a.igstper, decode(d.addless,'L',a.igstamt*-1,a.igstamt) igstamt, a.cgstper, decode(d.addless,'L',a.cgstamt*-1,a.cgstamt)cgstamt, a.sgstper, decode(d.addless,'L',a.sgstamt*-1,a.sgstamt) sgstamt, a.dutyper, decode(d.addless,'L',a.dutyamt*-1,a.dutyamt) dutyamt, a.cessper, decode(d.addless,'L',a.cessamt*-1,a.cessamt) cessamt,0 listprice,0 listdiscper  " + Environment.NewLine;
                sql += " from " + Scm1 + ".t_txnamt a, " + Scm1 + ".T_SORD b, " + Scm1 + ".t_cntrl_hdr c, " + Scm1 + ".m_amttype d  " + Environment.NewLine;
                sql += " where a.autono = b.autono and a.autono = c.autono and c.compcd = '" + COM + "' and c.loccd = '" + LOC + "' and c.yr_cd = '" + yr_cd + "' and  " + Environment.NewLine;
                if (docnos.retStr() == "")
                {
                    if (fdocno != "") sql += " c.doconlyno >= " + fdocno + " and c.doconlyno <= " + tdocno + " and " + Environment.NewLine;
                    if (fdate != "") sql += " c.docdt >= to_date('" + fdate + "', 'dd/mm/yyyy') and  " + Environment.NewLine;
                    if (tdate != "") sql += " c.docdt <= to_date('" + tdate + "', 'dd/mm/yyyy') and  " + Environment.NewLine;
                }
                else
                {
                    sql += "c.doconlyno in ( " + docnos + ") and " + Environment.NewLine;
                }
                sql += "c.doccd = '" + doccd + "'  " + Environment.NewLine;
                sql += "and a.amtcd = d.amtcd(+)  " + Environment.NewLine;
                sql += " ) a,  " + Environment.NewLine;

                sql += "( select distinct a.autono " + Environment.NewLine;
                sql += "from " + Scm1 + ".t_cntrl_doc_pass a, " + Scm1 + ".t_cntrl_hdr b, " + Scm1 + ".t_cntrl_auth c " + Environment.NewLine;
                sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and c.autono is null and " + Environment.NewLine;
                sql += "b.doccd='" + doccd + "' ) r, " + Environment.NewLine;

                sql += "( select distinct b.autono, e.expcd, e.salpur, decode(nvl(d.einvappl,'N'),'Y',(case when c.docdt >= d.einvappldt then 'Y' else 'N' end),d.einvappl) einvappl " + Environment.NewLine;
                sql += "from " + Scm1 + ".T_SORD b, " + Scm1 + ".t_cntrl_hdr c, " + Scmf + ".m_comp d, " + Scmf + ".t_vch_gst e " + Environment.NewLine;
                sql += "where b.autono = c.autono(+) and b.autono=e.autono(+) and " + Environment.NewLine;
                sql += sqlc;
                sql += "c.compcd = d.compcd(+) ) s, " + Environment.NewLine;

                sql += "(select listagg(t.pdesign, ', ') within group (order by t.autono, t.slno)pdesign, " + Environment.NewLine;
                sql += "listagg(nvl(nvl(t.pdesign, t.ourdesign), t.styleno), ', ') within group(order by t.autono, t.slno)styleno, " + Environment.NewLine;
                sql += "t.autono,t.slno from ( " + Environment.NewLine;
                sql += "select distinct a.pdesign, a.ourdesign, d.styleno, c.autono, c.slno " + Environment.NewLine;
                sql += "from " + Scm1 + ".t_batchmst a, " + Scm1 + ".t_batchdtl  b, " + Scm1 + ".T_SORDDTL c, " + Scm1 + ".m_sitem d where  a.barno = b.barno(+) " + Environment.NewLine;
                sql += "and b.autono = c.autono(+) and b.txnslno = c.slno(+) and c.itcd = d.itcd " + Environment.NewLine;
                sql += ") t " + Environment.NewLine;
                sql += "group by t.autono,t.slno)y, " + Environment.NewLine; //for pdesgin differn of same item for tres

                sql += " " + Scm1 + ".T_SORDDTL z, " + Scm1 + ".T_SORD b, " + Scm1 + ".t_txntrans c, " + Scm1 + ".t_txnoth d, " + Scmf + ".m_subleg e, " + Scmf + ".m_subleg f, " + Scmf + ".m_subleg g,  " + Environment.NewLine;
                sql += " " + Scm1 + ".t_cntrl_hdr h, " + Scmf + ".m_uom i, " + Scm1 + ".m_group j, " + Scm1 + ".m_sitem l, " + Scmf + ".m_subleg m," + Scmf + ".t_txneinv p, " + Scmf + ".m_subleg q, " + Scm1 + ".m_sitem t," + Scmf + ".m_subleg u," + Scm1 + ".m_subleg_sddtl v," + Scmf + ".m_subleg w, " + Scm1 + ".T_TXNMEMO x  " + Environment.NewLine;
                sql += " where a.autono = z.autono(+) and a.slno = z.slno(+) and a.autono = b.autono and a.autono = c.autono(+) and a.autono = d.autono(+) and  " + Environment.NewLine;
                sql += " b.slcd = e.slcd and nvl(b.conslcd, b.slcd) = f.slcd(+) and c.translcd = g.slcd(+) and a.autono = h.autono and a.itcd = l.itcd(+) and l.itgrpcd = j.itgrpcd(+) and a.uomcd = i.uomcd(+) and  " + Environment.NewLine;
                sql += " d.agslcd = m.slcd(+) and l.fabitcd=t.itcd(+) and b.slcd=v.slcd(+)  and (v.compcd='" + COM + "' or v.compcd is null) and (v.loccd='" + LOC + "'  or v.loccd is null)  and v.courcd=w.slcd(+) and  " + Environment.NewLine;
                sql += "a.autono=r.autono(+) and a.autono=s.autono(+) and a.autono=p.autono(+) and d.mutslcd=q.slcd(+) and d.sagslcd=u.slcd(+) and a.autono=x.autono(+) and a.autono = y.autono(+) and a.slno = y.slno(+) and " + Environment.NewLine;
                if (slcd.retStr() != "") sql += " b.slcd in (" + slcd + ") and " + Environment.NewLine;
                sql += " a.autono not in (select a.autono from " + Scm1 + ".t_cntrl_doc_pass a, " + Scm1 + ".t_cntrl_hdr b, " + Scm1 + ".t_cntrl_auth c  " + Environment.NewLine;
                sql += " where a.autono = b.autono(+) and a.autono = c.autono(+) and c.autono is null and b.doccd = '" + doccd + "' )   " + Environment.NewLine;
                if (VE.Checkbox11 == true && printemail == "Print")
                {
                    sql += "and a.autono not in (select a.autono from " + Scm1 + ".t_txnstatus a, " + Scm1 + ".t_cntrl_hdr b  " + Environment.NewLine;
                    sql += " where a.autono = b.autono(+) and a.ststype='P' and b.doccd = '" + doccd + "' )   " + Environment.NewLine;
                }
                sql += " order by docno,autono,slno  ";
                DataTable tbl = new DataTable();
                tbl = Master_Help.SQLquery(sql);
                if (tbl.Rows.Count == 0) return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "Records not found !!" });

                DataTable rsStkPrcDesc;
                sql = "";
                sql += "select distinct a.autono,a.txnslno,a.barno,a.shade,a.nos,a.qnty,a.flagmtr,a.disctype,a.discrate,a.scmdisctype,a.scmdiscrate, ";
                sql += "a.tddisctype,a.tddiscrate,a.itrem,a.baleno,a.cutlength,a.slno ";
                sql += "from " + Scm1 + ".t_batchdtl a, " + Scm1 + ".T_SORD b, " + Scm1 + ".t_cntrl_hdr c ";
                sql += "where  ";
                sql += sqlc;
                sql += "a.autono=b.autono and a.autono=c.autono order by a.autono,a.slno ";
                rsStkPrcDesc = Master_Help.SQLquery(sql);

                string blterms = "", inspoldesc = "", dealsin = "";
                Int16 bankslno = 0;
                sql = "select blterms, inspoldesc, dealsin, nvl(bankslno,0) bankslno from " + Scm1 + ".m_mgroup_spl where compcd='" + CommVar.Compcd(UNQSNO) + "' ";
                DataTable rsMgroupSpl = Master_Help.SQLquery(sql);
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
                IR.Columns.Add("invisstime", typeof(string), "");
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
                IR.Columns.Add("sladd11", typeof(string), "");
                IR.Columns.Add("sladd12", typeof(string), "");
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
                IR.Columns.Add("csladd11", typeof(string), "");
                IR.Columns.Add("csladd12", typeof(string), "");
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
                IR.Columns.Add("txnslno", typeof(int), "");
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
                IR.Columns.Add("tddiscamt", typeof(string), "");
                IR.Columns.Add("disc", typeof(string), "");
                IR.Columns.Add("discamt", typeof(string), "");
                IR.Columns.Add("scmdisctype", typeof(string), "");
                IR.Columns.Add("scmdiscrate", typeof(double), "");
                IR.Columns.Add("scmdiscamt", typeof(string), "");
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
                IR.Columns.Add("mutslnm", typeof(string), "");
                IR.Columns.Add("flagmtr", typeof(double), "");
                IR.Columns.Add("bltype", typeof(string), "");
                IR.Columns.Add("netqnty", typeof(double), "");
                IR.Columns.Add("nqdecimal", typeof(double), "");
                IR.Columns.Add("PDESIGN", typeof(string), "");
                IR.Columns.Add("fabitcd", typeof(string), "");
                IR.Columns.Add("fabitnm", typeof(string), "");
                IR.Columns.Add("printby", typeof(string), "");
                IR.Columns.Add("sagslnm", typeof(string), "");
                IR.Columns.Add("name", typeof(string), "");
                IR.Columns.Add("COURCD", typeof(string), "");
                IR.Columns.Add("COURNM", typeof(string), "");

                #endregion

                string bankname = "", bankactno = "", bankbranch = "", bankifsc = "", bankadd = "", bankrtgs = "";
                sql = "";
                sql += "select a.bankname, a.bankactno, a.ifsccode, a.address, a.branch ";
                sql += "from " + Scmf + ".m_loca_ifsc a ";
                sql += "where a.compcd = '" + COM + "' and a.loccd = '" + LOC + "' and ";
                if (bankslno == 0) sql += "a.defltbank = 'T' ";
                else sql += "a.slno=" + bankslno.ToString() + " ";
                DataTable rsbank = Master_Help.SQLquery(sql);
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
                string auto1 = ""; string copymode = ""; string blrem = ""; string itdsc = ""; string goadd = "";
                string rupinwords = "";
                int uomdecimal = 3; int uommaxdecimal = 0;
                int nuomdecimal = 3; int nuommaxdecimal = 0;



                Int16 maxCopy = 5;
                string[] totalautono = tbl.AsEnumerable().Select(a => a.Field<string>("autono")).Distinct().ToArray();
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
                        double dcgstamt = 0; double dsgstamt = 0; double dnetamt = 0; double dnos = 0; double dqnty = 0; double dnqnty = 0;
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
                                totalosamt = Convert.ToDouble(Master_HelpFa.slcdbal(oslcd, oglcd, odocdt, oclass1cd));
                                oslcd.retStr();

                                sql = "";
                                sql += "select sum(nvl(a.blamt,0)) blamt from ( ";
                                sql += "select nvl(b.pslcd,a.slcd) oslcd, sum(nvl(a.blamt,0)) blamt ";
                                sql += "from " + Scm1 + ".T_SORD a, " + Scmf + ".m_subleg b, " + Scm1 + ".t_cntrl_hdr c, " + Scm1 + ".m_doctype d ";
                                sql += "where a.autono=c.autono and c.doccd=d.doccd and a.slcd=b.slcd and nvl(c.cancel,'N')='N' and c.compcd='" + COM + "' and c.yr_cd='" + yr_cd + "' and ";
                                sql += "c.docdt=to_date('" + tbl.Rows[i]["docdt"].ToString().retDateStr() + "','dd/mm/yyyy') and ";
                                sql += "d.doctype='SBILL' and c.vchrno <= " + Convert.ToDouble(tbl.Rows[i]["vchrno"]) + " and c.doccd='" + doccd + "' ";
                                sql += "group by nvl(b.pslcd,a.slcd) ) a where oslcd='" + oslcd + "'";
                                rsTmp = Master_HelpFa.SQLquery(sql);
                                if (rsTmp.Rows.Count > 0) totalosamt = totalosamt + Convert.ToDouble(rsTmp.Rows[0]["blamt"] == DBNull.Value ? 0 : rsTmp.Rows[0]["blamt"]);
                            }

                            Type A_T = tbl.Rows[0]["amt"].GetType(); Type Q_T = tbl.Rows[0]["qnty"].GetType(); Type N_S = tbl.Rows[0]["nos"].GetType(); Type I_T = tbl.Rows[0]["igstamt"].GetType();
                            Type C_T = tbl.Rows[0]["cgstamt"].GetType(); Type S_T = tbl.Rows[0]["sgstamt"].GetType();
                            Type C_Tt = tbl.Rows[0]["totdiscamt"].GetType();
                            var GST_DATA = (from DataRow DR in tbl.Rows
                                            where DR["autono"].ToString() == auto1
                                            group DR by new { autono = DR["autono"].ToString() } into X
                                            select new
                                            {
                                                TAMT = (A_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("amt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("amt")))) - X.Sum(Z => Z.Field<decimal>("totdiscamt").retDbl()),
                                                //TDISCAMT = (A_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("discamt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("discamt")))),
                                                //TSCMDISCAMT = (A_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("SCMdiscamt")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("SCMdiscamt")))),
                                                TQNTY = Q_T.Name == "Double" ? X.Sum(Z => Z.Field<double>("qnty")) : Convert.ToDouble(X.Sum(Z => Z.Field<decimal>("qnty"))),
                                            }).ToList();

                            if (GST_DATA != null && GST_DATA.Count > 0)
                            {
                                foreach (var k in GST_DATA)
                                {
                                    tqnty = tqnty + Convert.ToDouble(k.TQNTY);
                                    //tnos = tnos + Convert.ToDouble(k.TNOS);
                                    //tamt = tamt + Convert.ToDouble(k.TAMT);
                                    //tgst = tgst + Convert.ToDouble(k.IGSTAMT) + Convert.ToDouble(k.CGSTAMT) + Convert.ToDouble(k.SGSTAMT);
                                    blamt += k.TAMT.retDbl();
                                }
                            }

                            //var HSN_DATA = (from a in DBF.T_VCH_GST
                            //                where a.AUTONO == auto1
                            //                group a by new { HSNCODE = a.HSNCODE, IGSTPER = a.IGSTPER, CGSTPER = a.CGSTPER, SGSTPER = a.SGSTPER } into x
                            //                select new
                            //                {

                            //                    HSNCODE = x.Key.HSNCODE,
                            //                    IGSTPER = x.Key.IGSTPER,
                            //                    CGSTPER = x.Key.CGSTPER,
                            //                    SGSTPER = x.Key.SGSTPER,
                            //                    TIGSTAMT = x.Sum(s => s.IGSTAMT),
                            //                    TCGSTAMT = x.Sum(s => s.CGSTAMT),
                            //                    TSGSTAMT = x.Sum(s => s.SGSTAMT),
                            //                    TAMT = x.Sum(s => s.AMT),
                            //                    TQNTY = x.Sum(s => s.QNTY)
                            //                    //DECIMAL = (from z in DBF.M_UOM
                            //                    //           where z.UOMCD == (from y in DBF.T_VCH_GST where y.AUTONO == auto1 select y.UOM).FirstOrDefault()
                            //                    //           select z.DECIMALS).FirstOrDefault()
                            //                    //DECIMALS = (from c in DBF.M_UOM where c.UOMCD ==  select c.DECIMALS)
                            //                }).ToList();

                            //if (HSN_DATA != null && HSN_DATA.Count > 0)
                            //{
                            //    foreach (var k in HSN_DATA)
                            //    {
                            //        var uom = (from a in DBF.T_VCH_GST
                            //                   where a.AUTONO == auto1 && a.IGSTPER == k.IGSTPER && a.CGSTPER == k.CGSTPER
                            //          && a.SGSTPER == k.SGSTPER && a.HSNCODE == k.HSNCODE
                            //                   select a.UOM).FirstOrDefault();
                            //        double DECIMAL = 0; string umnm = "";
                            //        var uomdata = DBF.M_UOM.Find(uom);
                            //        if (uomdata != null) DECIMAL = Convert.ToDouble(uomdata.DECIMALS);
                            //        if (uomdata != null) umnm = uomdata.UOMNM;
                            //        if (k.TIGSTAMT > 0) flagi = true;
                            //        if (k.TCGSTAMT > 0) flagc = true;

                            //        gstper1 = Convert.ToDouble(k.CGSTPER) + Convert.ToDouble(k.IGSTPER);
                            //        gstamt1 = Convert.ToDouble(k.TCGSTAMT) + Convert.ToDouble(k.TIGSTAMT);

                            //        if (k.HSNCODE != "") { hsncode += k.HSNCODE + "~"; }
                            //        if (k.TQNTY != 0) { hsnqnty += Convert.ToDouble(k.TQNTY).ToString("n" + DECIMAL.ToString()) + " " + umnm + "~"; }
                            //        if (k.TCGSTAMT + k.TIGSTAMT != 0)
                            //        {
                            //            if (k.IGSTPER != 0) hsngstper1 += Cn.Indian_Number_format(k.IGSTPER.ToString(), "0.00") + " %~";
                            //            if (k.TIGSTAMT != 0) hsngstamt1 += Convert.ToDouble(k.TIGSTAMT).ToINRFormat() + "~";
                            //        }
                            //        else
                            //        {
                            //            hsngstper1 += "~";
                            //            hsngstamt1 += "~";
                            //        }
                            //        if (k.TCGSTAMT.retDbl() + k.TCGSTAMT.retDbl() != 0)
                            //        {
                            //            if (k.CGSTPER.retDbl() != 0) hsngstper2 += Cn.Indian_Number_format(k.CGSTPER.ToString(), "0.00") + " %~";
                            //            if (k.TCGSTAMT.retDbl() != 0) hsngstamt2 += Convert.ToDouble(k.TCGSTAMT).ToINRFormat() + "~";
                            //        }
                            //        else
                            //        {
                            //            hsngstper2 += "~";
                            //            hsngstamt2 += "~";
                            //        }
                            //        if (k.TSGSTAMT.retDbl() != 0)
                            //        {
                            //            flags = true;
                            //            if (k.SGSTPER.retDbl() != 0) hsngstper3 += Cn.Indian_Number_format(k.SGSTPER.ToString(), "0.00") + " %~";
                            //            if (k.TSGSTAMT.retDbl() != 0) hsngstamt3 += Convert.ToDouble(k.TSGSTAMT).ToINRFormat() + "~";
                            //        }
                            //        else
                            //        {
                            //            hsngstper3 += "~";
                            //            hsngstamt3 += "~";
                            //        }
                            //        if (k.TAMT != 0) { hsntaxblval += Convert.ToDouble(k.TAMT).ToINRFormat() + "~"; } else { hsntaxblval += "~"; }

                            //        total_qnty = total_qnty + Convert.ToDouble(k.TQNTY);
                            //        total_taxval = total_taxval + Convert.ToDouble(k.TAMT);
                            //        total_gstamt1 = total_gstamt1 + Convert.ToDouble(k.TIGSTAMT);
                            //        total_gstamt2 = total_gstamt2 + Convert.ToDouble(k.TCGSTAMT);
                            //        total_gstamt3 = total_gstamt3 + Convert.ToDouble(k.TSGSTAMT);
                            //    }
                            //}
                            uommaxdecimal = tbl.Rows[i]["qdecimal"].retInt();
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
                            billno = docno;
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
                                    //copymode = "EXTRA COPY"; break;
                                    copymode = "AGENT COPY"; break;
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
                            payterms = tbl.Rows[i]["payterms"].ToString();
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
                            dr1["printby"] = CommVar.UserID();
                            dr1["cancel"] = tbl.Rows[i]["cancel"].ToString();
                            dr1["canc_rem"] = tbl.Rows[i]["canc_rem"].ToString();
                            dr1["copymode"] = copymode;
                            dr1["docno"] = tbl.Rows[i]["docno"].ToString();
                            dr1["docdt"] = tbl.Rows[i]["docdt"] == DBNull.Value ? "" : tbl.Rows[i]["docdt"].ToString().Substring(0, 10).ToString();
                            dr1["upiimg"] = "";
                            dr1["upidesc"] = "";
                            //dr1["areacd"] = tbl.Rows[i]["areacd"].ToString();
                            dr1["invisstime"] = tbl.Rows[i]["invisstime"].retStr();
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
                            dr1["name"] = VE.TEXTBOX9.retStr() == "" ? tbl.Rows[i]["slnm"].ToString() : VE.TEXTBOX9.retStr();
                            dr1["COURCD"] = tbl.Rows[i]["COURCD"].ToString();
                            dr1["COURNM"] = tbl.Rows[i]["COURNM"].ToString();
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
                            if (tbl.Rows[i]["sldistrict"].ToString() != "")
                            {
                                rf = rf + 1;
                                rfld = "sladd" + Convert.ToString(rf);
                                dr1[rfld] = tbl.Rows[i]["sldistrict"].ToString();
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
                                if (tbl.Rows[i]["csldistrict"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "csladd" + Convert.ToString(rf);
                                    dr1[rfld] = tbl.Rows[i]["csldistrict"].ToString();
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
                                //for (int f = 1; f <= 3; f++)
                                for (int f = 1; f <= 4; f++)
                                {
                                    cfld = "othadd" + Convert.ToString(f).ToString();
                                    if (tbl.Rows[i][cfld].ToString() != "")
                                    {
                                        rf = rf + 1;
                                        rfld = "csladd" + Convert.ToString(rf);
                                        dr1[rfld] = tbl.Rows[i][cfld].ToString();
                                    }
                                }
                                if (tbl.Rows[i]["OTHADDPIN"].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "csladd" + Convert.ToString(rf);
                                    dr1[rfld] = "Pin # " + tbl.Rows[i]["OTHADDPIN"].ToString();
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
                            if (VE.MENU_PARA == "SBPOS")
                            {
                                rf = 0;
                                string sfld = ((tbl.Rows[i]["cslcd"].ToString() == tbl.Rows[i]["slcd"].ToString() && tbl.Rows[i]["othadd1"].ToString() != "") || (tbl.Rows[i]["cslcd"].ToString() != tbl.Rows[i]["slcd"].ToString())) ? "" : "c";
                                if (tbl.Rows[i]["nm"].retStr() != "")
                                {
                                    dr1[sfld + "slnm"] = tbl.Rows[i]["nm"].ToString();
                                }
                                if (tbl.Rows[i]["addr1"].retStr() != "")
                                {
                                    for (int f = 1; f <= 12; f++)
                                    {
                                        cfld = "addr" + Convert.ToString(f).ToString();
                                        rf = rf + 1;
                                        rfld = sfld + "sladd" + Convert.ToString(rf);
                                        dr1[rfld] = tbl.Rows[i][cfld].ToString();
                                    }
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
                            dr1["grwt"] = tbl.Rows[i]["grwt"].retDbl();
                            dr1["ntwt"] = tbl.Rows[i]["ntwt"].retDbl();
                            dr1["noofcases"] = tbl.Rows[i]["noofcases"].retDbl();
                            dr1["agentnm"] = tbl.Rows[i]["agslnm"].ToString();
                            dr1["sagslnm"] = tbl.Rows[i]["sagslnm"].ToString();
                            dr1["mutslnm"] = tbl.Rows[i]["mutslnm"].ToString();
                            dr1["bltype"] = tbl.Rows[i]["bltype"].ToString();

                            dr1["revchrg"] = "N";
                            dr1["roamt"] = tbl.Rows[i]["roamt"] == DBNull.Value ? 0 : tbl.Rows[i]["roamt"].retDbl();
                            dr1["tcsper"] = tbl.Rows[i]["tcsper"].retDbl();
                            dr1["tcsamt"] = tbl.Rows[i]["tcsamt"].ToString().retDbl().ToINRFormat(); // == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["tcsamt"]);
                            dr1["blamt"] = blamt.retDbl().ToINRFormat(); // == DBNull.Value ? 0 : Convert.ToDouble(tbl.Rows[i]["blamt"]);

                            //dr1["blamt"] = (tbl.Rows[i]["blamt"].retDbl() - tbl.Rows[i]["ADVRECDAMT"].retDbl()).ToINRFormat();

                            dr1["rupinword"] = Cn.AmountInWords(blamt.retStr()); 
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
                            dr1["plsupply"] = tbl.Rows[i]["plsupply"];
                            dr1["dealsin"] = dealsin;
                            //dr1["blterms"] = blterms;
                            dr1["blterms"] = tbl.Rows[i]["B2C"].ToString() == "N" ? blterms : blterms + "~* I/We hereby declare that though our aggregate turnover in any preceding financial year from 2017-18 onwards is more than the aggregate turnover notified under sub-rule (4) of rule 48, we are not required to prepare an invoice in terms of the provisions of the said sub-rule.";

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
                                if (tbl.Rows[i]["batchdlprint"].ToString() == "Y" && tbl.Rows[i]["batchdtl"].ToString() != "") itdsc += "Batch # " + tbl.Rows[i]["batchdtl"].ToString(); else dr1["batchdtl"] = "";
                                if (tbl.Rows[i]["itcd"].ToString() != "") dr1["caltype"] = 1; else dr1["caltype"] = 0;
                                dr1["agdocno"] = tbl.Rows[i]["agdocno"].ToString();
                                dr1["agdocdt"] = tbl.Rows[i]["agdocdt"] == DBNull.Value ? "" : tbl.Rows[i]["agdocdt"].ToString().Substring(0, 10).ToString();
                                if (VE.MENU_PARA == "PJBL")
                                {
                                    if (doctotprint == false && totalreadyprint == false)
                                    { dr1["BL_TOP_DSC"] = "Being job changes for the following."; }
                                }

                                dr1["slno"] = lslno;
                                dr1["itcd"] = tbl.Rows[i]["itcd"].ToString();
                                dr1["txnslno"] = tbl.Rows[i]["slno"].retInt();
                                dr1["itnm"] = tbl.Rows[i]["itnm"].ToString();
                                dr1["styleno"] = tbl.Rows[i]["styleno"].ToString();
                                dr1["pdesign"] = tbl.Rows[i]["pdesign"].ToString();
                                dr1["itgrpnm"] = tbl.Rows[i]["itgrpnm"].ToString();
                                dr1["fabitcd"] = tbl.Rows[i]["fabitcd"].ToString();
                                dr1["fabitnm"] = tbl.Rows[i]["fabitnm"].ToString();
                                dr1["itdesc"] = itdsc;
                                if (tbl.Rows[i]["batchdlprint"].ToString() == "Y" && tbl.Rows[i]["batchdtl"].ToString() != "") dr1["batchdtl"] = "Batch # " + tbl.Rows[i]["batchdtl"].ToString(); else dr1["batchdtl"] = "";
                                dr1["nos"] = tbl.Rows[i]["nos"].ToString();
                                dr1["hsncode"] = tbl.Rows[i]["hsncode"].ToString();
                                dr1["nos"] = tbl.Rows[i]["nos"] == DBNull.Value ? 0 : (tbl.Rows[i]["nos"]).retDbl();
                                dr1["qnty"] = tbl.Rows[i]["qnty"] == DBNull.Value ? 0 : (tbl.Rows[i]["qnty"]).retDbl();
                                if (VE.MENU_PARA == "PB")
                                {
                                    dr1["netqnty"] = tbl.Rows[i]["qnty"].retDbl();
                                }
                                else { //for bhura time of sales qnty=qnty-flagmtr
                                    dr1["netqnty"] = tbl.Rows[i]["qnty"].retDbl() - tbl.Rows[i]["flagmtr"].retDbl();
                                }
                                dr1["flagmtr"] = tbl.Rows[i]["flagmtr"].retDbl();
                                uomdecimal = tbl.Rows[i]["qdecimal"] == DBNull.Value ? 0 : Convert.ToInt16(tbl.Rows[i]["qdecimal"]);
                                //string dbqtyu = string.Format("{0:N6}", (dr1["qnty"]).retDbl());
                                //if (dbqtyu.Substring(dbqtyu.Length - 2, 2) == "00")
                                //{
                                //    if (uomdecimal > 4) uomdecimal = 4;
                                //}
                                //if (uomdecimal > uommaxdecimal) uommaxdecimal = uomdecimal;

                                dr1["qdecimal"] = uomdecimal;

                                nuomdecimal = tbl.Rows[i]["qdecimal"] == DBNull.Value ? 0 : Convert.ToInt16(tbl.Rows[i]["qdecimal"]);
                                //string dbnqtyu = string.Format("{0:N6}", (dr1["netqnty"]).retDbl());
                                //if (dbqtyu.Substring(dbqtyu.Length - 2, 2) == "00")
                                //{
                                //    if (nuomdecimal > 4) nuomdecimal = 4;
                                //}
                                //if (nuomdecimal > nuommaxdecimal) nuommaxdecimal = nuomdecimal;
                                dr1["nqdecimal"] = nuomdecimal;
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

                                    if (batch_data[a]["nos"].retDbl() == 1)
                                    {
                                        pcsdesc += batch_data[a]["cutlength"].retDbl() == 0 ? "" : batch_data[a]["cutlength"].retDbl().ToString("0.00");
                                    }
                                    else if (batch_data[a]["nos"].retDbl() == 0 || batch_data[a]["cutlength"].retDbl() == 0)
                                    {
                                        pcsdesc += (batch_data[a]["nos"].retDbl() == 0 ? "" : batch_data[a]["nos"].retStr()) + (batch_data[a]["cutlength"].retDbl() == 0 ? "" : batch_data[a]["cutlength"].retDbl().ToString("0.00"));
                                    }
                                    else {
                                        if (VE.Checkbox9 == true)
                                        {
                                            for (int v = 0; v < batch_data[a]["nos"].retDbl(); v++)
                                            {
                                                pcsdesc += v == 0 ? "" : "+";
                                                pcsdesc += batch_data[a]["cutlength"].retDbl().ToString("0.00");
                                            }
                                        }
                                        else
                                        {
                                            pcsdesc += batch_data[a]["cutlength"].retDbl().ToString("0.00") + (batch_data[a]["nos"].retDbl() > 0 ? "x" + batch_data[a]["nos"].retDbl() : "");
                                        }
                                    }
                                    if (batch_data[a]["flagmtr"].retStr() != "")
                                    {
                                        double flagmtr = batch_data[a]["flagmtr"].retDbl() - Math.Truncate(batch_data[a]["flagmtr"].retDbl());
                                        if (flagmtr.retDbl() > 0)
                                        {
                                            pcsdesc += pcsdesc.retStr() == "" ? "" : " ";
                                            pcsdesc += "(F" + flagmtr + ")";
                                        }
                                    }

                                    if (batch_data[a]["tddiscrate"].retDbl() > 0)
                                    {
                                        pcsdesc += pcsdesc.retStr() == "" ? "" : " ";
                                        pcsdesc += "-SL " + batch_data[a]["tddiscrate"].retStr() + "% ";
                                    }
                                    if (batch_data[a]["baleno"].retStr() != "")
                                    {
                                        pcsdesc += pcsdesc.retStr() == "" ? "" : " ";
                                        pcsdesc += "Bale No. " + batch_data[a]["baleno"].retStr();
                                    }
                                }
                                dr1["pcsdesc"] = pcsdesc;
                                #endregion
                                dr1["curr_cd"] = tbl.Rows[i]["curr_cd"].ToString();
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
                                dr1["tddiscamt"] = (tbl.Rows[i]["tddiscamt"]).retDbl().ToINRFormat();
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
                                dr1["discamt"] = (tbl.Rows[i]["discamt"]).retDbl().ToINRFormat();
                                dr1["scmdiscrate"] = (tbl.Rows[i]["scmdiscrate"]).retDbl().ToINRFormat();
                                dr1["scmdiscamt"] = (tbl.Rows[i]["scmdiscamt"]).retDbl().ToINRFormat();
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
                                dnqnty = dnqnty + (dr1["netqnty"].ToString()).retDbl();
                                dbasamt = dbasamt + (dr1["amt"].ToString()).retDbl();
                                ddisc1 = ddisc1 + (tbl.Rows[i]["scmdiscamt"]).retDbl();
                                ddisc2 = ddisc2 + (tbl.Rows[i]["tddiscamt"]).retDbl();
                                ddisc3 = ddisc3 + (tbl.Rows[i]["discamt"]).retDbl();
                                dtxblval = dtxblval + (dr1["txblval"].ToString()).retDbl();
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

                            if (doctotprint == true && totalreadyprint == false)
                            {
                                dr1 = IR.NewRow();
                                dr1["autono"] = auto1 + copymode;
                                dr1["copymode"] = copymode;
                                dr1["docno"] = tbl.Rows[i]["docno"].ToString();

                                dr1["itnm"] = "Total";
                                dr1["nos"] = dnos;
                                dr1["qnty"] = dqnty;
                                dr1["netqnty"] = dnqnty;
                                //if (VE.DOCCD == "SOOS" && uommaxdecimal == 6) uommaxdecimal = 4;
                                dr1["qdecimal"] = uommaxdecimal;
                                dr1["amt"] = dbasamt;
                                dr1["scmdiscamt"] = ddisc1.ToINRFormat();
                                dr1["tddiscamt"] = ddisc2.ToINRFormat();
                                dr1["discamt"] = ddisc3.ToINRFormat();
                                dr1["txblval"] = dtxblval.ToINRFormat();
                                dr1["cgstamt"] = dcgstamt;
                                dr1["sgstamt"] = dsgstamt;
                                dr1["netamt"] = dnetamt;
                                dr1["titdiscamt"] = ddisc1 + ddisc2 + ddisc3;
                                dr1["curr_cd"] = tbl.Rows[i]["curr_cd"].ToString();
                                totalreadyprint = true;
                                goto docstart;
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
                compaddress = Salesfunc.retCompAddress("", grpemailid);
                stremail = compaddress.retCompValue("email");

                string ccemail = grpemailid;
                if (ccemail == "") ccemail = stremail;

                //ReportDocument reportdocument = new ReportDocument();
                string complogo = Salesfunc.retCompLogo();
                EmailControl EmailControl = new EmailControl();

                string complogosrc = complogo;
                string compfixlogosrc = "c:\\improvar\\" + CommVar.Compcd(UNQSNO) + "fix.jpg";
                string sendemailids = "";
                string rptfile = "Order.rpt";
                //if (VE.MENU_PARA == "PJBL") rptfile = "SaleBill_Job.rpt";
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
                            DataTable tblslcd = Master_HelpFa.retslcdCont(emlslcd, "S", true);
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
                                if (CommVar.ClientCode(UNQSNO) == "DIWH")
                                {
                                    body += "copy no " + rsemailid1.Rows[iz]["docno"] + " dated " + rsemailid1.Rows[iz]["docdt"] + " ";
                                }
                                else
                                {
                                    body += "<tr>";
                                    body += "<td>" + rsemailid1.Rows[iz]["docno"] + "</td>";
                                    body += "<td>" + rsemailid1.Rows[iz]["docdt"] + "</td>";
                                    body += "<td style='text-align:right'>" + Cn.Indian_Number_format((rsemailid1.Rows[iz]["blamt"]).retDbl().ToString(), "0.00") + "</td>";
                                    body += "</tr>";
                                }


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
                            string legalname = compaddress.retCompValue("legalname").retStr() == "" ? "" : "(" + compaddress.retCompValue("legalname") + ")";
                            reportdocument.SetDataSource(rsemailid1);
                            reportdocument.SetParameterValue("billheading", blhead);
                            reportdocument.SetParameterValue("complogo", Master_Help.retCompLogo());
                            reportdocument.SetParameterValue("complogo1", Master_Help.retCompLogo1());
                            reportdocument.SetParameterValue("compnm", compaddress.retCompValue("compnm"));
                            reportdocument.SetParameterValue("compadd", compaddress.retCompValue("compadd"));
                            reportdocument.SetParameterValue("compcommu", compaddress.retCompValue("compcommu"));
                            reportdocument.SetParameterValue("compstat", compaddress.retCompValue("compstat"));
                            reportdocument.SetParameterValue("locaadd", compaddress.retCompValue("locaadd"));
                            reportdocument.SetParameterValue("locacommu", compaddress.retCompValue("locacommu"));
                            reportdocument.SetParameterValue("locastat", compaddress.retCompValue("locastat"));
                            reportdocument.SetParameterValue("legalname", legalname);
                            reportdocument.SetParameterValue("corpadd", compaddress.retCompValue("corpadd"));
                            reportdocument.SetParameterValue("corpcommu", compaddress.retCompValue("corpcommu"));
                            reportdocument.SetParameterValue("formerlynm", compaddress.retCompValue("formerlynm"));
                            //if (CommVar.ClientCode(UNQSNO) == "DIWH") reportdocument.SetParameterValue("compStamp", Master_Help.retCompStamp());
                            reportdocument.SetParameterValue("compStamp", Master_Help.retCompStamp());
                            Response.Buffer = false;
                            Response.ClearContent();
                            Response.ClearHeaders();
                            Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                            stream.Seek(0, SeekOrigin.Begin);
                            if (!System.IO.Directory.Exists(path_Save)) { System.IO.Directory.CreateDirectory(path_Save); }
                            //path_Save = path_Save +"\\"+ doccd + "-" + emlslcd + "-" + ldt.Substring(6, 4) + ldt.Substring(3, 2) + ldt.Substring(0, 2) + ".pdf";
                            var edocno = (Regex.Replace(billno, @"[^0-9a-zA-Z_]+", ""));
                            path_Save = path_Save + "\\" + doccd + "-" + edocno + ".pdf";
                            if (System.IO.File.Exists(path_Save)) { System.IO.File.Delete(path_Save); }

                            reportdocument.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, path_Save);
                            reportdocument.Close();
                            // email

                            sql = "select b.compnm, b.add1, b.add2, b.add3, b.add4, b.add5, b.add6, b.state, b.country, b.panno, b.cinno, b.propname, ";
                            sql += "nvl(a.regdoffsame,'Y') regdoffsame, a.addtype, a.linkloccd, ";
                            sql += "a.add1 ladd1, a.add2 ladd2, a.add3 ladd3, a.add4 ladd4, a.add5 ladd5, a.add6 ladd6, ";
                            sql += "a.state lstate, a.country lcountry, a.statecd lstatecd, a.phno3, a.phno1std,a.phno2std, ";
                            sql += "a.gstno, a.phno1, a.phno2, a.regemailid ";
                            sql += "from " + Scmf + ".m_loca a, " + Scmf + ".m_comp b ";
                            sql += "where a.compcd='" + COM + "' and a.loccd='" + LOC + "' and a.compcd=b.compcd(+) ";
                            DataTable comptbl = Master_Help.SQLquery(sql);
                            string compMobile = comptbl.Rows[0]["phno1"].ToString();
                            string compEmail = comptbl.Rows[0]["regemailid"].ToString();



                            //System.Net.Mail.Attachment attchmail = new System.Net.Mail.Attachment(path_Save);
                            List<System.Net.Mail.Attachment> attchmail = new List<System.Net.Mail.Attachment>();// System.Net.Mail.Attachment(path_Save);
                            attchmail.Add(new System.Net.Mail.Attachment(path_Save));
                            string template = CommVar.ClientCode(UNQSNO) == "DIWH" ? "Salebill_DIWH.htm" : "Orderbill_ANKAN.htm";
                            string[,] emlaryBody = new string[9, 2];
                            if (VE.TEXTBOX5 != null)
                            {
                                emlaryBody[0, 0] = "{slnm}"; emlaryBody[0, 1] = slnm;
                                emlaryBody[1, 0] = "{tbody}"; emlaryBody[1, 1] = body;
                                emlaryBody[2, 0] = "{username}"; emlaryBody[2, 1] = System.Web.HttpContext.Current.Session["UR_NAME"].ToString();
                                emlaryBody[3, 0] = "{compname}"; emlaryBody[3, 1] = compaddress.retCompValue("compnm");
                                emlaryBody[4, 0] = "{usermobno}"; emlaryBody[4, 1] = MOBILE;
                                emlaryBody[5, 0] = "{complogo}"; emlaryBody[5, 1] = complogosrc;
                                emlaryBody[6, 0] = "{compfixlogo}"; emlaryBody[6, 1] = compfixlogosrc;
                                emlaryBody[7, 0] = "{compmobno}"; emlaryBody[7, 1] = compMobile;
                                emlaryBody[8, 0] = "{compemail}"; emlaryBody[8, 1] = compEmail;
                                bool emailsent = EmailControl.SendHtmlFormattedEmail(VE.TEXTBOX5, "Order copy", template, emlaryBody, attchmail, grpemailid);
                                if (emailsent == true)
                                {
                                    sendemailids = sendemailids + VE.TEXTBOX5 + ";";
                                    if (VE.Checkbox10 == true)
                                    {
                                        foreach (var autono in totalautono)
                                        {
                                            Master_Help.insT_TXNSTATUS(autono, "E", "", VE.TEXTBOX5);
                                        }
                                    }
                                }
                                else
                                {
                                    sendemailids = " not able to send on " + VE.TEXTBOX5 + ";";
                                }
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
                                emlaryBody[7, 0] = "{compmobno}"; emlaryBody[7, 1] = compMobile;
                                emlaryBody[8, 0] = "{compemail}"; emlaryBody[8, 1] = compEmail;
                                //bool emailsent = EmailControl.SendHtmlFormattedEmail(rsemailid[z].email.ToString() + ccemailid, "Sales Bill copy", "Salebill.htm", emlaryBody, attchmail, grpemailid);
                                bool emailsent = EmailControl.SendHtmlFormattedEmail(rsemailid[z].email.ToString() + ccemailid, "Order copy", template, emlaryBody, attchmail, grpemailid);
                                if (emailsent == true)
                                {
                                    sendemailids = sendemailids + rsemailid[z].email.ToString() + ";";
                                    if (VE.Checkbox10 == true)
                                    {
                                        Master_Help.insT_TXNSTATUS(rsemailid1.Rows[z]["autono"].retStr().Substring(0, rsemailid1.Rows[z]["autono"].ToString().Length - 1), "E", "SBILL", rsemailid[z].email.ToString());
                                    }
                                }
                                else
                                {
                                    sendemailids = sendemailids + " not able to send on " + rsemailid[z].email.ToString();
                                }
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
                    string legalname = compaddress.retCompValue("legalname").retStr() == "" ? "" : "(" + compaddress.retCompValue("legalname") + ")";
                    reportdocument.SetDataSource(IR);
                    reportdocument.SetParameterValue("billheading", blhead);
                    reportdocument.SetParameterValue("complogo", Master_Help.retCompLogo());
                    reportdocument.SetParameterValue("complogo1", Master_Help.retCompLogo1());
                    reportdocument.SetParameterValue("compnm", compaddress.retCompValue("compnm"));
                    reportdocument.SetParameterValue("compadd", compaddress.retCompValue("compadd"));
                    reportdocument.SetParameterValue("compcommu", compaddress.retCompValue("compcommu"));
                    reportdocument.SetParameterValue("compstat", compaddress.retCompValue("compstat"));
                    reportdocument.SetParameterValue("locaadd", compaddress.retCompValue("locaadd"));
                    reportdocument.SetParameterValue("locacommu", compaddress.retCompValue("locacommu"));
                    reportdocument.SetParameterValue("locastat", compaddress.retCompValue("locastat"));
                    reportdocument.SetParameterValue("legalname", legalname);
                    reportdocument.SetParameterValue("corpadd", compaddress.retCompValue("corpadd"));
                    reportdocument.SetParameterValue("corpcommu", compaddress.retCompValue("corpcommu"));
                    reportdocument.SetParameterValue("formerlynm", compaddress.retCompValue("formerlynm"));
                    //if (CommVar.ClientCode(UNQSNO) == "DIWH") reportdocument.SetParameterValue("compStamp", Master_Help.retCompStamp());
                    reportdocument.SetParameterValue("compStamp", Master_Help.retCompStamp());
                    if (printemail == "PDF")
                    {

                        Response.Buffer = false;
                        Response.ClearContent();
                        Response.ClearHeaders();
                        Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        stream.Seek(0, SeekOrigin.Begin);
                        reportdocument.Close();
                        reportdocument.Dispose(); GC.Collect();
                        var edocno = (Regex.Replace(billno, @"[^0-9a-zA-Z_]+", ""));
                        //path_Save = path_Save + "\\" + doccd + "-" + edocno + ".pdf";
                        FileName = FileName.retRepname();// + "-" + edocno;
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
                    if (printemail == "Excel")
                    {
                        string path_Save = @"C:\Ipsmart\" + doccd + (VE.TEXTBOX8.retStr() == "" ? VE.FDOCNO : VE.TEXTBOX8.retStr()) + ".xls";
                        string exlfile = doccd + (VE.TEXTBOX8.retStr() == "" ? VE.FDOCNO : VE.TEXTBOX8.retStr()) + ".xls";
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
                        stream.Seek(0, SeekOrigin.Begin);
                        reportdocument.Close(); reportdocument.Dispose(); GC.Collect();

                        if (VE.Checkbox10 == true)
                        {
                            foreach (var autono in totalautono)
                            {
                                Master_Help.insT_TXNSTATUS(autono, "P", "SBILL", "");
                            }
                        }

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

        //public ActionResult Rep_OrdPrint(ReportViewinHtml VE, FormCollection FC)
        //{
        //    string msg = ""; double minimumStockCoalculatedOn = 15;
        //    try
        //    {
        //        DataTable rstbl;
        //        string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
        //        string doccd = VE.DOCCD, slcd = VE.TEXTBOX1, fdt = VE.FDT, tdt = VE.TDT, fdocno = VE.FDOCNO, tdocno = VE.TDOCNO;
        //        if (slcd != null) slcd = "'" + slcd + "'";


        //        string sql = "";
        //        sql += "select a.SLNO,a.AUTONO, a.STKDRCR, a.STKTYPE, a.FREESTK, a.ITCD, c.ITNM, c.STYLENO, c.PCSPERSET, c.UOMCD, ";
        //        sql += "b.docno,b.docdt,i.slcd,H.SLNM,h.add1||' '||h.add2||' '||h.add3||' '||h.add4||' '||h.add5||' '||h.add6||' '||h.add7 address,";
        //        sql += "a.sizecd, a.rate, a.scmdiscamt, a.discamt, a.qnty,A.DELVDT,a.ITREM,a.PDESIGN,c.itgrpcd, d.itgrpnm,c.fabitcd, ";
        //        sql += "e.itnm fabitnm,a.colrcd,a.partcd,f.colrnm,g.sizenm,h.partnm,a.rate from ";
        //        sql += scm + ".T_SORDDTL a, " + scm + ".T_CNTRL_HDR b, ";
        //        sql += scm + ".m_sitem c, " + scm + ".m_group d, " + scm + ".m_sitem e, " + scm + ".m_color f, " + scm + ".m_size g, " + scm + ".m_parts h, "
        //            + scm + ".T_SORD i," + scmf + ".m_subleg h  ";
        //        sql += "where a.autono = b.autono and a.autono = i.autono and  a.itcd = c.itcd(+) and c.itgrpcd=d.itgrpcd and c.fabitcd=e.itcd(+) ";
        //        sql += "and a.colrcd=f.colrcd(+) and a.sizecd=g.sizecd(+) and a.partcd=h.partcd(+) and i.slcd= h.slcd and ";
        //        sql += "nvl(b.cancel,'N')='N' and b.compcd='" + COM + "' and b.loccd='" + LOC + "' and ";
        //        if (slcd != null) sql += "a.slcd in (" + slcd + ") and ";
        //        if (fdt != null) sql += "b.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and ";
        //        if (tdt != null) sql += "b.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and ";
        //        if (fdocno != null) sql += "b.doconlyno >= '" + fdocno + "' and ";
        //        if (fdocno != null) sql += "b.doconlyno <= '" + tdocno + "' and ";
        //        sql += "b.doccd = '" + doccd + "'  ";
        //        sql += "order by styleno ";
        //        rstbl = Master_Help.SQLquery(sql);

        //        if (rstbl.Rows.Count == 0)
        //        {
        //            return RedirectToAction("NorsPendOrds", "RPTViewer", new { errmsg = "No Records Found !!" });
        //        }


        //        DataTable IR = new DataTable();
        //        //IR.Columns.Add("docno", typeof(string), "");
        //        //IR.Columns.Add("docdt", typeof(string), "");
        //        //IR.Columns.Add("slcd", typeof(string), "");
        //        //IR.Columns.Add("SLNM", typeof(string), "");
        //        //IR.Columns.Add("address", typeof(string), "");
        //        IR.Columns.Add("slno", typeof(int), "");
        //        IR.Columns.Add("itgrpcd", typeof(string), "");
        //        IR.Columns.Add("itgrpnm", typeof(string), "");
        //        IR.Columns.Add("ITCD", typeof(string), "");
        //        IR.Columns.Add("ITNM", typeof(string), "");
        //        IR.Columns.Add("STYLENO", typeof(string), "");
        //        IR.Columns.Add("ITREM", typeof(string), "");
        //        IR.Columns.Add("fabitcd", typeof(string), "");
        //        IR.Columns.Add("PDESIGN", typeof(string), "");
        //        IR.Columns.Add("STKDRCR", typeof(string), "");
        //        IR.Columns.Add("STKTYPE", typeof(string), "");
        //        IR.Columns.Add("FREESTK", typeof(string), "");
        //        IR.Columns.Add("PCSPERSET", typeof(string), "");
        //        IR.Columns.Add("sizecd", typeof(string), "");
        //        IR.Columns.Add("UOMCD", typeof(string), "");
        //        IR.Columns.Add("qnty", typeof(double), "");
        //        IR.Columns.Add("rate", typeof(double), "");
        //        IR.Columns.Add("scmdiscamt", typeof(double), "");
        //        IR.Columns.Add("discamt", typeof(double), "");
        //        IR.Columns.Add("DELVDT", typeof(string), "");


        //        for (int m = 0; m <= rstbl.Rows.Count - 1; m++)
        //        {
        //            DataRow fin1 = IR.NewRow();
        //            //fin1["docno"] = rstbl.Rows[m]["docno"].ToString();
        //            //fin1["docdt"] = rstbl.Rows[m]["docdt"].ToString().Remove(10);
        //            //fin1["slcd"] = rstbl.Rows[m]["slcd"].ToString();
        //            //fin1["SLNM"] = rstbl.Rows[m]["SLNM"].ToString();
        //            //fin1["address"] = rstbl.Rows[m]["address"].ToString();
        //            fin1["slno"] = rstbl.Rows[m]["slno"].retInt();
        //            fin1["itgrpcd"] = rstbl.Rows[m]["itgrpcd"].ToString();
        //            fin1["itgrpnm"] = rstbl.Rows[m]["itgrpnm"].ToString();
        //            fin1["ITCD"] = rstbl.Rows[m]["ITCD"].ToString();
        //            fin1["ITNM"] = rstbl.Rows[m]["ITNM"].ToString();
        //            fin1["STYLENO"] = rstbl.Rows[m]["STYLENO"].ToString();
        //            fin1["fabitcd"] = rstbl.Rows[m]["fabitcd"].ToString();
        //            fin1["PDESIGN"] = rstbl.Rows[m]["PDESIGN"].ToString();
        //            fin1["STKDRCR"] = rstbl.Rows[m]["STKDRCR"].ToString();
        //            fin1["STKTYPE"] = rstbl.Rows[m]["STKTYPE"].ToString();
        //            fin1["FREESTK"] = rstbl.Rows[m]["FREESTK"].ToString();
        //            fin1["PCSPERSET"] = rstbl.Rows[m]["PCSPERSET"].ToString();
        //            fin1["sizecd"] = rstbl.Rows[m]["sizecd"].ToString();
        //            fin1["UOMCD"] = rstbl.Rows[m]["UOMCD"].ToString();
        //            fin1["qnty"] = rstbl.Rows[m]["qnty"].ToString();
        //            fin1["rate"] = rstbl.Rows[m]["rate"].ToString();
        //            fin1["scmdiscamt"] = rstbl.Rows[m]["scmdiscamt"].ToString();
        //            fin1["discamt"] = rstbl.Rows[m]["discamt"].ToString();
        //            fin1["DELVDT"] = rstbl.Rows[m]["DELVDT"].ToString();
        //            fin1["ITREM"] = rstbl.Rows[m]["ITREM"].ToString();
        //            IR.Rows.Add(fin1);
        //        }
        //        string compaddress;
        //        compaddress = Salesfunc.retCompAddress();

        //        ExcelPackage workbook = new ExcelPackage();
        //        ExcelWorksheet worksheet = workbook.Workbook.Worksheets.Add("Sheet1");
        //        int row = 1;
        //        worksheet.Cells["A" + row].Value = compaddress.retCompValue("compnm");
        //        worksheet.Cells["A" + row].Style.Font.Bold = true;
        //        row++;
        //        if (compaddress.retCompValue("compadd").retStr() != "")
        //        {
        //            worksheet.Cells["A" + row++].Value = compaddress.retCompValue("compadd");
        //        }
        //        if (compaddress.retCompValue("compcommu").retStr() != "")
        //        {
        //            worksheet.Cells["A" + row++].Value = compaddress.retCompValue("compcommu");
        //        }
        //        if (compaddress.retCompValue("compstat").retStr() != "")
        //        {
        //            worksheet.Cells["A" + row++].Value = compaddress.retCompValue("compstat");
        //        }
        //        if (compaddress.retCompValue("corpadd").retStr() != "")
        //        {
        //            worksheet.Cells["A" + row++].Value = compaddress.retCompValue("corpadd");
        //        }
        //        if (compaddress.retCompValue("corpcommu").retStr() != "")
        //        {
        //            worksheet.Cells["A" + row++].Value = compaddress.retCompValue("corpcommu");
        //        }
        //        if (compaddress.retCompValue("locaadd").retStr() != "")
        //        {
        //            worksheet.Cells["A" + row++].Value = compaddress.retCompValue("locaadd");
        //        }
        //        if (compaddress.retCompValue("locacommu").retStr() != "")
        //        {
        //            worksheet.Cells["A" + row++].Value = compaddress.retCompValue("locacommu");
        //        }
        //        if (compaddress.retCompValue("locastat").retStr() != "")
        //        {
        //            worksheet.Cells["A" + row++].Value = compaddress.retCompValue("locastat");
        //        }
        //        row++;
        //        worksheet.Cells["A" + row++].Value = "To : " + rstbl.Rows[0]["slnm"] + "[" + rstbl.Rows[0]["slcd"] + "]";
        //        worksheet.Cells["A" + row++].Value = rstbl.Rows[0]["address"];

        //        row++;
        //        worksheet.Cells["A" + row++].Value = "Order No: " + rstbl.Rows[0]["docno"];
        //        worksheet.Cells["A" + row++].Value = "Order Date: " + rstbl.Rows[0]["docdt"].retStr().Remove(10);

        //        row++;
        //        worksheet.Cells["A" + row++].LoadFromDataTable(IR, true);
        //        Response.ClearContent();
        //        Response.Clear();
        //        Response.Buffer = true;
        //        Response.Charset = "";
        //        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //        Response.AddHeader("content-disposition", "attachment;filename=Order" + VE.FDOCNO.Replace("/", "-") + ".xlsx");
        //        using (MemoryStream MyMemoryStream = new MemoryStream())
        //        {
        //            workbook.SaveAs(MyMemoryStream);
        //            MyMemoryStream.WriteTo(Response.OutputStream);
        //            Response.Flush();
        //            Response.End();
        //        }
        //        workbook.Dispose();



        //        return Content("Download sucessfully");

        //    }
        //    catch (Exception ex)
        //    {
        //        Cn.SaveException(ex, "");
        //        return Content(ex.Message);
        //    }
        //}
        private void GenerateBorder(ExcelWorksheet worksheet, int row, int column)
        {//make the borders of cell F6 thick
            worksheet.Cells[row, column].Style.Border.Right.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[row, column].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[row, column].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            worksheet.Cells[row, column].Style.Border.Left.Style = ExcelBorderStyle.Thin;
        }
    }
}