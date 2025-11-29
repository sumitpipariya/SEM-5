namespace My_Project.Models
{
    public class LoginModel
    {
        public string FullName { get; set; }   // or Username, depending on how you log in
        public string Password { get; set; }
        public string Role { get; set; }    // optional, default could be "User"

        public int UserID { get; set; }
    }
}
