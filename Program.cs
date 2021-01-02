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
                Console.WriteLine("Error: Please give either a file name or an absolute directory path to transform.");
                return;
            }

            var filesToImport = new List<string>();

            if (Directory.Exists(args[0]))
            {
                var path = args[0];
                var files = Directory.GetFiles(path, "*.csv");
                filesToImport.AddRange(files);
            } else
            {
                var fileName = args[0];
                filesToImport.Add(fileName);
            }

            var pathsToImport = filesToImport.Select(file => Path.GetFullPath(file));

            Console.WriteLine($"Found files ({pathsToImport.Count()}):");
            foreach(var fileName in pathsToImport)
            {
                Console.WriteLine(fileName);
            }

            Console.WriteLine($"\nType 'yes' if you wish to transform all {pathsToImport.Count()} files listed above.");

            var input = Console.ReadLine();
            if (input != "yes") {
                return;
            }


            foreach(var fileName in pathsToImport)
            {
                var transformedFileName = GetTransformedFileName(fileName);
                TransformCsv(fileName, transformedFileName);
            }
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

            Console.WriteLine($"Reading rows from {fileName}...");
            Console.WriteLine();

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
            Console.WriteLine();
            Console.WriteLine($"Read {rows.Count} lines from {fileName}.");
            Console.WriteLine();
            Console.WriteLine($"Writing to {transformedFileName}...");
            Console.WriteLine();

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
