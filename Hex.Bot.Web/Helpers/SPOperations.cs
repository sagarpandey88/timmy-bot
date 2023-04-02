using Microsoft.SharePoint.Client;
using PnP.Framework;
using System.Configuration;

namespace Hex.Bot.Web.Helpers
{

    public class SPOperations
    {
        private string ClientId { get; set; }
        private string ClientSecret { get; set; }

        private readonly IConfiguration Configuration;

        public SPOperations(IConfiguration configuration)
        {
            Configuration = configuration;
            this.ClientId = Configuration["SPClientId"];
            this.ClientSecret = Configuration["SPClientSecret"];
        }

        protected ClientContext GetContext(string siteUrl)
        {


            ClientContext ctx = new AuthenticationManager().GetACSAppOnlyContext(siteUrl, ClientId, ClientSecret);
            //ctx.Load(ctx.Web);
            //ctx.ExecuteQuery();
            return ctx;

        }


        public void GetRelatedFiles(string siteUrl, string searchText)
        {

            // use search to get list of files
        }

        public void UploadFile(string siteUrl, string libraryTitle, byte[] file)
        {
            //upload the file here.
        }
    }
}
