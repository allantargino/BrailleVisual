using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedianFilter
{
    public class MedianFilter
    {
        int _window;

        public MedianFilter()
        {
            _window = 3;
        }

        public MedianFilter(int window)
        {
            _window = window;
        }

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
