using System;
using System.Linq.Expressions;
using Xunit;
using ArLehm.QueryBuilder;
using System.Collections.Generic;
using System.Linq;

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
            public char Character { get; set; }

            public string GetPrefix() => "";
        }

        [DataSourceName("Prefixed")]
        public class PrefixedSource : IDataSource
        {
            public string Text { get; set; }
            public static string SourcePrefix => "dbo.";
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
                case "textnotnull": return (s) => s.Text != null;
                case "textisnull": return (s) => s.Text == null;
                case "text": return (s) => s.Text;
                case "value": return (s) => s.Value;
                case "char": return (s) => s.Character;
            }
            throw new ArgumentException("unknown func!");
        }

        private IEnumerable<string> GetKeys()
        {
            yield return "a";
        }

        private IEnumerable<int> GetValue()
        {
            yield return 1;
        }

        [Fact]
        public void ExpressionsOnTheRightSideShouldBeResolvedToString()
        {
            var key = $"{GetKeys().FirstOrDefault()}.key";

            var where = new Where<DataSource>(s => s.Text == key);

            Assert.Equal("Text = 'a.key'", where.ToString());
        }

        [Fact]
        public void ExpressionsOnTheRightSideShouldBeResolvedToNumber()
        {
            var key = GetValue().FirstOrDefault();

            var where = new Where<DataSource>(s => s.Value == key);

            Assert.Equal("Value = 1", where.ToString());
        }

        [Fact]
        public void NewSelectShouldReturnSelect()
        {
            var select = new Select<DataClass>();

            Assert.Equal("SELECT ", select.ToString());
        }

        [Theory]
        [InlineData("text", "SELECT Text")]
        [InlineData("value", "SELECT Value")]
        public void SelectWithPropertyShouldReturnCorrectSelectPart(string funcName, string expected)
        {
            var select = new Select<DataSource>(FuncProvider(funcName));

            Assert.Equal(expected, select.ToString());
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
        [InlineData("value=5", "WHERE Value = 5")]
        [InlineData("value>5", "WHERE Value > 5")]
        [InlineData("value>=5", "WHERE Value >= 5")]
        [InlineData("value<5", "WHERE Value < 5")]
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

        [Theory]
        [InlineData("textnotnull", "Text IS NOT NULL")]
        [InlineData("textisnull", "Text IS NULL")]
        public void WhereConditionWithNullShouldPrintNULL(string funcName, string expectedEnd)
        {
            var condition = FuncProvider(funcName);
            var where = new Select<DataSource>(c => c.Text)
                .Where(condition);

            Assert.True(where.ToString().EndsWith(expectedEnd), $"expected: {expectedEnd}, actual: {where.ToString()}");
        }

        [Fact]
        public void WhereInConditionShouldSetCorrectParenthesis()
        {
            var where = new Select<DataSource>(c => c.Text)
                .Where(c => c.Value)
                .In(new[] { 1, 2, 3, 4, 5 });

            Assert.Equal("SELECT Text FROM DataSourceTable WHERE Value IN (1,2,3,4,5)", where.ToString());
        }

        [Fact]
        public void WhereInConditionWithStringsShouldQuoteStrings()
        {
            var where = new Select<DataSource>(c => c.Text)
                .Where(c => c.Text)
                .In(new[] { "a", "b", "c" });

            Assert.Equal("SELECT Text FROM DataSourceTable WHERE Text IN ('a','b','c')", where.ToString());
        }

        [Fact]
        public void WhereNotInConditionWithIntsShouldSetCorrectParenthesis()
        {
            var where = new Select<DataSource>(c => c.Text)
                .Where(c => c.Value)
                .NotIn(new[] { 1, 2, 3, 4, 5 });

            Assert.Equal("SELECT Text FROM DataSourceTable WHERE Value NOT IN (1,2,3,4,5)", where.ToString());
        }

        [Fact]
        public void WhereNotInConditionWithStringsShouldQuoteStrings()
        {
            var where = new Select<DataSource>(c => c.Text)
                .Where(c => c.Text)
                .NotIn(new[] { "a", "b", "c" });

            Assert.Equal("SELECT Text FROM DataSourceTable WHERE Text NOT IN ('a','b','c')", where.ToString());
        }

        [Fact]
        public void WhereWithLikeShouldAddLIKE()
        {
            var where = new Select<DataSource>(c => c.Text)
                .Where(c => c.Text)
                .Like("abc");

            Assert.Equal("SELECT Text FROM DataSourceTable WHERE Text LIKE 'abc'", where.ToString());
        }

        [Fact]
        public void UsingLikeWithNonStringMemberShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => new Select<DataSource>(c => c.Text)
                                                       .Where(c => c.Value)
                                                       .Like("abc"));
        }

        [Theory]
        [InlineData("text")]
        [InlineData("char")]
        public void UsingValidTypesForLikeShouldNotThrow(string funcName)
        {
            var func = FuncProvider(funcName);

            var ex = Record.Exception(() => new Select<DataSource>(c => c.Text)
                .Where(func)
                .Like("abc"));

            Assert.Null(ex);
        }

        [Fact]
        public void ConstructingWhereOnItsOwnShouldConstructClauseWithoutWHERE()
        {
            var where = new Where<DataClass>(c => c.Text);

            Assert.Equal("Text", where.ToString());
        }

        [Theory]
        [InlineData("value=5", "Value = 5")]
        [InlineData("value>5", "Value > 5")]
        [InlineData("value>=5", "Value >= 5")]
        [InlineData("value<5", "Value < 5")]
        [InlineData("value<=5", "Value <= 5")]
        [InlineData("value!=5", "Value != 5")]
        public void ConstructWhereWithoutSelectAndFromShouldConstructCompleteWhereClause(string funcName, string expected)
        {
            var where = new Where<DataSource>(FuncProvider(funcName));

            Assert.Equal(expected, where.ToString());
        }

        [Fact]
        public void ConstructWhereWithoutFromAndFromWithChainedConditionsShouldConstructCorrectWhereClause()
        {
            var where = new Where<DataSource>(c => c.Text)
                .In(new[] { "a", "b", "c" })
                .And(c => c.Value < 5);

            Assert.Equal("(Text IN ('a','b','c')) AND Value < 5", where.ToString());
        }

        [Fact]
        public void WhereInWithMoreThan900ElementsUsesChunking()
        {
            var where = new Where<DataSource>(c => c.Text)
                .In(Enumerable.Repeat("a", 901));

            Assert.Equal(2, where.ToString().Count(c => c == '('));
            Assert.Equal(2, where.ToString().Count(c => c == ')'));
            Assert.Contains(") OR Text IN (", where.ToString());
            Assert.EndsWith("OR Text IN ('a')", where.ToString());
            Assert.Equal(901, where.ToString().Count(c => c == 'a'));
        }

        [Fact]
        public void FromWithPrefixedSourceShouldPrependPrefixBeforeTableName()
        {
            var from = new Select<PrefixedSource>(p => p.Text).From(typeof(PrefixedSource));

            Assert.Equal("SELECT Text FROM dbo.Prefixed", from.ToString());
        }

        [Fact]
        public void FromWithoutPrefixedSourceShouldNotPrependPrefixBeforeTableName()
        {
            var from = new Select<DataClass>(c => c.Text).From(typeof(DataClass));

            Assert.Equal("SELECT Text FROM DataClass", from.ToString());
        }

        [Fact]
        public void FromWithExplicitPrefixShouldPrependPrefixBeforeTableName()
        {
            var from = new Select<DataSource>(c => c.Text).From(prefix: "prefix.");

            Assert.Equal("SELECT Text FROM prefix.DataSourceTable", from.ToString());
        }

        [Fact]
        public void AddingOrderByShouldAppendORDERBYToTheWhereClause()
        {
            var where = new Where<DataSource>(x => x.Text == "abc").OrderBy(x => x.Value);

            Assert.Equal("Text = 'abc' ORDER BY Value", where.ToString());
        }
    }
}
