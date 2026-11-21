# Deployment Guide

TCGC Library Management System — Windows desktop application (.NET 8 WinForms) with a MySQL database.

---

## 1. What you need

| Item | Detail |
|------|--------|
| Operating system | Windows 10 or 11, 64-bit |
| Database | MySQL 8.x or MariaDB 10.4+ (the XAMPP MariaDB server is supported) |
| Build machine | .NET SDK 8 or newer (`dotnet --version`) |
| Target machine | Nothing extra if you publish self-contained (section 4B) |
| Optional hardware | Webcam, only for the QR attendance scanner |

No API keys, cloud services or third party accounts are used. All data stays in your MySQL server.

---

## 2. Database setup

### 2A. Fresh install (new server)

```powershell
mysql -u root -p -P 3306 < "database\libsys_database.sql"
```

The script already contains `CREATE DATABASE IF NOT EXISTS libsys_database` and `USE libsys_database`, so it creates the schema and seeds the lookup tables (roles, statuses, institutes, evaluation questions). It does **not** create a login account — see section 5.

Verify:

```powershell
mysql -u root -p -P 3306 -e "USE libsys_database; SHOW TABLES;"
```

You should see 21 tables.

### 2B. Upgrade an existing installation

- **Before anything else, take a backup:**

  ```powershell
  mysqldump -u root -p -P 3306 --hex-blob --result-file=backup_before_upgrade.sql libsys_database
  ```

- The application creates and upgrades the tables/columns it needs at startup (`evaluation_*`, `attendance_*`,
  `borrow_request_*`, `account_table.profile_picture`, `contact_number` columns). You do **not** have to
  run these by hand, but the database user therefore needs `CREATE`, `ALTER` and `INDEX` privileges.
- `database\normalize_to_3nf.sql` builds a verified `libsys_database_3nf` staging copy from either the legacy
  or the current normalized schema. It never modifies the source database. Review the staging copy, then
  swap it in using the steps in `database/NORMALIZATION.md`.

---

## 3. Configuration

The connection defaults point at a local XAMPP MySQL/MariaDB server (port 3306):

```
server = localhost   port = 3306   user = root   password = (empty)   database = libsys_database
```

To point the same executable at another server, set **environment variables** — no rebuild needed:

| Variable | Meaning | Default |
|----------|---------|---------|
| `LIBRARY_DB_SERVER` | Server name or IP | `localhost` |
| `LIBRARY_DB_PORT` | TCP port | `3306` |
| `LIBRARY_DB_USER` | MySQL user | `root` |
| `LIBRARY_DB_PASSWORD` | MySQL password (taken verbatim, spaces included) | empty |
| `LIBRARY_DB_NAME` | Database name | `libsys_database` |
| `LIBRARY_DB_CONNECTION` | Full connection string; overrides everything above and can add options such as `SslMode=Required` | — |

Example (current session only):

```powershell
$env:LIBRARY_DB_SERVER   = "10.0.0.25"
$env:LIBRARY_DB_PORT     = "3306"
$env:LIBRARY_DB_USER     = "libsys_app"
$env:LIBRARY_DB_PASSWORD = "change-me"
$env:LIBRARY_DB_NAME     = "libsys_database"
.\library_management_system.exe
```

For a permanent setting, use **System Properties → Environment Variables**, or:

```powershell
setx LIBRARY_DB_PASSWORD "change-me"
```

`setx` only affects newly started processes — log off/on (or reboot) before launching the app.

> An invalid port value silently falls back to `3306`; an empty variable falls back to its default.

---

## 4. Build and publish

Close the application first — a running instance locks `bin\...\library_management_system.exe`.

### 4A. Framework-dependent build (needs .NET 8 Desktop Runtime on the target PC)

```powershell
dotnet build -c Release
```

Output: `bin\Release\net8.0-windows\`.

### 4B. Self-contained single file (recommended for hand-over)

```powershell
dotnet publish -p:PublishProfile=win-x64
```

Output: `bin\Release\net8.0-windows\publish\win-x64\library_management_system.exe` (~79 MB, runtime bundled).
Copy that one file (plus the `.pdb` if you want readable crash traces) to the target computer and run it.

> Windows SmartScreen may warn about an unsigned executable on first launch. Choose
> **More info → Run anyway**, or sign the binary with your organisation's certificate.

---

## 5. First run on a fresh database

Sign-up always stores the new account as **PENDING**, and a PENDING account cannot sign in. So the first
librarian has to be created, then activated once in SQL:

1. Launch the app → **Sign up** → choose role **LIBRARIAN**, fill in the form, submit. Close the app.
2. Activate it:

   ```sql
   USE libsys_database;

   UPDATE account_role_table ar
   JOIN account_status_table st ON st.status_id = ar.status_id
   SET ar.status_id = (SELECT status_id FROM account_status_table WHERE status_code = 'ACTIVE')
   WHERE st.status_code = 'PENDING'
     AND ar.role_id = (SELECT role_id FROM role_table WHERE role_code = 'LIBRARIAN');
   ```

3. Start the app and sign in with that librarian account.
4. In the librarian dashboard, approve the remaining student/faculty/staff registrations.

If you are restoring an existing installation instead, import the dump and skip this section.

### Passwords

Passwords are stored as PBKDF2-SHA256 hashes (`PBKDF2-SHA256$iterations$salt$hash`).
Rows created by an older build still hold plain text; each one is converted to a hash automatically on its
next successful sign-in. Nothing extra to run — just make sure every existing user signs in at least once
if you want the plain text gone from the table. New sign-ups and password changes are hashed immediately.

---

## 6. Smoke test after deploying

Run through this list once on the target machine:

- [ ] App starts and the welcome/login screen appears with logos and background.
- [ ] Librarian signs in.
- [ ] Librarian dashboard loads counts (accounts, books, loans, attendance) without a "MYSQL OFFLINE" label.
- [ ] Add a book with a cover image; it appears in the catalog.
- [ ] Sign up a student, approve it from the librarian dashboard, then sign in as that student.
- [ ] Student requests a book; librarian approves the request; loan shows under borrowed books.
- [ ] Return the book; the loan moves out of the active list.
- [ ] QR attendance scan writes a row (webcam) or is skipped if no camera is available.
- [ ] Evaluation form opens from the evaluation login screen and stores a response.
- [ ] Each report exports to PDF, and the library report saves as `.txt` to a chosen folder.
- [ ] Edit a full name in the librarian dashboard and confirm it changes in loans, attendance and
      evaluation records too.
- [ ] Change a password, sign out and sign back in with the new one.

---

## 7. Security notes

- **Database credentials** live in environment variables, not in the source. Rename the default `root`
  account usage before going live; a dedicated account is better:

  ```sql
  CREATE USER 'libsys_app'@'%' IDENTIFIED BY 'a-strong-password';
  GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, INDEX, REFERENCES
        ON libsys_database.* TO 'libsys_app'@'%';
  FLUSH PRIVILEGES;
  ```

  `CREATE`/`ALTER`/`INDEX` are required because the app creates a few tables and columns at startup.
  Avoid granting `DROP`, `FILE` or global privileges. `REFERENCES` is needed so the foreign keys in the
  runtime-created tables can be defined.
- **Remote access**: put MySQL behind a firewall and add `SslMode=Required` through
  `LIBRARY_DB_CONNECTION` if the traffic crosses an untrusted network.
- **Backups**: schedule a nightly `mysqldump` of `libsys_database`; the app has no built-in backup.
- **Physical access**: the librarian dashboard can edit and delete accounts, so keep the workstation
  behind the staff desk. Password columns are masked in the UI.
- Restrict the `.env`-style notes and any local `.pubxml.user` file to your own machine — both are in
  `.gitignore`.

---

## 8. Known build warnings (safe to ignore)

| Warning | Cause |
|---------|-------|
| `NU1701` ×3 | `AForge`, `AForge.Video`, `AForge.Video.DirectShow` target .NET Framework. They work on Windows for webcam capture; a replacement library is only needed if you move off Windows. |
| `CS8981` | The generated resource class is named `resources` (lower-case). Renaming it would touch the designer files, so it is left as is. |

Current status: **0 errors**; the only warnings are the ones listed above.

---

## 9. Troubleshooting

| Symptom | Likely cause / fix |
|---------|--------------------|
| "MYSQL OFFLINE" labels, no data | MySQL not running, wrong port, or wrong environment variables. Test with `mysql -u <user> -p -P <port> -e "SELECT 1"`. |
| Login says incorrect username or password | Username is case-sensitive by design (`BINARY` compare). Legacy plain-text rows still work. |
| Login says account is pending | The account was never approved. See section 5, step 2 (swap `LIBRARIAN` for the role you need). |
| Startup error mentioning `ALTER TABLE` / `CREATE TABLE` | The database user lacks DDL privileges — see section 7. |
| QR scanner does nothing | No webcam, webcam in use by another program, or camera permission blocked in Windows Settings. |
| Build fails with "file is being used by another process" | The app is still running; close it and rebuild. |

---

## 10. Third-party licences

- **MySql.Data** (Oracle) — GPL-2.0 with the Universal FOSS exception; review before redistributing a
  closed-source build.
- **QRCoder** — MIT.
- **ZXing.Net / ZXing.Net.Bindings.Windows.Compatibility** — Apache-2.0 / MIT.
- **AForge.NET** — LGPL-3.0.

Keep these notices with any distributed copy of the application.
