using System;
using System.Collections.Generic;
using System.Text;

namespace NoeSbot.Models
{
    public class UrbanMain
    {
        public List<string> Tags { get; set; }
        public string Result_type { get; set; }
        public List<UrbanItem> List { get; set; }
        public List<string> Sounds { get; set; }

        public int CurrentDef { get; set; }
        public string[] Pages { get; set; }
        public int CurrentPage { get; set; }
        public int DefinitionCount => List.Count;
        public int PagesCount => Pages.Length;
        public UrbanItem CurrentItem => List[CurrentDef - 1];
        public bool PageIconsSet { get; set; }
        public bool InHelpMode { get; set; }
    }
}
