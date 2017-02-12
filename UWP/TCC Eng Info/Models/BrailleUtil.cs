using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace TCC_Eng_Info.Models
{
    public static class BrailleUtil
    {
        public static List<BraillePoint> GetBrailleCell(char letter)
        {
            var brailleCell = new List<BraillePoint>(6);

            var braillePoints = LetterBrailleConverter.LetterToBraille(letter);

            foreach (var point in braillePoints)
                brailleCell.Add(GetPointColors(point));

            return brailleCell;
        }

        private static BraillePoint GetPointColors(bool point)
        {
            if (point)
                return new BraillePoint() { Color = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)) };
            else
                return new BraillePoint() { Color = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)) };
        }

    }
}
