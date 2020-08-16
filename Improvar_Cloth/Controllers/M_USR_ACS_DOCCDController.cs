using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Collections;
using System.Data;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Microsoft.Ajax.Utilities;

namespace Improvar.Controllers
{
    public class M_USR_ACS_DOCCDController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        M_USR_ACS_DOCCD sl;
        // GET: M_USR_ACS_DOCCD
        public ActionResult M_USR_ACS_DOCCD(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            if (Session["UR_ID"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                ViewBag.formname = "User Wise Document Type Rights";
                ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
                ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                UserWiseDocumentTypeRightsEntry VE = new UserWiseDocumentTypeRightsEntry(); Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                string loccd = CommVar.Loccd(UNQSNO);
                string Compcd = CommVar.Compcd(UNQSNO);
                VE.User = (from i in DBI.USER_APPL select new User() { Value = i.USER_ID, Text = i.USER_NAME }).OrderBy(s => s.Text).ToList();
                if (op.Length != 0)
                {
                    string GCS = Cn.GCS();
                    VE.IndexKey = (from p in DB.M_USR_ACS_DOCCD
                                   where p.LOCCD == loccd && p.COMPCD == Compcd
                                   orderby p.DOCCD
                                   select new IndexKey() { Navikey = p.USER_ID }).Distinct().ToList();

                    if (op == "E" || op == "D" || op == "V")
                    {
                        if (searchValue != "") { Nindex = VE.IndexKey.FindIndex(r => r.Navikey.Equals(searchValue)); }
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
                        VE.M_USR_ACS_DOCCD = sl;
                    }
                    else if (VE.DefaultAction == "A")
                    {


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
        public UserWiseDocumentTypeRightsEntry Navigation(UserWiseDocumentTypeRightsEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            string loccd = CommVar.Loccd(UNQSNO);
            string Compcd = CommVar.Compcd(UNQSNO);
            sl = new M_USR_ACS_DOCCD();
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
                string usrid = aa[0].Trim();
                sl = DB.M_USR_ACS_DOCCD.Where(e => e.USER_ID == usrid && e.LOCCD == loccd && e.COMPCD == Compcd).First();
                VE.userID = sl.USER_ID;
                if (VE.DefaultAction == "E")
                {
                    var sql = "";
                    sql += "select a.doccd,b.docnm,c.DNAME,'YES' isfound ";
                    sql += " from " + CommVar.CurSchema(UNQSNO) + ".M_USR_ACS_DOCCD a, " + CommVar.CurSchema(UNQSNO) + ".M_DOCTYPE b, " + CommVar.CurSchema(UNQSNO) + ".M_DTYPE c";
                    sql += " where a.doccd=b.doccd and b.DOCTYPE=c.dcd AND a.user_id='" + usrid + "' AND A.loccd='" + CommVar.Loccd(UNQSNO) + "' AND COMPCD='" + CommVar.Compcd(UNQSNO) + "'";
                    sql += " union all";
                    sql += " select b.doccd,b.docnm,c.DNAME ,'NO' isfound";
                    sql += " from  " + CommVar.CurSchema(UNQSNO) + ".M_DOCTYPE b, " + CommVar.CurSchema(UNQSNO) + ".M_DTYPE c";
                    sql += " where b.DOCTYPE=c.dcd AND doccd not in (select a.doccd from  " + CommVar.CurSchema(UNQSNO) + ".M_USR_ACS_DOCCD a where a.user_id='"
                        + usrid + "' AND A.loccd='" + CommVar.Loccd(UNQSNO) + "' AND COMPCD='" + CommVar.Compcd(UNQSNO) + "')";
                    var dt = masterHelp.SQLquery(sql);
                    VE.MUSRACSDOCCD = (from DataRow dr in dt.Rows
                                       select new MUSRACSDOCCD
                                       {
                                           DNAME = dr["DNAME"].ToString(),
                                           DOCCD = dr["DOCCD"].ToString(),
                                           DOCNM = dr["DOCNM"].ToString(),
                                           Checked = dr["isfound"].ToString() == "YES" ? true : false
                                       }).ToList();
                }
                else
                {
                    VE.MUSRACSDOCCD = (from i in DB.M_USR_ACS_DOCCD
                                       join j in DB.M_DOCTYPE on i.DOCCD equals j.DOCCD
                                       join k in DB.M_DTYPE on j.DOCTYPE equals k.DCD
                                       where (i.USER_ID == VE.userID)
                                       select new MUSRACSDOCCD { DNAME = k.DNAME, DOCCD = i.DOCCD, DOCNM = j.DOCNM, Checked = true }).OrderBy(s => s.DNAME).ThenBy(e => e.DOCNM).ToList();
                }
            }
            return VE;
        }
        public ActionResult SearchPannelData()
        {
            ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            var query = (from j in DB.M_USR_ACS_DOCCD
                         select new
                         {
                             USER_ID = j.USER_ID,
                             DOCCD = j.DOCCD
                         }).OrderBy(s => s.USER_ID).ToList();
            var tquery = from q in DBI.USER_APPL
                         select q;
            var MDT = (from a in query
                       join b in tquery on a.USER_ID equals b.USER_ID
                       select new
                       {
                           USER_ID = a.USER_ID,
                           USER_NAME = b.USER_NAME,
                           USER_TYPE = b.USER_TYPE,
                           DOCCD = a.DOCCD
                       }).DistinctBy(a => a.USER_ID).ToList();

            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "User ID" + Cn.GCS() + "User Name" + Cn.GCS() + "User Type" + Cn.GCS() + "Document Type";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                var USER_TYPE = "";
                if (MDT[j].USER_TYPE == "A")
                {
                    USER_TYPE = "Admin";

                }
                else if (MDT[j].USER_TYPE == "N")
                {
                    USER_TYPE = "Normal User";

                }
                else if (MDT[j].USER_TYPE == "E")
                {
                    USER_TYPE = "Entry";

                }
                else if (MDT[j].USER_TYPE == "M")
                {
                    USER_TYPE = "Master";

                }
                else if (MDT[j].USER_TYPE == "R")
                {
                    USER_TYPE = "Report";

                }
                SB.Append("<tr><td>" + MDT[j].USER_ID + "</td><td>" + MDT[j].USER_NAME + "</td><td>" + USER_TYPE + "</td><td>" + MDT[j].DOCCD + "</td></tr>");
            }
            return PartialView("_SearchPannel2", masterHelp.Generate_SearchPannel(hdr, SB.ToString(), "0" + Cn.GCS() + "3", "3"));
        }
        public ActionResult Get_Rights(UserWiseDocumentTypeRightsEntry VE, string user)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            //string QUERY = "select b.dname, a.doccd, a.docnm from sd_hpcc2017.m_doctype a, sd_hpcc2017.m_dtype b where a.doctype=b.dcd(+)";
            VE.MUSRACSDOCCD = (from i in DB.M_DOCTYPE
                               join j in DB.M_DTYPE on i.DOCTYPE equals j.DCD
                               where (i.DOCTYPE == j.DCD)
                               select new MUSRACSDOCCD { DNAME = j.DNAME, DOCCD = i.DOCCD, DOCNM = i.DOCNM }).OrderBy(s => s.DNAME).ToList();

            for (int i = 0; i <= VE.MUSRACSDOCCD.Count - 1; i++)
            {
                string DOC = VE.MUSRACSDOCCD[i].DOCCD;
                var temp = (from z in DB.M_USR_ACS_DOCCD where z.USER_ID == user && z.DOCCD == DOC select z).ToList();

                if (temp.Count != 0)
                {
                    VE.MUSRACSDOCCD[i].Checked = true;
                }
            }

            VE.DefaultView = true;
            return PartialView("_M_USR_ACS_DOCCD_GRID", VE);
        }
        public ActionResult SAVE(FormCollection FC, UserWiseDocumentTypeRightsEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO));
            using (var transaction = DB.Database.BeginTransaction())
            {
                string COMCODE = CommVar.Compcd(UNQSNO);
                string LOCCODE = CommVar.Loccd(UNQSNO);
                string USER = VE.userID;
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO) + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_USR_ACS_DOCCD MPTX = new M_USR_ACS_DOCCD();
                        MPTX.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            MPTX.EMD_NO = 0;
                        }
                        else
                        {
                            var MAXEMDNO = DB.M_USR_ACS_DOCCD.Where(x => x.USER_ID == USER && x.COMPCD == COMCODE && x.LOCCD == LOCCODE).Max(s => s.EMD_NO);
                            if (MAXEMDNO == null)
                            {
                                MPTX.EMD_NO = 0;
                            }
                            else
                            {
                                MPTX.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }
                        }
                        if (VE.DefaultAction == "E")
                        {
                            DB.M_USR_ACS_DOCCD.Where(x => x.USER_ID == USER).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_USR_ACS_DOCCD.RemoveRange(DB.M_USR_ACS_DOCCD.Where(x => x.USER_ID == USER));
                        }
                        if (VE.MUSRACSDOCCD != null)
                        {
                            for (int i = 0; i <= VE.MUSRACSDOCCD.Count - 1; i++)
                            {
                                if (VE.MUSRACSDOCCD[i].Checked == true)
                                {
                                    M_USR_ACS_DOCCD USRACSDOCCD = new M_USR_ACS_DOCCD();
                                    USRACSDOCCD.USER_ID = USER;
                                    USRACSDOCCD.DOCCD = VE.MUSRACSDOCCD[i].DOCCD;
                                    USRACSDOCCD.EMD_NO = MPTX.EMD_NO;
                                    USRACSDOCCD.CLCD = CommVar.ClientCode(UNQSNO);
                                    USRACSDOCCD.SCHEMA_NAME = CommVar.CurSchema(UNQSNO);
                                    USRACSDOCCD.COMPCD = COMCODE;
                                    USRACSDOCCD.LOCCD = LOCCODE;
                                    USRACSDOCCD.USR_ID = Session["UR_ID"].ToString();
                                    USRACSDOCCD.USR_ENTDT = System.DateTime.Now;
                                    USRACSDOCCD.USR_LIP = Cn.GetIp();
                                    USRACSDOCCD.USR_SIP = Cn.GetIp();
                                    USRACSDOCCD.USR_OS = null;
                                    USRACSDOCCD.USR_MNM = Cn.DetermineCompName(Cn.GetIp());
                                    DB.M_USR_ACS_DOCCD.Add(USRACSDOCCD);
                                }
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
                        DB.M_USR_ACS_DOCCD.Where(x => x.USER_ID == USER).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();
                        DB.M_USR_ACS_DOCCD.RemoveRange(DB.M_USR_ACS_DOCCD.Where(x => x.USER_ID == USER));
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