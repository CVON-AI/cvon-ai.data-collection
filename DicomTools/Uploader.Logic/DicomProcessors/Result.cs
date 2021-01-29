// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Result.cs" company="UMCG - Thoraxcentrum">
//   (c) 2014, UMCG - Thoraxcentrum
// </copyright>
// <summary>
//   The generic result type.
//   Allows to specify why a piece of application logic
//   resulted in a given <see cref="Value" />.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Uploader.Logic.DicomProcessors
{
    using System.Text;

    /// <summary>
    /// The generic result type.
    /// Allows to specify why a piece of application logic
    /// resulted in a given <see cref="Value"/>.
    /// </summary>
    /// <typeparam name="TResultType">The type of the value created by the application.</typeparam>
    public class Result<TResultType>
    {
        #region Fields (2) 

        /// <summary>
        /// The information message-builder.
        /// </summary>
        protected readonly StringBuilder InformationStringBuilder = new StringBuilder();

        #endregion Fields 

        #region Constructors (1) 

        /// <summary>
        /// Initializes a new instance of the <see cref="Result{TResultType}"/> class. 
        /// </summary>
        /// <param name="initialValue">A value representing the initial value.</param>
        /// <param name="validationMessage">A description of why the given data has been marked valid or not.</param>
        public Result(TResultType initialValue, string validationMessage)
        {
            Value = initialValue;
            if (!string.IsNullOrEmpty(validationMessage))
            {
                InformationStringBuilder.AppendLine(validationMessage);
            }
        }

        #endregion Constructors 

        #region Properties (2) 

        /// <summary>
        /// Gets the validation message.
        /// </summary>
        /// <remarks>
        /// Primarily comes down to a description of the reason(s) of failure in case of failure.
        /// </remarks>
        internal string Information
        {
            get { return InformationStringBuilder.ToString(); }
        }

        /// <summary>
        /// Gets or sets the result value.
        /// </summary>
        protected internal TResultType Value { get; protected set; }
        #endregion Properties 

        /// <summary>
        /// Method to append a validation message. Does not change the value.
        /// </summary>
        /// <param name="validationMessage">A description of the appended validation result.</param>
        /// <remarks>Sets <see cref="Value"/> to <code>false</code> if <paramref name="validationMessage"/> is <code>false</code>.</remarks>
        internal void AppendInfo(string validationMessage)
        {
            InformationStringBuilder.AppendLine(validationMessage);
        }

        /// <summary>
        /// Method to append a validation message and change the result.
        /// </summary>
        /// <param name="newValue">The new value that will be set.</param>
        /// <param name="validationMessage">A description of the appended validation result.</param>
        /// <remarks>Might be altered in inheriting classes.</remarks>
        internal virtual void AppendValue(TResultType newValue, string validationMessage)
        {
            Value = newValue;
            InformationStringBuilder.AppendLine(validationMessage);
        }
    }
}