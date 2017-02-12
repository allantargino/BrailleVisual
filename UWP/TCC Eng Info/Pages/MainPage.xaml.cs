using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TCC_Eng_Info.Models;
using System.Text;

namespace TCC_Eng_Info
{
    /// <summary>
    /// Página principal do aplicativo tradutor
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer _recogTimer = null;
        private InkRecognizerContainer _inkRecognizerContainer = null;
        private SerialOutput _serialDevice = null;
        private StringBuilder _strBuilder = null;

        public MainPage()
        {
            this.InitializeComponent();

            // Escolhe-se quais dispositivos deseja-se 
            // permitir como entrada:
            inkCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Touch;

            // Inicializa-se os atributos de desenho
            // do componente usado:
            var attributes = new InkDrawingAttributes()
            {
                Color = Colors.Blue,
                FitToCurve = true,
                IgnorePressure = true
            };
            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(attributes);

            // Inicializa-se os eventos de clique:
            //RecognizeButton.Click += RecognizeButton_Click;
            ClearButton.Click += ClearButton_ClickAsync;
            EnableSerialButton.Click += EnableSerialButton_Click;
            ExportPDFButton.Click += ExportPDFButton_Click;

            // Listen for stroke events on the InkPresenter to
            // enable dynamic recognition.
            // StrokesCollected is fired when the user stops inking by
            // lifting their pen or finger, or releasing the mouse button.
            inkCanvas.InkPresenter.StrokesCollected +=
                inkCanvas_StrokesCollected;
            // StrokeStarted is fired when ink input is first detected.
            inkCanvas.InkPresenter.StrokeInput.StrokeStarted +=
                inkCanvas_StrokeStarted;

            // Cria o container usado no reconhecimento manuscrito:
            _inkRecognizerContainer = new InkRecognizerContainer();

            // Timer to manage dynamic recognition.
            _recogTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 1)
            };
            _recogTimer.Tick += recoTimer_Tick;

            _strBuilder = new StringBuilder();

        }

        private void ExportPDFButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ExportPage), _strBuilder);
        }

        private async void ClearButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            BraillePanel.Children.Clear();
            RecognitionResult.Text = string.Empty;

            if (_serialDevice != null)
                await _serialDevice.SendTextToDevice('\n'.ToString());
        }
        

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

        // Handler for the timer tick event calls the recognition function.
        private void recoTimer_Tick(object sender, object e)
        {
            Recognize_Tick();
        }

        // Handler for the InkPresenter StrokeStarted event.
        // If a new stroke starts before the next timer tick event,
        // stop the timer as the new stroke is likely the continuation
        // of a single handwriting entry.
        private void inkCanvas_StrokeStarted(InkStrokeInput sender, PointerEventArgs args)
        {
            _recogTimer.Stop();
        }

        // Handler for the InkPresenter StrokesCollected event.
        // Start the recognition timer when the user stops inking by
        // lifting their pen or finger, or releasing the mouse button.
        // After one second of no ink input, recognition is initiated.
        private void inkCanvas_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            _recogTimer.Start();
        }

        // Respond to timer Tick and initiate recognition.
        private async void Recognize_Tick()
        {
            // Pega todos os strokes do InkCanvas:
            IReadOnlyList<InkStroke> currentStrokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();

            // Garante que ao menos um stroke está presente:
            if (currentStrokes.Count > 0)
            {
                if (!(_inkRecognizerContainer == null))
                {
                    // Reconhece todos os strokes presentes no canvas.
                    IReadOnlyList<InkRecognitionResult> recognitionResults =
                        await _inkRecognizerContainer.RecognizeAsync(
                            inkCanvas.InkPresenter.StrokeContainer,
                            InkRecognitionTarget.All);

                    if (recognitionResults.Count > 0)
                    {
                        string text = string.Empty;
                        foreach (var result in recognitionResults)
                        {
                            // Pega-se todos os candidatos de cada resultado reconhecido possível:
                            IReadOnlyList<string> candidates = result.GetTextCandidates();
                            // Admite-se o primeiro de cada um como sendo o correto:
                            text += candidates[0] + " ";
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