using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Data.Entity.Validation;

namespace Improvar.Controllers
{
    public class M_SysCnfgController : Controller
    {
        Connection Cn = new Connection();
        string CS = null;
        M_SYSCNFG sl;
        M_CNTRL_HDR sll;
        M_BRAND sBRND;
        M_PRODGRP sPROD;
        MasterHelp Master_Help = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_SysCnfg

        public ActionResult M_SysCnfg(FormCollection FC, string op = "", string key = "", int Nindex = 0, string searchValue = "", string loadItem = "N")
        {
            //testing  
            try
            {//test
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Posting & Terms Setup";
                    string loca1 = CommVar.Loccd(UNQSNO).Trim();
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    SysCnfgMasterEntry VE = new SysCnfgMasterEntry();

                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();

                    //=================For WP/rp Price Gen================//
                    List<DropDown_list1> GT = new List<DropDown_list1>();
                    DropDown_list1 GT1 = new DropDown_list1();
                    GT1.text = "ROUND";
                    GT1.value = "RD";
                    GT.Add(GT1);
                    DropDown_list1 GT2 = new DropDown_list1();
                    GT2.text = "ROUNDNEXT";
                    GT2.value = "RN";
                    GT.Add(GT2);
                    DropDown_list1 GT3 = new DropDown_list1();
                    GT3.text = "NEAR";
                    GT3.value = "NR";
                    GT.Add(GT3);
                    DropDown_list1 GT4 = new DropDown_list1();
                    GT4.text = "NEXT";
                    GT4.value = "NT";
                    GT.Add(GT4);
                    VE.DropDown_list1 = GT;
                    //=================End WP/rp Price Gen ================//

                    //=================For Due Date Calc on ================//
                    List<DropDown_list3> list2 = new List<DropDown_list3>();
                    DropDown_list3 obj5 = new DropDown_list3();
                    obj5.text = "Bill Date";
                    obj5.value = "";
                    list2.Add(obj5);
                    DropDown_list3 obj6 = new DropDown_list3();
                    obj6.text = "LR Date";
                    obj6.value = "R";
                    list2.Add(obj6);
                    VE.DropDown_list3 = list2;
                    //=================End  Due Date Calc on ================//
                    VE.DefaultAction = op;
                    string GCS = Cn.GCS();
                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_SYSCNFG
                                       orderby p.M_AUTONO
                                       select new { M_AUTONO = p.M_AUTONO }).Select(a => new
                                     IndexKey()
                                       { Navikey = a.M_AUTONO.ToString() }).ToList();
                        if (searchValue != "") { Nindex = VE.IndexKey.FindIndex(r => r.Navikey.Equals(searchValue)); }

                        if (op == "E" || op == "D" || op == "V" || loadItem == "Y")
                        {
                            VE.Searchpannel_State = true;
                            if (searchValue.Length != 0)
                            {
                                VE.Index = Nindex;
                                VE = Navigation(VE, DB, 0, searchValue);
                            }
                            else
                            {
                                if (key == "F")
                                {
                                    VE.Index = 0;
                                    VE = Navigation(VE, DB, 0, searchValue);
                                }
                                else if (key == "" || key == "L")
                                {
                                    VE.Index = VE.IndexKey.Count - 1;
                                    VE = Navigation(VE, DB, VE.IndexKey.Count - 1, searchValue);
                                }
                                else if (key == "P")
                                {
                                    Nindex -= 1;
                                    if (Nindex < 0)
                                    {
                                        Nindex = 0;
                                    }
                                    VE.Index = Nindex;
                                    VE = Navigation(VE, DB, Nindex, searchValue);
                                }
                                else if (key == "N")
                                {
                                    Nindex += 1;
                                    if (Nindex > VE.IndexKey.Count - 1)
                                    {
                                        Nindex = VE.IndexKey.Count - 1;
                                    }
                                    VE.Index = Nindex;
                                    VE = Navigation(VE, DB, Nindex, searchValue);
                                }
                            }
                            VE.M_SYSCNFG = sl;
                            VE.M_CNTRL_HDR = sll;
                        }
                        if (op.ToString() == "A" && loadItem == "N")
                        {
                            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                            var compcd = CommVar.Compcd(UNQSNO);
                            M_SYSCNFG msyscnfg = new M_SYSCNFG();
                            var chkdata = (from i in DB.M_SYSCNFG
                                           join j in DB.M_CNTRL_HDR on i.M_AUTONO equals j.M_AUTONO
                                           select new
                                           {
                                               i
                                          .M_AUTONO,
                                               i.EFFDT,
                                               i.EMD_NO,
                                               i.COMPCD,
                                               i.SALDEBGLCD,
                                               i.PURDEBGLCD,
                                               i.CLASS1CD,
                                               i.RETDEBSLCD,
                                               i.DUEDATECALCON,
                                               i.WPPER,
                                               i.RPPER,
                                               i.PRICEINCODE,
                                               i.PRICEINCODECOST,
                                               i.RTDEBCD,
                                               i.DESIGNPATH,
                                               i.INC_RATE,
                                               i.MNTNSIZE,
                                               i.MNTNCOLOR,
                                               i.MNTNPART,
                                               i.MNTNFLAGMTR,
                                               i.MNTNLISTPRICE,
                                               i.MNTNSHADE,
                                               i.MNTNDISC1,
                                               i.MNTNDISC2,
                                               i.MNTNWPRPPER,
                                               i.MNTNBALE,
                                               i.MNTNOURDESIGN,
                                               i.MNTNPCSTYPE,
                                               i.MNTNBARNO,
                                               i.COMMONUNIQBAR,
                                               i.WPPRICEGEN,
                                               i.RPPRICEGEN,
                                               j.INACTIVE_TAG,
                                               i.INSPOLDESC,
                                               i.CMROFFTYPE,
                                               i.CMCASHRECDAUTO,
                                               i.SHORTAGE_GLCD,
                                               i.MERGEINDTL,
                                           }).OrderByDescending(a => a.M_AUTONO).FirstOrDefault();
                            if (chkdata != null)
                            {
                                msyscnfg.EFFDT = chkdata.EFFDT;
                                msyscnfg.M_AUTONO = chkdata.M_AUTONO;
                                msyscnfg.EMD_NO = chkdata.EMD_NO;
                                msyscnfg.COMPCD = chkdata.COMPCD;
                                msyscnfg.SALDEBGLCD = chkdata.SALDEBGLCD;
                                msyscnfg.PURDEBGLCD = chkdata.PURDEBGLCD;
                                msyscnfg.CLASS1CD = chkdata.CLASS1CD;
                                msyscnfg.RETDEBSLCD = chkdata.RETDEBSLCD;
                                msyscnfg.DUEDATECALCON = chkdata.DUEDATECALCON;
                                msyscnfg.WPPER = chkdata.WPPER;
                                msyscnfg.RPPER = chkdata.RPPER;
                                msyscnfg.PRICEINCODE = chkdata.PRICEINCODE;
                                msyscnfg.PRICEINCODECOST = chkdata.PRICEINCODECOST;
                                msyscnfg.RTDEBCD = chkdata.RTDEBCD;
                                msyscnfg.DESIGNPATH = chkdata.DESIGNPATH;
                                msyscnfg.INSPOLDESC = chkdata.INSPOLDESC;
                                msyscnfg.SHORTAGE_GLCD = chkdata.SHORTAGE_GLCD;
                                if (msyscnfg.CLASS1CD != null)
                                {
                                    var classnm = (from a in DBF.M_CLASS1 where a.CLASS1CD == msyscnfg.CLASS1CD select new { a.CLASS1NM }).FirstOrDefault();
                                    VE.CLASS1NM = classnm.CLASS1NM;
                                }
                                if (msyscnfg.SALDEBGLCD != null)
                                {
                                    var salglnm = (from a in DBF.M_GENLEG where a.GLCD == msyscnfg.SALDEBGLCD select new { a.GLNM }).FirstOrDefault();
                                    VE.SALDEBGLNM = salglnm.GLNM;
                                }
                                if (msyscnfg.PURDEBGLCD != null)
                                {
                                    var purglnm = (from a in DBF.M_GENLEG where a.GLCD == msyscnfg.PURDEBGLCD select new { a.GLNM }).FirstOrDefault();
                                    VE.PURDEBGLNM = purglnm.GLNM;
                                }
                                if (msyscnfg.RETDEBSLCD != null)
                                {
                                    var salretglnm = (from a in DBF.M_SUBLEG where a.SLCD == msyscnfg.RETDEBSLCD select new { a.SLNM }).FirstOrDefault();
                                    VE.RETDEBSLNM = salretglnm.SLNM;
                                }
                                if (msyscnfg.RTDEBCD != null)
                                { var Party = DBF.M_RETDEB.Find(msyscnfg.RTDEBCD); if (Party != null) { VE.RTDBNM = Party.RTDEBNM; } }
                                if (chkdata.INC_RATE == "Y") { VE.INC_RATE = true; } else { VE.INC_RATE = false; }
                                if (chkdata.MNTNSIZE == "Y") { VE.MNTNSIZE = true; } else { VE.MNTNSIZE = false; }
                                if (chkdata.MNTNCOLOR == "Y") { VE.MNTNCOLOR = true; } else { VE.MNTNCOLOR = false; }
                                if (chkdata.MNTNPART == "Y") { VE.MNTNPART = true; } else { VE.MNTNPART = false; }
                                if (chkdata.MNTNFLAGMTR == "Y") { VE.MNTNFLAGMTR = true; } else { VE.MNTNFLAGMTR = false; }
                                if (chkdata.MNTNLISTPRICE == "Y") { VE.MNTNLISTPRCE = true; } else { VE.MNTNLISTPRCE = false; }
                                if (chkdata.MNTNSHADE == "Y") { VE.MNTNSHADE = true; } else { VE.MNTNSHADE = false; }
                                if (chkdata.MNTNDISC1 == "Y") { VE.MNTNDISC1 = true; } else { VE.MNTNDISC1 = false; }
                                if (chkdata.MNTNDISC2 == "Y") { VE.MNTNDISC2 = true; } else { VE.MNTNDISC2 = false; }
                                if (chkdata.MNTNWPRPPER == "Y") { VE.MNTNWPRPPER = true; } else { VE.MNTNWPRPPER = false; }
                                if (chkdata.MNTNBALE == "Y") { VE.MNTNBALE = true; } else { VE.MNTNBALE = false; }
                                if (chkdata.MNTNOURDESIGN == "Y") { VE.MNTNOURDESIGN = true; } else { VE.MNTNOURDESIGN = false; }
                                if (chkdata.MNTNPCSTYPE == "Y") { VE.MNTNPCSTYPE = true; } else { VE.MNTNPCSTYPE = false; }
                                if (chkdata.MNTNBARNO == "Y") { VE.MNTNBARNO = true; } else { VE.MNTNBARNO = false; }
                                if (chkdata.COMMONUNIQBAR == "E") { VE.COMMONUIQBAR = true; } else { VE.COMMONUIQBAR = false; }

                                if (chkdata.WPPRICEGEN != null)
                                {
                                    VE.WPPRICEGENCD = chkdata.WPPRICEGEN.Substring(0, 2);
                                    VE.WPPRICEGENAMT = chkdata.WPPRICEGEN.Substring(2, 2);
                                }
                                if (chkdata.RPPRICEGEN != null)
                                {
                                    VE.RPPRICEGENCD = chkdata.RPPRICEGEN.Substring(0, 2);
                                    VE.RPPRICEGENAMT = chkdata.RPPRICEGEN.Substring(2, 2);
                                }
                                if (chkdata.CMROFFTYPE != null)
                                {
                                    VE.CMROFFTYPE = chkdata.CMROFFTYPE.Substring(0, 2);
                                    VE.CMROFFAMT = chkdata.CMROFFTYPE.Substring(2, 2);
                                }
                                if (chkdata.INACTIVE_TAG == "Y")
                                {
                                    VE.Checked = true;
                                }
                                else
                                {
                                    VE.Checked = false;
                                }

                                VE.M_SYSCNFG = msyscnfg;
                                VE.Checked_DataExsist = true;
                            }
                            else { VE.Checked_DataExsist = false; }
                            List<MMGROUPSPL> MSUBLEGlocth = new List<MMGROUPSPL>();
                            VE.MMGROUPSPL = (from j in DBF.M_COMP
                                             select new MMGROUPSPL()
                                             {
                                                 COMPCD = j.COMPCD,
                                                 COMPNM = j.COMPNM,
                                             }).Distinct().OrderBy(s => s.COMPCD).ToList();
                            int k = 0;
                            foreach (var v in VE.MMGROUPSPL)
                            {
                                v.SLNO = Convert.ToInt16(k + 1);
                                k++;
                            }
                        }
                        else if (op.ToString() == "E")
                        {
                            var chkdata = (from i in DB.M_SYSCNFG select i).ToList();
                            if (chkdata.Count > 1)
                            { VE.Checked_DataExsist = true; }
                            else { VE.Checked_DataExsist = false; }
                        }

                        return View(VE);
                    }
                    else
                    {
                        VE.DefaultView = false;
                        VE.DefaultDay = 0;
                        return View(VE);
                    }
                }
            }
            catch (Exception ex)
            {
                SysCnfgMasterEntry VE = new SysCnfgMasterEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public SysCnfgMasterEntry Navigation(SysCnfgMasterEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            try
            {
                sl = new M_SYSCNFG();
                sll = new M_CNTRL_HDR();
                sBRND = new M_BRAND();
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();

                if (VE.IndexKey.Count != 0)
                {
                    string[] aa = null;
                    if (searchValue.Length == 0)
                    {
                        aa = VE.IndexKey[index].Navikey.Split(Convert.ToChar(Cn.GCS()));
                    }
                    else
                    {
                        aa = searchValue.Split(Convert.ToChar(Cn.GCS()));
                    }
                    //DateTime EDT = Convert.ToDateTime(aa[1]);
                    int autono = aa[0].retInt();
                    sl = DB.M_SYSCNFG.Where(m => m.M_AUTONO == autono).First();
                    sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
                    string class1cd = sl.CLASS1CD.retStr();
                    string SALDEBGLCD = sl.SALDEBGLCD.retStr();
                    string PURDEBGLCD = sl.PURDEBGLCD.retStr();
                    string RETDEBSLCD = sl.RETDEBSLCD.retStr();
                    string SHORTAGE_GLCD = sl.SHORTAGE_GLCD.retStr();
                    if (class1cd != "")
                    {
                        var classnm = (from a in DBF.M_CLASS1 where a.CLASS1CD == class1cd select new { a.CLASS1NM }).FirstOrDefault();
                        VE.CLASS1NM = classnm.CLASS1NM;
                    }
                    if (SALDEBGLCD != "")
                    {
                        var salglnm = (from a in DBF.M_GENLEG where a.GLCD == SALDEBGLCD select new { a.GLNM }).FirstOrDefault();
                        VE.SALDEBGLNM = salglnm.GLNM;
                    }
                    if (PURDEBGLCD != "")
                    {
                        var purglnm = (from a in DBF.M_GENLEG where a.GLCD == PURDEBGLCD select new { a.GLNM }).FirstOrDefault();
                        VE.PURDEBGLNM = purglnm.GLNM;
                    }
                    if (RETDEBSLCD != "")
                    {
                        var salretglnm = (from a in DBF.M_SUBLEG where a.SLCD == RETDEBSLCD select new { a.SLNM }).FirstOrDefault();
                        VE.RETDEBSLNM = salretglnm.SLNM;
                    }
                    if (SHORTAGE_GLCD != "")
                    {
                        var shortglnm = (from a in DBF.M_GENLEG where a.GLCD == SHORTAGE_GLCD select new { a.GLNM }).FirstOrDefault();
                        VE.SHORTAGE_GLNM = shortglnm.GLNM;
                    }
                    if (sl.RTDEBCD != null)
                    { var Party = DBF.M_RETDEB.Find(sl.RTDEBCD); if (Party != null) { VE.RTDBNM = Party.RTDEBNM; } }
                    if (sl.INC_RATE == "Y") { VE.INC_RATE = true; } else { VE.INC_RATE = false; }
                    if (sl.MNTNSIZE == "Y") { VE.MNTNSIZE = true; } else { VE.MNTNSIZE = false; }
                    if (sl.MNTNCOLOR == "Y") { VE.MNTNCOLOR = true; } else { VE.MNTNCOLOR = false; }
                    if (sl.MNTNPART == "Y") { VE.MNTNPART = true; } else { VE.MNTNPART = false; }
                    if (sl.MNTNFLAGMTR == "Y") { VE.MNTNFLAGMTR = true; } else { VE.MNTNFLAGMTR = false; }
                    if (sl.MNTNLISTPRICE == "Y") { VE.MNTNLISTPRCE = true; } else { VE.MNTNLISTPRCE = false; }
                    if (sl.MNTNSHADE == "Y") { VE.MNTNSHADE = true; } else { VE.MNTNSHADE = false; }
                    if (sl.MNTNDISC1 == "Y") { VE.MNTNDISC1 = true; } else { VE.MNTNDISC1 = false; }
                    if (sl.MNTNDISC2 == "Y") { VE.MNTNDISC2 = true; } else { VE.MNTNDISC2 = false; }
                    if (sl.MNTNWPRPPER == "Y") { VE.MNTNWPRPPER = true; } else { VE.MNTNWPRPPER = false; }
                    if (sl.MNTNBALE == "Y") { VE.MNTNBALE = true; } else { VE.MNTNBALE = false; }
                    if (sl.MNTNOURDESIGN == "Y") { VE.MNTNOURDESIGN = true; } else { VE.MNTNOURDESIGN = false; }
                    if (sl.MNTNPCSTYPE == "Y") { VE.MNTNPCSTYPE = true; } else { VE.MNTNPCSTYPE = false; }
                    if (sl.MNTNBARNO == "Y") { VE.MNTNBARNO = true; } else { VE.MNTNBARNO = false; }
                    if (sl.COMMONUNIQBAR == "E") { VE.COMMONUIQBAR = true; } else { VE.COMMONUIQBAR = false; }
                    if (sl.CMCASHRECDAUTO == "Y") { VE.CMCASHRECDAUTO = true; } else { VE.CMCASHRECDAUTO = false; }
                    if (sl.MERGEINDTL == "Y") { VE.MERGEINDTL = true; } else { VE.MERGEINDTL = false; }
                    if (sl.WPPRICEGEN.retStr() != "")
                    {
                        VE.WPPRICEGENCD = sl.WPPRICEGEN.Substring(0, 2);
                        VE.WPPRICEGENAMT = sl.WPPRICEGEN.Substring(2, 2);
                    }
                    if (sl.RPPRICEGEN.retStr() != "")
                    {
                        VE.RPPRICEGENCD = sl.RPPRICEGEN.Substring(0, 2);
                        VE.RPPRICEGENAMT = sl.RPPRICEGEN.Substring(2, 2);
                    }
                    if (sl.CMROFFTYPE.retStr() != "")
                    {
                        VE.CMROFFTYPE = sl.CMROFFTYPE.Substring(0, 2);
                        VE.CMROFFAMT = sl.CMROFFTYPE.Substring(2, 2);
                    }
                    if (sll.INACTIVE_TAG == "Y")
                    {
                        VE.Checked = true;
                    }
                    else
                    {
                        VE.Checked = false;
                    }
                    if (VE.DefaultAction == "A")
                    {
                        sl.EFFDT = System.DateTime.Now.Date;
                    }
                    string sql = "";
                    //sql += " select k.COMPCD,j.COMPNM,k.DEALSIN,k.INSPOLDESC,k.BLTERMS,k.DUEDATECALCON,k.BANKSLNO ";
                    //sql += " from " + CommVar.FinSchema(UNQSNO) + ".M_COMP j, " + CommVar.CurSchema(UNQSNO) + ".M_MGROUP_SPL k where j.COMPCD = k.COMPCD(+) and k.COMPCD = '" + sl.COMPCD + "' union all ";
                    //sql += " select k.COMPCD,k.COMPNM,''DEALSIN,''INSPOLDESC,''BLTERMS,''DUEDATECALCON,0BANKSLNO ";
                    //sql += " from " + CommVar.FinSchema(UNQSNO) + ".M_COMP k ";
                    //sql += " where k.COMPCD not in (select compcd from " + CommVar.CurSchema(UNQSNO) + ".M_MGROUP_SPL where COMPCD = '" + sl.COMPCD + "'  ) ";

                    sql += " select j.COMPCD,j.COMPNM,k.DEALSIN,k.INSPOLDESC,k.BLTERMS,k.DUEDATECALCON,k.BANKSLNO ";
                    sql += " from " + CommVar.FinSchema(UNQSNO) + ".M_COMP j, " + CommVar.CurSchema(UNQSNO) + ".M_MGROUP_SPL k where j.COMPCD = k.COMPCD(+)";

                    var mmgrpspl = Master_Help.SQLquery(sql);
                    if (mmgrpspl != null && mmgrpspl.Rows.Count > 0)
                    {
                        VE.MMGROUPSPL = (from DataRow dr in mmgrpspl.Rows
                                         select new MMGROUPSPL
                                         {
                                             //T_REM = dr["trem"].retStr(),
                                             COMPCD = dr["COMPCD"].retStr(),
                                             COMPNM = dr["COMPNM"].ToString(),
                                             DEALSIN = dr["DEALSIN"].ToString(),
                                             INSPOLDESC = dr["INSPOLDESC"].ToString(),
                                             BLTERMS = dr["BLTERMS"].ToString(),
                                             DUEDATECALCON = dr["DUEDATECALCON"].ToString(),
                                             BANKSLNO = (dr["BANKSLNO"]).retInt() == 0 ? (Int32?)null : dr["BANKSLNO"].retInt(),

                                         }).OrderBy(a => a.COMPCD).ToList();
                        for (int i = 0; i <= VE.MMGROUPSPL.Count - 1; i++)
                        {
                            VE.MMGROUPSPL[i].SLNO = Convert.ToInt16(i + 1);

                        }
                    }
                    else {
                        List<MMGROUPSPL> MMGROUPSPL = new List<MMGROUPSPL>();
                        MMGROUPSPL UPL = new MMGROUPSPL();
                        UPL.SLNO = 1;
                        MMGROUPSPL.Add(UPL);
                        VE.MMGROUPSPL = MMGROUPSPL;



                    }

                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
            return VE;
        }
        public ActionResult SearchPannelData()
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            string scm = CommVar.CurSchema(UNQSNO);
            string scmf = CommVar.FinSchema(UNQSNO);
            string sql = "";
            sql += "select a.m_autono,a.compcd,a.effdt,b.glnm saldebglnm,c.glnm purdebglnm from " + scm + ".m_syscnfg a," + scmf + ".m_genleg b,  ";
            sql += scmf + ".m_genleg c where a.saldebglcd=b.glcd(+) and a.purdebglcd=c.glcd(+) ";
            DataTable tbl = Master_Help.SQLquery(sql);
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Autono" + Cn.GCS() + "Company Code" + Cn.GCS() + "Effective Date" + Cn.GCS() + "Debtors" + Cn.GCS() + "Creditors";
            for (int i = 0; i <= tbl.Rows.Count - 1; i++)
            {
                SB.Append("<tr><td>" + tbl.Rows[i]["m_autono"] + "</td><td>" + tbl.Rows[i]["compcd"] + "</td><td>" + tbl.Rows[i]["effdt"].retDateStr() + "</td><td>" + tbl.Rows[i]["saldebglnm"] + "</td><td>" + tbl.Rows[i]["purdebglnm"] + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "0", "0"));
        }
        public ActionResult GetSubLedgerDetails(string val, string Code)
        {
            try
            {
                var str = Master_Help.SLCD_help(val, Code);
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
        public ActionResult GetClass1Details(string val)
        {
            var str = Master_Help.CLASS1(val);
            if (str.IndexOf("='helpmnu'") >= 0)
            {
                return PartialView("_Help2", str);
            }
            else
            {
                return Content(str);
            }
        }
        public ActionResult GetGenLedgerDetails(string val, string Code)
        {
            var str = Master_Help.GLCD_help(val, Code);
            if (str.IndexOf("='helpmnu'") >= 0)
            {
                return PartialView("_Help2", str);
            }
            else
            {
                return Content(str);
            }
        }
        public ActionResult GetSysConfgDetails(string val)
        {
            var str = Master_Help.Get_SysConfgDetails(val);
            if (str.IndexOf("='helpmnu'") >= 0)
            {
                return PartialView("_Help2", str);
            }
            else
            {
                return Content(str);
            }
        }
        public ActionResult GetRefRetailDetails(string val, string code)
        {
            try
            {
                var str = Master_Help.RTDEBCD_help(val);
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
        public ActionResult SAVE(FormCollection FC, SysCnfgMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        string compcd = CommVar.Compcd(UNQSNO);
                        M_SYSCNFG MSYSCNFG = new M_SYSCNFG();
                        M_MGROUP_SPL MMGROUPSPL = new M_MGROUP_SPL();
                        MSYSCNFG.CLCD = CommVar.ClientCode(UNQSNO);
                        MSYSCNFG.COMPCD = CommVar.Compcd(UNQSNO);
                        MSYSCNFG.EFFDT = VE.M_SYSCNFG.EFFDT;
                        bool flag = false;
                        if (VE.Checked_DataExsist == true) flag = true;
                        if (VE.DefaultAction == "A")
                        {
                            string query = "select EFFDT from  " + CommVar.CurSchema(UNQSNO) + ".M_SYSCNFG  where EFFDT=to_date('" + VE.M_SYSCNFG.EFFDT.retDateStr() + "', 'dd/mm/yyyy') and COMPCD='" + compcd + "'";
                            DataTable EffdtChk = Master_Help.SQLquery(query);
                            if (EffdtChk.Rows.Count > 0) { transaction.Rollback(); return Content(" " + EffdtChk.Rows[0]["EFFDT"].retDateStr() + " Effective Date already exsist.Please select another Effective Date !"); }
                            MSYSCNFG.EMD_NO = 0;
                            MSYSCNFG.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());

                        }
                        if (VE.DefaultAction == "E")
                        {
                            MSYSCNFG.DTAG = "E";
                            MSYSCNFG.M_AUTONO = VE.M_SYSCNFG.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_SYSCNFG where p.M_AUTONO == VE.M_SYSCNFG.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MSYSCNFG.EMD_NO = 0;
                            }
                            else
                            {
                                MSYSCNFG.EMD_NO = Convert.ToInt16(MAXEMDNO + 1);
                            }

                        }
                        //var Chk_MGRP_SPL_data = (from i in DB.M_MGROUP_SPL where i.COMPCD == MSYSCNFG.COMPCD select i).ToList();
                        //if (Chk_MGRP_SPL_data.Count > 0) DB.M_MGROUP_SPL.RemoveRange(DB.M_MGROUP_SPL.Where(x => x.COMPCD == MSYSCNFG.COMPCD));
                        if (VE.MMGROUPSPL != null)
                        {
                            for (int i = 0; i <= VE.MMGROUPSPL.Count - 1; i++)
                            {
                                if (VE.MMGROUPSPL[i].SLNO != 0)
                                {
                                    var compcdd = VE.MMGROUPSPL[i].COMPCD;
                                    var Chk_MGRP_SPL_data = (from j in DB.M_MGROUP_SPL where j.COMPCD == compcdd select j).ToList();
                                    if (Chk_MGRP_SPL_data.Count > 0) DB.M_MGROUP_SPL.RemoveRange(DB.M_MGROUP_SPL.Where(x => x.COMPCD == compcdd));
                                }
                            }
                        }
                        MSYSCNFG.SALDEBGLCD = VE.M_SYSCNFG.SALDEBGLCD;
                        MSYSCNFG.PURDEBGLCD = VE.M_SYSCNFG.PURDEBGLCD;
                        MSYSCNFG.CLASS1CD = VE.M_SYSCNFG.CLASS1CD;
                        MSYSCNFG.RETDEBSLCD = VE.M_SYSCNFG.RETDEBSLCD;
                        if (VE.WPPRICEGENCD.retStr() != "")
                        {
                            MSYSCNFG.WPPRICEGEN = VE.WPPRICEGENCD.retStr() + VE.WPPRICEGENAMT.retStr().PadLeft(2, '0');//NR99
                        }
                        if (VE.RPPRICEGENCD.retStr() != "")
                        {
                            MSYSCNFG.RPPRICEGEN = VE.RPPRICEGENCD.retStr() + VE.RPPRICEGENAMT.retStr().PadLeft(2, '0');//NT99
                        }
                        if (VE.CMROFFTYPE.retStr() != "")
                        {
                            MSYSCNFG.CMROFFTYPE = VE.CMROFFTYPE.retStr() + VE.CMROFFAMT.retStr().PadLeft(2, '0');//NR99
                        }
                        //MSYSCNFG.DEALSIN = VE.M_SYSCNFG.DEALSIN;
                        MSYSCNFG.INSPOLDESC = VE.M_SYSCNFG.INSPOLDESC;
                        //MSYSCNFG.BLTERMS = VE.M_SYSCNFG.BLTERMS;
                        MSYSCNFG.DUEDATECALCON = VE.M_SYSCNFG.DUEDATECALCON;
                        //MSYSCNFG.BANKSLNO = VE.M_SYSCNFG.BANKSLNO;
                        MSYSCNFG.WPPER = VE.M_SYSCNFG.WPPER;
                        MSYSCNFG.RPPER = VE.M_SYSCNFG.RPPER;
                        MSYSCNFG.PRICEINCODE = VE.M_SYSCNFG.PRICEINCODE;
                        MSYSCNFG.PRICEINCODECOST = VE.M_SYSCNFG.PRICEINCODECOST;
                        MSYSCNFG.RTDEBCD = VE.M_SYSCNFG.RTDEBCD;
                        MSYSCNFG.DESIGNPATH = VE.M_SYSCNFG.DESIGNPATH;
                        MSYSCNFG.SHORTAGE_GLCD = VE.M_SYSCNFG.SHORTAGE_GLCD;
                        if (VE.INC_RATE == true) { MSYSCNFG.INC_RATE = "Y"; } else { MSYSCNFG.INC_RATE = "N"; }
                        if (VE.MNTNSIZE == true) { MSYSCNFG.MNTNSIZE = "Y"; } else { MSYSCNFG.MNTNSIZE = "N"; }
                        if (VE.MNTNCOLOR == true) { MSYSCNFG.MNTNCOLOR = "Y"; } else { MSYSCNFG.MNTNCOLOR = "N"; }
                        if (VE.MNTNPART == true) { MSYSCNFG.MNTNPART = "Y"; } else { MSYSCNFG.MNTNPART = "N"; }
                        if (VE.MNTNFLAGMTR == true) { MSYSCNFG.MNTNFLAGMTR = "Y"; } else { MSYSCNFG.MNTNFLAGMTR = "N"; }
                        if (VE.MNTNLISTPRCE == true) { MSYSCNFG.MNTNLISTPRICE = "Y"; } else { MSYSCNFG.MNTNLISTPRICE = "N"; }
                        if (VE.MNTNSHADE == true) { MSYSCNFG.MNTNSHADE = "Y"; } else { MSYSCNFG.MNTNSHADE = "N"; }
                        if (VE.MNTNDISC1 == true) { MSYSCNFG.MNTNDISC1 = "Y"; } else { MSYSCNFG.MNTNDISC1 = "N"; }
                        if (VE.MNTNDISC2 == true) { MSYSCNFG.MNTNDISC2 = "Y"; } else { MSYSCNFG.MNTNDISC2 = "N"; }
                        if (VE.MNTNWPRPPER == true) { MSYSCNFG.MNTNWPRPPER = "Y"; } else { MSYSCNFG.MNTNWPRPPER = "N"; }
                        if (VE.MNTNBALE == true) { MSYSCNFG.MNTNBALE = "Y"; } else { MSYSCNFG.MNTNBALE = "N"; }
                        if (VE.MNTNOURDESIGN == true) { MSYSCNFG.MNTNOURDESIGN = "Y"; } else { MSYSCNFG.MNTNOURDESIGN = "N"; }
                        if (VE.MNTNPCSTYPE == true) { MSYSCNFG.MNTNPCSTYPE = "Y"; } else { MSYSCNFG.MNTNPCSTYPE = "N"; }
                        if (VE.MNTNBARNO == true) { MSYSCNFG.MNTNBARNO = "Y"; } else { MSYSCNFG.MNTNBARNO = "N"; }
                        if (VE.COMMONUIQBAR == true) { MSYSCNFG.COMMONUNIQBAR = "E"; } else { MSYSCNFG.MNTNBARNO = "C"; }
                        if (VE.CMCASHRECDAUTO == true) { MSYSCNFG.CMCASHRECDAUTO = "Y"; } else { MSYSCNFG.CMCASHRECDAUTO = "N"; }
                        if (VE.MERGEINDTL == true) { MSYSCNFG.MERGEINDTL = "Y"; } else { MSYSCNFG.MERGEINDTL = "N"; }
                        if (VE.DefaultAction == "A")
                        {
                            M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SYSCNFG", MSYSCNFG.M_AUTONO, "A", CommVar.CurSchema(UNQSNO).ToString());
                            DB.M_SYSCNFG.Add(MSYSCNFG);
                            DB.M_CNTRL_HDR.Add(MCH);
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SYSCNFG", MSYSCNFG.M_AUTONO, "E", CommVar.CurSchema(UNQSNO).ToString());
                            DB.Entry(MSYSCNFG).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        }
                        if (VE.MMGROUPSPL != null)
                        {
                            for (int i = 0; i <= VE.MMGROUPSPL.Count - 1; i++)
                            {
                                if (VE.MMGROUPSPL[i].SLNO != 0)
                                {
                                    M_MGROUP_SPL MGROUPSPL = new M_MGROUP_SPL();
                                    MGROUPSPL.COMPCD = VE.MMGROUPSPL[i].COMPCD;
                                    MGROUPSPL.DEALSIN = VE.MMGROUPSPL[i].DEALSIN.retStr();
                                    MGROUPSPL.INSPOLDESC = VE.MMGROUPSPL[i].INSPOLDESC.retStr();
                                    MGROUPSPL.BLTERMS = VE.MMGROUPSPL[i].BLTERMS.retStr();
                                    MGROUPSPL.DUEDATECALCON = VE.MMGROUPSPL[i].DUEDATECALCON.retStr();
                                    MGROUPSPL.BANKSLNO = VE.MMGROUPSPL[i].BANKSLNO;
                                    //if (VE.MMGROUPSPL[i].DUEDATECALCONChecked == true) { MGROUPSPL.DUEDATECALCON = "Y"; } else { MGROUPSPL.DUEDATECALCON = "N"; }
                                    DB.M_MGROUP_SPL.Add(MGROUPSPL);
                                }
                            }
                        }
                        DB.SaveChanges();
                        transaction.Commit();
                        string ContentFlg = "";
                        if (VE.DefaultAction == "A")
                        {
                            ContentFlg = "1";
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            ContentFlg = "2";
                        }
                        return Content(ContentFlg);
                    }
                    else if (VE.DefaultAction == "V")
                    {

                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SYSCNFG", VE.M_SYSCNFG.M_AUTONO.retInt(), VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_SYSCNFG.Where(x => x.M_AUTONO == VE.M_SYSCNFG.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_SYSCNFG.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();
                        DB.M_MGROUP_SPL.RemoveRange(DB.M_MGROUP_SPL.Where(x => x.COMPCD == VE.M_SYSCNFG.COMPCD));
                        DB.SaveChanges();
                        DB.M_SYSCNFG.RemoveRange(DB.M_SYSCNFG.Where(x => x.M_AUTONO == VE.M_SYSCNFG.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_SYSCNFG.M_AUTONO));
                        DB.SaveChanges();
                        transaction.Commit();
                        return Content("3");
                    }
                    else
                    {
                        return Content("");
                    }
                }
                catch (DbEntityValidationException ex)
                {
                    var errorMessages = ex.EntityValidationErrors
                            .SelectMany(x => x.ValidationErrors)
                            .Select(x => x.ErrorMessage);
                    var fullErrorMessage = string.Join("&quot;", errorMessages);
                    var exceptionMessage = string.Concat(ex.Message, "<br/> &quot; The validation errors are: &quot;", fullErrorMessage);
                    throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException?.InnerException.Message);
                }
            }
        }
        [HttpPost]
        public ActionResult M_SysCnfg(FormCollection FC)
        {
            try
            {
                string dbname = CommVar.CurSchema(UNQSNO).ToString();

                string query = "select a.itgrpnm, a.itgrpcd, case ";
                query = query + "when a.itgrptype='F' then 'Finish Product' ";
                query = query + "when a.itgrptype='A' then 'Accessories' ";
                query = query + "when a.itgrptype='Y' then 'Yarn' ";
                query = query + "when a.itgrptype='S' then 'Scheme Material' ";
                query = query + "when a.itgrptype='C' then 'Fabric' ";
                query = query + "end itgrptype from " + dbname + ".M_SYSCNFG a, " + dbname + ".m_cntrl_hdr c ";
                query = query + "where  a.m_autono=c.m_autono(+) and nvl(c.inactive_tag,'N') = 'N' ";
                query = query + "order by itgrptype, a.itgrpnm";

                CS = Cn.GetConnectionString();

                Cn.con = new OracleConnection(CS);
                if ((Cn.ds.Tables["mst_rep"] == null) == false)
                {
                    Cn.ds.Tables["mst_rep"].Clear();
                }
                if (Cn.con.State == ConnectionState.Closed)
                {
                    Cn.con.Open();
                }

                string str = query;
                Cn.com = new OracleCommand(str, Cn.con);
                Cn.da.SelectCommand = Cn.com;
                bool bu = Convert.ToBoolean(Cn.da.Fill(Cn.ds, "mst_rep"));
                Cn.con.Close();
                var record = Master_Help.SQLquery(query);
                if (bu)
                {
                    DataTable IR = new DataTable("mstrep");

                    Models.PrintViewer PV = new Models.PrintViewer();
                    HtmlConverter HC = new HtmlConverter();

                    HC.RepStart(IR, 2);
                    HC.GetPrintHeader(IR, "itgrpnm", "string", "c,20", "Group Name");
                    HC.GetPrintHeader(IR, "itgrpcd", "string", "c,7", "Group Code");
                    //HC.GetPrintHeader(IR, "brandnm", "string", "c,20", "Brande");
                    HC.GetPrintHeader(IR, "itgrptype", "string", "c,10", "Group Type");

                    for (int i = 0; i <= Cn.ds.Tables["mst_rep"].Rows.Count - 1; i++)
                    {
                        DataRow dr = IR.NewRow();
                        dr["itgrpnm"] = Cn.ds.Tables["mst_rep"].Rows[i]["itgrpnm"];
                        dr["itgrpcd"] = Cn.ds.Tables["mst_rep"].Rows[i]["itgrpcd"];
                        //dr["brandnm"] = Cn.ds.Tables["mst_rep"].Rows[i]["brandnm"];
                        dr["itgrptype"] = Cn.ds.Tables["mst_rep"].Rows[i]["itgrptype"];
                        dr["Flag"] = " class='grid_td'";
                        IR.Rows.Add(dr);
                    }
                    string pghdr1 = "";
                    string repname = CommFunc.retRepname("Group Master Details");
                    pghdr1 = "Group Master Details";
                    PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);
                    return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });

                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
            return View();
        }
        public SysCnfgMasterEntry CheckDataExsistOrNot(SysCnfgMasterEntry VE)
        {
            try
            {
                string str = "";
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                var compcd = CommVar.Compcd(UNQSNO);
                var chkdata = (from i in DB.M_SYSCNFG where i.COMPCD == compcd select i).ToList();
                if (chkdata.Count > 0) VE.Checked_DataExsist = true; else VE.Checked_DataExsist = false;

            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                //return Content(ex.Message + ex.InnerException);
            }
            return VE;
        }
        public string RateEncode(int rate, string PRICEINCODE)
        {
            PRICEINCODE = PRICEINCODE.ToUpper();
            string str = ""; string rptchar = "";
            string[,] arr = new string[11, 2];
            //G A N A S H R //11th charecter can repeat eg 1122=ARNR
            //0 1 2 3 4 5 H
            if (PRICEINCODE != "")
            {
                int i = 0;
                foreach (char c in PRICEINCODE)
                {
                    arr[i, 0] = c.ToString();
                    arr[i, 1] = i.ToString(); i++;
                }
                var strate = rate.ToString(); string lastchar = "";
                if (PRICEINCODE.Length == 11) rptchar = arr[10, 0];
                foreach (char c in strate)
                {
                    for (int k = 0; k < arr.GetLength(0) - rptchar.Length; k++)
                    {
                        if (c.ToString() == arr[k, 1])
                        {
                            if (lastchar == arr[k, 0] && rptchar != "")
                            {
                                str += rptchar;
                                lastchar = "";
                            }
                            else
                            {
                                str += arr[k, 0];
                                lastchar = arr[k, 0];
                            }
                        }
                    }
                }
                return str;
            }
            else
            {
                return rate.ToString();
            }
        }

    }
}