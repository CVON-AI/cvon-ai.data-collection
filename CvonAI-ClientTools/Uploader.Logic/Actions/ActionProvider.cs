using System;
using System.Collections.Generic;
using System.Text;
using Uploader.Logic.Config;

namespace Uploader.Logic.Actions
{
    internal class ActionProvider
    {
        internal static IAction GetAction(Action action, Modality modality, Site siteConfiguration)
        {
            switch (action)
            {
                case Action.ListLocalDir:
                    return new LocalDirScanner(siteConfiguration, modality);

                case Action.VerifyLocalFiles:
                    return new LocalDirValidator(siteConfiguration, modality);

                case Action.ApplyCorrections:
                    return new LocalDirCorrector(siteConfiguration, modality);

            }

            throw new ApplicationException("No valid action passed to ActionProvider.GetAction");
        }
    }
}
