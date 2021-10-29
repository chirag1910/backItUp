using System;

namespace BackItUp
{
    class LogsPreferences
    {
        public BackupLogResult[] logs { get; set; }
    }

    class BackupLogResult
    {
        public String date { get; set; }
        public String filesBackedUp { get; set; }
        public String timeTaken { get; set; }
    }
}
