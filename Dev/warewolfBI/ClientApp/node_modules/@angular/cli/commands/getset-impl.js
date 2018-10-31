"use strict";
/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
const command_1 = require("../models/command");
class GetSetCommand extends command_1.Command {
    run(_options) {
        return __awaiter(this, void 0, void 0, function* () {
            this.logger.warn('get/set have been deprecated in favor of the config command.');
        });
    }
}
exports.GetSetCommand = GetSetCommand;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiZ2V0c2V0LWltcGwuanMiLCJzb3VyY2VSb290IjoiLi8iLCJzb3VyY2VzIjpbInBhY2thZ2VzL2FuZ3VsYXIvY2xpL2NvbW1hbmRzL2dldHNldC1pbXBsLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7QUFBQTs7Ozs7O0dBTUc7Ozs7Ozs7Ozs7QUFFSCwrQ0FBNEM7QUFPNUMsbUJBQTJCLFNBQVEsaUJBQU87SUFDM0IsR0FBRyxDQUFDLFFBQWlCOztZQUNoQyxJQUFJLENBQUMsTUFBTSxDQUFDLElBQUksQ0FBQyw4REFBOEQsQ0FBQyxDQUFDO1FBQ25GLENBQUM7S0FBQTtDQUNGO0FBSkQsc0NBSUMiLCJzb3VyY2VzQ29udGVudCI6WyIvKipcbiAqIEBsaWNlbnNlXG4gKiBDb3B5cmlnaHQgR29vZ2xlIEluYy4gQWxsIFJpZ2h0cyBSZXNlcnZlZC5cbiAqXG4gKiBVc2Ugb2YgdGhpcyBzb3VyY2UgY29kZSBpcyBnb3Zlcm5lZCBieSBhbiBNSVQtc3R5bGUgbGljZW5zZSB0aGF0IGNhbiBiZVxuICogZm91bmQgaW4gdGhlIExJQ0VOU0UgZmlsZSBhdCBodHRwczovL2FuZ3VsYXIuaW8vbGljZW5zZVxuICovXG5cbmltcG9ydCB7IENvbW1hbmQgfSBmcm9tICcuLi9tb2RlbHMvY29tbWFuZCc7XG5cbmV4cG9ydCBpbnRlcmZhY2UgT3B0aW9ucyB7XG4gIGtleXdvcmQ6IHN0cmluZztcbiAgc2VhcmNoPzogYm9vbGVhbjtcbn1cblxuZXhwb3J0IGNsYXNzIEdldFNldENvbW1hbmQgZXh0ZW5kcyBDb21tYW5kIHtcbiAgcHVibGljIGFzeW5jIHJ1bihfb3B0aW9uczogT3B0aW9ucykge1xuICAgIHRoaXMubG9nZ2VyLndhcm4oJ2dldC9zZXQgaGF2ZSBiZWVuIGRlcHJlY2F0ZWQgaW4gZmF2b3Igb2YgdGhlIGNvbmZpZyBjb21tYW5kLicpO1xuICB9XG59XG4iXX0=