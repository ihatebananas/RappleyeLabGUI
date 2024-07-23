using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GFFFeature
{
    public int n { get; set; }
    public string contig_name { get; set; }
    public string source { get; set; }
    public string feature { get; set; }
    public int start { get; set; }
    public int stop { get; set; }
    public double zeroes { get; set; }
    public string direction { get; set; }
    public string dots { get; set; }
    public string attribute { get; set; }

    public GFFFeature(int Num, string contig_name, string source, string feature, int start, int stop, double zeroes, string direction, string dots, string attribute)
    {
        this.n = Num;
        this.contig_name = contig_name;
        this.source = source;
        this.feature = feature;
        this.start = start;
        this.stop = stop;
        this.zeroes = zeroes;
        this.direction = direction;
        this.dots = dots;
        this.attribute = attribute;
    }
}

