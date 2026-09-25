using Microsoft.EntityFrameworkCore;
using Sirh.Application.Persistence;

namespace Sirh.Application.Personnel;

public sealed class EstablishmentService(IAppDbContext dbContext)
{
    public async Task<IReadOnlyList<EstablishmentSummary>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Establishments
            .OrderBy(e => e.Name)
            .Select(e => new EstablishmentSummary(e.Id, e.Name))
            .ToListAsync(cancellationToken);
}
