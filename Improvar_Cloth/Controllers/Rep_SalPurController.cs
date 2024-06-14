using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;

namespace Improvar.Controllers
{
    public class Rep_SalPurController : Controller
    {
        public static string[,] headerArray;
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Sales_func = new Salesfunc();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_SalPur
        public ActionResult Rep_SalPur()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Sale Purchase Report";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);

                    VE.DropDown_list_ITGRP = DropDownHelp.GetItgrpcdforSelection();
                    VE.Itgrpnm = MasterHelp.ComboFill("itgrpcd", VE.DropDown_list_ITGRP, 0, 1);

                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);

                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection();
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);

                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    //string selgrp = MasterHelp.GetUserITGrpCd().Replace("','", ",");
                    //string[] selgrpcd = selgrp.Split(',');

                    VE.DropDown_list = (from i in DB.M_GROUP select new DropDown_list() { value = i.ITGRPCD, text = i.ITGRPNM }).OrderBy(s => s.text).ToList();

                    VE.DefaultView = true;
                    VE.FDT = CommVar.FinStartDate(UNQSNO);
                    VE.TDT = CommVar.CurrDate(UNQSNO);
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
        public ActionResult Rep_SalPur(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string fdt = FC["FDT"].retDateStr(), tdt = FC["TDT"].retDateStr();
                string itgrpcd = "";
                string reptype = FC["reptype"].ToString();
                string sql = "";
                string txntag = "'SB','SR'";
                string selitcd = "", party = "";
                DataTable rsTbl = new DataTable();
                int maxR = 0, i = 0, rNo = 0;
                DataTable tblpur = new DataTable();

                if (FC.AllKeys.Contains("itcdvalue"))  //'ITEM'
                {
                    selitcd = CommFunc.retSqlformat(FC["itcdvalue"].ToString());
                }
                if (FC.AllKeys.Contains("slcdvalue"))
                {
                    party = FC["slcdvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("itgrpcdvalue")) itgrpcd = CommFunc.retSqlformat(FC["itgrpcdvalue"].retStr());

                tblpur = Sales_func.GenStocktblwithVal("FIFO", Convert.ToDateTime(fdt).AddDays(-1).retDateStr(), "", "", itgrpcd,selitcd,"", true, "", false, "", "", "", "", false,false, tdt);

                sql = "";
                sql += "select a.autono, a.doctag, a.docno, a.docdt, a.slcd, a.slnm, a.slno, a.itcd,a.ITGRPCD,a.itgrpnm,a.STYLENO, a.itnm, a.uomnm, a.batchautono, a.batchno,a.barno, " + Environment.NewLine; ;
                sql += "a.qnty, a.txblval, ROUND(((a.rate*a.qnty) + ((a.discamt/a.iqnty)*a.qnty)),2) btxblval,a.itnm||' '||a.STYLENO itstyle, nvl(b.othamt,0) othamt, " + Environment.NewLine; ;
                sql += "a.pblno, a.pdocdt, a.prate from " + Environment.NewLine; ;

                sql += "( select a.autono, c.doctag, nvl(c.prefno,d.docno) docno, d.docdt, c.slcd, e.slnm, b.slno, b.itcd,f.itgrpcd,k.itgrpnm,f.STYLENO, f.itnm, h.uomnm, ''batchautono, i.batchno,i.barno, " + Environment.NewLine; ;
                sql += "nvl(a.qnty,b.qnty) qnty, nvl(b.qnty,a.qnty) iqnty, b.rate, nvl(b.discamt,0)-nvl(b.SCMDISCAMT,0) discamt, " + Environment.NewLine; ;
                sql += "b.amt-nvl(b.discamt,0)-nvl(b.SCMDISCAMT,0) txblval, nvl(j.prefno,l.docno) pblno, l.docdt pdocdt, i.rate+nvl(i.othrate,0) prate " + Environment.NewLine; ;
                sql += "from " + scm1 + ".t_batchdtl a, " + scm1 + ".t_txndtl b, " + scm1 + ".t_txn c, " + scm1 + ".t_cntrl_hdr d, " + Environment.NewLine; ;
                sql += scmf + ".m_subleg e, " + scm1 + ".m_sitem f, " + scmf + ".m_uom h, " + Environment.NewLine; ;
                sql += scm1 + ".t_batchmst i, " + scm1 + ".t_txn j, " + scm1 + ".m_group k, " + scm1 + ".t_cntrl_hdr l " + Environment.NewLine; ;
                sql += "where b.autono=a.autono(+) and b.slno=a.txnslno(+) and b.autono=c.autono and b.autono=d.autono and " + Environment.NewLine; ;
                sql += "f.uomcd=h.uomcd(+) and  a.barno=i.barno(+) and i.autono=j.autono(+) and i.autono=l.autono(+) and f.itgrpcd=k.itgrpcd(+) and " + Environment.NewLine; ;
                sql += "d.compcd='" + COM + "' and d.loccd='" + LOC + "' and nvl(d.cancel,'N')='N' and " + Environment.NewLine; ;
                sql += "c.doctag in (" + txntag + ") and  ";
                sql += "d.docdt >= to_date('" + fdt + "','dd/mm/yyyy') and d.docdt <= to_date('" + tdt + "','dd/mm/yyyy') and  " + Environment.NewLine; ;
                sql += "c.slcd=e.slcd and b.itcd=f.itcd(+) and f.itgrpcd in (" + itgrpcd + ") ) a, " + Environment.NewLine; ;

                sql += "( select a.autono, sum(case c.addless when 'A' then a.amtrate when 'L' then a.amtrate*-1 end) othamt " + Environment.NewLine; ;
                sql += "from " + scm1 + ".t_txnamt a, " + scm1 + ".t_cntrl_hdr b, " + scm1 + ".m_amttype c " + Environment.NewLine; ;
                sql += "where a.autono=b.autono and a.amtcd=c.amtcd and b.compcd='" + COM + "' and b.loccd='" + LOC + "' " + Environment.NewLine; ;
                sql += "group by a.autono ) b " + Environment.NewLine; ;

                sql += "where a.autono=b.autono(+) " + Environment.NewLine; ;

                if (selitcd.retStr() != "") sql += "and a.itcd in (" + selitcd + ")  " + Environment.NewLine;
                if (party.retStr() != "") sql += "and a.slcd in (" + party + ") " + Environment.NewLine;
                sql += "order by docdt, docno " + Environment.NewLine; ;
                rsTbl = MasterHelp.SQLquery(sql);

                if (tblpur.Rows.Count == 0)
                {
                    return Content("no records..");
                }

                DataTable IR = new DataTable("mstrep");
                PrintViewer PV = new PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                HC.RepStart(IR, 3);
                HC.GetPrintHeader(IR, "docno", "string", "c,30", "Doc No");
                HC.GetPrintHeader(IR, "docdt", "string", "c,10", "Doc Date");
                HC.GetPrintHeader(IR, "slcd", "string", "c,10", "Code");
                HC.GetPrintHeader(IR, "slnm", "string", "c,35", "Party Name");
                HC.GetPrintHeader(IR, "ITGRPNM", "string", "c,35", "Item Group");


                HC.GetPrintHeader(IR, "itcd", "string", "c,10", "Item Cd");
                HC.GetPrintHeader(IR, "STYLENO", "string", "c,35", "Design");
                //HC.GetPrintHeader(IR, "itnm", "string", "c,35", "Item Name");
                HC.GetPrintHeader(IR, "uom", "string", "c,4", "uom");
                HC.GetPrintHeader(IR, "qnty", "double", "n,12,4", "Qnty");
                HC.GetPrintHeader(IR, "srate", "double", "n,12,4", "S.Rate");
                HC.GetPrintHeader(IR, "samt", "double", "n,12,2", "Gross Amt");
                HC.GetPrintHeader(IR, "prate", "double", "n,12,4", "P.Rate");
                HC.GetPrintHeader(IR, "pamt", "double", "n,12,2", "Cost Amt");
                HC.GetPrintHeader(IR, "diffamt", "double", "n,16,2:###,##,##,##0.00", "Profit/;Loss(-)");
                HC.GetPrintHeader(IR, "pblno", "string", "c,16", "Purch Doc No");

                i = 0; maxR = 0; rNo = 0;
                maxR = rsTbl.Rows.Count - 1;
                double tqnty = 0, tsamt = 0, tpamt = 0, tdiffamt = 0 ;
                while (i <= maxR)
                {
                    double imult = 1;
                    if (rsTbl.Rows[i]["doctag"].ToString() == "SR") imult = -1;

                    double srate = Math.Round(rsTbl.Rows[i]["btxblval"].retDbl() / rsTbl.Rows[i]["qnty"].retDbl(), 6);
                    double prate = 0; 
                    string pdoc = "";
                    string barno = rsTbl.Rows[i]["barno"].retStr();
                    string itcd = rsTbl.Rows[i]["itcd"].retStr();
                    if (VE.Checkbox1 == true)
                    {
                        prate = Math.Round(rsTbl.Rows[i]["prate"].retDbl(), 6);
                    }
                    else
                    {
                        if (barno != "")
                        {
                            prate = (from DataRow dr in tblpur.Rows where dr["barno"].retStr() == barno && dr["itcd"].retStr() == itcd select dr["rate"].retDbl()).FirstOrDefault();
                            pdoc= (from DataRow dr in tblpur.Rows where dr["barno"].retStr() == barno && dr["itcd"].retStr() == itcd select dr["docno"].retStr()).FirstOrDefault();
                        }
                        else
                        {
                            prate = Math.Round(rsTbl.Rows[i]["prate"].retDbl(), 6);
                        }
                    }
                    double pamt = Math.Round(prate * rsTbl.Rows[i]["qnty"].retDbl(), 2);
                    double samt = 0;

                    samt = rsTbl.Rows[i]["btxblval"].retDbl() * imult;

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["docno"] = rsTbl.Rows[i]["docno"];
                    IR.Rows[rNo]["docdt"] = rsTbl.Rows[i]["docdt"].retDateStr();
                    IR.Rows[rNo]["slcd"] = rsTbl.Rows[i]["slcd"];
                    IR.Rows[rNo]["slnm"] = rsTbl.Rows[i]["slnm"];
                    IR.Rows[rNo]["ITGRPNM"] = rsTbl.Rows[i]["ITGRPNM"];
                    IR.Rows[rNo]["itcd"] = rsTbl.Rows[i]["itcd"];
                    IR.Rows[rNo]["STYLENO"] = rsTbl.Rows[i]["itstyle"];
                    //IR.Rows[rNo]["itnm"] = rsTbl.Rows[i]["itnm"];
                    IR.Rows[rNo]["uom"] = rsTbl.Rows[i]["uomnm"];
                    IR.Rows[rNo]["qnty"] = rsTbl.Rows[i]["qnty"].retDbl() * imult;
                    IR.Rows[rNo]["srate"] = srate;
                    IR.Rows[rNo]["samt"] = samt;
                    IR.Rows[rNo]["prate"] = prate;
                    IR.Rows[rNo]["pamt"] = pamt;
                    Double diffamt = samt - pamt;
                    if (rsTbl.Rows[i]["doctag"].ToString() == "SR") diffamt = (samt * -1) - pamt;
                    IR.Rows[rNo]["diffamt"] = diffamt;
                    IR.Rows[rNo]["pblno"] = pdoc;
                    IR.Rows[rNo]["celldesign"] = "diffamt=font-weight:bold;font-size:13px;";

                    tqnty = tqnty + (rsTbl.Rows[i]["qnty"].retDbl() * imult);
                    tsamt = tsamt + samt;
                    tpamt = tpamt + pamt;
                    tdiffamt = tdiffamt + diffamt;

                    i++;
                }

                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["dammy"] = "";
                IR.Rows[rNo]["slnm"] = "Grand Totals";
                IR.Rows[rNo]["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 2px solid;";
                IR.Rows[rNo]["qnty"] = tqnty;
                IR.Rows[rNo]["samt"] = tsamt;
                IR.Rows[rNo]["pamt"] = tpamt;
                IR.Rows[rNo]["diffamt"] = tdiffamt;

                string pghdr1 = "Sale Purchase Statement from " + fdt + " to " + tdt;
                string rephdr = "SalPurStmt";
                PV = HC.ShowReport(IR, rephdr, pghdr1, "", true, true, "L", false);

                TempData[rephdr] = PV;
                TempData[rephdr + "xxx"] = IR;
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = rephdr });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
    }
}
