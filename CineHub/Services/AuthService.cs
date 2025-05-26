using Microsoft.EntityFrameworkCore;
using CineHub.Data;
using CineHub.Models;
using System.Security.Cryptography;
using System.Text;

namespace CineHub.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message, User? User)> RegisterAsync(string name, string email, string password)
        {
            // Verificar se já existe usuário com o mesmo email
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower() && u.DeletionDate == null))
            {
                return (false, "Este email já está em uso.", null);
            }

            // Verificar se já existe usuário com o mesmo nome
            if (await _context.Users.AnyAsync(u => u.Name.ToLower() == name.ToLower()))
            {
                return (false, "Este nome de usuário já está em uso.", null);
            }

            var user = new User
            {
                Name = name,
                Email = email.ToLower(),
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow
            };

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return (true, "Usuário registrado com sucesso!", user);
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao registrar usuário: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string Message, User? User)> LoginAsync(string email, string password)
        {
            var user = await _context.Users
                    .FirstOrDefaultAsync(u =>
                        u.Email.ToLower() == email.ToLower() && u.DeletionDate == null);

            if (user == null)
            {
                return (false, "Email ou senha incorretos.", null);
            }

            if (!VerifyPassword(password, user.PasswordHash))
            {
                return (false, "Email ou senha incorretos.", null);
            }

            // Atualizar último login
            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (true, "Login realizado com sucesso!", user);
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Ratings)
                    .ThenInclude(r => r.Movie)
                .Include(u => u.Favorites)
                    .ThenInclude(f => f.Movie)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> IsEmailAvailableAsync(string email, int? excludeUserId = null)
        {
            var query = _context.Users.Where(u => u.Email.ToLower() == email.ToLower());

            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.Id != excludeUserId.Value);
            }

            return !await query.AnyAsync();
        }

        public async Task<bool> IsNameAvailableAsync(string name, int? excludeUserId = null)
        {
            var query = _context.Users.Where(u => u.Name.ToLower() == name.ToLower());

            if (excludeUserId.HasValue)
            {
                query = query.Where(u => u.Id != excludeUserId.Value);
            }

            return !await query.AnyAsync();
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
    }
}