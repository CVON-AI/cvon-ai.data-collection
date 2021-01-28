using System;
using System.Collections.Generic;
using System.Linq;
using Uploader.Logic.Config;
using Uploader.Logic.Controllers;
using Uploader.Logic.DicomProcessors;

namespace Uploader.Logic.Actions
{
    internal class LocalDirValidator : ActionBase, IAction
    {
        public LocalDirValidator(Site siteConfiguration, Modality modality) : base(siteConfiguration, modality)
        {
        }

        protected override void DoPerform(Dictionary<string, List<string>> paths)
        {
            new IOControllerWrapper().WriteLine($"Validating local files in folders {string.Join(", ", paths.Keys)}");

            // todo, make a proper bridge to the wrapper, run it with all files!
            var result = ImageValidatorWrapper.Validate(this.SiteSettings, this.Modality, paths.SelectMany(p => p.Value));

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