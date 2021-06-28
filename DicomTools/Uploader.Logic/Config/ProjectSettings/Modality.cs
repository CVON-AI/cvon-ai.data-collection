using System.Text.Json.Serialization;

namespace Uploader.Logic.Config
{
    /// <summary>
    /// Defines all available data modalities for a project
    /// </summary>
    /// TODO: make imaging modality, add StructuredDataModality
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Modality
    {
        None = 0,

        CT = 1,

        MRI = 2,

        ClinicalData = 3,

        Echo = 4,

        Ecg = 5
    }
}