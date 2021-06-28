using Dicom;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Uploader.Logic.Config;
using Uploader.Logic.Controllers;
using Uploader.Logic.Providers;

namespace ConfigBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            ProjectSettings settings = new ProjectSettings
            {
                Name = "CVON-AI FS1 - RACE-V",
                RemotePath = "gs://cvon-ai-data",

                Modalities = new List<Modality> { Modality.Echo, Modality.CT },

                Sites = new List<Site> {
                    new Site
                    {
                        Name = "MZH",
                        ImageModalityConfig = new List<ImageModalityConfiguration>
                        {
                            new ImageModalityConfiguration
                            {
                                Modality = Modality.Echo
                            }
                        }
                    },
                    new Site
                    {
                        Name = "Rijnstate",
                        ImageModalityConfig = new List<ImageModalityConfiguration>
                        {
                            new ImageModalityConfiguration
                            {
                                Modality = Modality.Echo,

                                BlindedAreas = new List<PixelArea>
                                {
                                    new PixelArea { Left = 40, Top = 40, Width = 200, Height = 50}
                                }
                            }
                        }
                    },
                    new Site
                    {
                        Name = "AMC",
                        ImageModalityConfig = new List<ImageModalityConfiguration>
                        {
                            new ImageModalityConfiguration
                            {
                                Modality = Modality.Echo,
                                DataExportPath = @"D:\RACE-V\",
                                RemotePath = "${project_path}/RACE-V/Echo",
                                UploadControllerType = typeof(GoogleUploadController),
                                DicomTags = new List<DicomTagConfiguration>
                                {
                                    new DicomTagConfiguration
                                    {
                                        SelectiveSetting = SelectiveSettingValue.Required,
                                        Tag = DicomTag.PatientName,
                                        ValuePattern = "^RACEV$"
                                    },
                                    new DicomTagConfiguration
                                    {
                                        SelectiveSetting = SelectiveSettingValue.Required,
                                        Tag = DicomTag.PatientID,
                                        ValuePattern = "^06\\d{3}$"
                                    },
                                }
                            }
                        }
                    }
                }

            };

            // Use writeintended for whitespaces
            (DependencyInjector.Resolve<IIOController>() ?? new IOControllerWrapper()).WriteLine(new ProjectSettingsProvider().GetJson(settings));
            Console.ReadKey(true);
        }
    }
}
