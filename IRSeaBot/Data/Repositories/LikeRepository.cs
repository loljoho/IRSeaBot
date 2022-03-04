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
            Like oldLike = await GeyByKeyAndChannel(like.Key, like.ReplyTo);
            if (oldLike == null)
            {
                await Insert(like);
                return like;
            }
            else
            {
                oldLike.Score += like.Score;
                await Edit(oldLike);
                return oldLike;
            }
        }

        public async Task<Like> GeyByKeyAndChannel(string key, string replyTo)
        {
            try
            {
                return await _set.FirstOrDefaultAsync(x => x.Key.Equals(key) && x.ReplyTo.Equals(replyTo));
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
