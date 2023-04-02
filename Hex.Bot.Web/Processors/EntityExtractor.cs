using Azure;
using Azure.AI.Language.Conversations;
using Azure.AI.TextAnalytics;
using Azure.Core;
using Hex.Bot.Web.Helpers;
using Hex.Bot.Web.Models;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using System.Text.Json;

namespace Hex.Bot.Web.Processors
{
    public class EntityExtractor
    {

        private readonly IConfiguration Configuration;
        private string AzureKey { get; set; }
        private string LanguageCognitiveServicesEndPoint { get; set; }

        public EntityExtractor(IConfiguration configuration)
        {
            Configuration = configuration;
            this.AzureKey = Configuration["CognitiveServiceKey"];
            this.LanguageCognitiveServicesEndPoint = Configuration["LanguageServiceEndpoint"];

        }

        public IActionableEntity Extract(string userText)
        {
            AzureKeyCredential credentials = new AzureKeyCredential(AzureKey);
            Uri endpoint = new Uri(LanguageCognitiveServicesEndPoint);
            //   var client = new TextAnalyticsClient(endpoint, credentials);
            //var response = client.RecognizeEntities(text);
            //CategorizedEntity categorizedEntity = response.Value.First();
            //return categorizedEntity.Category.ToString();

            if (userText.Length > 200)
            {
                return new SummarizeAction { Summary = TextSummarizer.SummarizeText(userText.Split(":")[1], Configuration).Result, ActionType = ActionType.Summarize };
            }

            ConversationAnalysisClient client = new ConversationAnalysisClient(endpoint, credentials);

            string projectName = "docactions";
            string deploymentName = "v2";

            var data = new
            {
                analysisInput = new
                {
                    conversationItem = new
                    {
                        text = userText,
                        id = "1",
                        participantId = "1",
                    }
                },
                parameters = new
                {
                    projectName,
                    deploymentName,

                    // Use Utf16CodeUnit for strings in .NET.
                    stringIndexType = "Utf16CodeUnit",
                },
                kind = "Conversation",
            };

            Response response = client.AnalyzeConversation(RequestContent.Create(data));

            using JsonDocument result = JsonDocument.Parse(response.ContentStream);
            JsonElement conversationalTaskResult = result.RootElement;
            JsonElement conversationPrediction = conversationalTaskResult.GetProperty("result").GetProperty("prediction");
            JsonElement intents = conversationPrediction.GetProperty("intents");
            JsonElement entities = conversationPrediction.GetProperty("entities");

            List<EntityItem> entityList = JsonConvert.DeserializeObject<List<EntityItem>>(entities.ToString());

            EntityItem et = entityList.Where(x => x.category == "Action").First();
            switch (et.text.ToLower())
            {
                case "upload":
                case "push":

                    EntityItem docLibe = entityList.Where(x => x.category == "DocumentLibrary").FirstOrDefault();
                    return new UploadDocAction { DocumentLibraryName = docLibe.text, ActionType = ActionType.UploadDoc };


                case "get":
                case "search":
                case "find":
                    EntityItem fileName = entityList.Where(x => x.category == "FileName").FirstOrDefault();
                    return new GetDocAction { FileName = fileName.text, ActionType = ActionType.GetDoc };
                case "summarize":
                    return new SummarizeAction { Summary = TextSummarizer.SummarizeText(userText, Configuration).Result, ActionType = ActionType.Summarize };

            }

            return new IActionableEntity();



        }
    }

    public class EntityItem
    {
        public string category { get; set; }
        public string text { get; set; }


    }
}
