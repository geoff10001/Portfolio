using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Portfolio.Api.Repository;
using Portfolio.Api.Models;
//using Azure.Identity;

namespace Portfolio.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PowerBIDataController : ControllerBase
    {
        private readonly IPowerBIDataRepository _powerbidataRepository;

        public PowerBIDataController(IPowerBIDataRepository powerbidataRepository)
        {
            _powerbidataRepository = powerbidataRepository;
        }

        [HttpPost(nameof(CreateUserURLAudit))]
        public async Task<IActionResult> CreateUserURLAudit([FromBody] URLAuditModel content)
        {
            try
            {
                if (content == null)
                {
                    return BadRequest("Invalid data received");
                }

                string userName = content.UA_User;
                string URL = content.UA_URL;

                // Call the repository method once
                var result = await _powerbidataRepository.CreateUserURLAudit(userName, URL);

                if (result == null)
                {
                    return NotFound();
                }

                // Return the result in the OK response
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
