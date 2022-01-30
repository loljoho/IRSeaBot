namespace IRSeaBot.Models
{
    public class ReminderDuration
    {
        public int Years { get; set; } = 0;
        public int Months { get; set; } = 0;
        public int Weeks { get; set; } = 0;
        public int Days { get; set; } = 0;
        public int Hours { get; set; } = 0;
        public int Minutes { get; set; } = 0;
        public int Seconds { get; set; } = 0;

        public bool isNonZero()
        {
            bool isZero = (Years == 0 && Months == 0 && Weeks == 0 && Days == 0 && Hours == 0 && Minutes == 0 && Seconds == 0);
            return !isZero;
        }
    }
}
