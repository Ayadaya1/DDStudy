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
            if (await _userService.CheckUserExists(model.Email))
                throw new Exception("User already exists");
            await _userService.CreateUser(model);

        }


        [HttpGet]
        [Authorize]
        public async Task<List<UserModel>> GetUsers() => await _userService.GetUsers();
        [HttpGet]
        [Authorize]
        public async Task<UserModel> GetCurrentUser()
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
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

        [HttpPost]
        [Authorize]
        public async Task AddAvatarToUser(MetadataModel model)
        {
            var user = await GetCurrentUser();
            var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), model.TempId.ToString()));
            if (!tempFi.Exists)
            {
                throw new Exception("File not found!");
            }
            else
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Attaches", model.TempId.ToString());
                var destFi = new FileInfo(path);
                if (destFi.Directory != null&& !destFi.Directory.Exists)
                    destFi.Directory.Create();

                if (path != null)
                {
                    System.IO.File.Copy(tempFi.FullName, path, true);

                    await _userService.AddAvatarToUser(user.Id, model, path);
                }

            }
        }

        [HttpGet]
        public async Task<FileResult> GetUserAvatar(Guid userId)
        {
            var attach = await _userService.GetUserAvatar(userId);
            if(attach== null)
            {
                throw new Exception("No avatar");
            }    
            return File(System.IO.File.ReadAllBytes(attach.FilePath), attach.Mimetype);
        }

        [HttpGet]
        public async Task<FileResult> DownloadAvatar(Guid userId)
        {
            var attach = await _userService.GetUserAvatar(userId);

            HttpContext.Response.ContentType = attach.Mimetype;
            FileContentResult result = new FileContentResult(System.IO.File.ReadAllBytes(attach.FilePath), attach.Mimetype)
            {
                FileDownloadName = attach.Name
            };
            

            return result;
        }

        [Authorize]
        [HttpPost]
        public async Task SubscribeToUser(SubscriptionModel model)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            if (Guid.TryParse(userIdString, out var userId))
            {
                if (model.TargetId != userId)
                    await _userService.Subscribe(userId, model.TargetId);
                else
                    throw new Exception("Can't subscribe to yourself");
            }
            else
                throw new Exception("You are not authorized");
        }
    }

}

