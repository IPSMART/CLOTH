namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("T_TDSTXN")]

    public partial class T_TDSTXN
    {
        public int? EMD_NO { get; set; }
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
        [StringLength(5)]
        public string DOCCD { get; set; }
        [StringLength(6)]
        public string DOCNO { get; set; }
        public DateTime? DOCDT { get; set; }
        [StringLength(20)]
        public string BLNO { get; set; }
        public DateTime? BLDT { get; set; }
        [StringLength(8)]
        public string GLCD { get; set; }
        [StringLength(8)]
        public string SLCD { get; set; }
        [StringLength(3)]
        public string TDSCODE { get; set; }
        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SLNO { get; set; }
        public double? TDSON { get; set; }
        public double? TDSPER { get; set; }
        public double? TDSAMT { get; set; }
        public DateTime? TDSDT { get; set; }
        [StringLength(1)]
        public string LOW_TDS { get; set; }
        public double? BLAMT { get; set; }
        [StringLength(8)]
        public string CLASS1CD { get; set; }
        [StringLength(3)]
        public string TDSTCS { get; set; }
       
        public double? ADVADJ { get; set; }
        [StringLength(8)]
        public string EXPGLCD { get; set; }
    }
}
