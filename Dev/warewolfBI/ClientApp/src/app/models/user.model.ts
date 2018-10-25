export class User  {
  public id: string;
  public active: boolean;
  public firstName: string;
  public lastName: string;
  public email: string;
  public adminAccount: boolean;
  public userName: string;
  public message: string;
  password: string;
  confirmPassword: string;
  securityQuestion: string;
  securityAnswer: string;
  public termsAndConditions: boolean;
  public userToken: string;
  public startDate: string;
  public endDate: string;
  public createdDate: string;
  public userStatusId: string;

  constructor(values: Object = {}) {
    Object.assign(this, values);
  }  
}
