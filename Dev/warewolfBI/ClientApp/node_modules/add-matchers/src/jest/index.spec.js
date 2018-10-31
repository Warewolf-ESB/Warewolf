describe('jest adapters', () => {
  describe('getAdapters(globals)', () => {
    beforeEach(() => {
      this.framework = require('./index');
      this.wrappedMatchersByName = null;
      this.jestMatcherScope = {
        utils: {
          printExpected: jasmine.createSpy('this.utils.printExpected')
            .and.callFake(value => 'printExpected(' + value + ')'),
          printReceived: jasmine.createSpy('this.utils.printReceived')
            .and.callFake(value => 'printReceived(' + value + ')')
        }
      };
      this.globals = {
        jest: {},
        expect: {
          extend: jasmine.createSpy('expect.extend')
            .and.callFake(wrappedMatchersByName => {
              this.wrappedMatchersByName = wrappedMatchersByName;
            })
        }
      };
      this.adapters = this.framework.getAdapters(this.globals);
    });

    it('should return a set of adapters keyed by the number of arguments they expect', () => {
      expect(this.adapters).toEqual({
        1: jasmine.any(Function),
        2: jasmine.any(Function),
        3: jasmine.any(Function),
        4: jasmine.any(Function)
      });
    });

    describe('toBe* matchers ', () => {
      describe('adapter for zero argument matchers', () => {
        describeAdapter(this, 1, () => {
          describe('when the matcher is used', () => {
            beforeEach(() => {
              this.result = this.wrappedMatchersByName.toBeFoo.call(this.jestMatcherScope, 'received');
            });
            it('should call the matcher correctly', () => {
              expect(this.toBeFoo).toHaveBeenCalledWith('received');
            });
            it('should correctly state whether the test passed', () => {
              expect(this.result.pass).toEqual(false);
            });
            it('should return a useful error message', () => {
              expect(this.result.message()).toEqual('expected printReceived(received) to be foo');
            });
          });
        });
      });

      describe('adapter for single argument matchers', () => {
        describeAdapter(this, 2, () => {
          describe('when the matcher is used', () => {
            beforeEach(() => {
              this.result = this.wrappedMatchersByName.toBeFoo.call(this.jestMatcherScope, 'received', 'expected');
            });
            it('should call the matcher correctly', () => {
              expect(this.toBeFoo).toHaveBeenCalledWith('expected', 'received');
            });
            it('should correctly state whether the test passed', () => {
              expect(this.result.pass).toEqual(false);
            });
            it('should return a useful error message', () => {
              expect(this.result.message()).toEqual('expected printReceived(received) to be foo printExpected(expected)');
            });
          });
        });
      });

      describe('adapter for two argument matchers', () => {
        describeAdapter(this, 3, () => {
          describe('when the matcher is used', () => {
            beforeEach(() => {
              this.result = this.wrappedMatchersByName.toBeFoo.call(this.jestMatcherScope, 'received', 'expected 1', 'expected 2');
            });
            it('should call the matcher correctly', () => {
              expect(this.toBeFoo).toHaveBeenCalledWith('expected 1', 'expected 2', 'received');
            });
            it('should correctly state whether the test passed', () => {
              expect(this.result.pass).toEqual(false);
            });
            it('should return a useful error message', () => {
              expect(this.result.message()).toEqual('expected printReceived(received) to be foo printExpected(expected 1), printExpected(expected 2)');
            });
          });
        });
      });
    });

    describe('toHave* matchers ', () => {
      describe('adapter for zero argument matchers', () => {
        describeMemberMatcherAdapter(this, 2, () => {
          describe('when the matcher is used', () => {
            beforeEach(() => {
              this.result = this.wrappedMatchersByName.toHaveBar.call(this.jestMatcherScope, 'received', 'memberName');
            });
            it('should call the matcher correctly', () => {
              expect(this.toHaveBar).toHaveBeenCalledWith('memberName', 'received');
            });
            it('should correctly state whether the test passed', () => {
              expect(this.result.pass).toEqual(false);
            });
            it('should return a useful error message', () => {
              expect(this.result.message()).toEqual('expected member "memberName" of printReceived(received) to have bar');
            });
          });
        });
      });

      describe('adapter for single argument matchers', () => {
        describeMemberMatcherAdapter(this, 3, () => {
          describe('when the matcher is used', () => {
            beforeEach(() => {
              this.result = this.wrappedMatchersByName.toHaveBar.call(this.jestMatcherScope, 'received', 'memberName', 'expected');
            });
            it('should call the matcher correctly', () => {
              expect(this.toHaveBar).toHaveBeenCalledWith('memberName', 'expected', 'received');
            });
            it('should correctly state whether the test passed', () => {
              expect(this.result.pass).toEqual(false);
            });
            it('should return a useful error message', () => {
              expect(this.result.message()).toEqual('expected member "memberName" of printReceived(received) to have bar printExpected(expected)');
            });
          });
        });
      });

      describe('adapter for two argument matchers', () => {
        describeMemberMatcherAdapter(this, 4, () => {
          describe('when the matcher is used', () => {
            beforeEach(() => {
              this.result = this.wrappedMatchersByName.toHaveBar.call(this.jestMatcherScope, 'received', 'memberName', 'expected 1', 'expected 2');
            });
            it('should call the matcher correctly', () => {
              expect(this.toHaveBar).toHaveBeenCalledWith('memberName', 'expected 1', 'expected 2', 'received');
            });
            it('should correctly state whether the test passed', () => {
              expect(this.result.pass).toEqual(false);
            });
            it('should return a useful error message', () => {
              expect(this.result.message()).toEqual('expected member "memberName" of printReceived(received) to have bar printExpected(expected 1), printExpected(expected 2)');
            });
          });
        });
      });
    });
  });

  function describeAdapter(self, numberOfArguments, runFurtherTests) {
    beforeEach(() => {
      self.adapter = self.adapters[numberOfArguments];
      self.toBeFoo = jasmine.createSpy('toBeFoo')
        .and.returnValue(false);
    });

    describe('when given a matcher name and function', () => {
      beforeEach(() => {
        self.adapter('toBeFoo', self.toBeFoo);
      });
      it('should add the matcher to jest', () => {
        expect(self.globals.expect.extend).toHaveBeenCalledWith({
          toBeFoo: jasmine.any(Function)
        });
      });
      runFurtherTests();
    });
  }

  function describeMemberMatcherAdapter(self, numberOfArguments, runFurtherTests) {
    beforeEach(() => {
      self.adapter = self.adapters[numberOfArguments];
      self.toHaveBar = jasmine.createSpy('toHaveBar')
        .and.returnValue(false);
    });

    describe('when given a matcher name and function', () => {
      beforeEach(() => {
        self.adapter('toHaveBar', self.toHaveBar);
      });
      it('should add the matcher to jest', () => {
        expect(self.globals.expect.extend).toHaveBeenCalledWith({
          toHaveBar: jasmine.any(Function)
        });
      });
      runFurtherTests();
    });
  }
});
