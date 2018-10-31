"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var rxjs_1 = require("rxjs");
var do_1 = require("../../operator/do");
rxjs_1.Observable.prototype.do = do_1._do;
rxjs_1.Observable.prototype._do = do_1._do;
//# sourceMappingURL=do.js.map