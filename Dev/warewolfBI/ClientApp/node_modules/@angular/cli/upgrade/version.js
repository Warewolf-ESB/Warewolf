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
const node_1 = require("@angular-devkit/core/node");
const path = require("path");
const semver_1 = require("semver");
const config_1 = require("../utilities/config");
class Version {
    constructor(_version = null) {
        this._version = _version;
        this._semver = null;
        this._semver = _version ? new semver_1.SemVer(_version) : null;
    }
    isAlpha() { return this.qualifier == 'alpha'; }
    isBeta() { return this.qualifier == 'beta'; }
    isReleaseCandidate() { return this.qualifier == 'rc'; }
    isKnown() { return this._version !== null; }
    isLocal() { return this.isKnown() && this._version && path.isAbsolute(this._version); }
    isGreaterThanOrEqualTo(other) {
        return this._semver !== null && this._semver.compare(other) >= 0;
    }
    get major() { return this._semver ? this._semver.major : 0; }
    get minor() { return this._semver ? this._semver.minor : 0; }
    get patch() { return this._semver ? this._semver.patch : 0; }
    get qualifier() { return this._semver ? this._semver.prerelease[0] : ''; }
    get extra() { return this._semver ? this._semver.prerelease[1] : ''; }
    toString() { return this._version; }
    static assertCompatibleAngularVersion(projectRoot) {
        let angularPkgJson;
        let rxjsPkgJson;
        const isInside = (base, potential) => {
            const absoluteBase = path.resolve(base);
            const absolutePotential = path.resolve(potential);
            const relativePotential = path.relative(absoluteBase, absolutePotential);
            if (!relativePotential.startsWith('..') && !path.isAbsolute(relativePotential)) {
                return true;
            }
            return false;
        };
        try {
            const resolveOptions = {
                basedir: projectRoot,
                checkGlobal: false,
                checkLocal: true,
            };
            const angularPackagePath = node_1.resolve('@angular/core/package.json', resolveOptions);
            const rxjsPackagePath = node_1.resolve('rxjs/package.json', resolveOptions);
            if (!isInside(projectRoot, angularPackagePath)
                || !isInside(projectRoot, rxjsPackagePath)) {
                throw new Error();
            }
            angularPkgJson = require(angularPackagePath);
            rxjsPkgJson = require(rxjsPackagePath);
        }
        catch (_a) {
            console.error(core_1.terminal.bold(core_1.terminal.red(core_1.tags.stripIndents `
        You seem to not be depending on "@angular/core" and/or "rxjs". This is an error.
      `)));
            process.exit(2);
        }
        if (!(angularPkgJson && angularPkgJson['version'] && rxjsPkgJson && rxjsPkgJson['version'])) {
            console.error(core_1.terminal.bold(core_1.terminal.red(core_1.tags.stripIndents `
        Cannot determine versions of "@angular/core" and/or "rxjs".
        This likely means your local installation is broken. Please reinstall your packages.
      `)));
            process.exit(2);
        }
        const angularVersion = new Version(angularPkgJson['version']);
        const rxjsVersion = new Version(rxjsPkgJson['version']);
        if (angularVersion.isLocal()) {
            console.error(core_1.terminal.yellow('Using a local version of angular. Proceeding with care...'));
            return;
        }
        if (!angularVersion.isGreaterThanOrEqualTo(new semver_1.SemVer('5.0.0'))) {
            console.error(core_1.terminal.bold(core_1.terminal.red(core_1.tags.stripIndents `
          This version of CLI is only compatible with Angular version 5.0.0 or higher.

          Please visit the link below to find instructions on how to update Angular.
          https://angular-update-guide.firebaseapp.com/
        ` + '\n')));
            process.exit(3);
        }
        else if (angularVersion.isGreaterThanOrEqualTo(new semver_1.SemVer('6.0.0-rc.0'))
            && !rxjsVersion.isGreaterThanOrEqualTo(new semver_1.SemVer('5.6.0-forward-compat.0'))
            && !rxjsVersion.isGreaterThanOrEqualTo(new semver_1.SemVer('6.0.0-beta.0'))) {
            console.error(core_1.terminal.bold(core_1.terminal.red(core_1.tags.stripIndents `
          This project uses version ${rxjsVersion} of RxJs, which is not supported by Angular v6.
          The official RxJs version that is supported is 5.6.0-forward-compat.0 and greater.

          Please visit the link below to find instructions on how to update RxJs.
          https://docs.google.com/document/d/12nlLt71VLKb-z3YaSGzUfx6mJbc34nsMXtByPUN35cg/edit#
        ` + '\n')));
            process.exit(3);
        }
        else if (angularVersion.isGreaterThanOrEqualTo(new semver_1.SemVer('6.0.0-rc.0'))
            && !rxjsVersion.isGreaterThanOrEqualTo(new semver_1.SemVer('6.0.0-beta.0'))) {
            console.warn(core_1.terminal.bold(core_1.terminal.red(core_1.tags.stripIndents `
          This project uses a temporary compatibility version of RxJs (${rxjsVersion}).

          Please visit the link below to find instructions on how to update RxJs.
          https://docs.google.com/document/d/12nlLt71VLKb-z3YaSGzUfx6mJbc34nsMXtByPUN35cg/edit#
        ` + '\n')));
        }
    }
    static assertTypescriptVersion(projectRoot) {
        if (!config_1.isWarningEnabled('typescriptMismatch')) {
            return;
        }
        let compilerVersion;
        let tsVersion;
        let compilerTypeScriptPeerVersion;
        try {
            const resolveOptions = {
                basedir: projectRoot,
                checkGlobal: false,
                checkLocal: true,
            };
            const compilerPackagePath = node_1.resolve('@angular/compiler-cli/package.json', resolveOptions);
            const typescriptProjectPath = node_1.resolve('typescript', resolveOptions);
            const compilerPackageInfo = require(compilerPackagePath);
            compilerVersion = compilerPackageInfo['version'];
            compilerTypeScriptPeerVersion = compilerPackageInfo['peerDependencies']['typescript'];
            tsVersion = require(typescriptProjectPath).version;
        }
        catch (_a) {
            console.error(core_1.terminal.bold(core_1.terminal.red(core_1.tags.stripIndents `
        Versions of @angular/compiler-cli and typescript could not be determined.
        The most common reason for this is a broken npm install.

        Please make sure your package.json contains both @angular/compiler-cli and typescript in
        devDependencies, then delete node_modules and package-lock.json (if you have one) and
        run npm install again.
      `)));
            process.exit(2);
            return;
        }
        // These versions do not have accurate typescript peer dependencies
        const versionCombos = [
            { compiler: '>=2.3.1 <3.0.0', typescript: '>=2.0.2 <2.3.0' },
            { compiler: '>=4.0.0-beta.0 <5.0.0', typescript: '>=2.1.0 <2.4.0' },
            { compiler: '5.0.0-beta.0 - 5.0.0-rc.2', typescript: '>=2.4.2 <2.5.0' },
        ];
        let currentCombo = versionCombos.find((combo) => semver_1.satisfies(compilerVersion, combo.compiler));
        if (!currentCombo && compilerTypeScriptPeerVersion) {
            currentCombo = { compiler: compilerVersion, typescript: compilerTypeScriptPeerVersion };
        }
        if (currentCombo && !semver_1.satisfies(tsVersion, currentCombo.typescript)) {
            // First line of warning looks weird being split in two, disable tslint for it.
            console.error((core_1.terminal.yellow('\n' + core_1.tags.stripIndent `
        @angular/compiler-cli@${compilerVersion} requires typescript@'${currentCombo.typescript}' but ${tsVersion} was found instead.
        Using this version can result in undefined behaviour and difficult to debug problems.

        Please run the following command to install a compatible version of TypeScript.

            npm install typescript@"${currentCombo.typescript}"

        To disable this warning run "ng config cli.warnings.typescriptMismatch false".
      ` + '\n')));
        }
    }
}
exports.Version = Version;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoidmVyc2lvbi5qcyIsInNvdXJjZVJvb3QiOiIuLyIsInNvdXJjZXMiOlsicGFja2FnZXMvYW5ndWxhci9jbGkvdXBncmFkZS92ZXJzaW9uLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7QUFBQTs7Ozs7O0dBTUc7O0FBRUgsK0NBQXNEO0FBQ3RELG9EQUFvRDtBQUNwRCw2QkFBNkI7QUFDN0IsbUNBQTJDO0FBQzNDLGdEQUF1RDtBQUd2RDtJQUVFLFlBQW9CLFdBQTBCLElBQUk7UUFBOUIsYUFBUSxHQUFSLFFBQVEsQ0FBc0I7UUFEMUMsWUFBTyxHQUFrQixJQUFJLENBQUM7UUFFcEMsSUFBSSxDQUFDLE9BQU8sR0FBRyxRQUFRLENBQUMsQ0FBQyxDQUFDLElBQUksZUFBTSxDQUFDLFFBQVEsQ0FBQyxDQUFDLENBQUMsQ0FBQyxJQUFJLENBQUM7SUFDeEQsQ0FBQztJQUVELE9BQU8sS0FBSyxPQUFPLElBQUksQ0FBQyxTQUFTLElBQUksT0FBTyxDQUFDLENBQUMsQ0FBQztJQUMvQyxNQUFNLEtBQUssT0FBTyxJQUFJLENBQUMsU0FBUyxJQUFJLE1BQU0sQ0FBQyxDQUFDLENBQUM7SUFDN0Msa0JBQWtCLEtBQUssT0FBTyxJQUFJLENBQUMsU0FBUyxJQUFJLElBQUksQ0FBQyxDQUFDLENBQUM7SUFDdkQsT0FBTyxLQUFLLE9BQU8sSUFBSSxDQUFDLFFBQVEsS0FBSyxJQUFJLENBQUMsQ0FBQyxDQUFDO0lBRTVDLE9BQU8sS0FBSyxPQUFPLElBQUksQ0FBQyxPQUFPLEVBQUUsSUFBSSxJQUFJLENBQUMsUUFBUSxJQUFJLElBQUksQ0FBQyxVQUFVLENBQUMsSUFBSSxDQUFDLFFBQVEsQ0FBQyxDQUFDLENBQUMsQ0FBQztJQUN2RixzQkFBc0IsQ0FBQyxLQUFhO1FBQ2xDLE9BQU8sSUFBSSxDQUFDLE9BQU8sS0FBSyxJQUFJLElBQUksSUFBSSxDQUFDLE9BQU8sQ0FBQyxPQUFPLENBQUMsS0FBSyxDQUFDLElBQUksQ0FBQyxDQUFDO0lBQ25FLENBQUM7SUFFRCxJ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LElBQUksR0FBRyxXQUFJLENBQUMsV0FBVyxDQUFBO2dDQUM1QixlQUFlLHlCQUN2QyxZQUFZLENBQUMsVUFBVSxTQUFTLFNBQVM7Ozs7O3NDQUtYLFlBQVksQ0FBQyxVQUFVOzs7T0FHdEQsR0FBRyxJQUFJLENBQUMsQ0FBQyxDQUFDLENBQUM7U0FDYjtJQUNILENBQUM7Q0FFRjtBQWhMRCwwQkFnTEMiLCJzb3VyY2VzQ29udGVudCI6WyIvKipcbiAqIEBsaWNlbnNlXG4gKiBDb3B5cmlnaHQgR29vZ2xlIEluYy4gQWxsIFJpZ2h0cyBSZXNlcnZlZC5cbiAqXG4gKiBVc2Ugb2YgdGhpcyBzb3VyY2UgY29kZSBpcyBnb3Zlcm5lZCBieSBhbiBNSVQtc3R5bGUgbGljZW5zZSB0aGF0IGNhbiBiZVxuICogZm91bmQgaW4gdGhlIExJQ0VOU0UgZmlsZSBhdCBodHRwczovL2FuZ3VsYXIuaW8vbGljZW5zZVxuICovXG5cbmltcG9ydCB7IHRhZ3MsIHRlcm1pbmFsIH0gZnJvbSAnQGFuZ3VsYXItZGV2a2l0L2NvcmUnO1xuaW1wb3J0IHsgcmVzb2x2ZSB9IGZyb20gJ0Bhbmd1bGFyLWRldmtpdC9jb3JlL25vZGUnO1xuaW1wb3J0ICogYXMgcGF0aCBmcm9tICdwYXRoJztcbmltcG9ydCB7IFNlbVZlciwgc2F0aXNmaWVzIH0gZnJvbSAnc2VtdmVyJztcbmltcG9ydCB7IGlzV2FybmluZ0VuYWJsZWQgfSBmcm9tICcuLi91dGlsaXRpZXMvY29uZmlnJztcblxuXG5leHBvcnQgY2xhc3MgVmVyc2lvbiB7XG4gIHByaXZhdGUgX3NlbXZlcjogU2VtVmVyIHwgbnVsbCA9IG51bGw7XG4gIGNvbnN0cnVjdG9yKHByaXZhdGUgX3ZlcnNpb246IHN0cmluZyB8IG51bGwgPSBudWxsKSB7XG4gICAgdGhpcy5fc2VtdmVyID0gX3ZlcnNpb24gPyBuZXcgU2VtVmVyKF92ZXJzaW9uKSA6IG51bGw7XG4gIH1cblxuICBpc0FscGhhKCkgeyByZXR1cm4gdGhpcy5xdWFsaWZpZXIgPT0gJ2FscGhhJzsgfVxuICBpc0JldGEoKSB7IHJldHVybiB0aGlzLnF1YWxpZmllciA9PSAnYmV0YSc7IH1cbiAgaXNSZWxlYXNlQ2FuZGlkYXRlKCkgeyByZXR1cm4gdGhpcy5xdWFsaWZpZXIgPT0gJ3JjJzsgfVxuICBpc0tub3duKCkgeyByZXR1cm4gdGhpcy5fdmVyc2lvbiAhPT0gbnVsbDsgfVxuXG4gIGlzTG9jYWwoKSB7IHJldHVybiB0aGlzLmlzS25vd24oKSAmJiB0aGlzLl92ZXJzaW9uICYmIHBhdGguaXNBYnNvbHV0ZSh0aGlzLl92ZXJzaW9uKTsgfVxuICBpc0dyZWF0ZXJUaGFuT3JFcXVhbFRvKG90aGVyOiBTZW1WZXIpIHtcbiAgICByZXR1cm4gdGhpcy5fc2VtdmVyICE9PSBudWxsICYmIHRoaXMuX3NlbXZlci5jb21wYXJlKG90aGVyKSA+PSAwO1xuICB9XG5cbiAgZ2V0IG1ham9yKCkgeyByZXR1cm4gdGhpcy5fc2VtdmVyID8gdGhpcy5fc2VtdmVyLm1ham9yIDogMDsgfVxuICBnZXQgbWlub3IoKSB7IHJldHVybiB0aGlzLl9zZW12ZXIgPyB0aGlzLl9zZW12ZXIubWlub3IgOiAwOyB9XG4gIGdldCBwYXRjaCgpIHsgcmV0dXJuIHRoaXMuX3NlbXZlciA/IHRoaXMuX3NlbXZlci5wYXRjaCA6IDA7IH1cbiAgZ2V0IHF1YWxpZmllcigpIHsgcmV0dXJuIHRoaXMuX3NlbXZlciA/IHRoaXMuX3NlbXZlci5wcmVyZWxlYXNlWzBdIDogJyc7IH1cbiAgZ2V0IGV4dHJhKCkgeyByZXR1cm4gdGhpcy5fc2VtdmVyID8gdGhpcy5fc2VtdmVyLnByZXJlbGVhc2VbMV0gOiAnJzsgfVxuXG4gIHRvU3RyaW5nKCkgeyByZXR1cm4gdGhpcy5fdmVyc2lvbjsgfVxuXG4gIHN0YXRpYyBhc3NlcnRDb21wYXRpYmxlQW5ndWxhclZlcnNpb24ocHJvamVjdFJvb3Q6IHN0cmluZykge1xuICAgIGxldCBhbmd1bGFyUGtnSnNvbjtcbiAgICBsZXQgcnhqc1BrZ0pzb247XG5cbiAgICBjb25zdCBpc0luc2lkZSA9IChiYXNlOiBzdHJpbmcsIHBvdGVudGlhbDogc3RyaW5nKTogYm9vbGVhbiA9PiB7XG4gICAgICBjb25zdCBhYnNvbHV0ZUJhc2UgPSBwYXRoLnJlc29sdmUoYmFzZSk7XG4gICAgICBjb25zdCBhYnNvbHV0ZVBvdGVudGlhbCA9IHBhdGgucmVzb2x2ZShwb3RlbnRpYWwpO1xuICAgICAgY29uc3QgcmVsYXRpdmVQb3RlbnRpYWwgPSBwYXRoLnJlbGF0aXZlKGFic29sdXRlQmFzZSwgYWJzb2x1dGVQb3RlbnRpYWwpO1xuICAgICAgaWYgKCFyZWxhdGl2ZVBvdGVudGlhbC5zdGFydHNXaXRoKCcuLicpICYmICFwYXRoLmlzQWJzb2x1dGUocmVsYXRpdmVQb3RlbnRpYWwpKSB7XG4gICAgICAgIHJldHVybiB0cnVlO1xuICAgICAgfVxuXG4gICAgICByZXR1cm4gZmFsc2U7XG4gICAgfTtcblxuICAgIHRyeSB7XG4gICAgICBjb25zdCByZXNvbHZlT3B0aW9ucyA9IHtcbiAgICAgICAgYmFzZWRpcjogcHJvamVjdFJvb3QsXG4gICAgICAgIGNoZWNrR2xvYmFsOiBmYWxzZSxcbiAgICAgICAgY2hlY2tMb2NhbDogdHJ1ZSxcbiAgICAgIH07XG4gICAgICBjb25zdCBhbmd1bGFyUGFja2FnZVBhdGggPSByZXNvbHZlKCdAYW5ndWxhci9jb3JlL3BhY2thZ2UuanNvbicsIHJlc29sdmVPcHRpb25zKTtcbiAgICAgIGNvbnN0IHJ4anNQYWNrYWdlUGF0aCA9IHJlc29sdmUoJ3J4anMvcGFja2FnZS5qc29uJywgcmVzb2x2ZU9wdGlvbnMpO1xuXG4gICAgICBpZiAoIWlzSW5zaWRlKHByb2plY3RSb290LCBhbmd1bGFyUGFja2FnZVBhdGgpXG4gICAgICAgICAgfHwgIWlzSW5zaWRlKHByb2plY3RSb290LCByeGpzUGFja2FnZVBhdGgpKSB7XG4gICAgICAgIHRocm93IG5ldyBFcnJvcigpO1xuICAgICAgfVxuXG4gICAgICBhbmd1bGFyUGtnSnNvbiA9IHJlcXVpcmUoYW5ndWxhclBhY2thZ2VQYXRoKTtcbiAgICAgIHJ4anNQa2dKc29uID0gcmVxdWlyZShyeGpzUGFja2FnZVBhdGgpO1xuICAgIH0gY2F0Y2gge1xuICAgICAgY29uc29sZS5lcnJvcih0ZXJtaW5hbC5ib2xkKHRlcm1pbmFsLnJlZCh0YWdzLnN0cmlwSW5kZW50c2BcbiAgICAgICAgWW91IHNlZW0gdG8gbm90IGJlIGRlcGVuZGluZyBvbiBcIkBhbmd1bGFyL2NvcmVcIiBhbmQvb3IgXCJyeGpzXCIuIFRoaXMgaXMgYW4gZXJyb3IuXG4gICAgICBgKSkpO1xuICAgICAgcHJvY2Vzcy5leGl0KDIpO1xuICAgIH1cblxuICAgIGlmICghKGFuZ3VsYXJQa2dKc29uICYmIGFuZ3VsYXJQa2dKc29uWyd2ZXJzaW9uJ10gJiYgcnhqc1BrZ0pzb24gJiYgcnhqc1BrZ0pzb25bJ3ZlcnNpb24nXSkpIHtcbiAgICAgIGNvbnNvbGUuZXJyb3IodGVybWluYWwuYm9sZCh0ZXJtaW5hbC5yZWQodGFncy5zdHJpcEluZGVudHNgXG4gICAgICAgIENhbm5vdCBkZXRlcm1pbmUgdmVyc2lvbnMgb2YgXCJAYW5ndWxhci9jb3JlXCIgYW5kL29yIFwicnhqc1wiLlxuICAgICAgICBUaGlzIGxpa2VseSBtZWFucyB5b3VyIGxvY2FsIGluc3RhbGxhdGlvbiBpcyBicm9rZW4uIFBsZWFzZSByZWluc3RhbGwgeW91ciBwYWNrYWdlcy5cbiAgICAgIGApKSk7XG4gICAgICBwcm9jZXNzLmV4aXQoMik7XG4gICAgfVxuXG4gICAgY29uc3QgYW5ndWxhclZlcnNpb24gPSBuZXcgVmVyc2lvbihhbmd1bGFyUGtnSnNvblsndmVyc2lvbiddKTtcbiAgICBjb25zdCByeGpzVmVyc2lvbiA9IG5ldyBWZXJzaW9uKHJ4anNQa2dKc29uWyd2ZXJzaW9uJ10pO1xuXG4gICAgaWYgKGFuZ3VsYXJWZXJzaW9uLmlzTG9jYWwoKSkge1xuICAgICAgY29uc29sZS5lcnJvcih0ZXJtaW5hbC55ZWxsb3coJ1VzaW5nIGEgbG9jYWwgdmVyc2lvbiBvZiBhbmd1bGFyLiBQcm9jZWVkaW5nIHdpdGggY2FyZS4uLicpKTtcblxuICAgICAgcmV0dXJuO1xuICAgIH1cblxuICAgIGlmICghYW5ndWxhclZlcnNpb24uaXNHcmVhdGVyVGhhbk9yRXF1YWxUbyhuZXcgU2VtVmVyKCc1LjAuMCcpKSkge1xuICAgICAgY29uc29sZS5lcnJvcih0ZXJtaW5hbC5ib2xkKHRlcm1pbmFsLnJlZCh0YWdzLnN0cmlwSW5kZW50c2BcbiAgICAgICAgICBUaGlzIHZlcnNpb24gb2YgQ0xJIGlzIG9ubHkgY29tcGF0aWJsZSB3aXRoIEFuZ3VsYXIgdmVyc2lvbiA1LjAuMCBvciBoaWdoZXIuXG5cbiAgICAgICAgICBQbGVhc2UgdmlzaXQgdGhlIGxpbmsgYmVsb3cgdG8gZmluZCBpbnN0cnVjdGlvbnMgb24gaG93IHRvIHVwZGF0ZSBBbmd1bGFyLlxuICAgICAgICAgIGh0dHBzOi8vYW5ndWxhci11cGRhdGUtZ3VpZGUuZmlyZWJhc2VhcHAuY29tL1xuICAgICAgICBgICsgJ1xcbicpKSk7XG4gICAgICBwcm9jZXNzLmV4aXQoMyk7XG4gICAgfSBlbHNlIGlmIChcbiAgICAgIGFuZ3VsYXJWZXJzaW9uLmlzR3JlYXRlclRoYW5PckVxdWFsVG8obmV3IFNlbVZlcignNi4wLjAtcmMuMCcpKVxuICAgICAgJiYgIXJ4anNWZXJzaW9uLmlzR3JlYXRlclRoYW5PckVxdWFsVG8obmV3IFNlbVZlcignNS42LjAtZm9yd2FyZC1jb21wYXQuMCcpKVxuICAgICAgJiYgIXJ4anNWZXJzaW9uLmlzR3JlYXRlclRoYW5PckVxdWFsVG8obmV3IFNlbVZlcignNi4wLjAtYmV0YS4wJykpXG4gICAgKSB7XG4gICAgICBjb25zb2xlLmVycm9yKHRlcm1pbmFsLmJvbGQodGVybWluYWwucmVkKHRhZ3Muc3RyaXBJbmRlbnRzYFxuICAgICAgICAgIFRoaXMgcHJvamVjdCB1c2VzIHZlcnNpb24gJHtyeGpzVmVyc2lvbn0gb2YgUnhKcywgd2hpY2ggaXMgbm90IHN1cHBvcnRlZCBieSBBbmd1bGFyIHY2LlxuICAgICAgICAgIFRoZSBvZmZpY2lhbCBSeEpzIHZlcnNpb24gdGhhdCBpcyBzdXBwb3J0ZWQgaXMgNS42LjAtZm9yd2FyZC1jb21wYXQuMCBhbmQgZ3JlYXRlci5cblxuICAgICAgICAgIFBsZWFzZSB2aXNpdCB0aGUgbGluayBiZWxvdyB0byBmaW5kIGluc3RydWN0aW9ucyBvbiBob3cgdG8gdXBkYXRlIFJ4SnMuXG4gICAgICAgICAgaHR0cHM6Ly9kb2NzLmdvb2dsZS5jb20vZG9jdW1lbnQvZC8xMm5sTHQ3MVZMS2ItejNZYVNHelVmeDZtSmJjMzRuc01YdEJ5UFVOMzVjZy9lZGl0I1xuICAgICAgICBgICsgJ1xcbicpKSk7XG4gICAgICBwcm9jZXNzLmV4aXQoMyk7XG4gICAgfSBlbHNlIGlmIChcbiAgICAgIGFuZ3VsYXJWZXJzaW9uLmlzR3JlYXRlclRoYW5PckVxdWFsVG8obmV3IFNlbVZlcignNi4wLjAtcmMuMCcpKVxuICAgICAgJiYgIXJ4anNWZXJzaW9uLmlzR3JlYXRlclRoYW5PckVxdWFsVG8obmV3IFNlbVZlcignNi4wLjAtYmV0YS4wJykpXG4gICAgKSB7XG4gICAgICBjb25zb2xlLndhcm4odGVybWluYWwuYm9sZCh0ZXJtaW5hbC5yZWQodGFncy5zdHJpcEluZGVudHNgXG4gICAgICAgICAgVGhpcyBwcm9qZWN0IHVzZXMgYSB0ZW1wb3JhcnkgY29tcGF0aWJpbGl0eSB2ZXJzaW9uIG9mIFJ4SnMgKCR7cnhqc1ZlcnNpb259KS5cblxuICAgICAgICAgIFBsZWFzZSB2aXNpdCB0aGUgbGluayBiZWxvdyB0byBmaW5kIGluc3RydWN0aW9ucyBvbiBob3cgdG8gdXBkYXRlIFJ4SnMuXG4gICAgICAgICAgaHR0cHM6Ly9kb2NzLmdvb2dsZS5jb20vZG9jdW1lbnQvZC8xMm5sTHQ3MVZMS2ItejNZYVNHelVmeDZtSmJjMzRuc01YdEJ5UFVOMzVjZy9lZGl0I1xuICAgICAgICBgICsgJ1xcbicpKSk7XG4gICAgfVxuICB9XG5cbiAgc3RhdGljIGFzc2VydFR5cGVzY3JpcHRWZXJzaW9uKHByb2plY3RSb290OiBzdHJpbmcpIHtcbiAgICBpZiAoIWlzV2FybmluZ0VuYWJsZWQoJ3R5cGVzY3JpcHRNaXNtYXRjaCcpKSB7XG4gICAgICByZXR1cm47XG4gICAgfVxuXG4gICAgbGV0IGNvbXBpbGVyVmVyc2lvbjogc3RyaW5nO1xuICAgIGxldCB0c1ZlcnNpb246IHN0cmluZztcbiAgICBsZXQgY29tcGlsZXJUeXBlU2NyaXB0UGVlclZlcnNpb246IHN0cmluZztcbiAgICB0cnkge1xuICAgICAgY29uc3QgcmVzb2x2ZU9wdGlvbnMgPSB7XG4gICAgICAgIGJhc2VkaXI6IHByb2plY3RSb290LFxuICAgICAgICBjaGVja0dsb2JhbDogZmFsc2UsXG4gICAgICAgIGNoZWNrTG9jYWw6IHRydWUsXG4gICAgICB9O1xuICAgICAgY29uc3QgY29tcGlsZXJQYWNrYWdlUGF0aCA9IHJlc29sdmUoJ0Bhbmd1bGFyL2NvbXBpbGVyLWNsaS9wYWNrYWdlLmpzb24nLCByZXNvbHZlT3B0aW9ucyk7XG4gICAgICBjb25zdCB0eXBlc2NyaXB0UHJvamVjdFBhdGggPSByZXNvbHZlKCd0eXBlc2NyaXB0JywgcmVzb2x2ZU9wdGlvbnMpO1xuICAgICAgY29uc3QgY29tcGlsZXJQYWNrYWdlSW5mbyA9IHJlcXVpcmUoY29tcGlsZXJQYWNrYWdlUGF0aCk7XG5cbiAgICAgIGNvbXBpbGVyVmVyc2lvbiA9IGNvbXBpbGVyUGFja2FnZUluZm9bJ3ZlcnNpb24nXTtcbiAgICAgIGNvbXBpbGVyVHlwZVNjcmlwdFBlZXJWZXJzaW9uID0gY29tcGlsZXJQYWNrYWdlSW5mb1sncGVlckRlcGVuZGVuY2llcyddWyd0eXBlc2NyaXB0J107XG4gICAgICB0c1ZlcnNpb24gPSByZXF1aXJlKHR5cGVzY3JpcHRQcm9qZWN0UGF0aCkudmVyc2lvbjtcbiAgICB9IGNhdGNoIHtcbiAgICAgIGNvbnNvbGUuZXJyb3IodGVybWluYWwuYm9sZCh0ZXJtaW5hbC5yZWQodGFncy5zdHJpcEluZGVudHNgXG4gICAgICAgIFZlcnNpb25zIG9mIEBhbmd1bGFyL2NvbXBpbGVyLWNsaSBhbmQgdHlwZXNjcmlwdCBjb3VsZCBub3QgYmUgZGV0ZXJtaW5lZC5cbiAgICAgICAgVGhlIG1vc3QgY29tbW9uIHJlYXNvbiBmb3IgdGhpcyBpcyBhIGJyb2tlbiBucG0gaW5zdGFsbC5cblxuICAgICAgICBQbGVhc2UgbWFrZSBzdXJlIHlvdXIgcGFja2FnZS5qc29uIGNvbnRhaW5zIGJvdGggQGFuZ3VsYXIvY29tcGlsZXItY2xpIGFuZCB0eXBlc2NyaXB0IGluXG4gICAgICAgIGRldkRlcGVuZGVuY2llcywgdGhlbiBkZWxldGUgbm9kZV9tb2R1bGVzIGFuZCBwYWNrYWdlLWxvY2suanNvbiAoaWYgeW91IGhhdmUgb25lKSBhbmRcbiAgICAgICAgcnVuIG5wbSBpbnN0YWxsIGFnYWluLlxuICAgICAgYCkpKTtcbiAgICAgIHByb2Nlc3MuZXhpdCgyKTtcblxuICAgICAgcmV0dXJuO1xuICAgIH1cblxuICAgIC8vIFRoZXNlIHZlcnNpb25zIGRvIG5vdCBoYXZlIGFjY3VyYXRlIHR5cGVzY3JpcHQgcGVlciBkZXBlbmRlbmNpZXNcbiAgICBjb25zdCB2ZXJzaW9uQ29tYm9zID0gW1xuICAgICAgeyBjb21waWxlcjogJz49Mi4zLjEgPDMuMC4wJywgdHlwZXNjcmlwdDogJz49Mi4wLjIgPDIuMy4wJyB9LFxuICAgICAgeyBjb21waWxlcjogJz49NC4wLjAtYmV0YS4wIDw1LjAuMCcsIHR5cGVzY3JpcHQ6ICc+PTIuMS4wIDwyLjQuMCcgfSxcbiAgICAgIHsgY29tcGlsZXI6ICc1LjAuMC1iZXRhLjAgLSA1LjAuMC1yYy4yJywgdHlwZXNjcmlwdDogJz49Mi40LjIgPDIuNS4wJyB9LFxuICAgIF07XG5cbiAgICBsZXQgY3VycmVudENvbWJvID0gdmVyc2lvbkNvbWJvcy5maW5kKChjb21ibykgPT4gc2F0aXNmaWVzKGNvbXBpbGVyVmVyc2lvbiwgY29tYm8uY29tcGlsZXIpKTtcbiAgICBpZiAoIWN1cnJlbnRDb21ibyAmJiBjb21waWxlclR5cGVTY3JpcHRQZWVyVmVyc2lvbikge1xuICAgICAgY3VycmVudENvbWJvID0geyBjb21waWxlcjogY29tcGlsZXJWZXJzaW9uLCB0eXBlc2NyaXB0OiBjb21waWxlclR5cGVTY3JpcHRQZWVyVmVyc2lvbiB9O1xuICAgIH1cblxuICAgIGlmIChjdXJyZW50Q29tYm8gJiYgIXNhdGlzZmllcyh0c1ZlcnNpb24sIGN1cnJlbnRDb21iby50eXBlc2NyaXB0KSkge1xuICAgICAgLy8gRmlyc3QgbGluZSBvZiB3YXJuaW5nIGxvb2tzIHdlaXJkIGJlaW5nIHNwbGl0IGluIHR3bywgZGlzYWJsZSB0c2xpbnQgZm9yIGl0LlxuICAgICAgY29uc29sZS5lcnJvcigodGVybWluYWwueWVsbG93KCdcXG4nICsgdGFncy5zdHJpcEluZGVudGBcbiAgICAgICAgQGFuZ3VsYXIvY29tcGlsZXItY2xpQCR7Y29tcGlsZXJWZXJzaW9ufSByZXF1aXJlcyB0eXBlc2NyaXB0QCcke1xuICAgICAgICBjdXJyZW50Q29tYm8udHlwZXNjcmlwdH0nIGJ1dCAke3RzVmVyc2lvbn0gd2FzIGZvdW5kIGluc3RlYWQuXG4gICAgICAgIFVzaW5nIHRoaXMgdmVyc2lvbiBjYW4gcmVzdWx0IGluIHVuZGVmaW5lZCBiZWhhdmlvdXIgYW5kIGRpZmZpY3VsdCB0byBkZWJ1ZyBwcm9ibGVtcy5cblxuICAgICAgICBQbGVhc2UgcnVuIHRoZSBmb2xsb3dpbmcgY29tbWFuZCB0byBpbnN0YWxsIGEgY29tcGF0aWJsZSB2ZXJzaW9uIG9mIFR5cGVTY3JpcHQuXG5cbiAgICAgICAgICAgIG5wbSBpbnN0YWxsIHR5cGVzY3JpcHRAXCIke2N1cnJlbnRDb21iby50eXBlc2NyaXB0fVwiXG5cbiAgICAgICAgVG8gZGlzYWJsZSB0aGlzIHdhcm5pbmcgcnVuIFwibmcgY29uZmlnIGNsaS53YXJuaW5ncy50eXBlc2NyaXB0TWlzbWF0Y2ggZmFsc2VcIi5cbiAgICAgIGAgKyAnXFxuJykpKTtcbiAgICB9XG4gIH1cblxufVxuIl19