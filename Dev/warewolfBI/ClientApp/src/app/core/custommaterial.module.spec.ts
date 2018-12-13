import { CustomMaterialModule } from './custommaterial.module';

describe('CustomMaterialModule', () => {
  let custommaterialModule: CustomMaterialModule;

  beforeEach(() => {
    custommaterialModule = new CustomMaterialModule();
  });

  it('should create an instance', () => {
    expect(custommaterialModule).toBeTruthy();
  });
});
