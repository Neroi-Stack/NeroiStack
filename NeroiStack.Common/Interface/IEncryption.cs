namespace NeroiStack.Common.Interface;

public interface IEncryption
{
	string Encrypt(string plainText);
	string Decrypt(string cipherText);
}