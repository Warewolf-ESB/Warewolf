"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
class RemoveHashPlugin {
    constructor(options) {
        this.options = options;
    }
    apply(compiler) {
        compiler.hooks.compilation.tap('remove-hash-plugin', compilation => {
            const mainTemplate = compilation.mainTemplate;
            mainTemplate.hooks.assetPath.tap('remove-hash-plugin', (path, data) => {
                const chunkId = data.chunk && data.chunk.id;
                if (chunkId && this.options.chunkIds.includes(chunkId)) {
                    // Replace hash formats with empty strings.
                    return path
                        .replace(this.options.hashFormat.chunk, '')
                        .replace(this.options.hashFormat.extract, '');
                }
                return path;
            });
        });
    }
}
exports.RemoveHashPlugin = RemoveHashPlugin;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoicmVtb3ZlLWhhc2gtcGx1Z2luLmpzIiwic291cmNlUm9vdCI6Ii4vIiwic291cmNlcyI6WyJwYWNrYWdlcy9hbmd1bGFyX2RldmtpdC9idWlsZF9hbmd1bGFyL3NyYy9hbmd1bGFyLWNsaS1maWxlcy9wbHVnaW5zL3JlbW92ZS1oYXNoLXBsdWdpbi50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiOztBQWdCQTtJQUVFLFlBQW9CLE9BQWdDO1FBQWhDLFlBQU8sR0FBUCxPQUFPLENBQXlCO0lBQUksQ0FBQztJQUV6RCxLQUFLLENBQUMsUUFBa0I7UUFDdEIsUUFBUSxDQUFDLEtBQUssQ0FBQyxXQUFXLENBQUMsR0FBRyxDQUFDLG9CQUFvQixFQUFFLFdBQVcsQ0FBQyxFQUFFO1lBQ2pFLE1BQU0sWUFBWSxHQUFHLFdBQVcsQ0FBQyxZQUVoQyxDQUFDO1lBRUYsWUFBWSxDQUFDLEtBQUssQ0FBQyxTQUFTLENBQUMsR0FBRyxDQUFDLG9CQUFvQixFQUNuRCxDQUFDLElBQVksRUFBRSxJQUFnQyxFQUFFLEVBQUU7Z0JBQ2pELE1BQU0sT0FBTyxHQUFHLElBQUksQ0FBQyxLQUFLLElBQUksSUFBSSxDQUFDLEtBQUssQ0FBQyxFQUFFLENBQUM7Z0JBRTVDLElBQUksT0FBTyxJQUFJLElBQUksQ0FBQyxPQUFPLENBQUMsUUFBUSxDQUFDLFFBQVEsQ0FBQyxPQUFPLENBQUMsRUFBRTtvQkFDdEQsMkNBQTJDO29CQUMzQyxPQUFPLElBQUk7eUJBQ1IsT0FBTyxDQUFDLElBQUksQ0FBQyxPQUFPLENBQUMsVUFBVSxDQUFDLEtBQUssRUFBRSxFQUFFLENBQUM7eUJBQzFDLE9BQU8sQ0FBQyxJQUFJLENBQUMsT0FBTyxDQUFDLFVBQVUsQ0FBQyxPQUFPLEVBQUUsRUFBRSxDQUFDLENBQUM7aUJBQ2pEO2dCQUVELE9BQU8sSUFBSSxDQUFDO1lBQ2QsQ0FBQyxDQUNGLENBQUM7UUFDSixDQUFDLENBQUMsQ0FBQztJQUNMLENBQUM7Q0FDRjtBQTFCRCw0Q0EwQkMiLCJzb3VyY2VzQ29udGVudCI6WyIvKipcbiAqIEBsaWNlbnNlXG4gKiBDb3B5cmlnaHQgR29vZ2xlIEluYy4gQWxsIFJpZ2h0cyBSZXNlcnZlZC5cbiAqXG4gKiBVc2Ugb2YgdGhpcyBzb3VyY2UgY29kZSBpcyBnb3Zlcm5lZCBieSBhbiBNSVQtc3R5bGUgbGljZW5zZSB0aGF0IGNhbiBiZVxuICogZm91bmQgaW4gdGhlIExJQ0VOU0UgZmlsZSBhdCBodHRwczovL2FuZ3VsYXIuaW8vbGljZW5zZVxuICovXG5pbXBvcnQgeyBDb21waWxlciwgY29tcGlsYXRpb24gfSBmcm9tICd3ZWJwYWNrJztcbmltcG9ydCB7IEhhc2hGb3JtYXQgfSBmcm9tICcuLi9tb2RlbHMvd2VicGFjay1jb25maWdzL3V0aWxzJztcblxuXG5leHBvcnQgaW50ZXJmYWNlIFJlbW92ZUhhc2hQbHVnaW5PcHRpb25zIHtcbiAgY2h1bmtJZHM6IHN0cmluZ1tdO1xuICBoYXNoRm9ybWF0OiBIYXNoRm9ybWF0O1xufVxuXG5leHBvcnQgY2xhc3MgUmVtb3ZlSGFzaFBsdWdpbiB7XG5cbiAgY29uc3RydWN0b3IocHJpdmF0ZSBvcHRpb25zOiBSZW1vdmVIYXNoUGx1Z2luT3B0aW9ucykgeyB9XG5cbiAgYXBwbHkoY29tcGlsZXI6IENvbXBpbGVyKTogdm9pZCB7XG4gICAgY29tcGlsZXIuaG9va3MuY29tcGlsYXRpb24udGFwKCdyZW1vdmUtaGFzaC1wbHVnaW4nLCBjb21waWxhdGlvbiA9PiB7XG4gICAgICBjb25zdCBtYWluVGVtcGxhdGUgPSBjb21waWxhdGlvbi5tYWluVGVtcGxhdGUgYXMgY29tcGlsYXRpb24uTWFpblRlbXBsYXRlICYge1xuICAgICAgICBob29rczogY29tcGlsYXRpb24uQ29tcGlsYXRpb25Ib29rcztcbiAgICAgIH07XG5cbiAgICAgIG1haW5UZW1wbGF0ZS5ob29rcy5hc3NldFBhdGgudGFwKCdyZW1vdmUtaGFzaC1wbHVnaW4nLFxuICAgICAgICAocGF0aDogc3RyaW5nLCBkYXRhOiB7IGNodW5rPzogeyBpZDogc3RyaW5nIH0gfSkgPT4ge1xuICAgICAgICAgIGNvbnN0IGNodW5rSWQgPSBkYXRhLmNodW5rICYmIGRhdGEuY2h1bmsuaWQ7XG5cbiAgICAgICAgICBpZiAoY2h1bmtJZCAmJiB0aGlzLm9wdGlvbnMuY2h1bmtJZHMuaW5jbHVkZXMoY2h1bmtJZCkpIHtcbiAgICAgICAgICAgIC8vIFJlcGxhY2UgaGFzaCBmb3JtYXRzIHdpdGggZW1wdHkgc3RyaW5ncy5cbiAgICAgICAgICAgIHJldHVybiBwYXRoXG4gICAgICAgICAgICAgIC5yZXBsYWNlKHRoaXMub3B0aW9ucy5oYXNoRm9ybWF0LmNodW5rLCAnJylcbiAgICAgICAgICAgICAgLnJlcGxhY2UodGhpcy5vcHRpb25zLmhhc2hGb3JtYXQuZXh0cmFjdCwgJycpO1xuICAgICAgICAgIH1cblxuICAgICAgICAgIHJldHVybiBwYXRoO1xuICAgICAgICB9LFxuICAgICAgKTtcbiAgICB9KTtcbiAgfVxufVxuIl19