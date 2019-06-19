﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace OctreeQuantization
{
    public class Program
    {
        static void Main(string[] args)
        {
            var bitmap = new Bitmap("colorful-colourful-ink-1065714.jpg");
            var quantized = Quantize(bitmap, 128);
        }

        private static Bitmap Quantize(Bitmap bitmap, int colourCount)
        {
            var quantizer = new PaletteQuantizer();
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var colour = bitmap.GetPixel(x, y);
                    quantizer.AddColour(colour);
                }
            }
            quantizer.Quantize(colourCount);
            var ret = new Bitmap(bitmap.Width, bitmap.Height);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var colour = quantizer.GetQuantizedColour(bitmap.GetPixel(x, y));
                    ret.SetPixel(x, y, colour);
                }
            }
            return ret;
        }

        private class PaletteQuantizer
        {
            private readonly Node Root;
            private IDictionary<int, List<Node>> levelNodes;

            public PaletteQuantizer()
            {
                Root = new Node(this);
                levelNodes = new Dictionary<int, List<Node>>();
                for (int i = 0; i < 8; i++)
                {
                    levelNodes[i] = new List<Node>();
                }
            }

            public void AddColour(Color colour)
            {
                Root.AddColour(colour, 0);
            }

            public void AddLevelNode(Node node, int level)
            {
                levelNodes[level].Add(node);
            }

            public Color GetQuantizedColour(Color colour)
            {
                return Root.GetColour(colour, 0);
            }

            public void Quantize(int colourCount)
            {
                var nodesToRemove = levelNodes[7].Count - colourCount;
                int level = 6;
                var toBreak = false;
                while (level >= 0 && nodesToRemove > 0)
                {
                    var leaves = levelNodes[level]
                        .Where(n => n.ChildrenCount - 1 <= nodesToRemove)
                        .OrderBy(n => n.ChildrenCount);
                    foreach (var leaf in leaves)
                    {
                        if (leaf.ChildrenCount > nodesToRemove)
                        {
                            toBreak = true;
                            continue;
                        }
                        nodesToRemove -= (leaf.ChildrenCount - 1);
                        leaf.Merge();
                        if (nodesToRemove <= 0)
                        {
                            break;
                        }
                    }
                    levelNodes.Remove(level+1);
                    level--;
                    if (toBreak)
                    {
                        break;
                    }
                }
            }
        }

        private class Node
        {
            private readonly PaletteQuantizer parent;
            private Node[] Children = new Node[8];
            private Color Colour { get; set; }
            private int Count { get; set; }

            public int ChildrenCount => Children.Count(c => c != null);

            public Node(PaletteQuantizer parent)
            {
                this.parent = parent;
            }

            public void AddColour(Color colour, int level)
            {
                if (level < 8)
                {
                    var index = GetIndex(colour, level);
                    if (Children[index] == null)
                    {
                        var newNode = new Node(parent);
                        Children[index] = newNode;
                        parent.AddLevelNode(newNode, level);
                    }
                    Children[index].AddColour(colour, level + 1);
                }
                else
                {
                    Colour = colour;
                    Count++;
                }
            }

            public Color GetColour(Color colour, int level)
            {
                if (ChildrenCount == 0)
                {
                    return Colour;
                }
                else
                {
                    var index = GetIndex(colour, level);
                    return Children[index].GetColour(colour, level + 1);
                }
            }

            private byte GetIndex(Color colour, int level)
            {
                byte ret = 0;
                var mask = Convert.ToByte(0b10000000 >> level);
                if ((colour.R & mask) != 0)
                {
                    ret |= 0b100;
                }
                if ((colour.G & mask) != 0)
                {
                    ret |= 0b010;
                }
                if ((colour.B & mask) != 0)
                {
                    ret |= 0b001;
                }
                return ret;
            }

            public void Merge()
            {
                Colour = Average(Children.Where(c => c != null).Select(c => new Tuple<Color, int>(c.Colour, c.Count)));
                Count = Children.Sum(c => c?.Count ?? 0);
                Children = new Node[8];
            }
        }

        //private class PaletteQuantizer
        //{
        //    private Node Root = new Node();

        //    public void AddColour(Color colour)
        //    {
        //        Root.AddColour(colour, 0);
        //    }

        //    public void Quantize(int colourCount)
        //    {
        //        var leafCount = Root.GetLeafCount();
        //        var numberToRemove = leafCount - colourCount;

        //        var level = 7;
        //        while (level > 0 && numberToRemove > 0)
        //        {
        //            var leaves = Root.GetNodesOfLevel(level)
        //                .Where(n => n.ChildrenCount <= numberToRemove)
        //                .OrderBy(n => n.ChildrenCount);
        //            foreach (var leaf in leaves)
        //            {
        //                numberToRemove -= leaf.Merge();
        //                if (numberToRemove <= 0)
        //                {
        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    public Color GetQuantizedColour(Color oldColour)
        //    {
        //        return Root.GetColour(oldColour, 0);
        //    }
        //}

        //private class Node
        //{
        //    private Node[] Children = new Node[8];
        //    private Color Colour { get; set; }
        //    private int Count { get; set; }

        //    public int ChildrenCount => Children.Count(c => c != null);

        //    public void AddColour(Color colour, int level)
        //    {
        //        if (level < 8)
        //        {
        //            var index = GetIndex(colour, level);
        //            if (Children[index] == null)
        //            {
        //                Children[index] = new Node();
        //            }
        //            Children[index].AddColour(colour, level + 1);
        //        }
        //        else
        //        {
        //            Colour = colour;
        //            Count++;
        //        }
        //    }

        //    public Color GetColour(Color colour, int level)
        //    {
        //        if (ChildrenCount == 0)
        //        {
        //            return Colour;
        //        }
        //        else
        //        {
        //            var index = GetIndex(colour, level);
        //            return Children[index].GetColour(colour, level + 1);
        //        }
        //    }

        //    private byte GetIndex(Color colour, int level)
        //    {
        //        byte ret = 0;
        //        var mask = Convert.ToByte(0b10000000 >> level);
        //        if ((colour.R & mask) != 0)
        //        {
        //            ret |= 0b100;
        //        }
        //        if ((colour.G & mask) != 0)
        //        {
        //            ret |= 0b010;
        //        }
        //        if ((colour.B & mask) != 0)
        //        {
        //            ret |= 0b001;
        //        }
        //        return ret;
        //    }

        //    public int GetLeafCount()
        //    {
        //        if (ChildrenCount == 0)
        //        {
        //            return 1;
        //        }
        //        else
        //        {
        //            return Children.Sum(c => c?.GetLeafCount() ?? 0);
        //        }
        //    }

        //    public IEnumerable<Node> GetNodesOfLevel(int level)
        //    {
        //        if (level == 1)
        //        {
        //            return Children.Where(c => c != null);
        //        }
        //        else
        //        {
        //            return Children.Where(c => c != null).SelectMany(c => c.GetNodesOfLevel(level - 1));
        //        }
        //    }

        //    public int Merge()
        //    {
        //        Colour = Average(Children.Where(c => c != null).Select(c => new Tuple<Color, int>(c.Colour, c.Count)));
        //        Count = Children.Sum(c => c?.Count ?? 0);
        //        var ret = ChildrenCount - 1;
        //        Children = new Node[8];
        //        return ret;
        //    }
        //}

        private static Color Average(IEnumerable<Tuple<Color, int>> colours)
        {
            var totals = colours.Sum(c => c.Item2);
            return Color.FromArgb(
                alpha: 255,
                red: colours.Sum(c => c.Item1.R * c.Item2) / totals,
                green: colours.Sum(c => c.Item1.G * c.Item2) / totals,
                blue: colours.Sum(c => c.Item1.B * c.Item2) / totals);
        }
    }
}
