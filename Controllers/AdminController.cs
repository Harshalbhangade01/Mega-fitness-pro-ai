using Microsoft.AspNetCore.Mvc;

public class AdminController : Controller
{
    private readonly AppDbContext _c;
    public AdminController(AppDbContext c) { _c = c; }

    private bool IsAdmin => HttpContext.Session.GetString("role") == "Admin";

    public IActionResult Dashboard()
    {
        if (!IsAdmin) return RedirectToAction("Login", "Auth");
        ViewBag.TotalUsers = _c.Users.Count(u => u.Role == "User");
        ViewBag.TotalTrainers = _c.Users.Count(u => u.Role == "Trainer");
        ViewBag.TotalRevenue = _c.Payments.Sum(p => p.Amount);
        ViewBag.TotalBookings = _c.TrainerBookings.Count();
        ViewBag.TodayAttendance = _c.Attendances.Count(a => a.CheckIn.Date == DateTime.Today);
        ViewBag.Users = _c.Users.Where(u => u.Role == "User").ToList();
        ViewBag.Trainers = _c.Users.Where(u => u.Role == "Trainer").ToList();
        ViewBag.Plans = _c.Plans.ToList();
        ViewBag.UserPlans = _c.UserPlans.ToList();
        ViewBag.Bookings = _c.TrainerBookings.ToList();
        ViewBag.Payments = _c.Payments.OrderByDescending(p => p.PaymentDate).Take(10).ToList();
        ViewBag.Attendances = _c.Attendances.OrderByDescending(a => a.CheckIn).Take(20).ToList();
        return View();
    }

    // Trainer Add
    [HttpPost]
    public IActionResult AddTrainer(string name, string email, string password, string phone,
        string specialization, string bio, decimal hourlyRate)
    {
        if (!IsAdmin) return RedirectToAction("Login", "Auth");
        if (_c.Users.Any(u => u.Email == email))
        {
            TempData["Error"] = "Email already registered!";
            return RedirectToAction("Dashboard");
        }
        _c.Users.Add(new User
        {
            Name = name, Email = email, Password = password,
            Phone = phone, Role = "Trainer", JoinDate = DateTime.Now,
            Specialization = specialization, Bio = bio,
            HourlyRate = hourlyRate, Rating = 5
        });
        _c.SaveChanges();
        TempData["Success"] = $"Trainer {name} added successfully!";
        return RedirectToAction("Dashboard");
    }

    // Trainer Delete
    public IActionResult DeleteTrainer(int id)
    {
        if (!IsAdmin) return RedirectToAction("Login", "Auth");
        var trainer = _c.Users.Find(id);
        if (trainer != null && trainer.Role == "Trainer")
        {
            _c.Users.Remove(trainer);
            _c.SaveChanges();
            TempData["Success"] = "Trainer removed successfully!";
        }
        return RedirectToAction("Dashboard");
    }

    // Admin Chat with Users
    public IActionResult Chat(int? userId)
    {
        if (!IsAdmin) return RedirectToAction("Login", "Auth");
        int uid = HttpContext.Session.GetInt32("uid")!.Value;
        var users = _c.Users.Where(u => u.Role == "User").ToList();

        int selectedUserId = userId ?? 0;
        List<ChatMessage> messages = new();
        if (selectedUserId > 0)
        {
            messages = _c.ChatMessages
                .Where(m => (m.SenderId == uid && m.ReceiverId == selectedUserId) ||
                            (m.SenderId == selectedUserId && m.ReceiverId == uid))
                .OrderBy(m => m.SentAt).ToList();

            var unread = messages.Where(m => m.ReceiverId == uid && !m.IsRead).ToList();
            unread.ForEach(m => m.IsRead = true);
            _c.SaveChanges();
        }

        ViewBag.Users = users;
        ViewBag.Messages = messages;
        ViewBag.SelectedUserId = selectedUserId;
        return View();
    }

    [HttpPost]
    public IActionResult SendMessage(int receiverId, string message)
    {
        if (!IsAdmin) return RedirectToAction("Login", "Auth");
        int uid = HttpContext.Session.GetInt32("uid")!.Value;
        var sender = _c.Users.Find(uid);
        _c.ChatMessages.Add(new ChatMessage
        {
            SenderId = uid, ReceiverId = receiverId, Message = message,
            SentAt = DateTime.Now, SenderName = sender?.Name ?? "Admin", SenderRole = "Admin"
        });
        _c.SaveChanges();
        return RedirectToAction("Chat", new { userId = receiverId });
    }
}
