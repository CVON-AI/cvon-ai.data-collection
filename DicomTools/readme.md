# Dicom validation and data extraction tools
## Purpose
This tool was developed with use of your DICOM data on cloud and cluster machines in mind. The purpose of the tool is to validate and extract DICOM metadata, to ensure 
no private data from patients ends up in your research datasets, and to limit your dataset contents to the bare minimum required for your research and to prevent personal
identifiability of individuals through your dataset; you may feel comfortable storing enriched, yet pseudonymized DICOM images on your institution's network, to work with your research data on a cloud based facility, you should aim to reduce the data 
you carry around to its absolute minimum to carry out your research. 

## Features
First, you can use the tool ensure, and to enforce that all DICOM files you use in a project contain a certain set of required DICOM metadata fields, called_tags_, 
do not contain another set of tags, and may optionally have a set of optional tags. Furthermore, you may specificy for textual tag values what regular expression
string pattern they must match, to ensure no personal data is stored in a field you carry around. In most studies, a pattern, or set of patterns are used for patient 
identifiers and patient names that are related to or named after the study, but generally do not relate to regular human names. Additionally, in multi-center studies, 
acquiring sites' names or codes may be included in patient - or rather - participant's identifiers and names as stored in the DICOM files.

## Configuration
### DICOM tag reference
Below is listed a minimum reference set of the most typical DICOM tags you may want to include in or exclude from your files and datasets, or that you may want to 
validate for personnally identifiable content. For a full reference of all available DICOM tags, please go to ... **add ref**

#### Patient tags
Tag|Attribute Name|Comments
---|-----|--------
0010,0010|Patient's Name|
0010,0020|Patient ID|
0010,0030|Patient's Birth Date|
0010,0040|Patient's Sex|

#### General imaging tags
Tag|Attribute Name|Comments
---|-----|--------
0028,0010|Rows|Number of rows in an image, or image height
0028,0011|Columns|Number of columns in an image, or image width
0028,0030|Pixel Spacing|Typically used in MR and CT images for pixel calibration. Ultrasound carries a different set of tags for pixel to metric space calibrations
