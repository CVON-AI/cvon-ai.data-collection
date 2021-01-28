using System;
using System.Collections.Generic;
using System.Linq;
using Uploader.Logic.Config;
using Uploader.Logic.Controllers;
using Uploader.Logic.DicomProcessors;

namespace Uploader.Logic.Actions
{
    internal class LocalDirCorrector : ActionBase, IAction
    {
        public LocalDirCorrector(Site siteConfiguration, Modality modality) : base(siteConfiguration, modality)
        {
        }

        protected override void DoPerform(Dictionary<string, List<string>> paths)
        {
            var result = ImageValidatorWrapper.CorrectFiles(this.SiteSettings, this.Modality, paths.SelectMany(p => p.Value));

            if (result.Value)
            {
                new IOControllerWrapper().WriteLine("All DICOMS were valid");
            }
            else
            {
                new IOControllerWrapper().WriteLine("Following validation errors occurred reading DICOM files:");
                new IOControllerWrapper().WriteLine(result.TechnicalInformation);
            }
        }
    }
}