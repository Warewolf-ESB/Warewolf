export declare class JwtHelperService {
    tokenGetter: () => string;
    constructor(config?: any);
    urlBase64Decode(str: string): string;
    private b64decode(str);
    private b64DecodeUnicode(str);
    decodeToken(token?: string): any;
    getTokenExpirationDate(token?: string): Date | null;
    isTokenExpired(token?: string, offsetSeconds?: number): boolean;
}
