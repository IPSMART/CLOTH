using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;

namespace Improvar.Controllers
{
    public class M_GodownController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp Master_Help = new MasterHelp();
        MasterHelpFa Master_HelpFa = new MasterHelpFa();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        M_GODOWN sl;
        M_CNTRL_HDR sll;
        // GET: M_Godown
        public ActionResult M_Godown(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Godown Master";
                    GodownMasterEntry VE = new GodownMasterEntry();
                    string loca1 = CommVar.Loccd(UNQSNO).Trim();
                    ImprovarDB DBIMP = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    var doctP = (from i in DBIMP.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    //=========For Company / Location Name===========//
                    VE.CompanyLocationName = (from i in DBF.M_LOCA
                                              join l in DBF.M_COMP on i.COMPCD equals (l.COMPCD)
                                              select new CompanyLocationName()
                                              {
                                                COMPCD = l.COMPCD,
                                                COMPNM = l.COMPNM,
                                                LOCCD = i.LOCCD,
                                                LOCNM = i.LOCNM,
                                              }).OrderBy(s => s.LOCNM).ToList();

                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_GODOWN orderby p.M_AUTONO select new IndexKey() { Navikey = p.GOCD }).ToList();
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
                            VE.M_GODOWN = sl;
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
                GodownMasterEntry VE = new GodownMasterEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public GodownMasterEntry Navigation(GodownMasterEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new M_GODOWN();
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
                sl = DB.M_GODOWN.Find(aa[0].Trim());
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);

                //company view
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

                var ss = (from p in DB.M_CNTRL_LOCA where (p.M_AUTONO == sl.M_AUTONO) select new CompanyLocationName() { COMPCD = p.COMPCD, LOCCD = p.LOCCD, Checked = true }).ToList();
                foreach (var a in ss)
                {
                    var comp = DBF.M_COMP.Find(a.COMPCD);
                    var loc = (from p in DBF.M_LOCA where p.LOCCD == a.LOCCD && p.COMPCD == a.COMPCD select p).SingleOrDefault();
                    a.COMPNM = comp.COMPNM;
                    a.LOCNM = loc.LOCNM;
                }
                foreach (var i in VE.CompanyLocationName)
                {
                    foreach (var x in ss)
                    {
                        if (i.COMPCD == x.COMPCD && i.LOCCD == x.LOCCD)
                        {
                            i.Checked = true;
                        }
                    }
                }
                //END COMP
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
            var MDT = (from j in DB.M_GODOWN
                       join p in DB.M_CNTRL_HDR on j.M_AUTONO equals (p.M_AUTONO)
                       where (p.M_AUTONO == j.M_AUTONO)
                       select new{
                           GOCD = j.GOCD,
                           GONM = j.GONM }).Distinct().OrderBy(s => s.GOCD).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Godown Code" + Cn.GCS() + "Godown Name";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr>");
                SB.Append("<td>" + MDT[j].GOCD + "</td><td>" + MDT[j].GONM + "</td>");
                SB.Append("</tr>");
            }
            return PartialView("_SearchPannel2", Master_HelpFa.Generate_SearchPannel(hdr, SB.ToString(),"0"));
        }
        public ActionResult CheckGodownCode(string val)
        {
            string VALUE = val.ToUpper();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_GODOWN where (c.GOCD == VALUE) select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }
        }

        public ActionResult SAVE(FormCollection FC, GodownMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_GODOWN MGOD = new M_GODOWN();
                        MGOD.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            MGOD.EMD_NO = 0;
                            MGOD.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());
                        }
                        else
                        {
                            MGOD.M_AUTONO = VE.M_GODOWN.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_CNTRL_HDR where p.M_AUTONO == MGOD.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MGOD.EMD_NO = 0;
                            }
                            else
                            {
                                MGOD.EMD_NO = Convert.ToInt16(MAXEMDNO + 1);
                            }
                        }

                        MGOD.GOCD = VE.M_GODOWN.GOCD.ToUpper().Trim();
                        MGOD.GONM = VE.M_GODOWN.GONM;
                        MGOD.GOADD1 = VE.M_GODOWN.GOADD1;
                        MGOD.GOADD2 = VE.M_GODOWN.GOADD2;
                        MGOD.GOADD3 = VE.M_GODOWN.GOADD3;
                        MGOD.DISTRICT = VE.M_GODOWN.DISTRICT;
                        MGOD.PIN = VE.M_GODOWN.PIN;
                        MGOD.GOPHNO = VE.M_GODOWN.GOPHNO;
                        MGOD.GOEMAIL = VE.M_GODOWN.GOEMAIL;
                        MGOD.FSSAILICNO = VE.M_GODOWN.FSSAILICNO;
                        //company saving    
                        if (VE.DefaultAction == "E")
                        {
                            DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == MGOD.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_LOCA.RemoveRange(DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == MGOD.M_AUTONO));
                        }
                        if (VE.CompanyLocationName != null)
                        {
                            for (int i = 0; i <= VE.CompanyLocationName.Count - 1; i++)
                            {
                                if (VE.CompanyLocationName[i].Checked)
                                {
                                    M_CNTRL_LOCA MCL = new M_CNTRL_LOCA();
                                    MCL.M_AUTONO = MGOD.M_AUTONO;
                                    MCL.COMPCD = VE.CompanyLocationName[i].COMPCD;
                                    MCL.LOCCD = VE.CompanyLocationName[i].LOCCD;
                                    MCL.CLCD = CommVar.ClientCode(UNQSNO);
                                    DB.M_CNTRL_LOCA.Add(MCL);
                                }
                            }
                        }

                        //Control header 
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_GODOWN", MGOD.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_GODOWN.Add(MGOD);
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
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_GODOWN", VE.M_GODOWN.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_GODOWN.Where(x => x.M_AUTONO == VE.M_GODOWN.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == VE.M_GODOWN.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.M_CNTRL_LOCA.RemoveRange(DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == VE.M_GODOWN.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_GODOWN.RemoveRange(DB.M_GODOWN.Where(x => x.M_AUTONO == VE.M_GODOWN.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_GODOWN.M_AUTONO));
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