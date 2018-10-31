"use strict";
/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
Object.defineProperty(exports, "__esModule", { value: true });
const core_1 = require("@angular-devkit/core");
const command_1 = require("../models/command");
function pickOne(of) {
    return of[Math.floor(Math.random() * of.length)];
}
class AwesomeCommand extends command_1.Command {
    run() {
        const phrase = pickOne([
            `You're on it, there's nothing for me to do!`,
            `Let's take a look... nope, it's all good!`,
            `You're doing fine.`,
            `You're already doing great.`,
            `Nothing to do; already awesome. Exiting.`,
            `Error 418: As Awesome As Can Get.`,
            `I spy with my little eye a great developer!`,
            `Noop... already awesome.`,
        ]);
        this.logger.info(core_1.terminal.green(phrase));
    }
}
exports.AwesomeCommand = AwesomeCommand;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiZWFzdGVyLWVnZy1pbXBsLmpzIiwic291cmNlUm9vdCI6Ii4vIiwic291cmNlcyI6WyJwYWNrYWdlcy9hbmd1bGFyL2NsaS9jb21tYW5kcy9lYXN0ZXItZWdnLWltcGwudHMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IjtBQUFBOzs7Ozs7R0FNRzs7QUFFSCwrQ0FBZ0Q7QUFDaEQsK0NBQTRDO0FBRTVDLGlCQUFpQixFQUFZO0lBQzNCLE9BQU8sRUFBRSxDQUFDLElBQUksQ0FBQyxLQUFLLENBQUMsSUFBSSxDQUFDLE1BQU0sRUFBRSxHQUFHLEVBQUUsQ0FBQyxNQUFNLENBQUMsQ0FBQyxDQUFDO0FBQ25ELENBQUM7QUFFRCxvQkFBNEIsU0FBUSxpQkFBTztJQUN6QyxHQUFHO1FBQ0QsTUFBTSxNQUFNLEdBQUcsT0FBTyxDQUFDO1lBQ3JCLDZDQUE2QztZQUM3QywyQ0FBMkM7WUFDM0Msb0JBQW9CO1lBQ3BCLDZCQUE2QjtZQUM3QiwwQ0FBMEM7WUFDMUMsbUNBQW1DO1lBQ25DLDZDQUE2QztZQUM3QywwQkFBMEI7U0FDM0IsQ0FBQyxDQUFDO1FBQ0gsSUFBSSxDQUFDLE1BQU0sQ0FBQyxJQUFJLENBQUMsZUFBUSxDQUFDLEtBQUssQ0FBQyxNQUFNLENBQUMsQ0FBQyxDQUFDO0lBQzNDLENBQUM7Q0FDRjtBQWRELHdDQWNDIiwic291cmNlc0NvbnRlbnQiOlsiLyoqXG4gKiBAbGljZW5zZVxuICogQ29weXJpZ2h0IEdvb2dsZSBJbmMuIEFsbCBSaWdodHMgUmVzZXJ2ZWQuXG4gKlxuICogVXNlIG9mIHRoaXMgc291cmNlIGNvZGUgaXMgZ292ZXJuZWQgYnkgYW4gTUlULXN0eWxlIGxpY2Vuc2UgdGhhdCBjYW4gYmVcbiAqIGZvdW5kIGluIHRoZSBMSUNFTlNFIGZpbGUgYXQgaHR0cHM6Ly9hbmd1bGFyLmlvL2xpY2Vuc2VcbiAqL1xuXG5pbXBvcnQgeyB0ZXJtaW5hbCB9IGZyb20gJ0Bhbmd1bGFyLWRldmtpdC9jb3JlJztcbmltcG9ydCB7IENvbW1hbmQgfSBmcm9tICcuLi9tb2RlbHMvY29tbWFuZCc7XG5cbmZ1bmN0aW9uIHBpY2tPbmUob2Y6IHN0cmluZ1tdKTogc3RyaW5nIHtcbiAgcmV0dXJuIG9mW01hdGguZmxvb3IoTWF0aC5yYW5kb20oKSAqIG9mLmxlbmd0aCldO1xufVxuXG5leHBvcnQgY2xhc3MgQXdlc29tZUNvbW1hbmQgZXh0ZW5kcyBDb21tYW5kIHtcbiAgcnVuKCkge1xuICAgIGNvbnN0IHBocmFzZSA9IHBpY2tPbmUoW1xuICAgICAgYFlvdSdyZSBvbiBpdCwgdGhlcmUncyBub3RoaW5nIGZvciBtZSB0byBkbyFgLFxuICAgICAgYExldCdzIHRha2UgYSBsb29rLi4uIG5vcGUsIGl0J3MgYWxsIGdvb2QhYCxcbiAgICAgIGBZb3UncmUgZG9pbmcgZmluZS5gLFxuICAgICAgYFlvdSdyZSBhbHJlYWR5IGRvaW5nIGdyZWF0LmAsXG4gICAgICBgTm90aGluZyB0byBkbzsgYWxyZWFkeSBhd2Vzb21lLiBFeGl0aW5nLmAsXG4gICAgICBgRXJyb3IgNDE4OiBBcyBBd2Vzb21lIEFzIENhbiBHZXQuYCxcbiAgICAgIGBJIHNweSB3aXRoIG15IGxpdHRsZSBleWUgYSBncmVhdCBkZXZlbG9wZXIhYCxcbiAgICAgIGBOb29wLi4uIGFscmVhZHkgYXdlc29tZS5gLFxuICAgIF0pO1xuICAgIHRoaXMubG9nZ2VyLmluZm8odGVybWluYWwuZ3JlZW4ocGhyYXNlKSk7XG4gIH1cbn1cbiJdfQ==