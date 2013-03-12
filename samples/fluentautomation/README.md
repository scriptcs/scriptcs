*Instructions*

- Install nuget somewhere in your path.
- Install scriptcs somewhere in your path.
- Download this gist to a folder.
- From that directory, run 'nuget install FluentAutomation.SeleniumWebDriver -o packages'
- Then run 'scriptcs TestKnockoutJS.csx'

*Comments*

This could be really really good from a testing standpoint. One file per test-case, with no extra bits necessary feels awesome.

Also.. Don't forget to call I.Dispose() at the end of the script or the browser will just hang around after the test. I think we'll do some refactoring to make this a bit smarter in this context, as well as eliminate the need for the reflection in Bootstrap<T>() in FluentAutomation.csx