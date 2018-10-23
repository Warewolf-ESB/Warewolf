# AngularSpa

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 1.7.0.

## Development server

Run `ng serve` for a dev server. Navigate to `http://localhost:4200/`. The app will automatically reload if you change any of the source files.

## Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory. Use the `-prod` flag for a production build.

## Running unit tests

Run `ng test` to execute the unit tests via [Karma](https://karma-runner.github.io).

## Running end-to-end tests

Run `ng e2e` to execute the end-to-end tests via [Protractor](http://www.protractortest.org/).

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI README](https://github.com/angular/angular-cli/blob/master/README.md).

Angular CLI has test coverage reporting somewhat built in.

First install the dependencies.

## Testing
$ npm install karma karma-jasmine karma-chrome-launcher karma-jasmine-html-reporter karma-coverage-istanbul-reporter
1
$ npm install karma karma-jasmine karma-chrome-launcher karma-jasmine-html-reporter karma-coverage-istanbul-reporter
Then run ng test.


$ ng test --code-coverage
1
$ ng test --code-coverage
Then run the server that shows you your report.


$ http-server -c-1 -o -p 9875 ./coverage
1
$ http-server -c-1 -o -p 9875 ./coverage
You should see something like this.
