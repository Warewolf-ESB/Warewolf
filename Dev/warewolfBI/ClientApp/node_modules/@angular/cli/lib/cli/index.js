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
const core_1 = require("@angular-devkit/core");
const operators_1 = require("rxjs/operators");
const command_runner_1 = require("../../models/command-runner");
const project_1 = require("../../utilities/project");
function default_1(options) {
    return __awaiter(this, void 0, void 0, function* () {
        // const commands = await loadCommands();
        const logger = new core_1.logging.IndentLogger('cling');
        let loggingSubscription;
        if (!options.testing) {
            loggingSubscription = initializeLogging(logger);
        }
        let projectDetails = project_1.getProjectDetails();
        if (projectDetails === null) {
            projectDetails = { root: process.cwd() };
        }
        const context = {
            project: projectDetails,
        };
        try {
            const maybeExitCode = yield command_runner_1.runCommand(options.cliArgs, logger, context);
            if (typeof maybeExitCode === 'number') {
                console.assert(Number.isInteger(maybeExitCode));
                return maybeExitCode;
            }
            return 0;
        }
        catch (err) {
            if (err instanceof Error) {
                logger.fatal(err.message);
                if (err.stack) {
                    logger.fatal(err.stack);
                }
            }
            else if (typeof err === 'string') {
                logger.fatal(err);
            }
            else if (typeof err === 'number') {
                // Log nothing.
            }
            else {
                logger.fatal('An unexpected error occurred: ' + JSON.stringify(err));
            }
            if (options.testing) {
                debugger;
                throw err;
            }
            if (loggingSubscription) {
                loggingSubscription.unsubscribe();
            }
            return 1;
        }
    });
}
exports.default = default_1;
// Initialize logging.
function initializeLogging(logger) {
    return logger
        .pipe(operators_1.filter(entry => (entry.level != 'debug')))
        .subscribe(entry => {
        let color = (x) => core_1.terminal.dim(core_1.terminal.white(x));
        let output = process.stdout;
        switch (entry.level) {
            case 'info':
                color = core_1.terminal.white;
                break;
            case 'warn':
                color = core_1.terminal.yellow;
                break;
            case 'error':
                color = core_1.terminal.red;
                output = process.stderr;
                break;
            case 'fatal':
                color = (x) => core_1.terminal.bold(core_1.terminal.red(x));
                output = process.stderr;
                break;
        }
        output.write(color(entry.message) + '\n');
    });
}
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiaW5kZXguanMiLCJzb3VyY2VSb290IjoiLi8iLCJzb3VyY2VzIjpbInBhY2thZ2VzL2FuZ3VsYXIvY2xpL2xpYi9jbGkvaW5kZXgudHMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IjtBQUFBOzs7Ozs7R0FNRzs7Ozs7Ozs7OztBQUVILCtDQUF5RDtBQUN6RCw4Q0FBd0M7QUFDeEMsZ0VBQXlEO0FBQ3pELHFEQUE0RDtBQUc1RCxtQkFBOEIsT0FBaUQ7O1FBQzdFLHlDQUF5QztRQUV6QyxNQUFNLE1BQU0sR0FBRyxJQUFJLGNBQU8sQ0FBQyxZQUFZLENBQUMsT0FBTyxDQUFDLENBQUM7UUFDakQsSUFBSSxtQkFBbUIsQ0FBQztRQUN4QixJQUFJLENBQUMsT0FBTyxDQUFDLE9BQU8sRUFBRTtZQUNwQixtQkFBbUIsR0FBRyxpQkFBaUIsQ0FBQyxNQUFNLENBQUMsQ0FBQztTQUNqRDtRQUVELElBQUksY0FBYyxHQUFHLDJCQUFpQixFQUFFLENBQUM7UUFDekMsSUFBSSxjQUFjLEtBQUssSUFBSSxFQUFFO1lBQzNCLGNBQWMsR0FBRyxFQUFFLElBQUksRUFBRSxPQUFPLENBQUMsR0FBRyxFQUFFLEVBQUUsQ0FBQztTQUMxQztRQUNELE1BQU0sT0FBTyxHQUFHO1lBQ2QsT0FBTyxFQUFFLGNBQWM7U0FDeEIsQ0FBQztRQUVGLElBQUk7WUFDRixNQUFNLGFBQWEsR0FBRyxNQUFNLDJCQUFVLENBQUMsT0FBTyxDQUFDLE9BQU8sRUFBRSxNQUFNLEVBQUUsT0FBTyxDQUFDLENBQUM7WUFDekUsSUFBSSxPQUFPLGFBQWEsS0FBSyxRQUFRLEVBQUU7Z0JBQ3JDLE9BQU8sQ0FBQyxNQUFNLENBQUMsTUFBTSxDQUFDLFNBQVMsQ0FBQyxhQUFhLENBQUMsQ0FBQyxDQUFDO2dCQUVoRCxPQUFPLGFBQWEsQ0FBQzthQUN0QjtZQUVELE9BQU8sQ0FBQyxDQUFDO1NBQ1Y7UUFBQyxPQUFPLEdBQUcsRUFBRTtZQUNaLElBQUksR0FBRyxZQUFZLEtBQUssRUFBRTtnQkFDeEIsTUFBTSxDQUFDLEtBQUssQ0FBQyxHQUFHLENBQUMsT0FBTyxDQUFDLENBQUM7Z0JBQzFCLElBQUksR0FBRyxDQUFDLEtBQUssRUFBRTtvQkFDYixNQUFNLENBQUMsS0FBSyxDQUFDLEdBQUcsQ0FBQyxLQUFLLENBQUMsQ0FBQztpQkFDekI7YUFDRjtpQkFBTSxJQUFJLE9BQU8sR0FBRyxLQUFLLFFBQVEsRUFBRTtnQkFDbEMsTUFBTSxDQUFDLEtBQUssQ0FBQyxHQUFHLENBQUMsQ0FBQzthQUNuQjtpQkFBTSxJQUFJLE9BQU8sR0FBRyxLQUFLLFFBQVEsRUFBRTtnQkFDbEMsZUFBZTthQUNoQjtpQkFBTTtnQkFDTCxNQUFNLENBQUMsS0FBSyxDQUFDLGdDQUFnQyxHQUFHLElBQUksQ0FBQyxTQUFTLENBQUMsR0FBRyxDQUFDLENBQUMsQ0FBQzthQUN0RTtZQUVELElBQUksT0FBTyxDQUFDLE9BQU8sRUFBRTtnQkFDbkIsUUFBUSxDQUFDO2dCQUNULE1BQU0sR0FBRyxDQUFDO2FBQ1g7WUFFRCxJQUFJLG1CQUFtQixFQUFFO2dCQUN2QixtQkFBbUIsQ0FBQyxXQUFXLEVBQUUsQ0FBQzthQUNuQztZQUVELE9BQU8sQ0FBQyxDQUFDO1NBQ1Y7SUFDSCxDQUFDO0NBQUE7QUFuREQsNEJBbURDO0FBRUQsc0JBQXNCO0FBQ3RCLDJCQUEyQixNQUFzQjtJQUMvQyxPQUFPLE1BQU07U0FDVixJQUFJLENBQUMsa0JBQU0sQ0FBQyxLQUFLLENBQUMsRUFBRSxDQUFDLENBQUMsS0FBSyxDQUFDLEtBQUssSUFBSSxPQUFPLENBQUMsQ0FBQyxDQUFDO1NBQy9DLFNBQVMsQ0FBQyxLQUFLLENBQUMsRUFBRTtRQUNqQixJQUFJLEtBQUssR0FBRyxDQUFDLENBQVMsRUFBRSxFQUFFLENBQUMsZUFBUSxDQUFDLEdBQUcsQ0FBQyxlQUFRLENBQUMsS0FBSyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUM7UUFDM0QsSUFBSSxNQUFNLEdBQUcsT0FBTyxDQUFDLE1BQU0sQ0FBQztRQUM1QixRQUFRLEtBQUssQ0FBQyxLQUFLLEVBQUU7WUFDbkIsS0FBSyxNQUFNO2dCQUNULEtBQUssR0FBRyxlQUFRLENBQUMsS0FBSyxDQUFDO2dCQUN2QixNQUFNO1lBQ1IsS0FBSyxNQUFNO2dCQUNULEtBQUssR0FBRyxlQUFRLENBQUMsTUFBTSxDQUFDO2dCQUN4QixNQUFNO1lBQ1IsS0FBSyxPQUFPO2dCQUNWLEtBQUssR0FBRyxlQUFRLENBQUMsR0FBRyxDQUFDO2dCQUNyQixNQUFNLEdBQUcsT0FBTyxDQUFDLE1BQU0sQ0FBQztnQkFDeEIsTUFBTTtZQUNSLEtBQUssT0FBTztnQkFDVixLQUFLLEdBQUcsQ0FBQyxDQUFDLEVBQUUsRUFBRSxDQUFDLGVBQVEsQ0FBQyxJQUFJLENBQUMsZUFBUSxDQUFDLEdBQUcsQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDO2dCQUM5QyxNQUFNLEdBQUcsT0FBTyxDQUFDLE1BQU0sQ0FBQztnQkFDeEIsTUFBTTtTQUNUO1FBRUQsTUFBTSxDQUFDLEtBQUssQ0FBQyxLQUFLLENBQUMsS0FBSyxDQUFDLE9BQU8sQ0FBQyxHQUFHLElBQUksQ0FBQyxDQUFDO0lBQzVDLENBQUMsQ0FBQyxDQUFDO0FBQ1AsQ0FBQyIsInNvdXJjZXNDb250ZW50IjpbIi8qKlxuICogQGxpY2Vuc2VcbiAqIENvcHlyaWdodCBHb29nbGUgSW5jLiBBbGwgUmlnaHRzIFJlc2VydmVkLlxuICpcbiAqIFVzZSBvZiB0aGlzIHNvdXJjZSBjb2RlIGlzIGdvdmVybmVkIGJ5IGFuIE1JVC1zdHlsZSBsaWNlbnNlIHRoYXQgY2FuIGJlXG4gKiBmb3VuZCBpbiB0aGUgTElDRU5TRSBmaWxlIGF0IGh0dHBzOi8vYW5ndWxhci5pby9saWNlbnNlXG4gKi9cblxuaW1wb3J0IHsgbG9nZ2luZywgdGVybWluYWwgfSBmcm9tICdAYW5ndWxhci1kZXZraXQvY29yZSc7XG5pbXBvcnQgeyBmaWx0ZXIgfSBmcm9tICdyeGpzL29wZXJhdG9ycyc7XG5pbXBvcnQgeyBydW5Db21tYW5kIH0gZnJvbSAnLi4vLi4vbW9kZWxzL2NvbW1hbmQtcnVubmVyJztcbmltcG9ydCB7IGdldFByb2plY3REZXRhaWxzIH0gZnJvbSAnLi4vLi4vdXRpbGl0aWVzL3Byb2plY3QnO1xuXG5cbmV4cG9ydCBkZWZhdWx0IGFzeW5jIGZ1bmN0aW9uKG9wdGlvbnM6IHsgdGVzdGluZz86IGJvb2xlYW4sIGNsaUFyZ3M6IHN0cmluZ1tdIH0pIHtcbiAgLy8gY29uc3QgY29tbWFuZHMgPSBhd2FpdCBsb2FkQ29tbWFuZHMoKTtcblxuICBjb25zdCBsb2dnZXIgPSBuZXcgbG9nZ2luZy5JbmRlbnRMb2dnZXIoJ2NsaW5nJyk7XG4gIGxldCBsb2dnaW5nU3Vic2NyaXB0aW9uO1xuICBpZiAoIW9wdGlvbnMudGVzdGluZykge1xuICAgIGxvZ2dpbmdTdWJzY3JpcHRpb24gPSBpbml0aWFsaXplTG9nZ2luZyhsb2dnZXIpO1xuICB9XG5cbiAgbGV0IHByb2plY3REZXRhaWxzID0gZ2V0UHJvamVjdERldGFpbHMoKTtcbiAgaWYgKHByb2plY3REZXRhaWxzID09PSBudWxsKSB7XG4gICAgcHJvamVjdERldGFpbHMgPSB7IHJvb3Q6IHByb2Nlc3MuY3dkKCkgfTtcbiAgfVxuICBjb25zdCBjb250ZXh0ID0ge1xuICAgIHByb2plY3Q6IHByb2plY3REZXRhaWxzLFxuICB9O1xuXG4gIHRyeSB7XG4gICAgY29uc3QgbWF5YmVFeGl0Q29kZSA9IGF3YWl0IHJ1bkNvbW1hbmQob3B0aW9ucy5jbGlBcmdzLCBsb2dnZXIsIGNvbnRleHQpO1xuICAgIGlmICh0eXBlb2YgbWF5YmVFeGl0Q29kZSA9PT0gJ251bWJlcicpIHtcbiAgICAgIGNvbnNvbGUuYXNzZXJ0KE51bWJlci5pc0ludGVnZXIobWF5YmVFeGl0Q29kZSkpO1xuXG4gICAgICByZXR1cm4gbWF5YmVFeGl0Q29kZTtcbiAgICB9XG5cbiAgICByZXR1cm4gMDtcbiAgfSBjYXRjaCAoZXJyKSB7XG4gICAgaWYgKGVyciBpbnN0YW5jZW9mIEVycm9yKSB7XG4gICAgICBsb2dnZXIuZmF0YWwoZXJyLm1lc3NhZ2UpO1xuICAgICAgaWYgKGVyci5zdGFjaykge1xuICAgICAgICBsb2dnZXIuZmF0YWwoZXJyLnN0YWNrKTtcbiAgICAgIH1cbiAgICB9IGVsc2UgaWYgKHR5cGVvZiBlcnIgPT09ICdzdHJpbmcnKSB7XG4gICAgICBsb2dnZXIuZmF0YWwoZXJyKTtcbiAgICB9IGVsc2UgaWYgKHR5cGVvZiBlcnIgPT09ICdudW1iZXInKSB7XG4gICAgICAvLyBMb2cgbm90aGluZy5cbiAgICB9IGVsc2Uge1xuICAgICAgbG9nZ2VyLmZhdGFsKCdBbiB1bmV4cGVjdGVkIGVycm9yIG9jY3VycmVkOiAnICsgSlNPTi5zdHJpbmdpZnkoZXJyKSk7XG4gICAgfVxuXG4gICAgaWYgKG9wdGlvbnMudGVzdGluZykge1xuICAgICAgZGVidWdnZXI7XG4gICAgICB0aHJvdyBlcnI7XG4gICAgfVxuXG4gICAgaWYgKGxvZ2dpbmdTdWJzY3JpcHRpb24pIHtcbiAgICAgIGxvZ2dpbmdTdWJzY3JpcHRpb24udW5zdWJzY3JpYmUoKTtcbiAgICB9XG5cbiAgICByZXR1cm4gMTtcbiAgfVxufVxuXG4vLyBJbml0aWFsaXplIGxvZ2dpbmcuXG5mdW5jdGlvbiBpbml0aWFsaXplTG9nZ2luZyhsb2dnZXI6IGxvZ2dpbmcuTG9nZ2VyKSB7XG4gIHJldHVybiBsb2dnZXJcbiAgICAucGlwZShmaWx0ZXIoZW50cnkgPT4gKGVudHJ5LmxldmVsICE9ICdkZWJ1ZycpKSlcbiAgICAuc3Vic2NyaWJlKGVudHJ5ID0+IHtcbiAgICAgIGxldCBjb2xvciA9ICh4OiBzdHJpbmcpID0+IHRlcm1pbmFsLmRpbSh0ZXJtaW5hbC53aGl0ZSh4KSk7XG4gICAgICBsZXQgb3V0cHV0ID0gcHJvY2Vzcy5zdGRvdXQ7XG4gICAgICBzd2l0Y2ggKGVudHJ5LmxldmVsKSB7XG4gICAgICAgIGNhc2UgJ2luZm8nOlxuICAgICAgICAgIGNvbG9yID0gdGVybWluYWwud2hpdGU7XG4gICAgICAgICAgYnJlYWs7XG4gICAgICAgIGNhc2UgJ3dhcm4nOlxuICAgICAgICAgIGNvbG9yID0gdGVybWluYWwueWVsbG93O1xuICAgICAgICAgIGJyZWFrO1xuICAgICAgICBjYXNlICdlcnJvcic6XG4gICAgICAgICAgY29sb3IgPSB0ZXJtaW5hbC5yZWQ7XG4gICAgICAgICAgb3V0cHV0ID0gcHJvY2Vzcy5zdGRlcnI7XG4gICAgICAgICAgYnJlYWs7XG4gICAgICAgIGNhc2UgJ2ZhdGFsJzpcbiAgICAgICAgICBjb2xvciA9ICh4KSA9PiB0ZXJtaW5hbC5ib2xkKHRlcm1pbmFsLnJlZCh4KSk7XG4gICAgICAgICAgb3V0cHV0ID0gcHJvY2Vzcy5zdGRlcnI7XG4gICAgICAgICAgYnJlYWs7XG4gICAgICB9XG5cbiAgICAgIG91dHB1dC53cml0ZShjb2xvcihlbnRyeS5tZXNzYWdlKSArICdcXG4nKTtcbiAgICB9KTtcbn1cbiJdfQ==