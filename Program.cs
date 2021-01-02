using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace osuuspankki_import
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("\nError: Please give either a file name or an absolute directory path to transform.");
                Console.WriteLine("Eg. dotnet run new-file.csv");
                Console.WriteLine("Eg. dotnet run new-files");
                return;
            }

            var filesToImport = GetFilesToImport(args[0]);

            Console.WriteLine($"\nFound files ({filesToImport.Count()}):");
            foreach(var fileName in filesToImport)
            {
                Console.WriteLine(fileName);
            }

            Console.Write($"\nType 'yes' if you wish to transform all {filesToImport.Count()} files listed above: ");

            var input = Console.ReadLine();
            if (input != "yes") {
                return;
            }


            foreach(var fileName in filesToImport)
            {
                var transformedFileName = GetTransformedFileName(fileName);
                TransformCsv(fileName, transformedFileName);
            }
        }

        private static IEnumerable<string> GetFilesToImport(string searchPath) {
            var filesToImport = new List<string>();

            if (Directory.Exists(searchPath))
            {
                var path = searchPath;
                var files = Directory.GetFiles(path, "*.csv");
                filesToImport.AddRange(files);
            } else
            {
                var fileName = searchPath;
                filesToImport.Add(fileName);
            }

            return filesToImport.Select(file => Path.GetFullPath(file));

        }

        private static void TransformCsv(string fileName, string transformedFileName)
        {
            var configuration = new Configuration()
            {
                BadDataFound = (data) => Console.WriteLine("Error:" + data),
                Delimiter = ";"
            };

            configuration.RegisterClassMap<OsuuspankkiRowMap>();

            var rows = new List<OsuuspankkiRow>();

            Console.WriteLine($"Reading rows from {fileName}...\n");

            using (var streamReader = new StreamReader(fileName, Encoding.GetEncoding("ISO-8859-1")))
            using (var csvReader = new CsvReader(streamReader, configuration))
            {
                // Ignore header
                csvReader.Read();
                csvReader.ReadHeader();

                rows = csvReader.GetRecords<OsuuspankkiRow>().ToList();
            }

            foreach (var row in rows.Take(5))
            {
                Console.WriteLine($"{row.Date} {row.Amount} {row.Payee} {row.PaymentTypeExplanation}");
            }

            Console.WriteLine("...");
            Console.WriteLine($"\nRead {rows.Count} lines from {fileName}.");
            Console.WriteLine($"\nWriting to {transformedFileName}...\n");

            using (var stream = File.Create(transformedFileName))
            using (var streamWriter = new StreamWriter(stream))
            {
                foreach (var row in rows)
                {
                    var result = MapOsuuspankkiRowToHomebankRow(row);
                    Console.WriteLine(result);
                    streamWriter.WriteLine(result);
                }
            }

            Console.WriteLine();
        }

        private static string GetTransformedFileName(string fileName)
        {
            var splitFileName = fileName.Split('.');
            return splitFileName.First() + "-transformed" + "." + splitFileName.Last();
        }

        private static HomeBankImportRow MapOsuuspankkiRowToHomebankRow(OsuuspankkiRow OsuuspankkiRow) {
            var homebankRow = new HomeBankImportRow();

            homebankRow.Date = OsuuspankkiRow.Date;

            if( OsuuspankkiRow.PaymentTypeExplanation == "MAKSUPALVELU" ||
                OsuuspankkiRow.PaymentTypeExplanation == "TILISIIRTO") {
                homebankRow.Payment = PaymentType.Transfer;
            }
            else if(OsuuspankkiRow.PaymentTypeExplanation == "PKORTTIMAKSU") {
                homebankRow.Payment = PaymentType.DebitCard;
            }

            homebankRow.Amount = OsuuspankkiRow.Amount;
            homebankRow.Payee = OsuuspankkiRow.Payee;

            return homebankRow;
        }
    }
}
