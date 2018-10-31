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
const child_process_1 = require("child_process");
const util_1 = require("util");
const config_1 = require("./config");
const execPromise = util_1.promisify(child_process_1.exec);
const packageManager = config_1.getPackageManager();
function checkYarnOrCNPM() {
    // Don't show messages if user has already changed the default.
    if (packageManager !== 'default') {
        return Promise.resolve();
    }
    return Promise
        .all([checkYarn(), checkCNPM()])
        .then((data) => {
        const [isYarnInstalled, isCNPMInstalled] = data;
        if (isYarnInstalled && isCNPMInstalled) {
            console.error(core_1.terminal.yellow('You can `ng config -g cli.packageManager yarn` '
                + 'or `ng config -g cli.packageManager cnpm`.'));
        }
        else if (isYarnInstalled) {
            console.error(core_1.terminal.yellow('You can `ng config -g cli.packageManager yarn`.'));
        }
        else if (isCNPMInstalled) {
            console.error(core_1.terminal.yellow('You can `ng config -g cli.packageManager cnpm`.'));
        }
        else {
            if (packageManager !== 'default' && packageManager !== 'npm') {
                console.error(core_1.terminal.yellow(`Seems that ${packageManager} is not installed.`));
                console.error(core_1.terminal.yellow('You can `ng config -g cli.packageManager npm`.'));
            }
        }
    });
}
exports.checkYarnOrCNPM = checkYarnOrCNPM;
function checkYarn() {
    return execPromise('yarn --version')
        .then(() => true, () => false);
}
function checkCNPM() {
    return execPromise('cnpm --version')
        .then(() => true, () => false);
}
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiY2hlY2stcGFja2FnZS1tYW5hZ2VyLmpzIiwic291cmNlUm9vdCI6Ii4vIiwic291cmNlcyI6WyJwYWNrYWdlcy9hbmd1bGFyL2NsaS91dGlsaXRpZXMvY2hlY2stcGFja2FnZS1tYW5hZ2VyLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7QUFBQTs7Ozs7O0dBTUc7O0FBRUgsK0NBQWdEO0FBQ2hELGlEQUFxQztBQUNyQywrQkFBaUM7QUFDakMscUNBQTZDO0FBRTdDLE1BQU0sV0FBVyxHQUFHLGdCQUFTLENBQUMsb0JBQUksQ0FBQyxDQUFDO0FBQ3BDLE1BQU0sY0FBYyxHQUFHLDBCQUFpQixFQUFFLENBQUM7QUFHM0M7SUFFRSwrREFBK0Q7SUFDL0QsSUFBSSxjQUFjLEtBQUssU0FBUyxFQUFFO1FBQ2hDLE9BQU8sT0FBTyxDQUFDLE9BQU8sRUFBRSxDQUFDO0tBQzFCO0lBRUQsT0FBTyxPQUFPO1NBQ1QsR0FBRyxDQUFDLENBQUMsU0FBUyxFQUFFLEVBQUUsU0FBUyxFQUFFLENBQUMsQ0FBQztTQUMvQixJQUFJLENBQUMsQ0FBQyxJQUFvQixFQUFFLEVBQUU7UUFDN0IsTUFBTSxDQUFDLGVBQWUsRUFBRSxlQUFlLENBQUMsR0FBRyxJQUFJLENBQUM7UUFDaEQsSUFBSSxlQUFlLElBQUksZUFBZSxFQUFFO1lBQ3RDLE9BQU8sQ0FBQyxLQUFLLENBQUMsZUFBUSxDQUFDLE1BQU0sQ0FBQyxpREFBaUQ7a0JBQzNFLDRDQUE0QyxDQUFDLENBQUMsQ0FBQztTQUNwRDthQUFNLElBQUksZUFBZSxFQUFFO1lBQzFCLE9BQU8sQ0FBQyxLQUFLLENBQUMsZUFBUSxDQUFDLE1BQU0sQ0FBQyxpREFBaUQsQ0FBQyxDQUFDLENBQUM7U0FDbkY7YUFBTSxJQUFJLGVBQWUsRUFBRTtZQUMxQixPQUFPLENBQUMsS0FBSyxDQUFDLGVBQVEsQ0FBQyxNQUFNLENBQUMsaURBQWlELENBQUMsQ0FBQyxDQUFDO1NBQ25GO2FBQU87WUFDTixJQUFJLGNBQWMsS0FBSyxTQUFTLElBQUksY0FBYyxLQUFLLEtBQUssRUFBRTtnQkFDNUQsT0FBTyxDQUFDLEtBQUssQ0FBQyxlQUFRLENBQUMsTUFBTSxDQUFDLGNBQWMsY0FBYyxvQkFBb0IsQ0FBQyxDQUFDLENBQUM7Z0JBQ2pGLE9BQU8sQ0FBQyxLQUFLLENBQUMsZUFBUSxDQUFDLE1BQU0sQ0FBQyxnREFBZ0QsQ0FBQyxDQUFDLENBQUM7YUFDbEY7U0FDRjtJQUNILENBQUMsQ0FBQyxDQUFDO0FBQ1QsQ0FBQztBQXpCRCwwQ0F5QkM7QUFFRDtJQUNFLE9BQU8sV0FBVyxDQUFDLGdCQUFnQixDQUFDO1NBQ2pDLElBQUksQ0FBQyxHQUFHLEVBQUUsQ0FBQyxJQUFJLEVBQUUsR0FBRyxFQUFFLENBQUMsS0FBSyxDQUFDLENBQUM7QUFDbkMsQ0FBQztBQUVEO0lBQ0UsT0FBTyxXQUFXLENBQUMsZ0JBQWdCLENBQUM7U0FDakMsSUFBSSxDQUFDLEdBQUcsRUFBRSxDQUFDLElBQUksRUFBRSxHQUFHLEVBQUUsQ0FBQyxLQUFLLENBQUMsQ0FBQztBQUNuQyxDQUFDIiwic291cmNlc0NvbnRlbnQiOlsiLyoqXG4gKiBAbGljZW5zZVxuICogQ29weXJpZ2h0IEdvb2dsZSBJbmMuIEFsbCBSaWdodHMgUmVzZXJ2ZWQuXG4gKlxuICogVXNlIG9mIHRoaXMgc291cmNlIGNvZGUgaXMgZ292ZXJuZWQgYnkgYW4gTUlULXN0eWxlIGxpY2Vuc2UgdGhhdCBjYW4gYmVcbiAqIGZvdW5kIGluIHRoZSBMSUNFTlNFIGZpbGUgYXQgaHR0cHM6Ly9hbmd1bGFyLmlvL2xpY2Vuc2VcbiAqL1xuXG5pbXBvcnQgeyB0ZXJtaW5hbCB9IGZyb20gJ0Bhbmd1bGFyLWRldmtpdC9jb3JlJztcbmltcG9ydCB7IGV4ZWMgfSBmcm9tICdjaGlsZF9wcm9jZXNzJztcbmltcG9ydCB7IHByb21pc2lmeSB9IGZyb20gJ3V0aWwnO1xuaW1wb3J0IHsgZ2V0UGFja2FnZU1hbmFnZXIgfSBmcm9tICcuL2NvbmZpZyc7XG5cbmNvbnN0IGV4ZWNQcm9taXNlID0gcHJvbWlzaWZ5KGV4ZWMpO1xuY29uc3QgcGFja2FnZU1hbmFnZXIgPSBnZXRQYWNrYWdlTWFuYWdlcigpO1xuXG5cbmV4cG9ydCBmdW5jdGlvbiBjaGVja1lhcm5PckNOUE0oKSB7XG5cbiAgLy8gRG9uJ3Qgc2hvdyBtZXNzYWdlcyBpZiB1c2VyIGhhcyBhbHJlYWR5IGNoYW5nZWQgdGhlIGRlZmF1bHQuXG4gIGlmIChwYWNrYWdlTWFuYWdlciAhPT0gJ2RlZmF1bHQnKSB7XG4gICAgcmV0dXJuIFByb21pc2UucmVzb2x2ZSgpO1xuICB9XG5cbiAgcmV0dXJuIFByb21pc2VcbiAgICAgIC5hbGwoW2NoZWNrWWFybigpLCBjaGVja0NOUE0oKV0pXG4gICAgICAudGhlbigoZGF0YTogQXJyYXk8Ym9vbGVhbj4pID0+IHtcbiAgICAgICAgY29uc3QgW2lzWWFybkluc3RhbGxlZCwgaXNDTlBNSW5zdGFsbGVkXSA9IGRhdGE7XG4gICAgICAgIGlmIChpc1lhcm5JbnN0YWxsZWQgJiYgaXNDTlBNSW5zdGFsbGVkKSB7XG4gICAgICAgICAgY29uc29sZS5lcnJvcih0ZXJtaW5hbC55ZWxsb3coJ1lvdSBjYW4gYG5nIGNvbmZpZyAtZyBjbGkucGFja2FnZU1hbmFnZXIgeWFybmAgJ1xuICAgICAgICAgICAgKyAnb3IgYG5nIGNvbmZpZyAtZyBjbGkucGFja2FnZU1hbmFnZXIgY25wbWAuJykpO1xuICAgICAgICB9IGVsc2UgaWYgKGlzWWFybkluc3RhbGxlZCkge1xuICAgICAgICAgIGNvbnNvbGUuZXJyb3IodGVybWluYWwueWVsbG93KCdZb3UgY2FuIGBuZyBjb25maWcgLWcgY2xpLnBhY2thZ2VNYW5hZ2VyIHlhcm5gLicpKTtcbiAgICAgICAgfSBlbHNlIGlmIChpc0NOUE1JbnN0YWxsZWQpIHtcbiAgICAgICAgICBjb25zb2xlLmVycm9yKHRlcm1pbmFsLnllbGxvdygnWW91IGNhbiBgbmcgY29uZmlnIC1nIGNsaS5wYWNrYWdlTWFuYWdlciBjbnBtYC4nKSk7XG4gICAgICAgIH0gZWxzZSAge1xuICAgICAgICAgIGlmIChwYWNrYWdlTWFuYWdlciAhPT0gJ2RlZmF1bHQnICYmIHBhY2thZ2VNYW5hZ2VyICE9PSAnbnBtJykge1xuICAgICAgICAgICAgY29uc29sZS5lcnJvcih0ZXJtaW5hbC55ZWxsb3coYFNlZW1zIHRoYXQgJHtwYWNrYWdlTWFuYWdlcn0gaXMgbm90IGluc3RhbGxlZC5gKSk7XG4gICAgICAgICAgICBjb25zb2xlLmVycm9yKHRlcm1pbmFsLnllbGxvdygnWW91IGNhbiBgbmcgY29uZmlnIC1nIGNsaS5wYWNrYWdlTWFuYWdlciBucG1gLicpKTtcbiAgICAgICAgICB9XG4gICAgICAgIH1cbiAgICAgIH0pO1xufVxuXG5mdW5jdGlvbiBjaGVja1lhcm4oKSB7XG4gIHJldHVybiBleGVjUHJvbWlzZSgneWFybiAtLXZlcnNpb24nKVxuICAgIC50aGVuKCgpID0+IHRydWUsICgpID0+IGZhbHNlKTtcbn1cblxuZnVuY3Rpb24gY2hlY2tDTlBNKCkge1xuICByZXR1cm4gZXhlY1Byb21pc2UoJ2NucG0gLS12ZXJzaW9uJylcbiAgICAudGhlbigoKSA9PiB0cnVlLCAoKSA9PiBmYWxzZSk7XG59XG4iXX0=