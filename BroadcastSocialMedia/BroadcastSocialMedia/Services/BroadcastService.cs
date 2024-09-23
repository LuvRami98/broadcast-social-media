using BroadcastSocialMedia.Data;
using BroadcastSocialMedia.Models;
using BroadcastSocialMedia.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

public class BroadcastService : IBroadcastService
{
    private readonly ApplicationDbContext _context;

    public BroadcastService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Broadcast>> GetBroadcastsForUser(string userId)
    {
        return await _context.Broadcasts
            .Where(b => b.UserId == userId)
            .ToListAsync();
    }
}

