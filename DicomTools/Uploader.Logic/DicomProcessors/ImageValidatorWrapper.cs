// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageValidatorWrapper.cs" company="UMCG - Thoraxcentrum">
//   (c) 2014, UMCG - Thoraxcentrum
// </copyright>
// <summary>
//   Provides a wrapper for all configured <see cref="IImageValidator" /> instances.
//   Calls an instance of each configured validator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Uploader.Logic.DicomProcessors
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Dicom;
    using Dicom.IO.Writer;

    // TODO: using Microsoft.Practices.Unity;

    using Uploader.Logic.Config;
    using Uploader.Logic.Controllers;
    using Uploader.Logic.Providers;

    /// <summary>
    /// Provides a wrapper for all configured <see cref="IDicomImageProcessor"/> instances.
    /// Calls an instance of each configured validator type.
    /// </summary>
    internal static class ImageValidatorWrapper
    {
        delegate BinaryResult ProcessingFunction(string imageFileName, DicomFile file);

        // todo: make some abstraction to do both methods as one, pass a delegate in!
        internal static BinaryResult CorrectFiles(Site siteSettings, Modality modality, IEnumerable<string> filenames)
        {
            BinaryResult totalResult = BinaryResult.Positive;


            // todo: implement patterns and practices again... foreach (IImageValidator validator in AppContainer.Current.Resolve<IImageValidatorFactory>().GetAll())
            IEnumerable<IDicomImageProcessor> imageValidators = new ImageValidatorFactory().GetAll(siteSettings, modality);
            IEnumerable<IDicomImageProcessor> dicomDirValidators = new ImageValidatorFactory().GetAll(siteSettings, modality, true);

            foreach (string fileName in filenames)
            {
                BinaryResult result = BinaryResult.Positive;
                // todo: get rid of this one...
                //if (imageContent == null || imageContent.Length == 0)
                //{
                //    string technicalInfo = "File content is missing";

                //    return new ValidationResult(false, "Filedata was not complete", technicalInfo);
                //}

                // TODO: make an abstract implementation to skip actual file system during development!!!
                DicomFile file = null;
                byte[] imageContent = File.ReadAllBytes(fileName);

                try
                {
                    using (MemoryStream stream = new MemoryStream(imageContent))
                    {
                        file = DicomFile.Open(stream, FileReadOption.ReadAll);
                    }
                }
                catch (Exception ex)
                {
                    string technicalInformation =
                        $"{DicomFileValidator.NotADicomFile}. The following exception occurred while attempting to read the file as a DICOM file\r\n{ex.Message}\r\n{ex.StackTrace}";
                    return new BinaryResult(false, DicomFileValidator.NotADicomFile, technicalInformation);
                }

                // select image or dicomdir specific validators!
                IEnumerable<IDicomImageProcessor> actualValidators = (fileName.EndsWith("DICOMDIR")) ? dicomDirValidators : imageValidators;
                foreach (IDicomImageProcessor validator in actualValidators)
                {
                    result.AppendResult(validator.ApplyCorrection(fileName, file));

                    // in the generic method, for this a boolean argument should be added!!!
                    //// cancel validation and return a negative result if the image fails on one of the validators
                    //// and if the validator specifies no further validation should be done afterwards
                    if (!result.Value && validator.StopValidationOnFailure)
                    {
                        break;
                    }
                }

                // if all is well...
                // TODO: maybe else: move here instead of in correct?
                    if (result.Value)
                {
                    SaveProcessedFile(fileName, file, modality, siteSettings);
                }

                totalResult.AppendResult(result);
            }
            return totalResult;
        }

        
        private static void SaveProcessedFile(string imageFileName, DicomFile file, Modality modality, Site site)
        {
            string studyId = new FileInfo(imageFileName).Directory.Name; // file?.Dataset.GetSingleValue<string>(DicomTag.StudyInstanceUID) ?? "UNKNOWN_STUDY";

            IStorageSettingsProvider provider = DependencyInjector.Resolve<IStorageSettingsProvider>();

            if (provider == null)
            {
                // TODO: log
                return;
            }

            string destinationFolder = Path.Join(provider.GetProcessedFolder(), modality.ToString(), site.Name, studyId);

            //string studyId = new FileInfo(imageFileName).Directory.Name; // file?.Dataset.GetSingleValue<string>(DicomTag.StudyInstanceUID) ?? "UNKNOWN_STUDY";
            // INCOMING IN FAILING MERGE - FROM UMCG:
            string destinationFileName = PathController.GetNewPath(imageFileName, site, modality, Path.Join(provider.GetProcessedFolder(), modality.ToString(), site.Name));
            // INCOMING IN FAILING MERGE - FROM UMCG:string destinationFolder = new FileInfo(destinationFileName).DirectoryName; 
            
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            try
            {
                // TODO: write all terminal text also to a file!
                new IOControllerWrapper().WriteLine($"Saving processed file {imageFileName} to {destinationFolder}");
                file.Save(destinationFileName, DicomWriteOptions.Default);
            }
            catch (System.Exception ex)
            {
                // second move should be prevented!
                // todo log
            }
        }

        /// <summary>
        /// Verifies whether the images meets all requirements
        /// by calling instances of all configured <see cref="IDicomImageProcessor"/> implementations.
        /// </summary>
        /// <param name="registration">The registration the image has been uploaded for.</param>
        /// <param name="imageFileName">The image file name.</param>
        /// <param name="imageContent">The image content.</param>
        /// <returns>
        /// The <see cref="BinaryResult"/>.
        /// </returns>
        internal static BinaryResult Validate(Site siteSettings, Modality modality, IEnumerable<string> filenames)
        //{
        //    return ProcessFile(siteSettings, modality, filenames, IDicomImageProcessor)
        //}
        //private static BinaryResult ProcessFile(Site siteSettings, Modality modality, IEnumerable<string> filenames, ProcessingFunction function)
        {
            BinaryResult totalResult = BinaryResult.Positive;

            // todo: implement patterns and practices again... foreach (IImageValidator validator in AppContainer.Current.Resolve<IImageValidatorFactory>().GetAll())
            IEnumerable<IDicomImageProcessor> imageValidators = new ImageValidatorFactory().GetAll(siteSettings, modality);
            IEnumerable<IDicomImageProcessor> dicomDirValidators = new ImageValidatorFactory().GetAll(siteSettings, modality, true);
            //string[] wheel = new string[] { "|", "/", "-", "\\" };
            //int wheelIndex = 0;

            foreach (string fileName in filenames)
            {
                BinaryResult result = BinaryResult.Positive;

                // TODO: make an abstract implementation to skip actual file system during development!!!
                DicomFile file = null;
                byte[] imageContent = File.ReadAllBytes(fileName);

                IEnumerable<IDicomImageProcessor> actualValidators = (fileName.EndsWith("DICOMDIR")) ? dicomDirValidators : imageValidators;

                try
                {
                    using (MemoryStream stream = new MemoryStream(imageContent))
                    {
                        file = DicomFile.Open(stream);
                    }
                }
                catch (Exception ex)
                {
                    string technicalInformation =
                        $"{DicomFileValidator.NotADicomFile}. The following exception occurred while attempting to read the file as a DICOM file\r\n{ex.Message}\r\n{ex.StackTrace}";
                    return new BinaryResult(false, DicomFileValidator.NotADicomFile, technicalInformation);
                }

                foreach (IDicomImageProcessor validator in actualValidators)
                {
                    //Console.Write($"\r{wheel[wheelIndex]}");
                    Console.Write(".");
                    //wheelIndex = (wheelIndex + 1) & 3;
                    result.AppendResult(validator.Validate(fileName, file));

                    // cancel validation and return a negative result if the image fails on one of the validators
                    // and if the validator specifies no further validation should be done afterwards
                    if (!result.Value && validator.StopValidationOnFailure)
                    {
                        break;
                    }

                }
                totalResult.AppendResult(result);
            }
            Console.WriteLine();
            return totalResult;
        }
    }
}