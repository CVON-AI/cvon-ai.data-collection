using Dicom;
using Dicom.Imaging;
using Dicom.Imaging.Render;
using Dicom.IO.Buffer;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Uploader.Logic.Config;

namespace Uploader.Logic.DicomProcessors
{
    internal class PixelBlinder : DicomFileValidatorBase, IDicomImageProcessor
    {
        public PixelBlinder(Site site, Modality modality) : base(site, modality)
        {
        }

        public override bool StopValidationOnFailure => false;

        public override BinaryResult Validate(string imageFileName, DicomFile file)
        {
            return BinaryResult.Positive;
        }

        public override BinaryResult ApplyCorrection(string imageFileName, DicomFile file)
        {
            try
            {
                // get the current image modality configuration with blinding information in it
                ImageModalityConfiguration config = CurrentModalityConfiguration;
                var areas = config.BlindedAreas;

                if (areas != null && areas.Any())
                {
                    uint frameCount;

                    try
                    {
                        frameCount = file.Dataset.GetSingleValue<uint>(DicomTag.NumberOfFrames);
                    }
                    catch (Exception)
                    {
                        frameCount = 1;
                    }

                    for (int frame = 0; frame < frameCount; frame++)
                    {
                        foreach (var area in areas)
                        {
                            int width = area.Width;
                            int height = area.Height;
                            int left = area.Left;
                            int top = area.Top;

                            IPixelData pixelData = PixelDataFactory.Create(DicomPixelData.Create(file.Dataset), frame);

                            // Get Raw Data
                            //TODO: make GENERIC, CAN BE 16BIT as well!!
                            ushort[] originalRawBytes = ((GrayscalePixelDataU16)pixelData).Data; // .GetFrame(0).Data; // byte[] ColorPixelData24
                            Dicom.Imaging.IImage image = new DicomImage(file.Dataset).RenderImage(frame); //.As<ImageSharpImage>;
                            //using (MemoryStream readStream = new MemoryStream(image.AsBytes()))
                            {
                                //using (Image bmp = new Bitmap(image.As<Bitmap>()))
                                var format = new JpegImageFormatDetector().DetectFormat(new ReadOnlySpan<byte>(image.AsBytes()));

                                // ?? https://stackoverflow.com/questions/46807283/detect-picture-format-with-sixlabors-imagesharp
                                using (var bmp = Image.Load(image.AsBytes(), out format)) // new PngDecoder())) // new BmpDecoder())) //, new JpegDecoder())) //.FromStream(readStream)) // new Bitmap(new DicomImage(file.Dataset).RenderImage(frame).As<Image>()))
                                // INCOMING IN FAILING MERGE - FROM UMCG: using (Image bmp = Image.Load(image.AsBytes(), new JpegDecoder())) //.FromStream(readStream)) // new Bitmap(new DicomImage(file.Dataset).RenderImage(frame).As<Image>()))
                                {
                                    bmp.Mutate(x => x.Fill(Color.Black, new RectangleF(left, top, width, height)));
                                    //using (Graphics gr = Graphics.FromImage(bmp))
                                    //{
                                    //    gr.FillRectangle(Brushes.Black, left, top, width, height);
                                    //}


                                    using (var stream = new MemoryStream())
                                    {
                                        bmp.Save(stream, new JpegEncoder());
                                        MemoryByteBuffer modified = new MemoryByteBuffer(stream.ToArray());

                                        // Write back modified pixel data
                                        file.Dataset.AddOrUpdatePixelData(DicomVR.OB, modified);
                                    }

                                }
                            }
                            // Create new array with modified data
                            //byte[] modifiedRawBytes = new byte[originalRawBytes.Length];
                            //for (int i = 0; i < originalRawBytes.Length; i++)
                            //{
                            //    modifiedRawBytes[i] = (byte)(originalRawBytes[i] + 100);
                            //}

                            // Create new buffer supporting IByteBuffer to contain the modified data
                            //MemoryByteBuffer modified = new MemoryByteBuffer(modifiedRawBytes);

                            //// Write back modified pixel data
                            //file.Dataset.AddOrUpdatePixelData(DicomVR.OB, modified);

                            ////Image image = new DicomImage(file.Dataset).RenderImage(frame).As<Image>(); 
                            ////Graphics gr = Graphics.FromImage(image);
                            ////gr.FillRectangle(Brushes.Black, left, top, width, height);
                            ////file.Dataset.AddOrUpdatePixelData(DicomVR.OB, )

                            //var image = new DicomImage(file.Dataset).RenderImage(frame); // .As<Bitmap>();
                            //IPixelData data = PixelDataFactory.Create(DicomPixelData.Create(file.Dataset), frame);
                            //ImageGraphic graphic = new ImageGraphic(data);
                            //MemoryByteBuffer byteBuffer = new MemoryByteBuffer(image.AsBytes());

                            //new SingleBitPixelData()
                            ////DicomPixelData.Create(file.Dataset)
                            //graphic.AddOverlay(new OverlayGraphic(SingleBitPixelData(width, height, byteBuffer), left, top, 0));

                            //image.DrawGraphics(new List<IGraphic> { graphic });

                            // file.Dataset.AddOrUpdatePixelData(DicomVR.OB, byteBuffer);
                        }
                        //var frameData = file.
                    }
                }

                return BinaryResult.Positive;
            }
            catch (Exception ex)
            {
                
                base.MoveFile(imageFileName);
                return new BinaryResult(false, $"Exception occurred blinding {imageFileName}", $"Exception occurred blinding {imageFileName}: {ex.Message},{ex.StackTrace}");
                // todo : log;
            }
        }
    }
}
