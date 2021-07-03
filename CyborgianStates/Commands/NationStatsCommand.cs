﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CyborgianStates.CommandHandling;
using CyborgianStates.Enums;
using CyborgianStates.Interfaces;
using CyborgianStates.MessageHandling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NationStatesSharp;
using NationStatesSharp.Enums;
using Serilog;
using ILogger = Serilog.ILogger;
using IRequestDispatcher = NationStatesSharp.Interfaces.IRequestDispatcher;

namespace CyborgianStates.Commands
{
    public class NationStatsCommand : ICommand
    {
        private readonly AppSettings _config;
        private readonly IRequestDispatcher _dispatcher;
        private readonly IResponseBuilder _responseBuilder;
        private readonly ILogger _logger;
        private CancellationToken token;

        public NationStatsCommand() : this(Program.ServiceProvider)
        {
        }

        public NationStatsCommand(IServiceProvider serviceProvider)
        {
            _logger = Log.ForContext<NationStatsCommand>();
            _dispatcher = serviceProvider.GetRequiredService<IRequestDispatcher>();
            _config = serviceProvider.GetRequiredService<IOptions<AppSettings>>().Value;
            _responseBuilder = serviceProvider.GetRequiredService<IResponseBuilder>();
        }

        public async Task<CommandResponse> Execute(Message message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            try
            {
                _logger.Debug(message.Content);
                var parameters = message.Content.Split(" ").Skip(1);
                if (parameters.Any())
                {
                    string nationName = Helpers.ToID(string.Join(" ", parameters));
                    var request = new Request($"nation={nationName}&q=flag+wa+gavote+scvote+fullname+freedom+demonym2plural+category+population+region+founded+foundedtime+influence+lastactivity+census;mode=score;scale=0+1+2+65+66+80", ResponseFormat.Xml);
                    _dispatcher.Dispatch(request, 0);
                    await request.WaitForResponseAsync(token).ConfigureAwait(false);
                    await ProcessResultAsync(request, nationName).ConfigureAwait(false);
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
                _logger.Error(e.ToString());
                return await FailCommandAsync(message, "Request/Command has been canceled. Sorry :(").ConfigureAwait(false);
            }
            catch (HttpRequestFailedException e)
            {
                _logger.Error(e.ToString());
                return await FailCommandAsync(message, e.Message).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.Error(e.ToString());
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
            var request = new Request($"region={Helpers.ToID(regionName)}&q=officers", ResponseFormat.Xml);
            _dispatcher.Dispatch(request, 0);
            await request.WaitForResponseAsync(token).ConfigureAwait(false);
            var doc = request.GetResponseAsXml();

            var list = doc.GetParentsOfFilteredDescendants("NATION", nationName);
            if (list.Any())
            {
                return list.First().Element("OFFICE")?.Value;
            }
            else
            {
                return string.Empty;
            }
        }

        private async Task ProcessResultAsync(Request request, string nationName)
        {
            var response = request.GetResponseAsXml();
            string demonymplural = response.GetFirstValueByNodeName("DEMONYM2PLURAL");
            string category = response.GetFirstValueByNodeName("CATEGORY");
            string flagUrl = response.GetFirstValueByNodeName("FLAG");
            string fullname = response.GetFirstValueByNodeName("FULLNAME");
            string population = response.GetFirstValueByNodeName("POPULATION");
            string region = response.GetFirstValueByNodeName("REGION");
            string founded = response.GetFirstValueByNodeName("FOUNDED");
            string foundedtime = response.GetFirstValueByNodeName("FOUNDEDTIME");
            var dateFounded = DateTimeOffset.FromUnixTimeSeconds(long.Parse(foundedtime)).UtcDateTime;
            string lastActivity = response.GetFirstValueByNodeName("LASTACTIVITY");
            string Influence = response.GetFirstValueByNodeName("INFLUENCE");
            string wa = response.GetFirstValueByNodeName("UNSTATUS");
            var freedom = response.Descendants("FREEDOM").Elements();

            string civilStr = freedom.ElementAtOrDefault(0)?.Value;
            string economyStr = freedom.ElementAtOrDefault(1)?.Value;
            string politicalStr = freedom.ElementAtOrDefault(2)?.Value;

            var census = response.Descendants("CENSUS").Elements();
            string civilRights = census.ElementAtOrDefault(0)?.Element("SCORE")?.Value;
            string economy = census.ElementAtOrDefault(1)?.Element("SCORE")?.Value;
            string politicalFreedom = census.ElementAtOrDefault(2)?.Element("SCORE")?.Value;
            string influenceValue = census.ElementAtOrDefault(3)?.Element("SCORE")?.Value;
            string endorsementCount = census.ElementAtOrDefault(4)?.Element("SCORE")?.Value;
            string residency = census.ElementAtOrDefault(5)?.Element("SCORE")?.Value;

            double residencyDbl = Convert.ToDouble(residency, _config.CultureInfo);
            var dateJoined = DateTime.UtcNow.Subtract(TimeSpan.FromDays(residencyDbl));
            int residencyYears = (int) (residencyDbl / 365.242199);
            int residencyDays = (int) (residencyDbl % 365.242199);
            double populationdbl = Convert.ToDouble(population, _config.CultureInfo);
            string nationUrl = $"https://www.nationstates.net/nation={Helpers.ToID(nationName)}";
            string regionUrl = $"https://www.nationstates.net/region={Helpers.ToID(region)}";
            string waVoteString = "";
            string endoString = "";
            if (wa == "WA Member")
            {
                var gaVote = response.GetFirstValueByNodeName("GAVOTE");
                var scVote = response.GetFirstValueByNodeName("SCVOTE");
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
            var officerPosition = await GetOfficerPositionAsync(nationName, region).ConfigureAwait(false);
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
            _responseBuilder.WithField("Links", $"[Dispatches](https://www.nationstates.net/page=dispatches/nation={nationName})  |  [Cards Deck](https://www.nationstates.net/page=deck/nation={nationName})  |  [Challenge](https://www.nationstates.net/page=challenge?entity_name={nationName})")
            .WithDefaults(_config.Footer);
        }
    }
}