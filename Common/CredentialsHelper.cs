using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common
{
    public static class CredentialsHelper
    {
        private static bool HasNumbers(string str)
        {
            foreach(char c in str.ToCharArray())
            {
                if(Char.IsNumber(c))
                    return true;
            }
            return false;
        }
        public static bool CheckPassword(string password)
        {
            return (password.Length >= 6 && HasNumbers(password));
        }
        public static bool CheckEmail(string email)
        {
            string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
            Match isMatch = Regex.Match(email, pattern, RegexOptions.IgnoreCase);
            return isMatch.Success;
        }
    }
}
