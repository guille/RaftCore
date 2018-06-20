using System;

namespace RaftCore.Components {
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