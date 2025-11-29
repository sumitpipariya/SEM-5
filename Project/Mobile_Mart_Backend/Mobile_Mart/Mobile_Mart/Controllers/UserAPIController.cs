using FluentValidation;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mobile_Mart.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace Mobile_Mart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserAPIController : ControllerBase
    {

        #region Constructor and Context
        private readonly MobileMartsContext _context;
        private readonly IValidator<User> _validator;
        private readonly IConfiguration _configuration;
        public UserAPIController(MobileMartsContext context ,IValidator<User> validator, IConfiguration configuration)
        {
            _context = context;
            _validator = validator;
            _configuration = configuration;
        }
        #endregion

        #region JWT TOKEN API
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var expiryMinutes = Convert.ToDouble(jwtSettings["TokenExpiryMinutes"]);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(expiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion

        #region LOGIN API
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] Login loginModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "FullName and Password are required." });

            var fullName = loginModel.FullName.Trim().ToLower();
            var password = loginModel.Password.Trim();

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.FullName.ToLower() == fullName &&
                    u.Password == password);

            if (user == null)
                return Unauthorized(new { message = "Invalid username or password" });

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    user.UserId,
                    user.FullName,
                    user.Email,
                    user.Role
                }
            });
        }

        #endregion

        #region Registration 
        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register([FromBody] User user)
        {
            if (user == null)
                return BadRequest(new { success = false, message = "Invalid user data." });

            try
            {
                user.Created = DateTime.UtcNow;
                user.Modified = DateTime.UtcNow;

                _context.Users.Add(user);
                _context.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "User registered successfully!",
                    token = "dummy-jwt-token" // generate real JWT in future
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Get All Users
        [Authorize(Roles = "Admin,User")]
        [HttpGet("User")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            return Ok(await _context.Users.ToListAsync());
        }
        #endregion

        #region Get Top 10 Users
        [HttpGet("Top10")]
        public async Task<ActionResult<IEnumerable<User>>> GetTop10Users()
        {
            return await _context.Users.Take(10).ToListAsync();
        }
        #endregion

        #region Get User By ID
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);
            return user == null ? NotFound() : Ok(user);
        }
        #endregion

        #region Insert User
        [HttpPost]
        public async Task<ActionResult<User>> InsertUser([FromBody]User user)
        {
            var validationResult = await _validator.ValidateAsync(user);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => new
                {
                    Property = e.PropertyName,
                    Error = e.ErrorMessage
                }));
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllUsers), new { id = user.UserId }, user);
        }
        #endregion

        #region Update User
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user)
        {
            if (id != user.UserId)
                return BadRequest("User ID mismatch");

            user.Modified = DateTime.Now;
            _context.Entry(user).State = EntityState.Modified;
             await _context.SaveChangesAsync();
           
            return NoContent();
        }
        #endregion

        #region DELETE User
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                    return NotFound("User not found.");

                var carts = _context.Carts.Where(c => c.UserId == id);
                _context.Carts.RemoveRange(carts);

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return NoContent(); 
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, "Cannot delete user due to related data.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion

        #region Search User By Name
        [HttpGet("Search/{name}")]
        public async Task<ActionResult<IEnumerable<User>>> SearchUserByName(string name)
        {
            var users = await _context.Users
                .Where(u => u.FullName.Contains(name))
                .ToListAsync();

            if (users == null || users.Count == 0)
                return NotFound("No users found with the given name.");

            return Ok(users);
        }
        #endregion

        #region Get Users for Dropdown
        [HttpGet("Dropdown")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserDropdown()
        {
            var users = await _context.Users
                .Select(u => new { u.UserId, u.FullName })
                .ToListAsync();

            return Ok(users);
        }
        #endregion

        #region Pagination
        [HttpGet("GetPagedUsers")]
        public async Task<IActionResult> GetPagedUsers(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var usersQuery = _context.Users.AsQueryable();

            var totalRecords = await usersQuery.CountAsync();
            var users = await usersQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Users = users
            };

            return Ok(result);
        }
        #endregion
    }
}
