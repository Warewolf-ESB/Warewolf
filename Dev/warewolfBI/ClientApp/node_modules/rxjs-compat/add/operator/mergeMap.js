"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var rxjs_1 = require("rxjs");
var mergeMap_1 = require("../../operator/mergeMap");
rxjs_1.Observable.prototype.mergeMap = mergeMap_1.mergeMap;
rxjs_1.Observable.prototype.flatMap = mergeMap_1.mergeMap;
//# sourceMappingURL=mergeMap.js.map