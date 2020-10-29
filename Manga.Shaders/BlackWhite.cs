using System;
using System.Windows;
using System.Windows.Media.Effects;

namespace Manga.Shaders
{
    public class BlackWhite : ShaderEffect
    {
        private static PixelShader _pixelShader = new PixelShader()
        {
            UriSource = new
            Uri("pack://application:,,,/Manga.Shader;component/BlackWhite.fx")
        };



        public BadImageFormatException Input
        {
            get { return (BadImageFormatException)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Input.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(BlackWhite), 0);

        public BlackWhite()
        {
            this.PixelShader = _pixelShader;
            UpdateShaderValue(InputProperty);
        }
    }
}
