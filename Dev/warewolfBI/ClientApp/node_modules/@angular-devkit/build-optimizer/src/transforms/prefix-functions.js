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
const pureFunctionComment = '@__PURE__';
function getPrefixFunctionsTransformer() {
    return (context) => {
        const transformer = (sf) => {
            const topLevelFunctions = findTopLevelFunctions(sf);
            const pureImports = findPureImports(sf);
            const pureImportsComment = `* PURE_IMPORTS_START ${pureImports.join(',')} PURE_IMPORTS_END `;
            const visitor = (node) => {
                // Add the pure imports comment to the first node.
                if (node.parent && node.parent.parent === undefined && node.pos === 0) {
                    const newNode = ts.addSyntheticLeadingComment(node, ts.SyntaxKind.MultiLineCommentTrivia, pureImportsComment, true);
                    // Replace node with modified one.
                    return ts.visitEachChild(newNode, visitor, context);
                }
                // Add pure function comment to top level functions.
                if (topLevelFunctions.has(node)) {
                    const newNode = ts.addSyntheticLeadingComment(node, ts.SyntaxKind.MultiLineCommentTrivia, pureFunctionComment, false);
                    // Replace node with modified one.
                    return ts.visitEachChild(newNode, visitor, context);
                }
                // Otherwise return node as is.
                return ts.visitEachChild(node, visitor, context);
            };
            return ts.visitNode(sf, visitor);
        };
        return transformer;
    };
}
exports.getPrefixFunctionsTransformer = getPrefixFunctionsTransformer;
function findTopLevelFunctions(parentNode) {
    const topLevelFunctions = new Set();
    function cb(node) {
        // Stop recursing into this branch if it's a definition construct.
        // These are function expression, function declaration, class, or arrow function (lambda).
        // The body of these constructs will not execute when loading the module, so we don't
        // need to mark function calls inside them as pure.
        // Class static initializers in ES2015 are an exception we don't cover. They would need similar
        // processing as enums to prevent property setting from causing the class to be retained.
        if (ts.isFunctionDeclaration(node)
            || ts.isFunctionExpression(node)
            || ts.isClassDeclaration(node)
            || ts.isArrowFunction(node)
            || ts.isMethodDeclaration(node)) {
            return;
        }
        let noPureComment = !hasPureComment(node);
        let innerNode = node;
        while (innerNode && ts.isParenthesizedExpression(innerNode)) {
            innerNode = innerNode.expression;
            noPureComment = noPureComment && !hasPureComment(innerNode);
        }
        if (!innerNode) {
            return;
        }
        if (noPureComment) {
            if (ts.isNewExpression(innerNode)) {
                topLevelFunctions.add(node);
            }
            else if (ts.isCallExpression(innerNode)) {
                let expression = innerNode.expression;
                while (expression && ts.isParenthesizedExpression(expression)) {
                    expression = expression.expression;
                }
                if (expression) {
                    if (ts.isFunctionExpression(expression)) {
                        // Skip IIFE's with arguments
                        // This could be improved to check if there are any references to variables
                        if (innerNode.arguments.length === 0) {
                            topLevelFunctions.add(node);
                        }
                    }
                    else {
                        topLevelFunctions.add(node);
                    }
                }
            }
        }
        ts.forEachChild(innerNode, cb);
    }
    ts.forEachChild(parentNode, cb);
    return topLevelFunctions;
}
exports.findTopLevelFunctions = findTopLevelFunctions;
function findPureImports(parentNode) {
    const pureImports = [];
    ts.forEachChild(parentNode, cb);
    function cb(node) {
        if (node.kind === ts.SyntaxKind.ImportDeclaration
            && node.importClause) {
            // Save the path of the import transformed into snake case and remove relative paths.
            const moduleSpecifier = node.moduleSpecifier;
            const pureImport = moduleSpecifier.text
                .replace(/[\/@\-]/g, '_')
                .replace(/^\.+/, '');
            pureImports.push(pureImport);
        }
        ts.forEachChild(node, cb);
    }
    return pureImports;
}
exports.findPureImports = findPureImports;
function hasPureComment(node) {
    if (!node) {
        return false;
    }
    const leadingComment = ts.getSyntheticLeadingComments(node);
    return leadingComment && leadingComment.some((comment) => comment.text === pureFunctionComment);
}
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoicHJlZml4LWZ1bmN0aW9ucy5qcyIsInNvdXJjZVJvb3QiOiIuLyIsInNvdXJjZXMiOlsicGFja2FnZXMvYW5ndWxhcl9kZXZraXQvYnVpbGRfb3B0aW1pemVyL3NyYy90cmFuc2Zvcm1zL3ByZWZpeC1mdW5jdGlvbnMudHMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6Ijs7QUFBQTs7Ozs7O0dBTUc7QUFDSCxpQ0FBaUM7QUFHakMsTUFBTSxtQkFBbUIsR0FBRyxXQUFXLENBQUM7QUFFeEM7SUFDRSxPQUFPLENBQUMsT0FBaUMsRUFBaUMsRUFBRTtRQUMxRSxNQUFNLFdBQVcsR0FBa0MsQ0FBQyxFQUFpQixFQUFFLEVBQUU7WUFFdkUsTUFBTSxpQkFBaUIsR0FBRyxxQkFBcUIsQ0FBQyxFQUFFLENBQUMsQ0FBQztZQUNwRCxNQUFNLFdBQVcsR0FBRyxlQUFlLENBQUMsRUFBRSxDQUFDLENBQUM7WUFDeEMsTUFBTSxrQkFBa0IsR0FBRyx3QkFBd0IsV0FBVyxDQUFDLElBQUksQ0FBQyxHQUFHLENBQUMsb0JBQW9CLENBQUM7WUFFN0YsTUFBTSxPQUFPLEdBQWUsQ0FBQyxJQUFhLEVBQVcsRUFBRTtnQkFFckQsa0RBQWtEO2dCQUNsRCxJQUFJLElBQUksQ0FBQyxNQUFNLElBQUksSUFBSSxDQUFDLE1BQU0sQ0FBQyxNQUFNLEtBQUssU0FBUyxJQUFJLElBQUksQ0FBQyxHQUFHLEtBQUssQ0FBQyxFQUFFO29CQUNyRSxNQUFNLE9BQU8sR0FBRyxFQUFFLENBQUMsMEJBQTBCLENBQzNDLElBQUksRUFBRSxFQUFFLENBQUMsVUFBVSxDQUFDLHNCQUFzQixFQUFFLGtCQUFrQixFQUFFLElBQUksQ0FBQyxDQUFDO29CQUV4RSxrQ0FBa0M7b0JBQ2xDLE9BQU8sRUFBRSxDQUFDLGNBQWMsQ0FBQyxPQUFPLEVBQUUsT0FBTyxFQUFFLE9BQU8sQ0FBQyxDQUFDO2lCQUNyRDtnQkFFRCxvREFBb0Q7Z0JBQ3BELElBQUksaUJBQWlCLENBQUMsR0FBRyxDQUFDLElBQUksQ0FBQyxFQUFFO29CQUMvQixNQUFNLE9BQU8sR0FBRyxFQUFFLENBQUMsMEJBQTBCLENBQzNDLElBQUksRUFBRSxFQUFFLENBQUMsVUFBVSxDQUFDLHNCQUFzQixFQUFFLG1CQUFtQixFQUFFLEtBQUssQ0FBQyxDQUFDO29CQUUxRSxrQ0FBa0M7b0JBQ2xDLE9BQU8sRUFBRSxDQUFDLGNBQWMsQ0FBQyxPQUFPLEVBQUUsT0FBTyxFQUFFLE9BQU8sQ0FBQyxDQUFDO2lCQUNyRDtnQkFFRCwrQkFBK0I7Z0JBQy9CLE9BQU8sRUFBRSxDQUFDLGNBQWMsQ0FBQyxJQUFJLEVBQUUsT0FBTyxFQUFFLE9BQU8sQ0FBQyxDQUFDO1lBQ25ELENBQUMsQ0FBQztZQUVGLE9BQU8sRUFBRSxDQUFDLFNBQVMsQ0FBQyxFQUFFLEVBQUUsT0FBTyxDQUFDLENBQUM7UUFDbkMsQ0FBQyxDQUFDO1FBRUYsT0FBTyxXQUFXLENBQUM7SUFDckIsQ0FBQyxDQUFDO0FBQ0osQ0FBQztBQXJDRCxzRUFxQ0M7QUFFRCwrQkFBc0MsVUFBbUI7SUFDdkQsTUFBTSxpQkFBaUIsR0FBRyxJQUFJLEdBQUcsRUFBVyxDQUFDO0lBRTdDLFlBQVksSUFBYTtRQUN2QixrRUFBa0U7UUFDbEUsMEZBQTBGO1FBQzFGLHFGQUFxRjtRQUNyRixtREFBbUQ7UUFDbkQsK0ZBQStGO1FBQy9GLHlGQUF5RjtRQUN6RixJQUFJLEVBQUUsQ0FBQyxxQkFBcUIsQ0FBQyxJQUFJLENBQUM7ZUFDN0IsRUFBRSxDQUFDLG9CQUFvQixDQUFDLElBQUksQ0FBQztlQUM3QixFQUFFLENBQUMsa0JBQWtCLENBQUMsSUFBSSxDQUFDO2VBQzNCLEVBQUUsQ0FBQyxlQUFlLENBQUMsSUFBSSxDQUFDO2VBQ3hCLEVBQUUsQ0FBQyxtQkFBbUIsQ0FBQyxJQUFJLENBQUMsRUFDL0I7WUFDQSxPQUFPO1NBQ1I7UUFFRCxJQUFJLGFBQWEsR0FBRyxDQUFDLGNBQWMsQ0FBQyxJQUFJLENBQUMsQ0FBQztRQUMxQyxJQUFJLFNBQVMsR0FBRyxJQUFJLENBQUM7UUFDckIsT0FBTyxTQUFTLElBQUksRUFBRSxDQUFDLHlCQUF5QixDQUFDLFNBQVMsQ0FBQyxFQUFFO1lBQzNELFNBQVMsR0FBRyxTQUFTLENBQUMsVUFBVSxDQUFDO1lBQ2pDLGFBQWEsR0FBRyxhQUFhLElBQUksQ0FBQyxjQUFjLENBQUMsU0FBUyxDQUFDLENBQUM7U0FDN0Q7UUFFRCxJQUFJLENBQUMsU0FBUyxFQUFFO1lBQ2QsT0FBTztTQUNSO1FBRUQsSUFBSSxhQUFhLEVBQUU7WUFDakIsSUFBSSxFQUFFLENBQUMsZUFBZSxDQUFDLFNBQVMsQ0FBQyxFQUFFO2dCQUNqQyxpQkFBaUIsQ0FBQyxHQUFHLENBQUMsSUFBSSxDQUFDLENBQUM7YUFDN0I7aUJBQU0sSUFBSSxFQUFFLENBQUMsZ0JBQWdCLENBQUMsU0FBUyxDQUFDLEVBQUU7Z0JBQ3pDLElBQUksVUFBVSxHQUFrQixTQUFTLENBQUMsVUFBVSxDQUFDO2dCQUNyRCxPQUFPLFVBQVUsSUFBSSxFQUFFLENBQUMseUJBQXlCLENBQUMsVUFBVSxDQUFDLEVBQUU7b0JBQzdELFVBQVUsR0FBRyxVQUFVLENBQUMsVUFBVSxDQUFDO2lCQUNwQztnQkFDRCxJQUFJLFVBQVUsRUFBRTtvQkFDZCxJQUFJLEVBQUUsQ0FBQyxvQkFBb0IsQ0FBQyxVQUFVLENBQUMsRUFBRTt3QkFDdkMsNkJBQTZCO3dCQUM3QiwyRUFBMkU7d0JBQzNFLElBQUksU0FBUyxDQUFDLFNBQVMsQ0FBQyxNQUFNLEtBQUssQ0FBQyxFQUFFOzRCQUNwQyxpQkFBaUIsQ0FBQyxHQUFHLENBQUMsSUFBSSxDQUFDLENBQUM7eUJBQzdCO3FCQUNGO3lCQUFNO3dCQUNMLGlCQUFpQixDQUFDLEdBQUcsQ0FBQyxJQUFJLENBQUMsQ0FBQztxQkFDN0I7aUJBQ0Y7YUFDRjtTQUNGO1FBRUQsRUFBRSxDQUFDLFlBQVksQ0FBQyxTQUFTLEVBQUUsRUFBRSxDQUFDLENBQUM7SUFDakMsQ0FBQztJQUVELEVBQUUsQ0FBQyxZQUFZLENBQUMsVUFBVSxFQUFFLEVBQUUsQ0FBQyxDQUFDO0lBRWhDLE9BQU8saUJBQWlCLENBQUM7QUFDM0IsQ0FBQztBQTFERCxzREEwREM7QUFFRCx5QkFBZ0MsVUFBbUI7SUFDakQsTUFBTSxXQUFXLEdBQWEsRUFBRSxDQUFDO0lBQ2pDLEVBQUUsQ0FBQyxZQUFZLENBQUMsVUFBVSxFQUFFLEVBQUUsQ0FBQyxDQUFDO0lBRWhDLFlBQVksSUFBYTtRQUN2QixJQUFJLElBQUksQ0FBQyxJQUFJLEtBQUssRUFBRSxDQUFDLFVBQVUsQ0FBQyxpQkFBaUI7ZUFDM0MsSUFBNkIsQ0FBQyxZQUFZLEVBQUU7WUFFaEQscUZBQXFGO1lBQ3JGLE1BQU0sZUFBZSxHQUFJLElBQTZCLENBQUMsZUFBbUMsQ0FBQztZQUMzRixNQUFNLFVBQVUsR0FBRyxlQUFlLENBQUMsSUFBSTtpQkFDcEMsT0FBTyxDQUFDLFVBQVUsRUFBRSxHQUFHLENBQUM7aUJBQ3hCLE9BQU8sQ0FBQyxNQUFNLEVBQUUsRUFBRSxDQUFDLENBQUM7WUFDdkIsV0FBVyxDQUFDLElBQUksQ0FBQyxVQUFVLENBQUMsQ0FBQztTQUM5QjtRQUVELEVBQUUsQ0FBQyxZQUFZLENBQUMsSUFBSSxFQUFFLEVBQUUsQ0FBQyxDQUFDO0lBQzVCLENBQUM7SUFFRCxPQUFPLFdBQVcsQ0FBQztBQUNyQixDQUFDO0FBcEJELDBDQW9CQztBQUVELHdCQUF3QixJQUFhO0lBQ25DLElBQUksQ0FBQyxJQUFJLEVBQUU7UUFDVCxPQUFPLEtBQUssQ0FBQztLQUNkO0lBQ0QsTUFBTSxjQUFjLEdBQUcsRUFBRSxDQUFDLDJCQUEyQixDQUFDLElBQUksQ0FBQyxDQUFDO0lBRTVELE9BQU8sY0FBYyxJQUFJLGNBQWMsQ0FBQyxJQUFJLENBQUMsQ0FBQyxPQUFPLEVBQUUsRUFBRSxDQUFDLE9BQU8sQ0FBQyxJQUFJLEtBQUssbUJBQW1CLENBQUMsQ0FBQztBQUNsRyxDQUFDIiwic291cmNlc0NvbnRlbnQiOlsiLyoqXG4gKiBAbGljZW5zZVxuICogQ29weXJpZ2h0IEdvb2dsZSBJbmMuIEFsbCBSaWdodHMgUmVzZXJ2ZWQuXG4gKlxuICogVXNlIG9mIHRoaXMgc291cmNlIGNvZGUgaXMgZ292ZXJuZWQgYnkgYW4gTUlULXN0eWxlIGxpY2Vuc2UgdGhhdCBjYW4gYmVcbiAqIGZvdW5kIGluIHRoZSBMSUNFTlNFIGZpbGUgYXQgaHR0cHM6Ly9hbmd1bGFyLmlvL2xpY2Vuc2VcbiAqL1xuaW1wb3J0ICogYXMgdHMgZnJvbSAndHlwZXNjcmlwdCc7XG5cblxuY29uc3QgcHVyZUZ1bmN0aW9uQ29tbWVudCA9ICdAX19QVVJFX18nO1xuXG5leHBvcnQgZnVuY3Rpb24gZ2V0UHJlZml4RnVuY3Rpb25zVHJhbnNmb3JtZXIoKTogdHMuVHJhbnNmb3JtZXJGYWN0b3J5PHRzLlNvdXJjZUZpbGU+IHtcbiAgcmV0dXJuIChjb250ZXh0OiB0cy5UcmFuc2Zvcm1hdGlvbkNvbnRleHQpOiB0cy5UcmFuc2Zvcm1lcjx0cy5Tb3VyY2VGaWxlPiA9PiB7XG4gICAgY29uc3QgdHJhbnNmb3JtZXI6IHRzLlRyYW5zZm9ybWVyPHRzLlNvdXJjZUZpbGU+ID0gKHNmOiB0cy5Tb3VyY2VGaWxlKSA9PiB7XG5cbiAgICAgIGNvbnN0IHRvcExldmVsRnVuY3Rpb25zID0gZmluZFRvcExldmVsRnVuY3Rpb25zKHNmKTtcbiAgICAgIGNvbnN0IHB1cmVJbXBvcnRzID0gZmluZFB1cmVJbXBvcnRzKHNmKTtcbiAgICAgIGNvbnN0IHB1cmVJbXBvcnRzQ29tbWVudCA9IGAqIFBVUkVfSU1QT1JUU19TVEFSVCAke3B1cmVJbXBvcnRzLmpvaW4oJywnKX0gUFVSRV9JTVBPUlRTX0VORCBgO1xuXG4gICAgICBjb25zdCB2aXNpdG9yOiB0cy5WaXNpdG9yID0gKG5vZGU6IHRzLk5vZGUpOiB0cy5Ob2RlID0+IHtcblxuICAgICAgICAvLyBBZGQgdGhlIHB1cmUgaW1wb3J0cyBjb21tZW50IHRvIHRoZSBmaXJzdCBub2RlLlxuICAgICAgICBpZiAobm9kZS5wYXJlbnQgJiYgbm9kZS5wYXJlbnQucGFyZW50ID09PSB1bmRlZmluZWQgJiYgbm9kZS5wb3MgPT09IDApIHtcbiAgICAgICAgICBjb25zdCBuZXdOb2RlID0gdHMuYWRkU3ludGhldGljTGVhZGluZ0NvbW1lbnQoXG4gICAgICAgICAgICBub2RlLCB0cy5TeW50YXhLaW5kLk11bHRpTGluZUNvbW1lbnRUcml2aWEsIHB1cmVJbXBvcnRzQ29tbWVudCwgdHJ1ZSk7XG5cbiAgICAgICAgICAvLyBSZXBsYWNlIG5vZGUgd2l0aCBtb2RpZmllZCBvbmUuXG4gICAgICAgICAgcmV0dXJuIHRzLnZpc2l0RWFjaENoaWxkKG5ld05vZGUsIHZpc2l0b3IsIGNvbnRleHQpO1xuICAgICAgICB9XG5cbiAgICAgICAgLy8gQWRkIHB1cmUgZnVuY3Rpb24gY29tbWVudCB0byB0b3AgbGV2ZWwgZnVuY3Rpb25zLlxuICAgICAgICBpZiAodG9wTGV2ZWxGdW5jdGlvbnMuaGFzKG5vZGUpKSB7XG4gICAgICAgICAgY29uc3QgbmV3Tm9kZSA9IHRzLmFkZFN5bnRoZXRpY0xlYWRpbmdDb21tZW50KFxuICAgICAgICAgICAgbm9kZSwgdHMuU3ludGF4S2luZC5NdWx0aUxpbmVDb21tZW50VHJpdmlhLCBwdXJlRnVuY3Rpb25Db21tZW50LCBmYWxzZSk7XG5cbiAgICAgICAgICAvLyBSZXBsYWNlIG5vZGUgd2l0aCBtb2RpZmllZCBvbmUuXG4gICAgICAgICAgcmV0dXJuIHRzLnZpc2l0RWFjaENoaWxkKG5ld05vZGUsIHZpc2l0b3IsIGNvbnRleHQpO1xuICAgICAgICB9XG5cbiAgICAgICAgLy8gT3RoZXJ3aXNlIHJldHVybiBub2RlIGFzIGlzLlxuICAgICAgICByZXR1cm4gdHMudmlzaXRFYWNoQ2hpbGQobm9kZSwgdmlzaXRvciwgY29udGV4dCk7XG4gICAgICB9O1xuXG4gICAgICByZXR1cm4gdHMudmlzaXROb2RlKHNmLCB2aXNpdG9yKTtcbiAgICB9O1xuXG4gICAgcmV0dXJuIHRyYW5zZm9ybWVyO1xuICB9O1xufVxuXG5leHBvcnQgZnVuY3Rpb24gZmluZFRvcExldmVsRnVuY3Rpb25zKHBhcmVudE5vZGU6IHRzLk5vZGUpOiBTZXQ8dHMuTm9kZT4ge1xuICBjb25zdCB0b3BMZXZlbEZ1bmN0aW9ucyA9IG5ldyBTZXQ8dHMuTm9kZT4oKTtcblxuICBmdW5jdGlvbiBjYihub2RlOiB0cy5Ob2RlKSB7XG4gICAgLy8gU3RvcCByZWN1cnNpbmcgaW50byB0aGlzIGJyYW5jaCBpZiBpdCdzIGEgZGVmaW5pdGlvbiBjb25zdHJ1Y3QuXG4gICAgLy8gVGhlc2UgYXJlIGZ1bmN0aW9uIGV4cHJlc3Npb24sIGZ1bmN0aW9uIGRlY2xhcmF0aW9uLCBjbGFzcywgb3IgYXJyb3cgZnVuY3Rpb24gKGxhbWJkYSkuXG4gICAgLy8gVGhlIGJvZHkgb2YgdGhlc2UgY29uc3RydWN0cyB3aWxsIG5vdCBleGVjdXRlIHdoZW4gbG9hZGluZyB0aGUgbW9kdWxlLCBzbyB3ZSBkb24ndFxuICAgIC8vIG5lZWQgdG8gbWFyayBmdW5jdGlvbiBjYWxscyBpbnNpZGUgdGhlbSBhcyBwdXJlLlxuICAgIC8vIENsYXNzIHN0YXRpYyBpbml0aWFsaXplcnMgaW4gRVMyMDE1IGFyZSBhbiBleGNlcHRpb24gd2UgZG9uJ3QgY292ZXIuIFRoZXkgd291bGQgbmVlZCBzaW1pbGFyXG4gICAgLy8gcHJvY2Vzc2luZyBhcyBlbnVtcyB0byBwcmV2ZW50IHByb3BlcnR5IHNldHRpbmcgZnJvbSBjYXVzaW5nIHRoZSBjbGFzcyB0byBiZSByZXRhaW5lZC5cbiAgICBpZiAodHMuaXNGdW5jdGlvbkRlY2xhcmF0aW9uKG5vZGUpXG4gICAgICB8fCB0cy5pc0Z1bmN0aW9uRXhwcmVzc2lvbihub2RlKVxuICAgICAgfHwgdHMuaXNDbGFzc0RlY2xhcmF0aW9uKG5vZGUpXG4gICAgICB8fCB0cy5pc0Fycm93RnVuY3Rpb24obm9kZSlcbiAgICAgIHx8IHRzLmlzTWV0aG9kRGVjbGFyYXRpb24obm9kZSlcbiAgICApIHtcbiAgICAgIHJldHVybjtcbiAgICB9XG5cbiAgICBsZXQgbm9QdXJlQ29tbWVudCA9ICFoYXNQdXJlQ29tbWVudChub2RlKTtcbiAgICBsZXQgaW5uZXJOb2RlID0gbm9kZTtcbiAgICB3aGlsZSAoaW5uZXJOb2RlICYmIHRzLmlzUGFyZW50aGVzaXplZEV4cHJlc3Npb24oaW5uZXJOb2RlKSkge1xuICAgICAgaW5uZXJOb2RlID0gaW5uZXJOb2RlLmV4cHJlc3Npb247XG4gICAgICBub1B1cmVDb21tZW50ID0gbm9QdXJlQ29tbWVudCAmJiAhaGFzUHVyZUNvbW1lbnQoaW5uZXJOb2RlKTtcbiAgICB9XG5cbiAgICBpZiAoIWlubmVyTm9kZSkge1xuICAgICAgcmV0dXJuO1xuICAgIH1cblxuICAgIGlmIChub1B1cmVDb21tZW50KSB7XG4gICAgICBpZiAodHMuaXNOZXdFeHByZXNzaW9uKGlubmVyTm9kZSkpIHtcbiAgICAgICAgdG9wTGV2ZWxGdW5jdGlvbnMuYWRkKG5vZGUpO1xuICAgICAgfSBlbHNlIGlmICh0cy5pc0NhbGxFeHByZXNzaW9uKGlubmVyTm9kZSkpIHtcbiAgICAgICAgbGV0IGV4cHJlc3Npb246IHRzLkV4cHJlc3Npb24gPSBpbm5lck5vZGUuZXhwcmVzc2lvbjtcbiAgICAgICAgd2hpbGUgKGV4cHJlc3Npb24gJiYgdHMuaXNQYXJlbnRoZXNpemVkRXhwcmVzc2lvbihleHByZXNzaW9uKSkge1xuICAgICAgICAgIGV4cHJlc3Npb24gPSBleHByZXNzaW9uLmV4cHJlc3Npb247XG4gICAgICAgIH1cbiAgICAgICAgaWYgKGV4cHJlc3Npb24pIHtcbiAgICAgICAgICBpZiAodHMuaXNGdW5jdGlvbkV4cHJlc3Npb24oZXhwcmVzc2lvbikpIHtcbiAgICAgICAgICAgIC8vIFNraXAgSUlGRSdzIHdpdGggYXJndW1lbnRzXG4gICAgICAgICAgICAvLyBUaGlzIGNvdWxkIGJlIGltcHJvdmVkIHRvIGNoZWNrIGlmIHRoZXJlIGFyZSBhbnkgcmVmZXJlbmNlcyB0byB2YXJpYWJsZXNcbiAgICAgICAgICAgIGlmIChpbm5lck5vZGUuYXJndW1lbnRzLmxlbmd0aCA9PT0gMCkge1xuICAgICAgICAgICAgICB0b3BMZXZlbEZ1bmN0aW9ucy5hZGQobm9kZSk7XG4gICAgICAgICAgICB9XG4gICAgICAgICAgfSBlbHNlIHtcbiAgICAgICAgICAgIHRvcExldmVsRnVuY3Rpb25zLmFkZChub2RlKTtcbiAgICAgICAgICB9XG4gICAgICAgIH1cbiAgICAgIH1cbiAgICB9XG5cbiAgICB0cy5mb3JFYWNoQ2hpbGQoaW5uZXJOb2RlLCBjYik7XG4gIH1cblxuICB0cy5mb3JFYWNoQ2hpbGQocGFyZW50Tm9kZSwgY2IpO1xuXG4gIHJldHVybiB0b3BMZXZlbEZ1bmN0aW9ucztcbn1cblxuZXhwb3J0IGZ1bmN0aW9uIGZpbmRQdXJlSW1wb3J0cyhwYXJlbnROb2RlOiB0cy5Ob2RlKTogc3RyaW5nW10ge1xuICBjb25zdCBwdXJlSW1wb3J0czogc3RyaW5nW10gPSBbXTtcbiAgdHMuZm9yRWFjaENoaWxkKHBhcmVudE5vZGUsIGNiKTtcblxuICBmdW5jdGlvbiBjYihub2RlOiB0cy5Ob2RlKSB7XG4gICAgaWYgKG5vZGUua2luZCA9PT0gdHMuU3ludGF4S2luZC5JbXBvcnREZWNsYXJhdGlvblxuICAgICAgJiYgKG5vZGUgYXMgdHMuSW1wb3J0RGVjbGFyYXRpb24pLmltcG9ydENsYXVzZSkge1xuXG4gICAgICAvLyBTYXZlIHRoZSBwYXRoIG9mIHRoZSBpbXBvcnQgdHJhbnNmb3JtZWQgaW50byBzbmFrZSBjYXNlIGFuZCByZW1vdmUgcmVsYXRpdmUgcGF0aHMuXG4gICAgICBjb25zdCBtb2R1bGVTcGVjaWZpZXIgPSAobm9kZSBhcyB0cy5JbXBvcnREZWNsYXJhdGlvbikubW9kdWxlU3BlY2lmaWVyIGFzIHRzLlN0cmluZ0xpdGVyYWw7XG4gICAgICBjb25zdCBwdXJlSW1wb3J0ID0gbW9kdWxlU3BlY2lmaWVyLnRleHRcbiAgICAgICAgLnJlcGxhY2UoL1tcXC9AXFwtXS9nLCAnXycpXG4gICAgICAgIC5yZXBsYWNlKC9eXFwuKy8sICcnKTtcbiAgICAgIHB1cmVJbXBvcnRzLnB1c2gocHVyZUltcG9ydCk7XG4gICAgfVxuXG4gICAgdHMuZm9yRWFjaENoaWxkKG5vZGUsIGNiKTtcbiAgfVxuXG4gIHJldHVybiBwdXJlSW1wb3J0cztcbn1cblxuZnVuY3Rpb24gaGFzUHVyZUNvbW1lbnQobm9kZTogdHMuTm9kZSkge1xuICBpZiAoIW5vZGUpIHtcbiAgICByZXR1cm4gZmFsc2U7XG4gIH1cbiAgY29uc3QgbGVhZGluZ0NvbW1lbnQgPSB0cy5nZXRTeW50aGV0aWNMZWFkaW5nQ29tbWVudHMobm9kZSk7XG5cbiAgcmV0dXJuIGxlYWRpbmdDb21tZW50ICYmIGxlYWRpbmdDb21tZW50LnNvbWUoKGNvbW1lbnQpID0+IGNvbW1lbnQudGV4dCA9PT0gcHVyZUZ1bmN0aW9uQ29tbWVudCk7XG59XG4iXX0=