using Microsoft.AspNetCore.Mvc;
using VetClinic.Web.Services.Interfaces;
using VetClinic.Web.ViewModels.Common;
using VetClinic.Web.ViewModels.Owners;

namespace VetClinic.Web.Controllers;

public class OwnersController : Controller
{
    private readonly IOwnerService _ownerService;

    public OwnersController(IOwnerService ownerService)
    {
        _ownerService = ownerService;
    }

    // GET /Owners?q=&sort=&dir=&page=
    public async Task<IActionResult> Index(string? q, string? sort, string? dir, int page = 1)
    {
        var query = new ListQueryParams { Q = q, Sort = sort, Dir = dir ?? "asc", Page = page };
        var result = await _ownerService.GetPagedAsync(query);

        return View(new ListViewModel<OwnerListViewModel>
        {
            Result = result,
            Q = q,
            Sort = sort,
            Dir = query.Dir
        });
    }

    // GET /Owners/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var owner = await _ownerService.GetDetailsAsync(id);
        if (owner is null) return NotFound();

        return View(owner);
    }

    // GET /Owners/Create
    public IActionResult Create() => View(new OwnerCreateEditViewModel());

    // POST /Owners/Create
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

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // GET /Owners/Delete/{id} — onay sayfası (cascade uyarısı)
    public async Task<IActionResult> Delete(int id)
    {
        var owner = await _ownerService.GetDetailsAsync(id);
        if (owner is null) return NotFound();

        return View(owner);
    }

    // POST /Owners/Delete/{id}
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _ownerService.DeleteAsync(id);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // GET /Owners/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _ownerService.GetForEditAsync(id);
        if (vm is null) return NotFound();

        return View(vm);
    }

    // POST /Owners/Edit/{id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(OwnerCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _ownerService.UpdateAsync(vm);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Message);
            return View(vm);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
