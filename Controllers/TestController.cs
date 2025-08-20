using Microsoft.AspNetCore.Mvc;
using TarimHibe.Models;
using TarimHibe.Services;

namespace TarimHibe.Controllers
{
    public class TestController : BaseController
    {
        private readonly HibeDbContext _context;
        private readonly IzmirBBService _izmirBBService;

        public TestController(MenuService menuService, HibeDbContext context, IzmirBBService izmirBBService) : base(menuService)
        {
            _context = context;
            _izmirBBService = izmirBBService;
        }


        public IActionResult Index()
        {
            return View();
        }
    }
}
