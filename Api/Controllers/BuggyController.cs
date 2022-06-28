using Api.Data;
using Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class BuggyController : BaseApiController
    {
        private readonly AppDbContext _context;
        public BuggyController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret()
        {
            return "secret user";
        }

        [HttpGet("not-found")]
        public ActionResult<AppUser> GetNotFound()
        {
            // Try to get something you know does not exist.
            var thing = _context.Users.Find(-1);

            if (thing == null) return NotFound();

            return Ok(thing);
        }

        [HttpGet("server-error")]
        public ActionResult<string> GetServerError()
        {
            // Try to get something you know does not exist.
            var thing = _context.Users.Find(-1);

            // This will generate a server error.
            var thingToReturn = thing.ToString();

            return thingToReturn;
        }

        [HttpGet("bad-request")]
        public ActionResult<string> GetBadRequest()
        {
            return BadRequest("This was not a good request");
        }
    }
}
