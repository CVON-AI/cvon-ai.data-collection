using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Uploader.Logic.Config;

namespace Uploader.Logic.Controllers
{
    /// <summary>
    /// Handles logic regarding import and export paths
    /// </summary>
    internal class PathController
    {
        internal static string GetNewPath(string sourcePath, Site site, Modality modality, string destinationRoot)
        {
            string sourceRoot = site?.ImageModalityConfig?.FirstOrDefault(config => config.Modality == modality).DataExportPath ?? ".";
            return Path.Combine(destinationRoot, Path.GetRelativePath(sourceRoot, sourcePath));
        }
    }
}
