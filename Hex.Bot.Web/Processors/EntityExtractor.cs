using Azure;
using Azure.AI.Language.Conversations;
using Azure.AI.TextAnalytics;
using Azure.Core;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
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

        public string Extract(string text)
        {
            AzureKeyCredential credentials = new AzureKeyCredential(AzureKey);
            Uri endpoint = new Uri(LanguageCognitiveServicesEndPoint);
            //   var client = new TextAnalyticsClient(endpoint, credentials);
            //var response = client.RecognizeEntities(text);
            //CategorizedEntity categorizedEntity = response.Value.First();
            //return categorizedEntity.Category.ToString();

            ConversationAnalysisClient client = new ConversationAnalysisClient(endpoint, credentials);

            string projectName = "docactions";
            string deploymentName = "v2";

            var data = new
            {
                analysisInput = new
                {
                    conversationItem = new
                    {
                        text = text,
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

            // if(entities.)


            //foreach (var entity in response.Value)
            //{
            //    Console.WriteLine($"\tText: {entity.Text}\tCategory: {entity.Category}\tSub-Category: {entity.SubCategory}");
            //    Console.WriteLine($"\t\tScore: {entity.ConfidenceScore:F2}\tLength: {entity.Length}\tOffset: {entity.Offset}\n");
            //}
            return "";
        }
    }
}
