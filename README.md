dotnet restore
 To restore project dependencies

dotnet dev-certs https -ep ~/.aspnet/https/aspnetapp.pfx -p yourpassword
dotnet dev-certs https --trust
 To create certs in this project as they are required for docker to run server on https.

Run: Docker compose up —build with docker desktop application to get all the docker compose images running then cd Data from project directory in order to run dotnet ef database update while sqlserver is running (you can either manually start it on docker desktop or dotnet build on CLI).

Make sure to create an .env file and store it in the same directory as the .env.example. Follow the .env.example specs to provide the fields needed for the program to run correctly. 

Lastly just run docker compose up —build, once it’s running run https://localhost:5001 on your browser.
