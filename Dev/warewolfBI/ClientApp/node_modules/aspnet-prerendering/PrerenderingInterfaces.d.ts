export interface RenderToStringFunc {
    (callback: RenderToStringCallback, applicationBasePath: string, bootModule: BootModuleInfo, absoluteRequestUrl: string, requestPathAndQuery: string, customDataParameter: any, overrideTimeoutMilliseconds: number, requestPathBase: string): void;
}
export interface RenderToStringCallback {
    (error: any, result?: RenderResult): void;
}
export interface RenderToStringResult {
    html: string;
    statusCode?: number;
    globals?: {
        [key: string]: any;
    };
}
export interface RedirectResult {
    redirectUrl: string;
}
export declare type RenderResult = RenderToStringResult | RedirectResult;
export interface BootFunc {
    (params: BootFuncParams): Promise<RenderResult>;
}
export interface BootFuncParams {
    location: any;
    origin: string;
    url: string;
    baseUrl: string;
    absoluteUrl: string;
    domainTasks: Promise<any>;
    data: any;
}
export interface BootModuleInfo {
    moduleName: string;
    exportName?: string;
    webpackConfig?: string;
}
