using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Printing;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Printing;


namespace TCC_Eng_Info
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ExportPage : Page
    {
        ///  <summary> 
        /// use PrintManage.GetForCurrentView () Gets a PrintManager objects
        /// PrintManage Printer Manager is responsible for the arrangements for the print stream Windows applications
        // You must call PrintManager.GetForCurrentView when using () method to return specific to the current window PrintManager
        ///  </ summary> 
        PrintManager printmgr = PrintManager.GetForCurrentView();

        ///  <summary> 
        /// PrintDocument print stream sent to the printer in a reusable objects
        ///  </ summary> 
        PrintDocument printDoc = null;

        ///  <summary> 
        /// RotateTransform is to rotate the print element, if the device is sideways of the need to rotate 90 °
        ///  </ summary> 
        RotateTransform rottrf = null;

        ///  <summary> 
        /// representation includes content to be printed as well as provide access to the description of how to print the contents of the information The printing operation
        ///  </ summary> 
        PrintTask Task = null;


        private string _text = string.Empty;


        private int Pages { get; set; }

        public string TextContent { get; set; }

        public ExportPage()
        {
            this.InitializeComponent();

            printmgr.PrintTaskRequested += Printmgr_PrintTaskRequested;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _text = ((StringBuilder)e.Parameter).ToString();
            base.OnNavigatedTo(e);
        }


        public ExportPage(string textContent)
        {
            this.InitializeComponent();
            TextContent = textContent;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (printDoc != null)
            {
                printDoc.GetPreviewPage -= OnGetPreviewPage;
                printDoc.Paginate -= PrintDic_Paginate;
                printDoc.AddPages -= PrintDic_AddPages;
            }
            this.printDoc = new PrintDocument();
            // subscribe preview event 
            printDoc.GetPreviewPage += OnGetPreviewPage;
            // print parameters occur Subscribe preview change the direction of events such as the document 
            printDoc.Paginate += PrintDic_Paginate;
            // add a page to handle events 
            printDoc.AddPages += PrintDic_AddPages;

            // display the Print dialog box 
            bool showPrint = await PrintManager.ShowPrintUIAsync();
        }

        private void Printmgr_PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            // Get PrintTaskRequest tasks associated with property from the Request Parameter in
            // After creating the print content and tasks calling the Complete method for printing 
            var deferral = args.Request.GetDeferral();
            // create a print task 
            Task = args.Request.CreatePrintTask(" shopping information - Print single ", OnPrintTaskSourceRequested);
            Task.Completed += PrintTask_Completed;
            deferral.Complete();
        }

        private void PrintTask_Completed(PrintTask Sender, PrintTaskCompletedEventArgs args)
        {
            // print completed 
        }


        private async void OnPrintTaskSourceRequested(PrintTaskSourceRequestedArgs args)
        {
            var def = args.GetDeferral();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    // Set the print source 
                    args.SetSource(printDoc?.DocumentSource);
                });
            def.Complete();
        }

        // add the contents of the printed page 
        private void PrintDic_AddPages(Object Sender, AddPagesEventArgs e)
        {
            // add elements of a page to be printed 
            //printDoc.AddPage(InitialBrailleText);

            for (int i = 0; i < Pages; i++)
            {
                var element = (UIElement)this.FindName($"Block{i}");
                printDoc.AddPage(element);
            }

            // completed to increase the printed page 
            printDoc.AddPagesComplete();
        }

        private void PrintDic_Paginate(Object Sender, PaginateEventArgs e)
        {
            PrintTaskOptions opt = Task.Options;

            // set the preview page of the total number of pages 
            printDoc.SetPreviewPageCount(1, PreviewPageCountType.Final);
        }


        private void OnGetPreviewPage(Object Sender, GetPreviewPageEventArgs e)
        {
            // set to preview page 
            printDoc.SetPreviewPage(e.PageNumber, BrailleContent);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var chars = (double)_text.Count();
            var lines = 7;
            var charsPerLine = 8;
            var charsPerPage = lines * charsPerLine;
            Pages = (int)Math.Ceiling(chars / charsPerPage);

            for (int i = 0; i < Pages; i++)
            {
                TextBlock tb = new TextBlock()
                {
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
