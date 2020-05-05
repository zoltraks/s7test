dotnet restore
@rem dotnet restore -r win-x64 --verbosity detailed
dotnet build -c Release
dotnet publish -c Release -o publish
