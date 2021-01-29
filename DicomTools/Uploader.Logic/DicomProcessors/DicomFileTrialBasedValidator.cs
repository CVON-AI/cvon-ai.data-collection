namespace Uploader.Logic.DicomProcessors
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Dicom;
    //using Dicom.Core;
    using Uploader.Logic.Config;
    using Uploader.Logic.Controllers;

    /// <summary>
    /// Validates whether an image matches a trial's settings
    /// </summary>
    internal sealed class DicomFileTrialBasedValidator : DicomFileValidatorBase, IDicomImageProcessor
    {
        private static Dictionary<string, Dictionary<DicomTag, object>> Studies = new Dictionary<string, Dictionary<DicomTag, object>>();

        internal DicomFileTrialBasedValidator(Site site, Modality modality) : base(site, modality)
        {
        }

        /// <summary>
        /// Gets a value indicating whether validation should be
        /// stopped after validation fails on the current validator.
        /// </summary>
        public override bool StopValidationOnFailure
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Verifies whether the images meets trial wide requirements.
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
                string technicalInfo = $"File content is missing or not DICOM in {imageFileName}";

                return new BinaryResult(false, $"Provided Filedata in '{imageFileName}' was not complete", technicalInfo);
            }

            // try to read the configuration for the currently configured site and the given imaging modality
            ImageModalityConfiguration config = CurrentModalityConfiguration;

            if (config == null)
            {
                // TODO: LOG
                return new BinaryResult(false, $"No configuration found for modality {Modality} and site {SiteConfiguration?.Name ?? "Unknown site!!!!"}", null);
            }

            try
            {
                if (new FileInfo(imageFileName).Name == "DICOMDIR")
                {
                    return ValidateDicomdir(file, imageFileName, config);
                }
                else
                {
                    return ValidateDataSet(file.Dataset, imageFileName, config);
                }
            }
            catch (Exception ex)
            {
                string technicalInformation =
                    $"The Following error occurred validating DICOM file.\r\n{ex.Message}\r\n{ex.StackTrace}";
                return new BinaryResult(false, "There was a problem with an uploaded file", technicalInformation);
            }
        }

        private BinaryResult ValidateDicomdir(DicomFile file, string imageFileName, ImageModalityConfiguration config)
        {
            BinaryResult result = BinaryResult.Positive;
            DicomSequence dicomSequence;

            if (file.Dataset.TryGetSequence(DicomTag.DirectoryRecordSequence, out dicomSequence))
            {
                foreach (DicomDataset item in dicomSequence)
                {
                    result.AppendResult(ValidateDataSet(item, imageFileName, config));
                }
            }

            return result;
        }

        private BinaryResult CorrectDicomdir(DicomFile file, string imageFileName, ImageModalityConfiguration config)
        {
            BinaryResult result = BinaryResult.Positive;
            DicomSequence dicomSequence;

            if (file.Dataset.TryGetSequence(DicomTag.DirectoryRecordSequence, out dicomSequence))
            {
                foreach (DicomDataset item in dicomSequence)
                {
                    result.AppendResult(CorrectDataSet(item, imageFileName, config, false));
                }
            }

            return result;
        }

        private BinaryResult ValidateDataSet(DicomDataset dicomDataset, string imageFileName, ImageModalityConfiguration config)
        {
            BinaryResult validationResult = BinaryResult.Positive;

            try
            {

                Dictionary<DicomTag, string> stringSettings = new Dictionary<DicomTag, string>
                {
                    { DicomTag.PatientID, "Patient ID" },
                    { DicomTag.PatientName, "Patient name" },
                };

                foreach (DicomTag tag in stringSettings.Keys)
                {
                    // validate the file against trial specific name patterns
                    if (config.DicomTags.Any(tagCfg => tagCfg.Tag == tag))
                    {
                        ValidateStringTag(stringSettings[tag], dicomDataset.GetSingleValue<string>(tag), validationResult, config.DicomTags.First(tagCfg => tagCfg.Tag == tag).ValuePattern);
                    }
                }

                // todo: in loop... all selectivesettings
                Dictionary<DicomTag, string> selectiveSettings = new Dictionary<DicomTag, string>
                {
                    { DicomTag.PatientBirthDate, SelectiveSetting.Options.DateOfBirth },
                    { DicomTag.PatientSex, SelectiveSetting.Options.Sex },
                    { DicomTag.PatientSize, SelectiveSetting.Options.Height},
                    { DicomTag.PatientWeight, SelectiveSetting.Options.Weight},
                };


                DateTimeProcessor dateTimeStringProcessor = new DateTimeProcessor();

                foreach (DicomTag tag in selectiveSettings.Keys)
                {
                    // validate the file against prohibited identifiable fields, such as age, gender, etc.
                    if (config.DicomTags.Any(tagCfg => tagCfg.Tag == tag))
                    {
                        SelectiveSettingValue setting = config.DicomTags.First(tagCfg => tagCfg.Tag == tag).SelectiveSetting;

                        if (setting == SelectiveSettingValue.Prohibited)
                        {
                            ValidateProhibitedTag(dicomDataset, selectiveSettings[tag], tag, validationResult); ;
                        }
                        else if (setting == SelectiveSettingValue.Required || setting == SelectiveSettingValue.Optional)
                        {
                            ValidateTag(dicomDataset, selectiveSettings[tag], tag, validationResult, setting);
                        }

                    }
                }

                // remove all extra patient id tags! always!
                // maybe in a separate one?
                Dictionary<DicomTag, string> byAllMeansProhibited = new Dictionary<DicomTag, string>
                    {
                        { DicomTag.OtherPatientIDsRETIRED, "Other patient ids (retired)" }, // 0010,1000
                        { DicomTag.OtherPatientIDsSequence, "Other patient ids seq" }, // 0010,1002
                        { DicomTag.OtherPatientNames, "Other patient names" }, // 0010,1001
                        { DicomTag.PatientBirthName, "Patient birth name" }, // 0010,1005
                        { DicomTag.PatientMotherBirthName, "Patient mother's birht name" }, // 0010,1060
                        { DicomTag.PatientTelephoneNumbers, "Patient phone numbers" }, // 0010,2154
                        { DicomTag.PatientAddress, "Patient address" }, // 0010,1040
                        { DicomTag.RegionOfResidence, "Patient region of residence" }, // 0010,2152
                    };

                foreach (DicomTag tag in byAllMeansProhibited.Keys)
                {
                    // validate the file against prohibited identifiable fields, such as age, gender, etc.
                    ValidateProhibitedTag(dicomDataset, byAllMeansProhibited[tag], tag, validationResult);
                }

                return validationResult;

            }
            catch (Exception ex)
            {
                string technicalInformation =
                    $"The Following error occurred validating DICOM file.\r\n{ex.Message}\r\n{ex.StackTrace}";
                return new BinaryResult(false, "There was a problem with an uploaded file", technicalInformation);
            }
        }

        /// <summary>
        /// Validates a specific <paramref name="valueInFile"/> inside the dicom dataset. A negative result is appended if the
        /// value of the given <paramref name="valueInFile"/> does not match the given <paramref name="referencePattern"/>.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="valueInFile">The text which to be checked.</param>
        /// <param name="validate">The validationresult, which is to be appended to, if the required <paramref name="valueInFile"/> was not correct.</param>
        /// <param name="referencePattern">The reference value from the registration, which must equal the value in the given <paramref name="valueInFile"/>.</param>
        /// 
        private void ValidateStringTag(string label, string valueInFile, BinaryResult validate, string referencePattern)
        {
            Regex regex = new Regex(referencePattern);

            if (!regex.IsMatch(valueInFile))
            {
                string informationalMessage = $"{label} did not match the pattern '{referencePattern}': {valueInFile}";

                validate.AppendValue(
                    false,
                    validate.Information.Contains(informationalMessage) ? null : informationalMessage,
                    $"Dicom tag value ({valueInFile}) in image does not match the configured pattern {referencePattern}");
            }
        }

        /// <summary>
        /// Validates a specific <paramref name="tag"/> inside the dicom dataset. If the tag is not present,
        /// a negative <see cref="BinaryResult"/> is appended. Also a negative result is appended if the
        /// value of the given <paramref name="tag"/> does not match the given <paramref name="referenceValue"/>.
        /// </summary>
        /// <param name="dicomDataset">The dataset which is to be checked for availability and value of the specified <paramref name="tag"/></param>
        /// <param name="label"></param>
        /// <param name="tag">The text which is to be searched for.</param>
        /// <param name="validate">The validationresult, which is to be appended to, if the required <paramref name="tag"/> was not found.</param>
        /// <param name="referenceValue">The reference value from the registration, which must equal the value in the given <paramref name="tag"/> of the <paramref name="dicomDataset"/></param>
        /// <param name="readValueModificator"></param>
        /// <param name="processor">Optional parameter to perform custom string conversion</param>
        private void ValidateTag/*<T>*/(DicomDataset dicomDataset, string label, DicomTag tag, BinaryResult validate, SelectiveSettingValue selectiveSetting/*, Func<T, T> readValueModificator = null, IStringProcessor<T> processor = null*/)
        //where T : IEquatable<T>
        {
            if (selectiveSetting == SelectiveSettingValue.Required && !dicomDataset.Contains(tag))
            {
                string uploadedFiledataWasNotValid = $"{label} is missing in DICOM data";
                validate.AppendValue(
                    false,
                    validate.Information.Contains(uploadedFiledataWasNotValid) ? null : uploadedFiledataWasNotValid,
                    $"Dicom tag {tag} for {label} is missing in image");
            }
            else
            {
                /*
                T valueInFile = dicomDataset.GetSingleValue<T>(tag);

                // optionally modify the read value
                if (readValueModificator != null)
                {
                    valueInFile = readValueModificator(valueInFile);
                }
                */

                //if (!((IEquatable<T>)referenceValue).Equals(valueInFile))
                //{
                //    string informationalMessage;
                //    if (processor == null)
                //    {
                //        informationalMessage = $"{label} did not match the registration:{referenceValue}: {valueInFile}";
                //    }
                //    else
                //    {
                //        informationalMessage = $"{label} did not match the registration:{processor.ToString(referenceValue)}, {processor.ToString(valueInFile)}";
                //    }

                //    validate.AppendValue(
                //        false,
                //        validate.Information.Contains(informationalMessage)
                //            ? null
                //            : informationalMessage,
                //        RegistrationState.Unknown, $"Dicom tag {tag} value ({valueInFile}) in image does not match the registration value {referenceValue}");
                //}
            }
        }



        /// <summary>
        /// Validates whether a specific prohibited <paramref name="tag"/> is not available or at least empty inside the dicom dataset. 
        /// If the tag is present and filled, a negative <see cref="BinaryResult"/> is appended.
        /// </summary>
        /// <param name="dicomDataset">The dataset which is to be checked for availability and value of the specified <paramref name="tag"/></param>
        /// <param name="label"></param>
        /// <param name="tag">The text which is to be searched for.</param>
        /// <param name="validate">The validationresult, which is to be appended to, if the required <paramref name="tag"/> was not found.</param>
        private void ValidateProhibitedTag(DicomDataset dicomDataset, string label, DicomTag tag, BinaryResult validate)
        {
            if (dicomDataset.Contains(tag))
            {
                string uploadedFiledataWasNotValid = $"Prohibited field {label} contains a value in the uploaded File";

                // first try to get the tag as a sequence. If it is a single, continue after, otherwise return
                DicomSequence seq = null;
                if (dicomDataset.TryGetSequence(tag, out seq))
                {
                    foreach (DicomDataset sub in seq.Items)
                    {
                        //DicomLongString[] subArray = sub.Select(s => s.Get<string>()).ToArray();
                        //// Skip the last, because it's a sequence delemiter!
                        //for (int subIndex = 0; subIndex < subArray.Length; subIndex++)
                        //{
                        //    if (!string.IsNullOrEmpty(subArray[subIndex].StringValue)) { }
                        //}
                        DicomElement[] subs = sub.OfType<DicomElement>().SkipLast(1).ToArray();

                        if (subs.Any(item => item.Length != 0)) // !string.IsNullOrEmpty(item.ValueRepresentation.ToString())))
                        {
                            validate.AppendValue(
                                false,
                                validate.Information.Contains(uploadedFiledataWasNotValid) ? null : uploadedFiledataWasNotValid,
                                $"Prohibited Dicom tag {tag} for {label} is available and contains a value in image");
                            return;
                        }
                    }

                    return;
                }

                try
                {
                    string[] multipleValues = dicomDataset.GetValues<string>(tag);

                    if (multipleValues?.Any(item => !string.IsNullOrEmpty(item)) ?? false)
                    {
                        validate.AppendValue(
                                false,
                                validate.Information.Contains(uploadedFiledataWasNotValid) ? null : uploadedFiledataWasNotValid,
                                $"Prohibited Dicom tag {tag} for {label} is available and contains a value in image");
                        return;
                    }

                    return;
                }
                catch (Exception ex)
                {
                    // log
                }

                // validate the tag as a single valued tag
                string tagValue = dicomDataset.GetSingleValue<string>(tag);

                if (!string.IsNullOrEmpty(tagValue))
                {

                    validate.AppendValue(
                        false,
                        validate.Information.Contains(uploadedFiledataWasNotValid) ? null : uploadedFiledataWasNotValid,
                        $"Prohibited Dicom tag {tag} for {label} is available and contains a value in image");
                }
            }
        }

        /// <summary>
        /// Corrects a specific <paramref name="tag"/> inside the dicom dataset if needed. If the tag is not present,
        /// a tag is added. User input is required if a value is to be added or updated, an no info was available for the current study
        /// </summary>
        /// <param name="dicomDataset">The dataset which is to be checked for availability and value of the specified <paramref name="tag"/></param>
        /// <param name="label"></param>
        /// <param name="tag">The text which is to be searched for.</param>
        /// <param name="validate">The validationresult, which is to be appended to, if the required <paramref name="tag"/> was not found.</param>
        /// <param name="referenceValue">The reference value from the registration, which must equal the value in the given <paramref name="tag"/> of the <paramref name="dicomDataset"/></param>
        /// <param name="readValueModificator"></param>
        /// <param name="processor">Optional parameter to perform custom string conversion</param>
        private void CorrectTag(DicomDataset dicomDataset, string label, DicomTag tag, BinaryResult validate, SelectiveSettingValue selectiveSetting, string fileName)
        {
            Dictionary<DicomTag, object> currentStudy = GetStudyValues(dicomDataset, fileName);

            if (selectiveSetting == SelectiveSettingValue.Required && !dicomDataset.Contains(tag))
            {
                object addedValue = GetTagValueForStudy(dicomDataset, label, tag, fileName, currentStudy);
                string uploadedFiledataWasNotValid = $"{label} is missing in DICOM data";
                validate.AppendValue(
                    true,
                    validate.Information.Contains(uploadedFiledataWasNotValid) ? null : uploadedFiledataWasNotValid,
                    $"Dicom tag {tag} for {label} was missing in image, adding value {addedValue.ToString()}");


            }
        }

        private object GetTagValueForStudy(DicomDataset dicomDataset, string label, DicomTag tag, string fileName, Dictionary<DicomTag, object> currentStudy, string referencePattern = null)
        {
            while (!currentStudy.ContainsKey(tag))
            {
                string PatientID = dicomDataset.GetString(DicomTag.PatientID) ?? "UNKNOWN";
                new IOControllerWrapper().WriteLine($"Please add a value for {label} in file fileName: {fileName}, patientId: {PatientID}. Apply with Enter");
                string value = Console.ReadLine();

                if (tag == DicomTag.PatientWeight || tag == DicomTag.PatientSize)
                {
                    try
                    {
                        currentStudy.Add(tag, Convert.ToInt32(value));
                    }
                    catch (Exception)
                    {
                        new IOControllerWrapper().WriteLine("That was not a number. Please, try again");
                        throw;
                    }
                }
                else if (!string.IsNullOrEmpty(referencePattern))
                {
                    Regex regex = new Regex(referencePattern);

                    if (!regex.IsMatch(value))
                    {
                        new IOControllerWrapper().WriteLine($"'{value}' does not match pattern '{referencePattern}'. Please try again");
                    }
                    else
                    {
                        currentStudy.Add(tag, value);
                    }
                }
                else
                {
                    currentStudy.Add(tag, value);
                }
            }

            if (currentStudy[tag] is int)
            {
                dicomDataset.AddOrUpdate(tag, (int)currentStudy[tag]);
            }
            else
            {
                dicomDataset.AddOrUpdate(tag, currentStudy[tag].ToString());

            }

            return currentStudy[tag];
        }

        private static Dictionary<DicomTag, object> GetStudyValues(DicomDataset dicomDataset, string fileName)
        {
            string parentDir = new FileInfo(fileName).DirectoryName;
            //string studyId;
            //if (!string.IsNullOrEmpty(fileName) && !dicomDataset.Contains(DicomTag.StudyInstanceUID))
            //{ 
            //    studyId = NOT THE SOLUTION!!!! for DICOMDIR, need to get the original name and patient id to match the modified!!!!! 
            //}
            string studyId = parentDir; //  dicomDataset.GetString(DicomTag.StudyInstanceUID);
            if (!Studies.ContainsKey(studyId))
            {
                Studies.Add(studyId, new Dictionary<DicomTag, object>());
            }
            Dictionary<DicomTag, object> currentStudy = Studies[studyId];
            return currentStudy;
        }

        /// <summary>
        /// Corrects a specific <paramref name="valueInFile"/> inside the dicom dataset if the
        /// value of the given <paramref name="valueInFile"/> does not match the given <paramref name="referencePattern"/>.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="valueInFile">The text which to be checked.</param>
        /// <param name="validate">The validationresult, which is to be appended to, if the required <paramref name="valueInFile"/> was not correct.</param>
        /// <param name="referencePattern">The reference value from the registration, which must equal the value in the given <paramref name="valueInFile"/>.</param>
        /// 
        private void CorrectStringTag(string label, DicomDataset dicomDataset, DicomTag tag, BinaryResult validate, string referencePattern, string fileName, bool tagIsRequired = true)
        {
            Regex regex = new Regex(referencePattern);

            // only try to correct if it is required or available, causing an error if required and not available!
            if (tagIsRequired || dicomDataset.Contains(tag))
            {
                string valueInFile = dicomDataset.GetString(tag);

                if (!regex.IsMatch(valueInFile))
                {
                    Dictionary<DicomTag, object> currentStudy = GetStudyValues(dicomDataset, fileName);
                    object addedValue = GetTagValueForStudy(dicomDataset, label, tag, fileName, currentStudy, referencePattern);

                    string informationalMessage = $"{label} did not match the pattern '{referencePattern}' in '{fileName}': {valueInFile} and is corrected with {addedValue.ToString()}";

                    validate.AppendValue(
                        true,
                        validate.Information.Contains(informationalMessage) ? null : informationalMessage,
                        $"Dicom tag value ({valueInFile}) in image does not match the configured pattern {referencePattern}");

                }
            }
        }



        /// <summary>
        /// Removse a prohibited <paramref name="tag"/> if it is available inside the dicom dataset. 
        /// </summary>
        /// <param name="dicomDataset">The dataset which is to be checked for availability and value of the specified <paramref name="tag"/></param>
        /// <param name="label"></param>
        /// <param name="tag">The text which is to be searched for.</param>
        /// <param name="validate">The validationresult, which is to be appended to, if the required <paramref name="tag"/> was not found.</param>
        private void CorrectProhibitedTag(DicomDataset dicomDataset, string label, DicomTag tag, BinaryResult validate)
        {
            if (dicomDataset.Contains(tag))
            {
                dicomDataset.Remove(tag);
                string uploadedFiledataWasNotValid = $"Prohibited field {label} is removed in the uploaded File";

                validate.AppendValue(
                    true,
                    validate.Information.Contains(uploadedFiledataWasNotValid) ? null : uploadedFiledataWasNotValid,
                    $"Prohibited Dicom tag {tag} for {label} is available and contains a value in image");
            }
        }


        public override BinaryResult ApplyCorrection(string imageFileName, DicomFile file)
        {
            // no move if invalid =D
            BinaryResult binaryResult = Validate(imageFileName, file);

            if (!binaryResult.Value)
            {
                // try to read the configuration for the currently configured site and the given imaging modality
                ImageModalityConfiguration config = CurrentModalityConfiguration;

                if (config == null)
                {
                    // TODO: LOG
                    return new BinaryResult(false, $"No configuration found for modality {Modality} and site {SiteConfiguration?.Name ?? "Unknown site!!!!"}", null);
                }


                try
                {
                    if (new FileInfo(imageFileName).Name == "DICOMDIR")
                    {
                        return CorrectDicomdir(file, imageFileName, config);
                    }
                    else
                    {
                        return CorrectDataSet(file.Dataset, imageFileName, config);
                    }
                }
                catch (Exception ex)
                {

                    string technicalInformation =
                        $"The following exception occurred while attempting to correct the file '{imageFileName}'\r\n{ex.Message}\r\n{ex.StackTrace}";
                    return new BinaryResult(false, "There was a problem correcting file", technicalInformation);
                }
            }

            return binaryResult;
        }

        private BinaryResult CorrectDataSet(DicomDataset dicomDataset, string imageFileName, ImageModalityConfiguration config, bool tagIsRequired = true)
        {
            BinaryResult validationResult = BinaryResult.Positive;

            try
            {

                Dictionary<DicomTag, string> stringSettings = new Dictionary<DicomTag, string>
                    {
                        { DicomTag.PatientID, "Patient ID" },
                        { DicomTag.PatientName, "Patient name" },
                    };

                foreach (DicomTag tag in stringSettings.Keys)
                {
                    // validate the file against trial specific name patterns
                    if (config.DicomTags.Any(tagCfg => tagCfg.Tag == tag))
                    {
                        CorrectStringTag(stringSettings[tag], dicomDataset, tag, validationResult, config.DicomTags.First(tagCfg => tagCfg.Tag == tag).ValuePattern, imageFileName, tagIsRequired);
                    }
                }

                // todo: in loop... all selectivesettings
                Dictionary<DicomTag, string> selectiveSettings = new Dictionary<DicomTag, string>
                    {
                        { DicomTag.PatientBirthDate, SelectiveSetting.Options.DateOfBirth }, // 0010,0030
                        { DicomTag.PatientSex, SelectiveSetting.Options.Sex }, // 0010,0040
                        { DicomTag.PatientSize, SelectiveSetting.Options.Height}, // 0010,1030
                        { DicomTag.PatientWeight, SelectiveSetting.Options.Weight}, // 0010,1030
                    };


                DateTimeProcessor dateTimeStringProcessor = new DateTimeProcessor();

                foreach (DicomTag tag in selectiveSettings.Keys)
                {
                    // validate the file against prohibited identifiable fields, such as age, gender, etc.
                    if (config.DicomTags.Any(tagCfg => tagCfg.Tag == tag))
                    {
                        SelectiveSettingValue setting = config.DicomTags.First(tagCfg => tagCfg.Tag == tag).SelectiveSetting;

                        if (setting == SelectiveSettingValue.Prohibited)
                        {
                            CorrectProhibitedTag(dicomDataset, selectiveSettings[tag], tag, validationResult);
                        }
                        else if (setting == SelectiveSettingValue.Required || setting == SelectiveSettingValue.Optional)
                        {
                            CorrectTag(dicomDataset, selectiveSettings[tag], tag, validationResult, setting, imageFileName);
                        }

                    }
                }

                // remove all extra patient id tags! always!
                // maybe in a separate one?
                Dictionary<DicomTag, string> byAllMeansProhibited = new Dictionary<DicomTag, string>
                    {
                        { DicomTag.OtherPatientIDsRETIRED, "Other patient ids (retired)" }, // 0010,1000
                        { DicomTag.OtherPatientIDsSequence, "Other patient ids seq" }, // 0010,1002
                        { DicomTag.OtherPatientNames, "Other patient names" }, // 0010,1001
                        { DicomTag.PatientBirthName, "Patient birth name" }, // 0010,1005
                        { DicomTag.PatientMotherBirthName, "Patient mother's birht name" }, // 0010,1060
                        { DicomTag.PatientTelephoneNumbers, "Patient phone numbers" }, // 0010,2154
                        { DicomTag.PatientAddress, "Patient address" }, // 0010,1040
                        { DicomTag.RegionOfResidence, "Patient region of residence" }, // 0010,2152
                    };

                foreach (DicomTag tag in byAllMeansProhibited.Keys)
                {
                    CorrectProhibitedTag(dicomDataset, byAllMeansProhibited[tag], tag, validationResult);
                }

                return validationResult;
            }
            catch (Exception ex)
            {
                string technicalInformation =
                    $"The following exception occurred while attempting to correct the file '{imageFileName}'\r\n{ex.Message}\r\n{ex.StackTrace}";
                return new BinaryResult(false, "There was a problem correcting file", technicalInformation);
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