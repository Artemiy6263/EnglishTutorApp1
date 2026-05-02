using System; using System.Collections.Generic; using System.Linq; using System.Windows; using System.Windows.Media; using System.Windows.Threading;
using EnglishTutor.Data.Models; using EnglishTutor.Services;
namespace EnglishTutor.Windows.Student
{
    public partial class ExerciseSpellingWindow : Window
    {
        private List<ExerciseQuestion> _questions=new(); private int _idx=0,_score=0,_exerciseId,_secondsLeft; private DispatcherTimer _timer=new();
        public ExerciseSpellingWindow(int exerciseId){InitializeComponent();_exerciseId=exerciseId;var ex=ExerciseService.GetExercises().FirstOrDefault(e=>e.ExerciseId==exerciseId);if(ex!=null){TxtTitle.Text=ex.Title;_secondsLeft=ex.TimeLimit;}_questions=ExerciseService.GetQuestions(exerciseId);_timer.Interval=TimeSpan.FromSeconds(1);_timer.Tick+=(s,e)=>{_secondsLeft--;TxtTimer.Text=$"⏱ {_secondsLeft/60:D2}:{_secondsLeft%60:D2}";if(_secondsLeft<=0){_timer.Stop();Finish();}};_timer.Start();ShowQ();}
        private void ShowQ(){if(_idx>=_questions.Count){Finish();return;}var q=_questions[_idx];TxtQuestion.Text=q.QuestionText;TxtAnswer.Text="";TxtHint.Visibility=Visibility.Collapsed;TxtFeedback.Visibility=Visibility.Collapsed;BtnNext.IsEnabled=false;PbProgress.Value=(double)_idx/_questions.Count*100;TxtScore.Text=$"Счёт: {_score}";}
        private void BtnCheck_Click(object s,RoutedEventArgs e){var q=_questions[_idx];if(TxtAnswer.Text.Trim().ToLower()==q.CorrectAnswer.ToLower()){_score+=q.Points;TxtFeedback.Text="✅ Правильно!";TxtFeedback.Foreground=new SolidColorBrush(Color.FromRgb(0x3B,0xAE,0x6E));}else{TxtFeedback.Text=$"❌ Правильно: {q.CorrectAnswer}";TxtFeedback.Foreground=new SolidColorBrush(Color.FromRgb(0xE8,0x53,0x6A));}TxtFeedback.Visibility=Visibility.Visible;BtnNext.IsEnabled=true;}
        private void BtnHint_Click(object s,RoutedEventArgs e){var q=_questions[_idx];TxtHint.Text=$"Подсказка: {q.Hint??($"Слово из {q.CorrectAnswer.Length} букв")}";TxtHint.Visibility=Visibility.Visible;}
        private void BtnNext_Click(object s,RoutedEventArgs e){_idx++;ShowQ();}
        private void Finish(){_timer.Stop();int max=_questions.Sum(q=>q.Points);ExerciseService.SaveProgress(AuthService.CurrentUser!.UserId,_exerciseId,_score,max,0);MessageBox.Show($"Результат: {_score}/{max}","Задание завершено");new ExercisesListWindow().Show();this.Close();}
    }
}
