# Contributing
Curly is an open-source software mangaged under the MIT License. Consider contriuting to Curly to make the framework even better! You can choose to contribute directly to the soruce, or to the documentation.

## Contributing to the Source
To start contributing to Curly's source code, start by forking the [repository](https://github.com/evan-bertis-sample/curly) and cloning it locally. Checkout the [guide](https://docs.github.com/en/get-started/quickstart/contributing-to-projects) by GitHub on how to contribute to open source.

Then, open up Unity Hub, and open up the project using the ```2021.3.24f1``` version of the Unity Editor.

From here, you can make changes to the Curly source code as need be. Once you are finished, consider opening a pull request to integrate your improvements into the main distribution of Curly. Checkout this [guide](https://opensource.guide/how-to-contribute/) on how and why to contribute to open source. 

## Contributing to the Documentation
There are two methods to contribute to Curly's documentation:

1. Using the "Help us improve this page!" link at the bottom left of every page. This should be used for quick edits in content.
2. Cloning the [Curly repository](https://github.com/evan-bertis-sample/curly) and making changes as you would as described in [Contributing to the Source](#contributing-to-the-source). This is should be used for adding new pages and editing the routing structure of the site.

If you want to make drastic changes to Curly's documentation, consider using method 2. Curly's documentation is built using ```Vuepress```, and leverages markdown documents to write quick documentation. To view your edits locally (after following the steps to fork the repo and clone it locally), you can run these commands from the root Curly directory:

```sh
cd docs # Change into the documentation directory
npm install # Install all node modules
npm run docs:dev # Host a local version of the documentation
```

The steps above assume that you have ```Node.js``` installed.

After making your edits, open a pull request, and admins will build and deploy the page if deemed suitable.
