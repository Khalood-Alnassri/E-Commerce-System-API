using E_Commerce_System;
using E_Commerce_System_API.DTOs;
using E_Commerce_System_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace E_Commerce_System_API.Controllers
{
    [ApiController]
    [Route("api/User")]
    public class UserController : ControllerBase
    {
        ApplicationDbContext context = new ApplicationDbContext();

        // function to regist user (add user)
        [HttpPost("Register")]
        public IActionResult Register(UserRegisterDTO registerDto)
        {
            User user = new User();

            user.UName = registerDto.UName;
            user.Email = registerDto.Email;
            user.Password = HashPassword(registerDto.Password);
            user.Phone = registerDto.Phone;
            user.CreatedAt = DateTime.Now;  
            user.Role = "User";
            user.IsActive = true;

            context.Users.Add(user);
            context.SaveChanges();
            return Ok("Register successfully, with ID: " + user.UId);
        }

        // function to login
        [HttpGet("Login")]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                return BadRequest("Password is required.");
            }

            var user = context.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());

            string hashedPassword = HashPassword(password);

            if (user == null || user.Password != hashedPassword)
            {
                return BadRequest("Invalid email or password.");
            }

            if (!user.IsActive)
            {
                return BadRequest("Your account is inactive.");
            }

            // save user id and user role in session
            HttpContext.Session.SetInt32("UserId", user.UId);
            HttpContext.Session.SetString("Role", user.Role);

            return Ok( new {
                Message = "Login successful",
                UserId = user.UId,
                Role = user.Role } );
        }

        // function to Get user details
        [HttpGet("UserInformation")]
        public IActionResult UserInformation()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return NotFound("You should login frist!");
            }

            var user = context.Users.Find(userId);
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

        [HttpGet("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string role = HttpContext.Session.GetString("Role");

            // check login
            if (userId == null)
            {
                return Unauthorized("Please login first.");
            }

            // check user exists
            var user = context.Users.Find(userId);

            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            // check admin role
            if (role != "Admin")
            {
                return Forbid("You do not have permission to perform this action.");
            }

            // get all users
            var users = context.Users.ToList();

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

        // function to Deactivate User
        [HttpPut("DeactivateUser")]
        public IActionResult DeactivateUser(int userID, bool status)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string role = HttpContext.Session.GetString("Role");

            // check login
            if (userId == null)
            {
                return Unauthorized("Please login first.");
            }

            // check user exists
            var user = context.Users.Find(userId);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            // check admin role
            if (role != "Admin")
            {
                return Forbid("You do not have permission to perform this action.");
            }

            var users = context.Users.FirstOrDefault(a => a.UId == userID);

            if (users != null)
            {
                users.IsActive = status;
                context.SaveChanges();
                return Ok("User status updated successfully.");
            }
            return NotFound("User not found.");
        }

        // function helper to hash password
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();

                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
