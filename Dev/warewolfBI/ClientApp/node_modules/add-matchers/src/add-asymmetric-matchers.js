// public
module.exports = addAsymmetricMatchers;

// implementation
function addAsymmetricMatchers(matchersByName) {
  /* eslint guard-for-in: 0 */
  global.any = global.any || {};
  for (var name in matchersByName) {
    addAsymmetricMatcher(name, matchersByName[name]);
  }
}

function addAsymmetricMatcher(name, matcher) {
  global.any[name] = function () {
    var args = [].slice.call(arguments);
    return {
      asymmetricMatch: function (actual) {
        var clone = args.slice();
        clone.push(actual);
        return matcher.apply(this, clone);
      }
    };
  };
}
