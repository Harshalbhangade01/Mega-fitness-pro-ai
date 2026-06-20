using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5050");

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("FitnessDB"));
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Plans.Any())
    {
        db.Plans.AddRange(
            new Plan { Id=1, PlanName="Basic Plan", Price=999, Description="Perfect for beginners", DurationDays=30, Features="Gym Access|Locker Room|Basic Equipment" },
            new Plan { Id=2, PlanName="Standard Plan", Price=1999, Description="Most popular choice", DurationDays=30, Features="All Basic|Group Classes|Swimming Pool|Diet Consultation" },
            new Plan { Id=3, PlanName="Premium Plan", Price=3499, Description="The ultimate fitness experience", DurationDays=30, Features="All Standard|Personal Trainer|Sauna|Nutrition Plan|24/7 Access" }
        );
    }
    if (!db.Users.Any())
    {
        db.Users.AddRange(
            // Admin
            new User { Id=1, Name="Admin", Email="admin@fitness.com", Password="admin123", Role="Admin", Phone="9999999999" },
            // Trainers (ab User table mein)
            new User { Id=2, Name="Rahul Sharma", Email="rahul@fitness.com", Password="trainer123", Role="Trainer", Phone="9111111111",
                Specialization="Weight Loss", Bio="10+ years experience in body transformation", HourlyRate=500, Rating=5 },
            new User { Id=3, Name="Priya Verma", Email="priya@fitness.com", Password="trainer123", Role="Trainer", Phone="9222222222",
                Specialization="Yoga & Flexibility", Bio="Certified yoga instructor with 8 years experience", HourlyRate=400, Rating=5 },
            new User { Id=4, Name="Arjun Singh", Email="arjun@fitness.com", Password="trainer123", Role="Trainer", Phone="9333333333",
                Specialization="Muscle Building", Bio="Former national level athlete & bodybuilder", HourlyRate=600, Rating=4 }
        );
        db.SaveChanges();
    }
    else
    {
        db.SaveChanges();
    }
}

app.UseStaticFiles();
app.UseSession();
app.MapDefaultControllerRoute();
app.Run();
