using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using CrystalDecisions.CrystalReports.Engine;
using System.Data;
using System.IO;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class Rep_Doc_PrintController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        //BarCode CnBarCode = new BarCode();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        string sql = "";
        string LOC =  CommVar.Loccd(CommVar.getQueryStringUNQSNO()), COM = CommVar.Compcd(CommVar.getQueryStringUNQSNO()), scm1 = CommVar.CurSchema(CommVar.getQueryStringUNQSNO()), scmf = CommVar.FinSchema(CommVar.getQueryStringUNQSNO());

        // GET: Rep_Doc_Print
        public ActionResult Rep_Doc_Print(string reptype = "")
        {
            try {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    Rep_Doc_Print VE;
                    if (TempData["printparameter"] == null)
                    {
                        VE = new Rep_Doc_Print();
                    }
                    else
                    {
                        VE = (Rep_Doc_Print)TempData["printparameter"];
                    }
                    ViewBag.formname = VE.CaptionName;
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    //ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(),  CommVar.CurSchema(UNQSNO));
                    using (ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO)))
                    {
                        DataTable repformat = Salesfunc.getRepFormat(VE.RepType, VE.DOCCD);
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

                        VE.DOCNM = (from j in DB.M_DOCTYPE where j.DOCCD == VE.DOCCD select j.DOCNM).SingleOrDefault();
                        //VE = (Rep_Doc_Print)Cn.EntryCommonLoading(VE, VE.PermissionID);
                        VE.DefaultView = true;
                        VE.ExitMode = 1;
                        VE.DefaultDay = 0;
                        return View(VE);
                    }
                } }
            catch (Exception ex)
            {
                Rep_Doc_Print VE = new Rep_Doc_Print();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }

        }
        public ActionResult GetDOC_Code(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.DOCCD_help(val, "", ""));
                }
                else
                {
                    string str = masterHelp.DOCCD_help(val, "", "");
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetDOC_Number(string val, string Code)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.DOCNO_help(val, Code));
                }
                else
                {
                    string str = masterHelp.DOCNO_help(val, Code);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetItemGroupDetails(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            return Content("");
            //return PartialView("_Help2", masterHelp.ITGRPCD_help(val));

        }
        public ActionResult GetSLCDhelp(string val)
        {
            try
            {
                string LINK_CD = "D,C";
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.SubLeg_Help(val, LINK_CD));
                }
                else
                {
                    string str = masterHelp.SubLeg_Help(val, LINK_CD);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult ItemGroupDetails(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_GROUP where (c.ITGRPCD == val) select c);
            if (query.Any())
            {
                string str = "";
                foreach (var i in query)
                {
                    str = i.ITGRPCD + Cn.GCS() + i.ITGRPNM;
                }
                return Content(str);
            }
            else
            {
                return Content("0");
            }
        }
        [HttpPost]
        public ActionResult Rep_StkAdj_Print(Rep_Doc_Print VE)
        {
            sql = "";
            sql += "select a.itcd, a.stkdrcr, a.slno, '' batchautono, '' batchno, nvl(a.qnty,0) qnty, ";
            sql += "f.itnm||' ['||n.sizenm||'] '||nvl(a.stktype,'') itnm, a.itrem, ";
            sql += "n.sizenm, n.print_seq, ";
            sql += "c.doccd, c.docno, to_char(c.docdt,'dd/mm/yyyy') docdt, b.gocd, g.gonm, i.itgrpnm, j.uomnm, j.decimals, k.docrem remarks, ";
            sql += "decode(a.stkdrcr,'D','IN','OUT') inout, a.autono, c.usr_id, c.usr_entdt, m.user_name ";
            sql += "from " + scm1 + ".t_txndtl a, " + scm1 + ".t_txn b, " + scm1 + ".t_cntrl_hdr c, ";
            sql += scm1 + ".m_sitem f, " + scm1 + ".m_godown g, " + scm1 + ".m_group i, ";
            sql += scmf + ".m_uom j, " + scm1 + ".t_txnoth k, improvar.user_appl m, " + scm1 + ".m_size n ";
            sql += "where a.autono = b.autono(+) and a.autono = c.autono(+) and c.usr_id=m.user_id(+) and ";
            sql += "a.itcd = f.itcd(+) and a.sizecd=n.sizecd(+) and f.itgrpcd = i.itgrpcd(+) and ";
            sql += "c.compcd='" + COM + "' and c.loccd='" + LOC + "' and ";
            if (VE.FDT != null) sql += "c.docdt >= to_date('" + VE.FDT + "','dd/mm/yyyy') and ";
            if (VE.TDT != null) sql += "c.docdt <= to_date('" + VE.TDT + "','dd/mm/yyyy') and ";
            if (VE.FDOCNO != null) sql += "c.doconlyno >= '" + VE.FDOCNO + "' and ";
            if (VE.TDOCNO != null) sql += "c.doconlyno <= '" + VE.TDOCNO + "' and ";
            if (VE.DOCCD != null) sql += "c.doccd='" + VE.DOCCD + "' and ";
            sql += "f.uomcd = j.uomcd(+) and b.gocd = g.gocd(+) and a.autono=k.autono(+) ";
            sql += "order by docdt, docno, stkdrcr, slno, print_seq, sizenm ";
            DataTable tbl = masterHelp.SQLquery(sql);

            ReportDocument reportdocument = new ReportDocument();

            string[] compaddress;
            string gocd = "";
            if (VE.OtherPara != null) gocd = VE.OtherPara;
            compaddress = Salesfunc.retCompAddress(gocd).Split(Convert.ToChar(Cn.GCS()));
            string blhead = "STOCK ADJUSTMENT", rptname = "~/Report/" + "StockAdjCnv.rpt";
            if (VE.RepType == "STKCNV") blhead = "STOCK CONVERSION";

            reportdocument.Load(Server.MapPath(rptname));
            reportdocument.SetDataSource(tbl);
            reportdocument.SetParameterValue("complogo", Salesfunc.retCompLogo());
            reportdocument.SetParameterValue("compnm", compaddress[0]);
            reportdocument.SetParameterValue("compadd", compaddress[1]);
            reportdocument.SetParameterValue("compstat", compaddress[2]);
            reportdocument.SetParameterValue("locaadd", compaddress[3]);
            reportdocument.SetParameterValue("locastat", compaddress[4]);
            reportdocument.SetParameterValue("billheading", blhead);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);
            reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
            return new FileStreamResult(stream, "application/pdf");
        }
        [HttpPost]
        public ActionResult Rep_IssueChallan_Print(Rep_Doc_Print VE)
        {
            string scmI = CommVar.InvSchema(UNQSNO);
            string hddsp = "", chlntype = "";
            string gooditem = "", jobnm = "", barcode = "";
            string repname = "JobIssue";
            string QntyIn = "", jobcd = "", gocd = "";
            string[] otherpara = VE.OtherPara.Split(',');
            jobcd = otherpara[0];
            if (otherpara.Count() > 1) gocd = otherpara[1];
            bool progprint = false;

            sql = "select a.autono, a.doctag, max(c.mtrljobcd) mtrljobcd ";
            sql += "from " + scm1 + ".t_txn a, " + scm1 + ".t_cntrl_hdr b, " + scm1 + ".t_txndtl c ";
            sql += "where a.autono = b.autono(+) and a.autono=c.autono(+) and b.doccd = '" + VE.DOCCD + "' and ";
            sql += "b.docdt >= to_date('" + VE.FDT + "','dd/mm/yyyy') and b.docdt <= to_date('" + VE.FDT + "','dd/mm/yyyy') and ";
            sql += "b.doconlyno >= '" + VE.FDOCNO + "' and b.doconlyno <= '" + VE.TDOCNO + "' and ";
            sql += "b.compcd = '" + COM + "' and b.loccd = '" + LOC + "' ";
            sql += "group by a.autono, a.doctag ";
            DataTable tbltmp = masterHelp.SQLquery(sql);
            if (tbltmp.Rows.Count > 0)
            {
                if (tbltmp.Rows[0]["mtrljobcd"].ToString() == "TO") jobcd = tbltmp.Rows[0]["mtrljobcd"].ToString();
            }

            switch (jobcd)
            {
                case "KT": hddsp = "Issue Challan (Knitter)"; gooditem = "Yarn"; jobnm = "Knitting Job"; repname = "YarnIssue"; progprint = true; break;
                case "YD": hddsp = "Issue Challan (Yarn Dyer)"; gooditem = "Yarn"; jobnm = "Dying Job"; repname = "YarnIssue"; progprint = true; break;
                case "FP": hddsp = "Issue to Processor"; gooditem = "Fabrics"; jobnm = "Processing Job"; repname = "FabIssue"; progprint = true; break;
                case "DY": hddsp = "Issue Challan (Dyer)"; gooditem = "Fabrics"; jobnm = "Dying Job"; repname = "FabIssue"; progprint = true; break;
                case "BL": hddsp = "Issue Challan (Bleacher)"; gooditem = "Fabrics"; jobnm = "Bleaching Job"; repname = "FabIssue"; progprint = true; break;
                case "CT": hddsp = "Issue Challan (Cutter)"; gooditem = "Fabrics"; jobnm = "Cutting Job"; QntyIn = "D"; progprint = true; break;
                case "PR": hddsp = "Issue Challan Printer"; gooditem = "Cut fabrics"; jobnm = "Printing Job"; QntyIn = "B"; break;
                case "EM": hddsp = "Issue Challan (Embroider)"; gooditem = "cut fabrics"; jobnm = "Embriodery Job"; QntyIn = "B"; break;
                case "JW": hddsp = "Issue Challan (Job Work)"; gooditem = "cut fabrics"; jobnm = "Jow Work"; QntyIn = "B"; break;
                case "ST": hddsp = "Issue to Challan (Sticher)"; gooditem = "cut fabrics"; jobnm = "Stiching Job"; QntyIn = "B"; break;
                case "OJ": hddsp = "Issue Challan (Other Job)"; gooditem = "Fabrics"; jobnm = "Other Job"; QntyIn = "B"; break;
                case "WA": hddsp = "Issue Challan (Washing)"; gooditem = "Fabrics"; jobnm = "Washing"; QntyIn = "B"; break;
                case "IR": hddsp = "Issue Challan (Ironing)"; gooditem = "Fabrics"; jobnm = "Ironing"; QntyIn = "B"; break;
                default: hddsp = ""; break;
            }
            if (tbltmp.Rows.Count > 0)
            {
                if (tbltmp.Rows[0]["doctag"].ToString() == "TO") hddsp = "Stock Transfer";
            }
            if (VE.TEXTBOX6 != null) repname = VE.TEXTBOX6;
            string sql_c = "c.batchautono, c.batchslno, ";
            if (jobcd == "DY" || jobcd == "BL" || jobcd == "YP") sql_c = "'' batchautono, 0 batchslno, ";
            string sql_g = "c.batchautono, c.batchslno, ";
            if (jobcd == "DY" || jobcd == "BL" || jobcd == "YP") sql_g = "'', 0, ";

            sql = "";
            sql += "select a.autono, b.slno, '', b.itcd, b.mtrljobcd, n.partnm, b.stktype, l.qntyin, b.itnm, o.sizenm, o.print_seq, b.uomcd, m.styleno, p.itgrptype, nvl(m.pcsperbox,0) pcsperbox, decode(nvl(m.mergepcs,0),0,1,m.mergepcs) mergepcs, ";
            sql += "h.uomnm, nvl(h.decimals,0) decimals, b.qnty, nvl(b.nos, 0) nos, nvl(b.stkqnty, 0) stkqnty, nvl(b.lpcs,0) lpcs, b.itrem, nvl(b.batchno, l.batchno) batchno, ";
            sql += "b.rate, b.hsncode, l.millnm, l.colrnm, nvl(l.gsm, 0) gsm, nvl(l.dia, 0) dia, nvl(l.ll, 0) ll, l.texture, nvl(l.gauge, 0) gauge, ";
            sql += "l.cutlength, l.mchnname, l.fabtype, l.orgbatchautono, l.orgbatchslno, q.docdt orgbatchdocdt, q.doconlyno orgbatchdocno, q.doccd orgbatchdoccd, ";
            sql += "l.autono recautono, l.docno recdocno, l.docdt recdocdt, l.doccd recdoccd from ";
            sql += "(select a.autono ";
            sql += "from " + scm1 + ".t_txn a, " + scm1 + ".t_cntrl_hdr b ";
            sql += "where a.autono = b.autono(+) and b.doccd = '" + VE.DOCCD + "' and ";
            sql += "b.docdt >= to_date('" + VE.FDT + "','dd/mm/yyyy') and b.docdt <= to_date('" + VE.FDT + "','dd/mm/yyyy') and ";
            sql += "b.doconlyno >= '" + VE.FDOCNO + "' and b.doconlyno <= '" + VE.TDOCNO + "' and ";
            sql += "b.compcd = '" + COM + "' and b.loccd = '" + LOC + "' ) a,  ";

            sql += "(select a.autono, a.mtrljobcd, 0 slno, " + sql_c + " a.itcd, a.stktype, a.qntyin, b.itnm, b.uomcd, ";
            sql += "sum(nvl(c.qnty, a.qnty)) qnty, sum(nvl(c.nos, a.nos)) nos, sum(nvl(c.stkqnty,a.stkqnty)) stkqnty, ";
            sql += "a.rate, b.hsnsaccd hsncode, '' itrem, a.batchno, a.sizecd, a.partcd, ";
            sql += "(select sum(qnty) qnty from " + scm1 + ".t_txndtl where a.stktype <> stktype and ";
            sql += "autono||itcd||sizecd = a.autono||a.itcd||a.sizecd) lpcs ";
            sql += "from " + scm1 + ".t_txndtl a, " + scm1 + ".m_sitem b, " + scm1 + ".t_batchdtl c, " + scm1 + ".t_cntrl_hdr d ";
            sql += "where a.itcd = b.itcd(+) and a.autono = c.autono(+) and a.slno = c.slno(+) and a.autono=d.autono and ";
            sql += "d.docdt >= to_date('" + VE.FDT + "','dd/mm/yyyy') and d.docdt <= to_date('" + VE.FDT + "','dd/mm/yyyy') and ";
            sql += "d.doconlyno >= '" + VE.FDOCNO + "' and d.doconlyno <= '" + VE.TDOCNO + "' and ";
            sql += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' ";
            sql += "group by a.autono, a.mtrljobcd, 0, " + sql_g + " a.itcd, a.stktype, a.qntyin, b.itnm, b.uomcd, ";
            sql += "a.rate, b.hsnsaccd, '', a.batchno, a.sizecd, a.partcd ";
            sql += "union all ";
            sql += "select a.autono, '' mtrljobcd, a.slno+1000 slno, '' batchautono, 0 batchslno, a.itcd, '' stktype, '' qntyin, b.itdescn itnm, b.uom uomcd, a.qnty, 0 nos,  ";
            sql += "0 stkqnty, a.rate, b.hsncd hsncode, a.itrem, a.batchno, '' sizecd, '' partcd, 0 lpcs ";
            sql += "from " + scm1 + ".t_txninvdtl a, " + scmI + ".m_item b ";
            sql += "where a.itcd = b.itcd(+) ) b, ";
            sql += "" + scm1 + ".t_txn c, " + scm1 + ".t_txntrans d, " + scm1 + ".t_cntrl_hdr e, ";
            sql += " " + scmf + ".m_uom h,  " + scmf + ".m_subleg i, " + scm1 + ".t_batchmst l, ";
            sql += scm1 + ".m_sitem m, " + scm1 + ".m_parts n, " + scm1 + ".m_size o, " + scm1 + ".m_group p, " + scm1 + ".t_cntrl_hdr q ";
            sql += "where a.autono = b.autono and a.autono = c.autono and a.autono = d.autono(+) and a.autono = e.autono(+) and ";
            sql += "b.uomcd = h.uomcd(+) and c.slcd = i.slcd(+) and b.batchautono = l.batchautono(+) and b.batchslno = l.batchslno(+) and ";
            sql += "b.itcd=m.itcd(+) and b.partcd=n.partcd(+) and b.sizecd=o.sizecd(+) and m.itgrpcd=p.itgrpcd(+) and l.orgbatchautono=q.autono(+) ";
            sql += "order by autono, slno, itcd, print_seq";
            DataTable rstbl = masterHelp.SQLquery(sql);

            sql = "";
            sql += "select a.autono, a.slno, a.progautono, a.progslno, j.batchno, j.proguniqno, a.slcd, a.itcd, a.stktype, a.partcd, ";
            sql += "a.sizecd, d.itnm, d.styleno, d.hsnsaccd hsncode, nvl(d.pcsperbox,0) pcsperbox, d.mixsize, h.uomnm, nvl(h.decimals,0) decimals, e.sizenm, e.print_seq, f.partnm, ";
            sql += "a.stkdrcr, nvl(a.qnty,0) qnty, nvl(a.shortqnty,0) shortqnty, nvl(j.gsm, 0) gsm, j.texture, nvl(j.gauge,0) gauge, nvl(j.ll,0) ll, nvl(j.dia, 0) dia, j.qntyin, ";
            sql += "j.millnm, j.colrnm, j.mchnname, j.fabtype, l.slnm||decode(k.linecd,null,'',' ['||k.linecd||']') recslnm, k.linecd reclinecd, ";
            sql += "nvl(a.nos, 0) nos, nvl(a.stkqnty, 0) stkqnty from ";
            sql += "( select a.autono, a.slno, a.progautono, a.progslno, b.slcd, b.itcd, b.stktype, b.partcd, b.sizecd, a.stkdrcr, a.qnty, c.shortqnty, ";
            sql += "nvl(b.nos, 0) nos,nvl(b.stkqnty, 0) stkqnty ";
            sql += "from " + scm1 + ".t_progdtl a, " + scm1 + ".t_progmast b, " + scm1 + ".t_txndtl c, " + scm1 + ".t_cntrl_hdr d ";
            sql += "where a.progautono=b.autono(+) and a.progslno=b.slno(+) and a.autono=c.autono(+) and a.slno=c.slno(+) and ";
            sql += "a.autono=d.autono(+) and d.doccd = '" + VE.DOCCD + "' and ";
            sql += "d.docdt >= to_date('" + VE.FDT + "','dd/mm/yyyy') and d.docdt <= to_date('" + VE.FDT + "','dd/mm/yyyy') and ";
            sql += "d.doconlyno >= '" + VE.FDOCNO + "' and d.doconlyno <= '" + VE.TDOCNO + "' and ";
            sql += "d.compcd = '" + COM + "' and d.loccd = '" + LOC + "' ) a,  ";

            sql += scm1 + ".t_cntrl_hdr b, " + scm1 + ".t_txntrans c, " + scm1 + ".m_sitem d, " + scm1 + ".m_size e, " + scm1 + ".m_parts f, ";
            sql += scmf + ".m_subleg g, " + scmf + ".m_uom h, " + scm1 + ".t_progmast j, " + scm1 + ".t_txn k, " + scmf + ".m_subleg l ";
            sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and a.itcd=d.itcd(+) and a.sizecd=e.sizecd(+) and a.partcd=f.partcd(+) and ";
            sql += "a.slcd=g.slcd(+) and d.uomcd=h.uomcd(+) and a.progautono=j.autono(+) and a.progslno=j.slno(+) and ";
            sql += "j.recautono=k.autono(+) and k.slcd=l.slcd(+) ";
            sql += "order by autono, slno, print_seq";
            DataTable rsprog = masterHelp.SQLquery(sql);

            string sqlc = "";
            sqlc += "where a.autono = b.autono(+) and b.doccd = '" + VE.DOCCD + "' and ";
            sqlc += "b.docdt >= to_date('" + VE.FDT + "','dd/mm/yyyy') and b.docdt <= to_date('" + VE.FDT + "','dd/mm/yyyy') and ";
            sqlc += "b.doconlyno >= '" + VE.FDOCNO + "' and b.doconlyno <= '" + VE.TDOCNO + "' and ";
            sqlc += "b.compcd = '" + COM + "' and b.loccd = '" + LOC + "' ";

            sql = "";
            sql += "select a.autono, c.slcd, c.linecd, g.linenm, f.cancel, f.docno, f.docdt, f.usr_id, e.recvperson, e.lorryno, g.linenm, ";
            sql += "d.slnm, d.add1, d.add2, d.add3, d.add4, d.add5, d.add6, d.add7, d.statecd, d.panno, d.gstno, d.regmobile, ";
            sql += "c.mcpautono, j.docno mcpno, j.docdt mcpdt, j.refno mcprefno, c.cutrecdocautono, i.docno cutdocno, i.docdt cutdocdt, ";
            sql += "nvl(b.docrem,'') docrem from ";
            sql += "( ";
            sql += "select distinct a.autono from " + scm1 + ".t_txndtl a, " + scm1 + ".t_cntrl_hdr b ";
            sql += sqlc;
            sql += "union ";
            sql += "select distinct a.autono from " + scm1 + ".t_progdtl a, " + scm1 + ".t_cntrl_hdr b ";
            sql += sqlc;
            sql += ") a, ";
            sql += "( select a.autono, listagg(a.docrem, '') within group (order by a.slno) docrem ";
            sql += "from " + scm1 + ".t_cntrl_hdr_rem a group by a.autono ) b, " + scm1 + ".t_txn c, " + scmf + ".m_subleg d, " + scm1 + ".t_txntrans e, ";
            sql += scm1 + ".t_cntrl_hdr f, " + scm1 + ".m_linemast g, " + scm1 + ".t_cntrl_hdr h, " + scm1 + ".t_cntrl_hdr i, " + scm1 + ".t_mcp j ";
            sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and c.slcd=d.slcd(+) and a.autono=e.autono(+) and a.autono=f.autono(+) and c.linecd=g.linecd(+) and ";
            sql += "c.mcpautono=h.autono(+) and c.cutrecdocautono=i.autono(+) and c.mcpautono=j.autono(+) ";
            sql += "order by docdt, docno ";
            DataTable tblauto = masterHelp.SQLquery(sql);

            DataTable IR = new DataTable("");
            IR.Columns.Add("autono", typeof(string), "");
            IR.Columns.Add("slcd", typeof(string), "");
            IR.Columns.Add("slnm", typeof(string), "");
            IR.Columns.Add("sladd1", typeof(string), "");
            IR.Columns.Add("sladd2", typeof(string), "");
            IR.Columns.Add("sladd3", typeof(string), "");
            IR.Columns.Add("sladd4", typeof(string), "");
            IR.Columns.Add("sladd5", typeof(string), "");
            IR.Columns.Add("sladd6", typeof(string), "");
            IR.Columns.Add("sladd7", typeof(string), "");
            IR.Columns.Add("sladd8", typeof(string), "");
            IR.Columns.Add("docno", typeof(string), "");
            IR.Columns.Add("docdt", typeof(string), "");
            IR.Columns.Add("vechlno", typeof(string), "");
            IR.Columns.Add("recvperson", typeof(string), "");
            //programme
            IR.Columns.Add("slno", typeof(string), "");
            IR.Columns.Add("itdescn", typeof(string), "");
            IR.Columns.Add("partnm", typeof(string), "");
            IR.Columns.Add("styleno", typeof(string), "");
            IR.Columns.Add("colrnm", typeof(string), "");
            IR.Columns.Add("sizenm", typeof(string), "");
            IR.Columns.Add("uomnm", typeof(string), "");
            IR.Columns.Add("nos", typeof(double), "");
            IR.Columns.Add("cutlength", typeof(double), "");
            IR.Columns.Add("qnty", typeof(double), "");
            IR.Columns.Add("uniqno", typeof(string), "");
            IR.Columns.Add("itremark", typeof(string), "");
            IR.Columns.Add("totalqnty", typeof(double), "");
            //issue
            IR.Columns.Add("progslno", typeof(string), "");
            IR.Columns.Add("iss_slno", typeof(string), "");
            IR.Columns.Add("iss_itdescn", typeof(string), "");
            IR.Columns.Add("iss_styleno", typeof(string), "");
            IR.Columns.Add("iss_colrnm", typeof(string), "");
            IR.Columns.Add("iss_sizenm", typeof(string), "");
            IR.Columns.Add("iss_uomnm", typeof(string), "");
            IR.Columns.Add("iss_nos", typeof(double), "");
            IR.Columns.Add("iss_cutlength", typeof(double), "");
            IR.Columns.Add("iss_qnty", typeof(double), "");
            IR.Columns.Add("iss_totalqnty", typeof(double), "");

            IR.Columns.Add("user_nm", typeof(string), "");

            bool hdrprint = true;
            ReportDocument reportdocument = new ReportDocument();
            Int32 maxR = 0, i = 0;
            Int32 x = 0, maxX = tblauto.Rows.Count - 1;
            Int32 rNo = 0, sln = 0;
            double maxDeci = 0, currdeci = 0;
            while (x <= maxX)
            {
                //address
                string add = ""; string[] address;
                string cfld = "", rfld = ""; int rf = 0;
                for (int f = 1; f <= 7; f++)
                {
                    cfld = "add" + Convert.ToString(f).ToString();
                    if (tblauto.Rows[x][cfld].ToString() != "")
                    {
                        rf = rf + 1;
                        if (add == "")
                        {
                            add = add + tblauto.Rows[x][cfld].ToString();
                        }
                        else
                        {
                            add = add + Cn.GCS() + tblauto.Rows[x][cfld].ToString();
                        }
                    }
                }
                if (tblauto.Rows[x]["gstno"].ToString() != "")
                {
                    rf = rf + 1;
                    add = add + Cn.GCS() + "GST # " + tblauto.Rows[x]["gstno"].ToString();
                }
                if (tblauto.Rows[x]["panno"].ToString() != "")
                {
                    rf = rf + 1;
                    add = add + Cn.GCS() + "PAN # " + tblauto.Rows[x]["panno"].ToString();
                }
                address = add.Split(Convert.ToChar(Cn.GCS()));

                double t_qnty = 0, t_nos = 0, t_stkqnty = 0, t_lqnty = 0, nslno = 0;
                string autono = tblauto.Rows[x]["autono"].ToString();
                string trem = tblauto.Rows[x]["docrem"].ToString(), usr_id = tblauto.Rows[x]["usr_id"].ToString();
                string recrefno = "", mcprefno = "", orgrefno = "";
                mcprefno = tblauto.Rows[x]["mcprefno"].ToString();
                if (mcprefno != "") mcprefno += " dtd." + tblauto.Rows[x]["mcpdt"].ToString().retDateStr();
                #region Issue Material Printing
                string sel1 = "autono='" + autono + "'";

                DataTable tbl = new DataTable();

                string actof = "";
                tbl = new DataTable();
                var rowsx = rsprog.AsEnumerable()
                    .Where(t => ((string)t["autono"]) == autono);
                if (rowsx.Any()) tbl = rowsx.CopyToDataTable();
                if (tbl.Rows.Count > 0) actof = tbl.Rows[0]["recslnm"].ToString();

                var rows1 = rstbl.AsEnumerable()
                    .Where(q => ((string)q["autono"]) == autono);
                if (rows1.Any()) tbl = rows1.CopyToDataTable();

                var recvchdata = (from DataRow dr in tbl.Rows
                                  select new
                                  {
                                      recdocno = dr["recdocno"],
                                      recdoccd = dr["recdoccd"],
                                      recdocdt = dr["recdocdt"]
                                  }).Distinct().ToList();

                recrefno = string.Join(",", (from a in recvchdata select a.recdoccd.ToString() + "-" + a.recdocno.ToString() + " dt " + a.recdocdt.ToString().retDateStr()).Distinct());

                var orgbatchdata = (from DataRow dr in tbl.Rows
                                    select new
                                    {
                                        recdocno = dr["orgbatchdocno"],
                                        recdoccd = dr["orgbatchdoccd"],
                                        recdocdt = dr["orgbatchdocdt"]
                                    }).Distinct().ToList();

                orgrefno = string.Join(",", (from a in orgbatchdata select a.recdoccd.ToString() + "-" + a.recdocno.ToString() + " dt " + a.recdocdt.ToString().retDateStr()).Distinct());
                bool datahave = false;
                maxR = tbl.Rows.Count - 1; i = 0; sln = 0; hdrprint = true;
                while (i <= maxR)
                {
                    datahave = true;
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                prnloop:

                    IR.Rows[rNo]["recotype"] = "2";
                    IR.Rows[rNo]["autono"] = tbl.Rows[i]["autono"];
                    IR.Rows[rNo]["slcd"] = tblauto.Rows[x]["slcd"];
                    IR.Rows[rNo]["slnm"] = tblauto.Rows[x]["slnm"];
                    IR.Rows[rNo]["mcprefno"] = mcprefno;
                    IR.Rows[rNo]["recrefno"] = recrefno;
                    IR.Rows[rNo]["orgrefno"] = orgrefno;
                    IR.Rows[rNo]["cutrecvhno"] = tblauto.Rows[x]["cutdocno"].ToString() + " dtd. " + tblauto.Rows[x]["cutdocdt"].ToString().retDateStr();
                    if (tblauto.Rows[x]["linenm"].ToString().retStr() != "") IR.Rows[rNo]["linenm"] = "Unit - " + tblauto.Rows[x]["linenm"].ToString();
                    if (tblauto.Rows[x]["linenm"].ToString().retStr() != "") chlntype = "INTER FACTORY CHALLAN"; else chlntype = "ROAD CHALLAN";

                    int coutadd = 0;
                    for (int g = 0; g <= address.Count() - 1; g++)
                    {
                        coutadd++;
                        rfld = "sladd" + Convert.ToString(coutadd);
                        IR.Rows[rNo][rfld] = address[g].ToString();
                    }

                    IR.Rows[rNo]["user_nm"] = usr_id;
                    IR.Rows[rNo]["gooditem"] = gooditem;
                    IR.Rows[rNo]["jobnm"] = jobnm;
                    IR.Rows[rNo]["docno"] = tblauto.Rows[x]["docno"];
                    IR.Rows[rNo]["docdt"] = tblauto.Rows[x]["docdt"].ToString().retDateStr();
                    IR.Rows[rNo]["vechlno"] = tblauto.Rows[x]["lorryno"];
                    IR.Rows[rNo]["recvperson"] = tblauto.Rows[x]["recvperson"];
                    IR.Rows[rNo]["trem"] = trem;
                    IR.Rows[rNo]["user_nm"] = usr_id;
                    IR.Rows[rNo]["actof"] = actof == "" ? "" : "A/c of " + actof;

                    if (hdrprint == false)
                    {
                        t_qnty = t_qnty + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                        t_nos = t_nos + Convert.ToDouble(tbl.Rows[i]["nos"]);

                        sln++;
                        IR.Rows[rNo]["iisuomnm"] = tbl.Rows[i]["uomnm"].ToString();
                        IR.Rows[rNo]["slno"] = sln;
                        IR.Rows[rNo]["itcd"] = tbl.Rows[i]["itcd"];
                        IR.Rows[rNo]["mtrljobcd"] = tbl.Rows[i]["mtrljobcd"];
                        string dsc = tbl.Rows[i]["itnm"].ToString();
                        if (tbl.Rows[i]["mchnname"].ToString() != "") dsc += " " + tbl.Rows[i]["mchnname"].ToString();
                        if (tbl.Rows[i]["itgrptype"].ToString() == "F")
                        {
                            dsc = tbl.Rows[i]["styleno"].ToString() + " - " + tbl.Rows[i]["sizenm"].ToString();
                            if (tbl.Rows[i]["partnm"].ToString() != "") dsc += " [" + tbl.Rows[i]["partnm"].ToString() + "] ";
                        }
                        IR.Rows[rNo]["orgbatchdocno"] = tbl.Rows[i]["orgbatchdocno"];
                        IR.Rows[rNo]["orgbatchdocdt"] = tbl.Rows[i]["orgbatchdocdt"].ToString().retDateStr("yy");
                        IR.Rows[rNo]["itnm"] = dsc;
                        IR.Rows[rNo]["styleno"] = tbl.Rows[i]["styleno"];
                        IR.Rows[rNo]["pcsperbox"] = Convert.ToDouble(tbl.Rows[i]["pcsperbox"]);
                        IR.Rows[rNo]["partnm"] = tbl.Rows[i]["partnm"];
                        IR.Rows[rNo]["sizenm"] = tbl.Rows[i]["sizenm"];
                        //IR.Rows[rNo]["qntyin"] = Salesfunc.retQtyInDesc(tbl.Rows[i]["qntyin"].ToString().retStr());
                        IR.Rows[rNo]["hsncode"] = tbl.Rows[i]["hsncode"];
                        IR.Rows[rNo]["millnm"] = tbl.Rows[i]["millnm"];
                        IR.Rows[rNo]["nos"] = Convert.ToDouble(tbl.Rows[i]["nos"]);
                        double pp = 12;
                        if (QntyIn == "B") pp = Convert.ToDouble(tbl.Rows[i]["pcsperbox"]);
                        if (jobcd != "CT")
                        {
                            IR.Rows[rNo]["stkqnty"] = QntyIn != "" ? Salesfunc.ConvPcstoBox(Convert.ToDouble(tbl.Rows[i]["qnty"]), pp) : Convert.ToDouble(tbl.Rows[i]["stkqnty"]);
                            if (Convert.ToDouble(IR.Rows[rNo]["stkqnty"] == null ? 0 : Convert.ToDouble(IR.Rows[rNo]["stkqnty"])) == 0) IR.Rows[rNo]["qntyin"] = "";
                        }
                        IR.Rows[rNo]["qnty"] = Convert.ToDouble(tbl.Rows[i]["qnty"].ToString());
                        if (Convert.ToDouble(tbl.Rows[i]["lpcs"].ToString()) == 0 && tbl.Rows[i]["stktype"].ToString() == "L")
                        {
                            nslno++;
                            IR.Rows[rNo]["lpcs"] = Convert.ToDouble(tbl.Rows[i]["qnty"].ToString());
                            IR.Rows[rNo]["qnty"] = Convert.ToDouble(tbl.Rows[i]["qnty"].ToString());
                            t_lqnty = t_lqnty + Convert.ToDouble(tbl.Rows[i]["qnty"].ToString());
                            IR.Rows[rNo]["recoskip"] = "N";
                            IR.Rows[rNo]["nslno"] = nslno;
                            IR.Rows[rNo]["stkqnty"] = 0; IR.Rows[rNo]["qntyin"] = "";
                        }
                        else
                        {
                            if (tbl.Rows[i]["stktype"].ToString() == "F" || Convert.ToDouble(tbl.Rows[i]["slno"]) > 1000)
                            {
                                nslno++;
                                if (jobcd == "ST" && Convert.ToDouble(tbl.Rows[i]["mergepcs"].ToString()) > 1)
                                {
                                    IR.Rows[rNo]["nqnty"] = Convert.ToDouble(tbl.Rows[i]["qnty"].ToString()) / Convert.ToDouble(tbl.Rows[i]["mergepcs"].ToString());
                                    IR.Rows[rNo]["lpcs"] = Convert.ToDouble(tbl.Rows[i]["lpcs"].ToString()) / Convert.ToDouble(tbl.Rows[i]["mergepcs"].ToString());
                                    IR.Rows[rNo]["stkqnty"] = QntyIn != "" ? Salesfunc.ConvPcstoBox(Convert.ToDouble(tbl.Rows[i]["qnty"]) / Convert.ToDouble(tbl.Rows[i]["mergepcs"].ToString()), pp) : Convert.ToDouble(tbl.Rows[i]["stkqnty"]);
                                }
                                else
                                {
                                    IR.Rows[rNo]["nqnty"] = Convert.ToDouble(tbl.Rows[i]["qnty"].ToString());
                                    IR.Rows[rNo]["lpcs"] = Convert.ToDouble(tbl.Rows[i]["lpcs"].ToString());
                                }
                                t_lqnty = t_lqnty + Convert.ToDouble(tbl.Rows[i]["lpcs"].ToString());
                                IR.Rows[rNo]["recoskip"] = "N";
                                IR.Rows[rNo]["nslno"] = nslno;
                            }
                        }
                        IR.Rows[rNo]["tlqnty"] = t_lqnty.ToString();
                        IR.Rows[rNo]["tqnty"] = t_qnty.ToString();
                        IR.Rows[rNo]["tnos"] = t_nos;
                        currdeci = Convert.ToDouble(tbl.Rows[i]["decimals"].ToString());
                        if (maxDeci < currdeci) maxDeci = currdeci;
                        IR.Rows[rNo]["uomnm"] = tbl.Rows[i]["uomnm"].ToString();
                        IR.Rows[rNo]["decimals"] = currdeci;
                        IR.Rows[rNo]["maxdecimals"] = maxDeci;
                        IR.Rows[rNo]["gsm"] = Convert.ToDouble(tbl.Rows[i]["gsm"]) == 0 ? "" : tbl.Rows[i]["gsm"];
                        IR.Rows[rNo]["texture"] = tbl.Rows[i]["texture"];
                        IR.Rows[rNo]["gauge"] = Convert.ToDouble(tbl.Rows[i]["gauge"]) == 0 ? "" : tbl.Rows[i]["gauge"];
                        IR.Rows[rNo]["ll"] = Convert.ToDouble(tbl.Rows[i]["ll"]) == 0 ? "" : tbl.Rows[i]["ll"];
                        IR.Rows[rNo]["dia"] = Convert.ToDouble(tbl.Rows[i]["dia"]) == 0 ? "" : tbl.Rows[i]["dia"];
                        IR.Rows[rNo]["batchno"] = tbl.Rows[i]["batchno"];
                        IR.Rows[rNo]["colrnm"] = tbl.Rows[i]["colrnm"];
                        IR.Rows[rNo]["mchnname"] = tbl.Rows[i]["mchnname"];
                    }

                    if (hdrprint == true)
                    {
                        IR.Rows[rNo]["recotype"] = "1";
                        hdrprint = false;
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        goto prnloop;
                    }

                    i++;
                    if (i > maxR) break;
                }
                if (tbl.Rows.Count > 0)
                {
                    if (jobcd == "ST" && Convert.ToDouble(tbl.Rows[0]["mergepcs"].ToString()) > 1)
                    {
                        trem = (t_qnty + t_lqnty).ToString() + " Pcs converted to " + (t_qnty + t_lqnty) / Convert.ToDouble(tbl.Rows[0]["mergepcs"].ToString()) + " Pcs";
                    }
                }

                if (datahave == true)
                {
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["recotype"] = "3";
                    IR.Rows[rNo]["autono"] = autono;
                    IR.Rows[rNo]["tlqnty"] = t_lqnty.ToString();
                    IR.Rows[rNo]["tqnty"] = t_qnty.ToString();
                    IR.Rows[rNo]["tnos"] = t_nos;
                    IR.Rows[rNo]["maxdecimals"] = maxDeci;
                    IR.Rows[rNo]["trem"] = trem;
                    IR.Rows[rNo]["user_nm"] = usr_id;

                    IR.Rows[rNo]["slnm"] = tblauto.Rows[x]["slnm"];
                    IR.Rows[rNo]["docno"] = tblauto.Rows[x]["docno"];
                    IR.Rows[rNo]["docdt"] = tblauto.Rows[x]["docdt"].ToString().retDateStr();
                }
                #endregion

                if (repname != "JobIssueOld")
                {
                    #region Programme Printing
                    datahave = false;
                    string sel2 = "autono='" + autono + "'";

                    tbl = new DataTable();
                    var rows2 = rsprog.AsEnumerable()
                        .Where(t => ((string)t["autono"]) == autono);
                    if (rows2.Any()) tbl = rows2.CopyToDataTable();

                    //tbl = (from DataRow DR in rsprog.Rows where DR["autono"].ToString() == autono select DR).CopyToDataTable();

                    maxR = tbl.Rows.Count - 1; i = 0; sln = 0;
                    t_nos = 0; t_qnty = 0; hdrprint = true;
                    maxDeci = 0; currdeci = 0;
                    while (i <= maxR)
                    {
                        datahave = true;
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    progloop:
                        IR.Rows[rNo]["actof"] = tbl.Rows[i]["recslnm"].ToString();
                        IR.Rows[rNo]["recotype"] = "12";
                        IR.Rows[rNo]["autono"] = tbl.Rows[i]["autono"];
                        IR.Rows[rNo]["slcd"] = tblauto.Rows[x]["slcd"];
                        IR.Rows[rNo]["slnm"] = tblauto.Rows[x]["slnm"];
                        IR.Rows[rNo]["mcprefno"] = mcprefno;
                        IR.Rows[rNo]["recrefno"] = recrefno;
                        IR.Rows[rNo]["orgrefno"] = orgrefno;
                        IR.Rows[rNo]["cutrecvhno"] = tblauto.Rows[x]["cutdocno"].ToString() + " dtd. " + tblauto.Rows[x]["cutdocdt"].ToString().retDateStr();
                        if (tblauto.Rows[x]["linenm"].ToString().retStr() != "") IR.Rows[rNo]["linenm"] = "Unit - " + tblauto.Rows[x]["linenm"].ToString();
                        if (tblauto.Rows[x]["linenm"].ToString().retStr() != "") chlntype = "INTER FACTORY CHALLAN"; else chlntype = "ROAD CHALLAN";

                        int coutadd = 0;
                        for (int g = 0; g <= address.Count() - 1; g++)
                        {
                            coutadd++;
                            rfld = "sladd" + Convert.ToString(coutadd);
                            IR.Rows[rNo][rfld] = address[g].ToString();
                        }

                        IR.Rows[rNo]["gooditem"] = gooditem;
                        IR.Rows[rNo]["jobnm"] = jobnm;
                        IR.Rows[rNo]["docno"] = tblauto.Rows[x]["docno"];
                        IR.Rows[rNo]["docdt"] = tblauto.Rows[x]["docdt"].ToString().retDateStr();
                        IR.Rows[rNo]["vechlno"] = tblauto.Rows[x]["lorryno"];
                        IR.Rows[rNo]["recvperson"] = tblauto.Rows[x]["recvperson"];
                        IR.Rows[rNo]["trem"] = trem;
                        IR.Rows[rNo]["user_nm"] = usr_id;
                        IR.Rows[rNo]["recoskip"] = "";
                        if (hdrprint == false)
                        {
                            sln++;
                            IR.Rows[rNo]["slno"] = sln;
                            IR.Rows[rNo]["itcd"] = tbl.Rows[i]["itcd"];
                            string dsc = tbl.Rows[i]["itnm"].ToString();
                            if (tbl.Rows[i]["mchnname"].ToString() != "") dsc += " " + tbl.Rows[i]["mchnname"].ToString();
                            IR.Rows[rNo]["itnm"] = dsc;
                            IR.Rows[rNo]["styleno"] = tbl.Rows[i]["styleno"];
                            IR.Rows[rNo]["pcsperbox"] = Convert.ToDouble(tbl.Rows[i]["pcsperbox"]);
                            IR.Rows[rNo]["partnm"] = tbl.Rows[i]["partnm"];
                            IR.Rows[rNo]["sizenm"] = tbl.Rows[i]["sizenm"];
                            IR.Rows[rNo]["uomnm"] = tbl.Rows[i]["uomnm"].ToString();
                            IR.Rows[rNo]["hsncode"] = tbl.Rows[i]["hsncode"];
                            IR.Rows[rNo]["millnm"] = tbl.Rows[i]["millnm"];
                            currdeci = Convert.ToDouble(tbl.Rows[i]["decimals"].ToString());
                            IR.Rows[rNo]["nos"] = Convert.ToDouble(tbl.Rows[i]["nos"]);
                            double qnty = Convert.ToDouble(tbl.Rows[i]["qnty"]), s_qnty = Convert.ToDouble(tbl.Rows[i]["stkqnty"]);
                            if (progprint == true) IR.Rows[i]["progprint"] = "Y";
                            IR.Rows[rNo]["qnty"] = qnty;
                            s_qnty = 0;
                            switch (QntyIn)
                            {
                                case "D":
                                    IR.Rows[rNo]["qntyin"] = "Dzn";
                                    s_qnty = Salesfunc.ConvPcstoBox(qnty, 12);
                                    IR.Rows[rNo]["stkqnty"] = s_qnty;
                                    currdeci = 2;
                                    break;
                                case "B":
                                    IR.Rows[rNo]["qntyin"] = "Box";
                                    s_qnty = Salesfunc.ConvPcstoBox(qnty, Convert.ToDouble(tbl.Rows[i]["pcsperbox"]));
                                    IR.Rows[rNo]["stkqnty"] = s_qnty;
                                    break;
                                default:
                                    IR.Rows[rNo]["qntyin"] = tbl.Rows[i]["qntyin"];
                                    IR.Rows[rNo]["stkqnty"] = Convert.ToDouble(tbl.Rows[i]["stkqnty"]);
                                    break;
                            }
                            if (jobcd != "CT") if (tbl.Rows[i]["stktype"].ToString() != "L") t_stkqnty = t_stkqnty + s_qnty;
                            IR.Rows[rNo]["decimals"] = currdeci;
                            if (maxDeci < currdeci) maxDeci = currdeci;
                            IR.Rows[rNo]["maxdecimals"] = maxDeci;
                            IR.Rows[rNo]["gsm"] = Convert.ToDouble(tbl.Rows[i]["gsm"]) == 0 ? "" : tbl.Rows[i]["gsm"];
                            IR.Rows[rNo]["texture"] = tbl.Rows[i]["texture"];
                            IR.Rows[rNo]["gauge"] = Convert.ToDouble(tbl.Rows[i]["gauge"]) == 0 ? "" : tbl.Rows[i]["gauge"];
                            IR.Rows[rNo]["ll"] = Convert.ToDouble(tbl.Rows[i]["ll"]) == 0 ? "" : tbl.Rows[i]["ll"];
                            IR.Rows[rNo]["dia"] = Convert.ToDouble(tbl.Rows[i]["dia"]) == 0 ? "" : tbl.Rows[i]["dia"];
                            IR.Rows[rNo]["batchno"] = tbl.Rows[i]["batchno"];
                            IR.Rows[rNo]["colrnm"] = tbl.Rows[i]["colrnm"];
                            IR.Rows[rNo]["proguniqno"] = tbl.Rows[i]["proguniqno"];
                            IR.Rows[rNo]["mchnname"] = tbl.Rows[i]["mchnname"];
                            if (tbl.Rows[i]["proguniqno"].ToString() != "")
                            {
                                IR.Rows[rNo]["proguniqnojpg"] = "c:\\improvar\\" + tbl.Rows[i]["proguniqno"] + ".jpg";
                                barcode = tbl.Rows[i]["proguniqno"].ToString();
                                //CnBarCode.BarcodeGeneratre(tbl.Rows[i]["proguniqno"].ToString());
                                //CnBarCode.genBarCode(tbl.Rows[i]["proguniqno"].ToString(), true);
                            }
                        }

                        if (hdrprint == true)
                        {
                            IR.Rows[rNo]["recotype"] = "11";
                            hdrprint = false;
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            goto progloop;
                        }

                        t_qnty = t_qnty + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                        t_nos = t_nos + Convert.ToDouble(tbl.Rows[i]["nos"]);

                        i++;
                        if (i > maxR) break;
                    }

                    if (datahave == true)
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["recotype"] = "13";
                        IR.Rows[rNo]["autono"] = autono;
                        IR.Rows[rNo]["tqnty"] = t_qnty;
                        IR.Rows[rNo]["tstkqnty"] = t_stkqnty;
                        IR.Rows[rNo]["tnos"] = t_nos;
                        IR.Rows[rNo]["trem"] = trem;
                        IR.Rows[rNo]["maxdecimals"] = maxDeci;
                        IR.Rows[rNo]["tlqnty"] = t_lqnty;
                        IR.Rows[rNo]["user_nm"] = usr_id;
                    }
                    #endregion
                    //eof 
                }
                x++;
            }

            string[] compaddress;
            compaddress = Salesfunc.retCompAddress(gocd).Split(Convert.ToChar(Cn.GCS()));
            string rptname = "~/Report/" + repname + ".rpt";

            reportdocument.Load(Server.MapPath(rptname));
            reportdocument.SetDataSource(IR);
            reportdocument.SetParameterValue("complogo", Salesfunc.retCompLogo());
            reportdocument.SetParameterValue("compnm", compaddress[0]);
            reportdocument.SetParameterValue("compadd", compaddress[1]);
            reportdocument.SetParameterValue("compstat", compaddress[2]);
            reportdocument.SetParameterValue("locaadd", compaddress[3]);
            reportdocument.SetParameterValue("locastat", compaddress[4]);
            reportdocument.SetParameterValue("billheading", hddsp);
            reportdocument.SetParameterValue("chlntype", chlntype);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = reportdocument.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);

            for (Int32 f = 0; f <= rsprog.Rows.Count - 1; f++)
            {
                barcode = rsprog.Rows[f]["proguniqno"].ToString();

                string path1 = @"C:\Improvar\" + barcode + ".jpg";
                if (System.IO.File.Exists(path1))
                {
                    System.IO.File.Delete(path1);
                }
            }

            stream.Seek(0, SeekOrigin.Begin);
            reportdocument.Close(); reportdocument.Dispose(); GC.Collect();
            return new FileStreamResult(stream, "application/pdf");
        }
        [HttpPost]
        public ActionResult Rep_RecChallan_Print(Rep_Doc_Print VE)
        {
            string scmI = CommVar.InvSchema(UNQSNO);
            string hddsp = "", chlntype = "";
            string gooditem = "", jobnm = "", barcode = "";
            string repname = "JobIssue"; // "JobReceive";
            string QntyIn = "", jobcd = "", gocd = "";
            string[] otherpara = VE.OtherPara.Split(',');
            jobcd = otherpara[0];
            if (otherpara.Count() > 1) gocd = otherpara[1];
            bool progprint = false;

            switch (jobcd)
            {
                case "KT": hddsp = "Receive Challan (Knitter)"; gooditem = "Yarn"; jobnm = "Knitting Job"; repname = "YarnIssue"; progprint = true; break;
                case "YD": hddsp = "Receive Challan (Yarn Dyer)"; gooditem = "Yarn"; jobnm = "Dying Job"; repname = "YarnIssue"; progprint = true; break;
                case "FP": hddsp = "Receive to Processor"; gooditem = "Fabrics"; jobnm = "Processing Job"; repname = "FabIssue"; progprint = true; break;
                case "DY": hddsp = "Receive Challan (Dyer)"; gooditem = "Fabrics"; jobnm = "Dying Job"; repname = "FabIssue"; progprint = true; break;
                case "BL": hddsp = "Receive Challan (Bleacher)"; gooditem = "Fabrics"; jobnm = "Bleaching Job"; repname = "FabIssue"; progprint = true; break;
                case "CT": hddsp = "Receive Challan (Cutter)"; gooditem = "Fabrics"; jobnm = "Cutting Job"; repname = "CutRec"; QntyIn = "D"; progprint = true; break;
                case "PR": hddsp = "Receive Challan Printer"; gooditem = "Cut fabrics"; jobnm = "Printing Job"; QntyIn = "B"; break;
                case "EM": hddsp = "Receive Challan (Embroider)"; gooditem = "cut fabrics"; jobnm = "Embriodery Job"; QntyIn = "B"; break;
                case "JW": hddsp = "Receive Challan (Job Work)"; gooditem = "cut fabrics"; jobnm = "Jow Work"; QntyIn = "B"; break;
                case "ST": hddsp = "Receive to Challan (Sticher)"; gooditem = "cut fabrics"; jobnm = "Stiching Job"; QntyIn = "B"; break;
                case "OJ": hddsp = "Receive Challan (Other Job)"; gooditem = "Fabrics"; jobnm = "Other Job"; QntyIn = "B"; break;
                case "WA": hddsp = "Receive Challan (Washing)"; gooditem = "Fabrics"; jobnm = "Washing"; QntyIn = "B"; break;
                case "IR": hddsp = "Receive Challan (Ironing)"; gooditem = "Fabrics"; jobnm = "Ironing"; QntyIn = "B"; break;
                case "FB": hddsp = "Finish Purchase"; gooditem = "Finish Goods"; jobnm = "Finish Purchase"; QntyIn = "B"; break;
                default: hddsp = ""; break;
            }
            if (VE.TEXTBOX6 != null) repname = VE.TEXTBOX6;
            sql = "";
            sql += "select a.autono, b.slno, b.batchautono, b.itcd, b.partcd, n.partnm, b.stktype, l.qntyin, b.itnm, o.sizenm, o.print_seq, b.uomcd, m.styleno, p.itgrptype, nvl(m.pcsperbox,0) pcsperbox, ";
            sql += "h.uomnm, nvl(h.decimals,0) decimals,decode(b.stktype,'L',0, nvl(b.wght,0)) wght, b.qnty, nvl(b.nos, 0) nos, nvl(b.stkqnty, 0) stkqnty, b.itrem, nvl(b.batchno, l.batchno) batchno, ";
            sql += "b.rate, b.hsncode, l.millnm, l.colrnm, nvl(l.gsm, 0) gsm, nvl(l.dia, 0) dia, nvl(l.ll, 0) ll, l.texture, nvl(l.gauge, 0) gauge, ";
            sql += "l.cutlength, l.mchnname, l.fabtype, l.orgbatchautono, l.orgbatchslno, q.docdt orgbatchdocdt, q.doconlyno orgbatchdocno, q.doccd orgbatchdoccd, ";
            sql += "l.autono recautono, l.docno recdocno, l.docdt recdocdt, l.doccd recdoccd, nvl(c.cloth_used,0) cloth_used, nvl(c.cloth_was,0) cloth_was from ";
            sql += "(select a.autono ";
            sql += "from " + scm1 + ".t_txn a, " + scm1 + ".t_cntrl_hdr b ";
            sql += "where a.autono = b.autono(+) and b.doccd = '" + VE.DOCCD + "' and ";
            sql += "b.docdt >= to_date('" + VE.FDT + "','dd/mm/yyyy') and b.docdt <= to_date('" + VE.FDT + "','dd/mm/yyyy') and ";
            sql += "b.doconlyno >= '" + VE.FDOCNO + "' and b.doconlyno <= '" + VE.TDOCNO + "' and ";
            sql += "b.compcd = '" + COM + "' and b.loccd = '" + LOC + "' ) a,  ";

            sql += "(select a.autono, 0 slno, c.batchautono, c.batchslno, a.itcd, a.stktype, a.qntyin, b.itnm, b.uomcd, sum(nvl(a.wght,0)) wght, ";
            sql += "sum(nvl(c.qnty, a.qnty)) qnty, sum(nvl(c.nos, a.nos)) nos,  ";
            sql += "a.stkqnty, a.rate, b.hsnsaccd hsncode, '' itrem, a.batchno, a.sizecd, a.partcd ";
            sql += "from " + scm1 + ".t_txndtl a, " + scm1 + ".m_sitem b, " + scm1 + ".t_batchdtl c ";
            sql += "where a.itcd = b.itcd(+) and a.autono = c.autono(+) and a.slno = c.slno(+) and a.slno <= 5000 ";
            sql += "group by a.autono, 0, c.batchautono, c.batchslno, a.itcd, a.stktype, a.qntyin, b.itnm, b.uomcd, ";
            sql += "a.stkqnty, a.rate, b.hsnsaccd, '', a.batchno, a.sizecd, a.partcd ";
            sql += "union all ";
            sql += "select a.autono, a.slno+1000 slno, '' batchautono, 0 batchslno, a.itcd, '' stktype, '' qntyin, b.itdescn itnm, b.uom uomcd, 0 wght, a.qnty, 0 nos,  ";
            sql += "0 stkqnty, a.rate, b.hsncd hsncode, a.itrem, a.batchno, '' partcd, '' sizecd ";
            sql += "from " + scm1 + ".t_txninvdtl a, " + scmI + ".m_item b ";
            sql += "where a.itcd = b.itcd(+) ) b, ";
            sql += "" + scm1 + ".t_txn c, " + scm1 + ".t_txntrans d, " + scm1 + ".t_cntrl_hdr e, ";
            sql += " " + scmf + ".m_uom h,  " + scmf + ".m_subleg i, " + scm1 + ".t_batchmst l, ";
            sql += scm1 + ".m_sitem m, " + scm1 + ".m_parts n, " + scm1 + ".m_size o, " + scm1 + ".m_group p, " + scm1 + ".t_cntrl_hdr q ";
            sql += "where a.autono = b.autono and a.autono = c.autono and a.autono = d.autono(+) and a.autono = e.autono(+) and ";
            sql += "b.uomcd = h.uomcd(+) and c.slcd = i.slcd(+) and b.batchautono = l.batchautono(+) and b.batchslno = l.batchslno(+) and ";
            sql += "b.itcd=m.itcd(+) and b.partcd=n.partcd(+) and b.sizecd=o.sizecd(+) and m.itgrpcd=p.itgrpcd(+) and l.orgbatchautono=q.autono(+) ";
            sql += "order by autono, slno, print_seq";
            DataTable rstbl = masterHelp.SQLquery(sql);

            string sqlc = "";
            sqlc += "where a.autono = b.autono(+) and b.doccd = '" + VE.DOCCD + "' and ";
            sqlc += "b.docdt >= to_date('" + VE.FDT + "','dd/mm/yyyy') and b.docdt <= to_date('" + VE.FDT + "','dd/mm/yyyy') and ";
            sqlc += "b.doconlyno >= '" + VE.FDOCNO + "' and b.doconlyno <= '" + VE.TDOCNO + "' and ";
            sqlc += "b.compcd = '" + COM + "' and b.loccd = '" + LOC + "' ";

            sql = "";
            sql += "select a.autono, c.slcd, c.linecd, g.linenm, f.cancel, f.docno, f.docdt, f.usr_id, e.recvperson, e.lorryno, g.linenm, ";
            sql += "d.slnm, d.add1, d.add2, d.add3, d.add4, d.add5, d.add6, d.add7, d.statecd, d.panno, d.gstno, d.regmobile, ";
            sql += "c.mcpautono, j.docno mcpno, j.docdt mcpdt, j.refno mcprefno, c.cutrecdocautono, i.docno cutdocno, i.docdt cutdocdt, ";
            sql += "nvl(b.docrem,'') docrem from ";
            sql += "( ";
            sql += "select distinct a.autono from " + scm1 + ".t_txndtl a, " + scm1 + ".t_cntrl_hdr b ";
            sql += sqlc;
            sql += "union ";
            sql += "select distinct a.autono from " + scm1 + ".t_progdtl a, " + scm1 + ".t_cntrl_hdr b ";
            sql += sqlc;
            sql += ") a, ";
            sql += "( select a.autono, listagg(a.docrem, '') within group (order by a.slno) docrem ";
            sql += "from " + scm1 + ".t_cntrl_hdr_rem a group by a.autono ) b, " + scm1 + ".t_txn c, " + scmf + ".m_subleg d, " + scm1 + ".t_txntrans e, ";
            sql += scm1 + ".t_cntrl_hdr f, " + scm1 + ".m_linemast g, " + scm1 + ".t_cntrl_hdr h, " + scm1 + ".t_cntrl_hdr i, " + scm1 + ".t_mcp j ";
            sql += "where a.autono=b.autono(+) and a.autono=c.autono(+) and c.slcd=d.slcd(+) and a.autono=e.autono(+) and a.autono=f.autono(+) and c.linecd=g.linecd(+) and ";
            sql += "c.mcpautono=h.autono(+) and c.cutrecdocautono=i.autono(+) and c.mcpautono=j.autono(+) ";
            sql += "order by docdt, docno ";
            DataTable tblauto = masterHelp.SQLquery(sql);

            DataTable IR = new DataTable("");
            IR.Columns.Add("autono", typeof(string), "");
            IR.Columns.Add("slcd", typeof(string), "");
            IR.Columns.Add("slnm", typeof(string), "");
            IR.Columns.Add("linenm", typeof(string), "");
            IR.Columns.Add("sladd1", typeof(string), "");
            IR.Columns.Add("sladd2", typeof(string), "");
            IR.Columns.Add("sladd3", typeof(string), "");
            IR.Columns.Add("sladd4", typeof(string), "");
            IR.Columns.Add("sladd5", typeof(string), "");
            IR.Columns.Add("sladd6", typeof(string), "");
            IR.Columns.Add("sladd7", typeof(string), "");
            IR.Columns.Add("sladd8", typeof(string), "");
            IR.Columns.Add("gooditem", typeof(string), "");
            IR.Columns.Add("jobnm", typeof(string), "");
            IR.Columns.Add("docno", typeof(string), "");
            IR.Columns.Add("docdt", typeof(string), "");
            IR.Columns.Add("vechlno", typeof(string), "");
            IR.Columns.Add("recvperson", typeof(string), "");
            IR.Columns.Add("slno", typeof(double), "");
            IR.Columns.Add("itcd", typeof(string), "");
            IR.Columns.Add("itnm", typeof(string), "");
            IR.Columns.Add("partnm", typeof(string), "");
            IR.Columns.Add("stktype", typeof(string), "");
            IR.Columns.Add("sizenm", typeof(string), "");
            IR.Columns.Add("qntyin", typeof(string), "");
            IR.Columns.Add("uomnm", typeof(string), "");
            IR.Columns.Add("hsncode", typeof(string), "");
            IR.Columns.Add("millnm", typeof(string), "");
            IR.Columns.Add("decimals", typeof(double), "");
            IR.Columns.Add("uomdecimals", typeof(double), "");
            IR.Columns.Add("nos", typeof(double), "");
            IR.Columns.Add("stkqnty", typeof(double), "");
            IR.Columns.Add("qnty", typeof(double), "");
            IR.Columns.Add("gsm", typeof(string), "");
            IR.Columns.Add("texture", typeof(string), "");
            IR.Columns.Add("gauge", typeof(string), "");
            IR.Columns.Add("ll", typeof(string), "");
            IR.Columns.Add("dia", typeof(string), "");
            IR.Columns.Add("maxdecimals", typeof(double), "");
            IR.Columns.Add("tqnty", typeof(double), "");
            IR.Columns.Add("tstkqnty", typeof(double), "");
            IR.Columns.Add("tnos", typeof(double), "");
            IR.Columns.Add("trem", typeof(string), "");
            IR.Columns.Add("user_nm", typeof(string), "");
            IR.Columns.Add("bold_f", typeof(string), "");
            IR.Columns.Add("mchnname", typeof(string), "");
            IR.Columns.Add("batchno", typeof(string), "");
            IR.Columns.Add("colrnm", typeof(string), "");
            IR.Columns.Add("proguniqno", typeof(string), "");
            IR.Columns.Add("proguniqnojpg", typeof(string), "");
            IR.Columns.Add("recotype", typeof(double), "");
            IR.Columns.Add("iisuomnm", typeof(string), "");
            IR.Columns.Add("pcsperbox", typeof(double), "");
            IR.Columns.Add("styleno", typeof(string), "");
            IR.Columns.Add("cutrecvhno", typeof(string), "");
            IR.Columns.Add("mcprefno", typeof(string), "");
            IR.Columns.Add("recrefno", typeof(string), "");
            IR.Columns.Add("orgrefno", typeof(string), "");
            IR.Columns.Add("orgbatchdocdt", typeof(string), "");
            IR.Columns.Add("orgbatchdocno", typeof(string), "");
            IR.Columns.Add("progprint", typeof(string), "");
            IR.Columns.Add("wght", typeof(double), "");
            IR.Columns.Add("twght", typeof(double), "");
            IR.Columns.Add("tusedwt", typeof(double), "");
            IR.Columns.Add("twaswt", typeof(double), "");
            IR.Columns.Add("cloth_used", typeof(double), "");
            IR.Columns.Add("cloth_was", typeof(double), "");

            bool hdrprint = true;
            ReportDocument reportdocument = new ReportDocument();
            Int32 maxR = 0, i = 0;
            Int32 x = 0, maxX = tblauto.Rows.Count - 1;
            Int32 rNo = 0, sln = 0;
            double maxDeci = 0, currdeci = 0;
            while (x <= maxX)
            {
                //address
                string add = ""; string[] address;
                string cfld = "", rfld = ""; int rf = 0;
                for (int f = 1; f <= 7; f++)
                {
                    cfld = "add" + Convert.ToString(f).ToString();
                    if (tblauto.Rows[x][cfld].ToString() != "")
                    {
                        rf = rf + 1;
                        if (add == "")
                        {
                            add = add + tblauto.Rows[x][cfld].ToString();
                        }
                        else
                        {
                            add = add + Cn.GCS() + tblauto.Rows[x][cfld].ToString();
                        }
                    }
                }
                if (tblauto.Rows[x]["gstno"].ToString() != "")
                {
                    rf = rf + 1;
                    add = add + Cn.GCS() + "GST # " + tblauto.Rows[x]["gstno"].ToString();
                }
                if (tblauto.Rows[x]["panno"].ToString() != "")
                {
                    rf = rf + 1;
                    add = add + Cn.GCS() + "PAN # " + tblauto.Rows[x]["panno"].ToString();
                }
                address = add.Split(Convert.ToChar(Cn.GCS()));

                string selitgrptype = "";
                selitgrptype = "C,F,Y";
                double t_qnty = 0, t_nos = 0, t_stkqnty = 0, t_wght = 0;
                double tusedwt = 0, twaswt = 0;
                string autono = tblauto.Rows[x]["autono"].ToString();
                string trem = tblauto.Rows[x]["docrem"].ToString(), usr_id = tblauto.Rows[x]["usr_id"].ToString();
                string recrefno = "", mcprefno = "", orgrefno = "";
                mcprefno = tblauto.Rows[x]["mcprefno"].ToString();
                if (mcprefno != "") mcprefno += " dtd." + tblauto.Rows[x]["mcpdt"].ToString().retDateStr();
                #region Receive Material Printing

                DataTable tbl = new DataTable();
                var rows1 = rstbl.AsEnumerable()
                    .Where(q => ((string)q["autono"]) == autono && selitgrptype.Contains((string)q["itgrptype"]));
                if (rows1.Any()) tbl = rows1.CopyToDataTable();

                var recvchdata = (from DataRow dr in tbl.Rows
                                  select new
                                  {
                                      recdocno = dr["recdocno"],
                                      recdoccd = dr["recdoccd"],
                                      recdocdt = dr["recdocdt"]
                                  }).Distinct().ToList();

                recrefno = string.Join(",", (from a in recvchdata select a.recdoccd.ToString() + "-" + a.recdocno.ToString() + " dt " + a.recdocdt.ToString().retDateStr()).Distinct());

                var orgbatchdata = (from DataRow dr in tbl.Rows
                                    select new
                                    {
                                        recdocno = dr["orgbatchdocno"],
                                        recdoccd = dr["orgbatchdoccd"],
                                        recdocdt = dr["orgbatchdocdt"]
                                    }).Distinct().ToList();

                orgrefno = string.Join(",", (from a in orgbatchdata select a.recdoccd.ToString() + "-" + a.recdocno.ToString() + " dt " + a.recdocdt.ToString().retDateStr()).Distinct());
                string qntyin = "";
                maxR = tbl.Rows.Count - 1; i = 0; sln = 0; hdrprint = true;
                string partcd = tbl.Rows[i]["partcd"].ToString();
                while (tbl.Rows[i]["autono"].ToString() == autono)
                {
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                prnloop:
                    IR.Rows[rNo]["cloth_used"] = tbl.Rows[i]["cloth_used"];
                    IR.Rows[rNo]["cloth_was"] = tbl.Rows[i]["cloth_was"];
                    IR.Rows[rNo]["recotype"] = "2";
                    IR.Rows[rNo]["autono"] = tbl.Rows[i]["autono"];
                    IR.Rows[rNo]["slcd"] = tblauto.Rows[x]["slcd"];
                    IR.Rows[rNo]["slnm"] = tblauto.Rows[x]["slnm"];
                    IR.Rows[rNo]["mcprefno"] = mcprefno;
                    IR.Rows[rNo]["recrefno"] = recrefno;
                    IR.Rows[rNo]["orgrefno"] = orgrefno;
                    IR.Rows[rNo]["cutrecvhno"] = tblauto.Rows[x]["cutdocno"].ToString() + " dtd. " + tblauto.Rows[x]["cutdocdt"].ToString().retDateStr();
                    if (tblauto.Rows[x]["linenm"].ToString().retStr() != "") IR.Rows[rNo]["linenm"] = "Unit - " + tblauto.Rows[x]["linenm"].ToString();
                    if (tblauto.Rows[x]["linenm"].ToString().retStr() != "") chlntype = "INTER FACTORY CHALLAN"; else chlntype = "ROAD CHALLAN";

                    int coutadd = 0;
                    for (int g = 0; g <= address.Count() - 1; g++)
                    {
                        coutadd++;
                        rfld = "sladd" + Convert.ToString(coutadd);
                        IR.Rows[rNo][rfld] = address[g].ToString();
                    }

                    IR.Rows[rNo]["gooditem"] = gooditem;
                    IR.Rows[rNo]["jobnm"] = jobnm;
                    IR.Rows[rNo]["docno"] = tblauto.Rows[x]["docno"];
                    IR.Rows[rNo]["docdt"] = tblauto.Rows[x]["docdt"].ToString().retDateStr();
                    IR.Rows[rNo]["vechlno"] = tblauto.Rows[x]["lorryno"];
                    IR.Rows[rNo]["recvperson"] = tblauto.Rows[x]["recvperson"];
                    IR.Rows[rNo]["trem"] = trem;
                    IR.Rows[rNo]["user_nm"] = usr_id;

                    if (hdrprint == false)
                    {
                        sln++;
                        IR.Rows[rNo]["iisuomnm"] = tbl.Rows[i]["uomnm"].ToString();
                        IR.Rows[rNo]["slno"] = sln;
                        IR.Rows[rNo]["itcd"] = tbl.Rows[i]["itcd"];
                        string dsc = tbl.Rows[i]["itnm"].ToString();
                        if (tbl.Rows[i]["mchnname"].ToString() != "") dsc += " " + tbl.Rows[i]["mchnname"].ToString();
                        if (tbl.Rows[i]["itgrptype"].ToString() == "F")
                        {
                            dsc = tbl.Rows[i]["styleno"].ToString() + " - " + tbl.Rows[i]["sizenm"].ToString();
                            if (tbl.Rows[i]["partnm"].ToString() != "") dsc += " [" + tbl.Rows[i]["partnm"].ToString() + "] ";
                        }
                        IR.Rows[rNo]["orgbatchdocno"] = tbl.Rows[i]["orgbatchdocno"];
                        IR.Rows[rNo]["orgbatchdocdt"] = tbl.Rows[i]["orgbatchdocdt"].ToString().retDateStr("yy");
                        IR.Rows[rNo]["itnm"] = dsc;
                        IR.Rows[rNo]["styleno"] = tbl.Rows[i]["styleno"];
                        IR.Rows[rNo]["pcsperbox"] = Convert.ToDouble(tbl.Rows[i]["pcsperbox"]);
                        IR.Rows[rNo]["partnm"] = tbl.Rows[i]["partnm"];
                        IR.Rows[rNo]["sizenm"] = tbl.Rows[i]["sizenm"];
                        IR.Rows[rNo]["qntyin"] = qntyin;
                        IR.Rows[rNo]["hsncode"] = tbl.Rows[i]["hsncode"];
                        IR.Rows[rNo]["millnm"] = tbl.Rows[i]["millnm"];
                        IR.Rows[rNo]["nos"] = Convert.ToDouble(tbl.Rows[i]["nos"]);
                        double pp = 12;
                        if (QntyIn == "B") pp = Convert.ToDouble(tbl.Rows[i]["pcsperbox"]);

                        IR.Rows[rNo]["stkqnty"] = QntyIn != "" ? Salesfunc.ConvPcstoBox(Convert.ToDouble(tbl.Rows[i]["qnty"]), pp) : Convert.ToDouble(tbl.Rows[i]["stkqnty"]);

                        IR.Rows[rNo]["wght"] = Convert.ToDouble(tbl.Rows[i]["wght"].ToString());
                        IR.Rows[rNo]["qnty"] = Convert.ToDouble(tbl.Rows[i]["qnty"].ToString());
                        currdeci = Convert.ToDouble(tbl.Rows[i]["decimals"].ToString());
                        if (maxDeci < currdeci) maxDeci = currdeci;
                        IR.Rows[rNo]["uomnm"] = tbl.Rows[i]["uomnm"].ToString();
                        IR.Rows[rNo]["decimals"] = currdeci;
                        IR.Rows[rNo]["maxdecimals"] = maxDeci;
                        IR.Rows[rNo]["gsm"] = Convert.ToDouble(tbl.Rows[i]["gsm"]) == 0 ? "" : tbl.Rows[i]["gsm"];
                        IR.Rows[rNo]["texture"] = tbl.Rows[i]["texture"];
                        IR.Rows[rNo]["gauge"] = Convert.ToDouble(tbl.Rows[i]["gauge"]) == 0 ? "" : tbl.Rows[i]["gauge"];
                        IR.Rows[rNo]["ll"] = Convert.ToDouble(tbl.Rows[i]["ll"]) == 0 ? "" : tbl.Rows[i]["ll"];
                        IR.Rows[rNo]["dia"] = Convert.ToDouble(tbl.Rows[i]["dia"]) == 0 ? "" : tbl.Rows[i]["dia"];
                        IR.Rows[rNo]["batchno"] = tbl.Rows[i]["batchno"];
                        IR.Rows[rNo]["colrnm"] = tbl.Rows[i]["colrnm"];
                        IR.Rows[rNo]["mchnname"] = tbl.Rows[i]["mchnname"];
                    }

                    if (hdrprint == true)
                    {
                        IR.Rows[rNo]["recotype"] = "1";
                        hdrprint = false;
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        goto prnloop;
                    }
                    if (tbl.Rows[i]["partcd"].ToString() == partcd) t_qnty = t_qnty + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                    t_nos = t_nos + Convert.ToDouble(tbl.Rows[i]["nos"]);
                    t_stkqnty = t_stkqnty + Convert.ToDouble(IR.Rows[rNo]["stkqnty"]);
                    t_wght = t_wght + Convert.ToDouble(tbl.Rows[i]["wght"]);
                    i++;
                    if (i > maxR) break;
                }

                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["recotype"] = "3";
                IR.Rows[rNo]["autono"] = tbl.Rows[i - 1]["autono"];
                IR.Rows[rNo]["tqnty"] = t_qnty.ToString();
                IR.Rows[rNo]["tnos"] = t_nos;
                IR.Rows[rNo]["twght"] = t_wght;
                IR.Rows[rNo]["tstkqnty"] = t_stkqnty;
                IR.Rows[rNo]["qntyin"] = qntyin;
                IR.Rows[rNo]["maxdecimals"] = maxDeci;
                IR.Rows[rNo]["trem"] = trem;
                #endregion
                tusedwt = t_wght;
                #region Wastage/Patti Printing
                selitgrptype = "W,T";
                var rows2 = rstbl.AsEnumerable()
                    .Where(q => ((string)q["autono"]) == autono && selitgrptype.Contains((string)q["itgrptype"]));
                if (rows1.Any()) tbl = rows2.CopyToDataTable();

                maxR = tbl.Rows.Count - 1; i = 0; sln = 0;
                t_nos = 0; t_qnty = 0; hdrprint = true;
                maxDeci = 0; currdeci = 0;
                while (tbl.Rows[i]["autono"].ToString() == autono)
                {
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                progloop:

                    IR.Rows[rNo]["recotype"] = "12";
                    IR.Rows[rNo]["autono"] = tbl.Rows[i]["autono"];
                    IR.Rows[rNo]["slcd"] = tblauto.Rows[x]["slcd"];
                    IR.Rows[rNo]["slnm"] = tblauto.Rows[x]["slnm"];
                    IR.Rows[rNo]["mcprefno"] = mcprefno;
                    IR.Rows[rNo]["recrefno"] = recrefno;
                    IR.Rows[rNo]["orgrefno"] = orgrefno;
                    IR.Rows[rNo]["cutrecvhno"] = tblauto.Rows[x]["cutdocno"].ToString() + " dtd. " + tblauto.Rows[x]["cutdocdt"].ToString().retDateStr();
                    if (tblauto.Rows[x]["linenm"].ToString().retStr() != "") IR.Rows[rNo]["linenm"] = "Unit - " + tblauto.Rows[x]["linenm"].ToString();
                    if (tblauto.Rows[x]["linenm"].ToString().retStr() != "") chlntype = "INTER FACTORY CHALLAN"; else chlntype = "ROAD CHALLAN";

                    int coutadd = 0;
                    for (int g = 0; g <= address.Count() - 1; g++)
                    {
                        coutadd++;
                        rfld = "sladd" + Convert.ToString(coutadd);
                        IR.Rows[rNo][rfld] = address[g].ToString();
                    }

                    IR.Rows[rNo]["gooditem"] = gooditem;
                    IR.Rows[rNo]["jobnm"] = jobnm;
                    IR.Rows[rNo]["docno"] = tblauto.Rows[x]["docno"];
                    IR.Rows[rNo]["docdt"] = tblauto.Rows[x]["docdt"].ToString().retDateStr();
                    IR.Rows[rNo]["vechlno"] = tblauto.Rows[x]["lorryno"];
                    IR.Rows[rNo]["recvperson"] = tblauto.Rows[x]["recvperson"];
                    IR.Rows[rNo]["trem"] = trem;
                    IR.Rows[rNo]["user_nm"] = usr_id;

                    if (hdrprint == false)
                    {
                        sln++;
                        IR.Rows[rNo]["slno"] = sln;
                        IR.Rows[rNo]["itcd"] = tbl.Rows[i]["itcd"];
                        string dsc = tbl.Rows[i]["itnm"].ToString();
                        if (tbl.Rows[i]["mchnname"].ToString() != "") dsc += " " + tbl.Rows[i]["mchnname"].ToString();
                        IR.Rows[rNo]["itnm"] = dsc;
                        IR.Rows[rNo]["styleno"] = tbl.Rows[i]["styleno"];
                        IR.Rows[rNo]["pcsperbox"] = Convert.ToDouble(tbl.Rows[i]["pcsperbox"]);
                        IR.Rows[rNo]["partnm"] = tbl.Rows[i]["partnm"];
                        IR.Rows[rNo]["sizenm"] = tbl.Rows[i]["sizenm"];
                        IR.Rows[rNo]["uomnm"] = tbl.Rows[i]["uomnm"].ToString();
                        IR.Rows[rNo]["hsncode"] = tbl.Rows[i]["hsncode"];
                        IR.Rows[rNo]["millnm"] = tbl.Rows[i]["millnm"];
                        currdeci = Convert.ToDouble(tbl.Rows[i]["decimals"].ToString());
                        IR.Rows[rNo]["nos"] = Convert.ToDouble(tbl.Rows[i]["nos"]);
                        double qnty = Convert.ToDouble(tbl.Rows[i]["qnty"]), s_qnty = Convert.ToDouble(tbl.Rows[i]["stkqnty"]);
                        if (progprint == true) IR.Rows[i]["progprint"] = "Y";
                        IR.Rows[rNo]["qnty"] = qnty;
                        IR.Rows[rNo]["qntyin"] = tbl.Rows[i]["qntyin"];
                        IR.Rows[rNo]["stkqnty"] = Convert.ToDouble(tbl.Rows[i]["stkqnty"]);
                        if (tbl.Rows[i]["itgrptype"].ToString() == "W") twaswt = twaswt + qnty; else tusedwt = tusedwt + qnty;
                        t_stkqnty = t_stkqnty + s_qnty;
                        IR.Rows[rNo]["decimals"] = currdeci;
                        if (maxDeci < currdeci) maxDeci = currdeci;
                        IR.Rows[rNo]["maxdecimals"] = maxDeci;
                        IR.Rows[rNo]["gsm"] = Convert.ToDouble(tbl.Rows[i]["gsm"]) == 0 ? "" : tbl.Rows[i]["gsm"];
                        IR.Rows[rNo]["texture"] = tbl.Rows[i]["texture"];
                        IR.Rows[rNo]["gauge"] = Convert.ToDouble(tbl.Rows[i]["gauge"]) == 0 ? "" : tbl.Rows[i]["gauge"];
                        IR.Rows[rNo]["ll"] = Convert.ToDouble(tbl.Rows[i]["ll"]) == 0 ? "" : tbl.Rows[i]["ll"];
                        IR.Rows[rNo]["dia"] = Convert.ToDouble(tbl.Rows[i]["dia"]) == 0 ? "" : tbl.Rows[i]["dia"];
                        IR.Rows[rNo]["batchno"] = tbl.Rows[i]["batchno"];
                        IR.Rows[rNo]["colrnm"] = tbl.Rows[i]["colrnm"];
                        IR.Rows[rNo]["proguniqno"] = ""; // tbl.Rows[i]["proguniqno"];
                        IR.Rows[rNo]["mchnname"] = tbl.Rows[i]["mchnname"];
                        //if (tbl.Rows[i]["proguniqno"].ToString() != "")
                        //{
                        //    IR.Rows[rNo]["proguniqnojpg"] = "c:\\improvar\\" + tbl.Rows[i]["proguniqno"] + ".jpg";
                        //    barcode = tbl.Rows[i]["proguniqno"].ToString();
                        //    //CnBarCode.BarcodeGeneratre(tbl.Rows[i]["proguniqno"].ToString());
                        //    CnBarCode.genBarCode(tbl.Rows[i]["proguniqno"].ToString(), true);
                        //}
                    }

                    if (hdrprint == true)
                    {
                        IR.Rows[rNo]["recotype"] = "11";
                        hdrprint = false;
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        goto progloop;
                    }

                    t_qnty = t_qnty + Convert.ToDouble(tbl.Rows[i]["qnty"]);
                    t_nos = t_nos + Convert.ToDouble(tbl.Rows[i]["nos"]);

                    i++;
                    if (i > maxR) break;
                }

                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["recotype"] = "13";
                IR.Rows[rNo]["autono"] = tbl.Rows[i - 1]["autono"];
                IR.Rows[rNo]["tqnty"] = t_qnty;
                IR.Rows[rNo]["tstkqnty"] = t_stkqnty;
                IR.Rows[rNo]["tnos"] = t_nos;
                IR.Rows[rNo]["trem"] = trem;
                IR.Rows[rNo]["maxdecimals"] = maxDeci;
                IR.Rows[rNo]["tusedwt"] = tusedwt;
                IR.Rows[rNo]["twaswt"] = twaswt;
                IR.Rows[rNo]["cloth_used"] = tbl.Rows[i - 1]["cloth_used"];
                IR.Rows[rNo]["cloth_was"] = tbl.Rows[i - 1]["cloth_was"];
                #endregion
                //eof 
                x++;
            }

            string[] compaddress;
            compaddress = Salesfunc.retCompAddress(gocd).Split(Convert.ToChar(Cn.GCS()));
            string rptname = "~/Report/" + repname + ".rpt";

            reportdocument.Load(Server.MapPath(rptname));
            reportdocument.SetDataSource(IR);
            reportdocument.SetParameterValue("complogo", Salesfunc.retCompLogo());
            reportdocument.SetParameterValue("compnm", compaddress[0]);
            reportdocument.SetParameterValue("compadd", compaddress[1]);
            reportdocument.SetParameterValue("compstat", compaddress[2]);
            reportdocument.SetParameterValue("locaadd", compaddress[3]);
            reportdocument.SetParameterValue("locastat", compaddress[4]);
            reportdocument.SetParameterValue("billheading", hddsp);
            reportdocument.SetParameterValue("chlntype", chlntype);

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