using Hex.Bot.API.Helpers;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

[assembly: FunctionsStartup(typeof(Hex.Bot.API.Startup))]
namespace Hex.Bot.API
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {

            builder.Services
           .AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>()
           .AddSingleton<IStorage, MemoryStorage>(
               (sp) => new MemoryStorage())
           .AddSingleton<UserState>()
           .AddSingleton<ConversationState>();

            // Create the Bot Framework Adapter with error handling enabled.
            builder.Services.AddSingleton<BotFrameworkAdapter, AdapterWithErrorHandler>();
            

            builder.Services
                .AddHttpClient("BotFramework", c =>
                {
                    c.BaseAddress = new Uri("https://directline.botframework.com/");
                });


            //// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            //  services.AddTransient<IBot, TeamsMessagingExtensionsActionBot>();            


            //// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            builder.Services.AddTransient<IBot, Hex.Bot.API.Bots.TeamsBot>();

        }
    }


}