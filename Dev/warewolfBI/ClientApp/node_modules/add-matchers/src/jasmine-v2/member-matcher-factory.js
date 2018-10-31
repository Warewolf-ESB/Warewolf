module.exports = {
  2: forKeyAndActual,
  3: forKeyAndActualAndExpected,
  4: forKeyAndActualAndTwoExpected
};

function forKeyAndActual(name, matcher) {
  return function (util) {
    return {
      compare: function (actual, key, optionalMessage) {
        var passes = matcher(key, actual);
        return {
          pass: passes,
          message: util.buildFailureMessage(name, passes, actual, optionalMessage || key)
        };
      }
    };
  };
}

function forKeyAndActualAndExpected(name, matcher) {
  return function (util) {
    return {
      compare: function (actual, key, expected, optionalMessage) {
        var passes = matcher(key, expected, actual);
        var message = (optionalMessage ?
          util.buildFailureMessage(name, passes, actual, expected, optionalMessage) :
          util.buildFailureMessage(name, passes, actual, expected)
        );
        return {
          pass: passes,
          message: formatErrorMessage(name, message, key)
        };
      }
    };
  };
}

function forKeyAndActualAndTwoExpected(name, matcher) {
  return function (util) {
    return {
      compare: function (actual, key, expected1, expected2, optionalMessage) {
        var passes = matcher(key, expected1, expected2, actual);
        var message = (optionalMessage ?
          util.buildFailureMessage(name, passes, actual, expected1, expected2, optionalMessage) :
          util.buildFailureMessage(name, passes, actual, expected1, expected2)
        );
        return {
          pass: passes,
          message: formatErrorMessage(name, message, key)
        };
      }
    };
  };
}

function formatErrorMessage(name, message, key) {
  if (name.search(/^toHave/) !== -1) {
    return message
      .replace('Expected', 'Expected member "' + key + '" of')
      .replace(' to have ', ' to be ');
  }
  return message;
}
