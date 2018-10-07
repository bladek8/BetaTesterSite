using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BetaTesterSite.Models.Shared
{
    public class DataTablesAjaxPostModel
    {
        public int draw { get; set; }
        public int? start { get; set; }
        public int? length { get; set; }
        public List<Column> columns { get; set; }
        public Search search { get; set; }
        public List<Order> order { get; set; }

        public DataTablesAjaxPostModel()
        {
            this.columns = new List<Column>();
            this.search = new Search();
            this.order = new List<Order>();
        }
    }
    
    public class Column
    {
        public string data { get; set; }
        public string name { get; set; }
        public bool searchable { get; set; }
        public bool orderable { get; set; }
        public Search search { get; set; }
    }

    public class Search
    {
        public string value { get; set; }
        public string regex { get; set; }
    }

    public class Order
    {
        public int column { get; set; }
        public string dir { get; set; }
    }
}
