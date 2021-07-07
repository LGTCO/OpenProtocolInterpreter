using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;
using OpenProtocolInterpreter.TighteningResults;


namespace MIDTesters.Core.TignteningResult
{
    
    [TestClass]
    public class TestMid0900: MidTester
    {
        [TestMethod]
        public void Mid0900Revisions()
        {
            string package = "018109000010        00000005522021-07-07:19:24:550000101050001022130030100000000100010000000035002012021000035.......................................................................018109000010        00000005522021-07-07:19:24:550000201001001022130030100000000100010000000035002012021000035....2.........$...5.9.=.A.C.E.F.I.J.I.K.K.L.K.M.Q.m.......=.>.......\"..";

            var mid = _midInterpreter.Parse(package);

            Assert.AreEqual(typeof(Mid0900), mid.GetType());
            Assert.AreEqual(package, mid.Pack());
        }

        [TestMethod]
        public void Mid0900ParseByte()
        {

            string package = "018109000010        00000005522021-07-07:19:24:550000101050001022130030100000000100010000000035002012021000035.......................................................................018109000010        00000005522021-07-07:19:24:550000201001001022130030100000000100010000000035002012021000035....2.........$...5.9.=.A.C.E.F.I.J.I.K.K.L.K.M.Q.m.......=.>.......\"..";

            var data = Encoding.ASCII.GetBytes(package);

            var mid0900 = new Mid0900();
            mid0900.Parse(data);

            Assert.IsNotNull(mid0900.TraceSamples);
            Assert.AreEqual(mid0900.TraceSamples, 35);
        }

    }
}
