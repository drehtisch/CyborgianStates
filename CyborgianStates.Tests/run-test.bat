@echo off
dotnet clean ..\CyborgianStates\CyborgianStates.sln
dotnet build ..\CyborgianStates\CyborgianStates.sln
coverlet bin\Debug\netcoreapp3.1\CyborgianStates.Tests.dll --target "dotnet" --targetargs "test --no-build" --exclude "[*]CyborgianStates.MessageHandling.ConsoleInput" --exclude "[*]CyborgianStates.AppSettings" --exclude "[*]CyborgianStates.Models.*" --exclude "[*]CyborgianStates.Data.SqliteSqlProvider" --format opencover --output "./TestResults//"
pause
reportgenerator -reports:TestResults/coverage.opencover.xml -targetdir:TestResults/Reports -reporttypes:HtmlInline
start TestResults/Reports/index.htm