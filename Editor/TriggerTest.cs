using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;

namespace TBSGameCore.Tests
{
    public class TriggerTest
    {
        [Test]
        public void parseActionTest_normal()
        {
            string className;
            string methodName;
            string[] args;
            TriggerParser.parseReflectedAction("class.method(0,1,2)", out className, out methodName, out args);
            Assert.AreEqual("class", className);
            Assert.AreEqual("method", methodName);
            Assert.AreEqual(3, args.Length);
            Assert.AreEqual("0", args[0]);
            Assert.AreEqual("1", args[1]);
            Assert.AreEqual("2", args[2]);
        }
        [Test]
        public void parseActionTest_empty()
        {
            string className;
            string methodName;
            string[] args;
            TriggerParser.parseReflectedAction("(,,)", out className, out methodName, out args);
            Assert.AreEqual(null, className);
            Assert.AreEqual(null, methodName);
            Assert.AreEqual(3, args.Length);
            Assert.AreEqual(null, args[0]);
            Assert.AreEqual(null, args[1]);
            Assert.AreEqual(null, args[2]);
        }
        [Test]
        public void parseFuncTest_normal()
        {
            string className;
            string methodName;
            string[] args;
            TriggerParser.parseFunc("class.method(0,1,2)", out className, out methodName, out args);
            Assert.AreEqual("class", className);
            Assert.AreEqual("method", methodName);
            Assert.AreEqual(3, args.Length);
            Assert.AreEqual("0", args[0]);
            Assert.AreEqual("1", args[1]);
            Assert.AreEqual("2", args[2]);
        }
        [Test]
        public void parseFuncTest_empty()
        {
            string className;
            string methodName;
            string[] args;
            TriggerParser.parseFunc("(,,)", out className, out methodName, out args);
            Assert.AreEqual(null, className);
            Assert.AreEqual(null, methodName);
            Assert.AreEqual(3, args.Length);
            Assert.AreEqual(null, args[0]);
            Assert.AreEqual(null, args[1]);
            Assert.AreEqual(null, args[2]);
        }
        [Test]
        public void parseAction()
        {
            string formatName;
            TriggerParser.parseAction("format()", out formatName);
            Assert.AreEqual("format", formatName);
        }
    }
}