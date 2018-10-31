"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var rxjs_1 = require("rxjs");
var finally_1 = require("../../operator/finally");
rxjs_1.Observable.prototype.finally = finally_1._finally;
rxjs_1.Observable.prototype._finally = finally_1._finally;
//# sourceMappingURL=finally.js.map