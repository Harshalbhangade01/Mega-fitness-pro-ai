using Microsoft.AspNetCore.Mvc;

public class AuthController : Controller
{
    private readonly AppDbContext _c;
    public AuthController(AppDbContext c) { _c = c; }

    public IActionResult Register() => View();

    [HttpPost]
    public IActionResult Register(User u)
    {
        if (_c.Users.Any(x => x.Email == u.Email))
        { ViewBag.Error = "Email already registered!"; return View(); }
        u.Role = "User";
        u.JoinDate = DateTime.Now;
        _c.Users.Add(u);
        _c.SaveChanges();
        return RedirectToAction("Login");
    }

    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        var u = _c.Users.FirstOrDefault(x => x.Email == email && x.Password == password);
        if (u != null)
        {
            HttpContext.Session.SetInt32("uid", u.Id);
            HttpContext.Session.SetString("uname", u.Name);
            HttpContext.Session.SetString("role", u.Role);
            if (u.Role == "Admin") return RedirectToAction("Dashboard", "Admin");
            if (u.Role == "Trainer") return RedirectToAction("Dashboard", "Trainer");
            return RedirectToAction("Dashboard", "User");
        }
        ViewBag.Error = "Invalid email or password!";
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
