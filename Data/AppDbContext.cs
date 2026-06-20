using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> o) : base(o) { }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Plan> Plans { get; set; } = null!;
    public DbSet<UserPlan> UserPlans { get; set; } = null!;
    public DbSet<TrainerBooking> TrainerBookings { get; set; } = null!;
    public DbSet<Attendance> Attendances { get; set; } = null!;
    public DbSet<ProgressRecord> ProgressRecords { get; set; } = null!;
    public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
}
