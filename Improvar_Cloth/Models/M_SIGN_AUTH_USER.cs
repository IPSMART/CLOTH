namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_SIGN_AUTH_USER")]
    public partial class M_SIGN_AUTH_USER
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
        [StringLength(5)]
        public string AUTHCD { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(40)]
        public string USRID { get; set; }

        [StringLength(20)]
        public string SIGN_IMG { get; set; }

        [StringLength(10)]
        public string EMPID { get; set; }

        [StringLength(5)]
        public string DESIGCD { get; set; }        
    }
}
