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
const tools_1 = require("@angular-devkit/schematics/tools");
const command_runner_1 = require("../models/command-runner");
const schematic_command_1 = require("../models/schematic-command");
const config_1 = require("../utilities/config");
class AddCommand extends schematic_command_1.SchematicCommand {
    constructor() {
        super(...arguments);
        this.allowPrivateSchematics = true;
    }
    _parseSchematicOptions(collectionName) {
        return __awaiter(this, void 0, void 0, function* () {
            const schematicOptions = yield this.getOptions({
                schematicName: 'ng-add',
                collectionName,
            });
            this.addOptions(schematicOptions);
            return command_runner_1.parseOptions(this._rawArgs, this.options);
        });
    }
    validate(options) {
        const collectionName = options._[0];
        if (!collectionName) {
            this.logger.fatal(`The "ng add" command requires a name argument to be specified eg. `
                + `${core_1.terminal.yellow('ng add [name] ')}. For more details, use "ng help".`);
            return false;
        }
        return true;
    }
    run(options) {
        return __awaiter(this, void 0, void 0, function* () {
            const firstArg = options._[0];
            if (!firstArg) {
                this.logger.fatal(`The "ng add" command requires a name argument to be specified eg. `
                    + `${core_1.terminal.yellow('ng add [name] ')}. For more details, use "ng help".`);
                return 1;
            }
            const packageManager = config_1.getPackageManager();
            const npmInstall = require('../tasks/npm-install').default;
            const packageName = firstArg.startsWith('@')
                ? firstArg.split('/', 2).join('/')
                : firstArg.split('/', 1)[0];
            // Remove the tag/version from the package name.
            const collectionName = (packageName.startsWith('@')
                ? packageName.split('@', 2).join('@')
                : packageName.split('@', 1).join('@')) + firstArg.slice(packageName.length);
            // We don't actually add the package to package.json, that would be the work of the package
            // itself.
            yield npmInstall(packageName, this.logger, packageManager, this.project.root);
            // Reparse the options with the new schematic accessible.
            options = yield this._parseSchematicOptions(collectionName);
            const runOptions = {
                schematicOptions: options,
                workingDir: this.project.root,
                collectionName,
                schematicName: 'ng-add',
                allowPrivate: true,
                dryRun: false,
                force: false,
            };
            try {
                return yield this.runSchematic(runOptions);
            }
            catch (e) {
                if (e instanceof tools_1.NodePackageDoesNotSupportSchematics) {
                    this.logger.error(core_1.tags.oneLine `
          The package that you are trying to add does not support schematics. You can try using
          a different version of the package or contact the package author to add ng-add support.
        `);
                    return 1;
                }
                throw e;
            }
        });
    }
}
exports.AddCommand = AddCommand;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiYWRkLWltcGwuanMiLCJzb3VyY2VSb290IjoiLi8iLCJzb3VyY2VzIjpbInBhY2thZ2VzL2FuZ3VsYXIvY2xpL2NvbW1hbmRzL2FkZC1pbXBsLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7QUFBQTs7Ozs7O0dBTUc7Ozs7Ozs7Ozs7QUFFSCxpREFBaUQ7QUFDakQsK0NBQXNEO0FBQ3RELDREQUF1RjtBQUN2Riw2REFBd0Q7QUFDeEQsbUVBQStEO0FBRS9ELGdEQUF3RDtBQUd4RCxnQkFBd0IsU0FBUSxvQ0FBZ0I7SUFBaEQ7O1FBQ1csMkJBQXNCLEdBQUcsSUFBSSxDQUFDO0lBMkZ6QyxDQUFDO0lBekZlLHNCQUFzQixDQUFDLGNBQXNCOztZQUN6RCxNQUFNLGdCQUFnQixHQUFHLE1BQU0sSUFBSSxDQUFDLFVBQVUsQ0FBQztnQkFDN0MsYUFBYSxFQUFFLFFBQVE7Z0JBQ3ZCLGNBQWM7YUFDZixDQUFDLENBQUM7WUFDSCxJQUFJLENBQUMsVUFBVSxDQUFDLGdCQUFnQixDQUFDLENBQUM7WUFFbEMsT0FBTyw2QkFBWSxDQUFDLElBQUksQ0FBQyxRQUFRLEVBQUUsSUFBSSxDQUFDLE9BQU8sQ0FBQyxDQUFDO1FBQ25ELENBQUM7S0FBQTtJQUVELFFBQVEsQ0FBQyxPQUFZO1FBQ25CLE1BQU0sY0FBYyxHQUFHLE9BQU8sQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUM7UUFFcEMsSUFBSSxDQUFDLGNBQWMsRUFBRTtZQUNuQixJQUFJLENBQUMsTUFBTSxDQUFDLEtBQUssQ0FDZixvRUFBb0U7a0JBQ2xFLEdBQUcsZUFBUSxDQUFDLE1BQU0sQ0FBQyxnQkFBZ0IsQ0FBQyxvQ0FBb0MsQ0FDM0UsQ0FBQztZQUVGLE9BQU8sS0FBSyxDQUFDO1NBQ2Q7UUFFRCxPQUFPLElBQUksQ0FBQztJQUNkLENBQUM7SUFFSyxHQUFHLENBQUMsT0FBWTs7WUFDcEIsTUFBTSxRQUFRLEdBQUcsT0FBTyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQztZQUU5QixJQUFJLENBQUMsUUFBUSxFQUFFO2dCQUNiLElBQUksQ0FBQyxNQUFNLENBQUMsS0FBSyxDQUNmLG9FQUFvRTtzQkFDbEUsR0FBRyxlQUFRLENBQUMsTUFBTSxDQUFDLGdCQUFnQixDQUFDLG9DQUFvQyxDQUMzRSxDQUFDO2dCQUVGLE9BQU8sQ0FBQyxDQUFDO2FBQ1Y7WUFFRCxNQUFNLGNBQWMsR0FBRywwQkFBaUIsRUFBRSxDQUFDO1lBRTNDLE1BQU0sVUFBVSxHQUFlLE9BQU8sQ0FBQyxzQkFBc0IsQ0FBQyxDQUFDLE9BQU8sQ0FBQztZQUV2RSxNQUFNLFdBQVcsR0FBRyxRQUFRLENBQUMsVUFBVSxDQUFDLEdBQUcsQ0FBQztnQkFDMUMsQ0FBQyxDQUFDLFFBQVEsQ0FBQyxLQUFLLENBQUMsR0FBRyxFQUFFLENBQUMsQ0FBQyxDQUFDLElBQUksQ0FBQyxHQUFHLENBQUM7Z0JBQ2xDLENBQUMsQ0FBQyxRQUFRLENBQUMsS0FBSyxDQUFDLEdBQUcsRUFBRSxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQztZQUU5QixnREFBZ0Q7WUFDaEQsTUFBTSxjQUFjLEdBQUcsQ0FDckIsV0FBVyxDQUFDLFVBQVUsQ0FBQyxHQUFHLENBQUM7Z0JBQ3pCLENBQUMsQ0FBQyxXQUFXLENBQUMsS0FBSyxDQUFDLEdBQUcsRUFBRSxDQUFDLENBQUMsQ0FBQyxJQUFJLENBQUMsR0FBRyxDQUFDO2dCQUNyQyxDQUFDLENBQUMsV0FBVyxDQUFDLEtBQUssQ0FBQyxHQUFHLEVBQUUsQ0FBQyxDQUFDLENBQUMsSUFBSSxDQUFDLEdBQUcsQ0FBQyxDQUN4QyxHQUFHLFFBQVEsQ0FBQyxLQUFLLENBQUMsV0FBVyxDQUFDLE1BQU0sQ0FBQyxDQUFDO1lBRXZDLDJGQUEyRjtZQUMzRixVQUFVO1lBQ1YsTUFBTSxVQUFVLENBQ2QsV0FBVyxFQUNYLElBQUksQ0FBQyxNQUFNLEVBQ1gsY0FBYyxFQUNkLElBQUksQ0FBQyxPQUFPLENBQUMsSUFBSSxDQUNsQixDQUFDO1lBRUYseURBQXlEO1lBQ3pELE9BQU8sR0FBRyxNQUFNLElBQUksQ0FBQyxzQkFBc0IsQ0FBQyxjQUFjLENBQUMsQ0FBQztZQUU1RCxNQUFNLFVBQVUsR0FBRztnQkFDakIsZ0JBQWdCLEVBQUUsT0FBTztnQkFDekIsVUFBVSxFQUFFLElBQUksQ0FBQyxPQUFPLENBQUMsSUFBSTtnQkFDN0IsY0FBYztnQkFDZCxhQUFhLEVBQUUsUUFBUTtnQkFDdkIsWUFBWSxFQUFFLElBQUk7Z0JBQ2xCLE1BQU0sRUFBRSxLQUFLO2dCQUNiLEtBQUssRUFBRSxLQUFLO2FBQ2IsQ0FBQztZQUVGLElBQUk7Z0JBQ0YsT0FBTyxNQUFNLElBQUksQ0FBQyxZQUFZLENBQUMsVUFBVSxDQUFDLENBQUM7YUFDNUM7WUFBQyxPQUFPLENBQUMsRUFBRTtnQkFDVixJQUFJLENBQUMsWUFBWSwyQ0FBbUMsRUFBRTtvQkFDcEQsSUFBSSxDQUFDLE1BQU0sQ0FBQyxLQUFLLENBQUMsV0FBSSxDQUFDLE9BQU8sQ0FBQTs7O1NBRzdCLENBQUMsQ0FBQztvQkFFSCxPQUFPLENBQUMsQ0FBQztpQkFDVjtnQkFFRCxNQUFNLENBQUMsQ0FBQzthQUNUO1FBQ0gsQ0FBQztLQUFBO0NBQ0Y7QUE1RkQsZ0NBNEZDIiwic291cmNlc0NvbnRlbnQiOlsiLyoqXG4gKiBAbGljZW5zZVxuICogQ29weXJpZ2h0IEdvb2dsZSBJbmMuIEFsbCBSaWdodHMgUmVzZXJ2ZWQuXG4gKlxuICogVXNlIG9mIHRoaXMgc291cmNlIGNvZGUgaXMgZ292ZXJuZWQgYnkgYW4gTUlULXN0eWxlIGxpY2Vuc2UgdGhhdCBjYW4gYmVcbiAqIGZvdW5kIGluIHRoZSBMSUNFTlNFIGZpbGUgYXQgaHR0cHM6Ly9hbmd1bGFyLmlvL2xpY2Vuc2VcbiAqL1xuXG4vLyB0c2xpbnQ6ZGlzYWJsZTpuby1nbG9iYWwtdHNsaW50LWRpc2FibGUgbm8tYW55XG5pbXBvcnQgeyB0YWdzLCB0ZXJtaW5hbCB9IGZyb20gJ0Bhbmd1bGFyLWRldmtpdC9jb3JlJztcbmltcG9ydCB7IE5vZGVQYWNrYWdlRG9lc05vdFN1cHBvcnRTY2hlbWF0aWNzIH0gZnJvbSAnQGFuZ3VsYXItZGV2a2l0L3NjaGVtYXRpY3MvdG9vbHMnO1xuaW1wb3J0IHsgcGFyc2VPcHRpb25zIH0gZnJvbSAnLi4vbW9kZWxzL2NvbW1hbmQtcnVubmVyJztcbmltcG9ydCB7IFNjaGVtYXRpY0NvbW1hbmQgfSBmcm9tICcuLi9tb2RlbHMvc2NoZW1hdGljLWNvbW1hbmQnO1xuaW1wb3J0IHsgTnBtSW5zdGFsbCB9IGZyb20gJy4uL3Rhc2tzL25wbS1pbnN0YWxsJztcbmltcG9ydCB7IGdldFBhY2thZ2VNYW5hZ2VyIH0gZnJvbSAnLi4vdXRpbGl0aWVzL2NvbmZpZyc7XG5cblxuZXhwb3J0IGNsYXNzIEFkZENvbW1hbmQgZXh0ZW5kcyBTY2hlbWF0aWNDb21tYW5kIHtcbiAgcmVhZG9ubHkgYWxsb3dQcml2YXRlU2NoZW1hdGljcyA9IHRydWU7XG5cbiAgcHJpdmF0ZSBhc3luYyBfcGFyc2VTY2hlbWF0aWNPcHRpb25zKGNvbGxlY3Rpb25OYW1lOiBzdHJpbmcpOiBQcm9taXNlPGFueT4ge1xuICAgIGNvbnN0IHNjaGVtYXRpY09wdGlvbnMgPSBhd2FpdCB0aGlzLmdldE9wdGlvbnMoe1xuICAgICAgc2NoZW1hdGljTmFtZTogJ25nLWFkZCcsXG4gICAgICBjb2xsZWN0aW9uTmFtZSxcbiAgICB9KTtcbiAgICB0aGlzLmFkZE9wdGlvbnMoc2NoZW1hdGljT3B0aW9ucyk7XG5cbiAgICByZXR1cm4gcGFyc2VPcHRpb25zKHRoaXMuX3Jhd0FyZ3MsIHRoaXMub3B0aW9ucyk7XG4gIH1cblxuICB2YWxpZGF0ZShvcHRpb25zOiBhbnkpIHtcbiAgICBjb25zdCBjb2xsZWN0aW9uTmFtZSA9IG9wdGlvbnMuX1swXTtcblxuICAgIGlmICghY29sbGVjdGlvbk5hbWUpIHtcbiAgICAgIHRoaXMubG9nZ2VyLmZhdGFsKFxuICAgICAgICBgVGhlIFwibmcgYWRkXCIgY29tbWFuZCByZXF1aXJlcyBhIG5hbWUgYXJndW1lbnQgdG8gYmUgc3BlY2lmaWVkIGVnLiBgXG4gICAgICAgICsgYCR7dGVybWluYWwueWVsbG93KCduZyBhZGQgW25hbWVdICcpfS4gRm9yIG1vcmUgZGV0YWlscywgdXNlIFwibmcgaGVscFwiLmAsXG4gICAgICApO1xuXG4gICAgICByZXR1cm4gZmFsc2U7XG4gICAgfVxuXG4gICAgcmV0dXJuIHRydWU7XG4gIH1cblxuICBhc3luYyBydW4ob3B0aW9uczogYW55KSB7XG4gICAgY29uc3QgZmlyc3RBcmcgPSBvcHRpb25zLl9bMF07XG5cbiAgICBpZiAoIWZpcnN0QXJnKSB7XG4gICAgICB0aGlzLmxvZ2dlci5mYXRhbChcbiAgICAgICAgYFRoZSBcIm5nIGFkZFwiIGNvbW1hbmQgcmVxdWlyZXMgYSBuYW1lIGFyZ3VtZW50IHRvIGJlIHNwZWNpZmllZCBlZy4gYFxuICAgICAgICArIGAke3Rlcm1pbmFsLnllbGxvdygnbmcgYWRkIFtuYW1lXSAnKX0uIEZvciBtb3JlIGRldGFpbHMsIHVzZSBcIm5nIGhlbHBcIi5gLFxuICAgICAgKTtcblxuICAgICAgcmV0dXJuIDE7XG4gICAgfVxuXG4gICAgY29uc3QgcGFja2FnZU1hbmFnZXIgPSBnZXRQYWNrYWdlTWFuYWdlcigpO1xuXG4gICAgY29uc3QgbnBtSW5zdGFsbDogTnBtSW5zdGFsbCA9IHJlcXVpcmUoJy4uL3Rhc2tzL25wbS1pbnN0YWxsJykuZGVmYXVsdDtcblxuICAgIGNvbnN0IHBhY2thZ2VOYW1lID0gZmlyc3RBcmcuc3RhcnRzV2l0aCgnQCcpXG4gICAgICA/IGZpcnN0QXJnLnNwbGl0KCcvJywgMikuam9pbignLycpXG4gICAgICA6IGZpcnN0QXJnLnNwbGl0KCcvJywgMSlbMF07XG5cbiAgICAvLyBSZW1vdmUgdGhlIHRhZy92ZXJzaW9uIGZyb20gdGhlIHBhY2thZ2UgbmFtZS5cbiAgICBjb25zdCBjb2xsZWN0aW9uTmFtZSA9IChcbiAgICAgIHBhY2thZ2VOYW1lLnN0YXJ0c1dpdGgoJ0AnKVxuICAgICAgICA/IHBhY2thZ2VOYW1lLnNwbGl0KCdAJywgMikuam9pbignQCcpXG4gICAgICAgIDogcGFja2FnZU5hbWUuc3BsaXQoJ0AnLCAxKS5qb2luKCdAJylcbiAgICApICsgZmlyc3RBcmcuc2xpY2UocGFja2FnZU5hbWUubGVuZ3RoKTtcblxuICAgIC8vIFdlIGRvbid0IGFjdHVhbGx5IGFkZCB0aGUgcGFja2FnZSB0byBwYWNrYWdlLmpzb24sIHRoYXQgd291bGQgYmUgdGhlIHdvcmsgb2YgdGhlIHBhY2thZ2VcbiAgICAvLyBpdHNlbGYuXG4gICAgYXdhaXQgbnBtSW5zdGFsbChcbiAgICAgIHBhY2thZ2VOYW1lLFxuICAgICAgdGhpcy5sb2dnZXIsXG4gICAgICBwYWNrYWdlTWFuYWdlcixcbiAgICAgIHRoaXMucHJvamVjdC5yb290LFxuICAgICk7XG5cbiAgICAvLyBSZXBhcnNlIHRoZSBvcHRpb25zIHdpdGggdGhlIG5ldyBzY2hlbWF0aWMgYWNjZXNzaWJsZS5cbiAgICBvcHRpb25zID0gYXdhaXQgdGhpcy5fcGFyc2VTY2hlbWF0aWNPcHRpb25zKGNvbGxlY3Rpb25OYW1lKTtcblxuICAgIGNvbnN0IHJ1bk9wdGlvbnMgPSB7XG4gICAgICBzY2hlbWF0aWNPcHRpb25zOiBvcHRpb25zLFxuICAgICAgd29ya2luZ0RpcjogdGhpcy5wcm9qZWN0LnJvb3QsXG4gICAgICBjb2xsZWN0aW9uTmFtZSxcbiAgICAgIHNjaGVtYXRpY05hbWU6ICduZy1hZGQnLFxuICAgICAgYWxsb3dQcml2YXRlOiB0cnVlLFxuICAgICAgZHJ5UnVuOiBmYWxzZSxcbiAgICAgIGZvcmNlOiBmYWxzZSxcbiAgICB9O1xuXG4gICAgdHJ5IHtcbiAgICAgIHJldHVybiBhd2FpdCB0aGlzLnJ1blNjaGVtYXRpYyhydW5PcHRpb25zKTtcbiAgICB9IGNhdGNoIChlKSB7XG4gICAgICBpZiAoZSBpbnN0YW5jZW9mIE5vZGVQYWNrYWdlRG9lc05vdFN1cHBvcnRTY2hlbWF0aWNzKSB7XG4gICAgICAgIHRoaXMubG9nZ2VyLmVycm9yKHRhZ3Mub25lTGluZWBcbiAgICAgICAgICBUaGUgcGFja2FnZSB0aGF0IHlvdSBhcmUgdHJ5aW5nIHRvIGFkZCBkb2VzIG5vdCBzdXBwb3J0IHNjaGVtYXRpY3MuIFlvdSBjYW4gdHJ5IHVzaW5nXG4gICAgICAgICAgYSBkaWZmZXJlbnQgdmVyc2lvbiBvZiB0aGUgcGFja2FnZSBvciBjb250YWN0IHRoZSBwYWNrYWdlIGF1dGhvciB0byBhZGQgbmctYWRkIHN1cHBvcnQuXG4gICAgICAgIGApO1xuXG4gICAgICAgIHJldHVybiAxO1xuICAgICAgfVxuXG4gICAgICB0aHJvdyBlO1xuICAgIH1cbiAgfVxufVxuIl19