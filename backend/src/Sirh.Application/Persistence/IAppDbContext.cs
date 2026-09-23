using Microsoft.EntityFrameworkCore;
using Sirh.Domain.Personnel;

namespace Sirh.Application.Persistence;

/// <summary>
/// Ce dont la couche application a besoin de la base de données, sans dépendre du
/// fournisseur (MySQL, Identity…) : uniquement les tables métier. Implémentée par
/// Sirh.Infrastructure.SirhDbContext.
/// </summary>
public interface IAppDbContext
{
    DbSet<Establishment> Establishments { get; }
    DbSet<Department> Departments { get; }
    DbSet<JobPosition> JobPositions { get; }
    DbSet<Employee> Employees { get; }
    DbSet<Contract> Contracts { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
