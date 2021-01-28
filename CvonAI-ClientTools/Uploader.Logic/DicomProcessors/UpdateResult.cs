// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateResult.cs" company="UMCG - Thoraxcentrum">
//   (c) 2014, UMCG - Thoraxcentrum
// </copyright>
// <summary>
//   The update result, with a value of type <see cref="bool" />.
//   Also wraps up an instance of <see cref="ValidationResult" />,
//   since an unsuccessful validation will not result in an update.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Uploader.Logic.DicomProcessors
{
    using System.Text;

    /// <summary>
    /// The update result, with a value of type <see cref="bool"/>.
    /// Also wraps up an instance of <see cref="ValidationResult"/>,
    /// since an unsuccessful validation will not result in an update.
    /// </summary>
    public class UpdateResult : Result<bool>
    {
        /// <summary>
        /// The technical information string-builder.
        /// </summary>
        private readonly StringBuilder technicalInformationBuilder = new StringBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateResult"/> class.
        /// </summary>
        /// <param name="initialValue">
        /// The initial value.
        /// </param>
        /// <param name="validationMessage">
        /// The validation message.
        /// </param>
        /// <param name="technicalInformation">
        /// The technical information regarding the value.
        /// </param>
        /// <param name="validationResult">The result of the validation optionally preceding the update.</param>
        internal UpdateResult(bool initialValue, string validationMessage, string technicalInformation, BinaryResult validationResult)
            : base(initialValue, validationMessage)
        {
            ValidationResult = validationResult;
            if (!string.IsNullOrEmpty(technicalInformation))
            {
                technicalInformationBuilder.AppendLine(technicalInformation);
            }
        }

        /// <summary>
        /// Gets the successful validation result instance.
        /// </summary>
        public static UpdateResult Success
        {
            get
            {
                return new UpdateResult(true, string.Empty, string.Empty, BinaryResult.Positive);
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
        /// Gets the validation result.
        /// </summary>
        internal BinaryResult ValidationResult { get; }

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
        /// The new value that will be set (only if it is false).
        /// </param>
        /// <param name="validationMessage">
        /// A description of the appended validation result.
        /// </param>
        /// <param name="technicalInformation">
        /// The technical information regarding the validation result.
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
    }
}