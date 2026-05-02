using System.Globalization; using System.Windows.Data; using System.Windows.Media; using EnglishTutor.Data.Models;
namespace EnglishTutor.Converters
{
    public class DifficultyToColorConverter:IValueConverter{public object Convert(object v,Type t,object p,CultureInfo c){if(v is DifficultyLevel l) return l switch{DifficultyLevel.Easy=>new SolidColorBrush(Color.FromRgb(0x3B,0xAE,0x6E)),DifficultyLevel.Medium=>new SolidColorBrush(Color.FromRgb(0xF5,0xA6,0x23)),DifficultyLevel.Hard=>new SolidColorBrush(Color.FromRgb(0xE8,0x53,0x6A)),_=>new SolidColorBrush(Colors.Gray)};return new SolidColorBrush(Colors.Gray);}public object ConvertBack(object v,Type t,object p,CultureInfo c)=>throw new NotImplementedException();}
    public class DifficultyToTextConverter:IValueConverter{public object Convert(object v,Type t,object p,CultureInfo c){if(v is DifficultyLevel l) return l switch{DifficultyLevel.Easy=>"Лёгкий",DifficultyLevel.Medium=>"Средний",DifficultyLevel.Hard=>"Сложный",_=>"?"};return "?";}public object ConvertBack(object v,Type t,object p,CultureInfo c)=>throw new NotImplementedException();}
}
