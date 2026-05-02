using System.Windows; using System.Windows.Controls; using System.Windows.Input;
using EnglishTutor.Data; using EnglishTutor.Data.Models;
using Microsoft.EntityFrameworkCore;
namespace EnglishTutor.Windows.Student
{
    public partial class LessonsWindow : Window
    {
        public LessonsWindow(){InitializeComponent();using var ctx=new AppDbContext();LbLessons.ItemsSource=ctx.Lessons.Where(l=>l.IsActive).OrderBy(l=>l.OrderNumber).ToList();}
        private void LessonSelected(object s,MouseButtonEventArgs e){if(LbLessons.SelectedItem is Lesson lesson){TxtLessonTitle.Text=$"Слова урока: {lesson.Title}";TxtLessonTitle.Visibility=Visibility.Visible;DgWords.Visibility=Visibility.Visible;using var ctx=new AppDbContext();DgWords.ItemsSource=ctx.LessonWords.Include(lw=>lw.Word).Where(lw=>lw.LessonId==lesson.LessonId).OrderBy(lw=>lw.OrderIndex).Select(lw=>lw.Word).ToList();}}
        private void BtnBack_Click(object s,RoutedEventArgs e){new StudentDashboardWindow().Show();this.Close();}
    }
}
