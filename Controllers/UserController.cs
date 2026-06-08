using E_Commerce_System;
using E_Commerce_System_API.DTOs;
using E_Commerce_System_API.Models;
using E_Commerce_System_API.Serviece;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Security;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace E_Commerce_System_API.Controllers
{
    [ApiController]
    [Route("api/User")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly LoggingService _loggingService;

        private readonly Serviece.JwtService _jwtServiece;
        public UserController(ApplicationDbContext context, Serviece.JwtService jwtServiece, LoggingService logging)
        {
            _context = context;
            _jwtServiece = jwtServiece;
            _loggingService = logging;
        }

        // function to regist user (add user)
        [HttpPost("Register")]
        public IActionResult Register(UserRegisterDTO registerDto)
        {
            // check if email already exists
            if (_context.Users.Any(u => u.Email == registerDto.Email))
            {
                return BadRequest("Email already in use.");
            }

            User user = new User
            {
                UName = registerDto.UName,
                Email = registerDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Phone = registerDto.Phone,
                CreatedAt = DateTime.Now,
                Role = "User",
                IsActive = true
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            _loggingService.LogInfo("New user registered: " + user.Email);

            return Ok("Register successfully, with ID: " + user.UId);
        }

        // function to login
        [HttpPost("Login")]
        public IActionResult Login(UserLoginDTO loginDTO)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == loginDTO.Email.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.Password))
            {
                _loggingService.LogWarning("Failed login attempt: " + loginDTO.Email);
                return BadRequest("Invalid email or password.");
            }

            if (!user.IsActive)
            {
                _loggingService.LogWarning("Inactive user tried login: " + user.Email);
                return BadRequest("Your account is inactive.");
            }

            var token = _jwtServiece.GenerateToken(user);

            _loggingService.LogInfo("User logged in: " + user.Email);

            return Ok( new {
                Message = "Login successful",
                Token = token } );
        }

        // function to Get user details
        [Authorize]
        [HttpGet("UserInformation")]
        public IActionResult UserInformation()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized();
            }

            int id = int.Parse(userId);

            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var GetUserInfoDTO = new GetUserInfoDTO
            {
                UId = user.UId,
                UName = user.UName,
                Email = user.Email,
                Phone = user.Phone,
                CreatedAt = user.CreatedAt
            };

            return Ok(GetUserInfoDTO);
        }

        // function to Get all users (Admin only)
        [Authorize (Roles = "Admin")]
        [HttpGet("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // check login
            if (userId == null)
            {
                return Unauthorized("Please login first.");
            }
            
            if (role != "Admin")
                {
                    return Forbid("You do not have permission to perform this action.");
            }
            // get all users
            var users = _context.Users.ToList();

            // convert to DTO
            var usersDTO = new List<GetUserInfoDTO>();

            // loop to convert each user to DTO
            foreach (var u in users)
            {
                usersDTO.Add(new GetUserInfoDTO
                {
                    UId = u.UId,
                    UName = u.UName,
                    Email = u.Email,
                    Phone = u.Phone,
                    CreatedAt = u.CreatedAt
                });
            }

            return Ok(usersDTO);

        }

        // function to Deactivate User (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPut("DeactivateUser")]
        public IActionResult DeactivateUser(int userID, bool status)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Forbid();
            }

            var users = _context.Users.FirstOrDefault(a => a.UId == userID);

            if (users != null)
            {
                users.IsActive = status;
                _context.SaveChanges();
                _loggingService.LogInfo("Admin changed user " + users.UId + " status to " +status);
                return Ok("User status updated successfully.");
            }
            return NotFound("User not found.");
        }

    }
}
