namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("M_FLRLOCA")]
    public partial class M_FLRLOCA
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public M_FLRLOCA()
        {
            M_LINEMAST = new HashSet<M_LINEMAST>();
        }

        public short? EMD_NO { get; set; }

        [StringLength(1)]
        public string TTAG { get; set; }

        [Required]
        [StringLength(4)]
        public string CLCD { get; set; }

        [StringLength(1)]
        public string DTAG { get; set; }

        [Key]
        [StringLength(3)]
        public string FLRCD { get; set; }

        [Required]
        [StringLength(15)]
        public string FLRNM { get; set; }

        [Required]
        [StringLength(4)]
        public string JOBPRCCD { get; set; }

        [Required]
        [StringLength(8)]
        public string SLCD { get; set; }

        public long M_AUTONO { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<M_LINEMAST> M_LINEMAST { get; set; }
    }
}
