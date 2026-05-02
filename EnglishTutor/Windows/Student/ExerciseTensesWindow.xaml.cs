using System; using System.Collections.Generic; using System.Linq; using System.Windows; using System.Windows.Controls; using System.Windows.Media; using System.Windows.Threading;
using EnglishTutor.Data.Models; using EnglishTutor.Services;
using Newtonsoft.Json;
namespace EnglishTutor.Windows.Student
{
    public partial class ExerciseTensesWindow : Window
    {
        private List<ExerciseQuestion> _questions=new(); private int _idx=0,_score=0,_exerciseId,_secondsLeft; private DispatcherTimer _timer=new(); private bool _answered=false;
        public ExerciseTensesWindow(int exerciseId){InitializeComponent();_exerciseId=exerciseId;var ex=ExerciseService.GetExercises().FirstOrDefault(e=>e.ExerciseId==exerciseId);if(ex!=null){TxtTitle.Text=ex.Title;_secondsLeft=ex.TimeLimit;}_questions=ExerciseService.GetQuestions(exerciseId);_timer.Interval=TimeSpan.FromSeconds(1);_timer.Tick+=(s,e)=>{_secondsLeft--;TxtTimer.Text=$"⏱ {_secondsLeft/60:D2}:{_secondsLeft%60:D2}";if(_secondsLeft<=0){_timer.Stop();Finish();}};_timer.Start();ShowQ();}
        private void ShowQ(){if(_idx>=_questions.Count){Finish();return;}_answered=false;var q=_questions[_idx];TxtQuestion.Text=q.QuestionText;PbProgress.Value=(double)_idx/_questions.Count*100;TxtFeedback.Visibility=Visibility.Collapsed;BtnNext.IsEnabled=false;UgOptions.Children.Clear();var opts=new List<string>();if(!string.IsNullOrEmpty(q.Options))try{opts=JsonConvert.DeserializeObject<List<string>>(q.Options)??new();}catch{}if(!opts.Contains(q.CorrectAnswer))opts.Add(q.CorrectAnswer);opts=opts.OrderBy(_=>Guid.NewGuid()).ToList();foreach(var opt in opts){var btn=new Button{Content=opt,Style=FindResource("SecondaryButton") as Style,Margin=new Thickness(6),Height=48};btn.Click+=(s,e)=>Answer(opt,q.CorrectAnswer,q.Points);UgOptions.Children.Add(btn);}TxtScore.Text=$"Счёт: {_score}";}
        private void Answer(string sel,string correct,int pts){if(_answered)return;_answered=true;BtnNext.IsEnabled=true;if(sel==correct){_score+=pts;TxtFeedback.Text="✅ Правильно!";TxtFeedback.Foreground=new SolidColorBrush(Color.FromRgb(0x3B,0xAE,0x6E));}else{TxtFeedback.Text=$"❌ Правильно: {correct}";TxtFeedback.Foreground=new SolidColorBrush(Color.FromRgb(0xE8,0x53,0x6A));}TxtFeedback.Visibility=Visibility.Visible;TxtScore.Text=$"Счёт: {_score}";}
        private void BtnNext_Click(object s,RoutedEventArgs e){_idx++;ShowQ();}
        private void Finish(){_timer.Stop();int max=_questions.Sum(q=>q.Points);ExerciseService.SaveProgress(AuthService.CurrentUser!.UserId,_exerciseId,_score,max,0);MessageBox.Show($"Результат: {_score}/{max}","Задание завершено");new ExercisesListWindow().Show();this.Close();}
    }
}
