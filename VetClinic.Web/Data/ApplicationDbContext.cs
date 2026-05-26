using Microsoft.EntityFrameworkCore;
using VetClinic.Web.Models.Entities;

namespace VetClinic.Web.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Owner.Phone için DB seviyesinde unique index — race condition'a karşı güvenlik ağı.
        modelBuilder.Entity<Owner>()
            .HasIndex(o => o.Phone)
            .IsUnique();

        // Owner -> Pet (1-N): sahip silinince hayvanları da silinir.
        modelBuilder.Entity<Pet>()
            .HasOne(p => p.Owner)
            .WithMany(o => o.Pets)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Pet -> Appointment (1-N): hayvan silinince randevuları da silinir.
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Pet)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Service -> Appointment (1-N): geçmiş randevusu olan hizmet silinemez (soft delete tercih edilir).
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Service)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
