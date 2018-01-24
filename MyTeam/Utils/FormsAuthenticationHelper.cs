using System.Security.Cryptography;
using System.Text;

namespace MyTeam.Utils
{
    public class FormsAuthenticationHelper
    {
        
        // 替代原来的FormsAuthentication.HashPasswordForStoringInConfigFile(string, string) 
        public static string HashPasswordForStoringInConfigFile(string value, string method)
        {
            byte[] data = null;
            if ("MD5".Equals(method))
            {
                MD5 algorithm = MD5.Create();
                data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            else if ("SHA1".Equals(method))
            {
                SHA1 algorithm = SHA1.Create();
                data = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
            }
            else
            {
                return null;
            }
            
            string result = "";
            for (int i = 0; i < data.Length; i++)
            {
                result += data[i].ToString("x2").ToUpperInvariant();
            }
            return result;
        }
    }
}