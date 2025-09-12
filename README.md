# 📘 User Management API (with Middleware & Tests)

This project is part of the **[Back-End Development with .NET](https://www.coursera.org/learn/back-end-development-with-dotnet/)** course.  
It demonstrates how to build a **RESTful API** using **ASP.NET Core**, including **custom middleware** for request/response handling and a **controller** for user management.  
The solution also includes a **dedicated test project**.

---

## 📂 Project Structure

```
backend/
│── UserManagementAPI/         # Main ASP.NET Core Web API project
│   ├── Controllers/           # API Controllers
│   │    └── UsersController.cs
│   ├── Middleware/            # Custom middleware
│   │    ├── ErrorHandlingMiddleware.cs
│   │    ├── LoggingMiddleware.cs
│   │    └── RequestTimingMiddleware.cs
│   └── Program.cs / Startup   # API configuration
│
│── UserApi.Tests/              # xUnit test project referencing the API
│   ├── UsersControllerTests.cs
│   ├── MiddlewareTests.cs
│   └── ...
```

---

## ⚙️ Middleware Overview

The API registers multiple middleware components that enhance observability and error handling:

1. **LoggingMiddleware**  
   - Logs incoming requests and outgoing responses.  
   - Useful for debugging and auditing.

2. **RequestTimingMiddleware**  
   - Measures request processing duration.  
   - Helps monitor performance.

3. **ErrorHandlingMiddleware**  
   - Provides a global exception handler.  
   - Returns consistent error responses in JSON format.  

Each middleware is registered in the request pipeline (`Program.cs`) using `app.UseMiddleware<T>()`.

---

## 👥 Users Controller

**`UsersController`** handles user-related endpoints.  

### Routes

| HTTP Method | Endpoint       | Description                  |
|-------------|---------------|------------------------------|
| `GET`       | `/api/users`   | Returns a list of all users |
| `GET`       | `/api/users/{id}` | Gets a single user by ID  |
| `POST`      | `/api/users`   | Creates a new user          |
| `PUT`       | `/api/users/{id}` | Updates an existing user |
| `DELETE`    | `/api/users/{id}` | Deletes a user           |

All endpoints return **JSON responses**.

---

## 🚀 Running the API

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Run the API
```bash
cd UserManagementAPI
dotnet run
```

The API will be available at:
- `http://localhost:5000`
- `https://localhost:5001`

---

## 📖 API Documentation (Swagger)

Swagger UI is enabled by default.

👉 Visit:  
- [http://localhost:5000/swagger](http://localhost:5000/swagger)  
- or [https://localhost:5001/swagger](https://localhost:5001/swagger)

📸 **Swagger Screenshot**  
_Add screenshot here:_  
![Swagger UI](https://github.com/ksunami/UserManagementApiBackend/blob/main/docs/swagger.png)`


---

## ✅ Testing the API

A dedicated test project `UserApi.Tests` validates both the **controller endpoints** and **middleware behavior**.

Run tests:
```bash
cd UserApi.Tests
dotnet test
```

📸 **Test Execution Screenshot**  
_Add screenshot here:_  
![Test Results](https://github.com/ksunami/UserManagementApiBackend/blob/main/docs/tests.png)`


---

## 🔗 Repository

👉 [UserManagementApiBackend](https://github.com/ksunami/UserManagementApiBackend)

---

## ✨ Features

- ASP.NET Core Web API (C# / .NET 8)  
- User controller with CRUD endpoints  
- Custom middleware for logging, error handling, and request timing  
- Swagger UI for interactive API documentation  
- xUnit test project for automated testing  
- Clean separation of API vs Tests  
