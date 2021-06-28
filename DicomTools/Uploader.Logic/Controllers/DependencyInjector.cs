using Microsoft.Practices.Unity.Configuration;
using System;
using System.IO;
using Unity;
using Uploader.Logic.Providers;
using System.Text.Json;
using Uploader.Logic.Config;
using System.Collections.Generic;

namespace Uploader.Logic.Controllers
{
    /// <summary>
    /// Wrapper for unity inversion of control (dependency injection) configuration. 
    /// Provides unity based instantiation of types
    /// </summary>
    internal static class DependencyInjector
    {
        private static UnityContainer PrivateContainer;

        internal static IUnityContainer Container { 
            get
            {
                if (PrivateContainer == null)
                {
                    PrivateContainer = new UnityContainer();

                    try
                    {
                        var jsonString = File.ReadAllText(@".\ApplicationSettings.json");
                        ApplicationSettings settings =  JsonSerializer.Deserialize<ApplicationSettings>(jsonString);
                        Dictionary<Type, Type> typedRegistrations = settings?.Unity?.TypedRegistrations;

                        if (typedRegistrations != null)
                        {
                            foreach (Type abstraction in typedRegistrations.Keys)
                            {
                                //PrivateContainer.LoadConfiguration();
                                PrivateContainer.RegisterType(abstraction, typedRegistrations[abstraction]);
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        // todo: log e
                        PrivateContainer.RegisterType(typeof(IStorageSettingsProvider), typeof(BenzhiDummyStorageSettingsProvider));
                        PrivateContainer.RegisterType(typeof(IIOController), typeof(IOControllerWrapper));

                    }
                }
                return PrivateContainer;
            }
        }

        /// <summary>
        /// Tries to resolve the implemented type of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to be reseolved</typeparam>
        /// <returns>A configured instance of T if possible, otherwise null</returns>
        internal static T Resolve<T>() where T : class
        {
            try
            {
                return PrivateContainer.Resolve<T>();
            }
            catch (Exception e)
            {
                // todo: log
                return null;
            }
        }
    }
}
