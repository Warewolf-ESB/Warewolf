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
const change_1 = require("./change");
/**
 * Add Import `import { symbolName } from fileName` if the import doesn't exit
 * already. Assumes fileToEdit can be resolved and accessed.
 * @param fileToEdit (file we want to add import to)
 * @param symbolName (item to import)
 * @param fileName (path to the file)
 * @param isDefault (if true, import follows style for importing default exports)
 * @return Change
 */
function insertImport(source, fileToEdit, symbolName, fileName, isDefault = false) {
    const rootNode = source;
    const allImports = findNodes(rootNode, ts.SyntaxKind.ImportDeclaration);
    // get nodes that map to import statements from the file fileName
    const relevantImports = allImports.filter(node => {
        // StringLiteral of the ImportDeclaration is the import file (fileName in this case).
        const importFiles = node.getChildren()
            .filter(child => child.kind === ts.SyntaxKind.StringLiteral)
            .map(n => n.text);
        return importFiles.filter(file => file === fileName).length === 1;
    });
    if (relevantImports.length > 0) {
        let importsAsterisk = false;
        // imports from import file
        const imports = [];
        relevantImports.forEach(n => {
            Array.prototype.push.apply(imports, findNodes(n, ts.SyntaxKind.Identifier));
            if (findNodes(n, ts.SyntaxKind.AsteriskToken).length > 0) {
                importsAsterisk = true;
            }
        });
        // if imports * from fileName, don't add symbolName
        if (importsAsterisk) {
            return new change_1.NoopChange();
        }
        const importTextNodes = imports.filter(n => n.text === symbolName);
        // insert import if it's not there
        if (importTextNodes.length === 0) {
            const fallbackPos = findNodes(relevantImports[0], ts.SyntaxKind.CloseBraceToken)[0].getStart() ||
                findNodes(relevantImports[0], ts.SyntaxKind.FromKeyword)[0].getStart();
            return insertAfterLastOccurrence(imports, `, ${symbolName}`, fileToEdit, fallbackPos);
        }
        return new change_1.NoopChange();
    }
    // no such import declaration exists
    const useStrict = findNodes(rootNode, ts.SyntaxKind.StringLiteral)
        .filter((n) => n.text === 'use strict');
    let fallbackPos = 0;
    if (useStrict.length > 0) {
        fallbackPos = useStrict[0].end;
    }
    const open = isDefault ? '' : '{ ';
    const close = isDefault ? '' : ' }';
    // if there are no imports or 'use strict' statement, insert import at beginning of file
    const insertAtBeginning = allImports.length === 0 && useStrict.length === 0;
    const separator = insertAtBeginning ? '' : ';\n';
    const toInsert = `${separator}import ${open}${symbolName}${close}` +
        ` from '${fileName}'${insertAtBeginning ? ';\n' : ''}`;
    return insertAfterLastOccurrence(allImports, toInsert, fileToEdit, fallbackPos, ts.SyntaxKind.StringLiteral);
}
exports.insertImport = insertImport;
/**
 * Find all nodes from the AST in the subtree of node of SyntaxKind kind.
 * @param node
 * @param kind
 * @param max The maximum number of items to return.
 * @return all nodes of kind, or [] if none is found
 */
function findNodes(node, kind, max = Infinity) {
    if (!node || max == 0) {
        return [];
    }
    const arr = [];
    if (node.kind === kind) {
        arr.push(node);
        max--;
    }
    if (max > 0) {
        for (const child of node.getChildren()) {
            findNodes(child, kind, max).forEach(node => {
                if (max > 0) {
                    arr.push(node);
                }
                max--;
            });
            if (max <= 0) {
                break;
            }
        }
    }
    return arr;
}
exports.findNodes = findNodes;
/**
 * Get all the nodes from a source.
 * @param sourceFile The source file object.
 * @returns {Observable<ts.Node>} An observable of all the nodes in the source.
 */
function getSourceNodes(sourceFile) {
    const nodes = [sourceFile];
    const result = [];
    while (nodes.length > 0) {
        const node = nodes.shift();
        if (node) {
            result.push(node);
            if (node.getChildCount(sourceFile) >= 0) {
                nodes.unshift(...node.getChildren());
            }
        }
    }
    return result;
}
exports.getSourceNodes = getSourceNodes;
function findNode(node, kind, text) {
    if (node.kind === kind && node.getText() === text) {
        // throw new Error(node.getText());
        return node;
    }
    let foundNode = null;
    ts.forEachChild(node, childNode => {
        foundNode = foundNode || findNode(childNode, kind, text);
    });
    return foundNode;
}
exports.findNode = findNode;
/**
 * Helper for sorting nodes.
 * @return function to sort nodes in increasing order of position in sourceFile
 */
function nodesByPosition(first, second) {
    return first.getStart() - second.getStart();
}
/**
 * Insert `toInsert` after the last occurence of `ts.SyntaxKind[nodes[i].kind]`
 * or after the last of occurence of `syntaxKind` if the last occurence is a sub child
 * of ts.SyntaxKind[nodes[i].kind] and save the changes in file.
 *
 * @param nodes insert after the last occurence of nodes
 * @param toInsert string to insert
 * @param file file to insert changes into
 * @param fallbackPos position to insert if toInsert happens to be the first occurence
 * @param syntaxKind the ts.SyntaxKind of the subchildren to insert after
 * @return Change instance
 * @throw Error if toInsert is first occurence but fall back is not set
 */
function insertAfterLastOccurrence(nodes, toInsert, file, fallbackPos, syntaxKind) {
    // sort() has a side effect, so make a copy so that we won't overwrite the parent's object.
    let lastItem = [...nodes].sort(nodesByPosition).pop();
    if (!lastItem) {
        throw new Error();
    }
    if (syntaxKind) {
        lastItem = findNodes(lastItem, syntaxKind).sort(nodesByPosition).pop();
    }
    if (!lastItem && fallbackPos == undefined) {
        throw new Error(`tried to insert ${toInsert} as first occurence with no fallback position`);
    }
    const lastItemPosition = lastItem ? lastItem.getEnd() : fallbackPos;
    return new change_1.InsertChange(file, lastItemPosition, toInsert);
}
exports.insertAfterLastOccurrence = insertAfterLastOccurrence;
function getContentOfKeyLiteral(_source, node) {
    if (node.kind == ts.SyntaxKind.Identifier) {
        return node.text;
    }
    else if (node.kind == ts.SyntaxKind.StringLiteral) {
        return node.text;
    }
    else {
        return null;
    }
}
exports.getContentOfKeyLiteral = getContentOfKeyLiteral;
function _angularImportsFromNode(node, _sourceFile) {
    const ms = node.moduleSpecifier;
    let modulePath;
    switch (ms.kind) {
        case ts.SyntaxKind.StringLiteral:
            modulePath = ms.text;
            break;
        default:
            return {};
    }
    if (!modulePath.startsWith('@angular/')) {
        return {};
    }
    if (node.importClause) {
        if (node.importClause.name) {
            // This is of the form `import Name from 'path'`. Ignore.
            return {};
        }
        else if (node.importClause.namedBindings) {
            const nb = node.importClause.namedBindings;
            if (nb.kind == ts.SyntaxKind.NamespaceImport) {
                // This is of the form `import * as name from 'path'`. Return `name.`.
                return {
                    [nb.name.text + '.']: modulePath,
                };
            }
            else {
                // This is of the form `import {a,b,c} from 'path'`
                const namedImports = nb;
                return namedImports.elements
                    .map((is) => is.propertyName ? is.propertyName.text : is.name.text)
                    .reduce((acc, curr) => {
                    acc[curr] = modulePath;
                    return acc;
                }, {});
            }
        }
        return {};
    }
    else {
        // This is of the form `import 'path';`. Nothing to do.
        return {};
    }
}
function getDecoratorMetadata(source, identifier, module) {
    const angularImports = findNodes(source, ts.SyntaxKind.ImportDeclaration)
        .map((node) => _angularImportsFromNode(node, source))
        .reduce((acc, current) => {
        for (const key of Object.keys(current)) {
            acc[key] = current[key];
        }
        return acc;
    }, {});
    return getSourceNodes(source)
        .filter(node => {
        return node.kind == ts.SyntaxKind.Decorator
            && node.expression.kind == ts.SyntaxKind.CallExpression;
    })
        .map(node => node.expression)
        .filter(expr => {
        if (expr.expression.kind == ts.SyntaxKind.Identifier) {
            const id = expr.expression;
            return id.getFullText(source) == identifier
                && angularImports[id.getFullText(source)] === module;
        }
        else if (expr.expression.kind == ts.SyntaxKind.PropertyAccessExpression) {
            // This covers foo.NgModule when importing * as foo.
            const paExpr = expr.expression;
            // If the left expression is not an identifier, just give up at that point.
            if (paExpr.expression.kind !== ts.SyntaxKind.Identifier) {
                return false;
            }
            const id = paExpr.name.text;
            const moduleId = paExpr.expression.getText(source);
            return id === identifier && (angularImports[moduleId + '.'] === module);
        }
        return false;
    })
        .filter(expr => expr.arguments[0]
        && expr.arguments[0].kind == ts.SyntaxKind.ObjectLiteralExpression)
        .map(expr => expr.arguments[0]);
}
exports.getDecoratorMetadata = getDecoratorMetadata;
function findClassDeclarationParent(node) {
    if (ts.isClassDeclaration(node)) {
        return node;
    }
    return node.parent && findClassDeclarationParent(node.parent);
}
/**
 * Given a source file with @NgModule class(es), find the name of the first @NgModule class.
 *
 * @param source source file containing one or more @NgModule
 * @returns the name of the first @NgModule, or `undefined` if none is found
 */
function getFirstNgModuleName(source) {
    // First, find the @NgModule decorators.
    const ngModulesMetadata = getDecoratorMetadata(source, 'NgModule', '@angular/core');
    if (ngModulesMetadata.length === 0) {
        return undefined;
    }
    // Then walk parent pointers up the AST, looking for the ClassDeclaration parent of the NgModule
    // metadata.
    const moduleClass = findClassDeclarationParent(ngModulesMetadata[0]);
    if (!moduleClass || !moduleClass.name) {
        return undefined;
    }
    // Get the class name of the module ClassDeclaration.
    return moduleClass.name.text;
}
exports.getFirstNgModuleName = getFirstNgModuleName;
function addSymbolToNgModuleMetadata(source, ngModulePath, metadataField, symbolName, importPath = null) {
    const nodes = getDecoratorMetadata(source, 'NgModule', '@angular/core');
    let node = nodes[0]; // tslint:disable-line:no-any
    // Find the decorator declaration.
    if (!node) {
        return [];
    }
    // Get all the children property assignment of object literals.
    const matchingProperties = node.properties
        .filter(prop => prop.kind == ts.SyntaxKind.PropertyAssignment)
        // Filter out every fields that's not "metadataField". Also handles string literals
        // (but not expressions).
        .filter((prop) => {
        const name = prop.name;
        switch (name.kind) {
            case ts.SyntaxKind.Identifier:
                return name.getText(source) == metadataField;
            case ts.SyntaxKind.StringLiteral:
                return name.text == metadataField;
        }
        return false;
    });
    // Get the last node of the array literal.
    if (!matchingProperties) {
        return [];
    }
    if (matchingProperties.length == 0) {
        // We haven't found the field in the metadata declaration. Insert a new field.
        const expr = node;
        let position;
        let toInsert;
        if (expr.properties.length == 0) {
            position = expr.getEnd() - 1;
            toInsert = `  ${metadataField}: [${symbolName}]\n`;
        }
        else {
            node = expr.properties[expr.properties.length - 1];
            position = node.getEnd();
            // Get the indentation of the last element, if any.
            const text = node.getFullText(source);
            const matches = text.match(/^\r?\n\s*/);
            if (matches && matches.length > 0) {
                toInsert = `,${matches[0]}${metadataField}: [${symbolName}]`;
            }
            else {
                toInsert = `, ${metadataField}: [${symbolName}]`;
            }
        }
        if (importPath !== null) {
            return [
                new change_1.InsertChange(ngModulePath, position, toInsert),
                insertImport(source, ngModulePath, symbolName.replace(/\..*$/, ''), importPath),
            ];
        }
        else {
            return [new change_1.InsertChange(ngModulePath, position, toInsert)];
        }
    }
    const assignment = matchingProperties[0];
    // If it's not an array, nothing we can do really.
    if (assignment.initializer.kind !== ts.SyntaxKind.ArrayLiteralExpression) {
        return [];
    }
    const arrLiteral = assignment.initializer;
    if (arrLiteral.elements.length == 0) {
        // Forward the property.
        node = arrLiteral;
    }
    else {
        node = arrLiteral.elements;
    }
    if (!node) {
        console.error('No app module found. Please add your new class to your component.');
        return [];
    }
    if (Array.isArray(node)) {
        const nodeArray = node;
        const symbolsArray = nodeArray.map(node => node.getText());
        if (symbolsArray.includes(symbolName)) {
            return [];
        }
        node = node[node.length - 1];
    }
    let toInsert;
    let position = node.getEnd();
    if (node.kind == ts.SyntaxKind.ObjectLiteralExpression) {
        // We haven't found the field in the metadata declaration. Insert a new
        // field.
        const expr = node;
        if (expr.properties.length == 0) {
            position = expr.getEnd() - 1;
            toInsert = `  ${metadataField}: [${symbolName}]\n`;
        }
        else {
            node = expr.properties[expr.properties.length - 1];
            position = node.getEnd();
            // Get the indentation of the last element, if any.
            const text = node.getFullText(source);
            if (text.match('^\r?\r?\n')) {
                toInsert = `,${text.match(/^\r?\n\s+/)[0]}${metadataField}: [${symbolName}]`;
            }
            else {
                toInsert = `, ${metadataField}: [${symbolName}]`;
            }
        }
    }
    else if (node.kind == ts.SyntaxKind.ArrayLiteralExpression) {
        // We found the field but it's empty. Insert it just before the `]`.
        position--;
        toInsert = `${symbolName}`;
    }
    else {
        // Get the indentation of the last element, if any.
        const text = node.getFullText(source);
        if (text.match(/^\r?\n/)) {
            toInsert = `,${text.match(/^\r?\n(\r?)\s+/)[0]}${symbolName}`;
        }
        else {
            toInsert = `, ${symbolName}`;
        }
    }
    if (importPath !== null) {
        return [
            new change_1.InsertChange(ngModulePath, position, toInsert),
            insertImport(source, ngModulePath, symbolName.replace(/\..*$/, ''), importPath),
        ];
    }
    return [new change_1.InsertChange(ngModulePath, position, toInsert)];
}
exports.addSymbolToNgModuleMetadata = addSymbolToNgModuleMetadata;
/**
 * Custom function to insert a declaration (component, pipe, directive)
 * into NgModule declarations. It also imports the component.
 */
function addDeclarationToModule(source, modulePath, classifiedName, importPath) {
    return addSymbolToNgModuleMetadata(source, modulePath, 'declarations', classifiedName, importPath);
}
exports.addDeclarationToModule = addDeclarationToModule;
/**
 * Custom function to insert an NgModule into NgModule imports. It also imports the module.
 */
function addImportToModule(source, modulePath, classifiedName, importPath) {
    return addSymbolToNgModuleMetadata(source, modulePath, 'imports', classifiedName, importPath);
}
exports.addImportToModule = addImportToModule;
/**
 * Custom function to insert a provider into NgModule. It also imports it.
 */
function addProviderToModule(source, modulePath, classifiedName, importPath) {
    return addSymbolToNgModuleMetadata(source, modulePath, 'providers', classifiedName, importPath);
}
exports.addProviderToModule = addProviderToModule;
/**
 * Custom function to insert an export into NgModule. It also imports it.
 */
function addExportToModule(source, modulePath, classifiedName, importPath) {
    return addSymbolToNgModuleMetadata(source, modulePath, 'exports', classifiedName, importPath);
}
exports.addExportToModule = addExportToModule;
/**
 * Custom function to insert an export into NgModule. It also imports it.
 */
function addBootstrapToModule(source, modulePath, classifiedName, importPath) {
    return addSymbolToNgModuleMetadata(source, modulePath, 'bootstrap', classifiedName, importPath);
}
exports.addBootstrapToModule = addBootstrapToModule;
/**
 * Custom function to insert an entryComponent into NgModule. It also imports it.
 */
function addEntryComponentToModule(source, modulePath, classifiedName, importPath) {
    return addSymbolToNgModuleMetadata(source, modulePath, 'entryComponents', classifiedName, importPath);
}
exports.addEntryComponentToModule = addEntryComponentToModule;
/**
 * Determine if an import already exists.
 */
function isImported(source, classifiedName, importPath) {
    const allNodes = getSourceNodes(source);
    const matchingNodes = allNodes
        .filter(node => node.kind === ts.SyntaxKind.ImportDeclaration)
        .filter((imp) => imp.moduleSpecifier.kind === ts.SyntaxKind.StringLiteral)
        .filter((imp) => {
        return imp.moduleSpecifier.text === importPath;
    })
        .filter((imp) => {
        if (!imp.importClause) {
            return false;
        }
        const nodes = findNodes(imp.importClause, ts.SyntaxKind.ImportSpecifier)
            .filter(n => n.getText() === classifiedName);
        return nodes.length > 0;
    });
    return matchingNodes.length > 0;
}
exports.isImported = isImported;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiYXN0LXV0aWxzLmpzIiwic291cmNlUm9vdCI6Ii4vIiwic291cmNlcyI6WyJwYWNrYWdlcy9zY2hlbWF0aWNzL2FuZ3VsYXIvdXRpbGl0eS9hc3QtdXRpbHMudHMiXSwibmFtZXMiOltdLCJtYXBwaW5ncyI6Ijs7QUFBQTs7Ozs7O0dBTUc7QUFDSCxpQ0FBaUM7QUFDakMscUNBQTREO0FBRzVEOzs7Ozs7OztHQVFHO0FBQ0gsc0JBQTZCLE1BQXFCLEVBQUUsVUFBa0IsRUFBRSxVQUFrQixFQUM3RCxRQUFnQixFQUFFLFNBQVMsR0FBRyxLQUFLO0lBQzlELE1BQU0sUUFBUSxHQUFHLE1BQU0sQ0FBQztJQUN4QixNQUFNLFVBQVUsR0FBRyxTQUFTLENBQUMsUUFBUSxFQUFFLEVBQUUsQ0FBQyxVQUFVLENBQUMsaUJBQWlCLENBQUMsQ0FBQztJQUV4RSxpRUFBaUU7SUFDakUsTUFBTSxlQUFlLEdBQUcsVUFBVSxDQUFDLE1BQU0sQ0FBQyxJQUFJLENBQUMsRUFBRTtRQUMvQyxxRkFBcUY7UUFDckYsTUFBTSxXQUFXLEdBQUcsSUFBSSxDQUFDLFdBQVcsRUFBRTthQUNuQyxNQUFNLENBQUMsS0FBSyxDQUFDLEVBQUUsQ0FBQyxLQUFLLENBQUMsSUFBSSxLQUFLLEVBQUUsQ0FBQyxVQUFVLENBQUMsYUFBYSxDQUFDO2FBQzNELEdBQUcsQ0FBQyxDQUFDLENBQUMsRUFBRSxDQUFFLENBQXNCLENBQUMsSUFBSSxDQUFDLENBQUM7UUFFMUMsT0FBTyxXQUFXLENBQUMsTUFBTSxDQUFDLElBQUksQ0FBQyxFQUFFLENBQUMsSUFBSSxLQUFLLFFBQVEsQ0FBQyxDQUFDLE1BQU0sS0FBSyxDQUFDLENBQUM7SUFDcEUsQ0FBQyxDQUFDLENBQUM7SUFFSCxJQUFJLGVBQWUsQ0FBQyxNQUFNLEdBQUcsQ0FBQyxFQUFFO1FBQzlCLElBQUksZUFBZSxHQUFHLEtBQUssQ0FBQztRQUM1QiwyQkFBMkI7UUFDM0IsTUFBTSxPQUFPLEdBQWMsRUFBRSxDQUFDO1FBQzlCLGVBQWUsQ0FBQyxP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RDs7R0FFRztBQUNILG9CQUEyQixNQUFxQixFQUNyQixjQUFzQixFQUN0QixVQUFrQjtJQUMzQyxNQUFNLFFBQVEsR0FBRyxjQUFjLENBQUMsTUFBTSxDQUFDLENBQUM7SUFDeEMsTUFBTSxhQUFhLEdBQUcsUUFBUTtTQUMzQixNQUFNLENBQUMsSUFBSSxDQUFDLEVBQUUsQ0FBQyxJQUFJLENBQUMsSUFBSSxLQUFLLEVBQUUsQ0FBQyxVQUFVLENBQUMsaUJBQWlCLENBQUM7U0FDN0QsTUFBTSxDQUFDLENBQUMsR0FBeUIsRUFBRSxFQUFFLENBQUMsR0FBRyxDQUFDLGVBQWUsQ0FBQyxJQUFJLEtBQUssRUFBRSxDQUFDLFVBQVUsQ0FBQyxhQUFhLENBQUM7U0FDL0YsTUFBTSxDQUFDLENBQUMsR0FBeUIsRUFBRSxFQUFFO1FBQ3BDLE9BQVEsR0FBRyxDQUFDLGVBQW9DLENBQUMsSUFBSSxLQUFLLFVBQVUsQ0FBQztJQUN2RSxDQUFDLENBQUM7U0FDRCxNQUFNLENBQUMsQ0FBQyxHQUF5QixFQUFFLEVBQUU7UUFDcEMsSUFBSSxDQUFDLEdBQUcsQ0FBQyxZQUFZLEVBQUU7WUFDckIsT0FBTyxLQUFLLENBQUM7U0FDZDtRQUNELE1BQU0sS0FBSyxHQUFHLFNBQVMsQ0FBQyxHQUFHLENBQUMsWUFBWSxFQUFFLEVBQUUsQ0FBQyxVQUFVLENBQUMsZUFBZSxDQUFDO2FBQ3JFLE1BQU0sQ0FBQyxDQUFDLENBQUMsRUFBRSxDQUFDLENBQUMsQ0FBQyxPQUFPLEVBQUUsS0FBSyxjQUFjLENBQUMsQ0FBQztRQUUvQyxPQUFPLEtBQUssQ0FBQyxNQUFNLEdBQUcsQ0FBQyxDQUFDO0lBQzFCLENBQUMsQ0FBQyxDQUFDO0lBRUwsT0FBTyxhQUFhLENBQUMsTUFBTSxHQUFHLENBQUMsQ0FBQztBQUNsQyxDQUFDO0FBckJELGdDQXFCQyIsInNvdXJjZXNDb250ZW50IjpbIi8qKlxuICogQGxpY2Vuc2VcbiAqIENvcHlyaWdodCBHb29nbGUgSW5jLiBBbGwgUmlnaHRzIFJlc2VydmVkLlxuICpcbiAqIFVzZSBvZiB0aGlzIHNvdXJjZSBjb2RlIGlzIGdvdmVybmVkIGJ5IGFuIE1JVC1zdHlsZSBsaWNlbnNlIHRoYXQgY2FuIGJlXG4gKiBmb3VuZCBpbiB0aGUgTElDRU5TRSBmaWxlIGF0IGh0dHBzOi8vYW5ndWxhci5pby9saWNlbnNlXG4gKi9cbmltcG9ydCAqIGFzIHRzIGZyb20gJ3R5cGVzY3JpcHQnO1xuaW1wb3J0IHsgQ2hhbmdlLCBJbnNlcnRDaGFuZ2UsIE5vb3BDaGFuZ2UgfSBmcm9tICcuL2NoYW5nZSc7XG5cblxuLyoqXG4gKiBBZGQgSW1wb3J0IGBpbXBvcnQgeyBzeW1ib2xOYW1lIH0gZnJvbSBmaWxlTmFtZWAgaWYgdGhlIGltcG9ydCBkb2Vzbid0IGV4aXRcbiAqIGFscmVhZHkuIEFzc3VtZXMgZmlsZVRvRWRpdCBjYW4gYmUgcmVzb2x2ZWQgYW5kIGFjY2Vzc2VkLlxuICogQHBhcmFtIGZpbGVUb0VkaXQgKGZpbGUgd2Ugd2FudCB0byBhZGQgaW1wb3J0IHRvKVxuICogQHBhcmFtIHN5bWJvbE5hbWUgKGl0ZW0gdG8gaW1wb3J0KVxuICogQHBhcmFtIGZpbGVOYW1lIChwYXRoIHRvIHRoZSBmaWxlKVxuICogQHBhcmFtIGlzRGVmYXVsdCAoaWYgdHJ1ZSwgaW1wb3J0IGZvbGxvd3Mgc3R5bGUgZm9yIGltcG9ydGluZyBkZWZhdWx0IGV4cG9ydHMpXG4gKiBAcmV0dXJuIENoYW5nZVxuICovXG5leHBvcnQgZnVuY3Rpb24gaW5zZXJ0SW1wb3J0KHNvdXJjZTogdHMuU291cmNlRmlsZSwgZmlsZVRvRWRpdDogc3RyaW5nLCBzeW1ib2xOYW1lOiBzdHJpbmcsXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgIGZpbGVOYW1lOiBzdHJpbmcsIGlzRGVmYXVsdCA9IGZhbHNlKTogQ2hhbmdlIHtcbiAgY29uc3Qgcm9vdE5vZGUgPSBzb3VyY2U7XG4gIGNvbnN0IGFsbEltcG9ydHMgPSBmaW5kTm9kZXMocm9vdE5vZGUsIHRzLlN5bnRheEtpbmQuSW1wb3J0RGVjbGFyYXRpb24pO1xuXG4gIC8vIGdldCBub2RlcyB0aGF0IG1hcCB0byBpbXBvcnQgc3RhdGVtZW50cyBmcm9tIHRoZSBmaWxlIGZpbGVOYW1lXG4gIGNvbnN0IHJlbGV2YW50SW1wb3J0cyA9IGFsbEltcG9ydHMuZmlsdGVyKG5vZGUgPT4ge1xuICAgIC8vIFN0cmluZ0xpdGVyYWwgb2YgdGhlIEltcG9ydERlY2xhcmF0aW9uIGlzIHRoZSBpbXBvcnQgZmlsZSAoZmlsZU5hbWUgaW4gdGhpcyBjYXNlKS5cbiAgICBjb25zdCBpbXBvcnRGaWxlcyA9IG5vZGUuZ2V0Q2hpbGRyZW4oKVxuICAgICAgLmZpbHRlcihjaGlsZCA9PiBjaGlsZC5raW5kID09PSB0cy5TeW50YXhLaW5kLlN0cmluZ0xpdGVyYWwpXG4gICAgICAubWFwKG4gPT4gKG4gYXMgdHMuU3RyaW5nTGl0ZXJhbCkudGV4dCk7XG5cbiAgICByZXR1cm4gaW1wb3J0RmlsZXMuZmlsdGVyKGZpbGUgPT4gZmlsZSA9PT0gZmlsZU5hbWUpLmxlbmd0aCA9PT0gMTtcbiAgfSk7XG5cbiAgaWYgKHJlbGV2YW50SW1wb3J0cy5sZW5ndGggPiAwKSB7XG4gICAgbGV0IGltcG9ydHNBc3RlcmlzayA9IGZhbHNlO1xuICAgIC8vIGltcG9ydHMgZnJvbSBpbXBvcnQgZmlsZVxuICAgIGNvbnN0IGltcG9ydHM6IHRzLk5vZGVbXSA9IFtdO1xuICAgIHJlbGV2YW50SW1wb3J0cy5mb3JFYWNoKG4gPT4ge1xuICAgICAgQXJyYXkucHJvdG90eXBlLnB1c2guYXBwbHkoaW1wb3J0cywgZmluZE5vZGVzKG4sIHRzLlN5bnRheEtpbmQuSWRlbnRpZmllcikpO1xuICAgICAgaWYgKGZpbmROb2RlcyhuLCB0cy5TeW50YXhLaW5kLkFzdGVyaXNrVG9rZW4pLmxlbmd0aCA+IDApIHtcbiAgICAgICAgaW1wb3J0c0FzdGVyaXNrID0gdHJ1ZTtcbiAgICAgIH1cbiAgICB9KTtcblxuICAgIC8vIGlmIGltcG9ydHMgKiBmcm9tIGZpbGVOYW1lLCBkb24ndCBhZGQgc3ltYm9sTmFtZVxuICAgIGlmIChpbXBvcnRzQXN0ZXJpc2spIHtcbiAgICAgIHJldHVybiBuZXcgTm9vcENoYW5nZSgpO1xuICAgIH1cblxuICAgIGNvbnN0IGltcG9ydFRleHROb2RlcyA9IGltcG9ydHMuZmlsdGVyKG4gPT4gKG4gYXMgdHMuSWRlbnRpZmllcikudGV4dCA9PT0gc3ltYm9sTmFtZSk7XG5cbiAgICAvLyBpbnNlcnQgaW1wb3J0IGlmIGl0J3Mgbm90IHRoZXJlXG4gICAgaWYgKGltcG9ydFRleHROb2Rlcy5sZW5ndGggPT09IDApIHtcbiAgICAgIGNvbnN0IGZhbGxiYWNrUG9zID1cbiAgICAgICAgZmluZE5vZGVzKHJlbGV2YW50SW1wb3J0c1swXSwgdHMuU3ludGF4S2luZC5DbG9zZUJyYWNlVG9rZW4pWzBdLmdldFN0YXJ0KCkgfHxcbiAgICAgICAgZmluZE5vZGVzKHJlbGV2YW50SW1wb3J0c1swXSwgdHMuU3ludGF4S2luZC5Gcm9tS2V5d29yZClbMF0uZ2V0U3RhcnQoKTtcblxuICAgICAgcmV0dXJuIGluc2VydEFmdGVyTGFzdE9jY3VycmVuY2UoaW1wb3J0cywgYCwgJHtzeW1ib2xOYW1lfWAsIGZpbGVUb0VkaXQsIGZhbGxiYWNrUG9zKTtcbiAgICB9XG5cbiAgICByZXR1cm4gbmV3IE5vb3BDaGFuZ2UoKTtcbiAgfVxuXG4gIC8vIG5vIHN1Y2ggaW1wb3J0IGRlY2xhcmF0aW9uIGV4aXN0c1xuICBjb25zdCB1c2VTdHJpY3QgPSBmaW5kTm9kZXMocm9vdE5vZGUsIHRzLlN5bnRheEtpbmQuU3RyaW5nTGl0ZXJhbClcbiAgICAuZmlsdGVyKChuOiB0cy5TdHJpbmdMaXRlcmFsKSA9PiBuLnRleHQgPT09ICd1c2Ugc3RyaWN0Jyk7XG4gIGxldCBmYWxsYmFja1BvcyA9IDA7XG4gIGlmICh1c2VTdHJpY3QubGVuZ3RoID4gMCkge1xuICAgIGZhbGxiYWNrUG9zID0gdXNlU3RyaWN0WzBdLmVuZDtcbiAgfVxuICBjb25zdCBvcGVuID0gaXNEZWZhdWx0ID8gJycgOiAneyAnO1xuICBjb25zdCBjbG9zZSA9IGlzRGVmYXVsdCA/ICcnIDogJyB9JztcbiAgLy8gaWYgdGhlcmUgYXJlIG5vIGltcG9ydHMgb3IgJ3VzZSBzdHJpY3QnIHN0YXRlbWVudCwgaW5zZXJ0IGltcG9ydCBhdCBiZWdpbm5pbmcgb2YgZmlsZVxuICBjb25zdCBpbnNlcnRBdEJlZ2lubmluZyA9IGFsbEltcG9ydHMubGVuZ3RoID09PSAwICYmIHVzZVN0cmljdC5sZW5ndGggPT09IDA7XG4gIGNvbnN0IHNlcGFyYXRvciA9IGluc2VydEF0QmVnaW5uaW5nID8gJycgOiAnO1xcbic7XG4gIGNvbnN0IHRvSW5zZXJ0ID0gYCR7c2VwYXJhdG9yfWltcG9ydCAke29wZW59JHtzeW1ib2xOYW1lfSR7Y2xvc2V9YCArXG4gICAgYCBmcm9tICcke2ZpbGVOYW1lfScke2luc2VydEF0QmVnaW5uaW5nID8gJztcXG4nIDogJyd9YDtcblxuICByZXR1cm4gaW5zZXJ0QWZ0ZXJMYXN0T2NjdXJyZW5jZShcbiAgICBhbGxJbXBvcnRzLFxuICAgIHRvSW5zZXJ0LFxuICAgIGZpbGVUb0VkaXQsXG4gICAgZmFsbGJhY2tQb3MsXG4gICAgdHMuU3ludGF4S2luZC5TdHJpbmdMaXRlcmFsLFxuICApO1xufVxuXG5cbi8qKlxuICogRmluZCBhbGwgbm9kZXMgZnJvbSB0aGUgQVNUIGluIHRoZSBzdWJ0cmVlIG9mIG5vZGUgb2YgU3ludGF4S2luZCBraW5kLlxuICogQHBhcmFtIG5vZGVcbiAqIEBwYXJhbSBraW5kXG4gKiBAcGFyYW0gbWF4IFRoZSBtYXhpbXVtIG51bWJlciBvZiBpdGVtcyB0byByZXR1cm4uXG4gKiBAcmV0dXJuIGFsbCBub2RlcyBvZiBraW5kLCBvciBbXSBpZiBub25lIGlzIGZvdW5kXG4gKi9cbmV4cG9ydCBmdW5jdGlvbiBmaW5kTm9kZXMobm9kZTogdHMuTm9kZSwga2luZDogdHMuU3ludGF4S2luZCwgbWF4ID0gSW5maW5pdHkpOiB0cy5Ob2RlW10ge1xuICBpZiAoIW5vZGUgfHwgbWF4ID09IDApIHtcbiAgICByZXR1cm4gW107XG4gIH1cblxuICBjb25zdCBhcnI6IHRzLk5vZGVbXSA9IFtdO1xuICBpZiAobm9kZS5raW5kID09PSBraW5kKSB7XG4gICAgYXJyLnB1c2gobm9kZSk7XG4gICAgbWF4LS07XG4gIH1cbiAgaWYgKG1heCA+IDApIHtcbiAgICBmb3IgKGNvbnN0IGNoaWxkIG9mIG5vZGUuZ2V0Q2hpbGRyZW4oKSkge1xuICAgICAgZmluZE5vZGVzKGNoaWxkLCBraW5kLCBtYXgpLmZvckVhY2gobm9kZSA9PiB7XG4gICAgICAgIGlmIChtYXggPiAwKSB7XG4gICAgICAgICAgYXJyLnB1c2gobm9kZSk7XG4gICAgICAgIH1cbiAgICAgICAgbWF4LS07XG4gICAgICB9KTtcblxuICAgICAgaWYgKG1heCA8PSAwKSB7XG4gICAgICAgIGJyZWFrO1xuICAgICAgfVxuICAgIH1cbiAgfVxuXG4gIHJldHVybiBhcnI7XG59XG5cblxuLyoqXG4gKiBHZXQgYWxsIHRoZSBub2RlcyBmcm9tIGEgc291cmNlLlxuICogQHBhcmFtIHNvdXJjZUZpbGUgVGhlIHNvdXJjZSBmaWxlIG9iamVjdC5cbiAqIEByZXR1cm5zIHtPYnNlcnZhYmxlPHRzLk5vZGU+fSBBbiBvYnNlcnZhYmxlIG9mIGFsbCB0aGUgbm9kZXMgaW4gdGhlIHNvdXJjZS5cbiAqL1xuZXhwb3J0IGZ1bmN0aW9uIGdldFNvdXJjZU5vZGVzKHNvdXJjZUZpbGU6IHRzLlNvdXJjZUZpbGUpOiB0cy5Ob2RlW10ge1xuICBjb25zdCBub2RlczogdHMuTm9kZVtdID0gW3NvdXJjZUZpbGVdO1xuICBjb25zdCByZXN1bHQgPSBbXTtcblxuICB3aGlsZSAobm9kZXMubGVuZ3RoID4gMCkge1xuICAgIGNvbnN0IG5vZGUgPSBub2Rlcy5zaGlmdCgpO1xuXG4gICAgaWYgKG5vZGUpIHtcbiAgICAgIHJlc3VsdC5wdXNoKG5vZGUpO1xuICAgICAgaWYgKG5vZGUuZ2V0Q2hpbGRDb3VudChzb3VyY2VGaWxlKSA+PSAwKSB7XG4gICAgICAgIG5vZGVzLnVuc2hpZnQoLi4ubm9kZS5nZXRDaGlsZHJlbigpKTtcbiAgICAgIH1cbiAgICB9XG4gIH1cblxuICByZXR1cm4gcmVzdWx0O1xufVxuXG5leHBvcnQgZnVuY3Rpb24gZmluZE5vZGUobm9kZTogdHMuTm9kZSwga2luZDogdHMuU3ludGF4S2luZCwgdGV4dDogc3RyaW5nKTogdHMuTm9kZSB8IG51bGwge1xuICBpZiAobm9kZS5raW5kID09PSBraW5kICYmIG5vZGUuZ2V0VGV4dCgpID09PSB0ZXh0KSB7XG4gICAgLy8gdGhyb3cgbmV3IEVycm9yKG5vZGUuZ2V0VGV4dCgpKTtcbiAgICByZXR1cm4gbm9kZTtcbiAgfVxuXG4gIGxldCBmb3VuZE5vZGU6IHRzLk5vZGUgfCBudWxsID0gbnVsbDtcbiAgdHMuZm9yRWFjaENoaWxkKG5vZGUsIGNoaWxkTm9kZSA9PiB7XG4gICAgZm91bmROb2RlID0gZm91bmROb2RlIHx8IGZpbmROb2RlKGNoaWxkTm9kZSwga2luZCwgdGV4dCk7XG4gIH0pO1xuXG4gIHJldHVybiBmb3VuZE5vZGU7XG59XG5cblxuLyoqXG4gKiBIZWxwZXIgZm9yIHNvcnRpbmcgbm9kZXMuXG4gKiBAcmV0dXJuIGZ1bmN0aW9uIHRvIHNvcnQgbm9kZXMgaW4gaW5jcmVhc2luZyBvcmRlciBvZiBwb3NpdGlvbiBpbiBzb3VyY2VGaWxlXG4gKi9cbmZ1bmN0aW9uIG5vZGVzQnlQb3NpdGlvbihmaXJzdDogdHMuTm9kZSwgc2Vjb25kOiB0cy5Ob2RlKTogbnVtYmVyIHtcbiAgcmV0dXJuIGZpcnN0LmdldFN0YXJ0KCkgLSBzZWNvbmQuZ2V0U3RhcnQoKTtcbn1cblxuXG4vKipcbiAqIEluc2VydCBgdG9JbnNlcnRgIGFmdGVyIHRoZSBsYXN0IG9jY3VyZW5jZSBvZiBgdHMuU3ludGF4S2luZFtub2Rlc1tpXS5raW5kXWBcbiAqIG9yIGFmdGVyIHRoZSBsYXN0IG9mIG9jY3VyZW5jZSBvZiBgc3ludGF4S2luZGAgaWYgdGhlIGxhc3Qgb2NjdXJlbmNlIGlzIGEgc3ViIGNoaWxkXG4gKiBvZiB0cy5TeW50YXhLaW5kW25vZGVzW2ldLmtpbmRdIGFuZCBzYXZlIHRoZSBjaGFuZ2VzIGluIGZpbGUuXG4gKlxuICogQHBhcmFtIG5vZGVzIGluc2VydCBhZnRlciB0aGUgbGFzdCBvY2N1cmVuY2Ugb2Ygbm9kZXNcbiAqIEBwYXJhbSB0b0luc2VydCBzdHJpbmcgdG8gaW5zZXJ0XG4gKiBAcGFyYW0gZmlsZSBmaWxlIHRvIGluc2VydCBjaGFuZ2VzIGludG9cbiAqIEBwYXJhbSBmYWxsYmFja1BvcyBwb3NpdGlvbiB0byBpbnNlcnQgaWYgdG9JbnNlcnQgaGFwcGVucyB0byBiZSB0aGUgZmlyc3Qgb2NjdXJlbmNlXG4gKiBAcGFyYW0gc3ludGF4S2luZCB0aGUgdHMuU3ludGF4S2luZCBvZiB0aGUgc3ViY2hpbGRyZW4gdG8gaW5zZXJ0IGFmdGVyXG4gKiBAcmV0dXJuIENoYW5nZSBpbnN0YW5jZVxuICogQHRocm93IEVycm9yIGlmIHRvSW5zZXJ0IGlzIGZpcnN0IG9jY3VyZW5jZSBidXQgZmFsbCBiYWNrIGlzIG5vdCBzZXRcbiAqL1xuZXhwb3J0IGZ1bmN0aW9uIGluc2VydEFmdGVyTGFzdE9jY3VycmVuY2Uobm9kZXM6IHRzLk5vZGVbXSxcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIHRvSW5zZXJ0OiBzdHJpbmcsXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBmaWxlOiBzdHJpbmcsXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBmYWxsYmFja1BvczogbnVtYmVyLFxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgc3ludGF4S2luZD86IHRzLlN5bnRheEtpbmQpOiBDaGFuZ2Uge1xuICAvLyBzb3J0KCkgaGFzIGEgc2lkZSBlZmZlY3QsIHNvIG1ha2UgYSBjb3B5IHNvIHRoYXQgd2Ugd29uJ3Qgb3ZlcndyaXRlIHRoZSBwYXJlbnQncyBvYmplY3QuXG4gIGxldCBsYXN0SXRlbSA9IFsuLi5ub2Rlc10uc29ydChub2Rlc0J5UG9zaXRpb24pLnBvcCgpO1xuICBpZiAoIWxhc3RJdGVtKSB7XG4gICAgdGhyb3cgbmV3IEVycm9yKCk7XG4gIH1cbiAgaWYgKHN5bnRheEtpbmQpIHtcbiAgICBsYXN0SXRlbSA9IGZpbmROb2RlcyhsYXN0SXRlbSwgc3ludGF4S2luZCkuc29ydChub2Rlc0J5UG9zaXRpb24pLnBvcCgpO1xuICB9XG4gIGlmICghbGFzdEl0ZW0gJiYgZmFsbGJhY2tQb3MgPT0gdW5kZWZpbmVkKSB7XG4gICAgdGhyb3cgbmV3IEVycm9yKGB0cmllZCB0byBpbnNlcnQgJHt0b0luc2VydH0gYXMgZmlyc3Qgb2NjdXJlbmNlIHdpdGggbm8gZmFsbGJhY2sgcG9zaXRpb25gKTtcbiAgfVxuICBjb25zdCBsYXN0SXRlbVBvc2l0aW9uOiBudW1iZXIgPSBsYXN0SXRlbSA/IGxhc3RJdGVtLmdldEVuZCgpIDogZmFsbGJhY2tQb3M7XG5cbiAgcmV0dXJuIG5ldyBJbnNlcnRDaGFuZ2UoZmlsZSwgbGFzdEl0ZW1Qb3NpdGlvbiwgdG9JbnNlcnQpO1xufVxuXG5cbmV4cG9ydCBmdW5jdGlvbiBnZXRDb250ZW50T2ZLZXlMaXRlcmFsKF9zb3VyY2U6IHRzLlNvdXJjZUZpbGUsIG5vZGU6IHRzLk5vZGUpOiBzdHJpbmcgfCBudWxsIHtcbiAgaWYgKG5vZGUua2luZCA9PSB0cy5TeW50YXhLaW5kLklkZW50aWZpZXIpIHtcbiAgICByZXR1cm4gKG5vZGUgYXMgdHMuSWRlbnRpZmllcikudGV4dDtcbiAgfSBlbHNlIGlmIChub2RlLmtpbmQgPT0gdHMuU3ludGF4S2luZC5TdHJpbmdMaXRlcmFsKSB7XG4gICAgcmV0dXJuIChub2RlIGFzIHRzLlN0cmluZ0xpdGVyYWwpLnRleHQ7XG4gIH0gZWxzZSB7XG4gICAgcmV0dXJuIG51bGw7XG4gIH1cbn1cblxuXG5mdW5jdGlvbiBfYW5ndWxhckltcG9ydHNGcm9tTm9kZShub2RlOiB0cy5JbXBvcnREZWNsYXJhdGlvbixcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIF9zb3VyY2VGaWxlOiB0cy5Tb3VyY2VGaWxlKToge1tuYW1lOiBzdHJpbmddOiBzdHJpbmd9IHtcbiAgY29uc3QgbXMgPSBub2RlLm1vZHVsZVNwZWNpZmllcjtcbiAgbGV0IG1vZHVsZVBhdGg6IHN0cmluZztcbiAgc3dpdGNoIChtcy5raW5kKSB7XG4gICAgY2FzZSB0cy5TeW50YXhLaW5kLlN0cmluZ0xpdGVyYWw6XG4gICAgICBtb2R1bGVQYXRoID0gKG1zIGFzIHRzLlN0cmluZ0xpdGVyYWwpLnRleHQ7XG4gICAgICBicmVhaztcbiAgICBkZWZhdWx0OlxuICAgICAgcmV0dXJuIHt9O1xuICB9XG5cbiAgaWYgKCFtb2R1bGVQYXRoLnN0YXJ0c1dpdGgoJ0Bhbmd1bGFyLycpKSB7XG4gICAgcmV0dXJuIHt9O1xuICB9XG5cbiAgaWYgKG5vZGUuaW1wb3J0Q2xhdXNlKSB7XG4gICAgaWYgKG5vZGUuaW1wb3J0Q2xhdXNlLm5hbWUpIHtcbiAgICAgIC8vIFRoaXMgaXMgb2YgdGhlIGZvcm0gYGltcG9ydCBOYW1lIGZyb20gJ3BhdGgnYC4gSWdub3JlLlxuICAgICAgcmV0dXJuIHt9O1xuICAgIH0gZWxzZSBpZiAobm9kZS5pbXBvcnRDbGF1c2UubmFtZWRCaW5kaW5ncykge1xuICAgICAgY29uc3QgbmIgPSBub2RlLmltcG9ydENsYXVzZS5uYW1lZEJpbmRpbmdzO1xuICAgICAgaWYgKG5iLmtpbmQgPT0gdHMuU3ludGF4S2luZC5OYW1lc3BhY2VJbXBvcnQpIHtcbiAgICAgICAgLy8gVGhpcyBpcyBvZiB0aGUgZm9ybSBgaW1wb3J0ICogYXMgbmFtZSBmcm9tICdwYXRoJ2AuIFJldHVybiBgbmFtZS5gLlxuICAgICAgICByZXR1cm4ge1xuICAgICAgICAgIFsobmIgYXMgdHMuTmFtZXNwYWNlSW1wb3J0KS5uYW1lLnRleHQgKyAnLiddOiBtb2R1bGVQYXRoLFxuICAgICAgICB9O1xuICAgICAgfSBlbHNlIHtcbiAgICAgICAgLy8gVGhpcyBpcyBvZiB0aGUgZm9ybSBgaW1wb3J0IHthLGIsY30gZnJvbSAncGF0aCdgXG4gICAgICAgIGNvbnN0IG5hbWVkSW1wb3J0cyA9IG5iIGFzIHRzLk5hbWVkSW1wb3J0cztcblxuICAgICAgICByZXR1cm4gbmFtZWRJbXBvcnRzLmVsZW1lbnRzXG4gICAgICAgICAgLm1hcCgoaXM6IHRzLkltcG9ydFNwZWNpZmllcikgPT4gaXMucHJvcGVydHlOYW1lID8gaXMucHJvcGVydHlOYW1lLnRleHQgOiBpcy5uYW1lLnRleHQpXG4gICAgICAgICAgLnJlZHVjZSgoYWNjOiB7W25hbWU6IHN0cmluZ106IHN0cmluZ30sIGN1cnI6IHN0cmluZykgPT4ge1xuICAgICAgICAgICAgYWNjW2N1cnJdID0gbW9kdWxlUGF0aDtcblxuICAgICAgICAgICAgcmV0dXJuIGFjYztcbiAgICAgICAgICB9LCB7fSk7XG4gICAgICB9XG4gICAgfVxuXG4gICAgcmV0dXJuIHt9O1xuICB9IGVsc2Uge1xuICAgIC8vIFRoaXMgaXMgb2YgdGhlIGZvcm0gYGltcG9ydCAncGF0aCc7YC4gTm90aGluZyB0byBkby5cbiAgICByZXR1cm4ge307XG4gIH1cbn1cblxuXG5leHBvcnQgZnVuY3Rpb24gZ2V0RGVjb3JhdG9yTWV0YWRhdGEoc291cmNlOiB0cy5Tb3VyY2VGaWxlLCBpZGVudGlmaWVyOiBzdHJpbmcsXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgbW9kdWxlOiBzdHJpbmcpOiB0cy5Ob2RlW10ge1xuICBjb25zdCBhbmd1bGFySW1wb3J0czoge1tuYW1lOiBzdHJpbmddOiBzdHJpbmd9XG4gICAgPSBmaW5kTm9kZXMoc291cmNlLCB0cy5TeW50YXhLaW5kLkltcG9ydERlY2xhcmF0aW9uKVxuICAgIC5tYXAoKG5vZGU6IHRzLkltcG9ydERlY2xhcmF0aW9uKSA9PiBfYW5ndWxhckltcG9ydHNGcm9tTm9kZShub2RlLCBzb3VyY2UpKVxuICAgIC5yZWR1Y2UoKGFjYzoge1tuYW1lOiBzdHJpbmddOiBzdHJpbmd9LCBjdXJyZW50OiB7W25hbWU6IHN0cmluZ106IHN0cmluZ30pID0+IHtcbiAgICAgIGZvciAoY29uc3Qga2V5IG9mIE9iamVjdC5rZXlzKGN1cnJlbnQpKSB7XG4gICAgICAgIGFjY1trZXldID0gY3VycmVudFtrZXldO1xuICAgICAgfVxuXG4gICAgICByZXR1cm4gYWNjO1xuICAgIH0sIHt9KTtcblxuICByZXR1cm4gZ2V0U291cmNlTm9kZXMoc291cmNlKVxuICAgIC5maWx0ZXIobm9kZSA9PiB7XG4gICAgICByZXR1cm4gbm9kZS5raW5kID09IHRzLlN5bnRheEtpbmQuRGVjb3JhdG9yXG4gICAgICAgICYmIChub2RlIGFzIHRzLkRlY29yYXRvcikuZXhwcmVzc2lvbi5raW5kID09IHRzLlN5bnRheEtpbmQuQ2FsbEV4cHJlc3Npb247XG4gICAgfSlcbiAgICAubWFwKG5vZGUgPT4gKG5vZGUgYXMgdHMuRGVjb3JhdG9yKS5leHByZXNzaW9uIGFzIHRzLkNhbGxFeHByZXNzaW9uKVxuICAgIC5maWx0ZXIoZXhwciA9PiB7XG4gICAgICBpZiAoZXhwci5leHByZXNzaW9uLmtpbmQgPT0gdHMuU3ludGF4S2luZC5JZGVudGlmaWVyKSB7XG4gICAgICAgIGNvbnN0IGlkID0gZXhwci5leHByZXNzaW9uIGFzIHRzLklkZW50aWZpZXI7XG5cbiAgICAgICAgcmV0dXJuIGlkLmdldEZ1bGxUZXh0KHNvdXJjZSkgPT0gaWRlbnRpZmllclxuICAgICAgICAgICYmIGFuZ3VsYXJJbXBvcnRzW2lkLmdldEZ1bGxUZXh0KHNvdXJjZSldID09PSBtb2R1bGU7XG4gICAgICB9IGVsc2UgaWYgKGV4cHIuZXhwcmVzc2lvbi5raW5kID09IHRzLlN5bnRheEtpbmQuUHJvcGVydHlBY2Nlc3NFeHByZXNzaW9uKSB7XG4gICAgICAgIC8vIFRoaXMgY292ZXJzIGZvby5OZ01vZHVsZSB3aGVuIGltcG9ydGluZyAqIGFzIGZvby5cbiAgICAgICAgY29uc3QgcGFFeHByID0gZXhwci5leHByZXNzaW9uIGFzIHRzLlByb3BlcnR5QWNjZXNzRXhwcmVzc2lvbjtcbiAgICAgICAgLy8gSWYgdGhlIGxlZnQgZXhwcmVzc2lvbiBpcyBub3QgYW4gaWRlbnRpZmllciwganVzdCBnaXZlIHVwIGF0IHRoYXQgcG9pbnQuXG4gICAgICAgIGlmIChwYUV4cHIuZXhwcmVzc2lvbi5raW5kICE9PSB0cy5TeW50YXhLaW5kLklkZW50aWZpZXIpIHtcbiAgICAgICAgICByZXR1cm4gZmFsc2U7XG4gICAgICAgIH1cblxuICAgICAgICBjb25zdCBpZCA9IHBhRXhwci5uYW1lLnRleHQ7XG4gICAgICAgIGNvbnN0IG1vZHVsZUlkID0gKHBhRXhwci5leHByZXNzaW9uIGFzIHRzLklkZW50aWZpZXIpLmdldFRleHQoc291cmNlKTtcblxuICAgICAgICByZXR1cm4gaWQgPT09IGlkZW50aWZpZXIgJiYgKGFuZ3VsYXJJbXBvcnRzW21vZHVsZUlkICsgJy4nXSA9PT0gbW9kdWxlKTtcbiAgICAgIH1cblxuICAgICAgcmV0dXJuIGZhbHNlO1xuICAgIH0pXG4gICAgLmZpbHRlcihleHByID0+IGV4cHIuYXJndW1lbnRzWzBdXG4gICAgICAgICAgICAgICAgICYmIGV4cHIuYXJndW1lbnRzWzBdLmtpbmQgPT0gdHMuU3ludGF4S2luZC5PYmplY3RMaXRlcmFsRXhwcmVzc2lvbilcbiAgICAubWFwKGV4cHIgPT4gZXhwci5hcmd1bWVudHNbMF0gYXMgdHMuT2JqZWN0TGl0ZXJhbEV4cHJlc3Npb24pO1xufVxuXG5mdW5jdGlvbiBmaW5kQ2xhc3NEZWNsYXJhdGlvblBhcmVudChub2RlOiB0cy5Ob2RlKTogdHMuQ2xhc3NEZWNsYXJhdGlvbnx1bmRlZmluZWQge1xuICBpZiAodHMuaXNDbGFzc0RlY2xhcmF0aW9uKG5vZGUpKSB7XG4gICAgcmV0dXJuIG5vZGU7XG4gIH1cblxuICByZXR1cm4gbm9kZS5wYXJlbnQgJiYgZmluZENsYXNzRGVjbGFyYXRpb25QYXJlbnQobm9kZS5wYXJlbnQpO1xufVxuXG4vKipcbiAqIEdpdmVuIGEgc291cmNlIGZpbGUgd2l0aCBATmdNb2R1bGUgY2xhc3MoZXMpLCBmaW5kIHRoZSBuYW1lIG9mIHRoZSBmaXJzdCBATmdNb2R1bGUgY2xhc3MuXG4gKlxuICogQHBhcmFtIHNvdXJjZSBzb3VyY2UgZmlsZSBjb250YWluaW5nIG9uZSBvciBtb3JlIEBOZ01vZHVsZVxuICogQHJldHVybnMgdGhlIG5hbWUgb2YgdGhlIGZpcnN0IEBOZ01vZHVsZSwgb3IgYHVuZGVmaW5lZGAgaWYgbm9uZSBpcyBmb3VuZFxuICovXG5leHBvcnQgZnVuY3Rpb24gZ2V0Rmlyc3ROZ01vZHVsZU5hbWUoc291cmNlOiB0cy5Tb3VyY2VGaWxlKTogc3RyaW5nfHVuZGVmaW5lZCB7XG4gIC8vIEZpcnN0LCBmaW5kIHRoZSBATmdNb2R1bGUgZGVjb3JhdG9ycy5cbiAgY29uc3QgbmdNb2R1bGVzTWV0YWRhdGEgPSBnZXREZWNvcmF0b3JNZXRhZGF0YShzb3VyY2UsICdOZ01vZHVsZScsICdAYW5ndWxhci9jb3JlJyk7XG4gIGlmIChuZ01vZHVsZXNNZXRhZGF0YS5sZW5ndGggPT09IDApIHtcbiAgICByZXR1cm4gdW5kZWZpbmVkO1xuICB9XG5cbiAgLy8gVGhlbiB3YWxrIHBhcmVudCBwb2ludGVycyB1cCB0aGUgQVNULCBsb29raW5nIGZvciB0aGUgQ2xhc3NEZWNsYXJhdGlvbiBwYXJlbnQgb2YgdGhlIE5nTW9kdWxlXG4gIC8vIG1ldGFkYXRhLlxuICBjb25zdCBtb2R1bGVDbGFzcyA9IGZpbmRDbGFzc0RlY2xhcmF0aW9uUGFyZW50KG5nTW9kdWxlc01ldGFkYXRhWzBdKTtcbiAgaWYgKCFtb2R1bGVDbGFzcyB8fCAhbW9kdWxlQ2xhc3MubmFtZSkge1xuICAgIHJldHVybiB1bmRlZmluZWQ7XG4gIH1cblxuICAvLyBHZXQgdGhlIGNsYXNzIG5hbWUgb2YgdGhlIG1vZHVsZSBDbGFzc0RlY2xhcmF0aW9uLlxuICByZXR1cm4gbW9kdWxlQ2xhc3MubmFtZS50ZXh0O1xufVxuXG5leHBvcnQgZnVuY3Rpb24gYWRkU3ltYm9sVG9OZ01vZHVsZU1ldGFkYXRhKFxuICBzb3VyY2U6IHRzLlNvdXJjZUZpbGUsXG4gIG5nTW9kdWxlUGF0aDogc3RyaW5nLFxuICBtZXRhZGF0YUZpZWxkOiBzdHJpbmcsXG4gIHN5bWJvbE5hbWU6IHN0cmluZyxcbiAgaW1wb3J0UGF0aDogc3RyaW5nIHwgbnVsbCA9IG51bGwsXG4pOiBDaGFuZ2VbXSB7XG4gIGNvbnN0IG5vZGVzID0gZ2V0RGVjb3JhdG9yTWV0YWRhdGEoc291cmNlLCAnTmdNb2R1bGUnLCAnQGFuZ3VsYXIvY29yZScpO1xuICBsZXQgbm9kZTogYW55ID0gbm9kZXNbMF07ICAvLyB0c2xpbnQ6ZGlzYWJsZS1saW5lOm5vLWFueVxuXG4gIC8vIEZpbmQgdGhlIGRlY29yYXRvciBkZWNsYXJhdGlvbi5cbiAgaWYgKCFub2RlKSB7XG4gICAgcmV0dXJuIFtdO1xuICB9XG5cbiAgLy8gR2V0IGFsbCB0aGUgY2hpbGRyZW4gcHJvcGVydHkgYXNzaWdubWVudCBvZiBvYmplY3QgbGl0ZXJhbHMuXG4gIGNvbnN0IG1hdGNoaW5nUHJvcGVydGllczogdHMuT2JqZWN0TGl0ZXJhbEVsZW1lbnRbXSA9XG4gICAgKG5vZGUgYXMgdHMuT2JqZWN0TGl0ZXJhbEV4cHJlc3Npb24pLnByb3BlcnRpZXNcbiAgICAuZmlsdGVyKHByb3AgPT4gcHJvcC5raW5kID09IHRzLlN5bnRheEtpbmQuUHJvcGVydHlBc3NpZ25tZW50KVxuICAgIC8vIEZpbHRlciBvdXQgZXZlcnkgZmllbGRzIHRoYXQncyBub3QgXCJtZXRhZGF0YUZpZWxkXCIuIEFsc28gaGFuZGxlcyBzdHJpbmcgbGl0ZXJhbHNcbiAgICAvLyAoYnV0IG5vdCBleHByZXNzaW9ucykuXG4gICAgLmZpbHRlcigocHJvcDogdHMuUHJvcGVydHlBc3NpZ25tZW50KSA9PiB7XG4gICAgICBjb25zdCBuYW1lID0gcHJvcC5uYW1lO1xuICAgICAgc3dpdGNoIChuYW1lLmtpbmQpIHtcbiAgICAgICAgY2FzZSB0cy5TeW50YXhLaW5kLklkZW50aWZpZXI6XG4gICAgICAgICAgcmV0dXJuIChuYW1lIGFzIHRzLklkZW50aWZpZXIpLmdldFRleHQoc291cmNlKSA9PSBtZXRhZGF0YUZpZWxkO1xuICAgICAgICBjYXNlIHRzLlN5bnRheEtpbmQuU3RyaW5nTGl0ZXJhbDpcbiAgICAgICAgICByZXR1cm4gKG5hbWUgYXMgdHMuU3RyaW5nTGl0ZXJhbCkudGV4dCA9PSBtZXRhZGF0YUZpZWxkO1xuICAgICAgfVxuXG4gICAgICByZXR1cm4gZmFsc2U7XG4gICAgfSk7XG5cbiAgLy8gR2V0IHRoZSBsYXN0IG5vZGUgb2YgdGhlIGFycmF5IGxpdGVyYWwuXG4gIGlmICghbWF0Y2hpbmdQcm9wZXJ0aWVzKSB7XG4gICAgcmV0dXJuIFtdO1xuICB9XG4gIGlmIChtYXRjaGluZ1Byb3BlcnRpZXMubGVuZ3RoID09IDApIHtcbiAgICAvLyBXZSBoYXZlbid0IGZvdW5kIHRoZSBmaWVsZCBpbiB0aGUgbWV0YWRhdGEgZGVjbGFyYXRpb24uIEluc2VydCBhIG5ldyBmaWVsZC5cbiAgICBjb25zdCBleHByID0gbm9kZSBhcyB0cy5PYmplY3RMaXRlcmFsRXhwcmVzc2lvbjtcbiAgICBsZXQgcG9zaXRpb246IG51bWJlcjtcbiAgICBsZXQgdG9JbnNlcnQ6IHN0cmluZztcbiAgICBpZiAoZXhwci5wcm9wZXJ0aWVzLmxlbmd0aCA9PSAwKSB7XG4gICAgICBwb3NpdGlvbiA9IGV4cHIuZ2V0RW5kKCkgLSAxO1xuICAgICAgdG9JbnNlcnQgPSBgICAke21ldGFkYXRhRmllbGR9OiBbJHtzeW1ib2xOYW1lfV1cXG5gO1xuICAgIH0gZWxzZSB7XG4gICAgICBub2RlID0gZXhwci5wcm9wZXJ0aWVzW2V4cHIucHJvcGVydGllcy5sZW5ndGggLSAxXTtcbiAgICAgIHBvc2l0aW9uID0gbm9kZS5nZXRFbmQoKTtcbiAgICAgIC8vIEdldCB0aGUgaW5kZW50YXRpb24gb2YgdGhlIGxhc3QgZWxlbWVudCwgaWYgYW55LlxuICAgICAgY29uc3QgdGV4dCA9IG5vZGUuZ2V0RnVsbFRleHQoc291cmNlKTtcbiAgICAgIGNvbnN0IG1hdGNoZXMgPSB0ZXh0Lm1hdGNoKC9eXFxyP1xcblxccyovKTtcbiAgICAgIGlmIChtYXRjaGVzICYmIG1hdGNoZXMubGVuZ3RoID4gMCkge1xuICAgICAgICB0b0luc2VydCA9IGAsJHttYXRjaGVzWzBdfSR7bWV0YWRhdGFGaWVsZH06IFske3N5bWJvbE5hbWV9XWA7XG4gICAgICB9IGVsc2Uge1xuICAgICAgICB0b0luc2VydCA9IGAsICR7bWV0YWRhdGFGaWVsZH06IFske3N5bWJvbE5hbWV9XWA7XG4gICAgICB9XG4gICAgfVxuICAgIGlmIChpbXBvcnRQYXRoICE9PSBudWxsKSB7XG4gICAgICByZXR1cm4gW1xuICAgICAgICBuZXcgSW5zZXJ0Q2hhbmdlKG5nTW9kdWxlUGF0aCwgcG9zaXRpb24sIHRvSW5zZXJ0KSxcbiAgICAgICAgaW5zZXJ0SW1wb3J0KHNvdXJjZSwgbmdNb2R1bGVQYXRoLCBzeW1ib2xOYW1lLnJlcGxhY2UoL1xcLi4qJC8sICcnKSwgaW1wb3J0UGF0aCksXG4gICAgICBdO1xuICAgIH0gZWxzZSB7XG4gICAgICByZXR1cm4gW25ldyBJbnNlcnRDaGFuZ2UobmdNb2R1bGVQYXRoLCBwb3NpdGlvbiwgdG9JbnNlcnQpXTtcbiAgICB9XG4gIH1cbiAgY29uc3QgYXNzaWdubWVudCA9IG1hdGNoaW5nUHJvcGVydGllc1swXSBhcyB0cy5Qcm9wZXJ0eUFzc2lnbm1lbnQ7XG5cbiAgLy8gSWYgaXQncyBub3QgYW4gYXJyYXksIG5vdGhpbmcgd2UgY2FuIGRvIHJlYWxseS5cbiAgaWYgKGFzc2lnbm1lbnQuaW5pdGlhbGl6ZXIua2luZCAhPT0gdHMuU3ludGF4S2luZC5BcnJheUxpdGVyYWxFeHByZXNzaW9uKSB7XG4gICAgcmV0dXJuIFtdO1xuICB9XG5cbiAgY29uc3QgYXJyTGl0ZXJhbCA9IGFzc2lnbm1lbnQuaW5pdGlhbGl6ZXIgYXMgdHMuQXJyYXlMaXRlcmFsRXhwcmVzc2lvbjtcbiAgaWYgKGFyckxpdGVyYWwuZWxlbWVudHMubGVuZ3RoID09IDApIHtcbiAgICAvLyBGb3J3YXJkIHRoZSBwcm9wZXJ0eS5cbiAgICBub2RlID0gYXJyTGl0ZXJhbDtcbiAgfSBlbHNlIHtcbiAgICBub2RlID0gYXJyTGl0ZXJhbC5lbGVtZW50cztcbiAgfVxuXG4gIGlmICghbm9kZSkge1xuICAgIGNvbnNvbGUuZXJyb3IoJ05vIGFwcCBtb2R1bGUgZm91bmQuIFBsZWFzZSBhZGQgeW91ciBuZXcgY2xhc3MgdG8geW91ciBjb21wb25lbnQuJyk7XG5cbiAgICByZXR1cm4gW107XG4gIH1cblxuICBpZiAoQXJyYXkuaXNBcnJheShub2RlKSkge1xuICAgIGNvbnN0IG5vZGVBcnJheSA9IG5vZGUgYXMge30gYXMgQXJyYXk8dHMuTm9kZT47XG4gICAgY29uc3Qgc3ltYm9sc0FycmF5ID0gbm9kZUFycmF5Lm1hcChub2RlID0+IG5vZGUuZ2V0VGV4dCgpKTtcbiAgICBpZiAoc3ltYm9sc0FycmF5LmluY2x1ZGVzKHN5bWJvbE5hbWUpKSB7XG4gICAgICByZXR1cm4gW107XG4gICAgfVxuXG4gICAgbm9kZSA9IG5vZGVbbm9kZS5sZW5ndGggLSAxXTtcbiAgfVxuXG4gIGxldCB0b0luc2VydDogc3RyaW5nO1xuICBsZXQgcG9zaXRpb24gPSBub2RlLmdldEVuZCgpO1xuICBpZiAobm9kZS5raW5kID09IHRzLlN5bnRheEtpbmQuT2JqZWN0TGl0ZXJhbEV4cHJlc3Npb24pIHtcbiAgICAvLyBXZSBoYXZlbid0IGZvdW5kIHRoZSBmaWVsZCBpbiB0aGUgbWV0YWRhdGEgZGVjbGFyYXRpb24uIEluc2VydCBhIG5ld1xuICAgIC8vIGZpZWxkLlxuICAgIGNvbnN0IGV4cHIgPSBub2RlIGFzIHRzLk9iamVjdExpdGVyYWxFeHByZXNzaW9uO1xuICAgIGlmIChleHByLnByb3BlcnRpZXMubGVuZ3RoID09IDApIHtcbiAgICAgIHBvc2l0aW9uID0gZXhwci5nZXRFbmQoKSAtIDE7XG4gICAgICB0b0luc2VydCA9IGAgICR7bWV0YWRhdGFGaWVsZH06IFske3N5bWJvbE5hbWV9XVxcbmA7XG4gICAgfSBlbHNlIHtcbiAgICAgIG5vZGUgPSBleHByLnByb3BlcnRpZXNbZXhwci5wcm9wZXJ0aWVzLmxlbmd0aCAtIDFdO1xuICAgICAgcG9zaXRpb24gPSBub2RlLmdldEVuZCgpO1xuICAgICAgLy8gR2V0IHRoZSBpbmRlbnRhdGlvbiBvZiB0aGUgbGFzdCBlbGVtZW50LCBpZiBhbnkuXG4gICAgICBjb25zdCB0ZXh0ID0gbm9kZS5nZXRGdWxsVGV4dChzb3VyY2UpO1xuICAgICAgaWYgKHRleHQubWF0Y2goJ15cXHI/XFxyP1xcbicpKSB7XG4gICAgICAgIHRvSW5zZXJ0ID0gYCwke3RleHQubWF0Y2goL15cXHI/XFxuXFxzKy8pWzBdfSR7bWV0YWRhdGFGaWVsZH06IFske3N5bWJvbE5hbWV9XWA7XG4gICAgICB9IGVsc2Uge1xuICAgICAgICB0b0luc2VydCA9IGAsICR7bWV0YWRhdGFGaWVsZH06IFske3N5bWJvbE5hbWV9XWA7XG4gICAgICB9XG4gICAgfVxuICB9IGVsc2UgaWYgKG5vZGUua2luZCA9PSB0cy5TeW50YXhLaW5kLkFycmF5TGl0ZXJhbEV4cHJlc3Npb24pIHtcbiAgICAvLyBXZSBmb3VuZCB0aGUgZmllbGQgYnV0IGl0J3MgZW1wdHkuIEluc2VydCBpdCBqdXN0IGJlZm9yZSB0aGUgYF1gLlxuICAgIHBvc2l0aW9uLS07XG4gICAgdG9JbnNlcnQgPSBgJHtzeW1ib2xOYW1lfWA7XG4gIH0gZWxzZSB7XG4gICAgLy8gR2V0IHRoZSBpbmRlbnRhdGlvbiBvZiB0aGUgbGFzdCBlbGVtZW50LCBpZiBhbnkuXG4gICAgY29uc3QgdGV4dCA9IG5vZGUuZ2V0RnVsbFRleHQoc291cmNlKTtcbiAgICBpZiAodGV4dC5tYXRjaCgvXlxccj9cXG4vKSkge1xuICAgICAgdG9JbnNlcnQgPSBgLCR7dGV4dC5tYXRjaCgvXlxccj9cXG4oXFxyPylcXHMrLylbMF19JHtzeW1ib2xOYW1lfWA7XG4gICAgfSBlbHNlIHtcbiAgICAgIHRvSW5zZXJ0ID0gYCwgJHtzeW1ib2xOYW1lfWA7XG4gICAgfVxuICB9XG4gIGlmIChpbXBvcnRQYXRoICE9PSBudWxsKSB7XG4gICAgcmV0dXJuIFtcbiAgICAgIG5ldyBJbnNlcnRDaGFuZ2UobmdNb2R1bGVQYXRoLCBwb3NpdGlvbiwgdG9JbnNlcnQpLFxuICAgICAgaW5zZXJ0SW1wb3J0KHNvdXJjZSwgbmdNb2R1bGVQYXRoLCBzeW1ib2xOYW1lLnJlcGxhY2UoL1xcLi4qJC8sICcnKSwgaW1wb3J0UGF0aCksXG4gICAgXTtcbiAgfVxuXG4gIHJldHVybiBbbmV3IEluc2VydENoYW5nZShuZ01vZHVsZVBhdGgsIHBvc2l0aW9uLCB0b0luc2VydCldO1xufVxuXG4vKipcbiAqIEN1c3RvbSBmdW5jdGlvbiB0byBpbnNlcnQgYSBkZWNsYXJhdGlvbiAoY29tcG9uZW50LCBwaXBlLCBkaXJlY3RpdmUpXG4gKiBpbnRvIE5nTW9kdWxlIGRlY2xhcmF0aW9ucy4gSXQgYWxzbyBpbXBvcnRzIHRoZSBjb21wb25lbnQuXG4gKi9cbmV4cG9ydCBmdW5jdGlvbiBhZGREZWNsYXJhdGlvblRvTW9kdWxlKHNvdXJjZTogdHMuU291cmNlRmlsZSxcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG1vZHVsZVBhdGg6IHN0cmluZywgY2xhc3NpZmllZE5hbWU6IHN0cmluZyxcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGltcG9ydFBhdGg6IHN0cmluZyk6IENoYW5nZVtdIHtcbiAgcmV0dXJuIGFkZFN5bWJvbFRvTmdNb2R1bGVNZXRhZGF0YShcbiAgICBzb3VyY2UsIG1vZHVsZVBhdGgsICdkZWNsYXJhdGlvbnMnLCBjbGFzc2lmaWVkTmFtZSwgaW1wb3J0UGF0aCk7XG59XG5cbi8qKlxuICogQ3VzdG9tIGZ1bmN0aW9uIHRvIGluc2VydCBhbiBOZ01vZHVsZSBpbnRvIE5nTW9kdWxlIGltcG9ydHMuIEl0IGFsc28gaW1wb3J0cyB0aGUgbW9kdWxlLlxuICovXG5leHBvcnQgZnVuY3Rpb24gYWRkSW1wb3J0VG9Nb2R1bGUoc291cmNlOiB0cy5Tb3VyY2VGaWxlLFxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG1vZHVsZVBhdGg6IHN0cmluZywgY2xhc3NpZmllZE5hbWU6IHN0cmluZyxcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpbXBvcnRQYXRoOiBzdHJpbmcpOiBDaGFuZ2VbXSB7XG5cbiAgcmV0dXJuIGFkZFN5bWJvbFRvTmdNb2R1bGVNZXRhZGF0YShzb3VyY2UsIG1vZHVsZVBhdGgsICdpbXBvcnRzJywgY2xhc3NpZmllZE5hbWUsIGltcG9ydFBhdGgpO1xufVxuXG4vKipcbiAqIEN1c3RvbSBmdW5jdGlvbiB0byBpbnNlcnQgYSBwcm92aWRlciBpbnRvIE5nTW9kdWxlLiBJdCBhbHNvIGltcG9ydHMgaXQuXG4gKi9cbmV4cG9ydCBmdW5jdGlvbiBhZGRQcm92aWRlclRvTW9kdWxlKHNvdXJjZTogdHMuU291cmNlRmlsZSxcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG1vZHVsZVBhdGg6IHN0cmluZywgY2xhc3NpZmllZE5hbWU6IHN0cmluZyxcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGltcG9ydFBhdGg6IHN0cmluZyk6IENoYW5nZVtdIHtcbiAgcmV0dXJuIGFkZFN5bWJvbFRvTmdNb2R1bGVNZXRhZGF0YShzb3VyY2UsIG1vZHVsZVBhdGgsICdwcm92aWRlcnMnLCBjbGFzc2lmaWVkTmFtZSwgaW1wb3J0UGF0aCk7XG59XG5cbi8qKlxuICogQ3VzdG9tIGZ1bmN0aW9uIHRvIGluc2VydCBhbiBleHBvcnQgaW50byBOZ01vZHVsZS4gSXQgYWxzbyBpbXBvcnRzIGl0LlxuICovXG5leHBvcnQgZnVuY3Rpb24gYWRkRXhwb3J0VG9Nb2R1bGUoc291cmNlOiB0cy5Tb3VyY2VGaWxlLFxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG1vZHVsZVBhdGg6IHN0cmluZywgY2xhc3NpZmllZE5hbWU6IHN0cmluZyxcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICBpbXBvcnRQYXRoOiBzdHJpbmcpOiBDaGFuZ2VbXSB7XG4gIHJldHVybiBhZGRTeW1ib2xUb05nTW9kdWxlTWV0YWRhdGEoc291cmNlLCBtb2R1bGVQYXRoLCAnZXhwb3J0cycsIGNsYXNzaWZpZWROYW1lLCBpbXBvcnRQYXRoKTtcbn1cblxuLyoqXG4gKiBDdXN0b20gZnVuY3Rpb24gdG8gaW5zZXJ0IGFuIGV4cG9ydCBpbnRvIE5nTW9kdWxlLiBJdCBhbHNvIGltcG9ydHMgaXQuXG4gKi9cbmV4cG9ydCBmdW5jdGlvbiBhZGRCb290c3RyYXBUb01vZHVsZShzb3VyY2U6IHRzLlNvdXJjZUZpbGUsXG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgbW9kdWxlUGF0aDogc3RyaW5nLCBjbGFzc2lmaWVkTmFtZTogc3RyaW5nLFxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGltcG9ydFBhdGg6IHN0cmluZyk6IENoYW5nZVtdIHtcbiAgcmV0dXJuIGFkZFN5bWJvbFRvTmdNb2R1bGVNZXRhZGF0YShzb3VyY2UsIG1vZHVsZVBhdGgsICdib290c3RyYXAnLCBjbGFzc2lmaWVkTmFtZSwgaW1wb3J0UGF0aCk7XG59XG5cbi8qKlxuICogQ3VzdG9tIGZ1bmN0aW9uIHRvIGluc2VydCBhbiBlbnRyeUNvbXBvbmVudCBpbnRvIE5nTW9kdWxlLiBJdCBhbHNvIGltcG9ydHMgaXQuXG4gKi9cbmV4cG9ydCBmdW5jdGlvbiBhZGRFbnRyeUNvbXBvbmVudFRvTW9kdWxlKHNvdXJjZTogdHMuU291cmNlRmlsZSxcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIG1vZHVsZVBhdGg6IHN0cmluZywgY2xhc3NpZmllZE5hbWU6IHN0cmluZyxcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIGltcG9ydFBhdGg6IHN0cmluZyk6IENoYW5nZVtdIHtcbiAgcmV0dXJuIGFkZFN5bWJvbFRvTmdNb2R1bGVNZXRhZGF0YShcbiAgICBzb3VyY2UsIG1vZHVsZVBhdGgsXG4gICAgJ2VudHJ5Q29tcG9uZW50cycsIGNsYXNzaWZpZWROYW1lLCBpbXBvcnRQYXRoLFxuICApO1xufVxuXG4vKipcbiAqIERldGVybWluZSBpZiBhbiBpbXBvcnQgYWxyZWFkeSBleGlzdHMuXG4gKi9cbmV4cG9ydCBmdW5jdGlvbiBpc0ltcG9ydGVkKHNvdXJjZTogdHMuU291cmNlRmlsZSxcbiAgICAgICAgICAgICAgICAgICAgICAgICAgIGNsYXNzaWZpZWROYW1lOiBzdHJpbmcsXG4gICAgICAgICAgICAgICAgICAgICAgICAgICBpbXBvcnRQYXRoOiBzdHJpbmcpOiBib29sZWFuIHtcbiAgY29uc3QgYWxsTm9kZXMgPSBnZXRTb3VyY2VOb2Rlcyhzb3VyY2UpO1xuICBjb25zdCBtYXRjaGluZ05vZGVzID0gYWxsTm9kZXNcbiAgICAuZmlsdGVyKG5vZGUgPT4gbm9kZS5raW5kID09PSB0cy5TeW50YXhLaW5kLkltcG9ydERlY2xhcmF0aW9uKVxuICAgIC5maWx0ZXIoKGltcDogdHMuSW1wb3J0RGVjbGFyYXRpb24pID0+IGltcC5tb2R1bGVTcGVjaWZpZXIua2luZCA9PT0gdHMuU3ludGF4S2luZC5TdHJpbmdMaXRlcmFsKVxuICAgIC5maWx0ZXIoKGltcDogdHMuSW1wb3J0RGVjbGFyYXRpb24pID0+IHtcbiAgICAgIHJldHVybiAoaW1wLm1vZHVsZVNwZWNpZmllciBhcyB0cy5TdHJpbmdMaXRlcmFsKS50ZXh0ID09PSBpbXBvcnRQYXRoO1xuICAgIH0pXG4gICAgLmZpbHRlcigoaW1wOiB0cy5JbXBvcnREZWNsYXJhdGlvbikgPT4ge1xuICAgICAgaWYgKCFpbXAuaW1wb3J0Q2xhdXNlKSB7XG4gICAgICAgIHJldHVybiBmYWxzZTtcbiAgICAgIH1cbiAgICAgIGNvbnN0IG5vZGVzID0gZmluZE5vZGVzKGltcC5pbXBvcnRDbGF1c2UsIHRzLlN5bnRheEtpbmQuSW1wb3J0U3BlY2lmaWVyKVxuICAgICAgICAuZmlsdGVyKG4gPT4gbi5nZXRUZXh0KCkgPT09IGNsYXNzaWZpZWROYW1lKTtcblxuICAgICAgcmV0dXJuIG5vZGVzLmxlbmd0aCA+IDA7XG4gICAgfSk7XG5cbiAgcmV0dXJuIG1hdGNoaW5nTm9kZXMubGVuZ3RoID4gMDtcbn1cbiJdfQ==