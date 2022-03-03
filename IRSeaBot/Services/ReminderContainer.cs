using IRSeaBot.Data.Entities;
using IRSeaBot.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace IRSeaBot.Services
{
    public class ReminderContainer
    {
        public IServiceProvider Services;
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        public static List<Reminder> reminders = new List<Reminder>();
        private bool remindersLoaded = false;

        public ReminderContainer(IServiceProvider services)
        {
            Services = services;
        }

        public async Task CheckReminders(StreamWriter writer)
        {
            List<Reminder> toRemove = new List<Reminder>();
            await semaphore.WaitAsync();
            foreach (Reminder reminder in reminders)
            {
                if(reminder.RemindAt < DateTime.Now)
                {
                    writer.WriteLine(reminder.GetReminderMessage());
                    writer.Flush();
                    toRemove.Add(reminder);

                }
            }
            foreach (Reminder reminder in toRemove)
            {
                reminders.Remove(reminder);
            }
            semaphore.Release();
        }

        public async Task LoadReminders()
        {
            if (!remindersLoaded)
            {
                using var scope = Services.CreateScope();
                ReminderRepository repository = scope.ServiceProvider.GetRequiredService<ReminderRepository>();
                List<Reminder> newReminders = await repository.GetSet();
                foreach (Reminder reminder in newReminders)
                {
                    if (reminder.RemindAt > DateTime.Now)
                    {
                        await semaphore.WaitAsync();
                        reminders.Add(reminder);
                        semaphore.Release();
                    }
                }
                remindersLoaded = true;
            }
        }

        public static async Task AddReminder(Reminder reminder)
        {
            await semaphore.WaitAsync();
            reminders.Add(reminder);
            semaphore.Release();
        }
    }
}
