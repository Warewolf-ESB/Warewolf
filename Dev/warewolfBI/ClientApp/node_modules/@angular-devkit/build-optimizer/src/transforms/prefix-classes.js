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
function testPrefixClasses(content) {
    const exportVarSetter = /(?:export )?(?:var|const)\s+(\S+)\s*=\s*/;
    const multiLineComment = /\s*(?:\/\*[\s\S]*?\*\/)?\s*/;
    const newLine = /\s*\r?\n\s*/;
    const regexes = [
        [
            /^/,
            exportVarSetter, multiLineComment,
            /\(/, multiLineComment,
            /\s*function \(\) {/, newLine,
            multiLineComment,
            /function \1\([^\)]*\) \{/, newLine,
        ],
        [
            /^/,
            exportVarSetter, multiLineComment,
            /\(/, multiLineComment,
            /\s*function \(_super\) {/, newLine,
            /\w*\.?__extends\(\w+, _super\);/,
        ],
    ].map(arr => new RegExp(arr.map(x => x.source).join(''), 'm'));
    return regexes.some((regex) => regex.test(content));
}
exports.testPrefixClasses = testPrefixClasses;
const superParameterName = '_super';
const extendsHelperName = '__extends';
function getPrefixClassesTransformer() {
    return (context) => {
        const transformer = (sf) => {
            const pureFunctionComment = '@__PURE__';
            const visitor = (node) => {
                // Add pure comment to downleveled classes.
                if (ts.isVariableStatement(node) && isDownleveledClass(node)) {
                    const varDecl = node.declarationList.declarations[0];
                    const varInitializer = varDecl.initializer;
                    // Update node with the pure comment before the variable declaration initializer.
                    const newNode = ts.updateVariableStatement(node, node.modifiers, ts.updateVariableDeclarationList(node.declarationList, [
                        ts.updateVariableDeclaration(varDecl, varDecl.name, varDecl.type, ts.addSyntheticLeadingComment(varInitializer, ts.SyntaxKind.MultiLineCommentTrivia, pureFunctionComment, false)),
                    ]));
                    // Replace node with modified one.
                    return ts.visitEachChild(newNode, visitor, context);
                }
                // Otherwise return node as is.
                return ts.visitEachChild(node, visitor, context);
            };
            return ts.visitEachChild(sf, visitor, context);
        };
        return transformer;
    };
}
exports.getPrefixClassesTransformer = getPrefixClassesTransformer;
// Determine if a node matched the structure of a downleveled TS class.
function isDownleveledClass(node) {
    if (!ts.isVariableStatement(node)) {
        return false;
    }
    if (node.declarationList.declarations.length !== 1) {
        return false;
    }
    const variableDeclaration = node.declarationList.declarations[0];
    if (!ts.isIdentifier(variableDeclaration.name)
        || !variableDeclaration.initializer) {
        return false;
    }
    let potentialClass = variableDeclaration.initializer;
    // TS 2.3 has an unwrapped class IIFE
    // TS 2.4 uses a function expression wrapper
    // TS 2.5 uses an arrow function wrapper
    if (ts.isParenthesizedExpression(potentialClass)) {
        potentialClass = potentialClass.expression;
    }
    if (!ts.isCallExpression(potentialClass) || potentialClass.arguments.length > 1) {
        return false;
    }
    let wrapperBody;
    if (ts.isFunctionExpression(potentialClass.expression)) {
        wrapperBody = potentialClass.expression.body;
    }
    else if (ts.isArrowFunction(potentialClass.expression)
        && ts.isBlock(potentialClass.expression.body)) {
        wrapperBody = potentialClass.expression.body;
    }
    else {
        return false;
    }
    if (wrapperBody.statements.length === 0) {
        return false;
    }
    const functionExpression = potentialClass.expression;
    const functionStatements = wrapperBody.statements;
    // need a minimum of two for a function declaration and return statement
    if (functionStatements.length < 2) {
        return false;
    }
    // The variable name should be the class name.
    const className = variableDeclaration.name.text;
    const firstStatement = functionStatements[0];
    // find return statement - may not be last statement
    let returnStatement;
    for (let i = functionStatements.length - 1; i > 0; i--) {
        if (ts.isReturnStatement(functionStatements[i])) {
            returnStatement = functionStatements[i];
            break;
        }
    }
    if (returnStatement == undefined
        || returnStatement.expression == undefined
        || !ts.isIdentifier(returnStatement.expression)) {
        return false;
    }
    if (functionExpression.parameters.length === 0) {
        // potential non-extended class or wrapped es2015 class
        return (ts.isFunctionDeclaration(firstStatement) || ts.isClassDeclaration(firstStatement))
            && firstStatement.name !== undefined
            && firstStatement.name.text === className
            && returnStatement.expression.text === firstStatement.name.text;
    }
    else if (functionExpression.parameters.length !== 1) {
        return false;
    }
    // Potential extended class
    const functionParameter = functionExpression.parameters[0];
    if (!ts.isIdentifier(functionParameter.name)
        || functionParameter.name.text !== superParameterName) {
        return false;
    }
    if (functionStatements.length < 3 || !ts.isExpressionStatement(firstStatement)) {
        return false;
    }
    if (!ts.isCallExpression(firstStatement.expression)) {
        return false;
    }
    const extendCallExpression = firstStatement.expression;
    let functionName;
    if (ts.isIdentifier(extendCallExpression.expression)) {
        functionName = extendCallExpression.expression.text;
    }
    else if (ts.isPropertyAccessExpression(extendCallExpression.expression)) {
        functionName = extendCallExpression.expression.name.text;
    }
    if (!functionName || !functionName.endsWith(extendsHelperName)) {
        return false;
    }
    if (extendCallExpression.arguments.length === 0) {
        return false;
    }
    const lastArgument = extendCallExpression.arguments[extendCallExpression.arguments.length - 1];
    if (!ts.isIdentifier(lastArgument) || lastArgument.text !== functionParameter.name.text) {
        return false;
    }
    const secondStatement = functionStatements[1];
    return ts.isFunctionDeclaration(secondStatement)
        && secondStatement.name !== undefined
        && className.endsWith(secondStatement.name.text)
        && returnStatement.expression.text === secondStatement.name.text;
}
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoicHJlZml4LWNsYXNzZXMuanMiLCJzb3VyY2VSb290IjoiLi8iLCJzb3VyY2VzIjpbInBhY2thZ2VzL2FuZ3VsYXJfZGV2a2l0L2J1aWxkX29wdGltaXplci9zcmMvdHJhbnNmb3Jtcy9wcmVmaXgtY2xhc3Nlcy50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiOztBQUFBOzs7Ozs7R0FNRztBQUNILGlDQUFpQztBQUdqQywyQkFBa0MsT0FBZTtJQUMvQyxNQUFNLGVBQWUsR0FBRywwQ0FBMEMsQ0FBQztJQUNuRSxNQUFNLGdCQUFnQixHQUFHLDZCQUE2QixDQUFDO0lBQ3ZELE1BQU0sT0FBTyxHQUFHLGFBQWEsQ0FBQztJQUU5QixNQUFNLE9BQU8sR0FBRztRQUNkO1lBQ0UsR0FBRztZQUNILGVBQWUsRUFBRSxnQkFBZ0I7WUFDakMsSUFBSSxFQUFFLGdCQUFnQjtZQUN0QixvQkFBb0IsRUFBRSxPQUFPO1lBQzdCLGdCQUFnQjtZQUNoQiwwQkFBMEIsRUFBRSxPQUFPO1NBQ3BDO1FBQ0Q7WUFDRSxHQUFHO1lBQ0gsZUFBZSxFQUFFLGdCQUFnQjtZQUNqQyxJQUFJLEVBQUUsZ0JBQWdCO1lBQ3RCLDBCQUEwQixFQUFFLE9BQU87WUFDbkMsaUNBQWlDO1NBQ2xDO0tBQ0YsQ0FBQyxHQUFHLENBQUMsR0FBRyxDQUFDLEVBQUUsQ0FBQyxJQUFJLE1BQU0sQ0FBQyxHQUFHLENBQUMsR0FBRyxDQUFDLENBQUMsQ0FBQyxFQUFFLENBQUMsQ0FBQyxDQUFDLE1BQU0sQ0FBQyxDQUFDLElBQUksQ0FBQyxFQUFFLENBQUMsRUFBRSxHQUFHLENBQUMsQ0FBQyxDQUFDO0lBRS9ELE9BQU8sT0FBTyxDQUFDLElBQUksQ0FBQyxDQUFDLEtBQUssRUFBRSxFQUFFLENBQUMsS0FBSyxDQUFDLElBQUksQ0FBQyxPQUFPLENBQUMsQ0FBQyxDQUFDO0FBQ3RELENBQUM7QUF4QkQsOENBd0JDO0FBRUQsTUFBTSxrQkFBa0IsR0FBRyxRQUFRLENBQUM7QUFDcEMsTUFBTSxpQkFBaUIsR0FB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IjpbIi8qKlxuICogQGxpY2Vuc2VcbiAqIENvcHlyaWdodCBHb29nbGUgSW5jLiBBbGwgUmlnaHRzIFJlc2VydmVkLlxuICpcbiAqIFVzZSBvZiB0aGlzIHNvdXJjZSBjb2RlIGlzIGdvdmVybmVkIGJ5IGFuIE1JVC1zdHlsZSBsaWNlbnNlIHRoYXQgY2FuIGJlXG4gKiBmb3VuZCBpbiB0aGUgTElDRU5TRSBmaWxlIGF0IGh0dHBzOi8vYW5ndWxhci5pby9saWNlbnNlXG4gKi9cbmltcG9ydCAqIGFzIHRzIGZyb20gJ3R5cGVzY3JpcHQnO1xuXG5cbmV4cG9ydCBmdW5jdGlvbiB0ZXN0UHJlZml4Q2xhc3Nlcyhjb250ZW50OiBzdHJpbmcpIHtcbiAgY29uc3QgZXhwb3J0VmFyU2V0dGVyID0gLyg/OmV4cG9ydCApPyg/OnZhcnxjb25zdClcXHMrKFxcUyspXFxzKj1cXHMqLztcbiAgY29uc3QgbXVsdGlMaW5lQ29tbWVudCA9IC9cXHMqKD86XFwvXFwqW1xcc1xcU10qP1xcKlxcLyk/XFxzKi87XG4gIGNvbnN0IG5ld0xpbmUgPSAvXFxzKlxccj9cXG5cXHMqLztcblxuICBjb25zdCByZWdleGVzID0gW1xuICAgIFtcbiAgICAgIC9eLyxcbiAgICAgIGV4cG9ydFZhclNldHRlciwgbXVsdGlMaW5lQ29tbWVudCxcbiAgICAgIC9cXCgvLCBtdWx0aUxpbmVDb21tZW50LFxuICAgICAgL1xccypmdW5jdGlvbiBcXChcXCkgey8sIG5ld0xpbmUsXG4gICAgICBtdWx0aUxpbmVDb21tZW50LFxuICAgICAgL2Z1bmN0aW9uIFxcMVxcKFteXFwpXSpcXCkgXFx7LywgbmV3TGluZSxcbiAgICBdLFxuICAgIFtcbiAgICAgIC9eLyxcbiAgICAgIGV4cG9ydFZhclNldHRlciwgbXVsdGlMaW5lQ29tbWVudCxcbiAgICAgIC9cXCgvLCBtdWx0aUxpbmVDb21tZW50LFxuICAgICAgL1xccypmdW5jdGlvbiBcXChfc3VwZXJcXCkgey8sIG5ld0xpbmUsXG4gICAgICAvXFx3KlxcLj9fX2V4dGVuZHNcXChcXHcrLCBfc3VwZXJcXCk7LyxcbiAgICBdLFxuICBdLm1hcChhcnIgPT4gbmV3IFJlZ0V4cChhcnIubWFwKHggPT4geC5zb3VyY2UpLmpvaW4oJycpLCAnbScpKTtcblxuICByZXR1cm4gcmVnZXhlcy5zb21lKChyZWdleCkgPT4gcmVnZXgudGVzdChjb250ZW50KSk7XG59XG5cbmNvbnN0IHN1cGVyUGFyYW1ldGVyTmFtZSA9ICdfc3VwZXInO1xuY29uc3QgZXh0ZW5kc0hlbHBlck5hbWUgPSAnX19leHRlbmRzJztcblxuZXhwb3J0IGZ1bmN0aW9uIGdldFByZWZpeENsYXNzZXNUcmFuc2Zvcm1lcigpOiB0cy5UcmFuc2Zvcm1lckZhY3Rvcnk8dHMuU291cmNlRmlsZT4ge1xuICByZXR1cm4gKGNvbnRleHQ6IHRzLlRyYW5zZm9ybWF0aW9uQ29udGV4dCk6IHRzLlRyYW5zZm9ybWVyPHRzLlNvdXJjZUZpbGU+ID0+IHtcbiAgICBjb25zdCB0cmFuc2Zvcm1lcjogdHMuVHJhbnNmb3JtZXI8dHMuU291cmNlRmlsZT4gPSAoc2Y6IHRzLlNvdXJjZUZpbGUpID0+IHtcblxuICAgICAgY29uc3QgcHVyZUZ1bmN0aW9uQ29tbWVudCA9ICdAX19QVVJFX18nO1xuXG4gICAgICBjb25zdCB2aXNpdG9yOiB0cy5WaXNpdG9yID0gKG5vZGU6IHRzLk5vZGUpOiB0cy5WaXNpdFJlc3VsdDx0cy5Ob2RlPiA9PiB7XG5cbiAgICAgICAgLy8gQWRkIHB1cmUgY29tbWVudCB0byBkb3dubGV2ZWxlZCBjbGFzc2VzLlxuICAgICAgICBpZiAodHMuaXNWYXJpYWJsZVN0YXRlbWVudChub2RlKSAmJiBpc0Rvd25sZXZlbGVkQ2xhc3Mobm9kZSkpIHtcbiAgICAgICAgICBjb25zdCB2YXJEZWNsID0gbm9kZS5kZWNsYXJhdGlvbkxpc3QuZGVjbGFyYXRpb25zWzBdO1xuICAgICAgICAgIGNvbnN0IHZhckluaXRpYWxpemVyID0gdmFyRGVjbC5pbml0aWFsaXplciBhcyB0cy5FeHByZXNzaW9uO1xuXG4gICAgICAgICAgLy8gVXBkYXRlIG5vZGUgd2l0aCB0aGUgcHVyZSBjb21tZW50IGJlZm9yZSB0aGUgdmFyaWFibGUgZGVjbGFyYXRpb24gaW5pdGlhbGl6ZXIuXG4gICAgICAgICAgY29uc3QgbmV3Tm9kZSA9IHRzLnVwZGF0ZVZhcmlhYmxlU3RhdGVtZW50KFxuICAgICAgICAgICAgbm9kZSxcbiAgICAgICAgICAgIG5vZGUubW9kaWZpZXJzLFxuICAgICAgICAgICAgdHMudXBkYXRlVmFyaWFibGVEZWNsYXJhdGlvbkxpc3QoXG4gICAgICAgICAgICAgIG5vZGUuZGVjbGFyYXRpb25MaXN0LFxuICAgICAgICAgICAgICBbXG4gICAgICAgICAgICAgICAgdHMudXBkYXRlVmFyaWFibGVEZWNsYXJhdGlvbihcbiAgICAgICAgICAgICAgICAgIHZhckRlY2wsXG4gICAgICAgICAgICAgICAgICB2YXJEZWNsLm5hbWUsXG4gICAgICAgICAgICAgICAgICB2YXJEZWNsLnR5cGUsXG4gICAgICAgICAgICAgICAgICB0cy5hZGRTeW50aGV0aWNMZWFkaW5nQ29tbWVudChcbiAgICAgICAgICAgICAgICAgICAgdmFySW5pdGlhbGl6ZXIsXG4gICAgICAgICAgICAgICAgICAgIHRzLlN5bnRheEtpbmQuTXVsdGlMaW5lQ29tbWVudFRyaXZpYSxcbiAgICAgICAgICAgICAgICAgICAgcHVyZUZ1bmN0aW9uQ29tbWVudCxcbiAgICAgICAgICAgICAgICAgICAgZmFsc2UsXG4gICAgICAgICAgICAgICAgICApLFxuICAgICAgICAgICAgICAgICksXG4gICAgICAgICAgICAgIF0sXG4gICAgICAgICAgICApLFxuICAgICAgICAgICk7XG5cbiAgICAgICAgICAvLyBSZXBsYWNlIG5vZGUgd2l0aCBtb2RpZmllZCBvbmUuXG4gICAgICAgICAgcmV0dXJuIHRzLnZpc2l0RWFjaENoaWxkKG5ld05vZGUsIHZpc2l0b3IsIGNvbnRleHQpO1xuICAgICAgICB9XG5cbiAgICAgICAgLy8gT3RoZXJ3aXNlIHJldHVybiBub2RlIGFzIGlzLlxuICAgICAgICByZXR1cm4gdHMudmlzaXRFYWNoQ2hpbGQobm9kZSwgdmlzaXRvciwgY29udGV4dCk7XG4gICAgICB9O1xuXG4gICAgICByZXR1cm4gdHMudmlzaXRFYWNoQ2hpbGQoc2YsIHZpc2l0b3IsIGNvbnRleHQpO1xuICAgIH07XG5cbiAgICByZXR1cm4gdHJhbnNmb3JtZXI7XG4gIH07XG59XG5cbi8vIERldGVybWluZSBpZiBhIG5vZGUgbWF0Y2hlZCB0aGUgc3RydWN0dXJlIG9mIGEgZG93bmxldmVsZWQgVFMgY2xhc3MuXG5mdW5jdGlvbiBpc0Rvd25sZXZlbGVkQ2xhc3Mobm9kZTogdHMuTm9kZSk6IGJvb2xlYW4ge1xuXG4gIGlmICghdHMuaXNWYXJpYWJsZVN0YXRlbWVudChub2RlKSkge1xuICAgIHJldHVybiBmYWxzZTtcbiAgfVxuXG4gIGlmIChub2RlLmRlY2xhcmF0aW9uTGlzdC5kZWNsYXJhdGlvbnMubGVuZ3RoICE9PSAxKSB7XG4gICAgcmV0dXJuIGZhbHNlO1xuICB9XG5cbiAgY29uc3QgdmFyaWFibGVEZWNsYXJhdGlvbiA9IG5vZGUuZGVjbGFyYXRpb25MaXN0LmRlY2xhcmF0aW9uc1swXTtcblxuICBpZiAoIXRzLmlzSWRlbnRpZmllcih2YXJpYWJsZURlY2xhcmF0aW9uLm5hbWUpXG4gICAgICB8fCAhdmFyaWFibGVEZWNsYXJhdGlvbi5pbml0aWFsaXplcikge1xuICAgIHJldHVybiBmYWxzZTtcbiAgfVxuXG4gIGxldCBwb3RlbnRpYWxDbGFzcyA9IHZhcmlhYmxlRGVjbGFyYXRpb24uaW5pdGlhbGl6ZXI7XG5cbiAgLy8gVFMgMi4zIGhhcyBhbiB1bndyYXBwZWQgY2xhc3MgSUlGRVxuICAvLyBUUyAyLjQgdXNlcyBhIGZ1bmN0aW9uIGV4cHJlc3Npb24gd3JhcHBlclxuICAvLyBUUyAyLjUgdXNlcyBhbiBhcnJvdyBmdW5jdGlvbiB3cmFwcGVyXG4gIGlmICh0cy5pc1BhcmVudGhlc2l6ZWRFeHByZXNzaW9uKHBvdGVudGlhbENsYXNzKSkge1xuICAgIHBvdGVudGlhbENsYXNzID0gcG90ZW50aWFsQ2xhc3MuZXhwcmVzc2lvbjtcbiAgfVxuXG4gIGlmICghdHMuaXNDYWxsRXhwcmVzc2lvbihwb3RlbnRpYWxDbGFzcykgfHwgcG90ZW50aWFsQ2xhc3MuYXJndW1lbnRzLmxlbmd0aCA+IDEpIHtcbiAgICByZXR1cm4gZmFsc2U7XG4gIH1cblxuICBsZXQgd3JhcHBlckJvZHk6IHRzLkJsb2NrO1xuICBpZiAodHMuaXNGdW5jdGlvbkV4cHJlc3Npb24ocG90ZW50aWFsQ2xhc3MuZXhwcmVzc2lvbikpIHtcbiAgICB3cmFwcGVyQm9keSA9IHBvdGVudGlhbENsYXNzLmV4cHJlc3Npb24uYm9keTtcbiAgfSBlbHNlIGlmICh0cy5pc0Fycm93RnVuY3Rpb24ocG90ZW50aWFsQ2xhc3MuZXhwcmVzc2lvbilcbiAgICAgICAgICAgICAmJiB0cy5pc0Jsb2NrKHBvdGVudGlhbENsYXNzLmV4cHJlc3Npb24uYm9keSkpIHtcbiAgICB3cmFwcGVyQm9keSA9IHBvdGVudGlhbENsYXNzLmV4cHJlc3Npb24uYm9keTtcbiAgfSBlbHNlIHtcbiAgICByZXR1cm4gZmFsc2U7XG4gIH1cblxuICBpZiAod3JhcHBlckJvZHkuc3RhdGVtZW50cy5sZW5ndGggPT09IDApIHtcbiAgICByZXR1cm4gZmFsc2U7XG4gIH1cblxuICBjb25zdCBmdW5jdGlvbkV4cHJlc3Npb24gPSBwb3RlbnRpYWxDbGFzcy5leHByZXNzaW9uO1xuICBjb25zdCBmdW5jdGlvblN0YXRlbWVudHMgPSB3cmFwcGVyQm9keS5zdGF0ZW1lbnRzO1xuXG4gIC8vIG5lZWQgYSBtaW5pbXVtIG9mIHR3byBmb3IgYSBmdW5jdGlvbiBkZWNsYXJhdGlvbiBhbmQgcmV0dXJuIHN0YXRlbWVudFxuICBpZiAoZnVuY3Rpb25TdGF0ZW1lbnRzLmxlbmd0aCA8IDIpIHtcbiAgICByZXR1cm4gZmFsc2U7XG4gIH1cblxuICAvLyBUaGUgdmFyaWFibGUgbmFtZSBzaG91bGQgYmUgdGhlIGNsYXNzIG5hbWUuXG4gIGNvbnN0IGNsYXNzTmFtZSA9IHZhcmlhYmxlRGVjbGFyYXRpb24ubmFtZS50ZXh0O1xuXG4gIGNvbnN0IGZpcnN0U3RhdGVtZW50ID0gZnVuY3Rpb25TdGF0ZW1lbnRzWzBdO1xuXG4gIC8vIGZpbmQgcmV0dXJuIHN0YXRlbWVudCAtIG1heSBub3QgYmUgbGFzdCBzdGF0ZW1lbnRcbiAgbGV0IHJldHVyblN0YXRlbWVudDogdHMuUmV0dXJuU3RhdGVtZW50IHwgdW5kZWZpbmVkO1xuICBmb3IgKGxldCBpID0gZnVuY3Rpb25TdGF0ZW1lbnRzLmxlbmd0aCAtIDE7IGkgPiAwOyBpLS0pIHtcbiAgICBpZiAodHMuaXNSZXR1cm5TdGF0ZW1lbnQoZnVuY3Rpb25TdGF0ZW1lbnRzW2ldKSkge1xuICAgICAgcmV0dXJuU3RhdGVtZW50ID0gZnVuY3Rpb25TdGF0ZW1lbnRzW2ldIGFzIHRzLlJldHVyblN0YXRlbWVudDtcbiAgICAgIGJyZWFrO1xuICAgIH1cbiAgfVxuXG4gIGlmIChyZXR1cm5TdGF0ZW1lbnQgPT0gdW5kZWZpbmVkXG4gICAgICB8fCByZXR1cm5TdGF0ZW1lbnQuZXhwcmVzc2lvbiA9PSB1bmRlZmluZWRcbiAgICAgIHx8ICF0cy5pc0lkZW50aWZpZXIocmV0dXJuU3RhdGVtZW50LmV4cHJlc3Npb24pKSB7XG4gICAgcmV0dXJuIGZhbHNlO1xuICB9XG5cbiAgaWYgKGZ1bmN0aW9uRXhwcmVzc2lvbi5wYXJhbWV0ZXJzLmxlbmd0aCA9PT0gMCkge1xuICAgIC8vIHBvdGVudGlhbCBub24tZXh0ZW5kZWQgY2xhc3Mgb3Igd3JhcHBlZCBlczIwMTUgY2xhc3NcbiAgICByZXR1cm4gKHRzLmlzRnVuY3Rpb25EZWNsYXJhdGlvbihmaXJzdFN0YXRlbWVudCkgfHwgdHMuaXNDbGFzc0RlY2xhcmF0aW9uKGZpcnN0U3RhdGVtZW50KSlcbiAgICAgICAgICAgJiYgZmlyc3RTdGF0ZW1lbnQubmFtZSAhPT0gdW5kZWZpbmVkXG4gICAgICAgICAgICYmIGZpcnN0U3RhdGVtZW50Lm5hbWUudGV4dCA9PT0gY2xhc3NOYW1lXG4gICAgICAgICAgICYmIHJldHVyblN0YXRlbWVudC5leHByZXNzaW9uLnRleHQgPT09IGZpcnN0U3RhdGVtZW50Lm5hbWUudGV4dDtcbiAgfSBlbHNlIGlmIChmdW5jdGlvbkV4cHJlc3Npb24ucGFyYW1ldGVycy5sZW5ndGggIT09IDEpIHtcbiAgICByZXR1cm4gZmFsc2U7XG4gIH1cblxuICAvLyBQb3RlbnRpYWwgZXh0ZW5kZWQgY2xhc3NcblxuICBjb25zdCBmdW5jdGlvblBhcmFtZXRlciA9IGZ1bmN0aW9uRXhwcmVzc2lvbi5wYXJhbWV0ZXJzWzBdO1xuXG4gIGlmICghdHMuaXNJZGVudGlmaWVyKGZ1bmN0aW9uUGFyYW1ldGVyLm5hbWUpXG4gICAgICB8fCBmdW5jdGlvblBhcmFtZXRlci5uYW1lLnRleHQgIT09IHN1cGVyUGFyYW1ldGVyTmFtZSkge1xuICAgIHJldHVybiBmYWxzZTtcbiAgfVxuXG4gIGlmIChmdW5jdGlvblN0YXRlbWVudHMubGVuZ3RoIDwgMyB8fCAhdHMuaXNFeHByZXNzaW9uU3RhdGVtZW50KGZpcnN0U3RhdGVtZW50KSkge1xuICAgIHJldHVybiBmYWxzZTtcbiAgfVxuXG4gIGlmICghdHMuaXNDYWxsRXhwcmVzc2lvbihmaXJzdFN0YXRlbWVudC5leHByZXNzaW9uKSkge1xuICAgIHJldHVybiBmYWxzZTtcbiAgfVxuXG4gIGNvbnN0IGV4dGVuZENhbGxFeHByZXNzaW9uID0gZmlyc3RTdGF0ZW1lbnQuZXhwcmVzc2lvbjtcblxuICBsZXQgZnVuY3Rpb25OYW1lO1xuICBpZiAodHMuaXNJZGVudGlmaWVyKGV4dGVuZENhbGxFeHByZXNzaW9uLmV4cHJlc3Npb24pKSB7XG4gICAgZnVuY3Rpb25OYW1lID0gZXh0ZW5kQ2FsbEV4cHJlc3Npb24uZXhwcmVzc2lvbi50ZXh0O1xuICB9IGVsc2UgaWYgKHRzLmlzUHJvcGVydHlBY2Nlc3NFeHByZXNzaW9uKGV4dGVuZENhbGxFeHByZXNzaW9uLmV4cHJlc3Npb24pKSB7XG4gICAgZnVuY3Rpb25OYW1lID0gZXh0ZW5kQ2FsbEV4cHJlc3Npb24uZXhwcmVzc2lvbi5uYW1lLnRleHQ7XG4gIH1cblxuICBpZiAoIWZ1bmN0aW9uTmFtZSB8fCAhZnVuY3Rpb25OYW1lLmVuZHNXaXRoKGV4dGVuZHNIZWxwZXJOYW1lKSkge1xuICAgIHJldHVybiBmYWxzZTtcbiAgfVxuXG4gIGlmIChleHRlbmRDYWxsRXhwcmVzc2lvbi5hcmd1bWVudHMubGVuZ3RoID09PSAwKSB7XG4gICAgcmV0dXJuIGZhbHNlO1xuICB9XG5cbiAgY29uc3QgbGFzdEFyZ3VtZW50ID0gZXh0ZW5kQ2FsbEV4cHJlc3Npb24uYXJndW1lbnRzW2V4dGVuZENhbGxFeHByZXNzaW9uLmFyZ3VtZW50cy5sZW5ndGggLSAxXTtcblxuICBpZiAoIXRzLmlzSWRlbnRpZmllcihsYXN0QXJndW1lbnQpIHx8IGxhc3RBcmd1bWVudC50ZXh0ICE9PSBmdW5jdGlvblBhcmFtZXRlci5uYW1lLnRleHQpIHtcbiAgICByZXR1cm4gZmFsc2U7XG4gIH1cblxuICBjb25zdCBzZWNvbmRTdGF0ZW1lbnQgPSBmdW5jdGlvblN0YXRlbWVudHNbMV07XG5cbiAgcmV0dXJuIHRzLmlzRnVuY3Rpb25EZWNsYXJhdGlvbihzZWNvbmRTdGF0ZW1lbnQpXG4gICAgICAgICAmJiBzZWNvbmRTdGF0ZW1lbnQubmFtZSAhPT0gdW5kZWZpbmVkXG4gICAgICAgICAmJiBjbGFzc05hbWUuZW5kc1dpdGgoc2Vjb25kU3RhdGVtZW50Lm5hbWUudGV4dClcbiAgICAgICAgICYmIHJldHVyblN0YXRlbWVudC5leHByZXNzaW9uLnRleHQgPT09IHNlY29uZFN0YXRlbWVudC5uYW1lLnRleHQ7XG59XG4iXX0=