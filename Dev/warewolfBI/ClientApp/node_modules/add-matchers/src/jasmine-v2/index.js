var matcherFactory = require('./matcher-factory');
var memberMatcherFactory = require('./member-matcher-factory');

module.exports = {
  getAdapters: function (globals) {
    return {
      1: createFactory(getAdapter(1)),
      2: createFactory(getAdapter(2)),
      3: createFactory(getAdapter(3)),
      4: createFactory(getAdapter(4))
    };

    function createFactory(adapter) {
      return function (name, matcher) {
        var matchersByName = {};
        matchersByName[name] = adapter(name, matcher);
        globals.beforeEach(function () {
          globals.jasmine.addMatchers(matchersByName);
        });
        return matchersByName;
      };
    }

    function getAdapter(argsCount) {
      return function (name, matcher) {
        var factory = isMemberMatcher(name) ? memberMatcherFactory : matcherFactory;
        return factory[argsCount](name, matcher);
      };
    }

    function isMemberMatcher(name) {
      return name.search(/^toHave/) !== -1;
    }
  }
};
