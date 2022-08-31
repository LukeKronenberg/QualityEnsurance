using QualityEnsurance.Models;

namespace QualityEnsurance.Services
{
    public static class UserService
    {
        public static User GetUser(this ApplicationContext context, long userId)
        {
            User user = context.Users.Find(userId);
            if (user == null)
            {
                user = new User() { Id = userId };
                context.Users.Add(user);
            }
            return user;
        }
    }
}
