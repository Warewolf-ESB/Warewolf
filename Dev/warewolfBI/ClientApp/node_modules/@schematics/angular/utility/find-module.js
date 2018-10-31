"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
const core_1 = require("@angular-devkit/core");
/**
 * Find the module referred by a set of options passed to the schematics.
 */
function findModuleFromOptions(host, options) {
    if (options.hasOwnProperty('skipImport') && options.skipImport) {
        return undefined;
    }
    if (!options.module) {
        const pathToCheck = (options.path || '')
            + (options.flat ? '' : '/' + core_1.strings.dasherize(options.name));
        return core_1.normalize(findModule(host, pathToCheck));
    }
    else {
        const modulePath = core_1.normalize('/' + (options.path) + '/' + options.module);
        const moduleBaseName = core_1.normalize(modulePath).split('/').pop();
        if (host.exists(modulePath)) {
            return core_1.normalize(modulePath);
        }
        else if (host.exists(modulePath + '.ts')) {
            return core_1.normalize(modulePath + '.ts');
        }
        else if (host.exists(modulePath + '.module.ts')) {
            return core_1.normalize(modulePath + '.module.ts');
        }
        else if (host.exists(modulePath + '/' + moduleBaseName + '.module.ts')) {
            return core_1.normalize(modulePath + '/' + moduleBaseName + '.module.ts');
        }
        else {
            throw new Error('Specified module does not exist');
        }
    }
}
exports.findModuleFromOptions = findModuleFromOptions;
/**
 * Function to find the "closest" module to a generated file's path.
 */
function findModule(host, generateDir) {
    let dir = host.getDir('/' + generateDir);
    const moduleRe = /\.module\.ts$/;
    const routingModuleRe = /-routing\.module\.ts/;
    let foundRoutingModule = false;
    while (dir) {
        const allMatches = dir.subfiles.filter(p => moduleRe.test(p));
        const filteredMatches = allMatches.filter(p => !routingModuleRe.test(p));
        foundRoutingModule = foundRoutingModule || allMatches.length !== filteredMatches.length;
        if (filteredMatches.length == 1) {
            return core_1.join(dir.path, filteredMatches[0]);
        }
        else if (filteredMatches.length > 1) {
            throw new Error('More than one module matches. Use skip-import option to skip importing '
                + 'the component into the closest module.');
        }
        dir = dir.parent;
    }
    const errorMsg = foundRoutingModule ? 'Could not find a non Routing NgModule.'
        + '\nModules with suffix \'-routing.module\' are strictly reserved for routing.'
        + '\nUse the skip-import option to skip importing in NgModule.'
        : 'Could not find an NgModule. Use the skip-import option to skip importing in NgModule.';
    throw new Error(errorMsg);
}
exports.findModule = findModule;
/**
 * Build a relative path from one file path to another file path.
 */
function buildRelativePath(from, to) {
    from = core_1.normalize(from);
    to = core_1.normalize(to);
    // Convert to arrays.
    const fromParts = from.split('/');
    const toParts = to.split('/');
    // Remove file names (preserving destination)
    fromParts.pop();
    const toFileName = toParts.pop();
    const relativePath = core_1.relative(core_1.normalize(fromParts.join('/')), core_1.normalize(toParts.join('/')));
    let pathPrefix = '';
    // Set the path prefix for same dir or child dir, parent dir starts with `..`
    if (!relativePath) {
        pathPrefix = '.';
    }
    else if (!relativePath.startsWith('.')) {
        pathPrefix = `./`;
    }
    if (pathPrefix && !pathPrefix.endsWith('/')) {
        pathPrefix += '/';
    }
    return pathPrefix + (relativePath ? relativePath + '/' : '') + toFileName;
}
exports.buildRelativePath = buildRelativePath;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiZmluZC1tb2R1bGUuanMiLCJzb3VyY2VSb290IjoiLi8iLCJzb3VyY2VzIjpbInBhY2thZ2VzL3NjaGVtYXRpY3MvYW5ndWxhci91dGlsaXR5L2ZpbmQtbW9kdWxlLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7O0FBQUE7Ozs7OztHQU1HO0FBQ0gsK0NBQWdGO0FBYWhGOztHQUVHO0FBQ0gsK0JBQXNDLElBQVUsRUFBRSxPQUFzQjtJQUN0RSxJQUFJLE9BQU8sQ0FBQyxjQUFjLENBQUMsWUFBWSxDQUFDLElBQUksT0FBTyxDQUFDLFVBQVUsRUFBRTtRQUM5RCxPQUFPLFNBQVMsQ0FBQztLQUNsQjtJQUVELElBQUksQ0FBQyxPQUFPLENBQUMsTUFBTSxFQUFFO1FBQ25CLE1BQU0sV0FBVyxHQUFHLENBQUMsT0FBTyxDQUFDLElBQUksSUFBSSxFQUFFLENBQUM7Y0FDcEMsQ0FBQyxPQUFPLENBQUMsSUFBSSxDQUFDLENBQUMsQ0FBQyxFQUFFLENBQUMsQ0FBQyxDQUFDLEdBQUcsR0FBRyxjQUFPLENBQUMsU0FBUyxDQUFDLE9BQU8sQ0FBQyxJQUFJLENBQUMsQ0FBQyxDQUFDO1FBRWhFLE9BQU8sZ0JBQVMsQ0FBQyxVQUFVLENBQUMsSUFBSSxFQUFFLFdBQVcsQ0FBQyxDQUFDLENBQUM7S0FDakQ7U0FBTTtRQUNMLE1BQU0sVUFBVSxHQUFHLGdCQUFTLENBQzFCLEdBQUcsR0FBRyxDQUFDLE9BQU8sQ0FBQyxJQUFJLENBQUMsR0FBRyxHQUFHLEdBQUcsT0FBTyxDQUFDLE1BQU0sQ0FBQyxDQUFDO1FBQy9DLE1BQU0sY0FBYyxHQUFHLGdCQUFTLENBQUMsVUFBVSxDQUFDLENBQUMsS0FBSyxDQUFDLEdBQUcsQ0FBQyxDQUFDLEdBQUcsRUFBRSxDQUFDO1FBRTlELElBQUksSUFBSSxDQUFDLE1BQU0sQ0FBQyxVQUFVLENBQUMsRUFBRTtZQUMzQixPQUFPLGdCQUFTLENBQUMsVUFBVSxDQUFDLENBQUM7U0FDOUI7YUFBTSxJQUFJLElBQUksQ0FBQyxNQUFNLENB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QUksQ0FBQyxHQUFHLENBQUMsQ0FBQyxDQUFDLENBQUM7SUFDNUYsSUFBSSxVQUFVLEdBQUcsRUFBRSxDQUFDO0lBRXBCLDZFQUE2RTtJQUM3RSxJQUFJLENBQUMsWUFBWSxFQUFFO1FBQ2pCLFVBQVUsR0FBRyxHQUFHLENBQUM7S0FDbEI7U0FBTSxJQUFJLENBQUMsWUFBWSxDQUFDLFVBQVUsQ0FBQyxHQUFHLENBQUMsRUFBRTtRQUN4QyxVQUFVLEdBQUcsSUFBSSxDQUFDO0tBQ25CO0lBQ0QsSUFBSSxVQUFVLElBQUksQ0FBQyxVQUFVLENBQUMsUUFBUSxDQUFDLEdBQUcsQ0FBQyxFQUFFO1FBQzNDLFVBQVUsSUFBSSxHQUFHLENBQUM7S0FDbkI7SUFFRCxPQUFPLFVBQVUsR0FBRyxDQUFDLFlBQVksQ0FBQyxDQUFDLENBQUMsWUFBWSxHQUFHLEdBQUcsQ0FBQyxDQUFDLENBQUMsRUFBRSxDQUFDLEdBQUcsVUFBVSxDQUFDO0FBQzVFLENBQUM7QUExQkQsOENBMEJDIiwic291cmNlc0NvbnRlbnQiOlsiLyoqXG4gKiBAbGljZW5zZVxuICogQ29weXJpZ2h0IEdvb2dsZSBJbmMuIEFsbCBSaWdodHMgUmVzZXJ2ZWQuXG4gKlxuICogVXNlIG9mIHRoaXMgc291cmNlIGNvZGUgaXMgZ292ZXJuZWQgYnkgYW4gTUlULXN0eWxlIGxpY2Vuc2UgdGhhdCBjYW4gYmVcbiAqIGZvdW5kIGluIHRoZSBMSUNFTlNFIGZpbGUgYXQgaHR0cHM6Ly9hbmd1bGFyLmlvL2xpY2Vuc2VcbiAqL1xuaW1wb3J0IHsgUGF0aCwgam9pbiwgbm9ybWFsaXplLCByZWxhdGl2ZSwgc3RyaW5ncyB9IGZyb20gJ0Bhbmd1bGFyLWRldmtpdC9jb3JlJztcbmltcG9ydCB7IERpckVudHJ5LCBUcmVlIH0gZnJvbSAnQGFuZ3VsYXItZGV2a2l0L3NjaGVtYXRpY3MnO1xuXG5cbmV4cG9ydCBpbnRlcmZhY2UgTW9kdWxlT3B0aW9ucyB7XG4gIG1vZHVsZT86IHN0cmluZztcbiAgbmFtZTogc3RyaW5nO1xuICBmbGF0PzogYm9vbGVhbjtcbiAgcGF0aD86IHN0cmluZztcbiAgc2tpcEltcG9ydD86IGJvb2xlYW47XG59XG5cblxuLyoqXG4gKiBGaW5kIHRoZSBtb2R1bGUgcmVmZXJyZWQgYnkgYSBzZXQgb2Ygb3B0aW9ucyBwYXNzZWQgdG8gdGhlIHNjaGVtYXRpY3MuXG4gKi9cbmV4cG9ydCBmdW5jdGlvbiBmaW5kTW9kdWxlRnJvbU9wdGlvbnMoaG9zdDogVHJlZSwgb3B0aW9uczogTW9kdWxlT3B0aW9ucyk6IFBhdGggfCB1bmRlZmluZWQge1xuICBpZiAob3B0aW9ucy5oYXNPd25Qcm9wZXJ0eSgnc2tpcEltcG9ydCcpICYmIG9wdGlvbnMuc2tpcEltcG9ydCkge1xuICAgIHJldHVybiB1bmRlZmluZWQ7XG4gIH1cblxuICBpZiAoIW9wdGlvbnMubW9kdWxlKSB7XG4gICAgY29uc3QgcGF0aFRvQ2hlY2sgPSAob3B0aW9ucy5wYXRoIHx8ICcnKVxuICAgICAgKyAob3B0aW9ucy5mbGF0ID8gJycgOiAnLycgKyBzdHJpbmdzLmRhc2hlcml6ZShvcHRpb25zLm5hbWUpKTtcblxuICAgIHJldHVybiBub3JtYWxpemUoZmluZE1vZHVsZShob3N0LCBwYXRoVG9DaGVjaykpO1xuICB9IGVsc2Uge1xuICAgIGNvbnN0IG1vZHVsZVBhdGggPSBub3JtYWxpemUoXG4gICAgICAnLycgKyAob3B0aW9ucy5wYXRoKSArICcvJyArIG9wdGlvbnMubW9kdWxlKTtcbiAgICBjb25zdCBtb2R1bGVCYXNlTmFtZSA9IG5vcm1hbGl6ZShtb2R1bGVQYXRoKS5zcGxpdCgnLycpLnBvcCgpO1xuXG4gICAgaWYgKGhvc3QuZXhpc3RzKG1vZHVsZVBhdGgpKSB7XG4gICAgICByZXR1cm4gbm9ybWFsaXplKG1vZHVsZVBhdGgpO1xuICAgIH0gZWxzZSBpZiAoaG9zdC5leGlzdHMobW9kdWxlUGF0aCArICcudHMnKSkge1xuICAgICAgcmV0dXJuIG5vcm1hbGl6ZShtb2R1bGVQYXRoICsgJy50cycpO1xuICAgIH0gZWxzZSBpZiAoaG9zdC5leGlzdHMobW9kdWxlUGF0aCArICcubW9kdWxlLnRzJykpIHtcbiAgICAgIHJldHVybiBub3JtYWxpemUobW9kdWxlUGF0aCArICcubW9kdWxlLnRzJyk7XG4gICAgfSBlbHNlIGlmIChob3N0LmV4aXN0cyhtb2R1bGVQYXRoICsgJy8nICsgbW9kdWxlQmFzZU5hbWUgKyAnLm1vZHVsZS50cycpKSB7XG4gICAgICByZXR1cm4gbm9ybWFsaXplKG1vZHVsZVBhdGggKyAnLycgKyBtb2R1bGVCYXNlTmFtZSArICcubW9kdWxlLnRzJyk7XG4gICAgfSBlbHNlIHtcbiAgICAgIHRocm93IG5ldyBFcnJvcignU3BlY2lmaWVkIG1vZHVsZSBkb2VzIG5vdCBleGlzdCcpO1xuICAgIH1cbiAgfVxufVxuXG4vKipcbiAqIEZ1bmN0aW9uIHRvIGZpbmQgdGhlIFwiY2xvc2VzdFwiIG1vZHVsZSB0byBhIGdlbmVyYXRlZCBmaWxlJ3MgcGF0aC5cbiAqL1xuZXhwb3J0IGZ1bmN0aW9uIGZpbmRNb2R1bGUoaG9zdDogVHJlZSwgZ2VuZXJhdGVEaXI6IHN0cmluZyk6IFBhdGgge1xuICBsZXQgZGlyOiBEaXJFbnRyeSB8IG51bGwgPSBob3N0LmdldERpcignLycgKyBnZW5lcmF0ZURpcik7XG5cbiAgY29uc3QgbW9kdWxlUmUgPSAvXFwubW9kdWxlXFwudHMkLztcbiAgY29uc3Qgcm91dGluZ01vZHVsZVJlID0gLy1yb3V0aW5nXFwubW9kdWxlXFwudHMvO1xuXG4gIGxldCBmb3VuZFJvdXRpbmdNb2R1bGUgPSBmYWxzZTtcblxuICB3aGlsZSAoZGlyKSB7XG4gICAgY29uc3QgYWxsTWF0Y2hlcyA9IGRpci5zdWJmaWxlcy5maWx0ZXIocCA9PiBtb2R1bGVSZS50ZXN0KHApKTtcbiAgICBjb25zdCBmaWx0ZXJlZE1hdGNoZXMgPSBhbGxNYXRjaGVzLmZpbHRlcihwID0+ICFyb3V0aW5nTW9kdWxlUmUudGVzdChwKSk7XG5cbiAgICBmb3VuZFJvdXRpbmdNb2R1bGUgPSBmb3VuZFJvdXRpbmdNb2R1bGUgfHwgYWxsTWF0Y2hlcy5sZW5ndGggIT09IGZpbHRlcmVkTWF0Y2hlcy5sZW5ndGg7XG5cbiAgICBpZiAoZmlsdGVyZWRNYXRjaGVzLmxlbmd0aCA9PSAxKSB7XG4gICAgICByZXR1cm4gam9pbihkaXIucGF0aCwgZmlsdGVyZWRNYXRjaGVzWzBdKTtcbiAgICB9IGVsc2UgaWYgKGZpbHRlcmVkTWF0Y2hlcy5sZW5ndGggPiAxKSB7XG4gICAgICB0aHJvdyBuZXcgRXJyb3IoJ01vcmUgdGhhbiBvbmUgbW9kdWxlIG1hdGNoZXMuIFVzZSBza2lwLWltcG9ydCBvcHRpb24gdG8gc2tpcCBpbXBvcnRpbmcgJ1xuICAgICAgICArICd0aGUgY29tcG9uZW50IGludG8gdGhlIGNsb3Nlc3QgbW9kdWxlLicpO1xuICAgIH1cblxuICAgIGRpciA9IGRpci5wYXJlbnQ7XG4gIH1cblxuICBjb25zdCBlcnJvck1zZyA9IGZvdW5kUm91dGluZ01vZHVsZSA/ICdDb3VsZCBub3QgZmluZCBhIG5vbiBSb3V0aW5nIE5nTW9kdWxlLidcbiAgICArICdcXG5Nb2R1bGVzIHdpdGggc3VmZml4IFxcJy1yb3V0aW5nLm1vZHVsZVxcJyBhcmUgc3RyaWN0bHkgcmVzZXJ2ZWQgZm9yIHJvdXRpbmcuJ1xuICAgICsgJ1xcblVzZSB0aGUgc2tpcC1pbXBvcnQgb3B0aW9uIHRvIHNraXAgaW1wb3J0aW5nIGluIE5nTW9kdWxlLidcbiAgICA6ICdDb3VsZCBub3QgZmluZCBhbiBOZ01vZHVsZS4gVXNlIHRoZSBza2lwLWltcG9ydCBvcHRpb24gdG8gc2tpcCBpbXBvcnRpbmcgaW4gTmdNb2R1bGUuJztcblxuICB0aHJvdyBuZXcgRXJyb3IoZXJyb3JNc2cpO1xufVxuXG4vKipcbiAqIEJ1aWxkIGEgcmVsYXRpdmUgcGF0aCBmcm9tIG9uZSBmaWxlIHBhdGggdG8gYW5vdGhlciBmaWxlIHBhdGguXG4gKi9cbmV4cG9ydCBmdW5jdGlvbiBidWlsZFJlbGF0aXZlUGF0aChmcm9tOiBzdHJpbmcsIHRvOiBzdHJpbmcpOiBzdHJpbmcge1xuICBmcm9tID0gbm9ybWFsaXplKGZyb20pO1xuICB0byA9IG5vcm1hbGl6ZSh0byk7XG5cbiAgLy8gQ29udmVydCB0byBhcnJheXMuXG4gIGNvbnN0IGZyb21QYXJ0cyA9IGZyb20uc3BsaXQoJy8nKTtcbiAgY29uc3QgdG9QYXJ0cyA9IHRvLnNwbGl0KCcvJyk7XG5cbiAgLy8gUmVtb3ZlIGZpbGUgbmFtZXMgKHByZXNlcnZpbmcgZGVzdGluYXRpb24pXG4gIGZyb21QYXJ0cy5wb3AoKTtcbiAgY29uc3QgdG9GaWxlTmFtZSA9IHRvUGFydHMucG9wKCk7XG5cbiAgY29uc3QgcmVsYXRpdmVQYXRoID0gcmVsYXRpdmUobm9ybWFsaXplKGZyb21QYXJ0cy5qb2luKCcvJykpLCBub3JtYWxpemUodG9QYXJ0cy5qb2luKCcvJykpKTtcbiAgbGV0IHBhdGhQcmVmaXggPSAnJztcblxuICAvLyBTZXQgdGhlIHBhdGggcHJlZml4IGZvciBzYW1lIGRpciBvciBjaGlsZCBkaXIsIHBhcmVudCBkaXIgc3RhcnRzIHdpdGggYC4uYFxuICBpZiAoIXJlbGF0aXZlUGF0aCkge1xuICAgIHBhdGhQcmVmaXggPSAnLic7XG4gIH0gZWxzZSBpZiAoIXJlbGF0aXZlUGF0aC5zdGFydHNXaXRoKCcuJykpIHtcbiAgICBwYXRoUHJlZml4ID0gYC4vYDtcbiAgfVxuICBpZiAocGF0aFByZWZpeCAmJiAhcGF0aFByZWZpeC5lbmRzV2l0aCgnLycpKSB7XG4gICAgcGF0aFByZWZpeCArPSAnLyc7XG4gIH1cblxuICByZXR1cm4gcGF0aFByZWZpeCArIChyZWxhdGl2ZVBhdGggPyByZWxhdGl2ZVBhdGggKyAnLycgOiAnJykgKyB0b0ZpbGVOYW1lO1xufVxuIl19