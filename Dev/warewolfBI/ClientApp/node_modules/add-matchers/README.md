# add-matchers

[![NPM version](http://img.shields.io/npm/v/add-matchers.svg?style=flat-square)](https://www.npmjs.com/package/add-matchers)
[![npm downloads](https://img.shields.io/npm/dm/add-matchers.svg?style=flat-square)](https://www.npmjs.com/package/add-matchers)
[![Dependency Status](http://img.shields.io/david/JamieMason/add-matchers.svg?style=flat-square)](https://david-dm.org/JamieMason/add-matchers)
[![Join the chat at https://gitter.im/JamieMason/Jasmine-Matchers](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/JamieMason/Jasmine-Matchers)
[![Analytics](https://ga-beacon.appspot.com/UA-45466560-5/add-matchers?flat&useReferer)](https://github.com/igrigorik/ga-beacon)

|**What**|A JavaScript library to write test Matchers compatible with all versions of [Jest](http://facebook.github.io/jest/) and [Jasmine](https://jasmine.github.io/).|
|---|:---|
|**Why**|The way you write tests in Jasmine and Jest is _extremely_ similar, but the APIs for adding custom matchers vary wildly between Jasmine 1.x, Jasmine 2.x, and Jest. This library aims to remove those obstacles and encourage Developers to share useful matchers they've created with the community.|
|**How**|Developers use the API from this library, which converts them to be compatible with whichever test framework is running.|

## Contents

* [Installation](#installation)
* [API](#api)
* [Writing Matchers](#writing-matchers)
  * [Examples](#examples)
* [Related Projects](#related-projects)

## Installation

```
npm install --save-dev add-matchers
```

Include add-matchers after your test framework but before your tests, and register your matchers before your tests as well.

## API

```
var addMatchers = require('add-matchers');

addMatchers({
  toBeFoo: function() {},
  toBeBar: function() {}
});

// expect('foo').toBeFoo();
// expect('bar').toBeBar();

addMatchers.asymmetric({
  foo: function() {},
  bar: function() {}
});

// expect({ key: 'foo', prop: 'bar' }).toEqual({
//   key: any.foo(),
//   prop: any.bar()
// });
```

## Writing Matchers

The argument passed to `expect` is always the last argument passed to your Matcher, with any other arguments appearing before it in the order they were supplied.

This means that, in the case of `expect(received).toBeAwesome(arg1, arg2, arg3)`, your function will be called with `fn(arg1, arg2, arg3, received)`.

Arguments are ordered in this way to support [partial application](http://ejohn.org/blog/partial-functions-in-javascript/) and increase re-use of matchers.

### Examples

If we wanted to use the following Matchers in our tests;

```js
// matcher with 0 arguments
expect(4).toBeEvenNumber();

// matcher with 1 argument
expect({}).toBeOfType('Object');

// matcher with Many arguments
expect([100, 14, 15, 2]).toContainItems(2, 15, 100);
```

We would create them as follows;

```js
var addMatchers = require('add-matchers');

addMatchers({
  // matcher with 0 arguments
  toBeEvenNumber: function(received) {
    // received : 4
    return received % 2 === 0;
  },
  // matcher with 1 argument
  toBeOfType: function(type, received) {
    // type     : 'Object'
    // received : {}
    return Object.prototype.toString.call(received) === '[object ' + type + ']';
  },
  // matcher with many arguments
  toContainItems: function(arg1, arg2, arg3, received) {
    // arg1     : 2
    // arg2     : 15
    // arg3     : 100
    // received : [100, 14, 15, 2]
    return (
      received.indexOf(arg1) !== -1 &&
      received.indexOf(arg2) !== -1 &&
      received.indexOf(arg3) !== -1
    );
  }
});
```

For more examples, see [Jasmine Matchers](https://github.com/JamieMason/Jasmine-Matchers/tree/master/src) which is built using this library.

## Related Projects

+ [Jasmine Matchers](https://github.com/JamieMason/Jasmine-Matchers): A huge library of test assertion matchers to improve readability.
+ [karma-benchmark](https://github.com/JamieMason/karma-benchmark): A Karma plugin to run [Benchmark.js](https://benchmarkjs.com/) over multiple browsers, with CI compatible output.
+ [karma-jasmine-matchers](https://github.com/JamieMason/karma-jasmine-matchers): A Karma plugin to inject Jasmine Matchers.
+ [karma-nested-reporter](https://github.com/JamieMason/karma-nested-reporter): Easy to read test output with nested `describe` and `it` blocks.
