using Improvar.Models;
using Improvar.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace Improvar.Controllers
{
    public class M_FinProductController : Controller
    {
        Connection Cn = new Connection();
        string CS = null;
        MasterHelp Master_Help = new MasterHelp();
        MasterHelpFa Master_HelpFa = new MasterHelpFa();
        M_SITEM sl; M_CNTRL_HDR sll; M_GROUP slll; M_SUBBRAND slsb; M_BRAND slb; M_COLLECTION slc; M_COLOR slco; M_UOM sluom;
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: M_FinProduct
        public ActionResult M_FinProduct(string op = "", string key = "", int Nindex = 0, string searchValue = "")
        {
            try
            {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = "Finish Product Master";
                    ImprovarDB DB1 = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    var doctP = (from i in DB1.MS_DOCCTG select new DocumentType() { value = i.DOC_CTG, text = i.DOC_CTG }).OrderBy(s => s.text).ToList();
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ItemMasterEntry VE = new ItemMasterEntry();
                    Cn.getQueryString(VE); Cn.ValidateMenuPermission(VE);
                    VE.Database_Combo1 = (from n in DB.M_SITEM
                                          join j in DB.M_GROUP on n.ITGRPCD equals j.ITGRPCD
                                          where j.ITGRPTYPE == "F"
                                          select new Database_Combo1() { FIELD_VALUE = n.HSNCODE }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                    VE.Database_Combo2 = (from n in DB.M_SITEM_MEASURE
                                          select new Database_Combo2() { FIELD_VALUE = n.MDESC }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

                    //=================For Gender================//
                    List<Gender> G = new List<Gender>();
                    Gender G1 = new Gender();
                    G1.text = "Boys";
                    G1.value = "B";
                    G.Add(G1);
                    Gender G2 = new Gender();
                    G2.text = "Girls";
                    G2.value = "G";
                    G.Add(G2);
                    Gender G3 = new Gender();
                    G3.text = "Infant";
                    G3.value = "I";
                    G.Add(G3);
                    Gender G4 = new Gender();
                    G4.text = "Men";
                    G4.value = "M";
                    G.Add(G4);
                    Gender G5 = new Gender();
                    G5.text = "Women";
                    G5.value = "W";
                    G.Add(G5);
                    Gender G6 = new Gender();
                    G6.text = "Unisex";
                    G6.value = "U";
                    G.Add(G6);
                    VE.Gender = G;
                    //=================For Gender================//
                    //=================For ProductType================//
                    List<ProductType> P = new List<ProductType>();
                    ProductType P1 = new ProductType();
                    P1.text = "Inner Wear";
                    P1.value = "IW";
                    P.Add(P1);
                    ProductType P2 = new ProductType();
                    P2.text = "Outer Wear";
                    P2.value = "OW";
                    P.Add(P2);
                    VE.ProductType = P;
                    ProductType P3 = new ProductType();
                    P3.text = "Accessories";
                    P3.value = "AC";
                    P.Add(P3);
                    VE.ProductType = P;
                    //=================For ProductType================//
                    VE.DefaultAction = op;
                    if (op.Length != 0)
                    {
                        VE.IndexKey = (from p in DB.M_SITEM join o in DB.M_GROUP on p.ITGRPCD equals (o.ITGRPCD) where (p.ITGRPCD == o.ITGRPCD && o.ITGRPTYPE == "F") select new IndexKey() { Navikey = p.ITCD }).OrderBy(a => a.Navikey).ToList();
                        if (searchValue != "") { Nindex = VE.IndexKey.FindIndex(r => r.Navikey.Equals(searchValue)); }

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
                            VE.M_SITEM = sl;
                            VE.M_CNTRL_HDR = sll;
                            VE.M_GROUP = slll;
                            VE.M_SUBBRAND = slsb;
                            VE.M_BRAND = slb;
                            VE.M_COLLECTION = slc;
                            VE.M_COLOR = slco;
                            VE.M_UOM = sluom;
                        }
                        if (op.ToString() == "A")
                        {
                            List<MSITEMSIZE> ITEMSIZE = new List<MSITEMSIZE>();
                            MSITEMSIZE MIS = new MSITEMSIZE();
                            MIS.SRLNO = "1";
                            ITEMSIZE.Add(MIS);
                            VE.MSITEMSIZE = ITEMSIZE;

                            //List<MSITEMBOX> ITEMBOX = new List<MSITEMBOX>();
                            //MSITEMBOX MIB = new MSITEMBOX();
                            //MIB.SRLNO = "1";
                            //ITEMBOX.Add(MIB);
                            //VE.MSITEMBOX = ITEMBOX;

                            List<MSITEMCOLOR> ITEMCOLOR = new List<MSITEMCOLOR>();
                            MSITEMCOLOR MIC = new MSITEMCOLOR();
                            MIC.SLNO = 1;
                            ITEMCOLOR.Add(MIC);
                            VE.MSITEMCOLOR = ITEMCOLOR;

                            List<MSITEMPARTS> ITEMPARTS = new List<MSITEMPARTS>();
                            MSITEMPARTS MIP = new MSITEMPARTS();
                            MIP.SLNO = 1;
                            ITEMPARTS.Add(MIP);
                            VE.MSITEMPARTS = ITEMPARTS;

                            //List<MSITEMINVCD> ITEMINVCD = new List<MSITEMINVCD>();
                            //MSITEMINVCD MII = new MSITEMINVCD();
                            //MII.SRLNO = 1;
                            //ITEMINVCD.Add(MII);
                            //VE.MSITEMINVCD = ITEMINVCD;

                            List<MSITEMMEASURE> ITEMMEASURE = new List<MSITEMMEASURE>();
                            for (int i = 0; i < 10; i++)
                            {
                                MSITEMMEASURE MIM = new MSITEMMEASURE();
                                MIM.SLNO = Convert.ToByte(i + 1);
                                ITEMMEASURE.Add(MIM);
                            }
                            VE.MSITEMMEASURE = ITEMMEASURE;

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
                ItemMasterEntry VE = new ItemMasterEntry();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public ItemMasterEntry Navigation(ItemMasterEntry VE, ImprovarDB DB, int index, string searchValue)
        {
            sl = new M_SITEM(); sll = new M_CNTRL_HDR(); slll = new M_GROUP(); slsb = new M_SUBBRAND(); slb = new M_BRAND(); slc = new M_COLLECTION(); slco = new M_COLOR(); sluom = new M_UOM();
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
                sl = DB.M_SITEM.Find(aa[0]);
                sll = DB.M_CNTRL_HDR.Find(sl.M_AUTONO);
                slll = DB.M_GROUP.Find(sl.ITGRPCD);
                slsb = DB.M_SUBBRAND.Find(sl.SBRANDCD);
                slb = DB.M_BRAND.Find(sl.BRANDCD);
                slc = DB.M_COLLECTION.Find(sl.COLLCD);
                slco = DB.M_COLOR.Find(sl.COLRCD);
                sluom = DBF.M_UOM.Find(sl.UOMCD);
                if (sll.INACTIVE_TAG == "Y")
                {
                    VE.Checked = true;
                }
                else
                {
                    VE.Checked = false;
                }
                VE.MSITEMSIZE = (from i in DB.M_SITEM_SIZE
                                 join j in DB.M_SIZE on i.SIZECD equals (j.SIZECD)
                                 where (i.ITCD == sl.ITCD)
                                 orderby j.PRINT_SEQ
                                 select new MSITEMSIZE()
                                 {
                                     CLCD = i.CLCD,
                                     ITCD = i.ITCD,
                                     SIZECD = i.SIZECD,
                                     SIZENM = j.SIZENM,
                                     IChecked = i.INACTIVE_TAG == "Y" ? true : false,
                                 }).ToList();

                if (VE.MSITEMSIZE.Count == 0)
                {
                    List<MSITEMSIZE> ITEMSIZE = new List<MSITEMSIZE>();
                    MSITEMSIZE MIS = new MSITEMSIZE();
                    MIS.SRLNO = "1";
                    ITEMSIZE.Add(MIS);
                    VE.MSITEMSIZE = ITEMSIZE;
                }
                else
                {
                    for (int i = 0; i <= VE.MSITEMSIZE.Count - 1; i++)
                    {
                        VE.MSITEMSIZE[i].SRLNO = (i + 1).ToString();
                    }
                }
                //VE.MSITEMBOX = (from i in DB.M_SITEM_BOX
                //                join j in DB.M_SIZE on i.SIZECD equals (j.SIZECD)
                //                where (i.ITCD == sl.ITCD)
                //                select new MSITEMBOX()
                //                {
                //                    CLCD = i.CLCD,
                //                    ITCD = i.ITCD,
                //                    SIZECDBOX = i.SIZECD,
                //                    SIZEGRP = i.SIZEGRP,
                //                    SIZENMBOX = j.SIZENM,
                //                    PCSQTY = i.PCSQTY,
                //                }).OrderBy(s => s.ITCD).ToList();

                //if (VE.MSITEMBOX.Count == 0)
                //{
                //    List<MSITEMBOX> ITEMBOX = new List<MSITEMBOX>();
                //    MSITEMBOX MIB = new MSITEMBOX();
                //    MIB.SRLNO = "1";
                //    ITEMBOX.Add(MIB);
                //    VE.MSITEMBOX = ITEMBOX;
                //}
                //else
                //{
                //    for (int i = 0; i <= VE.MSITEMBOX.Count - 1; i++)
                //    {
                //        VE.MSITEMBOX[i].SRLNO = (i + 1).ToString();
                //    }

                //}

                VE.MSITEMCOLOR = (from i in DB.M_SITEM_COLOR
                                  join j in DB.M_COLOR on i.COLRCD equals (j.COLRCD)
                                  where (i.ITCD == sl.ITCD)
                                  select new MSITEMCOLOR()
                                  {
                                      SLNO = i.SLNO,
                                      COLRCD = i.COLRCD,
                                      COLRNM = j.COLRNM,
                                      IChecked = i.INACTIVE_TAG == "Y" ? true : false,
                                  }).OrderBy(s => s.SLNO).ToList();

                if (VE.MSITEMCOLOR.Count == 0)
                {
                    List<MSITEMCOLOR> ITEMCOLOR = new List<MSITEMCOLOR>();
                    MSITEMCOLOR MIC = new MSITEMCOLOR();
                    MIC.SLNO = 1;
                    ITEMCOLOR.Add(MIC);
                    VE.MSITEMCOLOR = ITEMCOLOR;
                }

                VE.MSITEMPARTS = (from i in DB.M_SITEM_PARTS
                                  join j in DB.M_PARTS on i.PARTCD equals (j.PARTCD)
                                  where (i.ITCD == sl.ITCD)
                                  select new MSITEMPARTS()
                                  {
                                      PARTCD = i.PARTCD,
                                      PARTNM = j.PARTNM,
                                  }).ToList();

                if (VE.MSITEMPARTS.Count == 0)
                {
                    List<MSITEMPARTS> ITEMPARTS = new List<MSITEMPARTS>();
                    MSITEMPARTS MIP = new MSITEMPARTS();
                    MIP.SLNO = 1;
                    ITEMPARTS.Add(MIP);
                    VE.MSITEMPARTS = ITEMPARTS;
                }
                else
                {
                    for (int i = 0; i <= VE.MSITEMPARTS.Count - 1; i++)
                    {
                        VE.MSITEMPARTS[i].SLNO = Convert.ToByte(i + 1);
                    }
                }
                //VE.MSITEMINVCD = (from i in DB.M_SITEM_INVCD
                //                  join j in DB.M_COLOR on i.COLRCD equals (j.COLRCD)
                //                  join k in DB.M_SIZE on i.SIZECD equals (k.SIZECD)
                //                  where (i.ITCD == sl.ITCD)
                //                  select new MSITEMINVCD()
                //                  {
                //                      SRLNO = i.SRLNO,
                //                      SIZECD = i.SIZECD,
                //                      SIZENM = k.SIZENM,
                //                      COLRCD = i.COLRCD,
                //                      COLRNM = j.COLRNM,
                //                      BARCODE = i.BARCODE
                //                  }).OrderBy(s => s.SRLNO).ToList();

                //if (VE.MSITEMINVCD.Count == 0)
                //{
                //    List<MSITEMINVCD> ITEMINVCD = new List<MSITEMINVCD>();
                //    MSITEMINVCD MII = new MSITEMINVCD();
                //    MII.SRLNO = 1;
                //    ITEMINVCD.Add(MII);
                //    VE.MSITEMINVCD = ITEMINVCD;
                //}


                VE.MSITEMMEASURE = (from i in DB.M_SITEM_MEASURE
                                    where (i.ITCD == sl.ITCD)
                                    select new MSITEMMEASURE()
                                    {
                                        MDESC = i.MDESC,
                                        MVAL = i.MVAL,
                                        REM = i.REM,
                                    }).ToList();

                if (VE.MSITEMMEASURE.Count == 0)
                {
                    List<MSITEMMEASURE> ITEMMEASURE = new List<MSITEMMEASURE>();
                    MSITEMMEASURE MIM = new MSITEMMEASURE();
                    MIM.SLNO = 1;
                    ITEMMEASURE.Add(MIM);
                    VE.MSITEMMEASURE = ITEMMEASURE;
                }
                else
                {
                    for (int i = 0; i <= VE.MSITEMMEASURE.Count - 1; i++)
                    {
                        VE.MSITEMMEASURE[i].SLNO = Convert.ToByte((i + 1));
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

                List<MSITEMSIZE> ITEMSIZE = new List<MSITEMSIZE>();
                MSITEMSIZE MIS = new MSITEMSIZE();
                MIS.SRLNO = "1";
                ITEMSIZE.Add(MIS);
                VE.MSITEMSIZE = ITEMSIZE;

                //List<MSITEMBOX> ITEMBOX = new List<MSITEMBOX>();
                //MSITEMBOX MIB = new MSITEMBOX();
                //MIB.SRLNO = "1";
                //ITEMBOX.Add(MIB);
                //VE.MSITEMBOX = ITEMBOX;

                List<MSITEMCOLOR> ITEMCOLOR = new List<MSITEMCOLOR>();
                MSITEMCOLOR MIC = new MSITEMCOLOR();
                MIC.SLNO = 1;
                ITEMCOLOR.Add(MIC);
                VE.MSITEMCOLOR = ITEMCOLOR;

                List<MSITEMPARTS> ITEMPARTS = new List<MSITEMPARTS>();
                MSITEMPARTS MIP = new MSITEMPARTS();
                MIP.SLNO = 1;
                ITEMPARTS.Add(MIP);
                VE.MSITEMPARTS = ITEMPARTS;

                //List<MSITEMINVCD> ITEMINVCD = new List<MSITEMINVCD>();
                //MSITEMINVCD MII = new MSITEMINVCD();
                //MII.SRLNO = 1;
                //ITEMINVCD.Add(MII);
                //VE.MSITEMINVCD = ITEMINVCD;

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
            var MDT = (from j in DB.M_SITEM
                       join p in DB.M_CNTRL_HDR on j.M_AUTONO equals (p.M_AUTONO)
                       join q in DB.M_GROUP on j.ITGRPCD equals (q.ITGRPCD)
                       where (j.M_AUTONO == p.M_AUTONO && q.ITGRPTYPE == "F")
                       select new
                       {
                           ITCD = j.ITCD,
                           STYLENO = j.STYLENO,
                           ITNM = j.ITNM,
                           ITGRPCD = j.ITGRPCD,
                           ITGRPNM = q.ITGRPNM
                       }).OrderBy(s => s.ITCD).ToList();
            System.Text.StringBuilder SB = new System.Text.StringBuilder();
            var hdr = "Style No" + Cn.GCS() + "Item Name" + Cn.GCS() + "Group" + Cn.GCS() + "Group Name" + Cn.GCS() + "ItemCd";
            for (int j = 0; j <= MDT.Count - 1; j++)
            {
                SB.Append("<tr><td>" + MDT[j].STYLENO + "</td><td>" + MDT[j].ITNM + "</td><td>" + MDT[j].ITGRPCD + "</td><td>" + MDT[j].ITGRPNM + "</td><td>" + MDT[j].ITCD + "</td></tr>");
            }
            return PartialView("_SearchPannel2", Master_Help.Generate_SearchPannel(hdr, SB.ToString(), "4"));
        }
        public ActionResult AddDOCRow(ItemMasterEntry VE)
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
        public ActionResult DeleteDOCRow(ItemMasterEntry VE)
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
        public ActionResult AddRowSIZE(ItemMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            if (VE.MSITEMSIZE == null)
            {
                List<MSITEMSIZE> ITEMSIZE1 = new List<MSITEMSIZE>();
                MSITEMSIZE MIS = new MSITEMSIZE();
                MIS.SRLNO = "1";
                ITEMSIZE1.Add(MIS);
                VE.MSITEMSIZE = ITEMSIZE1;
            }
            else
            {
                List<MSITEMSIZE> ITEMSIZE = new List<MSITEMSIZE>();
                for (int i = 0; i <= VE.MSITEMSIZE.Count - 1; i++)
                {
                    MSITEMSIZE MIS = new MSITEMSIZE();
                    MIS = VE.MSITEMSIZE[i];
                    ITEMSIZE.Add(MIS);
                }
                MSITEMSIZE MIS1 = new MSITEMSIZE();
                var max = VE.MSITEMSIZE.Max(a => Convert.ToInt32(a.SRLNO));
                int SRLNO = Convert.ToInt32(max) + 1;
                MIS1.SRLNO = Convert.ToString(SRLNO);
                ITEMSIZE.Add(MIS1);
                VE.MSITEMSIZE = ITEMSIZE;

            }

            VE.DefaultView = true;
            return PartialView("_M_FinProduct_SIZE", VE);

        }
        public ActionResult DeleteRowSIZE(ItemMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            List<MSITEMSIZE> ITEMSIZE = new List<MSITEMSIZE>();
            int count = 0;
            for (int i = 0; i <= VE.MSITEMSIZE.Count - 1; i++)
            {
                if (VE.MSITEMSIZE[i].Checked == false)
                {
                    count += 1;
                    MSITEMSIZE item = new MSITEMSIZE();
                    item = VE.MSITEMSIZE[i];
                    item.SRLNO = count.ToString();
                    ITEMSIZE.Add(item);
                }

            }
            VE.MSITEMSIZE = ITEMSIZE;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_M_FinProduct_SIZE", VE);

        }
        public ActionResult AddRowBOX(ItemMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            //List<MSITEMBOX> ITEMBOX = new List<MSITEMBOX>();
            //for (int i = 0; i <= VE.MSITEMBOX.Count - 1; i++)
            //{
            //    MSITEMBOX MIB = new MSITEMBOX();
            //    MIB = VE.MSITEMBOX[i];
            //    ITEMBOX.Add(MIB);
            //}
            //MSITEMBOX MIB1 = new MSITEMBOX();
            //var max = VE.MSITEMBOX.Max(a => Convert.ToInt32(a.SRLNO));
            //int SRLNO = Convert.ToInt32(max) + 1;
            //MIB1.SRLNO = Convert.ToString(SRLNO);
            //ITEMBOX.Add(MIB1);
            //VE.MSITEMBOX = ITEMBOX;

            VE.DefaultView = true;
            return PartialView("_M_FinProduct_ASSORTEDSIZE", VE);

        }
        public ActionResult GetItemDetails(string val, string TAG)
        {
            string ITGTYPE = "";
            if (val == null)
            {
                return PartialView("_Help2", Master_Help.ARTICLE_ITEM_DETAILS(val, ITGTYPE, "", "", TAG));
            }
            else
            {
                string str = Master_Help.ARTICLE_ITEM_DETAILS(val, ITGTYPE, "", "", TAG);
                return Content(str);
            }
        }
        public ActionResult GetBrandDetails()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            return PartialView("_Help2", Master_Help.BRANDCD_help(DB));
        }
        public ActionResult BrandCode(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_BRAND where (c.BRANDCD == val) select c);
            if (query.Any())
            {
                string str = "";
                foreach (var i in query)
                {
                    str = i.BRANDCD + Cn.GCS() + i.BRANDNM;
                }
                return Content(str);
            }
            else
            {
                return Content("0");
            }
        }
        //public ActionResult DeleteRowBOX(ItemMasterEntry VE)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //    List<MSITEMBOX> ITEMBOX = new List<MSITEMBOX>();
        //    int count = 0;
        //    for (int i = 0; i <= VE.MSITEMBOX.Count - 1; i++)
        //    {
        //        if (VE.MSITEMBOX[i].Checked == false)
        //        {
        //            count += 1;
        //            MSITEMBOX item = new MSITEMBOX();
        //            item = VE.MSITEMBOX[i];
        //            item.SRLNO = count.ToString();
        //            ITEMBOX.Add(item);
        //        }
        //    }
        //    VE.MSITEMBOX = ITEMBOX;
        //    ModelState.Clear();
        //    VE.DefaultView = true;
        //    return PartialView("_M_FinProduct_ASSORTEDSIZE", VE);

        //}
        public ActionResult AddRowCOLOR(ItemMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            List<MSITEMCOLOR> ITEMCOLOR = new List<MSITEMCOLOR>();
            for (int i = 0; i <= VE.MSITEMCOLOR.Count - 1; i++)
            {
                MSITEMCOLOR MIC = new MSITEMCOLOR();
                MIC = VE.MSITEMCOLOR[i];
                ITEMCOLOR.Add(MIC);
            }
            MSITEMCOLOR MIC1 = new MSITEMCOLOR();
            var max = VE.MSITEMCOLOR.Max(a => Convert.ToInt32(a.SLNO));
            int SRLNO = Convert.ToInt32(max) + 1;
            MIC1.SLNO = Convert.ToByte(SRLNO);
            ITEMCOLOR.Add(MIC1);
            VE.MSITEMCOLOR = ITEMCOLOR;

            VE.DefaultView = true;
            return PartialView("_M_FinProduct_COLOR", VE);

        }
        public ActionResult DeleteRowCOLOR(ItemMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            List<MSITEMCOLOR> ITEMCOLOR = new List<MSITEMCOLOR>();
            int count = 0;
            for (int i = 0; i <= VE.MSITEMCOLOR.Count - 1; i++)
            {
                if (VE.MSITEMCOLOR[i].Checked == false)
                {
                    count += 1;
                    MSITEMCOLOR item = new MSITEMCOLOR();
                    item = VE.MSITEMCOLOR[i];
                    item.SLNO = Convert.ToByte(count);
                    ITEMCOLOR.Add(item);
                }
            }
            VE.MSITEMCOLOR = ITEMCOLOR;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_M_FinProduct_COLOR", VE);

        }
        public ActionResult AddRowPARTS(ItemMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            List<MSITEMPARTS> ITEMPARTS = new List<MSITEMPARTS>();
            for (int i = 0; i <= VE.MSITEMPARTS.Count - 1; i++)
            {
                MSITEMPARTS MIP = new MSITEMPARTS();
                MIP = VE.MSITEMPARTS[i];
                ITEMPARTS.Add(MIP);
            }
            MSITEMPARTS MIP1 = new MSITEMPARTS();
            var max = VE.MSITEMPARTS.Max(a => Convert.ToInt32(a.SLNO));
            int SRLNO = Convert.ToInt32(max) + 1;
            MIP1.SLNO = Convert.ToByte(SRLNO);
            ITEMPARTS.Add(MIP1);
            VE.MSITEMPARTS = ITEMPARTS;

            VE.DefaultView = true;
            return PartialView("_M_FinProduct_PART", VE);

        }
        public ActionResult DeleteRowPARTS(ItemMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            List<MSITEMPARTS> ITEMPARTS = new List<MSITEMPARTS>();
            int count = 0;
            for (int i = 0; i <= VE.MSITEMPARTS.Count - 1; i++)
            {
                if (VE.MSITEMPARTS[i].Checked == false)
                {
                    count += 1;
                    MSITEMPARTS item = new MSITEMPARTS();
                    item = VE.MSITEMPARTS[i];
                    item.SLNO = Convert.ToByte(count);
                    ITEMPARTS.Add(item);
                }
            }
            VE.MSITEMPARTS = ITEMPARTS;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_M_FinProduct_PART", VE);

        }
        //public ActionResult AddRowINVCD(ItemMasterEntry VE)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

        //    //List<MSITEMINVCD> ITEMINVCD = new List<MSITEMINVCD>();
        //    //for (int i = 0; i <= VE.MSITEMINVCD.Count - 1; i++)
        //    //{
        //    //    MSITEMINVCD MII = new MSITEMINVCD();
        //    //    MII = VE.MSITEMINVCD[i];
        //    //    ITEMINVCD.Add(MII);
        //    //}
        //    //MSITEMINVCD MII1 = new MSITEMINVCD();
        //    //var max = VE.MSITEMINVCD.Max(a => Convert.ToInt32(a.SRLNO));
        //    //int SRLNO = Convert.ToInt32(max) + 1;
        //    //MII1.SRLNO = Convert.ToByte(SRLNO);
        //    //ITEMINVCD.Add(MII1);
        //    //VE.MSITEMINVCD = ITEMINVCD;

        //    VE.DefaultView = true;
        //    return PartialView("_M_FinProduct_INVCD", VE);

        //}
        //public ActionResult DeleteRowINVCD(ItemMasterEntry VE)
        //{
        //    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
        //    List<MSITEMINVCD> ITEMINVCD = new List<MSITEMINVCD>();
        //    int count = 0;
        //    for (int i = 0; i <= VE.MSITEMINVCD.Count - 1; i++)
        //    {
        //        if (VE.MSITEMINVCD[i].Checked == false)
        //        {
        //            count += 1;
        //            MSITEMINVCD item = new MSITEMINVCD();
        //            item = VE.MSITEMINVCD[i];
        //            item.SRLNO = Convert.ToByte(count);
        //            ITEMINVCD.Add(item);
        //        }
        //    }
        //    VE.MSITEMINVCD = ITEMINVCD;
        //    ModelState.Clear();
        //    VE.DefaultView = true;
        //    return PartialView("_M_FinProduct_INVCD", VE);

        //}
        public ActionResult AddRowMEASURE(ItemMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            VE.Database_Combo2 = (from n in DB.M_SITEM_MEASURE
                                  select new Database_Combo2() { FIELD_VALUE = n.MDESC }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();

            if (VE.MSITEMMEASURE == null)
            {
                List<MSITEMMEASURE> ITEMMEASURE1 = new List<MSITEMMEASURE>();
                MSITEMMEASURE MIM = new MSITEMMEASURE();
                MIM.SLNO = 1;
                ITEMMEASURE1.Add(MIM);
                VE.MSITEMMEASURE = ITEMMEASURE1;
            }
            else
            {
                List<MSITEMMEASURE> ITEMMEASURE = new List<MSITEMMEASURE>();
                for (int i = 0; i <= VE.MSITEMMEASURE.Count - 1; i++)
                {
                    MSITEMMEASURE MIM = new MSITEMMEASURE();
                    MIM = VE.MSITEMMEASURE[i];
                    ITEMMEASURE.Add(MIM);
                }
                MSITEMMEASURE MIM1 = new MSITEMMEASURE();
                var max = VE.MSITEMMEASURE.Max(a => Convert.ToInt32(a.SLNO));
                int SLNO = Convert.ToInt32(max) + 1;
                MIM1.SLNO = Convert.ToByte(SLNO);
                ITEMMEASURE.Add(MIM1);
                VE.MSITEMMEASURE = ITEMMEASURE;

            }

            VE.DefaultView = true;
            return PartialView("_M_FinProduct_Measurement", VE);

        }
        public ActionResult DeleteRowMEASURE(ItemMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            VE.Database_Combo2 = (from n in DB.M_SITEM_MEASURE
                                  select new Database_Combo2() { FIELD_VALUE = n.MDESC }).OrderBy(s => s.FIELD_VALUE).Distinct().ToList();


            List<MSITEMMEASURE> ITEMMEASURE = new List<MSITEMMEASURE>();
            int count = 0;
            for (int i = 0; i <= VE.MSITEMMEASURE.Count - 1; i++)
            {
                if (VE.MSITEMMEASURE[i].Checked == false)
                {
                    count += 1;
                    MSITEMMEASURE MEASURE = new MSITEMMEASURE();
                    MEASURE = VE.MSITEMMEASURE[i];
                    MEASURE.SLNO = Convert.ToByte(count);
                    ITEMMEASURE.Add(MEASURE);
                }

            }
            VE.MSITEMMEASURE = ITEMMEASURE;
            ModelState.Clear();
            VE.DefaultView = true;
            return PartialView("_M_FinProduct_Measurement", VE);

        }
        public ActionResult CheckItemCode(string val)
        {
            string VALUE = val.ToUpper();
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            var query = (from c in DB.M_SITEM where (c.ITCD == VALUE) select c);
            if (query.Any())
            {
                return Content("1");
            }
            else
            {
                return Content("0");
            }

        }
        public ActionResult GetItemGroupDetails()
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            string GRPTYPE = "F";
            return PartialView("_Help2", Master_Help.GROUP(DB, GRPTYPE));
        }
        public ActionResult ItemGroupCode(string val)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            var query = (from c in DB.M_GROUP where c.ITGRPTYPE == "F" && c.ITGRPCD == val select new { ITGRPCD = c.ITGRPCD, ITGRPNM = c.ITGRPNM }).ToList();

            if (query.Any())
            {
                string str = "";
                foreach (var i in query)
                {
                    str = i.ITGRPCD + Cn.GCS() + i.ITGRPNM;// + Cn.GCS() + i.BRANDCD + Cn.GCS() + i.BRANDNM;
                }
                return Content(str);
            }
            else
            {
                return Content("0");
            }
        }
        public ActionResult GetSubBrandDetails(string val, string Code)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.SBRAND(val, Code));
                }
                else
                {
                    string str = Master_Help.SBRAND(val, Code);
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
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.COLOR(val));
                }
                else
                {
                    string str = Master_Help.COLOR(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
            }
        }
        public ActionResult GetPartDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.PARTS(val));
                }
                else
                {
                    string str = Master_Help.PARTS(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
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
        public ActionResult GetSizeDetails(string val)
        {
            try
            {
                if (val == null)
                {
                    return PartialView("_Help2", Master_Help.SIZE(val));
                }
                else
                {
                    string str = Master_Help.SIZE(val);
                    return Content(str);
                }
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message + ex.InnerException);
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
        public ActionResult SAVE(FormCollection FC, ItemMasterEntry VE)
        {
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
            using (var transaction = DB.Database.BeginTransaction())
            {
                try
                {
                    if (VE.DefaultAction == "A" || VE.DefaultAction == "E")
                    {
                        M_SITEM MSITEM = new M_SITEM();
                        MSITEM.CLCD = CommVar.ClientCode(UNQSNO);

                        bool dataexist = false;
                        if (VE.DefaultAction == "A")
                        {
                            var STYLENO = DB.M_SITEM.Where(a => a.STYLENO == VE.M_SITEM.STYLENO);
                            if (STYLENO.Any()) dataexist = true;
                        }
                        else
                        {
                            var STYLENO = (from x in DB.M_SITEM where x.STYLENO == VE.M_SITEM.STYLENO && x.ITCD != VE.M_SITEM.ITCD select x).ToList();
                            if (STYLENO.Any()) dataexist = true;
                        }
                        if (dataexist == true)
                        {
                            transaction.Rollback();
                            return Content("This Article Already exist");
                        }
                        if (VE.DefaultAction == "A")
                        {
                            MSITEM.EMD_NO = 0;
                            MSITEM.M_AUTONO = Cn.M_AUTONO(CommVar.CurSchema(UNQSNO).ToString());

                            //auto code with first leter

                            string txtITGRPCD = VE.M_SITEM.ITGRPCD;
                            var grptype = (from p in DB.M_GROUP where p.ITGRPCD == txtITGRPCD select new { ITGRPTYPE = p.ITGRPTYPE }).ToList();
                            string h = grptype[0].ITGRPTYPE;
                            string txtst = h.Substring(0, 1);

                            var MAXJOBCD = DB.M_SITEM.Where(a => a.ITCD.Substring(0, 1) == txtst).Max(a => a.ITCD);

                            if (MAXJOBCD == null)
                            {
                                string txt = txtst;
                                string stxt = txt.Substring(0, 1);
                                string R = stxt + "000001";
                                MSITEM.ITCD = R.ToString();
                            }
                            else
                            {
                                string maxSLst = MAXJOBCD.Substring(0, 1);
                                if (maxSLst == txtst)
                                {
                                    string s = MAXJOBCD;
                                    string digits = new string(s.Where(char.IsDigit).ToArray());
                                    string letters = new string(s.Where(char.IsLetter).ToArray());
                                    int number;
                                    if (!int.TryParse(digits, out number))                   //int.Parse would do the job since only digits are selected
                                    {
                                        Console.WriteLine("Something weired happened");
                                    }
                                    string newStr = letters + (++number).ToString("D6");
                                    MSITEM.ITCD = newStr.ToString();
                                }
                                else
                                {
                                    string txt = txtst;
                                    string stxt = txt.Substring(0, 1);
                                    string R = stxt + "000001";
                                    MSITEM.ITCD = R.ToString();
                                }
                            }
                        }
                        MSITEM.ITGRPCD = VE.M_SITEM.ITGRPCD;
                        MSITEM.ITNM = VE.M_SITEM.QUALITY;
                        MSITEM.SBRANDCD = VE.M_SITEM.SBRANDCD;
                        MSITEM.QUALITY = VE.M_SITEM.QUALITY;
                        MSITEM.STYLENO = VE.M_SITEM.STYLENO.retStr().Trim();
                        //MSITEM.MIXSIZE = VE.M_SITEM.MIXSIZE;
                        //string altrStyle = "";
                        //if (VE.M_SITEM.STYLENODISP == null)
                        //{
                        //    altrStyle = VE.M_SITEM.STYLENO;
                        //}
                        //else
                        //{
                        //    altrStyle = VE.M_SITEM.STYLENODISP;
                        //}
                        //MSITEM.STYLENODISP = VE.M_SITEM.STYLENODISP;
                        MSITEM.UOMCD = VE.M_SITEM.UOMCD;
                        MSITEM.HSNCODE = VE.M_SITEM.HSNCODE;
                        MSITEM.COLRCD = VE.M_SITEM.COLRCD;
                        MSITEM.PRODUCTTYPE = FC["protyp"].ToString();
                        MSITEM.GENDER = FC["gender"].ToString();
                        MSITEM.COLLCD = VE.M_SITEM.COLLCD;
                        MSITEM.PCSPERBOX = VE.M_SITEM.PCSPERBOX;
                        MSITEM.PCSPERSET = VE.M_SITEM.PCSPERSET;
                        MSITEM.COLRPERSET = VE.M_SITEM.COLRPERSET;
                        //MSITEM.STD_RATE = VE.M_SITEM.STD_RATE;
                        //MSITEM.SAMPPC = VE.M_SITEM.SAMPPC;
                        //MSITEM.STDLOTQTY = VE.M_SITEM.SAMPPC;
                        //MSITEM.STDLOTCOMPLTDAYS = VE.M_SITEM.STDLOTCOMPLTDAYS;
                        //MSITEM.STDBATCHQTY = VE.M_SITEM.STDBATCHQTY;
                        //MSITEM.MERGEPCS = VE.M_SITEM.MERGEPCS;
                        if (VE.DefaultAction == "E")
                        {
                            MSITEM.DTAG = "E";
                            MSITEM.M_AUTONO = VE.M_SITEM.M_AUTONO;

                            MSITEM.ITCD = VE.M_SITEM.ITCD;

                            DB.M_SITEM_SIZE.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.SaveChanges();
                            DB.M_SITEM_SIZE.RemoveRange(DB.M_SITEM_SIZE.Where(x => x.ITCD == VE.M_SITEM.ITCD));

                            //DB.M_SITEM_BOX.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "E"; });
                            //DB.M_SITEM_BOX.RemoveRange(DB.M_SITEM_BOX.Where(x => x.ITCD == VE.M_SITEM.ITCD));

                            DB.M_SITEM_COLOR.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_SITEM_COLOR.RemoveRange(DB.M_SITEM_COLOR.Where(x => x.ITCD == VE.M_SITEM.ITCD));

                            DB.M_SITEM_PARTS.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_SITEM_PARTS.RemoveRange(DB.M_SITEM_PARTS.Where(x => x.ITCD == VE.M_SITEM.ITCD));

                            //DB.M_SITEM_INVCD.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "E"; });
                            //DB.M_SITEM_INVCD.RemoveRange(DB.M_SITEM_INVCD.Where(x => x.ITCD == VE.M_SITEM.ITCD));

                            DB.M_SITEM_MEASURE.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_SITEM_MEASURE.RemoveRange(DB.M_SITEM_MEASURE.Where(x => x.ITCD == VE.M_SITEM.ITCD));

                            DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO));

                            DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "E"; });
                            DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO));

                            var MAXEMDNO = (from p in DB.M_SITEM where p.M_AUTONO == VE.M_SITEM.M_AUTONO select p.EMD_NO).Max();
                            if (MAXEMDNO == null)
                            {
                                MSITEM.EMD_NO = 0;
                            }
                            else
                            {
                                MSITEM.EMD_NO = Convert.ToByte(MAXEMDNO + 1);
                            }

                        }
                        for (int i = 0; i <= VE.MSITEMSIZE.Count - 1; i++)
                        {
                            if (VE.MSITEMSIZE[i].SRLNO != null && VE.MSITEMSIZE[i].SIZECD != null)
                            {
                                M_SITEM_SIZE MIS = new M_SITEM_SIZE();
                                MIS.CLCD = MSITEM.CLCD;
                                MIS.EMD_NO = MSITEM.EMD_NO;
                                MIS.ITCD = MSITEM.ITCD;
                                MIS.SIZECD = VE.MSITEMSIZE[i].SIZECD;
                                if (VE.MSITEMSIZE[i].IChecked == true)
                                {
                                    MIS.INACTIVE_TAG = "Y";
                                }
                                else
                                {
                                    MIS.INACTIVE_TAG = "N";
                                }
                                DB.M_SITEM_SIZE.Add(MIS);
                            }
                        }
                        //for (int i = 0; i <= VE.MSITEMBOX.Count - 1; i++)
                        //{
                        //    if (VE.MSITEMBOX[i].SRLNO != null && VE.MSITEMBOX[i].SIZECDBOX != null)
                        //    {
                        //        M_SITEM_BOX MIB = new M_SITEM_BOX();
                        //        MIB.CLCD = MSITEM.CLCD;
                        //        MIB.EMD_NO = MSITEM.EMD_NO;
                        //        MIB.ITCD = MSITEM.ITCD;
                        //        MIB.SIZECD = VE.MSITEMBOX[i].SIZECDBOX;
                        //        MIB.PCSQTY = VE.MSITEMBOX[i].PCSQTY;
                        //        MIB.SIZEGRP = VE.MSITEMBOX[i].SIZEGRP;
                        //        DB.M_SITEM_BOX.Add(MIB);
                        //    }
                        //}
                        for (int i = 0; i <= VE.MSITEMCOLOR.Count - 1; i++)
                        {
                            if (VE.MSITEMCOLOR[i].SLNO != null && VE.MSITEMCOLOR[i].COLRCD != null)
                            {
                                M_SITEM_COLOR MIC = new M_SITEM_COLOR();
                                MIC.CLCD = MSITEM.CLCD;
                                MIC.EMD_NO = MSITEM.EMD_NO;
                                MIC.ITCD = MSITEM.ITCD;
                                MIC.SLNO = VE.MSITEMCOLOR[i].SLNO;
                                MIC.COLRCD = VE.MSITEMCOLOR[i].COLRCD;
                                if (VE.MSITEMCOLOR[i].IChecked == true)
                                {
                                    MIC.INACTIVE_TAG = "Y";
                                }
                                else
                                {
                                    MIC.INACTIVE_TAG = "N";
                                }
                                DB.M_SITEM_COLOR.Add(MIC);
                            }
                        }
                        for (int i = 0; i <= VE.MSITEMPARTS.Count - 1; i++)
                        {
                            if (VE.MSITEMPARTS[i].SLNO != null && VE.MSITEMPARTS[i].PARTCD != null)
                            {
                                M_SITEM_PARTS MIP = new M_SITEM_PARTS();
                                MIP.CLCD = MSITEM.CLCD;
                                MIP.EMD_NO = MSITEM.EMD_NO;
                                MIP.ITCD = MSITEM.ITCD;
                                MIP.PARTCD = VE.MSITEMPARTS[i].PARTCD;
                                DB.M_SITEM_PARTS.Add(MIP);
                            }
                        }
                        //for (int i = 0; i <= VE.MSITEMINVCD.Count - 1; i++)
                        //{
                        //    if ((VE.MSITEMINVCD[i].SRLNO != 0 && VE.MSITEMINVCD[i].SIZECD != null) || (VE.MSITEMINVCD[i].SRLNO != 0 && VE.MSITEMINVCD[i].COLRCD != null))
                        //    {
                        //        M_SITEM_INVCD MII = new M_SITEM_INVCD();
                        //        MII.CLCD = MSITEM.CLCD;
                        //        MII.EMD_NO = MSITEM.EMD_NO;
                        //        MII.ITCD = MSITEM.ITCD;
                        //        MII.SRLNO = VE.MSITEMINVCD[i].SRLNO;
                        //        MII.SIZECD = VE.MSITEMINVCD[i].SIZECD;
                        //        MII.COLRCD = VE.MSITEMINVCD[i].COLRCD;
                        //        MII.BARCODE = VE.MSITEMINVCD[i].BARCODE;
                        //        DB.M_SITEM_INVCD.Add(MII);
                        //    }
                        //}


                        for (int i = 0; i <= VE.MSITEMMEASURE.Count - 1; i++)
                        {
                            if (VE.MSITEMMEASURE[i].SLNO != 0 && VE.MSITEMMEASURE[i].MDESC != null)
                            {
                                M_SITEM_MEASURE MIM = new M_SITEM_MEASURE();
                                MIM.CLCD = MSITEM.CLCD;
                                MIM.EMD_NO = MSITEM.EMD_NO;
                                MIM.ITCD = MSITEM.ITCD;
                                MIM.SLNO = VE.MSITEMMEASURE[i].SLNO;
                                MIM.MDESC = VE.MSITEMMEASURE[i].MDESC;
                                MIM.MVAL = VE.MSITEMMEASURE[i].MVAL;
                                MIM.REM = VE.MSITEMMEASURE[i].REM;

                                DB.M_SITEM_MEASURE.Add(MIM);
                            }
                        }

                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SITEM", MSITEM.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());

                        if (VE.DefaultAction == "A")
                        {
                            DB.M_SITEM.Add(MSITEM);
                            DB.M_CNTRL_HDR.Add(MCH);
                        }

                        else if (VE.DefaultAction == "E")
                        {
                            DB.Entry(MSITEM).State = System.Data.Entity.EntityState.Modified;
                            DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        }
                        if (VE.UploadDOC != null)
                        {
                            var img = Cn.SaveUploadImage("M_SITEM", VE.UploadDOC, MSITEM.M_AUTONO, MSITEM.EMD_NO.Value);
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
                        M_CNTRL_HDR MCH = Cn.M_CONTROL_HDR(VE.Checked, "M_SITEM", VE.M_SITEM.M_AUTONO, VE.DefaultAction, CommVar.CurSchema(UNQSNO).ToString());
                        DB.Entry(MCH).State = System.Data.Entity.EntityState.Modified;
                        DB.SaveChanges();

                        DB.M_SITEM.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_SITEM_SIZE.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        //DB.M_SITEM_BOX.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_SITEM_COLOR.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_SITEM_PARTS.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        //DB.M_SITEM_INVCD.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_SITEM_MEASURE.Where(x => x.ITCD == VE.M_SITEM.ITCD).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO).ToList().ForEach(x => { x.DTAG = "D"; });
                        DB.SaveChanges();

                        DB.M_CNTRL_HDR_DOC_DTL.RemoveRange(DB.M_CNTRL_HDR_DOC_DTL.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR_DOC.RemoveRange(DB.M_CNTRL_HDR_DOC.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_SITEM_SIZE.RemoveRange(DB.M_SITEM_SIZE.Where(x => x.ITCD == VE.M_SITEM.ITCD));
                        DB.SaveChanges();
                        //DB.M_SITEM_BOX.RemoveRange(DB.M_SITEM_BOX.Where(x => x.ITCD == VE.M_SITEM.ITCD));
                        DB.SaveChanges();
                        DB.M_SITEM_COLOR.RemoveRange(DB.M_SITEM_COLOR.Where(x => x.ITCD == VE.M_SITEM.ITCD));
                        DB.SaveChanges();
                        DB.M_SITEM_PARTS.RemoveRange(DB.M_SITEM_PARTS.Where(x => x.ITCD == VE.M_SITEM.ITCD));
                        DB.SaveChanges();
                        //DB.M_SITEM_INVCD.RemoveRange(DB.M_SITEM_INVCD.Where(x => x.ITCD == VE.M_SITEM.ITCD));
                        DB.SaveChanges();
                        DB.M_SITEM_MEASURE.RemoveRange(DB.M_SITEM_MEASURE.Where(x => x.ITCD == VE.M_SITEM.ITCD));
                        DB.SaveChanges();
                        DB.M_SITEM.RemoveRange(DB.M_SITEM.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO));
                        DB.SaveChanges();
                        DB.M_CNTRL_HDR.RemoveRange(DB.M_CNTRL_HDR.Where(x => x.M_AUTONO == VE.M_SITEM.M_AUTONO));
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
        [HttpPost]
        public ActionResult M_FINProduct(FormCollection FC)
        {
            try
            {
                string dbname = CommVar.CurSchema(UNQSNO).ToString();

                string query = "SELECT A.STYLENO, A.ITNM, A.ITCD, B.ITGRPNM, F.BRANDNM, A.PCSPERBOX, A.STYLENODISP, A.HSNSACCD, ";
                query = query + "LISTAGG(D.SIZENM,',') WITHIN GROUP (ORDER BY C.ITCD, D.PRINT_SEQ) sizes ";
                query = query + "FROM " + dbname + ".M_SITEM A, " + dbname + ".M_GROUP B, " + dbname + ".M_SITEM_SIZE C, " + dbname + ".M_SIZE D, ";
                query = query + dbname + ".M_CNTRL_HDR E, " + dbname + ".M_BRAND F ";
                query = query + "WHERE A.ITGRPCD = B.ITGRPCD(+) AND A.ITCD=C.ITCD(+) AND C.SIZECD=D.SIZECD(+) AND ";
                query = query + "A.M_AUTONO=E.M_AUTONO(+) AND NVL(E.INACTIVE_TAG,'N')='N' AND B.BRANDCD=F.BRANDCD(+) ";
                query = query + "GROUP BY A.STYLENO, A.ITNM, A.ITCD, B.ITGRPNM, F.BRANDNM, A.PCSPERBOX, A.STYLENODISP, A.HSNSACCD ";
                query = query + "ORDER BY B.ITGRPNM, A.STYLENO";

                DataTable IR = Master_Help.SQLquery(query);
                if (IR.Rows.Count == 0) return Content("No Records");

                //IR = new DataTable("mst_rep");
                Models.PrintViewer PV = new Models.PrintViewer();
                HtmlConverter HC = new HtmlConverter();
                HC.RepStart(IR, 3);
                HC.GetPrintHeader(IR, "itgrpnm", "string", "c,35", "Group Name");
                HC.GetPrintHeader(IR, "styleno", "string", "c,15", "Style No");
                HC.GetPrintHeader(IR, "itcd", "string", "c,10", "Item;Code");
                HC.GetPrintHeader(IR, "itnm", "string", "c,35", "Item Name");
                HC.GetPrintHeader(IR, "pcsperbox", "double", "n,5", "Pcs/;Box");
                HC.GetPrintHeader(IR, "sizes", "string", "c,25", "Sizes");
                HC.GetPrintHeader(IR, "brandnm", "string", "c,15", "Brand Name");
                HC.GetPrintHeader(IR, "hsnsaccd", "string", "c,8", "HSN;Code");
                string lastgrpnm = ""; string lastbrandnm = "";
                for (int i = 0; i <= IR.Rows.Count - 1; i++)
                {
                    string grpnm = IR.Rows[i]["itgrpnm"].ToString();
                    string brandnm = IR.Rows[i]["brandnm"].ToString();
                    DataRow dr = IR.NewRow();
                    dr["styleno"] = "<b>" + IR.Rows[i]["styleno"] + "</b>";
                    dr["itgrpnm"] = IR.Rows[i]["itgrpnm"];
                    dr["itcd"] = IR.Rows[i]["itcd"];
                    dr["itnm"] = IR.Rows[i]["itnm"];
                    dr["pcsperbox"] = Convert.ToDouble(IR.Rows[i]["pcsperbox"] == DBNull.Value ? 0 : IR.Rows[i]["pcsperbox"]);
                    dr["sizes"] = (IR.Rows[i]["sizes"]);
                    dr["hsnsaccd"] = (IR.Rows[i]["hsnsaccd"]);
                    dr["brandnm"] = IR.Rows[i]["brandnm"];
                    dr["Flag"] = " class='grid_td'";
                    IR.Rows.Add(dr);
                    lastgrpnm = grpnm;
                    lastbrandnm = brandnm;
                }
                string repname = CommFunc.retRepname("Item_master");
                PV = HC.ShowReport(IR, repname, "Item Master List");
                return RedirectToAction("ResponsivePrintViewer", "RPTViewer", new { ReportName = repname });
            }
            catch (Exception ex)
            {
                Cn.SaveException(ex, "");
                return Content(ex.Message);
            }
            return View();
        }
        public ActionResult PrintReport()
        {
            return RedirectToAction("PrintViewer", "RPTViewer");
        }
    }
}