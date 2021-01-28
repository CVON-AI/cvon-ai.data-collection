using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Uploader.Logic.Config
{
    /// <summary>
    /// Describes the settings of a project
    /// </summary>
    public class ProjectSettings
    {
        /// <summary>
        /// Gets or sets all possible sites
        /// </summary>
        public IEnumerable<Site> Sites { get; set; } = new List<Site>();

        /// <summary>
        /// Gets or sets a collection of all available modalities
        /// </summary>
        //[JsonConverter(typeof(JsonStringEnumConverter))]
        public IEnumerable<Modality> Modalities { get; set; } = new List<Modality>();

        /// <summary>
        /// Gets or sets the global remote project root
        /// </summary>
        public string RemotePath { get; set; }
        public string Name { get; set; }
    }
}
