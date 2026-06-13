using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eVote360.Controllers;

[Authorize(Roles = "Dirigente político")]
public class HomeDirigenteController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}