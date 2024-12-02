using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Portfolio.Api.Repository;

namespace Portfolio.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyIdentitysController : ControllerBase
    {
        private readonly IMyIdentityRepository _myidentityRepository;

        public MyIdentitysController(IMyIdentityRepository myidentityRepository)
        {
            _myidentityRepository = myidentityRepository;
        }

        [HttpGet(nameof(GetAllRoles))]
        public async Task<IActionResult> GetAllRoles()
        {

            var result = await _myidentityRepository.GetRoles();
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);

            //try
            //{
            //    var result = await _myidentityRepository.GetRoles();
            //    if (result == null)
            //    {
            //        return NotFound();
            //    }
            //    return Ok(result);
            //}
            //catch (Exception)
            //{
            //    return StatusCode(StatusCodes.Status500InternalServerError,
            //        "Error retrieving data from the database");
            //}
        }

        [HttpGet(nameof(GetAllUsers))]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var result = await _myidentityRepository.GetUsers();
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

        [HttpGet(nameof(GetAllUsersInRoles))]
        public async Task<IActionResult> GetAllUsersInRoles()
        {
            try
            {
                var result = await _myidentityRepository.GetUsersInRoles();
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


        //not implemented as done via identity sevices but leaving as code example
        [HttpPost(nameof(CreateRole))]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            try
            {
                await _myidentityRepository.CreateRole(roleName);

                var result = await _myidentityRepository.CreateRole(roleName);
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

          [HttpGet(nameof(GetUsersNotInRole))]
        //[HttpGet("{projID}")]
        public async Task<IActionResult> GetUsersNotInRole([FromQuery] string roleID)
        {

            try
            {
                //string roleID = xroleID.ToString();
                var result = await _myidentityRepository.GetUsersNotInRole(roleID);
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

        [HttpGet(nameof(GetRolesNotInUser))]
        public async Task<IActionResult> GetRolesNotInUser([FromQuery] string userName)
        {
            try
            {
                var result = await _myidentityRepository.GetRolesNotInUser(userName);
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

        [HttpGet(nameof(GetRolesInUser))]
        public async Task<IActionResult> GetRolesInUser([FromQuery] string userName)
        {
            try
            {
                var result = await _myidentityRepository.GetRolesInUser(userName);
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

        //[HttpGet(nameof(GetRolesNotInUser))]
        ////[HttpGet("{projID}")]
        //public async Task<IActionResult> GetRolesNotInUser(object xuserName)
        //{

        //    try
        //    {
        //        string userName = xuserName.ToString();
        //        var result = await _myidentityRepository.GetRolesNotInUser(userName);
        //        if (result == null)
        //        {
        //            return NotFound();
        //        }
        //        return Ok(result);
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError,
        //        "Error retrieving data from the database");
        //    }
        //}
    }
}