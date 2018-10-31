"use strict";
/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
// tslint:disable:no-global-tslint-disable no-any
const core_1 = require("@angular-devkit/core");
const tools_1 = require("@angular-devkit/schematics/tools");
const fs_1 = require("fs");
const path_1 = require("path");
const rxjs_1 = require("rxjs");
const operators_1 = require("rxjs/operators");
const yargsParser = require("yargs-parser");
const command_1 = require("../models/command");
const find_up_1 = require("../utilities/find-up");
const project_1 = require("../utilities/project");
const json_schema_1 = require("./json-schema");
// Based off https://en.wikipedia.org/wiki/Levenshtein_distance
// No optimization, really.
function levenshtein(a, b) {
    /* base case: empty strings */
    if (a.length == 0) {
        return b.length;
    }
    if (b.length == 0) {
        return a.length;
    }
    // Test if last characters of the strings match.
    const cost = a[a.length - 1] == b[b.length - 1] ? 0 : 1;
    /* return minimum of delete char from s, delete char from t, and delete char from both */
    return Math.min(levenshtein(a.slice(0, -1), b) + 1, levenshtein(a, b.slice(0, -1)) + 1, levenshtein(a.slice(0, -1), b.slice(0, -1)) + cost);
}
/**
 * Run a command.
 * @param args Raw unparsed arguments.
 * @param logger The logger to use.
 * @param context Execution context.
 */
function runCommand(args, logger, context, commandMap) {
    return __awaiter(this, void 0, void 0, function* () {
        // if not args supplied, just run the help command.
        if (!args || args.length === 0) {
            args = ['help'];
        }
        const rawOptions = yargsParser(args, { alias: { help: ['h'] }, boolean: ['help'] });
        let commandName = rawOptions._[0] || '';
        // remove the command name
        rawOptions._ = rawOptions._.slice(1);
        const executionScope = project_1.insideProject()
            ? command_1.CommandScope.inProject
            : command_1.CommandScope.outsideProject;
        if (commandMap === undefined) {
            const commandMapPath = find_up_1.findUp('commands.json', __dirname);
            if (commandMapPath === null) {
                logger.fatal('Unable to find command map.');
                return 1;
            }
            const cliDir = path_1.dirname(commandMapPath);
            const commandsText = fs_1.readFileSync(commandMapPath).toString('utf-8');
            const commandJson = JSON.parse(commandsText);
            commandMap = {};
            for (const commandName of Object.keys(commandJson)) {
                commandMap[commandName] = path_1.join(cliDir, commandJson[commandName]);
            }
        }
        let commandMetadata = commandName ? findCommand(commandMap, commandName) : null;
        if (!commandMetadata && (rawOptions.v || rawOptions.version)) {
            commandName = 'version';
            commandMetadata = findCommand(commandMap, commandName);
        }
        else if (!commandMetadata && rawOptions.help) {
            commandName = 'help';
            commandMetadata = findCommand(commandMap, commandName);
        }
        if (!commandMetadata) {
            if (!commandName) {
                logger.error(core_1.tags.stripIndent `
        We could not find a command from the arguments and the help command seems to be disabled.
        This is an issue with the CLI itself. If you see this comment, please report it and
        provide your repository.
      `);
                return 1;
            }
            else {
                const commandsDistance = {};
                const allCommands = Object.keys(commandMap).sort((a, b) => {
                    if (!(a in commandsDistance)) {
                        commandsDistance[a] = levenshtein(a, commandName);
                    }
                    if (!(b in commandsDistance)) {
                        commandsDistance[b] = levenshtein(b, commandName);
                    }
                    return commandsDistance[a] - commandsDistance[b];
                });
                logger.error(core_1.tags.stripIndent `
          The specified command ("${commandName}") is invalid. For a list of available options,
          run "ng help".
          Did you mean "${allCommands[0]}"?
      `);
                return 1;
            }
        }
        const command = yield createCommand(commandMetadata, context, logger);
        const metadataOptions = yield json_schema_1.convertSchemaToOptions(commandMetadata.text);
        if (command === null) {
            logger.error(core_1.tags.stripIndent `Command (${commandName}) failed to instantiate.`);
            return 1;
        }
        // Add the options from the metadata to the command object.
        command.addOptions(metadataOptions);
        let options = parseOptions(args, metadataOptions);
        args = yield command.initializeRaw(args);
        const optionsCopy = core_1.deepCopy(options);
        yield processRegistry(optionsCopy, commandMetadata);
        yield command.initialize(optionsCopy);
        // Reparse options after initializing the command.
        options = parseOptions(args, command.options);
        if (commandName === 'help') {
            options.commandInfo = getAllCommandInfo(commandMap);
        }
        if (options.help) {
            command.printHelp(commandName, commandMetadata.rawData.description, options);
            return;
        }
        else {
            const commandScope = mapCommandScope(commandMetadata.rawData.$scope);
            if (commandScope !== undefined && commandScope !== command_1.CommandScope.everywhere) {
                if (commandScope !== executionScope) {
                    let errorMessage;
                    if (commandScope === command_1.CommandScope.inProject) {
                        errorMessage = `This command can only be run inside of a CLI project.`;
                    }
                    else {
                        errorMessage = `This command can not be run inside of a CLI project.`;
                    }
                    logger.fatal(errorMessage);
                    return 1;
                }
                if (commandScope === command_1.CommandScope.inProject) {
                    if (!context.project.configFile) {
                        logger.fatal('Invalid project: missing workspace file.');
                        return 1;
                    }
                    if (['.angular-cli.json', 'angular-cli.json'].includes(context.project.configFile)) {
                        // --------------------------------------------------------------------------------
                        // If changing this message, please update the same message in
                        // `packages/@angular/cli/bin/ng-update-message.js`
                        const message = core_1.tags.stripIndent `
            The Angular CLI configuration format has been changed, and your existing configuration
            can be updated automatically by running the following command:

              ng update @angular/cli
          `;
                        logger.warn(message);
                        return 1;
                    }
                }
            }
        }
        delete options.h;
        delete options.help;
        yield processRegistry(options, commandMetadata);
        const isValid = yield command.validate(options);
        if (!isValid) {
            logger.fatal(`Validation error. Invalid command options.`);
            return 1;
        }
        return command.run(options);
    });
}
exports.runCommand = runCommand;
function processRegistry(options, commandMetadata) {
    return __awaiter(this, void 0, void 0, function* () {
        const rawArgs = options._;
        const registry = new core_1.schema.CoreSchemaRegistry([]);
        registry.addSmartDefaultProvider('argv', (schema) => {
            if ('index' in schema) {
                return rawArgs[Number(schema['index'])];
            }
            else {
                return rawArgs;
            }
        });
        const jsonSchema = json_schema_1.parseSchema(commandMetadata.text);
        if (jsonSchema === null) {
            throw new Error('');
        }
        yield registry.compile(jsonSchema).pipe(operators_1.concatMap(validator => validator(options)), operators_1.concatMap(validatorResult => {
            if (validatorResult.success) {
                return rxjs_1.of(options);
            }
            else {
                return rxjs_1.throwError(new core_1.schema.SchemaValidationException(validatorResult.errors));
            }
        })).toPromise();
    });
}
function parseOptions(args, optionsAndArguments) {
    const parser = yargsParser;
    // filter out arguments
    const options = optionsAndArguments
        .filter(opt => {
        let isOption = true;
        if (opt.$default !== undefined && opt.$default.$source === 'argv') {
            isOption = false;
        }
        return isOption;
    });
    const aliases = options
        .reduce((aliases, opt) => {
        if (!opt || !opt.aliases || opt.aliases.length === 0) {
            return aliases;
        }
        aliases[opt.name] = (opt.aliases || [])
            .filter(a => a.length === 1)[0];
        return aliases;
    }, {});
    const booleans = options
        .filter(o => o.type && o.type === 'boolean')
        .map(o => o.name);
    const defaults = options
        .filter(o => o.default !== undefined || booleans.indexOf(o.name) !== -1)
        .reduce((defaults, opt) => {
        defaults[opt.name] = opt.default;
        return defaults;
    }, {});
    const strings = options
        .filter(o => o.type === 'string')
        .map(o => o.name);
    const numbers = options
        .filter(o => o.type === 'number')
        .map(o => o.name);
    aliases.help = ['h'];
    booleans.push('help');
    const yargsOptions = {
        alias: aliases,
        boolean: booleans,
        default: defaults,
        string: strings,
        number: numbers,
    };
    const parsedOptions = parser(args, yargsOptions);
    // Remove aliases.
    options
        .reduce((allAliases, option) => {
        if (!option || !option.aliases || option.aliases.length === 0) {
            return allAliases;
        }
        return allAliases.concat([...option.aliases]);
    }, [])
        .forEach((alias) => {
        delete parsedOptions[alias];
    });
    // Remove undefined booleans
    booleans
        .filter(b => parsedOptions[b] === undefined)
        .map(b => core_1.strings.camelize(b))
        .forEach(b => delete parsedOptions[b]);
    // remove options with dashes.
    Object.keys(parsedOptions)
        .filter(key => key.indexOf('-') !== -1)
        .forEach(key => delete parsedOptions[key]);
    // remove the command name
    parsedOptions._ = parsedOptions._.slice(1);
    return parsedOptions;
}
exports.parseOptions = parseOptions;
// Find a command.
function findCommand(map, name) {
    // let Cmd: CommandConstructor = map[name];
    let commandName = name;
    if (!map[commandName]) {
        // find command via aliases
        commandName = Object.keys(map)
            .filter(key => {
            // get aliases for the key
            const metadataText = fs_1.readFileSync(map[key]).toString('utf-8');
            const metadata = JSON.parse(metadataText);
            const aliases = metadata['$aliases'];
            if (!aliases) {
                return false;
            }
            const foundAlias = aliases.filter((alias) => alias === name);
            return foundAlias.length > 0;
        })[0];
    }
    const metadataPath = map[commandName];
    if (!metadataPath) {
        return null;
    }
    const metadataText = fs_1.readFileSync(metadataPath).toString('utf-8');
    const metadata = core_1.parseJson(metadataText);
    return {
        path: metadataPath,
        text: metadataText,
        rawData: metadata,
    };
}
// Create an instance of a command.
function createCommand(metadata, context, logger) {
    return __awaiter(this, void 0, void 0, function* () {
        const schema = json_schema_1.parseSchema(metadata.text);
        if (schema === null) {
            return null;
        }
        const implPath = schema.$impl;
        if (typeof implPath !== 'string') {
            throw new Error('Implementation path is incorrect');
        }
        const implRef = new tools_1.ExportStringRef(implPath, path_1.dirname(metadata.path));
        const ctor = implRef.ref;
        return new ctor(context, logger);
    });
}
function mapCommandScope(scope) {
    let commandScope = command_1.CommandScope.everywhere;
    switch (scope) {
        case 'in':
            commandScope = command_1.CommandScope.inProject;
            break;
        case 'out':
            commandScope = command_1.CommandScope.outsideProject;
            break;
    }
    return commandScope;
}
function getAllCommandInfo(map) {
    return Object.keys(map)
        .map(name => {
        return {
            name: name,
            metadata: findCommand(map, name),
        };
    })
        .map(info => {
        if (info.metadata === null) {
            return null;
        }
        return {
            name: info.name,
            description: info.metadata.rawData.description,
            aliases: info.metadata.rawData.$aliases || [],
            hidden: info.metadata.rawData.$hidden || false,
        };
    })
        .filter(info => info !== null);
}
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiY29tbWFuZC1ydW5uZXIuanMiLCJzb3VyY2VSb290IjoiLi8iLCJzb3VyY2VzIjpbInBhY2thZ2VzL2FuZ3VsYXIvY2xpL21vZGVscy9jb21tYW5kLXJ1bm5lci50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiO0FBQUE7Ozs7OztHQU1HOzs7Ozs7Ozs7O0FBRUgsaURBQWlEO0FBQ2pELCtDQVE4QjtBQUM5Qiw0REFBbUU7QUFDbkUsMkJBQWtDO0FBQ2xDLCtCQUFxQztBQUNyQywrQkFBc0M7QUFDdEMsOENBQTJDO0FBQzNDLDRDQUE0QztBQUM1QywrQ0FNMkI7QUFDM0Isa0RBQThDO0FBQzlDLGtEQUFxRDtBQUNyRCwrQ0FBb0U7QUFzQnBFLCtEQUErRDtBQUMvRCwyQkFBMkI7QUFDM0IscUJBQXFCLENBQVMsRUFBRSxDQUFTO0lBQ3ZDLDhCQUE4QjtJQUM5QixJQUFJLENBQUMsQ0FBQyxNQUFNLElBQUksQ0FBQyxFQUFFO1FBQ2pCLE9BQU8sQ0FBQyxDQUFDLE1BQU0sQ0FBQztLQUNqQjtJQUNELElBQUksQ0FBQyxDQUFDLE1BQU0sSUFBSSxDQUFDLEVBQUU7UUFDakIsT0FBTyxDQUFDLENBQUMsTUFBTSxDQUFDO0tBQ2pCO0lBRUQsZ0RBQWdEO0lBQ2hELE1BQU0sSUFBSSxHQUFHLENBQUMsQ0FBQyxDQUFDLENBQUMsTUFBTSxHQUFHLENBQUMsQ0FBQyxJQUFJLENBQUMsQ0FBQyxDQUFDLENBQUMsTUFBTSxHQUFHLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQyxDQUFDLENBQUMsQ0FBQztJQUV4RCx5RkFBeUY7SUFDekYsT0FBTyxJQUFJLENBQUMsR0FBRyxDQUNiLFdBQVcsQ0FBQyxDQUFDLENBQUMsS0FBSyxDQUFDLENBQUMsRUFBRSxDQUFDLENBQUMsQ0FBQyxFQUFFLENBQUMsQ0FBQyxHQUFHLENBQUMsRUFDbEMsV0FBVyxDQUFDLENBQUMsRUFBRSxDQUFDLENBQUMsS0FBSyxDQUFDLENBQUMsRUFBRSxDQUFDLENBQUMsQ0FB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QUMsUUFBUSxLQUFLLElBQUksRUFBRTtZQUMxQixPQUFPLElBQUksQ0FBQztTQUNiO1FBRUQsT0FBTztZQUNMLElBQUksRUFBRSxJQUFJLENBQUMsSUFBSTtZQUNmLFdBQVcsRUFBRSxJQUFJLENBQUMsUUFBUSxDQUFDLE9BQU8sQ0FBQyxXQUFXO1lBQzlDLE9BQU8sRUFBRSxJQUFJLENBQUMsUUFBUSxDQUFDLE9BQU8sQ0FBQyxRQUFRLElBQUksRUFBRTtZQUM3QyxNQUFNLEVBQUUsSUFBSSxDQUFDLFFBQVEsQ0FBQyxPQUFPLENBQUMsT0FBTyxJQUFJLEtBQUs7U0FDL0MsQ0FBQztJQUNKLENBQUMsQ0FBQztTQUNELE1BQU0sQ0FBQyxJQUFJLENBQUMsRUFBRSxDQUFDLElBQUksS0FBSyxJQUFJLENBQWtCLENBQUM7QUFDcEQsQ0FBQyIsInNvdXJjZXNDb250ZW50IjpbIi8qKlxuICogQGxpY2Vuc2VcbiAqIENvcHlyaWdodCBHb29nbGUgSW5jLiBBbGwgUmlnaHRzIFJlc2VydmVkLlxuICpcbiAqIFVzZSBvZiB0aGlzIHNvdXJjZSBjb2RlIGlzIGdvdmVybmVkIGJ5IGFuIE1JVC1zdHlsZSBsaWNlbnNlIHRoYXQgY2FuIGJlXG4gKiBmb3VuZCBpbiB0aGUgTElDRU5TRSBmaWxlIGF0IGh0dHBzOi8vYW5ndWxhci5pby9saWNlbnNlXG4gKi9cblxuLy8gdHNsaW50OmRpc2FibGU6bm8tZ2xvYmFsLXRzbGludC1kaXNhYmxlIG5vLWFueVxuaW1wb3J0IHtcbiAgSnNvbk9iamVjdCxcbiAgZGVlcENvcHksXG4gIGxvZ2dpbmcsXG4gIHBhcnNlSnNvbixcbiAgc2NoZW1hLFxuICBzdHJpbmdzIGFzIGNvcmVTdHJpbmdzLFxuICB0YWdzLFxufSBmcm9tICdAYW5ndWxhci1kZXZraXQvY29yZSc7XG5pbXBvcnQgeyBFeHBvcnRTdHJpbmdSZWYgfSBmcm9tICdAYW5ndWxhci1kZXZraXQvc2NoZW1hdGljcy90b29scyc7XG5pbXBvcnQgeyByZWFkRmlsZVN5bmMgfSBmcm9tICdmcyc7XG5pbXBvcnQgeyBkaXJuYW1lLCBqb2luIH0gZnJvbSAncGF0aCc7XG5pbXBvcnQgeyBvZiwgdGhyb3dFcnJvciB9IGZyb20gJ3J4anMnO1xuaW1wb3J0IHsgY29uY2F0TWFwIH0gZnJvbSAncnhqcy9vcGVyYXRvcnMnO1xuaW1wb3J0ICogYXMgeWFyZ3NQYXJzZXIgZnJvbSAneWFyZ3MtcGFyc2VyJztcbmltcG9ydCB7XG4gIENvbW1hbmQsXG4gIENvbW1hbmRDb25zdHJ1Y3RvcixcbiAgQ29tbWFuZENvbnRleHQsXG4gIENvbW1hbmRTY29wZSxcbiAgT3B0aW9uLFxufSBmcm9tICcuLi9tb2RlbHMvY29tbWFuZCc7XG5pbXBvcnQgeyBmaW5kVXAgfSBmcm9tICcuLi91dGlsaXRpZXMvZmluZC11cCc7XG5pbXBvcnQgeyBpbnNpZGVQcm9qZWN0IH0gZnJvbSAnLi4vdXRpbGl0aWVzL3Byb2plY3QnO1xuaW1wb3J0IHsgY29udmVydFNjaGVtYVRvT3B0aW9ucywgcGFyc2VTY2hlbWEgfSBmcm9tICcuL2pzb24tc2NoZW1hJztcblxuXG5pbnRlcmZhY2UgQ29tbWFuZE1hcCB7XG4gIFtrZXk6IHN0cmluZ106IHN0cmluZztcbn1cblxuaW50ZXJmYWNlIENvbW1hbmRNZXRhZGF0YSB7XG4gIGRlc2NyaXB0aW9uOiBzdHJpbmc7XG4gICRhbGlhc2VzPzogc3RyaW5nW107XG4gICRpbXBsOiBzdHJpbmc7XG4gICRzY29wZT86ICdpbicgfCAnb3V0JztcbiAgJHR5cGU/OiAnYXJjaGl0ZWN0JyB8ICdzY2hlbWF0aWMnO1xuICAkaGlkZGVuPzogYm9vbGVhbjtcbn1cblxuaW50ZXJmYWNlIENvbW1hbmRMb2NhdGlvbiB7XG4gIHBhdGg6IHN0cmluZztcbiAgdGV4dDogc3RyaW5nO1xuICByYXdEYXRhOiBDb21tYW5kTWV0YWRhdGE7XG59XG5cbi8vIEJhc2VkIG9mZiBodHRwczovL2VuLndpa2lwZWRpYS5vcmcvd2lraS9MZXZlbnNodGVpbl9kaXN0YW5jZVxuLy8gTm8gb3B0aW1pemF0aW9uLCByZWFsbHkuXG5mdW5jdGlvbiBsZXZlbnNodGVpbihhOiBzdHJpbmcsIGI6IHN0cmluZyk6IG51bWJlciB7XG4gIC8qIGJhc2UgY2FzZTogZW1wdHkgc3RyaW5ncyAqL1xuICBpZiAoYS5sZW5ndGggPT0gMCkge1xuICAgIHJldHVybiBiLmxlbmd0aDtcbiAgfVxuICBpZiAoYi5sZW5ndGggPT0gMCkge1xuICAgIHJldHVybiBhLmxlbmd0aDtcbiAgfVxuXG4gIC8vIFRlc3QgaWYgbGFzdCBjaGFyYWN0ZXJzIG9mIHRoZSBzdHJpbmdzIG1hdGNoLlxuICBjb25zdCBjb3N0ID0gYVthLmxlbmd0aCAtIDFdID09IGJbYi5sZW5ndGggLSAxXSA/IDAgOiAxO1xuXG4gIC8qIHJldHVybiBtaW5pbXVtIG9mIGRlbGV0ZSBjaGFyIGZyb20gcywgZGVsZXRlIGNoYXIgZnJvbSB0LCBhbmQgZGVsZXRlIGNoYXIgZnJvbSBib3RoICovXG4gIHJldHVybiBNYXRoLm1pbihcbiAgICBsZXZlbnNodGVpbihhLnNsaWNlKDAsIC0xKSwgYikgKyAxLFxuICAgIGxldmVuc2h0ZWluKGEsIGIuc2xpY2UoMCwgLTEpKSArIDEsXG4gICAgbGV2ZW5zaHRlaW4oYS5zbGljZSgwLCAtMSksIGIuc2xpY2UoMCwgLTEpKSArIGNvc3QsXG4gICk7XG59XG5cbi8qKlxuICogUnVuIGEgY29tbWFuZC5cbiAqIEBwYXJhbSBhcmdzIFJhdyB1bnBhcnNlZCBhcmd1bWVudHMuXG4gKiBAcGFyYW0gbG9nZ2VyIFRoZSBsb2dnZXIgdG8gdXNlLlxuICogQHBhcmFtIGNvbnRleHQgRXhlY3V0aW9uIGNvbnRleHQuXG4gKi9cbmV4cG9ydCBhc3luYyBmdW5jdGlvbiBydW5Db21tYW5kKFxuICBhcmdzOiBzdHJpbmdbXSxcbiAgbG9nZ2VyOiBsb2dnaW5nLkxvZ2dlcixcbiAgY29udGV4dDogQ29tbWFuZENvbnRleHQsXG4gIGNvbW1hbmRNYXA/OiBDb21tYW5kTWFwLFxuKTogUHJvbWlzZTxudW1iZXIgfCB2b2lkPiB7XG5cbiAgLy8gaWYgbm90IGFyZ3Mgc3VwcGxpZWQsIGp1c3QgcnVuIHRoZSBoZWxwIGNvbW1hbmQuXG4gIGlmICghYXJncyB8fCBhcmdzLmxlbmd0aCA9PT0gMCkge1xuICAgIGFyZ3MgPSBbJ2hlbHAnXTtcbiAgfVxuICBjb25zdCByYXdPcHRpb25zID0geWFyZ3NQYXJzZXIoYXJncywgeyBhbGlhczogeyBoZWxwOiBbJ2gnXSB9LCBib29sZWFuOiBbICdoZWxwJyBdIH0pO1xuICBsZXQgY29tbWFuZE5hbWUgPSByYXdPcHRpb25zLl9bMF0gfHwgJyc7XG5cbiAgLy8gcmVtb3ZlIHRoZSBjb21tYW5kIG5hbWVcbiAgcmF3T3B0aW9ucy5fID0gcmF3T3B0aW9ucy5fLnNsaWNlKDEpO1xuICBjb25zdCBleGVjdXRpb25TY29wZSA9IGluc2lkZVByb2plY3QoKVxuICAgID8gQ29tbWFuZFNjb3BlLmluUHJvamVjdFxuICAgIDogQ29tbWFuZFNjb3BlLm91dHNpZGVQcm9qZWN0O1xuXG4gIGlmIChjb21tYW5kTWFwID09PSB1bmRlZmluZWQpIHtcbiAgICBjb25zdCBjb21tYW5kTWFwUGF0aCA9IGZpbmRVcCgnY29tbWFuZHMuanNvbicsIF9fZGlybmFtZSk7XG4gICAgaWYgKGNvbW1hbmRNYXBQYXRoID09PSBudWxsKSB7XG4gICAgICBsb2dnZXIuZmF0YWwoJ1VuYWJsZSB0byBmaW5kIGNvbW1hbmQgbWFwLicpO1xuXG4gICAgICByZXR1cm4gMTtcbiAgICB9XG4gICAgY29uc3QgY2xpRGlyID0gZGlybmFtZShjb21tYW5kTWFwUGF0aCk7XG4gICAgY29uc3QgY29tbWFuZHNUZXh0ID0gcmVhZEZpbGVTeW5jKGNvbW1hbmRNYXBQYXRoKS50b1N0cmluZygndXRmLTgnKTtcbiAgICBjb25zdCBjb21tYW5kSnNvbiA9IEpTT04ucGFyc2UoY29tbWFuZHNUZXh0KSBhcyB7IFtuYW1lOiBzdHJpbmddOiBzdHJpbmcgfTtcblxuICAgIGNvbW1hbmRNYXAgPSB7fTtcbiAgICBmb3IgKGNvbnN0IGNvbW1hbmROYW1lIG9mIE9iamVjdC5rZXlzKGNvbW1hbmRKc29uKSkge1xuICAgICAgY29tbWFuZE1hcFtjb21tYW5kTmFtZV0gPSBqb2luKGNsaURpciwgY29tbWFuZEpzb25bY29tbWFuZE5hbWVdKTtcbiAgICB9XG4gIH1cblxuICBsZXQgY29tbWFuZE1ldGFkYXRhID0gY29tbWFuZE5hbWUgPyBmaW5kQ29tbWFuZChjb21tYW5kTWFwLCBjb21tYW5kTmFtZSkgOiBudWxsO1xuXG4gIGlmICghY29tbWFuZE1ldGFkYXRhICYmIChyYXdPcHRpb25zLnYgfHwgcmF3T3B0aW9ucy52ZXJzaW9uKSkge1xuICAgIGNvbW1hbmROYW1lID0gJ3ZlcnNpb24nO1xuICAgIGNvbW1hbmRNZXRhZGF0YSA9IGZpbmRDb21tYW5kKGNvbW1hbmRNYXAsIGNvbW1hbmROYW1lKTtcbiAgfSBlbHNlIGlmICghY29tbWFuZE1ldGFkYXRhICYmIHJhd09wdGlvbnMuaGVscCkge1xuICAgIGNvbW1hbmROYW1lID0gJ2hlbHAnO1xuICAgIGNvbW1hbmRNZXRhZGF0YSA9IGZpbmRDb21tYW5kKGNvbW1hbmRNYXAsIGNvbW1hbmROYW1lKTtcbiAgfVxuXG4gIGlmICghY29tbWFuZE1ldGFkYXRhKSB7XG4gICAgaWYgKCFjb21tYW5kTmFtZSkge1xuICAgICAgbG9nZ2VyLmVycm9yKHRhZ3Muc3RyaXBJbmRlbnRgXG4gICAgICAgIFdlIGNvdWxkIG5vdCBmaW5kIGEgY29tbWFuZCBmcm9tIHRoZSBhcmd1bWVudHMgYW5kIHRoZSBoZWxwIGNvbW1hbmQgc2VlbXMgdG8gYmUgZGlzYWJsZWQuXG4gICAgICAgIFRoaXMgaXMgYW4gaXNzdWUgd2l0aCB0aGUgQ0xJIGl0c2VsZi4gSWYgeW91IHNlZSB0aGlzIGNvbW1lbnQsIHBsZWFzZSByZXBvcnQgaXQgYW5kXG4gICAgICAgIHByb3ZpZGUgeW91ciByZXBvc2l0b3J5LlxuICAgICAgYCk7XG5cbiAgICAgIHJldHVybiAxO1xuICAgIH0gZWxzZSB7XG4gICAgICBjb25zdCBjb21tYW5kc0Rpc3RhbmNlID0ge30gYXMgeyBbbmFtZTogc3RyaW5nXTogbnVtYmVyIH07XG4gICAgICBjb25zdCBhbGxDb21tYW5kcyA9IE9iamVjdC5rZXlzKGNvbW1hbmRNYXApLnNvcnQoKGEsIGIpID0+IHtcbiAgICAgICAgaWYgKCEoYSBpbiBjb21tYW5kc0Rpc3RhbmNlKSkge1xuICAgICAgICAgIGNvbW1hbmRzRGlzdGFuY2VbYV0gPSBsZXZlbnNodGVpbihhLCBjb21tYW5kTmFtZSk7XG4gICAgICAgIH1cbiAgICAgICAgaWYgKCEoYiBpbiBjb21tYW5kc0Rpc3RhbmNlKSkge1xuICAgICAgICAgIGNvbW1hbmRzRGlzdGFuY2VbYl0gPSBsZXZlbnNodGVpbihiLCBjb21tYW5kTmFtZSk7XG4gICAgICAgIH1cblxuICAgICAgICByZXR1cm4gY29tbWFuZHNEaXN0YW5jZVthXSAtIGNvbW1hbmRzRGlzdGFuY2VbYl07XG4gICAgICB9KTtcblxuICAgICAgbG9nZ2VyLmVycm9yKHRhZ3Muc3RyaXBJbmRlbnRgXG4gICAgICAgICAgVGhlIHNwZWNpZmllZCBjb21tYW5kIChcIiR7Y29tbWFuZE5hbWV9XCIpIGlzIGludmFsaWQuIEZvciBhIGxpc3Qgb2YgYXZhaWxhYmxlIG9wdGlvbnMsXG4gICAgICAgICAgcnVuIFwibmcgaGVscFwiLlxuICAgICAgICAgIERpZCB5b3UgbWVhbiBcIiR7YWxsQ29tbWFuZHNbMF19XCI/XG4gICAgICBgKTtcblxuICAgICAgcmV0dXJuIDE7XG4gICAgfVxuICB9XG5cbiAgY29uc3QgY29tbWFuZCA9IGF3YWl0IGNyZWF0ZUNvbW1hbmQoY29tbWFuZE1ldGFkYXRhLCBjb250ZXh0LCBsb2dnZXIpO1xuICBjb25zdCBtZXRhZGF0YU9wdGlvbnMgPSBhd2FpdCBjb252ZXJ0U2NoZW1hVG9PcHRpb25zKGNvbW1hbmRNZXRhZGF0YS50ZXh0KTtcbiAgaWYgKGNvbW1hbmQgPT09IG51bGwpIHtcbiAgICBsb2dnZXIuZXJyb3IodGFncy5zdHJpcEluZGVudGBDb21tYW5kICgke2NvbW1hbmROYW1lfSkgZmFpbGVkIHRvIGluc3RhbnRpYXRlLmApO1xuXG4gICAgcmV0dXJuIDE7XG4gIH1cbiAgLy8gQWRkIHRoZSBvcHRpb25zIGZyb20gdGhlIG1ldGFkYXRhIHRvIHRoZSBjb21tYW5kIG9iamVjdC5cbiAgY29tbWFuZC5hZGRPcHRpb25zKG1ldGFkYXRhT3B0aW9ucyk7XG4gIGxldCBvcHRpb25zID0gcGFyc2VPcHRpb25zKGFyZ3MsIG1ldGFkYXRhT3B0aW9ucyk7XG4gIGFyZ3MgPSBhd2FpdCBjb21tYW5kLmluaXRpYWxpemVSYXcoYXJncyk7XG5cbiAgY29uc3Qgb3B0aW9uc0NvcHkgPSBkZWVwQ29weShvcHRpb25zKTtcbiAgYXdhaXQgcHJvY2Vzc1JlZ2lzdHJ5KG9wdGlvbnNDb3B5LCBjb21tYW5kTWV0YWRhdGEpO1xuICBhd2FpdCBjb21tYW5kLmluaXRpYWxpemUob3B0aW9uc0NvcHkpO1xuXG4gIC8vIFJlcGFyc2Ugb3B0aW9ucyBhZnRlciBpbml0aWFsaXppbmcgdGhlIGNvbW1hbmQuXG4gIG9wdGlvbnMgPSBwYXJzZU9wdGlvbnMoYXJncywgY29tbWFuZC5vcHRpb25zKTtcblxuICBpZiAoY29tbWFuZE5hbWUgPT09ICdoZWxwJykge1xuICAgIG9wdGlvbnMuY29tbWFuZEluZm8gPSBnZXRBbGxDb21tYW5kSW5mbyhjb21tYW5kTWFwKTtcbiAgfVxuXG4gIGlmIChvcHRpb25zLmhlbHApIHtcbiAgICBjb21tYW5kLnByaW50SGVscChjb21tYW5kTmFtZSwgY29tbWFuZE1ldGFkYXRhLnJhd0RhdGEuZGVzY3JpcHRpb24sIG9wdGlvbnMpO1xuXG4gICAgcmV0dXJuO1xuICB9IGVsc2Uge1xuICAgIGNvbnN0IGNvbW1hbmRTY29wZSA9IG1hcENvbW1hbmRTY29wZShjb21tYW5kTWV0YWRhdGEucmF3RGF0YS4kc2NvcGUpO1xuICAgIGlmIChjb21tYW5kU2NvcGUgIT09IHVuZGVmaW5lZCAmJiBjb21tYW5kU2NvcGUgIT09IENvbW1hbmRTY29wZS5ldmVyeXdoZXJlKSB7XG4gICAgICBpZiAoY29tbWFuZFNjb3BlICE9PSBleGVjdXRpb25TY29wZSkge1xuICAgICAgICBsZXQgZXJyb3JNZXNzYWdlO1xuICAgICAgICBpZiAoY29tbWFuZFNjb3BlID09PSBDb21tYW5kU2NvcGUuaW5Qcm9qZWN0KSB7XG4gICAgICAgICAgZXJyb3JNZXNzYWdlID0gYFRoaXMgY29tbWFuZCBjYW4gb25seSBiZSBydW4gaW5zaWRlIG9mIGEgQ0xJIHByb2plY3QuYDtcbiAgICAgICAgfSBlbHNlIHtcbiAgICAgICAgICBlcnJvck1lc3NhZ2UgPSBgVGhpcyBjb21tYW5kIGNhbiBub3QgYmUgcnVuIGluc2lkZSBvZiBhIENMSSBwcm9qZWN0LmA7XG4gICAgICAgIH1cbiAgICAgICAgbG9nZ2VyLmZhdGFsKGVycm9yTWVzc2FnZSk7XG5cbiAgICAgICAgcmV0dXJuIDE7XG4gICAgICB9XG4gICAgICBpZiAoY29tbWFuZFNjb3BlID09PSBDb21tYW5kU2NvcGUuaW5Qcm9qZWN0KSB7XG4gICAgICAgIGlmICghY29udGV4dC5wcm9qZWN0LmNvbmZpZ0ZpbGUpIHtcbiAgICAgICAgICBsb2dnZXIuZmF0YWwoJ0ludmFsaWQgcHJvamVjdDogbWlzc2luZyB3b3Jrc3BhY2UgZmlsZS4nKTtcblxuICAgICAgICAgIHJldHVybiAxO1xuICAgICAgICB9XG5cbiAgICAgICAgaWYgKFsnLmFuZ3VsYXItY2xpLmpzb24nLCAnYW5ndWxhci1jbGkuanNvbiddLmluY2x1ZGVzKGNvbnRleHQucHJvamVjdC5jb25maWdGaWxlKSkge1xuICAgICAgICAgIC8vIC0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tLS0tXG4gICAgICAgICAgLy8gSWYgY2hhbmdpbmcgdGhpcyBtZXNzYWdlLCBwbGVhc2UgdXBkYXRlIHRoZSBzYW1lIG1lc3NhZ2UgaW5cbiAgICAgICAgICAvLyBgcGFja2FnZXMvQGFuZ3VsYXIvY2xpL2Jpbi9uZy11cGRhdGUtbWVzc2FnZS5qc2BcbiAgICAgICAgICBjb25zdCBtZXNzYWdlID0gdGFncy5zdHJpcEluZGVudGBcbiAgICAgICAgICAgIFRoZSBBbmd1bGFyIENMSSBjb25maWd1cmF0aW9uIGZvcm1hdCBoYXMgYmVlbiBjaGFuZ2VkLCBhbmQgeW91ciBleGlzdGluZyBjb25maWd1cmF0aW9uXG4gICAgICAgICAgICBjYW4gYmUgdXBkYXRlZCBhdXRvbWF0aWNhbGx5IGJ5IHJ1bm5pbmcgdGhlIGZvbGxvd2luZyBjb21tYW5kOlxuXG4gICAgICAgICAgICAgIG5nIHVwZGF0ZSBAYW5ndWxhci9jbGlcbiAgICAgICAgICBgO1xuXG4gICAgICAgICAgbG9nZ2VyLndhcm4obWVzc2FnZSk7XG5cbiAgICAgICAgICByZXR1cm4gMTtcbiAgICAgICAgfVxuICAgICAgfVxuICAgIH1cbiAgfVxuICBkZWxldGUgb3B0aW9ucy5oO1xuICBkZWxldGUgb3B0aW9ucy5oZWxwO1xuICBhd2FpdCBwcm9jZXNzUmVnaXN0cnkob3B0aW9ucywgY29tbWFuZE1ldGFkYXRhKTtcblxuICBjb25zdCBpc1ZhbGlkID0gYXdhaXQgY29tbWFuZC52YWxpZGF0ZShvcHRpb25zKTtcbiAgaWYgKCFpc1ZhbGlkKSB7XG4gICAgbG9nZ2VyLmZhdGFsKGBWYWxpZGF0aW9uIGVycm9yLiBJbnZhbGlkIGNvbW1hbmQgb3B0aW9ucy5gKTtcblxuICAgIHJldHVybiAxO1xuICB9XG5cbiAgcmV0dXJuIGNvbW1hbmQucnVuKG9wdGlvbnMpO1xufVxuXG5hc3luYyBmdW5jdGlvbiBwcm9jZXNzUmVnaXN0cnkoXG4gIG9wdGlvbnM6IHtfOiAoc3RyaW5nIHwgYm9vbGVhbiB8IG51bWJlcilbXX0sIGNvbW1hbmRNZXRhZGF0YTogQ29tbWFuZExvY2F0aW9uKSB7XG4gIGNvbnN0IHJhd0FyZ3MgPSBvcHRpb25zLl87XG4gIGNvbnN0IHJlZ2lzdHJ5ID0gbmV3IHNjaGVtYS5Db3JlU2NoZW1hUmVnaXN0cnkoW10pO1xuICByZWdpc3RyeS5hZGRTbWFydERlZmF1bHRQcm92aWRlcignYXJndicsIChzY2hlbWE6IEpzb25PYmplY3QpID0+IHtcbiAgICBpZiAoJ2luZGV4JyBpbiBzY2hlbWEpIHtcbiAgICAgIHJldHVybiByYXdBcmdzW051bWJlcihzY2hlbWFbJ2luZGV4J10pXTtcbiAgICB9IGVsc2Uge1xuICAgICAgcmV0dXJuIHJhd0FyZ3M7XG4gICAgfVxuICB9KTtcblxuICBjb25zdCBqc29uU2NoZW1hID0gcGFyc2VTY2hlbWEoY29tbWFuZE1ldGFkYXRhLnRleHQpO1xuICBpZiAoanNvblNjaGVtYSA9PT0gbnVsbCkge1xuICAgIHRocm93IG5ldyBFcnJvcignJyk7XG4gIH1cbiAgYXdhaXQgcmVnaXN0cnkuY29tcGlsZShqc29uU2NoZW1hKS5waXBlKFxuICAgIGNvbmNhdE1hcCh2YWxpZGF0b3IgPT4gdmFsaWRhdG9yKG9wdGlvbnMpKSwgY29uY2F0TWFwKHZhbGlkYXRvclJlc3VsdCA9PiB7XG4gICAgICBpZiAodmFsaWRhdG9yUmVzdWx0LnN1Y2Nlc3MpIHtcbiAgICAgICAgcmV0dXJuIG9mKG9wdGlvbnMpO1xuICAgICAgfSBlbHNlIHtcbiAgICAgICAgcmV0dXJuIHRocm93RXJyb3IobmV3IHNjaGVtYS5TY2hlbWFWYWxpZGF0aW9uRXhjZXB0aW9uKHZhbGlkYXRvclJlc3VsdC5lcnJvcnMpKTtcbiAgICAgIH1cbiAgICB9KSkudG9Qcm9taXNlKCk7XG59XG5cbmV4cG9ydCBmdW5jdGlvbiBwYXJzZU9wdGlvbnMoYXJnczogc3RyaW5nW10sIG9wdGlvbnNBbmRBcmd1bWVudHM6IE9wdGlvbltdKSB7XG4gIGNvbnN0IHBhcnNlciA9IHlhcmdzUGFyc2VyO1xuXG4gIC8vIGZpbHRlciBvdXQgYXJndW1lbnRzXG4gIGNvbnN0IG9wdGlvbnMgPSBvcHRpb25zQW5kQXJndW1lbnRzXG4gICAgLmZpbHRlcihvcHQgPT4ge1xuICAgICAgbGV0IGlzT3B0aW9uID0gdHJ1ZTtcbiAgICAgIGlmIChvcHQuJGRlZmF1bHQgIT09IHVuZGVmaW5lZCAmJiBvcHQuJGRlZmF1bHQuJHNvdXJjZSA9PT0gJ2FyZ3YnKSB7XG4gICAgICAgIGlzT3B0aW9uID0gZmFsc2U7XG4gICAgICB9XG5cbiAgICAgIHJldHVybiBpc09wdGlvbjtcbiAgICB9KTtcblxuICBjb25zdCBhbGlhc2VzOiB7IFtrZXk6IHN0cmluZ106IHN0cmluZ1tdOyB9ID0gb3B0aW9uc1xuICAgIC5yZWR1Y2UoKGFsaWFzZXM6IHsgW2tleTogc3RyaW5nXTogc3RyaW5nOyB9LCBvcHQpID0+IHtcbiAgICAgIGlmICghb3B0IHx8ICFvcHQuYWxpYXNlcyB8fCBvcHQuYWxpYXNlcy5sZW5ndGggPT09IDApIHtcbiAgICAgICAgcmV0dXJuIGFsaWFzZXM7XG4gICAgICB9XG5cbiAgICAgIGFsaWFzZXNbb3B0Lm5hbWVdID0gKG9wdC5hbGlhc2VzIHx8IFtdKVxuICAgICAgICAuZmlsdGVyKGEgPT4gYS5sZW5ndGggPT09IDEpWzBdO1xuXG4gICAgICByZXR1cm4gYWxpYXNlcztcbiAgICB9LCB7fSk7XG5cbiAgY29uc3QgYm9vbGVhbnMgPSBvcHRpb25zXG4gICAgLmZpbHRlcihvID0+IG8udHlwZSAmJiBvLnR5cGUgPT09ICdib29sZWFuJylcbiAgICAubWFwKG8gPT4gby5uYW1lKTtcblxuICBjb25zdCBkZWZhdWx0cyA9IG9wdGlvbnNcbiAgICAuZmlsdGVyKG8gPT4gby5kZWZhdWx0ICE9PSB1bmRlZmluZWQgfHwgYm9vbGVhbnMuaW5kZXhPZihvLm5hbWUpICE9PSAtMSlcbiAgICAucmVkdWNlKChkZWZhdWx0czoge1trZXk6IHN0cmluZ106IHN0cmluZyB8IG51bWJlciB8IGJvb2xlYW4gfCB1bmRlZmluZWQgfSwgb3B0OiBPcHRpb24pID0+IHtcbiAgICAgIGRlZmF1bHRzW29wdC5uYW1lXSA9IG9wdC5kZWZhdWx0O1xuXG4gICAgICByZXR1cm4gZGVmYXVsdHM7XG4gICAgfSwge30pO1xuXG4gIGNvbnN0IHN0cmluZ3MgPSBvcHRpb25zXG4gICAgLmZpbHRlcihvID0+IG8udHlwZSA9PT0gJ3N0cmluZycpXG4gICAgLm1hcChvID0+IG8ubmFtZSk7XG5cbiAgY29uc3QgbnVtYmVycyA9IG9wdGlvbnNcbiAgICAuZmlsdGVyKG8gPT4gby50eXBlID09PSAnbnVtYmVyJylcbiAgICAubWFwKG8gPT4gby5uYW1lKTtcblxuXG4gIGFsaWFzZXMuaGVscCA9IFsnaCddO1xuICBib29sZWFucy5wdXNoKCdoZWxwJyk7XG5cbiAgY29uc3QgeWFyZ3NPcHRpb25zID0ge1xuICAgIGFsaWFzOiBhbGlhc2VzLFxuICAgIGJvb2xlYW46IGJvb2xlYW5zLFxuICAgIGRlZmF1bHQ6IGRlZmF1bHRzLFxuICAgIHN0cmluZzogc3RyaW5ncyxcbiAgICBudW1iZXI6IG51bWJlcnMsXG4gIH07XG5cbiAgY29uc3QgcGFyc2VkT3B0aW9ucyA9IHBhcnNlcihhcmdzLCB5YXJnc09wdGlvbnMpO1xuXG4gIC8vIFJlbW92ZSBhbGlhc2VzLlxuICBvcHRpb25zXG4gICAgLnJlZHVjZSgoYWxsQWxpYXNlcywgb3B0aW9uKSA9PiB7XG4gICAgICBpZiAoIW9wdGlvbiB8fCAhb3B0aW9uLmFsaWFzZXMgfHwgb3B0aW9uLmFsaWFzZXMubGVuZ3RoID09PSAwKSB7XG4gICAgICAgIHJldHVybiBhbGxBbGlhc2VzO1xuICAgICAgfVxuXG4gICAgICByZXR1cm4gYWxsQWxpYXNlcy5jb25jYXQoWy4uLm9wdGlvbi5hbGlhc2VzXSk7XG4gICAgfSwgW10gYXMgc3RyaW5nW10pXG4gICAgLmZvckVhY2goKGFsaWFzOiBzdHJpbmcpID0+IHtcbiAgICAgIGRlbGV0ZSBwYXJzZWRPcHRpb25zW2FsaWFzXTtcbiAgICB9KTtcblxuICAvLyBSZW1vdmUgdW5kZWZpbmVkIGJvb2xlYW5zXG4gIGJvb2xlYW5zXG4gICAgLmZpbHRlcihiID0+IHBhcnNlZE9wdGlvbnNbYl0gPT09IHVuZGVmaW5lZClcbiAgICAubWFwKGIgPT4gY29yZVN0cmluZ3MuY2FtZWxpemUoYikpXG4gICAgLmZvckVhY2goYiA9PiBkZWxldGUgcGFyc2VkT3B0aW9uc1tiXSk7XG5cbiAgLy8gcmVtb3ZlIG9wdGlvbnMgd2l0aCBkYXNoZXMuXG4gIE9iamVjdC5rZXlzKHBhcnNlZE9wdGlvbnMpXG4gICAgLmZpbHRlcihrZXkgPT4ga2V5LmluZGV4T2YoJy0nKSAhPT0gLTEpXG4gICAgLmZvckVhY2goa2V5ID0+IGRlbGV0ZSBwYXJzZWRPcHRpb25zW2tleV0pO1xuXG4gIC8vIHJlbW92ZSB0aGUgY29tbWFuZCBuYW1lXG4gIHBhcnNlZE9wdGlvbnMuXyA9IHBhcnNlZE9wdGlvbnMuXy5zbGljZSgxKTtcblxuICByZXR1cm4gcGFyc2VkT3B0aW9ucztcbn1cblxuLy8gRmluZCBhIGNvbW1hbmQuXG5mdW5jdGlvbiBmaW5kQ29tbWFuZChtYXA6IENvbW1hbmRNYXAsIG5hbWU6IHN0cmluZyk6IENvbW1hbmRMb2NhdGlvbiB8IG51bGwge1xuICAvLyBsZXQgQ21kOiBDb21tYW5kQ29uc3RydWN0b3IgPSBtYXBbbmFtZV07XG4gIGxldCBjb21tYW5kTmFtZSA9IG5hbWU7XG5cbiAgaWYgKCFtYXBbY29tbWFuZE5hbWVdKSB7XG4gICAgLy8gZmluZCBjb21tYW5kIHZpYSBhbGlhc2VzXG4gICAgY29tbWFuZE5hbWUgPSBPYmplY3Qua2V5cyhtYXApXG4gICAgICAuZmlsdGVyKGtleSA9PiB7XG4gICAgICAgIC8vIGdldCBhbGlhc2VzIGZvciB0aGUga2V5XG4gICAgICAgIGNvbnN0IG1ldGFkYXRhVGV4dCA9IHJlYWRGaWxlU3luYyhtYXBba2V5XSkudG9TdHJpbmcoJ3V0Zi04Jyk7XG4gICAgICAgIGNvbnN0IG1ldGFkYXRhID0gSlNPTi5wYXJzZShtZXRhZGF0YVRleHQpO1xuICAgICAgICBjb25zdCBhbGlhc2VzID0gbWV0YWRhdGFbJyRhbGlhc2VzJ107XG4gICAgICAgIGlmICghYWxpYXNlcykge1xuICAgICAgICAgIHJldHVybiBmYWxzZTtcbiAgICAgICAgfVxuICAgICAgICBjb25zdCBmb3VuZEFsaWFzID0gYWxpYXNlcy5maWx0ZXIoKGFsaWFzOiBzdHJpbmcpID0+IGFsaWFzID09PSBuYW1lKTtcblxuICAgICAgICByZXR1cm4gZm91bmRBbGlhcy5sZW5ndGggPiAwO1xuICAgICAgfSlbMF07XG4gIH1cblxuICBjb25zdCBtZXRhZGF0YVBhdGggPSBtYXBbY29tbWFuZE5hbWVdO1xuXG4gIGlmICghbWV0YWRhdGFQYXRoKSB7XG4gICAgcmV0dXJuIG51bGw7XG4gIH1cbiAgY29uc3QgbWV0YWRhdGFUZXh0ID0gcmVhZEZpbGVTeW5jKG1ldGFkYXRhUGF0aCkudG9TdHJpbmcoJ3V0Zi04Jyk7XG5cbiAgY29uc3QgbWV0YWRhdGEgPSBwYXJzZUpzb24obWV0YWRhdGFUZXh0KSBhcyBhbnk7XG5cbiAgcmV0dXJuIHtcbiAgICBwYXRoOiBtZXRhZGF0YVBhdGgsXG4gICAgdGV4dDogbWV0YWRhdGFUZXh0LFxuICAgIHJhd0RhdGE6IG1ldGFkYXRhLFxuICB9O1xufVxuXG4vLyBDcmVhdGUgYW4gaW5zdGFuY2Ugb2YgYSBjb21tYW5kLlxuYXN5bmMgZnVuY3Rpb24gY3JlYXRlQ29tbWFuZChtZXRhZGF0YTogQ29tbWFuZExvY2F0aW9uLFxuICAgICAgICAgICAgICAgICAgICAgICAgICAgICBjb250ZXh0OiBDb21tYW5kQ29udGV4dCxcbiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgbG9nZ2VyOiBsb2dnaW5nLkxvZ2dlcik6IFByb21pc2U8Q29tbWFuZCB8IG51bGw+IHtcbiAgY29uc3Qgc2NoZW1hID0gcGFyc2VTY2hlbWEobWV0YWRhdGEudGV4dCk7XG4gIGlmIChzY2hlbWEgPT09IG51bGwpIHtcbiAgICByZXR1cm4gbnVsbDtcbiAgfVxuICBjb25zdCBpbXBsUGF0aCA9IHNjaGVtYS4kaW1wbDtcbiAgaWYgKHR5cGVvZiBpbXBsUGF0aCAhPT0gJ3N0cmluZycpIHtcbiAgICB0aHJvdyBuZXcgRXJyb3IoJ0ltcGxlbWVudGF0aW9uIHBhdGggaXMgaW5jb3JyZWN0Jyk7XG4gIH1cblxuICBjb25zdCBpbXBsUmVmID0gbmV3IEV4cG9ydFN0cmluZ1JlZihpbXBsUGF0aCwgZGlybmFtZShtZXRhZGF0YS5wYXRoKSk7XG5cbiAgY29uc3QgY3RvciA9IGltcGxSZWYucmVmIGFzIENvbW1hbmRDb25zdHJ1Y3RvcjtcblxuICByZXR1cm4gbmV3IGN0b3IoY29udGV4dCwgbG9nZ2VyKTtcbn1cblxuZnVuY3Rpb24gbWFwQ29tbWFuZFNjb3BlKHNjb3BlOiAnaW4nIHwgJ291dCcgfCB1bmRlZmluZWQpOiBDb21tYW5kU2NvcGUge1xuICBsZXQgY29tbWFuZFNjb3BlID0gQ29tbWFuZFNjb3BlLmV2ZXJ5d2hlcmU7XG4gIHN3aXRjaCAoc2NvcGUpIHtcbiAgICBjYXNlICdpbic6XG4gICAgICBjb21tYW5kU2NvcGUgPSBDb21tYW5kU2NvcGUuaW5Qcm9qZWN0O1xuICAgICAgYnJlYWs7XG4gICAgY2FzZSAnb3V0JzpcbiAgICAgIGNvbW1hbmRTY29wZSA9IENvbW1hbmRTY29wZS5vdXRzaWRlUHJvamVjdDtcbiAgICAgIGJyZWFrO1xuICB9XG5cbiAgcmV0dXJuIGNvbW1hbmRTY29wZTtcbn1cblxuaW50ZXJmYWNlIENvbW1hbmRJbmZvIHtcbiAgbmFtZTogc3RyaW5nO1xuICBkZXNjcmlwdGlvbjogc3RyaW5nO1xuICBhbGlhc2VzOiBzdHJpbmdbXTtcbiAgaGlkZGVuOiBib29sZWFuO1xufVxuZnVuY3Rpb24gZ2V0QWxsQ29tbWFuZEluZm8obWFwOiBDb21tYW5kTWFwKTogQ29tbWFuZEluZm9bXSB7XG4gIHJldHVybiBPYmplY3Qua2V5cyhtYXApXG4gICAgLm1hcChuYW1lID0+IHtcbiAgICAgIHJldHVybiB7XG4gICAgICAgIG5hbWU6IG5hbWUsXG4gICAgICAgIG1ldGFkYXRhOiBmaW5kQ29tbWFuZChtYXAsIG5hbWUpLFxuICAgICAgfTtcbiAgICB9KVxuICAgIC5tYXAoaW5mbyA9PiB7XG4gICAgICBpZiAoaW5mby5tZXRhZGF0YSA9PT0gbnVsbCkge1xuICAgICAgICByZXR1cm4gbnVsbDtcbiAgICAgIH1cblxuICAgICAgcmV0dXJuIHtcbiAgICAgICAgbmFtZTogaW5mby5uYW1lLFxuICAgICAgICBkZXNjcmlwdGlvbjogaW5mby5tZXRhZGF0YS5yYXdEYXRhLmRlc2NyaXB0aW9uLFxuICAgICAgICBhbGlhc2VzOiBpbmZvLm1ldGFkYXRhLnJhd0RhdGEuJGFsaWFzZXMgfHwgW10sXG4gICAgICAgIGhpZGRlbjogaW5mby5tZXRhZGF0YS5yYXdEYXRhLiRoaWRkZW4gfHwgZmFsc2UsXG4gICAgICB9O1xuICAgIH0pXG4gICAgLmZpbHRlcihpbmZvID0+IGluZm8gIT09IG51bGwpIGFzIENvbW1hbmRJbmZvW107XG59XG4iXX0=