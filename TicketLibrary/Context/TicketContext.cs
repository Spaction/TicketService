using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using TicketLibrary.Models;

namespace TicketLibrary.Context
{
    public class TicketContext : DbContext
    {
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Show> Shows { get; set; }
        public DbSet<Location> Locations { get; set; }

        public TicketContext(DbContextOptions<TicketContext> options):base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Seat>()
                .HasOne(p => p.Show)
                .WithMany(b => b.Seats)
                .HasForeignKey(x => x.ShowId);

            modelBuilder.Entity<Seat>()
                .HasOne(l => l.Loc)
                .WithMany()
                .HasForeignKey(x=>x.LocationId);

            modelBuilder.Entity<Show>()
                .HasMany(s => s.Seats)
                .WithOne(r => r.Show);
        }

    }
}
