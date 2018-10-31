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
const command_1 = require("../models/command");
const schematic_command_1 = require("../models/schematic-command");
const find_up_1 = require("../utilities/find-up");
class UpdateCommand extends schematic_command_1.SchematicCommand {
    constructor() {
        super(...arguments);
        this.name = 'update';
        this.description = 'Updates your application and its dependencies.';
        this.arguments = ['packages'];
        this.options = [
            // Remove the --force flag.
            ...this.coreOptions.filter(option => option.name !== 'force'),
        ];
        this.allowMissingWorkspace = true;
        this.collectionName = '@schematics/update';
        this.schematicName = 'update';
        this.initialized = false;
    }
    initialize(options) {
        const _super = name => super[name];
        return __awaiter(this, void 0, void 0, function* () {
            if (this.initialized) {
                return;
            }
            yield _super("initialize").call(this, options);
            this.initialized = true;
            const schematicOptions = yield this.getOptions({
                schematicName: this.schematicName,
                collectionName: this.collectionName,
            });
            this.addOptions(schematicOptions);
        });
    }
    validate(options) {
        const _super = name => super[name];
        return __awaiter(this, void 0, void 0, function* () {
            if (options._[0] == '@angular/cli'
                && options.migrateOnly === undefined
                && options.from === undefined) {
                // Check for a 1.7 angular-cli.json file.
                const oldConfigFileNames = [
                    core_1.normalize('.angular-cli.json'),
                    core_1.normalize('angular-cli.json'),
                ];
                const oldConfigFilePath = find_up_1.findUp(oldConfigFileNames, process.cwd())
                    || find_up_1.findUp(oldConfigFileNames, __dirname);
                if (oldConfigFilePath) {
                    options.migrateOnly = true;
                    options.from = '1.0.0';
                }
            }
            return _super("validate").call(this, options);
        });
    }
    run(options) {
        return __awaiter(this, void 0, void 0, function* () {
            return this.runSchematic({
                collectionName: this.collectionName,
                schematicName: this.schematicName,
                schematicOptions: options,
                dryRun: options.dryRun,
                force: false,
                showNothingDone: false,
            });
        });
    }
}
UpdateCommand.aliases = [];
UpdateCommand.scope = command_1.CommandScope.everywhere;
exports.UpdateCommand = UpdateCommand;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoidXBkYXRlLWltcGwuanMiLCJzb3VyY2VSb290IjoiLi8iLCJzb3VyY2VzIjpbInBhY2thZ2VzL2FuZ3VsYXIvY2xpL2NvbW1hbmRzL3VwZGF0ZS1pbXBsLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7QUFBQTs7Ozs7O0dBTUc7Ozs7Ozs7Ozs7QUFFSCxpREFBaUQ7QUFDakQsK0NBQWlEO0FBQ2pELCtDQUF5RDtBQUN6RCxtRUFBcUY7QUFDckYsa0RBQThDO0FBUTlDLG1CQUEyQixTQUFRLG9DQUFnQjtJQUFuRDs7UUFDa0IsU0FBSSxHQUFHLFFBQVEsQ0FBQztRQUNoQixnQkFBVyxHQUFHLGdEQUFnRCxDQUFDO1FBR3hFLGNBQVMsR0FBYSxDQUFFLFVBQVUsQ0FBRSxDQUFDO1FBQ3JDLFlBQU8sR0FBYTtZQUN6QiwyQkFBMkI7WUFDM0IsR0FBRyxJQUFJLENBQUMsV0FBVyxDQUFDLE1BQU0sQ0FBQyxNQUFNLENBQUMsRUFBRSxDQUFDLE1BQU0sQ0FBQyxJQUFJLEtBQUssT0FBTyxDQUFDO1NBQzlELENBQUM7UUFDYywwQkFBcUIsR0FBRyxJQUFJLENBQUM7UUFFckMsbUJBQWMsR0FBRyxvQkFBb0IsQ0FBQztRQUN0QyxrQkFBYSxHQUFHLFFBQVEsQ0FBQztRQUV6QixnQkFBVyxHQUFHLEtBQUssQ0FBQztJQWdEOUIsQ0FBQztJQS9DYyxVQUFVLENBQUMsT0FBWTs7O1lBQ2xDLElBQUksSUFBSSxDQUFDLFdBQVcsRUFBRTtnQkFDcEIsT0FBTzthQUNSO1lBQ0QsTUFBTSxvQkFBZ0IsWUFBQyxPQUFPLENBQUMsQ0FBQztZQUNoQyxJQUFJLENBQUMsV0FBVyxHQUFHLElBQUksQ0FBQztZQUV4QixNQUFNLGdCQUFnQixHQUFHLE1BQU0sSUFBSSxDQUFDLFVBQVUsQ0FBQztnQkFDN0MsYUFBYSxFQUFFLElBQUksQ0FBQyxhQUFhO2dCQUNqQyxjQUFjLEVBQUUsSUFBSSxDQUFDLGNBQWM7YUFDcEMsQ0FBQyxDQUFDO1lBQ0gsSUFBSSxDQUFDLFVBQVUsQ0FBQyxnQkFBZ0IsQ0FBQyxDQUFDO1FBQ3BDLENBQUM7S0FBQTtJQUVLLFFBQVEsQ0FBQyxPQUFZOzs7WUFDekIsSUFBSSxPQUFPLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQyxJQUFJLGNBQWM7bUJBQzNCLE9BQU8sQ0FBQyxXQUFXLEtBQUssU0FBUzttQkFDakMsT0FBTyxDQUFDLElBQUksS0FBSyxTQUFTLEVBQUU7Z0JBQ2pDLHlDQUF5QztnQkFDekMsTUFBTSxrQkFBa0IsR0FBRztvQkFDekIsZ0JBQVMsQ0FBQyxtQkFBbUIsQ0FBQztvQkFDOUIsZ0JBQVMsQ0FBQyxrQkFBa0IsQ0FBQztpQkFDOUIsQ0FBQztnQkFDRixNQUFNLGlCQUFpQixHQUNyQixnQkFBTSxDQUFDLGtCQUFrQixFQUFFLE9BQU8sQ0FBQyxHQUFHLEVBQUUsQ0FBQzt1QkFDdEMsZ0JBQU0sQ0FBQyxrQkFBa0IsRUFBRSxTQUFTLENBQUMsQ0FBQztnQkFFM0MsSUFBSSxpQkFBaUIsRUFBRTtvQkFDckIsT0FBTyxDQUFDLFdBQVcsR0FBRyxJQUFJLENBQUM7b0JBQzNCLE9BQU8sQ0FBQyxJQUFJLEdBQUcsT0FBTyxDQUFDO2lCQUN4QjthQUNGO1lBRUQsT0FBTyxrQkFBYyxZQUFDLE9BQU8sRUFBRTtRQUNqQyxDQUFDO0tBQUE7SUFHWSxHQUFHLENBQUMsT0FBc0I7O1lBQ3JDLE9BQU8sSUFBSSxDQUFDLFlBQVksQ0FBQztnQkFDdkIsY0FBYyxFQUFFLElBQUksQ0FBQyxjQUFjO2dCQUNuQyxhQUFhLEVBQUUsSUFBSSxDQUFDLGFBQWE7Z0JBQ2pDLGdCQUFnQixFQUFFLE9BQU87Z0JBQ3pCLE1BQU0sRUFBRSxPQUFPLENBQUMsTUFBTTtnQkFDdEIsS0FBSyxFQUFFLEtBQUs7Z0JBQ1osZUFBZSxFQUFFLEtBQUs7YUFDdkIsQ0FBQyxDQUFDO1FBQ0wsQ0FBQztLQUFBOztBQTNEYSxxQkFBTyxHQUFhLEVBQUUsQ0FBQztBQUN2QixtQkFBSyxHQUFHLHNCQUFZLENBQUMsVUFBVSxDQUFDO0FBSmhELHNDQStEQyIsInNvdXJjZXNDb250ZW50IjpbIi8qKlxuICogQGxpY2Vuc2VcbiAqIENvcHlyaWdodCBHb29nbGUgSW5jLiBBbGwgUmlnaHRzIFJlc2VydmVkLlxuICpcbiAqIFVzZSBvZiB0aGlzIHNvdXJjZSBjb2RlIGlzIGdvdmVybmVkIGJ5IGFuIE1JVC1zdHlsZSBsaWNlbnNlIHRoYXQgY2FuIGJlXG4gKiBmb3VuZCBpbiB0aGUgTElDRU5TRSBmaWxlIGF0IGh0dHBzOi8vYW5ndWxhci5pby9saWNlbnNlXG4gKi9cblxuLy8gdHNsaW50OmRpc2FibGU6bm8tZ2xvYmFsLXRzbGludC1kaXNhYmxlIG5vLWFueVxuaW1wb3J0IHsgbm9ybWFsaXplIH0gZnJvbSAnQGFuZ3VsYXItZGV2a2l0L2NvcmUnO1xuaW1wb3J0IHsgQ29tbWFuZFNjb3BlLCBPcHRpb24gfSBmcm9tICcuLi9tb2RlbHMvY29tbWFuZCc7XG5pbXBvcnQgeyBDb3JlU2NoZW1hdGljT3B0aW9ucywgU2NoZW1hdGljQ29tbWFuZCB9IGZyb20gJy4uL21vZGVscy9zY2hlbWF0aWMtY29tbWFuZCc7XG5pbXBvcnQgeyBmaW5kVXAgfSBmcm9tICcuLi91dGlsaXRpZXMvZmluZC11cCc7XG5cbmV4cG9ydCBpbnRlcmZhY2UgVXBkYXRlT3B0aW9ucyBleHRlbmRzIENvcmVTY2hlbWF0aWNPcHRpb25zIHtcbiAgbmV4dDogYm9vbGVhbjtcbiAgc2NoZW1hdGljPzogYm9vbGVhbjtcbn1cblxuXG5leHBvcnQgY2xhc3MgVXBkYXRlQ29tbWFuZCBleHRlbmRzIFNjaGVtYXRpY0NvbW1hbmQge1xuICBwdWJsaWMgcmVhZG9ubHkgbmFtZSA9ICd1cGRhdGUnO1xuICBwdWJsaWMgcmVhZG9ubHkgZGVzY3JpcHRpb24gPSAnVXBkYXRlcyB5b3VyIGFwcGxpY2F0aW9uIGFuZCBpdHMgZGVwZW5kZW5jaWVzLic7XG4gIHB1YmxpYyBzdGF0aWMgYWxpYXNlczogc3RyaW5nW10gPSBbXTtcbiAgcHVibGljIHN0YXRpYyBzY29wZSA9IENvbW1hbmRTY29wZS5ldmVyeXdoZXJlO1xuICBwdWJsaWMgYXJndW1lbnRzOiBzdHJpbmdbXSA9IFsgJ3BhY2thZ2VzJyBdO1xuICBwdWJsaWMgb3B0aW9uczogT3B0aW9uW10gPSBbXG4gICAgLy8gUmVtb3ZlIHRoZSAtLWZvcmNlIGZsYWcuXG4gICAgLi4udGhpcy5jb3JlT3B0aW9ucy5maWx0ZXIob3B0aW9uID0+IG9wdGlvbi5uYW1lICE9PSAnZm9yY2UnKSxcbiAgXTtcbiAgcHVibGljIHJlYWRvbmx5IGFsbG93TWlzc2luZ1dvcmtzcGFjZSA9IHRydWU7XG5cbiAgcHJpdmF0ZSBjb2xsZWN0aW9uTmFtZSA9ICdAc2NoZW1hdGljcy91cGRhdGUnO1xuICBwcml2YXRlIHNjaGVtYXRpY05hbWUgPSAndXBkYXRlJztcblxuICBwcml2YXRlIGluaXRpYWxpemVkID0gZmFsc2U7XG4gIHB1YmxpYyBhc3luYyBpbml0aWFsaXplKG9wdGlvbnM6IGFueSkge1xuICAgIGlmICh0aGlzLmluaXRpYWxpemVkKSB7XG4gICAgICByZXR1cm47XG4gICAgfVxuICAgIGF3YWl0IHN1cGVyLmluaXRpYWxpemUob3B0aW9ucyk7XG4gICAgdGhpcy5pbml0aWFsaXplZCA9IHRydWU7XG5cbiAgICBjb25zdCBzY2hlbWF0aWNPcHRpb25zID0gYXdhaXQgdGhpcy5nZXRPcHRpb25zKHtcbiAgICAgIHNjaGVtYXRpY05hbWU6IHRoaXMuc2NoZW1hdGljTmFtZSxcbiAgICAgIGNvbGxlY3Rpb25OYW1lOiB0aGlzLmNvbGxlY3Rpb25OYW1lLFxuICAgIH0pO1xuICAgIHRoaXMuYWRkT3B0aW9ucyhzY2hlbWF0aWNPcHRpb25zKTtcbiAgfVxuXG4gIGFzeW5jIHZhbGlkYXRlKG9wdGlvbnM6IGFueSkge1xuICAgIGlmIChvcHRpb25zLl9bMF0gPT0gJ0Bhbmd1bGFyL2NsaSdcbiAgICAgICAgJiYgb3B0aW9ucy5taWdyYXRlT25seSA9PT0gdW5kZWZpbmVkXG4gICAgICAgICYmIG9wdGlvbnMuZnJvbSA9PT0gdW5kZWZpbmVkKSB7XG4gICAgICAvLyBDaGVjayBmb3IgYSAxLjcgYW5ndWxhci1jbGkuanNvbiBmaWxlLlxuICAgICAgY29uc3Qgb2xkQ29uZmlnRmlsZU5hbWVzID0gW1xuICAgICAgICBub3JtYWxpemUoJy5hbmd1bGFyLWNsaS5qc29uJyksXG4gICAgICAgIG5vcm1hbGl6ZSgnYW5ndWxhci1jbGkuanNvbicpLFxuICAgICAgXTtcbiAgICAgIGNvbnN0IG9sZENvbmZpZ0ZpbGVQYXRoID1cbiAgICAgICAgZmluZFVwKG9sZENvbmZpZ0ZpbGVOYW1lcywgcHJvY2Vzcy5jd2QoKSlcbiAgICAgICAgfHwgZmluZFVwKG9sZENvbmZpZ0ZpbGVOYW1lcywgX19kaXJuYW1lKTtcblxuICAgICAgaWYgKG9sZENvbmZpZ0ZpbGVQYXRoKSB7XG4gICAgICAgIG9wdGlvbnMubWlncmF0ZU9ubHkgPSB0cnVlO1xuICAgICAgICBvcHRpb25zLmZyb20gPSAnMS4wLjAnO1xuICAgICAgfVxuICAgIH1cblxuICAgIHJldHVybiBzdXBlci52YWxpZGF0ZShvcHRpb25zKTtcbiAgfVxuXG5cbiAgcHVibGljIGFzeW5jIHJ1bihvcHRpb25zOiBVcGRhdGVPcHRpb25zKSB7XG4gICAgcmV0dXJuIHRoaXMucnVuU2NoZW1hdGljKHtcbiAgICAgIGNvbGxlY3Rpb25OYW1lOiB0aGlzLmNvbGxlY3Rpb25OYW1lLFxuICAgICAgc2NoZW1hdGljTmFtZTogdGhpcy5zY2hlbWF0aWNOYW1lLFxuICAgICAgc2NoZW1hdGljT3B0aW9uczogb3B0aW9ucyxcbiAgICAgIGRyeVJ1bjogb3B0aW9ucy5kcnlSdW4sXG4gICAgICBmb3JjZTogZmFsc2UsXG4gICAgICBzaG93Tm90aGluZ0RvbmU6IGZhbHNlLFxuICAgIH0pO1xuICB9XG59XG4iXX0=