using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ProdigyPlanningAPI.Data;
using ProdigyPlanningAPI.Helpers;

namespace ProdigyPlanningAPI.Models;

[Table("events")]
public partial class Event
{
    public readonly ProdigyPlanningContext _context;

    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [StringLength(50)]
    public string? Name { get; set; }

    [Column("date", TypeName = "date")]
    public DateOnly Date { get; set; }

    [Column("time", TypeName = "time")]
    public TimeOnly Time { get; set; }

    [Column("duration")]
    public int? Duration { get; set; }

    [Column("location")]
    [StringLength(50)]
    public string? Location { get; set; }

    [Column("description")]
    [StringLength(1000)]
    public string? Description { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set;} = DateTime.Now;

    [Column("is_featured")]
    public bool IsFeatured { get; set; } = false;

    [Column("is_deleted")]
    public bool IsDeleted { get; set; } = false;

    [ForeignKey("CreatedBy")]
    [InverseProperty("Events")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("EventId")]
    [InverseProperty("Events")]
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    [InverseProperty("EventNavigation")]
    public virtual EventBanner? Banner { get; set; }

    [Column("is_active")]
    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
    public bool IsActive { get; private set; }

    public Event()
    {
    }
}
