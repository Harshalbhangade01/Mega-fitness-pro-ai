using Microsoft.AspNetCore.Mvc;

public class UserController : Controller
{
    private readonly AppDbContext _c;
    public UserController(AppDbContext c) { _c = c; }

    private int? Uid => HttpContext.Session.GetInt32("uid");

    public IActionResult Dashboard()
    {
        if (Uid == null) return RedirectToAction("Login", "Auth");
        int uid = Uid!.Value;
        ViewBag.UserName = HttpContext.Session.GetString("uname");
        ViewBag.Plans = _c.Plans.ToList();
        ViewBag.Trainers = _c.Users.Where(u => u.Role == "Trainer").ToList();
        ViewBag.MyPlans = _c.UserPlans.Where(x => x.UserId == uid).ToList();
        ViewBag.MyBookings = _c.TrainerBookings.Where(x => x.UserId == uid).ToList();
        ViewBag.TodayAttendance = _c.Attendances.FirstOrDefault(a => a.UserId == uid && a.CheckIn.Date == DateTime.Today);
        ViewBag.AttendanceCount = _c.Attendances.Count(a => a.UserId == uid);
        ViewBag.LatestProgress = _c.ProgressRecords.Where(p => p.UserId == uid).OrderByDescending(p => p.RecordDate).FirstOrDefault();
        ViewBag.UnreadMessages = _c.ChatMessages.Count(m => m.ReceiverId == uid && !m.IsRead);
        return View();
    }

    // Payment
    public IActionResult Payment(int planId)
    {
        if (Uid == null) return RedirectToAction("Login", "Auth");
        ViewBag.Plan = _c.Plans.Find(planId);
        return View();
    }

    [HttpPost]
    public IActionResult ProcessPayment(int planId, string paymentMethod, string cardNumber, string upiId)
    {
        if (Uid == null) return RedirectToAction("Login", "Auth");
        int uid = Uid!.Value;
        var plan = _c.Plans.Find(planId);
        if (plan == null) return RedirectToAction("Dashboard");

        string txnId = "TXN" + DateTime.Now.Ticks.ToString().Substring(10);
        string last4 = paymentMethod == "Card" && !string.IsNullOrEmpty(cardNumber) && cardNumber.Length >= 4
            ? cardNumber.Substring(cardNumber.Length - 4) : "0000";

        _c.Payments.Add(new Payment
        {
            UserId = uid, PlanId = planId, Amount = plan.Price,
            PaymentMethod = paymentMethod, TransactionId = txnId,
            PaymentDate = DateTime.Now, Status = "Success", CardLast4 = last4
        });
        _c.UserPlans.Add(new UserPlan
        {
            UserId = uid, PlanId = planId, PurchaseDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddDays(plan.DurationDays),
            PaymentStatus = "Paid", PaymentMethod = paymentMethod, TransactionId = txnId
        });
        _c.SaveChanges();
        TempData["Success"] = $"Payment successful! Transaction ID: {txnId}";
        return RedirectToAction("Dashboard");
    }

    // Attendance
    public IActionResult Attendance()
    {
        if (Uid == null) return RedirectToAction("Login", "Auth");
        int uid = Uid!.Value;
        ViewBag.Records = _c.Attendances.Where(a => a.UserId == uid).OrderByDescending(a => a.CheckIn).Take(30).ToList();
        ViewBag.TodayRecord = _c.Attendances.FirstOrDefault(a => a.UserId == uid && a.CheckIn.Date == DateTime.Today);
        ViewBag.TotalDays = _c.Attendances.Count(a => a.UserId == uid);
        ViewBag.ThisMonth = _c.Attendances.Count(a => a.UserId == uid && a.CheckIn.Month == DateTime.Today.Month);
        return View();
    }

    public IActionResult CheckIn()
    {
        if (Uid == null) return RedirectToAction("Login", "Auth");
        int uid = Uid!.Value;
        var existing = _c.Attendances.FirstOrDefault(a => a.UserId == uid && a.CheckIn.Date == DateTime.Today);
        if (existing == null)
        {
            _c.Attendances.Add(new Attendance { UserId = uid, CheckIn = DateTime.Now, Status = "Present" });
            _c.SaveChanges();
            TempData["Success"] = "Check-in successful! Welcome to the gym 💪";
        }
        return RedirectToAction("Attendance");
    }

    public IActionResult CheckOut()
    {
        if (Uid == null) return RedirectToAction("Login", "Auth");
        int uid = Uid!.Value;
        var record = _c.Attendances.FirstOrDefault(a => a.UserId == uid && a.CheckIn.Date == DateTime.Today);
        if (record != null && record.CheckOut == null)
        {
            record.CheckOut = DateTime.Now;
            _c.SaveChanges();
            var duration = (record.CheckOut.Value - record.CheckIn).TotalMinutes;
            TempData["Success"] = $"Check-out successful! You worked out for {(int)duration} minutes 🏆";
        }
        return RedirectToAction("Attendance");
    }

    // Progress
    public IActionResult Progress()
    {
        if (Uid == null) return RedirectToAction("Login", "Auth");
        int uid = Uid!.Value;
        ViewBag.Records = _c.ProgressRecords.Where(p => p.UserId == uid).OrderByDescending(p => p.RecordDate).ToList();
        ViewBag.Latest = _c.ProgressRecords.Where(p => p.UserId == uid).OrderByDescending(p => p.RecordDate).FirstOrDefault();
        return View();
    }

    [HttpPost]
    public IActionResult AddProgress(double weight, double height, double chest, double waist, double hips, string notes)
    {
        if (Uid == null) return RedirectToAction("Login", "Auth");
        int uid = Uid!.Value;
        double bmi = height > 0 ? Math.Round(weight / Math.Pow(height / 100.0, 2), 1) : 0;
        _c.ProgressRecords.Add(new ProgressRecord
        {
            UserId = uid, RecordDate = DateTime.Now,
            Weight = weight, Height = height, Chest = chest,
            Waist = waist, Hips = hips, BMI = bmi, Notes = notes
        });
        _c.SaveChanges();
        TempData["Success"] = "Progress recorded successfully!";
        return RedirectToAction("Progress");
    }

    // Chat with Trainer
    public IActionResult Chat(int? trainerId)
    {
        if (Uid == null) return RedirectToAction("Login", "Auth");
        int uid = Uid!.Value;
        var trainers = _c.Users.Where(u => u.Role == "Trainer").ToList();

        int tid = trainerId ?? 0;
        List<ChatMessage> messages = new();

        if (tid > 0)
        {
            messages = _c.ChatMessages
                .Where(m => (m.SenderId == uid && m.ReceiverId == tid) ||
                            (m.SenderId == tid && m.ReceiverId == uid))
                .OrderBy(m => m.SentAt).ToList();

            // Mark as read
            var unread = messages.Where(m => m.ReceiverId == uid && !m.IsRead).ToList();
            unread.ForEach(m => m.IsRead = true);
            _c.SaveChanges();
        }

        // Unread per trainer
        var unreadPerTrainer = _c.ChatMessages
            .Where(m => m.ReceiverId == uid && !m.IsRead)
            .GroupBy(m => m.SenderId)
            .ToDictionary(g => g.Key, g => g.Count());

        ViewBag.Trainers = trainers;
        ViewBag.Messages = messages;
        ViewBag.SelectedTrainerId = tid;
        ViewBag.UnreadPerTrainer = unreadPerTrainer;
        return View();
    }

    [HttpPost]
    public IActionResult SendMessage(int receiverId, string message)
    {
        if (Uid == null) return RedirectToAction("Login", "Auth");
        int uid = Uid!.Value;
        var sender = _c.Users.Find(uid);
        _c.ChatMessages.Add(new ChatMessage
        {
            SenderId = uid,
            ReceiverId = receiverId,
            Message = message,
            SentAt = DateTime.Now,
            SenderName = sender?.Name ?? "",
            SenderRole = "User"
        });
        _c.SaveChanges();
        return RedirectToAction("Chat", new { trainerId = receiverId });
    }

    public IActionResult BookTrainer(int id)
    {
        if (Uid == null) return RedirectToAction("Login", "Auth");
        int uid = Uid!.Value;
        // Verify trainer exists
        var trainer = _c.Users.FirstOrDefault(u => u.Id == id && u.Role == "Trainer");
        if (trainer == null) return RedirectToAction("Dashboard");

        bool alreadyBooked = _c.TrainerBookings.Any(b => b.UserId == uid && b.TrainerId == id);
        if (alreadyBooked)
        {
            TempData["Success"] = "You have already booked this trainer!";
            return RedirectToAction("Dashboard");
        }
        _c.TrainerBookings.Add(new TrainerBooking { UserId = uid, TrainerId = id, BookingDate = DateTime.Now });
        _c.SaveChanges();
        TempData["Success"] = $"Trainer {trainer.Name} booked successfully!";
        return RedirectToAction("Dashboard");
    }
}
