using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Improvar.Models
{
    public class TPROGBOM
    {
        public short? EMD_NO { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Key]
        [Column(Order = 0)]
        [StringLength(30)]
        public string AUTONO { get; set; }

        public short TXNSLNO { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short SLNO { get; set; }

        [Required]
        [StringLength(25)]
        public string BARNO { get; set; }

        [StringLength(8)]
        public string HSNCODE { get; set; }

        [Required]
        [StringLength(1)]
        public string STKDRCR { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? NOS { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? QNTY { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? QQNTY { get; set; }

        public double? FLAGMTR { get; set; }

        [StringLength(100)]
        public string ITREM { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? RATE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? DISCRATE { get; set; }

        [StringLength(1)]
        public string DISCTYPE { get; set; }
        public string DISCTYPE_DESC { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? SCMDISCRATE { get; set; }

        [StringLength(1)]
        public string SCMDISCTYPE { get; set; }
        [DisplayFormat(DataFormatString = "{0:0.000}", ApplyFormatInEditMode = true)]
        public double? TDDISCRATE { get; set; }

        [StringLength(1)]
        public string TDDISCTYPE { get; set; }

        [StringLength(30)]
        public string ORDAUTONO { get; set; }

        public short? ORDSLNO { get; set; }

        public double? DIA { get; set; }

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
        [StringLength(8)]
        public string ITCD { get; set; }
        public string ITNM { get; set; }
        [StringLength(2)]
        public string MTRLJOBCD { get; set; }
        public string MTRLJOBNM { get; set; }
        public string FABITCD { get; set; }
        public string QITNM { get; set; }
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
        public string QUOM { get; set; }
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
        public string ITREMARKS { get; set; }
        public bool CheckedSample { get; set; }
        public string BOMQNTY { get; set; }
        public string EXTRAQNTY { get; set; }
        public short RSLNO { get; set; }
      


    }
}