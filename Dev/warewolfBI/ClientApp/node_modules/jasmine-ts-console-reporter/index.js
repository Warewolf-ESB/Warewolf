const path = require('path');
const util = require('util');
const fs = require("fs");

const Timer = require('./timer');
const ConsoleReporter = require('./console-reporter');
const tsStackFilter = require('./ts-stack-filter');

function TSConsoleReporter(options) {

	const consoleReporter = new ConsoleReporter();
	options = options || {};
	const jasmineCorePath = options.jasmineCorePath || path.join(require('jasmine-core').files.path, 'jasmine.js');
	consoleReporter.setOptions({
		timer: options.timer || new Timer(),
		print: options.print || function () {
			process.stdout.write(util.format.apply(this, arguments));
		},
		showColors: options.showColors === undefined ? true : options.showColors,
		jasmineCorePath: jasmineCorePath,
		titleFilter: options.titleFilter,
		stackFilter: tsStackFilter.create(jasmineCorePath, options),
		messageFilter: options.messageFilter
	});

	return consoleReporter;
}

module.exports = exports = TSConsoleReporter;