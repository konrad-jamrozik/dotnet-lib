namespace Wikitools.Lib.Tables
{
    public record TabularData2(object[] HeaderRow, object[][] Rows)
    {
        public TabularData2((object[] headerRow, object[][] rows) Data) : this(Data.headerRow, Data.rows) { }
    }
}