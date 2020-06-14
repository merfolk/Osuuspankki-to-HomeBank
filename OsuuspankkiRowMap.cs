using CsvHelper.Configuration;

namespace osuuspankki_import
{
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
}
