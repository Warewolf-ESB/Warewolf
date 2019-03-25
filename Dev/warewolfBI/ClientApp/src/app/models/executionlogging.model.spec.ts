import { ExecutionLogging } from './executionlogging.model';

describe('Model: ExecutionLogging', () => {
  it('should create an instance of ExecutionLogging', () => {
    expect(new ExecutionLogging()).toBeTruthy();
  });

  it('should accept values', () => {
    let log = new ExecutionLogging();
    log = {
      Server: "Server",
      Port: "Port",
      Protocol: "Protocol",
      SortBy: "SortBy",
      ComputerName: "ComputerName"
    }
    expect(log.Server).toEqual("Server");
    expect(log.Port).toEqual("Port");
    expect(log.Protocol).toEqual("Protocol");
    expect(log.SortBy).toEqual("SortBy");
    expect(log.ComputerName).toEqual('ComputerName');
  });
})

