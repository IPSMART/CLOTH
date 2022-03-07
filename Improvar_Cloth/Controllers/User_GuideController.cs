using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
namespace Improvar.Controllers
{
    public class User_GuideController : Controller
    {
        // GET: User_Guide
        Connection Cn = new Connection();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        public ActionResult User_Guide()
        {
            ViewBag.formname = "User Guide";
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            UserGuideImprovar VE = new UserGuideImprovar();
            if (Module.MODULE == "SALES")
            {
              VE.COMPNM = DB.SD_COMPANY.Where(X => X.COMPCD == CommVar.Compcd(UNQSNO)).Select(x => new 
                {
                    COMPNM = x.COMPNM
                }).ToList().FirstOrDefault().COMPNM;
            }
            //VE.USERGUIDE = (from g in DB.USER_GUIDE
            //                where g.MODULE_CODE == Module.Module_Code
            //                select new USERGUIDE()
            //                {
            //                    QUESTIONID = g.QUESTIONID,
            //                    QUESTIONBY = g.QUESTIONBY,
            //                    QUESTION = g.QUESTION,
            //                    ANSWERBY = g.ANSWERBY,
            //                    ANSWER = g.ANSWER,
            //                    STATUS = g.STATUS,
            //                }).ToList();

            //if (VE.USERGUIDE.Count == 0)
            //{
            //    List<USERGUIDE> ug = new List<USERGUIDE>();
            //    USERGUIDE usr = new USERGUIDE();
            //    usr.SLNO = 1;
            //    ug.Add(usr);
            //    VE.USERGUIDE = ug;
            //}
            //else {
            //    for (Int16 i = 0; i < VE.USERGUIDE.Count; ++i)
            //    {
            //        VE.USERGUIDE[i].SLNO = Convert.ToInt16(i + 1);
            //    }
            //}
            return View(VE);
        }

        public ActionResult SaveData(UserGuideImprovar VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    var MXUGD = (from p in DB.USER_GUIDE select p.QUESTIONID).Max();
                    string qid = "";
                    string qno = "";
                    if (MXUGD == null)
                    {
                        qid = "Q00001";
                        qno = Convert.ToString(Convert.ToInt32(qid.Substring(1, 5)) + 1);
                    }
                    else {
                        qno = Convert.ToString(Convert.ToInt32(MXUGD.Substring(1, 5)) + 1);
                        qid = "Q" + Convert.ToString(qno).PadLeft(5, '0');

                    }
                    DB.USER_GUIDE.RemoveRange(DB.USER_GUIDE.Where(x => x.MODULE_CODE == Module.Module_Code));

                    if (VE.USERGUIDE != null)
                    {
                        for (int i = 0; i <= VE.USERGUIDE.Count - 1; i++)
                        {
                            if (VE.USERGUIDE[i].SLNO >= 0 && VE.USERGUIDE[i].QUESTION != null)
                            {
                                USER_GUIDE UG = new USER_GUIDE();
                                if (VE.USERGUIDE[i].QUESTIONID == null)
                                {
                                    if (i == 0)
                                    {
                                        UG.QUESTIONID = qid;
                                    }
                                    else
                                    {
                                        UG.QUESTIONID = "Q" + Convert.ToString(qno).PadLeft(5, '0');
                                        qno = Convert.ToString(Convert.ToInt32(UG.QUESTIONID.Substring(1, 5)) + 1);
                                    }
                                }
                                else
                                {
                                    UG.QUESTIONID = VE.USERGUIDE[i].QUESTIONID;
                                    qno = Convert.ToString(Convert.ToInt32(UG.QUESTIONID.Substring(1, 5)) + 1);
                                }

                                if (VE.USERGUIDE[i].QUESTIONBY == null)
                                {
                                    UG.QUESTIONBY = Session["UR_ID"].ToString();
                                }
                                else
                                {

                                    UG.QUESTIONBY = VE.USERGUIDE[i].QUESTIONBY;
                                }
                                UG.QUESTION = VE.USERGUIDE[i].QUESTION;
                                UG.ANSWER = VE.USERGUIDE[i].ANSWER;
                                if (VE.USERGUIDE[i].ANSWER != null)
                                {
                                    if (VE.USERGUIDE[i].ANSWERBY == null)
                                    {
                                        UG.ANSWERBY = Session["UR_ID"].ToString();
                                    }
                                    else
                                    {

                                        UG.ANSWERBY = VE.USERGUIDE[i].ANSWERBY;
                                    }
                                }
                                UG.STATUS = VE.USERGUIDE[i].STATUS;
                                UG.MODULE_CODE = Module.Module_Code;
                                DB.USER_GUIDE.Add(UG);
                            }
                        }
                    }
                    DB.SaveChanges();
                    transaction.Commit();

                    return View("Data Saved !");
                }
                catch (Exception ex)
                {
                    Cn.SaveException(ex, "");
                    return Content(ex.ToString());
                }



            }
        }

        public ActionResult AddRowUG(UserGuideImprovar VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            if (VE.USERGUIDE == null)
            {
                List<USERGUIDE> UGL = new List<USERGUIDE>();
                USERGUIDE UGO = new USERGUIDE();
                UGO.SLNO = 1;
                UGL.Add(UGO);
                VE.USERGUIDE = UGL;
            }
            else
            {
                List<USERGUIDE> UGL = new List<USERGUIDE>();
                for (int i = 0; i <= VE.USERGUIDE.Count - 1; i++)
                {
                    USERGUIDE UGO = new USERGUIDE();
                    UGO = VE.USERGUIDE[i];
                    UGL.Add(UGO);
                }
                USERGUIDE UGO1 = new USERGUIDE();
                var max = VE.USERGUIDE.Max(a => Convert.ToByte(a.SLNO));
                UGO1.SLNO = Convert.ToInt16(Convert.ToInt16(max) + 1);
                UGL.Add(UGO1);
                VE.USERGUIDE = UGL;
            }
            return PartialView("_User_Guide", VE);
        }
        public ActionResult DeleteRowUG(UserGuideImprovar VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            List<USERGUIDE> UGL = new List<USERGUIDE>();
            int count = 0;
            for (int i = 0; i <= VE.USERGUIDE.Count - 1; i++)
            {
                if (VE.USERGUIDE[i].Checked == false)
                {
                    count += 1;
                    USERGUIDE UGO = new USERGUIDE();
                    UGO = VE.USERGUIDE[i];
                    UGO.SLNO = Convert.ToByte(count);
                    UGL.Add(UGO);
                }
            }
            VE.USERGUIDE = UGL;
            ModelState.Clear();
            return PartialView("_User_Guide", VE);

        }



    }
}