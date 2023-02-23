using ActivityJournal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ActivityJournal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActivityLogController : ControllerBase
    {
        private readonly ActivityDBContext _context;
        private readonly IConfiguration _configuration;

        public ActivityLogController(ActivityDBContext context, IConfiguration config)
        {
            _context = context;
            _configuration = config;
        }

        // GET: api/ActivityLogs
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ActivityLog>>> GetActivityLogs()
        {
            // Get user and user id, throw failure response if user not found for debugging purpose. An authorized user with Jwt Token should exist in the database. 
            var userFromJwt = UserFromJwt();
            ActivityUser user = GetEntityUser(userFromJwt.UsrEmail);
            if (user == null)
            {
                return BadRequest(new Response { Status = "Failure", Message = "User not found." });
            }

            // return list of logs in format according to UpdateLogModel, with the created property
            var response = await _context.ActivityLogs.Where(o=>o.UserNavigation == user).Select(o => new { 
                    o.LogId, ActivityType = o.ActivityTypeNavigation.ActivityType, o.Created, o.Ended,o.Difficulty, o.Feeling}).ToListAsync();

            return Ok(response);
        }

        // GET: api/ActivityLog/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ActivityLog>> GetLog(int id)
        {

            if (!ActivityLogExists(id))
            {
                return NotFound();
            }
            var activityLog = await _context.ActivityLogs.FirstOrDefaultAsync(log => log.LogId == id);
                                                         

            // return list of logs in format according to UpdateLogModel, with the created property
            var response = await _context.ActivityLogs.Where(o => o.LogId == id).Select(o => new {
                o.LogId,
                ActivityType = o.ActivityTypeNavigation.ActivityType,
                o.Created,
                o.Ended,
                o.Difficulty,
                o.Feeling
            }).FirstOrDefaultAsync();


            return Ok(response);
        }

        //POST: a new log
        [HttpPost("NewLog")]
        [Authorize]
        public async Task<IActionResult> CreateLog(LogModel model)
        {
            
            // Get user and user id, throw failure response if user not found for debugging purpose. An authorized user with Jwt Token should exist in the database. 
            var userFromJwt = UserFromJwt();
            ActivityUser user = GetEntityUser(userFromJwt.UsrEmail);
            if (user == null) {
                return BadRequest(new Response { Status = "Failure", Message = "User not found."});
             }

            
            ActivityActivity newActivity = new ActivityActivity();
            //Check if activity with name does not exist in database, if not create new activity with said name
            if (!ActivityTypeExists(model.ActivityType)) 
            {   
                newActivity.ActivityType = model.ActivityType.ToLower();
                _context.Add(newActivity);
                await _context.SaveChangesAsync();
                
            }
            else
            {
                newActivity = GetActivity(model.ActivityType);
            }
            // get activty id 
         

            // add all values to database
            ActivityLog activityLog = new ActivityLog();
            if(model.Created != null)
            {
                activityLog.Created = DateTime.Parse(model.Created);
            }
            else
            {
                activityLog.Created = DateTime.Now;
            }
            
            activityLog.ActivityType = newActivity.Id;
            activityLog.User = user.UsrId;

            if(model.Difficulty != null)
            {
                activityLog.Difficulty = model.Difficulty;
            }
            if(model.Feeling != null)
            {
                activityLog.Feeling = model.Feeling;
            }
            if(model.Ended != null) // validate datetime at client side
            {   
                activityLog.Ended = DateTime.Parse(model.Ended); ;
            }

            //add navigations and add log to db
            activityLog.ActivityTypeNavigation = newActivity;
            activityLog.UserNavigation = user;
            // add created log to navigation entities list of logs
            newActivity.ActivityLogs.Add(activityLog);
            user.ActivityLogs.Add(activityLog);

            _context.Add(activityLog);



            await _context.SaveChangesAsync();
            return Ok(activityLog);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, LogModel model)
        {
            //check id and model id is the same
            if (id != model.LogId)
            {
                return BadRequest();
            }
            // get log from database
            ActivityLog log = GetActivityLog(model.LogId);
            if (log == null) 
            {
                return BadRequest(new Response { Status = "Failure", Message = "Log not found" });
            };

            // check if model values have changed, if yes: change values and save changes

            //Check if activity with name does not exist in database, if not create new activity with said name
            if (!ActivityTypeExists(model.ActivityType))
            {
                AddActivity(model.ActivityType);
            }

            //get activity ID from activity in the model
            ActivityActivity modelActivity = GetActivity(model.ActivityType);

            if(log.ActivityType != modelActivity.Id)
            {
                log.ActivityType = modelActivity.Id;
                log.ActivityTypeNavigation = modelActivity;
            }


            DateTime created = DateTime.Parse(model.Created);
            if (log.Created != created)
            {
                log.Created = created;
            }

            if(model.Ended != null)
            {
                DateTime ended = DateTime.Parse(model.Ended);
                if (log.Ended != ended)
                {
                    log.Ended = ended;
                }

            }


            if(log.Difficulty != model.Difficulty)
            {
                log.Difficulty = model.Difficulty;
            }

            if(log.Feeling != model.Feeling)
            {
                log.Feeling = model.Feeling;
            }

            _context.Entry(log).State= EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ActivityLogExists(log.LogId))
                {
                    return NotFound("Log not found");
                }
                else
                {
                    throw;
                }
            }
           
            return Ok(log);
        }

        // DELETE: api/ActivityLogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteActivityLog(int id)
        {
            var activityLog = await _context.ActivityLogs.FindAsync(id);
            if (activityLog == null)
            {
                return NotFound();
            }

            _context.ActivityLogs.Remove(activityLog);
            await _context.SaveChangesAsync();

            return Ok(new Response { Status = "Success", Message = "Log was successfully deleted"});
        }
        private ActivityUser UserFromJwt()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userClaims = identity.Claims;
                bool adminStatus = false;
                if (userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value == "admin")
                {
                    adminStatus = true;
                }
                return new ActivityUser
                {
                    UsrFirstName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.GivenName)?.Value,
                    UsrLastName = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Surname)?.Value,
                    UsrEmail = userClaims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value,
                    UsrIsAdmin = adminStatus

                };
            }
            return null;
        }

        private ActivityUser GetEntityUser(string email)
        {
            return _context.ActivityUsers.FirstOrDefault(x => x.UsrEmail == email);
        }

        private ActivityActivity GetActivity(string activityName)
        {
            ActivityActivity activity = _context.ActivityActivities.FirstOrDefault(o=>o.ActivityType == activityName);
            return activity;
        }

        private ActivityLog GetActivityLog(int? id)
        {
            ActivityLog log = _context.ActivityLogs.FirstOrDefault(o=>o.LogId == id);
            return log;
        }
        // return id istället, om den ej finns = 0
        private bool ActivityTypeExists(string activity)
        {
            return _context.ActivityActivities.Any(o => o.ActivityType.ToLower() == activity.ToLower());
        }

        [HttpPost("AddActivity")]
        public async void AddActivity(string activityName)
        {
            
            ActivityActivity newActivity = new ActivityActivity();
            newActivity.ActivityType = activityName.ToLower();
            _context.Add(newActivity);
            await _context.SaveChangesAsync();
        
        }

        private bool ActivityLogExists(int id)
        {
            return _context.ActivityLogs.Any(e => e.LogId == id);
        }



    }
}
