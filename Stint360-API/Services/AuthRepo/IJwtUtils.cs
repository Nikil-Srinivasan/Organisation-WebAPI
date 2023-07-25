namespace Organisation_WebAPI.Services.AuthRepo
{
    public interface IJwtUtils
    {
        public string GenerateJwtToken(User user);
        public int GetUserId();
    }
}
