// modules
var createRegister = require('./src/create-register');
var jasmineV1 = require('./src/jasmine-v1');
var jasmineV2 = require('./src/jasmine-v2');
var jest = require('./src/jest');

// public
module.exports = createRegister({
  jasmineV1: jasmineV1,
  jasmineV2: jasmineV2,
  jest: jest
}, global);
