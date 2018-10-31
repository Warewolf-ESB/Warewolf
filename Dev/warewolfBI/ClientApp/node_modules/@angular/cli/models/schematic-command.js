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
const node_1 = require("@angular-devkit/core/node");
const schematics_1 = require("@angular-devkit/schematics");
const tools_1 = require("@angular-devkit/schematics/tools");
const operators_1 = require("rxjs/operators");
const workspace_loader_1 = require("../models/workspace-loader");
const config_1 = require("../utilities/config");
const command_1 = require("./command");
class UnknownCollectionError extends Error {
    constructor(collectionName) {
        super(`Invalid collection (${collectionName}).`);
    }
}
exports.UnknownCollectionError = UnknownCollectionError;
class SchematicCommand extends command_1.Command {
    constructor(context, logger, engineHost = new tools_1.NodeModulesEngineHost()) {
        super(context, logger);
        this.options = [];
        this.allowPrivateSchematics = false;
        this._host = new node_1.NodeJsSyncHost();
        this.argStrategy = command_1.ArgumentStrategy.Nothing;
        this.coreOptions = [
            {
                name: 'dryRun',
                type: 'boolean',
                default: false,
                aliases: ['d'],
                description: 'Run through without making any changes.',
            },
            {
                name: 'force',
                type: 'boolean',
                default: false,
                aliases: ['f'],
                description: 'Forces overwriting of files.',
            }
        ];
        this._engineHost = engineHost;
        this._engine = new schematics_1.SchematicEngine(this._engineHost);
        const registry = new core_1.schema.CoreSchemaRegistry(schematics_1.formats.standardFormats);
        this._engineHost.registerOptionsTransform(tools_1.validateOptionsWithSchema(registry));
    }
    initialize(_options) {
        return __awaiter(this, void 0, void 0, function* () {
            this._loadWorkspace();
        });
    }
    getEngineHost() {
        return this._engineHost;
    }
    getEngine() {
        return this._engine;
    }
    getCollection(collectionName) {
        const engine = this.getEngine();
        const collection = engine.createCollection(collectionName);
        if (collection === null) {
            throw new UnknownCollectionError(collectionName);
        }
        return collection;
    }
    getSchematic(collection, schematicName, allowPrivate) {
        return collection.createSchematic(schematicName, allowPrivate);
    }
    setPathOptions(options, workingDir) {
        if (workingDir === '') {
            return {};
        }
        return this.options
            .filter(o => o.format === 'path')
            .map(o => o.name)
            .filter(name => options[name] === undefined)
            .reduce((acc, curr) => {
            acc[curr] = workingDir;
            return acc;
        }, {});
    }
    /*
     * Runtime hook to allow specifying customized workflow
     */
    getWorkflow(options) {
        const { force, dryRun } = options;
        const fsHost = new core_1.virtualFs.ScopedHost(new node_1.NodeJsSyncHost(), core_1.normalize(this.project.root));
        return new tools_1.NodeWorkflow(fsHost, {
            force,
            dryRun,
            packageManager: config_1.getPackageManager(),
            root: this.project.root,
        });
    }
    _getWorkflow(options) {
        if (!this._workflow) {
            this._workflow = this.getWorkflow(options);
        }
        return this._workflow;
    }
    runSchematic(options) {
        const { collectionName, schematicName, debug, dryRun } = options;
        let schematicOptions = this.removeCoreOptions(options.schematicOptions);
        let nothingDone = true;
        let loggingQueue = [];
        let error = false;
        const workflow = this._getWorkflow(options);
        const workingDir = process.cwd().replace(this.project.root, '').replace(/\\/g, '/');
        const pathOptions = this.setPathOptions(schematicOptions, workingDir);
        schematicOptions = Object.assign({}, schematicOptions, pathOptions);
        const defaultOptions = this.readDefaults(collectionName, schematicName, schematicOptions);
        schematicOptions = Object.assign({}, schematicOptions, defaultOptions);
        // TODO: Remove warning check when 'targets' is default
        if (collectionName !== '@schematics/angular') {
            const [ast, configPath] = config_1.getWorkspaceRaw('local');
            if (ast) {
                const projectsKeyValue = ast.properties.find(p => p.key.value === 'projects');
                if (!projectsKeyValue || projectsKeyValue.value.kind !== 'object') {
                    return;
                }
                const positions = [];
                for (const projectKeyValue of projectsKeyValue.value.properties) {
                    const projectNode = projectKeyValue.value;
                    if (projectNode.kind !== 'object') {
                        continue;
                    }
                    const targetsKeyValue = projectNode.properties.find(p => p.key.value === 'targets');
                    if (targetsKeyValue) {
                        positions.push(targetsKeyValue.start);
                    }
                }
                if (positions.length > 0) {
                    const warning = core_1.tags.oneLine `
            WARNING: This command may not execute successfully.
            The package/collection may not support the 'targets' field within '${configPath}'.
            This can be corrected by renaming the following 'targets' fields to 'architect':
          `;
                    const locations = positions
                        .map((p, i) => `${i + 1}) Line: ${p.line + 1}; Column: ${p.character + 1}`)
                        .join('\n');
                    this.logger.warn(warning + '\n' + locations + '\n');
                }
            }
        }
        // Remove all of the original arguments which have already been parsed
        const argumentCount = this._originalOptions
            .filter(opt => {
            let isArgument = false;
            if (opt.$default !== undefined && opt.$default.$source === 'argv') {
                isArgument = true;
            }
            return isArgument;
        })
            .length;
        // Pass the rest of the arguments as the smart default "argv". Then delete it.
        const rawArgs = schematicOptions._.slice(argumentCount);
        workflow.registry.addSmartDefaultProvider('argv', (schema) => {
            if ('index' in schema) {
                return rawArgs[Number(schema['index'])];
            }
            else {
                return rawArgs;
            }
        });
        delete schematicOptions._;
        workflow.registry.addSmartDefaultProvider('projectName', (_schema) => {
            if (this._workspace) {
                try {
                    return this._workspace.getProjectByPath(core_1.normalize(process.cwd()))
                        || this._workspace.getDefaultProjectName();
                }
                catch (e) {
                    if (e instanceof core_1.experimental.workspace.AmbiguousProjectPathException) {
                        this.logger.warn(core_1.tags.oneLine `
              Two or more projects are using identical roots.
              Unable to determine project using current working directory.
              Using default workspace project instead.
            `);
                        return this._workspace.getDefaultProjectName();
                    }
                    throw e;
                }
            }
            return undefined;
        });
        workflow.reporter.subscribe((event) => {
            nothingDone = false;
            // Strip leading slash to prevent confusion.
            const eventPath = event.path.startsWith('/') ? event.path.substr(1) : event.path;
            switch (event.kind) {
                case 'error':
                    error = true;
                    const desc = event.description == 'alreadyExist' ? 'already exists' : 'does not exist.';
                    this.logger.warn(`ERROR! ${eventPath} ${desc}.`);
                    break;
                case 'update':
                    loggingQueue.push(core_1.tags.oneLine `
            ${core_1.terminal.white('UPDATE')} ${eventPath} (${event.content.length} bytes)
          `);
                    break;
                case 'create':
                    loggingQueue.push(core_1.tags.oneLine `
            ${core_1.terminal.green('CREATE')} ${eventPath} (${event.content.length} bytes)
          `);
                    break;
                case 'delete':
                    loggingQueue.push(`${core_1.terminal.yellow('DELETE')} ${eventPath}`);
                    break;
                case 'rename':
                    loggingQueue.push(`${core_1.terminal.blue('RENAME')} ${eventPath} => ${event.to}`);
                    break;
            }
        });
        workflow.lifeCycle.subscribe(event => {
            if (event.kind == 'end' || event.kind == 'post-tasks-start') {
                if (!error) {
                    // Output the logging queue, no error happened.
                    loggingQueue.forEach(log => this.logger.info(log));
                }
                loggingQueue = [];
                error = false;
            }
        });
        return new Promise((resolve) => {
            workflow.execute({
                collection: collectionName,
                schematic: schematicName,
                options: schematicOptions,
                debug: debug,
                logger: this.logger,
                allowPrivate: this.allowPrivateSchematics,
            })
                .subscribe({
                error: (err) => {
                    // In case the workflow was not successful, show an appropriate error message.
                    if (err instanceof schematics_1.UnsuccessfulWorkflowExecution) {
                        // "See above" because we already printed the error.
                        this.logger.fatal('The Schematic workflow failed. See above.');
                    }
                    else if (debug) {
                        this.logger.fatal(`An error occured:\n${err.message}\n${err.stack}`);
                    }
                    else {
                        this.logger.fatal(err.message);
                    }
                    resolve(1);
                },
                complete: () => {
                    const showNothingDone = !(options.showNothingDone === false);
                    if (nothingDone && showNothingDone) {
                        this.logger.info('Nothing to be done.');
                    }
                    if (dryRun) {
                        this.logger.warn(`\nNOTE: Run with "dry run" no changes were made.`);
                    }
                    resolve();
                },
            });
        });
    }
    removeCoreOptions(options) {
        const opts = Object.assign({}, options);
        if (this._originalOptions.find(option => option.name == 'dryRun')) {
            delete opts.dryRun;
        }
        if (this._originalOptions.find(option => option.name == 'force')) {
            delete opts.force;
        }
        if (this._originalOptions.find(option => option.name == 'debug')) {
            delete opts.debug;
        }
        return opts;
    }
    getOptions(options) {
        // Make a copy.
        this._originalOptions = [...this.options];
        const collectionName = options.collectionName || config_1.getDefaultSchematicCollection();
        const collection = this.getCollection(collectionName);
        const schematic = this.getSchematic(collection, options.schematicName, this.allowPrivateSchematics);
        this._deAliasedName = schematic.description.name;
        if (!schematic.description.schemaJson) {
            return Promise.resolve([]);
        }
        const properties = schematic.description.schemaJson.properties;
        const keys = Object.keys(properties);
        const availableOptions = keys
            .map(key => (Object.assign({}, properties[key], { name: core_1.strings.dasherize(key) })))
            .map(opt => {
            const types = ['string', 'boolean', 'integer', 'number'];
            // Ignore arrays / objects.
            if (types.indexOf(opt.type) === -1) {
                return null;
            }
            let aliases = [];
            if (opt.alias) {
                aliases = [...aliases, opt.alias];
            }
            if (opt.aliases) {
                aliases = [...aliases, ...opt.aliases];
            }
            const schematicDefault = opt.default;
            return Object.assign({}, opt, { aliases, default: undefined, // do not carry over schematics defaults
                schematicDefault, hidden: opt.visible === false });
        })
            .filter(x => x);
        return Promise.resolve(availableOptions);
    }
    _loadWorkspace() {
        if (this._workspace) {
            return;
        }
        const workspaceLoader = new workspace_loader_1.WorkspaceLoader(this._host);
        try {
            workspaceLoader.loadWorkspace(this.project.root).pipe(operators_1.take(1))
                .subscribe((workspace) => this._workspace = workspace, (err) => {
                if (!this.allowMissingWorkspace) {
                    // Ignore missing workspace
                    throw err;
                }
            });
        }
        catch (err) {
            if (!this.allowMissingWorkspace) {
                // Ignore missing workspace
                throw err;
            }
        }
    }
    _cleanDefaults(defaults, undefinedOptions) {
        Object.keys(defaults)
            .filter(key => !undefinedOptions.map(core_1.strings.camelize).includes(key))
            .forEach(key => {
            delete defaults[key];
        });
        return defaults;
    }
    readDefaults(collectionName, schematicName, options) {
        if (this._deAliasedName) {
            schematicName = this._deAliasedName;
        }
        const projectName = options.project;
        const defaults = config_1.getSchematicDefaults(collectionName, schematicName, projectName);
        // Get list of all undefined options.
        const undefinedOptions = this.options
            .filter(o => options[o.name] === undefined)
            .map(o => o.name);
        // Delete any default that is not undefined.
        this._cleanDefaults(defaults, undefinedOptions);
        return defaults;
    }
}
exports.SchematicCommand = SchematicCommand;
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoic2NoZW1hdGljLWNvbW1hbmQuanMiLCJzb3VyY2VSb290IjoiLi8iLCJzb3VyY2VzIjpbInBhY2thZ2VzL2FuZ3VsYXIvY2xpL21vZGVscy9zY2hlbWF0aWMtY29tbWFuZC50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiO0FBQUE7Ozs7OztHQU1HOzs7Ozs7Ozs7O0FBRUgsaURBQWlEO0FBQ2pELCtDQVc4QjtBQUM5QixvREFBMkQ7QUFDM0QsMkRBU29DO0FBQ3BDLDREQU8wQztBQUMxQyw4Q0FBc0M7QUFDdEMsaUVBQTZEO0FBQzdELGdEQUs2QjtBQUM3Qix1Q0FBOEU7QUEyQjlFLDRCQUFvQyxTQUFRLEtBQUs7SUFDL0MsWUFBWSxjQUFzQjtRQUNoQyxLQUFLLENBQUMsdUJBQXVCLGNBQWMsSUFBSSxDQUFDLENBQUM7SUFDbkQsQ0FBQztDQUNGO0FBSkQsd0RBSUM7QUFFRCxzQkFBdUMsU0FBUSxpQkFBTztJQVlwRCxZQUNJLE9BQXVCLEVBQUUsTUFBc0IsRUFDL0MsYUFBdUMsSUFBSSw2QkFBcUIsRUFBRTtRQUNwRSxLQUFLLENBQUMsT0FBTyxFQUFFLE1BQU0sQ0FBQyxDQUFDO1FBZGhCLFlBQU8sR0FBYSxFQUFFLENBQUM7UUFDdkIsMkJBQXNCLEdBQVksS0FBSyxDQUFDO1FBQ3pDLFVBQUssR0FBRyxJQUFJLHFCQUFjLEVBQUUsQ0FBQztRQU9yQyxnQkFBVyxHQUFHLDBCQUFnQixDQUFDLE9BQU8sQ0FBQztRQWFwQixnQkFBVyxHQUFhO1lBQ3pDO2dCQUNFLElBQUksRUFBRSxRQUFRO2dCQUNkLElBQUksRUFBRSxTQUFTO2dCQUNmLE9BQU8sRUFBRSxLQUFLO2dCQUNkLE9BQU8sRUFBRSxDQUFDLEdBQUcsQ0FBQztnQkFDZCxXQUFXLEVBQUUseUNBQXlDO2FBQ3ZEO1lBQ0Q7Z0JBQ0UsSUFBSSxFQUFFLE9BQU87Z0JBQ2IsSUFBSSxFQUFFLFNBQVM7Z0JBQ2YsT0FBTyxFQUFFLEtBQUs7Z0JBQ2QsT0FBTyxFQUFFLENBQUMsR0FBRyxDQUFDO2dCQUNk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QzlFLE9BQU8sQ0FBQyxHQUFHLENBQUMsRUFBRTtZQUNiLE9BQU8sUUFBUSxDQUFDLEdBQUcsQ0FBQyxDQUFDO1FBQ3ZCLENBQUMsQ0FBQyxDQUFDO1FBRUwsT0FBTyxRQUFRLENBQUM7SUFDbEIsQ0FBQztJQUVPLFlBQVksQ0FBQyxjQUFzQixFQUFFLGFBQXFCLEVBQUUsT0FBWTtRQUM5RSxJQUFJLElBQUksQ0FBQyxjQUFjLEVBQUU7WUFDdkIsYUFBYSxHQUFHLElBQUksQ0FBQyxjQUFjLENBQUM7U0FDckM7UUFFRCxNQUFNLFdBQVcsR0FBRyxPQUFPLENBQUMsT0FBTyxDQUFDO1FBQ3BDLE1BQU0sUUFBUSxHQUFHLDZCQUFvQixDQUFDLGNBQWMsRUFBRSxhQUFhLEVBQUUsV0FBVyxDQUFDLENBQUM7UUFFbEYscUNBQXFDO1FBQ3JDLE1BQU0sZ0JBQWdCLEdBQUcsSUFBSSxDQUFDLE9BQU87YUFDbEMsTUFBTSxDQUFDLENBQUMsQ0FBQyxFQUFFLENBQUMsT0FBTyxDQUFDLENBQUMsQ0FBQyxJQUFJLENBQUMsS0FBSyxTQUFTLENBQUM7YUFDMUMsR0FBRyxDQUFDLENBQUMsQ0FBQyxFQUFFLENBQUMsQ0FBQyxDQUFDLElBQUksQ0FBQyxDQUFDO1FBRXBCLDRDQUE0QztRQUM1QyxJQUFJLENBQUMsY0FBYyxDQUFDLFFBQVEsRUFBRSxnQkFBZ0IsQ0FBQyxDQUFDO1FBRWhELE9BQU8sUUFBUSxDQUFDO0lBQ2xCLENBQUM7Q0FDRjtBQXJaRCw0Q0FxWkMiLCJzb3VyY2VzQ29udGVudCI6WyIvKipcbiAqIEBsaWNlbnNlXG4gKiBDb3B5cmlnaHQgR29vZ2xlIEluYy4gQWxsIFJpZ2h0cyBSZXNlcnZlZC5cbiAqXG4gKiBVc2Ugb2YgdGhpcyBzb3VyY2UgY29kZSBpcyBnb3Zlcm5lZCBieSBhbiBNSVQtc3R5bGUgbGljZW5zZSB0aGF0IGNhbiBiZVxuICogZm91bmQgaW4gdGhlIExJQ0VOU0UgZmlsZSBhdCBodHRwczovL2FuZ3VsYXIuaW8vbGljZW5zZVxuICovXG5cbi8vIHRzbGludDpkaXNhYmxlOm5vLWdsb2JhbC10c2xpbnQtZGlzYWJsZSBuby1hbnlcbmltcG9ydCB7XG4gIEpzb25PYmplY3QsXG4gIGV4cGVyaW1lbnRhbCxcbiAganNvbixcbiAgbG9nZ2luZyxcbiAgbm9ybWFsaXplLFxuICBzY2hlbWEsXG4gIHN0cmluZ3MsXG4gIHRhZ3MsXG4gIHRlcm1pbmFsLFxuICB2aXJ0dWFsRnMsXG59IGZyb20gJ0Bhbmd1bGFyLWRldmtpdC9jb3JlJztcbmltcG9ydCB7IE5vZGVKc1N5bmNIb3N0IH0gZnJvbSAnQGFuZ3VsYXItZGV2a2l0L2NvcmUvbm9kZSc7XG5pbXBvcnQge1xuICBDb2xsZWN0aW9uLFxuICBEcnlSdW5FdmVudCxcbiAgRW5naW5lLFxuICBTY2hlbWF0aWMsXG4gIFNjaGVtYXRpY0VuZ2luZSxcbiAgVW5zdWNjZXNzZnVsV29ya2Zsb3dFeGVjdXRpb24sXG4gIGZvcm1hdHMsXG4gIHdvcmtmbG93LFxufSBmcm9tICdAYW5ndWxhci1kZXZraXQvc2NoZW1hdGljcyc7XG5pbXBvcnQge1xuICBGaWxlU3lzdGVtQ29sbGVjdGlvbkRlc2MsXG4gIEZpbGVTeXN0ZW1FbmdpbmVIb3N0QmFzZSxcbiAgRmlsZVN5c3RlbVNjaGVtYXRpY0Rlc2MsXG4gIE5vZGVNb2R1bGVzRW5naW5lSG9zdCxcbiAgTm9kZVdvcmtmbG93LFxuICB2YWxpZGF0ZU9wdGlvbnNXaXRoU2NoZW1hLFxufSBmcm9tICdAYW5ndWxhci1kZXZraXQvc2NoZW1hdGljcy90b29scyc7XG5pbXBvcnQgeyB0YWtlIH0gZnJvbSAncnhqcy9vcGVyYXRvcnMnO1xuaW1wb3J0IHsgV29ya3NwYWNlTG9hZGVyIH0gZnJvbSAnLi4vbW9kZWxzL3dvcmtzcGFjZS1sb2FkZXInO1xuaW1wb3J0IHtcbiAgZ2V0RGVmYXVsdFNjaGVtYXRpY0NvbGxlY3Rpb24sXG4gIGdldFBhY2thZ2VNYW5hZ2VyLFxuICBnZXRTY2hlbWF0aWNEZWZhdWx0cyxcbiAgZ2V0V29ya3NwYWNlUmF3LFxufSBmcm9tICcuLi91dGlsaXRpZXMvY29uZmlnJztcbmltcG9ydCB7IEFyZ3VtZW50U3RyYXRlZ3ksIENvbW1hbmQsIENvbW1hbmRDb250ZXh0LCBPcHRpb24gfSBmcm9tICcuL2NvbW1hbmQnO1xuXG5leHBvcnQgaW50ZXJmYWNlIENvcmVTY2hlbWF0aWNPcHRpb25zIHtcbiAgZHJ5UnVuOiBib29sZWFuO1xuICBmb3JjZTogYm9vbGVhbjtcbn1cblxuZXhwb3J0IGludGVyZmFjZSBSdW5TY2hlbWF0aWNPcHRpb25zIHtcbiAgY29sbGVjdGlvbk5hbWU6IHN0cmluZztcbiAgc2NoZW1hdGljTmFtZTogc3RyaW5nO1xuICBzY2hlbWF0aWNPcHRpb25zOiBhbnk7XG4gIGRlYnVnPzogYm9vbGVhbjtcbiAgZHJ5UnVuOiBib29sZWFuO1xuICBmb3JjZTogYm9vbGVhbjtcbiAgc2hvd05vdGhpbmdEb25lPzogYm9vbGVhbjtcbn1cblxuZXhwb3J0IGludGVyZmFjZSBHZXRPcHRpb25zT3B0aW9ucyB7XG4gIGNvbGxlY3Rpb25OYW1lOiBzdHJpbmc7XG4gIHNjaGVtYXRpY05hbWU6IHN0cmluZztcbn1cblxuZXhwb3J0IGludGVyZmFjZSBHZXRPcHRpb25zUmVzdWx0IHtcbiAgb3B0aW9uczogT3B0aW9uW107XG4gIGFyZ3VtZW50czogT3B0aW9uW107XG59XG5cbmV4cG9ydCBjbGFzcyBVbmtub3duQ29sbGVjdGlvbkVycm9yIGV4dGVuZHMgRXJyb3Ige1xuICBjb25zdHJ1Y3Rvcihjb2xsZWN0aW9uTmFtZTogc3RyaW5nKSB7XG4gICAgc3VwZXIoYEludmFsaWQgY29sbGVjdGlvbiAoJHtjb2xsZWN0aW9uTmFtZX0pLmApO1xuICB9XG59XG5cbmV4cG9ydCBhYnN0cmFjdCBjbGFzcyBTY2hlbWF0aWNDb21tYW5kIGV4dGVuZHMgQ29tbWFuZCB7XG4gIHJlYWRvbmx5IG9wdGlvbnM6IE9wdGlvbltdID0gW107XG4gIHJlYWRvbmx5IGFsbG93UHJpdmF0ZVNjaGVtYXRpY3M6IGJvb2xlYW4gPSBmYWxzZTtcbiAgcHJpdmF0ZSBfaG9zdCA9IG5ldyBOb2RlSnNTeW5jSG9zdCgpO1xuICBwcml2YXRlIF93b3Jrc3BhY2U6IGV4cGVyaW1lbnRhbC53b3Jrc3BhY2UuV29ya3NwYWNlO1xuICBwcml2YXRlIF9kZUFsaWFzZWROYW1lOiBzdHJpbmc7XG4gIHByaXZhdGUgX29yaWdpbmFsT3B0aW9uczogT3B0aW9uW107XG4gIHByaXZhdGUgX2VuZ2luZUhvc3Q6IEZpbGVTeXN0ZW1FbmdpbmVIb3N0QmFzZTtcbiAgcHJpdmF0ZSBfZW5naW5lOiBFbmdpbmU8RmlsZVN5c3RlbUNvbGxlY3Rpb25EZXNjLCBGaWxlU3lzdGVtU2NoZW1hdGljRGVzYz47XG4gIHByaXZhdGUgX3dvcmtmbG93OiB3b3JrZmxvdy5CYXNlV29ya2Zsb3c7XG4gIGFyZ1N0cmF0ZWd5ID0gQXJndW1lbnRTdHJhdGVneS5Ob3RoaW5nO1xuXG4gIGNvbnN0cnVjdG9yKFxuICAgICAgY29udGV4dDogQ29tbWFuZENvbnRleHQsIGxvZ2dlcjogbG9nZ2luZy5Mb2dnZXIsXG4gICAgICBlbmdpbmVIb3N0OiBGaWxlU3lzdGVtRW5naW5lSG9zdEJhc2UgPSBuZXcgTm9kZU1vZHVsZXNFbmdpbmVIb3N0KCkpIHtcbiAgICBzdXBlcihjb250ZXh0LCBsb2dnZXIpO1xuICAgIHRoaXMuX2VuZ2luZUhvc3QgPSBlbmdpbmVIb3N0O1xuICAgIHRoaXMuX2VuZ2luZSA9IG5ldyBTY2hlbWF0aWNFbmdpbmUodGhpcy5fZW5naW5lSG9zdCk7XG4gICAgY29uc3QgcmVnaXN0cnkgPSBuZXcgc2NoZW1hLkNvcmVTY2hlbWFSZWdpc3RyeShmb3JtYXRzLnN0YW5kYXJkRm9ybWF0cyk7XG4gICAgdGhpcy5fZW5naW5lSG9zdC5yZWdpc3Rlck9wdGlvbnNUcmFuc2Zvcm0oXG4gICAgICAgIHZhbGlkYXRlT3B0aW9uc1dpdGhTY2hlbWEocmVnaXN0cnkpKTtcbiAgfVxuXG4gIHByb3RlY3RlZCByZWFkb25seSBjb3JlT3B0aW9uczogT3B0aW9uW10gPSBbXG4gICAge1xuICAgICAgbmFtZTogJ2RyeVJ1bicsXG4gICAgICB0eXBlOiAnYm9vbGVhbicsXG4gICAgICBkZWZhdWx0OiBmYWxzZSxcbiAgICAgIGFsaWFzZXM6IFsnZCddLFxuICAgICAgZGVzY3JpcHRpb246ICdSdW4gdGhyb3VnaCB3aXRob3V0IG1ha2luZyBhbnkgY2hhbmdlcy4nLFxuICAgIH0sXG4gICAge1xuICAgICAgbmFtZTogJ2ZvcmNlJyxcbiAgICAgIHR5cGU6ICdib29sZWFuJyxcbiAgICAgIGRlZmF1bHQ6IGZhbHNlLFxuICAgICAgYWxpYXNlczogWydmJ10sXG4gICAgICBkZXNjcmlwdGlvbjogJ0ZvcmNlcyBvdmVyd3JpdGluZyBvZiBmaWxlcy4nLFxuICAgIH1dO1xuXG4gIHB1YmxpYyBhc3luYyBpbml0aWFsaXplKF9vcHRpb25zOiBhbnkpIHtcbiAgICB0aGlzLl9sb2FkV29ya3NwYWNlKCk7XG4gIH1cblxuICBwcm90ZWN0ZWQgZ2V0RW5naW5lSG9zdCgpIHtcbiAgICByZXR1cm4gdGhpcy5fZW5naW5lSG9zdDtcbiAgfVxuICBwcm90ZWN0ZWQgZ2V0RW5naW5lKCk6XG4gICAgICBFbmdpbmU8RmlsZVN5c3RlbUNvbGxlY3Rpb25EZXNjLCBGaWxlU3lzdGVtU2NoZW1hdGljRGVzYz4ge1xuICAgIHJldHVybiB0aGlzLl9lbmdpbmU7XG4gIH1cblxuICBwcm90ZWN0ZWQgZ2V0Q29sbGVjdGlvbihjb2xsZWN0aW9uTmFtZTogc3RyaW5nKTogQ29sbGVjdGlvbjxhbnksIGFueT4ge1xuICAgIGNvbnN0IGVuZ2luZSA9IHRoaXMuZ2V0RW5naW5lKCk7XG4gICAgY29uc3QgY29sbGVjdGlvbiA9IGVuZ2luZS5jcmVhdGVDb2xsZWN0aW9uKGNvbGxlY3Rpb25OYW1lKTtcblxuICAgIGlmIChjb2xsZWN0aW9uID09PSBudWxsKSB7XG4gICAgICB0aHJvdyBuZXcgVW5rbm93bkNvbGxlY3Rpb25FcnJvcihjb2xsZWN0aW9uTmFtZSk7XG4gICAgfVxuXG4gICAgcmV0dXJuIGNvbGxlY3Rpb247XG4gIH1cblxuICBwcm90ZWN0ZWQgZ2V0U2NoZW1hdGljKFxuICAgICAgY29sbGVjdGlvbjogQ29sbGVjdGlvbjxhbnksIGFueT4sIHNjaGVtYXRpY05hbWU6IHN0cmluZyxcbiAgICAgIGFsbG93UHJpdmF0ZT86IGJvb2xlYW4pOiBTY2hlbWF0aWM8YW55LCBhbnk+IHtcbiAgICByZXR1cm4gY29sbGVjdGlvbi5jcmVhdGVTY2hlbWF0aWMoc2NoZW1hdGljTmFtZSwgYWxsb3dQcml2YXRlKTtcbiAgfVxuXG4gIHByb3RlY3RlZCBzZXRQYXRoT3B0aW9ucyhvcHRpb25zOiBhbnksIHdvcmtpbmdEaXI6IHN0cmluZyk6IGFueSB7XG4gICAgaWYgKHdvcmtpbmdEaXIgPT09ICcnKSB7XG4gICAgICByZXR1cm4ge307XG4gICAgfVxuXG4gICAgcmV0dXJuIHRoaXMub3B0aW9uc1xuICAgICAgLmZpbHRlcihvID0+IG8uZm9ybWF0ID09PSAncGF0aCcpXG4gICAgICAubWFwKG8gPT4gby5uYW1lKVxuICAgICAgLmZpbHRlcihuYW1lID0+IG9wdGlvbnNbbmFtZV0gPT09IHVuZGVmaW5lZClcbiAgICAgIC5yZWR1Y2UoKGFjYzogYW55LCBjdXJyKSA9PiB7XG4gICAgICAgIGFjY1tjdXJyXSA9IHdvcmtpbmdEaXI7XG5cbiAgICAgICAgcmV0dXJuIGFjYztcbiAgICAgIH0sIHt9KTtcbiAgfVxuXG4gIC8qXG4gICAqIFJ1bnRpbWUgaG9vayB0byBhbGxvdyBzcGVjaWZ5aW5nIGN1c3RvbWl6ZWQgd29ya2Zsb3dcbiAgICovXG4gIHByb3RlY3RlZCBnZXRXb3JrZmxvdyhvcHRpb25zOiBSdW5TY2hlbWF0aWNPcHRpb25zKTogd29ya2Zsb3cuQmFzZVdvcmtmbG93IHtcbiAgICBjb25zdCB7Zm9yY2UsIGRyeVJ1bn0gPSBvcHRpb25zO1xuICAgIGNvbnN0IGZzSG9zdCA9IG5ldyB2aXJ0dWFsRnMuU2NvcGVkSG9zdChcbiAgICAgICAgbmV3IE5vZGVKc1N5bmNIb3N0KCksIG5vcm1hbGl6ZSh0aGlzLnByb2plY3Qucm9vdCkpO1xuXG4gICAgcmV0dXJuIG5ldyBOb2RlV29ya2Zsb3coXG4gICAgICAgIGZzSG9zdCBhcyBhbnksXG4gICAgICAgIHtcbiAgICAgICAgICBmb3JjZSxcbiAgICAgICAgICBkcnlSdW4sXG4gICAgICAgICAgcGFja2FnZU1hbmFnZXI6IGdldFBhY2thZ2VNYW5hZ2VyKCksXG4gICAgICAgICAgcm9vdDogdGhpcy5wcm9qZWN0LnJvb3QsXG4gICAgICAgIH0sXG4gICAgKTtcbiAgfVxuXG4gIHByaXZhdGUgX2dldFdvcmtmbG93KG9wdGlvbnM6IFJ1blNjaGVtYXRpY09wdGlvbnMpOiB3b3JrZmxvdy5CYXNlV29ya2Zsb3cge1xuICAgIGlmICghdGhpcy5fd29ya2Zsb3cpIHtcbiAgICAgIHRoaXMuX3dvcmtmbG93ID0gdGhpcy5nZXRXb3JrZmxvdyhvcHRpb25zKTtcbiAgICB9XG5cbiAgICByZXR1cm4gdGhpcy5fd29ya2Zsb3c7XG4gIH1cblxuICBwcm90ZWN0ZWQgcnVuU2NoZW1hdGljKG9wdGlvbnM6IFJ1blNjaGVtYXRpY09wdGlvbnMpIHtcbiAgICBjb25zdCB7Y29sbGVjdGlvbk5hbWUsIHNjaGVtYXRpY05hbWUsIGRlYnVnLCBkcnlSdW59ID0gb3B0aW9ucztcbiAgICBsZXQgc2NoZW1hdGljT3B0aW9ucyA9IHRoaXMucmVtb3ZlQ29yZU9wdGlvbnMob3B0aW9ucy5zY2hlbWF0aWNPcHRpb25zKTtcbiAgICBsZXQgbm90aGluZ0RvbmUgPSB0cnVlO1xuICAgIGxldCBsb2dnaW5nUXVldWU6IHN0cmluZ1tdID0gW107XG4gICAgbGV0IGVycm9yID0gZmFsc2U7XG4gICAgY29uc3Qgd29ya2Zsb3cgPSB0aGlzLl9nZXRXb3JrZmxvdyhvcHRpb25zKTtcblxuICAgIGNvbnN0IHdvcmtpbmdEaXIgPSBwcm9jZXNzLmN3ZCgpLnJlcGxhY2UodGhpcy5wcm9qZWN0LnJvb3QsICcnKS5yZXBsYWNlKC9cXFxcL2csICcvJyk7XG4gICAgY29uc3QgcGF0aE9wdGlvbnMgPSB0aGlzLnNldFBhdGhPcHRpb25zKHNjaGVtYXRpY09wdGlvbnMsIHdvcmtpbmdEaXIpO1xuICAgIHNjaGVtYXRpY09wdGlvbnMgPSB7IC4uLnNjaGVtYXRpY09wdGlvbnMsIC4uLnBhdGhPcHRpb25zIH07XG4gICAgY29uc3QgZGVmYXVsdE9wdGlvbnMgPSB0aGlzLnJlYWREZWZhdWx0cyhjb2xsZWN0aW9uTmFtZSwgc2NoZW1hdGljTmFtZSwgc2NoZW1hdGljT3B0aW9ucyk7XG4gICAgc2NoZW1hdGljT3B0aW9ucyA9IHsgLi4uc2NoZW1hdGljT3B0aW9ucywgLi4uZGVmYXVsdE9wdGlvbnMgfTtcblxuICAgIC8vIFRPRE86IFJlbW92ZSB3YXJuaW5nIGNoZWNrIHdoZW4gJ3RhcmdldHMnIGlzIGRlZmF1bHRcbiAgICBpZiAoY29sbGVjdGlvbk5hbWUgIT09ICdAc2NoZW1hdGljcy9hbmd1bGFyJykge1xuICAgICAgY29uc3QgW2FzdCwgY29uZmlnUGF0aF0gPSBnZXRXb3Jrc3BhY2VSYXcoJ2xvY2FsJyk7XG4gICAgICBpZiAoYXN0KSB7XG4gICAgICAgIGNvbnN0IHByb2plY3RzS2V5VmFsdWUgPSBhc3QucHJvcGVydGllcy5maW5kKHAgPT4gcC5rZXkudmFsdWUgPT09ICdwcm9qZWN0cycpO1xuICAgICAgICBpZiAoIXByb2plY3RzS2V5VmFsdWUgfHwgcHJvamVjdHNLZXlWYWx1ZS52YWx1ZS5raW5kICE9PSAnb2JqZWN0Jykge1xuICAgICAgICAgIHJldHVybjtcbiAgICAgICAgfVxuXG4gICAgICAgIGNvbnN0IHBvc2l0aW9uczoganNvbi5Qb3NpdGlvbltdID0gW107XG4gICAgICAgIGZvciAoY29uc3QgcHJvamVjdEtleVZhbHVlIG9mIHByb2plY3RzS2V5VmFsdWUudmFsdWUucHJvcGVydGllcykge1xuICAgICAgICAgIGNvbnN0IHByb2plY3ROb2RlID0gcHJvamVjdEtleVZhbHVlLnZhbHVlO1xuICAgICAgICAgIGlmIChwcm9qZWN0Tm9kZS5raW5kICE9PSAnb2JqZWN0Jykge1xuICAgICAgICAgICAgY29udGludWU7XG4gICAgICAgICAgfVxuICAgICAgICAgIGNvbnN0IHRhcmdldHNLZXlWYWx1ZSA9IHByb2plY3ROb2RlLnByb3BlcnRpZXMuZmluZChwID0+IHAua2V5LnZhbHVlID09PSAndGFyZ2V0cycpO1xuICAgICAgICAgIGlmICh0YXJnZXRzS2V5VmFsdWUpIHtcbiAgICAgICAgICAgIHBvc2l0aW9ucy5wdXNoKHRhcmdldHNLZXlWYWx1ZS5zdGFydCk7XG4gICAgICAgICAgfVxuICAgICAgICB9XG5cbiAgICAgICAgaWYgKHBvc2l0aW9ucy5sZW5ndGggPiAwKSB7XG4gICAgICAgICAgY29uc3Qgd2FybmluZyA9IHRhZ3Mub25lTGluZWBcbiAgICAgICAgICAgIFdBUk5JTkc6IFRoaXMgY29tbWFuZCBtYXkgbm90IGV4ZWN1dGUgc3VjY2Vzc2Z1bGx5LlxuICAgICAgICAgICAgVGhlIHBhY2thZ2UvY29sbGVjdGlvbiBtYXkgbm90IHN1cHBvcnQgdGhlICd0YXJnZXRzJyBmaWVsZCB3aXRoaW4gJyR7Y29uZmlnUGF0aH0nLlxuICAgICAgICAgICAgVGhpcyBjYW4gYmUgY29ycmVjdGVkIGJ5IHJlbmFtaW5nIHRoZSBmb2xsb3dpbmcgJ3RhcmdldHMnIGZpZWxkcyB0byAnYXJjaGl0ZWN0JzpcbiAgICAgICAgICBgO1xuXG4gICAgICAgICAgY29uc3QgbG9jYXRpb25zID0gcG9zaXRpb25zXG4gICAgICAgICAgICAubWFwKChwLCBpKSA9PiBgJHtpICsgMX0pIExpbmU6ICR7cC5saW5lICsgMX07IENvbHVtbjogJHtwLmNoYXJhY3RlciArIDF9YClcbiAgICAgICAgICAgIC5qb2luKCdcXG4nKTtcblxuICAgICAgICAgIHRoaXMubG9nZ2VyLndhcm4od2FybmluZyArICdcXG4nICsgbG9jYXRpb25zICsgJ1xcbicpO1xuICAgICAgICB9XG4gICAgICB9XG4gICAgfVxuXG4gICAgLy8gUmVtb3ZlIGFsbCBvZiB0aGUgb3JpZ2luYWwgYXJndW1lbnRzIHdoaWNoIGhhdmUgYWxyZWFkeSBiZWVuIHBhcnNlZFxuXG4gICAgY29uc3QgYXJndW1lbnRDb3VudCA9IHRoaXMuX29yaWdpbmFsT3B0aW9uc1xuICAgICAgLmZpbHRlcihvcHQgPT4ge1xuICAgICAgICBsZXQgaXNBcmd1bWVudCA9IGZhbHNlO1xuICAgICAgICBpZiAob3B0LiRkZWZhdWx0ICE9PSB1bmRlZmluZWQgJiYgb3B0LiRkZWZhdWx0LiRzb3VyY2UgPT09ICdhcmd2Jykge1xuICAgICAgICAgIGlzQXJndW1lbnQgPSB0cnVlO1xuICAgICAgICB9XG5cbiAgICAgICAgcmV0dXJuIGlzQXJndW1lbnQ7XG4gICAgICB9KVxuICAgICAgLmxlbmd0aDtcblxuICAgIC8vIFBhc3MgdGhlIHJlc3Qgb2YgdGhlIGFyZ3VtZW50cyBhcyB0aGUgc21hcnQgZGVmYXVsdCBcImFyZ3ZcIi4gVGhlbiBkZWxldGUgaXQuXG4gICAgY29uc3QgcmF3QXJncyA9IHNjaGVtYXRpY09wdGlvbnMuXy5zbGljZShhcmd1bWVudENvdW50KTtcbiAgICB3b3JrZmxvdy5yZWdpc3RyeS5hZGRTbWFydERlZmF1bHRQcm92aWRlcignYXJndicsIChzY2hlbWE6IEpzb25PYmplY3QpID0+IHtcbiAgICAgIGlmICgnaW5kZXgnIGluIHNjaGVtYSkge1xuICAgICAgICByZXR1cm4gcmF3QXJnc1tOdW1iZXIoc2NoZW1hWydpbmRleCddKV07XG4gICAgICB9IGVsc2Uge1xuICAgICAgICByZXR1cm4gcmF3QXJncztcbiAgICAgIH1cbiAgICB9KTtcbiAgICBkZWxldGUgc2NoZW1hdGljT3B0aW9ucy5fO1xuXG4gICAgd29ya2Zsb3cucmVnaXN0cnkuYWRkU21hcnREZWZhdWx0UHJvdmlkZXIoJ3Byb2plY3ROYW1lJywgKF9zY2hlbWE6IEpzb25PYmplY3QpID0+IHtcbiAgICAgIGlmICh0aGlzLl93b3Jrc3BhY2UpIHtcbiAgICAgICAgdHJ5IHtcbiAgICAgICAgcmV0dXJuIHRoaXMuX3dvcmtzcGFjZS5nZXRQcm9qZWN0QnlQYXRoKG5vcm1hbGl6ZShwcm9jZXNzLmN3ZCgpKSlcbiAgICAgICAgICAgICAgIHx8IHRoaXMuX3dvcmtzcGFjZS5nZXREZWZhdWx0UHJvamVjdE5hbWUoKTtcbiAgICAgICAgfSBjYXRjaCAoZSkge1xuICAgICAgICAgIGlmIChlIGluc3RhbmNlb2YgZXhwZXJpbWVudGFsLndvcmtzcGFjZS5BbWJpZ3VvdXNQcm9qZWN0UGF0aEV4Y2VwdGlvbikge1xuICAgICAgICAgICAgdGhpcy5sb2dnZXIud2Fybih0YWdzLm9uZUxpbmVgXG4gICAgICAgICAgICAgIFR3byBvciBtb3JlIHByb2plY3RzIGFyZSB1c2luZyBpZGVudGljYWwgcm9vdHMuXG4gICAgICAgICAgICAgIFVuYWJsZSB0byBkZXRlcm1pbmUgcHJvamVjdCB1c2luZyBjdXJyZW50IHdvcmtpbmcgZGlyZWN0b3J5LlxuICAgICAgICAgICAgICBVc2luZyBkZWZhdWx0IHdvcmtzcGFjZSBwcm9qZWN0IGluc3RlYWQuXG4gICAgICAgICAgICBgKTtcblxuICAgICAgICAgICAgcmV0dXJuIHRoaXMuX3dvcmtzcGFjZS5nZXREZWZhdWx0UHJvamVjdE5hbWUoKTtcbiAgICAgICAgICB9XG4gICAgICAgICAgdGhyb3cgZTtcbiAgICAgICAgfVxuICAgICAgfVxuXG4gICAgICByZXR1cm4gdW5kZWZpbmVkO1xuICAgIH0pO1xuXG4gICAgd29ya2Zsb3cucmVwb3J0ZXIuc3Vic2NyaWJlKChldmVudDogRHJ5UnVuRXZlbnQpID0+IHtcbiAgICAgIG5vdGhpbmdEb25lID0gZmFsc2U7XG5cbiAgICAgIC8vIFN0cmlwIGxlYWRpbmcgc2xhc2ggdG8gcHJldmVudCBjb25mdXNpb24uXG4gICAgICBjb25zdCBldmVudFBhdGggPSBldmVudC5wYXRoLnN0YXJ0c1dpdGgoJy8nKSA/IGV2ZW50LnBhdGguc3Vic3RyKDEpIDogZXZlbnQucGF0aDtcblxuICAgICAgc3dpdGNoIChldmVudC5raW5kKSB7XG4gICAgICAgIGNhc2UgJ2Vycm9yJzpcbiAgICAgICAgICBlcnJvciA9IHRydWU7XG4gICAgICAgICAgY29uc3QgZGVzYyA9IGV2ZW50LmRlc2NyaXB0aW9uID09ICdhbHJlYWR5RXhpc3QnID8gJ2FscmVhZHkgZXhpc3RzJyA6ICdkb2VzIG5vdCBleGlzdC4nO1xuICAgICAgICAgIHRoaXMubG9nZ2VyLndhcm4oYEVSUk9SISAke2V2ZW50UGF0aH0gJHtkZXNjfS5gKTtcbiAgICAgICAgICBicmVhaztcbiAgICAgICAgY2FzZSAndXBkYXRlJzpcbiAgICAgICAgICBsb2dnaW5nUXVldWUucHVzaCh0YWdzLm9uZUxpbmVgXG4gICAgICAgICAgICAke3Rlcm1pbmFsLndoaXRlKCdVUERBVEUnKX0gJHtldmVudFBhdGh9ICgke2V2ZW50LmNvbnRlbnQubGVuZ3RofSBieXRlcylcbiAgICAgICAgICBgKTtcbiAgICAgICAgICBicmVhaztcbiAgICAgICAgY2FzZSAnY3JlYXRlJzpcbiAgICAgICAgICBsb2dnaW5nUXVldWUucHVzaCh0YWdzLm9uZUxpbmVgXG4gICAgICAgICAgICAke3Rlcm1pbmFsLmdyZWVuKCdDUkVBVEUnKX0gJHtldmVudFBhdGh9ICgke2V2ZW50LmNvbnRlbnQubGVuZ3RofSBieXRlcylcbiAgICAgICAgICBgKTtcbiAgICAgICAgICBicmVhaztcbiAgICAgICAgY2FzZSAnZGVsZXRlJzpcbiAgICAgICAgICBsb2dnaW5nUXVldWUucHVzaChgJHt0ZXJtaW5hbC55ZWxsb3coJ0RFTEVURScpfSAke2V2ZW50UGF0aH1gKTtcbiAgICAgICAgICBicmVhaztcbiAgICAgICAgY2FzZSAncmVuYW1lJzpcbiAgICAgICAgICBsb2dnaW5nUXVldWUucHVzaChgJHt0ZXJtaW5hbC5ibHVlKCdSRU5BTUUnKX0gJHtldmVudFBhdGh9ID0+ICR7ZXZlbnQudG99YCk7XG4gICAgICAgICAgYnJlYWs7XG4gICAgICB9XG4gICAgfSk7XG5cbiAgICB3b3JrZmxvdy5saWZlQ3ljbGUuc3Vic2NyaWJlKGV2ZW50ID0+IHtcbiAgICAgIGlmIChldmVudC5raW5kID09ICdlbmQnIHx8IGV2ZW50LmtpbmQgPT0gJ3Bvc3QtdGFza3Mtc3RhcnQnKSB7XG4gICAgICAgIGlmICghZXJyb3IpIHtcbiAgICAgICAgICAvLyBPdXRwdXQgdGhlIGxvZ2dpbmcgcXVldWUsIG5vIGVycm9yIGhhcHBlbmVkLlxuICAgICAgICAgIGxvZ2dpbmdRdWV1ZS5mb3JFYWNoKGxvZyA9PiB0aGlzLmxvZ2dlci5pbmZvKGxvZykpO1xuICAgICAgICB9XG5cbiAgICAgICAgbG9nZ2luZ1F1ZXVlID0gW107XG4gICAgICAgIGVycm9yID0gZmFsc2U7XG4gICAgICB9XG4gICAgfSk7XG5cbiAgICByZXR1cm4gbmV3IFByb21pc2U8bnVtYmVyIHwgdm9pZD4oKHJlc29sdmUpID0+IHtcbiAgICAgIHdvcmtmbG93LmV4ZWN1dGUoe1xuICAgICAgICBjb2xsZWN0aW9uOiBjb2xsZWN0aW9uTmFtZSxcbiAgICAgICAgc2NoZW1hdGljOiBzY2hlbWF0aWNOYW1lLFxuICAgICAgICBvcHRpb25zOiBzY2hlbWF0aWNPcHRpb25zLFxuICAgICAgICBkZWJ1ZzogZGVidWcsXG4gICAgICAgIGxvZ2dlcjogdGhpcy5sb2dnZXIgYXMgYW55LFxuICAgICAgICBhbGxvd1ByaXZhdGU6IHRoaXMuYWxsb3dQcml2YXRlU2NoZW1hdGljcyxcbiAgICAgIH0pXG4gICAgICAuc3Vic2NyaWJlKHtcbiAgICAgICAgZXJyb3I6IChlcnI6IEVycm9yKSA9PiB7XG4gICAgICAgICAgLy8gSW4gY2FzZSB0aGUgd29ya2Zsb3cgd2FzIG5vdCBzdWNjZXNzZnVsLCBzaG93IGFuIGFwcHJvcHJpYXRlIGVycm9yIG1lc3NhZ2UuXG4gICAgICAgICAgaWYgKGVyciBpbnN0YW5jZW9mIFVuc3VjY2Vzc2Z1bFdvcmtmbG93RXhlY3V0aW9uKSB7XG4gICAgICAgICAgICAvLyBcIlNlZSBhYm92ZVwiIGJlY2F1c2Ugd2UgYWxyZWFkeSBwcmludGVkIHRoZSBlcnJvci5cbiAgICAgICAgICAgIHRoaXMubG9nZ2VyLmZhdGFsKCdUaGUgU2NoZW1hdGljIHdvcmtmbG93IGZhaWxlZC4gU2VlIGFib3ZlLicpO1xuICAgICAgICAgIH0gZWxzZSBpZiAoZGVidWcpIHtcbiAgICAgICAgICAgIHRoaXMubG9nZ2VyLmZhdGFsKGBBbiBlcnJvciBvY2N1cmVkOlxcbiR7ZXJyLm1lc3NhZ2V9XFxuJHtlcnIuc3RhY2t9YCk7XG4gICAgICAgICAgfSBlbHNlIHtcbiAgICAgICAgICAgIHRoaXMubG9nZ2VyLmZhdGFsKGVyci5tZXNzYWdlKTtcbiAgICAgICAgICB9XG5cbiAgICAgICAgICByZXNvbHZlKDEpO1xuICAgICAgICB9LFxuICAgICAgICBjb21wbGV0ZTogKCkgPT4ge1xuICAgICAgICAgIGNvbnN0IHNob3dOb3RoaW5nRG9uZSA9ICEob3B0aW9ucy5zaG93Tm90aGluZ0RvbmUgPT09IGZhbHNlKTtcbiAgICAgICAgICBpZiAobm90aGluZ0RvbmUgJiYgc2hvd05vdGhpbmdEb25lKSB7XG4gICAgICAgICAgICB0aGlzLmxvZ2dlci5pbmZvKCdOb3RoaW5nIHRvIGJlIGRvbmUuJyk7XG4gICAgICAgICAgfVxuICAgICAgICAgIGlmIChkcnlSdW4pIHtcbiAgICAgICAgICAgIHRoaXMubG9nZ2VyLndhcm4oYFxcbk5PVEU6IFJ1biB3aXRoIFwiZHJ5IHJ1blwiIG5vIGNoYW5nZXMgd2VyZSBtYWRlLmApO1xuICAgICAgICAgIH1cbiAgICAgICAgICByZXNvbHZlKCk7XG4gICAgICAgIH0sXG4gICAgICB9KTtcbiAgICB9KTtcbiAgfVxuXG4gIHByb3RlY3RlZCByZW1vdmVDb3JlT3B0aW9ucyhvcHRpb25zOiBhbnkpOiBhbnkge1xuICAgIGNvbnN0IG9wdHMgPSBPYmplY3QuYXNzaWduKHt9LCBvcHRpb25zKTtcbiAgICBpZiAodGhpcy5fb3JpZ2luYWxPcHRpb25zLmZpbmQob3B0aW9uID0+IG9wdGlvbi5uYW1lID09ICdkcnlSdW4nKSkge1xuICAgICAgZGVsZXRlIG9wdHMuZHJ5UnVuO1xuICAgIH1cbiAgICBpZiAodGhpcy5fb3JpZ2luYWxPcHRpb25zLmZpbmQob3B0aW9uID0+IG9wdGlvbi5uYW1lID09ICdmb3JjZScpKSB7XG4gICAgICBkZWxldGUgb3B0cy5mb3JjZTtcbiAgICB9XG4gICAgaWYgKHRoaXMuX29yaWdpbmFsT3B0aW9ucy5maW5kKG9wdGlvbiA9PiBvcHRpb24ubmFtZSA9PSAnZGVidWcnKSkge1xuICAgICAgZGVsZXRlIG9wdHMuZGVidWc7XG4gICAgfVxuXG4gICAgcmV0dXJuIG9wdHM7XG4gIH1cblxuICBwcm90ZWN0ZWQgZ2V0T3B0aW9ucyhvcHRpb25zOiBHZXRPcHRpb25zT3B0aW9ucyk6IFByb21pc2U8T3B0aW9uW10+IHtcbiAgICAvLyBNYWtlIGEgY29weS5cbiAgICB0aGlzLl9vcmlnaW5hbE9wdGlvbnMgPSBbLi4udGhpcy5vcHRpb25zXTtcblxuICAgIGNvbnN0IGNvbGxlY3Rpb25OYW1lID0gb3B0aW9ucy5jb2xsZWN0aW9uTmFtZSB8fCBnZXREZWZhdWx0U2NoZW1hdGljQ29sbGVjdGlvbigpO1xuXG4gICAgY29uc3QgY29sbGVjdGlvbiA9IHRoaXMuZ2V0Q29sbGVjdGlvbihjb2xsZWN0aW9uTmFtZSk7XG5cbiAgICBjb25zdCBzY2hlbWF0aWMgPSB0aGlzLmdldFNjaGVtYXRpYyhjb2xsZWN0aW9uLCBvcHRpb25zLnNjaGVtYXRpY05hbWUsXG4gICAgICB0aGlzLmFsbG93UHJpdmF0ZVNjaGVtYXRpY3MpO1xuICAgIHRoaXMuX2RlQWxpYXNlZE5hbWUgPSBzY2hlbWF0aWMuZGVzY3JpcHRpb24ubmFtZTtcblxuICAgIGlmICghc2NoZW1hdGljLmRlc2NyaXB0aW9uLnNjaGVtYUpzb24pIHtcbiAgICAgIHJldHVybiBQcm9taXNlLnJlc29sdmUoW10pO1xuICAgIH1cblxuICAgIGNvbnN0IHByb3BlcnRpZXMgPSBzY2hlbWF0aWMuZGVzY3JpcHRpb24uc2NoZW1hSnNvbi5wcm9wZXJ0aWVzO1xuICAgIGNvbnN0IGtleXMgPSBPYmplY3Qua2V5cyhwcm9wZXJ0aWVzKTtcbiAgICBjb25zdCBhdmFpbGFibGVPcHRpb25zID0ga2V5c1xuICAgICAgLm1hcChrZXkgPT4gKHsgLi4ucHJvcGVydGllc1trZXldLCAuLi57IG5hbWU6IHN0cmluZ3MuZGFzaGVyaXplKGtleSkgfSB9KSlcbiAgICAgIC5tYXAob3B0ID0+IHtcbiAgICAgICAgY29uc3QgdHlwZXMgPSBbJ3N0cmluZycsICdib29sZWFuJywgJ2ludGVnZXInLCAnbnVtYmVyJ107XG4gICAgICAgIC8vIElnbm9yZSBhcnJheXMgLyBvYmplY3RzLlxuICAgICAgICBpZiAodHlwZXMuaW5kZXhPZihvcHQudHlwZSkgPT09IC0xKSB7XG4gICAgICAgICAgcmV0dXJuIG51bGw7XG4gICAgICAgIH1cblxuICAgICAgICBsZXQgYWxpYXNlczogc3RyaW5nW10gPSBbXTtcbiAgICAgICAgaWYgKG9wdC5hbGlhcykge1xuICAgICAgICAgIGFsaWFzZXMgPSBbLi4uYWxpYXNlcywgb3B0LmFsaWFzXTtcbiAgICAgICAgfVxuICAgICAgICBpZiAob3B0LmFsaWFzZXMpIHtcbiAgICAgICAgICBhbGlhc2VzID0gWy4uLmFsaWFzZXMsIC4uLm9wdC5hbGlhc2VzXTtcbiAgICAgICAgfVxuICAgICAgICBjb25zdCBzY2hlbWF0aWNEZWZhdWx0ID0gb3B0LmRlZmF1bHQ7XG5cbiAgICAgICAgcmV0dXJuIHtcbiAgICAgICAgICAuLi5vcHQsXG4gICAgICAgICAgYWxpYXNlcyxcbiAgICAgICAgICBkZWZhdWx0OiB1bmRlZmluZWQsIC8vIGRvIG5vdCBjYXJyeSBvdmVyIHNjaGVtYXRpY3MgZGVmYXVsdHNcbiAgICAgICAgICBzY2hlbWF0aWNEZWZhdWx0LFxuICAgICAgICAgIGhpZGRlbjogb3B0LnZpc2libGUgPT09IGZhbHNlLFxuICAgICAgICB9O1xuICAgICAgfSlcbiAgICAgIC5maWx0ZXIoeCA9PiB4KTtcblxuICAgIHJldHVybiBQcm9taXNlLnJlc29sdmUoYXZhaWxhYmxlT3B0aW9ucyk7XG4gIH1cblxuICBwcml2YXRlIF9sb2FkV29ya3NwYWNlKCkge1xuICAgIGlmICh0aGlzLl93b3Jrc3BhY2UpIHtcbiAgICAgIHJldHVybjtcbiAgICB9XG4gICAgY29uc3Qgd29ya3NwYWNlTG9hZGVyID0gbmV3IFdvcmtzcGFjZUxvYWRlcih0aGlzLl9ob3N0KTtcblxuICAgIHRyeSB7XG4gICAgICB3b3Jrc3BhY2VMb2FkZXIubG9hZFdvcmtzcGFjZSh0aGlzLnByb2plY3Qucm9vdCkucGlwZSh0YWtlKDEpKVxuICAgICAgICAuc3Vic2NyaWJlKFxuICAgICAgICAgICh3b3Jrc3BhY2U6IGV4cGVyaW1lbnRhbC53b3Jrc3BhY2UuV29ya3NwYWNlKSA9PiB0aGlzLl93b3Jrc3BhY2UgPSB3b3Jrc3BhY2UsXG4gICAgICAgICAgKGVycjogRXJyb3IpID0+IHtcbiAgICAgICAgICAgIGlmICghdGhpcy5hbGxvd01pc3NpbmdXb3Jrc3BhY2UpIHtcbiAgICAgICAgICAgICAgLy8gSWdub3JlIG1pc3Npbmcgd29ya3NwYWNlXG4gICAgICAgICAgICAgIHRocm93IGVycjtcbiAgICAgICAgICAgIH1cbiAgICAgICAgICB9LFxuICAgICAgICApO1xuICAgIH0gY2F0Y2ggKGVycikge1xuICAgICAgaWYgKCF0aGlzLmFsbG93TWlzc2luZ1dvcmtzcGFjZSkge1xuICAgICAgICAvLyBJZ25vcmUgbWlzc2luZyB3b3Jrc3BhY2VcbiAgICAgICAgdGhyb3cgZXJyO1xuICAgICAgfVxuICAgIH1cbiAgfVxuXG4gIHByaXZhdGUgX2NsZWFuRGVmYXVsdHM8VCwgSyBleHRlbmRzIGtleW9mIFQ+KGRlZmF1bHRzOiBULCB1bmRlZmluZWRPcHRpb25zOiBzdHJpbmdbXSk6IFQge1xuICAgIChPYmplY3Qua2V5cyhkZWZhdWx0cykgYXMgS1tdKVxuICAgICAgLmZpbHRlcihrZXkgPT4gIXVuZGVmaW5lZE9wdGlvbnMubWFwKHN0cmluZ3MuY2FtZWxpemUpLmluY2x1ZGVzKGtleSBhcyBzdHJpbmcpKVxuICAgICAgLmZvckVhY2goa2V5ID0+IHtcbiAgICAgICAgZGVsZXRlIGRlZmF1bHRzW2tleV07XG4gICAgICB9KTtcblxuICAgIHJldHVybiBkZWZhdWx0cztcbiAgfVxuXG4gIHByaXZhdGUgcmVhZERlZmF1bHRzKGNvbGxlY3Rpb25OYW1lOiBzdHJpbmcsIHNjaGVtYXRpY05hbWU6IHN0cmluZywgb3B0aW9uczogYW55KToge30ge1xuICAgIGlmICh0aGlzLl9kZUFsaWFzZWROYW1lKSB7XG4gICAgICBzY2hlbWF0aWNOYW1lID0gdGhpcy5fZGVBbGlhc2VkTmFtZTtcbiAgICB9XG5cbiAgICBjb25zdCBwcm9qZWN0TmFtZSA9IG9wdGlvbnMucHJvamVjdDtcbiAgICBjb25zdCBkZWZhdWx0cyA9IGdldFNjaGVtYXRpY0RlZmF1bHRzKGNvbGxlY3Rpb25OYW1lLCBzY2hlbWF0aWNOYW1lLCBwcm9qZWN0TmFtZSk7XG5cbiAgICAvLyBHZXQgbGlzdCBvZiBhbGwgdW5kZWZpbmVkIG9wdGlvbnMuXG4gICAgY29uc3QgdW5kZWZpbmVkT3B0aW9ucyA9IHRoaXMub3B0aW9uc1xuICAgICAgLmZpbHRlcihvID0+IG9wdGlvbnNbby5uYW1lXSA9PT0gdW5kZWZpbmVkKVxuICAgICAgLm1hcChvID0+IG8ubmFtZSk7XG5cbiAgICAvLyBEZWxldGUgYW55IGRlZmF1bHQgdGhhdCBpcyBub3QgdW5kZWZpbmVkLlxuICAgIHRoaXMuX2NsZWFuRGVmYXVsdHMoZGVmYXVsdHMsIHVuZGVmaW5lZE9wdGlvbnMpO1xuXG4gICAgcmV0dXJuIGRlZmF1bHRzO1xuICB9XG59XG4iXX0=