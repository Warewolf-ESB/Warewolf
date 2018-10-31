module.exports = {
  1: adapterForActual,
  2: adapterForActualAndExpected,
  3: adapterForActualAndTwoExpected
};

function adapterForActual(name, matcher) {
  return function (received) {
    var pass = matcher(received);
    var infix = pass ? ' not ' : ' ';
    var message = 'expected ' + this.utils.printReceived(received) + infix + getLongName(name);
    return {
      message: function () {
        return message;
      },
      pass: pass
    };
  };
}

function adapterForActualAndExpected(name, matcher) {
  return function (received, expected) {
    var pass = matcher(expected, received);
    var infix = pass ? ' not ' : ' ';
    var message = 'expected ' + this.utils.printReceived(received) + infix + getLongName(name) + ' ' + this.utils.printExpected(expected);
    return {
      message: function () {
        return message;
      },
      pass: pass
    };
  };
}

function adapterForActualAndTwoExpected(name, matcher) {
  return function (received, expected1, expected2) {
    var pass = matcher(expected1, expected2, received);
    var infix = pass ? ' not ' : ' ';
    var message = 'expected ' + this.utils.printReceived(received) + infix + getLongName(name) + ' ' + this.utils.printExpected(expected1) + ', ' + this.utils.printExpected(expected2);
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
