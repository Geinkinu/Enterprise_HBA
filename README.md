# Enterprise Programming – Bulk Import Application

**Unit:** ITSFT-506-2011  
**Assignment:** Building an Enterprise Application using a Clean Architecture

---

## Public Deployment URL

<u>Live application (myasp.net)</u>  
http://gerrardcompton-001-site1.ntempurl.com/

The deployed application is fully functional and demonstrates the complete workflow required by the assignment.

---

## Repository Contents

This repository contains:

- Full source code for the application
- EF Core migrations (schema creation)
- Commit history
- Test JSON file used for bulk import
- Clean architecture with separate **Domain**, **DataAccess**, and **Web** layers

---

## Database & EF Core Migrations

### Database configuration

The application uses SQL Server LocalDB via:

(localdb)\MSSQLLocalDB

The database name is **not hardcoded** and is resolved via the configured connection string.

---

## Running the Application Locally

To run the application locally:

1. Open the solution in Visual Studio
2. Ensure the launch profile is set to **HTTPS**
3. Open **Package Manager Console**
4. Run the following command:
Update-Database -Context AppDbContext

This will create the database schema using the included EF Core migrations.

**Note:**  
Data is not seeded automatically. All data is introduced via the **Bulk Import workflow**, as required by the assignment brief.

---

## Submission Notes

- Public URL is provided above
- Git repository includes full source code, migrations, and commit history
- JSON file used for testing bulk import is submitted via **VLE**
- Demonstration video is also submitted via **VLE**

