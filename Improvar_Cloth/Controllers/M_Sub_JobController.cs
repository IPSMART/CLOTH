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
    public class M_Sub_JobController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp Master_Help = new MasterHelp();
        M_JOBMSTSUB sl;
        M_CNTRL_HDR sll;
        M_JOBMST sJob;
        ImprovarDB DBF;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult M_Sub_Job(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Sub Job Master";
                    Job_SubMasterEntry VE = new Job_SubMasterEntry();
                    string loca1 = CommVar.Loccd(UNQSNO).Trim();
                    ImprovarDB DBIMP = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    var doctP = (from i in DBIMP.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);

                    VE.SJOBCATEGORY = (from i in DB.M_JOBMSTSUB select new SJOBCATEGORY() { CATEGORY = i.SJOBSTYLE }).DistinctBy(a => a.CATEGORY).ToList();

                    VE.SJOBSUBCATEGORY = (from i in DB.M_JOBMSTSUB select new SJOBSUBCATEGORY() { SUBCATEGORY = i.SCATE }).DistinctBy(a => a.SUBCATEGORY).ToList();

                    VE.SJOBMACHINE = (from i in DB.M_JOBMSTSUB select new SJOBMACHINE() { MACHINE = i.SJOBMCHN }).DistinctBy(a => a.MACHINE).ToList();

                    VE.SJOBBATCH = (from i in DB.M_JOBMSTSUB select new SJOBBATCH() { BATCH = i.SJOBATCH }).DistinctBy(a => a.BATCH).ToList();

                    VE.SJOBSIZE = (from i in DB.M_JOBMSTSUB select new SJOBSIZE() { SIZE = i.SJOBSIZE }).DistinctBy(a => a.SIZE).ToList();

                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_JOBMSTSUB select new IndexKey() { Navikey = p.SJOBCD }).OrderBy(a => a.Navikey).ToList();
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
                                else if (key == "L")
                                {
                                    VE.Index = VE.IndexKey.Count - 1;
                                    VE = Navigation(VE, DB, VE.IndexKey.Count - 1, searchValue);
                                }
                                else if (key == "")
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
                            VE.M_JOBMSTSUB = sl;
                            VE.M_JOBMST = sJob;
                            VE.M_CNTRL_HDR = sll;

                            if (op.ToString() == "A")
                            {
                                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                                UploadDOC UPL = new UploadDOC();
                                UPL.DocumentType = doctP;
                                UploadDOC1.Add(UPL);
                                VE.UploadDOC = UploadDOC1;
                            }
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
                Job_SubMasterEntry VE = new Job_SubMasterEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public Job_SubMasterEntry Navigation(Job_SubMasterEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new M_JOBMSTSUB();
            sll = new M_CNTRL_HDR();
            sJob = new M_JOBMST();
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
                sl = DB.M_JOBMSTSUB.Find(aa[0].Trim());
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
                sJob = DB.M_JOBMST.Find(sl.JOBCD);
                if (sll.INACTIVE_TAG == "Y")
                {
                    VE.Deactive = true;
                }
                else
                {
                    VE.Deactive = false;
                }
                VE.UploadDOC = Cn.GetUploadImage(CommVar.CurSchema(UNQSNO).ToString(), Convert.ToInt32(sl.M_AUTONO));

            }

            return VE;
        }
        public ActionResult SearchPannelData()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var MDT = (from j in DB.M_JOBMSTSUB
                       join p in DB.M_CNTRL_HDR on j.M_AUTONO equals (p.M_AUTONO)
                       join n in DB.M_JOBMST on j.JOBCD equals (n.JOBCD)
                       where (p.M_AUTONO == j.M_AUTONO)
                       select new 
                       {
                           SJOBCD = j.SJOBCD,
                           SJOBNM = j.SJOBNM,
                           JOBNM = n.JOBNM }).Distinct().OrderBy(s => s.SJOBCD).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Sub Job Name" + Cn.GCS() + "Sub Job Code" + Cn.GCS() + "Job Name";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].SJOBNM + "</td><td>" + MDT[j].SJOBCD + "</td><td>" + MDT[j].JOBNM + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "1"));
        }
        public ActionResult CheckJobSubCode(string val)
        {
            string VALUE = val.ToUpper();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_JOBMSTSUB where (c.SJOBCD == VALUE) select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }

        }
        public ActionResult GetJobMaster()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            return PartialView("_Help2", Master_Help.JOBCD_help(DB));
        }
        public ActionResult JobMaster(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_JOBMST where (c.JOBCD == val) select c);
            if (query.Any())
            {
                string str = "";
                foreach (var i in query)
                {
                    str = i.JOBCD + Cn.GCS() + i.JOBNM;
                }
                return Content(str);
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult AddDOCRow(ColorEntry VE)
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
        public ActionResult DeleteDOCRow(ColorEntry VE)
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
        public ActionResult SAVE(FormCollection FC, Job_SubMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_JOBMSTSUB MJMTSUB = new M_JOBMSTSUB();
                        MJMTSUB.CLCD = CommVar.ClientCode(UNQSNO);
                        string txtSJOBNM = VE.M_JOBMSTSUB.SJOBNM;
                        string txtst = txtSJOBNM.Substring(0, 1).ToUpper();
                        if (VE.DefaultAction == "A")
                        {
                            MJMTSUB.EMD_NO = 0;
                            MJMTSUB.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());

                            var MAXJOBCD = DB.M_JOBMSTSUB.Where(a => a.SJOBCD.Substring(0, 1) == txtst).Max(a => a.SJOBCD);

                            if (MAXJOBCD == null)
                            {
                                string R = txtst + "0001";
                                MJMTSUB.SJOBCD = R.ToString();
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
                                string newStr = letters + (++number).ToString("D4");
                                MJMTSUB.SJOBCD = newStr.ToString();
                            }
                        }
                        else
                        {
                            MJMTSUB.M_AUTONO = VE.M_JOBMSTSUB.M_AUTONO;
                            MJMTSUB.SJOBCD = VE.M_JOBMSTSUB.SJOBCD;
                            var MAXEMDNO = (from p in DB.M_CNTRL_HDR where p.M_AUTONO == MJMTSUB.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MJMTSUB.EMD_NO = 0;
                            }
                            else
                            {
                                MJMTSUB.EMD_NO = Convert.ToInt16(MAXEMDNO + 1);
                            }
                            MJMTSUB.DTAG = "E";
                        }
                        MJMTSUB.SJOBNM = VE.M_JOBMSTSUB.SJOBNM;
                        MJMTSUB.JOBCD = VE.M_JOBMSTSUB.JOBCD;
                        MJMTSUB.SJOBSTYLE = VE.M_JOBMSTSUB.SJOBSTYLE;
                        MJMTSUB.SJOBMCHN = VE.M_JOBMSTSUB.SJOBMCHN;
                        MJMTSUB.SCATE = VE.M_JOBMSTSUB.SCATE;
                        MJMTSUB.SJOBSIZE = VE.M_JOBMSTSUB.SJOBSIZE;
                        MJMTSUB.SJOBATCH = VE.M_JOBMSTSUB.SJOBATCH;
                        MJMTSUB.SJOBSAM = VE.M_JOBMSTSUB.SJOBSAM;
                        //Control header 
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_JOBMSTSUB", MJMTSUB.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(),VE.Audit_REM);
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_JOBMSTSUB.Add(MJMTSUB);
                            DB.M_CNTRL_HDR.Add(MCH);
                        }
                        else if (VE.DefaultAction == "E")
                        {

                            DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_JOBMSTSUB.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_JOBMSTSUB.M_AUTONO));

                            DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_JOBMSTSUB.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_JOBMSTSUB.M_AUTONO));
                        }
                        if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MJMTSUB).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        }
                        if (VE.UploadDOC != null)
                        {
                            var img = Cn.SaveUploadImage("M_JOBMSTSUB", VE.UploadDOC, MJMTSUB.M_AUTONO, MJMTSUB.EMD_NO.Value);
                            DB.M_CNTRL_HDR_DOC.AddRange(img.Item1);
                            DB.M_CNTRL_HDR_DOC_DTL.AddRange(img.Item2);
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
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_JOBMSTSUB", VE.M_JOBMSTSUB.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(),VE.Audit_REM);
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_JOBMSTSUB.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_JOBMSTSUB.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_JOBMSTSUB.Where(x => x.M_AUTONO == VE.M_JOBMSTSUB.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_JOBMSTSUB.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_JOBMSTSUB.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_JOBMSTSUB.RemoveRange(DB.M_JOBMSTSUB.Where(x => x.M_AUTONO == VE.M_JOBMSTSUB.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_JOBMSTSUB.M_AUTONO));
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