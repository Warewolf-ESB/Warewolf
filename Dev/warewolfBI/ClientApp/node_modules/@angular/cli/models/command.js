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
// tslint:disable:no-global-tslint-disable no-any
const core_1 = require("@angular-devkit/core");
var CommandScope;
(function (CommandScope) {
    CommandScope[CommandScope["everywhere"] = 0] = "everywhere";
    CommandScope[CommandScope["inProject"] = 1] = "inProject";
    CommandScope[CommandScope["outsideProject"] = 2] = "outsideProject";
})(CommandScope = exports.CommandScope || (exports.CommandScope = {}));
var ArgumentStrategy;
(function (ArgumentStrategy) {
    ArgumentStrategy[ArgumentStrategy["MapToOptions"] = 0] = "MapToOptions";
    ArgumentStrategy[ArgumentStrategy["Nothing"] = 1] = "Nothing";
})(ArgumentStrategy = exports.ArgumentStrategy || (exports.ArgumentStrategy = {}));
class Command {
    constructor(context, logger) {
        this.allowMissingWorkspace = false;
        this.additionalSchemas = [];
        this.logger = logger;
        if (context) {
            this.project = context.project;
        }
    }
    addOptions(options) {
        this.options = (this.options || []).concat(options);
    }
    initializeRaw(args) {
        return __awaiter(this, void 0, void 0, function* () {
            this._rawArgs = args;
            return args;
        });
    }
    initialize(_options) {
        return __awaiter(this, void 0, void 0, function* () {
            return;
        });
    }
    validate(_options) {
        return true;
    }
    printHelp(commandName, description, options) {
        if (description) {
            this.logger.info(description);
        }
        this.printHelpUsage(commandName, this.options);
        this.printHelpOptions(this.options);
    }
    _getArguments(options) {
        function _getArgIndex(def) {
            if (def === undefined || def.$source !== 'argv' || typeof def.index !== 'number') {
                // If there's no proper order, this argument is wonky. We will show it at the end only
                // (after all other arguments).
                return Infinity;
            }
            return def.index;
        }
        return options
            .filter(opt => this.isArgument(opt))
            .sort((a, b) => _getArgIndex(a.$default) - _getArgIndex(b.$default));
    }
    printHelpUsage(name, options) {
        const args = this._getArguments(options);
        const opts = options.filter(opt => !this.isArgument(opt));
        const argDisplay = args && args.length > 0
            ? ' ' + args.map(a => `<${a.name}>`).join(' ')
            : '';
        const optionsDisplay = opts && opts.length > 0
            ? ` [options]`
            : ``;
        this.logger.info(`usage: ng ${name}${argDisplay}${optionsDisplay}`);
    }
    isArgument(option) {
        let isArg = false;
        if (option.$default !== undefined && option.$default.$source === 'argv') {
            isArg = true;
        }
        return isArg;
    }
    printHelpOptions(options) {
        if (!options) {
            return;
        }
        const args = options.filter(opt => this.isArgument(opt));
        const opts = options.filter(opt => !this.isArgument(opt));
        if (args.length > 0) {
            this.logger.info(`arguments:`);
            args.forEach(o => {
                this.logger.info(`  ${core_1.terminal.cyan(o.name)}`);
                this.logger.info(`    ${o.description}`);
            });
        }
        if (this.options.length > 0) {
            this.logger.info(`options:`);
            opts
                .filter(o => !o.hidden)
                .sort((a, b) => a.name >= b.name ? 1 : -1)
                .forEach(o => {
                const aliases = o.aliases && o.aliases.length > 0
                    ? '(' + o.aliases.map(a => `-${a}`).join(' ') + ')'
                    : '';
                this.logger.info(`  ${core_1.terminal.cyan('--' + o.name)} ${aliases}`);
                this.logger.info(`    ${o.description}`);
            });
        }
    }
}
exports.Command = Command;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiY29tbWFuZC5qcyIsInNvdXJjZVJvb3QiOiIuLyIsInNvdXJjZXMiOlsicGFja2FnZXMvYW5ndWxhci9jbGkvbW9kZWxzL2NvbW1hbmQudHMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6IjtBQUFBOzs7Ozs7R0FNRzs7Ozs7Ozs7OztBQUVILGlEQUFpRDtBQUNqRCwrQ0FBb0U7QUFTcEUsSUFBWSxZQUlYO0FBSkQsV0FBWSxZQUFZO0lBQ3RCLDJEQUFVLENBQUE7SUFDVix5REFBUyxDQUFBO0lBQ1QsbUVBQWMsQ0FBQTtBQUNoQixDQUFDLEVBSlcsWUFBWSxHQUFaLG9CQUFZLEtBQVosb0JBQVksUUFJdkI7QUFFRCxJQUFZLGdCQUdYO0FBSEQsV0FBWSxnQkFBZ0I7SUFDMUIsdUVBQVksQ0FBQTtJQUNaLDZEQUFPLENBQUE7QUFDVCxDQUFDLEVBSFcsZ0JBQWdCLEdBQWhCLHdCQUFnQixLQUFoQix3QkFBZ0IsUUFHM0I7QUFFRDtJQUlFLFlBQVksT0FBdUIsRUFBRSxNQUFzQjtRQUZwRCwwQkFBcUIsR0FBRyxLQUFLLENBQUM7UUFxRzlCLHNCQUFpQixHQUFhLEVBQUUsQ0FBQztRQWxHdEMsSUFBSSxDQUFDLE1BQU0sR0FBRyxNQUFNLENBQUM7UUFDckIsSUFBSSxPQUFPLEVBQUU7WUFDWCxJQUFJLENBQUMsT0FBTyxHQUFHLE9BQU8sQ0FBQyxPQUFPLENBQUM7U0FDaEM7SUFDSCxDQUFDO0lBRU0sVUFBVSxDQUFDLE9BQWlCO1FBQ2pDLElBQUksQ0FBQyxPQUFPLEdBQUcsQ0FBQyxJQUFJLENBQUMsT0FBTyxJQUFJLEVBQUUsQ0FBQyxDQUFDLE1BQU0sQ0FBQyxPQUFPLENBQUMsQ0FBQztJQUN0RCxDQUFDO0lBRUssYUFBYSxDQUFDLElBQWM7O1lBQ2hDLElBQUksQ0FBQyxRQUFRLEdBQUcsSUFBSSxDQUFDO1lBRXJCLE9BQU8sSUFBSSxDQUFDO1FBQ2QsQ0FBQztLQUFBO0lBQ0ssVUFBVSxDQUFDLFFBQWE7O1lBQzVCLE9BQU87UUFDVCxDQUFDO0tBQUE7SUFF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O2dCQUNQLElBQUksQ0FBQyxNQUFNLENBQUMsSUFBSSxDQUFDLEtBQUssZUFBUSxDQUFDLElBQUksQ0FBQyxJQUFJLEdBQUcsQ0FBQyxDQUFDLElBQUksQ0FBQyxJQUFJLE9BQU8sRUFBRSxDQUFDLENBQUM7Z0JBQ2pFLElBQUksQ0FBQyxNQUFNLENBQUMsSUFBSSxDQUFDLE9BQU8sQ0FBQyxDQUFDLFdBQVcsRUFBRSxDQUFDLENBQUM7WUFDM0MsQ0FBQyxDQUFDLENBQUM7U0FDTjtJQUNILENBQUM7Q0FPRjtBQTFHRCwwQkEwR0MiLCJzb3VyY2VzQ29udGVudCI6WyIvKipcbiAqIEBsaWNlbnNlXG4gKiBDb3B5cmlnaHQgR29vZ2xlIEluYy4gQWxsIFJpZ2h0cyBSZXNlcnZlZC5cbiAqXG4gKiBVc2Ugb2YgdGhpcyBzb3VyY2UgY29kZSBpcyBnb3Zlcm5lZCBieSBhbiBNSVQtc3R5bGUgbGljZW5zZSB0aGF0IGNhbiBiZVxuICogZm91bmQgaW4gdGhlIExJQ0VOU0UgZmlsZSBhdCBodHRwczovL2FuZ3VsYXIuaW8vbGljZW5zZVxuICovXG5cbi8vIHRzbGludDpkaXNhYmxlOm5vLWdsb2JhbC10c2xpbnQtZGlzYWJsZSBuby1hbnlcbmltcG9ydCB7IEpzb25WYWx1ZSwgbG9nZ2luZywgdGVybWluYWwgfSBmcm9tICdAYW5ndWxhci1kZXZraXQvY29yZSc7XG5cbmV4cG9ydCBpbnRlcmZhY2UgQ29tbWFuZENvbnN0cnVjdG9yIHtcbiAgbmV3KGNvbnRleHQ6IENvbW1hbmRDb250ZXh0LCBsb2dnZXI6IGxvZ2dpbmcuTG9nZ2VyKTogQ29tbWFuZDtcbiAgcmVhZG9ubHkgbmFtZTogc3RyaW5nO1xuICBhbGlhc2VzOiBzdHJpbmdbXTtcbiAgc2NvcGU6IENvbW1hbmRTY29wZTtcbn1cblxuZXhwb3J0IGVudW0gQ29tbWFuZFNjb3BlIHtcbiAgZXZlcnl3aGVyZSxcbiAgaW5Qcm9qZWN0LFxuICBvdXRzaWRlUHJvamVjdCxcbn1cblxuZXhwb3J0IGVudW0gQXJndW1lbnRTdHJhdGVneSB7XG4gIE1hcFRvT3B0aW9ucyxcbiAgTm90aGluZyxcbn1cblxuZXhwb3J0IGFic3RyYWN0IGNsYXNzIENvbW1hbmQ8VCA9IGFueT4ge1xuICBwcm90ZWN0ZWQgX3Jhd0FyZ3M6IHN0cmluZ1tdO1xuICBwdWJsaWMgYWxsb3dNaXNzaW5nV29ya3NwYWNlID0gZmFsc2U7XG5cbiAgY29uc3RydWN0b3IoY29udGV4dDogQ29tbWFuZENvbnRleHQsIGxvZ2dlcjogbG9nZ2luZy5Mb2dnZXIpIHtcbiAgICB0aGlzLmxvZ2dlciA9IGxvZ2dlcjtcbiAgICBpZiAoY29udGV4dCkge1xuICAgICAgdGhpcy5wcm9qZWN0ID0gY29udGV4dC5wcm9qZWN0O1xuICAgIH1cbiAgfVxuXG4gIHB1YmxpYyBhZGRPcHRpb25zKG9wdGlvbnM6IE9wdGlvbltdKSB7XG4gICAgdGhpcy5vcHRpb25zID0gKHRoaXMub3B0aW9ucyB8fCBbXSkuY29uY2F0KG9wdGlvbnMpO1xuICB9XG5cbiAgYXN5bmMgaW5pdGlhbGl6ZVJhdyhhcmdzOiBzdHJpbmdbXSk6IFByb21pc2U8YW55PiB7XG4gICAgdGhpcy5fcmF3QXJncyA9IGFyZ3M7XG5cbiAgICByZXR1cm4gYXJncztcbiAgfVxuICBhc3luYyBpbml0aWFsaXplKF9vcHRpb25zOiBhbnkpOiBQcm9taXNlPHZvaWQ+IHtcbiAgICByZXR1cm47XG4gIH1cblxuICB2YWxpZGF0ZShfb3B0aW9uczogVCk6IGJvb2xlYW4gfCBQcm9taXNlPGJvb2xlYW4+IHtcbiAgICByZXR1cm4gdHJ1ZTtcbiAgfVxuXG4gIHByaW50SGVscChjb21tYW5kTmFtZTogc3RyaW5nLCBkZXNjcmlwdGlvbjogc3RyaW5nLCBvcHRpb25zOiBhbnkpOiB2b2lkIHtcbiAgICBpZiAoZGVzY3JpcHRpb24pIHtcbiAgICAgIHRoaXMubG9nZ2VyLmluZm8oZGVzY3JpcHRpb24pO1xuICAgIH1cbiAgICB0aGlzLnByaW50SGVscFVzYWdlKGNvbW1hbmROYW1lLCB0aGlzLm9wdGlvbnMpO1xuICAgIHRoaXMucHJpbnRIZWxwT3B0aW9ucyh0aGlzLm9wdGlvbnMpO1xuICB9XG5cbiAgcHJpdmF0ZSBfZ2V0QXJndW1lbnRzKG9wdGlvbnM6IE9wdGlvbltdKSB7XG4gICAgZnVuY3Rpb24gX2dldEFyZ0luZGV4KGRlZjogT3B0aW9uU21hcnREZWZhdWx0IHwgdW5kZWZpbmVkKTogbnVtYmVyIHtcbiAgICAgIGlmIChkZWYgPT09IHVuZGVmaW5lZCB8fCBkZWYuJHNvdXJjZSAhPT0gJ2FyZ3YnIHx8IHR5cGVvZiBkZWYuaW5kZXggIT09ICdudW1iZXInKSB7XG4gICAgICAgIC8vIElmIHRoZXJlJ3Mgbm8gcHJvcGVyIG9yZGVyLCB0aGlzIGFyZ3VtZW50IGlzIHdvbmt5LiBXZSB3aWxsIHNob3cgaXQgYXQgdGhlIGVuZCBvbmx5XG4gICAgICAgIC8vIChhZnRlciBhbGwgb3RoZXIgYXJndW1lbnRzKS5cbiAgICAgICAgcmV0dXJuIEluZmluaXR5O1xuICAgICAgfVxuXG4gICAgICByZXR1cm4gZGVmLmluZGV4O1xuICAgIH1cblxuICAgIHJldHVybiBvcHRpb25zXG4gICAgICAuZmlsdGVyKG9wdCA9PiB0aGlzLmlzQXJndW1lbnQob3B0KSlcbiAgICAgIC5zb3J0KChhLCBiKSA9PiBfZ2V0QXJnSW5kZXgoYS4kZGVmYXVsdCkgLSBfZ2V0QXJnSW5kZXgoYi4kZGVmYXVsdCkpO1xuICB9XG5cbiAgcHJvdGVjdGVkIHByaW50SGVscFVzYWdlKG5hbWU6IHN0cmluZywgb3B0aW9uczogT3B0aW9uW10pIHtcbiAgICBjb25zdCBhcmdzID0gdGhpcy5fZ2V0QXJndW1lbnRzKG9wdGlvbnMpO1xuICAgIGNvbnN0IG9wdHMgPSBvcHRpb25zLmZpbHRlcihvcHQgPT4gIXRoaXMuaXNBcmd1bWVudChvcHQpKTtcbiAgICBjb25zdCBhcmdEaXNwbGF5ID0gYXJncyAmJiBhcmdzLmxlbmd0aCA+IDBcbiAgICAgID8gJyAnICsgYXJncy5tYXAoYSA9PiBgPCR7YS5uYW1lfT5gKS5qb2luKCcgJylcbiAgICAgIDogJyc7XG4gICAgY29uc3Qgb3B0aW9uc0Rpc3BsYXkgPSBvcHRzICYmIG9wdHMubGVuZ3RoID4gMFxuICAgICAgPyBgIFtvcHRpb25zXWBcbiAgICAgIDogYGA7XG4gICAgdGhpcy5sb2dnZXIuaW5mbyhgdXNhZ2U6IG5nICR7bmFtZX0ke2FyZ0Rpc3BsYXl9JHtvcHRpb25zRGlzcGxheX1gKTtcbiAgfVxuXG4gIHByb3RlY3RlZCBpc0FyZ3VtZW50KG9wdGlvbjogT3B0aW9uKSB7XG4gICAgbGV0IGlzQXJnID0gZmFsc2U7XG4gICAgaWYgKG9wdGlvbi4kZGVmYXVsdCAhPT0gdW5kZWZpbmVkICYmIG9wdGlvbi4kZGVmYXVsdC4kc291cmNlID09PSAnYXJndicpIHtcbiAgICAgIGlzQXJnID0gdHJ1ZTtcbiAgICB9XG5cbiAgICByZXR1cm4gaXNBcmc7XG4gIH1cblxuICBwcm90ZWN0ZWQgcHJpbnRIZWxwT3B0aW9ucyhvcHRpb25zOiBPcHRpb25bXSkge1xuICAgIGlmICghb3B0aW9ucykge1xuICAgICAgcmV0dXJuO1xuICAgIH1cbiAgICBjb25zdCBhcmdzID0gb3B0aW9ucy5maWx0ZXIob3B0ID0+IHRoaXMuaXNBcmd1bWVudChvcHQpKTtcbiAgICBjb25zdCBvcHRzID0gb3B0aW9ucy5maWx0ZXIob3B0ID0+ICF0aGlzLmlzQXJndW1lbnQob3B0KSk7XG4gICAgaWYgKGFyZ3MubGVuZ3RoID4gMCkge1xuICAgICAgdGhpcy5sb2dnZXIuaW5mbyhgYXJndW1lbnRzOmApO1xuICAgICAgYXJncy5mb3JFYWNoKG8gPT4ge1xuICAgICAgICB0aGlzLmxvZ2dlci5pbmZvKGAgICR7dGVybWluYWwuY3lhbihvLm5hbWUpfWApO1xuICAgICAgICB0aGlzLmxvZ2dlci5pbmZvKGAgICAgJHtvLmRlc2NyaXB0aW9ufWApO1xuICAgICAgfSk7XG4gICAgfVxuICAgIGlmICh0aGlzLm9wdGlvbnMubGVuZ3RoID4gMCkge1xuICAgICAgdGhpcy5sb2dnZXIuaW5mbyhgb3B0aW9uczpgKTtcbiAgICAgIG9wdHNcbiAgICAgICAgLmZpbHRlcihvID0+ICFvLmhpZGRlbilcbiAgICAgICAgLnNvcnQoKGEsIGIpID0+IGEubmFtZSA+PSBiLm5hbWUgPyAxIDogLTEpXG4gICAgICAgIC5mb3JFYWNoKG8gPT4ge1xuICAgICAgICAgIGNvbnN0IGFsaWFzZXMgPSBvLmFsaWFzZXMgJiYgby5hbGlhc2VzLmxlbmd0aCA+IDBcbiAgICAgICAgICAgID8gJygnICsgby5hbGlhc2VzLm1hcChhID0+IGAtJHthfWApLmpvaW4oJyAnKSArICcpJ1xuICAgICAgICAgICAgOiAnJztcbiAgICAgICAgICB0aGlzLmxvZ2dlci5pbmZvKGAgICR7dGVybWluYWwuY3lhbignLS0nICsgby5uYW1lKX0gJHthbGlhc2VzfWApO1xuICAgICAgICAgIHRoaXMubG9nZ2VyLmluZm8oYCAgICAke28uZGVzY3JpcHRpb259YCk7XG4gICAgICAgIH0pO1xuICAgIH1cbiAgfVxuXG4gIGFic3RyYWN0IHJ1bihvcHRpb25zOiBUKTogbnVtYmVyIHwgdm9pZCB8IFByb21pc2U8bnVtYmVyIHwgdm9pZD47XG4gIHB1YmxpYyBvcHRpb25zOiBPcHRpb25bXTtcbiAgcHVibGljIGFkZGl0aW9uYWxTY2hlbWFzOiBzdHJpbmdbXSA9IFtdO1xuICBwcm90ZWN0ZWQgcmVhZG9ubHkgbG9nZ2VyOiBsb2dnaW5nLkxvZ2dlcjtcbiAgcHJvdGVjdGVkIHJlYWRvbmx5IHByb2plY3Q6IGFueTtcbn1cblxuZXhwb3J0IGludGVyZmFjZSBDb21tYW5kQ29udGV4dCB7XG4gIHByb2plY3Q6IGFueTtcbn1cblxuZXhwb3J0IGludGVyZmFjZSBPcHRpb24ge1xuICBuYW1lOiBzdHJpbmc7XG4gIGRlc2NyaXB0aW9uOiBzdHJpbmc7XG4gIHR5cGU6IHN0cmluZztcbiAgZGVmYXVsdD86IHN0cmluZyB8IG51bWJlciB8IGJvb2xlYW47XG4gIHJlcXVpcmVkPzogYm9vbGVhbjtcbiAgYWxpYXNlcz86IHN0cmluZ1tdO1xuICBmb3JtYXQ/OiBzdHJpbmc7XG4gIGhpZGRlbj86IGJvb2xlYW47XG4gICRkZWZhdWx0PzogT3B0aW9uU21hcnREZWZhdWx0O1xufVxuXG5leHBvcnQgaW50ZXJmYWNlIE9wdGlvblNtYXJ0RGVmYXVsdCB7XG4gICRzb3VyY2U6IHN0cmluZztcbiAgW2tleTogc3RyaW5nXTogSnNvblZhbHVlO1xufVxuIl19