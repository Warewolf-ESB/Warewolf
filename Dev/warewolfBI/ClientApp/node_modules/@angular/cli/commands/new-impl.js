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
const schematic_command_1 = require("../models/schematic-command");
const config_1 = require("../utilities/config");
class NewCommand extends schematic_command_1.SchematicCommand {
    constructor() {
        super(...arguments);
        this.allowMissingWorkspace = true;
        this.schematicName = 'ng-new';
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
            const collectionName = this.parseCollectionName(options);
            const schematicOptions = yield this.getOptions({
                schematicName: this.schematicName,
                collectionName,
            });
            this.addOptions(this.options.concat(schematicOptions));
        });
    }
    run(options) {
        return __awaiter(this, void 0, void 0, function* () {
            if (options.dryRun) {
                options.skipGit = true;
            }
            let collectionName;
            if (options.collection) {
                collectionName = options.collection;
            }
            else {
                collectionName = this.parseCollectionName(options);
            }
            const packageJson = require('../package.json');
            options.version = packageJson.version;
            // Ensure skipGit has a boolean value.
            options.skipGit = options.skipGit === undefined ? false : options.skipGit;
            options = this.removeLocalOptions(options);
            return this.runSchematic({
                collectionName: collectionName,
                schematicName: this.schematicName,
                schematicOptions: options,
                debug: options.debug,
                dryRun: options.dryRun,
                force: options.force,
            });
        });
    }
    parseCollectionName(options) {
        const collectionName = options.collection || options.c || config_1.getDefaultSchematicCollection();
        return collectionName;
    }
    removeLocalOptions(options) {
        const opts = Object.assign({}, options);
        delete opts.verbose;
        delete opts.collection;
        return opts;
    }
}
exports.NewCommand = NewCommand;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoibmV3LWltcGwuanMiLCJzb3VyY2VSb290IjoiLi8iLCJzb3VyY2VzIjpbInBhY2thZ2VzL2FuZ3VsYXIvY2xpL2NvbW1hbmRzL25ldy1pbXBsLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7QUFBQTs7Ozs7O0dBTUc7Ozs7Ozs7Ozs7QUFFSCxpREFBaUQ7QUFDakQsbUVBQStEO0FBQy9ELGdEQUFvRTtBQUdwRSxnQkFBd0IsU0FBUSxvQ0FBZ0I7SUFBaEQ7O1FBQ2tCLDBCQUFxQixHQUFHLElBQUksQ0FBQztRQUNyQyxrQkFBYSxHQUFHLFFBQVEsQ0FBQztRQUV6QixnQkFBVyxHQUFHLEtBQUssQ0FBQztJQThEOUIsQ0FBQztJQTdEYyxVQUFVLENBQUMsT0FBWTs7O1lBQ2xDLElBQUksSUFBSSxDQUFDLFdBQVcsRUFBRTtnQkFDcEIsT0FBTzthQUNSO1lBRUQsTUFBTSxvQkFBZ0IsWUFBQyxPQUFPLENBQUMsQ0FBQztZQUVoQyxJQUFJLENBQUMsV0FBVyxHQUFHLElBQUksQ0FBQztZQUV4QixNQUFNLGNBQWMsR0FBRyxJQUFJLENBQUMsbUJBQW1CLENBQUMsT0FBTyxDQUFDLENBQUM7WUFFekQsTUFBTSxnQkFBZ0IsR0FBRyxNQUFNLElBQUksQ0FBQyxVQUFVLENBQUM7Z0JBQzdDLGFBQWEsRUFBRSxJQUFJLENBQUMsYUFBYTtnQkFDakMsY0FBYzthQUNmLENBQUMsQ0FBQztZQUNILElBQUksQ0FBQyxVQUFVLENBQUMsSUFBSSxDQUFDLE9BQU8sQ0FBQyxNQUFNLENBQUMsZ0JBQWdCLENBQUMsQ0FBQyxDQUFDO1FBQ3pELENBQUM7S0FBQTtJQUVZLEdBQUcsQ0FBQyxPQUFZOztZQUMzQixJQUFJLE9BQU8sQ0FBQyxNQUFNLEVBQUU7Z0JBQ2xCLE9BQU8sQ0FBQyxPQUFPLEdBQUcsSUFBSSxDQUFDO2FBQ3hCO1lBRUQsSUFBSSxjQUFzQixDQUFDO1lBQzNCLElBQUksT0FBTyxDQUFDLFVBQVUsRUFBRTtnQkFDdEIsY0FBYyxHQUFHLE9BQU8sQ0FBQyxVQUFVLENBQUM7YUFDckM7aUJBQU07Z0JBQ0wsY0FBYyxHQUFHLElBQUksQ0FBQyxtQkFBbUIsQ0FBQyxPQUFPLENBQUMsQ0FBQzthQUNwRDtZQUVELE1BQU0sV0FBVyxHQUFHLE9BQU8sQ0FBQyxpQkFBaUIsQ0FBQyxDQUFDO1lBQy9DLE9BQU8sQ0FBQyxPQUFPLEdBQUcsV0FBVyxDQUFDLE9BQU8sQ0FBQztZQUV0QyxzQ0FBc0M7WUFDdEMsT0FBTyxDQUFDLE9BQU8sR0FBRyxPQUFPLENBQUMsT0FBTyxLQUFLLFNBQVMsQ0FBQyxDQUFDLENBQUMsS0FBSyxDQUFDLENBQUMsQ0FBQyxPQUFPLENBQUMsT0FBTyxDQUFDO1lBRTFFLE9BQU8sR0FBRyxJQUFJLENBQUMsa0JBQWtCLENBQUMsT0FBTyxDQUFDLENBQUM7WUFFM0MsT0FBTyxJQUFJLENBQUMsWUFBWSxDQUFDO2dCQUN2QixjQUFjLEVBQUUsY0FBYztnQkFDOUIsYUFBYSxFQUFFLElBQUksQ0FBQyxhQUFhO2dCQUNqQyxnQkFBZ0IsRUFBRSxPQUFPO2dCQUN6QixLQUFLLEVBQUUsT0FBTyxDQUFDLEtBQUs7Z0JBQ3BCLE1BQU0sRUFBRSxPQUFPLENBQUMsTUFBTTtnQkFDdEIsS0FBSyxFQUFFLE9BQU8sQ0FBQyxLQUFLO2FBQ3JCLENBQUMsQ0FBQztRQUNMLENBQUM7S0FBQTtJQUVPLG1CQUFtQixDQUFDLE9BQVk7UUFDdEMsTUFBTSxjQUFjLEdBQUcsT0FBTyxDQUFDLFVBQVUsSUFBSSxPQUFPLENBQUMsQ0FBQyxJQUFJLHNDQUE2QixFQUFFLENBQUM7UUFFMUYsT0FBTyxjQUFjLENBQUM7SUFDeEIsQ0FBQztJQUVPLGtCQUFrQixDQUFDLE9BQVk7UUFDckMsTUFBTSxJQUFJLEdBQUcsTUFBTSxDQUFDLE1BQU0sQ0FBQyxFQUFFLEVBQUUsT0FBTyxDQUFDLENBQUM7UUFDeEMsT0FBTyxJQUFJLENBQUMsT0FBTyxDQUFDO1FBQ3BCLE9BQU8sSUFBSSxDQUFDLFVBQVUsQ0FBQztRQUV2QixPQUFPLElBQUksQ0FBQztJQUNkLENBQUM7Q0FDRjtBQWxFRCxnQ0FrRUMiLCJzb3VyY2VzQ29udGVudCI6WyIvKipcbiAqIEBsaWNlbnNlXG4gKiBDb3B5cmlnaHQgR29vZ2xlIEluYy4gQWxsIFJpZ2h0cyBSZXNlcnZlZC5cbiAqXG4gKiBVc2Ugb2YgdGhpcyBzb3VyY2UgY29kZSBpcyBnb3Zlcm5lZCBieSBhbiBNSVQtc3R5bGUgbGljZW5zZSB0aGF0IGNhbiBiZVxuICogZm91bmQgaW4gdGhlIExJQ0VOU0UgZmlsZSBhdCBodHRwczovL2FuZ3VsYXIuaW8vbGljZW5zZVxuICovXG5cbi8vIHRzbGludDpkaXNhYmxlOm5vLWdsb2JhbC10c2xpbnQtZGlzYWJsZSBuby1hbnlcbmltcG9ydCB7IFNjaGVtYXRpY0NvbW1hbmQgfSBmcm9tICcuLi9tb2RlbHMvc2NoZW1hdGljLWNvbW1hbmQnO1xuaW1wb3J0IHsgZ2V0RGVmYXVsdFNjaGVtYXRpY0NvbGxlY3Rpb24gfSBmcm9tICcuLi91dGlsaXRpZXMvY29uZmlnJztcblxuXG5leHBvcnQgY2xhc3MgTmV3Q29tbWFuZCBleHRlbmRzIFNjaGVtYXRpY0NvbW1hbmQge1xuICBwdWJsaWMgcmVhZG9ubHkgYWxsb3dNaXNzaW5nV29ya3NwYWNlID0gdHJ1ZTtcbiAgcHJpdmF0ZSBzY2hlbWF0aWNOYW1lID0gJ25nLW5ldyc7XG5cbiAgcHJpdmF0ZSBpbml0aWFsaXplZCA9IGZhbHNlO1xuICBwdWJsaWMgYXN5bmMgaW5pdGlhbGl6ZShvcHRpb25zOiBhbnkpIHtcbiAgICBpZiAodGhpcy5pbml0aWFsaXplZCkge1xuICAgICAgcmV0dXJuO1xuICAgIH1cblxuICAgIGF3YWl0IHN1cGVyLmluaXRpYWxpemUob3B0aW9ucyk7XG5cbiAgICB0aGlzLmluaXRpYWxpemVkID0gdHJ1ZTtcblxuICAgIGNvbnN0IGNvbGxlY3Rpb25OYW1lID0gdGhpcy5wYXJzZUNvbGxlY3Rpb25OYW1lKG9wdGlvbnMpO1xuXG4gICAgY29uc3Qgc2NoZW1hdGljT3B0aW9ucyA9IGF3YWl0IHRoaXMuZ2V0T3B0aW9ucyh7XG4gICAgICBzY2hlbWF0aWNOYW1lOiB0aGlzLnNjaGVtYXRpY05hbWUsXG4gICAgICBjb2xsZWN0aW9uTmFtZSxcbiAgICB9KTtcbiAgICB0aGlzLmFkZE9wdGlvbnModGhpcy5vcHRpb25zLmNvbmNhdChzY2hlbWF0aWNPcHRpb25zKSk7XG4gIH1cblxuICBwdWJsaWMgYXN5bmMgcnVuKG9wdGlvbnM6IGFueSkge1xuICAgIGlmIChvcHRpb25zLmRyeVJ1bikge1xuICAgICAgb3B0aW9ucy5za2lwR2l0ID0gdHJ1ZTtcbiAgICB9XG5cbiAgICBsZXQgY29sbGVjdGlvbk5hbWU6IHN0cmluZztcbiAgICBpZiAob3B0aW9ucy5jb2xsZWN0aW9uKSB7XG4gICAgICBjb2xsZWN0aW9uTmFtZSA9IG9wdGlvbnMuY29sbGVjdGlvbjtcbiAgICB9IGVsc2Uge1xuICAgICAgY29sbGVjdGlvbk5hbWUgPSB0aGlzLnBhcnNlQ29sbGVjdGlvbk5hbWUob3B0aW9ucyk7XG4gICAgfVxuXG4gICAgY29uc3QgcGFja2FnZUpzb24gPSByZXF1aXJlKCcuLi9wYWNrYWdlLmpzb24nKTtcbiAgICBvcHRpb25zLnZlcnNpb24gPSBwYWNrYWdlSnNvbi52ZXJzaW9uO1xuXG4gICAgLy8gRW5zdXJlIHNraXBHaXQgaGFzIGEgYm9vbGVhbiB2YWx1ZS5cbiAgICBvcHRpb25zLnNraXBHaXQgPSBvcHRpb25zLnNraXBHaXQgPT09IHVuZGVmaW5lZCA/IGZhbHNlIDogb3B0aW9ucy5za2lwR2l0O1xuXG4gICAgb3B0aW9ucyA9IHRoaXMucmVtb3ZlTG9jYWxPcHRpb25zKG9wdGlvbnMpO1xuXG4gICAgcmV0dXJuIHRoaXMucnVuU2NoZW1hdGljKHtcbiAgICAgIGNvbGxlY3Rpb25OYW1lOiBjb2xsZWN0aW9uTmFtZSxcbiAgICAgIHNjaGVtYXRpY05hbWU6IHRoaXMuc2NoZW1hdGljTmFtZSxcbiAgICAgIHNjaGVtYXRpY09wdGlvbnM6IG9wdGlvbnMsXG4gICAgICBkZWJ1Zzogb3B0aW9ucy5kZWJ1ZyxcbiAgICAgIGRyeVJ1bjogb3B0aW9ucy5kcnlSdW4sXG4gICAgICBmb3JjZTogb3B0aW9ucy5mb3JjZSxcbiAgICB9KTtcbiAgfVxuXG4gIHByaXZhdGUgcGFyc2VDb2xsZWN0aW9uTmFtZShvcHRpb25zOiBhbnkpOiBzdHJpbmcge1xuICAgIGNvbnN0IGNvbGxlY3Rpb25OYW1lID0gb3B0aW9ucy5jb2xsZWN0aW9uIHx8IG9wdGlvbnMuYyB8fCBnZXREZWZhdWx0U2NoZW1hdGljQ29sbGVjdGlvbigpO1xuXG4gICAgcmV0dXJuIGNvbGxlY3Rpb25OYW1lO1xuICB9XG5cbiAgcHJpdmF0ZSByZW1vdmVMb2NhbE9wdGlvbnMob3B0aW9uczogYW55KTogYW55IHtcbiAgICBjb25zdCBvcHRzID0gT2JqZWN0LmFzc2lnbih7fSwgb3B0aW9ucyk7XG4gICAgZGVsZXRlIG9wdHMudmVyYm9zZTtcbiAgICBkZWxldGUgb3B0cy5jb2xsZWN0aW9uO1xuXG4gICAgcmV0dXJuIG9wdHM7XG4gIH1cbn1cbiJdfQ==