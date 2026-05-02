using System; using System.Collections.Generic; using System.Linq; using System.Windows; using System.Windows.Controls;
using EnglishTutor.Data; using EnglishTutor.Data.Models; using EnglishTutor.Services; using EnglishTutor.Windows;
using Microsoft.EntityFrameworkCore;
namespace EnglishTutor.Windows.Admin
{
    public partial class ManageWordsWindow : Window
    {
        private Word? _sel; private List<WordCategory> _cats=new();
        public ManageWordsWindow(){InitializeComponent();Load();}
        private void Load(){using var ctx=new AppDbContext();_cats=ctx.WordCategories.ToList();CbCategory.ItemsSource=_cats;CbCategory.DisplayMemberPath="Name";CbCategory.SelectedValuePath="CategoryId";CbCategory.SelectedIndex=0;DgWords.ItemsSource=ctx.Words.Include(w=>w.Category).ToList();}
        private void DgWords_SelectionChanged(object s,SelectionChangedEventArgs e){if(DgWords.SelectedItem is Word w){_sel=w;TxtEnglish.Text=w.EnglishWord;TxtRussian.Text=w.RussianTranslation;TxtTranscription.Text=w.Transcription??"";TxtExampleEn.Text=w.ExampleSentence;TxtExampleRu.Text=w.ExampleTranslation;CbDifficulty.SelectedIndex=(int)w.DifficultyLevel-1;CbCategory.SelectedValue=w.CategoryId;}}
        private void BtnSave_Click(object s,RoutedEventArgs e){if(string.IsNullOrWhiteSpace(TxtEnglish.Text)||string.IsNullOrWhiteSpace(TxtRussian.Text)){MessageBox.Show("Введите слово и перевод.");return;}using var ctx=new AppDbContext();var w=_sel!=null?ctx.Words.Find(_sel.WordId)??new Word():new Word();w.EnglishWord=TxtEnglish.Text.Trim();w.RussianTranslation=TxtRussian.Text.Trim();w.Transcription=TxtTranscription.Text.Trim();w.ExampleSentence=TxtExampleEn.Text.Trim();w.ExampleTranslation=TxtExampleRu.Text.Trim();w.DifficultyLevel=(DifficultyLevel)(CbDifficulty.SelectedIndex+1);w.CategoryId=(int)(CbCategory.SelectedValue??_cats.First().CategoryId);if(_sel==null)ctx.Words.Add(w);ctx.SaveChanges();MessageBox.Show("Сохранено!");Load();Clear();}
        private void BtnDelete_Click(object s,RoutedEventArgs e){if(_sel==null)return;if(MessageBox.Show("Удалить слово?","Подтверждение",MessageBoxButton.YesNo)==MessageBoxResult.Yes){using var ctx=new AppDbContext();var w=ctx.Words.Find(_sel.WordId);if(w!=null){ctx.Words.Remove(w);ctx.SaveChanges();}Load();Clear();}}
        private async void BtnApi_Click(object s,RoutedEventArgs e){var word=TxtEnglish.Text.Trim();if(string.IsNullOrEmpty(word)){MessageBox.Show("Введите слово.");return;}TxtApiResult.Text="Загружаем...";var r=await DictionaryApiService.GetWordInfoAsync(word);if(r==null){TxtApiResult.Text="Не найдено.";return;}if(!string.IsNullOrEmpty(r.Phonetic))TxtTranscription.Text=r.Phonetic;if(r.Examples.Count>0&&string.IsNullOrEmpty(TxtExampleEn.Text))TxtExampleEn.Text=r.Examples[0];TxtApiResult.Text=r.Definitions.Count>0?string.Join(" | ",r.Definitions.Take(2)):"Нет определений.";}
        private async void BtnImportWords_Click(object s,RoutedEventArgs e){if(CbCategory.SelectedValue is not int categoryId){MessageBox.Show("Выберите категорию.");return;}if(!int.TryParse(TxtImportCount.Text.Trim(),out int count)||count<1){MessageBox.Show("Введите количество слов.");return;}TxtApiResult.Text="Импортируем слова...";try{var result=await WordImportService.ImportWordsFromWordsApiAsync(categoryId,(DifficultyLevel)(CbDifficulty.SelectedIndex+1),count);TxtApiResult.Text=result.Message;MessageBox.Show(result.Message);var selectedCategoryId=categoryId;Load();CbCategory.SelectedValue=selectedCategoryId;}catch(Exception ex){TxtApiResult.Text="Ошибка импорта: "+ex.Message;MessageBox.Show("Ошибка импорта: "+ex.Message);}}
        private void BtnNew_Click(object s,RoutedEventArgs e)=>Clear();
        private void Clear(){_sel=null;TxtEnglish.Text=TxtRussian.Text=TxtTranscription.Text=TxtExampleEn.Text=TxtExampleRu.Text="";CbDifficulty.SelectedIndex=0;CbCategory.SelectedIndex=0;TxtApiResult.Text="";}
        private void NavDashboard_Click(object s,RoutedEventArgs e)=>new AdminDashboardWindow().Show();
        private void NavUsers_Click(object s,RoutedEventArgs e)=>new ManageUsersWindow().Show();
        private void NavLessons_Click(object s,RoutedEventArgs e)=>new ManageLessonsWindow().Show();
        private void NavExercises_Click(object s,RoutedEventArgs e)=>new ManageExercisesWindow().Show();
        private void NavStats_Click(object s,RoutedEventArgs e)=>new StatisticsWindow().Show();
        private void NavLogout_Click(object s,RoutedEventArgs e){AuthService.Logout();new LoginWindow().Show();this.Close();}
    }
}
