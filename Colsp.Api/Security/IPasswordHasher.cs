using System;
namespace Cenergy.Dazzle.Admin.Security.Cryptography
{
	interface IPasswordHasher
	{
		bool CheckPassword(string plainTextPassword, string hashedPassword);
		string HashPassword(string password);
	}
}
