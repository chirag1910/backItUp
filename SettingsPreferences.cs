﻿using System;

namespace BackItUp
{
    class SettingsPreferences
    {
        public Boolean autoBackup { get; set; }
        public String saveLocation { get; set; }
        public String saveAs { get; set; }
        public DateTime backupTime { get; set; }
    }
}
