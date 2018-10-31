const path = require('path');
const fs = require('fs');

const sourceMap = require('source-map'); //https://github.com/mozilla/source-map#consuming-a-source-map
const sourceMapResolve = require('source-map-resolve'); //https://www.npmjs.com/package/source-map-resolve
const errorStackParser = require('error-stack-parser'); //https://github.com/stacktracejs/error-stack-parser
const minimatch = require('minimatch');	//https://www.npmjs.com/package/minimatch

function defaultSourceMapProvider(fileName) {
	const code = fs.readFileSync(fileName, 'utf8');
	return sourceMapResolve.resolveSourceMapSync(code, fileName, fs.readFileSync);
}

function create(jasmineCorePath, options) {

	const sourceMapProvider = options.sourceMapProvider || defaultSourceMapProvider;

	return function tsStackFilter(stack) {

		if (!stack) {
			return '';
		}
		return errorStackParser.parse({
				stack: stack
			})
			.filter(function (stackFrame) {
				let result = !stackFrame.fileName.includes(jasmineCorePath);
				if (options.stackFilterIgnore instanceof RegExp) {
					result = result && !options.stackFilterIgnore.test(stackFrame.fileName);
				} else if (typeof options.stackFilterIgnore === 'string') {
					result = result && !minimatch(stackFrame.fileName, options.stackFilterIgnore);
				}
				return result;
			})
			.map((stackFrame) => {
				//console.log('stackFrame', stackFrame);
				if (!path.isAbsolute(stackFrame.fileName)) {
					return stackFrame.source;
				}
				const srcMapResult = sourceMapProvider(stackFrame.fileName);
				if (!srcMapResult) {
					return stackFrame.source;
				}
				//console.log('stackFrame', stackFrame);
				//console.log('srcMapResult', Object.assign({}, srcMapResult, { map: 'excluded' })); 
				//console.log('srcMapResult.map.sources', srcMapResult.map.sources);
				const srcMapConsumer = new sourceMap.SourceMapConsumer(srcMapResult.map);
				const originalPosition = srcMapConsumer.originalPositionFor({
					line: stackFrame.lineNumber,
					column: stackFrame.columnNumber
				});
				//console.log('originalPosition', originalPosition);
				if (!originalPosition.source) {
					//console.warn('Failed to resolve sourcemap original position');
					return stackFrame.source;
				}
				const fileName = path.resolve(path.dirname(srcMapResult.sourcesRelativeTo), originalPosition.source);
				if (!fs.existsSync(fileName)) {
					//console.warn('Failed to resolve source file', srcMapResult.sourcesRelativeTo, 'with', originalPosition.source);
					return stackFrame.source;
				}
				return stackFrame.source.replace(
					stackFrame.fileName + ':' + stackFrame.lineNumber + ':' + stackFrame.columnNumber,
					fileName + ':' + originalPosition.line + ':' + originalPosition.column
				);
			}).join('\n');
	}
}

module.exports = exports = {
	create: create
};