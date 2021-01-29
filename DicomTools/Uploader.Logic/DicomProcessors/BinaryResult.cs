// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidationResult.cs" company="UMCG - Thoraxcentrum">
//   (c) 2014, UMCG - Thoraxcentrum
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Uploader.Logic.DicomProcessors
{
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The validation result, with a value of type <see cref="bool"/>.
    /// </summary>
    public class BinaryResult : Result<bool>
    {
        /// <summary>
        /// The technical information string-builder.
        /// </summary>
        private readonly StringBuilder technicalInformationBuilder = new StringBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryResult"/> class.
        /// </summary>
        /// <param name="initialValue">
        ///     The initial value.
        /// </param>
        /// <param name="validationMessage">
        ///     The validation message.
        /// </param>
        /// <param name="technicalInformation">
        ///     The technical information regarding the value.
        /// </param>
        internal BinaryResult(bool initialValue, string validationMessage, string technicalInformation)
            : base(initialValue, validationMessage)
        {

            if (!string.IsNullOrEmpty(technicalInformation))
            {
                technicalInformationBuilder.AppendLine(technicalInformation);
            }
        }

        /// <summary>
        /// Gets the positive binary result instance.
        /// </summary>
        public static BinaryResult Positive
        {
            get
            {
                return new BinaryResult(true, string.Empty, string.Empty);
            }
        }

        /// <summary>
        /// Gets the technical information.
        /// </summary>
        public string TechnicalInformation
        {
            get
            {
                return technicalInformationBuilder.ToString();
            }
        }


        /// <summary>
        /// Appends a result.
        /// </summary>
        /// <param name="result">
        /// The result.
        /// </param>
        internal void AppendResult(BinaryResult result)
        {
            AppendValue(result.Value, result.Information, result.TechnicalInformation);
        }

        /// <summary>
        /// Method to append a validation message and change the result.
        /// </summary>
        /// <param name="newValue">
        ///     The new value that will be set (only if it is false).
        /// </param>
        /// <param name="validationMessage">
        ///     A description of the appended validation result.
        /// </param>
        /// <param name="failureState">The new state for a registration the given result belongs to.</param>
        /// <param name="technicalInformation">
        ///     The technical information regarding the validation result.
        /// </param>
        /// <remarks>
        /// Alters the base-class in such a way that when <see cref="Result{TResultType}.Value"/> has become <c>false</c>, it will never become <c>true</c> again.
        /// </remarks>
        internal void AppendValue(bool newValue, string validationMessage, string technicalInformation = null)
        {
            Value &= newValue;

            if (!string.IsNullOrEmpty(validationMessage))
            {
                InformationStringBuilder.AppendLine(validationMessage);
            }

            if (!string.IsNullOrEmpty(technicalInformation))
            {
                technicalInformationBuilder.AppendLine(technicalInformation);
            }
        }

        // todo : get rid of HTML specific code...
        /// <summary>
        /// Replaces (parts of) the current information text.
        /// </summary>
        /// <param name="searchText">The search text.</param>
        /// <param name="replacement">The text the <paramref name="searchText"/> should be replaced with.</param>
        /// <param name="isRegex">Determines wether a regulare expression find and replace should be used or a plain text replacement.</param>
        public void ReplaceInformationText(string searchText, string replacement, bool isRegex)
        {
            if (isRegex)
            {
                Regex regex = new Regex(searchText, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                string original = Information;

                string result = regex.Replace(original, replacement);
                InformationStringBuilder.Clear();
                InformationStringBuilder.AppendLine(result);
            }
            else
            {
                InformationStringBuilder.Replace(searchText, replacement);
            }
        }
    }
}