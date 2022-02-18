using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VSProjectConsistencyChecker.Common.Resolvers
{
    public class ProjectConsistencyResolver
    {
        public string GenerateConsistencyAssesmentReport(string location, ref int totalScannedFiles, ref string outputFile, string[] extensions = null)
        {
            var builder = new StringBuilder();

            builder.AppendLine("Project,Configuration,Platform,Target,Output, Platforms Match, Expected output, Outputs Match");

            if (extensions == null)
            {
                extensions = new string[] { "*.vbproj", "*.csproj" };
            }

            foreach (var extension in extensions)
            {
                GenerateConsistencyAssesmentReportInternal(location, extension, builder, ref totalScannedFiles);
            }

            outputFile = $"Output_{DateTime.Now.ToString("yyyyMMddhhmmss")}.csv";

            File.WriteAllText(outputFile, builder.ToString().Replace("|", ","));
            
            TryOpenFile(outputFile);

            return builder.ToString().Replace("|", ",");
        }

        private static void TryOpenFile(string outputFile)
        {
            try
            {
                Process.Start(outputFile);
            }
            catch
            {
                // Handled exception to avoid breaking the application when output file cannot directly be opened
                // from here.
            }
        }

        private void GenerateConsistencyAssesmentReportInternal(string location, string extension, StringBuilder builder, ref int totalFiles)
        {
            string fileName;
            string configurationAndPlatform;
            string platformTarget;
            string outputPath;
            string platform;
            string expectedOutput;
            string configuration;

            foreach (var f in Directory.GetFiles(location, extension, SearchOption.AllDirectories))
            {
                var fileInfo = new FileInfo(f);

                using (var reader = new StreamReader(fileInfo.FullName))
                {
                    XDocument xDocument = XDocument.Load(reader);

                    var propertyGroupElements = xDocument.Root.Elements().Where(e => e.Name.LocalName == "PropertyGroup").ToList();

                    foreach (var propertyGroupElement in propertyGroupElements)
                    {
                        fileName = "";
                        configurationAndPlatform = "";
                        platformTarget = "";
                        outputPath = "";
                        platform = "";
                        expectedOutput = "";
                        configuration = "";

                        if (propertyGroupElement.HasAttributes && propertyGroupElement.Attributes().Any(a => a.Name.LocalName.Equals("Condition")))
                        {
                            fileName = fileInfo.Name;                            
                            configurationAndPlatform = propertyGroupElement.Attributes().First(
                                    a => a.Name.LocalName.Equals("Condition")).Value.Replace("'$(Configuration)|$(Platform)' == ", "").Replace("'", "").Trim();
                            builder.Append(fileName).Append("|").Append(configurationAndPlatform);

                            if (propertyGroupElement.Elements().Any(e => e.Name.LocalName == "PlatformTarget"))
                            {
                                platformTarget = propertyGroupElement.Elements().First(e => e.Name.LocalName == "PlatformTarget").Value;
                                builder.Append("|").Append(platformTarget);
                            }
                            else
                            {
                                builder.Append("|");
                            }

                            if (propertyGroupElement.Elements().Any(e => e.Name.LocalName == "OutputPath"))
                            {
                                outputPath = propertyGroupElement.Elements().First(e => e.Name.LocalName == "OutputPath").Value;
                                builder.Append("|").Append(outputPath);
                            }

                            // Verify whether platform matches with platform targetx
                            configuration = configurationAndPlatform.Split('|').First();
                            platform = configurationAndPlatform.Split('|').Last();

                            if ((string.IsNullOrWhiteSpace(platformTarget) && platform.Equals("AnyCPU")) || platformTarget.Equals(platform))
                            {
                                builder.Append("|").Append("CORRECT");
                            }
                            else
                            {
                                builder.Append("|").Append("INCORRECT");
                            }

                            // Verify whether output matches with expected output

                            expectedOutput = $@"bin\{(platform.Equals("AnyCPU") ? "" : $@"{platform}\")}{configuration}\";
                            builder.Append("|").Append(expectedOutput);

                            if (expectedOutput.Equals(outputPath))
                            {
                                builder.Append("|").Append("CORRECT");
                            }
                            else
                            {
                                builder.Append("|").Append("INCORRECT");
                            }

                            builder.AppendLine();
                        }
                    }
                }

                totalFiles++;
            }
        }
    }
}
