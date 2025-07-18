﻿using CrystalDecisions.CrystalReports.Engine;
using Improvar.Models;
using Improvar.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using iTextSharp.text.html;
using iTextSharp.text.html.simpleparser;
using Oracle.ManagedDataAccess.Client;
using System.Web;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Text;

namespace Improvar.Controllers
{
    public class M_FinProductController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        MasterHelpFa Master_HelpFa = new MasterHelpFa(); Salesfunc salesfunc = new Salesfunc();
        M_SITEM sl; M_CNTRL_HDR sll; M_GROUP slll; M_SUBBRAND slsb; M_BRAND slb; M_COLLECTION slc; M_UOM sluom; M_PRODGRP sPROD;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        DropDownHelp DropDownHelp = new DropDownHelp();

        // GET: M_FinProduct
        public ActionResult M_FinProduct(string op = "", string key = "", int Nindex = 0, string searchValue = "", string loadItem = "N")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    //ViewBag.formname = "Finish Product/Design Master";
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ItemMasterEntry VE = new ItemMasterEntry();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    string MNUP = VE.MENU_PARA;
                    switch (MNUP)
                    {
                        case "F":
                            ViewBag.formname = "Finish Product/Design Master"; break;
                        case "C":
                            ViewBag.formname = "Fabric Item"; break;
                        case "A":
                            ViewBag.formname = "Accessories / Other Items"; break;
                        default: ViewBag.formname = ""; break;
                    }
                    VE.Database_Combo1 = (from n in DB.M_SITEM
                                          join j in DB.M_GROUP on n.ITGRPCD equals j.ITGRPCD
                                          where j.ITGRPTYPE == "F"
                                          select new Database_Combo1() { FIELD_VALUE = n.HSNCODE }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                    VE.Database_Combo2 = (from n in DB.M_SITEM_MEASURE
                                          select new Database_Combo2() { FIELD_VALUE = n.MDESC }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                    VE.DropDown_list_ITGRP = DropDownHelp.GetItgrpcdforSelection();
                    VE.Itgrpnm = masterHelp.ComboFill("itgrpcd", VE.DropDown_list_ITGRP, 0, 1);

                    VE.DropDown_list_ITEM = DropDownHelp.GetItcdforSelection();
                    VE.Itnm = masterHelp.ComboFill("itcd", VE.DropDown_list_ITEM, 0, 1);

                    VE.DropDown_list_COLLNM = DropDownHelp.GetCOLLNMforSelection();
                    VE.collcd = masterHelp.ComboFill("collcd", VE.DropDown_list_COLLNM, 0, 1);

                    //=================For Gender================//
                    List<Gender> G = new List<Gender>();
                    Gender G7 = new Gender();
                    G7.text = "";
                    G7.value = "";
                    G.Add(G7);
                    Gender G1 = new Gender();
                    G1.text = "Boys";
                    G1.value = "B";
                    G.Add(G1);
                    Gender G2 = new Gender();
                    G2.text = "Girls";
                    G2.value = "G";
                    G.Add(G2);
                    Gender G3 = new Gender();
                    G3.text = "Infant";
                    G3.value = "I";
                    G.Add(G3);
                    Gender G4 = new Gender();
                    G4.text = "Men";
                    G4.value = "M";
                    G.Add(G4);
                    Gender G5 = new Gender();
                    G5.text = "Women";
                    G5.value = "W";
                    G.Add(G5);
                    Gender G6 = new Gender();
                    G6.text = "Unisex";
                    G6.value = "U";
                    G.Add(G6);

                    VE.Gender = G;
                    //=================For Gender================//
                    //=================For ProductType================//
                    List<ProductType> ptlist = new List<ProductType>();
                    ProductType P1 = new ProductType();
                    P1.text = "Inner Wear";
                    P1.value = "IW";
                    ptlist.Add(P1);
                    ProductType P2 = new ProductType();
                    P2.text = "Outer Wear";
                    P2.value = "OW";
                    ptlist.Add(P2);
                    ProductType P3 = new ProductType();
                    P3.text = "Accessories";
                    P3.value = "AC";
                    ptlist.Add(P3);
                    VE.ProductType = ptlist;
                    //=================For ProductType================//
                    List<DropDown_list1> PRICES_EFFDTDROP = new List<DropDown_list1>();
                    VE.DropDown_list1 = PRICES_EFFDTDROP;
                    VE.DefaultAction = op;
                    if (op.Length != 0)
                    {
                        var itgrpcd = "";
                        if (VE.MENU_PARA == "F")
                        {
                            itgrpcd = "F";
                        }
                        else if (VE.MENU_PARA == "C")
                        {
                            itgrpcd = "C";
                        }

                        //sl = DB.M_SITEM.Where(r => r.ITCD.Remove(1) == "C" && r.ITCD == itcd).FirstOrDefault();
                        if (VE.MENU_PARA == "F" || VE.MENU_PARA == "C")
                        {
                            VE.IndexKey = (from p in DB.M_SITEM
                                           join o in DB.M_GROUP on p.ITGRPCD equals (o.ITGRPCD)
                                           where (p.ITGRPCD == o.ITGRPCD && o.ITGRPTYPE == itgrpcd)
                                           select new IndexKey() { Navikey = p.ITCD }).OrderBy(a => a.Navikey).ToList();
                        }
                        else
                        {
                            VE.IndexKey = (from p in DB.M_SITEM
                                           join o in DB.M_GROUP on p.ITGRPCD equals (o.ITGRPCD)
                                           where (p.ITGRPCD == o.ITGRPCD && o.ITGRPTYPE != "C" && o.ITGRPTYPE != "F")
                                           select new IndexKey() { Navikey = p.ITCD }).OrderBy(a => a.Navikey).ToList();
                        }

                        if (searchValue != "") { Nindex = VE.IndexKey.FindIndex(r => r.Navikey.Equals(searchValue)); }
                        VE.SrcFlagCaption = "Design/Style/Item Code";
                        if (op == "E" || op == "D" || op == "V" || loadItem == "Y")
                        {

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
                            //VE.M_SITEM = sl;
                            VE.M_CNTRL_HDR = sll;
                            VE.M_GROUP = slll;
                            VE.M_SUBBRAND = slsb;
                            VE.M_BRAND = slb;
                            VE.M_COLLECTION = slc;
                            VE.M_UOM = sluom;
                            VE.M_PRODGRP = sPROD;
                        }
                        if (op.ToString() == "A" && loadItem == "N")
                        {
                            //List<MSITEMSLCD> ITEMSIZE = new List<MSITEMSLCD>();
                            //MSITEMSLCD MIS = new MSITEMSLCD();
                            //MIS.SRLNO = "1";
                            //ITEMSIZE.Add(MIS);
                            //VE.MSITEMSLCD = ITEMSIZE;
                            List<MSITEMSLCD> MSITEMSLCD = new List<MSITEMSLCD>();
                            for (int i = 0; i < 5; i++)
                            {
                                MSITEMSLCD ITEMSIZE = new MSITEMSLCD();
                                ITEMSIZE.SRLNO = Convert.ToString(i + 1);
                                MSITEMSLCD.Add(ITEMSIZE);
                            }
                            VE.MSITEMSLCD = MSITEMSLCD;

                            //List<MSITEMBOX> ITEMBOX = new List<MSITEMBOX>();
                            //MSITEMBOX MIB = new MSITEMBOX();
                            //MIB.SRLNO = "1";
                            //ITEMBOX.Add(MIB);
                            //VE.MSITEMBOX = ITEMBOX;

                            //List<MSITEMCOLOR> ITEMCOLOR = new List<MSITEMCOLOR>();
                            //MSITEMCOLOR MIC = new MSITEMCOLOR();
                            //MIC.SLNO = 1;
                            //ITEMCOLOR.Add(MIC);
                            //VE.MSITEMCOLOR = ITEMCOLOR;
                            //List<MSITEMCOLOR> ITEMCOLOR = new List<MSITEMCOLOR>();
                            //for (int i = 0; i < 5; i++)
                            //{
                            //    MSITEMCOLOR MIC = new MSITEMCOLOR();
                            //    MIC.SLNO = Convert.ToByte(i + 1);
                            //    ITEMCOLOR.Add(MIC);
                            //}
                            //VE.MSITEMCOLOR = ITEMCOLOR;

                            List<MSITEMPARTS> ITEMPARTS = new List<MSITEMPARTS>();
                            MSITEMPARTS MIP = new MSITEMPARTS();
                            MIP.SLNO = 1;
                            ITEMPARTS.Add(MIP);
                            VE.MSITEMPARTS = ITEMPARTS;

                            List<MSITEMBARCODE> SITEMBARCODE = new List<MSITEMBARCODE>();
                            for (int i = 0; i < 15; i++)
                            {
                                MSITEMBARCODE MII = new MSITEMBARCODE();
                                MII.SRLNO = Convert.ToByte(i + 1);
                                SITEMBARCODE.Add(MII);
                            }
                            VE.MSITEMBARCODE = SITEMBARCODE;

                            List<MSITEMMEASURE> ITEMMEASURE = new List<MSITEMMEASURE>();
                            for (int i = 0; i < 10; i++)
                            {
                                MSITEMMEASURE MIM = new MSITEMMEASURE();
                                MIM.SLNO = Convert.ToByte(i + 1);
                                ITEMMEASURE.Add(MIM);
                            }
                            VE.MSITEMMEASURE = ITEMMEASURE;

                            List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                            UploadDOC UPL = new UploadDOC();
                            UPL.DocumentType = doctP;
                            UploadDOC1.Add(UPL);
                            VE.UploadDOC = UploadDOC1;

                        }
                        var MSYSCNFG = salesfunc.M_SYSCNFG(System.DateTime.Now.Date.retDateStr());
                        if (MSYSCNFG != null)
                        {
                            VE.COMMONUIQBAR = MSYSCNFG.COMMONUNIQBAR;
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
                ItemMasterEntry VE = new ItemMasterEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public ItemMasterEntry Navigation(ItemMasterEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new M_SITEM(); sll = new M_CNTRL_HDR(); slll = new M_GROUP(); slsb = new M_SUBBRAND(); slb = new M_BRAND(); slc = new M_COLLECTION(); sluom = new M_UOM();
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
                var itcd = aa[0];
                //if (VE.MENU_PARA == "C")
                //{
                //    sl = DB.M_SITEM.Where(r => r.ITCD.Remove(1) != "F" && r.ITCD.Remove(1) != "A" && r.ITCD == itcd).FirstOrDefault();
                //}
                //else if (VE.MENU_PARA == "F")
                //{
                //    sl = DB.M_SITEM.Where(r => r.ITCD.Remove(1) != "C" && r.ITCD.Remove(1) != "A" && r.ITCD == itcd).FirstOrDefault();
                //}
                //else { sl = DB.M_SITEM.Where(r => r.ITCD.Remove(1) != "C" && r.ITCD.Remove(1) != "F" && r.ITCD == itcd).FirstOrDefault(); }
                sl = DB.M_SITEM.Find(itcd);
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
                slll = DB.M_GROUP.Find(sl.ITGRPCD);
                slsb = DB.M_SUBBRAND.Find(sl.SBRANDCD);
                slb = DB.M_BRAND.Find(sl.BRANDCD);
                slc = DB.M_COLLECTION.Find(sl.COLLCD);
                sluom = DBF.M_UOM.Find(sl.UOMCD);
                sPROD = DB.M_PRODGRP.Find(sl.PRODGRPCD);

                VE.HASTRANSACTION = salesfunc.IsTransactionFound(itcd.retSqlformat(), "", "") != "" ? true : false;
                string fitcd = sl.FABITCD.retStr();
                if (fitcd != "")
                {
                    var fitem = (from a in DB.M_SITEM where a.ITCD == fitcd select new { a.ITNM, a.STYLENO, a.UOMCD }).FirstOrDefault();
                    VE.FABITNM = fitem.ITNM;
                    VE.FABSTYLENO = fitem.STYLENO;
                    VE.FABUOMNM = fitem.UOMCD;
                }
                if (sl.CONVUOMCD.retStr() != "")
                {
                    VE.CONVUOMNM = DBF.M_UOM.Where(a => a.UOMCD == sl.CONVUOMCD).Select(a => a.UOMNM).FirstOrDefault();
                }
                if (sl.NEGSTOCK == "Y")
                {
                    VE.NEGSTOCK = true;
                }
                else
                {
                    VE.NEGSTOCK = false;
                }
                if (sll.INACTIVE_TAG == "Y")
                {
                    VE.Checked = true;
                }
                else
                {
                    VE.Checked = false;
                }
                if (sl.LINKITCD.retStr() != "")
                {
                    var fitem = (from a in DB.M_SITEM where a.ITCD == sl.LINKITCD select new { a.ITNM, a.STYLENO, a.UOMCD }).FirstOrDefault();
                    VE.LINKITNM = fitem.STYLENO + " " + fitem.ITNM;
                    VE.LINKUOMCD = fitem.UOMCD;
                }
                VE.FAVITEMChecked = sl.FAVITEM == "Y" ? true : false;

                string sql = "";
                sql += "select i.SLCD,j.SLNM,i.JOBRT,i.PDESIGN from " + CommVar.CurSchema(UNQSNO) + ".M_SITEM_SLCD i,";
                sql += CommVar.FinSchema(UNQSNO) + ".M_SUBLEG j where i.SLCD =j.SLCD and i.ITCD='" + sl.ITCD + "'";
                DataTable dt = Master_HelpFa.SQLquery(sql);
                VE.MSITEMSLCD = (from DataRow dr in dt.Rows
                                 select new MSITEMSLCD()
                                 {
                                     SLCD = dr["SLCD"].retStr(),
                                     SLNM = dr["SLNM"].retStr(),
                                     JOBRT = dr["JOBRT"].retDbl(),
                                     PDESIGN = dr["PDESIGN"].retStr()
                                 }).ToList();
                if (VE.MSITEMSLCD.Count == 0)
                {
                    List<MSITEMSLCD> ITEMSIZE = new List<MSITEMSLCD>();
                    MSITEMSLCD MIS = new MSITEMSLCD();
                    MIS.SRLNO = "1";
                    ITEMSIZE.Add(MIS);
                    VE.MSITEMSLCD = ITEMSIZE;
                }
                else
                {
                    for (int i = 0; i <= VE.MSITEMSLCD.Count - 1; i++)
                    {
                        VE.MSITEMSLCD[i].SRLNO = (i + 1).ToString();
                    }
                }

                VE.MSITEMPARTS = (from i in DB.M_SITEM_PARTS
                                  join j in DB.M_PARTS on i.PARTCD equals (j.PARTCD)
                                  where (i.ITCD == sl.ITCD)
                                  select new MSITEMPARTS()
                                  {
                                      PARTCD = i.PARTCD,
                                      PARTNM = j.PARTNM,
                                  }).ToList();

                if (VE.MSITEMPARTS.Count == 0)
                {
                    List<MSITEMPARTS> ITEMPARTS = new List<MSITEMPARTS>();
                    MSITEMPARTS MIP = new MSITEMPARTS();
                    MIP.SLNO = 1;
                    ITEMPARTS.Add(MIP);
                    VE.MSITEMPARTS = ITEMPARTS;
                }
                else
                {
                    for (int i = 0; i <= VE.MSITEMPARTS.Count - 1; i++)
                    {
                        VE.MSITEMPARTS[i].SLNO = Convert.ToByte(i + 1);
                    }
                }
                sql = "";
                sql += "SELECT i.SIZECD,  k.SIZENM,k.SZBARCODE,i.COLRCD,j.COLRNM, i.BARNO, j.CLRBARCODE,i.PDESIGN, ";
                sql += "case when exists (select autono from " + CommVar.CurSchema(UNQSNO) + ".T_BATCHdtl a where a.BARNO = i.BARNO) then 'Y' else '' end as HASTRANSACTION ";
                sql += "FROM " + CommVar.CurSchema(UNQSNO) + ".T_BATCHmst i, " + CommVar.CurSchema(UNQSNO) + ".M_COLOR j, " + CommVar.CurSchema(UNQSNO) + ".M_SIZE k ";
                sql += "where I.COLRCD = j.colrcd(+) and I.SIZECD = k.SIZECD(+) and i.COMMONUNIQBAR='C' and i.itcd = '" + sl.ITCD + "' ";
                sql += " order by COLRCD NULLS first,sizecd NULLS first,K.PRINT_SEQ asc ";
                dt = masterHelp.SQLquery(sql);
                VE.MSITEMBARCODE = (from DataRow dr in dt.Rows
                                    select new MSITEMBARCODE()
                                    {
                                        SIZECD = dr["SIZECD"].ToString(),
                                        SIZENM = dr["SIZENM"].ToString(),
                                        SZBARCODE = dr["SZBARCODE"].ToString(),
                                        COLRCD = dr["COLRCD"].ToString(),
                                        COLRNM = dr["COLRNM"].ToString(),
                                        BARNO = dr["BARNO"].ToString(),
                                        PDESIGN = dr["PDESIGN"].ToString(),
                                        CLRBARCODE = dr["CLRBARCODE"].ToString(),
                                        HASTRANSACTION = dr["HASTRANSACTION"].ToString() == "Y" ? true : false
                                    }).ToList();

                if (VE.MSITEMBARCODE.Count == 0)
                {
                    List<MSITEMBARCODE> SITEMBARCODE = new List<MSITEMBARCODE>();
                    MSITEMBARCODE MII = new MSITEMBARCODE();
                    MII.SRLNO = 1;
                    SITEMBARCODE.Add(MII);
                    VE.MSITEMBARCODE = SITEMBARCODE;
                }
                else
                {
                    for (int i = 0; i <= VE.MSITEMBARCODE.Count - 1; i++)
                    {
                        VE.MSITEMBARCODE[i].SRLNO = Convert.ToByte((i + 1));
                        if (VE.DefaultAction == "A")
                        {
                            VE.MSITEMBARCODE[i].BARNO = null;
                        }
                    }
                }
                VE.MSITEMMEASURE = (from i in DB.M_SITEM_MEASURE
                                    where (i.ITCD == sl.ITCD)
                                    select new MSITEMMEASURE()
                                    {
                                        MDESC = i.MDESC,
                                        MVAL = i.MVAL,
                                        REM = i.REM,
                                    }).ToList();
                if (VE.MSITEMMEASURE.Count == 0)
                {
                    List<MSITEMMEASURE> ITEMMEASURE = new List<MSITEMMEASURE>();
                    MSITEMMEASURE MIM = new MSITEMMEASURE();
                    MIM.SLNO = 1;
                    ITEMMEASURE.Add(MIM);
                    VE.MSITEMMEASURE = ITEMMEASURE;
                }
                else
                {
                    for (int i = 0; i <= VE.MSITEMMEASURE.Count - 1; i++)
                    {
                        VE.MSITEMMEASURE[i].SLNO = Convert.ToByte((i + 1));
                    }
                }
                VE.M_SITEM = sl;
                string barnosql = getmasterbarno(sl.ITCD).retSqlformat();
                if (barnosql.retStr() != "")
                {
                    VE.DropDown_list1 = Price_Effdt(VE, barnosql);
                }
                if (VE.DropDown_list1.Count > 0)
                {
                    VE.PRICES_EFFDT = VE.DropDown_list1.First().value;
                    VE.PRICES_EFFDTDROP = VE.DropDown_list1.First().value;
                    VE.DTPRICES = GetPrices(VE);
                }
                VE.UploadDOC = Cn.GetUploadImage(CommVar.CurSchema(UNQSNO).ToString(), Convert.ToInt32(sl.M_AUTONO));

                if (VE.UploadDOC.Count == 0)
                {
                    List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                    UploadDOC UPL = new UploadDOC();
                    UPL.DocumentType = doctP;
                    UploadDOC1.Add(UPL);
                    VE.UploadDOC = UploadDOC1;
                }
                sql = "select * from " + CommVar.CurSchema(UNQSNO) + ".T_BATCH_IMG_HDR WHERE barno in(" + barnosql + ")";
                dt = masterHelp.SQLquery(sql);
                if (dt != null && dt.Rows.Count > 0)
                {
                    VE.UploadBarImages = (from DataRow dr in dt.Rows
                                          select new UploadDOC
                                          {
                                              docID = Path.GetFileNameWithoutExtension(dr["DOC_FLNAME"].retStr()),
                                              DOC_DESC = dr["DOC_DESC"].retStr(),
                                              DOC_FILE = CommVar.WebUploadDocURL(dr["DOC_FLNAME"].retStr()),
                                              DOC_FILE_NAME = dr["DOC_FLNAME"].retStr(),
                                          }).ToList();
                    foreach (var v in VE.UploadBarImages)
                    {
                        string FROMpath = CommVar.SaveFolderPath() + "/ItemImages/" + v.DOC_FILE_NAME;
                        FROMpath = Path.Combine(FROMpath, "");
                        string TOPATH = CommVar.LocalUploadDocPath() + v.DOC_FILE_NAME;
                        var tyy = Url.Action();
                        Cn.CopyImage(FROMpath, TOPATH);
                        VE.BarImages += Cn.GCS() + CommVar.WebUploadDocURL(v.DOC_FILE_NAME) + "~" + v.DOC_DESC;
                    }
                }


                if (VE.DefaultAction == "A")
                {
                    VE.SEARCH_ITCD = sl.ITCD;
                    sl.ITCD = null;
                    //VE.ITEM_BARCODE = null;
                    sl.STYLENO = null;

                }
            }
            else
            {
                VE.M_SITEM = sl;
                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                UploadDOC UPL = new UploadDOC();
                UPL.DocumentType = doctP;
                UploadDOC1.Add(UPL);
                VE.UploadDOC = UploadDOC1;
                List<MSITEMSLCD> ITEMSIZE = new List<MSITEMSLCD>();
                MSITEMSLCD MIS = new MSITEMSLCD();
                MIS.SRLNO = "1";
                ITEMSIZE.Add(MIS);
                VE.MSITEMSLCD = ITEMSIZE;
            }
            ImprovarDB DB_PREVYR_temp = new ImprovarDB(Cn.GetConnectionString(), CommVar.LastYearSchema(UNQSNO));
            if (CommVar.LastYearSchema(UNQSNO) == "")
            {
                VE.isPresentinLastYrSchema = "";
            }
            else
            {
                var ITCD = sl.ITCD;
                VE.isPresentinLastYrSchema = (from j in DB_PREVYR_temp.M_SITEM where (j.ITCD == ITCD) select j.ITCD).FirstOrDefault();
                if (string.IsNullOrEmpty(VE.isPresentinLastYrSchema))
                {
                    VE.isPresentinLastYrSchema = "ADD";
                }
                else
                {
                    VE.isPresentinLastYrSchema = "EDIT";
                }
            }
            return VE;
        }

        public ActionResult SearchPannelData(string SRC_FLAG)
        {
            try
            {
                ItemMasterEntry VE = new ItemMasterEntry();
                Cn.getQueryString(VE);
                string MNUP = VE.MENU_PARA;

                string scm = CommVar.CurSchema(UNQSNO);
                string str = "select distinct a.itcd, d.itnm, d.styleno, d.itgrpcd, e.itgrpnm, f.COLLNM, d.uomcd, nvl(b.rate, 0) wprate, nvl(c.rate, 0) rprate from ";
                str += "(select a.itcd, b.barno ";
                str += "from " + scm + ".m_sitem a, " + scm + ".T_BATCHmst b ";
                str += "where a.itcd = b.itcd(+) ) a, ";
                str += "(select barno, rate from ( ";
                str += "select a.barno, a.effdt, a.rate, ";
                str += "row_number() over(partition by a.barno, a.effdt order by a.effdt desc) as rn ";
                str += "from " + scm + ".T_BATCHMST_PRICE a ";
                //str += "where a.prccd = 'WP' ) where rn = 1 ) b, ";
                str += "where a.prccd = 'WP' ";
                if (CommVar.ClientCode(UNQSNO) == "DIWH" || CommVar.ClientCode(UNQSNO) == "SNFP") str += "and a.effdt=(select max(effdt) from " + scm + ".T_BATCHMST_PRICE  where barno = a.barno and prccd = 'WP'  ) ";//for diwans/SN FABRIC max effdt rate comes
                else str += "and nvl(a.rate,0) !=0 ";
                str += ") where rn = 1 ) b, ";

                str += "(select barno, rate from ( ";
                str += "select a.barno, a.effdt, a.rate, ";
                str += "row_number() over(partition by a.barno, a.effdt order by a.effdt desc) as rn ";
                str += "from " + scm + ".T_BATCHMST_PRICE a ";
                //str += "where a.prccd = 'WP' ) where rn = 1 ) b, ";
                str += "where a.prccd = 'RP' ";
                if (CommVar.ClientCode(UNQSNO) == "DIWH" || CommVar.ClientCode(UNQSNO) == "SNFP") str += "and a.effdt=(select max(effdt) from " + scm + ".T_BATCHMST_PRICE  where barno = a.barno and prccd = 'RP'  ) ";//for diwans/SN FABRIC max effdt rate comes
                else str += "and nvl(a.rate,0) !=0 ";
                str += " ) where rn = 1 ) c, ";

                str += "" + scm + ".m_sitem d, " + scm + ".m_group e, " + scm + ".M_COLLECTION f ";
                str += "where a.barno = b.barno(+) and a.barno = c.barno(+) and a.itcd = d.itcd(+) and d.itgrpcd = e.itgrpcd(+) and d.COLLCD = f.COLLCD(+) ";
                if (MNUP == "F" || MNUP == "C") str += " and e.itgrptype='" + MNUP + "' ";
                else str += " and e.itgrptype NOT IN ('F','C') ";
                if (SRC_FLAG.retStr() != "") str += "and(upper(d.styleno) like '%" + SRC_FLAG.retStr().ToUpper() + "%' or upper(d.itcd) like '%" + SRC_FLAG.retStr().ToUpper() + "%' ) ";
                DataTable MDT = masterHelp.SQLquery(str);

                System.Text.StringBuilder SB = new System.Text.StringBuilder();
                var hdr = "Design No" + Cn.GCS() + "Group Name" + Cn.GCS() + "Group" + Cn.GCS() + "Item Name" + Cn.GCS() + "WP Rate" + Cn.GCS() + "Retail Rate" + Cn.GCS() + "Collection Name" + Cn.GCS() + "Item Code";
                for (int j = 0; j <= MDT.Rows.Count - 1; j++)
                {
                    SB.Append("<tr><td>" + MDT.Rows[j]["STYLENO"].retStr() + "</td><td>" + MDT.Rows[j]["ITGRPNM"].retStr() + " </td><td> " + MDT.Rows[j]["ITGRPCD"].retStr() + "</td><td>" + MDT.Rows[j]["ITNM"].retStr() + " </td><td> " + MDT.Rows[j]["wprate"].retStr() + "</td><td> " + MDT.Rows[j]["rprate"].retStr() + "</td><td> " + MDT.Rows[j]["COLLNM"].retStr() + "</td><td>" + MDT.Rows[j]["ITCD"].retStr() + "</td></tr>");
                }
                return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "7"));
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GeneratePrices(ItemMasterEntry VE, string Tag)
        {
            try
            {
                string barnosql = getmasterbarno(VE.M_SITEM.ITCD.retStr()).retSqlformat();
                if (barnosql != "")
                {
                    VE.DropDown_list1 = Price_Effdt(VE, barnosql);
                }
                else
                {
                    VE.DropDown_list1 = new List<DropDown_list1>();
                }
                VE.DTPRICES = GetPrices(VE, Tag);
                TempData["DTPRICES"] = VE.DTPRICES;

                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_M_FinProduct_Prices", VE);
            }
            catch (Exception ex)
            {

            }
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_M_FinProduct_Prices", VE);
        }
        public ActionResult DeletePrices(ItemMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string sql = "";
            OracleConnection OraCon = new OracleConnection(Cn.GetConnectionString());
            OraCon.Open();
            OracleCommand OraCmd = OraCon.CreateCommand();
            using (OracleTransaction OraTrans = OraCon.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                OraCmd.Transaction = OraTrans;
                try
                {
                    DataTable DTPRICES = (DataTable)TempData["DTPRICES"]; TempData.Keep();
                    string barno = DTPRICES.AsEnumerable().Select(a => a.Field<string>("barno")).ToArray().retSqlfromStrarray();
                    sql = "delete from " + CommVar.CurSchema(UNQSNO) + ".T_BATCHMST_PRICE where barno in (" + barno + ") and effdt = to_date('" + VE.PRICES_EFFDT + "','dd/mm/yyyy') ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    sql = "delete from " + CommVar.CurSchema(UNQSNO) + ".T_BATCHMST_PRICE where barno in (" + barno + ") and effdt = to_date('" + VE.PRICES_EFFDT + "','dd/mm/yyyy') ";
                    OraCmd.CommandText = sql; OraCmd.ExecuteNonQuery();

                    string barnosql = getmasterbarno(VE.M_SITEM.ITCD).retSqlformat();
                    VE.DropDown_list1 = Price_Effdt(VE, barnosql);
                    if (VE.DropDown_list1.Count > 0)
                    {
                        VE.PRICES_EFFDT = VE.DropDown_list1.First().value;
                        VE.PRICES_EFFDTDROP = VE.DropDown_list1.First().value;
                        VE.DTPRICES = GetPrices(VE);
                    }

                    ModelState.Clear();
                    OraTrans.Commit();
                    OraTrans.Dispose();
                    VE.DefaultView = true;
                    return PartialView("_M_FinProduct_Prices", VE);
                }
                catch (Exception ex)
                {
                    OraTrans.Rollback();
                    OraTrans.Dispose();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }

        }
        public List<DropDown_list1> Price_Effdt(ItemMasterEntry VE, string barno)
        {
            string sql = "select distinct EFFDT from " + CommVar.CurSchema(UNQSNO) + ".T_BATCHMST_PRICE where barno in (" + barno + ") order by EFFDT desc";
            //string sql = "select distinct EFFDT from " + CommVar.CurSchema(UNQSNO) + ".T_BATCHMST_PRICE order by EFFDT desc";
            DataTable dt = masterHelp.SQLquery(sql);
            VE.DropDown_list1 = (from DataRow Dr in dt.Rows
                                 select new DropDown_list1() { value = Dr["EFFDT"].retDateStr(), text = Dr["EFFDT"].retDateStr() }).ToList();
            return VE.DropDown_list1;

        }
        public DataTable GetPrices(ItemMasterEntry VE, string Tag = "")
        {
            DataTable dt = new DataTable();
            try
            {
                string sql = "";
                sql += " select a.rate,prccd,SIZECD,COLRCD,to_char(a.effdt,'dd/mm/yyyy')effdt from " + CommVar.CurSchema(UNQSNO) + ".T_BATCHMST_PRICE a," + CommVar.CurSchema(UNQSNO) + ".T_BATCHmst b ";
                sql += "where a.barno=b.barno and b.itcd='" + VE.M_SITEM.ITCD + "' ";
                if (Tag != "All") sql += "and a.effdt = to_date('" + VE.PRICES_EFFDT + "','dd/mm/yyyy')  ";
                sql += "order by EFFDT desc ";
                DataTable dt_prcrt = masterHelp.SQLquery(sql);
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                var M_PRCLST = (from p in DBF.M_PRCLST
                                select new
                                {
                                    PRCCD = p.PRCCD,
                                    PRCNM = p.PRCNM,
                                    SEQNO = p.SEQNO
                                }).OrderBy(S => S.SEQNO).ToList();
                DataColumn column;

                column = dt.Columns.Add("effdt", typeof(string)); column.Caption = "Effective Date";
                column = dt.Columns.Add("COLRCD", typeof(string)); column.Caption = "COLRCD";
                column = dt.Columns.Add("COLRNM", typeof(string)); column.Caption = "COLRNM";
                column = dt.Columns.Add("CLRBARCODE", typeof(string)); column.Caption = "CLRBARCODE";
                column = dt.Columns.Add("SIZECD", typeof(string)); column.Caption = "SIZECD";
                column = dt.Columns.Add("SIZENM", typeof(string)); column.Caption = "SIZENM";
                column = dt.Columns.Add("SZBARCODE", typeof(string)); column.Caption = "SZBARCODE";
                column = dt.Columns.Add("BARNO", typeof(string)); column.Caption = "BARNO";

                foreach (var plist in M_PRCLST)
                {
                    column = dt.Columns.Add(plist.PRCCD, typeof(string)); column.Caption = plist.PRCNM;
                }

                List<string> effdt = new List<string>();
                if (Tag == "All")
                {
                    for (int i = 0; i <= VE.DropDown_list1.Count - 1; i++)
                    {
                        effdt.Add(VE.DropDown_list1[i].value);
                    }

                }
                else
                {
                    effdt.Add(VE.PRICES_EFFDTDROP);
                }
                foreach (MSITEMBARCODE bar in VE.MSITEMBARCODE)
                {
                    if (bar.SRLNO == 1 || bar.SIZECD != null || bar.COLRCD != null)
                    {
                        foreach (var e in effdt)
                        {
                            dt.Rows.Add("");
                            int rNo = dt.Rows.Count - 1;

                            dt.Rows[rNo]["effdt"] = e.retStr();
                            dt.Rows[rNo]["SIZECD"] = bar.SIZECD.retStr();
                            dt.Rows[rNo]["SIZENM"] = bar.SIZENM;
                            dt.Rows[rNo]["SZBARCODE"] = bar.SZBARCODE;
                            dt.Rows[rNo]["COLRCD"] = bar.COLRCD.retStr();
                            dt.Rows[rNo]["COLRNM"] = bar.COLRNM;
                            dt.Rows[rNo]["CLRBARCODE"] = bar.CLRBARCODE;
                            dt.Rows[rNo]["BARNO"] = bar.BARNO;
                            if (dt_prcrt != null && dt_prcrt.Rows.Count > 0)
                            {
                                foreach (var plist in M_PRCLST)
                                {
                                    string rate = (from DataRow dr in dt_prcrt.Rows
                                                   where dr["sizecd"].retStr() == bar.SIZECD.retStr() && dr["colrcd"].retStr() == bar.COLRCD.retStr() && dr["prccd"].retStr() == plist.PRCCD.retStr() && dr["effdt"].retStr() == e.retStr()
                                                   select dr["rate"].retStr()).FirstOrDefault();
                                    dt.Rows[rNo][plist.PRCCD] = rate;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
            TempData["DTPRICES"] = dt;
            return dt;
        }
        public ActionResult GetItemDetails(string val, string Code)
        {
            try
            {
                var str = masterHelp.ITCD_help(val, Code);
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
        public ActionResult GetBrandDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.BRANDCD_help(val));
                }
                else
                {
                    string str = masterHelp.BRANDCD_help(val);
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
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                ItemMasterEntry VE = new ItemMasterEntry();
                Cn.getQueryString(VE);
                string str = masterHelp.ITGRPCD_help(val, VE.MENU_PARA);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    string prodgrpcd = "", prodgrpnm = "";
                    prodgrpcd = str.retCompValue("PRODGRPCD");
                    prodgrpnm = DB.M_PRODGRP.Where(a => a.PRODGRPCD == prodgrpcd).Select(b => b.PRODGRPNM).FirstOrDefault();
                    str += "^PRODGRPNM=^" + prodgrpnm + Cn.GCS();
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetSubBrandDetails(string val, string Code)
        {
            try
            {
                if (Code.retStr() == "")
                {
                    return Content("Please Selet Brand Code !!");
                }
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.SBRAND(val, Code));
                }
                else
                {
                    string str = masterHelp.SBRAND(val, Code);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetColorDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.COLOR(val));
                }
                else
                {
                    string str = masterHelp.COLOR(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetPartDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.PARTS(val));
                }
                else
                {
                    string str = masterHelp.PARTS(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetCollectionDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.COLLECTION(val));
                }
                else
                {
                    string str = masterHelp.COLLECTION(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }

        }
        public ActionResult GetSizeDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.SIZE(val));
                }
                else
                {
                    string str = masterHelp.SIZE(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetUOMDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.UOM_help(val));
                }
                else
                {
                    string str = masterHelp.UOM_help(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult SearchItemData(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            try
            {
                string scm1 = CommVar.CurSchema(UNQSNO);
                string valsrch = val.ToUpper().Trim();
                string sql = "";

                sql += "select a.itcd,a.itnm,a.STYLENO,a.ITGRPCD,b.ITGRPNM,a.FABITCD,a.BRANDCD,c.BRANDNM,a.SBRANDCD,d.SBRANDNM ";
                sql += "from " + scm1 + ".m_sitem a, " + scm1 + ".m_group b, " + scm1 + ".m_brand c, " + scm1 + ".m_subbrand d ";
                sql += "where a.itgrpcd=b.itgrpcd(+) and a.brandcd=c.brandcd(+) and a.sbrandcd=d.sbrandcd(+) and b.itgrptype<>'C' ";
                if (valsrch.retStr() != "")
                {
                    sql += "and ( upper(a.itcd) like '%" + valsrch + "%' or upper(a.itnm) like '%" + valsrch + "%' ";
                    sql += "or upper(a.styleno) like '%" + valsrch + "%' or upper(a.ITGRPCD) like '%" + valsrch + "%' or upper(b.ITGRPNM) like '%" + valsrch + "%'   ";
                    sql += "or upper(a.BRANDCD) like '%" + valsrch + "%' or upper(c.BRANDNM) like '%" + valsrch + "%'   ";
                    sql += "or upper(a.SBRANDCD) like '%" + valsrch + "%' or upper(d.SBRANDNM) like '%" + valsrch + "%'  )  ";
                }
                sql += "order by a.itcd ";

                DataTable rsTmp = masterHelp.SQLquery(sql);

                if (val.retStr() == "" || rsTmp.Rows.Count > 1)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= rsTmp.Rows.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + rsTmp.Rows[i]["styleno"] + "</td><td>" + rsTmp.Rows[i]["itnm"] + "</td><td>" + rsTmp.Rows[i]["itcd"] + "</td><td>" + rsTmp.Rows[i]["ITGRPNM"] + " [ " + rsTmp.Rows[i]["ITGRPCD"] + " ] " + "</td><td>" + rsTmp.Rows[i]["FABITCD"] + "</td><td>" + rsTmp.Rows[i]["BRANDNM"] + " [ " + rsTmp.Rows[i]["BRANDCD"] + " ] " + "</td><td>" + rsTmp.Rows[i]["SBRANDNM"] + " [ " + rsTmp.Rows[i]["SBRANDCD"] + " ] " + "</td></tr>");
                    }
                    var hdr = "Design No." + Cn.GCS() + "Item Name" + Cn.GCS() + "Item Code" + Cn.GCS() + "Item Group Code" + Cn.GCS() + "Fabric Code" + Cn.GCS() + "Brand Code" + Cn.GCS() + "Sub Brand Code";
                    return PartialView("_Help2", masterHelp.Generate_help(hdr, SB.ToString()));
                }
                else
                {
                    string str = "";
                    if (rsTmp.Rows.Count > 0)
                    {
                        str = masterHelp.ToReturnFieldValues("", rsTmp);
                    }
                    else
                    {
                        str = "Invalid Item Code !!";
                    }
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetSubLedgerDetails(string val, string Code)
        {
            try
            {
                var str = Master_HelpFa.SLCD_help(val, Code);
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
        public ActionResult AddDOCRow(ItemMasterEntry VE)
        {
            try
            {
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), "IMPROVAR");
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
        public ActionResult DeleteDOCRow(ItemMasterEntry VE)
        {
            try
            {
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), "IMPROVAR");
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
        public ActionResult AddRowSIZE(ItemMasterEntry VE, int COUNT, string TAG)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                if (VE.MSITEMSLCD == null)
                {
                    List<MSITEMSLCD> MSITEMSLCD_HEAD = new List<MSITEMSLCD>();
                    if (COUNT > 0 && TAG == "Y")
                    {
                        Int16 SERIAL = 0;
                        for (int j = 0; j <= COUNT - 1; j++)
                        {
                            SERIAL++;
                            MSITEMSLCD DTL = new MSITEMSLCD();
                            DTL.SRLNO = SERIAL.ToString();
                            //DTL.DISC_TYPE = Master_Help.DISC_TYPE();
                            //DTL.STD_DIS_TYPE = Master_Help.STD_DISC_TYPE();
                            //DTL.DAMSTOCK_list = Master_Help.DAMSTOCK_list();
                            //DTL.CONVDESC_list = new List<DropDown_list1>();
                            MSITEMSLCD_HEAD.Add(DTL);
                            VE.MSITEMSLCD = MSITEMSLCD_HEAD;

                        }
                    }
                    else
                    {
                        MSITEMSLCD DTL = new MSITEMSLCD();
                        DTL.SRLNO = 1.ToString();
                        //DTL.DISC_TYPE = Master_Help.DISC_TYPE();
                        //DTL.STD_DIS_TYPE = Master_Help.STD_DISC_TYPE();
                        //DTL.DAMSTOCK_list = Master_Help.DAMSTOCK_list();
                        //DTL.CONVDESC_list = new List<DropDown_list1>();
                        MSITEMSLCD_HEAD.Add(DTL);
                        VE.MSITEMSLCD = MSITEMSLCD_HEAD;
                    }
                }
                else
                {
                    List<MSITEMSLCD> MSITEMSLCD_HEAD = new List<MSITEMSLCD>();
                    for (int i = 0; i <= VE.MSITEMSLCD.Count - 1; i++)
                    {
                        MSITEMSLCD MIB = new MSITEMSLCD();
                        MIB = VE.MSITEMSLCD[i];
                        //MIB.DISC_TYPE = Master_Help.DISC_TYPE();
                        //MIB.STD_DIS_TYPE = Master_Help.STD_DISC_TYPE();
                        //MIB.DAMSTOCK_list = Master_Help.DAMSTOCK_list();
                        //MIB.CONVDESC_list = new List<DropDown_list1>();
                        MSITEMSLCD_HEAD.Add(MIB);
                    }
                    if (COUNT > 0 && TAG == "Y")
                    {
                        Int16 SERIAL = Convert.ToInt16(VE.MSITEMSLCD.Max(a => Convert.ToInt32(a.SRLNO)));
                        for (int j = 0; j <= COUNT - 1; j++)
                        {
                            SERIAL++;
                            MSITEMSLCD MIB = new MSITEMSLCD();
                            MIB.SRLNO = SERIAL.ToString();
                            //MIB.DISC_TYPE = Master_Help.DISC_TYPE();
                            //MIB.STD_DIS_TYPE = Master_Help.STD_DISC_TYPE();
                            //MIB.DAMSTOCK_list = Master_Help.DAMSTOCK_list();
                            //MIB.CONVDESC_list = new List<DropDown_list1>();
                            MSITEMSLCD_HEAD.Add(MIB);
                        }
                    }
                    else
                    {
                        MSITEMSLCD MIB = new MSITEMSLCD();
                        MIB.SRLNO = Convert.ToString(Convert.ToByte(VE.MSITEMSLCD.Max(a => Convert.ToInt32(a.SRLNO))) + 1);
                        //MIB.DISC_TYPE = Master_Help.DISC_TYPE();
                        //MIB.STD_DIS_TYPE = Master_Help.STD_DISC_TYPE();
                        //MIB.DAMSTOCK_list = Master_Help.DAMSTOCK_list();
                        //MIB.CONVDESC_list = new List<DropDown_list1>();
                        MSITEMSLCD_HEAD.Add(MIB);
                    }
                    VE.MSITEMSLCD = MSITEMSLCD_HEAD;
                }
                VE.DefaultView = true;
                return PartialView("_M_FinProduct_SLCD", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult DeleteRowSIZE(ItemMasterEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                List<MSITEMSLCD> ITEMSIZE = new List<MSITEMSLCD>();
                int count = 0;
                for (int i = 0; i <= VE.MSITEMSLCD.Count - 1; i++)
                {
                    if (VE.MSITEMSLCD[i].Checked == false)
                    {
                        count += 1;
                        MSITEMSLCD item = new MSITEMSLCD();
                        item = VE.MSITEMSLCD[i];
                        item.SRLNO = count.ToString();
                        ITEMSIZE.Add(item);
                    }

                }
                VE.MSITEMSLCD = ITEMSIZE;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_M_FinProduct_SLCD", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddRowPARTS(ItemMasterEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

                List<MSITEMPARTS> ITEMPARTS = new List<MSITEMPARTS>();
                if (VE.MSITEMPARTS == null)
                {
                    List<MSITEMPARTS> SCMITMDTL1 = new List<MSITEMPARTS>();
                    MSITEMPARTS SCMIT = new MSITEMPARTS();
                    SCMIT.SLNO = 1;
                    SCMITMDTL1.Add(SCMIT);
                    VE.MSITEMPARTS = SCMITMDTL1;
                }
                else {
                    for (int i = 0; i <= VE.MSITEMPARTS.Count - 1; i++)
                    {
                        MSITEMPARTS MIP = new MSITEMPARTS();
                        MIP = VE.MSITEMPARTS[i];
                        ITEMPARTS.Add(MIP);
                    }
                    MSITEMPARTS MIP1 = new MSITEMPARTS();
                    var max = VE.MSITEMPARTS.Max(a => Convert.ToInt32(a.SLNO));
                    int SRLNO = Convert.ToInt32(max) + 1;
                    MIP1.SLNO = Convert.ToByte(SRLNO);
                    ITEMPARTS.Add(MIP1);
                    VE.MSITEMPARTS = ITEMPARTS;
                }
                VE.DefaultView = true;
                return PartialView("_M_FinProduct_PART", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult DeleteRowPARTS(ItemMasterEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                List<MSITEMPARTS> ITEMPARTS = new List<MSITEMPARTS>();
                int count = 0;
                for (int i = 0; i <= VE.MSITEMPARTS.Count - 1; i++)
                {
                    if (VE.MSITEMPARTS[i].Checked == false)
                    {
                        count += 1;
                        MSITEMPARTS item = new MSITEMPARTS();
                        item = VE.MSITEMPARTS[i];
                        item.SLNO = Convert.ToByte(count);
                        ITEMPARTS.Add(item);
                    }
                }
                VE.MSITEMPARTS = ITEMPARTS;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_M_FinProduct_PART", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddRowINVCD(ItemMasterEntry VE, int COUNT, string TAG)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            Cn.getQueryString(VE);
            //ViewBag.formname = formnamebydoccd(VE.DOC_CODE);

            //List<DebitCreditType> DCT = new List<DebitCreditType>();
            //DebitCreditType DCT1 = new DebitCreditType(); DCT1.text = "DR"; DCT1.value = "D"; DCT.Add(DCT1);
            //DebitCreditType DCT2 = new DebitCreditType(); DCT2.text = "CR"; DCT2.value = "C"; DCT.Add(DCT2); VE.DebitCreditType = DCT;
            //VE.Database_Combo2 = (from i in DB.T_VCH_DET select new Database_Combo2() { FIELD_VALUE = i.BANK_NAME }).DistinctBy(a => a.FIELD_VALUE).ToList();
            //VE.Database_Combo3 = (from i in DB.T_VCH_DET select new Database_Combo3() { FIELD_VALUE = i.T_REM }).DistinctBy(a => a.FIELD_VALUE).ToList();
            //VE.DropDown_list_TDS = INT_TDS();
            if (VE.MSITEMBARCODE == null)
            {
                List<MSITEMBARCODE> SCMITMDTL1 = new List<MSITEMBARCODE>();
                if (COUNT > 0 && TAG == "Y")
                {
                    int SERIAL = 0;
                    for (int j = 0; j <= COUNT - 1; j++)
                    {
                        SERIAL = SERIAL + 1;
                        MSITEMBARCODE MBILLDET = new MSITEMBARCODE();
                        MBILLDET.SRLNO = Convert.ToByte(SERIAL);
                        SCMITMDTL1.Add(MBILLDET);
                    }
                }
                else
                {
                    MSITEMBARCODE MBILLDET = new MSITEMBARCODE();
                    MBILLDET.SRLNO = 1;
                    SCMITMDTL1.Add(MBILLDET);
                }
                VE.MSITEMBARCODE = SCMITMDTL1;
            }
            else
            {
                List<MSITEMBARCODE> MSITEMBARCODE = new List<MSITEMBARCODE>();
                for (int i = 0; i <= VE.MSITEMBARCODE.Count - 1; i++)
                {
                    MSITEMBARCODE MBILLDET = new MSITEMBARCODE();
                    MBILLDET = VE.MSITEMBARCODE[i];
                    MSITEMBARCODE.Add(MBILLDET);
                }
                MSITEMBARCODE MBILLDET1 = new MSITEMBARCODE();
                if (COUNT > 0 && TAG == "Y")
                {
                    int SERIAL = Convert.ToInt32(VE.MSITEMBARCODE.Max(a => Convert.ToInt32(a.SRLNO)));
                    for (int j = 0; j <= COUNT - 1; j++)
                    {
                        SERIAL = SERIAL + 1;
                        MSITEMBARCODE OPENING_BL = new MSITEMBARCODE();
                        OPENING_BL.SRLNO = Convert.ToByte(SERIAL);
                        MSITEMBARCODE.Add(OPENING_BL);
                    }
                }
                else
                {
                    MBILLDET1.SRLNO = Convert.ToByte(Convert.ToByte(VE.MSITEMBARCODE.Max(a => Convert.ToInt32(a.SRLNO))) + 1);
                    MSITEMBARCODE.Add(MBILLDET1);
                }
                VE.MSITEMBARCODE = MSITEMBARCODE;
            }
            //VE.MSITEMBARCODE.ForEach(a => a.DRCRTA = masterHelp.DR_CR().OrderByDescending(s => s.text).ToList());
            VE.DefaultView = true;
            return PartialView("_M_FinProduct_BARCD", VE);
        }
        public ActionResult DeleteRowINVCD(ItemMasterEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                List<MSITEMBARCODE> SITEMBARCODE = new List<MSITEMBARCODE>();
                int count = 0;
                for (int i = 0; i <= VE.MSITEMBARCODE.Count - 1; i++)
                {
                    if (VE.MSITEMBARCODE[i].Checked == false)
                    {
                        count += 1;
                        MSITEMBARCODE item = new MSITEMBARCODE();
                        item = VE.MSITEMBARCODE[i];
                        item.SRLNO = Convert.ToByte(count);
                        SITEMBARCODE.Add(item);
                    }
                }
                VE.MSITEMBARCODE = SITEMBARCODE;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_M_FinProduct_BARCD", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult AddRowMEASURE(ItemMasterEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                VE.Database_Combo2 = (from n in DB.M_SITEM_MEASURE
                                      select new Database_Combo2() { FIELD_VALUE = n.MDESC }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                if (VE.MSITEMMEASURE == null)
                {
                    List<MSITEMMEASURE> ITEMMEASURE1 = new List<MSITEMMEASURE>();
                    MSITEMMEASURE MIM = new MSITEMMEASURE();
                    MIM.SLNO = 1;
                    ITEMMEASURE1.Add(MIM);
                    VE.MSITEMMEASURE = ITEMMEASURE1;
                }
                else
                {
                    List<MSITEMMEASURE> ITEMMEASURE = new List<MSITEMMEASURE>();
                    for (int i = 0; i <= VE.MSITEMMEASURE.Count - 1; i++)
                    {
                        MSITEMMEASURE MIM = new MSITEMMEASURE();
                        MIM = VE.MSITEMMEASURE[i];
                        ITEMMEASURE.Add(MIM);
                    }
                    MSITEMMEASURE MIM1 = new MSITEMMEASURE();
                    var max = VE.MSITEMMEASURE.Max(a => Convert.ToInt32(a.SLNO));
                    int SLNO = Convert.ToInt32(max) + 1;
                    MIM1.SLNO = Convert.ToByte(SLNO);
                    ITEMMEASURE.Add(MIM1);
                    VE.MSITEMMEASURE = ITEMMEASURE;

                }

                VE.DefaultView = true;
                return PartialView("_M_FinProduct_Measurement", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult DeleteRowMEASURE(ItemMasterEntry VE)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                VE.Database_Combo2 = (from n in DB.M_SITEM_MEASURE
                                      select new Database_Combo2() { FIELD_VALUE = n.MDESC }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();


                List<MSITEMMEASURE> ITEMMEASURE = new List<MSITEMMEASURE>();
                int count = 0;
                for (int i = 0; i <= VE.MSITEMMEASURE.Count - 1; i++)
                {
                    if (VE.MSITEMMEASURE[i].Checked == false)
                    {
                        count += 1;
                        MSITEMMEASURE MEASURE = new MSITEMMEASURE();
                        MEASURE = VE.MSITEMMEASURE[i];
                        MEASURE.SLNO = Convert.ToByte(count);
                        ITEMMEASURE.Add(MEASURE);
                    }

                }
                VE.MSITEMMEASURE = ITEMMEASURE;
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_M_FinProduct_Measurement", VE);
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        private string getmasterbarno(string ITCD, string curschema = "")
        {
            string scm = CommVar.CurSchema(UNQSNO);
            if (curschema != "") scm = curschema;
            var sql = "SELECT LISTAGG(barno,',') WITHIN GROUP (ORDER BY barno) AS barno FROM " + scm + ".T_BATCHmst where itcd='" + ITCD + "' and COMMONUNIQBAR='C' ";
            DataTable dt = masterHelp.SQLquery(sql);
            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0]["barno"].retStr();
            }
            else
            {
                return "";
            }
        }

        public Tuple<List<T_BATCH_IMG_HDR>> SaveBarImage(string BarImage, string BARNO, short EMD)
        {
            List<T_BATCH_IMG_HDR> doc = new List<T_BATCH_IMG_HDR>();
            int slno = 0;
            try
            {
                var BarImages = BarImage.retStr().Trim(Convert.ToChar(Cn.GCS())).Split(Convert.ToChar(Cn.GCS()));
                foreach (string image in BarImages)
                {
                    if (image != "")
                    {
                        var imagedes = image.Split('~');
                        T_BATCH_IMG_HDR mdoc = new T_BATCH_IMG_HDR();
                        mdoc.CLCD = CommVar.ClientCode(UNQSNO);
                        mdoc.EMD_NO = EMD;
                        mdoc.SLNO = Convert.ToByte(++slno);
                        mdoc.DOC_CTG = "PRODUCT";
                        var extension = Path.GetExtension(imagedes[0]);
                        mdoc.DOC_FLNAME = BARNO + "_" + slno + extension;
                        mdoc.DOC_DESC = imagedes[1].retStr().Replace('~', ' ');
                        mdoc.BARNO = BARNO;
                        mdoc.DOC_EXTN = extension;
                        doc.Add(mdoc);
                        string tmpdir = CommVar.SaveFolderPath() + "/ItemImages/";
                        if (!Directory.Exists(tmpdir)) Directory.CreateDirectory(tmpdir);
                        string topath = tmpdir + mdoc.DOC_FLNAME;
                        topath = Path.Combine(topath, "");
                        var addarr = imagedes[0].Split('/');
                        var tempimgName = (addarr[addarr.Length - 1]);
                        string frompath = CommVar.LocalUploadDocPath(tempimgName);
                        Cn.CopyImage(frompath, topath);
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, BarImage);
            }
            var result = Tuple.Create(doc);
            return result;
        }
        public ActionResult UploadImages(string ImageStr, string ImageName, string ImageDesc)
        {
            try
            {
                var extension = Path.GetExtension(ImageName);
                string filename = "I".retRepname() + extension;
                var folderpath = CommVar.LocalUploadDocPath(filename);
                var link = Cn.SaveImage(ImageStr, folderpath);
                var path = CommVar.WebUploadDocURL(filename);
                return Content(path);
            }
            catch (Exception ex)
            {
                return Content("//.");
            }

        }
        public ActionResult SAVE(FormCollection FC, ItemMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        var itgrptype = "";
                        if (VE.MENU_PARA == "F")
                        {
                            itgrptype = "F";
                        }
                        else if (VE.MENU_PARA == "C")
                        {
                            itgrptype = "C";
                        }

                        //checking design no
                        var query = (from c in DB.M_SITEM
                                     join o in DB.M_GROUP on c.ITGRPCD equals (o.ITGRPCD)
                                     where (c.STYLENO == VE.M_SITEM.STYLENO && c.M_AUTONO != VE.M_SITEM.M_AUTONO && o.ITGRPTYPE == itgrptype)
                                     select c);
                        if (query.Any())
                        {
                            return Content("Design Number Already Exists : Please Enter a Different Design Number !!");
                        }
                        //checking bar code grid contain distinct value
                        if (VE.MSITEMBARCODE != null)
                        {
                            var all_barcode = VE.MSITEMBARCODE.Where(b => b.SIZECD != null && b.COLRCD != null).Select(a => a.SZBARCODE + a.COLRCD).ToList();
                            var barcode = VE.MSITEMBARCODE.Where(b => b.SIZECD != null && b.COLRCD != null).Select(a => a.SZBARCODE + a.COLRCD).Distinct().ToList();
                            if (all_barcode.Count() != barcode.Count())
                            {
                                return Content("SIZE CODE and COLOR CODE duplicate in barcode grid.");
                            }
                        }
                        //
                        M_SITEM MSITEM = new M_SITEM();
                        MSITEM.CLCD = CommVar.ClientCode(UNQSNO);

                        bool dataexist = false;
                        if (VE.DefaultAction == "A")
                        {
                            //var STYLENO = DB.M_SITEM.Where(a => a.STYLENO == VE.M_SITEM.STYLENO);
                            var STYLENO = (from x in DB.M_SITEM
                                           join o in DB.M_GROUP on x.ITGRPCD equals (o.ITGRPCD)
                                           where x.STYLENO == VE.M_SITEM.STYLENO && o.ITGRPTYPE == itgrptype
                                           select x).ToList();
                            if (STYLENO.Any()) dataexist = true;
                        }
                        else
                        {
                            var STYLENO = (from x in DB.M_SITEM
                                           join o in DB.M_GROUP on x.ITGRPCD equals (o.ITGRPCD)
                                           where x.STYLENO == VE.M_SITEM.STYLENO && x.ITCD != VE.M_SITEM.ITCD && o.ITGRPTYPE == itgrptype
                                           select x).ToList();
                            if (STYLENO.Any()) dataexist = true;
                        }
                        if (dataexist == true)
                        {
                            transaction.Rollback();
                            return Content("This Design Already exist");
                        }
                        if (VE.DefaultAction == "A")
                        {
                            MSITEM.EMD_NO = 0;
                            MSITEM.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());

                            //auto code with first leter

                            string txtITGRPCD = VE.M_SITEM.ITGRPCD;
                            var txtst = (from p in DB.M_GROUP where p.ITGRPCD == txtITGRPCD select p.ITGRPTYPE.Substring(0, 1)).FirstOrDefault();
                            var txtst3 = (from p in DB.M_GROUP where p.ITGRPCD == txtITGRPCD select p.GRPBARCODE).FirstOrDefault();

                            string sql = "select max(itcd)itcd from " + CommVar.CurSchema(UNQSNO) + ".m_sitem where itcd like('" + txtst + txtst3 + "%') ";
                            var tbl = masterHelp.SQLquery(sql);
                            if (tbl.Rows[0]["itcd"].ToString() == "")
                            {
                                string R = txtst + txtst3 + "00001";
                                MSITEM.ITCD = R.ToString();
                            }

                            //    if (MAXJOBCD == null)
                            //{
                            //    string txt = txtst;
                            //    string stxt = txt.Substring(0, 1);
                            //    string R = stxt + "0000001";
                            //    MSITEM.ITCD = R.ToString();
                            //}
                            else
                            {
                                //string maxSLst = MAXJOBCD.Substring(0, 1);
                                string maxSLst = tbl.Rows[0]["itcd"].ToString().Substring(0, 1);
                                if (maxSLst == txtst)
                                {
                                    string s = tbl.Rows[0]["itcd"].ToString();
                                    string digits = new string(s.Where(char.IsDigit).ToArray());
                                    string letters = new string(s.Where(char.IsLetter).ToArray());
                                    int number;
                                    if (!int.TryParse(digits, out number))                   //int.Parse would do the job since only digits are selected
                                    {
                                        Console.WriteLine("Something weired happened");
                                    }
                                    string newStr = letters + (++number).ToString("D7");
                                    MSITEM.ITCD = newStr.ToString();
                                }
                                else
                                {
                                    string R = txtst + txtst3 + "00001";
                                    MSITEM.ITCD = R.ToString();
                                }
                            }
                        }

                        MSITEM.ITGRPCD = VE.M_SITEM.ITGRPCD;
                        MSITEM.ITNM = VE.M_SITEM.ITNM;
                        MSITEM.BRANDCD = VE.M_SITEM.BRANDCD;
                        MSITEM.SBRANDCD = VE.M_SITEM.SBRANDCD;
                        //MSITEM.QUALITY = VE.M_SITEM.QUALITY;
                        MSITEM.STYLENO = VE.M_SITEM.STYLENO.retStr().Trim();
                        MSITEM.FABITCD = VE.M_SITEM.FABITCD;
                        //MSITEM.MIXSIZE = VE.M_SITEM.MIXSIZE;
                        //string altrStyle = "";
                        //if (VE.M_SITEM.STYLENODISP == null)
                        //{
                        //    altrStyle = VE.M_SITEM.STYLENO;
                        //}
                        //else
                        //{
                        //    altrStyle = VE.M_SITEM.STYLENODISP;
                        //}
                        //MSITEM.STYLENODISP = VE.M_SITEM.STYLENODISP;
                        MSITEM.UOMCD = VE.M_SITEM.UOMCD;
                        MSITEM.HSNCODE = VE.M_SITEM.HSNCODE;
                        MSITEM.COLRCD = VE.M_SITEM.COLRCD;
                        //MSITEM.PRODUCTTYPE = FC["protyp"].ToString();
                        MSITEM.GENDER = VE.M_SITEM.GENDER;
                        MSITEM.COLLCD = VE.M_SITEM.COLLCD;
                        //MSITEM.PCSPERBOX = VE.M_SITEM.PCSPERBOX;
                        MSITEM.PCSPERSET = VE.M_SITEM.PCSPERSET;
                        MSITEM.COLRPERSET = VE.M_SITEM.COLRPERSET;
                        if (VE.MENU_PARA == "C" || VE.MENU_PARA == "A")
                        {
                            MSITEM.FEATURE = VE.M_SITEM.FEATURE;
                            MSITEM.DMNSN = VE.M_SITEM.DMNSN;
                        }
                        MSITEM.NEGSTOCK = VE.NEGSTOCK == true ? "Y" : "";
                        MSITEM.PRODGRPCD = VE.M_SITEM.PRODGRPCD;
                        MSITEM.SAPCODE = VE.M_SITEM.SAPCODE;
                        MSITEM.CONVQTYPUNIT = VE.M_SITEM.CONVQTYPUNIT;
                        MSITEM.CONVUOMCD = VE.M_SITEM.CONVUOMCD;
                        //MSITEM.STD_RATE = VE.M_SITEM.STD_RATE;
                        //MSITEM.SAMPPC = VE.M_SITEM.SAMPPC;
                        //MSITEM.STDLOTQTY = VE.M_SITEM.SAMPPC;
                        //MSITEM.STDLOTCOMPLTDAYS = VE.M_SITEM.STDLOTCOMPLTDAYS;
                        //MSITEM.STDBATCHQTY = VE.M_SITEM.STDBATCHQTY;
                        //MSITEM.MERGEPCS = VE.M_SITEM.MERGEPCS;
                        MSITEM.FAVITEM = VE.FAVITEMChecked == true ? "Y" : "N";
                        MSITEM.FAVCOLR = VE.M_SITEM.FAVCOLR;
                        MSITEM.LINKITCD = VE.M_SITEM.LINKITCD;
                        if (VE.DefaultAction == "E")
                        {
                            MSITEM.DTAG = "E";
                            MSITEM.M_AUTONO = VE.M_SITEM.M_AUTONO;
                            MSITEM.ITCD = VE.M_SITEM.ITCD;

                            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                            string sbarno = getmasterbarno(VE.M_SITEM.ITCD); var arrbarno = sbarno.Split(',');
                            var comp = DB1.T_BATCHMST.Where(x => arrbarno.Contains(x.BARNO)).OrderBy(s => s.BARNO).ToList();
                            foreach (var v in comp)
                            {
                                if (!VE.MSITEMBARCODE.Where(s => s.BARNO == v.BARNO).Any())
                                {
                                    DB.T_BATCHMST.Where(x => x.BARNO == v.BARNO).ToList().ForEach(x => { x.DTAG = "E"; });
                                    DB.T_BATCHMST.RemoveRange(DB.T_BATCHMST.Where(x => x.BARNO == v.BARNO));
                                    DB.SaveChanges();

                                    //DB.T_BATCHmst.Where(x => x.BARNO == v.BARNO).ToList().ForEach(x => { x.DTAG = "E"; });
                                    //DB.T_BATCHmst.RemoveRange(DB.T_BATCHmst.Where(x => x.BARNO == v.BARNO));
                                    //DB.SaveChanges();
                                }
                            }


                            DB.M_SITEM_SLCD.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.SaveChanges();
                            DB.M_SITEM_SLCD.RemoveRange(DB.M_SITEM_SLCD.Where(x => x.ITCD == VE.M_SITEM.ITCD));

                            //DB.M_SITEM_BOX.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "E"; });
                            //DB.M_SITEM_BOX.RemoveRange(DB.M_SITEM_BOX.Where(x => x.ITCD == VE.M_SITEM.ITCD));

                            //DB.M_SITEM_COLOR.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "E"; });
                            //DB.M_SITEM_COLOR.RemoveRange(DB.M_SITEM_COLOR.Where(x => x.ITCD == VE.M_SITEM.ITCD));

                            DB.M_SITEM_PARTS.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_SITEM_PARTS.RemoveRange(DB.M_SITEM_PARTS.Where(x => x.ITCD == VE.M_SITEM.ITCD));

                            DB.M_SITEM_MEASURE.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_SITEM_MEASURE.RemoveRange(DB.M_SITEM_MEASURE.Where(x => x.ITCD == VE.M_SITEM.ITCD));

                            if (VE.PRICES_EFFDT.retStr() != "")
                            {
                                DateTime PRICES_EFFDT = Convert.ToDateTime(VE.PRICES_EFFDT);
                                DB.T_BATCHMST_PRICE.Where(x => x.EFFDT == PRICES_EFFDT && arrbarno.Contains(x.BARNO)).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.T_BATCHMST_PRICE.RemoveRange(DB.T_BATCHMST_PRICE.Where(x => x.EFFDT == PRICES_EFFDT && arrbarno.Contains(x.BARNO)));

                                //DB.T_BATCHMST_PRICE.Where(x => x.EFFDT == PRICES_EFFDT && arrbarno.Contains(x.BARNO)).ToList().ForEach(x => { x.DTAG = "E"; });
                                //DB.T_BATCHMST_PRICE.RemoveRange(DB.T_BATCHMST_PRICE.Where(x => x.EFFDT == PRICES_EFFDT && arrbarno.Contains(x.BARNO)));
                            }
                            DB.T_BATCH_IMG_HDR_LINK.Where(x => arrbarno.Contains(x.BARNO)).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.T_BATCH_IMG_HDR_LINK.RemoveRange(DB.T_BATCH_IMG_HDR_LINK.Where(x => arrbarno.Contains(x.BARNO)));
                            DB.SaveChanges();

                            DB.T_BATCH_IMG_HDR.Where(x => arrbarno.Contains(x.BARNO)).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.T_BATCH_IMG_HDR.RemoveRange(DB.T_BATCH_IMG_HDR.Where(x => arrbarno.Contains(x.BARNO)));
                            DB.SaveChanges();

                            DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO));

                            DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO));

                            var MAXEMDNO = (from p in DB.M_SITEM where p.M_AUTONO == VE.M_SITEM.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MSITEM.EMD_NO = 0;
                            }
                            else
                            {
                                MSITEM.EMD_NO = Convert.ToInt16(MAXEMDNO + 1);
                            }

                        }
                        for (int i = 0; i <= VE.MSITEMSLCD.Count - 1; i++)
                        {
                            if (VE.MSITEMSLCD[i].SRLNO != null && VE.MSITEMSLCD[i].SLCD != null)
                            {
                                M_SITEM_SLCD MIS = new M_SITEM_SLCD();
                                MIS.CLCD = MSITEM.CLCD;
                                MIS.EMD_NO = MSITEM.EMD_NO;
                                MIS.DTAG = MSITEM.DTAG;
                                MIS.ITCD = MSITEM.ITCD;
                                MIS.SLCD = VE.MSITEMSLCD[i].SLCD;
                                MIS.JOBRT = VE.MSITEMSLCD[i].JOBRT;
                                MIS.PDESIGN = VE.MSITEMSLCD[i].PDESIGN;

                                //MIS.SIZECD = VE.MSITEMSLCD[i].SIZECD;
                                //if (VE.MSITEMSLCD[i].IChecked == true)
                                //{
                                //    MIS.INACTIVE_TAG = "Y";
                                //}
                                //else
                                //{
                                //    MIS.INACTIVE_TAG = "N";
                                //}
                                DB.M_SITEM_SLCD.Add(MIS);
                            }
                        }

                        for (int i = 0; i <= VE.MSITEMPARTS.Count - 1; i++)
                        {
                            if (VE.MSITEMPARTS[i].SLNO != null && VE.MSITEMPARTS[i].PARTCD != null)
                            {
                                M_SITEM_PARTS MIP = new M_SITEM_PARTS();
                                MIP.CLCD = MSITEM.CLCD;
                                MIP.EMD_NO = MSITEM.EMD_NO;
                                MIP.DTAG = MSITEM.DTAG;
                                MIP.ITCD = MSITEM.ITCD;
                                MIP.PARTCD = VE.MSITEMPARTS[i].PARTCD;
                                DB.M_SITEM_PARTS.Add(MIP);
                            }
                        }
                        //T_BATCHmst MSITEMBARCODE = new T_BATCHmst();
                        //MSITEMBARCODE.CLCD = MSITEM.CLCD;
                        //MSITEMBARCODE.EMD_NO = MSITEM.EMD_NO;
                        //MSITEMBARCODE.ITCD = MSITEM.ITCD;
                        //string txtitgrpcd = VE.M_SITEM.ITGRPCD;
                        //if (VE.ITEM_BARCODE.retStr() != "")
                        //{
                        //    MSITEMBARCODE.BARNO = VE.ITEM_BARCODE;
                        //}
                        //else
                        //{
                        //    MSITEMBARCODE.BARNO = salesfunc.GenerateBARNO(MSITEM.ITCD);
                        //}
                        //DB.T_BATCHmst.Add(MSITEMBARCODE);
                        List<string> barnos = new List<string>(); string MAINBARNO = "";
                        string sql1 = "";
                        DataTable recoexist = new DataTable();
                        for (int i = 0; i <= VE.MSITEMBARCODE.Count - 1; i++)
                        {
                            if (VE.MSITEMBARCODE[i].SIZECD != null || VE.MSITEMBARCODE[i].COLRCD != null || i == 0)
                            {

                                string barno = "";
                                if (VE.MSITEMBARCODE[i].BARNO.retStr() != "")
                                {
                                    barno = VE.MSITEMBARCODE[i].BARNO.retStr();
                                }
                                else
                                {
                                    barno = salesfunc.GenerateBARNO(MSITEM.ITCD, VE.MSITEMBARCODE[i].CLRBARCODE.retStr(), VE.MSITEMBARCODE[i].SZBARCODE);
                                }
                                barnos.Add(barno);
                                if (i == 0) MAINBARNO = barno;
                                sql1 = "Select * from " + CommVar.CurSchema(UNQSNO) + ".t_batchmst where barno='" + barno + "'";
                                recoexist = masterHelp.SQLquery(sql1);
                                if (recoexist.Rows.Count == 0)
                                {
                                    //T_BATCHmst MSITEMBARCODE1 = new T_BATCHmst();
                                    //MSITEMBARCODE1.EMD_NO = MSITEM.EMD_NO;
                                    //MSITEMBARCODE1.CLCD = MSITEM.CLCD;
                                    //MSITEMBARCODE1.ITCD = MSITEM.ITCD;
                                    //MSITEMBARCODE1.SIZECD = VE.MSITEMBARCODE[i].SIZECD;
                                    //MSITEMBARCODE1.COLRCD = VE.MSITEMBARCODE[i].COLRCD;
                                    //MSITEMBARCODE1.BARNO = barno.retStr();

                                    //DB.T_BATCHmst.Add(MSITEMBARCODE1);
                                    //if (i == 0) BARNO = MSITEMBARCODE1.BARNO;

                                    T_BATCHMST TBATCHMST = new T_BATCHMST();
                                    TBATCHMST.EMD_NO = MSITEM.EMD_NO;
                                    TBATCHMST.DTAG = MSITEM.DTAG;
                                    TBATCHMST.CLCD = MSITEM.CLCD;
                                    TBATCHMST.DTAG = MSITEM.DTAG;
                                    TBATCHMST.TTAG = MSITEM.TTAG;
                                    TBATCHMST.BARNO = barno.retStr();
                                    TBATCHMST.ITCD = MSITEM.ITCD;
                                    TBATCHMST.FABITCD = MSITEM.FABITCD;
                                    TBATCHMST.SIZECD = VE.MSITEMBARCODE[i].SIZECD;
                                    TBATCHMST.COLRCD = VE.MSITEMBARCODE[i].COLRCD;
                                    TBATCHMST.PDESIGN = VE.MSITEMBARCODE[i].PDESIGN;
                                    TBATCHMST.COMMONUNIQBAR = "C";

                                    DB.T_BATCHMST.Add(TBATCHMST);
                                }
                            }
                        }
                        for (int i = 0; i <= VE.MSITEMMEASURE.Count - 1; i++)
                        {
                            if (VE.MSITEMMEASURE[i].SLNO != 0 && VE.MSITEMMEASURE[i].MDESC != null)
                            {
                                M_SITEM_MEASURE MIM = new M_SITEM_MEASURE();
                                MIM.CLCD = MSITEM.CLCD;
                                MIM.EMD_NO = MSITEM.EMD_NO;
                                MIM.DTAG = MSITEM.DTAG;
                                MIM.ITCD = MSITEM.ITCD;
                                MIM.SLNO = VE.MSITEMMEASURE[i].SLNO;
                                MIM.MDESC = VE.MSITEMMEASURE[i].MDESC;
                                MIM.MVAL = VE.MSITEMMEASURE[i].MVAL;
                                MIM.REM = VE.MSITEMMEASURE[i].REM;

                                DB.M_SITEM_MEASURE.Add(MIM);
                            }
                        }

                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SITEM", MSITEM.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(), VE.Audit_REM);

                        if (VE.DefaultAction == "A")
                        {
                            DB.M_SITEM.Add(MSITEM);
                            DB.M_CNTRL_HDR.Add(MCH);
                        }

                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MSITEM).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        }
                        DB.SaveChanges();
                        if (VE.UploadDOC != null)
                        {
                            var img = Cn.SaveUploadImage("M_SITEM", VE.UploadDOC, MSITEM.M_AUTONO, MSITEM.EMD_NO.Value);
                            DB.M_CNTRL_HDR_DOC.AddRange(img.Item1);
                            DB.M_CNTRL_HDR_DOC_DTL.AddRange(img.Item2);
                            if (VE.BarImages.retStr() != "")
                            {
                                var barimg = SaveBarImage(VE.BarImages, MAINBARNO, MSITEM.EMD_NO.retShort());
                                DB.T_BATCH_IMG_HDR.AddRange(barimg.Item1);
                                DB.SaveChanges();
                                //var disntImgHdr = barimg.Item1.GroupBy(u => u.BARNO).Select(r => r.First()).ToList();
                                foreach (var imgbar in barnos)
                                {
                                    T_BATCH_IMG_HDR_LINK m_batchImglink = new T_BATCH_IMG_HDR_LINK();
                                    m_batchImglink.CLCD = MSITEM.CLCD;
                                    m_batchImglink.EMD_NO = MSITEM.EMD_NO;
                                    m_batchImglink.DTAG = MSITEM.DTAG;
                                    m_batchImglink.BARNO = imgbar;
                                    m_batchImglink.MAINBARNO = MAINBARNO;
                                    DB.T_BATCH_IMG_HDR_LINK.Add(m_batchImglink);
                                }
                            }
                        }
                        DB.SaveChanges();

                        #region Price list Save
                        DataTable DTPRICES = (DataTable)TempData["DTPRICES"]; TempData.Keep();
                        var prcRows = VE.STRPRICES.retStr().Split('~');
                        for (int i = 0; i <= prcRows.Length - 1; i++)
                        {
                            var prcCols = prcRows[i].Split(',');
                            for (int j = 8; j < prcCols.Length; j++)
                            {
                                string colorbarno = prcCols[3];// prcCols[2];
                                string sizebarno = prcCols[6];// prcCols[5];
                                string colorcd = prcCols[1];// prcCols[0];
                                string sizecd = prcCols[4];//prcCols[3];
                                string barno = prcCols[7];// prcCols[6];
                                var varcode = VE.MSITEMBARCODE.Where(d => d.SIZECD.retStr() == sizecd && d.COLRCD.retStr() == colorcd).FirstOrDefault();
                                if (varcode == null)
                                {
                                    transaction.Rollback();
                                    return Content("Color:" + colorcd + " Sizecd:" + sizecd + " not in barcode Tab. Please refresh pricelist. ");
                                }
                                var PRCCD = DTPRICES.Columns[j].ColumnName;
                                T_BATCHMST_PRICE MIP = new T_BATCHMST_PRICE();
                                MIP.EMD_NO = MSITEM.EMD_NO;
                                MIP.DTAG = MSITEM.DTAG;
                                MIP.CLCD = MSITEM.CLCD;
                                MIP.EFFDT = VE.PRICES_EFFDT != null ? Convert.ToDateTime(VE.PRICES_EFFDT) : System.DateTime.Now.Date;
                                MIP.PRCCD = PRCCD;
                                if (VE.MSITEMBARCODE[i].BARNO.retStr() != "")
                                {
                                    MIP.BARNO = VE.MSITEMBARCODE[i].BARNO.retStr();
                                }
                                else
                                {
                                    MIP.BARNO = salesfunc.GenerateBARNO(MSITEM.ITCD, VE.MSITEMBARCODE[i].CLRBARCODE.retStr(), VE.MSITEMBARCODE[i].SZBARCODE);
                                }
                                //if (i == 0)
                                //{
                                //    MIP.BARNO = MSITEMBARCODE.BARNO;
                                //}
                                //else
                                //{
                                //    MIP.BARNO = barno;
                                //}
                                MIP.RATE = prcCols[j].retDbl();
                                DB.T_BATCHMST_PRICE.Add(MIP);

                                //RATE
                                //T_BATCHMST_PRICE TBATCHMSTPRICE = new T_BATCHMST_PRICE();
                                //TBATCHMSTPRICE.EMD_NO = MSITEM.EMD_NO;
                                //TBATCHMSTPRICE.CLCD = MSITEM.CLCD;
                                //TBATCHMSTPRICE.DTAG = MSITEM.DTAG;
                                //TBATCHMSTPRICE.TTAG = MSITEM.TTAG;
                                //TBATCHMSTPRICE.BARNO = MIP.BARNO;
                                //TBATCHMSTPRICE.PRCCD = MIP.PRCCD;
                                //TBATCHMSTPRICE.EFFDT = MIP.EFFDT;
                                //TBATCHMSTPRICE.RATE = MIP.RATE;

                                //DB.T_BATCHMST_PRICE.Add(TBATCHMSTPRICE);
                            }
                        }
                        #endregion
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
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SITEM", VE.M_SITEM.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(), VE.Audit_REM);
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();
                        string sbarno = getmasterbarno(VE.M_SITEM.ITCD); var arrbarno = sbarno.Split(',');
                        DB.T_BATCH_IMG_HDR.Where(x => arrbarno.Contains(x.BARNO)).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.T_BATCH_IMG_HDR_LINK.Where(x => arrbarno.Contains(x.BARNO)).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.T_BATCHMST.Where(x => arrbarno.Contains(x.BARNO)).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.T_BATCHMST_PRICE.Where(x => arrbarno.Contains(x.BARNO)).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_SITEM.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_SITEM_SLCD.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_SITEM_PARTS.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        //DB.T_BATCHmst.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_SITEM_MEASURE.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        //DB.T_BATCHMST_PRICE.Where(x => arrbarno.Contains(x.BARNO)).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.T_BATCH_IMG_HDR_LINK.RemoveRange(DB.T_BATCH_IMG_HDR_LINK.Where(x => arrbarno.Contains(x.BARNO)));
                        DB.SaveChanges();

                        var DOCFLNAMEs = DB.T_BATCH_IMG_HDR.Where(x => arrbarno.Contains(x.BARNO)).Select(e => new
                        DropDown_list
                        { text = e.DOC_FLNAME, value = e.BARNO }).ToList();
                        foreach (var DOC_FLNAME in DOCFLNAMEs)
                        {
                            var extension = Path.GetExtension(DOC_FLNAME.text);
                            string path = CommVar.SaveFolderPath() + "/ItemImages/" + DOC_FLNAME.value + extension;
                            if (System.IO.File.Exists(path))
                            {
                                System.IO.File.Delete(path); //Delete file if it  exist  
                            }
                        }
                        DB.T_BATCH_IMG_HDR.RemoveRange(DB.T_BATCH_IMG_HDR.Where(x => arrbarno.Contains(x.BARNO)));
                        DB.SaveChanges();
                        DB.T_BATCHMST_PRICE.RemoveRange(DB.T_BATCHMST_PRICE.Where(x => arrbarno.Contains(x.BARNO)));
                        DB.SaveChanges();
                        DB.T_BATCHMST.RemoveRange(DB.T_BATCHMST.Where(x => arrbarno.Contains(x.BARNO)));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_SITEM_SLCD.RemoveRange(DB.M_SITEM_SLCD.Where(x => x.ITCD == VE.M_SITEM.ITCD));
                        DB.SaveChanges();
                        //DB.T_BATCHMST_PRICE.RemoveRange(DB.T_BATCHMST_PRICE.Where(x => arrbarno.Contains(x.BARNO)));
                        //DB.SaveChanges();
                        //DB.T_BATCHmst.RemoveRange(DB.T_BATCHmst.Where(x => x.ITCD == VE.M_SITEM.ITCD));
                        DB.M_SITEM_PARTS.RemoveRange(DB.M_SITEM_PARTS.Where(x => x.ITCD == VE.M_SITEM.ITCD));
                        DB.SaveChanges();
                        DB.M_SITEM_MEASURE.RemoveRange(DB.M_SITEM_MEASURE.Where(x => x.ITCD == VE.M_SITEM.ITCD));
                        DB.SaveChanges();
                        DB.M_SITEM.RemoveRange(DB.M_SITEM.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO));
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
        public ActionResult M_FINProduct(FormCollection FC, ItemMasterEntry VE, string Command = "")
        {
            try
            {
                if (Command == "Download Template")
                {
                    string resp = DownloadTemplate(VE);
                    return Content(resp);
                }
                else if (Command.Contains("Upload"))
                {
                    string resp = Item_Upload(VE, FC);
                    return Content(resp);
                }
                //string scm1 = CommVar.CurSchema(UNQSNO).ToString();
                //DataTable dt = new DataTable("Rep_BarcodeImage");
                //dt.Columns.Add("BARNO", typeof(string));
                //dt.Columns.Add("DOC_FLNAME", typeof(string));
                //dt.Columns.Add("LINE1", typeof(string));
                //dt.Columns.Add("LINE2", typeof(string));
                //string sql = "";
                //sql += "select a.barno, a.imgbarno, a.imgslno, b.doc_flname, b.doc_extn, b.doc_desc from ";
                //sql += "(select a.barno, a.barno imgbarno, a.slno imgslno ";
                //sql += "from " + scm1 + ".m_batch_img_hdr a ";
                //sql += "union ";
                //sql += "select a.barno, b.barno imgbarno, b.slno imgslno ";
                //sql += "from " + scm1 + ".m_batch_img_hdr_link a, " + scm1 + ".m_batch_img_hdr b ";
                //sql += "where a.mainbarno=b.barno(+) ) a, ";
                //sql += "" + scm1 + ".m_batch_img_hdr b ";
                //sql += "where a.imgbarno=b.barno(+) and a.imgslno=b.slno(+) ";
                //sql += "union ";
                //sql += "select a.barno, a.imgbarno, a.imgslno, b.doc_flname, b.doc_extn, b.doc_desc from ";
                //sql += "(select a.barno, a.barno imgbarno, a.slno imgslno ";
                //sql += "from " + scm1 + ".t_batch_img_hdr a ";
                //sql += "union ";
                //sql += "select a.barno, b.barno imgbarno, b.slno imgslno ";
                //sql += "from " + scm1 + ".t_batch_img_hdr_link a, " + scm1 + ".t_batch_img_hdr b ";
                //sql += "where a.mainbarno=b.barno(+) ) a, ";
                //sql += "" + scm1 + ".t_batch_img_hdr b ";
                //sql += "where a.imgbarno=b.barno(+) and a.imgslno=b.slno(+)  ";
                //var dttt = masterHelp.SQLquery(sql);
                //for (int i = 0; i < dttt.Rows.Count; i++)
                //{
                //    DataRow dr1 = dt.NewRow();
                //    dr1["BARNO"] = dttt.Rows[i]["BARNO"].ToString();
                //    dr1["DOC_FLNAME"] = dttt.Rows[i]["DOC_FLNAME"].ToString();
                //    dr1["LINE1"] = dttt.Rows[i]["DOC_DESC"].ToString(); ;
                //    dr1["LINE2"] = "1700001 SD";
                //    dt.Rows.Add(dr1);
                //}
                //Session["DtRepBarcodeImage"] = dt;
                //return RedirectToAction("Rep_BarcodeImage", "Rep_BarcodeImage");
                string itgrpcd = ""; string selitcd = ""; string collcd = "";
                if (FC.AllKeys.Contains("itcdvalue")) selitcd = CommFunc.retSqlformat(FC["itcdvalue"].ToString());
                if (FC.AllKeys.Contains("itgrpcdvalue")) itgrpcd = CommFunc.retSqlformat(FC["itgrpcdvalue"].retStr());
                if (FC.AllKeys.Contains("collcdvalue")) collcd = CommFunc.retSqlformat(FC["collcdvalue"].retStr());
                Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                string dbname = CommVar.CurSchema(UNQSNO).ToString();
                string dbname1 = CommVar.FinSchema(UNQSNO).ToString();
                string query = "SELECT distinct A.STYLENO, A.ITNM, A.ITCD, B.ITGRPNM, F.BRANDNM, A.HSNCODE, G.UOMCD, G.UOMNM, h.collcd, h.collnm,i.prccd,i.rate,i.effdt ";
                query = query + "FROM " + dbname + ".M_SITEM A, " + dbname + ".M_GROUP B, ";
                query = query + dbname + ".M_CNTRL_HDR E, " + dbname + ".M_BRAND F, " + dbname1 + ".M_UOM G, " + dbname + ".M_COLLECTION h, ";

                query += "(select a.rate,prccd,SIZECD,COLRCD,to_char(a.effdt,'dd/mm/yyyy')effdt,b.itcd from " + dbname + ".T_BATCHMST_PRICE a," + dbname + ".T_BATCHmst b ";
                query += "where a.barno=b.barno  ";
                query += "and a.effdt = (select max(effdt) from " + dbname + ".T_BATCHMST_PRICE where barno=a.barno)  ";
                query += ") i ";

                query = query + "WHERE A.ITGRPCD = B.ITGRPCD(+)   AND ";
                query = query + "A.M_AUTONO=E.M_AUTONO(+) AND NVL(E.INACTIVE_TAG,'N')='N' AND a.BRANDCD=F.BRANDCD(+) AND A.UOMCD=G.UOMCD(+) AND A.collcd=h.collcd(+) and a.itcd=i.itcd(+) ";
                // query = query + "A.M_AUTONO='" + VE.M_SITEM.M_AUTONO + "' ";
                if (itgrpcd.retStr() != "") query += "and B.itgrpcd in(" + itgrpcd + ") ";
                if (selitcd.retStr() != "") query += "and a.itcd in (" + selitcd + ") ";
                if (collcd.retStr() != "") query += "and h.collcd in (" + collcd + ") ";
                query = query + "GROUP BY A.STYLENO, A.ITNM, A.ITCD, B.ITGRPNM, F.BRANDNM, A.HSNCODE, G.UOMCD, G.UOMNM, h.collcd, h.collnm,i.prccd,i.rate,i.effdt ";
                query = query + "ORDER BY B.ITGRPNM, A.STYLENO,a.itcd ";
                DataTable tbl = masterHelp.SQLquery(query);
                if (tbl.Rows.Count == 0) return Content("No Records");

                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                var M_PRCLST = (from p in DBF.M_PRCLST
                                select new
                                {
                                    PRCCD = p.PRCCD,
                                    PRCNM = p.PRCNM,
                                    SEQNO = p.SEQNO
                                }).OrderBy(S => S.SEQNO).ToList();

                DataTable IR = new DataTable("mstrep");
                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                HC.RepStart(IR, 3);
                HC.GetPrintHeader(IR, "slno", "string", "c,5", "Sl.No");
                HC.GetPrintHeader(IR, "itgrpnm", "string", "c,20", "Group Name");
                HC.GetPrintHeader(IR, "styleno", "string", "c,15", "Design No");
                HC.GetPrintHeader(IR, "itcd", "string", "c,10", "Item;Code");
                HC.GetPrintHeader(IR, "itnm", "string", "c,20", "Item Name");
                HC.GetPrintHeader(IR, "uomnm", "string", "c,10", "UOM");
                //HC.GetPrintHeader(IR, "pcsperbox", "double", "n,5", "Pcs/;Box");
                //HC.GetPrintHeader(IR, "sizes", "string", "c,25", "Sizes");
                HC.GetPrintHeader(IR, "brandnm", "string", "c,15", "Brand Name");
                HC.GetPrintHeader(IR, "hsnsaccd", "string", "c,8", "HSN;Code");
                HC.GetPrintHeader(IR, "collnm", "string", "c,20", "Collection Name");
                HC.GetPrintHeader(IR, "lasteffdt", "string", "c,10", "Last;Effective Date");

                foreach (var plist in M_PRCLST)
                {
                    HC.GetPrintHeader(IR, plist.PRCCD, "double", "n,14,2", plist.PRCNM);
                }

                string lastgrpnm = ""; string lastbrandnm = ""; Int32 slno = 0;

                Int32 rNo = 0; Int32 i = 0; Int32 maxR = 0;
                i = 0; maxR = tbl.Rows.Count - 1;
                while (i <= maxR)
                {
                    slno = slno + 1;
                    string grpnm = tbl.Rows[i]["itgrpnm"].ToString();
                    string brandnm = tbl.Rows[i]["brandnm"].ToString();
                    DataRow dr = IR.NewRow();
                    dr["slno"] = slno;
                    dr["styleno"] = "<b>" + tbl.Rows[i]["styleno"] + "</b>";
                    dr["itgrpnm"] = tbl.Rows[i]["itgrpnm"];
                    dr["itcd"] = tbl.Rows[i]["itcd"];
                    dr["itnm"] = tbl.Rows[i]["itnm"];
                    dr["uomnm"] = tbl.Rows[i]["uomnm"];
                    //dr["pcsperbox"] = Convert.ToDouble(tbl.Rows[i]["pcsperbox"] == DBNull.Value ? 0 : tbl.Rows[i]["pcsperbox"]);
                    //dr["sizes"] = (tbl.Rows[i]["sizes"]);
                    dr["hsnsaccd"] = (tbl.Rows[i]["HSNCODE"]);
                    dr["brandnm"] = tbl.Rows[i]["brandnm"];
                    dr["collnm"] = (tbl.Rows[i]["collnm"]);
                    dr["Flag"] = " class='grid_td'";

                    dr["lasteffdt"] = tbl.Rows[i]["effdt"];
                    string itcd = tbl.Rows[i]["itcd"].retStr();
                    while (tbl.Rows[i]["itcd"].retStr() == itcd)
                    {
                        string colnm = tbl.Rows[i]["prccd"].retStr();
                        if (colnm != "")
                        {
                            dr[colnm] = tbl.Rows[i]["rate"].retDbl();
                        }
                        i++;
                        if (i > maxR) break;
                    }
                    IR.Rows.Add(dr);
                    lastgrpnm = grpnm;
                    lastbrandnm = brandnm;
                    if (i > maxR) break;
                }
                string repname = CommFunc.retRepname("Item_master");
                PV = HC.ShowReport(IR, repname, "Item Master List");
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public ActionResult GetProductGrp()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            return PartialView("_Help2", masterHelp.PRODGRPCD_help(null));
        }
        public ActionResult ProductGrp(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_PRODGRP where (c.PRODGRPCD == val) select c);
            if (query.Any())
            {
                string str = "";
                foreach (var i in query)
                {
                    str = i.PRODGRPCD + Cn.GCS() + i.PRODGRPNM;
                }
                return Content(str);
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult CheckDesignNumber(string STYLE_NO, string M_AUTO_NO)
        {
            ItemMasterEntry VE = new ItemMasterEntry();
            Cn.getQueryString(VE);
            if (VE.DefaultAction == "A") M_AUTO_NO = "0";
            long M_AUTONO = Convert.ToInt64(M_AUTO_NO);
            STYLE_NO = STYLE_NO.retStr().Trim();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            if (STYLE_NO.retStr() == "") return Content("0");

            var itgrptype = "";
            if (VE.MENU_PARA == "F")
            {
                itgrptype = "F";
            }
            else if (VE.MENU_PARA == "C")
            {
                itgrptype = "C";
            }

            var query = (from c in DB.M_SITEM
                         join o in DB.M_GROUP on c.ITGRPCD equals (o.ITGRPCD)
                         where (c.STYLENO == STYLE_NO && c.M_AUTONO != M_AUTONO && o.ITGRPTYPE == itgrptype)
                         select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }
        }
        public dynamic SavePreviousYrData(ItemMasterEntry VE, FormCollection FC, string Tag = "")
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            ImprovarDB DB_PREVYR = new ImprovarDB(Cn.GetConnectionString(), CommVar.LastYearSchema(UNQSNO));
            ImprovarDB DB_PREVYR_temp = new ImprovarDB(Cn.GetConnectionString(), CommVar.LastYearSchema(UNQSNO));
            using (var transaction = DB_PREVYR.Database.BeginTransaction())
            {
                try
                {
                    DateTime PRICES_EFFDT = Convert.ToDateTime(VE.PRICES_EFFDT);
                    string sbarno = getmasterbarno(VE.M_SITEM.ITCD); var arrbarno = sbarno.Split(',');
                    string prev_sbarno = getmasterbarno(VE.M_SITEM.ITCD, CommVar.LastYearSchema(UNQSNO)); var prev_arrbarno = prev_sbarno.Split(',');

                    var PSL = (from j in DB_PREVYR_temp.M_SITEM where (j.ITCD == VE.M_SITEM.ITCD) select j).FirstOrDefault();
                    var SL = (from j in DB.M_SITEM where (j.ITCD == VE.M_SITEM.ITCD) select j).FirstOrDefault();

                    var SBUSNAT = (from j in DB.T_BATCH_IMG_HDR_LINK where arrbarno.Contains(j.BARNO) select j).ToList();
                    var SIFSC = (from j in DB.T_BATCHMST_PRICE where arrbarno.Contains(j.BARNO) select j).ToList();
                    var SGL = (from j in DB.T_BATCHMST where arrbarno.Contains(j.BARNO) select j).ToList();
                    var IMGHDR = (from j in DB.T_BATCH_IMG_HDR where (arrbarno.Contains(j.BARNO)) select j).ToList();
                    var MDOCDTL = (from j in DB.M_CNTRL_HDR_DOC_DTL where (j.M_AUTONO == VE.M_SITEM.M_AUTONO) select j).ToList();
                    var SCONT = (from j in DB.M_SITEM_SLCD where (j.ITCD == VE.M_SITEM.ITCD) select j).ToList();
                    var SLINK = (from j in DB.M_SITEM_PARTS where (j.ITCD == VE.M_SITEM.ITCD) select j).ToList();
                    var STAX = (from j in DB.M_SITEM_MEASURE where (j.ITCD == VE.M_SITEM.ITCD) select j).ToList();
                    var MCNTRLHDRDOC = (from j in DB.M_CNTRL_HDR_DOC where (j.M_AUTONO == VE.M_SITEM.M_AUTONO) select j).ToList();
                    if (PSL == null)
                    {
                        var AUTONO_PREVYR = Cn.M_AUTONO(CommVar.LastYearSchema(UNQSNO));
                        M_CNTRL_HDR MCH_PREVYR = Cn.M_CONTROL_HDR(VE.Checked, "M_SITEM", AUTONO_PREVYR, "A", CommVar.LastYearSchema(UNQSNO), VE.Audit_REM);
                        DB_PREVYR.M_CNTRL_HDR.Add(MCH_PREVYR);
                        DB_PREVYR.SaveChanges();
                        M_SITEM MSUBLEG_PREVYR = new M_SITEM();
                        MSUBLEG_PREVYR = SL;
                        MSUBLEG_PREVYR.M_AUTONO = AUTONO_PREVYR;
                        DB_PREVYR.M_SITEM.Add(MSUBLEG_PREVYR);
                        DB_PREVYR.SaveChanges();


                        if (SBUSNAT.Count != 0) { DB_PREVYR.T_BATCH_IMG_HDR_LINK.AddRange(SBUSNAT); }
                        if (SIFSC.Count != 0) { DB_PREVYR.T_BATCHMST_PRICE.AddRange(SIFSC); }
                        if (IMGHDR.Count != 0) { DB_PREVYR.T_BATCH_IMG_HDR.AddRange(IMGHDR); }
                        if (SGL.Count != 0) { DB_PREVYR.T_BATCHMST.AddRange(SGL); }
                        if (MDOCDTL.Count != 0) { DB_PREVYR.M_CNTRL_HDR_DOC_DTL.AddRange(MDOCDTL); }
                        if (MCNTRLHDRDOC.Count != 0) { DB_PREVYR.M_CNTRL_HDR_DOC.AddRange(MCNTRLHDRDOC); }

                        if (SCONT.Count != 0) { DB_PREVYR.M_SITEM_SLCD.AddRange(SCONT); }
                        if (SLINK.Count != 0)
                        {
                            DB_PREVYR.M_SITEM_PARTS.AddRange(SLINK);
                        }

                        if (STAX.Count != 0) { DB_PREVYR.M_SITEM_MEASURE.AddRange(STAX); }

                        DB_PREVYR.SaveChanges();
                        ModelState.Clear();
                        transaction.Commit();
                    }
                    else
                    {
                        M_CNTRL_HDR MCH_PREVYR = Cn.M_CONTROL_HDR(VE.Checked, "M_SITEM", PSL.M_AUTONO.retInt(), "E", CommVar.LastYearSchema(UNQSNO), VE.Audit_REM);
                        DB_PREVYR.Entry(MCH_PREVYR).State = System.Data.Entity.EntityState.Modified;
                        DB_PREVYR.SaveChanges();

                        M_SITEM MSUBLEG_PREVYR = new M_SITEM();
                        MSUBLEG_PREVYR = SL; MSUBLEG_PREVYR.M_AUTONO = MCH_PREVYR.M_AUTONO;
                        DB_PREVYR.Entry(MSUBLEG_PREVYR).State = System.Data.Entity.EntityState.Modified;
                        DB_PREVYR.SaveChanges();

                        DB_PREVYR.T_BATCH_IMG_HDR_LINK.Where(x => arrbarno.Contains(x.BARNO)).ToList().ForEach(x => { x.DTAG = "E"; });
                        DB_PREVYR.T_BATCH_IMG_HDR_LINK.RemoveRange(DB_PREVYR.T_BATCH_IMG_HDR_LINK.Where(x => arrbarno.Contains(x.BARNO)));
                        DB_PREVYR.SaveChanges();

                        DB_PREVYR.T_BATCHMST_PRICE.Where(x => arrbarno.Contains(x.BARNO)).ToList().ForEach(x => { x.DTAG = "E"; });
                        DB_PREVYR.T_BATCHMST_PRICE.RemoveRange(DB_PREVYR.T_BATCHMST_PRICE.Where(x => arrbarno.Contains(x.BARNO)));
                        DB_PREVYR.SaveChanges();

                        ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.LastYearSchema(UNQSNO));
                        var comp = DB1.T_BATCHMST.Where(x => arrbarno.Contains(x.BARNO)).OrderBy(s => s.BARNO).ToList();
                        foreach (var v in comp)
                        {
                            if (!VE.MSITEMBARCODE.Where(s => s.BARNO == v.BARNO).Any())
                            {
                                DB_PREVYR.T_BATCHMST.Where(x => x.BARNO == v.BARNO).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB_PREVYR.T_BATCHMST.RemoveRange(DB_PREVYR.T_BATCHMST.Where(x => x.BARNO == v.BARNO));
                                DB_PREVYR.SaveChanges();
                            }
                        }
                        //DB_PREVYR.T_BATCHMST.RemoveRange(DB_PREVYR.T_BATCHMST.Where(x => arrbarno.Contains(x.BARNO)));
                        //DB_PREVYR.SaveChanges();
                        DB_PREVYR.T_BATCH_IMG_HDR.Where(x => arrbarno.Contains(x.BARNO)).ToList().ForEach(x => { x.DTAG = "E"; });
                        DB_PREVYR.T_BATCH_IMG_HDR.RemoveRange(DB_PREVYR.T_BATCH_IMG_HDR.Where(x => arrbarno.Contains(x.BARNO)));
                        DB_PREVYR.SaveChanges();

                        DB_PREVYR.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == MCH_PREVYR.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                        DB_PREVYR.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB_PREVYR.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == MCH_PREVYR.M_AUTONO));
                        DB_PREVYR.SaveChanges();

                        DB_PREVYR.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == MCH_PREVYR.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                        DB_PREVYR.M_CNTRL_HDR_DOC.RemoveRange(DB_PREVYR.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == MCH_PREVYR.M_AUTONO));
                        DB_PREVYR.SaveChanges();

                        DB_PREVYR.M_SITEM_SLCD.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "E"; });
                        DB_PREVYR.M_SITEM_SLCD.RemoveRange(DB_PREVYR.M_SITEM_SLCD.Where(x => x.ITCD == VE.M_SITEM.ITCD));
                        DB_PREVYR.SaveChanges();

                        DB_PREVYR.M_SITEM_PARTS.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "E"; });
                        DB_PREVYR.M_SITEM_PARTS.RemoveRange(DB_PREVYR.M_SITEM_PARTS.Where(x => x.ITCD == VE.M_SITEM.ITCD));
                        DB_PREVYR.SaveChanges();

                        DB_PREVYR.M_SITEM_MEASURE.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "E"; });
                        DB_PREVYR.M_SITEM_MEASURE.RemoveRange(DB_PREVYR.M_SITEM_MEASURE.Where(x => x.ITCD == VE.M_SITEM.ITCD));
                        DB_PREVYR.SaveChanges();


                        if (SBUSNAT.Count != 0) { DB_PREVYR.T_BATCH_IMG_HDR_LINK.AddRange(SBUSNAT); }

                        SGL = SGL.Where(a => !prev_arrbarno.Contains(a.BARNO)).ToList();
                        if (SGL.Count != 0) { DB_PREVYR.T_BATCHMST.AddRange(SGL); }

                        if (SCONT.Count != 0) { DB_PREVYR.M_SITEM_SLCD.AddRange(SCONT); }
                        if (IMGHDR.Count != 0) { DB_PREVYR.T_BATCH_IMG_HDR.AddRange(IMGHDR); }
                        if (SIFSC.Count != 0) { DB_PREVYR.T_BATCHMST_PRICE.AddRange(SIFSC); }
                        if (MCNTRLHDRDOC.Count != 0)
                        {
                            foreach (var v in MCNTRLHDRDOC)
                            {
                                v.M_AUTONO = MCH_PREVYR.M_AUTONO;
                            }
                            DB_PREVYR.M_CNTRL_HDR_DOC.AddRange(MCNTRLHDRDOC);
                        }
                        if (MDOCDTL.Count != 0)
                        {
                            foreach (var v in MDOCDTL)
                            {
                                v.M_AUTONO = MCH_PREVYR.M_AUTONO;
                            }
                            DB_PREVYR.M_CNTRL_HDR_DOC_DTL.AddRange(MDOCDTL);
                        }
                        if (SLINK.Count != 0) { DB_PREVYR.M_SITEM_PARTS.AddRange(SLINK); }
                        if (STAX.Count != 0) { DB_PREVYR.M_SITEM_MEASURE.AddRange(STAX); }

                        DB_PREVYR.SaveChanges();
                        ModelState.Clear();
                        transaction.Commit();
                    }
                    if (Tag != "")
                    {
                        return "1";
                    }
                    return Content("1");
                }


                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.Clear();
                    Cn.SaveException(ex, "");
                    if (Tag != "")
                    {
                        return ex.Message + " " + ex.InnerException;
                    }
                    return Content(ex.Message + " " + ex.InnerException);
                }
            }
        }
        public string DownloadTemplate(ItemMasterEntry VE)
        {
            try
            {
                Cn.getQueryString(VE);
                string MNUP = VE.MENU_PARA; string nm = "";
                switch (MNUP)
                {
                    case "F":
                        nm = "Finish Product/Design"; break;
                    case "C":
                        nm = "Fabric Item"; break;
                    case "A":
                        nm = "Accessories / Other Items"; break;
                    default: ViewBag.formname = ""; break;
                }
                //DESIGN ITEM GROUP ITEM NAME UOM HSN CODE    FAB ITNM    BARNO WPRATE    MRP CPRATE   RPRATE JOBPRATE    JOBSRATE IGST    RNO ITLEGACYCD  LEGACYCD

                string Excel_Header = "DESIGN" + "|" + "ITEM GROUP NAME" + "|" + "ITEM NAME" + "|" + "UOM CODE" + "|" + "HSN CODE" + "|" + "FAB ITNM"
                    + "|" + "BARNO" + "|" + "WPRATE" + "|" + "MRP" + " |" + "CPRATE" + "|" + "RPRATE" + "|" + "JOBPRATE" + "|" + "JOBSRATE" + "|" + "IGST" + "|" + "RNO"
                    + "|" + "ITLEGACYCD" + "|" + "LEGACYCD" + "|" + "Sales Ledger Code" + "|" + "Purchase Ledger Code" + "|" + "Bill_UOM" + "|" + "Effective Date" + "|" + "Allow Negetive Stock(Y/N)";

                ExcelPackage ExcelPkg = new ExcelPackage();
                ExcelWorksheet wsSheet1 = ExcelPkg.Workbook.Worksheets.Add("Sheet1");

                using (ExcelRange Rng = wsSheet1.Cells["A1:V1"])
                {
                    Rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    Rng.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    string[] Header = Excel_Header.Split('|');
                    for (int i = 0; i < Header.Length; i++)
                    {
                        wsSheet1.Cells[1, i + 1].Value = Header[i];
                    }
                }
                wsSheet1.Cells[1, 1, 1, 22].AutoFitColumns();

                Response.Clear();
                Response.ClearContent();
                Response.Buffer = true;
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("Content-Disposition", "attachment; filename=" + nm + ".xlsx");
                Response.BinaryWrite(ExcelPkg.GetAsByteArray());
                Response.Flush();
                Response.Close();
                Response.End();
                return "Download Sucessfull";
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return ex.Message + ex.InnerException + "  ";
            }
        }
        public string Item_Upload(ItemMasterEntry VE, FormCollection FC)
        {

            string msg = ""; string MESSAGE = "", strerrline = "";
            Stream stream;
            try
            {
                Cn.getQueryString(VE);
                if (Request.Files.Count == 0) return "No File Selected";
                HttpPostedFileBase file = Request.Files[0];
                if (System.IO.Path.GetExtension(file.FileName) != ".xlsx") return ".xlsx file need to choose";
                stream = file.InputStream;
                // Excel Columns: DESIGN ITEM GROUP ITEM NAME UOM HSN CODE    FAB ITNM    BARNO WPRATE    MRP CPRATE   RPRATE JOBPRATE    JOBSRATE IGST    RNO ITLEGACYCD  LEGACYCD Bill_UOM

                DataTable dbfdt = new DataTable();
                dbfdt.Columns.Add("Sl", typeof(int));
                dbfdt.Columns.Add("DESIGN", typeof(string));
                dbfdt.Columns.Add("ITEM GROUP NAME", typeof(string));
                dbfdt.Columns.Add("ITEM NAME", typeof(string));
                dbfdt.Columns.Add("UOM CODE", typeof(string));
                dbfdt.Columns.Add("HSN CODE", typeof(string));
                dbfdt.Columns.Add("FAB ITNM", typeof(string));
                dbfdt.Columns.Add("BARNO", typeof(string));
                dbfdt.Columns.Add("WPRATE", typeof(string));
                dbfdt.Columns.Add("MRP", typeof(string));
                dbfdt.Columns.Add("CPRATE", typeof(string));
                dbfdt.Columns.Add("RPRATE", typeof(string));
                dbfdt.Columns.Add("JOBPRATE", typeof(string));
                dbfdt.Columns.Add("JOBSRATE", typeof(string));
                dbfdt.Columns.Add("IGST", typeof(string));
                dbfdt.Columns.Add("RNO", typeof(string));
                dbfdt.Columns.Add("ITLEGACYCD", typeof(string));
                dbfdt.Columns.Add("LEGACYCD", typeof(string));
                dbfdt.Columns.Add("Sales Ledger Code", typeof(string));
                dbfdt.Columns.Add("Purchase Ledger Code", typeof(string));
                dbfdt.Columns.Add("Bill_UOM", typeof(string));
                dbfdt.Columns.Add("Effective Date", typeof(string));
                dbfdt.Columns.Add("Allow Negetive Stock(Y/N)", typeof(string));

                using (var package = new ExcelPackage(stream))
                {
                    var currentSheet = package.Workbook.Worksheets;
                    var workSheet = currentSheet.First();
                    var noOfCol = workSheet.Dimension.End.Column;
                    var noOfRow = workSheet.Dimension.End.Row;
                    int rowNum = 2;
                    for (rowNum = 2; rowNum <= noOfRow; rowNum++)
                    {
                        if (workSheet.Cells[rowNum, 1].Value.retStr() != "")
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
                                    return "Wrong ColumnName:" + colname + " Error:" + ex.Message;
                                }
                            }
                            dbfdt.Rows.Add(dr);
                        }
                    }
                }

                int excelrow = 1;
                DateTime dt = Convert.ToDateTime(CommVar.FinStartDate(UNQSNO)).AddDays(-365);
                // string effdt =VE.PreviousYrUpload==true? dt.retDateStr() : System.DateTime.Now.retDateStr();

                foreach (DataRow oudr in dbfdt.Rows)
                {
                    ++excelrow; msg = "<br/><br/> Excelrow:" + excelrow + " ";
                    string DESIGN = oudr["DESIGN"].retStr();
                    string GRPNM = oudr["ITEM GROUP NAME"].retStr();
                    string ITNM = oudr["ITEM NAME"].retStr();
                    string UOM = oudr["UOM CODE"].retStr();
                    string HSNCODE = oudr["HSN CODE"].retStr();
                    string FABITNM = oudr["FAB ITNM"].retStr();
                    string BARNO = oudr["BARNO"].retStr();
                    double WPRATE = oudr["WPRATE"].retDbl();
                    double MRP = oudr["MRP"].retDbl();
                    double CPRATE = oudr["CPRATE"].retDbl();
                    double RPRATE = oudr["RPRATE"].retDbl();
                    string JOBPRATE = oudr["JOBPRATE"].retStr();
                    string JOBSRATE = oudr["JOBSRATE"].retStr();
                    double IGST = oudr["IGST"].retDbl();
                    string RNO = oudr["RNO"].retStr();
                    string ITLEGACYCD = oudr["ITLEGACYCD"].retStr();
                    string SALGLCD = oudr["Sales Ledger Code"].retStr();
                    string PURGLCD = oudr["Purchase Ledger Code"].retStr();
                    string Bill_UOM = oudr["Bill_UOM"].retStr();
                    string EFFDT = oudr["Effective Date"].retDateStr();
                    string Negetive_Stock = oudr["Allow Negetive Stock(Y/N)"].retStr().ToUpper();

                    string itgrptype = "";
                    if (VE.MENU_PARA == "C")//Fabric Item
                    {
                        itgrptype = "C";

                    }
                    else if (VE.MENU_PARA == "F")//Finish Product/Design
                    {
                        itgrptype = "F";
                    }
                    else
                    {
                        itgrptype = "A";
                    }
                    bool flag = true;
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    var query1 = (from c in DB.T_BATCHMST
                                  where (c.BARNO.Trim() == BARNO)
                                  select new { c.BARNO }).FirstOrDefault();
                    if (query1 != null)
                    {
                        MESSAGE += msg + "Barno (" + BARNO + ") Already Exists : Please Enter a Different Barno !!";
                        strerrline += excelrow + ",";
                        continue;
                    }
                    var query = (from c in DB.M_SITEM
                                 join d in DB.T_BATCHMST on c.ITCD equals d.ITCD into x
                                 from d in x.DefaultIfEmpty()
                                 join h in DB.M_GROUP on c.ITGRPCD equals h.ITGRPCD into y
                                 from h in y.DefaultIfEmpty()
                                 where (c.STYLENO.Trim() == DESIGN)
                                 select new { c.ITCD, d.BARNO, h.ITGRPTYPE }).FirstOrDefault();
                    if (query != null)
                    {
                        if (VE.SkipDuplicateDesign == true)
                        {
                            MESSAGE += msg + "Design (" + DESIGN + ") Number Already Exists : Please Enter a Different Design Number !!";
                            strerrline += excelrow + ",";
                            flag = false;
                        }
                        else
                        {
                            if (query.ITGRPTYPE == itgrptype)
                            {
                                int len = DESIGN.Length;

                                string sql = "select styleno from " + CommVar.CurSchema(UNQSNO) + ".m_sitem ";
                                sql += "where SUBSTR( styleno,0," + len + ")='" + DESIGN + "' and REGEXP_LIKE(substr(styleno, " + (len + 1) + ",1),'^[a-zA-Z]') ";
                                sql += "order by styleno desc";
                                DataTable design = masterHelp.SQLquery(sql);
                                if (design != null && design.Rows.Count > 0)
                                {
                                    var maxval = Encoding.ASCII.GetBytes(design.Rows[0]["styleno"].retStr().Substring(len, 1))[0];
                                    var maxchar = Encoding.ASCII.GetString(BitConverter.GetBytes(maxval + 1), 0, 1);
                                    DESIGN = DESIGN + maxchar;
                                }
                                else
                                {
                                    DESIGN = DESIGN + "A";
                                }
                            }
                            else
                            {
                                MESSAGE += msg + "Design (" + DESIGN + ") Number Already Exists for itgrptype = " + itgrptype + " : Please Enter a Different Design Number !!";
                                strerrline += excelrow + ",";
                                flag = false;
                            }
                        }
                    }

                    //var query1 = (from c in DB.M_GROUP
                    //              where (c.ITGRPNM.ToUpper() == GRPNM && c.ITGRPTYPE == itgrptype)
                    //              select c);
                    //if (!query1.Any())
                    //{
                    //    MESSAGE += msg + "Please Create Group (" + GRPNM + ") Group Type : " + itgrptype;
                    //    flag = false;
                    //}

                    if (flag == true)
                    {
                        if (CPRATE + WPRATE + RPRATE != 0 && EFFDT == "")
                        {
                            MESSAGE += msg + "Please add Effective Date for style:(" + DESIGN + ")";
                            strerrline += excelrow + ",";
                            continue;
                        }
                        if (Negetive_Stock != "" && Negetive_Stock != "Y" && Negetive_Stock != "N")
                        {
                            MESSAGE += msg + "Allow Negetive Stock Should be (Y/N/blank) for style:(" + DESIGN + ")";
                            strerrline += excelrow + ",";
                            continue;
                        }
                        ItemDet ItemDet = new ItemDet();
                        if (VE.MENU_PARA == "C")//Fabric Item
                        {
                            ItemDet = salesfunc.CreateItem(DESIGN, UOM, GRPNM, HSNCODE, "", BARNO, itgrptype, "", ITNM, "", "", IGST, true, SALGLCD, PURGLCD, Bill_UOM, Negetive_Stock);

                        }
                        else if (VE.MENU_PARA == "F")//Finish Product/Design
                        {
                            string fabitcd = "";
                            if (FABITNM != "")
                            {
                                fabitcd = getFABITCD(FABITNM);
                                if (fabitcd == "")
                                {
                                    MESSAGE += msg + "Please add Fabric Item:(" + FABITNM + ") ";
                                    strerrline += excelrow + ",";
                                    continue;
                                }
                            }

                            ItemDet = salesfunc.CreateItem(DESIGN, UOM, GRPNM, HSNCODE, fabitcd, BARNO, itgrptype, "", ITNM, "", "", IGST, true, SALGLCD, PURGLCD, Bill_UOM, Negetive_Stock);
                        }
                        else//Accessories / Other Items
                        {
                            ItemDet = salesfunc.CreateItem(DESIGN, UOM, GRPNM, HSNCODE, "", BARNO, itgrptype, "", ITNM, "", "", IGST, true, SALGLCD, PURGLCD, Bill_UOM, Negetive_Stock);
                        }
                        if (ItemDet.ITCD.retStr() == "" && ItemDet.ErrMsg.retStr() == "")
                        {
                            MESSAGE += msg + "Please add style:(" + DESIGN + ") at Item Master Manually because master transfer done in next year . ";
                            strerrline += excelrow + ",";
                        }
                        else if (ItemDet.ErrMsg.retStr() != "")
                        {
                            MESSAGE += msg + ItemDet.ErrMsg;
                            strerrline += excelrow + ",";
                        }
                        else
                        {
                            string msgrate = "";
                            if (CPRATE + WPRATE + RPRATE != 0)
                            {
                                string tempmsg = salesfunc.CreatePricelist(ItemDet.BARNO, EFFDT, CPRATE, WPRATE, RPRATE);
                                if (tempmsg != "ok") msgrate = "<br/>" + tempmsg;
                            }
                            string msgprev = "";
                            if (msgrate == "" && CommVar.LastYearSchema(UNQSNO) != "" && VE.PreviousYrUpload == true)
                            {
                                VE.M_SITEM.ITCD = ItemDet.ITCD;
                                VE.M_SITEM.M_AUTONO = ItemDet.M_AUTONO;
                                VE.PRICES_EFFDT = EFFDT;
                                string tempmsg = SavePreviousYrData(VE, FC, "Upload");
                                if (tempmsg != "1") msgprev = "<br/>" + tempmsg;

                            }
                            MESSAGE += msg + "Success itcd :" + ItemDet.ITCD + " Design No : " + DESIGN + msgrate + msgprev;
                        }
                    }


                }

            }//try
            catch (Exception ex)
            {
                Cn.SaveException(ex, msg);
                return ex.Message + " at " + msg;
            }
            if (strerrline == "" && CommVar.LastYearSchema(UNQSNO) != "" && VE.PreviousYrUpload == true)
            {
                return "Excel Uploaded Successfully Current Year and last year !! <br/>" + MESSAGE;
            }
            //else if (strerrline == "" && CommVar.LastYearSchema(UNQSNO) == "" && VE.PreviousYrUpload == false)
            //{
            //    return "Excel Uploaded Successfully Current Year !! <br/>" + MESSAGE;
            //}
            else if (strerrline == "" && VE.PreviousYrUpload == false)
            {
                return "Excel Uploaded Successfully Current Year !! <br/>" + MESSAGE;
            }
            else
            {
                using (var ExcelPkg = new ExcelPackage(stream))
                {
                    string[] errline = strerrline.Split(',');
                    int rowNum = 2;
                    var noOfRow = ExcelPkg.Workbook.Worksheets[1].Dimension.End.Row;
                    for (rowNum = noOfRow; rowNum >= 2; rowNum--)
                    {
                        if (!errline.Contains(rowNum.retStr()))
                        {
                            ExcelPkg.Workbook.Worksheets[1].DeleteRow(rowNum, 1);
                        }
                    }
                    ExcelPkg.Workbook.Worksheets.Add("sheet2");

                    rowNum = 1;
                    string[] arrerrmsg = MESSAGE.Split(new string[] { "<br/>" }, StringSplitOptions.None);
                    foreach (var v in arrerrmsg)
                    {
                        ExcelPkg.Workbook.Worksheets[2].Cells[rowNum, 1].Value = v;
                        rowNum++;
                    }
                    Response.Clear();
                    Response.ClearContent();
                    Response.Buffer = true;
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("Content-Disposition", "attachment; filename=FinProduct.xlsx");
                    Response.BinaryWrite(ExcelPkg.GetAsByteArray());
                    Response.Flush();
                    Response.Close();
                    Response.End();
                    return "Excel Uploaded Successfully !!";
                }
            }
        }
        private string getFABITCD(string fabitnm)
        {
            string sql = "select a.itcd from " + CommVar.CurSchema(UNQSNO) + ".M_SITEM a," + CommVar.CurSchema(UNQSNO) + ".M_GROUP b  where a.ITGRPCD=b.ITGRPCD and b.ITGRPTYPE='C' ";
            if (fabitnm != "") sql += " and upper(a.itnm)='" + fabitnm.Trim().ToUpper() + "'";
            var dt = masterHelp.SQLquery(sql);
            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0]["itcd"].ToString();
            }
            return "";
        }
        public ActionResult GetLinkItemDetails(string val, string Code)
        {
            try
            {
                string scm1 = CommVar.CurSchema(UNQSNO);
                string sql = "";
                string valsrch = val.ToUpper().Trim();

                sql += "select nvl(a.linkitcd,a.itcd) itcd, b.itnm,b.styleno||' '||b.itnm itstyle,b.uomcd ";
                sql += " from " + scm1 + ".m_sitem a," + scm1 + ".m_sitem b, " + scm1 + ".M_CNTRL_HDR c ";
                sql += "where nvl(a.linkitcd,a.itcd)=b.itcd and  b.M_AUTONO=c.M_AUTONO(+) and c.INACTIVE_TAG = 'N' ";
                if (valsrch.retStr() != "") sql += "and ( upper(nvl(a.linkitcd,a.itcd)) = '" + valsrch + "' ) ";
                if (Code.retStr() != "") sql += "and nvl(a.linkitcd,a.itcd)<>'" + Code + "'  ";

                sql += "order by b.itcd,b.itnm";

                DataTable rsTmp = masterHelp.SQLquery(sql);

                if (val.retStr() == "" || rsTmp.Rows.Count > 1)
                {
                    System.Text.StringBuilder SB = new System.Text.StringBuilder();
                    for (int i = 0; i <= rsTmp.Rows.Count - 1; i++)
                    {
                        SB.Append("<tr><td>" + rsTmp.Rows[i]["itcd"] + "</td><td>" + rsTmp.Rows[i]["itstyle"] + "</td><td>" + rsTmp.Rows[i]["uomcd"] + "</td></tr>");
                    }
                    var hdr = "Item Code" + Cn.GCS() + "Item Name" + Cn.GCS() + "UOM";
                    return PartialView("_Help2", masterHelp.Generate_help(hdr, SB.ToString()));
                }
                else
                {
                    string str = "";
                    if (rsTmp.Rows.Count > 0)
                    {
                        str = masterHelp.ToReturnFieldValues("", rsTmp);
                    }
                    else
                    {
                        str = "Invalid Item Code. Please Enter a Valid Item Code !!";
                    }
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult SaveItemImage(ItemMasterEntry VE)
        {
            string m_autono = "";
            string errautono = "";
            try
            {
                string outputFolder = CommVar.SaveFolderPath() + "/ItemImages/";
                List<string> imgTypes = new List<string> { "jpg", "jpeg", "jpe", "bmp", "gif", "png" };

                string scm1 = CommVar.CurSchema(UNQSNO);
                string sql = "";
                Improvar.Models.ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                var doc = (from s in DB.M_CNTRL_HDR_DOC
                           join a in DB.M_SITEM on s.M_AUTONO equals a.M_AUTONO
                           join b in DB.T_BATCHMST on a.ITCD equals b.ITCD
                           where b.COMMONUNIQBAR == "C" && b.AUTONO == null
                           orderby s.M_AUTONO, s.SLNO
                           select new
                           {
                               s.M_AUTONO,
                               s.SLNO,
                               s.DOC_FLNAME,
                               b.BARNO
                           }).ToList();
                foreach (var i in doc)
                {
                    m_autono = i.M_AUTONO + " " + i.SLNO;

                    var image = (from h in DB.M_CNTRL_HDR_DOC_DTL
                                 where h.M_AUTONO == i.M_AUTONO && h.SLNO == i.SLNO
                                 select h).OrderBy(d => d.RSLNO).ToList();

                    var hh = image.GroupBy(x => x.M_AUTONO).Select(x => new
                    {
                        namefl = string.Join("", x.Select(n => n.DOC_STRING))
                    });
                    foreach (var ii in hh)
                    {
                        string base64String = ii.namefl.retStr(); // shortened for clarity
                                                                  // Remove the prefix
                        var base64Data = base64String.Substring(base64String.IndexOf(",") + 1);
                        byte[] imageData = Convert.FromBase64String(base64Data);
                        string extn = i.DOC_FLNAME.ToString().Split('.').Last();
                        bool isImage = imgTypes.Any(img => !img.Except(extn).Any());
                        if (isImage)
                        {
                            string imageName = i.BARNO + "_" + i.SLNO + "." + extn; // e.g., "example.jpg"
                            string fullPath = Path.Combine(outputFolder, imageName);
                            if (!System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.WriteAllBytes(fullPath, imageData);
                            }
                        }
                    }


                }
                return null;
            }
            catch (Exception ex)
            {
                errautono += m_autono + "/";
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException + " " + m_autono);
            }
        }


    }
}