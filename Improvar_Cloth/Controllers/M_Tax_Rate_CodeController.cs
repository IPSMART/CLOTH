using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;

namespace Improvar.Controllers
{
    public class M_Tax_Rate_CodeController : Controller
    {
        MasterHelpFa Master_HelpFa = new MasterHelpFa();
        string CS = null;
        Connection Cn = new Connection();
        M_TAXGRP sl; M_CNTRL_HDR sll;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_Tax_Rate_Code
        public ActionResult M_Tax_Rate_Code(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Tax Code Master";
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    TaxGroupEntry VE = new TaxGroupEntry();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                   
                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_TAXGRP orderby p.TAXGRPCD select new IndexKey() { Navikey = p.TAXGRPCD }).ToList();
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
                            VE.M_TAXGRP = sl;
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
                TaxGroupEntry VE = new TaxGroupEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public TaxGroupEntry Navigation(TaxGroupEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new M_TAXGRP(); sll = new M_CNTRL_HDR();
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
                sl = DB.M_TAXGRP.Find(aa[0].Trim());
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
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            var MDT = (from j in DBF.M_TAXGRP join p in DBF.M_CNTRL_HDR on j.M_AUTONO equals (p.M_AUTONO)
                       where (p.M_AUTONO == j.M_AUTONO)
                       select new {
                           TAXGRPCD = j.TAXGRPCD,
                           TAXGRPNM = j.TAXGRPNM }).Distinct().OrderBy(s => s.TAXGRPCD).ToList();

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Tax Group Code" + Cn.GCS() + "Tax Group Name";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].TAXGRPCD + "</td><td>" + MDT[j].TAXGRPNM + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_HelpFa.Generate_SearchPannel(hdr, SB.ToString(),"0"));
        }
        public ActionResult SAVE(FormCollection FC, TaxGroupEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.FinSchema(UNQSNO) + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_TAXGRP MTAXGRP = new M_TAXGRP();
                        MTAXGRP.CLCD = CommVar.ClientCode(UNQSNO);
                        string NAME = VE.M_TAXGRP.TAXGRPNM;
                        string NAME_CHAR = NAME.Substring(0, 1).ToUpper();
                        if (VE.DefaultAction == "A")
                        {
                            MTAXGRP.EMD_NO = 0;
                            MTAXGRP.M_AUTONO = Cn.M_AUTONO(CommVar.FinSchema(UNQSNO));

                            var MAXJOBCD = DB.M_TAXGRP.Where(a => a.TAXGRPCD.Substring(0, 1) == NAME_CHAR).Max(a => a.TAXGRPCD);
                            if (MAXJOBCD == null)
                            {
                                string R = NAME_CHAR + "001";
                                MTAXGRP.TAXGRPCD = R.ToString();
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
                                MTAXGRP.TAXGRPCD = newStr.ToString();
                            }
                        }
                        else
                        {
                            MTAXGRP.TAXGRPCD = VE.M_TAXGRP.TAXGRPCD;
                            MTAXGRP.M_AUTONO = VE.M_TAXGRP.M_AUTONO;
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
                        MTAXGRP.TAXGRPNM = VE.M_TAXGRP.TAXGRPNM;
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_TAXGRP", MTAXGRP.M_AUTONO.Value, VE.DefaultAction, CommVar.FinSchema(UNQSNO));
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_CNTRL_HDR.Add(MCH);
                            DB.SaveChanges();
                            DB.M_TAXGRP.Add(MTAXGRP);
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
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_TAXGRP", VE.M_TAXGRP.M_AUTONO.Value, VE.DefaultAction, CommVar.FinSchema(UNQSNO));
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_TAXGRP.Where(x => x.M_AUTONO == VE.M_TAXGRP.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.M_TAXGRP.RemoveRange(DB.M_TAXGRP.Where(x => x.M_AUTONO == VE.M_TAXGRP.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_TAXGRP.M_AUTONO));
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