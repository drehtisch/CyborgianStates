using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CyborgianStates.CommandHandling;
using CyborgianStates.Enums;
using CyborgianStates.Exceptions;
using CyborgianStates.Interfaces;
using CyborgianStates.MessageHandling;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CyborgianStates.Commands
{
    public class NationStatsCommand : ICommand
    {
        private readonly AppSettings _config;
        private readonly IRequestDispatcher _dispatcher;
        private readonly IResponseBuilder _responseBuilder;
        private readonly ILogger _logger;
        private CancellationToken token;

        public NationStatsCommand()
        {
            _logger = ApplicationLogging.CreateLogger(typeof(NationStatsCommand));
            _dispatcher = (IRequestDispatcher) Program.ServiceProvider.GetService(typeof(IRequestDispatcher));
            _config = ((IOptions<AppSettings>) Program.ServiceProvider.GetService(typeof(IOptions<AppSettings>))).Value;
            _responseBuilder = (IResponseBuilder) Program.ServiceProvider.GetService(typeof(IResponseBuilder));
        }

        public async Task<CommandResponse> Execute(Message message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            Request request = new Request(RequestType.GetBasicNationStats, ResponseFormat.XmlResult, DataSourceType.NationStatesAPI);
            try
            {
                _logger.LogDebug($"{message.Content}");
                var parameters = message.Content.Split(" ").Skip(1);
                if (parameters.Any())
                {
                    string nationName = string.Join(" ", parameters);
                    request.Params.Add("nationName", Helpers.ToID(nationName));
                    _dispatcher.Dispatch(request, 0);
                    await request.WaitForResponseAsync(token).ConfigureAwait(false);
                    await ProcessResultAsync(request).ConfigureAwait(false);
                    CommandResponse commandResponse = _responseBuilder.Build();
                    await message.Channel.ReplyToAsync(message, commandResponse).ConfigureAwait(false);
                    return commandResponse;
                }
                else
                {
                    return await FailCommandAsync(message, "No parameter passed.").ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException e)
            {
                _logger.LogError(e.ToString());
                return await FailCommandAsync(message, "Request/Command has been canceled. Sorry :(").ConfigureAwait(false);
            }
            catch (HttpRequestFailedException e)
            {
                _logger.LogError(e.ToString());
                return await FailCommandAsync(message, request.FailureReason).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
                return await FailCommandAsync(message, "An unexpected error occured. Please contact the bot administrator.").ConfigureAwait(false);
            }
        }

        public void SetCancellationToken(CancellationToken cancellationToken)
        {
            token = cancellationToken;
        }

        private async Task<CommandResponse> FailCommandAsync(Message message, string reason)
        {
            _responseBuilder.Clear();
            _responseBuilder.WithColor(Discord.Color.Red)
                .FailWithDescription(reason)
                .WithFooter(_config.Footer);
            var response = _responseBuilder.Build();
            await message.Channel.ReplyToAsync(message, response).ConfigureAwait(false);
            return response;
        }

        private async Task<string> GetOfficerPositionAsync(string nationName, string regionName)
        {
            var request = new Request(RequestType.GetRegionalOfficers, ResponseFormat.XmlResult, DataSourceType.NationStatesAPI);
            request.Params.Add("regionName", Helpers.ToID(regionName));
            _dispatcher.Dispatch(request, 0);
            await request.WaitForResponseAsync(token).ConfigureAwait(false);
            if (request.Response is XmlDocument result)
            {
                var doc = ToXDocument(result);

                var list = doc.Descendants().Where(e => e.Name == "NATION" && e.Value == nationName);
                if (list.Any())
                {
                    var office = list.First().Parent;
                    var officeName = office.Elements().Where(e => e.Name == "OFFICE");
                    return officeName.First().Value;
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                throw new InvalidOperationException("Expected Response to be XmlDocument but wasn't.");
            }
        }

        private static XDocument ToXDocument(XmlDocument xmlDocument) => XDocument.Parse(xmlDocument.OuterXml);

        private async Task ProcessResultAsync(Request request)
        {
            if (request.ExpectedReponseFormat == ResponseFormat.XmlResult && request.Response is XmlDocument nationStats)
            {
                string name = request.Params["nationName"].ToString();
                string demonymplural = nationStats.GetElementsByTagName("DEMONYM2PLURAL")[0].InnerText;
                string category = nationStats.GetElementsByTagName("CATEGORY")[0].InnerText;
                string flagUrl = nationStats.GetElementsByTagName("FLAG")[0].InnerText;
                string fullname = nationStats.GetElementsByTagName("FULLNAME")[0].InnerText;
                string population = nationStats.GetElementsByTagName("POPULATION")[0].InnerText;
                string region = nationStats.GetElementsByTagName("REGION")[0].InnerText;
                string founded = nationStats.GetElementsByTagName("FOUNDED")[0].InnerText;
                string foundedtime = nationStats.GetElementsByTagName("FOUNDEDTIME")[0].InnerText;
                var dateFounded = DateTimeOffset.FromUnixTimeSeconds(long.Parse(foundedtime)).UtcDateTime;
                string lastActivity = nationStats.GetElementsByTagName("LASTACTIVITY")[0].InnerText;
                string Influence = nationStats.GetElementsByTagName("INFLUENCE")[0].InnerText;
                string wa = nationStats.GetElementsByTagName("UNSTATUS")[0].InnerText;
                XmlNodeList freedom = nationStats.GetElementsByTagName("FREEDOM")[0].ChildNodes;
                string civilStr = freedom[0].InnerText;
                string economyStr = freedom[1].InnerText;
                string politicalStr = freedom[2].InnerText;
                XmlNodeList census = nationStats.GetElementsByTagName("CENSUS")[0].ChildNodes;
                string civilRights = census[0].ChildNodes[0].InnerText;
                string economy = census[1].ChildNodes[0].InnerText;
                string politicalFreedom = census[2].ChildNodes[0].InnerText;
                string influenceValue = census[3].ChildNodes[0].InnerText;
                string endorsementCount = census[4].ChildNodes[0].InnerText;
                string residency = census[5].ChildNodes[0].InnerText;
                double residencyDbl = Convert.ToDouble(residency, _config.CultureInfo);
                var dateJoined = DateTime.UtcNow.Subtract(TimeSpan.FromDays(residencyDbl));
                int residencyYears = (int) (residencyDbl / 365.242199);
                int residencyDays = (int) (residencyDbl % 365.242199);
                double populationdbl = Convert.ToDouble(population, _config.CultureInfo);
                string nationUrl = $"https://www.nationstates.net/nation={Helpers.ToID(name)}";
                string regionUrl = $"https://www.nationstates.net/region={Helpers.ToID(region)}";
                string waVoteString = "";
                string endoString = "";
                if (wa == "WA Member")
                {
                    var gaVote = nationStats.GetElementsByTagName("GAVOTE")[0].InnerText;
                    var scVote = nationStats.GetElementsByTagName("SCVOTE")[0].InnerText;
                    if (!string.IsNullOrWhiteSpace(gaVote))
                    {
                        waVoteString += $"GA: {gaVote}";
                    }
                    if (!string.IsNullOrWhiteSpace(waVoteString))
                    {
                        waVoteString += " | ";
                    }
                    if (!string.IsNullOrWhiteSpace(scVote))
                    {
                        waVoteString += $"SC: {scVote}";
                    }
                    endoString = $"{endorsementCount.Split('.')[0]} endorsements |";
                }
                var officerPosition = await GetOfficerPositionAsync(name, region).ConfigureAwait(false);
                _responseBuilder.Success()
                    .WithTitle(fullname)
                    .WithUrl(nationUrl)
                    .WithThumbnailUrl(flagUrl)
                    .WithDescription($"{(populationdbl / 1000.0 < 1 ? populationdbl : populationdbl / 1000.0).ToString(_config.CultureInfo)} {(populationdbl / 1000.0 < 1 ? "million" : "billion")} {demonymplural} | " +
                    $"Last active {lastActivity}")
                    .WithField("Founded", $"{dateFounded:dd.MM.yyyy} ({founded})")
                    .WithField("Region", $"[{region}]({regionUrl})", true);
                if (!string.IsNullOrWhiteSpace(officerPosition))
                {
                    _responseBuilder.WithField("Regional Officer", officerPosition, true);
                }
                _responseBuilder.WithField("Resident Since", $"{dateJoined:dd.MM.yyyy} ({(residencyYears < 1 ? "" : $"{residencyYears} y ")}{residencyDays} d)", string.IsNullOrWhiteSpace(officerPosition));
                _responseBuilder.WithField(category, $"C: {civilStr} ({civilRights}) | E: {economyStr} ({economy}) | P: {politicalStr} ({politicalFreedom})")
                    .WithField(wa, $"{endoString} {influenceValue} Influence ({Influence})");
                if (!string.IsNullOrWhiteSpace(waVoteString))
                {
                    _responseBuilder.WithField("WA Vote", waVoteString);
                }
                _responseBuilder.WithField("Links", $"[Dispatches](https://www.nationstates.net/page=dispatches/nation={name})  |  [Cards Deck](https://www.nationstates.net/page=deck/nation={name})  |  [Challenge](https://www.nationstates.net/page=challenge?entity_name={name})")
                .WithDefaults(_config.Footer);
            }
            else
            {
                throw new InvalidOperationException("Expected Response to be XmlDocument but wasn't.");
            }
        }
    }
}