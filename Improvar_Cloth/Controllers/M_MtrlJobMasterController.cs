﻿using System;
using System.Linq;
using System.Web.Mvc;                                                   
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;

namespace Improvar.Controllers
{
    public class M_MtrlJobMasterController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp Master_Help = new MasterHelp();
        MasterHelpFa Master_HelpFa = new MasterHelpFa();
        M_MTRLJOBMST sl;
        M_CNTRL_HDR sll;
        ImprovarDB DBF;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_JobMaster
        public ActionResult M_MtrlJobMaster(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Material Job Master";
                    JobMasterEntry VE = new JobMasterEntry();
                    string loca1 = CommVar.Loccd(UNQSNO).Trim();
                    ImprovarDB DBIMP = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    var doctP = (from i in DBIMP.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_MTRLJOBMST orderby p.MTRLJOBNM select new IndexKey() { Navikey = p.MTRLJOBCD }).ToList();
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

                            if (sll.INACTIVE_TAG == "Y")
                            {
                                VE.Deactive = true;
                            }
                            else
                            {
                                VE.Deactive = false;
                            }
                            VE.M_MTRLJOBMST = sl;
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
                JobMasterEntry VE = new JobMasterEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public JobMasterEntry Navigation(JobMasterEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new M_MTRLJOBMST();
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
                sl = DB.M_MTRLJOBMST.Find(aa[0].Trim());
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
            var MDT = (from j in DB.M_MTRLJOBMST
                       join p in DB.M_CNTRL_HDR on j.M_AUTONO 
                       equals (p.M_AUTONO)
                       where (p.M_AUTONO == j.M_AUTONO)
                       select new 
                       {
                           MTRLJOBCD = j.MTRLJOBCD,
                           MTRLJOBNM = j.MTRLJOBNM }).Distinct().OrderBy(s => s.MTRLJOBCD).ToList();

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Job Name" + Cn.GCS() + "Job Code";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].MTRLJOBNM + "</td><td>" + MDT[j].MTRLJOBCD + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "1"));
        }
        public ActionResult CheckJobCode(string val)
        {
            string VALUE = val.ToUpper();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_MTRLJOBMST where (c.MTRLJOBCD == VALUE) select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }
        }
        //public ActionResult CheckBatchInitial(string val, string jobcd)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //    if (val == null || val == "") return Content("0");

        //    var query = (from c in DB.M_MTRLJOBMST where (c.MBATCHNINI == val) select c);
        //    if (jobcd != "")
        //    {
        //        query = (from c in DB.M_MTRLJOBMST where (c.MBATCHNINI == val && c.MTRLJOBCD == jobcd) select c);
        //    }
        //    if (query.Any())
        //    {
        //        return Content("1");
        //    }
        //    else
        //    {
        //        return Content("0");
        //    }
        //}
        //public ActionResult GetGenLedger()
        //{
        //    ImprovarDB DBfin = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
        //    return PartialView("_Help2", Master_HelpFa.GLCD_help(DBfin));
        //}
        public ActionResult GetUOMDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.UOM_help(val));
                }
                else
                {
                    string str = Master_Help.UOM_help(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetPRODGRPDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.PRODGRPCD_help(val));
                }
                else
                {
                    string str = Master_Help.PRODGRPCD_help(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GenLedger(string val)
        {
            ImprovarDB DBfin = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            var query = (from c in DBfin.M_GENLEG where (c.GLCD == val) select c);
            if (query.Any())
            {
                string str = "";
                foreach (var i in query)
                {
                    str = i.GLCD + Cn.GCS() + i.GLNM;
                }
                return Content(str);
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult SAVE(FormCollection FC, JobMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_MTRLJOBMST MJBMST = new M_MTRLJOBMST();
                        MJBMST.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            MJBMST.EMD_NO = 0;
                            MJBMST.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());
                        }
                        else
                        {
                            MJBMST.M_AUTONO = VE.M_MTRLJOBMST.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_CNTRL_HDR where p.M_AUTONO == MJBMST.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MJBMST.EMD_NO = 0;
                            }
                            else
                            {
                                MJBMST.EMD_NO = Convert.ToInt16(MAXEMDNO + 1);
                            }
                            MJBMST.DTAG = "E";
                        }

                        MJBMST.MTRLJOBCD = VE.M_MTRLJOBMST.MTRLJOBCD.ToUpper();
                        MJBMST.MTRLJOBNM = VE.M_MTRLJOBMST.MTRLJOBNM;
                        //MJBMST.MBATCHNINI = VE.M_MTRLJOBMST.MBATCHNINI;
                        MJBMST.RMTRLJOBCD = VE.M_MTRLJOBMST.RMTRLJOBCD;
                        MJBMST.MTRLDESC = VE.M_MTRLJOBMST.MTRLDESC;

                        //Control header 
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_MTRLJOBMST", MJBMST.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO),VE.Audit_REM);
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_MTRLJOBMST.Add(MJBMST);
                            DB.M_CNTRL_HDR.Add(MCH);
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MJBMST).State = System.Data.Entity.EntityState.Modified;
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
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_MTRLJOBMST", VE.M_MTRLJOBMST.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(),VE.Audit_REM);
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_MTRLJOBMST.Where(x => x.M_AUTONO == VE.M_MTRLJOBMST.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.M_MTRLJOBMST.RemoveRange(DB.M_MTRLJOBMST.Where(x => x.M_AUTONO == VE.M_MTRLJOBMST.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_MTRLJOBMST.M_AUTONO));
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