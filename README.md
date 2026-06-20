# MegaFitness Pro 🏋️

A full-stack **Gym Management System** built with ASP.NET Core MVC. Supports Admin, Trainer, and Member roles with attendance tracking, membership plans, trainer booking, progress tracking, payments, and an AI-powered fitness tips assistant.

## ✨ Features

- **Role-based access** — Admin, Trainer, and User dashboards
- **Membership plans** — Basic / Standard / Premium with pricing & features
- **Attendance tracking** — Check-in / check-out logs
- **Trainer booking** — Book sessions with trainers, view specialization & ratings
- **Progress tracking** — Log weight, height, BMI, and body measurements over time
- **Payments** — Simulated payment & transaction history
- **In-app chat** — Messaging between members and trainers/admin
- **AI Fitness Tips** — Personalized tips powered by the Groq API

## 🛠️ Tech Stack

- **Backend:** ASP.NET Core MVC (.NET 8)
- **Database:** Entity Framework Core (In-Memory provider)
- **Frontend:** Razor Views, Bootstrap
- **AI:** Groq API (LLM-powered tips)

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)

### Setup

1. Clone the repo
   ```bash
   git clone https://github.com/<your-username>/MegaFitnessPro.git
   cd MegaFitnessPro/MegaFitnessPro1
   ```

2. Add your Groq API key (get one free at [console.groq.com](https://console.groq.com)) using .NET user-secrets — **do not put it in `appsettings.json`**:
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "Groq:ApiKey" "your_api_key_here"
   ```

3. Run the app
   ```bash
   dotnet run
   ```

4. Open `http://localhost:5050`

### Demo Logins

| Role    | Email               | Password     |
|---------|---------------------|--------------|
| Admin   | admin@fitness.com   | admin123     |
| Trainer | rahul@fitness.com   | trainer123   |
| Trainer | priya@fitness.com   | trainer123   |
| Trainer | arjun@fitness.com   | trainer123   |

> ⚠️ This is a demo/learning project — passwords are stored in plain text and the database is in-memory (resets on restart). Not production-ready as-is.

## 📁 Project Structure

```
MegaFitnessPro1/
├── Controllers/    # MVC controllers (Auth, Admin, User, AI, Home)
├── Models/         # Data models
├── Data/           # EF Core DbContext
├── Views/          # Razor views per controller
└── Program.cs      # App startup & seed data
```

## 📌 Roadmap / Ideas
- Persistent database (SQL Server / PostgreSQL)
- Password hashing (BCrypt/Identity)
- Real payment gateway integration
- Unit tests

## 📄 License
This project is open source and available under the [MIT License](LICENSE).
