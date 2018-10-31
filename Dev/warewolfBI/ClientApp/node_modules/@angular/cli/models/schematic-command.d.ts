/**
 * @license
 * Copyright Google Inc. All Rights Reserved.
 *
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://angular.io/license
 */
import { logging } from '@angular-devkit/core';
import { Collection, Engine, Schematic, workflow } from '@angular-devkit/schematics';
import { FileSystemCollectionDesc, FileSystemEngineHostBase, FileSystemSchematicDesc } from '@angular-devkit/schematics/tools';
import { ArgumentStrategy, Command, CommandContext, Option } from './command';
export interface CoreSchematicOptions {
    dryRun: boolean;
    force: boolean;
}
export interface RunSchematicOptions {
    collectionName: string;
    schematicName: string;
    schematicOptions: any;
    debug?: boolean;
    dryRun: boolean;
    force: boolean;
    showNothingDone?: boolean;
}
export interface GetOptionsOptions {
    collectionName: string;
    schematicName: string;
}
export interface GetOptionsResult {
    options: Option[];
    arguments: Option[];
}
export declare class UnknownCollectionError extends Error {
    constructor(collectionName: string);
}
export declare abstract class SchematicCommand extends Command {
    readonly options: Option[];
    readonly allowPrivateSchematics: boolean;
    private _host;
    private _workspace;
    private _deAliasedName;
    private _originalOptions;
    private _engineHost;
    private _engine;
    private _workflow;
    argStrategy: ArgumentStrategy;
    constructor(context: CommandContext, logger: logging.Logger, engineHost?: FileSystemEngineHostBase);
    protected readonly coreOptions: Option[];
    initialize(_options: any): Promise<void>;
    protected getEngineHost(): FileSystemEngineHostBase;
    protected getEngine(): Engine<FileSystemCollectionDesc, FileSystemSchematicDesc>;
    protected getCollection(collectionName: string): Collection<any, any>;
    protected getSchematic(collection: Collection<any, any>, schematicName: string, allowPrivate?: boolean): Schematic<any, any>;
    protected setPathOptions(options: any, workingDir: string): any;
    protected getWorkflow(options: RunSchematicOptions): workflow.BaseWorkflow;
    private _getWorkflow;
    protected runSchematic(options: RunSchematicOptions): Promise<number | void> | undefined;
    protected removeCoreOptions(options: any): any;
    protected getOptions(options: GetOptionsOptions): Promise<Option[]>;
    private _loadWorkspace;
    private _cleanDefaults;
    private readDefaults;
}
