#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion

using System.Text;

namespace Chraft.Utilities.Misc
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
