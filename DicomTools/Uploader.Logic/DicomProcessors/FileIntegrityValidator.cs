// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileIntegrityValidator.cs" company="UMCG - Thoraxcentrum">
//   (c) 2014, UMCG - Thoraxcentrum
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Uploader.Logic.DicomProcessors
{
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;

    using Dicom;

    /// <summary>
    /// The file integrity validator.
    /// Validates a file's integrity by calculating the SHA-256 hash and comparing it 
    /// to a reference hash.
    /// </summary>
    internal sealed class FileIntegrityValidator : IImageValidator
    {
        public bool StopValidationOnFailure
        {
            get
            {
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
        /// <param name="propertyBag">
        /// A dictionary with custom variables,
        ///     which may be used by the implemented validator.
        /// </param>
        /// <returns>
        /// The <see cref="ValidationResult"/>.
        /// </returns>
        public ValidationResult Validate(string imageFileName, DicomFile file, byte[] imageContents, Dictionary<string, object> propertyBag)
        {
            string errorInfoMissingHash = "Uploaded filedata could not be verified at the server. Please contact GICL about this problem";

            if (propertyBag == null)
            {
                string technicalInfo = "Property bag is missing";

                return new ValidationResult(false, errorInfoMissingHash, technicalInfo);
            }

            if (!propertyBag.ContainsKey(ImageValidationPropertyKeys.ReferenceHash) || !(propertyBag[ImageValidationPropertyKeys.ReferenceHash] is string))
            {
                string technicalInfo = $"Property bag misses the key '{ImageValidationPropertyKeys.ReferenceHash}'";

                return new ValidationResult(false, errorInfoMissingHash, technicalInfo);
            }

            if (imageContents == null || imageContents.Length == 0)
            {
                string technicalInfo = "File content is missing";

                return new ValidationResult(false, "Uploaded filedata was not complete", technicalInfo);

            }

            SHA256 sha256 = SHA256.Create();

            byte[] binaryOutput = sha256.ComputeHash(imageContents);
            StringBuilder hex = new StringBuilder();
            foreach (byte b in binaryOutput)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            string hashString = hex.ToString();
            string referenceHash = propertyBag[ImageValidationPropertyKeys.ReferenceHash] as string;

            if (hashString != referenceHash)
            {
                return new ValidationResult(
                    false,
                    "Uploaded filedata was corrupted during upload. Please contact GICL about this problem",
                    $"Hash computed from file upload ({hashString}) does not match the posted hash that was calculated by the client script ({referenceHash}");
            }

            return ValidationResult.Success;
        }
    }
}
