using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace TCC_Eng_Info.Models
{
    /// <summary>
    /// Representa um dispositivo com capacidade de receber mensagens de texto via porta Serial.
    /// </summary>
    public class SerialOutput
    {
        private SerialDevice _serialDevice = null;
        private DataWriter _dataWriter = null;

        /// <summary>
        /// Construtor que abstraia a inicialização do device COM.
        /// </summary>
        /// <param name="comPort">Porta de comunicação no formato COM. Exemplo: COM1, COM3, COM4.</param>
        /// <example>COM3</example>
        public SerialOutput(string comPort)
        {
            string qFilter = SerialDevice.GetDeviceSelector(comPort);

            Task.Run(async () => {
                DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(qFilter);

                if (devices.Any())
                {
                    string deviceId = devices.First().Id;

                    await OpenPort(deviceId);
                }
            }).Wait();

        }

        /// <summary>
        /// Abre a porta de comunicação serial com o dispositivo instanciado.
        /// </summary>
        /// <param name="deviceId">ID do device relativo ao selector obtido de uma porta COM</param>
        /// <returns>Retorna a operação assíncrona.</returns>
        private async Task OpenPort(string deviceId)
        {
            _serialDevice = await SerialDevice.FromIdAsync(deviceId);

            if (_serialDevice != null)
            {
                _serialDevice.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                _serialDevice.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                _serialDevice.BaudRate = 9600;
                _serialDevice.Parity = SerialParity.None;
                _serialDevice.StopBits = SerialStopBitCount.One;
                _serialDevice.DataBits = 8;
                _serialDevice.Handshake = SerialHandshake.None;
            }
        }

        /// <summary>
        /// Envia um texto pela interface Serial.
        /// </summary>
        /// <param name="text">Texto a ser enviado para o dispositivo pela porta COM.</param>
        /// <returns>Retorna a operação assíncrona.</returns>
        public async Task SendTextToDevice(string text)
        {
            try
            {
                if (_serialDevice != null)
                {
                    _dataWriter = new DataWriter(_serialDevice.OutputStream);
                    await WriteAsync(text);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //Limpa o buffer do DataWriter
                if (_dataWriter != null) 
                {
                    _dataWriter.DetachStream();
                    _dataWriter = null;
                }
            }
        }

        /// <summary>
        /// Escreve a mensagem no buffer do objeto de escrita.
        /// </summary>
        /// <param name="text">Texto a ser enviado para o dispositivo pela porta COM.</param>
        /// <returns>Retorna a operação assíncrona.</returns>
        private async Task WriteAsync(string text)
        {
            Task<UInt32> storeAsyncTask;

            if (text.Length != 0)
            {
                _dataWriter.WriteString(text);
                storeAsyncTask = _dataWriter.StoreAsync().AsTask(); 
                UInt32 bytesWritten = await storeAsyncTask; 
                //if (bytesWritten > 0)
                //{
                //}
            }
        }
    }
}
