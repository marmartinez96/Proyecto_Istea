using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProdigyPlanningAPI.Models
{
    [Table("security_questions")]
    public partial class SecurityQuestion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("question")]
        public string Question { get; set; }
    }
}
