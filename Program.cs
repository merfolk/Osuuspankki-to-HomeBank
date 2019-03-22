using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;

namespace osuuspankki_import
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0) {
                Console.WriteLine("Error: Please give the file name to transform.");
                return;
            }

            var fileName = args[0];

            var splitFileName = fileName.Split('.');
            var transformedFileName = splitFileName.First() + "-transformed" + "." + splitFileName.Last();

            var configuration = new Configuration() {
                BadDataFound = (data) => Console.WriteLine("Error:" + data),
                Delimiter = ";"
            };

            configuration.RegisterClassMap<OsuuspankkiRowMap>();

            var rows = new List<OsuuspankkiRow>();

            Console.WriteLine($"Reading rows from {fileName}...");
            Console.WriteLine();

            using(var streamReader = new StreamReader(fileName, Encoding.GetEncoding("ISO-8859-1")))
            using(var csvReader = new CsvReader(streamReader, configuration)) {
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

            using(var stream = File.Create(transformedFileName))
            using(var streamWriter = new StreamWriter(stream)) {
                foreach(var row in rows) {
                    var result = MapOsuuspankkiRowToHomebankRow(row);
                    Console.WriteLine(result);
                    streamWriter.WriteLine(result);
                }
            }

            Console.WriteLine();
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

    internal class OsuuspankkiRow {
        internal DateTime Date;
        internal Decimal Amount;
        internal int PaymentType;
        internal String PaymentTypeExplanation;
        internal String Payee;
        internal String PayeeAccount;
        internal String Reference;
        internal String Message;
        internal String ArchiveId;
    }

    class OsuuspankkiRowMap : ClassMap<OsuuspankkiRow> {
        public OsuuspankkiRowMap() {
            Map(m => m.Date).Index(0); //Name("Kirjauspäivä");
            // Skip "Arvopäivä"
            Map(m => m.Amount).Index(2); // Name("Määrä  EUROA");
            Map(m => m.PaymentType).Index(3); //Name("Laji");
            Map(m => m.PaymentTypeExplanation).Index(4); //.Name("Selitys");
            Map(m => m.Payee).Index(5); // Name("Saaja/Maksaja");
            Map(m => m.PayeeAccount).Index(6); // .Name("Saajan tilinumero ja pankin BIC");
            Map(m => m.Reference).Index(7); //.Name("Viite");
            Map(m => m.Message).Index(8); //.Name("Viesti");
            Map(m => m.ArchiveId).Index(9); //.Name("Arkistointitunnus");

        }
    }

    class HomeBankImportRow {
        internal DateTime Date;
        internal PaymentType Payment;
        internal String Info;
        internal String Payee;
        internal String Memo;
        internal Decimal Amount;
        internal String Category;
        internal String Tags;

        override public String ToString() {
            var formattedDate = $"{Date.Year}-{Date.Month.ToString().PadLeft(2, '0')}-{Date.Day.ToString().PadLeft(2, '0')}";

            var trimmedPayee = Regex.Replace(Payee, @"\s+", " ");

            return $"{formattedDate};{(int)Payment};{Info};{trimmedPayee};{Memo};{Amount};{Category};{Tags}";
        }
    }

    internal enum PaymentType {
        None = 0,
        CreditCard = 1,
        //Cheque = 2,
        Cash = 3,
        Transfer = 4,
        //Internal Transfer = 5 // does not work
        DebitCard = 6,
        // Standing Order = 7,
        //Electronic Payment = 8,
        Deposit = 9,
        //FI_fee = 10,
        //DirectDebit = 11

    }
}
