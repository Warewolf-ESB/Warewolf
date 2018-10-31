module.exports = {
  2: forKeyAndActual,
  3: forKeyAndActualAndExpected,
  4: forKeyAndActualAndTwoExpected
};

function forKeyAndActual(name, matcher) {
  return function (received, key) {
    var pass = matcher(key, received);
    var infix = pass ? ' not ' : ' ';
    var message = 'expected member "' + key + '" of ' + this.utils.printReceived(received) + infix + getLongName(name);
    return {
      message: function () {
        return message;
      },
      pass: pass
    };
  };
}

function forKeyAndActualAndExpected(name, matcher) {
  return function (received, key, expected) {
    var pass = matcher(key, expected, received);
    var infix = pass ? ' not ' : ' ';
    var message = 'expected member "' + key + '" of ' + this.utils.printReceived(received) + infix + getLongName(name) + ' ' + this.utils.printExpected(expected);
    return {
      message: function () {
        return message;
      },
      pass: pass
    };
  };
}

function forKeyAndActualAndTwoExpected(name, matcher) {
  return function (received, key, expected1, expected2) {
    var pass = matcher(key, expected1, expected2, received);
    var infix = pass ? ' not ' : ' ';
    var message = 'expected member "' + key + '" of ' + this.utils.printReceived(received) + infix + getLongName(name) + ' ' + this.utils.printExpected(expected1) + ', ' + this.utils.printExpected(expected2);
    return {
      message: function () {
        return message;
      },
      pass: pass
    };
  };
}

function getLongName(name) {
  return name.replace(/\B([A-Z])/g, ' $1').toLowerCase();
}
