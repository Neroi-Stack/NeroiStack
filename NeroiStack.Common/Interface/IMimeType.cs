namespace NeroiStack.Common.Interface;

public interface IMimeType
{
	Task<string> Get(byte[] bytes);
}