﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Collections;
using System.Data;
using OfficeOpenXml;
using Oracle.ManagedDataAccess.Client;
using System.IO;

namespace Improvar.Controllers
{
    public class Rep_PartyItemSummController : Controller
    {
        // GET: M_Grpmas
        Connection Cn = new Connection(); string CS = null;
        MasterHelp masterHelp = new MasterHelp();
        DropDownHelp dropDownHelp = new DropDownHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult Rep_PartyItemSumm(string SLCD = "", string SLNM = "", string FDT = "", string TDT = "", string CHECK = "", string ITGRPCD = "")
        {

            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Party & Design Summary";
                    PartyitemSummReport VE = new PartyitemSummReport();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    string scmf = CommVar.FinSchema(UNQSNO);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));

                    if (SLCD.retStr() == "")
                    {
                        VE.DropDown_list_ITGRP = dropDownHelp.GetItgrpcdforSelection();
                        VE.ITGRPCD = masterHelp.ComboFill("ITGRPCD", VE.DropDown_list_ITGRP, 0, 1);
                    }
                    else
                    {
                        VE.SLCD = SLCD; VE.FDT2 = FDT; VE.TDT2 = TDT; VE.ITGRPCD = ITGRPCD;

                        VE.SLCD2 = SLCD; VE.ITGRPCD2 = ITGRPCD; VE.ONLYSALES2 = CHECK;

                        if (CHECK == "Y")
                        {
                            VE.CHECK = true;
                        }
                        else
                        {
                            VE.CHECK = false;
                        }
                        ViewBag.SLNM = SLNM + " [" + SLCD + "]";

                        DataTable dt = GetData(SLCD, FDT, TDT, ITGRPCD, CHECK);

                        VE.billdet = (from DataRow DR in dt.Rows
                                      group DR by new
                                      {
                                          styleno = DR["itstyle"].retStr(),
                                          itgrpnm = DR["itgrpnm"].retStr(),
                                          itnm = DR["itnm"].retStr(),
                                          itcd = DR["itcd"].retStr(),
                                          BarImages = DR["barimage"].retStr(),
                                      } into X
                                      select new billdet()
                                      {
                                          itcd = X.Key.itcd.retStr(),
                                          styleno = X.Key.styleno.retStr(),
                                          itgrpnm = X.Key.itgrpnm.retStr(),
                                          itnm = X.Key.itnm.retStr(),
                                          BarImages = X.Key.BarImages.retStr(),
                                          sqnty = X.Sum(Z => Z.Field<decimal>("sqnty").retDbl()),
                                          samt = X.Sum(Z => Z.Field<decimal>("samt").retDbl()),
                                          srate = X.Sum(Z => Z.Field<decimal>("sqnty").retDbl()) == 0 ? 0 : (X.Sum(Z => Z.Field<decimal>("samt").retDbl()) / X.Sum(Z => Z.Field<decimal>("sqnty").retDbl())).toRound(2),

                                          rqnty = X.Sum(Z => Z.Field<decimal>("srqnty").retDbl()),
                                          ramt = X.Sum(Z => Z.Field<decimal>("sramt").retDbl()),
                                          rrate = X.Sum(Z => Z.Field<decimal>("srqnty").retDbl()) == 0 ? 0 : (X.Sum(Z => Z.Field<decimal>("sramt").retDbl()) / X.Sum(Z => Z.Field<decimal>("srqnty").retDbl())).toRound(2),

                                          netqnty = (X.Sum(Z => Z.Field<decimal>("sqnty").retDbl()) - X.Sum(Z => Z.Field<decimal>("srqnty").retDbl())).toRound(2),
                                          netamt = (X.Sum(Z => Z.Field<decimal>("samt").retDbl()) - X.Sum(Z => Z.Field<decimal>("sramt").retDbl())).toRound(2),
                                          netrate = ((X.Sum(Z => Z.Field<decimal>("sqnty").retDbl()) - X.Sum(Z => Z.Field<decimal>("srqnty").retDbl())).toRound(2)) == 0 ? 0 : (((X.Sum(Z => Z.Field<decimal>("samt").retDbl()) - X.Sum(Z => Z.Field<decimal>("sramt").retDbl())).toRound(2)) / ((X.Sum(Z => Z.Field<decimal>("sqnty").retDbl()) - X.Sum(Z => Z.Field<decimal>("srqnty").retDbl())).toRound(2))).toRound(2),
                                      }).OrderBy(A => A.styleno).ToList();

                        foreach (var v in VE.billdet)
                        {
                            if (v.BarImages.retStr() != "")
                            {
                                var brimgs = v.BarImages.retStr().Split((char)179);
                                v.BarImagesCount = brimgs.Length == 0 ? "" : brimgs.Length.retStr();
                                string BarImages = "";
                                foreach (var barimg in brimgs)
                                {
                                    string barfilename = barimg.Split('~')[0];
                                    string barimgdesc = barimg.Split('~')[1];
                                    BarImages += (char)179 + CommVar.WebUploadDocURL(barfilename) + "~" + barimgdesc;
                                    string FROMpath = CommVar.SaveFolderPath() + "/ItemImages/" + barfilename;
                                    FROMpath = Path.Combine(FROMpath, "");
                                    string TOPATH = CommVar.LocalUploadDocPath() + barfilename;
                                    Cn.CopyImage(FROMpath, TOPATH);
                                }
                                v.BarImages = BarImages.retStr().TrimStart((char)179);
                            }
                        }


                        VE.T_sqnty = VE.billdet.Sum(a => a.sqnty).retDbl();
                        VE.T_samt = VE.billdet.Sum(a => a.samt).retDbl();
                        VE.T_rqnty = VE.billdet.Sum(a => a.rqnty).retDbl();
                        VE.T_ramt = VE.billdet.Sum(a => a.ramt).retDbl();
                        VE.T_netqnty = VE.billdet.Sum(a => a.netqnty).retDbl();
                        VE.T_netamt = VE.billdet.Sum(a => a.netamt).retDbl();

                        //VE.MGRPNM = DB.M_TMGRP.Where(a => a.MGRPCD == MGRPCD).Select(a => a.MGRPNM).SingleOrDefault();
                        //var base64 = DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD).ToList();
                        //if (base64.Any())
                        //{
                        //    VE.FOUNDMGRP = true;
                        //}
                        //var group = (from i in DB.M_TGRP
                        //             where (i.MGRPCD == MGRPCD && i.GLCD == null)
                        //             select i).OrderBy(a => a.ROOTCD).ThenBy(a => a.GRPCDFULL).ToList();

                        //if (group.Any())
                        //{
                        //    List<Temp_TGRP> MLIST = new List<Temp_TGRP>();
                        //    VE.Tree = GenerateTree(group, ref MLIST);
                        //    VE.MLIST = MLIST;
                        //}
                        //var Mtype = DB.M_TMGRP.Where(a => a.MGRPCD == MGRPCD).Select(a => a.MGRPTYPE).SingleOrDefault();
                        //DataTable dt = new DataTable();
                        //if (Mtype == "SL")
                        //{
                        //    //string str = "select * from (select glcd, '' slcd, '' class1cd, glnm,'' slnm, '' class1nm from " + scmf + ".m_genleg where nvl(slcdmust,'N') = 'N' union all ";
                        //    //str = str + " select a.glcd, '', '' class1cd, a.glnm, c.slnm, '' class1nm from " + CommVar.CurSchema(UNQSNO) + ".m_genleg a, " + scmf + ".m_subleg_gl b, " + scmf + ".m_subleg c where a.glcd = b.glcd ";
                        //    //str = str + " and nvl(slcdmust,'N') = 'Y' and b.slcd = c.slcd) where glcd||slcd||class1cd not in (select glcd||slcd||class1cd from " + CommVar.CurSchema(UNQSNO) + ".m_tgrp where mgrpcd = '" + MGRPCD + "' and glcd||slcd||class1cd is not null)";
                        //    //DataTable dt = masterHelp.SQLquery(str);
                        //    //VE.AvailableGroup = (from i in dt.AsEnumerable()
                        //    //                     select new AvailableACGroup()
                        //    //                     {
                        //    //                         Checked = false,
                        //    //                         CLASS1CD = i.Field<string>("class1cd"),
                        //    //                         CLASS1NM = i.Field<string>("class1nm"),
                        //    //                         GLCD = i.Field<string>("glcd"),
                        //    //                         GLNM = i.Field<string>("glnm"),
                        //    //                         SLCD = i.Field<string>("slcd"),
                        //    //                         SLNM = i.Field<string>("slnm"),
                        //    //                     }).ToList();
                        //}
                        //else if (Mtype == "GL")
                        //{
                        //    string str = "select glcd, '' class1cd, glnm, '' class1nm from " + scmf + ".m_genleg where glcd not in (select glcd from " + scmf + ".m_tgrp where mgrpcd = '" + MGRPCD + "' and glcd is not null) order by glnm";
                        //    dt = masterHelp.SQLquery(str);
                        //}
                        //else if (Mtype == "CL" || Mtype == "SL")
                        //{
                        //    string str = "select * from (select glcd, '' class1cd, glnm, '' class1nm from " + scmf + ".m_genleg where nvl(class1cdmust,'N') = 'N' union all ";
                        //    str = str + "select distinct a.glcd, b.class1cd, c.glnm, e.class1nm from " + scmf + ".t_vch_det a, " + scmf + ".t_vch_class b, " + scmf + ".m_genleg c, ";
                        //    str = str + scmf + ".m_class1 e where a.autono = b.autono and a.slno = b.dslno and a.glcd = c.glcd and b.class1cd = e.class1cd and nvl(c.class1cdmust, 'N')= 'Y' order by glcd,class1cd) ";
                        //    str = str + " where glcd|| class1cd not in (select glcd || class1cd from " + scmf + ".m_tgrp where mgrpcd = '" + MGRPCD + "' and glcd|| class1cd is not null)";
                        //    dt = masterHelp.SQLquery(str);
                        //}
                        //VE.AvailableGroup = (from i in dt.AsEnumerable()
                        //                     select new AvailableACGroup()
                        //                     {
                        //                         Checked = false,
                        //                         CLASS1CD = i.Field<string>("class1cd"),
                        //                         CLASS1NM = i.Field<string>("class1nm"),
                        //                         GLCD = i.Field<string>("glcd"),
                        //                         GLNM = i.Field<string>("glnm"),
                        //                     }).ToList();
                    }
                    VE.DefaultView = true;
                    //List<DropDown_list1> drplist = new List<DropDown_list1>();
                    //DropDown_list1 drop1 = new DropDown_list1();
                    //drop1.text = "Yes"; drop1.value = "Y"; drplist.Add(drop1);
                    //DropDown_list1 drop2 = new DropDown_list1();
                    //drop2.text = "No"; drop2.value = "N"; drplist.Add(drop2);
                    //DropDown_list1 drop3 = new DropDown_list1();
                    //drop3.text = "Main"; drop3.value = "M"; drplist.Add(drop3);
                    //VE.SchedulePart = drplist;
                    //VE.LEGDTLSKP = false;

                    return View(VE);
                }
            }
            catch (Exception ex)
            {
                PartyitemSummReport VE = new PartyitemSummReport();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                return View(VE);
            }
        }
        public ActionResult GetSubLedgerDetails(string val, string Code)
        {
            try
            {
                var agent = Code.Split(Convert.ToChar(Cn.GCS()));
                if (agent.Count() > 1)
                {
                    if (agent[1] == "")
                    {
                        return Content("Please Select Agent !!");
                    }
                    else
                    {
                        Code = agent[0];
                    }
                }
                else if (Code == "party")
                {
                    PartyitemSummReport VE = new PartyitemSummReport();
                    Cn.getQueryString(VE);
                    switch (VE.DOC_CODE)
                    {
                        case "SORD": Code = "D"; break;
                        case "PORD": Code = "C"; break;
                        default: Code = "D"; break;
                    }

                }
                var str = masterHelp.SLCD_help(val, Code);
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

        public string BtnSubmit(string slcd, string slnm, string fdt, string tdt, string check, string itgrpcd)
        {
            try
            {
                string url = "";
                var PreviousUrl = System.Web.HttpContext.Current.Request.UrlReferrer.AbsoluteUri;
                var uri = new Uri(PreviousUrl);//Create Virtually Query String
                var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
                if (queryString.Get("SLCD") == null)
                {
                    url = Request.UrlReferrer.ToString() + "&SLCD=" + slcd + "&SLNM=" + slnm + "&FDT=" + fdt + "&TDT=" + tdt + "&CHECK=" + check + "&ITGRPCD=" + itgrpcd;
                }
                else
                {
                    string dd = Request.UrlReferrer.ToString();
                    int pos = Request.UrlReferrer.ToString().IndexOf("&SLCD=");
                    url = dd.Substring(0, pos);
                    url = url + "&SLCD=" + slcd + "&SLNM=" + slnm + "&FDT=" + fdt + "&TDT=" + tdt + "&CHECK=" + check + "&ITGRPCD=" + itgrpcd;
                }
                return url;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public ActionResult GetItemData(string slcd = "", string fdt = "", string tdt = "", string check = "", string itgrpcd = "", string itcd = "", string itnm = "")
        {
            try
            {
                PartyitemSummReport VE = new PartyitemSummReport();
                //string itcd = (from a in VE.billdet where a.Checked == true select a.itcd).ToArray().retSqlfromStrarray();
                DataTable dt = GetData(slcd, fdt, tdt, itgrpcd, check, itcd);

                VE.ItmDet = (from DataRow DR in dt.Rows
                             group DR by new
                             {
                                 refdt = DR["docdt"].retDateStr(),
                                 refno = DR["docno"].retStr(),
                                 slnm = DR["slnm"].retStr(),
                                 docnm = DR["docnm"].retStr(),
                                 colrnm = DR["colrnm"].retStr(),
                                 itrem = DR["itrem"].retStr(),
                             } into X
                             select new ItmDet()
                             {
                                 refdt = X.Key.refdt.retStr(),
                                 refno = X.Key.refno.retStr(),
                                 slnm = X.Key.slnm.retStr(),
                                 docnm = X.Key.docnm.retStr(),
                                 colrnm = X.Key.colrnm.retStr(),
                                 itrem = X.Key.itrem.retStr(),
                                 sqnty = X.Sum(Z => Z.Field<decimal>("sqnty").retDbl()),
                                 samt = X.Sum(Z => Z.Field<decimal>("samt").retDbl()),

                                 rqnty = X.Sum(Z => Z.Field<decimal>("srqnty").retDbl()),
                                 ramt = X.Sum(Z => Z.Field<decimal>("sramt").retDbl()),
                             }).OrderBy(A => A.refdt).OrderBy(A => A.refno).ToList();

                VE.T_sqntyi = VE.ItmDet.Sum(a => a.sqnty).retDbl();
                VE.T_samti = VE.ItmDet.Sum(a => a.samt).retDbl();
                VE.T_rqntyi = VE.ItmDet.Sum(a => a.rqnty).retDbl();
                VE.T_ramti = VE.ItmDet.Sum(a => a.ramt).retDbl();

                ViewBag.ITNM = itnm;

                VE.DefaultView = true;
                ModelState.Clear();
                return PartialView("_Rep_PartyItemSumm_Item_Det", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }

        public DataTable GetData(string SLCD = "", string FDT = "", string TDT = "", string ITGRPCD = "", string CHECK = "", string ITCD = "")
        {
            string LOC = CommVar.Loccd(UNQSNO), COM = CommVar.Compcd(UNQSNO), scm1 = CommVar.CurSchema(UNQSNO), scmf = CommVar.FinSchema(UNQSNO);
            string txntag = "'SB','SR','SD','SC'";
            if (CHECK == "Y")
            {
                txntag = "'SB'";
            }
            string sql = "";
            sql += " select a.autono, a.doccd, a.docno,a.doctag, a.cancel,a.docdt,a.agslcd, " + Environment.NewLine;
            sql += "a.prefno, a.prefdt, a.slcd, a.slnm,a.slarea,a.agslnm,a.sagslnm,a.nm,a.mobile,a.gstno, a.district, " + Environment.NewLine;
            //if (dtlsumm == "E")
            //{
            //    sql += "(case when a.doctag = 'SR' or a.doctag = 'PR' then (case when a.rn = 1 then nvl(a.roamt, 0) else 0 end)*-1 else (case when a.rn = 1 then nvl(a.roamt, 0) else 0 end)end) roamt, " + Environment.NewLine;
            //    sql += "(case when a.doctag = 'SR' or a.doctag = 'PR' then (case when a.rn = 1 then nvl(a.tcsamt, 0) else 0 end)*-1 else (case when a.rn = 1 then nvl(a.tcsamt, 0) else 0 end)end) tcsamt, " + Environment.NewLine;
            //    sql += "(case when a.doctag = 'SR' or a.doctag = 'PR' then (case when a.rn = 1 then nvl(a.blamt, 0) else 0 end)*-1 else (case when a.rn = 1 then nvl(a.blamt, 0) else 0 end)end) blamt, " + Environment.NewLine;
            //}
            //else
            //{
            sql += "a.roamt,a.blamt,a.tcsamt, " + Environment.NewLine;
            //}
            sql += "a.slno,a.stkdrcr,a.itgrpnm, a.itcd, " + Environment.NewLine;
            sql += "a.itnm,a.itstyle, a.itrem,a.barno, a.barimagecount, a.barimage, a.hsncode,a.uomcd,a.uomnm, a.decimals,a.colrcd,a.colrnm, " + Environment.NewLine;

            sql += "(case when a.doctag = 'SB' then nvl(a.nos, 0) else 0 end)snos," + Environment.NewLine;
            sql += "(case when a.doctag = 'SR' then nvl(a.nos, 0) else 0 end)srnos," + Environment.NewLine;

            sql += "(case when a.doctag = 'SB' then nvl(a.qnty, 0) else 0 end)sqnty," + Environment.NewLine;
            sql += "(case when a.doctag = 'SR' then nvl(a.qnty, 0) else 0 end)srqnty," + Environment.NewLine;

            sql += "(case when a.doctag = 'SB' then nvl(a.rate, 0) else 0 end)srate," + Environment.NewLine;
            sql += "(case when a.doctag = 'SR' then nvl(a.rate, 0) else 0 end)srrate," + Environment.NewLine;

            sql += "(case when a.doctag = 'SB' then nvl(a.amt, 0) else 0 end)samt," + Environment.NewLine;
            sql += "(case when a.doctag = 'SR' then nvl(a.amt, 0) else 0 end)sramt," + Environment.NewLine;

            //if (dtlsumm == "E")
            //{
            //    sql += "  (case when a.doctag = 'SR' or a.doctag = 'PR' then nvl(a.amt, 0) * -1 else nvl(a.amt, 0) end)amt," + Environment.NewLine;
            //    sql += "  (case when a.doctag = 'SR' or a.doctag = 'PR' then nvl(a.scmdiscamt, 0) * -1 else nvl(a.scmdiscamt, 0) end)scmdiscamt," + Environment.NewLine;
            //    sql += "  (case when a.doctag = 'SR' or a.doctag = 'PR' then nvl(a.tddiscamt, 0) * -1 else nvl(a.tddiscamt, 0) end)tddiscamt," + Environment.NewLine;
            //    sql += "  (case when a.doctag = 'SR' or a.doctag = 'PR' then nvl(a.discamt, 0) * -1 else nvl(a.discamt, 0) end)discamt," + Environment.NewLine;
            //    sql += "  (case when a.doctag = 'SR' or a.doctag = 'PR' then nvl(a.TXBLVAL, 0) * -1 else nvl(a.TXBLVAL, 0) end)TXBLVAL," + Environment.NewLine;
            //}
            //else
            //{
            sql += " a.amt,a.scmdiscamt, a.tddiscamt, a.discamt,a.TXBLVAL, " + Environment.NewLine;

            //}
            sql += " a.conslcd, a.cslnm,a.cgstno, a.cdistrict, " + Environment.NewLine;
            sql += "a.trslnm,a.lrno,a.lrdt,a.GRWT,a.TRWT,a.NTWT,a.ordrefno,a.ordrefdt," + Environment.NewLine;
            //if (dtlsumm == "E")
            //{
            //    sql += "  a.igstper,(case when a.doctag = 'SR' or a.doctag = 'PR' then nvl(a.igstamt, 0) * -1 else nvl(a.igstamt, 0) end)igstamt,a.cgstper," + Environment.NewLine;
            //    sql += "  (case when a.doctag = 'SR' or a.doctag = 'PR' then nvl(a.cgstamt, 0) * -1 else nvl(a.cgstamt, 0) end)cgstamt,a.sgstper," + Environment.NewLine;
            //    sql += "  (case when a.doctag = 'SR' or a.doctag = 'PR' then nvl(a.sgstamt, 0) * -1 else nvl(a.sgstamt, 0) end)sgstamt,a.cessper," + Environment.NewLine;
            //    sql += "  (case when a.doctag = 'SR' or a.doctag = 'PR' then nvl(a.cessamt, 0) * -1 else nvl(a.sgstamt, 0) end)cessamt," + Environment.NewLine;
            //    sql += "(case when a.doctag = 'SR' or a.doctag = 'PR' then nvl(a.blqnty, 0)*-1 else nvl(a.blqnty, 0) end)blqnty," + Environment.NewLine;
            //    sql += "(case when a.doctag = 'SR' or a.doctag = 'PR' then nvl(a.NETAMT, 0)*-1 else nvl(a.NETAMT, 0) end)NETAMT,a.gstper," + Environment.NewLine;
            //    sql += "(case when a.doctag = 'SR' or a.doctag = 'PR' then nvl(a.gstamt, 0)*-1 else nvl(a.gstamt, 0) end)gstamt," + Environment.NewLine;
            //}
            //else
            //{
            sql += "a.igstper,a.igstamt,a.cgstper,a.cgstamt,a.sgstper,a.sgstamt,a.cessper,a.cessamt,a.blqnty,a.NETAMT,a.gstper,a.gstamt," + Environment.NewLine;
            //}


            sql += "a.ackno,a.ackdt,a.pageno,a.PAGESLNO,a.baleno,a.docrem,a.bltype,a.docnm  " + Environment.NewLine;
            sql += "from ( " + Environment.NewLine;


            sql += " select a.autono, a.doccd, a.docno,a.doctag, a.cancel,to_char(a.docdt,'DD/MM/YYYY')docdt,a.docdt tchdocdt,h.agslcd, " + Environment.NewLine;
            sql += "  a.prefno, nvl(to_char(a.prefdt,'dd/mm/yyyy'),'')prefdt,a.prefdt prefdate, a.slcd, c.slnm,c.slarea,l.slnm agslnm,m.slnm sagslnm,nvl(i.nm,p.rtdebnm) nm,i.mobile,c.gstno, c.district, nvl(a.roamt, 0) roamt, " + Environment.NewLine;
            sql += " nvl(a.tcsamt, 0) tcsamt, a.blamt, " + Environment.NewLine;
            sql += "   b.slno,b.stkdrcr,o.itgrpnm, b.itcd, " + Environment.NewLine;
            sql += "   b.itnm,b.itstyle, b.itrem,b.barno, b.hsncode,nvl(b.bluomcd,b.uomcd)uomcd, nvl(b.bluomnm,b.uomnm)uomnm, nvl(nullif(b.bldecimals,0),b.decimals) decimals,b.colrcd,b.colrnm, b.nos, " + Environment.NewLine;
            sql += " nvl(nullif(b.blqnty,0),b.qnty)qnty, b.rate, b.amt,b.scmdiscamt, b.tddiscamt, b.discamt,b.TXBLVAL, g.conslcd, d.slnm cslnm, d.gstno cgstno, d.district cdistrict, " + Environment.NewLine;
            sql += " e.slnm trslnm, f.lrno,nvl(to_char(f.lrdt,'dd/mm/yyyy'),'')lrdt,f.GRWT,f.TRWT,f.NTWT, '' ordrefno, to_char(nvl('', ''), 'dd/mm/yyyy') ordrefdt, b.igstper, b.igstamt, b.cgstper, " + Environment.NewLine;
            sql += " b.cgstamt,b.sgstamt, b.cessper, b.cessamt,b.blqnty,b.NETAMT,b.sgstper,b.igstper+b.cgstper+b.sgstper gstper,b.igstamt + b.cgstamt + b.sgstamt gstamt,k.ackno,nvl(to_char(k.ackdt,'dd/mm/yyyy'),'')ackdt,b.pageno,b.PAGESLNO,b.baleno,h.docrem,h.bltype,  " + Environment.NewLine;
            sql += "row_number() over(partition by a.autono order by b.slno)rn,j.docnm, y.barimagecount, y.barimage " + Environment.NewLine;
            sql += " from ( " + Environment.NewLine;
            sql += " select a.autono,a.doctag, b.doccd, b.docno, b.cancel, " + Environment.NewLine;
            sql += "b.docdt, " + Environment.NewLine;
            sql += "a.prefno, a.prefdt, a.slcd, a.roamt, a.tcsamt, a.blamt  " + Environment.NewLine;

            sql += "from " + scm1 + ".t_txn a, " + scm1 + ".t_cntrl_hdr b, " + Environment.NewLine;
            sql += " " + scmf + ".m_subleg c " + Environment.NewLine;
            sql += "  where a.autono = b.autono and a.slcd = c.slcd(+) " + Environment.NewLine;
            sql += "  and  b.compcd = '" + COM + "' " + Environment.NewLine;
            sql += " and b.loccd = '" + LOC + "'  " + Environment.NewLine;

            if (FDT != "") sql += "and b.docdt >= to_date('" + FDT + "','dd/mm/yyyy')   " + Environment.NewLine;
            if (TDT != "") sql += "and b.docdt <= to_date('" + TDT + "','dd/mm/yyyy')   " + Environment.NewLine;

            sql += "and a.doctag in (" + txntag + ") " + Environment.NewLine;
            sql += " ) a,  " + Environment.NewLine;

            sql += "(select distinct a.autono,a.stkdrcr, a.slno, a.itcd, a.itrem,d.barno, " + Environment.NewLine;
            sql += " b.itnm,b.styleno||' '||b.itnm itstyle,nvl(a.hsncode,b.hsncode) hsncode, b.uomcd, c.uomnm, c.decimals,a.colrcd,g.colrnm, " + Environment.NewLine;
            sql += "  a.nos, a.qnty, a.rate, a.amt,a.scmdiscamt,a.tddiscamt,a.discamt,a.TXBLVAL,a.NETAMT,   " + Environment.NewLine;
            sql += " a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.cessper, a.cessamt,a.blqnty,a.bluomcd,f.uomnm bluomnm,f.decimals bldecimals,a.pageno,a.pageslno,a.baleno  from " + scm1 + ".t_txndtl a, " + Environment.NewLine;
            sql += "" + scm1 + ".m_sitem b, " + scmf + ".m_uom c, " + scm1 + ".t_batchdtl d, " + scm1 + ".t_batchmst e, " + scmf + ".m_uom f, " + scm1 + ".m_color g " + Environment.NewLine;
            sql += " where a.itcd = b.itcd  and b.uomcd = c.uomcd and a.autono = d.autono(+) and a.slno=d.txnslno and d.barno = e.barno(+) and  a.bluomcd= f.uomcd(+) and a.colrcd=g.colrcd(+) " + Environment.NewLine;
            sql += " group by " + Environment.NewLine;
            sql += " a.autono,a.stkdrcr, a.slno, a.itcd, a.itrem,d.barno, " + Environment.NewLine;
            sql += "  b.itnm, nvl(a.hsncode,b.hsncode), b.uomcd, c.uomnm, c.decimals,a.colrcd,g.colrnm, a.nos, a.qnty, a.rate, a.amt,a.scmdiscamt,  " + Environment.NewLine;
            sql += " a.tddiscamt, a.discamt,a.TXBLVAL,a.NETAMT, a.igstper, a.igstamt, a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.cessper, a.cessamt,a.blqnty,a.bluomcd,f.uomnm,f.decimals,b.styleno||' '||b.itnm,a.pageno,a.PAGESLNO,a.baleno " + Environment.NewLine;
            sql += " union " + Environment.NewLine;
            sql += "select a.autono,";
            sql += "(case when d.doctype in ('SBILD','SPSLP','SBCM','SBPOS','SBCMR','SPRM') then 'C' else 'D' end ) " + Environment.NewLine;
            sql += " stkdrcr, a.slno + 1000 slno, a.amtcd itcd, '' itrem ,'' barno, b.amtnm itnm,b.amtnm itstyle ,a.hsncode,  " + Environment.NewLine;
            sql += " 'OTH' uomcd, 'OTH' uomnm, 0 decimals,'' colrcd,'' colrnm, 0 nos, 0 qnty, a.amtrate rate, a.amt,0 scmdiscamt, 0 tddiscamt, 0 discamt,a.amt TXBLVAL,0 NETAMT, a.igstper, a.igstamt, " + Environment.NewLine;
            sql += " a.cgstper, a.cgstamt, a.sgstper, a.sgstamt, a.cessper, a.cessamt,0 blqnty,'' bluomcd,''bluomnm,0 bldecimals,0 pageno,0 PAGESLNO,''baleno " + Environment.NewLine;
            sql += " from " + scm1 + ".t_txnamt a, " + scm1 + ".m_amttype b, " + scm1 + ".t_cntrl_hdr c, " + scm1 + ".m_doctype d " + Environment.NewLine;
            sql += " where a.amtcd = b.amtcd and a.autono=c.autono(+) and c.doccd=d.doccd(+) " + Environment.NewLine;
            sql += " ) b, " + scmf + ".m_subleg c, " + scmf + ".m_subleg d, " + scmf + ".m_subleg e, " + scm1 + ".t_txntrans f, " + Environment.NewLine;
            sql += "" + scm1 + ".t_txn g, " + scm1 + ".t_txnoth h ," + scm1 + ".t_txnmemo i ," + scm1 + ".m_doctype j," + scmf + ".t_txneinv k," + scmf + ".m_subleg l, " + Environment.NewLine;
            sql += "" + scmf + ".m_subleg m ," + scm1 + ".m_sitem n," + scm1 + ".M_GROUP o, " + scmf + ".M_RETDEB p, " + Environment.NewLine;

            sql += "(select a.barno, count(*) barimagecount," + Environment.NewLine;
            sql += "listagg(a.doc_flname||'~'||a.doc_desc,chr(179)) " + Environment.NewLine;
            sql += "within group (order by a.barno) as barimage from " + Environment.NewLine;
            sql += "(select a.barno, a.imgbarno, a.imgslno, b.doc_flname, b.doc_extn, b.doc_desc from " + Environment.NewLine;
            sql += "(select a.barno, a.barno imgbarno, a.slno imgslno " + Environment.NewLine;
            sql += "from " + scm1 + ".t_batch_img_hdr a " + Environment.NewLine;
            sql += "union " + Environment.NewLine;
            sql += "select a.barno, b.barno imgbarno, b.slno imgslno " + Environment.NewLine;
            sql += "from " + scm1 + ".t_batch_img_hdr_link a, " + scm1 + ".t_batch_img_hdr b " + Environment.NewLine;
            sql += "where a.mainbarno=b.barno(+) ) a, " + Environment.NewLine;
            sql += "" + scm1 + ".t_batch_img_hdr b " + Environment.NewLine;
            sql += "where a.imgbarno=b.barno(+) and a.imgslno=b.slno(+) ) a " + Environment.NewLine;
            sql += "group by a.barno ) y " + Environment.NewLine;

            sql += "where a.autono = b.autono(+) and a.slcd = c.slcd and g.conslcd = d.slcd(+) and a.autono = f.autono(+) and h.agslcd = l.slcd(+)  and h.sagslcd = m.slcd(+) " + Environment.NewLine;
            sql += "and f.translcd = e.slcd(+) and a.autono = f.autono(+) and a.autono = g.autono(+) and a.autono = h.autono(+) and  g.autono = i.autono(+) and a.doccd = j.doccd(+) and a.autono = k.autono(+) and b.itcd=n.itcd(+) and n.itgrpcd=o.itgrpcd(+) and i.rtdebcd=p.rtdebcd(+) and b.barno=y.barno(+) " + Environment.NewLine;

            if (SLCD.retStr() != "") sql += " and a.slcd in ('" + SLCD + "') " + Environment.NewLine;
            if (ITGRPCD.retStr() != "") sql += " and n.itgrpcd in (" + ITGRPCD + ") " + Environment.NewLine;
            if (ITCD.retStr() != "") sql += " and b.itcd in (" + ITCD + ") " + Environment.NewLine;


            //if (doctype != "") sql += " and j.doctype in(" + doctype + ") " + Environment.NewLine;
            sql += " and b.stkdrcr in ('D','C') " + Environment.NewLine;
            sql += ") a " + Environment.NewLine;
            //if (dtlsumm == "E")
            //{
            //    sql += "where nvl(a.cancel,'N')='N' " + Environment.NewLine;
            //}
            sql += "order by itstyle,itnm,itcd ";


            DataTable tbl = masterHelp.SQLquery(sql);
            return tbl;
        }

        //public ActionResult BSGroupCode(string val)
        //{
        //    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
        //    var query = (from c in DB1.MS_GLHDGRP where (c.GLHDGRPCD == val) select c);
        //    if (query.Any())
        //    {
        //        string str = "";
        //        foreach (var i in query)
        //        {
        //            str = i.GLHDGRPCD + Cn.GCS() + i.GLHDGRPNM;
        //        }
        //        return Content(str);
        //    }
        //    else
        //    {
        //        return Content("0");
        //    }
        //}
        //public ActionResult GetBSGroupCode()
        //{
        //    ImprovarDB DBIMP = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
        //    return PartialView("_Help2", masterHelp.GLHDGRPCD_help(DBIMP));
        //}
        //public ActionResult CreateMainGroup(string Gname, string Budgt, string BSCode, string Gdetails, string MGRPCD)
        //{
        //    BalanceSheetGroup VE = new BalanceSheetGroup();
        //    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
        //    if (VE.Add != "A")
        //    {
        //        return Content("You have no permission to create Group.Please add from next year!");
        //    }
        //    M_TGRP MTG = new M_TGRP();
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //    using (var transaction = DB.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            MTG.EMD_NO = 0;
        //            MTG.CLCD = CommVar.ClientCode(UNQSNO);
        //            MTG.MGRPCD = MGRPCD;
        //            var gcd = DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD).Select(a => a.ROOTCD).Max();
        //            if (gcd == null)
        //            {
        //                gcd = "1";
        //            }
        //            else
        //            {
        //                gcd = (Convert.ToInt32(gcd) + 1).ToString();
        //            }
        //            MTG.GRPSLNO = Convert.ToDecimal(gcd);
        //            MTG.GCD = gcd.PadLeft(6, '0');
        //            MTG.ROOTCD = MTG.GCD;
        //            MTG.GRPCDFULL = MTG.GCD;
        //            MTG.GRPNM = Gname;
        //            MTG.GRPNMDTL = Gdetails;
        //            MTG.BUDGTAMT = Budgt == "" ? 0 : Convert.ToDecimal(Budgt);
        //            MTG.GLHDGRPCD = BSCode;
        //            DB.M_TGRP.Add(MTG);
        //            DB.SaveChanges();
        //            transaction.Commit();
        //            if (Session["account_treeview"] != null)
        //            {
        //                Session.Remove("account_treeview");
        //            }
        //            return Content("0");
        //        }
        //        catch (Exception ex)
        //        {
        //            return Content(ex.Message);
        //        }
        //    }
        //}
        //public string GenerateTree()
        //{
        //    return null;
        //    //string liid = "";
        //    //if (Session["account_treeview"] != null)
        //    //{
        //    //    liid = Session["account_treeview"].ToString();
        //    //}
        //    //Hashtable Main_Menu;
        //    //Hashtable Main_Menu_Head;
        //    //ArrayList ManuLine = null;
        //    //string Child = null;
        //    //string ReParent = null;
        //    //int Reuse = 0;
        //    //Stack ST1 = new Stack();
        //    //var results = from row in TGRP
        //    //              where row.PARENTCD == null
        //    //              orderby row.GRPSLNO
        //    //              select row;
        //    //Main_Menu = new Hashtable();
        //    //Main_Menu_Head = new Hashtable();
        //    //ManuLine = new ArrayList();
        //    //foreach (var menu_row in results)
        //    //{
        //    //    bool autochk = false;
        //    //    string parent = menu_row.GRPCDFULL;
        //    //    string menuid_Child = menu_row.GCD;
        //    //    string mname = menu_row.GRPNM;
        //    //    string parentchild = parent;
        //    //    if (liid == parentchild)
        //    //    {
        //    //        autochk = true;
        //    //    }
        //    //    string syntax = "";
        //    //    if (autochk)
        //    //    {
        //    //        syntax = "checked='checked'";
        //    //    }
        //    //    string Menu = "<li>";
        //    //    //Menu = Menu + "<input type='checkbox' "+ syntax + "  id='" + parent + "'/>" + "<label class='tree_label' for='" + parent + "'><img src='../Image/Glow.png' class='groupimg'/>&nbsp;<span id='" + mname.Replace(' ', '_') + "'>" + mname + "<script>Rmenu('" + mname.Replace(' ', '_') + "','" + menuid_Child + "^" + parent + "',3,'" + parentchild + "');</script></span></label>";
        //    //    Menu = Menu + "<input type='checkbox' " + syntax + "  id='C" + parent + "'/>" + "<label class='tree_label' for='C" + parent + "'><img src='../Image/Glow.png' class='groupimg'/>&nbsp;<span id='" + parent + "'>" + mname + "<script>Rmenu('" + parent + "','" + menuid_Child + "^" + parent + "',3,'" + parentchild + "');</script></span></label>";
        //    //    Menu = Menu + "<ul>";
        //    //    Main_Menu.Add(parent, mname);
        //    //    ManuLine.Add(Menu);
        //    //    Temp_TGRP TTG = new Temp_TGRP();
        //    //    TTG.GCD = menu_row.GCD;
        //    //    TTG.GRPCDFULL = menu_row.GRPCDFULL;
        //    //    TTG.GRPNM = menu_row.GRPNM;
        //    //    TTG.GRPSLNO = Convert.ToInt32(menu_row.GRPSLNO);
        //    //    TTG.MGRPCD = menu_row.MGRPCD;
        //    //    TTG.PARENTCD = menu_row.PARENTCD;
        //    //    TTG.ROOTCD = menu_row.ROOTCD;
        //    //    TTG.Space = "";
        //    //    MLIST.Add(TTG);
        //    //    Main_Menu_Head.Add(parent, ManuLine.Count - 1);
        //    //    Child = menuid_Child;
        //    //    ReParent = parent;
        //    //    ST1.Push(menuid_Child + "." + Reuse + "." + ReParent);
        //    //    while (true)
        //    //    {
        //    //        var results1 =
        //    //        (from row in TGRP
        //    //         where row.PARENTCD == Child
        //    //         orderby row.GRPSLNO
        //    //         select row).ToList();
        //    //        if (results1.Any() == true)
        //    //        {
        //    //            for (int x = Reuse; x <= results1.Count() - 1; x++)
        //    //            {
        //    //                var boundTable = results1[x];
        //    //                string parent1 = boundTable.GRPCDFULL;
        //    //                string menuid_Child1 = boundTable.GCD;
        //    //                string mname1 = boundTable.GRPNM;
        //    //                string space = "";
        //    //                int sp_count = boundTable.GRPCDFULL.Length / 6;
        //    //                for (int p = 0; p <= sp_count - 1; p++)
        //    //                {
        //    //                    space = space + "&nbsp;&nbsp;&nbsp;&nbsp;";
        //    //                }
        //    //                string SubMenu = "<li>";
        //    //                SubMenu = SubMenu + "<input type='checkbox' " + syntax + "  id='C" + parent1 + "'/>" + "<label class='tree_label' for='C" + parent1 + "'><img src='../Image/Generic.png' class='groupimg'/>&nbsp;<span id='" + parent1 + "'>" + boundTable.GRPNM + "<script>Rmenu('" + parent1 + "','" + menuid_Child1 + "^" + parent1 + "',3,'" + parentchild + "');</script></span></label>";
        //    //                SubMenu = SubMenu + "<ul>";
        //    //                Main_Menu.Add(parent1, boundTable.GRPNM);
        //    //                ManuLine.Add(SubMenu);
        //    //                Temp_TGRP TTG1 = new Temp_TGRP();
        //    //                TTG1.GCD = boundTable.GCD;
        //    //                TTG1.GRPCDFULL = boundTable.GRPCDFULL;
        //    //                TTG1.GRPNM = boundTable.GRPNM;
        //    //                TTG1.GRPSLNO = Convert.ToInt32(boundTable.GRPSLNO);
        //    //                TTG1.MGRPCD = boundTable.MGRPCD;
        //    //                TTG1.PARENTCD = boundTable.PARENTCD;
        //    //                TTG1.ROOTCD = boundTable.ROOTCD;
        //    //                TTG1.Space = space;
        //    //                MLIST.Add(TTG1);
        //    //                Main_Menu_Head.Add(parent1, ManuLine.Count - 1);
        //    //                Child = menuid_Child1;
        //    //                ReParent = parent1;
        //    //                Reuse = 0;
        //    //                ST1.Push(menuid_Child1 + "." + Reuse + "." + ReParent);
        //    //                break;
        //    //            }
        //    //            if (Reuse > results1.Count() - 1)
        //    //            {
        //    //                if (ST1.Count > 0)
        //    //                {
        //    //                    ST1.Pop();
        //    //                    ManuLine.Add("</ul> </li>");
        //    //                    if (ST1.Count == 0)
        //    //                    {
        //    //                        Reuse = 0;
        //    //                        break;
        //    //                    }
        //    //                    string str = ST1.Pop().ToString();
        //    //                    string[] getParent;
        //    //                    getParent = str.Split('.');
        //    //                    Child = getParent[0];
        //    //                    ReParent = getParent[2];
        //    //                    Reuse = Convert.ToInt32(getParent[1]);
        //    //                    Reuse += 1;
        //    //                    ST1.Push(Child + "." + Reuse + "." + ReParent);
        //    //                }
        //    //                else
        //    //                {
        //    //                    break;
        //    //                }
        //    //            }
        //    //        }
        //    //        else
        //    //        {
        //    //            if (ST1.Count > 0)
        //    //            {
        //    //                string Recall = ST1.Peek().ToString();
        //    //                string[] RecallDetails;
        //    //                RecallDetails = Recall.Split('.');
        //    //                int index = (int)Main_Menu_Head[RecallDetails[2]];
        //    //                string Manuname = (string)Main_Menu[RecallDetails[2]];
        //    //                string Controller = "";
        //    //                string SubMenu = " <li><span class='tree_label'>";
        //    //                if (RecallDetails[0] == RecallDetails[2])  //+ RecallDetails[2] is fullgrpcd
        //    //                {
        //    //                    SubMenu = SubMenu + "<img src='../Image/Glow.png' class='groupimg'/>&nbsp;<span id='" + RecallDetails[2] + "' onclick=ExistTag('" + RecallDetails[0] + "^" + RecallDetails[2] + "');>" + Manuname + "<script>Rmenu('" + RecallDetails[2] + "','" + RecallDetails[0] + "^" + RecallDetails[2] + "',2,'" + parentchild + "');</script></span>&nbsp;";
        //    //                }
        //    //                else
        //    //                {
        //    //                    SubMenu = SubMenu + "<img src='../Image/Generic.png' class='groupimg'/>&nbsp;<span id='" + RecallDetails[2] + "' onclick=ExistTag('" + RecallDetails[0] + "^" + RecallDetails[2] + "');>" + Manuname + "<script>Rmenu('" + RecallDetails[2] + "','" + RecallDetails[0] + "^" + RecallDetails[2] + "',2,'" + parentchild + "');</script></span>&nbsp;";
        //    //                }
        //    //                SubMenu = SubMenu + "</span></li>";
        //    //                ManuLine.RemoveAt(index);
        //    //                ManuLine.Insert(index, SubMenu);
        //    //                ST1.Pop();
        //    //                if (ST1.Count == 0)
        //    //                {
        //    //                    Reuse = 0;
        //    //                    break;
        //    //                }
        //    //                string str = ST1.Pop().ToString();
        //    //                string[] getParent;
        //    //                getParent = str.Split('.');
        //    //                Child = getParent[0];
        //    //                ReParent = getParent[2];
        //    //                Reuse = Convert.ToInt32(getParent[1]);
        //    //                Reuse += 1;
        //    //                ST1.Push(Child + "." + Reuse + "." + ReParent);
        //    //            }
        //    //            else
        //    //            {
        //    //                break;
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //    //System.Text.StringBuilder SB = new System.Text.StringBuilder();
        //    //if (ManuLine != null)
        //    //{
        //    //    for (int i = 0; i <= ManuLine.Count - 1; i++)
        //    //    {
        //    //        SB.Append(ManuLine[i].ToString());
        //    //    }
        //    //}
        //    //return SB.ToString();
        //}
        //public ActionResult CreateSubGroup(string Gname, string Budgt, string Gdetails, string MGRPCD, string Parent, string Liid, string legdtlskp, string schdl)
        //{
        //    M_TGRP MTG = new M_TGRP();
        //    string[] Parameter = Parent.Split('^');
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //    using (var transaction = DB.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            MTG.EMD_NO = 0;
        //            MTG.CLCD = CommVar.ClientCode(UNQSNO);
        //            MTG.MGRPCD = MGRPCD;
        //            string Gcd = Parameter[0];
        //            var Rootcd = DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD && a.GCD == Gcd).Select(a => a.ROOTCD).FirstOrDefault();
        //            var gcd = DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD && a.ROOTCD == Rootcd && a.PARENTCD == Gcd).Select(a => a.GRPSLNO).Max();
        //            var CODE = DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD && a.PARENTCD != null).Select(a => a.GCD).Max();
        //            if (gcd == null)
        //            {
        //                gcd = 1;
        //            }
        //            else
        //            {
        //                gcd = gcd + 1;
        //            }
        //            int temp = 0;
        //            if (CODE == null)
        //            {
        //                temp = 1;
        //            }
        //            else
        //            {
        //                temp = Convert.ToInt32(CODE.Substring(1)) + 1;
        //            }
        //            MTG.GRPSLNO = gcd;
        //            MTG.GCD = "S" + temp.ToString().PadLeft(5, '0');
        //            MTG.ROOTCD = Rootcd;
        //            MTG.PARENTCD = Gcd;
        //            MTG.GRPCDFULL = Parameter[1] + MTG.GCD;
        //            MTG.GRPNM = Gname.Replace("'", "`");
        //            MTG.GRPNMDTL = Gdetails;
        //            MTG.BUDGTAMT = Budgt == "" ? 0 : Convert.ToDecimal(Budgt);
        //            MTG.LEGDTLSKP = legdtlskp == "true" ? "Y" : "";
        //            MTG.SCHDL = schdl;
        //            DB.M_TGRP.Add(MTG);
        //            DB.SaveChanges();
        //            transaction.Commit();
        //            if (Session["account_treeview"] == null)
        //            {
        //                Session.Add("account_treeview", Liid);
        //            }
        //            else
        //            {
        //                Session["account_treeview"] = Liid;
        //            }
        //            return Content("0");
        //        }
        //        catch (Exception ex)
        //        {
        //            return Content(ex.Message);
        //        }
        //    }
        //}
        //public ActionResult GetGRPCDDTL(string Parent)
        //{
        //    try
        //    {
        //        if (Parent.retStr() != "")
        //        {
        //            string[] Parameter = Parent.Split('^');
        //            if (Parameter.Length > 1)
        //            {
        //                var GRPCDFULL = Parameter[1];
        //                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //                //ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), CommVar.CommSchema());
        //                var M_SUBLEGGRPlst = DB.M_TGRP.Where(D => D.GRPCDFULL == GRPCDFULL).ToList().Take(1);
        //                var str = masterHelp.ToReturnFieldValues(M_SUBLEGGRPlst, null);
        //                return Content(str);
        //            }
        //        }
        //        return Content("No Parent Found");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content(ex.Message);
        //    }
        //}
        //public ActionResult RenameGroup(string Gname, string MGRPCD, string Parent, string Details, string Budgt, string Liid, string legdtlskp, string schdl)
        //{
        //    M_TGRP MTG = new M_TGRP();
        //    string[] Parameter = Parent.Split('^');
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //    decimal B_amt = Budgt == "" ? 0 : Convert.ToDecimal(Budgt);
        //    using (var transaction = DB.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            string Gcd = Parameter[0];
        //            string gfull = Parameter[1];
        //            var emd = DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD && a.GCD == Gcd && a.GRPCDFULL == gfull).Select(a => a.EMD_NO).SingleOrDefault();
        //            emd += 1;
        //            DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD && a.GCD == Gcd && a.GRPCDFULL == gfull).ToList().ForEach(x =>
        //            {
        //                x.GRPNM = Gname.Replace("'", "`");
        //                x.EMD_NO = emd; x.GRPNMDTL = Details; x.BUDGTAMT = B_amt; x.LEGDTLSKP = legdtlskp == "true" ? "Y" : ""; x.SCHDL = schdl;
        //            });
        //            DB.SaveChanges();
        //            transaction.Commit();
        //            if (Session["account_treeview"] == null)
        //            {
        //                Session.Add("account_treeview", Liid);
        //            }
        //            else
        //            {
        //                Session["account_treeview"] = Liid;
        //            }
        //            return Content("0");
        //        }
        //        catch (Exception ex)
        //        {
        //            return Content(ex.Message);
        //        }
        //    }
        //}
        //public ActionResult DeleteGroup(string MGRPCD, string Parent, string Liid)
        //{
        //    M_TGRP MTG = new M_TGRP();
        //    string[] Parameter = Parent.Split('^');
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //    using (var transaction = DB.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            string Gcd = Parameter[0];
        //            string gfull = Parameter[1];
        //            List<String> GetGCD = GetHIRELINK(Gcd, CommVar.CurSchema(UNQSNO), MGRPCD);
        //            var AR = GetGCD.Distinct().ToList();
        //            DB.M_TGRP.RemoveRange(DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD && AR.Contains(a.GCD)));
        //            DB.SaveChanges();
        //            transaction.Commit();
        //            if (Session["account_treeview"] == null)
        //            {
        //                Session.Add("account_treeview", Liid);
        //            }
        //            else
        //            {
        //                Session["account_treeview"] = Liid;
        //            }
        //            return Content("0");
        //        }
        //        catch (Exception ex)
        //        {
        //            return Content(ex.Message);
        //        }
        //    }
        //}
        //public string urlRenameValue(string code)
        //{
        //    try
        //    {
        //        string url = "";
        //        var PreviousUrl = System.Web.HttpContext.Current.Request.UrlReferrer.AbsoluteUri;
        //        var uri = new Uri(PreviousUrl);//Create Virtually Query String
        //        var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
        //        if (queryString.Get("MGRPCD") == null)
        //        {
        //            url = Request.UrlReferrer.ToString() + "&MGRPCD=" + code;
        //        }
        //        else
        //        {
        //            string dd = Request.UrlReferrer.ToString();
        //            int pos = Request.UrlReferrer.ToString().IndexOf("&MGRPCD=");
        //            url = dd.Substring(0, pos);
        //            url = url + "&MGRPCD=" + code;
        //        }
        //        return url;
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}
        //public ActionResult IndexGroup(BalanceSheetGroup VE, string MGRPCD)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //    using (var transaction = DB.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            foreach (var i in VE.MLIST)
        //            {
        //                DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD && a.GCD == i.GCD && a.GRPCDFULL == i.GRPCDFULL && a.ROOTCD == i.ROOTCD).ToList().ForEach(x => { x.GRPSLNO = i.GRPSLNO; });
        //            }
        //            DB.SaveChanges();
        //            transaction.Commit();
        //            if (Session["account_treeview"] != null)
        //            {
        //                Session.Remove("account_treeview");
        //            }
        //            return Content("0");
        //        }
        //        catch (Exception ex)
        //        {
        //            return Content(ex.Message);
        //        }
        //    }
        //}
        //public ActionResult ADDACCOUNT(BalanceSheetGroup VE, string MGRPCD, string Parent, string Liid)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //    string[] Parameter = Parent.Split('^');
        //    List<M_TGRP> MTG1 = new List<M_TGRP>();
        //    string Gcd = Parameter[0];
        //    string gfull = Parameter[1];
        //    using (var transaction = DB.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            var temmp = DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD && a.GCD == Gcd && a.GRPCDFULL == gfull).Select(a => a).SingleOrDefault();
        //            foreach (var i in VE.AvailableGroup)
        //            {
        //                if (i.Checked)
        //                {
        //                    M_TGRP MTG = new M_TGRP();
        //                    MTG.BUDGTAMT = temmp.BUDGTAMT;
        //                    MTG.CLCD = temmp.CLCD;
        //                    MTG.DTAG = temmp.DTAG;
        //                    MTG.EMD_NO = temmp.EMD_NO;
        //                    MTG.GCD = temmp.GCD;
        //                    MTG.GLHDGRPCD = temmp.GLHDGRPCD;
        //                    MTG.GRPNM = temmp.GRPNM;
        //                    MTG.GRPNMDTL = temmp.GRPNMDTL;
        //                    MTG.MGRPCD = MGRPCD;
        //                    MTG.PARENTCD = temmp.PARENTCD;
        //                    MTG.ROOTCD = temmp.ROOTCD;
        //                    MTG.GLCD = i.GLCD;
        //                    MTG.CLASS1CD = i.CLASS1CD;
        //                    MTG.GRPCDFULL = gfull + i.GLCD + i.CLASS1CD;
        //                    MTG.GRPSLNO = temmp.GRPSLNO;
        //                    MTG.LEGDTLSKP = temmp.LEGDTLSKP;
        //                    MTG.SCHDL = "Y";
        //                    MTG1.Add(MTG);
        //                }
        //            }
        //            DB.M_TGRP.AddRange(MTG1);
        //            DB.SaveChanges();
        //            transaction.Commit();
        //            if (Session["account_treeview"] == null)
        //            {
        //                Session.Add("account_treeview", Liid);
        //            }
        //            else
        //            {
        //                Session["account_treeview"] = Liid;
        //            }
        //            return Content("0");
        //        }
        //        catch (Exception ex)
        //        {
        //            return Content(ex.Message);
        //        }
        //    }
        //}
        //public ActionResult ExistingAccount(BalanceSheetGroup VE, string MGRPCD, string Parent)
        //{
        //    try
        //    {
        //        string[] Parameter = Parent.Split('^');
        //        string Gcd = Parameter[0];
        //        string gfull = Parameter[1];
        //        ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //        string scmf = CommVar.FinSchema(UNQSNO);
        //        DataTable dt = new DataTable();
        //        var Mtype = DB.M_TMGRP.Where(a => a.MGRPCD == MGRPCD).Select(a => a.MGRPTYPE).SingleOrDefault();
        //        if (Mtype == "SL")
        //        {
        //            //string str = "select * from (select glcd,'' slcd,'' class1cd,glnm,'' slnm,'' class1nm from " + CommVar.CurSchema(UNQSNO) + ".m_genleg where nvl(slcdmust,'N') = 'N' union all ";
        //            //str = str + " select a.glcd,b.slcd,'' class1cd,a.glnm,c.slnm,'' class1nm from " + CommVar.CurSchema(UNQSNO) + ".m_genleg a, " + CommVar.CurSchema(UNQSNO) + ".m_subleg_gl b, " + CommVar.CurSchema(UNQSNO) + ".m_subleg c where a.glcd = b.glcd ";
        //            //str = str + " and nvl(slcdmust,'N') = 'Y' and b.slcd = c.slcd) where glcd||slcd||class1cd in (select glcd||slcd||class1cd from " + CommVar.CurSchema(UNQSNO) + ".m_tgrp where mgrpcd = '" + MGRPCD + "' and gcd='" + Gcd + "'  and glcd||slcd||class1cd is not null)";
        //            //DataTable dt = masterHelp.SQLquery(str);
        //            //VE.ExistingGroup = (from i in dt.AsEnumerable()
        //            //                     select new AvailableACGroup()
        //            //                     {
        //            //                         Checked = false,
        //            //                         CLASS1CD = i.Field<string>("class1cd"),
        //            //                         CLASS1NM = i.Field<string>("class1nm"),
        //            //                         GLCD = i.Field<string>("glcd"),
        //            //                         GLNM = i.Field<string>("glnm"),
        //            //                         SLCD = i.Field<string>("slcd"),
        //            //                         SLNM = i.Field<string>("slnm"),
        //            //                     }).ToList();
        //        }
        //        else if (Mtype == "GL")
        //        {
        //            string str = "select glcd, '' class1cd, glnm, '' class1nm from " + scmf + ".m_genleg where glcd in (select glcd from " + scmf + ".m_tgrp where mgrpcd = '" + MGRPCD + "' and gcd='" + Gcd + "'  and glcd is not null) order by glnm ";
        //            dt = masterHelp.SQLquery(str);
        //        }
        //        else if (Mtype == "CL" || Mtype == "SL")
        //        {
        //            string str = "select * from (select glcd, '' class1cd, glnm, '' class1nm from " + scmf + ".m_genleg where nvl(class1cdmust,'N') = 'N' union all ";
        //            str = str + "select distinct a.glcd, b.class1cd, c.glnm, e.class1nm from " + scmf + ".t_vch_det a, " + scmf + ".t_vch_class b, " + scmf + ".m_genleg c, ";
        //            str = str + scmf + ".m_class1 e where a.autono = b.autono and a.slno = b.dslno and a.glcd = c.glcd and b.class1cd = e.class1cd and nvl(c.class1cdmust, 'N')= 'Y' order by glcd,class1cd) ";
        //            str = str + "where glcd|| class1cd in (select glcd || class1cd from " + scmf + ".m_tgrp where mgrpcd = '" + MGRPCD + "' and gcd='" + Gcd + "'  and glcd|| class1cd is not null)";
        //            dt = masterHelp.SQLquery(str);
        //        }
        //        VE.ExistingGroup = (from i in dt.AsEnumerable()
        //                            select new AvailableACGroup()
        //                            {
        //                                Checked = false,
        //                                CLASS1CD = i.Field<string>("class1cd"),
        //                                CLASS1NM = i.Field<string>("class1nm"),
        //                                GLCD = i.Field<string>("glcd"),
        //                                GLNM = i.Field<string>("glnm"),
        //                            }).ToList();
        //        ModelState.Clear();
        //        return PartialView("_tree_ExistingTag", VE);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content("0^" + ex.Message);
        //    }
        //}
        //public ActionResult DELETEACCOUNT(BalanceSheetGroup VE, string MGRPCD, string Parent, string Liid)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
        //    string[] Parameter = Parent.Split('^');
        //    string Gcd = Parameter[0];
        //    string gfull = Parameter[1];
        //    using (var transaction = DB.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            foreach (var i in VE.ExistingGroup)
        //            {
        //                if (i.Checked)
        //                {
        //                    DB.M_TGRP.RemoveRange(DB.M_TGRP.Where(a => a.MGRPCD == MGRPCD && a.GCD == Gcd && a.GLCD == i.GLCD));
        //                }
        //            }
        //            DB.SaveChanges();
        //            transaction.Commit();
        //            if (Session["account_treeview"] == null)
        //            {
        //                Session.Add("account_treeview", Liid);
        //            }
        //            else
        //            {
        //                Session["account_treeview"] = Liid;
        //            }
        //            return Content("0");
        //        }
        //        catch (Exception ex)
        //        {
        //            return Content(ex.Message);
        //        }
        //    }
        //}
        //public List<string> GetHIRELINK(string EGCD, string SCHEMA, string MGRPCD)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), SCHEMA);
        //    List<String> AR = new List<String>();
        //    AR.Add(EGCD);
        //    System.Collections.Stack STK = new System.Collections.Stack();
        //    var EMPLO = (from i in DB.M_TGRP where (i.PARENTCD == EGCD && i.MGRPCD == MGRPCD) orderby i.GRPSLNO select i.GCD).ToList();
        //    for (int i = 0; i <= EMPLO.Count() - 1; i++)
        //    {
        //        string EMPCD2 = EMPLO[i];
        //        AR.Add(EMPCD2);
        //        STK.Push(EMPCD2);
        //        while (true)
        //        {
        //            string Qid = (string)STK.Pop();
        //            var EMPLO1 = (from q in DB.M_TGRP where (q.PARENTCD == Qid && q.MGRPCD == MGRPCD) orderby q.GRPSLNO select q.GCD).ToList();
        //            if (EMPLO1.Any() == false)
        //            {
        //                if (STK.Count <= 0)
        //                {
        //                    break;
        //                }
        //            }
        //            else
        //            {
        //                for (int x = 0; x <= EMPLO1.Count() - 1; x++)
        //                {
        //                    STK.Push(EMPLO1[x]);
        //                    AR.Add(EMPLO1[x]);
        //                }
        //            }
        //        }
        //    }
        //    return AR;
        //}
        //public ActionResult DownloadExcel(string MGRPCD)
        //{
        //    try
        //    {
        //        ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
        //        string MGRPNM = DB.M_TMGRP.Where(a => a.MGRPCD == MGRPCD).FirstOrDefault()?.MGRPNM;
        //        DataTable dt = new DataTable();
        //        dt.Columns.Add("GrpHdr1", typeof(string));
        //        dt.Columns.Add("GrpHdr2", typeof(string));
        //        dt.Columns.Add("GrpHdr3", typeof(string));
        //        dt.Columns.Add("GrpHdr4", typeof(string));
        //        dt.Columns.Add("GrpHdr5", typeof(string));
        //        dt.Columns.Add("LegerName", typeof(string));
        //        dt.Columns.Add("GLCD", typeof(string));
        //        //dt.Columns.Add("slcdgrpnmdesc", typeof(string));
        //        string str = "";

        //        str = " select grpcdfull,";
        //        str += " a.slcd,CASE WHEN nvl(a.slcd, ' ') = ' ' THEN A.GRPNM  else B.SLNM end SLNM,";
        //        str += " C.GLCD,CASE WHEN nvl(a.GLCD, ' ') = ' ' THEN A.GRPNM  else C.GLNM end GRPNM";
        //        str += " from " + CommVar.FinSchema(UNQSNO) + ".M_tGRP a, " + CommVar.FinSchema(UNQSNO) + ".m_subleg b, " + CommVar.FinSchema(UNQSNO) + ".m_GENLEG C ";
        //        str += " where A.SLCD = b.slcd(+) and A.GLCD = C.GLCD(+) AND a.mgrpcd = '" + MGRPCD + "'";
        //        str += " order by grpcdfull,grpslno";
        //        var tmpdt = masterHelp.SQLquery(str);
        //        foreach (DataRow dr in tmpdt.Rows)
        //        {
        //            DataRow newdr = dt.NewRow();
        //            string grpcdfull = dr["grpcdfull"].ToString();
        //            string glcd = dr["GLCD"].ToString();
        //            if (glcd == "")
        //            {
        //                if (grpcdfull.Length % 2 != 0 && glcd != "") grpcdfull = grpcdfull.Substring(0, grpcdfull.Length - 8);

        //                int exlColIndex = grpcdfull.Length / 6;
        //                if (exlColIndex <= 5)
        //                {
        //                    newdr["GrpHdr" + exlColIndex] = dr["GRPNM"];
        //                }
        //                else
        //                {
        //                    newdr["GrpHdr5"] = dr["GRPNM"];
        //                }
        //            }
        //            else
        //            {
        //                newdr["LegerName"] = dr["GRPNM"].ToString();
        //            }
        //            newdr["GLCD"] = dr["GLCD"];
        //            dt.Rows.Add(newdr);
        //        }
        //        using (ExcelPackage pck = new ExcelPackage())
        //        {
        //            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Sheet1");
        //            for (int q = 1; q <= 5; q++)
        //            {
        //                ws.Column(q).Width = 2.82;
        //                ws.Column(q).Style.Font.Bold = true;
        //            }
        //            ws.Column(6).Width = 35;
        //            ws.Cells[1, 1].Value = MGRPNM + " analysis as on " + System.DateTime.Now.retDateStr();
        //            ws.Cells["A2"].LoadFromDataTable(dt, true);
        //            Byte[] fileBytes = pck.GetAsByteArray();
        //            Response.ClearContent();
        //            Response.AddHeader("content-disposition", "attachment;filename=" + MGRPNM + " analysis " + DateTime.Now.ToString("dd_HHmm") + ".xlsx");
        //            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        //            Response.BinaryWrite(fileBytes);
        //            Response.End();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content(ex.Message);
        //    }
        //    return Content("Excel Downloded.");
        //}
        //public ActionResult SavePreviousYrData(BalanceSheetGroup VE, FormCollection FC)
        //{
        //    CS = Cn.GetConnectionString();
        //    Cn.con = new OracleConnection(CS);
        //    if ((Cn.ds.Tables["fill_RECORD"] == null) == false)
        //    {
        //        Cn.ds.Tables["fill_RECORD"].Clear();
        //    }
        //    if (Cn.con.State == ConnectionState.Closed)
        //    {
        //        Cn.con.Open();
        //    }
        //    var transaction = Cn.con.BeginTransaction();
        //    try
        //    {
        //        string PrvYrScm = CommVar.FinSchemaPrevYr(UNQSNO);
        //        string Scm = CommVar.CurSchema(UNQSNO);
        //        string sql1 = "select a.glcd,b.glnm from  " + Scm + ".m_tgrp a," + Scm + ".m_genleg b where a.glcd=b.glcd and ";
        //        sql1 += " a.glcd  in( ";
        //        sql1 += "select glcd from " + Scm + ".m_genleg where glcd not in (select glcd from " + PrvYrScm + ".m_genleg )) ";
        //        DataTable MDT = masterHelp.SQLquery(sql1);
        //        if (MDT.Rows.Count > 0) { transaction.Rollback(); return Content(""+MDT.Rows[0]["glnm"].retStr()+"("+MDT.Rows[0]["glcd"].retStr()+") not found at the last year !!"); }
        //        string sql = " delete from  " + PrvYrScm + ".m_tgrp ";
        //        Cn.com = new OracleCommand(sql, Cn.con);
        //        Cn.com.ExecuteNonQuery();
        //        string sqll = " insert into " + PrvYrScm + ".m_tgrp ";
        //        sqll += "(select * from " + Scm + ".m_tgrp where glcd in (select glcd from " + PrvYrScm + ".m_genleg ) or glcd is null) ";
        //        //sqll += "(select * from " + Scm + ".m_tgrp where glcd  in( ";
        //        //sqll += "select glcd from " + Scm + ".m_genleg where glcd not in (select glcd from " + PrvYrScm + ".m_genleg ))) ";

        //        Cn.com = new OracleCommand(sqll, Cn.con);
        //        Cn.com.ExecuteNonQuery();
        //        ModelState.Clear();
        //        transaction.Commit();
        //        return Content("1");
        //    }

        //    catch (Exception ex)
        //    {
        //        transaction.Rollback();
        //        ModelState.Clear();
        //        Cn.SaveException(ex, "");
        //        return Content(ex.Message + " " + ex.InnerException);
        //    }

        //}
    }
}