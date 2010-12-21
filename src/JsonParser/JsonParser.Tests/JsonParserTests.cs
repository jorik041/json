using System;
using System.Diagnostics;
using NUnit.Framework;

namespace JsonParser.Tests
{
    [TestFixture]
    public class JsonParserTests
    {
        private static char[] _base16 = new[]
                                             {
                                                 '0', '1', '2', '3', 
                                                 '4', '5', '6', '7', 
                                                 '8', '9', 'A', 'B', 
                                                 'C', 'D', 'E', 'F'
                                             };

        private static char[] _base10 = new[]
                                             {
                                                 '0', '1', '2', '3', 
                                                 '4', '5', '6', '7', 
                                                 '8', '9', '0'
                                             };

        public class Dog
        {
            public string Name { get; set; }    
        }

        [Test]
        public void Can_parse_simple_example()
        {
            const string json = @"{ ""name"" : ""spot"" }";
            var dog = JsonParser.Deserialize<Dog>(json);
            Assert.IsTrue(dog.Name.Equals("spot"));
        }

        [Test]
        public void Can_parse_unicode_literals_and_symbols()
        {
            const string json = "{ \"literal\": \"\\u03a0\", \"symbol\": \"\x3a0\" }";

            var bag = JsonParser.FromJson(json);
            Assert.IsNotNull(bag);
            Assert.AreEqual(bag["literal"], bag["symbol"]);
            Trace.WriteLine(bag["literal"]);
        }

        [Test]
        public void Can_parse_control_characters_as_whitespace()
        {
            const string json = "[\t\r\b\f\n{\"color\": \"red\",\"value\": \"#f00\"}]";

            var bag = JsonParser.FromJson(json);
            Assert.IsNotNull(bag);
        }

        [Test]
        public void Can_parse_arrays()
        {
            const string json = @"[{""color"": ""red"",""value"": ""#f00""}]";

            var bag = JsonParser.FromJson(json);
            Assert.IsNotNull(bag);
        }

        [Test]
        public void Can_parse_keywords()
        {
            const string json = @"{ ""yay"" : true, ""nay"": false, ""nada"": null }";
            var bag = JsonParser.FromJson(json);
            Assert.IsNotNull(bag);
            Assert.AreEqual(true, bag["yay"]);
            Assert.AreEqual(false, bag["nay"]);
            Assert.AreEqual(null, bag["nada"]);
        }

        [Test]
        public void Can_parse_numbers()
        {
            const string json = @"{""quantity"":8902,""cost"":45.33,""value"":-1.063E-02}";

            var bag = JsonParser.FromJson(json);
            Assert.IsNotNull(bag);
            Assert.AreEqual(8902, bag["quantity"]);
            Assert.AreEqual(45.33, bag["cost"]);
            Assert.AreEqual(-1.063E-02, bag["value"]);
        }

        [Test]
        public void Can_serialize_simple_example()
        {
            const string expected = @"{""name"":""spot""}";
            var dog = new Dog { Name = "spot" };

            var actual = JsonParser.Serialize(dog);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_serialize_with_numbers()
        {
            const string expected = @"{""quantity"":8902,""cost"":45.33,""value"":-0.01063}";
            var instance = new {
                                  quantity = 8902,
                                  cost = 45.33,
                                  value = -1.063E-02
                               };
            var actual = JsonParser.Serialize(instance);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_serialize_with_anonymous_types()
        {
            const string expected = @"{""array"":{""quantity"":8902,""cost"":45.33,""value"":-0.01063}}";
            var instance = new
            {
                array = new { quantity = 8902, cost = 45.33, value = -1.063E-02 }
            };
            var actual = JsonParser.Serialize(instance);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void Can_serialize_with_dates()
        {
            var instance = new
            {
                now = DateTime.UtcNow
            };
            var actual = JsonParser.Serialize(instance);
            Trace.WriteLine(actual);
        }

        public class Time
        {
            public DateTime Now { get; set; }
        }

        [Test]
        public void Can_parse_dates()
        {
            var instance = new Time { Now = DateTime.UtcNow };

            var time = instance.Now;
            var lossy = new DateTime(time.Year,
                                     time.Month,
                                     time.Day,
                                     time.Hour,
                                     time.Minute,
                                     time.Second);

            var expected = JsonParser.Serialize(instance);
            var actual = JsonParser.Deserialize<Time>(expected);

            Assert.AreEqual(lossy, actual.Now);
        }

        [Test]
        public void Can_convert_to_base_16()
        {
            const int input = 67987356;

            var converted = JsonParser.BaseConvert(input, _base16, 4);
            Assert.AreEqual("40D679C", converted);
        }

        [Test]
        public void Can_convert_to_base_16_with_padding()
        {
            const int input = 24;

            var converted = JsonParser.BaseConvert(input, _base16, 4);
            Assert.AreEqual(converted.Length, 4);
            Assert.AreEqual("0018", converted);
        }
    }
}
