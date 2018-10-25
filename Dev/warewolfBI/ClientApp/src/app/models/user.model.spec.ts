import { User } from './user.model';

describe('User', () => {

  it('should create an instance of User', () => {
    expect(new User()).toBeTruthy();
  });

  it('should accept values', () => {
    let user = new User();
    user = {
      id: '1111',
      active: true,
      firstName: 'firstName',
      lastName: 'lastName',
      email: 'email',
      adminAccount: false,
      userName: 'userName',
      message: 'message',
      password: 'password',
      confirmPassword: 'password',
      securityQuestion: 'securityQuestion',
      securityAnswer: 'securityAnswer',
      termsAndConditions: true,
      userToken: 'userToken',
      startDate: 'startDate',
      endDate: 'endDate',
      createdDate: 'createdDate',
      userStatusId: '111'
    }
    expect(user.firstName).toEqual("firstName");
    expect(user.lastName).toEqual("lastName");
    expect(user.email).toEqual('email');
  });
})
