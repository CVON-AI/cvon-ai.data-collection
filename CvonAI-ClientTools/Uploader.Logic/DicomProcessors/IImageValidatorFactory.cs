// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IImageValidatorFactory.cs" company="UMCG - Thoraxcentrum">
//   (c) 2014, UMCG - Thoraxcentrum
// </copyright>
// <summary>
//   Is responsible for supplying a set of <see cref="IImageValidator" />
//   instances to the application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Uploader.Logic.DicomProcessors
{
    using System.Collections.Generic;
    using Uploader.Logic.Config;

    /// <summary>
    /// Is responsible for supplying a set of <see cref="IDicomImageProcessor"/>
    /// instances to the application.
    /// </summary>
    internal interface IImageValidatorFactory
    {
        /// <summary>
        /// Returns all available image validators.
        /// </summary>
        /// <returns>
        /// The collection of all available image validators.
        /// </returns>
        IEnumerable<IDicomImageProcessor> GetAll(Site siteSettings, Modality currentModality, bool dicomDirIndexFile = false);
    }
}
