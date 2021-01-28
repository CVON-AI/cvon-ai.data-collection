namespace UMCG.GICL.UploadTool.BusinessLogic.ImageValidators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dicom;

    using Microsoft.Practices.Unity;

    using UMCG.GICL.Core;
    using UMCG.GICL.Core.Constants;
    using UMCG.GICL.DataAccess.Interfaces;
    using UMCG.GICL.SharedResources;
    using UMCG.GICL.UploadTool.BusinessLogic.DomainMappers;
    using UMCG.GICL.UploadTool.DomainModel;
    using UMCG.Thorax.NMI.Instrumentation;
    using Uploader.Logic.Dicom;
    using Manufacturer = UMCG.GICL.DataAccess.DataModel.Manufacturer;

    /// <summary>
    /// Validates a <![CDATA[dicom]]> image, comparing it to a given <see cref="Registration"/> instance.
    /// </summary>
    internal sealed class DicomFileRegistrationBasedValidator : DicomFileValidatorBase, IImageValidator
    {
        /// <summary>
        /// Gets a value indicating whether validation should be
        /// stopped after validation fails on the current validator.
        /// </summary>
        public bool StopValidationOnFailure
        {
            get
            {
                return false;
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
            if (file == null)
            {
                string technicalInfo = "File content is missing";

                return new ValidationResult(false, "Uploaded filedata was not complete", technicalInfo, RegistrationState.UploadFailed);
            }

            try
            {
                ValidationResult result = ValidationResult.Success;
                DateTimeProcessor dateTimeStringProcessor = new DateTimeProcessor();
                // todo: strings in resources of core voor uniforme namen
                this.ValidateTag(file.Dataset, RegistrationDisplayNames.PatientId, DicomTag.PatientID, result, registration.PatientId, this.DicomStringModificator);
                this.ValidateTag(file.Dataset, RegistrationDisplayNames.SonographerName, DicomTag.OperatorsName, result, registration.ParticipatingOperator.AlternativeUserId, this.DicomStringModificator);

                if (propertyBag.ContainsKey(SelectiveSetting.Options.DateOfBirth))
                {
                    SelectiveSettingValue setting = ParseSelectiveSettingValue(propertyBag, SelectiveSetting.Options.DateOfBirth);

                    if (setting == SelectiveSettingValue.Required || setting == SelectiveSettingValue.Optional)
                    {
                        this.ValidateTag(file.Dataset, RegistrationDisplayNames.SubjectDateOfBirth, DicomTag.PatientBirthDate, result, (registration.SubjectDateOfBirth ?? DateTime.MinValue).Date, null, dateTimeStringProcessor);
                    }
                }

                if (propertyBag.ContainsKey(SelectiveSetting.Options.Sex))
                {
                    SelectiveSettingValue setting = ParseSelectiveSettingValue(propertyBag, SelectiveSetting.Options.Sex);

                    if (setting == SelectiveSettingValue.Required || setting == SelectiveSettingValue.Optional)
                    {
                        // todo: sex in registration this.ValidateTag(file.Dataset, RegistrationDisplayNames.SubjectSex, DicomTag.PatientSex, result, (registration.Sex ?? DateTime.MinValue).Date, null, dateTimeStringProcessor);
                    }
                }


                this.ValidateTag(file.Dataset, RegistrationDisplayNames.ExamDateTime, DicomTag.StudyDate, result, registration.StudyDateTime.Date, null, dateTimeStringProcessor);

                if (propertyBag.ContainsKey(SelectiveSetting.Options.Weight))
                {
                    SelectiveSettingValue setting = ParseSelectiveSettingValue(propertyBag, SelectiveSetting.Options.Weight);

                    if (setting == SelectiveSettingValue.Required || setting == SelectiveSettingValue.Optional)
                    {
                        this.ValidateTag(file.Dataset, RegistrationDisplayNames.SubjectWeight, DicomTag.PatientWeight, result, Convert.ToDecimal(registration.SubjectWeight ?? 0));
                    }
                }

                //Where(setting => setting.Value is SelectiveSettingValue).Cast<>().
                if (propertyBag.ContainsKey(SelectiveSetting.Options.Height))
                {
                    SelectiveSettingValue setting = ParseSelectiveSettingValue(propertyBag, SelectiveSetting.Options.Height);

                    if (setting == SelectiveSettingValue.Required || setting == SelectiveSettingValue.Optional)
                    {
                        this.ValidateTag(file.Dataset, RegistrationDisplayNames.SubjectHeight, DicomTag.PatientSize, result, (registration.SubjectHeight ?? 0)  / 100m);
                    }
                }

                // todo: manufacturer.... any of all values...
                Manufacturer manufacturer =
                    AppContainer.Current.Resolve<IManufacturerRepository>()
                        .Where(m => m.MachineModels.Any(mm => mm.Id == registration.MachineModelId))
                        .FirstOrDefault(m => m.MachineModels.Any(mm => mm.Id == registration.MachineModelId));

                string machineModelName = null;
                MachineModel model = manufacturer?.MachineModels.FirstOrDefault(mm => mm.Id == registration.MachineModelId).Map();

                if (model != null)
                {
                    machineModelName = model.Name;
                }
                this.ValidateTag(file.Dataset, RegistrationDisplayNames.MachineModel, DicomTag.ManufacturerModelName, result, /*registration.MachineModel?.Name*/machineModelName, this.DicomStringModificator);

                // visit?
                
                // when validation failed based on registration, set the state to OnHold or UploadFailed,
                // depending on the number of images uploaded before. If no images have been uploaded before,
                // Onhold should be set in order to enable the uploading site to correct the registration.
                // Otherwise, the upload should fail and the site should restart uploading other files
                if (!result.Value)
                {
                    int imageCount = registration.Series?.SelectMany(s => s.Images).Count() ?? 0;

                    result.FailureState = imageCount == 0 ? RegistrationState.OnHold : RegistrationState.UploadFailed;

                    // format the results-text:
                    // result.ReplaceInformationText(@"([^:\r\n]+)(?:: ([^\r\n]+) <--> ([^\r\n]+))?", "<tr><td>$1</td><td>$2</td><td><--></td><td>$3</td></tr>", true);
                    // result.ReplaceInformationText(@"([^:\r\n]+)(?:: ([^\r\n]+) &#8654; ([^\r\n]+))?", "<tr><td>$1</td><td>$2</td><td><--></td><td>$3</td></tr>", true);
                    result.ReplaceInformationText("\r\n", string.Empty, false);
                    result.ReplaceInformationText(result.Information, $"<b>Invalid data regarding the entered registration:</b><table><tr><th>Description</th><th>Registration</th><th/><th>Upload</th></tr>{result.Information}</table>", false);
                }

                return result;
                
            }
            catch (Exception ex)
            {
                string technicalInformation =
                    $"The following exception occurred while validating the contents of a DICOM file\r\n{ex.Message}\r\n{ex.StackTrace}";
                return new ValidationResult(false, "There was a problem with an uploaded file", technicalInformation, RegistrationState.UploadFailed);
            }
        }

        /// <summary>
        /// Removes null bytes from strings
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private string DicomStringModificator(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return string.Empty;
            }

            return arg.Replace("\0", string.Empty);
        }

        /// <summary>
        /// Validates a specific <paramref name="tag"/> inside the dicom dataset. If the tag is not present,
        /// a negative <see cref="ValidationResult"/> is appended. Also a negative result is appended if the
        /// value of the given <paramref name="tag"/> does not match the given <paramref name="referenceValue"/>.
        /// </summary>
        /// <param name="dicomDataset">The dataset which is to be checked for availability and value of the specified <paramref name="tag"/></param>
        /// <param name="label"></param>
        /// <param name="tag">The text which is to be searched for.</param>
        /// <param name="validate">The validationresult, which is to be appended to, if the required <paramref name="tag"/> was not found.</param>
        /// <param name="referenceValue">The reference value from the registration, which must equal the value in the given <paramref name="tag"/> of the <paramref name="dicomDataset"/></param>
        /// <param name="readValueModificator"></param>
        /// <param name="processor">Optional parameter to perform custom string conversion</param>
        private void ValidateTag<T>(DicomDataset dicomDataset, string label, DicomTag tag, ValidationResult validate, T referenceValue, Func<T, T> readValueModificator = null, IStringProcessor<T> processor = null)
            where T : IEquatable<T>
        {
            if (!dicomDataset.Contains(tag))
            {
                string uploadedFiledataWasNotValid = $"<tr><td>{label} is missing in DICOM data</td></tr>";
                validate.AppendValue(
                    false,
                    validate.Information.Contains(uploadedFiledataWasNotValid) ? null : uploadedFiledataWasNotValid,
                    RegistrationState.UploadFailed, $"Dicom tag {tag} for {label} is missing in image");
            }
            else
            {
                T valueInFile = dicomDataset.Get<T>(tag);

                // optionally modify the read value
                if (readValueModificator != null)
                {
                    valueInFile = readValueModificator(valueInFile);
                }

                if (!((IEquatable<T>)referenceValue).Equals(valueInFile))
                {
                    string informationalMessage;
                    if (processor == null)
                    {
                        informationalMessage = $"<tr><td>{label} did not match the registration:</td><td>{referenceValue}</td><td>&#8654;</td><td>{valueInFile}</td></tr>";
                    }
                    else
                    {
                        informationalMessage = $"<tr><td>{label} did not match the registration:</td><td>{processor.ToString(referenceValue)}</td><td>&#8654;</td><td>{processor.ToString(valueInFile)}</td></tr>";
                    }

                    validate.AppendValue(
                        false,
                        validate.Information.Contains(informationalMessage)
                            ? null
                            : informationalMessage,
                        RegistrationState.Unknown, $"Dicom tag {tag} value ({valueInFile}) in image does not match the registration value {referenceValue}");
                }
            }
        }

        private interface IStringProcessor<T> where T : IEquatable<T>
        {
            string ToString(T value);
        }

        private class DateTimeProcessor : IStringProcessor<DateTime>
        {
            public string ToString(DateTime value)
            {
                return value.ToString(StringFormats.Iso8601DateFormat);
            }
        }
    }
}