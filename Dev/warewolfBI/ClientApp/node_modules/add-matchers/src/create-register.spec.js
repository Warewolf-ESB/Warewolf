describe('createRegister', () => {
  beforeEach(() => {
    this.createRegister = require('./create-register');
    this.frameworks = {
      jasmineV1: createMockFramework('jasmine 1.x.x'),
      jasmineV2: createMockFramework('jasmine 2.x.x'),
      jest: createMockFramework('jest')
    };
  });

  describe('when jest is installed', () => {
    beforeEach(() => {
      this.globals = {
        expect: {
          extend: jasmine.createSpy('expect.extend')
        }
      };
      this.addMatchers = this.createRegister(this.frameworks, this.globals);
    });
    it('should convert matchers to work with jest', () => {
      expect(this.frameworks.jest.getAdapters).toHaveBeenCalledWith(this.globals);
    });
    it('should expose the correct public API', () => {
      expect(typeof this.addMatchers).toEqual('function');
      expect(typeof this.addMatchers.asymmetric).toEqual('function');
    });
  });

  describe('when jasmine 2.x.x is installed', () => {
    beforeEach(() => {
      this.globals = {
        jasmine: {
          addMatchers: jasmine.createSpy('jasmine.addMatchers')
        }
      };
      this.addMatchers = this.createRegister(this.frameworks, this.globals);
    });
    it('should convert matchers to work with jasmine 2.x.x', () => {
      expect(this.frameworks.jasmineV2.getAdapters).toHaveBeenCalledWith(this.globals);
    });
    it('should expose the correct public API', () => {
      expect(typeof this.addMatchers).toEqual('function');
      expect(typeof this.addMatchers.asymmetric).toEqual('function');
    });
  });

  describe('when jasmine 1.x.x is installed', () => {
    beforeEach(() => {
      this.globals = {
        jasmine: {}
      };
      this.addMatchers = this.createRegister(this.frameworks, this.globals);
    });
    it('should convert matchers to work with jasmine 1.x.x', () => {
      expect(this.frameworks.jasmineV1.getAdapters).toHaveBeenCalledWith(this.globals);
    });
    it('should expose the correct public API', () => {
      expect(typeof this.addMatchers).toEqual('function');
      expect(typeof this.addMatchers.asymmetric).toEqual('function');
    });
  });

  describe('when no test framework is found', () => {
    beforeEach(() => {
      this.globals = {};
    });
    it('should throw an error explaining this', () => {
      expect(() => {
        this.createRegister(this.frameworks, this.globals);
      }).toThrow(new Error('jasmine-expect cannot find jest, jasmine v2.x, or jasmine v1.x'));
    });
  });

  function createMockFramework(name) {
    return {
      getAdapters: jasmine.createSpy().and.returnValue({
        1: jasmine.createSpy(name + ' adapter for 1 arguments'),
        2: jasmine.createSpy(name + ' adapter for 2 arguments'),
        3: jasmine.createSpy(name + ' adapter for 3 arguments'),
        4: jasmine.createSpy(name + ' adapter for 4 arguments')
      })
    };
  }
});
