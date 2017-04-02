using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCC_Eng_Info.Models
{
    ///<Summary>
    /// Ferramenta que aplica o filtro de Mediana a uma lista.
    ///</Summary>
    public class MedianFilter
    {
        int _window;

        /// <summary>
        /// Construtor da ferramenta com o valor padrão de window = 3.
        /// </summary>
        public MedianFilter():this(3)
        {
        }

        /// <summary>
        /// Construtor da ferramenta.
        /// </summary>
        /// <param name="window">Tamanho da janela de quantidade de itens a serem analisados ao mesmo tempo pela mediana.</param>
        public MedianFilter(int window)
        {
            _window = window;
        }

        /// <summary>
        /// Filtro que realiza uma suavização na curva dos dados.
        /// </summary>
        /// <param name="input">Lista de valores double a serem suavizados pelo filtro.</param>
        /// <returns></returns>
        public List<double> Filter(List<double> input)
        {
            var output = new List<double>();
            var newInput = new List<double>(input.ToArray());

            for (int i = 0; i < _window - 2; i++)
            {
                newInput.Insert(0, input[0]);
                newInput.Insert(newInput.Count - 1, input[input.Count - 1]);
            }

            for (int i = 0; i < input.Count; i++)
            {
                var elem = Median(newInput.Skip(i).Take(_window).ToList());
                output.Add(elem);
            }

            return output;
        }

        /// <summary>
        /// Método que aplica a mediana sobre um conjunto de dados de uma janela.
        /// </summary>
        /// <param name="input">Lista de dados de entrada do processamento</param>
        /// <returns></returns>
        private double Median(List<double> input)
        {
            if (input.Count == 1) return input[0];

            input.Sort();
            if (input.Count % 2 == 0)
            {
                return input.Sum() / input.Count;
            }
            else
            {
                var mid = input.Count / 2;
                return input[mid];
            }
        }
    }
}
