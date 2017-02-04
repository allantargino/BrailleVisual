using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TCC_Eng_Info
{
    public sealed partial class BrailleCell : UserControl
    {
        public BrailleCell(char letter)
        {
            this.InitializeComponent();

            if (letter != ' ')
                Cell.ItemsSource = BrailleUtil.GetBrailleCell(letter);
            else
                CellContainer.BorderThickness = new Thickness(0);
        }

    }
}
