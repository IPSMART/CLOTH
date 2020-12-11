using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class M_AmttypeController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp Master_Help = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        M_AMTTYPE sl; M_CNTRL_HDR sll; M_GENLEG sGEN;
        // GET: M_Amttype
        public ActionResult M_Amttype(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Amount Type Master";
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    AmountTypeMasterEntry VE = new AmountTypeMasterEntry();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);

                    List<DropDown_list> list = new List<DropDown_list>();
                    DropDown_list obj1 = new DropDown_list();
                    obj1.text = "Fixed";
                    obj1.value = "F";
                    list.Add(obj1);

                    // FTPControl FTPControl = new FTPControl();

                    //FTPControl.FTPDownloadFile("", "chemmast.7z", "e:\\asp.net");

                    DropDown_list obj2 = new DropDown_list();
                    obj2.text = "Percentage";
                    obj2.value = "P";
                    list.Add(obj2);
                    VE.DropDown_list = list;

                    List<DropDown_list1> list1 = new List<DropDown_list1>();
                    DropDown_list1 obj3 = new DropDown_list1();
                    obj3.text = "IGST";
                    obj3.value = "IG";
                    list1.Add(obj3);
                    DropDown_list1 obj4 = new DropDown_list1();
                    obj4.text = "CGST";
                    obj4.value = "CG";
                    list1.Add(obj4);
                    DropDown_list1 obj5 = new DropDown_list1();
                    obj5.text = "SGST";
                    obj5.value = "SG";
                    list1.Add(obj5);
                    DropDown_list1 obj6 = new DropDown_list1();
                    obj6.text = "Cess";
                    obj6.value = "CS";
                    list1.Add(obj6);
                    DropDown_list1 obj7 = new DropDown_list1();
                    obj7.text = "Import Duty";
                    obj7.value = "IM";
                    list1.Add(obj7);
                    VE.DropDown_list1 = list1;
                    DropDown_list1 obj8 = new DropDown_list1();
                    obj8.text = "Transportation";
                    obj8.value = "TC";
                    list1.Add(obj8);
                    VE.DropDown_list1 = list1;
                    DropDown_list1 obj9 = new DropDown_list1();
                    obj9.text = "Reverse Cal";
                    obj9.value = "RV";
                    list1.Add(obj9);
                    VE.DropDown_list1 = list1;

                    List<DropDown_list2> list2 = new List<DropDown_list2>();
                    DropDown_list2 obj21 = new DropDown_list2();
                    obj21.text = "Sales";
                    obj21.value = "S";
                    list2.Add(obj21);
                    DropDown_list2 obj22 = new DropDown_list2();
                    obj22.text = "Purchase";
                    obj22.value = "P";
                    list2.Add(obj22);
                    VE.DropDown_list2 = list2;
                    DropDown_list2 obj91 = new DropDown_list2();
                    obj91.text = "Transport";
                    obj91.value = "T";
                    list2.Add(obj91);
                    DropDown_list2 ddljs = new DropDown_list2();
                    ddljs.text = "Job Sales";
                    ddljs.value = "J";
                    list2.Add(ddljs);
                    DropDown_list2 ddljp = new DropDown_list2();
                    ddljp.text = "Job Purchase";
                    ddljp.value = "K";
                    list2.Add(ddljp);
                    VE.DropDown_list2 = list2;
                    DropDown_list2 ddlsb = new DropDown_list2();
                    ddlsb.text = "Shipping Bill";
                    ddlsb.value = "Z";
                    list2.Add(ddlsb);
                    VE.DropDown_list2 = list2;

                    List<DropDown_list3> list3 = new List<DropDown_list3>();
                    DropDown_list3 obj10 = new DropDown_list3();
                    obj10.text = "ADD";
                    obj10.value = "A";
                    list3.Add(obj10);
                    DropDown_list3 obj11 = new DropDown_list3();
                    obj11.text = "Less";
                    obj11.value = "L";
                    list3.Add(obj11);
                    VE.DropDown_list3 = list3;
                    VE.Database_Combo1 = (from n in DB.M_AMTTYPE
                                          select new Database_Combo1() { FIELD_VALUE = n.HSNCODE }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_AMTTYPE orderby p.AMTCD select new IndexKey() { Navikey = p.AMTCD }).ToList();
                        if (op == "E" || op == "D" || op == "V")
                        {

                            if (searchValue.Length != 0)
                            {
                                VE.Index = Nindex;
                                VE = Navigation(VE, DB, 0, searchValue);
                            }
                            else
                            {
                                if (key == "" || key == "F")
                                {
                                    VE.Index = 0;
                                    VE = Navigation(VE, DB, 0, searchValue);
                                }
                                else if (key == "L")
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
                            VE.M_AMTTYPE = sl;
                            VE.M_GENLEG = sGEN;
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
                AmountTypeMasterEntry VE = new AmountTypeMasterEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public AmountTypeMasterEntry Navigation(AmountTypeMasterEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            sl = new M_AMTTYPE(); sll = new M_CNTRL_HDR(); sGEN = new M_GENLEG();
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
                sl = DB.M_AMTTYPE.Find(aa[0].Trim());
                sGEN = DBF.M_GENLEG.Find(sl.GLCD);
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
                VE.TAXCODE = sl.TAXCODE;
                VE.SALPUR = sl.SALPUR;
                VE.CALCTYPE = sl.CALCTYPE;
                VE.ADDLESS = sl.ADDLESS;
                if (sll.INACTIVE_TAG == "Y")
                {
                    VE.Checked = true;
                }
                else
                {
                    VE.Checked = false;
                }
            }
            else
            {

            }
            return VE;
        }
        public ActionResult SearchPannelData()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            var MDT = (from j in DB.M_AMTTYPE
                       join p in DB.M_CNTRL_HDR on j.M_AUTONO equals (p.M_AUTONO)
                       where (p.M_AUTONO == j.M_AUTONO)
                       select new
                       {
                           AMTCD = j.AMTCD,
                           AMTNM = j.AMTNM
                       })
                       .Distinct().OrderBy(s => s.AMTCD).ToList();

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Amount Type Code" + Cn.GCS() + "Amount Type Name";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].AMTCD + "</td><td>" + MDT[j].AMTNM + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "0"));
        }
        //public ActionResult GetGeneralLedgerDetails(string val)
        //{
        //    try
        //    {
        //        if (val == null)
        //        {
        //            return PartialView("_Help2", Master_Help.GENERALLEDGER(val));
        //        }
        //        else
        //        {
        //            string str = Master_Help.GENERALLEDGER(val);
        //            return Content(str);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Cn.SaveException(ex, "");
        //        return Content(ex.Message + ex.InnerException);
        //    }
        //}
        public ActionResult GetGenLedgerDetails(string val)
        {
            var str = Master_Help.GLCD_help(val);
            if (str.IndexOf("='helpmnu'") >= 0)
            {
                return PartialView("_Help2", str);
            }
            else
            {
                return Content(str);
            }
        }
        public ActionResult SAVE(FormCollection FC, AmountTypeMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO) + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_AMTTYPE MTAXGRP = new M_AMTTYPE();
                        MTAXGRP.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            MTAXGRP.EMD_NO = 0;
                            MTAXGRP.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO));
                            var maxcd = DB.M_AMTTYPE.Select(a => a.AMTCD).Max();
                            if (maxcd == null)
                            {
                                string R = "0001";
                                MTAXGRP.AMTCD = R.ToString();
                            }
                            else
                            {
                                short hj = Convert.ToInt16(maxcd);
                                ++hj;
                                string ui = Convert.ToString(hj);
                                MTAXGRP.AMTCD = ui.PadLeft(4, '0');
                            }
                        }
                        else
                        {
                            MTAXGRP.AMTCD = VE.M_AMTTYPE.AMTCD;
                            MTAXGRP.M_AUTONO = VE.M_AMTTYPE.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_CNTRL_HDR where p.M_AUTONO == MTAXGRP.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MTAXGRP.EMD_NO = 0;
                            }
                            else
                            {
                                MTAXGRP.EMD_NO = Convert.ToInt16(MAXEMDNO + 1);
                            }
                        }
                        MTAXGRP.AMTNM = VE.M_AMTTYPE.AMTNM;
                        MTAXGRP.GLCD = VE.M_AMTTYPE.GLCD;
                        MTAXGRP.HSNCODE = VE.M_AMTTYPE.HSNCODE;
                        MTAXGRP.CALCCODE = VE.M_AMTTYPE.CALCCODE;
                        MTAXGRP.CALCTYPE = VE.CALCTYPE;
                        MTAXGRP.ADDLESS = VE.ADDLESS;
                        MTAXGRP.TAXCODE = VE.TAXCODE;
                        MTAXGRP.CALCFORMULA = VE.M_AMTTYPE.CALCFORMULA;
                        MTAXGRP.SALPUR = VE.SALPUR;
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_AMTTYPE", MTAXGRP.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO));
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_CNTRL_HDR.Add(MCH);
                            DB.SaveChanges();
                            DB.M_AMTTYPE.Add(MTAXGRP);
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MTAXGRP).State = System.Data.Entity.EntityState.Modified;
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
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_AMTTYPE", VE.M_AMTTYPE.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO));
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_AMTTYPE.Where(x => x.M_AUTONO == VE.M_AMTTYPE.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.M_AMTTYPE.RemoveRange(DB.M_AMTTYPE.Where(x => x.M_AUTONO == VE.M_AMTTYPE.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_AMTTYPE.M_AUTONO));
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
                    transaction.Rollback();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }
        }
    }
}


