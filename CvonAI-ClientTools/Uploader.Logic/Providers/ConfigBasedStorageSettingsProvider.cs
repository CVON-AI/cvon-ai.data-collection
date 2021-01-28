using System;

namespace Uploader.Logic.Providers
{
    /// <summary>
    /// storage settings-provider implementation that uses a json file for configuration.
    /// </summary>
    internal class ConfigBasedStorageSettingsProvider : IStorageSettingsProvider
    {
        /// <summary>
        /// Gets the folder path for dumping files that failed 
        /// while processing
        /// </summary>
        /// <returns>The path in the filesystem for files that failed to be processed</returns>
        public string GetFailedFolder()
        {
            // todo: create a config for this
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the folder path for storing files that were
        /// processed successfully
        /// </summary>
        /// <returns>The path in the filesystem for successfully processed files</returns>
        public string GetProcessedFolder()
        {
            // todo: create a config for this
            throw new NotImplementedException();
        }
    }
}
