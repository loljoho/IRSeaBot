using IRSeaBot.Models;
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

        public ReminderContainer(IServiceProvider services)
        {
            Services = services;
        }

        

        public async Task CheckReminders(StreamWriter writer)
        {
            List<Reminder> newReminders = new List<Reminder>();
            List<string> removalKeys = new List<string>();
            foreach (Reminder reminder in reminders)
            {
                if(reminder.RemindAt < DateTime.Now)
                {
                    writer.WriteLine(reminder.GetReminderMessage());
                    writer.Flush();
                    removalKeys.Add(reminder.Key);

                }
                else
                {
                    newReminders.Add(reminder);
                }
            }
            reminders = newReminders;
            using var scope = Services.CreateScope();
            IFileService<Reminder> _s = scope.ServiceProvider.GetRequiredService<IFileService<Reminder>>();
            await _s.RemoveFileItems(removalKeys);
        }

        public async Task LoadReminders()
        {
            using var scope = Services.CreateScope();
            IFileService<Reminder> _s = scope.ServiceProvider.GetRequiredService<IFileService<Reminder>>();
            FileList<Reminder> oldReminders = new FileList<Reminder>();
            oldReminders = await _s.GetFileList();
            foreach(Reminder reminder in oldReminders.Items)
            {
                if(reminder.RemindAt < DateTime.Now)
                {
                    await semaphore.WaitAsync();
                    reminders.Add(reminder);
                    semaphore.Release();
                }
            }
        }

        public static void AddReminder(Reminder reminder)
        {
            semaphore.Wait();
            reminders.Add(reminder);
            semaphore.Release();
        }
    }
}
