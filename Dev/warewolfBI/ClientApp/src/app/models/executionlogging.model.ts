export class ExecutionLogging {

  public Server: string
  public Port: string
  public Protocol: string
  public SortBy: string
  public ComputerName: string

  constructor(values: Object = {}) {
    Object.assign(this, values);
  }

 
}
