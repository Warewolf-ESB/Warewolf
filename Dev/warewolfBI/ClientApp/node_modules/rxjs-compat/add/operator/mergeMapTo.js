"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var rxjs_1 = require("rxjs");
var mergeMapTo_1 = require("../../operator/mergeMapTo");
rxjs_1.Observable.prototype.flatMapTo = mergeMapTo_1.mergeMapTo;
rxjs_1.Observable.prototype.mergeMapTo = mergeMapTo_1.mergeMapTo;
//# sourceMappingURL=mergeMapTo.js.map