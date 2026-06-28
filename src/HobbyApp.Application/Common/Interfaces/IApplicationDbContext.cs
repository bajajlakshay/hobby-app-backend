using HobbyApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HobbyApp.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the persistence context the Application layer depends on.
/// Add a <see cref="DbSet{TEntity}"/> here for each aggregate as it is introduced.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Note> Notes { get; }

    DbSet<TaskItem> Tasks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
