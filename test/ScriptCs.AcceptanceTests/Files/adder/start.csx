using System.Diagnostics;

var adder = Require<Adder>();

if (adder.Add(3, 4) != 7)
{
	throw new Exception("3 + 4 != 7");
}

if (adder.Add(4, 4) != 8)
{
	throw new Exception("4 + 4 != 8");
}