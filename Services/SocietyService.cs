using Microsoft.EntityFrameworkCore;
using MTSS.Data;
using MTSS.DTOs.Society;
using MTSS.Models;

namespace MTSS.Services
{
    public class SocietyService
    {
        private readonly ApplicationDbContext _context;

        public SocietyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Society>> GetAllAsync()
        {
            return await _context.Societies
                .OrderBy(s => s.SocietyId)
                .ToListAsync();
        }

        public async Task<Society?> GetByIdAsync(int id)
        {
            return await _context.Societies
                .FirstOrDefaultAsync(s => s.SocietyId == id);
        }

        public async Task<Society> CreateAsync(CreateSocietyRequest request)
        {
            ValidateRequest(
                request.Name,
                request.Address,
                request.City,
                request.State,
                request.Pincode);

            var society = new Society
            {
                Name = request.Name.Trim(),
                Address = request.Address.Trim(),
                City = request.City.Trim(),
                State = request.State.Trim(),
                Pincode = request.Pincode.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Societies.Add(society);
            await _context.SaveChangesAsync();

            return society;
        }

        public async Task<Society> UpdateAsync(
            int id,
            UpdateSocietyRequest request)
        {
            var society = await _context.Societies
                .FirstOrDefaultAsync(s => s.SocietyId == id);

            if (society == null)
            {
                throw new KeyNotFoundException("Society not found.");
            }

            ValidateRequest(
                request.Name,
                request.Address,
                request.City,
                request.State,
                request.Pincode);

            society.Name = request.Name.Trim();
            society.Address = request.Address.Trim();
            society.City = request.City.Trim();
            society.State = request.State.Trim();
            society.Pincode = request.Pincode.Trim();

            await _context.SaveChangesAsync();

            return society;
        }

        public async Task<Society> SetActiveStatusAsync(
            int id,
            bool isActive)
        {
            var society = await _context.Societies
                .FirstOrDefaultAsync(s => s.SocietyId == id);

            if (society == null)
            {
                throw new KeyNotFoundException("Society not found.");
            }

            society.IsActive = isActive;

            await _context.SaveChangesAsync();

            return society;
        }

        private static void ValidateRequest(
            string name,
            string address,
            string city,
            string state,
            string pincode)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Society name is required.");

            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Society address is required.");

            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("Society city is required.");

            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentException("Society state is required.");

            if (string.IsNullOrWhiteSpace(pincode))
                throw new ArgumentException("Society pincode is required.");
        }
    }
}