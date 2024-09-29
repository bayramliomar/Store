namespace Store.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class UserRole
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Key]
        [Column(Order = 0)]
        public Guid UserId { get; set; }

        [Key]
        [Column(Order = 1)]
        public Guid RoleId { get; set; }

        public virtual Role Role { get; set; }

        public virtual User User { get; set; }
    }
}
