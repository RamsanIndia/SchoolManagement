// Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs
using Microsoft.EntityFrameworkCore;
using SchoolManagement.Application.Interfaces;
using SchoolManagement.Domain.Entities;
using SchoolManagement.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SchoolManagement.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly SchoolManagementDbContext _context;

        public RefreshTokenRepository(SchoolManagementDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RefreshToken>()
                .FirstOrDefaultAsync(rt => rt.Id == id && !rt.IsDeleted, cancellationToken);
        }

        public async Task<RefreshToken> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RefreshToken>()
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsDeleted, cancellationToken);
        }

        public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RefreshToken>()
                .Where(rt => rt.UserId == userId && !rt.IsDeleted)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _context.Set<RefreshToken>()
                .Where(rt => rt.UserId == userId &&
                            !rt.IsDeleted &&
                            !rt.IsRevoked &&
                            rt.ExpiryDate > now)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            await _context.Set<RefreshToken>().AddAsync(refreshToken, cancellationToken);
        }

        public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            _context.Set<RefreshToken>().Update(refreshToken);
            await Task.CompletedTask;
        }

        public async Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            var expiredTokens = await _context.Set<RefreshToken>()
                .Where(rt => rt.ExpiryDate < DateTime.UtcNow || rt.IsRevoked)
                .ToListAsync(cancellationToken);

            _context.Set<RefreshToken>().RemoveRange(expiredTokens);
        }
    }
}
