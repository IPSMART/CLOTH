using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using System.Collections.Generic;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Improvar.Controllers
{
    public class Khasra_BaleTrnf_PrintController : Controller
    {
        // GET: Khasra_BaleTrnf_Print
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        MasterHelpFa MasterHelpFa = new MasterHelpFa();

        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult Khasra_BaleTrnf_Print()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
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
                    switch (VE.MENU_PARA)
                    {
                        case "KHSR":
                            ViewBag.formname = "Khasra Printing"; break;
                        case "TRFB":
                            ViewBag.formname = "Sotck Transfer Bale Printing"; break;
                        case "BLTR":
                            ViewBag.formname = "Receive from Mutia Report"; break;
                        case "TRWB":
                            ViewBag.formname = "Sotck Transfer With Waybill  Bale Printing"; break;
                        default: ViewBag.formname = ""; break;
                    }
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    string reptype = "KHSR";
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
        [HttpPost]
        public ActionResult Khasra_BaleTrnf_Print(ReportViewinHtml VE, FormCollection FC, string Command)
        {
            try
            {
                //Cn.getQueryString(VE);

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
                string slcd = VE.TEXTBOX3;

                string scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO), COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO);
                string yr_cd = CommVar.YearCode(UNQSNO);
                string section = "", filename = "";
                switch (Command)
                {
                    case "Print": section = "Print"; break;
                    case "Show Report": section = "Show Report"; break;
                    case "Excel": section = "Excel"; break;
                    default: return (View());
                }
                DataTable tbl = new DataTable(); ; DataTable restbl = new DataTable("restbl"); DataTable restblgodown = new DataTable("restblgodown");
                if (VE.MENU_PARA == "BLTR")
                {
                    string Scm = CommVar.CurSchema(UNQSNO);
                    string str = "";
                    str += "select distinct a.autono,a.blautono,a.slno,a.drcr,a.lrdt,a.lrno,a.baleyr,a.baleno,a.blslno,a.rslno, z.pcstype, ";
                    str += "b.itcd, c.styleno, c.itnm,c.uomcd,b.nos,b.qnty,b.pageno,b.pageslno,c.styleno||' '||c.itnm itstyle,d.prefno,d.prefdt from ";

                    str += "( select distinct a.autono, a.blautono, a.slno, a.baleno, a.blslno ";
                    str += "from " + Scm + ".T_BALE a ";
                    str += "  ) y, ";

                    str += "( select a.autono, a.txnslno, max(a.pcstype) pcstype ";
                    str += "from " + Scm + ".t_batchdtl a group by a.autono, a.txnslno) z, ";

                    str += Scm + ".T_BALE a," + Scm + ".T_TXNDTL b," + Scm + ".M_SITEM c," + Scm + ".T_TXN d," + scm + ".t_cntrl_hdr e ";

                    //str += "where y.autono=a.autono(+) and y.slno=a.slno(+) and y.autono=z.autono(+) and y.slno=z.txnslno(+) and ";
                    str += "where y.autono=a.autono(+) and y.slno=a.slno(+) and a.blautono=z.autono(+) and a.blslno=z.txnslno(+) and ";
                    str += "y.blautono=b.autono(+) and b.itcd=c.itcd(+) and y.blautono=d.autono(+) and y.baleno=b.baleno(+) and y.blslno=b.slno(+) and a.autono=e.autono(+)  ";
                    if (doccd.retStr() != "") str += "and e.doccd ='" + doccd + "' ";
                    if (fdate.retStr() != "") str += "and e.docdt >= to_date('" + fdate + "', 'dd/mm/yyyy') ";
                    if (tdate.retStr() != "") str += "and e.docdt <= to_date('" + tdate + "', 'dd/mm/yyyy') ";
                    if (fdocno != "") str += "and e.doconlyno >= '" + fdocno + "' ";
                    if (tdocno != "") str += "and e.doconlyno <= '" + tdocno + "'   ";
                    if (CommVar.ClientCode(UNQSNO) == "SNFP")
                    {
                        str += "order by a.baleno,a.rslno ";
                    }
                    else {
                        str += "order by a.rslno,a.baleno ";
                    }
                    tbl = masterHelp.SQLquery(str);
                }
                else if (VE.MENU_PARA == "KHSR" || VE.MENU_PARA == "TRWB" || VE.MENU_PARA == "TRFB")
                {
                    string sql = "select a.gocd, k.gonm, a.blautono,a.slno, a.blslno, a.baleno, a.baleyr, e.lrno, e.lrdt, ";
                    sql += "g.itcd, h.styleno, h.itnm, h.uomcd, h.itgrpcd, i.itgrpnm, ";
                    sql += "g.nos, g.qnty, h.styleno||' '||h.itnm  itstyle, listagg(j.shade,',') within group (order by j.autono, j.txnslno) as shade, ";
                    sql += "g.pageno, g.pageslno, g.rate, f.prefno, f.prefdt,a.autono,f.gocd hdrgocd,l.gonm hdrgonm,l.goadd1 hdrgoadd1,l.goadd2 hdrgoadd2,l.goadd3 hdrgoadd3,l.gophno hdrgophno,l.goemail hdrgoemail,a.slno,a.docno,a.docdt,a.baleopen, nvl(m.decimals, 0) qdecimal, ";
                    sql += " a.slcd,nvl(a.fullname, a.slnm) slnm,a.regemailid, a.add1 sladd1, a.add2 sladd2, a.add3 sladd3, a.add4 sladd4, a.add5 sladd5, a.add6 sladd6, a.add7 sladd7,  ";
                    sql += " a.gstno, a.panno, trim(a.regmobile || decode(a.regmobile, null, '', ',') || a.slphno || decode(a.phno1, null, '', ',' || a.phno1)) phno, a.state, a.country, a.statecd, a.actnameof slactnameof  ";

                    sql += "from  ( ";
                    sql += "select c.gocd, a.blautono, a.blslno, a.baleno, a.baleyr, a.baleyr || a.baleno balenoyr, ";
                    //sql += "c.qnty,a.autono,decode(a.baleopen,'Y',a.slno-1000,a.slno)slno,d.docno,d.docdt,a.baleopen,e.slcd,f.slnm,f.fullname,f.regemailid,f.add1,f.add2, f.add3,f.add4,f.add5,f.add6,f.add7,f.gstno, f.panno,f.regmobile, ";
                    sql += "c.qnty,a.autono,decode(a.baleopen,'Y',a.slno-5000,a.slno)slno,d.docno,d.docdt,a.baleopen,e.slcd,f.slnm,f.fullname,f.regemailid,f.add1,f.add2, f.add3,f.add4,f.add5,f.add6,f.add7,f.gstno, f.panno,f.regmobile, ";
                    sql += "f.slphno,f.phno1,f.state,f.country, f.statecd, f.actnameof ";
                    sql += "from " + scm + ".t_bale a, " + scm + ".t_bale_hdr b, " + scm + ".t_txndtl c, " + scm + ".t_cntrl_hdr d ," + scm + ".t_txn e," + scmf + ".m_subleg f ";
                    sql += "where a.autono = b.autono(+) and a.autono = d.autono(+)and c.autono = e.autono(+) and e.slcd=f.slcd(+) and ";
                    //sql += "a.autono=c.autono(+) and decode(a.baleopen,'Y',a.slno-1000,a.slno)=c.slno(+) and c.stkdrcr in ('D','C') and ";
                    sql += "a.autono=c.autono(+) and decode(a.baleopen,'Y',a.slno-5000,a.slno)=c.slno(+) and c.stkdrcr in ('D','C') and ";
                    sql += "d.compcd = '" + COM + "' and nvl(d.cancel, 'N') = 'N' and ";
                    sql += "d.loccd='" + LOC + "' and d.yr_cd = '" + yr_cd + "'  ";
                    if (doccd.retStr() != "") sql += "and d.doccd ='" + doccd + "' ";
                    if (fdate.retStr() != "") sql += "and d.docdt >= to_date('" + fdate + "', 'dd/mm/yyyy') ";
                    if (tdate.retStr() != "") sql += "and d.docdt <= to_date('" + tdate + "', 'dd/mm/yyyy') ";
                    if (fdocno != "") sql += "and d.doconlyno >= '" + fdocno + "' ";
                    if (tdocno != "") sql += "and d.doconlyno <= '" + tdocno + "'   ";
                    sql += ") a, ";

                    sql += "" + scm + ".t_txntrans e, " + scm + ".t_txn f, " + scm + ".t_txndtl g, " + scm + ".m_sitem h, " + scm + ".m_group i, " + scm + ".t_batchdtl j, " + scmf + ".m_godown k, " + scmf + ".m_godown l, " + scmf + ".m_uom m, " + scmf + ".m_subleg n ";
                    sql += "where a.blautono = e.autono(+) and a.blautono = f.autono(+) and f.slcd=n.slcd(+) and  ";
                    sql += "g.autono=j.autono(+) and g.slno=j.txnslno(+) and a.blautono = g.autono(+) and a.blslno = g.slno(+) and g.itcd = h.itcd(+) and f.gocd = l.gocd(+)  ";
                    //if (itgrpcd != "") sql += "and f.itgrpcd in (" + itgrpcd + ")  ";
                    //if (itcd != "") sql += "and a.itcd in (" + itcd + ")  ";
                    //if (baleno != "") sql += "and a.baleno||baleyr in (" + baleno + ")  ";
                    //sql += "and h.uomcd=m.uomcd(+) and h.itgrpcd = i.itgrpcd(+) and a.gocd=k.gocd(+)  and a.slno <1000 ";
                    sql += "and h.uomcd=m.uomcd(+) and h.itgrpcd = i.itgrpcd(+) and a.gocd=k.gocd(+)  and a.slno <5000 ";
                    sql += "group by a.gocd, k.gonm, a.blautono, a.blslno, a.baleno, a.baleyr, e.lrno, e.lrdt, g.itcd, h.styleno, h.itnm, h.uomcd, h.itgrpcd, i.itgrpnm, ";
                    sql += "g.nos, g.qnty, h.styleno||' '||h.itnm, g.pageno, g.pageslno,g.rate, f.prefno, f.prefdt,a.autono,f.gocd,l.gonm,l.goadd1,l.goadd2,l.goadd3,l.gophno,l.goemail,a.slno,a.docno,a.docdt,a.baleopen,m.decimals ";
                    sql += ",a.slcd,nvl(a.fullname, a.slnm),a.regemailid, a.add1, a.add2, a.add3, a.add4, a.add5, a.add6, a.add7,  ";
                    sql += " a.gstno, a.panno, trim(a.regmobile || decode(a.regmobile, null, '', ',') || a.slphno || decode(a.phno1, null, '', ',' || a.phno1)), a.state, a.country, a.statecd, a.actnameof  ";
                    //sql += "order by a.autono, f.prefno, a.baleno,a.slno ";
                    if(CommVar.ClientCode(UNQSNO)== "SNFP" && VE.MENU_PARA == "TRFB")
                    { sql += "order by a.autono,a.baleno,f.prefno,a.slno "; }
                    else { sql += "order by a.autono,a.slno,f.prefno, a.baleno "; }

                    restbl = masterHelp.SQLquery(sql);
                    sql = "select a.autono,a.gocd hdrgocd,b.gonm hdrgonm,b.goadd1 hdrgoadd1,b.goadd2 hdrgoadd2,b.goadd3 hdrgoadd3,b.gophno hdrgophno,b.goemail hdrgoemail ";
                    sql += "from " + scm + ".t_txn a, " + scmf + ".m_godown b, " + scm + ".t_cntrl_hdr c ";
                    sql += "where a.gocd=b.gocd(+) and a.autono=c.autono ";
                    sql += "and c.compcd = '" + COM + "' and nvl(c.cancel, 'N') = 'N' and ";
                    sql += "c.loccd='" + LOC + "' and c.yr_cd = '" + yr_cd + "'  ";
                    if (doccd.retStr() != "") sql += "and c.doccd ='" + doccd + "' ";
                    if (fdate.retStr() != "") sql += "and c.docdt >= to_date('" + fdate + "', 'dd/mm/yyyy') ";
                    if (tdate.retStr() != "") sql += "and c.docdt <= to_date('" + tdate + "', 'dd/mm/yyyy') ";
                    restblgodown = masterHelp.SQLquery(sql);
                }
                #region old query
                //string sql = "select a.gocd, k.gonm, a.blautono, a.blslno, a.baleno, a.baleyr, e.lrno, e.lrdt, ";
                //sql += "g.itcd, h.styleno, h.itnm, h.uomcd, h.itgrpcd, i.itgrpnm, ";
                //sql += "g.nos, g.qnty, h.styleno||' '||h.itnm  itstyle, listagg(j.shade,',') within group (order by j.autono, j.txnslno) as shade, ";
                //sql += "g.pageno, g.pageslno, g.rate, f.prefno, f.prefdt,a.autono,f.gocd hdrgocd,l.gonm hdrgonm,l.goadd1 hdrgoadd1,l.goadd2 hdrgoadd2,l.goadd3 hdrgoadd3,l.gophno hdrgophno,l.goemail hdrgoemail,a.slno,a.docno,a.docdt,a.baleopen, nvl(m.decimals, 0) qdecimal, ";
                //sql += " f.slcd,nvl(n.fullname, n.slnm) slnm,n.regemailid, n.add1 sladd1, n.add2 sladd2, n.add3 sladd3, n.add4 sladd4, n.add5 sladd5, n.add6 sladd6, n.add7 sladd7,  ";
                //sql += " n.gstno, n.panno, trim(n.regmobile || decode(n.regmobile, null, '', ',') || n.slphno || decode(n.phno1, null, '', ',' || n.phno1)) phno, n.state, n.country, n.statecd, n.actnameof slactnameof  ";
                //sql += "from  ( ";
                //sql += "select c.gocd, a.blautono, a.blslno, a.baleno, a.baleyr, a.baleyr || a.baleno balenoyr, ";
                //sql += "sum(case c.stkdrcr when 'D' then c.qnty when 'C' then c.qnty*-1 end) qnty,a.autono,a.slno,d.docno,d.docdt,a.baleopen ";
                //sql += "from " + scm + ".t_bale a, " + scm + ".t_bale_hdr b, " + scm + ".t_txndtl c, " + scm + ".t_cntrl_hdr d ";
                //sql += "where a.autono = b.autono(+) and a.autono = d.autono(+) and ";
                //sql += "a.autono=c.autono(+) and a.slno=c.slno(+) and c.stkdrcr in ('D','C') and ";
                //sql += "d.compcd = '" + COM + "' and nvl(d.cancel, 'N') = 'N' and ";
                //sql += "d.loccd='" + LOC + "' and d.yr_cd = '" + yr_cd + "'  ";
                //if (doccd.retStr() != "") sql += "and d.doccd ='" + doccd + "' ";
                //if (fdate.retStr() != "") sql += "and d.docdt >= to_date('" + fdate + "', 'dd/mm/yyyy') ";
                //if (tdate.retStr() != "") sql += "and d.docdt <= to_date('" + tdate + "', 'dd/mm/yyyy') ";
                //if (fdocno != "") sql += "and d.doconlyno >= '" + fdocno + "' ";
                //if (tdocno != "") sql += "and d.doconlyno <= '" + tdocno + "'   ";
                //sql += "group by c.gocd, a.blautono, a.blslno, a.baleno, a.baleyr, a.baleyr || a.baleno,a.autono,a.slno,d.docno,d.docdt,a.baleopen ";
                //sql += ") a, ";
                //sql += "" + scm + ".t_txntrans e, " + scm + ".t_txn f, " + scm + ".t_txndtl g, " + scm + ".m_sitem h, " + scm + ".m_group i, " + scm + ".t_batchdtl j, " + scmf + ".m_godown k, " + scmf + ".m_godown l, " + scmf + ".m_uom m, " + scmf + ".m_subleg n ";
                //sql += "where a.blautono = e.autono(+) and a.blautono = f.autono(+) and f.slcd=n.slcd(+) and  ";
                //sql += "g.autono=j.autono(+) and g.slno=j.txnslno(+) and a.blautono = g.autono(+) and a.blslno = g.slno(+) and g.itcd = h.itcd(+) and f.gocd = l.gocd(+)  ";
                ////if (itgrpcd != "") sql += "and f.itgrpcd in (" + itgrpcd + ")  ";
                ////if (itcd != "") sql += "and a.itcd in (" + itcd + ")  ";
                ////if (baleno != "") sql += "and a.baleno||baleyr in (" + baleno + ")  ";
                //sql += "and h.uomcd=m.uomcd(+) and h.itgrpcd = i.itgrpcd(+) and a.gocd=k.gocd(+) and nvl(a.qnty, 0) > 0 and a.slno <1000 ";
                //sql += "group by a.gocd, k.gonm, a.blautono, a.blslno, a.baleno, a.baleyr, e.lrno, e.lrdt, g.itcd, h.styleno, h.itnm, h.uomcd, h.itgrpcd, i.itgrpnm, ";
                //sql += "g.nos, g.qnty, h.styleno||' '||h.itnm, g.pageno, g.pageslno,g.rate, f.prefno, f.prefdt,a.autono,f.gocd,l.gonm,l.goadd1,l.goadd2,l.goadd3,l.gophno,l.goemail,a.slno,a.docno,a.docdt,a.baleopen,m.decimals ";
                //sql += ",f.slcd,nvl(n.fullname, n.slnm),n.regemailid, n.add1, n.add2, n.add3, n.add4, n.add5, n.add6, n.add7,  ";
                //sql += " n.gstno, n.panno, trim(n.regmobile || decode(n.regmobile, null, '', ',') || n.slphno || decode(n.phno1, null, '', ',' || n.phno1)), n.state, n.country, n.statecd, n.actnameof  ";
                //sql += "order by a.autono, f.prefno, a.baleno ";
                #endregion

                #region Print
                if (section == "Print")
                {
                    if (restbl.Rows.Count == 0)
                    {
                        return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "No Records Found !!" });
                    }
                    DataTable IR = new DataTable();
                    IR.Columns.Add("autono", typeof(string), "");
                    IR.Columns.Add("docno", typeof(string), "");
                    IR.Columns.Add("docdt", typeof(string), "");
                    IR.Columns.Add("hdrgocd", typeof(string), "");
                    IR.Columns.Add("hdrgonm", typeof(string), "");
                    IR.Columns.Add("hdrgoadd1", typeof(string), "");
                    IR.Columns.Add("hdrgoadd2", typeof(string), "");
                    IR.Columns.Add("hdrgoadd3", typeof(string), "");
                    IR.Columns.Add("hdrgophno", typeof(string), "");
                    IR.Columns.Add("hdrgoemail", typeof(string), "");
                    IR.Columns.Add("dealsin", typeof(string), "");

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
                    IR.Columns.Add("regemailid", typeof(string), "");
                    //det
                    IR.Columns.Add("slno", typeof(double), "");
                    IR.Columns.Add("prefno", typeof(string), "");
                    IR.Columns.Add("prefdt", typeof(string), "");
                    IR.Columns.Add("styleno", typeof(string), "");
                    IR.Columns.Add("shade", typeof(string), "");
                    IR.Columns.Add("baleno", typeof(string), "");
                    IR.Columns.Add("nos", typeof(double), "");
                    IR.Columns.Add("qnty", typeof(double), "");
                    IR.Columns.Add("qdecimal", typeof(double), "");
                    IR.Columns.Add("lrno", typeof(string), "");
                    IR.Columns.Add("pageno", typeof(string), "");
                    IR.Columns.Add("gonm", typeof(string), "");
                    IR.Columns.Add("baleopen", typeof(string), "");
                    IR.Columns.Add("totalbaleno", typeof(double), "");

                    int maxR = restbl.Rows.Count - 1;
                    Int32 i = 0;
                    while (i <= maxR)
                    {
                        string autono = restbl.Rows[i]["autono"].ToString();
                        double countbaleno = restbl.AsEnumerable().Where(a => a.Field<string>("autono") == autono && a.Field<string>("baleno").retStr() != "").Select(b => b.Field<string>("baleno")).Distinct().Count();
                        var rm1 = restblgodown.Select("autono = '" + autono + "'");
                        while (restbl.Rows[i]["autono"].ToString() == autono)
                        {
                            DataRow dr1 = IR.NewRow();
                            dr1["autono"] = restbl.Rows[i]["autono"].ToString();
                            dr1["docno"] = restbl.Rows[i]["docno"].ToString();
                            dr1["docdt"] = restbl.Rows[i]["docdt"].retStr().Remove(10);

                            if (VE.MENU_PARA == "TRFB")
                            {
                                if (rm1 != null && rm1.Count() > 0)
                                {
                                    for (int a = 0; a <= rm1.Count() - 1; a++)
                                    {
                                        //dr1["hdrgocd"] = rm1[a]["hdrgocd"].ToString();
                                        //dr1["hdrgonm"] = rm1[a]["hdrgonm"].ToString();
                                        //dr1["hdrgoadd1"] = rm1[a]["hdrgoadd1"].ToString();
                                        //dr1["hdrgoadd2"] = rm1[a]["hdrgoadd2"].ToString();
                                        //dr1["hdrgoadd3"] = rm1[a]["hdrgoadd3"].ToString();
                                        //dr1["hdrgophno"] = rm1[a]["hdrgophno"].ToString();
                                        //dr1["hdrgoemail"] = rm1[a]["hdrgoemail"].ToString();

                                        string goadd = rm1[a]["hdrgoadd1"].ToString() + " " + rm1[a]["hdrgoadd2"].ToString() + " " + rm1[a]["hdrgoadd3"].ToString();
                                        goadd = goadd.Trim();
                                        if (rm1[a]["hdrgophno"].ToString() != "") goadd = goadd + " Phone : " + rm1[a]["hdrgophno"].ToString();

                                        if (goadd.retStr() != "") dr1["hdrgonm"] = "Godown : " + rm1[a]["hdrgonm"].ToString();
                                        dr1["hdrgoadd1"] = goadd;
                                    }
                                }
                            }
                            dr1["slcd"] = restbl.Rows[i]["slcd"].ToString();
                            dr1["slnm"] = restbl.Rows[i]["slnm"].ToString();
                            dr1["regemailid"] = restbl.Rows[i]["regemailid"].ToString();

                            string cfld = "", rfld = ""; int rf = 0;
                            for (int f = 1; f <= 6; f++)
                            {
                                cfld = "sladd" + Convert.ToString(f).ToString();
                                if (restbl.Rows[i][cfld].ToString() != "")
                                {
                                    rf = rf + 1;
                                    rfld = "sladd" + Convert.ToString(rf);
                                    dr1[rfld] = restbl.Rows[i][cfld].ToString();
                                }
                            }
                            rf = rf + 1;
                            rfld = "sladd" + Convert.ToString(rf);
                            dr1[rfld] = restbl.Rows[i]["state"].ToString() + " [ Code - " + restbl.Rows[i]["statecd"].ToString() + " ]";
                            if (restbl.Rows[i]["gstno"].ToString() != "")
                            {
                                rf = rf + 1;
                                rfld = "sladd" + Convert.ToString(rf);
                                dr1[rfld] = "GST # " + restbl.Rows[i]["gstno"].ToString();
                            }
                            if (restbl.Rows[i]["panno"].ToString() != "")
                            {
                                rf = rf + 1;
                                rfld = "sladd" + Convert.ToString(rf);
                                dr1[rfld] = "PAN # " + restbl.Rows[i]["panno"].ToString();
                            }
                            if (restbl.Rows[i]["phno"].ToString() != "")
                            {
                                rf = rf + 1;
                                rfld = "sladd" + Convert.ToString(rf);
                                dr1[rfld] = "Ph. # " + restbl.Rows[i]["phno"].ToString();
                            }
                            if (restbl.Rows[i]["slactnameof"].ToString() != "")
                            {
                                rf = rf + 1;
                                rfld = "sladd" + Convert.ToString(rf);
                                dr1[rfld] = restbl.Rows[i]["slactnameof"].ToString();
                            }

                            dr1["slno"] = restbl.Rows[i]["slno"].ToString();
                            dr1["prefno"] = restbl.Rows[i]["prefno"].ToString();
                            dr1["prefdt"] = restbl.Rows[i]["prefdt"].retStr() == "" ? "" : restbl.Rows[i]["prefdt"].retStr().Remove(10);
                            dr1["styleno"] = restbl.Rows[i]["styleno"].ToString();
                            dr1["shade"] = restbl.Rows[i]["shade"].ToString();
                            dr1["baleno"] = restbl.Rows[i]["baleno"].ToString();
                            dr1["nos"] = restbl.Rows[i]["nos"].ToString();
                            dr1["qnty"] = restbl.Rows[i]["qnty"].ToString();
                            dr1["qdecimal"] = restbl.Rows[i]["qdecimal"].retDbl();
                            dr1["lrno"] = restbl.Rows[i]["lrno"].ToString();
                            dr1["pageno"] = restbl.Rows[i]["pageno"].ToString() + (restbl.Rows[i]["pageslno"].retStr() == "" ? "" : "/" + restbl.Rows[i]["pageslno"].retStr());
                            dr1["gonm"] = restbl.Rows[i]["gonm"].ToString();
                            dr1["baleopen"] = restbl.Rows[i]["baleopen"].retStr() == "Y" ? "Yes" : "";
                            dr1["totalbaleno"] = countbaleno;
                            dr1["dealsin"] = "";

                            IR.Rows.Add(dr1);
                            i = i + 1;
                            if (i > maxR) break;
                        }

                        if (i > maxR) break;
                    }
                    //
                    string rptfile = "Khasra_BaleTrnf.rpt";
                    if (VE.TEXTBOX6 != null) rptfile = VE.TEXTBOX6;
                    string rptname = "~/Report/" + rptfile; // "SaleBill.rpt";

                    string compaddress;
                    string gocd = "";
                    compaddress = Salesfunc.retCompAddress(gocd);

                    string blhead = "";
                    switch (VE.MENU_PARA)
                    {
                        case "KHSR":
                            blhead = "Khasra"; break;
                        case "TRFB":
                            blhead = "Sotck Transfer Bale"; break;
                        default: blhead = ""; break;
                    }
                    ReportDocument reportdocument = new ReportDocument();
                    reportdocument.Load(Server.MapPath(rptname));
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
                    reportdocument.SetParameterValue("menu_para", VE.MENU_PARA);

                    Response.Buffer = false;
                    Response.ClearContent();
                    Response.ClearHeaders();
                    Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                    reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
                    stream.Seek(0, SeekOrigin.Begin);
                    return new FileStreamResult(stream, "application/pdf");
                }
                #endregion
                #region Show Report
                else if (section == "Show Report")
                {
                    if (tbl.Rows.Count == 0)
                    {
                        return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "No Records Found !!" });
                    }
                    if (tbl.Rows.Count != 0)
                    {
                        DataTable IR = new DataTable("mstrep");

                        Models.PrintViewer PV = new Models.PrintViewer();
                        HtmlConverter HC = new HtmlConverter();

                        HC.RepStart(IR, 2);
                        HC.GetPrintHeader(IR, "prefno", "string", "c,12", "Bill No.");
                        HC.GetPrintHeader(IR, "prefdt", "string", "c,12", "Date");
                        HC.GetPrintHeader(IR, "itstyle", "string", "c,25", "Short No.");
                        HC.GetPrintHeader(IR, "pcstype", "string", "c,10", "Ctg");
                        HC.GetPrintHeader(IR, "slno", "string", "c,7", "Slno");
                        HC.GetPrintHeader(IR, "baleno", "string", "c,12", "Bale");
                        HC.GetPrintHeader(IR, "nos", "double", "c,15", "Nos");
                        HC.GetPrintHeader(IR, "qnty", "double", "c,15,3", "Qnty");
                        HC.GetPrintHeader(IR, "uomcd", "string", "c,15", "Uom");
                        HC.GetPrintHeader(IR, "lrno", "string", "c,12", "Lrno");
                        HC.GetPrintHeader(IR, "pageno", "string", "c,12", "PageNo/PageSlNo.");

                        Int32 rNo = 0; Int32 i = 0; Int32 maxR = 0;
                        i = 0; maxR = tbl.Rows.Count - 1;

                        while (i <= maxR)
                        {
                            string blautono = tbl.Rows[i]["blautono"].retStr();
                            while (tbl.Rows[i]["blautono"].retStr() == blautono)
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["prefno"] = tbl.Rows[i]["prefno"].retStr();
                                IR.Rows[rNo]["prefdt"] = tbl.Rows[i]["prefdt"].retDateStr();
                                IR.Rows[rNo]["itstyle"] = tbl.Rows[i]["itstyle"].retStr();
                                IR.Rows[rNo]["pcstype"] = tbl.Rows[i]["pcstype"].retStr();
                                IR.Rows[rNo]["slno"] = tbl.Rows[i]["rslno"].retStr();
                                IR.Rows[rNo]["baleno"] = tbl.Rows[i]["baleno"].retStr();
                                IR.Rows[rNo]["nos"] = tbl.Rows[i]["nos"].retDbl();
                                IR.Rows[rNo]["qnty"] = tbl.Rows[i]["qnty"].retDbl();
                                IR.Rows[rNo]["uomcd"] = tbl.Rows[i]["uomcd"].retStr();
                                IR.Rows[rNo]["lrno"] = tbl.Rows[i]["lrno"].retStr();
                                IR.Rows[rNo]["pageno"] = tbl.Rows[i]["pageno"].retStr() + "/" + tbl.Rows[i]["pageslno"].retStr();
                                //snfb requirement new line add after every row
                                IR.Rows.Add("");
                                //end snfb
                                i = i + 1;
                                if (i > maxR) break;
                            }
                        }

                        string pghdr1 = "";

                        pghdr1 = "Receive from Mutia Details as on from " + fdate + " to " + tdate;

                        PV = HC.ShowReport(IR, "T_BiltyR_Mutia", pghdr1, "", true, true, "P", false);

                        TempData["T_BiltyR_Mutia"] = PV;
                        return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = "T_BiltyR_Mutia" });
                    }
                }
                #endregion
                #region EXCEL
                else {
                    if (VE.MENU_PARA == "KHSR")
                    {
                        tbl = restbl;
                        filename = "Khasra_".retRepname();
                    }
                    else if (VE.MENU_PARA == "TRWB")
                    { tbl = restbl; filename = "Stk Trnf with Waybill (Bale)_".retRepname(); }
                    else if (VE.MENU_PARA == "TRFB")
                    { tbl = restbl; filename = "Stk Trnf w/o Waybill (Bale)_".retRepname(); }
                    else if (VE.MENU_PARA == "BLTR") { filename = "Receive from Mutia_".retRepname(); }
                    if (tbl.Rows.Count == 0)
                    {
                        return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "No Records Found !!" });
                    }
                    string Excel_Header = "Bill No." + "|" + "Date" + "|" + "Short No." + "|" + "Ctg" + "|" + "Slno" + "|" + " Bale" + "|" + "Nos" + "|" + "Qnty" + "|";
                    Excel_Header = Excel_Header + "Uom" + "|" + "Lrno" + "|" + "PageNo";
                    var sheetName = "";
                    //sheetName = VE.MENU_PARA == "KHSR" ? "T_Bilty_Khasra" : VE.MENU_PARA == "TRWB" ? "Stk Trnf with Waybill" : "T_BiltyR_Mutia";
                    sheetName = VE.MENU_PARA == "KHSR" ? "T_Bilty_Khasra" : VE.MENU_PARA == "TRWB" ? "Stk Trnf with Waybill" : VE.MENU_PARA == "TRFB" ? "Stk Trnf w/o Waybill" : "T_BiltyR_Mutia";

                    ExcelPackage ExcelPkg = new ExcelPackage();
                    ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add(sheetName);
                    wsSheet1.Column(1).Width = 11.57;
                    wsSheet1.Column(2).Width = 7.86;
                    wsSheet1.Column(3).Width = 21.43;
                    wsSheet1.Column(4).Width = 4.43;
                    wsSheet1.Column(5).Width = 3.57;
                    wsSheet1.Column(6).Width = 7.43;
                    wsSheet1.Column(6).Style.Font.Bold = true;
                    wsSheet1.Column(7).Width = 3.29;
                    wsSheet1.Column(8).Width = 5.57;
                    wsSheet1.Column(8).Style.Font.Bold = true;
                    wsSheet1.Column(8).Style.Numberformat.Format = "0.00";
                    wsSheet1.Column(9).Width = 3.99;
                    wsSheet1.Column(10).Width = 12.29;
                    wsSheet1.Column(11).Width = 7.43;
                    var modelTable = wsSheet1.Cells[1, 1];
                    using (ExcelRange Rng = wsSheet1.Cells["A1:K3"])
                    {
                        Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.White);
                        Rng.Style.Font.Size = 12; Rng.Style.Font.Bold = true;
                        wsSheet1.Cells["A1:A1"].Value = CommVar.CompName(UNQSNO);
                        wsSheet1.Cells["A2:A2"].Value = CommVar.LocName(UNQSNO);
                        //wsSheet1.Cells["A3:A3"].Value = VE.MENU_PARA == "KHSR" ? "Khasra Details as on " + fdate + " to " + tdate : VE.MENU_PARA == "TRWB" ? "Stk Trnf with Waybill (Bale)Details as on " + fdate + " to " + tdate : "Receive from Mutia Details as on " + fdate + " to " + tdate;
                        wsSheet1.Cells["A3:A3"].Value = VE.MENU_PARA == "KHSR" ? "Khasra Details as on " + fdate + " to " + tdate : VE.MENU_PARA == "TRWB" ? "Stk Trnf with Waybill (Bale)Details as on " + fdate + " to " + tdate : VE.MENU_PARA == "TRFB" ? "Stk Trnf w/o Waybill (Bale)Details as on " + fdate + " to " + tdate : "Receive from Mutia Details as on " + fdate + " to " + tdate;
                    }
                    using (ExcelRange Rng = wsSheet1.Cells["A4:K4"])
                    {
                        Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        Rng.Style.Font.Bold = true; Rng.Style.Font.Size = 9;
                        Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.SkyBlue);
                        Rng.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        Rng.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        string[] Header = Excel_Header.Split('|');
                        for (int j = 0; j < Header.Length; j++)
                        {
                            wsSheet1.Cells[4, j + 1].Value = Header[j];
                        }
                    }
                    int exlrowno = 5; var rslno = 0; var baleno = "";
                    for (int i = 0; i < tbl.Rows.Count; i++)
                    {
                        var curslno = 0; var cbaleno = "";
                        if (VE.MENU_PARA == "KHSR") { curslno = tbl.Rows[i]["slno"].retShort(); }
                        //else if (VE.MENU_PARA == "TRWB") { cbaleno = tbl.Rows[i]["baleno"].retStr(); }
                        else if (VE.MENU_PARA == "TRWB" || VE.MENU_PARA == "TRFB") { cbaleno = tbl.Rows[i]["baleno"].retStr(); }
                        else if (VE.MENU_PARA == "BLTR") { curslno = tbl.Rows[i]["rslno"].retShort(); }
                        if ((curslno != rslno || cbaleno != baleno) && i != 0)
                        {
                            modelTable = wsSheet1.Cells[exlrowno, 1];
                            modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[exlrowno, 1].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                            modelTable = wsSheet1.Cells[exlrowno, 2];
                            modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[exlrowno, 2].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                            modelTable = wsSheet1.Cells[exlrowno, 3];
                            modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[exlrowno, 3].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                            modelTable = wsSheet1.Cells[exlrowno, 4];
                            modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[exlrowno, 4].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                            modelTable = wsSheet1.Cells[exlrowno, 5];
                            modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[exlrowno, 5].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                            modelTable = wsSheet1.Cells[exlrowno, 6];
                            modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[exlrowno, 6].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                            modelTable = wsSheet1.Cells[exlrowno, 7];
                            modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[exlrowno, 7].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                            modelTable = wsSheet1.Cells[exlrowno, 8];
                            modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[exlrowno, 8].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                            modelTable = wsSheet1.Cells[exlrowno, 9];
                            modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[exlrowno, 9].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                            modelTable = wsSheet1.Cells[exlrowno, 10];
                            modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[exlrowno, 10].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                            modelTable = wsSheet1.Cells[exlrowno, 11];
                            modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            wsSheet1.Cells[exlrowno, 11].Value = ""; wsSheet1.Row(exlrowno).Height = 20;
                            exlrowno++;
                        }
                        wsSheet1.Row(exlrowno).Style.Font.Size = 10;
                        wsSheet1.Cells[exlrowno, 1].Value = tbl.Rows[i]["prefno"].retStr();
                        wsSheet1.Cells[exlrowno, 2].Value = tbl.Rows[i]["prefdt"].retDateStr("yy", "dd/MM/yy");
                        wsSheet1.Cells[exlrowno, 3].Value = tbl.Rows[i]["itstyle"].retStr();
                        wsSheet1.Cells[exlrowno, 4].Value = VE.MENU_PARA == "KHSR" || VE.MENU_PARA == "TRWB" || VE.MENU_PARA == "TRFB" ? "" : tbl.Rows[i]["pcstype"].retStr();
                        wsSheet1.Cells[exlrowno, 5].Value = VE.MENU_PARA == "KHSR" || VE.MENU_PARA == "TRWB" || VE.MENU_PARA == "TRFB" ? tbl.Rows[i]["slno"].retShort() : tbl.Rows[i]["rslno"].retShort();
                        //wsSheet1.Cells[exlrowno, 4].Value = VE.MENU_PARA == "KHSR" || VE.MENU_PARA == "TRWB" ? "" : tbl.Rows[i]["pcstype"].retStr();
                        //wsSheet1.Cells[exlrowno, 5].Value = VE.MENU_PARA == "KHSR" || VE.MENU_PARA == "TRWB" ? tbl.Rows[i]["slno"].retShort() : tbl.Rows[i]["rslno"].retShort();
                        wsSheet1.Cells[exlrowno, 6].Value = tbl.Rows[i]["baleno"].retStr();
                        wsSheet1.Cells[exlrowno, 7].Value = tbl.Rows[i]["nos"].retDbl();
                        wsSheet1.Cells[exlrowno, 8].Value = tbl.Rows[i]["qnty"].retDbl().toRound(2);
                        wsSheet1.Cells[exlrowno, 9].Value = tbl.Rows[i]["uomcd"].retStr();
                        wsSheet1.Cells[exlrowno, 10].Value = tbl.Rows[i]["lrno"].retStr();
                        wsSheet1.Cells[exlrowno, 11].Value = tbl.Rows[i]["pageno"].retStr() + "/" + tbl.Rows[i]["pageslno"].retStr();
                        if (VE.MENU_PARA == "KHSR")
                        { rslno = tbl.Rows[i]["slno"].retShort(); }
                        else if (VE.MENU_PARA == "TRWB" || VE.MENU_PARA == "TRFB") { baleno = tbl.Rows[i]["baleno"].retStr(); }
                        //else if (VE.MENU_PARA == "TRWB") { baleno = tbl.Rows[i]["baleno"].retStr(); }
                        else if (VE.MENU_PARA == "BLTR") { rslno = tbl.Rows[i]["rslno"].retShort(); }
                        exlrowno++;
                    }
                    #region Blank Row at the end
                    {
                        modelTable = wsSheet1.Cells[exlrowno, 1];
                        modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[exlrowno, 1].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                        modelTable = wsSheet1.Cells[exlrowno, 2];
                        modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[exlrowno, 2].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                        modelTable = wsSheet1.Cells[exlrowno, 3];
                        modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[exlrowno, 3].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                        modelTable = wsSheet1.Cells[exlrowno, 4];
                        modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[exlrowno, 4].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                        modelTable = wsSheet1.Cells[exlrowno, 5];
                        modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[exlrowno, 5].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                        modelTable = wsSheet1.Cells[exlrowno, 6];
                        modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[exlrowno, 6].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                        modelTable = wsSheet1.Cells[exlrowno, 7];
                        modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[exlrowno, 7].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                        modelTable = wsSheet1.Cells[exlrowno, 8];
                        modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[exlrowno, 8].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                        modelTable = wsSheet1.Cells[exlrowno, 9];
                        modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[exlrowno, 9].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                        modelTable = wsSheet1.Cells[exlrowno, 10];
                        modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[exlrowno, 10].Value = ""; wsSheet1.Row(exlrowno).Height = 20;

                        modelTable = wsSheet1.Cells[exlrowno, 11];
                        modelTable.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        modelTable.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        modelTable.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        modelTable.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        wsSheet1.Cells[exlrowno, 11].Value = ""; wsSheet1.Row(exlrowno).Height = 20;
                        exlrowno++;
                    }
                    #endregion

                    Response.Clear();
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("Content-Disposition", "attachment; filename=" + filename + ".xlsx");
                    Response.BinaryWrite(ExcelPkg.GetAsByteArray());
                    Response.Flush();
                    Response.Close();
                    Response.End();
                }
                #endregion


            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
            return null;
        }


    }
}


