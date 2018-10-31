"use strict";
/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
Object.defineProperty(exports, "__esModule", { value: true });
// tslint:disable:no-global-tslint-disable no-any
const core_1 = require("@angular-devkit/core");
const command_1 = require("../models/command");
class HelpCommand extends command_1.Command {
    run(options) {
        this.logger.info(`Available Commands:`);
        options.commandInfo
            .filter((cmd) => !cmd.hidden)
            .forEach((cmd) => {
            let aliasInfo = '';
            if (cmd.aliases.length > 0) {
                aliasInfo = ` (${cmd.aliases.join(', ')})`;
            }
            this.logger.info(`  ${core_1.terminal.cyan(cmd.name)}${aliasInfo} ${cmd.description}`);
        });
        this.logger.info(`\nFor more detailed help run "ng [command name] --help"`);
    }
    printHelp(_commandName, _description, options) {
        return this.run(options);
    }
}
exports.HelpCommand = HelpCommand;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiaGVscC1pbXBsLmpzIiwic291cmNlUm9vdCI6Ii4vIiwic291cmNlcyI6WyJwYWNrYWdlcy9hbmd1bGFyL2NsaS9jb21tYW5kcy9oZWxwLWltcGwudHMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IjtBQUFBOzs7Ozs7R0FNRzs7QUFFSCxpREFBaUQ7QUFDakQsK0NBQWdEO0FBQ2hELCtDQUE0QztBQVM1QyxpQkFBeUIsU0FBUSxpQkFBTztJQUN0QyxHQUFHLENBQUMsT0FBWTtRQUNkLElBQUksQ0FBQyxNQUFNLENBQUMsSUFBSSxDQUFDLHFCQUFxQixDQUFDLENBQUM7UUFDeEMsT0FBTyxDQUFDLFdBQVc7YUFDaEIsTUFBTSxDQUFDLENBQUMsR0FBZ0IsRUFBRSxFQUFFLENBQUMsQ0FBQyxHQUFHLENBQUMsTUFBTSxDQUFDO2FBQ3pDLE9BQU8sQ0FBQyxDQUFDLEdBQWdCLEVBQUUsRUFBRTtZQUM1QixJQUFJLFNBQVMsR0FBRyxFQUFFLENBQUM7WUFDbkIsSUFBSSxHQUFHLENBQUMsT0FBTyxDQUFDLE1BQU0sR0FBRyxDQUFDLEVBQUU7Z0JBQzFCLFNBQVMsR0FBRyxLQUFLLEdBQUcsQ0FBQyxPQUFPLENBQUMsSUFBSSxDQUFDLElBQUksQ0FBQyxHQUFHLENBQUM7YUFDNUM7WUFFRCxJQUFJLENBQUMsTUFBTSxDQUFDLElBQUksQ0FBQyxLQUFLLGVBQVEsQ0FBQyxJQUFJLENBQUMsR0FBRyxDQUFDLElBQUksQ0FBQyxHQUFHLFNBQVMsSUFBSSxHQUFHLENBQUMsV0FBVyxFQUFFLENBQUMsQ0FBQztRQUNsRixDQUFDLENBQUMsQ0FBQztRQUVMLElBQUksQ0FBQyxNQUFNLENBQUMsSUFBSSxDQUFDLHlEQUF5RCxDQUFDLENBQUM7SUFDOUUsQ0FBQztJQUVELFNBQVMsQ0FBQyxZQUFvQixFQUFFLFlBQW9CLEVBQUUsT0FBWTtRQUNoRSxPQUFPLElBQUksQ0FBQyxHQUFHLENBQUMsT0FBTyxDQUFDLENBQUM7SUFDM0IsQ0FBQztDQUNGO0FBcEJELGtDQW9CQyIsInNvdXJjZXNDb250ZW50IjpbIi8qKlxuICogQGxpY2Vuc2VcbiAqIENvcHlyaWdodCBHb29nbGUgSW5jLiBBbGwgUmlnaHRzIFJlc2VydmVkLlxuICpcbiAqIFVzZSBvZiB0aGlzIHNvdXJjZSBjb2RlIGlzIGdvdmVybmVkIGJ5IGFuIE1JVC1zdHlsZSBsaWNlbnNlIHRoYXQgY2FuIGJlXG4gKiBmb3VuZCBpbiB0aGUgTElDRU5TRSBmaWxlIGF0IGh0dHBzOi8vYW5ndWxhci5pby9saWNlbnNlXG4gKi9cblxuLy8gdHNsaW50OmRpc2FibGU6bm8tZ2xvYmFsLXRzbGludC1kaXNhYmxlIG5vLWFueVxuaW1wb3J0IHsgdGVybWluYWwgfSBmcm9tICdAYW5ndWxhci1kZXZraXQvY29yZSc7XG5pbXBvcnQgeyBDb21tYW5kIH0gZnJvbSAnLi4vbW9kZWxzL2NvbW1hbmQnO1xuXG5pbnRlcmZhY2UgQ29tbWFuZEluZm8ge1xuICBuYW1lOiBzdHJpbmc7XG4gIGRlc2NyaXB0aW9uOiBzdHJpbmc7XG4gIGhpZGRlbjogYm9vbGVhbjtcbiAgYWxpYXNlczogc3RyaW5nW107XG59XG5cbmV4cG9ydCBjbGFzcyBIZWxwQ29tbWFuZCBleHRlbmRzIENvbW1hbmQge1xuICBydW4ob3B0aW9uczogYW55KSB7XG4gICAgdGhpcy5sb2dnZXIuaW5mbyhgQXZhaWxhYmxlIENvbW1hbmRzOmApO1xuICAgIG9wdGlvbnMuY29tbWFuZEluZm9cbiAgICAgIC5maWx0ZXIoKGNtZDogQ29tbWFuZEluZm8pID0+ICFjbWQuaGlkZGVuKVxuICAgICAgLmZvckVhY2goKGNtZDogQ29tbWFuZEluZm8pID0+IHtcbiAgICAgICAgbGV0IGFsaWFzSW5mbyA9ICcnO1xuICAgICAgICBpZiAoY21kLmFsaWFzZXMubGVuZ3RoID4gMCkge1xuICAgICAgICAgIGFsaWFzSW5mbyA9IGAgKCR7Y21kLmFsaWFzZXMuam9pbignLCAnKX0pYDtcbiAgICAgICAgfVxuXG4gICAgICAgIHRoaXMubG9nZ2VyLmluZm8oYCAgJHt0ZXJtaW5hbC5jeWFuKGNtZC5uYW1lKX0ke2FsaWFzSW5mb30gJHtjbWQuZGVzY3JpcHRpb259YCk7XG4gICAgICB9KTtcblxuICAgIHRoaXMubG9nZ2VyLmluZm8oYFxcbkZvciBtb3JlIGRldGFpbGVkIGhlbHAgcnVuIFwibmcgW2NvbW1hbmQgbmFtZV0gLS1oZWxwXCJgKTtcbiAgfVxuXG4gIHByaW50SGVscChfY29tbWFuZE5hbWU6IHN0cmluZywgX2Rlc2NyaXB0aW9uOiBzdHJpbmcsIG9wdGlvbnM6IGFueSkge1xuICAgIHJldHVybiB0aGlzLnJ1bihvcHRpb25zKTtcbiAgfVxufVxuIl19