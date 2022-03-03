using IRSeaBot.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IRSeaBot.Data.Repositories
{
    public class ReminderRepository : BaseRepository<Reminder>
    {
        public ReminderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task RemoveKeys(List<int> keys)
        {
            try
            {
                List<Reminder> reminders = await _set.Where(x => keys.Contains(x.Id)).ToListAsync();
                _context.RemoveRange(reminders);
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
