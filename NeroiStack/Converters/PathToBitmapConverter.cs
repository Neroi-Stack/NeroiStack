using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace NeroiStack.Converters;

public class PathToBitmapConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		if (value is string path && !string.IsNullOrEmpty(path))
		{
			try
			{
				if (System.IO.File.Exists(path))
				{
					return new Bitmap(path);
				}
			}
			catch
			{
				// Return null or placeholder if load fails
			}
		}
		return null;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
