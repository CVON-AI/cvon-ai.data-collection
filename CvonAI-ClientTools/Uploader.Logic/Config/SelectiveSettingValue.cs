using System;
using System.Collections.Generic;
using System.Text;

namespace Uploader.Logic.Config
{
    /// <summary>
    /// The selective setting value enumeration.
    /// Gives a functional meaning to a selected setting.
    /// </summary>
    public enum SelectiveSettingValue
    {
        /// <summary>
        /// Required setting. The setting appears,
        /// based on the category the setting belongs to,
        /// in the according web site forms as required
        /// and will be verified in uploaded files.
        /// </summary>
        Required = 1,

        /// <summary>
        /// Setting is optional and may appear as not
        /// required in any form, but will be verified
        /// in DICOM files if a value is entered in
        /// one of the forms.
        /// </summary>
        Optional = 2,

        /// <summary>
        /// Marks a setting as prohibited.
        /// Implies the given value may not appear in any DICOM file.
        /// </summary>
        Prohibited = 3,

        /// <summary>
        /// The unregistered setting value.
        /// Explicitly marks a setting as optional and not verified.
        /// </summary>
        Unregistered = 4
    }
}