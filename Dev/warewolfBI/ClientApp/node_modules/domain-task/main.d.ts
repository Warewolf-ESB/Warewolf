export declare function addTask(task: PromiseLike<any>): void;
export declare function run<T>(codeToRun: () => T, completionCallback: (error: any) => void): T;
export declare function baseUrl(url?: string): string;
