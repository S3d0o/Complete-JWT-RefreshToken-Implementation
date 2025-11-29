# **Complete JWT + Refresh Token Implementation for .NET**
“This repository focuses on providing a complete JWT + Refresh Token system.
The project structure here is only an example; you can place the interfaces,
services, helpers, and controllers into your own architecture (Clean, Onion, MVC, etc.).”

---

This repository contains a **secure, production-grade implementation** of:

* **JWT Access Tokens**
* **Refresh Tokens**
* **Token Rotation**
* **Revocation**
* **Security Stamp Validation**
* **Refresh Token Theft Detection**
* **Concurrency Protection**

The repo is intentionally lightweight, educational, and clean — perfect for learning or integrating a real authentication system in any .NET project.

---

#  **Features Included**

### 🔐 **Access Token**

* Short-lived JWT (10 minutes)
* Contains UserId, Email, DisplayName, Roles
* Signed with HMAC-SHA256
* No DB checks required

### 🔄 **Refresh Token**

* Long-lived token stored **securely in database**
* 64-byte cryptographically secure random string
* Hashed using **SHA-256**
* Contains:

  * Created date
  * Expiration date
  * CreatedByIp
  * LastUsed + LastUsedByIp
  * Revoked + RevokedByIp
  * Replacement token hash
  * Security stamp snapshot

### 🔁 **Token Rotation**

* Old refresh token is revoked
* New refresh token is generated
* Concurrency-safe using `RowVersion`

### 🛡 **Theft Detection**

Detects suspicious refresh token reuse:

* If token is reused too quickly from a different IP
* If already rotated token is used again

### 🔒 **Security Stamp Validation**

If the user changes:

* password,
* 2FA settings,
* security info…

→ **All refresh tokens become invalid automatically**

---

# 📁 **Project Structure**

```
Jwt-RefreshToken-Implementation/
│
├── src/
│   ├── Controllers/
│   │   └── AuthenticationController.cs
│   │
│   ├── Interfaces/
│   │   ├── ITokenService.cs
│   │   ├── IServiceManager.cs
│   │   ├── IIdentityUnitOfWork.cs
│   │   ├── IRefreshTokenRepository.cs
│   │   └── IAuthenticationService.cs
│   │
│   ├── Services/
│   │   ├── TokenService.cs
│   │   ├── ServiceManager.cs
│   │   └── AuthenticationService.cs
│   │
│   ├── Entities/
│   │   └── RefreshToken.cs
│   │
│   ├── Repositories/
│   │   ├── IdentityUnitOfWork.cs
│   │   └── RefreshTokenRepository.cs
│   │
│   ├── Helpers/
│   │   └── ClientIpProvider.cs
│   │
│   ├── Infrastructure DI Setup/
│   ├── InfraStructureServiceExtentions.cs
│   └── Readme.md
│
├── docs/
│   ├── access-vs-refresh-story.md
│   └── token-flow.md
│
└── README.md

```

---

## 📖 Understanding Access Token vs Refresh Token

If you want a simple, visual explanation of the difference between them,  
see the short story inside:

👉 [Access Token vs Refresh Token - Short Story](./docs/access-vs-refresh-story.md)

# 📊 **Token Flow Diagram**

👉 [Token Flow Diagram](./docs/token-flow.md)

---

# 📝 **Technologies Used**

* ASP.NET Core
* Entity Framework Core
* Identity Framework
* JWT (System.IdentityModel.Tokens.Jwt)
* SQL Server
* C#

---

#  **Security Best Practices Implemented**

✔ Hashing refresh tokens using SHA-256

✔ Token rotation

✔ Last-used IP detection

✔ Reuse detection + blocking

✔ Security stamp integration

✔ Concurrency-safe DB writes

✔ Short-lived access tokens

✔ Long-lived but protected refresh tokens

---

#  **License**

You may choose MIT, Apache, or GPL.
