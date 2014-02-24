ScriptCs.Engine.Mono
====================

This is an implementation of a scriptcs scripting engine using *Mono.CSharp* and *NRefactory*. Built entirely on Windows, not tested yet on other platforms. It is implemented as a **scriptcs module**.

## Running this

1. Build from source. Package restore is enabled, but you will need the [scriptcs nightly build](https://www.myget.org/gallery/scriptcsnightly).
2. Install as scriptcs module. Eiether create a local nuget package and install `scriptcs -install [packagename] -g` or use latest version of scriptcs (0.9.0). In the latter case, simply copy the binaries to `/bin` folder relative to your script but remember to run with `-debug` flag.
3. Run. For example, create a `test.csx`:
      
        public class Test {
            public string Name {get; set;}
        }
    
        var x = new Test();
        x.Name = "xxx";
    
        Console.WriteLine(x.Name);
        //Console.WriteLine(Env.ScriptArgs[0]); //uncomment if you pass args
        Console.WriteLine("Hello");

`scriptcs test.csx -modules mono -debug`

For REPL, omit the file name:
`scriptcs -modules mono -debug`


## What is currently supported

 - execute scripts
 - execute scripts which have a combination of classes and statements, however all classes have to be defined *before* statements
 - supports require/scriptargs and script packs in general
 - supports REPL without some features (multiline didn't work yet, some issues with dump out and serialization)
 - #r to GAC (not testsed against path)

## What is not supported

 - loose global functions
 - statements mixed with classes (has to be first classes, then statements)
 - some small REPL issues
 - probably a few more things :-)

## Contribute

Everyone's welcome!
