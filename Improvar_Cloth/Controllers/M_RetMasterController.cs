using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
namespace Improvar.Controllers
{
    public class M_RetMasterController : Controller
    {
       
        Connection Cn = new Connection();
        string CS = null;
        M_RETDEB sl; MS_STATE sl1;
        M_CNTRL_HDR sll;
        M_CNTRL_HDR_REM MCHR;
        MasterHelp Master_Help = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_RetMaster
        public ActionResult M_RetMaster(FormCollection FC, string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            //testing
            try
            {//test
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Retail Debtor Master";
                    string loca1 = CommVar.Loccd(UNQSNO).Trim();
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO).ToString());
                    RetailDebtorMasterEntry VE = new RetailDebtorMasterEntry();

                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    var LOCCD = CommVar.Loccd(UNQSNO); var COMPCD = CommVar.Compcd(UNQSNO);
                    var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    //=================For City Database Combo================//
                    VE.Database_Combo1 = (from j in DB.M_RETDEB select new Database_Combo1() { FIELD_VALUE = j.CITY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                    //=================END City Database Combo================//
                    //=================For Country Database Combo================//
                    VE.Database_Combo2 = (from j in DB.M_RETDEB select new Database_Combo2() { FIELD_VALUE = j.COUNTRY }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();
                    //=================END Country Database Combo================//
                    //=================For Gender================//
                    List<DropDown_list1> G = new List<DropDown_list1>();
                    DropDown_list1 G1 = new DropDown_list1();
                    G1.text = " Male";
                    G1.value = "M";
                    G.Add(G1);
                    DropDown_list1 G2 = new DropDown_list1();
                    G2.text = "Female";
                    G2.value = "F";
                    G.Add(G2);
                    DropDown_list1 G3 = new DropDown_list1();
                    G3.text = "Transgender";
                    G3.value = "T";
                    G.Add(G3);
                    VE.DropDown_list1 = G;
                    //=================For Gender================//
                 
                    VE.DefaultAction = op;

                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_RETDEB orderby p.RTDEBCD select new IndexKey() { Navikey = p.RTDEBCD }).ToList();

                        if (op == "E" || op == "D" || op == "V")
                        {
                            VE.Searchpannel_State = true;
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
                            VE.MS_STATE = sl1;
                            VE.M_CNTRL_HDR = sll;
                            VE.M_CNTRL_HDR_REM = MCHR;
                        }
                        if (op.ToString() == "A")
                        {
                            sl = new M_RETDEB();
                            List<UploadDOC> UploadDOC1 = new List<UploadDOC>();
                            UploadDOC UPL = new UploadDOC();
                            UPL.DocumentType = doctP;
                            UploadDOC1.Add(UPL);
                            VE.UploadDOC = UploadDOC1;
                            var query = (from c in DB.M_LOCA where c.LOCCD == LOCCD && c.COMPCD == COMPCD
                                         select new
                                         {
                                             STATECD = c.STATECD,
                                             STATE = c.STATE,
                                             DISTRICT = c.DISTRICT,
                                             COUNTRY= c.COUNTRY
                                         }).FirstOrDefault();
                            if (query != null)
                            {
                                sl.CITY = query.DISTRICT;
                                sl.STATECD = query.STATECD;
                                sl.COUNTRY = query.COUNTRY;
                                MS_STATE mstate = new MS_STATE();
                                mstate.STATENM = query.STATE;
                                VE.MS_STATE = mstate;
                            }
                        }
                        VE.M_RETDEB = sl;
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
                RetailDebtorMasterEntry VE = new RetailDebtorMasterEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public RetailDebtorMasterEntry Navigation(RetailDebtorMasterEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            try
            {
                sl = new M_RETDEB();
                sl1 = new MS_STATE();
                sll = new M_CNTRL_HDR();
                MCHR = new M_CNTRL_HDR_REM();
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
                    sl = DB.M_RETDEB.Find(aa[0]);
                    sl1 = DB1.MS_STATE.Find(sl.STATECD);
                    sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
                    if (sl.REFRTDEBCD != null)
                    { var Party = DB.M_RETDEB.Find(sl.REFRTDEBCD); if (Party != null) { VE.REFRTDEBNM = Party.RTDEBNM; } }
                    if (sl.REFSLCD != null)
                    { var RefSLCD = DB.M_SUBLEG.Find(sl.REFSLCD); if (RefSLCD != null) { VE.REFSLNM = RefSLCD.SLNM;VE.GSTNO = RefSLCD.GSTNO; } }

                    if (sll.INACTIVE_TAG == "Y")
                    {
                        VE.Checked = true;
                    }
                    else
                    {
                        VE.Checked = false;
                    }
                    MCHR = Cn.GetMasterReamrks(CommVar.FinSchema(UNQSNO), Convert.ToInt32(sl.M_AUTONO));
                    VE.UploadDOC = Cn.GetUploadImage(CommVar.FinSchema(UNQSNO).ToString(), Convert.ToInt32(sl.M_AUTONO));

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
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
            }
            return VE;
        }
        public ActionResult SearchPannelData()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO).ToString());
            var MDT = (from j in DB.M_RETDEB
                       join o in DB.M_CNTRL_HDR on j.M_AUTONO equals (o.M_AUTONO)
                       where (j.M_AUTONO == o.M_AUTONO)
                       select new
                       {
                           RTDEBCD = j.RTDEBCD,
                           RTDEBNM = j.RTDEBNM,
                           MOBILE = j.MOBILE,
                           CITY = j.CITY,
                           PIN = j.PIN,
                           STATE = j.STATE,
                           COUNTRY = j.COUNTRY,
                           M_AUTONO = j.M_AUTONO
                       }).OrderBy(s => s.RTDEBCD).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Code" + Cn.GCS() + "Name" + Cn.GCS() + "Mobile No." + Cn.GCS() + "City" + Cn.GCS() + "Pin" + Cn.GCS() + "State" + Cn.GCS() + "Country" + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].RTDEBCD + "</td><td>" + MDT[j].RTDEBNM + "</td><td>" + MDT[j].MOBILE + "</td><td>" + MDT[j].CITY + "</td><td>" + MDT[j].PIN + "</td><td>" + MDT[j].STATE + "</td><td>" + MDT[j].COUNTRY + "</td><td>" + MDT[j].M_AUTONO + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "0", "7"));
        }
        public ActionResult GetState()
        {
            try
            {
             ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
             return PartialView("_Help2", Master_Help.STATECD_help(DB));  
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult State(string val)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                var query = (from c in DB.MS_STATE where (c.STATECD == val) select c);
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    { str = i.STATECD + Cn.GCS() + i.STATENM;
                    }
                    return Content(str);
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
        public ActionResult GetRefRetailDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    return PartialView("_Help2", Master_Help.RTDEBCD_help(val));
                }
                else
                {
                    string str = Master_Help.RTDEBCD_help(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
      
        public ActionResult GetRefLedgerDetails(string val, string Code)
        {
            try
            {
                //var agent = Code.Split(Convert.ToChar(Cn.GCS()));
                //if (agent.Count() > 1)
                //{
                //    if (agent[1] == "")
                //    {
                //        return Content("Please Select Ref Ledger !!");
                //    }
                //    else
                //    {
                //        Code = agent[0];
                //    }
                //}
                var str = Master_Help.SLCD_help(val, Code);
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
        
        public ActionResult AddDOCRow(RetailDebtorMasterEntry VE)
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
        public ActionResult DeleteDOCRow(RetailDebtorMasterEntry VE)
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
        public ActionResult SAVE(FormCollection FC, RetailDebtorMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.FinSchema(UNQSNO).ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_RETDEB MRETDEB = new M_RETDEB();
                        MRETDEB.CLCD = CommVar.ClientCode(UNQSNO);
                       
                        if (VE.DefaultAction == "A")
                        {
                            var mobno = VE.M_RETDEB.MOBILE;
                            var ChkDuplicateMobNo = (from i in DB.M_RETDEB where i.MOBILE == mobno select new { i.MOBILE, i.RTDEBNM }).FirstOrDefault();
                            if (ChkDuplicateMobNo != null) { transaction.Rollback(); return Content("Mobile No : '" + ChkDuplicateMobNo.MOBILE + "' already exsist for '" + ChkDuplicateMobNo.RTDEBNM + "' please change entered Mobile No."); }
                            MRETDEB.EMD_NO = 0;
                            MRETDEB.M_AUTONO = Cn.M_AUTONO(CommVar.FinSchema(UNQSNO).ToString());
                            MRETDEB.RTDEBCD = Cn.GenMasterCode("M_RETDEB", "RTDEBCD", VE.M_RETDEB.RTDEBNM.ToUpper().Trim().Substring(0, 1), 8,"F");

                           
                        }
                        if (VE.DefaultAction == "E")
                        {
                            var mobno = VE.M_RETDEB.MOBILE;
                            var code = VE.M_RETDEB.RTDEBCD;
                            var ChkDuplicateMobNo = (from i in DB.M_RETDEB where i.MOBILE == mobno && i.RTDEBCD!= code select new { i.MOBILE, i.RTDEBNM }).FirstOrDefault();
                            if (ChkDuplicateMobNo != null) { transaction.Rollback(); return Content("Mobile No : '" + ChkDuplicateMobNo.MOBILE + "' already exsist for '" + ChkDuplicateMobNo.RTDEBNM + "' please change entered Mobile No."); }
                            MRETDEB.RTDEBCD = VE.M_RETDEB.RTDEBCD;
                            MRETDEB.DTAG = "E";
                            MRETDEB.M_AUTONO = VE.M_RETDEB.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_RETDEB where p.M_AUTONO == VE.M_RETDEB.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MRETDEB.EMD_NO = 0;
                            }
                            else
                            {
                                MRETDEB.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                            DB.M_CNTRL_HDR_REM.Where(x => x.M_AUTONO == MRETDEB.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_REM.RemoveRange(DB.M_CNTRL_HDR_REM.Where(x => x.M_AUTONO == MRETDEB.M_AUTONO));
                            DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_RETDEB.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_RETDEB.M_AUTONO));

                            DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_RETDEB.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_RETDEB.M_AUTONO));

                        }
                      
                        MRETDEB.MOBILE = VE.M_RETDEB.MOBILE;
                        MRETDEB.ALTNO = VE.M_RETDEB.ALTNO;
                        MRETDEB.SEX = FC["gender"].ToString();
                        MRETDEB.RTDEBNM = VE.M_RETDEB.RTDEBNM;
                        MRETDEB.AREA = VE.M_RETDEB.AREA;
                        MRETDEB.CITY = VE.M_RETDEB.CITY;
                        MRETDEB.PIN = VE.M_RETDEB.PIN;
                        MRETDEB.DOB = VE.M_RETDEB.DOB;
                        MRETDEB.DOW = VE.M_RETDEB.DOW;
                        MRETDEB.EMAIL = VE.M_RETDEB.EMAIL;
                        MRETDEB.ADD1 = VE.M_RETDEB.ADD1;
                        MRETDEB.ADD2 = VE.M_RETDEB.ADD2;
                        MRETDEB.ADD3 = VE.M_RETDEB.ADD3;
                        MRETDEB.STATECD = VE.M_RETDEB.STATECD;
                        MRETDEB.STATE = VE.MS_STATE.STATENM;
                        MRETDEB.COUNTRY = VE.M_RETDEB.COUNTRY;
                        MRETDEB.REFRTDEBCD = VE.M_RETDEB.REFRTDEBCD;
                        MRETDEB.REFSLCD = VE.M_RETDEB.REFSLCD;
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_RETDEB", Convert.ToInt32(MRETDEB.M_AUTONO), VE.DefaultAction, CommVar.FinSchema(UNQSNO).ToString());
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_RETDEB.Add(MRETDEB);
                            DB.M_CNTRL_HDR.Add(MCH);
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MRETDEB).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        }
                        if (VE.UploadDOC != null)
                        {
                            var img = Cn.SaveUploadImage("M_RETDEB", VE.UploadDOC, Convert.ToInt32(MRETDEB.M_AUTONO), MRETDEB.EMD_NO.Value);
                            DB.M_CNTRL_HDR_DOC.AddRange(img.Item1);
                            DB.M_CNTRL_HDR_DOC_DTL.AddRange(img.Item2);
                        }
                        if (VE.M_CNTRL_HDR_REM.DOCREM != null)// add REMARKS
                        {
                            var NOTE = Cn.SAVEMASTERREMARKS(VE.M_CNTRL_HDR_REM, Convert.ToInt32(MRETDEB.M_AUTONO), MRETDEB.CLCD, MRETDEB.EMD_NO.Value);

                            if (NOTE.Item1.Count != 0)
                            {
                                DB.M_CNTRL_HDR_REM.AddRange(NOTE.Item1);

                            }
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
                        var refrtdebcd = VE.M_RETDEB.REFRTDEBCD;
                        var ChkRefRetail = (from i in DB.M_RETDEB where i.REFRTDEBCD == refrtdebcd select new { i.REFRTDEBCD,i.RTDEBCD,i.RTDEBNM }).FirstOrDefault();
                        if (ChkRefRetail != null) { transaction.Rollback(); return Content("Delete not possible Child record found. Reference Retail found at Retail code: '" + ChkRefRetail.RTDEBCD + "' and Name : '" + ChkRefRetail.RTDEBNM +"'"); }
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_RETDEB", Convert.ToInt32(VE.M_RETDEB.M_AUTONO), VE.DefaultAction, CommVar.FinSchema(UNQSNO).ToString());
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_RETDEB.Where(x => x.M_AUTONO == VE.M_RETDEB.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_RETDEB.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_RETDEB.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_RETDEB.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_REM.Where(x => x.M_AUTONO == VE.M_RETDEB.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });

                        DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_RETDEB.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_RETDEB.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR_REM.RemoveRange(DB.M_CNTRL_HDR_REM.Where(x => x.M_AUTONO == VE.M_RETDEB.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_RETDEB.RemoveRange(DB.M_RETDEB.Where(x => x.M_AUTONO == VE.M_RETDEB.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_RETDEB.M_AUTONO));
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
                    return Content(ex.Message);
                }
            }
        }
        [HttpPost]
        public ActionResult M_RetMaster(FormCollection FC, RetailDebtorMasterEntry VE)
        {
            try
            {
                string dbname = CommVar.FinSchema(UNQSNO).ToString();

                string query = "select a.RTDEBCD, a.RTDEBNM,a.MOBILE,a.CITY,a.PIN,a.STATE,a.COUNTRY ";
                query = query + " from " + dbname + ".M_RETDEB a, " + dbname + ".m_cntrl_hdr c ";
                query = query + "where  a.m_autono=c.m_autono(+) and nvl(c.inactive_tag,'N') = 'N'  and a.RTDEBCD ='" + VE.M_RETDEB.RTDEBCD + "' ";
                query = query + "order by RTDEBCD, a.RTDEBNM";

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
                string str = query;
                Cn.com = new OracleCommand(str, Cn.con);
                Cn.da.SelectCommand = Cn.com;
                bool bu = Convert.ToBoolean(Cn.da.Fill(Cn.ds, "mst_rep"));
                Cn.con.Close();
                var record = Master_Help.SQLquery(query);
                if (bu)
                {
                    DataTable IR = new DataTable("mstrep");

                    Models.PrintViewer PV = new Models.PrintViewer();
                    HtmlConverter HC = new HtmlConverter();

                    HC.RepStart(IR, 2);
                    HC.GetPrintHeader(IR, "RTDEBNM", "string", "c,20", "Name");
                    HC.GetPrintHeader(IR, "RTDEBCD", "string", "c,7", "Code");
                    HC.GetPrintHeader(IR, "MOBILE", "string", "c,10", "Mobile No.");
                    HC.GetPrintHeader(IR, "CITY", "string", "c,10", "City");
                    HC.GetPrintHeader(IR, "PIN", "string", "c,10", "Pin Code");
                    HC.GetPrintHeader(IR, "STATE", "string", "c,10", "State");
                    HC.GetPrintHeader(IR, "COUNTRY", "string", "c,10", "Country");

                    for (int i = 0; i <= Cn.ds.Tables["mst_rep"].Rows.Count - 1; i++)
                    {
                        DataRow dr = IR.NewRow();
                        dr["RTDEBNM"] = Cn.ds.Tables["mst_rep"].Rows[i]["RTDEBNM"];
                        dr["RTDEBCD"] = Cn.ds.Tables["mst_rep"].Rows[i]["RTDEBCD"];
                        dr["MOBILE"] = Cn.ds.Tables["mst_rep"].Rows[i]["MOBILE"];
                        dr["CITY"] = Cn.ds.Tables["mst_rep"].Rows[i]["CITY"];
                        dr["PIN"] = Cn.ds.Tables["mst_rep"].Rows[i]["PIN"];
                        dr["STATE"] = Cn.ds.Tables["mst_rep"].Rows[i]["STATE"];
                        dr["COUNTRY"] = Cn.ds.Tables["mst_rep"].Rows[i]["COUNTRY"];
                        dr["Flag"] = " class='grid_td'";
                        IR.Rows.Add(dr);
                    }
                    string pghdr1 = "";
                    string repname = CommFunc.retRepname("Retail Debtor Master Details");
                    pghdr1 = "Retail Debtor Master Details";
                    PV = HC.ShowReport(IR, repname, pghdr1, "", true, true, "P", false);
                    return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });

                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
            return View();
        }

        private ActionResult ResponsivePrintReport()
        {
            throw new NotImplementedException();
        }

        public ActionResult PrintReport()
        {
            return RedirectToAction("PrintViewer", "RPTViewer");
        }
    }
}