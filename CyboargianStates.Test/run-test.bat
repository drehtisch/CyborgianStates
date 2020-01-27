dotnet test --collect:"XPlat Code Coverage" 
reportgenerator -reports:TestResults\*\*.xml -targetdir:TestResults\Reports -reporttypes:HtmlInline
start TestResults\Reports\index.htm