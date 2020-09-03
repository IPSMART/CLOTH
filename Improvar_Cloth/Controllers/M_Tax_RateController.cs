using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;
using System.Collections.Generic;

namespace Improvar.Controllers
{
    public class M_Tax_RateController : Controller
    {
        Connection Cn = new Connection(); M_PRODTAX sl; M_CNTRL_HDR sll; M_PRODGRP sGRP; MasterHelp Master_Help = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_Tax_Rate

        public ActionResult M_Tax_Rate(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Tax Rate Setup with Product Group Code";
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBINV = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);

                    var doctP = (from i in DBINV.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    TaxRateProductGroupSetupEntry VE = new TaxRateProductGroupSetupEntry();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    if (op.Length != 0)
                    {
                        string GCS = Cn.GCS();
                        string sql = "select distinct PRODGRPCD||'" + GCS + "'|| TO_CHAR(EFFDT,'DD/MM/YYYY')||'" + GCS + "'|| FROMRT AS NAVKEY from " + CommVar.CurSchema(UNQSNO) + ".M_PRODTAX ";
                        DataTable DT = Master_Help.SQLquery(sql);
                        VE.IndexKey = (from DataRow DR in DT.Rows
                                       select new IndexKey
                                       {
                                           Navikey = DR["NAVKEY"].retStr()
                                       }).ToList();

                        //VE.IndexKey = (from p in DB.M_PRODTAX  select new IndexKey() { Navikey = p.PRODGRPCD + GCS + p.EFFDT + GCS + p.FROMRT }).ToList().Distinct().ToList();
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
                            VE.M_PRODTAX = sl;
                            VE.M_PRODGRP = sGRP;
                            VE.M_CNTRL_HDR = sll;
                        }
                        else
                        {
                            // Get_Data(VE, "");
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
                TaxRateProductGroupSetupEntry VE = new TaxRateProductGroupSetupEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public TaxRateProductGroupSetupEntry Navigation(TaxRateProductGroupSetupEntry VE, ImprovarDB DB, int index, string searchValue)
        {

            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            sl = new M_PRODTAX(); sll = new M_CNTRL_HDR(); sGRP = new M_PRODGRP();
            if (VE.IndexKey.Count != 0)
            {
                string[] aa = null;
                DateTime dt = new DateTime();
                if (searchValue.Length == 0)
                {
                    aa = VE.IndexKey[index].Navikey.Split(Convert.ToChar(Cn.GCS()));
                    dt = Convert.ToDateTime(aa[1]);
                }
                else
                {
                    aa = searchValue.Split(Convert.ToChar(Cn.GCS()));
                    dt = Convert.ToDateTime(aa[1]);
                    //dt = Convert.ToDateTime(aa[1].Remove(10)).AddDays(1);
                }
                var prodggrpcd = aa[0].Trim();
                double rate = aa[2].retDbl();
                sl = DB.M_PRODTAX.Where(e=>e.PRODGRPCD == prodggrpcd&&e.EFFDT==dt&&e.FROMRT== rate).First();
                if (sl.FROMRT == .00.retDbl()) { VE.FROMRT = ""; } else { VE.FROMRT = sl.FROMRT.retStr(); }
                if (sl.TORT == 999999999.99.retDbl()) { VE.TORT = ""; } else { VE.TORT = sl.TORT.retStr(); }
                sGRP = DB.M_PRODGRP.Find(sl.PRODGRPCD);
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
                if (sll.INACTIVE_TAG == "Y")
                {
                    VE.Checked = true;
                }
                else
                {
                    VE.Checked = false;
                }
                VE.MPRODTAX = (from i in DB.M_PRODTAX
                               where (i.PRODGRPCD == sl.PRODGRPCD && i.EFFDT == sl.EFFDT && i.FROMRT == rate)
                               select new MPRODTAX()
                               {
                                   TAXGRPCD = i.TAXGRPCD,
                                   IGSTPER = i.IGSTPER,
                                   CGSTPER = i.CGSTPER,
                                   SGSTPER = i.SGSTPER,
                                   CESSPER = i.CESSPER
                               }).OrderBy(s => s.TAXGRPCD).ToList();

                Int32 qs = 0;
                foreach (var i in VE.MPRODTAX)
                {
                    qs = qs + 1;
                    var temp = DBF.M_TAXGRP.Find(i.TAXGRPCD);
                    i.TAXGRPNM = temp.TAXGRPNM;
                    i.SLNO = Convert.ToSByte(qs);
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
            var MDT = (from j in DB.M_PRODTAX
                       join q in DB.M_PRODGRP on j.PRODGRPCD equals (q.PRODGRPCD)
                       join p in DB.M_CNTRL_HDR on j.M_AUTONO equals (p.M_AUTONO)
                       where (p.M_AUTONO == j.M_AUTONO)
                       select new
                       {
                           PRODGRPNM = q.PRODGRPNM,
                           PRODGRPCD = j.PRODGRPCD,
                           EFFDT = j.EFFDT,
                           FROMRT = j.FROMRT,
                           TORT = j.TORT
                       }).Distinct().OrderBy(s => s.PRODGRPNM).ToList();

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Group Name" + Cn.GCS() + "Group Code" + Cn.GCS() + "Date" + Cn.GCS() + "FROM RATE" + Cn.GCS() + "TO RATE";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].PRODGRPNM + "</td><td>" + MDT[j].PRODGRPCD + "</td><td>" + MDT[j].EFFDT.retDateStr() + "</td><td>" + MDT[j].FROMRT + "</td><td>" + MDT[j].TORT + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "1" + Cn.GCS() + "2" + Cn.GCS() + "3"));
        }
        public ActionResult PRODGRPCODE(string val)
        {
            try
            {
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                var query = (from c in DB.M_PRODGRP where (c.PRODGRPCD == val) select c);
                if (query.Any())
                {
                    string str = "";
                    foreach (var i in query)
                    {
                        str = i.PRODGRPCD + Cn.GCS() + i.PRODGRPNM;
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
        public ActionResult Get_Data(TaxRateProductGroupSetupEntry VE, string PRODGRPCD)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            string DBschm = CommVar.CurSchema(UNQSNO).ToString();
            string DFschm = CommVar.FinSchema(UNQSNO);
            string query = "";
            query = query + " select a.taxgrpcd, b.taxgrpnm, a.igstper, a.cgstper, a.sgstper, a.cessper   ";
            query = query + " from " + DBschm + ".m_prodtax a, " + DFschm + ".m_taxgrp b   ";
            query = query + " where a.taxgrpcd=b.taxgrpcd(+) and a.prodgrpcd='" + PRODGRPCD + "'   ";
            query = query + " union  ";
            query = query + " select b.taxgrpcd, b.taxgrpnm, 0 igstper, 0 cgstper, 0 sgstper, 0 cessper";
            query = query + " from " + DFschm + ".m_taxgrp b, " + DFschm + ".m_cntrl_hdr c       ";
            query = query + " where b.m_autono=c.m_autono(+) and nvl(c.inactive_tag,' ') <> 'Y'";
            query = query + " and b.taxgrpcd not in (  ";
            query = query + " select taxgrpcd from " + DBschm + ".m_prodtax where prodgrpcd='" + PRODGRPCD + "' )  ";
            var Recordset = Master_Help.SQLquery(query);
            VE.MPRODTAX = (from DataRow dr in Recordset.Rows
                           select new MPRODTAX()
                           {
                               TAXGRPCD = dr["taxgrpcd"].ToString(),
                               TAXGRPNM = dr["taxgrpnm"].ToString(),
                               IGSTPER = Convert.ToDouble(dr["igstper"].ToString()),
                               CGSTPER = Convert.ToDouble(dr["cgstper"].ToString()),
                               SGSTPER = Convert.ToDouble(dr["sgstper"].ToString()),
                               CESSPER = Convert.ToDouble(dr["cessper"].ToString())
                           }).ToList();

            for (int i = 0; i <= VE.MPRODTAX.Count - 1; i++)
            {//
                VE.MPRODTAX[i].SLNO = Convert.ToInt16(i + 1);
            }
            VE.DefaultView = true;
            return PartialView("_M_Tax_Rate_GRID", VE);
        }
        public ActionResult SAVE(FormCollection FC, TaxRateProductGroupSetupEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_PRODTAX MPrdTax = new M_PRODTAX();
                        MPrdTax.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            MPrdTax.EMD_NO = 0;
                            MPrdTax.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());
                        }
                        else
                        {
                            MPrdTax.M_AUTONO = VE.M_PRODTAX.M_AUTONO;
                            var MAXEMDNO = (from p in DB.M_CNTRL_HDR where p.M_AUTONO == MPrdTax.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MPrdTax.EMD_NO = 0;
                            }
                            else
                            {
                                MPrdTax.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                        }
                        //save M_PRODTAX
                        MPrdTax.PRODGRPCD = VE.M_PRODTAX.PRODGRPCD;
                        MPrdTax.EFFDT = VE.M_PRODTAX.EFFDT;

                        //remove GRID
                        if (VE.DefaultAction == "E")
                        {
                            DB.M_PRODTAX.Where(x => x.PRODGRPCD == VE.M_PRODTAX.PRODGRPCD && x.EFFDT == VE.M_PRODTAX.EFFDT).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_PRODTAX.RemoveRange(DB.M_PRODTAX.Where(x => x.PRODGRPCD == VE.M_PRODTAX.PRODGRPCD && x.EFFDT == VE.M_PRODTAX.EFFDT));
                        }

                        //tax list grid saving



                        //Control header 
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(false, "M_PRODTAX", MPrdTax.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());

                        if (VE.DefaultAction == "A")
                        {
                            //DB.M_PRODTAX.Add(MPrdTax);
                            DB.M_CNTRL_HDR.Add(MCH);
                            DB.SaveChanges();
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            //DB.Entry(MPrdTax).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        }

                        if (VE.MPRODTAX != null)
                        {
                            for (int i = 0; i <= VE.MPRODTAX.Count - 1; i++)
                            {
                                if (VE.MPRODTAX[i].TAXGRPCD != null)
                                {
                                    M_PRODTAX MPRODTAX_obj = new M_PRODTAX();
                                    MPRODTAX_obj.EMD_NO = MPrdTax.EMD_NO;
                                    MPRODTAX_obj.M_AUTONO = MPrdTax.M_AUTONO;
                                    MPRODTAX_obj.CLCD = MPrdTax.CLCD;
                                    MPRODTAX_obj.EFFDT = MPrdTax.EFFDT;
                                    MPRODTAX_obj.PRODGRPCD = MPrdTax.PRODGRPCD;
                                    if (VE.FROMRT.retDcml() == 0) { MPRODTAX_obj.FROMRT = ".00".retDbl(); }
                                    else { MPRODTAX_obj.FROMRT = VE.FROMRT.retDbl(); }
                                    if (VE.TORT.retDcml() == 0) { MPRODTAX_obj.TORT = "999999999.99".retDbl(); }
                                    else { MPRODTAX_obj.TORT = VE.TORT.retDbl(); }
                                    MPRODTAX_obj.TAXGRPCD = VE.MPRODTAX[i].TAXGRPCD;
                                    MPRODTAX_obj.IGSTPER = VE.MPRODTAX[i].IGSTPER;
                                    MPRODTAX_obj.CGSTPER = VE.MPRODTAX[i].CGSTPER;
                                    MPRODTAX_obj.SGSTPER = VE.MPRODTAX[i].SGSTPER;
                                    MPRODTAX_obj.CESSPER = VE.MPRODTAX[i].CESSPER;
                                    DB.M_PRODTAX.Add(MPRODTAX_obj);
                                }
                            }
                        }
                        else
                        {
                            return Content("**  Please Choose a Item [ You Can't Be save Without Item] **");
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
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(false, "M_PRODTAX", VE.M_PRODTAX.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_PRODTAX.Where(x => x.M_AUTONO == VE.M_PRODTAX.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_PRODTAX.Where(x => x.PRODGRPCD == VE.M_PRODTAX.PRODGRPCD && x.EFFDT == VE.M_PRODTAX.EFFDT).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.M_PRODTAX.RemoveRange(DB.M_PRODTAX.Where(x => x.PRODGRPCD == VE.M_PRODTAX.PRODGRPCD && x.EFFDT == VE.M_PRODTAX.EFFDT));
                        DB.SaveChanges();
                        DB.M_PRODTAX.RemoveRange(DB.M_PRODTAX.Where(x => x.M_AUTONO == VE.M_PRODTAX.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_PRODTAX.M_AUTONO));
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