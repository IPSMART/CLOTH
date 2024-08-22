using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using ClosedXML.Excel;
using System.IO;

namespace Improvar.Controllers
{
    public class M_ScmItemGrpController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp Master_Help = new MasterHelp();
        MasterHelpFa Master_HelpFa = new MasterHelpFa();
        M_SCMITMGRP_HDR sl;
        M_SCMITMGRP sl1;
        M_CNTRL_HDR sll;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_ScmItemGrp
        public ActionResult M_ScmItemGrp(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Scheme Item Group";
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    SchmItemGroupMaster VE = new SchmItemGroupMaster();
                    string loca1 = CommVar.Loccd(UNQSNO).Trim();
                    ImprovarDB DBIMP = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    SchemeCal SchemeCal = new SchemeCal();

                    if (op.Length != 0)
                    {
                        string GCS = Cn.GCS();
                        string scm = CommVar.CurSchema(UNQSNO);
                        string compcd = CommVar.Compcd(UNQSNO);
                        string LOC = CommVar.Loccd(UNQSNO);
                        string str = "select distinct a.SCMITMGRPCD from " + scm + ".M_SCMITMGRP_HDR a," + scm + ".m_cntrl_hdr b, " + scm + ".m_cntrl_loca c ";
                        str += "where a.m_autono=b.m_autono and a.m_autono=c.m_autono(+)  ";
                        str += "and (c.compcd='" + compcd + "' or c.compcd is null) and (c.loccd='" + LOC + "' or c.loccd is null) order by a.SCMITMGRPCD ";
                        DataTable tbl = Master_Help.SQLquery(str);
                        VE.IndexKey = (from DataRow dr in tbl.Rows select new IndexKey() { Navikey = dr["SCMITMGRPCD"].retStr() }).ToList();


                        //VE.IndexKey = (from p in DB.M_SCMITMGRP_HDR select new IndexKey() { Navikey = p.SCMITMGRPCD }).ToList();
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
                            VE.M_SCMITMGRP_HDR = sl;
                            VE.M_CNTRL_HDR = sll;
                        }

                        if (VE.DefaultAction == "A")
                        {
                            M_SCMITMGRP_HDR obj = new M_SCMITMGRP_HDR();
                            VE.M_SCMITMGRP_HDR = obj;

                            List<MSCMITMGRP> SCMITMGRP_OBJ = new List<MSCMITMGRP>();
                            for (int j = 0; j < 10; j++)
                            {
                                MSCMITMGRP SCMITMGRP_OBJ1 = new MSCMITMGRP();
                                SCMITMGRP_OBJ1.SLNO = Convert.ToInt16(j + 1);
                                SCMITMGRP_OBJ.Add(SCMITMGRP_OBJ1);
                            }
                            VE.MSCMITMGRP = SCMITMGRP_OBJ;

                            List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                            UploadDOC UPL = new UploadDOC();
                            UPL.DocumentType = doctP;
                            UploadDOC1.Add(UPL);
                            VE.UploadDOC = UploadDOC1;
                        }
                        VE.CompanyLocationName = Cn.GetCompanyLocation(VE.DefaultAction, scm, Convert.ToInt32(VE.M_SCMITMGRP_HDR.M_AUTONO));

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
                SchmItemGroupMaster VE = new SchmItemGroupMaster();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public SchmItemGroupMaster Navigation(SchmItemGroupMaster VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new M_SCMITMGRP_HDR();
            sll = new M_CNTRL_HDR();
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
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
                sl = DB.M_SCMITMGRP_HDR.Find(aa[0].Trim());
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
                //SchemeCal scmcal = new SchemeCal();
                //DataTable rstmp = scmcal.GenScmItmGrpData("'" + sl.SCMITMGRPCD + "'");
                //VE.MSCMITEMGRPDATA = (from DataRow dr in rstmp.Rows
                //                      select new MSCMITEMGRPDATA()
                //                      {
                //                          BRANDNM = dr["brandnm"].ToString(),
                //                          SBRANDNM = dr["sbrandnm"].ToString(),
                //                          COLLNM = dr["collnm"].ToString(),
                //                          ITGRPCD = dr["itgrpcd"].ToString(),
                //                          ITGRPNM = dr["itgrpnm"].ToString(),
                //                          STYLENO = dr["styleno"].ToString(),
                //                          ITCD = dr["itcd"].ToString(),
                //                          ITNM = dr["itnm"].ToString(),
                //                          SIZENM = dr["sizenm"].ToString(),
                //                          PRINT_SEQ = dr["print_seq"].ToString(),
                //                          COLRNM = dr["colrnm"].ToString(),
                //                      }).ToList();

                VE.MSCMITMGRP = (from j in DB.M_SCMITMGRP
                                 join k in DB.M_BRAND on j.BRANDCD equals (k.BRANDCD) into brand
                                 from k in brand.DefaultIfEmpty()
                                 join l in DB.M_SUBBRAND on j.SBRANDCD equals (l.SBRANDCD) into sbrand
                                 from l in sbrand.DefaultIfEmpty()
                                 join m in DB.M_GROUP on j.ITGRPCD equals (m.ITGRPCD) into itgrp
                                 from m in itgrp.DefaultIfEmpty()
                                 join n in DB.M_COLLECTION on j.COLLCD equals (n.COLLCD) into coll
                                 from n in coll.DefaultIfEmpty()
                                 join o in DB.M_SITEM on j.ITCD equals (o.ITCD) into item
                                 from o in item.DefaultIfEmpty()
                                 join p in DB.M_SIZE on j.SIZECD equals (p.SIZECD) into size
                                 from p in size.DefaultIfEmpty()
                                 join q in DB.M_COLOR on j.COLRCD equals (q.COLRCD) into color
                                 from q in color.DefaultIfEmpty()
                                 where (j.SCMITMGRPCD == sl.SCMITMGRPCD)
                                 select new MSCMITMGRP()
                                 {
                                     BRANDCD = j.BRANDCD,
                                     BRANDNM = k.BRANDNM,
                                     COLLCD = j.COLLCD,
                                     COLLNM = n.COLLNM,
                                     SBRANDCD = j.SBRANDCD,
                                     SBRANDNM = l.SBRANDNM,
                                     COLRCD = j.COLRCD,
                                     COLRNM = q.COLRNM,
                                     ITCD = j.ITCD,
                                     ITNM = o.ITNM,
                                     STYLENO = o.STYLENO,
                                     ITGRPCD = j.ITGRPCD,
                                     ITGRPNM = m.ITGRPNM,
                                     SIZECD = j.SIZECD,
                                     SIZENM = p.SIZENM
                                 }).ToList();

                if (VE.MSCMITMGRP != null)
                {
                    for (int i = 0; i <= VE.MSCMITMGRP.Count - 1; i++)
                    {
                        VE.MSCMITMGRP[i].SLNO = Convert.ToByte(i + 1);
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
                List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                UploadDOC UPL = new UploadDOC();
                UPL.DocumentType = doctP;
                UploadDOC1.Add(UPL);
                VE.UploadDOC = UploadDOC1;
            }
            return VE;
        }
        public ActionResult SchemeItemDet(string val)
        {
            SchmItemGroupMaster VE = new SchmItemGroupMaster();
            Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
            SchemeCal scmcal = new SchemeCal();
            DataTable rstmp = scmcal.GenScmItmGrpData("'" + val + "'");

            VE.MSCMITEMGRPDATA = (from DataRow dr in rstmp.Rows
                                  select new MSCMITEMGRPDATA()
                                  {
                                      //BRANDNM = dr["brandnm"].ToString(),
                                      //SBRANDNM = dr["sbrandnm"].ToString(),
                                      //COLLNM = dr["collnm"].ToString(),
                                      ITGRPCD = dr["itgrpcd"].ToString(),
                                      ITGRPNM = dr["itgrpnm"].ToString(),
                                      STYLENO = dr["styleno"].ToString(),
                                      ITCD = dr["itcd"].ToString(),
                                      ITNM = dr["itnm"].ToString(),
                                      SIZENM = dr["sizenm"].ToString(),
                                      PRINT_SEQ = dr["print_seq"].ToString(),
                                      COLRNM = dr["colrnm"].ToString(),
                                  }).ToList();
            VE.DefaultView = true;
            return PartialView("_M_ScmItemGrpData", VE);


        }
        //public ActionResult SearchPannelData()
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //    var MDT = (from a in DB.M_SCMITMGRP_HDR
        //               join c in DB.M_CNTRL_HDR on a.M_AUTONO equals (c.M_AUTONO)
        //               select new
        //               {
        //                   SCMITMGRPCD = a.SCMITMGRPCD,
        //                   SCMITMGRPNM = a.SCMITMGRPNM
        //               }).Distinct().OrderBy(s => s.SCMITMGRPCD).ToList();

        //    System.Text.StringBuilder SB = new System.Text.StringBuilder();
        //    var hdr = "Item Group Code" + Cn.GCS() + "Item Group Name";
        //    for (int j = 0; j <= MDT.Count - 1; j++)
        //    {
        //        SB.Append("<tr>");
        //        SB.Append("<td>" + MDT[j].SCMITMGRPCD + "</td><td>" + MDT[j].SCMITMGRPNM + "</td>");
        //        SB.Append("</tr>");
        //    }
        //    return PartialView("_SearchPannel2", Master_HelpFa.Generate_SearchPannel(hdr, SB.ToString(), "0"));
        //}
        public ActionResult SearchPannelData()
        {
            string scm = CommVar.CurSchema(UNQSNO);
            string compcd = CommVar.Compcd(UNQSNO);
            string LOC = CommVar.Loccd(UNQSNO);
            string str = "select distinct a.SCMITMGRPCD,a.SCMITMGRPNM from " + scm + ".M_SCMITMGRP_HDR a," + scm + ".m_cntrl_hdr b, " + scm + ".m_cntrl_loca c ";
            str += "where a.m_autono=b.m_autono and a.m_autono=c.m_autono(+)  ";
            str += "and (c.compcd='" + compcd + "' or c.compcd is null) and (c.loccd='" + LOC + "' or c.loccd is null) order by a.SCMITMGRPCD ";
            DataTable MDT = Master_Help.SQLquery(str);

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Item Group Code" + Cn.GCS() + "Item Group Name";
            for (int j = 0; j <= MDT.Rows.Count - 1; j++)
            {
                SB.Append("<tr>");
                SB.Append("<td>" + MDT.Rows[j]["SCMITMGRPCD"] + " </td><td>" + MDT.Rows[j]["SCMITMGRPNM"] + " </td>");
                SB.Append("</tr>");
            }
            return PartialView("_SearchPannel2", Master_HelpFa.Generate_SearchPannel(hdr, SB.ToString(), "0"));
        }
        //public ActionResult GetBrandDetails()
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //    return PartialView("_Help2", Master_Help.BRANDCD_help(DB));
        //}
        //public ActionResult BRANDCode(string val)
        //{
        //    string COM_CD = CommVar.Compcd(UNQSNO);
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //    var query = (from c in DB.M_BRAND
        //                 join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
        //                 join k in DB.M_CNTRL_LOCA on c.M_AUTONO equals k.M_AUTONO into g
        //                 from k in g.DefaultIfEmpty()
        //                 where (c.BRANDCD == val && i.INACTIVE_TAG == "N" && (k.COMPCD == COM_CD || k.COMPCD == null))
        //                 select c);
        //    if (query.Any())
        //    {
        //        string str = "";
        //        foreach (var i in query)
        //        {
        //            str = i.BRANDCD + Cn.GCS() + i.BRANDNM;
        //        }
        //        return Content(str);
        //    }
        //    else
        //    {
        //        return Content("0");
        //    }
        //}
        //public ActionResult GetSubBrandDetails(string val, string Code)
        //{
        //    try
        //    {
        //        if (val == null)
        //        {
        //            return PartialView("_Help2", Master_Help.SBRAND(val, Code));
        //        }
        //        else
        //        {
        //            string str = Master_Help.SBRAND(val, Code);
        //            return Content(str);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Cn.SaveException(ex, "");
        //        return Content(ex.Message + ex.InnerException);
        //    }
        //}
        //public ActionResult GetItmGrpDetails(string Code)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //    //return PartialView("_Help2", Master_Help.GROUP(DB, "F", Code));
        //    return PartialView("_Help2", Master_Help.ITGRPCD_help( "F", Code));
        //}
        public ActionResult ITEMGRPCode(string val, string Code)
        {
            string COM_CD = CommVar.Compcd(UNQSNO);
            string LOC_CD = CommVar.Loccd(UNQSNO);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            string GRPTYPE = "F";
            var query = (from c in DB.M_GROUP
                         join i in DB.M_CNTRL_HDR on c.M_AUTONO equals i.M_AUTONO
                         join k in DB.M_CNTRL_LOCA on c.M_AUTONO equals k.M_AUTONO into g
                         from k in g.DefaultIfEmpty()
                         where (c.ITGRPCD == val && c.ITGRPTYPE == GRPTYPE && i.INACTIVE_TAG == "N" && (k.COMPCD == COM_CD || k.COMPCD == null) && (k.LOCCD == LOC_CD || k.LOCCD == null))
                         select c);

            if (query.Any())
            {
                string str = "";
                foreach (var i in query)
                {
                    str = i.ITGRPCD + Cn.GCS() + i.ITGRPNM;
                }
                return Content(str);
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult GetCollectionDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.COLLECTION(val));
                }
                else
                {
                    string str = Master_Help.COLLECTION(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        //public ActionResult GetStyleDetails(string val)
        //{
        //    string ITGTYPE = "F";
        //    if (val == null)
        //    {
        //        return PartialView("_Help2", Master_Help.ITCD_help(val, ITGTYPE));
        //    }
        //    else
        //    {
        //        string str = Master_Help.ITCD_help(val, ITGTYPE);
        //        return Content(str);
        //    }
        //}
        //public ActionResult GetSizeDetails(string val, string Code)
        //{
        //    try
        //    {
        //        if (val == null)
        //        {
        //            return PartialView("_Help2", Master_Help.SIZE(val, Code));
        //        }
        //        else
        //        {
        //            string str = Master_Help.SIZE(val, Code);
        //            return Content(str);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Cn.SaveException(ex, "");
        //        return Content(ex.Message + ex.InnerException);
        //    }
        //}
        //public ActionResult GetColourDetails(string val)
        //{
        //    try
        //    {
        //        if (val == null)
        //        {
        //            return PartialView("_Help2", Master_Help.COLOR(val));
        //        }
        //        else
        //        {
        //            string str = Master_Help.COLOR(val);
        //            return Content(str);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Cn.SaveException(ex, "");
        //        return Content(ex.Message + ex.InnerException);
        //    }
        //}
        public ActionResult ChkITGRPCD(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            string val1 = val.ToUpper();
            var query = (from c in DB.M_SCMITMGRP_HDR where (c.SCMITMGRPCD == val1) select c);
            if (query.Any())
            {
                return Content("0");
            }
            else
            {
                return Content("1");
            }
        }
        public ActionResult Addrow(SchmItemGroupMaster VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());


            List<MSCMITMGRP> SCMITMDTL = new List<MSCMITMGRP>();
            if (VE.MSCMITMGRP == null)
            {
                List<MSCMITMGRP> SCMITMDTL1 = new List<MSCMITMGRP>();
                MSCMITMGRP SCMIT = new MSCMITMGRP();
                SCMIT.SLNO = 1;
                SCMITMDTL1.Add(SCMIT);
                VE.MSCMITMGRP = SCMITMDTL1;
            }
            else {
                for (int i = 0; i <= VE.MSCMITMGRP.Count - 1; i++)
                {
                    MSCMITMGRP SIDTL = new MSCMITMGRP();
                    SIDTL = VE.MSCMITMGRP[i];
                    SCMITMDTL.Add(SIDTL);
                }
                MSCMITMGRP SIL = new MSCMITMGRP();
                var max = VE.MSCMITMGRP.Max(a => Convert.ToInt32(a.SLNO));
                int SLNO = Convert.ToInt32(max) + 1;
                SIL.SLNO = Convert.ToByte(SLNO);
                SCMITMDTL.Add(SIL);
                VE.MSCMITMGRP = SCMITMDTL;
            }
            VE.DefaultView = true;
            return PartialView("_M_ScmItemGrp", VE);
        }
        public ActionResult Deleterow(SchmItemGroupMaster VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            List<MSCMITMGRP> SCMITMDTL = new List<MSCMITMGRP>();
            int count = 0;
            for (int i = 0; i <= VE.MSCMITMGRP.Count - 1; i++)
            {
                if (VE.MSCMITMGRP[i].Checked == false || (VE.MSCMITMGRP[i].Checked == false))
                {
                    count += 1;
                    MSCMITMGRP item = new MSCMITMGRP();
                    item = VE.MSCMITMGRP[i];
                    item.SLNO = Convert.ToByte(count);
                    SCMITMDTL.Add(item);
                }
            }
            VE.MSCMITMGRP = SCMITMDTL;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_M_ScmItemGrp", VE);
        }
        public ActionResult AddDOCRow(SchmItemGroupMaster VE)
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
        public ActionResult DeleteDOCRow(SchmItemGroupMaster VE)
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
        public ActionResult SAVE(FormCollection FC, SchmItemGroupMaster VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_SCMITMGRP_HDR MSCMITMGRPHDR = new M_SCMITMGRP_HDR();
                        MSCMITMGRPHDR.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            MSCMITMGRPHDR.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());
                            MSCMITMGRPHDR.EMD_NO = 0;
                        }
                        else
                        {
                            MSCMITMGRPHDR.M_AUTONO = VE.M_SCMITMGRP_HDR.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_CNTRL_HDR where p.M_AUTONO == MSCMITMGRPHDR.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MSCMITMGRPHDR.EMD_NO = 0;
                            }
                            else
                            {
                                MSCMITMGRPHDR.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                        }

                        MSCMITMGRPHDR.SCMITMGRPCD = VE.M_SCMITMGRP_HDR.SCMITMGRPCD.ToUpper();
                        MSCMITMGRPHDR.SCMITMGRPNM = VE.M_SCMITMGRP_HDR.SCMITMGRPNM;
                        //grid saving
                        if (VE.DefaultAction == "E")
                        {
                            DB.M_SCMITMGRP.Where(x => x.SCMITMGRPCD == MSCMITMGRPHDR.SCMITMGRPCD).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_SCMITMGRP.RemoveRange(DB.M_SCMITMGRP.Where(x => x.SCMITMGRPCD == MSCMITMGRPHDR.SCMITMGRPCD));

                            DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SCMITMGRP_HDR.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SCMITMGRP_HDR.M_AUTONO));

                            DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SCMITMGRP_HDR.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SCMITMGRP_HDR.M_AUTONO));

                            DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == VE.M_SCMITMGRP_HDR.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_LOCA.RemoveRange(DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == VE.M_SCMITMGRP_HDR.M_AUTONO));
                        }

                        //Control header
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_SCMITMGRP_HDR", MSCMITMGRPHDR.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(), VE.Audit_REM);
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_CNTRL_HDR.Add(MCH);
                            DB.SaveChanges();
                            DB.M_SCMITMGRP_HDR.Add(MSCMITMGRPHDR);
                            DB.SaveChanges();
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MSCMITMGRPHDR).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        }

                        // grid saving
                        if (VE.MSCMITMGRP != null)
                        {
                            for (int i = 0; i <= VE.MSCMITMGRP.Count - 1; i++)
                            {
                                if (VE.MSCMITMGRP[i].BRANDCD != null || VE.MSCMITMGRP[i].SBRANDCD != null || VE.MSCMITMGRP[i].ITGRPCD != null || VE.MSCMITMGRP[i].COLLCD != null || VE.MSCMITMGRP[i].ITCD != null || VE.MSCMITMGRP[i].SIZECD != null || VE.MSCMITMGRP[i].COLRCD != null)
                                {
                                    M_SCMITMGRP MSCMITMGRP = new M_SCMITMGRP();
                                    MSCMITMGRP.EMD_NO = MSCMITMGRPHDR.EMD_NO;
                                    MSCMITMGRP.CLCD = MSCMITMGRPHDR.CLCD;
                                    MSCMITMGRP.SCMITMGRPCD = VE.M_SCMITMGRP_HDR.SCMITMGRPCD.ToUpper();
                                    MSCMITMGRP.SLNO = VE.MSCMITMGRP[i].SLNO;
                                    MSCMITMGRP.BRANDCD = VE.MSCMITMGRP[i].BRANDCD;
                                    MSCMITMGRP.SBRANDCD = VE.MSCMITMGRP[i].SBRANDCD;
                                    MSCMITMGRP.ITGRPCD = VE.MSCMITMGRP[i].ITGRPCD;
                                    MSCMITMGRP.COLLCD = VE.MSCMITMGRP[i].COLLCD;
                                    MSCMITMGRP.ITCD = VE.MSCMITMGRP[i].ITCD;
                                    MSCMITMGRP.SIZECD = VE.MSCMITMGRP[i].SIZECD;
                                    MSCMITMGRP.COLRCD = VE.MSCMITMGRP[i].COLRCD;

                                    DB.M_SCMITMGRP.Add(MSCMITMGRP);

                                }
                            }

                        }

                        //end saving
                        if (VE.UploadDOC != null)
                        {
                            var img = Cn.SaveUploadImage("M_SCMITMGRP_HDR", VE.UploadDOC, MSCMITMGRPHDR.M_AUTONO, MSCMITMGRPHDR.EMD_NO.Value);
                            DB.M_CNTRL_HDR_DOC.AddRange(img.Item1);
                            DB.M_CNTRL_HDR_DOC_DTL.AddRange(img.Item2);
                        }
                        if (VE.CompanyLocationName != null)// add CompanyLocation
                        {
                            var cnt_comp_loc = VE.CompanyLocationName.Where(a => a.Checked == true).ToList();
                            if (cnt_comp_loc.Count > 0)
                            {
                                string com = CommVar.Compcd(UNQSNO);
                                string loc = CommVar.Loccd(UNQSNO);
                                var chk = cnt_comp_loc.Where(a => a.COMPCD == com && a.LOCCD == loc && a.Checked == true).ToList();
                                if (chk.Count == 0)
                                {
                                    transaction.Rollback();
                                    return Content("Please checked login Company and location in 'Company location' tab or unchecked all !! ");
                                }
                            }
                            var complocnm = Cn.SaveCompanyLocation(VE.CompanyLocationName, MSCMITMGRPHDR.M_AUTONO, MSCMITMGRPHDR.EMD_NO.Value);
                            if (complocnm.Item1.Count != 0)
                            {
                                DB.M_CNTRL_LOCA.AddRange(complocnm.Item1);
                            }
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

                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_SCMITMGRP_HDR", VE.M_SCMITMGRP_HDR.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString(), VE.Audit_REM);
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_SCMITMGRP_HDR.Where(x => x.M_AUTONO == VE.M_SCMITMGRP_HDR.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_SCMITMGRP.Where(x => x.SCMITMGRPCD == VE.M_SCMITMGRP_HDR.SCMITMGRPCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SCMITMGRP_HDR.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SCMITMGRP_HDR.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == VE.M_SCMITMGRP_HDR.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SCMITMGRP_HDR.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SCMITMGRP_HDR.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_LOCA.RemoveRange(DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == VE.M_SCMITMGRP_HDR.M_AUTONO));
                        DB.SaveChanges();

                        DB.M_SCMITMGRP.RemoveRange(DB.M_SCMITMGRP.Where(x => x.SCMITMGRPCD == VE.M_SCMITMGRP_HDR.SCMITMGRPCD));
                        DB.SaveChanges();
                        DB.M_SCMITMGRP_HDR.RemoveRange(DB.M_SCMITMGRP_HDR.Where(x => x.M_AUTONO == VE.M_SCMITMGRP_HDR.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_SCMITMGRP_HDR.M_AUTONO));
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
                    transaction.Rollback();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }


            }

        }

        [HttpPost]
        public ActionResult M_ScmItemGrp(FormCollection FC, SchmItemGroupMaster VE)
        {
            try
            {
                DataTable tbl;
                SchemeCal Scheme_Cal = new SchemeCal();

                tbl = Scheme_Cal.GenScmItmGrpData("'" + VE.M_SCMITMGRP_HDR.SCMITMGRPCD + "'");

                if (tbl != null)
                {
                    //string strfile = Server.MapPath("~/Templates/Improvar.xlt");
                    //Master_Help.DataTbltoExcel("Schme_Items", "items", "", tbl);
                    //return Content("Schme_Items.xls file generated in download folder...");

                    DataTable[] exdt = new DataTable[1] { tbl };
                    string strfile = "Schme_Items.xlsx";
                    string[] sheetname = new string[1] { "Sheet1" };
                    XLWorkbook workbook = new XLWorkbook();
                    // Master_Help.DataTbltoExcel1(workbook, "Sheet1", tbl, null, null);
                    Master_Help.ExcelfromDataTables(exdt, sheetname, "", false, null);
                    Response.Clear();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment;filename=" + strfile);
                    using (MemoryStream MyMemoryStream = new MemoryStream())
                    {
                        workbook.SaveAs(MyMemoryStream);
                        MyMemoryStream.WriteTo(Response.OutputStream);
                        Response.Flush();
                        Response.End();
                    }
                    return Content(strfile + " file generated in download folder...");
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
            return View();
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
        public ActionResult GetItemGroupDetails(string val, string Code)
        {
            try
            {
                TransactionOutIssProcess VE = new TransactionOutIssProcess();
                Cn.getQueryString(VE);
                string str = Master_Help.ITGRPCD_help(val, "", Code);
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
    }
}