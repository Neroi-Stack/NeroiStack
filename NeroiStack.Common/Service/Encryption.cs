using System.Security.Cryptography;
using NeroiStack.Common.Interface;

namespace NeroiStack.Common.Service;

public class Encryption() : IEncryption
{
	private readonly byte[] _masterKey = LoadOrGenerateMasterKey();
	private const string MasterKeyFileName = "master.key";

	private static byte[] LoadOrGenerateMasterKey()
	{
		var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
		var folder = Path.Combine(appData, "NeroiStack");
		if (!Directory.Exists(folder))
		{
			Directory.CreateDirectory(folder);
		}

		var path = Path.Combine(folder, MasterKeyFileName);
		if (File.Exists(path))
		{
			return File.ReadAllBytes(path);
		}

		using var aes = Aes.Create();
		aes.KeySize = 256;
		aes.GenerateKey();
		var key = aes.Key;
		File.WriteAllBytes(path, key);
		return key;
	}
	public string Encrypt(string plainText)
	{
		if (string.IsNullOrEmpty(plainText)) return string.Empty;

		using var aes = Aes.Create();
		aes.Key = _masterKey;
		aes.GenerateIV();
		var iv = aes.IV;

		using var encryptor = aes.CreateEncryptor(aes.Key, iv);
		using var ms = new MemoryStream();
		// Write IV first
		ms.Write(iv, 0, iv.Length);

		using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
		using (var sw = new StreamWriter(cs))
		{
			sw.Write(plainText);
		}

		return Convert.ToBase64String(ms.ToArray());
	}

	public string Decrypt(string cipherTextBase64)
	{
		if (string.IsNullOrEmpty(cipherTextBase64)) return string.Empty;

		var fullCipher = Convert.FromBase64String(cipherTextBase64);

		using var aes = Aes.Create();
		aes.Key = _masterKey;

		// Extract IV (BlockSize is in bits, divide by 8 for bytes)
		var ivLength = aes.BlockSize / 8;
		if (fullCipher.Length < ivLength) return string.Empty;

		var iv = new byte[ivLength];
		Array.Copy(fullCipher, 0, iv, 0, ivLength);
		aes.IV = iv;

		using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
		// Ciphertext starts after IV
		using var ms = new MemoryStream(fullCipher, ivLength, fullCipher.Length - ivLength);
		using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
		using var sr = new StreamReader(cs);

		return sr.ReadToEnd();
	}
}