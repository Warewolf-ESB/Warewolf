describe('jasmine 1.x adapters', () => {
  describe('getAdapters(globals)', () => {
    beforeEach(() => {
      this.framework = require('./index');
      this.wrappedMatchersByName = null;
      this.jasmineSuiteScope = {
        actual: 'expect(actual)',
        addMatchers: jasmine.createSpy('this.addMatchers')
          .and.callFake(wrappedMatchersByName => {
            this.wrappedMatchersByName = wrappedMatchersByName;
          })
      };
      this.globals = {
        beforeEach: jasmine.createSpy('beforeEach')
          .and.callFake(setupFunction => {
            setupFunction.call(this.jasmineSuiteScope);
          })
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

    describe('adapter for zero argument matchers', () => {
      describeAdapter(this, 1, () => {
        describe('when the matcher is used', () => {
          describe('with optional message', () => {
            beforeEach(() => {
              this.wrappedMatchersByName.toBeFoo.call(this.jasmineSuiteScope, 'some message');
            });

            it('should call the matcher correctly', () => {
              expect(this.toBeFoo).toHaveBeenCalledWith('expect(actual)', 'some message');
            });
          });

          describe('without optional message', () => {
            beforeEach(() => {
              this.wrappedMatchersByName.toBeFoo.call(this.jasmineSuiteScope);
            });

            it('should call the matcher correctly', () => {
              expect(this.toBeFoo).toHaveBeenCalledWith('expect(actual)', undefined);
            });
          });
        });
      });
    });

    describe('adapter for single argument matchers', () => {
      describeAdapter(this, 2, () => {
        describe('when the matcher is used', () => {
          describe('with optional message', () => {
            beforeEach(() => {
              this.wrappedMatchersByName.toBeFoo.call(this.jasmineSuiteScope, 'expected', 'some message');
            });

            it('should call the matcher correctly', () => {
              expect(this.toBeFoo).toHaveBeenCalledWith('expected', 'expect(actual)', 'some message');
            });
          });

          describe('without optional message', () => {
            beforeEach(() => {
              this.wrappedMatchersByName.toBeFoo.call(this.jasmineSuiteScope, 'expected');
            });

            it('should call the matcher correctly', () => {
              expect(this.toBeFoo).toHaveBeenCalledWith('expected', 'expect(actual)', undefined);
            });
          });
        });
      });
    });

    describe('adapter for two argument matchers', () => {
      describeAdapter(this, 3, () => {
        describe('when the matcher is used', () => {
          describe('with optional message', () => {
            beforeEach(() => {
              this.wrappedMatchersByName.toBeFoo.call(this.jasmineSuiteScope, 'expected', 'other value', 'some message');
            });

            it('should call the matcher correctly', () => {
              expect(this.toBeFoo).toHaveBeenCalledWith('expected', 'other value', 'expect(actual)', 'some message');
            });
          });

          describe('without optional message', () => {
            beforeEach(() => {
              this.wrappedMatchersByName.toBeFoo.call(this.jasmineSuiteScope, 'expected', 'other value');
            });

            it('should call the matcher correctly', () => {
              expect(this.toBeFoo).toHaveBeenCalledWith('expected', 'other value', 'expect(actual)', undefined);
            });
          });
        });
      });
    });

    describe('adapter for three argument matchers', () => {
      describeAdapter(this, 4, () => {
        describe('when the matcher is used', () => {
          describe('with optional message', () => {
            beforeEach(() => {
              this.wrappedMatchersByName.toBeFoo.call(this.jasmineSuiteScope, 'expected', 'other value', 'another value', 'some message');
            });

            it('should call the matcher correctly', () => {
              expect(this.toBeFoo).toHaveBeenCalledWith('expected', 'other value', 'another value', 'expect(actual)', 'some message');
            });
          });

          describe('without optional message', () => {
            beforeEach(() => {
              this.wrappedMatchersByName.toBeFoo.call(this.jasmineSuiteScope, 'expected', 'other value', 'another value');
            });

            it('should call the matcher correctly', () => {
              expect(this.toBeFoo).toHaveBeenCalledWith('expected', 'other value', 'another value', 'expect(actual)', undefined);
            });
          });
        });
      });
    });
  });

  function describeAdapter(self, numberOfArguments, runFurtherTests) {
    beforeEach(() => {
      self.adapter = self.adapters[numberOfArguments];
      self.toBeFoo = jasmine.createSpy('toBeFoo');
    });

    describe('when given a matcher name and function', () => {
      beforeEach(() => {
        self.adapter('toBeFoo', self.toBeFoo);
      });

      it('should add the matcher to jasmine', () => {
        expect(self.globals.beforeEach).toHaveBeenCalledWith(jasmine.any(Function));
        expect(self.jasmineSuiteScope.addMatchers).toHaveBeenCalledWith({
          toBeFoo: jasmine.any(Function)
        });
      });

      runFurtherTests();
    });
  }
});
