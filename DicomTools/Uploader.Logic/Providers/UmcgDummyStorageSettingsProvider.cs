namespace Uploader.Logic.Providers
{
    internal class UmcgDummyStorageSettingsProvider : IStorageSettingsProvider
    {
        /// <summary>
        /// Gets the folder path for dumping files that failed 
        /// while processing
        /// </summary>
        /// <returns>The path in the filesystem for files that failed to be processed</returns>
        public string GetFailedFolder()
        {
            // todo: create a config for this
            return @"D:\Cvon-AI-UploadTool\Failed";
        }

        /// <summary>
        /// Gets the folder path for storing files that were
        /// processed successfully
        /// </summary>
        /// <returns>The path in the filesystem for successfully processed files</returns>
        public string GetProcessedFolder()
        {
            // todo: create a config for this
            return @"D:\Cvon-AI-UploadTool\Processed";
        }
    }
}
