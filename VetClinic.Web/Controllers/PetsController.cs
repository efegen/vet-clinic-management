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

    // GET /Pets/Create?ownerId=
    public async Task<IActionResult> Create(int? ownerId)
    {
        var vm = await _petService.BuildEmptyCreateAsync(ownerId);
        return View(vm);
    }

    // POST /Pets/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PetCreateEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await _petService.PopulateOwnerOptionsAsync(vm);
            return View(vm);
        }

        var result = await _petService.CreateAsync(vm);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Message);
            await _petService.PopulateOwnerOptionsAsync(vm);
            return View(vm);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // GET /Pets/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _petService.GetForEditAsync(id);
        if (vm is null) return NotFound();

        return View(vm);
    }

    // POST /Pets/Edit/{id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PetCreateEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await _petService.PopulateOwnerOptionsAsync(vm);
            return View(vm);
        }

        var result = await _petService.UpdateAsync(vm);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Message);
            await _petService.PopulateOwnerOptionsAsync(vm);
            return View(vm);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // GET /Pets/Delete/{id} — onay sayfası
    public async Task<IActionResult> Delete(int id)
    {
        var pet = await _petService.GetDetailsAsync(id);
        if (pet is null) return NotFound();

        return View(pet);
    }

    // POST /Pets/Delete/{id}
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _petService.DeleteAsync(id);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
