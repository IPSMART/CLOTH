using System;
using System.Linq;
using System.Data;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;

namespace Improvar.Controllers
{
    public class Rep_Pend_JobController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp MasterHelp = new MasterHelp();
        Salesfunc Salesfunc = new Salesfunc();
        DropDownHelp DropDownHelp = new DropDownHelp();
        string jobcd = "", jobnm = "";
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Rep_Pend_Job
        public ActionResult Rep_Pend_Job()
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ReportViewinHtml VE = new ReportViewinHtml();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    jobcd = VE.MENU_PARA;
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    VE.FDT = CommVar.FinStartDate(UNQSNO);
                    VE.TDT = CommVar.CurrDate(UNQSNO);
                    VE.Checkbox1 = false; //show summary
                    VE.Checkbox2 = true; //Show Party
                    VE.Checkbox4 = false; //Show Barch No
                    VE.TEXTBOX2 = "P"; //Calc on Box/Pcs/Sets;
                    string comcd = CommVar.Compcd(UNQSNO);
                    string location = CommVar.Loccd(UNQSNO);
                    //jobcd = VE.MENU_PARA;
                    jobnm = DB.M_JOBMST.Find(jobcd)?.JOBNM;
                    VE.JOBCD = jobcd;
                    VE.JOBNM = jobnm;
                    //if (jobcd == "DY" || jobcd == "BL") VE.Checkbox4 = true;
                    ViewBag.formname = "Pending with " + jobnm;

                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = MasterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);

                    VE.DropDown_list_ITGRP = DropDownHelp.GetItgrpcdforSelection();
                    VE.Itgrpnm = MasterHelp.ComboFill("itgrpcd", VE.DropDown_list_ITGRP, 0, 1);

                    VE.DropDown_list_BRAND = DropDownHelp.GetBrandcddforSelection();
                    VE.Brandnm = MasterHelp.ComboFill("brandcd", VE.DropDown_list_BRAND, 0, 1);

                    VE.DropDown_list_SLCD = DropDownHelp.GetSlcdforSelection("J");
                    VE.Slnm = MasterHelp.ComboFill("slcd", VE.DropDown_list_SLCD, 0, 1);

                    VE.DropDown_list_LINECD = DropDownHelp.GetLinecdforSelection();
                    VE.Linenm = MasterHelp.ComboFill("linecd", VE.DropDown_list_LINECD, 0, 1);

                    VE.DropDown_list_StkType = Salesfunc.GetforStkTypeSelection();

                    string COM = CommVar.Compcd(UNQSNO), LOC = CommVar.Loccd(UNQSNO), linkcode = "J";
                    VE.TEXTBOX4 = MasterHelp.ComboFill("recslcd", VE.DropDown_list_SLCD, 0, 1);

                    //VE.DropDown_list4 = (from a in DBF.M_SUBLEG
                    //          join b in DBF.M_SUBLEG_LINK on a.SLCD equals b.SLCD into x
                    //          from b in x.DefaultIfEmpty()
                    //          join i in DBF.M_CNTRL_HDR on a.M_AUTONO equals i.M_AUTONO
                    //          join c in DBF.M_CNTRL_LOCA on a.M_AUTONO equals c.M_AUTONO into g
                    //          from c in g.DefaultIfEmpty()
                    //          where (linkcode.Contains(b.LINKCD) && c.COMPCD == COM && c.LOCCD == LOC && i.INACTIVE_TAG == "N" || linkcode.Contains(b.LINKCD) && c.COMPCD == null && c.LOCCD == null && i.INACTIVE_TAG == "N")
                    //          select new DropDown_list4()
                    //          {
                    //              text = a.SLNM,
                    //              value = a.SLCD
                    //          }).ToList(); 


                    VE.DefaultView = true;
                    VE.ExitMode = 1;
                    VE.DefaultDay = 0;
                    if (VE.MENU_PARA == "IR") VE.Checkbox3 = true;
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
        public ActionResult GetJobDetails(string val)
        {
            try
            {
                var str = MasterHelp.JOBCD_JOBMST_help(val, "");
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
        public ActionResult Rep_Pend_Job(ReportViewinHtml VE, FormCollection FC)
        {
            string repon = FC["Show_exl"];
            if (VE.TEXTBOX3 == "S" && VE.Checkbox5 == true)
            {
                return Rep_Pend_Job_Item(VE, FC, repon);
            }
            else
            {
                return Rep_Pend_Job_Dtl(VE, FC);

            }
            //return Content("");
        }
        public ActionResult Rep_Pend_Job_Dtl(ReportViewinHtml VE, FormCollection FC)
        {
            string ModuleCode = Module.Module_Code;
            try
            {
                Cn.getQueryString(VE);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string slcd = "", linecd = "", itcd = "", itgrpcd = "", fdt, tdt = "", stktype = "", brandcd = "", unselslcd = "", recslcd = "";
                fdt = VE.FDT;
                tdt = VE.TDT;
                jobcd = VE.JOBCD;// VE.MENU_PARA;
                jobnm = VE.JOBNM;// DB.M_JOBMST.Find(jobcd).JOBNM;
                bool showitem = false, showparty = VE.Checkbox2;
                bool showsumm = VE.TEXTBOX3 == "S" ? true : false;//VE.Checkbox1;
                if (VE.MENU_PARA == "IR")
                {
                    if (FC.AllKeys.Contains("RECPARTY"))
                    {
                        recslcd = FC["RECPARTY"].ToString().retSqlformat();
                    }
                }
                if (FC.AllKeys.Contains("slcdvalue")) slcd = FC["slcdvalue"].ToString().retSqlformat();
                if (FC.AllKeys.Contains("slcdunselvalue")) unselslcd = FC["slcdunselvalue"].ToString().retSqlformat();
                if (FC.AllKeys.Contains("recslcdvalue")) recslcd = FC["recslcdvalue"].ToString().retSqlformat();

                if (FC.AllKeys.Contains("linecdvalue")) linecd = FC["linecdvalue"].ToString().retSqlformat();
                if (FC.AllKeys.Contains("brandcdvalue"))
                {
                    brandcd = FC["brandcdvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("itgrpcdvalue"))
                {
                    itgrpcd = FC["itgrpcdvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("itcdvalue"))
                {
                    itcd = FC["itcdvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("STKTYPE"))
                {
                    stktype = FC["STKTYPE"].ToString().retSqlformat();
                }

                DataTable tbl = Salesfunc.getPendProg(tdt, tdt, slcd, itcd, "'" + jobcd + "'", "", "", linecd);
                //if (VE.Checkbox4 == true) tbl.DefaultView.Sort = "slnm, slcd, linenm, linecd, docdt, docno, batchno, styleno, itnm, itcd, partnm, partcd, print_seq, sizenm";
                //else tbl.DefaultView.Sort = "slnm, slcd, linenm, linecd, docdt, docno, styleno, itnm, itcd, partnm, partcd, print_seq, sizenm";
                if (VE.Checkbox4 == true) tbl.DefaultView.Sort = "slnm, slcd, docdt, docno, batchno, styleno, itnm, itcd, print_seq, sizenm";
                else tbl.DefaultView.Sort = "slnm, slcd, docdt, docno, styleno, itnm, itcd, print_seq, sizenm";
                tbl = tbl.DefaultView.ToTable();
                if (tbl.Rows.Count == 0)
                {
                    return Content("No Record Found");
                }
                if (tbl.Rows.Count == 0)
                {
                    return Content("No Record Found");
                }

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                string qtydsp = "n,13,0:##,##,##,##0", qtydspo = "n,13,2:###,##,##0.00";
                string stkcalcon = "P", qty1hd = "Box";
                if (stkcalcon == "S") qty1hd = "Sets";
                bool groupondate = false, showothrunit = true, showraka = true;
                if (showitem == false && showparty == false) groupondate = true;
                if (stkcalcon == "P") showothrunit = false;
                if (jobcd == "CT" || jobcd == "DY" || jobcd == "BL" || jobcd == "KT" || jobcd == "FP" || jobcd == "YD") qtydsp = "n,13,3:##,##,##0.000";
                if (jobcd == "KT" || jobcd == "DY" || jobcd == "BL" || jobcd == "FP") showraka = false;

                DataTable IR = new DataTable("mstrep");
                HC.RepStart(IR, 3);
                if (showsumm == true)
                {
                    HC.GetPrintHeader(IR, "docdt", "string", "n,3", "Sl");
                    HC.GetPrintHeader(IR, "docno", "string", "c,10", "Code");
                    HC.GetPrintHeader(IR, "dsc", "string", "c,40", "Party Name");
                }
                else
                {
                    HC.GetPrintHeader(IR, "docdt", "string", "c,10", "Date");
                    HC.GetPrintHeader(IR, "docno", "string", "c,16", "Ref No");
                    HC.GetPrintHeader(IR, "dsc", "string", "c,40", "particulars");
                }
                if (VE.Checkbox6 == true) HC.GetPrintHeader(IR, "ordno", "string", "c,15", "Order No");
                if (VE.Checkbox4 == true) HC.GetPrintHeader(IR, "batchno", "string", "c,15", "Batch No");
                if (VE.Checkbox3 == true) HC.GetPrintHeader(IR, "recdocno", "string", "c,16", "Recd Ref");
                if (VE.Checkbox3 == true) HC.GetPrintHeader(IR, "recslnm", "string", "c,20", "Recd from");
                HC.GetPrintHeader(IR, "issqnty", "double", qtydsp, "Bal.Qty");
                if (showothrunit == true) HC.GetPrintHeader(IR, "issqnty1", "double", qtydspo, "Iss." + qty1hd);

                //if (showraka == true)
                //{
                //    HC.GetPrintHeader(IR, "losqnty", "double", qtydsp, "Loose");
                //    HC.GetPrintHeader(IR, "rakqnty", "double", qtydsp, "Raka");
                //}

                double gopqty = 0, gissqty = 0, grecqty = 0, gretqty = 0, gshrqty = 0, gbalqty = 0, glosqty = 0, grakqty = 0;
                double gissqty1 = 0, grecqty1 = 0, gretqty1 = 0, gshrqty1 = 0;

                string chk1fld = "", chk1val = "", chk1nm = "", chk2fld = "", chk2val = "", chk2nm = "";
                Int32 maxR = 0, i = 0, rNo = 0, slno = 0;
                i = 0; maxR = tbl.Rows.Count - 1;
                if (showparty == false)
                {
                    chk1fld = "itcd"; chk1nm = "itnm"; chk2fld = "repslcd"; chk2nm = "repslnm";
                }
                else
                {
                    chk1fld = "repslcd"; chk1nm = "repslnm"; chk2fld = "docdt"; chk2nm = "docdt";
                }
                string chkfld3 = "";
                if(VE.Checkbox5 == true)
                {
                    chkfld3 = "itcd";
                }
                else
                {
                    chkfld3 = "progautono";
                }
                Int32 noparty = 0;
                while (i <= maxR)
                {
                    chk1val = tbl.Rows[i][chk1fld].ToString();
                    if (showsumm == false && (showparty == true || showitem == true))
                    {
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["Dammy"] = "[ " + chk1val + "] " + tbl.Rows[i][chk1nm];
                        if (showparty == true)
                        {
                            //IR.Rows[rNo]["Dammy"] = IR.Rows[rNo]["Dammy"] + " [ " + tbl.Rows[i]["propname"].ToString() + (tbl.Rows[i]["regmobile"] == DBNull.Value ? "" : "  Mob." + tbl.Rows[i]["regmobile"].ToString()) + " ]";
                            IR.Rows[rNo]["Dammy"] = IR.Rows[rNo]["Dammy"] + " [ " + tbl.Rows[i]["slnm"].retStr() + " ]";
                        }
                        IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                    }

                    double popqty = 0, pissqty = 0, precqty = 0, pretqty = 0, pshrqty = 0, pbalqty = 0, plosqty = 0, prakqty = 0;
                    double pissqty1 = 0, precqty1 = 0, pretqty1 = 0, pshrqty1 = 0;
                    while (tbl.Rows[i][chk1fld].ToString() == chk1val)
                    {
                        chk2val = tbl.Rows[i][chk2fld].ToString();
                        //if (showsumm == false)
                        //{
                        //    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        //    IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + "" + chk2val + " </span>" + tbl.Rows[i][chk2nm];
                        //    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                        //}
                        double op = 0, op1 = 0, cl = 0, cl1 = 0;
                        double sopqty = 0, sissqty = 0, srecqty = 0, sretqty = 0, sshrqty = 0, sbalqty = 0, slosqty = 0, srakqty = 0;
                        double sissqty1 = 0, srecqty1 = 0, sretqty1 = 0, sshrqty1 = 0;
                        while (tbl.Rows[i][chk1fld].ToString() == chk1val && tbl.Rows[i][chk2fld].ToString() == chk2val)
                        {
                            //for the period
                            double tissqty = 0, trecqty = 0, tretqty = 0, tshrqty = 0, tlosqty = 0, trakqty = 0;
                            double tissqty1 = 0, trecqty1 = 0, tretqty1 = 0, tshrqty1 = 0;
                            while (tbl.Rows[i][chk1fld].ToString() == chk1val && tbl.Rows[i][chk2fld].ToString() == chk2val && Convert.ToDateTime(tbl.Rows[i]["docdt"]) <= Convert.ToDateTime(tdt))
                            {
                                string autono = tbl.Rows[i]["progautono"].ToString();
                                string dsc = ""; string ordno = "";
                                double issqty = 0, recqty = 0, retqty = 0, shrqty = 0, losqty = 0, rakqty = 0;
                                double issqty1 = 0, recqty1 = 0, retqty1 = 0, shrqty1 = 0;
                                string oldbatchno = VE.Checkbox4 == true ? tbl.Rows[i]["batchno"].ToString() : "";
                                string chkval3 = tbl.Rows[i][chkfld3].ToString();
                                while (tbl.Rows[i][chk1fld].ToString() == chk1val && tbl.Rows[i][chk2fld].ToString() == chk2val && tbl.Rows[i]["progautono"].ToString() == autono && tbl.Rows[i][chkfld3].ToString() == chkval3)
                                {
                                    string chk1 = tbl.Rows[i]["itcd"].ToString();
                                    double cpcs = 0, cbox = 0, chkqty = 0, chkpcs = 0, cqty = 0;
                                    if (tbl.Rows[i]["styleno"].ToString() == "") dsc += tbl.Rows[i]["itnm"].ToString() + ",";
                                    else dsc += tbl.Rows[i]["styleno"].ToString() + " " + tbl.Rows[i]["partcd"] + ",";
                                    oldbatchno = VE.Checkbox4 == true ? tbl.Rows[i]["batchno"].ToString() : "";
                                    if (tbl.Rows[i]["ORDDOCNO"].ToString() != "") ordno += tbl.Rows[i]["ORDDOCNO"].ToString() + " ,";
                                    while (tbl.Rows[i][chk1fld].ToString() == chk1val && tbl.Rows[i][chk2fld].ToString() == chk2val && tbl.Rows[i]["progautono"].ToString() == autono && tbl.Rows[i]["itcd"].ToString() == chk1 && tbl.Rows[i][chkfld3].ToString() == chkval3)
                                    {
                                        chkqty = Convert.ToDouble(tbl.Rows[i]["balqnty"]);

                                        shrqty = shrqty + 0; // Convert.ToDouble(tbl.Rows[i]["shortqnty"]);
                                        if (tbl.Rows[i]["stktype"].ToString() == "D" || tbl.Rows[i]["stktype"].ToString() == "R") rakqty = rakqty + chkqty;
                                        else if (tbl.Rows[i]["stktype"].ToString() == "L") losqty = losqty + chkqty;
                                        else cpcs = cpcs + chkqty;
                                        if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F") chkpcs = chkpcs + chkqty;
                                        i++;
                                        if (i > maxR) break;
                                        if (VE.Checkbox4 == true && tbl.Rows[i]["batchno"].ToString() != oldbatchno) break;
                                        oldbatchno = VE.Checkbox4 == true ? tbl.Rows[i]["batchno"].ToString() : "";
                                    }
                                    if (stkcalcon == "B") cqty = Salesfunc.ConvPcstoBox(chkpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
                                    else if (stkcalcon == "S") cqty = Salesfunc.ConvPcstoSet(chkpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
                                    else cqty = cpcs;

                                    issqty = issqty + cpcs;
                                    //losqty = losqty + losqty;
                                    //rakqty = rakqty + rakqty;
                                    issqty1 = issqty1 + cqty;
                                    if (i > maxR) break;
                                    if (VE.Checkbox4 == true && tbl.Rows[i]["batchno"].ToString() != oldbatchno) break;
                                }
                                if (showsumm == false)
                                {
                                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                    IR.Rows[rNo]["docdt"] = tbl.Rows[i - 1]["docdt"].ToString().retDateStr();
                                    IR.Rows[rNo]["docno"] = tbl.Rows[i - 1]["docno"];
                                    IR.Rows[rNo]["dsc"] = dsc;
                                    IR.Rows[rNo]["issqnty"] = issqty;
                                    if (VE.Checkbox6 == true) IR.Rows[rNo]["ordno"] = ordno;
                                    if (VE.Checkbox4 == true) IR.Rows[rNo]["batchno"] = tbl.Rows[i - 1]["batchno"];
                                    if (VE.Checkbox3 == true) IR.Rows[rNo]["recdocno"] = tbl.Rows[i - 1]["recdocno"];
                                    if (VE.Checkbox3 == true) IR.Rows[rNo]["recslnm"] = tbl.Rows[i - 1]["recslnm"];
                                    if (showraka == true)
                                    {
                                        if (showothrunit == true) IR.Rows[rNo]["issqnty1"] = issqty1;
                                        //IR.Rows[rNo]["losqnty"] = losqty;
                                        //IR.Rows[rNo]["rakqnty"] = rakqty;
                                        if (showothrunit == true) IR.Rows[rNo]["issqnty1"] = issqty1;
                                    }
                                }
                                tissqty = tissqty + issqty;
                                tlosqty = tlosqty + losqty;
                                trakqty = trakqty + rakqty;
                                tissqty1 = tissqty1 + issqty1;

                                if (i > maxR) break;
                            }
                            //
                            sissqty = sissqty + tissqty;
                            slosqty = slosqty + tlosqty;
                            srakqty = srakqty + trakqty;
                            sissqty1 = sissqty1 + tissqty1;
                            if (i > maxR) break;
                        }
                        popqty = popqty + sopqty;
                        pissqty = pissqty + sissqty;
                        plosqty = plosqty + slosqty;
                        prakqty = prakqty + srakqty;
                        pissqty1 = pissqty1 + sissqty1;
                        if (i > maxR) break;
                    }
                    gissqty = gissqty + pissqty;
                    glosqty = glosqty + plosqty;
                    grakqty = grakqty + prakqty;
                    gissqty1 = gissqty1 + pissqty1;
                    //IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    if (showsumm == false)
                    {
                        if (showparty == true || showitem == true)
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;

                            IR.Rows[rNo]["dsc"] = "Total of [" + tbl.Rows[i - 1][chk1nm] + " ]";
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                        }
                        //else
                        //{
                        //    IR.Rows[rNo]["dsc"] = "Total";
                        //}
                        //IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                    }
                    else
                    {
                        noparty++;
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        IR.Rows[rNo]["docdt"] = noparty;
                        if (groupondate == true)
                        {
                            if (showparty == false)
                            {
                                IR.Rows[rNo]["docno"] = chk1val;
                                IR.Rows[rNo]["dsc"] = tbl.Rows[i - 1][chk1nm].ToString();
                            }
                            else
                            {
                                IR.Rows[rNo]["docno"] = chk1val.retDateStr();
                                IR.Rows[rNo]["dsc"] = tbl.Rows[i - 1][chk1nm].ToString().retDateStr();
                            }
                        }
                        else
                        {
                            IR.Rows[rNo]["docno"] = chk1val;
                            IR.Rows[rNo]["dsc"] = tbl.Rows[i - 1][chk1nm];
                        }
                        //IR.Rows[rNo]["opqnty"] = popqty;
                    }
                    if (showparty == true || showitem == true)
                    {
                        IR.Rows[rNo]["issqnty"] = pissqty;
                        if (showraka == true)
                        {
                            //IR.Rows[rNo]["losqnty"] = plosqty;
                            //IR.Rows[rNo]["rakqnty"] = prakqty;
                            if (showothrunit == true) IR.Rows[rNo]["issqnty1"] = pissqty1;
                        }
                    }
                    if (i > maxR) break;
                }

                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["dsc"] = "Grand Totals";
                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                IR.Rows[rNo]["issqnty"] = gissqty;
                if (showraka == true)
                {
                    //IR.Rows[rNo]["losqnty"] = glosqty;
                    //IR.Rows[rNo]["rakqnty"] = grakqty;
                    if (showothrunit == true) IR.Rows[rNo]["issqnty1"] = gissqty1;
                }

                string pghdr1 = "", repname = CommFunc.retRepname("rep_partyleg");
                pghdr1 = "Pending Challan (" + jobnm + ") from " + fdt + " to " + tdt;

                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public ActionResult Rep_Pend_Job_Item(ReportViewinHtml VE, FormCollection FC, string repon)
        {
            string ModuleCode = Module.Module_Code;
            try
            {
                Cn.getQueryString(VE);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));

                string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
                string brandcd = "", itgrpcd = "", itcd = "", tdt = "", gocd = "", stktype = "", slcd = "", linecd = "", recslcd = "";
                bool stockval = VE.Checkbox1;
                bool showitem = VE.Checkbox2;
                jobcd = VE.JOBCD;// VE.MENU_PARA;
                jobnm = VE.JOBNM;// DB.M_JOBMST.Find(jobcd).JOBNM;

                tdt = VE.TDT;
                if (VE.MENU_PARA == "IR")
                {
                    if (FC.AllKeys.Contains("RECPARTY"))
                    {
                        recslcd = FC["RECPARTY"].ToString().retSqlformat();
                    }
                }
                if (FC.AllKeys.Contains("slcdvalue"))
                {
                    slcd = FC["slcdvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("linecdvalue"))
                {
                    linecd = FC["linecdvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("brandcdvalue"))
                {
                    brandcd = FC["brandcdvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("itgrpcdvalue"))
                {
                    itgrpcd = FC["itgrpcdvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("itcdvalue"))
                {
                    itcd = FC["itcdvalue"].ToString().retSqlformat();
                }
                if (FC.AllKeys.Contains("STKTYPE"))
                {
                    stktype = FC["STKTYPE"].ToString().retSqlformat();
                }

                string mtrljobcd = "'FS'";
                if (VE.TEXTBOX3 != null) mtrljobcd = "'" + VE.TEXTBOX3 + "'";

                DataTable tbl = Salesfunc.getPendProg(tdt, tdt, slcd, itcd, "'" + jobcd + "'", "", "", linecd);
                tbl.DefaultView.Sort = "brandnm, brandcd, itgrpnm, itgrpcd, styleno, itcd, partcd, print_seq, sizenm";//sizecdgrp
                //var data = from d in tbl.AsEnumerable()
                //           orderby d.Field<string>("brandnm") + d.Field<string>("brandcd") + d.Field<string>("itgrpnm") + d.Field<string>("itgrpcd") +
                //           Convert.ToInt32(string.Join(null, System.Text.RegularExpressions.Regex.Split(d.Field<string>("styleno"), "[^\\d]"))) +
                //           d.Field<string>("itcd") + d.Field<string>("partcd") + d.Field<string>("print_seq") + d.Field<string>("sizecdgrp") + d.Field<string>("sizenm")
                //           select d;
                //tbl = data.AsDataView().ToTable();
                tbl = tbl.DefaultView.ToTable();
                if (tbl.Rows.Count == 0)
                {
                    return Content("No Record Found");
                }
                if (repon == "Download Excel")
                {
                    string excelstr = Salesfunc.genStockinExcel(tbl, "balqnty", "Pending at (" + jobnm + ") as on " + tdt);
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment; filename=pendstkstmt" + ".xls");
                    Response.ContentType = "application/ms-excel";
                    Response.Charset = "";
                    Response.Output.Write(excelstr);
                    Response.Flush();
                    Response.End();
                    return Content("Pending Job file downloaded sucessfully");
                }

                DataTable tblsizedata = Salesfunc.retSizeGrpData();

                mtrljobcd = jobcd;
                string qtystock = "N";
                if (mtrljobcd == "FT" || mtrljobcd == "GT" || mtrljobcd == "YP" && mtrljobcd == "WA" || mtrljobcd == "PF") qtystock = "Y";

                string qtyhd = "", qtydsp = "", stkcalcon = "P";
                if (mtrljobcd == "YP" || mtrljobcd == "FT" || mtrljobcd == "GT" || mtrljobcd == "YP" && mtrljobcd == "WA" || mtrljobcd == "PF") stkcalcon = "Q";

                if (stkcalcon == "P") { qtyhd = "Pcs"; qtydsp = "n,17,2:####,##,##,##0"; }
                else if (stkcalcon == "B") { qtyhd = "Box"; qtydsp = "n,17,2:##,##,##,##0.00"; }
                else if (stkcalcon == "S") { qtyhd = "Set"; qtydsp = "n,17,2:##,##,##,##0.00"; }
                else { qtyhd = "Un"; qtydsp = "n,17,2:####,##,##0.000"; }
                if (stkcalcon == "Q") qtydsp = "n,17,2:####,##,##0.000";

                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();

                DataTable IR = new DataTable("mstrep");
                HC.RepStart(IR, 3);
                HC.GetPrintHeader(IR, "slno", "double", "n,4", "Sl#");
                HC.GetPrintHeader(IR, "styleno", "string", "c,15", "Style No");
                HC.GetPrintHeader(IR, "itnm", "string", "c,25", "Item");
                if (showitem == true && qtystock == "N")
                {
                    //HC.GetPrintHeader(IR, "pcsperbox", "double", "n,5", "P/Box");
                    HC.GetPrintHeader(IR, "sizedsp", "string", "c,50", "Sizes");
                    //HC.GetPrintHeader(IR, "boxdsp", "string", "c,30", "Boxes");
                }
                HC.GetPrintHeader(IR, "qnty", "double", qtydsp, "Total " + qtyhd);
                if (qtystock == "N")
                {
                    //HC.GetPrintHeader(IR, "box", "double", "n,12,2:##,##,##0.00", "Total Boxes");
                    //HC.GetPrintHeader(IR, "set", "double", "n,10,2:##,##,##0.00", "Total Set");
                }
                if (stockval == true) HC.GetPrintHeader(IR, "amt", "double", "n,17,2:####,##,##,##0.00", "Stock Val");

                string strbrandcd = "", stritgrpcd = "";
                Int32 maxR = 0, i = 0, rNo = 0, slno = 0;
                double tpcs = 0, tbox = 0, tset = 0, tamt = 0;
                double tapcs = 0, tabox = 0, taset = 0, taamt = 0;
                double tspcs = 0, tsbox = 0, tsset = 0, tsamt = 0;
                i = 0; maxR = tbl.Rows.Count - 1;

                while (i <= maxR)
                {
                    strbrandcd = tbl.Rows[i]["brandcd"].ToString();

                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["Dammy"] = "<span style='font-weight:100;font-size:9px;'>" + " " + " </span>" + tbl.Rows[i]["brandnm"];
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                    tapcs = 0; tabox = 0; taset = 0; slno = 0; taamt = 0;

                    while (tbl.Rows[i]["brandcd"].ToString() == strbrandcd)
                    {
                        stritgrpcd = tbl.Rows[i]["itgrpcd"].ToString();
                        if (showitem == true)
                        {
                            IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                            IR.Rows[rNo]["Dammy"] = "[" + stritgrpcd + "] " + tbl.Rows[i]["itgrpnm"];
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;";
                        }
                        tspcs = 0; tsbox = 0; tsset = 0; tsamt = 0;
                        if (showitem == true) slno = 0;

                        while (tbl.Rows[i]["brandcd"].ToString() == strbrandcd && tbl.Rows[i]["itgrpcd"].ToString() == stritgrpcd)
                        {
                            if (showitem == true)
                            {
                                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                                slno++;
                                IR.Rows[rNo]["slno"] = slno;
                                IR.Rows[rNo]["itnm"] = tbl.Rows[i]["itnm"].ToString();
                                if (qtystock == "N")
                                {
                                    IR.Rows[rNo]["styleno"] = tbl.Rows[i]["styleno"].ToString();
                                    //IR.Rows[rNo]["pcsperbox"] = Convert.ToDouble(tbl.Rows[i]["pcsperbox"]);
                                }
                            }

                            string sizes = "", boxes = "";
                            string chk1 = tbl.Rows[i]["itcd"].ToString();
                            double ctpcs = 0, ctbox = 0, ctset = 0, chkpcs = 0, ctamt = 0;
                            //string sizedata = chk1 + Cn.GCS() + tbl.Rows[i]["mixsize"] + Cn.GCS() + "" + Cn.GCS() + tbl.Rows[1]["pcsperbox"] + Cn.GCS() + tbl.Rows[1]["pcsperset"] + Cn.GCS();
                            while (tbl.Rows[i]["itcd"].ToString() == chk1)
                            {
                                string chk2 = tbl.Rows[i]["sizecdgrp"].ToString();
                                double cpcs = 0, cbox = 0, crate = 0, sizepcs = 0;
                                string partcd = tbl.Rows[i]["partcd"].ToString();
                                while (tbl.Rows[i]["itcd"].ToString() == chk1 && tbl.Rows[i]["sizecdgrp"].ToString() == chk2)
                                {
                                    if (tbl.Rows[i]["partcd"].ToString() == partcd)
                                    {
                                        cpcs = cpcs + Convert.ToDouble(tbl.Rows[i]["balqnty"]);
                                        if (tbl.Rows[i]["stktype"].ToString() == "" || tbl.Rows[i]["stktype"].ToString() == "F")
                                        {
                                            chkpcs = chkpcs + Convert.ToDouble(tbl.Rows[i]["balqnty"]);
                                            sizepcs = sizepcs + Convert.ToDouble(tbl.Rows[i]["balqnty"]);
                                        }
                                    }
                                    i++;
                                    if (i > maxR) break;
                                }
                                //sizedata += tbl.Rows[i - 1]["sizenm"] + "=" + sizepcs.ToString() + "^";
                                //double dbboxes = Salesfunc.ConvPcstoBox(cpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
                                if (sizes != "") sizes += ", ";
                                //sizes += tbl.Rows[i - 1]["sizenm"]+"="+dbboxes.ToString();
                                //sizes += Salesfunc.retsizemaxmin(chk2) + "=" + dbboxes.ToString();
                                sizes += Salesfunc.retsizemaxmin(chk2);
                                //if (boxes != "") boxes += "+";
                                //boxes += dbboxes.ToString();
                                ctpcs = ctpcs + cpcs;
                                ctamt = ctamt + Math.Round(cpcs * crate, 0);
                                if (i > maxR) break;
                            }
                            //string sizedsp = Sales_func.retsizedata(tblsizedata, itcd, tbl.Rows[i-1]["mixsize"].ToString(), sizes, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
                            //ctbox = Salesfunc.ConvPcstoBox(chkpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperbox"]));
                            ctset = Salesfunc.ConvPcstoSet(chkpcs, Convert.ToDouble(tbl.Rows[i - 1]["pcsperset"]));
                            if (showitem == true)
                            {
                                if (showitem == true && qtystock == "N") IR.Rows[rNo]["sizedsp"] = sizes;
                                IR.Rows[rNo]["qnty"] = ctpcs;
                                if (qtystock == "N")
                                {
                                    //IR.Rows[rNo]["box"] = ctbox;
                                    //IR.Rows[rNo]["set"] = ctset;
                                }
                                if (stockval == true) IR.Rows[rNo]["amt"] = ctamt;
                            }
                            tspcs = tspcs + ctpcs;
                            tsbox = tsbox + ctbox;
                            tsset = tsset + ctset;
                            tsamt = tsamt + ctamt;
                            if (i > maxR) break;
                        }
                        //total of itgrpnm
                        tapcs = tapcs + tspcs;
                        tabox = tabox + tsbox;
                        taset = taset + tsset;
                        taamt = taamt + tsamt;
                        IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                        if (showitem == true)
                        {
                            IR.Rows[rNo]["itnm"] = "Total of [" + tbl.Rows[i - 1]["itgrpnm"].ToString() + " ]";
                            IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                        }
                        else
                        {
                            slno++;
                            IR.Rows[rNo]["slno"] = slno;
                            if (qtystock == "N") IR.Rows[rNo]["styleno"] = tbl.Rows[i - 1]["itgrpcd"].ToString();
                            IR.Rows[rNo]["itnm"] = tbl.Rows[i - 1]["itgrpnm"].ToString();
                        }
                        IR.Rows[rNo]["qnty"] = tspcs;
                        if (qtystock == "N")
                        {
                            //IR.Rows[rNo]["box"] = tsbox;
                            //IR.Rows[rNo]["set"] = tsset;
                        }
                        if (stockval == true) IR.Rows[rNo]["amt"] = tsamt;

                        if (i > maxR) break;
                    }
                    //total of Brand
                    tpcs = tpcs + tapcs;
                    tbox = tbox + tabox;
                    tset = tset + taset;
                    tamt = tamt + taamt;
                    IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                    IR.Rows[rNo]["itnm"] = "Total of [" + tbl.Rows[i - 1]["brandnm"].ToString() + " ]";
                    IR.Rows[rNo]["qnty"] = tapcs;
                    if (qtystock == "N")
                    {
                        //IR.Rows[rNo]["box"] = tabox;
                        //IR.Rows[rNo]["set"] = taset;
                    }
                    if (stockval == true) IR.Rows[rNo]["amt"] = taamt;
                    IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";
                    if (i > maxR) break;
                }

                IR.Rows.Add(""); rNo = IR.Rows.Count - 1;
                IR.Rows[rNo]["itnm"] = "Grand Totals";
                IR.Rows[rNo]["qnty"] = tpcs;
                if (qtystock == "N")
                {
                    //IR.Rows[rNo]["box"] = tbox;
                    //IR.Rows[rNo]["set"] = tset;
                }
                if (stockval == true) IR.Rows[rNo]["amt"] = tamt;
                IR.Rows[rNo]["flag"] = "font-weight:bold;font-size:13px;border-bottom: 3px solid;;border-top: 3px solid;";

                string pghdr1 = "", repname = CommFunc.retRepname("rep_partylegItem");
                pghdr1 = "Pending at (" + jobnm + ") as on " + tdt;

                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);
                TempData[repname] = PV;
                //TempData[repname + "xxx"] = IR;
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