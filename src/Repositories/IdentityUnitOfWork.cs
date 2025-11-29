using Domain.Contracts.IdentityDb;
using Domain.Entities.IdentityModule;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;

namespace Presistance.IdentityDb.IdentityRepositories
{
    public class IdentityUnitOfWork : IIdentityUnitOfWork
    {
        private readonly IdentityStoreDbContext _db;

        public IdentityUnitOfWork(IdentityStoreDbContext db)
        {
            _db = db;
            RefreshTokenRepository = new RefreshTokenRepository(db);
        }
        public IRefreshTokenRepository RefreshTokenRepository { get; }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        => await _db.Database.BeginTransactionAsync();

        public async Task<int> SaveChangesAsync()
        => await _db.SaveChangesAsync();
    }
}
