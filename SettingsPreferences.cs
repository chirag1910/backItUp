using System;

namespace BackItUp
{
    class SettingsPreferences
    {
        public Boolean autoBackup { get; set; }
        public String saveLocation { get; set; }
        public String saveAs { get; set; }
        public DateTime backupTime { get; set; }
        public String[] ignore { get; set; }
        public Int32 compressionLevel { get; set; }
        public Boolean caching  { get; set; }
        public Int32 threads { get; set; }
        public Int32 cacheSize { get; set; }
        public Boolean useTar { get; set; }
        public String execCmd { get; set; }
    }
}
