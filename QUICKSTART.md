# ⚡ INSTANT START - Copy & Paste These Commands

## For Windows PowerShell (Administrator)

```powershell
# Step 1: Install .NET SDK 10
winget install Microsoft.DotNet.SDK.10

# Step 2: Install PostgreSQL
winget install PostgreSQL.PostgreSQL

# Step 3: Create Database
psql -h localhost -U postgres -c "CREATE DATABASE ""InsureZenDBv3"" ENCODING 'UTF8';"

# Step 4: Navigate to Project
cd d:\InsureZenv2

# Step 5: Setup & Run (These 3 commands)
dotnet restore
dotnet ef database update
dotnet run
```

## Then Open Your Browser

```
https://localhost:5000/swagger/index.html
```

---

## That's It!

You now have:
- ✅ Backend API running
- ✅ Database created
- ✅ 12 endpoints ready to test
- ✅ Swagger UI for interactive testing

**⏱️ Time: 5-10 minutes**

---

## Next: Test the Complete Workflow

See [SETUP_SUMMARY.md](SETUP_SUMMARY.md) for step-by-step workflow test commands.

---

## Issues?

- PostgreSQL won't install? → See [README.md](README.md) Troubleshooting
- Port 5001 in use? → See [README.md](README.md) Troubleshooting  
- Can't connect to database? → See [README.md](README.md) Troubleshooting

---

## Want More Details?

- [START_HERE.md](START_HERE.md) - Overview
- [SETUP_SUMMARY.md](SETUP_SUMMARY.md) - Quick reference
- [README.md](README.md) - Complete guide
- [API_DOCUMENTATION.md](API_DOCUMENTATION.md) - API reference
