# 🎓 UpSkill LMS - DEPI Graduation Project

![UpSkill LMS Banner](https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=1200&q=80)

## 📖 Overview
**UpSkill LMS** is a modern, feature-rich Learning Management System developed as a graduation project for the **DEPI** initiative. It empowers instructors to create and share educational content while providing students with an intuitive platform to discover, enroll in, and track their learning progress.

The project is built using a robust **Onion Architecture**, separating the frontend (ASP.NET Core MVC) from the backend (ASP.NET Core Web API) to ensure scalability, clean code, and maintainability.

---

## ✨ Key Features

### 🧑‍🎓 For Students
* **Course Catalog:** Browse and search a comprehensive catalog of published courses with multi-level filtering (Category, Level, Price).
* **Enrollment:** Easily enroll in free or paid courses.
* **Student Dashboard:** Track learning progress, continue recent courses, and view completion status.
* **Course Player:** A modern video player interface with a structured curriculum hierarchy (Sections & Lessons).
* **Certificates:** Automatically generate and download certificates upon reaching 100% course completion.

### 👨‍🏫 For Instructors
* **Instructor Dashboard:** View detailed metrics such as total courses, active students, and publication status.
* **Course Creation:** Create new courses, upload thumbnails, set pricing, and write detailed descriptions.
* **Curriculum Management:** Add sections, lessons, videos, and reading materials to courses.
* **Submission Workflow:** Submit courses to administrators for review and approval before they go live.

### 🛡️ For Administrators
* **Admin Dashboard:** Oversee the entire platform's metrics (Users, Revenue, Total Courses).
* **Course Approval System:** Review courses submitted by instructors and approve them for the public catalog.
* **User Management:** Monitor roles (Student, Instructor, Admin).

---

## 🛠️ Technology Stack

* **Backend & API:** .NET 8 / ASP.NET Core Web API
* **Frontend:** ASP.NET Core MVC, Razor Pages, Bootstrap 5, Custom CSS
* **Architecture:** Onion Architecture (Domain, Application, Infrastructure, Presentation)
* **Database & ORM:** SQL Server, Entity Framework Core (Code-First Approach)
* **Authentication:** JWT (JSON Web Tokens) & ASP.NET Core Identity
* **Design Pattern:** Repository Pattern, Dependency Injection

---

## 📂 Project Structure

```text
DEPI_Graduation_project/
├── Domain/           # Enterprise logic and Entities (Course, User, Lesson, etc.)
├── Application/      # Business logic, Interfaces, Services, DTOs
├── Infrastructure/   # Database context, Repositories, External services implementation
├── UpSkill/          # (API Presentation) Controllers and endpoints
└── UpSkillView/      # (MVC Frontend) User interface, Views, and ViewModels
```

---

## 🚀 Getting Started

### Prerequisites
* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* SQL Server (LocalDB or standard instance)
* Visual Studio 2022 or VS Code

### Installation
1. **Clone the repository:**
   ```bash
   git clone https://github.com/Ahmedfathallahelsayed/DEPI_Graduation_project.git
   ```
2. **Update Connection String:**
   Ensure the connection string in `UpSkill/appsettings.json` points to your SQL Server instance.
3. **Apply Database Migrations:**
   Open the Package Manager Console and run:
   ```bash
   Update-Database
   ```
4. **Run the API & MVC Projects:**
   Set both `UpSkill` (API) and `UpSkillView` (MVC) as startup projects and run the solution.

---

## 🤝 Contributors
Developed by an amazing team as part of the DEPI Graduation Project. 

* **Member 1** - Team Leader / Backend Architecture
* **Ameen (Member 4)** - Student Flow, UI Integrations, API Logic
* *(Add other members here)*

> "Education is the passport to the future, for tomorrow belongs to those who prepare for it today."