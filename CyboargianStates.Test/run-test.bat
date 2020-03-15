@echo off
coverlet .\bin\Debug\netcoreapp3.1\CyboargianStates.dll --target "dotnet" --targetargs "test --no-build" --exclude-by-file "%UserProfile%\.nuget\packages\microsoft.net.test.sdk\16.4.0\build\netcoreapp2.1\Microsoft.NET.Test.Sdk.Program.cs" --exclude "[*]CyborgianStates.MessageHandling.ConsoleInput" --exclude "[*]CyborgianStates.AppSettings" --exclude "[*]CyborgianStates.Services.NotFoundException" --exclude "[*]CyborgianStates.Models.*" --format cobertura --output "./TestResults//"
pause
reportgenerator -reports:TestResults/coverage.cobertura.xml -targetdir:TestResults/Reports -reporttypes:HtmlInline
start TestResults/Reports/index.htm