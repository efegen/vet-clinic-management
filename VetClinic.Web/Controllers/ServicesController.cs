using Microsoft.AspNetCore.Mvc;
using VetClinic.Web.Services.Interfaces;
using VetClinic.Web.ViewModels.Services;

namespace VetClinic.Web.Controllers;

public class ServicesController : Controller
{
    private readonly IServiceCatalogService _serviceCatalog;

    public ServicesController(IServiceCatalogService serviceCatalog)
    {
        _serviceCatalog = serviceCatalog;
    }

    // GET /Services?q=
    public async Task<IActionResult> Index(string? q)
    {
        var all = (await _serviceCatalog.GetAllAsync(q)).ToList();

        return View(new ServiceIndexViewModel
        {
            Q = q,
            Active = all.Where(s => s.IsActive).ToList(),
            Passive = all.Where(s => !s.IsActive).ToList()
        });
    }

    // GET /Services/Create
    public IActionResult Create() => View(new ServiceCreateEditViewModel());

    // POST /Services/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _serviceCatalog.CreateAsync(vm);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Message);
            return View(vm);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // GET /Services/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _serviceCatalog.GetForEditAsync(id);
        if (vm is null) return NotFound();

        return View(vm);
    }

    // POST /Services/Edit/{id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ServiceCreateEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _serviceCatalog.UpdateAsync(vm);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Message);
            return View(vm);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // POST /Services/Deactivate/{id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _serviceCatalog.DeactivateAsync(id);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // POST /Services/Activate/{id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var result = await _serviceCatalog.ActivateAsync(id);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // GET /Services/Delete/{id} — sadece randevusu olmayan hizmetler için onay
    public async Task<IActionResult> Delete(int id)
    {
        var vm = await _serviceCatalog.GetForEditAsync(id);
        if (vm is null) return NotFound();

        return View(vm);
    }

    // POST /Services/Delete/{id}
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _serviceCatalog.DeleteAsync(id);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
