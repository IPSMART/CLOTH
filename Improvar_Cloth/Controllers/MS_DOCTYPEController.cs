using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;
using System.Globalization;

namespace Improvar.Controllers
{
    public class MS_DOCTYPEController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        M_DOCTYPE sl; M_CNTRL_HDR sll;


        // GET: MS_DOCTYPE
        public ActionResult MS_DOCTYPE(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                var UNQSNO = Cn.getQueryStringUNQSNO();
                ViewBag.formname = "Document Master";
                string SCHEMA = Cn.Getschema;
                Cn.M_AUTONO(CommVar.CurSchema(UNQSNO));
                string loca1 = CommVar.Loccd(UNQSNO).Trim();
                ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), SCHEMA);
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                DocumentTypeEntry VE = new DocumentTypeEntry(); Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                //=================For Document Type================//
                VE.DocumentType = (from i in DB.M_DTYPE select new DocumentType() { value = i.DCD, text = i.DNAME }).OrderBy(s => s.text).ToList();
                //=================End Document Type================//

                var module = Module.MODULE;
                if (Module.MODULE == "FINANCE")
                {
                    module = "F";
                }
                else if (Module.MODULE == "INVENTORY")
                {
                    module = "I";
                }
                else if (Module.MODULE == "SALES")
                {
                    module = "S";
                }
                else if (Module.MODULE == "PAYROLL")
                {
                    module = "P";
                }


                //=================For Document Numbering================//
                List<DocumentNumbering> DN = new List<DocumentNumbering>();
                DocumentNumbering DN1 = new DocumentNumbering();
                DN1.text = "Daily";
                DN1.value = "D";
                DN.Add(DN1);
                DocumentNumbering DN2 = new DocumentNumbering();
                DN2.text = "Monthly";
                DN2.value = "M";
                DN.Add(DN2);
                DocumentNumbering DN3 = new DocumentNumbering();
                DN3.text = "Yearly";
                DN3.value = "Y";
                DN.Add(DN3);
                DocumentNumbering DN4 = new DocumentNumbering();
                DN4.text = "Continuosly";
                DN4.value = "C";
                DN.Add(DN4);
                VE.DocumentNumbering = DN;
                //=================End Document Numbering================//
                //=================For PRO================//
                List<DropDown_list> PROLST = new List<DropDown_list>();
                DropDown_list PROobj3 = new DropDown_list();
                PROobj3.text = "Other";
                PROobj3.value = "O";
                PROLST.Add(PROobj3);
                DropDown_list PROobj1 = new DropDown_list();
                PROobj1.text = "Payment";
                PROobj1.value = "P";
                PROLST.Add(PROobj1);
                DropDown_list PROobj2 = new DropDown_list();
                PROobj2.text = "Receipt";
                PROobj2.value = "R";
                PROLST.Add(PROobj2);
                DropDown_list PROobj4 = new DropDown_list();
                PROobj4.text = "Inhouse";
                PROobj4.value = "I";
                PROLST.Add(PROobj4);

                VE.DropDown_list = PROLST;
                //=================End PRO================//
                //=================For Document W / WO Zero================//

                List<DocumentWithoutZero> DWZ = new List<DocumentWithoutZero>();
                DocumentWithoutZero DWZ1 = new DocumentWithoutZero();
                DWZ1.text = "Yes";
                DWZ1.value = "Y";
                DWZ.Add(DWZ1);
                DocumentWithoutZero DWZ2 = new DocumentWithoutZero();
                DWZ2.text = "No";
                DWZ2.value = "N";
                DWZ.Add(DWZ2);
                VE.DocumentWithoutZero = DWZ;
                //=================End Document W / WO Zero================//
                VE.LASTDOCNOPATTERN = (from I in DB.M_DOCTYPE
                                       orderby I.M_AUTONO descending
                                       select new Database_Combo1
                                       {
                                           FIELD_VALUE = I.DOCNOPATTERN
                                       }).Distinct().Take(10).ToList();
                //=================End LASTDOCNOPATTERN================//
                VE.CompanyLocationChk = (from i in DBF.M_LOCA join l in DBF.M_COMP on i.COMPCD equals (l.COMPCD) select new CompanyLocationName() { COMPCD = l.COMPCD, COMPNM = l.COMPNM, LOCCD = i.LOCCD, LOCNM = i.LOCNM }).OrderBy(s => s.COMPNM).ThenBy(k => k.LOCNM).ToList();

                if (op.Length != 0)
                {
                    VE.IndexKey = (from p in DB.M_DOCTYPE
                                   orderby p.DOCCD
                                   join o in DB.M_CNTRL_HDR on p.M_AUTONO equals (o.M_AUTONO)
                                   where (p.DOCCD.Remove(1) == module)
                                   select new IndexKey() { Navikey = p.DOCCD }).ToList();
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
                        VE.M_DOCTYPE = sl;
                        VE.M_CNTRL_HDR = sll;
                        UpdateCompanycode(VE, "fill");

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
        public dynamic UpdateCompanycode(DocumentTypeEntry VE, String tag = "")
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            var module = Module.MODULE;
            if (Module.MODULE == "FINANCE")
            {
                module = "FIN_COMPANY";

            }
            else if (Module.MODULE == "INVENTORY")
            {
                module = "INV_COMPANY";
            }
            else if (Module.MODULE == "SALES")
            {
                module = "SD_COMPANY";
            }
            else if (Module.MODULE == "PAYROLL")
            {
                module = "PAY_COMPANY";
            }
            else
            {
                module = "";
            }
            string sql = "select distinct COMPCD,COMPNM,LOCCD,LOCNM from " + module + " where MODULE_CODE like '%" + CommVar.ModuleCode() + "%' AND CLIENT_CODE='" + CommVar.ClientCode(UNQSNO) + "'";
            DataTable data = masterHelp.SQLquery(sql);
            VE.CompanyLocationName = (from DataRow dr in data.Rows
                                      select new CompanyLocationName()
                                      {
                                          COMPCD = dr["COMPCD"].ToString(),
                                          COMPNM = dr["COMPNM"].ToString(),
                                          LOCCD = dr["LOCCD"].ToString(),
                                          LOCNM = dr["LOCNM"].ToString(),
                                      }).OrderBy(s => s.COMPNM).ThenBy(s => s.LOCNM).ToList();
            if (VE.M_DOCTYPE.DOCCD != null)
            {
                foreach (var i in VE.CompanyLocationName)
                {
                    string DocPattern1 = DocPattern(VE.M_DOCTYPE.DOCNOPATTERN, VE.M_DOCTYPE.DOCNOWOZERO, (VE.M_DOCTYPE.DOCNOMAXLENGTH).ToString(), VE.M_DOCTYPE.DOCPRFX,
            Convert.ToInt64("1"), VE.M_DOCTYPE.DOCCD, CommVar.CurSchema(UNQSNO), CommVar.FinSchema(UNQSNO), i.COMPCD, i.LOCCD, "");
                    i.Docpattern = DocPattern1;
                }
            }
            if (tag == "fill")
            {
                return VE.CompanyLocationName;
            }
            else {
                return PartialView("_MS_DOCTYPE_Compcod", VE);
            }
        }

        private string DocPattern(string DOCNOPATTERN, string DOCNOWOZERO, string DOCNOMAXLENGTH, string DOCPRFX, long docno, string doc_type_code, string schema, string FIN_SCHEMA, string COMPCD, string LOCCD, string GOCD, string docdt = "")
        {
            Improvar.Models.ImprovarDB DB = new Models.ImprovarDB(Cn.GetConnectionString(), schema);

            var UNQSNO = Cn.getQueryStringUNQSNO();
            string[] pattern = new[] { "&comprefix&", "&locprefix&", "&docprefix&", "&mm&-&docno&", "&mmm&", "&docno&", "&yy&", "&finyr&", "&finyrs&", "&finyrf&", "&para&" };
            string newPattern = DOCNOPATTERN;
            string docno1 = "";
            DateTime ddate;
            if (docdt == "") ddate = DateTime.Now; else ddate = Convert.ToDateTime(docdt);

            if (DOCNOWOZERO == "N")
            {
                docno1 = docno.ToString().PadLeft(Convert.ToInt16(DOCNOMAXLENGTH), '0');
            }
            else
            {
                docno1 = docno.ToString();
            }
            string[] dfinyr = CommVar.FinPeriod(UNQSNO).Split('-');
            string finyr = "", finyrs = "", yy = "";
            yy = dfinyr[0].ToString().Trim().Substring(8);
            if (yy == dfinyr[1].ToString().Trim().Substring(8)) finyr = yy;
            else finyr = yy + "-" + dfinyr[1].ToString().Trim().Substring(8);
            finyrs = finyr.Replace("-", "");
            string finyrf = dfinyr[0].ToString().Trim().Substring(6) + "-" + yy;
            Improvar.Models.ImprovarDB DB1 = new Models.ImprovarDB(Cn.GetConnectionString(), FIN_SCHEMA);
            M_LOCA MLOCA = DB1.M_LOCA.Find(LOCCD, COMPCD);
            string sql = "select BLINIT from " + CommVar.CurSchema(UNQSNO) + ".M_GODOWN  WHERE GOCD='" + GOCD + "'";
            DataTable dt = masterHelp.SQLquery(sql); string docpara = "";
            if (dt != null && dt.Rows.Count > 0)
            {
                docpara = dt.Rows[0]["BLINIT"].ToString();
            }
            string[] pattern1 = new[] { CommVar.Compcd(UNQSNO), MLOCA.LOCA_CODE, DOCPRFX, ddate.Month.ToString().PadLeft(2, '0') + "-" + docno1.ToString(), CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(ddate.Month), docno1.ToString(), yy, finyr, finyrs, finyrf, docpara };
            //string[] pattern1 = new[] { COMPCD, MLOCA.LOCA_CODE, DOCPRFX, ddate.Month.ToString().PadLeft(2, '0') + "-" + docno1.ToString(), CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(ddate.Month), docno1.ToString(), Finyr, Finyrs, BLINIT };
            for (int i = 0; i <= pattern.Length - 1; i++)
            {
                int index = newPattern.IndexOf(pattern[i]);
                if (index >= 0)
                {
                    newPattern = newPattern.Replace(pattern[i], pattern1[i]);
                }
            }
            return newPattern;
        }
        public DocumentTypeEntry Navigation(DocumentTypeEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            sl = new M_DOCTYPE(); sll = new M_CNTRL_HDR();
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
                sl = DB.M_DOCTYPE.Find(aa[0].Trim());
                VE.MAINDOCNM = DB.M_DOCTYPE.Find(sl.MAINDOCCD)?.DOCNM;
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
                if (sll.INACTIVE_TAG == "Y")
                {
                    VE.Checked = true;
                }
                else
                {
                    VE.Checked = false;
                }
                if (sl.FDATE == "Y") VE.FDATE = true; else VE.FDATE = false;
                if (sl.BACKDATE == "Y") VE.BACKDATE = true; else VE.BACKDATE = false;
                var doccd = sl.DOCCD;
                string str = "select p.COMPCD,s.COMPNM,p.LOCCD,x.LOCNM from " + CommVar.CurSchema(UNQSNO) + ".M_CNTRL_LOCA p,"
                    + CommVar.FinSchema(UNQSNO) + ".M_COMP s," + CommVar.FinSchema(UNQSNO) + ".M_LOCA x where ";
                str += "p.COMPCD=s.COMPCD and p.LOCCD=x.LOCCD and p.M_AUTONO ='" + sl.M_AUTONO + "'";
                var DATA_CLN = masterHelp.SQLquery(str);

                var CLN = (from DataRow DR in DATA_CLN.Rows
                           select new CompanyLocationName()
                           {
                               COMPCD = DR["COMPCD"].ToString(),
                               COMPNM = DR["COMPNM"].ToString(),
                               LOCCD = DR["LOCCD"].ToString(),
                               LOCNM = DR["LOCNM"].ToString(),
                               Checked = true
                           }).ToList();


                if (VE.CompanyLocationChk != null)
                    foreach (var i in VE.CompanyLocationChk)
                    {
                        foreach (var x in CLN)
                        {
                            if (i.COMPCD == x.COMPCD && i.LOCCD == x.LOCCD)
                            {
                                i.Checked = true;
                            }
                        }
                    }
            }
            ImprovarDB DB_PREVYR_temp = new ImprovarDB(Cn.GetConnectionString(), CommVar.LastYearSchema(UNQSNO));
            if (CommVar.LastYearSchema(UNQSNO) == "")
            {
                VE.isPresentinLastYrSchema = "";
            }
            else
            {
                var DOCCD = sl.DOCCD;
                VE.isPresentinLastYrSchema = (from j in DB_PREVYR_temp.M_DOCTYPE where (j.DOCCD == DOCCD) select j.DOCCD).FirstOrDefault();
                if (string.IsNullOrEmpty(VE.isPresentinLastYrSchema))
                {
                    VE.isPresentinLastYrSchema = "ADD";
                }
                else
                {
                    VE.isPresentinLastYrSchema = "EDIT";
                }
            }
            return VE;
        }
        public ActionResult SearchPannelData()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            var module = Module.MODULE;
            if (Module.MODULE == "FINANCE")
            {
                module = "F";
            }
            else if (Module.MODULE == "INVENTORY")
            {
                module = "I";
            }
            else if (Module.MODULE == "SALES")
            {
                module = "S";
            }
            else if (Module.MODULE == "PAYROLL")
            {
                module = "P";
            }
            var MDT = (from j in DB.M_DOCTYPE
                       join o in DB.M_CNTRL_HDR on j.M_AUTONO equals (o.M_AUTONO)
                       join p in DB.M_DTYPE on j.DOCTYPE equals p.DCD
                       where (o.M_AUTONO == j.M_AUTONO && j.DOCCD.Remove(1) == module)
                       select new
                       {
                           DOCCD = j.DOCCD,
                           DOCNM = j.DOCNM,
                           DOCTYPE = j.DOCTYPE,
                           DNAME = p.DNAME,
                           DOCPRFX = j.DOCPRFX,
                           DOCNOPATTERN = j.DOCNOPATTERN,
                           M_AUTONO = o.M_AUTONO
                       }).OrderBy(s => s.DOCCD).ToList();

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Document Code" + Cn.GCS() + "Document Name" + Cn.GCS() + "Document Type" + Cn.GCS() + "Document Prefix" + Cn.GCS() + "Document No Pattern";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].DOCCD + "</td><td>" + MDT[j].DOCNM + "</td><td>" + MDT[j].DOCTYPE + " [" + MDT[j].DNAME + "]" + "</td><td>" + MDT[j].DOCPRFX + "</td><td>" + MDT[j].DOCNOPATTERN + "</td></tr>");
            }
            return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "0"));
        }
        public ActionResult CheckDocumentCode(string val)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            string CHECKDCD = "";
            if (Module.MODULE == "FINANCE")
            {
                CHECKDCD = "F" + val;
            }
            else if (Module.MODULE == "INVENTORY")
            {
                CHECKDCD = "I" + val;
            }
            else if (Module.MODULE == "SALES")
            {
                CHECKDCD = "S" + val;
            }
            else if (Module.MODULE == "PAYROLL")
            {
                CHECKDCD = "P" + val;
            }
            var query = (from c in DB.M_DOCTYPE where (c.DOCCD == CHECKDCD) select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult ChangeDocumentType(string val)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                string str = "";
                var query = (from c in DB.M_DOCTYPE where (c.DOCTYPE == val) select c).ToList();
                if (query != null && query.Count != 0)
                {
                    var data1 = query.FirstOrDefault();
                    List<M_DOCTYPE> M_DOCTYPElist = new List<M_DOCTYPE>();
                    data1.DOCCD = data1.DOCCD.Substring(1, data1.DOCCD.Length - 1);
                    M_DOCTYPElist.Add(data1);
                    str = masterHelp.ToReturnFieldValues(M_DOCTYPElist);
                    return Content(str);
                }
                else
                {
                    var M_DOCTYPE = (from c in DB.M_DOCTYPE orderby c.M_AUTONO descending select c).FirstOrDefault();
                    if (M_DOCTYPE == null)
                    {
                        M_DOCTYPE = new M_DOCTYPE();
                    }
                    var dtype = DB.M_DTYPE.Where(d => d.DCD == val).First();
                    M_DOCTYPE.DOCCD = dtype.DCD.Length > 4 ? dtype.DCD.Remove(4) : dtype.DCD;
                    M_DOCTYPE.DOCNM = dtype.DNAME;
                    M_DOCTYPE.DOCPRFX = dtype.DCD.Length > 2 ? dtype.DCD.Remove(2) : dtype.DCD;
                    query.Add(M_DOCTYPE);
                    str = masterHelp.ToReturnFieldValues(query.Take(1));
                    return Content(str);
                }
            }
            catch
            {
                return Content("0");
            }
        }
        public ActionResult GetDOC_Code(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", masterHelp.DOCCD_help(val, "", ""));
                }
                else
                {
                    string str = masterHelp.DOCCD_help(val, "", "");
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult SavePreviousYrData(DocumentTypeEntry VE, FormCollection FC)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            ImprovarDB DB_PREVYR = new ImprovarDB(Cn.GetConnectionString(), CommVar.LastYearSchema(UNQSNO));
            ImprovarDB DBF_PREVYR = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchemaPrevYr(UNQSNO));
            ImprovarDB DB_PREVYR_temp = new ImprovarDB(Cn.GetConnectionString(), CommVar.LastYearSchema(UNQSNO));
            ImprovarDB DBF_PREVYR_temp = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchemaPrevYr(UNQSNO));
            using (var transaction = DB_PREVYR.Database.BeginTransaction())
            {
                using (var transaction1 = DBF_PREVYR.Database.BeginTransaction())
                {
                    try
                    {
                        var PSL = (from j in DB_PREVYR_temp.M_DOCTYPE where (j.DOCCD == VE.M_DOCTYPE.DOCCD) select j).FirstOrDefault();
                        var PSL1 = (from j in DBF_PREVYR_temp.M_DOCTYPE where (j.DOCCD == VE.M_DOCTYPE.DOCCD) select j).FirstOrDefault();
                        var SL = (from j in DB.M_DOCTYPE where (j.DOCCD == VE.M_DOCTYPE.DOCCD) select j).FirstOrDefault();
                        var SLOCA = (from j in DB.M_CNTRL_LOCA where (j.M_AUTONO == VE.M_DOCTYPE.M_AUTONO) select j).ToList();
                        var SL1 = (from j in DBF.M_DOCTYPE where (j.DOCCD == VE.M_DOCTYPE.DOCCD) select j).FirstOrDefault();

                        string fintag = "";
                        M_DTYPE dcdfin = DB.M_DTYPE.Find(SL.DOCTYPE);
                        if (dcdfin != null) fintag = dcdfin.FIN.ToUpper();
                        // SAVE TO CurSchema//

                        if (PSL == null)
                        {
                            var AUTONO_PREVYR = Cn.M_AUTONO(CommVar.LastYearSchema(UNQSNO));
                            M_CNTRL_HDR MCH_PREVYR = Cn.M_CONTROL_HDR(VE.IsChecked, "M_DOCTYPE", AUTONO_PREVYR, "A", CommVar.LastYearSchema(UNQSNO));
                            DB_PREVYR.M_CNTRL_HDR.Add(MCH_PREVYR);
                            DB_PREVYR.SaveChanges();
                            M_DOCTYPE MDOCTYPE_PREVYR = new M_DOCTYPE();
                            MDOCTYPE_PREVYR = SL;
                            MDOCTYPE_PREVYR.M_AUTONO = AUTONO_PREVYR;
                            DB_PREVYR.M_DOCTYPE.Add(MDOCTYPE_PREVYR);
                            DB_PREVYR.SaveChanges();

                            if (SLOCA.Count != 0)
                            {
                                foreach (var v in SLOCA)
                                {
                                    v.M_AUTONO = AUTONO_PREVYR;
                                }
                                DB_PREVYR.M_CNTRL_LOCA.AddRange(SLOCA);
                            }
                            DB_PREVYR.SaveChanges();
                        }
                        else
                        {
                            M_CNTRL_HDR MCH_PREVYR = Cn.M_CONTROL_HDR(VE.IsChecked, "M_DOCTYPE", PSL.M_AUTONO.retInt(), "E", CommVar.LastYearSchema(UNQSNO));
                            DB_PREVYR.Entry(MCH_PREVYR).State = System.Data.Entity.EntityState.Modified;
                            M_DOCTYPE MDOCTYPE_PREVYR = new M_DOCTYPE();
                            MDOCTYPE_PREVYR = SL; MDOCTYPE_PREVYR.M_AUTONO = MCH_PREVYR.M_AUTONO;
                            DB_PREVYR.Entry(MDOCTYPE_PREVYR).State = System.Data.Entity.EntityState.Modified;
                            DB_PREVYR.M_CNTRL_LOCA.RemoveRange(DB_PREVYR.M_CNTRL_LOCA.Where(x => x.M_AUTONO == MCH_PREVYR.M_AUTONO));
                            DB_PREVYR.SaveChanges();
                            if (SLOCA.Count != 0)
                            {
                                foreach (var v in SLOCA)
                                {
                                    v.M_AUTONO = MCH_PREVYR.M_AUTONO;
                                }
                                DB_PREVYR.M_CNTRL_LOCA.AddRange(SLOCA);
                            }
                            DB_PREVYR.SaveChanges();

                        }//END  CurSchema

                        // SAVE TO FIN SCHEMA//
                        if (fintag == "Y" && Module.MODULE != "FINANCE")
                        {

                            if (PSL1 == null)
                            {
                                M_DOCTYPE MDOCTYPE_PREVYR = new M_DOCTYPE();
                                MDOCTYPE_PREVYR = SL;
                                MDOCTYPE_PREVYR.M_AUTONO = 0;
                                DBF_PREVYR.M_DOCTYPE.Add(MDOCTYPE_PREVYR);
                                DBF_PREVYR.SaveChanges();

                            }
                            else
                            {
                                var FMDOCTYPE = (from DOC in DBF_PREVYR.M_DOCTYPE
                                                 where (DOC.DOCCD == PSL1.DOCCD)
                                                 select new
                                                 {
                                                     DOCCD = DOC.DOCCD
                                                 }).FirstOrDefault();
                                if (FMDOCTYPE == null)
                                {
                                    M_DOCTYPE MDOCTYPE_PREVYR1 = new M_DOCTYPE();
                                    MDOCTYPE_PREVYR1 = SL;
                                    MDOCTYPE_PREVYR1.M_AUTONO = 0;
                                    DBF_PREVYR.M_DOCTYPE.Add(MDOCTYPE_PREVYR1);
                                }
                                else
                                {
                                    M_DOCTYPE MDOCTYPE_PREVYR1 = new M_DOCTYPE();
                                    MDOCTYPE_PREVYR1 = SL;
                                    MDOCTYPE_PREVYR1.M_AUTONO = 0;
                                    DBF_PREVYR.Entry(MDOCTYPE_PREVYR1).State = System.Data.Entity.EntityState.Modified;
                                }

                                DBF_PREVYR.SaveChanges();

                            }
                        }//END
                        ModelState.Clear();
                        transaction.Commit();
                        transaction1.Commit();

                        return Content("1");

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        transaction1.Rollback();
                        ModelState.Clear();
                        Cn.SaveException(ex, "");
                        return Content(ex.Message + " " + ex.InnerException);
                    }
                    //  }
                }
            }
        }
        public ActionResult SAVE(FormCollection FC, DocumentTypeEntry VE)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_DOCTYPE MDOCTYPE = new M_DOCTYPE();
                        MDOCTYPE.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            MDOCTYPE.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO));
                            MDOCTYPE.EMD_NO = 0;
                            string DCD = VE.M_DOCTYPE.DOCCD.Replace(" ", "");
                            string DCODE = "";
                            if (Module.MODULE == "FINANCE")
                            {
                                DCODE = "F" + DCD;
                            }
                            else if (Module.MODULE == "INVENTORY")
                            {
                                DCODE = "I" + DCD;
                            }
                            else if (Module.MODULE == "SALES")
                            {
                                DCODE = "S" + DCD;

                            }
                            else if (Module.MODULE == "PAYROLL")
                            {
                                DCODE = "P" + DCD;
                            }
                            string sql = "select * from " + CommVar.CurSchema(UNQSNO) + ".M_DOCTYPE where DOCCD='" + DCODE + "'";
                            DataTable tbl = masterHelp.SQLquery(sql);
                            if (tbl.Rows.Count > 0) return Content("Document Code Already Exists : Please Enter a Valid Document Code !!");
                            MDOCTYPE.DOCCD = DCODE.ToUpper();
                        }
                        else
                        {
                            MDOCTYPE.DOCCD = VE.M_DOCTYPE.DOCCD;
                            MDOCTYPE.M_AUTONO = VE.M_DOCTYPE.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_DOCTYPE where p.M_AUTONO == VE.M_DOCTYPE.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MDOCTYPE.EMD_NO = 0;
                            }
                            else
                            {
                                MDOCTYPE.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                        }
                        MDOCTYPE.DOCNM = VE.M_DOCTYPE.DOCNM;
                        MDOCTYPE.DOCPRFX = VE.M_DOCTYPE.DOCPRFX;
                        MDOCTYPE.DOCTYPE = VE.M_DOCTYPE.DOCTYPE;
                        string fintag = "";
                        M_DTYPE dcdfin = DB.M_DTYPE.Find(MDOCTYPE.DOCTYPE);
                        if (dcdfin != null) fintag = dcdfin.FIN.ToUpper();
                        MDOCTYPE.DOCJNRL = VE.M_DOCTYPE.DOCJNRL;
                        MDOCTYPE.DOCFOOT = VE.M_DOCTYPE.DOCFOOT;
                        MDOCTYPE.DOCNOMAXLENGTH = VE.M_DOCTYPE.DOCNOMAXLENGTH;
                        MDOCTYPE.DOCNOWOZERO = VE.M_DOCTYPE.DOCNOWOZERO;
                        MDOCTYPE.PRO = VE.M_DOCTYPE.PRO;
                        MDOCTYPE.DOCNOPATTERN = VE.M_DOCTYPE.DOCNOPATTERN;
                        if (VE.M_DOCTYPE.MAINDOCCD == MDOCTYPE.DOCCD)
                        {
                            transaction.Rollback();
                            return Content("MAINDOCCD AND DOCCD SAME. Please change main doccd.");
                        }
                        MDOCTYPE.MAINDOCCD = VE.M_DOCTYPE.MAINDOCCD;
                        MDOCTYPE.FLAG1 = VE.M_DOCTYPE.FLAG1;
                        if (VE.FDATE == true) MDOCTYPE.FDATE = "Y"; else MDOCTYPE.FDATE = "N";
                        if (VE.BACKDATE == true) MDOCTYPE.BACKDATE = "Y"; else MDOCTYPE.BACKDATE = "N";
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_DOCTYPE", MDOCTYPE.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO));
                        if (VE.DefaultAction == "E")
                        {
                            DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == MDOCTYPE.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_LOCA.RemoveRange(DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == MDOCTYPE.M_AUTONO));
                        }

                        if (VE.CompanyLocationChk != null)
                        {
                            for (int i = 0; i <= VE.CompanyLocationChk.Count - 1; i++)
                            {
                                if (VE.CompanyLocationChk[i].Checked)
                                {
                                    M_CNTRL_LOCA MCL = new M_CNTRL_LOCA();
                                    MCL.M_AUTONO = MDOCTYPE.M_AUTONO;
                                    MCL.EMD_NO = MDOCTYPE.EMD_NO;
                                    MCL.CLCD = CommVar.ClientCode(UNQSNO);
                                    MCL.COMPCD = VE.CompanyLocationChk[i].COMPCD;
                                    MCL.LOCCD = VE.CompanyLocationChk[i].LOCCD;
                                    DB.M_CNTRL_LOCA.Add(MCL);
                                }
                            }
                        }
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_DOCTYPE.Add(MDOCTYPE);
                            DB.M_CNTRL_HDR.Add(MCH);
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MDOCTYPE).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        }
                        DB.SaveChanges();
                        transaction.Commit();
                        if (fintag == "Y" && Module.MODULE != "FINANCE")
                        {
                            string doctype = VE.M_DOCTYPE.DOCTYPE;
                            DataTable dt = masterHelp.SQLquery("select dcd from " + CommVar.FinSchema(UNQSNO) + ".m_dtype where dcd='" + doctype + "'");
                            if (dt.Rows.Count == 0)
                            {
                                masterHelp.SQLNonQuery("insert into " + CommVar.FinSchema(UNQSNO) + ".m_dtype "
                              + " select dcd, dname, fin, menu_progcall, menu_para, flag1 from " + CommVar.CurSchema(UNQSNO) + ".m_dtype where dcd = '" + doctype + "'");
                            }
                            dt = masterHelp.SQLquery("select doctype from " + CommVar.FinSchema(UNQSNO) + ".m_doctype where doccd='" + MDOCTYPE.DOCCD + "'");
                            if (dt.Rows.Count > 0)
                            {
                                var sql = "delete from   " + CommVar.FinSchema(UNQSNO) + ".m_doctype A where doccd = '" + MDOCTYPE.DOCCD + "'";
                                masterHelp.SQLNonQuery(sql);
                            }
                            masterHelp.SQLNonQuery("insert into " + CommVar.FinSchema(UNQSNO) + ".m_doctype"
                            + " select a.emd_no,a.clcd,a.dtag,a.ttag,a.doccd,a.docnm,a.frdt,a.todt,a.docprfx,a.doctype,a.docjnrl,a.docfoot,a.docprn,a.docnopattern,a.docnomaxlength,a.docnowozero,1"//M_AUTONO
                            + " ,a.pro,a.fdate,a.backdate,a.maindoccd,a.flag1"
                            + " from " + CommVar.CurSchema(UNQSNO) + ".m_doctype A  where doccd = '" + MDOCTYPE.DOCCD + "'");
                        }
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
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_DOCTYPE", VE.M_DOCTYPE.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO));
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();
                        M_DOCTYPE dcdfin = DBF.M_DOCTYPE.Find(VE.M_DOCTYPE.DOCCD);
                        if (dcdfin != null && Module.MODULE != "FINANCE")
                        {
                            using (var transactionfin = DBF.Database.BeginTransaction())
                            {
                                try
                                {
                                    DBF.M_DOCTYPE.Where(x => x.DOCCD == VE.M_DOCTYPE.DOCCD).ToList().ForEach(x => { x.DTAG = "D"; });
                                    DBF.M_DOCTYPE.RemoveRange(DBF.M_DOCTYPE.Where(x => x.DOCCD == VE.M_DOCTYPE.DOCCD));
                                    DBF.SaveChanges();
                                    transactionfin.Commit();
                                }
                                catch (Exception ex)
                                {
                                    transaction.Rollback();
                                    transactionfin.Rollback();
                                    Cn.SaveException(ex, "");
                                    return Content(ex.Message + ex.InnerException);
                                }
                            }
                        }
                        DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == VE.M_DOCTYPE.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_DOCTYPE.Where(x => x.M_AUTONO == VE.M_DOCTYPE.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_DOCTYPE.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();
                        DB.M_CNTRL_LOCA.RemoveRange(DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == VE.M_DOCTYPE.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_DOCTYPE.RemoveRange(DB.M_DOCTYPE.Where(x => x.M_AUTONO == VE.M_DOCTYPE.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_DOCTYPE.M_AUTONO));
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
                    return Content(ex.Message + "     " + ex.InnerException);
                }
            }
        }

        [HttpPost]
        public ActionResult MS_DOCTYPE(FormCollection FC, DocumentTypeEntry VE)
        {
            var UNQSNO = Cn.getQueryStringUNQSNO();
            MasterHelp mas = new MasterHelp();
            string scm1 = CommVar.CurSchema(UNQSNO);
            string sqlquery = "select DOCTYPE,DOCCD,DOCNM,DOCPRFX,DOCJNRL,DOCNOPATTERN,DOCNOMAXLENGTH,FDATE,BACKDATE from " + scm1 + ".m_doctype";
            DataTable dt = masterHelp.SQLquery(sqlquery);
            DataTable[] exdt = new DataTable[1];
            exdt[0] = dt;
            string[] sheetname = new string[1];
            sheetname[0] = "Sheet1";
            mas.ExcelfromDataTables(exdt, sheetname, "Document Type Master", true,"Document Type");
            return Content("Downloded");
        }
    }
}