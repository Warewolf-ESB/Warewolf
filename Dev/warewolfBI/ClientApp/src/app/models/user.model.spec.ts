import { User } from './user.model';
 
describe('User', () => {
  it('should create an instance of User',() => {
    expect(new User()).toBeTruthy();
    });
})
