using System.Linq; using System.Windows; using System.Windows.Controls;
using EnglishTutor.Data; using EnglishTutor.Data.Models;
namespace EnglishTutor.Windows.Student
{
    public partial class GrammarRulesWindow : Window
    {
        public GrammarRulesWindow(){InitializeComponent();using var ctx=new AppDbContext();LbTenses.ItemsSource=ctx.Tenses.OrderBy(t=>t.OrderIndex).ToList();}
        private void TenseSelected(object s,SelectionChangedEventArgs e){if(LbTenses.SelectedItem is Tense t){TxtTenseName.Text=t.Name;TxtTenseDesc.Text=t.Description;TxtFormula.Text=t.Formula;TxtExamples.Text=t.Examples;using var ctx=new AppDbContext();IcRules.ItemsSource=ctx.GrammarRules.Where(r=>r.TenseId==t.TenseId).ToList();}}
        private void BtnBack_Click(object s,RoutedEventArgs e){new StudentDashboardWindow().Show();this.Close();}
    }
}
