using Api.DTOs;
using Api.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        // Gives the controller access to our db context.
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await _userRepository.GetMembersAsync();

            return Ok(users);
        }

        // GET: api/users/:username
        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _userRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            // Store the username (coming from token name property).
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Use the repository to fetch the username.
            var user = await _userRepository.GetUserByUsernameAsync(username);
            // Map user to dto.
            _mapper.Map(memberUpdateDto, user);
            // Use entity framework to update database
            _userRepository.Update(user);
            // Save if successful.
            if (await _userRepository.SaveAllAsync()) return NoContent();
            // Handle bad request.
            return BadRequest("Failed to update the user.");
        }
    }
}
