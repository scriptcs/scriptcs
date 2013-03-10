Demo of the [Nancy](http://nancyfx.org) framework running on [ScriptCS](https://github.com/scriptcs/scriptcs) - Awesome stuff!

## Usage
Make sure _ScriptCS.exe_ in in your path and then type `scriptcs start.csx` and browse `http://localhost:1234/` in order to have Nancy server you a view.

## Note
Nancy relies heavily on assembly scanning to compose the framework at runtime and to "light up" featured. Due to the fact that ScriptCS compiled into a _dynamic assembly_ and because you cannot use _GetExportedTypes_ on a _dynamic assembly_, you are required to provide a bit of customization in order for Nancy to function correctly.

The following customizations have been included in this demo

* Implemented a custom _IRootPathProvider_ to have the application set its root outside the bin folder, so that views can be located correctly

* Implemented a custom _IRouteDescriptionProvider_ that simply returns an empty string. The default implementation would use assembly scanning

* Use the _Nancy.Bootstrappers.Autofac_ because the default bootstrapper (which is based on _TinyIoC_) uses assembly scanning which is also affected by the limitations of _GetExportedTypes_ on a _dynamic assembly_

* Explicitly set _NancyBootstrapperLocator.Bootstrapper_ to an instance of the custom bootstrapper. Normally you would never assign this, but (again) this is due to the issues with assembly scanning

* Override the _Module_ property, of the bootstrapper, to explicitly tell Nancy which module to use. Modules can't automatically be discovered due to the scanning limitations

Hopefully these things will not be required in later releases of _ScriptCS_, but as it stands, it serves as a great testament on how Nancy can be modified, without changing a single line of code in the framework, to make it run in any environment!

-- The Nancy team