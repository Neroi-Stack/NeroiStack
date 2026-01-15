namespace NeroiStack.Common.Service;
using NeroiStack.Common.Interface;

public class MimeType : IMimeType
{
	public async Task<string> Get(byte[] bytes)
	{
		if (bytes.Length < 4) return "application/octet-stream";

		if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
			return "image/jpeg";

		if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
			return "image/png";

		if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46)
			return "image/gif";

		if (bytes[0] == 0x42 && bytes[1] == 0x4D)
			return "image/bmp";

		return "application/octet-stream";
	}
}