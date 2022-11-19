using Api.Services;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Api.Models;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {

        private readonly SubscriptionService _subscriptionService;
        private readonly UserService _userService;

        public SubscriptionController(SubscriptionService subscriptionService, UserService userService)
        {
            _subscriptionService = subscriptionService;
            _userService = userService;
        }


    }
}
