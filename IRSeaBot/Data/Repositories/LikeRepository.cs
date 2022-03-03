using IRSeaBot.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace IRSeaBot.Data.Repositories
{
    public class LikeRepository : BaseRepository<Like>
    {
        public LikeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Like> UpsertLike(Like like)
        {
            Like found = await _set.FirstOrDefaultAsync(x => x.Key == like.Key);
            if (found == null)
            {
                Like inserted = await Insert(like);
                return inserted;
            }
            else
            {
                found.Score = like.Score;
                _context.Update(found);
                await _context.SaveChangesAsync();
                return like;
            }
        }

        public async Task<Like> GetByKey(string key)
        {
            try
            {
                return await _set.FirstOrDefaultAsync(x => x.Key.Equals(key));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
