using System;
using GestionVentas.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionVentas.Context
{
    public class AppDbcontext :DbContext
    {
        public AppDbcontext(DbContextOptions<AppDbcontext> options) : base(options)
        {
        }
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Producto> Producto { get; set; }
        public DbSet<Factura> Factura { get; set; }
        public DbSet<HistorialFactura> HistorialFacturas { get; set; }
        public DbSet<LoginIntento> LoginIntentos { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbcontext).Assembly);
            base.OnModelCreating(modelBuilder);
            // Configure the Usuarios entity
            // Define the primary key for the Usuarios entity
            modelBuilder.Entity<Usuarios>()
                .HasKey(u => u.UsuarioID);

            //define la llave primaria de historial factura
            modelBuilder.Entity<HistorialFactura>()
                .HasKey(hf => hf.HistoriaFacturasID);
        }
    }

}

