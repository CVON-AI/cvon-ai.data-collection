using Dicom;
using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Uploader.Logic.Config
{
    public class DicomTagConfiguration
    {
        [JsonIgnore]
        public DicomTag Tag { get; set; }

        public string TagString
        {
            get
            {
                // use a default...
                var tag = this.Tag ?? DicomTag.PatientName;
                return $"{tag.Group:X4},{tag.Element:X4}";
            }

            set
            {
                try
                {
                    ushort group;
                    ushort element;
                    string[] elements = value.Split(",");

                    //try
                    //{
                    //    element = Convert.ToUInt16(elements[1], NumberStyles.HexNumber)
                    //}
                    //catch (System.Exception ex)
                    //{
                    //    // todo: log
                    //    this.Tag = DicomTag.PatientName;
                    //}
                    if (elements.Length == 2 && ushort.TryParse(elements[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out group) && ushort.TryParse(elements[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out element))
                    {
                        this.Tag = new DicomTag(group, element);
                    }
                    else
                    {
                        // todo: log
                        this.Tag = DicomTag.PatientName;
                    }

                }
                catch (System.Exception)
                {

                    this.Tag = DicomTag.PatientName;
                }
            }
        }

        public SelectiveSettingValue SelectiveSetting { get; set; } = SelectiveSettingValue.Required;

        /// <summary>
        /// Gets or sets the pattern for a value, only applies to strings.
        /// Should be a regular expression, even for fixed values. E.g. if a value
        /// <c>LASTNAME</c> is required for patientname, define <c>^LASTNAME$</c> to
        /// enforce an exact match. For more generic patterns, alway start with <c>^</c>
        /// for start of input and end with <c>$</c> for end of input to enforce a full match
        /// </summary>
        public string ValuePattern { get; set; }

        // todo: min-max ranges for numeric fields?
        //public decimal MinValue { get; set; }

    }
}