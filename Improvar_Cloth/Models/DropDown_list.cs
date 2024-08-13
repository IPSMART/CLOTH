using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Improvar.Models
{

    public class DropDown_list
    {
        public string text { get; set; }
        public string value { get; set; }
    }

    public class DropDown_list1
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DropDown_list2
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DropDown_list3
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DropDown_list4
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DropDown_list5
    {
        public string text { get; set; }
        public string value { get; set; }
    }

    public class LOW_TDS_list
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }

    public class DropDown_list_text
    {
        public string text1 { get; set; }
        public string value { get; set; }
        public string text2 { get; set; }
        public double text3 { get; set; }
        public long text4 { get; set; }
    }
    public class Database_Combo1
    {
        public string FIELD_VALUE { get; set; }
    }
    public class Database_Combo2
    {
        public string FIELD_VALUE { get; set; }
    }
    public class Database_Combo3
    {
        public string FIELD_VALUE { get; set; }
    }

    public class User
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
    public class GROUPMAIN
    {
        public string GRPMAIN { get; set; }
    }
    public class RateTag
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
    public class STD_DIS_TYPE
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
    public class DISC_TYPE
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
    public class PCSection
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
    public class DropDown_list_BLTYPE
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }

    public class PAY_TAG
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class DropDown_list_GLCD
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DropDown_list_SLCD
    {
        public string text { get; set; }
        public string value { get; set; }
        public string slarea { get; set; }
        public string City { get; set; }
    }
    public class DropDown_list_AGSLCD
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DropDown_list_SLMSLCD
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DropDown_list_EMPCD
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DropDown_list_Class1
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class SCHEME_DTL
    {
        public string SCMCODE { get; set; }
        public string SCMNAME { get; set; }
        public string SCMDATE { get; set; }
        public string SCMTYPE { get; set; }
        public bool Checked { get; set; }
    }
    public class PENDING_DO
    {
        public short SLNO { get; set; }
        public string ORDAUTONO { get; set; }
        public string DOAUTONO { get; set; }
        public string DONO { get; set; }
        public string DODT { get; set; }
        public string ORDNO { get; set; }
        public string ORDDT { get; set; }
        public string PRCCD { get; set; }
        public bool Checked { get; set; }
    }
    public class PACKING_SLIP
    {
        public short SLNO { get; set; }
        public string FREESTK { get; set; }
        public string STKTYPE { get; set; }
        public string ORDAUTONO { get; set; }
        public string DOAUTONO { get; set; }
        public string PACKAUTONO { get; set; }
        public string DOCNO { get; set; }
        public string DOCDT { get; set; }
        public double PACK_QNTY { get; set; }
        public double RATE { get; set; }
        public bool Checked { get; set; }
    }
    public class STOCK_ADJUSTMENT
    {
        public short SLNO { get; set; }
        public string STKTYPE { get; set; }
        public string STKTYPE_VALUE { get; set; }
        public double IN_QNTY { get; set; }
        public double OUT_QNTY { get; set; }
        public List<DropDown_list2> DropDown_list2 { get; set; }

    }
    public class CashOnDelivery
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DropDown_list_TDS
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DropDown_list_StkType
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class FABRIC_TYPE
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class QUANTITY_IN
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class DropDown_list_DelvType
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class MACHINE_NAME
    {
        public string FIELD_VALUE { get; set; }
    }
    public class DropDown_list_LINECD
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DropDown_list_JOBCD
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DropDown_Grid_SLCD
    {
        public string text { get; set; }
        public string district { get; set; }
        public string value { get; set; }
    }
    public class DropDown_list_ITEM
    {
        public string text { get; set; }
        public string value { get; set; }
        
    }
    public class DropDown_list_ITGRP
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DropDown_list_BRAND
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DropDown_list_GODOWN
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DropDown_list_TXN
    {
        public string text { get; set; }
        public string value { get; set; }
        public string docdt { get; set; }
        public string slcd { get; set; }
        public string slnm { get; set; }
        public string slarea { get; set; }
    }
    public class DropDown_list_DOCCD
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DocumentThrough
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class DropDown_list_SubLegGrp
    {
        public string text { get; set; }
        public string value { get; set; }
        public string grpnm { get; set; }
    }
    public class DropDown_list_COMP
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class INVTYPE_list
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
    public class EXPCD_list
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
    public class BARGEN_TYPE
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
    public class DropDown_list_MTRLJOBCD
    {
        public bool Checked { get; set; }
        public string MTRLJOBCD { get; set; }
        public string MTRLJOBNM { get; set; }
    }
    public class TDDISC_TYPE
    {
        public string FIELD_VALUE { get; set; }
    }
    public class SCMDISC_TYPE
    {
        public string FIELD_VALUE { get; set; }
    }
    public class HSN_CODE
    {
        public string text { get; set; }
        public string value { get; set; }
    }
    public class REV_CHRG
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
    public class VECHLTYPE
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }
    public class TRANSMODE
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }
    public class Database_Combo4
    {
        public string FIELD_VALUE { get; set; }
    }
    public class Database_Combo5
    {
        public string FIELD_VALUE { get; set; }
    }
    public class DropDown_list_RTCD
    {
        public string text { get; set; }
        public string value { get; set; }
        public string add { get; set; }
        public string mobile { get; set; }
    }
}