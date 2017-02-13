using TCC_Eng_Info.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TCC_Eng_Info
{
    /// <summary>
    /// View que representa visualmente a cela Braille de 6 pontos.
    /// </summary>
    public sealed partial class BrailleCell : UserControl
    {
        /// <summary>
        /// Construtor da View da cela Braille de 6 pontos.
        /// Caso a letra seja uma espaço vazio, uma célula Braille invisível será inserida.
        /// </summary>
        /// <param name="letter">Letra que será renderizada na cela Braille</param>
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
