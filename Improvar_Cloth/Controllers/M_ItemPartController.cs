using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;

namespace Improvar.Controllers
{
    public class M_ItemPartController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp Master_Help = new MasterHelp();
        M_PARTS sl;
        M_CNTRL_HDR sll;
        ImprovarDB DBF;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult M_ItemPart(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Part Master";
                    PartMasterEntry VE = new PartMasterEntry();
                    string loca1 = CommVar.Loccd(UNQSNO).Trim();
                    ImprovarDB DBIMP = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    var doctP = (from i in DBIMP.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_PARTS select new IndexKey() { Navikey = p.PARTCD }).OrderBy(a => a.Navikey).ToList();
                        if (op == "E" || op == "D" || op == "V")
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
                            if (sll.INACTIVE_TAG == "Y")
                            {
                                VE.Deactive = true;
                            }
                            else
                            {
                                VE.Deactive = false;
                            }
                            VE.M_PARTS = sl;
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
                PartMasterEntry VE = new PartMasterEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public PartMasterEntry Navigation(PartMasterEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new M_PARTS();
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
                sl = DB.M_PARTS.Find(aa[0].Trim());
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
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var MDT = (from j in DB.M_PARTS
                       join p in DB.M_CNTRL_HDR on j.M_AUTONO 
                       equals (p.M_AUTONO) where (p.M_AUTONO == j.M_AUTONO)
                       select new 
                       {
                           PARTNM = j.PARTNM,
                           PARTCD = j.PARTCD }).Distinct().OrderBy(s => s.PARTNM).ToList();

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Part Name" + Cn.GCS() + "Part Code";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].PARTNM + "</td><td>" + MDT[j].PARTCD + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "1"));
        }
        public ActionResult CheckPartCode(string val)
        {
            string VALUE = val.ToUpper();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_PARTS where (c.PARTCD == VALUE) select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }

        }
             
        public ActionResult SAVE(FormCollection FC, PartMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_PARTS MGOD = new M_PARTS();
                        MGOD.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            MGOD.EMD_NO = 0;
                            MGOD.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());
                        }
                        else
                        {
                            MGOD.M_AUTONO = VE.M_PARTS.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_CNTRL_HDR where p.M_AUTONO == MGOD.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MGOD.EMD_NO = 0;
                            }
                            else
                            {
                                MGOD.EMD_NO = Convert.ToByte(MAXEMDNO+1);
                            }
                        }

                        MGOD.PARTCD = VE.M_PARTS.PARTCD.ToUpper().Trim();
                        MGOD.PARTNM = VE.M_PARTS.PARTNM;
                        MGOD.PRTBARCODE = VE.M_PARTS.PRTBARCODE;
                        //Control header 
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_PARTS", MGOD.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_PARTS.Add(MGOD);
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
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_PARTS", VE.M_PARTS.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_PARTS.Where(x => x.M_AUTONO == VE.M_PARTS.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();
                        
                        DB.M_PARTS.RemoveRange(DB.M_PARTS.Where(x => x.M_AUTONO == VE.M_PARTS.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_PARTS.M_AUTONO));
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