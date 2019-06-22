using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace ColourQuantization
{
    public class Program
    {
        static void Main(string[] args)
        {
            var bitmap = new Bitmap("colorful-colourful-ink-1065714.jpg");
            var quantized = MedianCut.Quantize(bitmap, 15);
        }
    }
}
