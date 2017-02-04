using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Devices.SerialCommunication;
using Windows.Devices.Enumeration;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Threading;
using Windows.Storage.Streams;
using System.Linq;

namespace TCC_Eng_Info
{
    /// <summary>
    /// Página principal do aplicativo tradutor
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DispatcherTimer recoTimer;
        InkRecognizerContainer inkRecognizerContainer;

        private CancellationTokenSource ReadCancellationTokenSource;
        private SerialDevice serialPort = null;
        DataWriter dataWriteObject = null;

        //bool state = true;

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
            SentSerialButton.Click += SentSerialButton_Click;

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
            inkRecognizerContainer = new InkRecognizerContainer();

            // Timer to manage dynamic recognition.
            recoTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 1)
            };
            recoTimer.Tick += recoTimer_Tick;

            initializeSerial();
        }

        private async void ClearButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            BraillePanel.Children.Clear();
            RecognitionResult.Text = string.Empty;
            await sendToPort('\n'.ToString());
        }

        private async void initializeSerial()
        {
            string qFilter = SerialDevice.GetDeviceSelector("COM4");
            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(qFilter);

            if (devices.Any())
            {
                string deviceId = devices.First().Id;

                await OpenPort(deviceId);
            }
        }


        private async void SentSerialButton_Click(object sender, RoutedEventArgs e)
        {
            //if (state)
            //{
            //    await sendToPort("no" + '\n');
            //    state = false;
            //}
            //else
            //{
            //    await sendToPort("yes" + '\n');
            //    state = true;
            //}

        }

        private async Task OpenPort(string deviceId)
        {
            serialPort = await SerialDevice.FromIdAsync(deviceId);

            if (serialPort != null)
            {
                serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                serialPort.BaudRate = 9600;
                serialPort.Parity = SerialParity.None;
                serialPort.StopBits = SerialStopBitCount.One;
                serialPort.DataBits = 8;
                serialPort.Handshake = SerialHandshake.None;
            }
        }


        private async Task sendToPort(string sometext)
        {
            try
            {
                if (serialPort != null)
                {
                    dataWriteObject = new DataWriter(serialPort.OutputStream);

                    await WriteAsync(sometext);
                }
                else { }
            }
            catch (Exception ex)
            {
                //txtStatus.Text = ex.Message;
            }
            finally
            {
                if (dataWriteObject != null)   // Cleanup once complete
                {
                    dataWriteObject.DetachStream();
                    dataWriteObject = null;
                }
            }
        }



        private async Task WriteAsync(string text2write)
        {
            Task<UInt32> storeAsyncTask;

            if (text2write.Length != 0)
            {
                dataWriteObject.WriteString(text2write);

                storeAsyncTask = dataWriteObject.StoreAsync().AsTask();  // Create a task object

                UInt32 bytesWritten = await storeAsyncTask;   // Launch the task and wait
                if (bytesWritten > 0)
                {
                    //txtStatus.Text = bytesWritten + " bytes written at " + DateTime.Now.ToString(System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.LongTimePattern);
                }
            }
            else { }
        }

        // Faz o tratamento do evento de clique e inicia o reconhecimento:
        //private async void RecognizeButton_Click(object sender, RoutedEventArgs e)
        //{
        //    // Pega todos os strokes do InkCanvas:
        //    IReadOnlyList<InkStroke> currentStrokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();

        //    if (currentStrokes.Count > 0)
        //    {
        //        // Cria o container usado no reconhecimento manuscrito:
        //        InkRecognizerContainer inkRecognizerContainer =
        //            new InkRecognizerContainer();

        //        if (!(inkRecognizerContainer == null))
        //        {
        //            // Reconhece todos os strokes presentes no canvas.
        //            IReadOnlyList<InkRecognitionResult> recognitionResults =
        //                await inkRecognizerContainer.RecognizeAsync(
        //                    inkCanvas.InkPresenter.StrokeContainer,
        //                    InkRecognitionTarget.All);
        //            if (recognitionResults.Count > 0)
        //            {
        //                string text = string.Empty;
        //                foreach (var result in recognitionResults)
        //                {
        //                    // Pega-se todos os candidatos de cada resultado reconhecido possível:
        //                    IReadOnlyList<string> candidates = result.GetTextCandidates();
        //                    // Admite-se o primeiro de cada um como sendo o correto:
        //                    text += candidates[0] + " ";
        //                }
        //                // Mostra o resultado do reconhecimento:
        //                RecognitionResult.Text = "Resultado do reconhecimento: " + text;
        //                AddBrailleCells(text);
        //                // Limpa o Canvas uma vez que o reconhecimento foi finalizado:
        //                //inkCanvas.InkPresenter.StrokeContainer.Clear();
        //            }
        //            else
        //            {
        //                RecognitionResult.Text = "Nenhum resultado proveniente do reconhecimento!";
        //            }
        //        }
        //        else
        //        {
        //            Windows.UI.Popups.MessageDialog messageDialog = new Windows.UI.Popups.MessageDialog("Instale alguma engine de reconhecimento!");
        //            await messageDialog.ShowAsync();
        //        }
        //    }
        //    else
        //    {
        //        RecognitionResult.Text = "Nenhum resultado proveniente do reconhecimento!";
        //    }
        //}


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
            recoTimer.Stop();
        }

        // Handler for the InkPresenter StrokesCollected event.
        // Start the recognition timer when the user stops inking by
        // lifting their pen or finger, or releasing the mouse button.
        // After one second of no ink input, recognition is initiated.
        private void inkCanvas_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            recoTimer.Start();
        }

        // Respond to timer Tick and initiate recognition.
        private async void Recognize_Tick()
        {
            // Get all strokes on the InkCanvas.
            IReadOnlyList<InkStroke> currentStrokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();

            // Ensure an ink stroke is present.
            if (currentStrokes.Count > 0)
            {
                // inkRecognizerContainer is null if a recognition engine is not available.
                if (!(inkRecognizerContainer == null))
                {
                    // Recognize all ink strokes on the ink canvas.
                    IReadOnlyList<InkRecognitionResult> recognitionResults =
                        await inkRecognizerContainer.RecognizeAsync(
                            inkCanvas.InkPresenter.StrokeContainer,
                            InkRecognitionTarget.All);
                    // Process and display the recognition results.
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
                        await sendToPort(text + '\n');
                        //Adiciona as celas Braille a tela
                        AddBrailleCells(text);
                        //Clear the ink canvas once recognition is complete.
                        //inkCanvas.InkPresenter.StrokeContainer.Clear();
                    }
                    else
                    {
                        RecognitionResult.Text = "No recognition results.";
                    }
                }
                else
                {
                    Windows.UI.Popups.MessageDialog messageDialog = new Windows.UI.Popups.MessageDialog("You must install handwriting recognition engine.");
                    await messageDialog.ShowAsync();
                }
            }
            else
            {
                RecognitionResult.Text = "No ink strokes to recognize.";
            }

            // Stop the dynamic recognition timer.
            recoTimer.Stop();
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
