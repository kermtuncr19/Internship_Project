using System.Formats.Asn1;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Entities.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Contracts;
using StoreApp.Areas.Admin.Models;

namespace StoreApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IServiceManager _manager;

        public UserController(IServiceManager manager)
        {
            _manager = manager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _manager.AuthService.GetAllUsers().ToList();

            // username -> roles
            var roleMap = new Dictionary<string, IList<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var u in users)
            {
                if (string.IsNullOrWhiteSpace(u.UserName)) continue;
                roleMap[u.UserName] = await _manager.AuthService.GetUserRolesAsync(u.UserName);
            }

            ViewBag.UserRoles = roleMap;

            return View(users); // 👈 model aynı: IEnumerable<IdentityUser>
        }


        public IActionResult Create()
        {

            return View(new UserDtoForCreation()
            {
                Roles = new HashSet<string>(_manager
                .AuthService
                .Roles
                .Select(r => r.Name!)
                .ToList())
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] UserDtoForCreation userDto)
        {
            var result = await _manager.AuthService.CreateUser(userDto);
            return result.Succeeded
                ? RedirectToAction("Index")
                : View();
        }

        public async Task<IActionResult> Update([FromRoute(Name = "id")] string id)
        {
            var user = await _manager.AuthService.GetOneUserForUpdate(id);
            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([FromForm] UserDtoForUpdate userDto)
        {
            if (ModelState.IsValid)
            {
                await _manager.AuthService.Update(userDto);
                return RedirectToAction("Index");
            }
            return View();
        }

        public async Task<IActionResult> ResetPassword([FromRoute(Name = "id")] string id)
        {
            return View(new ResetPasswordDto()
            {
                UserName = id
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordDto model)
        {
            var result = await _manager.AuthService.ResetPassword(model);
            return result.Succeeded
                ? RedirectToAction("Index")
                : View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOneUser([FromForm] UserDto userDto)
        {
            var result = await _manager
                 .AuthService
                 .DeleteOneUser(userDto.UserName!);

            return result.Succeeded
                ? RedirectToAction("Index")
                : View();
        }

    }
}