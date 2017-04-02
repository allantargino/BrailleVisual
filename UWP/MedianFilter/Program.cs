using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedianFilter
{
    class Program
    {
        static void Main(string[] args)
        {
            var median = new MedianFilter(3);

            var input = new List<double> {
                139.846161,
                125.653854,
                115.769226,
                113.307678,
                196.461548,
                48.730774,
                89.730774,
                85.115356,
                97.807739,
                42.884521,
                107.153931
            };

            //var take1 = input.Take(1).ToList();
            //var output1 = median.Median(take1);
            //var take2 = input.Take(2).ToList();
            //var output2 = median.Median(take2);
            //var take3 = input.Take(3).ToList();
            //var take4 = input.Take(4);
            //var take5 = input.Take(5);

            var output = median.Filter(input);
            var max = output.Max();
        }
    }

}
