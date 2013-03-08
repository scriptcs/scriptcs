using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugSymbols
{
    class Program
    {
        static void Main(string[] args)
        {
            var filePath = args[0];
            var fileDir = Path.GetDirectoryName(filePath);
            fileDir = Path.IsPathRooted(filePath) ? fileDir : Path.Combine(Environment.CurrentDirectory, fileDir);
            var fileName = Path.GetFileName(filePath);
            var nameWithoutExtension = fileName.Replace(Path.GetExtension(fileName), string.Empty);
            var outputName = nameWithoutExtension + ".exe";
            var outputPath = Path.Combine(fileDir, outputName);
            var pdbPath = Path.Combine(fileDir, nameWithoutExtension + ".pdb");

            var code = File.ReadAllText(filePath);

            var tree = SyntaxTree.ParseText(code);

            var compilation = Compilation.Create("DerivedClass")
                             .WithOptions(new CompilationOptions(OutputKind.ConsoleApplication))
                             .AddSyntaxTrees(tree)
                             .AddReferences(
                                 MetadataReference.CreateAssemblyReference("mscorlib"),
                                 MetadataReference.CreateAssemblyReference("System"),
                                 MetadataReference.CreateAssemblyReference("System.Core"));

            // need to add assemblies
            EmitResult result;

            using (FileStream outputStream = new FileStream(outputPath, FileMode.OpenOrCreate))
            using (FileStream pdbStream = new FileStream(pdbPath, FileMode.OpenOrCreate))
            {
                result = compilation.Emit(outputStream, outputName, pdbPath, pdbStream);
            }

            if (result.Success)
            {
                Console.WriteLine("Compilation successful");
                Console.WriteLine(string.Format("Output .exe at {0}", outputPath));
                Console.WriteLine(string.Format("Output .pdb at {0}", pdbPath));
            }
            else
            {
                Console.WriteLine("Compilation failed");
                foreach (var error in result.Diagnostics) 
                {
                    Console.WriteLine(error);
                }
            }

            Console.ReadKey();
        }
    }
}