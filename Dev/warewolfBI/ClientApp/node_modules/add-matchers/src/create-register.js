// modules
var addAsymmetricMatchers = require('./add-asymmetric-matchers');

// public
module.exports = createRegister;

// implementation
function createRegister(frameworks, globals) {
  var adaptersByNumberOfArgs;

  if (globals.expect && globals.expect.extend) {
    adaptersByNumberOfArgs = frameworks.jest.getAdapters(globals);
  } else if (globals.jasmine && globals.jasmine.addMatchers) {
    adaptersByNumberOfArgs = frameworks.jasmineV2.getAdapters(globals);
  } else if (globals.jasmine) {
    adaptersByNumberOfArgs = frameworks.jasmineV1.getAdapters(globals);
  } else {
    throw new Error('jasmine-expect cannot find jest, jasmine v2.x, or jasmine v1.x');
  }

  addMatchers.asymmetric = addAsymmetricMatchers;

  return addMatchers;

  function addMatchers(matchersByName) {
    /* eslint guard-for-in: 0 */
    for (var name in matchersByName) {
      var matcherFunction = matchersByName[name];
      var numberOfArgs = matcherFunction.length;
      var adapter = adaptersByNumberOfArgs[numberOfArgs];
      adapter(name, matcherFunction);
    }
  }
}
