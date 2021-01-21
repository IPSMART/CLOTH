using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Data;

namespace Improvar.Controllers
{
    public class M_GodownController : Controller
    {
        Connection Cn = new Connection(); MasterHelp Master_Help = new MasterHelp(); M_GODOWN sl; M_CNTRL_HDR sll;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_Godown
        public ActionResult M_Godown(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Godown Master";
                    GodownMasterEntry VE = new GodownMasterEntry();
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);

                    //=========For Company / Location Name===========//
                    //VE.CompanyLocationName = (from i in DBF.M_LOCA
                    //                          join l in DBF.M_COMP on i.COMPCD equals (l.COMPCD)
                    //                          select new CompanyLocationName() { COMPCD = l.COMPCD, COMPNM = l.COMPNM, LOCCD = i.LOCCD, LOCNM = i.LOCNM }).OrderBy(s => s.LOCNM).ToList();
                    VE.CompanyLocationName = (from i in DBF.M_LOCA join l in DBF.M_COMP on i.COMPCD equals (l.COMPCD) select new CompanyLocationName() { COMPCD = l.COMPCD, COMPNM = l.COMPNM, LOCCD = i.LOCCD, LOCNM = i.LOCNM }).OrderBy(s => s.COMPNM).ThenBy(k => k.LOCNM).ToList();
                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DBF.M_GODOWN orderby p.M_AUTONO select new IndexKey() { Navikey = p.GOCD }).ToList();
                        if (op == "E" || op == "D" || op == "V")
                        {
                            if (searchValue.Length != 0)
                            {
                                VE.Index = Nindex;
                                VE = Navigation(VE, DBF, 0, searchValue);
                            }
                            else
                            {
                                if (key == "F")
                                {
                                    VE.Index = 0;
                                    VE = Navigation(VE, DBF, 0, searchValue);
                                }
                                else if (key == "" || key == "L")
                                {
                                    VE.Index = VE.IndexKey.Count - 1;
                                    VE = Navigation(VE, DBF, VE.IndexKey.Count - 1, searchValue);
                                }
                                else if (key == "P")
                                {
                                    Nindex -= 1;
                                    if (Nindex < 0)
                                    {
                                        Nindex = 0;
                                    }
                                    VE.Index = Nindex;
                                    VE = Navigation(VE, DBF, Nindex, searchValue);
                                }
                                else if (key == "N")
                                {
                                    Nindex += 1;
                                    if (Nindex > VE.IndexKey.Count - 1)
                                    {
                                        Nindex = VE.IndexKey.Count - 1;
                                    }
                                    VE.Index = Nindex;
                                    VE = Navigation(VE, DBF, Nindex, searchValue);
                                }
                            }
                            VE.M_GODOWN = sl;
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
                GodownMasterEntry VE = new GodownMasterEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public GodownMasterEntry Navigation(GodownMasterEntry VE, ImprovarDB DBF, int index, string searchValue)
        {
            sl = new M_GODOWN(); sll = new M_CNTRL_HDR();
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
                sl = DBF.M_GODOWN.Find(aa[0].Trim());
                sll = DBF.M_CNTRL_HDR.Find(sl.M_AUTONO);
                if (sl.LOCCD != null) VE.LOCNM = DBF.M_LOCA.Where(a => a.LOCCD == sl.LOCCD).Select(a => a.LOCNM).FirstOrDefault();  
                //company view
                //  ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));

                //var ss = (from p in DB.M_CNTRL_LOCA where (p.M_AUTONO == sl.M_AUTONO) select new CompanyLocationName() { COMPCD = p.COMPCD, LOCCD = p.LOCCD, Checked = true }).ToList();
                //foreach (var a in ss)
                //{
                //    var comp = DBF.M_COMP.Find(a.COMPCD);
                //    var loc = (from p in DBF.M_LOCA where p.LOCCD == a.LOCCD && p.COMPCD == a.COMPCD select p).SingleOrDefault();
                //    a.COMPNM = comp.COMPNM;
                //    a.LOCNM = loc.LOCNM;
                //}
                //foreach (var i in VE.CompanyLocationName)
                //{
                //    foreach (var x in ss)
                //    {
                //        if (i.COMPCD == x.COMPCD && i.LOCCD == x.LOCCD)
                //        {
                //            i.Checked = true;
                //        }
                //    }
                //}
                string str = "select p.COMPCD,s.COMPNM,p.LOCCD,x.LOCNM from " + CommVar.FinSchema(UNQSNO) + ".M_CNTRL_LOCA p,"
                    + CommVar.FinSchema(UNQSNO) + ".M_COMP s," + CommVar.FinSchema(UNQSNO) + ".M_LOCA x where ";
                str += "p.COMPCD=s.COMPCD and p.LOCCD=x.LOCCD and p.M_AUTONO ='" + sl.M_AUTONO + "'";
                var DATA_CLN = Master_Help.SQLquery(str);

                var CLN = (from DataRow DR in DATA_CLN.Rows
                           select new CompanyLocationChk()
                           {
                               COMPCD = DR["COMPCD"].ToString(),
                               COMPNM = DR["COMPNM"].ToString(),
                               LOCCD = DR["LOCCD"].ToString(),
                               LOCNM = DR["LOCNM"].ToString(),
                               Checked = true
                           }).ToList();


                if (VE.CompanyLocationName != null)
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
                //END COMP
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
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            var MDT = (from j in DBF.M_GODOWN
                       join p in DBF.M_CNTRL_HDR on j.M_AUTONO equals (p.M_AUTONO)
                       where (p.M_AUTONO == j.M_AUTONO)
                       select new
                       {
                           GOCD = j.GOCD,
                           GONM = j.GONM
                       }).Distinct().OrderBy(s => s.GOCD).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Godown Code" + Cn.GCS() + "Godown Name";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].GOCD + "</td><td>" + MDT[j].GONM + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "0"));
        }
        public ActionResult CheckGodownCode(string val)
        {
            string VALUE = val.ToUpper();
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            var query = (from c in DBF.M_GODOWN where (c.GOCD == VALUE) select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult SAVE(FormCollection FC, GodownMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            string Autono = "";
            //using (var transaction = DB.Database.BeginTransaction())
            //{
            using (var transactionF = DBF.Database.BeginTransaction())
            {
                try
                {
                    DBF.Database.ExecuteSqlCommand("lock table " + CommVar.FinSchema(UNQSNO) + ".M_CNTRL_HDR in  row share mode");
                    string scmf = CommVar.FinSchema(UNQSNO);
                    string qry = "select * from " + scmf + ".M_GODOWN a ," + scmf + ".M_CNTRL_LOCA c ";
                    qry += "where  a.M_AUTONO=c.M_AUTONO(+) and a.GOCD='" + VE.M_GODOWN.GOCD + "' ";
                    DataTable dt = Master_Help.SQLquery(qry);
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        //M_GODOWN MGOD = new M_GODOWN();
                        //MGOD.CLCD = CommVar.ClientCode(UNQSNO);
                        //if (VE.DefaultAction == "A")
                        //{
                        //    MGOD.EMD_NO = 0;
                        //    MGOD.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO));
                        //}
                        //else
                        //{
                        //    MGOD.M_AUTONO = VE.M_GODOWN.M_AUTONO;
                        //    var MAXEMDNO = (from p in DB.M_CNTRL_HDR where p.M_AUTONO == MGOD.M_AUTONO select p.EMD_NO).Max();
                        //    if (MAXEMDNO == null)
                        //    {
                        //        MGOD.EMD_NO = 0;
                        //    }
                        //    else
                        //    {
                        //        MGOD.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                        //    }
                        //}

                        //MGOD.GOCD = VE.M_GODOWN.GOCD.ToUpper();
                        //MGOD.GONM = VE.M_GODOWN.GONM;
                        //MGOD.GOADD1 = VE.M_GODOWN.GOADD1;
                        //MGOD.GOADD2 = VE.M_GODOWN.GOADD2;
                        //MGOD.GOADD3 = VE.M_GODOWN.GOADD3;
                        //MGOD.DISTRICT = VE.M_GODOWN.DISTRICT;
                        //MGOD.PIN = VE.M_GODOWN.PIN;
                        //MGOD.GOPHNO = VE.M_GODOWN.GOPHNO;
                        //MGOD.GOEMAIL = VE.M_GODOWN.GOEMAIL;
                        //MGOD.FSSAILICNO = VE.M_GODOWN.FSSAILICNO;
                        ////company saving    
                        //if (VE.DefaultAction == "E")
                        //{
                        //    DBF.M_CNTRL_LOCA.Where(x => x.M_AUTONO == MGOD.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                        //    DBF.M_CNTRL_LOCA.RemoveRange(DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == MGOD.M_AUTONO));
                        //}
                        //if (VE.CompanyLocationName != null)
                        //{
                        //    for (int i = 0; i <= VE.CompanyLocationName.Count - 1; i++)
                        //    {
                        //        if (VE.CompanyLocationName[i].Checked)
                        //        {
                        //            M_CNTRL_LOCA MCL = new M_CNTRL_LOCA();
                        //            MCL.M_AUTONO = MGOD.M_AUTONO;
                        //            MCL.EMD_NO = MGOD.EMD_NO;
                        //            MCL.CLCD = CommVar.ClientCode(UNQSNO);
                        //            MCL.COMPCD = VE.CompanyLocationName[i].COMPCD;
                        //            MCL.LOCCD = VE.CompanyLocationName[i].LOCCD;
                        //            DBF.M_CNTRL_LOCA.Add(MCL);
                        //        }
                        //    }
                        //}

                        ////Control header 
                        //M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_GODOWN", MGOD.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO));
                        //if (VE.DefaultAction == "A")
                        //{
                        //    DB.M_GODOWN.Add(MGOD);
                        //    DB.M_CNTRL_HDR.Add(MCH);

                        //}
                        //else if (VE.DefaultAction == "E")
                        //{
                        //    DB.Entry(MGOD).State = System.Data.Entity.EntityState.Modified;
                        //    DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        //}
                        #region //Finance Posting
                        M_GODOWN MGODF = new M_GODOWN();
                        MGODF.CLCD = CommVar.ClientCode(UNQSNO);
                        MGODF.GOCD = VE.M_GODOWN.GOCD.ToUpper();
                        MGODF.GONM = VE.M_GODOWN.GONM;
                        MGODF.GOADD1 = VE.M_GODOWN.GOADD1;
                        MGODF.GOADD2 = VE.M_GODOWN.GOADD2;
                        MGODF.GOADD3 = VE.M_GODOWN.GOADD3;
                        MGODF.DISTRICT = VE.M_GODOWN.DISTRICT;
                        MGODF.PIN = VE.M_GODOWN.PIN;
                        MGODF.GOPHNO = VE.M_GODOWN.GOPHNO;
                        MGODF.GOEMAIL = VE.M_GODOWN.GOEMAIL;
                        MGODF.FSSAILICNO = VE.M_GODOWN.FSSAILICNO;
                        MGODF.LOCCD = VE.M_GODOWN.LOCCD;
                        MGODF.FLAG1 = VE.M_GODOWN.FLAG1;
                        if (dt.Rows.Count > 0)
                        {
                            MGODF.M_AUTONO = dt.Rows[0]["M_AUTONO"].retInt();
                            Autono = MGODF.M_AUTONO.retStr();
                            var MAXEMDNO = (from p in DBF.M_CNTRL_HDR where p.M_AUTONO == VE.M_GODOWN.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MGODF.EMD_NO = 0;
                            }
                            else
                            {
                                MGODF.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                            M_CNTRL_HDR MCHF = Cn.M_CONTROL_HDR(VE.Deactive, "M_GODOWN", MGODF.M_AUTONO, "E", CommVar.FinSchema(UNQSNO));
                            DBF.Entry(MGODF).State = System.Data.Entity.EntityState.Modified;
                            DBF.M_CNTRL_LOCA.Where(x => x.M_AUTONO == MGODF.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DBF.M_CNTRL_LOCA.RemoveRange(DBF.M_CNTRL_LOCA.Where(x => x.M_AUTONO == MGODF.M_AUTONO));

                        }
                        else
                        {
                            MGODF.M_AUTONO = Cn.M_AUTONO(CommVar.FinSchema(UNQSNO));
                            Autono = MGODF.M_AUTONO.retStr();
                            MGODF.EMD_NO = 0;
                            // Finance Control header 
                            M_CNTRL_HDR MCHF = Cn.M_CONTROL_HDR(VE.Deactive, "M_GODOWN", MGODF.M_AUTONO, "A", CommVar.FinSchema(UNQSNO));
                            DBF.M_GODOWN.Add(MGODF);
                            DBF.M_CNTRL_HDR.Add(MCHF);
                        }
                        //Finance company saving    
                        if (VE.CompanyLocationName != null)
                        {
                            for (int i = 0; i <= VE.CompanyLocationName.Count - 1; i++)
                            {
                                if (VE.CompanyLocationName[i].Checked)
                                {
                                    M_CNTRL_LOCA MCLF = new M_CNTRL_LOCA();
                                    MCLF.M_AUTONO = MGODF.M_AUTONO;
                                    MCLF.EMD_NO = MGODF.EMD_NO;
                                    MCLF.CLCD = CommVar.ClientCode(UNQSNO);
                                    MCLF.COMPCD = VE.CompanyLocationName[i].COMPCD;
                                    MCLF.LOCCD = VE.CompanyLocationName[i].LOCCD;
                                    DBF.M_CNTRL_LOCA.Add(MCLF);
                                }
                            }
                        }
                        #endregion  //end
                        //DB.SaveChanges();
                        DBF.SaveChanges();
                        ModelState.Clear();
                        transactionF.Commit();
                        //transaction.Commit();
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
                        #region //Finance
                        if (dt.Rows.Count > 0)
                        {
                            int Autono1 = dt.Rows[0]["M_AUTONO"].retInt();
                            M_CNTRL_HDR MCHF = Cn.M_CONTROL_HDR(VE.Deactive, "M_GODOWN", Autono1, VE.DefaultAction, CommVar.FinSchema(UNQSNO));
                            DBF.Entry(MCHF).State = System.Data.Entity.EntityState.Modified;
                            DBF.SaveChanges();
                            DBF.M_GODOWN.Where(x => x.M_AUTONO == Autono1).ToList().ForEach(x => { x.DTAG = "D"; });
                            DBF.M_CNTRL_LOCA.Where(x => x.M_AUTONO == Autono1).ToList().ForEach(x => { x.DTAG = "D"; });
                            DBF.SaveChanges();
                            DBF.M_CNTRL_LOCA.RemoveRange(DBF.M_CNTRL_LOCA.Where(x => x.M_AUTONO == Autono1));
                            DBF.SaveChanges();
                            DBF.M_GODOWN.RemoveRange(DBF.M_GODOWN.Where(x => x.M_AUTONO == Autono1));
                            DBF.SaveChanges();
                            DBF.M_CNTRL_HDR.RemoveRange(DBF.M_CNTRL_HDR.Where(x => x.M_AUTONO == Autono1));
                            DBF.SaveChanges();

                        }
                        #endregion

                        //M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Deactive, "M_GODOWN", VE.M_GODOWN.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO));
                        //DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        //DB.SaveChanges();

                        //DB.M_GODOWN.Where(x => x.M_AUTONO == VE.M_GODOWN.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        //DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == VE.M_GODOWN.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        //DB.SaveChanges();
                        //DB.M_CNTRL_LOCA.RemoveRange(DB.M_CNTRL_LOCA.Where(x => x.M_AUTONO == VE.M_GODOWN.M_AUTONO));
                        //DB.SaveChanges();
                        //DB.M_GODOWN.RemoveRange(DB.M_GODOWN.Where(x => x.M_AUTONO == VE.M_GODOWN.M_AUTONO));
                        //DB.SaveChanges();


                        //DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_GODOWN.M_AUTONO));
                        //DB.SaveChanges();

                        ModelState.Clear();
                        transactionF.Commit();
                        //transaction.Commit();
                        return Content("3");
                    }
                    else
                    {
                        return Content("");
                    }
                }
                catch (Exception ex)
                {
                    transactionF.Rollback();
                    //transaction.Rollback();
                    Cn.SaveException(ex, "");
                    return Content(ex.Message + ex.InnerException);
                }
            }
            // }
        }
        public ActionResult GetLocationDetails(string val)
        {
            var str = Master_Help.Location_Help(val);
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