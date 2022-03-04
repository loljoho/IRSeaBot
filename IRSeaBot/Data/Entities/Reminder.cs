using IRSeaBot.Data.Entities;
using System;
using System.Collections.Generic;

namespace IRSeaBot.Data.Entities
{
    public class Reminder : IBotEntity
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Username { get; set; }
        public DateTime RemindAt { get; set; }

        public DateTime Timesamp { get; set; }

        public string ReplyTo { get; set; }

        //public string GetSendMessage(string replyTo)
        //{
        //    return $"PRIVMSG {replyTo} Ok I will remind you to do that at {RemindAt}";
        //}

        public string GetSendMessage()
        {
            return $"PRIVMSG {ReplyTo} Ok I will remind you to do that at {RemindAt}";
        }

        public string GetReminderMessage()
        {
            return $"PRIVMSG {ReplyTo} Hey {Username}, it is time to {Message}";
        }

        private static ReminderDuration ParseDuration(string durationString)
        {
            ReminderDuration duration = new();
            try
            {
                string[] years = durationString.Trim().Split('y');
                string[] months = years[years.Length - 1].Trim().Split("mo");
                string[] weeks = months[months.Length - 1].Trim().Split("w");
                string[] days = weeks[weeks.Length - 1].Trim().Split("d");
                string[] hours = days[days.Length - 1].Trim().Split("h");
                string[] minutes = hours[hours.Length - 1].Trim().Split("m");
                string[] seconds = minutes[minutes.Length - 1].Trim().Split("s");
                List<string[]> intervalStrings = new List<string[]>
                {
                    years, months, weeks, days, hours, minutes, seconds
                };

                if (years.Length > 0)
                {
                    bool yearsParsed = int.TryParse(years[0], out int iVal);
                    if (yearsParsed) duration.Years = iVal;
                    else duration.Years = 0;

                }
                else duration.Years = 0;

                if (months.Length > 0)
                {
                    bool mosParsed = int.TryParse(months[0], out int iVal);
                    if (mosParsed) duration.Months = iVal;
                    else duration.Months = 0;

                }
                else duration.Months = 0;

                if (weeks.Length > 0)
                {
                    bool parsed = int.TryParse(weeks[0], out int iVal);
                    if (parsed) duration.Weeks = iVal;
                    else duration.Weeks = 0;

                }
                else duration.Weeks = 0;

                if (days.Length > 0)
                {
                    bool parsed = int.TryParse(days[0], out int iVal);
                    if (parsed) duration.Days = iVal;
                    else duration.Days = 0;

                }
                else duration.Days = 0;

                if (hours.Length > 0)
                {
                    bool parsed = int.TryParse(hours[0], out int iVal);
                    if (parsed) duration.Hours = iVal;
                    else duration.Hours = 0;

                }
                else duration.Hours = 0;

                if (minutes.Length > 0)
                {
                    bool parsed = int.TryParse(minutes[0], out int iVal);
                    if (parsed) duration.Minutes = iVal;
                    else duration.Minutes = 0;

                }
                else duration.Minutes = 0;

                if (seconds.Length > 0)
                {
                    bool parsed = int.TryParse(seconds[0], out int iVal);
                    if (parsed) duration.Seconds = iVal;
                    else duration.Seconds = 0;

                }
                else duration.Seconds = 0;

                return duration;
            }
            catch
            {
                return duration;
            }

        }

        public static Reminder CreateReminder(string[] input, string username, string replyTo)
        {
            try
            {
                ReminderDuration reminderDuration = ParseDuration(input[0]);
                if (reminderDuration != null && reminderDuration.isNonZero())
                {
                    DateTime remindAt = DateTime.Now.AddYears(reminderDuration.Years);
                    remindAt = remindAt.AddMonths(reminderDuration.Months);
                    remindAt = remindAt.AddDays(reminderDuration.Weeks * 7);
                    remindAt = remindAt.AddDays(reminderDuration.Days);
                    remindAt = remindAt.AddHours(reminderDuration.Hours);
                    remindAt = remindAt.AddMinutes(reminderDuration.Minutes);
                    remindAt = remindAt.AddSeconds(reminderDuration.Seconds);

                    Reminder reminder = new()
                    {
                        Message = input[1].Trim(),
                        Username = username,
                        RemindAt = remindAt,
                        Timesamp = DateTime.Now,
                        ReplyTo = replyTo
                    };
                    return reminder;
                }
                else return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
