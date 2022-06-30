using Api.Data;
using Api.DTOs;
using Api.Entities;
using Api.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Api.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public AccountController(AppDbContext context, ITokenService tokenService, IMapper mapper) 
        {
            _context = context;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            // Handle exception, if username is already used in the database.
            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");

            // Map user to data transfer object.
            var user = _mapper.Map<AppUser>(registerDto);

            // Password encrypting algorithm.
            // Using statement to ensure that this class (HMACSHA512) gets disposed correctly every time it gets called.
            using var hmac = new HMACSHA512();

            user.UserName = registerDto.Username.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;

            // Add tracking to entity framework, then save.
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                KnownAs = user.KnownAs
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            // Get the user from the database.
            var user = await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDto.Username);

            // Handle username not found.
            if (user == null) return Unauthorized("Invalid username");

            // Using algorithm to decrypt password.
            using var hmac = new HMACSHA512(user.PasswordSalt);

            // Compare the computed passwords.
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                // Handle password does not match.
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Passowrd");
            }

            // Return the user.
            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs
            };
        }

        // Helper function to check if username exists in the database.
        private async Task<bool> UserExists(string username)
        {
            // Convert username to lower case before returning it.
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
