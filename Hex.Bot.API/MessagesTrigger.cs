using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Bot.Schema;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using System.Security.Claims;

namespace Hex.Bot.API
{
    public class MessagesTrigger
    {
        private readonly BotFrameworkAdapter Adapter;
        private readonly IBot Bot;


        public MessagesTrigger(BotFrameworkAdapter adapter, IBot bot)
        {
            Adapter = adapter;
            Bot = bot;

        }


        [FunctionName("messages")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log, CancellationToken hostCancellationToken)
        {

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Microsoft.Bot.Schema.Activity activity = JsonConvert.DeserializeObject<Microsoft.Bot.Schema.Activity>(requestBody);

            log.LogInformation("C# HTTP trigger function processed a request.");

            var authHeader =
               String.Join(' ',
                   req.Headers
                   .Where(x => (x.Key.ToUpper() == "AUTHORIZATION"))
                   .Select(x => x.Value)
                   .SelectMany(x => x)
            );

            var ident = new ClaimsIdentity();
            ident.AddClaim(new Claim("Bearer", authHeader));

            await Adapter.ProcessActivityAsync(ident, activity, BotLogic, hostCancellationToken);



            // return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);

            return new AcceptedResult();// OkObjectResult(responseMessage);
        }

        async Task BotLogic(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {


                //call file operation
                if (turnContext.Activity.Attachments != null && turnContext.Activity.Attachments.Count > 0)
                {

                }


                var typingActivity = new Microsoft.Bot.Schema.Activity
                {
                    Type = ActivityTypes.Typing,
                    Conversation = turnContext.Activity.Conversation,
                    From = turnContext.Activity.Recipient,
                    Recipient = turnContext.Activity.From
                };
                await turnContext.SendActivitiesAsync(new IActivity[] { typingActivity });



                await turnContext.SendActivityAsync(MessageFactory.Text("hello" + turnContext.Activity.Text));

            }
        }
    }
}
