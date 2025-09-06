# Web Development Interview Test

# Repo Structure

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
cd api
dotnet run

# Step to run the Client
cd client
npm install
npm run dev

# Step to run the Api.Test
cd Api.Test
dotnet restore
dotnet build
dotnet test

