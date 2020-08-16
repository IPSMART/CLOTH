namespace Improvar.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("MS_PORTCD")]
    public partial class MS_PORTCD
    {
        [Key]
        [StringLength(6)]
        public string PORTCD { get; set; }

        [Required]
        [StringLength(60)]
        public string PORTNM { get; set; }

        [Required]
        [StringLength(3)]
        public string CNCD { get; set; }

        [StringLength(2)]
        public string STATECD { get; set; }

        [StringLength(50)]
        public string STATENM { get; set; }

        [Required]
        [StringLength(40)]
        public string USR_ID { get; set; }

        public DateTime? USR_ENTDT { get; set; }

        [Required]
        [StringLength(15)]
        public string USR_LIP { get; set; }

        [Required]
        [StringLength(15)]
        public string USR_SIP { get; set; }

        public virtual MS_COUNTRY MS_COUNTRY { get; set; }

        public virtual MS_STATE MS_STATE { get; set; }
    }
}
