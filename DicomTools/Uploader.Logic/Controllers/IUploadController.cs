namespace Uploader.Logic.Controllers
{
    internal interface  IUploadController
    {
        bool ProcessDirectory(string path);
    }
}