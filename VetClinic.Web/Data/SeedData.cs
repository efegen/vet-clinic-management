using VetClinic.Web.Models.Entities;
using VetClinic.Web.Models.Enums;

namespace VetClinic.Web.Data;

public static class SeedData
{
    public static void Initialize(ApplicationDbContext db)
    {
        if (db.Owners.Any() || db.Services.Any())
            return;

        var now = DateTime.Now;

        // --- Services (10) ---
        var allSpecies = new List<PetSpecies> { PetSpecies.Kopek, PetSpecies.Kedi, PetSpecies.Kus, PetSpecies.Tavsan, PetSpecies.Diger };
        var services = new List<Service>
        {
            new() { Name = "Genel Muayene",                  DurationMinutes = 30,  Price = 350m,   IsActive = true, ApplicableSpecies = new(allSpecies) },
            new() { Name = "Aşı (Tek Doz)",                  DurationMinutes = 20,  Price = 250m,   IsActive = true, ApplicableSpecies = new(allSpecies) },
            new() { Name = "Kısırlaştırma — Kedi (Dişi)",    DurationMinutes = 90,  Price = 4500m,  IsActive = true, ApplicableSpecies = new() { PetSpecies.Kedi } },
            new() { Name = "Kısırlaştırma — Köpek (Erkek)",  DurationMinutes = 60,  Price = 3500m,  IsActive = true, ApplicableSpecies = new() { PetSpecies.Kopek } },
            new() { Name = "Diş Temizliği",                  DurationMinutes = 60,  Price = 1800m,  IsActive = true, ApplicableSpecies = new() { PetSpecies.Kopek, PetSpecies.Kedi } },
            new() { Name = "Tırnak Kesimi",                  DurationMinutes = 15,  Price = 150m,   IsActive = true, ApplicableSpecies = new() { PetSpecies.Kopek, PetSpecies.Kedi, PetSpecies.Kus, PetSpecies.Tavsan } },
            new() { Name = "Kan Tahlili",                    DurationMinutes = 20,  Price = 450m,   IsActive = true, ApplicableSpecies = new(allSpecies) },
            new() { Name = "Ultrason",                       DurationMinutes = 30,  Price = 700m,   IsActive = true, ApplicableSpecies = new() { PetSpecies.Kopek, PetSpecies.Kedi } },
            new() { Name = "Kene / Pire Tedavisi",           DurationMinutes = 20,  Price = 300m,   IsActive = true, ApplicableSpecies = new() { PetSpecies.Kopek, PetSpecies.Kedi, PetSpecies.Tavsan } },
            new() { Name = "Röntgen",                        DurationMinutes = 40,  Price = 900m,   IsActive = true, ApplicableSpecies = new(allSpecies) },
        };
        db.Services.AddRange(services);
        db.SaveChanges();

        // --- Owners (12) ---
        var owners = new List<Owner>
        {
            new() { FullName = "Ayşe Yılmaz",       Phone = "05321112233", Email = "ayse.yilmaz@example.com",     Address = "İzmir / Karşıyaka",  CreatedAt = now.AddDays(-180) },
            new() { FullName = "Mehmet Demir",       Phone = "05334445566", Email = "mehmet.demir@example.com",    Address = "İzmir / Bornova",     CreatedAt = now.AddDays(-160) },
            new() { FullName = "Selin Kara",         Phone = "05447778899", Email = null,                          Address = "İzmir / Konak",       CreatedAt = now.AddDays(-140) },
            new() { FullName = "Emre Çelik",         Phone = "05359998877", Email = "emre.celik@example.com",      Address = "İzmir / Alsancak",    CreatedAt = now.AddDays(-120) },
            new() { FullName = "Zeynep Arslan",      Phone = "05423334455", Email = "zeynep.arslan@example.com",   Address = "İzmir / Çiğli",       CreatedAt = now.AddDays(-100) },
            new() { FullName = "Burak Şahin",        Phone = "05316667788", Email = null,                          Address = "İzmir / Gaziemir",    CreatedAt = now.AddDays(-90)  },
            new() { FullName = "Canan Öztürk",       Phone = "05441234567", Email = "canan.ozturk@example.com",    Address = "İzmir / Balçova",     CreatedAt = now.AddDays(-75)  },
            new() { FullName = "Tarık Yıldız",       Phone = "05327654321", Email = "tarik.yildiz@example.com",    Address = "İzmir / Narlıdere",   CreatedAt = now.AddDays(-60)  },
            new() { FullName = "Meltem Güneş",       Phone = "05459876543", Email = "meltem.gunes@example.com",    Address = "İzmir / Urla",        CreatedAt = now.AddDays(-45)  },
            new() { FullName = "Serkan Aydın",       Phone = "05302345678", Email = null,                          Address = "İzmir / Buca",        CreatedAt = now.AddDays(-30)  },
            new() { FullName = "Figen Doğan",        Phone = "05338765432", Email = "figen.dogan@example.com",     Address = "İzmir / Menderes",    CreatedAt = now.AddDays(-20)  },
            new() { FullName = "Uğur Polat",         Phone = "05461112233", Email = "ugur.polat@example.com",      Address = "İzmir / Torbalı",     CreatedAt = now.AddDays(-10)  },
        };
        db.Owners.AddRange(owners);
        db.SaveChanges();

        // --- Pets (22) ---
        var pets = new List<Pet>
        {
            // Ayşe Yılmaz (owners[0]) — 2 kedi
            new() { Name = "Pamuk",    Species = PetSpecies.Kedi,   Breed = "Tekir",              BirthDate = new DateTime(2022, 3, 10),  Gender = PetGender.Disi,  Weight = 4.2m,  OwnerId = owners[0].Id },
            new() { Name = "Boncuk",   Species = PetSpecies.Kedi,   Breed = "Van",                BirthDate = new DateTime(2020, 6, 5),   Gender = PetGender.Erkek, Weight = 5.0m,  OwnerId = owners[0].Id },

            // Mehmet Demir (owners[1]) — 2 köpek
            new() { Name = "Karabaş",  Species = PetSpecies.Kopek,  Breed = "Golden Retriever",   BirthDate = new DateTime(2019, 1, 20),  Gender = PetGender.Erkek, Weight = 28.5m, OwnerId = owners[1].Id },
            new() { Name = "Fıstık",   Species = PetSpecies.Kopek,  Breed = "Labrador",           BirthDate = new DateTime(2021, 4, 12),  Gender = PetGender.Disi,  Weight = 22.0m, OwnerId = owners[1].Id },

            // Selin Kara (owners[2]) — kuş + köpek
            new() { Name = "Çıtçıt",   Species = PetSpecies.Kus,    Breed = "Muhabbet Kuşu",      BirthDate = new DateTime(2024, 2, 1),   Gender = PetGender.Erkek, Weight = 0.05m, OwnerId = owners[2].Id },
            new() { Name = "Maya",     Species = PetSpecies.Kopek,  Breed = "Pomeranian",         BirthDate = new DateTime(2023, 9, 15),  Gender = PetGender.Disi,  Weight = 3.1m,  OwnerId = owners[2].Id },

            // Emre Çelik (owners[3]) — tavşan + kedi
            new() { Name = "Bulut",    Species = PetSpecies.Tavsan, Breed = "Hollanda Lop",       BirthDate = new DateTime(2023, 5, 20),  Gender = PetGender.Disi,  Weight = 1.8m,  OwnerId = owners[3].Id },
            new() { Name = "Şeker",    Species = PetSpecies.Kedi,   Breed = "Scottish Fold",      BirthDate = new DateTime(2021, 11, 8),  Gender = PetGender.Disi,  Weight = 3.7m,  OwnerId = owners[3].Id },

            // Zeynep Arslan (owners[4]) — köpek
            new() { Name = "Aslan",    Species = PetSpecies.Kopek,  Breed = "German Shepherd",    BirthDate = new DateTime(2018, 7, 3),   Gender = PetGender.Erkek, Weight = 34.0m, OwnerId = owners[4].Id },

            // Burak Şahin (owners[5]) — kedi + köpek
            new() { Name = "Minnoş",   Species = PetSpecies.Kedi,   Breed = "British Shorthair",  BirthDate = new DateTime(2022, 9, 18),  Gender = PetGender.Disi,  Weight = 4.5m,  OwnerId = owners[5].Id },
            new() { Name = "Rex",      Species = PetSpecies.Kopek,  Breed = "Beagle",             BirthDate = new DateTime(2020, 12, 25), Gender = PetGender.Erkek, Weight = 10.2m, OwnerId = owners[5].Id },

            // Canan Öztürk (owners[6]) — kedi
            new() { Name = "Zeytin",   Species = PetSpecies.Kedi,   Breed = "Ankara Kedisi",      BirthDate = new DateTime(2019, 3, 14),  Gender = PetGender.Disi,  Weight = 4.8m,  OwnerId = owners[6].Id },

            // Tarık Yıldız (owners[7]) — köpek + kuş
            new() { Name = "Çakır",    Species = PetSpecies.Kopek,  Breed = "Husky",              BirthDate = new DateTime(2021, 8, 22),  Gender = PetGender.Erkek, Weight = 25.0m, OwnerId = owners[7].Id },
            new() { Name = "Yeşil",    Species = PetSpecies.Kus,    Breed = "Sultan Papağanı",    BirthDate = new DateTime(2022, 6, 10),  Gender = PetGender.Erkek, Weight = 0.08m, OwnerId = owners[7].Id },

            // Meltem Güneş (owners[8]) — kedi + tavşan
            new() { Name = "Pepe",     Species = PetSpecies.Kedi,   Breed = "Ragdoll",            BirthDate = new DateTime(2023, 1, 30),  Gender = PetGender.Erkek, Weight = 5.5m,  OwnerId = owners[8].Id },
            new() { Name = "Flop",     Species = PetSpecies.Tavsan, Breed = "Rex Tavşanı",        BirthDate = new DateTime(2024, 3, 5),   Gender = PetGender.Disi,  Weight = 2.2m,  OwnerId = owners[8].Id },

            // Serkan Aydın (owners[9]) — köpek
            new() { Name = "Baron",    Species = PetSpecies.Kopek,  Breed = "Rottweiler",         BirthDate = new DateTime(2020, 10, 8),  Gender = PetGender.Erkek, Weight = 42.0m, OwnerId = owners[9].Id },

            // Figen Doğan (owners[10]) — kedi + diğer
            new() { Name = "Papatya",  Species = PetSpecies.Kedi,   Breed = "Tekir",              BirthDate = new DateTime(2024, 4, 22),  Gender = PetGender.Disi,  Weight = 2.9m,  OwnerId = owners[10].Id },
            new() { Name = "Kaplumbağa", Species = PetSpecies.Diger, Breed = "Kara Kaplumbağa",  BirthDate = new DateTime(2015, 6, 1),   Gender = PetGender.Erkek, Weight = 0.6m,  OwnerId = owners[10].Id },

            // Uğur Polat (owners[11]) — köpek
            new() { Name = "Loki",     Species = PetSpecies.Kopek,  Breed = "Border Collie",      BirthDate = new DateTime(2022, 11, 17), Gender = PetGender.Erkek, Weight = 17.5m, OwnerId = owners[11].Id },
            new() { Name = "Luna",     Species = PetSpecies.Kedi,   Breed = "Maine Coon",         BirthDate = new DateTime(2021, 7, 9),   Gender = PetGender.Disi,  Weight = 6.1m,  OwnerId = owners[11].Id },

            // Canan Öztürk (owners[6]) — ikinci hayvan
            new() { Name = "Mırnav",   Species = PetSpecies.Kedi,   Breed = "Sphynx",             BirthDate = new DateTime(2023, 2, 14),  Gender = PetGender.Disi,  Weight = 3.4m,  OwnerId = owners[6].Id },
        };
        db.Pets.AddRange(pets);
        db.SaveChanges();

        // shorthand
        var sGenelMuayene = services[0]; // Genel Muayene (all)
        var sAsi          = services[1]; // Aşı (all)
        var sKisirlasKedi = services[2]; // Kısırlaştırma Kedi Dişi
        var sKisirlasKopek = services[3];// Kısırlaştırma Köpek Erkek
        var sDisTem       = services[4]; // Diş Temizliği (köpek, kedi)
        var sTirnak       = services[5]; // Tırnak Kesimi
        var sKanTahlili   = services[6]; // Kan Tahlili (all)
        var sUltrason     = services[7]; // Ultrason (köpek, kedi)
        var sKenePire     = services[8]; // Kene/Pire (köpek, kedi, tavşan)
        var sRontgen      = services[9]; // Röntgen (all)

        var today = DateTime.Today;

        var appointments = new List<Appointment>();

        // ---- GEÇMİŞ RANDEVUlar (Tamamlandi / IptalEdildi) ----

        // 3 hafta önce
        appointments.Add(Build(pets[2],  sGenelMuayene, today.AddDays(-21).AddHours(9),  AppointmentStatus.Tamamlandi, now.AddDays(-22)));
        appointments.Add(Build(pets[0],  sAsi,          today.AddDays(-20).AddHours(10), AppointmentStatus.Tamamlandi, now.AddDays(-21)));
        appointments.Add(Build(pets[8],  sDisTem,       today.AddDays(-19).AddHours(11), AppointmentStatus.Tamamlandi, now.AddDays(-20)));
        appointments.Add(Build(pets[10], sKenePire,     today.AddDays(-18).AddHours(14), AppointmentStatus.Tamamlandi, now.AddDays(-19)));
        appointments.Add(Build(pets[12], sGenelMuayene, today.AddDays(-17).AddHours(9),  AppointmentStatus.IptalEdildi, now.AddDays(-18)));
        appointments.Add(Build(pets[1],  sDisTem,       today.AddDays(-16).AddHours(15), AppointmentStatus.Tamamlandi, now.AddDays(-17)));

        // 2 hafta önce
        appointments.Add(Build(pets[6],  sTirnak,       today.AddDays(-14).AddHours(9),  AppointmentStatus.Tamamlandi, now.AddDays(-15)));
        appointments.Add(Build(pets[3],  sKisirlasKopek,today.AddDays(-14).AddHours(11), AppointmentStatus.Tamamlandi, now.AddDays(-15)));
        appointments.Add(Build(pets[7],  sKisirlasKedi, today.AddDays(-13).AddHours(10), AppointmentStatus.Tamamlandi, now.AddDays(-14)));
        appointments.Add(Build(pets[9],  sGenelMuayene, today.AddDays(-12).AddHours(14), AppointmentStatus.IptalEdildi, now.AddDays(-13)));
        appointments.Add(Build(pets[11], sKanTahlili,   today.AddDays(-11).AddHours(9),  AppointmentStatus.Tamamlandi, now.AddDays(-12)));
        appointments.Add(Build(pets[5],  sAsi,          today.AddDays(-10).AddHours(11), AppointmentStatus.Tamamlandi, now.AddDays(-11)));
        appointments.Add(Build(pets[15], sRontgen,      today.AddDays(-10).AddHours(13), AppointmentStatus.Tamamlandi, now.AddDays(-11)));

        // 1 hafta önce
        appointments.Add(Build(pets[16], sGenelMuayene, today.AddDays(-7).AddHours(9),   AppointmentStatus.Tamamlandi, now.AddDays(-8)));
        appointments.Add(Build(pets[19], sDisTem,       today.AddDays(-7).AddHours(10),  AppointmentStatus.Tamamlandi, now.AddDays(-8)));
        appointments.Add(Build(pets[4],  sTirnak,       today.AddDays(-6).AddHours(9),   AppointmentStatus.Tamamlandi, now.AddDays(-7)));
        appointments.Add(Build(pets[2],  sKanTahlili,   today.AddDays(-5).AddHours(14),  AppointmentStatus.Tamamlandi, now.AddDays(-6)));
        appointments.Add(Build(pets[8],  sUltrason,     today.AddDays(-4).AddHours(11),  AppointmentStatus.IptalEdildi, now.AddDays(-5)));
        appointments.Add(Build(pets[13], sAsi,          today.AddDays(-3).AddHours(9),   AppointmentStatus.Tamamlandi, now.AddDays(-4)));
        appointments.Add(Build(pets[20], sGenelMuayene, today.AddDays(-3).AddHours(10),  AppointmentStatus.Tamamlandi, now.AddDays(-4)));
        appointments.Add(Build(pets[17], sKanTahlili,   today.AddDays(-2).AddHours(15),  AppointmentStatus.Tamamlandi, now.AddDays(-3)));
        appointments.Add(Build(pets[0],  sDisTem,       today.AddDays(-1).AddHours(9),   AppointmentStatus.Tamamlandi, now.AddDays(-2)));
        appointments.Add(Build(pets[21], sGenelMuayene, today.AddDays(-1).AddHours(11),  AppointmentStatus.Tamamlandi, now.AddDays(-2)));

        // ---- BUGÜN ----
        // Bugün Pazar mı? Cumartesi mi? kontrol et; Pazar kapalı, o yüzden bugünü ata
        if (today.DayOfWeek != DayOfWeek.Sunday)
        {
            appointments.Add(Build(pets[2],  sGenelMuayene, today.AddHours(9),  AppointmentStatus.Onaylandi,  now.AddDays(-1)));
            appointments.Add(Build(pets[10], sTirnak,       today.AddHours(10), AppointmentStatus.Onaylandi,  now.AddDays(-1)));
            appointments.Add(Build(pets[6],  sAsi,          today.AddHours(11), AppointmentStatus.Beklemede,  now.AddDays(-1)));
            appointments.Add(Build(pets[1],  sKanTahlili,   today.AddHours(13), AppointmentStatus.Onaylandi,  now.AddDays(-1)));
            appointments.Add(Build(pets[19], sDisTem,       today.AddHours(14), AppointmentStatus.Beklemede,  now.AddDays(-1)));
            appointments.Add(Build(pets[3],  sUltrason,     today.AddHours(15), AppointmentStatus.Onaylandi,  now.AddDays(-1)));
        }

        // ---- GELECEK (Beklemede / Onaylandi) ----
        // Sonraki 14 gün içine yay; Pazar günlerini atla
        var futureDates = new List<(int daysAhead, int hour)>
        {
            (1, 9), (1, 10), (1, 11), (1, 14), (1, 15),
            (2, 9), (2, 11), (2, 14),
            (3, 9), (3, 10), (3, 13), (3, 16),
            (5, 9), (5, 11), (5, 14),
            (6, 10), (6, 15),
            (7, 9), (7, 11),
            (8, 10), (8, 14),
            (10, 9), (10, 11),
            (12, 9), (12, 13),
            (14, 10), (14, 14),
        };

        // pet-service eşleşmeleri (türe uygun)
        var futureSlots = new (Pet pet, Service svc)[]
        {
            (pets[5],  sGenelMuayene),
            (pets[8],  sDisTem),
            (pets[11], sAsi),
            (pets[7],  sKisirlasKedi),
            (pets[16], sGenelMuayene),
            (pets[9],  sKenePire),
            (pets[12], sTirnak),
            (pets[14], sAsi),
            (pets[15], sRontgen),
            (pets[20], sDisTem),
            (pets[4],  sTirnak),
            (pets[13], sAsi),
            (pets[21], sUltrason),
            (pets[17], sKanTahlili),
            (pets[18], sGenelMuayene),
            (pets[0],  sAsi),
            (pets[3],  sKisirlasKopek),
            (pets[10], sGenelMuayene),
            (pets[6],  sTirnak),
            (pets[2],  sKanTahlili),
            (pets[1],  sUltrason),
            (pets[19], sKenePire),
            (pets[8],  sGenelMuayene),
            (pets[16], sRontgen),
            (pets[5],  sAsi),
            (pets[21], sGenelMuayene),
            (pets[14], sTirnak),
        };

        int slotIdx = 0;
        foreach (var (daysAhead, hour) in futureDates)
        {
            if (slotIdx >= futureSlots.Length) break;
            var date = today.AddDays(daysAhead);
            if (date.DayOfWeek == DayOfWeek.Sunday) continue;
            var status = (slotIdx % 3 == 0) ? AppointmentStatus.Onaylandi : AppointmentStatus.Beklemede;
            appointments.Add(Build(futureSlots[slotIdx].pet, futureSlots[slotIdx].svc, date.AddHours(hour), status, now.AddDays(-1)));
            slotIdx++;
        }

        db.Appointments.AddRange(appointments);
        db.SaveChanges();
    }

    private static Appointment Build(Pet pet, Service service, DateTime start, AppointmentStatus status, DateTime createdAt, string? notes = null)
        => new()
        {
            PetId                = pet.Id,
            ServiceId            = service.Id,
            AppointmentDate      = start,
            EndTime              = start.AddMinutes(service.DurationMinutes),
            ServicePriceSnapshot = service.Price,
            Status               = status,
            Notes                = notes,
            CreatedAt            = createdAt,
        };
}
