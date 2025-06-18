namespace AsyncWebAPIExample.Services
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public UserInfo AdditionalInfo { get; set; }
    }
}