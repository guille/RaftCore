using System;
using Xunit;
using RaftCore.StateMachine.Implementations;

namespace RaftCoreTest.StateMachineTest {
    public class NumeralStateMachineTest {
        [Fact]
        public void TestStateMachineRequestStatus() {
            var testSM = new NumeralStateMachine();
            // Default value is 0
            Assert.Equal("0", testSM.RequestStatus(""));

            // RequestStatus ignores the parameter for this SM
            Assert.Equal("0", testSM.RequestStatus("random"));
        }


        [Fact]
        public void TestStateMachineIgnoresInvalidRequest() {
            var testSM = new NumeralStateMachine();
            Assert.Equal("0", testSM.RequestStatus(null));
            testSM.Apply("bad request");
            testSM.Apply("+-1");
            testSM.Apply("-1.9");
            Assert.Equal("0", testSM.RequestStatus(""));
        }
        

        [Fact]
        public void TestStateMachineAppliesValidRequest() {
        	var testSM = new NumeralStateMachine();

            Assert.Equal("0", testSM.RequestStatus(""));

            // + integer
            testSM.Apply("+10");
            Assert.Equal("10", testSM.RequestStatus(""));
            // integer, + ommitted
            testSM.Apply("10");
            Assert.Equal("20", testSM.RequestStatus(""));
            // - integer
            testSM.Apply("-15");
            Assert.Equal("5", testSM.RequestStatus(""));
            // 0
            testSM.Apply("0");
            Assert.Equal("5", testSM.RequestStatus(""));

        }
    }
}


