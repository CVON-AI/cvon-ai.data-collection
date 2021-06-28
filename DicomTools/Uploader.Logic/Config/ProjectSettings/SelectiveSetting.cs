namespace Uploader.Logic.Config
{
    /// <summary>
    /// Represents settings that may be included in 
    /// or explicitly excluded from registrations
    /// and uploaded files in a clinical trial.
    /// </summary>
    public class SelectiveSetting
    {
        ///// <summary>
        ///// Gets or sets the id.
        ///// </summary>
        //public int Id { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        ///// <summary>
        ///// Gets or sets the category.
        ///// </summary>
        //public SettingsCategory Category { get; set; }

        /// <summary>
        /// Defines the options for selective settings.
        /// </summary>
        public sealed class Options
        {
            /// <summary>
            /// Defines the 'date of birth' settings key;
            /// </summary>
            public const string DateOfBirth = "DateOfBirth";

            /// <summary>
            /// Defines the 'weight' settings key;
            /// </summary>
            public const string Weight = "Weight";

            /// <summary>
            /// Defines the 'height' settings key;
            /// </summary>
            public const string Height = "Height";

            /// <summary>
            /// Defines the 'Sex' settings key;
            /// </summary>
            public const string Sex = "Sex";
        }
    }

}