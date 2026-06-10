using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTwitchBot.Quotes
{
    public class Quote
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Person { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;

        public string ToDisplayString()
        {
            return $"\"{Text}\" - {Person} {Date}";
        }
    }
}
