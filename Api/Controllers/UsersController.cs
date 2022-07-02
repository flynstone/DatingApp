using Api.DTOs;
using Api.Entities;
using Api.Extensions;
using Api.Helpers;
using Api.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        // Gives the controller access to our db context.
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _photoService = photoService;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
            userParams.CurrentUsername = user.UserName;

            if (string.IsNullOrEmpty(userParams.Gender))
                userParams.Gender = user.Gender == "male" ? "female" : "male";

            var users = await _userRepository.GetMembersAsync(userParams);

            // Add pagination to http response header.
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }

        // GET: api/users/:username
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _userRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            // Use the repository to fetch the username.
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());
            // Map user to dto.
            _mapper.Map(memberUpdateDto, user);
            // Use entity framework to update database
            _userRepository.Update(user);
            // Save if successful.
            if (await _userRepository.SaveAllAsync()) return NoContent();
            // Handle bad request.
            return BadRequest("Failed to update the user.");
        }

        // Allow user to upload photo (to cloudinary)
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            // Get user (from claims... to make sure he is validated)
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            // Store photo returned from service.
            var result = await _photoService.AddPhotoAsync(file);

            // Handle error if there is no photo..
            if (result.Error != null) return BadRequest(result.Error.Message);

            // Cloudinary configuration.
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count == 0)
            {
                // If the app gets here, we know its the first image that the user is uploading.
                // Set it as main.
                photo.IsMain = true;
            }

            // Add to entity framework.
            user.Photos.Add(photo);

            // Save and map entity to data transfer object.
            if (await _userRepository.SaveAllAsync())
            {
                // return _mapper.Map<PhotoDto>(photo);

                // Proper way to return a 201 response.
                return CreatedAtRoute("GetUser", new {Username = user.UserName}, _mapper.Map<PhotoDto>(photo));
            }
                

            // Handle error.
            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            // Get user. (from token)
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            // Set the photo to main.
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            // Handle exception if photo is already the main one.
            if (photo.IsMain) return BadRequest("This is already your main photo");

            // Get current main photo
            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            // Switch main photo.
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;

            // Save changes to database.
            if (await _userRepository.SaveAllAsync()) return NoContent();

            // Handle exception.
            return BadRequest("Failed to set the main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            // Get user from token
            var user = await _userRepository.GetUserByUsernameAsync(User.GetUsername());

            // Get photo by id.
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            // Handle not found.
            if (photo == null) return NotFound();

            // Send bad request if user attempts to delete his main photo.
            if (photo.IsMain) return BadRequest("You cannot delete your main photo");

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            // Remove from repository.
            user.Photos.Remove(photo);

            // Save changes to database
            if (await _userRepository.SaveAllAsync()) return Ok();

            // Handle exception.
            return BadRequest("Failed to delete the photo");
        }
    }
}
