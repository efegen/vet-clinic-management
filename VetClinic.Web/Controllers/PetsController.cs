using Microsoft.AspNetCore.Mvc;
using VetClinic.Web.Models.Enums;
using VetClinic.Web.Services.Interfaces;
using VetClinic.Web.ViewModels.Common;
using VetClinic.Web.ViewModels.Pets;

namespace VetClinic.Web.Controllers;

public class PetsController : Controller
{
    private readonly IPetService _petService;

    public PetsController(IPetService petService)
    {
        _petService = petService;
    }

    // GET /Pets?q=&species=&sort=&dir=&page=
    public async Task<IActionResult> Index(string? q, PetSpecies? species, string? sort, string? dir, int page = 1)
    {
        var query = new ListQueryParams { Q = q, Sort = sort, Dir = dir ?? "asc", Page = page };
        var result = await _petService.GetPagedAsync(query, species);

        return View(new ListViewModel<PetListViewModel>
        {
            Result = result,
            Q = q,
            Sort = sort,
            Dir = query.Dir,
            Filters = new Dictionary<string, string?> { ["species"] = species?.ToString() }
        });
    }

    // GET /Pets/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var pet = await _petService.GetDetailsAsync(id);
        if (pet is null) return NotFound();

        return View(pet);
    }
}
