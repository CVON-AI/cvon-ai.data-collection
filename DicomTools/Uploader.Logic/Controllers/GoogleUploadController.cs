using System.IO;

namespace Uploader.Logic.Controllers
{
    internal class GoogleUploadController : IUploadController
    {
        public bool ProcessDirectory(string path)
        {

            foreach (string file in Directory.GetFiles(path))
            {
                
            }

            return true;
        }
    }
}
