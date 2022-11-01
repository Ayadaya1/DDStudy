using Api.Models;
using Api.Services;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Common;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task CreateUser(CreateUserModel model)
        {
            await _userService.CreateUser(model);
        }
        

        [HttpGet]
        [Authorize]
        public async Task<List<UserModel>> GetUsers() => await _userService.GetUsers();
        [HttpGet]
        [Authorize]
        public async Task<UserModel> GetCurrentUser()
        {
            var userIdString = User.Claims.FirstOrDefault(x=>x.Type == "id")?.Value;
            if (Guid.TryParse(userIdString, out var userId))
            {
                return await _userService.GetUser(userId);
            }
            else
                throw new Exception("You are not authorized");
        }

        [HttpPost]
        [Authorize]
        public async Task ChangePassword(ChangePasswordModel model) //Прекрасно понимаю, что метод немного корявый, написал на скорую руку эксперимента ради и чтобы чуть лучше разобраться...
        {
            var user = await GetCurrentUser();
            await _userService.ChangePassword(model.NewPassword, user);
        }
    }
}
