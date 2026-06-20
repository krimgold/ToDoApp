ToDoApp.Server
=================

This project is the ASP.NET Core backend for the ToDoApp. It exposes the API endpoints and includes JWT authentication.

Running and testing via Swagger
------------------------------
1. Start the server
   - From Visual Studio: press F5 (or Debug -> Start Debugging).
   - From command line: dotnet run --project ToDoApp.Server/ToDoApp.Server.csproj

2. Open the Swagger UI
   - By default Swagger is available at: https://localhost:7196/swagger
   - The exact URL and port may differ based on your launch settings.

3. Authenticate and obtain a JWT
   - Expand the POST /api/login endpoint in Swagger.
   - Click "Try it out" and provide a JSON body with valid credentials defined in appsettings.json under the "Auth" section. 
	Current valid credentials are:
	 {
	   "username": "admin",
	   "password": "admin"
	 }
   - Execute the request. A successful response contains a TokenResponse object with the JwtToken string and expires seconds.

4. Add the JWT to Swagger Authorization
   - In the Swagger UI click the "Authorize" button (lock icon) in the top-right.
   - In the "value" field enter: Bearer <your-jwt-token>
	 (for example: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...)
   - Click "Authorize" and close the dialog.

5. Call secured endpoints
   - With the JWT authorized you can now call endpoints under /api/tasks that require authentication.
   - Use the Try it out buttons to GET/POST/PUT/DELETE tasks.

Notes
-----
- If you change appsettings.json you may need to restart the application for the new settings to take effect.
- If run from Visual Studio and you use the IIS Express profile, the swagger URL will include the configured port for that profile.
- For integration testing from code, consider using Microsoft.AspNetCore.Mvc.Testing and WebApplicationFactory with test-specific configuration.

If you want I can add an example cURL command for login and a sample JWT usage.