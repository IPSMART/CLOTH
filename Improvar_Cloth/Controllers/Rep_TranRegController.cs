using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;

namespace Improvar.Controllers
{
    public class Rep_TranRegController : Controller
    {
        public static string[,] headerArray;
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        DropDownHelp dropDownHelp = new DropDownHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        Salesfunc salesfunc = new Salesfunc();
        // GET: Rep_TranReg
        public ActionResult Rep_TranReg()
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                ReportViewinHtml VE = new ReportViewinHtml();
                Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                ViewBag.formname = "Transporter Register";
                ViewBag.Title = "Transporter Register";
                VE.FDT = CommVar.FinStartDate(UNQSNO);
                VE.TDT = DateTime.Now.Date.ToShortDateString();
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                VE.DropDown_list_SLCD = dropDownHelp.DropDown_list_SLCD();
                VE.Slnm = masterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);
                VE.DropDown_list_SLCD = dropDownHelp.GetSlcdforSelection("T");
                VE.Translnm = masterHelp.ComboFill("translcd", VE.DropDown_list_SLCD, 0, 1);
                VE.DefaultView = true;
                return View(VE);
            }
        }
        [HttpPost]
        public ActionResult Rep_TranReg(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                string fdt = VE.FDT.retStr();
                string tdt = VE.TDT.retStr();


                string SLCD = "";
                if (FC.AllKeys.Contains("slcdvalue")) SLCD = CommFunc.retSqlformat(FC["slcdvalue"].ToString());
                string TRANSLCD = "";
                if (FC.AllKeys.Contains("translcdvalue")) TRANSLCD = CommFunc.retSqlformat(FC["translcdvalue"].ToString());

                DataTable IR = new DataTable("mstrep");
                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                HC.RepStart(IR, 3);
                HC.GetPrintHeader(IR, "docno", "string", "c,15", "Doc. No");
                HC.GetPrintHeader(IR, "docdt", "string", "d,10:dd/mm/yy", "Doc. Date");
                HC.GetPrintHeader(IR, "translnm", "string", "c,40", "Transporter");
                HC.GetPrintHeader(IR, "lrno", "string", "c,10", "LR Number");
                HC.GetPrintHeader(IR, "lrdt", "string", "d,10:dd/mm/yy", "LR;Date");
                HC.GetPrintHeader(IR, "ntwt", "double", "n,12,4", "Weight");
                HC.GetPrintHeader(IR, "parcel", "double", "n,10,2", "Parcel");
                HC.GetPrintHeader(IR, "slnm", "string", "c,40", "Party");
                HC.GetPrintHeader(IR, "amt", "double", "n,12,2", "Amt.");
                HC.GetPrintHeader(IR, "ACTFRGHTPAID", "double", "n,12,2", "Actual Freight.");
                if (VE.Checkbox1==true) HC.GetPrintHeader(IR, "topay", "string", "c,25", "Topay");

                string sql = "";
                sql += " select a.autono,a.docno,a.docdt,a.translcd,a.translnm,a.lrno,a.lrdt,a.trwt,a.slcd,a.slnm,a.blamt,a.ntwt,b.tranamt,a.cancel,a.topay,a.ACTFRGHTPAID from ";

                sql += "(select a.autono,c.docno,c.docdt,b.translcd,d.slnm translnm,b.lrno,b.lrdt,b.trwt,a.slcd,e.slnm,a.blamt,b.ntwt,c.cancel,g.topay,b.ACTFRGHTPAID ";
                sql += "from " + scm1 + ".t_txn a," + scm1 + ".t_txntrans b, " + scm1 + ".t_cntrl_hdr c," + scmf + ".m_subleg d," + scmf + ".m_subleg e," + scmf + ".m_doctype f, " + scm1 + ".t_txnoth g ";
                sql += "where a.autono = b.autono(+) and a.autono = c.autono(+) and b.translcd = d.slcd(+) and a.slcd = e.slcd(+) and c.doccd=f.doccd and a.autono = g.autono(+) and f.doctype in ('SBILD','SPRM','SBILL','SPBL','SPRM')  ";
                sql += "and c.compcd='" + COM + "' and c.loccd='" + LOC + "' and c.yr_cd='" + CommVar.YearCode(UNQSNO) + "'  ";
                if (fdt != "") sql += "and c.docdt >= to_date('" + fdt + "','dd/mm/yyyy')  ";
                if (tdt != "") sql += "and c.docdt <= to_date('" + tdt + "','dd/mm/yyyy')  ";
                if (SLCD != "") { sql += "and a.slcd in(" + SLCD + ")  "; }
                if (TRANSLCD != "") { sql += "and b.translcd in(" + TRANSLCD + ") "; }
                sql += "and b.translcd is not null )a, ";

                sql += "(select a.autono,a.amtcd,sum(nvl(a.amt,0))tranamt ";
                sql += "from " + scm1 + ".t_txnamt a," + scm1 + ".M_AMTTYPE b ";
                sql += "where a.amtcd=b.amtcd and b.TAXCODE='TC' ";
                sql += "group by  a.autono,a.amtcd )b ";

                sql += "where a.autono=b.autono(+)  ";
                sql += "order by a.docdt,a.autono ";

                DataTable tbl = masterHelp.SQLquery(sql);

                Int32 i = 0; Int32 rNo = 0;
                if (tbl == null || tbl.Rows.Count == 0) return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "Records not found !!" });
                Int32 maxR = tbl.Rows.Count - 1;
                while (i <= maxR)
                {
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    string cancrem = "";
                    if (tbl.Rows[i]["cancel"].ToString() == "Y") cancrem = "  (CANCELLED)";
                    IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"]+ cancrem;
                    IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].ToString().retDateStr();
                    IR.Rows[rNo]["translnm"] = tbl.Rows[i]["translnm"].ToString() + " [" + tbl.Rows[i]["translcd"].ToString() + "]";
                    IR.Rows[rNo]["lrno"] = tbl.Rows[i]["lrno"];
                    IR.Rows[rNo]["lrdt"] = tbl.Rows[i]["lrdt"].ToString().retDateStr();
                    if(tbl.Rows[i]["ntwt"].retDbl() != 0)
                    {
                        IR.Rows[rNo]["ntwt"] = tbl.Rows[i]["ntwt"];
                    }                    
                    IR.Rows[rNo]["slnm"] = tbl.Rows[i]["slnm"].ToString() + " [" + tbl.Rows[i]["slcd"].ToString() + "]";
                    IR.Rows[rNo]["amt"] = tbl.Rows[i]["tranamt"];
                    IR.Rows[rNo]["ACTFRGHTPAID"] = tbl.Rows[i]["ACTFRGHTPAID"];
                    if (VE.Checkbox1 == true) IR.Rows[rNo]["topay"] = tbl.Rows[i]["topay"];

                    i++;
                    if (i > maxR) break;
                }


                string pghdr1 = "Transporter Register  from " + fdt + " to " + tdt;
                string repname = "Transporter Register" + System.DateTime.Now;
                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);
                TempData[repname] = PV;
                TempData[repname + "xxx"] = IR;
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }

    }
}
