export class LogEntry {
    constructor(values = {}) {
        Object.assign(this, values);
    }
    deserialize(input) {
        Object.assign(this, input);
        return this;
    }
}
//# sourceMappingURL=logentry.model.js.map