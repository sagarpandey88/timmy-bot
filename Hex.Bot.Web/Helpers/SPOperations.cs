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


        public List<string> GetRelatedFiles(string siteUrl, string searchText)
        {
            var libraryTitle = "customerinfo";
            // use search to get list of files
            var context = GetContext(siteUrl);
            var library = context.Web.Lists.GetByTitle(libraryTitle);
            var items = library.GetItems(CamlQuery.CreateAllItemsQuery());
            context.Load(context.Web, x => x.Url, x => x.ServerRelativeUrl);
            context.Load(items, i => i.Include(y => y.DisplayName,
                                          y => y.File,
                                        y => y.File.ServerRelativeUrl
                                        ));
            context.ExecuteQuery();

            var urls = items.ToList().Where(x => x.DisplayName.ToLower().Contains(searchText)).Select(x => x.File.ServerRelativeUrl).ToList();

            urls = urls.Select(x => context.Web.Url + x.Replace(context.Web.ServerRelativeUrl, "")).ToList();

            return urls;

        }

        public string UploadFile(string siteUrl, string libraryTitle, string fileName, byte[] fileContent)
        {
            //upload the file here.
            var context = GetContext(siteUrl);
            var library = context.Web.Lists.GetByTitle(libraryTitle);
            context.Load(library.RootFolder);
            context.ExecuteQuery();

            var folder = library.RootFolder;

            var fileCreationInfo = new FileCreationInformation
            {
                ContentStream = new MemoryStream(fileContent),
                Overwrite = true,
                Url = fileName

            };

            var uploadFile = folder.Files.Add(fileCreationInfo);
            context.Load(uploadFile);
            context.ExecuteQuery();

            ChunkAndUpload(fileContent, uploadFile, library);

            return uploadFile.ServerRelativeUrl;

        }




        protected void ChunkAndUpload(byte[] fileContent, Microsoft.SharePoint.Client.File uploadFile, Microsoft.SharePoint.Client.List library)
        {
            Guid uploadId = Guid.NewGuid();

            var fileSize = fileContent.Length;
            var blockSize = 1024 * 1024 * 2; // 2MB
            var buffer = new byte[blockSize];
            var bytesUploaded = 0;
            var fileUrl = uploadFile.ServerRelativeUrl;

            using (var fs = new MemoryStream(fileContent))
            {
                while (bytesUploaded < fileSize)
                {
                    var bytesToRead = Math.Min(blockSize, fileSize - bytesUploaded);
                    fs.Read(buffer, 0, bytesToRead);

                    using (var binaryReader = new BinaryReader(new MemoryStream(buffer)))
                    {
                        var binaryContent = binaryReader.ReadBytes(bytesToRead);
                        if (bytesUploaded == 0)
                        {
                            uploadFile = library.RootFolder.Files.GetByUrl(fileUrl);
                            uploadFile.StartUploadFile(uploadId, new MemoryStream(binaryContent));
                        }
                        else
                        {
                            uploadFile.ContinueUpload(uploadId, bytesUploaded, new MemoryStream(binaryContent));
                        }
                        bytesUploaded += bytesToRead;
                    }
                }
                uploadFile = library.RootFolder.Files.GetByUrl(fileUrl);
                uploadFile.FinishUpload(uploadId, bytesUploaded, null);

            }
        }
    }

}
