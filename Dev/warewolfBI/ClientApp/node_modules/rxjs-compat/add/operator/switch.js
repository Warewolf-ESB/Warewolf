"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var rxjs_1 = require("rxjs");
var switch_1 = require("../../operator/switch");
rxjs_1.Observable.prototype.switch = switch_1._switch;
rxjs_1.Observable.prototype._switch = switch_1._switch;
//# sourceMappingURL=switch.js.map