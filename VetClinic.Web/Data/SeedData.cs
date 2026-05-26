using VetClinic.Web.Models.Entities;
using VetClinic.Web.Models.Enums;

namespace VetClinic.Web.Data;

public static class SeedData
{
    public static void Initialize(ApplicationDbContext db)
    {
        // Veritabanı zaten doluysa hiçbir şey yapma (idempotent).
        if (db.Owners.Any() || db.Services.Any())
            return;

        var now = DateTime.Now;

        // --- Owners (3) ---
        var owners = new List<Owner>
        {
            new() { FullName = "Ayşe Yılmaz",  Phone = "05321112233", Email = "ayse@example.com",   Address = "İzmir/Karşıyaka", CreatedAt = now },
            new() { FullName = "Mehmet Demir", Phone = "05334445566", Email = "mehmet@example.com", Address = "İzmir/Bornova",   CreatedAt = now },
            new() { FullName = "Selin Kara",   Phone = "05447778899", Email = null,                 Address = "İzmir/Konak",     CreatedAt = now },
        };
        db.Owners.AddRange(owners);
        db.SaveChanges();

        // --- Services (6) ---
        var services = new List<Service>
        {
            new() { Name = "Genel Muayene",                 DurationMinutes = 30, Price = 350m,  IsActive = true },
            new() { Name = "Aşı (Tek Doz)",                 DurationMinutes = 20, Price = 250m,  IsActive = true },
            new() { Name = "Kısırlaştırma — Kedi (Dişi)",   DurationMinutes = 90, Price = 4500m, IsActive = true },
            new() { Name = "Kısırlaştırma — Köpek (Erkek)", DurationMinutes = 60, Price = 3500m, IsActive = true },
            new() { Name = "Diş Temizliği",                 DurationMinutes = 60, Price = 1800m, IsActive = true },
            new() { Name = "Tırnak Kesimi",                 DurationMinutes = 15, Price = 150m,  IsActive = true },
        };
        db.Services.AddRange(services);
        db.SaveChanges();

        // --- Pets (5) — owner'larla ilişkili ---
        var pets = new List<Pet>
        {
            new() { Name = "Pamuk",   Species = PetSpecies.Kedi,  Breed = "Tekir",            BirthDate = new DateTime(2022, 3, 10), Gender = PetGender.Disi,  Weight = 4.2m,  OwnerId = owners[0].Id },
            new() { Name = "Boncuk",  Species = PetSpecies.Kedi,  Breed = "Van",              BirthDate = new DateTime(2020, 6, 5),  Gender = PetGender.Erkek, Weight = 5.0m,  OwnerId = owners[0].Id },
            new() { Name = "Karabaş", Species = PetSpecies.Kopek, Breed = "Golden Retriever", BirthDate = new DateTime(2019, 1, 20), Gender = PetGender.Erkek, Weight = 28.5m, OwnerId = owners[1].Id },
            new() { Name = "Çıtçıt",  Species = PetSpecies.Kus,   Breed = "Muhabbet Kuşu",    BirthDate = new DateTime(2024, 2, 1),  Gender = PetGender.Erkek, Weight = 0.05m, OwnerId = owners[2].Id },
            new() { Name = "Maya",    Species = PetSpecies.Kopek, Breed = "Pomeranian",       BirthDate = new DateTime(2023, 9, 15), Gender = PetGender.Disi,  Weight = 3.1m,  OwnerId = owners[2].Id },
        };
        db.Pets.AddRange(pets);
        db.SaveChanges();

        // --- Appointments (4) — gelecek tarihli; snapshot alanları doldurulur ---
        var appointments = new List<Appointment>
        {
            BuildAppointment(pets[0], services[0], DateTime.Today.AddDays(1).AddHours(10), AppointmentStatus.Onaylandi, now),
            BuildAppointment(pets[2], services[1], DateTime.Today.AddDays(2).AddHours(14), AppointmentStatus.Beklemede, now),
            BuildAppointment(pets[4], services[5], DateTime.Today.AddDays(3).AddHours(11), AppointmentStatus.Beklemede, now),
            BuildAppointment(pets[1], services[4], DateTime.Today.AddDays(5).AddHours(15), AppointmentStatus.Onaylandi, now),
        };
        db.Appointments.AddRange(appointments);
        db.SaveChanges();
    }

    private static Appointment BuildAppointment(Pet pet, Service service, DateTime start, AppointmentStatus status, DateTime createdAt)
        => new()
        {
            PetId = pet.Id,
            ServiceId = service.Id,
            AppointmentDate = start,
            EndTime = start.AddMinutes(service.DurationMinutes),
            ServicePriceSnapshot = service.Price,
            Status = status,
            CreatedAt = createdAt,
        };
}
