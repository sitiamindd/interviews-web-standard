# Web Development Interview Test

# Repo Structure
``` tree
.
|--api/         # ASP.NET Core API. Handle backend
|--client/      # Nuxt 3 app
|--Api.Test/    #xUnit test for API


# Note
dotnet api runs in this solution is using http://localhost:5113 . When starting the api, set the port to be 5113 using cmd as follows:
cd api
set ASPNETCORE_URLS=http://localhost:5113 && dotnet run

To run this solution, you will need to run the API and CLIENT in a separate CMD.
# Step to run the API
1. cd api
2. dotnet run

# Step to run the Client
1. cd client
2. npm install
3. npm run dev

# Step to run the Api.Test
1. cd Api.Test
2. dotnet restore
3. dotnet build
4. dotnet test

