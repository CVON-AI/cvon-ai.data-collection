// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DicomFileValidatorBase.cs" company="UMCG - Thoraxcentrum">
//   (c) 2014, UMCG - Thoraxcentrum
// </copyright>
// <summary>
//   The <![CDATA[DICOM]]> file validator base-class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Uploader.Logic.DicomProcessors
{
    using Dicom;
    using System;
    using System.IO;
    using System.Linq;
    using Uploader.Logic.Config;
    using Uploader.Logic.Controllers;
    using Uploader.Logic.Providers;

    /// <summary>
    /// The <![CDATA[DICOM]]> file validator base-class.
    /// </summary>
    internal abstract class DicomFileValidatorBase : IDicomImageProcessor
    {
        internal DicomFileValidatorBase(Site site, Modality modality)
        {
            Modality = modality;
            SiteConfiguration = site;
        }

        protected Modality Modality { get; }

        protected Site SiteConfiguration { get; }

        protected ImageModalityConfiguration CurrentModalityConfiguration
        {
            get
            {
                return SiteConfiguration?.ImageModalityConfig?.FirstOrDefault(config => config.Modality == Modality);
            }
        }

        public abstract bool StopValidationOnFailure { get; }

        public abstract BinaryResult Validate(string imageFileName, DicomFile file);


        /// <summary>
        /// Dummy applycorrection method, because
        /// </summary>
        /// <param name="imageFileName"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public virtual BinaryResult ApplyCorrection(string imageFileName, DicomFile file)
        {

            return BinaryResult.Positive;
        }

        protected BinaryResult MoveIfInvalid(string imageFileName, DicomFile file)
        {
            BinaryResult validationResult = this.Validate(imageFileName, file);

            if (!validationResult.Value)
            {
                MoveFile(imageFileName);

                return validationResult;
            }

            return BinaryResult.Positive;
        }

        protected void MoveFile(string imageFileName)
        {
            string studyId = new FileInfo(imageFileName).Directory.Name; // file?.Dataset.GetSingleValue<string>(DicomTag.StudyInstanceUID) ?? "UNKNOWN_STUDY";

            IStorageSettingsProvider provider = DependencyInjector.Resolve<IStorageSettingsProvider>();

            if (provider == null)
            {
                // TODO: log
                return;
            }

            string destinationFolder = Path.Join(provider.GetFailedFolder(), this.Modality.ToString(), this.SiteConfiguration.Name, studyId);
            // INCOMING IN FAILING MERGE - FROM UMCG:
            string destinationFileName = PathController.GetNewPath(imageFileName, this.SiteConfiguration, this.Modality, Path.Join(provider.GetFailedFolder(), this.Modality.ToString(), this.SiteConfiguration.Name));
            // INCOMING IN FAILING MERGE - FROM UMCG: string destinationFolder = new FileInfo(destinationFileName).DirectoryName; //  Path.Join(StorageSettingsProvider.GetFailedFolder(), modality.ToString(), site.Name, studyId);

            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            try
            {
                // TODO: write all terminal text also to a file!
                (DependencyInjector.Resolve<IIOController>() ?? new IOControllerWrapper()).WriteLine($"Moving {imageFileName} to {destinationFolder}");
                File.Move(imageFileName, destinationFileName);
            }
            catch (System.Exception ex)
            {
                // second move should be prevented!
                // todo log
            }
        }

        /// <summary>
        /// Tries to convert a data-access layer based enumeration value to selective setting value.
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The <see cref="SelectiveSettingValue"/>.
        /// </returns>
        //protected static SelectiveSettingValue ParseSelectiveSettingValue(string key)
        //{
        //    try
        //    {
        //        // ReSharper disable once PossibleInvalidCastException
        //        return propertyBag[key] is Enum ? (SelectiveSettingValue)((int)propertyBag[key]) : SelectiveSettingValue.Unknown;
        //    }
        //    catch (Exception)
        //    {
        //        return SelectiveSettingValue.Unknown;
        //    }
        //}
    }
}
