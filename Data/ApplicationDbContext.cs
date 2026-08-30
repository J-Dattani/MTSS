using Microsoft.EntityFrameworkCore;
using MTSS.Models;

namespace MTSS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Society> Societies { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<SocietySubscription> SocietySubscriptions { get; set; }
        public DbSet<Wing> Wings { get; set; }
        public DbSet<Floor> Floors { get; set; }
        public DbSet<Flat> Flats { get; set; }
        public DbSet<ResidentProfile> ResidentProfiles { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<GuestEntryRequest> GuestEntryRequests { get; set; }
        public DbSet<Parcel> Parcels { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AmenityAccessPin> AmenityAccessPins { get; set; }
        public DbSet<AmenityAccessLog> AmenityAccessLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Society>()
                .HasKey(s => s.SocietyId);

            modelBuilder.Entity<SubscriptionPlan>()
                .HasKey(p => p.SubscriptionPlanId);

            modelBuilder.Entity<SocietySubscription>()
                .HasKey(s => s.SocietySubscriptionId);

            modelBuilder.Entity<Wing>()
                .HasKey(w => w.WingId);

            modelBuilder.Entity<Floor>()
                .HasKey(f => f.FloorId);

            modelBuilder.Entity<Flat>()
                .HasKey(f => f.FlatId);

            modelBuilder.Entity<ResidentProfile>()
                .HasKey(r => r.ResidentProfileId);

            modelBuilder.Entity<Amenity>()
                .HasKey(a => a.AmenityId);

            modelBuilder.Entity<GuestEntryRequest>()
                .HasKey(g => g.GuestEntryRequestId);

            modelBuilder.Entity<Parcel>()
                .HasKey(p => p.ParcelId);

            modelBuilder.Entity<Complaint>()
                .HasKey(c => c.ComplaintId);

            modelBuilder.Entity<Notification>()
                .HasKey(n => n.NotificationId);

            modelBuilder.Entity<AmenityAccessPin>()
                .HasKey(p => p.AmenityAccessPinId);

            modelBuilder.Entity<AmenityAccessLog>()
                .HasKey(l => l.AmenityAccessLogId);

            // Society → Wing
            modelBuilder.Entity<Wing>()
                .HasOne<Society>()
                .WithMany()
                .HasForeignKey(w => w.SocietyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Wing → Floor
            modelBuilder.Entity<Floor>()
                .HasOne<Wing>()
                .WithMany()
                .HasForeignKey(f => f.WingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Floor → Flat
            modelBuilder.Entity<Flat>()
                .HasOne<Floor>()
                .WithMany()
                .HasForeignKey(f => f.FloorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Society → Flat
            modelBuilder.Entity<Flat>()
                .HasOne<Society>()
                .WithMany()
                .HasForeignKey(f => f.SocietyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Society → Floor
            modelBuilder.Entity<Floor>()
                .HasOne<Society>()
                .WithMany()
                .HasForeignKey(f => f.SocietyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Society → Subscription
            modelBuilder.Entity<SocietySubscription>()
                .HasOne<Society>()
                .WithMany()
                .HasForeignKey(s => s.SocietyId)
                .OnDelete(DeleteBehavior.Restrict);

            // SubscriptionPlan → SocietySubscription
            modelBuilder.Entity<SocietySubscription>()
                .HasOne<SubscriptionPlan>()
                .WithMany()
                .HasForeignKey(s => s.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // Society → Resident
            modelBuilder.Entity<ResidentProfile>()
                .HasOne<Society>()
                .WithMany()
                .HasForeignKey(r => r.SocietyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Flat → Resident
            modelBuilder.Entity<ResidentProfile>()
                .HasOne<Flat>()
                .WithMany()
                .HasForeignKey(r => r.FlatId)
                .OnDelete(DeleteBehavior.Restrict);

            // Society → Amenity
            modelBuilder.Entity<Amenity>()
                .HasOne<Society>()
                .WithMany()
                .HasForeignKey(a => a.SocietyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Society → GuestEntryRequest
            modelBuilder.Entity<GuestEntryRequest>()
                .HasOne<Society>()
                .WithMany()
                .HasForeignKey(g => g.SocietyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Flat → GuestEntryRequest
            modelBuilder.Entity<GuestEntryRequest>()
                .HasOne<Flat>()
                .WithMany()
                .HasForeignKey(g => g.FlatId)
                .OnDelete(DeleteBehavior.Restrict);

            // Resident → GuestEntryRequest
            modelBuilder.Entity<GuestEntryRequest>()
                .HasOne<ResidentProfile>()
                .WithMany()
                .HasForeignKey(g => g.ResidentProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Society → Parcel
            modelBuilder.Entity<Parcel>()
                .HasOne<Society>()
                .WithMany()
                .HasForeignKey(p => p.SocietyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Flat → Parcel
            modelBuilder.Entity<Parcel>()
                .HasOne<Flat>()
                .WithMany()
                .HasForeignKey(p => p.FlatId)
                .OnDelete(DeleteBehavior.Restrict);

            // Resident → Parcel
            modelBuilder.Entity<Parcel>()
                .HasOne<ResidentProfile>()
                .WithMany()
                .HasForeignKey(p => p.ResidentProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Society → Complaint
            modelBuilder.Entity<Complaint>()
                .HasOne<Society>()
                .WithMany()
                .HasForeignKey(c => c.SocietyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Flat → Complaint
            modelBuilder.Entity<Complaint>()
                .HasOne<Flat>()
                .WithMany()
                .HasForeignKey(c => c.FlatId)
                .OnDelete(DeleteBehavior.Restrict);

            // Resident → Complaint
            modelBuilder.Entity<Complaint>()
                .HasOne<ResidentProfile>()
                .WithMany()
                .HasForeignKey(c => c.ResidentProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Society → Notification
            modelBuilder.Entity<Notification>()
                .HasOne<Society>()
                .WithMany()
                .HasForeignKey(n => n.SocietyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Society → AmenityAccessPin
            modelBuilder.Entity<AmenityAccessPin>()
                .HasOne<Society>()
                .WithMany()
                .HasForeignKey(p => p.SocietyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Amenity → AmenityAccessPin
            modelBuilder.Entity<AmenityAccessPin>()
                .HasOne<Amenity>()
                .WithMany()
                .HasForeignKey(p => p.AmenityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Flat → AmenityAccessPin
            modelBuilder.Entity<AmenityAccessPin>()
                .HasOne<Flat>()
                .WithMany()
                .HasForeignKey(p => p.FlatId)
                .OnDelete(DeleteBehavior.Restrict);

            // Resident → AmenityAccessPin
            modelBuilder.Entity<AmenityAccessPin>()
                .HasOne<ResidentProfile>()
                .WithMany()
                .HasForeignKey(p => p.ResidentProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Society → AmenityAccessLog
            modelBuilder.Entity<AmenityAccessLog>()
                .HasOne<Society>()
                .WithMany()
                .HasForeignKey(l => l.SocietyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Amenity → AmenityAccessLog
            modelBuilder.Entity<AmenityAccessLog>()
                .HasOne<Amenity>()
                .WithMany()
                .HasForeignKey(l => l.AmenityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Flat → AmenityAccessLog
            modelBuilder.Entity<AmenityAccessLog>()
                .HasOne<Flat>()
                .WithMany()
                .HasForeignKey(l => l.FlatId)
                .OnDelete(DeleteBehavior.Restrict);

            // Resident → AmenityAccessLog
            modelBuilder.Entity<AmenityAccessLog>()
                .HasOne<ResidentProfile>()
                .WithMany()
                .HasForeignKey(l => l.ResidentProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}