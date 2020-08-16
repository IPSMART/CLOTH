using System;
using System.Linq;
using System.Web.Mvc;
using Improvar.Models;
using Improvar.ViewModels;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace Improvar.Controllers
{
    public class Prepare_JSONController : Controller
    {
        string CS = null;
        Connection Cn = new Connection();
        MasterHelp masterHelp = new MasterHelp();
        string UNQSNO = CommVar.getQueryStringUNQSNO();
        // GET: Prepare_JSON
        public ActionResult Prepare_JSON()
        {
            try {
                if (Session["UR_ID"] == null)
                {
                    return RedirectToAction("Login", "Login");
                }
                else
                {
                    ViewBag.formname = " PREPARE JSON";
                    ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());
                    ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
                    ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
                    Prepare_JSONvalidation VE = new Prepare_JSONvalidation();
                    VE = (Prepare_JSONvalidation)TempData["PrepareJSON"];
                    TempData.Keep();
                    VE.EWB_SUPPLY_TYPE_Ddown = EWB_SUPPLY_TYPE_Ddown();
                    VE.EWB_SUB_TYPE_Ddown = EWB_SUB_TYPE_Ddown();
                    VE.EWB_DOC_TYPE_Ddown = EWB_DOC_TYPE_Ddown();
                    VE.EWB_Transmode_Ddown = EWB_Transmode_Ddown();
                    VE.EWB_Unit_Ddown = EWB_Unit_Ddown();
                    VE.EWB_BlFrmSt_TYPE_Ddown = EWB_BlFrmSt_TYPE_Ddown();
                    VE.EWB_TRANSACTION_TYPE_Ddown = EWB_TRANSACTION_TYPE_Ddown();
                    VE.EWB_BlFrmSt_TYPE_Ddown = EWB_BlFrmSt_TYPE_Ddown();
                    VE.EWB_DisFrmSt_TYPE_Ddown = EWB_DisFrmSt_TYPE_Ddown();
                    VE.EWB_BlTo_TYPE_Ddown = EWB_BlTo_TYPE_Ddown();
                    VE.EWB_ShipTo_TYPE_Ddown = EWB_ShipTo_TYPE_Ddown();
                    VE.EWB_Vehicle_TYPE_Ddown = EWB_Vehicle_TYPE_Ddown();


                    VE.DefaultView = true;
                    return View(VE);
                } }
            catch (Exception ex)
            {
                Prepare_JSONvalidation VE = new Prepare_JSONvalidation();
                VE.DefaultView = false;
                VE.DefaultDay = 0;
                ViewBag.ErrorMessage = ex.Message + " " + ex.InnerException;
                Cn.SaveException(ex, "");
                return View(VE);
            }
        }
        public List<EWB_SUPPLY_TYPE_Ddown> EWB_SUPPLY_TYPE_Ddown()
        {

            List<EWB_SUPPLY_TYPE_Ddown> EWB_SUPPLY_TYPE = new List<EWB_SUPPLY_TYPE_Ddown>();
            EWB_SUPPLY_TYPE_Ddown EWBsup1 = new EWB_SUPPLY_TYPE_Ddown();
            EWBsup1.value = "I";
            EWBsup1.text = "Inward";
            EWB_SUPPLY_TYPE.Add(EWBsup1);
            EWB_SUPPLY_TYPE_Ddown EWBsup2 = new EWB_SUPPLY_TYPE_Ddown();
            EWBsup2.value = "O";
            EWBsup2.text = "Outward";
            EWB_SUPPLY_TYPE.Add(EWBsup2);
            return EWB_SUPPLY_TYPE;

        }
        public List<EWB_SUB_TYPE_Ddown> EWB_SUB_TYPE_Ddown()
        {

            List<EWB_SUB_TYPE_Ddown> EWB_SUB_TYPE = new List<EWB_SUB_TYPE_Ddown>();
            EWB_SUB_TYPE_Ddown EWBsub1 = new EWB_SUB_TYPE_Ddown();
            EWBsub1.value = "1";
            EWBsub1.text = "Supply";
            EWB_SUB_TYPE.Add(EWBsub1);
            EWB_SUB_TYPE_Ddown EWBsub2 = new EWB_SUB_TYPE_Ddown();
            EWBsub2.value = "2";
            EWBsub2.text = "Import";
            EWB_SUB_TYPE.Add(EWBsub2);
            EWB_SUB_TYPE_Ddown EWBsub3 = new EWB_SUB_TYPE_Ddown();
            EWBsub3.value = "3";
            EWBsub3.text = "Export";
            EWB_SUB_TYPE.Add(EWBsub3);
            EWB_SUB_TYPE_Ddown EWBsub4 = new EWB_SUB_TYPE_Ddown();
            EWBsub4.value = "4";
            EWBsub4.text = "Job Work";
            EWB_SUB_TYPE.Add(EWBsub4);
            EWB_SUB_TYPE_Ddown EWBsub5 = new EWB_SUB_TYPE_Ddown();
            EWBsub5.value = "5";
            EWBsub5.text = "For Own Use";
            EWB_SUB_TYPE.Add(EWBsub5);
            EWB_SUB_TYPE_Ddown EWBsub6 = new EWB_SUB_TYPE_Ddown();
            EWBsub6.value = "6";
            EWBsub6.text = "Job Works Returns";
            EWB_SUB_TYPE.Add(EWBsub6);
            EWB_SUB_TYPE_Ddown EWBsub7 = new EWB_SUB_TYPE_Ddown();
            EWBsub7.value = "7";
            EWBsub7.text = "Sales Return";
            EWB_SUB_TYPE.Add(EWBsub7);
            EWB_SUB_TYPE_Ddown EWBsub8 = new EWB_SUB_TYPE_Ddown();
            EWBsub8.value = "8";
            EWBsub8.text = "Others";
            EWB_SUB_TYPE.Add(EWBsub8);
            EWB_SUB_TYPE_Ddown EWBsub9 = new EWB_SUB_TYPE_Ddown();
            EWBsub9.value = "9";
            EWBsub9.text = "SKD/CKD/LOTS";
            EWB_SUB_TYPE.Add(EWBsub9);
            EWB_SUB_TYPE_Ddown EWBsub10 = new EWB_SUB_TYPE_Ddown();
            EWBsub10.value = "10";
            EWBsub10.text = "Line Sales";
            EWB_SUB_TYPE.Add(EWBsub10);
            EWB_SUB_TYPE_Ddown EWBsub11 = new EWB_SUB_TYPE_Ddown();
            EWBsub11.value = "11";
            EWBsub11.text = "Recipient Not known";
            EWB_SUB_TYPE.Add(EWBsub11);
            EWB_SUB_TYPE_Ddown EWBsub12 = new EWB_SUB_TYPE_Ddown();
            EWBsub12.value = "12";
            EWBsub12.text = "Exhibition or Fairs";
            EWB_SUB_TYPE.Add(EWBsub12);
            return EWB_SUB_TYPE;

        }
        public List<EWB_DOC_TYPE_Ddown> EWB_DOC_TYPE_Ddown()
        {

            List<EWB_DOC_TYPE_Ddown> EWB_DOC_TYPE = new List<EWB_DOC_TYPE_Ddown>();
            EWB_DOC_TYPE_Ddown EWBdoctyp1 = new EWB_DOC_TYPE_Ddown();
            EWBdoctyp1.value = "INV";
            EWBdoctyp1.text = "Tax Invoice";
            EWB_DOC_TYPE.Add(EWBdoctyp1);
            EWB_DOC_TYPE_Ddown EWBdoctyp2 = new EWB_DOC_TYPE_Ddown();
            EWBdoctyp2.value = "BIL";
            EWBdoctyp2.text = "Outward";
            EWB_DOC_TYPE.Add(EWBdoctyp2);
            EWB_DOC_TYPE_Ddown EWBdoctyp3 = new EWB_DOC_TYPE_Ddown();
            EWBdoctyp3.value = "BOE";
            EWBdoctyp3.text = "Bill of Entry";
            EWB_DOC_TYPE.Add(EWBdoctyp3);
            EWB_DOC_TYPE_Ddown EWBdoctyp4 = new EWB_DOC_TYPE_Ddown();
            EWBdoctyp4.value = "CHL";
            EWBdoctyp4.text = "Delivery Challan";
            EWB_DOC_TYPE.Add(EWBdoctyp4);
            EWB_DOC_TYPE_Ddown EWBdoctyp5 = new EWB_DOC_TYPE_Ddown();
            EWBdoctyp5.value = "OTH";
            EWBdoctyp5.text = "Others";
            EWB_DOC_TYPE.Add(EWBdoctyp5);
            return EWB_DOC_TYPE;
        }
        public List<EWB_Transmode_Ddown> EWB_Transmode_Ddown()
        {
            List<EWB_Transmode_Ddown> EWB_Transmode = new List<EWB_Transmode_Ddown>();
            EWB_Transmode_Ddown EWBtransmode1 = new EWB_Transmode_Ddown();
            EWBtransmode1.value = "1";
            EWBtransmode1.text = "Road";
            EWB_Transmode.Add(EWBtransmode1);
            EWB_Transmode_Ddown EWBtransmode2 = new EWB_Transmode_Ddown();
            EWBtransmode2.value = "2";
            EWBtransmode2.text = "Rail";
            EWB_Transmode.Add(EWBtransmode2);
            EWB_Transmode_Ddown EWBtransmode3 = new EWB_Transmode_Ddown();
            EWBtransmode3.value = "3";
            EWBtransmode3.text = "Air";
            EWB_Transmode.Add(EWBtransmode3);
            EWB_Transmode_Ddown EWBtransmode4 = new EWB_Transmode_Ddown();
            EWBtransmode4.value = "4";
            EWBtransmode4.text = "Ship";
            EWB_Transmode.Add(EWBtransmode4);
            return EWB_Transmode;
        }

        public List<EWB_Unit_Ddown> EWB_Unit_Ddown()
        {
            ImprovarDB DBF = new ImprovarDB(Cn.GetConnectionString(), CommVar.FinSchema(UNQSNO));
            var UOM = (from j in DBF.M_UOM
                       join o in DBF.M_CNTRL_HDR on j.M_AUTONO equals (o.M_AUTONO)
                       where (o.M_AUTONO == j.M_AUTONO)
                       select new EWB_Unit_Ddown
                       {
                           value = j.UOMCD,
                           text = j.UOMNM
                       }).OrderBy(s => s.text).ToList();
            return UOM;
        }

        public List<EWB_BlFrmSt_TYPE_Ddown> EWB_BlFrmSt_TYPE_Ddown()
        {
            ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);
            ImprovarDB DB = new ImprovarDB(Cn.GetConnectionString(), CommVar.CurSchema(UNQSNO).ToString());

            var State = (from j in DBI.MS_STATE
                         select new EWB_BlFrmSt_TYPE_Ddown
                         {
                             value = j.STATECD,
                             text = j.STATENM
                         }).OrderBy(s => s.text).ToList();
            return State;
        }


        public List<EWB_DisFrmSt_TYPE_Ddown> EWB_DisFrmSt_TYPE_Ddown()
        {
            ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);

            var disState = (from j in DBI.MS_STATE
                            select new EWB_DisFrmSt_TYPE_Ddown
                            {
                                value = j.STATECD,
                                text = j.STATENM
                            }).OrderBy(s => s.text).ToList();
            return disState;
        }
        public List<EWB_BlTo_TYPE_Ddown> EWB_BlTo_TYPE_Ddown()
        {
            ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);

            var BltoState = (from j in DBI.MS_STATE
                             select new EWB_BlTo_TYPE_Ddown
                             {
                                 value = j.STATECD,
                                 text = j.STATENM
                             }).OrderBy(s => s.text).ToList();
            return BltoState;
        }

        public List<EWB_ShipTo_TYPE_Ddown> EWB_ShipTo_TYPE_Ddown()
        {
            ImprovarDB DBI = new ImprovarDB(Cn.GetConnectionString(), Cn.Getschema);

            var ShiptoState = (from j in DBI.MS_STATE
                               select new EWB_ShipTo_TYPE_Ddown
                               {
                                   value = j.STATECD,
                                   text = j.STATENM
                               }).OrderBy(s => s.text).ToList();
            return ShiptoState;
        }

        public List<EWB_Vehicle_TYPE_Ddown> EWB_Vehicle_TYPE_Ddown()
        {

            List<EWB_Vehicle_TYPE_Ddown> EWB_Vehicle_TYPE = new List<EWB_Vehicle_TYPE_Ddown>();
            EWB_Vehicle_TYPE_Ddown EWBveh1 = new EWB_Vehicle_TYPE_Ddown();
            EWBveh1.value = "R";
            EWBveh1.text = "Regular";
            EWB_Vehicle_TYPE.Add(EWBveh1);
            EWB_Vehicle_TYPE_Ddown EWBveh2 = new EWB_Vehicle_TYPE_Ddown();
            EWBveh2.value = "O";
            EWBveh2.text = "ODC";
            EWB_Vehicle_TYPE.Add(EWBveh2);
            return EWB_Vehicle_TYPE;

        }

        public List<EWB_TRANSACTION_TYPE_Ddown> EWB_TRANSACTION_TYPE_Ddown()
        {
            List<EWB_TRANSACTION_TYPE_Ddown> EWBtransaction = new List<EWB_TRANSACTION_TYPE_Ddown>();
            EWB_TRANSACTION_TYPE_Ddown EWBtransaction1 = new EWB_TRANSACTION_TYPE_Ddown();
            EWBtransaction1.value = "1";
            EWBtransaction1.text = "Regular";
            EWBtransaction.Add(EWBtransaction1);
            EWB_TRANSACTION_TYPE_Ddown EWBtransaction2 = new EWB_TRANSACTION_TYPE_Ddown();
            EWBtransaction2.value = "2";
            EWBtransaction2.text = "Bill To-Ship To";
            EWBtransaction.Add(EWBtransaction2);
            EWB_TRANSACTION_TYPE_Ddown EWBtransaction3 = new EWB_TRANSACTION_TYPE_Ddown();
            EWBtransaction3.value = "3";
            EWBtransaction3.text = "Bill From-Dispatch";
            EWBtransaction.Add(EWBtransaction3);
            EWB_TRANSACTION_TYPE_Ddown EWBtransaction4 = new EWB_TRANSACTION_TYPE_Ddown();
            EWBtransaction4.value = "4";
            EWBtransaction4.text = "Combination of 2 and 3";
            EWBtransaction.Add(EWBtransaction4);
            return EWBtransaction;
        }

        [HttpPost]
        public ActionResult Prepare_JSON(FormCollection FC, Prepare_JSONvalidation VE)
        {
            EWayBill_JSON EWB = new EWayBill_JSON();
            EWB.version = "1.0.1118";
            var no_bill = (from I in VE.Prepare_JSON
                           select new
                           {
                               blno = I.blno,
                               hsncode = I.hsncode
                           }).ToArray();
            List<billLists> billLists = new List<billLists>();
            List<itemList> itemList = null;
            itemList = new List<itemList>();        
           int ITMCOUNT = 0;
            double totalTaxableAmt = 0;
            double totaligstAmt = 0;
            double totalcgstAmt = 0;
            double totalsgstAmt = 0;
            double totalcessAmt = 0;
            if (VE.Prepare_JSON != null)
            {
                for (int i = 0; i < VE.Prepare_JSON.Count; i++)
                {
                    billLists billList = new billLists();
                    billList.userGstin = VE.Prepare_JSON[i].frmgstno;
                    billList.supplyType = VE.Prepare_JSON[i].Supply_Type;
                    billList.subSupplyType = Convert.ToInt32(VE.Prepare_JSON[i].SubSupply_Type);
                    billList.docType = VE.Prepare_JSON[i].Doctype;
                    billList.docNo = VE.Prepare_JSON[i].blno;
                    billList.docDate = VE.Prepare_JSON[i].bldt.ToShortDateString();
                    billList.transType = VE.Prepare_JSON[i].Transaction_Type;
                    billList.fromGstin = VE.Prepare_JSON[i].frmgstno;
                    billList.fromTrdName = VE.Prepare_JSON[i].compnm;
                    billList.fromAddr1 = VE.Prepare_JSON[i].frmadd1;
                    billList.fromAddr2 = VE.Prepare_JSON[i].frmadd2;
                    billList.fromPlace = VE.Prepare_JSON[i].frmdistrict;
                    billList.fromPincode = Convert.ToInt32(VE.Prepare_JSON[i].frmpin);
                    billList.fromStateCode = Convert.ToInt32(VE.Prepare_JSON[i].frmstatecd);
                    billList.actualFromStateCode = Convert.ToInt32(VE.Prepare_JSON[i].frmstatecd);
                    billList.toGstin = VE.Prepare_JSON[i].togstno;
                    billList.toTrdName = VE.Prepare_JSON[i].slnm;
                    billList.toAddr1 = VE.Prepare_JSON[i].toadd1;
                    billList.toAddr2 = VE.Prepare_JSON[i].toadd2;
                    billList.toPlace = VE.Prepare_JSON[i].todistrict;
                    billList.toPincode = Convert.ToInt32(VE.Prepare_JSON[i].shiptopin);//
                    billList.toStateCode = Convert.ToInt32(VE.Prepare_JSON[i].billtostcd);
                    billList.actualToStateCode = Convert.ToInt32(VE.Prepare_JSON[i].shiptostcd);
                    totalTaxableAmt += VE.Prepare_JSON[i].amt;
                    billList.totalValue = Math.Round(totalTaxableAmt,2);
                    totalcgstAmt += VE.Prepare_JSON[i].cgstamt;
                    billList.cgstValue = Math.Round(totalcgstAmt,2);
                    totalsgstAmt += VE.Prepare_JSON[i].sgstamt;
                    billList.sgstValue = Math.Round(totalsgstAmt,2);
                    totaligstAmt += VE.Prepare_JSON[i].igstamt;
                    billList.igstValue = Math.Round(totaligstAmt,2);
                    totalcessAmt += VE.Prepare_JSON[i].cessamt;
                    billList.cessValue = Math.Round(totalcessAmt,2);
                    billList.TotNonAdvolVal = VE.Prepare_JSON[i].cess_non_advol;
                    billList.OthValue = VE.Prepare_JSON[i].othramt;
                    billList.totInvValue = VE.Prepare_JSON[i].invamt;
                    billList.transMode = Convert.ToInt32(VE.Prepare_JSON[i].transMode);
                    billList.transDistance = VE.Prepare_JSON[i].distance;
                    billList.transporterName = VE.Prepare_JSON[i].trslnm == null ? "" : VE.Prepare_JSON[i].trslnm;
                    billList.transporterId = VE.Prepare_JSON[i].trgst == null?"": VE.Prepare_JSON[i].trgst;
                    billList.transDocNo = VE.Prepare_JSON[i].lrno == null ? "" : VE.Prepare_JSON[i].lrno;
                    billList.transDocDate = VE.Prepare_JSON[i].lrdt;
                    billList.vehicleNo = VE.Prepare_JSON[i].Vehicle_No;
                    billList.vehicleType = VE.Prepare_JSON[i].Vehile_Type;
                    billList.mainHsnCode = Convert.ToInt32(VE.Prepare_JSON[i].hsncode);
                    itemList itemlst = new ViewModels.itemList();
                    itemlst.itemNo = ++ITMCOUNT; //ITMCOUNT++;
                    itemlst.productName = VE.Prepare_JSON[i].itnm;
                    itemlst.productDesc = VE.Prepare_JSON[i].itdscp;
                    itemlst.hsnCode = Convert.ToInt32(VE.Prepare_JSON[i].hsncode);
                    itemlst.quantity = VE.Prepare_JSON[i].qnty;
                    itemlst.qtyUnit = VE.Prepare_JSON[i].guomcd;
                    itemlst.taxableAmount = VE.Prepare_JSON[i].amt;
                    itemlst.sgstRate = VE.Prepare_JSON[i].sgstper;
                    itemlst.cgstRate = VE.Prepare_JSON[i].cgstper;
                    itemlst.igstRate = VE.Prepare_JSON[i].igstper;
                    itemlst.cessRate = VE.Prepare_JSON[i].cessper;
                    itemlst.cessNonAdvol = 0;
                    if (i + 1 < VE.Prepare_JSON.Count && VE.Prepare_JSON[i].blno == no_bill[i + 1].blno)
                    {
                        itemList.Add(itemlst);
                    }
                    else
                    {
                        itemList.Add(itemlst);
                        billList.itemList = itemList;
                        billLists.Add(billList);
                        ITMCOUNT = 0; totalTaxableAmt = 0; totalcgstAmt = 0; totalsgstAmt = 0; totaligstAmt = 0; totalcessAmt = 0;
                        itemList = new List<itemList>();
                    }
                }
            }
            EWB.billLists = billLists;
            string json = JsonConvert.SerializeObject(EWB);
            byte[] newBytes = Encoding.ASCII.GetBytes(json);
            Response.Clear();
            Response.ClearContent();
            Response.Buffer = true;
            //Response.Headers.Add("Content-type", "text/json");//This operation requires IIS integrated pipeline mode.
            Response.Headers.Add("Content-type", "application/json");
            Response.AddHeader("Content-Disposition", "attachment; filename=E-WayBill_JSON.json");
            Response.BinaryWrite(newBytes);
            // msg += " DONE All ";
            Response.Flush();
            Response.Close();
            Response.End();
            // return Content("Excel exported sucessfully");
            return null;
        }



    }
}