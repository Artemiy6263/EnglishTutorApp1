using System; using System.Collections.Generic; using System.Linq; using System.Windows; using System.Windows.Controls;
using EnglishTutor.Data; using EnglishTutor.Data.Models; using EnglishTutor.Services;
using Microsoft.EntityFrameworkCore;
namespace EnglishTutor.Windows.Student
{
    public partial class WordStudyWindow : Window
    {
        private List<Word> _words=new(); private int _idx=0;
        public WordStudyWindow(){InitializeComponent();CbDifficulty.SelectedIndex=0;}
        private void LoadWords(){using var ctx=new AppDbContext();var q=ctx.Words.AsQueryable();if(CbDifficulty.SelectedIndex==1)q=q.Where(w=>w.DifficultyLevel==DifficultyLevel.Easy);else if(CbDifficulty.SelectedIndex==2)q=q.Where(w=>w.DifficultyLevel==DifficultyLevel.Medium);else if(CbDifficulty.SelectedIndex==3)q=q.Where(w=>w.DifficultyLevel==DifficultyLevel.Hard);_words=q.OrderBy(w=>w.EnglishWord).ToList();_idx=0;Show2();}
        private void Show2(){if(_words.Count==0){TxtEnglish.Text="Нет слов";return;}var w=_words[_idx];TxtEnglish.Text=w.EnglishWord;TxtTranscription.Text=w.Transcription??"";TxtTranslation.Text=w.RussianTranslation;TxtExample.Text=string.IsNullOrEmpty(w.ExampleSentence)?"":('"'+w.ExampleSentence+'"'+"\n"+w.ExampleTranslation);TxtApiDef.Text="";PanelAnswer.Visibility=Visibility.Collapsed;BtnReveal.Visibility=Visibility.Visible;TxtProgress.Text=$"Карточка {_idx+1} из {_words.Count}";}
        private void BtnReveal_Click(object s,RoutedEventArgs e){PanelAnswer.Visibility=Visibility.Visible;BtnReveal.Visibility=Visibility.Collapsed;}
        private void BtnNext_Click(object s,RoutedEventArgs e){if(_idx<_words.Count-1){_idx++;Show2();}}
        private void BtnPrev_Click(object s,RoutedEventArgs e){if(_idx>0){_idx--;Show2();}}
        private async void BtnApi_Click(object s,RoutedEventArgs e){if(_words.Count==0)return;TxtApiDef.Text="Загружаем...";PanelAnswer.Visibility=Visibility.Visible;var r=await DictionaryApiService.GetWordInfoAsync(_words[_idx].EnglishWord);TxtApiDef.Text=r?.Definitions.Count>0?string.Join(" | ",r.Definitions.Take(2)):"Не найдено.";}
        private void DifficultyChanged(object s,SelectionChangedEventArgs e)=>LoadWords();
        private void BtnBack_Click(object s,RoutedEventArgs e){new StudentDashboardWindow().Show();this.Close();}
    }
}
