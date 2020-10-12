using Improvar.Models;
using Improvar.ViewModels;
using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace Improvar.Controllers
{
    public class M_ItemSizeController : Controller
    {
        Connection Cn = new Connection();
        MasterHelp Master_Help = new MasterHelp();
        M_SIZE sl;
        M_CNTRL_HDR sll;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_ItemSize
        public ActionResult M_ItemSize(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Item Size Master";
                    string loca1 = CommVar.Loccd(UNQSNO).Trim();
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ItemSizeEntry VE = new ItemSizeEntry();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    VE.DefaultAction = op;
                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_SIZE orderby p.SIZECD select new IndexKey() { Navikey = p.SIZECD }).ToList();

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
                            VE.M_SIZE = sl;
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
                ItemSizeEntry VE = new ItemSizeEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public ItemSizeEntry Navigation(ItemSizeEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new M_SIZE(); sll = new M_CNTRL_HDR();
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
                sl = DB.M_SIZE.Find(aa[0]);
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);

                if (sll.INACTIVE_TAG == "Y")
                {
                    VE.Checked = true;
                }
                else
                {
                    VE.Checked = false;
                }
            }

            return VE;
        }
        public ActionResult SearchPannelData()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var MDT = (from j in DB.M_SIZE
                       join o in DB.M_CNTRL_HDR on j.M_AUTONO equals (o.M_AUTONO)
                       where (j.M_AUTONO == o.M_AUTONO)
                       select new 
                       {
                           SIZECD = j.SIZECD,
                           SIZENM = j.SIZENM,
                           PRINT_SEQ = j.PRINT_SEQ,
                           M_AUTONO = o.M_AUTONO
                       }).OrderBy(s => s.SIZECD).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Item Size Code" + Cn.GCS() + "Item Size Name" + Cn.GCS() + "Print Sequence" + Cn.GCS() + "AUTO NO";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].SIZECD + "</td><td>" + MDT[j].SIZENM + "</td><td>" + MDT[j].PRINT_SEQ + "</td><td>" + MDT[j].M_AUTONO + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "0", "3"));
        }
        public ActionResult CheckSizeCode(string val)
        {
            string VALUE = val.ToUpper();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_SIZE where (c.SIZECD == VALUE) select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }

        }
        public ActionResult SAVE(FormCollection FC, ItemSizeEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    DB.Database.ExecuteSqlCommand("lock table " + CommVar.CurSchema(UNQSNO).ToString() + ".M_CNTRL_HDR in  row share mode");
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_SIZE MSIZE = new M_SIZE();
                        MSIZE.CLCD = CommVar.ClientCode(UNQSNO);
                        if (VE.DefaultAction == "A")
                        {
                            MSIZE.EMD_NO = 0;
                            MSIZE.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());
                            MSIZE.SZBARCODE = Cn.GenMasterCode("M_SIZE", "SZBARCODE", "", 2);
                            //var MAXSZBARCODE = (from p in DB.M_SIZE select p.SZBARCODE).Max();
                            //if (MAXSZBARCODE == null)
                            //{
                            //    MSIZE.SZBARCODE = "901";
                            //}
                            //else
                            //{
                            //    MSIZE.SZBARCODE = (Convert.ToInt16(MAXSZBARCODE)+1).retStr();
                            //}
                        }
                        if (VE.DefaultAction == "E")
                        {
                            MSIZE.DTAG = "E";
                            MSIZE.M_AUTONO = VE.M_SIZE.M_AUTONO;
                            MSIZE.SZBARCODE = VE.M_SIZE.SZBARCODE;
                            var MAXEMDNO = (from p in DB.M_SIZE where p.M_AUTONO == VE.M_SIZE.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MSIZE.EMD_NO = 0;
                            }
                            else
                            {
                                MSIZE.EMD_NO = Convert.ToByte( MAXEMDNO+1);
                            }
                        }
                        MSIZE.SIZECD = VE.M_SIZE.SIZECD.ToUpper();
                        MSIZE.SIZENM = VE.M_SIZE.SIZENM;
                        MSIZE.ALTSIZENM = VE.M_SIZE.ALTSIZENM;
                        MSIZE.PRINT_SEQ = VE.M_SIZE.PRINT_SEQ;

                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SIZE", MSIZE.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        if (VE.DefaultAction == "A")
                        {
                            DB.M_SIZE.Add(MSIZE);
                            DB.M_CNTRL_HDR.Add(MCH);
                        }
                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MSIZE).State = System.Data.Entity.EntityState.Modified;
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
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SIZE", VE.M_SIZE.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_SIZE.Where(x => x.M_AUTONO == VE.M_SIZE.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_SIZE.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();
                        DB.M_SIZE.RemoveRange(DB.M_SIZE.Where(x => x.M_AUTONO == VE.M_SIZE.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_SIZE.M_AUTONO));
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

    }
}