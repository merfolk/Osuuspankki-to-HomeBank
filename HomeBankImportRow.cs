using System;
using System.Text.RegularExpressions;

namespace osuuspankki_import
{
    class HomeBankImportRow {
        # pragma warning disable CS0649
        internal DateTime Date;
        internal PaymentType Payment;
        internal String Info;
        internal String Payee;
        internal String Memo;
        internal Decimal Amount;
        internal String Category;
        internal String Tags;
        # pragma warning restore CS0649

        override public String ToString() {
            var formattedDate = $"{Date.Year}-{Date.Month.ToString().PadLeft(2, '0')}-{Date.Day.ToString().PadLeft(2, '0')}";

            var trimmedPayee = Regex.Replace(Payee, @"\s+", " ");

            return $"{formattedDate};{(int)Payment};{Info};{trimmedPayee};{Memo};{Amount};{Category};{Tags}";
        }
    }
}
