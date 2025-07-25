﻿using Improvar.Models;
using Improvar.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using System.Web;
using OfficeOpenXml;
using System.Globalization;
using Oracle.ManagedDataAccess.Client;
using System.Xml;
using System.IO;
using OfficeOpenXml.Style;

namespace Improvar.Controllers
{
    public class SUB_LEDGERController : Controller
    {
        // GET: SUB_LEDGER
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        MasterHelpFa MasterHelpFa = new MasterHelpFa();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        M_SUBLEG sl; MS_STATE sll; M_CNTRL_HDR mchsll; M_DISTRICT SMD; MS_COUNTRY SMSC;
        public ActionResult SUB_LEDGER(string op = "", string key = "", int Nindex = 0, string searchValue = "", long searchMautono = 0)
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    string MODULE = CommVar.ModuleCode();
                    if (MODULE == "FIN") { ViewBag.formname = "Sub Ledger Master"; }
                    else if (MODULE == "INV") { ViewBag.formname = "Vendor Master"; }
                    else if (MODULE.Remove(4) == "SALE") { ViewBag.formname = "Customer Master"; }
                    SubLedgerEntry VE = new SubLedgerEntry();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(VE.UNQSNO));
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    //=========For Registraion Type===========//
                    List<RegistrationType> RT = new List<RegistrationType>();
                    RegistrationType RT1 = new RegistrationType();
                    RT1.Value = "R";
                    RT1.Text = "Registered";
                    RT.Add(RT1);
                    RegistrationType RT2 = new RegistrationType();
                    RT2.Value = "U";
                    RT2.Text = "Unregisterd";
                    RT.Add(RT2);
                    RegistrationType RT3 = new RegistrationType();
                    RT3.Value = "C";
                    RT3.Text = "Composite";
                    RT.Add(RT3);
                    RegistrationType RT4 = new RegistrationType();
                    RT4.Value = "N";
                    RT4.Text = "UAN";
                    RT.Add(RT4);
                    VE.RegistrationType = RT;
                    //=========End Registraion Type===========//
                    List<DropDown_list1> drplst = new List<DropDown_list1>();
                    DropDown_list1 dropobj1 = new DropDown_list1();
                    dropobj1.value = "SAVING";
                    dropobj1.text = "SAVING";
                    drplst.Add(dropobj1);

                    DropDown_list1 dropobj2 = new DropDown_list1();
                    dropobj2.value = "CURRENT";
                    dropobj2.text = "CURRENT";
                    drplst.Add(dropobj2);

                    DropDown_list1 dropobj3 = new DropDown_list1();
                    dropobj3.value = "CC";
                    dropobj3.text = "CC";
                    drplst.Add(dropobj3);

                    DropDown_list1 dropobj4 = new DropDown_list1();
                    dropobj4.value = "OD";
                    dropobj4.text = "OD";
                    drplst.Add(dropobj4);
                    VE.DropDown_list1 = drplst;
                    //=========For Dept Type===========//

                    List<DropDown_list> deptlst = new List<DropDown_list>();
                    DropDown_list deptobj1 = new DropDown_list();
                    deptobj1.value = "F";
                    deptobj1.text = "Finance";
                    deptlst.Add(deptobj1);

                    DropDown_list deptobj2 = new DropDown_list();
                    deptobj2.value = "P";
                    deptobj2.text = "Purchase";
                    deptlst.Add(deptobj2);

                    DropDown_list deptobj3 = new DropDown_list();
                    deptobj3.value = "A";
                    deptobj3.text = "Admin";
                    deptlst.Add(deptobj3);

                    DropDown_list deptobj4 = new DropDown_list();
                    deptobj4.value = "S";
                    deptobj4.text = "Sales";
                    deptlst.Add(deptobj4);
                    VE.DropDown_list = deptlst;
                    //=========For Dept Type===========//
                    //=========For TOT194Q ===========//
                    List<DropDown_list2> TOT = new List<DropDown_list2>();
                    //DropDown_list2 TOT1 = new DropDown_list2();
                    //TOT1.value = "";
                    //TOT1.text = "";
                    //TOT.Add(TOT1);
                    DropDown_list2 TOT2 = new DropDown_list2();
                    TOT2.value = "Y";
                    TOT2.text = "Yes";
                    TOT.Add(TOT2);
                    DropDown_list2 TOT3 = new DropDown_list2();
                    TOT3.value = "N";
                    TOT3.text = "No";
                    TOT.Add(TOT3);
                    VE.DropDown_list2 = TOT;
                    //=========End TOT194Q ===========//
                    //=========For pan_206ab_cca ===========//
                    List<DropDown_list3> PAN206AB = new List<DropDown_list3>();
                    DropDown_list3 PAN1 = new DropDown_list3();
                    PAN1.value = "";
                    PAN1.text = "No Recd";
                    PAN206AB.Add(PAN1);
                    DropDown_list3 PAN2 = new DropDown_list3();
                    PAN2.value = "Y";
                    PAN2.text = "Yes";
                    PAN206AB.Add(PAN2);
                    DropDown_list3 PAN3 = new DropDown_list3();
                    PAN3.value = "N";
                    PAN3.text = "No";
                    PAN206AB.Add(PAN3);
                    VE.DropDown_list3 = PAN206AB;
                    //=========End pan_206ab_cca ===========//
                    //==== Database Combo ====//

                    VE.Database_Combo1 = (from i in DB.M_DISTRICT select new Database_Combo1() { FIELD_VALUE = i.DISTCD }).OrderBy(s => s.FIELD_VALUE).ToList();
                    VE.Database_Combo2 = (from i in DB1.MS_COUNTRY select new Database_Combo2() { FIELD_VALUE = i.CNAME }).OrderBy(s => s.FIELD_VALUE).ToList();
                    VE.Database_Combo3 = (from i in DB.M_SUBLEG_IFSC select new Database_Combo3() { FIELD_VALUE = i.BANKNAME }).Distinct().OrderBy(s => s.FIELD_VALUE).ToList();
                    VE.Database_Combo4 = (from i in DB.M_SUBLEG select new Database_Combo4() { FIELD_VALUE = i.PARTYGRP }).Distinct().OrderBy(s => s.FIELD_VALUE).ToList();

                    VE.Document = (from i in DB.M_DOCTYPE select new DOCTYPE() { DOCCD = i.DOCCD, DOCNM = i.DOCNM }).OrderBy(s => s.DOCNM).ToList();

                    VE.LedgerType = (from i in DB1.MS_LINK select new LedgerType() { value = i.LINKCD, text = i.LINKNM }).OrderBy(s => s.value).ToList();

                    VE.PartyGroup = (from i in DB.M_PARTYGRP select new PartyGroup() { PARTYCD = i.PARTYCD, PARTYNM = i.PARTYNM }).OrderBy(s => s.PARTYNM).ToList();

                    VE.CompanyType = (from i in DB1.MS_COMPTYPE select new CompanyType() { COMPTYCD = i.COMPTYCD, COMPTYNM = i.COMPTYNM }).OrderBy(s => s.COMPTYNM).ToList();

                    VE.BusinessActivity = (from i in DB1.MS_NATBUSCODES select new BusinessActivity() { NATBUSCD = i.NATBUSCD, NATBUSNM = i.NATBUSNM }).OrderBy(s => s.NATBUSNM).ToList();

                    VE.CompanyLocationName = (from i in DB.M_LOCA join l in DB.M_COMP on i.COMPCD equals (l.COMPCD) select new CompanyLocationName() { COMPCD = l.COMPCD, COMPNM = l.COMPNM, LOCCD = i.LOCCD, LOCNM = i.LOCNM }).OrderBy(s => s.LOCNM).ToList();

                    VE.LinkType = (from i in DB1.MS_LINK select new LinkType() { LINKCD = i.LINKCD, LINKNM = i.LINKNM }).OrderBy(s => s.LINKNM).ToList();
                    VE.GeneralLedgerDetails = (from i in DB.M_GENLEG where (i.SLCDMUST == "Y") select new GeneralLedgerDetails() { GLCD = i.GLCD, GLNM = i.GLNM }).OrderBy(s => s.GLNM).ToList();
                    string sql = "select NVL(GSPCLIENTAPP,GSPAPPID) GSPCLIENTAPP from ms_ipsmart";
                    DataTable dt = masterHelp.SQLquery(sql);
                    if (dt != null && dt.Rows.Count > 0 && dt.Rows[0]["GSPCLIENTAPP"].ToString() != "")
                    {
                        VE.IsAPIEnabled = true;
                    }
                    VE.SrcFlagCaption = "Name/GST/Code";
                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_SUBLEG orderby p.SLCD select new IndexKey() { Navikey = p.SLCD }).ToList();

                        if (op == "E" || op == "D" || op == "V")
                        {
                            if (searchValue.Length != 0)
                            {
                                VE.Index = Nindex;
                                VE = Navigation(VE, DB, DB1, 0, searchValue);
                            }
                            else
                            {
                                if (key == "F")
                                {
                                    VE.Index = 0;
                                    VE = Navigation(VE, DB, DB1, 0, searchValue);
                                }
                                else if (key == "" || key == "L")
                                {
                                    VE.Index = VE.IndexKey.Count - 1;
                                    VE = Navigation(VE, DB, DB1, VE.IndexKey.Count - 1, searchValue);
                                }
                                else if (key == "P")
                                {
                                    Nindex -= 1;
                                    if (Nindex < 0)
                                    {
                                        Nindex = 0;
                                    }
                                    VE.Index = Nindex;
                                    VE = Navigation(VE, DB, DB1, Nindex, searchValue);
                                }
                                else if (key == "N")
                                {
                                    Nindex += 1;
                                    if (Nindex > VE.IndexKey.Count - 1)
                                    {
                                        Nindex = VE.IndexKey.Count - 1;
                                    }
                                    VE.Index = Nindex;
                                    VE = Navigation(VE, DB, DB1, Nindex, searchValue);
                                }
                            }
                            VE.M_SUBLEG = sl;
                            VE.M_DISTRICT = SMD;
                            VE.MS_STATE = sll;
                            VE.MS_COUNTRY = SMSC;
                            VE.M_CNTRL_HDR = mchsll;
                        }
                        if (op.ToString() == "A")
                        {
                            //=================For Communication================//
                            List<MSUBLEGCONT> SUBLEGCONT = new List<MSUBLEGCONT>();
                            MSUBLEGCONT MSLC = new MSUBLEGCONT();
                            MSLC.SLNO = 1;
                            MSLC.Designation = Designation();
                            SUBLEGCONT.Add(MSLC);
                            VE.MSUBLEGCONT = SUBLEGCONT;
                            //=================End Communication================//

                            //=================For Bank Details================//
                            List<MSUBLEGIFSC> LOCAIFSC = new List<MSUBLEGIFSC>();
                            MSUBLEGIFSC MSLIF = new MSUBLEGIFSC();
                            MSLIF.SLNO = 1;
                            LOCAIFSC.Add(MSLIF);
                            VE.MSUBLEGIFSC = LOCAIFSC;
                            //=================End Bank Details================//

                            //=================For Specific Company================//
                            List<MSUBLEGTAX> MSUBLEGTAX1 = new List<MSUBLEGTAX>();
                            MSUBLEGTAX MSLT = new MSUBLEGTAX();
                            MSLT.SLNO = 1;
                            MSUBLEGTAX1.Add(MSLT);
                            VE.MSUBLEGTAX = MSUBLEGTAX1;
                            //=============
                            List<MSUBLEGLOCOTH> MSUBLEGlocth = new List<MSUBLEGLOCOTH>();
                            VE.MSUBLEGLOCOTH = (from i in DB.M_LOCA
                                                join j in DB.M_COMP on i.COMPCD equals j.COMPCD
                                                select new MSUBLEGLOCOTH()
                                                {
                                                    COMPCD = i.COMPCD,
                                                    COMPNM = j.COMPNM,
                                                    LOCCD = i.LOCCD,
                                                    LOCNM = i.LOCNM
                                                })
                            .OrderBy(s => s.COMPCD).ToList();
                            int k = 0;
                            foreach (var v in VE.MSUBLEGLOCOTH)
                            {
                                v.SLNO = Convert.ToInt16(k + 1);
                                k++;
                            }
                            //=================End Specific Company================//
                            List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                            UploadDOC UPL = new UploadDOC();
                            UPL.DocumentType = doctP;
                            UploadDOC1.Add(UPL);
                            VE.UploadDOC = UploadDOC1;
                            VE.TCSAPPL = true;
                            VE.AUTOREMINDEROFF = true;
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
                SubLedgerEntry VE = new SubLedgerEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public SubLedgerEntry Navigation(SubLedgerEntry VE, ImprovarDB DB, ImprovarDB DB1, int index, string searchValue)
        {
            sl = new M_SUBLEG(); SMD = new M_DISTRICT(); sll = new MS_STATE(); SMSC = new MS_COUNTRY(); mchsll = new M_CNTRL_HDR();
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
                sl = DB.M_SUBLEG.Find(aa[0]);
                var PSL = (from j in DB.M_SUBLEG where (j.SLCD == sl.PSLCD) select new { PSLNM = j.SLNM }).ToList();
                if (PSL.Count != 0)
                {
                    VE.PSLNM = PSL[0].PSLNM;
                }
                sll = DB1.MS_STATE.Find(sl.STATECD);
                SMD = DB.M_DISTRICT.Find(sl.DISTRICT);
                SMSC = DB1.MS_COUNTRY.Find(sl.COUNTRY);
                mchsll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
                if (mchsll.INACTIVE_TAG == "Y")
                {
                    VE.Checked = true;
                }
                else
                {
                    VE.Checked = false;
                }
                VE.TCSAPPL = sl.TCSAPPL == null ? true : sl.TCSAPPL == "Y" ? true : false;
                VE.AUTOREMINDEROFF = sl.AUTOREMINDEROFF == "Y" ? true : false;
                VE.BLWSONACT_Checked = sl.BLWSONACT == "Y" ? true : false;
                if (sl.PARTYCD.retStr() != "")
                {
                    string PARTYCD = sl.PARTYCD;
                    var partynm = (from a in DB.M_PARTYGRP where a.PARTYCD == PARTYCD select new { a.PARTYNM }).FirstOrDefault();
                    VE.PARTYNM = partynm.PARTYNM;
                }
                var ss = (from p in DB.M_SUBLEG_BUSNAT where (p.SLCD == sl.SLCD) select new BusinessActivity() { NATBUSCD = p.NATBUSCD, Checked = true }).ToList();
                if (ss != null)
                    foreach (var i in ss)
                    {
                        var temp = DB1.MS_NATBUSCODES.Find(i.NATBUSCD);
                        i.NATBUSNM = temp.NATBUSNM;
                    }
                if (VE.BusinessActivity != null)
                    foreach (var i in VE.BusinessActivity)
                    {
                        foreach (var x in ss)
                        {
                            if (i.NATBUSCD == x.NATBUSCD)
                            {
                                i.Checked = true;
                            }
                        }
                    }
                var GLD = (from p in DB.M_SUBLEG_GL join s in DB.M_GENLEG on p.GLCD equals (s.GLCD) where (p.SLCD == sl.SLCD) select new GeneralLedgerDetails() { GLCD = p.GLCD, GLNM = s.GLNM, Checked = true }).ToList();
                if (VE.GeneralLedgerDetails != null)
                    foreach (var i in VE.GeneralLedgerDetails)
                    {
                        foreach (var x in GLD)
                        {
                            if (i.GLCD == x.GLCD)
                            {
                                i.Checked = true;
                            }
                        }
                    }
                var LT = (from p in DB.M_SUBLEG_LINK where (p.SLCD == sl.SLCD) select new LinkType() { LINKCD = p.LINKCD, Checked = true }).ToList();
                if (LT != null)
                    foreach (var i in LT)
                    {
                        var temp = DB1.MS_LINK.Find(i.LINKCD);
                        i.LINKNM = temp.LINKNM;
                    }
                if (VE.LinkType != null)
                    foreach (var i in VE.LinkType)
                    {
                        foreach (var x in LT)
                        {
                            if (i.LINKCD == x.LINKCD)
                            {
                                i.Checked = true;
                            }
                        }
                    }
                var CLN = (from p in DB.M_CNTRL_LOCA join s in DB.M_COMP on p.COMPCD equals (s.COMPCD) join x in DB.M_LOCA on p.LOCCD equals (x.LOCCD) where (p.M_AUTONO == sl.M_AUTONO) select new CompanyLocationName() { COMPCD = p.COMPCD, COMPNM = s.COMPNM, LOCCD = p.LOCCD, LOCNM = x.LOCNM, Checked = true }).ToList();

                if (VE.CompanyLocationName != null)
                    foreach (var i in VE.CompanyLocationName)
                    {
                        foreach (var x in CLN)
                        {
                            if (i.COMPCD == x.COMPCD && i.LOCCD == x.LOCCD)
                            {
                                i.Checked = true;
                            }
                        }
                    }
                VE.MSUBLEGCONT = (from i in DB.M_SUBLEG_CONT
                                  where (i.SLCD == sl.SLCD)
                                  select new MSUBLEGCONT()
                                  {
                                      SLCD = i.SLCD,
                                      SLNO = i.SLNO,
                                      CPERSON = i.CPERSON,
                                      DESIG = i.DESIG,
                                      DEPT = i.DEPT,
                                      EXTENSION = i.EXTENSION,
                                      PHNO1STD = i.PHNO1STD,
                                      PHNO1 = i.PHNO1,
                                      MOBILE1PREFIX = i.MOBILE1PREFIX,
                                      MOBILE1 = i.MOBILE1,
                                      MOBILE2PREFIX = i.MOBILE2PREFIX,
                                      MOBILE2 = i.MOBILE2,
                                      PERSEMAIL = i.PERSEMAIL,
                                      PERSDOB = i.PERSDOB,
                                      PERSDOA = i.PERSDOA,
                                  }).OrderBy(s => s.SLNO).ToList();
                foreach (var v in VE.MSUBLEGCONT)
                {
                    v.Designation = Designation();
                }

                if (VE.DefaultAction == "E")
                {
                    if (VE.MSUBLEGCONT.Count == 0)
                    {
                        List<MSUBLEGCONT> SUBLEGCONT = new List<MSUBLEGCONT>();
                        MSUBLEGCONT MSLC = new MSUBLEGCONT();
                        MSLC.SLNO = 1;
                        MSLC.Designation = Designation();
                        SUBLEGCONT.Add(MSLC);
                        VE.MSUBLEGCONT = SUBLEGCONT;
                    }
                }

                VE.MSUBLEGIFSC = (from i in DB.M_SUBLEG_IFSC
                                  where (i.SLCD == sl.SLCD)
                                  select new MSUBLEGIFSC()
                                  {
                                      SLCD = i.SLCD,
                                      SLNO = i.SLNO,
                                      IFSCCODE = i.IFSCCODE,
                                      BANKNAME = i.BANKNAME,
                                      BRANCH = i.BRANCH,
                                      ADDRESS = i.ADDRESS,
                                      BANKACTTYPE = i.BANKACTTYPE,
                                      BANKACTNO = i.BANKACTNO,
                                      DEFLTBANK = i.DEFLTBANK
                                  }).OrderBy(s => s.SLNO).ToList();

                if (VE.MSUBLEGIFSC.Count == 0)
                {
                    List<MSUBLEGIFSC> LOCAIFSC = new List<MSUBLEGIFSC>();
                    MSUBLEGIFSC MSLIF = new MSUBLEGIFSC();
                    MSLIF.SLNO = 1;
                    LOCAIFSC.Add(MSLIF);
                    VE.MSUBLEGIFSC = LOCAIFSC;
                }
                else
                {
                    foreach (var v in VE.MSUBLEGIFSC)
                    {
                        if (v.DEFLTBANK == "T")
                        {
                            v.DFLTBNK = true;
                        }
                        else
                        {
                            v.DFLTBNK = false;
                        }
                    }
                }




                VE.MSUBLEGTAX = (from i in DB.M_SUBLEG_TAX
                                 join j in DB.M_COMP on i.COMPCD equals (j.COMPCD)
                                 where (i.SLCD == sl.SLCD)
                                 select new MSUBLEGTAX()
                                 {
                                     SLCD = i.SLCD,
                                     COMPCD = i.COMPCD,
                                     COMPNM = j.COMPNM,
                                     LWRRT = i.LWRRT,
                                     TDSLMT = i.TDSLMT,
                                     INTPER = i.INTPER
                                 }).OrderBy(s => s.COMPCD).ToList();

                if (VE.MSUBLEGTAX.Count == 0)
                {
                    List<MSUBLEGTAX> MSUBLEGTAX1 = new List<MSUBLEGTAX>();
                    MSUBLEGTAX MSLT = new MSUBLEGTAX();
                    MSLT.SLNO = 1;
                    MSUBLEGTAX1.Add(MSLT);
                    VE.MSUBLEGTAX = MSUBLEGTAX1;
                }
                else
                {
                    for (int i = 0; i <= VE.MSUBLEGTAX.Count - 1; i++)
                    {
                        VE.MSUBLEGTAX[i].SLNO = i + 1;
                    }
                }

                //VE.MSUBLEGLOCOTH = (from i in DB.M_SUBLEG_LOCOTH
                //                    join j in DB.M_COMP on i.COMPCD equals (j.COMPCD)
                //                    join k in DB.M_LOCA on i.LOCCD equals (k.LOCCD) 
                //                    where i.SLCD == sl.SLCD && i.COMPCD==k.COMPCD
                //                    select new MSUBLEGLOCOTH()
                //                    {
                //                        COMPCD = i.COMPCD,
                //                        COMPNM = j.COMPNM,
                //                        LOCCD = i.LOCCD,
                //                        LOCNM = k.LOCNM,
                //                        DISTANCE = i.DISTANCE
                //                    }).Distinct().OrderBy(s => s.COMPCD).ToList();

                //int l = 0;
                //foreach (var v in VE.MSUBLEGLOCOTH)
                //{
                //    v.SLNO = Convert.ToInt16(l + 1);
                //    l++;
                //}
                string sql = "", scmf = CommVar.FinSchema(UNQSNO);
                sql = " select ROW_NUMBER() OVER(ORDER BY compcd) AS slno, compcd, compnm, loccd, locnm, distance from( ";
                sql += "  select a.compcd, a.loccd, b.compnm, c.locnm, a.distance from " + scmf + ".M_SUBLEG_LOCOTH A, " + scmf + ".M_comp b, " + scmf + ".M_loca c ";
                sql += "  where a.compcd = b.compcd and a.loccd = c.loccd and a.compcd = c.compcd  and  slcd = '" + sl.SLCD + "' ";
                sql += "  union all ";
                sql += "  select distinct b.compcd, c.loccd, b.compnm, c.locnm, 0 distance from " + scmf + ".M_comp b, " + scmf + ".M_loca c ";
                sql += "   where b.compcd = c.compcd  and c.compcd || c.loccd not in (select a.compcd || a.loccd from " + scmf + ".M_SUBLEG_LOCOTH A where slcd = '" + sl.SLCD + "') ";
                sql += "  ) a ";
                var FILTER_DATA = masterHelp.SQLquery(sql);
                VE.MSUBLEGLOCOTH = (from DataRow dr in FILTER_DATA.Rows
                                    select new MSUBLEGLOCOTH()
                                    {
                                        SLNO = Convert.ToSByte(dr["slno"]),
                                        COMPCD = dr["compcd"].retStr(),
                                        COMPNM = dr["compnm"].retStr(),
                                        LOCCD = dr["loccd"].retStr(),
                                        LOCNM = dr["locnm"].retStr(),
                                        DISTANCE = dr["distance"].retInt()
                                    }).ToList();

                for (int q = 0; q <= VE.MSUBLEGLOCOTH.Count - 1; q++)
                {
                    VE.MSUBLEGLOCOTH[q].SLNO = Convert.ToSByte(q + 1);
                }


                VE.UploadDOC = Cn.GetUploadImage(CommVar.FinSchema(VE.UNQSNO), Convert.ToInt32(sl.M_AUTONO));
            }
            else
            {
                List<MSUBLEGIFSC> LOCAIFSC = new List<MSUBLEGIFSC>();
                MSUBLEGIFSC MSLIF = new MSUBLEGIFSC();
                MSLIF.SLNO = 1;
                LOCAIFSC.Add(MSLIF);
                VE.MSUBLEGIFSC = LOCAIFSC;

                List<MSUBLEGCONT> SUBLEGCONT = new List<MSUBLEGCONT>();
                MSUBLEGCONT MSLC = new MSUBLEGCONT();
                MSLC.SLNO = 1;
                MSLC.Designation = Designation();
                SUBLEGCONT.Add(MSLC);
                VE.MSUBLEGCONT = SUBLEGCONT;

                List<MSUBLEGTAX> MSUBLEGTAX1 = new List<MSUBLEGTAX>();
                MSUBLEGTAX MSLT = new MSUBLEGTAX();
                MSLT.SLNO = 1;
                MSUBLEGTAX1.Add(MSLT);
                VE.MSUBLEGTAX = MSUBLEGTAX1;

                List<MSUBLEGLOCOTH> MSUBLEGlocth = new List<MSUBLEGLOCOTH>();
                VE.MSUBLEGLOCOTH = (from i in DB.M_LOCA
                                    join j in DB.M_COMP on i.COMPCD equals j.COMPCD
                                    select new MSUBLEGLOCOTH()
                                    {
                                        COMPCD = i.COMPCD,
                                        COMPNM = j.COMPNM,
                                        LOCCD = i.LOCCD,
                                        LOCNM = i.LOCNM
                                    })
                .OrderBy(s => s.COMPCD).ToList();
                int k = 0;
                foreach (var v in VE.MSUBLEGLOCOTH)
                {
                    v.SLNO = Convert.ToInt16(k + 1);
                }

                VE.LedgerType = (from i in DB1.MS_LINK select new LedgerType() { value = i.LINKCD, text = i.LINKNM }).OrderBy(s => s.text).ToList();

                VE.PartyGroup = (from i in DB.M_PARTYGRP select new PartyGroup() { PARTYCD = i.PARTYCD, PARTYNM = i.PARTYNM }).OrderBy(s => s.PARTYNM).ToList();
            }
            //VE.isLastYrSchema = CommVar.FinSchemaPrevYr(UNQSNO) == "" ? false : true;
            try
            {
                ImprovarDB DB_PREVYR_temp = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchemaPrevYr(UNQSNO));
                if (CommVar.FinSchemaPrevYr(UNQSNO) == "")
                {
                    VE.isPresentinLastYrSchema = "";
                }
                else
                {
                    var SLCD = sl.SLCD;
                    VE.isPresentinLastYrSchema = (from j in DB_PREVYR_temp.M_SUBLEG where (j.SLCD == SLCD) select j.SLCD).FirstOrDefault();
                    if (string.IsNullOrEmpty(VE.isPresentinLastYrSchema))
                    {
                        VE.isPresentinLastYrSchema = "ADD";
                    }
                    else
                    {
                        VE.isPresentinLastYrSchema = "EDIT";
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return VE;
        }
        public ActionResult SearchPannelData(string SRC_FLAG)
        {
            try
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                string scm = CommVar.FinSchema(UNQSNO);
                string sql = "select j.SLCD,j.SLNM,j.GSTNO,j.SLAREA,j.DISTRICT,j.PIN,j.STATE from " + scm + ".M_SUBLEG j ," + scm + ".M_CNTRL_HDR o where j.M_AUTONO=o.M_AUTONO   ";
                if (SRC_FLAG.retStr() != "") sql += "and (upper(j.slcd) like '%" + SRC_FLAG.retStr().ToUpper() + "%' or upper(j.slnm) like '%" + SRC_FLAG.retStr().ToUpper() + "%'or upper(j.GSTNO) like '%" + SRC_FLAG.retStr().ToUpper() + "%') ";
                DataTable MDT = masterHelp.SQLquery(sql);
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                var hdr = "Sub Ledger Code" + Cn.GCS() + "Sub Ledger Name" + Cn.GCS() + "GST" + Cn.GCS() + "District" + Cn.GCS() + "Area" + Cn.GCS() + "PIN" + Cn.GCS() + "State";
                for (int j = 0; j <= MDT.Rows.Count - 1; j++)
                {
                    SB.Append("<tr><td>" + MDT.Rows[j]["SLCD"].retStr() + "</td><td>" + MDT.Rows[j]["SLNM"].retStr() + " </td><td> " + MDT.Rows[j]["GSTNO"].retStr() + "</td><td>" + MDT.Rows[j]["DISTRICT"].retStr() + " </td><td> " + MDT.Rows[j]["SLAREA"].retStr() + "</td><td> " + MDT.Rows[j]["PIN"].retStr() + "</td><td> " + MDT.Rows[j]["STATE"].retStr() + "</td></tr>");
                }
                return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "0"));
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public string CheckSubledgerName(string val)
        {
            try
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                var query = (from c in DB.M_SUBLEG where (c.SLNM == val) select c);
                if (query.Any())
                {
                    string str = "<table class='table-bordered' border='2px'><tr><th style='border: 1px solid #b1ac05;padding-right:2px'>ID</th><th style='border: 1px solid #b1ac05;padding-right:2px'>Name</th><th style='border: 1px solid #b1ac05;padding-right:2px'>Address</th><th style='border: 1px solid #b1ac05;padding-right:2px'>GST</th></tr>";
                    foreach (var i in query)
                    {
                        str = str + "<tr><td style='border: 1px solid #a11818;'>" + i.SLCD + "</td><td style='border: 1px solid #a11818;'>" + i.SLNM + "</td><td style='border: 1px solid #a11818;'>" + i.BLDGNO + i.PREMISES + i.ROADNAME + "</td><td style='border: 1px solid #a11818;'>" + i.GSTNO + "</td></tr>";
                    }
                    str = str + "</table><br /> Sub Ledger: <u>" + val + "</u>  Allready Entered";
                    return str;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return ex.Message + ex.InnerException;
            }
        }
        public string CheckGSTNO(string val)
        {
            try
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                var query = (from c in DB.M_SUBLEG where (c.GSTNO == val) select c);
                if (query.Any())
                {
                    string str = "<table class='table-bordered' border='2px'><tr><th style='border: 1px solid #b1ac05;padding-right:2px'>ID</th><th style='border: 1px solid #b1ac05;padding-right:2px'>Name</th><th style='border: 1px solid #b1ac05;padding-right:2px'>Address</th><th style='border: 1px solid #b1ac05;padding-right:2px'>GST</th></tr>";
                    foreach (var i in query)
                    {
                        str = str + "<tr><td style='border: 1px solid #a11818;'>" + i.SLCD + "</td><td style='border: 1px solid #a11818;'>" + i.SLNM + "</td><td style='border: 1px solid #a11818;'>" + i.BLDGNO + i.PREMISES + i.ROADNAME + "</td><td style='border: 1px solid #a11818;'>" + i.GSTNO + "</td></tr>";
                    }
                    str = str + "</table><br /> GSTNO: <u>" + val + "</u>  Allready Entered";
                    return str;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return ex.Message + ex.InnerException;
            }
        }
        public List<Designation> Designation()
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            List<Designation> Desig = (from i in DBF.M_DESIGNATION select new Designation() { Value = i.DESIGCD, Text = i.DESIGNM }).OrderBy(s => s.Text).ToList();
            return Desig;
        }
        public ActionResult AddRow(SubLedgerEntry VE)
        {
            try
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                //=========For Dept Type===========//

                List<DropDown_list> deptlst = new List<DropDown_list>();
                DropDown_list deptobj1 = new DropDown_list();
                deptobj1.value = "F";
                deptobj1.text = "Finance";
                deptlst.Add(deptobj1);

                DropDown_list deptobj2 = new DropDown_list();
                deptobj2.value = "P";
                deptobj2.text = "Purchase";
                deptlst.Add(deptobj2);

                DropDown_list deptobj3 = new DropDown_list();
                deptobj3.value = "A";
                deptobj3.text = "Admin";
                deptlst.Add(deptobj3);

                DropDown_list deptobj4 = new DropDown_list();
                deptobj4.value = "S";
                deptobj4.text = "Sales";
                deptlst.Add(deptobj4);
                VE.DropDown_list = deptlst;
                //=========For Dept Type===========//
                if (VE.MSUBLEGCONT == null)
                {
                    List<MSUBLEGCONT> SUBLEGCONT1 = new List<MSUBLEGCONT>();
                    MSUBLEGCONT MSLC = new MSUBLEGCONT();
                    MSLC.SLNO = 1;
                    MSLC.Designation = Designation();
                    SUBLEGCONT1.Add(MSLC);
                    VE.MSUBLEGCONT = SUBLEGCONT1;
                }
                else
                {

                    List<MSUBLEGCONT> SUBLEGCONT = new List<MSUBLEGCONT>();
                    for (int i = 0; i <= VE.MSUBLEGCONT.Count - 1; i++)
                    {
                        MSUBLEGCONT MSLC = new MSUBLEGCONT();
                        MSLC = VE.MSUBLEGCONT[i];
                        MSLC.Designation = Designation();
                        SUBLEGCONT.Add(MSLC);
                    }
                    MSUBLEGCONT MSLC1 = new MSUBLEGCONT();
                    var max = VE.MSUBLEGCONT.Max(a => Convert.ToInt32(a.SLNO));
                    int mx = max + 1;
                    MSLC1.SLNO = Convert.ToByte(mx);
                    MSLC1.Designation = Designation();
                    SUBLEGCONT.Add(MSLC1);
                    VE.MSUBLEGCONT = SUBLEGCONT;
                }
                VE.DefaultView = true;
                return PartialView("_SUB_LEDGER", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }

        }
        public ActionResult DeleteRow(SubLedgerEntry VE)
        {
            try
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                //=========For Dept Type===========//

                List<DropDown_list> deptlst = new List<DropDown_list>();
                DropDown_list deptobj1 = new DropDown_list();
                deptobj1.value = "F";
                deptobj1.text = "Finance";
                deptlst.Add(deptobj1);

                DropDown_list deptobj2 = new DropDown_list();
                deptobj2.value = "P";
                deptobj2.text = "Purchase";
                deptlst.Add(deptobj2);

                DropDown_list deptobj3 = new DropDown_list();
                deptobj3.value = "A";
                deptobj3.text = "Admin";
                deptlst.Add(deptobj3);

                DropDown_list deptobj4 = new DropDown_list();
                deptobj4.value = "S";
                deptobj4.text = "Sales";
                deptlst.Add(deptobj4);
                VE.DropDown_list = deptlst;
                //=========For Dept Type===========//
                List<MSUBLEGCONT> SUBLEGCONT = new List<MSUBLEGCONT>();
                int count = 0;
                for (int i = 0; i <= VE.MSUBLEGCONT.Count - 1; i++)
                {
                    if (VE.MSUBLEGCONT[i].Checked == false)
                    {
                        count += 1;
                        MSUBLEGCONT MSLC = new MSUBLEGCONT();
                        MSLC = VE.MSUBLEGCONT[i];
                        MSLC.SLNO = Convert.ToByte(count);
                        MSLC.Designation = Designation();
                        SUBLEGCONT.Add(MSLC);
                    }
                }
                VE.MSUBLEGCONT = SUBLEGCONT;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_SUB_LEDGER", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddRowBank(SubLedgerEntry VE)
        {
            try
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                VE.Database_Combo3 = (from i in DB.M_SUBLEG_IFSC select new Database_Combo3() { FIELD_VALUE = i.BANKNAME }).Distinct().OrderBy(s => s.FIELD_VALUE).ToList();

                List<DropDown_list1> drplst = new List<DropDown_list1>();
                DropDown_list1 dropobj1 = new DropDown_list1();
                dropobj1.value = "SAVING";
                dropobj1.text = "SAVING";
                drplst.Add(dropobj1);

                DropDown_list1 dropobj2 = new DropDown_list1();
                dropobj2.value = "CURRENT";
                dropobj2.text = "CURRENT";
                drplst.Add(dropobj2);

                DropDown_list1 dropobj3 = new DropDown_list1();
                dropobj3.value = "CC";
                dropobj3.text = "CC";
                drplst.Add(dropobj3);

                DropDown_list1 dropobj4 = new DropDown_list1();
                dropobj4.value = "OD";
                dropobj4.text = "OD";
                drplst.Add(dropobj4);
                VE.DropDown_list1 = drplst;

                if (VE.MSUBLEGIFSC == null)
                {
                    List<MSUBLEGIFSC> MSUBLEGIFSC1 = new List<MSUBLEGIFSC>();
                    MSUBLEGIFSC MSLIFSC = new MSUBLEGIFSC();
                    MSLIFSC.SLNO = 1;
                    MSUBLEGIFSC1.Add(MSLIFSC);
                    VE.MSUBLEGIFSC = MSUBLEGIFSC1;
                }

                else
                {
                    List<MSUBLEGIFSC> MSUBLEGIFSC = new List<MSUBLEGIFSC>();
                    for (int i = 0; i <= VE.MSUBLEGIFSC.Count - 1; i++)
                    {
                        MSUBLEGIFSC MSLIFSC = new MSUBLEGIFSC();
                        MSLIFSC = VE.MSUBLEGIFSC[i];
                        MSUBLEGIFSC.Add(MSLIFSC);
                    }
                    MSUBLEGIFSC MSLIFSC1 = new MSUBLEGIFSC();
                    var max = VE.MSUBLEGIFSC.Max(a => Convert.ToInt32(a.SLNO));
                    int mx = max + 1;
                    MSLIFSC1.SLNO = Convert.ToByte(mx);
                    MSUBLEGIFSC.Add(MSLIFSC1);
                    VE.MSUBLEGIFSC = MSUBLEGIFSC;
                }
                VE.DefaultView = true;
                return PartialView("_SUB_LEDGER_BANK", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult DeleteRowBank(SubLedgerEntry VE)
        {
            try
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                VE.Database_Combo3 = (from i in DB.M_SUBLEG_IFSC select new Database_Combo3() { FIELD_VALUE = i.BANKNAME }).Distinct().OrderBy(s => s.FIELD_VALUE).ToList();

                List<DropDown_list1> drplst = new List<DropDown_list1>();
                DropDown_list1 dropobj1 = new DropDown_list1();
                dropobj1.value = "SAVING";
                dropobj1.text = "SAVING";
                drplst.Add(dropobj1);

                DropDown_list1 dropobj2 = new DropDown_list1();
                dropobj2.value = "CURRENT";
                dropobj2.text = "CURRENT";
                drplst.Add(dropobj2);

                DropDown_list1 dropobj3 = new DropDown_list1();
                dropobj3.value = "CC";
                dropobj3.text = "CC";
                drplst.Add(dropobj3);

                DropDown_list1 dropobj4 = new DropDown_list1();
                dropobj4.value = "OD";
                dropobj4.text = "OD";
                drplst.Add(dropobj4);
                VE.DropDown_list1 = drplst;

                List<MSUBLEGIFSC> SUBLEGIFSC = new List<MSUBLEGIFSC>();
                int count = 0;
                for (int i = 0; i <= VE.MSUBLEGIFSC.Count - 1; i++)
                {
                    if (VE.MSUBLEGIFSC[i].Checked == false)
                    {
                        count += 1;
                        MSUBLEGIFSC IFSC = new MSUBLEGIFSC();
                        IFSC = VE.MSUBLEGIFSC[i];
                        IFSC.SLNO = Convert.ToByte(count);
                        SUBLEGIFSC.Add(IFSC);
                    }
                }
                VE.MSUBLEGIFSC = SUBLEGIFSC;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_SUB_LEDGER_BANK", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddRowCompany(SubLedgerEntry VE)
        {
            try
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                if (VE.MSUBLEGTAX == null)
                {
                    List<MSUBLEGTAX> MSUBLEGTAX1 = new List<MSUBLEGTAX>();
                    MSUBLEGTAX MSLT = new MSUBLEGTAX();
                    MSLT.SLNO = 1;
                    MSUBLEGTAX1.Add(MSLT);
                    VE.MSUBLEGTAX = MSUBLEGTAX1;
                }

                else
                {
                    List<MSUBLEGTAX> MSUBLEGTAX = new List<MSUBLEGTAX>();
                    for (int i = 0; i <= VE.MSUBLEGTAX.Count - 1; i++)
                    {
                        MSUBLEGTAX MSLT = new MSUBLEGTAX();
                        MSLT = VE.MSUBLEGTAX[i];
                        MSUBLEGTAX.Add(MSLT);
                    }
                    MSUBLEGTAX MSLT1 = new MSUBLEGTAX();
                    var max = VE.MSUBLEGTAX.Max(a => Convert.ToInt32(a.SLNO));
                    MSLT1.SLNO = max + 1;
                    MSUBLEGTAX.Add(MSLT1);
                    VE.MSUBLEGTAX = MSUBLEGTAX;
                }
                VE.DefaultView = true;
                return PartialView("_SUB_LEDGER_COMPANY", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult DeleteRowCompany(SubLedgerEntry VE)
        {
            try
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                List<MSUBLEGTAX> SUBLEGTAX = new List<MSUBLEGTAX>();
                int count = 0;
                for (int i = 0; i <= VE.MSUBLEGTAX.Count - 1; i++)
                {
                    if (VE.MSUBLEGTAX[i].Checked == false)
                    {
                        count += 1;
                        MSUBLEGTAX TAX = new MSUBLEGTAX();
                        TAX = VE.MSUBLEGTAX[i];
                        TAX.SLNO = count;
                        SUBLEGTAX.Add(TAX);
                    }
                }
                VE.MSUBLEGTAX = SUBLEGTAX;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_SUB_LEDGER_COMPANY", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetState()
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                return PartialView("_Help2", MasterHelpFa.STATECD_help(DB));
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult State(string val)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                var query = (from c in DB.MS_STATE where (c.STATECD == val) select c);
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {

                        if (i.TDSSTATECD != null)
                        {
                            str = i.STATECD + Cn.GCS() + i.STATENM + Cn.GCS() + i.TDSSTATECD;
                        }
                        else {
                            str = i.STATECD + Cn.GCS() + i.STATENM + Cn.GCS() + i.STATECD;
                        }

                    }
                    return Content(str);
                }
                else
                {
                    return Content("0");
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetCountry()
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                return PartialView("_Help2", MasterHelpFa.CISDCD_help(DB));
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult Country(string val)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                var query = (from c in DB.MS_COUNTRY where (c.CNCD == val) select c);
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = i.CISDCD + Cn.GCS() + i.CNAME;
                    }
                    return Content(str);
                }
                else
                {
                    return Content("0");
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult District(string val)
        {
            try
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                var query = (from c in DB.M_DISTRICT where (c.DISTCD == val) select c);
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = i.DISTCD + Cn.GCS() + i.DISTNM;
                    }
                    return Content(str);
                }
                else
                {
                    return Content("0");
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetBankDetails()
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                return PartialView("_Help2", MasterHelpFa.IFSCCODE_help(DB));
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }

        }
        public ActionResult BankDetails(string val)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                var query = (from c in DB.MS_BANKIFSC where (c.IFSCCODE == val) select c);
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = i.IFSCCODE + Cn.GCS() + i.BANKNAME + Cn.GCS() + i.BRANCH + Cn.GCS() + i.CITY;
                    }
                    return Content(str);
                }
                else
                {
                    return Content("0");
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetCompanyDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.COMPANY_HELP(val));
                }
                else
                {
                    string str = masterHelp.COMPANY_HELP(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetDistrict()
        {
            try
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                return PartialView("_Help2", MasterHelpFa.DISTCD_help(DB));
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public string SLCD_PSLCD_help(ImprovarDB DB, String Code)
        {
            try
            {
                var query = (from c in DB.M_SUBLEG
                             where (c.SLCD != Code)
                             select new
                             {
                                 Code = c.SLCD,
                                 Description = c.SLNM
                             }).ToList();
                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                for (int i = 0; i <= query.Count - 1; i++)
                {
                    SB.Append("<tr id='Hrow_" + i.ToString() + "' onclick='HelpRowClick(this.id)'><td>" + query[i].Description + "</td><td>" + query[i].Code + "</td></tr>");
                }
                var hdr = "Company Name" + Cn.GCS() + "Company Code";
                return masterHelp.Generate_help(hdr, SB.ToString());
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return ex.Message + ex.InnerException;
            }
        }
        public ActionResult SavePreviousYrData(SubLedgerEntry VE, FormCollection FC)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DB_PREVYR = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchemaPrevYr(UNQSNO));
            ImprovarDB DB_PREVYR_temp = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchemaPrevYr(UNQSNO));
            using (var transaction = DB_PREVYR.Database.BeginTransaction())
            {
                try
                {
                    var PSL = (from j in DB_PREVYR_temp.M_SUBLEG where (j.SLCD == VE.M_SUBLEG.SLCD) select j).FirstOrDefault();
                    var SL = (from j in DB.M_SUBLEG where (j.SLCD == VE.M_SUBLEG.SLCD) select j).FirstOrDefault();
                    var SLOCA = (from j in DB.M_CNTRL_LOCA where (j.M_AUTONO == VE.M_SUBLEG.M_AUTONO) select j).ToList();
                    var SBUSNAT = (from j in DB.M_SUBLEG_BUSNAT where (j.SLCD == VE.M_SUBLEG.SLCD) select j).ToList();
                    var SGL = (from j in DB.M_SUBLEG_GL where (j.SLCD == VE.M_SUBLEG.SLCD) select j).ToList();
                    var SLINK = (from j in DB.M_SUBLEG_LINK where (j.SLCD == VE.M_SUBLEG.SLCD) select j).ToList();
                    var SCONT = (from j in DB.M_SUBLEG_CONT where (j.SLCD == VE.M_SUBLEG.SLCD) select j).ToList();
                    var SIFSC = (from j in DB.M_SUBLEG_IFSC where (j.SLCD == VE.M_SUBLEG.SLCD) select j).ToList();
                    var STAX = (from j in DB.M_SUBLEG_TAX where (j.SLCD == VE.M_SUBLEG.SLCD) select j).ToList();
                    //var SLOCOTH = (from j in DB.M_SUBLEG_LOCOTH where (j.SLCD == VE.M_SUBLEG.SLCD) select j).ToList();

                    string[] loca = (from i in DB_PREVYR_temp.M_LOCA select i.LOCCD + i.COMPCD).ToArray();

                    var SLOCOTH = (from j in DB.M_SUBLEG_LOCOTH where (j.SLCD == VE.M_SUBLEG.SLCD) && loca.Contains(j.LOCCD + j.COMPCD) select j).ToList();



                    if (PSL == null)
                    {
                        var AUTONO_PREVYR = Cn.M_AUTONO(CommVar.FinSchemaPrevYr(UNQSNO));
                        M_CNTRL_HDR MCH_PREVYR = Cn.M_CONTROL_HDR(VE.Checked, "M_SUBLEG", AUTONO_PREVYR, "A", CommVar.FinSchemaPrevYr(UNQSNO), VE.Audit_REM, VE.M_CNTRL_HDR == null ? "" : VE.M_CNTRL_HDR.PKGLEGACYCD);
                        DB_PREVYR.M_CNTRL_HDR.Add(MCH_PREVYR);
                        DB_PREVYR.SaveChanges();
                        M_SUBLEG MSUBLEG_PREVYR = new M_SUBLEG();
                        MSUBLEG_PREVYR = SL;
                        MSUBLEG_PREVYR.M_AUTONO = AUTONO_PREVYR;
                        DB_PREVYR.M_SUBLEG.Add(MSUBLEG_PREVYR);
                        DB_PREVYR.SaveChanges();
                        if (SLOCA.Count != 0)
                        {
                            foreach (var v in SLOCA)
                            {
                                v.M_AUTONO = AUTONO_PREVYR;
                            }
                            DB_PREVYR.M_CNTRL_LOCA.AddRange(SLOCA);
                        }

                        if (SBUSNAT.Count != 0) { DB_PREVYR.M_SUBLEG_BUSNAT.AddRange(SBUSNAT); }
                        if (SGL.Count != 0) { DB_PREVYR.M_SUBLEG_GL.AddRange(SGL); }
                        if (SLINK.Count != 0)
                        {
                            DB_PREVYR.M_SUBLEG_LINK.AddRange(SLINK);
                        }
                        if (SCONT.Count != 0) { DB_PREVYR.M_SUBLEG_CONT.AddRange(SCONT); }
                        if (SIFSC.Count != 0) { DB_PREVYR.M_SUBLEG_IFSC.AddRange(SIFSC); }
                        if (STAX.Count != 0) { DB_PREVYR.M_SUBLEG_TAX.AddRange(STAX); }
                        if (SLOCOTH.Count != 0) { DB_PREVYR.M_SUBLEG_LOCOTH.AddRange(SLOCOTH); }

                        DB_PREVYR.SaveChanges();
                        ModelState.Clear();
                        transaction.Commit();
                    }
                    else
                    {
                        M_CNTRL_HDR MCH_PREVYR = Cn.M_CONTROL_HDR(VE.Checked, "M_SUBLEG", PSL.M_AUTONO, "E", CommVar.FinSchemaPrevYr(UNQSNO));
                        DB_PREVYR.Entry(MCH_PREVYR).State = System.Data.Entity.EntityState.Modified;
                        M_SUBLEG MSUBLEG_PREVYR = new M_SUBLEG();
                        MSUBLEG_PREVYR = SL; MSUBLEG_PREVYR.M_AUTONO = MCH_PREVYR.M_AUTONO;
                        DB_PREVYR.Entry(MSUBLEG_PREVYR).State = System.Data.Entity.EntityState.Modified;

                        DB_PREVYR.M_CNTRL_LOCA.RemoveRange(DB_PREVYR.M_CNTRL_LOCA.Where(x => x.M_AUTONO == MCH_PREVYR.M_AUTONO));
                        DB_PREVYR.M_SUBLEG_BUSNAT.RemoveRange(DB_PREVYR.M_SUBLEG_BUSNAT.Where(x => x.SLCD == VE.M_SUBLEG.SLCD));
                        DB_PREVYR.M_SUBLEG_GL.RemoveRange(DB_PREVYR.M_SUBLEG_GL.Where(x => x.SLCD == VE.M_SUBLEG.SLCD));
                        DB_PREVYR.M_SUBLEG_LINK.RemoveRange(DB_PREVYR.M_SUBLEG_LINK.Where(x => x.SLCD == VE.M_SUBLEG.SLCD));
                        DB_PREVYR.M_SUBLEG_CONT.RemoveRange(DB_PREVYR.M_SUBLEG_CONT.Where(x => x.SLCD == VE.M_SUBLEG.SLCD));
                        DB_PREVYR.M_SUBLEG_IFSC.RemoveRange(DB_PREVYR.M_SUBLEG_IFSC.Where(x => x.SLCD == VE.M_SUBLEG.SLCD));
                        //DB.M_SUBLEG_TAX.RemoveRange(DB_PREVYR.M_SUBLEG_TAX.Where(x => x.SLCD == VE.M_SUBLEG.SLCD));
                        DB_PREVYR.M_SUBLEG_TAX.RemoveRange(DB_PREVYR.M_SUBLEG_TAX.Where(x => x.SLCD == VE.M_SUBLEG.SLCD));
                        DB_PREVYR.M_SUBLEG_LOCOTH.RemoveRange(DB_PREVYR.M_SUBLEG_LOCOTH.Where(x => x.SLCD == VE.M_SUBLEG.SLCD));
                        DB_PREVYR.SaveChanges();
                        if (SLOCA.Count != 0)
                        {
                            foreach (var v in SLOCA)
                            {
                                v.M_AUTONO = MCH_PREVYR.M_AUTONO;
                            }
                            DB_PREVYR.M_CNTRL_LOCA.AddRange(SLOCA);
                        }

                        if (SBUSNAT.Count != 0) { DB_PREVYR.M_SUBLEG_BUSNAT.AddRange(SBUSNAT); }
                        if (SGL.Count != 0) { DB_PREVYR.M_SUBLEG_GL.AddRange(SGL); }
                        if (SLINK.Count != 0)
                        {
                            DB_PREVYR.M_SUBLEG_LINK.AddRange(SLINK);
                        }
                        if (SCONT.Count != 0) { DB_PREVYR.M_SUBLEG_CONT.AddRange(SCONT); }
                        if (SIFSC.Count != 0) { DB_PREVYR.M_SUBLEG_IFSC.AddRange(SIFSC); }
                        if (STAX.Count != 0) { DB_PREVYR.M_SUBLEG_TAX.AddRange(STAX); }
                        if (SLOCOTH.Count != 0) { DB_PREVYR.M_SUBLEG_LOCOTH.AddRange(SLOCOTH); }
                        DB_PREVYR.SaveChanges();
                        ModelState.Clear();
                        transaction.Commit();
                    }

                    return Content("1");

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.Clear();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + " " + ex.InnerException);
                }
            }
        }
        public ActionResult GetLinkCode(string Code)
        {
            try
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                return PartialView("_Help2", SLCD_PSLCD_help(DB, Code));
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult LinkCode(string val)
        {
            try
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                var query = (from c in DB.M_SUBLEG where (c.SLCD == val) select c);
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = i.SLCD + Cn.GCS() + i.SLNM;
                    }
                    return Content(str);
                }
                else
                {
                    return Content("0");
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddDOCRow(SubLedgerEntry VE)
        {
            try
            {
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CommSchema());
                var doctP = (from i in DB1.MS_DOCCTG
                             select new DocumentType()
                             {
                                 value = i.DOC_CTG,
                                 text = i.DOC_CTG
                             }).OrderBy(s => s.text).ToList();
                if (VE.UploadDOC == null)
                {
                    List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
                    UploadDOC MLI = new UploadDOC();
                    MLI.DocumentType = doctP;
                    MLocIFSC1.Add(MLI);
                    VE.UploadDOC = MLocIFSC1;
                }
                else
                {
                    List<UploadDOC> MLocIFSC1 = new List<UploadDOC>();
                    for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
                    {
                        UploadDOC MLI = new UploadDOC();
                        MLI = VE.UploadDOC[i];
                        MLI.DocumentType = doctP;
                        MLocIFSC1.Add(MLI);
                    }
                    UploadDOC MLI1 = new UploadDOC();
                    MLI1.DocumentType = doctP;
                    MLocIFSC1.Add(MLI1);
                    VE.UploadDOC = MLocIFSC1;
                }
                VE.DefaultView = true;
                return PartialView("_UPLOADDOCUMENTS", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult DeleteDOCRow(SubLedgerEntry VE)
        {
            try
            {
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CommSchema());
                var doctP = (from i in DB1.MS_DOCCTG
                             select new DocumentType()
                             {
                                 value = i.DOC_CTG,
                                 text = i.DOC_CTG
                             }).OrderBy(s => s.text).ToList();
                List<UploadDOC> LOCAIFSC = new List<UploadDOC>();
                int count = 0;
                for (int i = 0; i <= VE.UploadDOC.Count - 1; i++)
                {
                    if (VE.UploadDOC[i].chk == false)
                    {
                        count += 1;
                        UploadDOC IFSC = new UploadDOC();
                        IFSC = VE.UploadDOC[i];
                        IFSC.DocumentType = doctP;
                        LOCAIFSC.Add(IFSC);
                    }
                }
                VE.UploadDOC = LOCAIFSC;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_UPLOADDOCUMENTS", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public dynamic SAVE(FormCollection FC, SubLedgerEntry VE, string Other_Para = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    try
                    {
                        if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                        {
                            string action = VE.DefaultAction;
                            string SLedType = "";
                            for (int i = 0; i <= VE.LinkType.Count - 1; i++)
                            {
                                if (VE.LinkType[i].Checked)
                                {

                                    SLedType = VE.LinkType[i].LINKCD;
                                }
                            }

                            M_SUBLEG MSUBLEG = new M_SUBLEG();
                            MSUBLEG.CLCD = CommVar.ClientCode(UNQSNO);
                            if (VE.DefaultAction == "A")
                            {
                                MSUBLEG.EMD_NO = 0;
                                MSUBLEG.M_AUTONO = Cn.M_AUTONO(CommVar.FinSchema(UNQSNO));
                                string txtst = (SLedType + VE.M_SUBLEG.SLNM.Trim().Substring(0, 1)).ToUpper();
                                var MAXSLCD = DB.M_SUBLEG.Where(a => a.SLCD.Substring(0, 2).ToUpper() == txtst).Max(a => a.SLCD);
                                if (MAXSLCD == null)
                                {
                                    MSUBLEG.SLCD = txtst + "00001";
                                }
                                else
                                {
                                    string digits = new string(MAXSLCD.Where(char.IsDigit).ToArray());
                                    int number;
                                    if (!int.TryParse(digits, out number))                   //int.Parse would do the job since only digits are selected
                                    {
                                        Console.WriteLine("Something weired happened");
                                    }
                                    string newStr = txtst + (++number).ToString("D5");
                                    MSUBLEG.SLCD = newStr.ToString();
                                }
                            }
                            MSUBLEG.SLNM = VE.M_SUBLEG.SLNM.TrimStart(' ');
                            MSUBLEG.PSLCD = VE.M_SUBLEG.PSLCD;
                            MSUBLEG.SHORTNM = VE.M_SUBLEG.SHORTNM;
                            MSUBLEG.PARTYCD = VE.M_SUBLEG.PARTYCD;
                            MSUBLEG.FULLNAME = VE.M_SUBLEG.FULLNAME;
                            MSUBLEG.BLDGNO = VE.M_SUBLEG.BLDGNO;
                            MSUBLEG.PREMISES = VE.M_SUBLEG.PREMISES;
                            MSUBLEG.FLOORNO = VE.M_SUBLEG.FLOORNO;
                            MSUBLEG.ROADNAME = VE.M_SUBLEG.ROADNAME;
                            MSUBLEG.LOCALITY = VE.M_SUBLEG.LOCALITY;
                            MSUBLEG.EXTADDR = VE.M_SUBLEG.EXTADDR;
                            MSUBLEG.STATE = VE.M_SUBLEG.STATE;
                            MSUBLEG.STATECD = VE.M_SUBLEG.STATECD;
                            MSUBLEG.DISTRICT = VE.M_SUBLEG.DISTRICT;
                            MSUBLEG.PIN = VE.M_SUBLEG.PIN;
                            MSUBLEG.COUNTRY = VE.M_SUBLEG.COUNTRY;
                            MSUBLEG.LANDMARK = VE.M_SUBLEG.LANDMARK;
                            MSUBLEG.SLAREA = VE.M_SUBLEG.SLAREA;
                            MSUBLEG.PARTYGRP = VE.M_SUBLEG.PARTYGRP; // FC["prtygrp"].ToString();
                            MSUBLEG.GSTNO = VE.M_SUBLEG.GSTNO.retStr().ToUpper();
                            MSUBLEG.REGNTYPE = VE.M_SUBLEG.REGNTYPE;
                            MSUBLEG.PANNO = VE.M_SUBLEG.PANNO;
                            MSUBLEG.TOT194Q = VE.M_SUBLEG.TOT194Q;
                            MSUBLEG.CENNO = VE.M_SUBLEG.CENNO;
                            MSUBLEG.TANNO = VE.M_SUBLEG.TANNO;
                            MSUBLEG.CINNO = VE.M_SUBLEG.CINNO;
                            MSUBLEG.GSTDT = VE.M_SUBLEG.GSTDT;
                            MSUBLEG.STATNO_1 = VE.M_SUBLEG.STATNO_1;
                            MSUBLEG.STATDT_1 = VE.M_SUBLEG.STATDT_1;
                            if (Other_Para == "Upload") MSUBLEG.SLCOMPTYPE = VE.M_SUBLEG.SLCOMPTYPE; else MSUBLEG.SLCOMPTYPE = FC["comtype"].ToString();

                            MSUBLEG.CMPNONCMP = DB1.MS_COMPTYPE.Where(s => s.COMPTYCD == MSUBLEG.SLCOMPTYPE).Select(s => s.LTDIND).FirstOrDefault();
                            if (VE.TCSAPPL == true)
                            {
                                MSUBLEG.TCSAPPL = "Y";
                            }
                            else
                            {
                                MSUBLEG.TCSAPPL = "N";
                            }
                            MSUBLEG.PROPNAME = VE.M_SUBLEG.PROPNAME;
                            MSUBLEG.ADHAARNO = VE.M_SUBLEG.ADHAARNO;
                            MSUBLEG.GPSLAT = VE.M_SUBLEG.GPSLAT;
                            MSUBLEG.GPSLOT = VE.M_SUBLEG.GPSLOT;
                            MSUBLEG.PHNO1STD = VE.M_SUBLEG.PHNO1STD;
                            MSUBLEG.PHNO1 = VE.M_SUBLEG.PHNO1;
                            MSUBLEG.PHNO2STD = VE.M_SUBLEG.PHNO2STD;
                            MSUBLEG.PHNO2 = VE.M_SUBLEG.PHNO2;
                            MSUBLEG.PHNO3STD = VE.M_SUBLEG.PHNO3STD;
                            MSUBLEG.PHNO3 = VE.M_SUBLEG.PHNO3;
                            MSUBLEG.WEBADDR = VE.M_SUBLEG.WEBADDR;
                            MSUBLEG.OFCEMAIL = VE.M_SUBLEG.OFCEMAIL;
                            MSUBLEG.FACEBOOK_ID = VE.M_SUBLEG.FACEBOOK_ID;
                            MSUBLEG.TWITTER_ID = VE.M_SUBLEG.TWITTER_ID;
                            MSUBLEG.REGEMAILID = VE.M_SUBLEG.REGEMAILID;
                            MSUBLEG.REGMOBILE = VE.M_SUBLEG.REGMOBILE;
                            MSUBLEG.WHATSAPP_NO = VE.M_SUBLEG.WHATSAPP_NO;
                            MSUBLEG.SLPHNO = VE.M_SUBLEG.SLPHNO;
                            MSUBLEG.OTHADD1 = VE.M_SUBLEG.OTHADD1;
                            MSUBLEG.OTHADD2 = VE.M_SUBLEG.OTHADD2;
                            MSUBLEG.OTHADD3 = VE.M_SUBLEG.OTHADD3;
                            MSUBLEG.OTHADD4 = VE.M_SUBLEG.OTHADD4;
                            MSUBLEG.OTHADDEMAIL = VE.M_SUBLEG.OTHADDEMAIL;
                            MSUBLEG.OTHADDPIN = VE.M_SUBLEG.OTHADDPIN;
                            MSUBLEG.OTHADDREM = VE.M_SUBLEG.OTHADDREM;
                            MSUBLEG.ACTNAMEOF = VE.M_SUBLEG.ACTNAMEOF;
                            MSUBLEG.SUBDISTRICT = VE.M_SUBLEG.SUBDISTRICT;

                            MSUBLEG.ADD1 = VE.M_SUBLEG.ADD1.retStr().Trim();
                            MSUBLEG.ADD2 = VE.M_SUBLEG.ADD2.retStr().Trim();
                            MSUBLEG.ADD3 = VE.M_SUBLEG.ADD3.retStr().Trim();
                            MSUBLEG.ADD4 = VE.M_SUBLEG.ADD4.retStr().Trim();
                            MSUBLEG.ADD5 = VE.M_SUBLEG.ADD5.retStr().Trim();
                            MSUBLEG.ADD6 = VE.M_SUBLEG.ADD6.retStr().Trim();
                            MSUBLEG.ADD7 = VE.M_SUBLEG.ADD7.retStr().Trim();
                            string add1 = "";
                            if (VE.M_SUBLEG.BLDGNO != null) { add1 = VE.M_SUBLEG.BLDGNO + " "; };
                            if (VE.M_SUBLEG.FLOORNO != null) { add1 = add1 + VE.M_SUBLEG.FLOORNO + " "; };
                            if (VE.M_SUBLEG.PREMISES != null) { add1 = add1 + VE.M_SUBLEG.PREMISES + " "; };
                            add1 = add1.Trim();
                            MSUBLEG.ADD1 = add1.Length > 60 ? add1.Substring(0, 60) : add1;
                            MSUBLEG.ADD2 = VE.M_SUBLEG.ROADNAME;
                            MSUBLEG.ADD3 = VE.M_SUBLEG.LOCALITY;
                            MSUBLEG.ADD4 = VE.M_SUBLEG.EXTADDR;
                            MSUBLEG.ADD5 = VE.M_SUBLEG.LANDMARK;
                            MSUBLEG.ADD6 = (VE.M_SUBLEG.DISTRICT + " " + VE.M_SUBLEG.PIN).Trim();
                            MSUBLEG.ADD7 = (VE.M_SUBLEG.SUBDISTRICT.retStr() != "" ? "DIST " + VE.M_SUBLEG.SUBDISTRICT.retStr() + " , " + VE.M_SUBLEG.STATE : VE.M_SUBLEG.STATE).Trim();
                            MSUBLEG.PANDT = VE.M_SUBLEG.PANDT;
                            MSUBLEG.PAN_206AB_CCA = VE.M_SUBLEG.PAN_206AB_CCA;
                            MSUBLEG.AUTOREMINDEROFF = VE.AUTOREMINDEROFF == true ? "Y" : "";
                            MSUBLEG.BLWSONACT = VE.BLWSONACT_Checked == true ? "Y" : "";
                            MSUBLEG.MSMENO = VE.M_SUBLEG.MSMENO;
                            if (VE.DefaultAction == "E")
                            {
                                MSUBLEG.SLCD = VE.M_SUBLEG.SLCD;
                                if (VE.M_SUBLEG.M_AUTONO.retDbl() == 0)
                                {
                                    MSUBLEG.M_AUTONO = Cn.M_AUTONO(CommVar.FinSchema(UNQSNO));
                                    action = "A";
                                }
                                else
                                {
                                    MSUBLEG.M_AUTONO = VE.M_SUBLEG.M_AUTONO;
                                }

                                DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == MSUBLEG.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.M_CNTRL_LOCA.RemoveRange(DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == MSUBLEG.M_AUTONO));

                                DB.M_SUBLEG_BUSNAT.Where(x => x.SLCD == MSUBLEG.SLCD).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.M_SUBLEG_BUSNAT.RemoveRange(DB.M_SUBLEG_BUSNAT.Where(x => x.SLCD == MSUBLEG.SLCD));

                                DB.M_SUBLEG_GL.Where(x => x.SLCD == MSUBLEG.SLCD).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.M_SUBLEG_GL.RemoveRange(DB.M_SUBLEG_GL.Where(x => x.SLCD == MSUBLEG.SLCD));

                                DB.M_SUBLEG_LINK.Where(x => x.SLCD == MSUBLEG.SLCD).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.M_SUBLEG_LINK.RemoveRange(DB.M_SUBLEG_LINK.Where(x => x.SLCD == MSUBLEG.SLCD));

                                DB.M_SUBLEG_CONT.Where(x => x.SLCD == MSUBLEG.SLCD).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.M_SUBLEG_CONT.RemoveRange(DB.M_SUBLEG_CONT.Where(x => x.SLCD == MSUBLEG.SLCD));

                                DB.M_SUBLEG_IFSC.Where(x => x.SLCD == MSUBLEG.SLCD).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.M_SUBLEG_IFSC.RemoveRange(DB.M_SUBLEG_IFSC.Where(x => x.SLCD == MSUBLEG.SLCD));

                                DB.M_SUBLEG_TAX.Where(x => x.SLCD == MSUBLEG.SLCD).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.M_SUBLEG_TAX.RemoveRange(DB.M_SUBLEG_TAX.Where(x => x.SLCD == MSUBLEG.SLCD));

                                DB.M_SUBLEG_LOCOTH.Where(x => x.SLCD == MSUBLEG.SLCD).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.M_SUBLEG_LOCOTH.RemoveRange(DB.M_SUBLEG_LOCOTH.Where(x => x.SLCD == MSUBLEG.SLCD));

                                DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == MSUBLEG.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == MSUBLEG.M_AUTONO));

                                DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == MSUBLEG.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == MSUBLEG.M_AUTONO));

                                var MAXEMDNO = (from p in DB.M_SUBLEG where p.M_AUTONO == VE.M_SUBLEG.M_AUTONO select p.EMD_NO).Max();
                                if (MAXEMDNO == null)
                                {
                                    MSUBLEG.EMD_NO = 0;
                                }
                                else
                                {
                                    MSUBLEG.EMD_NO = Convert.ToInt32(MAXEMDNO + 1);
                                }
                            }
                            if (VE.CompanyLocationName != null)
                            {
                                for (int i = 0; i <= VE.CompanyLocationName.Count - 1; i++)
                                {
                                    if (VE.CompanyLocationName[i].Checked)
                                    {
                                        M_CNTRL_LOCA MCL = new M_CNTRL_LOCA();
                                        MCL.M_AUTONO = MSUBLEG.M_AUTONO;
                                        MCL.EMD_NO = MSUBLEG.EMD_NO;
                                        MCL.CLCD = CommVar.ClientCode(UNQSNO);
                                        MCL.COMPCD = VE.CompanyLocationName[i].COMPCD;
                                        MCL.LOCCD = VE.CompanyLocationName[i].LOCCD;
                                        DB.M_CNTRL_LOCA.Add(MCL);
                                    }
                                }
                            }
                            if (VE.BusinessActivity != null)
                            {
                                for (int i = 0; i <= VE.BusinessActivity.Count - 1; i++)
                                {
                                    if (VE.BusinessActivity[i].Checked)
                                    {
                                        M_SUBLEG_BUSNAT MSB = new M_SUBLEG_BUSNAT();
                                        MSB.EMD_NO = MSUBLEG.EMD_NO;
                                        MSB.CLCD = CommVar.ClientCode(UNQSNO);
                                        MSB.SLCD = MSUBLEG.SLCD;
                                        MSB.NATBUSCD = VE.BusinessActivity[i].NATBUSCD;
                                        DB.M_SUBLEG_BUSNAT.Add(MSB);
                                    }
                                }
                            }
                            if (VE.GeneralLedgerDetails != null)
                            {
                                for (int i = 0; i <= VE.GeneralLedgerDetails.Count - 1; i++)
                                {
                                    if (VE.GeneralLedgerDetails[i].Checked)
                                    {
                                        M_SUBLEG_GL MSGL = new M_SUBLEG_GL();
                                        MSGL.EMD_NO = MSUBLEG.EMD_NO;
                                        MSGL.CLCD = CommVar.ClientCode(UNQSNO);
                                        MSGL.SLCD = MSUBLEG.SLCD;
                                        MSGL.GLCD = VE.GeneralLedgerDetails[i].GLCD;
                                        DB.M_SUBLEG_GL.Add(MSGL);
                                    }
                                }
                            }
                            if (VE.LinkType != null)
                            {
                                for (int i = 0; i <= VE.LinkType.Count - 1; i++)
                                {
                                    if (VE.LinkType[i].Checked)
                                    {
                                        M_SUBLEG_LINK MSL = new M_SUBLEG_LINK();
                                        MSL.EMD_NO = MSUBLEG.EMD_NO;
                                        MSL.CLCD = CommVar.ClientCode(UNQSNO);
                                        MSL.SLCD = MSUBLEG.SLCD;
                                        MSL.LINKCD = VE.LinkType[i].LINKCD;
                                        DB.M_SUBLEG_LINK.Add(MSL);
                                    }
                                }
                            }
                            if (VE.MSUBLEGCONT != null)
                            {
                                for (int i = 0; i <= VE.MSUBLEGCONT.Count - 1; i++)
                                {
                                    if (VE.MSUBLEGCONT[i].SLNO != 0 && VE.MSUBLEGCONT[i].CPERSON.retStr() != "")
                                    {
                                        M_SUBLEG_CONT MSC = new M_SUBLEG_CONT();
                                        MSC.CLCD = CommVar.ClientCode(UNQSNO);
                                        MSC.EMD_NO = MSUBLEG.EMD_NO;
                                        MSC.SLCD = MSUBLEG.SLCD;
                                        MSC.SLNO = VE.MSUBLEGCONT[i].SLNO;
                                        MSC.CPERSON = VE.MSUBLEGCONT[i].CPERSON;
                                        MSC.DESIG = VE.MSUBLEGCONT[i].DESIG;
                                        MSC.DEPT = VE.MSUBLEGCONT[i].DEPT;
                                        MSC.EXTENSION = VE.MSUBLEGCONT[i].EXTENSION;
                                        MSC.PHNO1STD = VE.MSUBLEGCONT[i].PHNO1STD;
                                        MSC.PHNO1 = VE.MSUBLEGCONT[i].PHNO1;
                                        MSC.MOBILE1PREFIX = VE.MSUBLEGCONT[i].MOBILE1PREFIX;
                                        MSC.MOBILE1 = VE.MSUBLEGCONT[i].MOBILE1;
                                        MSC.MOBILE2PREFIX = VE.MSUBLEGCONT[i].MOBILE2PREFIX;
                                        MSC.MOBILE2 = VE.MSUBLEGCONT[i].MOBILE2;
                                        MSC.PERSEMAIL = VE.MSUBLEGCONT[i].PERSEMAIL;
                                        MSC.PERSDOB = VE.MSUBLEGCONT[i].PERSDOB;
                                        MSC.PERSDOA = VE.MSUBLEGCONT[i].PERSDOA;
                                        DB.M_SUBLEG_CONT.Add(MSC);
                                    }
                                }
                            }
                            bool flag1 = false;
                            if (VE.MSUBLEGIFSC != null)
                            {
                                for (int i = 0; i <= VE.MSUBLEGIFSC.Count - 1; i++)
                                {//Bank Details Entry
                                    if (VE.MSUBLEGIFSC[i].IFSCCODE.retStr() != "" && VE.MSUBLEGIFSC[i].BANKACTNO.retStr() != "" && VE.MSUBLEGIFSC[i].BANKNAME.retStr() != "")
                                    {
                                        M_SUBLEG_IFSC MSI = new M_SUBLEG_IFSC();
                                        MSI.CLCD = CommVar.ClientCode(UNQSNO);
                                        MSI.EMD_NO = Convert.ToInt16(MSUBLEG.EMD_NO);
                                        MSI.SLCD = MSUBLEG.SLCD;
                                        MSI.SLNO = VE.MSUBLEGIFSC[i].SLNO;
                                        MSI.IFSCCODE = VE.MSUBLEGIFSC[i].IFSCCODE;
                                        MSI.BANKNAME = VE.MSUBLEGIFSC[i].BANKNAME;
                                        MSI.BRANCH = VE.MSUBLEGIFSC[i].BRANCH;
                                        MSI.ADDRESS = VE.MSUBLEGIFSC[i].ADDRESS;
                                        MSI.BANKACTNO = VE.MSUBLEGIFSC[i].BANKACTNO;
                                        MSI.BANKACTTYPE = VE.MSUBLEGIFSC[i].BANKACTTYPE;
                                        if (flag1 != true)
                                        {
                                            if (VE.MSUBLEGIFSC[i].DFLTBNK == true)
                                            {
                                                MSI.DEFLTBANK = "T";
                                                flag1 = true;
                                            }
                                            else
                                            {
                                                MSI.DEFLTBANK = "F";
                                            }
                                        }
                                        else
                                        {
                                            MSI.DEFLTBANK = "F";
                                        }
                                        DB.M_SUBLEG_IFSC.Add(MSI);
                                    }
                                }
                            }
                            if (VE.MSUBLEGTAX != null)
                            {
                                for (int i = 0; i <= VE.MSUBLEGTAX.Count - 1; i++)
                                {
                                    if (VE.MSUBLEGTAX[i].SLNO != 0 && VE.MSUBLEGTAX[i].COMPCD != null)
                                    {
                                        M_SUBLEG_TAX MST = new M_SUBLEG_TAX();
                                        MST.CLCD = CommVar.ClientCode(UNQSNO);
                                        MST.EMD_NO = MSUBLEG.EMD_NO;
                                        MST.SLCD = MSUBLEG.SLCD;
                                        MST.COMPCD = VE.MSUBLEGTAX[i].COMPCD;
                                        MST.TDSLMT = VE.MSUBLEGTAX[i].TDSLMT;
                                        MST.LWRRT = VE.MSUBLEGTAX[i].LWRRT;
                                        MST.INTPER = VE.MSUBLEGTAX[i].INTPER;
                                        DB.M_SUBLEG_TAX.Add(MST);
                                    }
                                }
                            }
                            if (VE.MSUBLEGLOCOTH != null)
                            {
                                for (int i = 0; i <= VE.MSUBLEGLOCOTH.Count - 1; i++)
                                {
                                    if (VE.MSUBLEGLOCOTH[i].SLNO != 0)
                                    {
                                        M_SUBLEG_LOCOTH MSoth = new M_SUBLEG_LOCOTH();
                                        MSoth.CLCD = CommVar.ClientCode(UNQSNO);
                                        MSoth.EMD_NO = MSUBLEG.EMD_NO;
                                        MSoth.SLCD = MSUBLEG.SLCD;
                                        MSoth.COMPCD = VE.MSUBLEGLOCOTH[i].COMPCD;
                                        MSoth.LOCCD = VE.MSUBLEGLOCOTH[i].LOCCD;
                                        MSoth.DISTANCE = VE.MSUBLEGLOCOTH[i].DISTANCE;
                                        DB.M_SUBLEG_LOCOTH.Add(MSoth);
                                    }
                                }
                            }
                            //M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SUBLEG", MSUBLEG.M_AUTONO, VE.DefaultAction, CommVar.FinSchema(UNQSNO));
                            M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SUBLEG", MSUBLEG.M_AUTONO, action, CommVar.FinSchema(UNQSNO), VE.Audit_REM);
                            MCH.PKGLEGACYCD = VE.M_CNTRL_HDR.PKGLEGACYCD;
                            if (VE.DefaultAction == "A")
                            {
                                DB.M_CNTRL_HDR.Add(MCH);
                                DB.M_SUBLEG.Add(MSUBLEG);
                            }

                            else if (VE.DefaultAction == "E")
                            {
                                //DB.Entry(MSUBLEG).State = System.Data.Entity.EntityState.Modified;
                                if (action == "A")
                                {
                                    DB.M_CNTRL_HDR.Add(MCH);
                                    DB.SaveChanges();
                                }
                                else
                                {
                                    DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                                }
                                DB.Entry(MSUBLEG).State = System.Data.Entity.EntityState.Modified;
                            }
                            if (VE.UploadDOC != null)
                            {
                                var img = Cn.SaveUploadImage("M_SUBLEG", VE.UploadDOC, MSUBLEG.M_AUTONO, MSUBLEG.EMD_NO.Value);
                                if (img.Item1.Count != 0)
                                {
                                    DB.M_CNTRL_HDR_DOC.AddRange(img.Item1);
                                    DB.M_CNTRL_HDR_DOC_DTL.AddRange(img.Item2);
                                }
                            }
                            DB.SaveChanges();
                            ModelState.Clear();
                            transaction.Commit();
                            string ContentFlg = "";
                            if (VE.DefaultAction == "A")
                            {
                                ContentFlg = "1" + "<br/><br/>Code:  " + MSUBLEG.SLCD + "    " + "  Name:  " + MSUBLEG.SLNM + "    " + "  District:  " + MSUBLEG.DISTRICT + "    " + "  Area:  " + MSUBLEG.SLAREA;
                            }
                            else if (VE.DefaultAction == "E")
                            {
                                ContentFlg = "2";
                            }
                            if (Other_Para == "Upload")
                                return ContentFlg;
                            else
                                return Content(ContentFlg);
                        }

                        else if (VE.DefaultAction == "V")
                        {

                            M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SUBLEG", VE.M_SUBLEG.M_AUTONO, VE.DefaultAction, CommVar.FinSchema(UNQSNO), VE.Audit_REM);
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                            DB.SaveChanges();

                            DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == VE.M_SUBLEG.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });

                            DB.M_SUBLEG.Where(x => x.M_AUTONO == VE.M_SUBLEG.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });

                            DB.M_SUBLEG_GL.Where(x => x.SLCD == VE.M_SUBLEG.SLCD).ToList().ForEach(x => { x.DTAG = "D"; });

                            DB.M_SUBLEG_BUSNAT.Where(x => x.SLCD == VE.M_SUBLEG.SLCD).ToList().ForEach(x => { x.DTAG = "D"; });

                            DB.M_SUBLEG_LINK.Where(x => x.SLCD == VE.M_SUBLEG.SLCD).ToList().ForEach(x => { x.DTAG = "D"; });

                            DB.M_SUBLEG_CONT.Where(x => x.SLCD == VE.M_SUBLEG.SLCD).ToList().ForEach(x => { x.DTAG = "D"; });

                            DB.M_SUBLEG_IFSC.Where(x => x.SLCD == VE.M_SUBLEG.SLCD).ToList().ForEach(x => { x.DTAG = "D"; });

                            DB.M_SUBLEG_TAX.Where(x => x.SLCD == VE.M_SUBLEG.SLCD).ToList().ForEach(x => { x.DTAG = "D"; });

                            DB.M_SUBLEG_LOCOTH.Where(x => x.SLCD == VE.M_SUBLEG.SLCD).ToList().ForEach(x => { x.DTAG = "D"; });

                            DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SUBLEG.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });

                            DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SUBLEG.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });

                            DB.SaveChanges();

                            DB.M_CNTRL_LOCA.RemoveRange(DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == VE.M_SUBLEG.M_AUTONO));
                            DB.SaveChanges();

                            DB.M_SUBLEG_BUSNAT.RemoveRange(DB.M_SUBLEG_BUSNAT.Where(x => x.SLCD == VE.M_SUBLEG.SLCD));
                            DB.SaveChanges();

                            DB.M_SUBLEG_GL.RemoveRange(DB.M_SUBLEG_GL.Where(x => x.SLCD == VE.M_SUBLEG.SLCD));
                            DB.SaveChanges();

                            DB.M_SUBLEG_LINK.RemoveRange(DB.M_SUBLEG_LINK.Where(x => x.SLCD == VE.M_SUBLEG.SLCD));
                            DB.SaveChanges();

                            DB.M_SUBLEG_CONT.RemoveRange(DB.M_SUBLEG_CONT.Where(x => x.SLCD == VE.M_SUBLEG.SLCD));
                            DB.SaveChanges();

                            DB.M_SUBLEG_IFSC.RemoveRange(DB.M_SUBLEG_IFSC.Where(x => x.SLCD == VE.M_SUBLEG.SLCD));
                            DB.SaveChanges();

                            DB.M_SUBLEG_TAX.RemoveRange(DB.M_SUBLEG_TAX.Where(x => x.SLCD == VE.M_SUBLEG.SLCD));
                            DB.SaveChanges();

                            DB.M_SUBLEG_LOCOTH.RemoveRange(DB.M_SUBLEG_LOCOTH.Where(x => x.SLCD == VE.M_SUBLEG.SLCD));
                            DB.SaveChanges();

                            DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SUBLEG.M_AUTONO));
                            DB.SaveChanges();

                            DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SUBLEG.M_AUTONO));
                            DB.SaveChanges();

                            DB.M_SUBLEG.RemoveRange(DB.M_SUBLEG.Where(x => x.M_AUTONO == VE.M_SUBLEG.M_AUTONO));
                            DB.SaveChanges();

                            DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_SUBLEG.M_AUTONO));
                            DB.SaveChanges();
                            ModelState.Clear();
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
                        // Retrieve the error messages as a list of strings.
                        var errorMessages = ex.EntityValidationErrors
                                .SelectMany(x => x.ValidationErrors)
                                .Select(x => x.ErrorMessage);

                        // Join the list to a single string.
                        var fullErrorMessage = string.Join("&quot;", errorMessages);

                        // Combine the original exception message with the new one.
                        var exceptionMessage = string.Concat(ex.Message, "<br/> &quot; The validation errors are: &quot;", fullErrorMessage);

                        // Throw a new DbEntityValidationException with the improved exception message.
                        throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.Clear();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + " " + ex.InnerException);
                }
            }
        }
        public ActionResult Print(SubLedgerEntry VE)
        {
            ReportViewinHtml rvh = new ReportViewinHtml();
            Cn.getQueryString(rvh);
            rvh.TEXTBOX1 = VE.M_SUBLEG.SLCD;
            rvh.TEXTBOX6 = VE.M_SUBLEG.SLNM;
            if (TempData["printparameter"] != null)
            {
                TempData.Remove("printparameter");
            }
            TempData["printparameter"] = rvh;
            return Content("");
        }
        public ActionResult GetPartyGroupDetails(string val)
        {
            try
            {
                var str = MasterHelpFa.PARTYCD_help(val);
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
        private string Getcomptype(string comtype)
        {
            string VARR = "";
            switch (comtype)//01
            {
                case "C":
                    VARR = "02"; break;
                case "P":
                    VARR = "05"; break;
                case "F":
                    VARR = "03"; break;
                case "H":
                    VARR = "08"; break;
                case "T":
                    VARR = "10"; break;
                default: VARR = ""; break;
            }
            //C  Company
            //P  Person
            //H  HUF(Hindu Undivided Family)
            //F  Firm
            //A  Association of Persons(AOP)
            //T AOP (Trust)
            //B Body of Individuals(BOI)
            //L Local Authority
            //J  Artificial Juridical Person
            //G  Govt
            return VARR;
        }
        public JsonResult GetGstInfo(string GSTNO)
        {
            try
            {
                AdaequareGSP adaequareGSP = new AdaequareGSP();
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                Dictionary<string, string> dic = new Dictionary<string, string>();
                //var AdqrRespGstInfo = adaequareGSP.AdqrGstInfoTestMode(GSTNO);
                var AdqrRespGstInfo = adaequareGSP.AdqrGstInfo(GSTNO);
                if (AdqrRespGstInfo.success == true && AdqrRespGstInfo.result != null)
                {
                    dic.Add("message", "ok");
                    dic.Add("Gstin", AdqrRespGstInfo.result.Gstin);
                    string StateCd = AdqrRespGstInfo.result.Gstin.Substring(0, 2);
                    string StateNm = DB1.MS_STATE.Find(StateCd)?.STATENM;
                    string panno = AdqrRespGstInfo.result.Gstin.Substring(2, 10);
                    string comtype = panno.Substring(3, 1);
                    dic.Add("StateCd", StateCd);
                    dic.Add("StateNm", StateNm);
                    dic.Add("Panno", panno);
                    dic.Add("Comptype", Getcomptype(comtype));
                    dic.Add("TradeName", AdqrRespGstInfo.result.TradeName);
                    if (AdqrRespGstInfo.result.TradeName == AdqrRespGstInfo.result.LegalName)
                    {
                        dic.Add("LegalName", "");
                    }
                    else
                    {
                        dic.Add("LegalName", AdqrRespGstInfo.result.LegalName);
                    }
                    dic.Add("AddrBnm", AdqrRespGstInfo.result.AddrBnm);
                    dic.Add("AddrBno", AdqrRespGstInfo.result.AddrBno);
                    dic.Add("AddrFlno", AdqrRespGstInfo.result.AddrFlno);
                    dic.Add("AddrSt", AdqrRespGstInfo.result.AddrSt.retStr());
                    dic.Add("AddrLoc", AdqrRespGstInfo.result.AddrLoc);
                    dic.Add("AddrPncd", AdqrRespGstInfo.result.AddrPncd.retStr());
                    dic.Add("TxpType", AdqrRespGstInfo.result.TxpType);
                }
                else
                {
                    dic.Add("message", AdqrRespGstInfo.message);
                }
                ModelState.Clear();
                return Json(dic, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Json(ex.Message + ex.InnerException, JsonRequestBehavior.AllowGet);
            }
        }
        public DataTable ToDataTable(ExcelWorksheet ws)
        {
            var tbl = new DataTable();
            foreach (var firstRowCell in ws.Cells[5, 1, 5, ws.Dimension.End.Column])
                tbl.Columns.Add(firstRowCell.Text);
            var startRow = 6;
            for (var rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
            {
                var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                var row = tbl.NewRow();
                foreach (var cell in wsRow) row[cell.Start.Column - 1] = cell.Value;
                tbl.Rows.Add(row);
            }
            return tbl;

        }
        public ActionResult UploadIncomeTaxFiles(SubLedgerEntry VE, FormCollection FC, HttpPostedFileBase file)
        {
            CS = Cn.GetConnectionString();
            Cn.con = new OracleConnection(CS);
            if (Cn.con.State == ConnectionState.Closed)
            {
                Cn.con.Open();
            }
            var new1 = Cn.con.BeginTransaction();
            try
            {
                string scm1 = CommVar.FinSchema(UNQSNO);
                DataTable IncomeTaxExceldt = new DataTable();
                if (file.ContentLength > 0)
                {
                    string fileExtension = System.IO.Path.GetExtension(file.FileName);
                    #region excel to datatable
                    if (fileExtension != ".xlsx")
                    {
                        return Json("File Extention must be [.XLSX] ");
                    }
                    using (var package = new ExcelPackage(file.InputStream))
                    {
                        var currentSheet = package.Workbook.Worksheets;
                        var workSheet = currentSheet.First();
                        IncomeTaxExceldt = ToDataTable(workSheet);
                    }
                    #endregion
                    for (int i = 0; i < IncomeTaxExceldt.Rows.Count; i++)
                    {
                        string sql = "";
                        try
                        {
                            //	PAN	Name	PAN Allotment Date	PAN-Aadhaar Link Status	Specified Person u/s 206AB & 206CCA
                            string PANno = IncomeTaxExceldt.Rows[i]["PAN"].ToString();
                            string PANDTstr = (IncomeTaxExceldt.Rows[i]["PAN Allotment Date"]).retStr();
                            DateTime Pandate = DateTime.ParseExact(PANDTstr, "dd-MM-yyyy", null);
                            PANDTstr = Pandate.retDateStr();
                            string PAN_206AB_CCA = IncomeTaxExceldt.Rows[i]["Specified Person u/s 206AB & 206CCA"].retStr();
                            PAN_206AB_CCA = PAN_206AB_CCA == "No" ? "N" : "Y";
                            sql = "update " + scm1 + ".M_SUBLEG set ";
                            sql += "PANDT=to_date('" + PANDTstr + "', 'dd/mm/yyyy'),";
                            sql += "PAN_206AB_CCA='" + PAN_206AB_CCA + "' ";
                            sql += " where panno='" + PANno + "' ";
                            Cn.com = new OracleCommand(sql, Cn.con);
                            Cn.com.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Cn.SaveException(ex, "Subledger update Pan sql:" + sql + "\n excel row no:" + i + 6);
                        }

                    }
                }
                new1.Commit();
                Cn.con.Close();
                ModelState.Clear();
                return Json("S");
            }
            catch (Exception ex)
            {
                new1.Rollback();
                Cn.SaveException(ex, "");
                return Json(ex.InnerException + ex.Message);
            }

        }
        [HttpPost]
        public ActionResult SUB_LEDGER(SubLedgerEntry VE, FormCollection FC, string Command)
        {
            try
            {
                Cn.getQueryString(VE);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                if (Command == "DownloadTemplate")
                {
                    #region Download
                    string MNUP = VE.MENU_PARA; string nm = "Customer Master";
                    string Excel_Header = "SL_CD" + "|" + "SL_NM" + "|" + "FULLNAME" + "|" + "" + "|" + "BLDNGNO" + "|" + "PREMISES" + "|" + "FLOORNO" + "|" + "ROADNAME"
                    + "|" + "LOCALITY" + "|" + "EXTADDR" + "|" + "LANDMARK" + "|" + "STATE" + "|" + "DISTRICT"
                    + "|" + "PIN" + "|" + "STATE_CD" + "|" + "COUNTRY" + "|" + "SL_AREA" + "|" + "ITFILE" + "|" + "TANNO" + "|" + "LTD_IND" + "|" + "CINNO"
                    + "|" + "LEGALNAME" + "|" + "ADHARNO" + "|" + "GST" + "|" + "GSTDT" + "|" + "REGNTYPE" + "|" + "EMAIL" + "|" + "MOBILE" + "|" + "CPERSON" + "|" + "CPERSONMOB"
                    + "|" + "PHNO1STD" + "|" + "PHNO1" + "|" + "PHNO2" + "|" + "PHNO2STD" + "|" + "PHNO3STD" + "|" + "PHNO3" + "|" + "OLDPHNO1" + "|" + "OLDPHNO2" + "|" + "CPERSON2"
                    + "|" + "CPERSON2MOBILE" + "|" + "FAX" + "|" + "FACEBOOK" + "|" + "TWITTER" + "|" + "WHATSAPP" + "|" + "IFSC" + "|" + "BANKNM" + "|" + "BANKADD"
                    + "|" + "BANKACT" + "|" + "BACNKACTTYPE" + "|" + "IFSC2" + "|" + "BANKNM2" + "|" + "BANKADD2" + "|" + "BANKACT2" + "|" + "BACNKACTTYPE2"
                    + "|" + "ACODE" + "|" + "TRAN_CD" + "|" + "LINKCD" + "|" + "FASMARTLINKCD" + "|" + "OTHADD1" + "|" + "OTHADD2" + "|" + "OTHADD3" + "|" + "OTHREM";

                    ExcelPackage ExcelPkg = new ExcelPackage();
                    ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("Sheet1");
                    using (ExcelRange Rng = wsSheet1.Cells["A1:BJ1"])
                    {
                        Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        string[] Header = Excel_Header.Split('|');
                        for (int i = 0; i < Header.Length; i++)
                        {
                            wsSheet1.Cells[1, i + 1].Value = Header[i];
                        }
                    }
                    wsSheet1.Cells[1, 1, 1, 61].AutoFitColumns();
                    Response.Clear();
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("Content-Disposition", "attachment; filename=Template_" + nm + ".xlsx");
                    Response.BinaryWrite(ExcelPkg.GetAsByteArray());
                    Response.Flush();
                    Response.Close();
                    Response.End();
                    return Content("Download Sucessfull");
                    #endregion
                }
                else
                {
                    #region Upload
                    if (Request.Files.Count == 0) return Content("No File Selected");
                    HttpPostedFileBase file = Request.Files[0];
                    if (System.IO.Path.GetExtension(file.FileName) != ".xlsx") return Content(".xlsx file need to choose");
                    Stream stream = file.InputStream;

                    string msg = "", MESSAGE = "", errorstat = "", strerrline = ""; //sl = new M_RTMAST();
                    try
                    {
                        // Excel Columns: SL_CD	SL_NM	PRNTNM	BLNKFLD	BLDNGNO	PREMISES	FLOORNO	ROADNAME	LOCALITY	EXTADDR	LANDMARK	STATE	CITY	PIN	STATE_CD	COUNTRY	SL_AREA	ITFILE	TANNO	LTD_IND	CINNO	LEGALNAME	ADHARNO	GST	GSTDT	REGNTYPE	EMAIL	MOBILE	CPERSON	CPERSONMOB	PHNO1STD	PHNO1	PHNO2STD	PHNO2	PHNO3STD	PHNO3	PHNO	RPHNO	CPERSON2	CPERSON2MOBILE	FAX	FACEBOOK	TWITTER	WHATSAPP	IFSC	BANKNM	BANKADD	BANKACT	BACNKACTTYPE	IFSC2	BANKNM2	BANKADD2	BANKACT2	BACNKACTTYPE2	ACODE	TRAN_CD	LINKCD	FASMARTLINKCD	OTHADD1	OTHADD2	OTHADD3	OTHREM

                        DataTable dbfdt = new DataTable();
                        dbfdt.Columns.Add("Sl", typeof(int));
                        dbfdt.Columns.Add("SL_CD", typeof(string));
                        dbfdt.Columns.Add("SL_NM", typeof(string));
                        dbfdt.Columns.Add("PRNTNM", typeof(string));
                        dbfdt.Columns.Add("BLNKFLD", typeof(string));
                        dbfdt.Columns.Add("BLDNGNO", typeof(string));
                        dbfdt.Columns.Add("PREMISES", typeof(string));
                        dbfdt.Columns.Add("FLOORNO", typeof(string));
                        dbfdt.Columns.Add("ROADNAME", typeof(string));
                        dbfdt.Columns.Add("LOCALITY", typeof(string));
                        dbfdt.Columns.Add("EXTADDR", typeof(string));
                        dbfdt.Columns.Add("LANDMARK", typeof(string));
                        dbfdt.Columns.Add("STATE", typeof(string));
                        dbfdt.Columns.Add("CITY", typeof(string));
                        dbfdt.Columns.Add("PIN", typeof(string));
                        dbfdt.Columns.Add("STATE_CD", typeof(string));
                        dbfdt.Columns.Add("COUNTRY", typeof(string));
                        dbfdt.Columns.Add("SL_AREA", typeof(string));
                        dbfdt.Columns.Add("ITFILE", typeof(string));
                        dbfdt.Columns.Add("TANNO", typeof(string));
                        dbfdt.Columns.Add("LTD_IND", typeof(string));
                        dbfdt.Columns.Add("CINNO", typeof(string));
                        dbfdt.Columns.Add("LEGALNAME", typeof(string));
                        dbfdt.Columns.Add("ADHARNO", typeof(string));
                        dbfdt.Columns.Add("GST", typeof(string));
                        dbfdt.Columns.Add("GSTDT", typeof(string));
                        dbfdt.Columns.Add("REGNTYPE", typeof(string));
                        dbfdt.Columns.Add("EMAIL", typeof(string));
                        dbfdt.Columns.Add("MOBILE", typeof(string));
                        dbfdt.Columns.Add("CPERSON", typeof(string));
                        dbfdt.Columns.Add("CPERSONMOB", typeof(string));
                        dbfdt.Columns.Add("PHNO1STD", typeof(string));
                        dbfdt.Columns.Add("PHNO1", typeof(string));
                        dbfdt.Columns.Add("PHNO2STD", typeof(string));
                        dbfdt.Columns.Add("PHNO2", typeof(string));
                        dbfdt.Columns.Add("PHNO3STD", typeof(string));
                        dbfdt.Columns.Add("PHNO3", typeof(string));
                        dbfdt.Columns.Add("PHNO", typeof(string));
                        dbfdt.Columns.Add("RPHNO", typeof(string));
                        dbfdt.Columns.Add("CPERSON2", typeof(string));
                        dbfdt.Columns.Add("CPERSON2MOBILE", typeof(string));
                        dbfdt.Columns.Add("FAX", typeof(string));
                        dbfdt.Columns.Add("FACEBOOK", typeof(string));
                        dbfdt.Columns.Add("TWITTER", typeof(string));
                        dbfdt.Columns.Add("WHATSAPP", typeof(string));
                        dbfdt.Columns.Add("IFSC", typeof(string));
                        dbfdt.Columns.Add("BANKNM", typeof(string));
                        dbfdt.Columns.Add("BANKADD", typeof(string));
                        dbfdt.Columns.Add("BANKACT", typeof(string));
                        dbfdt.Columns.Add("BACNKACTTYPE", typeof(string));
                        dbfdt.Columns.Add("IFSC2", typeof(string));
                        dbfdt.Columns.Add("BANKNM2", typeof(string));
                        dbfdt.Columns.Add("BANKADD2", typeof(string));
                        dbfdt.Columns.Add("BANKACT2", typeof(string));
                        dbfdt.Columns.Add("BACNKACTTYPE2", typeof(string));
                        dbfdt.Columns.Add("ACODE", typeof(string));
                        dbfdt.Columns.Add("TRAN_CD", typeof(string));
                        dbfdt.Columns.Add("LINKCD", typeof(string));
                        dbfdt.Columns.Add("FASMARTLINKCD", typeof(string));
                        dbfdt.Columns.Add("OTHADD1", typeof(string));
                        dbfdt.Columns.Add("OTHADD2", typeof(string));
                        dbfdt.Columns.Add("OTHADD3", typeof(string));
                        dbfdt.Columns.Add("OTHREM", typeof(string));

                        dbfdt.Columns.Add("FULLNAME", typeof(string));
                        dbfdt.Columns.Add("DISTRICT", typeof(string));
                        dbfdt.Columns.Add("OLDPHNO1", typeof(string));
                        dbfdt.Columns.Add("OLDPHNO2", typeof(string));


                        using (var package = new ExcelPackage(stream))
                        {
                            var currentSheet = package.Workbook.Worksheets;
                            var workSheet = currentSheet.First();
                            var noOfCol = workSheet.Dimension.End.Column;
                            var noOfRow = workSheet.Dimension.End.Row;
                            int rowNum = 2;
                            for (rowNum = 2; rowNum <= noOfRow; rowNum++)
                            {
                                if (workSheet.Cells[rowNum, 2].Value.retStr() != "")
                                {
                                    DataRow dr = dbfdt.NewRow();
                                    dr["Sl"] = rowNum;
                                    var wsRow = workSheet.Cells[rowNum, 1, rowNum, noOfCol];
                                    for (int colnum = 1; colnum <= noOfCol; colnum++)
                                    {
                                        string colname = workSheet.Cells[1, colnum].Value.retStr().Trim();
                                        string colValue = workSheet.Cells[rowNum, colnum].Value.retStr().Trim();
                                        try
                                        {
                                            if (colname == "") continue;
                                            dr[colname] = colValue;
                                        }
                                        catch (ArgumentException ex)
                                        {
                                            return Content("Wrong ColumnName:" + colname + " Error:" + ex.Message);
                                        }
                                    }
                                    dbfdt.Rows.Add(dr);
                                }
                            }
                        }



                        CultureInfo culture = CultureInfo.CreateSpecificCulture("en-GB");
                        VE.DefaultAction = "A";


                        int excelrow = 2; int slno = 0;
                        string DISTRICT = "", STATE_CD = "", STATE = "", COUNTRY = "", PIN = "", LINKCD = "", LTD_IND = "", REGNTYPE = "", slnm = "", MOBILE = "", CPERSONMOB = "";

                        foreach (DataRow oudr in dbfdt.Rows)
                        {
                            string ERROR = "";
                            slnm = oudr["SL_NM"].retStr().Length > 45 ? oudr["SL_NM"].retStr().Substring(0, 45) : oudr["SL_NM"].retStr();
                            //var chkslnm = CheckSubledgerName(slnm, "Y");
                            //if (chkslnm.retStr() != "")
                            //{
                            //    ERROR = ERROR + chkslnm + ". At Row Number : " + oudr["Sl"].retStr() + "<br/>";
                            //    strerrline += excelrow + ",";
                            //}
                            DISTRICT = oudr["DISTRICT"].retStr();
                            if (DISTRICT == "")
                            {
                                ERROR = ERROR + "DISTRICT not found !! At Row Number : " + oudr["Sl"].retStr() + "<br/>";
                            }
                            PIN = oudr["PIN"].retStr();
                            if (PIN == "")
                            {
                                ERROR = ERROR + "PIN not found !! At Row Number : " + oudr["Sl"].retStr() + "<br/>";
                            }
                            STATE_CD = oudr["STATE_CD"].retStr();
                            if (STATE_CD == "")
                            {
                                ERROR = ERROR + "STATE_CD not found !! At Row Number : " + oudr["Sl"].retStr() + "<br/>";
                            }
                            STATE = oudr["STATE"].retStr();
                            if (STATE == "")
                            {
                                ERROR = ERROR + "STATE not found !! At Row Number : " + oudr["Sl"].retStr() + "<br/>";
                            }
                            COUNTRY = oudr["COUNTRY"].retStr();
                            if (COUNTRY == "")
                            {
                                ERROR = ERROR + "COUNTRY not found !! At Row Number : " + oudr["Sl"].retStr() + "<br/>";
                            }
                            LINKCD = oudr["LINKCD"].retStr();
                            if (LINKCD == "")
                            {
                                ERROR = ERROR + "LINKCD not found !! At Row Number : " + oudr["Sl"].retStr() + "<br/>";
                            }
                            LTD_IND = oudr["LTD_IND"].retStr();
                            if (LTD_IND == "")
                            {
                                ERROR = ERROR + "LTD_IND not found !! At Row Number : " + oudr["Sl"].retStr() + "<br/>";
                            }
                            REGNTYPE = oudr["REGNTYPE"].retStr();
                            if (REGNTYPE == "")
                            {
                                ERROR = ERROR + "REGNTYPE not found !! At Row Number : " + oudr["Sl"].retStr() + "<br/>";
                            }

                            bool isNumeric = true;
                            MOBILE = oudr["MOBILE"].retStr();
                            int length = MOBILE.Length;
                            if (length > 12)
                            {
                                isNumeric = false;
                            }
                            foreach (char c in MOBILE)
                            {
                                if (!char.IsDigit(c))
                                {
                                    isNumeric = false;
                                    break;
                                }
                            }
                            if (isNumeric == false)
                            {
                                ERROR = ERROR + "MOBILE is not valid !! At Row Number : " + oudr["Sl"].retStr() + "<br/>";
                            }

                            bool isNumeric1 = true;
                            CPERSONMOB = oudr["CPERSONMOB"].retStr();
                            int length1 = CPERSONMOB.Length;
                            if (length1 > 12)
                            {
                                isNumeric1 = false;
                            }
                            foreach (char c in CPERSONMOB)
                            {
                                if (!char.IsDigit(c))
                                {
                                    isNumeric1 = false;
                                    break;
                                }
                            }
                            if (isNumeric1 == false)
                            {
                                ERROR = ERROR + "CPERSONMOB is not valid !! At Row Number : " + oudr["Sl"].retStr() + "<br/>";
                            }

                            if (ERROR != "")
                            {
                                errorstat += ERROR;
                                ++excelrow;
                                continue;
                            }
                            else
                            {
                                SUB_LEDGERController MSCntlr = new SUB_LEDGERController();
                                SubLedgerEntry SRMEVE = new SubLedgerEntry();
                                M_SUBLEG MSUBLEG = new M_SUBLEG();
                                M_CNTRL_HDR MCH = new M_CNTRL_HDR();
                                List<MSUBLEGCONT> MSUBLEGCONTLlist = new List<Models.MSUBLEGCONT>();
                                List<MSUBLEGIFSC> MSUBLEGIFSCLlist = new List<Models.MSUBLEGIFSC>();
                                List<LinkType> LinkTypeLlist = new List<Models.LinkType>();

                                slno++;
                                msg = " Excelrow:" + excelrow;
                                MCH.PKGLEGACYCD = oudr["SL_CD"].retStr();

                                MSUBLEG.SLNM = oudr["SL_NM"].retStr().Length > 45 ? oudr["SL_NM"].retStr().Substring(0, 45) : oudr["SL_NM"].retStr();
                                MSUBLEG.FULLNAME = oudr["FULLNAME"].retStr();
                                //MSUBLEG.PRNTNM = oudr["PRNTNM"].retStr();
                                //MSUBLEG.BLNKFLD = oudr["BLNKFLD"].retStr();
                                MSUBLEG.BLDGNO = oudr["BLDNGNO"].retStr();
                                MSUBLEG.PREMISES = oudr["PREMISES"].retStr().Length > 80 ? oudr["PREMISES"].retStr().Substring(0, 80) : oudr["PREMISES"].retStr();
                                MSUBLEG.FLOORNO = oudr["FLOORNO"].retStr().Length > 80 ? oudr["FLOORNO"].retStr().Substring(0, 80) : oudr["FLOORNO"].retStr();
                                MSUBLEG.ROADNAME = oudr["ROADNAME"].retStr();
                                MSUBLEG.LOCALITY = oudr["LOCALITY"].retStr();
                                MSUBLEG.EXTADDR = oudr["EXTADDR"].retStr();

                                MSUBLEG.LANDMARK = oudr["LANDMARK"].retStr();
                                MSUBLEG.STATE = oudr["STATE"].retStr();
                                //MSUBLEG.DISTRICT = oudr["CITY"].retStr();
                                MSUBLEG.DISTRICT = oudr["DISTRICT"].retStr();

                                MSUBLEG.PIN = oudr["PIN"].retStr();
                                MSUBLEG.STATECD = oudr["STATE_CD"].retStr();
                                MSUBLEG.COUNTRY = oudr["COUNTRY"].retStr();
                                MSUBLEG.SLAREA = oudr["SL_AREA"].retStr();
                                //MSUBLEG.ITFILE = oudr["ITFILE"].retStr();
                                MSUBLEG.PANNO = oudr["ITFILE"].retStr();
                                MSUBLEG.TANNO = oudr["TANNO"].retStr();
                                //MSUBLEG.LTD_IND = oudr["LTD_IND"].retStr();
                                MSUBLEG.CINNO = oudr["CINNO"].retStr();
                                MSUBLEG.PROPNAME = oudr["LEGALNAME"].retStr();
                                MSUBLEG.ADHAARNO = oudr["ADHARNO"].retStr();
                                MSUBLEG.GSTNO = oudr["GST"].retStr();
                                if (oudr["GSTDT"].retStr() != "")
                                {
                                    MSUBLEG.GSTDT = Convert.ToDateTime(oudr["GSTDT"].retStr());
                                }
                                MSUBLEG.REGEMAILID = oudr["EMAIL"].retStr();
                                MSUBLEG.REGMOBILE = Convert.ToInt64(oudr["MOBILE"].retDbl());
                                MSUBLEG.PHNO1STD = oudr["PHNO1STD"].retInt();
                                MSUBLEG.PHNO1 = Convert.ToInt64(oudr["PHNO1"].retDbl());
                                MSUBLEG.PHNO2STD = oudr["PHNO2STD"].retInt();
                                MSUBLEG.PHNO2 = Convert.ToInt64(oudr["PHNO2"].retDbl());
                                MSUBLEG.PHNO3STD = oudr["PHNO3STD"].retInt();
                                MSUBLEG.PHNO3 = Convert.ToInt64(oudr["PHNO3"].retDbl());
                                MSUBLEG.SLPHNO = oudr["OLDPHNO1"].retStr();
                                //MSUBLEG.PHNO1STD = oudr["PHNO"].retInt();
                                //MSUBLEG.PHNO1 = Convert.ToInt64(oudr["RPHNO"].retDbl());

                                //MSUBLEG.FAX = oudr["FAX"].retStr();
                                MSUBLEG.FACEBOOK_ID = oudr["FACEBOOK"].retStr();
                                MSUBLEG.TWITTER_ID = oudr["TWITTER"].retStr();
                                MSUBLEG.WHATSAPP_NO = Convert.ToInt64(oudr["WHATSAPP"].retDbl());
                                //MSUBLEG.ACODE = oudr["ACODE"].retStr();
                                //MSUBLEG.TRAN_CD = oudr["TRAN_CD"].retStr();
                                //MSUBLEG.FASMARTLINKCD = oudr["FASMARTLINKCD"].retStr();
                                MSUBLEG.CENNO = oudr["ACODE"].retStr() + "," + oudr["TRAN_CD"].retStr();
                                MSUBLEG.OTHADD1 = oudr["OTHADD1"].retStr();
                                MSUBLEG.OTHADD2 = oudr["OTHADD2"].retStr();
                                MSUBLEG.OTHADD3 = oudr["OTHADD3"].retStr();
                                MSUBLEG.OTHADDREM = oudr["OTHREM"].retStr();
                                MSUBLEG.SLCOMPTYPE = oudr["LTD_IND"].retStr() == "I" ? "07" : "02";
                                // MSUBLEG.REGNTYPE = "";
                                MSUBLEG.REGNTYPE = oudr["REGNTYPE"].retStr();

                                MSUBLEGCONT SUBLEGCONT1 = new MSUBLEGCONT();
                                //SUBLEGCONT.MOBILE1PREFIX = Convert.ToByte(oudr["CPERSON"].retDbl());
                                SUBLEGCONT1.SLNO = 1;
                                SUBLEGCONT1.CPERSON = oudr["CPERSON"].retStr();
                                SUBLEGCONT1.MOBILE1 = Convert.ToInt64(oudr["CPERSONMOB"].retDbl());
                                MSUBLEGCONTLlist.Add(SUBLEGCONT1);

                                MSUBLEGCONT SUBLEGCONT2 = new MSUBLEGCONT();
                                //SUBLEGCONT.MOBILE2PREFIX = Convert.ToByte(oudr["CPERSON2"].retDbl());
                                SUBLEGCONT2.SLNO = 2;
                                SUBLEGCONT2.CPERSON = oudr["CPERSON2"].retStr();
                                SUBLEGCONT2.MOBILE1 = Convert.ToInt64(oudr["CPERSON2MOBILE"].retDbl());
                                MSUBLEGCONTLlist.Add(SUBLEGCONT2);

                                MSUBLEGIFSC SUBLEGIFSC1 = new MSUBLEGIFSC();
                                SUBLEGIFSC1.IFSCCODE = oudr["IFSC"].retStr();
                                SUBLEGIFSC1.BANKNAME = oudr["BANKNM"].retStr();
                                SUBLEGIFSC1.ADDRESS = oudr["BANKADD"].retStr();
                                SUBLEGIFSC1.BANKACTNO = oudr["BANKACT"].retStr();
                                SUBLEGIFSC1.BANKACTTYPE = oudr["BACNKACTTYPE"].retStr();
                                SUBLEGIFSC1.BRANCH = oudr["BANKNM"].retStr() != "" ? "." : "";
                                SUBLEGIFSC1.SLNO = 1;

                                MSUBLEGIFSCLlist.Add(SUBLEGIFSC1);

                                MSUBLEGIFSC SUBLEGIFSC2 = new MSUBLEGIFSC();

                                SUBLEGIFSC2.IFSCCODE = oudr["IFSC2"].retStr();
                                SUBLEGIFSC2.BANKNAME = oudr["BANKNM2"].retStr();
                                SUBLEGIFSC2.ADDRESS = oudr["BANKADD2"].retStr();
                                SUBLEGIFSC2.BANKACTNO = oudr["BANKACT2"].retStr();
                                SUBLEGIFSC2.BANKACTTYPE = oudr["BACNKACTTYPE2"].retStr();
                                SUBLEGIFSC2.BRANCH = oudr["BANKNM2"].retStr() != "" ? "." : "";
                                SUBLEGIFSC2.SLNO = 2;
                                MSUBLEGIFSCLlist.Add(SUBLEGIFSC2);


                                LinkType LinkType1 = new LinkType();
                                LinkType1.LINKCD = oudr["LINKCD"].retStr();
                                LinkType1.Checked = true;
                                LinkTypeLlist.Add(LinkType1);


                                SRMEVE.DefaultAction = "A";
                                SRMEVE.M_SUBLEG = MSUBLEG;
                                SRMEVE.MSUBLEGCONT = MSUBLEGCONTLlist;
                                SRMEVE.MSUBLEGIFSC = MSUBLEGIFSCLlist;
                                SRMEVE.LinkType = LinkTypeLlist;
                                SRMEVE.M_CNTRL_HDR = MCH;
                                FormCollection collection = new FormCollection();
                                string tslCont = (string)MSCntlr.SAVE(collection, SRMEVE, "Upload");
                                tslCont = tslCont.retStr().Split('~')[0];
                                if (tslCont.Length > 0 && tslCont.Substring(0, 1) == "1")
                                {
                                    strerrline += excelrow + ",";
                                    ++excelrow;
                                    MESSAGE += "Success : " + tslCont.Remove(0, 11) + ". At Row Number : " + oudr["Sl"].retStr() + "<br/>";
                                }
                                else
                                {
                                    errorstat += tslCont + ". At Row Number : " + oudr["Sl"].retStr() + "<br/>";
                                }
                            }
                        }//outer
                        if (errorstat != "")
                        {
                            goto notsave;
                        }
                        return Content("Excel Uploaded Successfully !! <br/>" + MESSAGE);


                    }//try
                    catch (Exception ex)
                    {
                        Cn.SaveException(ex, msg);
                        string InnerException = "";
                        if (ex.InnerException != null)
                        {
                            if (ex.InnerException.InnerException != null)
                            {
                                InnerException = ex.InnerException.InnerException.Message;
                            }
                            else
                            {
                                InnerException = ex.InnerException.Message;
                            }
                        }
                        errorstat += "<br/>" + ex.Message + " at " + msg+" "+ InnerException;
                        goto notsave;
                        //return Content(ex.Message + " at " + msg + MESSAGE);
                    }
                    notsave:
                    {
                        using (var ExcelPkg = new ExcelPackage(stream))
                        {
                            string[] errline = strerrline.Split(',');
                            int rowNum = 2;
                            var noOfRow = ExcelPkg.Workbook.Worksheets[1].Dimension.End.Row;
                            for (rowNum = noOfRow; rowNum >= 2; rowNum--)
                            {
                                if (errline.Contains(rowNum.retStr()))
                                {
                                    ExcelPkg.Workbook.Worksheets[1].DeleteRow(rowNum, 1);
                                }
                            }
                            ExcelPkg.Workbook.Worksheets.Add("ErrorMsg");

                            rowNum = 1;
                            string[] arrerrmsg = errorstat.Split(new string[] { "<br/>" }, StringSplitOptions.None);
                            foreach (var v in arrerrmsg)
                            {
                                ExcelPkg.Workbook.Worksheets["ErrorMsg"].Cells[rowNum, 1].Value = v;
                                rowNum++;
                            }
                            ExcelPkg.Workbook.Worksheets.Add("SuccessMsg");

                            rowNum = 1;
                            string[] arrsmsg = MESSAGE.Split(new string[] { "<br/>" }, StringSplitOptions.None);
                            foreach (var v in arrsmsg)
                            {
                                ExcelPkg.Workbook.Worksheets["SuccessMsg"].Cells[rowNum, 1].Value = v;
                                rowNum++;
                            }
                            Response.Clear();
                            Response.ClearContent();
                            Response.Buffer = true;
                            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                            Response.AddHeader("Content-Disposition", "attachment; filename=ErrorExcel.xlsx");
                            Response.BinaryWrite(ExcelPkg.GetAsByteArray());
                            Response.Flush();
                            Response.Close();
                            Response.End();
                            return Content("Excel Uploaded Successfully !!");
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
    }
}