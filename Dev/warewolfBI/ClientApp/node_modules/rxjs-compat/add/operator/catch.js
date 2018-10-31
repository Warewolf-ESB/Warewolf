"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var rxjs_1 = require("rxjs");
var catch_1 = require("../../operator/catch");
rxjs_1.Observable.prototype.catch = catch_1._catch;
rxjs_1.Observable.prototype._catch = catch_1._catch;
//# sourceMappingURL=catch.js.map