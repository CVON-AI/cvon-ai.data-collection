using System;
using System.Collections.Generic;
using System.Linq;

namespace Uploader.Logic.Config
{
    public class UnityConfiguration
    {
#pragma warning disable CA2227 // Collection properties should be read only
        /// <summary>
        /// Gets or sets a dictionary of mappings between abstract / interface types and implementing 
        /// types. The types may be implemented directly, or indirectly through <see cref="Aliases"/>,
        /// which may be convenient for readability.
        /// </summary>
        public Dictionary<string, string> Registrations { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets a dictionary containing type aliases, which allows the user to define type-type
        /// mappings in <see cref="Registrations"/> in a more readable way.
        /// </summary>
        public Dictionary<string, string> Aliases { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets a dictionary of type-type mappings, based on the combination of <see cref="Registrations"/>
        /// and <see cref="Aliases"/>, which can be used for registering type-mappings in Unity containers.
        /// The value keys must be implementations of the corresponding key types.
        /// </summary>
        public Dictionary<Type, Type> TypedRegistrations
        {
            get
            {
                Dictionary<Type, Type> result = new Dictionary<Type, Type>();

                foreach (string regKey in this.Registrations.Keys)
                {
                    string abstractTypeString = (Aliases.ContainsKey(regKey)) ? Aliases[regKey] : regKey;
                    string regValue = this.Registrations[regKey];
                    string implementedTypeString = (Aliases.ContainsKey(regValue)) ? Aliases[regValue] : regValue;

                    Type abstractType = null;
                    Type implementedType = null;

                    // verify wheter abstract and implementation exist,
                    // whether abstract is interface, AND implementation
                    // is an implementation of abstract.
                    // If all conditions are met, add the key-value pair
                    // to the result.
                    try
                    {
                        abstractType = Type.GetType(abstractTypeString);
                    }
                    catch (Exception ex)
                    {
                        // todo: log!!!!
                    }

                    try
                    {
                        implementedType = Type.GetType(implementedTypeString);
                    }
                    catch (Exception ex)
                    {
                        // todo: log!!!!
                    }

                    if (abstractType != null && implementedType != null)
                    {
                        bool abstractIsInterface = abstractType.IsInterface;
                        bool implementedImplementsAbstract = implementedType.GetInterfaces().Any(i => i.FullName == abstractType.Name);

                        if (!abstractIsInterface)
                        {
                            // todo: log!!!
                        }

                        if (!implementedImplementsAbstract)
                        {
                            // todo: log!!!
                        }

                        if (abstractIsInterface && implementedImplementsAbstract)
                        {
                            result.Add(abstractType, implementedType);
                        }
                    }
                }

                return result;
            }
        }
        
#pragma warning restore CA2227 // Collection properties should be read only
    }
}