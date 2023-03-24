using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hex.Bot.API.Processors
{
    public class FileOperationProcessor
    {

        private ITurnContext TurnContext { get; set; }
        public FileOperationProcessor(ITurnContext turnContext)
        {

            TurnContext = turnContext;
        }

        public void ProcessFile()
        {


        }

        public void UploadDocument()
        {

        }
    }
}
