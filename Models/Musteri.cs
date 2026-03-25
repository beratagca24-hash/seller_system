using SQLite;

namespace Saller_System.Models
{
    // VERESİYE İÇİN MÜŞTERİ MODELİ
    public class Musteri
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string AdSoyad { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public decimal ToplamBorc { get; set; } = 0;
    }

    // VERESİYE HAREKETLERİ MODELİ
    public class VeresiyeIslem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int MusteriId { get; set; }
        public decimal Tutar { get; set; }
        public DateTime Tarih { get; set; }
        public string Aciklama { get; set; } = string.Empty;
        public bool OdendiMi { get; set; } = false; // Borç mu eklendi, tahsilat mı yapıldı?
    }

    // TOPTAN SATIŞ MODELİ
    public class ToptanSatis
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string AliciFirma { get; set; } = string.Empty;
        public decimal ToplamTutar { get; set; }
        public decimal ToplamKg { get; set; }
        public DateTime Tarih { get; set; }
        public string OdemeYontemi { get; set; } = string.Empty; // Nakit, Veresiye, Kart
    }
}