using System.Collections.Generic;

namespace Uploader.Logic.Config
{
    /// <summary>
    /// Describes all settings for a site, inside a project
    /// </summary>
    public class Site
    {
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is the current site on a single site location.
        /// </summary>
        public bool IsCurrent { get; set; }

        /// <summary>
        /// Gets the site's modality configuration. Ideally, this enumerable should not contain Configurations
        /// of <see cref="Modality"/> that are not selected in the <see cref="ProjectSettings.Modalities"/>
        /// </summary>
        public IEnumerable<StructuredModalityConfiguration> StructuredModalityConfig { get; set; } = new List<StructuredModalityConfiguration>();

        public IEnumerable<ImageModalityConfiguration> ImageModalityConfig { get; set; } = new List<ImageModalityConfiguration>();
    }
}