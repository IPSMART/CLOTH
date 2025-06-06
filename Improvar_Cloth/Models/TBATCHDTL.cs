﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TBATCHDTL
    {
        public short? EMD_NO { get; set; }
        
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }
        
        [StringLength(30)]
        public string AUTONO { get; set; }

        public short TXNSLNO { get; set; }
        
        public short SLNO { get; set; }
        
        [StringLength(25)]
        public string BARNO { get; set; }

        [StringLength(8)]
        public string HSNCODE { get; set; }
        
        [StringLength(1)]
        public string STKDRCR { get; set; }
        public double? NOS { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? QNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? BLQNTY { get; set; }

        public double? FLAGMTR { get; set; }

        [StringLength(100)]
        public string ITREM { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? RATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? DISCRATE { get; set; }

        [StringLength(1)]
        public string DISCTYPE { get; set; }
        public string DISCTYPE_DESC { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? SCMDISCRATE { get; set; }

        [StringLength(1)]
        public string SCMDISCTYPE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? TDDISCRATE { get; set; }

        [StringLength(1)]
        public string TDDISCTYPE { get; set; }

        [StringLength(30)]
        public string ORDAUTONO { get; set; }

        public short? ORDSLNO { get; set; }

        public double? DIA { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? CUTLENGTH { get; set; }

        [StringLength(10)]
        public string LOCABIN { get; set; }

        [StringLength(15)]
        public string SHADE { get; set; }

        [StringLength(15)]
        public string MILLNM { get; set; }

        [StringLength(40)]
        public string BATCHNO { get; set; }

        [StringLength(30)]
        public string RECPROGAUTONO { get; set; }

        [StringLength(15)]
        public string RECPROGLOTNO { get; set; }

        public short? RECPROGSLNO { get; set; }

        [StringLength(30)]
        public string ISSPROGAUTONO { get; set; }

        public short? ISSPROGSLNO { get; set; }

        [StringLength(15)]
        public string ISSPROGLOTNO { get; set; }
        public bool Checked { get; set; }
        public string ITGRPCD { get; set; }
        public string ITGRPNM { get; set; }
        [StringLength(10)]
        public string ITCD { get; set; }
        public string ITNM { get; set; }
        public string ITSTYLE { get; set; }
        [StringLength(2)]
        public string MTRLJOBCD { get; set; }
        public string MTRLJOBNM { get; set; }
        public string FABITCD { get; set; }
        public string FABITNM { get; set; }
        public string STYLENO { get; set; }
        [StringLength(4)]
        public string PARTCD { get; set; }
        public string PARTNM { get; set; }
        [StringLength(4)]
        public string COLRCD { get; set; }
        public string COLRNM { get; set; }
        [StringLength(4)]
        public string SIZECD { get; set; }
        public string SIZENM { get; set; }
        public string UOM { get; set; }
        [StringLength(1)]
        public string STKTYPE { get; set; }
        public List<DropDown_list1> DropDown_list1 { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? GSTPER { get; set; }
        public List<DISC_TYPE> DISC_TYPE { get; set; }
        public List<TDDISC_TYPE> TDDISC_TYPE { get; set; }
        public List<SCMDISC_TYPE> SCMDISC_TYPE { get; set; }
        [StringLength(15)]
        public string STKNAME { get; set; }
        [StringLength(30)]
        public string PDESIGN { get; set; }
        public string ALL_GSTPER { get; set; }
        public string PRODGRPGSTPER { get; set; }
        [StringLength(30)]
        public string BALENO { get; set; }
        public string SCMDISCTYPE_DESC { get; set; }
        public string TDDISCTYPE_DESC { get; set; }
        public string MTBARCODE { get; set; }
        public string PRTBARCODE { get; set; }
        public string CLRBARCODE { get; set; }        
        public string SZBARCODE { get; set; }
        public string BARGENTYPE { get; set; }
        [StringLength(4)]
        public string BALEYR { get; set; }
        [StringLength(30)]
        public string OURDESIGN { get; set; }
        [StringLength(8)]
        public string GLCD { get; set; }
        public string BarImages { get; set; }
        public string BarImagesCount { get; set; }
        public string ChildData { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? WPRATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? RPRATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? MRPRATE { get; set; }
        public string ORDDOCNO { get; set; }
        public string ORDDOCDT { get; set; }
        public string NEGSTOCK { get; set; }
        public double? BALSTOCK { get; set; }
        public string WPPRICEGEN { get; set; }
        public string RPPRICEGEN { get; set; }
        [StringLength(1)]
        public string SAMPLE { get; set; }
        public string BOMQNTY { get; set; }
        public string AGDOCNO { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? AGDOCDT { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? LISTPRICE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? LISTDISCPER { get; set; }
        public int? PAGENO { get; set; }
        public int? PAGESLNO { get; set; }
        [StringLength(15)]
        public string PCSTYPE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double? OTHRAMT { get; set; }
        public double? TXBLVAL { get; set; }
        [StringLength(3)]
        public string BLUOMCD { get; set; }
        public string COMMONUNIQBAR { get; set; }
        public double? WPPER { get; set; }
        public double? RPPER { get; set; }
        public double? MTRLCOST { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? SHORTQNTY { get; set; }
        public string NOOFROWCOPY { get; set; }
        public string RECPROGITCD { get; set; }
        public string RECPROGITSTYLE { get; set; }
        public double? CONVQTYPUNIT { get; set; }
        [StringLength(1)]
        public string FREESTK { get; set; }
        public string ADJAUTONO { get; set; }
        public int ParentSerialNo { get; set; }
        public string RECPROGUOMNM { get; set; }
        public double RECPROGQNTY { get; set; }
    }
}