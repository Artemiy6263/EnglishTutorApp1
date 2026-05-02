namespace EnglishTutor.Data.Models
{
    public class LessonWord
    {
        public int LessonWordId { get; set; }
        public int LessonId { get; set; }
        public Lesson Lesson { get; set; } = null!;
        public int WordId { get; set; }
        public Word Word { get; set; } = null!;
        public int OrderIndex { get; set; }
    }
}
