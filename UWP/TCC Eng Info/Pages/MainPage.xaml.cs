using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TCC_Eng_Info.Models;
using System.Text;
using Windows.Foundation;
using System.Linq;
using Windows.UI.Xaml.Media;

namespace TCC_Eng_Info
{
    /// <summary>
    /// Página principal do aplicativo tradutor.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer _recogTimer = null;
        private InkRecognizerContainer _inkRecognizerContainer = null;
        private SerialOutput _serialDevice = null;
        private StringBuilder _strBuilder = null;


        /// <summary>
        /// Construtor da página principal do aplicativo tradutor.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            // Escolhe-se quais dispositivos deseja-se permitir como entrada.
            inkCanvas.InkPresenter.InputDeviceTypes =
                CoreInputDeviceTypes.Mouse |
                CoreInputDeviceTypes.Touch;

            // Inicializa-se os atributos de desenho do componente usado.
            var attributes = new InkDrawingAttributes()
            {
                Color = Colors.Blue,
                FitToCurve = true,
                IgnorePressure = true
            };
            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(attributes);

            // Inicializa-se os eventos de clique.
            //RecognizeButton.Click += RecognizeButton_Click;
            ClearButton.Click += ClearButton_ClickAsync;
            EnableSerialButton.Click += EnableSerialButton_Click;
            ExportPDFButton.Click += ExportPDFButton_Click;

            // Evento disparado quando o usuário para de escrever na tela.
            inkCanvas.InkPresenter.StrokesCollected += inkCanvas_StrokesCollected;
            // Evento disparado quando o primeiro stroke é detectado.
            inkCanvas.InkPresenter.StrokeInput.StrokeStarted += inkCanvas_StrokeStarted;

            // Cria o container usado no reconhecimento manuscrito.
            _inkRecognizerContainer = new InkRecognizerContainer();

            // Timer de controle que gerencia o reconhecimento dinâmico.
            _recogTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 1)
            };
            _recogTimer.Tick += recoTimer_Tick;

            // Estrutura para guardar o texto da sessão de captura da linguagem natural.
            _strBuilder = new StringBuilder();
        }

        /// <summary>
        /// Navega até a página de exportação de uma sessão de texto Braille.
        /// </summary>
        /// <param name="sender">Objeto que disparou a tarefa.</param>
        /// <param name="e">Encapsulamento de parâmetros do evento.</param>
        private void ExportPDFButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ExportPage), _strBuilder);
        }

        /// <summary>
        /// Botão que limpa os strokes do Canvas, as células Braille e o texto presente no dispositivo conectado pela Serial.
        /// </summary>
        /// <param name="sender">Objeto que disparou a tarefa.</param>
        /// <param name="e">Encapsulamento de parâmetros do evento.</param>
        private async void ClearButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            BraillePanel.Children.Clear();
            var tbs = inkGrid.Children.OfType<TextBlock>().ToList();
            foreach (var tb in tbs)
            {
                inkGrid.Children.Remove(tb);
            }
            RecognitionResult.Text = string.Empty;

            if (_serialDevice != null)
                await _serialDevice.SendTextToDevice('\n'.ToString());
        }

        /// <summary>
        /// Se o respectivo botão de toggle possuir o valor verdadeiro, os dados do reconhecimento são enviados pela porta Serial.
        /// </summary>
        /// <param name="sender">Objeto que disparou a tarefa.</param>
        /// <param name="e">Encapsulamento de parâmetros do evento.</param>
        private void EnableSerialButton_Click(object sender, RoutedEventArgs e)
        {
            if (EnableSerialButton.IsChecked == true)
            {
                _serialDevice = new SerialOutput("COM4");
            }
            else
            {
                _serialDevice = null;
            }
        }

        /// <summary>
        /// Handler para o evento de tick do timer de controle.
        /// </summary>
        /// <param name="sender">Objeto que disparou a tarefa.</param>
        /// <param name="e">Encapsulamento de parâmetros do evento.</param>
        private void recoTimer_Tick(object sender, object e)
        {
            Recognize_Tick();
        }

        /// <summary>
        /// O evento é disparado quando o usuário inicia a escrita (usando o dedo ou o mouse na tela).
        /// O timer de controle é parado para permitir que o usuário escreva de maneira contínua.
        /// </summary>
        /// <param name="sender">Objeto que disparou a tarefa.</param>
        /// <param name="args">Encapsulamento de parâmetros do evento.</param>
        private void inkCanvas_StrokeStarted(InkStrokeInput sender, PointerEventArgs args)
        {
            _recogTimer.Stop();
        }

        /// <summary>
        /// O evento é disparado sempre que o usuário para de escrever (soltando o dedo ou o mouse da tela).
        /// Após 1 segundo ou caso não haja nenhuma entrada do usuário, o reconhecimento será iniciado.
        /// </summary>
        /// <param name="sender">Objeto que disparou a tarefa.</param>
        /// <param name="args">Encapsulamento de parâmetros do evento.</param>
        private void inkCanvas_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            _recogTimer.Start();
        }

        /// <summary>
        /// Quando o usuário para de escrever após 1 segundos, o reconhecimento se inicia.
        /// </summary>
        private async void Recognize_Tick()
        {
            // Pega todos os strokes do InkCanvas:
            IReadOnlyList<InkStroke> currentStrokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();

            // Garante que ao menos um stroke está presente:
            if (currentStrokes.Count > 0)
            {
                var positions = GetWordsPositions(currentStrokes, 30);

                if (!(_inkRecognizerContainer == null))
                {
                    // Reconhece todos os strokes presentes no canvas.
                    IReadOnlyList<InkRecognitionResult> recognitionResults =
                        await _inkRecognizerContainer.RecognizeAsync(
                            inkCanvas.InkPresenter.StrokeContainer,
                            InkRecognitionTarget.All);

                    if (recognitionResults.Count > 0)
                    {
                        int i = 0;
                        string text = string.Empty;
                        foreach (var result in recognitionResults)
                        {
                            // Pega-se todos os candidatos de cada resultado reconhecido possível:
                            IReadOnlyList<string> candidates = result.GetTextCandidates();
                            // Admite-se o primeiro de cada um como sendo o correto:
                            text += candidates[0] + " ";

                            TextBlock tb = new TextBlock()
                            {
                                //Utiliza-se a fonte Braille AOE presente na pasta de Assets para renderizar o PDF.
                                FontFamily = new FontFamily("../Assets/Fonts/Braille AOE Font.TTF#Braille AOE"),
                                FontSize = 50,
                                Name = $"Block{i}",
                                Text = candidates[0].ToString().ToLower()
                            };

                            //Adiciona as celas Braille abaixo de cada palavra.
                            inkGrid.Children.Add(tb);
                            PlaceText(tb, positions[i]);
                            i++;
                        }
                        // Mostra o resultado do reconhecimento:
                        RecognitionResult.Text = "Resultado do reconhecimento: " + text;
                        //Envia conteúdo do texto via Serial:
                        if (_serialDevice != null)
                            await _serialDevice.SendTextToDevice(text + '\n');

                        //Adiciona as celas Braille a tela
                        AddBrailleCells(text);

                        _strBuilder.AppendLine(text);

                        // Limpa o Canvas uma vez que o reconhecimento foi finalizado:
                        //inkCanvas.InkPresenter.StrokeContainer.Clear();
                    }
                    else
                    {
                        RecognitionResult.Text = "Nenhum resultado proveniente do reconhecimento!";
                    }
                }
                else
                {
                    Windows.UI.Popups.MessageDialog messageDialog = new Windows.UI.Popups.MessageDialog("Instale alguma engine de reconhecimento!");
                    await messageDialog.ShowAsync();
                }
            }
            else
            {
                RecognitionResult.Text = "Nenhum resultado proveniente do reconhecimento!";
            }

            // Para o timer e aguarda algum stroke ser desenhado na tela novamente.
            _recogTimer.Stop();
        }

        /// <summary>
        /// Posiciona o texto em baixo do conjunto correto de strokes
        /// </summary>
        private void PlaceText(TextBlock tb, Thickness position)
        {
            tb.Margin = position;
        }

        /// <summary>
        /// Obtem as posições que as palavras Braille serão posicionadas no Ink.
        /// </summary>
        /// <param name="strokes">Conjunto de strokes pertencentes a de um Ink usado em reconhecimento de escrita natural.</param>
        /// <param name="marginTop">Margem superior a ser adicionada à posição.</param>
        /// <returns></returns>
        private List<Thickness> GetWordsPositions(IReadOnlyList<InkStroke> strokes, double marginTop)
        {
            var positions = new List<Thickness>
            {
                getThickness(strokes[0], marginTop)
            };

            var filter = new MedianFilter(3);
            var diffs = GetDiffs(strokes);
            var widthMed = filter.Filter(diffs).Max();

            Rect lastMaxStroke;
            bool control = false;
            for (int i = 1; i < strokes.Count; i++)
            {
                var rect1 = strokes[i - 1].BoundingRect;
                var rect2 = strokes[i].BoundingRect;
                rect1.Intersect(rect2);
                if (rect1.IsEmpty)
                {
                    var diff = strokes[i].BoundingRect.Left - strokes[i - 1].BoundingRect.Left;
                    if (diff > widthMed)
                    {
                        positions.Add(getThickness(strokes[i], marginTop));
                    }
                }
                else
                {

                }
            }
            return positions;
        }

        /// <summary>
        /// Função utilizada para pegar a diferença entre letras adjacentes
        /// </summary>
        /// <param name="strokes">Conjunto de strokes pertencentes a de um Ink usado em reconhecimento de escrita natural.</param>
        /// <returns></returns>
        private List<double> GetDiffs(IReadOnlyList<InkStroke> strokes)
        {
            var diffs = new List<double>();

            for (int i = 1; i < strokes.Count; i++)
            {
                var rect1 = strokes[i - 1].BoundingRect;
                var rect2 = strokes[i].BoundingRect;
                rect1.Intersect(rect2);
                if (rect1.IsEmpty)
                {
                    diffs.Add(strokes[i].BoundingRect.Left - strokes[i - 1].BoundingRect.Left);
                }
            }

            return diffs;
        }

        /// <summary>
        /// Função utilizada somente no estudo estatistico de distância entre palavras.
        /// </summary>
        /// <param name="strokes">Conjunto de strokes pertencentes a de um Ink usado em reconhecimento de escrita natural.</param>
        /// <returns></returns>
        private string GetWordsSpaces(IReadOnlyList<InkStroke> strokes)
        {
            var positions = "";

            for (int i = 1; i < strokes.Count; i++)
            {
                var rect1 = strokes[i - 1].BoundingRect;
                var rect2 = strokes[i].BoundingRect;
                rect1.Intersect(rect2);
                if (rect1.IsEmpty)
                {
                    var diff = strokes[i].BoundingRect.Left - strokes[i - 1].BoundingRect.Left;
                    positions += diff + ",";
                }
            }
            return positions;
        }

        /// <summary>
        /// Extrai a posição mais abaixo e a esquerda de um stroke.
        /// </summary>
        /// <param name="stroke">Risco a ser analisado.</param>
        /// <param name="marginTop">Margem superior a ser adicionada à posição.</param>
        /// <returns></returns>
        private Thickness getThickness(InkStroke stroke, double marginTop)
        {
            var left = stroke.BoundingRect.Left;
            var top = stroke.BoundingRect.Top;
            var height = stroke.BoundingRect.Height;

            return new Thickness(left, top + height + marginTop, 0, 0);
        }

        /// <summary>
        /// Adiciona um conjunto de células Braille à View correspondentes a um texto em português.
        /// </summary>
        /// <param name="text">Texto a ser convertido para Braille</param>
        private void AddBrailleCells(string text)
        {
            BraillePanel.Children.Clear();

            var letters = text.ToUpper().ToCharArray();

            foreach (var letter in letters)
            {
                var cell = new BrailleCell(letter);
                BraillePanel.Children.Add(cell);
            }
        }
    }
}