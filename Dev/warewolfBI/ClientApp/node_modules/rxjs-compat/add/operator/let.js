"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var rxjs_1 = require("rxjs");
var let_1 = require("../../operator/let");
rxjs_1.Observable.prototype.let = let_1.letProto;
rxjs_1.Observable.prototype.letBind = let_1.letProto;
//# sourceMappingURL=let.js.map