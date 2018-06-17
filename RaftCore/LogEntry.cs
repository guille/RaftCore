using System;
using System.Collections.Generic;
using System.Text;

namespace RaftCore {
  public class LogEntry {
    // term when entry was received by leader
    public int TermNumber { get; }
    public int Index { get; }
    public string Command { get; }

    public LogEntry(int termNumber, int index, string command) {
      this.TermNumber = termNumber;
      this.Index = index;
      this.Command = command;
    }
  }
}
