namespace Portfolio.Api.Models
{
    public class UserModel
    {
        public int Id { get; set; }           // Unique identifier for the user
        public string Name { get; set; }      // User's name
        public string Email { get; set; }     // User's email
        public string Password { get; set; }  // Optional: user's password (make sure to hash passwords)
        public DateTime CreatedAt { get; set; } // When the user was created
    }

}
