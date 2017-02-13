using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Graphics.Printing;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Printing;

namespace TCC_Eng_Info
{
    /// <summary>
    /// Página de exportação de uma sessão de texto Braille.
    /// </summary>
    public sealed partial class ExportPage : Page
    {
        PrintManager _printManager = PrintManager.GetForCurrentView();
        PrintDocument _printDocument = null;
        PrintTask _printTask = null;

        private string _text = string.Empty;
        private int _pages = 0;
        private string TextContent { get; set; }

        /// <summary>
        /// Construtor da página de exportação de uma sessão.
        /// </summary>
        public ExportPage()
        {
            this.InitializeComponent();
            _printManager.PrintTaskRequested += Printmgr_PrintTaskRequested;
        }
        
        /// <summary>
        /// Método que é disparado após o frame ser navegado até esta página.
        /// </summary>
        /// <param name="e">Encapsula o texto proveniente de uma sessão de captura de linguagem natural para Braille.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _text = ((StringBuilder)e.Parameter).ToString();
            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// Evento de clique do botão de Imprimir. Inicializa handlers de eventos e mostra a UI de impressão.
        /// </summary>
        /// <param name="sender">Objeto que disparou a tarefa.</param>
        /// <param name="e">Encapsulamento de parâmetros do evento.</param>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (_printDocument != null)
            {
                _printDocument.GetPreviewPage -= OnGetPreviewPage;
                _printDocument.Paginate -= PrintDic_Paginate;
                _printDocument.AddPages -= PrintDic_AddPages;
            }
            this._printDocument = new PrintDocument();
            _printDocument.GetPreviewPage += OnGetPreviewPage;
            _printDocument.Paginate += PrintDic_Paginate;
            _printDocument.AddPages += PrintDic_AddPages;

            await PrintManager.ShowPrintUIAsync();
        }

        /// <summary>
        /// Cria a tarefa de impressão após ser disparado pelo evento de PrintTaskRequest.
        /// </summary>
        /// <param name="sender">Objeto que disparou a tarefa.</param>
        /// <param name="args">Encapsula os argumentos para a impressão</param>
        private void Printmgr_PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            var deferral = args.Request.GetDeferral();
            _printTask = args.Request.CreatePrintTask("Sessão de Braille Visual", OnPrintTaskSourceRequested);
            _printTask.Completed += PrintTask_Completed;
            deferral.Complete();
        }

        /// <summary>
        /// Ação a ser realizada após a tarefa de impressão estar finalizada.
        /// </summary>
        /// <param name="sender">Objeto que disparou a tarefa.</param>
        /// <param name="args">Encapsulamento de parâmetros do evento.</param>
        private void PrintTask_Completed(PrintTask sender, PrintTaskCompletedEventArgs args)
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Faz o handle do evento de impressão e indica que há uma fonte que irá imprimir.
        /// </summary>
        /// <param name="args">Encapsulamento de parâmetros do evento.</param>
        private async void OnPrintTaskSourceRequested(PrintTaskSourceRequestedArgs args)
        {
            var def = args.GetDeferral();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    args.SetSource(_printDocument?.DocumentSource);
                });
            def.Complete();
        }

        /// <summary>
        /// Adiciona os conteúdos das páginas que serão impressas. 
        /// Uma página por TextBlock da view será impresso.
        /// </summary>
        /// <param name="sender">Objeto que disparou a tarefa.</param>
        /// <param name="e">Encapsulamento de parâmetros do evento.</param>
        private void PrintDic_AddPages(Object sender, AddPagesEventArgs e)
        {
            for (int i = 0; i < _pages; i++)
            {
                var element = (UIElement)this.FindName($"Block{i}");
                _printDocument.AddPage(element);
            }

            _printDocument.AddPagesComplete();
        }

        /// <summary>
        /// Seta a quantidade de páginas de preview que será mostrada na UI de impressão.
        /// </summary>
        /// <param name="sender">Objeto que disparou a tarefa.</param>
        /// <param name="e">Encapsulamento de parâmetros do evento.</param>
        private void PrintDic_Paginate(Object sender, PaginateEventArgs e)
        {
            //PrintTaskOptions opt = _printTask.Options;
            _printDocument.SetPreviewPageCount(_pages, PreviewPageCountType.Final);
        }

        /// <summary>
        /// Seta a página de preview que será mostrada na UI de impressão.
        /// </summary>
        /// <param name="sender">Objeto que disparou a tarefa.</param>
        /// <param name="e">Encapsulamento de parâmetros do evento.</param>
        private void OnGetPreviewPage(Object sender, GetPreviewPageEventArgs e)
        {
            _printDocument.SetPreviewPage(e.PageNumber, BrailleContent);
        }

        /// <summary>
        /// Após a página ser carregada, realiza-se um processo de quebra do texto em trechos que caibam em uma página.
        /// Os parâmetros estão travados para contem 8 células Braille em uma linha e 7 linhas por página.
        /// Para cada página, renderiza-se um TextBlock com o respectivo texto específico na View.
        /// </summary>
        /// <param name="sender">Objeto que disparou a tarefa.</param>
        /// <param name="e">Encapsulamento de parâmetros do evento.</param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var chars = (double)_text.Count();
            var lines = 7;
            var charsPerLine = 8;
            var charsPerPage = lines * charsPerLine;
            _pages = (int)Math.Ceiling(chars / charsPerPage);

            //Para cada página, renderiza-se um TextBlock na View.
            for (int i = 0; i < _pages; i++)
            {
                TextBlock tb = new TextBlock()
                {
                    //Utiliza-se a fonte Braille AOE presente na pasta de Assets para renderizar o PDF.
                    FontFamily = new FontFamily("../Assets/Fonts/Braille AOE Font.TTF#Braille AOE"),
                    FontSize = 100,
                    TextWrapping = TextWrapping.WrapWholeWords,
                    Margin = new Thickness(20),
                    LineStackingStrategy = LineStackingStrategy.MaxHeight,
                    LineHeight = 150,
                    Name = $"Block{i}",
                    Text = new string(_text.Skip(i * charsPerPage).Take(charsPerPage).ToArray())
                };
                BrailleContent.Children.Add(tb);
            }
        }
    }

}
