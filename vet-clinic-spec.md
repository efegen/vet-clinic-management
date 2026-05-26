# Veteriner Klinik Randevu Yönetim Sistemi — Teknik Spesifikasyon

**Sürüm:** 1.0
**Doküman türü:** Geliştirme spesifikasyonu (standalone)
**Hedef platform:** ASP.NET Core 8 — Razor MVC

---

## İçindekiler

1. Proje özeti
2. Gerçek dünya problem tanımı
3. Teknoloji yığını ve mimari
4. Proje klasör yapısı
5. Veri modeli (entity'ler)
6. ViewModel katalogu
7. Repository katmanı
8. Service katmanı (business logic)
9. Controller katmanı
10. View katmanı
11. Dependency Injection kayıtları
12. Migration adımları
13. Seed data
14. UML sınıf diyagramı içeriği
15. Rapor outline (10–15 sayfa)
16. README taslağı

---

## 1. Proje Özeti

Veteriner Klinik Randevu Yönetim Sistemi, küçük ve orta ölçekli veteriner kliniklerinin günlük operasyonlarını yönetmek için tasarlanmış, iç kullanıma yönelik (internal tool) bir web uygulamasıdır. Sistem; klinik personelinin (resepsiyonist, veteriner hekim, klinik yöneticisi) hayvan sahiplerini, evcil hayvanları, sunulan hizmetleri ve randevuları tek bir merkezi panelden yönetmesini sağlar.

Uygulamada müşteri tarafına açık bir public arayüz **bulunmamaktadır**. Tüm CRUD işlemleri yalnızca klinik personeli tarafından, klinik içi ağda çalışan panel üzerinden gerçekleştirilir. Kimlik doğrulama / yetkilendirme katmanı **kapsam dışıdır**; sistem güvenli bir LAN ortamında konumlandırıldığı varsayılır.

Sistemin temel sorumlulukları:

- Hayvan sahibi (Owner) kayıtlarının yönetimi
- Sahiplere bağlı evcil hayvan (Pet) kayıtlarının yönetimi
- Klinikte sunulan hizmetlerin (Service) tanımlanması
- Randevu (Appointment) oluşturma, güncelleme, iptal etme
- Çakışma kontrolü, tarih validasyonu, durum (status) yönetimi
- Anasayfa dashboard üzerinden günlük ve haftalık özet

---

## 2. Gerçek Dünya Problem Tanımı

Küçük ölçekli veteriner kliniklerinde randevu yönetimi genellikle kâğıt ajandalar, duvar takvimleri veya basit Excel dosyalarıyla yürütülür. Bu yöntemler aşağıdaki problemleri doğurur:

**P1 — Randevu çakışmaları:** Aynı saat dilimine farkında olmadan birden fazla randevu yazılabilir; bu hem hekim hem müşteri için zaman kaybına yol açar.

**P2 — Hayvan geçmişi parçalı:** Bir köpek üç ay sonra tekrar geldiğinde, son ziyaretinde hangi hizmeti aldığı, hangi hekime gittiği kâğıt karıştırılarak bulunur.

**P3 — Müşteri iletişim bilgilerinin dağınıklığı:** Telefon numaraları farklı yerlerde, farklı formatlarda tutulur. Aynı sahibe ait birden fazla kayıt oluşabilir.

**P4 — Hizmet ve fiyat standardizasyonu yok:** "Aşı" denen hizmetin fiyatı kimi zaman 300 TL, kimi zaman 350 TL yazılır. Klinik içi tutarlılık sağlanamaz.

**P5 — Raporlama imkânsız:** "Bu hafta kaç randevu yapıldı?", "En çok talep gören hizmet hangisi?" gibi sorulara yanıt vermek için tüm kayıtlar elle taranır.

Bu sistem; **sahip → hayvan → hizmet → randevu** zincirini tek bir relational veritabanında birleştirerek, çakışma kontrolü ve tarih validasyonu gibi iş kurallarını uygulama katmanında otomatikleştirerek bu problemleri çözmeyi hedefler.

---

## 3. Teknoloji Yığını ve Mimari

### 3.1. Teknoloji yığını

| Katman | Teknoloji |
|---|---|
| Framework | ASP.NET Core 8.0 |
| UI | Razor View Engine (MVC) |
| CSS Framework | Bootstrap 5.3 |
| ORM | Entity Framework Core 8 (Code First) |
| Veritabanı | SQLite |
| Validation | Data Annotations |
| Logging | Built-in `ILogger<T>` |

### 3.2. Katmanlı mimari

İletişim yönü her zaman yukarıdan aşağıya doğrudur:

```
Controllers  →  Services  →  Repositories  →  DbContext  →  SQLite
```

**Katmanların sorumlulukları:**

- **Controller:** HTTP isteklerini karşılar; ViewModel ↔ Entity dönüşümünü yapar; sadece Service'leri çağırır. **DbContext'i ASLA doğrudan kullanmaz.** Thin Controller prensibine uyulur (action başına maks. ~15 satır).
- **Service:** İş kurallarını uygular: tarih validasyonu, çakışma kontrolü, yaş hesaplama, durum geçişleri. View döndürmez, HTTP bilmez.
- **Repository:** Veri erişimi. LINQ sorguları burada yazılır. İş kuralı içermez; sadece veri okur/yazar.
- **DbContext:** EF Core'un DbSet'leri ve `OnModelCreating` ilişki yapılandırması.

### 3.3. Ek mimari kararlar

- **Dependency Injection:** Tüm Repository ve Service'ler interface üzerinden enjekte edilir (`IOwnerRepository`, `IAppointmentService` gibi).
- **ViewModel pattern:** View'lara hiçbir zaman Entity gönderilmez; mutlaka ViewModel ile gönderilir.
- **AutoMapper kullanılmayacaktır.** ViewModel ↔ Entity dönüşümü manuel yapılır (eğitim amaçlı şeffaflık için).
- **Async/await:** Tüm DB erişimleri `async` olarak yazılır (`ToListAsync`, `FirstOrDefaultAsync`, `SaveChangesAsync`).

---

## 4. Proje Klasör Yapısı

Tek proje (single project) yapısı tercih edilir; klasörlerle mantıksal ayrım sağlanır:

```
VetClinic/
├── VetClinic.sln
├── VetClinic.Web/
│   ├── Controllers/
│   │   ├── HomeController.cs
│   │   ├── OwnersController.cs
│   │   ├── PetsController.cs
│   │   ├── ServicesController.cs
│   │   └── AppointmentsController.cs
│   ├── Models/
│   │   ├── Entities/
│   │   │   ├── Owner.cs
│   │   │   ├── Pet.cs
│   │   │   ├── Service.cs
│   │   │   └── Appointment.cs
│   │   └── Enums/
│   │       ├── PetSpecies.cs
│   │       ├── PetGender.cs
│   │       └── AppointmentStatus.cs
│   ├── ViewModels/
│   │   ├── Owners/
│   │   │   ├── OwnerListViewModel.cs
│   │   │   ├── OwnerDetailsViewModel.cs
│   │   │   └── OwnerCreateEditViewModel.cs
│   │   ├── Pets/
│   │   ├── Services/
│   │   ├── Appointments/
│   │   └── Dashboard/
│   │       └── DashboardViewModel.cs
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   ├── Migrations/
│   │   └── SeedData.cs
│   ├── Repositories/
│   │   ├── Interfaces/
│   │   │   ├── IRepository.cs              (generic)
│   │   │   ├── IOwnerRepository.cs
│   │   │   ├── IPetRepository.cs
│   │   │   ├── IServiceRepository.cs
│   │   │   └── IAppointmentRepository.cs
│   │   └── Implementations/
│   │       ├── Repository.cs               (generic, optional base)
│   │       ├── OwnerRepository.cs
│   │       ├── PetRepository.cs
│   │       ├── ServiceRepository.cs
│   │       └── AppointmentRepository.cs
│   ├── Services/
│   │   ├── Interfaces/
│   │   │   ├── IOwnerService.cs
│   │   │   ├── IPetService.cs
│   │   │   ├── IServiceCatalogService.cs
│   │   │   ├── IAppointmentService.cs
│   │   │   └── IDashboardService.cs
│   │   └── Implementations/
│   │       └── ... (her interface için karşılığı)
│   ├── Views/
│   │   ├── Shared/
│   │   │   ├── _Layout.cshtml
│   │   │   ├── _ValidationScriptsPartial.cshtml
│   │   │   └── Error.cshtml
│   │   ├── Home/
│   │   │   └── Index.cshtml
│   │   ├── Owners/
│   │   │   ├── Index.cshtml
│   │   │   ├── Details.cshtml
│   │   │   ├── Create.cshtml
│   │   │   ├── Edit.cshtml
│   │   │   └── Delete.cshtml
│   │   ├── Pets/
│   │   ├── Services/
│   │   └── Appointments/
│   ├── wwwroot/
│   │   ├── css/
│   │   ├── js/
│   │   └── lib/ (Bootstrap, jQuery, jQuery Validation)
│   ├── appsettings.json
│   ├── Program.cs
│   └── VetClinic.Web.csproj
└── README.md
```

---

## 5. Veri Modeli (Entity'ler)

### 5.1. Owner (Hayvan Sahibi)

| Alan | Tip | Validation | Açıklama |
|---|---|---|---|
| `Id` | `int` | PK, Identity | Birincil anahtar |
| `FullName` | `string` | `[Required]`, `[StringLength(100, MinimumLength = 2)]` | Ad soyad |
| `Phone` | `string` | `[Required]`, `[RegularExpression(@"^(\+90|0)?5\d{9}$")]` | TR mobil telefon formatı |
| `Email` | `string?` | `[EmailAddress]`, `[StringLength(150)]` | Opsiyonel |
| `Address` | `string?` | `[StringLength(250)]` | Opsiyonel |
| `CreatedAt` | `DateTime` | Service katmanında `DateTime.Now` ile set edilir | Kayıt tarihi |

**Navigation:**
- `ICollection<Pet> Pets` — Bir sahibin birden çok hayvanı olabilir (1-N).

> **`CreatedAt` set noktası kararı:** Entity constructor'ında veya `DbContext.SaveChanges` override'ında değil, **`IOwnerService.CreateAsync` içinde** set edilir. Gerekçe: test edilebilirlik (ileride `IClock` / `IDateTimeProvider` enjekte edilerek deterministik testler yazılabilir) ve sorumluluk netliği (audit field'ı iş katmanına aittir, DB ya da entity'nin değil). Aynı kural `Appointment.CreatedAt` için de geçerlidir.

### 5.2. Pet (Evcil Hayvan)

| Alan | Tip | Validation | Açıklama |
|---|---|---|---|
| `Id` | `int` | PK, Identity | |
| `Name` | `string` | `[Required]`, `[StringLength(50, MinimumLength = 1)]` | Hayvanın adı |
| `Species` | `PetSpecies` (enum) | `[Required]` | Köpek, Kedi, Kuş, Tavşan, Diğer |
| `Breed` | `string?` | `[StringLength(50)]` | Cins (örn. "Golden Retriever") |
| `BirthDate` | `DateTime` | `[Required]`, custom: geçmiş tarih olmalı | Doğum tarihi |
| `Gender` | `PetGender` (enum) | `[Required]` | Erkek, Dişi |
| `Weight` | `decimal?` | `[Range(0.1, 150)]` | Kilogram |
| `OwnerId` | `int` | FK | |

**Navigation:**
- `Owner Owner` — Sahibe referans (N-1).
- `ICollection<Appointment> Appointments` — Hayvanın randevuları (1-N).

**Enum tanımları:**

```csharp
public enum PetSpecies { Kopek = 1, Kedi = 2, Kus = 3, Tavsan = 4, Diger = 5 }
public enum PetGender  { Erkek = 1, Disi = 2 }
```

### 5.3. Service (Klinik Hizmeti)

| Alan | Tip | Validation | Açıklama |
|---|---|---|---|
| `Id` | `int` | PK, Identity | |
| `Name` | `string` | `[Required]`, `[StringLength(100, MinimumLength = 2)]` | "Aşı", "Genel Muayene" |
| `Description` | `string?` | `[StringLength(500)]` | Hizmet detayı |
| `DurationMinutes` | `int` | `[Range(15, 240)]` | Hizmetin süresi |
| `Price` | `decimal` | `[Range(0.01, 100000)]`, `[Column(TypeName="decimal(10,2)")]` | TL cinsinden ücret |
| `IsActive` | `bool` | Default `true` | Pasifleştirilen hizmet listede gözükmez |

**Navigation:**
- `ICollection<Appointment> Appointments` — Bu hizmetin randevuları (1-N).

### 5.4. Appointment (Randevu)

| Alan | Tip | Validation | Açıklama |
|---|---|---|---|
| `Id` | `int` | PK, Identity | |
| `PetId` | `int` | `[Required]`, FK | |
| `ServiceId` | `int` | `[Required]`, FK | |
| `AppointmentDate` | `DateTime` | `[Required]`, custom: gelecek tarih | Randevu başlangıç saati |
| `EndTime` | `DateTime` | Service katmanında hesaplanır | `AppointmentDate + Service.DurationMinutes`; çakışma sorgularında ve görüntüleme için kullanılır (snapshot — sonradan hizmet süresi değişse bile randevunun gerçek bitiş saati korunur) |
| `ServicePriceSnapshot` | `decimal` | Service katmanında doldurulur | Randevu oluşturulduğu/güncellendiği andaki hizmet ücreti |
| `Status` | `AppointmentStatus` (enum) | Default `Beklemede` | Beklemede, Onaylandi, Tamamlandi, IptalEdildi |
| `Notes` | `string?` | `[StringLength(500)]` | Hekim notu / iptal sebebi |
| `CreatedAt` | `DateTime` | Service katmanında `DateTime.Now` ile set edilir | Kayıt zamanı |

**Navigation:**
- `Pet Pet` — Hangi hayvana ait (N-1).
- `Service Service` — Hangi hizmet (N-1).

> **Snapshot stratejisi — Veri Bütünlüğü Kararı:**
> `EndTime` ve `ServicePriceSnapshot`, randevunun oluşturulduğu (veya güncellendiği) andaki hizmet bilgilerinden hesaplanıp Appointment'ta **fiziksel olarak saklanır**. Gerekçe:
> 1. **Tarihsel doğruluk:** Bir hizmetin ücreti veya süresi sonradan değişirse, geçmiş randevular oluşturuldukları andaki değerleri göstermeye devam eder. Aksi halde 1 Mart'taki randevu, 1 Nisan'da fiyat zammı sonrası yanlış raporlanır.
> 2. **SQLite uyumluluğu:** `EndTime`'ın fiziksel sütun olması, çakışma sorgularında `AppointmentDate + Service.DurationMinutes` gibi LINQ-to-SQL'e çevrilemeyen DateTime aritmetiği yapılmasını engeller. Sorgular sade karşılaştırmalar olur.
> 3. **Sorumluluk netliği:** `Service.Name` değişebilir (kozmetik), ama randevunun ücret ve süresi sabittir.

**Enum:**

```csharp
public enum AppointmentStatus
{
    Beklemede   = 1,
    Onaylandi   = 2,
    Tamamlandi  = 3,
    IptalEdildi = 4
}
```

### 5.5. İlişkiler ve Cascade Davranışı

| İlişki | Tip | Cascade davranışı | Gerekçe |
|---|---|---|---|
| Owner → Pet | 1-N | `DeleteBehavior.Cascade` | Sahip silinince hayvanları da silinir (yetim kayıt olmasın). |
| Pet → Appointment | 1-N | `DeleteBehavior.Cascade` | Hayvan silinince randevuları da silinir. |
| Service → Appointment | 1-N | `DeleteBehavior.Restrict` | Geçmiş randevuları olan bir hizmet **silinemez**. Bunun yerine `IsActive = false` yapılır (soft delete). |

`OnModelCreating` içinde Fluent API ile yapılandırma:

```csharp
// Owner.Phone için DB seviyesinde unique index — race condition'a karşı güvenlik ağı
modelBuilder.Entity<Owner>()
    .HasIndex(o => o.Phone)
    .IsUnique();

modelBuilder.Entity<Pet>()
    .HasOne(p => p.Owner)
    .WithMany(o => o.Pets)
    .HasForeignKey(p => p.OwnerId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<Appointment>()
    .HasOne(a => a.Pet)
    .WithMany(p => p.Appointments)
    .HasForeignKey(a => a.PetId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<Appointment>()
    .HasOne(a => a.Service)
    .WithMany(s => s.Appointments)
    .HasForeignKey(a => a.ServiceId)
    .OnDelete(DeleteBehavior.Restrict);

// Snapshot alanları için decimal precision
modelBuilder.Entity<Appointment>()
    .Property(a => a.ServicePriceSnapshot)
    .HasColumnType("decimal(10,2)");
```

> **`HasIndex(...).IsUnique()` neden gerekli?** `IOwnerService.CreateAsync` içindeki `PhoneExistsAsync` kontrolü kullanıcıya güzel bir Türkçe hata mesajı vermek için (UX) gereklidir, ancak iki request mikrosaniye farkla geldiğinde **race condition** doğurur: ikisi de "yok" görür, ikisi de eklenir. DB-level unique constraint son savunma hattıdır; bu durumda EF Core `DbUpdateException` fırlatır ve Service katmanı bunu yakalayıp `Result.Fail("Bu telefon zaten kayıtlı")` döner.

---

## 6. ViewModel Katalogu

Tüm View'lar ViewModel alır. Entity'ler doğrudan View'a gönderilmez.

### 6.1. Owner ViewModel'ları

**`OwnerListViewModel`** — `/Owners/Index` sayfasında, `IEnumerable<OwnerListViewModel>` olarak kullanılır.
```csharp
public class OwnerListViewModel
{
    public int    Id        { get; set; }
    public string FullName  { get; set; } = "";
    public string Phone     { get; set; } = "";
    public string? Email    { get; set; }
    public int    PetCount  { get; set; }   // Sahibin hayvan sayısı
    public DateTime CreatedAt { get; set; }
}
```

**`OwnerDetailsViewModel`** — `/Owners/Details/{id}` sayfasında. Sahip bilgileri + bağlı hayvanların listesi.
```csharp
public class OwnerDetailsViewModel
{
    public int    Id          { get; set; }
    public string FullName    { get; set; } = "";
    public string Phone       { get; set; } = "";
    public string? Email      { get; set; }
    public string? Address    { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PetSummaryViewModel> Pets { get; set; } = new();
}

public class PetSummaryViewModel
{
    public int    Id      { get; set; }
    public string Name    { get; set; } = "";
    public string Species { get; set; } = "";  // Enum → string
    public string AgeText { get; set; } = "";  // "3 yıl 2 ay"
}
```

**`OwnerCreateEditViewModel`** — Hem `/Owners/Create` hem `/Owners/Edit/{id}` sayfalarında ortak kullanılır.
```csharp
public class OwnerCreateEditViewModel
{
    public int? Id { get; set; }   // null → Create, dolu → Edit

    [Required, StringLength(100, MinimumLength = 2)]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = "";

    [Required, RegularExpression(@"^(\+90|0)?5\d{9}$",
        ErrorMessage = "Geçerli bir TR mobil telefon numarası giriniz.")]
    [Display(Name = "Telefon")]
    public string Phone { get; set; } = "";

    [EmailAddress, StringLength(150)]
    [Display(Name = "E-posta")]
    public string? Email { get; set; }

    [StringLength(250)]
    [Display(Name = "Adres")]
    public string? Address { get; set; }
}
```

### 6.2. Pet ViewModel'ları

**`PetListViewModel`** — `/Pets/Index` sayfasında.
```csharp
public class PetListViewModel
{
    public int    Id        { get; set; }
    public string Name      { get; set; } = "";
    public string Species   { get; set; } = "";   // "Köpek"
    public string? Breed    { get; set; }
    public string AgeText   { get; set; } = "";   // "3 yıl 2 ay" / "8 aylık"
    public string OwnerName { get; set; } = "";
    public int    OwnerId   { get; set; }
}
```

**`PetDetailsViewModel`** — `/Pets/Details/{id}` sayfasında, hayvanın tüm randevu geçmişiyle.
```csharp
public class PetDetailsViewModel
{
    public int    Id        { get; set; }
    public string Name      { get; set; } = "";
    public string Species   { get; set; } = "";
    public string? Breed    { get; set; }
    public string Gender    { get; set; } = "";
    public DateTime BirthDate { get; set; }
    public string  AgeText  { get; set; } = "";
    public decimal? Weight  { get; set; }
    public string OwnerName { get; set; } = "";
    public int    OwnerId   { get; set; }
    public List<AppointmentHistoryItemViewModel> Appointments { get; set; } = new();
}

public class AppointmentHistoryItemViewModel
{
    public int      Id              { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string   ServiceName     { get; set; } = "";
    public string   Status          { get; set; } = "";
}
```

**`PetCreateEditViewModel`** — `/Pets/Create` ve `/Pets/Edit/{id}` sayfalarında.
```csharp
public class PetCreateEditViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(50, MinimumLength = 1)]
    [Display(Name = "Hayvan Adı")]
    public string Name { get; set; } = "";

    [Required] [Display(Name = "Tür")]
    public PetSpecies Species { get; set; }

    [StringLength(50)] [Display(Name = "Cins")]
    public string? Breed { get; set; }

    [Required] [Display(Name = "Doğum Tarihi")]
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }

    [Required] [Display(Name = "Cinsiyet")]
    public PetGender Gender { get; set; }

    [Range(0.1, 150)] [Display(Name = "Kilo (kg)")]
    public decimal? Weight { get; set; }

    [Required] [Display(Name = "Sahibi")]
    public int OwnerId { get; set; }

    // Dropdown için doldurulur (Controller tarafından)
    public List<SelectListItem> OwnerOptions { get; set; } = new();
}
```

### 6.3. Service ViewModel'ları

**`ServiceListViewModel`** — `/Services/Index` sayfasında. Aktif ve pasif hizmetler ayrı sekmelerde gösterilir.
```csharp
public class ServiceListViewModel
{
    public int     Id              { get; set; }
    public string  Name            { get; set; } = "";
    public int     DurationMinutes { get; set; }
    public decimal Price           { get; set; }
    public bool    IsActive        { get; set; }
    public int     AppointmentCount{ get; set; }
}
```

**`ServiceCreateEditViewModel`** — `/Services/Create` ve `/Services/Edit/{id}`.
```csharp
public class ServiceCreateEditViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    [Display(Name = "Hizmet Adı")]
    public string Name { get; set; } = "";

    [StringLength(500)] [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Required, Range(15, 240)] [Display(Name = "Süre (dk)")]
    public int DurationMinutes { get; set; }

    [Required, Range(0.01, 100000)] [Display(Name = "Ücret (TL)")]
    public decimal Price { get; set; }

    [Display(Name = "Aktif mi?")]
    public bool IsActive { get; set; } = true;
}
```

### 6.4. Appointment ViewModel'ları

**`AppointmentListViewModel`** — `/Appointments/Index` sayfasında. Liste filtre ile (tarih aralığı, status) gösterilir.
```csharp
public class AppointmentListViewModel
{
    public int      Id              { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string   PetName         { get; set; } = "";
    public string   OwnerName       { get; set; } = "";
    public string   ServiceName     { get; set; } = "";
    public string   Status          { get; set; } = "";   // Renkli badge için
    public int      DurationMinutes { get; set; }
}
```

**`AppointmentDetailsViewModel`** — `/Appointments/Details/{id}`.
```csharp
public class AppointmentDetailsViewModel
{
    public int      Id              { get; set; }
    public DateTime AppointmentDate { get; set; }
    public DateTime EndTime         { get; set; }   // Start + service duration
    public string   Status          { get; set; } = "";
    public string?  Notes           { get; set; }
    public DateTime CreatedAt       { get; set; }
    public string   PetName         { get; set; } = "";
    public int      PetId           { get; set; }
    public string   OwnerName       { get; set; } = "";
    public int      OwnerId         { get; set; }
    public string   ServiceName     { get; set; } = "";
    public decimal  ServicePrice    { get; set; }
}
```

**`AppointmentCreateEditViewModel`** — `/Appointments/Create` ve `/Appointments/Edit/{id}`.
```csharp
public class AppointmentCreateEditViewModel
{
    public int? Id { get; set; }

    [Required] [Display(Name = "Hayvan")]
    public int PetId { get; set; }

    [Required] [Display(Name = "Hizmet")]
    public int ServiceId { get; set; }

    [Required] [Display(Name = "Randevu Tarihi/Saati")]
    [DataType(DataType.DateTime)]
    public DateTime AppointmentDate { get; set; }

    [Display(Name = "Durum")]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Beklemede;

    [StringLength(500)] [Display(Name = "Notlar")]
    public string? Notes { get; set; }

    // Dropdown'lar
    public List<SelectListItem> PetOptions     { get; set; } = new();
    public List<SelectListItem> ServiceOptions { get; set; } = new();
}
```

### 6.5. Dashboard ViewModel

**`DashboardViewModel`** — `/` (HomeController.Index) sayfasında.
```csharp
public class DashboardViewModel
{
    public int TodayAppointmentCount     { get; set; }
    public int ThisWeekAppointmentCount  { get; set; }
    public int TotalOwnerCount           { get; set; }
    public int TotalPetCount             { get; set; }
    public int PendingAppointmentCount   { get; set; }

    public List<AppointmentListViewModel> TodayAppointments      { get; set; } = new();
    public List<AppointmentListViewModel> UpcomingAppointments   { get; set; } = new(); // 7 günlük
    public List<TopServiceItem>          MostRequestedServices  { get; set; } = new(); // Son 30 gün
}

public class TopServiceItem
{
    public string ServiceName { get; set; } = "";
    public int    Count       { get; set; }
}
```

---

## 7. Repository Katmanı

Tüm repository'ler interface üzerinden enjekte edilir. Mimari, **generic `IRepository<T>` taban interface'i** üzerine kurulur; her spesifik repository bu generic'i implement eder ve **sadece entity'ye özel** sorguları ekstra metod olarak ekler.

### 7.0. Generic `IRepository<T>` (Taban Interface)

```csharp
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?>             GetByIdAsync(int id);
    Task                 AddAsync(T entity);
    Task                 UpdateAsync(T entity);
    Task                 DeleteAsync(int id);
    Task<int>            GetTotalCountAsync();
}
```

**Concrete generic implementation** (`Repository<T> : IRepository<T>`), bu metodların ortak EF Core mantığını içerir. Spesifik repository'ler bu sınıftan türeyebilir (`OwnerRepository : Repository<Owner>, IOwnerRepository`) veya doğrudan `DbContext` enjekte edip kendi implementasyonlarını yazabilir. Eğitim amaçlı şeffaflık için her ikisi de kabul edilebilir.

> **Neden generic pattern?** CRUD operasyonları tüm entity'ler için aynıdır (GetAll, GetById, Add, Update, Delete). Bu kodu her repository'de tekrar yazmak DRY ihlalidir. Generic taban interface, ortak sözleşmeyi tanımlar; spesifik interface'ler yalnızca entity'ye özel sorgu metodlarını (Include'lar, filtreler) ekler.

### 7.1. `IOwnerRepository`

```csharp
public interface IOwnerRepository : IRepository<Owner>
{
    // IRepository<Owner>'dan miras: GetAllAsync, GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync, GetTotalCountAsync

    Task<Owner?> GetByIdWithPetsAsync(int id);
    Task<bool>   PhoneExistsAsync(string phone, int? excludeId = null);
}
```

### 7.2. `IPetRepository`

```csharp
public interface IPetRepository : IRepository<Pet>
{
    Task<IEnumerable<Pet>> GetAllWithOwnerAsync();
    Task<Pet?>             GetByIdWithOwnerAndAppointmentsAsync(int id);
    Task<IEnumerable<Pet>> GetByOwnerIdAsync(int ownerId);
}
```

### 7.3. `IServiceRepository`

```csharp
public interface IServiceRepository : IRepository<Service>
{
    Task<IEnumerable<Service>> GetActiveAsync();
    Task<bool>                 HasAppointmentsAsync(int serviceId);
    Task<List<TopServiceItem>> GetTopRequestedAsync(int days, int top);
}
```

### 7.4. `IAppointmentRepository`

```csharp
public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<IEnumerable<Appointment>> GetAllWithDetailsAsync();
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<Appointment?>             GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Appointment>> GetByPetIdAsync(int petId);
    Task<IEnumerable<Appointment>> GetConflictingAsync(DateTime requestedStart, DateTime requestedEnd, int? excludeId = null);
    Task<int>                      GetCountByDateAsync(DateTime date);
    Task<int>                      GetCountByStatusAsync(AppointmentStatus status);
}
```

**`GetConflictingAsync` implementasyonu** (snapshot'lı `EndTime` sayesinde sade LINQ):

```csharp
public async Task<IEnumerable<Appointment>> GetConflictingAsync(
    DateTime requestedStart, DateTime requestedEnd, int? excludeId = null)
{
    return await _context.Appointments
        .Include(a => a.Service)
        .Include(a => a.Pet)
        .Where(a => a.Status != AppointmentStatus.IptalEdildi)
        .Where(a => excludeId == null || a.Id != excludeId)
        // Çakışma formülü: A.start < B.end AND A.end > B.start
        .Where(a => a.AppointmentDate < requestedEnd && a.EndTime > requestedStart)
        .ToListAsync();
}
```

> **SQLite uyumluluk notu:** Önceki tasarım `a.AppointmentDate.AddMinutes(a.Service.DurationMinutes) > start` gibi bir ifadeye sahipti. EF Core SQLite provider DateTime aritmetiğini LINQ-to-SQL'e çeviremez ve runtime'da `NotSupportedException` fırlatır. Çözüm olarak `EndTime` alanı entity'de fiziksel sütun olarak saklanır (§5.4'teki snapshot stratejisi). Böylece sorgu sade tarih karşılaştırmalarına indirgenir ve SQLite ile sorunsuz çalışır.
>
> **Alternatif (eğer EndTime saklamak istenmezse):** Geniş bir gün/saat penceresindeki tüm randevular `.AsEnumerable()` ile in-memory'e çekilip, hesaplama LINQ-to-Objects ile yapılır. Küçük klinikte performans sorun değildir; ancak veri büyüdükçe ölçeklenmez. Bu spec **EndTime snapshot** yaklaşımını tercih eder.

---

## 8. Service Katmanı (Business Logic)

### 8.1. `IOwnerService`

```csharp
public interface IOwnerService
{
    Task<IEnumerable<OwnerListViewModel>> GetAllAsync();
    Task<OwnerDetailsViewModel?>          GetDetailsAsync(int id);
    Task<OwnerCreateEditViewModel?>       GetForEditAsync(int id);
    Task<Result> CreateAsync(OwnerCreateEditViewModel vm);
    Task<Result> UpdateAsync(OwnerCreateEditViewModel vm);
    Task<Result> DeleteAsync(int id);
}
```

**Business kuralları:**
- `Phone` benzersiz olmalı; `Create`/`Update` öncesi `PhoneExistsAsync` ile kontrol edilir, varsa `Result.Fail("Bu telefon zaten kayıtlı")`.
- `Delete` yapıldığında cascade ile pet'leri ve onların appointment'ları da silinir; bu durum UI'da kullanıcıya **onay diyaloğunda açıkça belirtilir**.

### 8.2. `IPetService`

```csharp
public interface IPetService
{
    Task<IEnumerable<PetListViewModel>> GetAllAsync();
    Task<PetDetailsViewModel?>          GetDetailsAsync(int id);
    Task<PetCreateEditViewModel?>       GetForEditAsync(int id);
    Task<PetCreateEditViewModel>        BuildEmptyCreateAsync();
    Task<Result> CreateAsync(PetCreateEditViewModel vm);
    Task<Result> UpdateAsync(PetCreateEditViewModel vm);
    Task<Result> DeleteAsync(int id);
    string       CalculateAgeText(DateTime birthDate);
}
```

**`CalculateAgeText` algoritması:**

```
years  = today.Year  - birth.Year
months = today.Month - birth.Month
if (today.Day < birth.Day) months--
if (months < 0) { years--; months += 12 }

if years == 0: return $"{months} aylık"
if months == 0: return $"{years} yıl"
return $"{years} yıl {months} ay"
```

**Validasyonlar:**
- `BirthDate` bugünden büyük olamaz → `Result.Fail("Doğum tarihi gelecek olamaz.")`
- `BirthDate` 50 yıldan eski olamaz (sanity check) → uyarı.

### 8.3. `IServiceCatalogService`

> **İsimlendirme notu:** Application service katmanı `IXxxService` desenini izler (`IOwnerService`, `IPetService` vb.). Ancak entity adı da `Service` olduğu için `IServiceService` çakışma yaratıyordu. Bu spec, sadece çakışan tek interface için `IServiceCatalogService` adını kullanır — "klinik hizmet kataloğu yönetimi" semantiğini de daha iyi yansıtır. Entity ismi `Service` olarak korunur (Türkçe karşılığı "Hizmet" — UI dilinde standart terim; "Tedavi" ise tırnak kesimi gibi non-medical kalemler için yanlış olur).

```csharp
public interface IServiceCatalogService
{
    Task<IEnumerable<ServiceListViewModel>> GetAllAsync();
    Task<ServiceCreateEditViewModel?>       GetForEditAsync(int id);
    Task<Result> CreateAsync(ServiceCreateEditViewModel vm);
    Task<Result> UpdateAsync(ServiceCreateEditViewModel vm);

    // Tek tipli silme yerine iki ayrı operasyon — UI'da kullanıcı ne olacağını önceden bilir
    Task<Result> DeactivateAsync(int id);  // IsActive = false
    Task<Result> ActivateAsync(int id);    // IsActive = true
    Task<Result> DeleteAsync(int id);      // Sadece randevusu yoksa fiziksel sil
}
```

**Davranış kuralları:**
- `DeleteAsync` çağrılır ve hizmetin **mevcut/geçmiş randevusu varsa** → `Result.Fail("Bu hizmetin randevuları olduğundan silinemez. Bunun yerine pasifleştirin.")`. Silme yapılmaz.
- `DeleteAsync` çağrılır ve hiç randevusu yoksa → DB'den silinir.
- `DeactivateAsync` herhangi bir hizmeti pasifleştirir (`IsActive = false`). Pasif hizmetler yeni randevu dropdown'unda görünmez ama geçmiş randevular kaybolmaz.
- `ActivateAsync` pasif hizmeti geri aktif eder.

> **Neden tek `DeleteOrDeactivateAsync` yerine ayrı metodlar?** Önceki tasarımda tek "Sil" butonu bazen siliyor bazen pasifleştiriyordu — kullanıcı butona basmadan önce ne olacağını bilmiyordu (UX karışıklığı). Ayrı operasyonlar + UI'da bağlama göre buton göstermek (§10.5) daha net.

### 8.4. `IAppointmentService`

```csharp
public interface IAppointmentService
{
    Task<IEnumerable<AppointmentListViewModel>> GetAllAsync(
        DateTime? from = null, DateTime? to = null, AppointmentStatus? status = null);

    Task<AppointmentDetailsViewModel?>     GetDetailsAsync(int id);
    Task<AppointmentCreateEditViewModel?>  GetForEditAsync(int id);
    Task<AppointmentCreateEditViewModel>   BuildEmptyCreateAsync();

    Task<Result> CreateAsync(AppointmentCreateEditViewModel vm);
    Task<Result> UpdateAsync(AppointmentCreateEditViewModel vm);
    Task<Result> ChangeStatusAsync(int id, AppointmentStatus newStatus);
    Task<Result> CancelAsync(int id, string? reason);
    Task<Result> DeleteAsync(int id);
}
```

**Kritik iş kuralları (Appointment):**

**(A) Tarih validasyonu** — `Create` ve `Update` öncesi kontrol edilir:
1. `AppointmentDate` geçmiş bir tarih ise → `Result.Fail("Geçmiş bir tarihe randevu oluşturulamaz.")`
2. `AppointmentDate.TimeOfDay` `09:00` – `19:00` aralığında değilse → `Result.Fail("Randevu saati 09:00–19:00 arasında olmalıdır.")`
3. `AppointmentDate.DayOfWeek == Sunday` ise → `Result.Fail("Pazar günleri randevu alınamaz.")`
4. `Service.DurationMinutes` eklendikten sonra bitiş saati `19:00`'u geçiyorsa → `Result.Fail("Hizmet süresi mesai saatleri içine sığmıyor.")`

**(B) Çakışma kontrolü** — `_appointmentRepo.GetConflictingAsync(start, end, excludeId)` çağrılır:
- Aynı zaman dilimine başka bir randevu varsa (klinik tek poliklinik varsayımı) → `Result.Fail("Bu saat aralığında zaten bir randevu bulunmaktadır.")`
- Aynı hayvanın aynı gün başka bir randevusu varsa → uyarı olarak loglanır ama bloklanmaz (klinik kararına bırakılır).

**(C) Durum geçişleri:** Geçerli geçişler:
```
Beklemede   → Onaylandi | IptalEdildi
Onaylandi   → Tamamlandi | IptalEdildi
Tamamlandi  → (terminal — değiştirilemez)
IptalEdildi → (terminal — değiştirilemez)
```
`ChangeStatusAsync` geçersiz bir geçiş denenirse `Result.Fail("Bu durum geçişi geçersizdir.")` döner.

**(D) Soft cancel:** `CancelAsync`, fiziksel silme yapmaz. `Status = IptalEdildi`, `Notes` alanına iptal sebebi yazılır. Böylece raporlamada iptal istatistiği kaybolmaz.

**(E) Snapshot doldurma** — `CreateAsync` ve `UpdateAsync` içinde, `_serviceRepository.GetByIdAsync(vm.ServiceId)` ile hizmet çekilir ve:
```csharp
appointment.EndTime              = vm.AppointmentDate.AddMinutes(service.DurationMinutes);
appointment.ServicePriceSnapshot = service.Price;
appointment.CreatedAt            = DateTime.Now;  // sadece Create'te
```
Bu noktadan sonra hizmetin `Price` veya `DurationMinutes` alanları değişse bile, bu randevunun bitiş saati ve ücreti sabit kalır (§5.4 snapshot stratejisi).

### 8.5. `IDashboardService`

```csharp
public interface IDashboardService
{
    Task<DashboardViewModel> BuildAsync();
}
```

**`BuildAsync` içeriği:**
- `TodayAppointmentCount` = bugünün 00:00–23:59 randevu sayısı
- `ThisWeekAppointmentCount` = pazartesi–pazar randevu sayısı
- `TotalOwnerCount`, `TotalPetCount` = ilgili repo `GetTotalCountAsync`
- `PendingAppointmentCount` = `Status == Beklemede`
- `TodayAppointments` = bugün için, saate göre sıralı, max 20
- `UpcomingAppointments` = yarından itibaren 7 gün, max 10
- `MostRequestedServices` = son 30 günde en çok randevusu olan 5 hizmet

### 8.6. `Result` tipi (yardımcı sınıf)

```csharp
public class Result
{
    public bool   Succeeded { get; init; }
    public string Message   { get; init; } = "";

    public static Result Success(string msg = "") => new() { Succeeded = true,  Message = msg };
    public static Result Fail   (string msg)      => new() { Succeeded = false, Message = msg };
}
```

---

## 9. Controller Katmanı

Tüm controller'lar `Controller` base sınıfından türer. Constructor injection ile gereken service alınır. Action'lar mümkün olduğunca kısadır.

### 9.1. `HomeController`

| Action | Route | HTTP | Dönüş | Açıklama |
|---|---|---|---|---|
| `Index` | `/` veya `/Home/Index` | GET | `ViewResult` → `DashboardViewModel` | Anasayfa dashboard |
| `Error` | `/Home/Error` | GET | `ViewResult` | Hata sayfası |

### 9.2. `OwnersController`

| Action | Route | HTTP | Dönüş | Açıklama |
|---|---|---|---|---|
| `Index`        | `/Owners`             | GET  | `ViewResult` (`IEnumerable<OwnerListViewModel>`) | Tüm sahipler |
| `Details`      | `/Owners/Details/{id}` | GET  | `ViewResult` (`OwnerDetailsViewModel`) veya `NotFound` | Sahip + hayvanları |
| `Create` (GET) | `/Owners/Create`       | GET  | `ViewResult` (boş `OwnerCreateEditViewModel`) | Form |
| `Create` (POST)| `/Owners/Create`       | POST | `RedirectToAction(Index)` veya `ViewResult` | Validasyon başarısız → form |
| `Edit` (GET)   | `/Owners/Edit/{id}`    | GET  | `ViewResult` (`OwnerCreateEditViewModel` dolu) | |
| `Edit` (POST)  | `/Owners/Edit/{id}`    | POST | `RedirectToAction(Index)` veya `ViewResult` | |
| `Delete` (GET) | `/Owners/Delete/{id}`  | GET  | `ViewResult` (onay sayfası) | |
| `DeleteConfirmed`| `/Owners/Delete/{id}`| POST | `RedirectToAction(Index)` | Cascade uyarısı içerir |

**Örnek action (Thin Controller):**
```csharp
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Create(OwnerCreateEditViewModel vm)
{
    if (!ModelState.IsValid) return View(vm);

    var result = await _ownerService.CreateAsync(vm);
    if (!result.Succeeded)
    {
        ModelState.AddModelError("", result.Message);
        return View(vm);
    }

    TempData["Success"] = "Sahip başarıyla eklendi.";
    return RedirectToAction(nameof(Index));
}
```

### 9.3. `PetsController`

| Action | Route | HTTP | Dönüş |
|---|---|---|---|
| `Index`         | `/Pets`              | GET  | `ViewResult` (`IEnumerable<PetListViewModel>`) |
| `Details`       | `/Pets/Details/{id}` | GET  | `ViewResult` (`PetDetailsViewModel`) |
| `Create` (GET)  | `/Pets/Create`       | GET  | `ViewResult` (`PetCreateEditViewModel`, owner dropdown dolu) |
| `Create` (POST) | `/Pets/Create`       | POST | `RedirectToAction(Index)` |
| `Edit` (GET)    | `/Pets/Edit/{id}`    | GET  | `ViewResult` |
| `Edit` (POST)   | `/Pets/Edit/{id}`    | POST | `RedirectToAction(Index)` |
| `Delete` (GET)  | `/Pets/Delete/{id}`  | GET  | `ViewResult` (onay) |
| `DeleteConfirmed`| `/Pets/Delete/{id}` | POST | `RedirectToAction(Index)` |

### 9.4. `ServicesController`

| Action | Route | HTTP | Dönüş |
|---|---|---|---|
| `Index`         | `/Services`               | GET  | `ViewResult` |
| `Create` (GET)  | `/Services/Create`        | GET  | `ViewResult` |
| `Create` (POST) | `/Services/Create`        | POST | `RedirectToAction(Index)` |
| `Edit` (GET)    | `/Services/Edit/{id}`     | GET  | `ViewResult` |
| `Edit` (POST)   | `/Services/Edit/{id}`     | POST | `RedirectToAction(Index)` |
| `Deactivate`    | `/Services/Deactivate/{id}` | POST | `RedirectToAction(Index)` — randevusu olan hizmetler için |
| `Activate`      | `/Services/Activate/{id}`   | POST | `RedirectToAction(Index)` — pasif hizmeti geri aç |
| `Delete` (GET)  | `/Services/Delete/{id}`   | GET  | `ViewResult` — sadece randevusu olmayan hizmetler için onay |
| `DeleteConfirmed`| `/Services/Delete/{id}`  | POST | `RedirectToAction(Index)` |

> `Delete` action'ı `IServiceCatalogService.DeleteAsync`'i çağırır; randevusu varsa `Result.Fail` döner ve UI'ya "Pasifleştirmek ister misiniz?" uyarısı gösterilir.

### 9.5. `AppointmentsController`

| Action | Route | HTTP | Dönüş |
|---|---|---|---|
| `Index`         | `/Appointments?from=&to=&status=` | GET  | `ViewResult` (filtreli liste) |
| `Details`       | `/Appointments/Details/{id}`       | GET  | `ViewResult` |
| `Create` (GET)  | `/Appointments/Create`             | GET  | `ViewResult` (pet + service dropdown) |
| `Create` (POST) | `/Appointments/Create`             | POST | `RedirectToAction(Index)` |
| `Edit` (GET)    | `/Appointments/Edit/{id}`          | GET  | `ViewResult` |
| `Edit` (POST)   | `/Appointments/Edit/{id}`          | POST | `RedirectToAction(Index)` |
| `Cancel` (GET)  | `/Appointments/Cancel/{id}`        | GET  | `ViewResult` (sebep alanı) |
| `Cancel` (POST) | `/Appointments/Cancel/{id}`        | POST | `RedirectToAction(Index)` |
| `Complete`      | `/Appointments/Complete/{id}`      | POST | `RedirectToAction(Index)` |
| `Confirm`       | `/Appointments/Confirm/{id}`       | POST | `RedirectToAction(Index)` |
| `Delete` (GET)  | `/Appointments/Delete/{id}`        | GET  | `ViewResult` |
| `DeleteConfirmed`| `/Appointments/Delete/{id}`       | POST | `RedirectToAction(Index)` |

Not: `Confirm` ve `Complete`, status değişiklikleri için ayrı endpoint olarak konuldu (semantik netlik için).

---

## 10. View Katmanı

Her CRUD entity için minimum 5 view; ayrıca dashboard ve layout dosyaları.

### 10.1. Genel layout

**`Views/Shared/_Layout.cshtml`**
- Üst menü: `Anasayfa`, `Sahipler`, `Hayvanlar`, `Hizmetler`, `Randevular`
- Bootstrap 5 navbar (`navbar-dark bg-primary`)
- `TempData["Success"]` ve `TempData["Error"]` için flash mesaj alanı
- Footer: telif + sürüm

### 10.2. Dashboard view

**`Views/Home/Index.cshtml`**
- Üst sıra: 4 stat kartı (bugün randevu, bu hafta, toplam sahip, toplam hayvan)
- Orta sıra:
  - Sol: "Bugünün Randevuları" tablosu (saat, hayvan, hizmet, durum badge)
  - Sağ: "Yaklaşan Randevular (7 gün)" listesi
- Alt: "Son 30 günde en çok talep edilen hizmetler" — bar chart (Bootstrap progress bar ile)

### 10.3. Owners view'ları

**`Views/Owners/Index.cshtml`** — tablo kolonları:
| # | Ad Soyad | Telefon | E-posta | Hayvan Sayısı | Kayıt Tarihi | İşlemler |

İşlemler kolonunda: `Details`, `Edit`, `Delete` butonları (Bootstrap btn-group).
Üst sağda "Yeni Sahip" butonu (`/Owners/Create`).

**`Views/Owners/Details.cshtml`**
- Üstte sahip bilgi kartı (FullName, Phone, Email, Address, CreatedAt)
- Altta "Bu Sahibe Ait Hayvanlar" tablosu (Name, Species, AgeText, Details butonu)
- "Yeni Hayvan Ekle (Bu Sahibe)" hızlı butonu (`/Pets/Create?ownerId={id}`)

**`Views/Owners/Create.cshtml`** ve **`Edit.cshtml`** — form:
- FullName (text input)
- Phone (text input, JS mask `0(5xx) xxx xx xx`)
- Email (text input)
- Address (textarea, 3 satır)
- "Kaydet" + "İptal" butonları
- `_ValidationScriptsPartial` include

**`Views/Owners/Delete.cshtml`**
- Sahibin bilgileri ile birlikte uyarı:
  > "Bu sahip silinirse, sahibe ait **{N} hayvan** ve onlara ait tüm randevular da silinecektir. Devam etmek istiyor musunuz?"
- "Evet, Sil" (red) + "Vazgeç"

### 10.4. Pets view'ları

**`Views/Pets/Index.cshtml`** — kolonlar:
| # | Ad | Tür | Cins | Yaş | Sahibi | İşlemler |

Türler için renkli badge: Köpek=primary, Kedi=warning, Kuş=info, Tavşan=success, Diğer=secondary.

**`Views/Pets/Details.cshtml`**
- Hayvan bilgi kartı (tüm alanlar + sahibinin adı linkli)
- "Randevu Geçmişi" tablosu (Tarih, Hizmet, Durum)
- "Yeni Randevu Oluştur (Bu Hayvana)" butonu

**`Create.cshtml` / `Edit.cshtml`** — form alanları:
- Name, Species (dropdown), Breed, BirthDate (date input), Gender (radio), Weight, OwnerId (searchable dropdown)

**`Delete.cshtml`** — uyarı:
> "Bu hayvan silinirse, ona ait **{N} randevu** da silinecektir."

### 10.5. Services view'ları

**`Views/Services/Index.cshtml`** — iki tab:
- **Tab 1: Aktif Hizmetler** — kolonlar: Ad, Süre (dk), Ücret (TL), Randevu Sayısı, İşlemler.
  İşlemler kolonu **koşullu butonlar** içerir:
  - `Edit` (her zaman)
  - `Pasifleştir` (her zaman; POST `/Services/Deactivate/{id}`)
  - `Sil` (sadece randevu sayısı = 0 ise; GET `/Services/Delete/{id}`)
- **Tab 2: Pasif Hizmetler** — aynı kolonlar + İşlemler: `Aktifleştir` (POST `/Services/Activate/{id}`), `Sil` (sadece randevu sayısı = 0 ise).

> **UX kararı:** Kullanıcı bir butona basmadan önce sonucu net olarak bilir. "Sil" butonu sadece güvenli olduğunda görünür; veri kaybı riski olan tüm durumlarda kullanıcıya `Pasifleştir` seçeneği sunulur. Önceki tek-buton tasarımındaki "bazen siliyor bazen pasifleştiriyor" belirsizliği ortadan kalkar.

**`Create.cshtml` / `Edit.cshtml`** — Name, Description, DurationMinutes, Price, IsActive (checkbox).

**`Delete.cshtml`** — sadece randevu sayısı = 0 olan hizmetler için açılır. Standart "Bu hizmeti silmek istediğinize emin misiniz?" onay sayfası.

### 10.6. Appointments view'ları

**`Views/Appointments/Index.cshtml`**
- Üst filtre satırı: tarih aralığı (from–to), durum dropdown
- Tablo kolonları: | # | Tarih/Saat | Hayvan | Sahibi | Hizmet | Süre | Durum | İşlemler |
- Durum kolonu renkli badge:
  - Beklemede → `bg-warning`
  - Onaylandı → `bg-info`
  - Tamamlandı → `bg-success`
  - İptal → `bg-secondary`
- İşlemler: Details, Edit, Cancel (status uygunsa), Complete (status uygunsa), Delete

**`Details.cshtml`** — randevu kartı + ilgili hayvan + sahip + hizmet bilgileri; "Tamamla", "İptal Et" butonları.

**`Create.cshtml` / `Edit.cshtml`** — form:
- Pet (dropdown, "Hayvan Adı — Sahibi" formatında)
- Service (dropdown, sadece aktif olanlar)
- AppointmentDate (datetime-local input)
- Status (sadece Edit'te görünür)
- Notes (textarea)

**`Cancel.cshtml`** — iptal sebebi için textarea (zorunlu).

---

## 11. Dependency Injection Kayıtları (`Program.cs`)

```csharp
// DbContext — AddDbContext otomatik olarak Scoped kaydeder
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// Generic repository (opsiyonel — spesifik repo'lar bundan türeyebilir)
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Repositories — Scoped
builder.Services.AddScoped<IOwnerRepository,       OwnerRepository>();
builder.Services.AddScoped<IPetRepository,         PetRepository>();
builder.Services.AddScoped<IServiceRepository,     ServiceRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

// Application Services — Scoped
builder.Services.AddScoped<IOwnerService,          OwnerService>();
builder.Services.AddScoped<IPetService,            PetService>();
builder.Services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
builder.Services.AddScoped<IAppointmentService,    AppointmentService>();
builder.Services.AddScoped<IDashboardService,      DashboardService>();

// MVC
builder.Services.AddControllersWithViews();
```

### 11.1. Yaşam süresi (lifetime) gerekçeleri

| Bileşen | Lifetime | Gerekçe |
|---|---|---|
| `ApplicationDbContext` | **Scoped** | EF Core DbContext thread-safe değildir. Her HTTP request için yeni instance, request boyunca aynı kalır. `AddDbContext` zaten Scoped kaydeder. |
| `*Repository` | **Scoped** | Repository içine DbContext (Scoped) enjekte edildiği için Scoped olmalı. Singleton olsa DbContext yaşam süresinden uzun yaşar (captive dependency anti-pattern). Transient olsa request başına gereksiz instance üretir. |
| `*Service` | **Scoped** | Repository (Scoped) kullandığı için Scoped olmalı. Aynı request içindeki birden fazla service çağrısı aynı repo + aynı DbContext'i paylaşır (transaction tutarlılığı). |

**Neden Transient değil?** Transient her enjeksiyonda yeni instance üretir. Bir request içinde aynı service iki kez enjekte edilse iki ayrı DbContext bağlamı doğar; bu hem performans kaybı hem de transaction yönetimi karmaşası yaratır.

**Neden Singleton değil?** Singleton uygulama ömrü boyunca tek instance demek. DbContext Singleton olamaz (concurrent kullanım hataları); dolayısıyla ona bağımlı tüm katmanlar da olamaz.

### 11.2. Migration ve seed çağrısı

`Program.cs` sonunda, `app.Run()` öncesinde:

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    SeedData.Initialize(db);
}
```

---

## 12. Migration Adımları

### 12.1. Paket gereksinimleri

```
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet tool install --global dotnet-ef   # bir kerelik
```

### 12.2. İlk migration

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Bu komut `vetclinic.db` SQLite dosyasını proje köküne oluşturur.

### 12.3. Sonraki migration örnekleri

Entity'ye alan eklenirse:
```bash
dotnet ef migrations add AddWeightToPet
dotnet ef database update
```

Tüm migrationları sıfırlamak (sadece development):
```bash
dotnet ef database drop -f
dotnet ef migrations remove
```

### 12.4. `appsettings.json`

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=vetclinic.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 13. Seed Data

`Data/SeedData.cs` içinde, veritabanı boşsa örnek kayıtlar eklenir.

### 13.1. Owner seed (3 kayıt)

```csharp
new Owner { FullName = "Ayşe Yılmaz",    Phone = "05321112233", Email = "ayse@example.com",   Address = "İzmir/Karşıyaka" },
new Owner { FullName = "Mehmet Demir",   Phone = "05334445566", Email = "mehmet@example.com", Address = "İzmir/Bornova"   },
new Owner { FullName = "Selin Kara",     Phone = "05447778899", Email = null,                  Address = "İzmir/Konak"     },
```

### 13.2. Service seed (6 kayıt)

| Name | DurationMinutes | Price | IsActive |
|---|---|---|---|
| Genel Muayene | 30 | 350 | true |
| Aşı (Tek Doz) | 20 | 250 | true |
| Kısırlaştırma — Kedi (Dişi) | 90 | 4500 | true |
| Kısırlaştırma — Köpek (Erkek) | 60 | 3500 | true |
| Diş Temizliği | 60 | 1800 | true |
| Tırnak Kesimi | 15 | 150 | true |

### 13.3. Pet seed (5 kayıt — owner'larla ilişkili)

```csharp
new Pet { Name = "Pamuk",  Species = PetSpecies.Kedi, Breed = "Tekir",          BirthDate = new DateTime(2022,3,10), Gender = PetGender.Disi,  Weight = 4.2m, OwnerId = 1 },
new Pet { Name = "Boncuk", Species = PetSpecies.Kedi, Breed = "Van",            BirthDate = new DateTime(2020,6,5),  Gender = PetGender.Erkek, Weight = 5.0m, OwnerId = 1 },
new Pet { Name = "Karabaş",Species = PetSpecies.Kopek,Breed = "Golden Retriever",BirthDate= new DateTime(2019,1,20),Gender = PetGender.Erkek, Weight = 28.5m,OwnerId = 2 },
new Pet { Name = "Çıtçıt", Species = PetSpecies.Kus,  Breed = "Muhabbet Kuşu",  BirthDate = new DateTime(2024,2,1),  Gender = PetGender.Erkek, Weight = 0.05m,OwnerId = 3 },
new Pet { Name = "Maya",   Species = PetSpecies.Kopek,Breed = "Pomeranian",     BirthDate = new DateTime(2023,9,15), Gender = PetGender.Disi,  Weight = 3.1m, OwnerId = 3 },
```

### 13.4. Appointment seed (4 kayıt — gelecek tarihli)

```csharp
new Appointment { PetId = 1, ServiceId = 1, AppointmentDate = DateTime.Today.AddDays(1).AddHours(10), Status = AppointmentStatus.Onaylandi },
new Appointment { PetId = 3, ServiceId = 2, AppointmentDate = DateTime.Today.AddDays(2).AddHours(14), Status = AppointmentStatus.Beklemede },
new Appointment { PetId = 5, ServiceId = 6, AppointmentDate = DateTime.Today.AddDays(3).AddHours(11), Status = AppointmentStatus.Beklemede },
new Appointment { PetId = 2, ServiceId = 5, AppointmentDate = DateTime.Today.AddDays(5).AddHours(15), Status = AppointmentStatus.Onaylandi },
```

---

## 14. UML Sınıf Diyagramı İçeriği

Aşağıdaki yapı bir UML aracında (draw.io, PlantUML, Visual Paradigm) çizilmek üzere yeterli detaydadır.

### 14.1. Sınıflar ve nitelikler

```
[Owner]
  - Id : int (PK)
  - FullName : string
  - Phone : string
  - Email : string?
  - Address : string?
  - CreatedAt : DateTime
  + Pets : List<Pet>

[Pet]
  - Id : int (PK)
  - Name : string
  - Species : PetSpecies
  - Breed : string?
  - BirthDate : DateTime
  - Gender : PetGender
  - Weight : decimal?
  - OwnerId : int (FK)
  + Owner : Owner
  + Appointments : List<Appointment>

[Service]
  - Id : int (PK)
  - Name : string
  - Description : string?
  - DurationMinutes : int
  - Price : decimal
  - IsActive : bool
  + Appointments : List<Appointment>

[Appointment]
  - Id : int (PK)
  - PetId : int (FK)
  - ServiceId : int (FK)
  - AppointmentDate : DateTime
  - EndTime : DateTime           (snapshot — AppointmentDate + Service.DurationMinutes)
  - ServicePriceSnapshot : decimal (snapshot — randevu anındaki ücret)
  - Status : AppointmentStatus
  - Notes : string?
  - CreatedAt : DateTime
  + Pet : Pet
  + Service : Service

<<enum>> PetSpecies         { Kopek, Kedi, Kus, Tavsan, Diger }
<<enum>> PetGender          { Erkek, Disi }
<<enum>> AppointmentStatus  { Beklemede, Onaylandi, Tamamlandi, IptalEdildi }
```

### 14.2. İlişki notasyonları

```
Owner   1 ────< 0..*  Pet            (composition, cascade delete)
Pet     1 ────< 0..*  Appointment    (composition, cascade delete)
Service 1 ────< 0..*  Appointment    (aggregation, restrict delete)
```

### 14.3. Katman diyagramı için ek sınıflar (opsiyonel)

```
<<interface>> IRepository<T>           ← jenerik taban
   ▲
   │ extends
   │
[IOwnerRepository, IPetRepository, IServiceRepository, IAppointmentRepository]

[OwnerService]          --uses--> [IOwnerRepository]
[PetService]            --uses--> [IPetRepository], [IOwnerRepository]
[ServiceCatalogService] --uses--> [IServiceRepository], [IAppointmentRepository]
[AppointmentService]    --uses--> [IAppointmentRepository], [IPetRepository], [IServiceRepository]
[*Controller]           --uses--> [I*Service]
[*Repository]           --uses--> [ApplicationDbContext]
```

---

## 15. Rapor Outline (10–15 Sayfa)

Her bölüm için 2–3 paragraflık yönerge verilmiştir.

### Bölüm 1 — Giriş (≈1 sayfa)

- **Paragraf 1:** Veteriner kliniklerinin günlük operasyonel ihtiyaçları, dijitalleşme gerekliliği. Türkiye'de küçük kliniklerin mevcut yöntemlerine kısa bakış.
- **Paragraf 2:** Bu projenin amacı: kâğıt/Excel tabanlı süreçleri tek bir web panelinde birleştirmek. Hedef kitle: klinik personeli.
- **Paragraf 3:** Raporun yapısı: hangi bölümün ne içerdiğini özetleyen yol haritası.

### Bölüm 2 — Literatür Taraması (≈1.5 sayfa)

- **Paragraf 1:** **MVC Tasarım Deseni.** Trygve Reenskaug'un 1978'de Xerox PARC'ta ortaya koyduğu Model-View-Controller paradigmasının tarihçesi. Sorumluluk ayrımı (separation of concerns) prensibi: domain veri (Model), kullanıcı arayüzü (View), kullanıcı etkileşimi (Controller). Web uygulamaları bağlamında Ruby on Rails (2005) ile popülerleşmesi ve ASP.NET MVC (2009) ile .NET ekosistemine girişi. Akademik kaynak: Krasner & Pope (1988), "A Cookbook for Using the Model-View-Controller User Interface Paradigm in Smalltalk-80".
- **Paragraf 2:** **ASP.NET Core Evrimi.** Klasik .NET Framework üzerine kurulu ASP.NET (2002) → ASP.NET MVC (2009) → ASP.NET Core 1.0 (2016, cross-platform yeniden yazım) → ASP.NET Core 8 (2023, mevcut LTS). Cross-platform desteği, modüler middleware pipeline, built-in dependency injection container, ve `IHost` tabanlı startup modeli. Performans karşılaştırmaları (TechEmpower benchmarks) ile diğer framework'lere üstünlüğü. ASP.NET Core'un .NET 8 ile gelen native AOT, primary constructors, ve minimal API gibi yenilikleri.
- **Paragraf 3:** **Dependency Injection Prensibi.** Robert C. Martin'in SOLID prensiplerinden D (Dependency Inversion): "yüksek seviye modüller düşük seviye modüllere bağlı olmamalı; ikisi de soyutlamalara bağlı olmalı." Martin Fowler'ın 2004'teki "Inversion of Control Containers and the Dependency Injection pattern" makalesinde DI'nin IoC'nin özel bir formu olarak tanımlanması. Constructor injection, property injection, method injection karşılaştırması. ASP.NET Core'un built-in DI container'ında Singleton/Scoped/Transient lifetime yönetimi. Mark Seemann'ın "Dependency Injection in .NET" kitabına referans.

> **Yazım notu:** Bu bölümde her iddia kaynağa bağlanmalı; en az 5–7 akademik veya endüstri kaynağı (makale, kitap, Microsoft Docs) dipnotla/kaynakçaya eklenmeli. Spec §3'teki "Teknoloji Seçimleri" bu bölümün **uygulamalı** karşılığıdır (kavram → bu projede nasıl kullanıldı).

### Bölüm 3 — Problem Tanımı ve Gereksinim Analizi (≈1.5 sayfa)

- **Paragraf 1:** Spec'in §2'sinde tanımlanan P1–P5 problemlerinin detaylandırılması. Her problem için somut örnek senaryo.
- **Paragraf 2:** Fonksiyonel gereksinimler: CRUD işlemleri, çakışma kontrolü, dashboard. Maddeler halinde liste.
- **Paragraf 3:** Fonksiyonel olmayan gereksinimler: responsive UI, async I/O, validasyon, hata mesajları Türkçe.

### Bölüm 4 — Teknoloji Seçimleri (≈1.5 sayfa)

- **Paragraf 1:** Neden ASP.NET Core 8? Razor MVC vs Blazor vs API+SPA karşılaştırması; eğitim amaçlı şeffaflık nedeniyle Razor MVC tercih edildi.
- **Paragraf 2:** Neden EF Core Code First? Migration kolaylığı, OOP-veritabanı sürekliliği. Database First ile karşılaştırma.
- **Paragraf 3:** Neden SQLite? Kurulum kolaylığı, dosya tabanlı, lokal LAN ortamı için yeterli. SQL Server / PostgreSQL'e taşıma kolaylığı (provider değişikliği yeterli).

### Bölüm 5 — Sistem Mimarisi (≈2 sayfa)

- **Paragraf 1:** Katmanlı mimari açıklaması; Controller → Service → Repository → DbContext akışı. Şema (spec §3.2'deki blok diyagram).
- **Paragraf 2:** Dependency Injection ve interface segregation'ın faydaları (Bölüm 2'deki kavramların bu projedeki somut uygulaması). Generic `IRepository<T>` deseni ile DRY ve test edilebilirlik. Lifetime kararları (Scoped neden seçildi).
- **Paragraf 3:** ViewModel pattern: neden Entity'ler View'a gönderilmiyor. Aşırı veri sızıntısı (over-posting), sıkı bağ (tight coupling), ve validation yüzeyi (attack surface) konuları.

### Bölüm 6 — Veri Modeli (≈2 sayfa)

- **Paragraf 1:** 4 entity'nin (Owner, Pet, Service, Appointment) iş anlamı. Her birinin spec §5'teki alanları özetlenir. Snapshot alanlarının (EndTime, ServicePriceSnapshot) veri bütünlüğü gerekçesi.
- **Paragraf 2:** İlişkiler ve cascade davranışları. Neden Service için Restrict, neden Owner→Pet için Cascade. Owner.Phone üzerindeki unique index'in race condition senaryosunda rolü. UML diyagramı (spec §14) buraya gömülür.
- **Paragraf 3:** Validasyon stratejisi: Data Annotations ve özel doğrulamalar (tarih, telefon, çakışma). Validasyon katmanları: client-side (jQuery Validation) + server-side (ModelState) + business (Service katmanı `Result`).

### Bölüm 7 — Business Logic (≈2 sayfa)

- **Paragraf 1:** Tarih validasyonu adımları (geçmiş tarih, mesai saati, pazar günü, sığma kontrolü). Pseudocode örneği.
- **Paragraf 2:** Çakışma kontrolü algoritması: `A.start < B.end AND A.end > B.start` formülü ve neden çalıştığı. `EndTime` snapshot'ının SQLite uyumluluğundaki rolü. İptal edilmiş randevuların kontrole dahil edilmemesi.
- **Paragraf 3:** Yaş hesaplama algoritması ve durum geçiş diyagramı (Beklemede → Onaylandı → Tamamlandı / IptalEdildi). Terminal durumların değiştirilemezliği.

### Bölüm 8 — Kullanıcı Arayüzü ve Akışlar (≈1 sayfa)

- **Paragraf 1:** Dashboard'un içeriği: 4 stat kartı, bugünün randevuları, en çok talep edilen hizmetler.
- **Paragraf 2:** Tipik kullanım akışı: Yeni müşteri geldiğinde önce Owner kaydı, sonra Pet, sonra Appointment oluşturma. Akış diyagramı (UML activity diagram veya basit flowchart).
- **Paragraf 3:** Bootstrap 5 ile responsive tasarım; renkli durum badge'leri; flash mesajlar (TempData ile).

### Bölüm 9 — Uygulama Ekran Görüntüleri (≈2 sayfa)

Bu bölüm, çalışan uygulamanın ana ekranlarını görsel olarak sergilemek için ayrılmıştır. **10–12 ekran görüntüsü** yerleştirilir; her birinin altında 1–2 cümlelik açıklama (ne gösteriyor, hangi özellik vurgulanıyor) bulunur.

Gerekli ekran görüntüleri:
1. **Dashboard** (anasayfa, 4 stat kartı + bugünün randevuları + popüler hizmetler)
2. **Owners — Index** (tüm sahipler listesi, tablo görünümü)
3. **Owners — Details** (bir sahibin detayları + bağlı hayvanlar listesi)
4. **Owners — Create** (yeni sahip ekleme formu, validation mesajları görünür durumda — örn. yanlış telefon formatı)
5. **Pets — Index** (renkli tür badge'leri görünür)
6. **Pets — Details** (randevu geçmişi tablosu görünür)
7. **Services — Index** (Aktif/Pasif sekmeleri görünür)
8. **Appointments — Index** (filtre satırı + durum badge'leri görünür)
9. **Appointments — Create** (form, gelecek tarih seçilmiş)
10. **Appointments — Create** (çakışma hatası senaryosu — kırmızı uyarı mesajı görünür)
11. **Delete onay sayfası** (Owner cascade uyarısı — "{N} hayvan ve randevuları silinecek")
12. **Mobil/tablet görünüm** (responsive tasarımı kanıtlamak için tek bir ekran)

### Bölüm 10 — Veritabanı ve Migration (≈1 sayfa)

- **Paragraf 1:** SQLite dosyasının yapısı, EF Core migration mantığı (`__EFMigrationsHistory` tablosu).
- **Paragraf 2:** Seed data yaklaşımı: uygulama ilk açıldığında veritabanı boşsa örnek kayıtlar eklenir. Avantajı: demo ve test kolaylığı.

### Bölüm 11 — Karşılaşılan Zorluklar ve Çözümler (≈1.5 sayfa)

- **Paragraf 1:** **Çakışma kontrolü + SQLite uyumsuzluğu.** İlk denemede `AppointmentDate.AddMinutes(Service.DurationMinutes)` LINQ ifadesi SQLite'da çevrilemedi. Çözüm: `EndTime` alanını fiziksel sütun olarak entity'ye ekledik (snapshot). Hem bu sorunu çözdü hem de veri bütünlüğüne katkı sağladı.
- **Paragraf 2:** **Cascade delete'in beklenmedik sonuçları** (örn. service silindiğinde geçmiş randevuların kaybolması). Çözüm: Service için Restrict + IsActive ile soft delete. Tek "Sil" butonu yerine ayrı Pasifleştir/Sil butonları ile UX netleştirildi.
- **Paragraf 3:** **Race condition ve veri tutarlılığı.** İki request aynı anda gelirse `PhoneExistsAsync` kontrolü ikisi için de "yok" döner. Çözüm: DB seviyesinde unique index + Service katmanında `DbUpdateException` yakalama.

### Bölüm 12 — Geliştirme Önerileri ve Sonuç (≈1 sayfa)

- **Paragraf 1:** İlerideki olası geliştirmeler: kimlik doğrulama (Identity), public müşteri portalı, SMS hatırlatma (Twilio), Excel/PDF rapor export, çoklu poliklinik desteği, Chart.js ile zenginleştirilmiş dashboard grafikleri.
- **Paragraf 2:** Projeden öğrenilenler: katmanlı mimarinin değeri, EF Core ilişki yönetimi, validasyon stratejileri, snapshot pattern ile veri bütünlüğü.
- **Paragraf 3:** Sonuç: spec §2'deki P1–P5 problemlerinin nasıl çözüldüğünün kısa özeti.

### Ekler

- **Ek A:** UML sınıf diyagramı (tam sayfa)
- **Ek B:** Veritabanı tablo yapıları (her tablo için kolon listesi, tip, kısıtlar)
- **Ek C:** Kaynakça (Bölüm 2'deki referansların tam künyesi)

> **Sayfa sayısı tahmini:** 1 + 1.5 + 1.5 + 1.5 + 2 + 2 + 2 + 1 + 2 + 1 + 1.5 + 1 = **17 sayfa** (ekler hariç). Hocanın 10–15 sayfa hedefi için Bölüm 2 (Literatür) 1 sayfaya, Bölüm 9 (Ekran Görüntüleri) 1.5 sayfaya, Bölüm 6 (Veri Modeli) 1.5 sayfaya çekilebilir → ~13 sayfa.

---

## 16. README Taslağı

Aşağıdaki içerik proje kökündeki `README.md` için kullanılır.

```markdown
# Veteriner Klinik Randevu Yönetim Sistemi

ASP.NET Core 8 + Razor MVC + Entity Framework Core (SQLite) ile geliştirilmiş,
küçük ve orta ölçekli veteriner klinikleri için iç kullanıma yönelik
randevu yönetim paneli.

## Özellikler

- Hayvan sahibi (Owner) yönetimi
- Evcil hayvan (Pet) kayıtları ve yaş hesaplama
- Klinik hizmet (Service) tanımları ve fiyatlandırma
- Randevu oluşturma, çakışma kontrolü, durum yönetimi
- Dashboard: günlük/haftalık özet, en çok talep edilen hizmetler
- Responsive Bootstrap 5 arayüzü

## Teknoloji Yığını

- ASP.NET Core 8.0
- Razor View Engine (MVC)
- Entity Framework Core 8 (Code First)
- SQLite
- Bootstrap 5.3

## Mimari

Katmanlı: **Controllers → Services → Repositories → DbContext**
Dependency Injection ile interface üzerinden kayıtlı tüm bileşenler.
ViewModel pattern ile View ↔ Entity ayrımı.

## Kurulum

### Gereksinimler

- .NET 8 SDK
- (Opsiyonel) `dotnet-ef` global tool

### Adımlar

1. Repoyu klonlayın:
   ```
   git clone <repo-url>
   cd VetClinic
   ```

2. Bağımlılıkları yükleyin:
   ```
   dotnet restore
   ```

3. Veritabanını oluşturun:
   ```
   dotnet ef database update
   ```
   (Migration uygulanır; eğer ilk kurulumsa `vetclinic.db` dosyası oluşur.)

4. Uygulamayı çalıştırın:
   ```
   dotnet run
   ```

5. Tarayıcıdan açın:
   ```
   https://localhost:5001
   ```

İlk açılışta seed data otomatik yüklenir (3 sahip, 5 hayvan, 6 hizmet, 4 randevu).

## Proje Yapısı

```
VetClinic/
├── Controllers/      # MVC controller'lar (thin)
├── Models/           # Entity sınıfları + enum'lar
├── ViewModels/       # View'lara özel modeller
├── Data/             # DbContext, Migrations, SeedData
├── Repositories/     # Veri erişimi (interface + impl)
├── Services/         # İş kuralları (interface + impl)
├── Views/            # Razor view'lar
└── wwwroot/          # Statik dosyalar (CSS, JS, lib)
```

## Veri Modeli

4 ana entity:
- **Owner** (1) ──< **Pet** (N)
- **Pet** (1) ──< **Appointment** (N)
- **Service** (1) ──< **Appointment** (N)

Detaylı şema için `docs/spec.md` ve `docs/uml-diagram.png` dosyalarına bakınız.

## İş Kuralları

- Randevular sadece **mesai saatleri** (09:00–19:00) içinde alınabilir.
- **Pazar günleri** randevu alınamaz.
- Aynı saat aralığında **çakışma** varsa yeni randevu oluşturulamaz.
- Geçmiş randevusu olan hizmet **silinemez**; pasifleştirilir.
- Sahip silinince ona ait tüm hayvanlar ve randevular **otomatik silinir**.

## Migration Yönetimi

Yeni alan eklemek için:
```
dotnet ef migrations add <MigrationAdi>
dotnet ef database update
```

Veritabanını sıfırlamak için (sadece dev):
```
dotnet ef database drop -f
dotnet ef database update
```

## Lisans

Akademik proje. Eğitim amaçlı kullanılabilir.
```

---

## Sözlük

| Terim | Açıklama |
|---|---|
| Owner | Hayvan sahibi (müşteri) |
| Pet | Evcil hayvan |
| Service | Klinikte sunulan bir hizmet (aşı, muayene vb.) |
| Appointment | Belirli bir hayvan için belirli bir hizmetin alındığı randevu |
| Cascade delete | Üst kayıt silindiğinde bağlı alt kayıtların da silinmesi |
| Soft delete | Kaydın fiziksel olarak silinmemesi, sadece pasif işaretlenmesi (`IsActive=false`) |
| ViewModel | View'a özel olarak şekillendirilmiş model sınıfı |
| Thin Controller | İş kuralı içermeyen, kısa action'lara sahip controller |
| Captive dependency | Uzun ömürlü bir bileşenin kısa ömürlü bir bileşeni tutması (anti-pattern) |

---

**Doküman sonu.**
