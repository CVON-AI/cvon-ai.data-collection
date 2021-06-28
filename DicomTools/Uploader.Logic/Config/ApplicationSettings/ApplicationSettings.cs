using System;
using System.Collections.Generic;
using System.Text;

namespace Uploader.Logic.Config
{
    public class ApplicationSettings
    {
        /// <summary>
        /// Contains the configurtion of dependency injection 
        /// (a.k.a. inversion of control)
        /// </summary>
        public UnityConfiguration Unity { get; set; }


    }
}
