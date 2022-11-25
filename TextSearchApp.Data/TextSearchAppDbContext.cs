using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TextSearchApp.Data.Entities;

namespace TextSearchApp.Data
{
    public class TextSearchAppDbContext : DbContext
    {
        public TextSearchAppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<DocumentText> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DocumentText>()
                .Property(e => e.Rubrics)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries),
                    new ValueComparer<string[]>(
                        (c1, c2) => c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToArray()));
                ;
        }
    }
}