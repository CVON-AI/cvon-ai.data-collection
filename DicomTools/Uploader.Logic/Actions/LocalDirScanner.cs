using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Uploader.Logic.Config;
using Uploader.Logic.Controllers;

namespace Uploader.Logic.Actions
{
    // todo: add sync function
    // todo: add validators
    // todo: add export to file option
    // todo: add fix function!
    // todo: add validators/fixers  for tag scanning (patient name, id, DoB, etc, structured reports, blinding pixel data, 
    // todo: add DICOM tag and pixel extractor + db store!!!!
    // todo: add a visualcheck for images!!!

    internal class LocalDirScanner : ActionBase, IAction
    {
        public LocalDirScanner(Site siteSettings, Modality modality) : base(siteSettings, modality)
        {
        }

        protected override void DoPerform(Dictionary<string, List<string>> paths)
        {
            string pathPrefix = new string(' ', 4);
            IIOController ioController = DependencyInjector.Resolve<IIOController>() ?? new IOControllerWrapper();

            foreach (string path in paths.Keys)
            {
                ioController.WriteLine(path);
                foreach (string filename in paths[path])
                {
                    FileInfo info = new FileInfo(filename);
                    ioController.WriteLine("{0}{1}", pathPrefix, info.Name);
                }
            }
        }

        private void ShowPath(string localPath, int depth = 0)
        {
            IIOController ioController = DependencyInjector.Resolve<IIOController>() ?? new IOControllerWrapper();
            string pathPrefix = new string(' ', depth * 4);
            ioController.WriteLine(localPath);
         
            foreach (string filename in Directory.EnumerateFiles(localPath))
            {
                FileInfo info = new FileInfo(filename);
                ioController.WriteLine("{0}{1}", pathPrefix, info.Name);
            }

            foreach (string directoryName in Directory.EnumerateDirectories(localPath))
            {
                ShowPath(directoryName, depth + 1);
            }
        }
    }
}