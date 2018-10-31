import { ModuleWithProviders, Provider } from '@angular/core';
export * from './src/jwt.interceptor';
export * from './src/jwthelper.service';
export * from './src/jwtoptions.token';
export interface JwtModuleOptions {
    jwtOptionsProvider?: Provider;
    config?: {
        tokenGetter?: () => string | null | Promise<string | null>;
        headerName?: string;
        authScheme?: string;
        whitelistedDomains?: Array<string | RegExp>;
        blacklistedRoutes?: Array<string | RegExp>;
        throwNoTokenError?: boolean;
        skipWhenExpired?: boolean;
    };
}
export declare class JwtModule {
    constructor(parentModule: JwtModule);
    static forRoot(options: JwtModuleOptions): ModuleWithProviders;
}
