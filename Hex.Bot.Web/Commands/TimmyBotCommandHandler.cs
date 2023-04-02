using Hex.Bot.Web.Models;
using AdaptiveCards.Templating;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.TeamsFx.Conversation;
using Newtonsoft.Json;
using Hex.Bot.Web.Processors;
using Hex.Bot.Web.Helpers;
using System.Net;

namespace Hex.Bot.Web.Commands
{
    /// <summary>
    /// The <see cref="TimmyBotCommandHandler"/> registers a pattern with the <see cref="ITeamsCommandHandler"/> and 
    /// responds with an Adaptive Card if the user types the <see cref="TriggerPatterns"/>.
    /// </summary>
    public class TimmyBotCommandHandler : ITeamsCommandHandler
    {
        private readonly ILogger<TimmyBotCommandHandler> _logger;
        private readonly string _adaptiveCardFilePath = Path.Combine(".", "Resources", "HelloWorldCard.json");
        private readonly IConfiguration Configuration;

        public IEnumerable<ITriggerPattern> TriggerPatterns => new List<ITriggerPattern>
        {
            // Used to trigger the command handler if the command text contains 'helloWorld'
            new RegExpTrigger(".")
        };

        public TimmyBotCommandHandler(ILogger<TimmyBotCommandHandler> logger, IConfiguration config)
        {
            _logger = logger;
            Configuration = config;

        }

        public static byte[] LoadFileByteFromUrl(string fileUrl)
        {
            using (var client = new WebClient())
            {
                return client.DownloadData(fileUrl);
            }
        }

        public async Task<ICommandResponse> HandleCommandAsync(ITurnContext turnContext, CommandMessage message, CancellationToken cancellationToken = default)
        {
            _logger?.LogInformation($"Bot received message: {message.Text}");
            string repoSite = "https://spmpney.sharepoint.com/sites/Timmybot/";

            EntityExtractor et = new EntityExtractor(Configuration);
            IActionableEntity entity = et.Extract(message.Text);
            string response = string.Empty;
            switch (entity.ActionType)
            {
                case ActionType.UploadDoc:
                    Attachment att = turnContext.Activity.Attachments.FirstOrDefault();
                    dynamic attContent = JsonConvert.DeserializeObject(att.Content.ToString());
                    UploadDocAction uploadDocAction = (UploadDocAction)entity;
                    response = "File Uploaded , here is the link : " + new SPOperations(Configuration).UploadFile(repoSite, uploadDocAction.DocumentLibraryName, att.Name, LoadFileByteFromUrl(((dynamic)attContent).downloadUrl.ToString()));
                    break;

                case ActionType.GetDoc:
                    GetDocAction getDocAction = (GetDocAction)entity;
                    response = string.Join(",", new SPOperations(Configuration).GetRelatedFiles(repoSite, getDocAction.FileName));
                    break;
                case ActionType.Summarize:
                    SummarizeAction summarizeAction = (SummarizeAction)entity;
                    response = summarizeAction.Summary;
                    break;
            }




            // Read adaptive card template
            //var cardTemplate = await File.ReadAllTextAsync(_adaptiveCardFilePath, cancellationToken);

            ////// Render adaptive card content
            //var cardContent = new AdaptiveCardTemplate(cardTemplate).Expand
            //(
            //    new HelloWorldModel
            //    {
            //        Title = "Your Hello World Bot is Running",
            //        Body = "Congratulations! Your hello world bot is running. Click the documentation below to learn more about Bots and the Teams Toolkit.",
            //    }
            //);

            //// Build attachment
            //var activity = MessageFactory.Attachment
            //(
            //    new Attachment
            //    {
            //        ContentType = "application/vnd.microsoft.card.adaptive",
            //        Content = JsonConvert.DeserializeObject(cardContent),
            //    }
            //);

            // send response
            return new ActivityCommandResponse(MessageFactory.Text(response));
        }
    }
}
