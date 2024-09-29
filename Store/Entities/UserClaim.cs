namespace Store.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class UserClaim
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }

        public string ClaimType { get; set; }

        public string ClaimValue { get; set; }

        public virtual User User { get; set; }
    }
}
