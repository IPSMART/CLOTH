using System;
using System.Linq;
using System.Web.Mvc;                                                   
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class M_Plist_CodeController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp Master_Help = new MasterHelp();
        MasterHelpFa Master_HelpFa = new MasterHelpFa();
        M_PRCLST sl;
        M_SUBLEG sSBLDGE;
        M_CNTRL_HDR sll;
        ImprovarDB DBF;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_Plist_Code
        public ActionResult M_Plist_Code(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Price List Code Master";
                    PriceListCodeMasterEntry VE = new PriceListCodeMasterEntry();
                    string loca1 = CommVar.Loccd(UNQSNO).Trim();
                    ImprovarDB DBIMP = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    var doctP = (from i in DBIMP.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    // ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                  
                    //=================For Price List Group ================//
                    List<DropDown_list> DDl = new List<DropDown_list>();
                    DropDown_list DDl1 = new DropDown_list();
                    DDl1.text = "EXFTY";
                    DDl1.value = "EXFTY";
                    DDl.Add(DDl1);
                    DropDown_list DDl2 = new DropDown_list();
                    DDl2.text = "MRP";
                    DDl2.value = "MRP";
                    DDl.Add(DDl2);
                    DropDown_list DDl3 = new DropDown_list();
                    DDl3.text = "PARTY";
                    DDl3.value = "PARTY";
                    DDl.Add(DDl3);
                    DropDown_list DDl4 = new DropDown_list();
                    DDl4.text = "RAKA";
                    DDl4.value = "RAKA";
                    DDl.Add(DDl4);
                    DropDown_list DDl5 = new DropDown_list();
                    DDl5.text = "WP";
                    DDl5.value = "WP";
                    DDl.Add(DDl5);
                    VE.DropDown_list = DDl;
                    //=================End ================//
                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DBF.M_PRCLST orderby p.PRCCD select new IndexKey() { Navikey = p.PRCCD }).ToList();
                        if (op == "E" || op == "D" || op == "V")
                        {
                          
                            if (searchValue.Length != 0)
                            {
                                VE.Index = Nindex;
                                VE = Navigation(VE, DBF, 0, searchValue);
                            }
                            else
                            {
                                if (key == "" || key == "F")
                                {
                                    VE.Index = 0;
                                    VE = Navigation(VE, DBF, 0, searchValue);
                                }
                                else if (key == "L")
                                {
                                    VE.Index = VE.IndexKey.Count - 1;
                                    VE = Navigation(VE, DBF, VE.IndexKey.Count - 1, searchValue);
                                }
                                else if (key == "P")
                                {
                                    Nindex -= 1;
                                    if (Nindex < 0)
                                    {
                                        Nindex = 0;
                                    }
                                    VE.Index = Nindex;
                                    VE = Navigation(VE, DBF, Nindex, searchValue);
                                }
                                else if (key == "N")
                                {
                                    Nindex += 1;
                                    if (Nindex > VE.IndexKey.Count - 1)
                                    {
                                        Nindex = VE.IndexKey.Count - 1;
                                    }
                                    VE.Index = Nindex;
                                    VE = Navigation(VE, DBF, Nindex, searchValue);
                                }
                            }
                            if (sll.INACTIVE_TAG == "Y")
                            {
                                VE.Deactive = true;
                            }
                            else
                            {
                                VE.Deactive = false;
                            }
                            VE.M_PRCLST = sl;
                            VE.M_SUBLEG = sSBLDGE;
                            VE.M_CNTRL_HDR = sll;
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
                PriceListCodeMasterEntry VE = new PriceListCodeMasterEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public PriceListCodeMasterEntry Navigation(PriceListCodeMasterEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new M_PRCLST();
            sSBLDGE = new M_SUBLEG();
            sll = new M_CNTRL_HDR();
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
                sl = DB.M_PRCLST.Find(aa[0].Trim());
                sSBLDGE = DB.M_SUBLEG.Find(sl.SLCD);
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);

                if (sll.INACTIVE_TAG == "Y")
                {
                    VE.Deactive = true;
                }
                else
                {
                    VE.Deactive = false;
                }
            }
            else
            {

            }
            return VE;
        }
        public ActionResult SearchPannelData()
        {
            DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            var MDT = (from j in DBF.M_PRCLST
                       join p in DBF.M_CNTRL_HDR on j.M_AUTONO 
                       equals (p.M_AUTONO) where (p.M_AUTONO == j.M_AUTONO)
                       select new 
                       {
                           PRCCD = j.PRCCD,
                           PRCNM = j.PRCNM }).Distinct().OrderBy(s => s.PRCCD).ToList();

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Price List Code" + Cn.GCS() + "Price List Name";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].PRCCD + "</td><td>" + MDT[j].PRCNM + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "0"));
        }
        public ActionResult CheckPriceListCode(string val)
        {
            string VALUE = val.ToUpper();
            DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            var query = (from c in DBF.M_PRCLST where (c.PRCCD == VALUE) select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }

        }
        public ActionResult GetSubLedger()
        {
            ImprovarDB DBfin = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            return PartialView("_Help2", Master_HelpFa.SLCD_help(DBfin));
        }
        public ActionResult SubLedger(string val)
        {
            DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            var query = (from c in DBF.M_SUBLEG where (c.SLCD == val) select c);
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
        public ActionResult SAVE(FormCollection FC, PriceListCodeMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));  //fin_casu==
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.FinSchema(UNQSNO) + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_PRCLST MGOD = new M_PRCLST();
                        MGOD.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            MGOD.EMD_NO = 0;
                            MGOD.M_AUTONO = Cn.M_AUTONO(CommVar.FinSchema(UNQSNO));
                        }
                        else
                        {
                            MGOD.M_AUTONO = VE.M_PRCLST.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_CNTRL_HDR where p.M_AUTONO == MGOD.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MGOD.EMD_NO = 0;
                            }
                            else
                            {
                                MGOD.EMD_NO = Convert.ToByte( MAXEMDNO+1);
                            }
                        }

                        MGOD.PRCCD = VE.M_PRCLST.PRCCD.ToUpper();
                        MGOD.PRCNM = VE.M_PRCLST.PRCNM;
                        MGOD.PRCGRP = FC["PRCGRP"].ToString();
                        MGOD.SLCD = VE.M_PRCLST.SLCD;
                        //Control header 
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_PRCLST", MGOD.M_AUTONO.Value, VE.DefaultAction, CommVar.FinSchema(UNQSNO));
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_PRCLST.Add(MGOD);
                            DB.M_CNTRL_HDR.Add(MCH);
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MGOD).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        }
                        DB.SaveChanges();
                        ModelState.Clear();
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
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_PRCLST", VE.M_PRCLST.M_AUTONO.Value, VE.DefaultAction, CommVar.FinSchema(UNQSNO));
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_PRCLST.Where(x => x.M_AUTONO == VE.M_PRCLST.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.M_PRCLST.RemoveRange(DB.M_PRCLST.Where(x => x.M_AUTONO == VE.M_PRCLST.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_PRCLST.M_AUTONO));
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
                catch (Exception ex)
                {
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }
        }
    }
}