using System;
using System.Security.Cryptography;

namespace Cenergy.Dazzle.Admin.Security.Cryptography
{
    public class SaltedSha256PasswordHasher : IPasswordHasher
    {
		public string HashPassword(string password)
		{
			string salt = GenerateSalt();
			return GenerateHash(salt, password) + salt;
		}

		public bool CheckPassword(string plainTextPassword, string hashedPassword)
		{
			if (hashedPassword.Length != 60)
			{
				if (hashedPassword.Length == 32)
				{
					return hashedPassword == GenerateHashMd5(plainTextPassword);
				}
				else
				{
					return false;
				}
			}
			string hash = hashedPassword.Substring(0, 44);
			string salt = hashedPassword.Substring(44);
			return hash == GenerateHash(salt, plainTextPassword);
		}

		private string GenerateHash(string salt, string password)
		{
			SHA256 sha256 = SHA256.Create();
			byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.Unicode.GetBytes(salt + password));
			return Convert.ToBase64String(hashBytes);
		}

		private string GenerateSalt()
		{
			byte[] saltBytes = new byte[12];
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetBytes(saltBytes);
			return Convert.ToBase64String(saltBytes);
		}

        public string GenerateHashMd5(string password)
		{
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] dataMd5 = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
			System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
			for (int i = 0; i < dataMd5.Length; i++)
			{
				strBuilder.Append(dataMd5[i].ToString("x2"));
			}
			return strBuilder.ToString();
		}
	}
}