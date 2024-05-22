using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProdigyPlanningAPI.Models
{
    [Table("user_questions")]
    public partial class UserQuestion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("question_id")]
        public int QuestionId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("answer")]
        public string Answer { get; set; }

        [ForeignKey("QuestionId")]
        public virtual SecurityQuestion Question { get; set; }

        [ForeignKey("UserId")]
        [InverseProperty("UserQuestion")]
        public virtual User User { get; set; }

    }
}