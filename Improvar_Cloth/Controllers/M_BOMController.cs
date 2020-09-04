using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Microsoft.Ajax.Utilities;
using Oracle.ManagedDataAccess.Client;

namespace Improvar.Controllers
{
    public class M_BOMController : Controller
    {
        Connection Cn = new Connection();
        string CS = null;
        MasterHelp Master_Help = new MasterHelp();
        MasterHelpFa Master_HelpFa = new MasterHelpFa();
        M_SITEMBOM sl; M_CNTRL_HDR sll; M_SIZE sl1; M_COLOR sl2; M_SITEM sl3; M_JOBMSTSUB_STDRT MJS;M_UOM sl4;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_BOM
        public ActionResult M_BOM(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    BOMMasterEntry VE = new BOMMasterEntry();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    switch (VE.MENU_PARA)
                    {
                        case "OPR": ViewBag.formname = "BOM (Operation)"; break;
                        case "MTRL": ViewBag.formname = "BOM (Materials)"; break;
                        case "APRV": ViewBag.formname = "BOM (Approval)"; break;
                        default: ViewBag.formname = ""; break;
                    }
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    VE.SizeLink = Master_Help.SIZE_LINK();
                    M_SITEMBOM SITEMBOM = new M_SITEMBOM(); SITEMBOM.EFFDT = System.DateTime.Now; VE.M_SITEMBOM = SITEMBOM;
                    VE.DefaultAction = op;
                    if (op.Length != 0)
                    {
                        string GCS = Cn.GCS();
                        VE.IndexKey = (from p in DB.M_SITEMBOM orderby p.BOMCD select new IndexKey() { Navikey = p.BOMCD + GCS + p.EFFDT }).ToList();
                        if (op == "E" || op == "D" || op == "V")
                        {
                            if (searchValue.Length != 0)
                            {
                                VE.Index = Nindex;
                                VE = Navigation(VE, DB, Nindex, searchValue);
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
                            VE.M_SITEMBOM = sl;
                            VE.M_CNTRL_HDR = sll;
                            VE.M_SIZE = sl1;
                            VE.M_COLOR = sl2;
                            VE.M_SITEM = sl3;
                            VE.M_UOM = sl4;
                        }
                        if (op.ToString() == "A")
                        {
                            if (VE.MENU_PARA == "OPR")
                            {
                                List<MSITEMBOMSJOB> BOMSJOB = new List<MSITEMBOMSJOB>();
                                for (int i = 0; i <= 9; i++)
                                {
                                    MSITEMBOMSJOB SJOB = new MSITEMBOMSJOB();
                                    SJOB.SLNO = Convert.ToByte(i + 1);
                                    BOMSJOB.Add(SJOB);
                                    VE.MSITEMBOMSJOB = BOMSJOB;
                                }
                                VE.MSITEMBOMSJOB = BOMSJOB;
                            }
                            else if (VE.MENU_PARA == "MTRL")
                            {
                                List<MSITEMBOMPART> BOMPART = new List<MSITEMBOMPART>();
                                for (int i = 0; i <= 9; i++)
                                {
                                    MSITEMBOMPART PART = new MSITEMBOMPART();
                                    PART.SLNO = Convert.ToByte(i + 1);
                                    BOMPART.Add(PART);
                                    VE.MSITEMBOMPART = BOMPART;
                                }
                                VE.MSITEMBOMPART = BOMPART;
                            }
                            List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                            UploadDOC UPL = new UploadDOC();
                            UPL.DocumentType = Cn.DOC_TYPE();
                            UploadDOC1.Add(UPL);
                            VE.UploadDOC = UploadDOC1;

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
                BOMMasterEntry VE = new BOMMasterEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }

        }

        public BOMMasterEntry Navigation(BOMMasterEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new M_SITEMBOM(); sll = new M_CNTRL_HDR(); sl1 = new M_SIZE(); sl2 = new M_COLOR(); sl3 = new M_SITEM(); MJS = new M_JOBMSTSUB_STDRT();sl4 = new M_UOM();
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), CommVar.InvSchema(UNQSNO));
            var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();

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
                DateTime EDT = Convert.ToDateTime(aa[1]);
                sl = DB.M_SITEMBOM.Find(aa[0], EDT);
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
                sl1 = DB.M_SIZE.Find(sl.SIZECD);
                sl2 = DB.M_COLOR.Find(sl.COLRCD);
                sl3 = DB.M_SITEM.Find(sl.ITCD);
                sl4 = DBF.M_UOM.Find(sl3.UOMCD);
                if (sll.INACTIVE_TAG == "Y")
                {
                    VE.Checked = true;
                }
                else
                {
                    VE.Checked = false;
                }
                if (VE.MENU_PARA == "OPR")
                {
                    VE.MSITEMBOMSJOB = (from i in DB.M_SITEMBOMSJOB
                                        join j in DB.M_PARTS on i.PARTCD equals j.PARTCD into x
                                        from j in x.DefaultIfEmpty()
                                        join k in DB.M_JOBMSTSUB on i.SJOBCD equals k.SJOBCD into y
                                        from k in y.DefaultIfEmpty()
                                        join l in DB.M_JOBMST on k.JOBCD equals l.JOBCD into z
                                        from l in z.DefaultIfEmpty()
                                        join m in DB.M_MACHINE on i.MCCD equals m.MCCD into p
                                        from m in p.DefaultIfEmpty()
                                        where (i.BOMCD == sl.BOMCD && i.EFFDT == sl.EFFDT)
                                        select new MSITEMBOMSJOB()
                                        {
                                            BOMCD = i.BOMCD,
                                            EFFDT = i.EFFDT,
                                            SLNO = i.SLNO,
                                            PARTCD = i.PARTCD,
                                            PARTNM = j.PARTNM,
                                            SJOBCD = i.SJOBCD,
                                            SJOBNM = k.SJOBNM,
                                            SEQORDNO = i.SEQORDNO,
                                            JOBCD = k.JOBCD,
                                            JOBNM = l.JOBNM,
                                            JOBRT = i.JOBRT,
                                            REMARK = i.REMARK,
                                            LENCM = i.LENCM,
                                            SMV = i.SMV,
                                            TRATIO = i.TRATIO,
                                            MCCD = i.MCCD,
                                            MCNM = m.MCNM
                                        }).OrderBy(s => s.SLNO).ToList();

                    foreach (var i in VE.MSITEMBOMSJOB)
                    {
                        var TEMP_R1 = (from x in DB.M_JOBMSTSUB_STDRT where x.EFFDT <= sl.EFFDT && x.SJOBCD == i.SJOBCD && x.JOBPRCCD == "STD" select x).ToList();
                        TEMP_R1 = TEMP_R1.Where(a => a.EFFDT == TEMP_R1.Max(b => b.EFFDT)).ToList();
                        if (TEMP_R1 != null)
                        {
                            for (int z = 0; z <= TEMP_R1.Count() - 1; z++)
                            {
                                i.STD_SJ_JOBRT = TEMP_R1[z].JOBRT;
                            }
                        }
                    }

                    if (VE.MSITEMBOMSJOB.Count > 0)
                    {
                        VE.MSITEMBOM_PRINT = (from i in VE.MSITEMBOMSJOB
                                              select new MSITEMBOM_PRINT
                                              {
                                                  PARTCD = i.PARTCD,
                                                  PARTNM = i.PARTNM
                                              }).DistinctBy(A => A.PARTCD).ToList();
                    }
                    TempData["MSITEMBOMSJOB_Data"] = VE.MSITEMBOMSJOB;

                    if (VE.MSITEMBOMSJOB.Count == 0)
                    {
                        List<MSITEMBOMSJOB> BOMSJOB = new List<MSITEMBOMSJOB>();
                        MSITEMBOMSJOB SJOB = new MSITEMBOMSJOB();
                        SJOB.SLNO = 1;
                        BOMSJOB.Add(SJOB);
                        VE.MSITEMBOMSJOB = BOMSJOB;
                    }
                }
                else if (VE.MENU_PARA == "MTRL")
                {
                    VE.MSITEMBOMPART = (from i in DB.M_SITEMBOMPART
                                        join j in DB.M_PARTS on i.PARTCD equals j.PARTCD into x
                                        from j in x.DefaultIfEmpty()
                                        join l in DB.M_JOBMST on i.JOBCD equals l.JOBCD
                                        join m in DB.M_SIZE on i.SIZECD equals m.SIZECD into y
                                        from m in y.DefaultIfEmpty()
                                        where (i.BOMCD == sl.BOMCD && i.EFFDT == sl.EFFDT)
                                        select new MSITEMBOMPART()
                                        {
                                            BOMCD = i.BOMCD,
                                            EFFDT = i.EFFDT,
                                            SLNO = i.SLNO,
                                            PARTCD = i.PARTCD,
                                            PARTNM = j.PARTNM,
                                            JOBCD = i.JOBCD,
                                            JOBNM = l.JOBNM,
                                            SIZECD = i.SIZECD,
                                            SIZENM = m.SIZENM,
                                            JOBRT = i.JOBRT,
                                            MTRLCOST = i.MTRLCOST,
                                            REMARK = i.REMARK
                                        }).OrderBy(s => s.SLNO).ToList();

                    for (int s = 0; s <= VE.MSITEMBOMPART.Count - 1; s++)
                    {
                        int SSLNO = VE.MSITEMBOMPART[s].SLNO;
                        string BOM = VE.MSITEMBOMPART[s].BOMCD;
                        DateTime EDATE = VE.MSITEMBOMPART[s].EFFDT;

                        var SUBL = (from e in DB.M_SITEMBOMSUPLR
                                    where e.BOMCD == BOM && e.EFFDT == EDATE && e.SLNO == SSLNO && e.RSLNO == 0
                                    select new
                                    {
                                        SLNO = e.SLNO,
                                        PSLNO = e.PSLNO,
                                        SLCD = e.SLCD

                                    }).ToList();

                        for (int q = 0; q <= SUBL.Count - 1; q++)
                        {
                            if (SUBL[q].PSLNO == 1)
                            {
                                VE.MSITEMBOMPART[s].SLCD1 = SUBL[q].SLCD;
                            }
                            else if (SUBL[q].PSLNO == 2)
                            {
                                VE.MSITEMBOMPART[s].SLCD2 = SUBL[q].SLCD;
                            }
                            else if (SUBL[q].PSLNO == 3)
                            {
                                VE.MSITEMBOMPART[s].SLCD3 = SUBL[q].SLCD;
                            }
                        }
                    }
                    foreach (var i in VE.MSITEMBOMPART)
                    {
                        var temp1 = DBF.M_SUBLEG.Find(i.SLCD1);
                        if (temp1 != null)
                        {
                            i.SLNM1 = temp1.SLNM;
                        }
                        var temp2 = DBF.M_SUBLEG.Find(i.SLCD2);
                        if (temp2 != null)
                        {
                            i.SLNM2 = temp2.SLNM;
                        }
                        var temp3 = DBF.M_SUBLEG.Find(i.SLCD3);
                        if (temp3 != null)
                        {
                            i.SLNM3 = temp3.SLNM;
                        }
                    }
                    foreach (var i in VE.MSITEMBOMPART)
                    {
                        VE.MSITEMBOMMTRL = (from a in DB.M_SITEMBOMMTRL
                                            join b in DB.M_SITEM on a.ITCD equals b.ITCD into xx
                                            from b in xx.DefaultIfEmpty()
                                            join c in DB.M_PARTS on a.PARTCD equals c.PARTCD into x
                                            from c in x.DefaultIfEmpty()
                                            join d in DB.M_SIZE on a.SIZECD equals d.SIZECD into y
                                            from d in y.DefaultIfEmpty()
                                            join e in DB.M_COLOR on a.COLRCD equals e.COLRCD into z
                                            from e in z.DefaultIfEmpty()
                                            where (a.BOMCD == sl.BOMCD && a.EFFDT == sl.EFFDT && a.SLNO == i.SLNO)
                                            select new MSITEMBOMMTRL()
                                            {
                                                BOMCD = a.BOMCD,
                                                EFFDT = a.EFFDT,
                                                PSLNO = a.RSLNO,
                                                ITCD = a.ITCD,
                                                ITNM = b.ITNM,
                                                PARTCD = a.PARTCD,
                                                PARTNM = c.PARTNM,
                                                SIZECD = a.SIZECD,
                                                SIZENM = d.SIZENM,
                                                COLRCD = a.COLRCD,
                                                COLRNM = e.COLRNM,
                                                QNTY = a.QNTY,
                                                MTRLRT = a.MTRLRT,
                                                REMARK = a.REMARK,
                                                ParentSerialNo = i.SLNO,
                                                SLNO = i.SLNO
                                            }).OrderBy(s => s.PSLNO).ToList();
                        for (int s = 0; s <= VE.MSITEMBOMMTRL.Count - 1; s++)
                        {
                            int MSSLNO = VE.MSITEMBOMMTRL[s].SLNO;
                            byte MPSLNO = VE.MSITEMBOMMTRL[s].PSLNO;
                            string MBOM = VE.MSITEMBOMMTRL[s].BOMCD;
                            DateTime MEDATE = VE.MSITEMBOMMTRL[s].EFFDT;
                            var SUBLM = (from e in DB.M_SITEMBOMSUPLR where e.BOMCD == MBOM && e.EFFDT == MEDATE && e.SLNO == MSSLNO && e.RSLNO == MPSLNO select new { SLNO = e.SLNO, PSLNO = e.PSLNO, SLCD = e.SLCD, RSLNO = e.RSLNO }).ToList();
                            for (int q = 0; q <= SUBLM.Count - 1; q++) { if (SUBLM[q].PSLNO == 1) { VE.MSITEMBOMMTRL[s].SLCD1 = SUBLM[q].SLCD; } else if (SUBLM[q].PSLNO == 2) { VE.MSITEMBOMMTRL[s].SLCD2 = SUBLM[q].SLCD; } else if (SUBLM[q].PSLNO == 3) { VE.MSITEMBOMMTRL[s].SLCD3 = SUBLM[q].SLCD; } }
                        }
                        foreach (var q in VE.MSITEMBOMMTRL)
                        {
                            var tempM1 = DBF.M_SUBLEG.Find(q.SLCD1);
                            if (tempM1 != null)
                            {
                                q.SLNM1 = tempM1.SLNM;
                            }
                            var tempM2 = DBF.M_SUBLEG.Find(q.SLCD2);
                            if (tempM2 != null)
                            {
                                q.SLNM2 = tempM2.SLNM;
                            }
                            var tempM3 = DBF.M_SUBLEG.Find(q.SLCD3);
                            if (tempM3 != null)
                            {
                                q.SLNM3 = tempM3.SLNM;
                            }
                        }
                        var javaScriptSerializer1 = new System.Web.Script.Serialization.JavaScriptSerializer();
                        string JR1 = javaScriptSerializer1.Serialize(VE.MSITEMBOMMTRL);
                        i.ChildData = JR1;
                        VE.MSITEMBOMMTRL_RMPM = (from a in DB.M_SITEMBOMMTRL
                                                 where (a.BOMCD == sl.BOMCD && a.EFFDT == sl.EFFDT && a.SLNO == i.SLNO)
                                                 select new MSITEMBOMMTRL_RMPM()
                                                 {
                                                     BOMCD = a.BOMCD,
                                                     EFFDT = a.EFFDT,
                                                     PSLNO = a.RSLNO,
                                                     ITCD = a.ITCD,
                                                     QNTY = a.QNTY,
                                                     MTRLRT = a.MTRLRT,
                                                     REMARK = a.REMARK,
                                                     SIZE_LNK = a.SIZE_LNK,
                                                     ParentSerialNo = i.SLNO,
                                                     SLNO = i.SLNO
                                                 }).OrderBy(s => s.PSLNO).ToList();
                        //foreach (var x in VE.MSITEMBOMMTRL)
                        //{
                        //    var temp_data = (from xx in DBI.M_ITEM where xx.ITCD == x.ITCD select xx.ITDESCN).ToList();
                        //    x.ITNM = temp_data != null && temp_data.Count > 0 ? temp_data[0] : "";
                        //    //var temp = DBI.M_ITEM.Find(x.ITCD);
                        //}
                        foreach (var x in VE.MSITEMBOMMTRL_RMPM) { var temp = DB.M_SITEM.Find(x.ITCD); x.ITNM = temp.ITNM; }
                        var javaScriptSerializer2 = new System.Web.Script.Serialization.JavaScriptSerializer();
                        string JR2 = javaScriptSerializer2.Serialize(VE.MSITEMBOMMTRL_RMPM);
                        i.ChildData_RMPM = JR2;
                    }
                    if (VE.MSITEMBOMPART.Count == 0)
                    {
                        List<MSITEMBOMPART> BOMPART = new List<MSITEMBOMPART>();
                        MSITEMBOMPART PART = new MSITEMBOMPART();
                        PART.SLNO = 1;
                        BOMPART.Add(PART);
                        VE.MSITEMBOMPART = BOMPART;
                    }
                }
                if (VE.MSITEMBOM_PRINT != null)
                {
                    for (int x = 0; x <= VE.MSITEMBOM_PRINT.Count - 1; x++)
                    {
                        VE.MSITEMBOM_PRINT[x].SLNO = Convert.ToByte(x + 1);
                    }
                    if (VE.MSITEMBOM_PRINT.Count == 1)
                    {
                        VE.MSITEMBOM_PRINT[0].Checked = true;
                    }
                }
                VE.UploadDOC = Cn.GetUploadImage(CommVar.CurSchema(UNQSNO).ToString(), Convert.ToInt32(sl.M_AUTONO));
                if (VE.UploadDOC.Count == 0)
                {
                    List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                    UploadDOC UPL = new UploadDOC();
                    UPL.DocumentType = doctP;
                    UploadDOC1.Add(UPL);
                    VE.UploadDOC = UploadDOC1;
                }
            }
            else
            {
                sl.EFFDT = System.DateTime.Now;
                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                UploadDOC UPL = new UploadDOC();
                UPL.DocumentType = doctP;
                UploadDOC1.Add(UPL);
                VE.UploadDOC = UploadDOC1;

            }
            return VE;
        }
        public ActionResult SearchPannelData()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var MDT = (from j in DB.M_SITEMBOM
                       join p in DB.M_CNTRL_HDR on j.M_AUTONO equals (p.M_AUTONO)
                       join q in DB.M_SITEM on j.ITCD equals (q.ITCD)
                       where (j.M_AUTONO == p.M_AUTONO)
                       orderby j.M_AUTONO
                       select new { j.BOMCD, j.EFFDT, p.M_AUTONO, q.ITNM, q.STYLENO }).ToList().Select(x => new
                       {
                           BOMCD = x.BOMCD,
                           M_AUTONO = x.M_AUTONO,
                           STYLENO = x.STYLENO,
                           ITNM = x.ITNM,
                           EFFDT = x.EFFDT.ToString().Replace('-', '/').Substring(0, 10),
                       }).Distinct().OrderBy(s => s.BOMCD).ToList();

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Style No" + Cn.GCS() + "Item Name" + Cn.GCS() + "Bom Code" + Cn.GCS() + "AUTO NO" + Cn.GCS() + "EFFDT";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].STYLENO + "</td><td>" + MDT[j].ITNM + "</td><td>" + MDT[j].BOMCD + "</td><td>" + MDT[j].M_AUTONO + "</td><td>" + MDT[j].EFFDT + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "2" + Cn.GCS() + "4", "3" + Cn.GCS() + "4"));
        }
        public ActionResult GetItemDetails(string val, string Code)
        {
            var str = Master_Help.ITCD_help(val, Code);
            if (str.IndexOf("='helpmnu'") >= 0)
            {
                return PartialView("_Help2", str);
            }
            else
            {
                return Content(str);
            }
        }
        public ActionResult CheckArticleItem(BOMMasterEntry VE, string ITEM)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                var CHECK_DATA = (from X in DB.M_SITEMBOM where X.ITCD == ITEM select X).ToList();
                if (CHECK_DATA != null && CHECK_DATA.Count > 0)
                {
                    return Content("" + CHECK_DATA[0].EFFDT.ToString().Substring(0, 10) + "");
                }
                else
                {
                    return Content("0");
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetMachineDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.MACHINE_HELP(val));
                }
                else
                {
                    string str = Master_Help.MACHINE_HELP(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        //public ActionResult GetMaterialItemDetails(string val, string Code)
        //{
        //    //string ITGTYPE = "A" + Cn.GCS() + "C" + Cn.GCS() + "L" + Cn.GCS() + "P" + Cn.GCS() + "Y";
        //    string ITGTYPE = "'A','C','L','P','Y'";
        //    var str = Master_Help.ITCD_help(val, ITGTYPE, "", "");
        //    if (str.IndexOf("='helpmnu'") >= 0)
        //    {
        //        return PartialView("_Help2", str);
        //    }
        //    else
        //    {
        //        return Content(str);
        //    }
        //}
        public ActionResult GetRMPMITEMDETAILS(string val)
        {
            if (val == null)
            {
                //return PartialView("_Help2", Master_Help.INVENTORY_ITEM("001", "", val));
                return PartialView("_Help2", Master_Help.INVENTORY_ITEM("", "", val));
            }
            else
            {
                //string str = Master_Help.INVENTORY_ITEM("001", "", val);
                string str = Master_Help.INVENTORY_ITEM("", "", val);
                return Content(str);
            }
        }
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
        public ActionResult UOMCode(string val)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                var query = (from c in DB.M_UOM where (c.UOMCD == val) select c);
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = i.UOMCD + Cn.GCS() + i.UOMNM;
                    }
                    return Content(str);
                }
                else
                {
                    return Content("0");
                }
            }
            catch (Exception e)
            {
                Cn.SaveException(e, "");
                return Content(e.Message);
            }
        }
        public ActionResult GetSizeDetails(string val)
        {
            try
            {
                var str = Master_Help.SIZECD_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
       
        public ActionResult GetPartDetails(string val, string Code)
        {
            try
            {
                var str = Master_Help.PARTCD_help(val, Code);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetColorDetails(string val)
        {
            try
            {
                var str = Master_Help.COLRCD_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetSubJobDetails(string VAL, string VAL1)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            return PartialView("_Help2", Master_Help.SUBJOB(DB, VAL, VAL1));
        }
        public ActionResult SUBJOBCode(string val, string val1)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            DateTime EFF_DT = Convert.ToDateTime(val1);

            var query1 = (from a in DB.M_JOBMSTSUB_STDRT join b in DB.M_JOBMSTSUB on a.SJOBCD equals b.SJOBCD where (a.SJOBCD == val && a.SJOBCD == b.SJOBCD && a.EFFDT <= EFF_DT && a.JOBPRCCD == "STD") select new { SJOBCD = a.SJOBCD, SJOBNM = b.SJOBNM, JOBRT = a.JOBRT, EFFDT = a.EFFDT }).ToList();

            if (query1.Count != 0)
            {
                var query = (from c in DB.M_JOBMSTSUB
                             join j in DB.M_JOBMST on c.JOBCD equals j.JOBCD
                             join k in DB.M_JOBMSTSUB_STDRT on c.SJOBCD equals k.SJOBCD into g
                             from k in g.DefaultIfEmpty()
                             where (c.SJOBCD == val && c.JOBCD == j.JOBCD && k.EFFDT <= EFF_DT && k.JOBPRCCD == "STD")
                             select new
                             {
                                 SJOBCD = c.SJOBCD,
                                 SJOBNM = c.SJOBNM,
                                 JOBCD = c.JOBCD,
                                 JOBNM = j.JOBNM,
                                 JOBRT = k.JOBRT,
                                 EFFDT = k.EFFDT
                             }).ToList();

                query = query.Where(a => a.EFFDT == query.Max(b => b.EFFDT)).ToList();

                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = i.SJOBCD + Cn.GCS() + i.SJOBNM + Cn.GCS() + i.JOBCD + Cn.GCS() + i.JOBNM + Cn.GCS() + i.JOBRT;
                    }
                    return Content(str);
                }
                else
                {
                    return Content("0");
                }
            }
            else
            {
                var query = (from c in DB.M_JOBMSTSUB
                             join j in DB.M_JOBMST on c.JOBCD equals j.JOBCD
                             where (c.SJOBCD == val && c.JOBCD == j.JOBCD)
                             select new
                             {
                                 SJOBCD = c.SJOBCD,
                                 SJOBNM = c.SJOBNM,
                                 JOBCD = c.JOBCD,
                                 JOBNM = j.JOBNM
                             }).ToList();

                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = i.SJOBCD + Cn.GCS() + i.SJOBNM + Cn.GCS() + i.JOBCD + Cn.GCS() + i.JOBNM;
                    }
                    return Content(str);
                }
                else
                {
                    return Content("0");
                }
            }

        }
        public ActionResult GetJobDetails()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            return PartialView("_Help2", Master_Help.JOBCD_help(DB));
        }
        public ActionResult JOBCode(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_JOBMST where (c.JOBCD == val) select c).ToList();
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
        public ActionResult GetPrefJobberDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.PREFJOBBER(val));
                }
                else
                {
                    string str = Master_Help.PREFJOBBER(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetSubJobStyle()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            return PartialView("_Help2", Master_Help.STYLE(DB));
        }
        public ActionResult SubJobStyle(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_JOBMSTSUB where (c.SJOBSTYLE == val) select c).DistinctBy(a => a.SJOBSTYLE).ToList();
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult GetSubJobCategory(string VAL)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            return PartialView("_Help2", Master_Help.CATEGORY(DB, VAL));
        }
        public ActionResult SubJobCategory(string val, string val1)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            var query = (from c in DB.M_JOBMSTSUB
                         join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                         where i.INACTIVE_TAG == "N" && c.SJOBSTYLE == val1
                         select new
                         {
                             SCATE = c.SCATE
                         }).DistinctBy(a => a.SCATE).ToList();
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult AddRowMAIN(BOMMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            if (VE.MSITEMBOMPART == null)
            {
                List<MSITEMBOMPART> BOMPART = new List<MSITEMBOMPART>();
                MSITEMBOMPART PART = new MSITEMBOMPART();
                PART.SLNO = 1;
                BOMPART.Add(PART);
                VE.MSITEMBOMPART = BOMPART;
            }
            else
            {
                List<MSITEMBOMPART> BOMPART = new List<MSITEMBOMPART>();
                for (int i = 0; i <= VE.MSITEMBOMPART.Count - 1; i++)
                {
                    MSITEMBOMPART PART = new MSITEMBOMPART();
                    PART = VE.MSITEMBOMPART[i];
                    BOMPART.Add(PART);
                }
                MSITEMBOMPART PART1 = new MSITEMBOMPART();
                var max = VE.MSITEMBOMPART.Max(a => Convert.ToInt32(a.SLNO));
                int SRLNO = Convert.ToInt32(max) + 1;
                PART1.SLNO = Convert.ToByte(SRLNO);
                BOMPART.Add(PART1);
                VE.MSITEMBOMPART = BOMPART;
            }
            VE.DefaultView = true;
            return PartialView("_M_BOM_MAIN", VE);

        }
        public ActionResult DeleteRowMAIN(BOMMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            List<MSITEMBOMPART> BOMPART = new List<MSITEMBOMPART>();
            int count = 0;
            for (int i = 0; i <= VE.MSITEMBOMPART.Count - 1; i++)
            {
                if (VE.MSITEMBOMPART[i].Checked == false)
                {
                    count += 1;
                    MSITEMBOMPART item = new MSITEMBOMPART();
                    item = VE.MSITEMBOMPART[i];
                    item.SLNO = Convert.ToByte(count);
                    BOMPART.Add(item);
                }
            }
            VE.MSITEMBOMPART = BOMPART;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_M_BOM_MAIN", VE);

        }
        public ActionResult AddRowOPERATION(BOMMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            if (VE.MSITEMBOMSJOB == null)
            {
                List<MSITEMBOMSJOB> BOMSJOB = new List<MSITEMBOMSJOB>();
                MSITEMBOMSJOB SJOB = new MSITEMBOMSJOB();
                SJOB.SLNO = 1;
                BOMSJOB.Add(SJOB);
                VE.MSITEMBOMSJOB = BOMSJOB;
            }
            else
            {
                List<MSITEMBOMSJOB> BOMSJOB = new List<MSITEMBOMSJOB>();
                for (int i = 0; i <= VE.MSITEMBOMSJOB.Count - 1; i++)
                {
                    MSITEMBOMSJOB SJOB = new MSITEMBOMSJOB();
                    SJOB = VE.MSITEMBOMSJOB[i];
                    BOMSJOB.Add(SJOB);
                }
                MSITEMBOMSJOB SJOB1 = new MSITEMBOMSJOB();
                var max = VE.MSITEMBOMSJOB.Max(a => Convert.ToInt32(a.SLNO));
                int SRLNO = Convert.ToInt32(max) + 1;
                SJOB1.SLNO = Convert.ToByte(SRLNO);
                BOMSJOB.Add(SJOB1);
                VE.MSITEMBOMSJOB = BOMSJOB;
            }
            VE.DefaultView = true;
            return PartialView("_M_BOM_OPERATION", VE);
        }
        public ActionResult DeleteRowOPERATION(BOMMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            List<MSITEMBOMSJOB> BOMSJOB = new List<MSITEMBOMSJOB>();
            int count = 0;
            for (int i = 0; i <= VE.MSITEMBOMSJOB.Count - 1; i++)
            {
                if (VE.MSITEMBOMSJOB[i].Checked == false)
                {
                    count += 1;
                    MSITEMBOMSJOB item = new MSITEMBOMSJOB();
                    item = VE.MSITEMBOMSJOB[i];
                    item.SLNO = Convert.ToByte(count);
                    BOMSJOB.Add(item);
                }
            }
            VE.MSITEMBOMSJOB = BOMSJOB;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_M_BOM_OPERATION", VE);

        }
        public ActionResult AddRowMATERIAL(BOMMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            //=================For Size Link================//
            List<SizeLink> SLNK = new List<SizeLink>();
            SizeLink SLNK1 = new SizeLink(); SLNK1.text = "Yes"; SLNK1.value = "Y"; SLNK.Add(SLNK1);
            SizeLink SLNK2 = new SizeLink(); SLNK2.text = "No"; SLNK2.value = "N"; SLNK.Add(SLNK2); VE.SizeLink = SLNK;
            //=================End Size Link================//
            if (VE.MSITEMBOMMTRL == null)
            {
                List<MSITEMBOMMTRL> BOMMTRL = new List<MSITEMBOMMTRL>();
                MSITEMBOMMTRL MTRL = new MSITEMBOMMTRL();
                MTRL.PSLNO = 1;
                BOMMTRL.Add(MTRL);
                VE.MSITEMBOMMTRL = BOMMTRL;
            }
            else
            {
                List<MSITEMBOMMTRL> BOMMTRL = new List<MSITEMBOMMTRL>();
                for (int i = 0; i <= VE.MSITEMBOMMTRL.Count - 1; i++)
                {
                    MSITEMBOMMTRL MTRL = new MSITEMBOMMTRL();
                    MTRL = VE.MSITEMBOMMTRL[i];
                    BOMMTRL.Add(MTRL);
                }
                MSITEMBOMMTRL MTRL1 = new MSITEMBOMMTRL();
                var max = VE.MSITEMBOMMTRL.Max(a => Convert.ToInt32(a.PSLNO));
                int SRLNO = Convert.ToInt32(max) + 1;
                MTRL1.PSLNO = Convert.ToByte(SRLNO);
                BOMMTRL.Add(MTRL1);
                VE.MSITEMBOMMTRL = BOMMTRL;
            }
            VE.DefaultView = true;
            return PartialView("_M_BOM_MATERIAL", VE);

        }
        public ActionResult DeleteRowMATERIAL(BOMMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            //=================For Size Link================//
            List<SizeLink> SLNK = new List<SizeLink>();
            SizeLink SLNK1 = new SizeLink(); SLNK1.text = "Yes"; SLNK1.value = "Y"; SLNK.Add(SLNK1);
            SizeLink SLNK2 = new SizeLink(); SLNK2.text = "No"; SLNK2.value = "N"; SLNK.Add(SLNK2); VE.SizeLink = SLNK;
            //=================End Size Link================//
            List<MSITEMBOMMTRL> BOMMTRL = new List<MSITEMBOMMTRL>();
            int count = 0;
            for (int i = 0; i <= VE.MSITEMBOMMTRL.Count - 1; i++)
            {
                if (VE.MSITEMBOMMTRL[i].Checked == false)
                {
                    count += 1;
                    MSITEMBOMMTRL item = new MSITEMBOMMTRL();
                    item = VE.MSITEMBOMMTRL[i];
                    item.PSLNO = Convert.ToByte(count);
                    BOMMTRL.Add(item);
                }
            }
            VE.MSITEMBOMMTRL = BOMMTRL;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_M_BOM_MATERIAL", VE);

        }
        public ActionResult AddRowRMPM(BOMMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            //=================For Size Link================//
            List<SizeLink> SLNK = new List<SizeLink>();
            SizeLink SLNK1 = new SizeLink(); SLNK1.text = "Yes"; SLNK1.value = "Y"; SLNK.Add(SLNK1);
            SizeLink SLNK2 = new SizeLink(); SLNK2.text = "No"; SLNK2.value = "N"; SLNK.Add(SLNK2); VE.SizeLink = SLNK;
            //=================End Size Link================//
            if (VE.MSITEMBOMMTRL_RMPM == null)
            {
                List<MSITEMBOMMTRL_RMPM> BOMMTRL = new List<MSITEMBOMMTRL_RMPM>();
                MSITEMBOMMTRL_RMPM MTRL = new MSITEMBOMMTRL_RMPM();
                MTRL.PSLNO = 1;
                BOMMTRL.Add(MTRL);
                VE.MSITEMBOMMTRL_RMPM = BOMMTRL;
            }
            else
            {
                List<MSITEMBOMMTRL_RMPM> BOMMTRL = new List<MSITEMBOMMTRL_RMPM>();
                for (int i = 0; i <= VE.MSITEMBOMMTRL_RMPM.Count - 1; i++)
                {
                    MSITEMBOMMTRL_RMPM MTRL = new MSITEMBOMMTRL_RMPM();
                    MTRL = VE.MSITEMBOMMTRL_RMPM[i];
                    BOMMTRL.Add(MTRL);
                }
                MSITEMBOMMTRL_RMPM MTRL1 = new MSITEMBOMMTRL_RMPM();
                var max = VE.MSITEMBOMMTRL_RMPM.Max(a => Convert.ToInt32(a.PSLNO));
                int SRLNO = Convert.ToInt32(max) + 1;
                MTRL1.PSLNO = Convert.ToByte(SRLNO);
                BOMMTRL.Add(MTRL1);
                VE.MSITEMBOMMTRL_RMPM = BOMMTRL;
            }
            VE.DefaultView = true;
            return PartialView("_M_BOM_RMPM", VE);

        }
        public ActionResult DeleteRowRMPM(BOMMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            //=================For Size Link================//
            List<SizeLink> SLNK = new List<SizeLink>();
            SizeLink SLNK1 = new SizeLink(); SLNK1.text = "Yes"; SLNK1.value = "Y"; SLNK.Add(SLNK1);
            SizeLink SLNK2 = new SizeLink(); SLNK2.text = "No"; SLNK2.value = "N"; SLNK.Add(SLNK2); VE.SizeLink = SLNK;
            //=================End Size Link================//
            List<MSITEMBOMMTRL_RMPM> BOMMTRL = new List<MSITEMBOMMTRL_RMPM>();
            int count = 0;
            for (int i = 0; i <= VE.MSITEMBOMMTRL_RMPM.Count - 1; i++)
            {
                if (VE.MSITEMBOMMTRL_RMPM[i].Checked == false)
                {
                    count += 1;
                    MSITEMBOMMTRL_RMPM item = new MSITEMBOMMTRL_RMPM();
                    item = VE.MSITEMBOMMTRL_RMPM[i];
                    item.PSLNO = Convert.ToByte(count);
                    BOMMTRL.Add(item);
                }
            }
            VE.MSITEMBOMMTRL_RMPM = BOMMTRL;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_M_BOM_RMPM", VE);

        }
        public ActionResult AddDOCRow(BOMMasterEntry VE)
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
        public ActionResult DeleteDOCRow(BOMMasterEntry VE)
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
        public ActionResult OpenMaterial(BOMMasterEntry VE, int SerialNo, string JOBCODE)
        {
            //=================For Size Link================//
            List<SizeLink> SLNK = new List<SizeLink>(); SizeLink SLNK1 = new SizeLink(); SLNK1.text = "Yes"; SLNK1.value = "Y"; SLNK.Add(SLNK1); SizeLink SLNK2 = new SizeLink(); SLNK2.text = "No"; SLNK2.value = "N"; SLNK.Add(SLNK2); VE.SizeLink = SLNK;
            //=================End Size Link================//
            MSITEMBOMPART query = (from c in VE.MSITEMBOMPART where (c.SLNO == SerialNo) select c).SingleOrDefault();
            if (query != null)
            {
                if (query.ChildData == null)
                {
                    List<MSITEMBOMMTRL> BOMMTRL = new List<MSITEMBOMMTRL>();
                    for (int i = 0; i < 10; i++)
                    {
                        MSITEMBOMMTRL BOMMTRL1 = new MSITEMBOMMTRL();
                        BOMMTRL1.PSLNO = Convert.ToByte(i + 1);
                        BOMMTRL1.ParentSerialNo = SerialNo;
                        BOMMTRL.Add(BOMMTRL1);
                    }
                    VE.MSITEMBOMMTRL = BOMMTRL;
                    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string JR = javaScriptSerializer.Serialize(BOMMTRL);
                    query.ChildData = JR;
                    VE.DefaultView = true;
                    VE.SERIAL = SerialNo;
                    ModelState.Clear();
                    return PartialView("_M_BOM_MATERIAL", VE);
                }
                else
                {
                    VE.SERIAL = SerialNo;
                    VE.DefaultView = true;
                    ModelState.Clear();
                    return PartialView("_M_BOM_MATERIAL", VE);
                }
            }
            else
            {
                VE.SERIAL = SerialNo;
                VE.DefaultView = true;
                ModelState.Clear();
                return PartialView("_M_BOM_MATERIAL", VE);
            }
        }
        public ActionResult CloseMaterial(BOMMasterEntry VE, FormCollection FC)
        {
            List<MSITEMBOMMTRL> BOMMTRL = new List<MSITEMBOMMTRL>();
            BOMMTRL = VE.MSITEMBOMMTRL;
            if (BOMMTRL != null)
            {
                int srl = BOMMTRL[0].ParentSerialNo;
                MSITEMBOMPART query = (from c in VE.MSITEMBOMPART where (c.SLNO == srl) select c).SingleOrDefault();
                if (query != null)
                {
                    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string JR = javaScriptSerializer.Serialize(BOMMTRL);
                    query.ChildData = JR;
                }
            }
            else
            {
                int srl = Convert.ToInt32(FC["hiddensrl"]);
                MSITEMBOMPART query = (from c in VE.MSITEMBOMPART where (c.SLNO == srl) select c).SingleOrDefault();
                if (query != null)
                {
                    query.ChildData = null;
                }
            }
            VE.DefaultView = true;
            return PartialView("_M_BOM_MAIN", VE);
        }
        public ActionResult OPEN_RMPM(BOMMasterEntry VE, int SerialNo, string JOBCODE)
        {
            //=================For Size Link================//
            List<SizeLink> SLNK = new List<SizeLink>(); SizeLink SLNK1 = new SizeLink(); SLNK1.text = "Yes"; SLNK1.value = "Y"; SLNK.Add(SLNK1); SizeLink SLNK2 = new SizeLink(); SLNK2.text = "No"; SLNK2.value = "N"; SLNK.Add(SLNK2); VE.SizeLink = SLNK;
            //=================End Size Link================//
            MSITEMBOMPART query = (from c in VE.MSITEMBOMPART where (c.SLNO == SerialNo) select c).SingleOrDefault();
            if (query != null)
            {
                if (query.ChildData_RMPM == null)
                {
                    List<MSITEMBOMMTRL_RMPM> BOMMTRL = new List<MSITEMBOMMTRL_RMPM>();
                    for (int i = 0; i < 10; i++)
                    {
                        MSITEMBOMMTRL_RMPM BOMMTRL1 = new MSITEMBOMMTRL_RMPM();
                        BOMMTRL1.PSLNO = Convert.ToByte(i + 1);
                        BOMMTRL1.ParentSerialNo = SerialNo;
                        BOMMTRL.Add(BOMMTRL1);
                    }
                    VE.MSITEMBOMMTRL_RMPM = BOMMTRL;
                    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string JR = javaScriptSerializer.Serialize(BOMMTRL);
                    query.ChildData_RMPM = JR;
                    VE.DefaultView = true;
                    VE.SERIAL = SerialNo;
                    ModelState.Clear();
                    return PartialView("_M_BOM_RMPM", VE);
                }
                else
                {
                    VE.SERIAL = SerialNo;
                    VE.DefaultView = true;
                    ModelState.Clear();
                    return PartialView("_M_BOM_RMPM", VE);
                }
            }
            else
            {
                VE.SERIAL = SerialNo;
                VE.DefaultView = true;
                ModelState.Clear();
                return PartialView("_M_BOM_RMPM", VE);
            }
        }
        public ActionResult CLOSE_RMPM(BOMMasterEntry VE, FormCollection FC)
        {
            try
            {

                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                List<MSITEMBOMMTRL_RMPM> BOMMTRL = new List<MSITEMBOMMTRL_RMPM>();
                BOMMTRL = VE.MSITEMBOMMTRL_RMPM.Where(A => A.ITCD != null).ToList();
                if (BOMMTRL != null)
                {
                    int srl = BOMMTRL[0].ParentSerialNo;
                    MSITEMBOMPART query = (from c in VE.MSITEMBOMPART where (c.SLNO == srl) select c).SingleOrDefault();
                    if (query != null)
                    {
                        var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                        string JR = javaScriptSerializer.Serialize(BOMMTRL);
                        query.ChildData_RMPM = JR;
                    }

                    if (VE.APPLY_TO_ALL_SIZE == true)
                    {
                        var SIZE_COUNT = (from c in DB.M_SIZE                                         
                                          join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                                          where (c.M_AUTONO == i.M_AUTONO && i.INACTIVE_TAG == "N" ) select new { SIZECD = c.SIZECD, SIZENM = c.SIZENM }).ToList();
                        if (SIZE_COUNT != null && SIZE_COUNT.Count > 0)
                        {
                            List<MSITEMBOMPART> BOMPART = new List<MSITEMBOMPART>(); for (int i = 0; i <= VE.MSITEMBOMPART.Count - 1; i++) { if (VE.MSITEMBOMPART[i].JOBCD != null && VE.MSITEMBOMPART[i].SIZECD != null) { MSITEMBOMPART ITEMBOMPART = new MSITEMBOMPART(); ITEMBOMPART = VE.MSITEMBOMPART[i]; BOMPART.Add(ITEMBOMPART); } }

                            var SIZE_CODE = query.SIZECD;

                            int SL_NO = 0;
                            if (BOMPART != null && BOMPART.Count > 0)
                            {
                                SIZE_CODE = SIZE_CODE + "," + string.Join(",", from x in BOMPART where x.ChildData_RMPM != null select x.SIZECD);
                                SL_NO = BOMPART.Count;
                                SIZE_COUNT = SIZE_COUNT.Where(X => !SIZE_CODE.Contains(X.SIZECD)).ToList();
                            }
                            for (int i = 0; i <= SIZE_COUNT.Count - 1; i++)
                            {
                                MSITEMBOMPART BOM_PART = new MSITEMBOMPART();
                                BOM_PART.ChildData_RMPM = query.ChildData_RMPM;
                                BOM_PART.PARTCD = query.PARTCD;
                                BOM_PART.PARTNM = query.PARTNM;
                                BOM_PART.JOBCD = query.JOBCD;
                                BOM_PART.JOBNM = query.JOBNM;
                                BOM_PART.JOBRT = query.JOBRT;
                                BOM_PART.MTRLCOST = query.MTRLCOST;
                                BOM_PART.REMARK = query.REMARK;
                                BOM_PART.SLCD1 = query.SLCD1;
                                BOM_PART.SLCD2 = query.SLCD2;
                                BOM_PART.SLCD3 = query.SLCD3;
                                BOM_PART.SLNM1 = query.SLNM1;
                                BOM_PART.SLNM2 = query.SLNM2;
                                BOM_PART.SLNM3 = query.SLNM3;
                                BOM_PART.SLNO = ++SL_NO;
                                BOM_PART.SIZECD = SIZE_COUNT[i].SIZECD;
                                BOM_PART.SIZENM = SIZE_COUNT[i].SIZENM;
                                BOMPART.Add(BOM_PART);
                            }
                            VE.MSITEMBOMPART = BOMPART;
                        }
                    }
                }
                else
                {
                    int srl = Convert.ToInt32(FC["hiddensrl"]);
                    MSITEMBOMPART query = (from c in VE.MSITEMBOMPART where (c.SLNO == srl) select c).SingleOrDefault();
                    if (query != null)
                    {
                        query.ChildData_RMPM = null;
                    }
                }
                ModelState.Clear();
                VE.DefaultView = true;
                return PartialView("_M_BOM_MAIN", VE);
            }
            catch (Exception Ex)
            {
                Cn.SaveException(Ex, "");
                return Content(Ex.Message + Ex.InnerException);
            }
        }
        public ActionResult SAVE(FormCollection FC, BOMMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            Cn.getQueryString(VE);
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    bool recomodify = false;
                    //if (VE.MENU_PARA == "OPR")
                    //{
                    //    List<MSITEMBOMSJOB> Tmplst = (List<MSITEMBOMSJOB>)TempData["MSITEMBOMSJOB_Data"];
                    //    TempData.Keep();
                    //    List<MSITEMBOMSJOB> Tmplst1 = (List<MSITEMBOMSJOB>)VE.MSITEMBOMSJOB;
                    //    var frstL1 = ((Tmplst1.Select(a => a.JOBRT).DefaultIfEmpty()).Except(Tmplst.Select(c => c.JOBRT).DefaultIfEmpty())).ToList();
                    //    var sndL1 = ((Tmplst.Select(a => a.JOBRT).DefaultIfEmpty()).Except(Tmplst1.Select(c => c.JOBRT).DefaultIfEmpty())).ToList();
                    //    var frstL2 = Tmplst1.Except(Tmplst).ToList();
                    //    var sndL2 = Tmplst.Except(Tmplst1).ToList();
                    //    if (frstL1.Any() || sndL1.Any()) recomodify = true;
                    //    if (frstL2.Any() && sndL2.Any()) if (frstL2.Count != sndL2.Count) recomodify = true;
                    //}
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_SITEMBOM ITEMBOM = new M_SITEMBOM();
                        M_CNTRL_HDR MCH = new M_CNTRL_HDR();
                        ITEMBOM.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            ITEMBOM.EMD_NO = 0;
                            ITEMBOM.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());
                            var MAXBOMCD = DB.M_SITEMBOM.Max(a => a.BOMCD);
                            if (MAXBOMCD == null)
                            {
                                ITEMBOM.BOMCD = "0000000001";
                            }
                            else
                            {
                                ITEMBOM.BOMCD = Convert.ToString(Convert.ToInt32(MAXBOMCD) + 1).ToString().PadLeft(10, '0');
                            }
                        }
                        else
                        {
                            ITEMBOM.BOMCD = VE.M_SITEMBOM.BOMCD;
                            ITEMBOM.M_AUTONO = VE.M_SITEMBOM.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_CNTRL_HDR where p.M_AUTONO == VE.M_SITEMBOM.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                ITEMBOM.EMD_NO = 0;
                            }
                            else
                            {
                                ITEMBOM.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                        }
                        ITEMBOM.EFFDT = VE.M_SITEMBOM.EFFDT;
                        ITEMBOM.ITCD = VE.M_SITEMBOM.ITCD;
                        //ITEMBOM.SIZECD = VE.M_SITEMBOM.SIZECD;
                        //ITEMBOM.COLRCD = VE.M_SITEMBOM.COLRCD;
                        ITEMBOM.BASEQNTY = VE.M_SITEMBOM.BASEQNTY;
                        ITEMBOM.REMARKS = VE.M_SITEMBOM.REMARKS;

                        MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SITEMBOM", ITEMBOM.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());

                        if (VE.DefaultAction == "A")
                        {
                            DB.M_SITEMBOM.Add(ITEMBOM);
                            DB.M_CNTRL_HDR.Add(MCH);
                            DB.SaveChanges();
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(ITEMBOM).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        }
                        if (VE.DefaultAction == "E")
                        {
                            if (VE.MENU_PARA == "MTRL")
                            {
                                DB.M_SITEMBOMPART.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.M_SITEMBOMPART.RemoveRange(DB.M_SITEMBOMPART.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD));

                                DB.M_SITEMBOMMTRL.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.M_SITEMBOMMTRL.RemoveRange(DB.M_SITEMBOMMTRL.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD));

                                ////DB.M_SITEMBOMINVMTRL.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD).ToList().ForEach(x => { x.DTAG = "E"; });
                                ////DB.M_SITEMBOMINVMTRL.RemoveRange(DB.M_SITEMBOMINVMTRL.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD));

                                DB.M_SITEMBOMSUPLR.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.M_SITEMBOMSUPLR.RemoveRange(DB.M_SITEMBOMSUPLR.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD));
                            }
                            else if (VE.MENU_PARA == "OPR")
                            {
                                DB.M_SITEMBOMSJOB.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD).ToList().ForEach(x => { x.DTAG = "E"; });
                                DB.M_SITEMBOMSJOB.RemoveRange(DB.M_SITEMBOMSJOB.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD));
                            }
                            DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SITEMBOM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SITEMBOM.M_AUTONO));

                            DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SITEMBOM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SITEMBOM.M_AUTONO));

                        }
                        #region ENTRY FOR GRID
                        if (VE.MENU_PARA == "MTRL")
                        {
                            for (int i = 0; i <= VE.MSITEMBOMPART.Count - 1; i++)
                            {
                                if (VE.MSITEMBOMPART[i].SLNO != 0 && VE.MSITEMBOMPART[i].JOBCD != null)
                                {
                                    M_SITEMBOMPART ITEMBOMPART = new M_SITEMBOMPART();
                                    ITEMBOMPART.CLCD = ITEMBOM.CLCD;
                                    ITEMBOMPART.EMD_NO = ITEMBOM.EMD_NO;
                                    ITEMBOMPART.BOMCD = ITEMBOM.BOMCD;
                                    ITEMBOMPART.EFFDT = ITEMBOM.EFFDT;
                                    ITEMBOMPART.SLNO = Convert.ToByte(VE.MSITEMBOMPART[i].SLNO);
                                    ITEMBOMPART.JOBCD = VE.MSITEMBOMPART[i].JOBCD;
                                    ITEMBOMPART.PARTCD = VE.MSITEMBOMPART[i].PARTCD;
                                    ITEMBOMPART.SIZECD = VE.MSITEMBOMPART[i].SIZECD;
                                    ITEMBOMPART.MTRLCOST = VE.MSITEMBOMPART[i].MTRLCOST;
                                    ITEMBOMPART.JOBRT = VE.MSITEMBOMPART[i].JOBRT;
                                    ITEMBOMPART.REMARK = VE.MSITEMBOMPART[i].REMARK;

                                    if (VE.MSITEMBOMPART[i].SLCD1 != null)
                                    {
                                        M_SITEMBOMSUPLR ITEMBOMSUPLR = new M_SITEMBOMSUPLR();
                                        ITEMBOMSUPLR.EMD_NO = ITEMBOMPART.EMD_NO;
                                        ITEMBOMSUPLR.CLCD = ITEMBOMPART.CLCD;
                                        ITEMBOMSUPLR.BOMCD = ITEMBOMPART.BOMCD;
                                        ITEMBOMSUPLR.EFFDT = ITEMBOMPART.EFFDT;
                                        ITEMBOMSUPLR.SLNO = Convert.ToByte(i + 1);
                                        ITEMBOMSUPLR.RSLNO = 0;
                                        ITEMBOMSUPLR.SLCD = VE.MSITEMBOMPART[i].SLCD1;
                                        ITEMBOMSUPLR.PSLNO = 1;
                                        DB.M_SITEMBOMSUPLR.Add(ITEMBOMSUPLR);
                                    }
                                    if (VE.MSITEMBOMPART[i].SLCD2 != null)
                                    {
                                        M_SITEMBOMSUPLR ITEMBOMSUPLR = new M_SITEMBOMSUPLR();
                                        ITEMBOMSUPLR.EMD_NO = ITEMBOMPART.EMD_NO;
                                        ITEMBOMSUPLR.CLCD = ITEMBOMPART.CLCD;
                                        ITEMBOMSUPLR.BOMCD = ITEMBOMPART.BOMCD;
                                        ITEMBOMSUPLR.EFFDT = ITEMBOMPART.EFFDT;
                                        ITEMBOMSUPLR.SLNO = Convert.ToByte(i + 1);
                                        ITEMBOMSUPLR.RSLNO = 0;
                                        ITEMBOMSUPLR.SLCD = VE.MSITEMBOMPART[i].SLCD2;
                                        ITEMBOMSUPLR.PSLNO = 2;
                                        DB.M_SITEMBOMSUPLR.Add(ITEMBOMSUPLR);
                                    }
                                    if (VE.MSITEMBOMPART[i].SLCD3 != null)
                                    {
                                        M_SITEMBOMSUPLR ITEMBOMSUPLR = new M_SITEMBOMSUPLR();
                                        ITEMBOMSUPLR.EMD_NO = ITEMBOMPART.EMD_NO;
                                        ITEMBOMSUPLR.CLCD = ITEMBOMPART.CLCD;
                                        ITEMBOMSUPLR.BOMCD = ITEMBOMPART.BOMCD;
                                        ITEMBOMSUPLR.EFFDT = ITEMBOMPART.EFFDT;
                                        ITEMBOMSUPLR.SLNO = Convert.ToByte(i + 1);
                                        ITEMBOMSUPLR.RSLNO = 0;
                                        ITEMBOMSUPLR.SLCD = VE.MSITEMBOMPART[i].SLCD3;
                                        ITEMBOMSUPLR.PSLNO = 3;
                                        DB.M_SITEMBOMSUPLR.Add(ITEMBOMSUPLR);
                                    }

                                    DB.M_SITEMBOMPART.Add(ITEMBOMPART);
                                    DB.SaveChanges();

                                    if (VE.MSITEMBOMPART[i].ChildData_RMPM != null)
                                    {
                                        string data = VE.MSITEMBOMPART[i].ChildData_RMPM;
                                        var helpM = new List<Improvar.Models.MSITEMBOMMTRL>();
                                        var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                        helpM = javaScriptSerializer.Deserialize<List<Improvar.Models.MSITEMBOMMTRL>>(data);
                                        for (int j = 0; j <= helpM.Count - 1; j++)
                                        {
                                            if (helpM[j].PSLNO != 0 && helpM[j].ITCD != null)
                                            {
                                                M_SITEMBOMMTRL ITEMBOMMTRL = new M_SITEMBOMMTRL();
                                                ITEMBOMMTRL.CLCD = ITEMBOM.CLCD;
                                                ITEMBOMMTRL.EMD_NO = ITEMBOM.EMD_NO;
                                                ITEMBOMMTRL.BOMCD = ITEMBOM.BOMCD;
                                                ITEMBOMMTRL.EFFDT = ITEMBOM.EFFDT;
                                                ITEMBOMMTRL.SLNO = ITEMBOMPART.SLNO;
                                                ITEMBOMMTRL.RSLNO = helpM[j].PSLNO;
                                                ITEMBOMMTRL.ITCD = helpM[j].ITCD;
                                                ITEMBOMMTRL.PARTCD = helpM[j].PARTCD;
                                                ITEMBOMMTRL.SIZECD = helpM[j].SIZECD;
                                                ITEMBOMMTRL.COLRCD = helpM[j].COLRCD;
                                                ITEMBOMMTRL.QNTY = helpM[j].QNTY;
                                                ITEMBOMMTRL.MTRLRT = helpM[j].MTRLRT;
                                                ITEMBOMMTRL.SIZE_LNK = helpM[j].SIZE_LNK;
                                                ITEMBOMMTRL.REMARK = helpM[j].REMARK;
                                                if (helpM[j].SLCD1 != null)
                                                {
                                                    M_SITEMBOMSUPLR ITEMBOMSUPLRM = new M_SITEMBOMSUPLR();
                                                    ITEMBOMSUPLRM.EMD_NO = ITEMBOMPART.EMD_NO;
                                                    ITEMBOMSUPLRM.CLCD = ITEMBOMPART.CLCD;
                                                    ITEMBOMSUPLRM.BOMCD = ITEMBOMPART.BOMCD;
                                                    ITEMBOMSUPLRM.EFFDT = ITEMBOMPART.EFFDT;
                                                    ITEMBOMSUPLRM.SLNO = Convert.ToByte(i + 1);
                                                    ITEMBOMSUPLRM.RSLNO = Convert.ToByte(j + 1);
                                                    ITEMBOMSUPLRM.SLCD = helpM[j].SLCD1;
                                                    ITEMBOMSUPLRM.PSLNO = 1;
                                                    DB.M_SITEMBOMSUPLR.Add(ITEMBOMSUPLRM);
                                                }
                                                if (helpM[j].SLCD2 != null)
                                                {
                                                    M_SITEMBOMSUPLR ITEMBOMSUPLRM = new M_SITEMBOMSUPLR();
                                                    ITEMBOMSUPLRM.EMD_NO = ITEMBOMPART.EMD_NO;
                                                    ITEMBOMSUPLRM.CLCD = ITEMBOMPART.CLCD;
                                                    ITEMBOMSUPLRM.BOMCD = ITEMBOMPART.BOMCD;
                                                    ITEMBOMSUPLRM.EFFDT = ITEMBOMPART.EFFDT;
                                                    ITEMBOMSUPLRM.SLNO = Convert.ToByte(i + 1);
                                                    ITEMBOMSUPLRM.RSLNO = Convert.ToByte(j + 1);
                                                    ITEMBOMSUPLRM.SLCD = helpM[j].SLCD2;
                                                    ITEMBOMSUPLRM.PSLNO = 2;
                                                    DB.M_SITEMBOMSUPLR.Add(ITEMBOMSUPLRM);
                                                }
                                                if (helpM[j].SLCD3 != null)
                                                {
                                                    M_SITEMBOMSUPLR ITEMBOMSUPLRM = new M_SITEMBOMSUPLR();
                                                    ITEMBOMSUPLRM.EMD_NO = ITEMBOMPART.EMD_NO;
                                                    ITEMBOMSUPLRM.CLCD = ITEMBOMPART.CLCD;
                                                    ITEMBOMSUPLRM.BOMCD = ITEMBOMPART.BOMCD;
                                                    ITEMBOMSUPLRM.EFFDT = ITEMBOMPART.EFFDT;
                                                    ITEMBOMSUPLRM.SLNO = Convert.ToByte(i + 1);
                                                    ITEMBOMSUPLRM.RSLNO = Convert.ToByte(j + 1);
                                                    ITEMBOMSUPLRM.SLCD = helpM[j].SLCD3;
                                                    ITEMBOMSUPLRM.PSLNO = 3;
                                                    DB.M_SITEMBOMSUPLR.Add(ITEMBOMSUPLRM);
                                                }
                                                DB.M_SITEMBOMMTRL.Add(ITEMBOMMTRL);
                                            }
                                        }
                                    }

                                    if (VE.MSITEMBOMPART[i].ChildData_RMPM != null)
                                    {
                                        string data = VE.MSITEMBOMPART[i].ChildData_RMPM;
                                        var helpM = new List<Improvar.Models.MSITEMBOMMTRL_RMPM>();
                                        var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                                        helpM = javaScriptSerializer.Deserialize<List<Improvar.Models.MSITEMBOMMTRL_RMPM>>(data);
                                        for (int j = 0; j <= helpM.Count - 1; j++)
                                        {
                                            if (helpM[j].PSLNO != 0 && helpM[j].ITCD != null)
                                            {
                                                //M_SITEMBOMMTRL INV_ITEMBOMMTRL = new M_SITEMBOMMTRL();
                                                //INV_ITEMBOMMTRL.CLCD = ITEMBOM.CLCD;
                                                //INV_ITEMBOMMTRL.EMD_NO = ITEMBOM.EMD_NO;
                                                //INV_ITEMBOMMTRL.BOMCD = ITEMBOM.BOMCD;
                                                //INV_ITEMBOMMTRL.EFFDT = ITEMBOM.EFFDT;
                                                //INV_ITEMBOMMTRL.SLNO = ITEMBOMPART.SLNO;
                                                //INV_ITEMBOMMTRL.RSLNO = Convert.ToByte(helpM[j].PSLNO);
                                                //INV_ITEMBOMMTRL.ITCD = helpM[j].ITCD;
                                                //INV_ITEMBOMMTRL.QNTY = helpM[j].QNTY;
                                                //INV_ITEMBOMMTRL.MTRLRT = helpM[j].MTRLRT;
                                                //INV_ITEMBOMMTRL.SIZE_LNK = helpM[j].SIZE_LNK;
                                                //INV_ITEMBOMMTRL.REMARK = helpM[j].REMARK;
                                                //DB.M_SITEMBOMMTRL.Add(INV_ITEMBOMMTRL);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (VE.MENU_PARA == "OPR")
                        {
                            for (int i = 0; i <= VE.MSITEMBOMSJOB.Count - 1; i++)
                            {
                                if (VE.MSITEMBOMSJOB[i].SLNO != 0 && VE.MSITEMBOMSJOB[i].SJOBCD != null)
                                {
                                    M_SITEMBOMSJOB ITEMBOMSJOB = new M_SITEMBOMSJOB();
                                    ITEMBOMSJOB.CLCD = ITEMBOM.CLCD;
                                    ITEMBOMSJOB.EMD_NO = ITEMBOM.EMD_NO;
                                    ITEMBOMSJOB.BOMCD = ITEMBOM.BOMCD;
                                    ITEMBOMSJOB.EFFDT = ITEMBOM.EFFDT;
                                    ITEMBOMSJOB.SLNO = VE.MSITEMBOMSJOB[i].SLNO;
                                    ITEMBOMSJOB.JOBCD = VE.MSITEMBOMSJOB[i].JOBCD;
                                    ITEMBOMSJOB.SJOBCD = VE.MSITEMBOMSJOB[i].SJOBCD;
                                    ITEMBOMSJOB.PARTCD = VE.MSITEMBOMSJOB[i].PARTCD;
                                    ITEMBOMSJOB.JOBRT = VE.MSITEMBOMSJOB[i].JOBRT;
                                    ITEMBOMSJOB.REMARK = VE.MSITEMBOMSJOB[i].REMARK;
                                    ITEMBOMSJOB.SEQORDNO = VE.MSITEMBOMSJOB[i].SEQORDNO;
                                    ITEMBOMSJOB.LENCM = VE.MSITEMBOMSJOB[i].LENCM;
                                    ITEMBOMSJOB.SMV = VE.MSITEMBOMSJOB[i].SMV;
                                    ITEMBOMSJOB.TRATIO = VE.MSITEMBOMSJOB[i].TRATIO;
                                    ITEMBOMSJOB.MCCD = VE.MSITEMBOMSJOB[i].MCCD;
                                    DB.M_SITEMBOMSJOB.Add(ITEMBOMSJOB);
                                }
                            }
                        }
                        #endregion
                        if (VE.UploadDOC != null)
                        {
                            var img = Cn.SaveUploadImage("M_SITEMBOM", VE.UploadDOC, ITEMBOM.M_AUTONO, ITEMBOM.EMD_NO.Value);
                            if (img.Item1.Count != 0)
                            {
                                DB.M_CNTRL_HDR_DOC.AddRange(img.Item1);
                                DB.M_CNTRL_HDR_DOC_DTL.AddRange(img.Item2);
                            }
                        }
                        DB.SaveChanges();
                        #region //update transaction receive and issue details
                        if (VE.MENU_PARA == "OPR")
                        {
                            if (recomodify == true)
                            {
                                // transaction amount received update
                                string scm1 = CommVar.CurSchema(UNQSNO).ToString();
                                string batchnos = "";
                                string sql = "";
                                sql = "select a.batchno, c.emd_no, c.clcd, a.autono, c.docdt, a.slno, a.itcd, a.partcd, c.jobcd, d.jobprccd ";
                                sql += "from " + scm1 + ".t_inhissmst a, " + scm1 + ".t_inhlotclshdr b, " + scm1 + ".t_inhiss c, " + scm1 + ".m_flrloca d ";
                                sql += "where a.batchno = b.batchno(+) and b.batchno is null and a.autono=c.autono(+) and c.flrcd=d.flrcd(+) and a.itcd='" + ITEMBOM.ITCD.ToString() + "'";
                                DataTable rsBatch = Master_Help.SQLquery(sql);

                                //update issue table for sjob
                                Int32 maxR = rsBatch.Rows.Count - 1;
                                Int32 y = 0;
                                while (y <= maxR)
                                {
                                    batchnos += "'" + rsBatch.Rows[y]["batchno"].ToString() + "',";
                                    y++;
                                }
                                if (batchnos != "") batchnos = batchnos.Substring(0, batchnos.Length - 1);

                                y = 0;
                                while (y <= maxR)
                                {
                                    DateTime DDATE = Convert.ToDateTime(rsBatch.Rows[y]["docdt"].ToString().Substring(0, 10));
                                    string partcd = rsBatch.Rows[y]["partcd"].ToString();
                                    string jobcd = rsBatch.Rows[y]["jobcd"].ToString();
                                    string jobprccd = rsBatch.Rows[y]["jobprccd"].ToString();

                                    var query = (from c in DB.V_SJOBMST_STDRT
                                                 where c.ITCD == ITEMBOM.ITCD && c.PARTCD == partcd && c.BOMEFFDT <= DDATE && c.JOBCD == jobcd && c.JOBPRCCD == jobprccd
                                                 select new
                                                 {
                                                     SJOBCD = c.SJOBCD,
                                                     JOBRT = c.FIXJOBRT,
                                                     JOBRTEXT = c.JOBRT,
                                                     SEQORDNO = c.SEQORDNO,
                                                     BOMEFFDT = c.BOMEFFDT
                                                 }).DistinctBy(x => x.SJOBCD).ToList();

                                    if (query.Count == 0)
                                    {
                                        transaction.Rollback();
                                        return Content("Operation details not in this item");
                                    }

                                    string autono = rsBatch.Rows[y]["autono"].ToString();
                                    DB.T_INHISSMSTSJOB.RemoveRange(DB.T_INHISSMSTSJOB.Where(x => x.AUTONO == autono));
                                    DB.SaveChanges();

                                    for (int z = 0; z <= query.Count - 1; z++)
                                    {
                                        T_INHISSMSTSJOB tinhsjob = new T_INHISSMSTSJOB();
                                        tinhsjob.EMD_NO = Convert.ToSByte(rsBatch.Rows[y]["emd_no"]);
                                        tinhsjob.CLCD = rsBatch.Rows[y]["clcd"].ToString();
                                        tinhsjob.AUTONO = rsBatch.Rows[y]["autono"].ToString();
                                        tinhsjob.SLNO = Convert.ToSByte(rsBatch.Rows[y]["slno"]);
                                        tinhsjob.SJOBCD = query[z].SJOBCD;
                                        ////tinhsjob.JBRT = query[z].JOBRT;
                                        ////tinhsjob.JBRTEXT = query[z].JOBRTEXT;
                                        tinhsjob.SEQORDNO = query[z].SEQORDNO;
                                        DB.T_INHISSMSTSJOB.Add(tinhsjob);
                                    }
                                    y = y + 1;
                                    DB.SaveChanges();
                                }
                                if (batchnos != "")
                                {
                                    // select a.autono, a.batchno, a.sjobcd
                                    //from sd_hpcc2018.t_inhrecmst a
                                    //where a.batchno in ('0118S0608004','0118S0615003') and a.batchno || a.sjobcd not in 
                                    //(select b.batchno || c.sjobcd from
                                    //    sd_hpcc2018.t_inhissmst b, sd_hpcc2018.t_inhissmstsjob c
                                    //where b.autono = c.autono(+) and b.batchno in ('0118S0608004','0118S0615003') )
                                    var batchnot = batchnos.Replace("'", "");
                                    var btch = batchnot.Split(',');
                                    var q2 = (from b in DB.T_INHISSMST
                                              join c in DB.T_INHISSMSTSJOB on b.AUTONO equals c.AUTONO
                                              where btch.Contains(b.BATCHNO)
                                              select new
                                              {
                                                  BATCHNO = b.BATCHNO,
                                                  SJOBCD = c.SJOBCD
                                              }).ToList();

                                    var q1 = (from a in DB.T_INHRECMST
                                              where btch.Contains(a.BATCHNO)
                                              select new tempBtach
                                              {
                                                  AUTONO = a.AUTONO,
                                                  BATCHNO = a.BATCHNO,
                                                  SJOBCD = a.SJOBCD
                                              }).ToList();


                                    List<tempBtach> tempBtac = new List<tempBtach>();
                                    foreach (var b in q1)
                                    {
                                        foreach (var b1 in q2)
                                        {
                                            if (b.BATCHNO == b1.BATCHNO && b.SJOBCD == b1.SJOBCD)
                                            {
                                                tempBtach tempBta = new tempBtach();
                                                tempBta.AUTONO = b.AUTONO;
                                                tempBta.BATCHNO = b.BATCHNO;
                                                tempBta.SJOBCD = b.SJOBCD;
                                                tempBtac.Add(tempBta);
                                            }
                                            else
                                            {

                                            }
                                        }
                                    }
                                    var sndL11 = ((q1.Select(a => a.AUTONO).DefaultIfEmpty()).Except(tempBtac.Select(c => c.AUTONO).DefaultIfEmpty())).ToList();
                                    //If get type of record here . all record should be display. and roll back
                                    if (sndL11.Any())
                                    {
                                        string st = "";
                                        foreach (var v1 in sndL11)
                                        {
                                            st += " Auto No.:" + v1.ToString();
                                        }
                                        transaction.Rollback();
                                        return Content("Need to delete these vouchers in receive module.<br/> which have data of removed operations.<br/>" + st);
                                    }

                                    // update t_inhrecmst
                                    sql = "update " + scm1 + ".t_inhrecmst t1 set (rate, jobextrate, amt, jobextamt) = ";
                                    sql += "(select a.jbrt, a.jbrtext, round(a.jbrt*a.pcs,2) amt, round(a.jbrtext*a.pcs,2) extamt from ";
                                    sql += "(select a.autono, b.batchno, b.sjobcd, d.jbrt, d.jbrtext, sum(a.stkqnty) pcs ";
                                    sql += "from " + scm1 + ".t_inhrecmstdtl a, " + scm1 + ".t_inhrecmst b, " + scm1 + ".t_inhissmst c, " + scm1 + ".t_inhissmstsjob d ";
                                    sql += "where a.autono=b.autono and b.batchno=c.batchno and c.autono=d.autono and b.sjobcd=d.sjobcd and ";
                                    sql += "b.batchno in (" + batchnos + ") ";
                                    sql += "group by a.autono, b.batchno, b.sjobcd, d.jbrt, d.jbrtext ) a ";
                                    sql += "where t1.batchno=a.batchno and t1.autono=a.autono and t1.sjobcd=a.sjobcd ) ";
                                    sql += "where t1.batchno in (" + batchnos + ") ";
                                    DB.Database.ExecuteSqlCommand(sql);
                                }
                            }
                        }
                        #endregion // eof transaction updation if any change in bom operations
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
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SITEMBOM", VE.M_SITEMBOM.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_SITEMBOMPART.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_SITEMBOMMTRL.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        ////DB.M_SITEMBOMINVMTRL.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_SITEMBOMSUPLR.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();
                        DB.M_SITEMBOMSJOB.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SITEMBOM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SITEMBOM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_SITEMBOM.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SITEMBOM.M_AUTONO));
                        DB.SaveChanges();

                        DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SITEMBOM.M_AUTONO));
                        DB.SaveChanges();

                        ////DB.M_SITEMBOMINVMTRL.RemoveRange(DB.M_SITEMBOMINVMTRL.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD));
                        ////DB.SaveChanges();

                        DB.M_SITEMBOMSUPLR.RemoveRange(DB.M_SITEMBOMSUPLR.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD));
                        DB.SaveChanges();

                        DB.M_SITEMBOMMTRL.RemoveRange(DB.M_SITEMBOMMTRL.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD));
                        DB.SaveChanges();

                        DB.M_SITEMBOMPART.RemoveRange(DB.M_SITEMBOMPART.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD));
                        DB.SaveChanges();

                        DB.M_SITEMBOMSJOB.RemoveRange(DB.M_SITEMBOMSJOB.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD));
                        DB.SaveChanges();

                        DB.M_SITEMBOM.RemoveRange(DB.M_SITEMBOM.Where(x => x.BOMCD == VE.M_SITEMBOM.BOMCD));
                        DB.SaveChanges();

                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_SITEMBOM.M_AUTONO));
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
                    return Content(ex.Message);
                }
            }
        }
        [HttpPost]
        public ActionResult M_BOM(FormCollection FC, BOMMasterEntry VE)
        {
            try
            {
                string Bomcd1 = VE.M_SITEMBOM.BOMCD.ToString();
                string effdt1 = Convert.ToString(Convert.ToDateTime(VE.M_SITEMBOM.EFFDT.ToString())).Substring(0, 10);
                string Scm1 = CommVar.CurSchema(UNQSNO);
                string PartCluse = "";
                if (VE.MSITEMBOM_PRINT != null && VE.MSITEMBOM_PRINT.Count > 0)
                {
                    for (int z = 0; z <= VE.MSITEMBOM_PRINT.Count - 1; z++)
                    {
                        if (VE.MSITEMBOM_PRINT[z].Checked == true)
                        {
                            PartCluse = PartCluse + "'" + VE.MSITEMBOM_PRINT[z].PARTCD + "',";
                        }
                    }
                    if (PartCluse != "")
                    {
                        PartCluse = PartCluse.Substring(0, PartCluse.Length - 1);
                        PartCluse = " and b.partcd in (" + PartCluse + " ) ";
                    }
                }
                string query = "";
                query = "select a.effdt,a.itcd,a.bomcd,a.sizecd,a.colrcd, a.remarks, b.remark, to_char(e.m_autono) sjautono, to_char(f.m_autono) itmautono, ";
                query = query + "b.slno,b.partcd, nvl(b.jobrt,0) jobrt, b.jobcd,b.seqordno, f.styleno, ";
                query = query + "c.partnm,d.jobnm,e.sjobnm,e.sjobstyle, e.sjobmchn, f.itnm,b.sjobcd, ";
                query = query + "(select nvl(jobrt,0) jobrt from " + Scm1 + ".m_jobmstsub_stdrt mk ";
                query = query + "where mk.sjobcd = b.sjobcd and mk.jobprccd = 'STD' and mk.effdt = ";
                query = query + "(select max(effdt) from " + Scm1 + ".m_jobmstsub_stdrt ";
                query = query + "where sjobcd = b.sjobcd and effdt <= a.effdt )) actrt ";
                query = query + " from " + Scm1 + ".m_sitembom a, " + Scm1 + ".m_sitembomsjob b, " + Scm1 + ".m_parts c, " + Scm1 + ".m_jobmst d,";
                query = query + Scm1 + ".m_jobmstsub e, " + Scm1 + ".m_sitem f ";
                query = query + "where a.bomcd = b.bomcd(+) and a.effdt = b.effdt(+) and b.partcd = c.partcd(+) and b.jobcd = d.jobcd(+) and b.sjobcd = e.sjobcd(+) and ";
                query = query + "a.itcd = f.itcd and a.bomcd = '" + Bomcd1 + "' and a.effdt = to_date('" + effdt1 + "','dd/mm/yyyy') ";
                query = query + PartCluse;
                query = query + "order by jobcd,partcd,seqordno";

                string query1 = "select effdt,itcd,bomcd,sizecd,colrcd,remarks, remark, slno,partcd,jobrt,jobcd,seqordno,partnm,jobnm,sjobnm,itnm,sjobcd,actrt,actrt + jobrt totrt ";
                query1 = query1 + " from ( " + query + " ) order by jobcd,effdt,seqordno ";

                CS = Cn.GetConnectionString();

                Cn.con = new OracleConnection(CS);
                if ((Cn.ds.Tables["mst_rep"] == null) == false)
                {
                    Cn.ds.Tables["mst_rep"].Clear();
                }
                if (Cn.con.State == ConnectionState.Closed)
                {
                    Cn.con.Open();
                }

                Cn.com = new OracleCommand(query, Cn.con);
                Cn.da.SelectCommand = Cn.com;
                bool bu = Convert.ToBoolean(Cn.da.Fill(Cn.ds, "mst_rep"));
                Cn.con.Close();
                if (!bu)
                {
                    return RedirectToAction("NoRecords", "RPTViewer");
                }

                DataTable IR = new DataTable("mstrep");
                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                ImageData ID = new ImageData();

                HC.RepStart(IR, 3);
                HC.GetPrintHeader(IR, "slno", "double", "n,4", "Sl.No.");
                HC.GetPrintHeader(IR, "sjobcd", "string", "c,9", "SJob Code");
                HC.GetPrintHeader(IR, "image_1", "string", "c,8", "Image");
                HC.GetPrintHeader(IR, "sjobnm", "string", "c,35", "Sub Job Name");
                HC.GetPrintHeader(IR, "sjobstyle", "string", "c,20", "Style");
                HC.GetPrintHeader(IR, "sjobmchn", "string", "c,20", "Machine");
                HC.GetPrintHeader(IR, "rate", "double", "n,8:###0.00", "Rate");
                HC.GetPrintHeader(IR, "remark", "string", "c,20", "Remarks");

                ID.GetImageDataStarts();
                ID.GetImageData("'PRODUCT'", "S", "'M_JOBMSTSUB','M_SITEM'", "'" + Cn.ds.Tables["mst_rep"].Rows[0]["itmautono"].ToString() + "'");
                string itmimg = "";
                decimal rate1 = 0;
                decimal rate = 0;

                DataRow dr1 = IR.NewRow();
                itmimg = "";
                itmimg = ID.FindImageData("S", Cn.ds.Tables["mst_rep"].Rows[0]["itmautono"].ToString());
                if (itmimg != "")
                {
                    itmimg = "<IMG SRC='" + itmimg + "' name='HOOK' HEIGHT='60px' WIDTH='60px'/> &nbsp;&nbsp;&nbsp;";
                }
                dr1["Dammy"] = itmimg + "  Style " + Cn.ds.Tables["mst_rep"].Rows[0]["styleno"] + "   [ " + Cn.ds.Tables["mst_rep"].Rows[0]["partnm"] + "  ]  [" + Cn.ds.Tables["mst_rep"].Rows[0]["itcd"] + "]";
                dr1["Flag"] = " font-weight:bold;font-size:13px;";
                IR.Rows.Add(dr1);

                DataRow dr2 = IR.NewRow();
                dr2["Dammy"] = Cn.ds.Tables["mst_rep"].Rows[0]["itnm"];
                dr2["Flag"] = " font-weight:bold;font-size:13px;";
                IR.Rows.Add(dr2);

                string partcd = "";
                int i = 0;
                int sln = 0;
                int noparts = 0;
                long maxI = Cn.ds.Tables["mst_rep"].Rows.Count - 1;
                while (i <= maxI)
                {
                    noparts = noparts + 1;
                    DataRow dr11 = IR.NewRow();
                    dr11["Dammy"] = (Cn.ds.Tables["mst_rep"].Rows[i]["partnm"]);
                    dr11["Flag"] = " font-weight:bold;font-size:13px;";
                    IR.Rows.Add(dr11);
                    partcd = Cn.ds.Tables["mst_rep"].Rows[i]["partcd"].ToString();
                    sln = 0;
                    rate = 0;
                    while (Cn.ds.Tables["mst_rep"].Rows[i]["partcd"].ToString() == partcd)
                    {
                        DataRow dr = IR.NewRow();
                        sln = sln + 1;
                        dr["slno"] = sln;
                        dr["sjobcd"] = (Cn.ds.Tables["mst_rep"].Rows[i]["sjobcd"]);
                        dr["image_1"] = ID.FindImageData("S", Cn.ds.Tables["mst_rep"].Rows[i]["sjautono"].ToString());
                        dr["sjobnm"] = (Cn.ds.Tables["mst_rep"].Rows[i]["sjobnm"]);
                        dr["sjobstyle"] = (Cn.ds.Tables["mst_rep"].Rows[i]["sjobstyle"]);
                        dr["sjobmchn"] = (Cn.ds.Tables["mst_rep"].Rows[i]["sjobmchn"]);
                        dr["remark"] = (Cn.ds.Tables["mst_rep"].Rows[i]["remark"]);
                        Decimal actrt1 = Cn.ds.Tables["mst_rep"].Rows[i]["actrt"] == DBNull.Value ? 0 : Convert.ToDecimal(Cn.ds.Tables["mst_rep"].Rows[i]["actrt"]);

                        dr["rate"] = actrt1 + Convert.ToDecimal(Cn.ds.Tables["mst_rep"].Rows[i]["jobrt"]);
                        dr["celldesign"] = "sjobnm=font-weight:bold;font_size=13px;^rate=font-weight:bold;font_size=13px;";
                        IR.Rows.Add(dr);
                        rate = rate + actrt1 + Convert.ToDecimal(Cn.ds.Tables["mst_rep"].Rows[i]["jobrt"]);
                        rate1 = rate1 + actrt1 + Convert.ToDecimal(Cn.ds.Tables["mst_rep"].Rows[i]["jobrt"]);
                        i = i + 1;
                        if (i > maxI) break;
                    }
                    DataRow dr13 = IR.NewRow();
                    dr13["dammy"] = "";
                    dr13["sjobnm"] = "Total [" + (Cn.ds.Tables["mst_rep"].Rows[i - 1]["partnm"].ToString()) + "]";
                    dr13["rate"] = rate;
                    dr13["Flag"] = "font-weight:bold;font-size:14px;border-top: 2px solid;border-bottom: 3px solid;";
                    IR.Rows.Add(dr13);
                    DataRow dr211 = IR.NewRow();
                    dr211["dammy"] = " ";
                    dr211["flag"] = " height:14px; ";
                    IR.Rows.Add(dr211);
                }
                if (noparts > 1)
                {
                    DataRow dr3 = IR.NewRow();
                    dr3["dammy"] = "";
                    dr3["sjobnm"] = "Total of All Parts";
                    dr3["rate"] = rate1;
                    dr3["Flag"] = "font-weight:bold;font-size:14px;border-top: 2px solid;border-bottom: 3px solid;";
                    IR.Rows.Add(dr3);
                }

                DataRow dr21 = IR.NewRow();
                dr21["dammy"] = " ";
                dr21["flag"] = " height:14px; ";
                IR.Rows.Add(dr21);

                DataRow dr22 = IR.NewRow();
                dr22["dammy"] = " ";
                dr22["flag"] = " height:14px; ";
                IR.Rows.Add(dr22);

                DataRow dr23 = IR.NewRow();
                dr23["dammy"] = " ";
                dr23["flag"] = " height:14px; ";
                IR.Rows.Add(dr23);

                DataRow dr31 = IR.NewRow();
                dr31["dammy"] = "";
                dr31["sjobnm"] = "Checked By";
                dr31["sjobmchn"] = "Approved By";
                dr31["Flag"] = "font-weight:bold;font-size:13px;border-top: 2px solid;border-bottom: 3px solid;";
                IR.Rows.Add(dr31);

                string pghdr1 = ""; pghdr1 = "Stiching Operation and Rate Chart";
                string repname = CommFunc.retRepname("peration_Rate_Chart");
                PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
        }
        public class tempBtach
        {
            public string AUTONO { get; set; }
            public string BATCHNO { get; set; }
            public string SJOBCD { get; set; }
        }
        public ActionResult GetOperationData(BOMMasterEntry VE, string ITEM)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            VE.MSITEMBOMSJOB = (from i in DB.M_SITEMBOMSJOB
                                join j in DB.M_PARTS on i.PARTCD equals j.PARTCD into x
                                from j in x.DefaultIfEmpty()
                                join k in DB.M_JOBMSTSUB on i.SJOBCD equals k.SJOBCD into y
                                from k in y.DefaultIfEmpty()
                                join l in DB.M_JOBMST on k.JOBCD equals l.JOBCD into z
                                from l in z.DefaultIfEmpty()
                                join m in DB.M_MACHINE on i.MCCD equals m.MCCD into p
                                from m in p.DefaultIfEmpty()
                                join n in DB.M_SITEMBOM on i.BOMCD equals n.BOMCD into q
                                from n in q.DefaultIfEmpty()
                                where (i.BOMCD == n.BOMCD && n.ITCD == ITEM)
                                select new MSITEMBOMSJOB()
                                {
                                    BOMCD = i.BOMCD,
                                    EFFDT = i.EFFDT,
                                    SLNO = i.SLNO,
                                    PARTCD = i.PARTCD,
                                    PARTNM = j.PARTNM,
                                    SJOBCD = i.SJOBCD,
                                    SJOBNM = k.SJOBNM,
                                    SEQORDNO = i.SEQORDNO,
                                    JOBCD = k.JOBCD,
                                    JOBNM = l.JOBNM,
                                    JOBRT = i.JOBRT,
                                    REMARK = i.REMARK,
                                    LENCM = i.LENCM,
                                    SMV = i.SMV,
                                    TRATIO = i.TRATIO,
                                    MCCD = i.MCCD,
                                    MCNM = m.MCNM
                                }).OrderBy(s => s.SLNO).ToList();

            if (VE.MSITEMBOMSJOB != null && VE.MSITEMBOMSJOB.Count > 0)
            {
                VE.DefaultView = true;
                ModelState.Clear();
                return PartialView("_M_BOM_OPERATION", VE);
            }
            else
            {
                return Content("NODATA");
            }
        }
        public ActionResult Check_Copy_Size(BOMMasterEntry VE, int P_SLNO, string val)
        {
            try
            {
                var MAIN_GRID_DATA = (from z in VE.MSITEMBOMPART where z.PARTCD != null && z.JOBCD != null && z.SIZECD != null select z).ToList();
                if (MAIN_GRID_DATA != null && MAIN_GRID_DATA.Count > 0)
                {
                    string PART_CODE = (from X in VE.MSITEMBOMPART where X.SLNO == P_SLNO select X.PARTCD).SingleOrDefault();
                    string JOB_CODE = (from X in VE.MSITEMBOMPART where X.SLNO == P_SLNO select X.JOBCD).SingleOrDefault();
                    var query = (from z in VE.MSITEMBOMPART where z.PARTCD == PART_CODE && z.JOBCD == JOB_CODE && z.SIZECD == val select z).ToList();
                    if (query.Any())
                    {
                        return Content("1");
                    }
                    else
                    {
                        return Content("Invalid Size Code ! Please Enter a Valid Size Code !!");
                    }
                }
                else
                {
                    return Content("Data Not Available");
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetSizeWiseItemData(BOMMasterEntry VE, string SIZE_CODE, int P_SLNO)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            List<SizeLink> SLNK = new List<SizeLink>(); SizeLink SLNK1 = new SizeLink(); SLNK1.text = "Yes"; SLNK1.value = "Y"; SLNK.Add(SLNK1); SizeLink SLNK2 = new SizeLink(); SLNK2.text = "No"; SLNK2.value = "N"; SLNK.Add(SLNK2); VE.SizeLink = SLNK;
            if (VE.MSITEMBOMPART != null && VE.MSITEMBOMPART.Count > 0)
            {
                var SIZE_CHILD_DATA = (from X in VE.MSITEMBOMPART where X.SIZECD == SIZE_CODE && X.ChildData_RMPM != null select X.ChildData_RMPM).SingleOrDefault();
                if (SIZE_CHILD_DATA != null && SIZE_CHILD_DATA.Any())
                {
                    var helpMT = new List<Improvar.Models.MSITEMBOMMTRL_RMPM>();
                    var javaScriptSerializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    helpMT = javaScriptSerializer.Deserialize<List<Improvar.Models.MSITEMBOMMTRL_RMPM>>(SIZE_CHILD_DATA);
                    if (helpMT != null && helpMT.Count > 0)
                    {
                        helpMT.ForEach(A => { A.ParentSerialNo = P_SLNO; });
                    }
                    VE.MSITEMBOMMTRL_RMPM = helpMT;
                }
                else
                {
                    return Content("NODATA");
                }
            }
            if (VE.MSITEMBOMMTRL_RMPM != null && VE.MSITEMBOMMTRL_RMPM.Count > 0)
            {
                VE.DefaultView = true;
                ModelState.Clear();
                return PartialView("_M_BOM_RMPM", VE);
            }
            else
            {
                return Content("NODATA");
            }
        }
        public ActionResult CheckSizeCode(BOMMasterEntry VE, string SIZE, string PART, string JOB)
        {
            try
            {
                var CHECK_DATA = (from X in VE.MSITEMBOMPART where X.SIZECD == SIZE && X.PARTCD == PART && X.JOBCD == JOB select X).ToList();
                if (CHECK_DATA != null && CHECK_DATA.Count > 1)
                {
                    return Content("Invalid Size Code ! Size Already Exists with Same Job & Part , Please Enter a Valid / Different Size Code !!");
                }
                else
                {
                    return Content("0");
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetMaterialDetails(string val)
        {
            try
            {
                var str = Master_Help.MTRLJOBCD_help(val);
                if (str.IndexOf("='helpmnu'") >= 0)
                {
                    return PartialView("_Help2", str);
                }
                else
                {
                    return Content(str);
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

