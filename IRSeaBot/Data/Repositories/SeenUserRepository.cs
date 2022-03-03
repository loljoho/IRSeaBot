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
            SeenUser found = await _set.FirstOrDefaultAsync(x => x.User == seenUser.User);
            if(found == null)
            {
                SeenUser inserted = await Insert(seenUser);
                return inserted;
            }
            else
            {
                found.Message = seenUser.Message;
                found.Timestamp = seenUser.Timestamp;
                _context.Update(found);
                await _context.SaveChangesAsync();
                return seenUser;
            }
        }

        public async Task<SeenUser> GetByKey(string key)
        {
            try
            {
                return await _set.FirstOrDefaultAsync(x => x.User.Equals(key));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
