namespace UserAccounts.Api.Controllers
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using UserAccounts.Service.Models;
    using UserAccounts.Service.Services;


    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }
        // GET: UsersController

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var users = await _userService.GetUsersAsync();
            return Ok(users);
        }
        
        // GET: Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult> Details(int id)
        {
            var user = await _userService.GetUserAsync(id);

            if(user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }



        [HttpPost]
        public async Task<ActionResult> Create(UserModel userModel)
        {
            try
            {
                var result = await _userService.InsertUserAsync(userModel);
                if (result.Success)
                {
                    _logger.LogInformation($"User {userModel.FirstName} is created");
                    return Ok();
                }

                if(result.Errors.FirstOrDefault().Any())
                {
                    return BadRequest(result.Errors.FirstOrDefault());
                }

                return BadRequest();
            }
            catch
            {

                return BadRequest();
            }
        }


        // PUT: Users/5
        [HttpPut]
        public async Task<ActionResult> Edit(int id, UserModel requestModel)
        {
            var userModel = await _userService.GetUserAsync(id);

            if (userModel == null)
            {
                return NotFound();
            }

            try
            {
                userModel.Id = id;
                var result = await _userService.UpdateUserAsync(userModel);
                if (result.Success)
                {
                    return Ok();
                }

                if (result.Errors.FirstOrDefault().Any())
                {
                    return BadRequest(result.Errors.FirstOrDefault());
                }

                return BadRequest();
            }
            catch
            {

                return BadRequest();
            }
        }

        // POST: Users/Delete/5
        [HttpDelete]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                await _userService.DeleteAsync(id);
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
