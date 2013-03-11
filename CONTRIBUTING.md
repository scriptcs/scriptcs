# Contributing

Before submitting a feature or substantial code contribution please discuss it with the team in our [discussion group](https://groups.google.com/forum/?fromgroups#!forum/scriptcs).

Also please see the discussion [here] on which things are open for contribution (https://github.com/scriptcs/scriptcs/issues/79)

## Workflow

* We use the **dev** branch for active development. All pull requests should be made to this branch.
* The master branch is only updated via pull requests from the dev branch.
* Tests need to be provided for every bug/feature that is completed.
* Please do not submit a PR unless without running all unit tests. If any tests are failing in the PR they will be rejected.

## Issue Management

We use some tags to manage issue priority.

* **P1**: This is a big deal. It's either really awesome, terribly broken, or it's holding other awesome things up.
* **P2**: This is kind of important and we're really looking forward to doing it, but there are more important things that need done first. 
* **P3**: We'd like to see these get implemented, but we're just not sure when we'll get to them. Want to take a shot at it? Go for it!

If the issue isn't marked with one of the above tags it's either still under discussion or not something we think we'll get to.

## Code Style

* Indent with spaces (4) instead of tabs.
* Prefix private instance field names with an underscore and be camel-cased. The `this` keyword should be avoided.
* Use the `var` keyword unless the inferred type is not obvious.
* Use the C# type aliases for types that have them (ex. `int` instead of `Int32`).
* Use meaningful names (no hungarian notation).
* Unit tests should follow the conventions detailed in [this blog post](http://haacked.com/archive/2012/01/02/structuring-unit-tests.aspx) by Phil Haack.
* Unit tests assertions should be declared using the [Should Assertion Library](https://github.com/erichexter/Should) by Eric Hexter.
