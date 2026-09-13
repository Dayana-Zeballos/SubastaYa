using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Common.Interfaces;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Infrastructure.Persistence;

// Carga los datos de prueba que compartimos las dos. Es idempotente y granular: revisa qué
// falta y solo inserta eso, así nadie pierde lo que haya cargado a mano mientras probaba.
public static class DbSeeder
{
    // Misma contraseña para los cuatro usuarios de prueba, documentada en el README.
    public const string TestPassword = "Subasta2026!";

    private const string SellerEmail = "vendedor@test.com";
    private const string Buyer1Email = "comprador1@test.com";
    private const string Buyer2Email = "comprador2@test.com";
    private const string BrokeEmail = "sinfondos@test.com";

    public static async Task SeedAsync(
        SubastaYaDbContext context,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken = default)
    {
        var categories = await SeedCategoriesAsync(context, cancellationToken);
        var users = await SeedUsersAsync(context, passwordHasher, cancellationToken);
        await SeedAuctionsAsync(context, categories, users, cancellationToken);
    }

    private static async Task<Dictionary<string, Category>> SeedCategoriesAsync(
        SubastaYaDbContext context,
        CancellationToken cancellationToken)
    {
        var existing = await context.Categories.ToDictionaryAsync(c => c.Slug, cancellationToken);

        var wanted = new[]
        {
            new Category { Name = "Tecnología", Slug = "tecnologia" },
            new Category { Name = "Coleccionables", Slug = "coleccionables" },
            new Category { Name = "Indumentaria", Slug = "indumentaria" },
            new Category { Name = "Vehículos", Slug = "vehiculos" }
        };

        foreach (var category in wanted.Where(c => !existing.ContainsKey(c.Slug)))
        {
            context.Categories.Add(category);
            existing[category.Slug] = category;
        }

        await context.SaveChangesAsync(cancellationToken);

        return existing;
    }

    private static async Task<Dictionary<string, User>> SeedUsersAsync(
        SubastaYaDbContext context,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken)
    {
        var emails = new[] { SellerEmail, Buyer1Email, Buyer2Email, BrokeEmail };

        var existing = await context.Users
            .Include(u => u.Wallet)
            .Where(u => emails.Contains(u.Email))
            .ToDictionaryAsync(u => u.Email, cancellationToken);

        if (existing.Count == emails.Length)
        {
            return existing;
        }

        var hash = passwordHasher.Hash(TestPassword);

        // Los saldos salen de los casos de prueba: comprador1 lidera la subasta activa con
        // $45.000 retenidos, y sinfondos tiene sus $500 retenidos en la subasta vencida que
        // ganó, lo que lo deja sin saldo disponible para probar el rechazo de puja.
        AddUserIfMissing(existing, SellerEmail, "vendedor", hash, available: 0m, reserved: 0m);
        AddUserIfMissing(existing, Buyer1Email, "comprador1", hash, available: 105_000m, reserved: 45_000m);
        AddUserIfMissing(existing, Buyer2Email, "comprador2", hash, available: 200_000m, reserved: 0m);
        AddUserIfMissing(existing, BrokeEmail, "sinfondos", hash, available: 0m, reserved: 500m);

        foreach (var user in existing.Values.Where(u => context.Entry(u).State == EntityState.Detached))
        {
            context.Users.Add(user);
        }

        await context.SaveChangesAsync(cancellationToken);
        await SeedLedgerAsync(context, existing, cancellationToken);

        return existing;
    }

    private static void AddUserIfMissing(
        IDictionary<string, User> users,
        string email,
        string userName,
        string passwordHash,
        decimal available,
        decimal reserved)
    {
        if (users.ContainsKey(email))
        {
            return;
        }

        var user = new User
        {
            UserName = userName,
            Email = email,
            PasswordHash = passwordHash
        };

        user.Wallet = new Wallet
        {
            UserId = user.Id,
            AvailableBalance = available,
            ReservedBalance = reserved
        };

        users[email] = user;
    }

    // Asientos que explican cómo cada billetera llegó a su saldo. Sin esto los montos
    // aparecen puestos a dedo y el historial de movimientos sale vacío.
    private static async Task SeedLedgerAsync(
        SubastaYaDbContext context,
        IReadOnlyDictionary<string, User> users,
        CancellationToken cancellationToken)
    {
        if (await context.WalletTransactions.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var buyer1 = users[Buyer1Email];
        var buyer2 = users[Buyer2Email];
        var broke = users[BrokeEmail];

        context.WalletTransactions.AddRange(
            new WalletTransaction
            {
                WalletId = buyer1.Wallet!.Id,
                Type = WalletTransactionType.Deposit,
                Amount = 150_000m,
                Description = "Carga inicial de saldo",
                CreatedAt = now.AddDays(-3)
            },
            new WalletTransaction
            {
                WalletId = buyer1.Wallet!.Id,
                Type = WalletTransactionType.Reserve,
                Amount = 45_000m,
                Description = "Retención por puja líder",
                CreatedAt = now.AddMinutes(-10)
            },
            new WalletTransaction
            {
                WalletId = buyer2.Wallet!.Id,
                Type = WalletTransactionType.Deposit,
                Amount = 200_000m,
                Description = "Carga inicial de saldo",
                CreatedAt = now.AddDays(-3)
            },
            new WalletTransaction
            {
                WalletId = buyer2.Wallet!.Id,
                Type = WalletTransactionType.Reserve,
                Amount = 40_000m,
                Description = "Retención por puja líder",
                CreatedAt = now.AddMinutes(-30)
            },
            new WalletTransaction
            {
                WalletId = buyer2.Wallet!.Id,
                Type = WalletTransactionType.Release,
                Amount = 40_000m,
                Description = "Liberación al ser superado por otra puja",
                CreatedAt = now.AddMinutes(-10)
            },
            new WalletTransaction
            {
                WalletId = broke.Wallet!.Id,
                Type = WalletTransactionType.Deposit,
                Amount = 500m,
                Description = "Carga inicial de saldo",
                CreatedAt = now.AddDays(-3)
            },
            new WalletTransaction
            {
                WalletId = broke.Wallet!.Id,
                Type = WalletTransactionType.Reserve,
                Amount = 500m,
                Description = "Retención por puja líder",
                CreatedAt = now.AddDays(-2)
            });

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedAuctionsAsync(
        SubastaYaDbContext context,
        IReadOnlyDictionary<string, Category> categories,
        IReadOnlyDictionary<string, User> users,
        CancellationToken cancellationToken)
    {
        if (await context.Auctions.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var seller = users[SellerEmail];
        var buyer1 = users[Buyer1Email];
        var buyer2 = users[Buyer2Email];
        var broke = users[BrokeEmail];

        // Caso 1: activa estándar, cierra en 25 minutos y ya tiene dos pujas cargadas.
        var standard = new Auction
        {
            Title = "Notebook Lenovo ThinkPad T14 Gen 3",
            Description = "Core i7 de 12ª generación, 16 GB de RAM y 512 GB SSD. Batería con 94% de salud y cargador original incluido.",
            ImageUrl = "https://picsum.photos/seed/thinkpad/600/400",
            CategoryId = categories["tecnologia"].Id,
            SellerId = seller.Id,
            StartingPrice = 30_000m,
            MinimumIncrement = 5_000m,
            CurrentPrice = 45_000m,
            HighestBidderId = buyer1.Id,
            StartsAt = now.AddHours(-2),
            EndsAt = now.AddMinutes(25),
            Status = AuctionStatus.Active
        };

        // Caso 2: activa crítica, cierra en 90 segundos. Sirve para ver la alerta visual del
        // temporizador y para disparar la extensión por anti-sniping.
        var critical = new Auction
        {
            Title = "Figura Bandai Gundam RX-78-2 sellada",
            Description = "Edición Master Grade en caja original sin abrir. Pieza de colección.",
            ImageUrl = "https://picsum.photos/seed/gundam/600/400",
            CategoryId = categories["coleccionables"].Id,
            SellerId = seller.Id,
            StartingPrice = 12_000m,
            MinimumIncrement = 1_000m,
            CurrentPrice = 12_000m,
            StartsAt = now.AddHours(-1),
            EndsAt = now.AddSeconds(90),
            Status = AuctionStatus.Active
        };

        // Caso 3: próxima, abre recién en 24 horas. Las pujas tienen que salir rechazadas.
        var scheduled = new Auction
        {
            Title = "Fiat Cronos Drive 1.3 modelo 2021",
            Description = "62.000 km, service oficial al día, único dueño. Título y verificación policial en regla.",
            ImageUrl = "https://picsum.photos/seed/cronos/600/400",
            CategoryId = categories["vehiculos"].Id,
            SellerId = seller.Id,
            StartingPrice = 8_500_000m,
            MinimumIncrement = 100_000m,
            CurrentPrice = 8_500_000m,
            StartsAt = now.AddHours(24),
            EndsAt = now.AddHours(48),
            Status = AuctionStatus.Scheduled
        };

        // Caso 4: venció hace dos horas con una puja ganadora, todavía sin liquidar. Queda
        // así a propósito para que el worker la cierre y transfiera los fondos.
        var expiredWithWinner = new Auction
        {
            Title = "Campera de cuero vintage talle M",
            Description = "Cuero vacuno legítimo, forro interior de abrigo. Usada, en muy buen estado.",
            ImageUrl = "https://picsum.photos/seed/campera/600/400",
            CategoryId = categories["indumentaria"].Id,
            SellerId = seller.Id,
            StartingPrice = 400m,
            MinimumIncrement = 100m,
            CurrentPrice = 500m,
            HighestBidderId = broke.Id,
            StartsAt = now.AddDays(-3),
            EndsAt = now.AddHours(-2),
            Status = AuctionStatus.Active
        };

        // Caso 5: venció hace una hora sin una sola puja. El worker la tiene que pasar a
        // Deserted sin mover un peso.
        var expiredDeserted = new Auction
        {
            Title = "Álbum de figuritas Mundial 1994 incompleto",
            Description = "Le faltan 23 figuritas. Tapa despegada en la esquina inferior.",
            ImageUrl = "https://picsum.photos/seed/album/600/400",
            CategoryId = categories["coleccionables"].Id,
            SellerId = seller.Id,
            StartingPrice = 25_000m,
            MinimumIncrement = 2_500m,
            CurrentPrice = 25_000m,
            StartsAt = now.AddDays(-3),
            EndsAt = now.AddHours(-1),
            Status = AuctionStatus.Active
        };

        context.Auctions.AddRange(standard, critical, scheduled, expiredWithWinner, expiredDeserted);

        context.Bids.AddRange(
            new Bid
            {
                AuctionId = standard.Id,
                BidderId = buyer2.Id,
                Amount = 40_000m,
                CreatedAt = now.AddMinutes(-30)
            },
            new Bid
            {
                AuctionId = standard.Id,
                BidderId = buyer1.Id,
                Amount = 45_000m,
                CreatedAt = now.AddMinutes(-10)
            },
            new Bid
            {
                AuctionId = expiredWithWinner.Id,
                BidderId = broke.Id,
                Amount = 500m,
                CreatedAt = now.AddDays(-2)
            });

        await context.SaveChangesAsync(cancellationToken);
    }
}
