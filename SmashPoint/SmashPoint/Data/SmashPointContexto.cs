
using Microsoft.EntityFrameworkCore;
using SmashPoint.Models;

namespace SmashPoint.Data
{
    public class SmashPointContexto: DbContext
    {
        public SmashPointContexto(DbContextOptions<SmashPointContexto> options)
        : base(options)
        {
        }
        public DbSet<Categoria>? Categorias { get; set; }
        public DbSet<Cliente>? Clientes { get; set; }
        public DbSet<Detalle>? Detalles { get; set; }
        public DbSet<Estado>? Estados { get; set; }
        public DbSet<EstadoDetalle>? EstadoDetalles { get; set; }
        public DbSet<Pedido>? Pedidos { get; set; }
        public DbSet<Producto>? Productos { get; set; }
        public DbSet<Proveedor>? Proveedores { get; set; }
        public DbSet<Descuento>? Descuentos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Poner el nombre de las tablas en singular
            modelBuilder.Entity<Categoria>().ToTable("Categoria");
            modelBuilder.Entity<Cliente>().ToTable("Cliente");
            modelBuilder.Entity<Detalle>().ToTable("Detalle");
            modelBuilder.Entity<Estado>().ToTable("Estado");
            modelBuilder.Entity<EstadoDetalle>().ToTable("EstadoDetalle");
            modelBuilder.Entity<Pedido>().ToTable("Pedido");
            modelBuilder.Entity<Producto>().ToTable("Producto");
            modelBuilder.Entity<Proveedor>().ToTable("Proveedor");
            modelBuilder.Entity<Descuento>().ToTable("Descuento");
            // Deshabilitar la eliminación en cascada en todas las relaciones
            base.OnModelCreating(modelBuilder);
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
