using System.Collections.Generic;
using System.IO;
using System.Linq;
using Uploader.Logic.Config;

namespace Uploader.Logic.Actions
{
    internal abstract class ActionBase
    {
        protected ActionBase(Site siteSettings, Modality modality)
        {
            this.SiteSettings = siteSettings;
            this.Modality = modality;
        }

        public Site SiteSettings { get; }
        public Modality Modality { get; }

        // todo: generate generic folder processor... crawl, then print?
        public ActionResult Perform()
        {
            string localPath = null;

            if (this.SiteSettings?.ImageModalityConfig?.Any(ic => ic.Modality == this.Modality) ?? false)
            {
                localPath = this.SiteSettings.ImageModalityConfig.FirstOrDefault(ic => ic.Modality == this.Modality).DataExportPath;
            }
            else
            {
                localPath = this.SiteSettings?.StructuredModalityConfig?.FirstOrDefault(ic => ic.Modality == this.Modality)?.DataExportPath;
            }

            if (string.IsNullOrEmpty(localPath))
            {
                // TODO: LOG!!!
                return new ActionResult(false, $"No local path configured for modality {this.Modality}");
            }
            else
            {
                if (!Directory.Exists(localPath))
                {
                    // TODO: log!!!
                    return new ActionResult(false, $"Configured local path '{localPath}' for modality {this.Modality} does not exist");
                }

                Dictionary<string, List<string>> pathFiles = CrawlPath(localPath/*, 0*/);

                DoPerform(pathFiles/*localPath, 0*/);

                return new ActionResult(true);
            }
        }

        protected abstract void DoPerform(Dictionary<string, List<string>> paths);

        private void CrawlPath(Dictionary<string, List<string>> paths, string localPath/*, int depth*/)
        {
            //string pathPrefix = new string(' ', depth * 4);
            if (!paths.ContainsKey(localPath))
            {
                paths.Add(localPath, new List<string>(Directory.EnumerateFiles(localPath)));

                foreach (string directoryName in Directory.EnumerateDirectories(localPath))
                {
                    CrawlPath(paths, directoryName/*, depth + 1*/);
                }
            }
        }

        private Dictionary<string, List<string>> CrawlPath(string path)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

            CrawlPath(result, path/*, depth + 1*/);

            return result;
        }
    }
}