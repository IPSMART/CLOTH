
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

                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("J");
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);

                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);
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
                string jobslcd = "", itcd = "";
                string JOBCD = VE.JOBCD;
                if (FC.AllKeys.Contains("slcdvalue")) jobslcd = CommFunc.retSqlformat(FC["slcdvalue"].ToString());
                if (FC.AllKeys.Contains("itcdvalue")) itcd = CommFunc.retSqlformat(FC["itcdvalue"].ToString());


                string sql = "";
                sql += "select a.autono, c.slcd, a.slno, c.nos, c.qnty, c.cutlength, i.itnm, i.styleno,h.ourdesign, h.pdesign, i.itgrpcd, j.itgrpnm,k.uomnm, ";
                sql += "c.itremark, c.sample, g.slnm, g.regmobile, ptch.docno , ptch.docdt, y.issamt, ";
                sql += "b.autono recautono, rtch.docno recdocno, rtch.docdt recdocdt, b.prefno, b.prefdt, b.doctag,b.nos recnos, b.qnty recqnty, ";
                sql += "c.nos, c.qnty, c.nos-nvl(z.nos,0) balnos, c.qnty-nvl(z.qnty,0) balqnty,h.itcd from ";

                sql += "(select a.autono, a.slno, a.autono || a.slno autoslno ";
                sql += "from " + scm + ".t_progmast a, " + scm + ".t_cntrl_hdr b ";
                sql += "where a.autono = b.autono(+) and ";
                sql += "b.compcd = '" + COM + "' and nvl(b.cancel, 'N') = 'N' and ";
                if (jobslcd != "") sql += "a.slcd in(" + jobslcd + ") and ";
                if (itcd != "") sql += "a.slcd in(" + itcd + ") and ";
                if (fdt != "") sql += "b.docdt >= to_date('" + fdt + "', 'dd/mm/yyyy') and ";
                sql += "b.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') ) a, ";

                sql += "(select a.progautono||a.progslno progautoslno,  ";
                sql += "a.nos, a.qnty, a.autono, a.rate, c.prefno, c.prefdt, c.doctag ";
                sql += "from " + scm + ".t_progdtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_txn c ";
                sql += "where a.autono = b.autono(+) and a.autono = c.autono(+)  and a.autono<>a.progautono and ";
                sql += "b.compcd = '" + COM + "' and nvl(b.cancel, 'N')= 'N' and  C.JOBCD='" + JOBCD + "' and  ";
                sql += "b.docdt <= to_date('" + (recdt == "" ? tdt : recdt) + "', 'dd/mm/yyyy') ) b, ";

                sql += "(select a.progautono||a.progslno progautoslno, sum(round(a.qnty * a.rate, 2)) issamt ";
                sql += "from " + scm + ".t_kardtl a, " + scm + ".t_cntrl_hdr b ";
                sql += "where a.autono = b.autono(+) and ";
                sql += "b.compcd = '" + COM + "' and nvl(b.cancel, 'N')= 'N' and ";
                sql += "b.docdt <= to_date('" + tdt + "', 'dd/mm/yyyy') ";
                sql += "group by a.progautono || a.progslno ) y, ";

                sql += "(select a.progautono||a.progslno progautoslno,  ";
                sql += "sum(a.nos) nos, sum(a.qnty) qnty ";
                sql += "from " + scm + ".t_progdtl a, " + scm + ".t_cntrl_hdr b, " + scm + ".t_txn c ";
                sql += "where a.autono = b.autono(+) and a.autono = c.autono(+)  and a.autono<>a.progautono and ";
                sql += "b.compcd = '" + COM + "' and nvl(b.cancel, 'N')= 'N' and  C.JOBCD='" + JOBCD + "' and  ";
                sql += "b.docdt <= to_date('" + (recdt == "" ? tdt : recdt) + "', 'dd/mm/yyyy') ";
                sql += "group by a.progautono||a.progslno ) z, ";

                sql += scm + ".t_progmast c, " + scm + ".t_cntrl_hdr ptch, " + scm + ".t_cntrl_hdr rtch, ";
                sql += scmf + ".m_subleg g, " + scm + ".t_batchmst h, " + scm + ".m_sitem i, " + scm + ".m_group j, " + scmf + ".m_uom k ";
                sql += "where a.autono=c.autono(+) and a.slno=c.slno(+) and a.autoslno = b.progautoslno(+) and a.autoslno = y.progautoslno(+) and a.autoslno = z.progautoslno(+) and ";
                sql += "a.autono = ptch.autono(+) and b.autono = rtch.autono(+) and C.JOBCD='" + JOBCD + "' and ";
                if (ShowPending == "PENDING") sql += "c.qnty-nvl(z.qnty,0) <> 0 and ";
                sql += "c.slcd = g.slcd(+) and c.barno = h.barno(+) and h.itcd = i.itcd(+) and i.itgrpcd = j.itgrpcd(+) and i.uomcd = k.uomcd(+) ";
                sql += "order by slnm, slcd, docdt, docno, autono, slno, recdocdt, recdocno ";
                DataTable tbl = MasterHelp.SQLquery(sql);
                if (tbl.Rows.Count == 0)
                {
                    return RedirectToAction("NoRecords", "RPTViewer", new { errmsg = "Records not found !!" });
                }

                DataTable IR = new DataTable("mstrep");
                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                if (ReportType == "DETAIL" || ReportType == "SUMMARY")
                {
                    HC.RepStart(IR, 2);
                    if (RepFormat == "STANDARD") HC.GetPrintHeader(IR, "Slnm", "string", "c,25", "Jobber Name.");
                    HC.GetPrintHeader(IR, "docdt", "string", "c,10", "Iss. Date.");
                    HC.GetPrintHeader(IR, "docno", "string", "c,13", "Iss Doc No");
                    HC.GetPrintHeader(IR, "itgrpnm", "string", "c,12", "Group");
                    HC.GetPrintHeader(IR, "itnm", "string", "c,10", "Item");
                    HC.GetPrintHeader(IR, "styleno", "string", "c,14", "Styleno");
                    HC.GetPrintHeader(IR, "uom", "string", "c,16", "Uom");
                    HC.GetPrintHeader(IR, "Nos", "string", "n,5", "Nos");
                    HC.GetPrintHeader(IR, "cutlength", "double", "n,6,2", "cut length");
                    HC.GetPrintHeader(IR, "qnty", "double", "n,15,3", "Prog.Qnty");
                    if (showValue == true) HC.GetPrintHeader(IR, "issamt", "double", "n,15,2", "Iss Amt.");
                    HC.GetPrintHeader(IR, "itremarks", "string", "c,15", "itremark");

                    string rechdr = (ReportType == "SUMMARY" ? "Last " : "");
                    HC.GetPrintHeader(IR, "recdocdt", "string", "c,10", rechdr + "Rec Date");
                    HC.GetPrintHeader(IR, "recdocno", "string", "c,16", rechdr + "Rec No");

                    HC.GetPrintHeader(IR, "recnos", "double", "n,5", "Rec Nos.");
                    HC.GetPrintHeader(IR, "recqnty", "double", "n,15,3", "Rec Qnty.");

                    HC.GetPrintHeader(IR, "balqnty", "double", "n,15,3", "Bal Qnty.");
                    if (showValue == true) HC.GetPrintHeader(IR, "balamt", "double", "n,15,3", "Bal Value");
                    IR.Columns.Add("slcd", typeof(string), "");
                    string[] arrcolnm = { "Nos", "cutlength", "qnty", "issamt", "recnos", "recqnty", "balqnty", "balamt" };
                    Int32 rNo = 0; Int32 i = 0; Int32 maxR = 0;
                    i = 0; maxR = tbl.Rows.Count - 1;
                    string lastslcd = "";
                    while (i <= maxR)
                    {
                        string slcd = tbl.Rows[i]["slcd"].retStr();
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
                                    if (i > maxR) break;
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
                                IR.Rows[rNo]["balqnty"] = tbl.Rows[i - 1]["balqnty"].retDbl();
                                double avrate = (tbl.Rows[i - 1]["qnty"].retDbl() == 0 ? 0 : (tbl.Rows[i - 1]["issamt"].retDbl() / tbl.Rows[i - 1]["qnty"].retDbl()).toRound(2));
                                double balamt = (avrate * tbl.Rows[i - 1]["balqnty"].retDbl()).toRound(0);
                                if (showValue == true) IR.Rows[rNo]["balamt"] = balamt;
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
                }
                else
                {
                    HC.RepStart(IR, 2);
                    HC.GetPrintHeader(IR, "itgrpnm", "string", "c,12", "Group");
                    HC.GetPrintHeader(IR, "itstyle", "string", "c,10", "Item Description");
                    HC.GetPrintHeader(IR, "uom", "string", "c,16", "Uom");
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
                                balqnty += tbl.Rows[i]["balqnty"].retDbl();
                                double avrate = (tbl.Rows[i]["qnty"].retDbl() == 0 ? 0 : (tbl.Rows[i]["issamt"].retDbl() / tbl.Rows[i]["qnty"].retDbl()).toRound(2));
                                balamt += (avrate * tbl.Rows[i]["balqnty"].retDbl()).toRound(0);
                                i++;
                                if (i > maxR) break;
                            }
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["itgrpnm"] = tbl.Rows[i - 1]["itgrpnm"].retStr();
                            IR.Rows[rNo]["itstyle"] = tbl.Rows[i - 1]["itnm"].retStr() + " " + tbl.Rows[i - 1]["styleno"].retStr();
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
                }
                string repname = "Job Register".retRepname();
                pghdr1 = (ShowPending == "PENDING" ? "Pending " : " ") + "Job Work register " + (ReportType == "SUMARRY" ? "Sumarry " : "Details ") + (recdt != "" ? " Received date: " + recdt + "" : " ") + " Jobcd:" + JOBCD + (fdt != "" ? " from " + fdt + " to " : " as on ") + tdt;
                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);
                TempData[repname] = PV;
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