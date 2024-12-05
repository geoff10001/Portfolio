using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Portfolio.Api.Repository;

namespace Portfolio.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        

        [HttpGet(nameof(GetAll))]
        public async Task<IActionResult> GetAll()
        {
            //var users = await _userRepository.GetAllUsersAsync();
            //return Ok(users);
            try
            {
                var result = await _userRepository.GetAllUsersAsync();
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error retrieving data from the database");
            }
        }
    }

}
