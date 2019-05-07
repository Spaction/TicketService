# TicketService

Create SQL Server instance and change connection string in appsettings.json
Execute SQL Scripts under a created dateabase called Tickets (or anything changed by connection string). Ensure the executing use has permissions to execute against the database through windows authentication.

Load Website.sln into Visual studio 2019 and build.
ensure that node.js (npm installed) is present and installed. Browserfiy needs to execute for building scripts into the UI.

##Front End GUI is rough and had less time to complete\polish than I would have liked due to trip this past weekend.

##Currently is a concurrency issue with database saves when trying to update map. Most likely related to the automatic expiration of a held seat.

Additionally there are no Unit tests created\uploaded due to the time constraints mentioned above.
