# Oracle Cloud ADO.NET Advanced CRUD Demo

A comprehensive, production-ready C# asynchronous console application demonstrating advanced **CRUD (Create, Read, Update, Delete)** operations using pure **ADO.NET** and the official **Oracle Managed Data Access** driver (`Oracle.ManagedDataAccess.OpenConfiguration`).

This educational project establishes a secure, encrypted connection to an **Oracle Cloud Autonomous Database (ADB)** instance using an Oracle Wallet, advanced configuration builders, and data isolation patterns.

## 🚀 Advanced Tech Stack & Concepts Demonstrated

- **Pure ADO.NET Pipeline:** Hands-on implementation of `OracleConnection`, `OracleCommand`, `OracleDataReader`, and `OracleParameter` mapping.
- **Asynchronous Workflow:** Native utilization of `await using` infrastructure for robust, non-blocking I/O operations and automatic resource disposal.
- **Transaction Management:** Multi-command atomic transactions managed securely from C# using `BeginTransactionAsync()`, `CommitAsync()`, and automatic structural `RollbackAsync()` upon database errors.
- **Stored Procedures & REF CURSORs:** Execution of PL/SQL stored procedures, safe extraction of `OUT` parameters (`OracleDecimal` data conversion), and dynamic table streaming via a `SYS_REFCURSOR` parsed directly into an `OracleRefCursor`.
- **Database Contexts:** Execution of runtime context markers like `$$PLSQL_UNIT` embedded in the execution tree.
- **Secure Architecture:** Decoupling sensitive data from source control via `.NET Configuration Builder` (`appsettings.json` and local `User Secrets`).

## 🛠️ Prerequisites

- **.NET 8.0 SDK** or newer.
- **Visual Studio Code** with the C# Dev Kit extension.
- Access to an **Oracle Cloud ADB** instance with an exported **Wallet** configuration directory.

## ⚙️ Configuration

To execute this demo locally without exposing credentials, initialize .NET User Secrets in your repository terminal:

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:OracleCloudConnection" "User Id=your_user;Password=your_password;Data Source=(DESCRIPTION=...);"
dotnet user-secrets set "Settings:OracleWalletPath" "C:\(\path\to\your\wallet\)"
dotnet user-secrets set "Settings:SslServerDNMatch" "false"
```

## 💻 How to Run

```bash
dotnet run
```

