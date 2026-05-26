using Microsoft.AspNetCore.Mvc;
using VetClinic.Web.Models.Enums;
using VetClinic.Web.Services.Interfaces;
using VetClinic.Web.ViewModels.Appointments;
using VetClinic.Web.ViewModels.Common;

namespace VetClinic.Web.Controllers;

public class AppointmentsController : Controller
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    // GET /Appointments — haftalık takvim (varsayılan görünüm)
    public async Task<IActionResult> Index(DateTime? week)
    {
        var vm = await _appointmentService.GetWeekAsync(week ?? DateTime.Today);
        return View(vm);
    }

    // GET /Appointments/List?q=&from=&to=&status=&sort=&dir=&page=
    public async Task<IActionResult> List(string? q, DateTime? from, DateTime? to,
        AppointmentStatus? status, string? sort, string? dir, int page = 1)
    {
        var query = new ListQueryParams { Q = q, Sort = sort, Dir = dir ?? "desc", Page = page };
        var result = await _appointmentService.GetPagedAsync(query, from, to, status);

        return View(new ListViewModel<AppointmentListViewModel>
        {
            Result = result,
            Q = q,
            Sort = sort,
            Dir = query.Dir,
            Filters = new Dictionary<string, string?>
            {
                ["from"] = from?.ToString("yyyy-MM-dd"),
                ["to"] = to?.ToString("yyyy-MM-dd"),
                ["status"] = status?.ToString()
            }
        });
    }

    // GET /Appointments/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var vm = await _appointmentService.GetDetailsAsync(id);
        if (vm is null) return NotFound();

        return View(vm);
    }

    // GET /Appointments/Create?petId=
    public async Task<IActionResult> Create(int? petId)
    {
        var vm = await _appointmentService.BuildEmptyCreateAsync(petId);
        return View(vm);
    }

    // POST /Appointments/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentCreateEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await _appointmentService.PopulateOptionsAsync(vm);
            return View(vm);
        }

        var result = await _appointmentService.CreateAsync(vm);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Message);
            await _appointmentService.PopulateOptionsAsync(vm);
            return View(vm);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // GET /Appointments/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {
        var vm = await _appointmentService.GetForEditAsync(id);
        if (vm is null) return NotFound();

        return View(vm);
    }

    // POST /Appointments/Edit/{id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AppointmentCreateEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await _appointmentService.PopulateOptionsAsync(vm);
            return View(vm);
        }

        var result = await _appointmentService.UpdateAsync(vm);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", result.Message);
            await _appointmentService.PopulateOptionsAsync(vm);
            return View(vm);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // GET /Appointments/Cancel/{id}
    public async Task<IActionResult> Cancel(int id)
    {
        var vm = await _appointmentService.GetDetailsAsync(id);
        if (vm is null) return NotFound();

        return View(vm);
    }

    // POST /Appointments/Cancel/{id}
    [HttpPost, ActionName("Cancel"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelConfirmed(int id, string? reason)
    {
        var result = await _appointmentService.CancelAsync(id, reason);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // POST /Appointments/Confirm/{id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int id)
    {
        var result = await _appointmentService.ChangeStatusAsync(id, AppointmentStatus.Onaylandi);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // POST /Appointments/Complete/{id}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var result = await _appointmentService.ChangeStatusAsync(id, AppointmentStatus.Tamamlandi);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    // GET /Appointments/Delete/{id}
    public async Task<IActionResult> Delete(int id)
    {
        var vm = await _appointmentService.GetDetailsAsync(id);
        if (vm is null) return NotFound();

        return View(vm);
    }

    // POST /Appointments/Delete/{id}
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _appointmentService.DeleteAsync(id);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
