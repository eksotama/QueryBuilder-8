using System;
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

        [Fact]
        public void SimpleWhereClauseShouldWriteCorrectWhereString()
        {
            var where = new Select<DataSource>(s => s.Text).Where(s => s.Text == "abc");

            Assert.Equal("SELECT Text FROM DataSourceTable WHERE Text = 'abc'", where.ToString());
        }
    }
}
