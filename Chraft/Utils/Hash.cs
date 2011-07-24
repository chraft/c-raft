using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Utils
{
    internal static class Hash
    {
        private static string BytesToHexString(byte[] byteArray)
        {
            
            StringBuilder sb = new StringBuilder(byteArray.Length);
            for (int i = 0; i < byteArray.Length; i++)
            {
                sb.Append(byteArray[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static string MD5(string input)
        {
            return BytesToHexString(ComputeHash(System.Security.Cryptography.MD5.Create(), input));
        }

        public static string MD5(byte[] inputBytes)
        {
            return BytesToHexString(ComputeHash(System.Security.Cryptography.MD5.Create(), inputBytes));
        }

        public static byte[] ComputeHash(System.Security.Cryptography.HashAlgorithm algorithm, string input)
        {
            // calculate hash from input using provided algorithm
            return ComputeHash(algorithm, System.Text.Encoding.UTF8.GetBytes(input));
        }

        public static byte[] ComputeHash(System.Security.Cryptography.HashAlgorithm algorithm, byte[] inputBytes)
        {
            // calculate hash from inputBytes using provided algorithm
            byte[] hash = algorithm.ComputeHash(inputBytes);

            return hash;
        }
    }
}
