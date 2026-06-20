ToDoApp
=======

This solution contains two main projects:

- ToDoApp.Server — the ASP.NET Core backend (C# / .NET 10)
- todoapp.client — the React + TypeScript frontend

Each project contains its own README with usage and testing instructions:

- ToDoApp.Server/README.md — how to run the backend and test the API with Swagger
- todoapp.client/README.md — how to run the frontend and run the Jest test suite

Quick start
-----------
1. Build the solution
   - dotnet build

2. Run the backend
   - From Visual Studio: set ToDoApp.Server as the startup project and press F5
   - From terminal: dotnet run --project ToDoApp.Server/ToDoApp.Server.csproj

3. Run the frontend dev server
   - cd todoapp.client
   - npm install
   - npm run dev

4. To run the application with both backend and frontend:
   - Configure multiple startup behavior for the solution:
     - Right‑click the solution in Solution Explorer and choose Properties.
     - Under Common Properties -> Startup Project select "Multiple startup projects".
     - Set ToDoApp.Server action to "Start". (If you have other Visual Studio projects you want to start, set them to Start as well.)
     - Click OK.

Testing
-------
- Backend unit tests: dotnet test ToDoApp.Server.Tests/ToDoApp.Server.Tests.csproj
- Frontend tests: from todoapp.client run npm test
