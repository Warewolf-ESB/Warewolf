import { Deserializable } from "./../models/deserializable";

export class ExecutionLogging implements Deserializable {

  public Server: string
  public Port: string
  public Protocol: string
  public SortBy: string
  public ComputerName: string

  constructor(values: Object = {}) {
    Object.assign(this, values);
  }

  deserialize(input: any) {
    Object.assign(this, input);
    return this;
  }
}
