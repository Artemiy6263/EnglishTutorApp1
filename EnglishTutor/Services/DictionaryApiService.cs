using System.Net.Http;
using Newtonsoft.Json.Linq;
namespace EnglishTutor.Services
{
    public class DictionaryApiService
    {
        private static readonly HttpClient _http = new();
        private const string BaseUrl = "https://api.dictionaryapi.dev/api/v2/entries/en/";
        public static async Task<WordApiResult?> GetWordInfoAsync(string word)
        {
            try
            {
                var response = await _http.GetStringAsync(BaseUrl + word.ToLower().Trim());
                var json = JArray.Parse(response);
                if (!json.Any()) return null;
                var entry = json[0];
                var result = new WordApiResult { Word=entry["word"]?.ToString()??word, Phonetic=entry["phonetic"]?.ToString()??"" };
                var phonetics = entry["phonetics"] as JArray;
                if (phonetics!=null) foreach(var p in phonetics) { var a=p["audio"]?.ToString(); if(!string.IsNullOrEmpty(a)){result.AudioUrl=a;break;} }
                var meanings = entry["meanings"] as JArray;
                if (meanings!=null) foreach(var m in meanings.Take(3)) { var defs=m["definitions"] as JArray; if(defs!=null) foreach(var def in defs.Take(2)) { var d=def["definition"]?.ToString(); var ex=def["example"]?.ToString(); if(!string.IsNullOrEmpty(d)) result.Definitions.Add(d); if(!string.IsNullOrEmpty(ex)) result.Examples.Add(ex); } }
                return result;
            }
            catch { return null; }
        }
    }
    public class WordApiResult { public string Word{get;set;}=string.Empty; public string Phonetic{get;set;}=string.Empty; public string AudioUrl{get;set;}=string.Empty; public List<string> Definitions{get;set;}=new(); public List<string> Examples{get;set;}=new(); }
}
