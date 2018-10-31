"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const webpack_sources_1 = require("webpack-sources");
const purify_1 = require("./purify");
class PurifyPlugin {
    constructor() { }
    apply(compiler) {
        compiler.plugin('compilation', (compilation) => {
            // Webpack 4 provides the same functionality as this plugin and TS transformer
            compilation.warnings.push('PurifyPlugin is deprecated and will be removed in 0.7.0.');
            compilation.plugin('optimize-chunk-assets', (chunks, callback) => {
                chunks.forEach((chunk) => {
                    chunk.files
                        .filter((fileName) => fileName.endsWith('.js'))
                        .forEach((fileName) => {
                        const inserts = purify_1.purifyReplacements(compilation.assets[fileName].source());
                        if (inserts.length > 0) {
                            const replaceSource = new webpack_sources_1.ReplaceSource(compilation.assets[fileName], fileName);
                            inserts.forEach((insert) => {
                                replaceSource.insert(insert.pos, insert.content);
                            });
                            compilation.assets[fileName] = replaceSource;
                        }
                    });
                });
                callback();
            });
        });
    }
}
exports.PurifyPlugin = PurifyPlugin;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoid2VicGFjay1wbHVnaW4uanMiLCJzb3VyY2VSb290IjoiLi8iLCJzb3VyY2VzIjpbInBhY2thZ2VzL2FuZ3VsYXJfZGV2a2l0L2J1aWxkX29wdGltaXplci9zcmMvcHVyaWZ5L3dlYnBhY2stcGx1Z2luLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7O0FBU0EscURBQWdEO0FBQ2hELHFDQUE4QztBQU85QztJQUNFLGdCQUFnQixDQUFDO0lBQ1YsS0FBSyxDQUFDLFFBQWtCO1FBQzdCLFFBQVEsQ0FBQyxNQUFNLENBQUMsYUFBYSxFQUFFLENBQUMsV0FBb0MsRUFBRSxFQUFFO1lBQ3RFLDhFQUE4RTtZQUM5RSxXQUFXLENBQUMsUUFBUSxDQUFDLElBQUksQ0FBQywwREFBMEQsQ0FBQyxDQUFDO1lBRXRGLFdBQVcsQ0FBQyxNQUFNLENBQUMsdUJBQXVCLEVBQUUsQ0FBQyxNQUFlLEVBQUUsUUFBb0IsRUFBRSxFQUFFO2dCQUNwRixNQUFNLENBQUMsT0FBTyxDQUFDLENBQUMsS0FBWSxFQUFFLEVBQUU7b0JBQzlCLEtBQUssQ0FBQyxLQUFLO3lCQUNSLE1BQU0sQ0FBQyxDQUFDLFFBQWdCLEVBQUUsRUFBRSxDQUFDLFFBQVEsQ0FBQyxRQUFRLENBQUMsS0FBSyxDQUFDLENBQUM7eUJBQ3RELE9BQU8sQ0FBQyxDQUFDLFFBQWdCLEVBQUUsRUFBRTt3QkFDNUIsTUFBTSxPQUFPLEdBQUcsMkJBQWtCLENBQUMsV0FBVyxDQUFDLE1BQU0sQ0FBQyxRQUFRLENBQUMsQ0FBQyxNQUFNLEVBQUUsQ0FBQyxDQUFDO3dCQUUxRSxJQUFJLE9BQU8sQ0FBQyxNQUFNLEdBQUcsQ0FBQyxFQUFFOzRCQUN0QixNQUFNLGFBQWEsR0FBRyxJQUFJLCtCQUFhLENBQUMsV0FBVyxDQUFDLE1BQU0sQ0FBQyxRQUFRLENBQUMsRUFBRSxRQUFRLENBQUMsQ0FBQzs0QkFDaEYsT0FBTyxDQUFDLE9BQU8sQ0FBQyxDQUFDLE1BQU0sRUFBRSxFQUFFO2dDQUN6QixhQUFhLENBQUMsTUFBTSxDQUFDLE1BQU0sQ0FBQyxHQUFHLEVBQUUsTUFBTSxDQUFDLE9BQU8sQ0FBQyxDQUFDOzRCQUNuRCxDQUFDLENBQUMsQ0FBQzs0QkFDSCxXQUFXLENBQUMsTUFBTSxDQUFDLFFBQVEsQ0FBQyxHQUFHLGFBQWEsQ0FBQzt5QkFDOUM7b0JBQ0gsQ0FBQyxDQUFDLENBQUM7Z0JBQ1AsQ0FBQyxDQUFDLENBQUM7Z0JBQ0gsUUFBUSxFQUFFLENBQUM7WUFDYixDQUFDLENBQUMsQ0FBQztRQUNMLENBQUMsQ0FBQyxDQUFDO0lBQ0wsQ0FBQztDQUNGO0FBM0JELG9DQTJCQyIsInNvdXJjZXNDb250ZW50IjpbIi8qKlxuICogQGxpY2Vuc2VcbiAqIENvcHlyaWdodCBHb29nbGUgSW5jLiBBbGwgUmlnaHRzIFJlc2VydmVkLlxuICpcbiAqIFVzZSBvZiB0aGlzIHNvdXJjZSBjb2RlIGlzIGdvdmVybmVkIGJ5IGFuIE1JVC1zdHlsZSBsaWNlbnNlIHRoYXQgY2FuIGJlXG4gKiBmb3VuZCBpbiB0aGUgTElDRU5TRSBmaWxlIGF0IGh0dHBzOi8vYW5ndWxhci5pby9saWNlbnNlXG4gKi9cbi8vIHRzbGludDpkaXNhYmxlLW5leHQtbGluZTpuby1pbXBsaWNpdC1kZXBlbmRlbmNpZXNcbmltcG9ydCB7IENvbXBpbGVyLCBjb21waWxhdGlvbiB9IGZyb20gJ3dlYnBhY2snO1xuaW1wb3J0IHsgUmVwbGFjZVNvdXJjZSB9IGZyb20gJ3dlYnBhY2stc291cmNlcyc7XG5pbXBvcnQgeyBwdXJpZnlSZXBsYWNlbWVudHMgfSBmcm9tICcuL3B1cmlmeSc7XG5cblxuaW50ZXJmYWNlIENodW5rIHtcbiAgZmlsZXM6IHN0cmluZ1tdO1xufVxuXG5leHBvcnQgY2xhc3MgUHVyaWZ5UGx1Z2luIHtcbiAgY29uc3RydWN0b3IoKSB7IH1cbiAgcHVibGljIGFwcGx5KGNvbXBpbGVyOiBDb21waWxlcik6IHZvaWQge1xuICAgIGNvbXBpbGVyLnBsdWdpbignY29tcGlsYXRpb24nLCAoY29tcGlsYXRpb246IGNvbXBpbGF0aW9uLkNvbXBpbGF0aW9uKSA9PiB7XG4gICAgICAvLyBXZWJwYWNrIDQgcHJvdmlkZXMgdGhlIHNhbWUgZnVuY3Rpb25hbGl0eSBhcyB0aGlzIHBsdWdpbiBhbmQgVFMgdHJhbnNmb3JtZXJcbiAgICAgIGNvbXBpbGF0aW9uLndhcm5pbmdzLnB1c2goJ1B1cmlmeVBsdWdpbiBpcyBkZXByZWNhdGVkIGFuZCB3aWxsIGJlIHJlbW92ZWQgaW4gMC43LjAuJyk7XG5cbiAgICAgIGNvbXBpbGF0aW9uLnBsdWdpbignb3B0aW1pemUtY2h1bmstYXNzZXRzJywgKGNodW5rczogQ2h1bmtbXSwgY2FsbGJhY2s6ICgpID0+IHZvaWQpID0+IHtcbiAgICAgICAgY2h1bmtzLmZvckVhY2goKGNodW5rOiBDaHVuaykgPT4ge1xuICAgICAgICAgIGNodW5rLmZpbGVzXG4gICAgICAgICAgICAuZmlsdGVyKChmaWxlTmFtZTogc3RyaW5nKSA9PiBmaWxlTmFtZS5lbmRzV2l0aCgnLmpzJykpXG4gICAgICAgICAgICAuZm9yRWFjaCgoZmlsZU5hbWU6IHN0cmluZykgPT4ge1xuICAgICAgICAgICAgICBjb25zdCBpbnNlcnRzID0gcHVyaWZ5UmVwbGFjZW1lbnRzKGNvbXBpbGF0aW9uLmFzc2V0c1tmaWxlTmFtZV0uc291cmNlKCkpO1xuXG4gICAgICAgICAgICAgIGlmIChpbnNlcnRzLmxlbmd0aCA+IDApIHtcbiAgICAgICAgICAgICAgICBjb25zdCByZXBsYWNlU291cmNlID0gbmV3IFJlcGxhY2VTb3VyY2UoY29tcGlsYXRpb24uYXNzZXRzW2ZpbGVOYW1lXSwgZmlsZU5hbWUpO1xuICAgICAgICAgICAgICAgIGluc2VydHMuZm9yRWFjaCgoaW5zZXJ0KSA9PiB7XG4gICAgICAgICAgICAgICAgICByZXBsYWNlU291cmNlLmluc2VydChpbnNlcnQucG9zLCBpbnNlcnQuY29udGVudCk7XG4gICAgICAgICAgICAgICAgfSk7XG4gICAgICAgICAgICAgICAgY29tcGlsYXRpb24uYXNzZXRzW2ZpbGVOYW1lXSA9IHJlcGxhY2VTb3VyY2U7XG4gICAgICAgICAgICAgIH1cbiAgICAgICAgICAgIH0pO1xuICAgICAgICB9KTtcbiAgICAgICAgY2FsbGJhY2soKTtcbiAgICAgIH0pO1xuICAgIH0pO1xuICB9XG59XG4iXX0=