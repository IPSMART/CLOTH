using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Microsoft.Ajax.Utilities;

namespace Improvar.Controllers
{
    public class M_USR_ACS_GRPDTLController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelpFa masterHelpFa = new MasterHelpFa();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        ImprovarDB DB, DBF, DBI;
        M_USR_ACS_GRPDTL sl; M_CNTRL_HDR sll;USER_APPL sAPPL;
        // GET: M_USR_ACS_GRPDTL
        public ActionResult M_USR_ACS_GRPDTL(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "User With Group Link";
                    DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    UserGroupLinkDetails VE = new UserGroupLinkDetails(); Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    if (op.Length != 0)
                    {
                        string gcs = Cn.GCS();
                        VE.IndexKey = (from p in DB.M_USR_ACS_GRPDTL orderby p.LINKUSER_ID select new IndexKey() { Navikey = p.LINKUSER_ID }).Distinct().ToList();
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
                            VE.M_USR_ACS_GRPDTL = sl;
                            VE.USER_APPL = sAPPL;
                        }
                        else {

                            var USSR = (from p in DBI.USER_APPL
                                        select new
                                        {
                                            USER_ID = p.USER_ID,
                                            USER_NAME = p.USER_NAME
                                        }
                                ).ToList();
                            int i = 1;
                            List<MUSRACSGRPDTL> rt = new List<MUSRACSGRPDTL>();
                            foreach (var j in USSR)
                            {
                                MUSRACSGRPDTL useracs = new MUSRACSGRPDTL();
                                useracs.SLNO = i;
                                useracs.USER_ID = j.USER_ID;
                                useracs.USER_NAME = j.USER_NAME;
                                i++;
                                rt.Add(useracs);
                            }
                            VE.MUSRACSGRPDTL = rt;
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
                UserGroupLinkDetails VE = new UserGroupLinkDetails();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public UserGroupLinkDetails Navigation(UserGroupLinkDetails VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new M_USR_ACS_GRPDTL(); sll = new M_CNTRL_HDR(); sAPPL =new USER_APPL();
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
                var ty = aa[0];
                string USRID = "";
                if (VE.DefaultAction == "E")
                {
                    var USSR = (from p in DBI.USER_APPL
                                select new
                                {
                                    USER_ID = p.USER_ID,
                                    USER_NAME = p.USER_NAME
                                }
                                ).ToList();
                  
                  
                    var sd = (from i in DB.M_USR_ACS_GRPDTL
                              where (i.LINKUSER_ID == ty)
                              select new MUSRACSGRPDTL()
                              {
                                  LINKUSER_ID = i.LINKUSER_ID,
                                  USER_ID = i.USER_ID,
                                  M_AUTONO = i.M_AUTONO
                              }
                                        ).OrderBy(s => s.LINKUSER_ID).ToList();


                    List<MUSRACSGRPDTL> dtl = new List<MUSRACSGRPDTL>();
                    for (int i = 0; i < USSR.Count; i++)
                    {
                        MUSRACSGRPDTL acs = new Models.MUSRACSGRPDTL();
                        acs.SLNO = Convert.ToInt16(i + 1);
                        for (int j = 0; j < sd.Count; j++)
                        {
                            if (USSR[i].USER_ID == sd[j].USER_ID)
                            {
                                USRID = sd[j].USER_ID;
                                acs.Checked = true;
                                break;
                            }
                            else
                            {
                                acs.Checked = false;
                            }
                        }
                        acs.USER_ID = USSR[i].USER_ID;
                        acs.USER_NAME = USSR[i].USER_NAME;
                        dtl.Add(acs);
                    }
                    VE.MUSRACSGRPDTL = dtl;
                }
                else if(VE.DefaultAction=="V"){
                   VE.MUSRACSGRPDTL = (from i in DB.M_USR_ACS_GRPDTL
                              where (i.LINKUSER_ID == ty)
                              select new MUSRACSGRPDTL()
                              {
                                  LINKUSER_ID = i.LINKUSER_ID,
                                  USER_ID = i.USER_ID,
                                  M_AUTONO = i.M_AUTONO
                              }
                                      ).OrderBy(s => s.LINKUSER_ID).ToList();
                    int m = 1;
                    foreach(var v in VE.MUSRACSGRPDTL)
                    {
                        v.SLNO =Convert.ToInt16(m);
                        v.Checked = true;
                        v.USER_NAME = DBI.USER_APPL.Find(v.USER_ID)?.USER_NAME.ToString();
                        m++;
                    }
                    USRID = VE.MUSRACSGRPDTL[0].USER_ID;
                }
                sl = DB.M_USR_ACS_GRPDTL.Find(aa[0].Trim(),USRID );
                sAPPL = DBI.USER_APPL.Find(aa[0]);
            }
            else
            {

            }
            return VE;
        }
        public ActionResult SearchPannelData()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            var MDT = (from j in DB.M_USR_ACS_GRPDTL
                       join p in DB.M_CNTRL_HDR on j.M_AUTONO equals (p.M_AUTONO)
                       select new 
                       { LINKUSER_ID = j.LINKUSER_ID,
                           USER_ID = j.USER_ID }).DistinctBy(a => a.LINKUSER_ID).OrderBy(s => s.LINKUSER_ID).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Link Code" + Cn.GCS() + "User Id";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].LINKUSER_ID + "</td><td>" + MDT[j].USER_ID + "</td></tr>");
            }
            return PartialView("_SearchPannel2", masterHelpFa.Generate_SearchPannel(hdr, SB.ToString(), "0" + Cn.GCS() + "1", "1"));
        }
        public ActionResult GetLINKUSER_IDhelp(string val)
        {
            DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            if (val == null)
            {
                return PartialView("_Help2", masterHelpFa.USER_ID_HELP(val));
            }
            else
            {
                string str = masterHelpFa.USER_ID_HELP(val);
                return Content(str);
            }

        }
        public ActionResult SAVE(FormCollection FC, UserGroupLinkDetails VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO) + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_USR_ACS_GRPDTL USRGRPDTL = new M_USR_ACS_GRPDTL();
                        USRGRPDTL.CLCD = CommVar.ClientCode(UNQSNO);
                        USRGRPDTL.LINKUSER_ID = VE.M_USR_ACS_GRPDTL.LINKUSER_ID;
                        if (VE.DefaultAction == "A")
                        {
                            USRGRPDTL.EMD_NO = 0;
                            USRGRPDTL.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO));
                        }
                        else
                        {
                            USRGRPDTL.M_AUTONO = VE.M_USR_ACS_GRPDTL.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_CNTRL_HDR where p.M_AUTONO == USRGRPDTL.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                USRGRPDTL.EMD_NO = 0;
                            }
                            else
                            {
                                USRGRPDTL.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                            USRGRPDTL.DTAG = "E";
                            DB.M_USR_ACS_GRPDTL.Where(x => x.M_AUTONO == VE.M_USR_ACS_GRPDTL.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.SaveChanges();
                            DB.M_USR_ACS_GRPDTL.RemoveRange(DB.M_USR_ACS_GRPDTL.Where(x => x.M_AUTONO == VE.M_USR_ACS_GRPDTL.M_AUTONO));
                            DB.SaveChanges();
                        }

                        if (VE.MUSRACSGRPDTL != null)
                        {
                            for (int i = 0; i <= VE.MUSRACSGRPDTL.Count - 1; i++)
                            {
                                if (VE.MUSRACSGRPDTL[i].SLNO >= 0 && VE.MUSRACSGRPDTL[i].Checked == true && VE.MUSRACSGRPDTL[i].USER_ID != null)
                                {
                                    M_USR_ACS_GRPDTL acsdtl = new M_USR_ACS_GRPDTL();
                                    acsdtl.EMD_NO = USRGRPDTL.EMD_NO;
                                    acsdtl.DTAG = USRGRPDTL.DTAG;
                                    acsdtl.CLCD = USRGRPDTL.CLCD;
                                    acsdtl.M_AUTONO = USRGRPDTL.M_AUTONO;
                                    acsdtl.LINKUSER_ID = USRGRPDTL.LINKUSER_ID;
                                    acsdtl.USER_ID = VE.MUSRACSGRPDTL[i].USER_ID;
                                    DB.M_USR_ACS_GRPDTL.Add(acsdtl);
                                }
                            }
                        }
                        else
                        {
                            {
                                return Content("No Check Field Selected ");
                            }
                        }
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(false, "M_USR_ACS_GRPDTL", USRGRPDTL.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO),VE.Audit_REM);
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_CNTRL_HDR.Add(MCH);
                            DB.SaveChanges();
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            // DB.Entry(USRGRPDTL).State = System.Data.Entity.EntityState.Modified;
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
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(false, "M_USR_ACS_GRPDTL", VE.M_USR_ACS_GRPDTL.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO),VE.Audit_REM);
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_USR_ACS_GRPDTL.Where(x => x.M_AUTONO == VE.M_USR_ACS_GRPDTL.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_USR_ACS_GRPDTL.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.M_USR_ACS_GRPDTL.RemoveRange(DB.M_USR_ACS_GRPDTL.Where(x => x.M_AUTONO == VE.M_USR_ACS_GRPDTL.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_USR_ACS_GRPDTL.M_AUTONO));
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