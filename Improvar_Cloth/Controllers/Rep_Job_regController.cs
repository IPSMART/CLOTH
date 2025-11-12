
using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using OfficeOpenXml;
using System.Collections.Generic;
using OfficeOpenXml.Style;

namespace Improvar.Controllers
{
    public class Rep_Job_regController : Controller
    {
        // GET: Rep_Job_reg
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string fdt = ""; string tdt = ""; bool showpacksize = false, showrate = false; bool showValue = false; string ReportType = "";
        string modulecode = CommVar.ModuleCode(); string repname = "";
        string pghdr1 = "";
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult Rep_Job_reg()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Job Work Register";
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    string com = CommVar.Compcd(UNQSNO); string gcs = Cn.GCS();
                    VE.TDT = CommVar.CurrDate(UNQSNO);
                    VE.DropDown_list_JOBCD = DropDownHelp.DropDown_JOBCD();

                    VE.DropDown_list = (from i in DB.M_DOCTYPE
                                        select new DropDown_list()
                                        {
                                            value = i.DOCCD,
                                            text = i.DOCNM
                                        }).Distinct().OrderBy(s => s.text).ToList();
                    VE.TEXTBOX11 = MasterHelp.ComboFill("doctype", VE.DropDown_list, 0, 0);


                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("J");
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);

                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);

                    VE.DropDown_list_LOCCD = DropDownHelp.DropDownLoccation();
                    VE.Locnm = MasterHelp.ComboFill("loccd", VE.DropDown_list_LOCCD, 1, 0);

                    VE.JOBCD = VE.MENU_PARA;
                    VE.DefaultView = true;
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
        public ActionResult Rep_Job_reg(FormCollection FC, ReportViewinHtml VE)
        {
            try
            {
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                fdt = VE.FDT.retDateStr();
                tdt = VE.TDT.retDateStr();
                string recdt = VE.TEXTBOX1.retDateStr();
                showValue = VE.Checkbox1;
                string ShowPending = VE.TEXTBOX2.retStr();
                ReportType = VE.TEXTBOX3.retStr();
                string RepFormat = VE.TEXTBOX4.retStr();
                string jobslcd = "", itcd = "", loccd = "", pghdr2 = "";
                string JOBCD = VE.JOBCD;
                if (FC.AllKeys.Contains("slcdvalue")) jobslcd = CommFunc.retSqlformat(FC["slcdvalue"].ToString());
                if (FC.AllKeys.Contains("itcdvalue")) itcd = CommFunc.retSqlformat(FC["itcdvalue"].ToString());
                if (FC.AllKeys.Contains("loccdvalue"))
                {
                    loccd = FC["loccdvalue"].ToString().retSqlformat();
                    pghdr2 += (pghdr2.retStr() == "" ? "" : "<br/>") + "Location :" + FC["loccdtext"].ToString();
                }
                string sql = "",sql2 = "";
                DataTable IR = new DataTable("mstrep");
                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();


                if (ReportType != "REGISTER")
                {

                    sql += "select a.autono, c.slcd, a.slno, c.nos, c.qnty, c.cutlength, i.itnm, i.styleno,h.ourdesign, h.pdesign, i.itgrpcd, j.itgrpnm,k.uomnm, " + Environment.NewLine;
                    sql += "c.itremark, c.sample, g.slnm, g.regmobile, ptch.docno , ptch.docdt, y.issamt,m.docno ORDDOCNO, " + Environment.NewLine;
                    sql += "b.autono recautono, rtch.docno recdocno, rtch.docdt recdocdt, b.prefno, b.prefdt, b.doctag,b.nos recnos, b.qnty recqnty, " + Environment.NewLine;
                    sql += "c.nos, c.qnty, c.nos-nvl(z.nos,0) balnos, c.qnty-nvl(z.qnty,0) balqnty,h.itcd,i.fabitcd,l.itnm fabitnm from " + Environment.NewLine;

                    sql += "(select a.autono, a.slno, a.autono || a.slno autoslno " + Environment.NewLine;
                    sql += "from " + scm + ".t_progmast a, " + scm + ".t_cntrl_hdr b " + Environment.NewLine;
                    sql += "where a.autono = b.autono(+) and " + Environment.NewLine;
                    sql += "b.compcd = '" + COM + "' and nvl(b.cancel, 'N') = 'N' and " + Environment.NewLine;
                    if (jobslcd != "") sql += "a.slcd in(" + jobslcd + ") and " + Environment.NewLine;
                    if (itcd != "") sql += "a.itcd in(" + itcd + ") and " + Environment.NewLine;
                    if (fdt != "") sql += "b.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and " + Environment.NewLine;
                    if (loccd.retStr() != "")
                    {
                        sql += "b.loccd in (" + loccd + ") and ";
                    }
                    sql += "b.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') ) a, " + Environment.NewLine;

                    sql += "(select a.progautono||a.progslno progautoslno,  " + Environment.NewLine;
                    sql += "a.nos, a.qnty, a.autono, a.rate, c.prefno, c.prefdt, c.doctag " + Environment.NewLine;
                    sql += "from " + scm + ".t_progdtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_txn c " + Environment.NewLine;
                    sql += "where a.autono = b.autono(+) and a.autono = c.autono(+)  and a.autono<>a.progautono and " + Environment.NewLine;
                    sql += "b.compcd = '" + COM + "' and nvl(b.cancel, 'N')= 'N' and  C.JOBCD='" + JOBCD + "' and  " + Environment.NewLine;
                    sql += "b.docdt <= to_date('" + (recdt == "" ? tdt : recdt) + "', 'dd/mm/yyyy') ) b, " + Environment.NewLine;

                    sql += "(select a.progautono||a.progslno progautoslno, sum(round(a.qnty * a.rate, 2)) issamt " + Environment.NewLine;
                    sql += "from " + scm + ".t_kardtl a, " + scm + ".t_cntrl_hdr b " + Environment.NewLine;
                    sql += "where a.autono = b.autono(+) and " + Environment.NewLine;
                    sql += "b.compcd = '" + COM + "' and nvl(b.cancel, 'N')= 'N' and " + Environment.NewLine;
                    sql += "b.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') " + Environment.NewLine;
                    sql += "group by a.progautono || a.progslno ) y, " + Environment.NewLine;

                    sql += "(select a.progautono||a.progslno progautoslno,  " + Environment.NewLine;
                    sql += "sum(a.nos) nos, sum(a.qnty) qnty " + Environment.NewLine;
                    sql += "from " + scm + ".t_progdtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_txn c " + Environment.NewLine;
                    sql += "where a.autono = b.autono(+) and a.autono = c.autono(+)  and a.autono<>a.progautono and " + Environment.NewLine;
                    sql += "b.compcd = '" + COM + "' and nvl(b.cancel, 'N')= 'N' and  C.JOBCD='" + JOBCD + "' and  " + Environment.NewLine;
                    sql += "b.docdt <= to_date('" + (recdt == "" ? tdt : recdt) + "', 'dd/mm/yyyy') " + Environment.NewLine;
                    sql += "group by a.progautono||a.progslno ) z, " + Environment.NewLine;

                    sql += scm + ".t_progmast c, " + scm + ".t_cntrl_hdr ptch, " + scm + ".t_cntrl_hdr rtch, " + Environment.NewLine;
                    sql += scmf + ".m_subleg g, " + scm + ".t_batchmst h, " + scm + ".m_sitem i, " + scm + ".m_group j, " + scmf + ".m_uom k, " + scm + ".m_sitem l, " + scm + ".t_cntrl_hdr m, " + scm + ".t_sord n "+ Environment.NewLine;
                    sql += "where a.autono=c.autono(+) and a.slno=c.slno(+) and a.autoslno = b.progautoslno(+) and a.autoslno = y.progautoslno(+) and a.autoslno = z.progautoslno(+) and " + Environment.NewLine;
                    sql += "a.autono = ptch.autono(+) and b.autono = rtch.autono(+) and C.JOBCD='" + JOBCD + "' and i.fabitcd=l.itcd(+) and c.ordautono=m.autono(+) and c.ordautono=n.autono(+) and  " + Environment.NewLine;
                    if (ShowPending == "PENDING") sql += "c.qnty-nvl(z.qnty,0) <> 0 and " + Environment.NewLine;
                    sql += "c.slcd = g.slcd(+) and c.barno = h.barno(+) and h.itcd = i.itcd(+) and i.itgrpcd = j.itgrpcd(+) and i.uomcd = k.uomcd(+) " + Environment.NewLine;
                    sql += "order by slnm, slcd, docdt, docno, autono, slno, recdocdt, recdocno " + Environment.NewLine;
                    DataTable tbl = MasterHelp.SQLquery(sql);

                    string str1 = "";
                    str1 += "select i.AUTONO,i.SLNO,i.TXNSLNO,k.ITGRPCD,n.ITGRPNM,n.BARGENTYPE,i.MTRLJOBCD,o.MTRLJOBNM,o.MTBARCODE,k.ITCD,k.ITNM,k.UOMCD,k.STYLENO,i.PARTCD,p.PARTNM,p.PRTBARCODE,i.STKTYPE,q.STKNAME,i.BARNO, ";
                    str1 += "j.COLRCD,m.COLRNM,m.CLRBARCODE,j.SIZECD,l.SIZENM,l.SZBARCODE,i.SHADE,i.QNTY,i.NOS,i.RATE,i.DISCRATE,i.DISCTYPE,i.TDDISCRATE,i.TDDISCTYPE,i.SCMDISCTYPE,i.SCMDISCRATE,i.HSNCODE,i.BALENO,j.PDESIGN,j.OURDESIGN,i.FLAGMTR,i.LOCABIN,i.BALEYR ";
                    str1 += ",n.SALGLCD,n.PURGLCD,n.SALRETGLCD,n.PURRETGLCD,i.itrem,i.RECPROGSLNO,k.NEGSTOCK,i.cutlength,i.itrem ";
                    str1 += "from " + scm + ".T_BATCHDTL i, " + scm + ".T_BATCHMST j, " + scm + ".M_SITEM k, " + scm + ".M_SIZE l, " + scm + ".M_COLOR m, ";
                    str1 += scm + ".M_GROUP n," + scm + ".M_MTRLJOBMST o," + scm + ".M_PARTS p," + scm + ".M_STKTYPE q ";
                    str1 += "where i.BARNO = j.BARNO(+) and j.ITCD = k.ITCD(+) and j.SIZECD = l.SIZECD(+) and j.COLRCD = m.COLRCD(+) and k.ITGRPCD=n.ITGRPCD(+) ";
                    str1 += "and i.MTRLJOBCD=o.MTRLJOBCD(+) and i.PARTCD=p.PARTCD(+) and i.STKTYPE=q.STKTYPE(+) ";
                    str1 += "order by i.SLNO ";
                    DataTable Mtrl = MasterHelp.SQLquery(str1);

                    if (tbl.Rows.Count == 0)
                    {
                        return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "Records not found !!" });
                    }

                    HC.RepStart(IR, 2);

                    if (ReportType == "DETAIL" || ReportType == "SUMMARY")
                    {
                        #region
                        if (RepFormat == "STANDARD") HC.GetPrintHeader(IR, "Slnm", "string", "c,25", "Jobber Name.");
                        HC.GetPrintHeader(IR, "docdt", "string", "c,10", "Iss. Date.");
                        HC.GetPrintHeader(IR, "docno", "string", "c,13", "Iss Doc No");
                        HC.GetPrintHeader(IR, "itgrpnm", "string", "c,10", "Group");
                        HC.GetPrintHeader(IR, "itnm", "string", "c,10", "Item");
                        HC.GetPrintHeader(IR, "styleno", "string", "c,6", "Styleno");
                        HC.GetPrintHeader(IR, "uom", "string", "c,4", "Uom");
                        HC.GetPrintHeader(IR, "Nos", "string", "n,5", "Nos");
                        HC.GetPrintHeader(IR, "cutlength", "double", "n,6,2", "cut length");
                        HC.GetPrintHeader(IR, "qnty", "double", "n,11,3", "Prog.Qnty");
                        if (showValue == true) HC.GetPrintHeader(IR, "issamt", "double", "n,15,2", "Iss Amt.");
                        HC.GetPrintHeader(IR, "itremarks", "string", "c,15", "itremark");

                        string rechdr = (ReportType == "SUMMARY" ? "Last " : "");                        
                        HC.GetPrintHeader(IR, "ORDDOCNO", "string", "c,13", "Order Number");                        
                        HC.GetPrintHeader(IR, "recdocdt", "string", "c,10", rechdr + "Rec Date");
                        HC.GetPrintHeader(IR, "recdocno", "string", "c,13", rechdr + "Rec No");

                        HC.GetPrintHeader(IR, "recnos", "double", "n,5", "Rec Nos.");
                        HC.GetPrintHeader(IR, "recqnty", "double", "n,11,3", "Rec Qnty.");

                        HC.GetPrintHeader(IR, "balqnty", "double", "n,11,3", "Bal Qnty.");
                        if (showValue == true) HC.GetPrintHeader(IR, "balamt", "double", "n,12,3", "Bal Value");
                        IR.Columns.Add("slcd", typeof(string), "");
                        string[] arrcolnm = { "Nos", "cutlength", "qnty", "issamt", "recnos", "recqnty", "balqnty", "balamt" };
                        Int32 rNo = 0; Int32 i = 0; Int32 maxR = 0;
                        i = 0; maxR = tbl.Rows.Count - 1;
                        string lastslcd = "";
                        while (i <= maxR)
                        {
                            string slcd = tbl.Rows[i]["slcd"].retStr();
                            int cnt = 0;
                            if (RepFormat == "JOBBERWISE")
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["Dammy"] = "" + tbl.Rows[i]["slnm"].retStr() + " [" + tbl.Rows[i]["slcd"].retStr() + "]" + (tbl.Rows[i]["regmobile"].retStr() == "" ? "" : "Mob : " + tbl.Rows[i]["regmobile"].retStr());
                                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                                lastslcd = slcd;
                            }
                            while (tbl.Rows[i]["slcd"].retStr() == slcd)
                            {
                                string autono = tbl.Rows[i]["autono"].retStr();
                                bool frstreco = true;
                                while (tbl.Rows[i]["slcd"].retStr() == slcd && tbl.Rows[i]["autono"].retStr() == autono)
                                {
                                    double progslno = tbl.Rows[i]["slno"].retDbl();
                                    double trecnos = 0, trecqnty = 0;
                                    bool frstslnoreco = true;
                                    while (tbl.Rows[i]["slcd"].retStr() == slcd && tbl.Rows[i]["autono"].retStr() == autono && tbl.Rows[i]["slno"].retDbl() == progslno)
                                    {
                                        if (frstslnoreco == true || ReportType == "DETAIL")
                                        {
                                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                            IR.Rows[rNo]["slcd"] = tbl.Rows[i]["slcd"].retStr();
                                        }
                                        if (RepFormat == "STANDARD")
                                        {
                                            IR.Rows[rNo]["Slnm"] = "" + tbl.Rows[i]["slnm"].retStr() + "[" + tbl.Rows[i]["slcd"].retStr() + "]";
                                        }
                                            IR.Rows[rNo]["ORDDOCNO"] = tbl.Rows[i]["ORDDOCNO"].retStr();
                                        
                                            if (frstreco == true || RepFormat == "STANDARD")
                                        {
                                            IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].retDateStr();
                                            IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].retStr();
                                        }
                                        if (frstslnoreco == true)
                                        {
                                            IR.Rows[rNo]["itgrpnm"] = tbl.Rows[i]["itgrpnm"].retStr();
                                            IR.Rows[rNo]["styleno"] = tbl.Rows[i]["styleno"].retStr();
                                            IR.Rows[rNo]["itnm"] = tbl.Rows[i]["itnm"].retStr();
                                            IR.Rows[rNo]["uom"] = tbl.Rows[i]["uomnm"].retStr();
                                            IR.Rows[rNo]["nos"] = tbl.Rows[i]["nos"].retStr();
                                            IR.Rows[rNo]["cutlength"] = tbl.Rows[i]["cutlength"].retDbl();
                                            IR.Rows[rNo]["qnty"] = tbl.Rows[i]["qnty"].retDbl();
                                            if (showValue == true) IR.Rows[rNo]["issamt"] = tbl.Rows[i]["issamt"].retDbl();
                                            IR.Rows[rNo]["itremarks"] = tbl.Rows[i]["itremark"].retStr();
                                        }
                                        //Receive
                                        if (ReportType != "SUMMARY")
                                        {
                                            IR.Rows[rNo]["recdocdt"] = tbl.Rows[i]["recdocdt"].retDateStr();
                                            IR.Rows[rNo]["recdocno"] = tbl.Rows[i]["recdocno"].retStr();
                                            IR.Rows[rNo]["recnos"] = tbl.Rows[i]["recnos"].retDbl();
                                            IR.Rows[rNo]["recqnty"] = tbl.Rows[i]["recqnty"].retDbl();
                                        }
                                        trecnos = trecnos + tbl.Rows[i]["recnos"].retDbl();
                                        trecqnty = trecqnty + tbl.Rows[i]["recqnty"].retDbl();
                                        //
                                        frstslnoreco = false;
                                        frstreco = false;
                                        i++;
                                        cnt++;
                                        if (i > maxR) break;
                                    }
                                    IR.Rows[rNo]["balqnty"] = tbl.Rows[i - 1]["balqnty"].retDbl();
                                    double avrate = (tbl.Rows[i - 1]["qnty"].retDbl() == 0 ? 0 : (tbl.Rows[i - 1]["issamt"].retDbl() / tbl.Rows[i - 1]["qnty"].retDbl()).toRound(2));
                                    double balamt = (avrate * tbl.Rows[i - 1]["balqnty"].retDbl()).toRound(0);
                                    if (showValue == true) IR.Rows[rNo]["balamt"] = balamt;

                                    if (cnt > 0 && VE.Checkbox2 == true)
                                    {
                                        var ITCD_material_DATA = (from DataRow x in Mtrl.Rows
                                                                  where x["autono"].retStr() == autono && x["RECPROGSLNO"].retDbl() == progslno
                                                                  group x by new { ITCD = x["itcd"].retStr(), ITNM = x["itnm"].retStr(), STYLENO = x["styleno"].retStr(), ITREM = x["itrem"].retStr() } into x
                                                                  select new
                                                                  {
                                                                      itcd = x.Key.ITCD,
                                                                      itnm = x.Key.STYLENO + x.Key.ITNM,
                                                                      itrem = x.Key.ITREM,
                                                                      qnty = x.Sum(s => s["qnty"].retDbl())
                                                                      //TWASTGQNTY = x.Sum(s => s["qnty"].retDbl())
                                                                  }).ToList();

                                        if (ITCD_material_DATA != null)
                                        {
                                            foreach (var k in ITCD_material_DATA)
                                            {
                                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                                IR.Rows[rNo]["flag"] = "font-style:italic;";
                                                IR.Rows[rNo]["itnm"] = k.itcd.retStr();
                                                IR.Rows[rNo]["Styleno"] = k.itnm.retStr();
                                                IR.Rows[rNo]["qnty"] = k.qnty.retStr();
                                                IR.Rows[rNo]["itremarks"] = k.itrem.retStr();
                                            }
                                        }
                                    }
                                    if (ReportType == "SUMMARY")
                                    {
                                        IR.Rows[rNo]["recdocdt"] = tbl.Rows[i - 1]["recdocdt"].retDateStr();
                                        IR.Rows[rNo]["recdocno"] = tbl.Rows[i - 1]["recdocno"].retStr();
                                        if (trecqnty != 0)
                                        {
                                            IR.Rows[rNo]["recnos"] = trecnos;
                                            IR.Rows[rNo]["recqnty"] = trecqnty;
                                        }
                                    } 
                                    if (i > maxR) break;
                                }
                                
                               
                                if (i > maxR) break;
                            }                           
                            if (RepFormat == "JOBBERWISE")
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["itnm"] = "Total of " + tbl.Rows[i - 1]["slnm"].ToString();

                                for (int a = 9; a < IR.Columns.Count - 1; a++)
                                {
                                    string colnm = IR.Columns[a].ColumnName;
                                    if (arrcolnm.Contains(colnm))
                                    {
                                        var purewisegrptotal = (from DataRow dr in IR.Rows where dr["slcd"].retStr() == slcd select dr[colnm].retDbl()).Sum();
                                        if (purewisegrptotal.retDbl() != 0)
                                        {
                                            IR.Rows[rNo][IR.Columns[a].ColumnName] = purewisegrptotal;
                                        }
                                    }
                                }
                                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 1px solid;border-top: 1px solid;";
                            }
                            if (i > maxR) break;
                        }
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["itnm"] = "Grand Total";
                        for (int a = 9; a < IR.Columns.Count - 1; a++)
                        {
                            string colnm = IR.Columns[a].ColumnName;
                            if (arrcolnm.Contains(colnm))
                            {
                                var purewisegrptotal = (from DataRow dr in IR.Rows where dr["slcd"].retStr() != "" select dr[colnm].retDbl()).Sum();
                                if (purewisegrptotal.retDbl() != 0)
                                {
                                    IR.Rows[rNo][IR.Columns[a].ColumnName] = purewisegrptotal;
                                }
                            }
                        }
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 1px solid;border-top: 1px solid;";

                        IR.Columns.Remove("slcd");

                        string repname = "Job Register".retRepname();
                        pghdr1 = (ShowPending == "PENDING" ? "Pending " : " ") + "Job Work register " + (ReportType == "SUMARRY" ? "Sumarry " : "Details ") + (recdt != "" ? " Received date: " + recdt + "" : " ") + " Jobcd:" + JOBCD + (fdt != "" ? " from " + fdt + " to " : " as on ") + tdt;
                        PV = HC.ShowReport(IR, repname, pghdr1, pghdr2, true, true, "P", false);
                        TempData[repname] = PV;
                        return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
                        #endregion
                    }
                    else
                    {
                        #region
                        HC.GetPrintHeader(IR, "itgrpnm", "string", "c,12", "Group");
                        HC.GetPrintHeader(IR, "itnm", "string", "c,20", "Item Description");
                        HC.GetPrintHeader(IR, "uom", "string", "c,10", "Uom");
                        HC.GetPrintHeader(IR, "balqnty", "double", "n,15,3", "Bal Qnty.");
                        if (showValue == true) HC.GetPrintHeader(IR, "balamt", "double", "n,15,3", "Bal Value");
                        IR.Columns.Add("slcd", typeof(string), "");

                        if (RepFormat == "JOBBERWISE")
                        {
                            DataView dv = new DataView(tbl);
                            dv.Sort = "slcd,slnm,itcd,itnm ASC";
                            tbl = dv.ToTable();
                        }
                        else
                        {
                            DataView dv = new DataView(tbl);
                            dv.Sort = "itcd,itnm ASC";
                            tbl = dv.ToTable();
                        }

                        #region Stock printing
                        Int32 rNo = 0; Int32 i = 0; Int32 maxR = 0;
                        i = 0; maxR = tbl.Rows.Count - 1;
                        string fld1 = RepFormat == "JOBBERWISE" ? "slcd" : "itcd";
                        while (i <= maxR)
                        {
                            string val1 = tbl.Rows[i][fld1].retStr();
                            if (RepFormat == "JOBBERWISE")
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["Dammy"] = "" + tbl.Rows[i]["slnm"].retStr() + " [" + tbl.Rows[i]["slcd"].retStr() + "]" + (tbl.Rows[i]["regmobile"].retStr() == "" ? "" : "Mob : " + tbl.Rows[i]["regmobile"].retStr());
                                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                            }
                            while (tbl.Rows[i][fld1].retStr() == val1)
                            {
                                string itcd1 = tbl.Rows[i]["itcd"].retStr();
                                double balqnty = 0, balamt = 0;
                                while (tbl.Rows[i][fld1].retStr() == val1 && tbl.Rows[i]["itcd"].retStr() == itcd1)
                                {
                                    string autoslno = tbl.Rows[i]["autono"].retStr() + tbl.Rows[i]["slno"].retStr();
                                    while (tbl.Rows[i][fld1].retStr() == val1 && tbl.Rows[i]["itcd"].retStr() == itcd1 && tbl.Rows[i]["autono"].retStr() + tbl.Rows[i]["slno"].retStr() == autoslno)
                                    {
                                        i++;
                                        if (i > maxR) break;
                                    }
                                    balqnty += tbl.Rows[i - 1]["balqnty"].retDbl();
                                    double avrate = (tbl.Rows[i - 1]["qnty"].retDbl() == 0 ? 0 : (tbl.Rows[i - 1]["issamt"].retDbl() / tbl.Rows[i - 1]["qnty"].retDbl()).toRound(2));
                                    balamt += (avrate * tbl.Rows[i - 1]["balqnty"].retDbl()).toRound(0);
                                    //i++;
                                    if (i > maxR) break;
                                }
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["itgrpnm"] = tbl.Rows[i - 1]["itgrpnm"].retStr();
                                IR.Rows[rNo]["itnm"] = tbl.Rows[i - 1]["fabitnm"].retStr() + " " + tbl.Rows[i - 1]["styleno"].retStr() + " " + tbl.Rows[i - 1]["itnm"].retStr();
                                IR.Rows[rNo]["uom"] = tbl.Rows[i - 1]["uomnm"].retStr();
                                IR.Rows[rNo]["slcd"] = tbl.Rows[i - 1]["slcd"].retStr();
                                IR.Rows[rNo]["balqnty"] = balqnty;
                                if (showValue == true) IR.Rows[rNo]["balamt"] = balamt;
                                if (i > maxR) break;
                            }
                            if (RepFormat == "JOBBERWISE")
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                IR.Rows[rNo]["itgrpnm"] = "Total of " + tbl.Rows[i - 1]["slnm"].ToString();

                                for (int a = 6; a < IR.Columns.Count - 1; a++)
                                {
                                    string colnm = IR.Columns[a].ColumnName;
                                    var purewisegrptotal = (from DataRow dr in IR.Rows where dr["slcd"].retStr() == val1 select dr[colnm].retDbl()).Sum();
                                    if (purewisegrptotal.retDbl() != 0)
                                    {
                                        IR.Rows[rNo][IR.Columns[a].ColumnName] = purewisegrptotal;
                                    }
                                }
                                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 1px solid;border-top: 1px solid;";

                            }
                            if (i > maxR) break;
                        }
                        #endregion
                        IR.Columns.Remove("slcd");

                        string repname = "Job Register".retRepname();
                        pghdr1 = (ShowPending == "PENDING" ? "Pending " : " ") + "Job Work register " + (ReportType == "SUMARRY" ? "Sumarry " : "Details ") + (recdt != "" ? " Received date: " + recdt + "" : " ") + " Jobcd:" + JOBCD + (fdt != "" ? " from " + fdt + " to " : " as on ") + tdt;
                        PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);
                        TempData[repname] = PV;
                        return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
                        #endregion
                    }
                }
                else
                {
                    string RegisterType = VE.TEXTBOX10;

                    string doctype = "";

                    if (JOBCD == "ST")
                    {
                        if (RegisterType == "Issue")
                        {
                            doctype = "'OSTI'";
                        }
                        else if (RegisterType == "Receive")
                        {
                            doctype = "'OSTR'";
                        }
                        else
                        {
                            doctype = "'OSTI','OSTR'";
                        }

                    }
                    else if (JOBCD == "JB")
                    {
                        if (RegisterType == "Issue")
                        {
                            doctype = "'OJWI'";
                        }
                        else if (RegisterType == "Receive")
                        {
                            doctype = "'OJWR'";
                        }
                        else
                        {
                            doctype = "'OJWI','OJWR'";
                        }

                    }
                    else if (JOBCD == "DY")
                    {
                        if (RegisterType == "Issue")
                        {
                            doctype = "'ODYI'";
                        }
                        else if (RegisterType == "Receive")
                        {
                            doctype = "'ODYR'";
                        }
                        else
                        {
                            doctype = "'ODYI','ODYR'";
                        }

                    }
                    else if (JOBCD == "PR")
                    {
                        if (RegisterType == "Issue")
                        {
                            doctype = "'OPRI'";
                        }
                        else if (RegisterType == "Receive")
                        {
                            doctype = "'OPRR'";
                        }
                        else
                        {
                            doctype = "'OPRI','OPRR'";
                        }

                    }
                    else if (JOBCD == "KR")
                    {
                        if (RegisterType == "Issue")
                        {
                            doctype = "'OEMI'";
                        }
                        else if (RegisterType == "Receive")
                        {
                            doctype = "'OEMR'";
                        }
                        else
                        {
                            doctype = "'OEMI','OEMR'";
                        }

                    }
                    sql = "";
                    sql += "select a.autono,a.slno,c.docdt,c.docno,f.slnm,a.nos,a.qnty,c.doccd doccode, " + Environment.NewLine;
                    sql += "f.slarea area,f.gstno,a.amt basicamt,0 disc1amt,0 disc2amt, " + Environment.NewLine;
                    sql += "a.txblval taxableval,a.igstper,a.igstamt,a.cgstper,a.cgstamt,a.sgstper,a.sgstamt, " + Environment.NewLine;
                    sql += "d.tcsamt,d.roamt roffamt,d.blamt billval,b.recprogautono,b.recprogslno,e.itnm,e.styleno||' '||e.itnm itstyle, " + Environment.NewLine;
                    sql += "d.prefno,d.prefdt,nvl(a.hsncode,e.hsncode)hsncode,e.uomcd,a.rate,a.stkdrcr,nvl(c.cancel,'N')cancel  " + Environment.NewLine;
                    sql += "from " + scm + ".t_txndtl a," + scm + ".t_batchdtl b," + scm + ".t_cntrl_hdr c," + scm + " " + Environment.NewLine;
                    sql += ".t_txn d," + scm + ".m_sitem e," + scmf + ".m_subleg f ," + scm + ".m_doctype g " + Environment.NewLine;
                    sql += "where a.autono = b.autono(+) and a.slno=b.slno(+) and a.autono=c.autono(+) " + Environment.NewLine;
                    sql += "and c.autono=d.autono(+) and a.itcd=e.itcd(+) and  d.slcd=f.slcd(+) and c.doccd=g.doccd(+) " + Environment.NewLine;
                    sql += "and g.doctype in (" + doctype + ") and d.JOBCD='" + JOBCD + "' and c.compcd = '" + COM + "' and " + Environment.NewLine;
                    if (jobslcd != "") sql += "d.slcd in(" + jobslcd + ") and " + Environment.NewLine;
                    if (itcd != "") sql += "a.itcd in(" + itcd + ") and " + Environment.NewLine;
                    if (fdt != "") sql += "c.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and " + Environment.NewLine;
                    if (loccd.retStr() != "")
                    {
                        sql += "e.loccd in (" + loccd + ") and ";
                    }
                    sql += "c.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy')  " + Environment.NewLine;
                    sql += "order by c.docdt,c.docno,slnm,d.slcd,e.styleno||' '||e.itnm,a.itcd  " + Environment.NewLine;

                    DataTable maintbl = MasterHelp.SQLquery(sql);
                    if (maintbl.Rows.Count == 0)
                    {
                        return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "Records not found !!" });
                    }


                    HC.RepStart(IR, 3);

                    HC.GetPrintHeader(IR, "doccd", "string", "c,5", "Doc;Code");
                    HC.GetPrintHeader(IR, "docdt", "string", "d,10:dd/mm/yy", ";Doc Date");
                    HC.GetPrintHeader(IR, "docno", "string", "c,18", ";Doc No");
                    HC.GetPrintHeader(IR, "prefdt", "string", "d,10:dd/mm/yy", "Party;Bill Date");
                    HC.GetPrintHeader(IR, "prefno", "string", "c,18", "Party;Bill No");
                    HC.GetPrintHeader(IR, "slnm", "string", "c,20", "Party;Name");
                    HC.GetPrintHeader(IR, "slarea", "string", "c,10", "Area");
                    HC.GetPrintHeader(IR, "gstno", "string", "c,15", "GST No.");
                    HC.GetPrintHeader(IR, "itnm", "string", "c,35", "Item;Name");
                    HC.GetPrintHeader(IR, "hsncode", "string", "c,8", "HSN/SAC");
                    HC.GetPrintHeader(IR, "uomcd", "string", "c,2", "Uom");
                    HC.GetPrintHeader(IR, "nos", "double", "n,5", "Nos");
                    HC.GetPrintHeader(IR, "qnty", "double", "n,12,3", "Qnty");
                    HC.GetPrintHeader(IR, "rate", "double", "n,10,2", "Rate");
                    HC.GetPrintHeader(IR, "amt", "double", "n,12,2", "Basic;Amount");
                    HC.GetPrintHeader(IR, "taxableval", "double", "n,12,2", "Taxable;Value");
                    HC.GetPrintHeader(IR, "igstper", "double", "n,5,3", "IGST;%");
                    HC.GetPrintHeader(IR, "igstamt", "double", "n,10,2", "IGST;Amt");
                    HC.GetPrintHeader(IR, "cgstper", "double", "n,5,3", "CGST;%");
                    HC.GetPrintHeader(IR, "cgstamt", "double", "n,10,2", "CGST;Amt");
                    HC.GetPrintHeader(IR, "sgstper", "double", "n,5,3", "SGST;%");
                    HC.GetPrintHeader(IR, "sgstamt", "double", "n,10,2", "SGST;Amt");
                    HC.GetPrintHeader(IR, "tcsamt", "double", "n,10,2", "TCS;Amt");
                    HC.GetPrintHeader(IR, "roamt", "double", "n,6,2", "R/Off;Amt");
                    HC.GetPrintHeader(IR, "blamt", "double", "n,12,2", ";Bill Value");


                    Int32 rNo = 0; Int32 i = 0; Int32 maxR = 0;
                    i = 0; maxR = maintbl.Rows.Count - 1;
                    int istore = 0;
                    while (i <= maxR)
                    {
                        double mult = 1;
                        if (RegisterType == "Both" && maintbl.Rows[i]["stkdrcr"].retStr() == "D")
                        {
                            mult = -1;
                        }
                        string cancrem = "";
                        if (maintbl.Rows[i]["cancel"].ToString() == "Y") cancrem = "  (CANCELLED)";
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["flag"] = "font-weight:bold";
                        IR.Rows[rNo]["doccd"] = maintbl.Rows[i]["doccode"].retStr();
                        IR.Rows[rNo]["docdt"] = maintbl.Rows[i]["docdt"].retDateStr();
                        IR.Rows[rNo]["docno"] = maintbl.Rows[i]["docno"].retStr() + cancrem;
                        IR.Rows[rNo]["slnm"] = maintbl.Rows[i]["slnm"].retStr();
                        IR.Rows[rNo]["slarea"] = maintbl.Rows[i]["area"].retStr();
                        IR.Rows[rNo]["gstno"] = maintbl.Rows[i]["gstno"].retStr();
                        IR.Rows[rNo]["tcsamt"] = maintbl.Rows[i]["tcsamt"].retDbl();
                        IR.Rows[rNo]["roamt"] = maintbl.Rows[i]["roffamt"].retDbl() * mult;
                        IR.Rows[rNo]["blamt"] = maintbl.Rows[i]["billval"].retDbl() * mult;
                        IR.Rows[rNo]["igstper"] = maintbl.Rows[i]["igstper"].retDbl();
                        IR.Rows[rNo]["cgstper"] = maintbl.Rows[i]["cgstper"].retDbl();
                        IR.Rows[rNo]["sgstper"] = maintbl.Rows[i]["sgstper"].retDbl();
                        IR.Rows[rNo]["prefno"] = maintbl.Rows[i]["prefno"].retStr();
                        IR.Rows[rNo]["prefdt"] = maintbl.Rows[i]["prefdt"].retDateStr();

                        string autono = maintbl.Rows[i]["autono"].ToString();
                        istore = i;
                        double nos = 0, qnty = 0, amt = 0, taxableval = 0, igstamt = 0, cgstamt = 0, sgstamt = 0;
                        while (maintbl.Rows[i]["autono"].ToString() == autono)
                        {
                            nos += maintbl.Rows[i]["nos"].retDbl();
                            qnty += maintbl.Rows[i]["qnty"].retDbl();
                            amt += maintbl.Rows[i]["basicamt"].retDbl();
                            taxableval += maintbl.Rows[i]["taxableval"].retDbl();
                            igstamt += maintbl.Rows[i]["igstamt"].retDbl();
                            cgstamt += maintbl.Rows[i]["cgstamt"].retDbl();
                            sgstamt += maintbl.Rows[i]["sgstamt"].retDbl();

                            i++;
                            if (i > maxR) break;
                        }


                        IR.Rows[rNo]["nos"] = (nos * mult);
                        IR.Rows[rNo]["qnty"] = (qnty * mult);
                        IR.Rows[rNo]["amt"] = (amt * mult);
                        IR.Rows[rNo]["taxableval"] = (taxableval * mult);
                        IR.Rows[rNo]["igstamt"] = (igstamt * mult);
                        IR.Rows[rNo]["cgstamt"] = (cgstamt * mult);
                        IR.Rows[rNo]["sgstamt"] = (sgstamt * mult);

                        i = istore;
                        while (maintbl.Rows[i]["autono"].ToString() == autono)
                        {

                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;

                            IR.Rows[rNo]["itnm"] = maintbl.Rows[i]["itstyle"].retStr();
                            IR.Rows[rNo]["hsncode"] = maintbl.Rows[i]["hsncode"].retStr();
                            IR.Rows[rNo]["uomcd"] = maintbl.Rows[i]["uomcd"].retStr();
                            IR.Rows[rNo]["nos"] = (maintbl.Rows[i]["nos"].retDbl() * mult);
                            IR.Rows[rNo]["qnty"] = (maintbl.Rows[i]["qnty"].retDbl() * mult);
                            IR.Rows[rNo]["rate"] = (maintbl.Rows[i]["rate"].retDbl() * mult);
                            IR.Rows[rNo]["amt"] = (maintbl.Rows[i]["basicamt"].retDbl() * mult);
                            IR.Rows[rNo]["taxableval"] = (maintbl.Rows[i]["taxableval"].retDbl() * mult);
                            IR.Rows[rNo]["igstper"] = maintbl.Rows[i]["igstper"].retDbl();
                            IR.Rows[rNo]["igstamt"] = (maintbl.Rows[i]["igstamt"].retDbl() * mult);
                            IR.Rows[rNo]["cgstper"] = maintbl.Rows[i]["cgstper"].retDbl();
                            IR.Rows[rNo]["cgstamt"] = (maintbl.Rows[i]["cgstamt"].retDbl() * mult);
                            IR.Rows[rNo]["sgstper"] = maintbl.Rows[i]["sgstper"].retDbl();
                            IR.Rows[rNo]["sgstamt"] = (maintbl.Rows[i]["sgstamt"].retDbl() * mult);

                            i++;
                            if (i > maxR) break;
                        }


                        if (i > maxR) break;
                    }

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["docno"] = "Grand Total";
                    for (int a = 14; a < IR.Columns.Count; a++)
                    {
                        string colnm = IR.Columns[a].ColumnName;
                        var purewisegrptotal = (from DataRow dr in IR.Rows where dr["docno"].retStr() != "" select dr[colnm].retDbl()).Sum();
                        if (purewisegrptotal.retDbl() != 0)
                        {
                            IR.Rows[rNo][IR.Columns[a].ColumnName] = purewisegrptotal;
                        }
                    }
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 1px solid;border-top: 1px solid;";

                    string repname = "Job Register".retRepname();
                    pghdr1 = RegisterType + " Register " + " Jobcd:" + JOBCD + (fdt != "" ? " from " + fdt + " to " : " as on ") + tdt;
                    PV = HC.ShowReport(IR, repname, pghdr1, pghdr2, true, true, "P", false);
                    TempData[repname] = PV;
                    return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
                }

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        //public ActionResult Rep_Job_reg(FormCollection FC, ReportViewinHtml VE)
        //{
        //    try
        //    {
        //        string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
        //        fdt = VE.FDT.retDateStr();
        //        tdt = VE.TDT.retDateStr();
        //        string recdt = VE.TEXTBOX1.retDateStr();
        //        showValue = VE.Checkbox1;
        //        string ShowPending = VE.TEXTBOX2.retStr();
        //        ReportType = VE.TEXTBOX3.retStr();
        //        string RepFormat = VE.TEXTBOX4.retStr();
        //        string jobslcd = "", itcd = "";
        //        string JOBCD = VE.JOBCD;
        //        if (FC.AllKeys.Contains("slcdvalue")) jobslcd = CommFunc.retSqlformat(FC["slcdvalue"].ToString());
        //        if (FC.AllKeys.Contains("itcdvalue")) itcd = CommFunc.retSqlformat(FC["itcdvalue"].ToString());


        //        string sql = "";

        //        sql += "select a.autono, c.slcd, a.slno, c.nos, c.qnty, c.cutlength, i.itnm, i.styleno,h.ourdesign, h.pdesign, i.itgrpcd, j.itgrpnm,k.uomnm, " + Environment.NewLine;
        //        sql += "c.itremark, c.sample, g.slnm, g.regmobile, ptch.docno , ptch.docdt, y.issamt, " + Environment.NewLine;
        //        sql += "b.autono recautono, rtch.docno recdocno, rtch.docdt recdocdt, b.prefno, b.prefdt, b.doctag,b.nos recnos, b.qnty recqnty, " + Environment.NewLine;
        //        sql += "c.nos, c.qnty, c.nos-nvl(z.nos,0) balnos, c.qnty-nvl(z.qnty,0) balqnty,h.itcd,i.fabitcd,l.itnm fabitnm from " + Environment.NewLine;

        //        sql += "(select a.autono, a.slno, a.autono || a.slno autoslno " + Environment.NewLine;
        //        sql += "from " + scm + ".t_progmast a, " + scm + ".t_cntrl_hdr b " + Environment.NewLine;
        //        sql += "where a.autono = b.autono(+) and " + Environment.NewLine;
        //        sql += "b.compcd = '" + COM + "' and nvl(b.cancel, 'N') = 'N' and " + Environment.NewLine;
        //        if (jobslcd != "") sql += "a.slcd in(" + jobslcd + ") and " + Environment.NewLine;
        //        if (itcd != "") sql += "a.itcd in(" + itcd + ") and " + Environment.NewLine;
        //        if (fdt != "") sql += "b.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and " + Environment.NewLine;
        //        sql += "b.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') ) a, " + Environment.NewLine;

        //        sql += "(select a.progautono||a.progslno progautoslno,  " + Environment.NewLine;
        //        sql += "a.nos, a.qnty, a.autono, a.rate, c.prefno, c.prefdt, c.doctag " + Environment.NewLine;
        //        sql += "from " + scm + ".t_progdtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_txn c " + Environment.NewLine;
        //        sql += "where a.autono = b.autono(+) and a.autono = c.autono(+)  and a.autono<>a.progautono and " + Environment.NewLine;
        //        sql += "b.compcd = '" + COM + "' and nvl(b.cancel, 'N')= 'N' and  C.JOBCD='" + JOBCD + "' and  " + Environment.NewLine;
        //        sql += "b.docdt <= to_date('" + (recdt == "" ? tdt : recdt) + "', 'dd/mm/yyyy') ) b, " + Environment.NewLine;

        //        sql += "(select a.progautono||a.progslno progautoslno, sum(round(a.qnty * a.rate, 2)) issamt " + Environment.NewLine;
        //        sql += "from " + scm + ".t_kardtl a, " + scm + ".t_cntrl_hdr b " + Environment.NewLine;
        //        sql += "where a.autono = b.autono(+) and " + Environment.NewLine;
        //        sql += "b.compcd = '" + COM + "' and nvl(b.cancel, 'N')= 'N' and " + Environment.NewLine;
        //        sql += "b.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') " + Environment.NewLine;
        //        sql += "group by a.progautono || a.progslno ) y, " + Environment.NewLine;

        //        sql += "(select a.progautono||a.progslno progautoslno,  " + Environment.NewLine;
        //        sql += "sum(a.nos) nos, sum(a.qnty) qnty " + Environment.NewLine;
        //        sql += "from " + scm + ".t_progdtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_txn c " + Environment.NewLine;
        //        sql += "where a.autono = b.autono(+) and a.autono = c.autono(+)  and a.autono<>a.progautono and " + Environment.NewLine;
        //        sql += "b.compcd = '" + COM + "' and nvl(b.cancel, 'N')= 'N' and  C.JOBCD='" + JOBCD + "' and  " + Environment.NewLine;
        //        sql += "b.docdt <= to_date('" + (recdt == "" ? tdt : recdt) + "', 'dd/mm/yyyy') " + Environment.NewLine;
        //        sql += "group by a.progautono||a.progslno ) z, " + Environment.NewLine;

        //        sql += scm + ".t_progmast c, " + scm + ".t_cntrl_hdr ptch, " + scm + ".t_cntrl_hdr rtch, " + Environment.NewLine;
        //        sql += scmf + ".m_subleg g, " + scm + ".t_batchmst h, " + scm + ".m_sitem i, " + scm + ".m_group j, " + scmf + ".m_uom k, " + scm + ".m_sitem l " + Environment.NewLine;
        //        sql += "where a.autono=c.autono(+) and a.slno=c.slno(+) and a.autoslno = b.progautoslno(+) and a.autoslno = y.progautoslno(+) and a.autoslno = z.progautoslno(+) and " + Environment.NewLine;
        //        sql += "a.autono = ptch.autono(+) and b.autono = rtch.autono(+) and C.JOBCD='" + JOBCD + "' and i.fabitcd=l.itcd(+) and " + Environment.NewLine;
        //        if (ShowPending == "PENDING") sql += "c.qnty-nvl(z.qnty,0) <> 0 and " + Environment.NewLine;
        //        sql += "c.slcd = g.slcd(+) and c.barno = h.barno(+) and h.itcd = i.itcd(+) and i.itgrpcd = j.itgrpcd(+) and i.uomcd = k.uomcd(+) " + Environment.NewLine;
        //        sql += "order by slnm, slcd, docdt, docno, autono, slno, recdocdt, recdocno " + Environment.NewLine;
        //        DataTable tbl = MasterHelp.SQLquery(sql);
        //        if (tbl.Rows.Count == 0)
        //        {
        //            return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "Records not found !!" });
        //        }




        //        DataTable IR = new DataTable("mstrep");
        //        Models.PrintViewer PV = new Models.PrintViewer();
        //        HtmlConverter HC = new HtmlConverter();

        //        if (ReportType == "DETAIL" || ReportType == "SUMMARY")
        //        {
        //            HC.RepStart(IR, 2);
        //            if (RepFormat == "STANDARD") HC.GetPrintHeader(IR, "Slnm", "string", "c,25", "Jobber Name.");
        //            HC.GetPrintHeader(IR, "docdt", "string", "c,10", "Iss. Date.");
        //            HC.GetPrintHeader(IR, "docno", "string", "c,13", "Iss Doc No");
        //            HC.GetPrintHeader(IR, "itgrpnm", "string", "c,12", "Group");
        //            HC.GetPrintHeader(IR, "itnm", "string", "c,10", "Item");
        //            HC.GetPrintHeader(IR, "styleno", "string", "c,14", "Styleno");
        //            HC.GetPrintHeader(IR, "uom", "string", "c,16", "Uom");
        //            HC.GetPrintHeader(IR, "Nos", "string", "n,5", "Nos");
        //            HC.GetPrintHeader(IR, "cutlength", "double", "n,6,2", "cut length");
        //            HC.GetPrintHeader(IR, "qnty", "double", "n,15,3", "Prog.Qnty");
        //            if (showValue == true) HC.GetPrintHeader(IR, "issamt", "double", "n,15,2", "Iss Amt.");
        //            HC.GetPrintHeader(IR, "itremarks", "string", "c,15", "itremark");

        //            string rechdr = (ReportType == "SUMMARY" ? "Last " : "");
        //            HC.GetPrintHeader(IR, "recdocdt", "string", "c,10", rechdr + "Rec Date");
        //            HC.GetPrintHeader(IR, "recdocno", "string", "c,16", rechdr + "Rec No");

        //            HC.GetPrintHeader(IR, "recnos", "double", "n,5", "Rec Nos.");
        //            HC.GetPrintHeader(IR, "recqnty", "double", "n,15,3", "Rec Qnty.");

        //            HC.GetPrintHeader(IR, "balqnty", "double", "n,15,3", "Bal Qnty.");
        //            if (showValue == true) HC.GetPrintHeader(IR, "balamt", "double", "n,15,3", "Bal Value");
        //            IR.Columns.Add("slcd", typeof(string), "");
        //            string[] arrcolnm = { "Nos", "cutlength", "qnty", "issamt", "recnos", "recqnty", "balqnty", "balamt" };
        //            Int32 rNo = 0; Int32 i = 0; Int32 maxR = 0;
        //            i = 0; maxR = tbl.Rows.Count - 1;
        //            string lastslcd = "";
        //            while (i <= maxR)
        //            {
        //                string slcd = tbl.Rows[i]["slcd"].retStr();
        //                if (RepFormat == "JOBBERWISE")
        //                {
        //                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                    IR.Rows[rNo]["Dammy"] = "" + tbl.Rows[i]["slnm"].retStr() + " [" + tbl.Rows[i]["slcd"].retStr() + "]" + (tbl.Rows[i]["regmobile"].retStr() == "" ? "" : "Mob : " + tbl.Rows[i]["regmobile"].retStr());
        //                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
        //                    lastslcd = slcd;
        //                }
        //                while (tbl.Rows[i]["slcd"].retStr() == slcd)
        //                {
        //                    string autono = tbl.Rows[i]["autono"].retStr();
        //                    bool frstreco = true;
        //                    while (tbl.Rows[i]["slcd"].retStr() == slcd && tbl.Rows[i]["autono"].retStr() == autono)
        //                    {
        //                        double progslno = tbl.Rows[i]["slno"].retDbl();
        //                        double trecnos = 0, trecqnty = 0;
        //                        bool frstslnoreco = true;
        //                        while (tbl.Rows[i]["slcd"].retStr() == slcd && tbl.Rows[i]["autono"].retStr() == autono && tbl.Rows[i]["slno"].retDbl() == progslno)
        //                        {
        //                            if (frstslnoreco == true || ReportType == "DETAIL")
        //                            {
        //                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                                IR.Rows[rNo]["slcd"] = tbl.Rows[i]["slcd"].retStr();
        //                            }
        //                            if (RepFormat == "STANDARD")
        //                            {
        //                                IR.Rows[rNo]["Slnm"] = "" + tbl.Rows[i]["slnm"].retStr() + "[" + tbl.Rows[i]["slcd"].retStr() + "]";
        //                            }
        //                            if (frstreco == true || RepFormat == "STANDARD")
        //                            {
        //                                IR.Rows[rNo]["docdt"] = tbl.Rows[i]["docdt"].retDateStr();
        //                                IR.Rows[rNo]["docno"] = tbl.Rows[i]["docno"].retStr();
        //                            }
        //                            if (frstslnoreco == true)
        //                            {
        //                                IR.Rows[rNo]["itgrpnm"] = tbl.Rows[i]["itgrpnm"].retStr();
        //                                IR.Rows[rNo]["styleno"] = tbl.Rows[i]["styleno"].retStr();
        //                                IR.Rows[rNo]["itnm"] = tbl.Rows[i]["itnm"].retStr();
        //                                IR.Rows[rNo]["uom"] = tbl.Rows[i]["uomnm"].retStr();
        //                                IR.Rows[rNo]["nos"] = tbl.Rows[i]["nos"].retStr();
        //                                IR.Rows[rNo]["cutlength"] = tbl.Rows[i]["cutlength"].retDbl();
        //                                IR.Rows[rNo]["qnty"] = tbl.Rows[i]["qnty"].retDbl();
        //                                if (showValue == true) IR.Rows[rNo]["issamt"] = tbl.Rows[i]["issamt"].retDbl();
        //                                IR.Rows[rNo]["itremarks"] = tbl.Rows[i]["itremark"].retStr();
        //                            }
        //                            //Receive
        //                            if (ReportType != "SUMMARY")
        //                            {
        //                                IR.Rows[rNo]["recdocdt"] = tbl.Rows[i]["recdocdt"].retDateStr();
        //                                IR.Rows[rNo]["recdocno"] = tbl.Rows[i]["recdocno"].retStr();
        //                                IR.Rows[rNo]["recnos"] = tbl.Rows[i]["recnos"].retDbl();
        //                                IR.Rows[rNo]["recqnty"] = tbl.Rows[i]["recqnty"].retDbl();
        //                            }
        //                            trecnos = trecnos + tbl.Rows[i]["recnos"].retDbl();
        //                            trecqnty = trecqnty + tbl.Rows[i]["recqnty"].retDbl();
        //                            //
        //                            frstslnoreco = false;
        //                            frstreco = false;
        //                            i++;
        //                            if (i > maxR) break;
        //                        }
        //                        if (ReportType == "SUMMARY")
        //                        {
        //                            IR.Rows[rNo]["recdocdt"] = tbl.Rows[i - 1]["recdocdt"].retDateStr();
        //                            IR.Rows[rNo]["recdocno"] = tbl.Rows[i - 1]["recdocno"].retStr();
        //                            if (trecqnty != 0)
        //                            {
        //                                IR.Rows[rNo]["recnos"] = trecnos;
        //                                IR.Rows[rNo]["recqnty"] = trecqnty;
        //                            }
        //                        }
        //                        IR.Rows[rNo]["balqnty"] = tbl.Rows[i - 1]["balqnty"].retDbl();
        //                        double avrate = (tbl.Rows[i - 1]["qnty"].retDbl() == 0 ? 0 : (tbl.Rows[i - 1]["issamt"].retDbl() / tbl.Rows[i - 1]["qnty"].retDbl()).toRound(2));
        //                        double balamt = (avrate * tbl.Rows[i - 1]["balqnty"].retDbl()).toRound(0);
        //                        if (showValue == true) IR.Rows[rNo]["balamt"] = balamt;
        //                        if (i > maxR) break;
        //                    }
        //                    if (i > maxR) break;
        //                }
        //                if (RepFormat == "JOBBERWISE")
        //                {
        //                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                    IR.Rows[rNo]["itnm"] = "Total of " + tbl.Rows[i - 1]["slnm"].ToString();

        //                    for (int a = 9; a < IR.Columns.Count - 1; a++)
        //                    {
        //                        string colnm = IR.Columns[a].ColumnName;
        //                        if (arrcolnm.Contains(colnm))
        //                        {
        //                            var purewisegrptotal = (from DataRow dr in IR.Rows where dr["slcd"].retStr() == slcd select dr[colnm].retDbl()).Sum();
        //                            if (purewisegrptotal.retDbl() != 0)
        //                            {
        //                                IR.Rows[rNo][IR.Columns[a].ColumnName] = purewisegrptotal;
        //                            }
        //                        }
        //                    }
        //                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 1px solid;border-top: 1px solid;";
        //                }
        //                if (i > maxR) break;
        //            }

        //            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //            IR.Rows[rNo]["itnm"] = "Grand Total";
        //            for (int a = 9; a < IR.Columns.Count - 1; a++)
        //            {
        //                string colnm = IR.Columns[a].ColumnName;
        //                if (arrcolnm.Contains(colnm))
        //                {
        //                    var purewisegrptotal = (from DataRow dr in IR.Rows where dr["slcd"].retStr() != "" select dr[colnm].retDbl()).Sum();
        //                    if (purewisegrptotal.retDbl() != 0)
        //                    {
        //                        IR.Rows[rNo][IR.Columns[a].ColumnName] = purewisegrptotal;
        //                    }
        //                }
        //            }
        //            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 1px solid;border-top: 1px solid;";

        //            IR.Columns.Remove("slcd");

        //            string repname = "Job Register".retRepname();
        //            pghdr1 = (ShowPending == "PENDING" ? "Pending " : " ") + "Job Work register " + (ReportType == "SUMARRY" ? "Sumarry " : "Details ") + (recdt != "" ? " Received date: " + recdt + "" : " ") + " Jobcd:" + JOBCD + (fdt != "" ? " from " + fdt + " to " : " as on ") + tdt;
        //            PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);
        //            TempData[repname] = PV;
        //            return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
        //        }
        //        else if (ReportType == "REGISTER")
        //        {
        //            string doctype = "";
        //            if (FC.AllKeys.Contains("doctypevalue")) doctype = CommFunc.retSqlformat(FC["doctypevalue"].ToString());

        //            string issdoctype = "", recdoctype = "";
        //            if (JOBCD == "ST")
        //            {
        //                issdoctype = "OSTI"; recdoctype = "OSTR";
        //            }
        //            else if (JOBCD == "JB")
        //            {
        //                issdoctype = "OJWI"; recdoctype = "OJWR";

        //            }
        //            else if (JOBCD == "DY")
        //            {
        //                issdoctype = "ODYI"; recdoctype = "ODYR";

        //            }
        //            else if (JOBCD == "PR")
        //            {
        //                issdoctype = "OPRI"; recdoctype = "OPRR";

        //            }
        //            else if (JOBCD == "KR")
        //            {
        //                issdoctype = "OEMI"; recdoctype = "OEMR";

        //            }
        //            string sql1 = "";
        //            sql1 += "select a.autono,a.slno,c.docdt,c.docno,f.slnm,a.nos,a.qnty,c.doccd doccode,f.slarea area, " + Environment.NewLine;
        //            sql1 += "f.gstno,a.amt basicamt,0 disc1amt,0 disc2amt, " + Environment.NewLine;
        //            sql1 += "a.txblval taxableval,a.igstper,a.igstamt,a.cgstper,a.cgstamt,a.sgstper,a.sgstamt,d.tcsamt, " + Environment.NewLine;
        //            sql1 += "d.roamt roffamt,d.blamt billval,e.itnm,e.styleno||' '||e.itnm itstyle " + Environment.NewLine;
        //            sql1 += "from " + scm + ".t_txndtl a," + scm + ".t_batchdtl b," + scm + ".t_cntrl_hdr c," + scm + " " + Environment.NewLine;
        //            sql1 += ".t_txn d," + scm + ".m_sitem e," + scmf + ".m_subleg f," + scm + ".m_doctype g " + Environment.NewLine;
        //            sql1 += "where a.autono = b.autono(+) and a.slno=b.slno(+) and a.autono=c.autono(+) and c.autono=d.autono(+) " + Environment.NewLine;
        //            sql1 += "and a.itcd=e.itcd(+) and  d.slcd=f.slcd(+) and c.doccd=g.doccd(+) and " + Environment.NewLine;
        //            sql1 += "g.doctype in ('" + issdoctype + "') and d.JOBCD='" + JOBCD + "' and c.compcd = '" + COM + "' and " + Environment.NewLine;
        //            if (jobslcd != "") sql1 += "d.slcd in(" + jobslcd + ") and " + Environment.NewLine;
        //            if (itcd != "") sql1 += "a.itcd in(" + itcd + ") and " + Environment.NewLine;
        //            if (doctype != "") sql1 += "g.doctype in(" + doctype + ") and " + Environment.NewLine;
        //            if (fdt != "") sql1 += "c.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and " + Environment.NewLine;
        //            sql1 += "c.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy')  " + Environment.NewLine;
        //            sql1 += "and a.stkdrcr in ('C')  " + Environment.NewLine;
        //            sql1 += "order by c.docdt,c.docno,slnm,d.slcd,e.styleno||' '||e.itnm,a.itcd  " + Environment.NewLine;

        //            DataTable tbl1 = MasterHelp.SQLquery(sql1);
        //            //if (tbl1.Rows.Count == 0)
        //            //{
        //            //    return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "Records not found !!" });
        //            //}

        //            string sql2 = "";
        //            sql2 += "select a.autono,a.slno,c.docdt,c.docno,f.slnm,a.nos,a.qnty,c.doccd doccode, " + Environment.NewLine;
        //            sql2 += "f.slarea area,f.gstno,a.amt basicamt,0 disc1amt,0 disc2amt, " + Environment.NewLine;
        //            sql2 += "a.txblval taxableval,a.igstper,a.igstamt,a.cgstper,a.cgstamt,a.sgstper,a.sgstamt, " + Environment.NewLine;
        //            sql2 += "d.tcsamt,d.roamt roffamt,d.blamt billval,b.recprogautono,b.recprogslno,e.itnm,e.styleno||' '||e.itnm itstyle " + Environment.NewLine;
        //            sql2 += "from " + scm + ".t_txndtl a," + scm + ".t_batchdtl b," + scm + ".t_cntrl_hdr c," + scm + " " + Environment.NewLine;
        //            sql2 += ".t_txn d," + scm + ".m_sitem e," + scmf + ".m_subleg f ," + scm + ".m_doctype g " + Environment.NewLine;
        //            sql2 += "where a.autono = b.autono(+) and a.slno=b.slno(+) and a.autono=c.autono(+) " + Environment.NewLine;
        //            sql2 += "and c.autono=d.autono(+) and a.itcd=e.itcd(+) and  d.slcd=f.slcd(+) and c.doccd=g.doccd(+) " + Environment.NewLine;
        //            sql2 += "and g.doctype in ('" + recdoctype + "') and d.JOBCD='" + JOBCD + "' and c.compcd = '" + COM + "' and " + Environment.NewLine;
        //            if (jobslcd != "") sql2 += "d.slcd in(" + jobslcd + ") and " + Environment.NewLine;
        //            if (itcd != "") sql2 += "a.itcd in(" + itcd + ") and " + Environment.NewLine;
        //            if (doctype != "") sql2 += "g.doctype in(" + doctype + ") and " + Environment.NewLine;
        //            if (fdt != "") sql2 += "c.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and " + Environment.NewLine;
        //            sql2 += "c.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy')  " + Environment.NewLine;
        //            sql2 += "and a.stkdrcr in ('D')  " + Environment.NewLine;
        //            sql2 += "order by c.docdt,c.docno,slnm,d.slcd,e.styleno||' '||e.itnm,a.itcd  " + Environment.NewLine;

        //            DataTable tbl2 = MasterHelp.SQLquery(sql2);
        //            //if (tbl2.Rows.Count == 0)
        //            //{
        //            //    return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "Records not found !!" });
        //            //}


        //            HC.RepStart(IR, 3);

        //            HC.GetPrintHeader(IR, "doccd", "string", "c,5", "Doc;Code");
        //            HC.GetPrintHeader(IR, "docdt", "string", "d,10:dd/mm/yy", ";Doc Date");
        //            HC.GetPrintHeader(IR, "docno", "string", "c,18", ";Doc No");
        //            HC.GetPrintHeader(IR, "slnm", "string", "c,20", "Party;Name");
        //            HC.GetPrintHeader(IR, "slarea", "string", "c,10", "Area");
        //            HC.GetPrintHeader(IR, "gstno", "string", "c,15", "GST No.");
        //            HC.GetPrintHeader(IR, "itnm", "string", "c,35", "Item;Name");
        //            HC.GetPrintHeader(IR, "nos", "double", "n,5", "Nos");
        //            HC.GetPrintHeader(IR, "qnty", "double", "n,12,3", "Qnty");
        //            HC.GetPrintHeader(IR, "amt", "double", "n,12,2", "Basic;Amount");
        //            HC.GetPrintHeader(IR, "taxableval", "double", "n,12,2", "Taxable;Value");
        //            HC.GetPrintHeader(IR, "igstper", "double", "n,5,3", "IGST;%");
        //            HC.GetPrintHeader(IR, "igstamt", "double", "n,10,2", "IGST;Amt");
        //            HC.GetPrintHeader(IR, "cgstper", "double", "n,5,3", "CGST;%");
        //            HC.GetPrintHeader(IR, "cgstamt", "double", "n,10,2", "CGST;Amt");
        //            HC.GetPrintHeader(IR, "sgstper", "double", "n,5,3", "SGST;%");
        //            HC.GetPrintHeader(IR, "sgstamt", "double", "n,10,2", "SGST;Amt");
        //            HC.GetPrintHeader(IR, "tcsamt", "double", "n,10,2", "TCS;Amt");
        //            HC.GetPrintHeader(IR, "roamt", "double", "n,6,2", "R/Off;Amt");
        //            HC.GetPrintHeader(IR, "blamt", "double", "n,12,2", ";Bill Value");

        //            DataTable maintbl = new DataTable();
        //            //if iss or both then 
        //            if (VE.TEXTBOX10 == "Issue" || VE.TEXTBOX10 == "Both")
        //                maintbl = tbl1;
        //            else
        //                maintbl = tbl2;

        //            Int32 rNo = 0; Int32 i = 0; Int32 maxR = 0;
        //            i = 0; maxR = maintbl.Rows.Count - 1;
        //            while (i <= maxR)
        //            {
        //                //IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                //IR.Rows[rNo]["Dammy"] = " ";
        //                //IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:12px;border-top: 1px solid;border-bottom: 1px solid;";

        //                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                IR.Rows[rNo]["doccd"] = maintbl.Rows[i]["doccode"].retStr();
        //                IR.Rows[rNo]["docdt"] = maintbl.Rows[i]["docdt"].retDateStr();
        //                IR.Rows[rNo]["docno"] = maintbl.Rows[i]["docno"].retStr();
        //                IR.Rows[rNo]["slnm"] = maintbl.Rows[i]["slnm"].retStr();
        //                IR.Rows[rNo]["slarea"] = maintbl.Rows[i]["area"].retStr();
        //                IR.Rows[rNo]["gstno"] = maintbl.Rows[i]["gstno"].retStr();
        //                IR.Rows[rNo]["itnm"] = maintbl.Rows[i]["itstyle"].retStr();
        //                IR.Rows[rNo]["nos"] = maintbl.Rows[i]["nos"].retDbl();
        //                IR.Rows[rNo]["qnty"] = maintbl.Rows[i]["qnty"].retDbl();
        //                IR.Rows[rNo]["amt"] = maintbl.Rows[i]["basicamt"].retDbl();
        //                IR.Rows[rNo]["taxableval"] = maintbl.Rows[i]["taxableval"].retDbl();
        //                IR.Rows[rNo]["igstper"] = maintbl.Rows[i]["igstper"].retDbl();
        //                IR.Rows[rNo]["igstamt"] = maintbl.Rows[i]["igstamt"].retDbl();
        //                IR.Rows[rNo]["cgstper"] = maintbl.Rows[i]["cgstper"].retDbl();
        //                IR.Rows[rNo]["cgstamt"] = maintbl.Rows[i]["cgstamt"].retDbl();
        //                IR.Rows[rNo]["sgstper"] = maintbl.Rows[i]["sgstper"].retDbl();
        //                IR.Rows[rNo]["sgstamt"] = maintbl.Rows[i]["sgstamt"].retDbl();
        //                IR.Rows[rNo]["tcsamt"] = maintbl.Rows[i]["tcsamt"].retDbl();
        //                IR.Rows[rNo]["roamt"] = maintbl.Rows[i]["roffamt"].retDbl();
        //                IR.Rows[rNo]["blamt"] = maintbl.Rows[i]["billval"].retDbl();

        //                string issautono = maintbl.Rows[i]["autono"].ToString();
        //                Int32 issslno = maintbl.Rows[i]["slno"].retInt();

        //                var st1 = tbl2.Select("recprogautono ='" + issautono + "' and recprogslno='" + issslno + "' ");

        //                //if not both thn skip this part
        //                if (VE.TEXTBOX10 == "Both")
        //                {
        //                    if (st1 != null && st1.Count() > 0)
        //                    {
        //                        DataTable innertable1 = st1.CopyToDataTable();
        //                        for (int j = 0; j <= innertable1.Rows.Count - 1; j++)
        //                        {
        //                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                            IR.Rows[rNo]["doccd"] = innertable1.Rows[j]["doccode"].retStr();
        //                            IR.Rows[rNo]["docdt"] = innertable1.Rows[j]["docdt"].retDateStr();
        //                            IR.Rows[rNo]["docno"] = innertable1.Rows[j]["docno"].retStr();
        //                            IR.Rows[rNo]["slnm"] = innertable1.Rows[j]["slnm"].retStr();
        //                            IR.Rows[rNo]["slarea"] = innertable1.Rows[j]["area"].retStr();
        //                            IR.Rows[rNo]["gstno"] = innertable1.Rows[j]["gstno"].retStr();
        //                            IR.Rows[rNo]["itnm"] = innertable1.Rows[j]["itstyle"].retStr();
        //                            IR.Rows[rNo]["nos"] = innertable1.Rows[j]["nos"].retDbl();
        //                            IR.Rows[rNo]["qnty"] = innertable1.Rows[j]["qnty"].retDbl();
        //                            IR.Rows[rNo]["amt"] = innertable1.Rows[j]["basicamt"].retDbl();
        //                            IR.Rows[rNo]["taxableval"] = innertable1.Rows[j]["taxableval"].retDbl();
        //                            IR.Rows[rNo]["igstper"] = innertable1.Rows[j]["igstper"].retDbl();
        //                            IR.Rows[rNo]["igstamt"] = innertable1.Rows[j]["igstamt"].retDbl();
        //                            IR.Rows[rNo]["cgstper"] = innertable1.Rows[j]["cgstper"].retDbl();
        //                            IR.Rows[rNo]["cgstamt"] = innertable1.Rows[j]["cgstamt"].retDbl();
        //                            IR.Rows[rNo]["sgstper"] = innertable1.Rows[j]["sgstper"].retDbl();
        //                            IR.Rows[rNo]["sgstamt"] = innertable1.Rows[j]["sgstamt"].retDbl();
        //                            IR.Rows[rNo]["tcsamt"] = innertable1.Rows[j]["tcsamt"].retDbl();
        //                            IR.Rows[rNo]["roamt"] = innertable1.Rows[j]["roffamt"].retDbl();
        //                            IR.Rows[rNo]["blamt"] = innertable1.Rows[j]["billval"].retDbl();

        //                        }
        //                    }
        //                }


        //                i++;
        //                if (i > maxR) break;
        //            }

        //            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //            IR.Rows[rNo]["docno"] = "Grand Total";
        //            for (int a = 10; a < IR.Columns.Count; a++)
        //            {
        //                string colnm = IR.Columns[a].ColumnName;
        //                var purewisegrptotal = (from DataRow dr in IR.Rows where dr["docno"].retStr() != "" select dr[colnm].retDbl()).Sum();
        //                if (purewisegrptotal.retDbl() != 0)
        //                {
        //                    IR.Rows[rNo][IR.Columns[a].ColumnName] = purewisegrptotal;
        //                }
        //            }
        //            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 1px solid;border-top: 1px solid;";

        //            string repname = "Job Register".retRepname();
        //            pghdr1 = "Register " + " Jobcd:" + JOBCD + (fdt != "" ? " from " + fdt + " to " : " as on ") + tdt;
        //            PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);
        //            TempData[repname] = PV;
        //            return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
        //        }
        //        else
        //        {
        //            HC.RepStart(IR, 2);
        //            HC.GetPrintHeader(IR, "itgrpnm", "string", "c,12", "Group");
        //            HC.GetPrintHeader(IR, "itnm", "string", "c,20", "Item Description");
        //            HC.GetPrintHeader(IR, "uom", "string", "c,10", "Uom");
        //            HC.GetPrintHeader(IR, "balqnty", "double", "n,15,3", "Bal Qnty.");
        //            if (showValue == true) HC.GetPrintHeader(IR, "balamt", "double", "n,15,3", "Bal Value");
        //            IR.Columns.Add("slcd", typeof(string), "");

        //            if (RepFormat == "JOBBERWISE")
        //            {
        //                DataView dv = new DataView(tbl);
        //                dv.Sort = "slcd,slnm,itcd,itnm ASC";
        //                tbl = dv.ToTable();
        //            }
        //            else
        //            {
        //                DataView dv = new DataView(tbl);
        //                dv.Sort = "itcd,itnm ASC";
        //                tbl = dv.ToTable();
        //            }

        //            #region Stock printing
        //            Int32 rNo = 0; Int32 i = 0; Int32 maxR = 0;
        //            i = 0; maxR = tbl.Rows.Count - 1;
        //            string fld1 = RepFormat == "JOBBERWISE" ? "slcd" : "itcd";
        //            while (i <= maxR)
        //            {
        //                string val1 = tbl.Rows[i][fld1].retStr();
        //                if (RepFormat == "JOBBERWISE")
        //                {
        //                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                    IR.Rows[rNo]["Dammy"] = "" + tbl.Rows[i]["slnm"].retStr() + " [" + tbl.Rows[i]["slcd"].retStr() + "]" + (tbl.Rows[i]["regmobile"].retStr() == "" ? "" : "Mob : " + tbl.Rows[i]["regmobile"].retStr());
        //                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
        //                }
        //                while (tbl.Rows[i][fld1].retStr() == val1)
        //                {
        //                    string itcd1 = tbl.Rows[i]["itcd"].retStr();
        //                    double balqnty = 0, balamt = 0;
        //                    while (tbl.Rows[i][fld1].retStr() == val1 && tbl.Rows[i]["itcd"].retStr() == itcd1)
        //                    {
        //                        string autoslno = tbl.Rows[i]["autono"].retStr() + tbl.Rows[i]["slno"].retStr();
        //                        while (tbl.Rows[i][fld1].retStr() == val1 && tbl.Rows[i]["itcd"].retStr() == itcd1 && tbl.Rows[i]["autono"].retStr() + tbl.Rows[i]["slno"].retStr() == autoslno)
        //                        {
        //                            i++;
        //                            if (i > maxR) break;
        //                        }
        //                        balqnty += tbl.Rows[i - 1]["balqnty"].retDbl();
        //                        double avrate = (tbl.Rows[i - 1]["qnty"].retDbl() == 0 ? 0 : (tbl.Rows[i - 1]["issamt"].retDbl() / tbl.Rows[i - 1]["qnty"].retDbl()).toRound(2));
        //                        balamt += (avrate * tbl.Rows[i - 1]["balqnty"].retDbl()).toRound(0);
        //                        //i++;
        //                        if (i > maxR) break;
        //                    }
        //                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                    IR.Rows[rNo]["itgrpnm"] = tbl.Rows[i - 1]["itgrpnm"].retStr();
        //                    IR.Rows[rNo]["itnm"] = tbl.Rows[i - 1]["fabitnm"].retStr() + " " + tbl.Rows[i - 1]["itnm"].retStr();
        //                    IR.Rows[rNo]["uom"] = tbl.Rows[i - 1]["uomnm"].retStr();
        //                    IR.Rows[rNo]["slcd"] = tbl.Rows[i - 1]["slcd"].retStr();
        //                    IR.Rows[rNo]["balqnty"] = balqnty;
        //                    if (showValue == true) IR.Rows[rNo]["balamt"] = balamt;
        //                    if (i > maxR) break;
        //                }
        //                if (RepFormat == "JOBBERWISE")
        //                {
        //                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
        //                    IR.Rows[rNo]["itgrpnm"] = "Total of " + tbl.Rows[i - 1]["slnm"].ToString();

        //                    for (int a = 6; a < IR.Columns.Count - 1; a++)
        //                    {
        //                        string colnm = IR.Columns[a].ColumnName;
        //                        var purewisegrptotal = (from DataRow dr in IR.Rows where dr["slcd"].retStr() == val1 select dr[colnm].retDbl()).Sum();
        //                        if (purewisegrptotal.retDbl() != 0)
        //                        {
        //                            IR.Rows[rNo][IR.Columns[a].ColumnName] = purewisegrptotal;
        //                        }
        //                    }
        //                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 1px solid;border-top: 1px solid;";

        //                }
        //                if (i > maxR) break;
        //            }
        //            #endregion
        //            IR.Columns.Remove("slcd");

        //            string repname = "Job Register".retRepname();
        //            pghdr1 = (ShowPending == "PENDING" ? "Pending " : " ") + "Job Work register " + (ReportType == "SUMARRY" ? "Sumarry " : "Details ") + (recdt != "" ? " Received date: " + recdt + "" : " ") + " Jobcd:" + JOBCD + (fdt != "" ? " from " + fdt + " to " : " as on ") + tdt;
        //            PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);
        //            TempData[repname] = PV;
        //            return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Cn.SaveException(ex, "");
        //        return Content(ex.Message);
        //    }
        //}
        public ActionResult Getdoctype(FormCollection FC, ReportViewinHtml VE)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            string scm = CommVar.CurSchema(UNQSNO);
            try
            {
                string doctype = "";
                if (VE.JOBCD == "ST")
                {
                    doctype = "'OSTI','OSTR'";

                }
                else if (VE.JOBCD == "JB")
                {
                    doctype = "'OJWI','OJWR'";

                }
                else if (VE.JOBCD == "DY")
                {
                    doctype = "'ODYI','ODYR'";

                }
                else if (VE.JOBCD == "PR")
                {
                    doctype = "'OPRI','OPRR'";

                }
                else if (VE.JOBCD == "KR")
                {
                    doctype = "'OEMI','OEMR'";

                }
                string sql = "";
                sql += "select a.doccd, a.doctype, a.docnm " + Environment.NewLine;
                sql += "from " + scm + ".m_doctype a " + Environment.NewLine;
                sql += "where a.doctype in(" + doctype + ") " + Environment.NewLine;

                DataTable tbl = MasterHelp.SQLquery(sql);
                if (tbl != null && tbl.Rows.Count > 0)
                {
                    VE.DropDown_list = (from DataRow dr in tbl.Rows
                                        select new DropDown_list()
                                        {
                                            text = dr["doctype"].retStr(),
                                            value = dr["docnm"].retStr()
                                        }).OrderBy(A => A.text).ToList();
                }
                else
                {
                    VE.DropDown_list = new List<DropDown_list>();
                }
                var doctp = MasterHelp.ComboFill("doctype", VE.DropDown_list, 0, 0);
                ModelState.Clear();
                VE.DefaultView = true;
                //dic.Add("DropDown_list_WORKNO", Workno);
                //dic.Add("message", "ok");
                return Content("ok" + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + doctp);
            }
            catch (Exception ex)
            {
                dic.Add("message", ex.Message + ex.InnerException);
                Cn.SaveException(ex, "");
            }
            return Json(dic, JsonRequestBehavior.AllowGet);
        }
    }
}