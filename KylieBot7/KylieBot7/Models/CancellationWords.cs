using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KylieBot7.Models
{
    public class CancellationWords
    {
        public static List<string> GetCancellationWords()
        {
            return AuthText.CancellationWords.Split(',').ToList();
        }
    }
}