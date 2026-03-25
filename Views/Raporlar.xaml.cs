using Saller_System.Services;
using Saller_System.Models;

namespace Saller_System.Views
{
    public partial class Raporlar : ContentPage
    {
        private readonly DatabaseService _db;
        private readonly ExcelServisi _excel;
        private readonly AyarlarServisi _ayarlar;

        public Raporlar(DatabaseService db, ExcelServisi excel, AyarlarServisi ayarlar)
        {
            InitializeComponent();
            _db = db;
            _excel = excel;
            _ayarlar = ayarlar;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (await ZamanAsimKontrolAsync()) return;

            OturumServisi.AktiviteYenile();

            var magazaAdi = await _ayarlar.GetAsync("MagazaAdi", "");
            if (!string.IsNullOrWhiteSpace(magazaAdi))
                RaporBaslikLabel.Text = $"{magazaAdi} — Finans";

            await VerileriYukle();
        }

        private async Task<bool> ZamanAsimKontrolAsync()
        {
            if (!OturumServisi.OturumSuresiDolduMu()) return false;

            OturumServisi.Cikis();
            await DisplayAlert("Oturum Süresi Doldu", "Güvenlik nedeniyle oturumunuz sonlandırıldı.", "Tamam");
            await Shell.Current.GoToAsync("//LoginPage");
            return true;
        }

        private async Task VerileriYukle()
        {
            await _db.InitAsync();
            var bugun = DateTime.Today;

            var gunlukKar = await _db.GunlukKarAsync(bugun);
            // 1. DÜZELTME: Sadece gerçek ciro (Tahsilatlar Hariç) hesaplanır
            var gunlukCiro = await _db.GunlukGercekCiroAsync(bugun);

            var gunlukSayi = await _db.GunlukSatisSayisiAsync(bugun);
            var aylikCiro = await _db.AylikCiroAsync(bugun.Year, bugun.Month);
            var performans = await _db.PersonelPerformansRaporuGetirAsync(bugun);

            // 2. YENİ EKLENEN: KASA MATEMATİĞİ (Çift sayım önleyici)
            decimal tahsilat = await _db.GunlukTahsilatToplamiAsync(bugun);
            var veresiyeIslemleri = await _db.GunlukVeresiyeDetaylariAsync(bugun);
            decimal veresiyeCikan = veresiyeIslemleri.Where(v => v.Tutar > 0).Sum(v => v.Tutar);
            decimal netKasa = gunlukCiro - veresiyeCikan + tahsilat;

            PersonelPerformansListesi.ItemsSource = performans;
            GunlukKarLabel.Text = $"₺{gunlukKar:N2}";
            // AylikKarLabel XAML tasarımında olmadığı için bu satır SİLİNDİ.
            GunlukSayiLabel.Text = $"{gunlukSayi} Adet";
            AylikCiroLabel.Text = $"₺{aylikCiro:N2}";
            AyLabel.Text = bugun.ToString("MMMM yyyy").ToUpper();

            // Kasa Değerleri
            GunlukCiroLabel.Text = $"₺{gunlukCiro:N2}";
            VeresiyeCikanLabel.Text = $"- ₺{veresiyeCikan:N2}";
            TahsilatGirenLabel.Text = $"+ ₺{tahsilat:N2}";
            NetKasaLabel.Text = $"₺{netKasa:N2}";
        }

        private async void SatisGecmisiClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//SatisGecmisiSayfa");
        }

        private async void ExcelAktarClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            var satislar = await _db.GunlukSatislerAsync(DateTime.Today);
            if (satislar.Count == 0) return;

            // Excel Servisine "tarih" bağlantısını gönderiyoruz ki veresiye datalarını da çekebilsin
            string dosyaYolu = await _excel.RaporOlustur(satislar, "Gunluk_Rapor", DateTime.Today);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Rapor",
                File = new ShareFile(dosyaYolu)
            });
        }

        private async void GeriClicked(object sender, EventArgs e)
        {
            OturumServisi.AktiviteYenile();
            await Shell.Current.GoToAsync("//AnaSayfa");
        }
    }
}