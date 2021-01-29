// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DicomModality.cs" company="UMCG - Thoraxcentrum">
//   (c) 2014, UMCG - Thoraxcentrum
// </copyright>
// <summary>
//   Contains a set of predefined <![CDATA[dicom]]> modality codes.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Uploader.Logic.DicomProcessors
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Contains a set of predefined <![CDATA[dicom]]> modality codes.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1027:TabsMustNotBeUsed", Justification = "Reviewed. Suppression is OK here.")]
    internal sealed class DicomModality
    {
        /*
        AU	Audio
        BI	Biomagnetic Imaging
        CD	Color flow Doppler
        CR	Computed radiography
        CT	Computed tomography
        DD	Duplex Doppler
        DG	Diaphanography
        DSA	Digital Subtraction Angiography
        DX	Digital Radiography
        ECG	Electrocardiography
        EPS	Cardiac Electrophysiology
        ES	Endoscopy
        GM	General Microscopy
        HC	Hard Copy
        HD	Hemodynamic Waveform
        IO	Intra-Oral Radiography
        IVUS	Intravascular Ultrasound
        LS	Laser surface scan
        MG	Mammography
        MR	Magnetic Resonance
        NM	Nuclear Medicine
 	 
        Value	Description
        OCT	Optical Coherence Tomography
        OP	Ophthalmic Photography
        OPM	Ophthalmic Mapping
        OPR	Ophthalmic Refraction
        OPV	Ophthalmic Visual Field
        OT	Other
 	 
        Value	Description
        PR	Presentation State
        PET	Positron Emission Tomography - PET
        PX	Panoramic X-Ray
        REG	Registration
 	 
        Value	Description
        RF	Radio Fluoroscopy
        RG	Radiographic imaging (conventional film/screen)
        RTDOSE	Radiotherapy Dose
        RTIMAGE	Radiotherapy Image
        RTPLAN	Radiotherapy Plan
        RTRECORD	RT Treatment Record
        RTSTRUCT	Radiotherapy Structure Set
 	 
        Value	Description
        SEG	Segmentation
        SM	Slide Microscopy
        SMR	Stereometric Relationship
        SR	SR Document
        ST	Single-photon emission computed tomography (SPECT)
 	 
        Value	Description
        TG	Thermography
        US	Ultrasound
        XA	X-Ray Angiography
        XC	External-camera photography
        */

        /// <summary>
        /// Defines the 'Other' modality, which
        /// is equivalent to any modality, not otherwise specified
        /// </summary>
        internal const string Other = "OT";

        /// <summary>
        /// Defines the 'Ultrasound' modality
        /// </summary>
        internal const string Ultrasound = "US";

        /// <summary>
        /// Defines the computed tomography modality
        /// </summary>
        internal const string Ct = "CT";

        /// <summary>
        /// Defines the magnetic resonance modality
        /// </summary>
        internal const string MRI = "MR";

    }
}