#load dep.csx

using System.IO;

var result = Add(20, 22);

File.WriteAllText("result.txt", result.ToString());