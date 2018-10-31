(function e(t,n,r){function s(o,u){if(!n[o]){if(!t[o]){var a=typeof require=="function"&&require;if(!u&&a)return a(o,!0);if(i)return i(o,!0);var f=new Error("Cannot find module '"+o+"'");throw f.code="MODULE_NOT_FOUND",f}var l=n[o]={exports:{}};t[o][0].call(l.exports,function(e){var n=t[o][1][e];return s(n?n:e)},l,l.exports,e,t,n,r)}return n[o].exports}var i=typeof require=="function"&&require;for(var o=0;o<r.length;o++)s(r[o]);return s})({1:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.after', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy(new Date('2013-01-01T00:00:00.000Z'));
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.after(new Date('1998-08-12T01:00:00.000Z')));
  });
});

},{"./lib/callSpy":18}],2:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.arrayOfBooleans', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy([true, false, new Boolean(true), new Boolean(false)]);
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.arrayOfBooleans());
  });
});

},{"./lib/callSpy":18}],3:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.arrayOfNumbers', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy([1, new Number(6)]);
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.arrayOfNumbers());
  });
});

},{"./lib/callSpy":18}],4:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.arrayOfObjects', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy([{}, new Object()]);
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.arrayOfObjects());
  });
});

},{"./lib/callSpy":18}],5:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.arrayOfSize', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy(['a', 'b', 'c']);
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.arrayOfSize(3));
  });
});

},{"./lib/callSpy":18}],6:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.arrayOfStrings', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy(['', ' ', new String('hello')]);
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.arrayOfStrings());
  });
});

},{"./lib/callSpy":18}],7:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.before', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy(new Date('1998-08-12T01:00:00.000Z'));
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.before(new Date('2013-01-01T00:00:00.000Z')));
  });
});

},{"./lib/callSpy":18}],8:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.calculable', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy('1');
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.calculable());
  });
});

},{"./lib/callSpy":18}],9:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.emptyArray', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy([]);
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.emptyArray());
  });
});

},{"./lib/callSpy":18}],10:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.emptyObject', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy({});
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.emptyObject());
  });
});

},{"./lib/callSpy":18}],11:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.endingWith', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy('Guybrush Threepwood');
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.endingWith('eepwood'));
  });
});

},{"./lib/callSpy":18}],12:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.evenNumber', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy(4);
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.evenNumber());
  });
});

},{"./lib/callSpy":18}],13:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.greaterThanOrEqualTo', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy(8);
    shared.spy2 = callSpy(9);
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.greaterThanOrEqualTo(8));
    expect(shared.spy2).toHaveBeenCalledWith(any.greaterThanOrEqualTo(8));
  });
});

},{"./lib/callSpy":18}],14:[function(require,module,exports){
'use strict';

module.exports = {
  after: require('./after.spec'),
  arrayOfBooleans: require('./arrayOfBooleans.spec'),
  arrayOfNumbers: require('./arrayOfNumbers.spec'),
  arrayOfObjects: require('./arrayOfObjects.spec'),
  arrayOfSize: require('./arrayOfSize.spec'),
  arrayOfStrings: require('./arrayOfStrings.spec'),
  before: require('./before.spec'),
  calculable: require('./calculable.spec'),
  emptyArray: require('./emptyArray.spec'),
  emptyObject: require('./emptyObject.spec'),
  endingWith: require('./endingWith.spec'),
  evenNumber: require('./evenNumber.spec'),
  greaterThanOrEqualTo: require('./greaterThanOrEqualTo.spec'),
  iso8601: require('./iso8601.spec'),
  jsonString: require('./jsonString.spec'),
  lessThanOrEqualTo: require('./lessThanOrEqualTo.spec'),
  longerThan: require('./longerThan.spec'),
  nonEmptyArray: require('./nonEmptyArray.spec'),
  nonEmptyObject: require('./nonEmptyObject.spec'),
  nonEmptyString: require('./nonEmptyString.spec'),
  oddNumber: require('./oddNumber.spec'),
  regExp: require('./regExp.spec'),
  sameLengthAs: require('./sameLengthAs.spec'),
  shorterThan: require('./shorterThan.spec'),
  startingWith: require('./startingWith.spec'),
  toBeAfter: require('./toBeAfter.spec'),
  toBeArray: require('./toBeArray.spec'),
  toBeArrayOfBooleans: require('./toBeArrayOfBooleans.spec'),
  toBeArrayOfNumbers: require('./toBeArrayOfNumbers.spec'),
  toBeArrayOfObjects: require('./toBeArrayOfObjects.spec'),
  toBeArrayOfSize: require('./toBeArrayOfSize.spec'),
  toBeArrayOfStrings: require('./toBeArrayOfStrings.spec'),
  toBeBefore: require('./toBeBefore.spec'),
  toBeBoolean: require('./toBeBoolean.spec'),
  toBeCalculable: require('./toBeCalculable.spec'),
  toBeDate: require('./toBeDate.spec'),
  toBeEmptyArray: require('./toBeEmptyArray.spec'),
  toBeEmptyObject: require('./toBeEmptyObject.spec'),
  toBeEmptyString: require('./toBeEmptyString.spec'),
  toBeEvenNumber: require('./toBeEvenNumber.spec'),
  toBeFalse: require('./toBeFalse.spec'),
  toBeFunction: require('./toBeFunction.spec'),
  toBeGreaterThanOrEqualTo: require('./toBeGreaterThanOrEqualTo.spec'),
  toBeHtmlString: require('./toBeHtmlString.spec'),
  toBeIso8601: require('./toBeIso8601.spec'),
  toBeJsonString: require('./toBeJsonString.spec'),
  toBeLessThanOrEqualTo: require('./toBeLessThanOrEqualTo.spec'),
  toBeLongerThan: require('./toBeLongerThan.spec'),
  toBeNear: require('./toBeNear.spec'),
  toBeNonEmptyArray: require('./toBeNonEmptyArray.spec'),
  toBeNonEmptyObject: require('./toBeNonEmptyObject.spec'),
  toBeNonEmptyString: require('./toBeNonEmptyString.spec'),
  toBeNumber: require('./toBeNumber.spec'),
  toBeObject: require('./toBeObject.spec'),
  toBeOddNumber: require('./toBeOddNumber.spec'),
  toBeRegExp: require('./toBeRegExp.spec'),
  toBeSameLengthAs: require('./toBeSameLengthAs.spec'),
  toBeShorterThan: require('./toBeShorterThan.spec'),
  toBeString: require('./toBeString.spec'),
  toBeTrue: require('./toBeTrue.spec'),
  toBeValidDate: require('./toBeValidDate.spec'),
  toBeWhitespace: require('./toBeWhitespace.spec'),
  toBeWholeNumber: require('./toBeWholeNumber.spec'),
  toBeWithinRange: require('./toBeWithinRange.spec'),
  toEndWith: require('./toEndWith.spec'),
  toHaveArray: require('./toHaveArray.spec'),
  toHaveArrayOfBooleans: require('./toHaveArrayOfBooleans.spec'),
  toHaveArrayOfNumbers: require('./toHaveArrayOfNumbers.spec'),
  toHaveArrayOfObjects: require('./toHaveArrayOfObjects.spec'),
  toHaveArrayOfSize: require('./toHaveArrayOfSize.spec'),
  toHaveArrayOfStrings: require('./toHaveArrayOfStrings.spec'),
  toHaveBoolean: require('./toHaveBoolean.spec'),
  toHaveCalculable: require('./toHaveCalculable.spec'),
  toHaveDate: require('./toHaveDate.spec'),
  toHaveDateAfter: require('./toHaveDateAfter.spec'),
  toHaveDateBefore: require('./toHaveDateBefore.spec'),
  toHaveEmptyArray: require('./toHaveEmptyArray.spec'),
  toHaveEmptyObject: require('./toHaveEmptyObject.spec'),
  toHaveEmptyString: require('./toHaveEmptyString.spec'),
  toHaveEvenNumber: require('./toHaveEvenNumber.spec'),
  toHaveFalse: require('./toHaveFalse.spec'),
  toHaveHtmlString: require('./toHaveHtmlString.spec'),
  toHaveIso8601: require('./toHaveIso8601.spec'),
  toHaveJsonString: require('./toHaveJsonString.spec'),
  toHaveMember: require('./toHaveMember.spec'),
  toHaveMethod: require('./toHaveMethod.spec'),
  toHaveNonEmptyArray: require('./toHaveNonEmptyArray.spec'),
  toHaveNonEmptyObject: require('./toHaveNonEmptyObject.spec'),
  toHaveNonEmptyString: require('./toHaveNonEmptyString.spec'),
  toHaveNumber: require('./toHaveNumber.spec'),
  toHaveNumberWithinRange: require('./toHaveNumberWithinRange.spec'),
  toHaveObject: require('./toHaveObject.spec'),
  toHaveOddNumber: require('./toHaveOddNumber.spec'),
  toHaveString: require('./toHaveString.spec'),
  toHaveStringLongerThan: require('./toHaveStringLongerThan.spec'),
  toHaveStringSameLengthAs: require('./toHaveStringSameLengthAs.spec'),
  toHaveStringShorterThan: require('./toHaveStringShorterThan.spec'),
  toHaveTrue: require('./toHaveTrue.spec'),
  toHaveUndefined: require('./toHaveUndefined.spec'),
  toHaveWhitespaceString: require('./toHaveWhitespaceString.spec'),
  toHaveWholeNumber: require('./toHaveWholeNumber.spec'),
  toStartWith: require('./toStartWith.spec'),
  toThrowAnyError: require('./toThrowAnyError.spec'),
  toThrowErrorOfType: require('./toThrowErrorOfType.spec'),
  whitespace: require('./whitespace.spec'),
  wholeNumber: require('./wholeNumber.spec'),
  withinRange: require('./withinRange.spec')
};

},{"./after.spec":1,"./arrayOfBooleans.spec":2,"./arrayOfNumbers.spec":3,"./arrayOfObjects.spec":4,"./arrayOfSize.spec":5,"./arrayOfStrings.spec":6,"./before.spec":7,"./calculable.spec":8,"./emptyArray.spec":9,"./emptyObject.spec":10,"./endingWith.spec":11,"./evenNumber.spec":12,"./greaterThanOrEqualTo.spec":13,"./iso8601.spec":15,"./jsonString.spec":16,"./lessThanOrEqualTo.spec":17,"./longerThan.spec":26,"./nonEmptyArray.spec":27,"./nonEmptyObject.spec":28,"./nonEmptyString.spec":29,"./oddNumber.spec":30,"./regExp.spec":31,"./sameLengthAs.spec":32,"./shorterThan.spec":33,"./startingWith.spec":34,"./toBeAfter.spec":35,"./toBeArray.spec":36,"./toBeArrayOfBooleans.spec":37,"./toBeArrayOfNumbers.spec":38,"./toBeArrayOfObjects.spec":39,"./toBeArrayOfSize.spec":40,"./toBeArrayOfStrings.spec":41,"./toBeBefore.spec":42,"./toBeBoolean.spec":43,"./toBeCalculable.spec":44,"./toBeDate.spec":45,"./toBeEmptyArray.spec":46,"./toBeEmptyObject.spec":47,"./toBeEmptyString.spec":48,"./toBeEvenNumber.spec":49,"./toBeFalse.spec":50,"./toBeFunction.spec":51,"./toBeGreaterThanOrEqualTo.spec":52,"./toBeHtmlString.spec":53,"./toBeIso8601.spec":54,"./toBeJsonString.spec":55,"./toBeLessThanOrEqualTo.spec":56,"./toBeLongerThan.spec":57,"./toBeNear.spec":58,"./toBeNonEmptyArray.spec":59,"./toBeNonEmptyObject.spec":60,"./toBeNonEmptyString.spec":61,"./toBeNumber.spec":62,"./toBeObject.spec":63,"./toBeOddNumber.spec":64,"./toBeRegExp.spec":65,"./toBeSameLengthAs.spec":66,"./toBeShorterThan.spec":67,"./toBeString.spec":68,"./toBeTrue.spec":69,"./toBeValidDate.spec":70,"./toBeWhitespace.spec":71,"./toBeWholeNumber.spec":72,"./toBeWithinRange.spec":73,"./toEndWith.spec":74,"./toHaveArray.spec":75,"./toHaveArrayOfBooleans.spec":76,"./toHaveArrayOfNumbers.spec":77,"./toHaveArrayOfObjects.spec":78,"./toHaveArrayOfSize.spec":79,"./toHaveArrayOfStrings.spec":80,"./toHaveBoolean.spec":81,"./toHaveCalculable.spec":82,"./toHaveDate.spec":83,"./toHaveDateAfter.spec":84,"./toHaveDateBefore.spec":85,"./toHaveEmptyArray.spec":86,"./toHaveEmptyObject.spec":87,"./toHaveEmptyString.spec":88,"./toHaveEvenNumber.spec":89,"./toHaveFalse.spec":90,"./toHaveHtmlString.spec":91,"./toHaveIso8601.spec":92,"./toHaveJsonString.spec":93,"./toHaveMember.spec":94,"./toHaveMethod.spec":95,"./toHaveNonEmptyArray.spec":96,"./toHaveNonEmptyObject.spec":97,"./toHaveNonEmptyString.spec":98,"./toHaveNumber.spec":99,"./toHaveNumberWithinRange.spec":100,"./toHaveObject.spec":101,"./toHaveOddNumber.spec":102,"./toHaveString.spec":103,"./toHaveStringLongerThan.spec":104,"./toHaveStringSameLengthAs.spec":105,"./toHaveStringShorterThan.spec":106,"./toHaveTrue.spec":107,"./toHaveUndefined.spec":108,"./toHaveWhitespaceString.spec":109,"./toHaveWholeNumber.spec":110,"./toStartWith.spec":111,"./toThrowAnyError.spec":112,"./toThrowErrorOfType.spec":113,"./whitespace.spec":114,"./wholeNumber.spec":115,"./withinRange.spec":116}],15:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.iso8601', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy('2013-07-08T07:29:15.863Z');
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.iso8601());
  });
});

},{"./lib/callSpy":18}],16:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.jsonString', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy('{}');
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.jsonString());
  });
});

},{"./lib/callSpy":18}],17:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.lessThanOrEqualTo', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy(8);
    shared.spy2 = callSpy(9);
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.lessThanOrEqualTo(9));
    expect(shared.spy2).toHaveBeenCalledWith(any.lessThanOrEqualTo(9));
  });
});

},{"./lib/callSpy":18}],18:[function(require,module,exports){
"use strict";

module.exports = function callSpy(arg) {
  var spy = jasmine.createSpy();
  spy(arg);
  return spy;
};

},{}],19:[function(require,module,exports){
'use strict';

var describeWhenNotArray = require('./describeWhenNotArray');

module.exports = function describeToBeArrayOfX(name, options) {
  describe(name, function () {
    describe('when invoked', function () {
      describe('when subject is a true Array', function () {
        describe('when subject has no members', function () {
          it('should confirm (an empty array of ' + options.type + 's is valid)', function () {
            expect([])[name]();
          });
        });
        describe('when subject has members', function () {
          describe('when subject has a mix of ' + options.type + 's and other items', function () {
            it('should deny', options.whenMixed);
          });
          describe('when subject has only ' + options.type + 's', function () {
            it('should confirm', options.whenValid);
          });
          describe('when subject has other items', function () {
            it('should deny', options.whenInvalid);
          });
        });
      });
      describeWhenNotArray(name);
    });
  });
};

},{"./describeWhenNotArray":23}],20:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./describeToHaveX');

module.exports = function describeToHaveArrayX(name, whenArray) {
  describeToHaveX(name, function () {
    describe('when member is an array', whenArray);
  });
};

},{"./describeToHaveX":22}],21:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./describeToHaveX');

module.exports = function describeToHaveBooleanX(name, whenBoolean) {
  describeToHaveX(name, function () {
    describe('when member is truthy', function () {
      it('should deny', function () {
        expect({
          memberName: 1
        }).not[name]('memberName');
        expect({
          memberName: 'true'
        }).not[name]('memberName');
      });
    });
    describe('when member is falsy', function () {
      it('should deny', function () {
        expect({
          memberName: 0
        }).not[name]('memberName');
        expect({
          memberName: ''
        }).not[name]('memberName');
      });
    });
    describe('when member is boolean', whenBoolean);
  });
};

},{"./describeToHaveX":22}],22:[function(require,module,exports){
'use strict';

module.exports = function describeToHaveX(name, whenPresent) {
  describe('when invoked', function () {
    describe('when subject is not an object', function () {
      it('should deny', function () {
        expect(0).not[name]('memberName');
        expect(null).not[name]('memberName');
        expect(true).not[name]('memberName');
        expect(false).not[name]('memberName');
        expect('').not[name]('memberName');
      });
    });
    describe('when subject is an object', function () {
      describe('when member is not present', function () {
        it('should deny', function () {
          expect({}).not[name]('memberName');
        });
      });
      describe('when member is present', function () {
        whenPresent();
      });
    });
  });
};

},{}],23:[function(require,module,exports){
'use strict';

var getArgumentsObject = require('./getArgumentsObject');
var getArrayLikeObject = require('./getArrayLikeObject');

module.exports = function describeWhenNotArray(toBeArrayMemberName) {
  describe('when subject is not a true Array', function () {
    describe('when subject is Array-like', function () {
      it('should deny', function () {
        expect(getArgumentsObject()).not[toBeArrayMemberName]();
        expect(getArrayLikeObject()).not[toBeArrayMemberName]();
      });
    });
    describe('when subject is not Array-like', function () {
      it('should deny', function () {
        var _undefined = void 0;
        expect({}).not[toBeArrayMemberName]();
        expect(null).not[toBeArrayMemberName]();
        expect(_undefined).not[toBeArrayMemberName]();
        expect(true).not[toBeArrayMemberName]();
        expect(false).not[toBeArrayMemberName]();
        expect(Array).not[toBeArrayMemberName]();
      });
    });
  });
};

},{"./getArgumentsObject":24,"./getArrayLikeObject":25}],24:[function(require,module,exports){
"use strict";

module.exports = function getArgumentsObject() {
  return function () {
    return arguments;
  }(1, 2, 3);
};

},{}],25:[function(require,module,exports){
"use strict";

module.exports = function getArrayLikeObject() {
  return {
    0: 1,
    1: 2,
    2: 3,
    length: 3
  };
};

},{}],26:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.longerThan', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy('antidisestablishmentarianism');
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.longerThan('dog'));
  });
});

},{"./lib/callSpy":18}],27:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.nonEmptyArray', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy([0, false, 1, true]);
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.nonEmptyArray());
  });
});

},{"./lib/callSpy":18}],28:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.nonEmptyObject', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy({
      prop: 'value'
    });
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.nonEmptyObject());
  });
});

},{"./lib/callSpy":18}],29:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.nonEmptyString', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy('hello world');
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.nonEmptyString());
  });
});

},{"./lib/callSpy":18}],30:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.oddNumber', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy(3);
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.oddNumber());
  });
});

},{"./lib/callSpy":18}],31:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.regExp', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy(/abc/);
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.regExp());
  });
});

},{"./lib/callSpy":18}],32:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.sameLengthAs', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy('Cat');
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.sameLengthAs('Dog'));
  });
});

},{"./lib/callSpy":18}],33:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.shorterThan', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy('skeletor');
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.shorterThan('trogdor the burninator'));
  });
});

},{"./lib/callSpy":18}],34:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.startingWith', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy('San Francisco');
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.startingWith('San'));
  });
});

},{"./lib/callSpy":18}],35:[function(require,module,exports){
'use strict';

describe('toBeAfter', function () {
  describe('when invoked', function () {
    describe('when value is a Date', function () {
      describe('when date occurs after another', function () {
        it('should confirm', function () {
          expect(new Date('2013-01-01T01:00:00.000Z')).toBeAfter(new Date('2013-01-01T00:00:00.000Z'));
        });
      });
      describe('when date does NOT occur after another', function () {
        it('should deny', function () {
          expect(new Date('2013-01-01T00:00:00.000Z')).not.toBeAfter(new Date('2013-01-01T01:00:00.000Z'));
        });
      });
    });
  });
});

},{}],36:[function(require,module,exports){
'use strict';

var describeWhenNotArray = require('./lib/describeWhenNotArray');

describe('toBeArray', function () {
  describe('when invoked', function () {
    describe('when subject is a true Array', function () {
      it('should confirm', function () {
        expect([]).toBeArray();
        expect(new Array()).toBeArray();
      });
    });
    describeWhenNotArray('toBeArray');
  });
});

},{"./lib/describeWhenNotArray":23}],37:[function(require,module,exports){
'use strict';

var describeToBeArrayOfX = require('./lib/describeToBeArrayOfX');

describe('toBeArrayOfBooleans', function () {
  describeToBeArrayOfX('toBeArrayOfBooleans', {
    type: 'Boolean',
    whenValid: function whenValid() {
      expect([true]).toBeArrayOfBooleans();
      expect([new Boolean(true)]).toBeArrayOfBooleans();
      expect([new Boolean(false)]).toBeArrayOfBooleans();
      expect([false, true]).toBeArrayOfBooleans();
    },
    whenInvalid: function whenInvalid() {
      expect([null]).not.toBeArrayOfBooleans();
    },
    whenMixed: function whenMixed() {
      expect([null, false]).not.toBeArrayOfBooleans();
      expect([null, true]).not.toBeArrayOfBooleans();
    }
  });
});

},{"./lib/describeToBeArrayOfX":19}],38:[function(require,module,exports){
'use strict';

var describeToBeArrayOfX = require('./lib/describeToBeArrayOfX');

describe('toBeArrayOfNumbers', function () {
  describeToBeArrayOfX('toBeArrayOfNumbers', {
    type: 'Number',
    whenValid: function whenValid() {
      expect([1]).toBeArrayOfNumbers();
      expect([new Number(1)]).toBeArrayOfNumbers();
      expect([new Number(0)]).toBeArrayOfNumbers();
      expect([0, 1]).toBeArrayOfNumbers();
    },
    whenInvalid: function whenInvalid() {
      expect([null]).not.toBeArrayOfNumbers();
    },
    whenMixed: function whenMixed() {
      expect([null, 0]).not.toBeArrayOfNumbers();
    }
  });
});

},{"./lib/describeToBeArrayOfX":19}],39:[function(require,module,exports){
'use strict';

var describeToBeArrayOfX = require('./lib/describeToBeArrayOfX');

describe('toBeArrayOfObjects', function () {
  describeToBeArrayOfX('toBeArrayOfObjects', {
    type: 'Object',
    whenValid: function whenValid() {
      expect([{}, {}]).toBeArrayOfObjects();
    },
    whenInvalid: function whenInvalid() {
      expect([null]).not.toBeArrayOfObjects();
      expect(['Object']).not.toBeArrayOfObjects();
      expect(['[object Object]']).not.toBeArrayOfObjects();
    },
    whenMixed: function whenMixed() {
      expect([null, {}]).not.toBeArrayOfObjects();
    }
  });
});

},{"./lib/describeToBeArrayOfX":19}],40:[function(require,module,exports){
'use strict';

var describeWhenNotArray = require('./lib/describeWhenNotArray');

describe('toBeArrayOfSize', function () {
  describe('when invoked', function () {
    describe('when subject is a true Array', function () {
      describe('when subject has the expected number of members', function () {
        it('should confirm', function () {
          var _undefined = void 0;
          expect([]).toBeArrayOfSize(0);
          expect([null]).toBeArrayOfSize(1);
          expect([false, false]).toBeArrayOfSize(2);
          expect([_undefined, _undefined]).toBeArrayOfSize(2);
        });
      });
      describe('when subject has an unexpected number of members', function () {
        it('should deny', function () {
          expect([]).not.toBeArrayOfSize(1);
          expect([null]).not.toBeArrayOfSize(0);
          expect([true, true]).not.toBeArrayOfSize(1);
        });
      });
    });
    describeWhenNotArray('toBeArrayOfSize');
  });
});

},{"./lib/describeWhenNotArray":23}],41:[function(require,module,exports){
'use strict';

var describeToBeArrayOfX = require('./lib/describeToBeArrayOfX');

describe('toBeArrayOfStrings', function () {
  describeToBeArrayOfX('toBeArrayOfStrings', {
    type: 'String',
    whenValid: function whenValid() {
      expect(['truthy']).toBeArrayOfStrings();
      expect([new String('truthy')]).toBeArrayOfStrings();
      expect([new String('')]).toBeArrayOfStrings();
      expect(['', 'truthy']).toBeArrayOfStrings();
    },
    whenInvalid: function whenInvalid() {
      expect([null]).not.toBeArrayOfStrings();
    },
    whenMixed: function whenMixed() {
      expect([null, '']).not.toBeArrayOfStrings();
    }
  });
});

},{"./lib/describeToBeArrayOfX":19}],42:[function(require,module,exports){
'use strict';

describe('toBeBefore', function () {
  describe('when invoked', function () {
    describe('when value is a Date', function () {
      describe('when date occurs before another', function () {
        it('should confirm', function () {
          expect(new Date('2013-01-01T00:00:00.000Z')).toBeBefore(new Date('2013-01-01T01:00:00.000Z'));
        });
      });
      describe('when date does NOT occur before another', function () {
        it('should deny', function () {
          expect(new Date('2013-01-01T01:00:00.000Z')).not.toBeBefore(new Date('2013-01-01T00:00:00.000Z'));
        });
      });
    });
  });
});

},{}],43:[function(require,module,exports){
'use strict';

describe('toBeBoolean', function () {
  describe('when invoked', function () {
    describe('when subject not only truthy or falsy, but a boolean', function () {
      it('should confirm', function () {
        expect(true).toBeBoolean();
        expect(false).toBeBoolean();
        expect(new Boolean(true)).toBeBoolean();
        expect(new Boolean(false)).toBeBoolean();
      });
    });
    describe('when subject is truthy or falsy', function () {
      it('should deny', function () {
        expect(1).not.toBeBoolean();
        expect(0).not.toBeBoolean();
      });
    });
  });
});

},{}],44:[function(require,module,exports){
'use strict';

describe('toBeCalculable', function () {
  describe('when invoked', function () {
    describe('when subject CAN be coerced to be used in mathematical operations', function () {
      it('should confirm', function () {
        expect('1').toBeCalculable();
        expect('').toBeCalculable();
        expect(null).toBeCalculable();
      });
    });
    describe('when subject can NOT be coerced by JavaScript to be used in mathematical operations', function () {
      it('should deny', function () {
        expect({}).not.toBeCalculable();
        expect(NaN).not.toBeCalculable();
      });
    });
  });
});

},{}],45:[function(require,module,exports){
'use strict';

describe('toBeDate', function () {
  describe('when invoked', function () {
    describe('when value is an instance of Date', function () {
      it('should confirm', function () {
        expect(new Date()).toBeDate();
      });
    });
    describe('when value is NOT an instance of Date', function () {
      it('should deny', function () {
        expect(null).not.toBeDate();
      });
    });
  });
});

},{}],46:[function(require,module,exports){
'use strict';

var describeWhenNotArray = require('./lib/describeWhenNotArray');

describe('toBeEmptyArray', function () {
  describe('when invoked', function () {
    describe('when subject is a true Array', function () {
      describe('when subject has members', function () {
        it('should confirm', function () {
          expect([]).toBeEmptyArray();
        });
      });
      describe('when subject has no members', function () {
        it('should deny', function () {
          expect([null]).not.toBeEmptyArray();
          expect(['']).not.toBeEmptyArray();
          expect([1]).not.toBeEmptyArray();
          expect([true]).not.toBeEmptyArray();
          expect([false]).not.toBeEmptyArray();
        });
      });
    });
    describeWhenNotArray('toBeEmptyArray');
  });
});

},{"./lib/describeWhenNotArray":23}],47:[function(require,module,exports){
'use strict';

describe('toBeEmptyObject', function () {
  var Foo = void 0;
  beforeEach(function () {
    Foo = function Foo() {};
  });
  describe('when invoked', function () {
    describe('when subject IS an Object with no instance members', function () {
      beforeEach(function () {
        Foo.prototype = {
          b: 2
        };
      });
      it('should confirm', function () {
        expect(new Foo()).toBeEmptyObject();
        expect({}).toBeEmptyObject();
      });
    });
    describe('when subject is NOT an Object with no instance members', function () {
      it('should deny', function () {
        expect({
          a: 1
        }).not.toBeEmptyObject();
        expect(null).not.toBeNonEmptyObject();
      });
    });
  });
});

},{}],48:[function(require,module,exports){
'use strict';

describe('toBeEmptyString', function () {
  describe('when invoked', function () {
    describe('when subject IS a string with no characters', function () {
      it('should confirm', function () {
        expect('').toBeEmptyString();
      });
    });
    describe('when subject is NOT a string with no characters', function () {
      it('should deny', function () {
        expect(' ').not.toBeEmptyString();
      });
    });
  });
});

},{}],49:[function(require,module,exports){
'use strict';

describe('toBeEvenNumber', function () {
  describe('when invoked', function () {
    describe('when subject IS an even number', function () {
      it('should confirm', function () {
        expect(2).toBeEvenNumber();
      });
    });
    describe('when subject is NOT an even number', function () {
      it('should deny', function () {
        expect(1).not.toBeEvenNumber();
        expect(NaN).not.toBeEvenNumber();
      });
    });
  });
});

},{}],50:[function(require,module,exports){
'use strict';

describe('toBeFalse', function () {
  describe('when invoked', function () {
    describe('when subject is not only falsy, but a boolean false', function () {
      it('should confirm', function () {
        expect(false).toBeFalse();
        expect(new Boolean(false)).toBeFalse();
      });
    });
    describe('when subject is falsy', function () {
      it('should deny', function () {
        expect(1).not.toBeFalse();
      });
    });
  });
});

},{}],51:[function(require,module,exports){
'use strict';

describe('toBeFunction', function () {
  describe('when invoked', function () {
    describe('when subject IS a function', function () {
      it('should confirm', function () {
        expect(function () {}).toBeFunction();
      });
    });
    describe('when subject is NOT a function', function () {
      it('should deny', function () {
        expect(/regexp/).not.toBeFunction();
      });
    });
  });
});

},{}],52:[function(require,module,exports){
'use strict';

describe('toBeGreaterThanOrEqualTo', function () {
  it('asserts value is greater or equal than a given number', function () {
    expect(2).toBeGreaterThanOrEqualTo(1);
    expect(1).toBeGreaterThanOrEqualTo(-1);
    expect(-1).toBeGreaterThanOrEqualTo(-2);
    expect(-2).toBeGreaterThanOrEqualTo(-2);
    expect(NaN).not.toBeGreaterThanOrEqualTo(0);
    expect(1).not.toBeGreaterThanOrEqualTo(2);
    expect(-1).not.toBeGreaterThanOrEqualTo(0);
  });
});

},{}],53:[function(require,module,exports){
'use strict';

describe('toBeHtmlString', function () {
  describe('when invoked', function () {
    describe('when subject IS a string of HTML markup', function () {
      var ngMultiLine = void 0;
      beforeEach(function () {
        ngMultiLine = '';
        ngMultiLine += '<a data-ng-href="//www.google.com" data-ng-click="launchApp($event)" target="_blank" class="ng-binding" href="//www.google.com">';
        ngMultiLine += '\n';
        ngMultiLine += '  Watch with Google TV';
        ngMultiLine += '\n';
        ngMultiLine += '</a>';
        ngMultiLine += '\n';
      });
      it('should confirm', function () {
        expect('<element>text</element>').toBeHtmlString();
        expect('<a data-ng-href="//foo.com" data-ng-click="bar($event)">baz</a>').toBeHtmlString();
        expect('<div ng-if="foo > bar || bar < foo && baz == bar"></div>').toBeHtmlString();
        expect('<li ng-if="foo > bar || bar < foo && baz == bar">').toBeHtmlString();
        expect(ngMultiLine).toBeHtmlString();
      });
    });
    describe('when subject is NOT a string of HTML markup', function () {
      it('should deny', function () {
        expect('div').not.toBeHtmlString();
        expect(null).not.toBeHtmlString();
      });
    });
  });
});

},{}],54:[function(require,module,exports){
'use strict';

describe('toBeIso8601', function () {
  describe('when invoked', function () {
    describe('when value is a Date String conforming to the ISO 8601 standard', function () {
      describe('when specified date is valid', function () {
        it('should confirm', function () {
          expect('2013-07-08T07:29:15.863Z').toBeIso8601();
          expect('2013-07-08T07:29:15.863').toBeIso8601();
          expect('2013-07-08T07:29:15').toBeIso8601();
          expect('2013-07-08T07:29').toBeIso8601();
          expect('2013-07-08').toBeIso8601();
        });
      });
      describe('when specified date is NOT valid', function () {
        it('should deny', function () {
          expect('2013-99-12T00:00:00.000Z').not.toBeIso8601();
          expect('2013-12-99T00:00:00.000Z').not.toBeIso8601();
          expect('2013-01-01T99:00:00.000Z').not.toBeIso8601();
          expect('2013-01-01T99:99:00.000Z').not.toBeIso8601();
          expect('2013-01-01T00:00:99.000Z').not.toBeIso8601();
        });
      });
    });
    describe('when value is a String NOT conforming to the ISO 8601 standard', function () {
      it('should deny', function () {
        expect('2013-07-08T07:29:15.').not.toBeIso8601();
        expect('2013-07-08T07:29:').not.toBeIso8601();
        expect('2013-07-08T07:2').not.toBeIso8601();
        expect('2013-07-08T07:').not.toBeIso8601();
        expect('2013-07-08T07').not.toBeIso8601();
        expect('2013-07-08T').not.toBeIso8601();
        expect('2013-07-0').not.toBeIso8601();
        expect('2013-07-').not.toBeIso8601();
        expect('2013-07').not.toBeIso8601();
        expect('2013-0').not.toBeIso8601();
        expect('2013-').not.toBeIso8601();
        expect('2013').not.toBeIso8601();
        expect('201').not.toBeIso8601();
        expect('20').not.toBeIso8601();
        expect('2').not.toBeIso8601();
        expect('').not.toBeIso8601();
      });
    });
  });
});

},{}],55:[function(require,module,exports){
'use strict';

describe('toBeJsonString', function () {
  describe('when invoked', function () {
    describe('when subject IS a string of parseable JSON', function () {
      it('should confirm', function () {
        expect('{}').toBeJsonString();
        expect('[]').toBeJsonString();
        expect('[1]').toBeJsonString();
      });
    });
    describe('when subject is NOT a string of parseable JSON', function () {
      it('should deny', function () {
        var _undefined = void 0;
        expect('[1,]').not.toBeJsonString();
        expect('<>').not.toBeJsonString();
        expect(null).not.toBeJsonString();
        expect('').not.toBeJsonString();
        expect(_undefined).not.toBeJsonString();
      });
    });
  });
});

},{}],56:[function(require,module,exports){
'use strict';

describe('toBeLessThanOrEqualTo', function () {
  it('asserts value is less or equal than a given number', function () {
    expect(1).toBeLessThanOrEqualTo(2);
    expect(-1).toBeLessThanOrEqualTo(1);
    expect(-2).toBeLessThanOrEqualTo(-1);
    expect(-2).toBeLessThanOrEqualTo(-2);
    expect(NaN).not.toBeLessThanOrEqualTo(0);
    expect(2).not.toBeLessThanOrEqualTo(1);
    expect(0).not.toBeLessThanOrEqualTo(-1);
  });
});

},{}],57:[function(require,module,exports){
'use strict';

describe('toBeLongerThan', function () {
  describe('when invoked', function () {
    describe('when the subject and comparison ARE both strings', function () {
      describe('when the subject IS longer than the comparision string', function () {
        it('should confirm', function () {
          expect('abc').toBeLongerThan('ab');
          expect('a').toBeLongerThan('');
        });
      });
      describe('when the subject is NOT longer than the comparision string', function () {
        it('should deny', function () {
          expect('ab').not.toBeLongerThan('abc');
          expect('').not.toBeLongerThan('a');
        });
      });
    });
    describe('when the subject and comparison are NOT both strings', function () {
      it('should deny (we are asserting the relative lengths of two strings)', function () {
        var _undefined = void 0;
        expect('truthy').not.toBeLongerThan(_undefined);
        expect(_undefined).not.toBeLongerThan('truthy');
        expect('').not.toBeLongerThan(_undefined);
        expect(_undefined).not.toBeLongerThan('');
      });
    });
  });
});

},{}],58:[function(require,module,exports){
'use strict';

describe('toBeNear', function () {
  describe('when invoked', function () {
    describe('when subject IS a number >= number-epsilon and <= number+epsilon', function () {
      it('should confirm', function () {
        expect(4.23223432434).toBeNear(4, 0.25);
        expect(22).toBeNear(20, 2);
        expect(-42).toBeNear(-40, 2);
      });
    });
    describe('when subject is NOT a number >= number-epsilon and <= number+epsilon', function () {
      it('should deny', function () {
        expect(NaN).not.toBeNear(42, 2);
        expect(4.23223432434).not.toBeNear(4, 0.2);
        expect(22).not.toBeNear(20, 1);
        expect(-42).not.toBeNear(-18, 11);
      });
    });
  });
});

},{}],59:[function(require,module,exports){
'use strict';

var describeWhenNotArray = require('./lib/describeWhenNotArray');

describe('toBeNonEmptyArray', function () {
  describe('when invoked', function () {
    describe('when subject is a true Array', function () {
      describe('when subject has members', function () {
        it('should confirm', function () {
          var _undefined = void 0;
          expect([null]).toBeNonEmptyArray();
          expect([_undefined]).toBeNonEmptyArray();
          expect(['']).toBeNonEmptyArray();
        });
      });
      describe('when subject has no members', function () {
        it('should deny', function () {
          expect([]).not.toBeNonEmptyArray();
        });
      });
    });
    describeWhenNotArray('toBeNonEmptyArray');
  });
});

},{"./lib/describeWhenNotArray":23}],60:[function(require,module,exports){
'use strict';

describe('toBeNonEmptyObject', function () {
  var Foo = void 0;
  beforeEach(function () {
    Foo = function Foo() {};
  });
  describe('when invoked', function () {
    describe('when subject IS an Object with at least one instance member', function () {
      it('should confirm', function () {
        expect({
          a: 1
        }).toBeNonEmptyObject();
      });
    });
    describe('when subject is NOT an Object with at least one instance member', function () {
      beforeEach(function () {
        Foo.prototype = {
          b: 2
        };
      });
      it('should deny', function () {
        expect(new Foo()).not.toBeNonEmptyObject();
        expect({}).not.toBeNonEmptyObject();
        expect(null).not.toBeNonEmptyObject();
      });
    });
  });
});

},{}],61:[function(require,module,exports){
'use strict';

describe('toBeNonEmptyString', function () {
  describe('when invoked', function () {
    describe('when subject IS a string with at least one character', function () {
      it('should confirm', function () {
        expect(' ').toBeNonEmptyString();
      });
    });
    describe('when subject is NOT a string with at least one character', function () {
      it('should deny', function () {
        expect('').not.toBeNonEmptyString();
        expect(null).not.toBeNonEmptyString();
      });
    });
  });
});

},{}],62:[function(require,module,exports){
'use strict';

describe('toBeNumber', function () {
  describe('when invoked', function () {
    describe('when subject IS a number', function () {
      it('should confirm', function () {
        expect(1).toBeNumber();
        expect(1.11).toBeNumber();
        expect(1e3).toBeNumber();
        expect(0.11).toBeNumber();
        expect(-11).toBeNumber();
      });
    });
    describe('when subject is NOT a number', function () {
      it('should deny', function () {
        expect('1').not.toBeNumber();
        expect(NaN).not.toBeNumber();
      });
    });
  });
});

},{}],63:[function(require,module,exports){
'use strict';

describe('toBeObject', function () {
  var Foo = void 0;
  beforeEach(function () {
    Foo = function Foo() {};
  });
  describe('when invoked', function () {
    describe('when subject IS an Object', function () {
      it('should confirm', function () {
        expect(new Object()).toBeObject();
        expect(new Foo()).toBeObject();
        expect({}).toBeObject();
      });
    });
    describe('when subject is NOT an Object', function () {
      it('should deny', function () {
        expect(null).not.toBeObject();
        expect(123).not.toBeObject();
        expect('[object Object]').not.toBeObject();
      });
    });
  });
});

},{}],64:[function(require,module,exports){
'use strict';

describe('toBeOddNumber', function () {
  describe('when invoked', function () {
    describe('when subject IS an odd number', function () {
      it('should confirm', function () {
        expect(1).toBeOddNumber();
      });
    });
    describe('when subject is NOT an odd number', function () {
      it('should deny', function () {
        expect(2).not.toBeOddNumber();
        expect(NaN).not.toBeOddNumber();
      });
    });
  });
});

},{}],65:[function(require,module,exports){
'use strict';

describe('toBeRegExp', function () {
  describe('when invoked', function () {
    describe('when value is an instance of RegExp', function () {
      it('should confirm', function () {
        expect(new RegExp()).toBeRegExp();
        expect(/abc/).toBeRegExp();
      });
    });
    describe('when value is not an instance of RegExp', function () {
      it('should deny', function () {
        expect(null).not.toBeRegExp();
        expect(function () {}).not.toBeRegExp();
        expect('abc').not.toBeRegExp();
      });
    });
  });
});

},{}],66:[function(require,module,exports){
'use strict';

describe('toBeSameLengthAs', function () {
  describe('when invoked', function () {
    describe('when the subject and comparison ARE both strings', function () {
      describe('when the subject IS the same length as the comparision string', function () {
        it('should confirm', function () {
          expect('ab').toBeSameLengthAs('ab');
        });
      });
      describe('when the subject is NOT the same length as the comparision string', function () {
        it('should deny', function () {
          expect('abc').not.toBeSameLengthAs('ab');
          expect('a').not.toBeSameLengthAs('');
          expect('').not.toBeSameLengthAs('a');
        });
      });
    });
    describe('when the subject and comparison are NOT both strings', function () {
      it('should deny (we are asserting the relative lengths of two strings)', function () {
        var _undefined = void 0;
        expect('truthy').not.toBeSameLengthAs(_undefined);
        expect(_undefined).not.toBeSameLengthAs('truthy');
        expect('').not.toBeSameLengthAs(_undefined);
        expect(_undefined).not.toBeSameLengthAs('');
      });
    });
  });
});

},{}],67:[function(require,module,exports){
'use strict';

describe('toBeShorterThan', function () {
  describe('when invoked', function () {
    describe('when the subject and comparison ARE both strings', function () {
      describe('when the subject IS shorter than the comparision string', function () {
        it('should confirm', function () {
          expect('ab').toBeShorterThan('abc');
          expect('').toBeShorterThan('a');
        });
      });
      describe('when the subject is NOT shorter than the comparision string', function () {
        it('should deny', function () {
          expect('abc').not.toBeShorterThan('ab');
          expect('a').not.toBeShorterThan('');
        });
      });
    });
    describe('when the subject and comparison are NOT both strings', function () {
      it('should deny (we are asserting the relative lengths of two strings)', function () {
        var _undefined = void 0;
        expect('truthy').not.toBeShorterThan(_undefined);
        expect(_undefined).not.toBeShorterThan('truthy');
        expect('').not.toBeShorterThan(_undefined);
        expect(_undefined).not.toBeShorterThan('');
      });
    });
  });
});

},{}],68:[function(require,module,exports){
'use strict';

describe('toBeString', function () {
  describe('when invoked', function () {
    describe('when subject IS a string of any length', function () {
      it('should confirm', function () {
        expect('').toBeString();
        expect(' ').toBeString();
      });
    });
    describe('when subject is NOT a string of any length', function () {
      it('should deny', function () {
        expect(null).not.toBeString();
      });
    });
  });
});

},{}],69:[function(require,module,exports){
'use strict';

describe('toBeTrue', function () {
  describe('when invoked', function () {
    describe('when subject is not only truthy, but a boolean true', function () {
      it('should confirm', function () {
        expect(true).toBeTrue();
        expect(new Boolean(true)).toBeTrue();
      });
    });
    describe('when subject is truthy', function () {
      it('should deny', function () {
        expect(1).not.toBeTrue();
      });
    });
  });
});

},{}],70:[function(require,module,exports){
'use strict';

describe('toBeValidDate', function () {
  describe('when invoked', function () {
    describe('when value is a valid instance of Date', function () {
      it('should confirm', function () {
        expect(new Date()).toBeValidDate();
        expect(new Date('November 18, 1985 08:22:00')).toBeValidDate();
        expect(new Date('1985-11-18T08:22:00')).toBeValidDate();
        expect(new Date(1985, 11, 18, 8, 22, 0)).toBeValidDate();
      });
    });
    describe('when value is NOT a valid instance of Date', function () {
      it('should deny', function () {
        expect(null).not.toBeValidDate();
        expect(function () {}).not.toBeValidDate();
        try {
          expect(new Date('')).not.toBeValidDate();
          expect(new Date('invalid')).not.toBeValidDate();
        } catch (err) {
          // ignore "RangeError: Invalid time value" seen only in node.js
        }
      });
    });
  });
});

},{}],71:[function(require,module,exports){
'use strict';

describe('toBeWhitespace', function () {
  describe('when invoked', function () {
    describe('when subject IS a string containing only tabs, spaces, returns etc', function () {
      it('should confirm', function () {
        expect(' ').toBeWhitespace();
        expect('').toBeWhitespace();
      });
    });
    describe('when subject is NOT a string containing only tabs, spaces, returns etc', function () {
      it('should deny', function () {
        expect('has-no-whitespace').not.toBeWhitespace();
        expect('has whitespace').not.toBeWhitespace();
        expect(null).not.toBeWhitespace();
      });
    });
  });
});

},{}],72:[function(require,module,exports){
'use strict';

describe('toBeWholeNumber', function () {
  describe('when invoked', function () {
    describe('when subject IS a number with no positive decimal places', function () {
      it('should confirm', function () {
        expect(1).toBeWholeNumber();
        expect(0).toBeWholeNumber();
        expect(0.0).toBeWholeNumber();
      });
    });
    describe('when subject is NOT a number with no positive decimal places', function () {
      it('should deny', function () {
        expect(NaN).not.toBeWholeNumber();
        expect(1.1).not.toBeWholeNumber();
        expect(0.1).not.toBeWholeNumber();
      });
    });
  });
});

},{}],73:[function(require,module,exports){
'use strict';

describe('toBeWithinRange', function () {
  describe('when invoked', function () {
    describe('when subject IS a number >= floor and <= ceiling', function () {
      it('should confirm', function () {
        expect(0).toBeWithinRange(0, 2);
        expect(1).toBeWithinRange(0, 2);
        expect(2).toBeWithinRange(0, 2);
      });
    });
    describe('when subject is NOT a number >= floor and <= ceiling', function () {
      it('should deny', function () {
        expect(-3).not.toBeWithinRange(0, 2);
        expect(-2).not.toBeWithinRange(0, 2);
        expect(-1).not.toBeWithinRange(0, 2);
        expect(3).not.toBeWithinRange(0, 2);
        expect(NaN).not.toBeWithinRange(0, 2);
      });
    });
  });
});

},{}],74:[function(require,module,exports){
'use strict';

describe('toEndWith', function () {
  describe('when invoked', function () {
    describe('when subject is NOT an undefined or empty string', function () {
      describe('when subject is a string whose trailing characters match the expected string', function () {
        it('should confirm', function () {
          expect('jamie').toEndWith('mie');
        });
      });
      describe('when subject is a string whose trailing characters DO NOT match the expected string', function () {
        it('should deny', function () {
          expect('jamie ').not.toEndWith('mie');
          expect('jamiE').not.toEndWith('mie');
        });
      });
    });
    describe('when subject IS an undefined or empty string', function () {
      it('should deny', function () {
        var _undefined = void 0;
        expect('').not.toEndWith('');
        expect(_undefined).not.toEndWith('');
        expect(_undefined).not.toEndWith('undefined');
        expect('undefined').not.toEndWith(_undefined);
      });
    });
  });
});

},{}],75:[function(require,module,exports){
'use strict';

var describeToHaveArrayX = require('./lib/describeToHaveArrayX');

describe('toHaveArray', function () {
  describeToHaveArrayX('toHaveArray', function () {
    it('should confirm', function () {
      expect({
        memberName: []
      }).toHaveArray('memberName');
      expect({
        memberName: [1, 2, 3]
      }).toHaveArray('memberName');
    });
  });
});

},{"./lib/describeToHaveArrayX":20}],76:[function(require,module,exports){
'use strict';

var describeToHaveArrayX = require('./lib/describeToHaveArrayX');

describe('toHaveArrayOfBooleans', function () {
  describeToHaveArrayX('toHaveArrayOfBooleans', function () {
    describe('when named Array is empty', function () {
      it('should confirm', function () {
        expect({
          memberName: []
        }).toHaveArrayOfBooleans('memberName');
      });
    });
    describe('when named Array has items', function () {
      describe('when all items are booleans', function () {
        it('should confirm', function () {
          expect({
            memberName: [true]
          }).toHaveArrayOfBooleans('memberName');
          expect({
            memberName: [new Boolean(true)]
          }).toHaveArrayOfBooleans('memberName');
          expect({
            memberName: [new Boolean(false)]
          }).toHaveArrayOfBooleans('memberName');
          expect({
            memberName: [false, true]
          }).toHaveArrayOfBooleans('memberName');
        });
      });
      describe('when any item is not a boolean', function () {
        it('should deny', function () {
          expect({
            memberName: [null]
          }).not.toHaveArrayOfBooleans('memberName');
          expect({
            memberName: [null, false]
          }).not.toHaveArrayOfBooleans('memberName');
        });
      });
    });
  });
});

},{"./lib/describeToHaveArrayX":20}],77:[function(require,module,exports){
'use strict';

var describeToHaveArrayX = require('./lib/describeToHaveArrayX');

describe('toHaveArrayOfNumbers', function () {
  describeToHaveArrayX('toHaveArrayOfNumbers', function () {
    describe('when named Array is empty', function () {
      it('should confirm', function () {
        expect({
          memberName: []
        }).toHaveArrayOfNumbers('memberName');
      });
    });
    describe('when named Array has items', function () {
      describe('when all items are numbers', function () {
        it('should confirm', function () {
          expect({
            memberName: [1]
          }).toHaveArrayOfNumbers('memberName');
          expect({
            memberName: [new Number(1)]
          }).toHaveArrayOfNumbers('memberName');
          expect({
            memberName: [new Number(0)]
          }).toHaveArrayOfNumbers('memberName');
          expect({
            memberName: [0, 1]
          }).toHaveArrayOfNumbers('memberName');
        });
      });
      describe('when any item is not a number', function () {
        it('should deny', function () {
          expect({
            memberName: [null]
          }).not.toHaveArrayOfNumbers('memberName');
          expect({
            memberName: [null, 0]
          }).not.toHaveArrayOfNumbers('memberName');
        });
      });
    });
  });
});

},{"./lib/describeToHaveArrayX":20}],78:[function(require,module,exports){
'use strict';

var describeToHaveArrayX = require('./lib/describeToHaveArrayX');

describe('toHaveArrayOfObjects', function () {
  describeToHaveArrayX('toHaveArrayOfObjects', function () {
    describe('when named Array is empty', function () {
      it('should confirm', function () {
        expect({
          memberName: []
        }).toHaveArrayOfObjects('memberName');
      });
    });
    describe('when named Array has items', function () {
      describe('when all items are objects', function () {
        it('should confirm', function () {
          expect({
            memberName: [{}]
          }).toHaveArrayOfObjects('memberName');
          expect({
            memberName: [{}, {}]
          }).toHaveArrayOfObjects('memberName');
        });
      });
      describe('when any item is not an object', function () {
        it('should deny', function () {
          expect({
            memberName: [null]
          }).not.toHaveArrayOfObjects('memberName');
          expect({
            memberName: [null, {}]
          }).not.toHaveArrayOfObjects('memberName');
        });
      });
    });
  });
});

},{"./lib/describeToHaveArrayX":20}],79:[function(require,module,exports){
'use strict';

var describeToHaveArrayX = require('./lib/describeToHaveArrayX');

describe('toHaveArrayOfSize', function () {
  describeToHaveArrayX('toHaveArrayOfSize', function () {
    describe('when number of expected items does not match', function () {
      it('should deny', function () {
        expect({
          memberName: ''
        }).not.toHaveArrayOfSize('memberName');
        expect({
          memberName: ['bar']
        }).not.toHaveArrayOfSize('memberName', 0);
      });
    });
    describe('when number of expected items does match', function () {
      it('should confirm', function () {
        expect({
          memberName: []
        }).toHaveArrayOfSize('memberName', 0);
        expect({
          memberName: ['bar']
        }).toHaveArrayOfSize('memberName', 1);
        expect({
          memberName: ['bar', 'baz']
        }).toHaveArrayOfSize('memberName', 2);
      });
    });
  });
});

},{"./lib/describeToHaveArrayX":20}],80:[function(require,module,exports){
'use strict';

var describeToHaveArrayX = require('./lib/describeToHaveArrayX');

describe('toHaveArrayOfStrings', function () {
  describeToHaveArrayX('toHaveArrayOfStrings', function () {
    describe('when named Array is empty', function () {
      it('should confirm', function () {
        expect({
          memberName: []
        }).toHaveArrayOfStrings('memberName');
      });
    });
    describe('when named Array has items', function () {
      describe('when all items are strings', function () {
        it('should confirm', function () {
          expect({
            memberName: ['truthy']
          }).toHaveArrayOfStrings('memberName');
          expect({
            memberName: [new String('truthy')]
          }).toHaveArrayOfStrings('memberName');
          expect({
            memberName: [new String('')]
          }).toHaveArrayOfStrings('memberName');
          expect({
            memberName: ['', 'truthy']
          }).toHaveArrayOfStrings('memberName');
        });
      });
      describe('when any item is not a string', function () {
        it('should deny', function () {
          expect({
            memberName: [null]
          }).not.toHaveArrayOfStrings('memberName');
          expect({
            memberName: [null, '']
          }).not.toHaveArrayOfStrings('memberName');
        });
      });
    });
  });
});

},{"./lib/describeToHaveArrayX":20}],81:[function(require,module,exports){
'use strict';

var describeToHaveBooleanX = require('./lib/describeToHaveBooleanX');

describe('toHaveBoolean', function () {
  describeToHaveBooleanX('toHaveBoolean', function () {
    describe('when primitive', function () {
      it('should confirm', function () {
        expect({
          memberName: true
        }).toHaveBoolean('memberName');
        expect({
          memberName: false
        }).toHaveBoolean('memberName');
      });
    });
    describe('when Boolean object', function () {
      it('should confirm', function () {
        expect({
          memberName: new Boolean(true)
        }).toHaveBoolean('memberName');
        expect({
          memberName: new Boolean(false)
        }).toHaveBoolean('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveBooleanX":21}],82:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveCalculable', function () {
  describeToHaveX('toHaveCalculable', function () {
    describe('when subject CAN be coerced to be used in mathematical operations', function () {
      it('should confirm', function () {
        expect({
          memberName: '1'
        }).toHaveCalculable('memberName');
        expect({
          memberName: ''
        }).toHaveCalculable('memberName');
        expect({
          memberName: null
        }).toHaveCalculable('memberName');
      });
    });
    describe('when subject can NOT be coerced by JavaScript to be used in mathematical operations', function () {
      it('should deny', function () {
        expect({
          memberName: {}
        }).not.toHaveCalculable('memberName');
        expect({
          memberName: NaN
        }).not.toHaveCalculable('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],83:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveDate', function () {
  var mockDate = void 0;
  beforeEach(function () {
    mockDate = {
      any: new Date(),
      early: new Date('2013-01-01T00:00:00.000Z'),
      late: new Date('2013-01-01T01:00:00.000Z')
    };
  });
  describeToHaveX('toHaveDate', function () {
    describe('when member is an instance of Date', function () {
      it('should confirm', function () {
        expect({
          memberName: mockDate.any
        }).toHaveDate('memberName');
      });
    });
    describe('when member is NOT an instance of Date', function () {
      it('should deny', function () {
        expect({
          memberName: null
        }).not.toHaveDate('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],84:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveDateAfter', function () {
  var mockDate = void 0;
  beforeEach(function () {
    mockDate = {
      any: new Date(),
      early: new Date('2013-01-01T00:00:00.000Z'),
      late: new Date('2013-01-01T01:00:00.000Z')
    };
  });
  describeToHaveX('toHaveDateAfter', function () {
    describe('when member is an instance of Date', function () {
      describe('when date occurs before another', function () {
        it('should confirm', function () {
          expect({
            memberName: mockDate.late
          }).toHaveDateAfter('memberName', mockDate.early);
        });
      });
      describe('when date does NOT occur before another', function () {
        it('should deny', function () {
          expect({
            memberName: mockDate.early
          }).not.toHaveDateAfter('memberName', mockDate.late);
        });
      });
    });
    describe('when member is NOT an instance of Date', function () {
      it('should deny', function () {
        expect({
          memberName: null
        }).not.toHaveDateAfter('memberName', mockDate.any);
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],85:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describeToHaveX('toHaveDateBefore', function () {
  var mockDate = void 0;
  beforeEach(function () {
    mockDate = {
      any: new Date(),
      early: new Date('2013-01-01T00:00:00.000Z'),
      late: new Date('2013-01-01T01:00:00.000Z')
    };
  });
  describe('when member is an instance of Date', function () {
    describe('when date occurs before another', function () {
      it('should confirm', function () {
        expect({
          memberName: mockDate.early
        }).toHaveDateBefore('memberName', mockDate.late);
      });
    });
    describe('when date does NOT occur before another', function () {
      it('should deny', function () {
        expect({
          memberName: mockDate.late
        }).not.toHaveDateBefore('memberName', mockDate.early);
      });
    });
  });
  describe('when member is NOT an instance of Date', function () {
    it('should deny', function () {
      expect({
        memberName: null
      }).not.toHaveDateBefore('memberName', mockDate.any);
    });
  });
});

},{"./lib/describeToHaveX":22}],86:[function(require,module,exports){
'use strict';

var describeToHaveArrayX = require('./lib/describeToHaveArrayX');

describe('toHaveEmptyArray', function () {
  describeToHaveArrayX('toHaveEmptyArray', function () {
    describe('when named array has members', function () {
      it('should deny', function () {
        expect({
          memberName: [1, 2, 3]
        }).not.toHaveEmptyArray('memberName');
        expect({
          memberName: ''
        }).not.toHaveEmptyArray('memberName');
      });
    });
    describe('when named array has no members', function () {
      it('should confirm', function () {
        expect({
          memberName: []
        }).toHaveEmptyArray('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveArrayX":20}],87:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveEmptyObject', function () {
  var Foo = void 0;
  beforeEach(function () {
    Foo = function Foo() {};
  });
  describeToHaveX('toHaveEmptyObject', function () {
    describe('when subject IS an Object with no instance members', function () {
      beforeEach(function () {
        Foo.prototype = {
          b: 2
        };
      });
      it('should confirm', function () {
        expect({
          memberName: new Foo()
        }).toHaveEmptyObject('memberName');
        expect({
          memberName: {}
        }).toHaveEmptyObject('memberName');
      });
    });
    describe('when subject is NOT an Object with no instance members', function () {
      it('should deny', function () {
        expect({
          memberName: {
            a: 1
          }
        }).not.toHaveEmptyObject('memberName');
        expect({
          memberName: null
        }).not.toHaveNonEmptyObject('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],88:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveEmptyString', function () {
  describeToHaveX('toHaveEmptyString', function () {
    describe('when subject IS a string with no characters', function () {
      it('should confirm', function () {
        expect({
          memberName: ''
        }).toHaveEmptyString('memberName');
      });
    });
    describe('when subject is NOT a string with no characters', function () {
      it('should deny', function () {
        expect({
          memberName: ' '
        }).not.toHaveEmptyString('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],89:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveEvenNumber', function () {
  describeToHaveX('toHaveEvenNumber', function () {
    describe('when subject IS an even number', function () {
      it('should confirm', function () {
        expect({
          memberName: 2
        }).toHaveEvenNumber('memberName');
      });
    });
    describe('when subject is NOT an even number', function () {
      it('should deny', function () {
        expect({
          memberName: 1
        }).not.toHaveEvenNumber('memberName');
        expect({
          memberName: NaN
        }).not.toHaveEvenNumber('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],90:[function(require,module,exports){
'use strict';

var describeToHaveBooleanX = require('./lib/describeToHaveBooleanX');

describe('toHaveFalse', function () {
  describeToHaveBooleanX('toHaveFalse', function () {
    describe('when primitive', function () {
      describe('when true', function () {
        it('should deny', function () {
          expect({
            memberName: true
          }).not.toHaveFalse('memberName');
        });
      });
      describe('when false', function () {
        it('should confirm', function () {
          expect({
            memberName: false
          }).toHaveFalse('memberName');
        });
      });
    });
    describe('when Boolean object', function () {
      describe('when true', function () {
        it('should deny', function () {
          expect({
            memberName: new Boolean(true)
          }).not.toHaveFalse('memberName');
        });
      });
      describe('when false', function () {
        it('should confirm', function () {
          expect({
            memberName: new Boolean(false)
          }).toHaveFalse('memberName');
        });
      });
    });
  });
});

},{"./lib/describeToHaveBooleanX":21}],91:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveHtmlString', function () {
  describeToHaveX('toHaveHtmlString', function () {
    describe('when subject IS a string of HTML markup', function () {
      var ngMultiLine = void 0;
      beforeEach(function () {
        ngMultiLine = '';
        ngMultiLine += '<a data-ng-href="//www.google.com" data-ng-click="launchApp($event)" target="_blank" class="ng-binding" href="//www.google.com">';
        ngMultiLine += '\n';
        ngMultiLine += '  Watch with Google TV';
        ngMultiLine += '\n';
        ngMultiLine += '</a>';
        ngMultiLine += '\n';
      });
      it('should confirm', function () {
        expect({
          memberName: '<element>text</element>'
        }).toHaveHtmlString('memberName');
        expect({
          memberName: '<a data-ng-href="//foo.com" data-ng-click="bar($event)">baz</a>'
        }).toHaveHtmlString('memberName');
        expect({
          memberName: '<div ng-if="foo > bar || bar < foo && baz == bar"></div>'
        }).toHaveHtmlString('memberName');
        expect({
          memberName: '<li ng-if="foo > bar || bar < foo && baz == bar">'
        }).toHaveHtmlString('memberName');
        expect({
          memberName: ngMultiLine
        }).toHaveHtmlString('memberName');
      });
    });
    describe('when subject is NOT a string of HTML markup', function () {
      it('should deny', function () {
        expect({
          memberName: 'div'
        }).not.toHaveHtmlString('memberName');
        expect({
          memberName: null
        }).not.toHaveHtmlString('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],92:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveIso8601', function () {
  describeToHaveX('toHaveIso8601', function () {
    describe('when member is a Date String conforming to the ISO 8601 standard', function () {
      describe('when specified date is valid', function () {
        it('should confirm', function () {
          expect({
            memberName: '2013-07-08T07:29:15.863Z'
          }).toHaveIso8601('memberName');
          expect({
            memberName: '2013-07-08T07:29:15.863'
          }).toHaveIso8601('memberName');
          expect({
            memberName: '2013-07-08T07:29:15'
          }).toHaveIso8601('memberName');
          expect({
            memberName: '2013-07-08T07:29'
          }).toHaveIso8601('memberName');
          expect({
            memberName: '2013-07-08'
          }).toHaveIso8601('memberName');
        });
      });
      describe('when specified date is NOT valid', function () {
        it('should deny', function () {
          expect({
            memberName: '2013-99-12T00:00:00.000Z'
          }).not.toHaveIso8601('memberName');
          expect({
            memberName: '2013-12-99T00:00:00.000Z'
          }).not.toHaveIso8601('memberName');
          expect({
            memberName: '2013-01-01T99:00:00.000Z'
          }).not.toHaveIso8601('memberName');
          expect({
            memberName: '2013-01-01T99:99:00.000Z'
          }).not.toHaveIso8601('memberName');
          expect({
            memberName: '2013-01-01T00:00:99.000Z'
          }).not.toHaveIso8601('memberName');
        });
      });
    });
    describe('when member is a String NOT conforming to the ISO 8601 standard', function () {
      it('should deny', function () {
        expect({
          memberName: '2013-07-08T07:29:15.'
        }).not.toHaveIso8601('memberName');
        expect({
          memberName: '2013-07-08T07:29:'
        }).not.toHaveIso8601('memberName');
        expect({
          memberName: '2013-07-08T07:2'
        }).not.toHaveIso8601('memberName');
        expect({
          memberName: '2013-07-08T07:'
        }).not.toHaveIso8601('memberName');
        expect({
          memberName: '2013-07-08T07'
        }).not.toHaveIso8601('memberName');
        expect({
          memberName: '2013-07-08T'
        }).not.toHaveIso8601('memberName');
        expect({
          memberName: '2013-07-0'
        }).not.toHaveIso8601('memberName');
        expect({
          memberName: '2013-07-'
        }).not.toHaveIso8601('memberName');
        expect({
          memberName: '2013-07'
        }).not.toHaveIso8601('memberName');
        expect({
          memberName: '2013-0'
        }).not.toHaveIso8601('memberName');
        expect({
          memberName: '2013-'
        }).not.toHaveIso8601('memberName');
        expect({
          memberName: '2013'
        }).not.toHaveIso8601('memberName');
        expect({
          memberName: '201'
        }).not.toHaveIso8601('memberName');
        expect({
          memberName: '20'
        }).not.toHaveIso8601('memberName');
        expect({
          memberName: '2'
        }).not.toHaveIso8601('memberName');
        expect({
          memberName: ''
        }).not.toHaveIso8601('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],93:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveJsonString', function () {
  describeToHaveX('toHaveJsonString', function () {
    describe('when subject IS a string of parseable JSON', function () {
      it('should confirm', function () {
        expect({
          memberName: '{}'
        }).toHaveJsonString('memberName');
        expect({
          memberName: '[]'
        }).toHaveJsonString('memberName');
        expect({
          memberName: '[1]'
        }).toHaveJsonString('memberName');
      });
    });
    describe('when subject is NOT a string of parseable JSON', function () {
      it('should deny', function () {
        var _undefined = void 0;
        expect({
          memberName: '[1,]'
        }).not.toHaveJsonString('memberName');
        expect({
          memberName: '<>'
        }).not.toHaveJsonString('memberName');
        expect({
          memberName: null
        }).not.toHaveJsonString('memberName');
        expect({
          memberName: ''
        }).not.toHaveJsonString('memberName');
        expect({
          memberName: _undefined
        }).not.toHaveJsonString('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],94:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveMember', function () {
  describeToHaveX('toHaveMember', function () {});
});

},{"./lib/describeToHaveX":22}],95:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveMethod', function () {
  describeToHaveX('toHaveMethod', function () {
    describe('when subject IS a function', function () {
      it('should confirm', function () {
        expect({
          memberName: function memberName() {}
        }).toHaveMethod('memberName');
      });
    });
    describe('when subject is NOT a function', function () {
      it('should deny', function () {
        expect({
          memberName: /regexp/
        }).not.toHaveMethod('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],96:[function(require,module,exports){
'use strict';

var describeToHaveArrayX = require('./lib/describeToHaveArrayX');

describe('toHaveNonEmptyArray', function () {
  describeToHaveArrayX('toHaveNonEmptyArray', function () {
    describe('when named array has no members', function () {
      it('should deny', function () {
        expect({
          memberName: []
        }).not.toHaveNonEmptyArray('memberName');
      });
    });
    describe('when named array has members', function () {
      it('should confirm', function () {
        expect({
          memberName: [1, 2, 3]
        }).toHaveNonEmptyArray('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveArrayX":20}],97:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveNonEmptyObject', function () {
  describeToHaveX('toHaveNonEmptyObject', function () {
    var Foo = void 0;
    beforeEach(function () {
      Foo = function Foo() {};
    });
    describe('when subject IS an Object with at least one instance member', function () {
      it('should confirm', function () {
        expect({
          memberName: {
            a: 1
          }
        }).toHaveNonEmptyObject('memberName');
      });
    });
    describe('when subject is NOT an Object with at least one instance member', function () {
      beforeEach(function () {
        Foo.prototype = {
          b: 2
        };
      });
      it('should deny', function () {
        expect({
          memberName: new Foo()
        }).not.toHaveNonEmptyObject('memberName');
        expect({
          memberName: {}
        }).not.toHaveNonEmptyObject('memberName');
        expect({
          memberName: null
        }).not.toHaveNonEmptyObject('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],98:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveNonEmptyString', function () {
  describeToHaveX('toHaveNonEmptyString', function () {
    describe('when subject IS a string with at least one character', function () {
      it('should confirm', function () {
        expect({
          memberName: ' '
        }).toHaveNonEmptyString('memberName');
      });
    });
    describe('when subject is NOT a string with at least one character', function () {
      it('should deny', function () {
        expect({
          memberName: ''
        }).not.toHaveNonEmptyString('memberName');
        expect({
          memberName: null
        }).not.toHaveNonEmptyString('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],99:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveNumber', function () {
  describeToHaveX('toHaveNumber', function () {
    describe('when subject IS a number', function () {
      it('should confirm', function () {
        expect({
          memberName: 1
        }).toHaveNumber('memberName');
        expect({
          memberName: 1.11
        }).toHaveNumber('memberName');
        expect({
          memberName: 1e3
        }).toHaveNumber('memberName');
        expect({
          memberName: 0.11
        }).toHaveNumber('memberName');
        expect({
          memberName: -11
        }).toHaveNumber('memberName');
      });
    });
    describe('when subject is NOT a number', function () {
      it('should deny', function () {
        expect({
          memberName: '1'
        }).not.toHaveNumber('memberName');
        expect({
          memberName: NaN
        }).not.toHaveNumber('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],100:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveNumberWithinRange', function () {
  describeToHaveX('toHaveNumberWithinRange', function () {
    describe('when subject IS a number >= floor and <= ceiling', function () {
      it('should confirm', function () {
        expect({
          memberName: 0
        }).toHaveNumberWithinRange('memberName', 0, 2);
        expect({
          memberName: 1
        }).toHaveNumberWithinRange('memberName', 0, 2);
        expect({
          memberName: 2
        }).toHaveNumberWithinRange('memberName', 0, 2);
      });
    });
    describe('when subject is NOT a number >= floor and <= ceiling', function () {
      it('should deny', function () {
        expect({
          memberName: -3
        }).not.toHaveNumberWithinRange('memberName', 0, 2);
        expect({
          memberName: -2
        }).not.toHaveNumberWithinRange('memberName', 0, 2);
        expect({
          memberName: -1
        }).not.toHaveNumberWithinRange('memberName', 0, 2);
        expect({
          memberName: 3
        }).not.toHaveNumberWithinRange('memberName', 0, 2);
        expect({
          memberName: NaN
        }).not.toHaveNumberWithinRange('memberName', 0, 2);
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],101:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveObject', function () {
  describeToHaveX('toHaveObject', function () {
    var Foo = void 0;
    beforeEach(function () {
      Foo = function Foo() {};
    });
    describe('when subject IS an Object', function () {
      it('should confirm', function () {
        expect({
          memberName: new Object()
        }).toHaveObject('memberName');
        expect({
          memberName: new Foo()
        }).toHaveObject('memberName');
        expect({
          memberName: {}
        }).toHaveObject('memberName');
      });
    });
    describe('when subject is NOT an Object', function () {
      it('should deny', function () {
        expect({
          memberName: null
        }).not.toHaveObject('memberName');
        expect({
          memberName: 123
        }).not.toHaveObject('memberName');
        expect({
          memberName: '[object Object]'
        }).not.toHaveObject('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],102:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveOddNumber', function () {
  describeToHaveX('toHaveOddNumber', function () {
    describe('when subject IS an odd number', function () {
      it('should confirm', function () {
        expect({
          memberName: 1
        }).toHaveOddNumber('memberName');
      });
    });
    describe('when subject is NOT an odd number', function () {
      it('should deny', function () {
        expect({
          memberName: 2
        }).not.toHaveOddNumber('memberName');
        expect({
          memberName: NaN
        }).not.toHaveOddNumber('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],103:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveString', function () {
  describeToHaveX('toHaveString', function () {
    describe('when subject IS a string of any length', function () {
      it('should confirm', function () {
        expect({
          memberName: ''
        }).toHaveString('memberName');
        expect({
          memberName: ' '
        }).toHaveString('memberName');
      });
    });
    describe('when subject is NOT a string of any length', function () {
      it('should deny', function () {
        expect({
          memberName: null
        }).not.toHaveString('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],104:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveStringLongerThan', function () {
  describeToHaveX('toHaveStringLongerThan', function () {
    describe('when the subject and comparison ARE both strings', function () {
      describe('when the subject IS longer than the comparision string', function () {
        it('should confirm', function () {
          expect({
            memberName: 'abc'
          }).toHaveStringLongerThan('memberName', 'ab');
          expect({
            memberName: 'a'
          }).toHaveStringLongerThan('memberName', '');
        });
      });
      describe('when the subject is NOT longer than the comparision string', function () {
        it('should deny', function () {
          expect({
            memberName: 'ab'
          }).not.toHaveStringLongerThan('memberName', 'abc');
          expect({
            memberName: ''
          }).not.toHaveStringLongerThan('memberName', 'a');
        });
      });
    });
    describe('when the subject and comparison are NOT both strings', function () {
      it('should deny (we are asserting the relative lengths of two strings)', function () {
        var _undefined = void 0;
        expect({
          memberName: 'truthy'
        }).not.toHaveStringLongerThan('memberName', _undefined);
        expect({
          memberName: _undefined
        }).not.toHaveStringLongerThan('memberName', 'truthy');
        expect({
          memberName: ''
        }).not.toHaveStringLongerThan('memberName', _undefined);
        expect({
          memberName: _undefined
        }).not.toHaveStringLongerThan('memberName', '');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],105:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveStringSameLengthAs', function () {
  describeToHaveX('toHaveStringSameLengthAs', function () {
    describe('when the subject and comparison ARE both strings', function () {
      describe('when the subject IS the same length as the comparision string', function () {
        it('should confirm', function () {
          expect({
            memberName: 'ab'
          }).toHaveStringSameLengthAs('memberName', 'ab');
        });
      });
      describe('when the subject is NOT the same length as the comparision string', function () {
        it('should deny', function () {
          expect({
            memberName: 'abc'
          }).not.toHaveStringSameLengthAs('memberName', 'ab');
          expect({
            memberName: 'a'
          }).not.toHaveStringSameLengthAs('memberName', '');
          expect({
            memberName: ''
          }).not.toHaveStringSameLengthAs('memberName', 'a');
        });
      });
    });
    describe('when the subject and comparison are NOT both strings', function () {
      it('should deny (we are asserting the relative lengths of two strings)', function () {
        var _undefined = void 0;
        expect({
          memberName: 'truthy'
        }).not.toHaveStringSameLengthAs('memberName', _undefined);
        expect({
          memberName: _undefined
        }).not.toHaveStringSameLengthAs('memberName', 'truthy');
        expect({
          memberName: ''
        }).not.toHaveStringSameLengthAs('memberName', _undefined);
        expect({
          memberName: _undefined
        }).not.toHaveStringSameLengthAs('memberName', '');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],106:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveStringShorterThan', function () {
  describeToHaveX('toHaveStringShorterThan', function () {
    describe('when the subject and comparison ARE both strings', function () {
      describe('when the subject IS shorter than the comparision string', function () {
        it('should confirm', function () {
          expect({
            memberName: 'ab'
          }).toHaveStringShorterThan('memberName', 'abc');
          expect({
            memberName: ''
          }).toHaveStringShorterThan('memberName', 'a');
        });
      });
      describe('when the subject is NOT shorter than the comparision string', function () {
        it('should deny', function () {
          expect({
            memberName: 'abc'
          }).not.toHaveStringShorterThan('memberName', 'ab');
          expect({
            memberName: 'a'
          }).not.toHaveStringShorterThan('memberName', '');
        });
      });
    });
    describe('when the subject and comparison are NOT both strings', function () {
      it('should deny (we are asserting the relative lengths of two strings)', function () {
        var _undefined = void 0;
        expect({
          memberName: 'truthy'
        }).not.toHaveStringShorterThan('memberName', _undefined);
        expect({
          memberName: _undefined
        }).not.toHaveStringShorterThan('memberName', 'truthy');
        expect({
          memberName: ''
        }).not.toHaveStringShorterThan('memberName', _undefined);
        expect({
          memberName: _undefined
        }).not.toHaveStringShorterThan('memberName', '');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],107:[function(require,module,exports){
'use strict';

var describeToHaveBooleanX = require('./lib/describeToHaveBooleanX');

describe('toHaveTrue', function () {
  describeToHaveBooleanX('toHaveTrue', function () {
    describe('when primitive', function () {
      describe('when true', function () {
        it('should confirm', function () {
          expect({
            memberName: true
          }).toHaveTrue('memberName');
        });
      });
      describe('when false', function () {
        it('should deny', function () {
          expect({
            memberName: false
          }).not.toHaveTrue('memberName');
        });
      });
    });
    describe('when Boolean object', function () {
      describe('when true', function () {
        it('should confirm', function () {
          expect({
            memberName: new Boolean(true)
          }).toHaveTrue('memberName');
        });
      });
      describe('when false', function () {
        it('should deny', function () {
          expect({
            memberName: new Boolean(false)
          }).not.toHaveTrue('memberName');
        });
      });
    });
  });
});

},{"./lib/describeToHaveBooleanX":21}],108:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveUndefined', function () {
  describeToHaveX('toHaveUndefined', function () {
    describe('when subject does NOT have a member at the given key', function () {
      it('should deny', function () {
        expect({}).not.toHaveUndefined('memberName');
        expect(null).not.toHaveUndefined('memberName');
      });
    });
    describe('when subject DOES have a member at the given key', function () {
      describe('when subject IS undefined', function () {
        it('should confirm', function () {
          expect({
            memberName: undefined
          }).toHaveUndefined('memberName');
        });
      });
      describe('when subject is NOT undefined', function () {
        it('should deny', function () {
          expect({
            memberName: null
          }).not.toHaveUndefined('memberName');
          expect({
            memberName: 'undefined'
          }).not.toHaveUndefined('memberName');
        });
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],109:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveWhitespaceString', function () {
  describeToHaveX('toHaveWhitespaceString', function () {
    describe('when subject IS a string containing only tabs, spaces, returns etc', function () {
      it('should confirm', function () {
        expect({
          memberName: ' '
        }).toHaveWhitespaceString('memberName');
        expect({
          memberName: ''
        }).toHaveWhitespaceString('memberName');
      });
    });
    describe('when subject is NOT a string containing only tabs, spaces, returns etc', function () {
      it('should deny', function () {
        expect({
          memberName: 'has-no-whitespace'
        }).not.toHaveWhitespaceString('memberName');
        expect({
          memberName: 'has whitespace'
        }).not.toHaveWhitespaceString('memberName');
        expect({
          memberName: null
        }).not.toHaveWhitespaceString('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],110:[function(require,module,exports){
'use strict';

var describeToHaveX = require('./lib/describeToHaveX');

describe('toHaveWholeNumber', function () {
  describeToHaveX('toHaveWholeNumber', function () {
    describe('when subject IS a number with no positive decimal places', function () {
      it('should confirm', function () {
        expect({
          memberName: 1
        }).toHaveWholeNumber('memberName');
        expect({
          memberName: 0
        }).toHaveWholeNumber('memberName');
        expect({
          memberName: 0.0
        }).toHaveWholeNumber('memberName');
      });
    });
    describe('when subject is NOT a number with no positive decimal places', function () {
      it('should deny', function () {
        expect({
          memberName: NaN
        }).not.toHaveWholeNumber('memberName');
        expect({
          memberName: 1.1
        }).not.toHaveWholeNumber('memberName');
        expect({
          memberName: 0.1
        }).not.toHaveWholeNumber('memberName');
      });
    });
  });
});

},{"./lib/describeToHaveX":22}],111:[function(require,module,exports){
'use strict';

describe('toStartWith', function () {
  describe('when invoked', function () {
    describe('when subject is NOT an undefined or empty string', function () {
      describe('when subject is a string whose leading characters match the expected string', function () {
        it('should confirm', function () {
          expect('jamie').toStartWith('jam');
        });
      });
      describe('when subject is a string whose leading characters DO NOT match the expected string', function () {
        it('should deny', function () {
          expect(' jamie').not.toStartWith('jam');
          expect('Jamie').not.toStartWith('jam');
        });
      });
    });
    describe('when subject IS an undefined or empty string', function () {
      it('should deny', function () {
        var _undefined = void 0;
        expect('').not.toStartWith('');
        expect(_undefined).not.toStartWith('');
        expect(_undefined).not.toStartWith('undefined');
        expect('undefined').not.toStartWith(_undefined);
      });
    });
  });
});

},{}],112:[function(require,module,exports){
'use strict';

describe('toThrowAnyError', function () {
  describe('when supplied a function', function () {
    describe('when function errors when invoked', function () {
      var throwError = void 0;
      var badReference = void 0;
      beforeEach(function () {
        throwError = function throwError() {
          throw new Error('wut?');
        };
        badReference = function badReference() {
          return doesNotExist.someValue; // eslint-disable-line no-undef
        };
      });
      it('should confirm', function () {
        expect(throwError).toThrowAnyError();
        expect(badReference).toThrowAnyError();
      });
    });
    describe('when function does NOT error when invoked', function () {
      var noErrors = void 0;
      beforeEach(function () {
        noErrors = function noErrors() {};
      });
      it('should deny', function () {
        expect(noErrors).not.toThrowAnyError();
      });
    });
  });
});

},{}],113:[function(require,module,exports){
'use strict';

describe('toThrowErrorOfType', function () {
  describe('when supplied a function', function () {
    describe('when function errors when invoked', function () {
      var throwError = void 0;
      var badReference = void 0;
      beforeEach(function () {
        throwError = function throwError() {
          throw new Error('wut?');
        };
        badReference = function badReference() {
          return doesNotExist.someValue; // eslint-disable-line no-undef
        };
      });
      describe('when the error is of the expected type', function () {
        it('should confirm', function () {
          expect(throwError).toThrowErrorOfType('Error');
          expect(badReference).toThrowErrorOfType('ReferenceError');
        });
      });
      describe('when the error is NOT of the expected type', function () {
        it('should confirm', function () {
          expect(throwError).not.toThrowErrorOfType('ReferenceError');
          expect(badReference).not.toThrowErrorOfType('Error');
        });
      });
    });
  });
});

},{}],114:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.whitespace', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy(' \n\t ');
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.whitespace());
  });
});

},{"./lib/callSpy":18}],115:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.wholeNumber', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy(15);
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.wholeNumber());
  });
});

},{"./lib/callSpy":18}],116:[function(require,module,exports){
'use strict';

var callSpy = require('./lib/callSpy');

describe('any.withinRange', function () {
  var shared = {};
  beforeEach(function () {
    shared.spy = callSpy(11);
  });
  it('should confirm', function () {
    expect(shared.spy).toHaveBeenCalledWith(any.withinRange(10, 15));
  });
});

},{"./lib/callSpy":18}]},{},[14]);
