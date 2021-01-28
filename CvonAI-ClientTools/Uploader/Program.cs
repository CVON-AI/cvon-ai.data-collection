using System;
using System.CommandLine;
using System.CommandLine.IO;
//using System.CommandLine;
using System.IO;
using System.Linq;
using Uploader.Logic.Actions;
using Uploader.Logic.Config;
using Uploader.Logic.Controllers;
using Uploader.Logic.Providers;
using Action = Uploader.Logic.Actions.Action;

namespace Uploader
{
    class Program
    {
        //public static void Main(string[] args)
        //{
        //    new System.CommandLine.Help.DefaultHelpText().
        //}

        /// <summary>
        /// Converts an image file from one format to another.
        /// </summary>
        /// <param name="scannedModality">The modality for which an action needs to be taken.</param>
        /// <param name="actionToPerform">The action to perform on the modality.</param>
        public static void Main(Modality scannedModality, Action actionToPerform)
        {
            // TODO: automate help on missing arguments!

            bool execute = true;
            var helpBuilder = new System.CommandLine.Help.HelpBuilder(new SystemConsole());

            if (scannedModality == Modality.None)
            {
                new IOControllerWrapper().WriteLine("Modality is missing");
                
                helpBuilder.Write(new Command("modality", "The modality for which an action needs to be taken"));
                execute = false;
            }

            if (actionToPerform == Action.None)
            {
                new IOControllerWrapper().WriteLine("Action is missing");
                helpBuilder.Write(new Command("actionToPerform", "The action to perform on the modality"));
                execute = false;
            }

            if (!execute)
            {
                return;
            }

            ProjectSettingsProvider projectSettingsController = new ProjectSettingsProvider();
            ProjectSettings settings = projectSettingsController.ReadFromFile(@".\ProjectSettings.json");
            Site siteConfiguration = settings?.Sites.FirstOrDefault(s => s.IsCurrent);

            string resultsFile = Path.Combine(DependencyInjector.Resolve<IStorageSettingsProvider>().GetProcessedFolder(), scannedModality.ToString(), siteConfiguration.Name, DateTime.UtcNow.ToString().Replace(":", string.Empty) + ".txt");
            FileOutputController.FileName = resultsFile;
            
            IAction action = ActionProvider.GetAction(actionToPerform, scannedModality, siteConfiguration);
            ActionResult actionResult = action.Perform();

            if (!actionResult.Succeeded)
            {
                new IOControllerWrapper().WriteLine("Action failed: {0} for site {1} and modality {2}", actionToPerform, siteConfiguration?.Name ?? "<SITE-NOT-CONFIGURED>", scannedModality);
                new IOControllerWrapper().WriteLine("Cause of failue:");
                new IOControllerWrapper().WriteLine(actionResult.CauseOfFailure);
            }

            // File.WriteAllText(resultsFile, $"{actionResult.Succeeded}\r\n{(actionResult.Succeeded ? "" : actionResult.CauseOfFailure)}");
            // test to rewrite the configuration
            // new IOControllerWrapper().WriteLine(projectSettingsController.GetJson(settings));
            // settings.Name = "ModifiedName";
            // projectSettingsController.WriteToFile(settings, @".\ModifiedSettings.json");
            Console.ReadKey(true);
        }
    }
}
