﻿using System;
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

    [Column("date", TypeName = "datetime")]
    public DateTime? Date { get; set; }

    [Column("location")]
    [StringLength(50)]
    public string? Location { get; set; }

    [Column("description")]
    [StringLength(1000)]
    public string? Description { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("Events")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("EventId")]
    [InverseProperty("Events")]
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    [InverseProperty("EventNavigation")]
    public virtual EventBanner? Banner { get; set; }

    public Event()
    {
    }
}
