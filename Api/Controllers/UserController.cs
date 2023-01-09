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
using DAL.Entities;
using Common.Extentions;
using Api.Models.User;
using Api.Models.Attaches;
using Api.Models.Subscriptions;
using Api.Exceptions;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService, LinkGeneratorService links)
        {
            _userService = userService;
            links.LinkAvatarGenerator = x => Url.ControllerAction<UserController>(nameof(UserController.GetUserAvatar), new
            {
                userId = x.Id
            }) ;
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
            var userId = GetCurrentUserId();
            return await _userService.GetUser(userId);
        }

        [HttpPost]
        [Authorize]
        public async Task ChangePassword(String newPassword) //Прекрасно понимаю, что метод немного корявый, написал на скорую руку эксперимента ради и чтобы чуть лучше разобраться...
        {
            var user = await GetCurrentUser();
            await _userService.ChangePassword(newPassword, user);
        }

        [HttpPost]
        [Authorize]
        public async Task AddAvatarToUser(MetadataModel model)
        {
            var user = await GetCurrentUser();
            var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), model.TempId.ToString()));
            if (!tempFi.Exists)
            {
                throw new Api.Exceptions.FileNotFoundException();
            }
            else
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Attaches", model.TempId.ToString());
                var destFi = new FileInfo(path);
                if (destFi.Directory != null && !destFi.Directory.Exists)
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
            var attach = await _userService.GetUserAvatar(userId, GetCurrentUserIdWithoutException());
            if (attach == null)
            {
                throw new AttachNotFoundException();
            }
            return File(System.IO.File.ReadAllBytes(attach.FilePath), attach.Mimetype);
        }

        [HttpGet]
        public async Task<FileResult> DownloadAvatar(Guid userId)
        {
            var attach = await _userService.GetUserAvatar(userId,GetCurrentUserId());

            HttpContext.Response.ContentType = attach.Mimetype;
            FileContentResult result = new FileContentResult(System.IO.File.ReadAllBytes(attach.FilePath), attach.Mimetype)
            {
                FileDownloadName = attach.Name
            };


            return result;
        }

        [Authorize]
        [HttpPost]
        public async Task SubscribeToUser(Guid targetId)
        {
            var userId = GetCurrentUserId();
            if(userId == targetId)
                throw new Exception("Can't subscribe to yourself");

            await _userService.Subscribe(userId, targetId);   
        }


        //[Authorize]
        //[HttpGet]
        // async Task<List<UserModel>> OfferUsersYouMightLike() => await _userService.GetUsersYouMightLike(GetCurrentUserId());

        [Authorize]
        [HttpGet]
        public async Task<List<UserModel>> GetSubscriptions(Guid userId) => await _userService.GetSubs(userId);

        [Authorize]
        [HttpGet]
        public async Task<List<UserModel>> GetSubscribers(Guid userId) => await _userService.GetSubbers(userId);


        [Authorize]
        [HttpGet]
        public async Task<bool> CheckSubscription(Guid targetId) => await _userService.CheckSubscription(GetCurrentUserId(), targetId);


        [Authorize]
        [HttpGet]
        private Guid GetCurrentUserId()
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            if (Guid.TryParse(userIdString, out var userId))
                return userId;
            else
                throw new Exception("You are not authorized");
        }

        [Authorize]
        [HttpGet]
        private Guid GetCurrentUserIdWithoutException() //Нужно для того, когда хотелось бы найти текущего пользователя, но авторизация не важна.
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            Guid.TryParse(userIdString, out var userId);
            return userId;

        }

        [Authorize]
        [HttpPost]
        public async Task ChangePrivacySettings(ChangePrivacySettingsModel model) => await _userService.ChangePrivacySettings(GetCurrentUserId(), model);

        [Authorize]
        [HttpGet]
        public async Task<ChangePrivacySettingsModel> GetPrivacySettings() => await _userService.GetPrivacySettings(GetCurrentUserId());

        [Authorize]
        [HttpPost]
        public async Task UnsubscribeFromUser(Guid targetId) => await _userService.Unsubscribe(GetCurrentUserId(), targetId);

        [HttpGet]
        public async Task<UserModel> GetUserById(Guid userId) => await _userService.GetUser(userId);
    }

}

