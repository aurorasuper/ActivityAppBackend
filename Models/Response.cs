namespace ActivityJournal.Models
{
    public class Response
    {
        public string? Status { get; set; }
        public string? Message { get; set; }
    }

    public class AuthenticateResponse
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Token { get; set; }
        
        
        public AuthenticateResponse(ActivityUser user, string token)
        {
            Id = user.UsrId;
            Email = user.UsrEmail;
            FirstName = user.UsrFirstName;
            LastName = user.UsrLastName;
            Token = token;
        }

    }
}
