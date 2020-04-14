using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jwt.Model;
using Jwt.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jwt.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]UserViewModel userParam)
        {
            var user = _userService.Authenticate(userParam.Username, userParam.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(user);
        }

        [Authorize(Roles = Role.Admin)]
        [HttpGet]
        public IActionResult GetAll()
        {
            var currentUserId = int.Parse(User.Identity.Name);
            if (!User.IsInRole(Role.Admin))
            {
                return Forbid();
            }
            var users = _userService.GetAll();
            return Ok(users);
        }

        [Authorize(Roles = Role.User)]
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _userService.GetById(id);

            if (user == null)
            {
                return NotFound();
            }

            // only allow admins to access other user records
            var currentUserId = int.Parse(User.Identity.Name);
            if (id != currentUserId && !User.IsInRole(Role.User))
            {
                return Forbid();
            }

            return Ok(user);
        }
    }
}