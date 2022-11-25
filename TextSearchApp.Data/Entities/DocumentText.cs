using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TextSearchApp.Data.Entities;

public class DocumentText
{
    [Column("id")] [Key] public long Id { get; set; }

    [Column("rubrics")] public string[] Rubrics { get; set; }

    [Column("text")] public string Text { get; set; }

    [Column("created_date")] public DateTime? CreatedDate { get; set; }
}