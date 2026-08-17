namespace Pos.Helpers
{
    public class TokenHelper
    {
        public string GenerateToken(string email, string userId)
        {
            var token = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes($"{email}|{userId}|{DateTime.UtcNow.Ticks}"));
            return token;
        }
    }
}