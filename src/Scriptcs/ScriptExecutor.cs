﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roslyn.Scripting.CSharp;
using Scriptcs.Contracts;

namespace Scriptcs
{
    public class ScriptExecutor : IScriptExecutor
    {
        private readonly IFileSystem _fileSystem;

        [ImportingConstructor]
        public ScriptExecutor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public void Execute(string script, IEnumerable<string> paths, IEnumerable<IScriptcsRecipe> recipes)
        {
            var engine = new ScriptEngine();
            engine.AddReference("System");
            engine.AddReference("System.Core");
            var bin = _fileSystem.CurrentDirectory + @"\bin";
            engine.BaseDirectory = bin;
            
            if (!_fileSystem.DirectoryExists(bin))
                _fileSystem.CreateDirectory(bin);

            foreach (var file in paths)
            {
                var destFile = bin + @"\" + Path.GetFileName(file);
                var sourceFileLastWriteTime = _fileSystem.GetLastWriteTime(file);
                var destFileLastWriteTime = _fileSystem.GetLastWriteTime(destFile);
                if (sourceFileLastWriteTime != destFileLastWriteTime)
                    _fileSystem.Copy(file, destFile,true);
    
                engine.AddReference(destFile);
            }
 
            var session = engine.CreateSession();
            var csx = _fileSystem.ReadFile(_fileSystem.CurrentDirectory + @"\" + script);
            session.Execute(csx);
            
        }

    }
}
