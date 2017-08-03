﻿using System;
using System.Linq.Expressions;
using Xunit;

namespace QueryBuilder.Tests
{
    public class BuilderTests
    {
        public class DataClass
        {
            public string Text { get; set; }
        }

        [DataSourceName("DataSourceTable")]
        public class DataSource : IDataSource
        {
            public string Text { get; set; }
            public int Value { get; set; }
        }

        private static Expression<Func<DataSource, object>> FuncProvider(string funcName)
        {
            switch (funcName)
            {
                case "value=5": return (s) => s.Value == 5;
                case "value>5": return (s) => s.Value > 5;
                case "value>=5": return (s) => s.Value >= 5;
                case "value<5": return (s) => s.Value < 5;
                case "value<=5": return (s) => s.Value <= 5;
                case "value!=5": return (s) => s.Value != 5;
            }
            throw new ArgumentException("unknown func!");
        }

        [Fact]
        public void NewSelectShouldReturnSelect()
        {
            var select = new Select<DataClass>();

            Assert.Equal("SELECT ", select.ToString());
        }

        [Fact]
        public void SelectWithPropertyShouldReturnCorrectSelectPart()
        {
            var select = new Select<DataClass>(c => c.Text);

            var from = new Select<DataClass>(c => c.Text).Where(c => c.Text != "hallo");

            Assert.Equal("SELECT Text", select.ToString());
        }

        [Fact]
        public void FromShouldAppendSource()
        {
            var select = new Select<DataClass>(c => c.Text).From("mytable");

            Assert.IsAssignableFrom(typeof(From<DataClass>), select);
            Assert.Equal("SELECT Text FROM mytable", select.ToString());
        }

        [Fact]
        public void QuerySelectWithIDataSourceShouldAutomaticallyAppendCorrecTable()
        {
            var from = Query.Select<DataSource>(s => s.Text);

            Assert.Equal("SELECT Text FROM DataSourceTable", from.ToString());
        }

        [Fact]
        public void FromWithIDataSourceShouldUseNameOfAttribute()
        {
            var from = new Select<DataSource>(s => s.Text).From(typeof(DataSource));

            Assert.Equal("SELECT Text FROM DataSourceTable", from.ToString());
        }

        [Fact]
        public void FromWithNormalTypeShouldUseNameOfType()
        {
            var from = new Select<DataClass>(s => s.Text).From(typeof(DataClass));

            Assert.Equal("SELECT Text FROM DataClass", from.ToString());
        }

        [Theory]
        [InlineData("value=5" , "WHERE Value = 5")]
        [InlineData("value>5" , "WHERE Value > 5")]
        [InlineData("value>=5", "WHERE Value >= 5")]
        [InlineData("value<5" , "WHERE Value < 5")]
        [InlineData("value<=5", "WHERE Value <= 5")]
        [InlineData("value!=5", "WHERE Value != 5")]
        public void SimpleWhereClauseShouldWriteCorrectWhereString(string func, string expected)
        {
            var whereFunc = FuncProvider(func);
            var where = new Select<DataSource>(s => s.Text).Where(whereFunc);
            var firstPart = "SELECT Text FROM DataSourceTable ";
            var complete = firstPart + expected;

            Assert.Equal(complete, where.ToString());
        }

        [Fact]
        public void SimpleWhereClauseComparingStringShouldPutStringInQuotes()
        {
            var where = new Select<DataSource>(s => s.Text).Where(s => s.Text == "abc");

            Assert.Equal("SELECT Text FROM DataSourceTable WHERE Text = 'abc'", where.ToString());
        }

        [Fact]
        public void TwoWhereConditionsConnectedViaANDShouldBePutInParenthesis()
        {
            var where = new Select<DataSource>(s => s.Text)
                .Where(s => s.Text != "abc")
                .And(s => s.Text != "dce");

            Assert.Equal("SELECT Text FROM DataSourceTable WHERE (Text != 'abc') AND Text != 'dce'", where.ToString());
        }

        [Fact]
        public void ThreeWhereConditionsLinkedWithANDShouldHaveCorrectParenthesis()
        {
            var where = new Select<DataSource>(s => s.Text)
                .Where(s => s.Text != "abc")
                .And(s => s.Value < 5)
                .And(s => s.Value <= 10);

            var expected = "SELECT Text FROM DataSourceTable WHERE (Text != 'abc') AND (Value < 5) AND Value <= 10";
            Assert.Equal(expected, where.ToString());
        }

        [Fact]
        public void TwoWhereConditionsConnectedViaORShouldBePutInParenthesis()
        {
            var where = new Select<DataSource>(s => s.Text)
                .Where(s => s.Value > 10)
                .Or(s => s.Text == "abc");

            var expected = "SELECT Text FROM DataSourceTable WHERE (Value > 10) OR Text = 'abc'";
            Assert.Equal(expected, where.ToString());
        }

        [Fact]
        public void ThreeWhereConditionsLinkedWithORShouldHaveCorrectParenthesis()
        {
            var where = new Select<DataSource>(s => s.Text)
                .Where(s => s.Text != "abc")
                .Or(s => s.Value < 5)
                .Or(s => s.Value <= 10);

            var expected = "SELECT Text FROM DataSourceTable WHERE (Text != 'abc') OR (Value < 5) OR Value <= 10";
            Assert.Equal(expected, where.ToString());
        }
    }
}
