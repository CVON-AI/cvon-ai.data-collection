// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageValidationPropertyKeys.cs" company="UMCG - Thoraxcentrum">
//   (c) 2014, UMCG - Thoraxcentrum
// </copyright>
// <summary>
//   The image validation property keys.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Uploader.Logic.DicomProcessors
{
    /// <summary>
    /// The image validation property keys.
    /// </summary>
    internal sealed class ImageValidationPropertyKeys
    {
        /// <summary>
        /// Defines the key string for a reference Hash value, which can be used to compare with a server-side computed hash.
        /// </summary>
        public const string ReferenceHash = "ReferenceHash";
    }
}
