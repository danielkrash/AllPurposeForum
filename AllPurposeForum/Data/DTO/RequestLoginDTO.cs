namespace AllPurposeForum.Data.DTO
{
    public class RequestLoginDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; } = false;
    }
}