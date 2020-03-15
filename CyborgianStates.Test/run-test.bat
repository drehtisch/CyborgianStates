@echo off
dotnet clean ..\CyborgianStates\CyborgianStates.sln
dotnet build ..\CyborgianStates\CyborgianStates.sln
coverlet bin\Debug\netcoreapp3.1\CyborgianStates.Test.dll --target "dotnet" --targetargs "test --no-build" --exclude "[*]CyborgianStates.MessageHandling.ConsoleInput" --exclude "[*]CyborgianStates.AppSettings" --exclude "[*]CyborgianStates.Models.*" --format cobertura --output "./TestResults//"
pause
reportgenerator -reports:TestResults/coverage.cobertura.xml -targetdir:TestResults/Reports -reporttypes:HtmlInline
start TestResults/Reports/index.htm