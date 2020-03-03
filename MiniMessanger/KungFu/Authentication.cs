using System;
using Common;
using Serilog;
using System.Linq;
using Serilog.Core;
using miniMessanger.Models;

namespace miniMessanger
{
    public class Authentication
    {
        public Context context;
        public Validator validator;
        public MailF mail;
        public Logger log;
        public string IP;
        public int PORT;
        public Authentication(Context context, Validator validator)
        {
            this.context = context;
            this.validator = validator;
            mail = new MailF();
            Config config = new Config();
            IP = config.IP;
            PORT = config.Port;
            log = new LoggerConfiguration()
            .WriteTo.File("./logs/log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
        }
        public bool LogOut(string UserToken, ref string message)
        {
            User user = GetUserByToken(UserToken, ref message);
            if (user != null)
            {
                user.UserToken = validator.GenerateHash(40);
                context.User.Update(user);
                context.SaveChanges();
                log.Information("User log out, id -> " + user.UserId);
                return true;
            }   
            return false;
        }
        public User GetUserByToken(string userToken, ref string message)
        {
            if (!string.IsNullOrEmpty(userToken)) {
                User user = context.User.Where(u 
                => u.UserToken == userToken
                && !u.Deleted).FirstOrDefault();
                if (user == null)
                    message = "Server can't define user by token.";
                return user;
            }
            return null;
        }
        public User Auth(UserCache cache, ref string message)
        {
            User user = null;
            if (validator.ValidateUser(cache, ref message)) {
                if ((user = GetUserByLogin(cache.user_login, ref message)) == null) {
                    user = CreateUser(cache.user_login, cache.user_password);
                    message = "User account was successfully registered. See your email to activate account by link.";
                }
                else {
                    if (validator.VerifyHashedPassword(user.UserPassword, cache.user_password)) {
                        user.LastLoginAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        context.User.Update(user);
                        context.SaveChanges();
                        user.Profile = CreateIfNotExistProfile(user.UserId);
                        log.Information("User login, id -> " + user.UserId);
                    }
                    else {
                        message = "Wrong password.";
                        user = null;
                    }
                }
            }
            return user;
        }
        /// <exception>
        /// You can't create a new user with the same email in database. You need to check out it before use this method.
        /// </exception>
        public User CreateUser(string UserLogin, string UserPassword)
        {
            if (!string.IsNullOrEmpty(UserLogin) 
                && !string.IsNullOrEmpty(UserPassword)) {
                User user = new User() {
                    UserLogin = UserLogin,
                    UserPassword = validator.HashPassword(UserPassword),
                    Deleted = false,
                    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    LastLoginAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    UserToken = validator.GenerateHash(40),
                    UserPublicToken = validator.GenerateHash(20)
                };
                context.User.Add(user);
                context.SaveChanges();
                user.Profile = CreateIfNotExistProfile(user.UserId);
                log.Information("Registrate new user, id -> " + user.UserId);
                return user;
            }
            return null;
        }
        public User GetUserByLogin(string UserLogin, ref string message)
        {
            if (!string.IsNullOrEmpty(UserLogin)) {
                User user = context.User.Where(u => u.UserLogin == UserLogin).FirstOrDefault();
                if (user == null)
                    message = "Server can't define user by login.";
                return user;
            }
            return null;
        }
        public User GetActiveUserByLogin(string UserLogin, ref string message)
        {
            if (!string.IsNullOrEmpty(UserLogin)) {
                User user = context.User.Where(u 
                => u.UserLogin == UserLogin
                && !u.Deleted).FirstOrDefault();
                if (user == null)
                    message = "Server can't define user by login.";
                return user;
            }
            return null;
        }
        public Profile CreateIfNotExistProfile(int UserId)
        {
            Profile profile = context.Profile.Where(p => p.UserId == UserId).FirstOrDefault();
            if (profile == null) {
                profile = new Profile();
                profile.UserId = UserId;
                profile.ProfileGender = true;
                context.Add(profile);
                context.SaveChanges();
            }
            return profile;
        }
    }
}