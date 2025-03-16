using IfcToolbox.Core.Utilities;
using IfcToolbox.Examples.Samples;
using IfcToolbox.Tests;
using IfcToolbox.Tools.Configurations;
using IfcToolbox.Tools.Helper;
using IfcToolbox.Tools.Processors;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xbim.Ifc;

namespace IfcToolbox.Examples.Batch
{
    public class ProcessTimeEstimation
    {
        public static List<string> GetAllTestFiles()
        {
            List<string> files = new List<string>();
            files.Add(LocalFiles.Ifc4_CubeAdvancedBrep); // NO site NO level
            files.Add(LocalFiles.Ifc4_WallElementedCase); // NO building
            files.Add(LocalFiles.Ifc4_Revit_ARC);
            files.Add(LocalFiles.Ifc4_Revit_STR); // NO building NO level
            files.Add(LocalFiles.Ifc4_Revit_MEP);
            files.Add(LocalFiles.Ifc2x3_Duplex_Architecture);
            files.Add(LocalFiles.Ifc2s3_Duplex_Electrical);
            files.Add(LocalFiles.Ifc2x3_Duplex_Mechanical);
            files.Add(LocalFiles.Ifc2x3_Duplex_Plumbing);
            files.Add(LocalFiles.Ifc2x3_Duplex_MEP);
            files.Add(LocalFiles.Ifc2x3_SampleCastle);
            files.Add(LocalFiles.Ifc4_SampleHouse);
            return files;
        }

        public static List<TimeReport> IfcSplitter_TimeEstimate(string outputFolder)
        {
            List<string> files = GetAllTestFiles();
            List<TimeReport> reports = new List<TimeReport>();
            foreach (var file in files)
            {
                var report = new TimeReport(file);
                var estimateTime = TimeEstimator.ForSplitterAsSeconds(file);
                var realTime = GetRealTime(file, outputFolder);
                report.AddProcessingTime(realTime, estimateTime);
                report.LogDetail();
                reports.Add(report);
            }
            reports.ToDataTable().SaveAsCsv(ConsoleFile.GetOutputFileName("IfcSplitter_TimeEstimate", outputFolder, "", ".csv"));
            return reports;

            double GetRealTime(string inputFileName, string outputFolder)
            {
                Log.Information($"IfcSplitter - Start");
                string copiedIfcFile = ConsoleFile.GetOutputFileName(inputFileName, outputFolder);
                ConsoleFile.CreateCopyIfcFile(inputFileName, copiedIfcFile);

                Stopwatch Watch = new Stopwatch();
                Watch.Start();
                TimeSpan Start = Watch.Elapsed;

                IConfigSplit config = ConfigFactory.CreateConfigSplit();
                config.SplitStrategy = SplitStrategy.DataOnly;
                using (var model = IfcStore.Open(copiedIfcFile))
                    SplitterProcessor.Split(model, config, copiedIfcFile);

                TimeSpan elapsed = Watch.Elapsed - Start;
                return elapsed.TotalSeconds;
            }
        }
    }
}
