namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_ITEM")]
    public partial class M_ITEM
    {
        public int? EMD_NO { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [StringLength(3)]
        public string BHCD { get; set; }

        [StringLength(3)]
        public string SHCD { get; set; }

        [Key]
        [StringLength(12)]
        public string ITCD { get; set; }

        [StringLength(50)]
        public string ITDESCN { get; set; }

        [StringLength(30)]
        public string SHTDESCN { get; set; }

        [StringLength(10)]
        public string DEPTCD { get; set; }

        [StringLength(3)]
        public string UOM { get; set; }

        [StringLength(10)]
        public string EQUIPCD { get; set; }

        [StringLength(5)]
        public string ITMGRPCD { get; set; }

        public double? STD_QTY { get; set; }

        [StringLength(10)]
        public string HSNCD { get; set; }

        [StringLength(8)]
        public string ACCD { get; set; }

        [StringLength(8)]
        public string LIABCD { get; set; }

        [StringLength(10)]
        public string CLASS1CD { get; set; }

        [StringLength(10)]
        public string CLASS2CD { get; set; }

        [StringLength(1)]
        public string NO_STK { get; set; }

        [StringLength(1)]
        public string BATCHOP { get; set; }

        [StringLength(10)]
        public string PROP1 { get; set; }

        [StringLength(10)]
        public string PROP2 { get; set; }

        [StringLength(10)]
        public string PROP3 { get; set; }

        [StringLength(10)]
        public string PROP4 { get; set; }

        [StringLength(10)]
        public string PROP5 { get; set; }

        [StringLength(10)]
        public string PROP6 { get; set; }

        [StringLength(4)]
        public string ITEMCATECD { get; set; }

        [StringLength(20)]
        public string BARCODE { get; set; }

        public long M_AUTONO { get; set; }
    }
}
