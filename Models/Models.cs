public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "User"; // "Admin", "User", "Trainer"
    public DateTime JoinDate { get; set; } = DateTime.Now;
    public string Phone { get; set; } = "";
    // Trainer-specific fields (sirf Role=="Trainer" ke liye use hoga)
    public string Specialization { get; set; } = "";
    public string Bio { get; set; } = "";
    public decimal HourlyRate { get; set; } = 0;
    public int Rating { get; set; } = 5;
}

public class Plan
{
    public int Id { get; set; }
    public string PlanName { get; set; } = "";
    public decimal Price { get; set; }
    public string Description { get; set; } = "";
    public int DurationDays { get; set; } = 30;
    public string Features { get; set; } = "";
}

public class UserPlan
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PlanId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string PaymentStatus { get; set; } = "Paid";
    public string PaymentMethod { get; set; } = "Online";
    public string TransactionId { get; set; } = "";
}

public class TrainerBooking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TrainerId { get; set; } // User.Id jiska Role=="Trainer"
    public DateTime BookingDate { get; set; }
    public string Status { get; set; } = "Confirmed";
    public string Notes { get; set; } = "";
}

public class Attendance
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public string Status { get; set; } = "Present";
}

public class ProgressRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime RecordDate { get; set; }
    public double Weight { get; set; }
    public double Height { get; set; }
    public double Chest { get; set; }
    public double Waist { get; set; }
    public double Hips { get; set; }
    public double BMI { get; set; }
    public string Notes { get; set; } = "";
}

public class ChatMessage
{
    public int Id { get; set; }
    public int SenderId { get; set; }    // User.Id
    public int ReceiverId { get; set; }  // User.Id
    public string Message { get; set; } = "";
    public DateTime SentAt { get; set; } = DateTime.Now;
    public bool IsRead { get; set; } = false;
    public string SenderName { get; set; } = "";
    public string SenderRole { get; set; } = "";
}

public class Payment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PlanId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "";
    public string TransactionId { get; set; } = "";
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    public string Status { get; set; } = "Success";
    public string CardLast4 { get; set; } = "";
}
