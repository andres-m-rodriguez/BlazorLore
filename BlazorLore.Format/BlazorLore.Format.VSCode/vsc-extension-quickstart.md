# Blazor Formatter Extension Quick Start

## What's in the folder

* `package.json` - this is the manifest file in which you declare your extension and command.
* `src/extension.ts` - this is the main file where you will provide the implementation of your command.

## Get up and running straight away

* Press `F5` to open a new window with your extension loaded.
* Open a `.razor` or `.cshtml` file
* Run the command from the command palette by pressing (`Ctrl+Shift+P` or `Cmd+Shift+P` on Mac) and typing `Blazor Formatter`.

## Make changes

* You can relaunch the extension from the debug toolbar after changing code in `src/extension.ts`.
* You can also reload (`Ctrl+R` or `Cmd+R` on Mac) the VS Code window with your extension to load your changes.

## Run tests

* Open the debug viewlet (`Ctrl+Shift+D` or `Cmd+Shift+D` on Mac) and from the launch configuration dropdown pick `Extension Tests`.
* Press `F5` to run the tests in a new window with your extension loaded.
* See the output of the test result in the debug console.
* Make changes to `src/test/suite/extension.test.ts` or create new test files inside the `test/suite` folder.

## Go further

* [Follow UX guidelines](https://code.visualstudio.com/api/ux-guidelines/overview) to create extensions that seamlessly integrate with VS Code's native interface and patterns.
* [Publish your extension](https://code.visualstudio.com/api/working-with-extensions/publishing-extension) on the VS Code extension marketplace.
* Automate builds by setting up [Continuous Integration](https://code.visualstudio.com/api/working-with-extensions/continuous-integration).