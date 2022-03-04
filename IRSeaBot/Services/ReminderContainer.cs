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
        private static readonly SemaphoreSlim semaphore = new(1);
        private static readonly List<Reminder> reminders = new();
        private bool remindersLoaded = false;

        public ReminderContainer(IServiceProvider services)
        {
            Services = services;
        }

        public async Task CheckReminders(StreamWriter writer, string channel)
        {
            try
            {
                await semaphore.WaitAsync();
                List<Reminder> toRemove = new();
                foreach (Reminder reminder in reminders)
                {
                    if (reminder.RemindAt < DateTime.Now && reminder.ReplyTo.Equals(channel))
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
            }catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                semaphore.Release();
            }     
        }

        public async Task LoadReminders()
        {
            if (!remindersLoaded)
            {
                try
                {
                    using var scope = Services.CreateScope();
                    ReminderRepository repository = scope.ServiceProvider.GetRequiredService<ReminderRepository>();
                    List<Reminder> newReminders = await repository.GetSet();
                    await semaphore.WaitAsync();
                    foreach (Reminder reminder in newReminders)
                    {
                        if (reminder.RemindAt > DateTime.Now)
                        {
                            reminders.Add(reminder);
                        }
                    }
                    remindersLoaded = true;
                }catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    semaphore.Release();
                }

            }
        }

        public static async Task AddReminder(Reminder reminder)
        {
            try
            {
                await semaphore.WaitAsync();
                reminders.Add(reminder);
            }catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                semaphore.Release();
            }            
        }
    }
}
