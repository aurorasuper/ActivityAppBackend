#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ActivityJournal.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ActivityJournal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityUsersController : ControllerBase
    {
        private readonly ActivityDBContext _context;
        private readonly IConfiguration _configuration;

        public ActivityUsersController(ActivityDBContext context, IConfiguration config)
        {
            _context = context;
            _configuration = config;
        }

        // GET: api/ActivityUsers/5
        //Login
        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody]UserLogin userLogin)
        {
            /*
             * //use email to verify that user exists
            var user = _context.ActivityUsers.FromSqlRaw("SELECT * FROM dbo.Activity_Users WHERE Usr_Email = {0}", userLogin.Email);
            
            if (user == null)
            {
                return NotFound("User not found");
            }*/
            string loginerror = "";
            //compare given passwordHash with stored passwordhash
            ActivityUser activityUser = Authenticate(userLogin, out loginerror);
            if (activityUser == null)
            {
                Response ErrorRes = new Response();
                ErrorRes.Status = "Failure";
                ErrorRes.Message = loginerror;
                return BadRequest(loginerror);
            }
            AuthenticateResponse res = new AuthenticateResponse(activityUser, generate(activityUser));
            // validation success return json web token
            return Ok(JsonSerializer.Serialize(res));
        }

        // PUT: api/ActivityUsers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //Update
        [HttpPut("{id}")]
        public async Task<IActionResult> PutActivityUser(int id, ActivityUser activityUser)
        {
            if (id != activityUser.UsrId)
            {
                return BadRequest();
            }

            _context.Entry(activityUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ActivityUserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ActivityUsers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // Register
        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<ActionResult<ActivityUser>> Register(UserRegister userLogin)
        {

            var user = _context.ActivityUsers.FirstOrDefault(o => o.UsrEmail.ToLower() == userLogin.Email.ToLower());
            if (user != null)
            {
                return BadRequest("Email adress is already registered.");
            }

            if (userLogin.Password != userLogin.ConfirmPassword)
            {
                return BadRequest("Passwords do not match.");
            }

            if (userLogin.Password.Length < 8)
            {
                return BadRequest("Password must be at least 8 characters long.");
            }

            ActivityUser activityUser = new ActivityUser();
            activityUser.UsrFirstName = userLogin.Firstname;
            activityUser.UsrLastName = userLogin.Lastname;
            activityUser.UsrEmail = userLogin.Email.ToLower();

            userLogin.createHash(userLogin.Password, out byte[] passwordHash, out byte[] passwordSalt);
            activityUser.UsrPassword = passwordHash;
            activityUser.UsrSalt = passwordSalt;

            _context.ActivityUsers.Add(activityUser);
            await _context.SaveChangesAsync();

            return Ok(new Response{ Status = "Success", Message = "User created successfully!" });
        }

        // DELETE: api/ActivityUsers/
        [HttpDelete]
        public async Task<IActionResult> DeleteActivityUser(int id)
        {
            var activityUser = await _context.ActivityUsers.FindAsync(id);
            if (activityUser == null)
            {
                return NotFound();
            }

            _context.ActivityUsers.Remove(activityUser);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ActivityUserExists(int id)
        {
            return _context.ActivityUsers.Any(e => e.UsrId == id);
        }


        // Authenticate user
        //Generate A JWT from user, as per https://www.youtube.com/watch?v=kM1fPt1BcLc&t=861s
        private string generate(ActivityUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            string role = "";
            if (user.UsrIsAdmin)
            {
                role = "admin";
            }
            else { role = "user"; }
            string errormsg;
            var claims = new[]
            {
                new Claim("Id", user.GetId(user.UsrEmail, out errormsg).ToString()),
                new Claim(ClaimTypes.Email, user.UsrEmail),
                new Claim(ClaimTypes.GivenName, user.UsrFirstName),
                new Claim(ClaimTypes.Surname, user.UsrLastName),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);


        }

        private ActivityUser Authenticate(UserLogin userLogin, out string errormessage)
        {
            errormessage = "";

            // get user by email
            ActivityUser currentUser = _context.ActivityUsers.FirstOrDefault(o => o.UsrEmail.ToLower() == userLogin.Email.ToLower());
            if(currentUser == null)
            {
                errormessage = "User not found";
                return null;
            }

            //compare given passwordHash with stored passwordhash
            if (!userLogin.VerifyHash(userLogin.Password, currentUser.UsrPassword, currentUser.UsrSalt))
            {
                errormessage = "Wrong password";
                return null;
            }
            
            return currentUser;
        }
    }
}
