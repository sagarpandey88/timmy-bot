namespace Hex.Bot.Web.Models
{

    public class IActionableEntity
    {
        public ActionType ActionType { get; set; }
    }


    public class UploadDocAction : IActionableEntity
    {

        public string DocumentLibraryName { get; set; }
        public string SiteUrl { get; set; }

    }
    public class GetDocAction : IActionableEntity
    {

        public string FileName { get; set; }
        public string SiteUrl { get; set; }

    }

    public class SummarizeAction : IActionableEntity
    {

        public string Summary { get; set; }


    }


    public enum ActionType
    {
        UploadDoc,
        GetDoc,
        Summarize
    }

}
