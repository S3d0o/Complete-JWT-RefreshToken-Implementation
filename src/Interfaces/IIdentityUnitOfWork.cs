using Domain.Entities.IdentityModule;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using System;

namespace Domain.Contracts.IdentityDb
{
    public interface IIdentityUnitOfWork
    {
        IRefreshTokenRepository RefreshTokenRepository { get; }
        Task<IDbContextTransaction> BeginTransactionAsync();   // add this!
        Task<int> SaveChangesAsync();
    }
}
