export class User {
    getFullName() {
        return this.firstName + ' ' + this.lastName;
    }
    isAdmin() {
        return this.adminAccount;
    }
    deserialize(input) {
        Object.assign(this, input);
        return this;
    }
}
//# sourceMappingURL=user.model.js.map