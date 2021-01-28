using System;
using System.Text.Json.Serialization;

namespace Uploader.Logic.Config
{
    public interface IModalityConfiguration
    {
        /// <summary>
        /// Gets or sets the modality for the configuration
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        Modality Modality { get; set; }

        /// <summary>
        /// Gets or sets the local or network path to the directory 
        /// structure that may contain the the exported data that 
        /// needs to be processed by the uploader.
        /// </summary>
        string DataExportPath { get; set; }

        /// <summary>
        /// Gets or sets the remote path where the images need
        /// to be exported to
        /// </summary>
        string RemotePath { get; set; }

        /// <summary>
        /// Gets or sets the type that will be used to 
        /// process the data during upload
        /// </summary>
        Type UploadControllerType { get; set; }
    }
}