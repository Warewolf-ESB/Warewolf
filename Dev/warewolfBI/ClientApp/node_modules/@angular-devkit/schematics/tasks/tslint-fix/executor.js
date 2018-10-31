"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
const node_1 = require("@angular-devkit/core/node");
const fs = require("fs");
const path = require("path");
const rxjs_1 = require("rxjs");
function _loadConfiguration(Configuration, options, root, file) {
    if (options.tslintConfig) {
        return Configuration.parseConfigFile(options.tslintConfig, root);
    }
    else if (options.tslintPath) {
        return Configuration.findConfiguration(path.join(root, options.tslintPath)).results;
    }
    else if (file) {
        return Configuration.findConfiguration(null, file).results;
    }
    else {
        throw new Error('Executor must specify a tslint configuration.');
    }
}
function _getFileContent(file, options, program) {
    // The linter retrieves the SourceFile TS node directly if a program is used
    if (program) {
        const source = program.getSourceFile(file);
        if (!source) {
            const message = `File '${file}' is not part of the TypeScript project '${options.tsConfigPath}'.`;
            throw new Error(message);
        }
        return source.getFullText(source);
    }
    // NOTE: The tslint CLI checks for and excludes MPEG transport streams; this does not.
    try {
        // Strip BOM from file data.
        // https://stackoverflow.com/questions/24356713
        return fs.readFileSync(file, 'utf-8').replace(/^\uFEFF/, '');
    }
    catch (_a) {
        throw new Error(`Could not read file '${file}'.`);
    }
}
function _listAllFiles(root) {
    const result = [];
    function _recurse(location) {
        const dir = fs.readdirSync(path.join(root, location));
        dir.forEach(name => {
            const loc = path.join(location, name);
            if (fs.statSync(path.join(root, loc)).isDirectory()) {
                _recurse(loc);
            }
            else {
                result.push(loc);
            }
        });
    }
    _recurse('');
    return result;
}
function default_1() {
    return (options, context) => {
        return new rxjs_1.Observable(obs => {
            const root = process.cwd();
            const tslint = require(node_1.resolve('tslint', {
                basedir: root,
                checkGlobal: true,
                checkLocal: true,
            }));
            const includes = (Array.isArray(options.includes)
                ? options.includes
                : (options.includes ? [options.includes] : []));
            const files = (Array.isArray(options.files)
                ? options.files
                : (options.files ? [options.files] : []));
            const Linter = tslint.Linter;
            const Configuration = tslint.Configuration;
            let program = undefined;
            let filesToLint = files;
            if (options.tsConfigPath && files.length == 0) {
                const tsConfigPath = path.join(process.cwd(), options.tsConfigPath);
                if (!fs.existsSync(tsConfigPath)) {
                    obs.error(new Error('Could not find tsconfig.'));
                    return;
                }
                program = Linter.createProgram(tsConfigPath);
                filesToLint = Linter.getFileNames(program);
            }
            if (includes.length > 0) {
                const allFilesRel = _listAllFiles(root);
                const pattern = '^('
                    + includes
                        .map(ex => '('
                        + ex.split(/[\/\\]/g).map(f => f
                            .replace(/[\-\[\]{}()+?.^$|]/g, '\\$&')
                            .replace(/^\*\*/g, '(.+?)?')
                            .replace(/\*/g, '[^/\\\\]*'))
                            .join('[\/\\\\]')
                        + ')')
                        .join('|')
                    + ')($|/|\\\\)';
                const re = new RegExp(pattern);
                filesToLint.push(...allFilesRel
                    .filter(x => re.test(x))
                    .map(x => path.join(root, x)));
            }
            const lintOptions = {
                fix: true,
                formatter: options.format || 'prose',
            };
            const linter = new Linter(lintOptions, program);
            // If directory doesn't change, we
            let lastDirectory = null;
            let config;
            for (const file of filesToLint) {
                const dir = path.dirname(file);
                if (lastDirectory !== dir) {
                    lastDirectory = dir;
                    config = _loadConfiguration(Configuration, options, root, file);
                }
                const content = _getFileContent(file, options, program);
                if (!content) {
                    continue;
                }
                linter.lint(file, content, config);
            }
            const result = linter.getResult();
            // Format and show the results.
            if (!options.silent) {
                const Formatter = tslint.findFormatter(options.format || 'prose');
                if (!Formatter) {
                    throw new Error(`Invalid lint format "${options.format}".`);
                }
                const formatter = new Formatter();
                const output = formatter.format(result.failures, result.fixes);
                if (output) {
                    context.logger.info(output);
                }
            }
            if (!options.ignoreErrors && result.errorCount > 0) {
                obs.error(new Error('Lint errors were found.'));
            }
            else {
                obs.next();
                obs.complete();
            }
        });
    };
}
exports.default = default_1;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiZXhlY3V0b3IuanMiLCJzb3VyY2VSb290IjoiLi8iLCJzb3VyY2VzIjpbInBhY2thZ2VzL2FuZ3VsYXJfZGV2a2l0L3NjaGVtYXRpY3MvdGFza3MvdHNsaW50LWZpeC9leGVjdXRvci50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiOztBQUFBOzs7Ozs7R0FNRztBQUNILG9EQUFvRDtBQUNwRCx5QkFBeUI7QUFDekIsNkJBQTZCO0FBQzdCLCtCQUFrQztBQWNsQyw0QkFDRSxhQUE2QixFQUM3QixPQUE2QixFQUM3QixJQUFZLEVBQ1osSUFBYTtJQUViLElBQUksT0FBTyxDQUFDLFlBQVksRUFBRTtRQUN4QixPQUFPLGFBQWEsQ0FBQyxlQUFlLENBQUMsT0FBTyxDQUFDLFlBQVksRUFBRSxJQUFJLENBQUMsQ0FBQztLQUNsRTtTQUFNLElBQUksT0FBTyxDQUFDLFVBQVUsRUFBRTtRQUM3QixPQUFPLGFBQWEsQ0FBQyxpQkFBaUIsQ0FBQyxJQUFJLENBQUMsSUFBSSxDQUFDLElBQUksRUFBRSxPQUFPLENBQUMsVUFBVSxDQUFDLENBQUMsQ0FBQyxPQUFPLENBQUM7S0FDckY7U0FBTSxJQUFJLElBQUksRUFBRTtRQUNmLE9BQU8sYUFBYSxDQUFDLGlCQUFpQixDQUFDLElBQUksRUFBRSxJQUFJLENBQUMsQ0FBQyxPQUFPLENBQUM7S0FDNUQ7U0FBTTtRQUNMLE1BQU0sSUFBSSxLQUFLLENBQUMsK0NBQStDLENBQUMsQ0FBQztLQUNsRTtBQUNILENBQUM7QUFHRCx5QkFDRSxJQUFZLEVBQ1osT0FBNkIsRUFDN0IsT0FBb0I7SUFFcEIsNEVBQTRFO0lBQzVFLElBQUksT0FBTyxFQUFFO1FBQ1gsTUFBTSxNQUFNLEdBQUcsT0FBTyxDQUFDLGFBQWEsQ0FBQyxJQUFJLENBQUMsQ0FBQztRQUMzQyxJQUFJLENBQUMsTUFBTSxFQUFFO1lBQ1gsTUFBTSxPQUFPLEdBQ1QsU0FBUyxJQUFJLDRDQUE0QyxPQUFPLENBQUMsWUFBWSxJQUFJLENBQUM7WUFDdEYsTUFBTSxJQUFJ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Q0FBQyxTQUFTLEVBQUU7b0JBQ2QsTUFBTSxJQUFJLEtBQUssQ0FBQyx3QkFBd0IsT0FBTyxDQUFDLE1BQU0sSUFBSSxDQUFDLENBQUM7aUJBQzdEO2dCQUNELE1BQU0sU0FBUyxHQUFHLElBQUksU0FBUyxFQUFFLENBQUM7Z0JBRWxDLE1BQU0sTUFBTSxHQUFHLFNBQVMsQ0FBQyxNQUFNLENBQUMsTUFBTSxDQUFDLFFBQVEsRUFBRSxNQUFNLENBQUMsS0FBSyxDQUFDLENBQUM7Z0JBQy9ELElBQUksTUFBTSxFQUFFO29CQUNWLE9BQU8sQ0FBQyxNQUFNLENBQUMsSUFBSSxDQUFDLE1BQU0sQ0FBQyxDQUFDO2lCQUM3QjthQUNGO1lBRUQsSUFBSSxDQUFDLE9BQU8sQ0FBQyxZQUFZLElBQUksTUFBTSxDQUFDLFVBQVUsR0FBRyxDQUFDLEVBQUU7Z0JBQ2xELEdBQUcsQ0FBQyxLQUFLLENBQUMsSUFBSSxLQUFLLENBQUMseUJBQXlCLENBQUMsQ0FBQyxDQUFDO2FBQ2pEO2lCQUFNO2dCQUNMLEdBQUcsQ0FBQyxJQUFJLEVBQUUsQ0FBQztnQkFDWCxHQUFHLENBQUMsUUFBUSxFQUFFLENBQUM7YUFDaEI7UUFDSCxDQUFDLENBQUMsQ0FBQztJQUNMLENBQUMsQ0FBQztBQUNKLENBQUM7QUEzR0QsNEJBMkdDIiwic291cmNlc0NvbnRlbnQiOlsiLyoqXG4gKiBAbGljZW5zZVxuICogQ29weXJpZ2h0IEdvb2dsZSBJbmMuIEFsbCBSaWdodHMgUmVzZXJ2ZWQuXG4gKlxuICogVXNlIG9mIHRoaXMgc291cmNlIGNvZGUgaXMgZ292ZXJuZWQgYnkgYW4gTUlULXN0eWxlIGxpY2Vuc2UgdGhhdCBjYW4gYmVcbiAqIGZvdW5kIGluIHRoZSBMSUNFTlNFIGZpbGUgYXQgaHR0cHM6Ly9hbmd1bGFyLmlvL2xpY2Vuc2VcbiAqL1xuaW1wb3J0IHsgcmVzb2x2ZSB9IGZyb20gJ0Bhbmd1bGFyLWRldmtpdC9jb3JlL25vZGUnO1xuaW1wb3J0ICogYXMgZnMgZnJvbSAnZnMnO1xuaW1wb3J0ICogYXMgcGF0aCBmcm9tICdwYXRoJztcbmltcG9ydCB7IE9ic2VydmFibGUgfSBmcm9tICdyeGpzJztcbmltcG9ydCB7XG4gIENvbmZpZ3VyYXRpb24gYXMgQ29uZmlndXJhdGlvbk5TLFxuICBMaW50ZXIgYXMgTGludGVyTlMsXG59IGZyb20gJ3RzbGludCc7ICAvLyB0c2xpbnQ6ZGlzYWJsZS1saW5lOm5vLWltcGxpY2l0LWRlcGVuZGVuY2llc1xuaW1wb3J0ICogYXMgdHMgZnJvbSAndHlwZXNjcmlwdCc7ICAvLyB0c2xpbnQ6ZGlzYWJsZS1saW5lOm5vLWltcGxpY2l0LWRlcGVuZGVuY2llc1xuaW1wb3J0IHsgU2NoZW1hdGljQ29udGV4dCwgVGFza0V4ZWN1dG9yIH0gZnJvbSAnLi4vLi4vc3JjJztcbmltcG9ydCB7IFRzbGludEZpeFRhc2tPcHRpb25zIH0gZnJvbSAnLi9vcHRpb25zJztcblxuXG50eXBlIENvbmZpZ3VyYXRpb25UID0gdHlwZW9mIENvbmZpZ3VyYXRpb25OUztcbnR5cGUgTGludGVyVCA9IHR5cGVvZiBMaW50ZXJOUztcblxuXG5mdW5jdGlvbiBfbG9hZENvbmZpZ3VyYXRpb24oXG4gIENvbmZpZ3VyYXRpb246IENvbmZpZ3VyYXRpb25ULFxuICBvcHRpb25zOiBUc2xpbnRGaXhUYXNrT3B0aW9ucyxcbiAgcm9vdDogc3RyaW5nLFxuICBmaWxlPzogc3RyaW5nLFxuKSB7XG4gIGlmIChvcHRpb25zLnRzbGludENvbmZpZykge1xuICAgIHJldHVybiBDb25maWd1cmF0aW9uLnBhcnNlQ29uZmlnRmlsZShvcHRpb25zLnRzbGludENvbmZpZywgcm9vdCk7XG4gIH0gZWxzZSBpZiAob3B0aW9ucy50c2xpbnRQYXRoKSB7XG4gICAgcmV0dXJuIENvbmZpZ3VyYXRpb24uZmluZENvbmZpZ3VyYXRpb24ocGF0aC5qb2luKHJvb3QsIG9wdGlvbnMudHNsaW50UGF0aCkpLnJlc3VsdHM7XG4gIH0gZWxzZSBpZiAoZmlsZSkge1xuICAgIHJldHVybiBDb25maWd1cmF0aW9uLmZpbmRDb25maWd1cmF0aW9uKG51bGwsIGZpbGUpLnJlc3VsdHM7XG4gIH0gZWxzZSB7XG4gICAgdGhyb3cgbmV3IEVycm9yKCdFeGVjdXRvciBtdXN0IHNwZWNpZnkgYSB0c2xpbnQgY29uZmlndXJhdGlvbi4nKTtcbiAgfVxufVxuXG5cbmZ1bmN0aW9uIF9nZXRGaWxlQ29udGVudChcbiAgZmlsZTogc3RyaW5nLFxuICBvcHRpb25zOiBUc2xpbnRGaXhUYXNrT3B0aW9ucyxcbiAgcHJvZ3JhbT86IHRzLlByb2dyYW0sXG4pOiBzdHJpbmcgfCB1bmRlZmluZWQge1xuICAvLyBUaGUgbGludGVyIHJldHJpZXZlcyB0aGUgU291cmNlRmlsZSBUUyBub2RlIGRpcmVjdGx5IGlmIGEgcHJvZ3JhbSBpcyB1c2VkXG4gIGlmIChwcm9ncmFtKSB7XG4gICAgY29uc3Qgc291cmNlID0gcHJvZ3JhbS5nZXRTb3VyY2VGaWxlKGZpbGUpO1xuICAgIGlmICghc291cmNlKSB7XG4gICAgICBjb25zdCBtZXNzYWdlXG4gICAgICAgID0gYEZpbGUgJyR7ZmlsZX0nIGlzIG5vdCBwYXJ0IG9mIHRoZSBUeXBlU2NyaXB0IHByb2plY3QgJyR7b3B0aW9ucy50c0NvbmZpZ1BhdGh9Jy5gO1xuICAgICAgdGhyb3cgbmV3IEVycm9yKG1lc3NhZ2UpO1xuICAgIH1cblxuICAgIHJldHVybiBzb3VyY2UuZ2V0RnVsbFRleHQoc291cmNlKTtcbiAgfVxuXG4gIC8vIE5PVEU6IFRoZSB0c2xpbnQgQ0xJIGNoZWNrcyBmb3IgYW5kIGV4Y2x1ZGVzIE1QRUcgdHJhbnNwb3J0IHN0cmVhbXM7IHRoaXMgZG9lcyBub3QuXG4gIHRyeSB7XG4gICAgLy8gU3RyaXAgQk9NIGZyb20gZmlsZSBkYXRhLlxuICAgIC8vIGh0dHBzOi8vc3RhY2tvdmVyZmxvdy5jb20vcXVlc3Rpb25zLzI0MzU2NzEzXG4gICAgcmV0dXJuIGZzLnJlYWRGaWxlU3luYyhmaWxlLCAndXRmLTgnKS5yZXBsYWNlKC9eXFx1RkVGRi8sICcnKTtcbiAgfSBjYXRjaCB7XG4gICAgdGhyb3cgbmV3IEVycm9yKGBDb3VsZCBub3QgcmVhZCBmaWxlICcke2ZpbGV9Jy5gKTtcbiAgfVxufVxuXG5cbmZ1bmN0aW9uIF9saXN0QWxsRmlsZXMocm9vdDogc3RyaW5nKTogc3RyaW5nW10ge1xuICBjb25zdCByZXN1bHQ6IHN0cmluZ1tdID0gW107XG5cbiAgZnVuY3Rpb24gX3JlY3Vyc2UobG9jYXRpb246IHN0cmluZykge1xuICAgIGNvbnN0IGRpciA9IGZzLnJlYWRkaXJTeW5jKHBhdGguam9pbihyb290LCBsb2NhdGlvbikpO1xuXG4gICAgZGlyLmZvckVhY2gobmFtZSA9PiB7XG4gICAgICBjb25zdCBsb2MgPSBwYXRoLmpvaW4obG9jYXRpb24sIG5hbWUpO1xuICAgICAgaWYgKGZzLnN0YXRTeW5jKHBhdGguam9pbihyb290LCBsb2MpKS5pc0RpcmVjdG9yeSgpKSB7XG4gICAgICAgIF9yZWN1cnNlKGxvYyk7XG4gICAgICB9IGVsc2Uge1xuICAgICAgICByZXN1bHQucHVzaChsb2MpO1xuICAgICAgfVxuICAgIH0pO1xuICB9XG4gIF9yZWN1cnNlKCcnKTtcblxuICByZXR1cm4gcmVzdWx0O1xufVxuXG5cbmV4cG9ydCBkZWZhdWx0IGZ1bmN0aW9uKCk6IFRhc2tFeGVjdXRvcjxUc2xpbnRGaXhUYXNrT3B0aW9ucz4ge1xuICByZXR1cm4gKG9wdGlvbnM6IFRzbGludEZpeFRhc2tPcHRpb25zLCBjb250ZXh0OiBTY2hlbWF0aWNDb250ZXh0KSA9PiB7XG4gICAgcmV0dXJuIG5ldyBPYnNlcnZhYmxlKG9icyA9PiB7XG4gICAgICBjb25zdCByb290ID0gcHJvY2Vzcy5jd2QoKTtcbiAgICAgIGNvbnN0IHRzbGludCA9IHJlcXVpcmUocmVzb2x2ZSgndHNsaW50Jywge1xuICAgICAgICBiYXNlZGlyOiByb290LFxuICAgICAgICBjaGVja0dsb2JhbDogdHJ1ZSxcbiAgICAgICAgY2hlY2tMb2NhbDogdHJ1ZSxcbiAgICAgIH0pKTtcbiAgICAgIGNvbnN0IGluY2x1ZGVzID0gKFxuICAgICAgICBBcnJheS5pc0FycmF5KG9wdGlvbnMuaW5jbHVkZXMpXG4gICAgICAgICAgPyBvcHRpb25zLmluY2x1ZGVzXG4gICAgICAgICAgOiAob3B0aW9ucy5pbmNsdWRlcyA/IFtvcHRpb25zLmluY2x1ZGVzXSA6IFtdKVxuICAgICAgKTtcbiAgICAgIGNvbnN0IGZpbGVzID0gKFxuICAgICAgICBBcnJheS5pc0FycmF5KG9wdGlvbnMuZmlsZXMpXG4gICAgICAgICAgPyBvcHRpb25zLmZpbGVzXG4gICAgICAgICAgOiAob3B0aW9ucy5maWxlcyA/IFtvcHRpb25zLmZpbGVzXSA6IFtdKVxuICAgICAgKTtcblxuICAgICAgY29uc3QgTGludGVyID0gdHNsaW50LkxpbnRlciBhcyBMaW50ZXJUO1xuICAgICAgY29uc3QgQ29uZmlndXJhdGlvbiA9IHRzbGludC5Db25maWd1cmF0aW9uIGFzIENvbmZpZ3VyYXRpb25UO1xuICAgICAgbGV0IHByb2dyYW06IHRzLlByb2dyYW0gfCB1bmRlZmluZWQgPSB1bmRlZmluZWQ7XG4gICAgICBsZXQgZmlsZXNUb0xpbnQ6IHN0cmluZ1tdID0gZmlsZXM7XG5cbiAgICAgIGlmIChvcHRpb25zLnRzQ29uZmlnUGF0aCAmJiBmaWxlcy5sZW5ndGggPT0gMCkge1xuICAgICAgICBjb25zdCB0c0NvbmZpZ1BhdGggPSBwYXRoLmpvaW4ocHJvY2Vzcy5jd2QoKSwgb3B0aW9ucy50c0NvbmZpZ1BhdGgpO1xuXG4gICAgICAgIGlmICghZnMuZXhpc3RzU3luYyh0c0NvbmZpZ1BhdGgpKSB7XG4gICAgICAgICAgb2JzLmVycm9yKG5ldyBFcnJvcignQ291bGQgbm90IGZpbmQgdHNjb25maWcuJykpO1xuXG4gICAgICAgICAgcmV0dXJuO1xuICAgICAgICB9XG4gICAgICAgIHByb2dyYW0gPSBMaW50ZXIuY3JlYXRlUHJvZ3JhbSh0c0NvbmZpZ1BhdGgpO1xuICAgICAgICBmaWxlc1RvTGludCA9IExpbnRlci5nZXRGaWxlTmFtZXMocHJvZ3JhbSk7XG4gICAgICB9XG5cbiAgICAgIGlmIChpbmNsdWRlcy5sZW5ndGggPiAwKSB7XG4gICAgICAgIGNvbnN0IGFsbEZpbGVzUmVsID0gX2xpc3RBbGxGaWxlcyhyb290KTtcbiAgICAgICAgY29uc3QgcGF0dGVybiA9ICdeKCdcbiAgICAgICAgICArIChpbmNsdWRlcyBhcyBzdHJpbmdbXSlcbiAgICAgICAgICAgIC5tYXAoZXggPT4gJygnXG4gICAgICAgICAgICAgICsgZXguc3BsaXQoL1tcXC9cXFxcXS9nKS5tYXAoZiA9PiBmXG4gICAgICAgICAgICAgICAgLnJlcGxhY2UoL1tcXC1cXFtcXF17fSgpKz8uXiR8XS9nLCAnXFxcXCQmJylcbiAgICAgICAgICAgICAgICAucmVwbGFjZSgvXlxcKlxcKi9nLCAnKC4rPyk/JylcbiAgICAgICAgICAgICAgICAucmVwbGFjZSgvXFwqL2csICdbXi9cXFxcXFxcXF0qJykpXG4gICAgICAgICAgICAgICAgLmpvaW4oJ1tcXC9cXFxcXFxcXF0nKVxuICAgICAgICAgICAgICArICcpJylcbiAgICAgICAgICAgIC5qb2luKCd8JylcbiAgICAgICAgICArICcpKCR8L3xcXFxcXFxcXCknO1xuICAgICAgICBjb25zdCByZSA9IG5ldyBSZWdFeHAocGF0dGVybik7XG5cbiAgICAgICAgZmlsZXNUb0xpbnQucHVzaCguLi5hbGxGaWxlc1JlbFxuICAgICAgICAgIC5maWx0ZXIoeCA9PiByZS50ZXN0KHgpKVxuICAgICAgICAgIC5tYXAoeCA9PiBwYXRoLmpvaW4ocm9vdCwgeCkpLFxuICAgICAgICApO1xuICAgICAgfVxuXG4gICAgICBjb25zdCBsaW50T3B0aW9ucyA9IHtcbiAgICAgICAgZml4OiB0cnVlLFxuICAgICAgICBmb3JtYXR0ZXI6IG9wdGlvbnMuZm9ybWF0IHx8ICdwcm9zZScsXG4gICAgICB9O1xuXG4gICAgICBjb25zdCBsaW50ZXIgPSBuZXcgTGludGVyKGxpbnRPcHRpb25zLCBwcm9ncmFtKTtcbiAgICAgIC8vIElmIGRpcmVjdG9yeSBkb2Vzbid0IGNoYW5nZSwgd2VcbiAgICAgIGxldCBsYXN0RGlyZWN0b3J5OiBzdHJpbmcgfCBudWxsID0gbnVsbDtcbiAgICAgIGxldCBjb25maWc7XG5cbiAgICAgIGZvciAoY29uc3QgZmlsZSBvZiBmaWxlc1RvTGludCkge1xuICAgICAgICBjb25zdCBkaXIgPSBwYXRoLmRpcm5hbWUoZmlsZSk7XG4gICAgICAgIGlmIChsYXN0RGlyZWN0b3J5ICE9PSBkaXIpIHtcbiAgICAgICAgICBsYXN0RGlyZWN0b3J5ID0gZGlyO1xuICAgICAgICAgIGNvbmZpZyA9IF9sb2FkQ29uZmlndXJhdGlvbihDb25maWd1cmF0aW9uLCBvcHRpb25zLCByb290LCBmaWxlKTtcbiAgICAgICAgfVxuICAgICAgICBjb25zdCBjb250ZW50ID0gX2dldEZpbGVDb250ZW50KGZpbGUsIG9wdGlvbnMsIHByb2dyYW0pO1xuXG4gICAgICAgIGlmICghY29udGVudCkge1xuICAgICAgICAgIGNvbnRpbnVlO1xuICAgICAgICB9XG5cbiAgICAgICAgbGludGVyLmxpbnQoZmlsZSwgY29udGVudCwgY29uZmlnKTtcbiAgICAgIH1cblxuICAgICAgY29uc3QgcmVzdWx0ID0gbGludGVyLmdldFJlc3VsdCgpO1xuXG4gICAgICAvLyBGb3JtYXQgYW5kIHNob3cgdGhlIHJlc3VsdHMuXG4gICAgICBpZiAoIW9wdGlvbnMuc2lsZW50KSB7XG4gICAgICAgIGNvbnN0IEZvcm1hdHRlciA9IHRzbGludC5maW5kRm9ybWF0dGVyKG9wdGlvbnMuZm9ybWF0IHx8ICdwcm9zZScpO1xuICAgICAgICBpZiAoIUZvcm1hdHRlcikge1xuICAgICAgICAgIHRocm93IG5ldyBFcnJvcihgSW52YWxpZCBsaW50IGZvcm1hdCBcIiR7b3B0aW9ucy5mb3JtYXR9XCIuYCk7XG4gICAgICAgIH1cbiAgICAgICAgY29uc3QgZm9ybWF0dGVyID0gbmV3IEZvcm1hdHRlcigpO1xuXG4gICAgICAgIGNvbnN0IG91dHB1dCA9IGZvcm1hdHRlci5mb3JtYXQocmVzdWx0LmZhaWx1cmVzLCByZXN1bHQuZml4ZXMpO1xuICAgICAgICBpZiAob3V0cHV0KSB7XG4gICAgICAgICAgY29udGV4dC5sb2dnZXIuaW5mbyhvdXRwdXQpO1xuICAgICAgICB9XG4gICAgICB9XG5cbiAgICAgIGlmICghb3B0aW9ucy5pZ25vcmVFcnJvcnMgJiYgcmVzdWx0LmVycm9yQ291bnQgPiAwKSB7XG4gICAgICAgIG9icy5lcnJvcihuZXcgRXJyb3IoJ0xpbnQgZXJyb3JzIHdlcmUgZm91bmQuJykpO1xuICAgICAgfSBlbHNlIHtcbiAgICAgICAgb2JzLm5leHQoKTtcbiAgICAgICAgb2JzLmNvbXBsZXRlKCk7XG4gICAgICB9XG4gICAgfSk7XG4gIH07XG59XG4iXX0=