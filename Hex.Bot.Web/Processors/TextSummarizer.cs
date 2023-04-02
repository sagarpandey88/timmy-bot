using Azure;
using Azure.AI.TextAnalytics;
using System.Text;

namespace Hex.Bot.Web.Processors
{
    public class TextSummarizer
    {

        public static async Task<string> SummarizeText(string document, IConfiguration Configuration)
        {
            string key = Configuration["CognitiveServiceKey"];
            string languageEndPoint = Configuration["LanguageServiceEndpoint"];
            AzureKeyCredential credentials = new AzureKeyCredential(key);
            Uri endpoint = new Uri(languageEndPoint);

            var client = new TextAnalyticsClient(endpoint, credentials);
            return await TextSummarizationExample(client, document);
        }

        private static async Task<string> TextSummarizationExample(TextAnalyticsClient client, string document)
        {


            // Prepare analyze operation input. You can add multiple documents to this list and perform the same
            // operation to all of them.
            var batchInput = new List<string>
            {
                document
            };

            TextAnalyticsActions actions = new TextAnalyticsActions()
            {

                ExtractSummaryActions = new List<ExtractSummaryAction>() { new ExtractSummaryAction() }
            };

            // Start analysis process.
            AnalyzeActionsOperation operation = await client.StartAnalyzeActionsAsync(batchInput, actions);
            await operation.WaitForCompletionAsync();
            // View operation status.


            Console.WriteLine();
            // View operation results.

            StringBuilder sb = new StringBuilder();

            await foreach (AnalyzeActionsResult documentsInPage in operation.Value)
            {
                IReadOnlyCollection<ExtractSummaryActionResult> summaryResults = documentsInPage.ExtractSummaryResults;

                foreach (ExtractSummaryActionResult summaryActionResults in summaryResults)
                {
                    if (summaryActionResults.HasError)
                    {
                        sb.Append("Error Processing this data");

                        continue;
                    }

                    foreach (ExtractSummaryResult documentResults in summaryActionResults.DocumentsResults)
                    {
                        if (documentResults.HasError)
                        {
                            sb.Append("Error Processing this data");
                            continue;
                        }

                        foreach (SummarySentence sentence in documentResults.Sentences)
                        {
                            Console.WriteLine($"  Sentence: {sentence.Text}");
                            Console.WriteLine();

                            sb.Append(sentence.Text);
                        }
                    }
                }


            }
            return sb.ToString();
        }


    }
}
