describe('jasmine 2.x adapters', () => {
  describe('getAdapters(globals)', () => {
    beforeEach(() => {
      this.framework = require('./index');
      this.wrappedMatchersByName = null;
      this.globals = {
        jasmine: {
          addMatchers: jasmine.createSpy('jasmine.addMatchers')
            .and.callFake(wrappedMatchersByName => {
              this.wrappedMatchersByName = wrappedMatchersByName;
            })
        },
        beforeEach: jasmine.createSpy('beforeEach')
          .and.callFake(setupFunction => setupFunction())
      };
      this.jasmineUtils = {
        buildFailureMessage: jasmine.createSpy('util.buildFailureMessage')
          .and.returnValue('util.buildFailureMessage return value')
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
        describeMatcherAdapter(this, 1, () => {
          describe('with optional message', () => {
            beforeEach(() => {
              this.result = this.wrappedMatcher('expect(actual)', 'some message');
            });
            it('should call the matcher correctly', () => {
              expect(this.toBeFoo).toHaveBeenCalledWith('expect(actual)');
            });
            it('should provide the correct result to jasmine', () => {
              expect(this.result).toEqual({
                pass: true,
                message: 'util.buildFailureMessage return value'
              });
            });
            it('should construct error messages correctly', () => {
              expect(this.jasmineUtils.buildFailureMessage).toHaveBeenCalledWith('toBeFoo', true, 'expect(actual)', 'some message');
            });
          });

          describe('without optional message', () => {
            beforeEach(() => {
              this.result = this.wrappedMatcher('expect(actual)');
            });
            it('should call the matcher correctly', () => {
              expect(this.toBeFoo).toHaveBeenCalledWith('expect(actual)');
            });
            it('should provide the correct result to jasmine', () => {
              expect(this.result).toEqual({
                pass: true,
                message: 'util.buildFailureMessage return value'
              });
            });
            it('should construct error messages correctly', () => {
              expect(this.jasmineUtils.buildFailureMessage).toHaveBeenCalledWith('toBeFoo', true, 'expect(actual)');
            });
          });
        });
      });

      describe('adapter for single argument matchers', () => {
        describeMatcherAdapter(this, 2, () => {
          describe('with optional message', () => {
            beforeEach(() => {
              this.result = this.wrappedMatcher('expect(actual)', 'expected', 'some message');
            });
            it('should call the matcher correctly', () => {
              expect(this.toBeFoo).toHaveBeenCalledWith('expected', 'expect(actual)');
            });
            it('should provide the correct result to jasmine', () => {
              expect(this.result).toEqual({
                pass: true,
                message: 'util.buildFailureMessage return value'
              });
            });
            it('should construct error messages correctly', () => {
              expect(this.jasmineUtils.buildFailureMessage).toHaveBeenCalledWith('toBeFoo', true, 'expect(actual)', 'expected', 'some message');
            });
          });

          describe('without optional message', () => {
            beforeEach(() => {
              this.result = this.wrappedMatcher('expect(actual)', 'expected');
            });
            it('should call the matcher correctly', () => {
              expect(this.toBeFoo).toHaveBeenCalledWith('expected', 'expect(actual)');
            });
            it('should provide the correct result to jasmine', () => {
              expect(this.result).toEqual({
                pass: true,
                message: 'util.buildFailureMessage return value'
              });
            });
            it('should construct error messages correctly', () => {
              expect(this.jasmineUtils.buildFailureMessage).toHaveBeenCalledWith('toBeFoo', true, 'expect(actual)', 'expected');
            });
          });
        });
      });

      describe('adapter for two argument matchers', () => {
        describeMatcherAdapter(this, 3, () => {
          describe('with optional message', () => {
            beforeEach(() => {
              this.result = this.wrappedMatcher('expect(actual)', 'expected', 'other', 'some message');
            });
            it('should call the matcher correctly', () => {
              expect(this.toBeFoo).toHaveBeenCalledWith('expected', 'other', 'expect(actual)');
            });
            it('should provide the correct result to jasmine', () => {
              expect(this.result).toEqual({
                pass: true,
                message: 'util.buildFailureMessage return value'
              });
            });
            it('should construct error messages correctly', () => {
              expect(this.jasmineUtils.buildFailureMessage).toHaveBeenCalledWith('toBeFoo', true, 'expect(actual)', 'expected', 'other', 'some message');
            });
          });

          describe('without optional message', () => {
            beforeEach(() => {
              this.result = this.wrappedMatcher('expect(actual)', 'expected', 'other');
            });
            it('should call the matcher correctly', () => {
              expect(this.toBeFoo).toHaveBeenCalledWith('expected', 'other', 'expect(actual)');
            });
            it('should provide the correct result to jasmine', () => {
              expect(this.result).toEqual({
                pass: true,
                message: 'util.buildFailureMessage return value'
              });
            });
            it('should construct error messages correctly', () => {
              expect(this.jasmineUtils.buildFailureMessage).toHaveBeenCalledWith('toBeFoo', true, 'expect(actual)', 'expected', 'other');
            });
          });
        });
      });
    });

    describe('toHave* matchers ', () => {
      describe('adapter for zero argument matchers', () => {
        describeMemberMatcherAdapter(this, 2, () => {
          describe('with optional message', () => {
            beforeEach(() => {
              this.result = this.wrappedMatcher('expect(actual)', 'memberName', 'some message');
            });
            it('should call the matcher correctly', () => {
              expect(this.toHaveBar).toHaveBeenCalledWith('memberName', 'expect(actual)');
            });
            it('should provide the correct result to jasmine', () => {
              expect(this.result).toEqual({
                pass: true,
                message: 'util.buildFailureMessage return value'
              });
            });
            it('should construct error messages correctly', () => {
              expect(this.jasmineUtils.buildFailureMessage).toHaveBeenCalledWith('toHaveBar', true, 'expect(actual)', 'some message');
            });
          });

          describe('without optional message', () => {
            beforeEach(() => {
              this.result = this.wrappedMatcher('expect(actual)', 'memberName');
            });
            it('should call the matcher correctly', () => {
              expect(this.toHaveBar).toHaveBeenCalledWith('memberName', 'expect(actual)');
            });
            it('should provide the correct result to jasmine', () => {
              expect(this.result).toEqual({
                pass: true,
                message: 'util.buildFailureMessage return value'
              });
            });
            it('should construct error messages correctly', () => {
              expect(this.jasmineUtils.buildFailureMessage).toHaveBeenCalledWith('toHaveBar', true, 'expect(actual)', 'memberName');
            });
          });
        });
      });

      describe('adapter for single argument matchers', () => {
        describeMemberMatcherAdapter(this, 3, () => {
          describe('with optional message', () => {
            beforeEach(() => {
              this.result = this.wrappedMatcher('expect(actual)', 'memberName', 'other', 'some message');
            });
            it('should call the matcher correctly', () => {
              expect(this.toHaveBar).toHaveBeenCalledWith('memberName', 'other', 'expect(actual)');
            });
            it('should provide the correct result to jasmine', () => {
              expect(this.result).toEqual({
                pass: true,
                message: 'util.buildFailureMessage return value'
              });
            });
            it('should construct error messages correctly', () => {
              expect(this.jasmineUtils.buildFailureMessage).toHaveBeenCalledWith('toHaveBar', true, 'expect(actual)', 'other', 'some message');
            });
          });

          describe('without optional message', () => {
            beforeEach(() => {
              this.result = this.wrappedMatcher('expect(actual)', 'memberName', 'other');
            });
            it('should call the matcher correctly', () => {
              expect(this.toHaveBar).toHaveBeenCalledWith('memberName', 'other', 'expect(actual)');
            });
            it('should provide the correct result to jasmine', () => {
              expect(this.result).toEqual({
                pass: true,
                message: 'util.buildFailureMessage return value'
              });
            });
            it('should construct error messages correctly', () => {
              expect(this.jasmineUtils.buildFailureMessage).toHaveBeenCalledWith('toHaveBar', true, 'expect(actual)', 'other');
            });
          });
        });
      });

      describe('adapter for two argument matchers', () => {
        describeMemberMatcherAdapter(this, 4, () => {
          describe('with optional message', () => {
            beforeEach(() => {
              this.result = this.wrappedMatcher('expect(actual)', 'memberName', 'other', 'another', 'some message');
            });
            it('should call the matcher correctly', () => {
              expect(this.toHaveBar).toHaveBeenCalledWith('memberName', 'other', 'another', 'expect(actual)');
            });
            it('should provide the correct result to jasmine', () => {
              expect(this.result).toEqual({
                pass: true,
                message: 'util.buildFailureMessage return value'
              });
            });
            it('should construct error messages correctly', () => {
              expect(this.jasmineUtils.buildFailureMessage).toHaveBeenCalledWith('toHaveBar', true, 'expect(actual)', 'other', 'another', 'some message');
            });
          });

          describe('without optional message', () => {
            beforeEach(() => {
              this.result = this.wrappedMatcher('expect(actual)', 'memberName', 'other', 'another');
            });
            it('should call the matcher correctly', () => {
              expect(this.toHaveBar).toHaveBeenCalledWith('memberName', 'other', 'another', 'expect(actual)');
            });
            it('should provide the correct result to jasmine', () => {
              expect(this.result).toEqual({
                pass: true,
                message: 'util.buildFailureMessage return value'
              });
            });
            it('should construct error messages correctly', () => {
              expect(this.jasmineUtils.buildFailureMessage).toHaveBeenCalledWith('toHaveBar', true, 'expect(actual)', 'other', 'another');
            });
          });
        });
      });
    });
  });

  function describeMatcherAdapter(self, numberOfArguments, runFurtherTests) {
    beforeEach(() => {
      self.adapter = self.adapters[numberOfArguments];
      self.toBeFoo = jasmine.createSpy('toBeFoo').and.returnValue(true);
    });

    describe('when given a matcher name and function', () => {
      beforeEach(() => {
        self.adapter('toBeFoo', self.toBeFoo);
      });

      it('should add a jasmine v2 matcher factory to jasmine', () => {
        expect(self.globals.beforeEach).toHaveBeenCalledWith(jasmine.any(Function));
        expect(self.globals.jasmine.addMatchers).toHaveBeenCalledWith({
          toBeFoo: jasmine.any(Function)
        });
      });

      describe('when the matcher is used', () => {
        beforeEach(() => {
          self.matcherFactory = self.wrappedMatchersByName.toBeFoo;
          self.wrappedMatcher = self.matcherFactory(self.jasmineUtils).compare;
        });

        runFurtherTests();
      });
    });
  }

  function describeMemberMatcherAdapter(self, numberOfArguments, runFurtherTests) {
    beforeEach(() => {
      self.adapter = self.adapters[numberOfArguments];
      self.toHaveBar = jasmine.createSpy('toHaveBar').and.returnValue(true);
    });

    describe('when given a matcher name and function', () => {
      beforeEach(() => {
        self.adapter('toHaveBar', self.toHaveBar);
      });

      it('should add a jasmine v2 matcher factory to jasmine', () => {
        expect(self.globals.beforeEach).toHaveBeenCalledWith(jasmine.any(Function));
        expect(self.globals.jasmine.addMatchers).toHaveBeenCalledWith({
          toHaveBar: jasmine.any(Function)
        });
      });

      describe('when the matcher is used', () => {
        beforeEach(() => {
          self.matcherFactory = self.wrappedMatchersByName.toHaveBar;
          self.wrappedMatcher = self.matcherFactory(self.jasmineUtils).compare;
        });

        runFurtherTests();
      });
    });
  }
});
