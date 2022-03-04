using IRSeaBot.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace IRSeaBot.Data.Repositories
{
    public class SeenUserRepository : BaseRepository<SeenUser>
    {
        public SeenUserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<SeenUser> UpsertSeenUser(SeenUser seenUser)
        {
            SeenUser found = await GetByKeyAndChannel(seenUser.User, seenUser.ReplyTo);
            if(found == null)
            {
                SeenUser inserted = await Insert(seenUser);
                return inserted;
            }
            else
            {
                found.Message = seenUser.Message;
                found.Timestamp = seenUser.Timestamp;
                await Edit(found);
                return found;
            }
        }

        public async Task<SeenUser> GetByKeyAndChannel(string key, string replyTo)
        {
            try
            {
                return await _set.FirstOrDefaultAsync(x => x.User.Equals(key) && x.ReplyTo.Equals(replyTo));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
