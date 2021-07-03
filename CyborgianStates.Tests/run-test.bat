@echo off
REM Run following commands first
REM 
REM ============================
REM dotnet tool install --global coverlet.console
REM dotnet tool install -g dotnet-reportgenerator-globaltool
REM ============================
REM
REM Note that some coverage may not be detected correctly by reportgenerator
REM
dotnet clean ..\CyborgianStates\CyborgianStates.sln
dotnet build ..\CyborgianStates\CyborgianStates.sln
coverlet bin\Debug\net5.0\CyborgianStates.Tests.dll --target "dotnet" --targetargs "test --no-build" --exclude "[*]CyborgianStates.MessageHandling.ConsoleInput" --exclude "[*]CyborgianStates.MessageHandling.DiscordClientWrapper" --exclude "[*]CyborgianStates.AppSettings" --exclude "[*]CyborgianStates.Data.Models.*" --exclude "[*]CyborgianStates.Data.SqliteSqlProvider" --exclude "[*]CyborgianStates.BotEnvironment" --exclude "[*]*.Exceptions.*" --exclude "[NationStatesSharp]*" --format opencover --output "./TestResults//"
pause
reportgenerator -reports:TestResults/coverage.opencover.xml -targetdir:TestResults/Reports -reporttypes:HtmlInline
start TestResults/Reports/index.htm