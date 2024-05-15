using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProdigyPlanningAPI.Models
{
    [Table("event_banners")]
    public class EventBanner
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("event_id")]
        public int? EventId { get; set; }

        [ForeignKey("Id")]
        [InverseProperty("Banner")]
        public virtual Event? EventNavigation { get; set; }

        [Column("event_image", TypeName ="image")]
        public byte[]? EventImage { get; set; }
    }
}
