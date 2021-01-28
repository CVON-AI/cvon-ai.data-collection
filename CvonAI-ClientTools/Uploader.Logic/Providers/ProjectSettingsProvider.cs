namespace Uploader.Logic.Providers
{
    using System;
    using System.IO;
    using System.Text.Json;
    using Uploader.Logic.Config;

    /// <summary>
    /// Provides access to stored project settings and also facilitates storage 
    /// of updated settings
    /// </summary>
    internal class ProjectSettingsProvider
    {
        internal string GetJson(ProjectSettings settings)
        {
            return JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        }

        internal bool WriteToFile(ProjectSettings settings, string fileName)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(fileName, jsonString);
                return true;
            }
            catch (Exception ex)
            {
                // todo: log
                return false;
            }
        }

        internal ProjectSettings ReadFromFile(string fileName)
        {
            var jsonString = File.ReadAllText(fileName);
            return JsonSerializer.Deserialize<ProjectSettings>(jsonString);
        }
    }
}
