
namespace LocalDatabase_Server.Database
{
    public static class TokenGenerator
    {
        public static string GenerateLogin(string surname, string name)
        {
            return surname + '.' + name;
        }
    }
}
