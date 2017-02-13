using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace TCC_Eng_Info.Models
{
    /// <summary>
    /// Utilitário estático responsável por realizar operaçòes em celas Braille.
    /// </summary>
    public static class BrailleUtil
    {
        /// <summary>
        /// Pega as coordenadas de uma cela Braille e em seguida as converte em uma lista de pontos Braille.
        /// </summary>
        /// <param name="letter">Letra que será convertida em uma cela Braille.</param>
        /// <returns>Uma lista de pontos Braille prontos para serem consumidos pela View.</returns>
        public static List<BraillePoint> GetBrailleCell(char letter)
        {
            var brailleCell = new List<BraillePoint>(6);

            var braillePoints = LetterBrailleConverter.LetterToBraille(letter);

            foreach (var point in braillePoints)
                brailleCell.Add(GetPointColors(point));

            return brailleCell;
        }

        /// <summary>
        /// Determina as cores que serão inseridas nos pontos Braille.
        /// Por padrão, está fixado a cor preta para a presença de ponto e a cor branca para a ausência de ponto.
        /// </summary>
        /// <param name="point">Coordenada booleana de um ponto Braille.</param>
        /// <returns>Um ponto Braille com a respectiva cor de preenchimento.</returns>
        private static BraillePoint GetPointColors(bool point)
        {
            if (point)
                return new BraillePoint() { Color = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)) };
            else
                return new BraillePoint() { Color = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)) };
        }

    }
}
