using System;

namespace RaftCore {
	// TODO: Internal/protected?
    public class Result<T> {
        public T Value { get; }
        public int Term { get; }

        public Result(T value, int term) {
        	this.Value = value;
        	this.Term = term;

        }
	}
}