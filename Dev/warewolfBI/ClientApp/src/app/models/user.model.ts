import { Deserializable } from "./../models/deserializable";

export class User implements Deserializable {
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

  getFullName() {
    return this.firstName + ' ' + this.lastName;
  }

  isAdmin() {
    return this.adminAccount;
  }

  deserialize(input: any) {
    Object.assign(this, input);
    return this;
  }
}
