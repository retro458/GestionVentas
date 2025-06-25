using System;
using GestionVentas.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionVentas.Context
{
    public class Appcontext :DbContext
    {
        public Appcontext(DbContextOptions<Appcontext> options) : base(options)
        {
        }
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(Appcontext).Assembly);
            base.OnModelCreating(modelBuilder);
            // Configure the Usuarios entity
            // Define the primary key for the Usuarios entity
            modelBuilder.Entity<Usuarios>()
                .HasKey(u => u.UsuarioID);

        }
    }
    
    }

