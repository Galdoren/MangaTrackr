using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace Manga.Converters
{
    public class GenresConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return String.Empty;
            String[] genres = value as String[];
            String result = String.Empty;
            StringBuilder sb = new StringBuilder(String.Empty);

            for (int i = 0; i < genres.Length; i++)
            {
                if (i != 0)
                    sb.AppendFormat(", {0}", genres[i]);
                else
                sb.AppendFormat(genres[i] ?? String.Empty);
            }
            result = sb.ToString();
            sb.Clear();
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
