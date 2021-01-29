// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DicomFileValidator.cs" company="UMCG - Thoraxcentrum">
//   (c) 2014, UMCG - Thoraxcentrum
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Uploader.Logic.DicomProcessors
{
    using Dicom;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Uploader.Logic.Config;
    using Uploader.Logic.Providers;

    /// <summary>
    /// Verifies whether an uploaded file is a DICOM file.
    /// </summary>
    /// <remarks>
    /// This validator was previously used to verify whether an uploaded file
    /// is a DICOM file. Since DICOM file instantiation is now done before
    /// calling the validators, this makes no sence.</remarks>
    internal sealed class DicomFileValidator : DicomFileValidatorBase, IDicomImageProcessor
    {
        internal const string NotADicomFile = "Uploaded is not a DICOM file";

        internal DicomFileValidator(Site site, Modality modality) : base(site, modality)
        {
        }

        public override bool StopValidationOnFailure
        {
            get
            {
                // todo: configurable?
                return true;
            }
        }

        /// <summary>
        /// Verifies whether the images meets certain requirements.
        /// </summary>
        /// <param name="registration">
        /// The registration the image has been uploaded for.
        /// </param>
        /// <param name="imageFileName">
        /// The image file name.
        /// </param>
        /// <param name="file">
        /// The DICOM file, created based on the uploaded bytes.
        /// </param>
        /// <param name="imageContents">The uploaded bytes.</param>
        /// <returns>
        /// The <see cref="BinaryResult"/>.
        /// </returns>
        public override BinaryResult Validate(string imageFileName, DicomFile file)
        {
            if (file == null)
            {
                string technicalInfo = "File content is missing";

                return new BinaryResult(false, $"Filedata in '{imageFileName}' was not complete or not DICOM", technicalInfo);
            }

            try
            {
                // todo: add basic DICOM tag validations?
            }
            catch (Exception ex)
            {
                string technicalInformation =
                    $"{NotADicomFile}. The following exception occurred while attempting to read the file as a DICOM file\r\n{ex.Message}\r\n{ex.StackTrace}";
                return new BinaryResult(false, NotADicomFile, technicalInformation);
            }

            return BinaryResult.Positive;
        }

        public override BinaryResult ApplyCorrection(string imageFileName, DicomFile file)
        {
            return base.MoveIfInvalid(imageFileName, file);
        }

    }
}