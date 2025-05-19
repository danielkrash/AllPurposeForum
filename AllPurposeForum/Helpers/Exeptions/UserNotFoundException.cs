namespace AllPurposeForum.Exeptions
{
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string userId)
            : base($"User with ID '{userId}' was not found.")
        {
        }
        public UserNotFoundException(string userId, Exception innerException)
            : base($"User with ID '{userId}' was not found.", innerException)
        {
        }
    }
}
