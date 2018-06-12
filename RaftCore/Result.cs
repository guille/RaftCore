using System;
using System.Collections.Generic;

namespace EasyRaftTest.Connections {
	// TODO: Internal/protected?
    class Result<T> {
        T result {get;}
        int Term {get;}
	}
}