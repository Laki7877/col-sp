using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Colsp.Api.Commons
{
	public static class EncryptionHelper
	{
		public static string Encrypt(string input)
		{
			return BCrypt.Net.BCrypt.HashString(input);
		}

		public static bool Verify(string input, string hash)
		{
			return BCrypt.Net.BCrypt.Verify(input, hash);
		}
	}
}