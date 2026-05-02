using EnglishTutor.Data;
using EnglishTutor.Data.Models;

namespace EnglishTutor.Services
{
    public class AuthService
    {
        private static User? _currentUser;
        public static User? CurrentUser => _currentUser;
        public static bool IsAdmin => _currentUser?.Role == UserRole.Admin;

        public static bool Login(string username, string password)
        {
            using var ctx = new AppDbContext();
            var user = ctx.Users.FirstOrDefault(u => u.Username == username && u.IsActive);
            if (user == null) return false;
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return false;
            _currentUser = user;
            return true;
        }

        public static void Logout() => _currentUser = null;

        public static List<User> GetAllUsers()
        {
            using var ctx = new AppDbContext();
            return ctx.Users.OrderBy(u => u.CreatedAt).ToList();
        }

        public static RegisterResult RegisterStudent(string username, string password, string confirmPassword, string email)
        {
            var normalizedUsername = username.Trim();
            var normalizedEmail = email.Trim();

            if (string.IsNullOrWhiteSpace(normalizedUsername)) return RegisterResult.Fail("Введите логин.");
            if (normalizedUsername.Length < 3) return RegisterResult.Fail("Логин должен содержать минимум 3 символа.");
            if (!normalizedUsername.All(ch => ch == '_' || ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z' || ch >= '0' && ch <= '9')) return RegisterResult.Fail("Логин может содержать только латинские буквы, цифры и '_'.");
            if (!string.IsNullOrWhiteSpace(normalizedEmail) && !normalizedEmail.Contains('@')) return RegisterResult.Fail("Введите корректный email или оставьте поле пустым.");
            if (string.IsNullOrEmpty(password)) return RegisterResult.Fail("Введите пароль.");
            if (password.Length < 6) return RegisterResult.Fail("Пароль должен содержать минимум 6 символов.");
            if (password != confirmPassword) return RegisterResult.Fail("Пароли не совпадают.");

            using var ctx = new AppDbContext();
            if (ctx.Users.Any(u => u.Username.ToLower() == normalizedUsername.ToLower()))
                return RegisterResult.Fail("Пользователь с таким логином уже существует.");

            ctx.Users.Add(new User
            {
                Username = normalizedUsername,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Email = normalizedEmail,
                Role = UserRole.Student,
                CreatedAt = DateTime.Now,
                IsActive = true
            });
            ctx.SaveChanges();
            return RegisterResult.Ok(normalizedUsername);
        }

        public static bool CreateUser(string username, string password, string email, UserRole role)
        {
            using var ctx = new AppDbContext();
            var normalizedUsername = username.Trim();
            if (ctx.Users.Any(u => u.Username.ToLower() == normalizedUsername.ToLower())) return false;
            ctx.Users.Add(new User { Username = normalizedUsername, PasswordHash = BCrypt.Net.BCrypt.HashPassword(password), Email = email.Trim(), Role = role, CreatedAt = DateTime.Now, IsActive = true });
            ctx.SaveChanges();
            return true;
        }

        public static bool UpdateUser(int userId, string email, UserRole role, bool isActive)
        {
            using var ctx = new AppDbContext();
            var user = ctx.Users.Find(userId);
            if (user == null) return false;
            user.Email = email.Trim();
            user.Role = role;
            user.IsActive = isActive;
            ctx.SaveChanges();
            return true;
        }

        public static bool DeleteUser(int userId)
        {
            using var ctx = new AppDbContext();
            var user = ctx.Users.Find(userId);
            if (user == null) return false;
            ctx.Users.Remove(user);
            ctx.SaveChanges();
            return true;
        }

        public static bool ChangePassword(int userId, string newPassword)
        {
            using var ctx = new AppDbContext();
            var user = ctx.Users.Find(userId);
            if (user == null) return false;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            ctx.SaveChanges();
            return true;
        }
    }

    public class RegisterResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;

        public static RegisterResult Ok(string username) => new() { Success = true, Username = username };
        public static RegisterResult Fail(string message) => new() { Success = false, Message = message };
    }
}
