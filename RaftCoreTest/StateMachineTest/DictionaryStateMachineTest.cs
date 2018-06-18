using System;
using Xunit;
using RaftCore.StateMachine.Implementations;

namespace RaftCoreTest.StateMachineTest {
    public class DictionaryStateMachineTest {
        [Fact]
        public void TestStateMachineRequestStatus() {
            var testSM = new DictionaryStateMachine();
            // Default value is "", for both empty string and missing key
            Assert.Equal("", testSM.RequestStatus(""));
            Assert.Equal("", testSM.RequestStatus("random"));
        }


        [Fact]
        public void TestStateMachineIgnoresInvalidRequest() {
            var testSM = new DictionaryStateMachine();
            Assert.Equal("", testSM.RequestStatus(""));
            testSM.Apply("bad request");
            testSM.Apply("CLEAR X X");
            testSM.Apply("SET X X");
            Assert.Equal("", testSM.RequestStatus("X"));
        }
        

        [Fact]
        public void TestStateMachineAppliesValidRequest() {
            var testSM = new DictionaryStateMachine();

            Assert.Equal("", testSM.RequestStatus("X"));

            testSM.Apply("SET X 0");
            Assert.Equal("0", testSM.RequestStatus("X"));
            Assert.Equal("", testSM.RequestStatus("x"));
            testSM.Apply("SET X 20");
            Assert.Equal("20", testSM.RequestStatus("X"));
            testSM.Apply("CLEAR x");
            Assert.Equal("20", testSM.RequestStatus("X"));
            testSM.Apply("clear X");
            Assert.Equal("", testSM.RequestStatus("X"));
            testSM.Apply("set x 5");
            Assert.Equal("5", testSM.RequestStatus("x"));
            Assert.Equal("", testSM.RequestStatus("X"));

        }
    }
}


