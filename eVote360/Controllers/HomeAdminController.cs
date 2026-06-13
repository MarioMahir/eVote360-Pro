using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers;

[Authorize(Roles = "Administrador")]
public class HomeAdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}