"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
const ts = require("typescript");
// Find all nodes from the AST in the subtree of node of SyntaxKind kind.
function collectDeepNodes(node, kind) {
    const nodes = [];
    const helper = (child) => {
        if (child.kind === kind) {
            nodes.push(child);
        }
        ts.forEachChild(child, helper);
    };
    ts.forEachChild(node, helper);
    return nodes;
}
exports.collectDeepNodes = collectDeepNodes;
function drilldownNodes(startingNode, path) {
    let currentNode = startingNode;
    for (const segment of path) {
        if (segment.prop) {
            // ts.Node has no index signature, so we need to cast it as any.
            const tempNode = currentNode[segment.prop];
            if (!tempNode || typeof tempNode != 'object' || currentNode.kind !== segment.kind) {
                return null;
            }
            // tslint:disable-next-line:no-any
            currentNode = tempNode;
        }
    }
    return currentNode;
}
exports.drilldownNodes = drilldownNodes;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiYXN0LXV0aWxzLmpzIiwic291cmNlUm9vdCI6Ii4vIiwic291cmNlcyI6WyJwYWNrYWdlcy9hbmd1bGFyX2RldmtpdC9idWlsZF9vcHRpbWl6ZXIvc3JjL2hlbHBlcnMvYXN0LXV0aWxzLnRzIl0sIm5hbWVzIjpbXSwibWFwcGluZ3MiOiI7O0FBQUE7Ozs7OztHQU1HO0FBQ0gsaUNBQWlDO0FBRWpDLHlFQUF5RTtBQUN6RSwwQkFBb0QsSUFBYSxFQUFFLElBQW1CO0lBQ3BGLE1BQU0sS0FBSyxHQUFRLEVBQUUsQ0FBQztJQUN0QixNQUFNLE1BQU0sR0FBRyxDQUFDLEtBQWMsRUFBRSxFQUFFO1FBQ2hDLElBQUksS0FBSyxDQUFDLElBQUksS0FBSyxJQUFJLEVBQUU7WUFDdkIsS0FBSyxDQUFDLElBQUksQ0FBQyxLQUFVLENBQUMsQ0FBQztTQUN4QjtRQUNELEVBQUUsQ0FBQyxZQUFZLENBQUMsS0FBSyxFQUFFLE1BQU0sQ0FBQyxDQUFDO0lBQ2pDLENBQUMsQ0FBQztJQUNGLEVBQUUsQ0FBQyxZQUFZLENBQUMsSUFBSSxFQUFFLE1BQU0sQ0FBQyxDQUFDO0lBRTlCLE9BQU8sS0FBSyxDQUFDO0FBQ2YsQ0FBQztBQVhELDRDQVdDO0FBRUQsd0JBQ0UsWUFBZSxFQUNmLElBQThDO0lBRTlDLElBQUksV0FBVyxHQUFNLFlBQVksQ0FBQztJQUNsQyxLQUFLLE1BQU0sT0FBTyxJQUFJLElBQUksRUFBRTtRQUMxQixJQUFJLE9BQU8sQ0FBQyxJQUFJLEVBQUU7WUFDaEIsZ0VBQWdFO1lBQ2hFLE1BQU0sUUFBUSxHQUFHLFdBQVcsQ0FBQyxPQUFPLENBQUMsSUFBSSxDQUFDLENBQUM7WUFDM0MsSUFBSSxDQUFDLFFBQVEsSUFBSSxPQUFPLFFBQVEsSUFBSSxRQUFRLElBQUksV0FBVyxDQUFDLElBQUksS0FBSyxPQUFPLENBQUMsSUFBSSxFQUFFO2dCQUNqRixPQUFPLElBQUksQ0FBQzthQUNiO1lBRUQsa0NBQWtDO1lBQ2xDLFdBQVcsR0FBRyxRQUFvQixDQUFDO1NBQ3BDO0tBQ0Y7SUFFRCxPQUFPLFdBQVcsQ0FBQztBQUNyQixDQUFDO0FBbkJELHdDQW1CQyIsInNvdXJjZXNDb250ZW50IjpbIi8qKlxuICogQGxpY2Vuc2VcbiAqIENvcHlyaWdodCBHb29nbGUgSW5jLiBBbGwgUmlnaHRzIFJlc2VydmVkLlxuICpcbiAqIFVzZSBvZiB0aGlzIHNvdXJjZSBjb2RlIGlzIGdvdmVybmVkIGJ5IGFuIE1JVC1zdHlsZSBsaWNlbnNlIHRoYXQgY2FuIGJlXG4gKiBmb3VuZCBpbiB0aGUgTElDRU5TRSBmaWxlIGF0IGh0dHBzOi8vYW5ndWxhci5pby9saWNlbnNlXG4gKi9cbmltcG9ydCAqIGFzIHRzIGZyb20gJ3R5cGVzY3JpcHQnO1xuXG4vLyBGaW5kIGFsbCBub2RlcyBmcm9tIHRoZSBBU1QgaW4gdGhlIHN1YnRyZWUgb2Ygbm9kZSBvZiBTeW50YXhLaW5kIGtpbmQuXG5leHBvcnQgZnVuY3Rpb24gY29sbGVjdERlZXBOb2RlczxUIGV4dGVuZHMgdHMuTm9kZT4obm9kZTogdHMuTm9kZSwga2luZDogdHMuU3ludGF4S2luZCk6IFRbXSB7XG4gIGNvbnN0IG5vZGVzOiBUW10gPSBbXTtcbiAgY29uc3QgaGVscGVyID0gKGNoaWxkOiB0cy5Ob2RlKSA9PiB7XG4gICAgaWYgKGNoaWxkLmtpbmQgPT09IGtpbmQpIHtcbiAgICAgIG5vZGVzLnB1c2goY2hpbGQgYXMgVCk7XG4gICAgfVxuICAgIHRzLmZvckVhY2hDaGlsZChjaGlsZCwgaGVscGVyKTtcbiAgfTtcbiAgdHMuZm9yRWFjaENoaWxkKG5vZGUsIGhlbHBlcik7XG5cbiAgcmV0dXJuIG5vZGVzO1xufVxuXG5leHBvcnQgZnVuY3Rpb24gZHJpbGxkb3duTm9kZXM8VCBleHRlbmRzIHRzLk5vZGU+KFxuICBzdGFydGluZ05vZGU6IFQsXG4gIHBhdGg6IHsgcHJvcDoga2V5b2YgVCwga2luZDogdHMuU3ludGF4S2luZCB9W10sXG4pOiBUIHwgbnVsbCB7XG4gIGxldCBjdXJyZW50Tm9kZTogVCA9IHN0YXJ0aW5nTm9kZTtcbiAgZm9yIChjb25zdCBzZWdtZW50IG9mIHBhdGgpIHtcbiAgICBpZiAoc2VnbWVudC5wcm9wKSB7XG4gICAgICAvLyB0cy5Ob2RlIGhhcyBubyBpbmRleCBzaWduYXR1cmUsIHNvIHdlIG5lZWQgdG8gY2FzdCBpdCBhcyBhbnkuXG4gICAgICBjb25zdCB0ZW1wTm9kZSA9IGN1cnJlbnROb2RlW3NlZ21lbnQucHJvcF07XG4gICAgICBpZiAoIXRlbXBOb2RlIHx8IHR5cGVvZiB0ZW1wTm9kZSAhPSAnb2JqZWN0JyB8fCBjdXJyZW50Tm9kZS5raW5kICE9PSBzZWdtZW50LmtpbmQpIHtcbiAgICAgICAgcmV0dXJuIG51bGw7XG4gICAgICB9XG5cbiAgICAgIC8vIHRzbGludDpkaXNhYmxlLW5leHQtbGluZTpuby1hbnlcbiAgICAgIGN1cnJlbnROb2RlID0gdGVtcE5vZGUgYXMgYW55IGFzIFQ7XG4gICAgfVxuICB9XG5cbiAgcmV0dXJuIGN1cnJlbnROb2RlO1xufVxuIl19