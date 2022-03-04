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
    }
}
