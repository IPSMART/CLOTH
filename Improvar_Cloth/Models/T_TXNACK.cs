namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_TXNACK")]
    public partial class T_TXNACK
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

        [Key]
        [Column(Order = 1)]
        [StringLength(5)]
        public string FLAG1 { get; set; }

        public DateTime? DOCDT { get; set; }

        [StringLength(8)]
        public string TRSLCD { get; set; }

        [StringLength(20)]
        public string REFNO { get; set; }

        public DateTime? REFDT { get; set; }

        [StringLength(50)]
        public string PERSNAME { get; set; }

        [StringLength(1000)]
        public string REMARKS { get; set; }

        public decimal? WT { get; set; }

        public decimal? AMT { get; set; }

        [Required]
        [StringLength(40)]
        public string USR_ID { get; set; }

        public DateTime? USR_ENTDT { get; set; }

        [StringLength(15)]
        public string USR_LIP { get; set; }

        [StringLength(15)]
        public string USR_SIP { get; set; }

        [StringLength(50)]
        public string USR_OS { get; set; }

        [StringLength(50)]
        public string USR_MNM { get; set; }
    }
}
