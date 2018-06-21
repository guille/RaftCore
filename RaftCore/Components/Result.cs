namespace RaftCore.Components {
	/// <summary>
    /// Represents a node's response to RPCs.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Result<T> {
        /// <summary>
        /// Value of the response.
        /// </summary>
        public T Value { get; }
        /// <summary>
        /// Term of the response.
        /// </summary>
        public int Term { get; }

        /// <summary>
        /// Initializes a result object.
        /// </summary>
        /// <param name="value">Parameter with the response's value</param>
        /// <param name="term">Parameter with the response's term</param>
        public Result(T value, int term) {
        	this.Value = value;
        	this.Term = term;

        }
	}
}