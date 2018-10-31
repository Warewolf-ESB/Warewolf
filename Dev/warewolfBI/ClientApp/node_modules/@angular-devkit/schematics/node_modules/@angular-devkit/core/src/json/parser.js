"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
const exception_1 = require("../exception");
/**
 * A character was invalid in this context.
 */
class InvalidJsonCharacterException extends exception_1.BaseException {
    constructor(context) {
        const pos = context.previous;
        super(`Invalid JSON character: ${JSON.stringify(_peek(context))} `
            + `at ${pos.line}:${pos.character}.`);
    }
}
exports.InvalidJsonCharacterException = InvalidJsonCharacterException;
/**
 * More input was expected, but we reached the end of the stream.
 */
class UnexpectedEndOfInputException extends exception_1.BaseException {
    constructor(_context) {
        super(`Unexpected end of file.`);
    }
}
exports.UnexpectedEndOfInputException = UnexpectedEndOfInputException;
/**
 * Peek and return the next character from the context.
 * @private
 */
function _peek(context) {
    return context.original[context.position.offset];
}
/**
 * Move the context to the next character, including incrementing the line if necessary.
 * @private
 */
function _next(context) {
    context.previous = context.position;
    let { offset, line, character } = context.position;
    const char = context.original[offset];
    offset++;
    if (char == '\n') {
        line++;
        character = 0;
    }
    else {
        character++;
    }
    context.position = { offset, line, character };
}
function _token(context, valid) {
    const char = _peek(context);
    if (valid) {
        if (!char) {
            throw new UnexpectedEndOfInputException(context);
        }
        if (valid.indexOf(char) == -1) {
            throw new InvalidJsonCharacterException(context);
        }
    }
    // Move the position of the context to the next character.
    _next(context);
    return char;
}
/**
 * Read the exponent part of a number. The exponent part is looser for JSON than the number
 * part. `str` is the string of the number itself found so far, and start the position
 * where the full number started. Returns the node found.
 * @private
 */
function _readExpNumber(context, start, str, comments) {
    let char;
    let signed = false;
    while (true) {
        char = _token(context);
        if (char == '+' || char == '-') {
            if (signed) {
                break;
            }
            signed = true;
            str += char;
        }
        else if (char == '0' || char == '1' || char == '2' || char == '3' || char == '4'
            || char == '5' || char == '6' || char == '7' || char == '8' || char == '9') {
            signed = true;
            str += char;
        }
        else {
            break;
        }
    }
    // We're done reading this number.
    context.position = context.previous;
    return {
        kind: 'number',
        start,
        end: context.position,
        text: context.original.substring(start.offset, context.position.offset),
        value: Number.parseFloat(str),
        comments: comments,
    };
}
/**
 * Read the hexa part of a 0xBADCAFE hexadecimal number.
 * @private
 */
function _readHexaNumber(context, isNegative, start, comments) {
    // Read an hexadecimal number, until it's not hexadecimal.
    let hexa = '';
    const valid = '0123456789abcdefABCDEF';
    for (let ch = _peek(context); ch && valid.includes(ch); ch = _peek(context)) {
        // Add it to the hexa string.
        hexa += ch;
        // Move the position of the context to the next character.
        _next(context);
    }
    const value = Number.parseInt(hexa, 16);
    // We're done reading this number.
    return {
        kind: 'number',
        start,
        end: context.position,
        text: context.original.substring(start.offset, context.position.offset),
        value: isNegative ? -value : value,
        comments,
    };
}
/**
 * Read a number from the context.
 * @private
 */
function _readNumber(context, comments = _readBlanks(context)) {
    let str = '';
    let dotted = false;
    const start = context.position;
    // read until `e` or end of line.
    while (true) {
        const char = _token(context);
        // Read tokens, one by one.
        if (char == '-') {
            if (str != '') {
                throw new InvalidJsonCharacterException(context);
            }
        }
        else if (char == 'I'
            && (str == '-' || str == '' || str == '+')
            && (context.mode & JsonParseMode.NumberConstantsAllowed) != 0) {
            // Infinity?
            // _token(context, 'I'); Already read.
            _token(context, 'n');
            _token(context, 'f');
            _token(context, 'i');
            _token(context, 'n');
            _token(context, 'i');
            _token(context, 't');
            _token(context, 'y');
            str += 'Infinity';
            break;
        }
        else if (char == '0') {
            if (str == '0' || str == '-0') {
                throw new InvalidJsonCharacterException(context);
            }
        }
        else if (char == '1' || char == '2' || char == '3' || char == '4' || char == '5'
            || char == '6' || char == '7' || char == '8' || char == '9') {
            if (str == '0' || str == '-0') {
                throw new InvalidJsonCharacterException(context);
            }
        }
        else if (char == '+' && str == '') {
            // Pass over.
        }
        else if (char == '.') {
            if (dotted) {
                throw new InvalidJsonCharacterException(context);
            }
            dotted = true;
        }
        else if (char == 'e' || char == 'E') {
            return _readExpNumber(context, start, str + char, comments);
        }
        else if (char == 'x' && (str == '0' || str == '-0')
            && (context.mode & JsonParseMode.HexadecimalNumberAllowed) != 0) {
            return _readHexaNumber(context, str == '-0', start, comments);
        }
        else {
            // We read one too many characters, so rollback the last character.
            context.position = context.previous;
            break;
        }
        str += char;
    }
    // We're done reading this number.
    if (str.endsWith('.') && (context.mode & JsonParseMode.HexadecimalNumberAllowed) == 0) {
        throw new InvalidJsonCharacterException(context);
    }
    return {
        kind: 'number',
        start,
        end: context.position,
        text: context.original.substring(start.offset, context.position.offset),
        value: Number.parseFloat(str),
        comments,
    };
}
/**
 * Read a string from the context. Takes the comments of the string or read the blanks before the
 * string.
 * @private
 */
function _readString(context, comments = _readBlanks(context)) {
    const start = context.position;
    // Consume the first string delimiter.
    const delim = _token(context);
    if ((context.mode & JsonParseMode.SingleQuotesAllowed) == 0) {
        if (delim == '\'') {
            throw new InvalidJsonCharacterException(context);
        }
    }
    let str = '';
    while (true) {
        let char = _token(context);
        if (char == delim) {
            return {
                kind: 'string',
                start,
                end: context.position,
                text: context.original.substring(start.offset, context.position.offset),
                value: str,
                comments: comments,
            };
        }
        else if (char == '\\') {
            char = _token(context);
            switch (char) {
                case '\\':
                case '\/':
                case '"':
                case delim:
                    str += char;
                    break;
                case 'b':
                    str += '\b';
                    break;
                case 'f':
                    str += '\f';
                    break;
                case 'n':
                    str += '\n';
                    break;
                case 'r':
                    str += '\r';
                    break;
                case 't':
                    str += '\t';
                    break;
                case 'u':
                    const [c0] = _token(context, '0123456789abcdefABCDEF');
                    const [c1] = _token(context, '0123456789abcdefABCDEF');
                    const [c2] = _token(context, '0123456789abcdefABCDEF');
                    const [c3] = _token(context, '0123456789abcdefABCDEF');
                    str += String.fromCharCode(parseInt(c0 + c1 + c2 + c3, 16));
                    break;
                case undefined:
                    throw new UnexpectedEndOfInputException(context);
                case '\n':
                    // Only valid when multiline strings are allowed.
                    if ((context.mode & JsonParseMode.MultiLineStringAllowed) == 0) {
                        throw new InvalidJsonCharacterException(context);
                    }
                    str += char;
                    break;
                default:
                    throw new InvalidJsonCharacterException(context);
            }
        }
        else if (char === undefined) {
            throw new UnexpectedEndOfInputException(context);
        }
        else if (char == '\b' || char == '\f' || char == '\n' || char == '\r' || char == '\t') {
            throw new InvalidJsonCharacterException(context);
        }
        else {
            str += char;
        }
    }
}
/**
 * Read the constant `true` from the context.
 * @private
 */
function _readTrue(context, comments = _readBlanks(context)) {
    const start = context.position;
    _token(context, 't');
    _token(context, 'r');
    _token(context, 'u');
    _token(context, 'e');
    const end = context.position;
    return {
        kind: 'true',
        start,
        end,
        text: context.original.substring(start.offset, end.offset),
        value: true,
        comments,
    };
}
/**
 * Read the constant `false` from the context.
 * @private
 */
function _readFalse(context, comments = _readBlanks(context)) {
    const start = context.position;
    _token(context, 'f');
    _token(context, 'a');
    _token(context, 'l');
    _token(context, 's');
    _token(context, 'e');
    const end = context.position;
    return {
        kind: 'false',
        start,
        end,
        text: context.original.substring(start.offset, end.offset),
        value: false,
        comments,
    };
}
/**
 * Read the constant `null` from the context.
 * @private
 */
function _readNull(context, comments = _readBlanks(context)) {
    const start = context.position;
    _token(context, 'n');
    _token(context, 'u');
    _token(context, 'l');
    _token(context, 'l');
    const end = context.position;
    return {
        kind: 'null',
        start,
        end,
        text: context.original.substring(start.offset, end.offset),
        value: null,
        comments: comments,
    };
}
/**
 * Read the constant `NaN` from the context.
 * @private
 */
function _readNaN(context, comments = _readBlanks(context)) {
    const start = context.position;
    _token(context, 'N');
    _token(context, 'a');
    _token(context, 'N');
    const end = context.position;
    return {
        kind: 'number',
        start,
        end,
        text: context.original.substring(start.offset, end.offset),
        value: NaN,
        comments: comments,
    };
}
/**
 * Read an array of JSON values from the context.
 * @private
 */
function _readArray(context, comments = _readBlanks(context)) {
    const start = context.position;
    // Consume the first delimiter.
    _token(context, '[');
    const value = [];
    const elements = [];
    _readBlanks(context);
    if (_peek(context) != ']') {
        const node = _readValue(context);
        elements.push(node);
        value.push(node.value);
    }
    while (_peek(context) != ']') {
        _token(context, ',');
        const valueComments = _readBlanks(context);
        if ((context.mode & JsonParseMode.TrailingCommasAllowed) !== 0 && _peek(context) === ']') {
            break;
        }
        const node = _readValue(context, valueComments);
        elements.push(node);
        value.push(node.value);
    }
    _token(context, ']');
    return {
        kind: 'array',
        start,
        end: context.position,
        text: context.original.substring(start.offset, context.position.offset),
        value,
        elements,
        comments,
    };
}
/**
 * Read an identifier from the context. An identifier is a valid JavaScript identifier, and this
 * function is only used in Loose mode.
 * @private
 */
function _readIdentifier(context, comments = _readBlanks(context)) {
    const start = context.position;
    let char = _peek(context);
    if (char && '0123456789'.indexOf(char) != -1) {
        const identifierNode = _readNumber(context);
        return {
            kind: 'identifier',
            start,
            end: identifierNode.end,
            text: identifierNode.text,
            value: identifierNode.value.toString(),
        };
    }
    const identValidFirstChar = 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMOPQRSTUVWXYZ';
    const identValidChar = '_$abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMOPQRSTUVWXYZ0123456789';
    let first = true;
    let value = '';
    while (true) {
        char = _token(context);
        if (char == undefined
            || (first ? identValidFirstChar.indexOf(char) : identValidChar.indexOf(char)) == -1) {
            context.position = context.previous;
            return {
                kind: 'identifier',
                start,
                end: context.position,
                text: context.original.substr(start.offset, context.position.offset),
                value,
                comments,
            };
        }
        value += char;
        first = false;
    }
}
/**
 * Read a property from the context. A property is a string or (in Loose mode only) a number or
 * an identifier, followed by a colon `:`.
 * @private
 */
function _readProperty(context, comments = _readBlanks(context)) {
    const start = context.position;
    let key;
    if ((context.mode & JsonParseMode.IdentifierKeyNamesAllowed) != 0) {
        const top = _peek(context);
        if (top == '"' || top == '\'') {
            key = _readString(context);
        }
        else {
            key = _readIdentifier(context);
        }
    }
    else {
        key = _readString(context);
    }
    _readBlanks(context);
    _token(context, ':');
    const value = _readValue(context);
    const end = context.position;
    return {
        kind: 'keyvalue',
        key,
        value,
        start,
        end,
        text: context.original.substring(start.offset, end.offset),
        comments,
    };
}
/**
 * Read an object of properties -> JSON values from the context.
 * @private
 */
function _readObject(context, comments = _readBlanks(context)) {
    const start = context.position;
    // Consume the first delimiter.
    _token(context, '{');
    const value = {};
    const properties = [];
    _readBlanks(context);
    if (_peek(context) != '}') {
        const property = _readProperty(context);
        value[property.key.value] = property.value.value;
        properties.push(property);
        while (_peek(context) != '}') {
            _token(context, ',');
            const propertyComments = _readBlanks(context);
            if ((context.mode & JsonParseMode.TrailingCommasAllowed) !== 0 && _peek(context) === '}') {
                break;
            }
            const property = _readProperty(context, propertyComments);
            value[property.key.value] = property.value.value;
            properties.push(property);
        }
    }
    _token(context, '}');
    return {
        kind: 'object',
        properties,
        start,
        end: context.position,
        value,
        text: context.original.substring(start.offset, context.position.offset),
        comments,
    };
}
/**
 * Remove any blank character or comments (in Loose mode) from the context, returning an array
 * of comments if any are found.
 * @private
 */
function _readBlanks(context) {
    if ((context.mode & JsonParseMode.CommentsAllowed) != 0) {
        const comments = [];
        while (true) {
            const char = context.original[context.position.offset];
            if (char == '/' && context.original[context.position.offset + 1] == '*') {
                const start = context.position;
                // Multi line comment.
                _next(context);
                _next(context);
                while (context.original[context.position.offset] != '*'
                    || context.original[context.position.offset + 1] != '/') {
                    _next(context);
                    if (context.position.offset >= context.original.length) {
                        throw new UnexpectedEndOfInputException(context);
                    }
                }
                // Remove "*/".
                _next(context);
                _next(context);
                comments.push({
                    kind: 'multicomment',
                    start,
                    end: context.position,
                    text: context.original.substring(start.offset, context.position.offset),
                    content: context.original.substring(start.offset + 2, context.position.offset - 2),
                });
            }
            else if (char == '/' && context.original[context.position.offset + 1] == '/') {
                const start = context.position;
                // Multi line comment.
                _next(context);
                _next(context);
                while (context.original[context.position.offset] != '\n') {
                    _next(context);
                    if (context.position.offset >= context.original.length) {
                        break;
                    }
                }
                // Remove "\n".
                if (context.position.offset < context.original.length) {
                    _next(context);
                }
                comments.push({
                    kind: 'comment',
                    start,
                    end: context.position,
                    text: context.original.substring(start.offset, context.position.offset),
                    content: context.original.substring(start.offset + 2, context.position.offset - 1),
                });
            }
            else if (char == ' ' || char == '\t' || char == '\n' || char == '\r' || char == '\f') {
                _next(context);
            }
            else {
                break;
            }
        }
        return comments;
    }
    else {
        let char = context.original[context.position.offset];
        while (char == ' ' || char == '\t' || char == '\n' || char == '\r' || char == '\f') {
            _next(context);
            char = context.original[context.position.offset];
        }
        return [];
    }
}
/**
 * Read a JSON value from the context, which can be any form of JSON value.
 * @private
 */
function _readValue(context, comments = _readBlanks(context)) {
    let result;
    // Clean up before.
    const char = _peek(context);
    switch (char) {
        case undefined:
            throw new UnexpectedEndOfInputException(context);
        case '-':
        case '0':
        case '1':
        case '2':
        case '3':
        case '4':
        case '5':
        case '6':
        case '7':
        case '8':
        case '9':
            result = _readNumber(context, comments);
            break;
        case '.':
        case '+':
            if ((context.mode & JsonParseMode.LaxNumberParsingAllowed) == 0) {
                throw new InvalidJsonCharacterException(context);
            }
            result = _readNumber(context, comments);
            break;
        case '\'':
        case '"':
            result = _readString(context, comments);
            break;
        case 'I':
            if ((context.mode & JsonParseMode.NumberConstantsAllowed) == 0) {
                throw new InvalidJsonCharacterException(context);
            }
            result = _readNumber(context, comments);
            break;
        case 'N':
            if ((context.mode & JsonParseMode.NumberConstantsAllowed) == 0) {
                throw new InvalidJsonCharacterException(context);
            }
            result = _readNaN(context, comments);
            break;
        case 't':
            result = _readTrue(context, comments);
            break;
        case 'f':
            result = _readFalse(context, comments);
            break;
        case 'n':
            result = _readNull(context, comments);
            break;
        case '[':
            result = _readArray(context, comments);
            break;
        case '{':
            result = _readObject(context, comments);
            break;
        default:
            throw new InvalidJsonCharacterException(context);
    }
    // Clean up after.
    _readBlanks(context);
    return result;
}
/**
 * The Parse mode used for parsing the JSON string.
 */
var JsonParseMode;
(function (JsonParseMode) {
    JsonParseMode[JsonParseMode["Strict"] = 0] = "Strict";
    JsonParseMode[JsonParseMode["CommentsAllowed"] = 1] = "CommentsAllowed";
    JsonParseMode[JsonParseMode["SingleQuotesAllowed"] = 2] = "SingleQuotesAllowed";
    JsonParseMode[JsonParseMode["IdentifierKeyNamesAllowed"] = 4] = "IdentifierKeyNamesAllowed";
    JsonParseMode[JsonParseMode["TrailingCommasAllowed"] = 8] = "TrailingCommasAllowed";
    JsonParseMode[JsonParseMode["HexadecimalNumberAllowed"] = 16] = "HexadecimalNumberAllowed";
    JsonParseMode[JsonParseMode["MultiLineStringAllowed"] = 32] = "MultiLineStringAllowed";
    JsonParseMode[JsonParseMode["LaxNumberParsingAllowed"] = 64] = "LaxNumberParsingAllowed";
    JsonParseMode[JsonParseMode["NumberConstantsAllowed"] = 128] = "NumberConstantsAllowed";
    JsonParseMode[JsonParseMode["Default"] = 0] = "Default";
    JsonParseMode[JsonParseMode["Loose"] = 255] = "Loose";
    JsonParseMode[JsonParseMode["Json"] = 0] = "Json";
    JsonParseMode[JsonParseMode["Json5"] = 255] = "Json5";
})(JsonParseMode = exports.JsonParseMode || (exports.JsonParseMode = {}));
/**
 * Parse the JSON string and return its AST. The AST may be losing data (end comments are
 * discarded for example, and space characters are not represented in the AST), but all values
 * will have a single node in the AST (a 1-to-1 mapping).
 * @param input The string to use.
 * @param mode The mode to parse the input with. {@see JsonParseMode}.
 * @returns {JsonAstNode} The root node of the value of the AST.
 */
function parseJsonAst(input, mode = JsonParseMode.Default) {
    if (mode == JsonParseMode.Default) {
        mode = JsonParseMode.Strict;
    }
    const context = {
        position: { offset: 0, line: 0, character: 0 },
        previous: { offset: 0, line: 0, character: 0 },
        original: input,
        comments: undefined,
        mode,
    };
    const ast = _readValue(context);
    if (context.position.offset < input.length) {
        const rest = input.substr(context.position.offset);
        const i = rest.length > 20 ? rest.substr(0, 20) + '...' : rest;
        throw new Error(`Expected end of file, got "${i}" at `
            + `${context.position.line}:${context.position.character}.`);
    }
    return ast;
}
exports.parseJsonAst = parseJsonAst;
/**
 * Parse a JSON string into its value.  This discards the AST and only returns the value itself.
 * @param input The string to parse.
 * @param mode The mode to parse the input with. {@see JsonParseMode}.
 * @returns {JsonValue} The value represented by the JSON string.
 */
function parseJson(input, mode = JsonParseMode.Default) {
    // Try parsing for the fastest path available, if error, uses our own parser for better errors.
    if (mode == JsonParseMode.Strict) {
        try {
            return JSON.parse(input);
        }
        catch (err) {
            return parseJsonAst(input, mode).value;
        }
    }
    return parseJsonAst(input, mode).value;
}
exports.parseJson = parseJson;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoicGFyc2VyLmpzIiwic291cmNlUm9vdCI6Ii4vIiwic291cmNlcyI6WyJwYWNrYWdlcy9hbmd1bGFyX2RldmtpdC9jb3JlL3NyYy9qc29uL3BhcnNlci50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiOztBQUFBOzs7Ozs7R0FNRztBQUNILDRDQUE2QztBQXFCN0M7O0dBRUc7QUFDSCxtQ0FBMkMsU0FBUSx5QkFBYTtJQUM5RCxZQUFZLE9BQTBCO1FBQ3BDLE1BQU0sR0FBRyxHQUFHLE9BQU8sQ0FBQyxRQUFRLENBQUM7UUFDN0IsS0FBSyxDQUFDLDJCQUEyQixJQUFJLENBQUMsU0FBUyxDQUFDLEtBQUssQ0FBQyxPQUFPLENBQUMsQ0FBQyxHQUFHO2NBQzVELE1BQU0sR0FBRyxDQUFDLElBQUksSUFBSSxHQUFHLENBQUMsU0FBUyxHQUFHLENBQUMsQ0FBQztJQUM1QyxDQUFDO0NBQ0Y7QUFORCxzRUFNQztBQUdEOztHQUVHO0FBQ0gsbUNBQTJDLFNBQVEseUJBQWE7SUFDOUQsWUFBWSxRQUEyQjtRQUNyQyxLQUFLLENBQUMseUJBQXlCLENBQUMsQ0FBQztJQUNuQyxDQUFDO0NBQ0Y7QUFKRCxzRUFJQztBQWNEOzs7R0FHRztBQUNILGVBQWUsT0FBMEI7SUFDdkMsT0FBTyxPQUFPLENBQUMsUUFBUSxDQUFDLE9BQU8sQ0FBQyxRQUFRLENBQUMsTUFBTSxDQUFDLENBQUM7QUFDbkQsQ0FBQztBQUdEOzs7R0FHRztBQUNILGVBQWUsT0FBMEI7SUFDdkMsT0FBTyxDQUFDLFFBQVEsR0FBRyxPQUFPLENBQUMsUUFBUSxDQUFDO0lBRXBDLElBQUksRUFBQyxNQUFNLEVBQUUsSUFBSSxFQUFFLFNBQVMsRUFBQyxHQUFHLE9BQU8sQ0FBQyxRQUFRLENBQUM7SUFDakQsTUFBTSxJQUFJLEdBQUcsT0FBTyxDQUFDLFFBQVEsQ0FBQyxNQUFNLENBQUMsQ0FBQztJQUN0QyxNQUFNLEVBQUUsQ0FBQztJQUNULElBQUksSUFBSSxJQUFJLElBQUksRUFBRTtRQUNoQixJQUFJLEVB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QUMsTUFBTSxFQUFFO1FBQzFDLE1BQU0sSUFBSSxHQUFHLEtBQUssQ0FBQyxNQUFNLENBQUMsT0FBTyxDQUFDLFFBQVEsQ0FBQyxNQUFNLENBQUMsQ0FBQztRQUNuRCxNQUFNLENBQUMsR0FBRyxJQUFJLENBQUMsTUFBTSxHQUFHLEVBQUUsQ0FBQyxDQUFDLENBQUMsSUFBSSxDQUFDLE1BQU0sQ0FBQyxDQUFDLEVBQUUsRUFBRSxDQUFDLEdBQUcsS0FBSyxDQUFDLENBQUMsQ0FBQyxJQUFJLENBQUM7UUFDL0QsTUFBTSxJQUFJLEtBQUssQ0FBQyw4QkFBOEIsQ0FBQyxPQUFPO2NBQ2hELEdBQUcsT0FBTyxDQUFDLFFBQVEsQ0FBQyxJQUFJLElBQUksT0FBTyxDQUFDLFFBQVEsQ0FBQyxTQUFTLEdBQUcsQ0FBQyxDQUFDO0tBQ2xFO0lBRUQsT0FBTyxHQUFHLENBQUM7QUFDYixDQUFDO0FBdEJELG9DQXNCQztBQUdEOzs7OztHQUtHO0FBQ0gsbUJBQTBCLEtBQWEsRUFBRSxJQUFJLEdBQUcsYUFBYSxDQUFDLE9BQU87SUFDbkUsK0ZBQStGO0lBQy9GLElBQUksSUFBSSxJQUFJLGFBQWEsQ0FBQyxNQUFNLEVBQUU7UUFDaEMsSUFBSTtZQUNGLE9BQU8sSUFBSSxDQUFDLEtBQUssQ0FBQyxLQUFLLENBQUMsQ0FBQztTQUMxQjtRQUFDLE9BQU8sR0FBRyxFQUFFO1lBQ1osT0FBTyxZQUFZLENBQUMsS0FBSyxFQUFFLElBQUksQ0FBQyxDQUFDLEtBQUssQ0FBQztTQUN4QztLQUNGO0lBRUQsT0FBTyxZQUFZLENBQUMsS0FBSyxFQUFFLElBQUksQ0FBQyxDQUFDLEtBQUssQ0FBQztBQUN6QyxDQUFDO0FBWEQsOEJBV0MiLCJzb3VyY2VzQ29udGVudCI6WyIvKipcbiAqIEBsaWNlbnNlXG4gKiBDb3B5cmlnaHQgR29vZ2xlIEluYy4gQWxsIFJpZ2h0cyBSZXNlcnZlZC5cbiAqXG4gKiBVc2Ugb2YgdGhpcyBzb3VyY2UgY29kZSBpcyBnb3Zlcm5lZCBieSBhbiBNSVQtc3R5bGUgbGljZW5zZSB0aGF0IGNhbiBiZVxuICogZm91bmQgaW4gdGhlIExJQ0VOU0UgZmlsZSBhdCBodHRwczovL2FuZ3VsYXIuaW8vbGljZW5zZVxuICovXG5pbXBvcnQgeyBCYXNlRXhjZXB0aW9uIH0gZnJvbSAnLi4vZXhjZXB0aW9uJztcbmltcG9ydCB7XG4gIEpzb25BcnJheSxcbiAgSnNvbkFzdEFycmF5LFxuICBKc29uQXN0Q29tbWVudCxcbiAgSnNvbkFzdENvbnN0YW50RmFsc2UsXG4gIEpzb25Bc3RDb25zdGFudE51bGwsXG4gIEpzb25Bc3RDb25zdGFudFRydWUsXG4gIEpzb25Bc3RJZGVudGlmaWVyLFxuICBKc29uQXN0S2V5VmFsdWUsXG4gIEpzb25Bc3RNdWx0aWxpbmVDb21tZW50LFxuICBKc29uQXN0Tm9kZSxcbiAgSnNvbkFzdE51bWJlcixcbiAgSnNvbkFzdE9iamVjdCxcbiAgSnNvbkFzdFN0cmluZyxcbiAgSnNvbk9iamVjdCxcbiAgSnNvblZhbHVlLFxuICBQb3NpdGlvbixcbn0gZnJvbSAnLi9pbnRlcmZhY2UnO1xuXG5cbi8qKlxuICogQSBjaGFyYWN0ZXIgd2FzIGludmFsaWQgaW4gdGhpcyBjb250ZXh0LlxuICovXG5leHBvcnQgY2xhc3MgSW52YWxpZEpzb25DaGFyYWN0ZXJFeGNlcHRpb24gZXh0ZW5kcyBCYXNlRXhjZXB0aW9uIHtcbiAgY29uc3RydWN0b3IoY29udGV4dDogSnNvblBhcnNlckNvbnRleHQpIHtcbiAgICBjb25zdCBwb3MgPSBjb250ZXh0LnByZXZpb3VzO1xuICAgIHN1cGVyKGBJbnZhbGlkIEpTT04gY2hhcmFjdGVyOiAke0pTT04uc3RyaW5naWZ5KF9wZWVrKGNvbnRleHQpKX0gYFxuICAgICAgICArIGBhdCAke3Bvcy5saW5lfToke3Bvcy5jaGFyYWN0ZXJ9LmApO1xuICB9XG59XG5cblxuLyoqXG4gKiBNb3JlIGlucHV0IHdhcyBleHBlY3RlZCwgYnV0IHdlIHJlYWNoZWQgdGhlIGVuZCBvZiB0aGUgc3RyZWFtLlxuICovXG5leHBvcnQgY2xhc3MgVW5leHBlY3RlZEVuZE9mSW5wdXRFeGNlcHRpb24gZXh0ZW5kcyBCYXNlRXhjZXB0aW9uIHtcbiAgY29uc3RydWN0b3IoX2NvbnRleHQ6IEpzb25QYXJzZXJDb250ZXh0KSB7XG4gICAgc3VwZXIoYFVuZXhwZWN0ZWQgZW5kIG9mIGZpbGUuYCk7XG4gIH1cbn1cblxuXG4vKipcbiAqIENvbnRleHQgcGFzc2VkIGFyb3VuZCB0aGUgcGFyc2VyIHdpdGggaW5mb3JtYXRpb24gYWJvdXQgd2hlcmUgd2UgY3VycmVudGx5IGFyZSBpbiB0aGUgcGFyc2UuXG4gKi9cbmV4cG9ydCBpbnRlcmZhY2UgSnNvblBhcnNlckNvbnRleHQge1xuICBwb3NpdGlvbjogUG9zaXRpb247XG4gIHByZXZpb3VzOiBQb3NpdGlvbjtcbiAgcmVhZG9ubHkgb3JpZ2luYWw6IHN0cmluZztcbiAgcmVhZG9ubHkgbW9kZTogSnNvblBhcnNlTW9kZTtcbn1cblxuXG4vKipcbiAqIFBlZWsgYW5kIHJldHVybiB0aGUgbmV4dCBjaGFyYWN0ZXIgZnJvbSB0aGUgY29udGV4dC5cbiAqIEBwcml2YXRlXG4gKi9cbmZ1bmN0aW9uIF9wZWVrKGNvbnRleHQ6IEpzb25QYXJzZXJDb250ZXh0KTogc3RyaW5nIHwgdW5kZWZpbmVkIHtcbiAgcmV0dXJuIGNvbnRleHQub3JpZ2luYWxbY29udGV4dC5wb3NpdGlvbi5vZmZzZXRdO1xufVxuXG5cbi8qKlxuICogTW92ZSB0aGUgY29udGV4dCB0byB0aGUgbmV4dCBjaGFyYWN0ZXIsIGluY2x1ZGluZyBpbmNyZW1lbnRpbmcgdGhlIGxpbmUgaWYgbmVjZXNzYXJ5LlxuICogQHByaXZhdGVcbiAqL1xuZnVuY3Rpb24gX25leHQoY29udGV4dDogSnNvblBhcnNlckNvbnRleHQpIHtcbiAgY29udGV4dC5wcmV2aW91cyA9IGNvbnRleHQucG9zaXRpb247XG5cbiAgbGV0IHtvZmZzZXQsIGxpbmUsIGNoYXJhY3Rlcn0gPSBjb250ZXh0LnBvc2l0aW9uO1xuICBjb25zdCBjaGFyID0gY29udGV4dC5vcmlnaW5hbFtvZmZzZXRdO1xuICBvZmZzZXQrKztcbiAgaWYgKGNoYXIgPT0gJ1xcbicpIHtcbiAgICBsaW5lKys7XG4gICAgY2hhcmFjdGVyID0gMDtcbiAgfSBlbHNlIHtcbiAgICBjaGFyYWN0ZXIrKztcbiAgfVxuICBjb250ZXh0LnBvc2l0aW9uID0ge29mZnNldCwgbGluZSwgY2hhcmFjdGVyfTtcbn1cblxuXG4vKipcbiAqIFJlYWQgYSBzaW5nbGUgY2hhcmFjdGVyIGZyb20gdGhlIGlucHV0LiBJZiBhIGB2YWxpZGAgc3RyaW5nIGlzIHBhc3NlZCwgdmFsaWRhdGUgdGhhdCB0aGVcbiAqIGNoYXJhY3RlciBpcyBpbmNsdWRlZCBpbiB0aGUgdmFsaWQgc3RyaW5nLlxuICogQHByaXZhdGVcbiAqL1xuZnVuY3Rpb24gX3Rva2VuKGNvbnRleHQ6IEpzb25QYXJzZXJDb250ZXh0LCB2YWxpZDogc3RyaW5nKTogc3RyaW5nO1xuZnVuY3Rpb24gX3Rva2VuKGNvbnRleHQ6IEpzb25QYXJzZXJDb250ZXh0KTogc3RyaW5nIHwgdW5kZWZpbmVkO1xuZnVuY3Rpb24gX3Rva2VuKGNvbnRleHQ6IEpzb25QYXJzZXJDb250ZXh0LCB2YWxpZD86IHN0cmluZyk6IHN0cmluZyB8IHVuZGVmaW5lZCB7XG4gIGNvbnN0IGNoYXIgPSBfcGVlayhjb250ZXh0KTtcbiAgaWYgKHZhbGlkKSB7XG4gICAgaWYgKCFjaGFyKSB7XG4gICAgICB0aHJvdyBuZXcgVW5leHBlY3RlZEVuZE9mSW5wdXRFeGNlcHRpb24oY29udGV4dCk7XG4gICAgfVxuICAgIGlmICh2YWxpZC5pbmRleE9mKGNoYXIpID09IC0xKSB7XG4gICAgICB0aHJvdyBuZXcgSW52YWxpZEpzb25DaGFyYWN0ZXJFeGNlcHRpb24oY29udGV4dCk7XG4gICAgfVxuICB9XG5cbiAgLy8gTW92ZSB0aGUgcG9zaXRpb24gb2YgdGhlIGNvbnRleHQgdG8gdGhlIG5leHQgY2hhcmFjdGVyLlxuICBfbmV4dChjb250ZXh0KTtcblxuICByZXR1cm4gY2hhcjtcbn1cblxuXG4vKipcbiAqIFJlYWQgdGhlIGV4cG9uZW50IHBhcnQgb2YgYSBudW1iZXIuIFRoZSBleHBvbmVudCBwYXJ0IGlzIGxvb3NlciBmb3IgSlNPTiB0aGFuIHRoZSBudW1iZXJcbiAqIHBhcnQuIGBzdHJgIGlzIHRoZSBzdHJpbmcgb2YgdGhlIG51bWJlciBpdHNlbGYgZm91bmQgc28gZmFyLCBhbmQgc3RhcnQgdGhlIHBvc2l0aW9uXG4gKiB3aGVyZSB0aGUgZnVsbCBudW1iZXIgc3RhcnRlZC4gUmV0dXJucyB0aGUgbm9kZSBmb3VuZC5cbiAqIEBwcml2YXRlXG4gKi9cbmZ1bmN0aW9uIF9yZWFkRXhwTnVtYmVyKGNvbnRleHQ6IEpzb25QYXJzZXJDb250ZXh0LFxuICAgICAgICAgICAgICAgICAgICAgICAgc3RhcnQ6IFBvc2l0aW9uLFxuICAgICAgICAgICAgICAgICAgICAgICAgc3RyOiBzdHJpbmcsXG4gICAgICAgICAgICAgICAgICAgICAgICBjb21tZW50czogKEpzb25Bc3RDb21tZW50IHwgSnNvbkFzdE11bHRpbGluZUNvbW1lbnQpW10pOiBKc29uQXN0TnVtYmVyIHtcbiAgbGV0IGNoYXI7XG4gIGxldCBzaWduZWQgPSBmYWxzZTtcblxuICB3aGlsZSAodHJ1ZSkge1xuICAgIGNoYXIgPSBfdG9rZW4oY29udGV4dCk7XG4gICAgaWYgKGNoYXIgPT0gJysnIHx8IGNoYXIgPT0gJy0nKSB7XG4gICAgICBpZiAoc2lnbmVkKSB7XG4gICAgICAgIGJyZWFrO1xuICAgICAgfVxuICAgICAgc2lnbmVkID0gdHJ1ZTtcbiAgICAgIHN0ciArPSBjaGFyO1xuICAgIH0gZWxzZSBpZiAoY2hhciA9PSAnMCcgfHwgY2hhciA9PSAnMScgfHwgY2hhciA9PSAnMicgfHwgY2hhciA9PSAnMycgfHwgY2hhciA9PSAnNCdcbiAgICAgICAgfHwgY2hhciA9PSAnNScgfHwgY2hhciA9PSAnNicgfHwgY2hhciA9PSAnNycgfHwgY2hhciA9PSAnOCcgfHwgY2hhciA9PSAnOScpIHtcbiAgICAgIHNpZ25lZCA9IHRydWU7XG4gICAgICBzdHIgKz0gY2hhcjtcbiAgICB9IGVsc2Uge1xuICAgICAgYnJlYWs7XG4gICAgfVxuICB9XG5cbiAgLy8gV2UncmUgZG9uZSByZWFkaW5nIHRoaXMgbnVtYmVyLlxuICBjb250ZXh0LnBvc2l0aW9uID0gY29udGV4dC5wcmV2aW91cztcblxuICByZXR1cm4ge1xuICAgIGtpbmQ6ICdudW1iZXInLFxuICAgIHN0YXJ0LFxuICAgIGVuZDogY29udGV4dC5wb3NpdGlvbixcbiAgICB0ZXh0OiBjb250ZXh0Lm9yaWdpbmFsLnN1YnN0cmluZyhzdGFydC5vZmZzZXQsIGNvbnRleHQucG9zaXRpb24ub2Zmc2V0KSxcbiAgICB2YWx1ZTogTnVtYmVyLnBhcnNlRmxvYXQoc3RyKSxcbiAgICBjb21tZW50czogY29tbWVudHMsXG4gIH07XG59XG5cblxuLyoqXG4gKiBSZWFkIHRoZSBoZXhhIHBhcnQgb2YgYSAweEJBRENBRkUgaGV4YWRlY2ltYWwgbnVtYmVyLlxuICogQHByaXZhdGVcbiAqL1xuZnVuY3Rpb24gX3JlYWRIZXhhTnVtYmVyKGNvbnRleHQ6IEpzb25QYXJzZXJDb250ZXh0LFxuICAgICAgICAgICAgICAgICAgICAgICAgIGlzTmVnYXRpdmU6IGJvb2xlYW4sXG4gICAgICAgICAgICAgICAgICAgICAgICAgc3RhcnQ6IFBvc2l0aW9uLFxuICAgICAgICAgICAgICAgICAgICAgICAgIGNvbW1lbnRzOiAoSnNvbkFzdENvbW1lbnQgfCBKc29uQXN0TXVsdGlsaW5lQ29tbWVudClbXSk6IEpzb25Bc3ROdW1iZXIge1xuICAvLyBSZWFkIGFuIGhleGFkZWNpbWFsIG51bWJlciwgdW50aWwgaXQncyBub3QgaGV4YWRlY2ltYWwuXG4gIGxldCBoZXhhID0gJyc7XG4gIGNvbnN0IHZhbGlkID0gJzAxMjM0NTY3ODlhYmNkZWZBQkNERUYnO1xuXG4gIGZvciAobGV0IGNoID0gX3BlZWsoY29udGV4dCk7IGNoICYmIHZhbGlkLmluY2x1ZGVzKGNoKTsgY2ggPSBfcGVlayhjb250ZXh0KSkge1xuICAgIC8vIEFkZCBpdCB0byB0aGUgaGV4YSBzdHJpbmcuXG4gICAgaGV4YSArPSBjaDtcbiAgICAvLyBNb3ZlIHRoZSBwb3NpdGlvbiBvZiB0aGUgY29udGV4dCB0byB0aGUgbmV4dCBjaGFyYWN0ZXIuXG4gICAgX25leHQoY29udGV4dCk7XG4gIH1cblxuICBjb25zdCB2YWx1ZSA9IE51bWJlci5wYXJzZUludChoZXhhLCAxNik7XG5cbiAgLy8gV2UncmUgZG9uZSByZWFkaW5nIHRoaXMgbnVtYmVyLlxuICByZXR1cm4ge1xuICAgIGtpbmQ6ICdudW1iZXInLFxuICAgIHN0YXJ0LFxuICAgIGVuZDogY29udGV4dC5wb3NpdGlvbixcbiAgICB0ZXh0OiBjb250ZXh0Lm9yaWdpbmFsLnN1YnN0cmluZyhzdGFydC5vZmZzZXQsIGNvbnRleHQucG9zaXRpb24ub2Zmc2V0KSxcbiAgICB2YWx1ZTogaXNOZWdhdGl2ZSA/IC12YWx1ZSA6IHZhbHVlLFxuICAgIGNvbW1lbnRzLFxuICB9O1xufVxuXG4vKipcbiAqIFJlYWQgYSBudW1iZXIgZnJvbSB0aGUgY29udGV4dC5cbiAqIEBwcml2YXRlXG4gKi9cbmZ1bmN0aW9uIF9yZWFkTnVtYmVyKGNvbnRleHQ6IEpzb25QYXJzZXJDb250ZXh0LCBjb21tZW50cyA9IF9yZWFkQmxhbmtzKGNvbnRleHQpKTogSnNvbkFzdE51bWJlciB7XG4gIGxldCBzdHIgPSAnJztcbiAgbGV0IGRvdHRlZCA9IGZhbHNlO1xuICBjb25zdCBzdGFydCA9IGNvbnRleHQucG9zaXRpb247XG5cbiAgLy8gcmVhZCB1bnRpbCBgZWAgb3IgZW5kIG9mIGxpbmUuXG4gIHdoaWxlICh0cnVlKSB7XG4gICAgY29uc3QgY2hhciA9IF90b2tlbihjb250ZXh0KTtcblxuICAgIC8vIFJlYWQgdG9rZW5zLCBvbmUgYnkgb25lLlxuICAgIGlmIChjaGFyID09ICctJykge1xuICAgICAgaWYgKHN0ciAhPSAnJykge1xuICAgICAgICB0aHJvdyBuZXcgSW52YWxpZEpzb25DaGFyYWN0ZXJFeGNlcHRpb24oY29udGV4dCk7XG4gICAgICB9XG4gICAgfSBlbHNlIGlmIChjaGFyID09ICdJJ1xuICAgICAgICAmJiAoc3RyID09ICctJyB8fCBzdHIgPT0gJycgfHwgc3RyID09ICcrJylcbiAgICAgICAgJiYgKGNvbnRleHQubW9kZSAmIEpzb25QYXJzZU1vZGUuTnVtYmVyQ29uc3RhbnRzQWxsb3dlZCkgIT0gMCkge1xuICAgICAgLy8gSW5maW5pdHk/XG4gICAgICAvLyBfdG9rZW4oY29udGV4dCwgJ0knKTsgQWxyZWFkeSByZWFkLlxuICAgICAgX3Rva2VuKGNvbnRleHQsICduJyk7XG4gICAgICBfdG9rZW4oY29udGV4dCwgJ2YnKTtcbiAgICAgIF90b2tlbihjb250ZXh0LCAnaScpO1xuICAgICAgX3Rva2VuKGNvbnRleHQsICduJyk7XG4gICAgICBfdG9rZW4oY29udGV4dCwgJ2knKTtcbiAgICAgIF90b2tlbihjb250ZXh0LCAndCcpO1xuICAgICAgX3Rva2VuKGNvbnRleHQsICd5Jyk7XG5cbiAgICAgIHN0ciArPSAnSW5maW5pdHknO1xuICAgICAgYnJlYWs7XG4gICAgfSBlbHNlIGlmIChjaGFyID09ICcwJykge1xuICAgICAgaWYgKHN0ciA9PSAnMCcgfHwgc3RyID09ICctMCcpIHtcbiAgICAgICAgdGhyb3cgbmV3IEludmFsaWRKc29uQ2hhcmFjdGVyRXhjZXB0aW9uKGNvbnRleHQpO1xuICAgICAgfVxuICAgIH0gZWxzZSBpZiAoY2hhciA9PSAnMScgfHwgY2hhciA9PSAnMicgfHwgY2hhciA9PSAnMycgfHwgY2hhciA9PSAnNCcgfHwgY2hhciA9PSAnNSdcbiAgICAgICAgfHwgY2hhciA9PSAnNicgfHwgY2hhciA9PSAnNycgfHwgY2hhciA9PSAnOCcgfHwgY2hhciA9PSAnOScpIHtcbiAgICAgIGlmIChzdHIgPT0gJzAnIHx8IHN0ciA9PSAnLTAnKSB7XG4gICAgICAgIHRocm93IG5ldyBJbnZhbGlkSnNvbkNoYXJhY3RlckV4Y2VwdGlvbihjb250ZXh0KTtcbiAgICAgIH1cbiAgICB9IGVsc2UgaWYgKGNoYXIgPT0gJysnICYmIHN0ciA9PSAnJykge1xuICAgICAgLy8gUGFzcyBvdmVyLlxuICAgIH0gZWxzZSBpZiAoY2hhciA9PSAnLicpIHtcbiAgICAgIGlmIChkb3R0ZWQpIHtcbiAgICAgICAgdGhyb3cgbmV3IEludmFsaWRKc29uQ2hhcmFjdGVyRXhjZXB0aW9uKGNvbnRleHQpO1xuICAgICAgfVxuICAgICAgZG90dGVkID0gdHJ1ZTtcbiAgICB9IGVsc2UgaWYgKGNoYXIgPT0gJ2UnIHx8IGNoYXIgPT0gJ0UnKSB7XG4gICAgICByZXR1cm4gX3JlYWRFeHBOdW1iZXIoY29udGV4dCwgc3RhcnQsIHN0ciArIGNoYXIsIGNvbW1lbnRzKTtcbiAgICB9IGVsc2UgaWYgKGNoYXIgPT0gJ3gnICYmIChzdHIgPT0gJzAnIHx8IHN0ciA9PSAnLTAnKVxuICAgICAgICAgICAgICAgJiYgKGNvbnRleHQubW9kZSAmIEpzb25QYXJzZU1vZGUuSGV4YWRlY2ltYWxOdW1iZXJBbGxvd2VkKSAhPSAwKSB7XG4gICAgICByZXR1cm4gX3JlYWRIZXhhTnVtYmVyKGNvbnRleHQsIHN0ciA9PSAnLTAnLCBzdGFydCwgY29tbWVudHMpO1xuICAgIH0gZWxzZSB7XG4gICAgICAvLyBXZSByZWFkIG9uZSB0b28gbWFueSBjaGFyYWN0ZXJzLCBzbyByb2xsYmFjayB0aGUgbGFzdCBjaGFyYWN0ZXIuXG4gICAgICBjb250ZXh0LnBvc2l0aW9uID0gY29udGV4dC5wcmV2aW91cztcbiAgICAgIGJyZWFrO1xuICAgIH1cblxuICAgIHN0ciArPSBjaGFyO1xuICB9XG5cbiAgLy8gV2UncmUgZG9uZSByZWFkaW5nIHRoaXMgbnVtYmVyLlxuICBpZiAoc3RyLmVuZHNXaXRoKCcuJykgJiYgKGNvbnRleHQubW9kZSAmIEpzb25QYXJzZU1vZGUuSGV4YWRlY2ltYWxOdW1iZXJBbGxvd2VkKSA9PSAwKSB7XG4gICAgdGhyb3cgbmV3IEludmFsaWRKc29uQ2hhcmFjdGVyRXhjZXB0aW9uKGNvbnRleHQpO1xuICB9XG5cbiAgcmV0dXJuIHtcbiAgICBraW5kOiAnbnVtYmVyJyxcbiAgICBzdGFydCxcbiAgICBlbmQ6IGNvbnRleHQucG9zaXRpb24sXG4gICAgdGV4dDogY29udGV4dC5vcmlnaW5hbC5zdWJzdHJpbmcoc3RhcnQub2Zmc2V0LCBjb250ZXh0LnBvc2l0aW9uLm9mZnNldCksXG4gICAgdmFsdWU6IE51bWJlci5wYXJzZUZsb2F0KHN0ciksXG4gICAgY29tbWVudHMsXG4gIH07XG59XG5cblxuLyoqXG4gKiBSZWFkIGEgc3RyaW5nIGZyb20gdGhlIGNvbnRleHQuIFRha2VzIHRoZSBjb21tZW50cyBvZiB0aGUgc3RyaW5nIG9yIHJlYWQgdGhlIGJsYW5rcyBiZWZvcmUgdGhlXG4gKiBzdHJpbmcuXG4gKiBAcHJpdmF0ZVxuICovXG5mdW5jdGlvbiBfcmVhZFN0cmluZyhjb250ZXh0OiBKc29uUGFyc2VyQ29udGV4dCwgY29tbWVudHMgPSBfcmVhZEJsYW5rcyhjb250ZXh0KSk6IEpzb25Bc3RTdHJpbmcge1xuICBjb25zdCBzdGFydCA9IGNvbnRleHQucG9zaXRpb247XG5cbiAgLy8gQ29uc3VtZSB0aGUgZmlyc3Qgc3RyaW5nIGRlbGltaXRlci5cbiAgY29uc3QgZGVsaW0gPSBfdG9rZW4oY29udGV4dCk7XG4gIGlmICgoY29udGV4dC5tb2RlICYgSnNvblBhcnNlTW9kZS5TaW5nbGVRdW90ZXNBbGxvd2VkKSA9PSAwKSB7XG4gICAgaWYgKGRlbGltID09ICdcXCcnKSB7XG4gICAgICB0aHJvdyBuZXcgSW52YWxpZEpzb25DaGFyYWN0ZXJFeGNlcHRpb24oY29udGV4dCk7XG4gICAgfVxuICB9XG5cbiAgbGV0IHN0ciA9ICcnO1xuICB3aGlsZSAodHJ1ZSkge1xuICAgIGxldCBjaGFyID0gX3Rva2VuKGNvbnRleHQpO1xuICAgIGlmIChjaGFyID09IGRlbGltKSB7XG4gICAgICByZXR1cm4ge1xuICAgICAgICBraW5kOiAnc3RyaW5nJyxcbiAgICAgICAgc3RhcnQsXG4gICAgICAgIGVuZDogY29udGV4dC5wb3NpdGlvbixcbiAgICAgICAgdGV4dDogY29udGV4dC5vcmlnaW5hbC5zdWJzdHJpbmcoc3RhcnQub2Zmc2V0LCBjb250ZXh0LnBvc2l0aW9uLm9mZnNldCksXG4gICAgICAgIHZhbHVlOiBzdHIsXG4gICAgICAgIGNvbW1lbnRzOiBjb21tZW50cyxcbiAgICAgIH07XG4gICAgfSBlbHNlIGlmIChjaGFyID09ICdcXFxcJykge1xuICAgICAgY2hhciA9IF90b2tlbihjb250ZXh0KTtcbiAgICAgIHN3aXRjaCAoY2hhcikge1xuICAgICAgICBjYXNlICdcXFxcJzpcbiAgICAgICAgY2FzZSAnXFwvJzpcbiAgICAgICAgY2FzZSAnXCInOlxuICAgICAgICBjYXNlIGRlbGltOlxuICAgICAgICAgIHN0ciArPSBjaGFyO1xuICAgICAgICAgIGJyZWFrO1xuXG4gICAgICAgIGNhc2UgJ2InOiBzdHIgKz0gJ1xcYic7IGJyZWFrO1xuICAgICAgICBjYXNlICdmJzogc3RyICs9ICdcXGYnOyBicmVhaztcbiAgICAgICAgY2FzZSAnbic6IHN0ciArPSAnXFxuJzsgYnJlYWs7XG4gICAgICAgIGNhc2UgJ3InOiBzdHIgKz0gJ1xccic7IGJyZWFrO1xuICAgICAgICBjYXNlICd0Jzogc3RyICs9ICdcXHQnOyBicmVhaztcbiAgICAgICAgY2FzZSAndSc6XG4gICAgICAgICAgY29uc3QgW2MwXSA9IF90b2tlbihjb250ZXh0LCAnMDEyMzQ1Njc4OWFiY2RlZkFCQ0RFRicpO1xuICAgICAgICAgIGNvbnN0IFtjMV0gPSBfdG9rZW4oY29udGV4dCwgJzAxMjM0NTY3ODlhYmNkZWZBQkNERUYnKTtcbiAgICAgICAgICBjb25zdCBbYzJdID0gX3Rva2VuKGNvbnRleHQsICcwMTIzNDU2Nzg5YWJjZGVmQUJDREVGJyk7XG4gICAgICAgICAgY29uc3QgW2MzXSA9IF90b2tlbihjb250ZXh0LCAnMDEyMzQ1Njc4OWFiY2RlZkFCQ0RFRicpO1xuICAgICAgICAgIHN0ciArPSBTdHJpbmcuZnJvbUNoYXJDb2RlKHBhcnNlSW50KGMwICsgYzEgKyBjMiArIGMzLCAxNikpO1xuICAgICAgICAgIGJyZWFrO1xuXG4gICAgICAgIGNhc2UgdW5kZWZpbmVkOlxuICAgICAgICAgIHRocm93IG5ldyBVbmV4cGVjdGVkRW5kT2ZJbnB1dEV4Y2VwdGlvbihjb250ZXh0KTtcblxuICAgICAgICBjYXNlICdcXG4nOlxuICAgICAgICAgIC8vIE9ubHkgdmFsaWQgd2hlbiBtdWx0aWxpbmUgc3RyaW5ncyBhcmUgYWxsb3dlZC5cbiAgICAgICAgICBpZiAoKGNvbnRleHQubW9kZSAmIEpzb25QYXJzZU1vZGUuTXVsdGlMaW5lU3RyaW5nQWxsb3dlZCkgPT0gMCkge1xuICAgICAgICAgICAgdGhyb3cgbmV3IEludmFsaWRKc29uQ2hhcmFjdGVyRXhjZXB0aW9uKGNvbnRleHQpO1xuICAgICAgICAgIH1cbiAgICAgICAgICBzdHIgKz0gY2hhcjtcbiAgICAgICAgICBicmVhaztcblxuICAgICAgICBkZWZhdWx0OlxuICAgICAgICAgIHRocm93IG5ldyBJbnZhbGlkSnNvbkNoYXJhY3RlckV4Y2VwdGlvbihjb250ZXh0KTtcbiAgICAgIH1cbiAgICB9IGVsc2UgaWYgKGNoYXIgPT09IHVuZGVmaW5lZCkge1xuICAgICAgdGhyb3cgbmV3IFVuZXhwZWN0ZWRFbmRPZklucHV0RXhjZXB0aW9uKGNvbnRleHQpO1xuICAgIH0gZWxzZSBpZiAoY2hhciA9PSAnXFxiJyB8fCBjaGFyID09ICdcXGYnIHx8IGNoYXIgPT0gJ1xcbicgfHwgY2hhciA9PSAnXFxyJyB8fCBjaGFyID09ICdcXHQnKSB7XG4gICAgICB0aHJvdyBuZXcgSW52YWxpZEpzb25DaGFyYWN0ZXJFeGNlcHRpb24oY29udGV4dCk7XG4gICAgfSBlbHNlIHtcbiAgICAgIHN0ciArPSBjaGFyO1xuICAgIH1cbiAgfVxufVxuXG5cbi8qKlxuICogUmVhZCB0aGUgY29uc3RhbnQgYHRydWVgIGZyb20gdGhlIGNvbnRleHQuXG4gKiBAcHJpdmF0ZVxuICovXG5mdW5jdGlvbiBfcmVhZFRydWUoY29udGV4dDogSnNvblBhcnNlckNvbnRleHQsXG4gICAgICAgICAgICAgICAgICAgY29tbWVudHMgPSBfcmVhZEJsYW5rcyhjb250ZXh0KSk6IEpzb25Bc3RDb25zdGFudFRydWUge1xuICBjb25zdCBzdGFydCA9IGNvbnRleHQucG9zaXRpb247XG4gIF90b2tlbihjb250ZXh0LCAndCcpO1xuICBfdG9rZW4oY29udGV4dCwgJ3InKTtcbiAgX3Rva2VuKGNvbnRleHQsICd1Jyk7XG4gIF90b2tlbihjb250ZXh0LCAnZScpO1xuXG4gIGNvbnN0IGVuZCA9IGNvbnRleHQucG9zaXRpb247XG5cbiAgcmV0dXJuIHtcbiAgICBraW5kOiAndHJ1ZScsXG4gICAgc3RhcnQsXG4gICAgZW5kLFxuICAgIHRleHQ6IGNvbnRleHQub3JpZ2luYWwuc3Vic3RyaW5nKHN0YXJ0Lm9mZnNldCwgZW5kLm9mZnNldCksXG4gICAgdmFsdWU6IHRydWUsXG4gICAgY29tbWVudHMsXG4gIH07XG59XG5cblxuLyoqXG4gKiBSZWFkIHRoZSBjb25zdGFudCBgZmFsc2VgIGZyb20gdGhlIGNvbnRleHQuXG4gKiBAcHJpdmF0ZVxuICovXG5mdW5jdGlvbiBfcmVhZEZhbHNlKGNvbnRleHQ6IEpzb25QYXJzZXJDb250ZXh0LFxuICAgICAgICAgICAgICAgICAgICBjb21tZW50cyA9IF9yZWFkQmxhbmtzKGNvbnRleHQpKTogSnNvbkFzdENvbnN0YW50RmFsc2Uge1xuICBjb25zdCBzdGFydCA9IGNvbnRleHQucG9zaXRpb247XG4gIF90b2tlbihjb250ZXh0LCAnZicpO1xuICBfdG9rZW4oY29udGV4dCwgJ2EnKTtcbiAgX3Rva2VuKGNvbnRleHQsICdsJyk7XG4gIF90b2tlbihjb250ZXh0LCAncycpO1xuICBfdG9rZW4oY29udGV4dCwgJ2UnKTtcblxuICBjb25zdCBlbmQgPSBjb250ZXh0LnBvc2l0aW9uO1xuXG4gIHJldHVybiB7XG4gICAga2luZDogJ2ZhbHNlJyxcbiAgICBzdGFydCxcbiAgICBlbmQsXG4gICAgdGV4dDogY29udGV4dC5vcmlnaW5hbC5zdWJzdHJpbmcoc3RhcnQub2Zmc2V0LCBlbmQub2Zmc2V0KSxcbiAgICB2YWx1ZTogZmFsc2UsXG4gICAgY29tbWVudHMsXG4gIH07XG59XG5cblxuLyoqXG4gKiBSZWFkIHRoZSBjb25zdGFudCBgbnVsbGAgZnJvbSB0aGUgY29udGV4dC5cbiAqIEBwcml2YXRlXG4gKi9cbmZ1bmN0aW9uIF9yZWFkTnVsbChjb250ZXh0OiBKc29uUGFyc2VyQ29udGV4dCxcbiAgICAgICAgICAgICAgICAgICBjb21tZW50cyA9IF9yZWFkQmxhbmtzKGNvbnRleHQpKTogSnNvbkFzdENvbnN0YW50TnVsbCB7XG4gIGNvbnN0IHN0YXJ0ID0gY29udGV4dC5wb3NpdGlvbjtcblxuICBfdG9rZW4oY29udGV4dCwgJ24nKTtcbiAgX3Rva2VuKGNvbnRleHQsICd1Jyk7XG4gIF90b2tlbihjb250ZXh0LCAnbCcpO1xuICBfdG9rZW4oY29udGV4dCwgJ2wnKTtcblxuICBjb25zdCBlbmQgPSBjb250ZXh0LnBvc2l0aW9uO1xuXG4gIHJldHVybiB7XG4gICAga2luZDogJ251bGwnLFxuICAgIHN0YXJ0LFxuICAgIGVuZCxcbiAgICB0ZXh0OiBjb250ZXh0Lm9yaWdpbmFsLnN1YnN0cmluZyhzdGFydC5vZmZzZXQsIGVuZC5vZmZzZXQpLFxuICAgIHZhbHVlOiBudWxsLFxuICAgIGNvbW1lbnRzOiBjb21tZW50cyxcbiAgfTtcbn1cblxuXG4vKipcbiAqIFJlYWQgdGhlIGNvbnN0YW50IGBOYU5gIGZyb20gdGhlIGNvbnRleHQuXG4gKiBAcHJpdmF0ZVxuICovXG5mdW5jdGlvbiBfcmVhZE5hTihjb250ZXh0OiBKc29uUGFyc2VyQ29udGV4dCxcbiAgICAgICAgICAgICAgICAgIGNvbW1lbnRzID0gX3JlYWRCbGFua3MoY29udGV4dCkpOiBKc29uQXN0TnVtYmVyIHtcbiAgY29uc3Qgc3RhcnQgPSBjb250ZXh0LnBvc2l0aW9uO1xuXG4gIF90b2tlbihjb250ZXh0LCAnTicpO1xuICBfdG9rZW4oY29udGV4dCwgJ2EnKTtcbiAgX3Rva2VuKGNvbnRleHQsICdOJyk7XG5cbiAgY29uc3QgZW5kID0gY29udGV4dC5wb3NpdGlvbjtcblxuICByZXR1cm4ge1xuICAgIGtpbmQ6ICdudW1iZXInLFxuICAgIHN0YXJ0LFxuICAgIGVuZCxcbiAgICB0ZXh0OiBjb250ZXh0Lm9yaWdpbmFsLnN1YnN0cmluZyhzdGFydC5vZmZzZXQsIGVuZC5vZmZzZXQpLFxuICAgIHZhbHVlOiBOYU4sXG4gICAgY29tbWVudHM6IGNvbW1lbnRzLFxuICB9O1xufVxuXG5cbi8qKlxuICogUmVhZCBhbiBhcnJheSBvZiBKU09OIHZhbHVlcyBmcm9tIHRoZSBjb250ZXh0LlxuICogQHByaXZhdGVcbiAqL1xuZnVuY3Rpb24gX3JlYWRBcnJheShjb250ZXh0OiBKc29uUGFyc2VyQ29udGV4dCwgY29tbWVudHMgPSBfcmVhZEJsYW5rcyhjb250ZXh0KSk6IEpzb25Bc3RBcnJheSB7XG4gIGNvbnN0IHN0YXJ0ID0gY29udGV4dC5wb3NpdGlvbjtcblxuICAvLyBDb25zdW1lIHRoZSBmaXJzdCBkZWxpbWl0ZXIuXG4gIF90b2tlbihjb250ZXh0LCAnWycpO1xuICBjb25zdCB2YWx1ZTogSnNvbkFycmF5ID0gW107XG4gIGNvbnN0IGVsZW1lbnRzOiBKc29uQXN0Tm9kZVtdID0gW107XG5cbiAgX3JlYWRCbGFua3MoY29udGV4dCk7XG4gIGlmIChfcGVlayhjb250ZXh0KSAhPSAnXScpIHtcbiAgICBjb25zdCBub2RlID0gX3JlYWRWYWx1ZShjb250ZXh0KTtcbiAgICBlbGVtZW50cy5wdXNoKG5vZGUpO1xuICAgIHZhbHVlLnB1c2gobm9kZS52YWx1ZSk7XG4gIH1cblxuICB3aGlsZSAoX3BlZWsoY29udGV4dCkgIT0gJ10nKSB7XG4gICAgX3Rva2VuKGNvbnRleHQsICcsJyk7XG5cbiAgICBjb25zdCB2YWx1ZUNvbW1lbnRzID0gX3JlYWRCbGFua3MoY29udGV4dCk7XG4gICAgaWYgKChjb250ZXh0Lm1vZGUgJiBKc29uUGFyc2VNb2RlLlRyYWlsaW5nQ29tbWFzQWxsb3dlZCkgIT09IDAgJiYgX3BlZWsoY29udGV4dCkgPT09ICddJykge1xuICAgICAgYnJlYWs7XG4gICAgfVxuICAgIGNvbnN0IG5vZGUgPSBfcmVhZFZhbHVlKGNvbnRleHQsIHZhbHVlQ29tbWVudHMpO1xuICAgIGVsZW1lbnRzLnB1c2gobm9kZSk7XG4gICAgdmFsdWUucHVzaChub2RlLnZhbHVlKTtcbiAgfVxuXG4gIF90b2tlbihjb250ZXh0LCAnXScpO1xuXG4gIHJldHVybiB7XG4gICAga2luZDogJ2FycmF5JyxcbiAgICBzdGFydCxcbiAgICBlbmQ6IGNvbnRleHQucG9zaXRpb24sXG4gICAgdGV4dDogY29udGV4dC5vcmlnaW5hbC5zdWJzdHJpbmcoc3RhcnQub2Zmc2V0LCBjb250ZXh0LnBvc2l0aW9uLm9mZnNldCksXG4gICAgdmFsdWUsXG4gICAgZWxlbWVudHMsXG4gICAgY29tbWVudHMsXG4gIH07XG59XG5cblxuLyoqXG4gKiBSZWFkIGFuIGlkZW50aWZpZXIgZnJvbSB0aGUgY29udGV4dC4gQW4gaWRlbnRpZmllciBpcyBhIHZhbGlkIEphdmFTY3JpcHQgaWRlbnRpZmllciwgYW5kIHRoaXNcbiAqIGZ1bmN0aW9uIGlzIG9ubHkgdXNlZCBpbiBMb29zZSBtb2RlLlxuICogQHByaXZhdGVcbiAqL1xuZnVuY3Rpb24gX3JlYWRJZGVudGlmaWVyKGNvbnRleHQ6IEpzb25QYXJzZXJDb250ZXh0LFxuICAgICAgICAgICAgICAgICAgICAgICAgIGNvbW1lbnRzID0gX3JlYWRCbGFua3MoY29udGV4dCkpOiBKc29uQXN0SWRlbnRpZmllciB7XG4gIGNvbnN0IHN0YXJ0ID0gY29udGV4dC5wb3NpdGlvbjtcblxuICBsZXQgY2hhciA9IF9wZWVrKGNvbnRleHQpO1xuICBpZiAoY2hhciAmJiAnMDEyMzQ1Njc4OScuaW5kZXhPZihjaGFyKSAhPSAtMSkge1xuICAgIGNvbnN0IGlkZW50aWZpZXJOb2RlID0gX3JlYWROdW1iZXIoY29udGV4dCk7XG5cbiAgICByZXR1cm4ge1xuICAgICAga2luZDogJ2lkZW50aWZpZXInLFxuICAgICAgc3RhcnQsXG4gICAgICBlbmQ6IGlkZW50aWZpZXJOb2RlLmVuZCxcbiAgICAgIHRleHQ6IGlkZW50aWZpZXJOb2RlLnRleHQsXG4gICAgICB2YWx1ZTogaWRlbnRpZmllck5vZGUudmFsdWUudG9TdHJpbmcoKSxcbiAgICB9O1xuICB9XG5cbiAgY29uc3QgaWRlbnRWYWxpZEZpcnN0Q2hhciA9ICdhYmNkZWZnaGlqa2xtbm9wcXJzdHV2d3h5ekFCQ0RFRkdISUpLTE1PUFFSU1RVVldYWVonO1xuICBjb25zdCBpZGVudFZhbGlkQ2hhciA9ICdfJGFiY2RlZmdoaWprbG1ub3BxcnN0dXZ3eHl6QUJDREVGR0hJSktMTU9QUVJTVFVWV1hZWjAxMjM0NTY3ODknO1xuICBsZXQgZmlyc3QgPSB0cnVlO1xuICBsZXQgdmFsdWUgPSAnJztcblxuICB3aGlsZSAodHJ1ZSkge1xuICAgIGNoYXIgPSBfdG9rZW4oY29udGV4dCk7XG4gICAgaWYgKGNoYXIgPT0gdW5kZWZpbmVkXG4gICAgICAgIHx8IChmaXJzdCA/IGlkZW50VmFsaWRGaXJzdENoYXIuaW5kZXhPZihjaGFyKSA6IGlkZW50VmFsaWRDaGFyLmluZGV4T2YoY2hhcikpID09IC0xKSB7XG4gICAgICBjb250ZXh0LnBvc2l0aW9uID0gY29udGV4dC5wcmV2aW91cztcblxuICAgICAgcmV0dXJuIHtcbiAgICAgICAga2luZDogJ2lkZW50aWZpZXInLFxuICAgICAgICBzdGFydCxcbiAgICAgICAgZW5kOiBjb250ZXh0LnBvc2l0aW9uLFxuICAgICAgICB0ZXh0OiBjb250ZXh0Lm9yaWdpbmFsLnN1YnN0cihzdGFydC5vZmZzZXQsIGNvbnRleHQucG9zaXRpb24ub2Zmc2V0KSxcbiAgICAgICAgdmFsdWUsXG4gICAgICAgIGNvbW1lbnRzLFxuICAgICAgfTtcbiAgICB9XG5cbiAgICB2YWx1ZSArPSBjaGFyO1xuICAgIGZpcnN0ID0gZmFsc2U7XG4gIH1cbn1cblxuXG4vKipcbiAqIFJlYWQgYSBwcm9wZXJ0eSBmcm9tIHRoZSBjb250ZXh0LiBBIHByb3BlcnR5IGlzIGEgc3RyaW5nIG9yIChpbiBMb29zZSBtb2RlIG9ubHkpIGEgbnVtYmVyIG9yXG4gKiBhbiBpZGVudGlmaWVyLCBmb2xsb3dlZCBieSBhIGNvbG9uIGA6YC5cbiAqIEBwcml2YXRlXG4gKi9cbmZ1bmN0aW9uIF9yZWFkUHJvcGVydHkoY29udGV4dDogSnNvblBhcnNlckNvbnRleHQsXG4gICAgICAgICAgICAgICAgICAgICAgIGNvbW1lbnRzID0gX3JlYWRCbGFua3MoY29udGV4dCkpOiBKc29uQXN0S2V5VmFsdWUge1xuICBjb25zdCBzdGFydCA9IGNvbnRleHQucG9zaXRpb247XG5cbiAgbGV0IGtleTtcbiAgaWYgKChjb250ZXh0Lm1vZGUgJiBKc29uUGFyc2VNb2RlLklkZW50aWZpZXJLZXlOYW1lc0FsbG93ZWQpICE9IDApIHtcbiAgICBjb25zdCB0b3AgPSBfcGVlayhjb250ZXh0KTtcbiAgICBpZiAodG9wID09ICdcIicgfHwgdG9wID09ICdcXCcnKSB7XG4gICAgICBrZXkgPSBfcmVhZFN0cmluZyhjb250ZXh0KTtcbiAgICB9IGVsc2Uge1xuICAgICAga2V5ID0gX3JlYWRJZGVudGlmaWVyKGNvbnRleHQpO1xuICAgIH1cbiAgfSBlbHNlIHtcbiAgICBrZXkgPSBfcmVhZFN0cmluZyhjb250ZXh0KTtcbiAgfVxuXG4gIF9yZWFkQmxhbmtzKGNvbnRleHQpO1xuICBfdG9rZW4oY29udGV4dCwgJzonKTtcbiAgY29uc3QgdmFsdWUgPSBfcmVhZFZhbHVlKGNvbnRleHQpO1xuICBjb25zdCBlbmQgPSBjb250ZXh0LnBvc2l0aW9uO1xuXG4gIHJldHVybiB7XG4gICAga2luZDogJ2tleXZhbHVlJyxcbiAgICBrZXksXG4gICAgdmFsdWUsXG4gICAgc3RhcnQsXG4gICAgZW5kLFxuICAgIHRleHQ6IGNvbnRleHQub3JpZ2luYWwuc3Vic3RyaW5nKHN0YXJ0Lm9mZnNldCwgZW5kLm9mZnNldCksXG4gICAgY29tbWVudHMsXG4gIH07XG59XG5cblxuLyoqXG4gKiBSZWFkIGFuIG9iamVjdCBvZiBwcm9wZXJ0aWVzIC0+IEpTT04gdmFsdWVzIGZyb20gdGhlIGNvbnRleHQuXG4gKiBAcHJpdmF0ZVxuICovXG5mdW5jdGlvbiBfcmVhZE9iamVjdChjb250ZXh0OiBKc29uUGFyc2VyQ29udGV4dCxcbiAgICAgICAgICAgICAgICAgICAgIGNvbW1lbnRzID0gX3JlYWRCbGFua3MoY29udGV4dCkpOiBKc29uQXN0T2JqZWN0IHtcbiAgY29uc3Qgc3RhcnQgPSBjb250ZXh0LnBvc2l0aW9uO1xuICAvLyBDb25zdW1lIHRoZSBmaXJzdCBkZWxpbWl0ZXIuXG4gIF90b2tlbihjb250ZXh0LCAneycpO1xuICBjb25zdCB2YWx1ZTogSnNvbk9iamVjdCA9IHt9O1xuICBjb25zdCBwcm9wZXJ0aWVzOiBKc29uQXN0S2V5VmFsdWVbXSA9IFtdO1xuXG4gIF9yZWFkQmxhbmtzKGNvbnRleHQpO1xuICBpZiAoX3BlZWsoY29udGV4dCkgIT0gJ30nKSB7XG4gICAgY29uc3QgcHJvcGVydHkgPSBfcmVhZFByb3BlcnR5KGNvbnRleHQpO1xuICAgIHZhbHVlW3Byb3BlcnR5LmtleS52YWx1ZV0gPSBwcm9wZXJ0eS52YWx1ZS52YWx1ZTtcbiAgICBwcm9wZXJ0aWVzLnB1c2gocHJvcGVydHkpO1xuXG4gICAgd2hpbGUgKF9wZWVrKGNvbnRleHQpICE9ICd9Jykge1xuICAgICAgX3Rva2VuKGNvbnRleHQsICcsJyk7XG5cbiAgICAgIGNvbnN0IHByb3BlcnR5Q29tbWVudHMgPSBfcmVhZEJsYW5rcyhjb250ZXh0KTtcbiAgICAgIGlmICgoY29udGV4dC5tb2RlICYgSnNvblBhcnNlTW9kZS5UcmFpbGluZ0NvbW1hc0FsbG93ZWQpICE9PSAwICYmIF9wZWVrKGNvbnRleHQpID09PSAnfScpIHtcbiAgICAgICAgYnJlYWs7XG4gICAgICB9XG4gICAgICBjb25zdCBwcm9wZXJ0eSA9IF9yZWFkUHJvcGVydHkoY29udGV4dCwgcHJvcGVydHlDb21tZW50cyk7XG4gICAgICB2YWx1ZVtwcm9wZXJ0eS5rZXkudmFsdWVdID0gcHJvcGVydHkudmFsdWUudmFsdWU7XG4gICAgICBwcm9wZXJ0aWVzLnB1c2gocHJvcGVydHkpO1xuICAgIH1cbiAgfVxuXG4gIF90b2tlbihjb250ZXh0LCAnfScpO1xuXG4gIHJldHVybiB7XG4gICAga2luZDogJ29iamVjdCcsXG4gICAgcHJvcGVydGllcyxcbiAgICBzdGFydCxcbiAgICBlbmQ6IGNvbnRleHQucG9zaXRpb24sXG4gICAgdmFsdWUsXG4gICAgdGV4dDogY29udGV4dC5vcmlnaW5hbC5zdWJzdHJpbmcoc3RhcnQub2Zmc2V0LCBjb250ZXh0LnBvc2l0aW9uLm9mZnNldCksXG4gICAgY29tbWVudHMsXG4gIH07XG59XG5cblxuLyoqXG4gKiBSZW1vdmUgYW55IGJsYW5rIGNoYXJhY3RlciBvciBjb21tZW50cyAoaW4gTG9vc2UgbW9kZSkgZnJvbSB0aGUgY29udGV4dCwgcmV0dXJuaW5nIGFuIGFycmF5XG4gKiBvZiBjb21tZW50cyBpZiBhbnkgYXJlIGZvdW5kLlxuICogQHByaXZhdGVcbiAqL1xuZnVuY3Rpb24gX3JlYWRCbGFua3MoY29udGV4dDogSnNvblBhcnNlckNvbnRleHQpOiAoSnNvbkFzdENvbW1lbnQgfCBKc29uQXN0TXVsdGlsaW5lQ29tbWVudClbXSB7XG4gIGlmICgoY29udGV4dC5tb2RlICYgSnNvblBhcnNlTW9kZS5Db21tZW50c0FsbG93ZWQpICE9IDApIHtcbiAgICBjb25zdCBjb21tZW50czogKEpzb25Bc3RDb21tZW50IHwgSnNvbkFzdE11bHRpbGluZUNvbW1lbnQpW10gPSBbXTtcbiAgICB3aGlsZSAodHJ1ZSkge1xuICAgICAgY29uc3QgY2hhciA9IGNvbnRleHQub3JpZ2luYWxbY29udGV4dC5wb3NpdGlvbi5vZmZzZXRdO1xuICAgICAgaWYgKGNoYXIgPT0gJy8nICYmIGNvbnRleHQub3JpZ2luYWxbY29udGV4dC5wb3NpdGlvbi5vZmZzZXQgKyAxXSA9PSAnKicpIHtcbiAgICAgICAgY29uc3Qgc3RhcnQgPSBjb250ZXh0LnBvc2l0aW9uO1xuICAgICAgICAvLyBNdWx0aSBsaW5lIGNvbW1lbnQuXG4gICAgICAgIF9uZXh0KGNvbnRleHQpO1xuICAgICAgICBfbmV4dChjb250ZXh0KTtcblxuICAgICAgICB3aGlsZSAoY29udGV4dC5vcmlnaW5hbFtjb250ZXh0LnBvc2l0aW9uLm9mZnNldF0gIT0gJyonXG4gICAgICAgICAgICB8fCBjb250ZXh0Lm9yaWdpbmFsW2NvbnRleHQucG9zaXRpb24ub2Zmc2V0ICsgMV0gIT0gJy8nKSB7XG4gICAgICAgICAgX25leHQoY29udGV4dCk7XG4gICAgICAgICAgaWYgKGNvbnRleHQucG9zaXRpb24ub2Zmc2V0ID49IGNvbnRleHQub3JpZ2luYWwubGVuZ3RoKSB7XG4gICAgICAgICAgICB0aHJvdyBuZXcgVW5leHBlY3RlZEVuZE9mSW5wdXRFeGNlcHRpb24oY29udGV4dCk7XG4gICAgICAgICAgfVxuICAgICAgICB9XG4gICAgICAgIC8vIFJlbW92ZSBcIiovXCIuXG4gICAgICAgIF9uZXh0KGNvbnRleHQpO1xuICAgICAgICBfbmV4dChjb250ZXh0KTtcblxuICAgICAgICBjb21tZW50cy5wdXNoKHtcbiAgICAgICAgICBraW5kOiAnbXVsdGljb21tZW50JyxcbiAgICAgICAgICBzdGFydCxcbiAgICAgICAgICBlbmQ6IGNvbnRleHQucG9zaXRpb24sXG4gICAgICAgICAgdGV4dDogY29udGV4dC5vcmlnaW5hbC5zdWJzdHJpbmcoc3RhcnQub2Zmc2V0LCBjb250ZXh0LnBvc2l0aW9uLm9mZnNldCksXG4gICAgICAgICAgY29udGVudDogY29udGV4dC5vcmlnaW5hbC5zdWJzdHJpbmcoc3RhcnQub2Zmc2V0ICsgMiwgY29udGV4dC5wb3NpdGlvbi5vZmZzZXQgLSAyKSxcbiAgICAgICAgfSk7XG4gICAgICB9IGVsc2UgaWYgKGNoYXIgPT0gJy8nICYmIGNvbnRleHQub3JpZ2luYWxbY29udGV4dC5wb3NpdGlvbi5vZmZzZXQgKyAxXSA9PSAnLycpIHtcbiAgICAgICAgY29uc3Qgc3RhcnQgPSBjb250ZXh0LnBvc2l0aW9uO1xuICAgICAgICAvLyBNdWx0aSBsaW5lIGNvbW1lbnQuXG4gICAgICAgIF9uZXh0KGNvbnRleHQpO1xuICAgICAgICBfbmV4dChjb250ZXh0KTtcblxuICAgICAgICB3aGlsZSAoY29udGV4dC5vcmlnaW5hbFtjb250ZXh0LnBvc2l0aW9uLm9mZnNldF0gIT0gJ1xcbicpIHtcbiAgICAgICAgICBfbmV4dChjb250ZXh0KTtcbiAgICAgICAgICBpZiAoY29udGV4dC5wb3NpdGlvbi5vZmZzZXQgPj0gY29udGV4dC5vcmlnaW5hbC5sZW5ndGgpIHtcbiAgICAgICAgICAgIGJyZWFrO1xuICAgICAgICAgIH1cbiAgICAgICAgfVxuXG4gICAgICAgIC8vIFJlbW92ZSBcIlxcblwiLlxuICAgICAgICBpZiAoY29udGV4dC5wb3NpdGlvbi5vZmZzZXQgPCBjb250ZXh0Lm9yaWdpbmFsLmxlbmd0aCkge1xuICAgICAgICAgIF9uZXh0KGNvbnRleHQpO1xuICAgICAgICB9XG4gICAgICAgIGNvbW1lbnRzLnB1c2goe1xuICAgICAgICAgIGtpbmQ6ICdjb21tZW50JyxcbiAgICAgICAgICBzdGFydCxcbiAgICAgICAgICBlbmQ6IGNvbnRleHQucG9zaXRpb24sXG4gICAgICAgICAgdGV4dDogY29udGV4dC5vcmlnaW5hbC5zdWJzdHJpbmcoc3RhcnQub2Zmc2V0LCBjb250ZXh0LnBvc2l0aW9uLm9mZnNldCksXG4gICAgICAgICAgY29udGVudDogY29udGV4dC5vcmlnaW5hbC5zdWJzdHJpbmcoc3RhcnQub2Zmc2V0ICsgMiwgY29udGV4dC5wb3NpdGlvbi5vZmZzZXQgLSAxKSxcbiAgICAgICAgfSk7XG4gICAgICB9IGVsc2UgaWYgKGNoYXIgPT0gJyAnIHx8IGNoYXIgPT0gJ1xcdCcgfHwgY2hhciA9PSAnXFxuJyB8fCBjaGFyID09ICdcXHInIHx8IGNoYXIgPT0gJ1xcZicpIHtcbiAgICAgICAgX25leHQoY29udGV4dCk7XG4gICAgICB9IGVsc2Uge1xuICAgICAgICBicmVhaztcbiAgICAgIH1cbiAgICB9XG5cbiAgICByZXR1cm4gY29tbWVudHM7XG4gIH0gZWxzZSB7XG4gICAgbGV0IGNoYXIgPSBjb250ZXh0Lm9yaWdpbmFsW2NvbnRleHQucG9zaXRpb24ub2Zmc2V0XTtcbiAgICB3aGlsZSAoY2hhciA9PSAnICcgfHwgY2hhciA9PSAnXFx0JyB8fCBjaGFyID09ICdcXG4nIHx8IGNoYXIgPT0gJ1xccicgfHwgY2hhciA9PSAnXFxmJykge1xuICAgICAgX25leHQoY29udGV4dCk7XG4gICAgICBjaGFyID0gY29udGV4dC5vcmlnaW5hbFtjb250ZXh0LnBvc2l0aW9uLm9mZnNldF07XG4gICAgfVxuXG4gICAgcmV0dXJuIFtdO1xuICB9XG59XG5cblxuLyoqXG4gKiBSZWFkIGEgSlNPTiB2YWx1ZSBmcm9tIHRoZSBjb250ZXh0LCB3aGljaCBjYW4gYmUgYW55IGZvcm0gb2YgSlNPTiB2YWx1ZS5cbiAqIEBwcml2YXRlXG4gKi9cbmZ1bmN0aW9uIF9yZWFkVmFsdWUoY29udGV4dDogSnNvblBhcnNlckNvbnRleHQsIGNvbW1lbnRzID0gX3JlYWRCbGFua3MoY29udGV4dCkpOiBKc29uQXN0Tm9kZSB7XG4gIGxldCByZXN1bHQ6IEpzb25Bc3ROb2RlO1xuXG4gIC8vIENsZWFuIHVwIGJlZm9yZS5cbiAgY29uc3QgY2hhciA9IF9wZWVrKGNvbnRleHQpO1xuICBzd2l0Y2ggKGNoYXIpIHtcbiAgICBjYXNlIHVuZGVmaW5lZDpcbiAgICAgIHRocm93IG5ldyBVbmV4cGVjdGVkRW5kT2ZJbnB1dEV4Y2VwdGlvbihjb250ZXh0KTtcblxuICAgIGNhc2UgJy0nOlxuICAgIGNhc2UgJzAnOlxuICAgIGNhc2UgJzEnOlxuICAgIGNhc2UgJzInOlxuICAgIGNhc2UgJzMnOlxuICAgIGNhc2UgJzQnOlxuICAgIGNhc2UgJzUnOlxuICAgIGNhc2UgJzYnOlxuICAgIGNhc2UgJzcnOlxuICAgIGNhc2UgJzgnOlxuICAgIGNhc2UgJzknOlxuICAgICAgcmVzdWx0ID0gX3JlYWROdW1iZXIoY29udGV4dCwgY29tbWVudHMpO1xuICAgICAgYnJlYWs7XG5cbiAgICBjYXNlICcuJzpcbiAgICBjYXNlICcrJzpcbiAgICAgIGlmICgoY29udGV4dC5tb2RlICYgSnNvblBhcnNlTW9kZS5MYXhOdW1iZXJQYXJzaW5nQWxsb3dlZCkgPT0gMCkge1xuICAgICAgICB0aHJvdyBuZXcgSW52YWxpZEpzb25DaGFyYWN0ZXJFeGNlcHRpb24oY29udGV4dCk7XG4gICAgICB9XG4gICAgICByZXN1bHQgPSBfcmVhZE51bWJlcihjb250ZXh0LCBjb21tZW50cyk7XG4gICAgICBicmVhaztcblxuICAgIGNhc2UgJ1xcJyc6XG4gICAgY2FzZSAnXCInOlxuICAgICAgcmVzdWx0ID0gX3JlYWRTdHJpbmcoY29udGV4dCwgY29tbWVudHMpO1xuICAgICAgYnJlYWs7XG5cbiAgICBjYXNlICdJJzpcbiAgICAgIGlmICgoY29udGV4dC5tb2RlICYgSnNvblBhcnNlTW9kZS5OdW1iZXJDb25zdGFudHNBbGxvd2VkKSA9PSAwKSB7XG4gICAgICAgIHRocm93IG5ldyBJbnZhbGlkSnNvbkNoYXJhY3RlckV4Y2VwdGlvbihjb250ZXh0KTtcbiAgICAgIH1cbiAgICAgIHJlc3VsdCA9IF9yZWFkTnVtYmVyKGNvbnRleHQsIGNvbW1lbnRzKTtcbiAgICAgIGJyZWFrO1xuXG4gICAgY2FzZSAnTic6XG4gICAgICBpZiAoKGNvbnRleHQubW9kZSAmIEpzb25QYXJzZU1vZGUuTnVtYmVyQ29uc3RhbnRzQWxsb3dlZCkgPT0gMCkge1xuICAgICAgICB0aHJvdyBuZXcgSW52YWxpZEpzb25DaGFyYWN0ZXJFeGNlcHRpb24oY29udGV4dCk7XG4gICAgICB9XG4gICAgICByZXN1bHQgPSBfcmVhZE5hTihjb250ZXh0LCBjb21tZW50cyk7XG4gICAgICBicmVhaztcblxuICAgIGNhc2UgJ3QnOlxuICAgICAgcmVzdWx0ID0gX3JlYWRUcnVlKGNvbnRleHQsIGNvbW1lbnRzKTtcbiAgICAgIGJyZWFrO1xuICAgIGNhc2UgJ2YnOlxuICAgICAgcmVzdWx0ID0gX3JlYWRGYWxzZShjb250ZXh0LCBjb21tZW50cyk7XG4gICAgICBicmVhaztcbiAgICBjYXNlICduJzpcbiAgICAgIHJlc3VsdCA9IF9yZWFkTnVsbChjb250ZXh0LCBjb21tZW50cyk7XG4gICAgICBicmVhaztcblxuICAgIGNhc2UgJ1snOlxuICAgICAgcmVzdWx0ID0gX3JlYWRBcnJheShjb250ZXh0LCBjb21tZW50cyk7XG4gICAgICBicmVhaztcblxuICAgIGNhc2UgJ3snOlxuICAgICAgcmVzdWx0ID0gX3JlYWRPYmplY3QoY29udGV4dCwgY29tbWVudHMpO1xuICAgICAgYnJlYWs7XG5cbiAgICBkZWZhdWx0OlxuICAgICAgdGhyb3cgbmV3IEludmFsaWRKc29uQ2hhcmFjdGVyRXhjZXB0aW9uKGNvbnRleHQpO1xuICB9XG5cbiAgLy8gQ2xlYW4gdXAgYWZ0ZXIuXG4gIF9yZWFkQmxhbmtzKGNvbnRleHQpO1xuXG4gIHJldHVybiByZXN1bHQ7XG59XG5cblxuLyoqXG4gKiBUaGUgUGFyc2UgbW9kZSB1c2VkIGZvciBwYXJzaW5nIHRoZSBKU09OIHN0cmluZy5cbiAqL1xuZXhwb3J0IGVudW0gSnNvblBhcnNlTW9kZSB7XG4gIFN0cmljdCAgICAgICAgICAgICAgICAgICAgPSAgICAgIDAsICAvLyBTdGFuZGFyZCBKU09OLlxuICBDb21tZW50c0FsbG93ZWQgICAgICAgICAgID0gMSA8PCAwLCAgLy8gQWxsb3dzIGNvbW1lbnRzLCBib3RoIHNpbmdsZSBvciBtdWx0aSBsaW5lcy5cbiAgU2luZ2xlUXVvdGVzQWxsb3dlZCAgICAgICA9IDEgPDwgMSwgIC8vIEFsbG93IHNpbmdsZSBxdW90ZWQgc3RyaW5ncy5cbiAgSWRlbnRpZmllcktleU5hbWVzQWxsb3dlZCA9IDEgPDwgMiwgIC8vIEFsbG93IGlkZW50aWZpZXJzIGFzIG9iamVjdHAgcHJvcGVydGllcy5cbiAgVHJhaWxpbmdDb21tYXNBbGxvd2VkICAgICA9IDEgPDwgMyxcbiAgSGV4YWRlY2ltYWxOdW1iZXJBbGxvd2VkICA9IDEgPDwgNCxcbiAgTXVsdGlMaW5lU3RyaW5nQWxsb3dlZCAgICA9IDEgPDwgNSxcbiAgTGF4TnVtYmVyUGFyc2luZ0FsbG93ZWQgICA9IDEgPDwgNiwgIC8vIEFsbG93IGAuYCBvciBgK2AgYXMgdGhlIGZpcnN0IGNoYXJhY3RlciBvZiBhIG51bWJlci5cbiAgTnVtYmVyQ29uc3RhbnRzQWxsb3dlZCAgICA9IDEgPDwgNywgIC8vIEFsbG93IC1JbmZpbml0eSwgSW5maW5pdHkgYW5kIE5hTi5cblxuICBEZWZhdWx0ICAgICAgICAgICAgICAgICAgID0gU3RyaWN0LFxuICBMb29zZSAgICAgICAgICAgICAgICAgICAgID0gQ29tbWVudHNBbGxvd2VkIHwgU2luZ2xlUXVvdGVzQWxsb3dlZCB8XG4gICAgICAgICAgICAgICAgICAgICAgICAgICAgICBJZGVudGlmaWVyS2V5TmFtZXNBbGxvd2VkIHwgVHJhaWxpbmdDb21tYXNBbGxvd2VkIHxcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIEhleGFkZWNpbWFsTnVtYmVyQWxsb3dlZCB8IE11bHRpTGluZVN0cmluZ0FsbG93ZWQgfFxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgTGF4TnVtYmVyUGFyc2luZ0FsbG93ZWQgfCBOdW1iZXJDb25zdGFudHNBbGxvd2VkLFxuXG4gIEpzb24gICAgICAgICAgICAgICAgICAgICAgPSBTdHJpY3QsXG4gIEpzb241ICAgICAgICAgICAgICAgICAgICAgPSBMb29zZSxcbn1cblxuXG4vKipcbiAqIFBhcnNlIHRoZSBKU09OIHN0cmluZyBhbmQgcmV0dXJuIGl0cyBBU1QuIFRoZSBBU1QgbWF5IGJlIGxvc2luZyBkYXRhIChlbmQgY29tbWVudHMgYXJlXG4gKiBkaXNjYXJkZWQgZm9yIGV4YW1wbGUsIGFuZCBzcGFjZSBjaGFyYWN0ZXJzIGFyZSBub3QgcmVwcmVzZW50ZWQgaW4gdGhlIEFTVCksIGJ1dCBhbGwgdmFsdWVzXG4gKiB3aWxsIGhhdmUgYSBzaW5nbGUgbm9kZSBpbiB0aGUgQVNUIChhIDEtdG8tMSBtYXBwaW5nKS5cbiAqIEBwYXJhbSBpbnB1dCBUaGUgc3RyaW5nIHRvIHVzZS5cbiAqIEBwYXJhbSBtb2RlIFRoZSBtb2RlIHRvIHBhcnNlIHRoZSBpbnB1dCB3aXRoLiB7QHNlZSBKc29uUGFyc2VNb2RlfS5cbiAqIEByZXR1cm5zIHtKc29uQXN0Tm9kZX0gVGhlIHJvb3Qgbm9kZSBvZiB0aGUgdmFsdWUgb2YgdGhlIEFTVC5cbiAqL1xuZXhwb3J0IGZ1bmN0aW9uIHBhcnNlSnNvbkFzdChpbnB1dDogc3RyaW5nLCBtb2RlID0gSnNvblBhcnNlTW9kZS5EZWZhdWx0KTogSnNvbkFzdE5vZGUge1xuICBpZiAobW9kZSA9PSBKc29uUGFyc2VNb2RlLkRlZmF1bHQpIHtcbiAgICBtb2RlID0gSnNvblBhcnNlTW9kZS5TdHJpY3Q7XG4gIH1cblxuICBjb25zdCBjb250ZXh0ID0ge1xuICAgIHBvc2l0aW9uOiB7IG9mZnNldDogMCwgbGluZTogMCwgY2hhcmFjdGVyOiAwIH0sXG4gICAgcHJldmlvdXM6IHsgb2Zmc2V0OiAwLCBsaW5lOiAwLCBjaGFyYWN0ZXI6IDAgfSxcbiAgICBvcmlnaW5hbDogaW5wdXQsXG4gICAgY29tbWVudHM6IHVuZGVmaW5lZCxcbiAgICBtb2RlLFxuICB9O1xuXG4gIGNvbnN0IGFzdCA9IF9yZWFkVmFsdWUoY29udGV4dCk7XG4gIGlmIChjb250ZXh0LnBvc2l0aW9uLm9mZnNldCA8IGlucHV0Lmxlbmd0aCkge1xuICAgIGNvbnN0IHJlc3QgPSBpbnB1dC5zdWJzdHIoY29udGV4dC5wb3NpdGlvbi5vZmZzZXQpO1xuICAgIGNvbnN0IGkgPSByZXN0Lmxlbmd0aCA+IDIwID8gcmVzdC5zdWJzdHIoMCwgMjApICsgJy4uLicgOiByZXN0O1xuICAgIHRocm93IG5ldyBFcnJvcihgRXhwZWN0ZWQgZW5kIG9mIGZpbGUsIGdvdCBcIiR7aX1cIiBhdCBgXG4gICAgICAgICsgYCR7Y29udGV4dC5wb3NpdGlvbi5saW5lfToke2NvbnRleHQucG9zaXRpb24uY2hhcmFjdGVyfS5gKTtcbiAgfVxuXG4gIHJldHVybiBhc3Q7XG59XG5cblxuLyoqXG4gKiBQYXJzZSBhIEpTT04gc3RyaW5nIGludG8gaXRzIHZhbHVlLiAgVGhpcyBkaXNjYXJkcyB0aGUgQVNUIGFuZCBvbmx5IHJldHVybnMgdGhlIHZhbHVlIGl0c2VsZi5cbiAqIEBwYXJhbSBpbnB1dCBUaGUgc3RyaW5nIHRvIHBhcnNlLlxuICogQHBhcmFtIG1vZGUgVGhlIG1vZGUgdG8gcGFyc2UgdGhlIGlucHV0IHdpdGguIHtAc2VlIEpzb25QYXJzZU1vZGV9LlxuICogQHJldHVybnMge0pzb25WYWx1ZX0gVGhlIHZhbHVlIHJlcHJlc2VudGVkIGJ5IHRoZSBKU09OIHN0cmluZy5cbiAqL1xuZXhwb3J0IGZ1bmN0aW9uIHBhcnNlSnNvbihpbnB1dDogc3RyaW5nLCBtb2RlID0gSnNvblBhcnNlTW9kZS5EZWZhdWx0KTogSnNvblZhbHVlIHtcbiAgLy8gVHJ5IHBhcnNpbmcgZm9yIHRoZSBmYXN0ZXN0IHBhdGggYXZhaWxhYmxlLCBpZiBlcnJvciwgdXNlcyBvdXIgb3duIHBhcnNlciBmb3IgYmV0dGVyIGVycm9ycy5cbiAgaWYgKG1vZGUgPT0gSnNvblBhcnNlTW9kZS5TdHJpY3QpIHtcbiAgICB0cnkge1xuICAgICAgcmV0dXJuIEpTT04ucGFyc2UoaW5wdXQpO1xuICAgIH0gY2F0Y2ggKGVycikge1xuICAgICAgcmV0dXJuIHBhcnNlSnNvbkFzdChpbnB1dCwgbW9kZSkudmFsdWU7XG4gICAgfVxuICB9XG5cbiAgcmV0dXJuIHBhcnNlSnNvbkFzdChpbnB1dCwgbW9kZSkudmFsdWU7XG59XG4iXX0=