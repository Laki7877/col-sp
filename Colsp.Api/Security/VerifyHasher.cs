using System;
using System.Security.Cryptography;

namespace Cenergy.Dazzle.Admin.Security.Cryptography
{
    public class VerifyHasher
	{
		public string GenerateVerifyKey(string hashedKey)
		{
			string key = GenerateKey();
			return key + hashedKey;
		}
		private string GenerateKey()
		{
			byte[] keyBytes = new byte[15];
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			rng.GetBytes(keyBytes);
			return Convert.ToBase64String(keyBytes);
		}
		public string GetHashedPassword(string verifyKey)
		{
			return verifyKey.Substring(20);
		}
	}
}