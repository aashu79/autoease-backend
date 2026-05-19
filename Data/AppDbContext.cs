using Microsoft.EntityFrameworkCore;
using autoease_backend.Models;

namespace autoease_backend.Data
{
   
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<Appointment> Appointments { get; set; } = null!;
        public DbSet<VehicleUsageLog> VehicleUsageLogs { get; set; } = null!;
        public DbSet<PartRequest> PartRequests { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Vendor> Vendors { get; set; } = null!;
        public DbSet<Part> Parts { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<InvoiceItem> InvoiceItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Customer)
                .WithMany(u => u.CustomerAppointments)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Staff)
                .WithMany(u => u.StaffAppointments)
                .HasForeignKey(a => a.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Customer)
                .WithMany(u => u.CustomerInvoices)
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Staff)
                .WithMany(u => u.StaffInvoices)
                .HasForeignKey(i => i.StaffId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VehicleUsageLog>()
                .HasOne(v => v.Customer)
                .WithMany(u => u.VehicleUsageLogs)
                .HasForeignKey(v => v.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvoiceItem>()
                .HasOne(i => i.Invoice)
                .WithMany(iv => iv.InvoiceItems)
                .HasForeignKey(i => i.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InvoiceItem>()
                .HasOne(i => i.Part)
                .WithMany(p => p.InvoiceItems)
                .HasForeignKey(i => i.PartId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
