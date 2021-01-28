using System.Collections.Generic;

namespace Uploader.Logic.Config
{
    public class ImageModalityConfiguration : ModalityConfigurationBase, IModalityConfiguration
    {
        /// <summary>
        /// Gets or sets a list of <see cref="PixelArea"/> instances that need to be blinded 
        /// during export. This should be done ahead of export, since during export
        /// areas inside of the image, if not the whole, may contain identifying patient
        /// information that needs to be blinded.
        /// </summary>
        public IEnumerable<PixelArea> BlindedAreas { get; set; } = new List<PixelArea>();

        /// <summary>
        /// Gets or sets an optional set of <see cref="PixelArea"/> instances that
        /// need to be exported. In case none is set, the whole source image should be
        /// exported.
        /// </summary>
        public IEnumerable<PixelArea> ExportedAreas { get; set; } = new List<PixelArea>();

        public IEnumerable<DicomTagConfiguration> DicomTags { get; set; } = new List<DicomTagConfiguration>();
    }
}