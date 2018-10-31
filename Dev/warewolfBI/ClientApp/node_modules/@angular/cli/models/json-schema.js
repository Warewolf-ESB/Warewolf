"use strict";
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
const core_1 = require("@angular-devkit/core");
const jsonSchemaTraverse = require("json-schema-traverse");
function convertSchemaToOptions(schema) {
    return __awaiter(this, void 0, void 0, function* () {
        const options = yield getOptions(schema);
        return options;
    });
}
exports.convertSchemaToOptions = convertSchemaToOptions;
function getOptions(schemaText, onlyRootProperties = true) {
    // TODO: Use devkit core's visitJsonSchema
    return new Promise((resolve) => {
        const fullSchema = core_1.parseJson(schemaText);
        if (!core_1.isJsonObject(fullSchema)) {
            return Promise.resolve([]);
        }
        const traverseOptions = {};
        const options = [];
        function postCallback(schema, jsonPointer, _rootSchema, _parentJsonPointer, parentKeyword, _parentSchema, property) {
            if (parentKeyword === 'properties') {
                let includeOption = true;
                if (onlyRootProperties && isPropertyNested(jsonPointer)) {
                    includeOption = false;
                }
                const description = typeof schema.description == 'string' ? schema.description : '';
                const type = typeof schema.type == 'string' ? schema.type : '';
                let defaultValue = undefined;
                if (schema.default !== null) {
                    if (typeof schema.default !== 'object') {
                        defaultValue = schema.default;
                    }
                }
                let $default = undefined;
                if (schema.$default !== null && core_1.isJsonObject(schema.$default)) {
                    $default = schema.$default;
                }
                let required = false;
                if (typeof schema.required === 'boolean') {
                    required = schema.required;
                }
                let aliases = undefined;
                if (typeof schema.aliases === 'object' && Array.isArray(schema.aliases)) {
                    aliases = schema.aliases;
                }
                let format = undefined;
                if (typeof schema.format === 'string') {
                    format = schema.format;
                }
                let hidden = false;
                if (typeof schema.hidden === 'boolean') {
                    hidden = schema.hidden;
                }
                const option = {
                    name: property,
                    // ...schema,
                    description,
                    type,
                    default: defaultValue,
                    $default,
                    required,
                    aliases,
                    format,
                    hidden,
                };
                if (includeOption) {
                    options.push(option);
                }
            }
            else if (schema === fullSchema) {
                resolve(options);
            }
        }
        const callbacks = { post: postCallback };
        jsonSchemaTraverse(fullSchema, traverseOptions, callbacks);
    });
}
function isPropertyNested(jsonPath) {
    return jsonPath.split('/')
        .filter(part => part == 'properties' || part == 'items')
        .length > 1;
}
function parseSchema(schema) {
    const parsedSchema = core_1.parseJson(schema);
    if (parsedSchema === null || !core_1.isJsonObject(parsedSchema)) {
        return null;
    }
    return parsedSchema;
}
exports.parseSchema = parseSchema;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoianNvbi1zY2hlbWEuanMiLCJzb3VyY2VSb290IjoiLi8iLCJzb3VyY2VzIjpbInBhY2thZ2VzL2FuZ3VsYXIvY2xpL21vZGVscy9qc29uLXNjaGVtYS50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiOzs7Ozs7Ozs7O0FBQUE7Ozs7OztHQU1HO0FBQ0gsK0NBQTJFO0FBQzNFLDJEQUEyRDtBQUczRCxnQ0FBNkMsTUFBYzs7UUFDekQsTUFBTSxPQUFPLEdBQUcsTUFBTSxVQUFVLENBQUMsTUFBTSxDQUFDLENBQUM7UUFFekMsT0FBTyxPQUFPLENBQUM7SUFDakIsQ0FBQztDQUFBO0FBSkQsd0RBSUM7QUFFRCxvQkFBb0IsVUFBa0IsRUFBRSxrQkFBa0IsR0FBRyxJQUFJO0lBQy9ELDBDQUEwQztJQUMxQyxPQUFPLElBQUksT0FBTyxDQUFDLENBQUMsT0FBTyxFQUFFLEVBQUU7UUFDN0IsTUFBTSxVQUFVLEdBQUcsZ0JBQVMsQ0FBQyxVQUFVLENBQUMsQ0FBQztRQUN6QyxJQUFJLENBQUMsbUJBQVksQ0FBQyxVQUFVLENBQUMsRUFBRTtZQUM3QixPQUFPLE9BQU8sQ0FBQyxPQUFPLENBQUMsRUFBRSxDQUFDLENBQUM7U0FDNUI7UUFDRCxNQUFNLGVBQWUsR0FBRyxFQUFFLENBQUM7UUFDM0IsTUFBTSxPQUFPLEdBQWEsRUFBRSxDQUFDO1FBQzdCLHNCQUFzQixNQUFrQixFQUNsQixXQUFtQixFQUNuQixXQUFtQixFQUNuQixrQkFBMEIsRUFDMUIsYUFBcUIsRUFDckIsYUFBcUIsRUFDckIsUUFBZ0I7WUFDcEMsSUFBSSxhQUFhLEtBQUssWUFBWSxFQUFFO2dCQUNsQyxJQUFJLGFBQWEsR0FBRyxJQUFJLENBQUM7Z0JBQ3pCLElBQUksa0JBQWtCLElBQUksZ0JBQWdCLENBQUMsV0FBVyxDQUFDLEVBQUU7b0JBQ3ZELGFBQWEsR0FBRyxLQUFLLENBQUM7aUJBQ3ZCO2dCQUNELE1BQU0sV0FBVyxHQUFHLE9BQU8sTUFBTSxDQUFDLFdBQVcsSUFBSSxRQUFRLENBQUMs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LE9BQU8sWUFBWSxDQUFDO0FBQ3RCLENBQUM7QUFQRCxrQ0FPQyIsInNvdXJjZXNDb250ZW50IjpbIi8qKlxuICogQGxpY2Vuc2VcbiAqIENvcHlyaWdodCBHb29nbGUgSW5jLiBBbGwgUmlnaHRzIFJlc2VydmVkLlxuICpcbiAqIFVzZSBvZiB0aGlzIHNvdXJjZSBjb2RlIGlzIGdvdmVybmVkIGJ5IGFuIE1JVC1zdHlsZSBsaWNlbnNlIHRoYXQgY2FuIGJlXG4gKiBmb3VuZCBpbiB0aGUgTElDRU5TRSBmaWxlIGF0IGh0dHBzOi8vYW5ndWxhci5pby9saWNlbnNlXG4gKi9cbmltcG9ydCB7IEpzb25PYmplY3QsIGlzSnNvbk9iamVjdCwgcGFyc2VKc29uIH0gZnJvbSAnQGFuZ3VsYXItZGV2a2l0L2NvcmUnO1xuaW1wb3J0ICogYXMganNvblNjaGVtYVRyYXZlcnNlIGZyb20gJ2pzb24tc2NoZW1hLXRyYXZlcnNlJztcbmltcG9ydCB7IE9wdGlvbiwgT3B0aW9uU21hcnREZWZhdWx0IH0gZnJvbSAnLi9jb21tYW5kJztcblxuZXhwb3J0IGFzeW5jIGZ1bmN0aW9uIGNvbnZlcnRTY2hlbWFUb09wdGlvbnMoc2NoZW1hOiBzdHJpbmcpOiBQcm9taXNlPE9wdGlvbltdPiB7XG4gIGNvbnN0IG9wdGlvbnMgPSBhd2FpdCBnZXRPcHRpb25zKHNjaGVtYSk7XG5cbiAgcmV0dXJuIG9wdGlvbnM7XG59XG5cbmZ1bmN0aW9uIGdldE9wdGlvbnMoc2NoZW1hVGV4dDogc3RyaW5nLCBvbmx5Um9vdFByb3BlcnRpZXMgPSB0cnVlKTogUHJvbWlzZTxPcHRpb25bXT4ge1xuICAvLyBUT0RPOiBVc2UgZGV2a2l0IGNvcmUncyB2aXNpdEpzb25TY2hlbWFcbiAgcmV0dXJuIG5ldyBQcm9taXNlKChyZXNvbHZlKSA9PiB7XG4gICAgY29uc3QgZnVsbFNjaGVtYSA9IHBhcnNlSnNvbihzY2hlbWFUZXh0KTtcbiAgICBpZiAoIWlzSnNvbk9iamVjdChmdWxsU2NoZW1hKSkge1xuICAgICAgcmV0dXJuIFByb21pc2UucmVzb2x2ZShbXSk7XG4gICAgfVxuICAgIGNvbnN0IHRyYXZlcnNlT3B0aW9ucyA9IHt9O1xuICAgIGNvbnN0IG9wdGlvbnM6IE9wdGlvbltdID0gW107XG4gICAgZnVuY3Rpb24gcG9zdENhbGxiYWNrKHNjaGVtYTogSnNvbk9iamVjdCxcbiAgICAgICAgICAgICAgICAgICAgICAgICAganNvblBvaW50ZXI6IHN0cmluZyxcbiAgICAgICAgICAgICAgICAgICAgICAgICAgX3Jvb3RTY2hlbWE6IHN0cmluZyxcbiAgICAgICAgICAgICAgICAgICAgICAgICAgX3BhcmVudEpzb25Qb2ludGVyOiBzdHJpbmcsXG4gICAgICAgICAgICAgICAgICAgICAgICAgIHBhcmVudEtleXdvcmQ6IHN0cmluZyxcbiAgICAgICAgICAgICAgICAgICAgICAgICAgX3BhcmVudFNjaGVtYTogc3RyaW5nLFxuICAgICAgICAgICAgICAgICAgICAgICAgICBwcm9wZXJ0eTogc3RyaW5nKSB7XG4gICAgICBpZiAocGFyZW50S2V5d29yZCA9PT0gJ3Byb3BlcnRpZXMnKSB7XG4gICAgICAgIGxldCBpbmNsdWRlT3B0aW9uID0gdHJ1ZTtcbiAgICAgICAgaWYgKG9ubHlSb290UHJvcGVydGllcyAmJiBpc1Byb3BlcnR5TmVzdGVkKGpzb25Qb2ludGVyKSkge1xuICAgICAgICAgIGluY2x1ZGVPcHRpb24gPSBmYWxzZTtcbiAgICAgICAgfVxuICAgICAgICBjb25zdCBkZXNjcmlwdGlvbiA9IHR5cGVvZiBzY2hlbWEuZGVzY3JpcHRpb24gPT0gJ3N0cmluZycgPyBzY2hlbWEuZGVzY3JpcHRpb24gOiAnJztcbiAgICAgICAgY29uc3QgdHlwZSA9IHR5cGVvZiBzY2hlbWEudHlwZSA9PSAnc3RyaW5nJyA/IHNjaGVtYS50eXBlIDogJyc7XG4gICAgICAgIGxldCBkZWZhdWx0VmFsdWU6IHN0cmluZyB8IG51bWJlciB8IGJvb2xlYW4gfCB1bmRlZmluZWQgPSB1bmRlZmluZWQ7XG4gICAgICAgIGlmIChzY2hlbWEuZGVmYXVsdCAhPT0gbnVsbCkge1xuICAgICAgICAgIGlmICh0eXBlb2Ygc2NoZW1hLmRlZmF1bHQgIT09ICdvYmplY3QnKSB7XG4gICAgICAgICAgICBkZWZhdWx0VmFsdWUgPSBzY2hlbWEuZGVmYXVsdDtcbiAgICAgICAgICB9XG4gICAgICAgIH1cbiAgICAgICAgbGV0ICRkZWZhdWx0OiBPcHRpb25TbWFydERlZmF1bHQgfCB1bmRlZmluZWQgPSB1bmRlZmluZWQ7XG4gICAgICAgIGlmIChzY2hlbWEuJGRlZmF1bHQgIT09IG51bGwgJiYgaXNKc29uT2JqZWN0KHNjaGVtYS4kZGVmYXVsdCkpIHtcbiAgICAgICAgICAkZGVmYXVsdCA9IHNjaGVtYS4kZGVmYXVsdCBhcyBPcHRpb25TbWFydERlZmF1bHQ7XG4gICAgICAgIH1cbiAgICAgICAgbGV0IHJlcXVpcmVkID0gZmFsc2U7XG4gICAgICAgIGlmICh0eXBlb2Ygc2NoZW1hLnJlcXVpcmVkID09PSAnYm9vbGVhbicpIHtcbiAgICAgICAgICByZXF1aXJlZCA9IHNjaGVtYS5yZXF1aXJlZDtcbiAgICAgICAgfVxuICAgICAgICBsZXQgYWxpYXNlczogc3RyaW5nW10gfCB1bmRlZmluZWQgPSB1bmRlZmluZWQ7XG4gICAgICAgIGlmICh0eXBlb2Ygc2NoZW1hLmFsaWFzZXMgPT09ICdvYmplY3QnICYmIEFycmF5LmlzQXJyYXkoc2NoZW1hLmFsaWFzZXMpKSB7XG4gICAgICAgICAgYWxpYXNlcyA9IHNjaGVtYS5hbGlhc2VzIGFzIHN0cmluZ1tdO1xuICAgICAgICB9XG4gICAgICAgIGxldCBmb3JtYXQ6IHN0cmluZyB8IHVuZGVmaW5lZCA9IHVuZGVmaW5lZDtcbiAgICAgICAgaWYgKHR5cGVvZiBzY2hlbWEuZm9ybWF0ID09PSAnc3RyaW5nJykge1xuICAgICAgICAgIGZvcm1hdCA9IHNjaGVtYS5mb3JtYXQ7XG4gICAgICAgIH1cbiAgICAgICAgbGV0IGhpZGRlbiA9IGZhbHNlO1xuICAgICAgICBpZiAodHlwZW9mIHNjaGVtYS5oaWRkZW4gPT09ICdib29sZWFuJykge1xuICAgICAgICAgIGhpZGRlbiA9IHNjaGVtYS5oaWRkZW47XG4gICAgICAgIH1cblxuICAgICAgICBjb25zdCBvcHRpb246IE9wdGlvbiA9IHtcbiAgICAgICAgICBuYW1lOiBwcm9wZXJ0eSxcbiAgICAgICAgICAvLyAuLi5zY2hlbWEsXG5cbiAgICAgICAgICBkZXNjcmlwdGlvbixcbiAgICAgICAgICB0eXBlLFxuICAgICAgICAgIGRlZmF1bHQ6IGRlZmF1bHRWYWx1ZSxcbiAgICAgICAgICAkZGVmYXVsdCxcbiAgICAgICAgICByZXF1aXJlZCxcbiAgICAgICAgICBhbGlhc2VzLFxuICAgICAgICAgIGZvcm1hdCxcbiAgICAgICAgICBoaWRkZW4sXG4gICAgICAgIH07XG5cbiAgICAgICAgaWYgKGluY2x1ZGVPcHRpb24pIHtcbiAgICAgICAgICBvcHRpb25zLnB1c2gob3B0aW9uKTtcbiAgICAgICAgfVxuICAgICAgfSBlbHNlIGlmIChzY2hlbWEgPT09IGZ1bGxTY2hlbWEpIHtcbiAgICAgICAgcmVzb2x2ZShvcHRpb25zKTtcbiAgICAgIH1cbiAgICB9XG5cbiAgICBjb25zdCBjYWxsYmFja3MgPSB7IHBvc3Q6IHBvc3RDYWxsYmFjayB9O1xuXG4gICAganNvblNjaGVtYVRyYXZlcnNlKGZ1bGxTY2hlbWEsIHRyYXZlcnNlT3B0aW9ucywgY2FsbGJhY2tzKTtcbiAgfSk7XG59XG5cbmZ1bmN0aW9uIGlzUHJvcGVydHlOZXN0ZWQoanNvblBhdGg6IHN0cmluZyk6IGJvb2xlYW4ge1xuICByZXR1cm4ganNvblBhdGguc3BsaXQoJy8nKVxuICAgIC5maWx0ZXIocGFydCA9PiBwYXJ0ID09ICdwcm9wZXJ0aWVzJyB8fCBwYXJ0ID09ICdpdGVtcycpXG4gICAgLmxlbmd0aCA+IDE7XG59XG5cbmV4cG9ydCBmdW5jdGlvbiBwYXJzZVNjaGVtYShzY2hlbWE6IHN0cmluZyk6IEpzb25PYmplY3QgfCBudWxsIHtcbiAgY29uc3QgcGFyc2VkU2NoZW1hID0gcGFyc2VKc29uKHNjaGVtYSk7XG4gIGlmIChwYXJzZWRTY2hlbWEgPT09IG51bGwgfHwgIWlzSnNvbk9iamVjdChwYXJzZWRTY2hlbWEpKSB7XG4gICAgcmV0dXJuIG51bGw7XG4gIH1cblxuICByZXR1cm4gcGFyc2VkU2NoZW1hO1xufVxuIl19