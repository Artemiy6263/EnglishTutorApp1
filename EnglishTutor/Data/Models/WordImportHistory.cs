using System.ComponentModel.DataAnnotations;

namespace EnglishTutor.Data.Models
{
    public class WordImportHistory
    {
        public int WordImportHistoryId { get; set; }
        public DateTime ImportedAt { get; set; } = DateTime.Now;
        [MaxLength(100)] public string Source { get; set; } = string.Empty;
        [MaxLength(200)] public string CategoryName { get; set; } = string.Empty;
        public int RequestedCount { get; set; }
        public int Added { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }
        public int LinkedToLesson { get; set; }
        [MaxLength(1000)] public string Message { get; set; } = string.Empty;
    }
}
