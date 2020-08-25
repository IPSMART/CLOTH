namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_RETDEB")]
    public partial class M_RETDEB
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
        [StringLength(8)]
        public string RTDEBCD { get; set; }

        [StringLength(50)]
        public string RTDEBNM { get; set; }

        [StringLength(1)]
        public string SEX { get; set; }

        [StringLength(12)]
        public string MOBILE { get; set; }

        [StringLength(12)]
        public string ALTNO { get; set; }

        [StringLength(100)]
        public string EMAIL { get; set; }

        [StringLength(30)]
        public string CITY { get; set; }

        [StringLength(7)]
        public string PIN { get; set; }

        [StringLength(2)]
        public string STATECD { get; set; }

        [StringLength(30)]
        public string STATE { get; set; }

        [StringLength(30)]
        public string COUNTRY { get; set; }

        [StringLength(75)]
        public string ADD1 { get; set; }

        [StringLength(75)]
        public string ADD2 { get; set; }

        [StringLength(75)]
        public string ADD3 { get; set; }

        [StringLength(30)]
        public string AREA { get; set; }

        [StringLength(8)]
        public string REFRTDEBCD { get; set; }

        [StringLength(8)]
        public string REFSLCD { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DOB { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? DOW { get; set; }

        public long? M_AUTONO { get; set; }
       
        

    }
}
