using Microsoft.EntityFrameworkCore;

namespace Sirh.Infrastructure;

/// <summary>
/// Contexte de base de données de l'application.
/// Aucune entité pour l'instant : le modèle de données et les premières migrations
/// sont ajoutés à l'étape « modèle de données et module Personnel ».
/// </summary>
public sealed class SirhDbContext(DbContextOptions<SirhDbContext> options) : DbContext(options)
{
}
