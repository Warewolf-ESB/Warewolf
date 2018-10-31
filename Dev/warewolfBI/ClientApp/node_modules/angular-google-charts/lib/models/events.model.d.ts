export interface ChartEvent {
    column: number;
    row: number;
}
export interface ChartErrorEvent {
    id: string;
    message: string;
    detailedMessage: string;
    options: object;
}
