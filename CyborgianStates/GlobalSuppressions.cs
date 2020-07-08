// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.
using System.Diagnostics.CodeAnalysis;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Bezeichner dürfen keine Unterstriche enthalten", Justification = "<Ausstehend>", Scope = "type", Target = "CyborgianStates.AppSettings")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1707:Bezeichner dürfen keine Unterstriche enthalten", Justification = "<Ausstehend>", Scope = "type", Target = "CyborgianStates.Services.NationStatesApiDataService")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Sammlungseigenschaften müssen schreibgeschützt sein", Justification = "<Ausstehend>", Scope = "type", Target = "CyborgianStates.Models.User")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Zeichenfolgen in Großbuchstaben normalisieren", Justification = "<Ausstehend>", Scope = "member", Target = "~M:CyborgianStates.Helpers.FromID(System.String)~System.String")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Zeichenfolgen in Großbuchstaben normalisieren", Justification = "<Ausstehend>", Scope = "member", Target = "~M:CyborgianStates.Helpers.ToID(System.String)~System.String")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>", Scope = "member", Target = "~M:CyborgianStates.CommandHandling.NationStatesApiRequestQueue.ExecuteRequest(CyborgianStates.Interfaces.Request)~System.Threading.Tasks.Task")]
[assembly: SuppressMessage("Design", "CA1031:Keine allgemeinen Ausnahmetypen abfangen", Justification = "<Ausstehend>", Scope = "member", Target = "~M:CyborgianStates.Services.BotService.ProgressMessage(CyborgianStates.Interfaces.MessageReceivedEventArgs)~System.Threading.Tasks.Task")]
