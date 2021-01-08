using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class DOCPASS_STRGYController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        ImprovarDB DB, DBI, DBINV, DBFIN;
        M_DOC_AUTH sl;
        M_CNTRL_HDR sll;

        // GET: DOCPASS_STRGY

        public ActionResult DOCPASS_STRGY(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Document Authorization Rule";
                    DocumentAuthorizationEntry VE = new DocumentAuthorizationEntry(); Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    string SCHEMA = Cn.Getschema;
                    DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(VE.UNQSNO));
                    DBFIN = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(VE.UNQSNO));
                    DBI = new ImprovarDB(Cn.GetConnectionString(), SCHEMA);
                    VE.CompanyLocationName = (from i in DBFIN.M_LOCA
                                              join l in DBFIN.M_COMP on i.COMPCD equals (l.COMPCD)
                                              select new CompanyLocationName() { COMPCD = l.COMPCD, COMPNM = l.COMPNM, LOCCD = i.LOCCD, LOCNM = i.LOCNM }).OrderBy(s => s.LOCNM).ToList();

                    VE.DocumentType = (from i in DB.M_DOCTYPE select new DocumentType() { value = i.DOCCD, text = i.DOCNM }).OrderBy(s => s.text).ToList();
                    if (op.Length != 0)
                    {
                        string GCS = Cn.GCS();
                        var data = (from p in DB.M_DOC_AUTH
                                    orderby p.DOCCD
                                    select new
                                    {
                                        DOCCD = p.DOCCD,
                                        EFF_DT = p.EFF_DT
                                    }).Distinct().ToList();
                        VE.IndexKey = (from p in data
                                       orderby p.DOCCD
                                       select new IndexKey()
                                       {
                                           Navikey = p.DOCCD + GCS + p.EFF_DT
                                       }).Distinct().ToList();


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
                                else if (key == "L" || key == "")
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
                                VE.Checked = true;
                            }
                            else
                            {
                                VE.Checked = false;
                            }
                            VE.M_DOC_AUTH = sl;
                            VE.M_CNTRL_HDR = sll;
                        }
                        else
                        {
                            VE.CompanyLocationName = (from i in DBFIN.M_LOCA join l in DBFIN.M_COMP on i.COMPCD equals (l.COMPCD) select new CompanyLocationName() { COMPCD = l.COMPCD, COMPNM = l.COMPNM, LOCCD = i.LOCCD, LOCNM = i.LOCNM }).OrderBy(s => s.LOCNM).ToList();

                            List<MDOCAUTHLAVEL1> DOCAUTHLEVEL11 = new List<MDOCAUTHLAVEL1>();
                            MDOCAUTHLAVEL1 MISLEVEL1 = new MDOCAUTHLAVEL1();
                            MISLEVEL1.SLNO = 1;
                            DOCAUTHLEVEL11.Add(MISLEVEL1);
                            VE.MDOCAUTHLEVEL1 = DOCAUTHLEVEL11;

                            List<MDOCAUTHLAVEL2> DOCAUTHLEVEL22 = new List<MDOCAUTHLAVEL2>();
                            MDOCAUTHLAVEL2 MISLEVEL2 = new MDOCAUTHLAVEL2();
                            MISLEVEL2.SLNO = 1;
                            DOCAUTHLEVEL22.Add(MISLEVEL2);
                            VE.MDOCAUTHLEVEL2 = DOCAUTHLEVEL22;


                            List<MDOCAUTHLAVEL3> DOCAUTHLEVEL33 = new List<MDOCAUTHLAVEL3>();
                            MDOCAUTHLAVEL3 MISLEVEL3 = new MDOCAUTHLAVEL3();
                            MISLEVEL3.SLNO = 1;
                            DOCAUTHLEVEL33.Add(MISLEVEL3);
                            VE.MDOCAUTHLEVEL3 = DOCAUTHLEVEL33;



                            List<MDOCAUTHLAVEL4> DOCAUTHLEVEL44 = new List<MDOCAUTHLAVEL4>();
                            MDOCAUTHLAVEL4 MISLEVEL4 = new MDOCAUTHLAVEL4();
                            MISLEVEL4.SLNO = 1;
                            DOCAUTHLEVEL44.Add(MISLEVEL4);
                            VE.MDOCAUTHLEVEL4 = DOCAUTHLEVEL44;


                            List<MDOCAUTHLAVEL5> DOCAUTHLEVEL55 = new List<MDOCAUTHLAVEL5>();
                            MDOCAUTHLAVEL5 MISLEVEL5 = new MDOCAUTHLAVEL5();
                            MISLEVEL5.SLNO = 1;
                            DOCAUTHLEVEL55.Add(MISLEVEL5);
                            VE.MDOCAUTHLEVEL5 = DOCAUTHLEVEL55;
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
                DocumentAuthorizationEntry VE = new DocumentAuthorizationEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public DocumentAuthorizationEntry Navigation(DocumentAuthorizationEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            DBFIN = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(VE.UNQSNO));

            sl = new M_DOC_AUTH(); sll = new M_CNTRL_HDR();
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
                //var lvl = Convert.ToByte(aa[1].Trim());
                var date = Convert.ToDateTime(aa[1].Trim());
                //var slno = Convert.ToInt16(aa[3].Trim());
                string doccd = aa[0].Trim();
                sl = (from A in DB.M_DOC_AUTH where A.DOCCD == doccd && A.EFF_DT == date select A).FirstOrDefault();
                //sl = DB.M_DOC_AUTH.Find(aa[0].Trim(), lvl, date, slno);
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
                if (sll.INACTIVE_TAG == "Y")
                {
                    VE.Checked = true;
                }
                else
                {
                    VE.Checked = false;
                }

                var CLN = (from p in DBFIN.M_CNTRL_LOCA
                           join s in DBFIN.M_COMP on p.COMPCD equals (s.COMPCD)
                           join x in DBFIN.M_LOCA on p.LOCCD equals (x.LOCCD)
                           where (p.M_AUTONO == sl.M_AUTONO)
                           select new CompanyLocationName() { COMPCD = p.COMPCD, COMPNM = s.COMPNM, LOCCD = p.LOCCD, LOCNM = x.LOCNM, Checked = true }).ToList();
                foreach (var i in VE.CompanyLocationName)
                {
                    foreach (var x in CLN)
                    {
                        if (i.COMPCD == x.COMPCD && i.LOCCD == x.LOCCD)
                        {
                            i.Checked = true;
                        }
                    }
                }


                VE.MDOCAUTHLEVEL1 = (from i in DB.M_DOC_AUTH
                                     where (i.DOCCD == sl.DOCCD && i.LVL == 1 && i.M_AUTONO == sl.M_AUTONO)
                                     select new MDOCAUTHLAVEL1()
                                     {
                                         AUTHCD = i.AUTHCD,
                                         SLNO = i.SLNO,
                                         EFF_DT = i.EFF_DT,
                                         AMT_FR = i.AMT_FR,
                                         AMT_TO = i.AMT_TO
                                     }).OrderBy(s => s.SLNO).ToList();
                foreach (var i in VE.MDOCAUTHLEVEL1)
                {
                    if (i.AUTHCD != null)
                    {
                        var temp1 = DBFIN.M_SIGN_AUTH.Find(i.AUTHCD);
                        i.AUTHNM = temp1.AUTHNM;
                    }
                }

                //level2                 
                VE.MDOCAUTHLEVEL2 = (from i in DB.M_DOC_AUTH
                                     where (i.DOCCD == sl.DOCCD && i.LVL == 2 && i.M_AUTONO == sl.M_AUTONO)
                                     select new MDOCAUTHLAVEL2()
                                     {
                                         AUTHCD = i.AUTHCD,

                                         SLNO = i.SLNO,
                                         EFF_DT = i.EFF_DT,
                                         AMT_FR = i.AMT_FR,
                                         AMT_TO = i.AMT_TO
                                     }).OrderBy(s => s.SLNO).ToList();
                foreach (var i in VE.MDOCAUTHLEVEL2)
                {
                    if (i.AUTHCD != null)
                    {
                        var temp1 = DBFIN.M_SIGN_AUTH.Find(i.AUTHCD);
                        i.AUTHNM = temp1.AUTHNM;
                    }
                }

                //level3              
                VE.MDOCAUTHLEVEL3 = (from i in DB.M_DOC_AUTH
                                     where (i.DOCCD == sl.DOCCD && i.LVL == 3 && i.M_AUTONO == sl.M_AUTONO)
                                     select new MDOCAUTHLAVEL3()
                                     {
                                         AUTHCD = i.AUTHCD,

                                         SLNO = i.SLNO,
                                         EFF_DT = i.EFF_DT,
                                         AMT_FR = i.AMT_FR,
                                         AMT_TO = i.AMT_TO
                                     }).OrderBy(s => s.SLNO).ToList();
                foreach (var i in VE.MDOCAUTHLEVEL3)
                {
                    if (i.AUTHCD != null)
                    {
                        var temp1 = DBFIN.M_SIGN_AUTH.Find(i.AUTHCD);
                        i.AUTHNM = temp1.AUTHNM;
                    }
                }

                //level4            
                VE.MDOCAUTHLEVEL4 = (from i in DB.M_DOC_AUTH
                                     where (i.DOCCD == sl.DOCCD && i.LVL == 4 && i.M_AUTONO == sl.M_AUTONO)
                                     select new MDOCAUTHLAVEL4()
                                     {
                                         AUTHCD = i.AUTHCD,

                                         SLNO = i.SLNO,
                                         EFF_DT = i.EFF_DT,
                                         AMT_FR = i.AMT_FR,
                                         AMT_TO = i.AMT_TO
                                     }).OrderBy(s => s.SLNO).ToList();
                foreach (var i in VE.MDOCAUTHLEVEL4)
                {
                    if (i.AUTHCD != null)
                    {
                        var temp1 = DBFIN.M_SIGN_AUTH.Find(i.AUTHCD);
                        i.AUTHNM = temp1.AUTHNM;
                    }
                }

                //level3

                VE.MDOCAUTHLEVEL5 = (from i in DB.M_DOC_AUTH
                                     where (i.DOCCD == sl.DOCCD && i.LVL == 5 && i.M_AUTONO == sl.M_AUTONO)
                                     select new MDOCAUTHLAVEL5()
                                     {
                                         AUTHCD = i.AUTHCD,

                                         SLNO = i.SLNO,
                                         EFF_DT = i.EFF_DT,
                                         AMT_FR = i.AMT_FR,
                                         AMT_TO = i.AMT_TO
                                     }).OrderBy(s => s.SLNO).ToList();
                foreach (var i in VE.MDOCAUTHLEVEL5)
                {
                    if (i.AUTHCD != null)
                    {
                        var temp1 = DBFIN.M_SIGN_AUTH.Find(i.AUTHCD);
                        i.AUTHNM = (string)temp1.AUTHNM;
                    }
                }
            }
            return VE;
        }
        public ActionResult SearchPannelData()
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            var MDT = (from j in DB.M_DOC_AUTH
                       join o in DB.M_CNTRL_HDR on j.M_AUTONO equals (o.M_AUTONO)
                       join p in DB.M_DOCTYPE on j.DOCCD equals (p.DOCCD)
                       where (o.M_AUTONO == j.M_AUTONO)
                       select new
                       {
                           DOCCD = j.DOCCD,
                           EFF_DT = j.EFF_DT,
                           DOCNM = p.DOCNM
                       }).OrderBy(s => s.DOCCD).Distinct().ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Document Code" + Cn.GCS() + "Document Name" + Cn.GCS() + "Effective Date";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].DOCCD + "</td><td>" + MDT[j].DOCNM + "</td><td>" + MDT[j].EFF_DT.ToString().Remove(10) + "</td></tr>");
            }
            return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "0" + Cn.GCS() + "2"));
        }
        public ActionResult GetAUTHCDhelp(string val)
        {
            try
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                DBFIN = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.AUTHCD_help(DBFIN));
                }
                else {
                    var query = (from c in DBFIN.M_SIGN_AUTH where (c.AUTHCD == val) select c);
                    if (query.Any())
                    {
                        string str = "";
                        foreach (var i in query)
                        {
                            str = i.AUTHCD + Cn.GCS() + i.AUTHNM;
                        }
                        return Content(str);
                    }
                    else
                    {
                        return Content("0");
                    }
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public ActionResult AddRowLEVEL1(DocumentAuthorizationEntry VE)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            List<MDOCAUTHLAVEL1> DOCAUTH = new List<MDOCAUTHLAVEL1>();
            for (int i = 0; i <= VE.MDOCAUTHLEVEL1.Count - 1; i++)
            {
                MDOCAUTHLAVEL1 MIB = new MDOCAUTHLAVEL1();
                MIB = VE.MDOCAUTHLEVEL1[i];
                DOCAUTH.Add(MIB);
            }
            MDOCAUTHLAVEL1 DOCAUTH1 = new MDOCAUTHLAVEL1();
            var max = VE.MDOCAUTHLEVEL1.Max(a => Convert.ToInt32(a.SLNO));
            int SLNO = Convert.ToInt32(max) + 1;
            DOCAUTH1.SLNO = Convert.ToSByte(SLNO);
            DOCAUTH.Add(DOCAUTH1);
            VE.MDOCAUTHLEVEL1 = DOCAUTH;

            VE.DefaultView = true;
            return PartialView("_DOCPASS_STRGY_LEVEL1", VE);

        }
        public ActionResult DeleteRowLEVEL1(DocumentAuthorizationEntry VE)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            List<MDOCAUTHLAVEL1> DOCAUTH = new List<MDOCAUTHLAVEL1>();
            int count = 0;
            for (int i = 0; i <= VE.MDOCAUTHLEVEL1.Count - 1; i++)
            {
                if (VE.MDOCAUTHLEVEL1[i].Checked == false)
                {
                    count += 1;
                    MDOCAUTHLAVEL1 item = new MDOCAUTHLAVEL1();
                    item = VE.MDOCAUTHLEVEL1[i];
                    item.SLNO = Convert.ToSByte(count);
                    DOCAUTH.Add(item);
                }
            }
            VE.MDOCAUTHLEVEL1 = DOCAUTH;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_DOCPASS_STRGY_LEVEL1", VE);

        }
        public ActionResult AddRowLEVEL2(DocumentAuthorizationEntry VE)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));

            List<MDOCAUTHLAVEL2> DOCAUTH = new List<MDOCAUTHLAVEL2>();
            for (int i = 0; i <= VE.MDOCAUTHLEVEL2.Count - 1; i++)
            {
                MDOCAUTHLAVEL2 MIB = new MDOCAUTHLAVEL2();
                MIB = VE.MDOCAUTHLEVEL2[i];
                DOCAUTH.Add(MIB);
            }
            MDOCAUTHLAVEL2 DOCAUTH1 = new MDOCAUTHLAVEL2();
            var max = VE.MDOCAUTHLEVEL2.Max(a => Convert.ToInt32(a.SLNO));
            int SLNO = Convert.ToInt32(max) + 1;
            DOCAUTH1.SLNO = Convert.ToSByte(SLNO);
            DOCAUTH.Add(DOCAUTH1);
            VE.MDOCAUTHLEVEL2 = DOCAUTH;

            VE.DefaultView = true;
            return PartialView("_DOCPASS_STRGY_LEVEL2", VE);

        }
        public ActionResult DeleteRowLEVEL2(DocumentAuthorizationEntry VE)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            List<MDOCAUTHLAVEL2> DOCAUTH = new List<MDOCAUTHLAVEL2>();
            int count = 0;
            for (int i = 0; i <= VE.MDOCAUTHLEVEL2.Count - 1; i++)
            {
                if (VE.MDOCAUTHLEVEL2[i].Checked == false)
                {
                    count += 1;
                    MDOCAUTHLAVEL2 item = new MDOCAUTHLAVEL2();
                    item = VE.MDOCAUTHLEVEL2[i];
                    item.SLNO = Convert.ToSByte(count);
                    DOCAUTH.Add(item);
                }
            }
            VE.MDOCAUTHLEVEL2 = DOCAUTH;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_DOCPASS_STRGY_LEVEL2", VE);

        }
        public ActionResult AddRowLEVEL3(DocumentAuthorizationEntry VE)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));

            List<MDOCAUTHLAVEL3> DOCAUTH = new List<MDOCAUTHLAVEL3>();
            for (int i = 0; i <= VE.MDOCAUTHLEVEL3.Count - 1; i++)
            {
                MDOCAUTHLAVEL3 MIB = new MDOCAUTHLAVEL3();
                MIB = VE.MDOCAUTHLEVEL3[i];
                DOCAUTH.Add(MIB);
            }
            MDOCAUTHLAVEL3 DOCAUTH1 = new MDOCAUTHLAVEL3();
            var max = VE.MDOCAUTHLEVEL3.Max(a => Convert.ToInt32(a.SLNO));
            int SLNO = Convert.ToInt32(max) + 1;
            DOCAUTH1.SLNO = Convert.ToSByte(SLNO);
            DOCAUTH.Add(DOCAUTH1);
            VE.MDOCAUTHLEVEL3 = DOCAUTH;

            VE.DefaultView = true;
            return PartialView("_DOCPASS_STRGY_LEVEL3", VE);

        }
        public ActionResult DeleteRowLEVEL3(DocumentAuthorizationEntry VE)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            List<MDOCAUTHLAVEL3> DOCAUTH = new List<MDOCAUTHLAVEL3>();
            int count = 0;
            for (int i = 0; i <= VE.MDOCAUTHLEVEL3.Count - 1; i++)
            {
                if (VE.MDOCAUTHLEVEL3[i].Checked == false)
                {
                    count += 1;
                    MDOCAUTHLAVEL3 item = new MDOCAUTHLAVEL3();
                    item = VE.MDOCAUTHLEVEL3[i];
                    item.SLNO = Convert.ToSByte(count);
                    DOCAUTH.Add(item);
                }
            }
            VE.MDOCAUTHLEVEL3 = DOCAUTH;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_DOCPASS_STRGY_LEVEL3", VE);

        }
        public ActionResult AddRowLEVEL4(DocumentAuthorizationEntry VE)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));

            List<MDOCAUTHLAVEL4> DOCAUTH = new List<MDOCAUTHLAVEL4>();
            for (int i = 0; i <= VE.MDOCAUTHLEVEL4.Count - 1; i++)
            {
                MDOCAUTHLAVEL4 MIB = new MDOCAUTHLAVEL4();
                MIB = VE.MDOCAUTHLEVEL4[i];
                DOCAUTH.Add(MIB);
            }
            MDOCAUTHLAVEL4 DOCAUTH1 = new MDOCAUTHLAVEL4();
            var max = VE.MDOCAUTHLEVEL4.Max(a => Convert.ToInt32(a.SLNO));
            int SLNO = Convert.ToInt32(max) + 1;
            DOCAUTH1.SLNO = Convert.ToSByte(SLNO);
            DOCAUTH.Add(DOCAUTH1);
            VE.MDOCAUTHLEVEL4 = DOCAUTH;

            VE.DefaultView = true;
            return PartialView("_DOCPASS_STRGY_LEVEL4", VE);

        }
        public ActionResult DeleteRowLEVEL4(DocumentAuthorizationEntry VE)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            List<MDOCAUTHLAVEL4> DOCAUTH = new List<MDOCAUTHLAVEL4>();
            int count = 0;
            for (int i = 0; i <= VE.MDOCAUTHLEVEL4.Count - 1; i++)
            {
                if (VE.MDOCAUTHLEVEL4[i].Checked == false)
                {
                    count += 1;
                    MDOCAUTHLAVEL4 item = new MDOCAUTHLAVEL4();
                    item = VE.MDOCAUTHLEVEL4[i];
                    item.SLNO = Convert.ToSByte(count);
                    DOCAUTH.Add(item);
                }
            }
            VE.MDOCAUTHLEVEL4 = DOCAUTH;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_DOCPASS_STRGY_LEVEL4", VE);

        }
        public ActionResult AddRowLEVEL5(DocumentAuthorizationEntry VE)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));

            List<MDOCAUTHLAVEL5> DOCAUTH = new List<MDOCAUTHLAVEL5>();
            for (int i = 0; i <= VE.MDOCAUTHLEVEL5.Count - 1; i++)
            {
                MDOCAUTHLAVEL5 MIB = new MDOCAUTHLAVEL5();
                MIB = VE.MDOCAUTHLEVEL5[i];
                DOCAUTH.Add(MIB);
            }
            MDOCAUTHLAVEL5 DOCAUTH1 = new MDOCAUTHLAVEL5();
            var max = VE.MDOCAUTHLEVEL5.Max(a => Convert.ToInt32(a.SLNO));
            int SLNO = Convert.ToInt32(max) + 1;
            DOCAUTH1.SLNO = Convert.ToSByte(SLNO);
            DOCAUTH.Add(DOCAUTH1);
            VE.MDOCAUTHLEVEL5 = DOCAUTH;

            VE.DefaultView = true;
            return PartialView("_DOCPASS_STRGY_LEVEL5", VE);

        }
        public ActionResult DeleteRowLEVEL5(DocumentAuthorizationEntry VE)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            List<MDOCAUTHLAVEL5> DOCAUTH = new List<MDOCAUTHLAVEL5>();
            int count = 0;
            for (int i = 0; i <= VE.MDOCAUTHLEVEL5.Count - 1; i++)
            {
                if (VE.MDOCAUTHLEVEL5[i].Checked == false)
                {
                    count += 1;
                    MDOCAUTHLAVEL5 item = new MDOCAUTHLAVEL5();
                    item = VE.MDOCAUTHLEVEL5[i];
                    item.SLNO = Convert.ToSByte(count);
                    DOCAUTH.Add(item);
                }
            }
            VE.MDOCAUTHLEVEL5 = DOCAUTH;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_DOCPASS_STRGY_LEVEL5", VE);

        }
        public ActionResult SAVE(FormCollection FC, DocumentAuthorizationEntry VE)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_DOC_AUTH MDOCAUTH = new M_DOC_AUTH();
                        M_CNTRL_HDR MCH = new M_CNTRL_HDR();

                        MDOCAUTH.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO));
                        MDOCAUTH.EMD_NO = 0;
                        MDOCAUTH.CLCD = CommVar.ClientCode(UNQSNO);

                        if (VE.DefaultAction == "E")
                        {
                            MDOCAUTH.DOCCD = VE.M_DOC_AUTH.DOCCD;
                            MDOCAUTH.M_AUTONO = VE.M_DOC_AUTH.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_DOC_AUTH where p.M_AUTONO == VE.M_DOC_AUTH.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MDOCAUTH.EMD_NO = 0;
                            }
                            else
                            {
                                MDOCAUTH.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }


                            DB.M_DOC_AUTH.Where(x => x.M_AUTONO == VE.M_DOC_AUTH.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_DOC_AUTH.RemoveRange(DB.M_DOC_AUTH.Where(x => x.M_AUTONO == MDOCAUTH.M_AUTONO));

                            DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == VE.M_DOC_AUTH.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_LOCA.RemoveRange(DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == VE.M_DOC_AUTH.M_AUTONO));
                        }
                        else
                        {

                            MDOCAUTH.DOCCD = FC["doctyp"].ToString();
                        }
                        if (VE.MDOCAUTHLEVEL1 != null)
                        {
                            for (int i = 0; i <= VE.MDOCAUTHLEVEL1.Count - 1; i++)
                            {
                                if (VE.MDOCAUTHLEVEL1[i].SLNO != 0 && VE.MDOCAUTHLEVEL1[i].AUTHNM != null)
                                {
                                    M_DOC_AUTH MDOCAUTHLEVEL1 = new M_DOC_AUTH();
                                    MDOCAUTHLEVEL1.M_AUTONO = MDOCAUTH.M_AUTONO;
                                    MDOCAUTHLEVEL1.EMD_NO = MDOCAUTH.EMD_NO;
                                    MDOCAUTHLEVEL1.CLCD = CommVar.ClientCode(UNQSNO);
                                    MDOCAUTHLEVEL1.DOCCD = MDOCAUTH.DOCCD;
                                    MDOCAUTHLEVEL1.LVL = 1;
                                    MDOCAUTHLEVEL1.SLNO = VE.MDOCAUTHLEVEL1[i].SLNO;
                                    MDOCAUTHLEVEL1.EFF_DT = VE.M_DOC_AUTH.EFF_DT;
                                    MDOCAUTHLEVEL1.AUTHCD = VE.MDOCAUTHLEVEL1[i].AUTHCD;
                                    MDOCAUTHLEVEL1.AMT_FR = VE.MDOCAUTHLEVEL1[i].AMT_FR;
                                    MDOCAUTHLEVEL1.AMT_TO = VE.MDOCAUTHLEVEL1[i].AMT_TO;
                                    DB.M_DOC_AUTH.Add(MDOCAUTHLEVEL1);
                                }
                            }
                        }
                        if (VE.MDOCAUTHLEVEL2 != null)
                        {
                            for (int i = 0; i <= VE.MDOCAUTHLEVEL2.Count - 1; i++)
                            {
                                if (VE.MDOCAUTHLEVEL2[i].SLNO != 0 && VE.MDOCAUTHLEVEL2[i].AUTHNM != null)
                                {
                                    M_DOC_AUTH MDOCAUTHLEVEL2 = new M_DOC_AUTH();
                                    MDOCAUTHLEVEL2.M_AUTONO = MDOCAUTH.M_AUTONO;
                                    MDOCAUTHLEVEL2.EMD_NO = MDOCAUTH.EMD_NO;
                                    MDOCAUTHLEVEL2.CLCD = CommVar.ClientCode(UNQSNO);
                                    MDOCAUTHLEVEL2.DOCCD = MDOCAUTH.DOCCD;
                                    MDOCAUTHLEVEL2.LVL = 2;
                                    MDOCAUTHLEVEL2.SLNO = VE.MDOCAUTHLEVEL2[i].SLNO;
                                    MDOCAUTHLEVEL2.EFF_DT = VE.M_DOC_AUTH.EFF_DT;
                                    MDOCAUTHLEVEL2.AUTHCD = VE.MDOCAUTHLEVEL2[i].AUTHCD;
                                    MDOCAUTHLEVEL2.AMT_FR = VE.MDOCAUTHLEVEL2[i].AMT_FR;
                                    MDOCAUTHLEVEL2.AMT_TO = VE.MDOCAUTHLEVEL2[i].AMT_TO;
                                    DB.M_DOC_AUTH.Add(MDOCAUTHLEVEL2);
                                }
                            }
                        }
                        if (VE.MDOCAUTHLEVEL3 != null)
                        {
                            for (int i = 0; i <= VE.MDOCAUTHLEVEL3.Count - 1; i++)
                            {
                                if (VE.MDOCAUTHLEVEL3[i].SLNO != 0 && VE.MDOCAUTHLEVEL3[i].AUTHNM != null)
                                {
                                    M_DOC_AUTH MDOCAUTHLEVEL3 = new M_DOC_AUTH();
                                    MDOCAUTHLEVEL3.M_AUTONO = MDOCAUTH.M_AUTONO;
                                    MDOCAUTHLEVEL3.EMD_NO = MDOCAUTH.EMD_NO;
                                    MDOCAUTHLEVEL3.CLCD = CommVar.ClientCode(UNQSNO);
                                    MDOCAUTHLEVEL3.DOCCD = MDOCAUTH.DOCCD;
                                    MDOCAUTHLEVEL3.LVL = 3;
                                    MDOCAUTHLEVEL3.SLNO = VE.MDOCAUTHLEVEL3[i].SLNO;
                                    MDOCAUTHLEVEL3.EFF_DT = VE.M_DOC_AUTH.EFF_DT;
                                    MDOCAUTHLEVEL3.AUTHCD = VE.MDOCAUTHLEVEL3[i].AUTHCD;
                                    MDOCAUTHLEVEL3.AMT_FR = VE.MDOCAUTHLEVEL3[i].AMT_FR;
                                    MDOCAUTHLEVEL3.AMT_TO = VE.MDOCAUTHLEVEL3[i].AMT_TO;
                                    DB.M_DOC_AUTH.Add(MDOCAUTHLEVEL3);
                                }
                            }
                        }
                        if (VE.MDOCAUTHLEVEL4 != null)
                        {
                            for (int i = 0; i <= VE.MDOCAUTHLEVEL4.Count - 1; i++)
                            {
                                if (VE.MDOCAUTHLEVEL4[i].SLNO != 0 && VE.MDOCAUTHLEVEL4[i].AUTHNM != null)
                                {
                                    M_DOC_AUTH MDOCAUTHLEVEL4 = new M_DOC_AUTH();
                                    MDOCAUTHLEVEL4.M_AUTONO = MDOCAUTH.M_AUTONO;
                                    MDOCAUTHLEVEL4.EMD_NO = MDOCAUTH.EMD_NO;
                                    MDOCAUTHLEVEL4.CLCD = CommVar.ClientCode(UNQSNO);
                                    MDOCAUTHLEVEL4.DOCCD = MDOCAUTH.DOCCD;
                                    MDOCAUTHLEVEL4.LVL = 4;
                                    MDOCAUTHLEVEL4.SLNO = VE.MDOCAUTHLEVEL4[i].SLNO;
                                    MDOCAUTHLEVEL4.EFF_DT = VE.M_DOC_AUTH.EFF_DT;
                                    MDOCAUTHLEVEL4.AUTHCD = VE.MDOCAUTHLEVEL4[i].AUTHCD;
                                    MDOCAUTHLEVEL4.AMT_FR = VE.MDOCAUTHLEVEL4[i].AMT_FR;
                                    MDOCAUTHLEVEL4.AMT_TO = VE.MDOCAUTHLEVEL4[i].AMT_TO;
                                    DB.M_DOC_AUTH.Add(MDOCAUTHLEVEL4);
                                }
                            }
                        }
                        if (VE.MDOCAUTHLEVEL5 != null)
                        {
                            for (int i = 0; i <= VE.MDOCAUTHLEVEL5.Count - 1; i++)
                            {
                                if (VE.MDOCAUTHLEVEL5[i].SLNO != 0 && VE.MDOCAUTHLEVEL5[i].AUTHNM != null)
                                {
                                    M_DOC_AUTH MDOCAUTHLEVEL5 = new M_DOC_AUTH();
                                    MDOCAUTHLEVEL5.M_AUTONO = MDOCAUTH.M_AUTONO;
                                    MDOCAUTHLEVEL5.EMD_NO = MDOCAUTH.EMD_NO;
                                    MDOCAUTHLEVEL5.CLCD = CommVar.ClientCode(UNQSNO);
                                    MDOCAUTHLEVEL5.DOCCD = MDOCAUTH.DOCCD;
                                    MDOCAUTHLEVEL5.LVL = 5;
                                    MDOCAUTHLEVEL5.SLNO = VE.MDOCAUTHLEVEL5[i].SLNO;
                                    MDOCAUTHLEVEL5.EFF_DT = VE.M_DOC_AUTH.EFF_DT;
                                    MDOCAUTHLEVEL5.AUTHCD = VE.MDOCAUTHLEVEL5[i].AUTHCD;
                                    MDOCAUTHLEVEL5.AMT_FR = VE.MDOCAUTHLEVEL5[i].AMT_FR;
                                    MDOCAUTHLEVEL5.AMT_TO = VE.MDOCAUTHLEVEL5[i].AMT_TO;
                                    DB.M_DOC_AUTH.Add(MDOCAUTHLEVEL5);
                                }
                            }
                        }
                        if (VE.CompanyLocationName != null)
                        {
                            for (int i = 0; i <= VE.CompanyLocationName.Count - 1; i++)
                            {
                                if (VE.CompanyLocationName[i].Checked)
                                {
                                    M_CNTRL_LOCA MCL = new M_CNTRL_LOCA();
                                    MCL.M_AUTONO = MDOCAUTH.M_AUTONO;
                                    MCL.EMD_NO = MDOCAUTH.EMD_NO;
                                    MCL.CLCD = CommVar.ClientCode(UNQSNO);
                                    MCL.COMPCD = VE.CompanyLocationName[i].COMPCD;
                                    MCL.LOCCD = VE.CompanyLocationName[i].LOCCD;
                                    DB.M_CNTRL_LOCA.Add(MCL);
                                }
                            }
                        }

                        MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_DOC_AUTH", MDOCAUTH.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO));
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_CNTRL_HDR.Add(MCH);
                            DB.SaveChanges();
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        }
                        DB.SaveChanges();
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
                        DB.M_DOC_AUTH.Where(x => x.M_AUTONO == VE.M_DOC_AUTH.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_DOC_AUTH.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == VE.M_DOC_AUTH.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.M_CNTRL_LOCA.RemoveRange(DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == VE.M_DOC_AUTH.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_DOC_AUTH.RemoveRange(DB.M_DOC_AUTH.Where(x => x.M_AUTONO == VE.M_DOC_AUTH.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_DOC_AUTH.M_AUTONO));
                        DB.SaveChanges();

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