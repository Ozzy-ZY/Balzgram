using Domain.Models;
using Infrastructure.DataAccess.Db;
using Infrastructure.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ImageRepository : IImageRepository
{
    private readonly AppDbContext _context;

    public ImageRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Image?> GetByIdAsync(Guid imageId, CancellationToken cancellationToken = default)
    {
        return _context.Images
            .FirstOrDefaultAsync(i => i.Id == imageId, cancellationToken);
    }
    public Task<Image?> GetProfilePictureByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return _context.Images
            .Where(i => i.UserId == userId && i.Type == ImageType.ProfilePicture)
            .OrderByDescending(i => i.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Image>> GetImagesByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Images
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Image image, CancellationToken cancellationToken = default)
    {
        await _context.Images.AddAsync(image, cancellationToken);
    }

    public async Task<bool> SoftDeleteAsync(Guid imageId, CancellationToken cancellationToken = default)
    {
        var image = await _context.Images
            .FirstOrDefaultAsync(i => i.Id == imageId, cancellationToken);

        if (image == null)
        {
            return false;
        }

        image.IsDeleted = true;
        image.DeletedAtUtc = DateTime.UtcNow;
        return true;
    }
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}

