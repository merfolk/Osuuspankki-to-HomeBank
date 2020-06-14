namespace osuuspankki_import
{
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
