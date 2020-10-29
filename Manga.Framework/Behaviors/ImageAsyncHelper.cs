using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Manga.Framework.Behaviors
{

    public class ImageAsyncHelper : DependencyObject
    {
        #region Old

        public static String GetSourceUri(DependencyObject obj) { return (String)obj.GetValue(SourceUriProperty); }
        public static void SetSourceUri(DependencyObject obj, String value) { obj.SetValue(SourceUriProperty, value); }
        public static readonly DependencyProperty SourceUriProperty = DependencyProperty.RegisterAttached("SourceUri", typeof(String), typeof(ImageAsyncHelper), new PropertyMetadata
        {
            PropertyChangedCallback = (obj, e) =>
            {
                ((Image)obj).SetBinding(Image.SourceProperty,
                  new Binding("VerifiedUri")
                  {
                      Source = new ImageAsyncHelper
                      {
                          GivenUri = !String.IsNullOrEmpty((String)e.NewValue) ? (new Uri((String)e.NewValue)) : (null),
                      },
                      IsAsync = true,
                  });                
            }
        });

        Uri GivenUri;
        public Uri VerifiedUri
        {
            get
            {
                try
                {
                    Dns.GetHostEntry(GivenUri.DnsSafeHost);
                    return GivenUri;
                }
                catch (Exception)
                {
                    return null;
                }

            }
        }

        #endregion
    }
}
