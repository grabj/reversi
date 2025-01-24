using System.Windows.Media;

namespace Reversi
{
    static class MieszanieKolorów
    {
        public static Color Lerp(this Color kolor, Color innyKolor, double waga = 0.5)
        {
            byte r = (byte)(waga * kolor.R + (1 - waga) * innyKolor.R);
            byte g = (byte)(waga * kolor.G + (1 - waga) * innyKolor.G);
            byte b = (byte)(waga * kolor.B + (1 - waga) * innyKolor.B);
            return Color.FromRgb(r, g, b);  
        }
        public static SolidColorBrush Lerp(this SolidColorBrush pędzel, SolidColorBrush innyPędziel, double waga = 0.5)
        {
            return new SolidColorBrush(Lerp(pędzel.Color, innyPędziel.Color, waga));
        }
    }
}
