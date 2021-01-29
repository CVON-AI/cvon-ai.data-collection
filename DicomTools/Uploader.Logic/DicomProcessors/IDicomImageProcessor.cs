
namespace Uploader.Logic.DicomProcessors
{
    using System.Collections.Generic;

    using Dicom;
    
    /// <summary>
    /// The image validator interface. Implementations will 
    /// verify validity of the given image by it's name or content
    /// and based on the clinical trial a registration has created for
    /// and the uploaded file is attached to.
    /// </summary>
    internal interface IDicomImageProcessor
    {
        /// <summary>
        /// Gets a value indicating whether validation should be
        /// stopped after validation fails on the current validator.
        /// </summary>
        bool StopValidationOnFailure { get; }

        /// <summary>
        /// Verifies whether the images meets certain requirements.
        /// </summary>
        /// <param name="imageFileName">
        /// The image file name.
        /// </param>
        /// <param name="file">
        /// The DICOM file, created based on the uploaded bytes.
        /// </param>
        /// 
        /// <returns>
        /// The <see cref="BinaryResult"/>.
        /// </returns>
        BinaryResult Validate(string imageFileName, DicomFile file);

        /// <summary>
        /// Processes the images meets certain requirements.
        /// </summary>
        /// <param name="imageFileName">
        /// The image file name.
        /// </param>
        /// <param name="file">
        /// The DICOM file, created based on the uploaded bytes.
        /// </param>
        /// 
        /// <returns>
        /// The <see cref="BinaryResult"/>.
        /// </returns>
        BinaryResult ApplyCorrection(string imageFileName, DicomFile file);


    }
}
