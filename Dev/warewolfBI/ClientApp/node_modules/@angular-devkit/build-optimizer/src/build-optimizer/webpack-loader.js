"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
const source_map_1 = require("source-map");
const loaderUtils = require('loader-utils');
const build_optimizer_1 = require("./build-optimizer");
function buildOptimizerLoader(content, previousSourceMap) {
    this.cacheable();
    const options = loaderUtils.getOptions(this) || {};
    // Make up names of the intermediate files so we can chain the sourcemaps.
    const inputFilePath = this.resourcePath + '.pre-build-optimizer.js';
    const outputFilePath = this.resourcePath + '.post-build-optimizer.js';
    const boOutput = build_optimizer_1.buildOptimizer({
        content,
        originalFilePath: this.resourcePath,
        inputFilePath,
        outputFilePath,
        emitSourceMap: options.sourceMap,
        isSideEffectFree: this._module
            && this._module.factoryMeta
            && this._module.factoryMeta.sideEffectFree,
    });
    if (boOutput.emitSkipped || boOutput.content === null) {
        // Webpack typings for previousSourceMap are wrong, they are JSON objects and not strings.
        // tslint:disable-next-line:no-any
        this.callback(null, content, previousSourceMap);
        return;
    }
    const intermediateSourceMap = boOutput.sourceMap;
    let newContent = boOutput.content;
    let newSourceMap;
    if (options.sourceMap && intermediateSourceMap) {
        // Webpack doesn't need sourceMappingURL since we pass them on explicitely.
        newContent = newContent.replace(/^\/\/# sourceMappingURL=[^\r\n]*/gm, '');
        if (previousSourceMap) {
            // If there's a previous sourcemap, we have to chain them.
            // See https://github.com/mozilla/source-map/issues/216#issuecomment-150839869 for a simple
            // source map chaining example.
            // Use http://sokra.github.io/source-map-visualization/ to validate sourcemaps make sense.
            // Force the previous sourcemap to use the filename we made up for it.
            // In order for source maps to be chained, the consumed source map `file` needs to be in the
            // consumers source map `sources` array.
            previousSourceMap.file = inputFilePath;
            // Chain the sourcemaps.
            const consumer = new source_map_1.SourceMapConsumer(intermediateSourceMap);
            const generator = source_map_1.SourceMapGenerator.fromSourceMap(consumer);
            generator.applySourceMap(new source_map_1.SourceMapConsumer(previousSourceMap));
            newSourceMap = generator.toJSON();
        }
        else {
            // Otherwise just return our generated sourcemap.
            newSourceMap = intermediateSourceMap;
        }
    }
    // Webpack typings for previousSourceMap are wrong, they are JSON objects and not strings.
    // tslint:disable-next-line:no-any
    this.callback(null, newContent, newSourceMap);
}
exports.default = buildOptimizerLoader;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoid2VicGFjay1sb2FkZXIuanMiLCJzb3VyY2VSb290IjoiLi8iLCJzb3VyY2VzIjpbInBhY2thZ2VzL2FuZ3VsYXJfZGV2a2l0L2J1aWxkX29wdGltaXplci9zcmMvYnVpbGQtb3B0aW1pemVyL3dlYnBhY2stbG9hZGVyLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7O0FBQUE7Ozs7OztHQU1HO0FBQ0gsMkNBQWlGO0FBR2pGLE1BQU0sV0FBVyxHQUFHLE9BQU8sQ0FBQyxjQUFjLENBQUMsQ0FBQztBQUU1Qyx1REFBbUQ7QUFPbkQsOEJBQ3VDLE9BQWUsRUFBRSxpQkFBK0I7SUFDckYsSUFBSSxDQUFDLFNBQVMsRUFBRSxDQUFDO0lBQ2pCLE1BQU0sT0FBTyxHQUFnQyxXQUFXLENBQUMsVUFBVSxDQUFDLElBQUksQ0FBQyxJQUFJLEVBQUUsQ0FBQztJQUVoRiwwRUFBMEU7SUFDMUUsTUFBTSxhQUFhLEdBQUcsSUFBSSxDQUFDLFlBQVksR0FBRyx5QkFBeUIsQ0FBQztJQUNwRSxNQUFNLGNBQWMsR0FBRyxJQUFJLENBQUMsWUFBWSxHQUFHLDBCQUEwQixDQUFDO0lBRXRFLE1BQU0sUUFBUSxHQUFHLGdDQUFjLENBQUM7UUFDOUIsT0FBTztRQUNQLGdCQUFnQixFQUFFLElBQUksQ0FBQyxZQUFZO1FBQ25DLGFBQWE7UUFDYixjQUFjO1FBQ2QsYUFBYSxFQUFFLE9BQU8sQ0FBQyxTQUFTO1FBQ2hDLGdCQUFnQixFQUFFLElBQUksQ0FBQyxPQUFPO2VBQ1QsSUFBSSxDQUFDLE9BQU8sQ0FBQyxXQUFXO2VBQ3hCLElBQUksQ0FBQyxPQUFPLENBQUMsV0FBVyxDQUFDLGNBQWM7S0FDN0QsQ0FBQyxDQUFDO0lBRUgsSUFBSSxRQUFRLENBQUMsV0FBVyxJQUFJLFFBQVEsQ0FBQyxPQUFPLEtBQUssSUFBSSxFQUFFO1FBQ3JELDBGQUEwRjtRQUMxRixrQ0FBa0M7UUFDbEMsSUFBSSxDQUFDLFFBQVEsQ0FBQyxJQUFJLEVBQUUsT0FBTyxFQUFFLGlCQUF3QixDQUFDLENBQUM7UUFFdkQsT0FBTztLQUNSO0lBRUQsTUFBTSxxQkFBcUIsR0FBRyxRQUFRLENBQUMsU0FBUyxDQUFDO0lBQ2pELElBQUksVUFBVSxHQUFHLFFBQVEsQ0FBQyxPQUFPLENBQUM7SUFFbEMsSUFBSSxZQUFZLENBQUM7SUFFakIsSUFBSSxPQUFPLENBQUMsU0FBUyxJQUFJLHFCQUFxQixFQUFFO1FBQzlDLDJFQUEyRTtRQUMzRSxVQUFVLEdBQUcsVUFBVSxDQUFDLE9BQU8sQ0FBQyxvQ0FBb0MsRUFBRSxFQUFFLENBQUMsQ0FBQztRQUUxRSxJQUFJLGlCQUFpQixFQUFFO1lBQ3JCLDBEQUEwRDtZQUMxRCwyRkFBMkY7WUFDM0YsK0JBQStCO1lBQy9CLDBGQUEwRjtZQUUxRixzRUFBc0U7WUFDdEUsNEZBQTRGO1lBQzVGLHdDQUF3QztZQUN4QyxpQkFBaUIsQ0FBQyxJQUFJLEdBQUcsYUFBYSxDQUFDO1lBRXZDLHdCQUF3QjtZQUN4QixNQUFNLFFBQVEsR0FBRyxJQUFJLDhCQUFpQixDQUFDLHFCQUFxQixDQUFDLENBQUM7WUFDOUQsTUFBTSxTQUFTLEdBQUcsK0JBQWtCLENBQUMsYUFBYSxDQUFDLFFBQVEsQ0FBQyxDQUFDO1lBQzdELFNBQVMsQ0FBQyxjQUFjLENBQUMsSUFBSSw4QkFBaUIsQ0FBQyxpQkFBaUIsQ0FBQyxDQUFDLENBQUM7WUFDbkUsWUFBWSxHQUFHLFNBQVMsQ0FBQyxNQUFNLEVBQUUsQ0FBQztTQUNuQzthQUFNO1lBQ0wsaURBQWlEO1lBQ2pELFlBQVksR0FBRyxxQkFBcUIsQ0FBQztTQUN0QztLQUNGO0lBRUQsMEZBQTBGO0lBQzFGLGtDQUFrQztJQUNsQyxJQUFJLENBQUMsUUFBUSxDQUFDLElBQUksRUFBRSxVQUFVLEVBQUUsWUFBbUIsQ0FBQyxDQUFDO0FBQ3ZELENBQUM7QUE5REQsdUNBOERDIiwic291cmNlc0NvbnRlbnQiOlsiLyoqXG4gKiBAbGljZW5zZVxuICogQ29weXJpZ2h0IEdvb2dsZSBJbmMuIEFsbCBSaWdodHMgUmVzZXJ2ZWQuXG4gKlxuICogVXNlIG9mIHRoaXMgc291cmNlIGNvZGUgaXMgZ292ZXJuZWQgYnkgYW4gTUlULXN0eWxlIGxpY2Vuc2UgdGhhdCBjYW4gYmVcbiAqIGZvdW5kIGluIHRoZSBMSUNFTlNFIGZpbGUgYXQgaHR0cHM6Ly9hbmd1bGFyLmlvL2xpY2Vuc2VcbiAqL1xuaW1wb3J0IHsgUmF3U291cmNlTWFwLCBTb3VyY2VNYXBDb25zdW1lciwgU291cmNlTWFwR2VuZXJhdG9yIH0gZnJvbSAnc291cmNlLW1hcCc7XG5pbXBvcnQgKiBhcyB3ZWJwYWNrIGZyb20gJ3dlYnBhY2snOyAgLy8gdHNsaW50OmRpc2FibGUtbGluZTpuby1pbXBsaWNpdC1kZXBlbmRlbmNpZXNcblxuY29uc3QgbG9hZGVyVXRpbHMgPSByZXF1aXJlKCdsb2FkZXItdXRpbHMnKTtcblxuaW1wb3J0IHsgYnVpbGRPcHRpbWl6ZXIgfSBmcm9tICcuL2J1aWxkLW9wdGltaXplcic7XG5cblxuaW50ZXJmYWNlIEJ1aWxkT3B0aW1pemVyTG9hZGVyT3B0aW9ucyB7XG4gIHNvdXJjZU1hcDogYm9vbGVhbjtcbn1cblxuZXhwb3J0IGRlZmF1bHQgZnVuY3Rpb24gYnVpbGRPcHRpbWl6ZXJMb2FkZXJcbiAgKHRoaXM6IHdlYnBhY2subG9hZGVyLkxvYWRlckNvbnRleHQsIGNvbnRlbnQ6IHN0cmluZywgcHJldmlvdXNTb3VyY2VNYXA6IFJhd1NvdXJjZU1hcCkge1xuICB0aGlzLmNhY2hlYWJsZSgpO1xuICBjb25zdCBvcHRpb25zOiBCdWlsZE9wdGltaXplckxvYWRlck9wdGlvbnMgPSBsb2FkZXJVdGlscy5nZXRPcHRpb25zKHRoaXMpIHx8IHt9O1xuXG4gIC8vIE1ha2UgdXAgbmFtZXMgb2YgdGhlIGludGVybWVkaWF0ZSBmaWxlcyBzbyB3ZSBjYW4gY2hhaW4gdGhlIHNvdXJjZW1hcHMuXG4gIGNvbnN0IGlucHV0RmlsZVBhdGggPSB0aGlzLnJlc291cmNlUGF0aCArICcucHJlLWJ1aWxkLW9wdGltaXplci5qcyc7XG4gIGNvbnN0IG91dHB1dEZpbGVQYXRoID0gdGhpcy5yZXNvdXJjZVBhdGggKyAnLnBvc3QtYnVpbGQtb3B0aW1pemVyLmpzJztcblxuICBjb25zdCBib091dHB1dCA9IGJ1aWxkT3B0aW1pemVyKHtcbiAgICBjb250ZW50LFxuICAgIG9yaWdpbmFsRmlsZVBhdGg6IHRoaXMucmVzb3VyY2VQYXRoLFxuICAgIGlucHV0RmlsZVBhdGgsXG4gICAgb3V0cHV0RmlsZVBhdGgsXG4gICAgZW1pdFNvdXJjZU1hcDogb3B0aW9ucy5zb3VyY2VNYXAsXG4gICAgaXNTaWRlRWZmZWN0RnJlZTogdGhpcy5fbW9kdWxlXG4gICAgICAgICAgICAgICAgICAgICAgJiYgdGhpcy5fbW9kdWxlLmZhY3RvcnlNZXRhXG4gICAgICAgICAgICAgICAgICAgICAgJiYgdGhpcy5fbW9kdWxlLmZhY3RvcnlNZXRhLnNpZGVFZmZlY3RGcmVlLFxuICB9KTtcblxuICBpZiAoYm9PdXRwdXQuZW1pdFNraXBwZWQgfHwgYm9PdXRwdXQuY29udGVudCA9PT0gbnVsbCkge1xuICAgIC8vIFdlYnBhY2sgdHlwaW5ncyBmb3IgcHJldmlvdXNTb3VyY2VNYXAgYXJlIHdyb25nLCB0aGV5IGFyZSBKU09OIG9iamVjdHMgYW5kIG5vdCBzdHJpbmdzLlxuICAgIC8vIHRzbGludDpkaXNhYmxlLW5leHQtbGluZTpuby1hbnlcbiAgICB0aGlzLmNhbGxiYWNrKG51bGwsIGNvbnRlbnQsIHByZXZpb3VzU291cmNlTWFwIGFzIGFueSk7XG5cbiAgICByZXR1cm47XG4gIH1cblxuICBjb25zdCBpbnRlcm1lZGlhdGVTb3VyY2VNYXAgPSBib091dHB1dC5zb3VyY2VNYXA7XG4gIGxldCBuZXdDb250ZW50ID0gYm9PdXRwdXQuY29udGVudDtcblxuICBsZXQgbmV3U291cmNlTWFwO1xuXG4gIGlmIChvcHRpb25zLnNvdXJjZU1hcCAmJiBpbnRlcm1lZGlhdGVTb3VyY2VNYXApIHtcbiAgICAvLyBXZWJwYWNrIGRvZXNuJ3QgbmVlZCBzb3VyY2VNYXBwaW5nVVJMIHNpbmNlIHdlIHBhc3MgdGhlbSBvbiBleHBsaWNpdGVseS5cbiAgICBuZXdDb250ZW50ID0gbmV3Q29udGVudC5yZXBsYWNlKC9eXFwvXFwvIyBzb3VyY2VNYXBwaW5nVVJMPVteXFxyXFxuXSovZ20sICcnKTtcblxuICAgIGlmIChwcmV2aW91c1NvdXJjZU1hcCkge1xuICAgICAgLy8gSWYgdGhlcmUncyBhIHByZXZpb3VzIHNvdXJjZW1hcCwgd2UgaGF2ZSB0byBjaGFpbiB0aGVtLlxuICAgICAgLy8gU2VlIGh0dHBzOi8vZ2l0aHViLmNvbS9tb3ppbGxhL3NvdXJjZS1tYXAvaXNzdWVzLzIxNiNpc3N1ZWNvbW1lbnQtMTUwODM5ODY5IGZvciBhIHNpbXBsZVxuICAgICAgLy8gc291cmNlIG1hcCBjaGFpbmluZyBleGFtcGxlLlxuICAgICAgLy8gVXNlIGh0dHA6Ly9zb2tyYS5naXRodWIuaW8vc291cmNlLW1hcC12aXN1YWxpemF0aW9uLyB0byB2YWxpZGF0ZSBzb3VyY2VtYXBzIG1ha2Ugc2Vuc2UuXG5cbiAgICAgIC8vIEZvcmNlIHRoZSBwcmV2aW91cyBzb3VyY2VtYXAgdG8gdXNlIHRoZSBmaWxlbmFtZSB3ZSBtYWRlIHVwIGZvciBpdC5cbiAgICAgIC8vIEluIG9yZGVyIGZvciBzb3VyY2UgbWFwcyB0byBiZSBjaGFpbmVkLCB0aGUgY29uc3VtZWQgc291cmNlIG1hcCBgZmlsZWAgbmVlZHMgdG8gYmUgaW4gdGhlXG4gICAgICAvLyBjb25zdW1lcnMgc291cmNlIG1hcCBgc291cmNlc2AgYXJyYXkuXG4gICAgICBwcmV2aW91c1NvdXJjZU1hcC5maWxlID0gaW5wdXRGaWxlUGF0aDtcblxuICAgICAgLy8gQ2hhaW4gdGhlIHNvdXJjZW1hcHMuXG4gICAgICBjb25zdCBjb25zdW1lciA9IG5ldyBTb3VyY2VNYXBDb25zdW1lcihpbnRlcm1lZGlhdGVTb3VyY2VNYXApO1xuICAgICAgY29uc3QgZ2VuZXJhdG9yID0gU291cmNlTWFwR2VuZXJhdG9yLmZyb21Tb3VyY2VNYXAoY29uc3VtZXIpO1xuICAgICAgZ2VuZXJhdG9yLmFwcGx5U291cmNlTWFwKG5ldyBTb3VyY2VNYXBDb25zdW1lcihwcmV2aW91c1NvdXJjZU1hcCkpO1xuICAgICAgbmV3U291cmNlTWFwID0gZ2VuZXJhdG9yLnRvSlNPTigpO1xuICAgIH0gZWxzZSB7XG4gICAgICAvLyBPdGhlcndpc2UganVzdCByZXR1cm4gb3VyIGdlbmVyYXRlZCBzb3VyY2VtYXAuXG4gICAgICBuZXdTb3VyY2VNYXAgPSBpbnRlcm1lZGlhdGVTb3VyY2VNYXA7XG4gICAgfVxuICB9XG5cbiAgLy8gV2VicGFjayB0eXBpbmdzIGZvciBwcmV2aW91c1NvdXJjZU1hcCBhcmUgd3JvbmcsIHRoZXkgYXJlIEpTT04gb2JqZWN0cyBhbmQgbm90IHN0cmluZ3MuXG4gIC8vIHRzbGludDpkaXNhYmxlLW5leHQtbGluZTpuby1hbnlcbiAgdGhpcy5jYWxsYmFjayhudWxsLCBuZXdDb250ZW50LCBuZXdTb3VyY2VNYXAgYXMgYW55KTtcbn1cbiJdfQ==