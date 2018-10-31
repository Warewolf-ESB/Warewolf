module.exports = {
  getAdapters: function (globals) {
    return {
      1: createFactory(adapterForActual),
      2: createFactory(adapterForActualAndExpected),
      3: createFactory(adapterForActualAndTwoExpected),
      4: createFactory(adapterForKeyAndActualAndTwoExpected)
    };

    function createFactory(adapter) {
      return function (name, matcher) {
        var matchersByName = {};
        matchersByName[name] = adapter(name, matcher);
        globals.beforeEach(function () {
          this.addMatchers(matchersByName);
        });
        return matchersByName;
      };
    }

    function adapterForActual(name, matcher) {
      return function (optionalMessage) {
        return matcher(this.actual, optionalMessage);
      };
    }

    function adapterForActualAndExpected(name, matcher) {
      return function (expected, optionalMessage) {
        return matcher(expected, this.actual, optionalMessage);
      };
    }

    function adapterForActualAndTwoExpected(name, matcher) {
      return function (expected1, expected2, optionalMessage) {
        return matcher(expected1, expected2, this.actual, optionalMessage);
      };
    }

    function adapterForKeyAndActualAndTwoExpected(name, matcher) {
      return function (key, expected1, expected2, optionalMessage) {
        return matcher(key, expected1, expected2, this.actual, optionalMessage);
      };
    }
  }
};
