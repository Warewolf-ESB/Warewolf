"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var rxjs_1 = require("rxjs");
function staticNever() {
    return rxjs_1.NEVER;
}
exports.staticNever = staticNever;
rxjs_1.Observable.never = staticNever;
//# sourceMappingURL=never.js.map