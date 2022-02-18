using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VSProjectConsistencyChecker.Common.Resolvers;

namespace VSProjectConsistencyChecker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                int totalScannedFiles = 0;
                string outputFile = "";
                ProjectConsistencyResolver resolver = new ProjectConsistencyResolver();
                string output = resolver.GenerateConsistencyAssesmentReport(args[1], ref totalScannedFiles, ref outputFile);
                Console.WriteLine(output.Replace(",", " "));
                Console.WriteLine($"Total scanned projects: {totalScannedFiles}");
                Console.WriteLine($"Assesment report: {Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), outputFile)}");
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex);
                Console.WriteLine();
                Console.WriteLine("Usage:");
                Console.WriteLine("VSProjectConsistencyChecker -path <The path where projects are located>");
            }
            finally
            {
                Console.WriteLine("Press any key to close.");
                Console.ReadKey();
            }
        }
    }
}
