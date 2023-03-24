using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hex.Bot.API.Helpers
{
    public class AdapterWithErrorHandler : BotFrameworkAdapter
    {
        public AdapterWithErrorHandler(ICredentialProvider credentialProvider,
            ILogger<AdapterWithErrorHandler> logger,
            ConversationState conversationState)
            : base(credentialProvider, logger: logger)
        {


            this.UseBotState(conversationState);

            this.OnTurnError = async (turnContext, exception) =>
            {
                if (exception != null)
                {
                    // Log any leaked exception from the application.
                    logger.LogError($"Exception caught : {exception.ToString()}");
                }

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync("Sorry, it looks like something went wrong.");

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Exception caught on attempting to Delete ConversationState : {e.Message}");
                    }
                }
            };
        }

    }
}
