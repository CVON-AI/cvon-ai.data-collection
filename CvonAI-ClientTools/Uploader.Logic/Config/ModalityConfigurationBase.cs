using System;
using System.Text.Json.Serialization;

namespace Uploader.Logic.Config
{
    /// <summary>
    /// Abstract baseclass for modality configurations
    /// </summary>
    public abstract class ModalityConfigurationBase
    {
        /// <summary>
        /// Gets or sets the modality for the configuration
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Modality Modality { get; set; }

        /// <summary>
        /// Gets or sets the local or network path to the directory 
        /// structure that may contain the the exported data that 
        /// needs to be processed by the uploader.
        /// </summary>
        public string DataExportPath { get; set; }

        /// <summary>
        /// Gets or sets the remote path where the images need
        /// to be exported to
        /// </summary>
        public string RemotePath { get; set; }

        /// <summary>
        /// Gets or sets the string representation for <see cref="UploadControllerType"/>
        /// which can be used for serialization of configuration files
        /// </summary>
        public string ControllerTypeString { get; set; }

        /// <summary>
        /// Gets or sets the type that will be used to 
        /// process the data during upload
        /// </summary>
        [JsonIgnore]
        public Type UploadControllerType
        {
            get
            {
                if (string.IsNullOrEmpty(this.ControllerTypeString))
                {
                    return null;
                }

                Type type = Type.GetType(this.ControllerTypeString);

                if (type != null &&
                    type.IsClass /*&&
                    type.Get.GetInterface("Thorax.DTO.FunctieOnderzoeken.IAanvraag, Thorax.DTO") != null*/)
                {
                    return type;
                }

                return null;
            }

            set
            {
                this.ControllerTypeString = value?.FullName;
            }
        }
    }
}