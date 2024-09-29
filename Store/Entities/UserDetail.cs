namespace Store.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserDetail")]
    public partial class UserDetail
    {
        public int ID { get; set; }

        public Guid UserId { get; set; }

        public long TelegramId { get; set; }

        [StringLength(50)]
        public string TelegramUserName { get; set; }

        [StringLength(50)]
        public string LoginToken { get; set; }
        public DateTime? TokenExpiration { get; set; }
        public bool? IsDelete { get; set; }

        public virtual User User { get; set; }
    }
}
