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
}
