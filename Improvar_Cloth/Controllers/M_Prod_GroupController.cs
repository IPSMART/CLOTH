using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;

namespace Improvar.Controllers
{
    public class M_Prod_GroupController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        MasterHelpFa masterHelpFa = new MasterHelpFa();
        ImprovarDB DB, DBF, DBI;
        M_PRODGRP sl; M_CNTRL_HDR sll;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_Prod_Group
        public ActionResult M_Prod_Group(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Product Group (Tax/Disc) Master";
                    DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ProductGroupEntry VE = new ProductGroupEntry();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_PRODGRP orderby p.PRODGRPCD select new IndexKey() { Navikey = p.PRODGRPCD }).ToList();
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
                            VE.M_PRODGRP = sl;
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
                ProductGroupEntry VE = new ProductGroupEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public ProductGroupEntry Navigation(ProductGroupEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new M_PRODGRP(); sll = new M_CNTRL_HDR();
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
                sl = DB.M_PRODGRP.Find(aa[0].Trim());
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
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
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var MDT = (from j in DB.M_PRODGRP join p in DB.M_CNTRL_HDR on j.M_AUTONO equals (p.M_AUTONO)
                       where (p.M_AUTONO == j.M_AUTONO)
                       select new  {
                           PRODGRPCD = j.PRODGRPCD,
                           PRODGRPNM = j.PRODGRPNM }).Distinct().OrderBy(s => s.PRODGRPCD).ToList();

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Product Group Code" + Cn.GCS() + "Product Group Name";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr id='Srow_" + j.ToString() + "' onclick='SearchPannelRowClick(this.id)'><td>" + MDT[j].PRODGRPCD + "</td><td>" + MDT[j].PRODGRPNM + "</td></tr>");
            }
            return PartialView("_SearchPannel2", masterHelpFa.Generate_SearchPannel(hdr, SB.ToString(),"0"));
        }
        public ActionResult SAVE(FormCollection FC, ProductGroupEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_PRODGRP MTAXGRP = new M_PRODGRP();
                        MTAXGRP.CLCD = CommVar.ClientCode(UNQSNO);
                        string NAME = VE.M_PRODGRP.PRODGRPNM;
                        string NAME_CHAR = NAME.Substring(0, 1).ToUpper();
                        if (VE.DefaultAction == "A")
                        {
                            MTAXGRP.EMD_NO = 0;
                            MTAXGRP.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());

                            var MAXJOBCD = DB.M_PRODGRP.Where(a => a.PRODGRPCD.Substring(0, 1) == NAME_CHAR).Max(a => a.PRODGRPCD);
                            if (MAXJOBCD == null)
                            {
                                string R = NAME_CHAR + "001";
                                MTAXGRP.PRODGRPCD = R.ToString();
                            }
                            else
                            {
                                string s = MAXJOBCD;
                                string digits = new string(s.Where(char.IsDigit).ToArray());
                                string letters = new string(s.Where(char.IsLetter).ToArray());
                                int number;
                                if (!int.TryParse(digits, out number))                   //int.Parse would do the job since only digits are selected
                                {
                                    Console.WriteLine("Something weired happened");
                                }
                                string newStr = letters + (++number).ToString("D3");
                                MTAXGRP.PRODGRPCD = newStr.ToString();
                            }
                        }
                        else
                        {
                            MTAXGRP.PRODGRPCD = VE.M_PRODGRP.PRODGRPCD;
                            MTAXGRP.M_AUTONO = VE.M_PRODGRP.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_CNTRL_HDR where p.M_AUTONO == MTAXGRP.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MTAXGRP.EMD_NO = 0;
                            }
                            else
                            {
                                MTAXGRP.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                        }
                        MTAXGRP.PRODGRPNM = VE.M_PRODGRP.PRODGRPNM;
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_PRODGRP", MTAXGRP.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_CNTRL_HDR.Add(MCH);
                            DB.SaveChanges();
                            DB.M_PRODGRP.Add(MTAXGRP);
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
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_PRODGRP", VE.M_PRODGRP.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_PRODGRP.Where(x => x.M_AUTONO == VE.M_PRODGRP.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.M_PRODGRP.RemoveRange(DB.M_PRODGRP.Where(x => x.M_AUTONO == VE.M_PRODGRP.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_PRODGRP.M_AUTONO));
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