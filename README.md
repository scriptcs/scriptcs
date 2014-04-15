ScriptCs.Engine.Mono
====================

This is an implementation of a scriptcs scripting engine using *Mono.CSharp* and *NRefactory*. Built entirely on Windows, not tested yet on other platforms. It is implemented as a **scriptcs module**.

## Running scriptcs on Mono

1. Build from source. Package restore is enabled.
2. Install as scriptcs module. Either create a local nuget package and install `scriptcs -install [packagename] -g` or, if you are using at least scriptcs 0.9.0, simply copy the binaries to `/bin` folder relative to your script but remember to run with `-debug` flag.
3. Run. For example, create a `test.csx`:
      
        public class Test {
            public string Name {get; set;}
        }
    
        var x = new Test();
        x.Name = "xxx";
    
        Console.WriteLine(x.Name);
        //Console.WriteLine(Env.ScriptArgs[0]); //uncomment if you pass args
        Console.WriteLine("Hello");

`mono scriptcs test.csx -modules mono -debug`

For REPL, omit the file name and pass a `-repl` flag:
`mono scriptcs -modules mono -debug -repl`

## Use REPL in Sublime Text 

1. Install SublimeREPL package from package control

2. Install scriptcs package from package control

3. go to `~/.config/sublime-text-2/Packages/SublimeREPL/config/ScriptCS` and open `Main.sublime-menu` file. IF you are in OSX, the location of that file is `~/Library/Application Support/Sublime Text 3/Packages/SublimeREPL/config/ScriptCS`

4. Update the "linux" (or "osx") path in that file to `"linux": ["mono", "PATH/TO/YOUR/scriptcs.exe", "-modules", "mono", "-repl"]`

5. Profit. Open Sublime > Tools > SublimeREPL > ScriptCS to start the REPL

![Alt text](https://pbs.twimg.com/media/Bk7q3M_CcAA2id-.png:large "REPL on Mono")

## What is currently supported

 - execute scripts
 - execute scripts which have a combination of classes and statements, however all classes have to be defined *before* statements
 - supports require/scriptargs and script packs in general
 - supports REPL without some features (multiline didn't work yet, some issues with dump out and serialization)
 - #r to GAC (not testsed against path)

## What is not supported

 - loose global functions
 - statements mixed with classes (you have to first declare classes, then use statements)
 - some small REPL issues
 - probably a few more things :-)

## Contribute

Everyone's welcome!
