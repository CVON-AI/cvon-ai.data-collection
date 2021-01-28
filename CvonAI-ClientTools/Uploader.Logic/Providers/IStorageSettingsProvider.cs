namespace Uploader.Logic.Providers
{
    internal interface IStorageSettingsProvider
    {
        /// <summary>
        /// Gets the folder path for dumping files that failed 
        /// while processing
        /// </summary>
        /// <returns>The path in the filesystem for files that failed to be processed</returns>
        string GetFailedFolder();

        /// <summary>
        /// Gets the folder path for storing files that were
        /// processed successfully
        /// </summary>
        /// <returns>The path in the filesystem for successfully processed files</returns>
        string GetProcessedFolder();
    }
}