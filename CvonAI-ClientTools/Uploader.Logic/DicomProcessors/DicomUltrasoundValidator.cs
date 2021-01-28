// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DicomUltrasoundValidator.cs" company="UMCG - Thoraxcentrum">
//   (c) 2014, UMCG - Thoraxcentrum
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Uploader.Logic.DicomProcessors
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using Dicom;
    using Uploader.Logic.Config;

    //using Microsoft.Practices.ObjectBuilder2;

    /// <summary>
    /// Verifies whether an uploaded file is a DICOM Ultrasound Image.
    /// </summary>
    internal sealed class DicomUltrasoundValidator : DicomFileValidatorBase, IDicomImageProcessor
    {
        internal DicomUltrasoundValidator(Site site, Modality modality) : base(site, modality)
        {
        }

        public override bool StopValidationOnFailure
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// The validate.
        /// </summary>
        /// <param name="registration">
        /// The registration.
        /// </param>
        /// <param name="imageFileName">
        /// The image file name.
        /// </param>
        /// <param name="file">
        /// The file.
        /// </param>
        /// <param name="imageContents">
        /// The image contents.
        /// </param>
        /// <returns>
        /// The <see cref="BinaryResult"/>.
        /// </returns>
        public override BinaryResult Validate(string imageFileName, DicomFile file)
        {
            if (file == null)
            {
                string technicalInfo = $"File content is missing or not DICOM in {imageFileName}";

                return new BinaryResult(false, $"Filedata in '{imageFileName}' was not complete or not DICOM", technicalInfo);
            }

            try
            {
                // verify whether the Modality tag is available and has the value US, because
                // EchoPAC does not read images without this tag value.
                if (!file.Dataset.Contains(DicomTag.Modality))
                {
                    return new BinaryResult(false, $"Exported Filedata in '{imageFileName}' was not valid", $"Dicom tag Modality is missing in image {imageFileName}");
                }

                string modality = file.Dataset.GetSingleValue<string>(DicomTag.Modality);
                string requiredModality = "UNDEFINED";

                switch (this.Modality)
                {
                    case Modality.CT:
                        requiredModality = DicomModality.Ct;
                        break;

                    case Modality.Echo:
                        requiredModality = DicomModality.Ultrasound;
                        break;

                    case Modality.MRI:
                        requiredModality = DicomModality.MRI;
                        break;
                    //case Modality.Echo...vaat!!!

                }

                if (requiredModality == "UNDEFINED")
                {
                    return new BinaryResult(
                    false,
                    $"No required modality string configured for {this.Modality}",
                    $"Dicom tag Modality configuration not configured for '{this.Modality}'");
                }

                if (modality != requiredModality)
                {
                    return new BinaryResult(
                        false,
                        $"Filedata in '{imageFileName}' was not valid",
                        $"Dicom tag Modality in {imageFileName} does not contain the value '{requiredModality}'");
                }



                // TODO: reimplement!!!!
                // TODO: make separate classes for modalities...
                if (modality == DicomModality.Ultrasound)
                {
                    DicomSequence dicomSequence; // = file.Dataset.GetSequence(DicomTag.SequenceOfUltrasoundRegions);
                    // verify whether ultrasound regions are available. These are required for calibration.
                    if (!file.Dataset.TryGetSequence(DicomTag.SequenceOfUltrasoundRegions, out dicomSequence))
                    {
                        return new BinaryResult(false, $"Filedata in '{imageFileName}' was not valid", $"Dicom tag SequenceOfUltrasoundRegions is missing in image {imageFileName}");
                    }


                    BinaryResult result = BinaryResult.Positive;

                    // validate all entries in SequenceOfUltrasoundRegions
                    if (dicomSequence != null && dicomSequence.Items.Count != 0)
                    {
                        foreach (DicomDataset dicomDataset in dicomSequence.Items)
                        {
                            DicomTag[] tagsToValidate =
                                {
                                DicomTag.RegionSpatialFormat, DicomTag.RegionLocationMinX0, DicomTag.RegionLocationMinY0,
                                DicomTag.RegionLocationMaxX1, DicomTag.RegionLocationMaxY1, DicomTag.PhysicalDeltaX,
                                DicomTag.PhysicalDeltaY, DicomTag.PhysicalUnitsXDirection, DicomTag.PhysicalUnitsYDirection/*, 
                                DicomTag.ReferencePixelX0, DicomTag.ReferencePixelY0 /*, DicomTag.ReferencePixelPhysicalValueX,
                                    DicomTag.ReferencePixelPhysicalValueY*/
                            };

                            foreach (DicomTag tag in tagsToValidate)
                            {
                                this.ValidateTag(dicomDataset, tag, result, imageFileName);
                            }
                        }
                    }

                    return result;
                }

                return BinaryResult.Positive;
            }
            catch (Exception ex)
            {
                string technicalInformation =
                    $"Uploaded file is not a DICOM file or any of the required DICOM tags is missing or not readable.\r\nThe following exception occurred while attempting to read the file as a DICOM file\r\n{ex.Message}\r\n{ex.StackTrace}";
                return new BinaryResult(false, "There was a problem with an uploaded file", technicalInformation);
            }
        }

        /// <summary>
        /// Validates a specific <paramref name="tag"/> inside the dicom dataset. If the tag is not present,
        /// a negative <see cref="BinaryResult"/> is appended.
        /// </summary>
        /// <param name="dicomDataset">
        ///     The dataset which is to be checked for availability of the specified <paramref name="tag"/>
        /// </param>
        /// <param name="tag">
        ///     The text which is to be searched for.
        /// </param>
        /// <param name="validate">
        ///     The validationresult, which is to be appended to, if the required <paramref name="tag"/> was not found.
        /// </param>
        /// <param name="imageFileName"></param>
        private void ValidateTag(DicomDataset dicomDataset, DicomTag tag, BinaryResult validate, string imageFileName)
        {
            if (!dicomDataset.Contains(tag))
            {
                validate.AppendResult(
                    new BinaryResult(
                        false,
                        validate.Information.Contains($"Uploaded filedata was not valid for file") ? null : $"Uploaded filedata was not valid for file '{imageFileName}'",
                        $"Dicom tag {tag} is missing in SequenceOfUltrasoundRegions of image '{imageFileName}'"));
            }
        }

        public override BinaryResult ApplyCorrection(string imageFileName, DicomFile file)
        {
            return base.MoveIfInvalid(imageFileName, file);
        }
    }
}
