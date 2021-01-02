using System;

namespace osuuspankki_import
{
    internal class OsuuspankkiRow {
        # pragma warning disable CS0649
        internal DateTime Date;
        internal Decimal Amount;
        internal int PaymentType;
        internal String PaymentTypeExplanation;
        internal String Payee;
        internal String PayeeAccount;
        internal String Reference;
        internal String Message;
        internal String ArchiveId;
        # pragma warning restore CS0649
    }
}
