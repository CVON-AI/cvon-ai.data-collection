// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageValidatorFactory.cs" company="UMCG - Thoraxcentrum">
//   (c) 2014, UMCG - Thoraxcentrum
// </copyright>
// <summary>
//   Is responsible for supplying a set of <see cref="IImageValidator" />
//   instances to the application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Uploader.Logic.DicomProcessors
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    using NLog;
    using Uploader.Logic.Config;

    /// <summary>
    /// Is responsible for supplying a set of <see cref="IDicomImageProcessor"/>
    /// instances to the application.
    /// </summary>
    internal class ImageValidatorFactory : IImageValidatorFactory
    {
        /// <summary>
        /// Returns all available image validators.
        /// </summary>
        /// <returns>
        /// The collection of all available image validators.
        /// </returns>
        public IEnumerable<IDicomImageProcessor> GetAll(Site siteSettings, Modality currentModality, bool dicomDirIndexFile = false)
        {
            // TODO: implement configuration from other project
            // TODO: implement modality based config, separate for CT, MR, US, etc...
            List<IDicomImageProcessor> result = new List<IDicomImageProcessor>
            {
                new DicomFileValidator(siteSettings, currentModality),
                new DicomFileTrialBasedValidator(siteSettings, currentModality),
                new PixelBlinder(siteSettings, currentModality)
            };

            // only add modality specific validators for non-DICOMDIR files
            if (!dicomDirIndexFile)
            {
                switch (currentModality)
                {
                    case Modality.Echo:
                        result.Add(new DicomUltrasoundValidator(siteSettings, currentModality));
                        result.Add(new PixelBlinder(siteSettings, currentModality));
                        break;

                    case Modality.CT:
                    case Modality.MRI:
                        result.Add(new DicomUltrasoundValidator(siteSettings, currentModality));
                        result.Add(new PixelBlinder(siteSettings, currentModality));
                        break;
                }
            }
            //ApplicationConfiguration configuration = ApplicationConfigurationHandler<ApplicationConfiguration>.ImplementedInstance;

            switch (currentModality)
            {
                case Modality.Echo:
                    result.Add(new DicomUltrasoundValidator(siteSettings, currentModality));
                    break;

                    // TODO: ADD MRI and CT
            }

            return result;
        }
    }
}
