using Common;
using System.Linq;
using NUnit.Framework;
using miniMessanger.Models;

namespace miniMessanger.Test
{
    [TestFixture]
    public class TestAuthentication
    {
        public TestAuthentication()
        {
            this.context = new Context(true, true);
            this.validator = new Validator();
            this.authentication = new Authentication(context, validator);
        }
        public Context context;
        public Validator validator;
        public Authentication authentication;
        public string UserEmail = "test@gmail.com";
        public string UserPassword = "Test1234";
        public string UserLogin = "Test";
        public string UserToken = "Test";
        public string message;
        
        [Test]
        public void GetActiveUserByLogin()
        {
            User user = CreateMockingUser();
            User Success = authentication.GetActiveUserByLogin(user.UserLogin, ref message);
            User UnknowEmail = authentication.GetActiveUserByLogin("1234", ref message);
            Assert.AreEqual(Success.UserId, user.UserId);
            Assert.AreEqual(UnknowEmail, null);
        }
        [Test]
        public void GetNonActivateUserByLogin()
        {
            User user = CreateMockingUser();
            User NonActivate = authentication.GetActiveUserByLogin(user.UserLogin, ref message);
            Assert.AreEqual(NonActivate.UserLogin, user.UserLogin);
        }
        [Test]
        public void GetDeletedUserByEmail()
        {
            User user = CreateMockingUser();
            user.Deleted = true;
            context.Update(user);
            context.SaveChanges();
            User DeletedUser = authentication.GetActiveUserByLogin(user.UserLogin, ref message);
            Assert.AreEqual(DeletedUser, null);
        }
        [Test]
        public void GetUserByToken()
        {
            User user = CreateMockingUser();
            User Success = authentication.GetUserByToken(user.UserToken, ref message);
            User UnknowToken = authentication.GetUserByToken("1234", ref message);
            Assert.AreEqual(Success.UserId, user.UserId);
            Assert.AreEqual(UnknowToken, null);
        }
        [Test]
        public void GetNonActivateUserByToken()
        {
            DeleteUser();
            User user = authentication.CreateUser(UserLogin, UserPassword);
            User NonActivate = authentication.GetUserByToken(user.UserLogin, ref message);
            Assert.AreEqual(NonActivate, null);
        }
        [Test]
        public void GetDeletedUserByToken()
        {
            User user = CreateMockingUser();
            user.Deleted = true;
            context.Update(user);
            context.SaveChanges();
            User DeletedUser = authentication.GetUserByToken(user.UserLogin, ref message);
            Assert.AreEqual(DeletedUser, null);
        }
        [Test]
        public void LogOut()
        {
            User user = CreateMockingUser();
            bool success = authentication.LogOut(user.UserToken, ref message);
            bool unsuccess = authentication.LogOut("", ref message);
            Assert.AreEqual(success, true);
            Assert.AreEqual(unsuccess, false);
        }
        [Test]
        public void GetUserByLogin()
        {
            User user = CreateMockingUser();
            User success = authentication.GetUserByLogin(user.UserLogin, ref message);
            User unsuccess = authentication.GetUserByLogin("1234", ref message);
            Assert.AreEqual(success.UserLogin, user.UserLogin);
            Assert.AreEqual(unsuccess, null);
        }
        [Test]
        public void Auth()
        {
            DeleteUser();
            UserCache cache = new UserCache();
            cache.user_login = UserLogin;
            cache.user_password = UserPassword;
            User success = authentication.Auth(cache, ref message);
            User unsuccess = authentication.Auth(cache, ref message);
            Assert.AreEqual(success.UserLogin, UserLogin);
            Assert.AreEqual(unsuccess, null);
        }
        [Test]
        public void CreateUser()
        {
            User success = authentication.CreateUser(UserLogin, UserPassword);
            User unsuccess = authentication.CreateUser("", "");
            Assert.AreEqual(success.UserLogin, UserLogin);
            Assert.AreEqual(unsuccess, null);
        }
        public User CreateMockingUser()
        {
            DeleteUser();
            User user = authentication.CreateUser(UserLogin, UserPassword);
            UserToken = user.UserToken;
            context.User.Update(user);
            context.SaveChanges();
            return user;
        }
        public void DeleteUser()
        {
            System.Collections.Generic.List<User> users = context.User.Where(u => u.UserLogin == UserLogin).ToList();
            context.User.RemoveRange(users);
            context.SaveChanges();
        }
    }
}