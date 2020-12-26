using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using System.IO;

namespace Improvar.Controllers
{
    public class M_StitchController : Controller
    {
        // GET: M_Stitch
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        M_CNTRL_HDR sll; M_STCHGRP sl; 
        public ActionResult M_Stitch(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Stiching Master";
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    StitchMaster VE = new StitchMaster(); Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    VE.DefaultAction = op;
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    List<DropDown_list1> GTYPE = new List<DropDown_list1>();
                    DropDown_list1 GTYPE1 = new DropDown_list1();
                    GTYPE1.text = "Stitching";
                    GTYPE1.value = "S";
                    GTYPE.Add(GTYPE1);
                    DropDown_list1 GTYPE2 = new DropDown_list1();
                    GTYPE2.text = "Alteration";
                    GTYPE2.value = "A";
                    GTYPE.Add(GTYPE2);
                    VE.DropDown_list1 = GTYPE;
                    VE.DropDown_list2 = FLDTYPE();
                    VE.Database_Combo1 = (from n in DB.M_STCHGRP
                                          select new Database_Combo1() { FIELD_VALUE = n.STCHUOM }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_STCHGRP orderby p.STCHCD select new IndexKey() { Navikey = p.STCHCD }).ToList();
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
                            VE.M_STCHGRP = sl;
                            VE.M_CNTRL_HDR = sll;
                        }
                        else
                        {
                            List<MSTCHGRPCOMP> MSTCHGRPCOMP = new List<MSTCHGRPCOMP>();
                            for (int i = 0; i <= 9; i++)
                            {
                                MSTCHGRPCOMP FIXGRPCOMP = new MSTCHGRPCOMP();
                                FIXGRPCOMP.SLNO = Convert.ToByte(i + 1);
                                MSTCHGRPCOMP.Add(FIXGRPCOMP);
                            }
                            VE.MSTCHGRPCOMP = MSTCHGRPCOMP;

                            List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                            UploadDOC UPL = new UploadDOC();
                            UPL.DocumentType = doctP;
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
                StitchMaster VE = new StitchMaster();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public StitchMaster Navigation(StitchMaster VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new M_STCHGRP(); sll = new M_CNTRL_HDR(); 
            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
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
                sl = DB.M_STCHGRP.Find(aa[0].Trim());
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
                if (sl != null) if (sl.INCLTAX == "Y")  VE.Checked_INCLTAX = true; else VE.Checked_INCLTAX = false;
                if(sl!=null)if(sl.JOBCD!=null) VE.JOBNM= (from i in DB.M_JOBMST where i.JOBCD==sl.JOBCD select i.JOBNM).FirstOrDefault();
                //Stiching
                string sql = "";
                sql += "select i.SLNO,i.FLDCD,i.FLDNM,i.FLDDESC,i.FLDTYPE,i.FLDLEN,i.FLDDEC,i.FLDMANDT,i.FLDDATACOMBO,i.FLDFLAG1,i.FLDMIN,i.FLDMAX,i.DEACTIVATE ";
                sql += "from " + CommVar.CurSchema(UNQSNO) + ".M_STCHGRP_COMP i ";
                sql += "where i.STCHCD = '" + sl.STCHCD + "' ";
                DataTable tbl_comp = masterHelp.SQLquery(sql);

                VE.MSTCHGRPCOMP = (from DataRow dr in tbl_comp.Rows
                                   select new MSTCHGRPCOMP()
                                   {
                                       SLNO = Convert.ToByte(dr["SLNO"]),
                                       FLDCD = dr["FLDCD"].retStr(),
                                       FLDNM = dr["FLDNM"].retStr(),
                                       FLDDESC = dr["FLDDESC"].retStr(),
                                       FLDTYPE = dr["FLDTYPE"].retStr(),
                                       FLDLEN = dr["FLDLEN"].retShort(),
                                       FLDDEC = dr["FLDDEC"].retStr() == "" ? (Byte?)null : Convert.ToByte(dr["FLDDEC"].retStr()),
                                       Checked_FLDMANDT = dr["FLDMANDT"].retStr() == "Y" ? true : false,
                                       //Checked_FLDREMIND = dr["FLDREMIND"].retStr() == "Y" ? true : false,
                                       Checked_FLDDATACOMBO = dr["FLDDATACOMBO"].retStr() == "Y" ? true : false,
                                       FLDMAX = dr["FLDMAX"].retDbl(),
                                       FLDMIN = dr["FLDMIN"].retDbl(),
                                       FLDFLAG1 = dr["FLDFLAG1"].retStr(),
                                       Checked_DEACTIVATE = dr["DEACTIVATE"].retStr() == "Y" ? true : false
                                   }).OrderBy(s => s.SLNO).ToList();

                if (VE.MSTCHGRPCOMP.Count != 0)
                {
                    for (int i = 0; i <= VE.MSTCHGRPCOMP.Count - 1; i++)
                    {
                        string FLDCD = VE.MSTCHGRPCOMP[i].FLDCD;
                        var chk_T_FIXASTCOMP = false; /*DB.T_FIXASTCOMP.Where(a => a.FLDCD == FLDCD).Any();*/
                        var chk_T_FIXIMPDT = false; /*DB.T_FIXIMPDT.Where(a => a.FLDCD == FLDCD).Any();*/
                        if (chk_T_FIXASTCOMP == true || chk_T_FIXIMPDT == true)
                        {
                            VE.MSTCHGRPCOMP[i].Checked_Disable = "Y";
                        }

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
                VE.M_STCHGRP = sl;
                //List<MSTCHGRPCOMP> STCHGRP = new List<MSTCHGRPCOMP>();
                //MSTCHGRPCOMP STGP = new MSTCHGRPCOMP();
                //STGP.SLNO = Convert.ToByte("1");
                //VE.DropDown_list2 = FLDTYPE();
                //STCHGRP.Add(STGP);
                //VE.MSTCHGRPCOMP = STCHGRP;
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
            string sql = "";
            sql += "select i.STCHCD,i.STCHNM,decode(i.STCHALT,'A','Alteration','S','Stitching') STCHALT ";
            sql += "from " + CommVar.CurSchema(UNQSNO) + ".M_STCHGRP i," + CommVar.CurSchema(UNQSNO) + ".M_CNTRL_HDR j ";
            sql += "where i.M_AUTONO = j.M_AUTONO  order by i.STCHCD ";
            DataTable MDT = masterHelp.SQLquery(sql);

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Stich Code" + Cn.GCS() + "Dress Pattern" + Cn.GCS() + "Type";
            for (int j = 0; j <= MDT.Rows.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT.Rows[j]["STCHCD"].retStr() + "</td><td>" + MDT.Rows[j]["STCHNM"].retStr() + " </td><td>" + MDT.Rows[j]["STCHALT"].retStr() + " </td></tr>");
            }
            return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "0"));
        }

        public List<DropDown_list2> FLDTYPE()
        {
            List<DropDown_list2> FTYPE = new List<DropDown_list2>();
            DropDown_list2 FTYPE1 = new DropDown_list2();
            FTYPE1.text = "Date";
            FTYPE1.value = "D";
            FTYPE.Add(FTYPE1);
            DropDown_list2 FTYPE2 = new DropDown_list2();
            FTYPE2.text = "Numeric";
            FTYPE2.value = "N";
            FTYPE.Add(FTYPE2);
            DropDown_list2 FTYPE3 = new DropDown_list2();
            FTYPE3.text = "Character";
            FTYPE3.value = "C";
            FTYPE.Add(FTYPE3);

            return FTYPE;
        }
        public ActionResult GetJobMaster()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            return PartialView("_Help2", masterHelp.JOBCD_help(DB));
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
        public ActionResult AddGridRow(StitchMaster VE, FormCollection FC, int COUNT, string TAG, string TABLE)
        {
            try
            {
                Cn.getQueryString(VE);
                if (TABLE == "M_Stitch_Grid")
                {
                    VE.DropDown_list2 = FLDTYPE();
                    if (VE.MSTCHGRPCOMP == null)
                    {
                        List<MSTCHGRPCOMP> FIXGRPCOMP1 = new List<MSTCHGRPCOMP>();
                        if (COUNT > 0 && TAG == "Y")
                        { int SERIAL = 0; for (int j = 0; j <= COUNT - 1; j++) { SERIAL = SERIAL + 1; MSTCHGRPCOMP DET = new MSTCHGRPCOMP(); DET.SLNO = Convert.ToByte(SERIAL); FIXGRPCOMP1.Add(DET); } }
                        else { MSTCHGRPCOMP FIXGRPCOMP = new MSTCHGRPCOMP(); FIXGRPCOMP.SLNO = 1; FIXGRPCOMP1.Add(FIXGRPCOMP); }
                        VE.MSTCHGRPCOMP = FIXGRPCOMP1;
                    }
                    else
                    {
                        List<MSTCHGRPCOMP> MSTCHGRPCOMP = new List<MSTCHGRPCOMP>();
                        for (int i = 0; i <= VE.MSTCHGRPCOMP.Count - 1; i++) { MSTCHGRPCOMP FIXGRPCOMP = new MSTCHGRPCOMP(); FIXGRPCOMP = VE.MSTCHGRPCOMP[i]; MSTCHGRPCOMP.Add(FIXGRPCOMP); }
                        MSTCHGRPCOMP FIXGRPCOMP1 = new MSTCHGRPCOMP();
                        if (COUNT > 0 && TAG == "Y")
                        {
                            int SERIAL = Convert.ToInt32(VE.MSTCHGRPCOMP.Max(a => Convert.ToInt32(a.SLNO)));
                            for (int j = 0; j <= COUNT - 1; j++) { SERIAL = SERIAL + 1; MSTCHGRPCOMP OPENING_BL = new MSTCHGRPCOMP(); OPENING_BL.SLNO = Convert.ToByte(SERIAL); MSTCHGRPCOMP.Add(OPENING_BL); }
                        }
                        else { FIXGRPCOMP1.SLNO = Convert.ToByte(VE.MSTCHGRPCOMP.Max(a => Convert.ToInt32(a.SLNO)) + 1); MSTCHGRPCOMP.Add(FIXGRPCOMP1); }
                        VE.MSTCHGRPCOMP = MSTCHGRPCOMP;
                    }
                    VE.DefaultView = true;
                    return PartialView("_M_Stitch_Main", VE);
                }
                else
                {
                    return Content("");
                }
            }
            catch (Exception Ex)
            {
                Cn.SaveException(Ex, "");
                return Content(Ex.Message + Ex.InnerException);
            }
        }
        public ActionResult DeleteGridRow(StitchMaster VE, string TABLE)
        {
            try
            {
                string str = "0";
                Cn.getQueryString(VE); ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                if (TABLE == "M_Stitch_Grid")
                {
                    VE.DropDown_list2 = FLDTYPE();
                    string[] fldcd2 = null;
                    if (VE.DefaultAction == "E")
                    {
                        //var fldcd1 = (from a in VE.MSTCHGRPCOMP where a.Checked_Comp == true select a.FLDCD).ToArray();
                        //fldcd2 = (from c in DB.T_CNTRL_HDR join b in DB.T_STCHGRP on c.AUTONO equals b.AUTONO where fldcd11.Contains(c.FLDCD) select c.FLDCD).ToArray();
                        //var fldcd = string.Join(",", (from c in fldcd2 select c));
                        //if (fldcd.retStr() != "")
                        //{ str = fldcd; }
                    }

                    List<MSTCHGRPCOMP> MFIXGRPDOCCD = new List<MSTCHGRPCOMP>();
                    int count = 0;
                    for (int i = 0; i <= VE.MSTCHGRPCOMP.Count - 1; i++)
                    {
                        if (VE.DefaultAction == "E")
                        {
                            if (VE.MSTCHGRPCOMP[i].Checked_Comp == true && fldcd2.Contains(VE.MSTCHGRPCOMP[i].FLDCD))
                            {
                                VE.MSTCHGRPCOMP[i].Checked_Comp = false;
                            }
                        }
                        if (VE.MSTCHGRPCOMP[i].Checked_Comp == false)
                        { count += 1; MSTCHGRPCOMP item = new MSTCHGRPCOMP(); item = VE.MSTCHGRPCOMP[i]; item.SLNO = Convert.ToByte(count); MFIXGRPDOCCD.Add(item); }
                    }
                    VE.MSTCHGRPCOMP = MFIXGRPDOCCD;
                    ModelState.Clear();
                    VE.DefaultView = true;
                    var Component = RenderRazorViewToString(ControllerContext, "_M_Stitch_Main", VE);
                    return Content(str + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + Component);
                }
                //else if (TABLE == "M_Stitch_User_Grid")
                //{
                //    List<MFIXGRPUSER> MFIXGRPUSER = new List<MFIXGRPUSER>();
                //    int count = 0;
                //    for (int i = 0; i <= VE.MFIXGRPUSER.Count - 1; i++) { if (VE.MFIXGRPUSER[i].Checked_User == false) { count += 1; MFIXGRPUSER item = new MFIXGRPUSER(); item = VE.MFIXGRPUSER[i]; item.SLNO_User = Convert.ToByte(count); MFIXGRPUSER.Add(item); } }
                //    VE.MFIXGRPUSER = MFIXGRPUSER;
                //    ModelState.Clear();
                //    VE.DefaultView = true;
                //    return PartialView("_M_Stitch_User", VE);
                //}
                //else if (TABLE == "M_Stitch_Doc_Grid")
                //{
                //    string[] docd2 = null;
                //    if (VE.DefaultAction == "E")
                //    {
                //        var doccd1 = (from a in VE.MFIXGRPDOCCD where a.Checked_Doccd == true select a.DOCCD).ToArray();
                //        docd2 = (from c in DB.T_CNTRL_HDR join b in DB.T_FIXAST on c.AUTONO equals b.AUTONO where doccd1.Contains(c.DOCCD) select c.DOCCD).ToArray();
                //        var docd = string.Join(",", (from c in docd2 select c));
                //        if (docd.retStr() != "")
                //        { str = docd; }
                //    }

                //    List<MFIXGRPDOCCD> MFIXGRPDOCCD = new List<MFIXGRPDOCCD>();
                //    int count = 0;
                //    for (int i = 0; i <= VE.MFIXGRPDOCCD.Count - 1; i++)
                //    {
                //        if (VE.DefaultAction == "E")
                //        {
                //            if (VE.MFIXGRPDOCCD[i].Checked_Doccd == true && docd2.Contains(VE.MFIXGRPDOCCD[i].DOCCD))
                //            {
                //                VE.MFIXGRPDOCCD[i].Checked_Doccd = false;
                //            }
                //        }
                //        if (VE.MFIXGRPDOCCD[i].Checked_Doccd == false)
                //        { count += 1; MFIXGRPDOCCD item = new MFIXGRPDOCCD(); item = VE.MFIXGRPDOCCD[i]; item.SLNO_Doccd = Convert.ToByte(count); MFIXGRPDOCCD.Add(item); }
                //    }
                //    VE.MFIXGRPDOCCD = MFIXGRPDOCCD;
                //    ModelState.Clear();
                //    VE.DefaultView = true;
                //    var Component = RenderRazorViewToString(ControllerContext, "_M_Stitch_Doc", VE);
                //    return Content(str + "^^^^^^^^^^^^~~~~~~^^^^^^^^^^" + Component);
                //}
                else
                {
                    return Content("");
                }
            }
            catch (Exception Ex)
            {
                Cn.SaveException(Ex, "");
                return Content(Ex.Message + Ex.InnerException);
            }
        }
        public ActionResult AddDOCRow(ItemMasterEntry VE)
        {
            try
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
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult DeleteDOCRow(ItemMasterEntry VE)
        {
            try
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
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public string RenderRazorViewToString(ControllerContext controllerContext, string viewName, object model)
        {
            controllerContext.Controller.ViewData.Model = model;

            using (var stringWriter = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
                var viewContext = new ViewContext(controllerContext, viewResult.View, controllerContext.Controller.ViewData, controllerContext.Controller.TempData, stringWriter);
                viewResult.View.Render(viewContext, stringWriter);
                viewResult.ViewEngine.ReleaseView(controllerContext, viewResult.View);
                return stringWriter.GetStringBuilder().ToString();
            }
        }
        public ActionResult SAVE(FormCollection FC, StitchMaster VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO) + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {

                        M_STCHGRP MFXGRP = new M_STCHGRP();
                        MFXGRP.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            MFXGRP.EMD_NO = 0;
                            MFXGRP.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO));
                            MFXGRP.STCHCD = Cn.GenMasterCode("M_STCHGRP", "STCHCD", VE.M_STCHGRP.STCHNM.ToUpper().Trim().Substring(0, 1), 3);
                        }
                        if (VE.DefaultAction == "E")
                        {
                            MFXGRP.DTAG = "E";
                            MFXGRP.STCHCD = VE.M_STCHGRP.STCHCD;
                            MFXGRP.M_AUTONO = VE.M_STCHGRP.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_STCHGRP where p.M_AUTONO == VE.M_STCHGRP.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MFXGRP.EMD_NO = 0;
                            }
                            else
                            {
                                MFXGRP.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                        }
                        MFXGRP.STCHNM = VE.M_STCHGRP.STCHNM;
                        MFXGRP.STCHALT = VE.M_STCHGRP.STCHALT;
                        MFXGRP.JOBCD = VE.M_STCHGRP.JOBCD;
                        MFXGRP.STCHUOM = VE.M_STCHGRP.STCHUOM;
                        MFXGRP.STCHRATE = VE.M_STCHGRP.STCHRATE;
                        MFXGRP.STCHREM = VE.M_STCHGRP.STCHREM;
                       if(VE.Checked_INCLTAX==true)  MFXGRP.INCLTAX = "Y";
                        if (VE.DefaultAction == "E")
                        {
                            ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                            var comp = DB1.M_STCHGRP_COMP.Where(x => x.STCHCD == VE.M_STCHGRP.STCHCD).OrderBy(s => s.FLDCD).ToList();
                            foreach (var v in comp)
                            {
                                if (!VE.MSTCHGRPCOMP.Where(s => s.FLDCD == v.FLDCD).Any())
                                {
                                    DB.M_STCHGRP_COMP.Where(x => x.FLDCD == v.FLDCD).ToList().ForEach(x => { x.DTAG = "E"; });
                                    DB.M_STCHGRP_COMP.RemoveRange(DB.M_STCHGRP_COMP.Where(x => x.FLDCD == v.FLDCD));
                                }
                            }

                            //DB.M_Stitch_COMP.Where(x => x.STCHCD == VE.M_Stitch.STCHCD).ToList().ForEach(x => { x.DTAG = "E"; });
                            //DB.M_Stitch_COMP.RemoveRange(DB.M_Stitch_COMP.Where(x => x.STCHCD == VE.M_Stitch.STCHCD));

                            DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_STCHGRP.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_STCHGRP.M_AUTONO));

                            DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_STCHGRP.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_STCHGRP.M_AUTONO));
                        }
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_STCHGRP", MFXGRP.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO));
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_CNTRL_HDR.Add(MCH);
                            DB.SaveChanges();
                            DB.M_STCHGRP.Add(MFXGRP);
                            DB.SaveChanges();
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MFXGRP).State = System.Data.Entity.EntityState.Modified;
                        }
                        DB.SaveChanges();
                        bool flag = false;
                        int slno = 0;
                        if (VE.MSTCHGRPCOMP != null)
                        {
                            int FLDCD_no = 0;
                            if (VE.DefaultAction == "A")
                            {
                                FLDCD_no = 1;
                            }
                            else
                            {
                                var FIXGRPCOMP = VE.MSTCHGRPCOMP.Where(a => a.FLDCD != null).ToList();
                                string maxfldcd = FIXGRPCOMP.Where(a => a.FLDCD.Substring(0, 3).ToUpper() == MFXGRP.STCHCD).Max(a => a.FLDCD);
                                FLDCD_no = maxfldcd == null ? 1 : (maxfldcd.Substring(3, 2).retInt() + 1);
                            }

                            for (int i = 0; i <= VE.MSTCHGRPCOMP.Count - 1; i++)
                            {
                                if (VE.MSTCHGRPCOMP[i].SLNO != 0 && VE.MSTCHGRPCOMP[i].FLDTYPE != null && VE.MSTCHGRPCOMP[i].FLDNM != null)
                                {
                                    slno++;
                                    string FLDCD = MFXGRP.STCHCD + (FLDCD_no).ToString("D4");
                                    M_STCHGRP_COMP MSTCHGRPCOMP = new M_STCHGRP_COMP();
                                    MSTCHGRPCOMP.EMD_NO = MFXGRP.EMD_NO;
                                    MSTCHGRPCOMP.CLCD = MFXGRP.CLCD;
                                    MSTCHGRPCOMP.STCHCD = MFXGRP.STCHCD;
                                    MSTCHGRPCOMP.FLDCD = VE.MSTCHGRPCOMP[i].FLDCD == null ? FLDCD : VE.MSTCHGRPCOMP[i].FLDCD;
                                    MSTCHGRPCOMP.SLNO = VE.MSTCHGRPCOMP[i].SLNO; // Convert.ToByte(slno);
                                    MSTCHGRPCOMP.FLDNM = VE.MSTCHGRPCOMP[i].FLDNM;
                                    MSTCHGRPCOMP.FLDDESC = VE.MSTCHGRPCOMP[i].FLDDESC;
                                    MSTCHGRPCOMP.FLDTYPE = VE.MSTCHGRPCOMP[i].FLDTYPE;
                                    MSTCHGRPCOMP.FLDLEN = VE.MSTCHGRPCOMP[i].FLDLEN;
                                    MSTCHGRPCOMP.FLDDEC = VE.MSTCHGRPCOMP[i].FLDDEC;
                                    MSTCHGRPCOMP.FLDMANDT = VE.MSTCHGRPCOMP[i].Checked_FLDMANDT == true ? "Y" : null;
                                    MSTCHGRPCOMP.FLDFLAG1 = VE.MSTCHGRPCOMP[i].FLDFLAG1;
                                    MSTCHGRPCOMP.FLDDATACOMBO = VE.MSTCHGRPCOMP[i].Checked_FLDDATACOMBO == true ? "Y" : null;
                                    MSTCHGRPCOMP.FLDMAX = VE.MSTCHGRPCOMP[i].FLDMAX;
                                    MSTCHGRPCOMP.FLDMIN = VE.MSTCHGRPCOMP[i].FLDMIN;
                                    MSTCHGRPCOMP.DEACTIVATE = VE.MSTCHGRPCOMP[i].Checked_DEACTIVATE == true ? "Y" : "N";
                                    flag = true;

                                    if (VE.MSTCHGRPCOMP[i].FLDCD == null)
                                    {
                                        FLDCD_no++;
                                        //FLDCD = MFXGRP.STCHCD + (FLDCD_no).ToString("D4");
                                        //MSTCHGRPCOMP.FLDCD = VE.MSTCHGRPCOMP[i].FLDCD == null ? FLDCD : VE.MSTCHGRPCOMP[i].FLDCD;
                                        
                                        DB.M_STCHGRP_COMP.Add(MSTCHGRPCOMP);
                                        DB.SaveChanges();
                                    }
                                    else
                                    {
                                        DB.Entry(MSTCHGRPCOMP).State = System.Data.Entity.EntityState.Modified;
                                    }
                                }
                            }
                        }
                        if (flag == false)
                        {
                            transaction.Rollback();
                            return Content("Please Fill Stiching Details!!");
                        }

                        if (VE.UploadDOC != null)
                        {
                            var img = Cn.SaveUploadImage("M_STCHGRP", VE.UploadDOC, MFXGRP.M_AUTONO, MFXGRP.EMD_NO.Value);
                            DB.M_CNTRL_HDR_DOC.AddRange(img.Item1);
                            DB.M_CNTRL_HDR_DOC_DTL.AddRange(img.Item2);
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
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_STCHGRP", VE.M_STCHGRP.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO));
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();
                        DB.M_STCHGRP.Where(x => x.M_AUTONO == VE.M_STCHGRP.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_STCHGRP_COMP.Where(x => x.STCHCD == VE.M_STCHGRP.STCHCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_STCHGRP.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_STCHGRP.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_STCHGRP.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_STCHGRP.M_AUTONO));
                        DB.M_STCHGRP_COMP.RemoveRange(DB.M_STCHGRP_COMP.Where(x => x.STCHCD == VE.M_STCHGRP.STCHCD));
                        DB.SaveChanges();
                        DB.M_STCHGRP.RemoveRange(DB.M_STCHGRP.Where(x => x.M_AUTONO == VE.M_STCHGRP.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_STCHGRP.M_AUTONO));
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
                    transaction.Rollback();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }
        }
    }
}